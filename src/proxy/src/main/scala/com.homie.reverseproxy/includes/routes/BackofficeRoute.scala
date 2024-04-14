// (c) 2024 @Maxylan
package com.homie.reverseproxy.includes.routes

import com.homie.reverseproxy.ReverseProxy
import com.homie.reverseproxy.includes.{Routing, RoutingDefinition, Logger, AccessLogsHandler, UsersHandler}
import akka.http.scaladsl.model.{RemoteAddress, HttpEntity, HttpRequest, HttpResponse}
import scala.concurrent.{ExecutionContext, Future}
import concurrent.duration.DurationInt
import akka.stream.ActorMaterializer

/**
  * Handle all requests to the API (homie.api).
  */
object BackofficeRoute extends RoutingDefinition
{
	implicit val materializer: ActorMaterializer = ReverseProxy.materializer
	implicit val executionContext: ExecutionContext = ReverseProxy.executionContext

	val routeId = "homie.api"

	/**
	  * Handle all requests to the API (homie.api).
	  *
	  * @param ip
	  * @param request
	  * @param requestEntity
	  * @return `Future[HttpResponse]`
	  */
	def Handle(ip: RemoteAddress, request: HttpRequest, requestEntity: HttpEntity.Strict): Future[HttpResponse] = {

		// Create a new access log entry.
		var accessLog = Logger.newAccessLog(ip, request)

		// Build targetUri to the API (homie.api) - Remove the prefix from the path
		val extractedRequest: HttpRequest = request.copy(
			uri = Routing.filterRequestUri(request.uri, Option("api")).withAuthority(
				ReverseProxy.env.required("API_HOST"), 
				ReverseProxy.env.required("API_PORT").toInt
			)
		)

		// Send request.
		val responseFuture = Routing.requestHomie(extractedRequest).map(Routing.filterLocationHeader(_, "api"))

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

		// Route onComplete: Proxy request to "backoffice" (homie.api / homie.fastapi)
		response onComplete { r => println(s"(Info) ($routeId) IP: \"${ip}\" Request complete!"); }
		response
	}
}