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

Add new C# APIs to SkiaSharp by wrapping Skia C++ functionality.

## Key References

- **[documentation/adding-apis.md](../../../documentation/adding-apis.md)** — Complete examples, pointer types, error handling
- **[.github/copilot-instructions.md](../../copilot-instructions.md)** — Architecture, patterns, critical rules

## ⚠️ MANDATORY: Follow Every Phase

You MUST complete ALL phases in order. Do not skip phases or claim completion prematurely.

### Pre-Flight Checklist

Before starting, confirm you will:
- [ ] Complete Phase 1-6 in order
- [ ] Commit C API changes in `externals/skia` submodule BEFORE staging in parent
- [ ] Run `./utils/generate.ps1` after C API changes (NEVER edit `*.generated.cs` manually)
- [ ] Write AND RUN tests before claiming completion
- [ ] Stop and ask at every 🛑 checkpoint

## Critical Rules

> **🛑 STOP AND ASK** before: Editing `*.generated.cs` manually, Skipping generator, Skipping tests

### ❌ NEVER Do These

| Shortcut | Why It's Wrong |
|----------|----------------|
| Edit `*.generated.cs` manually | Will be overwritten; causes binding mismatch |
| Skip generator after C API change | C# bindings won't match C API |
| Skip test execution | Can't verify functionality actually works |
| Commit C API but don't stage submodule | Parent repo won't use your changes |
| Only run build, not tests | Passing build ≠ working code |

---

## Workflow

### Phase 1: Analyze C++ API

1. **Find the C++ API** in upstream Skia headers
2. **Identify pointer type:**
   - Raw pointer (`T*`) — parameter or getter return
   - Owned pointer — needs manual delete
   - Ref-counted (`sk_sp<T>`) — virtual (SkRefCnt) or non-virtual (SkNVRefCnt)
3. **Check error handling:**
   - Returns null/bool for errors?
   - Can it throw exceptions?
4. **Note parameter types:**
   - Primitives (float, int)
   - References vs pointers
   - Const-ness

👉 See [copilot-instructions.md](../../copilot-instructions.md#pointer-type-decision-tree) for pointer type guide.

### Phase 2: Add C API

> **Location:** `externals/skia/src/c/` and `externals/skia/include/c/`

**Header** (`externals/skia/include/c/sk_*.h`):
```cpp
SK_C_API void sk_canvas_draw_circle(sk_canvas_t* canvas, float cx, float cy, 
                                     float radius, const sk_paint_t* paint);
```

**Implementation** (`externals/skia/src/c/sk_*.cpp`):
```cpp
void sk_canvas_draw_circle(sk_canvas_t* canvas, float cx, float cy,
                           float radius, const sk_paint_t* paint) {
    AsCanvas(canvas)->drawCircle(cx, cy, radius, *AsPaint(paint));
}
```

**Conversion macros:**
- `AsCanvas(sk_canvas_t*)` → `SkCanvas*`
- `ToCanvas(SkCanvas*)` → `sk_canvas_t*`
- Use `sk_ref_sp()` for ref-counted parameters
- Use `.release()` for ref-counted returns

### Phase 3: Commit Submodule

> **🛑 CRITICAL: This phase is where changes are most often lost**

**Step 1: Configure git in submodule (first time only)**
```bash
cd externals/skia
git config user.email "copilot@github.com"
git config user.name "GitHub Copilot"
```

**Step 2: Commit C API changes IN the submodule**
```bash
cd externals/skia
git add include/c/sk_*.h src/c/sk_*.cpp
git commit -m "Add sk_foo_bar to C API"
```

**Step 3: Stage submodule reference in parent repo**
```bash
cd ..  # Return to SkiaSharp root
git add externals/skia
```

**Step 4: Verify**
```bash
git status
# Should show: modified:   externals/skia (new commits)
```

> ⚠️ **If you skip step 2:** Your C API changes disappear on next submodule update
> ⚠️ **If you skip step 3:** Parent repo won't use your updated C API

### Phase 4: Regenerate Bindings

> **🛑 MANDATORY: Always run generator after C API changes**

```bash
pwsh ./utils/generate.ps1
```

**What this does:**
- Parses C headers in `externals/skia/include/c/`
- Generates P/Invoke declarations in `binding/SkiaSharp/SkiaApi.generated.cs`
- Creates delegate wrappers for dynamic loading

**Verify:**
```bash
git diff binding/SkiaSharp/SkiaApi.generated.cs
# Should show your new sk_* function declarations
```

> ❌ **NEVER** manually edit `*.generated.cs` — always regenerate

**If generation fails:**
- Retry once in case of transient issues
- Verify C API syntax is correct
- If still failing, stop and notify the developer to fix the environment

### Phase 5: Add C# Wrapper

> **Location:** `binding/SkiaSharp/SK*.cs`

**Factory method pattern** (returns null on failure):
```csharp
public static SKImage FromPixels(SKImageInfo info, SKData data, int rowBytes)
{
    if (data == null)
        throw new ArgumentNullException(nameof(data));
    var cinfo = SKImageInfoNative.FromManaged(ref info);
    return GetObject(SkiaApi.sk_image_new_raster_data(&cinfo, data.Handle, (IntPtr)rowBytes));
}
```

**Instance method pattern:**
```csharp
public void DrawCircle(float cx, float cy, float radius, SKPaint paint)
{
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));
    SkiaApi.sk_canvas_draw_circle(Handle, cx, cy, radius, paint.Handle);
}
```

**Key points:**
- Validate null parameters
- Factory methods return null on failure
- Constructors throw on failure
- Use overload chains, not default parameters (ABI stability)

### Phase 6: Test

> **🛑 MANDATORY: Tests are NOT optional**

**Add test methods** in `tests/Tests/SkiaSharp/`:
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

**Run tests:**
```bash
dotnet build binding/SkiaSharp/SkiaSharp.csproj
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj
```

> ❌ **Building alone is NOT sufficient** — must verify tests pass
> ❌ **If tests fail, stop and fix** — don't skip or work around test failures

---

## Phase Completion Checklist

Before proceeding to next phase, verify:

**After Phase 2 (C API):**
- [ ] Header has `SK_C_API` declaration
- [ ] Implementation uses `AsType()`/`ToType()` macros
- [ ] Ref-counted parameters use `sk_ref_sp()`
- [ ] Ref-counted returns use `.release()`

**After Phase 3 (Submodule):**
- [ ] C API committed IN submodule
- [ ] Submodule staged in parent (`git add externals/skia`)
- [ ] `git status` shows "modified: externals/skia (new commits)"

**After Phase 4 (Generate):**
- [ ] Generator ran successfully
- [ ] `SkiaApi.generated.cs` shows new function
- [ ] No manual edits to `*.generated.cs`

**After Phase 5 (C# Wrapper):**
- [ ] Null parameters validated
- [ ] Correct error handling (factory vs constructor)
- [ ] Follows ABI stability rules

**After Phase 6 (Test):**
- [ ] Test methods added
- [ ] Build succeeds
- [ ] **Tests PASS** (not just compile)

---

## Common Issues

### Issue: "Changes lost when I ran git command"
**Cause:** Didn't commit in submodule before staging in parent
**Fix:** Always commit inside `externals/skia/` FIRST, then `git add externals/skia`

### Issue: "Generated file doesn't have my function"
**Cause:** Didn't run generator after C API changes
**Fix:** Run `pwsh ./utils/generate.ps1` — NEVER skip this

### Issue: "Test compiles but fails"
**Cause:** C API implementation bug, wrong pointer handling, incorrect parameters
**Fix:** Debug C API layer, check conversion macros, verify pointer types

### Issue: "Generator or tests fail"
**Cause:** Environment issue, missing dependencies, transient failure
**Fix:** Retry once. If still failing, stop and notify developer to fix the environment — don't skip or work around

---

## Summary

Adding APIs requires disciplined workflow:
1. **Analyze** C++ to understand types and errors
2. **Write** C API wrapper with proper conversions
3. **Commit** in submodule, stage in parent
4. **Generate** P/Invoke bindings (mandatory)
5. **Wrap** in C# with validation
6. **Test** to verify functionality (mandatory)

Each phase builds on the previous. Skipping phases or shortcuts causes bugs, lost work, or broken implementations.
