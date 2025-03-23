#!/usr/bin/env bash
set -ex

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

# the target architecture to build for
ARCH=$1
DEBIAN_VERSION=$3
if [ -z "$DEBIAN_VERSION" ]; then
  case $ARCH in
    loongarch64) DEBIAN_VERSION=13 ;;
    riscv64) DEBIAN_VERSION=12     ;;
    *) DEBIAN_VERSION=11           ;;
  esac
fi

$DIR/../../_clang-cross-common.sh "$DIR/$DEBIAN_VERSION" "$ARCH" "$2" "$DEBIAN_VERSION" "gnu"
