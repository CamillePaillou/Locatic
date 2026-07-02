variable "kubeconfig_path" {
  type        = string
  description = "Chemin vers le kubeconfig local."
  default     = "~/.kube/config"
}

variable "kube_context" {
  type        = string
  description = "Contexte kubectl à utiliser (le cluster minikube local)."
  default     = "minikube"
}

variable "namespace" {
  type        = string
  description = "Namespace Kubernetes dédié à l'application Locatic."
  default     = "locatic"
}

variable "sqlite_storage_size" {
  type        = string
  description = "Taille du volume persistant utilisé par la base SQLite."
  default     = "1Gi"
}
