# Use a base image with a JDK
FROM openjdk:11

# Set the working directory in the container
WORKDIR /proxy

# Copy only the build files to leverage Docker cache
COPY /src/proxy/build.sbt build.sbt
COPY /src/proxy/project project

# Download and resolve dependencies
RUN sbt update

# Command to run your application with SBT Revolver defined in docker-compose.yaml