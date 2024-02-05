// (c) 2024 @Maxylan
package com.homie.reverseproxy.includes

import scala.util.Properties
import akka.actor.ActorSystem
import akka.http.scaladsl.Http
import akka.http.scaladsl.model.{HttpRequest, HttpResponse, Uri}
import akka.stream.ActorMaterializer
import akka.http.scaladsl.server.Directives._
import scala.concurrent.Future
import scala.util.{Failure, Success}
import akka.http.scaladsl.ConnectionContext
import javax.net.ssl.{KeyManager, KeyManagerFactory, SSLContext, TrustManagerFactory, X509TrustManager}
import javax.net.ssl.KeyManagerFactorySpi
import java.security.KeyStore
import java.security.Provider
import java.security.SecureRandom
import java.security.cert.CertificateFactory
import java.security.cert.Certificate
import java.security.Security
import com.typesafe.config.ConfigFactory
import javax.net.ssl.TrustManager
import java.security.PrivateKey
import java.io.InputStream
import java.security.spec.PKCS8EncodedKeySpec
import java.security.KeyFactory

/**
 * I used documentation from docs.oracle.com to help understand JSSE API and TLS 1.3. Extremely helpful, check it out.
 * https://docs.oracle.com/javase/8/docs/technotes/guides/security/jsse/JSSERefGuide.html#Introduction
 */
object SSLHelpers {
	val selfSigned = Properties.envOrElse("SSL_SELF_SIGNED", "true").toBoolean;

	/**
	  * Loads, or creates then configures, a KeyStore.
	  *
	  * @param	$type The type of KeyStore to create. Default is PKCS12.
	  * @return	KeyStore
	  */
	def getKeyStore($type: String = "PKCS12", loadKeyStore: Boolean = true): KeyStore = {
		val keyStore: KeyStore = KeyStore.getInstance($type)
		
		if (loadKeyStore) {
			val keystoreFilePath = Properties.envOrElse("SSL_KEYSTORE", null)
			require(keystoreFilePath != null, s"Failed to load \"SSL_KEYSTORE\" from environment.")
			println(s"(Debug) SSL_KEYSTORE: ${keystoreFilePath}")

			val keyStoreFile: InputStream = getClass.getClassLoader.getResourceAsStream(keystoreFilePath)
			require(keyStoreFile != null, s"Could not find keyStore file: ${keystoreFilePath}")

			keyStore.load(keyStoreFile, Properties.envOrElse("SSL_KEYSTORE_PASSWORD", "password").toCharArray)
		}
		else {
			// Load the certificate
			val certificateConfig: String = if selfSigned 
				then "SSL_SNAKEOIL_FULLCHAIN_CERTIFICATE" 
				else "SSL_CERTIFICATE";
			val certificateFilePath = Properties.envOrElse(certificateConfig, null)
			require(certificateFilePath != null, s"Failed to load \"${certificateConfig}\" from environment.")
			println(s"(Debug) ${certificateConfig}: ${certificateFilePath}")

			val certificateFile: InputStream = getClass.getClassLoader.getResourceAsStream(certificateFilePath)
			require(certificateFile != null, s"Could not find certificate file: ${certificateFilePath}")

			// Load the private key
			val privateKeyConfig: String = if selfSigned 
				then "SSL_SNAKEOIL_PRIVATE_KEY" 
				else "SSL_PRIVATE_KEY";
			val privateKeyFilePath = Properties.envOrElse(privateKeyConfig, null)
			require(certificateFilePath != null, s"Failed to load \"${privateKeyConfig}\" from environment.")
			println(s"(Debug) ${privateKeyConfig}: ${privateKeyFilePath}")

			val privateKeyFile: InputStream = getClass.getClassLoader.getResourceAsStream(privateKeyFilePath)
			require(privateKeyFile != null, s"Could not find privateKey file: ${privateKeyFilePath}")

			val certificate: Certificate = CertificateFactory.getInstance("X.509").generateCertificate(certificateFile)
			val privateKeyBytes: Array[Byte] = privateKeyFile.readAllBytes()

			if ($type == "PKCS12") {
				// For PKCS12, use the key and certificate directly
				keyStore.load(null)
				keyStore.setCertificateEntry("cert", certificate)
				keyStore.setKeyEntry("key", privateKeyBytes, Array(certificate))
			} else {
				// For other types, you may need to use a PKCS8EncodedKeySpec
				val keySpec = new PKCS8EncodedKeySpec(privateKeyBytes)
				val privateKey = KeyFactory.getInstance("RSA").generatePrivate(keySpec)
				
				keyStore.load(null)
				keyStore.setCertificateEntry("cert", certificate)
				keyStore.setKeyEntry("key", privateKey, Properties.envOrElse("SSL_PRIVATE_KEY_PASSWORD", "password").toCharArray, Array.empty)
			}
		}

		return keyStore;
	}

	/**
	  * Creates and initializes a KeyManagerFactory using a keystore and provided "instance" (default is SunX509).
	  * This in turn returns an array of KeyManagers.
	  *
	  * @param	$key_store The KeyStore to use.
	  * @param	$instance The instance to use. Default is SunX509.
	  * @return	Array[KeyManager]
	  */
	def getKeyManagers($key_store: KeyStore, $instance: String = "SunX509"): Array[KeyManager] = {
		val keyManagerFactory = KeyManagerFactory.getInstance($instance);
		keyManagerFactory.init($key_store, Properties.envOrElse("SSL_KEYSTORE_PASSWORD", "password").toCharArray);

		return keyManagerFactory.getKeyManagers();
	}


	def getTrustManagers($key_store: KeyStore, $instance: String = "SunX509"): Array[TrustManager] = {
		val trustManagerFactory = TrustManagerFactory.getInstance($instance);
		trustManagerFactory.init($key_store);
		return trustManagerFactory.getTrustManagers();
	}
}