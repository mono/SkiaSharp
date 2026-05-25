# Android NDK cross-compilation image.
#
# Thin layer on top of .NET's official Android image which provides:
#   - Azure Linux 3.0 host (clean CG surface)
#   - Android NDK 27.2 with SDK tools
#   - Clang 20 + lld
#
# We add: .NET SDK (Cake), skia-env toolchain vars, libpthread stub.
#
# Usage:
#   docker build --build-arg BUILD_ARCH=x64 .
#
# Supported architectures: x64, arm64

FROM mcr.microsoft.com/dotnet-buildtools/prereqs:azurelinux-3.0-net10.0-android-amd64

SHELL ["/bin/bash", "-euo", "pipefail", "-c"]

# Install the .NET SDK (needed for Cake build scripts).
ARG DOTNET_SDK_VERSION=10.0.108
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN curl -fsSL https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.sh -o dotnet-install.sh \
    && bash dotnet-install.sh --version ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet --verbose \
    && rm dotnet-install.sh \
    && dotnet --info

ARG BUILD_ARCH=x64

# Map BUILD_ARCH to Android triple, discover NDK dynamically, and write skia-env.
RUN case ${BUILD_ARCH} in \
        x64)   ANDROID_TRIPLE=x86_64-linux-android29  ; ANDROID_SYSROOT_LIB=x86_64-linux-android ;; \
        arm64) ANDROID_TRIPLE=aarch64-linux-android29 ; ANDROID_SYSROOT_LIB=aarch64-linux-android ;; \
        *) echo "FATAL: Unsupported BUILD_ARCH=${BUILD_ARCH}" && exit 1 ;; \
    esac \
    && NDK_HOME=$(ls -d /usr/local/android-sdk/ndk/*/ | head -1) \
    && NDK_HOME=${NDK_HOME%/} \
    && if [ -z "$NDK_HOME" ] || [ ! -d "$NDK_HOME" ]; then \
         echo "FATAL: No Android NDK found in /usr/local/android-sdk/ndk/" && exit 1 ; \
       fi \
    && NDK_BIN="${NDK_HOME}/toolchains/llvm/prebuilt/linux-x86_64/bin" \
    && if [ ! -d "$NDK_BIN" ]; then \
         echo "FATAL: NDK bin directory not found: $NDK_BIN" && exit 1 ; \
       fi \
    && echo "export ANDROID_TRIPLE=${ANDROID_TRIPLE}" > /etc/skia-env \
    && echo "export ANDROID_SYSROOT_LIB=${ANDROID_SYSROOT_LIB}" >> /etc/skia-env \
    && echo "export NDK_HOST_TAG=linux-x86_64" >> /etc/skia-env \
    && echo "export ANDROID_NDK_HOME=${NDK_HOME}" >> /etc/skia-env \
    && echo "export CC=${NDK_BIN}/${ANDROID_TRIPLE}-clang" >> /etc/skia-env \
    && echo "export CXX=${NDK_BIN}/${ANDROID_TRIPLE}-clang++" >> /etc/skia-env \
    && echo "export AR=${NDK_BIN}/llvm-ar" >> /etc/skia-env \
    && echo "export LD=${NDK_BIN}/lld" >> /etc/skia-env

# Create empty libpthread stub — Bionic bundles pthreads into libc, but
# Skia's GN build adds -lpthread for target_os='linux' builds.
RUN . /etc/skia-env \
    && NDK_SYSROOT="${ANDROID_NDK_HOME}/toolchains/llvm/prebuilt/linux-x86_64/sysroot" \
    && PTHREAD_TARGET="$NDK_SYSROOT/usr/lib/$ANDROID_SYSROOT_LIB/libpthread.a" \
    && if [ -e "$PTHREAD_TARGET" ]; then \
         echo "FATAL: NDK already ships libpthread.a — review needed" && exit 1 ; \
       fi \
    && printf '!<arch>\n' > "$PTHREAD_TARGET"

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
