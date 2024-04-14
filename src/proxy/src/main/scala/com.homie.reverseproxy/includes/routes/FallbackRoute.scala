// (c) 2024 @Maxylan
package com.homie.reverseproxy.includes.routes

import com.homie.reverseproxy.ReverseProxy
import com.homie.reverseproxy.includes.{Routing, RoutingDefinition, Logger, AccessLogsHandler, UsersHandler}
import akka.http.scaladsl.model.{RemoteAddress, HttpRequest, HttpResponse}
import scala.concurrent.{ExecutionContext, Future}

/**
  * Handle all requests to the app/frontend (homie.httpd). Default route for other requests
  */
object FallbackRoute extends RoutingDefinition
{
	implicit val executionContext: ExecutionContext = ReverseProxy.executionContext

	/**
	  * Handle all requests to the app/frontend (homie.httpd). Default route for other requests
	  *
	  * @param ip
	  * @param request
	  * @return
	  */
	def Handle(ip: RemoteAddress, request: HttpRequest): Future[HttpResponse] = {

		// Create a new access log entry.
		val accessLog = Logger.newAccessLog(ip, request)

		// Build targetUri to the App/Frontend (homie.httpd)
		val extractedRequest: HttpRequest = request.copy(
			uri = Routing.filterRequestUri(request.uri).withAuthority(
				ReverseProxy.env.required("HOMIE_HOST"), 
				ReverseProxy.env.required("HOMIE_PORT").toInt
			)
		)

		var logFutureSequence = Seq(
			UsersHandler.includeUserDetailsInLog(accessLog, accessLog.userToken), // Add user details to the access log.
			Logger.addRequestDetailsTo(accessLog, request), // Add request details to the access log.
		)

		// Send request.
		val responseFuture = Routing.requestHomie(extractedRequest)

		// Start this future chain after `responseFuture` has completed. 
		// Not awaited, `responseFuture` is what will be returned to `completed(...)`.
		val logFuture = responseFuture map { response =>
			logFutureSequence :+ Logger.addResponseDetailsTo(accessLog, response) // Add response details to the access log.
		}

		logFuture map { seq =>
			// Merges/Reduces `Seq[Future[AccessLog]]` into one AccessLog with all the latest details. 
			// Then inserts the merged `AccessLog` instance once all futures are complete.

			Future.sequence(seq) map {
				// Merge AccessLog instances 
				_.foldLeft(accessLog) { (acc, log) =>
					acc.copy(
						platformId = log.platformId.orElse(acc.platformId),
						userToken = log.userToken.orElse(acc.userToken),
						username = log.username.orElse(acc.username),
						body = log.body.orElse(acc.body),
						responseMessage = log.responseMessage.orElse(acc.responseMessage),
						responseStatus = log.responseStatus.orElse(acc.responseStatus)
					)
				}
			} map {
				AccessLogsHandler.insert(_)
			} 
		}

		// Route onComplete: Proxy request to "frontend" (homie.httpd)
		responseFuture map { response =>
			println(s"(Info) (homie.httpd) IP: \"${ip}\" Request complete!"); 
			response
		}
	}
}