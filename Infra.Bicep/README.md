# Bicep demo

This folder shows how to run the stable version of the Bank API, including all sidecars, in Azure Container Apps, via Bicep.

## API deployment

The following reconciles the Azure Container Apps infrastructure, and generates a new revision for the Container App running the Bank API, using the Bicep template:

```bash
az login --tenant b81eb003-1c5c-45fd-848f-90d9d3f8d016
az deployment group create --resource-group containering --template-file ./main.bicep
```

For the live API with sidecars, the [GitHub workflow](/.github/workflows/main_bankapi-001.yml) leverages the same Bicep template.
