FROM amd64/ubuntu:16.04

RUN apt-get update \
    && apt-get install -y apt-transport-https curl wget \
    && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF \
    && echo "deb https://download.mono-project.com/repo/ubuntu stable-xenial/snapshots/6.4.0 main" | tee /etc/apt/sources.list.d/mono-official-stable.list \
    && curl -L https://apt.llvm.org/llvm-snapshot.gpg.key | apt-key add - \
    && echo "deb http://apt.llvm.org/xenial/ llvm-toolchain-xenial-9 main" | tee /etc/apt/sources.list.d/llvm.list \
    && apt-get update \
    && apt-get install -y mono-complete msbuild python git libfontconfig1-dev clang-9 make \
    && rm -rf /var/lib/apt/lists/*

ENV CC=clang-9 CXX=clang++-9

WORKDIR /work
