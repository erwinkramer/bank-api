FROM ghcr.io/dapr/daprd:1.19.1-stablecomponents

COPY components /components

CMD ["./daprd", "--app-id", "bank-api", "--dapr-grpc-port", "50002", "--resources-path", "/components"]