FROM mcr.microsoft.com/vscode/devcontainers/dotnet:0-6.0-bullseye-slim

RUN apt-get update \
    && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends \
    && libfontconfig1-dev
