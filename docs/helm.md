# Helm

## État actuel

Ce projet **n'inclut pas de chart Helm dédié à l'application Locatic** — le bonus "Chart Helm configurable et utilisé par Ansible" n'est pas réalisé pour la partie applicative. Le déploiement de Locatic (app, Nginx, ConfigMap, Secret, `PodMonitor`, `PrometheusRule`) utilise des manifests Kubernetes templatés en Jinja2, rendus et appliqués par Ansible via `kubernetes.core.k8s` — voir [ansible.md](ansible.md) et [kubernetes.md](kubernetes.md).

## Où Helm est réellement utilisé

Helm intervient pour un seul composant : la stack de monitoring **`kube-prometheus-stack`** (chart communautaire du dépôt `prometheus-community`), qui embarque Prometheus, Grafana, Alertmanager, `kube-state-metrics` et `node-exporter`. Ce chart est installé et mis à jour **depuis Ansible**, via le module `kubernetes.core.helm` — exactement le pattern que le sujet autorise : *"Si vous réalisez le bonus Helm, Ansible peut aussi lancer ou mettre à jour une release Helm."*

```yaml
# infra/ansible/site.yml (extrait)
- name: Installer/mettre à jour kube-prometheus-stack
  kubernetes.core.helm:
    name: kube-prometheus-stack
    chart_ref: prometheus-community/kube-prometheus-stack
    chart_version: "{{ monitoring_chart_version }}"
    release_namespace: "{{ monitoring_namespace }}"
    create_namespace: true
    values: { ... }
```

Ce choix a été délibéré : écrire à la main l'équivalent de `kube-prometheus-stack` (scrape de tout le cluster, `CustomResourceDefinition` pour `PodMonitor`/`ServiceMonitor`/`PrometheusRule`, dashboards Grafana intégrés) aurait représenté un travail disproportionné et une source d'erreurs, pour un résultat moins robuste qu'un chart mature et largement utilisé en production.

## Ce que demanderait le bonus complet

Pour obtenir pleinement ce bonus, il resterait à transformer le déploiement de Locatic lui-même en chart Helm :

1. Créer `infra/helm/locatic/` (`Chart.yaml`, `values.yaml`, `templates/`)
2. Migrer les templates Jinja2 actuels (`infra/kubernetes/templates/*.yaml.j2`) vers la syntaxe Helm native (`{{ .Values.x }}`, moteur Go template, au lieu de `{{ x }}` rendu par Ansible)
3. Migrer le contenu de `infra/ansible/vars/locatic.yml` vers `values.yaml`
4. Remplacer, dans `site.yml`, la logique actuelle (recherche des `.j2` puis application un par un) par un unique appel `kubernetes.core.helm` pointant vers ce chart local

Ce travail n'a pas été priorisé par rapport à la documentation et aux vérifications fonctionnelles du déploiement existant.
