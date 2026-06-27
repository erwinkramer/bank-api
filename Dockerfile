# Structure of this file is based of https://github.com/dotnet/dotnet-docker/blob/main/samples/aspnetapp/Dockerfile.alpine-composite
# For more information on the alpine composite image, see: https://github.com/richlander/container-workshop/issues/7
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:11.0-preview-alpine AS build
ARG TARGETARCH=x64
ARG SERVICENAME=BankApi.Service.Stable
ARG PUBLISHPROFILE=AlpineContainer

WORKDIR /source

COPY . .

# Copy the Zscaler certificates to the build image (the following only works on some distro's)
# Also see https://github.com/millermatt/osca?tab=readme-ov-file#operating-system-cert-management
RUN cat .certs/*.crt >> /etc/ssl/certs/ca-certificates.crt

# Restore and publish from the service project
RUN dotnet restore $SERVICENAME/$SERVICENAME.csproj -a $TARGETARCH \
	-p:PublishProfile=$PUBLISHPROFILE
WORKDIR /source/$SERVICENAME
RUN dotnet publish -c Release -a $TARGETARCH --no-restore \
	-p:PublishProfile=$PUBLISHPROFILE \
	-o /app

# Create a symlink to the service executable with a generic name, 
# so that the runtime image can use a common entry point
RUN ln -s /app/$SERVICENAME /app/app-entrypoint

# Enable globalization and time zones:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:11.0-preview-alpine-composite
EXPOSE 8080
WORKDIR /app
COPY --from=build /source/.certs/*.crt /usr/local/share/ca-certificates/
RUN cat /usr/local/share/ca-certificates/*.crt >> /etc/ssl/certs/ca-certificates.crt
COPY --from=build /app .

# Specifically for the MCP server. Keep this in until following issue is solved: https://github.com/abdebek/MCPify/issues/20
RUN mkdir -p /app/AuthTokens && chown -R $APP_UID:$APP_UID /app/AuthTokens

USER $APP_UID
ENTRYPOINT ["./app-entrypoint"]
