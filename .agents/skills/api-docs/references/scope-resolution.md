# Workflow: Scope resolution

Both `add` and `review` start by turning a human selector into an **explicit list of `.xml` files** (plus
the candidate C# source path for each). This is deterministic and must happen before any agent runs, so
the work is auditable and shardable.

```bash
pwsh .agents/skills/api-docs/scripts/docs-tool.ps1 resolve-scope <selector> [-Confirm:$false]
```

Output: the resolved file list, a per-file candidate `binding/` source path, and a count. Fuzzy selectors
(`match:`, `group:`) print the set and **require confirmation** before proceeding (`-Confirm:$false` in
CI).

## Selector grammar

| Selector | Resolves to |
|---|---|
| `file:<path>` | exactly that file |
| `type:SKFont` | the file for that type (`**/SKFont.xml`) |
| `ns:HarfBuzzSharp` | every file in that namespace folder |
| `all` | every `*.xml` under `docs/SkiaSharpAPI/`, excluding generated (`index.xml`, `ns-*.xml`, `_filter.xml`, `FrameworksIndex/`) |
| `new` / `changed` | files added/modified vs a base — runs `git -C docs diff --name-only --diff-filter=ACM <base>...HEAD` **inside the submodule**; base defaults to `origin/main` or the `last-reviewed` marker |
| `match:<text>` | filename substring, case-insensitive (crosses libraries — this is how "font things" works) |
| `group:<name>` | curated alias from `assets/scope-aliases.yml` (e.g. `group:text`) |

## Sharding & resumability

- **443 files never go to one agent.** Split the resolved list into batches of ~25–40 files, grouped by
  namespace, and process batch by batch.
- Review is **incremental**: a `last-reviewed` marker (a tag or a small state file) records what has been
  covered so periodic runs review only `new`/`changed` since last time instead of re-scanning everything.
- Each batch is independent and resumable — a failed batch can be re-run without redoing the others.

## Source cross-reference

For each `<Type>.xml`, the candidate source is `binding/SkiaSharp/<Type>.cs` (or the matching
`binding/HarfBuzzSharp/<Type>.cs`). The resolver emits this path so agents read source first; if the file
does not exist, it emits `source:NONE` and the agent must locate it by `grep`.
