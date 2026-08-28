#!/usr/bin/env bash

set -euo pipefail

CHROME_VERSION="150.0.7871.186-1"
CHROME_SHA256="4193e00b6d5d5969ee63f7a69596868f546aa0e8cb077b3e0bf9cc1e2c719d00"
CHROME_PACKAGE_URL="https://dl.google.com/linux/chrome/deb/pool/main/g/google-chrome-stable/google-chrome-stable_${CHROME_VERSION}_amd64.deb"

if command -v google-chrome >/dev/null 2>&1; then
    chrome_path="$(command -v google-chrome)"
else
    package="$(mktemp --suffix=.deb)"
    trap 'rm -f "$package"' EXIT

    curl --fail --location --retry 5 \
        --output "$package" \
        "$CHROME_PACKAGE_URL"

    actual_sha256="$(sha256sum "$package" | cut -d' ' -f1)"
    if [[ "$actual_sha256" != "$CHROME_SHA256" ]]; then
        echo "Chrome SHA-256 mismatch: expected $CHROME_SHA256, got $actual_sha256" >&2
        exit 1
    fi

    package_name="$(dpkg-deb --field "$package" Package)"
    package_version="$(dpkg-deb --field "$package" Version)"
    package_architecture="$(dpkg-deb --field "$package" Architecture)"
    if [[ "$package_name" != "google-chrome-stable" ||
          "$package_version" != "$CHROME_VERSION" ||
          "$package_architecture" != "amd64" ]]; then
        echo "Unexpected Chrome package: $package_name $package_version $package_architecture" >&2
        exit 1
    fi

    sudo apt-get update
    sudo apt-get install -y "$package"

    chrome_path="$(command -v google-chrome)"
fi

"$chrome_path" --version
echo "##vso[task.setvariable variable=CHROME_PATH]$chrome_path"
