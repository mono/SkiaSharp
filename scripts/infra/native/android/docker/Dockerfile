FROM mcr.microsoft.com/dotnet-buildtools/prereqs:azurelinux-3.0-net10.0-android-amd64@sha256:8627dd84187534066002691c1cafc31dd037f1cccd28361801a967ab664fb81c

SHELL ["/bin/bash", "-euo", "pipefail", "-c"]

ARG DOTNET_SDK_VERSION=10.0.108
ARG DOTNET_SDK_SHA512=e32f76b768017718aa5a3a51fcac4b617ab4428c5f2d10b1b30f07dd7f1ea3dad4df917df48d855d78a347d635e48ce0ec309787899c53a32c4c10dd216b6b19
ARG ANDROID_NDK_VERSION=27.2.12479018

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
ENV ANDROID_NDK_HOME=${ANDROID_NDK_ROOT}

RUN test "$(basename "${ANDROID_NDK_ROOT}")" = "${ANDROID_NDK_VERSION}" \
    && test -x "${ANDROID_NDK_ROOT}/ndk-build" \
    && command -v ninja \
    && curl -fsSL --retry 5 --retry-all-errors "https://builds.dotnet.microsoft.com/dotnet/Sdk/${DOTNET_SDK_VERSION}/dotnet-sdk-${DOTNET_SDK_VERSION}-linux-x64.tar.gz" -o dotnet-sdk.tar.gz \
    && echo "${DOTNET_SDK_SHA512}  dotnet-sdk.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -xzf dotnet-sdk.tar.gz -C /usr/share/dotnet \
    && rm dotnet-sdk.tar.gz \
    && dotnet --info

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh

ENTRYPOINT ["/startup.sh"]
