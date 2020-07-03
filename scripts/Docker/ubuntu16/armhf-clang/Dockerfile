FROM amd64/ubuntu:16.04

RUN apt-get update \
    && apt-get install -y apt-transport-https curl wget \
    && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF \
    && echo "deb https://download.mono-project.com/repo/ubuntu stable-xenial/snapshots/6.4.0 main" | tee /etc/apt/sources.list.d/mono-official-stable.list \
    && curl -L https://apt.llvm.org/llvm-snapshot.gpg.key | apt-key add - \
    && echo "deb http://apt.llvm.org/xenial/ llvm-toolchain-xenial-10 main" | tee /etc/apt/sources.list.d/llvm.list \
    && apt-get update \
    && apt-get install -y mono-complete msbuild python git libfontconfig1-dev clang-10 make \
    && rm -rf /var/lib/apt/lists/*

RUN apt-get update
RUN apt-get install -y libstdc++-4.8-dev-armhf-cross libgcc-4.8-dev-armhf-cross binutils-arm-linux-gnueabihf

# ARG FONTCONFIG_VERSION=2.12.6-0ubuntu2
# RUN (mkdir -p /skia-utils \
#     && cd /skia-utils \
#     && wget -O libfontconfig1-dev_armhf.deb http://ftp.nl.debian.org/debian/pool/main/f/fontconfig/libfontconfig1-dev_${FONTCONFIG_VERSION}_armhf.deb \
#     && ar vx libfontconfig1-dev_armhf.deb \
#     && tar -xJvf data.tar.xz)

# ARG LIBJPEG_VERSION=2.12.6-0ubuntu2
# RUN (mkdir -p /skia-utils \
#     && cd /skia-utils \
#     && wget -O libfontconfig1-dev_armhf.deb http://ftp.nl.debian.org/debian/pool/main/f/fontconfig/libfontconfig1-dev_${LIBJPEG_VERSION}_armhf.deb \
#     && ar vx libfontconfig1-dev_armhf.deb \
#     && tar -xJvf data.tar.xz)

ENV CC=clang-10 CXX=clang++-10

WORKDIR /work
