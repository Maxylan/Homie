// (c) 2024 @Maxylan
package com.homie.reverseproxy.includes

import akka.stream.ActorMaterializer
import scala.concurrent.{ExecutionContext, Promise, Future}
import scala.concurrent.duration.{Duration, DurationInt}
import scala.util.{Success, Failure}
import akka.http.scaladsl.model.{HttpRequest, HttpResponse, HttpHeader, HttpEntity, RemoteAddress}
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
	def newAccessLog(requestingIp: RemoteAddress, request: HttpRequest, response: Option[HttpResponse] = None): AccessLog = {

		println(s"(Debug) newAccessLog");
		val timestamp = new Timestamp(System.currentTimeMillis());
		println(s"(Debug) (for) timestamp " + timestamp.toString());

		val platformId: Option[Int] = request.headers.find(_ is "x-requesting-platform") map { _.value().toInt }
		println(s"(Debug) (for) platformId");

		val userToken: Option[String] = request.headers.find(_ is "x-requesting-user") map { _.value() }
		println(s"(Debug) (for) userToken");

		val requestMethod = request.method.value
		val requestUri = request.uri.toString
		val requestPath = Some(request.uri.path.toString)
		val requestQueryString = request.uri.rawQueryString
		val requestHeaders = Some(request.headers.mkString("\n"))
		val requestStatus = response map { _.status.intValue } getOrElse 503

		return new AccessLog(
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
	}

	/**
	 * If the request is defined, then we will attempt to store the request body.
	 * 
	  * @param accessLog
	  * @param request
	  * @return `Future[AccessLog]`
	 */
	def addRequestDetailsTo(accessLog: AccessLog, request: HttpRequest, route: String = ""): Future[AccessLog] = {
		println(s"(Debug) ($route) add*Request*DetailsTo");

		if request.entity.contentType.toString().contains("application/json") then {

			val maxConentLength = 1023*63 // 64449

			try {
				for {
					entity <- request.entity.toStrict(3.seconds)
					body <- Future.successful(entity.data.utf8String)
				} yield {
					if (body.length > maxConentLength) then {
						println(s"(Warn) Request body too large: ${body.length} > $maxConentLength")

						accessLog.copy(
							body = Some(s"{\"loggerMessage\":\"Request body too large (${body.length}).\"}"),
						)
					} 
					else {
						accessLog.copy(
							body = Some(body.substring(0, math.min(body.length, maxConentLength /*64449*/))),
						)
					}
				}
			} 
			catch ex => {
				val message = s"{\"loggerMessage\":\"Reading request body failed: ${ex.getMessage()}\"}";
				request.discardEntityBytes() // just in case?

				Future.successful(
					accessLog.copy(
						body = Some(message.substring(0, math.min(message.length, maxConentLength /*64449*/))),
					)
				)
			}
		} 
		else {
			Future.successful(accessLog)
		}
	}

	/**
	 * If the response is defined, then we will store response status and attempt to read the response body.
	 * 
	  * @param accessLog
	  * @param request
	  * @return `Future[AccessLog]`
	 */
	def addResponseDetailsTo(accessLog: AccessLog, response: HttpResponse, route: String = ""): Future[AccessLog] = {
		println(s"(Debug) ($route) add*Response*DetailsTo");

		if (response.entity.contentType.toString().contains("application/json")) then {

			val maxConentLength = 1023

			try {
				for {
					entity <- response.entity.toStrict(3.seconds)
					body <- Future.successful(entity.data.utf8String)
				} yield {
					accessLog.copy(
						responseMessage = Some(body.substring(0, math.min(body.length, maxConentLength)))
					)
				}
			} 
			catch ex => {
				val message = s"{\"loggerMessage\":\"Reading response body failed: ${ex.getMessage()}\"}";
				response.discardEntityBytes() // just in case?

				Future.successful(
					accessLog.copy(
						body = Some(message.substring(0, math.min(message.length, maxConentLength /*1023*/))),
					)
				)
			}
		} 
		else {
			Future.successful(
				accessLog.copy(
					responseMessage = Some(response.entity.contentType.toString()),
					responseStatus = response.status.intValue
				)
			)
		}
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
	/*
	def logAccess(ip: RemoteAddress, route: String, request: HttpRequest, response: Option[HttpResponse]): Future[Int] = {

		// Create the access log.
		var accessLog: AccessLog = newAccessLog(ip, request, response) 
		
		// Add request details to the access log.
		val accessLogWithRequestDetails: Option[Future[AccessLog]] = if (request.isDefined) then Some(
			for {
				withRequest <- addRequestDetailsTo(accessLog, request.get, route)
			} yield {
				accessLog = accessLog.copy(
					body = withRequest.body
				)

				accessLog
			}
		) else None
		
		// Add response details to the access log.
		val accessLogWithResponseDetails: Option[Future[AccessLog]] = if (response.isDefined) then Some(
			for {
				withResponse <- addResponseDetailsTo(accessLog, response.get, route)
			} yield {
				accessLog = accessLog.copy(
					responseMessage = withResponse.responseMessage,
					responseStatus = withResponse.responseStatus
				)

				accessLog
			}
		) else None
		
		// Add user details to the access log.
		val accessLogWithUserDetails: Option[Future[AccessLog]] = if (request.isDefined) then Some(
			for {
				withUserDetails <- UsersHandler.includeUserDetailsInLog(accessLog, accessLog.userToken.get)
			} yield {
				accessLog = accessLog.copy(
					username = withUserDetails.username
				)

				accessLog
			}
		) else None

		// List of Option[Future[AccessLog]]
		val futuresList: List[Option[Future[AccessLog]]] = List(
			accessLogWithRequestDetails,
			accessLogWithResponseDetails, 
			accessLogWithUserDetails
		)

		// Convert List[Option[Future[AccessLog]]] to Future[List[Option[AccessLog]]], `.flatten` Removes `None` values.
		val futureList: Future[List[AccessLog]] = Future.sequence(futuresList.flatten)

		// Merge AccessLog instances
		val mergedAccessLog: Future[AccessLog] = futureList.map { 
			_.foldLeft(accessLog) { (acc, log) =>
				acc.copy(
					body = log.body.orElse(acc.body),
					responseMessage = log.responseMessage.orElse(acc.responseMessage),
					responseStatus = if log.responseStatus == 503 then acc.responseStatus else log.responseStatus,
					username = log.username.orElse(acc.username)
				)
			}
		}

		// Insert the access log.
		val insertResult: Future[Int] = mergedAccessLog.flatMap(AccessLogsHandler.insert(_)) // Insert the access log.

		insertResult.onComplete {
			case Success(rows) =>
				println(s"(Info) ($route) Access log for IP: \"${ip.value}\" created")
			case Failure(ex) =>
				println(s"(Warn) ($route) Failed to create access log: ${ex.getMessage}")
		}

		insertResult;
	}
	*/
}