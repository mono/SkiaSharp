# Tizen native build toolchain image.
#
# Builds libSkiaSharp.so / libHarfBuzzSharp.so for Tizen 10.0 (x86_64 + aarch64)
# inside a container, instead of on the build agent. Mirrors the WASM image
# (scripts/infra/native/wasm/docker/Dockerfile), including its Debian 12 base.
#
# Tizen Studio ships GCC 14.2 and the Tizen 10 rootstraps. The base needs:
# .NET SDK (Cake scripts), python3 + ninja + git (the Skia gn/ninja build), and
# a JRE (the Tizen installer / CLI). Host dependencies are installed
# explicitly, while the native SDK package set is pinned in install-tizen.sh.

FROM mcr.microsoft.com/mirror/docker/library/debian:12

SHELL ["/bin/bash", "-euo", "pipefail", "-c"]

ENV DEBIAN_FRONTEND=noninteractive

# Base utilities + JRE + the Skia/Tizen build host dependencies.
#
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        ca-certificates \
        curl \
        wget \
        git \
        sudo \
        zip \
        unzip \
        xz-utils \
        make \
        cpio \
        rpm \
        gettext \
        ninja-build \
        python3 \
        openjdk-17-jre-headless \
    && rm -rf /var/lib/apt/lists/*

# Install the .NET SDK (needed for the Cake build scripts).
#
# Run .NET in invariant globalization mode (like the WASM image) so the runtime
# does not require ICU. The Cake build only shells out to the Tizen toolchain and
# does path/string work, so culture-insensitive formatting has no effect on the
# produced native libraries.
ARG DOTNET_SDK_VERSION=10.0.401
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN curl -fsSL https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.sh -o dotnet-install.sh \
    && bash dotnet-install.sh --version ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet --verbose \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet-install.sh \
    && dotnet --info

# Install Tizen Studio + the native app development packages.
#
# The installer refuses to run as root, so create an unprivileged user and
# install as that user (via a login shell so $HOME is set). The installer's Java
# backend also validates that the destination is *under the user's home
# directory*, which is why install-tizen.sh installs to "$HOME/tizen-studio".
# The build itself (run as root via `docker run`) can still read/execute the
# toolchain afterwards. The version, packages, install location and pinned
# installer hash are all fixed inside install-tizen.sh — there are no knobs here.
#
# The tizen user gets passwordless sudo so that, if the Tizen package-manager
# ever needs to apt-install a host dependency we did not pre-install above, it
# can do so instead of failing. With the dependencies above it is not exercised
# in practice, but it keeps the install robust to package-set changes.
ENV TIZEN_STUDIO_HOME=/home/tizen/tizen-studio
COPY ./install-tizen.sh /tmp/install-tizen.sh
RUN useradd --create-home --shell /bin/bash tizen \
    && echo "tizen ALL=(ALL) NOPASSWD:ALL" > /etc/sudoers.d/tizen \
    && chmod 0440 /etc/sudoers.d/tizen \
    && chmod +x /tmp/install-tizen.sh \
    && runuser -l tizen -c "/tmp/install-tizen.sh" \
    && rm -f /tmp/install-tizen.sh

# Skia's gn/ninja build compiles a set of host tools (e.g. the SkSL compiler)
# using the host toolchain, which defaults to `cc` / `c++`. Tizen Studio only
# ships the cross toolchains, so install a host C/C++ compiler for the host
# tools (the target objects are still built with Tizen GCC via `ncli`).
RUN apt-get update \
    && apt-get install -y --no-install-recommends gcc g++ \
    && rm -rf /var/lib/apt/lists/* \
    && cc --version \
    && c++ --version

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
