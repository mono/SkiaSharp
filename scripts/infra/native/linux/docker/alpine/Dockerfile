# musl (Alpine) cross-compilation image.
#
# Thin layer on top of .NET's official musl cross-compilation images which provide:
#   - Azure Linux 3.0 host (clean CG surface)
#   - Clang 20 + lld (built from source, GPG-verified)
#   - Complete Alpine sysroot with musl headers, libstdc++ headers, kernel headers
#   - Cross-compiled libc++ (static)
#
# We add: .NET SDK (Cake), fontconfig-dev from Alpine repos, skia-env toolchain vars.
#
# The base images use Alpine 3.17 (musl 1.2.3). musl is forward-compatible:
# binaries built against 3.17 run on any later Alpine version.

ARG BUILD_ARCH=arm64
ARG BASE_TAG=azurelinux-3.0-net10.0-cross-arm64-musl
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
ARG TOOLCHAIN_ARCH=aarch64-alpine-linux-musl
ARG TOOLCHAIN_ARCH_TARGET=aarch64-alpine-linux-musl
ARG APK_ARCH=aarch64
ARG ROOTFS_DIR=/crossrootfs/arm64

RUN echo "export TOOLCHAIN_ARCH=${TOOLCHAIN_ARCH}" > /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_TARGET=${TOOLCHAIN_ARCH_TARGET}" >> /etc/skia-env \
    && echo "export SYSROOT_PATH=${ROOTFS_DIR}/usr" >> /etc/skia-env

# Create c++/current symlink and discover GCC lib dir.
RUN GCC_VER=$(basename "$(find ${ROOTFS_DIR}/usr/include/c++/ -mindepth 1 -maxdepth 1 -type d | sort -V | head -n1)") \
    && ln -sf ${GCC_VER} ${ROOTFS_DIR}/usr/include/c++/current \
    && echo "export GCC_LIB_DIR=${ROOTFS_DIR}/usr/lib/gcc/${TOOLCHAIN_ARCH}/${GCC_VER}" >> /etc/skia-env

# Install fontconfig-dev into the sysroot from Alpine repos.
# The base image already has apk infrastructure in the sysroot, so we can
# use apk.static to add packages. If apk.static is not available, download it.
ARG APK_STATIC_SHA256=cd282c693059c39b68ce0575fbdc4e0c162af85ed633a7834f9cfd01b8b1d58b
ARG DISTRO_VERSION=3.21
RUN APK_DIR="$(mktemp -d)" \
    && curl -fSL --retry 5 --retry-delay 10 --retry-all-errors \
        -o "$APK_DIR/apk.static" \
        "https://gitlab.alpinelinux.org/api/v4/projects/5/packages/generic/v2.12.14/x86_64/apk.static" \
    && echo "${APK_STATIC_SHA256}  $APK_DIR/apk.static" | sha256sum -c - \
    && chmod +x "$APK_DIR/apk.static" \
    && mkdir -p ${ROOTFS_DIR}/etc/apk/keys \
    && curl -fsSL -o ${ROOTFS_DIR}/etc/apk/keys/alpine-devel@lists.alpinelinux.org-6165ee59.rsa.pub \
        "https://alpinelinux.org/keys/alpine-devel@lists.alpinelinux.org-6165ee59.rsa.pub" \
    && curl -fsSL -o ${ROOTFS_DIR}/etc/apk/keys/alpine-devel@lists.alpinelinux.org-61666e3f.rsa.pub \
        "https://alpinelinux.org/keys/alpine-devel@lists.alpinelinux.org-61666e3f.rsa.pub" \
    && "$APK_DIR/apk.static" \
        -X "https://dl-cdn.alpinelinux.org/alpine/v$DISTRO_VERSION/main" \
        -X "https://dl-cdn.alpinelinux.org/alpine/v$DISTRO_VERSION/community" \
        -U --keys-dir ${ROOTFS_DIR}/etc/apk/keys --root ${ROOTFS_DIR} --arch "$APK_ARCH" --no-scripts \
        add fontconfig-dev \
    && rm -rf "$APK_DIR"

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
