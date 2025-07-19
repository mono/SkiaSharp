# Arguments:
#   DOTNET_SDK_VERSION   - the version of dotnet for the Cake script        [ 8.0 | * ]

FROM arm64v8/alpine:3.17

# Install the required packages
RUN apk add --no-cache \
        gn ninja clang python3 bash git build-base linux-headers \
        fontconfig-dev

# Install the .NET SDK
ARG DOTNET_SDK_VERSION=8.0
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN wget https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.sh -O dotnet-install.sh \
    && bash dotnet-install.sh --channel ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet --verbose \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet-install.sh \
    && dotnet help \
    && dotnet --info

ENV CC=clang CXX=clang++ GN_EXE=/usr/bin/gn

WORKDIR /work
