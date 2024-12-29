# Specify platform for multi-arch support
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TARGETARCH

# Set working directory
WORKDIR /src

# Copy solution and project files first to leverage Docker cache
COPY ["src/AuthService/AuthService.API/AuthService.API.csproj", "src/AuthService/AuthService.API/"]
COPY ["src/AuthService/AuthService.Application/AuthService.Application.csproj", "src/AuthService/AuthService.Application/"]
COPY ["src/AuthService/AuthService.Domain/AuthService.Domain.csproj", "src/AuthService/AuthService.Domain/"]
COPY ["src/AuthService/AuthService.Infrastructure/AuthService.Infrastructure.csproj", "src/AuthService/AuthService.Infrastructure/"]
COPY ["src/SharedLibrary/SharedLibrary.csproj", "src/SharedLibrary/"]

# Restore NuGet packages
RUN dotnet restore "src/AuthService/AuthService.API/AuthService.API.csproj" -a $TARGETARCH

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/src/AuthService/AuthService.API"
RUN dotnet build "AuthService.API.csproj" -c Release -o /app/build -a $TARGETARCH

# Publish the application
FROM build AS publish
RUN dotnet publish "AuthService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false -a $TARGETARCH

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Add labels for better maintainability
LABEL maintainer="gulshankumar.official2000@example.com"
LABEL project="AuthService"
LABEL version="1.0.0"

# # Install curl for healthcheck and create non-root user
# RUN apt-get update \
#     && apt-get install -y --no-install-recommends curl=7.88.1-10+deb12u5 \
#     && rm -rf /var/lib/apt/lists/* \
#     && adduser --disabled-password --gecos "" appuser \
#     && chown -R appuser /app

# Install curl without specifying a version
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && adduser --disabled-password --gecos "" appuser \
    && chown -R appuser /app

# Create directory for SSL certificate with proper permissions
RUN mkdir -p /https \
    && chown -R appuser /https \
    && chmod 700 /https

# Switch to non-root user
USER appuser

# Copy published files from publish stage
COPY --from=publish --chown=appuser:appuser /app/publish .

# Configure environment variables
ENV ASPNETCORE_URLS="https://+:5001;http://+:5111" \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx \
    # Don't set the certificate password here, it should be passed through docker-compose
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Expose ports
EXPOSE 5111
EXPOSE 5001

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f -k https://localhost:5001/health || exit 1

# Set the entry point
ENTRYPOINT ["dotnet", "AuthService.API.dll"]
