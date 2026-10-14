FROM envoyproxy/envoy:contrib-distroless-v1.40-latest

WORKDIR /envoy-config

COPY . /envoy-config

CMD ["-c", "/envoy-config/bootstrap.yaml", "-l", "debug"]