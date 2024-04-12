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
import scala.util.Success
import scala.util.Failure

object AccessLogsHandler 
{
	import slick.jdbc.MySQLProfile.api._
	import slick.lifted.TableQuery

	implicit val executionContext: ExecutionContext = com.homie.reverseproxy.ReverseProxy.executionContext
	implicit val materializerContext: ActorMaterializer = com.homie.reverseproxy.ReverseProxy.materializer
	
	/**
	  * Insert a new access log entry.
	  *
	  * @param accessLog
	  * @return
	  */
	def insert(accessLog: AccessLog): Future[Int] = {
		
		return DbContext.executeAsync(DbContext.access_logs += accessLog) /*
		val future = DbContext.db.run(DbContext.access_logs += accessLog)
		future.onComplete {
			case Success(_) => println("Access log inserted.")
			case Failure(e) => println(s"Error inserting access log: ${e.getMessage}")
		}

		return future */
	}
}