---
name: review-skia-update
description: >-
  Review a Skia upstream merge PR in mono/skia. Produces a security-auditable
  report by diffing against the upstream branch, verifying generated P/Invoke
  bindings, checking source integrity, and auditing DEPS changes.
  Triggers: "review skia update PR #NNN", "review skia PR", "review skia bump",
  "check skia update integrity".
---

# Review Skia Update

Analyze a Skia upstream merge PR in `mono/skia` and produce a structured, schema-validated
review report. Reduces 100K–500K line diffs to focused, human-reviewable artifacts.

## ⛔ MANDATORY FIRST STEPS (do not skip)

1. Read THIS entire SKILL.md before any investigation
2. Read [references/schema-cheatsheet.md](references/schema-cheatsheet.md) for required JSON fields

## Overview

A Skia update always involves two PRs that must be reviewed together:
- **mono/skia PR** — the Skia submodule bump (C headers, DEPS, upstream merge)
- **SkiaSharp PR** — the companion C# changes (generated bindings, hand-written wrappers)

```
Phase 1: Run orchestrator  →  Phase 2: Write summaries & build report
Phase 3: Review C# PR      →  Phase 4: Validate & persist
```

---

## Phase 1 — Run the Orchestrator

A single script handles all mechanical work: fetching PR metadata, checking out both PRs,
running the generator, checking source integrity, auditing DEPS, and analyzing companion PR files.

```bash
python3 .claude/skills/review-skia-update/scripts/run_review.py \
    --skia-pr {skia_pr_number} \
    --skiasharp-pr {skiasharp_pr_number}
```

Both parameters are required. A Skia update always has a companion SkiaSharp PR.

**Output:** `raw-results.json` in the output directory with all check results, including
mechanically generated file lists and diffs for upstream integrity, interop integrity, DEPS,
and companion PR files.

After this runs, the working tree is checked out to the PR state — you can browse files locally.

> **⚠️ If the orchestrator fails**, report the failure and stop. Do not attempt to
> run individual scripts manually.

> **⚠️ NO-RETRY POLICY:** Run the orchestrator exactly ONCE. If generation reports FAIL,
> that is the result. Do NOT re-run to get a different outcome.

---

## Phase 2 — Write Summaries & Build Report

Read [references/writing-summaries.md](references/writing-summaries.md) for detailed guidance.

1. Load `raw-results.json` from the output directory printed by the orchestrator
2. For every item in added/removed/changed across ALL sections (upstream, interop, companion PR):
   read the diff and write a factual summary
3. **Verify removed patches** — For each removed upstream patch, check the new upstream
   code to determine WHY it was dropped. See writing-summaries.md "Verifying Removed Patches"
   for the required process. Never speculate about patch removal reasons.
4. Assemble the final JSON report conforming to `skia-review-schema.json`
5. Include actual diff content in the JSON (not file path references)
6. **Save the report to `{output_dir}/{pr_number}.json`** — the same directory as `raw-results.json`

---

## Phase 3 — Review Companion C# PR

Read [references/csharp-review.md](references/csharp-review.md) for detailed guidance.

The orchestrator already produced the `companionPr` section with file lists and diffs in
`raw-results.json`. This phase adds human-oriented review context:

1. Ignore all `*Api.generated.cs` files (already filtered by the orchestrator)
2. For each companion PR file, review the diff for: null handling, disposal patterns, ABI compatibility
3. Check test coverage for new/changed APIs
4. Add `relatedFiles` cross-links to interop files where applicable

The working tree is checked out to the companion PR, so you can read files directly.

---

## Phase 4 — Validate & Persist

### 1. Validate

> **🛑 PHASE GATE: You CANNOT proceed to persist without passing validation.**
> **Skipping validation = INVALID review. The task is incomplete.**

```bash
# Try pwsh first, fall back to python3
pwsh .claude/skills/review-skia-update/scripts/validate-skia-review.ps1 {output_dir}/{pr_number}.json \
  || python3 .claude/skills/review-skia-update/scripts/validate-skia-review.py {output_dir}/{pr_number}.json
```

- **Exit 0** = ✅ valid → proceed to persist
- **Exit 1** = ❌ fix the errors listed in the output, then re-run. Repeat up to 3 times.
- **Exit 2** = fatal error, stop and report

> **⚠️ NEVER hand-roll your own validation. NEVER assume it passes. RUN THE SCRIPT.**

### 2. Persist

> **🛑 PHASE GATE: The validator MUST have printed ✅ before you reach this step.**
> **If you have not run the validation script, GO BACK and run it now.**

Copy the validated JSON to `output/ai/` for collection.

```bash
pwsh .claude/skills/review-skia-update/scripts/persist-skia-review.ps1 {output_dir}/{pr_number}.json
```

This copies the JSON to `output/ai/repos/mono-skia/ai-review/` and generates an HTML report
alongside it. The HTML is a self-contained file (Bootstrap 5 + diff2html) suitable for attaching
to a PR/issue or uploading as a gist.

To push to the data-cache branch separately: `pwsh scripts/persist-to-cache.ps1`

### 3. Present summary

```
✅ Review: ai-review/{pr_number}.json

Generated Files:    PASS/FAIL
Upstream Integrity: PASS/REVIEW_REQUIRED (Na/Nr/Nc)
Interop Integrity:  PASS/REVIEW_REQUIRED (Na/Nc)
DEPS Audit:         PASS/REVIEW_REQUIRED (Na/Nc)
Companion PR:       PASS/REVIEW_REQUIRED (Na/Nc)
Risk:               HIGH/MEDIUM/LOW
```

After presenting the summary, ask the user if they'd like to open the HTML report in their browser
to review the full contents (diffs, recommendations, dependency table, etc.). If yes:

```bash
open output/ai/repos/mono-skia/ai-review/{pr_number}.html  # macOS
# or: xdg-open ... (Linux) / start ... (Windows)
```

---

## Rules

1. **Run orchestrator first** — Do not run individual check scripts manually
2. **No retries** — Run once, report what happened
3. **Never trust generated files** — The orchestrator regenerates independently
4. **No per-file PASS/FAIL** — AI provides factual summaries; all items need human review
5. **Include actual diffs in JSON** — Dashboard renders them directly
6. **Validate before persist** — Must see `✅ valid`
7. **No absolute paths in report** — Redact to relative paths
