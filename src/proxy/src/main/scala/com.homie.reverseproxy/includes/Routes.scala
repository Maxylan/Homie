package com.homie.reverseproxy.includes

import akka.actor.ActorSystem
import akka.stream.ActorMaterializer
import scala.util.{Properties, Failure, Success}
import akka.http.scaladsl.Http
import akka.http.scaladsl.model.{HttpRequest, HttpResponse, Uri, StatusCodes, HttpEntity, ContentTypes, HttpHeader, RemoteAddress}
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

	def newAccessLog(request: HttpRequest, requestingIp: Option[RemoteAddress] = None): Future[AccessLog] = {

		val timestamp = new Timestamp(System.currentTimeMillis());
		val ip: String = if !requestingIp.isEmpty then requestingIp.get.value else request.headers.find(_.is("x-forwarded-for")).map(_.value().split(",").head).getOrElse(request.uri.authority.host.address /* Final fallback. */);

		val platformId: Option[String] = request.headers.find(_ is "x-requesting-platform").map(_.value());
		val user_token: Option[String] = request.headers.find(_ is "x-requesting-user").map(_.value());
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
			request.headers.mkString("\n").asInstanceOf[Option[String]],
			Await.result(request.entity.toStrict(Duration(30, "s")).map(x => Option(x.data.utf8String)), Duration(30, "s")), // body
			None, // responseMessage,
			503 // responseStatus,
		)

		return if user_token.isEmpty 
			then Future.successful(log)
			else includeUserDetails(log, user_token.get)
	}

	/**
	  * Include user details in the access log. 
	  * (Get user details from the DB and include in the access log.)
	  *
	  * @param AccessLog
	  * @param user_token
	  * @return
	  */
	def includeUserDetails(accessLog: AccessLog, user_token: String): Future[AccessLog] = {
		val userQuery = DbContext.users.filter(_.token === user_token).take(1).result.headOption
		val user = DbContext.query(userQuery)

		user.map { user =>
			accessLog.copy(
				userToken = Some(user.get.token),
				username = Some(user.get.username)
			)
		}
	}

	def requestHomie(targetUri: Uri, request: HttpRequest, requestingIp: Option[RemoteAddress] = None): Future[HttpResponse] = {

		var accessLogFuture = newAccessLog(request, requestingIp)
		val insertedLogAndRequest: Future[(AccessLog, HttpResponse)] = accessLogFuture.flatMap { accessLog =>

			// val insertActionComposer = (DbContext.access_logs returning DbContext.access_logs)
			// val insertAccessLog = DbContext.access_logs.returning(DbContext.access_logs) += accessLog
			val insertActionComposer = (DbContext.access_logs returning DbContext.access_logs.map(_.*))
			val insertAccessLog = DbContext.executeAsync(insertActionComposer += accessLog)
			insertAccessLog.flatMap { insertedAccessLog =>
				
				println(s"(${insertedAccessLog.timestamp.toString()}) (+${insertedAccessLog.id}) IP: \"${insertedAccessLog.ip}\" Request: (${insertedAccessLog.method}) ${insertedAccessLog.fullUrl}")
				
				// Perform the HTTP request after inserting the access log
				val targetRequest = HttpRequest(method = request.method, uri = targetUri, entity = request.entity)
				val responseFuture: Future[HttpResponse] = Http().singleRequest(targetRequest)

				// Create a tuple of the returned access_log id and the response
				responseFuture.map(response => (insertedAccessLog, response))
			} 
			recover { ex =>
				println(s"(${accessLog.timestamp.toString()}) Warn: Database query against `access_logs` failed. IP: \"${accessLog.ip}\" Request: (${accessLog.method}) ${accessLog.fullUrl} \n${ex.getMessage} ${ex.getClass}\n${ex.getStackTrace()}")
				throw new RuntimeException(s"Database insert failed: ${ex.getMessage}")
			}
		} 
		recover { ex =>
			val recoverTimestamp = new Timestamp(System.currentTimeMillis());
			val fullUrl = s"${request.uri.scheme}://${request.uri.authority.toString}${request.uri.path.toString}";

			println(s"(${recoverTimestamp.toString()}) Warn: Database query against `access_logs` failed. IP: \"${requestingIp}\" Request: (${request.method.value}) ${fullUrl} \n${ex.getMessage} ${ex.getClass}\n${ex.getStackTrace()}")
			throw new RuntimeException(s"Database insert failed: ${ex.getMessage}")
		}
		
		// Update the access_log entry with `id`, with the response body and status returned from the request
		val updateAccessLogAfterRequest: Future[(AccessLog, HttpResponse, Int)] = insertedLogAndRequest.flatMap { case (insertedAccessLog, response) =>

			val updateAction = DbContext.access_logs.filter(_.id === id).update({
				// Body, but let's not save this for now.
				// Await.result(response.entity.toStrict(Duration(30, "s")).map(x => Option(x.data.utf8String)), Duration(30, "s"))
													// LEFT HERE
				val accessLogResponse = response.headers.mkString("\n") // Await.result(request.entity.toStrict(Duration(30, "s")).map(x => Option(x.data.utf8String)), Duration(30, "s"))
				val accessLogResponseStatus = response.status.intValue

				insertedAccessLog.copy(response = Some(accessLogResponse), responseStatus = Some(accessLogResponseStatus))
			})

			val updateAccessLog = DbContext.executeAsync(updateAction)

			updateAccessLog.map(update => (insertedAccessLog, response, update))
		} recover { case ex => 
			val currentAccessLog: AccessLog = insertedLogAndRequest.map(_._1);
			println(s"(${currentAccessLog.timestamp.toString()}) Warn: Database query against existing `access_logs` (${currentAccessLog.id}) failed. IP: \"$currentAccessLog.ip\" Request: (${currentAccessLog.method}) ${currentAccessLog.fullUrl} \n${ex.getMessage} ${ex.getClass}\n${ex.getStackTrace()}")
			throw new RuntimeException(s"Database update failed: ${ex.getMessage}")
		}

		updateAccessLogAfterRequest.map(_._2)
	}

	val route = {
		require(!apiHost.isEmpty, s"Failed to load \"API_HOST\" from environment.")
		require(!apiPort.isEmpty, s"Failed to load \"API_PORT\" from environment.")
		require(!homieHost.isEmpty, s"Failed to load \"HOMIE_HOST\" from environment.")
		require(!homiePort.isEmpty, s"Failed to load \"HOMIE_PORT\" from environment.")

		extractClientIP {
			ip => {
				// println(s"Request from: $ip")
				extractRequest { request =>
					pathPrefix("api") {
						path(".*".r) { path =>
							extractUri { uri =>
								// Build targetUri to the API (homie.api)
								val targetUri = uri.withAuthority(apiHost.get, apiPort.get.toInt).withScheme("http")
								onComplete(requestHomie(targetUri, request, Some(ip))) {
									case Success(response) => {
										DbContext.db.close() 
										complete(response)
									}
									case Failure(ex) => {
										DbContext.db.close() 
										complete(s"Request failed: ${ex.getMessage}")
									}
								}
							}
						}
					} ~ {
						// Default route for other requests
						path(".*".r) { path =>
							extractUri { uri =>
								if (uri.path.endsWith("favicon.ico", true)) {
									complete(StatusCodes.NotFound)
								}
								else {
									// Build targetUri to "homie" (homie.httpd)
									val targetUri = uri.withAuthority(homieHost.get, homiePort.get.toInt).withScheme("http")
									onComplete(requestHomie(targetUri, request, Some(ip))) {
										case Success(response) => {
											DbContext.db.close() 
											complete(response)
										}
										case Failure(ex) => {
											DbContext.db.close() 
											complete(s"Request failed: ${ex.getMessage}")
										}
									}
								}
							}
						}
					}
				}
			}
		}
	};
}
