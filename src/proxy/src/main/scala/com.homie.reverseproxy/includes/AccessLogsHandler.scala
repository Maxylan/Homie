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
	def insert(accessLog: AccessLog): Future[Option[Int]] = {
		
		val future: Future[Option[Int]] = DbContext.db.run((DbContext.access_logs returning DbContext.access_logs.map(_.id)) += accessLog)
		future.onComplete {
			case Success(id) => println(s"(Info) Access log inserted. ID: ${id.get}")
			case Failure(ex) => { println(s"(Error) Error inserting access log: ${ex.getMessage}"); Future.failed(ex) }
		}

		return future
	}
}