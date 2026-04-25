# Feature Scout Report Schema

JSON schema for the Skia Feature Scout report. The skill generates this JSON, then
`render-feature-scout.py` produces a standalone HTML dashboard.

## Top-Level Structure

```json
{
  "meta": { ... },
  "summary": { ... },
  "items": [ ... ],
  "hiddenApis": [ ... ],
  "deprecations": [ ... ],
  "performance": [ ... ],
  "nextSteps": [ ... ]
}
```

## `meta` — Audit Metadata

```json
{
  "date": "2026-04-25",
  "schemaVersion": "2.0",
  "mode": "full",
  "previousMilestone": 119,
  "currentMilestone": 133,
  "latestUpstreamMilestone": 148,
  "skiaSubmoduleCommit": "fb76f3dd84ca...",
  "releaseNotesSource": "https://raw.githubusercontent.com/google/skia/main/RELEASE_NOTES.md"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `date` | string | Yes | ISO date of the audit |
| `schemaVersion` | string | Yes | Always `"2.0"` |
| `mode` | string | Yes | `"full"` or `"windowed"` |
| `previousMilestone` | integer | No | Only for windowed mode |
| `currentMilestone` | integer | Yes | The milestone SkiaSharp is on |
| `latestUpstreamMilestone` | integer | Yes | Latest milestone in upstream |
| `skiaSubmoduleCommit` | string | No | mono/skia fork commit |
| `releaseNotesSource` | string | Yes | URL of release notes |

## `summary` — Overview Counts

```json
{
  "totalItems": 52,
  "full": 3,
  "partial": 1,
  "missing": 14,
  "actionNeeded": 5,
  "correctlyAbsent": 4,
  "notApplicable": 12,
  "hiddenApiCount": 8,
  "performanceCount": 3,
  "critical": 2,
  "high": 6,
  "medium": 8,
  "low": 10
}
```

## `items` — Individual Feature Items

Each item tracks a feature across its full lifecycle:

```json
{
  "id": 1,
  "name": "SkMesh / Custom Vertex Mesh Drawing",
  "category": "new_feature",
  "milestoneIntroduced": 106,
  "milestoneEnhanced": "110,117,119,120",
  "milestoneDeprecated": null,
  "milestoneRemoved": null,
  "skiaApi": "SkMesh::Make, SkMeshSpecification",
  "description": "Draw custom vertex meshes with user-defined attributes and varyings using SkSL.",
  "userValue": "Enables particle systems, deformable meshes, and custom geometry rendering.",
  "cApiStatus": "missing",
  "cApiFunction": null,
  "cApiFile": null,
  "csharpStatus": "missing",
  "csharpMethod": null,
  "csharpFile": null,
  "bindingStatus": "missing",
  "priority": "high",
  "notes": "HUGE for custom rendering. M106: initial. M120: child effects."
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | integer | Yes | Sequential identifier |
| `name` | string | Yes | Human-readable feature name |
| `category` | string | Yes | See category enum below |
| `milestoneIntroduced` | integer | Yes | When it first appeared |
| `milestoneEnhanced` | string | No | Comma-separated milestones with improvements |
| `milestoneDeprecated` | integer | No | When deprecated |
| `milestoneRemoved` | integer | No | When removed |
| `skiaApi` | string | Yes | Upstream Skia API name(s) |
| `description` | string | Yes | What changed technically |
| `userValue` | string | No | Why it matters to SkiaSharp users (plain language) |
| `cApiStatus` | string | Yes | `"present"`, `"missing"`, `"not_applicable"` |
| `cApiFunction` | string | No | C API function name if present |
| `cApiFile` | string | No | C API header file if present |
| `csharpStatus` | string | Yes | `"present"`, `"missing"`, `"not_applicable"` |
| `csharpMethod` | string | No | C# method/property name if present |
| `csharpFile` | string | No | C# file if present |
| `bindingStatus` | string | Yes | See binding status enum |
| `priority` | string | Yes | `"critical"`, `"high"`, `"medium"`, `"low"` |
| `notes` | string | No | Additional context |

### Category Enum

`new_feature`, `codec`, `image`, `image_filter`, `shader`, `color`, `canvas`, `path`, `font`,
`utility`, `performance`, `behavior_change`, `deprecation`

### Binding Status Enum

| Value | Description |
|-------|-------------|
| `full` | C API + C# wrapper both exist |
| `partial` | C API exists but C# wrapper missing (quick win) |
| `missing` | Neither C API nor C# wrapper |
| `action_needed` | SkiaSharp wraps deprecated/removed Skia API |
| `correctly_absent` | Skia removed; SkiaSharp never wrapped |
| `not_applicable` | Doesn't need a binding |

## `hiddenApis` — Discovered via C++ Header Scan

Features found in upstream C++ headers that are NOT mentioned in release notes:

```json
{
  "id": 1,
  "cppClass": "SkImage",
  "cppHeader": "include/core/SkImage.h",
  "cppMethod": "makeScaledToFit(SkISize)",
  "description": "Scale image to fit within given dimensions while preserving aspect ratio.",
  "cApiStatus": "missing",
  "csharpStatus": "missing",
  "priority": "medium",
  "notes": "Not in release notes but would be useful for thumbnail generation."
}
```

## `deprecations` — APIs Needing [Obsolete] Markers

```json
{
  "id": 1,
  "skiaApi": "SkTypeface::MakeFromName",
  "deprecatedSince": 120,
  "skiasharpApi": "SKTypeface.FromFamilyName()",
  "skiasharpFile": "SKTypeface.cs",
  "replacement": "SKFontManager.CreateTypeface()",
  "obsoleteMessage": "Use SKFontManager.CreateTypeface() instead. Deprecated in Skia m120."
}
```

## `performance` — Performance-Related Changes

```json
{
  "id": 1,
  "milestone": 124,
  "area": "shader",
  "description": "Perlin noise shaders now properly rotate when transformed. Raster performance significantly improved.",
  "impact": "Automatic speedup for apps using Perlin noise on raster surfaces.",
  "bindingActionNeeded": false
}
```

## `nextSteps` — Prioritized Action Items

```json
{
  "priority": 1,
  "severity": "critical",
  "action": "Create SKPathBuilder bindings",
  "milestone": 143,
  "reason": "SkPath going immutable. All edit methods will be removed.",
  "skillToUse": "add-api",
  "effort": "large"
}
```

| Field | Type | Required |
|-------|------|----------|
| `priority` | integer | Yes (1 = highest) |
| `severity` | string | Yes |
| `action` | string | Yes |
| `milestone` | integer | No |
| `reason` | string | Yes |
| `skillToUse` | string | No (`add-api`, `issue-fix`, `update-skia`, `native-dependency-update`) |
| `effort` | string | No (`trivial`, `small`, `medium`, `large`) |
