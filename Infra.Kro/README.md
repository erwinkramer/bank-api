# kro demo

This folder shows how to run the Bank API using kro (Kube Resource Orchestrator).

## Rename secret file

Rename [bank-api-secret.yaml.sample](./bank-api-secret.yaml.sample) to `bank-api-secret.yaml` and replace the `AZURE_CLIENT_SECRET` value.

## Install kro on cluster

```bash
helm install kro oci://registry.k8s.io/kro/charts/kro \
  --namespace infra-kro \
  --create-namespace
```

## Validate kro manifests

You can validate structure using:

```bash
cd Infra.Kro
kubectl apply --dry-run=server -f bank-api-rgd.yaml
kubectl apply --dry-run=server -f bank-api-secret.yaml
kubectl apply --dry-run=server -f bank-api-instance.yaml
```

## Deploy to cluster

```bash
kubectl apply -f bank-api-rgd.yaml
kubectl apply -f bank-api-secret.yaml
kubectl apply -f bank-api-instance.yaml
```

Check status:

```bash
kubectl get pods -n infra-services -l app.kubernetes.io/name=bank-api
```

## Test

Without the `HTTPRoute`:

```bash
kubectl port-forward -n infra-services svc/bank-api 5201:80
```

With the `HTTPRoute`: <https://bankapi.w.guanchen.nl/scalar>

Renew pod:

```bash
kubectl delete pod -n infra-services -l app.kubernetes.io/name=bank-api
```

Get a token, via [Keycloak - Kubernetes identity provider](https://www.keycloak.org/docs/latest/server_admin/index.html#_identity_broker_kubernetes):

```bash
kubectl -n infra-services exec deploy/bank-api -c api -- sh -c '
TOKEN="$(tr -d "\r\n" < /var/run/secrets/bank-api/token)"

wget -O- \
  --header="Content-Type: application/x-www-form-urlencoded" \
  --post-data="grant_type=client_credentials&client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion=$TOKEN" \
  http://keycloak-service.infra-keycloak.svc.cluster.local:8080/realms/master/protocol/openid-connect/token
'
```
