# BoletoNetCore Server
# Multi-stage build for production deployment with Playwright support

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

# Install timezone data, curl for health checks, and Chromium dependencies for Playwright
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        tzdata \
        curl \
        # Chromium dependencies for Playwright (Ubuntu 24.04 Noble)
        libasound2t64 \
        libatk-bridge2.0-0t64 \
        libatk1.0-0t64 \
        libatspi2.0-0t64 \
        libcairo2 \
        libcups2t64 \
        libdbus-1-3 \
        libdrm2 \
        libgbm1 \
        libglib2.0-0t64 \
        libnspr4 \
        libnss3 \
        libpango-1.0-0 \
        libx11-6 \
        libxcb1 \
        libxcomposite1 \
        libxdamage1 \
        libxext6 \
        libxfixes3 \
        libxkbcommon0 \
        libxrandr2 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Environment defaults
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    TZ=America/Sao_Paulo \
    # Playwright browser location (accessible by non-root user)
    PLAYWRIGHT_BROWSERS_PATH=/app/.playwright

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
COPY ["BoletoNetCore.Playwright/BoletoNetCore.Playwright.csproj", "BoletoNetCore.Playwright/"]
COPY ["BoletoNetCore.Playwright.Extensions/BoletoNetCore.Playwright.Extensions.csproj", "BoletoNetCore.Playwright.Extensions/"]
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
# PUBLISH STAGE - Create optimized artifacts and install browsers
# =============================================================================
FROM build AS publish

ARG BUILD_CONFIGURATION=Release

# Set browser path for installation
ENV PLAYWRIGHT_BROWSERS_PATH=/app/.playwright

RUN dotnet publish "BoletoNetCore.Server.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Install Playwright CLI and Chromium browser
RUN dotnet tool install --global Microsoft.Playwright.CLI \
    && /root/.dotnet/tools/playwright install chromium

# =============================================================================
# FINAL STAGE - Production image
# =============================================================================
FROM base AS final

# Copy published application
COPY --from=publish /app/publish .

# Copy Playwright browsers
COPY --from=publish /app/.playwright ./.playwright

# Create non-root user directory and set permissions
RUN chown -R $APP_UID:$APP_UID /app

# Switch to non-root user (uses built-in 'app' user, UID 1654)
# Using $APP_UID for Kubernetes runAsNonRoot compatibility
USER $APP_UID

# Health check using ASP.NET Core health endpoint
# For gRPC health checks, use grpc_health_probe instead
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "BoletoNetCore.Server.dll"]
