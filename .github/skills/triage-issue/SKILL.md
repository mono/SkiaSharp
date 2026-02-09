---
name: triage-issue
description: Triage a SkiaSharp GitHub issue into structured JSON (type, area, platform, severity, actionability). Use when the user says "triage #123", "triage issue 123", "classify issue", or "analyze issue". Produces a schema-validated JSON file and pushes to the data cache branch.
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

Ensure the environment is ready. All three steps run once per session.

### 1. Check prerequisites

```bash
pwsh --version    # Requires 7.5+
```

### 2. Ensure cache worktree exists and is current

The `docs-data-cache` branch must be checked out as a worktree at `.data-cache`:

```bash
if [ ! -d ".data-cache" ]; then
    git worktree add .data-cache docs-data-cache
fi
git -C .data-cache rev-parse --abbrev-ref HEAD  # Must be "docs-data-cache"
git -C .data-cache pull --rebase origin docs-data-cache
```

If the branch check fails, abort with a clear message. All paths below use `$CACHE` as shorthand:

```bash
CACHE=".data-cache/repos/mono-SkiaSharp"
```

### 3. Ensure docs worktree exists

The `docs` branch contains conceptual documentation (tutorials, guides). Check it out as a read-only worktree at `.docs`:

```bash
if git show-ref --verify --quiet refs/heads/docs 2>/dev/null; then
    [ -d ".docs" ] || git worktree add .docs docs
fi
```

---

## Phase 2 — Preprocess

Read the issue data and prepare for analysis.

### 1. Read the cached issue

The cached issue JSON is at: `$CACHE/github/items/{number}.json`

```bash
cat .data-cache/repos/mono-SkiaSharp/github/items/{number}.json
```

If the file doesn't exist, tell the user: "Issue #{number} is not in the cache. It will be available after the next hourly sync." **Stop triage — do not call the GitHub API as a fallback.**

### 2. Convert to annotated markdown

```bash
pwsh .github/skills/triage-issue/scripts/issue-to-markdown.ps1 $CACHE/github/items/{number}.json
```

If the script exits non-zero, the cache file may be corrupted — tell the user to resync instead of continuing.

The preprocessor produces annotated markdown with:
- **Author role tags**: `[OP]`, `[MEMBER]`, `[CONTRIBUTOR]`, `[BOT]`
- **Time-deltas** between comments: `(+2d)`, `(+6mo)`
- **Attachments & Links table**: all screenshots, zip files, repo links, related issues
- **Bot comments collapsed** to single summary lines
- **All URLs preserved** — image URLs, zip downloads, repo links kept intact

### 3. Review allowed values

The schema (`references/triage-schema.json`) is self-documenting — every enum field lists the exact allowed values. Label-backed fields (area, backend, platform, tenet, partner) use values that match GitHub label suffixes: `prefix/ + value` = full label.

### 4. Research documentation (conditional)

Consult documentation **when relevant signals are present** — not every issue needs research. Project-specific docs override general .NET knowledge.

| Signal in issue | Source to consult |
|----------------|-------------------|
| NativeAssets, DllNotFoundException, container, WASM | `documentation/packages.md` |
| DllNotFoundException on Linux + NoDependencies | Also read `references/native-loading-diagnostics.md` |
| Specific SkiaSharp types or methods | `docs/SkiaSharpAPI/*.xml` |
| How-to about drawing, paths, bitmaps, transforms | `.docs/docs/docs/` (from Phase 1 Step 3 worktree) |
| Non-SkiaSharp tech (MAUI, Blazor, WPF, containers, trimming) | `mslearn-microsoft_docs_search` MCP tool |
| `type=question` or `type=documentation` | Check all sources above — the answer likely already exists |

---

## Phase 3 — Analyze

Generate the triage JSON. On the first triage in a session, skim `references/triage-schema.json` to understand fields and allowed values. After that, rely on the examples below and let the validator enforce exact shape.

### 1. Reason first

Write a brief internal analysis (3-5 sentences):
1. What type is this issue? What's the key evidence?
2. What's the suggested action and why?
3. Is it a bug? If so, what's the severity and is there a workaround?

### 2. Generate JSON

Use the examples and rules below to produce the triage JSON.

#### Examples

**Bug** (bugSignals required, analysisNotes required, resolutionAnalysis with 3+ proposals):

```json
{
  "schemaVersion": "1.0",
  "number": 1234,
  "repo": "mono/SkiaSharp",
  "analyzedAt": "2026-02-08T15:00:00Z",
  "summary": "Crash on Android when disposing SKCanvasView",
  "type": { "value": "bug", "confidence": 0.92, "reason": "Stack trace present" },
  "area": { "value": "SkiaSharp.Views", "confidence": 0.85, "reason": "SKCanvasView is in Views" },
  "backends": null, "platforms": null, "tenets": null, "partner": null,
  "regression": null, "fixStatus": null,
  "bugSignals": {
    "hasCrash": true, "hasStackTrace": true, "reproQuality": "partial",
    "hasScreenshot": false, "hasWorkaround": false, "workaroundSummary": null,
    "targetFrameworks": ["net8.0-android"],
    "severity": "high", "severityReason": "Crash with no workaround"
  },
  "reproEvidence": null, "versionAnalysis": null,
  "actionability": { "suggestedAction": "needs-investigation", "confidence": 0.80, "reason": "Real bug with stack trace" },
  "suggestedResponse": null,
  "analysisNotes": {
    "summary": "Crash in SKCanvasView disposal on Android. Stack trace points to native memory access after the surface was released. Classified as bug because the reporter describes working code that now crashes.",
    "keySignals": [
      { "text": "ObjectDisposedException at SKCanvasView.OnDetachedFromWindow", "source": "stack-trace", "interpretation": "Native surface accessed after disposal — use-after-free pattern", "supportedFields": ["type", "area", "bugSignals.severity"] },
      { "text": "works on iOS, crashes on Android only", "source": "body", "interpretation": "Platform-specific disposal timing issue", "supportedFields": ["platforms"] }
    ],
    "fieldRationales": [
      { "field": "type", "chosen": "bug", "expandedReason": "Reporter describes a crash with stack trace during a normal lifecycle event (view detachment). This is clearly broken behavior, not a usage question.", "alternatives": [{ "value": "question", "whyRejected": "Not asking how to do something — reporting a crash." }] },
      { "field": "area", "chosen": "SkiaSharp.Views", "expandedReason": "SKCanvasView is in the Views package. The crash occurs in view lifecycle methods, not core SkiaSharp APIs.", "alternatives": [{ "value": "SkiaSharp", "whyRejected": "Crash is in view disposal, not core drawing." }] },
      { "field": "bugSignals.severity", "chosen": "high", "expandedReason": "Hard crash with no workaround. Occurs during normal view lifecycle — not an edge case." }
    ],
    "docsConsulted": [
      { "path": "documentation/packages.md", "relevance": "Confirmed SKCanvasView is in SkiaSharp.Views package", "usedFor": ["area"] }
    ],
    "docsNotConsulted": "No native loading diagnostics needed — this is a managed disposal crash, not a DllNotFoundException.",
    "uncertainties": ["Unclear if this is specific to Android 14+ or affects all Android versions", "Don't know if the same crash occurs with SKGLView (GPU) vs SKCanvasView (raster)"],
    "assumptions": ["Assumed the reporter is using the latest stable NativeAssets.Android since no version conflict was mentioned"]
  },
  "resolutionAnalysis": {
    "hypothesis": "Android detaches views on a different thread than iOS, causing the native surface to be freed before the managed disposal completes.",
    "researchDone": ["Checked SkiaSharp.Views Android source for disposal pattern", "Searched for similar Android lifecycle crash issues"],
    "proposals": [
      { "title": "Guard disposal with null check", "description": "Add null/disposed check before accessing native surface in OnDetachedFromWindow.", "steps": ["Add IsDisposed check in SKCanvasView.OnDetachedFromWindow", "Run Android lifecycle tests"], "pros": ["Simple fix", "Low risk"], "cons": ["May mask deeper lifecycle issue"], "confidence": 0.75, "effort": "low" },
      { "title": "Synchronize disposal with UI thread", "description": "Post disposal to the UI thread to ensure surface is still valid.", "steps": ["Wrap surface release in RunOnUiThread", "Add thread-safety tests"], "pros": ["Addresses root cause"], "cons": ["May introduce subtle timing issues"], "confidence": 0.70, "effort": "medium" },
      { "title": "Weak reference to native surface", "description": "Hold a weak reference to prevent use-after-free.", "steps": ["Replace strong reference with weak reference", "Add null check before use", "Test across Android versions"], "pros": ["Prevents crash definitively"], "cons": ["More invasive change", "Needs careful testing"], "confidence": 0.65, "effort": "medium" }
    ],
    "recommendedProposal": "Guard disposal with null check",
    "recommendedReason": "Lowest risk, addresses the immediate crash. Can be followed up with a deeper fix if the lifecycle issue recurs."
  }
}
```

**Question** (bugSignals null, resolutionAnalysis null, analysisNotes still required):

```json
{
  "schemaVersion": "1.0",
  "number": 5678,
  "repo": "mono/SkiaSharp",
  "analyzedAt": "2026-02-08T15:00:00Z",
  "summary": "How to load custom fonts in SkiaSharp on Linux",
  "type": { "value": "question", "confidence": 0.90, "reason": "Asking how to do something" },
  "area": { "value": "SkiaSharp", "confidence": 0.80, "reason": "Core font loading API" },
  "backends": null, "platforms": null, "tenets": null, "partner": null,
  "regression": null, "fixStatus": null, "bugSignals": null,
  "reproEvidence": null, "versionAnalysis": null,
  "actionability": {
    "suggestedAction": "close-with-docs", "confidence": 0.85,
    "reason": "Answered by existing documentation", "closeable": true,
    "closeReason": "See SKTypeface.FromFile() docs"
  },
  "suggestedResponse": {
    "responseType": "documentation", "confidence": 0.85,
    "reason": "Direct answer exists in API docs",
    "draft": "Use `SKTypeface.FromFile()` or `SKTypeface.FromData()` to load custom fonts."
  },
  "analysisNotes": {
    "summary": "User is asking how to load custom fonts on Linux. This is a usage question, not a bug — the API exists and works.",
    "keySignals": [
      { "text": "How do I load a custom .ttf font?", "source": "body", "interpretation": "How-to question about existing API", "supportedFields": ["type"] }
    ],
    "fieldRationales": [
      { "field": "type", "chosen": "question", "expandedReason": "The reporter is asking how to accomplish a task. No broken behavior is described.", "alternatives": [{ "value": "documentation", "whyRejected": "The docs exist — user just hasn't found them. Better to point them there than open a docs issue." }] },
      { "field": "actionability.suggestedAction", "chosen": "close-with-docs", "expandedReason": "SKTypeface.FromFile() and FromData() are documented. Can answer directly and close." }
    ],
    "docsConsulted": [
      { "path": "docs/SkiaSharpAPI/SkiaSharp/SKTypeface.xml", "relevance": "Confirmed FromFile and FromData methods exist and are documented", "usedFor": ["type", "actionability"] }
    ]
  },
  "resolutionAnalysis": {
    "hypothesis": "User wants to render text with a custom .ttf font on Linux, where system fonts may not be available or fontconfig may not be installed.",
    "researchDone": ["Checked SKTypeface API docs for font loading methods", "Reviewed Linux package selection guide in packages.md"],
    "proposals": [
      { "title": "Use SKTypeface.FromFile()", "description": "Load the font directly from a file path at runtime.", "steps": ["Bundle the .ttf file with the application", "Call SKTypeface.FromFile(\"/path/to/font.ttf\")", "Pass the typeface to SKPaint.Typeface"], "pros": ["Simplest approach", "No dependencies"], "cons": ["Requires knowing the file path at runtime"], "confidence": 0.90, "effort": "low" },
      { "title": "Use SKTypeface.FromData() with embedded resource", "description": "Embed the font as a resource and load from a byte array.", "steps": ["Add .ttf as an embedded resource in the project", "Read the resource stream into SKData", "Call SKTypeface.FromData(skData)"], "pros": ["Font travels with the assembly", "No file path concerns"], "cons": ["Slightly more code"], "confidence": 0.90, "effort": "low" },
      { "title": "Use SKFontManager with NativeAssets.Linux", "description": "Install fontconfig and use system font enumeration.", "steps": ["Use SkiaSharp.NativeAssets.Linux instead of NoDependencies", "Install fontconfig in the environment", "Use SKFontManager.Default to enumerate system fonts"], "pros": ["Access to all system fonts", "Standard Linux font stack"], "cons": ["Requires fontconfig dependency", "Not suitable for minimal containers"], "confidence": 0.75, "effort": "medium" }
    ],
    "recommendedProposal": "Use SKTypeface.FromFile()",
    "recommendedReason": "Simplest and most portable approach. Works on all Linux environments regardless of fontconfig availability."
  }
}
```

#### Key rules

- **`schemaVersion`** must be `"1.0"`
- **`analyzedAt`** must be full ISO 8601: `"2026-02-08T15:00:00Z"` (include T, time, and Z)
- **Every classification** needs `value`, `confidence` (0.0–1.0 as decimal), and `reason`
- **Label-backed fields** use exact label suffixes from the schema enums: `prefix/ + value` = GitHub label.
- **Single-value fields** (`type`, `area`, `partner`): pick ONE best fit
- **Multi-value fields** (`backends`, `platforms`, `tenets`): include all that apply, `minItems: 1`
- **Nullable sections**: use JSON `null` — never empty object `{}` or empty array `[]`
- **`bugSignals`**: REQUIRED for `type.value == "bug"`, must be `null` for non-bugs (schema enforces this)
- **`reproEvidence`**: extract ALL screenshots, zip files, repo links, code snippets, steps to reproduce — preserve every URL for future AI debugging
- **`duplicateOf`**: REQUIRED when `suggestedAction == "close-as-duplicate"` (schema enforces this)
- **`close-as-stale`** is not a valid action (not in enum — schema rejects it)
- **`suggestedResponse.draft`**: inline markdown, max 2000 chars. Follow the tone and structure rules below.
- **`analysisNotes`**: REQUIRED on every triage. Explain your thinking for every non-null classified field. Include direct quotes as key signals, per-field rationales with alternatives considered, docs consulted, and uncertainties. This is how a human reviewer traces your logic.
  - **`keySignals`**: Direct quotes or close paraphrases from the issue. Each must name its source and which fields it supports.
  - **`fieldRationales`**: One entry for every non-null classified field (type, area, platforms, tenets, severity, suggestedAction, etc). Include `alternatives` when the choice wasn't obvious.
  - **`docsConsulted`**: Every doc you actually read, with why it was relevant.
  - **`docsNotConsulted`**: Explain why potentially-relevant docs were skipped (e.g. "No GPU signals so didn't check rendering docs"). Use `null` only if all relevant docs were checked.
  - **`uncertainties`**: What's still unclear and what info would resolve it. These flag where human review is most valuable.
  - **`assumptions`**: Explicit assumptions made when evidence was insufficient. Omit the field entirely if no assumptions were needed.
- **`resolutionAnalysis`**: Should be populated for ALL issue types — the goal is to give the maintainer ready-to-use options to help the developer quickly. Null only for duplicates or truly abandoned issues. Must have 3+ proposals.
  - **Bugs**: Fix proposals with steps, tradeoffs, and effort.
  - **Questions**: 3+ possible answers with code examples or doc pointers.
  - **Feature/enhancement requests**: Existing alternatives, workarounds, or approaches to achieve the goal.
  - **Documentation gaps**: Draft answers or sample code the maintainer can use directly.

#### Response tone & structure

Drafts follow a three-part structure: **acknowledge → analyze → ask**.

1. **Acknowledge** — Recognize what the reporter did well (detailed logs, repro steps, workaround). Be genuine, not effusive.
2. **Analyze** — Give your technical read of the situation. Be direct and specific. This is the value we're providing.
3. **Ask** (when needed) — If more info is required, ask respectfully. Remember the reporter may be under pressure, up against deadlines, or deep in other work. Asking them to do more means asking them to stop what they're doing. Frame requests as questions ("Would you be able to..."), not commands ("Run ldd on..."). Don't over-explain why you need it — trust them to understand.

**Tone rules:**
- Empathetic, direct, technical. Not lecturing, not buddy-buddy.
- Write as a genuine attempt to understand the problem, not a knowledge dump.
- Avoid "here's how it works" explanations that talk down to the reporter.
- People may suspect AI wrote this — don't give them reasons to be sure. No emoji, no "Great question!", no forced enthusiasm.
- **URLs**: must start with `https://` or `http://` (no javascript:, data:, file: schemes)

#### Confidence scoring

Use the schema's `$defs.confidence.description` for the scale (0.95+=explicit, 0.80-0.94=strong inference, 0.60-0.79=ambiguous, <0.40=guessing). Set `requiresHumanReview: true` if any key confidence (`type`, `area`, `partner`, `actionability`, or any item in `backends`/`platforms`/`tenets`) is below 0.70.

#### reproEvidence extraction

This section is critical for future AI debugging. Extract from the preprocessed markdown:

- **screenshots**: Every image URL with a `context` describing what the *surrounding text says* about the image (not what you think the image contains — you cannot see images)
- **attachments**: Every `.zip`, `.rar`, etc. with filename and download URL
- **repoLinks**: GitHub repository URLs shared as repro projects
- **relatedIssues**: Issue numbers referenced in the thread
- **stepsToReproduce**: Array of step strings synthesized from across all comments, not just the issue body
- **codeSnippets**: Extract code blocks with language and context
- **environmentDetails**: Consolidate OS, IDE, .NET version, device info

---

## Phase 4 — Validate

Schema validation before persisting. Write JSON to `/tmp/triage-{number}.json` first.

### 1. Run validator

```bash
pwsh .github/skills/triage-issue/scripts/validate-triage.ps1 /tmp/triage-{number}.json
```

The validator checks against `references/triage-schema.json` (JSON Schema draft 2020-12). It catches:
- Invalid enum values (including close-as-stale)
- Missing required fields
- Confidence out of range
- Cross-field violations (bugSignals on non-bugs, missing duplicateOf)
- Unknown fields (additionalProperties: false)
- Invalid date format

### 2. Handle failures

| Exit code | Meaning | Action |
|-----------|---------|--------|
| 0 | Valid | Proceed to Phase 5 |
| 1 | Schema violations | Read errors, fix JSON, re-validate (max 3 attempts) |
| 2 | Environment error | Stop and report to user |

---

## Phase 5 — Persist & Present

Write the validated triage, show the summary, and push.

### 1. Write to cache

```bash
mkdir -p $CACHE/ai-triage/
cp /tmp/triage-{number}.json $CACHE/ai-triage/{number}.json
```

**Re-triage:** If `ai-triage/{number}.json` already exists, the new analysis **replaces** it. Check the existing file's `analyzedAt` — if the issue hasn't changed since, confirm with the user first. In non-interactive mode, replace without confirmation when the issue's `updatedAt` is newer than `analyzedAt`; otherwise skip with a note.

### 2. Present summary

```
✅ Triage written: ai-triage/{number}.json

Type:       bug (0.98)
Area:       SkiaSharp
Platforms:  Windows-Classic, Android
Tenets:     reliability, compatibility
Severity:   critical — Fatal crash in disposal path
Regression: Yes (2.88.6 → 3.0.0-preview2.1)
Action:     needs-investigation
Evidence:   6 screenshots, 3 code snippets, 0 attachments
```

If `suggestedResponse.draft` exists, show it in a copy-paste block:

```
📝 Draft response (⚠️ requires human review before posting):

> [draft content]
```

**⚠️ NEVER post draft responses automatically via GitHub API.** Always present to the user for review.

### 3. Push to remote

```bash
cd .data-cache
git restore --staged :/
git add repos/mono-SkiaSharp/ai-triage/{number}.json
git --no-pager diff --cached --name-only
git commit -m "ai-triage: classify #{number}"
git push
cd ..
```

### 4. Handle push conflicts

The cache branch is updated frequently. If `git push` fails with a non-fast-forward error:

**Attempt rebase** (up to 3 tries):

```bash
cd .data-cache
git pull --rebase origin docs-data-cache
git push
cd ..
```

**If rebase hits a merge conflict**: abort (`git rebase --abort`), ask the user whether to keep **local** or **remote**, then re-apply:

```bash
# Keep our triage (local):
git pull --rebase -X theirs origin docs-data-cache && git push

# Keep remote (discard ours):
git pull --rebase -X ours origin docs-data-cache && git push
```

> **Note:** `-X theirs` during rebase keeps *our* changes (confusing but correct — during rebase, "theirs" is the branch being rebased, i.e., our local commits). `-X ours` keeps the remote version.

If push still fails after 3 attempts, surface the error to the user and do not claim success.

---

## References

- **`references/triage-schema.json`** — Self-documenting JSON Schema (draft 2020-12) with descriptions on every field, all enums, and cross-field rules
- **`references/labels.md`** — Label taxonomy, cardinality rules, classification tips
- **`references/native-loading-diagnostics.md`** — How to diagnose DllNotFoundException on Linux: distinguishing "file not found" from "wrong binary deployed"
- **`documentation/packages.md`** — Complete NuGet package reference: purpose, contents, loading mechanism, deployment guidance

## Scripts

All scripts are PowerShell 7.5+ — only `pwsh` needs to be installed.

- **`scripts/issue-to-markdown.ps1 <file.json>`** — Preprocess cached issue JSON into annotated markdown. Preserves all URLs. Supports pipeline: `Get-Content issue.json -Raw | pwsh scripts/issue-to-markdown.ps1`
- **`scripts/validate-triage.ps1 <triage.json>`** — Validate triage JSON against the schema using `Test-Json` (draft 2020-12). Reports all errors for AI self-correction.
- **`scripts/get-labels.ps1 [prefix/] [-Json]`** — Maintenance utility to fetch live label values from GitHub (not used during triage — schema enums are the source of truth). Requires `gh` CLI.
