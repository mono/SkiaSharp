FROM amd64/ubuntu:18.04

# Arguments:
#   MONO_VERSION         - the version of mono for the Cake script   [ 6.4.0 | * ]
#   CLANG_VERSION        - the version of the Clang compiler         [ 10 | * ]
#   TOOLCHAIN_VERSION    - the version of the GCC toolchain          [ 7 | * ]

ARG MONO_VERSION=6.4.0
ARG TOOLCHAIN_VERSION=7
ARG CLANG_VERSION=10

RUN apt-get update \
    && apt-get install -y apt-transport-https curl wget python git make \
        libfontconfig1-dev \
        libstdc++-${TOOLCHAIN_VERSION}-dev libgcc-${TOOLCHAIN_VERSION}-dev binutils \
        clang-${CLANG_VERSION} \
    && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF \
    && echo "deb https://download.mono-project.com/repo/ubuntu stable-bionic/snapshots/${MONO_VERSION} main" | tee /etc/apt/sources.list.d/mono-official-stable.list \
    && apt-get update \
    && apt-get install -y mono-devel \
    && rm -rf /var/lib/apt/lists/*

ENV CC=clang-${CLANG_VERSION} CXX=clang++-${CLANG_VERSION}

WORKDIR /work
