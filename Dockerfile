# Structure of this file is based of https://github.com/dotnet/dotnet-docker/blob/main/samples/aspnetapp/Dockerfile.alpine-composite
# For more information on the alpine composite image, see: https://github.com/richlander/container-workshop/issues/7
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG TARGETARCH=x64
WORKDIR /source

COPY . .

# Copy the Zscaler certificates to the build image (the following only works on some distro's)
# Also see https://github.com/millermatt/osca?tab=readme-ov-file#operating-system-cert-management
RUN cat .certs/*.crt >> /etc/ssl/certs/ca-certificates.crt

# Restore and publish from the service project
RUN dotnet restore BankApi.Service.Stable/BankApi.Service.Stable.csproj -a $TARGETARCH \
	-p:PublishProfile=AlpineContaine
WORKDIR /source/BankApi.Service.Stable
RUN dotnet publish -c Release -a $TARGETARCH --no-restore \
	-p:PublishProfile=AlpineContainer \
	-o /app

# Enable globalization and time zones:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md

# Runtime stage
FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0-alpine-composite
EXPOSE 8080
WORKDIR /app
COPY --from=build /source/.certs/*.crt /usr/local/share/ca-certificates/
RUN cat /usr/local/share/ca-certificates/*.crt >> /etc/ssl/certs/ca-certificates.crt
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["./BankApi.Service.Stable"]
