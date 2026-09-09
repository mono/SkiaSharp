# Documentation Review Criteria

Based on [official .NET API documentation guidelines](https://github.com/dotnet/dotnet-api-docs/wiki).

Classify issues by severity when reviewing documentation.

## CRITICAL (Must fix)

Issues that damage credibility or break functionality:

- **Fabricated APIs** — code examples that reference methods, overloads, or types that don't exist. Always verify against actual C# source before writing examples.
- **Obsolete APIs in examples** — using a member marked `[Obsolete("...", true)]` in a code example. These are compile errors, so the example never builds. Most common: legacy text rendering (`SKPaint.TextSize`/`Typeface`/`TextAlign`, old `SKCanvas.DrawText(string,float,float,SKPaint)`) — use `SKFont` instead. Check examples against `references/obsolete-api-map.md`; mind §2 there, where the obsolete and modern calls share a method name and differ only by signature.
- **Wrong standard values or behavior** — enum descriptions citing the wrong standard number, or mischaracterizing the standard (e.g. calling a gamma-2.6 transfer "linear"). Cross-reference against `MemberValue` **and the member name**, which usually encodes the exact standard (`SmpteRp4312` = SMPTE RP 431-2, not 432-2; `SmpteSt4281` in `SKColorspaceTransferFnCicp` = SMPTE ST 428-1, a gamma-2.6 transfer, not linear). Note the same member name can mean different things in sibling enums (`SmpteSt4281` is also a *primaries* member). Verify both the identifier and the described behavior against the member's own enum.
- **Spelling errors** in public-facing text (teh, recieve, seperate, occured, paramter, retreive, initalize)
- **Repeated words** ("the the", "a a", "an an")
- **Offensive or inappropriate content** - vulgar language, problematic terminology (master/slave, blacklist/whitelist), dismissive language (stupid, dumb, hack)
- **Malformed XML** - unescaped `<`, `>`, `&`; missing closing tags; mismatched tag names
- **Security-sensitive information** - credentials, internal URLs, PII
- **Escaped xref in CDATA** — `&lt;xref:` appearing in CDATA blocks means CDATA was destroyed during editing

## IMPORTANT (Should fix)

Issues that violate standards or leave gaps:

- **Placeholders remaining** - "To be added.", TODO, FIXME, TBD
- **Empty tags** - `<value />`, `<summary />`, `<returns />`, or `<summary></summary>` with only whitespace (note: `<remarks />` is acceptable for simple members)
- **.NET guideline violations**:
  - Summaries that just repeat the member name without context
  - Properties not starting with "Gets" or "Gets or sets" — match the verb to the accessor in the entry's `signature` field (`{ get; }` → "Gets", `{ get; set; }` → "Gets or sets")
  - Boolean properties not using "Gets a value indicating whether..."
  - Constructors not using the full "Initializes a new instance of the `<see cref>` class" (or "struct" for value types) — a shortened "Initializes a new `<see cref>` from..." is wrong
  - Methods not starting with third-person present-tense verb
  - Boolean parameters using "true if..." instead of "true to..."
  - Boolean returns using "true to..." instead of "true if..."
  - Boolean property `<value>` using "true to..." instead of "true if..." — a property value describes a state, like a return value, so it reads "true if..." (only parameters use "true to...")
  - Missing `<see langword>` for true/false/null
  - Using `<see langword="default" />` instead of `<see langword="null" />` for nullable params
- **Invalid cref references** - wrong prefix (T:, M:, P:, F:) or nonexistent target
- **DocId prefix inside a CDATA xref** — `<xref:T:...>`, `<xref:M:...>`, `<xref:P:...>` are broken links. Inside CDATA an xref takes the bare UID (`<xref:SkiaSharp.SKPath>`); the prefix is only for `<see cref>` outside CDATA.
- **Missing required documentation** - public APIs without summaries
- **Incomplete overloads** - params filled on one overload but "To be added." on another overload of the same method
- **Wrong default-value claims** — stating "the default is X" for a struct property that has no field initializer. C# structs zero-initialize, so the default is `0` / `null` / `false` unless the source explicitly sets it. A "typical" constant exposed elsewhere (e.g. `SKDocument.DefaultRasterDpi` = 72) is NOT the struct's default and must be documented separately. Verify against the C# source in `binding/`, not against a value that "looks typical". (Recurring error: `SKDocumentXpsOptions.Dpi` documented as "default 72" — it is actually 0.)
- **Examples that won't compile** — a code example is broken (so it never builds) when it:
  - references an undeclared variable/identifier (e.g. using `bitmap2` when only `bitmap` was declared) — examples must be self-contained
  - disposes (or wraps in `using`) an object owned by a parent — e.g. the canvas from `SKDocument.BeginPage(...)` is owned by the document, and `SKSurface.Canvas` is owned by the surface; calling `Dispose()`/`using` on them is wrong (see skia-patterns.md "Caller-owned vs parent-owned")

## MINOR (Nice to have)

Style improvements that enhance quality:

- Inconsistent patterns between similar APIs
- Summaries that could be more descriptive
- Missing `<paramref>` when referring to parameters
- Whitespace issues (trailing spaces, space before period)
- Missing `<remarks>` that would add value

## Report Format

When reporting issues, organize by severity and include:

```
### CRITICAL
- **File**: `docs/SkiaSharpAPI/SkiaSharp/SKPaint.xml`
- **Issue**: Spelling error "recieve"
- **Current**: `<summary>Recieve the paint data.</summary>`
- **Fix**: `<summary>Receives the paint data.</summary>`

### IMPORTANT
- **File**: `docs/SkiaSharpAPI/SkiaSharp/SKCanvas.xml`
- **Member**: `DrawRect`
- **Issue**: Empty param tag
- **Current**: `<param name="rect" />`
- **Fix**: `<param name="rect">The rectangle to draw.</param>`
```

## Summary

End reviews with:
1. Files reviewed count
2. Issues by severity (Critical: N, Important: N, Minor: N)
3. Assessment: **Ready for release** / **Needs fixes** / **Major issues**
