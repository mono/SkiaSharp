# Arguments:
#   DOTNET_SDK_VERSION   - the version of dotnet for the Cake script        [ 8.0 | * ]

FROM mcr.microsoft.com/dotnet-buildtools/prereqs:debian-11-amd64

# Install the skia build dependencies
RUN apt-get update \
    && apt-get install -y libfontconfig1-dev ninja-build \
    && rm -rf /var/lib/apt/lists/*

# Install the .NET SDK
ARG DOTNET_SDK_VERSION=8.0
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT 1
RUN curl https://dot.net/v1/dotnet-install.sh -L -o dotnet-install.sh \
    && bash dotnet-install.sh --channel ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet --verbose \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet-install.sh \
    && dotnet help

ENV CC=clang CXX=clang++

WORKDIR /work
