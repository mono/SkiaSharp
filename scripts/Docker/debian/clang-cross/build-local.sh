#!/usr/bin/env bash
set -ex

# Parameters:
# $1 - The target architecture to build for     [ arm | arm64 | riscv64 | x86 | x64 | loongarch64 ]
# $2 - The Debian distro version                [ 10 | 12 | 13 ]
# $3+ - Additional arguments to pass to the cake script
# LoongArch needs to get the packages from loongnix25

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

ARCH="${1:-arm}"
DEBIAN_VERSION="${2:-10}"
shift 2 || true
EXTRA_ARGS="$@"

$DIR/../../_clang-cross-common.sh "$DIR/$DEBIAN_VERSION" "$ARCH" "gnu" "" "$EXTRA_ARGS"
