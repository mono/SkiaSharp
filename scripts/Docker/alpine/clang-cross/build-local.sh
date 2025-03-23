#!/usr/bin/env bash
set -ex

# Parameters:
# $1 - The target architecture to build for     [ arm | arm64 | riscv64 | x86 | x64 ]

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

ARCH="${1:-arm}"

$DIR/../../_clang-cross-common.sh "$DIR" "$ARCH" "musl" "alpine"
