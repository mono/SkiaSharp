---
name: issue-triage
description: >-
  Triage a SkiaSharp GitHub issue or PR into structured JSON with classification
  (type, area, platform, severity), suggested response, and automatable actions.
  Triggers: "triage #123", "triage issue", "classify issue", "analyze issue",
  "what's this issue about". Also triggered when an issue number is given after
  the issue-triage skill is already mentioned.
---

# Triage Issue

**Bug pipeline: Step 1 of 3 (Triage).** See [`documentation/bug-pipeline.md`](../../../documentation/bug-pipeline.md).

Analyze a SkiaSharp GitHub issue and produce a structured, schema-validated triage JSON.

## ⛔ MANDATORY FIRST STEPS (do not skip)

1. Read THIS entire SKILL.md before any investigation
2. Read [references/schema-cheatsheet.md](references/schema-cheatsheet.md) for required fields and enums
3. Read [references/anti-patterns.md](references/anti-patterns.md) for critical rules

These 3 reads are REQUIRED. Do not proceed to Phase 1 until all three are loaded.

> **Quick flow:**
> 1. Setup cache worktree
> 2. Load issue data (cache first, GitHub API fallback)
> 3. Read references: [schema-cheatsheet](references/schema-cheatsheet.md), [labels](references/labels.md), [triage-examples](references/triage-examples.md), [anti-patterns](references/anti-patterns.md)
> 4. Create brief plan (5-10 lines)
> 5. Investigate code — **READ-ONLY, never edit source files**
> 6. Generate JSON
> 7. Validate with script
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

### 3. Code Investigation (MANDATORY)

> **Scope: READ code, don't WRITE code.** Grep, read files, trace call chains. Never create files, compile, or execute.

**Before ANY classification**, search the source code for the types, methods, APIs, or behaviors mentioned in the issue. Read the relevant files. Record every finding in `analysis.codeInvestigation` as `{file, finding, relevance}` (with optional `lines`).

**Do NOT classify until you have examined source code.** For bugs, include at least one `codeInvestigation` entry. Close-* actions should include at least two.

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

> **Pre-flight — confirm before analyzing:**
>
> - [ ] Cache worktree is set up and pulled
> - [ ] Issue data loaded (cache JSON or GitHub API)
> - [ ] Read [references/schema-cheatsheet.md](references/schema-cheatsheet.md) for required fields and enums
> - [ ] Read [references/labels.md](references/labels.md) for valid label values
> - [ ] Read [references/triage-examples.md](references/triage-examples.md) for calibration
> - [ ] Read [references/anti-patterns.md](references/anti-patterns.md) — at least the critical rules
> - [ ] Created a brief plan (5-10 lines: what to investigate, what type you suspect)
>
> **Reminder:** Triage is READ-ONLY. Do NOT edit any source files (.cs, .cpp, .csproj, .json).

## Phase 3 — Analyze

### First triage in session

Read [references/labels.md](references/labels.md) for valid label values and cardinality, [references/triage-examples.md](references/triage-examples.md) for calibration, and [references/schema-cheatsheet.md](references/schema-cheatsheet.md) for required fields.

### Classify and generate JSON

Write brief internal analysis (3–5 sentences), classify the type, then read [references/research-by-type.md](references/research-by-type.md) for type-specific research. Conduct the research, then generate the JSON. Write to `/tmp/skiasharp/triage/{number}.json` — use this exact literal path, do NOT substitute `$TMPDIR` or any other variable.

> **⚠️ Schema Compliance:**
>
> 1. **Read [references/schema-cheatsheet.md](references/schema-cheatsheet.md)** — This is the authoritative source for structure, fields, and enums.
> 2. **Review [references/labels.md](references/labels.md)** — Use only valid label values.
> 3. **Follow these critical constraints:**
>    - `meta.schemaVersion` must be `"1.0"`
>    - **Optional fields:** OMIT entirely if not applicable. Do NOT set to `null`.
>    - **String Arrays:** `platforms`, `backends`, `tenets` are simple string arrays (no confidence wrapper).
>    - **Investigation:** `analysis.codeInvestigation` is MANDATORY. At least one entry for bugs, two for close-* actions.
>    - **Rationale:** `analysis.rationale` is a single summary string (not per-field).
>    - **Validation:** No extra properties allowed (`additionalProperties: false`).

#### JSON Groups Overview

| Group | Content |
|-------|---------|
| `summary` | One-sentence description of the issue (top-level string, required). |
| `meta` | Version `"1.0"`, issue number, repo, `analyzedAt` (ISO 8601). |
| `classification` | `type` and `area` (required objects with confidence). `platforms`, `backends` (optional string arrays). |
| `evidence` | `bugSignals` (for bugs), `reproEvidence` (all attachments/links), `regression` (if claimed), `fixStatus` (if fixed). |
| `analysis` | `summary` (required), `codeInvestigation` (findings from Phase 2), `keySignals` (quotes), `rationale` (decision summary), `resolution` (proposals). |
| `output` | `actionability` (suggested action) and `actions` (automatable tasks). |

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

> **🛑 PHASE GATE: You CANNOT proceed to Phase 5 without passing validation.**
> **Skipping validation = INVALID triage. The task is incomplete.**

```bash
# Try pwsh first, fall back to python3
pwsh .github/skills/issue-triage/scripts/validate-triage.ps1 /tmp/skiasharp/triage/{number}.json \
  || python3 .github/skills/issue-triage/scripts/validate-triage.py /tmp/skiasharp/triage/{number}.json
```

- **Exit 0** = ✅ valid → proceed to Phase 5
- **Exit 1** = ❌ fix the errors listed in the output, then re-run. Repeat up to 3 times.
- **Exit 2** = fatal error, stop and report

> **⚠️ NEVER hand-roll your own validation. NEVER assume it passes. RUN THE SCRIPT.**

---

## Phase 5 — Persist & Present

> **🛑 PHASE GATE: Phase 4 validator MUST have printed ✅ before you reach this step.**
> **If you have not run the validation script, GO BACK and run it now.**

### 1. Persist

```bash
pwsh .github/skills/issue-triage/scripts/persist-triage.ps1 /tmp/skiasharp/triage/{number}.json
```

This copies the JSON to data-cache and handles git automatically (skips in benchmark mode).

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
- If repro already exists and reproduces: next step is **issue-fix** (consume both JSONs).

If `add-comment` exists, show `comment` in a copy-paste block. **⚠️ NEVER post via GitHub API.**

---

## Anti-Patterns

See [references/anti-patterns.md](references/anti-patterns.md) — **read this file on first triage in session**.

**#0 (CRITICAL):** Triage is READ-ONLY. If you edit a source file during triage, you have FAILED. See the anti-patterns reference for the full list.

**#1 (CRITICAL):** NEVER use `store_memory` during triage. Triage produces JSON artifacts, not memories. Storing unverified facts pollutes all future sessions.

**#2 (CRITICAL):** NEVER skip the validation script. You MUST run `validate-triage.ps1` (or `.py` fallback) and see ✅ before persisting. Mentally checking fields is not validation. If the script isn't run, the triage is invalid.

---

## Scripts

- **`scripts/issue-to-markdown.ps1 <file.json>`** — Preprocess cached issue → annotated markdown
- **`scripts/validate-triage.ps1 <triage.json>`** — Validate against schema + rationale coverage + action integrity
