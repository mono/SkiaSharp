# Arguments:
#   DOTNET_SDK_VERSION   - the version of dotnet for the Cake script        [ 8.0 | * ]

FROM mcr.microsoft.com/dotnet-buildtools/prereqs:alpine-3.17-arm64v8-local

# Install the skia build dependencies
RUN apk add --no-cache fontconfig-dev gn ninja

# Install the .NET SDK
ARG DOTNET_SDK_VERSION=8.0
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT 1
RUN wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh \
    && bash dotnet-install.sh --channel ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet --verbose \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet-install.sh \
    && dotnet help

ENV CC=clang CXX=clang++ GN_EXE=/usr/bin/gn

WORKDIR /work
