# musl (Alpine) cross-compilation image.
#
# Uses apk.static to create an Alpine sysroot, same concept as glibc image.
# Host is Azure Linux 3.0; sysroot is Alpine 3.21 for musl target.
#
# musl is forward-compatible: binaries built against 3.21 run on 3.22+.
# musl is NOT backward-compatible: cannot target 3.17 from 3.21 headers.
#
# Alpine 3.21 (musl 1.2.5) is the minimum supported — 3.17-3.20 are all EOL.
# .NET 10 officially supports only Alpine 3.21+.

FROM mcr.microsoft.com/azurelinux/base/core:3.0

# Install host build tools from Azure Linux packages.
# NOTE: We do NOT install clang/lld/llvm from tdnf because the Azure Linux
# LLVM package only includes host+AMDGPU+BPF targets. We need ARM, AArch64,
# RISCV, LoongArch, and X86 backends for cross-compilation.
RUN tdnf install -y \
        ninja-build \
        python3 \
        git \
        curl \
        tar \
        xz \
        gawk \
        ca-certificates \
        binutils \
        libxml2 \
        ncurses-libs \
        zlib \
        libffi \
    && tdnf clean all

# Install official LLVM 18 release with all target backends.
# Azure Linux's LLVM only has "host;AMDGPU;BPF" — we need all cross targets.
ARG LLVM_VERSION=18.1.8
RUN curl -sL "https://github.com/llvm/llvm-project/releases/download/llvmorg-${LLVM_VERSION}/clang+llvm-${LLVM_VERSION}-x86_64-linux-gnu-ubuntu-18.04.tar.xz" \
        | tar xJ --strip-components=1 -C /usr/local \
    && rm -rf /usr/local/share/doc /usr/local/share/man /usr/local/share/clang \
    && clang --version && lld --version

# Install the .NET SDK (needed for Cake build scripts)
ARG DOTNET_SDK_VERSION=10.0.1xx
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN curl https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.sh -L -o dotnet-install.sh \
    && bash dotnet-install.sh --channel ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet --verbose \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet-install.sh \
    && dotnet help \
    && dotnet --info

ENV CC=clang CXX=clang++

ARG BUILD_ARCH=arm64

# Map BUILD_ARCH to Alpine triple and apk architecture
RUN case ${BUILD_ARCH} in \
        arm)         TOOLCHAIN_ARCH=armv7-alpine-linux-musleabihf     ; TOOLCHAIN_ARCH_TARGET=armv7-alpine-linux-musleabihf     ; APK_ARCH=armv7       ;; \
        arm64)       TOOLCHAIN_ARCH=aarch64-alpine-linux-musl         ; TOOLCHAIN_ARCH_TARGET=aarch64-alpine-linux-musl         ; APK_ARCH=aarch64     ;; \
        loongarch64) TOOLCHAIN_ARCH=loongarch64-alpine-linux-musl     ; TOOLCHAIN_ARCH_TARGET=loongarch64-alpine-linux-musl     ; APK_ARCH=loongarch64 ;; \
        riscv64)     TOOLCHAIN_ARCH=riscv64-alpine-linux-musl         ; TOOLCHAIN_ARCH_TARGET=riscv64-alpine-linux-musl         ; APK_ARCH=riscv64     ;; \
        x86)         TOOLCHAIN_ARCH=i586-alpine-linux-musl            ; TOOLCHAIN_ARCH_TARGET=i586-alpine-linux-musl            ; APK_ARCH=x86         ;; \
        x64)         TOOLCHAIN_ARCH=x86_64-alpine-linux-musl          ; TOOLCHAIN_ARCH_TARGET=x86_64-alpine-linux-musl          ; APK_ARCH=x86_64      ;; \
        *) echo "Unsupported architecture: ${BUILD_ARCH}" && exit 1 ;; \
    esac \
    && echo "export TOOLCHAIN_ARCH=${TOOLCHAIN_ARCH}" > /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_TARGET=${TOOLCHAIN_ARCH_TARGET}" >> /etc/skia-env \
    && echo "export APK_ARCH=${APK_ARCH}" >> /etc/skia-env \
    && echo "export SYSROOT_PATH=/alpine/usr" >> /etc/skia-env

# Create Alpine sysroot using apk.static.
# Only install the minimal packages needed for cross-compilation:
#   musl-dev: C library headers and runtime
#   linux-headers: kernel headers
#   fontconfig-dev: font configuration headers and library
#   libstdc++-dev: C++ standard library headers (needed by Skia)
# Avoids build-base which pulls in busybox (~35 CVEs) and binutils (~15 CVEs).
ARG DISTRO_VERSION=3.21
ARG MACHINE_ARCH=x86_64
RUN . /etc/skia-env \
    && APK_DIR="$(mktemp -d)" \
    && curl -SLO --fail --retry 5 --retry-delay 10 --retry-all-errors \
        --create-dirs --output-dir "$APK_DIR" \
        "https://gitlab.alpinelinux.org/api/v4/projects/5/packages/generic/v2.12.14/$MACHINE_ARCH/apk.static" \
    && chmod +x "$APK_DIR/apk.static" \
    && "$APK_DIR/apk.static" \
        -X "http://dl-cdn.alpinelinux.org/alpine/v$DISTRO_VERSION/main" \
        -X "http://dl-cdn.alpinelinux.org/alpine/v$DISTRO_VERSION/community" \
        -U --allow-untrusted --root /alpine --arch "$APK_ARCH" --initdb add \
    && "$APK_DIR/apk.static" \
        -X "http://dl-cdn.alpinelinux.org/alpine/v$DISTRO_VERSION/main" \
        -X "http://dl-cdn.alpinelinux.org/alpine/v$DISTRO_VERSION/community" \
        -U --allow-untrusted --root /alpine --arch "$APK_ARCH" --no-scripts \
        add musl-dev linux-headers fontconfig-dev libstdc++-dev \
    && rm -rf "$APK_DIR"

# Create c++/current symlink so build.cake can find libstdc++ headers
# without hardcoding the GCC version number. Alpine installs headers at
# /alpine/usr/include/c++/<version>/ (e.g. 14.2.0).
RUN . /etc/skia-env \
    && INCLUDE_DIR=/alpine/usr/include/c++ \
    && if [ -d "${INCLUDE_DIR}" ]; then \
         GCC_VERSION=$(ls ${INCLUDE_DIR}/ | grep -E '^[0-9]' | head -1) ; \
         if [ -n "$GCC_VERSION" ]; then \
           ln -sf ${GCC_VERSION} ${INCLUDE_DIR}/current ; \
           echo "Created symlink: ${INCLUDE_DIR}/current -> ${GCC_VERSION}" ; \
         fi ; \
       fi

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
