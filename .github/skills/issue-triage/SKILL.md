---
name: issue-triage
description: >-
  Triage a SkiaSharp GitHub issue or PR into structured JSON with classification
  (type, area, platform, severity), suggested response, and automatable actions.
  Use whenever someone asks to triage, classify, categorize, or analyze a GitHub
  issue — including "triage #123", "what kind of issue is this", "what should we
  do about #NNNN", "analyze this issue", "sort this bug report", or any request
  to understand and categorize an issue before investigation begins.
---

# Triage Issue

**Issue pipeline: Step 1 of 3 (Triage).** See [`documentation/issue-pipeline.md`](../../../documentation/issue-pipeline.md).

Analyze a SkiaSharp GitHub issue and produce a structured, schema-validated triage JSON.

## Before You Start

Read these files first — they define the output format and hard constraints:

1. This SKILL.md (the workflow below)
2. [references/schema-cheatsheet.md](references/schema-cheatsheet.md) — required fields and enums
3. [references/anti-patterns.md](references/anti-patterns.md) — critical rules (the authoritative list)

> **Quick flow:**
> 1. Setup cache worktree
> 2. Load issue data (cache first, GitHub API fallback)
> 3. Investigate code — **READ-ONLY, never edit source files**
> 4. Create brief plan (5-10 lines)
> 5. Read [labels](references/labels.md) + [examples](references/triage-examples.md), then classify and generate JSON
> 6. Search for workarounds, validate any code snippets
> 7. Validate JSON with script
> 8. Persist to data-cache

### Data sources

**Local cache first.** Issue data is pre-cached on the `docs-data-cache` branch (synced hourly). The cache contains full issue JSON with comments, labels, and timeline — use it as the primary source. You can `grep` across all cached items for duplicate detection and cross-referencing.

**GitHub CLI fallback.** If the issue is not in the cache (too new, or cache stale), use `gh` CLI or the GitHub MCP tools (`issue_read`, `get_file_contents`) to fetch it directly. Local cache is faster and searchable; API is the fallback, not forbidden.

```
Phase 1 (Setup) → Phase 2 (Preprocess + Investigate) → Phase 3 (Analyze) → Phase 3.5 (Workaround Search) → Phase 3.7 (Validate Code) → Phase 4 (Validate) → Phase 5 (Persist)
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
pwsh .github/skills/issue-triage/scripts/issue-to-markdown.ps1 $CACHE/github/items/{number}.json > /tmp/skiasharp/triage/{number}.md
```

If fetched via API, work directly from the `gh` output (skip the script).

### 3. Code Investigation

> **Scope: READ code, don't WRITE code.** Grep, read files, trace call chains. Never create files, compile, or execute.

Code investigation grounds the triage in reality rather than surface-level pattern matching. Without reading the actual source, you'll misclassify "works as designed" bugs and miss already-fixed issues. Search for the types, methods, or APIs mentioned in the issue and record findings in `analysis.codeInvestigation` as `{file, finding, relevance}` (with optional `lines`).

For bugs, include at least one `codeInvestigation` entry. Close-* actions should include at least two — the higher bar is because closing an issue on wrong evidence is worse than leaving it open.

**Steps:**
1. Grep for the types/methods/APIs mentioned in the issue
2. Read the relevant source files (platform handlers, core APIs, etc.)
3. Check if the reported behavior matches current code
4. For bugs: trace the code path — does the issue still exist?
5. For feature requests: has it been implemented since filing?
6. For questions: does the source confirm or contradict the assumption?
7. **Search for related PRs** — check data-cache first, then fall back to CLI:
   ```bash
   # Data-cache lookup (fast, offline)
   ls $CACHE/github/items/ | head -5  # check cache structure
   # Fall back to GitHub CLI
   gh pr list --search '{keywords from issue}' --state all --repo mono/SkiaSharp --limit 10 --json number,title,state,mergedAt
   ```
   Include ALL related PRs in `evidence.reproEvidence.repoLinks` — especially closed/unmerged PRs, as they reveal prior attempts and maintainer decisions.

### 4. Additional Research

| Signal in issue | Source to consult |
|----------------|-------------------|
| NativeAssets, DllNotFoundException, container, WASM | [documentation/packages.md](../../documentation/packages.md) |
| Platform quirks, common traps | [references/skia-patterns.md](references/skia-patterns.md) |
| Specific SkiaSharp types or methods | `docs/SkiaSharpAPI/*.xml` |
| How-to about drawing, paths, bitmaps | `.docs/docs/docs/` |
| Non-SkiaSharp tech (MAUI, Blazor, WPF) | `mslearn`/`microsoft_docs_search` MCP tool |

---

### ✅ GATE: Do not proceed until you have:
- [ ] Cache worktree set up and pulled
- [ ] Issue data loaded (cache JSON or GitHub API)
- [ ] Code investigation done (at least one `codeInvestigation` entry)
- [ ] Brief plan created (5-10 lines: what to investigate, what type you suspect)

---

## Phase 3 — Analyze

### First triage in session

Read [references/labels.md](references/labels.md) for valid label values, [references/triage-examples.md](references/triage-examples.md) for calibration examples, and [references/schema-cheatsheet.md](references/schema-cheatsheet.md) for required fields.

### Classify and generate JSON

Write brief internal analysis (3–5 sentences), classify the type, then read [references/research-by-type.md](references/research-by-type.md) for type-specific research. Conduct the research, then generate the JSON. Write to `/tmp/skiasharp/triage/{number}.json` — use this exact literal path, do NOT substitute `$TMPDIR` or any other variable.

> **Schema:** Follow [references/schema-cheatsheet.md](references/schema-cheatsheet.md) exactly — it covers required fields, enum values, and common mistakes. Key gotchas: `platforms`/`backends`/`tenets` are plain string arrays (not `{value, confidence}` objects), optional fields should be omitted entirely (never `null`), and `additionalProperties: false` is enforced at every level.

#### JSON Groups Overview

| Group | Content |
|-------|---------|
| `summary` | One-sentence description of the issue (top-level string, required). |
| `meta` | Version `"1.0"`, issue number, repo, `analyzedAt` (ISO 8601). |
| `classification` | `type` and `area` (required objects with confidence). `platforms`, `backends` (optional string arrays). |
| `evidence` | `bugSignals` (for bugs), `reproEvidence` (all attachments/links), `regression` (if claimed), `fixStatus` (if fixed). |
| `analysis` | `summary` (required), `codeInvestigation` (findings from Phase 2), `keySignals` (quotes), `rationale` (decision summary), `resolution` (proposals). |
| `output` | `actionability` (suggested action + `suggestedReproPlatform`) and `actions` (automatable tasks). |

Refer to the cheatsheet for the exact field structure and enum values.

---

## Phase 3.5 — Workaround Search

For bugs, questions, and feature requests: **actively search for workarounds** the reporter can use now. Follow [references/workaround-search.md](references/workaround-search.md) — 9 sources in priority order (cached triages → closed issues → known patterns → source code → docs → web).

- Set proposal `category` to `"workaround"` / `"fix"` / `"alternative"` / `"investigation"`
- Include `codeSnippet` on any proposal with copy-paste-ready code
- Set `validated` to `"untested"` initially (upgraded in Phase 3.7)

Skip this phase for duplicates and abandoned issues (omit `analysis.resolution`).

---

## Phase 3.7 — Workaround Validation (conditional)

If any proposal `description`, `codeSnippet`, or `add-comment` `comment` contains fenced code blocks or `SK*` API calls: validate with 3 parallel agents per [references/workaround-validation.md](references/workaround-validation.md).

**Gate:** No code blocks → skip (set `validated: "untested"`). ~60% of triages skip this step.

**Agents** (parallel `explore` type, Haiku model):
1. **API correctness** — do the SK* types/methods exist with correct signatures?
2. **Behavioral correctness** — disposal, null-checks, threading, does it solve the problem?
3. **Platform safety** — will it work on the reporter's platform?

**Synthesis:** All pass → `validated: "yes"`. Any warn → add caveats to `comment`, reduce confidence. Any fail → fix or strip code, set `validated: "no"`.

---

## Phase 4 — Validate

Validation catches schema errors that cause downstream pipeline failures — the repro and fix skills parse this JSON programmatically, so a malformed triage breaks the entire chain.

```bash
# Try pwsh first, fall back to python3
pwsh .github/skills/issue-triage/scripts/validate-triage.ps1 /tmp/skiasharp/triage/{number}.json \
  || python3 .github/skills/issue-triage/scripts/validate-triage.py /tmp/skiasharp/triage/{number}.json
```

- **Exit 0** = ✅ valid → proceed to Phase 5
- **Exit 1** = ❌ fix the errors listed in the output, then re-run. Repeat up to 3 times.
- **Exit 2** = fatal error, stop and report

> Always use the validation scripts — hand-checking fields is error-prone and misses schema constraints.

---

## Phase 5 — Persist & Present

> **Prerequisite:** Phase 4 validator printed ✅.

### 1. Persist

Use the persist script — it handles copying, committing, and pushing with retry logic. Manual git commands risk leaving unpushed commits that are lost when the runner shuts down.

```bash
pwsh .github/skills/issue-triage/scripts/persist-triage.ps1 /tmp/skiasharp/triage/{number}.json
```

This copies the JSON to data-cache and handles git commit + push automatically (skips in benchmark mode).

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
- If `classification.type.value == "type/bug"` and `output.actionability.suggestedAction == "needs-investigation"`: next step is **issue-repro** (`ai-repro/{number}.json`).
- If `classification.type.value` is `type/enhancement` or `type/feature-request` and `suggestedAction == "needs-investigation"`: next step is also **issue-repro**, but repro will use `confirmed`/`not-confirmed` instead of `reproduced`/`not-reproduced`.
- `output.actionability.suggestedReproPlatform` tells CI which runner to use for reproduction:
  - `macos` — for os/iOS, os/macOS, os/tvOS, os/watchOS issues
  - `windows` — for os/Windows-Classic, os/Windows-WinUI, os/Windows-Universal-UWP, os/Windows-Nano-Server issues
  - `linux` — for everything else (os/Linux, os/WASM, os/Android, os/Tizen, or no platform specified)
  - **Note:** For platform-independent API bugs (e.g., SVG output, stream handling), prefer `linux` even if the reporter tests on other platforms.
- If repro already exists and reproduces: next step is **issue-fix** (consume both JSONs).

If `add-comment` exists, show `comment` in a copy-paste block. Comments are never posted automatically — always present them for human review.

---

## Error Recovery

| Issue | Recovery |
|-------|----------|
| Cache worktree missing/corrupt | `git worktree remove .data-cache && git worktree add .data-cache docs-data-cache` |
| Issue not in cache | Fall back to `gh issue view` or GitHub MCP tools |
| Validation fails (Exit 1) | Fix the listed errors, re-run. Up to 3 retries before stopping. |
| Validation fatal (Exit 2) | Stop and report — likely a schema version mismatch |
| Persist script fails | Check branch is up-to-date, retry. If push conflict, pull and retry. |
| Can't classify type | Default to `type/bug` with low confidence + `request-info` action |
| Workaround validation agents timeout | Set `validated: "untested"` and proceed |

---

## Anti-Patterns

See [references/anti-patterns.md](references/anti-patterns.md) for the full list of critical rules (loaded at startup).

---

## Scripts

- **`scripts/issue-to-markdown.ps1 <file.json>`** — Preprocess cached issue → annotated markdown
- **`scripts/validate-triage.ps1 <triage.json>`** — Validate against schema + rationale coverage + action integrity
