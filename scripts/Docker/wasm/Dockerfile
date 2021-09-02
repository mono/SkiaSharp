FROM mcr.microsoft.com/dotnet/core/sdk:3.1.412-bionic

# Arguments:
#   EMSCRIPTEN_VERSION   - the version of the emscripten SDK                [ 2.0.23 | * ]

# default value, but should be provided depending on app
ARG EMSCRIPTEN_VERSION=2.0.23

RUN apt-get update \
    && apt-get install -y apt-transport-https curl wget python python3 git make \
    && rm -rf /var/lib/apt/lists/*

RUN git clone --branch ${EMSCRIPTEN_VERSION} https://github.com/emscripten-core/emsdk ~/emsdk && \
    cd ~/emsdk && \
    ./emsdk install ${EMSCRIPTEN_VERSION} && \
    ./emsdk activate ${EMSCRIPTEN_VERSION}

# Workaround for https://github.com/dotnet/sdk/issues/11108
RUN cd /usr/share/dotnet/sdk/3.1.412/Sdks/Microsoft.NET.Sdk.WindowsDesktop/targets && \
    mv Microsoft.WinFx.props Microsoft.WinFX.props && \
    mv Microsoft.WinFx.targets Microsoft.WinFX.targets

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
