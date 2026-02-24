---
name: triage-issue
description: >-
  Triage a SkiaSharp GitHub issue or PR into structured JSON with classification
  (type, area, platform, severity), suggested response, and automatable actions.
  Triggers: "triage #123", "triage issue", "classify issue", "analyze issue",
  "what's this issue about". Also triggered when an issue number is given after
  the triage-issue skill is already mentioned.
---

# Triage Issue

**Bug pipeline: Step 1 of 3 (Triage).** See [`documentation/bug-pipeline.md`](../../../documentation/bug-pipeline.md).

Analyze a SkiaSharp GitHub issue and produce a structured, schema-validated triage JSON.

### Data sources

**Local cache first.** Issue data is pre-cached on the `docs-data-cache` branch (synced hourly). The cache contains full issue JSON with comments, labels, and timeline — use it as the primary source. You can `grep` across all cached items for duplicate detection and cross-referencing.

**GitHub CLI fallback.** If the issue is not in the cache (too new, or cache stale), use `gh` CLI or the GitHub MCP tools (`issue_read`, `get_file_contents`) to fetch it directly. Local cache is faster and searchable; API is the fallback, not forbidden.

```
Phase 1 (Setup) → Phase 2 (Preprocess + Investigate) → Phase 3 (Analyze) → Phase 3.5 (Workaround Search) → Phase 3.7 (Validate Code) → Phase 4 (Schema Validate) → Phase 5 (Persist)
```

---

## Phase 1 — Setup

Run once per session:

```bash
pwsh --version    # Requires 7.5+

# Cache worktree
[ -d ".data-cache" ] || git worktree add .data-cache docs-data-cache
git -C .data-cache pull --rebase origin docs-data-cache
CACHE=".data-cache/repos/mono-SkiaSharp"

# Docs worktree (optional — for question/docs research)
if git show-ref --verify --quiet refs/heads/docs 2>/dev/null; then
    [ -d ".docs" ] || git worktree add .docs docs
fi
```

---

## Phase 2 — Preprocess

### 1. Read the issue

```bash
cat $CACHE/github/items/{number}.json
```

If not in cache, fetch via GitHub CLI or MCP tools:

```bash
gh issue view {number} --repo mono/SkiaSharp --json title,body,labels,comments,state,createdAt,closedAt,author
```

### 2. Convert to annotated markdown

If using cached JSON:

```bash
pwsh .github/skills/triage-issue/scripts/issue-to-markdown.ps1 $CACHE/github/items/{number}.json > /tmp/issue-{number}.md
```

If fetched via API, work directly from the `gh` output (skip the script).

### 3. Code Investigation (MANDATORY)

**Before ANY classification**, search the source code for the types, methods, APIs, or behaviors mentioned in the issue. Read the relevant files. Record every finding in `analysis.codeInvestigation` as `{file, lines, relevance}`.

**Do NOT classify until you have examined source code.** The schema requires at least one `codeInvestigation` entry. Close-* actions require at least two.

**Steps:**
1. Grep for the types/methods/APIs mentioned in the issue
2. Read the relevant source files (platform handlers, core APIs, etc.)
3. Check if the reported behavior matches current code
4. For bugs: trace the code path — does the issue still exist?
5. For feature requests: has it been implemented since filing?
6. For questions: does the source confirm or contradict the assumption?

### 4. Additional Research

| Signal in issue | Source to consult |
|----------------|-------------------|
| NativeAssets, DllNotFoundException, container, WASM | [documentation/packages.md](../../documentation/packages.md) |
| Platform quirks, common traps | [references/skia-patterns.md](references/skia-patterns.md) |
| Specific SkiaSharp types or methods | `docs/SkiaSharpAPI/*.xml` |
| How-to about drawing, paths, bitmaps | `.docs/docs/docs/` |
| Non-SkiaSharp tech (MAUI, Blazor, WPF) | `mslearn`/`microsoft_docs_search` MCP tool |

---

## Phase 3 — Analyze

### First triage in session

Read [references/labels.md](references/labels.md) for valid label values and cardinality, and [references/triage-examples.md](references/triage-examples.md) for calibration.

### Classify and generate JSON

Write brief internal analysis (3–5 sentences), classify the type, then read [references/research-by-type.md](references/research-by-type.md) for type-specific research. Conduct the research, then generate JSON with 6 groups:

> **⚠️ Schema rules:**
> - `meta.schemaVersion` must be `"1.0"`
> - **Optional fields: OMIT them entirely** — do NOT set them to `null`. If a field is not applicable, leave it out of the JSON.
> - `classification.platforms`, `classification.backends`, and `classification.tenets` are simple string arrays (no confidence wrapper)
> - `classification.partner` is a single string (no confidence wrapper)
> - `evidence.bugSignals` includes: `severity`, `isRegression`, `errorType`, `errorMessage`, `stackTrace`, `reproQuality`, `targetFrameworks`
> - `evidence.regression` and `evidence.fixStatus` are optional objects — include when signals exist
> - `analysis.keySignals[]` captures structured evidence quotes `{text, source, interpretation}`
> - `analysis.resolution.proposals[]` include `title`, `codeSnippet` when applicable
> - `analysis.resolution.recommendedProposal` names the best proposal to try first
> - `analysis.rationale` is a single summary string (not per-field)
> - `output.actions` are simplified: `{ type, description }` with optional `risk`, `confidence`, `labels`, `comment`, `linkedIssue`
> - `additionalProperties: false` — no extra fields allowed at any level

#### meta + summary

- `schemaVersion`: `"1.0"`
- `analyzedAt`: ISO 8601 UTC
- `currentLabels`: labels currently on the issue

#### classification

- `type` and `area` are **compulsory** (non-nullable) — every issue must have both
- `type` and `area` have `value` (full GitHub label) and `confidence` (0.0–1.0)
- `platforms`, `backends`, and `tenets` are simple string arrays (no confidence) — omit if not applicable
- `partner` is a single string — omit if no partner involvement
- Valid labels: see [references/labels.md](references/labels.md)

#### evidence

- `bugSignals`: REQUIRED for bugs — `severity`, `isRegression`, `errorType`, `errorMessage`, `stackTrace`, `reproQuality` (complete/partial/none), `targetFrameworks` (TFM strings)
- `reproEvidence`: extract ALL screenshots, attachments, code snippets, steps — preserve every URL. Include `relatedIssues` (issue numbers) and `repoLinks` (external repro repos) when found.
- `versionAnalysis`: `mentionedVersions`, `workedIn`, `brokeIn`, `currentRelevance` (likely/unlikely/unknown), `relevanceReason`
- `regression`: Include when regression signals exist — `{isRegression, confidence, reason, workedInVersion, brokeInVersion}`
- `fixStatus`: Include when there's evidence the issue may already be fixed — `{likelyFixed, confidence, reason, relatedPRs, relatedCommits, fixedInVersion}`

#### analysis

- **`codeInvestigation`**: Source code signals from mandatory investigation — `{file, lines?, finding, relevance}` for each file examined. At least one required for bugs; close-* actions require at least two.
- **`keySignals`**: Structured evidence quotes — `{text, source, interpretation}` for each signal that drove triage decisions. Enables downstream querying and audit.
- **`rationale`**: Single paragraph explaining key classification decisions (type, area, severity). Replace the former per-field rationales with one concise explanation.
- **`workarounds`**: Array of workaround strings found during triage.
- **`nextQuestions`**: Open questions that repro or fix should investigate.
- **`errorFingerprint`**: Normalized fingerprint of the error for cross-issue dedup (optional).
- **`resolution`**: Proposals with `{title?, description, codeSnippet?, confidence?, effort?}`. Include `recommendedProposal` (title of best proposal) and `recommendedReason`. Null only for duplicates/abandoned.

#### output

- **`actionability`**: `suggestedAction`, `confidence`, `reason`
- **`missingInfo`**: Information needed from the reporter (optional array).
- **`actions`**: Simplified automatable operations — each is `{type, description}` with optional `risk`, `confidence`, `labels`, `comment`, `linkedIssue`:

| Type | Risk | Notes |
|------|------|-------|
| `update-labels` | low | Include `labels` array |
| `add-comment` | high | Read [references/response-guidelines.md](references/response-guidelines.md) for tone. Include `comment` text |
| `close-issue` | medium | |
| `link-related` | low | Include `linkedIssue` number |
| `link-duplicate` | medium | Include `linkedIssue` number |
| `convert-to-discussion` | high | |
| `update-project` | low | |
| `set-milestone` | low | |

---

## Phase 3.5 — Workaround Search

For bugs, questions, and feature requests: **actively search for workarounds** the reporter can use now. Follow [references/workaround-search.md](references/workaround-search.md) — 9 sources in priority order (cached triages → closed issues → known patterns → source code → docs → web).

- Set proposal `category` to `"workaround"` / `"fix"` / `"alternative"` / `"investigation"`
- Include `codeSnippet` on any proposal with copy-paste-ready code
- Set `validated` to `"untested"` initially (upgraded in Phase 3.7)

Skip this phase for duplicates and abandoned issues (resolution is null).

---

## Phase 3.7 — Workaround Validation (conditional)

If any proposal `description`, `codeSnippet`, or `add-comment` `draftBody` contains fenced code blocks or `SK*` API calls: validate with 3 parallel agents per [references/workaround-validation.md](references/workaround-validation.md).

**Gate:** No code blocks → skip (set `validated: "untested"`). ~60% of triages skip this step.

**Agents** (parallel `explore` type, Haiku model):
1. **API correctness** — do the SK* types/methods exist with correct signatures?
2. **Behavioral correctness** — disposal, null-checks, threading, does it solve the problem?
3. **Platform safety** — will it work on the reporter's platform?

**Synthesis:** All pass → `validated: "yes"`. Any warn → add caveats to draftBody, reduce confidence. Any fail → fix or strip code, set `validated: "no"`.

---

## Phase 4 — Validate

```bash
pwsh .github/skills/triage-issue/scripts/validate-triage.ps1 /tmp/triage-{number}.json
```

- Exit 0 = valid → Phase 5
- Exit 1 = fix and retry (max 3)
- Exit 2 = stop and report

---

## Phase 5 — Persist & Present

### 1. Write to cache

```bash
cp /tmp/triage-{number}.json $CACHE/ai-triage/{number}.json
```

### 2. Present summary

```
✅ Triage: ai-triage/{number}.json

Type:     type/bug (0.98)     Area: area/SkiaSharp
Severity: critical            Action: needs-investigation

Actions:
  labels-1   update-labels   [low]  Add type/bug, area/SkiaSharp
  comment-1  add-comment     [high] ⚠️ requires human edit
  link-1     link-related    [low]  Cross-ref #1234
```

**Pipeline hint:**
- If `classification.type.value == "type/bug"` and `output.actionability.suggestedAction == "needs-investigation"`: next step is **bug-repro** (`ai-repro/{number}.json`).
- If repro already exists and reproduces: next step is **bug-fix** (consume both JSONs).

If `add-comment` exists, show `draftBody` in a copy-paste block. **⚠️ NEVER post via GitHub API.**

### 3. Push

```bash
cd .data-cache
git add repos/mono-SkiaSharp/ai-triage/{number}.json
git commit -m "ai-triage: classify #{number}"
git push  # Rebase up to 3x on conflict
cd ..
```

---

## Anti-Patterns (NEVER DO)

1. **Pre-baked delegation.** NEVER write your classification in a sub-agent prompt. Sub-agents investigate and return evidence. YOU classify based on their evidence.

2. **Age-based closure.** NEVER close an issue because it's old. Old issues with no code fix are STILL OPEN BUGS/REQUESTS.

3. **Platform-deprecated ≠ stale.** NEVER assume a Xamarin.Forms issue is stale. Check if the same code/gap exists in MAUI before suggesting closure.

4. **Assertion without citation.** NEVER write "the code does X" without a `{file, lines}` entry in `codeInvestigation`. No file:line = no claim.

5. **Batch shortcuts.** When triaging multiple issues, each gets FULL investigation. Parallel investigation is fine; parallel conclusions are not.

---

## Scripts

- **`scripts/issue-to-markdown.ps1 <file.json>`** — Preprocess cached issue → annotated markdown
- **`scripts/validate-triage.ps1 <triage.json>`** — Validate against schema + rationale coverage + action integrity
