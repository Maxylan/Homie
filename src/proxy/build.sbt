// (c) 2024 @Maxylan
name := "HomieReverseProxy"
version := "7"

mainClass in Compile := Some("com.homie.reverseproxy.ReverseProxy")
classLoaderLayeringStrategy := ClassLoaderLayeringStrategy.AllLibraryJars

// Enable debugging options
// logLevel := Level.Debug
logLevel := Level.Error
scalaVersion := "3.3.1"

libraryDependencies ++= Seq(
  "com.typesafe" % "config" % "1.4.3",
  "com.typesafe.slick" %% "slick" % "3.5.1",
  "mysql" % "mysql-connector-java" % "8.0.31",
  "com.typesafe.akka" %% "akka-http" % "10.6.0-M1",
  "com.typesafe.akka" %% "akka-http-spray-json" % "10.6.0-M1",
  "com.typesafe.akka" %% "akka-stream" % "2.9.0-M2",
  "com.typesafe.akka" %% "akka-slf4j" % "2.9.0-M2"
)