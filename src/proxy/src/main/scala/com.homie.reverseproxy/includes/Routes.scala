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

	val route = {
		extractRequest { request =>
			pathPrefix("api") {
				path(".*".r) { path =>
					extractUri { uri =>
						val targetUri = uri.withAuthority(Properties.envOrElse("API_HOST", "homie.api"), Properties.envOrElse("API_PORT", "10001").toInt)
						val targetRequest = HttpRequest(method = request.method, uri = targetUri, entity = request.entity)
						val responseFuture: Future[HttpResponse] = Http().singleRequest(targetRequest)
						onComplete(responseFuture) {
							case Success(response) => complete(response)
							case Failure(ex) => complete(s"Request failed: ${ex.getMessage}")
						}
					}
				}
			} ~ {
				// Default route for other requests
				path(".*".r) { path =>
					extractUri { uri =>
						val targetUri = uri.withAuthority(Properties.envOrElse("HOMIE_HOST", "homie.httpd"), Properties.envOrElse("HOMIE_PORT", "10000").toInt)
						val targetRequest = HttpRequest(method = request.method, uri = targetUri, entity = request.entity)
						val responseFuture: Future[HttpResponse] = Http().singleRequest(targetRequest)
						onComplete(responseFuture) {
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
