# Hot path: text & fonts (SKFont, HarfBuzz, shaping)

Three related overheads on text/font/shaping paths, which are per-glyph and per-draw hot.

---

## A. Per-element interop in a loop

**The signature:** a managed loop calls a native function once per element when Skia exposes — or
the math allows — a **single bulk operation**, or the batch can be done with managed SIMD.

**Split candidates by whether a managed-only fix exists** — this is the key discriminator:
- **Managed-fixable:** a bulk native entry point or a pure-managed batch already exists (e.g. a
  per-point `matrix.MapPoint` loop that a managed SIMD `MapPoints` batch replaces — see
  [geometry-math.md](geometry-math.md) and
  [../bcl-patterns/numerics-and-simd.md](../bcl-patterns/numerics-and-simd.md)).
- **Native-API-needed → issue-only:** `SKFont` glyph-path loops (`sk_font_get_path` per glyph),
  `SKPathMeasure.GetPositionAndTangent` per point, `SKRuntimeEffect` uniform/child enumeration,
  `SKFontManager.FontFamilies`/`SKFontStyleSet` per-item calls have **no managed-only bulk
  alternative** — removing the per-element transition needs a *new bulk C API* under
  `externals/skia/**`, which is out of scope. **File the finding as an issue; do not open a managed
  PR that can't remove the loop.**

### Where to look
```bash
rg -n "for\s*\(|foreach\s*\(" binding/SkiaSharp binding/HarfBuzzSharp --glob '!*.generated.cs' -A 12 | rg -n "SkiaApi|HarfBuzzApi"
rg -n "GetGlyphPath|GetPositionAndTangent|MapPoint|Get.*Names|GetFamilyName|GetStyle" binding/SkiaSharp --glob '!*.generated.cs'
```
**Already optimized (skip):** `SKMatrix.MapPoints`/`MapVectors` (bulk), `SKFont.GetGlyphPaths`
(bulk), `SKBitmap.CopyTo` (no longer per-pixel), `SKWebpEncoder` (array then one bulk encode).

### Watch out (❌ don't)
Don't break buffer semantics while batching. **In-place** (`dst == src`) and **overlapping shifted
spans** change the required iteration direction (#4241's perspective path walks back-to-front).
Handle the **scalar tail** and odd/small counts. Prove parity at counts `{0,1,2,3,5,6,8,10,11,12}`,
in-place, and overlapping — the SIMD lane loop, the unrolled loop, and the tail are three code paths.

**Real case:** #4241 (batch `MapPoints`/`MapVectors`); #3489 (`SKBitmap.CopyTo`).

---

## B. Avoidable marshalling on text calls

**The signature:** the wrapper marshals a string/array per call — `AllocHGlobal`/`StringToHGlobal`
per shaper string, `features?.ToArray()`, `GetEncodedText` appending a NUL by string concat then
allocating the encoded `byte[]` — on a per-draw/per-shape path.

### Where to look
```bash
rg -n "Marshal\.|AllocHGlobal|StringToHGlobal|Encoding\..*GetBytes|\.ToArray\(\)" binding/SkiaSharp binding/HarfBuzzSharp --glob '!*.generated.cs'
```
Candidates: `HarfBuzzSharp.Font.Shape` (per-shaper HGlobal strings + `features?.ToArray()`),
`HarfBuzzSharp.Feature.ToString` (HGlobal per call), `Util.GetEncodedText` (NUL via string concat
then byte-array alloc). **Already optimized (skip):** `HarfBuzzSharp.Font.GlyphToString` (uses
`ArrayPool<byte>`), `SKTextBlob` (span-pinned text). See
[../bcl-patterns/interop-and-marshalling.md](../bcl-patterns/interop-and-marshalling.md) and
[../repo-helpers.md](../repo-helpers.md) (`RentArray`, `RentHandlesArray`).

### Watch out (❌ don't)
`GetEncodedText` is **not** UTF-8-only — it switches on `SKTextEncoding` (Utf8/Utf16/Utf32) with an
`addNull` variant. Prove byte-for-byte parity across every encoding and both `addNull` cases — the
canonical warning is in
[../bcl-patterns/interop-and-marshalling.md](../bcl-patterns/interop-and-marshalling.md).

---

## C. Redundant managed work / memoization

**The signature:** the managed code recomputes an expensive result (text shaping, path measuring)
that is invariant across identical calls, re-allocates an immutable that could be shared, boxes via
LINQ on a hot loop, or fails to inline a trivial forwarding wrapper.

**Why it matters.** None of this touches Skia — pure managed waste. #3033 adds caching to
`DrawShapedText` so repeated identical shaping becomes a lookup.

### Slow → Fast
```csharp
// Slow (❌): full HarfBuzz shaping every call, even if identical
var shaped = shaper.Shape(text, font);
// Fast (✓): memoize keyed on EVERYTHING that affects output
if (!cache.TryGetValue(key, out var shaped)) cache[key] = shaped = shaper.Shape(text, font);
```

### Watch out (❌ don't)
A cache keyed on the **wrong** identity returns a stale/incorrect result — key on *everything* that
affects the output (text, font, size, features, direction) and **bound the cache** so it is not
unbounded growth (a perf fix that becomes a leak — `memory-leak-fixer` territory). Don't
`AggressiveInlining` anything non-trivial (i-cache pressure). Prove the memoized/rewritten path
returns identical results across the input space.

**Real case:** #3033 (add caching to `DrawShapedText`).
