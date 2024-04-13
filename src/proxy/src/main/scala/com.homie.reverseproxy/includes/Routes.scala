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
import akka.util.ByteString

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
		println(s"(Info) 'originalUri': \"$originalUri\", 'targetUri': \"$targetUri\"")

		// Perform the HTTP request
		val targetRequest = HttpRequest(method = request.method, uri = targetUri, entity = request.entity)
		val proxyIncommingRequestResult: Future[HttpResponse] = Http().singleRequest(targetRequest)

		proxyIncommingRequestResult.recoverWith({ ex =>
			println(s"(Error) Proxied Request against '${targetUri.toString()}' failed. Request: (${request.method.value}) ${targetUri} (${originalUri}) \n${ex.getMessage} ${ex.getClass.toString()}")
			Future.failed(ex) // throw new RuntimeException(s"Proxied request failed: ${ex.getClass} ${ex.getMessage}")
		})

		proxyIncommingRequestResult.flatMap { response =>
			response.entity.dataBytes.runFold(ByteString(""))(_ ++ _).map { _ =>
				response
			}
		}

		proxyIncommingRequestResult;
	}

	/**
	  * Filter the "Location" header to exclude the "api" prefix.
	  *
	  * @param response
	  * @param prefix
	  * @return
	  */
	def filterRequestUri(uri: Uri, prefix: Option[String|Path], route: String = ""): Uri = {
		println(s"(Debug) ($route) *filter*RequestUri");

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
		println(s"(Info) (homie.api) IP: \"${ip}\" Requesting: ${request.uri}")

		// Create a new access log entry.
		val accessLog = Logger.newAccessLog(ip, request, None)

		println(s"(Debug) (homie.api) accessLog");

		// Build targetUri to the API (homie.api) - Remove the prefix from the path
		val extractedRequest: HttpRequest = request.copy(
			uri = filterRequestUri(request.uri, Some("api"), "homie.api").withAuthority(apiHost.get, apiPort.get.toInt)
		)

		println(s"(Debug) (homie.api) extractedRequest");

		// Add user details to the access log.
		val accessLogWithUserDetails: Future[AccessLog] = if (accessLog.userToken.isDefined) then {
			UsersHandler.includeUserDetailsInLog(accessLog, accessLog.userToken.get)
		} else {
			Future.successful(accessLog)
		}

		val responseFuture = for {
			logWithUserDetails <- { println(s"(Debug) (homie.api) logWithUserDetails"); accessLogWithUserDetails } // Add user details to the access log.
			logWithRequest <- { println(s"(Debug) (homie.api) logWithRequest"); Logger.addRequestDetailsTo(accessLog, request, "homie.api") } // Add request details to the access log.
			response <- { println(s"(Debug) (homie.api) response"); requestHomie(extractedRequest, request.uri).map(filterLocationHeader(_, "api")) } // Send request.
		} yield response

		// Start this future chain after `responseFuture` has completed. 
		// Not awaited, `responseFuture` is what will be returned to `completed(...)`.
		responseFuture flatMap { response =>
			for { // Add response details to the access log.
				logWithResponse <- { println(s"(Debug) (homie.api) logWithResponse"); Logger.addResponseDetailsTo(accessLog, response, "homie.api") }
				insertLog <- { println(s"(Debug) (homie.api) insertLog"); AccessLogsHandler.insert(accessLog, "homie.api") }
			} yield logWithResponse
		}

		// Route onComplete: Proxy request to "backoffice" (homie.api / homie.fastapi)
		println(s"(Info) (homie.api) ..returned futures, now waiting.");
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
		println(s"(Info) (homie.httpd) IP: \"${ip}\" Requesting: ${request.uri}")

		// Create a new access log entry.
		val accessLog = Logger.newAccessLog(ip, request)

		// Build targetUri to the App/Frontend (homie.httpd)
		val extractedRequest: HttpRequest = request.copy(
			uri = filterRequestUri(request.uri, None).withAuthority(homieHost.get, homiePort.get.toInt)
		)

		// Add user details to the access log.
		val accessLogWithUserDetails: Future[AccessLog] = if (accessLog.userToken.isDefined) then {
			UsersHandler.includeUserDetailsInLog(accessLog, accessLog.userToken.get)
		} else {
			Future.successful(accessLog)
		}

		val responseFuture = for {
			logWithUserDetails <- { println(s"(Debug) (homie.httpd) logWithUserDetails"); accessLogWithUserDetails } // Add user details to the access log.
			logWithRequest <- { println(s"(Debug) (homie.httpd) logWithRequest"); Logger.addRequestDetailsTo(accessLog, request, "homie.api") } // Add request details to the access log.
			response <- { println(s"(Debug) (homie.httpd) response"); requestHomie(extractedRequest, request.uri).map(filterLocationHeader(_, "api")) } // Send request.
		} yield response

		// Start this future chain after `responseFuture` has completed. 
		// Not awaited, `responseFuture` is what will be returned to `completed(...)`.
		responseFuture flatMap { response =>
			for { // Add response details to the access log.
				logWithResponse <- { println(s"(Debug) (homie.httpd) logWithResponse"); Logger.addResponseDetailsTo(accessLog, response, "homie.api") }
				insertLog <- { println(s"(Debug) (homie.httpd) insertLog"); AccessLogsHandler.insert(accessLog, "homie.api") }
			} yield logWithResponse
		}

		// Route onComplete: Proxy request to "backoffice" (homie.api / homie.fastapi)
		println(s"(Info) (homie.httpd) ..returned futures, now waiting.");
		responseFuture map { response =>
			println(s"(Info) (homie.httpd) IP: \"${ip}\" Request complete!"); 
			response
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

		// Combine the routes defined above to handle all incomming requests.
		concat(
			pathPrefix("api") { ctx =>
				extractClientIP { ip => 
					extractRequest { request => 
						println(s"(Info) (homie.api) IP: \"${ip}\" Requesting: ${request.uri}")

						// Create a new access log entry.
						var accessLog = Logger.newAccessLog(ip, request, None)

						println(s"(Debug) (homie.api) accessLog");

						// Build targetUri to the API (homie.api) - Remove the prefix from the path
						val extractedRequest: HttpRequest = request.copy(
							uri = filterRequestUri(request.uri, Some("api"), "homie.api").withAuthority(apiHost.get, apiPort.get.toInt)
						)

						println(s"(Debug) (homie.api) extractedRequest");

						// Add user details to the access log.
						val accessLogWithUserDetails: Future[AccessLog] = if (accessLog.userToken.isDefined) then {
							UsersHandler.includeUserDetailsInLog(accessLog, accessLog.userToken.get)
						} else {
							Future.successful(accessLog)
						}

						val responseFuture = for {
							logWithUserDetails <- { println(s"(Debug) (homie.api) logWithUserDetails"); accessLogWithUserDetails } // Add user details to the access log.
							logWithRequest <- { println(s"(Debug) (homie.api) logWithRequest"); Logger.addRequestDetailsTo(accessLog, request, "homie.api") } // Add request details to the access log.
							response <- { println(s"(Debug) (homie.api) response"); requestHomie(extractedRequest, request.uri).map(filterLocationHeader(_, "api")) } // Send request.
						} yield response

						// Start this future chain after `responseFuture` has completed. 
						// Not awaited, `responseFuture` is what will be returned to `completed(...)`.
						responseFuture flatMap { response =>
							for { // Add response details to the access log.
								logWithResponse <- { println(s"(Debug) (homie.api) logWithResponse"); Logger.addResponseDetailsTo(accessLog, response, "homie.api") }
								insertLog <- { println(s"(Debug) (homie.api) insertLog"); AccessLogsHandler.insert(accessLog, "homie.api") }
							} yield logWithResponse
						}

						// Route onComplete: Proxy request to "backoffice" (homie.api / homie.fastapi)
						println(s"(Info) (homie.api) ..returned futures, now waiting.");
						responseFuture map {
							println(s"(Info) (homie.api) IP: \"${ip}\" Request complete!"); 
							ctx.complete(_)
						}
					}
				}
			}, 
			pathPrefix("api") { ctx =>
				extractClientIP { ip => 
					pathPrefix("") { 
						println(s"(Info) (homie.httpd) IP: \"${ip}\" Requesting: ${request.uri}")

						// Create a new access log entry.
						var accessLog = Logger.newAccessLog(ip, request)

						// Build targetUri to the App/Frontend (homie.httpd)
						val extractedRequest: HttpRequest = request.copy(
							uri = filterRequestUri(request.uri, None).withAuthority(homieHost.get, homiePort.get.toInt)
						)

						// Add user details to the access log.
						val accessLogWithUserDetails: Future[AccessLog] = if (accessLog.userToken.isDefined) then {
							UsersHandler.includeUserDetailsInLog(accessLog, accessLog.userToken.get)
						} else {
							Future.successful(accessLog)
						}

						val responseFuture = for {
							logWithUserDetails <- { println(s"(Debug) (homie.httpd) logWithUserDetails"); accessLogWithUserDetails } // Add user details to the access log.
							logWithRequest <- { println(s"(Debug) (homie.httpd) logWithRequest"); Logger.addRequestDetailsTo(accessLog, request, "homie.api") } // Add request details to the access log.
							response <- { println(s"(Debug) (homie.httpd) response"); requestHomie(extractedRequest, request.uri).map(filterLocationHeader(_, "api")) } // Send request.
						} yield response

						// Start this future chain after `responseFuture` has completed. 
						// Not awaited, `responseFuture` is what will be returned to `completed(...)`.
						responseFuture flatMap { response =>
							for { // Add response details to the access log.
								logWithResponse <- { println(s"(Debug) (homie.httpd) logWithResponse"); Logger.addResponseDetailsTo(accessLog, response, "homie.api") }
								insertLog <- { println(s"(Debug) (homie.httpd) insertLog"); AccessLogsHandler.insert(accessLog, "homie.api") }
							} yield logWithResponse
						}

						// Route onComplete: Proxy request to "backoffice" (homie.api / homie.fastapi)
						println(s"(Info) (homie.httpd) ..returned futures, now waiting.");
						/*responseFuture.map { response =>
							println(s"(Info) (homie.httpd) IP: \"${ip}\" Request complete!"); 
							ctx.complete(response)
						}*/
						/* complete(responseFuture) */
						/*onComplete(responseFuture) {
							case Success(response) => ctx.complete(response)
							case Failure(ex) => ctx.complete(s"(Error) (homie.api) Request failed: ${ex.getMessage}")
						}*/
						onComplete(responseFuture) { 
							case Success(response: HttpResponse) => ctx.complete(response)
							case Failure(ex) => ctx.complete(s"(Error) (homie.api) Request failed: ${ex.getMessage}")
						}
					}
				}
			}
		)
		
		// Close the database connection, no longer needed (hopefully).
		// onComplete { DbContext.db.shutdown }
		/* routeResult */
	};
}
