# Arguments:
#   IMAGE_ARCH           - the architecture of the image                    [ amd64 | arm64v8 | riscv64 ]
#   DOTNET_SDK_VERSION   - the version of dotnet for the Cake script        [ 8.0 | * ]
#   LLVM_VERSION         - the version of the LLVM compiler                 [ 13 | * ]
#   TOOLCHAIN_VERSION    - the version of the GCC toolchain                 [ 9 | * ]
#   TOOLCHAIN_ARCH       - the architecture of the GCC toolchain            [ arm-linux-gnueabihf | aarch64-linux-gnu | riscv64-linux-gnu ]
#   TOOLCHAIN_ARCH_SHORT - the short form architecture of the GCC toolchain [ armhf | arm64 | riscv64 ]

ARG IMAGE_ARCH=amd64
FROM ${IMAGE_ARCH}/debian:10

# Install the required packages
ARG LLVM_VERSION=13
RUN apt-get update \
    && apt-get install -y \
        curl python3 git clang-${LLVM_VERSION} ninja-build xz-utils \
    && rm -rf /var/lib/apt/lists/*

# Install the cross-compilation GCC toolchain
ARG TOOLCHAIN_VERSION=8
ARG TOOLCHAIN_ARCH=arm-linux-gnueabihf
ARG TOOLCHAIN_ARCH_SHORT=armhf
RUN apt-get update \
    && apt-get install -y \
       libstdc++-${TOOLCHAIN_VERSION}-dev-${TOOLCHAIN_ARCH_SHORT}-cross \
       libgcc-${TOOLCHAIN_VERSION}-dev-${TOOLCHAIN_ARCH_SHORT}-cross \
       binutils-${TOOLCHAIN_ARCH} \
    && rm -rf /var/lib/apt/lists/*

# Make the script more flexible and use "current" instead of the actual version
RUN TOOLCHAIN_ARCH=$(echo ${TOOLCHAIN_ARCH} | sed 's/x86-64/x86_64/g') \
    && ln -s /usr/${TOOLCHAIN_ARCH}/include/c++/${TOOLCHAIN_VERSION} /usr/${TOOLCHAIN_ARCH}/include/c++/current \
    && sed -i "s/\/usr\/${TOOLCHAIN_ARCH}\/lib\///g" /usr/${TOOLCHAIN_ARCH}/lib/libpthread.so \
    && sed -i "s/\/usr\/${TOOLCHAIN_ARCH}\/lib\///g" /usr/${TOOLCHAIN_ARCH}/lib/libc.so

# Install the cross-compilation skia build dependencies (fontconfig)
RUN set -ex \
    && mkdir -p /skia-utils/libfontconfig-dev \
    && cd /skia-utils/libfontconfig-dev \
    && case "${TOOLCHAIN_ARCH_SHORT}" in \
         riscv64) FC_VERSION=2.15.0-2 ; FC_PKG=libfontconfig-dev  ;; \
         *)       FC_VERSION=2.13.1-2 ; FC_PKG=libfontconfig1-dev ;; \
       esac \
    && curl http://deb.debian.org/debian/pool/main/f/fontconfig/${FC_PKG}_${FC_VERSION}_${TOOLCHAIN_ARCH_SHORT}.deb -L -o libfontconfig-dev.deb \
    && ar vx libfontconfig-dev.deb \
    && tar -xJvf data.tar.xz \
    && rm libfontconfig-dev.deb \
    && TOOLCHAIN_ARCH=$(echo ${TOOLCHAIN_ARCH} | sed 's/x86-64/x86_64/g') \
    && cp -R usr/lib/*/* /usr/${TOOLCHAIN_ARCH}/lib/ \
    && cp -R usr/include/* /usr/${TOOLCHAIN_ARCH}/include/

# Install the .NET SDK
ARG DOTNET_SDK_VERSION=8.0
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN curl https://dot.net/v1/dotnet-install.sh -L -o dotnet-install.sh \
    && bash dotnet-install.sh --channel ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet --verbose \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet-install.sh \
    && dotnet help \
    && dotnet --info

ENV CC=clang-${LLVM_VERSION} CXX=clang++-${LLVM_VERSION}

WORKDIR /work
