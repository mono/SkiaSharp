# Arguments:
#   DOTNET_SDK_VERSION   - the version of dotnet for the Cake script        [ 10.0 | * ]
#   LLVM_VERSION         - the version of the LLVM compiler                 [ 13 | * ]
#   TOOLCHAIN_VERSION    - the version of the GCC toolchain                 [ 10 | * ]
#   TOOLCHAIN_ARCH       - the cross triple                                 [ arm-linux-gnueabihf | aarch64-linux-gnu | i686-linux-gnu | riscv64-linux-gnu | x86_64-linux-gnu ]
#   TOOLCHAIN_ARCH_SHORT - the dpkg/Debian short architecture               [ armhf | arm64 | i386 | riscv64 | amd64 ]
#   TOOLCHAIN_ARCH_TARGET - the clang target triple                         [ armv7a-linux-gnueabihf | aarch64-linux-gnu | i686-linux-gnu | riscv64-linux-gnu | x86_64-linux-gnu ]
#
# Toolchain: clang as the front-end, gcc-cross packages for the per-arch
# sysroot. Each gcc-N-cross .deb is shipped as an amd64 package that
# unpacks a target-arch sysroot under /usr/<triple>/. libSkiaSharp links
# against libstdc++ + libgcc statically. Trying to use Debian's
# `libclang-rt-N-dev`/`libc++-N-dev`/`libunwind-N-dev` multiarch
# packages instead doesn't work on Bullseye: libclang-rt-13-dev wasn't
# split out from libclang-common-13-dev until LLVM 14, riscv64 has no
# Bullseye binary archive so multiarch fails outright there, and the
# carve-outs end up using gcc-cross anyway.

# Use --platform flag when building to select architecture
FROM mcr.microsoft.com/mirror/docker/library/debian:11

# Set the architecture-specific variables based on the value of the BUILD_ARCH argument
# NOTE: x64 keeps `x86-64-linux-gnu` (with hyphen) in TOOLCHAIN_ARCH so
# the apt package name `binutils-${TOOLCHAIN_ARCH}` matches Debian's
# binutils-x86-64-linux-gnu (Debian uses the dpkg/gcc alias spelling
# for cross binutils, not the canonical `x86_64-linux-gnu` triple). The
# follow-up sed in the symlink step normalizes `x86-64` -> `x86_64`
# for the `/usr/<triple>/` filesystem paths, which use the canonical
# spelling.
ARG BUILD_ARCH=arm64
RUN case ${BUILD_ARCH} in \
        arm)         TOOLCHAIN_ARCH=arm-linux-gnueabihf       ; TOOLCHAIN_ARCH_SHORT=armhf   ; TOOLCHAIN_ARCH_TARGET=armv7a-linux-gnueabihf     ;; \
        arm64)       TOOLCHAIN_ARCH=aarch64-linux-gnu         ; TOOLCHAIN_ARCH_SHORT=arm64   ; TOOLCHAIN_ARCH_TARGET=aarch64-linux-gnu          ;; \
        riscv64)     TOOLCHAIN_ARCH=riscv64-linux-gnu         ; TOOLCHAIN_ARCH_SHORT=riscv64 ; TOOLCHAIN_ARCH_TARGET=riscv64-linux-gnu          ;; \
        x86)         TOOLCHAIN_ARCH=i686-linux-gnu            ; TOOLCHAIN_ARCH_SHORT=i386    ; TOOLCHAIN_ARCH_TARGET=i686-linux-gnu             ;; \
        x64)         TOOLCHAIN_ARCH=x86-64-linux-gnu          ; TOOLCHAIN_ARCH_SHORT=amd64   ; TOOLCHAIN_ARCH_TARGET=x86_64-linux-gnu           ;; \
        *) echo "Unsupported architecture: ${BUILD_ARCH}" && exit 1 ;; \
    esac \
    && echo "export TOOLCHAIN_ARCH=${TOOLCHAIN_ARCH}" > /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_SHORT=${TOOLCHAIN_ARCH_SHORT}" >> /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_TARGET=${TOOLCHAIN_ARCH_TARGET}" >> /etc/skia-env

# Install the host build tools.
# clang-13 (rather than 11) — clang-11's bundled mm_malloc.h declares
# posix_memalign() without `throw()` while the cross-target glibc
# stdlib.h declares it with `throw()`. C++ requires matching exception
# specs across redeclarations, so any TU that pulls in <immintrin.h>
# (e.g. modules/skcms/skcms.cc on x86/x64 with SSE) fails to compile
# with clang-11. Fixed upstream in clang-12+ which added the throw()
# spec to the intrinsics header.
ARG LLVM_VERSION=13
RUN apt-get update \
    && apt-get install -y \
        curl python3 git ninja-build xz-utils \
        clang-${LLVM_VERSION} lld-${LLVM_VERSION} \
    && rm -rf /var/lib/apt/lists/*

# Install the gcc-cross sysroot for the target arch.
# GCC 10 (Bullseye default) ships <bit> and <span> in libstdc++ —
# required by Skia post-m143 ("Build everything with c++20", commit
# 50a7aaced1) which uses std::countl_zero / std::popcount / std::span.
# Older Buster (gcc-8) cannot compile modern Skia.
ARG TOOLCHAIN_VERSION=10
RUN . /etc/skia-env \
    && apt-get update \
    && apt-get install -y \
        libstdc++-${TOOLCHAIN_VERSION}-dev-${TOOLCHAIN_ARCH_SHORT}-cross \
        libgcc-${TOOLCHAIN_VERSION}-dev-${TOOLCHAIN_ARCH_SHORT}-cross \
        binutils-${TOOLCHAIN_ARCH} \
    && rm -rf /var/lib/apt/lists/*

# Normalize TOOLCHAIN_ARCH for filesystem path use — the apt package
# name `binutils-x86-64-linux-gnu` uses hyphens, but the cross sysroot
# lives under `/usr/x86_64-linux-gnu/` (underscore). Rewrite from this
# point on so the rest of the Dockerfile (and the build) sees the
# canonical underscored triple.
#
# native/linux-clang-cross/build.cake adds
# `-I/usr/<triple>/include/c++/current` to the GN cflags so the
# version-specific libstdc++ headers can be referenced without baking
# the toolchain version into the build script. Create that symlink and
# strip absolute `/usr/<triple>/lib/` prefixes from the libc /
# libpthread linker scripts so they resolve relative to the cross
# sysroot at link time. (Bullseye glibc 2.31 keeps the libpthread
# linker script under libpthread.so, not libpthread.so.0.)
RUN . /etc/skia-env \
    && TOOLCHAIN_ARCH=$(echo ${TOOLCHAIN_ARCH} | sed 's/x86-64/x86_64/g') \
    && ln -s /usr/${TOOLCHAIN_ARCH}/include/c++/${TOOLCHAIN_VERSION} \
             /usr/${TOOLCHAIN_ARCH}/include/c++/current \
    && sed -i "s|/usr/${TOOLCHAIN_ARCH}/lib/||g" /usr/${TOOLCHAIN_ARCH}/lib/libpthread.so \
    && sed -i "s|/usr/${TOOLCHAIN_ARCH}/lib/||g" /usr/${TOOLCHAIN_ARCH}/lib/libc.so

# Install fontconfig for the target arch. We need BOTH libfontconfig1
# (runtime .so) and libfontconfig1-dev (headers + .a). The pool/ tree
# is suite-agnostic, so for riscv64 (which has no Bullseye binary
# archive) we reach into Trixie's pool — the resulting libfontconfig
# .so.1 / headers drop into the cross sysroot cleanly because the
# fontconfig ABI is stable and runtime resolution happens against
# whatever fontconfig the user's system ships. Other archs use
# Bullseye's own .debs.
RUN . /etc/skia-env \
    && mkdir -p /skia-utils/libfontconfig-dev \
    && cd /skia-utils/libfontconfig-dev \
    && case "${TOOLCHAIN_ARCH_SHORT}" in \
         riscv64) FC_VERSION=2.15.0-2.3 ;; \
         *)       FC_VERSION=2.13.1-4.2 ;; \
       esac \
    && APT_REPO=http://deb.debian.org/debian \
    && TOOLCHAIN_ARCH=$(echo ${TOOLCHAIN_ARCH} | sed 's/x86-64/x86_64/g') \
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
