# Adding source API documentation

Write C# XML documentation comments with the public API change. A `///` block
immediately preceding the public declaration is the source of truth; the
managed build generates compiler XML from it. Do not write or edit an XML file
for this work.

## Required reading

1. [`patterns.md`](patterns.md) for C# XML syntax, declaration patterns, and
   examples.
2. [`skia-patterns.md`](skia-patterns.md) for verified ownership, threading,
   color, default-value, and standards facts.
3. [`obsolete-api-map.md`](obsolete-api-map.md) before placing an API in an
   example.

## Source-first procedure

1. Identify every new or changed consumer-visible type, constructor, member,
   generic parameter, and parameter under `binding/` or `source/`. A member
   emitted only by an implementation assembly is not a new public-reference
   documentation obligation.
2. Read the declaration *and implementation* before authoring. Record:
   - accessor shape (`get`, `set`, `init`);
   - nullability, validation, exceptions, and factory failure behavior;
   - actual default values and numeric constants;
   - ownership and threading rules; and
   - native layout or external-standard facts, after checking the relevant
     native header where applicable.
3. Put an accurate `///` block directly above each public declaration. Add
   `<summary>` for the API, then only the useful supporting elements:
   `<param>`, `<typeparam>`, `<returns>`, `<value>`, `<exception>`,
   `<remarks>`, and `<example>`. Match names and behavior exactly.
4. Treat the source as authoritative over the name. In particular, do not
   infer that a parameter rejects an input when the code pads, truncates,
   clamps, or otherwise accepts it; do not infer a struct default from a
   sibling constant.
5. For a generated binding declaration, edit only the source-controlled `///`
   comment trivia in that generated file, then regenerate:

   ```bash
   pwsh -NoLogo -NoProfile -File ./utils/generate.ps1
   ```

   Confirm the regenerated file preserves the intended comment. Do not manually
   change generated declarations, interop code, or implementation.
6. Add remarks or an example when they make a non-obvious API usable. Verify
   every type, overload, identifier, null path, and disposal action against
   current source. Examples must be self-contained and use no error-obsolete
   API.
7. Run the concrete checks in [`validation.md`](validation.md).

## Factual requirements

- **Accessor wording:** inspect the declaration. A getter-only property starts
  “Gets”; a writable property starts “Gets or sets.” An `init` accessor is
  writable during initialization, so describe it precisely rather than calling
  it read-only.
- **Defaults:** state a default only when source establishes it. A
  zero-initialized struct member defaults to `0`, `false`, or `null` absent an
  initializer; a useful constant elsewhere is not its default.
- **Exceptions and failure:** document an exception only when the public API
  actually throws it. For factories, distinguish a `null`/invalid result from
  an exception and follow the declared nullability.
- **Standards and layout:** verify both the standard identifier and behavior,
  and verify channel/byte/bit order in the native definition. Do not transfer
  a HarfBuzzSharp convention to SkiaSharp or vice versa.
- **Lifetime:** state caller disposal, parent ownership, or threading limits
  only when source establishes the rule. In an example, never dispose a canvas
  owned by `SKDocument` or `SKSurface`.

## Completion record

For a focused authoring pass, report each changed declaration rather than a
generated-document file:

```text
WROTE | <source file>:<line> | <type/member> | summary:<yes/no> params:<n> returns:<yes/no> remarks:<yes/no>
```

If a fact cannot be verified, omit the unsupported claim and report the
uncertainty; do not add a placeholder or fabricate documentation. The external
API-docs repository owns any later ECMA/mdoc representation.
