package com.homie.reverseproxy.includes

import slick.jdbc.MySQLProfile.api._
import scala.concurrent.ExecutionContext.Implicits.global
import scala.util.Properties
import java.util.concurrent.Executors
import scala.concurrent.{ExecutionContext, Promise, Future, Await}
import scala.concurrent.duration.Duration
import java.sql.Timestamp
import slick.lifted.ProvenShape
import slick.dbio.{DBIO, DBIOAction}

object DbContext {
	// val executor = Executors.newFixedThreadPool(4)
	// implicit val executionContext: ExecutionContext = ExecutionContext.fromExecutorService(executor)

	private def acquireDatabaseContext(): Database = {
		val dbHost = Properties.envOrNone("DB_HOST"/*, "homie.db"*/)
		val dbPort = Properties.envOrNone("DB_PORT"/*, "10002"*/)
		val dbUser = Properties.envOrNone("DB_PROXY_USER"/*, "proxy"*/)
		val dbPassword = Properties.envOrNone("DB_PROXY_PASSWORD"/*, "password"*/)
		return {
			require(!dbHost.isEmpty, s"Failed to load \"DB_HOST\" from environment.")
			require(!dbPort.isEmpty, s"Failed to load \"DB_PORT\" from environment.")
			require(!dbUser.isEmpty, s"Failed to load \"DB_PROXY_USER\" from environment.")
			require(!dbPassword.isEmpty, s"Failed to load \"DB_PROXY_PASSWORD\" from environment.")
			Database.forURL(url = s"jdbc:mysql://${dbHost.get}:${dbPort.get}/HomieDB", user = dbUser.get, password = dbPassword.get, driver = "com.mysql.cj.jdbc.Driver", executor = AsyncExecutor.default(), keepAliveConnection = true)
		}
	}

	lazy val db = acquireDatabaseContext()
	lazy val access_logs = TableQuery[AccessLogsTable]

	/**
	  * My attempt at a query function which continously opens and closes database 
	  * connections without having to worry about it.
	  *
	  * @param action
	  * @return
	  */
	def query[R](action: DBIO[R]): Future[R] = {
		lazy val db = acquireDatabaseContext()
		try return db.run(action) finally db.close();
	}

	/**
     * Executes a DBIO action asynchronously and returns a Future.
     *
     * @param action The DBIO action to execute.
     * @tparam R The result type of the DBIO action.
     * @return A Future containing the result of the DBIO action.
     */
	def executeAsync[R](action: DBIO[R]): Future[R] = {
		db.run(action)
	}
}

case class AccessLogs(
	id: Option[Long],
	platformId: Option[Int],
	uid: Option[Int],
	timestamp: Timestamp,
	ip: String,
	method: String,
	uri: String,
	path: String,
	parameters: String,
	fullUrl: String,
	headers: String,
	body: Option[String],
	response: Option[String],
	responseStatus: Option[Int]
)

// (Option[Long], Option[Int], Option[Int], Timestamp, String, String, String, String, String, String, String, Option[String], Option[String], Option[Int])
class AccessLogsTable(tag: Tag) extends Table[AccessLogs](tag, "access_logs") {
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

	// ProvenShape[AccessLogs]
	override def * = (
		id, 
		platformId, 
		uid, 
		timestamp, 
		ip, 
		method, 
		uri, 
		path, 
		parameters, 
		fullUrl, 
		headers, 
		body, 
		response, 
		responseStatus
	) <> (AccessLogs.apply, AccessLogs.unapply) 
}
