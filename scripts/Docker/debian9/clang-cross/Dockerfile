FROM amd64/debian:10

# Arguments:
#   DOTNET_SDK_VERSION   - the version of dotnet for the Cake script        [ 7.0 | * ]
#   CLANG_VERSION        - the version of clang/llvm tools                  [ 12 | * ]
#   TOOLCHAIN_VERSION    - the version of the GCC toolchain                 [ 8 | * ]
#   TOOLCHAIN_ARCH       - the architecture of the GCC toolchain            [ arm-linux-gnueabihf | aarch64-linux-gnu]
#   TOOLCHAIN_ARCH_SHORT - the short form architecture of the GCC toolchain [ armhf | arm64 ]
#   FONTCONFIG_VERSION   - the exact version of libfontconfig1 to use       [ 2.13.1-2 | * ]
#
# To build a arm image:
#   --build-arg TOOLCHAIN_ARCH=arm-linux-gnueabihf --build-arg TOOLCHAIN_ARCH_SHORT=armhf
# To build a arm64 image:
#   --build-arg TOOLCHAIN_ARCH=aarch64-linux-gnu --build-arg TOOLCHAIN_ARCH_SHORT=arm64

RUN apt-get update \
    && apt-get install -y apt-transport-https curl wget python python3 git make dirmngr gnupg \
    && apt-get install -y libfontconfig1-dev ninja-build

ARG TOOLCHAIN_VERSION=8
ARG TOOLCHAIN_ARCH=arm-linux-gnueabihf
ARG TOOLCHAIN_ARCH_SHORT=armhf
RUN apt-get update \
    && apt-get install -y \
       libstdc++-${TOOLCHAIN_VERSION}-dev-${TOOLCHAIN_ARCH_SHORT}-cross \
       libgcc-${TOOLCHAIN_VERSION}-dev-${TOOLCHAIN_ARCH_SHORT}-cross \
       binutils-${TOOLCHAIN_ARCH}

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

# make the script more flexible and use "current" instead of the actual version
RUN ln -s /usr/${TOOLCHAIN_ARCH}/include/c++/${TOOLCHAIN_VERSION} /usr/${TOOLCHAIN_ARCH}/include/c++/current \
    && sed -i "s/\/usr\/${TOOLCHAIN_ARCH}\/lib\///g" /usr/${TOOLCHAIN_ARCH}/lib/libpthread.so \
    && sed -i "s/\/usr\/${TOOLCHAIN_ARCH}\/lib\///g" /usr/${TOOLCHAIN_ARCH}/lib/libc.so

# skia dependencies (fontconfig)
ARG FONTCONFIG_VERSION=2.13.1-2
RUN (mkdir -p /skia-utils/libfontconfig1-dev \
    && cd /skia-utils/libfontconfig1-dev \
    && wget -O libfontconfig1-dev.deb http://deb.debian.org/debian/pool/main/f/fontconfig/libfontconfig1-dev_${FONTCONFIG_VERSION}_${TOOLCHAIN_ARCH_SHORT}.deb \
    && ar vx libfontconfig1-dev.deb \
    && tar -xJvf data.tar.xz \
    && rm libfontconfig1-dev.deb \
    && cp -R usr/lib/*/* /usr/${TOOLCHAIN_ARCH}/lib/ \
    && cp -R usr/include/* /usr/${TOOLCHAIN_ARCH}/include/ )
RUN (mkdir -p /skia-utils/libfontconfig1 \
    && cd /skia-utils/libfontconfig1 \
    && wget -O libfontconfig1.deb http://deb.debian.org/debian/pool/main/f/fontconfig/libfontconfig1_${FONTCONFIG_VERSION}_${TOOLCHAIN_ARCH_SHORT}.deb \
    && ar vx libfontconfig1.deb \
    && tar -xJvf data.tar.xz \
    && rm libfontconfig1.deb \
    && cp -R usr/lib/*/* /usr/${TOOLCHAIN_ARCH}/lib/ )

RUN rm -rf /var/lib/apt/lists/*

ENV CC=clang-${CLANG_VERSION} CXX=clang++-${CLANG_VERSION}

WORKDIR /work
