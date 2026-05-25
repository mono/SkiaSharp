# musl (Alpine) cross-compilation image.
#
# Thin layer on top of .NET's official musl cross-compilation images which provide:
#   - Azure Linux 3.0 host (clean CG surface)
#   - Clang 20 + lld (built from source, GPG-verified)
#   - Complete Alpine sysroot with musl headers, kernel headers
#   - Cross-compiled libc++ (static)
#
# We add: .NET SDK (Cake), fontconfig-dev from Alpine repos, skia-env toolchain vars.
#
# Usage:
#   docker build --build-arg BUILD_ARCH=arm64 .
#
# Supported architectures: arm, arm64, x64, riscv64, loongarch64

ARG BUILD_ARCH=arm64

# .NET cross images use "amd64" where SkiaSharp uses "x64".
# CI must pass IMAGE_ARCH=amd64 when BUILD_ARCH=x64.
# All other architectures: IMAGE_ARCH defaults to BUILD_ARCH.
ARG IMAGE_ARCH=${BUILD_ARCH}
FROM mcr.microsoft.com/dotnet-buildtools/prereqs:azurelinux-3.0-net10.0-cross-${IMAGE_ARCH}-musl

SHELL ["/bin/bash", "-euo", "pipefail", "-c"]

ARG BUILD_ARCH

# Derive all toolchain variables from BUILD_ARCH.
RUN case "${BUILD_ARCH}" in \
      arm)         TOOLCHAIN_ARCH=armv7-alpine-linux-musleabihf; \
                   APK_ARCH=armv7; \
                   ROOTFS_DIR=/crossrootfs/arm ;; \
      arm64)       TOOLCHAIN_ARCH=aarch64-alpine-linux-musl; \
                   APK_ARCH=aarch64; \
                   ROOTFS_DIR=/crossrootfs/arm64 ;; \
      x64)         TOOLCHAIN_ARCH=x86_64-alpine-linux-musl; \
                   APK_ARCH=x86_64; \
                   ROOTFS_DIR=/crossrootfs/x64 ;; \
      riscv64)     TOOLCHAIN_ARCH=riscv64-alpine-linux-musl; \
                   APK_ARCH=riscv64; \
                   ROOTFS_DIR=/crossrootfs/riscv64 ;; \
      loongarch64) TOOLCHAIN_ARCH=loongarch64-alpine-linux-musl; \
                   APK_ARCH=loongarch64; \
                   ROOTFS_DIR=/crossrootfs/loongarch64 ;; \
      *) echo "FATAL: Unsupported BUILD_ARCH=${BUILD_ARCH}" && exit 1 ;; \
    esac \
    && echo "export TOOLCHAIN_ARCH=${TOOLCHAIN_ARCH}" > /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_TARGET=${TOOLCHAIN_ARCH}" >> /etc/skia-env \
    && echo "export SYSROOT_PATH=${ROOTFS_DIR}/usr" >> /etc/skia-env \
    && echo "export SYSROOT_ROOT=${ROOTFS_DIR}" >> /etc/skia-env \
    && echo "export APK_ARCH=${APK_ARCH}" >> /etc/skia-env \
    && GCC_VER=$(ls ${ROOTFS_DIR}/usr/lib/gcc/${TOOLCHAIN_ARCH}/ 2>/dev/null | sort -V | tail -1) \
    && echo "export GCC_LIB_DIR=${ROOTFS_DIR}/usr/lib/gcc/${TOOLCHAIN_ARCH}/${GCC_VER}" >> /etc/skia-env

# Install the .NET SDK (needed for Cake build scripts).
ARG DOTNET_SDK_VERSION=10.0.108
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN curl -fsSL https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.sh -o dotnet-install.sh \
    && bash dotnet-install.sh --version ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet --verbose \
    && rm dotnet-install.sh \
    && dotnet --info

ENV CC=clang CXX=clang++

# Install fontconfig-dev into the sysroot from Alpine repos.
# The sysroot is Alpine 3.17 but we pull fontconfig from 3.21 (newer keys needed).
# SSL_CERT_FILE is needed because apk.static (musl) can't find Azure Linux's CA bundle.
ARG APK_STATIC_VERSION=2.12.14
ARG APK_STATIC_SHA256=cd282c693059c39b68ce0575fbdc4e0c162af85ed633a7834f9cfd01b8b1d58b
ARG ALPINE_VERSION=3.21
RUN . /etc/skia-env \
    && APK_DIR="$(mktemp -d)" \
    && curl -fSL --retry 5 --retry-delay 10 --retry-all-errors \
        -o "$APK_DIR/apk.static" \
        "https://gitlab.alpinelinux.org/api/v4/projects/5/packages/generic/v${APK_STATIC_VERSION}/x86_64/apk.static" \
    && echo "${APK_STATIC_SHA256}  $APK_DIR/apk.static" | sha256sum -c - \
    && chmod +x "$APK_DIR/apk.static" \
    && curl -fsSL -o ${SYSROOT_ROOT}/etc/apk/keys/alpine-devel@lists.alpinelinux.org-6165ee59.rsa.pub \
        "https://alpinelinux.org/keys/alpine-devel@lists.alpinelinux.org-6165ee59.rsa.pub" \
    && curl -fsSL -o ${SYSROOT_ROOT}/etc/apk/keys/alpine-devel@lists.alpinelinux.org-61666e3f.rsa.pub \
        "https://alpinelinux.org/keys/alpine-devel@lists.alpinelinux.org-61666e3f.rsa.pub" \
    && SSL_CERT_FILE=/etc/pki/tls/certs/ca-bundle.crt "$APK_DIR/apk.static" \
        -X "https://dl-cdn.alpinelinux.org/alpine/v${ALPINE_VERSION}/main" \
        -X "https://dl-cdn.alpinelinux.org/alpine/v${ALPINE_VERSION}/community" \
        --no-cache --root ${SYSROOT_ROOT} --arch "${APK_ARCH}" --no-scripts \
        add fontconfig-dev \
    && rm -rf "$APK_DIR"

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
