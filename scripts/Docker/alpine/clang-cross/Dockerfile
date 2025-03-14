# Arguments:
#   IMAGE_ARCH           - the architecture of the image                    [ amd64 | arm64v8 | riscv64 ]
#   DOTNET_SDK_VERSION   - the version of dotnet for the Cake script        [ 8.0 | * ]
#   TOOLCHAIN_VERSION    - the version of the GCC toolchain                 [ 9 | * ]
#   TOOLCHAIN_ARCH       - the architecture of the GCC toolchain            [ arm-alpine-linux-musleabihf | aarch64-alpine-linux-musl | riscv64-alpine-linux-musl ]
#   TOOLCHAIN_ARCH_SHORT - the short form architecture of the GCC toolchain [ armhf | arm64 ]
#   FONTCONFIG_VERSION   - the exact version of libfontconfig1 to use       [ 2.13.1-2 | * ]

ARG IMAGE_ARCH=amd64
FROM ${IMAGE_ARCH}/debian:12

# Install the required packages
RUN apt-get update \
    && apt-get install -y \
        curl python3 git clang-19 lld-19 ninja-build xz-utils curl \
    && rm -rf /var/lib/apt/lists/*

# Install the cross-compilation musl toolchain
ARG DISTRO_VERSION=3.17
ARG TOOLCHAIN_ARCH_SHORT=armhf
ARG MACHINE_ARCH=x86_64
ARG TARGET_MACHINE_ARCH=armhf
RUN APK_DIR="$(mktemp -d)" && \
    curl -SLO --create-dirs --output-dir "$APK_DIR" "https://gitlab.alpinelinux.org/api/v4/projects/5/packages/generic/v2.12.11/$MACHINE_ARCH/apk.static" && \
    chmod +x "$APK_DIR/apk.static" && \
    "$APK_DIR/apk.static" \
        -X "http://dl-cdn.alpinelinux.org/alpine/v$DISTRO_VERSION/main" \
        -X "http://dl-cdn.alpinelinux.org/alpine/v$DISTRO_VERSION/community" \
        -U --allow-untrusted --root /alpine --arch "$TARGET_MACHINE_ARCH" --initdb add && \
    "$APK_DIR/apk.static" \
        -X "http://dl-cdn.alpinelinux.org/alpine/v$DISTRO_VERSION/main" \
        -X "http://dl-cdn.alpinelinux.org/alpine/v$DISTRO_VERSION/community" \
        -U --allow-untrusted --root /alpine --arch "$TARGET_MACHINE_ARCH" --no-scripts \
        add fontconfig-dev build-base

# Install the .NET SDK
ARG DOTNET_SDK_VERSION=8.0
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT 1
RUN curl https://dot.net/v1/dotnet-install.sh -L -o dotnet-install.sh \
    && bash dotnet-install.sh --channel ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet --verbose \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet-install.sh \
    && dotnet help \
    && dotnet --info

ENV CC=clang-19 CXX=clang++-19

WORKDIR /work
