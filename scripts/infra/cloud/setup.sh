#!/usr/bin/env bash

set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "$script_dir/../../.." && pwd)"
profile="${1:-}"

case "$profile" in
    coding-agent | code-review | skia-sync)
        ;;
    *)
        echo "Usage: $0 {coding-agent|code-review|skia-sync}" >&2
        exit 1
        ;;
esac

bash "$script_dir/install-linux-dependencies.sh"
bash "$repo_root/scripts/infra/tests/start-xvfb.sh"

export CC=clang
export CXX=clang++
if [[ -n "${GITHUB_ENV:-}" ]]; then
    {
        echo "CC=$CC"
        echo "CXX=$CXX"
    } >> "$GITHUB_ENV"
fi

case "$profile" in
    coding-agent)
        bash "$script_dir/restore-dotnet-tools.sh"
        bash "$script_dir/download-prebuilt-natives.sh"
        ;;
    code-review)
        bash "$script_dir/restore-dotnet-tools.sh"
        ;;
    skia-sync)
        bash "$script_dir/install-android-workload.sh"
        ;;
esac
