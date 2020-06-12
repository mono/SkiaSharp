FROM unoplatform/wasm-build:2.1.1

ARG EMSCRIPTEN_VERSION=1.39.11

RUN git clone https://github.com/emscripten-core/emsdk ~/emsdk && \
    cd ~/emsdk && \
    ./emsdk install ${EMSCRIPTEN_VERSION} && \
    ./emsdk activate ${EMSCRIPTEN_VERSION}

WORKDIR /work
