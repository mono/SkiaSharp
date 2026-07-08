# Detection signals: which reference to consult

Route from **what the managed code is doing** to the reference that covers it. The point is
progressive disclosure — open a section only when the code under scan touches that area. Match on
what the code *does*, not just a token; then apply [decision-framework.md](decision-framework.md)
(hot vs cold path, complexity, the two-proof gate) before acting. A signal is a reason to look,
**not a verdict** — a match on a cold path is recorded briefly and dropped.

Grep starting points to sweep the managed binding (`binding/**`, `source/**`; never
`*.generated.cs` or `externals/skia/**`) are in each hot-path/bcl-pattern reference.

| When the managed code does this | Consult |
|---|---|
| A property/method on a blittable struct (`SKMatrix`/`SKRect`/`SKPoint`/`SKColorF`) calls `SkiaApi.sk_*` to compute a few float ops — invert, concat, map point/vector/rect/radius | [hot-paths/geometry-math.md](hot-paths/geometry-math.md) |
| `SKColor`/`SKColorF` parse/format/convert: `Parse`/`TryParse`, `byte.TryParse`+`NumberStyles`, `new string(...)`, `FromHsl`, missing `ReadOnlySpan<char>` overload | [hot-paths/color.md](hot-paths/color.md), [bcl-patterns/strings-and-spans.md](bcl-patterns/strings-and-spans.md) |
| A getter wraps a child native object every access (`GetObject`/`OwnedBy` with no cache); a `Dictionary`/`ConcurrentDictionary` created without capacity; `HandleDictionary`/lock on a hot registration path | [hot-paths/handles-and-collections.md](hot-paths/handles-and-collections.md), [bcl-patterns/collections.md](bcl-patterns/collections.md) |
| A `for`/`foreach` loop containing `SkiaApi.sk_*` per element (glyph paths, uniform enumeration); HarfBuzz `Shape`; `AllocHGlobal`/`StringToHGlobal`/`Encoding.GetBytes` per text call; `GetEncodedText` | [hot-paths/text-and-fonts.md](hot-paths/text-and-fonts.md), [bcl-patterns/interop-and-marshalling.md](bcl-patterns/interop-and-marshalling.md) |
| Pixel/scanline copy element-by-element; `SKBitmap.Bytes`/`Pixels` `.ToArray()`; `SKColor[]`↔`uint*`; blittable reinterpret opportunities | [hot-paths/pixels-and-images.md](hot-paths/pixels-and-images.md), [bcl-patterns/memory-and-buffers.md](bcl-patterns/memory-and-buffers.md) |
| `new byte[]`/`new char[]`/`new IntPtr[]` scratch buffers, `.ToArray()` on a hot path, a large blittable struct passed by value | [bcl-patterns/memory-and-buffers.md](bcl-patterns/memory-and-buffers.md), [repo-helpers.md](repo-helpers.md) (`Utils.RentArray`, `RentHandlesArray`) |
| `IndexOfAny(char[])`/manual char-set scans in a parser/tokenizer (SVG path data), repeated membership checks | [bcl-patterns/collections.md](bcl-patterns/collections.md) (`SearchValues`), [bcl-patterns/strings-and-spans.md](bcl-patterns/strings-and-spans.md) |
| A P/Invoke declaration or blittable-signature concern, `[MarshalAs]`, `bool`/`char` marshalling, `LibraryImport`/`DllImport` | [bcl-patterns/interop-and-marshalling.md](bcl-patterns/interop-and-marshalling.md) |
| `System.Numerics.Vector*`, `Vector128`/`Vector256`, `MathF`, manual SIMD, batch math | [bcl-patterns/numerics-and-simd.md](bcl-patterns/numerics-and-simd.md) |
| A one-line forwarding wrapper or nibble/parse helper the JIT should inline; a non-`sealed` internal type; an expensive computation (shaping, path measure) recomputed with no cache | [bcl-patterns/numerics-and-simd.md](bcl-patterns/numerics-and-simd.md), [hot-paths/text-and-fonts.md](hot-paths/text-and-fonts.md) |
| `StringBuilder`, hand-rolled pooling, a locally-reimplemented ArrayPool/throw-helper | [repo-helpers.md](repo-helpers.md) (reuse the shared helper) |

Notes:
- One line can match several signals — open each relevant section and pick the highest-leverage,
  lowest-complexity fix.
- Confirm the code is on a hot path (the SkiaSharp hot list in [decision-framework.md](decision-framework.md))
  before proving a change. If it's cold, note it and move on.
- Some per-element native loops have **no managed-only bulk alternative** (they'd need a new bulk
  C API under `externals/skia/**`). Those are **issue-only** — see the split in
  [hot-paths/text-and-fonts.md](hot-paths/text-and-fonts.md).
