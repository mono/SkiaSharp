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
if: needs.pre_activation.outputs.detect_result == 'success'
checkout:
  - fetch-depth: 0
    submodules: recursive
timeout-minutes: 120
concurrency:
  group: auto-skia-track-${{ github.event.inputs.mode || github.event.schedule || 'manual' }}
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
    - github
    - chromium.googlesource.com
permissions:
  contents: read
  pull-requests: read
safe-outputs:
  create-pull-request:
    if-no-changes: ignore
jobs:
  pre-activation:
    outputs:
      current: ${{ steps.detect.outputs.current }}
      next: ${{ steps.detect.outputs.next }}
      latest: ${{ steps.detect.outputs.latest }}
      target: ${{ steps.detect.outputs.target }}
      mode: ${{ steps.detect.outputs.mode }}
env:
  AUTOBUMP_TARGET: ${{ needs.pre_activation.outputs.target }}
  AUTOBUMP_CURRENT: ${{ needs.pre_activation.outputs.current }}
post-steps:
  - name: Push branches and create PRs
    env:
      GH_TOKEN: ${{ secrets.SKIASHARP_AUTOBUMP_TOKEN }}
    run: |
      set -euo pipefail
      TARGET="${AUTOBUMP_TARGET}"
      CURRENT="${AUTOBUMP_CURRENT}"

      if [ -z "$TARGET" ]; then
        echo "TARGET is empty — skipping push"
        exit 0
      fi

      BRANCH="autobump/skia-m${TARGET}"

      # Read agent summary
      SUMMARY=""
      if [ -f /tmp/gh-aw/agent/autobump-summary.md ]; then
        SUMMARY=$(cat /tmp/gh-aw/agent/autobump-summary.md)
      fi

      # --- Push submodule branch to mono/skia ---
      cd externals/skia
      if git rev-parse --verify "$BRANCH" &>/dev/null; then
        echo "Pushing $BRANCH to mono/skia..."
        git remote set-url origin "https://x-access-token:${GH_TOKEN}@github.com/mono/skia.git"
        git push origin "$BRANCH" --force-with-lease 2>/dev/null || git push origin "$BRANCH" --force

        SKIA_PR=$(gh pr list --repo mono/skia --head "$BRANCH" --json number --jq '.[0].number' 2>/dev/null || echo "")
        if [ -z "$SKIA_PR" ]; then
          echo "Creating mono/skia PR..."
          gh pr create --repo mono/skia \
            --head "$BRANCH" --base skiasharp \
            --title "[autobump] Update skia to milestone ${TARGET}" \
            --draft \
            --body "Automated upstream merge of chrome/m${TARGET}.

      ${SUMMARY}

      Created by auto-skia-track workflow." 2>/dev/null || true
        else
          echo "Updating mono/skia PR #${SKIA_PR}..."
          gh pr edit "$SKIA_PR" --repo mono/skia \
            --body "Automated upstream merge of chrome/m${TARGET}.

      ${SUMMARY}

      Updated: $(date -u +%Y-%m-%dT%H:%M:%SZ)" 2>/dev/null || true
        fi
      else
        echo "No submodule branch $BRANCH — skipping mono/skia push"
      fi

      # --- Push SkiaSharp branch ---
      cd "$GITHUB_WORKSPACE"
      git checkout -b "$BRANCH" 2>/dev/null || git checkout "$BRANCH" 2>/dev/null || true
      git add -A
      if git diff --cached --quiet; then
        echo "No SkiaSharp changes to push"
        exit 0
      fi
      git commit -m "Bump skia to milestone ${TARGET}

      Automated merge of upstream chrome/m${TARGET}."

      echo "Pushing $BRANCH to mono/SkiaSharp..."
      git remote set-url origin "https://x-access-token:${GH_TOKEN}@github.com/mono/SkiaSharp.git"
      git push origin "$BRANCH" --force-with-lease 2>/dev/null || git push origin "$BRANCH" --force

      SKIA_PR=$(gh pr list --repo mono/skia --head "$BRANCH" --json number --jq '.[0].number' 2>/dev/null || echo "")
      SKIA_PR_LINK=""
      [ -n "$SKIA_PR" ] && SKIA_PR_LINK="**Companion skia PR:** https://github.com/mono/skia/pull/$SKIA_PR"

      SS_PR=$(gh pr list --repo mono/SkiaSharp --head "$BRANCH" --json number --jq '.[0].number' 2>/dev/null || echo "")
      if [ -z "$SS_PR" ]; then
        echo "Creating mono/SkiaSharp PR..."
        gh pr create --repo mono/SkiaSharp \
          --head "$BRANCH" --base main \
          --title "[autobump] Bump skia to milestone ${TARGET}" \
          --draft \
          --body "Automated Skia milestone bump from m${CURRENT} to m${TARGET}.

      $SKIA_PR_LINK

      ${SUMMARY}

      Created by auto-skia-track workflow."
      else
        echo "Updating mono/SkiaSharp PR #${SS_PR}..."
        gh pr edit "$SS_PR" --repo mono/SkiaSharp \
          --body "Automated Skia milestone bump from m${CURRENT} to m${TARGET}.

      $SKIA_PR_LINK

      ${SUMMARY}

      Updated: $(date -u +%Y-%m-%dT%H:%M:%SZ)" 2>/dev/null || true
      fi
---

# Auto Skia Track

Merge new upstream commits from `chrome/m${{ needs.pre_activation.outputs.target }}` into
branch `autobump/skia-m${{ needs.pre_activation.outputs.target }}`.

The current SkiaSharp milestone is m${{ needs.pre_activation.outputs.current }}.
The target is m${{ needs.pre_activation.outputs.target }}.

**Important**: Even when current == target, there may be new upstream bug-fix commits on
`chrome/m${{ needs.pre_activation.outputs.target }}` that need merging. Always check for
new commits — a matching milestone number does NOT mean there's nothing to do.

Read and follow `.agents/skills/update-skia/SKILL.md` for detailed instructions on every phase.

## Step 1 — Breaking change analysis and validation

Follow **Phases 2–3** of the skill. Save the analysis — it goes into the PR description.

## Step 2 — Merge upstream in submodule

Follow **Phase 4** of the skill in `externals/skia`:

First, check if the branch already exists or needs creating:

```bash
cd externals/skia
git fetch origin --quiet
git fetch upstream --quiet
```

**If `autobump/skia-m${{ needs.pre_activation.outputs.target }}` already exists on origin:**
1. Check it out and check for new upstream commits:
   ```bash
   git checkout -b autobump/skia-m${{ needs.pre_activation.outputs.target }} origin/autobump/skia-m${{ needs.pre_activation.outputs.target }}
   git log --oneline HEAD..upstream/chrome/m${{ needs.pre_activation.outputs.target }} | head -5
   ```
2. If there are no new commits, the branch is up-to-date — report this and stop.
3. If there ARE new commits, merge them in:
   ```bash
   git merge --no-commit upstream/chrome/m${{ needs.pre_activation.outputs.target }}
   ```

**If the branch does NOT exist:**
1. Create it from `origin/skiasharp`:
   ```bash
   git checkout -b autobump/skia-m${{ needs.pre_activation.outputs.target }} origin/skiasharp
   git merge --no-commit upstream/chrome/m${{ needs.pre_activation.outputs.target }}
   ```

In either case, resolve any conflicts per the skill's strategy table (Phase 4, Step 3).
Verify: no conflict markers, C API files intact. Commit the merge.

## Step 3 — Fix C API shim layer

Follow **Phase 5** of the skill. Build on Linux x64:

```bash
dotnet cake --target=externals-linux --arch=x64
```

Fix errors iteratively. Commit fixes in the submodule:

```bash
cd externals/skia
git add -A && git commit -m "Adapt SkiaSharp shims for m${{ needs.pre_activation.outputs.target }}"
```

## Step 4 — Commit submodule changes

All submodule work stays local — the post-step handles pushing and PR creation.
Make sure all changes in `externals/skia` are committed on the `autobump/skia-m${{ needs.pre_activation.outputs.target }}` branch.

## Step 5 — Update SkiaSharp parent repo

Follow **Phases 6–9** of the skill:

1. `pwsh .agents/skills/update-skia/scripts/update-versions.ps1 -Current ${{ needs.pre_activation.outputs.current }} -Target ${{ needs.pre_activation.outputs.target }}`
2. `pwsh .agents/skills/update-skia/scripts/regenerate-bindings.ps1`
3. Fix C# wrappers per Phase 8
4. Build and test:
   ```bash
   dotnet build binding/SkiaSharp/SkiaSharp.csproj
   dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj
   ```

## Step 6 — Write summary

Write a markdown summary of everything you did to `/tmp/gh-aw/agent/autobump-summary.md`.
The post-step will include this in the PR descriptions.

Include:
- Breaking change analysis table from Step 1
- What conflicts were resolved and how
- What C API / C# fixes were applied
- Build and test results
- Any issues that need human attention
