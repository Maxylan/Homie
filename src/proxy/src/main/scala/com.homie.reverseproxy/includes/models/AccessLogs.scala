// (c) 2024 @Maxylan
package com.homie.reverseproxy.includes.models

import java.sql.Timestamp
import slick.jdbc.MySQLProfile.api._

object AccessLog {
    /**
      * Alternative, shorter constructor for creating a new `AccessLog` object.
      * Optional properties are moved to a `Map` object.
      *
      * @param timestamp
      * @param version
      * @param ip
      * @param method
      * @param uri
      * @param fullUrl
      * @param rest     Optional properties.
      * @return         new `AccessLog` structure.
      */
    def < (
        timestamp: Timestamp,
        version: String,
        ip: String,
        method: String,
        uri: String,
        fullUrl: String,
        rest: Map[String, Option[String]]
    ): AccessLog = AccessLog(
        rest.getOrElse("id", None) map (_.toInt),
        rest.getOrElse("platformId", None) map (_.toInt),
        rest.getOrElse("userToken", None) map (_.toString),
        rest.getOrElse("username", None) map (_.toString),
        timestamp,
        version,
        ip,
        method,
        uri,
        rest.getOrElse("path", None) map (_.toString),
        rest.getOrElse("parameters", None) map (_.toString),
        fullUrl,
        rest.getOrElse("headers", None) map (_.toString),
        rest.getOrElse("body", None) map (_.toString),
        rest.getOrElse("responseMessage", None) map (_.toString),
        rest.getOrElse("responseStatus", None) map (_.toInt)
    )
}
case class AccessLog(
    id: Option[Int] = None,
    platformId: Option[Int] = None,
    userToken: Option[String] = None,
    username: Option[String] = None,
    timestamp: Timestamp,
    version: String,
    ip: String,
    method: String,
    uri: String,
    path: Option[String] = None,
    parameters: Option[String] = None,
    fullUrl: String,
    headers: Option[String] = None,
    body: Option[String] = None,
    responseMessage: Option[String] = None,
    responseStatus: Option[Int] = None
)

class AccessLogs(tag: Tag) extends Table[AccessLog](tag, "g_access_logs") {
    def id: Rep[Option[Int]] = column[Option[Int]]("id", O.AutoInc, O.PrimaryKey)
    def platformId: Rep[Option[Int]] = column[Option[Int]]("platform_id")
    def userToken: Rep[Option[String]] = column[Option[String]]("user_token", O.Length(31))
    def username: Rep[Option[String]] = column[Option[String]]("username", O.Length(63))
    def timestamp: Rep[Timestamp] = column[Timestamp]("timestamp")
    def version: Rep[String] = column[String]("version", O.Length(31))
    def ip: Rep[String] = column[String]("ip", O.Length(63))
    def method: Rep[String] = column[String]("method") // ENUM('GET', 'PUT', 'POST', 'DELETE', 'OPTIONS', 'HEAD', 'PATCH', 'UNKNOWN') NOT NULL DEFAULT 'UNKNOWN'
    def uri: Rep[String] = column[String]("uri", O.Length(255))
    def path: Rep[Option[String]] = column[Option[String]]("path", O.Length(255))
    def parameters: Rep[Option[String]] = column[Option[String]]("parameters", O.Length(511))
    def fullUrl: Rep[String] = column[String]("full_url", O.Length(1023))
    def headers: Rep[Option[String]] = column[Option[String]]("headers", O.SqlType("TEXT"))
    def body: Rep[Option[String]] = column[Option[String]]("body", O.SqlType("TEXT"))
    def responseMessage: Rep[Option[String]] = column[Option[String]]("response_message", O.Length(1023))
    def responseStatus: Rep[Option[Int]] = column[Option[Int]]("response_status", O.Default(Option(503)))

    def platformFK = foreignKey("access_logs_ibfk_1", platformId, TableQuery[Platforms])(_.id, onUpdate=ForeignKeyAction.NoAction, onDelete=ForeignKeyAction.SetNull)
    def userFK = foreignKey("access_logs_ibfk_2", userToken, TableQuery[Users])(_.token.?, onUpdate=ForeignKeyAction.NoAction, onDelete=ForeignKeyAction.SetNull)

    override def * = (
        id,
        platformId,
        userToken,
        username,
        timestamp,
        version,
        ip,
        method,
        uri,
        path,
        parameters,
        fullUrl,
        headers,
        body,
        responseMessage,
        responseStatus
    ).mapTo[AccessLog]
}