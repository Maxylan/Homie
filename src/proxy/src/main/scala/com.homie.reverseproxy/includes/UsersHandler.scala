// (c) 2024 @Maxylan
package com.homie.reverseproxy.includes

import akka.stream.ActorMaterializer
import scala.concurrent.{ExecutionContext, Await, Promise, Future}
import akka.http.scaladsl.model.{HttpRequest, RemoteAddress}
import slick.dbio.{DBIO, DBIOAction, NoStream, Effect}
import slick.lifted.Query
import java.sql.Timestamp
import com.homie.reverseproxy.ReverseProxy
import models._

object UsersHandler 
{
	import slick.jdbc.MySQLProfile.api._
	import slick.lifted.TableQuery

	implicit val executionContext: ExecutionContext = com.homie.reverseproxy.ReverseProxy.executionContext
	implicit val materializerContext: ActorMaterializer = com.homie.reverseproxy.ReverseProxy.materializer
	
	/**
	  * Include user details in the access log. 
	  * (Get user details from the DB and include in the access log.)
	  *
	  * @param accessLog
	  * @param userToken
	  * @return
	  */
	def includeUserDetailsInLog(accessLog: AccessLog, userToken: String, route: String = ""): Future[AccessLog] = {
		println(s"(Debug) ($route) include*UserDetails*InLog");

		val userQuery = DbContext.users.filter(_.token === userToken).take(1).result.headOption
		val accessLogWithUserDetails: Future[AccessLog] = DbContext.executeAsync(userQuery).map { user =>
			accessLog.copy(
				userToken = Some(user.get.token),
				username = Some(user.get.username)
			)
		}

		accessLogWithUserDetails.recover({ case ex =>
			println(s"Err: Database query against `users` failed. Token: \"$userToken\" \n${ex.getMessage} ${ex.getClass.toString()}")
			Promise.failed(ex)
		})

		accessLogWithUserDetails;
	}
}