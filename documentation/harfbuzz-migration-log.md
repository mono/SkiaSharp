# HarfBuzz Binding Migration Log

This file tracks progress through the migration for transparency and resumability.

---

## Session Start
- **Started:** 2026-02-01T23:20:04Z
- **Target:** HarfBuzz 2.8.2 → 8.3.1
- **Branch:** `dev/harfbuzz-binding-update`

---

## Current State

**Phase:** 1 - Documentation Creation
**Status:** Starting
**Last Action:** Created tracking files

---

## Progress Log

### 2026-02-01T23:20 - Session Start
- Created migration plan: `documentation/harfbuzz-binding-migration-plan.md`
- Created this log file: `documentation/harfbuzz-migration-log.md`
- Starting Phase 1: Documentation Creation

### 2026-02-01T23:25 - Phase 1 In Progress
- Analyzed all generator code files
- Studied all JSON config files
- Created comprehensive `documentation/binding-generation.md`

### 2026-02-01T23:30 - Phase 1 Complete
- `documentation/binding-generation.md` created with full documentation
- Starting Phase 2: Multi-model documentation review

---

## Phase 1: Documentation Creation

### Generator Code Analysis
- [x] `ConfigJson/Config.cs`
- [x] `ConfigJson/TypeMapping.cs`
- [x] `ConfigJson/FunctionMapping.cs`
- [x] `ConfigJson/Mappings.cs`
- [x] `ConfigJson/Exclude.cs`
- [x] `ConfigJson/NamespaceMapping.cs`
- [x] `Generate/Generator.cs`

### JSON Config Study
- [x] `binding/libHarfBuzzSharp.json`
- [x] `binding/libSkiaSharp.json`
- [x] `binding/libSkiaSharp.Resources.json`
- [x] `binding/libSkiaSharp.SceneGraph.json`
- [x] `binding/libSkiaSharp.Skottie.json`

---

## Phase 2: Documentation Review

### Model Reviews
- [x] claude-opus-4.5 - Complete
- [x] claude-sonnet-4 - Complete
- [x] gpt-5.2 - Complete
- [x] gpt-5.1 - Complete
- [x] gemini-3-pro-preview - Complete

### Key Findings from Reviews

1. **`namespaces` clarification**: Only affects types (structs/enums/delegates), not P/Invoke function names
2. **`generate` and `obsolete`**: Only apply to enums, not structs
3. **`members` with empty string**: Hides property but field is still generated for struct layout
4. **`mappings.functions.cs`**: Only renames callback typedefs/proxies, not P/Invoke functions
5. **Excluded types**: Pointer variants (`T*`, `T**`) are automatically excluded too
6. **`source` config**: Used by verify command, not just for documentation
7. **Name cleaning**: Has additional rules for `_private_` prefix, `reserved` fields, `f` prefix removal
8. **Standard type mappings**: Table was incomplete (missing `usize_t`, `intptr_t`, `char` variants)
9. **Build configurations**: Clarified as preprocessor defines, not build modes
10. **WASM compatibility**: Function pointers converted to `void*` for compatibility

### Documentation Updated
- All inaccuracies corrected
- Missing information added
- Examples clarified

---

## Phase 3: Baseline Verification
- [x] Branch created and pushed
- [x] DEPS updated to 2.8.2
- [x] git-sync-deps run
- [x] Generator run
- [x] No changes verified ✓
- [x] Tests pass (106/106)
- [x] Committed (docs only - baseline verified)

---

## Phase 4: Version Bumps

| Version | DEPS Updated | Synced | Generated | Config Updated | Built | Tests Pass | Tests Added | Committed |
|---------|--------------|--------|-----------|----------------|-------|------------|-------------|-----------|
| 2.9.1   | [x]          | [x]    | [x]       | N/A            | [x]   | [x]        | N/A         | [x]       |
| 3.4.0   | [x]          | [x]    | [x]       | [x]            | [x]   | [x]        | N/A         | [x]       |
| 4.4.1   | [x]          | [x]    | [x]       | [x]            | [x]   | [x]        | N/A         | [x]       |
| 5.3.1   | [x]          | [x]    | [x]       | N/A            | [x]   | [x]        | N/A         | [x]       |
| 6.0.0   | [x]          | [x]    | [x]       | N/A            | [x]   | [x]        | N/A (no API change) | N/A (no API change) |
| 7.3.0   | [x]          | [x]    | [x]       | [x]            | [x]   | [x]        | N/A         | [x]       |
| 8.3.0*  | [x]          | [x]    | [x]       | N/A            | [x]   | [x]        | [ ]         | [x]       |

*Note: Using 8.3.0 instead of 8.3.1 due to libclang parsing issue with inttypes.h change

---

## Phase 5: Skipped APIs
- [x] Documentation created (`documentation/harfbuzz-skipped-apis.md`)

---

## Key Findings

### Namespace Exclude Feature
- The `namespaces` config supports an `exclude: true` option to exclude entire API families
- This is the cleanest way to exclude complex callback-based APIs like Paint
- Works by filtering types and functions whose names start with the prefix

### API Changes Across Versions
| Version | Major API Additions | Notable Changes |
|---------|--------------------|--------------------|
| 2.9.1 | `hb_set_invert` | - |
| 3.4.0 | Buffer similar, style APIs, synthetic slant | - |
| 4.4.1 | Draw API (`hb_draw_funcs_*`) | Callback-based drawing |
| 5.3.1 | `hb_language_matches`, optical bound API | - |
| 6.0.0 | (Subsetting APIs only, not parsed) | No public API changes |
| 7.3.0 | Paint API | Complex ColorLine callbacks excluded |
| 8.3.0 | `baseline2`, `font_extents` APIs | Removed deprecated `glyph_shape` |

### Generator Limitations Discovered
1. Struct fields with function pointer types cause issues between USE_LIBRARY_IMPORT and standard mode
2. libclang `#include_next` chains can fail to resolve SDK headers
3. Excluding files only works for directly enumerated files, not transitively included ones

---

## Issues Encountered

### 1. Paint API with ColorLine struct (7.3.0)
- **Problem:** HarfBuzz 7.3.0 introduced Paint API with `hb_color_line_t` struct containing function pointer fields
- **Impact:** Generator creates delegate types for non-USE_LIBRARY_IMPORT builds but struct properties reference delegate types that aren't defined in USE_LIBRARY_IMPORT mode
- **Solution:** Used namespace exclusion (`hb_paint_`, `hb_color_line_`) to exclude all Paint-related types and functions
- **APIs affected:** All `hb_paint_*` functions, `hb_color_line_*` types

### 2. libclang inttypes.h parsing failure (8.3.1)
- **Problem:** HarfBuzz 8.3.1 changed from `#include <stdint.h>` to `#include <inttypes.h>` in hb-common.h
- **Impact:** libclang's `inttypes.h` uses `#include_next` which fails to find SDK's implementation
- **Solution:** Use 8.3.0 instead (identical API, only C implementation difference)
- **Alternative (not pursued):** Adding SDK include path caused system types (pthread) to be parsed

---

## Files Modified

(Will be populated as files are changed)

