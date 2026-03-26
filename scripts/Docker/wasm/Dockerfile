FROM mcr.microsoft.com/mirror/docker/library/debian:12

# Arguments:
#   DOTNET_SDK_VERSION   - the version of dotnet for the Cake script        [ 10.0.1xx | * ]
#   EMSCRIPTEN_VERSION   - the version of the emscripten SDK                [ 3.1.34 | * ]

# default value, but should be provided depending on app
ARG EMSCRIPTEN_VERSION=3.1.34

RUN apt-get update \
    && apt-get install -y apt-transport-https curl wget python3 git make xz-utils ninja-build lbzip2 \
    && rm -rf /var/lib/apt/lists/*

# Install the .NET SDK
ARG DOTNET_SDK_VERSION=10.0.1xx
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
RUN curl https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.sh -L -o dotnet-install.sh \
    && bash dotnet-install.sh --channel ${DOTNET_SDK_VERSION} --install-dir /usr/share/dotnet --verbose \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet-install.sh \
    && dotnet help \
    && dotnet --info

RUN git clone https://github.com/emscripten-core/emsdk ~/emsdk && \
    cd ~/emsdk && \
    ./emsdk install ${EMSCRIPTEN_VERSION} && \
    ./emsdk activate ${EMSCRIPTEN_VERSION}

WORKDIR /work

COPY ./startup.sh /
RUN chmod +x /startup.sh
ENTRYPOINT [ "/startup.sh" ]
