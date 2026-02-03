# Arguments:
#   DOTNET_SDK_VERSION   - the version of dotnet for the Cake script        [ 8.0 | * ]
#   LLVM_VERSION         - the version of the LLVM compiler                 [ 19 | * ]
#   TOOLCHAIN_VERSION    - the version of the GCC toolchain                 [ 14 | * ]
#   TOOLCHAIN_ARCH       - the architecture of the GCC toolchain            [ arm-linux-gnueabihf | aarch64-linux-gnu | riscv64-linux-gnu | loongarch64-linux-gnu ]
#   TOOLCHAIN_ARCH_SHORT - the short form architecture of the GCC toolchain [ armhf | arm64 | riscv64 | loong64 ]

# Use --platform flag when building to select architecture
FROM debian:13

# Set the architecture-specific variables based on the value of the BUILD_ARCH argument
ARG BUILD_ARCH=arm64
RUN case ${BUILD_ARCH} in \
        arm)         TOOLCHAIN_ARCH=arm-linux-gnueabihf       ; TOOLCHAIN_ARCH_SHORT=armhf   ; TOOLCHAIN_ARCH_TARGET=armv7a-linux-gnueabihf     ;; \
        arm64)       TOOLCHAIN_ARCH=aarch64-linux-gnu         ; TOOLCHAIN_ARCH_SHORT=arm64   ; TOOLCHAIN_ARCH_TARGET=aarch64-linux-gnu          ;; \
        loongarch64) TOOLCHAIN_ARCH=loongarch64-linux-gnu     ; TOOLCHAIN_ARCH_SHORT=loong64 ; TOOLCHAIN_ARCH_TARGET=loongarch64-linux-gnu      ;; \
        riscv64)     TOOLCHAIN_ARCH=riscv64-linux-gnu         ; TOOLCHAIN_ARCH_SHORT=riscv64 ; TOOLCHAIN_ARCH_TARGET=riscv64-linux-gnu          ;; \
        x86)         TOOLCHAIN_ARCH=i686-linux-gnu            ; TOOLCHAIN_ARCH_SHORT=i386    ; TOOLCHAIN_ARCH_TARGET=i686-linux-gnu             ;; \
        x64)         TOOLCHAIN_ARCH=x86-64-linux-gnu          ; TOOLCHAIN_ARCH_SHORT=amd64   ; TOOLCHAIN_ARCH_TARGET=x86_64-linux-gnu           ;; \
        *) echo "Unsupported architecture: ${BUILD_ARCH}" && exit 1 ;; \
    esac \
    && echo "export TOOLCHAIN_ARCH=${TOOLCHAIN_ARCH}" > /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_SHORT=${TOOLCHAIN_ARCH_SHORT}" >> /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_TARGET=${TOOLCHAIN_ARCH_TARGET}" >> /etc/skia-env

# Install the required packages
ARG LLVM_VERSION=19
RUN apt-get update \
    && apt-get install -y \
        curl python3 git clang-${LLVM_VERSION} ninja-build xz-utils \
    && rm -rf /var/lib/apt/lists/*

# Install the cross-compilation GCC toolchain
ARG TOOLCHAIN_VERSION=14
RUN . /etc/skia-env \
    && apt-get update \
    && apt-get install -y \
       libc6-dev-${TOOLCHAIN_ARCH_SHORT}-cross \
       libstdc++-${TOOLCHAIN_VERSION}-dev-${TOOLCHAIN_ARCH_SHORT}-cross \
       libgcc-${TOOLCHAIN_VERSION}-dev-${TOOLCHAIN_ARCH_SHORT}-cross \
       binutils-${TOOLCHAIN_ARCH} \
    && rm -rf /var/lib/apt/lists/*

# Make the script more flexible and use "current" instead of the actual version
# libpthread.so.0 for loong64 is in https://packages.debian.org/trixie/all/libc6-loong64-cross/filelist that use GCC 14 and Glibc 2.41 to programmed
RUN . /etc/skia-env \
    && TOOLCHAIN_ARCH=$(echo ${TOOLCHAIN_ARCH} | sed 's/x86-64/x86_64/g') \
    && ln -s /usr/${TOOLCHAIN_ARCH}/include/c++/${TOOLCHAIN_VERSION} /usr/${TOOLCHAIN_ARCH}/include/c++/current \
    && sed -i "s/\/usr\/${TOOLCHAIN_ARCH}\/lib\///g" /usr/${TOOLCHAIN_ARCH}/lib/libpthread.so.0 \
    && sed -i "s/\/usr\/${TOOLCHAIN_ARCH}\/lib\///g" /usr/${TOOLCHAIN_ARCH}/lib/libc.so

# Install the cross-compilation skia build dependencies (fontconfig)
# We need BOTH libfontconfig1 (runtime .so) and libfontconfig1-dev (headers and .a)
# The -dev package only has a broken symlink; the runtime package has the actual .so
RUN . /etc/skia-env \
    && mkdir -p /skia-utils/libfontconfig-dev \
    && cd /skia-utils/libfontconfig-dev \
    && case "${TOOLCHAIN_ARCH_SHORT}" in \
         loong64) FC_VERSION=2.17.1-3   ; FC_DEV_PKG=libfontconfig-dev  ; FC_RT_PKG=libfontconfig1 ; APT_REPO=http://deb.debian.org/debian ;; \
         riscv64) FC_VERSION=2.15.0-2.3 ; FC_DEV_PKG=libfontconfig-dev  ; FC_RT_PKG=libfontconfig1 ; APT_REPO=http://deb.debian.org/debian ;; \
         *)       FC_VERSION=2.13.1-2   ; FC_DEV_PKG=libfontconfig1-dev ; FC_RT_PKG=libfontconfig1 ; APT_REPO=http://deb.debian.org/debian ;; \
       esac \
    && TOOLCHAIN_ARCH=$(echo ${TOOLCHAIN_ARCH} | sed 's/x86-64/x86_64/g') \
    && FC_DEV_URL=${APT_REPO}/pool/main/f/fontconfig/${FC_DEV_PKG}_${FC_VERSION}_${TOOLCHAIN_ARCH_SHORT}.deb \
    && echo "Downloading fontconfig-dev from $FC_DEV_URL" \
    && curl $FC_DEV_URL -L -o libfontconfig-dev.deb \
    && ar vx libfontconfig-dev.deb \
    && tar -xJvf data.tar.xz \
    && cp -R usr/lib/*/* /usr/${TOOLCHAIN_ARCH}/lib/ \
    && cp -R usr/include/* /usr/${TOOLCHAIN_ARCH}/include/ \
    && rm -rf *.deb *.tar* debian-binary usr \
    && FC_RT_URL=${APT_REPO}/pool/main/f/fontconfig/${FC_RT_PKG}_${FC_VERSION}_${TOOLCHAIN_ARCH_SHORT}.deb \
    && echo "Downloading fontconfig runtime from $FC_RT_URL" \
    && curl $FC_RT_URL -L -o libfontconfig1.deb \
    && ar vx libfontconfig1.deb \
    && tar -xJvf data.tar.xz \
    && cp -R usr/lib/*/* /usr/${TOOLCHAIN_ARCH}/lib/ \
    && rm -rf *.deb *.tar* debian-binary usr

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

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
