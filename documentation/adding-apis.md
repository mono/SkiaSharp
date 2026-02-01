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
cd /home/runner/work/SkiaSharp/SkiaSharp  # Return to repo root

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
| Skip running `./utils/generate.ps1` | C# bindings won't match C API |
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

## Complete Implementation Checklist

**Phase 1: C++ Analysis**
- [ ] Identified pointer type (raw/owned/ref-counted)
- [ ] Checked error conditions

**Phase 2: C API in Submodule**
- [ ] Header declaration with `SK_C_API` in `externals/skia/include/c/sk_*.h`
- [ ] Implementation in `externals/skia/src/c/sk_*.cpp` uses `AsType()`/`ToType()` macros
- [ ] Ref-counted params use `sk_ref_sp()`
- [ ] Ref-counted returns use `.release()`
- [ ] **Configured git in submodule** (`git config user.email/name`)
- [ ] **Committed changes in submodule** (`cd externals/skia && git commit`)
- [ ] **Staged submodule in parent repo** (`cd /home/runner/work/SkiaSharp/SkiaSharp && git add externals/skia`)

**Phase 3: C# Bindings**
- [ ] **Regenerated P/Invoke** (`./utils/generate.ps1`) — **MANDATORY, never skip**
- [ ] Verified `SkiaApi.generated.cs` updated correctly
- [ ] Added C# wrapper method
- [ ] Validates null parameters
- [ ] Checks return values (factory→null, constructor→throw)
- [ ] Correct ownership (`owns: true/false`)

**Phase 4: Tests (MANDATORY)**
- [ ] Added test methods in `tests/Tests/SkiaSharp/`
- [ ] Tests follow existing patterns (`[SkippableFact]`, `using` statements)
- [ ] **Ran tests and verified they pass** — **NOT OPTIONAL**
- [ ] All assertions pass

**Phase 5: Final Verification**
- [ ] Build succeeds: `dotnet build binding/SkiaSharp/SkiaSharp.csproj`
- [ ] Tests pass: `dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj`
- [ ] Both submodule and C# changes committed together

## Build & Test Commands

```bash
# Step 1: Regenerate P/Invoke (after C API changes)
pwsh ./utils/generate.ps1

# Step 2: Build C# bindings
dotnet build binding/SkiaSharp/SkiaSharp.csproj

# Step 3: Run tests (MANDATORY - not optional)
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj

# Alternative: Use Cake for everything
dotnet cake --target=libs         # Build
dotnet cake --target=tests        # Test
```

## Common Mistakes

| Mistake | Impact | Solution |
|---------|--------|----------|
| Edited C API but didn't commit in submodule | Changes disappear on next submodule update | Always commit inside `externals/skia/` first |
| Forgot `git add externals/skia` in parent | Parent repo doesn't reference your C API changes | Stage submodule after committing inside it |
| Manually edited `*.generated.cs` | Binding mismatch, overwrites on next generation | Always run `./utils/generate.ps1` |
| Only built, didn't test | Functionality may not work despite compiling | Always run tests — passing tests required |
| Generator or tests fail | Incomplete implementation | Retry once; if still failing, stop and notify developer to fix environment |
