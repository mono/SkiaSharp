# glibc cross-compilation image.
#
# Thin layer on top of .NET's official cross-compilation images which provide:
#   - Azure Linux 3.0 host (clean CG surface)
#   - Clang 20 + lld (built from source, GPG-verified)
#   - Complete sysroot with glibc headers, kernel headers
#   - Cross-compiled libc++ (static) and compiler-rt
#
# We add: .NET SDK (Cake), fontconfig-dev headers, skia-env toolchain vars.
#
# Usage (BUILD_ARCH is the only input — the x64→amd64 image-tag mapping is
# handled internally via stage aliases below):
#   docker build --build-arg BUILD_ARCH=arm64 .
#   docker build --build-arg BUILD_ARCH=x64 .
#
# NOTE: x86 uses a separate directory (glibc-x86/) with its own Dockerfile
# because the .NET team doesn't ship libc++ for x86.
#
# Supported architectures: arm, arm64, x64, riscv64, loongarch64
# (x86 uses glibc-x86/Dockerfile instead)

ARG BUILD_ARCH=arm64

# One stage per arch — .NET cross images use "amd64" instead of "x64", so we
# map it here in a single place. BuildKit only pulls the stage referenced by
# the final `FROM base-${BUILD_ARCH}`; the others are inert.
FROM mcr.microsoft.com/dotnet-buildtools/prereqs:azurelinux-3.0-net10.0-cross-arm         AS base-arm
FROM mcr.microsoft.com/dotnet-buildtools/prereqs:azurelinux-3.0-net10.0-cross-arm64       AS base-arm64
FROM mcr.microsoft.com/dotnet-buildtools/prereqs:azurelinux-3.0-net10.0-cross-amd64       AS base-x64
FROM mcr.microsoft.com/dotnet-buildtools/prereqs:azurelinux-3.0-net10.0-cross-riscv64     AS base-riscv64
FROM mcr.microsoft.com/dotnet-buildtools/prereqs:azurelinux-3.0-net10.0-cross-loongarch64 AS base-loongarch64

FROM base-${BUILD_ARCH}

SHELL ["/bin/bash", "-euo", "pipefail", "-c"]

ARG BUILD_ARCH

# Derive all toolchain variables from BUILD_ARCH.
# This is the single source of truth — no external build-args needed.
RUN case "${BUILD_ARCH}" in \
      arm)         TOOLCHAIN_ARCH=arm-linux-gnueabihf; \
                   TOOLCHAIN_ARCH_SHORT=armhf; \
                   TOOLCHAIN_ARCH_TARGET=armv7a-linux-gnueabihf; \
                   ROOTFS_DIR=/crossrootfs/arm ;; \
      arm64)       TOOLCHAIN_ARCH=aarch64-linux-gnu; \
                   TOOLCHAIN_ARCH_SHORT=arm64; \
                   TOOLCHAIN_ARCH_TARGET=aarch64-linux-gnu; \
                   ROOTFS_DIR=/crossrootfs/arm64 ;; \
      x64)         TOOLCHAIN_ARCH=x86_64-linux-gnu; \
                   TOOLCHAIN_ARCH_SHORT=amd64; \
                   TOOLCHAIN_ARCH_TARGET=x86_64-linux-gnu; \
                   ROOTFS_DIR=/crossrootfs/x64 ;; \
      riscv64)     TOOLCHAIN_ARCH=riscv64-linux-gnu; \
                   TOOLCHAIN_ARCH_SHORT=riscv64; \
                   TOOLCHAIN_ARCH_TARGET=riscv64-linux-gnu; \
                   ROOTFS_DIR=/crossrootfs/riscv64 ;; \
      loongarch64) TOOLCHAIN_ARCH=loongarch64-linux-gnu; \
                   TOOLCHAIN_ARCH_SHORT=loong64; \
                   TOOLCHAIN_ARCH_TARGET=loongarch64-linux-gnu; \
                   ROOTFS_DIR=/crossrootfs/loongarch64 ;; \
      x86)         echo "FATAL: x86 must use glibc-x86/Dockerfile (.NET doesn't ship libc++ for x86)" && exit 1 ;; \
      *) echo "FATAL: Unsupported BUILD_ARCH=${BUILD_ARCH}" && exit 1 ;; \
    esac \
    && echo "export TOOLCHAIN_ARCH=${TOOLCHAIN_ARCH}" > /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_SHORT=${TOOLCHAIN_ARCH_SHORT}" >> /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_TARGET=${TOOLCHAIN_ARCH_TARGET}" >> /etc/skia-env \
    && echo "export SYSROOT_PATH=${ROOTFS_DIR}/usr" >> /etc/skia-env \
    && echo "export SYSROOT_ROOT=${ROOTFS_DIR}" >> /etc/skia-env \
    && GCC_DIR=$(find ${ROOTFS_DIR}/usr/lib/gcc -maxdepth 3 -name "crtbeginS.o" -exec dirname {} \; | sort -V | tail -1) \
    && if [ -z "$GCC_DIR" ] || [ ! -d "$GCC_DIR" ]; then \
         echo "FATAL: GCC lib dir not found (crtbeginS.o missing under ${ROOTFS_DIR}/usr/lib/gcc)" && exit 1 ; \
       fi \
    && echo "export GCC_LIB_DIR=${GCC_DIR}" >> /etc/skia-env \
    # Verify libc++ headers exist — deterministic build requirement.
    && if [ ! -d "${ROOTFS_DIR}/usr/include/c++/v1" ]; then \
         echo "FATAL: libc++ headers not found at ${ROOTFS_DIR}/usr/include/c++/v1" ; \
         echo "       The .NET cross-image is expected to provide libc++." ; \
         exit 1 ; \
       fi

# Install the .NET SDK (needed for Cake build scripts).
ARG DOTNET_SDK_VERSION=10.0.108
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN curl -fsSL https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.sh -o dotnet-install.sh \
    && bash dotnet-install.sh --version ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet --verbose \
    && rm dotnet-install.sh \
    && dotnet --info

ENV CC=clang CXX=clang++

# zstd is needed to extract newer .deb packages (Ubuntu 22.04+ uses zstd compression).
# (xz is already present in the .NET cross base image for the older .debs.)
RUN tdnf install -y zstd && tdnf clean all

# Install fontconfig headers into the sysroot.
# Fontconfig .deb packages are downloaded and SHA-256 verified.
# Package names differ: Ubuntu uses libfontconfig1-dev, Debian uses libfontconfig-dev.
RUN . /etc/skia-env \
    && case "${BUILD_ARCH}" in \
      arm)         FC_REPO=http://ports.ubuntu.com/ubuntu-ports; \
                   FC_VERSION=2.13.1-4.2ubuntu5; \
                   FC_DEV_PKG=libfontconfig-dev; \
                   FC_DEV_SHA256=8e4b18145fd9990a1f8c6d05aadb4fa40f0fcab23a81c8ef801532551a341fdb; \
                   FC_LIB_SHA256=0ac3c1b2b2dcc7cabd11f7e2daa4b3c0f89fd30d97d0722302fc7db623208cef ;; \
      arm64)       FC_REPO=http://ports.ubuntu.com/ubuntu-ports; \
                   FC_VERSION=2.12.6-0ubuntu2; \
                   FC_DEV_PKG=libfontconfig1-dev; \
                   FC_DEV_SHA256=93fc9ec2f69a87cbbc2efe7e5f3769c56ea9453a1449f28d78f192062b532975; \
                   FC_LIB_SHA256=de5942257801f0d344cc55c95cc190aae4e434b3d8d0d77a029f210da755fdc3 ;; \
      x64)         FC_REPO=http://archive.ubuntu.com/ubuntu; \
                   FC_VERSION=2.12.6-0ubuntu2; \
                   FC_DEV_PKG=libfontconfig1-dev; \
                   FC_DEV_SHA256=ce4d63624128b06eef6ea0f2d04f0baa1cba22d7325fcbcce48ce5cbe2533b1a; \
                   FC_LIB_SHA256=647bb8f09e751a39d488b0260db6015ce1118c56114cc771dd5bf907180f0c80 ;; \
      riscv64)     FC_REPO=http://ports.ubuntu.com/ubuntu-ports; \
                   FC_VERSION=2.15.0-1.1ubuntu2; \
                   FC_DEV_PKG=libfontconfig-dev; \
                   FC_DEV_SHA256=c219de19d80aa7195b1d2dcddc1b5c651eb85b4f616e7d837f2aada3e56ea5fd; \
                   FC_LIB_SHA256=eb2574c2316922efe83b5a91da5278cf8abfc65051ddce28308032c99fafd36b ;; \
      loongarch64) FC_REPO=https://deb.debian.org/debian; \
                   FC_VERSION=2.17.1-5; \
                   FC_DEV_PKG=libfontconfig-dev; \
                   FC_DEV_SHA256=0d3aee5780ecbab1f709036cedd935f2af690bd5392227411d97e6b160537456; \
                   FC_LIB_SHA256=234a8998df450d4cd83f9c0a4a942fe1892b556122ff96fc737e0c74cb9c4421 ;; \
    esac \
    && mkdir -p /tmp/fontconfig && cd /tmp/fontconfig \
    && for pkg_info in \
         "${FC_DEV_PKG} ${FC_DEV_SHA256}" \
         "libfontconfig1 ${FC_LIB_SHA256}"; do \
         pkg=$(echo $pkg_info | cut -d' ' -f1) ; \
         expected_sha=$(echo $pkg_info | cut -d' ' -f2) ; \
         url="${FC_REPO}/pool/main/f/fontconfig/${pkg}_${FC_VERSION}_${TOOLCHAIN_ARCH_SHORT}.deb" ; \
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
         tar xf data.tar.* -C ${SYSROOT_ROOT}/ ; \
         rm -f control.tar.* data.tar.* debian-binary "${pkg}.deb" ; \
       done \
    # Sanity check: catches empty/transitional dev packages (e.g., libfontconfig1-dev
    # on Ubuntu 22.04+ which is a transitional shell with no headers — the real
    # package is libfontconfig-dev without the "1").
    && if [ ! -f "${SYSROOT_ROOT}/usr/include/fontconfig/fontconfig.h" ]; then \
         echo "FATAL: fontconfig.h not found at ${SYSROOT_ROOT}/usr/include/fontconfig/fontconfig.h" ; \
         echo "       The ${FC_DEV_PKG} package may be a transitional/empty shell." ; \
         exit 1 ; \
       fi \
    && rm -rf /tmp/fontconfig

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
