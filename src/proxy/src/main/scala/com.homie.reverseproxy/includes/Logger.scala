// (c) 2024 @Maxylan
package com.homie.reverseproxy.includes

import akka.stream.ActorMaterializer
import scala.concurrent.{ExecutionContext, Await, Promise, Future}
import scala.concurrent.duration.{Duration, DurationInt}
import scala.util.{Success, Failure}
import akka.http.scaladsl.model.{HttpRequest, HttpResponse, HttpEntity, RemoteAddress}
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
	def newAccessLog(requestingIp: RemoteAddress, request: Option[HttpRequest], response: Option[HttpResponse]): Future[AccessLog] = {

		val timestamp = new Timestamp(System.currentTimeMillis());

		val platformId: Option[Int] = for {
			header <- if request.isDefined then request.get.headers.find(_ is "x-requesting-platform") else None
			id <- header.value().toIntOption
		} yield id

		val userToken: Option[String] = for {
			header <- if request.isDefined then request.get.headers.find(_ is "x-requesting-user") else None
			token <- Option(header.value())
		} yield token

		val requestMethod = if request.isDefined then request.get.method.value else "UNKNOWN"
		val requestUri = if request.isDefined then request.get.uri.toString else ""
		val requestPath = if request.isDefined then Some(request.get.uri.path.toString) else None
		val requestQueryString = if request.isDefined then request.get.uri.rawQueryString else None
		val requestHeaders = if request.isDefined then Some(request.get.headers.mkString("\n")) else None
		val requestStatus = if response.isDefined then response.get.status.intValue else 503

		var standardLog = AccessLog(
			None, // id
			platformId, // platform_id - Requesting platform..
			userToken, // userToken/token - Requesting user..
			None, // username
			timestamp,
			ReverseProxy.homieVersion,
			requestingIp.value, // ip of caller
			requestMethod, // method
			requestUri, // uri
			requestPath, // path
			requestQueryString, // parameters
			requestUri, // full_url
			requestHeaders,
			None, // body
			None, // responseMessage,
			requestStatus // responseStatus,
		)

		// val requestBodyFuture: Future[String] = request.map(_.entity.toStrict(3.seconds).map(_)).getOrElse(Future.successful(""))
		// val responseBodyFuture: Future[String] = response.map(_.entity.toStrict(3.seconds).map(_.data.utf8String)).getOrElse(Future.successful(""))

		/**
		  * If the request is defined, then we will attempt to store the request body.
		  */
		val accessLogWithRequest: Future[AccessLog] = if request.isDefined then {

			val maxConentLength = 1023*63 // 64449
			try {
				for {
					req <- Future.successful(request.get)
					reqEntity <- req.entity.toStrict(3.seconds)
					reqBody <- Future.successful(reqEntity.data.utf8String)
				} yield {
					if (reqBody.length > maxConentLength) then {
						println(s"(Warn) Request body too large: ${reqBody.length} > $maxConentLength")

						standardLog = standardLog.copy(
							body = Some(s"{\"loggerMessage\":\"Request body too large (${reqBody.length}).\"}"),
						)
					} else {
						standardLog = standardLog.copy(
							body = Some(reqBody.substring(0, math.min(reqBody.length, maxConentLength /*64449*/))),
						)
					}

					standardLog
				}
			} catch ex => {
				val message = s"{\"loggerMessage\":\"Reading request body failed: ${ex.getMessage()}\"}";
				request.get.discardEntityBytes()
				standardLog.copy(
					body = Some(message.substring(0, math.min(message.length, maxConentLength /*64449*/))),
				)

				Future.successful(standardLog)
			}
		} else {
			Future.successful(standardLog)
		}

		/**
		  * If the response is defined, then we will store response status and attempt to read the response body.
		  */
		val accessLogWithResponse: Future[AccessLog] = if response.isDefined then {

			val maxConentLength = 1023

			standardLog = standardLog.copy(
				responseStatus = response.get.status.intValue
			)

			if (response.get.entity.contentType.toString().contains("application/json")) then {
				standardLog = standardLog.copy(
					responseMessage = Some("JSON")
				)

				try {
					for {
						res <- Future.successful(response.get)
						resEntity <- res.entity.toStrict(3.seconds)
						resBody <- Future.successful(resEntity.data.utf8String)
					} yield {
						standardLog = standardLog.copy(
							responseMessage = Some(resBody.substring(0, math.min(resBody.length, maxConentLength)))
						)

						standardLog
					}
				} catch ex => {
					val message = s"{\"loggerMessage\":\"Reading response body failed: ${ex.getMessage()}\"}";
					request.get.discardEntityBytes()
					standardLog = standardLog.copy(
						body = Some(message.substring(0, math.min(message.length, maxConentLength /*1023*/))),
					)

					Future.successful(standardLog)
				}
			} else {
				standardLog = standardLog.copy(
					responseMessage = Some(response.get.entity.contentType.toString())
				)

				Future.successful(standardLog)
			}
		} else {
			Future.successful(standardLog)
		}


		for {
			withRequest <- accessLogWithRequest
			withResponse <- accessLogWithResponse
			log <- if userToken.isDefined 
				then UsersHandler.includeUserDetailsInLog(standardLog, userToken.get)
				else Future.successful(standardLog)
		} yield log
	}

	/**
	  * Log an request against any of Homie's services.
	  *
	  * @param ip
	  * @param route
	  * @param request
	  * @param response
	  * @return
	  */
	def logAccess(ip: RemoteAddress, route: String, request: Option[HttpRequest], response: Option[HttpResponse]): Future[Int] = {

		// Maybe in the future I delegate creating the log to `Routes.scala` and have it happen whilst awaiting the response.
		// Today is not that day!
		val accessLog: Future[AccessLog] = newAccessLog(ip, request, response) // Create the access log.
		val insertResult: Future[Int] = accessLog.flatMap(AccessLogsHandler.insert(_)) // Insert the access log.

		insertResult.onComplete {
			case Success(rows) =>
				println(s"(Info) ($route) Access log for IP: \"${ip.value}\" created")
			case Failure(ex) =>
				println(s"(Warn) ($route) Failed to create access log: ${ex.getMessage}")
		}

		return insertResult;
	}
}