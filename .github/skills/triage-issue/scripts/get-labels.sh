#!/usr/bin/env bash
# Fetch GitHub labels for a SkiaSharp repo, optionally filtered by prefix.
#
# Usage:
#   get-labels.sh                      # all labels
#   get-labels.sh area/                # labels starting with "area/"
#   get-labels.sh type/                # labels starting with "type/"
#   get-labels.sh --repo mono/SkiaSharp.Extended backend/
#   get-labels.sh --no-cache area/     # bypass cache
#
# Caches results for 10 minutes in a temp file.

set -euo pipefail

REPO="mono/SkiaSharp"
PREFIX=""
USE_CACHE=true

while [[ $# -gt 0 ]]; do
  case "$1" in
    --repo) REPO="$2"; shift 2 ;;
    --no-cache) USE_CACHE=false; shift ;;
    *) PREFIX="$1"; shift ;;
  esac
done

# Simple file cache (10 min TTL)
CACHE_DIR="${TMPDIR:-/tmp}/skiasharp-labels"
mkdir -p "$CACHE_DIR"
CACHE_KEY=$(echo "$REPO" | tr '/' '-')
CACHE_FILE="$CACHE_DIR/$CACHE_KEY.json"

# Check cache age using portable find (works on macOS and Linux)
if [[ "$USE_CACHE" == true ]] && [[ -f "$CACHE_FILE" ]]; then
  STALE=$(find "$CACHE_FILE" -mmin +10 2>/dev/null || true)
  if [[ -n "$STALE" ]]; then
    rm -f "$CACHE_FILE"
  fi
elif [[ "$USE_CACHE" == false ]]; then
  rm -f "$CACHE_FILE"
fi

if [[ ! -f "$CACHE_FILE" ]]; then
  TEMP_FILE="$CACHE_FILE.tmp.$$"
  if ! gh label list --repo "$REPO" --limit 500 --json name,description,color > "$TEMP_FILE"; then
    echo "❌ Failed to fetch labels from GitHub (check auth/network)" >&2
    rm -f "$TEMP_FILE"
    exit 1
  fi
  mv "$TEMP_FILE" "$CACHE_FILE"
fi

LABELS=$(cat "$CACHE_FILE")

if [[ -n "$PREFIX" ]]; then
  echo "$LABELS" | LABEL_PREFIX="$PREFIX" python3 -c "
import json, sys, os
prefix = os.environ['LABEL_PREFIX']
labels = json.load(sys.stdin)
filtered = [l for l in labels if l['name'].startswith(prefix)]
for l in sorted(filtered, key=lambda x: x['name']):
    suffix = l['name'][len(prefix):]
    desc = f\"  — {l['description']}\" if l.get('description') else ''
    print(f\"{suffix}{desc}\")
print(f\"\n{len(filtered)} labels with prefix '{prefix}'\")
"
else
  echo "$LABELS" | python3 -c "
import json, sys
from collections import defaultdict
labels = json.load(sys.stdin)
groups = defaultdict(list)
for l in labels:
    name = l['name']
    if '/' in name:
        prefix = name[:name.index('/') + 1]
        groups[prefix].append(name[len(prefix):])
    else:
        groups['(no prefix)'].append(name)
for prefix in sorted(groups.keys()):
    values = sorted(groups[prefix])
    print(f\"{prefix} ({len(values)} labels)\")
    for v in values:
        print(f\"  {v}\")
    print()
print(f\"{len(labels)} total labels\")
"
fi
