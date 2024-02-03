// (c) 2024 @Maxylan
package com.homie.reverseproxy

import scala.util.Properties
import akka.actor.ActorSystem
import akka.stream.ActorMaterializer
import akka.http.scaladsl.Http
import akka.http.scaladsl.model.{HttpRequest, HttpResponse, Uri}
import akka.http.scaladsl.server.Directives._
import scala.concurrent.{Promise, Future, Await}
import scala.concurrent.duration.Duration
import akka.http.scaladsl.ConnectionContext
import javax.net.ssl.{KeyManager, SSLContext, TrustManager}
import java.security.{SecureRandom, Security, Provider, KeyStore}
import scala.io.StdIn

object ReverseProxy extends App {
	implicit val system: ActorSystem = ActorSystem("ReverseProxy")
	implicit val materializer: ActorMaterializer = ActorMaterializer()
	import system.dispatcher;
	import SSLHelpers._;
	import Routes._;

	// Create a promise to await the termination of the server
	val serverTerminationPromise = Promise[Unit]()

	val tcpPort: Int = Properties.envOrElse("PORT", "80").toInt;
	val bindingFuture = Http().bindAndHandle(Routes.route, "0.0.0.0", tcpPort)

	println(s"Homie Reverse Proxy online at \"http://0.0.0.0:${tcpPort}/\"")

	sys.addShutdownHook {
		bindingFuture
			.flatMap(_.unbind())
			.onComplete(_ => system.terminate())
	}

	try {
		// SSL
		println(s"SSL Security.getProviders():")
		Security.getProviders().map(provider => provider.toString()).foreach(println)
		println(s"(Debug) ssl-config.self-signed: ${Properties.envOrElse("SSL_SELF_SIGNED", "true").toBoolean}")

		val keyStore: KeyStore = SSLHelpers.getKeyStore("PKCS12")
		val keyManagers: Array[KeyManager] = SSLHelpers.getKeyManagers(keyStore)
		val trustManagers: Array[TrustManager] = SSLHelpers.getTrustManagers(keyStore)
		val sslPort: Int = Properties.envOrElse("SSL_PORT", "443").toInt
		val sslContext: SSLContext = SSLContext.getInstance("TLS")
		sslContext.init(
			keyManagers,
			trustManagers,
			new SecureRandom()
		)

		val bindingHttpsFuture = Http().bindAndHandle(Routes.route, "0.0.0.0", sslPort, ConnectionContext.httpsServer(sslContext))
		println(s"Homie Reverse Proxy online at \"https://0.0.0.0:${sslPort}/\" (SSL)")

		// // Block the main thread until the server is shut down
		// Await.result(serverTerminationPromise.future, Duration.Inf)

		// Maybe this is cleaner than awaiting infinity.
		while (StdIn.readLine() != "q") { }
		bindingHttpsFuture
			.flatMap(_.unbind())
			.onComplete(_ => system.terminate())
		/*
		sys.addShutdownHook {
			bindingHttpsFuture
				.flatMap(_.unbind())
				.onComplete(_ => system.terminate())
		}
		*/
	}
	catch {
		case ei: ExceptionInInitializerError => {
			println(s"ExceptionInInitializerError: ${ei.getMessage()}")
			ei.printStackTrace()
			println("---")
			if (ei.getCause() != null) then ei.getCause().printStackTrace()
		}
		case e: Exception => {
			println(s"Exception: ${e.getMessage()}"); 
			e.printStackTrace();
		}
	}
}
