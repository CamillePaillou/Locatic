output "namespace" {
  description = "Namespace Kubernetes préparé pour l'application Locatic."
  value       = kubernetes_namespace_v1.locatic.metadata[0].name
}

output "sqlite_pvc_name" {
  description = "Nom de la PersistentVolumeClaim SQLite, à référencer dans le Deployment."
  value       = kubernetes_persistent_volume_claim_v1.sqlite.metadata[0].name
}
