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

Read and follow `.agents/skills/update-skia/SKILL.md` for the detailed workflow phases.
This workflow runs the skill in **automated mode** — all analysis goes into PR descriptions,
no human approval gates are needed.

## Step 1 — Discovery

Determine the current milestone and available upstream targets.

```bash
# Current milestone
grep "^libSkiaSharp.*milestone" scripts/VERSIONS.txt
```

```bash
# Set up upstream remote and fetch
cd externals/skia
git remote add upstream https://github.com/google/skia.git 2>/dev/null || true
git fetch upstream --quiet
git fetch origin --quiet
cd ../..
```

```bash
# Find the latest upstream milestone
cd externals/skia
git branch -r | sed -n 's|.*upstream/chrome/m\([0-9]*\).*|\1|p' | sort -n | tail -1
cd ../..
```

The **current** milestone is from VERSIONS.txt. The targets are:
- **Next**: current + 1
- **Latest**: highest `chrome/m*` branch found upstream

If next == latest, there's only one target. Deduplicate.

For each target, check if `upstream/chrome/m{N}` exists. If not, skip it.

## Step 2 — Process each target milestone

For each target milestone (process them sequentially, lowest first):

### 2a. Check for existing work

Check if a branch `autobump/skia-m{N}` already exists on `origin` in the submodule:

```bash
cd externals/skia
git rev-parse --verify "origin/autobump/skia-m{N}" 2>/dev/null
cd ../..
```

If the branch exists and is already up-to-date with `upstream/chrome/m{N}` (merge-base equals
upstream tip), skip this target — it's already current.

### 2b. Merge upstream in submodule

Follow **Phase 4** of the update-skia skill:

```bash
cd externals/skia

# Create or checkout the branch
if git rev-parse --verify "origin/autobump/skia-m{N}" 2>/dev/null; then
  git checkout -b "autobump/skia-m{N}" "origin/autobump/skia-m{N}" 2>/dev/null || \
    git checkout "autobump/skia-m{N}"
  git reset --hard "origin/autobump/skia-m{N}"
else
  git checkout "origin/skiasharp" --detach
  git checkout -b "autobump/skia-m{N}"
fi

# Merge upstream
git merge --no-commit "upstream/chrome/m{N}"
```

If there are conflicts, resolve them following the strategy table from the skill:

| File Category | Strategy |
|--------------|----------|
| `BUILD.gn` | **Combine both** — keep upstream structure AND SkiaSharp's platform flags + `skiasharp_build` target |
| `DEPS` | **Combine** — keep our dependency pins, accept upstream structure |
| `RELEASE_NOTES.md`, `infra/bots/` | **Take upstream** (`git checkout --theirs`) |
| C API (`include/c/`, `src/c/`) | **Keep SkiaSharp** (`git checkout --ours`) — these don't exist upstream |
| Other upstream source | **Check history**: `git log --oneline skiasharp -- <file>` — if fork patches exist, keep ours |

**⚠️ MANDATORY**: Before resolving ANY conflict, check file history for fork-specific patches.
See `.agents/skills/update-skia/references/known-gotchas.md` gotcha #15.

After resolving all conflicts:

```bash
git commit -m "Merge upstream chrome/m{N}"
```

Verify:
```bash
ls src/c/*.cpp include/c/*.h       # C API files intact
git diff --check                     # Zero conflict markers
```

### 2c. Breaking change analysis

Follow **Phase 2** of the update-skia skill. Read the Skia release notes and analyze
breaking changes. **Include the full analysis in the PR description** (see Step 4).

### 2d. Fix C API shim layer

Follow **Phase 5** of the update-skia skill. Build native on Linux x64:

```bash
cd ../..  # Back to repo root
dotnet cake --target=externals-linux --arch=x64
```

If there are compilation errors, fix them following the error patterns in the skill.
Iterate: fix → rebuild → fix → rebuild until clean.

After fixing, commit the C API changes in the submodule:
```bash
cd externals/skia
git add -A
git commit -m "Adapt SkiaSharp shims for m{N}"
```

### 2e. Push submodule branch

```bash
cd externals/skia
git push origin "autobump/skia-m{N}" --force-with-lease 2>/dev/null || \
  git push -u origin "autobump/skia-m{N}"
cd ../..
```

### 2f. Create mono/skia PR

Use the GitHub MCP tool to check if a PR already exists for this branch in mono/skia
targeting `skiasharp`. If not, create one with:
- Title: `Update skia to milestone {N}`
- Base: `skiasharp`
- Head: `autobump/skia-m{N}`
- Draft: true
- Body: Include the breaking change analysis from step 2c

## Step 3 — Update SkiaSharp parent repo

Back in the parent repo root:

### 3a. Update submodule pointer

```bash
git add externals/skia
```

### 3b. Update version files (Phase 6)

```bash
pwsh .agents/skills/update-skia/scripts/update-versions.ps1 -Current {CURRENT} -Target {N}
```

### 3c. Regenerate bindings (Phase 7)

```bash
pwsh .agents/skills/update-skia/scripts/regenerate-bindings.ps1
```

### 3d. Fix C# wrappers (Phase 8)

Review new generated bindings for unwrapped functions. Fix any C# compilation issues
in `binding/SkiaSharp/` following the skill's Phase 8 guidance.

### 3e. Build C#

```bash
dotnet build binding/SkiaSharp/SkiaSharp.csproj
```

### 3f. Test

```bash
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj
```

## Step 4 — Commit for PR creation

Use `autobump/skia-m{N}` as the branch name.

Commit all changes with message:
```
Bump skia to milestone {N}

Automated merge of upstream chrome/m{N}.
```

The `safe-outputs: create-pull-request` will create the mono/SkiaSharp PR.
Include in the PR body:
- Breaking change analysis summary from Step 2c
- Link to the companion mono/skia PR from Step 2f
- List of any new generated functions that may need C# wrappers
- Build/test status

## Step 5 — Summary

Output a summary of what was done:
- Which milestones were processed
- What action was taken (created, updated, up-to-date, skipped)
- Links to any PRs created
- Any issues that need human attention (test failures, unresolved build errors)
