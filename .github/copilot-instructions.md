# SkiaSharp AI Instructions

SkiaSharp is a cross-platform 2D graphics API for .NET wrapping Google's Skia library.

---

## ⚠️ Critical Rules (Read First)

These rules are **non-negotiable**. Violating them causes broken builds, crashes, or downstream breakage.

### 1. Never Edit Generated Files

Files matching `*.generated.cs` and `docs/` are auto-generated.

- **❌ NEVER** manually edit these files
- **✅ ALWAYS** run the generator after C API changes: `pwsh ./utils/generate.ps1`

### 2. ABI Stability

SkiaSharp maintains stable ABI. Breaking changes break downstream apps.

| ✅ Allowed | ❌ Never |
|-----------|---------|
| Add new overloads | Modify existing signatures |
| Add new methods | Remove public APIs |
| Add new classes | Change return types |

### 3. Tests Are Mandatory

**Building alone is NOT sufficient.** Run tests before claiming completion:

```bash
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj
```

### 4. Same-Instance Returns

Some methods return the **same instance**. Check before disposing:

```csharp
// ✅ CORRECT
var source = GetImage();
var result = source.Subset(bounds);
if (result != source)
    source.Dispose();
return result;
```

**Methods that may return same instance:** `Subset()`, `ToRasterImage()`, `ToRasterImage(false)`

### 5. Threading

Skia is **NOT thread-safe**. Canvas/Paint/Path must be thread-local. Only immutable objects (Image/Shader/Data) can be shared.

---

## Skills & Routing

Skills are specialized workflows. Match user request to skill and invoke it.

### Skill Triggers

| Skill | Triggers |
|-------|----------|
| `add-api` | "add API", "expose", "wrap method", issue classified as New API |
| `bug-fix` | "crash", "exception", "broken", issue classified as Bug Fix |
| `api-docs` | "document", "XML docs", "fill in missing docs" |
| `native-dependency-update` | "bump libpng", "update zlib", "fix CVE" |
| `security-audit` | "security audit", "CVE status" (read-only) |
| `release-branch` | "release now", "start release X" |
| `release-testing` | "test the release", "verify packages" |
| `release-publish` | "publish", "push to nuget", "tag release" |

### When User Mentions an Issue (#NNNN)

1. **Fetch** — Use `github-mcp-server-issue_read` with method `get`
2. **Classify** — Determine type from issue content:
   | Indicators | Type | Skill |
   |------------|------|-------|
   | "add", "expose", "missing", "support" | New API | `add-api` |
   | "crash", "exception", "incorrect", "fails" | Bug Fix | `bug-fix` |
   | "docs", "documentation", "XML" | Documentation | `api-docs` |
3. **Brief context** — Grep for affected class/method, find similar patterns
4. **Confirm** — Show classification and ask: "Ready to proceed with [skill]?"
5. **Invoke skill** — After confirmation, invoke the destination skill

> **Note:** Skills handle the detailed workflow. Your job is classification, context, and routing.

### Adding APIs (Submodule Workflow)

When adding C API functions in `externals/skia/`, follow this **mandatory** sequence:

```
1. Edit C API      →  externals/skia/include/c/*.h + src/c/*.cpp
2. Commit IN submodule  →  cd externals/skia && git add -A && git commit
3. Stage in parent      →  cd ../.. && git add externals/skia
4. Generate bindings    →  pwsh ./utils/generate.ps1
5. Add C# wrapper       →  binding/SkiaSharp/*.cs
6. Test                 →  dotnet test
```

**Why each step matters:**
- Skip step 2 → Changes lost on submodule reset
- Skip step 3 → Parent repo ignores your changes
- Skip step 4 → C# bindings won't match C API
- Skip step 6 → Can't verify it works

👉 **Use the `add-api` skill** for guided execution with checkpoints.

---

## Quick Reference

### Architecture

```
C# Wrapper (binding/SkiaSharp/)  →  P/Invoke  →  C API (externals/skia/src/c/)  →  C++ Skia
```

C# validates parameters, C API trusts and passes through.

### Directory Guide

| Directory | Editable? | Notes |
|-----------|-----------|-------|
| `binding/SkiaSharp/` | ✅ Yes | C# wrappers |
| `externals/skia/src/c/` | ✅ Yes | C API implementation |
| `externals/skia/include/c/` | ✅ Yes | C API headers |
| `externals/skia/**` (other) | ❌ No | Upstream Skia - never modify |
| `*.generated.cs` | ❌ No | Regenerate with `pwsh ./utils/generate.ps1` |
| `docs/` | ❌ No | Auto-generated |
| `documentation/` | ✅ Yes | Architecture guides |

### Commands

| Task | Command |
|------|---------|
| Setup | `dotnet cake --target=externals-download` |
| Build | `dotnet build <project.csproj>` |
| Test | `dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj` |
| Regenerate | `pwsh ./utils/generate.ps1` |

> **Bootstrap:** If `output/native/` is empty, run `dotnet cake --target=externals-download`

---

## Memory Management

### Pointer Type Decision Tree

```
Is it wrapped in sk_sp<T>?
├─ Yes → SkRefCnt? → ISKReferenceCounted
│        SkNVRefCnt<T>? → ISKNonVirtualReferenceCounted
└─ No  → Parameter/getter? → Raw pointer (owns: false)
         Otherwise → Owned (DisposeNative deletes)
```

### Quick Reference

| Type | C++ | C# | Examples |
|------|-----|-----|----------|
| Raw | `T*` param | `owns: false` | Temporary refs |
| Owned | Manual delete | `DisposeNative()` | Canvas, Paint, Path |
| Ref-counted | `sk_sp<T>` | `ISKReferenceCounted` | Image, Shader, Surface |

### Error Handling

| Layer | Pattern |
|-------|---------|
| C API | Return `nullptr`/bool |
| C# Factory | Return `null` |
| C# Constructor | Throw |

---

## Code Patterns

### C# Wrapper

```csharp
// Factory - return null on failure
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

### C API

```cpp
// Naming: sk_<type>_<action>
sk_image_t* sk_image_new_from_encoded(const sk_data_t* cdata) {
    return ToImage(SkImages::DeferredFromEncodedData(sk_ref_sp(AsData(cdata))).release());
}
```

### API Design Rules

- **Overloads, not defaults** — Default parameters break ABI
- **Deprecate, don't remove** — Use `[Obsolete]` with migration guidance
- **Naming:** `SK` prefix, PascalCase methods, camelCase parameters

---

## Testing

### Test Command

```bash
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj
```

### Writing Tests

```csharp
[SkippableFact]
public void FeatureWorks()
{
    using var data = SKData.Create(Path.Combine(PathToImages, "baboon.jpg"));
    using var image = SKImage.FromEncodedData(data);
    Assert.NotNull(image);
}
```

**BaseTest helpers:** `PathToImages`, `PathToFonts`, `IsWindows/Mac/Linux`

**Philosophy:** Tests FAIL when wrong, never skip (except missing hardware).

---

## Debugging

1. Establish baseline — What's the known-good state?
2. One change at a time — Verify each change before proceeding
3. Track changes in a table — Log what you changed and the result
4. Platform differences are signals — If X works and Y fails, the difference IS the answer
5. Revert if worse — Don't pile fixes on top of failures

See [documentation/debugging-methodology.md](../documentation/debugging-methodology.md).

---

## Further Reading

| Topic | Document |
|-------|----------|
| Architecture | `documentation/architecture.md` |
| Memory Management | `documentation/memory-management.md` |
| Adding APIs | `documentation/adding-apis.md` |
| API Design | `documentation/api-design.md` |
| Error Handling | `documentation/error-handling.md` |

---

**Remember:** Three layers (C# → C API → C++), C# validates, C API trusts.
