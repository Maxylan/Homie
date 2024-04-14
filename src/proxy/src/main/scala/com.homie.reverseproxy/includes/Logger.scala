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
	  * @param request
	  * @param response
	  * @return new `AccessLog`
	  */
	def newAccessLog(requestingIp: RemoteAddress, request: HttpRequest, response: Option[HttpResponse] = None): AccessLog = {

		println(s"(Debug) newAccessLog");

		val timestamp = new Timestamp(System.currentTimeMillis());
		val platformId: Option[Int] = request.headers.find(_ is "x-requesting-platform") map { _.value().toInt }
		val userToken: Option[String] = request.headers.find(_ is "x-requesting-user") map { _.value() }
		val requestMethod = request.method.value
		val requestUri = request.uri.toString
		val requestPath = Option(request.uri.path.toString)
		val requestQueryString = request.uri.rawQueryString
		val requestHeaders = Option(request.headers.mkString("\n"))
		val responseStatus = response map { _.status.intValue }

		/*
		return AccessLog(
			None, // id
			platformId, // platform_id - Requesting platform..
			userToken, // userToken/token - Requesting user..
			None, // username
			timestamp,
			ReverseProxy.env.version.homie,
			requestingIp.value, // ip of caller
			requestMethod, // method
			requestUri, // uri
			requestPath, // path
			requestQueryString, // parameters
			requestUri, // full_url
			requestHeaders,
			None, // body
			None, // responseMessage,
			responseStatus // responseStatus,
		)
		*/

		return AccessLog < (
			timestamp,
			ReverseProxy.env.version.homie,
			requestingIp.value,
			requestMethod,
			requestUri,
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