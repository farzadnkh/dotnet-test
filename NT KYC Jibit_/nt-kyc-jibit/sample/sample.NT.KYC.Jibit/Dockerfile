FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["sample/sample.NT.KYC.Jibit/sample.NT.KYC.Jibit.csproj", "sample/sample.NT.KYC.Jibit/"]
RUN dotnet restore "sample/sample.NT.KYC.Jibit/sample.NT.KYC.Jibit.csproj"
COPY . .
WORKDIR "/src/sample/sample.NT.KYC.Jibit"
RUN dotnet build "sample.NT.KYC.Jibit.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "sample.NT.KYC.Jibit.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "sample.NT.KYC.Jibit.dll"]
