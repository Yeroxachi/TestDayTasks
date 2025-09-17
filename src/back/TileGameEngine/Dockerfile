FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TileGameEngine.Presentation/TileGameEngine.Presentation.csproj", "TileGameEngine.Presentation/"]
COPY ["TileGameEngine.Domain/TileGameEngine.Domain.csproj", "TileGameEngine.Domain/"]
COPY ["TileGameEngine.Application/TileGameEngine.Application.csproj", "TileGameEngine.Application/"]
COPY ["TileGameEngine.Persistence/TileGameEngine.Persistence.csproj", "TileGameEngine.Persistence/"]
RUN dotnet restore "TileGameEngine.Presentation/TileGameEngine.Presentation.csproj"
COPY . .
WORKDIR "/src/TileGameEngine.Presentation"
RUN dotnet build "./TileGameEngine.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TileGameEngine.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TileGameEngine.Presentation.dll"]
