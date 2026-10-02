---
name: api-docs
description: >
  Add and review C# XML documentation comments for SkiaSharp public APIs. Use
  for documenting a class or member, filling missing API docs, reviewing public
  API documentation, or correcting inaccurate API examples.
metadata:
  layer: router
---

# API Documentation

Write and review the public API prose that `mono/SkiaSharp` owns. C# `///`
comments on public declarations are authoritative. Managed builds generate
compiler XML from those comments and package it beside the managed assemblies.

This repository has no API-reference ECMA/mdoc source, mdoc engine, `docs`
submodule, XML-editing workflow, or placeholder-writing workflow.
`mono/SkiaSharp-API-docs` independently consumes package/compiler XML and
`_DocsMedia`, produces ECMA/mdoc, preserves deferred Uno output, validates,
and publishes Learn. Do not edit that repository or generated output here.

## Scope and procedure

1. Resolve the consumer-visible public types and members in scope. Read their
   declarations and implementations in `binding/` or `source/` before writing
   any prose. Implementation-assembly XML does not make an API public.
2. Add or correct accurate `///` comments immediately above public
   declarations. Work from the source declaration, never from compiler XML.
3. Generated bindings are source-controlled. Edit only their `///` comment
   trivia directly, then run `pwsh -NoLogo -NoProfile -File
   ./utils/generate.ps1` and verify it preserves the intended comments. Never
   manually change generated declarations, interop code, or implementation.
4. Follow [`references/adding.md`](references/adding.md) for source-first
   authoring or [`references/reviewing.md`](references/reviewing.md) for a
   source-first review. Apply the detailed syntax and prose rules in
   [`references/patterns.md`](references/patterns.md).
5. Refresh documentation-convention knowledge from the first-party sources
   listed in [`references/patterns.md`](references/patterns.md) before a
   broad authoring or review pass. Reconcile a changed official convention
   with existing source forms and external rendering before applying it; do
   not mechanically rewrite source comments merely to match a style rule.
6. Verify SkiaSharp and HarfBuzzSharp facts in
   [`references/skia-patterns.md`](references/skia-patterns.md), and ensure
   examples avoid error-obsolete APIs with
   [`references/obsolete-api-map.md`](references/obsolete-api-map.md).
7. Classify findings with [`references/checklist.md`](references/checklist.md)
   and perform the build, compiler-XML, and package checks in
   [`references/validation.md`](references/validation.md).

## API-documentation contract

- The compiler XML generated from public `///` comments is packaged beside the
  corresponding managed assemblies in both `lib/<tfm>` and `ref/<tfm>`.
- The XML beside the reference assembly is the consumer-visible documentation
  contract. The copy beside `lib` is intentional but implementation-only XML
  entries do not expand the public API surface.
- `_DocsMedia` is versioned source in this repository and a nonshipping
  transport artifact. The external API-docs repository retrieves matching
  package and media inputs and owns downstream ECMA/mdoc generation,
  validation, and publication.

## References

| Need | Read |
|---|---|
| Adding or updating source comments | [`references/adding.md`](references/adding.md) |
| Reviewing documentation and examples | [`references/reviewing.md`](references/reviewing.md) |
| C# XML-comment syntax and conventions | [`references/patterns.md`](references/patterns.md) |
| Severity bar and reporting | [`references/checklist.md`](references/checklist.md) |
| Build, compiler-XML, and package validation | [`references/validation.md`](references/validation.md) |
| SkiaSharp/HarfBuzzSharp factual sources | [`references/skia-patterns.md`](references/skia-patterns.md) |
| Error-obsolete API replacements | [`references/obsolete-api-map.md`](references/obsolete-api-map.md) |

## Boundaries

- Do not manually edit compiler-generated XML or ECMA/mdoc files. In generated
  bindings, edit only source-controlled `///` comment trivia and verify a
  generator round trip preserves it.
- Do not remove valid public source comments to defer documentation elsewhere.
  Missing or inaccurate public documentation blocks API review.
- Do not claim defaults, validation, ownership, threading, native layout, or
  standards behavior without verifying the relevant source.
