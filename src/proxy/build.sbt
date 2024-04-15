// (c) 2024 @Maxylan
name := "HomieReverseProxy"

scalaVersion := "3.3.1"
enablePlugins(JavaAppPackaging)

// Enable debugging options
// logLevel := Level.Debug
logLevel := Level.Warn

dockerBaseImage := "openjdk:8-jre-alpine"

mainClass in Compile := Some("com.homie.reverseproxy.ReverseProxy")

libraryDependencies ++= Seq(
  "mysql" % "mysql-connector-java" % "8.0.31",
  "com.typesafe" % "config" % "1.4.3",
  "com.typesafe.slick" %% "slick" % "3.5.0-M5",
  "org.slf4j" % "slf4j-nop" % "2.0.8",
  "com.typesafe.akka" %% "akka-http" % "10.6.0-M1",
  "com.typesafe.akka" %% "akka-http-spray-json" % "10.6.0-M1",
  "com.typesafe.akka" %% "akka-stream" % "2.9.0-M2"
)