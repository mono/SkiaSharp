#!/bin/bash
# pipeline-status.sh — Trace the SkiaSharp release pipeline chain for a given ref.
#
# Usage:
#   .agents/skills/release-branch/scripts/pipeline-status.sh <branch-or-sha>
#
# Examples:
#   .agents/skills/release-branch/scripts/pipeline-status.sh release/3.119.4
#   .agents/skills/release-branch/scripts/pipeline-status.sh f568ac94dd7
#
# Outputs a structured summary of all three pipelines in the chain:
#   SkiaSharp-Native (26493) → SkiaSharp (10789) → SkiaSharp-Tests (15756)
#
# Requires: az CLI with devops extension, authenticated to devdiv.visualstudio.com

set -euo pipefail

ORG="https://devdiv.visualstudio.com"
PROJECT="DevDiv"

NATIVE_ID=26493
MANAGED_ID=10789
TESTS_ID=15756

REF="${1:-}"

if [ -z "$REF" ]; then
  echo "Usage: $0 <branch-or-sha>"
  echo ""
  echo "  branch-or-sha: A release branch name (e.g., release/3.119.4) or a git SHA"
  exit 1
fi

# Resolve: if it looks like a SHA (hex, 7-40 chars), find the branch from git
BRANCH=""
if echo "$REF" | grep -qE '^[0-9a-f]{7,40}$'; then
  # It's a SHA — find which release branch contains it
  BRANCH=$(git branch -r --contains "$REF" 2>/dev/null | grep -oE 'origin/release/[^ ]+' | head -1 | sed 's|origin/||')
  if [ -z "$BRANCH" ]; then
    echo "ERROR: Could not find a release branch containing SHA $REF"
    exit 1
  fi
  echo "Resolved SHA $REF → branch: $BRANCH"
else
  BRANCH="$REF"
fi

echo ""
echo "═══════════════════════════════════════════════════════════════"
echo " Pipeline Chain Status: $BRANCH"
echo "═══════════════════════════════════════════════════════════════"
echo ""

# Query function: get the most recent runs for a pipeline on this branch
query_pipeline() {
  local pipeline_id=$1
  local pipeline_name=$2

  az pipelines runs list \
    --pipeline-ids "$pipeline_id" \
    --branch "$BRANCH" \
    --org "$ORG" \
    --project "$PROJECT" \
    --query "[].{id:id, status:status, result:result, buildNumber:buildNumber, sourceVersion:sourceVersion}" \
    --top 5 \
    -o json 2>/dev/null
}

# Get trigger info for a specific build
get_trigger_info() {
  local build_id=$1
  az pipelines runs show \
    --id "$build_id" \
    --org "$ORG" \
    --project "$PROJECT" \
    --query "triggerInfo" \
    -o json 2>/dev/null
}

# Format a pipeline run as a table row
format_run() {
  local name=$1
  local id=$2
  local status=$3
  local result=$4
  local build_number=$5

  local icon="⏳"
  if [ "$status" = "completed" ]; then
    case "$result" in
      succeeded) icon="✅" ;;
      partiallySucceeded) icon="⚠️" ;;
      failed) icon="❌" ;;
      canceled) icon="🚫" ;;
      *) icon="❓" ;;
    esac
  elif [ "$status" = "inProgress" ]; then
    icon="🔄"
  elif [ "$status" = "notStarted" ]; then
    icon="⏳"
  fi

  printf " %s %-20s  id=%-10s  %-12s  %-20s  %s\n" "$icon" "$name" "$id" "$status" "${result:-pending}" "$build_number"
}

# --- SkiaSharp-Native ---
echo "┌─ SkiaSharp-Native (ID $NATIVE_ID)"
NATIVE_RUNS=$(query_pipeline $NATIVE_ID "SkiaSharp-Native")
NATIVE_COUNT=$(echo "$NATIVE_RUNS" | python3 -c "import json,sys; print(len(json.load(sys.stdin)))" 2>/dev/null || echo "0")

if [ "$NATIVE_COUNT" = "0" ]; then
  echo "│  No runs found on branch $BRANCH"
  NATIVE_BUILD_ID=""
else
  echo "$NATIVE_RUNS" | python3 -c "
import json, sys
runs = json.load(sys.stdin)
for r in runs:
    status = r['status']
    result = r.get('result') or 'pending'
    icon = {'completed': {'succeeded':'✅','partiallySucceeded':'⚠️','failed':'❌','canceled':'🚫'}.get(result,'❓'), 'inProgress':'🔄', 'notStarted':'⏳'}.get(status, '⏳')
    print(f\"│  {icon} id={r['id']}  {status:<12}  {result:<20}  {r['buildNumber']}\")
"
  # Pick the most recent successful (or partially succeeded) native build
  NATIVE_BUILD_ID=$(echo "$NATIVE_RUNS" | python3 -c "
import json, sys
runs = json.load(sys.stdin)
for r in runs:
    if r['status'] == 'completed' and r.get('result') in ('succeeded', 'partiallySucceeded'):
        print(r['id']); break
    elif r['status'] == 'inProgress':
        print(r['id']); break
else:
    if runs: print(runs[0]['id'])
" 2>/dev/null)
fi
echo "│"

# --- SkiaSharp (Managed) ---
echo "├─ SkiaSharp (ID $MANAGED_ID) — managed build, signing & publishing"
MANAGED_RUNS=$(query_pipeline $MANAGED_ID "SkiaSharp")
MANAGED_COUNT=$(echo "$MANAGED_RUNS" | python3 -c "import json,sys; print(len(json.load(sys.stdin)))" 2>/dev/null || echo "0")

MANAGED_BUILD_ID=""
if [ "$MANAGED_COUNT" = "0" ]; then
  echo "│  No runs found — not yet triggered"
else
  echo "$MANAGED_RUNS" | python3 -c "
import json, sys
runs = json.load(sys.stdin)
for r in runs:
    status = r['status']
    result = r.get('result') or 'pending'
    icon = {'completed': {'succeeded':'✅','partiallySucceeded':'⚠️','failed':'❌','canceled':'🚫'}.get(result,'❓'), 'inProgress':'🔄', 'notStarted':'⏳'}.get(status, '⏳')
    print(f\"│  {icon} id={r['id']}  {status:<12}  {result:<20}  {r['buildNumber']}\")
"
  MANAGED_BUILD_ID=$(echo "$MANAGED_RUNS" | python3 -c "
import json, sys
runs = json.load(sys.stdin)
if runs: print(runs[0]['id'])
" 2>/dev/null)

  # Show trigger info for the first managed build
  if [ -n "$MANAGED_BUILD_ID" ]; then
    TRIGGER=$(get_trigger_info "$MANAGED_BUILD_ID")
    TRIGGER_SOURCE=$(echo "$TRIGGER" | python3 -c "import json,sys; d=json.load(sys.stdin); print(f\"triggered by {d.get('source','?')} build {d.get('pipelineId','?')}\")" 2>/dev/null || echo "")
    if [ -n "$TRIGGER_SOURCE" ]; then
      echo "│  ↑ $TRIGGER_SOURCE"
    fi
  fi
fi
echo "│"

# --- SkiaSharp-Tests ---
echo "└─ SkiaSharp-Tests (ID $TESTS_ID) — device & unit tests"
TESTS_RUNS=$(query_pipeline $TESTS_ID "SkiaSharp-Tests")
TESTS_COUNT=$(echo "$TESTS_RUNS" | python3 -c "import json,sys; print(len(json.load(sys.stdin)))" 2>/dev/null || echo "0")
TESTS_BUILD_ID=""

if [ "$TESTS_COUNT" = "0" ]; then
  echo "   No runs found — not yet triggered"
else
  echo "$TESTS_RUNS" | python3 -c "
import json, sys
runs = json.load(sys.stdin)
for r in runs:
    status = r['status']
    result = r.get('result') or 'pending'
    icon = {'completed': {'succeeded':'✅','partiallySucceeded':'⚠️','failed':'❌','canceled':'🚫'}.get(result,'❓'), 'inProgress':'🔄', 'notStarted':'⏳'}.get(status, '⏳')
    print(f\"   {icon} id={r['id']}  {status:<12}  {result:<20}  {r['buildNumber']}\")
"
  TESTS_BUILD_ID=$(echo "$TESTS_RUNS" | python3 -c "
import json, sys
runs = json.load(sys.stdin)
if runs: print(runs[0]['id'])
" 2>/dev/null)

  if [ -n "$TESTS_BUILD_ID" ]; then
    TRIGGER=$(get_trigger_info "$TESTS_BUILD_ID")
    TRIGGER_SOURCE=$(echo "$TRIGGER" | python3 -c "import json,sys; d=json.load(sys.stdin); print(f\"triggered by {d.get('source','?')} build {d.get('pipelineId','?')}\")" 2>/dev/null || echo "")
    if [ -n "$TRIGGER_SOURCE" ]; then
      echo "   ↑ $TRIGGER_SOURCE"
    fi
  fi
fi

echo ""
echo "═══════════════════════════════════════════════════════════════"

# Summary
echo ""
echo "Summary:"
if [ "$NATIVE_COUNT" != "0" ] && [ "$MANAGED_COUNT" != "0" ] && [ "$TESTS_COUNT" != "0" ]; then
  # Check if all passed
  ALL_DONE=$(echo "$NATIVE_RUNS" "$MANAGED_RUNS" "$TESTS_RUNS" | python3 -c "
import json, sys
content = sys.stdin.read()
# Parse the three JSON arrays
import re
arrays = re.findall(r'\[.*?\]', content, re.DOTALL)
all_good = True
for arr_str in arrays:
    runs = json.loads(arr_str)
    if not runs:
        all_good = False; break
    latest = runs[0]
    if latest['status'] != 'completed' or latest.get('result') not in ('succeeded', 'partiallySucceeded'):
        all_good = False; break
print('yes' if all_good else 'no')
" 2>/dev/null || echo "no")

  if [ "$ALL_DONE" = "yes" ]; then
    echo "  ✅ All pipelines completed successfully. Packages should be on the internal feed."
  else
    echo "  🔄 Pipeline chain still in progress or has failures. Check individual statuses above."
  fi
elif [ "$NATIVE_COUNT" != "0" ] && [ "$MANAGED_COUNT" = "0" ]; then
  echo "  ⏳ Waiting for SkiaSharp (managed) to be triggered by SkiaSharp-Native."
elif [ "$NATIVE_COUNT" = "0" ]; then
  echo "  ⏳ No native build found yet. Pipeline may not have been triggered."
else
  echo "  🔄 Pipeline chain in progress."
fi

echo ""
echo "ADO Links:"
[ -n "${NATIVE_BUILD_ID:-}" ] && echo "  Native:  https://devdiv.visualstudio.com/DevDiv/_build/results?buildId=$NATIVE_BUILD_ID"
[ -n "${MANAGED_BUILD_ID:-}" ] && echo "  Managed: https://devdiv.visualstudio.com/DevDiv/_build/results?buildId=$MANAGED_BUILD_ID"
[ -n "${TESTS_BUILD_ID:-}" ] && echo "  Tests:   https://devdiv.visualstudio.com/DevDiv/_build/results?buildId=$TESTS_BUILD_ID"

exit 0
