---
description: "Daily upstream Skia milestone tracking — merges new commits, resolves conflicts, builds, tests, and creates PRs."
on:
  schedule:
    - cron: "0 9 * * *"
  workflow_dispatch:
checkout:
  fetch-depth: 0
  submodules: recursive
timeout-minutes: 120
concurrency:
  group: auto-skia-track
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

## Step 1 — Discovery

Run the detection script to find current milestone, upstream targets, and existing branches:

```bash
bash scripts/detect-skia-milestones.sh
```

Parse the output. The targets are listed as `TARGET_0`, `TARGET_1`, etc.
If `NO_TARGETS=true`, output "No milestones to track" and stop.

For any target where `UP_TO_DATE_m{N}=true`, skip it — already current.

## Step 2 — Process each target milestone

For each target milestone (process sequentially, lowest first):

### 2a. Merge upstream in submodule

Follow **Phase 4** of the update-skia skill in `externals/skia`:

1. Create or checkout `autobump/skia-m{N}` branch from `origin/skiasharp`
2. `git merge --no-commit upstream/chrome/m{N}`
3. Resolve any conflicts following the **skill's conflict strategy table** (Phase 4, Step 3)
4. Verify: no conflict markers, C API files intact
5. Commit the merge

### 2b. Breaking change analysis

Follow **Phase 2** of the skill. Save the analysis — it goes into the PR description (Step 4).

### 2c. Validation

Follow **Phase 3** of the skill — run the validation check to catch blind spots.

### 2d. Fix C API shim layer

Follow **Phase 5** of the skill. Build native on Linux x64:

```bash
dotnet cake --target=externals-linux --arch=x64
```

Fix compilation errors iteratively per the skill's error pattern table. Commit fixes:

```bash
cd externals/skia
git add -A && git commit -m "Adapt SkiaSharp shims for m{N}"
```

### 2e. Push submodule branch

```bash
cd externals/skia
git push origin "autobump/skia-m{N}" --force-with-lease 2>/dev/null || \
  git push -u origin "autobump/skia-m{N}"
```

### 2f. Create mono/skia PR

Use the GitHub MCP tool to check if a PR exists for `autobump/skia-m{N}` → `skiasharp`
in mono/skia. If not, create a draft PR with the breaking change analysis in the body.

## Step 3 — Update SkiaSharp parent repo

Follow **Phases 6–9** of the skill:

1. **Phase 6**: `pwsh .agents/skills/update-skia/scripts/update-versions.ps1 -Current {CURRENT} -Target {N}`
2. **Phase 7**: `pwsh .agents/skills/update-skia/scripts/regenerate-bindings.ps1`
3. **Phase 8**: Fix C# wrappers per the skill
4. **Phase 9**: Build C# and run console tests:
   ```bash
   dotnet build binding/SkiaSharp/SkiaSharp.csproj
   dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj
   ```

## Step 4 — Commit for PR creation

Use `autobump/skia-m{N}` as the branch name. Commit all changes with message:

```
Bump skia to milestone {N}

Automated merge of upstream chrome/m{N}.
```

The `safe-outputs: create-pull-request` creates the mono/SkiaSharp PR. Include in the body:
- Breaking change analysis summary
- Link to the companion mono/skia PR
- Build/test status

## Step 5 — Summary

Output a summary: which milestones processed, actions taken, PR links, issues needing attention.
