FROM amd64/debian:9

# Arguments:
#   DOTNET_SDK_VERSION   - the version of dotnet for the Cake script        [ 3.1.412 | * ]
#   CLANG_VERSION        - the version of clang/llvm tools                  [ 12 | * ]
#   TOOLCHAIN_VERSION    - the version of the GCC toolchain                 [ 6 | * ]

ARG CLANG_VERSION=12
ARG TOOLCHAIN_VERSION=6
RUN apt-get update \
    && apt-get install -y apt-transport-https curl wget python python3 git make dirmngr gnupg \
    && curl -L https://apt.llvm.org/llvm-snapshot.gpg.key | apt-key add - \
    && echo "deb http://apt.llvm.org/stretch/ llvm-toolchain-stretch-${CLANG_VERSION} main" | tee /etc/apt/sources.list.d/llvm.list \
    && apt-get update \
    && apt-get install -y libfontconfig1-dev gcc-${TOOLCHAIN_VERSION} g++-${TOOLCHAIN_VERSION} clang-${CLANG_VERSION} \
    && rm -rf /var/lib/apt/lists/*

ARG DOTNET_SDK_VERSION=3.1.412

RUN wget -O dotnet.tar.gz https://dotnetcli.azureedge.net/dotnet/Sdk/$DOTNET_SDK_VERSION/dotnet-sdk-$DOTNET_SDK_VERSION-linux-x64.tar.gz \
    && dotnet_sha512='1ed0c1ab48723cef834906a55fb1889b29dd810cd2bc66cbd345a0baf8a2796045b5b7f4beef3c48bd56bef3ffed690b6fae4a5f017ad8687817b25a76fbd9be' \
    && echo "$dotnet_sha512  dotnet.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -C /usr/share/dotnet -xzf dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet.tar.gz

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT 1

RUN dotnet help

ENV CC=clang-${CLANG_VERSION} CXX=clang++-${CLANG_VERSION}

WORKDIR /work
