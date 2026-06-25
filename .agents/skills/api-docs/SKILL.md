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

Add and review SkiaSharp API documentation. This file is a **router**: it picks a workflow and points to
the prompt, reference, and tooling files that do the work. It deliberately holds no prompt text — the
detailed instructions live one level down so they load only when needed.

## Key facts

- `docs/` is the **`mono/SkiaSharp-API-docs`** submodule — one ECMA/mdoc **`.xml` per type**, generated
  from NuGet assemblies via `mdoc`. CDATA `<remarks>` may hold `csharp` code fences. Run
  `git submodule update --init docs` if it is empty.
- Each `<Type>.xml` maps 1:1 to `binding/SkiaSharp/<Type>.cs` (or `binding/HarfBuzzSharp/`) → always read
  source before documenting.
- **Agents edit the XML directly** — there is no extract/merge JSON step. Safety comes from the
  post-edit structural validator (`workflows/validation.md`).
- **Never edit generated files:** `index.xml`, `ns-*.xml`, `_filter.xml`, `FrameworksIndex/`.

## Pick a workflow

| If the task is… | Go to |
|---|---|
| Documenting **new** APIs / filling `To be added.` placeholders | [`workflows/add.md`](workflows/add.md) |
| **Reviewing/correcting/expanding** existing docs (by API, file, namespace, theme, or all) | [`workflows/review.md`](workflows/review.md) |

Both begin by turning a human selector (`type:SKFont`, `group:text`, `ns:HarfBuzzSharp`, `changed`,
`all`, …) into an explicit, shardable file list — see [`workflows/scope-resolution.md`](workflows/scope-resolution.md).

## Orchestration model

You are an **orchestrator**, not a writer. Resolve scope, shard into ~25–40-file batches, launch
sub-agents, then synthesize — do not pre-read source or docs yourself (that duplicates the agents' work).

Sub-agents are launched via the `task` tool with an explicit `model` per role. Run the orchestrator on
the cheap default (`engine.model`, `claude-sonnet-4.6`) and spend premium models only where reasoning is
hard. The per-role model table lives in [`workflows/review.md`](workflows/review.md#per-role-model-assignment);
each agent file also states its recommended model in a one-line `Model:` header.

## Agent prompts (the HOW)

- [`agents/writer.md`](agents/writer.md) — fills placeholders, edits XML directly.
- [`agents/reviewer-factual.md`](agents/reviewer-factual.md) — claims vs source.
- [`agents/reviewer-examples.md`](agents/reviewer-examples.md) — examples compile, real APIs, no obsolete members.
- [`agents/reviewer-quality.md`](agents/reviewer-quality.md) — .NET conventions, completeness, style.
- [`agents/review-synthesizer.md`](agents/review-synthesizer.md) — dedupe/normalize findings.

All findings use one machine-parseable contract: `SEVERITY | class | file | docId | message`.

## References (the WHAT — canonical facts)

- [`references/patterns.md`](references/patterns.md) — .NET XML doc syntax, verb conventions, formatting.
- [`references/skia-patterns.md`](references/skia-patterns.md) — domain facts (color layouts, struct
  defaults, standard-based enums, caller-owned vs parent-owned).
- [`references/checklist.md`](references/checklist.md) — CRITICAL/IMPORTANT/MINOR severity taxonomy.
- [`references/obsolete-api-map.md`](references/obsolete-api-map.md) — obsolete→replacement table (linter
  + prompts read it).

> **DRY rule:** prompts describe the *role and procedure*; references hold the *facts*. Prompts point to
> references — they must not restate the tables. Keep reference chains one level deep (router → agent →
> reference).

## Tooling & validation

- Scope + checks: `scripts/docs-tool.ps1` — `resolve-scope`, `lint` (deterministic), `validate`
  (structural). See [`workflows/validation.md`](workflows/validation.md).
- Curated theme groups: [`assets/scope-aliases.yml`](assets/scope-aliases.yml).
- Snippet build (C#-only, download is fine): `dotnet cake --target=externals-download` then
  `dotnet build binding/SkiaSharp/SkiaSharp.csproj`.
- Format: `dotnet cake --target=docs-format-docs`.

## Landing changes

The `docs` submodule protects `main` — commit on a `dev/...` branch and open a PR (per-wave). Skill and
workflow asset changes land in the parent `mono/SkiaSharp` repo; the `auto-api-docs-writer` agentic
workflow lives in `mono/SkiaSharp-API-docs`.

## Eval

The skill is validated by a model-driven eval that seeds known defects into pinned fixtures and measures
detection recall/precision, plus a per-role model bake-off. See `eval/` (and the plan) — never run the
eval against the real submodule.
