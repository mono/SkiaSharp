---
description: "Daily upstream Skia milestone sync - merges new commits, resolves conflicts, builds, tests, and creates PRs."

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
      milestone:
        description: "Exact milestone number (e.g. 148) — overrides mode if set"
        required: false
        type: string

  # -- Pre-activation step -------------------------------------------
  # Runs BEFORE the agent job. Detects the target milestone.
  # Exit 1 = skip the entire workflow (upstream branch doesn't exist).
  # Outputs are available in the prompt via ${{ needs.pre_activation.outputs.* }}.
  steps:
    - name: Detect milestone
      id: detect
      env:
        INPUT_MODE: ${{ github.event.inputs.mode }}
        INPUT_MILESTONE: ${{ github.event.inputs.milestone }}
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

        # Explicit milestone input overrides mode
        if [ -n "$INPUT_MILESTONE" ]; then
          TARGET="$INPUT_MILESTONE"
          MODE="explicit"
        elif [ "$MODE" = "latest" ]; then
          TARGET="$LATEST"
        elif [ "$MODE" = "current" ]; then
          TARGET="$CURRENT"
        else
          TARGET="$NEXT"
        fi
        echo "target=$TARGET" >> "$GITHUB_OUTPUT"

        UPSTREAM_SHA=$(git ls-remote https://github.com/google/skia.git "refs/heads/chrome/m${TARGET}" | awk '{print $1}')
        if [ -z "$UPSTREAM_SHA" ]; then
          echo "::notice::upstream/chrome/m${TARGET} does not exist yet"
          exit 1
        fi

        # Check if the sync branch already contains all upstream commits.
        # This avoids spinning up the expensive agent job when there's nothing new.
        SYNC_SHA=$(git ls-remote https://github.com/mono/skia.git "refs/heads/skia-sync/m${TARGET}" | awk '{print $1}')
        if [ -n "$SYNC_SHA" ]; then
          BEHIND=$(gh api "repos/mono/skia/compare/${UPSTREAM_SHA}...skia-sync/m${TARGET}" \
            --jq '.behind_by' 2>/dev/null || echo "unknown")
          if [ "$BEHIND" = "0" ]; then
            echo "::notice::chrome/m${TARGET} already fully merged into skia-sync/m${TARGET} (upstream HEAD: ${UPSTREAM_SHA:0:12}) — skipping"
            exit 1
          fi
          echo "Sync branch exists but is ${BEHIND} commits behind upstream"
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

# -- Safe outputs ------------------------------------------------------
# All GitHub writes (push, PR) are done in post-steps via bash.
# Stage all safe outputs so the agent can't create issues/PRs directly.
safe-outputs:
  staged: true

# -- Sandbox -----------------------------------------------------------
# Mount host /etc/fonts into the AWF chroot so Skia's fontconfig can find fonts.
# Without this, /etc/fonts/fonts.conf doesn't exist in the chroot and
# SKTypeface.Default is empty (46 test failures).
sandbox:
  agent:
    mounts:
      - "/etc/fonts:/etc/fonts:ro"

# -- Pre-agent steps (host) ------------------------------------------
# Both steps: and pre-agent-steps: run on the HOST, not inside the AWF container.
# The agent runs in an AWF chroot. sandbox.agent.mounts handles /etc/fonts.
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
      cp .github/scripts/skia-sync-push-prs.sh /tmp/gh-aw/skia-sync-push-prs.sh

# -- Pre-agent steps ---------------------------------------------------
# Run on the host before the agent starts. Packages installed here are visible
# in the AWF chroot (shared host filesystem), but dotnet tool restore does NOT
# carry into the chroot — the agent must run it itself.
pre-agent-steps:
  - name: Install native build dependencies
    run: |
      sudo apt-get update -qq
      sudo apt-get install -y clang fontconfig libfontconfig1-dev ninja-build fonts-dejavu-core ttf-ancient-fonts
      fc-cache -f
      dotnet workload install android --skip-sign-check
    env:
      DEBIAN_FRONTEND: noninteractive

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

Current: `m${{ needs.pre_activation.outputs.current }}`.  
Target: `m${{ needs.pre_activation.outputs.target }}`.  
Branch: `skia-sync/m${{ needs.pre_activation.outputs.target }}`.  

**Read `.agents/skills/update-skia/SKILL.md` and follow Phases 2-10.** Notes specific to this automated workflow:

- **Phase 1 is pre-computed** (above). Skip it — but you still need to add the `upstream` remote
  and fetch `chrome/m${{ needs.pre_activation.outputs.target }}` (Phase 1 step 4) since Phase 5 depends on it.
- **First thing**: run `dotnet tool restore` (pre-agent-steps can't do this for the chroot).
- **Phase 4**: Before creating a fresh branch, check if `origin/skia-sync/m${{ needs.pre_activation.outputs.target }}` already exists.
  If so, check it out — the pre-activation step already verified new upstream commits exist.
  Even when current == target, there may be new upstream bug-fix commits — a matching milestone does NOT mean no work.
  **Skip Phase 4 step 5** (submodule SHA alignment) — the pre-agent step already aligned it.
  Branch from the current HEAD when creating the submodule feature branch in step 6.
- **Build platform**: use Linux x64 (`dotnet cake --target=externals-linux --arch=x64`). Clang is pre-configured via env vars.
  This also applies to Phase 10 if a native rebuild is needed.
- **NEVER run `externals-download`** in this workflow — not even for debugging or baseline comparison. Build from source only.
- **Phase 9 reminder**: a green C# build is NOT sufficient - run the new-function diff check from Phase 9 step 1.
- **Phase 11 — do NOT execute it.** Replace it entirely with the file writes below.
  Do NOT push branches, create PRs, or create issues — all GitHub artifacts are handled by the post-step.
  Just commit locally. Do NOT call `create_issue` or `create_pull_request`.
- **"No work" signal**: the pre-activation step skips the workflow when there are no new upstream commits,
  so this should not happen. If somehow it does, do NOT write `skia-sync-env.sh` and stop.

After Phase 10, write these files:

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
