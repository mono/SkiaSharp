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
| mono/skia | `DEPS` | Merge conflict resolution — restore custom dependency hashes |
| mono/skia | `include/core/SkMilestone.h` | New milestone number (from upstream) |
| mono/skia | `include/c/sk_types.h` | Enum/type updates |
| mono/skia | `include/c/sk_typeface.h` | Remove dead factory declarations |
| mono/skia | `include/c/skresources_resource_provider.h` | New function declarations (e.g. enum-based) |
| mono/skia | `src/c/sk_typeface.cpp` | Platform font manager, typeface C API |
| mono/skia | `src/c/sk_font.cpp` | Null typeface → default interception |
| mono/skia | `src/c/gr_context.cpp` | Ganesh namespace migrations |
| mono/skia | `src/c/sk_image.cpp` | Mipmapped enum updates |
| mono/skia | `src/c/sk_types_priv.h` | Type mapping macro fixes (DEF_MAP) |
| mono/skia | `src/c/sk_enums.cpp` | Enum mapping updates |
| mono/skia | `src/c/skresources_resource_provider.cpp` | Bool→enum wrapper functions |
| mono/skia | `src/xamarin/SkCompatPaint.cpp` | Default typeface for internal SkFont |
| mono/SkiaSharp | `externals/skia` | Submodule pointer |
| mono/SkiaSharp | `scripts/VERSIONS.txt` | All version numbers |
| mono/SkiaSharp | `cgmanifest.json` | Security tracking |
| mono/SkiaSharp | `scripts/azure-pipelines-variables.yml` | CI config |
| mono/SkiaSharp | `binding/SkiaSharp/SkiaApi.generated.cs` | Regenerated |
| mono/SkiaSharp | `binding/SkiaSharp/SKTypeface.cs` | [Obsolete] redirects to SKFontManager |
| mono/SkiaSharp | `binding/SkiaSharp/Definitions.cs` | Type definitions |
| mono/SkiaSharp | `binding/SkiaSharp/EnumMappings.cs` | Enum mappings |
| mono/SkiaSharp | `binding/SkiaSharp.Resources/ResourceProvider.cs` | New enum overloads, [Obsolete] on bool |
| mono/SkiaSharp | `binding/SkiaSharp.Resources/ResourcesApi.generated.cs` | Regenerated |
| mono/SkiaSharp | `binding/libSkiaSharp.json` | Remove dead type mappings |
| mono/SkiaSharp | `tests/Tests/SkiaSharp/*.cs` | Test updates for changed behavior |

---

## Lessons Learned / Known Gotchas

Hard-won findings from past Skia milestone updates. Check these proactively — they will save hours of debugging.

### 1. `DEF_STRUCT_MAP` vs Type Aliases

When upstream changes a C++ type from a `struct` to a `using` alias (e.g., `GrVkYcbcrConversionInfo` → `using VulkanYcbcrConversionInfo`), the `DEF_STRUCT_MAP` macro in `sk_types_priv.h` forward-declares `struct X`, which conflicts with the alias. Fix by switching to `DEF_MAP` (not `DEF_STRUCT_MAP`) and wrapping in the appropriate platform guard (e.g., `#if defined(SK_VULKAN)`).

```cpp
// ❌ WRONG — DEF_STRUCT_MAP forward-declares struct, conflicts with using alias
DEF_STRUCT_MAP(GrVkYcbcrConversionInfo, gr_vk_ycbcrconversioninfo_t, GrVkYcbcrConversionInfo)

// ✅ CORRECT — DEF_MAP with platform guard
#if defined(SK_VULKAN)
DEF_MAP(GrVkYcbcrConversionInfo, gr_vk_ycbcrconversioninfo_t, GrVkYcbcrConversionInfo)
#endif
```

### 2. `git-sync-deps` emsdk Failure

Upstream m121+ added an `activate-emsdk` call in `tools/git-sync-deps`. Since SkiaSharp comments out emsdk in DEPS, this call fails. Set the environment variable `GIT_SYNC_DEPS_SKIP_EMSDK=1` in `scripts/cake/native-shared.cake` to prevent build failures during dependency sync.

### 3. `BUILD.gn` Legacy Flags

Upstream may introduce defines like `SK_DEFAULT_TYPEFACE_IS_EMPTY` and `SK_DISABLE_LEGACY_DEFAULT_TYPEFACE` that break SkiaSharp's C API, which still relies on legacy typeface/fontmgr APIs. Comment these defines out in `BUILD.gn` when they cause compilation errors in the C API shim layer.

### 4. Custom Patches: No More Upstream Header Modifications

**Policy (established during m122 update):** Do NOT add custom methods to upstream Skia headers
(`include/core/SkTypeface.h`, `include/core/SkFontMgr.h`, etc.). Previous milestones added
`SkTypeface::RefDefault()`, `SkTypeface::UniqueID()`, `SkFontMgr::MakeDefault()` to upstream
headers — these invariably break on merge because upstream modifies the same files.

**Instead:** Implement all custom behavior in the C API layer (`src/c/`) using platform-specific
`#ifdef` blocks. For example, the default font manager singleton is implemented entirely in
`sk_typeface.cpp` using `create_platform_fontmgr()` — not by patching upstream code.

After any upstream merge, **always verify** that no stale custom declarations leaked back into
upstream headers. Search for: `RefDefault`, `UniqueID`, `MakeDefault` in `include/core/`.

### 5. Version Compatibility After `VERSIONS.txt` Update

The C# `SkiaSharpVersion` class auto-generates version constants from build targets. After updating `VERSIONS.txt`, you must rebuild with `--no-incremental` to pick up new version numbers. Without this, you'll get `InvalidOperationException: The version of the native libSkiaSharp library (X) is incompatible` at runtime.

### 6. Test Runner Filtering for Platform-Specific Tests

`SkipOn` traits only work with MAUI device test runners, not the console test runner. For console tests, use `dotnet test --filter "SkipOn!=macOS"` (or the relevant platform) to skip platform-specific tests that would otherwise crash the test host process.

### 7. HarfBuzz Binding Generation Failures

HarfBuzz generated bindings may fail due to system header issues (`inttypes.h` not found). This is independent of SkiaSharp bindings — if it happens, restore the file from git with `git checkout -- binding/HarfBuzzSharp/HarfBuzzApi.generated.cs` and continue. SkiaSharp bindings generate independently.

### 8. New C API Functions From Upstream

Upstream may add new C API functions (e.g., `sk_surface_draw_with_sampling`) that weren't in the previous milestone. The regeneration step (`pwsh ./utils/generate.ps1`) picks these up automatically. Always review the diff of `*.generated.cs` files for new functions that may need corresponding C# wrappers in `binding/SkiaSharp/`.

### 9. C API Has NO ABI Compatibility — Remove Dead Code

The C API (`src/c/`, `include/c/`) has **NO ABI compatibility** guarantee. If upstream removes a
C++ function that a C API function wraps, **delete the C API function entirely**. Do NOT stub it
with a `return nullptr` or `return false` — dead stubs are useless and confusing. The C# side
should redirect to the replacement API using `[Obsolete]` annotations and then call the new
underlying function.

```csharp
// ✅ CORRECT — redirect in C#, not a dead C stub
[Obsolete("Use SKFontManager.Default.CreateTypeface(data, index) instead.")]
public static SKTypeface FromData(SKData data, int index = 0)
{
    if (data == null) throw new ArgumentNullException(nameof(data));
    return SKFontManager.Default.CreateTypeface(data, index);
}
```

### 10. SkFont Null Typeface = Empty Typeface (m122+)

Starting in m122, `SkFont(nullptr)` creates a font with the "empty" typeface that **draws nothing**.
This breaks all text rendering when no explicit typeface is set. The fix is in the C API layer:

- **`sk_font.cpp`**: Intercept null typeface in `sk_font_new`, `sk_font_new_with_values`, and
  `sk_font_set_typeface` — substitute the platform default typeface.
- **`SkCompatPaint.cpp`** (`src/xamarin/`): Also has its own internal `SkFont` member that must be
  initialized with the default typeface.
- **`sk_font_get_typeface()`**: Must compare against `SkTypeface::MakeEmpty()` singleton and return
  null to C# when the font has the empty typeface (preserving the C# expectation that no-typeface = null).

Both `sk_font.cpp` and `SkCompatPaint.cpp` need an `extern` declaration for the shared singleton
accessor:

```cpp
// At the top of sk_font.cpp and SkCompatPaint.cpp:
extern sk_sp<SkFontMgr> skiasharp_ref_default_fontmgr();

static sk_sp<SkTypeface> get_default_typeface() {
    auto fontmgr = skiasharp_ref_default_fontmgr();
    return fontmgr->matchFamilyStyle(nullptr, SkFontStyle::Normal());
}
```

### 11. Platform-Specific Font Manager Selection

The C API implements a platform default font manager in `sk_typeface.cpp` via
`create_platform_fontmgr()`. This is a critical function — getting it wrong means text rendering
breaks on that platform. Here is the COMPLETE platform mapping:

```cpp
static sk_sp<SkFontMgr> create_platform_fontmgr() {
#if defined(SK_BUILD_FOR_MAC) || defined(SK_BUILD_FOR_IOS)
    return SkFontMgr_New_CoreText(nullptr);
#elif defined(SK_BUILD_FOR_WIN)
    return SkFontMgr_New_DirectWrite();
#elif defined(SK_BUILD_FOR_ANDROID)
    return SkFontMgr_New_Android(nullptr);
#elif defined(SK_FONTMGR_FONTCONFIG_AVAILABLE)
    return SkFontMgr_New_FontConfig(nullptr);
#elif defined(SK_FONTMGR_FREETYPE_EMBEDDED_AVAILABLE)
    return SkFontMgr_New_Custom_Embedded(&SK_EMBEDDED_FONTS);
#else
    return SkFontMgr::RefEmpty();
#endif
}
```

#### ⚠️ WASM Font Manager — Critical Details

WASM uses the **embedded** font manager (`SK_FONTMGR_FREETYPE_EMBEDDED_AVAILABLE`), NOT the
empty one. The WASM build has specific font manager settings in `native/wasm/build.cake`:

```
skia_enable_fontmgr_custom_directory=false
skia_enable_fontmgr_custom_empty=false
skia_enable_fontmgr_custom_embedded=true   ← THIS ONE
skia_enable_fontmgr_empty=false
```

The WASM build also embeds **NotoMono-Regular.ttf** into the binary as a fallback font:

```bash
# In native/wasm/build.cake (~line 92):
embed_resources.py --name SK_EMBEDDED_FONTS \
  --input modules/canvaskit/fonts/NotoMono-Regular.ttf \
  --output NotoMono-Regular.ttf.cpp --align 4
```

This generates an `extern "C" const SkEmbeddedResourceHeader SK_EMBEDDED_FONTS` symbol.

**Why each alternative is WRONG for WASM:**

| Alternative | Problem |
|------------|---------|
| `SkFontMgr::RefEmpty()` | `onMakeFromData()` returns nullptr — can't create typefaces from font data AT ALL |
| `SkFontMgr_New_Custom_Data({})` | No built-in fonts — text draws nothing unless fonts explicitly registered first |
| `SkFontMgr_New_Custom_Empty()` | Not linked — `skia_enable_fontmgr_custom_empty=false` in WASM build |

**`SkFontMgr_New_Custom_Embedded(&SK_EMBEDDED_FONTS)` is the ONLY correct choice** because it:
1. Includes NotoMono as a fallback font (text renders without explicit font registration)
2. Supports `onMakeFromData()` / `onMakeFromStreamIndex()` via FreeType (runtime font loading works)
3. Is actually linked in the WASM build

Since `SkFontMgr_New_Custom_Embedded` has no public header, it requires forward declarations
in `sk_typeface.cpp`:

```cpp
#elif defined(SK_FONTMGR_FREETYPE_EMBEDDED_AVAILABLE)
struct SkEmbeddedResource { const uint8_t* data; size_t size; };
struct SkEmbeddedResourceHeader { const SkEmbeddedResource* entries; int count; };
extern "C" const SkEmbeddedResourceHeader SK_EMBEDDED_FONTS;
SK_API sk_sp<SkFontMgr> SkFontMgr_New_Custom_Embedded(const SkEmbeddedResourceHeader*);
#endif
```

#### Auditing Platform Font Manager Coverage

When updating `create_platform_fontmgr()`, always verify which font managers are linked per
platform by checking the `native/*/build.cake` files:

```bash
grep -rn "fontmgr" native/*/build.cake
```

Key things to check:
- Which `skia_enable_fontmgr_*` flags are set per platform
- Whether the platform links a custom font manager (embedded, directory, empty)
- Whether the `#ifdef` guards in `create_platform_fontmgr()` match what's actually linked

### 12. Upstream DEPS Reverts Custom Dependency Bumps

When merging upstream, the `DEPS` file will revert any custom dependency version bumps that
SkiaSharp has made (e.g., libwebp, brotli, zlib, libpng, freetype, harfbuzz, libjpeg-turbo,
expat). After resolving DEPS merge conflicts:

1. Check `cgmanifest.json` for the expected dependency versions
2. Verify each custom hash in DEPS matches what SkiaSharp expects
3. Run `python3 tools/git-sync-deps` to ensure all deps fetch correctly

### 13. Ganesh Namespace Migration Pattern

Upstream is gradually moving Ganesh APIs from global `Gr*` prefixes to namespaced functions.
Watch for these patterns during updates:

```cpp
// Before (may break across milestones)
GrDirectContext::MakeGL(...)
GrDirectContext::MakeVulkan(...)
GrMipMapped → GrMipmapped

// After (new namespace)
GrDirectContexts::MakeGL(...)      // include/gpu/ganesh/gl/GrGLDirectContext.h
GrDirectContexts::MakeVulkan(...)  // include/gpu/ganesh/vk/GrVkDirectContext.h
skgpu::Mipmapped                   // include/gpu/GpuTypes.h
```

Update the C API files (`gr_context.cpp`, `sk_image.cpp`) to use the new namespaces and headers.

### 14. SKTypeface Factory Migration (m122+)

In m122, upstream removed direct typeface creation methods from `SkTypeface` (e.g.,
`SkTypeface::MakeFromFile`, `SkTypeface::MakeFromData`). These are now only available via
`SkFontMgr::makeFromFile()`, `SkFontMgr::makeFromData()`, etc.

**C API:** Remove dead factory functions from `sk_typeface.h` / `sk_typeface.cpp`. Also remove
mappings from `binding/libSkiaSharp.json`.

**C# approach:**
- Mark all `SKTypeface.FromFile`, `FromStream`, `FromData`, `FromFamilyName`, `CreateDefault`
  as `[Obsolete("Use SKFontManager.Default.XYZ instead.")]`
- Redirect each method body to `SKFontManager.Default.CreateTypeface(...)` or
  `SKFontManager.Default.MatchFamily(...)`
- The static `SKTypeface.Default` property should use
  `SKFontManager.Default.MatchFamily(null)` instead of the removed `sk_typeface_ref_default()`

**Static initialization order matters:** `SKObject.EnsureStaticInstanceAreInitialized()` calls
`SKFontManager` before `SKTypeface` — so `SKFontManager.Default` is available when
`SKTypeface`'s static constructor runs.

### 15. skresources Bool → Enum Migration Pattern

When upstream changes a `bool` parameter to an `enum` (e.g., `skresources::ImageDecodeStrategy`),
the right approach is:

1. **C API:** Add a real enum to `sk_types.h`, create new `_make2` functions with the enum
   parameter, keep the old bool functions as thin wrappers delegating to the new ones
2. **C#:** Auto-generate the enum via `pwsh ./utils/generate.ps1`, add new constructors using
   the enum, mark bool overloads as `[Obsolete]`
3. **Do NOT** just change the bool to an int — use a proper typed enum

### 16. Static Initialization and Singleton Patterns

When the C API needs a shared singleton (e.g., the default font manager), use a static local
variable in a named function that can be `extern`-declared from other translation units:

```cpp
// In sk_typeface.cpp — the DEFINITION
sk_sp<SkFontMgr> skiasharp_ref_default_fontmgr() {
    static sk_sp<SkFontMgr> singleton = create_platform_fontmgr();
    return singleton;
}

// In sk_font.cpp and SkCompatPaint.cpp — extern DECLARATION
extern sk_sp<SkFontMgr> skiasharp_ref_default_fontmgr();
```

This pattern is thread-safe (C++11 guarantees) and avoids duplication. The singleton is created
on first access and lives for the process lifetime.

---

## Troubleshooting

| Error | Cause | Fix |
|-------|-------|-----|
| `EntryPointNotFoundException` | Native lib not rebuilt after C API change | `dotnet cake --target=externals-{platform}` |
| `error CS0246` missing type | Binding not regenerated | `pwsh ./utils/generate.ps1` |
| Merge conflict in DEPS | Both forks updated deps independently | Keep our DEPS pins, accept upstream structure |
| `LNK2001 unresolved external` | C function name mismatch | Verify C API function names match exactly |
| Build fails after merge | Missing `#include` for moved headers | Check upstream header relocation notes |
| Text draws nothing on WASM | Wrong font manager (RefEmpty/Custom_Data) | Must use `SkFontMgr_New_Custom_Embedded(&SK_EMBEDDED_FONTS)` |
| Text draws nothing everywhere | Null typeface = empty typeface (m122+) | Intercept null in `sk_font.cpp` + `SkCompatPaint.cpp` → substitute default |
| `SkFontMgr_New_Custom_Empty` link error | `skia_enable_fontmgr_custom_empty=false` | Check platform's build.cake for which fontmgrs are linked |
| GC-dependent tests flaky in full suite | GC pressure delays collection | Pre-existing — passes in isolation, not a regression |
