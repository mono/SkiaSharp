ARG DOTNET_SDK_VERSION=10.0.108
FROM mcr.microsoft.com/dotnet-buildtools/prereqs:azurelinux-3.0-net10.0-android-amd64

SHELL ["/bin/bash", "-euo", "pipefail", "-c"]

ARG DOTNET_SDK_VERSION
ARG DOTNET_SDK_SHA512=e32f76b768017718aa5a3a51fcac4b617ab4428c5f2d10b1b30f07dd7f1ea3dad4df917df48d855d78a347d635e48ce0ec309787899c53a32c4c10dd216b6b19
ENV DOTNET_ROOT=/usr/share/dotnet \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 \
    PATH=/usr/share/dotnet:${PATH}
RUN curl -fsSL "https://builds.dotnet.microsoft.com/dotnet/Sdk/${DOTNET_SDK_VERSION}/dotnet-sdk-${DOTNET_SDK_VERSION}-linux-x64.tar.gz" -o dotnet-sdk.tar.gz \
    && echo "${DOTNET_SDK_SHA512}  dotnet-sdk.tar.gz" | sha512sum -c - \
    && mkdir -p "${DOTNET_ROOT}" \
    && tar -xzf dotnet-sdk.tar.gz -C "${DOTNET_ROOT}" \
    && rm dotnet-sdk.tar.gz \
    && dotnet --info

ENV ANDROID_NDK_HOME=/usr/local/android-sdk/ndk/27.2.12479018
RUN grep -q '^Pkg.Revision = 27.2.12479018$' "${ANDROID_NDK_HOME}/source.properties" \
    && test -x "${ANDROID_NDK_HOME}/ndk-build" \
    && command -v ninja

WORKDIR /work
