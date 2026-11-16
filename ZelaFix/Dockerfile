# ==============================================================================
# Dockerfile Multi-Stage Build para ZelaFix API (.NET 10)
# Compatível quando Root Directory no Render é configurado como "ZelaFix"
# ==============================================================================

# Estágio 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# 1. Copia csproj dos projetos para aproveitar cache de camadas do Docker
COPY ["ZelaFix.Domain/ZelaFix.Domain.csproj", "ZelaFix.Domain/"]
COPY ["ZelaFix.Application/ZelaFix.Application.csproj", "ZelaFix.Application/"]
COPY ["ZelaFix.Infrastructure/ZelaFix.Infrastructure.csproj", "ZelaFix.Infrastructure/"]
COPY ["ZelaFix.Api/ZelaFix.Api.csproj", "ZelaFix.Api/"]

# 2. Restaura as dependências NuGet
RUN dotnet restore "ZelaFix.Api/ZelaFix.Api.csproj"

# 3. Copia todo o código fonte e compila em modo Release
COPY . .
WORKDIR "/src/ZelaFix.Api"
RUN dotnet publish "ZelaFix.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio 2: Runtime enxuto
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app

# Render / PaaS bindings
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ZelaFix.Api.dll"]
