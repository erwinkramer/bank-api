# kro demo

This folder shows how to run both versions of the Bank API, including all sidecars, in Kubernetes.

## API deployment

Using kro (Kube Resource Orchestrator) for deploying the API.

### Install kro on cluster

```bash
helm install kro oci://registry.k8s.io/kro/charts/kro \
  --namespace infra-kro \
  --create-namespace
```

### Deploy to cluster

First, rename the secret file [bank-api-secret.yaml.sample](./bank-api-secret.yaml.sample) to `bank-api-secret.yaml` and replace all occurrences of the text `REPLACE_ME`.

You can validate the deploy structure using:

```bash
cd Infra.Kro
kubectl apply --dry-run=server -f bank-api-rgd.yaml
kubectl apply --dry-run=server -f bank-api-secret.yaml
kubectl apply --dry-run=server -f bank-api-instance.yaml
```

Then deploy:

```bash
kubectl apply -f bank-api-rgd.yaml
kubectl apply -f bank-api-secret.yaml
kubectl apply -f bank-api-instance.yaml
```

Check deployment status:

```bash
kubectl get pods -n infra-services -l app.kubernetes.io/name=bank-api
```

Renew pod, useful if you want to pull the container images again:

```bash
kubectl delete pod -n infra-services -l app.kubernetes.io/name=bank-api
```

### Test

Without the `HTTPRoute`:

```bash
kubectl port-forward -n infra-services svc/bank-api 5201:80
```

With the `HTTPRoute`: <https://bankapi.w.guanchen.nl/v1/docs>

### Teardown

Teardown `kro` setup, in this order only:

```bash
kubectl delete -f bank-api-instance.yaml
kubectl delete -f bank-api-rgd.yaml
kubectl delete crd bankapis.kro.run
```

## Keycloak configuration

Because we're running in Kubernetes with a mounted service account token, the Bank API assumes it can do token assertion with Keycloak, as defined in [KubernetesServiceAccountExtensions.cs](../BankApi.Core/Defaults/Helper.KubernetesServiceAccountExtensions.cs). Following steps make sure that it works:

1. Configure a [Keycloak Kubernetes identity provider](https://www.keycloak.org/docs/latest/server_admin/index.html#_identity_broker_kubernetes), in a realm called `bank`, with issuer: `https://kubernetes.default.svc` and name `local-kubus`.

1. Configure an OpenID Connect client called `sa-bank-api`. Enable capabilities `Client authentication` and `Service account roles`.

1. Change in the `Credentials` tab of the client the `Client Authenticator` to `Signed JWT - Federated`, with provider `local-kubus` and subject `system:serviceaccount:infra-services:bank-api`.

1. As a sanity test, get a Keycloak token in exchange for the Kubernetes token, via the `api-stable` container:

```bash
kubectl -n infra-services exec deploy/bank-api -c api-stable -- sh -c \
'wget -qO- --header="Content-Type: application/x-www-form-urlencoded" --post-data="grant_type=client_credentials&client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion=$(cat /var/run/secrets/bank-api/token)" http://keycloak-service.infra-keycloak.svc:8080/realms/bank/protocol/openid-connect/token'
```
