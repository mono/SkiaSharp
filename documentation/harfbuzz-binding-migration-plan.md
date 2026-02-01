# HarfBuzz Binding Migration Plan

## Overview

**Goal:** Update the HarfBuzzSharp bindings from version 2.8.2 to 8.3.1 incrementally, documenting the binding generation process along the way.

**Current State:**
- Generated bindings based on: HarfBuzz 2.8.2 (`63e15eac4f443fa53565d1e4fb9611cdd7814f28`)
- DEPS currently points to: HarfBuzz 8.3.1 (`2b3631a866b3077d9d675caa4ec9010b342b5a7c`)
- Gap: ~6 years of HarfBuzz development

**Branch:** `dev/harfbuzz-binding-update`

---

## Phase 1: Documentation Creation

### Objective
Create comprehensive documentation for the binding generation system before starting migration work.

### Output
`documentation/binding-generation.md`

### Tasks

- [ ] Analyze generator code structure in `utils/SkiaSharpGenerator/`
  - [ ] `ConfigJson/Config.cs` - Main configuration model
  - [ ] `ConfigJson/TypeMapping.cs` - Type mapping options
  - [ ] `ConfigJson/FunctionMapping.cs` - Function mapping options
  - [ ] `ConfigJson/Mappings.cs` - Container for type/function mappings
  - [ ] `ConfigJson/Exclude.cs` - Exclusion configuration
  - [ ] `ConfigJson/NamespaceMapping.cs` - Namespace mapping options
  - [ ] `Generate/Generator.cs` - Main generation logic

- [ ] Study existing JSON configuration files
  - [ ] `binding/libHarfBuzzSharp.json`
  - [ ] `binding/libSkiaSharp.json`
  - [ ] `binding/libSkiaSharp.Resources.json`
  - [ ] `binding/libSkiaSharp.SceneGraph.json`
  - [ ] `binding/libSkiaSharp.Skottie.json`

- [ ] Document all configuration options with examples
  - [ ] Top-level options (dllName, namespace, className, includeDirs)
  - [ ] Headers and source configuration
  - [ ] Namespace mappings
  - [ ] Exclusion rules (files, types)
  - [ ] Type mappings (cs, internal, flags, obsolete, properties, generate, readonly, equality, members)
  - [ ] Function mappings (cs, parameters, generateProxy, proxySuffixes)

- [ ] Document the generation workflow
  - [ ] How to run the generator
  - [ ] What files are produced
  - [ ] How to iterate on configuration

---

## Phase 2: Documentation Review & Validation

### Objective
Validate documentation accuracy using multiple AI models, ensuring all statements are backed by actual code.

### Constraints
- **Code is source of truth** — documentation must describe what code does
- **No generator code changes** — only documentation corrections allowed
- All claims must have code evidence

### Models to Use
1. claude-opus-4.5
2. claude-sonnet-4
3. gpt-5.2
4. gpt-5.1
5. gemini-3-pro-preview

### Tasks

- [ ] Run review with each of the 5 models
  - [ ] Each review validates statements against `utils/SkiaSharpGenerator/` code
  - [ ] Each review checks for missing options or incorrect descriptions
  - [ ] Each review verifies examples are accurate

- [ ] Compile findings from all reviews
- [ ] Update documentation to address issues found
- [ ] Final validation pass

---

## Phase 3: Baseline Verification

### Objective
Establish a verified baseline at HarfBuzz 2.8.2 to confirm the current generated code matches expectations.

### Tasks

- [ ] Create and push branch `dev/harfbuzz-binding-update`

- [ ] Update `externals/skia/DEPS` harfbuzz line to:
  ```
  "third_party/externals/harfbuzz": "https://chromium.googlesource.com/external/github.com/harfbuzz/harfbuzz.git@63e15eac4f443fa53565d1e4fb9611cdd7814f28",
  ```

- [ ] Sync dependencies
  ```bash
  dotnet cake --target=git-sync-deps
  ```

- [ ] Verify HarfBuzz version
  ```bash
  cd externals/skia/third_party/externals/harfbuzz && git describe --tags
  # Expected: 2.8.2 or similar
  ```

- [ ] Run generator
  ```bash
  pwsh ./utils/generate.ps1 -Config libHarfBuzzSharp.json
  ```

- [ ] Verify no changes to generated file
  ```bash
  git diff binding/HarfBuzzSharp/HarfBuzzApi.generated.cs
  # Expected: No changes (or minimal whitespace)
  ```

- [ ] Build and run tests
  ```bash
  dotnet build binding/HarfBuzzSharp/HarfBuzzSharp.csproj
  dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj
  ```

- [ ] Commit baseline (SkiaSharp repo only, not externals/skia)
  ```
  docs: add binding generation documentation
  
  - Add documentation/binding-generation.md
  - Verified baseline at HarfBuzz 2.8.2
  ```

---

## Phase 4: Incremental Version Bumps

### Objective
Progressively update bindings through each major HarfBuzz version, adding tests and documenting changes.

### Version Progression

| Step | Version | Tag/Commit | Notes |
|------|---------|------------|-------|
| 0 | 2.8.2 | `63e15eac4f443fa53565d1e4fb9611cdd7814f28` | Baseline (current bindings) |
| 1 | 2.9.1 | `upstream/2.9.1` | Last 2.x release |
| 2 | 3.4.0 | `upstream/3.4.0` | Last 3.x release |
| 3 | 4.4.1 | `upstream/4.4.1` | Last 4.x release |
| 4 | 5.3.1 | `upstream/5.3.1` | Last 5.x release |
| 5 | 6.0.0 | `upstream/6.0.0` | Only 6.x release |
| 6 | 7.3.0 | `upstream/7.3.0` | Last 7.x release |
| 7 | 8.3.1 | `2b3631a866b3077d9d675caa4ec9010b342b5a7c` | Target (current DEPS) |

### Per-Version Workflow

For each version bump:

#### 1. Update Dependencies
```bash
# Edit externals/skia/DEPS - update harfbuzz line to new version
dotnet cake --target=git-sync-deps
```

#### 2. Review Changelog
```bash
# Check NEWS file for API changes
cat externals/skia/third_party/externals/harfbuzz/NEWS
```

Look for:
- New functions added
- New types/enums added
- Deprecated functions
- Breaking changes

#### 3. Run Generator
```bash
pwsh ./utils/generate.ps1 -Config libHarfBuzzSharp.json
```

#### 4. Review Generated Changes
```bash
git diff binding/HarfBuzzSharp/HarfBuzzApi.generated.cs
```

Categorize changes:
- **New APIs:** Functions, types, enums added
- **Modified APIs:** Signature changes, new parameters
- **Removed APIs:** Deprecated/removed functions

#### 5. Update Configuration (if needed)

Edit `binding/libHarfBuzzSharp.json` to handle new APIs:

**Prefer including APIs** — only exclude if:
- API requires custom interop code
- API is cumbersome to expose safely
- API uses unsupported patterns

If excluding, document in Phase 5.

#### 6. Build and Test
```bash
dotnet build binding/HarfBuzzSharp/HarfBuzzSharp.csproj
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj
```

#### 7. Add Tests for New APIs

Location: `tests/Tests/HarfBuzzSharp/`

Guidelines:
- Use existing test files for existing types
- Create new files for new types
- Focus on basic coverage, not exhaustive testing
- Test happy path and one error case where applicable
- Reference changelog for what's important to test

#### 8. Update Documentation
- Add findings to `documentation/binding-generation.md` if new patterns discovered
- Note any issues or workarounds needed

#### 9. Commit and Push
```bash
git add binding/ tests/ documentation/
git commit -m "feat(harfbuzz): update bindings to X.Y.Z

- [List key API additions]
- [List any config changes made]
- [List tests added]"
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

## Phase 5: Skipped API Documentation

### Objective
Document any APIs that were excluded during migration, with rationale and future implementation plans.

### Output
`documentation/harfbuzz-skipped-apis.md`

### Template for Each Skipped API

```markdown
## `hb_function_name`

**Signature:**
```c
return_type hb_function_name(param1_type param1, param2_type param2);
```

**Added in version:** X.Y.Z

**Reason for skipping:**
- [ ] Requires custom interop code
- [ ] Uses unsupported C patterns (e.g., unions, bitfields)
- [ ] Callback signature incompatible with generator
- [ ] Would require manual memory management wrapper
- [ ] Other: [explain]

**Proposed future approach:**

Option A: Generator enhancement
- [Describe what generator changes would be needed]

Option B: Custom wrapper code
- [Describe manual C# wrapper approach]

**Priority:** Low / Medium / High

**Related APIs:** [List any related functions that were also skipped]
```

### Tasks

- [ ] Review all exclusions made during Phase 4
- [ ] Document each skipped API using template above
- [ ] Categorize by reason for skipping
- [ ] Prioritize based on likely user demand
- [ ] Commit documentation

---

## Final Deliverables Checklist

- [ ] `documentation/binding-generation.md` — Comprehensive binding generation guide
- [ ] `documentation/harfbuzz-skipped-apis.md` — Skipped APIs with rationale and future plans
- [ ] `binding/libHarfBuzzSharp.json` — Updated configuration for 8.3.1
- [ ] `binding/HarfBuzzSharp/HarfBuzzApi.generated.cs` — Updated generated bindings
- [ ] `tests/Tests/HarfBuzzSharp/` — New tests for added APIs
- [ ] Branch `dev/harfbuzz-binding-update` pushed with incremental commits

---

## Key Files Reference

| File | Purpose |
|------|---------|
| `externals/skia/DEPS` | Dependency versions (harfbuzz line) |
| `externals/skia/third_party/externals/harfbuzz/` | HarfBuzz source |
| `externals/skia/third_party/externals/harfbuzz/NEWS` | HarfBuzz changelog |
| `binding/libHarfBuzzSharp.json` | Generator configuration |
| `binding/HarfBuzzSharp/HarfBuzzApi.generated.cs` | Generated C# bindings |
| `utils/SkiaSharpGenerator/` | Generator source code |
| `utils/generate.ps1` | Generator entry script |
| `tests/Tests/HarfBuzzSharp/` | HarfBuzzSharp tests |

---

## Commands Reference

```bash
# Sync dependencies after DEPS change
dotnet cake --target=git-sync-deps

# Run generator for HarfBuzz only
pwsh ./utils/generate.ps1 -Config libHarfBuzzSharp.json

# Build HarfBuzzSharp
dotnet build binding/HarfBuzzSharp/HarfBuzzSharp.csproj

# Run tests
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj

# Check current HarfBuzz version
cd externals/skia/third_party/externals/harfbuzz && git describe --tags
```

---

## Notes

- **Do not push externals/skia** — It will eventually match the final commit (8.3.1)
- **Commits are for reviewer visibility** — Each version bump is a separate commit
- **Code is source of truth** — Documentation describes actual behavior, not aspirational
- **Prefer inclusion over exclusion** — Only skip APIs when truly necessary
