# Source API documentation review criteria

Classify source-comment findings using this severity bar. The facts in a C#
`///` comment must remain true when compiled to XML and consumed from the
reference assembly.

## CRITICAL — must fix

- Malformed source XML (`<`, `>`, or `&` unescaped; mismatched/unclosed tags)
  that makes compiler XML invalid or produces a compiler documentation warning.
- A fabricated type, member, overload, parameter name, exception type, or
  `cref`; an unresolved `inheritdoc`; or an example that cannot compile.
- An error-obsolete API in an example, including a legacy `SKPaint` text
  member or an `SKCanvas.DrawText` overload lacking `SKFont`.
- Incorrect ownership, disposal, threading, security, native-data-layout, or
  standards guidance. Examples include disposing `SKSurface.Canvas`, claiming
  BGRA for `SKColor`, transposing SMPTE RP 431-2 as 432-2, or calling the ST
  428-1 transfer function linear.
- Public-facing spelling errors, repeated words, offensive terminology, or
  exposed credentials, personal data, or internal URLs.

## IMPORTANT — should fix

- A new or changed consumer-visible public declaration has no accurate
  `<summary>`, or lacks needed parameter, type-parameter, return, value, or
  exception information.
- Prose contradicts source validation, nullability, default values, factory
  failure behavior, accessors, or ownership. For example, claiming a
  zero-initialized struct property defaults to a sibling “typical” constant.
- Constructor wording does not use “Initializes a new instance of the … class”
  (or “struct”); property wording says “Gets” for a writable property; Boolean
  parameter/return/value wording reverses “true to” and “true if.”
- Keywords are written as plain text or backticks instead of appropriate
  `<see langword="…" />` markup; a nullable parameter is called `default`
  rather than `null`.
- A source comment has incorrect escaping, `cref`, `paramref`, `typeparamref`,
  exception relation, or parameter/generic-parameter name.
- An example is non-self-contained, dereferences a nullable factory result,
  uses a warning-obsolete API, or disposes a parent-owned object.
- A generated binding comment was edited directly rather than changed through
  `utils/SkiaSharpGenerator` and regeneration.

## MINOR — improve when practical

- A correct summary can be clearer or more specific without changing its
  factual meaning.
- Parallel members use inconsistent but valid prose.
- A useful nonessential `<remarks>`, `<example>`, `<paramref>`, or
  cross-reference is missing.
- Whitespace, sentence punctuation, or capitalization differs from the source
  comment conventions.

## Finding format

```text
SEVERITY | class | source file:line | type.member | issue; source evidence; proposed correction
```

Report only high-confidence factual findings with a source citation. A fact
that cannot be verified is `UNVERIFIED`, not a defect. Implementation-only XML
entries do not create public-reference coverage gaps.
