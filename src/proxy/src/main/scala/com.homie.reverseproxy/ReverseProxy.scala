// (c) 2024 @Maxylan
package com.homie.reverseproxy

import akka.actor.ActorSystem
import akka.http.scaladsl.Http
import akka.http.scaladsl.model.{HttpRequest, HttpResponse, Uri}
import akka.stream.ActorMaterializer
import akka.http.scaladsl.server.Directives._
import scala.concurrent.Future
import scala.util.{Failure, Success}
import akka.http.scaladsl.ConnectionContext
import javax.net.ssl.{KeyManager, KeyManagerFactory, SSLContext, TrustManagerFactory, X509TrustManager}
import java.security.SecureRandom
import javax.net.ssl.KeyManagerFactorySpi
import java.security.Provider
import java.security.KeyStore
import java.security.cert.CertificateFactory
import java.security.Security
import com.typesafe.config.ConfigFactory
import javax.net.ssl.TrustManager

object ReverseProxy extends App {
	implicit val system: ActorSystem = ActorSystem("ReverseProxy")
	implicit val materializer: ActorMaterializer = ActorMaterializer()
	import system.dispatcher;
	import SSLHelpers._;
	val config = ConfigFactory.load();

	val route = {
		extractRequest { request =>
			pathPrefix("api") {
				path(".*".r) { path =>
					extractUri { uri =>
						val targetUri = uri.withAuthority(config.getString("api.host"), config.getInt("api.port")) // "127.0.0.1"
						val targetRequest = HttpRequest(uri = targetUri, method = request.method, entity = request.entity)
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
						val targetUri = uri.withAuthority(config.getString("homie.host"), config.getInt("homie.port")) // "127.0.0.1"
						val targetRequest = HttpRequest(uri = targetUri, method = request.method, entity = request.entity)
						val responseFuture: Future[HttpResponse] = Http().singleRequest(targetRequest)
						onComplete(responseFuture) {
							case Success(response) => complete(response)
							case Failure(ex) => complete(s"Request failed: ${ex.getMessage}")
						}
					}
				}
			}
		}
	};

	val tcpPort: Int = config.getInt("proxy.port");
	val bindingFuture = Http().bindAndHandle(route, "0.0.0.0", tcpPort)
	println(s"Server online at http://0.0.0.0:${tcpPort}/")

	try {
		// SSL
		println(s"Security.getProviders(): ${Security.getProviders().toString()}");
		println(s"ssl-config.self-signed: ${config.getString("ssl-config.self-signed")}");

		val keyStore: KeyStore = SSLHelpers.getKeyStore("PKCS12");
		val keyManagers: Array[KeyManager] = SSLHelpers.getKeyManagers(keyStore);
		val trustManagers: Array[TrustManager] = SSLHelpers.getTrustManagers(keyStore);
		val sslPort: Int = config.getInt("proxy.port_ssl");
		val sslContext: SSLContext = SSLContext.getInstance("TLS"); 
		sslContext.init(
			keyManagers,
			trustManagers,
			new SecureRandom()
		);

		println(s"keyStore, keyManagers, trustManagers and sslContext created and initialized: ${keyStore.toString()}/");

		val bindingHttpsFuture = Http().bindAndHandle(
			route, 
			"0.0.0.0", 
			config.getInt("proxy.port_ssl"), 
			ConnectionContext.httpsServer(sslContext)
		);

		println(s"Server online at https://0.0.0.0:${sslPort}/");
	}
	catch {
		case ei: ExceptionInInitializerError => {
			println("ExceptionInInitializerError");
			ei.printStackTrace();
			println("---");
			println(s"ExceptionInInitializerError: ${ei.getMessage()}\n${ei.getMessage()}\n${}\n\n${ei.getCause()}\n${ei.getCause().toString()}\n${ei.getCause().getMessage()}");
			println("------");
			ei.getCause().printStackTrace();
		}
		case e: Exception => {
			println(s"Exception: ${e.getMessage()}"); 
			e.printStackTrace();
		}
	}

	sys.addShutdownHook {
		bindingFuture
			.flatMap(_.unbind())
			.onComplete(_ => system.terminate())
	}
}
