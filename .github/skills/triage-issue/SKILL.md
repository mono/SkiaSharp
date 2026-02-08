---
name: triage-issue
description: Triage a SkiaSharp GitHub issue by analyzing it and producing a structured JSON classification. Use when the user says "triage #123", "triage issue 123", or asks to classify/analyze a specific issue. Reads issue data from the local data cache (docs-data-cache branch), classifies across multiple dimensions (type, area, platform, severity, etc.), validates against a JSON Schema, writes a per-issue triage JSON file back to the cache branch, and automatically pushes.
---

# Triage Issue

Analyze a SkiaSharp GitHub issue and produce a structured, schema-validated triage JSON file.

## Data Flow

All issue data is **pre-cached** on the `docs-data-cache` branch (synced hourly). Never call the GitHub API for issue data — it's already there.

```
Cache branch (worktree) → Preprocess → Analyze → Validate → Write back to cache branch → Push
```

## Step 0: Verify environment

### 0a. Check prerequisites

```bash
pwsh --version    # Requires 7.5+
```

### 0b. Ensure cache worktree exists and is current

The `docs-data-cache` branch must be checked out as a worktree at `.data-cache`:

```bash
# Verify worktree exists and is on correct branch
if [ ! -d ".data-cache" ]; then
    git worktree add .data-cache docs-data-cache
fi
# Verify branch
git -C .data-cache rev-parse --abbrev-ref HEAD  # Must be "docs-data-cache"
# Pull latest data
git -C .data-cache pull --rebase origin docs-data-cache
```

If the branch check fails, abort with a clear message. All paths below use `$CACHE` as shorthand:

```bash
CACHE=".data-cache/repos/mono-SkiaSharp"
```

### 0c. Ensure docs worktree exists

The `docs` branch contains conceptual documentation (tutorials, guides). Check it out as a read-only worktree at `.docs`:

```bash
if git show-ref --verify --quiet refs/heads/docs 2>/dev/null; then
    [ -d ".docs" ] || git worktree add .docs docs
fi
```

This worktree provides access to tutorials on drawing, paths, bitmaps, transforms, effects, and shaders. Useful for answering how-to questions and verifying expected behavior.

## Step 1: Preprocess

### 1a. Read the cached issue

The cached issue JSON is at: `$CACHE/github/items/{number}.json`

```bash
cat .data-cache/repos/mono-SkiaSharp/github/items/2794.json
```

If the file doesn't exist, the issue hasn't been cached yet. Tell the user: "Issue #{number} is not in the cache. It will be available after the next hourly sync." **Stop triage — do not call the GitHub API as a fallback.**

### 1b. Convert to annotated markdown

```bash
pwsh .github/skills/triage-issue/scripts/issue-to-markdown.ps1 $CACHE/github/items/{number}.json
```

If the script exits non-zero or emits parse errors, the cache file may be corrupted — tell the user to resync instead of continuing.

The preprocessor produces annotated markdown with:
- **Author role tags**: `[OP]`, `[MEMBER]`, `[CONTRIBUTOR]`, `[BOT]`
- **Time-deltas** between comments: `(+2d)`, `(+6mo)`
- **Attachments & Links table**: all screenshots, zip files, repo links, related issues
- **Bot comments collapsed** to single summary lines
- **All URLs preserved** — image URLs, zip downloads, repo links kept intact

### 1c. Review allowed values

The schema (`references/triage-schema.json`) is self-documenting — every enum field lists the exact allowed values. Label-backed fields (area, backend, platform, tenet, partner) use values that match GitHub label suffixes: `prefix/ + value` = full label.

### 1d. Research documentation sources

Before generating the triage JSON, consult available documentation to inform your analysis. This prevents misdiagnosis from missing domain knowledge.

**For every issue**, identify the key topics and check these sources in priority order:

1. **Package behavior** — read `documentation/packages.md` when the issue mentions NativeAssets packages, DllNotFoundException, container deployment, or WASM. This file documents what each package contains, how native loading works per platform, and common deployment issues.

2. **API docs** — search `docs/SkiaSharpAPI/*.xml` when specific SkiaSharp types or methods are mentioned. These XML files document expected behavior, parameters, and return values for every public API.

3. **Conceptual docs** — search `.docs/docs/docs/` (from Step 0c worktree) when the issue involves how-to questions about drawing, paths, bitmaps, transforms, effects, or shaders.

4. **MS Learn** — use the `mslearn-microsoft_docs_search` MCP tool when the issue involves non-SkiaSharp technologies: MAUI, Blazor WebAssembly, WPF, Windows Forms, containers/Docker, .NET runtime behavior, trimming, NuGet packaging, or P/Invoke mechanics. MS Learn is the authoritative source for platform-specific .NET behavior that SkiaSharp docs don't cover.

**Key principle:** Project-specific docs first (they override general .NET knowledge), MS Learn for external platform topics.

**For question/documentation issues:** The answer likely already exists in one of these sources. Check before drafting a response, and cite the source.

**For native loading issues** (DllNotFoundException, container, WASM): Always read `documentation/packages.md` — it explains the loading mechanism and dependencies for each NativeAssets package.

## Step 2: Analyze and Generate JSON

Read the human-readable schema docs to understand the output format and allowed values:

```bash
cat .github/skills/triage-issue/references/triage-schema.json
```

The schema is self-documenting — every property has a `description` field explaining its purpose and allowed values.

### Before generating JSON, reason first

Write a brief internal analysis (3-5 sentences):
1. What type is this issue? What's the key evidence?
2. What's the suggested action and why?
3. Is it a bug? If so, what's the severity and is there a workaround?

Then generate the JSON.

### Minimal valid example

```json
{
  "schemaVersion": "1.0",
  "number": 1234,
  "repo": "mono/SkiaSharp",
  "analyzedAt": "2026-02-08T15:00:00Z",
  "summary": "Crash on Android when disposing SKCanvasView",
  "type": { "value": "bug", "confidence": 0.92, "reason": "Stack trace present" },
  "bugSignals": { "reproQuality": "partial", "severity": "high", "severityReason": "Crash with no workaround" },
  "actionability": { "suggestedAction": "needs-investigation", "confidence": 0.80, "reason": "Real bug" }
}
```

### Key rules

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
- **`suggestedResponse.draft`**: inline markdown, max 2000 chars — keep it concise and actionable
- **URLs**: must start with `https://` or `http://` (no javascript:, data:, file: schemes)

### Confidence calibration guide

| Range | Meaning | Example |
|-------|---------|---------|
| 0.95+ | Explicit statement in issue text | User says "this is a bug", version explicitly stated |
| 0.80–0.94 | Strong inference from context | Crash + stack trace → bug; mentions "Android" → platform |
| 0.60–0.79 | Reasonable inference with ambiguity | Code suggests OpenGL but not stated explicitly |
| 0.40–0.59 | Uncertain, multiple plausible interpretations | Could be bug or question |
| < 0.40 | Guessing — flag for human review | No clear evidence for classification |

Set `requiresHumanReview: true` if the minimum confidence across these fields is below 0.70: `type.confidence`, `area.confidence`, `partner.confidence`, `actionability.confidence`, and each item's `confidence` in `backends`, `platforms`, `tenets` arrays.

### reproEvidence extraction

This section is critical for future AI debugging. Extract from the preprocessed markdown:

- **screenshots**: Every image URL with a `context` describing what the *surrounding text says* about the image (not what you think the image contains — you cannot see images)
- **attachments**: Every `.zip`, `.rar`, etc. with filename and download URL
- **repoLinks**: GitHub repository URLs shared as repro projects
- **relatedIssues**: Issue numbers referenced in the thread
- **stepsToReproduce**: Array of step strings synthesized from across all comments, not just the issue body
- **codeSnippets**: Extract code blocks with language and context
- **environmentDetails**: Consolidate OS, IDE, .NET version, device info

## Step 3: Validate

Write JSON to a temp file and validate BEFORE writing to cache:

```bash
# Use OS-agnostic temp path
pwsh -c '$tmp = Join-Path ([IO.Path]::GetTempPath()) "triage-{number}.json"; $tmp'
pwsh .github/skills/triage-issue/scripts/validate-triage.ps1 /tmp/triage-{number}.json
```

The validator checks against `references/triage-schema.json` (JSON Schema draft 2020-12). It catches:
- Invalid enum values (including close-as-stale)
- Missing required fields
- Confidence out of range
- Cross-field violations (bugSignals on non-bugs, missing duplicateOf)
- Unknown fields (additionalProperties: false)
- Invalid date format

### Validator exit codes

| Exit | Meaning | Agent action |
|------|---------|--------------|
| 0 | Valid | Proceed to Step 4 |
| 1 | Schema violations | Read errors, fix JSON, re-validate |
| 2 | Environment error (missing file, bad schema) | Stop and report to user |

**If validation fails**: read the error messages, fix the JSON, and re-validate. **Maximum 3 attempts** — if still failing after 3 tries, present the errors to the user and stop.

## Step 4: Write to cache branch

Write the validated triage file to the cache worktree (this is the `docs-data-cache` branch):

```bash
mkdir -p $CACHE/ai-triage/
cp /tmp/triage-{number}.json $CACHE/ai-triage/{number}.json
```

### Re-triage

If `ai-triage/{number}.json` already exists, the new analysis **replaces** it. Check the existing file's `analyzedAt` — if the issue hasn't changed since, confirm with the user first. In non-interactive mode, replace without confirmation when the issue's `updatedAt` is newer than `analyzedAt`; otherwise skip with a note.

## Step 5: Present Summary

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

**⚠️ NEVER post draft responses automatically via GitHub API.** Always present to the user for review. Verify any version numbers, issue references, and code samples before posting.

## Step 6: Push to cache branch

Always push after writing the triage file. This pushes **only** to the `docs-data-cache` branch:

```bash
cd .data-cache
git restore --staged :/
git add repos/mono-SkiaSharp/ai-triage/{number}.json
# Verify only the triage file is staged
git --no-pager diff --cached --name-only
git commit -m "ai-triage: classify #{number}"
git push
cd ..
```

### Push conflict retry

The cache branch is updated frequently by other processes. If `git push` fails with a non-fast-forward error:

1. **Rebase and retry** (up to 3 attempts):

```bash
cd .data-cache
git pull --rebase origin docs-data-cache
# If rebase succeeds (exit 0), retry push:
git push
cd ..
```

2. **If rebase hits a merge conflict**: stop the rebase (`git rebase --abort`), show the user which files conflict, and ask whether to keep the **local** (our new triage) or **remote** (incoming) version. Then re-apply accordingly:

```bash
# After user chooses "local" (keep our triage):
cd .data-cache
git pull --rebase -X theirs origin docs-data-cache
git push
cd ..

# After user chooses "remote" (discard our triage):
cd .data-cache
git pull --rebase -X ours origin docs-data-cache
git push
cd ..
```

> **Note:** `-X theirs` during rebase keeps *our* changes (confusing but correct — during rebase, "theirs" is the branch being rebased, i.e., our local commits). `-X ours` keeps the remote version.

3. **If push still fails after 3 attempts**, surface the error to the user and do not claim success.

## References

- **`references/triage-schema.json`** — Self-documenting JSON Schema (draft 2020-12) with descriptions on every field, all enums, and cross-field rules
- **`references/labels.md`** — Label taxonomy, cardinality rules, classification tips
- **`documentation/packages.md`** — Complete NuGet package reference: purpose, contents, loading mechanism, deployment guidance

## Documentation Sources (consult during Step 1d)

- **`docs/SkiaSharpAPI/`** — XML API reference for every public type and method (~300 files)
- **`.docs/docs/docs/`** — Conceptual docs worktree: tutorials on drawing, paths, bitmaps, transforms, effects, shaders
- **MS Learn MCP** — Non-SkiaSharp platform knowledge. Use `mslearn-microsoft_docs_search` for MAUI, Blazor, WPF, containers, WASM, .NET runtime topics.

## Scripts

All scripts are PowerShell 7.5+ — only `pwsh` needs to be installed.

- **`scripts/issue-to-markdown.ps1 <file.json>`** — Preprocess cached issue JSON into annotated markdown. Preserves all URLs. Supports pipeline: `Get-Content issue.json -Raw | pwsh scripts/issue-to-markdown.ps1`
- **`scripts/validate-triage.ps1 <triage.json>`** — Validate triage JSON against the schema using `Test-Json` (draft 2020-12). Reports all errors for AI self-correction.
- **`scripts/get-labels.ps1 [prefix/] [-Json]`** — Utility to fetch live label values from GitHub (not used during triage — schema enums are the source of truth). Requires `gh` CLI.
