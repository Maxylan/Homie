// (c) 2024 @Maxylan
package com.homie.reverseproxy.includes

import akka.stream.ActorMaterializer
import scala.concurrent.{ExecutionContext, Await, Promise, Future}
import scala.concurrent.duration.{Duration, DurationInt}
import akka.http.scaladsl.model.{HttpRequest, RemoteAddress}
import slick.dbio.{DBIO, DBIOAction, NoStream, Effect}
import slick.lifted.Query
import java.sql.Timestamp
import com.homie.reverseproxy.ReverseProxy
import models._

object Logger 
{
	import slick.jdbc.MySQLProfile.api._
	import slick.lifted.TableQuery

	implicit val executionContext: ExecutionContext = com.homie.reverseproxy.ReverseProxy.executionContext
	implicit val materializerContext: ActorMaterializer = com.homie.reverseproxy.ReverseProxy.materializer

	/**
	  * Create a new access log entry.
	  *
	  * @param request
	  * @param requestingIp
	  * @return
	  */
	def newAccessLog(request: HttpRequest, requestingIp: Option[RemoteAddress] = None): Future[AccessLog] = {

		val timestamp = new Timestamp(System.currentTimeMillis());
		val ip: String = if !requestingIp.isEmpty then requestingIp.get.value else request.headers.find(_.is("x-forwarded-for")).map(_.value().split(",").head).getOrElse(request.uri.authority.host.address /* Final fallback. */);

		val platformId: Option[String] = request.headers.find(_ is "x-requesting-platform").map(_.value());
		val userToken: Option[String] = request.headers.find(_ is "x-requesting-user").map(_.value());
		val log = AccessLog(
			None, // id
			platformId.asInstanceOf[Option[Int]], // platform_id
			None, // userToken, // token
			None, // username, // username
			timestamp,
			ReverseProxy.homieVersion,
			ip, // ip of caller
			request.method.value, // method
			request.uri.toString, // uri
			request.uri.path.toString, // path
			request.uri.rawQueryString.getOrElse(""), // parameters
			s"${request.uri.scheme}://${request.uri.authority.toString}${request.uri.path.toString}", // full_url
			Some(request.headers.mkString("\n")),
			Await.result(request.entity.toStrict(30.seconds).map(x => Option(x.data.utf8String)), 30.seconds), // body
			None, // responseMessage,
			503 // responseStatus,
		)

		return if userToken.isEmpty 
			then Future.successful(log)
			else UsersHandler.includeUserDetailsInLog(log, userToken.get)
	}
}