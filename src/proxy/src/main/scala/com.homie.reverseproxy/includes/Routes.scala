package com.homie.reverseproxy

import akka.actor.ActorSystem
import akka.stream.ActorMaterializer
import scala.util.Properties
import akka.http.scaladsl.Http
import akka.http.scaladsl.model.{HttpRequest, HttpResponse, Uri}
import akka.http.scaladsl.server.Directives._
import scala.concurrent.{Promise, Future, Await}
import scala.concurrent.duration.Duration
import scala.util.{Failure, Success}

object Routes {
	implicit val system: ActorSystem = ReverseProxy.system
	implicit val materializer: ActorMaterializer = ReverseProxy.materializer
	val apiHost = Properties.envOrNone("API_HOST"/*, "homie.api"*/)
	val apiPort = Properties.envOrNone("API_PORT"/*, "10001"*/)
	val homieHost = Properties.envOrNone("HOMIE_HOST"/*, "homie.httpd"*/)
	val homiePort = Properties.envOrNone("HOMIE_PORT"/*, "10000"*/)

	def requestHomie(targetUri: Uri, request: HttpRequest): Future[HttpResponse] = {
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
