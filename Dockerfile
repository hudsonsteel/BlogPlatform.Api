# syntax=docker/dockerfile:1

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy props and project files first so the restore layer is cached
# until project files actually change.
COPY Directory.Build.props ./
COPY src/BlogPlatform.Domain/BlogPlatform.Domain.csproj          src/BlogPlatform.Domain/
COPY src/BlogPlatform.Application/BlogPlatform.Application.csproj src/BlogPlatform.Application/
COPY src/BlogPlatform.Infrastructure/BlogPlatform.Infrastructure.csproj src/BlogPlatform.Infrastructure/
COPY src/BlogPlatform.Presentation/BlogPlatform.Presentation.csproj src/BlogPlatform.Presentation/

RUN dotnet restore src/BlogPlatform.Presentation/BlogPlatform.Presentation.csproj

# Now copy the rest of the source and publish.
COPY . .
RUN dotnet publish src/BlogPlatform.Presentation/BlogPlatform.Presentation.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Run as a non-root user.
RUN groupadd --system app && useradd --system --gid app app \
    && mkdir -p /app/data /app/logs \
    && chown -R app:app /app

USER app

COPY --from=build --chown=app:app /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__Default="Data Source=/app/data/blogplatform.db"

EXPOSE 8080

ENTRYPOINT ["dotnet", "BlogPlatform.Presentation.dll"]
