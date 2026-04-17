# Release Notes Audit Report Schema

JSON schema for the release notes audit report. The AI generates this JSON as structured output, then `render-release-notes-audit.py` injects it into `viewer.html` to produce a standalone HTML report.

## Top-Level Structure

```json
{
  "meta": { ... },
  "summary": { ... },
  "items": [ ... ],
  "deprecations": [ ... ],
  "nextSteps": [ ... ]
}
```

## `meta` — Audit Metadata

```json
{
  "date": "2026-04-15",
  "schemaVersion": "1.0",
  "previousMilestone": 119,
  "currentMilestone": 133,
  "latestUpstreamMilestone": 148,
  "skiaSubmoduleCommit": "0c28aa73beebf4747af531a893b4e8d94690f5cf",
  "releaseNotesSource": "https://raw.githubusercontent.com/google/skia/main/RELEASE_NOTES.md"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `date` | string | Yes | ISO date of the audit |
| `schemaVersion` | string | Yes | Always `"1.0"` |
| `previousMilestone` | integer | Yes | The milestone SkiaSharp was on before this bump |
| `currentMilestone` | integer | Yes | The milestone SkiaSharp is now on |
| `latestUpstreamMilestone` | integer | Yes | Latest milestone in upstream release notes |
| `skiaSubmoduleCommit` | string | No | mono/skia fork commit if available |
| `releaseNotesSource` | string | Yes | URL where release notes were fetched |

## `summary` — Overview Counts

```json
{
  "totalItems": 52,
  "full": 3,
  "partial": 1,
  "missing": 14,
  "actionNeeded": 5,
  "correctlyAbsent": 12,
  "notApplicable": 17,
  "critical": 2,
  "high": 6,
  "medium": 8,
  "low": 10
}
```

| Field | Type | Description |
|-------|------|-------------|
| `totalItems` | integer | Total items cataloged |
| `full` | integer | Count with full binding |
| `partial` | integer | Count with partial binding |
| `missing` | integer | Count with no binding |
| `actionNeeded` | integer | Count needing `[Obsolete]` or other fix |
| `correctlyAbsent` | integer | Removed by Skia, never wrapped (correct) |
| `notApplicable` | integer | Internal/header-only changes |
| `critical` | integer | Critical priority items |
| `high` | integer | High priority items |
| `medium` | integer | Medium priority items |
| `low` | integer | Low priority items |

## `items` — Individual API Change Items

Array of objects, one per release notes entry:

```json
{
  "id": 1,
  "milestone": 133,
  "category": "new_feature",
  "skiaApi": "SkColorSpace::MakeCICP",
  "description": "Create SkColorSpace from CICP code points (Rec. ITU-T H.273)",
  "cApiStatus": "present",
  "cApiFunction": "sk_colorspace_new_cicp",
  "cApiFile": "sk_colorspace.h",
  "csharpStatus": "present",
  "csharpMethod": "SKColorSpace.CreateCicp()",
  "csharpFile": "SKColorSpace.cs",
  "bindingStatus": "full",
  "priority": "low",
  "notes": "Already fully bound"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | integer | Yes | Sequential identifier |
| `milestone` | integer | Yes | Skia milestone where this appeared |
| `category` | string | Yes | One of: `new_feature`, `deprecation`, `removal`, `behavior_change`, `header_reorg`, `internal` |
| `skiaApi` | string | Yes | Upstream Skia API name or description |
| `description` | string | Yes | What changed and why it matters |
| `cApiStatus` | string | Yes | `"present"`, `"missing"`, `"not_applicable"` |
| `cApiFunction` | string | No | C API function name if present |
| `cApiFile` | string | No | C API header file if present |
| `csharpStatus` | string | Yes | `"present"`, `"missing"`, `"not_applicable"` |
| `csharpMethod` | string | No | C# method/property name if present |
| `csharpFile` | string | No | C# file if present |
| `bindingStatus` | string | Yes | `"full"`, `"partial"`, `"missing"`, `"not_applicable"`, `"correctly_absent"`, `"action_needed"` |
| `priority` | string | Yes | `"critical"`, `"high"`, `"medium"`, `"low"`, `"none"` |
| `notes` | string | No | Additional context |

### `category` Values

| Value | Description |
|-------|-------------|
| `new_feature` | New public API added to Skia |
| `deprecation` | API marked deprecated in Skia |
| `removal` | API removed from Skia public surface |
| `behavior_change` | Existing API changed behavior silently |
| `header_reorg` | Headers moved/renamed |
| `internal` | Build system, backend-specific, or irrelevant to SkiaSharp |

### `bindingStatus` Values

| Value | Badge | Description |
|-------|-------|-------------|
| `full` | ✅ Full | C API + C# wrapper both exist |
| `partial` | 🟡 Partial | C API exists but C# incomplete, or vice versa |
| `missing` | ❌ Missing | No C API or C# wrapper |
| `not_applicable` | 🚫 N/A | Change doesn't need a binding |
| `correctly_absent` | 🚫 N/A | Skia removed this; SkiaSharp never wrapped it |
| `action_needed` | ⚠️ Action | SkiaSharp wraps deprecated/removed Skia API |

## `deprecations` — APIs Needing [Obsolete] Markers

Array of objects for APIs that Skia deprecated but SkiaSharp still exposes without `[Obsolete]`:

```json
{
  "id": 1,
  "skiaApi": "SkTypeface::MakeFromName",
  "deprecatedSince": 120,
  "skiasharpApi": "SKTypeface.FromFamilyName()",
  "skiasharpFile": "SKTypeface.cs",
  "replacement": "SKFontManager.CreateTypeface()",
  "obsoleteMessage": "Use SKFontManager.CreateTypeface() instead."
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | integer | Yes | Sequential identifier |
| `skiaApi` | string | Yes | Upstream Skia API that was deprecated |
| `deprecatedSince` | integer | Yes | Milestone when deprecated |
| `skiasharpApi` | string | Yes | SkiaSharp method/property name |
| `skiasharpFile` | string | Yes | C# file containing the API |
| `replacement` | string | No | What users should migrate to |
| `obsoleteMessage` | string | Yes | Suggested `[Obsolete("...")]` text |

## `nextSteps` — Prioritized Action Items

```json
{
  "priority": 1,
  "severity": "critical",
  "action": "Create SKPathBuilder bindings",
  "milestone": 143,
  "reason": "SkPath is going immutable. All edit methods will be removed.",
  "skillToUse": "add-api"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `priority` | integer | Yes | 1 = highest |
| `severity` | string | Yes | `"critical"`, `"high"`, `"medium"`, `"low"` |
| `action` | string | Yes | What to do |
| `milestone` | integer | No | Which Skia milestone drives this |
| `reason` | string | Yes | Why (cite impact) |
| `skillToUse` | string | No | Recommended SkiaSharp skill for follow-up |
