---
name: update-skia
description: >
  Update the Skia graphics library to a new Chrome milestone in SkiaSharp's mono/skia fork.
  Handles upstream merge, C API shim fixes, binding regeneration, C# wrapper updates, and
  dual-repo PR coordination.

  Use when user asks to:
  - Update/bump Skia to a new milestone (m120, m121, etc.)
  - Merge upstream Skia changes
  - Update the Skia submodule to a newer version
  - Check what Skia milestone is current or what version of Skia is used

  Triggers: "update skia", "bump skia", "skia milestone", "update to m121",
  "merge upstream skia", "skia update", "new skia version", "what milestone",
  "what version of skia", "current skia version", "check skia version".

  For updating individual dependencies (libpng, zlib, etc.), use `native-dependency-update` instead.
  For security audits, use `security-audit` instead.
---

# Update Skia Milestone Skill

Update Google Skia to a new Chrome milestone in SkiaSharp's mono/skia fork.

## Key References

- **[references/breaking-changes-checklist.md](references/breaking-changes-checklist.md)** — How to analyze breaking changes between milestones
- **[references/known-gotchas.md](references/known-gotchas.md)** — Hard-won lessons and troubleshooting table
- **[references/typical-changes.md](references/typical-changes.md)** — Files typically changed during an update
- **[documentation/dev/dependencies.md](../../../documentation/dev/dependencies.md)** — Dependency tracking and cgmanifest.json format
- **[RELEASE_NOTES.md in upstream Skia](https://raw.githubusercontent.com/google/skia/main/RELEASE_NOTES.md)** — Official Skia release notes

## Scripts

- **`scripts/update-versions.ps1`** — Phase 6: Updates all version files and runs verification
- **`scripts/regenerate-bindings.ps1`** — Phase 8: Regenerates bindings, reverts HarfBuzz, reports new functions

## Overview

Updating Skia is the **highest-risk operation** in SkiaSharp. It touches:
- The native C++ Skia library (upstream merge)
- The custom C API shim layer (must be adapted to new C++ APIs)
- Generated P/Invoke bindings
- C# wrapper code
- All platforms (macOS, Windows, Linux, iOS, Android, etc.)

**Go slow. Research first. Build and test before any PR.**

## ⚠️ Follow Every Phase In Order

This is an 11-phase workflow where each phase builds on the previous one. The phases exist
because Skia updates touch four layers (C++ → C API → generated bindings → C# wrappers)
and two repositories (mono/skia + mono/SkiaSharp). Skipping a phase doesn't just risk a
build failure — it risks shipping broken binaries to customers who won't see the problem
until runtime.

Each phase ends with a gate — a verification step that confirms the phase completed
correctly. Re-read each phase's instructions before executing it, because the details
are project-specific and easy to get wrong from memory.

### What You're About to Do

```
A. Research (Phases 1–3)
   1. Discovery & Current State
   2. Breaking Change Analysis
   3. Validation

B. Branch & Merge (Phases 4–5)
   4. Branch Setup
   5. Upstream Merge

C. Update & Build (Phases 6–7)
   6. Update Version Files
   7. Fix C API Shim & Build Native

D. Regenerate & Verify (Phases 8–10)
   8. Regenerate Bindings
   9. Fix C# Wrappers
  10. Build & Test

E. Ship (Phase 11)
  11. Create PRs
```

## Critical Rules

> **🛑 STOP AND ASK** before: Merging PRs, Force pushing, Deleting branches, Any destructive git operations

### 🚫 BRANCH PROTECTION (MANDATORY)

| Repository | Protected Branches | Action Required |
|------------|-------------------|-----------------|
| **mono/SkiaSharp** (parent) | `main` | Create feature branch first |
| **mono/skia** (submodule) | `main`, `skiasharp` | Create feature branch first |

### ❌ NEVER Do These

| Shortcut | Why It's Wrong |
|----------|----------------|
| Push directly to `skiasharp` or `main` | Bypasses PR review and CI |
| Skip breaking change analysis | Causes runtime crashes for customers |
| Use `externals-download` after C API changes | Causes `EntryPointNotFoundException` |
| Merge both PRs without updating submodule in between | Squash-merge orphans commits |
| Skip tests | Untested code = broken customers |

---

## A. Research (Phases 1–3)

### Phase 1: Discovery & Current State

1. **Identify current milestone**:
   ```bash
   grep SK_MILESTONE externals/skia/include/core/SkMilestone.h
   grep "^libSkiaSharp.*milestone" scripts/VERSIONS.txt
   grep chrome_milestone cgmanifest.json
   ```

2. **Identify target milestone** from user request.

3. **Check for existing PRs** — Search both mono/SkiaSharp and mono/skia for open update PRs.

4. **Verify upstream branches exist and fetch**:
   ```bash
   cd externals/skia
   git remote add upstream https://github.com/google/skia.git 2>/dev/null
   git fetch upstream {UPSTREAM_REF}   # chrome/m{TARGET} for a milestone, or main for tip mode
   ```
   `{UPSTREAM_REF}` is the upstream ref you merge from: `chrome/m{TARGET}` for a normal
   milestone/release-line update, or `main` for a `main`/tip sync (see step 5).
   > **Note:** When this phase is pre-computed by the automated workflow, you still need to
   > add the `upstream` remote and fetch `{UPSTREAM_REF}` — Phase 5 depends on it.

5. **Determine the base branch (`main` vs a release line).** Most updates target `main`
   (newest, in-development line) with `skiasharp` as the mono/skia base. But when the target
   milestone is **older** than `main`'s milestone AND a matching release branch exists, the
   update targets that release line instead:

   ```bash
   # SkiaSharp uses release/<major>.<milestone>.x (e.g. release/4.148.x for m148)
   git ls-remote --heads origin "refs/heads/release/*.{TARGET}.x"
   # mono/skia mirrors the SAME branch name
   git -C externals/skia ls-remote --heads origin "refs/heads/release/*.{TARGET}.x"
   ```

   | Variable | `main` target | release-line target | `main`/tip mode |
   |----------|---------------|---------------------|-----------------|
   | `{UPSTREAM_REF}` (merge from) | `chrome/m{TARGET}` | `chrome/m{TARGET}` | `main` (HEAD) |
   | `{BASE_BRANCH}` (SkiaSharp) | `main` | `release/<major>.{TARGET}.x` | `main` |
   | `{SKIA_BASE_BRANCH}` (mono/skia) | `skiasharp` | `release/<major>.{TARGET}.x` | `skiasharp` |
   | `{HEAD_BRANCH}` (both repos) | `skia-sync/m{TARGET}` | `skia-sync/release-<major>.{TARGET}.x` | `skia-sync/main` |

   The table's three columns are the three possible sync modes. **`main` target** (the default)
   is a normal milestone bump; the other two are special cases:

   **Release-line target** — chosen when `{TARGET}` is older than `main`'s milestone AND a
   matching `release/<major>.{TARGET}.x` branch exists. Always a **bug-fix-only sync**
   (`CURRENT == TARGET`): do NOT bump the milestone/soname/nuget version in Phase 6 — only
   `cgmanifest.json`'s commit hash changes.

   > 🛑 **The release branch must already exist in BOTH repos.** Branch *creation* is owned by
   > the release process (`release-branch` skill), not this skill. If the SkiaSharp
   > `release/<major>.{TARGET}.x` branch exists but the mono/skia one does NOT, **STOP and fail** —
   > do not create it here. Ask a human to cut the mono/skia release branch first.

   **`main` (tip) mode** — instead of a `chrome/m{TARGET}` milestone branch, merge the very tip of
   upstream `google/skia` `main` (HEAD). Like a `main` target it uses `{BASE_BRANCH}` = `main` and
   `{SKIA_BASE_BRANCH}` = `skiasharp`, but with `{HEAD_BRANCH}` = `skia-sync/main` and
   `{UPSTREAM_REF}` = `main`, and `CURRENT == TARGET`, so it is **NOT a version bump** — keep the
   milestone/soname/nuget versions unchanged. Unlike a release-line bug-fix sync it MAY still
   include new upstream API/binding changes, so regenerate + build + test as normal (Phases 8–10).
   A tip merge is large and conflict-heavy because the submodule base is well behind `main`, so the
   verify-upstream-or-reapply policy (Phase 5 / [gotcha #15](references/known-gotchas.md)) is
   mandatory. Because the tip is not a milestone branch, the milestone-pair diff in **Phase 2 step 4**
   doesn't apply — substitute `$(git merge-base {SKIA_BASE_BRANCH} upstream/main)..upstream/main`
   (Phase 5's diffs already use `{UPSTREAM_REF}`, so they need no change).

   Use these `{UPSTREAM_REF}` / `{BASE_BRANCH}` / `{SKIA_BASE_BRANCH}` / `{HEAD_BRANCH}` values
   everywhere below in place of the hardcoded `chrome/m{TARGET}` / `main` / `skiasharp` /
   `skia-sync/m{TARGET}` defaults.

> 🛑 **GATE**: Confirm current milestone, target milestone, the base branch (main vs release line,
> with the mono/skia base branch confirmed to exist), and that the upstream branch exists.

### Phase 2: Breaking Change Analysis

**This is the most critical phase.** Thorough analysis here prevents customer-facing breakage.

1. **Read official release notes** for EVERY milestone being skipped:
   - Fetch `https://raw.githubusercontent.com/google/skia/main/RELEASE_NOTES.md`
   - Document all changes for each milestone between current and target

2. **Categorize changes by impact**:

   | Category | Risk | Examples |
   |----------|------|---------|
   | **Removed APIs** | 🔴 HIGH | Functions deleted, enums removed |
   | **Renamed/Moved APIs** | 🟡 MEDIUM | Namespace changes, header moves |
   | **New APIs** | 🟢 LOW | Additive changes, new factories |
   | **Behavior changes** | 🟡 MEDIUM | Default changes, semantic shifts |
   | **Graphite-only** | ⚪ SKIP | SkiaSharp uses Ganesh, not Graphite |

3. **Map each HIGH/MEDIUM change to C API files**:
   ```bash
   cd externals/skia
   # Check which C API files reference affected APIs
   grep -r "GrMipmapped\|GrMipMapped" src/c/ include/c/
   grep -r "refTypefaceOrDefault\|getTypefaceOrDefault" src/c/ include/c/
   ```

4. **Run structural diff on include/ directory**:
   ```bash
   git diff upstream/chrome/m{CURRENT}..upstream/chrome/m{TARGET} --stat -- include/
   git diff upstream/chrome/m{CURRENT}..upstream/chrome/m{TARGET} -- include/core/ include/gpu/ganesh/
   ```

👉 See [references/breaking-changes-checklist.md](references/breaking-changes-checklist.md) for the full analysis template, including verification steps for struct sizes, moved files, and diff-reading traps.

> 🛑 **GATE**: Include the breaking change analysis in the PR description body. Summarize the
> key findings (HIGH/MEDIUM risk changes and their C API impact) for the user.

### Phase 3: Validation

The agent performing the breaking change analysis has blind spots — it may filter out
relevant changes or miss moved headers. An independent validation catches these before
they become runtime crashes.

Launch an **explore agent** with `model: "claude-opus-4.6"` using the prompt template from
[references/validation-prompt.md](references/validation-prompt.md) — substitute the
milestone numbers and paste your breaking change analysis table. The default explore model
(Haiku) is too weak for accurate header-level validation — use Opus for reliability.

> 🛑 **GATE**: Validation agent has run and confirmed analysis. If it found missed items,
> update the analysis and re-present to user before proceeding.

> ✅ **Before proceeding to B (Branch & Merge):**
> - Current and target milestones confirmed
> - Breaking change analysis complete
> - Validation passed

---

## B. Branch & Merge (Phases 4–5)

### Phase 4: Branch Setup

> ⚠️ **This phase creates BOTH branches before making ANY changes.** You may be on a
> workflow branch, a feature branch, or a detached HEAD — none of which is the right base.
> You MUST branch from `origin/{BASE_BRANCH}` (parent) and `origin/{SKIA_BASE_BRANCH}`
> (submodule). For a `main` update (and `main`/tip mode) those are `origin/main` and
> `origin/skiasharp`; for a release-line update they are `origin/release/<major>.{TARGET}.x` in
> both repos (see Phase 1 step 5).

**Parent repo (SkiaSharp):**

1. **Fetch the latest base branch:**
   ```bash
   git fetch origin {BASE_BRANCH}
   ```

2. **Create the feature branch from `origin/{BASE_BRANCH}`:**
   ```bash
   git checkout -b {HEAD_BRANCH} origin/{BASE_BRANCH}
   ```
   If the branch already exists on the remote, check it out instead:
   ```bash
   git fetch origin {HEAD_BRANCH} && git checkout {HEAD_BRANCH}
   ```

3. **Verify you are on the correct branch and it is based on `origin/{BASE_BRANCH}`:**
   ```bash
   git log --oneline -1 origin/{BASE_BRANCH}
   git log --oneline -1 HEAD
   ```
   These should show the same commit (or HEAD should be ahead by only your own commits).

**Submodule (mono/skia):**

4. **Enter the submodule:**
   ```bash
   cd externals/skia
   ```

5. **Align to the SHA that `origin/{BASE_BRANCH}` expects** (the submodule tracks
   `{SKIA_BASE_BRANCH}` in mono/skia — `skiasharp` for a `main` update, or the matching
   `release/<major>.{TARGET}.x` for a release-line update — NOT `main`):
   ```bash
   BASE_SUB_SHA=$(git -C ../.. ls-tree origin/{BASE_BRANCH} -- externals/skia | awk '{print $3}')
   git fetch origin {SKIA_BASE_BRANCH}
   git checkout "$BASE_SUB_SHA"
   ```
   Verify this SHA is on `origin/{SKIA_BASE_BRANCH}`:
   ```bash
   git branch -r --contains "$BASE_SUB_SHA" | grep 'origin/{SKIA_BASE_BRANCH}'
   ```

6. **Create the submodule feature branch:**
   ```bash
   git checkout -b {HEAD_BRANCH}
   ```

> ⚠️ **Do NOT skip the SHA alignment step (step 5).** If the submodule is at a different
> SHA than `origin/{BASE_BRANCH}` expects, the merge will produce phantom diffs — functions that
> already exist on the base branch will appear as new or removed.

> 🛑 **GATE**: Both branches created. Verify:
> ```bash
> # In parent repo:
> git rev-parse --abbrev-ref HEAD          # → {HEAD_BRANCH}
> git merge-base HEAD origin/{BASE_BRANCH}  # → should match origin/{BASE_BRANCH} tip
> # In submodule:
> git -C externals/skia rev-parse --abbrev-ref HEAD  # → {HEAD_BRANCH}
> ```

### Phase 5: Upstream Merge (mono/skia)

You should still be inside `externals/skia` from Phase 4.

1. **Merge upstream** — use `--no-commit` for manual conflict resolution:
   ```bash
   git merge --no-commit upstream/{UPSTREAM_REF}   # chrome/m{TARGET}, or main in tip mode
   ```

2. **Resolve conflicts** — each conflict must be resolved individually.
   Never use `git merge -s ours` or `git read-tree --reset` — this destroys `git blame` attribution.

   **⚠️ MANDATORY: classify every fork patch as *upstreamed* or *not* before resolving.**
   For each conflicted file, list the fork patches touching it and check whether upstream already
   carries each one (see [gotcha #15](references/known-gotchas.md) for the exact commands):

   ```bash
   git log --oneline {SKIA_BASE_BRANCH} -- <conflicted-file>           # our fork patches on this file
   git log -S "<distinctive code>" --oneline upstream/{UPSTREAM_REF}   # did upstream adopt it?
   ```

   - **Upstreamed** → take upstream's (possibly refined) form; record `"<subject>" upstreamed as <sha>`.
   - **Not upstreamed** → re-apply our patch on top of upstream's edits; **never drop it**; record `re-applied`.
   - **Never** blanket `git checkout --theirs`/`--ours` on a file you have not classified.

   | File Category | Strategy |
   |--------------|----------|
   | `BUILD.gn` | **Combine both** — keep upstream structure AND SkiaSharp's platform flags + `skiasharp_build` target |
   | `DEPS` | **Combine** — keep our dependency pins, accept upstream structure |
   | `RELEASE_NOTES.md`, `infra/` | **Take upstream** |
   | C API (`include/c/`, `src/c/`) | **Keep SkiaSharp** — adapt includes/API calls in post-merge commits |
   | Other upstream source (`src/`, `include/`) | **Verify-upstream-or-reapply** — see [gotcha #15](references/known-gotchas.md) |

   **Audit (mandatory).** Snapshot fork patches before merging, then cross-reference every conflict:
   ```bash
   MB=$(git merge-base {SKIA_BASE_BRANCH} upstream/{UPSTREAM_REF})
   git log --oneline "$MB..{SKIA_BASE_BRANCH}" > /tmp/fork-patches-before.txt
   ```
   For every conflicted file, the fork patch(es) touching it must appear in the mono/skia PR's "Conflicts
   resolved" table as *upstreamed* or *re-applied*. A fork patch on a conflicted file that is neither is a
   lost patch — STOP and fix it. (Patches whose files did not conflict merge cleanly and need no listing.)

3. **Commit the merge**:
   ```bash
   git commit  # Creates proper two-parent merge
   ```

4. **Verify our C API files survived the merge**:
   ```bash
   ls src/c/*.cpp include/c/*.h  # All files should still exist
   ```

5. **Source file verification** — Check for added/deleted upstream files:
   ```bash
   git diff $(git merge-base {SKIA_BASE_BRANCH} upstream/{UPSTREAM_REF})..upstream/{UPSTREAM_REF} --diff-filter=AD --name-only -- src/ include/
   ```
   Cross-reference against `BUILD.gn` — new source files may need to be added.

> 🛑 **GATE**: Merge complete, conflicts resolved. Verify:
> ```bash
> ls src/c/*.cpp include/c/*.h                    # C API files intact
> git diff --check                                  # Zero conflict markers
> git blame src/c/sk_canvas.cpp | head -20         # Attribution shows original commits, not just merge
> ```

> ✅ **Before proceeding to C (Update & Build):**
> - Parent branch is based on `origin/{BASE_BRANCH}`
> - Submodule is at the SHA referenced by the parent's `origin/{BASE_BRANCH}` submodule pointer
> - Upstream merge committed with proper two-parent history
> - C API files intact, zero conflict markers

---

## C. Update & Build (Phases 6–7)

### Phase 6: Update SkiaSharp Version Files

> **⚠️ This MUST be done before any native build.** The build scripts verify version
> consistency — if VERSIONS.txt still says the old milestone, the build will fail.

> **Note:** The script automatically resets `SK_C_INCREMENT` to `0` when the milestone changes.
> If you had a pending increment that must survive, capture it before running.

> 📋 **This phase is handled by a script.** The script updates VERSIONS.txt, cgmanifest.json,
> azure-templates-variables.yml, and verifies SK_C_INCREMENT — then runs the mandatory
> verification greps. It exits non-zero if any stale references remain.

In the **SkiaSharp parent repo**, run:
```bash
cd ../..  # back to parent repo (Phase 5 ends inside externals/skia)
pwsh .agents/skills/update-skia/scripts/update-versions.ps1 -Current {CURRENT} -Target {TARGET}
```

> **Main-tip sync?** When Phase 5 merged `upstream/main` (not a `chrome/m{TARGET}` branch),
> add `-UpstreamRef main` so `cgmanifest.json`'s `upstream_merge_commit` is resolved from the
> ref you actually merged. Without it the script defaults to `upstream/chrome/m{TARGET}`, which
> does not exist on a tip merge, and `upstream_merge_commit` is left `UNKNOWN` for you to set by hand.

The script handles all of these (so you don't have to do them manually):
- `scripts/VERSIONS.txt`: milestone, increment→0, soname, assembly, file, ALL ~30 nuget lines
- `cgmanifest.json`: commitHash, version, chrome_milestone, upstream_merge_commit
- `scripts/azure-templates-variables.yml`: `SKIASHARP_VERSION` (must match VERSIONS.txt nuget version)
- Verifies `SK_C_INCREMENT` is 0 in `externals/skia/include/c/sk_types.h`
- Runs mandatory `grep` verification — fails if any stale references remain

> 🛑 **GATE**: Script exits with ✅. If it exits with ❌, fix the reported stale references
> and re-run until it passes.

> **Note:** The SK_C_INCREMENT reset modifies a file in the submodule (`externals/skia/`).
> Don't commit it separately — it will be committed with Phase 7's C API fixes.

### Phase 7: Fix C API Shim & Build Native

This is where most of the work happens. The C API (`src/c/`, `include/c/`) wraps Skia C++ and
must be updated when the underlying C++ APIs change.

> **❌ NEVER use `externals-download` during a milestone update.** It downloads pre-built
> binaries from the OLD milestone that don't contain your C API changes. Always build from
> source with `externals-{platform}`.

1. **Restore tools and attempt to build** to identify all compilation errors:
   ```bash
   dotnet tool restore
   dotnet cake --target=externals-{platform} --arch={arch}
   ```
   Replace `{platform}` with your OS (`macos`, `linux`, `windows`) and `{arch}` with your architecture (`arm64`, `x64`).

2. **Fix each error** following these patterns:

   | Error Type | Fix Pattern |
   |-----------|-------------|
   | Missing type | Add/update typedef in `sk_types.h` |
   | Renamed function | Update call in `*.cpp` |
   | Removed enum value | Remove from `sk_enums.cpp` + `sk_types.h`. Note this for Phase 9 — it needs `[Obsolete]` or documented removal |
   | Changed signature | Update C wrapper function signature |
   | New header required | Add `#include` in the relevant `.cpp` |
   | Legacy flag breaks C API | Update C API to use replacement API (see gotcha #6). Do not just comment out the flag without a plan |
   | New *required* upstream gn arg | A new upstream dependency our fork doesn't vendor may need a gn toggle (e.g. `skia_use_partition_alloc=false`). Add it to the affected platforms' `native/**/build.cake` gn-args lists — NOT a one-off `--gnArgs` flag (see [gotcha #23](references/known-gotchas.md)) |

   > **GN args belong in `build.cake`, not CLI flags.** When the upstream merge introduces a
   > *genuinely required* new gn argument (typically a dependency our `DEPS` deliberately doesn't
   > vendor), add it to **every affected platform's** `native/**/build.cake` gn-args list — that
   > file is the single source of truth, next to the existing `skia_use_*` toggles. Don't paper
   > over it with a one-off `dotnet cake … --gnArgs` flag (non-durable), and don't add gn args (or
   > change compiler/linker flags) merely to silence a host-specific build error — that's a
   > missing-dependency problem, not a config one. Full rationale + the `skia_use_partition_alloc`
   > example and the milestone-sequencing caveat: [gotcha #23](references/known-gotchas.md).

3. **Update `sk_types.h`** for any new enums or type changes.
   Phase 6 reset `SK_C_INCREMENT` to 0. Only bump it if you add new C API functions in this milestone.
   The build enforces that `SK_C_INCREMENT` matches `libSkiaSharp increment` in `VERSIONS.txt`.

4. **Build again** — iterate until clean compilation.

> 🛑 **GATE**: Native library builds successfully on at least one platform.

> ✅ **Before proceeding to D (Regenerate & Verify):**
> - Version files updated (Phase 6 script passed)
> - Native library builds cleanly

---

## D. Regenerate & Verify (Phases 8–10)

### Phase 8: Regenerate Bindings

> **Prerequisite:** Phase 7's native build must have completed at least once — it runs
> `git-sync-deps`, which fetches HarfBuzz and other headers the generator needs.

> 📋 **This phase is handled by a script.** The script runs the generator, IMMEDIATELY
> reverts HarfBuzz bindings (HarfBuzz updates are always separate), reports what changed,
> and lists any new functions that may need C# wrappers.

```bash
pwsh .agents/skills/update-skia/scripts/regenerate-bindings.ps1
```

The script handles all of these (so you don't forget any):
- Runs `pwsh ./utils/generate.ps1`
- Reverts `binding/HarfBuzzSharp/HarfBuzzApi.generated.cs` (proactively, not reactively)
- Reports the binding diff summary
- Lists NEW generated functions that may need C# wrappers in Phase 9

After the script completes, build C# to verify compilation:
```bash
dotnet build binding/SkiaSharp/SkiaSharp.csproj
```

> 🛑 **GATE**: Script prints `✅ Phase 8 complete`. C# build succeeds with 0 errors.

### Phase 9: Fix C# Wrappers

The C# build can pass with 0 errors while new C API functions remain invisible to users.
New functions compile fine as unused `internal static` methods in the generated file, but
without C# wrappers they're not part of the public API. This phase applies even when
the build succeeds.

1. **Review new generated bindings for unwrapped functions:**
   ```bash
   git diff origin/{BASE_BRANCH} -- binding/SkiaSharp/SkiaApi.generated.cs | grep "^+.*internal static"
   ```
   > ⚠️ The `git diff origin/{BASE_BRANCH}` may show additional changes beyond new functions (e.g.
   > struct renames, type changes from Phase 7 shim work). These are expected and correct.
   > Only investigate `+internal static` lines — ignore other diff noise.

2. **Check whether each new function has a C# wrapper:**
   ```bash
   # Example: if sk_foo_bar was added, check for a wrapper
   grep -rn "sk_foo_bar" binding/SkiaSharp/*.cs | grep -v generated
   ```
   New functions from our custom C API additions typically need wrappers.
   New functions from upstream changes are usually additive and can be deferred.

3. **Fix files in `binding/SkiaSharp/`** based on the breaking change analysis:

   | File | When to Update |
   |------|---------------|
   | `Definitions.cs` | New enums, types, or constants |
   | `EnumMappings.cs` | New enum values that need C#↔C mapping |
   | `GRDefinitions.cs` | Graphics context changes (Ganesh) |
   | `SKImage.cs` | SkImage factory changes |
   | `SKTypeface.cs` | SkTypeface API changes |
   | `SKFont.cs` | SkFont API changes |
   | `SKCanvas.cs` | Canvas drawing API changes |

**Key rules:**
- **Add new overloads**, never modify existing signatures (ABI stability)
- Use `[Obsolete]` for deprecated APIs with migration guidance
- Return `null` from factory methods on failure (don't throw)

### Phase 10: Build & Test

```bash
# Rebuild native only if you touched C API files in Phase 9
# (Phase 7 already built — skip if no native changes since then)
dotnet cake --target=externals-{platform} --arch={arch}

# Build C#
dotnet build binding/SkiaSharp/SkiaSharp.csproj
```

1. **Smoke tests (fast gate, ~100ms):**
   ```bash
   dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj --filter "Category=Smoke"
   ```
   Smoke tests verify basic native interop: version compatibility, object creation, drawing,
   image loading, fonts, codecs, effects, and more. If these fail, something fundamental is
   broken — go back and fix before wasting time on the full suite.

   > ⚠️ If the version compatibility smoke test fails with "incompatible native library",
   > you missed a version update — go back to Phase 6 and verify ALL version lines.
   > Do NOT work around this with `--no-incremental` or by copying native libs manually.

2. **Full test suite (required before any PR):** capture the output to a log you can inspect
   afterward. Standalone runs can use any writable path; the automated workflow overrides this to
   `/tmp/gh-aw/agent/test-output.txt` so it's uploaded as an artifact (see the workflow's Phase 10 note).
   ```bash
   dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj 2>&1 | tee /tmp/skia-test-output.txt
   ```
   Wait for it to finish (takes 5–7 min). Then read the summary:
   ```bash
   tail -5 /tmp/skia-test-output.txt
   ```
   The last line will look like: `Passed!  - Failed:     0, Passed:  5435, Skipped:   171, Total:  5606`

   This runs all test projects (core, Vulkan, Direct3D). Backend-specific tests
   self-skip when hardware isn't available. CI handles WASM/Android/iOS separately.

   > **⚠️ These MUST be two separate commands.** Do NOT combine them into a single pipeline
   > like `| tee ... | tail` — the piped tail runs immediately and will show nothing useful
   > while tests are still running. Capture with `tee` first, wait for completion, then `tail`
   > the output file. After the run, inspect failures with:
   > ```bash
   > grep '^  Failed' /tmp/skia-test-output.txt
   > ```

Smoke tests are just that — smoke. They verify the basics. The full suite MUST pass
before the update can be considered complete. Do not create PRs with only smoke tests passing.

> 🛑 **GATE**: ALL tests pass (full suite, not just smoke). Do NOT skip failing tests.
> Do NOT proceed with failures.

> ✅ **Before proceeding to E (Ship):**
> - Bindings regenerated (Phase 8 script passed)
> - C# builds with 0 errors
> - ALL tests pass (full suite)

---

## E. Ship (Phase 11)

### Phase 11: Create PRs

> **Same-milestone bug-fix syncs:** When `CURRENT == TARGET`, only `cgmanifest.json`'s
> `commitHash`/`upstream_merge_commit` change. Use PR titles like
> `[skia-sync] Merge upstream chrome/m{TARGET} bug fixes` instead of milestone-bump titles.
> A release-line update is always such a sync.

> **Branch targets by mode:** Use the `{BASE_BRANCH}` / `{SKIA_BASE_BRANCH}` / `{HEAD_BRANCH}`
> values resolved in Phase 1 step 5. For a `main` update they are `main` / `skiasharp` /
> `skia-sync/m{TARGET}`; for `main`/tip mode `main` / `skiasharp` / `skia-sync/main`; for a
> release-line update `release/<major>.{TARGET}.x` (both repos) / `skia-sync/release-<major>.{TARGET}.x`.

#### PR 1: mono/skia (submodule)

| Field | Value |
|-------|-------|
| Branch | `{HEAD_BRANCH}` |
| Target | `{SKIA_BASE_BRANCH}` |
| Title | `[skia-sync] Merge upstream chrome/m{TARGET}` |

#### PR 2: mono/SkiaSharp (parent)

| Field | Value |
|-------|-------|
| Branch | `{HEAD_BRANCH}` |
| Target | `{BASE_BRANCH}` |
| Title | `[skia-sync] Update skia to milestone {TARGET}` (or `Merge upstream chrome/m{TARGET} bug fixes` when `CURRENT == TARGET`) |

**Submodule must point to the mono/skia PR branch.**

#### Cross-link both PRs (same as native-dependency-update).

After creating BOTH PRs, update the earlier PR's description to include a link to the later one.
Both PRs must reference each other.

#### Phase 11 Completion Checklist

Before proceeding to merge, verify ALL of these:

- [ ] Branch names follow `{HEAD_BRANCH}` convention in BOTH repos
- [ ] mono/skia PR targets `{SKIA_BASE_BRANCH}` branch
- [ ] mono/SkiaSharp PR targets `{BASE_BRANCH}` branch
- [ ] **SkiaSharp's `externals/skia` submodule points to the mono/skia PR branch** (`git submodule status`)
- [ ] `cgmanifest.json` updated with new commit hash, version, and chrome_milestone
- [ ] `scripts/VERSIONS.txt` updated (ALL version lines, not just milestone)
- [ ] `SkiaApi.generated.cs` regenerated and committed
- [ ] Both PRs cross-reference each other
- [ ] Native build passes on at least one platform
- [ ] C# build passes with 0 errors
- [ ] All tests pass (with proper platform filtering)

#### Merge Sequence (CRITICAL)

1. Merge mono/skia PR first → creates new squashed SHA on `{SKIA_BASE_BRANCH}`
2. Fetch new SHA in SkiaSharp's submodule
3. Update submodule pointer, push to SkiaSharp PR branch
4. **Only then** merge SkiaSharp PR

#### Merge Checklist

Before proceeding past each step, verify:

- [ ] mono/skia PR merged
- [ ] Fetched `{SKIA_BASE_BRANCH}` branch to get new squashed SHA
- [ ] Updated SkiaSharp submodule to new SHA (`cd externals/skia && git fetch origin && git checkout {new-sha}`)
- [ ] Pushed submodule update to SkiaSharp PR branch
- [ ] CI passes on updated SkiaSharp PR
- [ ] SkiaSharp PR merged
- [ ] **Submodule points to a commit on `{SKIA_BASE_BRANCH}` branch** (not an orphaned branch commit)

> ❌ **NEVER** merge both PRs without updating the submodule in between.
> ❌ **NEVER** assume the submodule reference is correct after squash-merging mono/skia.

---

## Reference Material

These files contain lookup information — consult them when you hit a problem or need context,
not necessarily upfront:

- **[references/known-gotchas.md](references/known-gotchas.md)** — Hard-won lessons from past updates (DEF_STRUCT_MAP, emsdk, BUILD.gn flags, HarfBuzz, DEPS forks, etc.) and a troubleshooting table
- **[references/typical-changes.md](references/typical-changes.md)** — Files typically changed in each repository during an update
- **[references/breaking-changes-checklist.md](references/breaking-changes-checklist.md)** — How to analyze breaking changes between milestones
