# Adding source API documentation

Write C# XML documentation comments with the public API change. The `///`
comment immediately above the public declaration is the source of truth; a
managed build produces compiler XML from it.

## Procedure

1. Identify all new or changed public types, constructors, members, and
   parameters in `binding/` or `source/`.
2. Read the implementation and related native API before making a factual
   claim. Use [`skia-patterns.md`](skia-patterns.md) for verified ownership,
   threading, color, and standard facts.
3. Add concise `///` documentation using the patterns in
   [`patterns.md`](patterns.md). Document validation, null/failure behavior,
   ownership, or threading only when the source establishes it.
4. For generated binding declarations, change the source-controlled generator
   input/comment and regenerate with `pwsh -NoLogo -NoProfile -File
   ./utils/generate.ps1`. Do not hand-edit generated output.
5. Include a short, self-contained example only where it improves use of a
   type or important operation. Verify every API call and dispose only
   caller-owned objects. See [`obsolete-api-map.md`](obsolete-api-map.md).
6. Run the validation in [`validation.md`](validation.md).

## Scope

Document public APIs that consumers can reach through reference assemblies.
Implementation-only compiler XML entries are not API-reference prose and do
not create a documentation obligation. Never use placeholders: an uncertain
claim must be researched or omitted until it can be stated accurately.

## External generation boundary

`mono/SkiaSharp-API-docs` independently converts package/compiler-XML/media
inputs to ECMA/mdoc and publishes Microsoft Learn. It owns generated ECMA
output, including its currently deferred Uno output. This repository supplies
source comments and package inputs only.
