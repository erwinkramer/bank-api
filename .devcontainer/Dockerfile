FROM mcr.microsoft.com/devcontainers/dotnet:1-9.0-bookworm

# Install Kiota as root
RUN dotnet tool install --global Microsoft.OpenApi.Kiota

# Set the user to vscode (1000:1000).
# Note that this user exists on the dotnet devcontainer image.
# USER vscode

# # Set the working directory
# WORKDIR /home/vscode
