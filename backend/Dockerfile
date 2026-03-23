# ╔══════════════════════════════════════════════════════════════════════════════╗
# ║  AutoHelper Backend — Multi-stage Dockerfile                                ║
# ║  Stage 1 (build)   : restore + publish Release                              ║
# ║  Stage 2 (runtime) : minimal ASP.NET runtime, non-root user                 ║
# ╚══════════════════════════════════════════════════════════════════════════════╝

# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution manifests first — changes to source code won't bust this cache
COPY AutoHelper.sln Directory.Build.props Directory.Packages.props ./

# Copy project files
COPY src/AutoHelper.Domain/AutoHelper.Domain.csproj          src/AutoHelper.Domain/
COPY src/AutoHelper.Application/AutoHelper.Application.csproj src/AutoHelper.Application/
COPY src/AutoHelper.Infrastructure/AutoHelper.Infrastructure.csproj src/AutoHelper.Infrastructure/
COPY src/AutoHelper.Api/AutoHelper.Api.csproj                 src/AutoHelper.Api/

# Restore (cached as long as .csproj files don't change)
RUN dotnet restore src/AutoHelper.Api/AutoHelper.Api.csproj

# Copy the rest of the source
COPY src/ src/

# Publish self-contained-ready release build
RUN dotnet publish src/AutoHelper.Api/AutoHelper.Api.csproj \
      --configuration Release \
      --no-restore \
      --output /app/publish

# ── Stage 2: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install curl for health-check probe only; clean up afterwards
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

# Create a dedicated non-root user
RUN groupadd --system --gid 1001 appgroup \
 && useradd  --system --uid 1001 --gid appgroup --no-create-home appuser

# Copy published artefacts with correct ownership
COPY --from=build --chown=appuser:appgroup /app/publish .

# Drop privileges
USER appuser

# ASP.NET Core listens on 8080 by default (ASPNETCORE_HTTP_PORTS)
EXPOSE 8080

ENTRYPOINT ["dotnet", "AutoHelper.Api.dll"]
