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
