#!/usr/bin/env bash
set -ex

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

ARCH=$1
ALPINE_VERSION=$3
if [ -z "$ALPINE_VERSION" ]; then
  case $ARCH in
    riscv64) ALPINE_VERSION=3.20 ;;
    *) ALPINE_VERSION=3.17       ;;
  esac
fi

$DIR/../../_clang-cross-common.sh "$DIR" "$ARCH" "$2" "$ALPINE_VERSION" "musl" "-alpine"
