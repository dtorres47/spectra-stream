# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY csharp/src/SpectraStream.Api/SpectraStream.Api.csproj SpectraStream.Api/
RUN dotnet restore SpectraStream.Api/SpectraStream.Api.csproj

COPY csharp/src/SpectraStream.Api/ SpectraStream.Api/
RUN dotnet publish SpectraStream.Api/SpectraStream.Api.csproj -c Release -o /app/publish

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SpectraStream.Api.dll"]