# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy csproj and restore for caching
COPY ContosoMoments/*.csproj ./ContosoMoments/
RUN dotnet restore ContosoMoments/ContosoMoments.csproj

# Copy all source
COPY . .

# Publish
RUN dotnet publish ContosoMoments/ContosoMoments.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine
WORKDIR /app

# Security: Run as non-root
RUN adduser -D appuser
USER appuser

# Copy published output
COPY --from=build /app/publish .

# Expose HTTP
EXPOSE 80

ENTRYPOINT ["dotnet", "ContosoMoments.dll"]
