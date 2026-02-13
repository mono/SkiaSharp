# HarfBuzz Binding Migration Plan

## Overview

**Goal:** Update the HarfBuzzSharp bindings from version 2.8.2 to 8.3.0 incrementally, documenting the binding generation process along the way.

**Current State:** ✅ **COMPLETE**
- Generated bindings based on: HarfBuzz 8.3.0 (`894a1f72ee93a1fd8dc1d9218cb3fd8f048be29a`)
- Note: Using 8.3.0 instead of 8.3.1 due to libclang parsing issue with inttypes.h change

**Branch:** `dev/harfbuzz-binding-update`

---

## Phase 1: Documentation Creation ✅ COMPLETE

### Objective
Create comprehensive documentation for the binding generation system before starting migration work.

### Output
`documentation/binding-generation.md` ✅

### Tasks

- [x] Analyze generator code structure in `utils/SkiaSharpGenerator/`
  - [x] `ConfigJson/Config.cs` - Main configuration model
  - [x] `ConfigJson/TypeMapping.cs` - Type mapping options
  - [x] `ConfigJson/FunctionMapping.cs` - Function mapping options
  - [x] `ConfigJson/Mappings.cs` - Container for type/function mappings
  - [x] `ConfigJson/Exclude.cs` - Exclusion configuration
  - [x] `ConfigJson/NamespaceMapping.cs` - Namespace mapping options
  - [x] `Generate/Generator.cs` - Main generation logic

- [x] Study existing JSON configuration files
  - [x] `binding/libHarfBuzzSharp.json`
  - [x] `binding/libSkiaSharp.json`
  - [x] `binding/libSkiaSharp.Resources.json`
  - [x] `binding/libSkiaSharp.SceneGraph.json`
  - [x] `binding/libSkiaSharp.Skottie.json`

- [x] Document all configuration options with examples
- [x] Document the generation workflow

---

## Phase 2: Documentation Review & Validation ✅ COMPLETE

### Objective
Validate documentation accuracy using multiple AI models, ensuring all statements are backed by actual code.

### Models Used
1. claude-opus-4.5 ✅
2. claude-sonnet-4 ✅
3. gpt-5.2 ✅
4. gpt-5.1 ✅
5. gemini-3-pro-preview ✅

### Key Findings
- Namespace `exclude` feature discovered and documented
- Many standard type mappings added to documentation
- `generate`/`obsolete` clarified as enum-only options
- `members` with empty string behavior clarified

- [ ] Compile findings from all reviews
- [ ] Update documentation to address issues found
- [ ] Final validation pass

---

## Phase 3: Baseline Verification ✅ COMPLETE

### Objective
Establish a verified baseline at HarfBuzz 2.8.2 to confirm the current generated code matches expectations.

### Tasks

- [x] Create and push branch `dev/harfbuzz-binding-update`
- [x] Update DEPS to HarfBuzz 2.8.2
- [x] Sync dependencies and regenerate
- [x] Verify no changes to generated file ✅
- [x] Build and run tests (106/106 passed) ✅
- [x] Commit baseline

---

## Phase 4: Incremental Version Bumps ✅ COMPLETE

### Objective
Progressively update bindings through each major HarfBuzz version, adding tests and documenting changes.

### Version Progression (Completed)

| Step | Version | Status | Key Changes |
|------|---------|--------|-------------|
| 0 | 2.8.2 | ✅ Baseline | - |
| 1 | 2.9.1 | ✅ Complete | `hb_set_invert` |
| 2 | 3.4.0 | ✅ Complete | Buffer similar, style APIs, synthetic slant |
| 3 | 4.4.1 | ✅ Complete | Draw API (`hb_draw_funcs_*`) |
| 4 | 5.3.1 | ✅ Complete | `hb_language_matches`, optical bound API |
| 5 | 6.0.0 | ✅ No API changes | (Subsetting only) |
| 6 | 7.3.0 | ✅ Complete | Paint API (excluded via namespace) |
| 7 | 8.3.0* | ✅ Complete | `baseline2`, `font_extents` APIs |

*Note: Using 8.3.0 instead of 8.3.1 due to libclang inttypes.h parsing issue
git push origin dev/harfbuzz-binding-update
```

### Detailed Version Tasks

#### 2.9.1 (Last 2.x)
- [ ] Update DEPS to 2.9.1
- [ ] Run git-sync-deps
- [ ] Review NEWS for 2.9.0 and 2.9.1 changes
- [ ] Run generator
- [ ] Review diff
- [ ] Update config if needed
- [ ] Build and test
- [ ] Add tests for new APIs
- [ ] Commit and push

#### 3.4.0 (Last 3.x)
- [ ] Update DEPS to 3.4.0
- [ ] Run git-sync-deps
- [ ] Review NEWS for 3.0.0 through 3.4.0 changes
- [ ] Run generator
- [ ] Review diff (expect more changes - major version)
- [ ] Update config if needed
- [ ] Build and test
- [ ] Add tests for new APIs
- [ ] Commit and push

#### 4.4.1 (Last 4.x)
- [ ] Update DEPS to 4.4.1
- [ ] Run git-sync-deps
- [ ] Review NEWS for 4.0.0 through 4.4.1 changes
- [ ] Run generator
- [ ] Review diff
- [ ] Update config if needed
- [ ] Build and test
- [ ] Add tests for new APIs
- [ ] Commit and push

#### 5.3.1 (Last 5.x)
- [ ] Update DEPS to 5.3.1
- [ ] Run git-sync-deps
- [ ] Review NEWS for 5.0.0 through 5.3.1 changes
- [ ] Run generator
- [ ] Review diff
- [ ] Update config if needed
- [ ] Build and test
- [ ] Add tests for new APIs
- [ ] Commit and push

#### 6.0.0 (Only 6.x)
- [ ] Update DEPS to 6.0.0
- [ ] Run git-sync-deps
- [ ] Review NEWS for 6.0.0 changes
- [ ] Run generator
- [ ] Review diff
- [ ] Update config if needed
- [ ] Build and test
- [ ] Add tests for new APIs
- [ ] Commit and push

#### 7.3.0 (Last 7.x)
- [ ] Update DEPS to 7.3.0
- [ ] Run git-sync-deps
- [ ] Review NEWS for 7.0.0 through 7.3.0 changes
- [ ] Run generator
- [ ] Review diff
- [ ] Update config if needed
- [ ] Build and test
- [ ] Add tests for new APIs
- [ ] Commit and push

#### 8.3.1 (Target)
- [ ] Update DEPS to 8.3.1 (restore original commit)
- [ ] Run git-sync-deps
- [ ] Review NEWS for 8.0.0 through 8.3.1 changes
- [ ] Run generator
- [ ] Review diff
- [ ] Update config if needed
- [ ] Build and test
- [ ] Add tests for new APIs
- [ ] Commit and push

---

## Phase 5: Skipped API Documentation ✅ COMPLETE

### Objective
Document any APIs that were excluded during migration, with rationale and future implementation plans.

### Output
`documentation/harfbuzz-skipped-apis.md` ✅

### Excluded API Categories
- Paint API (`hb_paint_*`, `hb_color_line_*`) - complex callbacks with struct-embedded function pointers
- Draw API callbacks - proxy generation skipped due to complexity
- Deprecated APIs - excluded via file
- Shape Plan API - excluded via file
- Variation API (hb-ot-var) - excluded via file

---

## Summary

### Commits Made
1. `c6c6a174` - docs: add binding generation documentation and migration plan
2. `f66a53ba` - feat(harfbuzz): update bindings to 2.9.1
3. `befdb35e` - feat(harfbuzz): update bindings to 3.4.0
4. `9dc5b763` - feat(harfbuzz): update bindings to 4.4.1
5. `6418f493` - feat(harfbuzz): update bindings to 5.3.1
6. `89651699` - feat(harfbuzz): update bindings to 7.3.0
7. `0bdf7329` - feat(harfbuzz): update bindings to 8.3.0
8. `83eb73cc` - feat(harfbuzz): add C# wrappers and tests for new APIs
9. `996f918b` - feat(harfbuzz): add variable font and additional APIs
10. `22f0bc25` - docs: document 8.3.1 parsing issue and attempted solutions
11. `32e4faf3` - refactor(harfbuzz): split HBNewApiTest.cs into appropriate test files
12. `00ab5c57` - feat(harfbuzz): add Face.GetName and Face.TryGetName for OpenType names
13. `e0581af7` - feat(harfbuzz): add Font Ppem/Ptem and Face OpenType layout APIs

### New C# APIs Added
- `Language.Matches(Language)` - prefix matching for languages
- `Buffer.CreateSimilar(Buffer)` - create buffer with same settings
- `Font.SyntheticSlant` - get/set synthetic slant for oblique fonts
- `Font.SetVariations(Variation[])` - set multiple variation axes
- `Font.SetVariations(ReadOnlySpan<Variation>)` - span overload
- `Font.SetVariation(uint tag, float value)` - set single axis by numeric tag
- `Font.SetVariation(string tag, float value)` - set single axis by string name
- `Font.SetSyntheticBold(float x, float y, bool inPlace)` - synthetic emboldening
- `Font.GetSyntheticBold(out float x, out float y, out bool inPlace)` - get emboldening
- `Font.NamedInstance` - get/set named instance index
- `Font.GetPpem(out int, out int)` - get pixels per em
- `Font.SetPpem(int, int)` - set pixels per em
- `Font.Ptem` property - point size for optical sizing
- `Face.GetName(OpenTypeNameId)` - get font name from OpenType name table
- `Face.GetName(OpenTypeNameId, Language)` - with language parameter
- `Face.TryGetName(OpenTypeNameId, out string)` - returns false if not found
- `Face.TryGetName(OpenTypeNameId, Language, out string)` - with language parameter
- `Face.GetAllNameEntries()` - enumerate all name table entries
- `Face.GetOpenTypeLayoutScriptTags(tableTag)` - scripts in GSUB/GPOS
- `Face.GetOpenTypeLayoutFeatureTags(tableTag)` - features in GSUB/GPOS
- `Face.TryGetOpenTypeLayoutFeatureNameIds()` - feature name IDs for UI

### New Types Added
- `OpenTypeLayoutTableTag` enum (Gsub, Gpos)
- `OpenTypeFeatureNameIds` struct

### Configuration Changes
- Added namespace exclusions: `hb_paint_`, `hb_color_line_`
- Added type exclusions for Paint API related types
- Added `generateProxy: false` for Draw API callbacks

### Test Results
- All 155 HarfBuzz tests pass (106 original + 49 new)

### Documentation Created
- `documentation/binding-generation.md` - comprehensive generator documentation
- `documentation/harfbuzz-binding-migration-plan.md` - this plan
- `documentation/harfbuzz-migration-log.md` - progress tracking
- `documentation/harfbuzz-skipped-apis.md` - excluded API documentation
