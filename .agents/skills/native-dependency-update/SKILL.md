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
- [ ] Complete Phase 0-8 in order
- [ ] Update DEPS, `externals/skia` submodule, AND `cgmanifest.json`
- [ ] Build and test locally before any PR
- [ ] Create PRs (never push directly to protected branches)
- [ ] Stop and ask at every 🛑 checkpoint

## Critical Rules

> **🛑 STOP AND ASK** before: Creating PRs, Merging PRs, Force pushing, Any destructive git operations

### 🚫 BRANCH PROTECTION (MANDATORY COMPLIANCE)

> **⛔ POLICY VIOLATION: Direct commits to protected branches are prohibited.**

**This rule applies to BOTH repositories:**

| Repository | Protected Branches | Action Required |
|------------|-------------------|-----------------|
| **mono/SkiaSharp** (parent repo) | `main`, `release/*` | Create feature branch first |
| **mono/skia** (`externals/skia` submodule) | `main`, `skiasharp` | Create feature branch first |

**Before ANY commit in either repository:**

1. **Create a feature branch** — Use naming convention: `dev/update-{dep}`
2. **Never commit directly** to protected branches — All changes require a PR
3. **This is a compliance requirement** — Direct commits bypass review, CI, and audit trails

### 🔒 Security Rules (ALWAYS — not just for CVE bumps)

All dependency updates are assumed security-sensitive. These rules apply to EVERY bump:

**Commit message:** `Update {dep} to {version}` — NOTHING else (plus Co-authored-by trailer)
**PR title:** `Update {dep} to {version}`
**PR body:** Version numbers, file changes, build verification results ONLY
**Branch name:** `dev/update-{dep}` — NEVER include CVE IDs

**Prohibited in ALL public artifacts (PRs, commits, branches, PR comments):**
- ❌ CVE IDs (e.g., CVE-2026-XXXXX)
- ❌ Severity ratings or CVSS scores
- ❌ Words: "security", "vulnerability", "exploit", "fix CVE", "security bump"
- ❌ Version ranges like "from X to Y" in commit messages
- ❌ Branch context like "(release/3.119.x)" in commit messages

**Security analysis goes in the session conversation ONLY** — report to the user:
1. Which CVEs are fixed, severity, CVSS scores
2. Which CVEs affect SkiaSharp's code paths and which don't (with reasoning)
3. Behavior changes that may need test coverage
4. Upstream issues that remain unfixed

### ❌ NEVER Do These

| Shortcut | Why It's Wrong |
|----------|----------------|
| Push directly to protected branches | Bypasses PR review and CI |
| Skip native build phase | CI is too slow; must verify locally first |
| Manually close issues | Breaks audit trail; PR merge auto-closes |
| Skip `cgmanifest.json` update | Security compliance requires it |
| Skip `externals/skia` submodule update | SkiaSharp won't use the new dependency version |
| Revert/undo pushed commits | Fix forward with new commit instead |
| **Merge both PRs without updating submodule in between** | **Squash-merge creates new SHA; submodule points to orphaned commit; BREAKS USERS** |
| Include security details in public artifacts | Leaks vulnerability info before users can update |

### Environment Reminders

These do NOT persist across bash tool calls. Prefix every relevant command:

| Command | Prefix |
|---------|--------|
| `dotnet` | `export PATH="/usr/local/share/dotnet:/opt/homebrew/bin:$PATH" &&` |
| `gh pr create`, `gh pr edit`, etc. | `unset GH_TOKEN &&` |
| `grep` (pattern matching) | Use `grep -E` not `grep -P` (BSD grep on macOS) |

---

## Phase 0: Environment Setup (MANDATORY FIRST STEP)

Run the setup script before any other work. It initializes submodules, unshallows the dependency, creates the skia feature branch, and verifies the environment:

```bash
bash .agents/skills/native-dependency-update/scripts/setup.sh {dep} {skia_target_branch} {skiasharp_target_branch}
```

**Arguments:**
| Arg | Default | Examples |
|-----|---------|----------|
| `dep` | (required) | `libpng`, `expat`, `zlib`, `libwebp`, `freetype` |
| `skia_target_branch` | `skiasharp` | `skiasharp`, `release/3.119.x` |
| `skiasharp_target_branch` | `main` | `main`, `release/3.119.x` |

### Determining the skia target branch

⚠️ **NEVER assume the skia target branch.** It depends on what the user is asking:

| User request | `skiasharp_target_branch` | `skia_target_branch` |
|-------------|--------------------------|---------------------|
| Update on main | `main` | `skiasharp` |
| Backport to release branch | `release/3.119.x` | Ask the user |

If you're unsure which skia branch to target, **ask the user**. Do not guess.

**After the script completes**, proceed to Phase 1.

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

Both repos use branch name `dev/update-{dep}`. The skia branch was already created by the setup script.

#### Step 1: Create mono/skia PR

The branch `dev/update-{dep}` already exists in `externals/skia` (created by setup script).

1. **Stage ALL changes before committing** (DEPS, BUILD.gn if changed)
2. **Commit ONCE** with this exact format:
   ```
   Update {dep} to {version}

   Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
   ```
3. **Push ONCE** and create a PR targeting `{skia_target_branch}`:
   ```bash
   unset GH_TOKEN && gh pr create --repo mono/skia --base {skia_target_branch} --title "Update {dep} to {version}" --body "..."
   ```

Do NOT commit-then-amend. Every amend requires a force-push which re-triggers CI (wasting 2+ hours of compute).

#### Step 2: Create SkiaSharp PR

> **⚠️ CRITICAL: You MUST update the submodule reference, not just cgmanifest.json**

1. **Update the submodule** — In `externals/skia`, fetch and checkout the branch you just pushed in Step 1
2. **Stage both changes** — `git add externals/skia cgmanifest.json` (the submodule AND the manifest)
3. **Commit ONCE** with the same format as Step 1
4. **Push and create PR** targeting `{skiasharp_target_branch}`:
   - If targeting `main`: use the `create_pull_request` tool
   - If targeting a release branch: use the `create_pull_request` tool, then immediately fix the base:
     ```bash
     unset GH_TOKEN && gh pr edit {number} --repo mono/SkiaSharp --base {skiasharp_target_branch}
     ```

#### Step 3: Cross-link the PRs

Edit **both** PRs to reference each other:
```bash
unset GH_TOKEN && gh pr edit {skia_pr_number} --repo mono/skia --body "...Required SkiaSharp PR: https://github.com/mono/SkiaSharp/pull/{number}..."
unset GH_TOKEN && gh pr edit {skiasharp_pr_number} --repo mono/SkiaSharp --body "...Required skia PR: https://github.com/mono/skia/pull/{number}..."
```

#### Phase 5 Completion Checklist

Before proceeding, verify ALL of these:

- [ ] Branch names follow `dev/update-{dep}` convention
- [ ] mono/skia PR targets `{skia_target_branch}` branch
- [ ] mono/SkiaSharp PR targets `{skiasharp_target_branch}` branch
- [ ] **SkiaSharp's `externals/skia` submodule points to the mono/skia PR branch** (check with `git submodule status`)
- [ ] `cgmanifest.json` updated with new version
- [ ] Both PRs cross-reference each other
- [ ] **No security details in any public artifact** (see Security Rules above)

### Phase 6: Monitor CI

SkiaSharp uses Azure DevOps. mono/skia has no CI — relies on SkiaSharp's.

### Phase 7: Merge

> **🛑 STOP AND ASK FOR APPROVAL** before each merge.

> **🚨 CRITICAL: SQUASH MERGE CREATES NEW COMMITS**
>
> When you squash-merge mono/skia PR, GitHub creates a **NEW commit SHA** on the target branch.
> The original commits on `dev/update-{dep}` become **orphaned** when the branch is deleted.
> 
> **If SkiaSharp's submodule still points to the old (orphaned) commit, it will BREAK:**
> - New clones will fail
> - Submodule updates will fail
> - Users cannot build SkiaSharp
>
> **YOU MUST UPDATE THE SUBMODULE BEFORE MERGING SKIASHARP PR.**

#### Merge Sequence (MANDATORY)

1. **Merge mono/skia PR first** — This creates a new squashed commit on `{skia_target_branch}`
2. **Fetch the updated `{skia_target_branch}`** and note the new commit SHA
3. **Update the SkiaSharp submodule** to point to the new squashed commit (not the old branch commit)
4. **Push the updated submodule reference** to the SkiaSharp PR branch
5. **Only then merge the SkiaSharp PR**

#### Merge Checklist

Before proceeding past each step, verify:

- [ ] mono/skia PR merged
- [ ] Fetched `{skia_target_branch}` to get new SHA
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
- No failures on `{skiasharp_target_branch}`
- **Submodule points to a commit on `{skia_target_branch}`** — fetch the target branch, check that `externals/skia` commit exists on it (not orphaned)

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
