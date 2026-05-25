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

# Use bash with strict error handling for all RUN commands.
SHELL ["/bin/bash", "-euo", "pipefail", "-c"]

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
# SHA-256 checksums computed from official GitHub releases (llvm/llvm-project).
ARG LLVM_VERSION=18.1.8
ARG MACHINE_ARCH=x86_64
ARG LLVM_SHA256_X86_64=54ec30358afcc9fb8aa74307db3046f5187f9fb89fb37064cdde906e062ebf36
ARG LLVM_SHA256_AARCH64=dcaa1bebbfbb86953fdfbdc7f938800229f75ad26c5c9375ef242edad737d999
RUN ln -sf /usr/lib64/libtinfo.so.6 /usr/lib64/libtinfo.so.5 \
    && ln -sf /usr/lib64/libncursesw.so.6 /usr/lib64/libncurses.so.5 \
    && case "${MACHINE_ARCH}" in \
         x86_64)  LLVM_SUFFIX="x86_64-linux-gnu-ubuntu-18.04" ; LLVM_SHA256="${LLVM_SHA256_X86_64}" ;; \
         aarch64) LLVM_SUFFIX="aarch64-linux-gnu"              ; LLVM_SHA256="${LLVM_SHA256_AARCH64}" ;; \
         *)       echo "Unsupported MACHINE_ARCH: ${MACHINE_ARCH}" && exit 1 ;; \
       esac \
    && curl -fSL -o /tmp/llvm.tar.xz \
        "https://github.com/llvm/llvm-project/releases/download/llvmorg-${LLVM_VERSION}/clang+llvm-${LLVM_VERSION}-${LLVM_SUFFIX}.tar.xz" \
    && echo "${LLVM_SHA256}  /tmp/llvm.tar.xz" | sha256sum -c - \
    && tar xJf /tmp/llvm.tar.xz --strip-components=1 -C /usr/local \
    && rm -f /tmp/llvm.tar.xz \
    && rm -rf /usr/local/share/doc /usr/local/share/man /usr/local/share/clang \
    && clang --version && ld.lld --version && llvm-ar --version

# Install the .NET SDK (needed for Cake build scripts).
ARG DOTNET_SDK_VERSION=10.0.108
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN curl -fsSL https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.sh -o dotnet-install.sh \
    && bash dotnet-install.sh --version ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet --verbose \
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
# SHA-256 of apk.static pinned for supply-chain integrity.
ARG DISTRO_VERSION=3.21
ARG APK_STATIC_SHA256_X86_64=cd282c693059c39b68ce0575fbdc4e0c162af85ed633a7834f9cfd01b8b1d58b
ARG APK_STATIC_SHA256_AARCH64=77ebdbb193aaa605e10610586d443909331872799ac78648dff1216822d76299
RUN . /etc/skia-env \
    && APK_DIR="$(mktemp -d)" \
    && case "${MACHINE_ARCH}" in \
         x86_64)  APK_STATIC_SHA256="${APK_STATIC_SHA256_X86_64}" ;; \
         aarch64) APK_STATIC_SHA256="${APK_STATIC_SHA256_AARCH64}" ;; \
       esac \
    && curl -fSL --retry 5 --retry-delay 10 --retry-all-errors \
        -o "$APK_DIR/apk.static" \
        "https://gitlab.alpinelinux.org/api/v4/projects/5/packages/generic/v2.12.14/$MACHINE_ARCH/apk.static" \
    && echo "${APK_STATIC_SHA256}  $APK_DIR/apk.static" | sha256sum -c - \
    && chmod +x "$APK_DIR/apk.static" \
    && mkdir -p /alpine/etc/apk/keys \
    && curl -fsSL -o /alpine/etc/apk/keys/alpine-devel@lists.alpinelinux.org-6165ee59.rsa.pub \
        "https://alpinelinux.org/keys/alpine-devel@lists.alpinelinux.org-6165ee59.rsa.pub" \
    && curl -fsSL -o /alpine/etc/apk/keys/alpine-devel@lists.alpinelinux.org-61666e3f.rsa.pub \
        "https://alpinelinux.org/keys/alpine-devel@lists.alpinelinux.org-61666e3f.rsa.pub" \
    && "$APK_DIR/apk.static" \
        -X "https://dl-cdn.alpinelinux.org/alpine/v$DISTRO_VERSION/main" \
        -X "https://dl-cdn.alpinelinux.org/alpine/v$DISTRO_VERSION/community" \
        -U --keys-dir /alpine/etc/apk/keys --root /alpine --arch "$APK_ARCH" --initdb add \
    && "$APK_DIR/apk.static" \
        -X "https://dl-cdn.alpinelinux.org/alpine/v$DISTRO_VERSION/main" \
        -X "https://dl-cdn.alpinelinux.org/alpine/v$DISTRO_VERSION/community" \
        -U --keys-dir /alpine/etc/apk/keys --root /alpine --arch "$APK_ARCH" --no-scripts \
        add musl-dev linux-headers fontconfig-dev libstdc++-dev gcc \
    && rm -rf "$APK_DIR"

# Create c++/current symlink so build.cake can find libstdc++ headers
# without hardcoding the GCC version number. Alpine installs headers at
# /alpine/usr/include/c++/<version>/ (e.g. 14.2.0).
RUN . /etc/skia-env \
    && INCLUDE_DIR=/alpine/usr/include/c++ \
    && if [ -d "${INCLUDE_DIR}" ]; then \
         GCC_VERSION=$(ls ${INCLUDE_DIR}/ | grep -E '^[0-9]' | sort -V | tail -1) ; \
         if [ -n "$GCC_VERSION" ]; then \
           ln -sf ${GCC_VERSION} ${INCLUDE_DIR}/current ; \
           echo "Created symlink: ${INCLUDE_DIR}/current -> ${GCC_VERSION}" ; \
           GCC_LIB_DIR="/alpine/usr/lib/gcc/${TOOLCHAIN_ARCH}/${GCC_VERSION}" ; \
           echo "export GCC_LIB_DIR=${GCC_LIB_DIR}" >> /etc/skia-env ; \
           echo "GCC lib dir: ${GCC_LIB_DIR}" ; \
         else \
           echo "ERROR: No GCC version directory found in ${INCLUDE_DIR}" && exit 1 ; \
         fi ; \
       else \
         echo "ERROR: ${INCLUDE_DIR} does not exist!" && exit 1 ; \
       fi

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
