---
name: issue-triage
description: >-
  Triage a SkiaSharp GitHub issue or PR into structured JSON with classification
  (type, area, platform, severity), suggested response, automatable actions, and
  companion Markdown/HTML reports. Triggers: "triage #123", "triage issue",
  "classify issue", "analyze issue", "what's this issue about". Also triggered
  when an issue number is given after the issue-triage skill is already mentioned.
---

# Triage Issue

**Issue pipeline: Step 1 of 3 (Triage).** See [`documentation/dev/issue-pipeline.md`](../../../documentation/dev/issue-pipeline.md).

Analyze a SkiaSharp GitHub issue and produce a structured, schema-validated triage JSON.

## ⛔ MANDATORY FIRST STEPS (do not skip)

1. Read THIS entire SKILL.md before any investigation
2. Read [references/schema-cheatsheet.md](references/schema-cheatsheet.md) for required fields and enums
3. Read [references/anti-patterns.md](references/anti-patterns.md) for critical rules

These 3 reads are REQUIRED. Do not proceed to Phase 1 until all three are loaded.

> **Quick flow:**
> 1. Load issue data from GitHub (prefer `gh`, or GitHub MCP when available)
> 2. Read references: [schema-cheatsheet](references/schema-cheatsheet.md), [labels](references/labels.md), [triage-examples](references/triage-examples.md), [anti-patterns](references/anti-patterns.md)
> 3. Create brief plan (5-10 lines)
> 4. Investigate code — **READ-ONLY, never edit source files**
> 5. Generate JSON
> 6. Validate with script
> 7. Persist JSON + Markdown + HTML reports

### Data sources

**Primary source: GitHub itself.** Prefer `gh` CLI for repeatable issue/PR fetches and searches. If the environment exposes GitHub MCP issue/PR lookup tools, those are also valid. Do not set up or depend on a cache worktree or cache branch for this skill.

```
Phase 1 (Setup) → Phase 2 (Investigate) → Phase 3 (Analyze) → Phase 4 (Workarounds) → Phase 5 (Validate) → Phase 6 (Persist & Render)
```

---

## Phase 1 — Setup

Run once per session:

```bash
pip3 install -r .agents/skills/issue-triage/scripts/requirements.txt --quiet
python3 --version  # Requires 3.9+
python3 -c "import jinja2; print('jinja2', jinja2.__version__)"  # pip3 install jinja2
gh --version
gh auth status
```

If `gh` is unavailable or unauthenticated, use the GitHub MCP issue/PR retrieval and search tools available in the environment.

---

## Phase 2 — Investigate

### 1. Read the issue

```bash
gh issue view {number} --repo mono/SkiaSharp \
  --json number,title,body,labels,comments,state,createdAt,updatedAt,closedAt,author,milestone \
  > /tmp/{number}.issue.json
```

GitHub MCP issue/PR retrieval tools are equally valid if the environment exposes them.

### 2. Code Investigation (MANDATORY)

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
7. **Search for related issues AND PRs** — use `gh` CLI or GitHub MCP directly:
   ```bash
   # Search issues (finds duplicates, prior reports, related discussions)
   gh search issues "{keywords from issue}" --repo mono/SkiaSharp --limit 10 \
     --json number,title,state,url
   # Search PRs (finds fixes, prior attempts, reverted changes)
   gh pr list --search '{keywords from issue}' --state all --repo mono/SkiaSharp --limit 10 --json number,title,state,mergedAt
   ```
   Include ALL related issues and PRs in `evidence.reproEvidence.repoLinks` — both duplicate/related issues AND closed/unmerged PRs, as they reveal prior reports and maintainer decisions. Duplicate issues are especially important for `close-as-duplicate` classification.

### 3. Additional Research

| Signal in issue | Source to consult |
|----------------|-------------------|
| NativeAssets, DllNotFoundException, container, WASM | [documentation/dev/packages.md](../../../documentation/dev/packages.md) |
| Platform quirks, common traps | [references/skia-patterns.md](references/skia-patterns.md) |
| Specific SkiaSharp types or methods | `docs/SkiaSharpAPI/*.xml` |
| How-to about drawing, paths, bitmaps | `.docs/docs/docs/` |
| Non-SkiaSharp tech (MAUI, Blazor, WPF) | `mslearn`/`microsoft_docs_search` MCP tool |

---

> **Pre-flight — confirm before analyzing:**
>
> - [ ] Issue data loaded from `gh` CLI or GitHub MCP
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

Write brief internal analysis (3–5 sentences), classify the type, then read [references/research-by-type.md](references/research-by-type.md) for type-specific research. Conduct the research, then generate the JSON. Write to `/tmp/{number}.json`. Use this exact literal path structure, do NOT substitute `$TMPDIR` or any other variable. Do NOT use `mkdir` — write directly to `/tmp/`.

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
| `output` | `actionability` (suggested action + `suggestedReproPlatform`) and `actions` (automatable tasks). |

Refer to the cheatsheet for the exact field structure and enum values.

---

## Phase 4 — Workarounds

For bugs, questions, and feature requests: **actively search for workarounds** the reporter can use now. Follow [references/workaround-search.md](references/workaround-search.md) — 9 sources in priority order (existing triages → closed issues → known patterns → source code → docs → web).

- Set proposal `category` to `"workaround"` / `"fix"` / `"alternative"` / `"investigation"`
- Include `codeSnippet` on any proposal with copy-paste-ready code

Skip this phase for duplicates and abandoned issues (omit `analysis.resolution`).

### Validate Code in Proposals

If any proposal includes a `codeSnippet` or the `add-comment` `comment` contains code: **you must validate it** with 3 parallel agents per [references/workaround-validation.md](references/workaround-validation.md). Do not suggest code that has not been checked.

**Agents** (parallel `explore` type, Haiku model):
1. **API correctness** — do the SK* types/methods exist with correct signatures?
2. **Behavioral correctness** — disposal, null-checks, threading, does it solve the problem?
3. **Platform safety** — will it work on the reporter's platform?

**Synthesis:** All pass → `validated: "yes"`. Any warn → add caveats to `comment`, reduce confidence. Any fail → fix or strip code, set `validated: "no"`.

If no proposals contain code, set `validated: "untested"`.

---

## Phase 5 — Validate

> **🛑 PHASE GATE: You CANNOT proceed to Phase 6 without passing validation.**
> **Skipping validation = INVALID triage. The task is incomplete.**

```bash
python3 .agents/skills/issue-triage/scripts/validate-triage.py /tmp/{number}.json
```

- **Exit 0** = ✅ valid → proceed to Phase 6
- **Exit 1** = ❌ fix the errors listed in the output, then re-run. Repeat up to 3 times.
- **Exit 2** = fatal error, stop and report

> **⚠️ NEVER hand-roll your own validation. NEVER assume it passes. RUN THE SCRIPT.**

---

## Phase 6 — Persist & Present

> **🛑 PHASE GATE: Phase 5 validator MUST have printed ✅ before you reach this step.**
> **If you have not run the validation script, GO BACK and run it now.**

### 1. Persist

Copy the validated JSON to `output/ai/` for collection, then render companion Markdown and HTML reports that wrap the same JSON data.

```bash
python3 .agents/skills/issue-triage/scripts/persist-triage.py /tmp/{number}.json
```

This produces:

- `output/ai/repos/mono-SkiaSharp/ai-triage/{number}.json`
- `output/ai/repos/mono-SkiaSharp/ai-triage/{number}.md`
- `output/ai/repos/mono-SkiaSharp/ai-triage/{number}.html`

### 2. Present summary

```
✅ Triage: ai-triage/{number}.json
✅ Report: ai-triage/{number}.md
✅ Viewer: ai-triage/{number}.html

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

If `add-comment` exists, show `comment` in a copy-paste block. **⚠️ NEVER post via GitHub API.**

---

## Anti-Patterns

See [references/anti-patterns.md](references/anti-patterns.md) — **read this file on first triage in session**.

**#0 (CRITICAL):** Triage is READ-ONLY. If you edit a source file during triage, you have FAILED. See the anti-patterns reference for the full list.

**#1 (CRITICAL):** NEVER use `store_memory` during triage. Triage produces JSON artifacts, not memories. Storing unverified facts pollutes all future sessions.

**#2 (CRITICAL):** NEVER skip the validation script. You MUST run `validate-triage.py` and see ✅ before persisting. Mentally checking fields is not validation. If the script isn't run, the triage is invalid.

---

## Scripts

- **`scripts/validate-triage.py <triage.json>`** — Validate against schema + rationale coverage + action integrity
- **`scripts/persist-triage.py <triage.json>`** — Copy validated JSON to `output/ai/` and invoke renderer
- **`scripts/render-triage-report.py <triage.json>`** — Render validated triage JSON → `.md` + `.html` (uses Jinja2 template for markdown)
- **`scripts/triage-report.md.jinja2`** — Jinja2 template for the Markdown report
- **`scripts/viewer.html`** — Bootstrap 5 HTML template for the interactive viewer
