# Reviewing source API documentation

Review C# `///` comments together with the public declarations they document.
The goal is accurate consumer-facing compiler XML, not direct editing of
generated XML or external ECMA/mdoc output.

## Procedure

1. Resolve the changed public API surface from `binding/` and `source/`; read
   each declaration and implementation before reviewing its comment.
2. Check factual claims, parameter constraints, failure behavior, ownership,
   and threading against source. Use [`skia-patterns.md`](skia-patterns.md) for
   verified domain facts.
3. Check summaries, parameters, returns, values, exceptions, `cref` targets,
   and examples against [`patterns.md`](patterns.md).
4. Check examples for real signatures, declared identifiers, disposal safety,
   and obsolete API use with [`obsolete-api-map.md`](obsolete-api-map.md).
5. Report findings as:

   ```
   SEVERITY | class | source file | member | message with source citation
   ```

6. Correct high-confidence defects in the source comment, then run the build
   and package validation in [`validation.md`](validation.md).

## Review rules

- A public API without accurate documentation is an IMPORTANT finding.
- A fabricated API, broken example, unsafe ownership guidance, malformed XML
  comment, or invalid `cref` is CRITICAL.
- Do not remove a valid comment to make a review pass; fix it at its source.
- Generated binding comments are reviewed as source-controlled generated
  output. Correct their inputs and regenerate rather than editing output.
- Compiler XML beside `ref/<tfm>` defines the consumer-visible documentation
  surface. Implementation-only entries under `lib/<tfm>` do not expand the
  public API scope.
