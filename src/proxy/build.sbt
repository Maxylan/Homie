// (c) 2024 @Maxylan
name := "HomieReverseProxy"

version := "1.0"

scalaVersion := "3.3.1"

// Enable debugging options
logLevel := Level.Debug

libraryDependencies ++= Seq(
  "com.typesafe.akka" %% "akka-http" % "10.6.0-M1",
  "com.typesafe.akka" %% "akka-http-spray-json" % "10.6.0-M1",
  "com.typesafe.akka" %% "akka-stream" % "2.9.0-M2"
)