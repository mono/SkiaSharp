FROM amd64/ubuntu:16.04

ARG MONO_VERSION=6.4.0
ARG TOOLCHAIN_VERSION=9.2-2019.12
ARG TOOLCHAIN_ARCH=arm-none-linux-gnueabihf
ARG FONTCONFIG_VERSION=2.11.0-6.7+b1
ARG FONTCONFIG_ARCH=armhf

# pre-requisites for building (python, git, mono)
RUN apt-get update \
    && apt-get install -y apt-transport-https curl wget python git make \
    && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF \
    && echo "deb https://download.mono-project.com/repo/ubuntu stable-xenial/snapshots/${MONO_VERSION} main" | tee /etc/apt/sources.list.d/mono-official-stable.list \
    && apt-get update \
    && apt-get install -y mono-devel \
    && rm -rf /var/lib/apt/lists/*

# toolchain (gcc/g++)
RUN (mkdir -p /skia-utils/toolchain \
    && cd /skia-utils/toolchain \
    && wget -O toolchain.tar.xz https://armkeil.blob.core.windows.net/developer/Files/downloads/gnu-a/${TOOLCHAIN_VERSION}/binrel/gcc-arm-${TOOLCHAIN_VERSION}-x86_64-${TOOLCHAIN_ARCH}.tar.xz \
    && tar -xJvf toolchain.tar.xz \
    && mv gcc-arm-${TOOLCHAIN_VERSION}-x86_64-${TOOLCHAIN_ARCH} /opt/${TOOLCHAIN_ARCH})

# skia dependencies (fontconfig)
RUN (mkdir -p /skia-utils/libfontconfig1-dev \
    && cd /skia-utils/libfontconfig1-dev \
    && wget -O libfontconfig1-dev.deb http://ftp.nl.debian.org/debian/pool/main/f/fontconfig/libfontconfig1-dev_${FONTCONFIG_VERSION}_${FONTCONFIG_ARCH}.deb \
    && ar vx libfontconfig1-dev.deb \
    && tar -xJvf data.tar.xz \
    && cp -R usr/lib/*/* /opt/${TOOLCHAIN_ARCH}/${TOOLCHAIN_ARCH}/lib/ \
    && cp -R usr/include/* /opt/${TOOLCHAIN_ARCH}/${TOOLCHAIN_ARCH}/include/ )
RUN (mkdir -p /skia-utils/libfontconfig1 \
    && cd /skia-utils/libfontconfig1 \
    && wget -O libfontconfig1.deb http://ftp.nl.debian.org/debian/pool/main/f/fontconfig/libfontconfig1_${FONTCONFIG_VERSION}_${FONTCONFIG_ARCH}.deb \
    && ar vx libfontconfig1.deb \
    && tar -xJvf data.tar.xz \
    && cp -R usr/lib/*/* /opt/${TOOLCHAIN_ARCH}/${TOOLCHAIN_ARCH}/lib/ )

# container environment
ENV PATH="${PATH}:/opt/${TOOLCHAIN_ARCH}/bin"
ENV CC=${TOOLCHAIN_ARCH}-gcc \
    CXX=${TOOLCHAIN_ARCH}-g++ \
    AR=${TOOLCHAIN_ARCH}-gcc-ar

WORKDIR /work
