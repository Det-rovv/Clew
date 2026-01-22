FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Clew.Api/Clew.Api.csproj", "src/Clew.Api/"]
COPY ["src/Clew.Application/Clew.Application.csproj", "src/Clew.Application/"]
COPY ["src/Clew.Domain/Clew.Domain.csproj", "src/Clew.Domain/"]
COPY ["src/Clew.Infrastructure/Clew.Infrastructure.csproj", "src/Clew.Infrastructure/"]
RUN dotnet restore "src/Clew.Api/Clew.Api.csproj"
COPY . .
WORKDIR "/src/src/Clew.Api"
RUN dotnet build "./Clew.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Clew.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Clew.Api.dll"]
