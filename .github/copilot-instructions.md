# SkiaSharp AI Instructions

SkiaSharp is a cross-platform 2D graphics API for .NET wrapping Google's Skia library.

## Skills

**Always check for applicable skills first.** Before starting work on a request, review available skills in `.github/skills/`. If a skill matches the task, invoke it.

| Skill | When to Use |
|-------|-------------|
| `implement-issue` | Implementing GitHub issues (new APIs, bug fixes) |
| `api-docs` | Writing or reviewing XML documentation |
| `release-branch` | Creating release branches |
| `release-tag` | Tagging releases after packages are published |

**If uncertain:** If a skill seems related but you're not sure it applies, ask the user: *"I found the [skill-name] skill which handles [description]. Should I use it for this task? If the description doesn't match well, I can help improve it."*

---

## Quick Reference

```
C# Wrapper (binding/SkiaSharp/)  →  P/Invoke  →  C API (externals/skia/src/c/)  →  C++ Skia
```

| Directory | Purpose | Editable? |
|-----------|---------|-----------|
| `binding/SkiaSharp/` | C# wrappers | ✅ Yes |
| `externals/skia/src/c/` | C API implementation | ✅ Yes |
| `externals/skia/include/c/` | C API headers | ✅ Yes |
| `externals/skia/**` (other) | Upstream Skia | ❌ No |
| `*.generated.cs` | Auto-generated P/Invoke | ❌ No (regenerate) |
| `docs/` | Auto-generated docs | ❌ No |
| `documentation/` | Architecture guides | ✅ Yes |

**Setup:** `dotnet cake --target=externals-download` (one-time, gets native libs)  
**Build:** `dotnet build <project.csproj>` (builds project + dependencies)  
**Test:** `dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj`  
**Regenerate bindings:** `./utils/generate.ps1`

> **Check if externals exist:** `ls output/native/` - if empty/missing, run the download.

---

## Testing

### Test Projects

| Project | Purpose | When to Use |
|---------|---------|-------------|
| `SkiaSharp.Tests.Console` | Core unit tests | **Default** - use for most development |
| `SkiaSharp.Tests.Devices` | MAUI on-device tests | Platform-specific behavior (iOS/Android/Mac/Windows) |
| `SkiaSharp.Direct3D.Tests.Console` | Direct3D GPU tests | Windows GPU backend testing |
| `SkiaSharp.Vulkan.Tests.Console` | Vulkan GPU tests | Cross-platform GPU backend testing |
| `SkiaSharp.Tests.Wasm` | WebAssembly tests | Browser/WASM testing |

### Project Structure

```
tests/
├── Tests/                   # Shared test code (used by multiple projects)
│   ├── SkiaSharp/           # Core SkiaSharp tests
│   ├── HarfBuzzSharp/       # HarfBuzz tests
│   └── Resources/           # Test images and files
├── SkiaSharp.Tests.Console/ # Console runner (references Tests/)
├── SkiaSharp.Tests.Devices/ # MAUI device runner
└── Content/                 # Test assets copied to output
```

### Running Tests

```bash
# Build and run console tests (most common)
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj

# Run specific test class
dotnet test --filter "FullyQualifiedName~SKImageTest"

# Run specific test method
dotnet test --filter "FullyQualifiedName~SKImageTest.FromEncodedDataWorks"
```

### Writing Tests

All test classes inherit from `BaseTest`, which provides helpers for accessing test content and platform detection:

```csharp
public class SKImageTest : BaseTest
{
    [SkippableFact]
    public void FeatureWorks()
    {
        // Access test images via PathToImages helper
        using var data = SKData.Create(Path.Combine(PathToImages, "baboon.jpg"));
        Assert.NotNull(data);
        
        using var image = SKImage.FromEncodedData(data);
        Assert.NotNull(image);
        
        Assert.True(image.IsLazyGenerated);
    }
    
    [SkippableFact]
    public void PlatformSpecificTest()
    {
        // Platform detection helpers
        if (IsWindows)
        {
            // Windows-specific assertions
        }
    }
}
```

### BaseTest Helpers

| Helper | Description |
|--------|-------------|
| `PathToImages` | Path to `tests/Content/images/` (baboon.jpg, color-wheel.png, etc.) |
| `PathToFonts` | Path to `tests/Content/fonts/` |
| `PathRoot` | Root path for test content |
| `IsWindows`, `IsMac`, `IsLinux`, `IsUnix` | Platform detection booleans |
| `DefaultFontFamily` | Platform-appropriate default font |
| `UnicodeFontFamilies` | Font families with Unicode support |
| `CollectGarbage()` | Force GC collection (for memory tests) |

### Test Guidelines

- ✅ Always use `using` statements - test memory management
- ✅ Test null/invalid inputs and edge cases
- ✅ Test disposal behavior  
- ✅ Use `[SkippableFact]` (standard for all tests in this repo)
- ✅ Use `PathToImages` helper for test assets in `tests/Content/images/`
- ❌ Don't leave objects undisposed
- ❌ Don't skip disposal even in failure paths

---

## ⚠️ Critical Rules

### 1. ABI Stability (Non-negotiable)

SkiaSharp maintains stable ABI across versions. Breaking changes break downstream apps.

| ✅ Allowed | ❌ Never |
|-----------|---------|
| Add new overloads | Modify existing signatures |
| Add new methods | Remove public APIs |
| Add new classes | Change return types |

### 2. Same-Instance Returns

Some Skia methods return the **same instance** as an optimization. Always check before disposing:

```csharp
// ❌ WRONG - crashes if Subset returns same instance
using var source = FromEncodedData(data);
var result = source.Subset(subset);
return result;  // source disposed, but result IS source!

// ✅ CORRECT
var source = FromEncodedData(data);
var result = source.Subset(subset);
if (result != source)
    source.Dispose();
return result;
```

**Methods that may return same instance:** `Subset()`, `ToRasterImage()`, `ToRasterImage(false)`

### 3. Threading

Skia is **NOT thread-safe**. Canvas/Paint/Path must be thread-local. Only immutable objects (Image/Shader/Data) can be shared across threads.

---

## Memory Management

### Pointer Types

| Type | C++ Pattern | C# Pattern | Examples |
|------|-------------|------------|----------|
| **Raw** | `T*` parameter/getter | `owns: false` | Temporary refs |
| **Owned** | Manual delete | `DisposeNative()` | Canvas, Paint, Path, Bitmap |
| **Ref-counted (virtual)** | `sk_sp<T>`, inherits `SkRefCnt` | `ISKReferenceCounted` | Image, Shader, Surface, Picture |
| **Ref-counted (non-virtual)** | `sk_sp<T>`, inherits `SkNVRefCnt<T>` | `ISKNonVirtualReferenceCounted` | Data, TextBlob, Vertices, ColorSpace |

### Error Handling

| Layer | Pattern |
|-------|---------|
| C API | Pass through (return bool/null/void) |
| C# Factory | Return `null` on failure |
| C# Constructor | Throw on failure |

---

## API Design

### Naming

| Type | Convention | Example |
|------|------------|---------|
| Classes/Structs | `SK` + PascalCase | `SKCanvas`, `SKRect` |
| Enums | `SK` + PascalCase | `SKBlendMode` |
| Methods | PascalCase verb | `DrawRect()`, `Create()` |
| Parameters | camelCase | `sourceRect`, `filterMode` |
| Private fields | camelCase | `handle`, `isDisposed` |

### Factory Methods

| Prefix | Usage | On Failure |
|--------|-------|------------|
| `Create` | New instance | Returns `null` |
| `From*` | Convert existing | Returns `null` |
| `Decode` | Parse data | Returns `null` |
| Constructor | New instance | Throws exception |

### Overloads vs Defaults

**Always use overloads**, not default parameters:

```csharp
// ✅ CORRECT - Overload chain (from SKData.CreateCopy)
public static SKData CreateCopy(byte[] bytes) =>
    CreateCopy(bytes, (ulong)bytes.Length);

public static SKData CreateCopy(byte[] bytes, ulong length)
{
    fixed (byte* b = bytes) {
        return GetObject(SkiaApi.sk_data_new_with_copy(b, (IntPtr)length));
    }
}

// ❌ AVOID - Default parameters break ABI when changed
public static SKData CreateCopy(byte[] bytes, ulong length = 0)
```

### Deprecation

Never remove APIs. Use `[Obsolete]` with guidance:

```csharp
[Obsolete("Use ToShader(SKShaderTileMode, SKShaderTileMode, SKSamplingOptions) instead.")]
public SKShader ToShader(SKShaderTileMode tmx, SKShaderTileMode tmy) =>
    ToShader(tmx, tmy, SKSamplingOptions.Default);
```

See [documentation/api-design.md](../documentation/api-design.md) for complete guidelines.

---

## Code Patterns

### C# Bindings (`binding/SkiaSharp/*.cs`)

```csharp
// Naming: SKType (e.g., SKCanvas, SKPaint)
// Inherit SKObject, add ISKReferenceCounted for ref-counted types

// Factory method - return null on failure
public static SKImage FromPixels(SKImageInfo info, SKData data, int rowBytes)
{
    if (data == null)
        throw new ArgumentNullException(nameof(data));
    var cinfo = SKImageInfoNative.FromManaged(ref info);
    return GetObject(SkiaApi.sk_image_new_raster_data(&cinfo, data.Handle, (IntPtr)rowBytes));
}

// Instance method - validate then call
public void DrawRect(SKRect rect, SKPaint paint)
{
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));
    SkiaApi.sk_canvas_draw_rect(Handle, &rect, paint.Handle);
}
```

### C API Layer (`externals/skia/src/c/*.cpp`)

```cpp
// Naming: sk_<type>_<action>, types: sk_<type>_t
// Use SK_C_API, C types only, no exceptions

sk_image_t* sk_image_new_from_encoded(const sk_data_t* cdata) {
    return ToImage(SkImages::DeferredFromEncodedData(sk_ref_sp(AsData(cdata))).release());
}

// Conversion macros generated by DEF_CLASS_MAP in sk_types_priv.h:
// AsCanvas(sk_canvas_t*) → SkCanvas*
// ToCanvas(SkCanvas*)    → sk_canvas_t*
```

---

## Further Reading

| Topic | Document |
|-------|----------|
| Architecture | [documentation/architecture.md](../documentation/architecture.md) |
| Memory Management | [documentation/memory-management.md](../documentation/memory-management.md) |
| Error Handling | [documentation/error-handling.md](../documentation/error-handling.md) |
| API Design | [documentation/api-design.md](../documentation/api-design.md) |
| Adding New APIs | [documentation/adding-apis.md](../documentation/adding-apis.md) |
| Building | [documentation/building.md](../documentation/building.md) |
