FROM amd64/ubuntu:16.04

RUN apt-get update \
    # install essentials
    && apt-get install -y apt-transport-https curl wget \
    # install build tools
    && apt-get install -y software-properties-common \
    # add mono packages
    && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF \
    && echo "deb https://download.mono-project.com/repo/ubuntu stable-xenial/snapshots/6.4.0 main" | tee /etc/apt/sources.list.d/mono-official-stable.list \
    # add clang/llvm packages
    && curl -L https://apt.llvm.org/llvm-snapshot.gpg.key | apt-key add - \
    && echo "deb http://apt.llvm.org/xenial/ llvm-toolchain-xenial-10 main" | tee /etc/apt/sources.list.d/llvm.list \
    # add gcc 6 packages
    && add-apt-repository ppa:ubuntu-toolchain-r/test -y \
    # install packages
    && apt-get update \
    && apt-get install -y mono-complete msbuild python git libfontconfig1-dev clang-10 gcc-6 g++-6 make \
    # cleanup
    && rm -rf /var/lib/apt/lists/*

ENV CC=clang-10 CXX=clang++-10

WORKDIR /work
