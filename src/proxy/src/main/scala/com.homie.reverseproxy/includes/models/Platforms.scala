// (c) 2024 @Maxylan
package com.homie.reverseproxy.includes.models

import java.sql.Timestamp
import slick.jdbc.MySQLProfile.api._

case class Platform(
    id: Option[Int] = None,
    name: String,
    guestCode: String,
    memberCode: String,
    masterPassword: String,
    resetToken: String,
    created: Timestamp
)

class Platforms(tag: Tag) extends Table[Platform](tag, "g_platforms") {
    def id: Rep[Option[Int]] = column[Option[Int]]("id", O.AutoInc, O.PrimaryKey)
    def name: Rep[String] = column[String]("name", O.Length(63))
    def guestCode: Rep[String] = column[String]("guest_code", O.Length(31), O.Unique)
    def memberCode: Rep[String] = column[String]("member_code", O.Length(31), O.Unique)
    def masterPassword: Rep[String] = column[String]("master_pswd", O.Length(63))
    def resetToken: Rep[String] = column[String]("reset_token", O.Length(63))
    def created: Rep[Timestamp] = column[Timestamp]("created")

    override def * = (
        id,
        name,
        guestCode,
        memberCode,
        masterPassword,
        resetToken,
        created
    ).mapTo[Platform]
}