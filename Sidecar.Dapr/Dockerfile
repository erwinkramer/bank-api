FROM docker.io/daprio/daprd:1.17.9-stablecomponents

COPY components /components

CMD ["./daprd", "--app-id", "bank-api", "--dapr-grpc-port", "50002", "--resources-path", "/components"]