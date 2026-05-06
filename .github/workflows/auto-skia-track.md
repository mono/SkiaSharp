---
description: "Daily upstream Skia milestone tracking - merges new commits, resolves conflicts, builds, tests, and creates PRs."

# -- Triggers ----------------------------------------------------------
# Three daily crons: current (7 AM), next (12 PM), latest (5 PM UTC).
# Manual dispatch with mode selector.
on:
  schedule:
    - cron: "0 7 * * *"
    - cron: "0 12 * * *"
    - cron: "0 17 * * *"
  workflow_dispatch:
    inputs:
      mode:
        description: "Which milestone to track"
        required: false
        type: choice
        default: next
        options: [current, next, latest]

  # -- Pre-activation step -------------------------------------------
  # Runs BEFORE the agent job. Detects the target milestone.
  # Exit 1 = skip the entire workflow (upstream branch doesn't exist).
  # Outputs are available in the prompt via ${{ needs.pre_activation.outputs.* }}.
  steps:
    - name: Detect milestone
      id: detect
      env:
        INPUT_MODE: ${{ github.event.inputs.mode }}
        SCHEDULE: ${{ github.event.schedule }}
        GH_TOKEN: ${{ github.token }}
      run: |
        if [ -n "$INPUT_MODE" ]; then
          MODE="$INPUT_MODE"
        elif [ "$SCHEDULE" = "0 7 * * *" ]; then
          MODE=current
        elif [ "$SCHEDULE" = "0 17 * * *" ]; then
          MODE=latest
        else
          MODE=next
        fi
        echo "mode=$MODE" >> "$GITHUB_OUTPUT"

        CURRENT=$(gh api "repos/$GITHUB_REPOSITORY/contents/scripts/VERSIONS.txt?ref=$GITHUB_REF" \
          --jq '.content' | base64 -d | grep '^libSkiaSharp.*milestone' | awk '{print $NF}')
        echo "current=$CURRENT" >> "$GITHUB_OUTPUT"

        NEXT=$((CURRENT + 1))
        echo "next=$NEXT" >> "$GITHUB_OUTPUT"

        LATEST=$(git ls-remote --heads https://github.com/google/skia.git 'refs/heads/chrome/m*' \
          | sed -n 's|.*refs/heads/chrome/m\([0-9]*\)$|\1|p' \
          | sort -n | tail -1)
        echo "latest=$LATEST" >> "$GITHUB_OUTPUT"

        if [ "$MODE" = "latest" ]; then
          TARGET="$LATEST"
        elif [ "$MODE" = "current" ]; then
          TARGET="$CURRENT"
        else
          TARGET="$NEXT"
        fi
        echo "target=$TARGET" >> "$GITHUB_OUTPUT"

        if ! git ls-remote --exit-code https://github.com/google/skia.git "refs/heads/chrome/m${TARGET}" >/dev/null 2>&1; then
          echo "::notice::upstream/chrome/m${TARGET} does not exist yet"
          exit 1
        fi

        echo "Will process: m${TARGET} (mode=${MODE}, current=m${CURRENT}, latest=m${LATEST})"

# -- Pre-activation outputs ------------------------------------------
# Expose detect step outputs for use in the prompt and other jobs.
# Required: without this, ${{ needs.pre_activation.outputs.target }} is empty.
jobs:
  pre-activation:
    outputs:
      current: ${{ steps.detect.outputs.current }}
      next: ${{ steps.detect.outputs.next }}
      latest: ${{ steps.detect.outputs.latest }}
      target: ${{ steps.detect.outputs.target }}
      mode: ${{ steps.detect.outputs.mode }}

# -- Agent job gate --------------------------------------------------
# Only run the agent if pre-activation succeeded (milestone detected).
if: needs.pre_activation.outputs.detect_result == 'success'

# -- Checkout --------------------------------------------------------
checkout:
  - fetch-depth: 0
    submodules: recursive
timeout-minutes: 120
concurrency:
  group: skia-upstream-sync-${{ github.event.inputs.mode || github.event.schedule || 'manual' }}
  cancel-in-progress: true

# -- Agent tools -----------------------------------------------------
tools:
  github:
    toolsets: [repos, pull_requests]
    allowed-repos: ["mono/skia", "mono/skiasharp"]
    min-integrity: none
  bash: ["*"]
  edit:

# -- Network allowlist -----------------------------------------------
# Skia build fetches deps from *.googlesource.com and GN from storage/cipd.
network:
  allowed:
    - defaults
    - github
    - dotnet
    - "chromium.googlesource.com"
    - "skia.googlesource.com"
    - "android.googlesource.com"
    - "dawn.googlesource.com"
    - "swiftshader.googlesource.com"
    - "chrome-infra-packages.appspot.com"
    - "gn.googlesource.com"
    - "storage.googleapis.com"

# -- Environment -----------------------------------------------------
# Clang is required for the Linux native build (retpoline flag).
env:
  CC: clang
  CXX: clang++
permissions:
  contents: read
  pull-requests: read

# -- Pre-agent steps -------------------------------------------------
# Run in the agent job AFTER checkout, BEFORE the AI executes.
# Install native build deps and write the env file for the post-step.
steps:
  - name: Install build dependencies
    run: |
      sudo apt-get update -qq
      sudo apt-get install -y clang libfontconfig1-dev ninja-build
      dotnet workload install android --skip-sign-check
      dotnet tool restore
    env:
      DEBIAN_FRONTEND: noninteractive
  - name: Write env for post-step
    run: |
      mkdir -p /tmp/gh-aw/agent
      echo "TARGET=${{ needs.pre_activation.outputs.target }}" > /tmp/gh-aw/agent/skia-sync-env.sh
      echo "CURRENT=${{ needs.pre_activation.outputs.current }}" >> /tmp/gh-aw/agent/skia-sync-env.sh

# -- Post-agent steps -----------------------------------------------
# Run AFTER the AI finishes. Pushes branches and creates/updates PRs
# using the SKIASHARP_AUTOBUMP_TOKEN (has write access to mono/skia).
post-steps:
  - name: Push branches and create PRs
    env:
      GH_TOKEN: ${{ secrets.SKIASHARP_AUTOBUMP_TOKEN }}
    run: bash scripts/skia-sync-push-prs.sh
---

# Skia Upstream Sync

Current: m${{ needs.pre_activation.outputs.current }}.
Target: m${{ needs.pre_activation.outputs.target }}.
Branch: `skia-sync/m${{ needs.pre_activation.outputs.target }}`.

**Read `.agents/skills/update-skia/SKILL.md` and follow Phases 2-9.** Notes specific to this automated workflow:

- **Phase 1 is pre-computed** (above). Skip it.
- **Phase 4 branch name**: use `skia-sync/m${{ needs.pre_activation.outputs.target }}` (not `dev/update-skia-{TARGET}`).
  Before creating a fresh branch, check if `origin/skia-sync/m${{ needs.pre_activation.outputs.target }}` already exists.
  If so, check it out, check for new upstream commits with `git log HEAD..upstream/chrome/m${{ needs.pre_activation.outputs.target }}`,
  and merge if any. Stop if there are none.
  Even when current == target, there may be new upstream bug-fix commits - a matching milestone does NOT mean no work.
- **Build platform**: use Linux x64 (`dotnet cake --target=externals-linux --arch=x64`). Clang is pre-configured via env vars.
- **Phase 8 reminder**: a green C# build is NOT sufficient - run the new-function diff check from Phase 8 Step 1.
- **Phase 10 is handled by a post-step.** Do NOT push branches or create PRs yourself - both are handled by the post-step.
  Just commit locally. After Phase 9, write these summary files:

1. `/tmp/gh-aw/agent/skia-sync-skia-summary.md` - for the mono/skia PR:
   - Upstream merge details, conflicts resolved, C API fixes, items needing human attention

2. `/tmp/gh-aw/agent/skia-sync-skiasharp-summary.md` - for the mono/SkiaSharp PR:
   - Breaking change analysis, version/binding updates, C# changes, build/test results, items needing human attention

Commit submodule changes inside `externals/skia` on `skia-sync/m${{ needs.pre_activation.outputs.target }}`.
Commit parent repo changes on `skia-sync/m${{ needs.pre_activation.outputs.target }}` in the parent.
