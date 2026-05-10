FROM docker.io/andrewgaul/s3proxy:3.1.0

ENV S3PROXY_AUTHORIZATION=none \
    S3PROXY_ENDPOINT=http://0.0.0.0:9000 \
    JCLOUDS_PROVIDER=azureblob-sdk \
    JCLOUDS_ENDPOINT=https://sabankapiprd001.blob.core.windows.net \
    JCLOUDS_IDENTITY= \
    JCLOUDS_CREDENTIAL=

EXPOSE 9000