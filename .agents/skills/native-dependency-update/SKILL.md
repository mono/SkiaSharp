---
name: native-dependency-update
description: >
  Update native dependencies (libpng, libexpat, zlib, libwebp, harfbuzz, freetype, libjpeg-turbo, etc.)
  in SkiaSharp's Skia fork. Handles security CVE fixes, bug fixes, and version bumps.
  
  Use when user asks to:
  - Bump/update a native dependency (libpng, zlib, expat, webp, etc.)
  - Fix a CVE or security vulnerability in a native library
  - Update Skia's DEPS file
  - Check what version of a dependency is currently used
  - Analyze breaking changes between dependency versions
  
  Triggers: "bump libpng", "update zlib", "fix CVE in expat", "update native deps",
  "what version of libpng", "check for breaking changes".

  For security audits (finding CVEs, checking PR coverage), use the `security-audit` skill instead.
---

# Native Dependency Update Skill

Update native dependencies in SkiaSharp's Skia fork (mono/skia).

## Key References

- **[documentation/dev/dependencies.md](../../../documentation/dev/dependencies.md)** — Complete dependency list, cgmanifest.json format, CVE database names
- **[references/breaking-changes.md](references/breaking-changes.md)** — Breaking change analysis guidance

## ⚠️ MANDATORY: Follow Every Phase

You MUST complete ALL phases in order. Do not skip phases to save time.

### Pre-Flight Checklist

Before starting, confirm you will:
- [ ] Complete Phase 1-8 in order
- [ ] Update DEPS, `externals/skia` submodule, AND `cgmanifest.json`
- [ ] Build and test locally before any PR
- [ ] Create PRs (never push directly to `skiasharp` or `main`)
- [ ] Use "Fixes #NNNNN" in PR body (never close issues manually)
- [ ] Stop and ask at every 🛑 checkpoint

## Critical Rules

> **🛑 STOP AND ASK** before: Creating PRs, Merging PRs, Force pushing, Any destructive git operations

### 🚫 BRANCH PROTECTION (MANDATORY COMPLIANCE)

> **⛔ POLICY VIOLATION: Direct commits to protected branches are prohibited.**

**This rule applies to BOTH repositories:**

| Repository | Protected Branches | Action Required |
|------------|-------------------|-----------------|
| **mono/SkiaSharp** (parent repo) | `main` | Create feature branch first |
| **mono/skia** (`externals/skia` submodule) | `main`, `skiasharp` | Create feature branch first |

**Before ANY commit in either repository:**

1. **Create a feature branch** — Use naming convention: `dev/issue-NNNN-description` or `dev/update-{dep}`
2. **Never commit directly** to `main` or `skiasharp` — All changes require a PR
3. **This is a compliance requirement** — Direct commits bypass review, CI, and audit trails

```bash
# ✅ CORRECT — Always create feature branch first
cd externals/skia
git checkout skiasharp
git checkout -b dev/update-libpng
# Now make commits...

# ❌ WRONG — Never do this
cd externals/skia
git checkout skiasharp
git commit -m "Update libpng"  # POLICY VIOLATION
```

### ❌ NEVER Do These

| Shortcut | Why It's Wrong |
|----------|----------------|
| Push directly to `skiasharp` or `main` | Bypasses PR review and CI |
| Skip native build phase | CI is too slow; must verify locally first |
| Manually close issues | Breaks audit trail; PR merge auto-closes |
| Skip `cgmanifest.json` update | Security compliance requires it |
| Skip `externals/skia` submodule update | SkiaSharp won't use the new dependency version |
| Revert/undo pushed commits | Fix forward with new commit instead |
| **Merge both PRs without updating submodule in between** | **Squash-merge creates new SHA; submodule points to orphaned commit; BREAKS USERS** |

---

## Phase 0: Environment Setup (MANDATORY FIRST STEP)

Before ANY other action in a new worktree session:

### Step 1: Initialize all submodules
```bash
git submodule update --init
```
⚠️ `externals/skia` is ~900MB and takes ~8 minutes to clone. Wait for completion.
Do NOT attempt to read DEPS, edit files, or build until this finishes.

### Step 2: Set up PATH and tools
```bash
export PATH="/usr/local/share/dotnet:/opt/homebrew/bin:$PATH"
```
⚠️ PATH does not persist between bash tool calls. Prefix EVERY `dotnet` command with this export.
⚠️ Environment variables do not persist across bash tool calls. Every `gh` write command (pr create, pr edit, etc.) must be prefixed with `unset GH_TOKEN &&` to clear the read-only default token.
⚠️ `grep -P` (Perl regex) is NOT available on macOS (BSD grep). Use `grep -E` or `sed` instead.

### Step 3: Unshallow the target dependency
`git-sync-deps` clones dependencies as shallow repos. `git log` and `git diff` across version ranges will show incorrect results in shallow repos. Always unshallow the dependency you're updating:
```bash
cd externals/skia/third_party/externals/{dep}
git fetch --unshallow origin
cd -
```

### Step 4: Set target branch (for release branch work)
If you're targeting a non-default branch (e.g., `release/3.119.x`):
```bash
git fetch origin {target_branch}
git reset --hard origin/{target_branch}
# Re-run submodule update to get the correct submodule commit:
git submodule update
```

### Step 5: Verify environment
```bash
dotnet --version && ls externals/skia/DEPS && ls externals/depot_tools/ninja.py
```
Do NOT proceed to Phase 1 until all three checks pass.

---

## Workflow

### Phase 1: Discovery

1. **Check for existing PRs** in mono/SkiaSharp and mono/skia
2. **Check current version** in `externals/skia/DEPS`
3. **Find target version** — get commit hash with `git rev-parse {tag}^{commit}`

### Phase 2: Analysis

**Source File Verification (MANDATORY):**

```bash
cd externals/skia/third_party/externals/{dep}
git diff {old}..{new} --diff-filter=AD --name-only  # Added/Deleted files
```

Cross-reference against `externals/skia/third_party/{dep}/BUILD.gn` — new source files may need to be added.

👉 See [references/breaking-changes.md](references/breaking-changes.md) for risk assessment.

### Phase 3: Local Changes

1. Edit `externals/skia/DEPS` with new commit hash
2. Update BUILD.gn if needed (rare)
3. **Update `cgmanifest.json`** with new version (required for CVE detection)
4. Checkout new version in dependency directory

👉 See [documentation/dev/dependencies.md](../../../documentation/dev/dependencies.md#cgmanifestjson) for the cgmanifest format.

### Phase 4: Build & Test

> 🛑 **MANDATORY: Build locally before creating PRs.**

See [documentation/dev/building.md](../../../documentation/dev/building.md#building-native-libraries) for platform-specific build commands.

```bash
dotnet cake --target=externals-macos --arch=arm64  # Example

# Run all tests (core + Vulkan + Direct3D — backends self-skip if unavailable)
dotnet test tests/SkiaSharp.Tests.Console.sln
```

### Build Retry Strategy

**Common transient failure: HTTP 429 from chromium.googlesource.com**

When running `dotnet cake --target=externals-macos`, the `git-sync-deps` step fetches 10+ dependencies from Google's mirrors in parallel. If multiple sessions run concurrently, you'll hit rate limits:
```
error: RPC failed; HTTP 429 curl 22 The requested URL returned error: 429
Exception: Thread failure detected
```

**Strategy:**
1. Wait a short while (rate limits are transient)
2. Retry the build command
3. If it still fails after 3 retries, stop and ask for help

Do NOT attempt to manually clone dependencies from other repos — you may pick wrong versions or SHAs.

**Other common build issues:**
- `fetch-gn` network abort → retry (transient)
- `--no-restore` flag on `dotnet test` → remove it, let NuGet restore run
- Test TTY noise → pipe to file or use `tail -50` to read results

### cgmanifest.json Schema

The cgmanifest.json uses different structures per component type:
- Type `"other"`: `component.other.name`, `component.other.version`
- Type `"git"`: `component.git.repositoryUrl`, `component.git.commitHash`

Do NOT assume all entries use the same schema. Check `component.type` first.

### Phase 5: Create PRs

> **🛑 STOP AND ASK FOR APPROVAL** before creating PRs.

**Both PRs must be created together** — CI requires both.

#### Branch Naming Convention

| Repository | Branch Name | Target Branch |
|------------|-------------|---------------|
| mono/skia | `dev/update-{dep}` | `skiasharp` |
| mono/SkiaSharp | `dev/update-{dep}` | `main` |

Example: For libfoo, use `dev/update-libwebp` in both repos.

#### Step 1: Create mono/skia PR

In the `externals/skia` directory:

1. **Create the branch with its final name** — do NOT create-then-rename:
   ```bash
   git checkout -b dev/update-{dep} origin/skiasharp
   ```
2. **Stage ALL changes before committing** (DEPS, BUILD.gn if changed)
3. **Commit ONCE** with this exact format:
   ```
   Update {dep} to {version}

   Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
   ```
4. **Push ONCE** and create a PR targeting the `skiasharp` branch

Do NOT commit-then-amend. Every amend requires a force-push which re-triggers CI (wasting 2+ hours of compute).

#### Step 2: Create SkiaSharp PR

> **⚠️ CRITICAL: You MUST update the submodule reference, not just cgmanifest.json**

In the SkiaSharp root, create a branch named `dev/update-{dep}`. Then:

1. **Update the submodule** — In `externals/skia`, fetch and checkout the branch you just pushed in Step 1
2. **Stage both changes** — `git add externals/skia cgmanifest.json` (the submodule AND the manifest)
3. Commit, push, and create a PR targeting `main`

#### Step 3: Cross-link the PRs

Edit **both** PRs to reference each other:
- mono/skia PR → Add: `Required SkiaSharp PR: https://github.com/mono/SkiaSharp/pull/{number}`
- mono/SkiaSharp PR → Add: `Required skia PR: https://github.com/mono/skia/pull/{number}`

### GitHub CLI for mono org repos

The default `GH_TOKEN` is read-only and causes `gh pr create`, `gh pr edit`, and `gh run rerun` to fail. Prefix every `gh` write command with `unset GH_TOKEN &&`:
```bash
unset GH_TOKEN && gh pr create ...
unset GH_TOKEN && gh pr edit ...
```

### Release Branch PRs

The `create_pull_request` built-in tool always targets the repo's default branch (`main`). For release branch PRs, immediately fix after creation:
```bash
gh api repos/mono/SkiaSharp/pulls/{number} --method PATCH \
  -f base="{target_branch}"
```

#### Phase 5 Completion Checklist

Before proceeding, verify ALL of these:

- [ ] Branch names follow `dev/update-{dep}` convention
- [ ] mono/skia PR targets `skiasharp` branch
- [ ] mono/SkiaSharp PR targets `main` branch
- [ ] **SkiaSharp's `externals/skia` submodule points to the mono/skia PR branch** (check with `git submodule status`)
- [ ] `cgmanifest.json` updated with new version
- [ ] Both PRs cross-reference each other

### Phase 6: Monitor CI

SkiaSharp uses Azure DevOps. mono/skia has no CI — relies on SkiaSharp's.

### Phase 7: Merge

> **🛑 STOP AND ASK FOR APPROVAL** before each merge.

> **🚨 CRITICAL: SQUASH MERGE CREATES NEW COMMITS**
>
> When you squash-merge mono/skia PR, GitHub creates a **NEW commit SHA** on the `skiasharp` branch.
> The original commits on `dev/update-{dep}` become **orphaned** when the branch is deleted.
> 
> **If SkiaSharp's submodule still points to the old (orphaned) commit, it will BREAK:**
> - New clones will fail
> - Submodule updates will fail
> - Users cannot build SkiaSharp
>
> **YOU MUST UPDATE THE SUBMODULE BEFORE MERGING SKIASHARP PR.**

#### Merge Sequence (MANDATORY)

1. **Merge mono/skia PR first** — This creates a new squashed commit on the `skiasharp` branch
2. **Fetch the updated skiasharp branch** and note the new commit SHA
3. **Update the SkiaSharp submodule** to point to the new squashed commit (not the old branch commit)
4. **Push the updated submodule reference** to the SkiaSharp PR branch
5. **Only then merge the SkiaSharp PR**

#### Merge Checklist

Before proceeding past each step, verify:

- [ ] mono/skia PR merged
- [ ] Fetched `skiasharp` branch to get new SHA
- [ ] Updated SkiaSharp submodule to new SHA (`cd externals/skia && git checkout {new-sha}`)
- [ ] Pushed submodule update to SkiaSharp PR branch
- [ ] SkiaSharp PR merged

> ❌ **NEVER** merge both PRs in quick succession without updating the submodule in between.
> ❌ **NEVER** assume the submodule reference is correct after squash-merging mono/skia.

#### If You Must Amend a Pushed Commit
If you must amend a commit in `externals/skia`:
1. Amend the skia commit
2. Force-push the skia branch
3. In SkiaSharp root: `git add externals/skia` (picks up new SHA)
4. `git commit --amend --no-edit`
5. Force-push the SkiaSharp branch

⚠️ NEVER amend the skia commit without also updating the parent submodule reference. The old SHA becomes orphaned after force-push.

### Phase 8: Verify

- Related issues auto-closed
- Both PRs merged
- No failures on main
- **Submodule points to a commit on `skiasharp` branch** — fetch main, check that `externals/skia` commit exists on `origin/skiasharp` (not orphaned)

---

## Common Dependencies

| Dependency | DEPS Key |
|------------|----------|
| libpng | `third_party/externals/libpng` |
| libexpat | `third_party/externals/expat` |
| zlib | `third_party/externals/zlib` |
| libwebp | `third_party/externals/libwebp` |
| harfbuzz | `third_party/externals/harfbuzz` |
| freetype | `third_party/externals/freetype` |
| libjpeg-turbo | `third_party/externals/libjpeg-turbo` |

For cgmanifest names and upstream URLs, see [documentation/dev/dependencies.md](../../../documentation/dev/dependencies.md#name-mapping).

---

## Security Bump Protocol

When the dependency update is security-related (CVE fix):

### What to Report to the User (in session conversation ONLY)
1. Full CVE analysis — which CVEs are fixed, severity, CVSS scores
2. Which CVEs affect SkiaSharp's code paths and which don't (with reasoning)
3. Behavior changes that may need test coverage
4. Upstream issues that remain unfixed

### What Goes in Public Artifacts (PRs, commits, branches)
- **Commit message:** `Update {dep} to {version}` — NOTHING else
- **PR title:** `Update {dep} to {version}` (optionally add target branch like `(release/3.119.x)`)
- **PR body:** Version numbers, file changes, build verification results ONLY
- **Branch name:** `dev/update-{dep}` — NEVER include CVE IDs

### Prohibited in Public Artifacts
- ❌ CVE IDs (e.g., CVE-2026-XXXXX)
- ❌ Severity ratings or CVSS scores
- ❌ Words: "security", "vulnerability", "exploit", "fix CVE", "security bump"
- ❌ Version ranges like "from X to Y" in commit messages
- ❌ Branch context like "(release/3.119.x)" in commit messages

### After-Bump Checklist (report to user in session)
Before marking the task complete, tell the user:
- "Here's my security analysis for your records: ..."
- "These CVEs affect our code paths: ... / do NOT affect us because: ..."
- "New tests recommended: yes/no, because: ..."
