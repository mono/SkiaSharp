FROM amd64/debian:9

# Arguments:
#   DOTNET_SDK_VERSION   - the version of dotnet for the Cake script        [ 3.1.412 | * ]
#   CLANG_VERSION        - the version of clang/llvm tools                  [ 12 | * ]
#   TOOLCHAIN_VERSION    - the version of the GCC toolchain                 [ 6 | * ]
#   TOOLCHAIN_ARCH       - the architecture of the GCC toolchain            [ arm-linux-gnueabihf | aarch64-linux-gnu]
#   TOOLCHAIN_ARCH_SHORT - the short form architecture of the GCC toolchain [ armhf | arm64 ]
#   FONTCONFIG_VERSION   - the exact version of libfontconfig1 to use       [ 2.11.0-6.7+b1 | * ]
#
# To build a arm image:
#   --build-arg TOOLCHAIN_ARCH=arm-linux-gnueabihf --build-arg TOOLCHAIN_ARCH_SHORT=armhf
# To build a arm64 image:
#   --build-arg TOOLCHAIN_ARCH=aarch64-linux-gnu --build-arg TOOLCHAIN_ARCH_SHORT=arm64

# pre-requisites for building (python, git)
ARG CLANG_VERSION=12
RUN apt-get update \
    && apt-get install -y apt-transport-https curl wget python python3 git make dirmngr gnupg \
    && curl -L https://apt.llvm.org/llvm-snapshot.gpg.key | apt-key add - \
    && echo "deb http://apt.llvm.org/stretch/ llvm-toolchain-stretch-${CLANG_VERSION} main" | tee /etc/apt/sources.list.d/llvm.list \
    && apt-get update \
    && apt-get install -y clang-${CLANG_VERSION} \
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

# toolchain (gcc/g++)
ARG TOOLCHAIN_VERSION=6
ARG TOOLCHAIN_ARCH=arm-linux-gnueabihf
ARG TOOLCHAIN_ARCH_SHORT=armhf
RUN apt-get update \
    && apt-get install -y \
         libstdc++-${TOOLCHAIN_VERSION}-dev-${TOOLCHAIN_ARCH_SHORT}-cross \
         libgcc-${TOOLCHAIN_VERSION}-dev-${TOOLCHAIN_ARCH_SHORT}-cross \
         binutils-${TOOLCHAIN_ARCH} \
    && rm -rf /var/lib/apt/lists/*

# make it easier for the build script
RUN ln -s /usr/${TOOLCHAIN_ARCH}/include/c++/${TOOLCHAIN_VERSION}.* /usr/${TOOLCHAIN_ARCH}/include/c++/current \
    && sed -i "s/\/usr\/${TOOLCHAIN_ARCH}\/lib\///g" /usr/${TOOLCHAIN_ARCH}/lib/libpthread.so \
    && sed -i "s/\/usr\/${TOOLCHAIN_ARCH}\/lib\///g" /usr/${TOOLCHAIN_ARCH}/lib/libc.so

# skia dependencies (fontconfig)
ARG FONTCONFIG_VERSION=2.11.0-6.7+b1
RUN (mkdir -p /skia-utils/libfontconfig1-dev \
    && cd /skia-utils/libfontconfig1-dev \
    && wget -O libfontconfig1-dev.deb http://ftp.nl.debian.org/debian/pool/main/f/fontconfig/libfontconfig1-dev_${FONTCONFIG_VERSION}_${TOOLCHAIN_ARCH_SHORT}.deb \
    && ar vx libfontconfig1-dev.deb \
    && tar -xJvf data.tar.xz \
    && rm libfontconfig1-dev.deb \
    && cp -R usr/lib/*/* /usr/${TOOLCHAIN_ARCH}/lib/ \
    && cp -R usr/include/* /usr/${TOOLCHAIN_ARCH}/include/ )
RUN (mkdir -p /skia-utils/libfontconfig1 \
    && cd /skia-utils/libfontconfig1 \
    && wget -O libfontconfig1.deb http://ftp.nl.debian.org/debian/pool/main/f/fontconfig/libfontconfig1_${FONTCONFIG_VERSION}_${TOOLCHAIN_ARCH_SHORT}.deb \
    && ar vx libfontconfig1.deb \
    && tar -xJvf data.tar.xz \
    && rm libfontconfig1.deb \
    && cp -R usr/lib/*/* /usr/${TOOLCHAIN_ARCH}/lib/ )

# container environment
ENV CC=clang-${CLANG_VERSION} CXX=clang++-${CLANG_VERSION}

WORKDIR /work
