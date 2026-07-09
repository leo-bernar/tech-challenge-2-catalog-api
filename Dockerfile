FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/FCG.Catalog.Api/FCG.Catalog.Api.csproj", "src/FCG.Catalog.Api/"]
COPY ["src/FCG.Catalog.Domain/FCG.Catalog.Domain.csproj", "src/FCG.Catalog.Domain/"]
COPY ["src/FCG.Catalog.Infrastructure/FCG.Catalog.Infrastructure.csproj", "src/FCG.Catalog.Infrastructure/"]
RUN dotnet restore "src/FCG.Catalog.Api/FCG.Catalog.Api.csproj"

COPY . .
RUN dotnet restore "src/FCG.Catalog.Api/FCG.Catalog.Api.csproj"
RUN dotnet publish "src/FCG.Catalog.Api/FCG.Catalog.Api.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "FCG.Catalog.Api.dll"]
