# BCL patterns: interop & marshalling

The techniques specific to a P/Invoke binding — reducing the marshalling the *managed* wrapper adds
around a native call. The generated `SkiaApi` P/Invoke declarations in `*.generated.cs` are
**off-limits to hand-edit**; the lever you control is the managed wrapper. Prove byte-for-byte
parity for anything that reaches native ([../measuring.md](../measuring.md)).

## Keep managed signatures blittable and exact
- **Do:** keep wrapper signatures blittable (structs/spans passed directly), match the native C
  declaration exactly.
- **Instead of:** marshalled arrays/strings where a pinned span/pointer works; "simplifying"
  `bool`/`char` marshalling.
- **Why:** blittable types pin/pass directly instead of copying through a marshal buffer.
- **Complexity:** low–medium · **TFM:** any · **ABI:** internal.
- **Watch out:** never change `bool`/`char` marshalling without checking the native layout — C++
  `bool` is 1 byte; default .NET `bool` marshals as 4.

## Avoid temp strings/byte[] in text-encoding interop
- **Do:**
  1. When the target is a new `SKData` (such as `SKData.FromCString`), compute `var byteCount = encoding.GetByteCount(span)`, allocate writable native memory once with `var data = SKData.Create(byteCount + 1)`, encode directly into `var bytes = data.Data.AsSpan(byteCount + 1)` via `encoding.GetBytes(span, bytes)`, and explicitly zero the terminator (`bytes[byteCount] = 0`).
  2. When a transient buffer is only passed to a native P/Invoke for the duration of the call, encode into a bounded `stackalloc byte[...]` or `Utils.RentArray<byte>(...)` `Span<byte>` rather than allocating `new byte[]`.
  3. Rely on `binding/SkiaSharp/Util.cs` (`Utils.GetByteCount`, `Utils.GetBytes`, `Utils.AsSpan`) so `ReadOnlySpan<char>` / `Span<byte>` calls compile across all TFMs without `#if` clutter at call sites.
- **Instead of:** appending a NUL by string concatenation (`(str ?? string.Empty) + '\0'`), or allocating an intermediate `new byte[byteCount + 1]` and copying it a second time via `SKData.CreateCopy(bytes)`.
- **Why:** writing directly into `SKData.Create(size)` eliminates both the managed array allocation and the extra copy into native memory; using `data.Data.AsSpan(size)` instead of `data.Span` also avoids a redundant `sk_data_get_size` P/Invoke when `size` is already known (#5429).
- **Complexity:** low · **TFM:** all TFMs via `Util.cs` span polyfills · **ABI:** internal (don't change public return types unless adding an overload).
- **Watch out:**
  - `SKData.Create(size)` allocates **uninitialized** native memory (`sk_data_new_uninitialized`), so you must explicitly write any trailing NUL terminator bytes (`bytes[byteCount] = 0`).
  - `GetEncodedText` is **not** UTF-8-only — it switches on `SKTextEncoding` (`Utf8`→UTF8, `Utf16`→Unicode, `Utf32`→UTF32) with an `addNull` variant. Prove parity for **every encoding AND both `addNull` cases** — the NUL terminator is 1/2/4 bytes.

## Cache delegates / prefer static callbacks
- **Do:** cache a `static` delegate for a native callback, or use `delegate* unmanaged` /
  `[UnmanagedCallersOnly]` on modern TFMs (retain the delegate fallback for old ones).
- **Instead of:** allocating a lambda per call on callback-heavy paths (managed streams, drawables).
- **Why:** avoids per-call delegate allocation and marshalling setup.
- **Complexity:** medium · **TFM:** function pointers net5+ · **ABI:** internal if the callback
  signature is unchanged.

## `LibraryImport` source-gen marshalling (net7)
- Already conditional in this repo (`USE_LIBRARY_IMPORT`). It reduces stub overhead and helps
  AOT/trimming, but the declarations live in **generated** `SkiaApi` — **never hand-edit them**.
  Your lever is keeping the managed wrapper signatures blittable.

## `[SuppressGCTransition]` (net5, advanced — usually not actionable here)
- Skips the GC-mode transition for *tiny, non-blocking, non-callback, non-throwing* leaf getters
  (width/height/sample-count). **But** it must annotate the P/Invoke declaration, which lives in the
  **generated** bindings — applying it needs a **binding-generator/config change plus maintainer
  approval**, not a hand-edit. Never on draw/encode/decode/stream/lock/callback calls. Treat as a
  proposal to **file**, not a quick fix.
