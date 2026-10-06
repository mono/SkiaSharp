# Hot path: color (SKColor, SKColorF)

**The signature:** a hot color parse/format/convert helper allocates (`new string(char, n)`,
`.ToArray()`, `.Split(...)`, boxing via `NumberStyles`/`CultureInfo`) or forces callers to allocate
a substring because it lacks a `ReadOnlySpan<char>`/`Span<T>` overload.

**Why it matters.** Color hydration sits on startup/theming hot paths (XAML/CSS/JSON/theme files —
thousands of colors parsed at launch). #4345 replaced per-channel `byte.TryParse` +
`uint.TryParse`-with-`NumberStyles.HexNumber` (and a `new string(char,2)` per channel on old TFMs)
with a manual span parser: **zero allocations, 1.4–2.3× faster** across all shapes, and the `#if`
TFM split vanished (one implementation everywhere).

**Complexity: LOW** (per [decision-framework.md](../decision-framework.md)) — a local, drop-in span
parser plus an additive overload. Apply on hot paths by default; still prove parity (§Watch out).

## Where to look

Managed parse/format/convert helpers on structs and static factories, in `binding/SkiaSharp/` and
`binding/HarfBuzzSharp/`:

```bash
rg -n "Parse|TryParse|ToString|new string\(|\.Split\(|\.ToArray\(|NumberStyles|string\.Format|Encoding\." binding/SkiaSharp binding/HarfBuzzSharp --glob '!*.generated.cs'
rg -n "public .*Parse \(string|public .*TryParse \(string" binding --glob '!*.generated.cs'
```

Candidates seen in this tree: `SKFourByteTag.Parse` / `HarfBuzzSharp.Tag.Parse` (allocate `char[4]`,
no span overload), `HarfBuzzSharp.Feature`/`Script` (string-only), `SKPath.ParseSvgPathData`.
**Already optimized (skip):** `SKColor.Parse/TryParse` (already span + manual hex — the #4345 win),
`SKTextBlob` (string overload forwards to span), `HarfBuzzSharp.Buffer.AddUtf16(ReadOnlySpan<char>)`.
See also [../bcl-patterns/strings-and-spans.md](../bcl-patterns/strings-and-spans.md).

## Slow → Fast

**Slow (❌):**
```csharp
if (!byte.TryParse(hexSpan.Slice(i, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r))
    ...                                   // NumberStyles/culture overhead per channel
var s = new string(c, 2);                 // allocation per channel on net4x/netstandard2.0
```

**Fast (✓):**
```csharp
public static bool TryParse(ReadOnlySpan<char> hex, out SKColor color) {
    hex = hex.Trim().TrimStart('#');
    // small case-insensitive nibble helper, straight into the uint — no alloc, no culture
    ...
}
// ABI-safe additive overload; the string version delegates, matching SKFont/SKTextBlob:
public static bool TryParse(string hex, out SKColor color) => TryParse(hex.AsSpan(), out color);
```

## Watch out (❌ don't)

Don't change accepted inputs while "cleaning up". A manual parser can silently accept or reject
strings the old one didn't — #4345 intentionally tightened `"# 12345"` and *documented* it. Prove
the new path agrees with the old across whitespace, case (`ff`/`FF`/`fF`), `null`/empty/`#`,
named-color rejection, and slices — any *unintended* difference is a behaviour change. The new
overload is **additive**; never alter the existing signature (ABI).

## Real case
#4345 — SKColor hex parse: allocation-free span parser + `ReadOnlySpan<char>` overloads (1.4–2.3×,
zero-alloc).
