# Workflow: Add docs for new APIs

Use this when new APIs have shipped and their doc files contain `To be added.` placeholders (typically
triggered by the daily `auto-api-docs-writer` workflow after a NuGet/CI update).

This is the simplified, **direct-XML** pipeline — no extract/merge JSON round-trip.

## Steps

1. **Regenerate stubs (only if new APIs were added).** Skip if you are editing existing docs or the
   automated workflow already ran this as a pre-step.
   ```bash
   dotnet tool restore
   dotnet cake --target=docs-download-output   # latest NuGets from the CI feed
   dotnet cake --target=update-docs            # mdoc update + format → "To be added." placeholders
   ```

2. **Resolve scope.** New members are easiest to target with the `new` selector (see
   `workflows/scope-resolution.md`):
   ```bash
   pwsh .agents/skills/api-docs/scripts/docs-tool.ps1 resolve-scope new
   ```
   Shard the result into ~25–40-file batches.

3. **Write (per batch).** As orchestrator, launch the **writer** sub-agent on each batch, passing the
   resolved file list and source paths. Use the model named in the header of `agents/writer.md` (see the
   model table in `workflows/review.md`). The writer reads source, fills placeholders, and edits the XML
   in place. Do not pre-read source yourself — that duplicates the writer's discovery.

4. **Review.** Run the review pass (`workflows/review.md`) restricted to the files just written: the
   deterministic linter + the three reviewers + the synthesizer.

5. **Fix CRITICAL findings** by editing the XML directly.

6. **Validate & format** (`workflows/validation.md`): structural validator → linter → `docs-format-docs`.

7. **Land:** commit on a `dev/...` branch in the `docs` submodule and open a PR (the submodule protects
   `main`).

## Notes

- The writer only fills in-scope (empty/placeholder) fields; it does not rewrite existing prose.
- If a large type runs out of certainty, its placeholder is left intact (`DEFERRED` line) so the next run
  re-detects it — the file stays clean and well-formed either way.
