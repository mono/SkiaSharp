FROM mcr.microsoft.com/dotnet-buildtools/prereqs:azurelinux-3.0-net10.0-android-amd64

SHELL ["/bin/bash", "-euo", "pipefail", "-c"]

ARG DOTNET_SDK_VERSION=10.0.108
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN curl -fsSL https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.sh -o dotnet-install.sh \
    && bash dotnet-install.sh --version ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet \
    && rm dotnet-install.sh \
    && dotnet --info

RUN NDK_HOME=$(find /usr/local/android-sdk/ndk -mindepth 1 -maxdepth 1 -type d | sort -V | tail -1) \
    && grep -q '^Pkg.Revision = 27.2.12479018$' "${NDK_HOME}/source.properties" \
    && ln -s "${NDK_HOME}" /usr/local/android-ndk \
    && test -x /usr/local/android-ndk/ndk-build \
    && command -v ninja

ENV ANDROID_NDK_HOME=/usr/local/android-ndk
WORKDIR /work
