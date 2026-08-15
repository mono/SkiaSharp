# glibc cross-compilation image for ppc64le (powerpc64le-linux-gnu).
#
# The .NET cross-image for ppc64le exists but does NOT include libc++.
# This Dockerfile mirrors glibc-x86/Dockerfile: builds libc++ from source,
# then layers the SkiaSharp build environment on top.
#
# Based on: dotnet/dotnet-buildtools-prereqs-docker
#   src/azurelinux/3.0/net10.0/cross/amd64/Dockerfile
 
# ===========================================================================
# Stage 1: Build libc++ for ppc64le
# ===========================================================================
 
ARG ROOTFS_DIR=/crossrootfs/ppc64le
 
FROM mcr.microsoft.com/dotnet-buildtools/prereqs:azurelinux-3.0-net10.0-crossdeps-builder-amd64 AS builder
ARG ROOTFS_DIR
 
RUN PYTHON_EXECUTABLE="$ROOTFS_PIP_HOME/bin/python" /scripts/eng/common/cross/build-rootfs.sh ppc64le bionic --skipunmount --skipemulation
 
RUN TARGET_TRIPLE="powerpc64le-linux-gnu" && \
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
        -DCMAKE_POSITION_INDEPENDENT_CODE=ON \
        -DCMAKE_SYSROOT="$ROOTFS_DIR" \
        -DCMAKE_EXE_LINKER_FLAGS="-fuse-ld=lld" \
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
        -DCOMPILER_RT_BUILD_LIBFUZZER=OFF \
        -DCOMPILER_RT_DEFAULT_TARGET_ONLY=ON \
        -DCOMPILER_RT_INSTALL_PATH="/usr/local/lib/clang/$CLANG_MAJOR_VERSION" && \
    cmake --build runtimes -j $(getconf _NPROCESSORS_ONLN) && \
    cmake --install runtimes --prefix "$ROOTFS_DIR/usr"
 
# ===========================================================================
# Stage 2: Final image with libc++ + SkiaSharp build environment
# ===========================================================================
 
FROM mcr.microsoft.com/dotnet-buildtools/prereqs:azurelinux-3.0-net10.0-crossdeps-llvm-amd64
ARG ROOTFS_DIR
 
COPY --from=builder /usr/local/lib/clang /usr/local/lib/clang/
COPY --from=builder $ROOTFS_DIR $ROOTFS_DIR
 
SHELL ["/bin/bash", "-euo", "pipefail", "-c"]
 
# Toolchain variables for ppc64le (hardcoded).
RUN echo "export TOOLCHAIN_ARCH=powerpc64le-linux-gnu" > /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_SHORT=ppc64el" >> /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_TARGET=powerpc64le-linux-gnu" >> /etc/skia-env \
    && echo "export SYSROOT_PATH=/crossrootfs/ppc64le/usr" >> /etc/skia-env \
    && echo "export SYSROOT_ROOT=/crossrootfs/ppc64le" >> /etc/skia-env \
    && GCC_DIR=$(find /crossrootfs/ppc64le/usr/lib/gcc -maxdepth 3 -name "crtbeginS.o" -exec dirname {} \; | sort -V | tail -1) \
    && if [ -z "$GCC_DIR" ] || [ ! -d "$GCC_DIR" ]; then \
         echo "FATAL: GCC lib dir not found" && exit 1 ; \
       fi \
    && echo "export GCC_LIB_DIR=${GCC_DIR}" >> /etc/skia-env \
    && if [ ! -d "/crossrootfs/ppc64le/usr/include/c++/v1" ]; then \
         echo "FATAL: libc++ headers not found" ; \
         exit 1 ; \
       fi
 
# Install .NET SDK
ARG DOTNET_SDK_VERSION=10.0.108
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN curl -fsSL https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.sh -o dotnet-install.sh \
    && bash dotnet-install.sh --version ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet --verbose \
    && rm dotnet-install.sh \
    && dotnet --info
 
ENV CC=clang CXX=clang++
 
RUN tdnf install -y zstd && tdnf clean all
 
# Install fontconfig headers into the sysroot (ppc64el).
RUN . /etc/skia-env \
    && FC_REPO=http://ports.ubuntu.com/ubuntu-ports \
    && FC_VERSION=2.15.0-1.1ubuntu2 \
    && FC_DEV_PKG=libfontconfig-dev \
    && FC_DEV_SHA256=3f3e986a8dc0c3b311b8ba57521291be342d86b6e01774d07c858b4f4b5c6983 \
    && FC_LIB_SHA256=152b5f61a7c3831242fe5bc43a653907044f7fd7231d94207a85cf6fb05fb00f \
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
    && if [ ! -f "${SYSROOT_ROOT}/usr/include/fontconfig/fontconfig.h" ]; then \
         echo "FATAL: fontconfig.h not found" ; \
         exit 1 ; \
       fi \
    && rm -rf /tmp/fontconfig
 
WORKDIR /work
 
COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
