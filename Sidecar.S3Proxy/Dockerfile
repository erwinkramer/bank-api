FROM ghcr.io/gaul/s3proxy/container:4.2.0

# using an agent for OTel. Native OTel is not implemented in the S3Proxy container yet
# see current attempt: https://github.com/gaul/s3proxy/pull/1276
COPY --from=ghcr.io/open-telemetry/opentelemetry-operator/autoinstrumentation-java:2.34.0 \
    /javaagent.jar /opt/otel/opentelemetry-javaagent.jar

ENV S3PROXY_AUTHORIZATION=none \
    S3PROXY_ENDPOINT=http://0.0.0.0:6070 \
    JCLOUDS_PROVIDER=azureblob-sdk \
    JCLOUDS_ENDPOINT=https://sabankapiprd001.blob.core.windows.net \
    JCLOUDS_IDENTITY= \
    JCLOUDS_CREDENTIAL=

ENV S3PROXY_JAVA_OPTS=-javaagent:/opt/otel/opentelemetry-javaagent.jar \
    OTEL_SERVICE_NAME=s3proxy

EXPOSE 6070
