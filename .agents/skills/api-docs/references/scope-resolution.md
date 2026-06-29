# Choosing which docs to work on

The user speaks in natural language — "review the font docs", "fill in everything that's missing". There is
**no selector grammar to craft**: the model decides which files a request covers. The script only provides a
deterministic *inventory* so the choice is auditable and shardable:

```bash
pwsh .agents/skills/api-docs/scripts/docs-tool.ps1 resolve-scope <all|new|changed|file:PATH>
```

Output per file: the doc path, a candidate `binding/` source path, and a count.

## The inventory modes

| Mode | Lists |
|---|---|
| `all` | every `*.xml` under `docs/SkiaSharpAPI/`, excluding generated (`index.xml`, `ns-*.xml`, `_filter.xml`, `FrameworksIndex/`) |
| `new` | docs that still contain `To be added.` placeholders — the "do all adds" set |
| `changed` | docs added/modified vs a base — `git -C docs diff --name-only --diff-filter=ACM <base>...HEAD` inside the submodule; base defaults to `origin/main` or the `last-reviewed` marker |
| `file:<path>` | exactly that one file (plumbing for lint/validate and for naming a specific pick) |

## Mapping a request to files

1. **"Add the missing docs" / "do all adds"** → `resolve-scope new`, then fill every placeholder.
2. **A theme ("review the font docs", "the image-filter types")** → `resolve-scope all`, then **select the
   files that fit the theme yourself** and print the chosen set for the record before proceeding. A capable
   model expands "font" to `SKFont`, `SKTypeface`, `SKFontMetrics`, `SKTextBlob`, the text APIs on
   `SKPaint`/`SKCanvas`, … far more accurately than any filename substring or hand-curated list (which
   drifts as new types ship).
3. **"Whatever changed" / CI incremental** → `resolve-scope changed`.
4. **A specific type the user named** → open that file directly; pass `file:<path>` to lint/validate it.

## Sharding & resumability

- **400+ files never go to one agent.** Split the chosen list into batches of ~25–40 files, grouped by
  namespace, and process batch by batch.
- Review is **incremental**: a `last-reviewed` marker (a tag or a small state file) records what has been
  covered so periodic runs review only `changed` since last time instead of re-scanning everything.
- Each batch is independent and resumable — a failed batch can be re-run without redoing the others.

## Source cross-reference

For each `<Type>.xml`, the candidate source is `binding/SkiaSharp/<Type>.cs` (or the matching
`binding/HarfBuzzSharp/<Type>.cs`). The resolver emits this path so you read source first; if the file
does not exist, it emits `source:NONE` and you must locate it by `grep`.
