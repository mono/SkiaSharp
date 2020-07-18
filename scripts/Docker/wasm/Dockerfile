FROM mcr.microsoft.com/dotnet/core/sdk:3.1.201-bionic

ARG EMSCRIPTEN_VERSION=1.39.11

RUN apt-get update \
    && apt-get install -y apt-transport-https curl wget python git make \
    && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF \
    && echo "deb https://download.mono-project.com/repo/ubuntu stable-bionic main" | tee /etc/apt/sources.list.d/mono-official-stable.list \
    && apt-get update \
    && apt-get install -y mono-devel  \
    && rm -rf /var/lib/apt/lists/*

RUN git clone --branch ${EMSCRIPTEN_VERSION} https://github.com/emscripten-core/emsdk ~/emsdk && \
    cd ~/emsdk && \
    ./emsdk install ${EMSCRIPTEN_VERSION} && \
    ./emsdk activate ${EMSCRIPTEN_VERSION}

# Workaround for https://github.com/dotnet/sdk/issues/11108
RUN cd /usr/share/dotnet/sdk/3.1.201/Sdks/Microsoft.NET.Sdk.WindowsDesktop/targets && \
    mv Microsoft.WinFx.props Microsoft.WinFX.props && \
    mv Microsoft.WinFx.targets Microsoft.WinFX.targets

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
