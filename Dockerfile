# =============================================================================
# Dockerfile - build multi-estagio
#
# O estagio final usa a imagem de RUNTIME (aspnet), nao a de SDK: a imagem
# publicada nao carrega compilador nem codigo-fonte. Menos superficie de
# ataque e imagem varias vezes menor.
# =============================================================================

# ----- Estagio 1: restore + build + publish ---------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Os arquivos de projeto sao copiados ANTES do codigo-fonte.
# Assim a camada do "restore" so e invalidada quando uma dependencia muda -
# alterar uma linha de C# nao provoca um restore completo a cada build.
COPY Directory.Build.props Directory.Packages.props ./
COPY src/GestaoColaboradores.Domain/*.csproj          src/GestaoColaboradores.Domain/
COPY src/GestaoColaboradores.Application/*.csproj     src/GestaoColaboradores.Application/
COPY src/GestaoColaboradores.Infrastructure/*.csproj  src/GestaoColaboradores.Infrastructure/
COPY src/GestaoColaboradores.Web/*.csproj             src/GestaoColaboradores.Web/

RUN dotnet restore src/GestaoColaboradores.Web/GestaoColaboradores.Web.csproj

COPY src/ src/

RUN dotnet publish src/GestaoColaboradores.Web/GestaoColaboradores.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ----- Estagio 2: runtime ---------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Usuario sem privilegios. Container rodando como root e achado recorrente
# em revisao de seguranca corporativa.
USER $APP_UID

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "GestaoColaboradores.Web.dll"]
