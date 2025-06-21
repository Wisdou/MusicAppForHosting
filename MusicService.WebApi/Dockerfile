FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN groupadd -r appgroup &&  \
    useradd -r -g appgroup -d /home/appuser -m appuser

WORKDIR /app
EXPOSE 5555

RUN mkdir -p /app/data-protection-keys &&  \
    chown -R appuser:appgroup /app/data-protection-keys

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["MusicService.WebApi/MusicService.WebApi.csproj", "MusicService.WebApi/"]
COPY ["MusicService.DAL.PostgreSQL/MusicService.DAL.PostgreSQL.csproj", "MusicService.DAL.PostgreSQL/"]
COPY ["MusicService.Domain/MusicService.Domain.csproj", "MusicService.Domain/"]
COPY ["MusicService.WebApi.Contracts/MusicService.WebApi.Contracts.csproj", "MusicService.WebApi.Contracts/"]
RUN dotnet restore "MusicService.WebApi/MusicService.WebApi.csproj"

COPY . .
WORKDIR "/src/MusicService.WebApi"
RUN dotnet build "MusicService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MusicService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN chown -R appuser:appgroup /app
USER appuser

ENTRYPOINT ["dotnet", "MusicService.WebApi.dll"]
