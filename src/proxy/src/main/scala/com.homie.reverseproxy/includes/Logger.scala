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
import scala.util.Properties

object Logger 
{
	implicit val executionContext: ExecutionContext = ReverseProxy.executionContext
	implicit val materializerContext: ActorMaterializer = ReverseProxy.materializer

	/**
	  * Create a new access log entry.
	  *
	  * @param requestingIp
	  * @param originalRequest
	  * @param response
	  * @return new `AccessLog`
	  */
	def newAccessLog(requestingIp: RemoteAddress, originalRequest: HttpRequest, response: Option[HttpResponse] = None): AccessLog = {

		val timestamp = new Timestamp(System.currentTimeMillis());
		val platformId: Option[Int] = originalRequest.headers.find(_ is "x-requesting-platform") map { _.value().toInt }
		val userToken: Option[String] = originalRequest.headers.find(_ is "x-requesting-user") map { _.value() }
		val requestMethod = originalRequest.method.value
		val requestUrl = originalRequest.uri.toString
		val requestUri = originalRequest.uri.authority.toString
		val requestPath = Option(originalRequest.uri.path.toString)
		val requestQueryString = originalRequest.uri.rawQueryString
		val requestHeaders = Option(originalRequest.headers.mkString("\n"))
		val responseStatus = response map { _.status.intValue }

		return AccessLog < (
			timestamp,
			ReverseProxy.env.version.homie,
			requestingIp.value,
			requestMethod,
			originalRequest.uri.toString, // ..originalUrl
			requestUrl, // ..fullUrl, at this point its the same as `originalUrl`.
			requestUri,
			Map[String, Option[String]] (
				"platformId" -> platformId.map(_.toString),
				"userToken" -> userToken,
				"path" -> requestPath,
				"parameters" -> requestQueryString,
				"headers" -> requestHeaders,
				"responseStatus" -> responseStatus.map(_.toString),
			)
		)
	}

	/**
	 * Overwrite all "original" request details with the new request details. (except `originalUrl`)
	 * 
	  * @param accessLog
	  * @param newRequest
	  * @return `AccessLog`
	 */
	def addUpdatedRequestDetailsTo(accessLog: AccessLog, newRequest: HttpRequest): AccessLog = {

		val requestMethod = newRequest.method.value
		val requestUrl = newRequest.uri.toString
		val requestUri = newRequest.uri.authority.toString
		val requestPath = Option(newRequest.uri.path.toString)
		val requestQueryString = newRequest.uri.rawQueryString
		val requestHeaders = Option(newRequest.headers.mkString("\n"))

		accessLog.copy(
			method = requestMethod,
			fullUrl = requestUrl,
			uri = requestUri,
			path = requestPath,
			parameters = requestQueryString,
			headers = requestHeaders,
		)
	}

	/**
	 * If the request is defined, then we will attempt to store the request body.
	 * 
	  * @param accessLog
	  * @param request
	  * @return `Future[AccessLog]`
	 */
	def addRequestDetailsTo(accessLog: AccessLog, request: HttpRequest): Future[AccessLog] = {

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
							body = Option(s"{\"loggerMessage\":\"Request body too large (${body.length}).\"}"),
						)
					} 
					else {
						accessLog.copy(
							body = Option(body.substring(0, math.min(body.length, maxConentLength /*64449*/))),
						)
					}
				}
			} 
			catch ex => {
				val message = s"{\"loggerMessage\":\"Reading request body failed: ${ex.getMessage()}\"}";
				request.discardEntityBytes() // just in case?

				Future.successful(
					accessLog.copy(
						body = Option(message.substring(0, math.min(message.length, maxConentLength /*64449*/))),
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
	def addResponseDetailsTo(accessLog: AccessLog, response: HttpResponse): Future[AccessLog] = {

		if (response.entity.contentType.toString().contains("application/json")) then {

			val maxConentLength = 1023

			try {
				for {
					entity <- response.entity.toStrict(3.seconds)
					body <- Future.successful(entity.data.utf8String)
				} yield {
					accessLog.copy(
						responseMessage = Option(body.substring(0, math.min(body.length, maxConentLength)))
					)
				}
			} 
			catch ex => {
				val message = s"{\"loggerMessage\":\"Reading response body failed: ${ex.getMessage()}\"}";
				response.discardEntityBytes() // just in case?

				Future.successful(
					accessLog.copy(
						body = Option(message.substring(0, math.min(message.length, maxConentLength /*1023*/))),
					)
				)
			}
		} 
		else {
			Future.successful(
				accessLog.copy(
					responseMessage = Option(response.entity.contentType.toString()),
					responseStatus = Option(response.status.intValue)
				)
			)
		}
	}
}