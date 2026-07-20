# BCL patterns: collections & searching

Sizing, specialized lookups, and set-search primitives for the managed layer. See
[../hot-paths/handles-and-collections.md](../hot-paths/handles-and-collections.md) for the
object-tracking specifics and [../decision-framework.md](../decision-framework.md) for the rubric.

## Size collections to a known/expected count
- **Do:** create `Dictionary`/`List`/`ConcurrentDictionary` with `capacity` (and
  `concurrencyLevel`) when the count is known or estimable.
- **Instead of:** default-sized collections that rehash/resize under load.
- **Why:** avoids rehash and (for concurrent maps) contention on hot registration paths — the
  `HandleDictionary`, `SKRuntimeEffect`/glyph-path caches.
- **Complexity:** low · **TFM:** any · **ABI:** internal.
- **Watch out (memory):** size to *evidence*. A large capacity on a **global/shared** map is
  justified; a large capacity on a **per-object** map (thousands of instances) multiplies memory
  across every instance — size those to the small known count or leave them default.

## `SearchValues<T>` for repeated char/byte-set search
- **Do:** cache a `static readonly SearchValues<char>` (from `SearchValues.Create`) and pass it to
  `IndexOfAny`/`ContainsAny` over spans (SVG path-data tokenizing, separator/invalid-char scans).
- **Instead of:** `IndexOfAny(char[])`, rebuilt delimiter arrays, or open-coded membership loops.
- **Why:** the runtime preselects a vectorized/bitmap/probabilistic strategy instead of rebuilding
  state each call.
- **Complexity:** low · **TFM:** net8+ — guard `#if NET8_0_OR_GREATER`, fall back to the manual scan
  · **ABI:** internal.

## `ContainsAny` when only existence matters
- **Do:** `span.ContainsAny(searchValues)` instead of `span.IndexOfAny(searchValues) >= 0`.
- **Why:** avoids computing an index you discard.
- **Complexity:** low · **TFM:** net8+ · **ABI:** internal.

## Frozen collections for build-once, read-many maps
- **Do:** `FrozenDictionary`/`FrozenSet` for static lookup tables built once and queried many times
  (named colors, codec/extension tables).
- **Instead of:** a `Dictionary` rebuilt or probed on a hot path.
- **Why:** expensive to build, very fast to read.
- **Complexity:** low · **TFM:** net8+ — guard, fall back to `Dictionary` · **ABI:** internal.
- **Watch out:** **not** for maps rebuilt often (build cost dominates).

## `CollectionsMarshal` for in-place access
- **Do:** `CollectionsMarshal.AsSpan(list)` / `GetValueRefOrAddDefault` to read/update in place.
- **Why:** avoids re-lookups and copies.
- **Complexity:** medium (plants an "don't structurally modify while holding the span/ref"
  invariant — comment it) · **TFM:** net6+ · **ABI:** internal.

Avoid LINQ (`Where`/`Select`/`Any`/`Count`/`ToList`) in tight/hot loops — the enumerator and closure
allocate; a plain `for` over a `Span`/array is allocation-free.
