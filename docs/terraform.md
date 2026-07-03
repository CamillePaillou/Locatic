# Terraform

## Rôle

Terraform prépare le **socle minimal** nécessaire dans le cluster Kubernetes local (minikube), avant tout déploiement applicatif : le namespace et le volume persistant pour la base SQLite. Rien de plus — le `Deployment`, le `Service`, la configuration Nginx et le monitoring sont posés ensuite par Ansible ([ansible.md](ansible.md)), pas par Terraform. Cette séparation est volontaire, pas un oubli.

Provider utilisé : [`hashicorp/kubernetes`](https://registry.terraform.io/providers/hashicorp/kubernetes) — Terraform pilote directement l'API Kubernetes de minikube, il ne crée ni VM ni conteneur Docker lui-même.

## Emplacement

```
infra/terraform/
├── main.tf           # provider + ressources
├── variables.tf
├── outputs.tf
└── terraform.tfvars   # valeurs locales (committé, ne contient aucun secret)
```

## Ressources gérées

| Ressource | Nom | Rôle |
|---|---|---|
| `kubernetes_namespace_v1` | `locatic` | Isole toutes les ressources applicatives du reste du cluster |
| `kubernetes_persistent_volume_claim_v1` | `locatic-sqlite-pvc` | Volume persistant (1Gi par défaut) monté par le pod applicatif sur `/data`, découplant les données SQLite du cycle de vie des pods |

## Variables

| Variable | Défaut | Rôle |
|---|---|---|
| `kubeconfig_path` | `~/.kube/config` | Emplacement du kubeconfig local |
| `kube_context` | `minikube` | Contexte kubectl ciblé |
| `namespace` | `locatic` | Nom du namespace créé |
| `sqlite_storage_size` | `1Gi` | Taille demandée pour la PVC |

## Outputs

| Output | Valeur | Consommé par |
|---|---|---|
| `namespace` | Nom du namespace créé | Ansible (`terraform output -json`), pour cibler les ressources qu'il applique ensuite |
| `sqlite_pvc_name` | Nom de la PVC créée | Ansible, injecté dans le `Deployment` de l'application comme référence de volume |

Vérifier ces outputs :
```bash
cd infra/terraform
terraform output               # format texte
terraform output -raw namespace   # une valeur seule
terraform output -json            # format structuré, celui qu'utilise Ansible
```

## Gestion de l'état

L'état (`terraform.tfstate`, `.terraform/`) est **local** et **jamais committé** — exclu via `.gitignore` à la racine du dépôt. Aucun backend distant n'est configuré : ce projet ne cible qu'une seule machine (celle du développeur), un état local suffit et évite d'introduire une dépendance externe pour un usage strictement local.

Le fichier de verrouillage des providers (`.terraform.lock.hcl`) est, lui, **committé** — il garantit que la même version du provider `kubernetes` est utilisée à chaque `terraform init`, sur n'importe quelle machine.

## Initialiser, planifier, appliquer

```bash
cd infra/terraform
terraform init      # télécharge le provider, une seule fois (ou après changement de version)
terraform fmt        # formatage standard
terraform validate   # vérifie la syntaxe
terraform plan        # prévisualise les changements
terraform apply        # applique réellement

# Vérification indépendante de Terraform
kubectl get namespace locatic
kubectl get pvc -n locatic
```

Nettoyage complet (supprime le namespace et la PVC — **détruit la base SQLite si elle existe encore dans le volume**) :
```bash
terraform destroy
```
