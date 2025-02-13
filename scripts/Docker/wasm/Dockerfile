FROM mcr.microsoft.com/dotnet/sdk:8.0

# Arguments:
#   EMSCRIPTEN_VERSION   - the version of the emscripten SDK                [ 3.1.34 | * ]

# default value, but should be provided depending on app
ARG EMSCRIPTEN_VERSION=3.1.34

RUN apt-get update \
    && apt-get install -y apt-transport-https curl wget python3 git make xz-utils ninja-build lbzip2 \
    && rm -rf /var/lib/apt/lists/*

RUN git clone https://github.com/emscripten-core/emsdk ~/emsdk && \
    cd ~/emsdk && \
    ./emsdk install ${EMSCRIPTEN_VERSION} && \
    ./emsdk activate ${EMSCRIPTEN_VERSION}

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
