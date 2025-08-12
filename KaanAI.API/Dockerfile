FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy all project files for dependencies
COPY ["KaanAI.API/KaanAI.API.csproj", "KaanAI.API/"]
COPY ["KaanAI.Application/KaanAI.Application.csproj", "KaanAI.Application/"]
COPY ["KaanAI.Application.Abstraction/KaanAI.Application.Abstraction.csproj", "KaanAI.Application.Abstraction/"]
COPY ["KaanAI.Domain/KaanAI.Domain.csproj", "KaanAI.Domain/"]
COPY ["KaanAI.Persistence/KaanAI.Persistence.csproj", "KaanAI.Persistence/"]

# Restore dependencies
RUN dotnet restore "KaanAI.API/KaanAI.API.csproj"

# Copy all source code
COPY . .

WORKDIR "/src/KaanAI.API"
RUN dotnet build "./KaanAI.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./KaanAI.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KaanAI.API.dll"]
