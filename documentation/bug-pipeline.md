# Bug Pipeline Guide

This repo has a **3-step bug pipeline** designed to minimize duplicated work:

1. **Triage** → classify + collect evidence + investigate code + propose actions
2. **Repro** → produce factual reproduction results + version matrix + minimal repro source
3. **Fix** → root-cause, minimal fix, regression test, PR

The skills are meant to be run in order (often by different people / at different times). **Data is handed off via JSON files** stored on the `docs-data-cache` branch.

| Step | Skill | Output | Primary job |
|------|-------|--------|-------------|
| 1 | `triage-issue` | `ai-triage/{n}.json` | Classification, code investigation, workaround search, suggested action |
| 2 | `bug-repro` | `ai-repro/{n}.json` | Standalone repro + version results (reporter → latest → main) |
| 3 | `bug-fix` | `ai-fix/{n}.json` + PR | Root cause, fix, regression test; consumes triage/repro artifacts |

---

## Data cache layout (handoff mechanism)

All three skills should prefer the local cache worktree when available:

```bash
[ -d ".data-cache" ] || git worktree add .data-cache docs-data-cache
git -C .data-cache pull --rebase origin docs-data-cache
CACHE=".data-cache/repos/mono-SkiaSharp"
```

Expected artifact locations:

- `"$CACHE/ai-triage/{n}.json"`
- `"$CACHE/ai-repro/{n}.json"`

---

## Role boundaries (what each skill owns)

### triage-issue (Step 1 of 3)
Owns (exclusive):
- **Classification**: `type`, `area`, platform labels, severity, actionability
- **Evidence extraction**: steps, snippets, attachments, environment info
- **Source code investigation**: `analysis.codeInvestigation` with file:line citations
- **Workaround search**: actionable options for the reporter now
- **Suggested action**: whether the issue should proceed to repro/fix

Does *not* own:
- Building/running code to confirm (that’s repro)
- Root-cause debugging/fixes (that’s bug-fix)

### bug-repro (Step 2 of 3)
Owns (exclusive):
- **Factual reproduction** (no judgment): did the reported symptoms occur?
- **Version matrix**: reporter → latest stable → `main (source)` when feasible
- **Platform dispatch**: select platform-specific playbook (`references/platform-*.md`) based on issue signals and triage data
- **Cross-platform verification**: test on alternative platform to determine scope (universal vs platform-specific)
- **Minimal repro source**: capture text source files via `reproductionSteps[].filesCreated[].content` (Program.cs, .csproj, etc.)

Does *not* own:
- Root-cause analysis or proposing fixes
- Deciding labels/closure (triage owns that)

### bug-fix (Step 3 of 3)
Owns (exclusive):
- Root-cause analysis and minimal code change
- Regression tests and verifying the fix
- PR authoring, review readiness

Does *not* own:
- Re-triaging/classifying when `ai-triage` exists (only update if evidence contradicts)
- Rebuilding a repro project from scratch when `ai-repro` exists (reuse it)

---

## Handoff contract (fields consumers should rely on)

### triage → repro
Repro consumes triage for platform selection and hints:
- `classification.platforms[]` → **informs platform file selection** (e.g., `["os/Linux"]` → Docker)
- `classification.*` (type/area/backends)
- `evidence.reproEvidence.*` (steps/snippets/attachments)
- `evidence.bugSignals.*` (severity, errorType, errorMessage, stackTrace)
- `analysis.codeInvestigation[]` (entry points / suspected code paths)
- `analysis.workarounds[]` (workarounds to validate)
- `analysis.nextQuestions[]` (what's still unknown)
- `output.actionability.*` + `output.missingInfo`

### repro → fix
Fix should treat repro JSON as the baseline execution record:
- `conclusion` + `notes`
- `versionResults[]` (especially whether `main (source)` reproduces)
- `reproProject` (packages + tfm)
- `reproductionSteps[]` including **source file content** in `filesCreated[].content`
- `environment` (os/arch/dotnetVersion/dockerUsed)
- `artifacts[]` (binary inputs and where to get them)
- `feedback.triageCorrections[]` (where repro corrected triage)

---

## Reproduction strategy alignment (NuGet vs Docker)

- **Default**: reproduce using a standalone NuGet-based console/test project.
- **Docker**: use only when the reported platform is unavailable on the host (or when the repro explicitly requires Linux/Android/etc.).
- **bug-fix should start from ai-repro**:
  - Rehydrate the repro project from `ai-repro` source files.
  - Re-run after applying the fix to verify it is resolved.
  - Only run a separate Docker-based repro if the issue is platform-only and cannot be exercised otherwise.

---

## Pipeline gates (when to run the next step)

- If triage suggests `request-info` or `close-*`: do **not** run repro/fix unless a human overrides.
- If repro concludes `not-reproduced` / `inconclusive`: bug-fix generally should **not** start; instead update triage actions (request more info / close / document).
- If repro concludes `reproduced` or `wrong-output`: proceed to bug-fix.

---

## PR integration (bug-fix)

When `ai-triage`/`ai-repro` exist, the bug-fix PR should copy in:
- A short triage summary (type/area/platform/severity + key codeInvestigation entries)
- Reproduction matrix from `versionResults[]`
- The minimal repro commands (from `reproductionSteps[].command`) and any required artifacts

---

## Feedback loop (downstream corrections)

Each step records corrections to upstream findings **in its OWN JSON** — never edit upstream files directly.

| Step | Field | Corrects |
|------|-------|----------|
| Repro | `feedback.triageCorrections[]` | Triage findings that reproduction contradicts |
| Fix | `feedback.corrections[]` | Both triage and repro findings that the fix contradicts |

Each correction has:
- `topic` — what category (classification, scope, root-cause, affected-platforms, etc.)
- `upstream` — what the upstream step said
- `corrected` — what this step actually found

**Rules:**
- Corrections are additive — never edit upstream JSON files
- Future triage/repro runs can read downstream corrections to improve
- Corrections help identify systematic triage/repro weaknesses

---

## Progress tracking

Pipeline progress per issue can be determined by checking which artifacts exist:

| Artifacts present | Pipeline state |
|-------------------|----------------|
| `ai-triage` only | Triaged, awaiting repro |
| `ai-triage` + `ai-repro` (conclusion: `reproduced`) | Reproduced, awaiting fix |
| `ai-triage` + `ai-repro` (conclusion: `not-reproduced`) | Not reproduced — may need more info |
| `ai-triage` + `ai-repro` + `ai-fix` (status: `in-progress`) | Fix in progress |
| `ai-triage` + `ai-repro` + `ai-fix` (status: `fixed`) | Fixed ✅ |
| `ai-triage` + `ai-repro` + `ai-fix` (status: `cannot-fix`) | Blocked — needs upstream or different approach |

To check pipeline status for an issue:
```bash
CACHE=".data-cache/repos/mono-SkiaSharp"
N=1234
[ -f "$CACHE/ai-triage/$N.json" ] && echo "✅ Triaged"
[ -f "$CACHE/ai-repro/$N.json" ] && echo "✅ Reproduced" && python3 -c "import json; print('  conclusion:', json.load(open('$CACHE/ai-repro/$N.json'))['conclusion'])"
[ -f "$CACHE/ai-fix/$N.json" ] && echo "✅ Fix" && python3 -c "import json; print('  status:', json.load(open('$CACHE/ai-fix/$N.json'))['status'])"
```
