# Use the official .NET 8.0 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 8.0 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["PharmaDNA.Web/PharmaDNA.Web.csproj", "PharmaDNA.Web/"]
COPY ["PharmaDNA.Web.Tests/PharmaDNA.Web.Tests.csproj", "PharmaDNA.Web.Tests/"]

# Restore dependencies
RUN dotnet restore "PharmaDNA.Web/PharmaDNA.Web.csproj"
RUN dotnet restore "PharmaDNA.Web.Tests/PharmaDNA.Web.Tests.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/PharmaDNA.Web"
RUN dotnet build "PharmaDNA.Web.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "PharmaDNA.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install Node.js for Tailwind CSS
RUN apt-get update && apt-get install -y curl
RUN curl -fsSL https://deb.nodesource.com/setup_18.x | bash -
RUN apt-get install -y nodejs

# Install Tailwind CSS
WORKDIR /app
RUN npm install
RUN npm run build-css-prod

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "PharmaDNA.Web.dll"]
