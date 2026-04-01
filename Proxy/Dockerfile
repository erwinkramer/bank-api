FROM envoyproxy/envoy:contrib-distroless-v1.37-latest

WORKDIR /tool

COPY . /tool/

CMD ["-c", "/tool/envoy-config.yaml"]