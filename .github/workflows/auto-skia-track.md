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
        # Determine mode from input or schedule
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

        # Read current milestone from VERSIONS.txt via API (no checkout yet)
        CURRENT=$(gh api "repos/$GITHUB_REPOSITORY/contents/scripts/VERSIONS.txt?ref=$GITHUB_REF" \
          --jq '.content' | base64 -d | grep '^libSkiaSharp.*milestone' | awk '{print $NF}')
        echo "current=$CURRENT" >> "$GITHUB_OUTPUT"

        NEXT=$((CURRENT + 1))
        echo "next=$NEXT" >> "$GITHUB_OUTPUT"

        # Find latest upstream chrome/m* branch (no clone needed)
        LATEST=$(git ls-remote --heads https://github.com/google/skia.git 'refs/heads/chrome/m*' \
          | sed -n 's|.*refs/heads/chrome/m\([0-9]*\)$|\1|p' \
          | sort -n | tail -1)
        echo "latest=$LATEST" >> "$GITHUB_OUTPUT"

        # Pick target
        if [ "$MODE" = "latest" ]; then
          TARGET="$LATEST"
        elif [ "$MODE" = "current" ]; then
          TARGET="$CURRENT"
        else
          TARGET="$NEXT"
        fi
        echo "target=$TARGET" >> "$GITHUB_OUTPUT"

        # Verify upstream branch exists
        if ! git ls-remote --exit-code https://github.com/google/skia.git "refs/heads/chrome/m${TARGET}" >/dev/null 2>&1; then
          echo "::notice::upstream/chrome/m${TARGET} does not exist yet"
          exit 1
        fi

        # Note: we can't reliably check if the autobump branch is up-to-date
        # from pre-activation (no clone, merge commits != upstream tip).
        # The agent does the real check with git log HEAD..upstream after checkout.

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
  github-token: ${{ secrets.SKIASHARP_AUTOBUMP_TOKEN }}
  create-pull-request:
    title-prefix: "[autobump] "
    labels: [upstream-tracking]
    draft: true
    max: 2
    allowed-base-branches: [main, skiasharp]
    allowed-repos: ["mono/skia"]
    preserve-branch-name: true
    protected-files: allowed
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

## Step 4 — Push submodule and create mono/skia PR

Push the submodule branch to mono/skia:

```bash
cd externals/skia
git push origin "autobump/skia-m${{ needs.pre_activation.outputs.target }}" --force-with-lease 2>/dev/null || \
  git push -u origin "autobump/skia-m${{ needs.pre_activation.outputs.target }}"
```

Then create a draft PR in mono/skia via the `create_pull_request` safe-output tool:
- `repo`: `mono/skia`
- Branch: `autobump/skia-m${{ needs.pre_activation.outputs.target }}`
- Base: `skiasharp`
- Title: `Update skia to milestone ${{ needs.pre_activation.outputs.target }}`
- Body: Breaking change analysis from Step 1

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

## Step 6 — Commit for mono/SkiaSharp PR

Use `autobump/skia-m${{ needs.pre_activation.outputs.target }}` as the branch name. Commit:

```
Bump skia to milestone ${{ needs.pre_activation.outputs.target }}

Automated merge of upstream chrome/m${{ needs.pre_activation.outputs.target }}.
```

Create the mono/SkiaSharp PR via the `create_pull_request` safe-output tool:
- Branch: `autobump/skia-m${{ needs.pre_activation.outputs.target }}`
- Base: `main`
- Title: `Bump skia to milestone ${{ needs.pre_activation.outputs.target }}`
- Body: Breaking change analysis, link to the companion mono/skia PR from Step 4, build/test status
