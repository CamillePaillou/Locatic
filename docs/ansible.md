# Ansible

## Rôle

Ansible est l'**orchestrateur** entre l'infrastructure préparée par Terraform et le déploiement applicatif. Il tourne entièrement **en local**, depuis le poste du développeur (`connection: local`, cible `localhost`) — il ne configure aucune machine distante, il pilote `terraform`, `kubectl` (via le module `kubernetes.core.k8s`) et `helm` (via `kubernetes.core.helm`) sur la machine où il s'exécute.

C'est un choix différent d'un usage "classique" d'Ansible (configurer des serveurs via SSH) : ici, il n'y a qu'un seul hôte (la machine locale), et le travail consiste à enchaîner des vérifications puis des appels d'API Kubernetes/Helm, pas à installer des paquets sur des machines distantes.

## Emplacement

```
infra/ansible/
├── ansible.cfg          # inventaire par défaut
├── inventory.yml        # localhost uniquement, connection: local
├── site.yml             # le playbook
├── vars/locatic.yml      # toutes les valeurs configurables du déploiement
└── files/
    └── grafana-dashboard-locatic.json   # dashboard Grafana "as code"
```

## Étapes orchestrées par `site.yml`

1. **Prérequis** — vérifie que `terraform`, `kubectl`, `minikube` et `helm` sont installés, et que minikube est démarré (`minikube status`). Échoue immédiatement et clairement si un outil manque, plutôt que de planter plus loin avec une erreur cryptique.

2. **Lecture des outputs Terraform** — exécute `terraform -chdir=../terraform output -json`, parse le JSON obtenu, et expose `namespace` et `sqlite_pvc_name` comme variables Ansible (`locatic_namespace`, `locatic_sqlite_pvc`). C'est le lien concret entre Terraform et Ansible : aucun de ces noms n'est recopié en dur dans les manifests Kubernetes.

3. **Identifiants GHCR et Grafana** — lus depuis les variables d'environnement (`GHCR_USERNAME`, `GHCR_TOKEN`, `GRAFANA_ADMIN_PASSWORD`), jamais stockés dans un fichier. Une assertion bloque le playbook avec un message clair si l'une d'elles manque, avant toute action.

4. **Stack de monitoring** — ajoute le dépôt Helm `prometheus-community` et installe/met à jour la release `kube-prometheus-stack` (Prometheus, Grafana, Alertmanager) dans le namespace `monitoring`, avec les valeurs nécessaires pour que le dashboard Locatic soit chargé et que Prometheus scrape les ressources du namespace `locatic` (voir [monitoring.md](monitoring.md)). Installée **avant** les manifests suivants, car ceux-ci incluent des ressources personnalisées (`PodMonitor`, `PrometheusRule`) qui n'existent dans le cluster qu'une fois ce chart posé.

5. **Rendu et application des manifests Kubernetes** — les fichiers `.yaml.j2` de `infra/kubernetes/templates/` (voir [kubernetes.md](kubernetes.md)) sont rendus avec les variables de `vars/locatic.yml` puis appliqués via `kubernetes.core.k8s`, en **deux temps explicitement ordonnés** :
   - d'abord les `ConfigMap`/`Secret`,
   - puis tout le reste (`Deployment`, `Service`, `PodMonitor`, `PrometheusRule`).

## Pourquoi cet ordre en deux temps

`find` ne garantit pas un ordre alphabétique. Appliquer un `Deployment` avant que le `ConfigMap` qu'il monte ne soit à jour peut faire démarrer un pod avec une configuration obsolète — observé concrètement en développement : le pod Nginx a démarré avec une version du `ConfigMap` ne contenant pas encore le bloc `/nginx_status`, et n'a jamais rechargé cette configuration tout seul. La correction : deux tâches `find` + `apply` distinctes, l'une pour les `ConfigMap`/`Secret`, l'autre pour le reste, dans un ordre fixé par le playbook lui-même — pas par l'ordre de retour de `find`.

## Dépendance aux outputs Terraform

Le playbook ne fonctionne que si `infra/terraform` a déjà été appliqué (le namespace et la PVC doivent exister). Si `terraform output -json` échoue (état vide), le playbook échoue dès l'étape 2, avant toute tentative de déploiement applicatif.

## Exécuter le playbook

```bash
cd infra/ansible
export GHCR_USERNAME=<username-github>
export GHCR_TOKEN=<token-avec-read:packages>
export GRAFANA_ADMIN_PASSWORD=<mot-de-passe-de-ton-choix>

ansible-playbook --syntax-check site.yml     # vérifier la syntaxe sans exécuter
ansible-playbook -i inventory.yml site.yml --list-tasks   # aperçu des tâches
ansible-playbook -i inventory.yml site.yml -v               # exécution réelle
```

Le playbook est idempotent : le relancer sans rien changer ne produit aucun `changed` (sauf pour la release Helm, qui peut afficher `changed` si le chart amont a une nouvelle version disponible).
