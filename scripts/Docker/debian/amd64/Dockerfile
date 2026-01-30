# Arguments:
#   DOTNET_SDK_VERSION   - the version of dotnet for the Cake script        [ 8.0 | * ]
#   LLVM_VERSION         - the version of the LLVM compiler                 [ 13 | * ]

FROM amd64/debian:10

# Switch to archives
RUN sed -i s/deb.debian.org/archive.debian.org/g /etc/apt/sources.list && \
    sed -i s/security.debian.org/archive.debian.org/g /etc/apt/sources.list

# Install the required packages
ARG LLVM_VERSION=13
RUN apt-get update \
    && apt-get install -y \
        curl python3 git clang-${LLVM_VERSION} ninja-build \
        libfontconfig1-dev \
    && rm -rf /var/lib/apt/lists/*

# Install the .NET SDK
ARG DOTNET_SDK_VERSION=8.0
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN curl https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.sh -L -o dotnet-install.sh \
    && bash dotnet-install.sh --channel ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet --verbose \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet-install.sh \
    && dotnet help \
    && dotnet --info

ENV CC=clang-${LLVM_VERSION} CXX=clang++-${LLVM_VERSION}

WORKDIR /work
