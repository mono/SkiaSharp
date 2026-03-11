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

Complete ALL phases in strict order. Do not skip phases.

### Pre-Flight Checklist

Before starting, confirm:
- [ ] Complete Phases 1–10 in order
- [ ] Research ALL breaking changes before writing any code
- [ ] Build and test locally before any PR
- [ ] Create PRs (never push directly to `skiasharp` or `main`)
- [ ] Stop and ask at every 🛑 checkpoint

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

Have a **separate agent** validate the breaking change analysis:
- Cross-check each identified change against the actual upstream diff
- Verify no HIGH-risk changes were missed
- Confirm Graphite-only changes were correctly filtered out

> 🛑 **GATE**: Validation agent confirms analysis is complete and accurate.

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

> 🛑 **GATE**: Merge complete, conflicts resolved. C API files intact. New source files accounted for.

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
   - `SK_C_INCREMENT` may need bumping if C API signature changes

4. **Build again** — iterate until clean compilation

> 🛑 **GATE**: Native library builds successfully on at least one platform.

### Phase 6: Update SkiaSharp Version Files

In the **SkiaSharp parent repo**:

1. **`scripts/VERSIONS.txt`** — Update ALL of these:
   ```
   skia                    release     m{TARGET}
   libSkiaSharp            milestone   {TARGET}
   libSkiaSharp            increment   0          # Reset to 0 for new milestone
   libSkiaSharp            soname      {TARGET}.0.0
   SkiaSharp               assembly    3.{TARGET}.0.0
   SkiaSharp               file        3.{TARGET}.0.0
   # ALL nuget versions → 3.{TARGET}.0
   ```

2. **`cgmanifest.json`** — Update Skia entries:
   - `commitHash`: New submodule commit
   - `version`: `chrome/m{TARGET}`
   - `chrome_milestone`: {TARGET}
   - `upstream_merge_commit`: The upstream chrome/m{TARGET} branch tip

3. **`scripts/azure-pipelines-variables.yml`** — Update milestone references

### Phase 7: Regenerate Bindings

```bash
pwsh ./utils/generate.ps1
```

This regenerates `binding/SkiaSharp/SkiaApi.generated.cs` from the C API headers.

**After regeneration, check for C# compilation:**
```bash
dotnet build binding/SkiaSharp/SkiaSharp.csproj
```

### Phase 8: Fix C# Wrappers

Fix files in `binding/SkiaSharp/` based on the breaking change analysis:

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
# Build native
dotnet cake --target=externals-macos --arch=arm64

# Build C# (use --no-incremental after VERSIONS.txt changes)
dotnet build binding/SkiaSharp/SkiaSharp.csproj --no-incremental

# Run tests (prefer Cake for proper skip trait handling)
dotnet cake --target=tests-netcore --skipExternals=all
# OR if using dotnet test directly, MUST filter platform-specific tests:
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj --filter "SkipOn!=macOS"
```

> ⚠️ **Test runner caveat**: `SkipOn` traits only work with MAUI device test runners.
> The console test runner does NOT filter by traits automatically. Use `--filter "SkipOn!=macOS"`
> (or `!=Linux`, etc.) to avoid test host crashes from exceptions thrown through native callbacks.

> 🛑 **GATE**: All tests pass. Do NOT skip failing tests. Do NOT proceed with failures.

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

## Multi-Milestone Jumps

When jumping more than one milestone (e.g., m119 → m121):

**Option A: Sequential merges** (RECOMMENDED for first time)
- Merge m120 first, fix C API, verify build
- Then merge m121, fix C API, verify build
- Single PR at the end with all changes

**Option B: Direct jump** (faster, experienced only)
- Merge directly to target milestone
- Higher risk of complex merge conflicts
- Harder to identify which milestone broke what

Always document which milestones were crossed in the PR description.

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

### 5. Version Compatibility After `VERSIONS.txt` Update

The C# `SkiaSharpVersion` class auto-generates version constants from build targets. After updating `VERSIONS.txt`, you must rebuild with `--no-incremental` to pick up new version numbers. Without this, you'll get `InvalidOperationException: The version of the native libSkiaSharp library (X) is incompatible` at runtime.

### 6. Test Runner Filtering for Platform-Specific Tests

`SkipOn` traits only work with MAUI device test runners, not the console test runner. For console tests, use `dotnet test --filter "SkipOn!=macOS"` (or the relevant platform) to skip platform-specific tests that would otherwise crash the test host process.

### 7. HarfBuzz Binding Generation Failures

HarfBuzz generated bindings may fail due to system header issues (`inttypes.h` not found). This is independent of SkiaSharp bindings — if it happens, restore the file from git with `git checkout -- binding/HarfBuzzSharp/HarfBuzzApi.generated.cs` and continue. SkiaSharp bindings generate independently.

### 8. New C API Functions From Upstream

Upstream may add new C API functions (e.g., `sk_surface_draw_with_sampling`) that weren't in the previous milestone. The regeneration step (`pwsh ./utils/generate.ps1`) picks these up automatically. Always review the diff of `*.generated.cs` files for new functions that may need corresponding C# wrappers in `binding/SkiaSharp/`.

---

## Troubleshooting

| Error | Cause | Fix |
|-------|-------|-----|
| `EntryPointNotFoundException` | Native lib not rebuilt after C API change | `dotnet cake --target=externals-{platform}` |
| `error CS0246` missing type | Binding not regenerated | `pwsh ./utils/generate.ps1` |
| Merge conflict in DEPS | Both forks updated deps independently | Keep our DEPS pins, accept upstream structure |
| `LNK2001 unresolved external` | C function name mismatch | Verify C API function names match exactly |
| Build fails after merge | Missing `#include` for moved headers | Check upstream header relocation notes |
