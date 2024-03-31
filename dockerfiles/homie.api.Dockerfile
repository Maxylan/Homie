# Lifted from..
# @see https://medium.com/@jaydeepvpatil225/containerization-of-the-net-core-7-web-api-using-docker-3abdd543f78a
# Credit where its due!

# Use the official .NET Core SDK 8.0 to build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /api

# Copy the project file and restore any dependencies (use .csproj for the project name)
COPY src/api/HomieBackoffice.csproj ./
COPY src/api/HomieBackoffice.sln ./
RUN dotnet restore

# Copy the rest of the application code
COPY src/api/. .

# Publish the application
RUN dotnet publish -c Release -o out

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /api
COPY --from=build /api/out ./