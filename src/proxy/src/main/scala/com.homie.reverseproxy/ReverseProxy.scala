// (c) 2024 @Maxylan
package com.homie.reverseproxy

import com.typesafe.config.{Config, ConfigFactory}
import akka.stream.ActorMaterializer
import akka.actor.ActorSystem
import scala.util.Properties
import akka.http.scaladsl.Http
import java.util.concurrent.{Executors, TimeUnit}
import scala.concurrent.{ExecutionContext, Promise, Future, Await}
import scala.concurrent.duration.Duration
import akka.http.scaladsl.ConnectionContext
import javax.net.ssl.{KeyManager, SSLContext, TrustManager}
import java.security.{SecureRandom, Security, Provider, KeyStore}
import com.homie.reverseproxy.includes._

object ReverseProxy extends App {

	// Load configurations and define application-wide variables
	val homieReverseProxyConfiguration = ConfigFactory.load("homie-http-core")
	val config: Config = homieReverseProxyConfiguration.withFallback(ConfigFactory.load())

	/**
	  * Environment variables for the application.
	  *
	  * @param required	Environment variables required to be defined. If omitted, they will throw `java.util.NoSuchElementException`.
	  * @param options	Optional environment variables. If omitted, there should be fallback/default values for these.
	  * @param version	Versioning Environment variables.
		* @param .release 	Major Project Version
		* @param .platform	Database, docker, e2e/unit tests...
		* @param .proxy 	Scala SSL-Terminating Reverse Proxy + AccessLogging
		* @param .api 		C# ASP.NET Web API
		* @param .fastapi 	Python FastAPI
		* @param .app 		Frontend (React or Flutter, maybe also a website, TBD)
		* @param .homie 	Full version (ex: 1.12.4080 = Release 1, Platform 12, Proxy 4, ...)
	  */
	lazy val env: Variables = Variables(
		version = AppVersion(
			release = Properties.envOrNone("RELEASE_V").get,
			platform = Properties.envOrNone("PLATFORM_V").get,
			proxy = Properties.envOrNone("PROXY_V").get,
			api = Properties.envOrNone("API_V").get,
			fastapi = Properties.envOrNone("FASTAPI_V").get,
			app = Properties.envOrNone("APP_V").get,
			homie = ( // Full version (ex: 1.12.4080 = Release 1, Platform 12, Proxy 4, ...)
				s"${Properties.envOrNone("RELEASE_V").get}.${Properties.envOrNone("PLATFORM_V").get}."
					+ Properties.envOrNone("PROXY_V").get
					+ Properties.envOrNone("API_V").get
					+ Properties.envOrNone("FASTAPI_V").get
					+ Properties.envOrNone("APP_V").get
			)
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

	// Create a promise to await the termination of the server
	/* val serverTerminationPromise = Promise[Unit]() */

	val hostname: String = env.required("HOST");
	val tcpPortForwarded: Int = env.required("PORT").toInt; // 8080 - Is what's exposed internally (docker cluster network)
	val tcpPort: Int = env.required("PORT_FORWARDED").toInt; // 80 - Is what's exposed on the Host PC
	val bindingFuture = Http().bindAndHandle(Routing.init, "0.0.0.0", tcpPort)

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

		val bindingHttpsFuture = Http().bindAndHandle(Routing.init, "0.0.0.0", sslPort, ConnectionContext.httpsServer(sslContext))
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

/** (Struct) Versioning of the application. Found in `.env` file (or environment variables) */
case class AppVersion (
	release: String, // Major Project Version
	platform: String, // Database, docker, e2e/unit tests...
	proxy: String, // Scala SSL-Terminating Reverse Proxy + AccessLogging
	api: String, // C# ASP.NET Web API
	fastapi: String, // Python FastAPI
	app: String, // Frontend (React or Flutter, maybe also a website, TBD)
	homie: String, // Full version (ex: 1.12.4080 = Release 1, Platform 12, Proxy 4, ...)
)

/** (Struct) Environment variables for the application. */
case class Variables (
	version: AppVersion,
	required: Map[String, String],
	options: Map[String, String],
)