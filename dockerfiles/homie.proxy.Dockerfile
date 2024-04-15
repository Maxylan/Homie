# Use a base image with a JDK
FROM openjdk:11-slim

# Set the working directory in the container
WORKDIR /proxy

# Copy the snakeoil certificate and key
# COPY dockerfiles/ssl src/main/resources/ssl
# RUN chown -R root:root src/main/resources/ssl
# RUN chmod -R 600 src/main/resources/ssl

# EXPOSE ${PORT}
# EXPOSE ${SSL_PORT}