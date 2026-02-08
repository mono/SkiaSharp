---
name: triage-issue
description: Triage a SkiaSharp GitHub issue by analyzing it and producing a structured JSON classification. Use when the user says "triage #123", "triage issue 123", or asks to classify/analyze a specific issue. Fetches the issue from GitHub, classifies it across multiple dimensions (type, area, platform, severity, etc.), validates against a JSON Schema, and writes a per-issue JSON file to the data cache.
---

# Triage Issue

Analyze a SkiaSharp GitHub issue and produce a structured, schema-validated triage JSON file.

## Pipeline

```
Fetch Issue → Preprocess to Markdown → Analyze → Generate JSON → Validate → Fix → Write
```

## Step 1: Fetch and Preprocess

### 1a. Fetch the issue

Use GitHub MCP tools. Default repo is `mono/SkiaSharp`:

```
github-mcp-server-issue_read(method: "get", owner: "mono", repo: "SkiaSharp", issue_number: N)
github-mcp-server-issue_read(method: "get_comments", owner: "mono", repo: "SkiaSharp", issue_number: N, perPage: 100)
github-mcp-server-issue_read(method: "get_labels", owner: "mono", repo: "SkiaSharp", issue_number: N)
```

For issues with 100+ comments, paginate to get all of them.

If the user specifies a different repo (e.g. "triage mono/SkiaSharp.Extended#45"), adjust owner/repo.

### 1b. Preprocess to annotated markdown

If the issue is available as a cached JSON file, run the preprocessor:

```bash
python3 .github/skills/triage-issue/scripts/issue-to-markdown.py {CACHE_PATH}/repos/{owner}-{repo}/github/items/{number}.json
```

The preprocessor produces annotated markdown with:
- **Author role tags**: `[OP]`, `[MEMBER]`, `[CONTRIBUTOR]`, `[BOT]`
- **Time-deltas** between comments: `(+2d)`, `(+6mo)`
- **Attachments & Links table**: all screenshots, zip files, repo links, related issues
- **Bot comments collapsed** to single summary lines
- **All URLs preserved** — image URLs, zip downloads, repo links kept intact

If no cached JSON exists, read the issue data from the MCP response directly.

### 1c. Fetch live label values

```bash
bash .github/skills/triage-issue/scripts/get-labels.sh
bash .github/skills/triage-issue/scripts/get-labels.sh area/
bash .github/skills/triage-issue/scripts/get-labels.sh os/
bash .github/skills/triage-issue/scripts/get-labels.sh backend/
```

Use the **exact values returned** — these are the source of truth for label-backed fields.

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
- **Label-backed fields** use exact label suffixes: `prefix/ + value` = GitHub label. If a live label exists that isn't in the schema enum, use the closest match and note it in the reason.
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

Set `requiresHumanReview: true` if ANY top-level confidence is below 0.70.

### reproEvidence extraction

This section is critical for future AI debugging. Extract from the preprocessed markdown:

- **screenshots**: Every image URL with a `context` describing what the *surrounding text says* about the image (not what you think the image contains — you cannot see images)
- **attachments**: Every `.zip`, `.rar`, etc. with filename and download URL
- **repoLinks**: GitHub repository URLs shared as repro projects
- **relatedIssues**: Issue numbers referenced in the thread
- **stepsToReproduce**: Synthesize from across all comments, not just the issue body
- **codeSnippets**: Extract code blocks with language and context
- **environmentDetails**: Consolidate OS, IDE, .NET version, device info

## Step 3: Validate

Write JSON to a temp file and validate BEFORE writing to cache:

```bash
python3 .github/skills/triage-issue/scripts/validate-triage.py /tmp/triage-{number}.json
```

The validator checks against `references/triage-schema.json` (JSON Schema draft 2020-12). It catches:
- Invalid enum values (including close-as-stale)
- Missing required fields
- Confidence out of range
- Cross-field violations (bugSignals on non-bugs, missing duplicateOf)
- Unknown fields (additionalProperties: false)
- Invalid date format

**If validation fails**: read the error messages, fix the JSON, and re-validate. **Maximum 3 attempts** — if still failing after 3 tries, present the errors to the user and stop.

## Step 4: Write

Determine the cache path:

```bash
if [ -d ".data-cache" ]; then
  CACHE_PATH=".data-cache"
else
  echo "❌ Cache worktree not found. Run: git worktree add .data-cache docs-data-cache"
  exit 1
fi
```

**Stop if cache path is missing** — do not proceed without a valid path.

Derive triage directory: `{CACHE_PATH}/repos/{owner}-{repo}/ai-triage/`

```bash
mkdir -p {CACHE_PATH}/repos/{owner}-{repo}/ai-triage/
mv /tmp/triage-{number}.json {CACHE_PATH}/repos/{owner}-{repo}/ai-triage/{number}.json
```

### Re-triage

If `ai-triage/{number}.json` already exists, the new analysis **replaces** it. Check the existing file's `analyzedAt` — if the issue hasn't changed since, confirm with the user first.

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

## Step 6: Push (Optional)

Only if the user explicitly says "push" or "commit":

```bash
cd {CACHE_PATH}
git restore --staged :/
git add repos/{owner}-{repo}/ai-triage/{number}.json
# Verify only the triage file is staged
git diff --cached --name-only
git commit -m "ai-triage: classify #{number}"
git push
```

## References

- **`references/triage-schema.json`** — Self-documenting JSON Schema (draft 2020-12) with descriptions on every field, all enums, and cross-field rules
- **`references/labels.md`** — Label taxonomy, cardinality rules, classification tips

## Scripts

- **`scripts/issue-to-markdown.py <file.json>`** — Preprocess cached issue JSON into annotated markdown. Preserves all URLs. Supports stdin: `cat issue.json | issue-to-markdown.py -`
- **`scripts/validate-triage.py <triage.json>`** — Validate triage JSON against the schema. Reports errors for AI self-correction.
- **`scripts/get-labels.sh [prefix/]`** — Fetch live label values from GitHub (cached 10 min). Use `--no-cache` to refresh.
