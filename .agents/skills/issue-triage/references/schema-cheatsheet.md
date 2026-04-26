# Triage Schema Quick Reference

Read this BEFORE generating JSON. Full schema: `references/triage-schema.json`.

## Required Top-Level Fields

| Field | Type | Required Sub-fields |
|-------|------|-------------------|
| `meta` | object | `schemaVersion` ("1.0"), `number` (int), `repo` ("mono/SkiaSharp"), `analyzedAt` (ISO 8601) |
| `summary` | string | — (one-sentence description) |
| `classification` | object | `type` + `area` (see below) |
| `evidence` | object | — (no sub-fields required, but include what's relevant) |
| `analysis` | object | `summary` (string, required) |
| `output` | object | `actionability` + `actions` (see below) |

## Classification (required sub-objects)

```json
"classification": {
  "type": { "value": "<classifiedType>", "confidence": 0.0-1.0 },
  "area": { "value": "<classifiedArea>", "confidence": 0.0-1.0 },
  "platforms": ["<classifiedPlatform>", ...],   // optional, plain strings NOT objects
  "backends": ["<classifiedBackend>", ...],     // optional, plain strings NOT objects
  "tenets": ["<classifiedTenet>", ...],         // optional, plain strings NOT objects
  "partner": "<classifiedPartner>"              // optional, plain string NOT object
}
```

⚠️ `platforms`, `backends`, `tenets` are **plain string arrays** — NOT `{value, confidence}` objects.

## Evidence & Analysis Fields

| Field | Structure / Notes |
|-------|-------------------|
| `evidence.bugSignals` | `{ severity, regressionClaimed, errorType, errorMessage, stackTrace, reproQuality, targetFrameworks }`. Include ONLY for bugs. |
| `evidence.reproEvidence` | Extract ALL screenshots, attachments, code snippets, steps. Preserve every URL. |
| `evidence.versionAnalysis` | `{ mentionedVersions[], workedIn, brokeIn, currentRelevance, relevanceReason }`. Include when version info is mentioned. |
| `evidence.regression` | `{ isRegression, confidence, reason, workedInVersion, brokeInVersion }`. Include if regression signals exist. |
| `evidence.fixStatus` | `{ likelyFixed, confidence, reason, relatedPRs, relatedCommits, fixedInVersion }`. Include if fix evidence exists. |
| `analysis.codeInvestigation` | Array of `{ file, finding, relevance, lines? }`. **MANDATORY**: At least one for bugs, two for close-* actions. |
| `analysis.keySignals` | Array of `{ text, source, interpretation? }`. Structured evidence quotes. |
| `analysis.rationale` | Single string summarizing key classification decisions. |
| `analysis.workarounds` | Array of workaround strings found during triage. |
| `analysis.nextQuestions` | Array of open questions for repro/fix to investigate. |
| `analysis.errorFingerprint` | Normalized error fingerprint for cross-issue dedup (optional). |
| `analysis.resolution` | `{ hypothesis?, proposals[], recommendedProposal, recommendedReason }`. Omit for duplicates/abandoned. |

## Enum Values

| Enum | Values |
|------|--------|
| **classifiedType** | `type/bug`, `type/documentation`, `type/enhancement`, `type/feature-request`, `type/question` |
| **classifiedArea** | `area/Build`, `area/Docs`, `area/HarfBuzzSharp`, `area/SkiaSharp`, `area/SkiaSharp.HarfBuzz`, `area/SkiaSharp.Views`, `area/SkiaSharp.Views.Blazor`, `area/SkiaSharp.Views.Forms`, `area/SkiaSharp.Views.Maui`, `area/SkiaSharp.Views.Uno`, `area/SkiaSharp.Workbooks`, `area/libHarfBuzzSharp.native`, `area/libSkiaSharp.native` |
| **classifiedPlatform** | `os/Android`, `os/Linux`, `os/Tizen`, `os/WASM`, `os/Windows-Classic`, `os/Windows-Nano-Server`, `os/Windows-Universal-UWP`, `os/Windows-WinUI`, `os/iOS`, `os/macOS`, `os/tvOS`, `os/watchOS` |
| **classifiedBackend** | `backend/Direct3D`, `backend/Metal`, `backend/OpenGL`, `backend/PDF`, `backend/Raster`, `backend/SVG`, `backend/Vulkan`, `backend/XPS` |
| **suggestedAction** | `needs-info`, `needs-reproduction`, `needs-investigation`, `ready-to-fix`, `keep-open`, `close-as-fixed`, `close-as-duplicate`, `close-as-not-a-bug`, `close-as-external` |
| **errorType** | `crash`, `exception`, `memory-leak`, `build-error`, `wrong-output`, `missing-output`, `missing-api`, `performance`, `platform-specific`, `other` |
| **severity** | `critical`, `high`, `medium`, `low` |

### Severity rubric (bugs only)

| Severity | Criteria |
|----------|----------|
| `critical` | Crash/data loss affecting most users, no workaround |
| `high` | Crash or memory corruption possible, narrow trigger or workaround exists |
| `medium` | Wrong output, degraded behavior, workaround available |
| `low` | Cosmetic, edge case, minor inconvenience |

| **reproQuality** | `complete`, `partial`, `none` |
| **currentRelevance** | `likely`, `unlikely`, `unknown` |
| **relevance** (codeInvestigation) | `direct`, `related`, `context` |
| **category** (proposals) | `fix`, `workaround`, `alternative`, `investigation` |
| **validated** (proposals) | `untested`, `yes`, `no` |
| **effort** (proposals) | `cost/xs`, `cost/s`, `cost/m`, `cost/l`, `cost/xl` |
| **suggestedReproPlatform** | `linux`, `macos`, `windows` |
| **actionType** | `add-comment`, `close-issue`, `convert-to-discussion`, `link-duplicate`, `link-related`, `set-milestone`, `update-labels` |
| **actionRisk** | `low`, `medium`, `high` |
| **stateReason** | `completed`, `not_planned` |

### Choosing `suggestedAction` by issue type

| Type | Common suggestedAction | When |
|------|----------------------|------|
| `type/bug` | `needs-investigation` | Repro code provided, bug seems real |
| `type/bug` | `needs-info` | Missing repro, vague description |
| `type/bug` | `needs-reproduction` | Some info but no reliable minimal repro |
| `type/bug` | `close-as-not-a-bug` | Behavior is correct but confusing |
| `type/bug` | `close-as-external` | Root cause is in upstream Skia or another dependency |
| `type/bug` | `ready-to-fix` | Root cause is clear, fix path is known |
| `type/enhancement` | `needs-investigation` | Well-specified, has clear scope |
| `type/enhancement` | `keep-open` | Valid but low priority / needs design |
| `type/feature-request` | `needs-investigation` | Clear request with implementation path |
| `type/feature-request` | `keep-open` | Valid but requires design discussion |
| `type/question` | `close-as-not-a-bug` | Answer exists in docs or is by-design |
| `type/question` | `needs-info` | Need more context to answer |
| `type/documentation` | `needs-investigation` | Docs are missing or wrong |

**Distinguishing `needs-info` vs `needs-reproduction`:**
- Use `needs-info` when critical information is missing from the reporter (version, platform, repro steps, etc.).
- Use `needs-reproduction` when the issue description has enough context but no reliable minimal reproduction case.

**Empty-template heuristic:** If the code/repro sections contain only template placeholders or are completely empty, use `needs-info` regardless of type classification. A regression claim in the version fields upgrades priority even with sparse content.

## Output (required)

```json
"output": {
  "actionability": {
    "suggestedAction": "<suggestedAction enum>",
    "confidence": 0.0-1.0,
    "reason": "Why this action",
    "suggestedReproPlatform": "linux|macos|windows (optional)"
  },
  "missingInfo": ["What info is needed from reporter"],
  "actions": [
    { "type": "<actionType>", "description": "...", "risk": "low|medium|high", "confidence": 0.0-1.0, ... }
  ]
}
```

`missingInfo` is optional — include when reporter needs to provide more details (pair with `needs-info` action).

### Action Types & Required Fields

| Type | Risk | Required Fields |
|------|------|-----------------|
| `update-labels` | low | `labels` (array of label strings to apply) |
| `add-comment` | **dynamic** (see below) | `comment` (markdown string). See `response-guidelines.md`. |
| `close-issue` | medium | `stateReason` (`completed` or `not_planned`) |
| `link-related` | low | `linkedIssue` (integer issue number) |
| `link-duplicate` | medium | `linkedIssue` (integer issue number) |
| `convert-to-discussion` | high | `category` (discussion category name) |
| `set-milestone` | low | `milestone` (milestone title) |

#### `add-comment` Risk Calculation

Risk for `add-comment` is computed from the comment's content and the action's confidence score:

| Condition | Risk | Rationale |
|-----------|------|-----------|
| Factual only (repro findings, version matrix, link references) AND confidence ≥ 0.85 | **low** | Reporting observed facts — minimal reputation exposure |
| Includes workaround or technical suggestion AND confidence ≥ 0.85 | **medium** | Advice could be wrong, but evidence is strong |
| Suggests closing, rejects the issue, or states "by-design" (any confidence) | **high** | Telling a reporter their issue isn't valid always needs human review |
| Any content AND confidence < 0.70 | **high** | Not confident enough to speak for the maintainer |
| Default / everything else | **medium** | |

## Common Mistakes

1. **`platforms`/`backends` as objects** — They're plain string arrays, NOT `{value, confidence}`
2. **Missing `meta.analyzedAt`** — Must be ISO 8601: `"2026-02-19T12:00:00Z"`
3. **Extra fields** — `additionalProperties: false` at every level. No extra keys allowed.
4. **Null values** — Omit optional fields entirely. Never set to `null`.
5. **Absolute paths** — Redact `/Users/name/` → `$HOME/`, `/tmp/...` → relative descriptions.
6. **`bugSignals` without type/bug** — Only include when `classification.type.value` is `"type/bug"`.
