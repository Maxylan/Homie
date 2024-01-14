# Use a base image with a JDK
FROM openjdk:11

# Set the working directory in the container
WORKDIR /api

# Copy only the build files to leverage Docker cache
COPY /src/api/build.sbt build.sbt
COPY /src/api/project project

# Download and resolve dependencies
RUN sbt update

# Command to run your application with SBT Revolver defined in docker-compose.yaml