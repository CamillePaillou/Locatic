# Preuves d'exécution

## Git / GitHub

- [x] Règle de protection de branche `main` (Settings > Rules), montrant "Require a pull request", les checks obligatoires (`test`, `source-scan`), et le blocage suppression/force-push
- [x] Une Pull Request fusionnée, avec les checks CI verts

![Ruleset `main` : statut `Active`, cible la branche `main` ("Applies to 1 target: main")](branches/branches-ruleset-main-active.png)

![Détail des règles du ruleset : PR obligatoire, checks `test`/`source-scan` requis, blocage des suppressions et des force-push](branches/branches-ruleset-rules-detail.png)

![Paramètres Actions du dépôt : permissions du `GITHUB_TOKEN` en lecture/écriture et politique d'approbation des workflows pour les contributeurs](branches/branches-actions-workflow-permissions.png)

![PR #1 "docs: add architecture docs and execution proofs" prête à merger : les 4 checks obligatoires sont passés, statut "Ready to merge"](branches/branches-pr-ready-to-merge.png)

![PR #1 fusionnée ("Merged") dans `main` depuis la branche `docs/preuves-execution`](branches/branches-pr-merged.png)

![Run CI déclenché par le merge de la PR #1 : les 4 jobs `test`, `source-scan`, `build`, `publish` sont verts, statut Success en 8m32s](branches/branches-pr-merge-ci-success.png)

![Liste des exécutions de workflow sur `main`, toutes réussies](git/git-actions-all-workflows.png)

## CI/CD

- [x] Un run complet du pipeline GitHub Actions vert (les 4 jobs : `test`, `source-scan`, `build`, `publish`)
- [x] La page du package GHCR `locatic` montrant une image publiée

![Run "fix: add dashboard provider grafana" (#13) : les 4 jobs `test`, `source-scan`, `build`, `publish` sont verts, statut Success en 8m54s](git/git-ci-pipeline-run-success.png)

![`docker push` puis `docker pull` de `ghcr.io/camillepaillou/locatic:latest` avec le même digest, confirmant la publication de l'image](packageGHCR/ghcr-docker-push-pull.png)

![Page du package GHCR `locatic` : versions récentes publiées (`latest`, `f523d9f`, `f36a535`...), dernière publication il y a 5 minutes, 30 téléchargements au total](packageGHCR/ghcr-package-published-versions.png)

![Paramètres du package GHCR : accès Actions accordé au dépôt `Locatic` en écriture](packageGHCR/ghcr-package-settings-access.png)

## Terraform

```bash
cd infra/terraform && terraform apply
kubectl get namespace locatic
kubectl get pvc -n locatic
```

![`terraform apply` sans changement ("No changes"), puis `kubectl get namespace locatic` (Active) et `kubectl get pvc -n locatic` (PVC `locatic-sqlite-pvc` Bound, 1Gi)](terraform/terraform-apply-kubectl-get.png)

## Ansible

```bash
cd infra/ansible && ansible-playbook -i inventory.yml site.yml -v
```
- [x] Sortie complète d'une exécution réussie
- [x] Sortie montrant `changed=0` sur les tâches déjà appliquées (idempotence), et `changed=1` sur une ressource réellement modifiée (voir `docs/ansible.md`)

![`ansible-playbook --list-tasks` : liste complète des tâches du play (installation, lecture des outputs Terraform, secret GHCR, application des manifestes Kubernetes)](ansible/ansible-list-tasks.png)

![Exécution verbeuse (`-v`) : vérification des outils, lecture des outputs Terraform et affichage de la preuve du lien Terraform → Ansible (namespace et PVC récupérés)](ansible/ansible-run-verbose-terraform-outputs.png)

![Fin du run avec `changed=1` sur le service Nginx réellement mis à jour, PLAY RECAP `ok=10 changed=1 failed=0`](ansible/ansible-run-changed-1.png)

![Deuxième exécution du même playbook : PLAY RECAP `ok=7 changed=0 skipped=1` (idempotence — les manifestes Kubernetes ne sont pas encore présents donc l'étape est "skipped", mais aucune tâche déjà appliquée n'est "changed")](ansible/ansible-run-idempotent-changed-0.png)

## Kubernetes

```bash
kubectl get all -n locatic
kubectl get all -n monitoring
```
- [x] Tous les pods `Running`/`Ready`

![`kubectl get all -n locatic` et `kubectl get all -n monitoring` : tous les pods sont `Running` avec tous leurs conteneurs `Ready` (app, nginx, Prometheus, Grafana, Alertmanager, kube-state-metrics, node-exporter)](kubernetes/kubernetes-get-all-locatic-monitoring.png)

## Accès via Nginx

```bash
curl -i http://localhost:8081/
curl -i http://localhost:8081/health/live
curl -i http://localhost:8081/health/ready
```
- [x] Capture du navigateur montrant l'application accessible via le port-forward Nginx (page d'accueil stylée)

![Page d'accueil "Bienvenue sur Locatic" ouverte dans le navigateur via le port-forward Nginx, avec les liens Marques/Modèles/Voitures/Client/Booking](nginx/nginx-browser-homepage.png)

![`kubectl port-forward svc/locatic-nginx-svc 8081:80` puis `curl -i http://localhost:8081/` : réponse `200 OK` servie par `nginx/1.27.5` avec le HTML de la page d'accueil](nginx/nginx-portforward-curl-homepage.png)

![`curl -i http://localhost:8081/health/live` et `/health/ready` : deux réponses `200 OK` avec le corps `Healthy`](nginx/nginx-health-live-ready.png)

![Test antérieur via l'URL de service Minikube (`curl -i http://127.0.0.1:56427`) : même page d'accueil renvoyée en `200 OK`](nginx/nginx-curl-minikube-service.png)

## Persistance SQLite

- [x] `ls -la /data` avant/après suppression d'un pod applicatif, montrant que `locatic.db` garde la même taille/date

![Avant suppression : `locatic.db` fait 53248 octets, daté du 2 juillet 21:42. Le pod applicatif est ensuite supprimé (`kubectl delete pod`) et redémarre automatiquement](persistanceSQLite/sqlite-before-pod-delete.png)

![Après suppression et redémarrage du pod : `locatic.db` conserve exactement la même taille (53248 octets) et la même date (21:42), preuve de la persistance sur le volume ; `curl /health/ready` répond `Healthy`](persistanceSQLite/sqlite-after-pod-delete.png)

## Monitoring

- [x] Capture de `http://localhost:9090/targets` montrant `locatic/locatic-app` et `locatic/locatic-nginx` en `UP`
- [x] Capture du dashboard Grafana "Locatic - Vue d'ensemble" avec des données réelles sur les 6 panels
- [x] Capture de `http://localhost:9090/alerts` montrant les 3 règles chargées

![Page Prometheus "Status > Target health" (`/targets`) : `podMonitor/locatic/locatic-app` (2/2 up) et `podMonitor/locatic/locatic-nginx` (1/1 up), ainsi que les cibles du monitoring stack, toutes `UP`](prometheus/prometheus-targets-up.png)

![Confirmation en ligne de commande via l'API `/api/v1/targets` : `locatic/locatic-app` (×2) et `locatic/locatic-nginx` sont bien `up`](monitoring/monitoring-targets-up.png)

![Dashboard Grafana "Locatic - Vue d'ensemble" avec des données réelles sur les 6 panels : débit de requêtes app, latence P95, connexions et débit Nginx, état des pods, état du volume SQLite](grafana/grafana-dashboard-vue-ensemble.png)

![Page Prometheus `/alerts` : les groupes de règles `locatic-app` (2 règles), `locatic-storage` (1 règle) et `alertmanager.rules` sont bien chargés (toutes `INACTIVE`, donc pas d'alerte déclenchée)](prometheus/prometheus-alerts-rules-loaded.png)

![Métriques exposées par `nginx-exporter` sur `:9113/metrics` : connexions acceptées/actives/traitées et `nginx_up 1`](monitoring/monitoring-nginx-exporter-metrics.png)

![Port-forward vers Prometheus (`svc/kube-prometheus-stack-prometheus 9090:9090`) et chargement de la page `/targets` en HTML](monitoring/monitoring-prometheus-portforward.png)

![Endpoint `/metrics` de l'application (.NET) : métriques Prometheus `http_request_duration_seconds` par contrôleur/action/code](metrics/metrics-prometheus-endpoint.png)

![Grafana Drilldown > Metrics : exploration des métriques Prometheus disponibles (ex. `alertmanager_alerts`, `node_memory_MemAvailable_bytes`)](grafana/grafana-drilldown-metrics.png)
