FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore API/Bootstrapper.csproj
RUN dotnet publish API/Bootstrapper.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# curl is used by the container health check below.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

HEALTHCHECK --interval=30s --timeout=5s --start-period=90s --retries=3 \
    CMD curl --fail http://localhost:5000/health/live || exit 1

ENTRYPOINT ["dotnet", "FluentPOS.Bootstrapper.dll"]
