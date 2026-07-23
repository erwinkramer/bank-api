FROM ghcr.io/open-telemetry/opentelemetry-collector-releases/opentelemetry-collector-contrib:0.157.0-amd64

COPY otelcol.yaml /etc/otelcol-contrib/config.yaml

# https://github.com/open-telemetry/opentelemetry-collector-contrib/issues/42256#issuecomment-3685808950
CMD ["--config=/etc/otelcol-contrib/config.yaml", "--feature-gates=+extension.healthcheck.useComponentStatus"]
