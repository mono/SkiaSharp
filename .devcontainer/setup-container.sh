#!/usr/bin/env bash
set -e

echo "Restoring developer tools..."
dotnet tool restore

echo "Downloading latest native assets..."
dotnet cake --target=externals-download

echo "Setup complete."
