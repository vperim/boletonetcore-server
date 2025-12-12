# BoletoNetCore Server
# Multi-stage build for production deployment

# =============================================================================
# BASE STAGE - Runtime image with security configuration
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble AS base

# OCI Labels - Standard metadata for container registries
LABEL org.opencontainers.image.title="BoletoNetCore Server"
LABEL org.opencontainers.image.description="gRPC/REST API para geração de boletos bancários brasileiros. Baseado em BoletoNetCore."
LABEL org.opencontainers.image.vendor="Perim"
LABEL org.opencontainers.image.authors="Perim"
LABEL org.opencontainers.image.source="https://github.com/vperim/boletonetcore-server"
LABEL org.opencontainers.image.documentation="https://github.com/vperim/boletonetcore-server"
LABEL org.opencontainers.image.licenses="MIT"
LABEL org.opencontainers.image.base.name="BoletoNetCore by BoletoNet"

# Build-time version injection
ARG VERSION=0.0.0
ARG BUILD_DATE
ARG VCS_REF
LABEL org.opencontainers.image.version="${VERSION}"
LABEL org.opencontainers.image.created="${BUILD_DATE}"
LABEL org.opencontainers.image.revision="${VCS_REF}"

# Install timezone data and curl for health checks
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        tzdata \
        curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Environment defaults
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    TZ=America/Sao_Paulo

# Port 8080: HTTP/1.1 (REST/Swagger)
# Port 8081: HTTP/2 (gRPC)
EXPOSE 8080
EXPOSE 8081

# =============================================================================
# BUILD STAGE - SDK image for compilation
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copy project files first for layer caching
COPY ["BoletoNetCore/BoletoNetCore.csproj", "BoletoNetCore/"]
COPY ["BoletoNetCore.QuestPdf/BoletoNetCore.QuestPdf.csproj", "BoletoNetCore.QuestPdf/"]
COPY ["BoletoNetCore.Server.Contracts/BoletoNetCore.Server.Contracts.csproj", "BoletoNetCore.Server.Contracts/"]
COPY ["BoletoNetCore.Server/BoletoNetCore.Server.csproj", "BoletoNetCore.Server/"]

# Restore dependencies
RUN dotnet restore "BoletoNetCore.Server/BoletoNetCore.Server.csproj"

# Copy source code
COPY . .

# Build
WORKDIR "/src/BoletoNetCore.Server"
RUN dotnet build "BoletoNetCore.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# =============================================================================
# PUBLISH STAGE - Create optimized artifacts
# =============================================================================
FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "BoletoNetCore.Server.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# =============================================================================
# FINAL STAGE - Production image
# =============================================================================
FROM base AS final

# Copy published application
COPY --from=publish /app/publish .

# Switch to non-root user (uses built-in 'app' user, UID 1654)
# Using $APP_UID for Kubernetes runAsNonRoot compatibility
USER $APP_UID

# Health check using ASP.NET Core health endpoint
# For gRPC health checks, use grpc_health_probe instead
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "BoletoNetCore.Server.dll"]
