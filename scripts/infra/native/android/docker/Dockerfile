ARG DOTNET_SDK_VERSION=10.0.108
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_SDK_VERSION}-azurelinux3.0-amd64 AS dotnet-sdk

FROM mcr.microsoft.com/dotnet-buildtools/prereqs:azurelinux-3.0-net10.0-android-amd64

SHELL ["/bin/bash", "-euo", "pipefail", "-c"]

COPY --from=dotnet-sdk /usr/share/dotnet /usr/share/dotnet
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN dotnet --info

ENV ANDROID_NDK_HOME=/usr/local/android-sdk/ndk/27.2.12479018
RUN grep -q '^Pkg.Revision = 27.2.12479018$' "${ANDROID_NDK_HOME}/source.properties" \
    && test -x "${ANDROID_NDK_HOME}/ndk-build" \
    && command -v ninja

WORKDIR /work
