# loongarch64 glibc cross-compilation image.
#
# loongarch64 has no Debian binary archive (even in Trixie), so it cannot use
# debootstrap. Instead, we download gcc-cross-ports packages from Debian's pool
# and manually extract them to create a sysroot.
#
# Architecture:
#   Azure Linux 3.0 (host) → clang-18 → --sysroot=/sysroot/usr/loongarch64-linux-gnu → libSkiaSharp.so
#
# The cross-package layout places headers/libs under /usr/{TOOLCHAIN_ARCH}/ rather
# than the native /usr/include/ + /usr/lib/{triple}/ layout that debootstrap produces.
# This requires linker-script path fixups since the scripts use absolute paths.

FROM mcr.microsoft.com/azurelinux/base/core:3.0

# Use bash with strict error handling for all RUN commands.
SHELL ["/bin/bash", "-euo", "pipefail", "-c"]

# Install host build tools from Azure Linux packages.
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

# loongarch64 constants — no BUILD_ARCH arg needed since this image is single-arch.
ARG SYSROOT_RELEASE=trixie
ARG SYSROOT_DIR=/sysroot

RUN TOOLCHAIN_ARCH=loongarch64-linux-gnu \
    && TOOLCHAIN_ARCH_SHORT=loong64 \
    && TOOLCHAIN_ARCH_TARGET=loongarch64-linux-gnu \
    && case "${SYSROOT_RELEASE}" in \
         trixie) TOOLCHAIN_VERSION=14 ;; \
         *) echo "Unsupported release for loongarch64: ${SYSROOT_RELEASE}" && exit 1 ;; \
       esac \
    && echo "export TOOLCHAIN_ARCH=${TOOLCHAIN_ARCH}" > /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_SHORT=${TOOLCHAIN_ARCH_SHORT}" >> /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_TARGET=${TOOLCHAIN_ARCH_TARGET}" >> /etc/skia-env \
    && echo "export TOOLCHAIN_VERSION=${TOOLCHAIN_VERSION}" >> /etc/skia-env

# Download and extract gcc-cross-ports packages from Debian's package pool.
# loongarch64 is only available via cross-compilation packages (gcc-14-cross-ports).
# We fetch: libstdc++-dev, libstdc++6, libgcc-dev, libgcc-s1, libc6-dev, libc6, linux-libc-dev.
RUN . /etc/skia-env \
    && mkdir -p ${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH} \
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
    && echo "Extracted cross-package contents:" && find extract/usr/${TOOLCHAIN_ARCH}/ -maxdepth 1 -type d \
    && if [ -d extract/usr/${TOOLCHAIN_ARCH} ]; then \
         cp -a extract/usr/${TOOLCHAIN_ARCH}/* ${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH}/ ; \
       fi \
    && if [ -d extract/usr/lib/gcc-cross ]; then \
         mkdir -p ${SYSROOT_DIR}/usr/lib/gcc-cross ; \
         cp -a extract/usr/lib/gcc-cross/* ${SYSROOT_DIR}/usr/lib/gcc-cross/ ; \
       fi \
    && rm -rf /tmp/cross-pkgs \
    && echo "export SYSROOT_PATH=${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH}" >> /etc/skia-env \
    && echo "export GCC_LIB_DIR=${SYSROOT_DIR}/usr/lib/gcc-cross/${TOOLCHAIN_ARCH}/${TOOLCHAIN_VERSION}" >> /etc/skia-env

# Create "current" symlink so build.cake can reference c++ headers version-agnostically.
#   include/c++/current -> VERSION
RUN . /etc/skia-env \
    && ln -sf ${TOOLCHAIN_VERSION} ${SYSROOT_PATH}/include/c++/current

# Fix linker scripts to use relative paths.
# Cross-packages contain linker scripts (libc.so, libpthread.so) with absolute paths like
# /usr/loongarch64-linux-gnu/lib/libc.so.6 — but we've relocated them to /sysroot/.
# Strip the absolute prefixes so the linker finds libs relative to the script location.
RUN . /etc/skia-env \
    && for script in \
         ${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH}/lib/libc.so \
         ${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH}/lib/libpthread.so \
         ${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH}/lib/libpthread.so.0 \
         ${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH}/lib64/libc.so \
         ${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH}/lib64/libpthread.so; do \
       if [ -f "$script" ]; then \
         sed -i "s|/usr/${TOOLCHAIN_ARCH}/lib64/||g; s|/usr/${TOOLCHAIN_ARCH}/lib/||g; s|/lib/${TOOLCHAIN_ARCH}/||g; s|/lib64/||g; s|/lib/||g" "$script" ; \
         echo "Fixed linker script: $script" ; \
       fi ; \
    done

# Install fontconfig into the sysroot.
# Cross-package layout: headers go to SYSROOT_PATH/include, libs to SYSROOT_PATH/lib.
RUN . /etc/skia-env \
    && mkdir -p /tmp/fontconfig && cd /tmp/fontconfig \
    && case "${SYSROOT_RELEASE}" in \
         trixie) FC_VERSION=2.17.1-5 ;; \
       esac \
    && APT_REPO=https://deb.debian.org/debian \
    && mkdir -p /tmp/fontconfig/extract \
    && for pkg in libfontconfig-dev libfontconfig1; do \
         url="${APT_REPO}/pool/main/f/fontconfig/${pkg}_${FC_VERSION}_${TOOLCHAIN_ARCH_SHORT}.deb" ; \
         echo "Downloading $url" ; \
         curl -fSL -o "${pkg}.deb" "$url" ; \
         ar x "${pkg}.deb" ; \
         tar xf data.tar.* -C /tmp/fontconfig/extract/ ; \
         rm -f control.tar.* data.tar.* debian-binary "${pkg}.deb" ; \
       done \
    && mkdir -p ${SYSROOT_PATH}/include ${SYSROOT_PATH}/lib \
    && cp -a /tmp/fontconfig/extract/usr/include/* ${SYSROOT_PATH}/include/ \
    && if [ -d "/tmp/fontconfig/extract/usr/lib/${TOOLCHAIN_ARCH}" ]; then \
         cp -a /tmp/fontconfig/extract/usr/lib/${TOOLCHAIN_ARCH}/* ${SYSROOT_PATH}/lib/ ; \
       fi \
    && rm -rf /tmp/fontconfig

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
