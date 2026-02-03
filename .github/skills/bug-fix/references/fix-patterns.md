# Common Fix Patterns

## Missing Null Check

```csharp
// Before (crashes)
public void DrawPath(SKPath path, SKPaint paint)
{
    SkiaApi.sk_canvas_draw_path(Handle, path.Handle, paint.Handle);
}

// After (validates)
public void DrawPath(SKPath path, SKPaint paint)
{
    if (path == null)
        throw new ArgumentNullException(nameof(path));
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));
    SkiaApi.sk_canvas_draw_path(Handle, path.Handle, paint.Handle);
}
```

## Same-Instance Return Bug

Some methods may return the **same instance** passed in. Always check before disposing:

```csharp
// WRONG - may dispose what we're returning
using var source = GetImage();
var result = source.Subset(bounds);
return result;

// CORRECT - check first
var source = GetImage();
var result = source.Subset(bounds);
if (result != source)
    source.Dispose();
return result;
```

**Methods that may return same instance:** `Subset()`, `ToRasterImage()`, `ToRasterImage(false)`

## Native Linking Issues (undefined symbol)

When you see `undefined symbol: xxx` errors on Linux:

### Step 1: Identify what library provides the symbol

```bash
# Search which library provides a symbol (run in Docker for target platform)
docker run --platform linux/arm64 -it debian:bookworm-slim bash -c \
  "apt-get update && apt-get install -y binutils && \
   apt-file update && apt-file search uuid_generate_random"
```

### Step 2: Compare DT_NEEDED entries between working and broken builds

```bash
# CRITICAL: Compare DT_NEEDED entries between platforms
# This often reveals the root cause immediately
docker run --rm -v $(pwd):/work debian:bookworm-slim bash -c \
  "apt-get update -qq && apt-get install -y -qq binutils && \
   echo '=== x64 ===' && \
   readelf -d /work/output/native/linux/x64/libSkiaSharp.so | grep NEEDED && \
   echo && echo '=== arm64 ===' && \
   readelf -d /work/output/native/linux/arm64/libSkiaSharp.so | grep NEEDED"
```

**If a library appears in x64 but NOT in arm64 (or vice versa), that's your root cause!**

### Step 3: Check if the linker can actually find the library

The ninja file may have `-lfoo` but the linker silently skips it if it can't find the library.
Check if the library exists in the cross-compile sysroot:

```bash
docker run --rm skiasharp-linux-gnu-cross-arm64 bash -c \
  "ls -la /usr/aarch64-linux-gnu/lib/libfontconfig*"
```

**Common issue:** The `-dev` package provides a broken symlink (`libfoo.so -> libfoo.so.1.2.3`)
but the actual `.so.1.2.3` file is in the runtime package (`libfoo1`), not the dev package.

### Step 4: Fix location depends on the issue

| Root Cause | Fix Location |
|------------|--------------|
| Library not in linker flags | `native/linux/build.cake` or `externals/skia/third_party/BUILD.gn` |
| Library not in cross-compile sysroot | `scripts/Docker/debian/clang-cross/*/Dockerfile` |
| Indirect dependency (A→B→C) | May need to link C explicitly, or fix B's linkage |

### Real Example: ARM64 fontconfig issue (#3369)

**Symptom:** `undefined symbol: uuid_generate_random` on ARM64 only

**Investigation:**
- x64 had: `libfontconfig.so.1` in DT_NEEDED
- ARM64 missing: `libfontconfig.so.1` in DT_NEEDED
- But ninja file had `-lfontconfig` for BOTH builds!

**Root cause:** Cross-compile Docker only had `libfontconfig1-dev` which provides a broken symlink.
The actual `libfontconfig.so.1.12.0` is in `libfontconfig1` (runtime package).

**Fix:** Add runtime package to Dockerfile alongside the dev package.

## Platform-Specific Crashes

Check build configuration in `native/{platform}/build.cake`:
- Linker flags (`extra_ldflags`)
- Compiler flags (`extra_cflags`)
- GN args for the specific architecture

## Memory Leaks

1. Check if the object implements `ISKReferenceCounted` or `ISKNonVirtualReferenceCounted`
2. Verify disposal pattern matches ownership:
   - `owns: false` → Don't dispose (temporary reference)
   - `owns: true` or manual ownership → Must dispose
3. Check for circular references between native objects
