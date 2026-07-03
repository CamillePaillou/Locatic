# Monitoring

## Stack

[`kube-prometheus-stack`](https://github.com/prometheus-community/helm-charts/tree/main/charts/kube-prometheus-stack) (Prometheus, Grafana, Alertmanager, `kube-state-metrics`, `node-exporter`), installée via Helm depuis Ansible dans le namespace **`monitoring`** (séparé de `locatic` — le monitoring n'est pas un composant applicatif). Voir [helm.md](helm.md) pour le détail de ce choix.

## Services monitorés

| Service | Comment | Métriques |
|---|---|---|
| Application Locatic | `PodMonitor` `locatic-app`, cible le port `http` (8080), chemin `/metrics` | `http_request_duration_seconds_*` (latence), `http_requests_received_total` (débit, par route/méthode/code), `http_requests_in_progress` |
| Nginx | `PodMonitor` `locatic-nginx`, cible le port `metrics` (9113, le sidecar `nginx-exporter`) | `nginx_connections_active/accepted/handled`, `nginx_http_requests_total` |
| Cluster (pods, PVC, nœud) | Scrape par défaut de `kube-prometheus-stack` (`kube-state-metrics`, `node-exporter`, `kubelet`) | `up`, `kube_pod_status_phase`, `kube_persistentvolumeclaim_status_phase`, métriques CPU/mémoire |

### Comment les métriques applicatives sont exposées

- **Application** — package NuGet `prometheus-net.AspNetCore` (`Program.cs` : `app.UseHttpMetrics()` + `app.MapMetrics()`), endpoint `/metrics` sur le même port que l'application (8080).
- **Nginx** — module natif `stub_status` (endpoint interne `/nginx_status`, restreint à `localhost`) + sidecar `nginx-prometheus-exporter` qui le convertit en métriques Prometheus, exposées sur son propre port (9113).

## Pourquoi Prometheus scrape des namespaces autres que `monitoring`

Par défaut, l'opérateur Prometheus de `kube-prometheus-stack` n'accepte de scraper que les `PodMonitor`/`ServiceMonitor` du namespace de la release (`monitoring`). L'application et Nginx tournent dans `locatic`. Les valeurs Helm suivantes (dans `infra/ansible/site.yml`) élargissent cette portée à tout le cluster :

```yaml
prometheus:
  prometheusSpec:
    serviceMonitorSelectorNilUsesHelmValues: false
    podMonitorSelectorNilUsesHelmValues: false
    ruleSelectorNilUsesHelmValues: false
    podMonitorNamespaceSelector: {}
    serviceMonitorNamespaceSelector: {}
    ruleNamespaceSelector: {}
```

## Dashboard Grafana

Un dashboard dédié, **"Locatic - Vue d'ensemble"**, provisionné "as code" depuis `infra/ansible/files/grafana-dashboard-locatic.json` (injecté dans les valeurs Helm sous `grafana.dashboards.default`, couplé à `grafana.dashboardProviders` — les deux sont nécessaires, voir le commentaire dans `site.yml`). 6 panels, un par élément demandé :

1. Débit de requêtes de l'application
2. Latence P95 de l'application
3. Connexions actives Nginx
4. Débit de requêtes Nginx
5. État des pods Locatic (up/down par job)
6. État du volume SQLite (`Bound` / non-`Bound`)

L'état des composants du monitoring lui-même (Prometheus, Alertmanager) est couvert par les dashboards **intégrés** au chart (`Alertmanager / Overview`, `Grafana Overview`, etc.), disponibles nativement sans configuration supplémentaire.

Accès :
```bash
kubectl port-forward -n monitoring svc/kube-prometheus-stack-grafana 3000:80
```
→ http://localhost:3000 (utilisateur `admin`, mot de passe = la valeur de `GRAFANA_ADMIN_PASSWORD` fournie au moment du déploiement)

## Accès à Prometheus

```bash
kubectl port-forward -n monitoring svc/kube-prometheus-stack-prometheus 9090:9090
```
→ http://localhost:9090/targets (état des cibles scrapées) et http://localhost:9090/alerts (état des règles d'alerte)

## Alertes

Trois règles (`infra/kubernetes/templates/prometheusrule-locatic.yaml.j2`) :

| Alerte | Sévérité | Condition |
|---|---|---|
| `LocaticAppDown` | critical | `up{job="locatic/locatic-app"} == 0` pendant plus d'1 minute |
| `LocaticHighErrorRate` | warning | Plus de 5% de réponses 5xx sur les 5 dernières minutes |
| `LocaticStorageNotBound` | warning | La PVC SQLite n'est plus à l'état `Bound` depuis plus de 5 minutes |

**Note sur le stockage** : `kubelet_volume_stats_*` (usage réel en octets) n'est pas exposé par le provisioner `hostPath` de minikube. L'alerte de stockage se base donc sur l'état de liaison de la PVC (`kube_persistentvolumeclaim_status_phase`, fournie par `kube-state-metrics`, toujours disponible), pas sur un seuil de remplissage en pourcentage.

Aucun récepteur externe (Slack, e-mail) n'est configuré pour Alertmanager — les alertes sont visibles dans l'interface Prometheus/Alertmanager, ce qui suffit pour un usage local de démonstration.
