# Android NDK cross-compilation image.
#
# The NDK provides its own Bionic sysroot and clang toolchain — no debootstrap needed.
# Host is Azure Linux 3.0 for consistent CG surface across all Linux build images.
#
# The NDK's own clang is used for compilation (not the host clang-18), since the
# NDK bundles Android-specific runtime support (compiler-rt, libc++, etc.).

FROM mcr.microsoft.com/azurelinux/base/core:3.0

# Use bash with strict error handling for all RUN commands.
SHELL ["/bin/bash", "-euo", "pipefail", "-c"]

# Install host build tools from Azure Linux packages.
# Bionic doesn't need clang/lld from host — NDK provides its own.
# But we need python3, git, ninja for Skia's GN build system and unzip for NDK.
RUN tdnf install -y \
        ninja-build \
        python3 \
        git \
        curl \
        tar \
        xz \
        unzip \
        gawk \
        ca-certificates \
        binutils \
    && tdnf clean all

# Install the .NET SDK (needed for Cake build scripts).
ARG DOTNET_SDK_VERSION=10.0.100
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN curl -fsSL https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.sh -o dotnet-install.sh \
    && bash dotnet-install.sh --version ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet --verbose \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet-install.sh \
    && dotnet help \
    && dotnet --info

ARG BUILD_ARCH=x64

# Map BUILD_ARCH to Android triple and sysroot lib directory
RUN case ${BUILD_ARCH} in \
        x64)   ANDROID_TRIPLE=x86_64-linux-android29  ; ANDROID_SYSROOT_LIB=x86_64-linux-android ;; \
        arm64) ANDROID_TRIPLE=aarch64-linux-android29  ; ANDROID_SYSROOT_LIB=aarch64-linux-android ;; \
        *) echo "Unsupported architecture: ${BUILD_ARCH}" && exit 1 ;; \
    esac \
    && echo "export ANDROID_TRIPLE=${ANDROID_TRIPLE}" > /etc/skia-env \
    && echo "export ANDROID_SYSROOT_LIB=${ANDROID_SYSROOT_LIB}" >> /etc/skia-env

# Install Android NDK r26b with SHA-256 verification.
ARG NDK_VERSION=r26b
ARG NDK_SHA256=ad73c0370f0b0a87d1671ed2fd5a9ac9acfd1eb5c43a7fbfbd330f85d19dd632
ARG MACHINE_ARCH=x86_64
RUN curl -fSL -o /tmp/ndk.zip \
        "https://dl.google.com/android/repository/android-ndk-${NDK_VERSION}-linux.zip" \
    && echo "${NDK_SHA256}  /tmp/ndk.zip" | sha256sum -c - \
    && unzip -q /tmp/ndk.zip -d /opt \
    && rm /tmp/ndk.zip
ENV ANDROID_NDK_HOME=/opt/android-ndk-r26b

# Determine NDK host platform (supports x86_64 and aarch64 build agents).
RUN case "${MACHINE_ARCH}" in \
        x86_64)  NDK_HOST_TAG="linux-x86_64" ;; \
        aarch64) NDK_HOST_TAG="linux-aarch64" ;; \
        *)       echo "Unsupported MACHINE_ARCH for NDK: ${MACHINE_ARCH}" && exit 1 ;; \
    esac \
    && echo "export NDK_HOST_TAG=${NDK_HOST_TAG}" >> /etc/skia-env
ENV PATH=$PATH:/opt/android-ndk-r26b/toolchains/llvm/prebuilt/linux-x86_64/bin

# Create empty libpthread stub - Bionic bundles pthreads into libc, but
# Skia's GN build adds -lpthread for target_os='linux' builds.
RUN . /etc/skia-env \
    && NDK_SYSROOT="$ANDROID_NDK_HOME/toolchains/llvm/prebuilt/${NDK_HOST_TAG}/sysroot" \
    && PTHREAD_TARGET="$NDK_SYSROOT/usr/lib/$ANDROID_SYSROOT_LIB/libpthread.a" \
    && if [ -e "$PTHREAD_TARGET" ]; then \
         echo "WARNING: NDK already ships libpthread.a — review needed" && exit 1 ; \
       fi \
    && printf '!<arch>\n' > "$PTHREAD_TARGET"

# Set cross-compiler to NDK clang
RUN . /etc/skia-env \
    && echo "export CC=${ANDROID_TRIPLE}-clang" >> /etc/skia-env \
    && echo "export CXX=${ANDROID_TRIPLE}-clang++" >> /etc/skia-env \
    && echo "export AR=llvm-ar" >> /etc/skia-env \
    && echo "export LD=lld" >> /etc/skia-env

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
