# Breaking Changes Checklist for Skia Milestone Updates

## How to Use This Document

When updating Skia to a new milestone, follow these steps in order. Each step builds
on the previous one. The goal is to identify every change that affects our C API shim
layer **before** you start merging.

## Step 1: Gather Release Notes

Use the authoritative range prepared by Phase 2:

```bash
git log --oneline "$DIFF_RANGE"
git diff --stat "$DIFF_RANGE" -- src/ include/ BUILD.gn DEPS
```

Read the official release-note section for every milestone crossed as additional context.

## Step 2: Filter by Relevance

Relevance is whether our C API or a build we ship touches the code, not which backend family
owns it:

| Prefix/Keyword | Relevant? | Notes |
|----------------|-----------|-------|
| `Gr*`, `skgpu::`, `Dawn*`, `wgpu::` | ✅ Yes | GPU code — inspect the affected C shim and `native/*/build.cake` consumers |
| `SkImage`, `SkSurface`, `SkCanvas` | ✅ Yes | Core APIs — always check |
| `SkTypeface`, `SkFont` | ✅ Yes | Text/font APIs — always check |
| `SkPath`, `SkPaint` | ✅ Yes | Drawing APIs — always check |
| `SkSL`, `SkRuntimeEffect` | ⚠️ Maybe | Only if C API exposes runtime effects |
| `SkCodec`, `SkEncoder` | ⚠️ Maybe | Only if C API exposes codec/encoder APIs |

**Two common misclassification traps:**

1. **Shared GPU headers** (`include/gpu/GpuTypes.h`, `include/gpu/*.h`): check consumers in both
   `include/gpu/ganesh/` and `include/gpu/graphite/` before classifying a change as local.

2. **Struct field changes in asserted types**: `sk_structs.cpp` has `static_assert(sizeof(...))`
   for every C API struct mapped via `reinterpret_cast`. A field addition to any of these
   C++ structs (encoders, lattice, frame info, etc.) is a **build break** even if the
   release notes don't mention it prominently.

## Step 3: Categorize Each Change

Create a table for each change:

```markdown
| # | Change | Risk | C API Impact | C# Impact | Action |
|---|--------|------|-------------|-----------|--------|
| 1 | `GrMipmapped` removed | 🔴 HIGH | sk_types.h enum | GRDefinitions.cs | Update enum + mapping |
| 2 | `SkFont::refTypefaceOrDefault` removed | 🟡 MED | None (not wrapped) | None | No action needed |
| 3 | New `SkVertices::Builder` SK_API | 🟢 LOW | None | None | Optional: wrap later |
```

## Step 4: Verify the Analysis

The release notes don't cover everything. These checks catch what they miss.

**Check struct sizes:**
```bash
# List all asserted structs, then compare each against the target milestone
grep "static_assert.*sizeof" src/c/sk_structs.cpp
git show upstream/chrome/m{TARGET}:include/encode/SkPngEncoder.h | grep -A30 "struct Options"
```

**Check for moved files** (Skia relocates, it rarely deletes):
```bash
git diff "$DIFF_RANGE" --diff-filter=D --name-only
# For each deleted file our C API references:
git ls-tree -r upstream/chrome/m{TARGET} --name-only | grep -i "FILENAME_STEM"
```

**Confirm removals on the target branch** (diff `-` lines can be reorders, not deletions):
```bash
# Don't trust the diff — verify directly
git show upstream/chrome/m{TARGET}:include/gpu/ganesh/GrContextOptions.h | grep "fSuppressPrints"
```

## Step 5: C API Impact Analysis

For each HIGH/MEDIUM risk change, check the C API:

```bash
cd externals/skia
grep -rn "SYMBOL_NAME" src/c/ include/c/
```

For every wrapped factory or context-creation path touched by the range, inspect implementation
changes as well as public headers. An unchanged signature can still gain a new null-return
precondition or lose default/fallback behavior:

```bash
git log --oneline "$DIFF_RANGE" -- <implementation-paths-used-by-the-C-API>
git diff "$DIFF_RANGE" -- <implementation-paths-used-by-the-C-API>
```

Do not classify a backend update as low risk solely because the public header and C API
signature are unchanged.

## Step 6: Dependency Compatibility Audit

Diff the fork base against the target; an upstream-to-upstream diff hides fork-specific pins:

```bash
git diff origin/{SKIA_BASE_BRANCH}..{TARGET_UPSTREAM_REF} -- DEPS
```

For each changed revision or enabled/commented state, inspect both fork history and the
upstream roll commit. A dependency roll and wrapper-source change in the same commit are
presumed coupled until the older fork revision is proven to expose the target API.

### Recurring patterns across milestone bumps

| Pattern | Risk | C API Fix | C# Fix |
|---------|------|-----------|--------|
| **Removed static factory** → replaced with context-taking free function | 🔴 HIGH | Update call + add `#include` for new header | Add new overload or update existing |
| **Header path reorganization** (e.g., `include/gpu/` → `include/gpu/ganesh/`) | 🟡 MED | Update `#include` paths | None |
| **Factory moved to namespace** (`ClassName::Make*` → `ClassNames::Make*`) | 🔴 HIGH | Update function call + add `#include` | None (auto-generated) |
| **Type renamed into namespace** (e.g., `GrVkAlloc` → `skgpu::VulkanAlloc`) | 🔴 HIGH | Update type refs in `sk_types_priv.h` | Update managed struct names |
| **Struct field added/removed** (breaks `static_assert`) | 🔴 HIGH | Update `sk_types.h` struct definition | Update managed struct |
| **Enum value inserted mid-sequence** (renumbers everything after) | 🟡 MED | Regenerate bindings — never hand-edit | Regenerate + update mappings |
| **File moved to new module** (e.g., `src/utils/` → `modules/`) | 🟡 MED | Update `#include` path | None |

## Step 7: C# Impact Analysis

```bash
grep -rn "ENUM_NAME\|FUNCTION_NAME" binding/SkiaSharp/
grep -rn "SYMBOL" binding/SkiaSharp/SkiaApi.generated.cs
```

## Step 8: Build & Verify

After applying fixes:
1. Build native for the current host from source
2. Regenerate: `python3 .agents/skills/update-skia/scripts/regenerate_bindings.py`
3. Build C#: `dotnet build binding/SkiaSharp/SkiaSharp.csproj`
4. Run `dotnet test tests/SkiaSharp.Tests.Console.slnx` unfiltered
5. If it fails, use the owning host project for filtered diagnostics
6. Rerun the unfiltered solution; only that run is final validation

## Historical Examples

### m132 → m133 Changes Required

| Change | C API Fix | C# Fix |
|--------|-----------|--------|
| `src/utils/SkJSON.h` moved to `modules/jsonreader/` | Updated `#include` in `sk_linker.cpp` | None |
| `SkPngEncoder::Options` gained `fGainmap` + `fGainmapInfo` | Added fields to `sk_pngencoder_options_t` | None (null pointers) |
| `SkColorSpace::MakeCICP` + CICP enums added | New C API function + enum types | New `CreateCicp` factory + enums |
| `GrContextOptions` fields reordered (no add/remove) | None (field-by-field copy) | None |
| `SkMaskFilter::approximateFilteredBounds` removed | None (not wrapped) | None |
| Shared GPU stats types added (`GpuStatsFlags`, `GpuStats`) | None (additive, safe defaults) | None (callback-based, deferred) |

**Lessons learned:**
- `SkJSON.h` deletion was a **relocation** to `modules/jsonreader/` — always search for moves
- `SkPngEncoder::Options` field addition broke `static_assert` — always audit asserted structs
- `fSuppressPrints` appeared "removed" in diff but was reordered — verify on target branch
- `GpuTypes.h` changes looked backend-local but `GrFlushInfo` (Ganesh) uses them — trace consumers

### m118 → m119 Changes Required

| Change | C API Fix | C# Fix |
|--------|-----------|--------|
| `SkTime::DateTime` → `SkPDF::DateTime` | Updated `sk_structs.cpp` | Updated `Definitions.cs` |
| New color type added | Added to `sk_enums.cpp` | Added to `EnumMappings.cs` |
| `SkImages::MakeWithFilter` API change | Updated `sk_image.cpp` call | None (auto-generated) |
| `GrDirectContext` API updates | Updated `gr_context.cpp` | Updated `GRDefinitions.cs` |

### m117 → m118 Changes Required

| Change | C API Fix | C# Fix |
|--------|-----------|--------|
| `GrDirectContext::flush` signature change | Updated `gr_context.cpp` | Updated wrappers |
| `SkImage::makeWithFilter` deprecated | Updated to new factory | Auto-regenerated |

## Risk Assessment Template

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
