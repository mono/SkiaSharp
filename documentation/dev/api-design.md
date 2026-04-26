# API Design Guidelines

This guide defines patterns for creating consistent, stable APIs in SkiaSharp.

## Core Principles

1. **ABI Stability** - Never break existing code
2. **Consistency** - Follow established patterns
3. **Safety** - Validate early, fail predictably
4. **Discoverability** - APIs should be easy to find and use

---

## ABI Stability (Non-Negotiable)

SkiaSharp maintains **stable ABI across versions**. Breaking changes break downstream applications at runtime without recompilation.

### Allowed Changes

| Change Type | Example |
|-------------|---------|
| ✅ Add new overloads | `CreateCopy(byte[] bytes, ulong length)` |
| ✅ Add new methods | `NewFeatureMethod()` |
| ✅ Add new classes | `public class SKNewFeature` |
| ✅ Add new properties | `public bool NewProperty { get; }` |
| ✅ Add optional interfaces | Class can implement new interface |

### Forbidden Changes

| Change Type | Why It Breaks |
|-------------|---------------|
| ❌ Modify existing signatures | Callers compiled against old signature fail |
| ❌ Remove public APIs | Even deprecated - breaks callers |
| ❌ Change return types | Even to derived types - ABI mismatch |
| ❌ Change parameter types | Breaks overload resolution |
| ❌ Reorder parameters | Same signature, different meaning |
| ❌ Add required parameters | Existing callers don't provide them |

### Deprecation Pattern

When an API needs to be replaced, use `[Obsolete]` but **never remove**:

```csharp
// ✅ CORRECT - Add obsolete, provide alternative
[Obsolete("Use ToShader(SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling) instead.")]
public SKShader ToShader(SKShaderTileMode tmx, SKShaderTileMode tmy) =>
    ToShader(tmx, tmy, SKSamplingOptions.Default);

// Add the new preferred method alongside
public SKShader ToShader(SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling) =>
    // implementation
```

---

## Naming Conventions

### Types

| Type | Convention | Examples |
|------|------------|----------|
| Classes | `SK` prefix + PascalCase | `SKCanvas`, `SKPaint`, `SKImage` |
| Structs | `SK` prefix + PascalCase | `SKRect`, `SKPoint`, `SKColor` |
| Enums | `SK` prefix + PascalCase | `SKBlendMode`, `SKFilterMode` |
| Interfaces | `ISK` prefix | `ISKReferenceCounted` |

### Members

| Member | Convention | Examples |
|--------|------------|----------|
| Methods | PascalCase, verb phrase | `DrawRect()`, `CreateFromData()` |
| Properties | PascalCase, noun/adjective | `Width`, `IsDisposed`, `Handle` |
| Parameters | camelCase | `sourceRect`, `destPoint`, `filterQuality` |
| Private fields | camelCase | `handle`, `isDisposed`, `referenceCount` |
| Constants | PascalCase | `DefaultDpi`, `MaxTextureSize` |

### Factory Method Naming

| Prefix | When to Use | Returns on Failure |
|--------|-------------|-------------------|
| `Create` | Creates new instance | `null` |
| `From*` | Converts/wraps existing data | `null` |
| `Decode` | Parses formatted data | `null` |
| `Try*` | Operation that may fail | `bool` + out param |

```csharp
// Create - new instance from parameters
public static SKSurface Create(SKImageInfo info);

// From* - convert existing data
public static SKImage FromEncodedData(SKData data);
public static SKData FromStream(Stream stream);

// Decode - parse formatted content
public static SKCodec DecodeStream(Stream stream);

// Try* - explicit failure handling
public bool TryAllocPixels(SKImageInfo info);
```

---

## Method Design

### Overloading vs Optional Parameters

**Prefer overloads** over optional/default parameters:

```csharp
// ✅ PREFERRED - Overload chain (from SKData.CreateCopy)
public static SKData CreateCopy(byte[] bytes) =>
    CreateCopy(bytes, (ulong)bytes.Length);

public static SKData CreateCopy(byte[] bytes, ulong length)
{
    fixed (byte* b = bytes) {
        return GetObject(SkiaApi.sk_data_new_with_copy(b, (IntPtr)length));
    }
}

// ❌ AVOID - Default parameters
public static SKData CreateCopy(byte[] bytes, ulong length = 0)
```

**Why:**
- Overloads are CLS-compliant (some languages don't support defaults)
- Adding overloads doesn't break ABI; changing defaults does
- Clearer what the "default" behavior is

### Overload Chain Pattern

Simpler overloads should delegate to the most complete version:

```csharp
// Simplest - convenience for int
public static SKData CreateCopy(IntPtr bytes, int length) =>
    CreateCopy(bytes, (ulong)length);

// Intermediate - convenience for long  
public static SKData CreateCopy(IntPtr bytes, long length) =>
    CreateCopy(bytes, (ulong)length);

// Most complete - core implementation
public static SKData CreateCopy(IntPtr bytes, ulong length)
{
    if (!PlatformConfiguration.Is64Bit && length > UInt32.MaxValue)
        throw new ArgumentOutOfRangeException(nameof(length));
    return GetObject(SkiaApi.sk_data_new_with_copy((void*)bytes, (IntPtr)length));
}
```

### Boolean Parameters

**Avoid boolean parameters** when the meaning isn't obvious at the call site:

```csharp
// ❌ AVOID - Unclear what 'true' means
image.ToRasterImage(true);

// ✅ BETTER - Named parameter makes intent clear
image.ToRasterImage(ensurePixelData: true);
```

If a method needs multiple boolean options, consider:
1. Named parameters (C# callers can use them)
2. Options enum with `[Flags]`
3. Options struct/class

### Parameter Order

Standard parameter ordering:
1. Required inputs (most important first)
2. Optional inputs
3. Out/ref parameters last

```csharp
// Good order: source → destination → options
public bool ScalePixels(SKPixmap source, SKPixmap destination, SKSamplingOptions sampling);
```

---

## Error Handling

### Factory Methods Return Null

Factory methods (`Create`, `From*`, `Decode`) return `null` on failure - they do **NOT** throw:

```csharp
public static SKImage FromEncodedData(SKData data)
{
    if (data == null)
        throw new ArgumentNullException(nameof(data));
    
    var handle = SkiaApi.sk_image_new_from_encoded(data.Handle);
    return GetObject(handle);  // Returns null if handle is IntPtr.Zero
}
```

### Constructors Throw

Constructors throw on failure (can't return null):

```csharp
public SKBitmap(SKImageInfo info) : base(IntPtr.Zero, true)
{
    Handle = SkiaApi.sk_bitmap_new();
    if (Handle == IntPtr.Zero)
        throw new InvalidOperationException("Failed to create bitmap");
    
    if (!SkiaApi.sk_bitmap_try_alloc_pixels(Handle, &info))
    {
        SkiaApi.sk_bitmap_destructor(Handle);
        Handle = IntPtr.Zero;
        throw new InvalidOperationException("Failed to allocate pixels");
    }
}
```

### Validation Pattern

Validate parameters at the C# layer, not in C API:

```csharp
public void DrawRect(SKRect rect, SKPaint paint)
{
    // Validate BEFORE P/Invoke
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));
    
    // Call native - C API trusts us
    SkiaApi.sk_canvas_draw_rect(Handle, &rect, paint.Handle);
}
```

### Exception Types

| Exception | When |
|-----------|------|
| `ArgumentNullException` | Null parameter that cannot be null |
| `ArgumentOutOfRangeException` | Value outside valid range |
| `ArgumentException` | Other parameter validation failures |
| `ObjectDisposedException` | Operation on disposed object |
| `InvalidOperationException` | Operation failed (catch-all) |

---

## Memory Management

### Same-Instance Returns

Some Skia methods return the **same instance** as an optimization. Always check before disposing:

```csharp
// ❌ WRONG - crashes if methods return same instance
using var source = GetImage();
var result = source.Subset(bounds);
return result;  // source disposed, but result IS source!

// ✅ CORRECT - check first
var source = GetImage();
var result = source.Subset(bounds);
if (result != source)
    source.Dispose();
return result;
```

**Methods that may return same instance:**
- `Subset()` - when subset equals full bounds
- `ToRasterImage()` - when already a raster image  
- `ToRasterImage(false)` - when already non-texture

### Ownership Transfer

When implementing factory methods that take ownership:

```csharp
// When SKImage takes ownership of data, use owns: true
internal static SKImage FromPixels(IntPtr pixels, int length, Action<IntPtr> releaseProc)
{
    // Data will be released when image is disposed
    return GetObject(SkiaApi.sk_image_new_from_raster(...), owns: true);
}
```

---

## Style Guide

### Code Formatting

From `.editorconfig`:
- **Indentation:** Tabs (4 spaces wide) for `.cs` files
- **Braces:** Allman style (new line before opening brace)
- **Using var:** Preferred everywhere (`csharp_style_var_elsewhere = true`)
- **Final newline:** Required
- **Trailing whitespace:** Trimmed

### Expression Bodies

| Member | Style |
|--------|-------|
| Properties | Expression body ✅ |
| Indexers | Expression body ✅ |
| Accessors | Expression body ✅ |
| Methods | Expression body for simple ones ✅ |
| Constructors | Block body preferred |
| Operators | Expression body ✅ |

```csharp
// Properties - expression body
public int Width => info.Width;

// Simple methods - expression body
public SKRect GetBounds() => new SKRect(0, 0, Width, Height);

// Complex methods - block body
public void Draw(SKCanvas canvas)
{
    canvas.Save();
    // multiple statements
    canvas.Restore();
}

// Constructors - block body preferred
public SKBitmap(int width, int height)
{
    Handle = SkiaApi.sk_bitmap_new();
    TryAllocPixels(new SKImageInfo(width, height));
}
```

### Nullable Reference Types

The codebase is transitioning to nullable reference types:
- **`binding/`** - Currently `#nullable disable` (older code)
- **`source/`** - Uses `#nullable enable` (newer code)

When adding new code, follow the pattern of the file you're editing.

---

## Type Design

### Structs vs Classes

| Use Struct When | Use Class When |
|-----------------|----------------|
| Immutable value type | Mutable or has identity |
| Small (<16 bytes typically) | Contains native handle |
| No inheritance needed | Needs inheritance/polymorphism |
| Frequently allocated | Long-lived, shared instances |

**SkiaSharp examples:**
- `SKRect`, `SKPoint`, `SKColor` → Structs (immutable values)
- `SKCanvas`, `SKPaint`, `SKImage` → Classes (native handle, lifecycle)

### Enum Design

```csharp
// Standard enum - mutually exclusive values
public enum SKBlendMode
{
    Clear,
    Src,
    Dst,
    // ...
}

// Flags enum - combinable values
[Flags]
public enum SKFontStyleSlant
{
    None = 0,
    Upright = 1,
    Italic = 2,
    Oblique = 4,
}
```

---

## Thread Safety

Skia is **NOT thread-safe**. Document thread safety requirements clearly:

```csharp
/// <summary>
/// Draws a rectangle on the canvas.
/// </summary>
/// <remarks>
/// This method is not thread-safe. Canvas operations must be performed
/// on a single thread or with external synchronization.
/// </remarks>
public void DrawRect(SKRect rect, SKPaint paint)
```

**Thread-safe objects:** `SKData`, `SKImage`, `SKShader`, `SKColorFilter` (immutable)

**NOT thread-safe:** `SKCanvas`, `SKPaint`, `SKPath`, `SKBitmap` (mutable)

---

## Span Overloads

Every API that returns an array should also provide a `Span<T>` fill overload for
allocation-free usage, plus a count property for pre-allocation:

```csharp
// Array property — convenient, allocates
public SKFontVariationAxis[] VariationDesignParameters { get { ... } }

// Count property — for pre-allocating span buffers
public int VariationDesignParameterCount => ...;

// Span overload — allocation-free, returns items written
public int GetVariationDesignParameters(Span<SKFontVariationAxis> axes) { ... }
```

Every API that accepts an array should use `ReadOnlySpan<T>` instead, which accepts
arrays, spans, and `stackalloc`:

```csharp
// Accepts array, span, or stackalloc
public SKTypeface Clone(ReadOnlySpan<SKFontVariationPositionCoordinate> position) { ... }
```

---

## Properties vs Methods

Parameterless getters that return arrays or simple values should be **properties**.
If a method takes parameters (including Span buffers), it stays a **method**.

| Pattern | Example | Rationale |
|---------|---------|-----------|
| Property | `Face.Tables`, `Face.VariationAxisInfos`, `Face.GlyphCount` | No parameters, returns data |
| Method | `GetVariationAxisInfos(Span<>)`, `GetTableData(tag)` | Takes parameters |
| Property | `VariationCoordsNormalized` | No parameters, returns `int[]` |
| Method | `GetVariationCoordsNormalized(Span<int>)` | Span fill buffer |

Existing patterns to follow: `Buffer.GlyphInfos`, `Buffer.GlyphPositions`, `Face.Tables`.

---

## Type Wrapping for Typedefs

When Skia uses a `uint32_t` typedef (like `SkFourByteTag`), wrap it as a C# struct
rather than using raw `uint`. This provides type safety and discoverability:

```csharp
public struct SKFourByteTag : IEquatable<SKFourByteTag>
{
    private readonly uint value;

    public SKFourByteTag(uint value) { this.value = value; }
    public SKFourByteTag(char c1, char c2, char c3, char c4) { ... }

    public static SKFourByteTag Parse(string? tag) { ... }
    public override string ToString() => ...;

    public static implicit operator uint(SKFourByteTag tag) => tag.value;
    public static implicit operator SKFourByteTag(uint tag) => new(tag);

    // IEquatable<T>, ==, !=, GetHashCode
}
```

Then define a C typedef and map it in `libSkiaSharp.json` so generated structs use
the type directly:

```cpp
// C header
typedef uint32_t sk_fourbytetag_t;
```

```json
// libSkiaSharp.json
"sk_fourbytetag_t": { "cs": "SKFourByteTag" }
```

Check if HarfBuzzSharp already has an equivalent type (e.g., `Tag`) — model yours
after it, keeping it in the SkiaSharp namespace.

---

## Ref Struct Parameter Bags

When a method has many optional parameters (especially if more will be added in future
PRs), use a `ref struct` with `ReadOnlySpan` properties instead of overload explosion:

```csharp
public ref struct SKFontArguments
{
    public ReadOnlySpan<SKFontVariationPositionCoordinate> VariationDesignPosition { get; set; }
    public int CollectionIndex { get; set; }
}
```

`ref struct` allows `ReadOnlySpan` properties (which regular structs cannot hold).
Callers can use arrays, spans, or `stackalloc`. The tradeoff: `ref struct` cannot be
boxed, stored in fields, or used in async methods — but for call-site parameter bags
this is fine.

Keep 1-2 common-case shortcut overloads alongside the ref struct overload:

```csharp
Clone(ReadOnlySpan<Position> position)  // simple shortcut
Clone(SKFontArguments args)             // full control
```

---

## Test Patterns

### What to assert

**Exact values** — load a known test font and assert precise field values:

```csharp
var axes = typeface.VariationDesignParameters;
Assert.Single(axes);
Assert.Equal(SKFourByteTag.Parse("wght"), axes[0].Tag);
Assert.Equal(0.5f, axes[0].Min);
Assert.Equal(1.0f, axes[0].Default);
Assert.Equal(2.0f, axes[0].Max);
Assert.False(axes[0].IsHidden);
```

**Span vs property equivalence** — every Span overload should produce identical results
to its array counterpart:

```csharp
var arrayResult = typeface.VariationDesignParameters;
var spanBuffer = new SKFontVariationAxis[arrayResult.Length];
var written = typeface.GetVariationDesignParameters(spanBuffer);
Assert.Equal(arrayResult.Length, written);
for (int i = 0; i < arrayResult.Length; i++)
    Assert.Equal(arrayResult[i].Tag, spanBuffer[i].Tag);
```

**Interop round-trip** — create data in C#, pass through native, read back:

```csharp
var position = new[] {
    new SKFontVariationPositionCoordinate { Axis = SKFourByteTag.Parse("wght"), Value = 1.5f }
};
using var cloned = typeface.Clone(position);
var readBack = cloned.VariationDesignPosition;
Assert.Equal(SKFourByteTag.Parse("wght"), readBack[0].Axis);
Assert.Equal(1.5f, readBack[0].Value);
```

**Static/empty font handling** — APIs should return empty arrays, not null or crash:

```csharp
using var staticFont = SKTypeface.FromFile("content-font.ttf");
Assert.Empty(staticFont.VariationDesignParameters);
```

**Negative index validation** — every index parameter should be tested:

```csharp
Assert.Throws<ArgumentOutOfRangeException>(() => face.GetNamedInstanceDesignCoords(-1));
```

**Type-specific tests** — standalone types get their own test file with Parse, ToString,
constructors, equality, edge cases (null, empty, short strings, known hex values).

### Test organization

- One test class per type: `SKTypefaceTest`, `SKFourByteTagTest`, `HBFaceTest`
- Standalone types (like `SKFourByteTag`) get their own test file
- Use `[SkippableFact]` attribute and `using` for all disposables
- Never use early returns that silently pass — assert explicitly

---

## XML Documentation

Do **not** add triple-slash XML doc comments (`/// <summary>`) to new APIs.
Documentation is generated from a separate localized repository and inserted via an
automated process. Adding them manually creates merge conflicts with that process.

---

## File Conventions

- **File-scoped namespaces** preferred for new files: `namespace SkiaSharp;`
- **No `#nullable disable`** unless the file has reference-type fields that require it
- **One primary type per file**, file named after the type

---

## Summary Checklist

When designing a new API:

- [ ] Does it follow naming conventions (SK prefix, PascalCase)?
- [ ] Factory methods return null on failure?
- [ ] Constructors throw on failure?
- [ ] Parameters validated at C# layer?
- [ ] Overloads used instead of optional parameters?
- [ ] Same-instance returns handled correctly?
- [ ] Memory ownership clearly documented?
- [ ] Thread safety documented?
- [ ] ABI stable (no breaking changes)?
- [ ] Deprecated APIs use `[Obsolete]` (not removed)?

---

## Further Reading

- [.NET Framework Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/)
- [Member Overloading](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/member-overloading)
- [Parameter Design](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/parameter-design)
- [SkiaSharp Memory Management](memory-management.md)
- [SkiaSharp Error Handling](error-handling.md)
- [Adding New APIs](adding-apis.md)
