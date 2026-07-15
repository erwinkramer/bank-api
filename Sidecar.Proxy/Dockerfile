FROM envoyproxy/envoy:contrib-distroless-v1.39-latest

WORKDIR /envoy-config

COPY . /envoy-config

CMD ["-c", "/envoy-config/bootstrap.yaml", "-l", "debug"]