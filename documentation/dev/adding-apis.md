# Adding APIs

This guide walks through the process of exposing new Skia C++ functionality in C#.

The C API layer is maintained in the [mono/skia](https://github.com/mono/skia) fork. Changes are upstreamed to Google when appropriate.

## Process Overview

```
1. Find C++ API     →  Identify pointer type & error handling
2. Add C API        →  Header + implementation in externals/skia submodule
3. Commit Submodule →  Git workflow for submodule changes (CRITICAL)
4. Regenerate       →  Run generator to create P/Invoke (MANDATORY)
5. Add C# Wrapper   →  Validate, call, handle errors
6. Test             →  Write and run tests (MANDATORY)
```

## ⚠️ Critical: Submodule Workflow

The `externals/skia/` directory is a **git submodule** (separate repository). Changes require special git handling.

### Submodule Git Commands

**Step 1: Make changes in the submodule**
```bash
cd externals/skia

# Configure git (first time only)
git config user.email "your-email@example.com"
git config user.name "Your Name"

# Edit files
# ... edit include/c/sk_*.h and src/c/sk_*.cpp ...

# Commit in submodule
git add include/c/sk_*.h src/c/sk_*.cpp
git commit -m "Add sk_foo_bar to C API"
```

**Step 2: Update parent repo to use new submodule commit**
```bash
cd ../..  # Return to SkiaSharp repo root

# Stage the submodule reference
git add externals/skia

# Verify submodule is staged
git status
# Should show: modified:   externals/skia (new commits)
```

**Step 3: Commit everything together in parent repo**
```bash
# Stage your C# changes too
git add binding/SkiaSharp/ tests/Tests/

# Commit everything
git commit -m "Add SKFoo.Bar API with C API changes"
```

### What NOT to Do

| ❌ WRONG | Why It Fails |
|---------|-------------|
| Edit C API but don't commit in submodule | Changes lost when submodule resets |
| Commit in submodule but don't `git add externals/skia` | Parent repo doesn't know about your C API changes |
| Skip running `pwsh ./utils/generate.ps1` | C# bindings won't match C API |
| Only build, don't test | Can't verify functionality actually works |

## Simple Example: SKPaint.IsAntialias

**C++ API** (`SkPaint.h`):
```cpp
bool isAntiAlias() const;
void setAntiAlias(bool aa);
```

**C API** (`sk_paint.cpp`):
```cpp
bool sk_paint_is_antialias(const sk_paint_t* paint) {
    return AsPaint(paint)->isAntiAlias();
}
void sk_paint_set_antialias(sk_paint_t* paint, bool aa) {
    AsPaint(paint)->setAntiAlias(aa);
}
```

**P/Invoke** (generated in `SkiaApi.generated.cs` after running generator):
```csharp
[DllImport(SKIA)] public static extern bool sk_paint_is_antialias(sk_paint_t t);
[DllImport(SKIA)] public static extern void sk_paint_set_antialias(sk_paint_t t, bool aa);
```

**C# Wrapper** (`SKPaint.cs`):
```csharp
public bool IsAntialias {
    get => SkiaApi.sk_paint_is_antialias(Handle);
    set => SkiaApi.sk_paint_set_antialias(Handle, value);
}
```

## Complete Example: DrawCircle

### Step 1: Find C++ API

```cpp
// SkCanvas.h
void drawCircle(SkScalar cx, SkScalar cy, SkScalar radius, const SkPaint& paint);
```

Analysis: Canvas (owned), paint (borrowed), primitives, void return.

### Step 2: Add C API

**Header** (`externals/skia/include/c/sk_canvas.h`):
```cpp
SK_C_API void sk_canvas_draw_circle(sk_canvas_t* canvas, float cx, float cy, 
                                     float radius, const sk_paint_t* paint);
```

**Implementation** (`externals/skia/src/c/sk_canvas.cpp`):
```cpp
void sk_canvas_draw_circle(sk_canvas_t* canvas, float cx, float cy,
                           float radius, const sk_paint_t* paint) {
    AsCanvas(canvas)->drawCircle(cx, cy, radius, *AsPaint(paint));
}
```

### Step 3: Commit Submodule

```bash
# Commit in submodule first
cd externals/skia
git add include/c/sk_canvas.h src/c/sk_canvas.cpp
git commit -m "Add sk_canvas_draw_circle to C API"

# Stage submodule in parent
cd ../..
git add externals/skia
```

### Step 4: Regenerate P/Invoke

Run the generator to create P/Invoke declarations from C API headers:

```pwsh
pwsh ./utils/generate.ps1
```

This generates in `SkiaApi.generated.cs`:
```csharp
[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
public static extern void sk_canvas_draw_circle(sk_canvas_t canvas, float cx, 
                                                 float cy, float radius, sk_paint_t paint);
```

### Step 5: Add C# Wrapper

```csharp
public void DrawCircle(float cx, float cy, float radius, SKPaint paint) {
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));
    SkiaApi.sk_canvas_draw_circle(Handle, cx, cy, radius, paint.Handle);
}
```

### Step 6: Test

```bash
dotnet build binding/SkiaSharp/SkiaSharp.csproj
dotnet test tests/SkiaSharp.Tests.Console.sln
```

## Reference-Counted Return Example

When returning ref-counted objects, use `.release()` and `sk_ref_sp()`:

```cpp
// C API - returning ref-counted object
sk_image_t* sk_image_new_from_encoded(const sk_data_t* data) {
    // sk_ref_sp increments ref count, .release() transfers ownership
    return ToImage(SkImages::DeferredFromEncodedData(sk_ref_sp(AsData(data))).release());
}
```

```csharp
// C# - factory returns null on failure
public static SKImage FromEncodedData(SKData data) {
    if (data == null) throw new ArgumentNullException(nameof(data));
    return GetObject(SkiaApi.sk_image_new_from_encoded(data.Handle));
}
```

## Complete Implementation Checklist

**Phase 1: C++ Analysis**
- [ ] Identified pointer type (raw/owned/ref-counted)
- [ ] Checked error conditions

**Phase 2: C API in Submodule**
- [ ] Header declaration with `SK_C_API` in `externals/skia/include/c/sk_*.h`
- [ ] Implementation in `externals/skia/src/c/sk_*.cpp` uses `AsType()`/`ToType()` macros
- [ ] Ref-counted params use `sk_ref_sp()`
- [ ] Ref-counted returns use `.release()`

**Phase 3: Commit Submodule**
- [ ] **Configured git in submodule** (`git config user.email/name`)
- [ ] **Committed changes in submodule** (`cd externals/skia && git commit`)
- [ ] **Staged submodule in parent repo** (`cd ../.. && git add externals/skia`)

**Phase 4: Regenerate Bindings**
- [ ] **Regenerated P/Invoke** (`pwsh ./utils/generate.ps1`) — **MANDATORY, never skip**
- [ ] Verified `SkiaApi.generated.cs` updated correctly

**Phase 5: C# Wrapper**
- [ ] Added C# wrapper method
- [ ] Validates null parameters
- [ ] Checks return values (factory→null, constructor→throw)
- [ ] Correct ownership (`owns: true/false`)

**Phase 6: Tests (MANDATORY)**
- [ ] Added test methods in `tests/Tests/SkiaSharp/`
- [ ] Tests follow existing patterns (`[SkippableFact]`, `using` statements)
- [ ] **Ran tests and verified they pass** — **NOT OPTIONAL**
- [ ] All assertions pass
- [ ] Build succeeds: `dotnet build binding/SkiaSharp/SkiaSharp.csproj`
- [ ] Both submodule and C# changes committed together

## Build & Test Commands

```bash
# Step 1: Build native library (REQUIRED after C API changes)
dotnet cake --target=externals-macos --arch=arm64    # macOS Apple Silicon
dotnet cake --target=externals-android --arch=arm64  # Android ARM64

# Step 2: Regenerate P/Invoke (after C API changes)
pwsh ./utils/generate.ps1

# Step 3: Build C# bindings
dotnet build binding/SkiaSharp/SkiaSharp.csproj

# Step 4: Run tests (MANDATORY - not optional)
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj
```

> **Note:** If `bin/gn` is killed with error 137 after a Docker WASM build, re-sign it:
> `codesign --force --sign - externals/skia/bin/gn`

---

## C API Struct Conversion Patterns

### DEF_MAP Macros (sk_types_priv.h)

The C API uses macros to generate bidirectional conversion functions between C and C++
types. Each generates 8 inline functions: `As*()` (C→C++) and `To*()` (C++→C) in
const/non-const × reference/pointer variants.

| Macro | Forward Declaration | When to Use |
|-------|-------------------|-------------|
| `DEF_CLASS_MAP(SkType, sk_type, Name)` | `class SkType;` | Opaque class types (SkCanvas, SkPaint) |
| `DEF_CLASS_MAP_WITH_NS(Ns, SkType, sk_type, Name)` | `class SkType;` in namespace | Namespaced classes (skottie::Animation) |
| `DEF_STRUCT_MAP(SkType, sk_type, Name)` | `struct SkType;` | POD structs not yet included (SkRect, SkPoint) |
| `DEF_MAP(SkType, sk_type, Name)` | none | Types already declared via #include |

```cpp
// For a class — forward-declares the class
DEF_CLASS_MAP(SkCanvas, sk_canvas_t, Canvas)
// Generates: AsCanvas(), ToCanvas()

// For a namespaced class
DEF_CLASS_MAP_WITH_NS(skottie, Animation, skottie_animation_t, SkottieAnimation)
// Generates: AsSkottieAnimation(), ToSkottieAnimation()

// For a POD struct — forward-declares the struct
DEF_STRUCT_MAP(SkRect, sk_rect_t, Rect)
// Generates: AsRect(), ToRect()

// For an already-included type (nested class, etc.)
#include "include/core/SkFontArguments.h"
DEF_MAP(SkFontArguments::VariationPosition::Coordinate,
        sk_fontarguments_variation_position_coordinate_t,
        VariationPositionCoordinate)
```

### Layout-Compatible vs Manual Conversion

If the C struct has identical layout to the C++ type, use `DEF_MAP` (reinterpret_cast).
If the layout differs (e.g., C++ uses a getter method where C has a bool field), write
a manual converter:

```cpp
// Layout-compatible — use DEF_MAP
DEF_MAP(SkFontArguments::VariationPosition::Coordinate,
        sk_fontarguments_variation_position_coordinate_t,
        VariationPositionCoordinate)

// Not layout-compatible — manual converter
// (SkFontParameters::Variation::Axis uses isHidden() getter + uint16_t flags,
//  but C struct has a bool field)
static inline sk_fontarguments_variation_axis_t ToVariationAxis(
    const SkFontParameters::Variation::Axis& axis) {
    return { axis.tag, axis.min, axis.def, axis.max, axis.isHidden() };
}
```

### Parameter Bag Helpers

For complex functions that bundle multiple C structs into one C++ object:

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

### ABI Verification (sk_structs.cpp)

Every layout-compatible struct mapping **must** have a `static_assert` in
`externals/skia/src/c/sk_structs.cpp` to catch ABI drift at compile time:

```cpp
static_assert(sizeof(sk_fontarguments_variation_position_coordinate_t) ==
    sizeof(SkFontArguments::VariationPosition::Coordinate),
    ASSERT_MSG(SkFontArguments::VariationPosition::Coordinate,
               sk_fontarguments_variation_position_coordinate_t));
```

### New Typedefs

For simple `uint32_t` types (tags, identifiers), define a C typedef:

```cpp
// In the header (sk_typeface.h)
typedef uint32_t sk_fourbytetag_t;
```

Then map it in `libSkiaSharp.json` so the generator uses the C# wrapper type:

```json
"sk_fourbytetag_t": { "cs": "SKFourByteTag" }
```

---

## Generator Configuration (libSkiaSharp.json)

The generator reads `binding/libSkiaSharp.json` to map C types to C# types.

### Type Aliases

```json
"sk_color_t": { "cs": "UInt32" },
"sk_fourbytetag_t": { "cs": "SKFourByteTag" }
```

### Struct Mapping with Member Renames

```json
"sk_fontarguments_variation_axis_t": {
    "cs": "SKFontVariationAxis",
    "members": {
        "def": "Default",
        "isHidden": "IsHidden"
    }
}
```

The `members` dictionary is `Dictionary<string, string>` — name remapping only.
To change a field's C# type, define a C typedef and map it at the type level.

### Struct Options

| Option | Effect | Example |
|--------|--------|---------|
| `"cs"` | C# type name | `"SKFontVariationAxis"` |
| `"members"` | Rename fields | `{ "def": "Default" }` |
| `"internal"` | Hide from public API | GPU native structs |
| `"properties": false` | Generate private fields only | Complex structs with manual properties |
| `"readonly"` | Generate readonly struct | Immutable value types |
| `"flags"` | Generate `[Flags]` enum | Bitfield enums |

### Function Parameter Overrides

Override specific parameters by index (0-based, -1 for return type):

```json
"sk_fontmgr_legacy_create_typeface": {
    "parameters": {
        "1": "IntPtr"
    }
}
```

String marshaling:
```json
"gr_glinterface_has_extension": {
    "parameters": {
        "1": "[MarshalAs (UnmanagedType.LPStr)] String"
    }
}
```

Delegate overrides:
```json
"gr_vk_func_ptr": {
    "convention": "stdcall",
    "generateProxy": false
}
```

---

## GPU-Specific Patterns

GPU types use the `GR` prefix and are guarded by `#if defined(SK_GANESH)` in the
C API and by platform-conditional compilation in C#.

### Prefix Conventions

| Prefix | Meaning | Examples |
|--------|---------|----------|
| `SK` | Core SkiaSharp | `SKCanvas`, `SKPaint` |
| `GR` | GPU rendering | `GRContext`, `GRBackendTexture` |
| `GRGl` | OpenGL-specific | `GRGlInterface`, `GRGlTextureInfo` |
| `GRVk` | Vulkan-specific | `GRVkBackendContext`, `GRVkExtensions` |
| `GRMtl` | Metal-specific | `GRMtlBackendContext`, `GRMtlTextureInfo` |
| `GRD3D` | Direct3D 12-specific | `GRD3DBackendContext` |

### C API Guards

```cpp
// sk_types_priv.h
#if defined(SK_GANESH)
  DEF_STRUCT_MAP(GrGLTextureInfo, gr_gl_textureinfo_t, GrGLTextureInfo)

  #if defined(SK_VULKAN)
    DEF_MAP_WITH_NS(skgpu, VulkanYcbcrConversionInfo, ...)
  #endif

  #if defined(SK_DIRECT3D)
    DEF_STRUCT_MAP(GrD3DBackendContext, ...)
  #endif
#endif
```

### C# Platform Guards

```csharp
// Metal — Apple platforms only
#if __IOS__ || __MACOS__ || __TVOS__
    public IMTLDevice Device { get; set; }
#endif
```

### JSON Mapping for GPU Types

GPU native structs are typically `internal`:
```json
"gr_vk_backendcontext_t": { "cs": "GRVkBackendContextNative", "internal": true },
"gr_mtl_textureinfo_t": { "cs": "GRMtlTextureInfoNative", "internal": true }
```

Public GPU structs get member renames:
```json
"gr_gl_textureinfo_t": {
    "cs": "GRGlTextureInfo",
    "members": { "fID": "Id" }
}
```

---

## Common Mistakes

| Mistake | Impact | Solution |
|---------|--------|----------|
| Edited C API but didn't commit in submodule | Changes disappear on next submodule update | Always commit inside `externals/skia/` first |
| Forgot `git add externals/skia` in parent | Parent repo doesn't reference your C API changes | Stage submodule after committing inside it |
| Manually edited `*.generated.cs` | Binding mismatch, overwrites on next generation | Always run `pwsh ./utils/generate.ps1` |
| Only built, didn't test | Functionality may not work despite compiling | Always run tests — passing tests required |
| Used `externals-download` after C API change | Downloaded natives don't have your new functions | Use `externals-{platform}` to build |
| No `static_assert` for DEF_MAP struct | ABI mismatch goes undetected | Add to `sk_structs.cpp` |
| Missing `#include` in `sk_types_priv.h` | C struct types not declared | Include the C header before DEF_MAP |
| Generator or tests fail | Incomplete implementation | Retry once; if still failing, stop and investigate |
