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
- **[references/known-gotchas.md](references/known-gotchas.md)** — 10 hard-won lessons and troubleshooting table
- **[references/typical-changes.md](references/typical-changes.md)** — Files typically changed during an update
- **[documentation/dev/dependencies.md](../../../documentation/dev/dependencies.md)** — Dependency tracking and cgmanifest.json format
- **[RELEASE_NOTES.md in upstream Skia](https://raw.githubusercontent.com/google/skia/main/RELEASE_NOTES.md)** — Official Skia release notes

## Scripts

- **`scripts/update-versions.ps1`** — Phase 6: Updates all version files and runs verification (replaces manual sed/grep)
- **`scripts/regenerate-bindings.ps1`** — Phase 7: Regenerates bindings, reverts HarfBuzz, reports new functions

## Overview

Updating Skia is the **highest-risk operation** in SkiaSharp. It touches:
- The native C++ Skia library (upstream merge)
- The custom C API shim layer (must be adapted to new C++ APIs)
- Generated P/Invoke bindings
- C# wrapper code
- All platforms (macOS, Windows, Linux, iOS, Android, etc.)

**Go slow. Research first. Build and test before any PR.**

## ⚠️ Follow Every Phase In Order

This is a 10-phase workflow where each phase builds on the previous one. The phases exist
because Skia updates touch four layers (C++ → C API → generated bindings → C# wrappers)
and two repositories (mono/skia + mono/SkiaSharp). Skipping a phase doesn't just risk a
build failure — it risks shipping broken binaries to customers who won't see the problem
until runtime.

Each phase ends with a gate — a verification step that confirms the phase completed
correctly. Re-read each phase's instructions before executing it, because the details
are project-specific and easy to get wrong from memory.

### What You're About to Do

The workflow follows this shape:
1. **Research** (Phases 1–3): Understand what changed in Skia, validate the analysis
2. **Merge & Fix** (Phases 4–5): Merge upstream, fix C API compilation errors
3. **Update & Regenerate** (Phases 6–7): Version files and bindings (both handled by scripts)
4. **Verify** (Phases 8–9): Review new functions, build, test
5. **Ship** (Phase 10): Create cross-linked PRs in both repos

## Critical Rules

> **🛑 STOP AND ASK** before: Creating PRs, Merging PRs, Force pushing, Any destructive git operations

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

## Workflow

### Phase 1: Discovery & Current State

1. **Identify current milestone**:
   ```bash
   grep SK_MILESTONE externals/skia/include/core/SkMilestone.h
   grep "^libSkiaSharp.*milestone" scripts/VERSIONS.txt
   grep chrome_milestone cgmanifest.json
   ```

2. **Identify target milestone** from user request

3. **Check for existing PRs** — Search both mono/SkiaSharp and mono/skia for open update PRs

4. **Verify upstream branches exist**:
   ```bash
   cd externals/skia
   git remote add upstream https://github.com/google/skia.git 2>/dev/null
   git fetch upstream chrome/m{TARGET}
   ```

> 🛑 **GATE**: Confirm current milestone, target milestone, and that upstream branch exists.

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

> 🛑 **GATE**: Present full breaking change analysis to user. Get approval before proceeding.

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

### Phase 4: Upstream Merge (mono/skia)

1. **Create feature branch**:
   ```bash
   cd externals/skia
   git checkout skiasharp
   git pull origin skiasharp
   git checkout -b dev/update-skia-{TARGET}
   ```

2. **Merge upstream** — use `--no-commit` for manual conflict resolution:
   ```bash
   git merge --no-commit upstream/chrome/m{TARGET}
   ```

3. **Resolve conflicts** — each conflict must be resolved individually.
   Never use `git merge -s ours` or `git read-tree --reset` — this destroys `git blame` attribution.

   **⚠️ MANDATORY: Before resolving ANY conflict, check file history for fork-specific patches.**
   Run `git log --oneline skiasharp -- <conflicted-file>` — if the log shows intentional
   fork patches, keep our version. See [gotcha #15](references/known-gotchas.md) for details.

   | File Category | Strategy |
   |--------------|----------|
   | `BUILD.gn` | **Combine both** — keep upstream structure AND SkiaSharp's platform flags + `skiasharp_build` target |
   | `DEPS` | **Combine** — keep our dependency pins, accept upstream structure |
   | `RELEASE_NOTES.md`, `infra/bots/` | **Take upstream** |
   | C API (`include/c/`, `src/c/`) | **Keep SkiaSharp** — adapt includes/API calls in post-merge commits |
   | Other upstream source (`src/`, `include/`) | **Check history first** — see [gotcha #15](references/known-gotchas.md) |

4. **Commit the merge**:
   ```bash
   git commit  # Creates proper two-parent merge
   ```

5. **Verify our C API files survived the merge**:
   ```bash
   ls src/c/*.cpp include/c/*.h  # All files should still exist
   ```

6. **Source file verification** — Check for added/deleted upstream files:
   ```bash
   git diff upstream/chrome/m{CURRENT}..upstream/chrome/m{TARGET} --diff-filter=AD --name-only -- src/ include/
   ```
   Cross-reference against `BUILD.gn` — new source files may need to be added.

> 🛑 **GATE**: Merge complete, conflicts resolved. Verify:
> ```bash
> ls src/c/*.cpp include/c/*.h                    # C API files intact
> git diff --check                                  # Zero conflict markers
> git blame src/c/sk_canvas.cpp | head -20         # Attribution shows original commits, not just merge
> ```

### Phase 5: Fix C API Shim Layer

This is where most of the work happens. The C API (`src/c/`, `include/c/`) wraps Skia C++ and
must be updated when the underlying C++ APIs change.

1. **Attempt to build** to identify all compilation errors:
   ```bash
   dotnet cake --target=externals-macos --arch=arm64
   ```

2. **Fix each error** following these patterns:

   | Error Type | Fix Pattern |
   |-----------|-------------|
   | Missing type | Add/update typedef in `sk_types.h` |
   | Renamed function | Update call in `*.cpp` |
   | Removed enum value | Remove from `sk_enums.cpp` + `sk_types.h`. Flag as a C# breaking change — Phase 8 must add `[Obsolete]` or document removal |
   | Changed signature | Update C wrapper function signature |
   | New header required | Add `#include` in the relevant `.cpp` |
   | Legacy flag breaks C API | Update C API to use replacement API (see gotcha #6). Do not just comment out the flag without a plan |

3. **Update `sk_types.h`** for any new enums or type changes:
   - **Reset `SK_C_INCREMENT` to `0`** in `externals/skia/include/c/sk_types.h` for the new milestone
   - Only bump it later if you add new C API functions in the same milestone
   - The build enforces that `SK_C_INCREMENT` matches `libSkiaSharp increment` in `VERSIONS.txt`

4. **Build again** — iterate until clean compilation

> 🛑 **GATE**: Native library builds successfully on at least one platform.

### Phase 6: Update SkiaSharp Version Files

> 📋 **This phase is handled by a script.** The script updates VERSIONS.txt, cgmanifest.json,
> azure-pipelines-variables.yml, and verifies SK_C_INCREMENT — then runs the mandatory
> verification greps. It exits non-zero if any stale references remain.

In the **SkiaSharp parent repo**, run:
```bash
pwsh .claude/skills/update-skia/scripts/update-versions.ps1 -Current {CURRENT} -Target {TARGET}
```

The script handles all of these (so you don't have to do them manually):
- `scripts/VERSIONS.txt`: milestone, increment→0, soname, assembly, file, ALL ~30 nuget lines
- `cgmanifest.json`: commitHash, version, chrome_milestone, upstream_merge_commit
- `scripts/azure-pipelines-variables.yml` (if it exists)
- Verifies `SK_C_INCREMENT` is 0 in `externals/skia/include/c/sk_types.h`
- Runs mandatory `grep` verification — fails if any stale references remain

> 🛑 **GATE**: Script exits with ✅. If it exits with ❌, fix the reported stale references
> and re-run until it passes.

### Phase 7: Regenerate Bindings

> 📋 **This phase is handled by a script.** The script runs the generator, IMMEDIATELY
> reverts HarfBuzz bindings (HarfBuzz updates are always separate), reports what changed,
> and lists any new functions that may need C# wrappers.

```bash
pwsh .claude/skills/update-skia/scripts/regenerate-bindings.ps1
```

The script handles all of these (so you don't forget any):
- Runs `pwsh ./utils/generate.ps1`
- Reverts `binding/HarfBuzzSharp/HarfBuzzApi.generated.cs` (proactively, not reactively)
- Reports the binding diff summary
- Lists NEW generated functions that may need C# wrappers in Phase 8

After the script completes, build C# to verify compilation:
```bash
dotnet build binding/SkiaSharp/SkiaSharp.csproj
```

### Phase 8: Fix C# Wrappers

The C# build can pass with 0 errors while new C API functions remain invisible to users.
New functions compile fine as unused `internal static` methods in the generated file, but
without C# wrappers they're not part of the public API. This phase applies even when
the build succeeds.

**Step 1: Review new generated bindings for unwrapped functions:**
```bash
# Show only NEW functions added by the regeneration
git diff binding/SkiaSharp/SkiaApi.generated.cs | grep "^+.*internal static"
```

For each new function, check whether a C# wrapper exists:
```bash
# Example: if sk_foo_bar was added, check for a wrapper
grep -rn "sk_foo_bar" binding/SkiaSharp/*.cs | grep -v generated
```
New functions from our custom C API additions typically need wrappers.
New functions from upstream changes are usually additive and can be deferred.

**Step 2:** Fix files in `binding/SkiaSharp/` based on the breaking change analysis:

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

### Phase 9: Build & Test

```bash
# Build native (this also runs git-sync-deps)
dotnet cake --target=externals-macos --arch=arm64

# Build C#
dotnet build binding/SkiaSharp/SkiaSharp.csproj
```

**Step 1 — Smoke tests (fast gate, ~100ms):**
```bash
dotnet test tests/SkiaSharp.Tests.Console.sln --filter "Category=Smoke"
```
Smoke tests verify basic native interop: version compatibility, object creation, drawing,
image loading, fonts, codecs, effects, and more. If these fail, something fundamental is
broken — go back and fix before wasting time on the full suite.

> ⚠️ If the version compatibility smoke test fails with "incompatible native library",
> you missed a version update — go back to Phase 6 and verify ALL version lines.
> Do NOT work around this with `--no-incremental` or by copying native libs manually.

**Step 2 — Full test suite (required before any PR):**
```bash
dotnet test tests/SkiaSharp.Tests.Console.sln
```
This runs all test projects (core, Vulkan, Direct3D). Backend-specific tests
self-skip when hardware isn't available. CI handles WASM/Android/iOS separately.

Smoke tests are just that — smoke. They verify the basics. The full suite MUST pass
before the update can be considered complete. Do not create PRs with only smoke tests passing.

> 🛑 **GATE**: ALL tests pass (full suite, not just smoke). Do NOT skip failing tests.
> Do NOT proceed with failures.

### Phase 10: Create PRs

> **🛑 STOP AND ASK FOR APPROVAL** before creating PRs.

#### PR 1: mono/skia (submodule)

| Field | Value |
|-------|-------|
| Branch | `dev/update-skia-{TARGET}` |
| Target | `skiasharp` |
| Title | `Update skia to milestone {TARGET}` |

#### PR 2: mono/SkiaSharp (parent)

| Field | Value |
|-------|-------|
| Branch | `dev/update-skia-{TARGET}` |
| Target | `main` |
| Title | `Bump skia to milestone {TARGET} (#ISSUE)` |

**Submodule must point to the mono/skia PR branch.**

#### Cross-link both PRs (same as native-dependency-update).

After creating BOTH PRs, update the earlier PR's description to include a link to the later one.
Both PRs must reference each other.

#### Phase 10 Completion Checklist

Before proceeding to merge, verify ALL of these:

- [ ] Branch names follow `dev/update-skia-{TARGET}` convention in BOTH repos
- [ ] mono/skia PR targets `skiasharp` branch
- [ ] mono/SkiaSharp PR targets `main` branch
- [ ] **SkiaSharp's `externals/skia` submodule points to the mono/skia PR branch** (`git submodule status`)
- [ ] `cgmanifest.json` updated with new commit hash, version, and chrome_milestone
- [ ] `scripts/VERSIONS.txt` updated (ALL version lines, not just milestone)
- [ ] `SkiaApi.generated.cs` regenerated and committed
- [ ] Both PRs cross-reference each other
- [ ] Native build passes on at least one platform
- [ ] C# build passes with 0 errors
- [ ] All tests pass (with proper platform filtering)

#### Merge Sequence (CRITICAL)

1. Merge mono/skia PR first → creates new squashed SHA on `skiasharp`
2. Fetch new SHA in SkiaSharp's submodule
3. Update submodule pointer, push to SkiaSharp PR branch
4. **Only then** merge SkiaSharp PR

#### Merge Checklist

Before proceeding past each step, verify:

- [ ] mono/skia PR merged
- [ ] Fetched `skiasharp` branch to get new squashed SHA
- [ ] Updated SkiaSharp submodule to new SHA (`cd externals/skia && git fetch origin && git checkout {new-sha}`)
- [ ] Pushed submodule update to SkiaSharp PR branch
- [ ] CI passes on updated SkiaSharp PR
- [ ] SkiaSharp PR merged
- [ ] **Submodule points to a commit on `skiasharp` branch** (not an orphaned branch commit)

> ❌ **NEVER** merge both PRs without updating the submodule in between.
> ❌ **NEVER** assume the submodule reference is correct after squash-merging mono/skia.

---

## Reference Material

These files contain lookup information — consult them when you hit a problem or need context,
not necessarily upfront:

- **[references/known-gotchas.md](references/known-gotchas.md)** — 10 hard-won lessons from past updates (DEF_STRUCT_MAP, emsdk, BUILD.gn flags, HarfBuzz, DEPS forks, etc.) and a troubleshooting table
- **[references/typical-changes.md](references/typical-changes.md)** — Files typically changed in each repository during an update
- **[references/breaking-changes-checklist.md](references/breaking-changes-checklist.md)** — How to analyze breaking changes between milestones
