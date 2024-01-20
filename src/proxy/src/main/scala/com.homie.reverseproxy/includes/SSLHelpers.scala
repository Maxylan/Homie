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

/**
 * I used documentation from docs.oracle.com to help understand JSSE API and TLS 1.3. Extremely helpful, check it out.
 * https://docs.oracle.com/javase/8/docs/technotes/guides/security/jsse/JSSERefGuide.html#Introduction
 */
object SSLHelpers {
	val config = ConfigFactory.load();
	
	val certificateConfig: String = if config.getString("ssl-config.self-signed") == "true" then "snakeoil-fullchain-file" else "certificate-file";
	val privateKeyConfig: String = if config.getString("ssl-config.self-signed") == "true" then "snakeoil-pk-file" else "pk-file";

	val certificateFile: InputStream = getClass.getClassLoader.getResourceAsStream(config.getString(s"ssl-config.${certificateConfig}"))
	val privateKeyFile: InputStream = getClass.getClassLoader.getResourceAsStream(config.getString(s"ssl-config.${privateKeyConfig}"))
	
	/**
	  * Creates and configures a KeyStore with a certificate and private key.
	  *
	  * @param	$type The type of KeyStore to create. Default is PKCS12.
	  * @return	KeyStore
	  */
	def getKeyStore($type: String = "PKCS12"): KeyStore = {
        val keyStore: KeyStore = KeyStore.getInstance($type);
        val certificate: Certificate = CertificateFactory.getInstance("X.509").generateCertificate(certificateFile);
        val privateKey: Array[Byte] = privateKeyFile.readAllBytes();
        
        keyStore.load(null);
        keyStore.setCertificateEntry("cert", certificate);
        keyStore.setKeyEntry("key", privateKey, Array.empty);

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
		keyManagerFactory.init($key_store, null /* config.getString("ssl-config.pk-password").toCharArray */);

		return keyManagerFactory.getKeyManagers();
	}


	def getTrustManagers($key_store: KeyStore, $instance: String = "SunX509"): Array[TrustManager] = {
		val trustManagerFactory = TrustManagerFactory.getInstance($instance);
		trustManagerFactory.init($key_store);
		return trustManagerFactory.getTrustManagers();
	}
}