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

object Routes {

	implicit val system: ActorSystem = com.homie.reverseproxy.ReverseProxy.system
	implicit val materializer: ActorMaterializer = com.homie.reverseproxy.ReverseProxy.materializer
	implicit val executionContext: ExecutionContext = com.homie.reverseproxy.ReverseProxy.executionContext

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

		proxyIncommingRequestResult.flatMap { response =>
			response.entity.dataBytes.runFold(ByteString(""))(_ ++ _).map { _ =>
				(response)
			}
		}

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
	  * Handle all requests to the API (homie.api).
	  *
	  * @param ip
	  * @param request
	  * @return
	  */
	def apiRouteHandler(ip: RemoteAddress, request: HttpRequest): Future[HttpResponse] = {

		// Create a new access log entry.
		var accessLog = Logger.newAccessLog(ip, request)

		// Build targetUri to the API (homie.api) - Remove the prefix from the path
		val extractedRequest: HttpRequest = request.copy(
			uri = filterRequestUri(request.uri).withAuthority(
				ReverseProxy.env.required("API_HOST"), 
				ReverseProxy.env.required("API_PORT").toInt
			)
		)

		var logFutureSequence = Seq(
			UsersHandler.includeUserDetailsInLog(accessLog, accessLog.userToken), // Add user details to the access log.
			Logger.addRequestDetailsTo(accessLog, request), // Add request details to the access log.
		)

		// Send request.
		val responseFuture = requestHomie(extractedRequest).map(filterLocationHeader(_, "api"))

		// Start this future chain after `responseFuture` has completed. 
		// Not awaited, `responseFuture` is what will be returned to `completed(...)`.
		val logFuture = responseFuture map { response =>
			logFutureSequence :+ Logger.addResponseDetailsTo(accessLog, response) // Add response details to the access log.
		}

		logFuture map { seq =>
			// Merges/Reduces `Seq[Future[AccessLog]]` into one AccessLog with all the latest details. 
			// Then inserts the merged `AccessLog` instance once all futures are complete.

			Future.sequence(seq) map {
				// Merge AccessLog instances 
				_.foldLeft(accessLog) { (acc, log) =>
					acc.copy(
						platformId = log.platformId.orElse(acc.platformId),
						userToken = log.userToken.orElse(acc.userToken),
						username = log.username.orElse(acc.username),
						body = log.body.orElse(acc.body),
						responseMessage = log.responseMessage.orElse(acc.responseMessage),
						responseStatus = log.responseStatus.orElse(acc.responseStatus)
					)
				}
			} map {
				AccessLogsHandler.insert(_)
			}
		}

		// Route onComplete: Proxy request to "backoffice" (homie.api / homie.fastapi)
		responseFuture map { response =>
			println(s"(Info) (homie.api) IP: \"${ip}\" Request complete!"); 
			response
		}
	}

	/**
	  * Handle all requests to the app/frontend (homie.httpd). Default route for other requests
	  *
	  * @param ip
	  * @param request
	  * @return
	  */
	def fallbackRouteHandler(ip: RemoteAddress, request: HttpRequest): Future[HttpResponse] = {

		// Create a new access log entry.
		val accessLog = Logger.newAccessLog(ip, request)

		// Build targetUri to the App/Frontend (homie.httpd)
		val extractedRequest: HttpRequest = request.copy(
			uri = filterRequestUri(request.uri).withAuthority(
				ReverseProxy.env.required("HOMIE_HOST"), 
				ReverseProxy.env.required("HOMIE_PORT").toInt
			)
		)

		var logFutureSequence = Seq(
			UsersHandler.includeUserDetailsInLog(accessLog, accessLog.userToken), // Add user details to the access log.
			Logger.addRequestDetailsTo(accessLog, request), // Add request details to the access log.
		)

		// Send request.
		val responseFuture = requestHomie(extractedRequest)

		// Start this future chain after `responseFuture` has completed. 
		// Not awaited, `responseFuture` is what will be returned to `completed(...)`.
		val logFuture = responseFuture map { response =>
			logFutureSequence :+ Logger.addResponseDetailsTo(accessLog, response) // Add response details to the access log.
		}

		logFuture map { seq =>
			// Merges/Reduces `Seq[Future[AccessLog]]` into one AccessLog with all the latest details. 
			// Then inserts the merged `AccessLog` instance once all futures are complete.

			Future.sequence(seq) map {
				// Merge AccessLog instances 
				_.foldLeft(accessLog) { (acc, log) =>
					acc.copy(
						platformId = log.platformId.orElse(acc.platformId),
						userToken = log.userToken.orElse(acc.userToken),
						username = log.username.orElse(acc.username),
						body = log.body.orElse(acc.body),
						responseMessage = log.responseMessage.orElse(acc.responseMessage),
						responseStatus = log.responseStatus.orElse(acc.responseStatus)
					)
				}
			} map {
				AccessLogsHandler.insert(_)
			} 
		}

		// Route onComplete: Proxy request to "frontend" (homie.httpd)
		responseFuture map { response =>
			println(s"(Info) (homie.httpd) IP: \"${ip}\" Request complete!"); 
			response
		}
	}

	/**
	  * Combine the routes defined above to handle all incomming requests.
	  *
	  * @param ip
	  * @param request
	  * @return
	  */
	def routingLogic(ip: RemoteAddress, request: HttpRequest): Route =
		concat(
			pathPrefix("api") {
				complete { // "API" Path
					// Lots of chained futures..
					// Finally returns Future[HttpResponse]
					apiRouteHandler(ip, request)
				}
			},
			pathPrefix("") {
				complete { // "Fallback"
					// Lots of chained futures..
					// Finally returns Future[HttpResponse]
					fallbackRouteHandler(ip, request)
				}
			}
		)

	/**
	  * Define routes handling all incomming requests.
	  */
	val route: Route = {
		require(!ReverseProxy.env.required("API_HOST").isEmpty, s"Failed to load \"API_HOST\" from environment.")
		require(!ReverseProxy.env.required("API_PORT").isEmpty, s"Failed to load \"API_PORT\" from environment.")
		require(!ReverseProxy.env.required("HOMIE_HOST").isEmpty, s"Failed to load \"HOMIE_HOST\" from environment.")
		require(!ReverseProxy.env.required("HOMIE_PORT").isEmpty, s"Failed to load \"HOMIE_PORT\" from environment.")
		
		extractClientIP { ip => 
			extractRequest { request => 
				println(s"(Info) IP: \"${ip}\" Requesting: ${request.uri}")
				routingLogic(ip, request)
			}
		}
	};
}
