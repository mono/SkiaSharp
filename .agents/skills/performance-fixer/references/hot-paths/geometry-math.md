# Hot path: geometry & math (SKMatrix, SKRect, SKPoint)

**The signature:** a method or property on a pure-managed blittable struct computes something —
invert, concat, map a point/vector/rect/radius — by calling into native Skia, paying a full
P/Invoke transition (pinning, marshalling, a managed→native→managed hop) for math that is a few
multiplies and adds with **no native state**. Porting that math to managed C# removes the
transition entirely.

**Why it's the highest-leverage family.** On a per-point or per-frame path the transition cost
*dominates*; the math itself is nearly free. #4241 measured `MapPoint (perspective)` at **9.3 ns
native → 0.3 ns managed (24×)**, `MapVector` 8×, and 1.6–2.9× across invert/concat/mapRect.

**Complexity: HIGH** (per [decision-framework.md](../decision-framework.md)) — a faithful bit-exact
port with a cross-runtime float-determinism invariant. Reserve for the proven hot path, prove both
ways ([measuring.md](../measuring.md)), isolate, and keep a simple reference.

## Where to look

Blittable value types in `binding/SkiaSharp/`: `SKMatrix.cs`, `SKColorF.cs`, `SKPMColor.cs`,
`SKRect.cs`, `SKPoint.cs`, `SKSize.cs`. Ask of each hit: is the native side just a handful of float
ops with **no native state**?

```bash
rg -n "SkiaApi\.sk_(matrix|color|color4f|rect|point|size)" binding/SkiaSharp --glob '!*.generated.cs'
rg -n "Map(Point|Rect|Vector|Radius)|Invert|Concat|PreConcat|PostConcat|operator" binding/SkiaSharp
```

Candidates seen in this tree: `SKMatrix.MapPoint/MapVector/MapRadius/MapRect/Concat/TryInvert` (the
#4241 port — **check its state first**, likely already covered), the `SKColor`⇄`SKColorF` channel
conversion, single-value `SKPMColor` premultiply. **Already managed (skip):** `SKMatrix44` (backed
by `System.Numerics.Matrix4x4`), most of `MathTypes.cs`, the *array* `SKPMColor` premultiply.

## Slow → Fast

**Slow (❌):**
```csharp
public readonly SKPoint MapPoint(float x, float y) {
    SKPoint result;
    SkiaApi.sk_matrix_map_xy(ref this, x, y, &result);  // full P/Invoke for 6 FMAs
    return result;
}
```

**Fast (✓):**
```csharp
// AFFINE-ONLY illustration. The real SKMatrix.MapPoint must dispatch on the type mask:
// perspective matrices need the full Skia path (w' = persp0*x + persp1*y + persp2, then divide) —
// porting the affine formula alone silently corrupts perspective transforms.
public readonly SKPoint MapPoint(float x, float y) =>
    new SKPoint(scaleX * x + skewX  * y + transX,
                skewY  * x + scaleY * y + transY);
```
For batch APIs (`MapPoints`/`MapVectors`) mirror Skia's `skvx` procs with `System.Numerics.Vector4`
(two points per lane) + `Vector128.Shuffle` on net8+ and a portable `Vector4` fallback on older
TFMs — see [../bcl-patterns/numerics-and-simd.md](../bcl-patterns/numerics-and-simd.md). Keep the
genuinely-hard cases native (perspective `MapRect` uses Skia's edge-clipping engine — not worth
porting).

## Watch out (❌ don't)

Don't eyeball the math from the header — **port Skia's actual algorithm** (float-vs-double, op
order, type-mask/`±0`/`Inf`/`NaN` semantics) and prove **bit-exact** equality against
`SkiaApi.sk_matrix_*` across degenerate/NaN/Inf inputs. A port that is "close" changes rendering
output on an edge — a regression, not an optimization. Don't port a call that touches native
*state*; only touch pure math.

> **Float-determinism trap (must handle).** Managed float math is bit-exact with native Skia only
> on runtimes that round every op to `float32` — x64 .NET Framework and all .NET Core/5+ on
> x64/x86/ARM64 (SSE2/NEON). **x86 .NET Framework uses the x87 FPU** (80-bit intermediates): it
> diverges categorically for `0*∞`-style inputs (SSE→`NaN`, x87→finite) and by thousands of ULP in
> cancellation-prone paths. #4241 handles this with a `static readonly bool UseNativeMath` (detected
> once via `RuntimeInformation`) that routes the ported methods **back through the native C API on
> x86 .NET Framework only** — output stays byte-identical there, the managed fast path (an
> always-false branch) is unaffected everywhere else, and the equivalence tests assert bit-parity on
> *all* platforms. **Any managed float port must do the same.**

## Real case
#4241 — SKMatrix math ported native→managed (bit-exact, x87 fallback), up to 24×.
