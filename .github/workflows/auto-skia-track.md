---
description: "Daily upstream Skia milestone tracking — merges new commits, resolves conflicts, builds, tests, and creates PRs."
on:
  schedule:
    - cron: "0 9 * * *"
    - cron: "0 15 * * *"
  workflow_dispatch:
    inputs:
      mode:
        description: "Which milestone to track: next (current+1) or latest (highest upstream)"
        required: false
        type: choice
        default: next
        options: [next, latest]
  steps:
    - name: Detect milestone
      id: detect
      env:
        INPUT_MODE: ${{ github.event.inputs.mode }}
        SCHEDULE: ${{ github.event.schedule }}
      run: |
        # Determine mode from input or schedule
        if [ -n "$INPUT_MODE" ]; then
          MODE="$INPUT_MODE"
        elif [ "$SCHEDULE" = "0 15 * * *" ]; then
          MODE=latest
        else
          MODE=next
        fi
        export MODE
        bash scripts/detect-skia-milestones.sh
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
  fetch-depth: 0
  submodules: recursive
timeout-minutes: 120
concurrency:
  group: auto-skia-track-m${{ needs.pre_activation.outputs.target }}
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
    title-prefix: "[autobump] "
    labels: [upstream-tracking]
    draft: true
    allowed-base-branches: [main]
    preserve-branch-name: true
    protected-files: allowed
---

# Auto Skia Track

Automatically track upstream Skia milestones by creating and updating PRs.

This workflow uses the **update-skia skill** (`.agents/skills/update-skia/SKILL.md`).
Read the skill document and its references for detailed guidance on conflict resolution,
breaking change analysis, C API fixes, and all other phases. **Do not deviate from the
skill's instructions** — this workflow just orchestrates when and how to invoke those phases.

## Context (from pre-activation)

- **Current milestone**: m${{ needs.pre_activation.outputs.current }}
- **Next milestone**: m${{ needs.pre_activation.outputs.next }}
- **Latest upstream**: m${{ needs.pre_activation.outputs.latest }}
- **Target milestone**: m${{ needs.pre_activation.outputs.target }}
- **Mode**: ${{ needs.pre_activation.outputs.mode }}

Use `autobump/skia-m${{ needs.pre_activation.outputs.target }}` as the branch name in both repos.

## Step 1 — Merge upstream in submodule

Follow **Phase 4** of the update-skia skill in `externals/skia`:

1. Create or checkout `autobump/skia-m${{ needs.pre_activation.outputs.target }}` branch from `origin/skiasharp`
2. `git merge --no-commit upstream/chrome/m${{ needs.pre_activation.outputs.target }}`
3. Resolve any conflicts following the **skill's conflict strategy table** (Phase 4, Step 3)
4. Verify: no conflict markers, C API files intact
5. Commit the merge

## Step 2 — Breaking change analysis and validation

Follow **Phase 2** of the skill — analyze breaking changes between m${{ needs.pre_activation.outputs.current }} and m${{ needs.pre_activation.outputs.target }}. Save the analysis for the PR description.

Then follow **Phase 3** — run the validation check to catch blind spots.

## Step 3 — Fix C API shim layer

Follow **Phase 5** of the skill. Build native on Linux x64:

```bash
dotnet cake --target=externals-linux --arch=x64
```

Fix compilation errors iteratively per the skill's error pattern table. Commit fixes:

```bash
cd externals/skia
git add -A && git commit -m "Adapt SkiaSharp shims for m${{ needs.pre_activation.outputs.target }}"
```

## Step 4 — Push submodule and create mono/skia PR

```bash
cd externals/skia
git push origin "autobump/skia-m${{ needs.pre_activation.outputs.target }}" --force-with-lease 2>/dev/null || \
  git push -u origin "autobump/skia-m${{ needs.pre_activation.outputs.target }}"
```

Use the GitHub MCP tool to check if a PR exists for `autobump/skia-m${{ needs.pre_activation.outputs.target }}` → `skiasharp`
in mono/skia. If not, create a draft PR with the breaking change analysis in the body.

## Step 5 — Update SkiaSharp parent repo

Follow **Phases 6–9** of the skill:

1. **Phase 6**: `pwsh .agents/skills/update-skia/scripts/update-versions.ps1 -Current ${{ needs.pre_activation.outputs.current }} -Target ${{ needs.pre_activation.outputs.target }}`
2. **Phase 7**: `pwsh .agents/skills/update-skia/scripts/regenerate-bindings.ps1`
3. **Phase 8**: Fix C# wrappers per the skill
4. **Phase 9**: Build C# and run console tests:
   ```bash
   dotnet build binding/SkiaSharp/SkiaSharp.csproj
   dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj
   ```

## Step 6 — Commit for PR creation

Use `autobump/skia-m${{ needs.pre_activation.outputs.target }}` as the branch name. Commit all changes with message:

```
Bump skia to milestone ${{ needs.pre_activation.outputs.target }}

Automated merge of upstream chrome/m${{ needs.pre_activation.outputs.target }}.
```

The `safe-outputs: create-pull-request` creates the mono/SkiaSharp PR. Include in the body:
- Breaking change analysis summary
- Link to the companion mono/skia PR from Step 4
- Build/test status

## Step 7 — Summary

Output a summary: milestone processed, action taken, PR links, issues needing attention.
