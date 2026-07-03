# CI/CD

## Règles de branche

La branche `main` est protégée (GitHub Ruleset) :

- **Pull Request obligatoire** avant de merger (0 approbation requise — travail en solo sur ce projet)
- **Suppression et force-push bloqués** sur `main`
- **Checks de statut obligatoires** : les jobs `test` et `source-scan` du workflow CI doivent réussir avant qu'une PR ne puisse être fusionnée

Aucun push direct sur `main` n'est possible ; tout changement passe par une branche de travail (`feat/...`, `fix/...`) et une Pull Request.

## Pipeline (`.github/workflows/ci.yml`)

Déclenché sur chaque Pull Request vers `main` et sur chaque push sur `main`. Quatre jobs, avec des dépendances explicites :

```
test ──────────┐
               ├──▶ build (image Docker + scan Trivy) ──▶ publish (main uniquement)
source-scan ───┘
```

| Job | Rôle |
|---|---|
| `test` | `dotnet restore` / `build` / `test` — compile et exécute les 13 tests xUnit. Tourne nativement, pas via Docker, pour un retour rapide. |
| `source-scan` | Scan Trivy du dépôt (`scan-type: fs`) — détecte des secrets committés par erreur et des mauvaises pratiques dans le Dockerfile. Tourne en parallèle de `test` (aucune dépendance entre les deux). |
| `build` | Construit l'image Docker (chargée localement, jamais publiée à ce stade) puis la scanne avec Trivy (vulnérabilités des paquets Alpine et des dépendances NuGet publiées). Échoue sur toute vulnérabilité `HIGH`/`CRITICAL`. |
| `publish` | Reconstruit l'image (rapide grâce au cache GitHub Actions) en **multi-architecture** (`linux/amd64` + `linux/arm64`, via QEMU) et la pousse sur `ghcr.io/camillepaillou/locatic`, taguée à la fois `latest` et par SHA de commit. Ne s'exécute que si `github.ref == 'refs/heads/main'` — jamais depuis une simple PR. |

### Pourquoi l'image est multi-architecture

Les runners GitHub Actions sont en `amd64`. minikube, lui, tourne en `arm64` sur les Mac Apple Silicon. Sans construction multi-plateforme explicite (`docker/setup-qemu-action` + `platforms: linux/amd64,linux/arm64`), l'image publiée n'existerait qu'en `amd64` et le `pull` échouerait sur ce type de machine avec `no matching manifest for linux/arm64`.

### Pourquoi le scan d'image utilise une version Trivy figée

`aquasecurity/trivy-action` installe par défaut une version de Trivy (`v0.65.0`) dont la release a depuis été supprimée du dépôt GitHub — l'installation échoue silencieusement. Le pipeline force explicitement `version: v0.72.0` (une version toujours disponible) dans les deux jobs qui utilisent Trivy.

## Limites du pipeline GitHub

Le pipeline s'arrête **après la publication de l'image**. Il ne contient et ne doit jamais contenir de job de déploiement : minikube tourne sur la machine locale du développeur, une ressource que les runners GitHub Actions (hébergés dans le cloud) ne peuvent pas atteindre. La suite (Terraform, Ansible) s'exécute exclusivement en local — voir [deploiement-local.md](deploiement-local.md).

## Secrets utilisés

- `secrets.GITHUB_TOKEN` — généré automatiquement par GitHub Actions à chaque exécution, utilisé pour s'authentifier auprès de GHCR (`packages: write` déclaré explicitement dans le job `publish`). Aucun token personnel n'est utilisé côté CI.

Le repo doit avoir **"Read and write permissions"** activé dans `Settings > Actions > General > Workflow permissions`, et le package GHCR `locatic` doit accorder le rôle **Write** au repo dans ses réglages d'accès (`Package settings > Manage Actions access`) — sans ces deux réglages, le job `publish` échoue avec `permission_denied: write_package` malgré un token valide.
