# SkiaSharp AI Instructions

SkiaSharp is a cross-platform 2D graphics API for .NET wrapping Google's Skia library.

## Skills

**Always check for applicable skills first.** Before starting work on a request, review available skills in `.github/skills/`. If a skill matches the task, invoke it.

| Skill | When to Use |
|-------|-------------|
| `implement-issue` | User provides GitHub issue URL or says "implement #NNNN", "fix #NNNN". Gathers context and creates implementation plans for new APIs, bug fixes, enhancements. |
| `api-docs` | Writing/reviewing XML documentation. Triggers: "document SKFoo", "add XML docs", "fill in missing docs", "remove To be added placeholders". |
| `native-dependency-update` | Updating native dependencies. Triggers: "bump libpng", "update zlib", "fix CVE in expat". Updates DEPS, cgmanifest.json, builds locally, creates PRs. |
| `security-audit` | Security investigation. Triggers: "security audit", "audit CVEs", "CVE status". Searches issues/PRs, scans for CVEs, verifies fixes, produces report. Read-only. |
| `release-branch` | Creating release branches. Triggers: "release now", "start release X", "create release branch". Auto-detects next preview version, updates PREVIEW_LABEL, bumps main. |
| `release-testing` | Testing packages before publishing. Triggers: "test the release", "verify packages", "run tests on iPad". Runs integration tests on Console, Blazor, iOS, Android, Mac Catalyst. |
| `release-publish` | Publishing and finalizing releases. Triggers: "publish X", "push to nuget", "tag the release". Publishes to NuGet.org, creates tag, GitHub release, annotates notes with emojis. |

**If uncertain:** Ask the user: *"I found the [skill-name] skill which handles [description]. Should I use it for this task?"*

---

## Quick Reference

### Architecture

```
C# Wrapper (binding/SkiaSharp/)  →  P/Invoke  →  C API (externals/skia/src/c/)  →  C++ Skia
```

**Key principle:** C# validates parameters, C API trusts and passes through.

### Directory Guide

| Directory | Purpose | Editable? |
|-----------|---------|-----------|
| `binding/SkiaSharp/` | C# wrappers | ✅ Yes |
| `externals/skia/src/c/` | C API implementation | ✅ Yes |
| `externals/skia/include/c/` | C API headers | ✅ Yes |
| `externals/skia/**` (other) | Upstream Skia | ❌ No - never modify |
| `*.generated.cs` | Auto-generated P/Invoke | ❌ No - regenerate with `./utils/generate.ps1` |
| `docs/` | Auto-generated API docs | ❌ No |
| `documentation/` | Architecture guides | ✅ Yes |

### Commands

| Task | Command |
|------|---------|
| Setup (one-time) | `dotnet cake --target=externals-download` |
| Build | `dotnet build <project.csproj>` |
| Test | `dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj` |
| Regenerate bindings | `./utils/generate.ps1` |

> **Check if externals exist:** `ls output/native/` - if empty/missing, run the download.

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

### 4. Never Edit Generated Files

Files matching `*.generated.cs` are auto-generated from C headers. After C API changes, regenerate with:

```pwsh
./utils/generate.ps1
```

---

## Memory Management

### Pointer Type Decision Tree

```
Is it wrapped in sk_sp<T>?
├─ Yes → Is it SkRefCnt or SkNVRefCnt?
│        ├─ SkRefCnt → ISKReferenceCounted (virtual ref counting)
│        └─ SkNVRefCnt<T> → ISKNonVirtualReferenceCounted
└─ No → Is it a parameter or getter return?
         ├─ Yes → Raw pointer (owns: false)
         └─ No → Owned (DisposeNative deletes)
```

### Pointer Types

| Type | C++ Pattern | C# Pattern | Examples |
|------|-------------|------------|----------|
| **Raw** | `T*` parameter/getter | `owns: false` | Temporary refs |
| **Owned** | Manual delete | `DisposeNative()` | Canvas, Paint, Path, Bitmap |
| **Ref-counted (virtual)** | `sk_sp<T>`, inherits `SkRefCnt` | `ISKReferenceCounted` | Image, Shader, Surface, Picture |
| **Ref-counted (non-virtual)** | `sk_sp<T>`, inherits `SkNVRefCnt<T>` | `ISKNonVirtualReferenceCounted` | Data, TextBlob, Vertices, ColorSpace |

### Error Handling by Layer

| Layer | Pattern | Example |
|-------|---------|---------|
| C API | Pass through (bool/null/void) | Return `nullptr` on failure |
| C# Factory | Return `null` on failure | `SKImage.FromEncodedData()` |
| C# Constructor | Throw on failure | `new SKBitmap()` |

---

## API Design

### Naming Conventions

| Type | Convention | Example |
|------|------------|---------|
| Classes/Structs | `SK` + PascalCase | `SKCanvas`, `SKRect` |
| Enums | `SK` + PascalCase | `SKBlendMode` |
| Methods | PascalCase verb | `DrawRect()`, `Create()` |
| Parameters | camelCase | `sourceRect`, `filterMode` |
| Private fields | camelCase | `handle`, `isDisposed` |

### Factory Method Prefixes

| Prefix | Usage | On Failure |
|--------|-------|------------|
| `Create` | New instance | Returns `null` |
| `From*` | Convert existing | Returns `null` |
| `Decode` | Parse data | Returns `null` |
| Constructor | New instance | Throws exception |

### Overloads vs Defaults

**Always use overloads**, not default parameters (ABI stability):

```csharp
// ✅ CORRECT - Overload chain
public static SKData CreateCopy(byte[] bytes) =>
    CreateCopy(bytes, (ulong)bytes.Length);

public static SKData CreateCopy(byte[] bytes, ulong length)
{
    fixed (byte* b = bytes) {
        return GetObject(SkiaApi.sk_data_new_with_copy(b, (IntPtr)length));
    }
}

// ❌ AVOID - Default parameters break ABI
public static SKData CreateCopy(byte[] bytes, ulong length = 0)
```

### Deprecation

Never remove APIs. Use `[Obsolete]` with migration guidance:

```csharp
[Obsolete("Use ToShader(SKShaderTileMode, SKShaderTileMode, SKSamplingOptions) instead.")]
public SKShader ToShader(SKShaderTileMode tmx, SKShaderTileMode tmy) =>
    ToShader(tmx, tmy, SKSamplingOptions.Default);
```

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

// Conversion macros from sk_types_priv.h:
// AsCanvas(sk_canvas_t*) → SkCanvas*
// ToCanvas(SkCanvas*)    → sk_canvas_t*
```

---

## Testing

### Test Projects

| Project | Purpose | When to Use |
|---------|---------|-------------|
| `SkiaSharp.Tests.Console` | Core unit tests | **Default** - use for most development |
| `SkiaSharp.Tests.Devices` | MAUI on-device tests | Platform-specific behavior |
| `SkiaSharp.Direct3D.Tests.Console` | Direct3D GPU tests | Windows GPU backend |
| `SkiaSharp.Vulkan.Tests.Console` | Vulkan GPU tests | Cross-platform GPU backend |

### Running Tests

```bash
# Run all console tests
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj

# Run specific test
dotnet test --filter "FullyQualifiedName~SKImageTest.FromEncodedDataWorks"
```

### Writing Tests

```csharp
public class SKImageTest : BaseTest
{
    [SkippableFact]
    public void FeatureWorks()
    {
        using var data = SKData.Create(Path.Combine(PathToImages, "baboon.jpg"));
        Assert.NotNull(data);
        
        using var image = SKImage.FromEncodedData(data);
        Assert.NotNull(image);
    }
}
```

### BaseTest Helpers

| Helper | Description |
|--------|-------------|
| `PathToImages` | Path to `tests/Content/images/` |
| `PathToFonts` | Path to `tests/Content/fonts/` |
| `IsWindows`, `IsMac`, `IsLinux` | Platform detection |
| `CollectGarbage()` | Force GC (for memory tests) |

### Test Guidelines

- ✅ Always use `using` statements
- ✅ Use `[SkippableFact]` for all tests
- ✅ Test null/invalid inputs
- ✅ Test disposal behavior
- ❌ Don't leave objects undisposed

### Test Philosophy

**Tests must FAIL when something is wrong, never skip.**

- Missing dependencies → FAIL with helpful error
- Missing reference data → FAIL
- Environment not set up → FAIL

The **ONLY** acceptable skip is for hardware that physically cannot be present:
- iOS tests on non-macOS
- GPU tests on machines without GPU

A green test run means everything works.

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
| Releasing | [documentation/releasing.md](../documentation/releasing.md) |
| Versioning | [documentation/versioning.md](../documentation/versioning.md) |

---

**Remember:** Three layers (C# → C API → C++), three pointer types (raw/owned/ref-counted), C# validates, C API trusts.
