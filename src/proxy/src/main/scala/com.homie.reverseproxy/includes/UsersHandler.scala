// (c) 2024 @Maxylan
package com.homie.reverseproxy.includes

import com.homie.reverseproxy.ReverseProxy
import akka.stream.ActorMaterializer
import scala.util.{Success, Failure}
import scala.concurrent.{ExecutionContext, Await, Promise, Future}
import akka.http.scaladsl.model.{HttpRequest, RemoteAddress}
import slick.dbio.{DBIO, DBIOAction, NoStream, Effect}
import slick.lifted.Query
import java.sql.Timestamp
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
	def includeUserDetailsInLog(accessLog: AccessLog, userToken: Option[String], route: String = ""): Future[AccessLog] = {

		if userToken.isEmpty then {
			println(s"($route) (Info) No user token provided. Skipping user details inclusion.")
			return Future.successful(accessLog)
		}
		
		val userQuery = DbContext.users.filter(_.token === userToken).take(1).result.headOption
		val accessLogWithUserDetails: Future[AccessLog] = DbContext.executeAsync(userQuery).map { user =>
			accessLog.copy(
				userToken = Some(user.get.token),
				username = Some(user.get.username)
			)
		}

		accessLogWithUserDetails.onComplete {
			case Success(_) => println(s"($route) (Info) Access log enhanced with user details.")
			case Failure(ex) => { println(s"($route) (Error) Error enhancing access log with user details: ${ex.getMessage}"); Future.failed(ex) }
		}

		accessLogWithUserDetails;
	}
}