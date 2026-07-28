#!/usr/bin/env bash

set -euo pipefail

if command -v google-chrome >/dev/null 2>&1; then
    chrome_path="$(command -v google-chrome)"
else
    package="$(mktemp --suffix=.deb)"
    trap 'rm -f "$package"' EXIT

    curl --fail --location --retry 5 \
        --output "$package" \
        https://dl.google.com/linux/direct/google-chrome-stable_current_amd64.deb

    sudo apt-get update
    sudo apt-get install -y "$package"

    chrome_path="$(command -v google-chrome)"
fi

"$chrome_path" --version
echo "##vso[task.setvariable variable=CHROME_PATH]$chrome_path"
