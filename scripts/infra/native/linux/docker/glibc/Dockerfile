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
ARG DOTNET_SDK_VERSION=10.0.100
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
        arm)         TOOLCHAIN_ARCH=arm-linux-gnueabihf       ; TOOLCHAIN_ARCH_SHORT=armhf   ; TOOLCHAIN_ARCH_TARGET=armv7a-linux-gnueabihf     ;; \
        arm64)       TOOLCHAIN_ARCH=aarch64-linux-gnu         ; TOOLCHAIN_ARCH_SHORT=arm64   ; TOOLCHAIN_ARCH_TARGET=aarch64-linux-gnu          ;; \
        riscv64)     TOOLCHAIN_ARCH=riscv64-linux-gnu         ; TOOLCHAIN_ARCH_SHORT=riscv64 ; TOOLCHAIN_ARCH_TARGET=riscv64-linux-gnu          ;; \
        x86)         TOOLCHAIN_ARCH=i386-linux-gnu            ; TOOLCHAIN_ARCH_SHORT=i386    ; TOOLCHAIN_ARCH_TARGET=i686-linux-gnu             ;; \
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
# NOTE: loongarch64 is not a release arch in any current Debian version — use
# gcc-cross-ports package extraction. riscv64 became a release arch in Trixie
# so it uses debootstrap (below) when built with Trixie.
ARG SYSROOT_DIR=/sysroot
RUN . /etc/skia-env \
    && case "${BUILD_ARCH}" in \
         loongarch64) \
           # loongarch64 has no standard Debian binary archive (even in Trixie) —
           # use gcc-cross package extraction.
           # This produces the cross-package layout at /sysroot/usr/{TOOLCHAIN_ARCH}/
           mkdir -p ${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH} \
           && APT_REPO=https://deb.debian.org/debian \
           && POOL="${APT_REPO}/pool/main" \
           && PORTS_SUFFIX="-ports" \
           && FETCH_FAILURES=0 \
           && mkdir -p /tmp/cross-pkgs && cd /tmp/cross-pkgs \
           && for pkg in \
                "g/gcc-${TOOLCHAIN_VERSION}-cross${PORTS_SUFFIX}/libstdc++-${TOOLCHAIN_VERSION}-dev-${TOOLCHAIN_ARCH_SHORT}-cross" \
                "g/gcc-${TOOLCHAIN_VERSION}-cross${PORTS_SUFFIX}/libstdc++6-${TOOLCHAIN_ARCH_SHORT}-cross" \
                "g/gcc-${TOOLCHAIN_VERSION}-cross${PORTS_SUFFIX}/libgcc-${TOOLCHAIN_VERSION}-dev-${TOOLCHAIN_ARCH_SHORT}-cross" \
                "g/gcc-${TOOLCHAIN_VERSION}-cross${PORTS_SUFFIX}/libgcc-s1-${TOOLCHAIN_ARCH_SHORT}-cross" \
                "c/cross-toolchain-base${PORTS_SUFFIX}/libc6-dev-${TOOLCHAIN_ARCH_SHORT}-cross" \
                "c/cross-toolchain-base${PORTS_SUFFIX}/libc6-${TOOLCHAIN_ARCH_SHORT}-cross" \
                "c/cross-toolchain-base${PORTS_SUFFIX}/linux-libc-dev-${TOOLCHAIN_ARCH_SHORT}-cross"; do \
                dir=$(echo $pkg | rev | cut -d/ -f1 | rev) ; \
                base=$(echo $pkg | rev | cut -d/ -f2- | rev) ; \
                url="${POOL}/${base}/" ; \
                echo "Fetching index: $url (looking for ${dir}_*.deb)" ; \
                deb_url=$(curl -fsSL "$url" | grep -o "href=\"${dir}_[^\"]*\.deb\"" | sed 's/href="//;s/"$//' | sort -V | tail -1) ; \
                if [ -n "$deb_url" ]; then \
                  echo "  Downloading: ${url}${deb_url}" ; \
                  curl -fsSL -O "${url}${deb_url}" ; \
                  ar x "${deb_url}" ; \
                  mkdir -p extract && tar xf data.tar.* -C extract/ ; \
                  rm -f control.tar.* data.tar.* debian-binary "${deb_url}" ; \
                else \
                  echo "  FATAL: No package found for ${dir}" ; \
                  FETCH_FAILURES=$((FETCH_FAILURES + 1)) ; \
                fi ; \
              done \
           && if [ "${FETCH_FAILURES}" -gt 0 ]; then \
                echo "ERROR: ${FETCH_FAILURES} package(s) could not be fetched" && exit 1 ; \
              fi \
           && echo "Extracted files:" && ls extract/usr/${TOOLCHAIN_ARCH}/ | head -5 \
           && if [ -d extract/usr/${TOOLCHAIN_ARCH} ]; then \
                cp -a extract/usr/${TOOLCHAIN_ARCH}/* ${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH}/ ; \
              fi \
           && if [ -d extract/usr/lib/gcc-cross ]; then \
                mkdir -p ${SYSROOT_DIR}/usr/lib/gcc-cross ; \
                cp -a extract/usr/lib/gcc-cross/* ${SYSROOT_DIR}/usr/lib/gcc-cross/ ; \
              fi \
           && rm -rf /tmp/cross-pkgs \
           && echo "export SYSROOT_PATH=${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH}" >> /etc/skia-env \
           && echo "export GCC_LIB_DIR=${SYSROOT_DIR}/usr/lib/gcc-cross/${TOOLCHAIN_ARCH}/${TOOLCHAIN_VERSION}" >> /etc/skia-env ;; \
         *) \
           # Standard debootstrap for architectures with Debian binary archives.
           # Produces native layout: headers at /usr/include/, libs at /usr/lib/{triple}/
           # NOTE: --foreign only extracts essential/base packages; --include packages
           # are downloaded to var/cache/apt/archives/ but NOT extracted. We must
           # manually extract them after debootstrap completes.
           # NOTE: Trixie's base-files creates merged-usr symlinks (bin→usr/bin etc.)
           # but debootstrap pre-creates /bin/ as a directory, causing tar to fail with
           # "Cannot create symlink: File exists" (exit 2). This is harmless — debootstrap
           # still downloads all packages to the cache and creates the symlinks afterward.
           # We tolerate ONLY this specific scenario and verify ALL required packages cached.
           debootstrap --foreign --variant=minbase --arch=${TOOLCHAIN_ARCH_SHORT} \
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
           && echo "export GCC_LIB_DIR=${GCC_DIR}" >> /etc/skia-env ;; \
       esac \
    && echo "Sysroot created: ${SYSROOT_RELEASE} ${BUILD_ARCH}"

# Create the c++/current symlink so build.cake can reference libstdc++ headers
# without baking the GCC version number into the build script.
# Debootstrap: headers at /sysroot/usr/include/c++/VERSION/
# Cross-packages (loongarch64): headers at /sysroot/usr/{ARCH}/include/c++/VERSION/
#
# Additionally, Debian native packages place arch-specific c++ headers at:
#   /usr/include/{TOOLCHAIN_ARCH}/c++/VERSION/  (e.g. /usr/include/arm-linux-gnueabihf/c++/10/)
# But build.cake expects them at:
#   /usr/include/c++/VERSION/{TOOLCHAIN_ARCH}/  (the cross-toolchain layout)
# We create a symlink to bridge this gap.
RUN . /etc/skia-env \
    && INCLUDE_DIR=${SYSROOT_PATH}/include/c++ \
    && echo "Looking for c++ headers in: ${INCLUDE_DIR}" \
    && echo "TOOLCHAIN_VERSION=${TOOLCHAIN_VERSION}, TOOLCHAIN_ARCH=${TOOLCHAIN_ARCH}" \
    && if [ -d "${INCLUDE_DIR}" ]; then ls ${INCLUDE_DIR}/ ; fi \
    && if [ -d "${INCLUDE_DIR}/${TOOLCHAIN_VERSION}" ]; then \
         ln -sf ${TOOLCHAIN_VERSION} ${INCLUDE_DIR}/current ; \
         echo "Created symlink: ${INCLUDE_DIR}/current -> ${TOOLCHAIN_VERSION}" ; \
       elif [ -d "${INCLUDE_DIR}" ]; then \
         GCC_VERSION=$(ls ${INCLUDE_DIR}/ | grep -E '^[0-9]' | sort -V | tail -1) ; \
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
    && ARCH_INCLUDE=${SYSROOT_PATH}/include/${TOOLCHAIN_ARCH}/c++/${TOOLCHAIN_VERSION} \
    && if [ -d "${ARCH_INCLUDE}" ] && [ ! -e "${INCLUDE_DIR}/${TOOLCHAIN_VERSION}/${TOOLCHAIN_ARCH}" ]; then \
         ln -sf ${ARCH_INCLUDE} ${INCLUDE_DIR}/${TOOLCHAIN_VERSION}/${TOOLCHAIN_ARCH} ; \
         echo "Created arch symlink: ${INCLUDE_DIR}/${TOOLCHAIN_VERSION}/${TOOLCHAIN_ARCH} -> ${ARCH_INCLUDE}" ; \
       fi \
    && if [ ! -e "${INCLUDE_DIR}/current/${TOOLCHAIN_ARCH}" ] && [ -d "${INCLUDE_DIR}/current/" ]; then \
         EXISTING_ARCH=$(ls -d ${INCLUDE_DIR}/current/*-linux-gnu* 2>/dev/null | head -1 | xargs basename 2>/dev/null) ; \
         if [ -n "${EXISTING_ARCH}" ] && [ "${EXISTING_ARCH}" != "${TOOLCHAIN_ARCH}" ]; then \
           ln -sf ${EXISTING_ARCH} ${INCLUDE_DIR}/current/${TOOLCHAIN_ARCH} ; \
           echo "Created arch compat symlink: ${TOOLCHAIN_ARCH} -> ${EXISTING_ARCH}" ; \
         fi ; \
       fi

# Fix libc/libpthread linker scripts to use relative paths.
# Only needed for cross-package builds (loongarch64) which don't use --sysroot.
# For debootstrap builds, --sysroot causes lld to automatically prepend the sysroot
# to absolute paths in linker scripts, so no modification is needed.
RUN . /etc/skia-env \
    && if [ -z "${SYSROOT_ROOT:-}" ]; then \
         for script in \
              ${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH}/lib/libc.so \
              ${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH}/lib/libpthread.so \
              ${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH}/lib/libpthread.so.0 \
              ${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH}/lib64/libc.so \
              ${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH}/lib64/libpthread.so; do \
           if [ -f "$script" ]; then \
             sed -i "s|/usr/${TOOLCHAIN_ARCH}/lib64/||g; s|/usr/${TOOLCHAIN_ARCH}/lib/||g; s|/lib/${TOOLCHAIN_ARCH}/||g; s|/lib64/||g; s|/lib/||g" "$script" ; \
             echo "Fixed linker script: $script" ; \
           fi ; \
         done ; \
       else \
         echo "Skipping linker script fix (--sysroot handles absolute paths)" ; \
       fi

# Install fontconfig into the sysroot.
# Download .deb packages from Debian's pool and extract headers + libraries.
# For cross-package arches (loongarch64), relocate to match the SYSROOT_PATH layout.
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
    && case "${BUILD_ARCH}" in \
         loongarch64) \
           # Cross-package layout: put headers and libs under SYSROOT_PATH
           mkdir -p ${SYSROOT_PATH}/include ${SYSROOT_PATH}/lib ; \
           cp -a /tmp/fontconfig/extract/usr/include/* ${SYSROOT_PATH}/include/ ; \
           if [ -d "/tmp/fontconfig/extract/usr/lib/${TOOLCHAIN_ARCH}" ]; then \
             cp -a /tmp/fontconfig/extract/usr/lib/${TOOLCHAIN_ARCH}/* ${SYSROOT_PATH}/lib/ ; \
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
