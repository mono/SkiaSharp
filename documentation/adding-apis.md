# Adding APIs

This guide walks through the process of exposing new Skia C++ functionality in C#.

The C API layer is maintained in the [mono/skia](https://github.com/mono/skia) fork. Changes are upstreamed to Google when appropriate.

## Process Overview

```
1. Find C++ API     →  Identify pointer type & error handling
2. Add C API        →  Header + implementation
3. Regenerate       →  Run generator to create P/Invoke
4. Add C# Wrapper   →  Validate, call, handle errors
```

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

**Header** (`sk_canvas.h`):
```cpp
SK_C_API void sk_canvas_draw_circle(sk_canvas_t* canvas, float cx, float cy, 
                                     float radius, const sk_paint_t* paint);
```

**Implementation** (`sk_canvas.cpp`):
```cpp
void sk_canvas_draw_circle(sk_canvas_t* canvas, float cx, float cy,
                           float radius, const sk_paint_t* paint) {
    AsCanvas(canvas)->drawCircle(cx, cy, radius, *AsPaint(paint));
}
```

### Step 3: Regenerate P/Invoke

Run the generator to create P/Invoke declarations from C API headers:

```pwsh
./utils/generate.ps1
```

This generates in `SkiaApi.generated.cs`:
```csharp
[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
public static extern void sk_canvas_draw_circle(sk_canvas_t canvas, float cx, 
                                                 float cy, float radius, sk_paint_t paint);
```

### Step 4: Add C# Wrapper

```csharp
public void DrawCircle(float cx, float cy, float radius, SKPaint paint) {
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));
    SkiaApi.sk_canvas_draw_circle(Handle, cx, cy, radius, paint.Handle);
}
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

## Checklist

**C++ Analysis:**
- [ ] Identified pointer type (raw/owned/ref-counted)
- [ ] Checked error conditions

**C API:**
- [ ] Header declaration with `SK_C_API`
- [ ] Implementation uses `AsType()`/`ToType()` macros
- [ ] Ref-counted params use `sk_ref_sp()`
- [ ] Ref-counted returns use `.release()`

**C#:**
- [ ] Regenerated P/Invoke (`./utils/generate.ps1`)
- [ ] Validates null parameters
- [ ] Checks return values (factory→null, constructor→throw)
- [ ] Correct ownership (`owns: true/false`)

## Build & Test

```pwsh
./utils/generate.ps1              # Regenerate P/Invoke
dotnet cake --target=libs         # Build
dotnet cake --target=tests        # Test
```
