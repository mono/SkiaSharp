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

- **[documentation/dependencies.md](../../../documentation/dependencies.md)** — Complete dependency list, cgmanifest.json format, CVE database names
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

👉 See [documentation/dependencies.md](../../../documentation/dependencies.md#cgmanifestjson) for the cgmanifest format.

### Phase 4: Build & Test

> 🛑 **MANDATORY: Build locally before creating PRs.**

See [documentation/building.md](../../../documentation/building.md#building-native-libraries) for platform-specific build commands.

```bash
dotnet cake --target=externals-macos --arch=arm64  # Example

dotnet cake --target=tests-netcore --skipExternals=all
```

> ⚠️ **Never use `dotnet test` directly** — use Cake for proper skip trait handling.

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

In the `externals/skia` directory, create a branch named `dev/update-{dep}`, commit the DEPS and BUILD.gn changes, push, and create a PR targeting the `skiasharp` branch.

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

For cgmanifest names and upstream URLs, see [documentation/dependencies.md](../../../documentation/dependencies.md#name-mapping).
