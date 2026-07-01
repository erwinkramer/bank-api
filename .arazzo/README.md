# Arazzo

This folder contains a basic [Arazzo](https://spec.openapis.org/arazzo/latest.html) workflow for some operations in the Bank API.

## Visualize

Navigate to the [Arazzo UI for the Bank API](https://arazzo-ui.jentic.com/?document=https%3A%2F%2Fraw.githubusercontent.com%2Ferwinkramer%2Fbank-api%2Frefs%2Fheads%2Fmain%2F.arazzo%2Fv1_arazzo.yaml) to visualize the workflow.

## Execute

Uses [Arazzo Runner](https://github.com/jentic/arazzo-engine/blob/main/runner/README.md) with the live API, via [run.py](run.py).

If using Zscaler, first set the right CA Bundle:

```powershell
$env:REQUESTS_CA_BUNDLE = "$PWD\.certs\ZscalerRootCA.crt"
```

Then, start the runner:

```powershell
$env:BANK_APIKEY_HEADER = "Lifetime Subscription"

python -m pip install arazzo-runner
python .\.arazzo\run.py .\.arazzo\v1_arazzo.yaml
```
