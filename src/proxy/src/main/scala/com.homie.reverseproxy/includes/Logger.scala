// (c) 2024 @Maxylan
package com.homie.reverseproxy.includes

import akka.stream.ActorMaterializer
import scala.concurrent.{ExecutionContext, Await, Promise, Future}
import scala.concurrent.duration.{Duration, DurationInt}
import scala.util.{Success, Failure}
import akka.http.scaladsl.model.{HttpRequest, HttpResponse, RemoteAddress}
import slick.dbio.{DBIO, DBIOAction, NoStream, Effect}
import slick.lifted.Query
import java.sql.Timestamp
import com.homie.reverseproxy.ReverseProxy
import models._

object Logger 
{
	implicit val executionContext: ExecutionContext = com.homie.reverseproxy.ReverseProxy.executionContext
	implicit val materializerContext: ActorMaterializer = com.homie.reverseproxy.ReverseProxy.materializer

	/**
	  * Create a new access log entry.
	  *
	  * @param request
	  * @param requestingIp
	  * @return
	  */
	def newAccessLog(request: HttpRequest, response: HttpResponse, requestingIp: RemoteAddress): Future[AccessLog] = {

		val timestamp = new Timestamp(System.currentTimeMillis());

		val platformId: Option[Int] = for {
			header <- request.headers.find(_ is "x-requesting-platform")
			id <- header.value().toIntOption
		} yield id

		val userToken: Option[String] = for {
			header <- request.headers.find(_ is "x-requesting-user")
			token <- Option(header.value())
		} yield token

		val log = AccessLog(
			None, // id
			platformId, // platform_id - Requesting platform..
			userToken, // userToken/token - Requesting user..
			None, // username
			timestamp,
			ReverseProxy.homieVersion,
			requestingIp.value, // ip of caller
			request.method.value, // method
			request.uri.toString, // uri
			request.uri.path.toString, // path
			request.uri.rawQueryString.getOrElse(""), // parameters
			s"${request.uri.scheme}://${request.uri.authority.toString}${request.uri.path.toString}", // full_url
			Some(request.headers.mkString("\n")),
			Await.result(request.entity.toStrict(30.seconds).map(x => Option(x.data.utf8String)), 30.seconds), // body
			None, // responseMessage,
			503 // responseStatus,
		)

		return if userToken.isEmpty 
			then Future.successful(log)
			else UsersHandler.includeUserDetailsInLog(log, userToken.get)
	}

	def logAccess(request: HttpRequest, response: HttpResponse, ip: RemoteAddress, route: String): Future[Int] = {

		// Maybe in the future I delegate creating the log to `Routes.scala` and have it happen whilst awaiting the response.
		// Today is not that day!
		val accessLog: Future[AccessLog] = newAccessLog(request, response, ip) // Create the access log.
		val insertResult: Future[Int] = accessLog.flatMap(AccessLogsHandler.insert(_)) // Insert the access log.

		insertResult.onComplete {
			case Success(rows) =>
				println(s"($route) Access log for IP: \"${ip.value}\" created")
			case Failure(ex) =>
				println(s"($route) Failed to create access log: ${ex.getMessage}")
		}

		return insertResult;
	}
}