# musl (Alpine) cross-compilation image.
#
# Thin layer on top of .NET's official musl cross-compilation images which provide:
#   - Azure Linux 3.0 host (clean CG surface)
#   - Clang 20 + lld (built from source, GPG-verified)
#   - Complete Alpine sysroot with musl headers, kernel headers
#   - Cross-compiled libc++ (static)
#
# We add: .NET SDK (Cake), fontconfig-dev from Alpine repos, skia-env toolchain vars.
#
# Usage (BUILD_ARCH is the only input — the x64→amd64 image-tag mapping is
# handled internally via stage aliases below):
#   docker build --build-arg BUILD_ARCH=arm64 .
#   docker build --build-arg BUILD_ARCH=x64 .
#
# Supported architectures: arm, arm64, x64, riscv64, loongarch64

ARG BUILD_ARCH=arm64

# One stage per arch — .NET cross images use "amd64" instead of "x64", so we
# map it here in a single place. BuildKit only pulls the stage referenced by
# the final `FROM base-${BUILD_ARCH}`; the others are inert.
FROM mcr.microsoft.com/dotnet-buildtools/prereqs:azurelinux-3.0-net10.0-cross-arm-musl         AS base-arm
FROM mcr.microsoft.com/dotnet-buildtools/prereqs:azurelinux-3.0-net10.0-cross-arm64-musl       AS base-arm64
FROM mcr.microsoft.com/dotnet-buildtools/prereqs:azurelinux-3.0-net10.0-cross-amd64-musl       AS base-x64
FROM mcr.microsoft.com/dotnet-buildtools/prereqs:azurelinux-3.0-net10.0-cross-riscv64-musl     AS base-riscv64
FROM mcr.microsoft.com/dotnet-buildtools/prereqs:azurelinux-3.0-net10.0-cross-loongarch64-musl AS base-loongarch64

FROM base-${BUILD_ARCH}

SHELL ["/bin/bash", "-euo", "pipefail", "-c"]

ARG BUILD_ARCH

# Derive all toolchain variables from BUILD_ARCH.
RUN case "${BUILD_ARCH}" in \
      arm)         TOOLCHAIN_ARCH=armv7-alpine-linux-musleabihf; \
                   APK_ARCH=armv7; \
                   ROOTFS_DIR=/crossrootfs/arm ;; \
      arm64)       TOOLCHAIN_ARCH=aarch64-alpine-linux-musl; \
                   APK_ARCH=aarch64; \
                   ROOTFS_DIR=/crossrootfs/arm64 ;; \
      x64)         TOOLCHAIN_ARCH=x86_64-alpine-linux-musl; \
                   APK_ARCH=x86_64; \
                   ROOTFS_DIR=/crossrootfs/x64 ;; \
      riscv64)     TOOLCHAIN_ARCH=riscv64-alpine-linux-musl; \
                   APK_ARCH=riscv64; \
                   ROOTFS_DIR=/crossrootfs/riscv64 ;; \
      loongarch64) TOOLCHAIN_ARCH=loongarch64-alpine-linux-musl; \
                   APK_ARCH=loongarch64; \
                   ROOTFS_DIR=/crossrootfs/loongarch64 ;; \
      *) echo "FATAL: Unsupported BUILD_ARCH=${BUILD_ARCH}" && exit 1 ;; \
    esac \
    && echo "export TOOLCHAIN_ARCH=${TOOLCHAIN_ARCH}" > /etc/skia-env \
    && echo "export TOOLCHAIN_ARCH_TARGET=${TOOLCHAIN_ARCH}" >> /etc/skia-env \
    && echo "export SYSROOT_PATH=${ROOTFS_DIR}/usr" >> /etc/skia-env \
    && echo "export SYSROOT_ROOT=${ROOTFS_DIR}" >> /etc/skia-env \
    && echo "export APK_ARCH=${APK_ARCH}" >> /etc/skia-env \
    && GCC_VER=$(ls ${ROOTFS_DIR}/usr/lib/gcc/${TOOLCHAIN_ARCH}/ 2>/dev/null | sort -V | tail -1) \
    && echo "export GCC_LIB_DIR=${ROOTFS_DIR}/usr/lib/gcc/${TOOLCHAIN_ARCH}/${GCC_VER}" >> /etc/skia-env

# apk.static (musl, static) — used to operate on the Alpine cross sysroot from
# the Azure Linux host (which has no apk of its own). Shared by the purge step
# below and the fontconfig install step further down.
ARG APK_STATIC_VERSION=2.12.14
ARG APK_STATIC_SHA256=cd282c693059c39b68ce0575fbdc4e0c162af85ed633a7834f9cfd01b8b1d58b

# Purge unused Alpine packages from the cross sysroot to shrink the Component
# Governance surface. The base image's sysroot ships a full Alpine build
# toolchain plus a pile of dev/runtime packages we never use (python3, lldb,
# llvm*-libs, libxml2, icu, krb5, lttng-ust, glib, gettext, ncurses, busybox,
# curl, openssl-dev …): we cross-compile with the host's clang/lld + libc++ and
# only need musl + headers, the GCC CRT/libgcc/libstdc++ archives, and the libs
# Skia links against (fontconfig and its freetype/png/expat/bz2/zlib/brotli deps
# are installed in the next step).
#
# apk garbage-collects a package's dependencies once they become orphaned, so we
# first pin everything that must survive into the apk "world", then delete the
# unwanted top-level world members and let apk drop the rest. Removal is done
# with `apk del` (never `rm`) so lib/apk/db/installed — which CG reads — stays in
# sync. The delete set is derived from the sysroot's own world file, so it is
# fully arch-agnostic: arch-specific members (llvm19-libs on riscv64, llvm22-libs
# on loongarch64, …) are handled automatically. `--no-scripts` keeps apk from
# trying to execute target-architecture trigger scripts on the x86_64 host.
RUN . /etc/skia-env \
    && APK_DIR="$(mktemp -d)" \
    && curl -fSL --retry 5 --retry-delay 10 --retry-all-errors \
        -o "$APK_DIR/apk.static" \
        "https://gitlab.alpinelinux.org/api/v4/projects/5/packages/generic/v${APK_STATIC_VERSION}/x86_64/apk.static" \
    && echo "${APK_STATIC_SHA256}  $APK_DIR/apk.static" | sha256sum -c - \
    && chmod +x "$APK_DIR/apk.static" \
    && APK="$APK_DIR/apk.static --root ${SYSROOT_ROOT} --no-network" \
    # World members that the clang/lld cross toolchain still needs — keep these.
    && KEEP_WORLD="compiler-rt linux-headers zlib-dev" \
    # Toolchain + Skia link-time libraries to pin so apk does not collect them
    # when their parent world members (e.g. build-base) are deleted. gcc supplies
    # the CRT objects + libgcc.a used by -static-libgcc and pulls in binutils,
    # which is therefore unavoidable; libstdc++-dev supplies the libstdc++.a that
    # libc++ delegates its ABI to (see native/linux-clang-cross/build.cake).
    # alpine-keys is kept so the apk signing keys (and /etc/apk/keys) survive for
    # the fontconfig-dev install in the next step.
    && PIN="gcc libstdc++ libstdc++-dev libgcc binutils musl musl-dev libc-dev \
            libc-utils musl-utils scanelf fortify-headers libucontext alpine-keys \
            zlib brotli brotli-dev brotli-libs libexpat libbz2" \
    # Pin only the packages actually present (libc-dev/libc-utils are absent on
    # some arches), otherwise apk add would try to fetch them over the network.
    && PIN_PRESENT="" \
    && for p in ${PIN}; do \
         if ${APK} info -e "$p" >/dev/null 2>&1; then PIN_PRESENT="${PIN_PRESENT} $p"; fi; \
       done \
    && ${APK} add --no-scripts ${PIN_PRESENT} \
    # Delete every world member that is neither required nor pinned; apk then
    # autoremoves the dependencies that are no longer referenced.
    && DEL="$(grep -vxF -f <(printf '%s\n' ${KEEP_WORLD} ${PIN_PRESENT}) \
                "${SYSROOT_ROOT}/etc/apk/world" | tr '\n' ' ')" \
    && echo "Purging unused sysroot world members:${DEL:+ }${DEL}" \
    && if [ -n "${DEL}" ]; then \
         ${APK} del --no-scripts --purge --force-broken-world ${DEL}; \
       fi \
    # Fail the build immediately if the purge took out a toolchain artifact the
    # clang/lld cross link depends on, instead of discovering it later in Skia.
    && for f in \
         "${GCC_LIB_DIR}/crtbeginS.o" \
         "${GCC_LIB_DIR}/libgcc.a" \
         "${SYSROOT_ROOT}/usr/lib/libstdc++.a" \
         "${SYSROOT_ROOT}/usr/lib/crt1.o" \
         "${SYSROOT_ROOT}/usr/lib/libc.a" \
         "${SYSROOT_ROOT}/usr/include/bits/alltypes.h" \
         "${SYSROOT_ROOT}/usr/include/linux/limits.h" \
       ; do \
         test -e "$f" || { echo "FATAL: required toolchain artifact missing after purge: $f" >&2; exit 1; }; \
       done \
    && rm -rf "$APK_DIR" \
    && rm -rf "${SYSROOT_ROOT}/var/cache/apk/"* "${SYSROOT_ROOT}/tmp/"*

# Install the .NET SDK (needed for Cake build scripts).
ARG DOTNET_SDK_VERSION=10.0.108
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN curl -fsSL https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.sh -o dotnet-install.sh \
    && bash dotnet-install.sh --version ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet --verbose \
    && rm dotnet-install.sh \
    && dotnet --info

ENV CC=clang CXX=clang++

# Install fontconfig-dev into the sysroot from Alpine repos.
# The sysroot is Alpine 3.17 but we pull fontconfig from 3.21 (newer keys needed).
# SSL_CERT_FILE is needed because apk.static (musl) can't find Azure Linux's CA bundle.
ARG ALPINE_VERSION=3.21
RUN . /etc/skia-env \
    && APK_DIR="$(mktemp -d)" \
    && curl -fSL --retry 5 --retry-delay 10 --retry-all-errors \
        -o "$APK_DIR/apk.static" \
        "https://gitlab.alpinelinux.org/api/v4/projects/5/packages/generic/v${APK_STATIC_VERSION}/x86_64/apk.static" \
    && echo "${APK_STATIC_SHA256}  $APK_DIR/apk.static" | sha256sum -c - \
    && chmod +x "$APK_DIR/apk.static" \
    && mkdir -p ${SYSROOT_ROOT}/etc/apk/keys \
    && curl -fsSL -o ${SYSROOT_ROOT}/etc/apk/keys/alpine-devel@lists.alpinelinux.org-6165ee59.rsa.pub \
        "https://alpinelinux.org/keys/alpine-devel@lists.alpinelinux.org-6165ee59.rsa.pub" \
    && curl -fsSL -o ${SYSROOT_ROOT}/etc/apk/keys/alpine-devel@lists.alpinelinux.org-61666e3f.rsa.pub \
        "https://alpinelinux.org/keys/alpine-devel@lists.alpinelinux.org-61666e3f.rsa.pub" \
    && SSL_CERT_FILE=/etc/pki/tls/certs/ca-bundle.crt "$APK_DIR/apk.static" \
        -X "https://dl-cdn.alpinelinux.org/alpine/v${ALPINE_VERSION}/main" \
        -X "https://dl-cdn.alpinelinux.org/alpine/v${ALPINE_VERSION}/community" \
        --no-cache --root ${SYSROOT_ROOT} --arch "${APK_ARCH}" --no-scripts \
        add fontconfig-dev \
    && rm -rf "$APK_DIR"

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
