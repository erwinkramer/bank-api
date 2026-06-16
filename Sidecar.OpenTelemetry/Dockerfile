FROM ghcr.io/open-telemetry/opentelemetry-collector-releases/opentelemetry-collector-contrib:0.154.0-amd64

COPY otelcol.yaml /etc/otelcol-contrib/config.yaml

CMD ["--feature-gates=+extension.healthcheck.useComponentStatus"]