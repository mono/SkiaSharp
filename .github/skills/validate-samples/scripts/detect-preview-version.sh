#!/bin/bash
# detect-preview-version.sh
# Detects the preview label and build number from downloaded nupkg files in output/nugets/
# Usage: source detect-preview-version.sh
#   Sets: PREVIEW_LABEL, BUILD_NUMBER, PREVIEW_SUFFIX

set -euo pipefail

NUGETS_DIR="${1:-output/nugets}"

# Find a preview SkiaSharp package (not NativeAssets, not HarfBuzz)
PREVIEW_PKG=$(ls "$NUGETS_DIR"/SkiaSharp.[0-9]*-*.nupkg 2>/dev/null | grep -v NativeAssets | head -1)

if [ -z "$PREVIEW_PKG" ]; then
    echo "ERROR: No preview SkiaSharp package found in $NUGETS_DIR" >&2
    echo "Run 'dotnet cake --target=docs-download-output' first." >&2
    exit 1
fi

# Extract filename
FILENAME=$(basename "$PREVIEW_PKG")
echo "Found: $FILENAME"

# Extract suffix: SkiaSharp.3.119.4-preview.0.76.nupkg → preview.0.76
SUFFIX=$(echo "$FILENAME" | sed 's/^SkiaSharp\.[0-9]*\.[0-9]*\.[0-9]*-//' | sed 's/\.nupkg$//')

# Split on last dot: preview.0 + 76
PREVIEW_LABEL=$(echo "$SUFFIX" | sed 's/\.[0-9]*$//')
BUILD_NUMBER=$(echo "$SUFFIX" | grep -o '[0-9]*$')

echo "Preview label: $PREVIEW_LABEL"
echo "Build number:  $BUILD_NUMBER"
echo "Full suffix:   $SUFFIX"

# Export for use by caller
export PREVIEW_LABEL
export BUILD_NUMBER
export PREVIEW_SUFFIX="$SUFFIX"
