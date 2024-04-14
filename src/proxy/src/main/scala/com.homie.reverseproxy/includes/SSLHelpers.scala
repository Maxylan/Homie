// (c) 2024 @Maxylan
package com.homie.reverseproxy.includes

import com.homie.reverseproxy.ReverseProxy
import scala.concurrent.Future
import scala.util.{Failure, Success}
import akka.http.scaladsl.ConnectionContext
import javax.net.ssl.{KeyManager, KeyManagerFactory, SSLContext, TrustManagerFactory, X509TrustManager}
import javax.net.ssl.KeyManagerFactorySpi
import java.security.{Provider, SecureRandom, Security, PrivateKey, KeyStore, KeyFactory}
import java.security.cert.{CertificateFactory, Certificate}
import com.typesafe.config.ConfigFactory
import javax.net.ssl.TrustManager
import java.security.spec.PKCS8EncodedKeySpec
import java.io.InputStream

/**
 * I used documentation from docs.oracle.com to help understand JSSE API and TLS 1.3. Extremely helpful, check it out.
 * https://docs.oracle.com/javase/8/docs/technotes/guides/security/jsse/JSSERefGuide.html#Introduction
 */
object SSLHelpers {
	val selfSigned = ReverseProxy.env.options("SSL_SELF_SIGNED").toBoolean;

	/**
	  * Loads, or creates then configures, a KeyStore.
	  *
	  * @param	$type The type of KeyStore to create. Default is PKCS12.
	  * @return	`KeyStore`
	  */
	def getKeyStore($type: String = "PKCS12", loadKeyStore: Boolean = true): KeyStore = {
		val keyStore: KeyStore = KeyStore.getInstance($type)
		
		if (loadKeyStore) {
			val keystoreFilePath = ReverseProxy.env.options("SSL_KEYSTORE")

			require(!keystoreFilePath.isEmpty, "Failed to load \"SSL_KEYSTORE\" from environment.")
			println(s"(Debug) SSL_KEYSTORE: ${keystoreFilePath}")

			val keyStoreFile: InputStream = getClass.getClassLoader.getResourceAsStream(keystoreFilePath)
			require(keyStoreFile != null, s"Could not find keyStore file: ${keystoreFilePath}")

			keyStore.load(keyStoreFile, ReverseProxy.env.options("SSL_KEYSTORE_PASSWORD").toCharArray)
		}
		else {
			// Load the certificate
			val certificateConfig: String = if selfSigned 
				then "SSL_SNAKEOIL_FULLCHAIN_CERTIFICATE" 
				else "SSL_CERTIFICATE";
			val certificateFilePath = ReverseProxy.env.options(certificateConfig)

			require(!certificateFilePath.isEmpty, s"Failed to load \"${certificateConfig}\" from environment.")
			println(s"(Debug) ${certificateConfig}: ${certificateFilePath}")

			val certificateFile: InputStream = getClass.getClassLoader.getResourceAsStream(certificateFilePath)
			require(certificateFile != null, s"Could not find certificate file: ${certificateFilePath}")

			// Load the private key
			val privateKeyConfig: String = if selfSigned 
				then "SSL_SNAKEOIL_PRIVATE_KEY" 
				else "SSL_PRIVATE_KEY";
			val privateKeyFilePath = ReverseProxy.env.options(privateKeyConfig)

			require(!certificateFilePath.isEmpty, s"Failed to load \"${privateKeyConfig}\" from environment.")
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
				keyStore.setKeyEntry("key", privateKey, ReverseProxy.env.options("SSL_PRIVATE_KEY_PASSWORD").toCharArray, Array.empty)
			}
		}

		return keyStore;
	}

	/**
	  * Creates and initializes a KeyManagerFactory using a keystore and provided "instance" (default is SunX509).
	  * This in turn returns an array of KeyManagers.
	  *
	  * @param	`$key_store` - The KeyStore to use.
	  * @param	`$instance` - The instance to use. Default is SunX509.
	  * @return	`Array[KeyManager]`
	  */
	def getKeyManagers($key_store: KeyStore, $instance: String = "SunX509"): Array[KeyManager] = {
		val keyManagerFactory = KeyManagerFactory.getInstance($instance);
		keyManagerFactory.init($key_store, ReverseProxy.env.options("SSL_KEYSTORE_PASSWORD").toCharArray);

		return keyManagerFactory.getKeyManagers();
	}

	/**
	  * Creates and initializes a TrustManagerFactory using a keystore and provided "instance" (default is SunX509).
	  * This in turn returns an array of TrustManagers.
	  *
	  * @param 	`$key_store` - The KeyStore to use.
	  * @param 	`$instance` - The instance to use. Default is SunX509.
	  * @return	`Array[TrustManager]`
	  */
	def getTrustManagers($key_store: KeyStore, $instance: String = "SunX509"): Array[TrustManager] = {
		val trustManagerFactory = TrustManagerFactory.getInstance($instance);
		trustManagerFactory.init($key_store);
		return trustManagerFactory.getTrustManagers();
	}
}