FROM mcr.microsoft.com/vscode/devcontainers/dotnetcore:3.1

RUN apt-get update \
    && export DEBIAN_FRONTEND=noninteractive \
    && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF \
    && echo "deb https://download.mono-project.com/repo/ubuntu stable-bionic main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list \
    && curl -L https://apt.llvm.org/llvm-snapshot.gpg.key | apt-key add - \
    && echo "deb http://apt.llvm.org/xenial/ llvm-toolchain-xenial-10 main" | tee /etc/apt/sources.list.d/llvm.list \
    && apt-get update \
    && apt-get -y install --no-install-recommends \
      python \
      gnupg \
      ca-certificates \
      mono-devel \
      libfontconfig1-dev \
      clang-10 \
      msbuild

ENV CC=clang-10 CXX=clang++-10
