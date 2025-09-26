# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project folders needed for the API
COPY src/Cloud/ContosoMoments.Common/ ./src/Cloud/ContosoMoments.Common/
COPY src/Cloud/ContosoMoments.API/ ./src/Cloud/ContosoMoments.API/

# Restore & build API project only
RUN dotnet restore src/Cloud/ContosoMoments.API/ContosoMoments.API.csproj
RUN dotnet build src/Cloud/ContosoMoments.API/ContosoMoments.API.csproj -c Release -o /app/build
RUN dotnet publish src/Cloud/ContosoMoments.API/ContosoMoments.API.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Security: non-root
RUN adduser -D appuser
USER appuser

# Copy published API
COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "ContosoMoments.API.dll"]



