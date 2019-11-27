FROM amd64/ubuntu:16.04

RUN apt-get update \
    && apt-get install -y apt-transport-https curl wget \
    && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF \
    && echo "deb https://download.mono-project.com/repo/ubuntu stable-xenial/snapshots/5.20.1.34 main" | tee /etc/apt/sources.list.d/mono-official-stable.list \
    && apt-get update \
    && apt-get install -y mono-complete msbuild python git libfontconfig1-dev clang-4.0 make \
    && rm -rf /var/lib/apt/lists/*

ENV CC=clang-4.0 CXX=clang++-4.0

WORKDIR /work
