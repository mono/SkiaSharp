FROM mcr.microsoft.com/dotnet/sdk:7.0

# Arguments:
#   EMSCRIPTEN_VERSION   - the version of the emscripten SDK                [ 2.0.23 | * ]

# default value, but should be provided depending on app
ARG EMSCRIPTEN_VERSION=2.0.23

RUN apt-get update \
    && apt-get install -y apt-transport-https curl wget python python3 git make xz-utils ninja-build \
    && rm -rf /var/lib/apt/lists/*

RUN git clone --branch ${EMSCRIPTEN_VERSION} https://github.com/emscripten-core/emsdk ~/emsdk && \
    cd ~/emsdk && \
    ./emsdk install ${EMSCRIPTEN_VERSION} && \
    ./emsdk activate ${EMSCRIPTEN_VERSION}

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
