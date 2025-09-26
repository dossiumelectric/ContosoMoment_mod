# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY ContosoCloud.sln ./

# Copy project folders
COPY src/Cloud/ContosoMoments.Common/ ./src/Cloud/ContosoMoments.Common/
COPY src/Cloud/ContosoMoments.API/ ./src/Cloud/ContosoMoments.API/
COPY src/Cloud/ContosoMoments.Function/ ./src/Cloud/ContosoMoments.Function/
COPY src/Cloud/ContosoMoments.ResizerWebJob/ ./src/Cloud/ContosoMoments.ResizerWebJob/
COPY src/Cloud/ContosoMoments.WebJobWrapper/ ./src/Cloud/ContosoMoments.WebJobWrapper/

# Restore NuGet packages for the solution
RUN dotnet restore ContosoCloud.sln

# Build the solution in Release mode
RUN dotnet build ContosoCloud.sln -c Release -o /app/build

# Publish the API project only (adjust if you want a different project)
RUN dotnet publish src/Cloud/ContosoMoments.API/ContosoMoments.API.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "ContosoMoments.API.dll"]
