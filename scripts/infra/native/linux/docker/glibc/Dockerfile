# glibc cross-compilation image.
#
# Thin layer on top of .NET's official cross-compilation images which provide:
#   - Azure Linux 3.0 host (clean CG surface)
#   - Clang 20 + lld (built from source, GPG-verified)
#   - Complete sysroot with glibc headers, libstdc++ headers, kernel headers
#   - Cross-compiled libc++ (static) and compiler-rt
#
# We add: .NET SDK (Cake), fontconfig-dev headers, skia-env toolchain vars.
#
# Architecture:
#   .NET cross image (host) → clang-20 → --sysroot=/crossrootfs/<arch> → libSkiaSharp.so
#
# Base images use these sysroot distros:
# | arch    | sysroot distro | glibc | Supported distros                    |
# |---------|----------------|-------|--------------------------------------|
# | arm64   | bionic         | 2.27  | Debian 10+, Ubuntu 18.04+, RHEL 8+  |
# | arm     | jammy          | 2.35  | Debian 12+, Ubuntu 22.04+, RHEL 9+  |
# | x86     | bionic         | 2.27  | Debian 10+, Ubuntu 18.04+            |
# | x64     | bionic         | 2.27  | Debian 10+, Ubuntu 18.04+, RHEL 8+  |
# | riscv64 | noble          | 2.39  | Debian 13+, Ubuntu 24.04+            |
# | loong64 | sid            | latest| Debian unstable                      |
#
# Supported architectures: arm, arm64, x86, x64, riscv64, loongarch64

# Map BUILD_ARCH to the .NET cross image tag.
ARG BUILD_ARCH=arm64
ARG BASE_TAG=azurelinux-3.0-net10.0-cross-arm64
FROM mcr.microsoft.com/dotnet-buildtools/prereqs:${BASE_TAG}

SHELL ["/bin/bash", "-euo", "pipefail", "-c"]

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

# Toolchain configuration for build.cake.
# These map to the env vars expected by native/linux-clang-cross/build.cake.
ARG TOOLCHAIN_ARCH=aarch64-linux-gnu
ARG TOOLCHAIN_ARCH_SHORT=arm64
ARG TOOLCHAIN_ARCH_TARGET=aarch64-linux-gnu
ARG ROOTFS_DIR=/crossrootfs/arm64

RUN echo "export TOOLCHAIN_ARCH=${TOOLCHAIN_ARCH}" > /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_SHORT=${TOOLCHAIN_ARCH_SHORT}" >> /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_TARGET=${TOOLCHAIN_ARCH_TARGET}" >> /etc/skia-env \
    && echo "export SYSROOT_PATH=${ROOTFS_DIR}/usr" >> /etc/skia-env \
    && echo "export SYSROOT_ROOT=${ROOTFS_DIR}" >> /etc/skia-env \
    && GCC_DIR=$(find ${ROOTFS_DIR}/usr/lib/gcc -maxdepth 3 -name "crtbeginS.o" -exec dirname {} \; | head -1) \
    && echo "export GCC_LIB_DIR=${GCC_DIR}" >> /etc/skia-env

# Create c++/current symlinks so build.cake can reference headers version-agnostically.
RUN GCC_VER=$(basename "$(find ${ROOTFS_DIR}/usr/include/c++/ -mindepth 1 -maxdepth 1 -type d | sort -V | head -n1)") \
    && ln -sf ${GCC_VER} ${ROOTFS_DIR}/usr/include/c++/current \
    && if [ -d "${ROOTFS_DIR}/usr/include/${TOOLCHAIN_ARCH}/c++/${GCC_VER}" ]; then \
         ln -sf ${GCC_VER} ${ROOTFS_DIR}/usr/include/${TOOLCHAIN_ARCH}/c++/current ; \
       fi \
    && if [ -d "${ROOTFS_DIR}/usr/include/c++/${GCC_VER}/${TOOLCHAIN_ARCH}" ]; then \
         mkdir -p ${ROOTFS_DIR}/usr/include/c++/current 2>/dev/null || true ; \
         ln -sf ../${GCC_VER}/${TOOLCHAIN_ARCH} ${ROOTFS_DIR}/usr/include/c++/current/${TOOLCHAIN_ARCH} 2>/dev/null || true ; \
       fi

# Install fontconfig headers into the sysroot.
# Download .deb packages from the same Debian release as the sysroot and verify SHA-256.
ARG FC_DISTRO=bullseye
ARG FC_VERSION=2.13.1-4.2
ARG FC_DEV_SHA256
ARG FC_LIB_SHA256
RUN if [ -n "${FC_DEV_SHA256:-}" ] && [ -n "${FC_LIB_SHA256:-}" ]; then \
      mkdir -p /tmp/fontconfig && cd /tmp/fontconfig \
      && APT_REPO=https://deb.debian.org/debian \
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
           tar xf data.tar.* -C ${ROOTFS_DIR}/ ; \
           rm -f control.tar.* data.tar.* debian-binary "${pkg}.deb" ; \
         done \
      && rm -rf /tmp/fontconfig ; \
    else \
      echo "WARNING: No fontconfig SHA-256 provided — skipping fontconfig install" ; \
    fi

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
