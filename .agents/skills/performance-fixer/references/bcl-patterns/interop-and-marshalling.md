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
- **Do:** on modern TFMs, `encoding.GetByteCount(ReadOnlySpan<char>)` then encode into a
  `stackalloc`/pooled `Span<byte>`.
- **Instead of:** appending a NUL by string concatenation and allocating an encoded `byte[]` per
  call (`Util.GetEncodedText`).
- **Why:** removes both the temp string and the array on per-text-call paths.
- **Complexity:** low–medium · **TFM:** better on net-modern; keep the `byte[]` path as old-TFM
  fallback · **ABI:** internal (don't change the return type unless all callers are updated).
- **Watch out:** `GetEncodedText` is **not** UTF-8-only — it switches on `SKTextEncoding`
  (`Utf8`→UTF8, `Utf16`→Unicode, `Utf32`→UTF32) with an `addNull` variant. Prove parity for **every
  encoding AND both `addNull` cases** — the NUL terminator is 1/2/4 bytes.

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
