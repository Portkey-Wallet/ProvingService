# Use the .NET 8.0 SDK as the base image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Set the working directory
WORKDIR /app

# Copy the entire solution directory
COPY . ./

# Restore the NuGet packages
RUN dotnet restore ProvingService/ProvingService.csproj

# Build the application
RUN dotnet publish ProvingService/ProvingService.csproj -c Release -o out

# Use the .NET 8.0 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Set the working directory
WORKDIR /app

# Copy the build output from the build environment
COPY --from=build-env /app/out .

# Set the ASP.NET Core URLs environment variable
ENV ASPNETCORE_URLS=http://0.0.0.0:7020

# Set the entrypoint
ENTRYPOINT ["/app/ProvingService"]
