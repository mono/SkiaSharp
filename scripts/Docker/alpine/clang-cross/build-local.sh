#!/usr/bin/env bash
set -ex

# Parameters:
# $1 - The target architecture to build for     [ arm | arm64 | riscv64 | x86 | x64 ]
# $2 - The Alpine distro version                [ 3.17 | 3.20 ]

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

ARCH="${1:-x64}"
case $ARCH in
  riscv64) DEFAULT_ALPINE_VERSION=3.20 ;;
  *)       DEFAULT_ALPINE_VERSION=3.17 ;;
esac

ALPINE_VERSION="${2:-$DEFAULT_ALPINE_VERSION}"

$DIR/../../_clang-cross-common.sh "$DIR" "$ARCH" "$ALPINE_VERSION" "musl" "alpine"
