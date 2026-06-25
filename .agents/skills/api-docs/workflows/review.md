# Workflow: Periodic review

Use this to audit **existing** docs for accuracy, freshness, examples, and hygiene — by scope. This is
the dual of `add`: `add` fills blanks, `review` improves what is already filled. Review is **report-only
by default**; fixing is a separate, gated step.

## Steps

1. **Resolve scope** (`workflows/scope-resolution.md`):
   ```bash
   pwsh .agents/skills/api-docs/scripts/docs-tool.ps1 resolve-scope <selector>
   ```
   Examples: `group:text` (font work), `type:SKImageFilter`, `ns:HarfBuzzSharp`, `changed`, `all`.
   Shard into ~25–40-file batches; review is incremental against the `last-reviewed` marker.

2. **Run the deterministic linter** on the batch (`workflows/validation.md` §1). It finds objective
   defects with no model cost and emits findings in the shared contract.

3. **Launch the three reviewers in parallel** (per batch), each on the resolved file list. They report
   only — they do not edit:
   - `agents/reviewer-factual.md` — claims vs source.
   - `agents/reviewer-examples.md` — examples compile, real APIs, no obsolete members.
   - `agents/reviewer-quality.md` — .NET conventions, completeness, style.

4. **Synthesize** with `agents/review-synthesizer.md` — feed it the linter output + all three reviewers'
   findings. It dedupes, reconciles severity, and emits one report plus a machine `FINDING |` block.

5. **(Gated) Fix.** If fixing is approved, the writer/fixer edits the XML directly for CRITICAL (and
   chosen IMPORTANT) findings, expanding examples where types are example-poor (port the `SKCanvas`/
   `SKShader` bar to barren types like `SKFont`, `SKImageFilter`).

6. **Validate & format** (`workflows/validation.md` §2–3) — only if step 5 made edits.

7. **Land** per-wave PRs on `dev/...` branches in the `docs` submodule.

## Per-role model assignment

The workflow runs on a **cheap orchestrator** (`engine.model`, default `claude-sonnet-4.6`) that resolves
scope, launches sub-agents, and synthesizes. The orchestrator launches each sub-agent **via the `task`
tool with an explicit `model`** — there is no config file that does this; the orchestrator reads the table
below (and each agent file's `Model:` header) and passes the right model per call.

| Sub-agent | Model | Why |
|---|---|---|
| writer | `claude-opus-4.6` | XML editing + accuracy is the hardest job |
| reviewer-factual | `claude-opus-4.6` | source-grounded correctness |
| reviewer-examples | `claude-opus-4.6` | compile/obsolete reasoning |
| reviewer-quality | `claude-sonnet-4.6` | style/completeness — lighter |
| review-synthesizer | `claude-sonnet-4.6` | dedupe/normalize — lighter |
| (mechanical checks) | — (deterministic linter) | no model needed |

These are **starting points**; tune them from the eval per-role bake-off (`eval/`). The only real
primitives are `engine.model` (orchestrator) and the `task` tool's `model` parameter (sub-agents); this
table is skill prose the orchestrator obeys. If the gh-aw CI sandbox does not honor per-sub-agent models
(verify with the routing probe in `eval/`), fall back to a single best `engine.model` for the whole run.

## Output

A review report (Markdown) + the machine `FINDING |` block from the synthesizer, written to
`output/docs-review/` (gitignored). Nothing in `docs/` changes unless the gated fix step runs.
