# glibc cross-compilation image.
#
# Uses debootstrap to create a Debian sysroot, then compiles with --sysroot.
# The sysroot's glibc version determines the minimum target distro.
# The host's glibc version is IRRELEVANT — only the sysroot matters.
#
# Architecture:
#   Azure Linux 3.0 (host) → clang-18 → --sysroot=/sysroot → libSkiaSharp.so
#
# Replaces: debian/11/Dockerfile (glibc 2.31) and debian/13/Dockerfile (glibc 2.38)
#
# | SYSROOT_RELEASE | glibc | libstdc++ | Supported distros                    |
# |-----------------|-------|-----------|--------------------------------------|
# | bullseye (11)   | 2.31  | GCC 10    | Debian 11+, Ubuntu 20.04+, RHEL 9+  |
# | trixie (13)     | 2.38  | GCC 14    | Debian 13+, Ubuntu 24.04+           |
#
# Bullseye is the oldest with GCC 10 libstdc++ (C++20: std::span, std::countl_zero).
#
# Supported architectures: arm, arm64, x86, x64, riscv64
# For loongarch64, see ../loongarch64/Dockerfile (cross-package extraction).

FROM mcr.microsoft.com/azurelinux/base/core:3.0

# Use bash with strict error handling for all RUN commands.
# -e: exit on error, -u: error on undefined vars, -o pipefail: pipe failures propagate.
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
        debootstrap \
        libxml2 \
        ncurses-libs \
        zlib \
        libffi \
    && tdnf clean all

# Install official LLVM 18 release with all target backends.
# Azure Linux's LLVM only has "host;AMDGPU;BPF" — we need all cross targets.
# The release binary needs libtinfo.so.5 (built on Ubuntu 18.04) but Azure Linux
# only has libtinfo.so.6 — create a compat symlink (ABI compatible for our use).
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
# Pin to exact version matching global.json for reproducible builds.
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

# Map BUILD_ARCH to Debian triple, dpkg short arch, and clang target triple.
RUN case ${BUILD_ARCH} in \
        arm)     TOOLCHAIN_ARCH=arm-linux-gnueabihf   ; TOOLCHAIN_ARCH_SHORT=armhf   ; TOOLCHAIN_ARCH_TARGET=armv7a-linux-gnueabihf ;; \
        arm64)   TOOLCHAIN_ARCH=aarch64-linux-gnu     ; TOOLCHAIN_ARCH_SHORT=arm64   ; TOOLCHAIN_ARCH_TARGET=aarch64-linux-gnu      ;; \
        riscv64) TOOLCHAIN_ARCH=riscv64-linux-gnu     ; TOOLCHAIN_ARCH_SHORT=riscv64 ; TOOLCHAIN_ARCH_TARGET=riscv64-linux-gnu      ;; \
        x86)     TOOLCHAIN_ARCH=i386-linux-gnu        ; TOOLCHAIN_ARCH_SHORT=i386    ; TOOLCHAIN_ARCH_TARGET=i686-linux-gnu         ;; \
        x64)     TOOLCHAIN_ARCH=x86_64-linux-gnu      ; TOOLCHAIN_ARCH_SHORT=amd64   ; TOOLCHAIN_ARCH_TARGET=x86_64-linux-gnu       ;; \
        *) echo "Unsupported architecture: ${BUILD_ARCH} (use ../loongarch64/ for loongarch64)" && exit 1 ;; \
    esac \
    && echo "export TOOLCHAIN_ARCH=${TOOLCHAIN_ARCH}" > /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_SHORT=${TOOLCHAIN_ARCH_SHORT}" >> /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_TARGET=${TOOLCHAIN_ARCH_TARGET}" >> /etc/skia-env

# Choose the Debian release for the sysroot.
# Bullseye (glibc 2.31) is default — oldest with C++20 libstdc++.
# riscv64 uses Trixie because lld 18 is incompatible with GCC 10's static libstdc++.a.
ARG SYSROOT_RELEASE=bullseye

# Determine GCC/libstdc++ version from the release.
RUN case ${SYSROOT_RELEASE} in \
        bullseye) TOOLCHAIN_VERSION=10 ;; \
        trixie)   TOOLCHAIN_VERSION=14 ;; \
        *) echo "Unsupported release: ${SYSROOT_RELEASE}" && exit 1 ;; \
    esac \
    && echo "export TOOLCHAIN_VERSION=${TOOLCHAIN_VERSION}" >> /etc/skia-env

# Create the Debian sysroot using debootstrap.
#
# --foreign: Do not run second stage (we're creating a foreign-arch sysroot
#   on an amd64/arm64 host — postinst scripts can't execute). We only need
#   the unpacked headers + libraries for cross-compilation, not a bootable rootfs.
# --variant=minbase: Only essential packages (dpkg, apt, coreutils).
# --include: Add the gcc cross-compilation development packages we need.
#
# NOTE: Trixie's base-files creates merged-usr symlinks (bin→usr/bin etc.)
# but debootstrap pre-creates /bin/ as a directory, causing tar to fail with
# "Cannot create symlink: File exists" (exit 2). This is harmless — debootstrap
# still downloads all packages to the cache and creates the symlinks afterward.
# We tolerate ONLY this specific scenario and verify ALL required packages cached.
ARG SYSROOT_DIR=/sysroot
RUN . /etc/skia-env \
    && debootstrap --foreign --variant=minbase --arch=${TOOLCHAIN_ARCH_SHORT} \
         --include=libstdc++-${TOOLCHAIN_VERSION}-dev,libgcc-${TOOLCHAIN_VERSION}-dev,linux-libc-dev,libc6-dev \
         ${SYSROOT_RELEASE} ${SYSROOT_DIR} https://deb.debian.org/debian \
       || { DEBOOTSTRAP_EXIT=$? ; \
            echo "debootstrap exited ${DEBOOTSTRAP_EXIT} — verifying cache..." ; \
            for required_pkg in \
              "libstdc++-${TOOLCHAIN_VERSION}-dev" \
              "libgcc-${TOOLCHAIN_VERSION}-dev" \
              "linux-libc-dev" \
              "libc6-dev" \
              "libc6" \
              "libgcc-s1"; do \
              ls ${SYSROOT_DIR}/var/cache/apt/archives/${required_pkg}_*.deb > /dev/null 2>&1 \
              || { echo "FATAL: required package '${required_pkg}' not in cache (debootstrap exit ${DEBOOTSTRAP_EXIT})" && exit 1 ; } ; \
            done ; \
            echo "All required packages found in cache." ; } \
    && echo "Extracting all cached packages into sysroot..." \
    && for deb in ${SYSROOT_DIR}/var/cache/apt/archives/*.deb; do \
         if [ -f "$deb" ]; then \
           echo "  Extracting: $(basename $deb)" ; \
           cd /tmp && ar x "$deb" \
           && tar xf data.tar.* -C ${SYSROOT_DIR}/ 2>&1 \
              | { grep -v "Cannot create symlink" || true ; } \
           && rm -f control.tar.* data.tar.* debian-binary ; \
         fi ; \
       done \
    && echo "export SYSROOT_PATH=${SYSROOT_DIR}/usr" >> /etc/skia-env \
    && echo "export SYSROOT_ROOT=${SYSROOT_DIR}" >> /etc/skia-env \
    && GCC_DIR=$(find ${SYSROOT_DIR}/usr/lib/gcc -maxdepth 3 -name "crtbeginS.o" -exec dirname {} \; | head -1) \
    && echo "export GCC_LIB_DIR=${GCC_DIR}" >> /etc/skia-env \
    && echo "Sysroot created: ${SYSROOT_RELEASE} ${BUILD_ARCH}"

# Create "current" symlinks so build.cake can reference c++ headers version-agnostically.
#   include/c++/current -> VERSION  (main c++ headers)
#   include/{ARCH}/c++/current -> VERSION  (arch-specific c++ headers, Debian native layout)
RUN . /etc/skia-env \
    && ln -sf ${TOOLCHAIN_VERSION} ${SYSROOT_PATH}/include/c++/current \
    && if [ -d "${SYSROOT_PATH}/include/${TOOLCHAIN_ARCH}/c++/${TOOLCHAIN_VERSION}" ]; then \
         ln -sf ${TOOLCHAIN_VERSION} ${SYSROOT_PATH}/include/${TOOLCHAIN_ARCH}/c++/current ; \
       fi

# Install fontconfig into the sysroot.
# Download .deb packages from Debian's pool and extract headers + libraries.
RUN . /etc/skia-env \
    && mkdir -p /tmp/fontconfig \
    && cd /tmp/fontconfig \
    && case "${SYSROOT_RELEASE}" in \
         bullseye) FC_VERSION=2.13.1-4.2 ;; \
         trixie)   FC_VERSION=2.17.1-5   ;; \
       esac \
    && APT_REPO=https://deb.debian.org/debian \
    && mkdir -p /tmp/fontconfig/extract \
    && for pkg in libfontconfig-dev libfontconfig1; do \
           url="${APT_REPO}/pool/main/f/fontconfig/${pkg}_${FC_VERSION}_${TOOLCHAIN_ARCH_SHORT}.deb"; \
           echo "Downloading $url"; \
           curl -fSL -o "${pkg}.deb" "$url"; \
           ar x "${pkg}.deb" ; \
           tar xf data.tar.* -C /tmp/fontconfig/extract/ ; \
           rm -f control.tar.* data.tar.* debian-binary "${pkg}.deb" ; \
       done \
    && cp -a /tmp/fontconfig/extract/* ${SYSROOT_DIR}/ \
    && rm -rf /tmp/fontconfig

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
