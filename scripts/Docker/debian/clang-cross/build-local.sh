#!/usr/bin/env bash
set -ex

# Parameters:
# $1 - The target architecture to build for     [ arm | arm64 | riscv64 | x86 | x64 ]
# $2 - The Debian distro version                [ 10 | 12 ]

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

ARCH="${1:-arm}"
DEBIAN_VERSION="${2:-10}"

$DIR/../../_clang-cross-common.sh "$DIR/$DEBIAN_VERSION" "$ARCH" "$DEBIAN_VERSION" "gnu"
