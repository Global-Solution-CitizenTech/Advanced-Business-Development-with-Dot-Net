# ==============================================================================
# Dockerfile Multi-Stage Build para ZelaFix API (.NET 10)
# Compatível com Render Web Service (Root Directory vazio ou .)
# ==============================================================================

# Estágio 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# 1. Copia csproj dos projetos para aproveitar cache de camadas do Docker
COPY ["ZelaFix/ZelaFix.Domain/ZelaFix.Domain.csproj", "ZelaFix/ZelaFix.Domain/"]
COPY ["ZelaFix/ZelaFix.Application/ZelaFix.Application.csproj", "ZelaFix/ZelaFix.Application/"]
COPY ["ZelaFix/ZelaFix.Infrastructure/ZelaFix.Infrastructure.csproj", "ZelaFix/ZelaFix.Infrastructure/"]
COPY ["ZelaFix/ZelaFix.Api/ZelaFix.Api.csproj", "ZelaFix/ZelaFix.Api/"]

# 2. Restaura as dependências NuGet
RUN dotnet restore "ZelaFix/ZelaFix.Api/ZelaFix.Api.csproj"

# 3. Copia todo o código fonte e compila em modo Release
COPY ZelaFix/ ZelaFix/
WORKDIR "/src/ZelaFix/ZelaFix.Api"
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
