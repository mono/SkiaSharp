FROM amd64/ubuntu:16.04

# Arguments:
#   MONO_VERSION         - the version of mono for the Cake script          [ 6.4.0 | * ]
#   TOOLCHAIN_VERSION    - the version of the GCC toolchain                 [ 7 | * ]
#   TOOLCHAIN_ARCH       - the architecture of the GCC toolchain            [ arm-linux-gnueabihf | aarch64-linux-gnu]
#   TOOLCHAIN_ARCH_SHORT - the short form architecture of the GCC toolchain [ armhf | arm64 ]
#   FONTCONFIG_VERSION   - the exact version of libfontconfig1 to use       [ 2.11.0-6.7+b1 | * ]
#
# To build a arm64 image:
#   --build-arg TOOLCHAIN_ARCH=aarch64-linux-gnu --build-arg TOOLCHAIN_ARCH_SHORT=arm64

# pre-requisites for building (python, git, mono)
ARG MONO_VERSION=6.4.0
RUN apt-get update \
    && apt-get install -y apt-transport-https curl wget python git make \
    && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF \
    && echo "deb https://download.mono-project.com/repo/ubuntu stable-xenial/snapshots/${MONO_VERSION} main" | tee /etc/apt/sources.list.d/mono-official-stable.list \
    && curl -L https://apt.llvm.org/llvm-snapshot.gpg.key | apt-key add - \
    && echo "deb http://apt.llvm.org/xenial/ llvm-toolchain-xenial-10 main" | tee /etc/apt/sources.list.d/llvm.list \
    && apt-get update \
    && apt-get install -y mono-devel clang-10 \
    && rm -rf /var/lib/apt/lists/*

# toolchain (gcc/g++)
ARG TOOLCHAIN_VERSION=7
ARG TOOLCHAIN_ARCH=arm-linux-gnueabihf
ARG TOOLCHAIN_ARCH_SHORT=armhf
RUN apt-get update \
    && apt-get install -y \
         libstdc++-${TOOLCHAIN_VERSION}-dev-${TOOLCHAIN_ARCH_SHORT}-cross \
         libgcc-${TOOLCHAIN_VERSION}-dev-${TOOLCHAIN_ARCH_SHORT}-cross \
         binutils-${TOOLCHAIN_ARCH} \
    && rm -rf /var/lib/apt/lists/*

# make it easier for the build script
RUN ln -s /usr/${TOOLCHAIN_ARCH}/include/c++/${TOOLCHAIN_VERSION}.* /usr/${TOOLCHAIN_ARCH}/include/c++/current \
    && sed -i "s/\/usr\/${TOOLCHAIN_ARCH}\/lib\///g" /usr/${TOOLCHAIN_ARCH}/lib/libpthread.so \
    && sed -i "s/\/usr\/${TOOLCHAIN_ARCH}\/lib\///g" /usr/${TOOLCHAIN_ARCH}/lib/libc.so

# skia dependencies (fontconfig)
ARG FONTCONFIG_VERSION=2.11.0-6.7+b1
RUN (mkdir -p /skia-utils/libfontconfig1-dev \
    && cd /skia-utils/libfontconfig1-dev \
    && wget -O libfontconfig1-dev.deb http://ftp.nl.debian.org/debian/pool/main/f/fontconfig/libfontconfig1-dev_${FONTCONFIG_VERSION}_${TOOLCHAIN_ARCH_SHORT}.deb \
    && ar vx libfontconfig1-dev.deb \
    && tar -xJvf data.tar.xz \
    && rm libfontconfig1-dev.deb \
    && cp -R usr/lib/*/* /usr/${TOOLCHAIN_ARCH}/lib/ \
    && cp -R usr/include/* /usr/${TOOLCHAIN_ARCH}/include/ )
RUN (mkdir -p /skia-utils/libfontconfig1 \
    && cd /skia-utils/libfontconfig1 \
    && wget -O libfontconfig1.deb http://ftp.nl.debian.org/debian/pool/main/f/fontconfig/libfontconfig1_${FONTCONFIG_VERSION}_${TOOLCHAIN_ARCH_SHORT}.deb \
    && ar vx libfontconfig1.deb \
    && tar -xJvf data.tar.xz \
    && rm libfontconfig1.deb \
    && cp -R usr/lib/*/* /usr/${TOOLCHAIN_ARCH}/lib/ )

# container environment
ENV CC=clang-10 CXX=clang++-10

WORKDIR /work
