FROM python:3.11-slim

# Set the working directory in the container and copy the 
# contents of the host's src/api/* directory into /app
WORKDIR /api
COPY src/api /api

# Install any needed dependencies specified in requirements.txt
RUN pip install --no-cache-dir -r requirements.txt