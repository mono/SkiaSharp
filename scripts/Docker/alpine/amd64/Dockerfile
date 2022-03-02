FROM amd64/alpine:3.9

# Arguments:
#   DOTNET_SDK_VERSION   - the version of dotnet for the Cake script        [ 3.1.412 | * ]

RUN apk add --no-cache bash curl wget python python3 git build-base ninja fontconfig-dev libintl
RUN apk add --no-cache samurai --repository http://dl-cdn.alpinelinux.org/alpine/edge/main
RUN apk add --no-cache clang gn --repository http://dl-cdn.alpinelinux.org/alpine/edge/testing

ARG DOTNET_SDK_VERSION=3.1.412

RUN wget -O dotnet.tar.gz https://dotnetcli.azureedge.net/dotnet/Sdk/$DOTNET_SDK_VERSION/dotnet-sdk-$DOTNET_SDK_VERSION-linux-musl-x64.tar.gz \
    && dotnet_sha512='f81ec24d3550bd414fb0fa154007f88a2e2956c73d033c2a82a6cea3402f0a842d1f32e8ffc199c0d9ed86faa8005a76b7c3b130e9cd0c1bea71d0631c9a1bcd' \
    && echo "$dotnet_sha512  dotnet.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -C /usr/share/dotnet -xzf dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet.tar.gz

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT 1

RUN dotnet help

ENV GN_EXE=gn NINJA_EXE=ninja

WORKDIR /work
