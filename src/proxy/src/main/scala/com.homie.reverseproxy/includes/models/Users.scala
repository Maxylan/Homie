// (c) 2024 @Maxylan
package com.homie.reverseproxy.includes.models

import java.sql.Timestamp
import slick.lifted.TableQuery
import slick.jdbc.MySQLProfile.api._
import com.homie.reverseproxy.includes.models.Platforms
import slick.sql.SqlProfile.ColumnOption.NotNull

case class User(
    id: Option[Int] = None,
    token: String,
    platformId: Int,
    username: String,
    firstName: Option[String] = None,
    lastName: Option[String] = None,
    group: String,
    expires: Option[Timestamp] = None,
    created: Timestamp,
    changed: Timestamp
)

class Users(tag: Tag) extends Table[User](tag, "users") {
    def id: Rep[Option[Int]] = column[Option[Int]]("id", O.AutoInc, O.PrimaryKey)
    def token: Rep[String] = column[String]("token", O.Length(31), O.Unique)
    def platformId: Rep[Option[Int]] = column[Option[Int]]("platform_id", NotNull)
    def username: Rep[String] = column[String]("username", O.Length(63))
    def firstName: Rep[Option[String]] = column[Option[String]]("first_name", O.Length(63))
    def lastName: Rep[Option[String]] = column[Option[String]]("last_name", O.Length(63))
    def group: Rep[String] = column[String]("group")
    def expires: Rep[Option[Timestamp]] = column[Option[Timestamp]]("expires")
    def created: Rep[Timestamp] = column[Timestamp]("created")
    def changed: Rep[Timestamp] = column[Timestamp]("changed")

    def platformFK = foreignKey("users_ibfk_1", platformId, TableQuery[Platforms])(_.id, onUpdate=ForeignKeyAction.NoAction, onDelete=ForeignKeyAction.Cascade)

    override def * = (
        id,
        token,
        platformId,
        username,
        firstName,
        lastName,
        group,
        expires,
        created,
        changed
    ).mapTo[User]
}