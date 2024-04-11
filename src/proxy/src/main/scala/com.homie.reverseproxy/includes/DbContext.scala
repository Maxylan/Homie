// (c) 2024 @Maxylan
package com.homie.reverseproxy.includes

import slick.jdbc.MySQLProfile.api._
import java.util.concurrent.Executors
import scala.concurrent.{ExecutionContext, Promise, Future, Await}
import scala.concurrent.duration.Duration
import scala.util.Properties
import java.sql.Timestamp
import slick.lifted.ProvenShape
import slick.dbio.{DBIO, DBIOAction}
import models._

object DbContext {
	implicit val executionContext: ExecutionContext = com.homie.reverseproxy.ReverseProxy.executionContext

	private def acquireDatabaseContext(): Database = {
		val dbHost = Properties.envOrNone("DB_HOST"/*, "homie.db"*/)
		val dbPort = Properties.envOrNone("DB_PORT"/*, "10003"*/)
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
	lazy val access_logs = TableQuery[AccessLogs]
	lazy val platforms = TableQuery[Platforms]
	lazy val users = TableQuery[Users]

	/**
	  * My attempt at a query function which continously opens and closes database 
	  * connections without having to worry about it.
	  *
	  * @param action
	  * @return
	  */
	def query[R](action: DBIO[R]): Future[R] = {
		lazy val db = acquireDatabaseContext()
		try return db.run(action) finally Await.result(db.shutdown, Duration(30, "s"));
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