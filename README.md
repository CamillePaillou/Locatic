# Locatic — Chaîne DevOps

Application de location de voitures (ASP.NET Core MVC + SQLite), déployée sur un cluster Kubernetes local (minikube) via une chaîne complète : GitHub Actions, Terraform, Ansible, Helm, Prometheus/Grafana.

## Objectifs

Reprendre l'application Locatic (projet de POO avec C#) et y associer une chaîne DevOps complète :

1. Gestion Git professionnelle (branche `main` protégée, Pull Requests, checks obligatoires)
2. Pipeline CI GitHub Actions (tests, scan de sécurité, build et publication d'image Docker)
3. Infrastructure locale automatisée (Terraform prépare le socle Kubernetes, Ansible pour l'orchestration du déploiement)
4. Déploiement Kubernetes local (minikube), avec Nginx en reverse proxy et un volume persistant pour la base SQLite
5. Supervision et vision des métriques avec Prometheus + Grafana

Le pipeline GitHub s'arrête après les contrôles, le build, le scan et la publication de l'image jamais depuis GitHub Actions.


**Les avantages d'une chaine devops au lieu d'un déploiement manuel** :
- **Reproductibilité** — n'importe qui peut reconstruire le déploiement à l'identique sur sa propre machine, en suivant les mêmes fichiers (pas de configuration "tribale" gardée uniquement dans la tête d'une personne).
- **Détection précoce des problèmes** — les tests et les scans de sécurité bloquent un changement cassé ou vulnérable *avant* qu'il n'atteigne `main`, pas après coup en production.
- **Moins d'erreurs manuelles** — Terraform et Ansible remplacent des suites de clics/commandes répétées à la main, une source classique d'oublis et d'incohérences entre deux déploiements.
- **Traçabilité** — chaque changement passe par une Pull Request relue ; l'historique Git documente qui a changé quoi, et pourquoi.
- **Observabilité** — Prometheus/Grafana rendent visibles les problèmes (application down, taux d'erreur élevé, stockage non lié) au lieu de les découvrir par hasard, ou par un utilisateur mécontent.
- **Résilience** — Kubernetes redémarre automatiquement ce qui plante, sans intervention humaine.



## Prérequis

Pour reproduire le déploiement complet sur sa propre machine :

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (avec buildx)
- [minikube](https://minikube.sigs.k8s.io/) et [kubectl](https://kubernetes.io/docs/tasks/tools/)
- [Terraform](https://www.terraform.io/) >= 1.5
- [Ansible](https://www.ansible.com/) (avec la collection `kubernetes.core`)
- [Helm](https://helm.sh/) (utilisé par Ansible via `kubernetes.core.helm`, pas besoin de l'invoquer à la main)
- Un compte GitHub avec accès en lecture/écriture au package GHCR `locatic` (privé)

## Structure du dépôt

```
Locatic/
├── .github/workflows/ci.yml    # Pipeline CI : test, scan, build, publication
├── app/                        # Code applicatif (ASP.NET Core), issu du projet de POO
│   ├── Locatic/                # Le projet web
│   ├── Locatic.Tests/          # Tests xUnit
│   ├── Dockerfile              # Build multi-stage (test -> publish -> runtime alpine)
│   └── Locatic.sln
├── infra/
│   ├── terraform/               # Socle Kubernetes : namespace + volume persistant SQLite
│   ├── ansible/                 # Orchestration du déploiement local
│   │   ├── site.yml             # Le playbook principal
│   │   ├── vars/locatic.yml     # Toutes les valeurs configurables du déploiement
│   │   └── files/               # Dashboard Grafana (as code)
│   └── kubernetes/templates/    # Manifests Kubernetes (templates Jinja2, rendus par Ansible)
├── docs/                        # Documentation détaillée (ce dossier)
└── mini-projet.md               # Énoncé original du projet de POO
```

## Démarrage rapide

### 1. Lancer l'application seule, en local (sans Kubernetes)

```bash
cd app/Locatic
dotnet run   # applique les migrations et crée locatic.db automatiquement au démarrage
```

### 2. Déployer la chaîne complète sur minikube

```bash
minikube start

# Étape 1 : infrastructure de base (namespace + volume persistant)
cd infra/terraform
terraform init
terraform apply

# Étape 2 : déploiement applicatif + monitoring
cd ../ansible
export GHCR_USERNAME=<ton-username-github>
export GHCR_TOKEN=<un-token-avec-read:packages>
export GRAFANA_ADMIN_PASSWORD=<mot-de-passe-de-ton-choix>
ansible-playbook -i inventory.yml site.yml
```

> **Comment obtenir un token GitHub** : *Settings* → *Developer settings* → *Personal access tokens* → *Tokens (classic)* → *Generate new token* → coche `read:packages` (ajoute `write:packages` si tu comptes aussi publier des images depuis ce poste) → génère et copie-le tout de suite (il ne sera plus jamais réaffiché).

### 3. Accéder à l'application

```bash
kubectl port-forward -n locatic svc/locatic-nginx-svc 8081:80
```
→ http://localhost:8081

Voir [docs/deploiement-local.md](docs/deploiement-local.md) pour la procédure détaillée et [docs/exploitation.md](docs/exploitation.md) pour les vérifications post-déploiement.

## Documentation détaillée

| Document | Contenu |
|---|---|
| [docs/architecture.md](docs/architecture.md) | Vue d'ensemble de l'architecture et rôle de chaque brique |
| [docs/ci-cd.md](docs/ci-cd.md) | Règles de branche, pipeline GitHub Actions, limites |
| [docs/deploiement-local.md](docs/deploiement-local.md) | Procédure pas à pas pour déployer depuis une image publiée |
| [docs/terraform.md](docs/terraform.md) | Ressources gérées, variables, outputs, gestion de l'état |
| [docs/ansible.md](docs/ansible.md) | Rôle du playbook, étapes orchestrées |
| [docs/kubernetes.md](docs/kubernetes.md) | Ressources Kubernetes, services, stockage, config Nginx |
| [docs/helm.md](docs/helm.md) | Usage de Helm dans le projet |
| [docs/monitoring.md](docs/monitoring.md) | Prometheus, Grafana, métriques, alertes |
| [docs/exploitation.md](docs/exploitation.md) | Vérifications, logs, rollback, diagnostic |
| [docs/preuves/](docs/preuves/) | Captures et preuves d'exécution |
