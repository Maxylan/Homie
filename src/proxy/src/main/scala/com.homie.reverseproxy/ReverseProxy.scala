// (c) 2024 @Maxylan
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

object ReverseProxy extends App {
	implicit val system: ActorSystem = ActorSystem("ReverseProxy")
	implicit val materializer: ActorMaterializer = ActorMaterializer()
	import system.dispatcher
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

	val bindingFuture = Http().bindAndHandle(route, "0.0.0.0", 80)
	println(s"Server online at http://0.0.0.0:80/")

	val sslContext= SSLContext.getInstance("TLS"); 
	val (certificate, privateKey) = if config.getString("ssl-config.self-signed") == "true" 
		then (
			// Load your self-signed certificate, privateKey will be null.
			getClass.getClassLoader.getResourceAsStream(config.getString("ssl-config.snakeoil-fullchain-file")), 
			getClass.getClassLoader.getResourceAsStream(config.getString("ssl-config.snakeoil-pk-file"))
		) else (
			// Load your Let's Encrypt certificate and private key
			getClass.getClassLoader.getResourceAsStream(config.getString("ssl-config.fullchain-file")), 
			getClass.getClassLoader.getResourceAsStream(config.getString("ssl-config.pk-file"))
		)
	
	println(Security.getProviders()) 

	// If privateKey != null then it's a a trusted certificate
	if (privateKey != null) {
		val keyStore: KeyStore = KeyStore.getInstance("PKCS12");
		// keyStore.load(certificate, config.getString("ssl-config.pk-password").toCharArray)
		keyStore.load(null)
		keyStore.setCertificateEntry("cert", CertificateFactory.getInstance("X.509").generateCertificate(certificate))
		keyStore.setKeyEntry("key", privateKey.asInstanceOf[java.security.Key], null /* config.getString("ssl-config.pk-password").toCharArray */, Array.empty)
		
		// Create a KeyManagerFactory with a KeyStore containing your Let's Encrypt certificate and private key
		var keyManagerFactory = KeyManagerFactory.getInstance("SunX509");
		keyManagerFactory.init(keyStore, null /* config.getString("ssl-config.pk-password").toCharArray */)

		sslContext.init(
			keyManagerFactory.getKeyManagers,
			null, // (certificates are already trusted)
			new SecureRandom
		)
	}
	else {
		sslContext.init(
			// KeyManagers: Use KeyManagerFactory with a KeyStore containing your self-signed certificate
			Array(new X509TrustManager {
				override def checkClientTrusted(chain: Array[java.security.cert.X509Certificate], authType: String): Unit = {}
				override def checkServerTrusted(chain: Array[java.security.cert.X509Certificate], authType: String): Unit = {}
				override def getAcceptedIssuers: Array[java.security.cert.X509Certificate] = Array.empty
			}).asInstanceOf[Array[KeyManager]],
			null, // Use TrustManagerFactory with null or empty trust managers (since it's self-signed)
			new SecureRandom
		)
	}

	val bindingHttpsFuture = Http().bindAndHandle(route, "0.0.0.0", 443, ConnectionContext.httpsServer(sslContext))
	println(s"Server online at https://0.0.0.0:443/")

	sys.addShutdownHook {
		bindingFuture
			.flatMap(_.unbind())
			.onComplete(_ => system.terminate())
	}
}
