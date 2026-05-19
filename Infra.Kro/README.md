# kro demo

This folder shows how to run the Bank API using kro (Kube Resource Orchestrator).

## Install kro on cluster

```bash
helm install kro oci://registry.k8s.io/kro/charts/kro \
  --namespace kro-system \
  --create-namespace
```

## Validate kro manifests

You can validate structure using:

```bash
kubectl apply --dry-run=server -f bank-api-rgd.yaml
kubectl apply --dry-run=server -f bank-api-secret.yaml
kubectl apply --dry-run=server -f bank-api-instance.yaml
```

## Deploy later to cluster

```bash
kubectl apply -f bank-api-rgd.yaml
kubectl apply -f bank-api-secret.yaml
kubectl apply -f bank-api-instance.yaml
```

## Test

This is without the http route:

```bash
kubectl port-forward -n bank-api svc/bank-api 5201:80
```
