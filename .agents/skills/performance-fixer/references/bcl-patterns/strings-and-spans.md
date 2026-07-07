# BCL patterns: strings & spans

General .NET fast-string techniques, applied to SkiaSharp's managed layer. Prefer the BCL here;
use a shared helper only for the specific benefit it documents ([../repo-helpers.md](../repo-helpers.md)).
Mind **all TFMs** — SkiaSharp targets net462/netstandard2.0 → net10; guard newer APIs with `#if` and
provide a correct fallback. See [../decision-framework.md](../decision-framework.md) for the
complexity rubric and [../measuring.md](../measuring.md) to verify.

## Span / ReadOnlySpan caller-owned overloads
- **Do:** add a `ReadOnlySpan<char>`/`Span<T>` overload so callers parse/convert out of a larger
  buffer without allocating a substring; the `string` overload delegates via `.AsSpan()`.
- **Instead of:** forcing a `Substring`/`new string` at every call site.
- **Why:** removes per-call allocation on hot parse/theming paths. Repeated across Toub's .NET 6–10
  posts as the top allocation-reduction pattern.
- **Complexity:** low · **TFM:** any (`System.Memory`) · **ABI:** additive overload — never replace
  the `T[]`/`string` one.
- SkiaSharp: `SKColor.Parse`, `SKFourByteTag.Parse`, `HarfBuzzSharp.Tag/Feature/Script` (see
  [../hot-paths/color.md](../hot-paths/color.md)).

## Allocation-free manual parse
- **Do:** parse hex/number spans char-by-char with a small `[MethodImpl(AggressiveInlining)]` nibble
  helper straight into the target `uint`/`int`.
- **Instead of:** `byte.TryParse`/`uint.TryParse` with `NumberStyles.HexNumber` + `CultureInfo`, or
  `new string(char, n)` per channel.
- **Why:** removes culture/NumberStyles overhead and the intermediate string; collapses `#if` TFM
  splits into one path. #4345: zero-alloc, 1.4–2.3×.
- **Complexity:** low · **TFM:** any · **ABI:** internal (body).
- **Watch out:** don't change the set of accepted inputs — prove agreement with the old parser
  across whitespace/case/`null`/empty/`#`/named-color rejection.

## Format into a caller buffer with TryWrite / TryFormat
- **Do:** `destination.TryWrite($"{a}:{b}", out var n)` or `value.TryFormat(dst, out n)` to write
  directly into a `Span<char>`.
- **Instead of:** `string.Format`/`ToString` into a temporary then copying.
- **Why:** avoids composite-format parsing, boxing, the `object[]`, and the intermediate string.
- **Complexity:** low · **TFM:** `TryWrite` net6+ (fall back to `TryFormat`/manual) · **ABI:** internal.

## string.Create for a known-length final string
- **Do:** `string.Create(len, state, static (span, s) => { ... })` to build the final string in one
  allocation.
- **Instead of:** `StringBuilder` + `ToString`, or concatenation, when the length is known.
- **Why:** one allocation, no builder churn; the `static` lambda avoids a closure capture.
- **Complexity:** low · **TFM:** modern TFMs (netstandard2.1+/net6+) — **not** on `net462`/
  `netstandard2.0`; guard `#if` with a `StringBuilder`/`char[]` fallback there · **ABI:** internal.

## UTF-8 literals (`u8`) for constant byte prefixes
- **Do:** `"\r\n--"u8` for constant ASCII/UTF-8 data copied into a native buffer.
- **Instead of:** `Encoding.UTF8.GetBytes("...")` of a constant at runtime.
- **Why:** the bytes are baked into the binary — no runtime encode, no allocation.
- **Complexity:** low · **TFM:** net7+ (C# 11) — guard · **ABI:** internal.

For repeated char/byte-set scanning (`IndexOfAny` over a literal set) use `SearchValues` — see
[collections.md](collections.md). For encoding whole text buffers to native, see
[interop-and-marshalling.md](interop-and-marshalling.md).
