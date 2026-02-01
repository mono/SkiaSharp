---
name: add-api
description: >
  Add new C# APIs to SkiaSharp by wrapping Skia C++ functionality.
  Handles the complete workflow: C++ analysis → C API creation → submodule commits →
  binding generation → C# wrapper → testing.
  
  Use when user asks to:
  - Add/expose new API from Skia
  - Wrap C++ function in C#
  - Add missing method/property
  - Expose Skia functionality
  
  Triggers: "add API", "expose function", "wrap method", "add SKFoo.Bar",
  "implement #NNNN" (when issue requests new API)
---

# Add API Skill

## ❌ NEVER Do These

| Shortcut | Consequence |
|----------|-------------|
| Edit `*.generated.cs` manually | Overwrites on next generation; binding mismatch |
| Skip generator after C API change | C# bindings won't match C API |
| Skip tests | Can't verify functionality works |
| Commit C API without staging submodule | Parent repo won't use your changes |

## Workflow Overview

```
1. Analyze C++ API  →  Find in Skia headers, identify pointer type
2. Add C API        →  Header + impl in externals/skia/
3. Commit Submodule →  Commit IN submodule, then stage in parent
4. Generate         →  Run ./utils/generate.ps1 (MANDATORY)
5. Add C# Wrapper   →  Validate params, call P/Invoke
6. Test             →  Run tests (MANDATORY)
```

👉 **Detailed checklists:** See [references/checklists.md](references/checklists.md)
👉 **If stuck:** See [references/troubleshooting.md](references/troubleshooting.md)

---

## Phase 1: Analyze C++ API

Find the C++ API in Skia headers and identify:

1. **Pointer type** — determines C# wrapper pattern:
   - Raw (`T*`) → `owns: false`
   - Owned → `DisposeNative()` deletes
   - Ref-counted (`sk_sp<T>`) → `ISKReferenceCounted` or `ISKNonVirtualReferenceCounted`

2. **Error handling** — returns null/bool? throws?

3. **Parameters** — primitives, references, pointers, const-ness

👉 See [copilot-instructions.md#pointer-type-decision-tree](../../copilot-instructions.md#pointer-type-decision-tree)

---

## Phase 2: Add C API

**Location:** `externals/skia/include/c/` (header) and `externals/skia/src/c/` (impl)

```cpp
// Header (sk_canvas.h)
SK_C_API void sk_canvas_draw_circle(sk_canvas_t* canvas, float cx, float cy, 
                                     float radius, const sk_paint_t* paint);

// Implementation (sk_canvas.cpp)
void sk_canvas_draw_circle(sk_canvas_t* canvas, float cx, float cy,
                           float radius, const sk_paint_t* paint) {
    AsCanvas(canvas)->drawCircle(cx, cy, radius, *AsPaint(paint));
}
```

**Conversion macros:** `AsType()` unwraps, `ToType()` wraps. Use `sk_ref_sp()` for ref-counted params, `.release()` for ref-counted returns.

---

## Phase 3: Commit Submodule

> **🛑 CRITICAL — This is where changes get lost**

```bash
# Step 1: Commit IN the submodule
cd externals/skia
git config user.email "copilot@github.com"  # first time only
git config user.name "GitHub Copilot"        # first time only
git add include/c/sk_*.h src/c/sk_*.cpp
git commit -m "Add sk_foo_bar to C API"

# Step 2: Stage submodule in parent
cd ..
git add externals/skia

# Step 3: Verify
git status
# Must show: modified: externals/skia (new commits)
```

---

## Phase 4: Generate Bindings

> **🛑 MANDATORY — Never skip, never edit generated files manually**

```bash
pwsh ./utils/generate.ps1
git diff binding/SkiaSharp/SkiaApi.generated.cs  # verify new function appears
```

---

## Phase 5: Add C# Wrapper

**Location:** `binding/SkiaSharp/SK*.cs`

```csharp
// Factory method — returns null on failure
public static SKImage FromPixels(SKImageInfo info, SKData data, int rowBytes)
{
    if (data == null)
        throw new ArgumentNullException(nameof(data));
    var cinfo = SKImageInfoNative.FromManaged(ref info);
    return GetObject(SkiaApi.sk_image_new_raster_data(&cinfo, data.Handle, (IntPtr)rowBytes));
}

// Instance method
public void DrawCircle(float cx, float cy, float radius, SKPaint paint)
{
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));
    SkiaApi.sk_canvas_draw_circle(Handle, cx, cy, radius, paint.Handle);
}
```

**Rules:** Validate nulls, factory→null on fail, constructor→throw on fail, use overloads not defaults (ABI stability).

---

## Phase 6: Test

> **🛑 MANDATORY — Build alone is NOT sufficient**

```csharp
[SkippableFact]
public void DrawCircleWorks()
{
    using var surface = SKSurface.Create(new SKImageInfo(100, 100));
    using var canvas = surface.Canvas;
    using var paint = new SKPaint { Color = SKColors.Red };
    
    canvas.DrawCircle(50, 50, 25, paint);
    
    using var image = surface.Snapshot();
    Assert.NotNull(image);
}
```

```bash
dotnet build binding/SkiaSharp/SkiaSharp.csproj
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj
```

**Tests must pass before claiming completion.**
