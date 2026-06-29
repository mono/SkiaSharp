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
- **Edit the XML directly** — there is no extract/merge JSON step. Safety comes from the post-edit
  structural validator ([`references/validation.md`](references/validation.md)).
- **Never edit generated files:** `index.xml`, `ns-*.xml`, `_filter.xml`, `FrameworksIndex/`.

## How to work

One agent does the whole pass — there is no sub-agent fan-out and no per-role model selection. Read the
relevant reference, resolve scope into an explicit file list, then work in batches of ~25–40 files so each
pass stays auditable and resumable.

| If the task is… | Read |
|---|---|
| Documenting **new** APIs / filling `To be added.` placeholders | [`references/adding.md`](references/adding.md) |
| **Reviewing/correcting/expanding** existing docs (by API, file, namespace, theme, or all) | [`references/reviewing.md`](references/reviewing.md) |

Both begin by turning a human selector (`type:SKFont`, `group:text`, `ns:HarfBuzzSharp`, `changed`,
`all`, …) into an explicit, shardable file list — see
[`references/scope-resolution.md`](references/scope-resolution.md).

All findings use one machine-parseable contract: `SEVERITY | class | file | docId | message`.

## References (canonical facts)

- [`references/patterns.md`](references/patterns.md) — .NET XML doc syntax, verb conventions, formatting.
- [`references/skia-patterns.md`](references/skia-patterns.md) — domain facts (color layouts, struct
  defaults, standard-based enums, caller-owned vs parent-owned).
- [`references/checklist.md`](references/checklist.md) — CRITICAL/IMPORTANT/MINOR severity taxonomy.
- [`references/obsolete-api-map.md`](references/obsolete-api-map.md) — obsolete→replacement table (linter
  + procedures read it).

> **DRY rule:** the procedures describe *what to do*; the reference tables hold the *facts*. Procedures
> point to references — they must not restate the tables. Keep reference chains one level deep.

## Tooling & validation

- Scope + checks: `scripts/docs-tool.ps1` — `resolve-scope`, `lint` (deterministic), `validate`
  (structural). See [`references/validation.md`](references/validation.md).
- Curated theme groups: [`assets/scope-aliases.yml`](assets/scope-aliases.yml).
- Snippet build (C#-only, download is fine): `dotnet cake --target=externals-download` then
  `dotnet build binding/SkiaSharp/SkiaSharp.csproj`.
- Format: `dotnet cake --target=docs-format-docs`.

## Landing changes

The `docs` submodule protects `main` — commit on a `dev/...` branch and open a PR (per-wave). Skill asset
changes land in the parent `mono/SkiaSharp` repo; the `auto-api-docs-writer` agentic workflow that runs
this skill on CI lives in `mono/SkiaSharp-API-docs`.
