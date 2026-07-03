# Architecture

## Vue d'ensemble et explicaiton de l'architecture

```
Développeur ──push/PR──▶ GitHub ──▶ GitHub Actions ──▶ GHCR (image Docker privée)
                                                              │
                                                    (aucun déploiement automatique)
                                                              │
Poste local ──terraform apply──▶ minikube (namespace + volume persistant)
             ──ansible-playbook──▶ minikube (Nginx, application, monitoring)
                                        │
                                        ▼
                              ┌─────────────────────┐
                              │   Nginx (reverse     │  ◀── seul point d'entrée utilisateur
                              │   proxy, exposé)      │
                              └──────────┬───────────┘
                                         │ (ClusterIP interne)
                              ┌──────────▼───────────┐
                              │   Application         │
                              │   Locatic (2 replicas)│
                              └──────────┬───────────┘
                                         │
                              ┌──────────▼───────────┐
                              │  Volume persistant     │
                              │  SQLite (PVC Terraform)│
                              └───────────────────────┘

                    Prometheus scrape l'app + Nginx + le cluster
                    Grafana visualise, Alertmanager notifie
```

## Détail du flux local (Terraform → Ansible → Kubernetes)

Zoom sur ce qui se passe entre le moment où on lance les commandes et le moment où l'application tourne. 

D'abord, l'enchaînement des actions dans l'ordre — chacune ne démarre qu'une fois la précédente terminée :

```
Poste local
│
├─ 1. terraform apply
│     └─▶ lit infra/terraform/main.tf, compare avec l'état actuel
│     └─▶ CRÉE le namespace "locatic" et la PVC dans minikube
│     └─▶ ÉCRIT leur nom dans le Terraform state (local, jamais committé)
│
└─ 2. ansible-playbook site.yml
      │
      ├─ 2a. LIT le Terraform state
      │      └─▶ commande "terraform output -json" (récupère namespace + nom de la PVC)
      │
      ├─ 2b. CONSTRUIT le secret GHCR (depuis GHCR_USERNAME/GHCR_TOKEN)
      │
      ├─ 2c. INSTALLE la stack de monitoring (Helm, kube-prometheus-stack)
      │
      └─ 2d. REND puis APPLIQUE les manifests Kubernetes
             └─▶ rend les templates .yaml.j2 avec les valeurs de vars/locatic.yml
             └─▶ applique le résultat dans minikube (Secret, ConfigMap, Deployments,
                 Services, PodMonitor, PrometheusRule)
```

Point important : Terraform et Ansible **agissent tous les deux directement sur minikube**, chacun à son étape — Ansible ne passe pas par Terraform pour déployer, il se contente de **lire** ce que Terraform a déjà créé (le namespace et le nom de la PVC), avant d'appliquer le reste lui-même.

Ensuite, ce que ça donne une fois déployé :

```
┌───────────────────────────────── minikube ──────────────────────────────────┐
│                                                                              │
│  namespace "locatic"                                                        │
│  ┌────────────────────┐         monte          ┌──────────────────────────┐ │
│  │ PVC SQLite (1Gi)     │◀────── le volume ─────│ Deployment locatic-app    │ │
│  │ (posée par Terraform)│                        │ (posé par Ansible)        │ │
│  └────────────────────┘                        └────────────┬─────────────┘ │
│                                                               │ expose        │
│                                                               ▼               │
│                                          Service locatic-app-svc (ClusterIP) │
│                                                               ▲               │
│                                                       relaie  │ (proxy_pass)  │
│                                          ┌────────────────────┴────────────┐ │
│                                          │ Deployment locatic-nginx         │ │
│                                          │ (posé par Ansible)                │ │
│                                          └────────────────┬─────────────────┘ │
│                                                            │ expose (NodePort)│
│                                                            ▼                  │
│                                                Utilisateur / navigateur       │
│                                                                              │
│  namespace "monitoring" (installé par Ansible via Helm)                     │
│  Prometheus ──scrape──▶ app + Nginx + cluster ──interrogé par──▶ Grafana     │
└──────────────────────────────────────────────────────────────────────────────┘
```

**Les verbes qui comptent** :
- Terraform **crée** le namespace et la PVC, et **écrit** leur nom dans son state.
- Ansible **lit** ce state via `terraform output`, **construit** le secret GHCR, **rend** les templates Jinja2 avec les valeurs de `vars/locatic.yml`, puis **applique** le résultat au cluster et **installe** la stack de monitoring via Helm.
- Kubernetes **exécute** les pods décrits, **surveille** leur santé (probes), et **redémarre** ceux qui échouent.
- Nginx **relaie** le trafic entrant vers l'application, sans jamais l'exposer directement.
- Prometheus **scrape** (interroge périodiquement) les endpoints `/metrics`, Grafana **interroge** Prometheus pour dessiner les dashboards — Grafana ne mesure jamais rien lui-même.

## Rôle de chaque brique

### GitHub

Point central du projet : code source, historique, Pull Requests, exécution de la CI, hébergement de l'image Docker (GHCR). La branche `main` est protégée : aucun push direct, PR obligatoire, deux checks CI obligatoires (`test`, `source-scan`).

### GitHub Actions

*En une image : l'ouvrier d'usine qui contrôle qualité et emballe le colis (l'image Docker) à chaque livraison de code — mais qui ne le dépose jamais lui-même chez le client. Ça, c'est à quelqu'un d'autre (toi, en local) de le faire.*

Construit et valide chaque changement : compile et teste l'application, scanne le dépôt (secrets, config Dockerfile) et l'image Docker (vulnérabilités), puis publie l'image sur GHCR — uniquement quand le code atteint `main`. Ne déploie jamais sur minikube (contrainte du projet : minikube tourne sur une machine que GitHub Actions ne peut pas atteindre). Détails : [ci-cd.md](ci-cd.md).

### Terraform

*En une image : un plan d'architecte accompagné d'un carnet de chantier. Tu décris l'état voulu ("un namespace, un volume de 1 Gi"), Terraform compare avec ce qui existe déjà (le carnet, appelé "state") et ne construit que ce qui manque — jamais deux fois la même chose.*

Prépare le socle minimal nécessaire dans le cluster Kubernetes, avant que quoi que ce soit d'applicatif ne soit déployé : le namespace `locatic` et la `PersistentVolumeClaim` qui accueillera la base SQLite. Rien de plus — pas de Deployment, pas de Service : ce n'est pas son rôle ici. Détails : [terraform.md](terraform.md).

### Ansible

*En une image : une checklist de mise en route, suivie pas à pas à chaque déploiement. Contrairement à Terraform, Ansible n'a pas de carnet de bord central — chaque étape vérifie elle-même, à chaque exécution, si elle a quelque chose à faire ou si c'est déjà fait.*

Orchestre tout ce qui suit la préparation Terraform, depuis le poste local : vérifie les prérequis (outils installés, minikube démarré), récupère les informations produites par Terraform (`terraform output`), construit le secret d'accès à l'image privée, rend et applique les manifests Kubernetes de l'application et de Nginx, puis installe/maintient à jour la stack de monitoring via Helm. Détails : [ansible.md](ansible.md).

### Déploiement Kubernetes

*En une image : le système d'exploitation du cluster. Kubernetes garde en permanence vivant le nombre de copies (pods) qu'on lui a demandé pour chaque service, remplace instantanément une copie qui plante, et route le trafic uniquement vers celles qui répondent correctement (grâce aux probes de santé).*

Deux composants applicatifs, dans le namespace `locatic` :
- **Nginx** (reverse proxy) — seul point d'entrée utilisateur, exposé via un `Service` de type `NodePort`.
- **Application Locatic** — jamais exposée directement (`Service` de type `ClusterIP`), jointe uniquement par Nginx.

Détails des ressources : [kubernetes.md](kubernetes.md).

### Nginx

*En une image : le standardiste qui répond à tous les appels entrants et les transfère au bon poste en interne — personne à l'extérieur n'a le numéro direct d'un poste interne (l'application).*

Reçoit tout le trafic utilisateur et le relaie vers le `Service` interne de l'application. Un sidecar `nginx-prometheus-exporter`, dans le même pod, convertit les statistiques internes de Nginx (`stub_status`) en métriques Prometheus.

### Volume SQLite

Une `PersistentVolumeClaim` (créée par Terraform, montée par le Deployment de l'application sur `/data`) découple la durée de vie des données de celle des pods : un pod peut être recréé (mise à jour, crash, scaling) sans perdre la base `locatic.db`. Les migrations Entity Framework Core s'appliquent automatiquement au démarrage de l'application (`Database.Migrate()` dans `Program.cs`), de sorte qu'un volume vide (premier déploiement) soit initialisé sans intervention manuelle.

### Monitoring

*En une image : Prometheus est un releveur de compteurs qui passe toutes les 15 secondes noter les chiffres affichés par chaque service, et garde tout l'historique. Grafana ne mesure rien lui-même — c'est le tableau de bord qui dessine des graphes à partir de ce que Prometheus a collecté. Helm, ici, sert d'installateur : plutôt que d'écrire à la main les dizaines de ressources Kubernetes nécessaires à cette stack, on installe un paquet préconfiguré, comme `apt`/`npm` mais pour Kubernetes.*

`kube-prometheus-stack` (Prometheus, Grafana, Alertmanager, kube-state-metrics, node-exporter) est installé via Helm dans un namespace séparé (`monitoring`). Prometheus scrape l'application (métriques HTTP natives via `prometheus-net.AspNetCore`) et Nginx (via son sidecar exporter), en plus des métriques standard du cluster. Un dashboard Grafana dédié et trois règles d'alerte couvrent les composants applicatifs. Détails : [monitoring.md](monitoring.md).

## Adaptations apportées au projet de POO

Le code métier (contrôleurs, modèles, vues, repositories, migrations) est repris tel quel du projet de POO. Les seuls ajouts, nécessaires pour le rendre exploitable en déploiement, sont dans `app/Locatic/Program.cs` et `Locatic.csproj` :

- Application automatique des migrations EF Core au démarrage
- Endpoints de santé `/health/live` (liveness) et `/health/ready` (readiness, vérifie la connexion SQLite)
- Endpoint `/metrics` (métriques Prometheus, via `prometheus-net.AspNetCore`)
- Ajout d'une référence directe à `SQLitePCLRaw.lib.e_sqlite3` version corrigée (CVE-2025-6965, détectée par le scan Trivy de la CI)

Aucune modification du domaine métier, des contrôleurs ou des vues.
