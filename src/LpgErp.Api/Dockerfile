# Build context: ./src  (docker compose sets: context: ./src, dockerfile: LpgErp.Api/Dockerfile)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first for layer caching
COPY LpgErp.Domain/LpgErp.Domain.csproj LpgErp.Domain/
COPY LpgErp.Application/LpgErp.Application.csproj LpgErp.Application/
COPY LpgErp.Infrastructure/LpgErp.Infrastructure.csproj LpgErp.Infrastructure/
COPY LpgErp.Api/LpgErp.Api.csproj LpgErp.Api/
RUN dotnet restore LpgErp.Api/LpgErp.Api.csproj

COPY . .
RUN dotnet publish LpgErp.Api/LpgErp.Api.csproj -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "LpgErp.Api.dll"]
