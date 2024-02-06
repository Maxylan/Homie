package com.homie.reverseproxy.includes

import akka.actor.ActorSystem
import akka.stream.ActorMaterializer
import scala.util.Properties
import akka.http.scaladsl.Http
import akka.http.scaladsl.model.{HttpRequest, HttpResponse, Uri, StatusCodes, HttpEntity, ContentTypes, HttpHeader}
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

	def requestHomie(targetUri: Uri, request: HttpRequest): Future[HttpResponse] = {
		import slick.lifted.TableQuery
		import slick.jdbc.MySQLProfile.api._

		val timestamp = new Timestamp(System.currentTimeMillis());
		val platformId: Option[String] = request.headers.find(_ is "x-requesting-platform").map(_.value());
		val uid: Option[String] = request.headers.find(_ is "x-requesting-uid").map(_.value());
		val ip: String = request.headers.find(_.is("x-forwarded-for")).map(_.value().split(",").head).getOrElse(request.uri.authority.host.address);

		val query = DbContext.query(TableQuery[AccessLogTable] += (
			None, // id
			if platformId.isInstanceOf[String] then platformId.toString().toIntOption else None, // platform_id // request.cookies.find(_ is "x-requesting-platform").map(_.value()).getOrElse(None)
			if uid.isInstanceOf[String] then uid.toString().toIntOption else None, // uid
			timestamp,
			ip, // ip of caller
			request.method.toString, // method
			request.uri.toString, // uri
			request.uri.path.toString, // path
			request.uri.rawQueryString.getOrElse(""), // parameters
			s"${request.uri.scheme}://${request.uri.authority.toString}${request.uri.path.toString}", // full_url
			request.headers.mkString("\n"),
			Await.result(request.entity.toStrict(Duration(30, "s")).map(x => Option(x.data.utf8String)), Duration("30")), // body
			None, // response
			None // responseStatus
		))

		query.andThen { // Left off here!!!
			case Failure(ex) => Future.failed(new RuntimeException(s"Database query failed: ${ex.getMessage}")) // Handle the failure
			case Success(inserts) => println(s"(${timestamp.toString()}) (§$inserts) $ip ${request.method} ${request.uri}")
		}

		val targetRequest = HttpRequest(method = request.method, uri = targetUri, entity = request.entity)
		val responseFuture: Future[HttpResponse] = Http().singleRequest(targetRequest)
		return responseFuture
	}

	val route = {
		require(apiHost != null, s"Failed to load \"API_HOST\" from environment.")
		require(apiPort != null, s"Failed to load \"API_PORT\" from environment.")
		require(homieHost != null, s"Failed to load \"HOMIE_HOST\" from environment.")
		require(homiePort != null, s"Failed to load \"HOMIE_PORT\" from environment.")

		extractRequest { request =>
			pathPrefix("api") {
				path(".*".r) { path =>
					extractUri { uri =>
						// Build targetUri to the API (homie.api)
						val targetUri = uri.withAuthority(apiHost.get, apiPort.get.toInt).withScheme("http")
						onComplete(requestHomie(targetUri, request)) {
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
						onComplete(requestHomie(targetUri, request)) {
							case Success(response) => {
								complete(response)
							}
							case Failure(ex) => complete(s"Request failed: ${ex.getMessage}")
						}
					}
				}
			}
		}
	};
}
