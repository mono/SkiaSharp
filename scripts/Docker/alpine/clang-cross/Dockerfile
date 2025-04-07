# Arguments:
#   IMAGE_ARCH           - the architecture of the image                    [ amd64 | arm64v8 | riscv64 | loongarch64 ]
#   DOTNET_SDK_VERSION   - the version of dotnet for the Cake script        [ 8.0 | * ]
#   LLVM_VERSION         - the version of the LLVM compiler                 [ 13 | * ]
#   TOOLCHAIN_VERSION    - the version of the GCC toolchain                 [ 14 | * ]
#   TOOLCHAIN_ARCH       - the architecture of the GCC toolchain            [ arm-alpine-linux-musleabihf | aarch64-alpine-linux-musl | riscv64-alpine-linux-musl | loongarch64-alpine-linux-musl ]

ARG IMAGE_ARCH=amd64
FROM ${IMAGE_ARCH}/debian:12

# Set the architecture-specific variables based on the value of the BUILD_ARCH argument
ARG BUILD_ARCH=arm64
RUN case ${BUILD_ARCH} in \
        arm)         TOOLCHAIN_ARCH=armv7-alpine-linux-musleabihf     ; TOOLCHAIN_ARCH_TARGET=armv7-alpine-linux-musleabihf     ;; \
        arm64)       TOOLCHAIN_ARCH=aarch64-alpine-linux-musl         ; TOOLCHAIN_ARCH_TARGET=aarch64-alpine-linux-musl         ;; \
        loongarch64) TOOLCHAIN_ARCH=loongarch64-alpine-linux-musl     ; TOOLCHAIN_ARCH_TARGET=loongarch64-alpine-linux-musl     ;; \
        riscv64)     TOOLCHAIN_ARCH=riscv64-alpine-linux-musl         ; TOOLCHAIN_ARCH_TARGET=riscv64-alpine-linux-musl         ;; \
        x86)         TOOLCHAIN_ARCH=i586-alpine-linux-musl            ; TOOLCHAIN_ARCH_TARGET=i586-alpine-linux-musl            ;; \
        x64)         TOOLCHAIN_ARCH=x86_64-alpine-linux-musl          ; TOOLCHAIN_ARCH_TARGET=x86_64-alpine-linux-musl          ;; \
        *) echo "Unsupported architecture: ${BUILD_ARCH}" && exit 1 ;; \
    esac \
    && echo "export TOOLCHAIN_ARCH=${TOOLCHAIN_ARCH}" > /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_TARGET=${TOOLCHAIN_ARCH_TARGET}" >> /etc/skia-env

# Install the required packages
ARG LLVM_VERSION=19
RUN apt-get update \
    && apt-get install -y \
        curl python3 git clang-${LLVM_VERSION} lld-${LLVM_VERSION} ninja-build xz-utils \
    && rm -rf /var/lib/apt/lists/*

# Install the cross-compilation musl toolchain
# First, obtain apk.static from https://gitlab.alpinelinux.org/alpine/apk-tools/-/releases/v2.12.14
ARG MACHINE_ARCH=x86_64
RUN . /etc/skia-env \
    && case "${BUILD_ARCH}" in \
         arm)           APK_ARCH=armv7       ;; \
         arm64)         APK_ARCH=aarch64     ;; \
         loongarch64)   APK_ARCH=loongarch64 ;; \
         riscv64)       APK_ARCH=riscv64     ;; \
         x86)           APK_ARCH=x86         ;; \
         x64)           APK_ARCH=x86_64      ;; \
         *) echo "Unsupported architecture: ${BUILD_ARCH}" && exit 1 ;; \
         esac \
    && case "${BUILD_ARCH}" in \
         loongarch64) DISTRO_VERSION=3.21 ;; \
         riscv64)     DISTRO_VERSION=3.20 ;; \
         *)           DISTRO_VERSION=3.17 ;; \
         esac \
    && APK_DIR="$(mktemp -d)" \
    && curl -SLO --create-dirs --output-dir "$APK_DIR" "https://gitlab.alpinelinux.org/api/v4/projects/5/packages/generic/v2.12.14/$MACHINE_ARCH/apk.static" \
    && chmod +x "$APK_DIR/apk.static" \
    && "$APK_DIR/apk.static" \
        -X "http://dl-cdn.alpinelinux.org/alpine/v$DISTRO_VERSION/main" \
        -X "http://dl-cdn.alpinelinux.org/alpine/v$DISTRO_VERSION/community" \
        -U --allow-untrusted --root /alpine --arch "$APK_ARCH" --initdb add \
    && "$APK_DIR/apk.static" \
        -X "http://dl-cdn.alpinelinux.org/alpine/v$DISTRO_VERSION/main" \
        -X "http://dl-cdn.alpinelinux.org/alpine/v$DISTRO_VERSION/community" \
        -U --allow-untrusted --root /alpine --arch "$APK_ARCH" --no-scripts \
        add fontconfig-dev build-base linux-headers

# Install the .NET SDK
ARG DOTNET_SDK_VERSION=8.0
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN curl https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.sh -L -o dotnet-install.sh \
    && bash dotnet-install.sh --channel ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet --verbose \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet-install.sh \
    && dotnet help \
    && dotnet --info

ENV CC=clang-${LLVM_VERSION} CXX=clang++-${LLVM_VERSION}

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
