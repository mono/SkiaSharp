# SkiaSharp shared helpers and the native oracle

Before reaching for a raw BCL primitive or hand-rolling one, check whether SkiaSharp already has a
shared helper for the job. This mirrors the aspnetcore "prefer the BCL, use a helper only for the
specific benefit it documents" rule — adapted to a binding whose shared helpers exist to manage the
P/Invoke boundary.

## The rule

Prefer the BCL abstraction by default. Use a shared SkiaSharp helper only for the specific benefit
it provides: pooling across the interop boundary, allocation-free native-string handling, or the
object-tracking/ownership machinery. When both a BCL type and a helper fit, the reason for the
helper must be clear.

## Interop / allocation helpers (`binding/SkiaSharp/Util.cs`)

### `Utils.RentArray<T>` / `RentedArray<T>`
A pooled-array wrapper (`internal readonly ref struct` over `ArrayPool<T>.Shared`) that returns the
array on `Dispose`, with implicit conversions to `Span<T>`/`ReadOnlySpan<T>`.
- **Use it for:** a medium/large transient buffer you pin and pass to native once (glyph buffers,
  handle arrays), instead of `new T[n]`. `using var rented = Utils.RentArray<float>(n);`
- **Not for:** tiny fixed buffers — `stackalloc` is cheaper (see
  [bcl-patterns/memory-and-buffers.md](bcl-patterns/memory-and-buffers.md)).

### `Utils.RentHandlesArray(SKObject[] objects)`
Rents an `IntPtr[]` and fills it with the objects' handles — the idiomatic way to marshal an array
of wrappers to a native function that takes `void**`. Prefer this over `objects.Select(o => o.Handle).ToArray()`
(which allocates) in APIs like `SKImageFilter.CreateMerge`.

### `Utils` span & `Encoding` polyfills (`IntPtr.AsSpan`, `Encoding.GetByteCount`, `Encoding.GetBytes`)
`binding/SkiaSharp/Util.cs` defines `internal` extension methods so wrapper code can use modern span
APIs on **all** target frameworks without `#if` clutter at call sites:
- `ptr.AsSpan(int size)` / `ptr.AsReadOnlySpan(int size)` — wraps a native `IntPtr` as a
  `Span<byte>` / `ReadOnlySpan<byte>` of known length.
- `encoding.GetByteCount(ReadOnlySpan<char>)`, `encoding.GetBytes(ReadOnlySpan<char>, Span<byte>)`,
  and `encoding.GetBytes(ReadOnlySpan<char>)` — on `netstandard2.1+` / `.NET`, C# overload
  resolution picks the built-in BCL instance methods on `Encoding`; on `netstandard2.0` / `.NET
  Framework`, it binds to these pointer-based extension methods in `Utils`.
- **Rule:** When a BCL span method is missing on `netstandard2.0` / `net462` / `net48`, add or reuse
  an extension method in `Util.cs` (`Utils`) rather than scattering `#if NETSTANDARD2_1_OR_GREATER`
  blocks across wrapper methods.

### Direct writes into `SKData.Create(size)` vs `SKData.CreateCopy(bytes)`
When creating an `SKData` from encoded text or computed bytes whose final byte count is known up
front (e.g. `SKData.FromCString`):
- **Do:** allocate the uninitialized native buffer once with `var data = SKData.Create(size);`, wrap
  its pointer with `var bytes = data.Data.AsSpan(size);`, and encode/write directly into `bytes`.
- **Instead of:** allocating a managed `new byte[size]` (or renting/stackallocing a scratch buffer)
  and then calling `SKData.CreateCopy(bytes)`, which pays for a second copy into native memory.
- **Watch out for hidden P/Invokes on convenience properties:** `SKData.Span` calls **both** `Data`
  (`sk_data_get_data`) and `Size` (`sk_data_get_size`). When you already know `size` (because you
  just called `SKData.Create(size)`), use `data.Data.AsSpan(size)` to avoid the redundant
  `sk_data_get_size` P/Invoke.
- **Initialize every byte the contract requires:** `SKData.Create(size)` is uninitialized native
  memory. Explicitly write terminators/sentinels and prove parity for null, empty, non-ASCII, and
  embedded-NUL inputs.

### `StringUtilities.GetEncodedText(...)` / `SKString`
`StringUtilities.GetEncodedText` encodes text for native calls, switching on `SKTextEncoding`
(`Utf8`→`Encoding.UTF8`, `Utf16`→`Encoding.Unicode`, `Utf32`→`Encoding.UTF32`) with an `addNull`
variant; `SKString` is the wrapper for a native `SkString`. When optimizing these, preserve
byte-for-byte parity across **every** encoding and both `addNull` cases — the canonical warning is in
[bcl-patterns/interop-and-marshalling.md](bcl-patterns/interop-and-marshalling.md).

## Object-tracking / ownership machinery (`SKObject.cs`, `HandleDictionary.cs`)

Not "helpers to call" so much as the hot infrastructure a fix in family
`handles-and-collections` touches — know it before changing it:

- **`SKObject.GetOrAddObject` / `GetObject` / `GetInstance`** — look up or create the managed
  wrapper for a native handle via the `HandleDictionary` (a lock + dictionary lookup on **every**
  native object creation — the hottest shared path in the binding).
- **`SKObject.OwnedBy(child, owner)`** — registers a child in the owner's `ConcurrentDictionary` so
  it is kept alive / disposed with the owner. Called by many getters; the reason `SKSurface.Canvas`
  was cached (#4247) was to stop redoing `GetObject` + `OwnedBy` per access.
- **`GC.KeepAlive(this)`** — used pervasively after native calls to keep the owner rooted for the
  duration; **any cache/`in`/`ref` refactor must preserve the keep-alive** or it introduces a GC
  race. Grep shows it in nearly every wrapper — treat it as part of the contract, not noise.

## The native oracle (for equivalence tests and benchmarks)

Both `SkiaSharp.Tests` and `SkiaSharp.Benchmarks` are listed in
`binding/SkiaSharp/Properties/SkiaSharpAssemblyInfo.cs`'s `InternalsVisibleTo`, so **both can call
`SkiaApi.sk_*` and other `internal` members directly**. This is what makes the two-proof workflow
possible:
- **Equivalence tests** call the native function as the oracle and assert the managed result equals
  it (PR #4241's `SKMatrixManagedTests` do exactly this).
- **Benchmarks** can measure the native path (`Old`) against the managed path (`New`) in one
  process.

## Bootstrapping (managed-C# work)

The native library is consumed as a **pre-built package** — you never build native code for this
skill. In every mode, bootstrap only after the source candidate has a citable hot path/invariant
and clears de-dup:

```bash
dotnet tool restore
dotnet cake --target=externals-download  # only when setup has not already populated output/native
```

Do not restore tools or download natives while selecting a focus area, scanning source, or
checking duplicates; quiet and duplicate runs do neither. Autonomous runs perform this in Phase
1.4; interactive modes perform it after selecting a candidate and before the first proof. Reuse
natives provisioned by the Copilot setup workflow instead of downloading the packages again.

If you have modified anything under `externals/skia/**` you are out of scope for this skill (that
requires a source build). This skill's fixes are always managed-C# only.
