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
  "what version of libpng", "check for breaking changes", "update DEPS".
---

# Native Dependency Update Skill

Update native dependencies in SkiaSharp's Skia fork (mono/skia).

## ⚠️ MANDATORY: Follow Every Phase

**"Do everything" means "follow every step of this skill."**

You MUST complete ALL phases in order. Do not skip phases to save time.
Do not interpret user requests like "please do" or "do everything" as permission to take shortcuts.

### Pre-Flight Checklist

Before starting, confirm you will:
- [ ] Complete Phase 1-8 in order
- [ ] Update BOTH `DEPS` AND `cgmanifest.json`
- [ ] Build and test locally before any PR
- [ ] Create PRs (never push directly to `skiasharp` or `main`)
- [ ] Use "Fixes #NNNNN" in PR body (never close issues manually)
- [ ] Stop and ask at every 🛑 checkpoint

## Critical Rules

> **🛑 STOP AND ASK** before any of these actions:
> - Creating PRs (publicly visible, triggers CI)
> - Merging PRs (irreversible)
> - Force pushing branches
> - Any destructive git operations
>
> Present a summary of what will happen and wait for explicit approval.

### ❌ NEVER Do These

| Shortcut | Why It's Wrong |
|----------|----------------|
| Push directly to `skiasharp` branch | Bypasses PR review and CI |
| Push directly to `main` branch | Bypasses PR review and CI |
| Skip build/test phase | Broken code may be merged |
| Manually close issues | Breaks audit trail; PR merge auto-closes |
| Skip `cgmanifest.json` update | Security compliance requires it |
| Skip asking for approval at 🛑 points | User must approve visible/irreversible actions |
| Revert/undo pushed commits | Breaks everyone who pulled; history is permanent |
| Force push to shared branches | Breaks everyone who pulled; history is permanent |

> ⚠️ **Pushed commits are permanent.** If you made a mistake, fix it forward with a new commit. Never revert or force push shared branches.

## Architecture

```
SkiaSharp repo
└── externals/skia (mono/skia fork, submodule)
    ├── DEPS (pins dependencies to commit hashes)
    └── third_party/
        ├── {dep}/BUILD.gn (build config)
        └── externals/{dep}/ (source checkout)
```

**Key insight:** Changes go to mono/skia first, then SkiaSharp updates its submodule.

---

## Workflow

### Phase 1: Discovery

**Goal:** Understand current state and target version.

1. Check `externals/skia/DEPS` for the dependency entry (format: `"third_party/externals/{name}": "{url}@{commit}"`)

2. Navigate to the dependency checkout at `externals/skia/third_party/externals/{name}` and determine the current version from git tags or commit history

3. Find the target version:
   - For CVE fixes: Check the security advisory
   - For bug fixes: Check upstream changelog
   - For general updates: Use latest stable or match upstream Google Skia

4. Get the commit hash for the target version using `git rev-parse {tag}^{commit}` (handles annotated tags correctly)

**Present findings to user:** Current version, target version, and reason for update.

### Phase 2: Analysis

**Goal:** Assess risk and identify required changes.

Review changes between versions:
- Check changelog/release notes for breaking changes or API modifications
- Look for new source files that may need adding to BUILD.gn
- Look for deleted source files that would break the build
- Check header files for API changes

**Risk assessment:**
- Security-only releases → usually safe
- Patch versions (1.2.3 → 1.2.4) → typically safe
- Minor versions (1.2 → 1.3) → review changelog carefully
- Major versions (1.x → 2.x) → expect breaking changes

👉 See [references/breaking-changes.md](references/breaking-changes.md) for detailed guidance.

**Present analysis to user:** Risk level, any BUILD.gn changes needed, potential concerns.

### Phase 3: Local Changes

**Goal:** Make and verify changes locally.

1. Edit `externals/skia/DEPS` with the new commit hash

2. Update `externals/skia/third_party/{dep}/BUILD.gn` if new source files were added (rare)

3. **Update `cgmanifest.json`** in the SkiaSharp root with the new commit hash (for security compliance)

4. Checkout the new version in the dependency directory

> ⚠️ **Do not skip step 3.** The `cgmanifest.json` file is required for security compliance and must be updated alongside DEPS.

**Present changes to user:** Summary of files modified.

### Phase 4: Build & Test

> ⚠️ **Do not skip this phase.** Even "simple" version bumps can break builds.

**Goal:** Verify the update works.

Build for one platform to verify:
```bash
dotnet cake --target=externals-macos --arch=arm64
```

Run tests (must use Cake for proper skip trait handling):
```bash
dotnet cake --target=tests-netcore --skipExternals=all
```

> ⚠️ **Never use `dotnet test` directly** - it will crash on tests that expect to be skipped on certain platforms.

**If build fails:** Check for missing symbols or files. Usually means BUILD.gn needs updating.

**Present results to user:** Build status, test results, any failures.

### Phase 5: Create PRs

> **🛑 STOP AND ASK FOR APPROVAL**
> 
> Before creating PRs, present:
> - Summary of changes (dependency, old version → new version)
> - Related issues found
> - PR titles and descriptions that will be used
>
> Wait for user to confirm before proceeding.

**Both PRs must be created together** - CI requires both to exist.

> ❌ **NEVER push directly to `skiasharp` or `main` branches.** Always create a PR from a `dev/update-{dep}` branch.

#### Step 1: Find related issues

Search SkiaSharp for related issues (e.g., CVE reports, update requests).

#### Step 2: Create mono/skia PR

In `externals/skia`:
- Create branch `dev/update-{dep}` (e.g., `dev/update-expat`)
- Commit the DEPS change
- Push branch and create PR targeting `skiasharp` branch
- Fill template: link to SkiaSharp issue, leave SkiaSharp PR as placeholder

> ❌ **Do NOT push directly to `skiasharp`.** Create a PR.

#### Step 3: Create SkiaSharp PR

In SkiaSharp root:
- Create branch `dev/update-{dep}` (e.g., `dev/update-expat`)
- Commit cgmanifest.json change
- Push branch and create PR targeting `main` branch
- Fill template: link to mono/skia PR, and list related issues as bullet points (see below)

> ❌ **Do NOT push directly to `main`.** Create a PR.

**Linking issues:** Add a bullet point for each related issue using "Fixes #NNNNN" syntax.
When the PR is merged, these issues will be **automatically closed** by GitHub.

```markdown
- Fixes #3389
- Fixes #3425
```

> ⚠️ **Never close issues manually.** The PR merge will auto-close them.

#### Step 4: Cross-link PRs

Edit the mono/skia PR to add the SkiaSharp PR link.

**Present to user:** Links to both PRs and the issue.

### Phase 6: Monitor CI

**Goal:** Ensure CI passes before merge.

**CI Systems:**
- **SkiaSharp** uses Azure DevOps for build/test
- **mono/skia** has no CI - relies on SkiaSharp's CI

Check both CI status and PR state (PRs may auto-merge when CI passes):

```
# Check CI status
gh pr checks {pr-number} --repo mono/SkiaSharp

# Check if PR was auto-merged
gh pr view {pr-number} --repo mono/SkiaSharp --json state,merged
```

Status meanings:
- `pending` - Build in progress
- `success` - All checks passed  
- `failure` - Build or tests failed

**Note:** PRs may have auto-merge enabled. Always check the PR state, not just CI status—the PR may already be merged.

**If CI fails:** Check Azure DevOps logs for details. Common issues:
- Platform-specific build failures (BUILD.gn may need updates)
- Test failures (may indicate breaking changes)

**Present to user:** CI status, PR state (open/merged), any failures.

### Phase 7: Merge

> **🛑 STOP AND ASK FOR APPROVAL**
>
> Before each merge, confirm:
> - CI status is green (or failures are unrelated)
> - User wants to proceed
> - PR hasn't already been auto-merged
>
> Merging is irreversible.

#### Step 1: Merge mono/skia PR

Squash merge the mono/skia PR to `skiasharp` branch.

#### Step 2: Update SkiaSharp submodule

After mono/skia merges:
- Fetch the new commit in `externals/skia`
- Checkout the merged commit (now on `skiasharp` branch)
- Update the submodule reference in SkiaSharp
- Commit and push (new commit, not amend)

#### Step 3: Check for auto-merge

The SkiaSharp PR may auto-merge once CI passes. Check the PR state before attempting manual merge.

#### Step 4: Merge SkiaSharp PR (if not auto-merged)

If the PR hasn't auto-merged, squash merge it to `main` once CI is green.

### Phase 8: Verify

Confirm completion:
- Related issues were auto-closed (linked with "Fixes #NNNNN" in PR body)
- Both PRs show as merged
- No failures on main branch

> ⚠️ **Never close issues manually.** If issues didn't auto-close, check that the "Fixes #NNNNN" syntax was correct in the PR body.

**Present to user:** Final status and any follow-up needed.

---

## Common Dependencies

| Dependency | DEPS Key | Notes |
|------------|----------|-------|
| libpng | `third_party/externals/libpng` | PNG images |
| libexpat | `third_party/externals/expat` | XML/SVG parsing |
| zlib | `third_party/externals/zlib` | Compression |
| libwebp | `third_party/externals/libwebp` | WebP images |
| harfbuzz | `third_party/externals/harfbuzz` | Text shaping |
| freetype | `third_party/externals/freetype` | Font rendering |
| libjpeg-turbo | `third_party/externals/libjpeg-turbo` | JPEG images |

## Security Updates

For CVE/security fixes, also:
- Identify all CVE numbers
- Verify the fix is in the target commit
- Check for additional CVEs fixed in newer versions
- Decide whether to mention CVEs in PR (may prefer to keep quiet)

## References

- [references/breaking-changes.md](references/breaking-changes.md) - Breaking change analysis
- [documentation/building.md](../../../documentation/building.md) - Build setup
- [CONTRIBUTING.md](../../../CONTRIBUTING.md) - PR guidelines
