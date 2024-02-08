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

object Routes {
	// import scala.concurrent.ExecutionContext.Implicits.global
	import slick.jdbc.MySQLProfile.api._
	import slick.lifted.TableQuery

	implicit val system: ActorSystem = com.homie.reverseproxy.ReverseProxy.system
	implicit val materializer: ActorMaterializer = com.homie.reverseproxy.ReverseProxy.materializer
	implicit val executionContext: ExecutionContext = com.homie.reverseproxy.ReverseProxy.executionContext

	lazy val apiHost = Properties.envOrNone("API_HOST"/*, "homie.api"*/)
	lazy val apiPort = Properties.envOrNone("API_PORT"/*, "10001"*/)
	lazy val homieHost = Properties.envOrNone("HOMIE_HOST"/*, "homie.httpd"*/)
	lazy val homiePort = Properties.envOrNone("HOMIE_PORT"/*, "10000"*/)

	def newAccessLog(request: HttpRequest, requestingIp: Option[RemoteAddress] = None) = {
		val timestamp = new Timestamp(System.currentTimeMillis());
		val platformId: Option[String] = request.headers.find(_ is "x-requesting-platform").map(_.value());
		val uid: Option[String] = request.headers.find(_ is "x-requesting-uid").map(_.value());
		val ip: String = if !requestingIp.isEmpty then requestingIp.get.value else request.headers.find(_.is("x-forwarded-for")).map(_.value().split(",").head).getOrElse(request.uri.authority.host.address /* Final fallback. */);
		AccessLogs(
			None, // id
			platformId.asInstanceOf[Option[Int]], // platform_id
			uid.asInstanceOf[Option[Int]], // uid
			timestamp,
			ip, // ip of caller
			request.method.value, // method
			request.uri.toString, // uri
			request.uri.path.toString, // path
			request.uri.rawQueryString.getOrElse(""), // parameters
			s"${request.uri.scheme}://${request.uri.authority.toString}${request.uri.path.toString}", // full_url
			request.headers.mkString("\n"),
			Await.result(request.entity.toStrict(Duration(30, "s")).map(x => Option(x.data.utf8String)), Duration(30, "s")), // body
			None, // response
			None // responseStatus
		)
	}

	def requestHomie(targetUri: Uri, request: HttpRequest, requestingIp: Option[RemoteAddress] = None): Future[HttpResponse] = {

		var accessLog = newAccessLog(request, requestingIp)
		var insertAction = (DbContext.access_logs returning DbContext.access_logs.map(_.id)) += accessLog;
		val insertAccessLog = DbContext.executeAsync(insertAction)

		// insertAccessLog.map { id =>
		// 	println(s"(${accessLog.timestamp.toString()}) (+${id.get}) IP: \"${accessLog.ip}\" Request: (${accessLog.method}) ${accessLog.fullUrl}")
		// }.recover { case ex =>
		// 	println(s"(${accessLog.timestamp.toString()}) Warn: Database query against `access_logs` failed. IP: \"$ip\" Request: (${accessLog.method}) ${accessLog.fullUrl} \n${ex.getMessage} ${ex.getClass}\n${ex.getStackTrace()}")
		// 	// throw new RuntimeException(s"Database query failed: ${ex.getMessage}")
		// }

		val insertAndSendRequest: Future[(Long, HttpResponse)] = insertAccessLog.flatMap { id =>
			println(s"(${accessLog.timestamp.toString()}) (+${id.get}) IP: \"${accessLog.ip}\" Request: (${accessLog.method}) ${accessLog.fullUrl}")
			
			// Perform the HTTP request after inserting the access log
			val targetRequest = HttpRequest(method = request.method, uri = targetUri, entity = request.entity)
			val responseFuture: Future[HttpResponse] = Http().singleRequest(targetRequest)

			// Create a tuple of the returned access_log id and the response
			responseFuture.map(response => (id.get, response))
		} recover { ex =>
			println(s"(${accessLog.timestamp.toString()}) Warn: Database query against `access_logs` failed. IP: \"${accessLog.ip}\" Request: (${accessLog.method}) ${accessLog.fullUrl} \n${ex.getMessage} ${ex.getClass}\n${ex.getStackTrace()}")
			throw new RuntimeException(s"Database query failed: ${ex.getMessage}")
		}
		
		// Update the access_log entry with `id`, with the response body and status returned from the request
		val updateAccessLogAfterRequest: Future[(Long, HttpResponse, Int)] = insertAndSendRequest.flatMap { (id, response) =>
			val updateAction = DbContext.access_logs.filter(_.id === id).update({
				val accessLogResponse = Await.result(response.entity.toStrict(Duration(30, "s")).map(x => Option(x.data.utf8String)), Duration(30, "s"))
				val accessLogResponseStatus = response.status.intValue
				accessLog.copy(response = accessLogResponse, responseStatus = Some(accessLogResponseStatus))
			})

			val updateAccessLog = DbContext.executeAsync(updateAction)

			updateAccessLog.map(update => (id, response, update))
		} recover { ex => 
			println(s"(${accessLog.timestamp.toString()}) Warn: Database query against existing `access_logs` (${insertAndSendRequest.map(_._1)}) failed. IP: \"$accessLog.ip\" Request: (${accessLog.method}) ${accessLog.fullUrl} \n${ex.getMessage} ${ex.getClass}\n${ex.getStackTrace()}")
			throw new RuntimeException(s"Database query failed: ${ex.getMessage}")
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
				println(s"Request from: $ip")
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
