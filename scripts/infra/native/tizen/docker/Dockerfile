# Tizen native build toolchain image.
#
# Builds libSkiaSharp.so / libHarfBuzzSharp.so for Tizen 8.0 (x86_64 + aarch64)
# inside a container, instead of on the build agent. Mirrors the WASM image
# (scripts/infra/native/wasm/docker/Dockerfile).
#
# Base: Ubuntu 20.04. Tizen Studio's CLI installer ships as a "ubuntu-64"
# package and its package-manager resolves the native-toolchain host
# dependencies via apt using Ubuntu package names (e.g. python3.8 / libpython3.8,
# rpm2cpio, make, cpio, gettext, libkf5*), so a Debian/Ubuntu base is required.
# Azure Linux was evaluated first but its package manager (tdnf) is incompatible
# with Tizen's apt-based dependency resolver.
#
# Tizen Studio ships its own clang/gcc toolchain + rootstraps, so beyond the
# Tizen host dependencies the base only needs: the .NET SDK (Cake scripts),
# python3 + ninja + git (the Skia gn/ninja build), and a JRE.
#
# Arguments:
#   DOTNET_SDK_VERSION  - the .NET SDK version for the Cake scripts
#   TIZEN_VERSION       - the Tizen Studio version to install
#   TIZEN_PACKAGES      - the Tizen Studio packages to install

FROM mcr.microsoft.com/mirror/docker/library/ubuntu:20.04

SHELL ["/bin/bash", "-euo", "pipefail", "-c"]

ENV DEBIAN_FRONTEND=noninteractive

# Base utilities + JRE + Skia build tools + the Tizen native-toolchain host
# dependencies. We pre-install the host deps so the Tizen package-manager finds
# them already present; the `tizen` user also gets passwordless sudo as a safety
# net for anything the package-manager still wants to apt-install.
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
        python3.8 \
        libpython3.8 \
        libncurses6 \
        openjdk-17-jre-headless \
    && rm -rf /var/lib/apt/lists/*

# Install the .NET SDK (needed for the Cake build scripts).
ARG DOTNET_SDK_VERSION=10.0.108
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
# directory*, so install into the tizen user's home. The build itself (run as
# root via `docker run`) can still read/execute the toolchain afterwards.
#
# Use the "-CLI" native app development packages: they install the same build
# toolchain + tizen-8.0 rootstraps + `tizen build-native` CLI as the IDE
# packages, but without the GUI-only NativeIDE (whose post-install hangs
# headless). See install-tizen.sh for details.
ARG TIZEN_VERSION=10.0
ARG TIZEN_PACKAGES=MOBILE-6.0-NativeAppDevelopment-CLI,TIZEN-8.0-NativeAppDevelopment-CLI
ENV TIZEN_STUDIO_HOME=/home/tizen/tizen-studio
COPY ./install-tizen.sh /tmp/install-tizen.sh
RUN useradd --create-home --shell /bin/bash tizen \
    && echo "tizen ALL=(ALL) NOPASSWD:ALL" > /etc/sudoers.d/tizen \
    && chmod 0440 /etc/sudoers.d/tizen \
    && apt-get update \
    && chmod +x /tmp/install-tizen.sh \
    && runuser -l tizen -c "/tmp/install-tizen.sh '${TIZEN_VERSION}' '${TIZEN_STUDIO_HOME}' '${TIZEN_PACKAGES}'" \
    && rm -f /tmp/install-tizen.sh \
    && rm -rf /var/lib/apt/lists/*

# Skia's gn/ninja build compiles a set of host tools (e.g. the SkSL compiler)
# using the host toolchain, which defaults to `cc` / `c++`. Tizen Studio only
# ships the cross toolchains, so install a host C/C++ compiler for the host
# tools (the target objects are still built with the Tizen llvm via `ncli`).
#
# Tizen Studio's bundled llvm-10 clang links against the legacy ncurses 5
# library (`libtinfo.so.5`), which is not installed by default on Ubuntu 20.04
# (it ships libtinfo6). Install libtinfo5 so the cross compiler can run.
RUN apt-get update \
    && apt-get install -y --no-install-recommends gcc g++ libtinfo5 \
    && rm -rf /var/lib/apt/lists/* \
    && cc --version \
    && c++ --version

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
