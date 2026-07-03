# Kubernetes

## Emplacement des manifests

```
infra/kubernetes/templates/
├── secret-ghcr.yaml.j2          # accès à l'image privée GHCR
├── configmap-nginx.yaml.j2      # configuration du reverse proxy
├── deployment-app.yaml.j2       # application Locatic
├── service-app.yaml.j2          # ClusterIP, interne uniquement
├── deployment-nginx.yaml.j2     # Nginx + sidecar exporter
├── service-nginx.yaml.j2        # point d'entrée exposé
├── podmonitor-app.yaml.j2       # cible de scrape Prometheus (app)
├── podmonitor-nginx.yaml.j2     # cible de scrape Prometheus (Nginx)
└── prometheusrule-locatic.yaml.j2  # règles d'alerte
```

Ce sont des templates Jinja2 (`.yaml.j2`), rendus et appliqués par Ansible ([ansible.md](ansible.md)) — pas des manifests statiques appliqués via `kubectl apply -f` directement. Toutes les valeurs configurables (image, tag, replicas, ressources, type de service...) vivent dans `infra/ansible/vars/locatic.yml`, pas dans ces fichiers.

## Ressources déployées (namespace `locatic`)

| Ressource | Nom | Détail |
|---|---|---|
| `Secret` | `ghcr-pull-secret` | `kubernetes.io/dockerconfigjson`, construit depuis `GHCR_USERNAME`/`GHCR_TOKEN` |
| `ConfigMap` | `nginx-config` | Configuration du reverse proxy (voir plus bas) |
| `Deployment` | `locatic-app` | 2 replicas par défaut, image `ghcr.io/camillepaillou/locatic` |
| `Service` | `locatic-app-svc` | **ClusterIP** — jamais exposé directement |
| `Deployment` | `locatic-nginx` | Nginx + sidecar `nginx-exporter` |
| `Service` | `locatic-nginx-svc` | **NodePort** — seul point d'entrée utilisateur |
| `PodMonitor` | `locatic-app`, `locatic-nginx` | Cibles de scrape pour Prometheus (CRD fournie par `kube-prometheus-stack`) |
| `PrometheusRule` | `locatic-alerts` | 3 règles d'alerte (voir [monitoring.md](monitoring.md)) |

La `PersistentVolumeClaim` (`locatic-sqlite-pvc`) n'est **pas** définie ici : elle est créée par Terraform ([terraform.md](terraform.md)), référencée par son nom (récupéré via `terraform output`) dans le volume du `Deployment` applicatif.

## Application (`locatic-app`)

- Image et tag configurables (`app_image_repository`, `app_image_tag`)
- Port `8080` (correspond à `ASPNETCORE_HTTP_PORTS=8080` défini dans le `Dockerfile`)
- Variables d'environnement : `ASPNETCORE_ENVIRONMENT`, `ConnectionStrings__Default` (calculée depuis `sqlite_mount_path`, ex. `Data Source=/data/locatic.db`)
- Volume : la PVC Terraform, montée sur `/data`
- Probes :
  - **Readiness** → `GET /health/ready` (vérifie la connexion SQLite)
  - **Liveness** → `GET /health/live` (vérifie juste que le process répond)
- Ressources : `requests`/`limits` CPU et mémoire configurables (`app_resources`)
- `imagePullSecrets: [ghcr-pull-secret]` — nécessaire car le package GHCR est privé

## Nginx (`locatic-nginx`)

Deux conteneurs dans le même pod :

1. **`nginx`** — sert de reverse proxy. Sa configuration (`ConfigMap` `nginx-config`) relaie tout le trafic vers `locatic-app-svc` (le `Service` interne de l'application), et expose un endpoint interne `/nginx_status` (module `stub_status`, restreint à `127.0.0.1`) pour les statistiques.
2. **`nginx-exporter`** (`nginx/nginx-prometheus-exporter`) — lit `/nginx_status` via `localhost` (partagé avec le conteneur `nginx` dans le même pod) et réexpose ces données au format Prometheus, sur son propre port (`9113` par défaut).

Le `Service` `locatic-nginx-svc` est le seul exposé (`NodePort` par défaut, configurable en `LoadBalancer`) — c'est le point d'entrée utilisateur unique. L'application n'est jamais accessible autrement que via ce reverse proxy.

## Configuration Nginx

```nginx
server {
    listen 80;
    server_name _;

    location / {
        proxy_pass http://locatic-app-svc.locatic.svc.cluster.local;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    location /nginx_status {
        stub_status;
        allow 127.0.0.1;
        deny all;
    }
}
```

## Stockage SQLite

La PVC créée par Terraform (`locatic-sqlite-pvc`, `ReadWriteOnce`, 1Gi par défaut) est montée sur `/data` dans le conteneur applicatif. Les données survivent à la suppression/recréation d'un pod — vérifié concrètement en supprimant un pod applicatif et en confirmant que le fichier `locatic.db` garde exactement la même taille et le même horodatage après recréation (voir [docs/preuves/](preuves/)).

## Commandes utiles

```bash
kubectl get all -n locatic
kubectl get pvc -n locatic
kubectl describe pod -n locatic <pod>
kubectl logs -n locatic -l app=locatic
kubectl logs -n locatic -l app=locatic-nginx -c nginx
kubectl logs -n locatic -l app=locatic-nginx -c nginx-exporter
kubectl port-forward -n locatic svc/locatic-nginx-svc 8081:80
```
