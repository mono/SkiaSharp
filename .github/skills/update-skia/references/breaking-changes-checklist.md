# Breaking Changes Checklist for Skia Milestone Updates

## How to Use This Document

When updating Skia to a new milestone, use this checklist to systematically identify
and categorize all breaking changes that affect SkiaSharp.

## Step 1: Gather Release Notes

For each milestone between current and target:

```bash
# Fetch and read the official release notes
curl -s https://raw.githubusercontent.com/google/skia/main/RELEASE_NOTES.md | \
  sed -n '/^Milestone {N}$/,/^\* \* \*$/p'
```

## Step 2: Filter by Relevance

SkiaSharp uses **Ganesh** (not Graphite). Filter changes:

| Prefix/Keyword | Relevant? | Notes |
|----------------|-----------|-------|
| `skgpu::graphite::` | ❌ No | Graphite backend — skip |
| `GrDirectContext`, `Gr*` | ✅ Yes | Ganesh backend — always check |
| `SkImage`, `SkSurface`, `SkCanvas` | ✅ Yes | Core APIs — always check |
| `SkTypeface`, `SkFont` | ✅ Yes | Text/font APIs — always check |
| `SkPath`, `SkPaint` | ✅ Yes | Drawing APIs — always check |
| `Dawn*`, `wgpu::` | ❌ No | Dawn/WebGPU — skip |
| `SkSL`, `SkRuntimeEffect` | ⚠️ Maybe | Only if C API exposes runtime effects |
| `SkCodec` | ⚠️ Maybe | Only if C API exposes codec APIs |
| `include/gpu/GpuTypes.h`, `skgpu::` (non-graphite) | ⚠️ Check | Shared GPU types — trace consumers in **both** Ganesh and Graphite before skipping |
| `SkEncoder::Options` structs | ✅ Yes | Size changes break `static_assert` in `sk_structs.cpp` |

> ⚠️ **Shared GPU headers:** Types in `include/gpu/GpuTypes.h` and `include/gpu/` (outside
> `ganesh/` and `graphite/`) are shared between backends. Before classifying as "Graphite-only",
> grep for consumers in `include/gpu/ganesh/` — e.g., `GrFlushInfo` in `GrTypes.h` uses types
> from `GpuTypes.h`.

## Step 3: Categorize Each Change

Create a table for each change:

```markdown
| # | Change | Risk | C API Impact | C# Impact | Action |
|---|--------|------|-------------|-----------|--------|
| 1 | `GrMipmapped` removed | 🔴 HIGH | sk_types.h enum | GRDefinitions.cs | Update enum + mapping |
| 2 | `SkFont::refTypefaceOrDefault` removed | 🟡 MED | None (not wrapped) | None | No action needed |
| 3 | New `SkVertices::Builder` SK_API | 🟢 LOW | None | None | Optional: wrap later |
```

## Step 4: C API Impact Analysis

For each HIGH/MEDIUM risk change, check the C API:

```bash
cd externals/skia

# Search for affected symbols in C API
grep -rn "SYMBOL_NAME" src/c/ include/c/

# Show the C API file that wraps the affected C++ class
# Example: For SkImage changes, check sk_image.cpp
cat src/c/sk_image.cpp | grep -A5 "FUNCTION_NAME"
```

### Step 4a: Verify struct size assertions (MANDATORY)

Every milestone update MUST check `sk_structs.cpp` for `static_assert(sizeof(...))` lines.
For each asserted struct, compare the C API struct (in `sk_types.h`) against the C++ struct
in the **target milestone** — not the current branch:

```bash
# List all size-asserted structs
grep "static_assert.*sizeof" src/c/sk_structs.cpp

# For each struct, show the C++ definition at the target milestone
git show upstream/chrome/m{TARGET}:include/encode/SkPngEncoder.h | grep -A30 "struct Options"
git show upstream/chrome/m{TARGET}:include/encode/SkJpegEncoder.h | grep -A20 "struct Options"
git show upstream/chrome/m{TARGET}:include/encode/SkWebpEncoder.h | grep -A20 "struct Options"
# ... repeat for all asserted structs
```

Any new fields in a C++ struct that the C API mirrors via `reinterpret_cast` (not field-by-field
copy) will cause either a `static_assert` failure at compile time or silent memory corruption
at runtime. This check catches both.

### Step 4b: Verify deleted files → search for moves (MANDATORY)

When a file is deleted between milestones, **always search for where it moved** before
recommending removal of references. Skia rarely deletes functionality outright — it relocates.

```bash
# Find what was deleted
git diff upstream/chrome/m{CURRENT}..upstream/chrome/m{TARGET} --diff-filter=D --name-only

# For each deleted file, search for the same content at the new location
git ls-tree -r upstream/chrome/m{TARGET} --name-only | grep -i "FILENAME_STEM"
# Example: SkJSON.h deleted → search for "SkJSON" or "skjson" → finds modules/jsonreader/
```

Then update `#include` paths in our C API files rather than removing code.

### Step 4c: Verify "removed" symbols on target branch (MANDATORY)

When a diff hunk shows a symbol on a `-` line, it may have been **moved within the same
file** (reordered), not actually removed. Always confirm on the target branch:

```bash
# WRONG: Trusting the diff alone
git diff m{CURRENT}..m{TARGET} -- include/gpu/ganesh/GrContextOptions.h
# Shows "- bool fSuppressPrints" → might conclude "removed"

# RIGHT: Check the target branch directly
git show upstream/chrome/m{TARGET}:include/gpu/ganesh/GrContextOptions.h | grep "fSuppressPrints"
# Shows it still exists → it was reordered, not removed
```

### Common C API Patterns

**Enum removed from C++:**
```cpp
// Before (sk_enums.cpp)
case ENUM_VALUE_FOO:    return CppEnum::kFoo;

// After: Remove the line and the corresponding sk_types.h entry
```

**Function signature changed:**
```cpp
// Before (sk_image.cpp)
return ToImage(SkImage::MakeOld(args).release());

// After: Update to new API
return ToImage(SkImages::MakeNew(args).release());
```

**Header moved:**
```cpp
// Before
#include "include/gpu/GrOldHeader.h"

// After: Update include path
#include "include/gpu/ganesh/GrNewHeader.h"
```

## Step 5: C# Impact Analysis

For each C API change, check the C# side:

```bash
# Search C# wrappers for affected types
grep -rn "ENUM_NAME\|FUNCTION_NAME" binding/SkiaSharp/

# Check generated bindings
grep -rn "SYMBOL" binding/SkiaSharp/SkiaApi.generated.cs
```

## Step 6: Build & Verify

After applying fixes:
1. Build native: `dotnet cake --target=externals-macos --arch=arm64`
2. Regenerate: `pwsh ./utils/generate.ps1`
3. Build C#: `dotnet build binding/SkiaSharp/SkiaSharp.csproj`
4. Test: `dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj`

## Historical Examples

### m132 → m133 Changes Required

| Change | C API Fix | C# Fix |
|--------|-----------|--------|
| `src/utils/SkJSON.h` moved to `modules/jsonreader/` | Updated `#include` in `sk_linker.cpp` | None |
| `SkPngEncoder::Options` gained `fGainmap` + `fGainmapInfo` | Added fields to `sk_pngencoder_options_t` | None (null pointers) |
| `SkColorSpace::MakeCICP` + CICP enums added | New C API function + enum types | New `CreateCicp` factory + enums |
| `SkTypeface::getResourceName` added | New C API function | New `ResourceName` property |
| `GrContextOptions` fields reordered (no add/remove) | None (field-by-field copy) | None |
| `SkMaskFilter::approximateFilteredBounds` removed | None (not wrapped) | None |
| `SkSL::DebugTrace::writeTrace` removed | None (not wrapped) | None |
| Shared GPU stats types added (`GpuStatsFlags`, `GpuStats`) | None (additive, safe defaults) | None (callback-based, deferred) |

**Lessons learned:**
- `SkJSON.h` deletion was a **relocation** to `modules/jsonreader/` — always search for moves before removing references
- `SkPngEncoder::Options` field addition broke `static_assert` in `sk_structs.cpp` — always audit struct assertions
- `fSuppressPrints` appeared "removed" in diff but was actually reordered within `GrContextOptions.h` — verify on target branch
- `GpuTypes.h` changes looked Graphite-only but `GrFlushInfo` (Ganesh) uses those types — trace shared GPU header consumers

**Files changed:** 2 in C API (fixes), 2 in C API (new APIs), ~6 total in SkiaSharp PR

### m118 → m119 Changes Required

| Change | C API Fix | C# Fix |
|--------|-----------|--------|
| `SkTime::DateTime` → `SkPDF::DateTime` | Updated `sk_structs.cpp` | Updated `Definitions.cs` |
| New color type added | Added to `sk_enums.cpp` | Added to `EnumMappings.cs` |
| `SkImages::MakeWithFilter` API change | Updated `sk_image.cpp` call | None (auto-generated) |
| `GrDirectContext` API updates | Updated `gr_context.cpp` | Updated `GRDefinitions.cs` |

**Files changed:** 8 in C API, 12 total in SkiaSharp PR

### m117 → m118 Changes Required

| Change | C API Fix | C# Fix |
|--------|-----------|--------|
| `GrDirectContext::flush` signature change | Updated `gr_context.cpp` | Updated wrappers |
| `SkImage::makeWithFilter` deprecated | Updated to new factory | Auto-regenerated |

## Risk Assessment Template

Before proceeding with a milestone update, score the risk:

| Factor | Low Risk | Medium Risk | High Risk |
|--------|----------|-------------|-----------|
| Milestones skipped | 1 | 2-3 | 4+ |
| Removed APIs in C API | 0 | 1-3 | 4+ |
| Ganesh API changes | Minor | Moderate | Significant |
| New enum values | Additive only | Some renamed | Values removed |
| Test impact | None | Few tests | Many tests |

**Total risk** determines approach:
- **Low**: Direct merge, single iteration
- **Medium**: Sequential merges recommended
- **High**: Sequential merges mandatory, extensive testing required
