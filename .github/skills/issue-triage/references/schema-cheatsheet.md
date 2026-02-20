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
| **classifiedType** | `type/bug`, `type/feature-request`, `type/question`, `type/enhancement`, `type/documentation` |
| **classifiedArea** | `area/Build`, `area/Docs`, `area/HarfBuzzSharp`, `area/libHarfBuzzSharp.native`, `area/libSkiaSharp.native`, `area/SkiaSharp`, `area/SkiaSharp.HarfBuzz`, `area/SkiaSharp.Views`, `area/SkiaSharp.Views.Blazor`, `area/SkiaSharp.Views.Forms`, `area/SkiaSharp.Views.Maui`, `area/SkiaSharp.Views.Uno`, `area/SkiaSharp.Workbooks` |
| **classifiedPlatform** | `os/Android`, `os/iOS`, `os/macOS`, `os/Linux`, `os/Windows-Classic`, `os/Windows-WinUI`, `os/Windows-Universal-UWP`, `os/Windows-Nano-Server`, `os/WASM`, `os/Tizen`, `os/tvOS`, `os/watchOS` |
| **classifiedBackend** | `backend/OpenGL`, `backend/Metal`, `backend/Vulkan`, `backend/Raster`, `backend/Direct3D`, `backend/PDF`, `backend/SVG`, `backend/XPS` |
| **suggestedAction** | `needs-investigation`, `close-as-fixed`, `close-as-by-design`, `close-with-docs`, `close-as-duplicate`, `convert-to-discussion`, `request-info`, `keep-open` |
| **errorType** | `crash`, `exception`, `wrong-output`, `missing-output`, `performance`, `build-error`, `memory-leak`, `platform-specific`, `other` |
| **severity** | `critical`, `high`, `medium`, `low` |
| **reproQuality** | `complete`, `partial`, `none` |
| **currentRelevance** | `likely`, `unlikely`, `unknown` |
| **relevance** (codeInvestigation) | `direct`, `related`, `context` |
| **category** (proposals) | `workaround`, `fix`, `alternative`, `investigation` |
| **validated** (proposals) | `untested`, `yes`, `no` |
| **actionType** | `update-labels`, `add-comment`, `close-issue`, `convert-to-discussion`, `link-related`, `link-duplicate`, `update-project`, `set-milestone` |

## Output (required)

```json
"output": {
  "actionability": {
    "suggestedAction": "<suggestedAction enum>",
    "confidence": 0.0-1.0,
    "reason": "Why this action"
  },
  "missingInfo": ["What info is needed from reporter"],
  "actions": [
    { "type": "<actionType>", "description": "...", "risk": "low|medium|high", "confidence": 0.0-1.0, ... }
  ]
}
```

`missingInfo` is optional — include when reporter needs to provide more details (pair with `request-info` action).

### Action Types & Specific Fields

| Type | Risk | Required Specific Fields |
|------|------|--------------------------|
| `update-labels` | low | `labels` (array of strings) |
| `add-comment` | high | `comment` (markdown string). See `response-guidelines.md`. |
| `close-issue` | medium | — |
| `link-related` | low | `linkedIssue` (integer) |
| `link-duplicate` | medium | `linkedIssue` (integer) |
| `convert-to-discussion` | high | — |
| `update-project` | low | — |
| `set-milestone` | low | — |

## Common Mistakes

1. **`platforms`/`backends` as objects** — They're plain string arrays, NOT `{value, confidence}`
2. **Missing `meta.analyzedAt`** — Must be ISO 8601: `"2026-02-19T12:00:00Z"`
3. **Extra fields** — `additionalProperties: false` at every level. No extra keys allowed.
4. **Null values** — Omit optional fields entirely. Never set to `null`.
5. **Absolute paths** — Redact `/Users/name/` → `$HOME/`, `/tmp/...` → relative descriptions.
6. **`bugSignals` without type/bug** — Only include when `classification.type.value` is `"type/bug"`.
