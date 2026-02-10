---
name: triage-issue
description: Triage a SkiaSharp GitHub issue into structured JSON (type, area, platform, severity, actionability). Use when the user says "triage #123", "triage issue 123", "classify issue", or "analyze issue". Produces a schema-validated JSON file with classification, evidence, analysis (signals + rationales + resolution proposals), and automatable actions (label updates, comments, close/link operations). Pushes to the data cache branch.
---

# Triage Issue

Analyze a SkiaSharp GitHub issue and produce a structured, schema-validated triage JSON file.

## Overview

All issue data is **pre-cached** on the `docs-data-cache` branch (synced hourly). Never call the GitHub API for issue data — it's already there.

```
Phase 1 (Setup) → Phase 2 (Preprocess) → Phase 3 (Analyze) → Phase 4 (Validate) → Phase 5 (Persist)
```

---

## Phase 1 — Setup

Ensure the environment is ready. All steps run once per session.

```bash
pwsh --version    # Requires 7.5+

# Cache worktree
if [ ! -d ".data-cache" ]; then
    git worktree add .data-cache docs-data-cache
fi
git -C .data-cache pull --rebase origin docs-data-cache
CACHE=".data-cache/repos/mono-SkiaSharp"

# Docs worktree (optional — for question/docs research)
if git show-ref --verify --quiet refs/heads/docs 2>/dev/null; then
    [ -d ".docs" ] || git worktree add .docs docs
fi
```

---

## Phase 2 — Preprocess

### 1. Read the cached issue

```bash
cat $CACHE/github/items/{number}.json
```

If missing, tell the user it will be available after the next hourly sync. **Stop — do not call the GitHub API.**

### 2. Convert to annotated markdown

```bash
pwsh .github/skills/triage-issue/scripts/issue-to-markdown.ps1 $CACHE/github/items/{number}.json
```

Produces: author role tags, time-deltas, attachments table, collapsed bot comments, preserved URLs.

### 3. Research documentation (conditional)

Consult docs **when relevant signals are present** — not every issue needs research.

| Signal in issue | Source to consult |
|----------------|-------------------|
| NativeAssets, DllNotFoundException, container, WASM | [documentation/packages.md](../../documentation/packages.md) |
| Platform-specific quirks, common traps | [references/skia-patterns.md](references/skia-patterns.md) |
| Specific SkiaSharp types or methods | `docs/SkiaSharpAPI/*.xml` |
| How-to about drawing, paths, bitmaps | `.docs/docs/docs/` |
| Non-SkiaSharp tech (MAUI, Blazor, WPF) | `mslearn`/`microsoft_docs_search` MCP tool |

---

## Phase 3 — Analyze

### 1. First triage in session

Skim [references/triage-schema.json](references/triage-schema.json) for field definitions and allowed values, and [references/triage-examples.md](references/triage-examples.md) for calibration. After that, rely on the schema validator.

### 2. Classify and reason

Write brief internal analysis (3-5 sentences), then classify the issue type. After classification, read [references/research-by-type.md](references/research-by-type.md) for type-specific research guidance. Conduct the research indicated before generating JSON.

### 3. Generate JSON

Produce the full triage JSON with 6 top-level groups: `meta`, `summary`, `classification`, `evidence`, `analysis`, `output`.

#### meta + summary

- **`schemaVersion`**: `"2.0"`
- **`analyzedAt`**: Full ISO 8601 UTC: `"2026-02-08T15:00:00Z"`
- **`currentLabels`**: Array of labels currently on the issue (for delta computation)

#### classification

- **Every field**: needs `value` and `confidence` (0.0–1.0) — NO per-field `reason`
- **Values ARE full GitHub labels**: `"type/bug"`, `"os/Linux"`, `"area/SkiaSharp"` — not bare suffixes
- **Nullable sections**: use JSON `null`, never `{}` or `[]`

#### evidence

- **`bugSignals`**: REQUIRED for bugs, `null` for non-bugs (schema enforces)
- **`reproEvidence`**: extract ALL screenshots, attachments, repo links, code, steps — preserve every URL
- **`versionAnalysis`**: no `era` field — just `mentionedVersions`, `currentRelevance`, `reason`, `migrationPath`

#### analysis (REQUIRED)

Single reasoning section. All classification reasoning goes in `fieldRationales` (the only source of truth):

- **`keySignals`**: Direct quotes with source and interpretation (no `supportedFields`)
- **`fieldRationales`**: Required for every judgment field listed below. Include `alternatives` for ambiguous choices.
- **`uncertainties`**: What's unclear and what would resolve it
- **`assumptions`**: Explicit assumptions when evidence was insufficient (omit if none)
- **`resolution`**: Proposals with `{title, description, confidence, effort}` — no steps/pros/cons

Resolution proposals populated for ALL issue types. Null only for duplicates or abandoned issues.

##### fieldRationales — required fields

**Always** (must have a rationale in every triage):
- `classification.type`
- `classification.area`
- `output.actionability.suggestedAction`
- `evidence.bugSignals.severity` (when bugSignals is non-null)

**When set** (must have a rationale whenever the field is non-null):
- `classification.platforms`
- `classification.tenets`
- `classification.backends`
- `classification.partner`
- `evidence.regression.isRegression`
- `evidence.fixStatus.likelyFixed`
- `evidence.versionAnalysis.currentRelevance`

#### output

- **`actionability`**: `suggestedAction`, `confidence`, `reason`, `requiresHumanReview`, `missingInfo`
  - No `closeable`/`closeReason`/`abandoned`/`duplicateOf` — those are now in actions
- **`actions`**: Array of automatable operations, each independently approvable:
  - `update-labels` (risk: low), `add-comment` (risk: high), `close-issue` (risk: medium)
  - `link-related` (risk: low), `link-duplicate` (risk: medium), `convert-to-discussion` (risk: high)
  - `update-project` (risk: low), `set-milestone` (risk: low)
  - Each action: `{id, type, risk, description, reason, confidence, dependsOn, payload}`
  - No `status` field — consumer adds this

#### add-comment action (replaces suggestedResponse)

When generating a draft, read [references/response-guidelines.md](references/response-guidelines.md) for tone and structure. Follow acknowledge → analyze → ask. Max 2000 chars, inline markdown. Set `requiresHumanEdit: true` always.

#### Confidence scoring

Scale: 0.95+ = explicit evidence, 0.80-0.94 = strong inference, 0.60-0.79 = ambiguous, <0.40 = guessing. Set `requiresHumanReview: true` if any key confidence is below 0.70.

---

## Phase 4 — Validate

Write JSON to `/tmp/triage-{number}.json`, then validate:

```bash
pwsh .github/skills/triage-issue/scripts/validate-triage.ps1 /tmp/triage-{number}.json
```

| Exit code | Meaning | Action |
|-----------|---------|--------|
| 0 | Valid | Proceed to Phase 5 |
| 1 | Schema violations | Fix JSON, re-validate (max 3 attempts) |
| 2 | Environment error | Stop and report |

---

## Phase 5 — Persist & Present

### 1. Write to cache

```bash
mkdir -p $CACHE/ai-triage/
cp /tmp/triage-{number}.json $CACHE/ai-triage/{number}.json
```

Re-triage replaces existing files. If the issue hasn't changed since `analyzedAt`, confirm with the user first.

### 2. Present summary

```
✅ Triage written: ai-triage/{number}.json

Type:       type/bug (0.98)
Area:       area/SkiaSharp
Severity:   critical — Fatal crash in disposal path
Action:     needs-investigation

Actions (3):
  labels-1    update-labels    [low]  Add type/bug, area/SkiaSharp
  comment-1   add-comment      [high] Post analysis response ⚠️ requires human edit
  link-1      link-related     [low]  Cross-ref #1234
```

If an `add-comment` action exists, show its `draftBody` in a copy-paste block with "⚠️ requires human review" warning.

**⚠️ NEVER post draft responses via GitHub API.** Always present for human review.

### 3. Push to remote

```bash
cd .data-cache
git restore --staged :/
git add repos/mono-SkiaSharp/ai-triage/{number}.json
git commit -m "ai-triage: classify #{number}"
git push
cd ..
```

On push conflict, rebase up to 3 times. On merge conflict, ask user whether to keep local or remote. If push fails after 3 attempts, surface the error.

---

## References

| File | When to read |
|------|--------------|
| [references/triage-schema.json](references/triage-schema.json) | First triage in session — field definitions, enums, cross-field rules |
| [references/triage-examples.md](references/triage-examples.md) | First triage in session — full JSON examples for calibration |
| [references/research-by-type.md](references/research-by-type.md) | After type classification — type-specific research checklists |
| [references/skia-patterns.md](references/skia-patterns.md) | During research — platform quirks, common traps, diagnostic heuristics |
| [references/response-guidelines.md](references/response-guidelines.md) | When generating `suggestedResponse.draft` — tone, structure, good/bad examples |
| [references/labels.md](references/labels.md) | If unsure about label values — taxonomy and cardinality rules |
| [documentation/packages.md](../../documentation/packages.md) | When issue involves NativeAssets, DllNotFoundException, containers, WASM, deployment |

## Scripts

All PowerShell 7.5+:

- **`scripts/issue-to-markdown.ps1 <file.json>`** — Preprocess cached issue into annotated markdown
- **`scripts/validate-triage.ps1 <triage.json>`** — Validate against schema (draft 2020-12)
