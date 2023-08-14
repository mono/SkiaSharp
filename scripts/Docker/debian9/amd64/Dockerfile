FROM amd64/debian:10

# Arguments:
#   DOTNET_SDK_VERSION   - the version of dotnet for the Cake script        [ 7.0 | * ]
#   CLANG_VERSION        - the version of clang/llvm tools                  [ 12 | * ]
#   TOOLCHAIN_VERSION    - the version of the GCC toolchain                 [ 8 | * ]

RUN apt-get update \
    && apt-get install -y apt-transport-https curl wget python python3 git make dirmngr gnupg \
    && apt-get install -y libfontconfig1-dev ninja-build

ARG TOOLCHAIN_VERSION=8
RUN apt-get update \
    && apt-get install -y gcc-${TOOLCHAIN_VERSION} g++-${TOOLCHAIN_VERSION}
RUN gcc-${TOOLCHAIN_VERSION} --version

ARG CLANG_VERSION=12
RUN curl -L https://apt.llvm.org/llvm-snapshot.gpg.key | apt-key add - \
    && echo "deb http://apt.llvm.org/buster/ llvm-toolchain-buster-${CLANG_VERSION} main" | tee /etc/apt/sources.list.d/llvm.list \
    && apt-get update \
    && apt-get install -y clang-${CLANG_VERSION}
RUN clang-${CLANG_VERSION} --version

ARG DOTNET_SDK_VERSION=7.0
RUN wget https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && rm packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y dotnet-sdk-${DOTNET_SDK_VERSION}
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT 1
RUN dotnet --version

RUN rm -rf /var/lib/apt/lists/*

ENV CC=clang-${CLANG_VERSION} CXX=clang++-${CLANG_VERSION}

WORKDIR /work
