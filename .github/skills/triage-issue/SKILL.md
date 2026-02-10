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

Analyze a SkiaSharp GitHub issue and produce a structured, schema-validated triage JSON.

### Data sources

**Local cache first.** Issue data is pre-cached on the `docs-data-cache` branch (synced hourly). The cache contains full issue JSON with comments, labels, and timeline — use it as the primary source. You can `grep` across all cached items for duplicate detection and cross-referencing.

**GitHub CLI fallback.** If the issue is not in the cache (too new, or cache stale), use `gh` CLI or the GitHub MCP tools (`issue_read`, `get_file_contents`) to fetch it directly. Local cache is faster and searchable; API is the fallback, not forbidden.

```
Phase 1 (Setup) → Phase 2 (Preprocess) → Phase 3 (Analyze) → Phase 4 (Validate) → Phase 5 (Persist)
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

### 3. Research (conditional)

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

#### meta + summary

- `schemaVersion`: `"2.0"`
- `analyzedAt`: ISO 8601 UTC
- `currentLabels`: labels currently on the issue

#### classification

- `type` and `area` are **compulsory** (non-nullable) — every issue must have both
- Every field has `value` (full GitHub label) and `confidence` (0.0–1.0)
- Nullable sections: use JSON `null`, never `{}` or `[]`
- Valid labels: see [references/labels.md](references/labels.md)

#### evidence

- `bugSignals`: REQUIRED for bugs, `null` for non-bugs (schema enforces)
- `reproEvidence`: extract ALL screenshots, attachments, repo links, code, steps — preserve every URL
- `versionAnalysis`: `mentionedVersions`, `currentRelevance`, `reason`, `migrationPath`

#### analysis

- **`keySignals`**: Direct quotes with source and interpretation
- **`fieldRationales`**: Required for every judgment field (see below). Include `alternatives` for ambiguous choices.
- **`uncertainties`**: What's unclear and what would resolve it
- **`resolution`**: Proposals with `{title, description, confidence, effort}`. Null only for duplicates/abandoned.

##### fieldRationales — required fields

**Compulsory** (every triage — `type` and `area` are never nullable):
- `type`, `area`

**When set** (rationale required whenever the field is non-null):
- `actionability.suggestedAction`, `bugSignals.severity`, `platforms`, `tenets`, `backends`, `partner`, `regression.isRegression`, `fixStatus.likelyFixed`, `versionAnalysis.currentRelevance`

Schema validates rationale coverage. Use short field names (e.g., `"type"` not `"classification.type"`).

#### output

- **`actionability`**: `suggestedAction`, `confidence`, `reason`, `requiresHumanReview`, `missingInfo`
- **`actions`**: Automatable operations, each independently approvable:

| Type | Risk | Notes |
|------|------|-------|
| `update-labels` | low | |
| `add-comment` | high | Read [references/response-guidelines.md](references/response-guidelines.md) for tone |
| `close-issue` | medium | |
| `link-related` | low | |
| `link-duplicate` | medium | |
| `convert-to-discussion` | high | |
| `update-project` | low | |
| `set-milestone` | low | |

Each action: `{id, type, risk, description, reason, confidence, dependsOn, payload}`. 
Set `requiresHumanReview: true` if any key confidence < 0.70.

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

## Scripts

- **`scripts/issue-to-markdown.ps1 <file.json>`** — Preprocess cached issue → annotated markdown
- **`scripts/validate-triage.ps1 <triage.json>`** — Validate against schema + rationale coverage + action integrity
