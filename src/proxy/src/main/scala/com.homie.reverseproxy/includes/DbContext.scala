package com.homie.reverseproxy.includes

import slick.jdbc.MySQLProfile.api._
import scala.concurrent.ExecutionContext.Implicits.global
import scala.util.Properties
import scala.concurrent.Future
import scala.concurrent.Await
import scala.concurrent.duration.Duration
import java.sql.Timestamp
import slick.lifted.ProvenShape
import slick.dbio.{DBIO, DBIOAction}

object DbContext {
	private def acquireDatabaseContext(): Database = {
		val dbHost = Properties.envOrNone("DB_HOST"/*, "homie.db"*/)
		val dbPort = Properties.envOrNone("DB_PORT"/*, "10002"*/)
		val dbUser = Properties.envOrNone("DB_PROXY_USER"/*, "proxy"*/)
		val dbPassword = Properties.envOrNone("DB_PROXY_PASSWORD"/*, "password"*/)
		return {
			require(dbHost != null, s"Failed to load \"DB_HOST\" from environment.")
			require(dbPort != null, s"Failed to load \"DB_PORT\" from environment.")
			require(dbUser != null, s"Failed to load \"DB_PROXY_USER\" from environment.")
			require(dbPassword != null, s"Failed to load \"DB_PROXY_PASSWORD\" from environment.")
			Database.forURL(url = s"jdbc:mysql://${dbHost.get}:${dbPort.get}", user = dbUser.get, password = dbPassword.get, driver = "com.mysql.cj.jdbc.Driver")
		}
	}

	val access_logs = TableQuery[AccessLogTable]

	def query[R](action: DBIO[R]): Future[R] = {
		lazy val db = acquireDatabaseContext()
		try return db.run(action) finally db.close();
	}
}

class AccessLogTable(tag: Tag) extends Table[(Option[Long], Option[Int], Option[Int], Timestamp, String, String, String, String, String, String, String, Option[String], Option[String], Option[Int])](tag, "access_logs") {
	def id: Rep[Option[Long]] = column[Option[Long]]("id", O.AutoInc, O.PrimaryKey)
	def platformId: Rep[Option[Int]] = column[Option[Int]]("platform_id")
	def uid: Rep[Option[Int]] = column[Option[Int]]("uid")
	def timestamp: Rep[Timestamp] = column[Timestamp]("timestamp")
	def ip: Rep[String] = column[String]("ip", O.Length(63))
	def method: Rep[String] = column[String]("method")
	def uri: Rep[String] = column[String]("uri", O.Length(127))
	def path: Rep[String] = column[String]("path", O.Length(255))
	def parameters: Rep[String] = column[String]("parameters", O.Length(255))
	def fullUrl: Rep[String] = column[String]("full_url", O.Length(511))
	def headers: Rep[String] = column[String]("headers", O.Length(1023))
	def body: Rep[Option[String]] = column[Option[String]]("body", O.SqlType("TEXT"))
	def response: Rep[Option[String]] = column[Option[String]]("response", O.SqlType("TEXT"))
	def responseStatus: Rep[Option[Int]] = column[Option[Int]]("response_status")

	def * : ProvenShape[(Option[Long], Option[Int], Option[Int], Timestamp, String, String, String, String, String, String, String, Option[String], Option[String], Option[Int])] =
		(id, platformId, uid, timestamp, ip, method, uri, path, parameters, fullUrl, headers, body, response, responseStatus)
}
