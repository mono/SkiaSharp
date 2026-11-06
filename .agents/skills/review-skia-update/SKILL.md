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

A Skia update involves two PRs that must be reviewed together:
- **mono/skia PR** — the Skia submodule bump (C headers, DEPS, upstream merge)
- **SkiaSharp PR** — the companion C# changes (generated bindings, hand-written wrappers)

```
Phase 1: Run orchestrator  →  Phase 2: Write summaries & build report
Phase 3: Review C# PR      →  Phase 4: Validate & persist
```

gh-aw installs this skill and its bundled scripts before the agent starts.
Its `$SKIA_REVIEW_SKILL_DIR` points to that trusted installed copy; use it for
all scripts and references in prepared-results mode, without checking out PR code.
The commands below default to the repository copy for interactive reviews.

---

## Phase 1 — Run the Orchestrator

A single script handles metadata, both PRs, regeneration, source integrity, DEPS, and
companion files. Start with the parent PR; its **Required skia PR** section must link
exactly one canonical `https://github.com/mono/skia/pull/N` URL:

```bash
python3 "${SKIA_REVIEW_SKILL_DIR:-.agents/skills/review-skia-update}/scripts/run_review.py" \
    --skiasharp-pr {skiasharp_pr_number} --isolated
```

`--isolated` is recommended for automation: only public repository code enters the
tokenless Docker worker; the host checkout stays unchanged. Without it, the script
retains the historical behavior of checking out the companion and native PRs locally.
The legacy `--skia-pr N --skiasharp-pr N --milestone N` invocation still works.
If supplied, `--milestone` must match the companion head `cgmanifest.json`; otherwise
the script infers it there, never from an AI guess or the PR title.

**Output:** `raw-results.json` in the output directory with all check results, including
mechanically generated file lists and diffs for upstream integrity, interop integrity, DEPS,
and companion PR files.

Without `--isolated`, the working tree is checked out to the PR state. With it,
read the included diffs and fetch public companion/upstream sources read-only at
the recorded exact SHAs when verifying removed patches; do not inspect the host
checkout as though it were the PR.

**Prepared results (gh-aw):** If `$SKIA_REVIEW_RAW_RESULTS` is set, the trusted
pre-agent step already ran Phase 1 **once**. Read that file directly, do not
rerun the orchestrator, then complete Phases 2–4 below. A missing file is an
incomplete review: stop instead of rerunning.

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
6. **Save the report to `{output_dir}/{pr_number}.json`** — use the native PR number,
   and in prepared-results mode use the raw results directory.
   Carry `meta.shas` and `companionPr.headSha`/`baseSha` from the raw results
   unchanged so the reviewed native and parent head/base commits remain visible.

---

## Phase 3 — Review Companion C# PR

Read [references/csharp-review.md](references/csharp-review.md) for detailed guidance.

The orchestrator already produced the `companionPr` section with file lists and diffs in
`raw-results.json`. This phase adds human-oriented review context:

1. Ignore generator-owned declaration and interop changes in `*.generated.cs`,
   but review direct `///` documentation-comment changes reported by the orchestrator
2. For each companion PR file, review the diff for: null handling, disposal patterns, ABI compatibility
3. Check test coverage for new/changed APIs
4. Add `relatedFiles` cross-links to interop files where applicable

For non-isolated runs the working tree is at the companion PR. In isolated/prepared
mode, use raw diffs and read-only GitHub access to exact SHAs for needed context.

---

## Phase 4 — Validate & Persist

### 1. Validate

> **🛑 PHASE GATE: You CANNOT proceed to persist without passing validation.**
> **Skipping validation = INVALID review. The task is incomplete.**

```bash
python3 "${SKIA_REVIEW_SKILL_DIR:-.agents/skills/review-skia-update}/scripts/validate-skia-review.py" \
    {output_dir}/{pr_number}.json
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
python3 "${SKIA_REVIEW_SKILL_DIR:-.agents/skills/review-skia-update}/scripts/persist-skia-review.py" \
    {output_dir}/{pr_number}.json
```

This copies the JSON to `output/ai/repos/mono-skia/ai-review/` and generates an HTML report
alongside it. The HTML is a self-contained file (Bootstrap 5 + diff2html) suitable for attaching
to a PR/issue or uploading as a gist.
In headless gh-aw, add `--output-dir "$SKIA_REVIEW_ARTIFACT_DIR"` to the persist
command so the normal agent artifact contains both files. Do not prompt to open
a browser in automation.

To push to the data-cache branch separately, use the `persist-aw-data` GitHub Actions workflow.

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

For interactive reviews, ask the user if they'd like to open the HTML report in their browser
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
8. **Advisory only** — JSON/HTML findings inform a human reviewer, not approval or
   a machine-verifiable landing contract; never merge, approve or repin from this review
