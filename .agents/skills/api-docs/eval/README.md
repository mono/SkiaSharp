# api-docs eval harness

This harness proves two things and keeps proving them as the skill evolves:

1. **The deterministic linter keeps working.** Seeded mechanical defects (spelling,
   obsolete-in-example, bad cref/xref, empty tags, …) must all be caught, and clean
   control files must stay silent. This is a fast, model-free CI gate.
2. **Which model is best per reviewer role.** The judgment-only defects (wrong default
   value, ARGB↔RGBA, fabricated members) are scored across candidate models to pick the
   cheapest model that still hits the recall/precision bar for each role.

Everything runs against **copies** of the fixtures written to the gitignored `output/eval/`
tree. The injector refuses to write anywhere under `docs/`, so the real submodule is never
touched.

## Layout

| Path | Role |
|---|---|
| `fixtures/Sample/*.xml` | Clean, self-contained mdoc files. They lint with **zero** findings, so any finding on a control copy is unambiguously a false positive. |
| `mutations.json` | Ground truth: every seeded defect as a `find`→`replace` plus its expected `(file, docId, class, detector)`. |
| `inject.ps1` | Copies fixtures into `output/eval/{corrupted,controls}/`, applies mutations, writes `answer-key.json`. Validates each corrupted file is still well-formed. |
| `score.ps1` | Matches a findings file (contract lines) against the answer key → recall, precision (via controls), per-class/per-detector breakdown, JSON + Markdown scorecard. |
| `run.ps1` | One command: inject → lint corrupted+controls → score the `lint` subset, gated at recall 1.0 / precision 1.0. |

## Findings contract

Both the linter and the LLM reviewers emit one finding per line:

```
SEVERITY | class | file | docId | message
```

`score.ps1` is detector-agnostic: it reads any file of these lines. Lines whose path
contains `/controls/` are treated as control findings (any one is a false positive); lines
on `/corrupted/` files are matched to the answer key by `(file basename, docId, class)`.

## 1. Deterministic self-test (CI gate)

```bash
pwsh .agents/skills/api-docs/eval/run.ps1
```

Expected: `SCORE | detector:lint | recall:9/9=1 | precision:1 | controlFP:0 | extra:0`.
A regression in `docs-tool.ps1` — a check that stops firing, or a new false positive on a
control — drops recall or precision below 1.0 and fails the run. Wire this into CI alongside
`skills-ref validate`.

### Adding a defect class

1. Add a clean baseline sentence/example to a fixture so it still lints clean (`run.ps1`
   must stay green before you mutate).
2. Add a mutation to `mutations.json` whose `find` occurs exactly once, with the expected
   `class` and `detector`.
3. Re-run `run.ps1`. For a `lint` defect, recall must return to 1.0; for an `llm` defect,
   it shows up under the `llm` detector and is exercised by the bake-off below.

## 2. Model bake-off (per role)

The `llm`-detector defects (`factual`, `fabricated-member`) can't be caught mechanically —
they need a reviewer agent reading the source. Use the bake-off to choose each role's model.

For each **role** (`reviewer-factual`, `reviewer-examples`, `writer` self-check) and each
**candidate model** (`claude-opus-4.6`, `claude-opus-4.5`, `claude-sonnet-4.6`, `gpt-5`):

1. `pwsh eval/inject.ps1` to (re)produce the corrupted tree + answer key.
2. Launch the role's sub-agent with that model via the `task` tool, pointing it at
   `output/eval/corrupted/` and instructing it to emit the findings contract to a file, e.g.
   `output/eval/llm-<role>-<model>.txt`. Use the corresponding `agents/<role>.md` prompt.
3. Score it: `pwsh eval/score.ps1 -Findings output/eval/llm-<role>-<model>.txt -Detector llm`.
4. Repeat **k=3** (LLMs are nondeterministic); record mean and worst-case recall/precision.

Assemble a per-role × per-model table and set:
- the one-line `Model:` header in each `agents/<role>.md`, and
- the model table in `workflows/review.md`,

to the cheapest model whose **worst-case** recall ≥ 0.9 and precision ≥ 0.8. The factual and
example reviewers usually need Opus; the quality reviewer and synthesizer are typically fine
on Sonnet.

### Routing probe (confirm per-sub-agent models in CI)

Per-sub-agent model routing works locally through the `task` tool, but the gh-aw CI sandbox
must honor it too. Before trusting the bake-off numbers in CI, launch a trivial sub-agent
with `model: claude-opus-4.6` whose only job is to report the model it actually ran on. If CI
does **not** honor per-sub-agent models, fall back to a single best `engine.model` for the
whole run and treat the bake-off as guidance for that single choice.

## Safety

- `inject.ps1` writes only under `output/eval/` and aborts if the output root is under
  `docs/`.
- `output/` is gitignored — corrupted copies, findings, and scorecards never get committed.
- Fixtures are synthetic (`Sample.*`), so nothing here depends on the real docs staying
  byte-stable.
