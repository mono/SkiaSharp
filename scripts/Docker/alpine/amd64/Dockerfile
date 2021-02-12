FROM amd64/alpine:3.9

RUN apk add --no-cache bash curl ca-certificates python git build-base ninja fontconfig-dev
RUN apk add --no-cache samurai --repository http://dl-cdn.alpinelinux.org/alpine/edge/main
RUN apk add --no-cache mono clang gn --repository http://dl-cdn.alpinelinux.org/alpine/edge/testing

RUN cert-sync /etc/ssl/certs/ca-certificates.crt

ENV GN_EXE=gn NINJA_EXE=ninja

WORKDIR /work
