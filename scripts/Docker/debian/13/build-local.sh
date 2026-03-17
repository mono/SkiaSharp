#!/usr/bin/env bash
set -ex

# Parameters:
# $1 - The target architecture to build for     [ arm | arm64 | riscv64 | x86 | x64 | loongarch64 ]
# $2+ - Additional arguments to pass to the cake script

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

ARCH="${1:-loongarch64}"
shift 1 || true
EXTRA_ARGS="$@"

$DIR/../../_clang-cross-common.sh "$DIR" "$ARCH" "gnu" "" "$EXTRA_ARGS"
