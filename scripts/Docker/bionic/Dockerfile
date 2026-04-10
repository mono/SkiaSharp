# Arguments:
#   DOTNET_SDK_VERSION   - the version of dotnet for the Cake script        [ 10.0 | * ]
#   BUILD_ARCH           - the target architecture to build for             [ x64 | arm64 ]

# NOTE: Using devcontainers base image because mcr.microsoft.com/mirror/docker/library/debian:13
# is not yet available. This is a Debian 13 (Trixie) image hosted on MCR.
# For local builds without MCR access, replace with: debian:trixie
FROM mcr.microsoft.com/devcontainers/base:debian13

USER root

# Set the architecture-specific variables based on the value of the BUILD_ARCH argument
ARG BUILD_ARCH=x64
RUN case ${BUILD_ARCH} in \
        x64)   ANDROID_TRIPLE=x86_64-linux-android29  ; ANDROID_SYSROOT_LIB=x86_64-linux-android ;; \
        arm64) ANDROID_TRIPLE=aarch64-linux-android29  ; ANDROID_SYSROOT_LIB=aarch64-linux-android ;; \
        *) echo "Unsupported architecture: ${BUILD_ARCH}" && exit 1 ;; \
    esac \
    && echo "export ANDROID_TRIPLE=${ANDROID_TRIPLE}" > /etc/skia-env \
    && echo "export ANDROID_SYSROOT_LIB=${ANDROID_SYSROOT_LIB}" >> /etc/skia-env

# Install the required packages
# binutils provides readelf for post-build dependency verification
RUN apt-get update \
    && apt-get install -y \
        curl python3 git ninja-build unzip binutils \
    && rm -rf /var/lib/apt/lists/*

# Configure Android NDK r26b
RUN curl -SL https://dl.google.com/android/repository/android-ndk-r26b-linux.zip -o /tmp/ndk.zip \
    && unzip -q /tmp/ndk.zip -d /opt \
    && rm /tmp/ndk.zip
ENV ANDROID_NDK_HOME=/opt/android-ndk-r26b
ENV PATH=$PATH:$ANDROID_NDK_HOME/toolchains/llvm/prebuilt/linux-x86_64/bin

# Create empty libpthread stub - Bionic bundles pthreads into libc, but
# Skia's GN build adds -lpthread for target_os='linux' builds.
RUN . /etc/skia-env \
    && llvm-ar rcs $ANDROID_NDK_HOME/toolchains/llvm/prebuilt/linux-x86_64/sysroot/usr/lib/$ANDROID_SYSROOT_LIB/libpthread.a

# Set cross-compiler and archiver to NDK toolchain
RUN . /etc/skia-env \
    && echo "export CC=${ANDROID_TRIPLE}-clang" >> /etc/skia-env \
    && echo "export CXX=${ANDROID_TRIPLE}-clang++" >> /etc/skia-env \
    && echo "export AR=llvm-ar" >> /etc/skia-env

# Install the .NET SDK
ARG DOTNET_SDK_VERSION=10.0
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN curl https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.sh -L -o dotnet-install.sh \
    && bash dotnet-install.sh --channel ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet --verbose \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet-install.sh \
    && dotnet help \
    && dotnet --info

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
