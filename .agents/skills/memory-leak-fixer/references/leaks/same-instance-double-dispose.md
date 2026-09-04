# 2 — Same-instance double-dispose

**Where to look.** `binding/SkiaSharp/**`. `grep -rnE "Subset|ToRasterImage|== source|!= source" binding/SkiaSharp` — any method that can return `this`.

**What it is.** Some methods may return the **same** instance rather than a new one —
`SKImage.Subset` (can return `this`), `SKImage.ToRasterImage(ensurePixelData:false)`,
`SKImage.Encode` (routes through `ToRasterImage`). A caller that disposes both the source and
the "result" then disposes the same native object twice.

**Why it's bad.** Double-free → `AccessViolationException`, or the source is destroyed out
from under the caller who still needs it.

**Leak/crash (❌):**
```csharp
var raster = image.ToRasterImage();   // may return `image` itself
raster.Encode(...);
raster.Dispose();
image.Dispose();                       // ❌ if raster == image, second free crashes
```

**Fix (✓):** the framework guards this internally, and callers should too:
```csharp
var raster = image.ToRasterImage();
raster.Encode(...);
if (raster != image)                   // never dispose a same-instance return twice
    raster.Dispose();
image.Dispose();
```
Framework-side pattern (see `SKImage.Encode`): `if (this != raster) raster.Dispose();`.

**Watch out (❌ don't):** don't add an *unconditional* `result.Dispose()` — the reference
check `if (result != source)` is the whole fix; dropping it re-introduces the double-free.
And don't dispose the source before you're finished with the result, since they may be the
same object.

**Real cases:** the `Subset`/`ToRasterImage` contract in `documentation/dev/memory-management.md`.
