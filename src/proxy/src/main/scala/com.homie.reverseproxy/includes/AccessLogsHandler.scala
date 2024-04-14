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
	def insert(accessLog: AccessLog, route: String = ""): Future[Int] = {
		
		val future = DbContext.db.run(DbContext.access_logs += accessLog)
		future.onComplete {
			case Success(_) => println(s"($route) Access log inserted.")
			case Failure(ex) => { println(s"($route) (Error) Error inserting access log: ${ex.getMessage}"); Future.failed(ex) }
		}

		return future
	}
}