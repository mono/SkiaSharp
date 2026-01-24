# SkiaSharp AI Instructions

SkiaSharp is a cross-platform 2D graphics API for .NET wrapping Google's Skia library.

## Architecture

```
C# Wrapper (binding/SkiaSharp/)  →  P/Invoke  →  C API (externals/skia/src/c/)  →  C++ Skia
```

**Key principle:** C# is the safety boundary. C API is minimal pass-through.

## Three Pointer Types

| Type | Examples | C# Pattern | Cleanup |
|------|----------|------------|---------|
| **Raw** | Parameters, getters | `owns: false` | None |
| **Owned** | Canvas, Paint, Path | `DisposeNative()` | Delete |
| **Ref-counted** | Image, Shader, Data | `ISKReferenceCounted` | Unref |

**Identify:** Inherits `SkRefCnt`? → Ref-counted. Mutable? → Owned. Otherwise → Raw.

## Quick Decision Trees

**Pointer type?**
- Inherits SkRefCnt/SkNVRefCnt → Ref-counted
- Mutable (Canvas/Paint/Path) → Owned  
- Parameter or getter → Raw

**Error handling?**
- C API → Pass through (bool/null/void)
- C# → Validate params, check returns, throw exceptions
- Factory methods → Return null on failure
- Constructors → Throw on failure

## Key Directories

| Path | Purpose |
|------|---------|
| `binding/SkiaSharp/` | C# wrappers |
| `externals/skia/src/c/` | C API implementation |
| `externals/skia/include/c/` | C API headers |
| `documentation/` | Architecture & guides |
| `docs/` | ⚠️ Auto-generated, don't edit |

## Build & Generate

```bash
dotnet cake --target=externals-download  # Get native libs
dotnet cake --target=libs                # Build managed
dotnet cake --target=tests               # Run tests
```

Regenerate P/Invoke:
```pwsh
./utils/generate.ps1                            # All bindings
./utils/generate.ps1 -Config libSkiaSharp.json  # Specific config
```

## Threading

⚠️ Skia is NOT thread-safe. Canvas/Paint/Path must be thread-local. Immutable objects (Image/Shader) can be shared.

---

## C API Layer

**Applies to:** `externals/skia/src/c/*.cpp`, `externals/skia/include/c/*.h`

### Style Rules

- Use `SK_C_API` for all exported functions
- Function naming: `sk_<type>_<action>` (e.g., `sk_canvas_draw_rect`)
- Type naming: `sk_<type>_t` (e.g., `sk_canvas_t`)
- Use C types only (no `std::string`, etc.)

### Type Conversion

```cpp
// Use macros from sk_types_priv.h
AsCanvas(sk_canvas_t*) → SkCanvas*
ToCanvas(SkCanvas*)    → sk_canvas_t*

// Dereference for references
AsCanvas(canvas)->drawRect(*AsRect(rect), *AsPaint(paint));
```

### Memory Patterns

```cpp
// Owned: create/destroy pairs
SK_C_API sk_paint_t* sk_paint_new(void);
SK_C_API void sk_paint_delete(sk_paint_t* paint);

// Ref-counted: use sk_ref_sp when C++ expects sk_sp<T>
sk_ref_sp(AsImageFilter(filter))
```

### Rules

- ✅ Pass through directly to C++
- ✅ Document ownership in comments
- ❌ Never throw exceptions
- ❌ Don't validate params (C# does this)
- ❌ Don't use C++ types in signatures

---

## C# Bindings

**Applies to:** `binding/SkiaSharp/*.cs`

### Style Rules

- Class naming: `SKType` (e.g., `SKCanvas`)
- Inherit from `SKObject` for handle management
- Implement `ISKReferenceCounted` for ref-counted types
- Never expose `IntPtr` in public APIs

### Validation Pattern

```csharp
public void DrawRect(SKRect rect, SKPaint paint)
{
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));
    
    SkiaApi.sk_canvas_draw_rect(Handle, &rect, paint.Handle);
}
```

### Memory Patterns

```csharp
// Owned - explicit dispose
protected override void DisposeNative()
{
    SkiaApi.sk_canvas_destroy(Handle);
}

// Ref-counted
public class SKImage : SKObject, ISKReferenceCounted { }

// Non-owning
return GetOrAddObject(handle, owns: false, (h, o) => new SKSurface(h, o));
```

### Factory vs Constructor

```csharp
// Factory: return null on failure
public static SKImage? FromEncodedData(SKData data)
{
    var handle = SkiaApi.sk_image_new_from_encoded(data.Handle);
    return GetObject(handle);  // null if IntPtr.Zero
}

// Constructor: throw on failure
public SKBitmap(SKImageInfo info) : base(IntPtr.Zero, true)
{
    Handle = SkiaApi.sk_bitmap_new();
    if (!SkiaApi.sk_bitmap_try_alloc_pixels(Handle, &info))
        throw new InvalidOperationException("Failed to allocate");
}
```

### Rules

- ✅ Validate all parameters before P/Invoke
- ✅ Check `IntPtr.Zero` returns
- ✅ Follow existing patterns
- ❌ Don't expose IntPtr publicly
- ❌ Don't throw from Dispose

---

## Generated Code

**Applies to:** `*.generated.cs`

⚠️ **DO NOT manually edit.** Changes will be lost.

Regenerate after C API changes:
```pwsh
./utils/generate.ps1
```

- ✅ Add wrappers in separate `.cs` files
- ❌ Don't edit generated files directly

---

## Native Skia

**Applies to:** `externals/skia/**` (excluding `src/c/`, `include/c/`)

⚠️ **Upstream code - DO NOT modify** unless contributing to Google's Skia.

Use for reference when identifying pointer types:
- `sk_sp<T>` → Reference-counted
- Inherits `SkRefCnt` → Reference-counted  
- `const T*` → Non-owning

To create bindings, add C API in `externals/skia/src/c/`.

---

## Tests

**Applies to:** `tests/**/*.cs`

### Test Philosophy

**Tests must FAIL when something is wrong, never skip.**

- Missing dependencies → FAIL with helpful error message
- Missing reference data → FAIL
- Validation fails → FAIL
- Environment not set up → FAIL

The ONLY acceptable skip is for **hardware requirements** that physically cannot be met:
- iOS tests on non-macOS (no iOS SDK available)
- GPU/OpenGL tests on machines without GPU hardware

Everything else must fail. A green test run means everything works.

### Always Use `using`

```csharp
[Fact]
public void DrawRectWorks()
{
    using var bitmap = new SKBitmap(100, 100);
    using var canvas = new SKCanvas(bitmap);
    using var paint = new SKPaint { Color = SKColors.Red };
    
    canvas.DrawRect(new SKRect(10, 10, 90, 90), paint);
    Assert.NotEqual(SKColors.White, bitmap.GetPixel(50, 50));
}
```

### Test Focus

- ✅ Memory management (no leaks)
- ✅ Disposal behavior
- ✅ Null/error handling
- ✅ Edge cases
- ❌ Don't leave objects undisposed

---

## Samples

**Applies to:** `samples/**/*.cs`

### Always Use `using`

```csharp
using var surface = SKSurface.Create(info);
using var canvas = surface.Canvas;
using var paint = new SKPaint { Color = SKColors.Blue };

canvas.Clear(SKColors.White);
canvas.DrawRect(new SKRect(50, 50, 200, 200), paint);
```

- ✅ Complete, self-contained examples
- ✅ Include all necessary usings
- ❌ Don't skip disposal

---

## Documentation

**Applies to:** `*.md` (excluding externals)

- Clear, concise language
- Title + 1-2 sentence summary at top
- Show disposal in code examples
- Use tables for comparisons

---

## Documentation Links

| Document | Content |
|----------|---------|
| [architecture.md](../documentation/architecture.md) | Three-layer design, threading |
| [memory-management.md](../documentation/memory-management.md) | Pointer types, ownership |
| [error-handling.md](../documentation/error-handling.md) | Error patterns |
| [adding-apis.md](../documentation/adding-apis.md) | Step-by-step binding guide |
| [building.md](../documentation/building.md) | Build instructions |

---

**Remember:** Three layers, three pointer types, C# validates, C API trusts.
