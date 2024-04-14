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
import com.homie.reverseproxy.ReverseProxy
import models._

object DbContext {
	implicit val executionContext: ExecutionContext = ReverseProxy.executionContext

	/**
	  * Parmaters required to construct the database connection string.
	  */
	private lazy val Connection = Map[String, String] (
		"Host" -> ReverseProxy.env.required("DB_HOST"),
		"Port" -> ReverseProxy.env.required("DB_PORT"),
		"User" -> ReverseProxy.env.required("DB_PROXY_USER"),
		"Password" -> ReverseProxy.env.required("DB_PROXY_PASSWORD")
	)

	/**
	  * Acquire a database context.
	  *
	  * @return `MySQLProfile.api.Database`
	  */
	private def acquireDatabaseContext(): Database = {
		require(!Connection("Host").isEmpty, s"Failed to load \"DB_HOST\" from environment.")
		require(!Connection("Port").isEmpty, s"Failed to load \"DB_PORT\" from environment.")
		require(!Connection("User").isEmpty, s"Failed to load \"DB_PROXY_USER\" from environment.")
		require(!Connection("Password").isEmpty, s"Failed to load \"DB_PROXY_PASSWORD\" from environment.")

		Database.forURL(url = s"jdbc:mysql://${Connection("Host")}:${Connection("Port")}/HomieDB", user = Connection("User"), password = Connection("Password"), driver = "com.mysql.cj.jdbc.Driver", executor = AsyncExecutor.default(), keepAliveConnection = true)
	}

	lazy val db: Database = acquireDatabaseContext()
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
		// lazy val db = acquireDatabaseContext()
		// return db.run(action);
		// try return db.run(action) finally Await.result(db.shutdown, Duration(30, "s"));
		db.run(action);
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