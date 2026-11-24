#!/usr/bin/env bash
set -euo pipefail

git checkout -B bot/release-notes
# Retain the selected source's tools and prepared files, but publish only new docs commits on main.
git reset --mixed origin/main
