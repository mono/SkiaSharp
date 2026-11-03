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
- **Do:** compute the encoded size first, then write into the final destination when it exposes a
  writable buffer. Otherwise use bounded `stackalloc` or `Utils.RentArray<byte>` for transient
  interop buffers. Reuse the `Util.cs` span polyfills rather than adding call-site `#if` blocks.
- **Instead of:** concatenating a terminator onto the source string or allocating an intermediate
  encoded array that is immediately copied again.
- **Why:** removes temporary strings, managed arrays, and redundant copies/P/Invokes.
- **Complexity:** low–medium · **TFM:** all TFMs via `Util.cs` polyfills · **ABI:** internal.
- **Watch out:** the destination may be uninitialized, so write every sentinel/terminator byte.
  `GetEncodedText` supports UTF-8/16/32; prove parity for every encoding and `addNull` value.
  See [../repo-helpers.md](../repo-helpers.md) for the concrete `SKData` pattern.

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
