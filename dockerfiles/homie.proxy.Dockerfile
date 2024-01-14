# Use a base image with a JDK
FROM openjdk:11

# Install sbt
ARG SBT_VERSION=1.9.8
RUN \
    curl -L -o sbt-$SBT_VERSION.deb https://repo.scala-sbt.org/scalasbt/debian/sbt-$SBT_VERSION.deb && \
    dpkg -i sbt-$SBT_VERSION.deb && \
    rm sbt-$SBT_VERSION.deb && \
    apt-get update && \
    apt-get install sbt

# Set the working directory in the container
WORKDIR /proxy

# Copy only the build files to leverage Docker cache
COPY /src/proxy/build.sbt build.sbt
COPY /src/proxy/project project

# Copy the snakeoil certificate and key
COPY /dockerfiles/snakeoil /etc/ssl
RUN chown -R root:root /etc/ssl
RUN chmod -R 600 /etc/ssl

# Download and resolve dependencies
RUN sbt update

EXPOSE 80
EXPOSE 443

# Command to run your application with SBT Revolver defined in docker-compose.yaml