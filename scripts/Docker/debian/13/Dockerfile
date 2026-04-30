# Arguments:
#   DOTNET_SDK_VERSION   - the version of dotnet for the Cake script        [ 10.0 | * ]
#   LLVM_VERSION         - the version of the LLVM compiler                 [ 19 | * ]
#   TOOLCHAIN_VERSION    - the version of the GCC toolchain                 [ 14 | * ]
#   TOOLCHAIN_ARCH       - the multiarch triple                             [ loongarch64-linux-gnu ]
#   TOOLCHAIN_ARCH_SHORT - the dpkg/Debian short architecture               [ loong64 ]
#   TOOLCHAIN_ARCH_TARGET - the LLVM target triple passed to clang          [ loongarch64-linux-gnu ]
#
# This image only services loongarch64. Other archs are routed to
# scripts/Docker/debian/11 (Bullseye + glibc 2.31), which has the
# matching gcc-N-cross packages for those targets.
#
# Trixie main has no loong64 binary archive (loong64 is still a
# debian-ports arch, not yet a release arch), but Trixie does ship
# gcc-14-cross packages built from cross-toolchain-base — packaged
# as amd64 .debs containing a loong64 sysroot under
# /usr/loongarch64-linux-gnu/. The build links libstdc++/libgcc
# statically against that sysroot. Same shape as the Bullseye image,
# just on Trixie's gcc-14 / glibc 2.38.

# Use --platform flag when building to select architecture
# NOTE: Using devcontainers base image because mcr.microsoft.com/mirror/docker/library/debian:13
# is not yet available. This is a Debian 13 (Trixie) image hosted on MCR.
FROM mcr.microsoft.com/devcontainers/base:debian13

USER root

# Set the architecture-specific variables based on the value of the BUILD_ARCH argument
ARG BUILD_ARCH=loongarch64
RUN case ${BUILD_ARCH} in \
        loongarch64) TOOLCHAIN_ARCH=loongarch64-linux-gnu     ; TOOLCHAIN_ARCH_SHORT=loong64 ; TOOLCHAIN_ARCH_TARGET=loongarch64-linux-gnu      ;; \
        *) echo "debian/13 image only supports loongarch64; got: ${BUILD_ARCH}" && exit 1 ;; \
    esac \
    && echo "export TOOLCHAIN_ARCH=${TOOLCHAIN_ARCH}" > /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_SHORT=${TOOLCHAIN_ARCH_SHORT}" >> /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_TARGET=${TOOLCHAIN_ARCH_TARGET}" >> /etc/skia-env

# Install the host build tools.
ARG LLVM_VERSION=19
RUN apt-get update \
    && apt-get install -y \
        curl python3 git ninja-build xz-utils \
        clang-${LLVM_VERSION} lld-${LLVM_VERSION} \
    && rm -rf /var/lib/apt/lists/*

# Install the loong64 gcc-cross sysroot. cross-toolchain-base ships
# loong64 cross packages on Trixie even though loong64 isn't a release
# arch — they install under /usr/loongarch64-linux-gnu/ and bring along
# glibc + libstdc++ + libgcc.
ARG TOOLCHAIN_VERSION=14
RUN . /etc/skia-env \
    && apt-get update \
    && apt-get install -y \
        libc6-dev-${TOOLCHAIN_ARCH_SHORT}-cross \
        libstdc++-${TOOLCHAIN_VERSION}-dev-${TOOLCHAIN_ARCH_SHORT}-cross \
        libgcc-${TOOLCHAIN_VERSION}-dev-${TOOLCHAIN_ARCH_SHORT}-cross \
        binutils-${TOOLCHAIN_ARCH} \
    && rm -rf /var/lib/apt/lists/*

# native/linux-clang-cross/build.cake adds
# `-I/usr/<triple>/include/c++/current` to the GN cflags so the
# version-specific libstdc++ headers can be referenced without baking
# the toolchain version into the build script. Create that symlink and
# strip absolute `/usr/<triple>/lib/` prefixes from the libc /
# libpthread linker scripts so they resolve relative to the cross
# sysroot at link time. (libpthread.so.0 here, not libpthread.so —
# Trixie's glibc 2.41 keeps the linker script under the .0 name.)
RUN . /etc/skia-env \
    && ln -s /usr/${TOOLCHAIN_ARCH}/include/c++/${TOOLCHAIN_VERSION} \
             /usr/${TOOLCHAIN_ARCH}/include/c++/current \
    && sed -i "s|/usr/${TOOLCHAIN_ARCH}/lib/||g" /usr/${TOOLCHAIN_ARCH}/lib/libpthread.so.0 \
    && sed -i "s|/usr/${TOOLCHAIN_ARCH}/lib/||g" /usr/${TOOLCHAIN_ARCH}/lib/libc.so

# loong64 fontconfig: pull from the Trixie pool — the pool/ tree is
# suite-agnostic and serves loong64 .debs even though Trixie's main
# binary lists don't include them. ABI-stable so headers + libfont-
# config.so.1 from the Trixie build drop into the cross sysroot
# cleanly.
RUN . /etc/skia-env \
    && mkdir -p /skia-utils/libfontconfig-dev \
    && cd /skia-utils/libfontconfig-dev \
    && FC_VERSION=2.17.1-5 \
    && APT_REPO=http://deb.debian.org/debian \
    && for pkg in libfontconfig-dev libfontconfig1; do \
           url="${APT_REPO}/pool/main/f/fontconfig/${pkg}_${FC_VERSION}_${TOOLCHAIN_ARCH_SHORT}.deb"; \
           echo "Downloading $url"; \
           curl -L -o "${pkg}.deb" "$url"; \
           ar x "${pkg}.deb" data.tar.xz \
           && tar -xf data.tar.xz \
           && cp -R usr/lib/*/* /usr/${TOOLCHAIN_ARCH}/lib/ \
           && { [ -d usr/include ] && cp -R usr/include/* /usr/${TOOLCHAIN_ARCH}/include/ || true; } \
           && rm -rf *.deb *.tar* debian-binary usr; \
       done

# Install the .NET SDK
ARG DOTNET_SDK_VERSION=10.0.1xx
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN curl https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.sh -L -o dotnet-install.sh \
    && bash dotnet-install.sh --channel ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet --verbose \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet-install.sh \
    && dotnet help \
    && dotnet --info

ENV CC=clang-${LLVM_VERSION} CXX=clang++-${LLVM_VERSION}

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
