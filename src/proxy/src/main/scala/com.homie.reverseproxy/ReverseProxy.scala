// (c) 2024 @Maxylan
package com.homie.reverseproxy

import com.typesafe.config.{Config, ConfigFactory}
import scala.util.Properties
import akka.actor.ActorSystem
import akka.stream.ActorMaterializer
import akka.http.scaladsl.Http
import akka.http.scaladsl.model.{HttpRequest, HttpResponse, Uri}
import akka.http.scaladsl.server.Directives._
import akka.http.scaladsl.settings.ServerSettings
import java.util.concurrent.{Executors, TimeUnit}
import scala.concurrent.{ExecutionContext, Promise, Future, Await}
import scala.concurrent.duration.Duration
import akka.http.scaladsl.ConnectionContext
import javax.net.ssl.{KeyManager, SSLContext, TrustManager}
import java.security.{SecureRandom, Security, Provider, KeyStore}
import com.homie.reverseproxy.includes._
import scala.io.StdIn

object ReverseProxy extends App {
	// Create a thread pool for the execution context
	val executor = Executors.newFixedThreadPool(4)
	implicit val executionContext: ExecutionContext = ExecutionContext.fromExecutorService(executor)

	val homieReverseProxyConfiguration = ConfigFactory.load("homie-http-core")
	val config: Config = homieReverseProxyConfiguration.withFallback(ConfigFactory.load())

	// Create the actor system and materializer
	implicit val system: ActorSystem = ActorSystem("ReverseProxy", config)
	implicit val materializer: ActorMaterializer = ActorMaterializer()

	lazy val proxyVersion = Properties.envOrNone("PROXY_V"/*, "1"*/)
	lazy val homieVersion = Properties.envOrNone("HOMIE"/*, "1.1111"*/)

	// Import a standard dispatcher that will be used by all actors
	import system.dispatcher;
	import SSLHelpers._;
	import Routes._;

	// Create a promise to await the termination of the server
	// val serverTerminationPromise = Promise[Unit]()

	val hostname: String = Properties.envOrElse("HOST", "homie.proxy");
	val tcpPortForwarded: Int = Properties.envOrElse("PORT", "8080").toInt;
	val tcpPort: Int = Properties.envOrElse("PORT_FORWARDED", "80").toInt;
	val bindingFuture = Http().bindAndHandle(Routes.route, "0.0.0.0", tcpPort)

	println(s"Homie Reverse Proxy online at \"http://0.0.0.0:${tcpPort}/\" (\"http://${hostname}:${tcpPortForwarded}/\")")

	sys.addShutdownHook {
		bindingFuture
			.flatMap(_.unbind())
			.onComplete(_ => {
				executor.awaitTermination(30, TimeUnit.SECONDS)
				executor.shutdown()
				system.terminate()
			})
	}

	try {
		// SSL
		println(s"SSL Security.getProviders():")
		Security.getProviders().map(provider => provider.toString()).foreach(println)
		println(s"(Debug) ssl-config.self-signed: ${Properties.envOrElse("SSL_SELF_SIGNED", "true").toBoolean}")

		val keyStore: KeyStore = SSLHelpers.getKeyStore("PKCS12")
		val keyManagers: Array[KeyManager] = SSLHelpers.getKeyManagers(keyStore)
		val trustManagers: Array[TrustManager] = SSLHelpers.getTrustManagers(keyStore)
		val sslPort: Int = Properties.envOrElse("SSL_PORT", "44300").toInt
		val sslPortForwarded: Int = Properties.envOrElse("SSL_PORT_FORWARDED", "443").toInt
		val sslContext: SSLContext = SSLContext.getInstance("TLS")
		sslContext.init(
			keyManagers,
			trustManagers,
			new SecureRandom()
		)

		val bindingHttpsFuture = Http().bindAndHandle(Routes.route, "0.0.0.0", sslPort, ConnectionContext.httpsServer(sslContext))
		println(s"Homie Reverse Proxy online at \"https://0.0.0.0:${sslPort}/\" (\"https://${hostname}:${sslPortForwarded}/\") (SSL)")

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
