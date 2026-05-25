# loongarch64 glibc cross-compilation image.
#
# loongarch64 has no Debian binary archive (even in Trixie), so it cannot use
# debootstrap. Instead, we download gcc-cross-ports packages from Debian's pool
# and manually extract them to create a sysroot.
#
# Architecture:
#   Azure Linux 3.0 (host) → clang-18 → --sysroot=/sysroot → libSkiaSharp.so
#
# The cross-package layout places headers/libs under /usr/{TOOLCHAIN_ARCH}/ rather
# than the native /usr/include/ + /usr/lib/{triple}/ layout that debootstrap produces.
# We set SYSROOT_ROOT=/sysroot so that lld resolves absolute paths in linker scripts
# (e.g., /usr/loongarch64-linux-gnu/lib/libc.so.6) relative to the sysroot automatically.

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
# URLs and SHA-256 checksums are pinned for reproducibility — update when Debian releases new versions.
RUN . /etc/skia-env \
    && mkdir -p ${SYSROOT_DIR}/usr/${TOOLCHAIN_ARCH} \
    && mkdir -p /tmp/cross-pkgs && cd /tmp/cross-pkgs \
    && PACKAGES=" \
         https://deb.debian.org/debian/pool/main/g/gcc-14-cross-ports/libstdc++-14-dev-loong64-cross_14.3.0-14cross2_all.deb \
           81600199ccac5a0a8c019ce973ea4532530a671dd3e4ee3c552a9d5b97f5bffa \
         https://deb.debian.org/debian/pool/main/g/gcc-14-cross-ports/libstdc++6-loong64-cross_14.2.0-19cross1_all.deb \
           29ea1472a2b1b6c30db334bef7d7ba302db03ca46ff8a243359f6d488c62ef76 \
         https://deb.debian.org/debian/pool/main/g/gcc-14-cross-ports/libgcc-14-dev-loong64-cross_14.3.0-14cross2_all.deb \
           b4c019526c04c8d45ee6c7044446b2a968ab50b96c55600943312802d2d71531 \
         https://deb.debian.org/debian/pool/main/g/gcc-14-cross-ports/libgcc-s1-loong64-cross_14.2.0-19cross1_all.deb \
           a87453248b2a60ce083606d60950154291fc7e347df9c14263adade49fb18c41 \
         https://deb.debian.org/debian/pool/main/c/cross-toolchain-base-ports/libc6-dev-loong64-cross_2.42-10cross1_all.deb \
           0b47a5a3f41c6d598ed880b38eb8d3b897d3b78ec13d6529d194a390216dc988 \
         https://deb.debian.org/debian/pool/main/c/cross-toolchain-base-ports/libc6-loong64-cross_2.42-10cross1_all.deb \
           fc58b06c95911192f413b785a2399c3e6c72dc302680def6ad0534fa27ece3e6 \
         https://deb.debian.org/debian/pool/main/c/cross-toolchain-base-ports/linux-libc-dev-loong64-cross_6.12.38-1cross1_all.deb \
           b30c0c8a0d1a8480695b498c1791c930df489b94b26d15ccafa37bfc2e37f3ed \
       " \
    && set -- $PACKAGES \
    && while [ $# -ge 2 ]; do \
         url="$1"; expected_sha="$2"; shift 2 ; \
         filename=$(basename "$url") ; \
         echo "Downloading: $filename" ; \
         curl -fsSL -o "$filename" "$url" ; \
         actual_sha=$(sha256sum "$filename" | cut -d' ' -f1) ; \
         if [ "$actual_sha" != "$expected_sha" ]; then \
           echo "FATAL: SHA-256 mismatch for $filename" ; \
           echo "  Expected: $expected_sha" ; \
           echo "  Actual:   $actual_sha" ; \
           exit 1 ; \
         fi ; \
         echo "  SHA-256 verified: $actual_sha" ; \
         ar x "$filename" ; \
         mkdir -p extract && tar xf data.tar.* -C extract/ ; \
         rm -f control.tar.* data.tar.* debian-binary "$filename" ; \
       done \
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
    && echo "export SYSROOT_ROOT=${SYSROOT_DIR}" >> /etc/skia-env \
    && echo "export GCC_LIB_DIR=${SYSROOT_DIR}/usr/lib/gcc-cross/${TOOLCHAIN_ARCH}/${TOOLCHAIN_VERSION}" >> /etc/skia-env

# Create "current" symlink so build.cake can reference c++ headers version-agnostically.
#   include/c++/current -> VERSION
RUN . /etc/skia-env \
    && ln -sf ${TOOLCHAIN_VERSION} ${SYSROOT_PATH}/include/c++/current

# Install fontconfig into the sysroot with SHA-256 verification.
# Cross-package layout: headers go to SYSROOT_PATH/include, libs to SYSROOT_PATH/lib.
ARG FC_DEV_SHA256=0d3aee5780ecbab1f709036cedd935f2af690bd5392227411d97e6b160537456
ARG FC_LIB_SHA256=234a8998df450d4cd83f9c0a4a942fe1892b556122ff96fc737e0c74cb9c4421
RUN . /etc/skia-env \
    && mkdir -p /tmp/fontconfig && cd /tmp/fontconfig \
    && case "${SYSROOT_RELEASE}" in \
         trixie) FC_VERSION=2.17.1-5 ;; \
       esac \
    && APT_REPO=https://deb.debian.org/debian \
    && mkdir -p /tmp/fontconfig/extract \
    && for pkg_info in \
         "libfontconfig-dev ${FC_DEV_SHA256}" \
         "libfontconfig1 ${FC_LIB_SHA256}"; do \
         pkg=$(echo $pkg_info | cut -d' ' -f1) ; \
         expected_sha=$(echo $pkg_info | cut -d' ' -f2) ; \
         url="${APT_REPO}/pool/main/f/fontconfig/${pkg}_${FC_VERSION}_${TOOLCHAIN_ARCH_SHORT}.deb" ; \
         echo "Downloading $url" ; \
         curl -fSL -o "${pkg}.deb" "$url" ; \
         actual_sha=$(sha256sum "${pkg}.deb" | cut -d' ' -f1) ; \
         if [ "$actual_sha" != "$expected_sha" ]; then \
           echo "FATAL: SHA-256 mismatch for ${pkg}.deb" ; \
           echo "  Expected: $expected_sha" ; \
           echo "  Actual:   $actual_sha" ; \
           exit 1 ; \
         fi ; \
         echo "  SHA-256 verified: $actual_sha" ; \
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
