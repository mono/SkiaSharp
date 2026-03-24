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
  - Check what Skia milestone is current

  Triggers: "update skia", "bump skia", "skia milestone", "update to m121",
  "merge upstream skia", "skia update", "new skia version".

  For updating individual dependencies (libpng, zlib, etc.), use `native-dependency-update` instead.
  For security audits, use `security-audit` instead.
---

# Update Skia Milestone Skill

Update Google Skia to a new Chrome milestone in SkiaSharp's mono/skia fork.

## Key References

- **[references/breaking-changes-checklist.md](references/breaking-changes-checklist.md)** — How to analyze breaking changes between milestones
- **[documentation/dependencies.md](../../../documentation/dependencies.md)** — Dependency tracking and cgmanifest.json format
- **[RELEASE_NOTES.md in upstream Skia](https://raw.githubusercontent.com/google/skia/main/RELEASE_NOTES.md)** — Official Skia release notes

## Overview

Updating Skia is the **highest-risk operation** in SkiaSharp. It touches:
- The native C++ Skia library (upstream merge)
- The custom C API shim layer (must be adapted to new C++ APIs)
- Generated P/Invoke bindings
- C# wrapper code
- All platforms (macOS, Windows, Linux, iOS, Android, etc.)

**Go slow. Research first. Build and test before any PR.**

## ⚠️ MANDATORY: Follow Every Phase

**PHASES MUST BE EXECUTED IN STRICT ORDER. NO SKIPPING. NO REORDERING.**

Every phase ends with a verification step or gate. Do not proceed to the next phase until
the current phase's gate criteria are met. **Re-read each phase's instructions before
executing it** — do not rely on memory from an earlier read.

### Why This Matters

In past runs, these specific failures occurred from skipping or partially completing phases:
- Phase 3 skipped → missed breaking changes that caused build failures
- Phase 6 partially done → only milestone line updated, 30+ version lines missed
- Phase 8 skipped → new generated functions left without C# wrappers
- Phase 9 wrong command → test host crashed from unfiltered platform tests

### Pre-Flight Checklist

Before starting, confirm:
- [ ] Complete Phases 1–10 in order
- [ ] Research ALL breaking changes before writing any code
- [ ] Build and test locally before any PR
- [ ] Create PRs (never push directly to `skiasharp` or `main`)
- [ ] Stop and ask at every 🛑 checkpoint
- [ ] Re-read each phase before executing it

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

👉 See [references/breaking-changes-checklist.md](references/breaking-changes-checklist.md) for detailed analysis template.

> 🛑 **GATE**: Present full breaking change analysis to user. Get approval before proceeding.

### Phase 3: Validation

> 🛑 **MANDATORY**: Launch a validation agent. Do NOT proceed to Phase 4 without its output.
> This phase exists because the agent performing the analysis has blind spots — it may
> filter out relevant changes or miss moved headers. An independent check catches these.

Launch an **explore agent** with this prompt (substitute actual milestone numbers):

```
I'm updating SkiaSharp's Skia submodule from m{CURRENT} to m{TARGET}.
Here is the breaking change analysis I produced: [paste analysis table]

Please validate by (run from externals/skia):
1. Run: git diff upstream/chrome/m{CURRENT}..upstream/chrome/m{TARGET} --stat -- src/ include/
   Count the files changed and compare to my analysis — did I miss any?
2. For each HIGH/MEDIUM item I identified, verify the C API impact by grepping src/c/ include/c/
3. Check for changes I may have filtered as "Graphite-only" that actually affect Ganesh
4. Check for removed/moved headers that our C API includes:
   grep -rh '#include' src/c/*.cpp | sort -u
   Then verify each included header still exists at upstream/chrome/m{TARGET}
5. Report: missed items, incorrect classifications, and confirmed items
```

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

2. **Merge upstream**:
   ```bash
   git merge upstream/chrome/m{TARGET}
   ```

3. **Resolve conflicts** — Common conflicts:
   - `DEPS` — Keep our dependency pins, accept upstream structure changes
   - `RELEASE_NOTES.md` — Accept upstream
   - `infra/bots/jobs.json` — Accept upstream
   - Source files in `src/` — Carefully resolve (don't lose our C API)

4. **Verify our C API files survived the merge**:
   ```bash
   ls src/c/*.cpp include/c/*.h  # All files should still exist
   ```

5. **Source file verification** — Check for added/deleted upstream files:
   ```bash
   git diff upstream/chrome/m{CURRENT}..upstream/chrome/m{TARGET} --diff-filter=AD --name-only -- src/ include/
   ```
   Cross-reference against `BUILD.gn` — new source files may need to be added.

> 🛑 **GATE**: Merge complete, conflicts resolved. Run ALL of these verification commands
> (from inside `externals/skia`):
> ```bash
> cd externals/skia
> ls src/c/*.cpp include/c/*.h                    # C API files intact
> git diff upstream/chrome/m{CURRENT}..upstream/chrome/m{TARGET} --diff-filter=AD --name-only -- src/ include/
>                                                   # Review added/deleted files
> git diff --check                                  # Zero conflict markers remain
> ```
> All three must produce expected output before proceeding.

### Phase 5: Fix C API Shim Layer

This is where most of the work happens. The C API (`src/c/`, `include/c/`) wraps Skia C++ and
must be updated when the underlying C++ APIs change.

1. **Attempt to build** to identify all compilation errors:
   ```bash
   cd /path/to/SkiaSharp
   dotnet cake --target=externals-macos --arch=arm64
   ```

2. **Fix each error** following these patterns:

   | Error Type | Fix Pattern |
   |-----------|-------------|
   | Missing type | Add/update typedef in `sk_types.h` |
   | Renamed function | Update call in `*.cpp` |
   | Removed enum value | Remove from `sk_enums.cpp`, update `sk_types.h` |
   | Changed signature | Update C wrapper function signature |
   | New header required | Add `#include` in the relevant `.cpp` |

3. **Update `sk_types.h`** for any new enums or type changes:
   - **Reset `SK_C_INCREMENT` to `0`** in `externals/skia/include/c/sk_types.h` for the new milestone
   - Only bump it later if you add new C API functions in the same milestone
   - The build enforces that `SK_C_INCREMENT` matches `libSkiaSharp increment` in `VERSIONS.txt`

4. **Build again** — iterate until clean compilation

> 🛑 **GATE**: Native library builds successfully on at least one platform.

### Phase 6: Update SkiaSharp Version Files

> 📋 **RE-READ this phase carefully.** In past runs, only the milestone line was updated
> and 30+ other version lines were missed. This phase has MANY lines to change.

In the **SkiaSharp parent repo**:

1. **`scripts/VERSIONS.txt`** — Update **ALL** of these (not just milestone):
   ```
   skia                    release     m{TARGET}        ← was m{CURRENT}
   libSkiaSharp            milestone   {TARGET}         ← was {CURRENT}
   libSkiaSharp            increment   0                ← RESET to 0
   libSkiaSharp            soname      {TARGET}.0.0     ← was {CURRENT}.x.x
   SkiaSharp               assembly    3.{TARGET}.0.0   ← was 3.{CURRENT}.x.x
   SkiaSharp               file        3.{TARGET}.0.0   ← was 3.{CURRENT}.x.x
   # EVERY nuget line with 3.{CURRENT}.x → 3.{TARGET}.0
   # (there are ~30 nuget lines — use sed, not manual editing)
   ```

   **Recommended approach — use sed for bulk update (replace {CURRENT}/{TARGET} with actual numbers):**
   ```bash
   # Example for m119 → m120 (adjust numbers for your update):
   # Update all nuget versions (e.g., 3.119.4 → 3.120.0)
   sed -i '' 's/3\.119\.[0-9]*/3.120.0/g' scripts/VERSIONS.txt
   # Update soname
   sed -i '' 's/119\.0\.0/120.0.0/' scripts/VERSIONS.txt
   # Update assembly/file versions  
   sed -i '' 's/3\.119\.[0-9]*\.0/3.120.0.0/g' scripts/VERSIONS.txt
   # Update skia release
   sed -i '' 's/m119/m120/' scripts/VERSIONS.txt
   ```

2. **`cgmanifest.json`** — Update **ALL** Skia entries (there are multiple sections):
   - `commitHash`: New submodule commit (from `cd externals/skia && git rev-parse HEAD`)
   - `version`: `chrome/m{TARGET}`
   - `chrome_milestone`: {TARGET}
   - `upstream_merge_commit`: The upstream chrome/m{TARGET} branch tip

3. **`scripts/azure-pipelines-variables.yml`** — Update milestone references (if file exists)

**Verification (MANDATORY — catches the most common mistake):**
```bash
# MUST return 0 non-comment lines. If any remain, you missed something.
grep -n "{CURRENT}" scripts/VERSIONS.txt | grep -v "^#\|HarfBuzz"
grep -n "m{CURRENT}\|{CURRENT}\." cgmanifest.json
# Verify SK_C_INCREMENT is 0 in the native header
grep "SK_C_INCREMENT" externals/skia/include/c/sk_types.h
# Should show: #define SK_C_INCREMENT 0
```
> 🛑 **GATE**: Version greps return zero results (excluding HarfBuzz). `SK_C_INCREMENT` is `0`.
> If ANY lines still contain the old milestone, fix them.

### Phase 7: Regenerate Bindings

```bash
pwsh ./utils/generate.ps1
```

This regenerates `binding/SkiaSharp/SkiaApi.generated.cs` from the C API headers.

**After regeneration, review the diff and check for C# compilation:**
```bash
# Review what changed — look for NEW functions that may need C# wrappers
git diff --stat binding/
git diff binding/SkiaSharp/SkiaApi.generated.cs | head -100

# ALWAYS revert HarfBuzz generated bindings — HarfBuzz updates are separate from Skia updates.
# The generator picks up API changes that need hand-written delegate proxies.
git checkout HEAD -- binding/HarfBuzzSharp/HarfBuzzApi.generated.cs

dotnet build binding/SkiaSharp/SkiaSharp.csproj
```

### Phase 8: Fix C# Wrappers

> 📋 **RE-READ**: This phase applies even when the C# build passes with 0 errors.
> New generated functions may need C# wrappers even though the build succeeds.

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

## Files Changed in a Typical Update

| Repository | File | Change |
|-----------|------|--------|
| mono/skia | `DEPS` | Merge conflict resolution |
| mono/skia | `include/core/SkMilestone.h` | New milestone number (from upstream) |
| mono/skia | `include/c/sk_types.h` | Enum/type updates |
| mono/skia | `src/c/*.cpp` | C API fixes for new C++ APIs |
| mono/skia | `src/c/sk_enums.cpp` | Enum mapping updates |
| mono/SkiaSharp | `externals/skia` | Submodule pointer |
| mono/SkiaSharp | `scripts/VERSIONS.txt` | All version numbers |
| mono/SkiaSharp | `cgmanifest.json` | Security tracking |
| mono/SkiaSharp | `scripts/azure-pipelines-variables.yml` | CI config |
| mono/SkiaSharp | `binding/SkiaSharp/SkiaApi.generated.cs` | Regenerated |
| mono/SkiaSharp | `binding/SkiaSharp/Definitions.cs` | Type definitions |
| mono/SkiaSharp | `binding/SkiaSharp/EnumMappings.cs` | Enum mappings |
| mono/SkiaSharp | `binding/libSkiaSharp.json` | Type config |
| mono/SkiaSharp | `tests/Tests/SkiaSharp/*.cs` | Test updates |

---

## Lessons Learned / Known Gotchas

Hard-won findings from past Skia milestone updates. Check these proactively — they will save hours of debugging.

### 1. `DEF_STRUCT_MAP` vs Type Aliases

When upstream changes a C++ type from a `struct` to a `using` alias (e.g., `GrVkYcbcrConversionInfo` → `using VulkanYcbcrConversionInfo`), the `DEF_STRUCT_MAP` macro in `sk_types_priv.h` forward-declares `struct X`, which conflicts with the alias. Fix by switching to `DEF_MAP_WITH_NS(namespace, ActualType, CType)` and wrapping in the appropriate platform guard (e.g., `#if SK_VULKAN`).

### 2. `git-sync-deps` emsdk Failure

Upstream m121+ added an `activate-emsdk` call in `tools/git-sync-deps`. Since SkiaSharp comments out emsdk in DEPS, this call fails. Set the environment variable `GIT_SYNC_DEPS_SKIP_EMSDK=1` in `scripts/cake/native-shared.cake` to prevent build failures during dependency sync.

### 3. `BUILD.gn` Legacy Flags

Upstream may introduce defines like `SK_DEFAULT_TYPEFACE_IS_EMPTY` and `SK_DISABLE_LEGACY_DEFAULT_TYPEFACE` that break SkiaSharp's C API, which still relies on legacy typeface/fontmgr APIs. Comment these defines out in `BUILD.gn` when they cause compilation errors in the C API shim layer.

### 4. Custom Patches May Partially Survive Merges

SkiaSharp adds custom methods to upstream headers (e.g., `SkTypeface::RefDefault()`, `SkTypeface::UniqueID()`, `SkFontMgr::MakeDefault()`). After an upstream merge, implementations in `.cpp` files may survive but header declarations can be silently removed by upstream changes. Always verify that header declarations in `include/` still match the implementations in `src/`.

### 5. Version Compatibility Errors Mean You Missed a Step

If you get `InvalidOperationException: The version of the native libSkiaSharp library (X) is incompatible`, this means the native milestone and C# expected milestone don't match. This is always caused by an incomplete Phase 6 (VERSIONS.txt not fully updated) or a stale build. Go back and fix the root cause — do NOT work around it with `--no-incremental` or by manually copying native libraries.

### 6. Test Runner

Tests use runtime `Skip.If()` calls to self-skip on unsupported platforms. Run all tests
with `dotnet test tests/SkiaSharp.Tests.Console.sln` — this runs core, Vulkan, and Direct3D
test projects. Backend-specific tests self-skip when hardware isn't available. CI handles
WASM, Android, and iOS testing separately.

### 7. HarfBuzz Binding Generation Failures

HarfBuzz generated bindings may fail due to system header issues (`inttypes.h` not found). This is independent of SkiaSharp bindings — if it happens, restore the file from git with `git checkout -- binding/HarfBuzzSharp/HarfBuzzApi.generated.cs` and continue. SkiaSharp bindings generate independently.

### 8. New C API Functions From Upstream

Upstream may add new C API functions (e.g., `sk_surface_draw_with_sampling`) that weren't in the previous milestone. The regeneration step (`pwsh ./utils/generate.ps1`) picks these up automatically. Always review the diff of `*.generated.cs` files for new functions that may need corresponding C# wrappers in `binding/SkiaSharp/`.

### 9. DEPS: Fork-Customized Dependencies

SkiaSharp's fork often has **newer** dependency versions than upstream Skia (from custom security/bug-fix updates via the `native-dependency-update` skill). When merging upstream and resolving DEPS, do NOT blindly update all hashes to the upstream milestone's versions — you may **downgrade** dependencies and break the build.

**Check the skiasharp branch commit log** for custom dependency updates:
```bash
git log --oneline skiasharp | grep -i "update\|bump\|libpng\|zlib\|expat\|brotli\|webp\|harfbuzz\|vulkan"
```

For each dep with a custom update, **keep the fork's hash**. Only update deps that the fork hasn't customized. Common fork-customized deps: libwebp, brotli, expat, libpng, zlib, vulkanmemoryallocator, **harfbuzz**.

> ⚠️ **HarfBuzz is ALWAYS a fork-customized dep.** HarfBuzz updates require hand-written C# delegate
> proxies and must be done as a separate task via the `native-dependency-update` skill. During a Skia
> milestone update, ALWAYS keep the fork's harfbuzz hash in DEPS and ALWAYS revert any changes to
> `binding/HarfBuzzSharp/HarfBuzzApi.generated.cs`.

**Symptoms of getting this wrong**: Build failures referencing missing source files (e.g., `palette.c` in libwebp), or HarfBuzz generated binding errors from a version mismatch.

### 10. HarfBuzz Generated Bindings — ALWAYS Revert

HarfBuzz updates are **always separate** from Skia milestone updates. When the harfbuzz DEPS version changes (even accidentally during a merge), the code generator picks up new APIs (paint/draw/colorline callbacks) that require hand-written delegate proxy implementations in `binding/HarfBuzzSharp/DelegateProxies.*.cs`. This causes `CS8795` errors for missing partial method implementations.

**During a Skia milestone update, ALWAYS:**
1. Keep the fork's harfbuzz hash in DEPS (do not accept upstream's version)
2. Revert any generated HarfBuzz binding changes: `git checkout HEAD -- binding/HarfBuzzSharp/HarfBuzzApi.generated.cs`

HarfBuzz version bumps should be done separately via the `native-dependency-update` skill, which includes writing the required delegate proxies.

---

## Troubleshooting

| Error | Cause | Fix |
|-------|-------|-----|
| `EntryPointNotFoundException` | Native lib not rebuilt after C API change | `dotnet cake --target=externals-{platform}` |
| `error CS0246` missing type | Binding not regenerated | `pwsh ./utils/generate.ps1` |
| Merge conflict in DEPS | Both forks updated deps independently | Keep our DEPS pins, accept upstream structure |
| `LNK2001 unresolved external` | C function name mismatch | Verify C API function names match exactly |
| Build fails after merge | Missing `#include` for moved headers | Check upstream header relocation notes |
