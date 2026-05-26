# glibc cross-compilation image for x86 (i686-linux-gnu).
#
# This is a self-contained Dockerfile that:
#   1. Builds libc++ for x86 (stage: libcxx-builder)
#   2. Layers the shared SkiaSharp build environment on top (stage: final)
#
# The .NET team builds libc++ for amd64/arm64/arm/riscv64/loongarch64 but NOT x86
# because they don't ship linux-x86 binaries. We need libc++ for x86 because Skia
# requires C++17 and the build uses -stdlib=libc++ which expects libc++ headers at
# c++/v1/. The x86 sysroot only has GCC 6/7 libstdc++ (incomplete C++17 support).
#
# SOURCE for the libc++ build stage: dotnet/dotnet-buildtools-prereqs-docker
#   src/azurelinux/3.0/net10.0/cross/amd64/Dockerfile
#   https://github.com/dotnet/dotnet-buildtools-prereqs-docker/blob/main/src/azurelinux/3.0/net10.0/cross/amd64/Dockerfile
#
# CHANGES from the amd64 original (only actual diffs listed):
#
#   1. ROOTFS_DIR: /crossrootfs/x64 → /crossrootfs/x86
#      The .NET cross images use /crossrootfs/<arch> as the sysroot mount point.
#      The shared Dockerfile expects this path via /etc/skia-env SYSROOT_ROOT.
#
#   2. build-rootfs.sh: x64 bionic → x86 bionic
#      build-rootfs.sh is a .NET script that debootstraps a minimal Ubuntu sysroot.
#      "x86" selects i386 packages; "bionic" (18.04) matches the distro used by
#      the other .NET cross-images (arm64, x64, arm all use bionic sysroots).
#
#   3. TARGET_TRIPLE: x86_64-linux-gnu → i686-linux-gnu
#      Controls clang's -target flag for cross-compilation. i686 produces 32-bit
#      x86 code. This triple must match the sysroot's lib/gcc directory layout
#      (i686-linux-gnu/) so cmake finds the correct GCC headers and CRT objects.
#
# EVERYTHING ELSE is identical to upstream: cmake flags, compiler-rt, libstdc++
# ABI layer, GCC include path discovery, PIC, lld linker, sysroot settings.
#
# WHEN TO RE-SYNC:
#   If the upstream amd64 Dockerfile changes (new LLVM version, new cmake flags),
#   re-apply those changes here with the 3 substitutions above (ROOTFS, arch, triple).
#   The shared layer below (stage: final) should match ../glibc/Dockerfile exactly.

# ===========================================================================
# Stage 1: Build libc++ for x86 (mirrors upstream amd64 Dockerfile)
# ===========================================================================

ARG ROOTFS_DIR=/crossrootfs/x86

FROM mcr.microsoft.com/dotnet-buildtools/prereqs:azurelinux-3.0-net10.0-crossdeps-builder-amd64 AS libcxx-builder
ARG ROOTFS_DIR

RUN PYTHON_EXECUTABLE="$ROOTFS_PIP_HOME/bin/python" /scripts/eng/common/cross/build-rootfs.sh x86 bionic --skipunmount --skipemulation

RUN TARGET_TRIPLE="i686-linux-gnu" && \
    GCC_VER=$(basename "$(find "$ROOTFS_DIR/usr/include/c++/" -mindepth 1 -maxdepth 1 -type d | sort -V | head -n1)") && \
    CPP_INCLUDES="$ROOTFS_DIR/usr/include/c++/$GCC_VER" && \
    TRIPLET_INCLUDES=$([ -e "$CPP_INCLUDES/$TARGET_TRIPLE" ] && echo "$CPP_INCLUDES/$TARGET_TRIPLE" || echo "$(realpath "$CPP_INCLUDES/../../$TARGET_TRIPLE");$(realpath "$CPP_INCLUDES/../../$TARGET_TRIPLE/c++/$GCC_VER")") && \
    CLANG_MAJOR_VERSION=$(clang --version | grep -oP "(?<=version )\d+") && \
    cmake -S llvm-project.src/runtimes -B runtimes \
        -DCMAKE_BUILD_TYPE=Release \
        -DCMAKE_ASM_COMPILER=clang \
        -DCMAKE_C_COMPILER=clang \
        -DCMAKE_CXX_COMPILER=clang++ \
        -DCMAKE_ASM_COMPILER_TARGET="$TARGET_TRIPLE" \
        -DCMAKE_C_COMPILER_TARGET="$TARGET_TRIPLE" \
        -DCMAKE_CXX_COMPILER_TARGET="$TARGET_TRIPLE" \
        # We're going to link the static libraries we build here into PIC images, so build the static libraries as PIC.
        -DCMAKE_POSITION_INDEPENDENT_CODE=ON \
        -DCMAKE_SYSROOT="$ROOTFS_DIR" \
        # Specify linker to use for exes directly for CMake toolchain detection
        -DCMAKE_EXE_LINKER_FLAGS="-fuse-ld=lld" \
        # Don't search for tools in the sysroot as we're cross-compiling
        -DCMAKE_FIND_ROOT_PATH_MODE_PROGRAM="NEVER" \
        -DLLVM_USE_LINKER=lld \
        -DLLVM_ENABLE_RUNTIMES="libcxx;compiler-rt" \
        -DLIBCXX_ENABLE_SHARED=OFF \
        -DLIBCXX_CXX_ABI=libstdc++ \
        -DLIBCXX_CXX_ABI_INCLUDE_PATHS="$CPP_INCLUDES;$TRIPLET_INCLUDES" \
        -DCOMPILER_RT_CXX_LIBRARY="libcxx" \
        -DCOMPILER_RT_STATIC_CXX_LIBRARY=ON \
        -DCOMPILER_RT_BUILD_SANITIZERS=OFF \ 
        -DCOMPILER_RT_BUILD_MEMPROF=OFF \
        # The libfuzzer build in LLVM doesn't correctly forward the required CMake properties to the "fuzzed libc++" build
        # so skip it here.
        -DCOMPILER_RT_BUILD_LIBFUZZER=OFF \
        -DCOMPILER_RT_DEFAULT_TARGET_ONLY=ON \
        -DCOMPILER_RT_INSTALL_PATH="/usr/local/lib/clang/$CLANG_MAJOR_VERSION" && \
    cmake --build runtimes -j $(getconf _NPROCESSORS_ONLN) && \
    cmake --install runtimes --prefix "$ROOTFS_DIR/usr"

# ===========================================================================
# Stage 2: Final image with libc++ + SkiaSharp build environment
# This section mirrors ../glibc/Dockerfile exactly (hardcoded for x86).
# ===========================================================================

FROM mcr.microsoft.com/dotnet-buildtools/prereqs:azurelinux-3.0-net10.0-crossdeps-llvm-amd64
ARG ROOTFS_DIR

COPY --from=libcxx-builder /usr/local/lib/clang /usr/local/lib/clang/
COPY --from=libcxx-builder $ROOTFS_DIR $ROOTFS_DIR

SHELL ["/bin/bash", "-euo", "pipefail", "-c"]

# Toolchain variables for x86 (hardcoded — this Dockerfile is x86-only).
RUN echo "export TOOLCHAIN_ARCH=i386-linux-gnu" > /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_SHORT=i386" >> /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_TARGET=i686-linux-gnu" >> /etc/skia-env \
    && echo "export SYSROOT_PATH=/crossrootfs/x86/usr" >> /etc/skia-env \
    && echo "export SYSROOT_ROOT=/crossrootfs/x86" >> /etc/skia-env \
    && GCC_DIR=$(find /crossrootfs/x86/usr/lib/gcc -maxdepth 3 -name "crtbeginS.o" -exec dirname {} \; | head -1) \
    && if [ -z "$GCC_DIR" ] || [ ! -d "$GCC_DIR" ]; then \
         echo "FATAL: GCC lib dir not found (crtbeginS.o missing under /crossrootfs/x86/usr/lib/gcc)" && exit 1 ; \
       fi \
    && echo "export GCC_LIB_DIR=${GCC_DIR}" >> /etc/skia-env \
    # Verify libc++ headers exist — deterministic build requirement.
    && if [ ! -d "/crossrootfs/x86/usr/include/c++/v1" ]; then \
         echo "FATAL: libc++ headers not found at /crossrootfs/x86/usr/include/c++/v1" ; \
         exit 1 ; \
       fi

# Install the .NET SDK (needed for Cake build scripts).
ARG DOTNET_SDK_VERSION=10.0.108
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN curl -fsSL https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.sh -o dotnet-install.sh \
    && bash dotnet-install.sh --version ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet --verbose \
    && rm dotnet-install.sh \
    && dotnet --info

ENV CC=clang CXX=clang++

# zstd is needed to extract newer .deb packages (Ubuntu 22.04+ uses zstd compression).
RUN tdnf install -y zstd && tdnf clean all

# Install fontconfig headers into the sysroot (x86/i386).
RUN . /etc/skia-env \
    && FC_REPO=http://archive.ubuntu.com/ubuntu \
    && FC_VERSION=2.12.6-0ubuntu2 \
    && FC_DEV_PKG=libfontconfig1-dev \
    && FC_DEV_SHA256=cd775534e5377dbb26a789781bfd06b93ca63fe366eec9304b7339b54ba55905 \
    && FC_LIB_SHA256=f4324b87b99f1750241d9b61d2a0658ccc76ff8d85e539d4f7f4a07723941119 \
    && mkdir -p /tmp/fontconfig && cd /tmp/fontconfig \
    && for pkg_info in \
         "${FC_DEV_PKG} ${FC_DEV_SHA256}" \
         "libfontconfig1 ${FC_LIB_SHA256}"; do \
         pkg=$(echo $pkg_info | cut -d' ' -f1) ; \
         expected_sha=$(echo $pkg_info | cut -d' ' -f2) ; \
         url="${FC_REPO}/pool/main/f/fontconfig/${pkg}_${FC_VERSION}_${TOOLCHAIN_ARCH_SHORT}.deb" ; \
         echo "Downloading $url" ; \
         curl -fSL -o "${pkg}.deb" "$url" ; \
         actual_sha=$(sha256sum "${pkg}.deb" | cut -d' ' -f1) ; \
         if [ "$actual_sha" != "$expected_sha" ]; then \
           echo "FATAL: SHA-256 mismatch for ${pkg}.deb" ; \
           echo "  Expected: $expected_sha" ; \
           echo "  Actual:   $actual_sha" ; \
           exit 1 ; \
         fi ; \
         echo "  SHA-256 verified: $actual_sha" ; \
         ar x "${pkg}.deb" ; \
         tar xf data.tar.* -C ${SYSROOT_ROOT}/ ; \
         rm -f control.tar.* data.tar.* debian-binary "${pkg}.deb" ; \
       done \
    && rm -rf /tmp/fontconfig

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
