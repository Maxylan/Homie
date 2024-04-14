// (c) 2024 @Maxylan
package com.homie.reverseproxy.includes.routes

import com.homie.reverseproxy.ReverseProxy
import com.homie.reverseproxy.includes.{Routing, RoutingDefinition, Logger, AccessLogsHandler, UsersHandler}
import akka.http.scaladsl.model.{RemoteAddress, HttpEntity, HttpRequest, HttpResponse}
import scala.concurrent.{ExecutionContext, Future}
import concurrent.duration.DurationInt
import akka.stream.ActorMaterializer

/**
  * Handle all requests to the app/frontend (homie.httpd). Default route for other requests
  */
object FallbackRoute extends RoutingDefinition
{
	implicit val materializer: ActorMaterializer = ReverseProxy.materializer
	implicit val executionContext: ExecutionContext = ReverseProxy.executionContext

	val routeId = "homie.httpd"

	/**
	  * Handle all requests to the app/frontend (homie.httpd). Default route for other requests
	  *
	  * @param ip
	  * @param request
	  * @param requestEntity
	  * @return `Future[HttpResponse]`
	  */
	def Handle(ip: RemoteAddress, request: HttpRequest, requestEntity: HttpEntity.Strict): Future[HttpResponse] = {

		// Create a new access log entry.
		var accessLog = Logger.newAccessLog(ip, request)

		// Build targetUri to the App/Frontend (homie.httpd)
		val extractedRequest: HttpRequest = request.copy(
			uri = Routing.filterRequestUri(request.uri).withAuthority(
				ReverseProxy.env.required("HOMIE_HOST"), 
				ReverseProxy.env.required("HOMIE_PORT").toInt
			)
		)

		// Send request.
		val responseFuture = Routing.requestHomie(extractedRequest)

		accessLog = Logger.addUpdatedRequestDetailsTo(accessLog, extractedRequest) // Enhance the log with updated request details.
		accessLog = Logger.addRequestEntityDetailsTo(accessLog, requestEntity) // Add request details to the access log.
		val accessLogWithUserDetails = UsersHandler.includeUserDetailsInLog(accessLog, accessLog.userToken) // Add user details to the access log.

		// Start this future chain after `responseFuture` has completed. 
		// `Future[HttpResponse]` is what will be returned to the Route Definition's `completed(...)` directive.
		val response = for {
			response <- responseFuture
			responseEntity: HttpEntity.Strict <- response.entity.toStrict(3.seconds)
			newAccessLog <- accessLogWithUserDetails
		} yield {
			// Add response details to the access log.
			accessLog = Logger.addResponseEntityDetailsTo(newAccessLog, responseEntity, Option(response.status.intValue))
			AccessLogsHandler.insert(accessLog) // ..finally: Inserts the access log into the database.

			// Yields the `Future[HttpResponse]` with 'responseEntity' (`HttpEntity.Strict`)
			response.withEntity(responseEntity)
		}

		// Route onComplete: Proxy request to "frontend" (homie.httpd)
		response onComplete { r => println(s"(Info) ($routeId) IP: \"${ip}\" Request complete!"); }
		response
	}
}