# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS sdk
ARG BUILD_CONFIGURATION=Release
WORKDIR /exchangeRate-gateway

COPY ["nuget.config", "."]
COPY ["src/4.EndPoint/Gateway/ExchangeRateProvider.EndPoint.ApiGateway.App/ExchangeRateProvider.EndPoint.ApiGateway.App.csproj", "src/4.EndPoint/Gateway/ExchangeRateProvider.EndPoint.ApiGateway.App/"]

RUN dotnet restore "src/4.EndPoint/Gateway/ExchangeRateProvider.EndPoint.ApiGateway.App/./ExchangeRateProvider.EndPoint.ApiGateway.App.csproj" --configfile nuget.config
COPY . .

RUN dotnet publish "src/4.EndPoint/Gateway/ExchangeRateProvider.EndPoint.ApiGateway.App/./ExchangeRateProvider.EndPoint.ApiGateway.App.csproj" --no-restore -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0
ARG ENVIRONMENT=$ENVIRONMENT
RUN echo "Current env: $ENVIRONMENT"
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
ENV ASPNETCORE_ENVIRONMENT=$ENVIRONMENT
COPY --from=sdk /app/publish .
ENTRYPOINT ["dotnet", "ExchangeRateProvider.EndPoint.ApiGateway.App.dll"]
