---
name: add-api
description: >
  Add new C# APIs to SkiaSharp by wrapping Skia C++ functionality.
  Structured 6-phase workflow: C++ analysis → C API creation → submodule commits →
  binding generation → C# wrapper → testing.
  
  Triggers:
  - Issue classified as "New API" (after fetching and classification)
  - Direct request: "add DrawFoo method", "expose SkSurface::draw", "wrap sk_foo_bar"
  - Keywords: "add API", "expose function", "wrap method", "create binding for"
---

# Add API Skill

## ⚠️ Branch Protection (COMPLIANCE REQUIRED)

> **🛑 NEVER commit directly to protected branches. This is a policy violation.**

| Repository | Protected Branches | Required Action |
|------------|-------------------|-----------------|
| SkiaSharp (parent) | `main` | Create feature branch first |
| externals/skia (submodule) | `main`, `skiasharp` | Create feature branch first |

**BEFORE making any changes**, create feature branches in BOTH repos:

```bash
# Step 1: Create branch in SkiaSharp repo
git checkout -b dev/issue-NNNN-description

# Step 2: Create branch in submodule (REQUIRED for C API changes)
cd externals/skia
git checkout -b dev/issue-NNNN-description
cd ../..
```

## ❌ NEVER Do These

| Shortcut | Consequence |
|----------|-------------|
| Commit directly to `main` or `skiasharp` branches | **Policy violation** — blocks PR, requires revert |
| Edit `*.generated.cs` manually | Overwrites on next generation; binding mismatch |
| Skip generator after C API change | C# bindings won't match C API |
| **Skip native build after C API change** | **`EntryPointNotFoundException` at runtime — tests fail** |
| Skip tests | Can't verify functionality works |
| **Skip tests because they fail** | **Unacceptable — fix the issue, don't skip** |
| Commit C API without staging submodule | Parent repo won't use your changes |
| Use `externals-download` after modifying C API | Downloaded natives don't have your new functions |

## Workflow Overview

```
1. Analyze C++ API  →  Find in Skia headers, identify pointer type
2. Add C API        →  Header + impl in externals/skia/
3. Commit Submodule →  Commit IN submodule, then stage in parent
4. Generate         →  Run pwsh ./utils/generate.ps1 (MANDATORY)
5. Add C# Wrapper   →  Validate params, call P/Invoke
6. Build Native     →  dotnet cake --target=externals-{platform} (MANDATORY)
7. Test             →  Run tests — must PASS (MANDATORY)
```

👉 **Detailed checklists:** [references/checklists.md](references/checklists.md)
👉 **Troubleshooting:** [references/troubleshooting.md](references/troubleshooting.md)

---

## Phase 1: Analyze C++ API

Find the C++ API in Skia headers and identify:

1. **Pointer type** — determines C# wrapper pattern:
   - Raw (`T*`) → `owns: false`
   - Owned → `DisposeNative()` deletes
   - Ref-counted (`sk_sp<T>`) → `ISKReferenceCounted` or `ISKNonVirtualReferenceCounted`

2. **Error handling** — returns null/bool? throws?

3. **Parameters** — primitives, references, pointers, const-ness

👉 See [../../../documentation/memory-management.md#pointer-type-decision-tree](../../../documentation/memory-management.md#pointer-type-decision-tree)

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
git config user.email "you@example.com"  # first time only
git config user.name "Your Name"             # first time only
git add include/c/sk_*.h src/c/sk_*.cpp
git commit -m "Add sk_foo_bar to C API"

# Step 2: Stage submodule in parent
cd ../..  # Back to repo root
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

## Phase 6: Build & Test

> **🛑 MANDATORY — You modified the C API, so you MUST build the native library**

### Step 1: Build Native Library (REQUIRED)

Since you added/modified C API functions in Phase 2, you **MUST** rebuild the native library before testing. The pre-built natives from `externals-download` do not contain your new functions.

```bash
# macOS (Apple Silicon)
dotnet cake --target=externals-macos --arch=arm64

# macOS (Intel)
dotnet cake --target=externals-macos --arch=x64

# Windows (x64)
dotnet cake --target=externals-windows --arch=x64

# Linux (requires Docker)
dotnet cake --target=externals-linux --arch=x64
```

> ⚠️ **Native builds take 10-30 minutes.** Only build for platforms you can test on.

### Step 2: Write Tests

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

### Step 3: Run Tests

```bash
dotnet build binding/SkiaSharp/SkiaSharp.csproj
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj
```

> **🛑 Tests MUST PASS.** Do not skip tests. Do not claim completion if tests fail.
> 
> Skipping tests is only acceptable for hardware limitations (no GPU, no screen attached). `EntryPointNotFoundException` means you forgot to build the native library — go back to Step 1.

**Tests must pass before claiming completion.**
