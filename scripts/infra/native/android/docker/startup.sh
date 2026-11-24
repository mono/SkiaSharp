#!/usr/bin/env bash
set -e

echo "Android NDK: ${ANDROID_NDK_HOME}"
echo "Container CPUs: $(nproc)"
echo "Ninja: $(ninja --version)"

exec "$@"
