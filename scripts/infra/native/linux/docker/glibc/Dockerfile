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
# loongarch64 requires Trixie (no Bullseye/Bookworm binary archive).

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
ARG LLVM_VERSION=18.1.8
RUN ln -sf /usr/lib64/libtinfo.so.6 /usr/lib64/libtinfo.so.5 \
    && ln -sf /usr/lib64/libncursesw.so.6 /usr/lib64/libncurses.so.5 \
    && curl -sL "https://github.com/llvm/llvm-project/releases/download/llvmorg-${LLVM_VERSION}/clang+llvm-${LLVM_VERSION}-x86_64-linux-gnu-ubuntu-18.04.tar.xz" \
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

# Map BUILD_ARCH to Debian triple, dpkg short arch, and clang target triple.
RUN case ${BUILD_ARCH} in \
        arm)         TOOLCHAIN_ARCH=arm-linux-gnueabihf       ; TOOLCHAIN_ARCH_SHORT=armhf   ; TOOLCHAIN_ARCH_TARGET=armv7a-linux-gnueabihf     ;; \
        arm64)       TOOLCHAIN_ARCH=aarch64-linux-gnu         ; TOOLCHAIN_ARCH_SHORT=arm64   ; TOOLCHAIN_ARCH_TARGET=aarch64-linux-gnu          ;; \
        riscv64)     TOOLCHAIN_ARCH=riscv64-linux-gnu         ; TOOLCHAIN_ARCH_SHORT=riscv64 ; TOOLCHAIN_ARCH_TARGET=riscv64-linux-gnu          ;; \
        x86)         TOOLCHAIN_ARCH=i686-linux-gnu            ; TOOLCHAIN_ARCH_SHORT=i386    ; TOOLCHAIN_ARCH_TARGET=i686-linux-gnu             ;; \
        x64)         TOOLCHAIN_ARCH=x86_64-linux-gnu          ; TOOLCHAIN_ARCH_SHORT=amd64   ; TOOLCHAIN_ARCH_TARGET=x86_64-linux-gnu           ;; \
        loongarch64) TOOLCHAIN_ARCH=loongarch64-linux-gnu     ; TOOLCHAIN_ARCH_SHORT=loong64 ; TOOLCHAIN_ARCH_TARGET=loongarch64-linux-gnu      ;; \
        *) echo "Unsupported architecture: ${BUILD_ARCH}" && exit 1 ;; \
    esac \
    && echo "export TOOLCHAIN_ARCH=${TOOLCHAIN_ARCH}" > /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_SHORT=${TOOLCHAIN_ARCH_SHORT}" >> /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_TARGET=${TOOLCHAIN_ARCH_TARGET}" >> /etc/skia-env

# Choose the Debian release for the sysroot.
# Bullseye (glibc 2.31) is default — oldest with C++20 libstdc++.
# loongarch64 needs Trixie — no binary archive exists in Bullseye/Bookworm.
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
# NOTE: riscv64 is not a release arch in Bullseye — it's available from the
# debian-ports archive. For riscv64+bullseye, we fall back to extracting
# gcc-10-cross .deb packages (same approach as the old debian/11 Dockerfile).
ARG SYSROOT_DIR=/sysroot
RUN . /etc/skia-env \
    && case "${BUILD_ARCH}" in \
         riscv64|loongarch64) \
           # riscv64/loongarch64 have no standard Debian binary archive — use gcc-cross package extraction.
           # This produces the cross-package layout at /sysroot/usr/{TOOLCHAIN_ARCH}/
           # loong64 packages are in -ports variants; riscv64 is in the standard pool.
           mkdir -p ${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH} \
           && TOOLCHAIN_VERSION=$(grep TOOLCHAIN_VERSION /etc/skia-env | cut -d= -f2) \
           && APT_REPO=https://deb.debian.org/debian \
           && POOL="${APT_REPO}/pool/main" \
           && case "${TOOLCHAIN_ARCH_SHORT}" in \
                loong64|riscv64) PORTS_SUFFIX="-ports" ;; \
                *)       PORTS_SUFFIX="" ;; \
              esac \
           && mkdir -p /tmp/cross-pkgs && cd /tmp/cross-pkgs \
           && for pkg in \
                "g/gcc-${TOOLCHAIN_VERSION}-cross${PORTS_SUFFIX}/libstdc++-${TOOLCHAIN_VERSION}-dev-${TOOLCHAIN_ARCH_SHORT}-cross" \
                "g/gcc-${TOOLCHAIN_VERSION}-cross${PORTS_SUFFIX}/libgcc-${TOOLCHAIN_VERSION}-dev-${TOOLCHAIN_ARCH_SHORT}-cross" \
                "c/cross-toolchain-base${PORTS_SUFFIX}/libc6-dev-${TOOLCHAIN_ARCH_SHORT}-cross" \
                "c/cross-toolchain-base${PORTS_SUFFIX}/linux-libc-dev-${TOOLCHAIN_ARCH_SHORT}-cross"; do \
                dir=$(echo $pkg | rev | cut -d/ -f1 | rev) ; \
                base=$(echo $pkg | rev | cut -d/ -f2- | rev) ; \
                url="${POOL}/${base}/" ; \
                echo "Fetching index: $url (looking for ${dir}_*_all.deb)" ; \
                deb_url=$(curl -sL "$url" | grep -o "href=\"${dir}_[^\"]*_all\.deb\"" | sed 's/href="//;s/"$//' | sort -V | tail -1) ; \
                if [ -n "$deb_url" ]; then \
                  echo "  Downloading: ${url}${deb_url}" ; \
                  curl -sL -O "${url}${deb_url}" ; \
                  ar x "${deb_url}" ; \
                  mkdir -p extract && tar xf data.tar.* -C extract/ ; \
                  rm -f control.tar.* data.tar.* debian-binary "${deb_url}" ; \
                else \
                  echo "  WARNING: No package found for ${dir}" ; \
                fi ; \
              done \
           && echo "Extracted files:" && ls extract/usr/${TOOLCHAIN_ARCH}/ 2>/dev/null | head -5 || true \
           && if [ -d extract/usr/${TOOLCHAIN_ARCH} ]; then \
                cp -a extract/usr/${TOOLCHAIN_ARCH}/* ${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH}/ ; \
              fi \
           && if [ -d extract/usr/lib/gcc-cross ]; then \
                mkdir -p ${SYSROOT_DIR}/usr/lib/gcc-cross ; \
                cp -a extract/usr/lib/gcc-cross/* ${SYSROOT_DIR}/usr/lib/gcc-cross/ ; \
              fi \
           && rm -rf /tmp/cross-pkgs \
           && echo "export SYSROOT_PATH=${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH}" >> /etc/skia-env ;; \
         *) \
           # Standard debootstrap for architectures with Debian binary archives.
           # Produces native layout: headers at /usr/include/, libs at /usr/lib/{triple}/
           # NOTE: --foreign only extracts essential/base packages; --include packages
           # are downloaded to var/cache/apt/archives/ but NOT extracted. We must
           # manually extract them after debootstrap completes.
           debootstrap --foreign --variant=minbase --arch=${TOOLCHAIN_ARCH_SHORT} \
             --include=libstdc++-${TOOLCHAIN_VERSION}-dev,libgcc-${TOOLCHAIN_VERSION}-dev,linux-libc-dev,libc6-dev \
             ${SYSROOT_RELEASE} ${SYSROOT_DIR} https://deb.debian.org/debian \
           && echo "Extracting --include dev packages from cache..." \
           && for deb in \
                ${SYSROOT_DIR}/var/cache/apt/archives/libstdc++-${TOOLCHAIN_VERSION}-dev*.deb \
                ${SYSROOT_DIR}/var/cache/apt/archives/libgcc-${TOOLCHAIN_VERSION}-dev*.deb \
                ${SYSROOT_DIR}/var/cache/apt/archives/linux-libc-dev*.deb \
                ${SYSROOT_DIR}/var/cache/apt/archives/libc6-dev*.deb; do \
                if [ -f "$deb" ]; then \
                  echo "  Extracting: $(basename $deb)" ; \
                  cd /tmp && ar x "$deb" && tar xf data.tar.* -C ${SYSROOT_DIR}/ && rm -f control.tar.* data.tar.* debian-binary ; \
                fi ; \
              done \
           && echo "export SYSROOT_PATH=${SYSROOT_DIR}/usr" >> /etc/skia-env ;; \
       esac \
    && echo "Sysroot created: ${SYSROOT_RELEASE} ${BUILD_ARCH}"

# Create the c++/current symlink so build.cake can reference libstdc++ headers
# without baking the GCC version number into the build script.
# Debootstrap: headers at /sysroot/usr/include/c++/VERSION/
# Cross-packages (riscv64/loongarch64): headers at /sysroot/usr/{ARCH}/include/c++/VERSION/
#
# Additionally, Debian native packages place arch-specific c++ headers at:
#   /usr/include/{TOOLCHAIN_ARCH}/c++/VERSION/  (e.g. /usr/include/arm-linux-gnueabihf/c++/10/)
# But build.cake expects them at:
#   /usr/include/c++/VERSION/{TOOLCHAIN_ARCH}/  (the cross-toolchain layout)
# We create a symlink to bridge this gap.
RUN . /etc/skia-env \
    && TOOLCHAIN_VERSION=$(grep TOOLCHAIN_VERSION /etc/skia-env | cut -d= -f2) \
    && SYSROOT_PATH_VAL=$(grep "SYSROOT_PATH=" /etc/skia-env | cut -d= -f2) \
    && INCLUDE_DIR=${SYSROOT_PATH_VAL}/include/c++ \
    && echo "Looking for c++ headers in: ${INCLUDE_DIR}" \
    && echo "TOOLCHAIN_VERSION=${TOOLCHAIN_VERSION}, TOOLCHAIN_ARCH=${TOOLCHAIN_ARCH}" \
    && if [ -d "${INCLUDE_DIR}" ]; then ls ${INCLUDE_DIR}/ ; fi \
    && if [ -d "${INCLUDE_DIR}/${TOOLCHAIN_VERSION}" ]; then \
         ln -sf ${TOOLCHAIN_VERSION} ${INCLUDE_DIR}/current ; \
         echo "Created symlink: ${INCLUDE_DIR}/current -> ${TOOLCHAIN_VERSION}" ; \
       elif [ -d "${INCLUDE_DIR}" ]; then \
         GCC_VERSION=$(ls ${INCLUDE_DIR}/ | grep -E '^[0-9]' | head -1) ; \
         if [ -n "$GCC_VERSION" ]; then \
           ln -sf ${GCC_VERSION} ${INCLUDE_DIR}/current ; \
           echo "Created symlink: ${INCLUDE_DIR}/current -> ${GCC_VERSION}" ; \
           TOOLCHAIN_VERSION=${GCC_VERSION} ; \
         else \
           echo "ERROR: No version directory found in ${INCLUDE_DIR}" && exit 1 ; \
         fi ; \
       else \
         echo "ERROR: ${INCLUDE_DIR} does not exist!" && exit 1 ; \
       fi \
    && ARCH_INCLUDE=${SYSROOT_PATH_VAL}/include/${TOOLCHAIN_ARCH}/c++/${TOOLCHAIN_VERSION} \
    && if [ -d "${ARCH_INCLUDE}" ] && [ ! -e "${INCLUDE_DIR}/${TOOLCHAIN_VERSION}/${TOOLCHAIN_ARCH}" ]; then \
         ln -sf ${ARCH_INCLUDE} ${INCLUDE_DIR}/${TOOLCHAIN_VERSION}/${TOOLCHAIN_ARCH} ; \
         echo "Created arch symlink: ${INCLUDE_DIR}/${TOOLCHAIN_VERSION}/${TOOLCHAIN_ARCH} -> ${ARCH_INCLUDE}" ; \
       fi

# Fix libc/libpthread linker scripts to use relative paths.
# Debian's linker scripts contain absolute paths like:
#   GROUP ( /usr/lib/x86_64-linux-gnu/libc.so.6 ... )
# These must be relative for cross-compilation with --sysroot to work.
RUN . /etc/skia-env \
    && for script in \
         ${SYSROOT_DIR}/usr/lib/${TOOLCHAIN_ARCH}/libc.so \
         ${SYSROOT_DIR}/usr/lib/${TOOLCHAIN_ARCH}/libpthread.so \
         ${SYSROOT_DIR}/usr/lib/${TOOLCHAIN_ARCH}/libpthread.so.0 \
         ${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH}/lib/libc.so \
         ${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH}/lib/libpthread.so \
         ${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH}/lib/libpthread.so.0; do \
       if [ -f "$script" ]; then \
         sed -i "s|/usr/${TOOLCHAIN_ARCH}/lib/||g; s|/usr/lib/${TOOLCHAIN_ARCH}/||g; s|/lib/${TOOLCHAIN_ARCH}/||g" "$script" ; \
       fi ; \
    done

# Install fontconfig into the sysroot.
# Download .deb packages from Debian's pool and extract headers + libraries.
# For cross-package arches (riscv64/loongarch64), relocate to match the SYSROOT_PATH layout.
RUN . /etc/skia-env \
    && SYSROOT_PATH_VAL=$(grep "SYSROOT_PATH=" /etc/skia-env | cut -d= -f2) \
    && mkdir -p /tmp/fontconfig \
    && cd /tmp/fontconfig \
    && case "${SYSROOT_RELEASE}" in \
         bullseye) FC_VERSION=2.13.1-4.2 ;; \
         trixie)   FC_VERSION=2.17.1-5   ;; \
       esac \
    && case "${TOOLCHAIN_ARCH_SHORT}" in \
         riscv64)    FC_VERSION=2.15.0-2.3 ;; \
       esac \
    && APT_REPO=https://deb.debian.org/debian \
    && mkdir -p /tmp/fontconfig/extract \
    && for pkg in libfontconfig-dev libfontconfig1; do \
           url="${APT_REPO}/pool/main/f/fontconfig/${pkg}_${FC_VERSION}_${TOOLCHAIN_ARCH_SHORT}.deb"; \
           echo "Downloading $url"; \
           curl -L -o "${pkg}.deb" "$url"; \
           ar x "${pkg}.deb" ; \
           tar xf data.tar.* -C /tmp/fontconfig/extract/ ; \
           rm -f control.tar.* data.tar.* debian-binary "${pkg}.deb" ; \
       done \
    && case "${BUILD_ARCH}" in \
         riscv64|loongarch64) \
           # Cross-package layout: put headers and libs under SYSROOT_PATH
           mkdir -p ${SYSROOT_PATH_VAL}/include ${SYSROOT_PATH_VAL}/lib ; \
           cp -a /tmp/fontconfig/extract/usr/include/* ${SYSROOT_PATH_VAL}/include/ ; \
           if [ -d "/tmp/fontconfig/extract/usr/lib/${TOOLCHAIN_ARCH}" ]; then \
             cp -a /tmp/fontconfig/extract/usr/lib/${TOOLCHAIN_ARCH}/* ${SYSROOT_PATH_VAL}/lib/ ; \
           fi ;; \
         *) \
           # Debootstrap layout: extract directly to sysroot root
           cp -a /tmp/fontconfig/extract/* ${SYSROOT_DIR}/ ;; \
       esac \
    && rm -rf /tmp/fontconfig

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
