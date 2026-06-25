# api-docs model bake-off — results

This records the empirical answer to "which model is best per reviewer role." Re-run and update
this file whenever the candidate set, the role prompts, or the fixtures change. Procedure lives in
`README.md` (§2); the harness is reproducible end-to-end from `inject.ps1` + `score.ps1`.

## Setup

- **Date:** 2026-06-25
- **Role tested:** `reviewer-factual` (it owns the judgment-only defects — `factual`, `fabricated-member`).
- **Candidates:** `gpt-5.5`, `claude-opus-4.6`, `claude-opus-4.5`, `claude-sonnet-4.6`.
- **Repetitions:** k=3 per model (LLMs are nondeterministic).
- **Launcher:** each run is a `task` sub-agent launched with the candidate `model`, given the
  `agents/reviewer-factual.md` role, pointed at a **blind** review tree (neutral directory names hide
  which copies are corrupted vs pristine; the corrupted/control mapping is rewritten back only at
  scoring time, so neither recall nor precision can be gamed from path names).
- **Scored with:** `score.ps1 -Detector llm` against the seeded answer key
  (`m10` wrong default, `m11` ARGB→RGBA byte order, `m13` fabricated `SetAlpha`).

## Routing probe

The `task` tool honored the per-sub-agent `model` parameter locally for all four candidates
(harness-reported model matched the request in every case). Models cannot reliably self-report their
id, so the harness metadata is the authoritative signal. **CI caveat:** the gh-aw sandbox must still be
confirmed to honor per-sub-agent models; if it does not, fall back to a single best `engine.model`.

## Results (k=3)

| model | mean recall | worst recall | mean precision | worst precision | caught byte-order (m11) |
|---|---|---|---|---|---|
| **gpt-5.5** | **0.889** | 0.667 | **0.750** | 0.250 | **yes — 3/3 substantive** |
| claude-opus-4.5 | 0.667 | 0.667 | 0.411 | 0.333 | no |
| claude-sonnet-4.6 | 0.667 | 0.667 | 0.340 | 0.286 | no |
| claude-opus-4.6 | 0.667 | 0.667 | 0.317 | 0.286 | no¹ |

¹ The scorer matches on `(file, docId, class)`, so one opus-4.6 run got *false credit* for `m11` via an
unrelated `factual` finding on `T:Sample.SampleColor`. A raw-text check confirmed **no Claude run ever
mentioned byte/channel order** — only `gpt-5.5` substantively caught it.

## Decision

**`reviewer-factual` → `gpt-5.5`.** It is the only candidate that catches the subtle internal
contradiction the role exists for (stated "RGBA" vs example bytes `0xFF,0x80,0x40,0x20` read as
A,R,G,B), and it leads on both mean recall and mean precision. Applied in
`agents/reviewer-factual.md` (`Model:` header) and `workflows/review.md`.

## What the bake-off taught the skill

1. **False-positive pattern (fixed).** The Claude candidates lost precision by extending the
   "a field with no initializer defaults to 0" rule to *documented* defaults they could not verify in
   source (flagging `Alpha = 255` / `Width = 1` as "should be 0"). `agents/reviewer-factual.md` now
   says: only assert a default is wrong when you have **read the source**; otherwise mark it
   `UNVERIFIED`, never assume `0`.
2. **High variance even for the winner.** `gpt-5.5`'s worst run (1 of 3) was the noisiest of all 12
   (precision 0.25). No candidate met the strict worst-case gate (recall ≥ 0.9 **and** precision ≥ 0.8)
   on this micro-eval — treat the numbers as **directional**, not a contract.

## Harness limitations to address next (fixture hardening)

- **Fixtures are not perfectly "clean" to an LLM judge.** They carry latent ambiguities — a `struct`
  default documented as `255`, an example that passes `SKFont` where `SampleFont` is required, a
  "pixels" vs "device-independent units" mismatch — that a strong reviewer legitimately flags. Because
  they appear on both the corrupted and control copies, the control flag is mechanically scored as a
  false positive, depressing every model's precision. Either clean these so controls are truly
  defect-free, or promote them to seeded defects with answer-key entries.
- **Scoring is `(file, docId, class)` only** — no message-similarity check — so a correct-slot but
  wrong-reason finding can earn false credit (see ¹). Add fuzzy message matching before trusting
  precision/recall to two decimal places.
- **Scope:** only `reviewer-factual` was baked off. `writer` and `reviewer-examples` (which would also
  exercise the `SKFont`/`SampleFont` and obsolete-member reasoning) still need their own runs before
  their model assignments are anything more than reasoned defaults.
