package com.homie.reverseproxy.includes

import akka.actor.ActorSystem
import akka.stream.ActorMaterializer
import scala.util.{Properties, Failure, Success}
import akka.http.scaladsl.Http
import akka.http.scaladsl.model.{HttpRequest, HttpResponse, Uri, StatusCode, StatusCodes, HttpEntity, ContentTypes, HttpHeader, RemoteAddress}
import akka.http.scaladsl.model.Uri.{Path, Query}
import akka.http.scaladsl.model.headers.RawHeader
import akka.http.scaladsl.server.Directives._
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
	def requestHomie(request: HttpRequest, originalUri: Uri, requestingIp: Option[RemoteAddress] = None): Future[(HttpResponse, StatusCode)] = {

		val targetUri = request.uri
		val timestamp = new Timestamp(System.currentTimeMillis());
		println(s"(${timestamp.toString()}) (Info/Debug) Intercepted Request - originalUri: \"$originalUri\", targetUri: \"$targetUri\"")

		// Perform the HTTP request
		val targetRequest = HttpRequest(method = request.method, uri = targetUri, entity = request.entity)
		val proxyIncommingRequestResult: Future[HttpResponse] = Http().singleRequest(targetRequest)

		proxyIncommingRequestResult.recoverWith({ ex =>
			println(s"(${timestamp.toString()}) Err: Proxied Request against '${targetUri.toString()}' failed. IP: \"${requestingIp}\" Request: (${request.method.value}) ${targetUri} (${originalUri}) \n${ex.getMessage} ${ex.getClass.toString()}")
			Future.failed(ex) // throw new RuntimeException(s"Proxied request failed: ${ex.getClass} ${ex.getMessage}")
		})

		val proxiedRequestAndAccessLogResult: Future[(HttpResponse, AccessLog)] = newAccessLogResult.flatMap { accessLog =>
			

			// Insert the access_log entry into the database
			// The fact DBMI can't return the inserted row as an object instance is actually rediculous...
			val DBMIReturning = DbContext.access_logs returning DbContext.access_logs.map(_.id) // <-- WHY IS THIS LINE NEEDED IN ORDER FOR `returning` TO NOT THROW A COMPILER ERROR??
			val insertAccessLog = (DBMIReturning += accessLog)
			val insertAccessLogResult: Future[AccessLog] = DbContext.query(insertAccessLog).map[AccessLog] { insertedAccessLogID => accessLog.copy(id = insertedAccessLogID) } 
			
			insertAccessLogResult.recoverWith({ ex =>
				println(s"(${accessLog.timestamp.toString()}) Err: Database query (insert) against the 'access_logs' table failed. IP: \"${accessLog.ip}\" Request: (${accessLog.method}) ${targetUri} (${originalUri}) \n${ex.getMessage} ${ex.getClass.toString()}")
				Future.failed(ex) // throw new RuntimeException(s"Database insert failed: ${ex.getClass} ${ex.getMessage}")
			})
			
			// Return the two futures.
			for {
				response <- proxyIncommingRequestResult
				insertedAccessLog <- insertAccessLogResult
			} yield (response, insertedAccessLog)
		} 
		
		proxiedRequestAndAccessLogResult.recoverWith { ex =>
			// val currentAccessLog: AccessLog = Await.result(newAccessLogResult, Duration(30, "s"));
			// println(s"(${currentAccessLog.timestamp.toString()}) Err: HttpRequest or Database query failed. IP: \"${currentAccessLog.ip}\" Request: (${request.method.value}) ${targetUri} (${originalUri}) \n${ex.getMessage} ${ex.getClass.toString()}")
			Future.failed(ex) // throw new RuntimeException(s"Unknown Failure: ${ex.getClass} ${ex.getMessage}")
		}
		
		// Update the access_log entry with `id`, with the response body and status returned from the request
		val proxiedRequestAndUpdateAccessLogResult: Future[(HttpResponse, StatusCode)] = proxiedRequestAndAccessLogResult.flatMap { case (response, insertedAccessLog) =>

			val responseCopy = response.copy()
			val responseBody = Await.result(responseCopy.entity.toStrict(30.seconds), 30.seconds).data.utf8String

			val accessLogResponse = responseCopy.headers.mkString("\n")
			val accessLogResponseStatus = responseCopy.status.intValue
			
			responseCopy.discardEntityBytes()

			val updateAction = DbContext.access_logs.filter(_.id === insertedAccessLog.id).update(
				insertedAccessLog.copy(
					responseMessage = Some(responseBody.substring(0, math.min(responseBody.length, 1023))), 
					responseStatus = accessLogResponseStatus
				) // <-- left here
			)

			// Execute the update action
			val updateAccessLogResult = DbContext.query(updateAction)
		
			updateAccessLogResult.recoverWith { case ex => 
				println(s"(${insertedAccessLog.timestamp.toString()}) Err: (Inner Recover) Database query (update) on existing `access_log` (${insertedAccessLog.id}) against the 'access_logs' table failed. IP: \"${insertedAccessLog.ip}\" Request: (${insertedAccessLog.method}) ${targetUri} (${originalUri}) \n${ex.getMessage} ${ex.getClass.toString()}")
				Future.failed(ex) // throw new RuntimeException(s"(Inner RuntimeException) Database update failed: ${ex.getClass} ${ex.getMessage}")
			}

			updateAccessLogResult.map(update => (response, response.status)) 
		} 
		
		proxiedRequestAndUpdateAccessLogResult.recoverWith { case ex => 
			val currentAccessLog: AccessLog = Await.result(proxiedRequestAndAccessLogResult.map(_._2), Duration(30, "s"));
			println(s"(${currentAccessLog.timestamp.toString()}) Err: (Outer Recover) Database query (update) on existing `access_log` (${currentAccessLog.id}) against the 'access_logs' table failed. IP: \"${currentAccessLog.ip}\" Request: (${currentAccessLog.method}) ${targetUri} (${originalUri}) \n${ex.getMessage} ${ex.getClass.toString()}")
			Future.failed(ex) // throw new RuntimeException(s"(Outer RuntimeException) Database update failed: ${ex.getClass} ${ex.getMessage}")
		}

		try return proxiedRequestAndUpdateAccessLogResult.map { case (response, responseStatus) => (response, responseStatus) } finally Await.result(DbContext.db.shutdown, Duration(30, "s"))
	}

	/**
	  * Filter the "Location" header to exclude the "api" prefix.
	  *
	  * @param response
	  * @param prefix
	  * @return
	  */
	def filterLocationHeader(response: HttpResponse, prefix: String|Path): HttpResponse = {


								val locationHeader = httpResponse.headers.find(_.is("location")).map(_.value())
								if locationHeader.isEmpty then {
									println(s"(Debug) (backoffice) Completed Response.")
									/* return */ (
										httpResponse, 
										httpResponse.copy()
									)
								}
								else {
									// * Add "api" prefix from the location header
									val locationUri = locationHeader.map(Uri(_))
									if locationUri.isEmpty then {
										println("(Debug) (backoffice) Skipped modifying \"Location\" header. Completed Response.")
										/* return */ (
											httpResponse, 
											httpResponse.copy()
										)
									}
									else {
										// If path is complete (i.e starts with a "/") OR has a fully-qualified domain, then add "api" prefix
										val locationPath = {
											if (locationUri.get.path.startsWithSlash || locationUri.get.authority.nonEmpty) {
												println("(Debug) (backoffice) Modifying \"Location\" header.")
												Path("/api" + locationUri.get.path)
											} else {
												locationUri.get.path
											}
										}

										val newHttpResponse = httpResponse.withHeaders(
											httpResponse.headers.filterNot(_.is("location")) :+ RawHeader("location", s"${locationUri.get.withPath(locationPath).toString}")
										)

										println(s"(Debug) (backoffice) Completed Response.")
										/* return */ (
											newHttpResponse, 
											newHttpResponse.copy()
										)
									}
								}

		try return proxiedRequestAndUpdateAccessLogResult.map { case (response, responseStatus) => (response, responseStatus) } finally Await.result(DbContext.db.shutdown, Duration(30, "s"))
	}

	val route = {
		require(!apiHost.isEmpty, s"Failed to load \"API_HOST\" from environment.")
		require(!apiPort.isEmpty, s"Failed to load \"API_PORT\" from environment.")
		require(!homieHost.isEmpty, s"Failed to load \"HOMIE_HOST\" from environment.")
		require(!homiePort.isEmpty, s"Failed to load \"HOMIE_PORT\" from environment.")

		extractClientIP {
			ip => {
				println(s"(Debug) Request from: $ip")
				extractRequest { request =>
					println(s"(Debug) Requesting: ${request.uri}")
					pathPrefix("api") {
						println("(Debug) Requesting api (homie.api)")
						// Build targetUri to the API (homie.api)

						// Remove the prefix from the path
						val pathString = ("""^/?api/?""".r).replaceFirstIn(request.uri.path.toString(), "").stripPrefix("/")
						val extractedPath: Path = Path(s"/$pathString")
						val extractedRequest: HttpRequest = request.copy(
							uri = request.uri.withScheme("http").withAuthority(apiHost.get, apiPort.get.toInt).withPath(extractedPath)
						)

						// Send the request to the homie.httpd
						val sendProxyRequest: Future[HttpResponse] = requestHomie(extractedRequest, request.uri, Some(ip))

						// On complete, filter the "Location" header to exclude the "api" prefix.
						// (prefix is used for routing only)
						val proxyResponse: HttpResponse = sendProxyRequest.map(filterLocationHeader(_, "api"))
						val proxyResponseCopy: HttpResponse = proxyResponse.copy()

						// Route onComplete: Proxy request to "backoffice" (homie.api / homie.fastapi)
						onComplete(sendRequestProxy) {
							case Success(httpResponse, status) => {
								println(s"(backoffice) Response result: $status")
								complete(httpResponse)
							}
							case Failure(ex) => {
								complete(s"(backoffice) Request failed: ${ex.getMessage}")
							}
						}
					} ~ {
						println("(Debug) Requesting default (homie.httpd)")
						// Default route for other requests
						// Build targetUri to the App/Frontend (homie.httpd)
						
						if (request.uri.path.endsWith("favicon.ico", true)) {
							complete(StatusCodes.NotFound)
						}
						else {
							val extractedRequest: HttpRequest = request.copy(
								uri = request.uri.withScheme("http").withAuthority(homieHost.get, homiePort.get.toInt)
							)

							val requestCopy = extractedRequest.copy()
							
							// Send the request to the homie.httpd
							val sendRequestProxy = requestHomie(extractedRequest, request.uri, Some(ip))

							// Route onComplete: Proxy request to "homie" (homie.httpd)
							onComplete(sendRequestProxy) {
								case Success(httpResponse, status) => {
									println(s"(homie) Response result: $status")
									complete(httpResponse)
								}
								case Failure(ex) => {
									complete(s"(homie) Request failed: ${ex.getMessage}")
								}
							}
						}
					}
				}
			}
		}
	};
}
