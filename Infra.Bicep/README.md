# Bicep demo

This folder shows how to run both versions of the Bank API, including all sidecars, in Azure Container Apps, via Bicep.

## API deployment

The following reconciles the Azure Container Apps infrastructure, and generates a new revision for the App running the Bank API.

```bash
az login --tenant b81eb003-1c5c-45fd-848f-90d9d3f8d016
az deployment group create --resource-group containering --template-file ./main.bicep
```
