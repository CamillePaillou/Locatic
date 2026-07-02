terraform {
  required_version = ">= 1.5"
  required_providers {
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.35"
    }
  }
}

provider "kubernetes" {
  config_path    = pathexpand(var.kubeconfig_path)
  config_context = var.kube_context
}

# Le socle minimal que Terraform doit préparer, d'après le sujet : le
# namespace applicatif et le stockage persistant SQLite. Le Deployment, le
# Service, Nginx et le monitoring sont posés ensuite par Ansible + les
# manifests Kubernetes, pas ici.
resource "kubernetes_namespace_v1" "locatic" {
  metadata {
    name = var.namespace
    labels = {
      project    = "locatic"
      managed-by = "terraform"
    }
  }
}

resource "kubernetes_persistent_volume_claim_v1" "sqlite" {
  metadata {
    name      = "locatic-sqlite-pvc"
    namespace = kubernetes_namespace_v1.locatic.metadata[0].name
    labels = {
      app = "locatic"
    }
  }

  spec {
    access_modes = ["ReadWriteOnce"]
    resources {
      requests = {
        storage = var.sqlite_storage_size
      }
    }
  }

  # La StorageClass par défaut de minikube (`standard`, provisioner
  # k8s.io/minikube-hostpath) utilise le mode "Immediate" : le volume est
  # lié tout de suite, pas besoin d'attendre qu'un pod le réclame. On garde
  # donc wait_until_bound à sa valeur par défaut (true) pour que `terraform
  # apply` vérifie vraiment que le volume s'est lié avant de continuer.
}
