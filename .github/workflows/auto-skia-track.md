---
description: "Daily upstream Skia milestone tracking — merges new commits, resolves conflicts, builds, tests, and creates PRs."
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
jobs:
  pre-activation:
    outputs:
      current: ${{ steps.detect.outputs.current }}
      next: ${{ steps.detect.outputs.next }}
      latest: ${{ steps.detect.outputs.latest }}
      target: ${{ steps.detect.outputs.target }}
      mode: ${{ steps.detect.outputs.mode }}
if: needs.pre_activation.outputs.detect_result == 'success'
checkout:
  - fetch-depth: 0
    submodules: recursive
timeout-minutes: 120
concurrency:
  group: skia-upstream-sync-${{ github.event.inputs.mode || github.event.schedule || 'manual' }}
  cancel-in-progress: true
tools:
  github:
    toolsets: [repos, pull_requests]
    allowed-repos: ["mono/skia", "mono/skiasharp"]
    min-integrity: none
  bash: ["*"]
  edit:
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
permissions:
  contents: read
  pull-requests: read
steps:
  - name: Install Android workload
    run: dotnet workload install android --skip-sign-check
post-steps:
  - name: Push branches and create PRs
    env:
      GH_TOKEN: ${{ secrets.SKIASHARP_AUTOBUMP_TOKEN }}
    run: bash scripts/skia-sync-push-prs.sh
---

# Skia Upstream Sync

Current: m${{ needs.pre_activation.outputs.current }}. Target: m${{ needs.pre_activation.outputs.target }}. Branch: `skia-sync/m${{ needs.pre_activation.outputs.target }}`.

**Read `.agents/skills/update-skia/SKILL.md` and follow Phases 2–9.** Notes specific to this automated workflow:

- **Phase 1 is pre-computed** (above). Skip it.
- **Phase 4 branch name**: use `skia-sync/m${{ needs.pre_activation.outputs.target }}` (not `dev/update-skia-{TARGET}`).
  Before creating a fresh branch, check if `origin/skia-sync/m${{ needs.pre_activation.outputs.target }}` already exists.
  If so, check it out, check for new upstream commits with `git log HEAD..upstream/chrome/m${{ needs.pre_activation.outputs.target }}`, and merge if any. Stop if there are none.
  Even when current == target, there may be new upstream bug-fix commits — a matching milestone does NOT mean no work.
- **Build platform**: use Linux x64 for all native builds (`dotnet cake --target=externals-linux --arch=x64`).
- **Phase 8 reminder**: a green C# build is NOT sufficient — run the new-function diff check from Phase 8 Step 1.
- **Phase 10 is handled by a post-step.** Do NOT push branches or create PRs yourself — both are handled by the post-step. Just commit locally. After Phase 9, write these files:

1. `/tmp/gh-aw/agent/skia-sync-env.sh`:
   ```bash
   TARGET=${{ needs.pre_activation.outputs.target }}
   CURRENT=${{ needs.pre_activation.outputs.current }}
   ```

2. `/tmp/gh-aw/agent/skia-sync-skia-summary.md` — for the mono/skia PR:
   - Upstream merge details, conflicts resolved, C API fixes, items needing human attention

3. `/tmp/gh-aw/agent/skia-sync-skiasharp-summary.md` — for the mono/SkiaSharp PR:
   - Breaking change analysis, version/binding updates, C# changes, build/test results, items needing human attention

Commit submodule changes inside `externals/skia` on `skia-sync/m${{ needs.pre_activation.outputs.target }}`.
Commit parent repo changes on `skia-sync/m${{ needs.pre_activation.outputs.target }}` in the parent.
