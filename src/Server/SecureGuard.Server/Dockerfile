FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Server/SecureGuard.Server/SecureGuard.Server.csproj", "src/Server/SecureGuard.Server/"]
COPY ["src/Shared/SecureGuard.Shared/SecureGuard.Shared.csproj", "src/Shared/SecureGuard.Shared/"]
RUN dotnet restore "src/Server/SecureGuard.Server/SecureGuard.Server.csproj"
COPY . .
WORKDIR "/src/src/Server/SecureGuard.Server"
RUN dotnet build "SecureGuard.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SecureGuard.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SecureGuard.Server.dll"]
