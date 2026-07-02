# --- Stage 1 : restore + tests -----------------------------------------
# Image complète avec le SDK (compilateur), jamais expédiée en prod.
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS test
WORKDIR /src

# On copie d'abord uniquement les fichiers de projet pour profiter du cache
# Docker : `dotnet restore` ne se relance que si un .csproj/.sln change,
# pas à chaque modification de code source.
COPY Locatic.sln ./
COPY Locatic/Locatic.csproj Locatic/
COPY Locatic.Tests/Locatic.Tests.csproj Locatic.Tests/
RUN dotnet restore

COPY Locatic/ Locatic/
COPY Locatic.Tests/ Locatic.Tests/

# Si un test échoue, `docker build` s'arrête ici avec une erreur : impossible
# de publier une image dont les tests ne passent pas.
RUN dotnet test --no-restore

# --- Stage 2 : publish ----------------------------------------------------
FROM test AS build
RUN dotnet publish Locatic/Locatic.csproj -c Release -o /app/publish --no-restore

# --- Stage 3 : image finale, minimale -------------------------------------
# Juste le runtime ASP.NET Core, pas le SDK : image beaucoup plus légère,
# et le code source / outils de build n'y sont pas embarqués.
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS production
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
# Chemin par défaut, prévu pour être écrasé par la variable d'environnement
# ConnectionStrings__Default (ex: pointer vers un volume monté en Kubernetes).
ENV ConnectionStrings__Default="Data Source=/data/locatic.db"

COPY --from=build /app/publish .

# "app" est un utilisateur non privilégié déjà présent dans les images
# officielles .NET 8 (comme "node" dans les images node:*-alpine).
RUN mkdir -p /data && chown -R app:app /data
USER app

EXPOSE 8080

# Utilise l'endpoint de santé ajouté à l'étape 4. wget est fourni par
# busybox dans l'image Alpine, pas besoin de l'installer séparément.
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD wget -qO- http://127.0.0.1:8080/health/live >/dev/null || exit 1

ENTRYPOINT ["dotnet", "Locatic.dll"]
