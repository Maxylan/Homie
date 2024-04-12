package com.homie.reverseproxy.includes

import akka.actor.ActorSystem
import akka.stream.ActorMaterializer
import scala.util.{Properties, Failure, Success}
import akka.http.scaladsl.Http
import akka.http.scaladsl.model.{HttpRequest, HttpResponse, Uri, StatusCode, StatusCodes, HttpEntity, ContentTypes, HttpHeader, RemoteAddress}
import akka.http.scaladsl.model.Uri.{Path, Query}
import akka.http.scaladsl.model.headers.RawHeader
import akka.http.scaladsl.server.Directives._
import akka.http.scaladsl.server.Route
import java.util.concurrent.Executors
import scala.concurrent.{ExecutionContext, Promise, Future, Await}
import scala.concurrent.duration.FiniteDuration
import scala.concurrent.duration.Duration
import java.sql.Timestamp
import concurrent.duration.DurationInt
import models._

object Routes {

	implicit val system: ActorSystem = com.homie.reverseproxy.ReverseProxy.system
	implicit val materializer: ActorMaterializer = com.homie.reverseproxy.ReverseProxy.materializer
	implicit val executionContext: ExecutionContext = com.homie.reverseproxy.ReverseProxy.executionContext
	lazy val apiHost = Properties.envOrNone("API_HOST"/*, "homie.api"*/)
	lazy val apiPort = Properties.envOrNone("API_PORT"/*, "10001"*/)
	lazy val homieHost = Properties.envOrNone("HOMIE_HOST"/*, "homie.httpd"*/)
	lazy val homiePort = Properties.envOrNone("HOMIE_PORT"/*, "10000"*/)

	/**
	  * Perform a request to the targetUri and log the request and response as an 
	  * `AccessLog` in the database.
	  * 
	  * Damn I feel like every time I have to do a small fix or change here I have
	  * to rewrite this whole thing.
	  *
	  * @param targetUri
	  * @param request
	  * @param requestingIp
	  * @return
	  */
	def requestHomie(request: HttpRequest, originalUri: Uri): Future[HttpResponse] = {

		val targetUri = request.uri
		val timestamp = new Timestamp(System.currentTimeMillis());
		println(s"(${timestamp.toString()}) (Info) Intercepted Request - originalUri: \"$originalUri\", targetUri: \"$targetUri\"")

		// Perform the HTTP request
		val targetRequest = HttpRequest(method = request.method, uri = targetUri, entity = request.entity)
		val proxyIncommingRequestResult: Future[HttpResponse] = Http().singleRequest(targetRequest)

		proxyIncommingRequestResult.recoverWith({ ex =>
			println(s"(${timestamp.toString()}) Err: Proxied Request against '${targetUri.toString()}' failed. Request: (${request.method.value}) ${targetUri} (${originalUri}) \n${ex.getMessage} ${ex.getClass.toString()}")
			Future.failed(ex) // throw new RuntimeException(s"Proxied request failed: ${ex.getClass} ${ex.getMessage}")
		})

		return proxyIncommingRequestResult;
	}

	/**
	  * Filter the "Location" header to exclude the "api" prefix.
	  *
	  * @param response
	  * @param prefix
	  * @return
	  */
	def filterRequestUri(uri: Uri, prefix: Option[String|Path]): Uri = {

		val extractedPath: Path = if prefix.isDefined then {
			val pathString = (s"""^/?${prefix.get}/?""".r).replaceFirstIn(uri.path.toString(), "").stripPrefix("/")
			Path(s"/$pathString")
		} else {
			uri.path
		}
		 
		uri.withScheme("http").withPath(extractedPath)
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
			println(s"(Debug) (backoffice) Completed Response.")
			/* return */ response
		}
		else {
			// * Add "api" prefix from the location header
			val locationUri = locationHeader.map(Uri(_))
			if locationUri.isEmpty then {
				println("(Debug) (backoffice) Skipped modifying \"Location\" header. Completed Response.")
				/* return */ response
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

				println(s"(Debug) (backoffice) Completed Response.")
				/* return */ newHttpResponse
			}
		}
	}

	/**
	  * Define routes handling all incomming requests.
	  */
	val route: Route = { 
		require(!apiHost.isEmpty, s"Failed to load \"API_HOST\" from environment.")
		require(!apiPort.isEmpty, s"Failed to load \"API_PORT\" from environment.")
		require(!homieHost.isEmpty, s"Failed to load \"HOMIE_HOST\" from environment.")
		require(!homiePort.isEmpty, s"Failed to load \"HOMIE_PORT\" from environment.")
		
		/**
		 * Route all requests prefixed with '/api' to (homie.api).
		 */
		val apiRoute: Route = Route.seal(
			pathPrefix("api") { 
				extractClientIP { ip => 
					extractRequest { request => 
						complete {
							println(s"(Info) (homie.api) IP: \"${ip}\" Requesting: ${request.uri}")

							/* // Saving some of my debug-prints because, well, I like them.
							println(s"(Warn) (homie.api) Failed to log access: ${ex.getMessage}"); 
							println(s"(Warn) (homie.api) Failed to log access: ${ex.getMessage}"); 
							case Failure(ex) => { /* Quietly do nothing, just so that we can say its been handled. */ }
							println(s"(Info) (homie.api) Completed Response result: ${httpResponse.status.intValue}")
							println(s"(Error) (homie.api) Request failed: ${ex.getMessage}")
							*/

							// Build targetUri to the API (homie.api) - Remove the prefix from the path
							val extractedRequest: HttpRequest = request.copy(
								uri = filterRequestUri(request.uri, Some("api")).withAuthority(apiHost.get, apiPort.get.toInt)
							)

							// Route onComplete: Proxy request to "backoffice" (homie.api / homie.fastapi)
							for {
								proxyResponse <- requestHomie(extractedRequest, request.uri).map(filterLocationHeader(_, "api"))
								logging <- Logger.logAccess(ip, "homie.api", Some(request), Some(proxyResponse))
								proxyResponseCopy <- proxyResponse.entity.toStrict(3.seconds).map(e => proxyResponse.copy(entity = e.copy()))
								disposeBytes <- Future.successful(proxyResponse.discardEntityBytes())
								result <- Future.successful(proxyResponseCopy)
							} yield result
						}
					}
				}
			}
		);

		val fallbackRoute: Route = {
			extractClientIP { ip => 
				extractRequest { request => 
					complete {
						// Default route for other requests
						println(s"(Info) (homie.httpd) IP: \"${ip}\" Requesting: ${request.uri}")
						
						/* // Saving some of my debug-prints because, well, I like them.
						ex => println(s"(Warn) (homie.httpd) Failed to log access: ${ex.getMessage}"); 
						case Failure(ex) => { /* Quietly do nothing, just so that we can say its been handled. */ }
						println(s"(Info) (homie.httpd) Completed Response result: ${httpResponse.status.intValue}")
						complete(s"(Error) (homie.httpd) Request failed: ${ex.getMessage}")
						*/

						// Build targetUri to the App/Frontend (homie.httpd)
						val extractedRequest: HttpRequest = request.copy(
							uri = filterRequestUri(request.uri, None).withAuthority(homieHost.get, homiePort.get.toInt)
						)

						for {
							proxyResponse <- requestHomie(extractedRequest, request.uri)
							logging <- Logger.logAccess(ip, "homie.httpd", Some(request), Some(proxyResponse))
							proxyResponseCopy <- proxyResponse.entity.toStrict(3.seconds).map(e => proxyResponse.copy(entity = e.copy()))
							disposeBytes <- Future.successful(proxyResponse.discardEntityBytes())
							result <- Future.successful(proxyResponseCopy)
						} yield result
					}
				}
			}
		};

		// Combine the routes defined above to handle all incomming requests.
		val routeResult = concat(
			apiRoute, 
			fallbackRoute
		)
		
		// Close the database connection, no longer needed (hopefully).
		onComplete { DbContext.db.shutdown }
		routeResult
	};
}
