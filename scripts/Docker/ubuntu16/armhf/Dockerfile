FROM amd64/ubuntu:16.04

ARG MONO_VERSION=6.4.0

RUN apt-get update \
    && apt-get install -y apt-transport-https curl wget python \
    && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF \
    && echo "deb https://download.mono-project.com/repo/ubuntu stable-xenial/snapshots/${MONO_VERSION} main" | tee /etc/apt/sources.list.d/mono-official-stable.list \
    && apt-get update \
    && apt-get install -y mono-devel

ARG TOOLCHAIN_VERSION=9.2-2019.12
ARG TOOLCHAIN_URL=https://armkeil.blob.core.windows.net/developer/Files/downloads/gnu-a/${TOOLCHAIN_VERSION}/binrel/gcc-arm-${TOOLCHAIN_VERSION}-x86_64-arm-none-linux-gnueabihf.tar.xz

RUN (mkdir -p /skia-utils/toolchain \
    && cd /skia-utils/toolchain \
    && wget -O toolchain.tar.xz ${TOOLCHAIN_URL})

RUN (mkdir -p /skia-utils/toolchain \
    && cd /skia-utils/toolchain \
    && tar -xJvf toolchain.tar.xz)

RUN mv /skia-utils/toolchain/gcc-arm-9.2-2019.12-x86_64-arm-none-linux-gnueabihf /opt/arm-none-linux-gnueabihf

RUN apt-get install -y git

ENV PATH="${PATH}:/opt/arm-none-linux-gnueabihf/bin"

ARG FONTCONFIG_VERSION=2.11.0-6.7+b1

RUN (mkdir -p /skia-utils/libfontconfig1-dev \
    && cd /skia-utils/libfontconfig1-dev \
    && wget -O libfontconfig1-dev_armhf.deb http://ftp.nl.debian.org/debian/pool/main/f/fontconfig/libfontconfig1-dev_${FONTCONFIG_VERSION}_armhf.deb \
    && ar vx libfontconfig1-dev_armhf.deb \
    && tar -xJvf data.tar.xz)

RUN (cd /skia-utils/libfontconfig1-dev \
    && cp -R usr/lib/arm-linux-gnueabihf/* /opt/arm-none-linux-gnueabihf/arm-none-linux-gnueabihf/lib/ \
    && cp -R usr/include/* /opt/arm-none-linux-gnueabihf/arm-none-linux-gnueabihf/include/ )

RUN (mkdir -p /skia-utils/libfontconfig1 \
    && cd /skia-utils/libfontconfig1 \
    && wget -O libfontconfig1_armhf.deb http://ftp.nl.debian.org/debian/pool/main/f/fontconfig/libfontconfig1_${FONTCONFIG_VERSION}_armhf.deb \
    && ar vx libfontconfig1_armhf.deb \
    && tar -xJvf data.tar.xz)

RUN (cd /skia-utils/libfontconfig1 \
    && cp -R usr/lib/arm-linux-gnueabihf/* /opt/arm-none-linux-gnueabihf/arm-none-linux-gnueabihf/lib/ )

ENV CC=arm-none-linux-gnueabihf-gcc \
    CXX=arm-none-linux-gnueabihf-g++ \
    AR=arm-none-linux-gnueabihf-gcc-ar

WORKDIR /work
