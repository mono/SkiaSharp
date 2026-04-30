# API Review Workflow

Review an existing PR or diff containing new SkiaSharp/HarfBuzzSharp APIs.
Produces actionable feedback and can auto-fix high-confidence issues.

This workflow is used:
- Standalone: when asked to review a PR or API surface
- As final gate: called by the add-workflow after implementing new APIs

## Inputs

One of:
- A PR number or URL
- A branch with uncommitted changes
- A set of files to review

## Step 1: Gather the API Surface

List every new or changed public member:
- Properties, methods, constructors, operators
- New types (structs, classes, enums, ref structs)
- Generated struct changes (from libSkiaSharp.json / libHarfBuzzSharp.json)
- C API functions (in externals/skia/include/c/ and src/c/)

Build a table: `| Type | Member | Signature | Notes |`

## Step 2: Check Against Design Rules

Read [api-design-rules.md](api-design-rules.md) and verify each item:

### Naming
- [ ] Type names match Skia upstream (not invented names)
- [ ] No unnecessary context baked into type names
- [ ] Full words, not abbreviations (unless existing convention)
- [ ] Consistent with existing HarfBuzzSharp/SkiaSharp naming
- [ ] File names match primary type

### Properties vs Methods
- [ ] Parameterless array getters are properties, not Get*() methods
- [ ] Count properties exist alongside array properties
- [ ] Span fill methods are methods (they take parameters)

### Span Overloads
- [ ] Every array-returning API has a Span overload
- [ ] Every array-accepting API uses ReadOnlySpan
- [ ] Span overloads return int (items written or total count)

### Parameter Design
- [ ] No default parameters (use overloads for ABI stability)
- [ ] Ref struct parameter bag if >3 optional params
- [ ] Common-case shortcuts alongside full-control overloads

### Type Wrapping
- [ ] Simple uint32_t typedefs wrapped as structs with Parse/ToString
- [ ] Implicit uint conversion operators
- [ ] IEquatable, ==, !=, GetHashCode
- [ ] Mapped in JSON config so generated structs use the type

### Generated Code
- [ ] No manual edits to *.generated.cs
- [ ] JSON config maps types and member names correctly
- [ ] Generator was re-run after any C API changes

### C API
- [ ] DEF_MAP for layout-compatible structs
- [ ] static_assert in sk_structs.cpp
- [ ] Proper As*/To* helpers in sk_types_priv.h
- [ ] Include guards for C headers
- [ ] Return value semantics match upstream docs (not just header)

### Validation
- [ ] Null checks with ArgumentNullException
- [ ] Index validation with ArgumentOutOfRangeException
- [ ] No validation in C layer (trust caller pattern)

### Documentation
- [ ] No triple-slash XML doc comments (inserted by separate process)
- [ ] No #nullable disable unless needed for reference-type fields

### Style
- [ ] File-scoped namespaces in new files
- [ ] using statements for all disposables in tests and samples

## Step 3: Check Test Coverage

For each new public API member, verify a test exists that:

1. **Exercises the API** — calls it and asserts a result
2. **Asserts exact values** — not just "not null" or "not empty"
3. **Has a Span equivalence test** — if there's both array and Span version
4. **Has a static/empty font test** — graceful degradation
5. **Has negative index tests** — for all index parameters
6. **Has interop round-trip test** — data survives P/Invoke
7. **Has a dedicated test file** — standalone types get their own file

### Missing test checklist

Build a table:
```
| API | Exact Values | Span Equiv | Static Font | Neg Index | Round-trip | File |
```

Flag any gaps as "Missing test: [description]".

## Step 4: Build and Run Tests

```bash
# Build native (required if C API changed)
dotnet cake --target=externals-macos --arch=arm64

# Build C#
dotnet build binding/SkiaSharp/SkiaSharp.csproj

# Run all tests
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj
```

If tests fail, categorize:
- **Our code:** fix immediately
- **Pre-existing:** note and skip
- **Flaky:** run again to confirm

## Step 5: Check Sample Code

Determine whether the new API needs a gallery sample:

### Does a sample exist?

If a gallery sample **already exists** for this feature area, verify:
- [ ] Uses the new APIs correctly (not deprecated patterns)
- [ ] No HarfBuzz dependency in SkiaSharp-only samples (unless justified)
- [ ] Uses real fonts, not fabricated/modified ones
- [ ] Sample is clear and demonstrates API patterns (not optimized for perf)
- [ ] `static readonly` for constant data (weights arrays, etc.)
- [ ] Slider casts handled correctly (`(int)(float)value` for int sliders)
- [ ] New API parameters exposed via interactive controls where appropriate

### Should a new sample be created?

If no sample exists, evaluate:

| Feature scope | Sample needed? |
|--------------|---------------|
| New standalone capability (new drawing mode, rendering technique, font feature) | **Yes** — new sample file |
| Enhancement to existing capability (new overload, extra option) | **Maybe** — update existing sample with new controls |
| Internal/plumbing API (no visible user effect) | **No** |

Flag missing samples as "Missing sample: [description]" in the review report.

### New sample checklist
- [ ] Extends `CanvasSampleBase` or `DocumentSampleBase`
- [ ] Has `Title`, `Description`, and `Category` (from `SampleCategories`)
- [ ] Interactive controls (sliders, pickers, toggles) let users explore the API
- [ ] Assets loaded from `SampleMedia` (new assets added to `Media/` and registered)
- [ ] All SkiaSharp objects disposed (`using` or `OnDestroy`)
- [ ] Teaches the API — clarity over performance
- [ ] Real assets only — no fabricated fonts or images
- [ ] Reasonable defaults — shows a good result before any user interaction

## Step 6: Produce Report

Output a structured review:

```
## API Review: [feature name]

### ✅ Correct
- [list things that are good]

### ⚠️ Suggestions
- [non-blocking improvements]

### ❌ Must Fix
- [blocking issues that need resolution]

### 📋 Missing Tests
- [specific tests that should be added]
```

## Auto-Fix Mode

When called with "fix-first" or from the add-workflow, automatically implement
minimal safe fixes for high-confidence issues:
- Rename properties/methods to match conventions
- Add missing Span overloads
- Add missing Count properties
- Replace default parameters with overloads
- Add missing negative index validation
- Remove XML doc comments
- Switch to file-scoped namespaces
- Add missing test cases

Commit fixes and re-run tests to verify.
