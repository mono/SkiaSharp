---
name: api-docs
description: >
  Write AND review XML API documentation for SkiaSharp (ECMA/mdoc XML in the docs submodule). Two modes:
  (1) ADD docs for new APIs with "To be added." placeholders; (2) REVIEW existing docs by scope for
  accuracy, freshness, examples, and hygiene.
  Triggers: "document class", "add XML docs", "write XML documentation", "fill in missing docs",
  "remove To be added placeholders", "review documentation", "check docs for errors", "fix doc issues",
  "audit the docs", "review the font docs", "are the examples correct", "update out-of-date docs",
  any request to add, validate, correct, or expand SkiaSharp API documentation.
metadata:
  layer: router
---

# API Documentation

Add and review SkiaSharp API documentation. This file is a **router**: it picks a procedure and points to
the reference and tooling files that do the work. The detailed instructions live in `references/` so they
load only when needed.

## Key facts

- `docs/` is the **`mono/SkiaSharp-API-docs`** submodule — one ECMA/mdoc **`.xml` per type**, generated
  from NuGet assemblies via `mdoc`. CDATA `<remarks>` may hold `csharp` code fences. Run
  `git submodule update --init docs` if it is empty.
- Each `<Type>.xml` maps 1:1 to `binding/SkiaSharp/<Type>.cs` (or `binding/HarfBuzzSharp/`) → always read
  source before documenting.
- **Edit the XML directly.** Safety comes from `docs-format-docs`, which formats every file and fails the
  build on broken XML/CDATA ([`references/validation.md`](references/validation.md)).
- **Never edit generated files:** `index.xml`, `ns-*.xml`, `_filter.xml`, `FrameworksIndex/`.

## How to work

One agent does the whole pass. Read the relevant reference, resolve scope into an explicit file list, then
work in batches of ~25–40 files so each pass stays auditable and resumable.

| If the task is… | Read |
|---|---|
| Documenting **new** APIs / filling `To be added.` placeholders | [`references/adding.md`](references/adding.md) |
| **Reviewing/correcting/expanding** existing docs (one type, a theme, what changed, or all) | [`references/reviewing.md`](references/reviewing.md) |

The user asks in plain language ("review the font docs", "fill in what's missing"). The docs live at
`docs/SkiaSharpAPI/<Namespace>/<Type>.xml`; list them directly, and use
`git -C docs diff --name-only origin/main...HEAD` for "what changed". Each `<Type>.xml` maps to its source
at `binding/<Namespace>/<Type>.cs`, and **you** pick the files a request covers — for a theme, scan the
list and select the matching types yourself; the chosen procedure file covers the rest.

All findings use one machine-parseable contract: `SEVERITY | class | file | docId | message`.

## References (canonical facts)

- [`references/patterns.md`](references/patterns.md) — .NET XML doc syntax, verb conventions, formatting.
- [`references/skia-patterns.md`](references/skia-patterns.md) — domain facts (color layouts, struct
  defaults, standard-based enums, caller-owned vs parent-owned).
- [`references/checklist.md`](references/checklist.md) — CRITICAL/IMPORTANT/MINOR severity taxonomy.
- [`references/obsolete-api-map.md`](references/obsolete-api-map.md) — obsolete members and their modern
  replacements; the writer and example reviewer read it (not the linter — obsolete use is a model
  judgement, see the reference for why).

> **DRY rule:** the procedures describe *what to do*; the reference tables hold the *facts*. Procedures
> point to references — they must not restate the tables. Keep reference chains one level deep.

## Tooling & validation

- Format + checks (one Cake target in `scripts/infra/docs/docs.cake`): `docs-format-docs` formats every
  type file and runs the deterministic content checks — warnings for missing/quality issues, build-failing
  errors for broken XML/CDATA. See [`references/validation.md`](references/validation.md).
- Snippet build (C#-only, download is fine): `dotnet cake --target=externals-download` then
  `dotnet build binding/SkiaSharp/SkiaSharp.csproj`.

## Landing changes

The `docs` submodule protects `main` — commit on a `dev/...` branch and open a PR (per-wave). Skill asset
changes land in the parent `mono/SkiaSharp` repo; the `auto-api-docs-writer` agentic workflow that runs
this skill on CI lives in `mono/SkiaSharp-API-docs`.
