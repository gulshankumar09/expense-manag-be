# Build stage
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TARGETARCH
WORKDIR /src

# Copy solution files
COPY ["src/AuthService/AuthService.API/AuthService.API.csproj", "AuthService/AuthService.API/"]
COPY ["src/AuthService/AuthService.Application/AuthService.Application.csproj", "AuthService/AuthService.Application/"]
COPY ["src/AuthService/AuthService.Domain/AuthService.Domain.csproj", "AuthService/AuthService.Domain/"]
COPY ["src/AuthService/AuthService.Infrastructure/AuthService.Infrastructure.csproj", "AuthService/AuthService.Infrastructure/"]
COPY ["src/SharedLibrary/SharedLibrary.csproj", "SharedLibrary/"]

# Restore dependencies
RUN dotnet restore "AuthService/AuthService.API/AuthService.API.csproj" -a $TARGETARCH

# Copy the source code
COPY src/ .

# Build and publish
WORKDIR "/src/AuthService/AuthService.API"
RUN dotnet publish "AuthService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false -a $TARGETARCH

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Add labels
LABEL maintainer="gulshankumar.official2000@gmail.com" \
      project="AuthService" \
      version="1.0.0"

# Setup user and directories
#RUN groupadd -r appgroup && \
    #useradd -r -g appgroup appuser && \
    #mkdir -p /https && \
    #chown -R appuser:appgroup /app /https && \
    #chmod 700 /https && \
    #apt-get update && \
    #apt-get install -y curl && \
    #rm -rf /var/lib/apt/lists/*

USER root
RUN apt-get update && apt-get install -y curl
USER $APP_UID

# Copy published files
COPY --from=build --chown=appuser:appgroup /app/publish/ ./

# Switch to non-root user
USER appuser

# Configure environment variables
ENV ASPNETCORE_URLS="https://+:5001;http://+:5000" \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx \
    DOTNET_RUNNING_IN_CONTAINER=true \
    TZ=UTC

# Expose ports
EXPOSE 5000
EXPOSE 5001

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f -k http://localhost:5000/health || curl -f -k https://localhost:5001/health

# Set the entry point
ENTRYPOINT ["dotnet", "AuthService.API.dll"]