package com.homie.reverseproxy.includes

import akka.actor.ActorSystem
import akka.stream.ActorMaterializer
import scala.util.Properties
import akka.http.scaladsl.Http
import akka.http.scaladsl.model.{HttpRequest, HttpResponse, Uri, StatusCodes, HttpEntity, ContentTypes, HttpHeader, RemoteAddress}
import akka.http.scaladsl.model.headers.RawHeader
import akka.http.scaladsl.server.Directives._
import scala.concurrent.{Promise, Future, Await}
import scala.concurrent.duration.FiniteDuration
import scala.concurrent.duration.Duration
import scala.util.{Failure, Success}
import slick.dbio.{DBIO, DBIOAction}
import java.sql.Timestamp

object Routes {
	import scala.concurrent.ExecutionContext.Implicits.global
	implicit val system: ActorSystem = com.homie.reverseproxy.ReverseProxy.system
	implicit val materializer: ActorMaterializer = com.homie.reverseproxy.ReverseProxy.materializer
	lazy val apiHost = Properties.envOrNone("API_HOST"/*, "homie.api"*/)
	lazy val apiPort = Properties.envOrNone("API_PORT"/*, "10001"*/)
	lazy val homieHost = Properties.envOrNone("HOMIE_HOST"/*, "homie.httpd"*/)
	lazy val homiePort = Properties.envOrNone("HOMIE_PORT"/*, "10000"*/)

	def requestHomie(targetUri: Uri, requestingIp: RemoteAddress, request: HttpRequest): Future[HttpResponse] = {
		import slick.lifted.TableQuery
		import slick.jdbc.MySQLProfile.api._

		val timestamp = new Timestamp(System.currentTimeMillis());
		val platformId: Option[String] = request.headers.find(_ is "x-requesting-platform").map(_.value());
		val uid: Option[String] = request.headers.find(_ is "x-requesting-uid").map(_.value());
		val ip: String = request.headers.find(_.is("x-forwarded-for")).map(_.value().split(",").head).getOrElse(requestingIp.value /* request.uri.authority.host.address */);
		val accessLogEntry = AccessLogs(
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

		val insertAccessLog = DbContext.db.run(
			(DbContext.access_logs returning DbContext.access_logs.map(_.id)) += accessLogEntry
		)

		insertAccessLog.map { id =>
			println(s"(${timestamp.toString()}) (+${id.get}) IP: \"$ip\" Request: (${accessLogEntry.method}) ${accessLogEntry.fullUrl}")
		}.recover { case ex =>
			println(s"(${timestamp.toString()}) Warn: Database query against `access_logs` failed. IP: \"$ip\" Request: (${accessLogEntry.method}) ${accessLogEntry.fullUrl} \n${ex.getMessage} ${ex.getClass}\n${ex.getStackTrace()}")
			// throw new RuntimeException(s"Database query failed: ${ex.getMessage}")
		}

		val accessLogId = Await.result(insertAccessLog, Duration(30, "s"))

		val targetRequest = HttpRequest(method = request.method, uri = targetUri, entity = request.entity)
		val responseFuture: Future[HttpResponse] = Http().singleRequest(targetRequest)
		
		// Update the access_log entry with the response body and status
		responseFuture.onComplete {
			try {
				case Success(response) => {
					if (!accessLogId.isEmpty) {
						val query = DbContext.access_logs.filter(_.id === accessLogId).update({
							val accessLogResponse = Await.result(response.entity.toStrict(Duration(30, "s")).map(x => Option(x.data.utf8String)), Duration(30, "s"))
							val accessLogResponseStatus = response.status.intValue
							accessLogEntry.copy(response = accessLogResponse, responseStatus = Some(accessLogResponseStatus))
						})
					
						DbContext.db.run(query)
					}
				}
				case Failure(ex) => {
					println(s"(${timestamp.toString()}) Warn: Database query against existing `access_logs` (${accessLogId.getOrElse("None")}) failed. IP: \"$ip\" Request: (${accessLogEntry.method}) ${accessLogEntry.fullUrl} \n${ex.getMessage} ${ex.getClass}\n${ex.getStackTrace()}")
					// throw new RuntimeException(s"Database query failed: ${ex.getMessage}")
				}
				/*
				case Failure(ex) => {
					val query = DbContext.access_logs.update(_.response := Await.result(response.entity.toStrict(Duration(30, "s")).map(x => Option(x.data.utf8String)), Duration(30, "s")), _.responseStatus := response.status.intValue())
					DbContext.db.run()
				}
				*/
			}
			finally {
				DbContext.db.close()
			}
		} 

		return responseFuture
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
								onComplete(requestHomie(targetUri, ip, request)) {
									case Success(response) => complete(response)
									case Failure(ex) => complete(s"Request failed: ${ex.getMessage}")
								}
							}
						}
					} ~ {
						// Default route for other requests
						path(".*".r) { path =>
							extractUri { uri =>
								// Build targetUri to "homie" (homie.httpd)
								val targetUri = uri.withAuthority(homieHost.get, homiePort.get.toInt).withScheme("http")
								onComplete(requestHomie(targetUri, ip, request)) {
									case Success(response) => {
										complete(response)
									}
									case Failure(ex) => complete(s"Request failed: ${ex.getMessage}")
								}
							}
						}
					}
				}
			}
		}
	};
}
