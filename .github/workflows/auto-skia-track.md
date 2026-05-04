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
    - name: Detect milestone and ensure PRs exist
      id: detect
      env:
        INPUT_MODE: ${{ github.event.inputs.mode }}
        SCHEDULE: ${{ github.event.schedule }}
        GH_TOKEN: ${{ secrets.SKIASHARP_AUTOBUMP_TOKEN }}
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

        BRANCH="autobump/skia-m${TARGET}"

        # --- Ensure mono/skia branch + PR exist ---
        if ! git ls-remote --exit-code https://github.com/mono/skia.git "refs/heads/${BRANCH}" >/dev/null 2>&1; then
          echo "Creating ${BRANCH} in mono/skia from skiasharp..."
          SKIASHARP_SHA=$(git ls-remote https://github.com/mono/skia.git refs/heads/skiasharp | awk '{print $1}')
          gh api "repos/mono/skia/git/refs" -f ref="refs/heads/${BRANCH}" -f sha="$SKIASHARP_SHA" 2>/dev/null || true
        fi
        SKIA_PR=$(gh pr list --repo mono/skia --head "$BRANCH" --json number --jq '.[0].number' 2>/dev/null || echo "")
        if [ -z "$SKIA_PR" ]; then
          echo "Creating mono/skia draft PR..."
          SKIA_PR=$(gh pr create --repo mono/skia \
            --head "$BRANCH" --base skiasharp \
            --title "[autobump] Update skia to milestone ${TARGET}" \
            --draft \
            --body "Automated upstream merge of chrome/m${TARGET}. Pending agent run." 2>/dev/null | grep -oE '[0-9]+$' || echo "")
        fi
        echo "skia_pr=$SKIA_PR" >> "$GITHUB_OUTPUT"

        # --- Ensure mono/SkiaSharp branch + PR exist ---
        if ! git ls-remote --exit-code https://github.com/mono/SkiaSharp.git "refs/heads/${BRANCH}" >/dev/null 2>&1; then
          echo "Creating ${BRANCH} in mono/SkiaSharp from main..."
          MAIN_SHA=$(git ls-remote https://github.com/mono/SkiaSharp.git refs/heads/main | awk '{print $1}')
          gh api "repos/mono/SkiaSharp/git/refs" -f ref="refs/heads/${BRANCH}" -f sha="$MAIN_SHA" 2>/dev/null || true
        fi
        SS_PR=$(gh pr list --repo mono/SkiaSharp --head "$BRANCH" --json number --jq '.[0].number' 2>/dev/null || echo "")
        if [ -z "$SS_PR" ]; then
          echo "Creating mono/SkiaSharp draft PR..."
          SS_PR=$(gh pr create --repo mono/SkiaSharp \
            --head "$BRANCH" --base main \
            --title "[autobump] Bump skia to milestone ${TARGET}" \
            --draft \
            --body "Automated Skia milestone bump to m${TARGET}. Pending agent run." 2>/dev/null | grep -oE '[0-9]+$' || echo "")
        fi
        echo "skiasharp_pr=$SS_PR" >> "$GITHUB_OUTPUT"

        echo "Ready: target=m${TARGET}, skia PR=#${SKIA_PR}, SkiaSharp PR=#${SS_PR}"
jobs:
  pre-activation:
    outputs:
      current: ${{ steps.detect.outputs.current }}
      next: ${{ steps.detect.outputs.next }}
      latest: ${{ steps.detect.outputs.latest }}
      target: ${{ steps.detect.outputs.target }}
      mode: ${{ steps.detect.outputs.mode }}
      skia_pr: ${{ steps.detect.outputs.skia_pr }}
      skiasharp_pr: ${{ steps.detect.outputs.skiasharp_pr }}
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
  push-to-pull-request-branch:
    target: "*"
    title-prefix: "[autobump] "
    max: 2
    if-no-changes: ignore
    check-branch-protection: false
    protected-files: allowed
    allowed-repos: ["mono/skia"]
  update-pull-request:
    target: "*"
    max: 2
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

## Step 4 — Update SkiaSharp parent repo

Follow **Phases 6–9** of the skill:

1. `pwsh .agents/skills/update-skia/scripts/update-versions.ps1 -Current ${{ needs.pre_activation.outputs.current }} -Target ${{ needs.pre_activation.outputs.target }}`
2. `pwsh .agents/skills/update-skia/scripts/regenerate-bindings.ps1`
3. Fix C# wrappers per Phase 8
4. Build and test:
   ```bash
   dotnet build binding/SkiaSharp/SkiaSharp.csproj
   dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj
   ```

Commit all SkiaSharp changes on the `autobump/skia-m${{ needs.pre_activation.outputs.target }}` branch.

## Step 5 — Push to PRs and update descriptions

The PRs already exist (created by the pre-step). Use `push_to_pull_request_branch` to push
your commits to both PRs. Then use `update_pull_request` to set the PR descriptions.

**Push to mono/skia PR #${{ needs.pre_activation.outputs.skia_pr }}:**
- `pull_request_number`: ${{ needs.pre_activation.outputs.skia_pr }}
- `repo`: `mono/skia`

**Push to mono/SkiaSharp PR #${{ needs.pre_activation.outputs.skiasharp_pr }}:**
- `pull_request_number`: ${{ needs.pre_activation.outputs.skiasharp_pr }}

**Update mono/skia PR description:**
- `pull_request_number`: ${{ needs.pre_activation.outputs.skia_pr }}
- `repo`: `mono/skia`
- Include: breaking change analysis, merge details

**Update mono/SkiaSharp PR description:**
- `pull_request_number`: ${{ needs.pre_activation.outputs.skiasharp_pr }}
- Include: breaking change analysis, companion skia PR link, build/test status
