#!/usr/bin/env bash

set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "$script_dir/../../.." && pwd)"

bash "$script_dir/install-linux-dependencies.sh"
bash "$repo_root/scripts/infra/tests/start-xvfb.sh"
