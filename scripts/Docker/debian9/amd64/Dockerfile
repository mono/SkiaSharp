FROM amd64/debian:9

# Arguments:
#   MONO_VERSION         - the version of mono for the Cake script          [ 6.4.0 | * ]
#   CLANG_VERSION        - the version of clang/llvm tools                  [ 10 | * ]
#   TOOLCHAIN_VERSION    - the version of the GCC toolchain                 [ 6 | * ]

ARG MONO_VERSION=6.4.0
ARG CLANG_VERSION=10
ARG TOOLCHAIN_VERSION=6
RUN apt-get update \
    && apt-get install -y apt-transport-https curl wget python git make \
    && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF \
    && echo "deb https://download.mono-project.com/repo/debian stable-stretch/snapshots/${MONO_VERSION} main" | tee /etc/apt/sources.list.d/mono-official-stable.list \
    && curl -L https://apt.llvm.org/llvm-snapshot.gpg.key | apt-key add - \
    && echo "deb http://apt.llvm.org/stretch/ llvm-toolchain-stretch-${CLANG_VERSION} main" | tee /etc/apt/sources.list.d/llvm.list \
    && apt-get update \
    && apt-get install -y mono-devel libfontconfig1-dev gcc-${TOOLCHAIN_VERSION} g++-${TOOLCHAIN_VERSION} clang-${CLANG_VERSION} \
    && rm -rf /var/lib/apt/lists/*

ENV CC=clang-${CLANG_VERSION} CXX=clang++-${CLANG_VERSION}

WORKDIR /work
