# Déploiement local

Procédure complète pour passer d'une image publiée sur GHCR à une application fonctionnelle sur minikube, depuis un poste local.

## Prérequis

Voir [../README.md](../README.md#prérequis-locaux) pour la liste des outils. En plus :
- Un token GitHub personnel (PAT classique) avec les scopes `read:packages` et `write:packages`, pour accéder au package GHCR privé `locatic`
- Le package GHCR doit être accessible par ton compte (lié au repo ou accès direct)

## 1. Démarrer minikube

```bash
minikube start
minikube status   # vérifier que tout est "Running"
```

## 2. Préparer le socle Kubernetes (Terraform)

```bash
cd infra/terraform
terraform init
terraform plan
terraform apply
```

Vérification indépendante :
```bash
kubectl get namespace locatic
kubectl get pvc -n locatic     # doit être à l'état "Bound"
```

## 3. Déployer l'application, Nginx et le monitoring (Ansible)

```bash
cd ../ansible

export GHCR_USERNAME=<ton-username-github>
export GHCR_TOKEN=<ton-token-avec-read:packages>
export GRAFANA_ADMIN_PASSWORD=<mot-de-passe-de-ton-choix>

ansible-playbook -i inventory.yml site.yml -v
```

Le package GHCR `locatic` est **privé** ; l'accès de minikube à l'image passe par un `Secret` Kubernetes construit à partir de ce token personnel fourni en variable d'environnement — jamais committé, jamais stocké dans un fichier du dépôt.

Ce playbook :
1. vérifie que les outils nécessaires sont installés et que minikube tourne
2. récupère le namespace et le nom de la PVC depuis Terraform
3. installe/met à jour `kube-prometheus-stack` (Prometheus, Grafana, Alertmanager)
4. applique le `Secret` GHCR, puis le `ConfigMap` Nginx, puis les `Deployment`/`Service`/`PodMonitor`/`PrometheusRule`

Voir [ansible.md](ansible.md) pour le détail de chaque étape.

## 4. Vérifier que tout tourne

```bash
kubectl get pods -n locatic
kubectl get pods -n monitoring
```

Tous les pods doivent être `Running` avec toutes leurs conteneurs `Ready` (`locatic-nginx` doit afficher `2/2` — Nginx + son sidecar exporter).

## 5. Accéder à l'application

```bash
kubectl port-forward -n locatic svc/locatic-nginx-svc 8081:80
```
→ http://localhost:8081

Ou, avec le driver Docker de minikube (macOS/Linux sans hyperviseur), en gardant le terminal ouvert :
```bash
minikube service locatic-nginx-svc -n locatic --url
```

## 6. Accéder au monitoring

```bash
kubectl port-forward -n monitoring svc/kube-prometheus-stack-grafana 3000:80     # Grafana
kubectl port-forward -n monitoring svc/kube-prometheus-stack-prometheus 9090:9090  # Prometheus
```

## Relancer après un changement

- **Changement de configuration applicative** (image, tag, replicas, ressources...) → modifier `infra/ansible/vars/locatic.yml`, relancer `ansible-playbook`. Aucun redémarrage de minikube ni de Terraform nécessaire.
- **Nouvelle image publiée sur `:latest`** → le tag ne change pas mais son contenu si ; forcer un nouveau pull :
  ```bash
  kubectl rollout restart deployment/locatic-app -n locatic
  ```
- **Changement d'infrastructure** (taille du volume, nom du namespace) → modifier `infra/terraform/terraform.tfvars`, relancer `terraform apply`, puis `ansible-playbook` (pour que les outputs mis à jour soient repris).

## Tout arrêter / nettoyer

```bash
cd infra/ansible
helm uninstall kube-prometheus-stack -n monitoring
kubectl delete namespace monitoring

cd ../terraform
terraform destroy   # supprime le namespace locatic et la PVC — détruit la base SQLite
```
