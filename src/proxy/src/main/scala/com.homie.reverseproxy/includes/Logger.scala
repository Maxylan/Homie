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
	 * Attempt to store the request body in an `AccessLog` instance.
	 * 
	  * @param accessLog
	  * @param requestEntity
	  * @return `AccessLog`
	 */
	def addRequestEntityDetailsTo(accessLog: AccessLog, requestEntity: HttpEntity.Strict): AccessLog = {

		if (requestEntity.contentType.toString().contains("application/json")) then {

			val maxContentLength = 1023*63 // 64449
			val asUtf8: String = requestEntity.data.utf8String;

			if (asUtf8.length > maxContentLength) then {
				println(s"(Warn) Request body too large: ${asUtf8.length} > $maxContentLength")

				accessLog.copy(
					body = Option(s"{\"loggerMessage\":\"Request body too large (${asUtf8.length}).\"}"),
				)
			} 

			accessLog.copy(
				body = Option(asUtf8.substring(0, math.min(asUtf8.length, maxContentLength /*64449*/))),
			)
		} 
		else {
			requestEntity.discardBytes() // just in case?
			accessLog.copy(
				body = Option(requestEntity.contentType.toString())
			)
		}
	}

	/**
	 * Attempt to store the response body (and status, if included) in an `AccessLog` instance.
	 * 
	  * @param accessLog
	  * @param responseEntity
	  * @param status
	  * @return `AccessLog`
	 */
	def addResponseEntityDetailsTo(accessLog: AccessLog, responseEntity: HttpEntity.Strict, status: Option[Int] = None): AccessLog = {

		if (responseEntity.contentType.toString().contains("application/json")) then {

			val maxConentLength = 1023
			val asUtf8: String = responseEntity.data.utf8String;
			accessLog.copy(
				responseMessage = Option(asUtf8.substring(0, math.min(asUtf8.length, maxConentLength))),
				responseStatus = status
			)
		} 
		else {
			responseEntity.discardBytes() // just in case?
			accessLog.copy(
				responseMessage = Option(responseEntity.contentType.toString()),
				responseStatus = status
			)
		}
	}
}