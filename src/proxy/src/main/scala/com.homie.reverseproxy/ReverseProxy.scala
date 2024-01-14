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

object ReverseProxy extends App {
	implicit val system: ActorSystem = ActorSystem("ReverseProxy")
	implicit val materializer: ActorMaterializer = ActorMaterializer()
	import system.dispatcher

	val route = {
		extractRequest { request =>
			pathPrefix("api") {
				path(".*".r) { path =>
					extractUri { uri =>
						val targetUri = uri.withAuthority("127.0.0.1", 10001)
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
						val targetUri = uri.withAuthority("127.0.0.1", 10000)
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
	val certificate, privateKey = if system.settings.config.getConfig("akka.http.server").getString("ssl-config.self-signed") == "true" 
		then (
			// Load your self-signed certificate, privateKey will be null.
			getClass.getClassLoader.getResourceAsStream("self-signed-cert.pem"), null
		) else (
			// Load your Let's Encrypt certificate and private key
			getClass.getClassLoader.getResourceAsStream("fullchain.pem"), getClass.getClassLoader.getResourceAsStream("privkey.pem")
		)
	
	// If privateKey != null then it's a a trusted certificate
	if (privateKey != null) {
		sslContext.init(
			// KeyManagers: Use KeyManagerFactory with a KeyStore containing your Let's Encrypt certificate and private key
			Array(new KeyManagerFactory("") {
				val keyStore: KeyStore = KeyStore.getInstance(KeyStore.getDefaultType)
				keyStore.load(null) // No need for a KeyStore since Let's Encrypt certificates are already in PEM format
				keyStore.setCertificateEntry("cert", CertificateFactory.getInstance("X.509").generateCertificate(certificate))
				keyStore.setKeyEntry("key", privateKey, "your-private-key-password".toCharArray, Array.empty)
				engineInit(keyStore, "your-private-key-password".toCharArray)
			}),
			// TrustManagers: Use TrustManagerFactory with null or empty trust managers (certificates are already trusted)
			null,
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
			// TrustManagers: Use TrustManagerFactory with null or empty trust managers (since it's self-signed)
			null,
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
