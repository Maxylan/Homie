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
import slick.dbio.{DBIO, DBIOAction}
import java.sql.Timestamp
import java.util.concurrent.TimeUnit
import models._
import scala.compiletime.ops.int
import concurrent.duration.DurationInt

object Routes {
	// import scala.concurrent.ExecutionContext.Implicits.global
	import slick.jdbc.MySQLProfile.api._
	import slick.lifted.TableQuery

	implicit val system: ActorSystem = com.homie.reverseproxy.ReverseProxy.system
	implicit val materializer: ActorMaterializer = com.homie.reverseproxy.ReverseProxy.materializer
	implicit val executionContext: ExecutionContext = com.homie.reverseproxy.ReverseProxy.executionContext

	lazy val proxyVersion = Properties.envOrNone("PROXY_V"/*, "1"*/)
	lazy val homieVersion = Properties.envOrNone("HOMIE"/*, "1.1111"*/)
	lazy val apiHost = Properties.envOrNone("API_HOST"/*, "homie.api"*/)
	lazy val apiPort = Properties.envOrNone("API_PORT"/*, "10001"*/)
	lazy val homieHost = Properties.envOrNone("HOMIE_HOST"/*, "homie.httpd"*/)
	lazy val homiePort = Properties.envOrNone("HOMIE_PORT"/*, "10000"*/)

	/**
	  * Include user details in the access log. 
	  * (Get user details from the DB and include in the access log.)
	  *
	  * @param accessLog
	  * @param userToken
	  * @return
	  */
	def includeUserDetails(accessLog: AccessLog, userToken: String): Future[AccessLog] = {

		val userQuery = DbContext.users.filter(_.token === userToken).take(1).result.headOption
		val user = DbContext.query(userQuery).map { user =>
			accessLog.copy(
				userToken = Some(user.get.token),
				username = Some(user.get.username)
			)
		}

		user.recover({ case ex =>
			println(s"Err: Database query against `users` failed. Token: \"$userToken\" \n${ex.getMessage} ${ex.getClass.toString()}")
		})

		return user;
	}

	/**
	  * Create a new access log entry.
	  *
	  * @param request
	  * @param requestingIp
	  * @return
	  */
	def newAccessLog(request: HttpRequest, requestingIp: Option[RemoteAddress] = None): Future[AccessLog] = {

		val timestamp = new Timestamp(System.currentTimeMillis());
		val ip: String = if !requestingIp.isEmpty then requestingIp.get.value else request.headers.find(_.is("x-forwarded-for")).map(_.value().split(",").head).getOrElse(request.uri.authority.host.address /* Final fallback. */);

		val platformId: Option[String] = request.headers.find(_ is "x-requesting-platform").map(_.value());
		val userToken: Option[String] = request.headers.find(_ is "x-requesting-user").map(_.value());
		val log = AccessLog(
			None, // id
			platformId.asInstanceOf[Option[Int]], // platform_id
			None, // userToken, // token
			None, // username, // username
			timestamp,
			homieVersion,
			ip, // ip of caller
			request.method.value, // method
			request.uri.toString, // uri
			request.uri.path.toString, // path
			request.uri.rawQueryString.getOrElse(""), // parameters
			s"${request.uri.scheme}://${request.uri.authority.toString}${request.uri.path.toString}", // full_url
			Some(request.headers.mkString("\n")),
			Await.result(request.entity.toStrict(Duration(30, "s")).map(x => Option(x.data.utf8String)), Duration(30, "s")), // body
			None, // responseMessage,
			503 // responseStatus,
		)

		return if userToken.isEmpty 
			then Future.successful(log)
			else includeUserDetails(log, userToken.get)
	}

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
		println(s"(Debug) Request originalUri: \"$originalUri\", targetUri: \"$targetUri\"")

		var newAccessLogResult = newAccessLog(request, requestingIp)

		newAccessLogResult.recoverWith({ ex =>
			val recoverTimestamp = new Timestamp(System.currentTimeMillis());
			val ip: String = if !requestingIp.isEmpty then requestingIp.get.value else request.headers.find(_.is("x-forwarded-for")).map(_.value().split(",").head).getOrElse(request.uri.authority.host.address /* Final fallback. */);
			val fullUrl = s"${request.uri.scheme}://${request.uri.authority.toString}${request.uri.path.toString}"

			println(s"(${recoverTimestamp}) Err: Failed to create new Access Log instance. IP: \"${ip}\" Request: (${request.method.value}) ${targetUri} (${originalUri}) \n${ex.getMessage} ${ex.getClass.toString()}")
			Future.failed(ex) // throw new RuntimeException(s"Unknown Failure: ${ex.getClass} ${ex.getMessage}")
		})

		val proxiedRequestAndAccessLogResult: Future[(HttpResponse, AccessLog)] = newAccessLogResult.flatMap { accessLog =>
			
			// Perform the HTTP request
			val targetRequest = HttpRequest(method = request.method, uri = targetUri, entity = request.entity)
			val proxyIncommingRequestResult: Future[HttpResponse] = Http().singleRequest(targetRequest)

			proxyIncommingRequestResult.recoverWith({ ex =>
				println(s"(${accessLog.timestamp.toString()}) Err: Proxied Request against '${targetUri.toString()}' failed. IP: \"${accessLog.ip}\" Request: (${accessLog.method}) ${targetUri} (${originalUri}) \n${ex.getMessage} ${ex.getClass.toString()}")
				Future.failed(ex) // throw new RuntimeException(s"Proxied request failed: ${ex.getClass} ${ex.getMessage}")
			})

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
				)
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

						// Proxy request to "backoffice" (homie.api / homie.fastapi)
						onComplete(requestHomie(extractedRequest, request.uri, Some(ip))) {
							case Success(httpResponse, status) => {
								val statusVal = status.intValue;
								println(s"(backoffice) Response result: $status")

								val locationHeader = httpResponse.headers.find(_.is("location")).map(_.value())
								if locationHeader.isEmpty then {
									println(s"(Debug) (backoffice) Completed Response.")
									complete(httpResponse)
								}
								else {
									// * Add "api" prefix from the location header
									val locationUri = locationHeader.map(Uri(_))
									if locationUri.isEmpty then {
										println("(Debug) (backoffice) Skipped modifying \"Location\" header. Completed Response.")
										complete(httpResponse)
									}

									println("(Debug) (backoffice) Modifying \"Location\" header.")

									// If path is complete (i.e starts with a "/") OR has a fully-qualified domain, then add "api" prefix
									val locationPath = {
										if (locationUri.get.path.startsWithSlash || locationUri.get.authority.nonEmpty) {
											Path("/api" + locationUri.get.path)
										} else {
											locationUri.get.path
										}
									}

									val newHttpResponse = httpResponse.withHeaders(
										httpResponse.headers.filterNot(_.is("location")) :+ RawHeader("location", s"${locationUri.get.withPath(locationPath).toString}")
									)

									println(s"(Debug) (backoffice) Modifying \"Location\" header completed: $locationUri")
									complete(newHttpResponse)
								}
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

							// Proxy request to "homie" (homie.httpd)
							onComplete(requestHomie(extractedRequest, request.uri, Some(ip))) {
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
