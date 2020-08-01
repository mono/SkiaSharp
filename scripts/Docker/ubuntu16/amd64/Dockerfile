FROM amd64/ubuntu:16.04

# Arguments:
#   TOOLCHAIN_VERSION    - the version of the GCC toolchain    [ 7 | * ]
#   CLANG_VERSION        - the version of the Clang compiler   [ 10 | * ]

ARG TOOLCHAIN_VERSION=7
ARG CLANG_VERSION=10

RUN apt-get update \
    && apt-get install -y apt-transport-https curl wget python git make \
    && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF \
    && echo "deb https://download.mono-project.com/repo/ubuntu stable-xenial/snapshots/6.4.0 main" | tee /etc/apt/sources.list.d/mono-official-stable.list \
    && curl -L https://apt.llvm.org/llvm-snapshot.gpg.key | apt-key add - \
    && echo "deb http://apt.llvm.org/xenial/ llvm-toolchain-xenial-${CLANG_VERSION} main" | tee /etc/apt/sources.list.d/llvm.list \
    && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 60C317803A41BA51845E371A1E9377A2BA9EF27F \
    && echo "deb http://ppa.launchpad.net/ubuntu-toolchain-r/test/ubuntu xenial main" | tee /etc/apt/sources.list.d/ubuntu-toolchain-r.list \
    && apt-get update \
    && apt-get install -y mono-devel libfontconfig1-dev clang-${CLANG_VERSION} gcc-${TOOLCHAIN_VERSION} g++-${TOOLCHAIN_VERSION} \
    && update-alternatives \
        --install /usr/bin/gcc gcc /usr/bin/gcc-${TOOLCHAIN_VERSION} 60 \
        --slave /usr/bin/g++ g++ /usr/bin/g++-${TOOLCHAIN_VERSION} \
        --slave /usr/bin/gcc-ar gcc-ar /usr/bin/gcc-ar-${TOOLCHAIN_VERSION} \
        --slave /usr/bin/gcc-nm gcc-nm /usr/bin/gcc-nm-${TOOLCHAIN_VERSION} \
        --slave /usr/bin/gcc-ranlib gcc-ranlib /usr/bin/gcc-ranlib-${TOOLCHAIN_VERSION} \
    && rm -rf /var/lib/apt/lists/*

ENV CC=clang-${CLANG_VERSION} CXX=clang++-${CLANG_VERSION}

WORKDIR /work
