FROM docker.io/daprio/daprd:1.18.4-stablecomponents

COPY components /components

CMD ["./daprd", "--app-id", "bank-api", "--dapr-grpc-port", "50002", "--resources-path", "/components"]