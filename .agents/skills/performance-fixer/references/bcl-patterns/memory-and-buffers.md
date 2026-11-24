# BCL patterns: memory & buffers

Removing allocations and copies in the managed layer — the lowest-risk, highest-frequency win.
Prefer a shared helper where one exists ([../repo-helpers.md](../repo-helpers.md)). Prove no
allocation regression with `[MemoryDiagnoser]` ([../measuring.md](../measuring.md)).

## `stackalloc` + `Span<T>` for small transient buffers
- **Do:** `Span<T> tmp = n <= cap ? stackalloc T[n] : ...;` for short-lived scratch (≤ ~256–1024
  bytes) you write before reading. Pair with `[SkipLocalsInit]` on the method to skip zeroing.
- **Instead of:** `new T[n]` for a temporary (e.g. `SKPath.GetLine` allocates `new SKPoint[2]`).
- **Why:** stack, no GC, no pinning of a managed array.
- **Complexity:** low (write-before-read is local/self-evident) · **TFM:** any · **ABI:** internal.
- **Watch out:** **cap the size** and never `stackalloc` inside a loop or with unbounded `n` — that
  is a stack-overflow *crash*, not a slowdown. Above the cap, fall back to `ArrayPool`/heap.

## `ArrayPool<T>` / `Utils.RentArray` for medium/large transient buffers
- **Do:** rent from `Utils.RentArray<T>(n)` (the repo's `ArrayPool` ref-struct wrapper) or
  `ArrayPool<T>.Shared`; return in a `using`/`finally`. Use `Utils.RentHandlesArray` for `IntPtr[]`
  of wrapper handles.
- **Instead of:** `new T[n]` / `objects.Select(o => o.Handle).ToArray()` per call.
- **Why:** reuses arrays, cutting GC pressure for frequent temporaries (glyph buffers, filter-handle
  arrays). SkiaSharp already uses `RentedArray` for glyph positions.
- **Complexity:** low · **TFM:** any · **ABI:** internal.
- **Watch out:** don't pool *tiny* buffers (rent/return overhead exceeds `stackalloc`).

## `MemoryMarshal.Cast` / `AsBytes` for blittable reinterpret
- **Do:** `MemoryMarshal.Cast<uint, SKColor>(span)` to view blittable data as another type with **no
  copy** when layouts match (pixel/color paths).
- **Instead of:** an element-wise convert loop, or an extra allocation, to reinterpret.
- **Why:** free view instead of a copy.
- **Complexity:** low, but **medium** once correctness depends on endianness/alignment you can't see
  locally · **TFM:** any (`System.Memory`) · **ABI:** internal.
- **Watch out:** both types must be blittable with matching layout/size (no `bool`/reference fields,
  no platform packing). Verify before casting; a property that promised an *independent copy* must
  keep returning one.

## `in` / `ref readonly` to avoid struct copies
- **Do:** pass large blittable structs (`SKMatrix` 36 bytes, `SKMatrix44`, `SKSamplingOptions`,
  `GR*Info`) by `in` through internal helpers and *new* overloads. SkiaSharp already uses
  `in SKMatrix` on canvas draws.
- **Instead of:** copying the struct by value through several call layers.
- **Why:** passes a reference; real bytes not moved per layer.
- **Complexity:** low–medium · **TFM:** any · **ABI:** changing an *existing public* by-value
  parameter to `in` is a signature change — **additive overloads / internal only**.
- **Watch out:** never `in` a struct the method mutates (or that reintroduces a defensive copy).

## `params ReadOnlySpan<T>` overloads (net9 / C# 13)
- **Do:** add `DrawPoints(params ReadOnlySpan<SKPoint>)` convenience overloads that avoid the
  implicit `params T[]` heap array.
- **Complexity:** low · **TFM:** net9+ (guard) · **ABI:** additive — watch ambiguity with the
  existing `params T[]`.
