#!/usr/bin/env bash
set -e

# Make the Tizen Studio CLI available on PATH for the build.
export TIZEN_STUDIO_HOME="${TIZEN_STUDIO_HOME:-/home/tizen/tizen-studio}"
export PATH="${TIZEN_STUDIO_HOME}/tools/ide/bin:${PATH}"

exec "$@"
