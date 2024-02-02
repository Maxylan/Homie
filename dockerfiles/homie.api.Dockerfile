# Use a base image with a JDK
FROM openjdk:11

# Install sbt
# Default: ARG API_SBT_VERSION=1.9.8

RUN \
    curl -L -o sbt-${API_SBT_VERSION}.deb https://repo.scala-sbt.org/scalasbt/debian/sbt-${API_SBT_VERSION}.deb && \
    dpkg -i sbt-${API_SBT_VERSION}.deb && \
    rm sbt-${API_SBT_VERSION}.deb && \
    apt-get update && \
    apt-get install sbt

# Set the working directory in the container
WORKDIR /api

# Copy only the build files to leverage Docker cache
COPY src/api/build.sbt build.sbt
COPY src/api/project project
COPY src/api/src src

# Copy the snakeoil certificate and key
COPY /dockerfiles/ssl src/main/resources/ssl
RUN chown -R root:root src/main/resources/ssl
RUN chmod -R 600 src/main/resources/ssl

# Download and resolve dependencies
RUN sbt update

EXPOSE ${API_PORT}
EXPOSE ${API_SSL_PORT}

# Command to run your application with SBT Revolver defined in docker-compose.yaml
# (if you can get SBT Revolver to work, haha, ha)