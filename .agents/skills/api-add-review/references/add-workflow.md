# Add API Workflow

Step-by-step workflow for adding new C# APIs to SkiaSharp by wrapping Skia C++
functionality. After completing all phases, the review-workflow runs automatically
to verify the API meets design standards.

Read [api-design-rules.md](api-design-rules.md) before starting — it defines
naming, Span patterns, property conventions, and test requirements.

## Before Starting (MANDATORY)

- [ ] **NOT on `main` or `skiasharp` branch** — Check with `git branch`
- [ ] **Created feature branch:** `git checkout -b dev/issue-NNNN-description`
- [ ] **Submodule on feature branch:** `cd externals/skia && git checkout -b dev/issue-NNNN-description`

## Phase 1: Analyze C++ API

Find the C++ API in Skia headers and identify:

- [ ] Found C++ API in Skia headers
- [ ] Identified pointer type: raw / owned / ref-counted (virtual or non-virtual)
- [ ] Noted error handling: returns null? returns bool? can throw?
- [ ] Documented parameter types and const-ness
- [ ] Checked if any params are `SkFourByteTag` or similar typedefs that need wrapping
- [ ] Checked if return is an array/count pattern (two-call: null first, then fill)
- [ ] Checked HarfBuzz docs if wrapping HB APIs — verify `[inout]` vs return value semantics

## Phase 2: Add C API

- [ ] Header in `externals/skia/include/c/sk_*.h` with `SK_C_API`
- [ ] Implementation in `externals/skia/src/c/sk_*.cpp`
- [ ] Uses `AsType()`/`ToType()` conversion macros from `sk_types_priv.h`
- [ ] Ref-counted parameters use `sk_ref_sp()`
- [ ] Ref-counted returns use `.release()`

### Struct conversion patterns

For **layout-compatible** C/C++ structs (same field types and order):
```cpp
// In sk_types_priv.h — generates As*/To* via reinterpret_cast
#include "include/c/sk_typeface.h"
DEF_MAP(SkFontArguments::VariationPosition::Coordinate,
        sk_fontarguments_variation_position_coordinate_t,
        VariationPositionCoordinate)

// In sk_structs.cpp — compile-time size check
static_assert(sizeof(sk_fontarguments_variation_position_coordinate_t) ==
    sizeof(SkFontArguments::VariationPosition::Coordinate),
    ASSERT_MSG(SkFontArguments::VariationPosition::Coordinate,
               sk_fontarguments_variation_position_coordinate_t));
```

For **non-layout-compatible** structs (e.g., isHidden() getter vs bool field):
```cpp
// Manual converter
static inline sk_fontarguments_variation_axis_t ToVariationAxis(
    const SkFontParameters::Variation::Axis& axis) {
    return { axis.tag, axis.min, axis.def, axis.max, axis.isHidden() };
}
```

For **parameter bag helpers**:
```cpp
static inline SkFontArguments AsSkFontArguments(
    const sk_fontarguments_variation_position_coordinate_t* coordinates,
    int coordinateCount, int collectionIndex) {
    SkFontArguments args;
    args.setCollectionIndex(collectionIndex);
    args.setVariationDesignPosition(
        {AsVariationPositionCoordinate(coordinates), coordinateCount});
    return args;
}
```

### New typedefs
For uint32_t-based types like tags:
```cpp
typedef uint32_t sk_fourbytetag_t;
```
Then map in `libSkiaSharp.json`:
```json
"sk_fourbytetag_t": { "cs": "SKFourByteTag" }
```

## Phase 3: Commit Submodule

- [ ] Changes committed IN submodule (`cd externals/skia && git commit`)
- [ ] Submodule staged in parent (`git add externals/skia`)
- [ ] Verified: `git status` shows "modified: externals/skia (new commits)"

## Phase 4: Generate Bindings

- [ ] Ran `pwsh ./utils/generate.ps1`
- [ ] Verified `SkiaApi.generated.cs` contains new function
- [ ] Did NOT manually edit any `*.generated.cs` file
- [ ] Verified JSON config maps new types and members correctly

If HarfBuzz headers changed, ensure the correct version is checked out for generation
(may differ from build version — check DEPS).

## Phase 5: Add C# Wrapper

Apply the rules from [api-design-rules.md](api-design-rules.md):

- [ ] Properties for parameterless array getters
- [ ] Count properties alongside array properties
- [ ] Span overloads for all array APIs
- [ ] ReadOnlySpan for input parameters
- [ ] Overloads not default parameters
- [ ] Ref struct parameter bag if many optional params
- [ ] Common-case shortcut overloads
- [ ] File-scoped namespace
- [ ] No XML doc comments
- [ ] Null validation with ArgumentNullException
- [ ] Index validation with ArgumentOutOfRangeException
- [ ] Wrapper type for typedefs (Parse, ToString, implicit conversion)

## Phase 6: Build Native

Since you added/modified C API functions, you MUST rebuild:

```bash
# macOS (Apple Silicon)
dotnet cake --target=externals-macos --arch=arm64

# If GN is killed (error 137), re-sign: codesign --force --sign - externals/skia/bin/gn
```

## Phase 7: Write Tests

Follow the test requirements from [api-design-rules.md](api-design-rules.md):

- [ ] Exact value assertions for known test fonts
- [ ] Span vs property equivalence
- [ ] Interop round-trip (data survives P/Invoke)
- [ ] Static/empty font graceful handling
- [ ] Negative index validation
- [ ] Count matches array length
- [ ] Type-specific tests (Parse, ToString, equality, edge cases)
- [ ] Rendering tests where applicable
- [ ] Dedicated test file for standalone types

## Phase 8: Run Tests

```bash
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj
```

Tests MUST PASS. Do not skip, do not claim completion if they fail.

## Phase 9: Review

Run the [review-workflow.md](review-workflow.md) against your changes.
Fix any issues it identifies. Re-run tests after fixes.
