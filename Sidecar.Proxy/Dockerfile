FROM envoyproxy/envoy:contrib-distroless-v1.38-latest

WORKDIR /envoy-config

COPY . /envoy-config

CMD ["-c", "/envoy-config/bootstrap.yaml"]