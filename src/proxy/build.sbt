// (c) 2024 @Maxylan
name := "HomieReverseProxy"

version := "1.0"

scalaVersion := "3.3.1"

// Enable debugging options
logLevel := Level.Debug

libraryDependencies ++= Seq(
  "com.typesafe.slick" %% "slick" % "3.5.0-M5",
  "org.slf4j" % "slf4j-nop" % "2.0.8",
  "com.typesafe.akka" %% "akka-http" % "10.6.0-M1",
  "com.typesafe.akka" %% "akka-http-spray-json" % "10.6.0-M1",
  "com.typesafe.akka" %% "akka-stream" % "2.9.0-M2"
)