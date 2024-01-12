FROM amd64/alpine:3.9

# Arguments:
#   DOTNET_SDK_VERSION   - the version of dotnet for the Cake script        [ 3.1.412 | * ]

RUN apk add -X https://dl-cdn.alpinelinux.org/alpine/v3.16/main -u alpine-keys --allow-untrusted
RUN apk add --no-cache bash curl wget python python3 git build-base ninja fontconfig-dev libintl clang

# use the specific commit before the tool switched to C++20 which is too new for this old alpine
RUN git clone https://gn.googlesource.com/gn /usr/share/gn \
    && cd /usr/share/gn \
    && git checkout d4be45bb28fbfc16a41a1e02c86137df6815f2dd \
    && python build/gen.py --allow-warning \
    && ninja -C out \
    && /usr/share/gn/out/gn --version

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

ENV GN_EXE=/usr/share/gn/out/gn

WORKDIR /work
