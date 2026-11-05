#!/usr/bin/env bash

set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "$script_dir/../../.." && pwd)"
restore_tools=false
download_prebuilt_natives=false
install_android_workload=false

while [[ $# -gt 0 ]]; do
    case "$1" in
        --restore-tools)
            restore_tools=true
            ;;
        --download-prebuilt-natives)
            download_prebuilt_natives=true
            ;;
        --install-android-workload)
            install_android_workload=true
            ;;
        *)
            echo "Usage: $0 [--restore-tools] [--download-prebuilt-natives] [--install-android-workload]" >&2
            exit 1
            ;;
    esac
    shift
done

bash "$script_dir/install-linux-dependencies.sh"
bash "$repo_root/scripts/infra/tests/setup-software-gpu.sh"

export CC=clang
export CXX=clang++
if [[ -n "${GITHUB_ENV:-}" ]]; then
    {
        echo "CC=$CC"
        echo "CXX=$CXX"
    } >> "$GITHUB_ENV"
fi

if [[ "$restore_tools" == true ]]; then
    bash "$script_dir/restore-dotnet-tools.sh"
fi

# Prebuilt natives are only safe when the caller knows it is doing managed-only work.
if [[ "$download_prebuilt_natives" == true ]]; then
    bash "$script_dir/download-prebuilt-natives.sh"
fi

# The repository's standard managed build includes Android TFMs.
if [[ "$install_android_workload" == true ]]; then
    bash "$script_dir/install-android-workload.sh"
fi
