# Hot path: pixels & images (SKBitmap, SKPixmap, SKImage)

**The signature:** pixel/scanline data is copied element-by-element, or a property always
materializes a fresh array (`.ToArray()`), where a **single bulk copy** or a **zero-copy blittable
reinterpret** would do — on paths that move whole buffers (decode → bitmap, pixel readback, format
conversion).

**Why it matters.** Pixel buffers are large; per-element conversion and redundant array
materialization are pure managed overhead on top of the copy Skia already does. The batch/bulk
approach amortises or eliminates it.

**Complexity: LOW–MEDIUM** — a bulk copy or a `MemoryMarshal.Cast` view is low; anything with
`unsafe` pointer juggling or a layout invariant that depends on endianness is medium (comment it).

## Where to look

```bash
rg -n "for\s*\(|foreach" binding/SkiaSharp/SKBitmap.cs binding/SkiaSharp/SKPixmap.cs binding/SkiaSharp/SKImage.cs -A 8 | rg -n "SkiaApi|for|\.ToArray"
rg -n "\.ToArray\(\)|new byte\[|new SKColor\[|uint\*|MemoryMarshal|Unsafe\." binding/SkiaSharp/SKBitmap.cs binding/SkiaSharp/SKPixmap.cs
```

Candidates seen in this tree: `SKBitmap.Bytes` (always `ToArray`), `SKBitmap.Pixels` (allocates
`SKColor[]` then passes as `uint*`), pixel-conversion helpers where `Span<SKColor>` and
`Span<uint>` share a layout. **Already optimized (skip):** `SKBitmap.CopyTo` (no longer a per-pixel
loop), `SKPixmap` (exposes pixel spans to avoid array copies). See
[../bcl-patterns/memory-and-buffers.md](../bcl-patterns/memory-and-buffers.md).

## Slow → Fast

**Slow (❌):**
```csharp
// Element-wise conversion where the layouts already match.
var colors = new SKColor[n];
for (var i = 0; i < n; i++) colors[i] = (SKColor)raw[i];
```

**Fast (✓):**
```csharp
// Free blittable view — no copy, no per-element convert (layouts must match).
var colors = MemoryMarshal.Cast<uint, SKColor>(raw);
// — or one bulk native call over the whole span instead of per pixel.
```

## Watch out (❌ don't)

`MemoryMarshal.Cast`/`Unsafe.As` require both types to be **blittable with matching layout,
size, and endianness** — verify before reinterpreting (a struct with a `bool`/reference field or
platform-dependent packing is unsafe to cast). A property like `Bytes` that callers expect to return
an **independent copy** must keep doing so — don't hand out an aliasing view where a copy was the
contract (that is a behaviour change). Prove identical bytes out, and for anything that renders, add
a rendered-bitmap parity check ([../measuring.md](../measuring.md)).

## Real case
#3489 (optimize `SKBitmap.CopyTo`); the `MemoryMarshal`/blittable-reinterpret technique in
[../bcl-patterns/memory-and-buffers.md](../bcl-patterns/memory-and-buffers.md).
