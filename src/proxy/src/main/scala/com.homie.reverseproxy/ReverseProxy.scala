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

	// Load configurations and define application-wide variables
	val homieReverseProxyConfiguration = ConfigFactory.load("homie-http-core")
	val config: Config = homieReverseProxyConfiguration.withFallback(ConfigFactory.load())

	/** (Struct) Versioning of the application. Found in `.env` file (or environment variables) */
	case class AppVersion (
		proxy: String,
		homie: String,
	)

	/** (Struct) Environment variables for the application. */
	case class Variables (
		version: AppVersion,
		required: Map[String, String],
		options: Map[String, String],
	)

	/**
	  * Environment variables for the application.
	  *
	  * @param version Versioning Environment variables.
	  * @param required Environment variables required to be defined. If omitted, they will throw `java.util.NoSuchElementException`.
	  * @param options Optional environment variables. If omitted, there should be fallback/default values for these.
	  */
	lazy val env: Variables = Variables(
		version = AppVersion(
			proxy = Properties.envOrNone("PROXY_V"/*, "1"*/).get, // Required
			homie = Properties.envOrNone("HOMIE"/*, "1.1111"*/).get // Required
		),
		required = Map[String, String] (
			"HOST" -> Properties.envOrNone("HOST"/*, "homie.proxy"*/).get,
			"PORT" -> Properties.envOrNone("PORT"/*, "8080"*/).get,
			"PORT_FORWARDED" -> Properties.envOrNone("PORT_FORWARDED"/*, "80"*/).get,
			"SSL_PORT" -> Properties.envOrNone("SSL_PORT"/*, "44300"*/).get,
			"SSL_PORT_FORWARDED" -> Properties.envOrNone("SSL_PORT_FORWARDED"/*, "443"*/).get,     
			"API_HOST" -> Properties.envOrNone("API_HOST"/*, "homie.api"*/).get,
			"API_PORT" -> Properties.envOrNone("API_PORT"/*, "10001"*/).get,
			"HOMIE_HOST" -> Properties.envOrNone("HOMIE_HOST"/*, "homie.httpd"*/).get,
			"HOMIE_PORT" -> Properties.envOrNone("HOMIE_PORT"/*, "10000"*/).get,
			"DB_HOST" -> Properties.envOrNone("DB_HOST"/*, "homie.db"*/).get,
			"DB_PORT" -> Properties.envOrNone("DB_PORT"/*, "10003"*/).get,
			"DB_PROXY_USER" -> Properties.envOrNone("DB_PROXY_USER"/*, "proxy"*/).get,
			"DB_PROXY_PASSWORD" -> Properties.envOrNone("DB_PROXY_PASSWORD"/*, "password"*/).get
		),
		options = Map[String, String](
			"SSL_SELF_SIGNED" -> Properties.envOrElse("SSL_SELF_SIGNED", "true"),
			"SSL_KEYSTORE" -> Properties.envOrElse("SSL_KEYSTORE", ""),
			"SSL_KEYSTORE_PASSWORD" -> Properties.envOrElse("SSL_KEYSTORE_PASSWORD", ""),
			"SSL_SNAKEOIL_FULLCHAIN_CERTIFICATE" -> Properties.envOrElse("SSL_SNAKEOIL_FULLCHAIN_CERTIFICATE", ""),
			"SSL_CERTIFICATE" -> Properties.envOrElse("SSL_CERTIFICATE", ""),
			"SSL_SNAKEOIL_PRIVATE_KEY" -> Properties.envOrElse("SSL_SNAKEOIL_PRIVATE_KEY", ""),
			"SSL_PRIVATE_KEY" -> Properties.envOrElse("SSL_PRIVATE_KEY", ""),
			"SSL_PRIVATE_KEY_PASSWORD" -> Properties.envOrElse("SSL_PRIVATE_KEY_PASSWORD", "")
		)
	)

	// Create a thread pool for the execution context
	val executor = Executors.newFixedThreadPool(4)
	implicit val executionContext: ExecutionContext = ExecutionContext.fromExecutorService(executor)

	// Create the actor system and materializer
	implicit val system: ActorSystem = ActorSystem("ReverseProxy", config)
	implicit val materializer: ActorMaterializer = ActorMaterializer()

	// Import a standard dispatcher that will be used by all actors
	import system.dispatcher;
	import SSLHelpers._;
	import Routes._;

	// Create a promise to await the termination of the server
	/* val serverTerminationPromise = Promise[Unit]() */

	val hostname: String = env.required("HOST");
	val tcpPortForwarded: Int = env.required("PORT").toInt; // 8080 - Is what's exposed internally (docker cluster network)
	val tcpPort: Int = env.required("PORT_FORWARDED").toInt; // 80 - Is what's exposed on the Host PC
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
		println(s"(Debug) ssl-config.self-signed: ${env.options("SSL_SELF_SIGNED")}")

		val keyStore: KeyStore = SSLHelpers.getKeyStore("PKCS12")
		val keyManagers: Array[KeyManager] = SSLHelpers.getKeyManagers(keyStore)
		val trustManagers: Array[TrustManager] = SSLHelpers.getTrustManagers(keyStore)
		val sslPort: Int = env.required("SSL_PORT").toInt; // 44300 - Is what's exposed internally (docker cluster network)
		val sslPortForwarded: Int = env.required("SSL_PORT_FORWARDED").toInt; // 443 - Is what's exposed on the Host PC
		val sslContext: SSLContext = SSLContext.getInstance("TLS")
		sslContext.init(
			keyManagers,
			trustManagers,
			new SecureRandom()
		)

		val bindingHttpsFuture = Http().bindAndHandle(Routes.route, "0.0.0.0", sslPort, ConnectionContext.httpsServer(sslContext))
		println(s"Homie Reverse Proxy online at \"https://0.0.0.0:${sslPort}/\" (\"https://${hostname}:${sslPortForwarded}/\") (SSL)")

		// Holy !$@~ this cost me upwards of 48h of troubleshooting and neither 
		// Stack Overflow, Akka's Docs, Google or ChatGPT where of any help.

		// When defining system, materializer & execution context here, then
		// awaiting a future or user-input here, future requests/routes "hang",
		// then time-out without ever responding. 
		// 
		// **This cost me all of April's sanity**
		// ---

		// Block the main thread until the server is shut down
		/* Await.result(serverTerminationPromise.future, Duration.Inf) */

		// Maybe this is cleaner than awaiting infinity.
		/* while (StdIn.readLine() != "q") { }
		bindingHttpsFuture
			.flatMap(_.unbind())
			.onComplete(_ => system.terminate()) */
		/* sys.addShutdownHook {
			bindingHttpsFuture
				.flatMap(_.unbind())
				.onComplete(_ => system.terminate())
		} */
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
