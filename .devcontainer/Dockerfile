ARG VARIANT=6.0
FROM mcr.microsoft.com/vscode/devcontainers/dotnet:0-${VARIANT}

RUN export DEBIAN_FRONTEND=noninteractive \
    && apt-get update \
    && apt-get install apt-transport-https dirmngr gnupg ca-certificates \
    && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF \
    && echo "deb https://download.mono-project.com/repo/debian stable-buster main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list \
    && apt-get update \
    && apt-get -y install --no-install-recommends \
    libfontconfig1 mono-complete
