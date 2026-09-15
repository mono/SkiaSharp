# Reviewing source API documentation

Review C# `///` comments with their public declarations and implementations.
The target is accurate consumer-facing compiler XML, not direct editing of
compiler XML or ECMA/mdoc output. A review is report-only unless the request
also asks for corrections.

## Required reading

Read [`patterns.md`](patterns.md), [`checklist.md`](checklist.md),
[`skia-patterns.md`](skia-patterns.md), and
[`obsolete-api-map.md`](obsolete-api-map.md) before reviewing. These references
provide the syntax rules, severity bar, verified domain facts, and modern
example APIs.

## Source-first procedure

1. Resolve scope to consumer-visible public declarations in `binding/` or
   `source/`. For a theme, include related APIs based on purpose as well as
   names. For a changed-API review, inspect the changed public declarations,
   not generated XML artifacts.
2. Read the declaration and implementation *before* its comment. Record
   accessors, parameters, nullability, validation, exceptions, defaults,
   ownership, threading, and overload behavior. Cite `path:line` for every
   factual contradiction.
3. Inspect each `///` element against [`patterns.md`](patterns.md):
   `<summary>`, `<param>`, `<typeparam>`, `<returns>`, `<value>`,
   `<exception>`, `<remarks>`, `<example>`, `cref`, and `paramref`.
4. Verify examples as code: resolve every identifier, constructor, method
   overload, property, nullable result, and disposal action in current source.
   Check error-obsolete usage against both source attributes and
   [`obsolete-api-map.md`](obsolete-api-map.md).
5. Verify factual claims against source rather than intuition:
   - constraints must reflect whether code throws, accepts, pads, truncates,
     or clamps;
   - property wording must match its accessor;
   - a default requires a source initializer or other concrete source proof;
   - native layouts require the native header and standard claims require the
     member's own enum/value; and
   - SkiaSharp and HarfBuzzSharp conventions remain distinct.
6. Deduplicate findings by source location and issue. Use the highest
   applicable severity from [`checklist.md`](checklist.md).
7. If corrections are requested, change the source comment (or the generator
   input for a generated binding), regenerate where necessary, and run
   [`validation.md`](validation.md).

## Review checks

### Factual accuracy

- Do parameter and return descriptions match nullability, validation, actual
  failure behavior, and the exact overload?
- Does “default” mean the source default rather than a typical value?
- Do property summaries identify getter-only versus writable behavior?
- Are ownership and threading statements consistent with the managed wrapper
  and, where needed, native implementation?
- Do byte order, color channels, packed values, and standards citations match
  the verified sources in [`skia-patterns.md`](skia-patterns.md)?

### Examples and remarks

- Is every snippet self-contained, current, and compilable from the shown
  imports/context?
- Are nullable factory results handled before dereference where required?
- Does a `using` own its resource? `SKSurface.Canvas` and a canvas returned by
  `SKDocument.BeginPage` are parent-owned and must not be disposed by the
  example.
- Does a text example use the `SKFont`-based overload rather than an
  error-obsolete `SKPaint` form? Same-name modern overloads are valid only
  when their receiver and parameter list are correct.
- Are remarks useful and sourced, rather than unsupported comparisons or
  lifecycle claims?

### Source-comment quality

- Is the summary meaningful, grammatically correct, and punctuated?
- Are constructor, property, Boolean, `langword`, escaping, and `cref` forms
  correct?
- Do parameter names, generic parameter names, and exception types match the
  declaration exactly?
- Is an `inheritdoc` reference resolvable and appropriate to the inherited
  contract?
- Has a generated binding comment been changed through
  `utils/SkiaSharpGenerator`, rather than by manually editing generated output?

## Reporting

One machine-readable finding per line:

```text
SEVERITY | class | <source file>:<line> | <type.member> | <what the comment says; source evidence path:line; required correction>
```

Then provide a trace for every reviewed source file:

```text
TRACE | <source file> | declarations:<n> | implementation-read:<yes/no> | issues:<n>
```

Finish with files/declarations reviewed, counts by severity, and an assessment:
**Ready for release**, **Needs fixes**, or **Major issues**. Do not call
implementation-only compiler-XML entries coverage gaps; only the XML beside
the reference assembly defines the consumer-visible documentation surface.
