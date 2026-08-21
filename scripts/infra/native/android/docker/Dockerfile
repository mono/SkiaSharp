FROM mcr.microsoft.com/dotnet-buildtools/prereqs:azurelinux-3.0-net10.0-android-amd64

SHELL ["/bin/bash", "-euo", "pipefail", "-c"]

ARG DOTNET_SDK_VERSION=10.0.108
ARG ANDROID_NDK_VERSION=27.2.12479018

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
ENV ANDROID_NDK_HOME=${ANDROID_NDK_ROOT}

RUN test "$(basename "${ANDROID_NDK_ROOT}")" = "${ANDROID_NDK_VERSION}" \
    && test -x "${ANDROID_NDK_ROOT}/ndk-build" \
    && command -v ninja \
    && curl -fsSL https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.sh -o dotnet-install.sh \
    && bash dotnet-install.sh --version ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet --verbose \
    && rm dotnet-install.sh \
    && dotnet --info

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh

ENTRYPOINT ["/startup.sh"]
