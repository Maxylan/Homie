package com.homie.reverseproxy.includes

import com.homie.reverseproxy.ReverseProxy
import akka.actor.ActorSystem
import akka.stream.ActorMaterializer
import scala.util.{Failure, Success}
import akka.util.ByteString
import akka.http.scaladsl.Http
import akka.http.scaladsl.model.{HttpRequest, HttpResponse, Uri, StatusCode, StatusCodes, HttpEntity, ContentTypes, HttpHeader, RemoteAddress}
import akka.http.scaladsl.model.Uri.{Path, Query}
import akka.http.scaladsl.model.headers.RawHeader
import akka.http.scaladsl.server.Directives._
import akka.http.scaladsl.server.Route
import scala.concurrent.{ExecutionContext, Promise, Future, Await}
import scala.concurrent.duration.FiniteDuration
import scala.concurrent.duration.Duration
import concurrent.duration.DurationInt
import models._
import routes._

/**
  * Routing logic for the Reverse Proxy.
  */
object Routing {

	implicit val system: ActorSystem = ReverseProxy.system
	implicit val materializer: ActorMaterializer = ReverseProxy.materializer
	implicit val executionContext: ExecutionContext = ReverseProxy.executionContext

	/**
	  * Follow through on an HttpRequest, proxying it to its destination of `targetUri`.
	  *
	  * @param request
	  * @return `Future[HttpResponse]`
	  */
	def requestHomie(request: HttpRequest): Future[HttpResponse] = {

		// Perform the HTTP request
		val targetUri = request.uri
		val targetRequest = HttpRequest(method = request.method, uri = targetUri, entity = request.entity)
		val proxyIncommingRequestResult: Future[HttpResponse] = Http().singleRequest(targetRequest)

		proxyIncommingRequestResult.recoverWith({ ex =>
			println(s"(Error) Proxied Request against '${targetUri.toString()}' failed. Request: (${request.method.value}) ${targetUri} \n${ex.getClass.toString()} \n${ex.getMessage}")
			Future.failed(ex) // throw new RuntimeException(s"Proxied request failed: ${ex.getClass} ${ex.getMessage}")
		})

		/* proxyIncommingRequestResult.flatMap { response =>
			response.entity.dataBytes.runFold(ByteString(""))(_ ++ _).map { _ =>
				(response)
			}
		} */
		
		proxyIncommingRequestResult;
	}

	/**
	  * Filter the "Location" header to exclude the "api" prefix.
	  *
	  * @param uri
	  * @param prefix
	  * @return `Uri` *.withScheme("http").withPath(filteredPath)*
	  */
	def filterRequestUri(uri: Uri, prefix: Option[String|Path] = None): Uri = {

		val filteredPath: Path = if prefix.isDefined then {
			val pathString = (s"""^/?${prefix.get}/?""".r).replaceFirstIn(uri.path.toString(), "").stripPrefix("/")
			Path(s"/$pathString")
		} else {
			uri.path
		}
		 
		uri.withScheme("http").withPath(filteredPath)
	}

	/**
	  * Filter the "Location" header to exclude the "api" prefix.
	  *
	  * @param response
	  * @param prefix
	  * @return
	  */
	def filterLocationHeader(response: HttpResponse, prefix: String|Path): HttpResponse = {

		val locationHeader = response.headers.find(_.is("location")).map(_.value())
		if locationHeader.isEmpty then {
			response
		}
		else {
			// * Add "api" prefix from the location header
			val locationUri = locationHeader.map(Uri(_))
			if locationUri.isEmpty then {
				response
			}
			else {
				// If path is complete (i.e starts with a "/") OR has a fully-qualified domain, then add "api" prefix
				val locationPath = {
					if (locationUri.get.path.startsWithSlash || locationUri.get.authority.nonEmpty) {
						println("(Debug) (backoffice) Modifying \"Location\" header.")
						Path("/" + prefix + locationUri.get.path)
					} else {
						locationUri.get.path
					}
				}

				val newHttpResponse = response.withHeaders(
					response.headers.filterNot(_.is("location")) :+ RawHeader("location", s"${locationUri.get.withPath(locationPath).toString}")
				)

				newHttpResponse
			}
		}
	}

	/**
	  * Combine the routes defined to handle all incomming requests.
	  *
	  * @param	ip
	  * @param	request
	  * @return `Route`
	  */
	def routingLogic(ip: RemoteAddress, request: HttpRequest, requestEntity: HttpEntity.Strict): Route =
		concat(
			pathPrefix("api") {
				complete { // "API" Path
					BackofficeRoute.Handle(ip, request, requestEntity)
				}
			},
			pathPrefix("") {
				complete { // "Fallback"
					FallbackRoute.Handle(ip, request, requestEntity)
				}
			}
		)

	/**
	  * Define routes handling all incomming requests.
	  */
	def init: Route = {
		require(!ReverseProxy.env.required("API_HOST").isEmpty, s"Failed to load \"API_HOST\" from environment.")
		require(!ReverseProxy.env.required("API_PORT").isEmpty, s"Failed to load \"API_PORT\" from environment.")
		require(!ReverseProxy.env.required("HOMIE_HOST").isEmpty, s"Failed to load \"HOMIE_HOST\" from environment.")
		require(!ReverseProxy.env.required("HOMIE_PORT").isEmpty, s"Failed to load \"HOMIE_PORT\" from environment.")
		
		extractClientIP { ip => 
			extractRequest { request => 
				extractStrictEntity(3.seconds) { entity => 
					println(s"(Info) IP: \"${ip}\" Requesting: ${request.uri}")
					routingLogic(ip, request, entity)
				}
			}
		}
	};
}

/**
  * Trait to be implemented by my "routing definitions" in package `com.homie.reverseproxy.includes.routes`.
  */
trait RoutingDefinition {
	val routeId: String
	def Handle(ip: RemoteAddress, request: HttpRequest, requestEntity: HttpEntity.Strict): Future[HttpResponse]
}