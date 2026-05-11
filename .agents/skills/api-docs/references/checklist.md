# Documentation Review Criteria

Based on [official .NET API documentation guidelines](https://github.com/dotnet/dotnet-api-docs/wiki).

Classify issues by severity when reviewing documentation.

## CRITICAL (Must fix)

Issues that damage credibility or break functionality:

- **Fabricated APIs** — code examples that reference methods, overloads, or types that don't exist. Always verify against actual C# source before writing examples.
- **Wrong numeric values** — enum descriptions citing incorrect standard values (ITU-T H.273, Vulkan, etc.). Cross-reference against `MemberValue` in the XML.
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
  - Properties not starting with "Gets" or "Gets or sets" (check C# source for actual accessor)
  - Boolean properties not using "Gets a value indicating whether..."
  - Constructors not using "Initializes a new instance of the..."
  - Methods not starting with third-person present-tense verb
  - Boolean parameters using "true if..." instead of "true to..."
  - Boolean returns using "true to..." instead of "true if..."
  - Missing `<see langword>` for true/false/null
  - Using `<see langword="default" />` instead of `<see langword="null" />` for nullable params
- **Invalid cref references** - wrong prefix (T:, M:, P:, F:) or nonexistent target
- **Missing required documentation** - public APIs without summaries
- **Incomplete overloads** - params filled on one overload but "To be added." on another overload of the same method

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
