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

# -- Pre-agent steps (host) ------------------------------------------
# Run in the agent job AFTER checkout, BEFORE the container starts.
# Only host-level setup that doesn't need to be visible inside the container.
steps:
  - name: Set up agent output directory
    run: |
      mkdir -p /tmp/gh-aw/agent
  - name: Align submodule to origin/main
    run: |
      # The checkout uses the workflow branch, so the submodule may be at a
      # different SHA than what origin/main expects. Fix it before the agent runs.
      # Note: the submodule tracks the `skiasharp` branch in mono/skia, so this
      # SHA should be a commit on origin/skiasharp.
      MAIN_SUB_SHA=$(git ls-tree origin/main -- externals/skia | awk '{print $3}')
      echo "origin/main submodule SHA: $MAIN_SUB_SHA"
      git -C externals/skia fetch origin skiasharp 2>&1
      git -C externals/skia checkout "$MAIN_SUB_SHA" 2>&1
      echo "Verifying SHA is on skiasharp branch:"
      git -C externals/skia branch -r --contains "$MAIN_SUB_SHA" | grep -q 'origin/skiasharp' \
        && echo "  ✅ SHA is on origin/skiasharp" \
        || echo "  ⚠️ SHA is NOT on origin/skiasharp — submodule pointer may be stale"
  - name: Copy push script for post-step
    run: |
      cp scripts/skia-sync-push-prs.sh /tmp/gh-aw/skia-sync-push-prs.sh

# -- Pre-agent steps (container) -------------------------------------
# Run INSIDE the agent container, immediately before AI execution.
# All packages, fonts, and tools must be installed here to be visible to the agent.
pre-agent-steps:
  - name: Install native build dependencies
    run: |
      sudo apt-get update -qq
      sudo apt-get install -y clang fontconfig libfontconfig1-dev ninja-build fonts-dejavu-core ttf-ancient-fonts
      fc-cache -f
      fc-list | grep -i "dejavu\|symbola" | head -5 || echo "⚠️ No DejaVu/Symbola fonts found"
    env:
      DEBIAN_FRONTEND: noninteractive
  - name: Install dotnet workload and restore tools
    run: |
      dotnet workload install android --skip-sign-check
      dotnet tool restore

# -- Post-agent steps -----------------------------------------------
# Run AFTER the AI finishes. Pushes branches and creates/updates PRs
# using the SKIASHARP_AUTOBUMP_TOKEN (has write access to mono/skia).
post-steps:
  - name: Push branches and create PRs
    env:
      GH_TOKEN: ${{ secrets.SKIASHARP_AUTOBUMP_TOKEN }}
    run: bash /tmp/gh-aw/skia-sync-push-prs.sh
---

# Skia Upstream Sync

Current: m${{ needs.pre_activation.outputs.current }}.
Target: m${{ needs.pre_activation.outputs.target }}.
Branch: `skia-sync/m${{ needs.pre_activation.outputs.target }}`.

**Read `.agents/skills/update-skia/SKILL.md` and follow Phases 2-9.** Notes specific to this automated workflow:

- **Before anything else**, run a font diagnostic and save to artifacts:
  ```bash
  fc-list | head -20 > /tmp/gh-aw/agent/fc-list.txt 2>&1
  fc-list | wc -l >> /tmp/gh-aw/agent/fc-list.txt
  echo "FONTCONFIG_PATH=$FONTCONFIG_PATH" >> /tmp/gh-aw/agent/fc-list.txt
  echo "XDG_DATA_HOME=$XDG_DATA_HOME" >> /tmp/gh-aw/agent/fc-list.txt
  ls /usr/share/fonts/ >> /tmp/gh-aw/agent/fc-list.txt 2>&1
  ```
- **Phase 1 is pre-computed** (above). Skip it.
- **Phase 4 branch name**: use `skia-sync/m${{ needs.pre_activation.outputs.target }}` (not `dev/update-skia-{TARGET}`).
  Create the parent repo branch from `origin/main`: `git checkout -b skia-sync/m{N} origin/main`.
  Before creating a fresh branch, check if `origin/skia-sync/m${{ needs.pre_activation.outputs.target }}` already exists.
  If so, check it out, check for new upstream commits with `git log HEAD..upstream/chrome/m${{ needs.pre_activation.outputs.target }}`,
  and merge if any. Stop if there are none.
  Even when current == target, there may be new upstream bug-fix commits - a matching milestone does NOT mean no work.
- **Build platform**: use Linux x64 (`dotnet cake --target=externals-linux --arch=x64`). Clang is pre-configured via env vars.
- **NEVER run `externals-download`** in this workflow — not even for debugging or baseline comparison. Build from source only.
- **Submodule alignment**: the pre-agent step already checked out the submodule to the correct SHA.
  When creating your submodule feature branch, branch from the current HEAD.
- **Phase 8 reminder**: a green C# build is NOT sufficient - run the new-function diff check from Phase 8 Step 1.
- **Phase 10 is handled by a post-step.** Do NOT push branches, create PRs, or create issues yourself — all GitHub artifacts are handled by the post-step.
  Just commit locally. Do NOT call `create_issue` or `create_pull_request`. After Phase 9, write these files:

1. `/tmp/gh-aw/agent/skia-sync-env.sh` — **required** for the post-step to know what to push:
   ```bash
   mkdir -p /tmp/gh-aw/agent
   cat > /tmp/gh-aw/agent/skia-sync-env.sh << EOF
   TARGET=${{ needs.pre_activation.outputs.target }}
   CURRENT=${{ needs.pre_activation.outputs.current }}
   EOF
   ```

2. `/tmp/gh-aw/agent/skia-sync-skia-summary.md` - for the mono/skia PR:
   - Upstream merge details, conflicts resolved, C API fixes, items needing human attention

3. `/tmp/gh-aw/agent/skia-sync-skiasharp-summary.md` - for the mono/SkiaSharp PR:
   - Breaking change analysis, version/binding updates, C# changes, build/test results, items needing human attention

All files written to `/tmp/gh-aw/agent/` are automatically uploaded as workflow artifacts.
Write test output there too (`/tmp/gh-aw/agent/test-output.txt`) so failures can be inspected after the run.

Commit submodule changes inside `externals/skia` on `skia-sync/m${{ needs.pre_activation.outputs.target }}`.
Commit parent repo changes on `skia-sync/m${{ needs.pre_activation.outputs.target }}` in the parent.
