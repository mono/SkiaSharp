# Feature Scout Report Schema (v3)

JSON schema for the Skia Feature Scout report. One unified `findings` array instead of
separate arrays for items, hidden APIs, performance, and deprecations. The formal JSON Schema
is at `feature-scout-schema.json`.

## Top-Level Structure

```json
{
  "meta": { ... },
  "summary": { ... },
  "findings": [ ... ],
  "nextSteps": [ ... ]
}
```

## `meta` — Audit Metadata

```json
{
  "date": "2026-04-25",
  "schemaVersion": "3.0",
  "mode": "full",
  "previousMilestone": 119,
  "currentMilestone": 133,
  "latestUpstreamMilestone": 148,
  "skiaSubmoduleCommit": "fb76f3dd84ca...",
  "releaseNotesSource": "https://raw.githubusercontent.com/google/skia/main/RELEASE_NOTES.md"
}
```

## `summary` — Overview Counts

```json
{
  "totalFindings": 75,
  "byStatus": { "missing": 38, "full": 14, "partial": 3, "action_needed": 2, "not_applicable": 12, "correctly_absent": 6 },
  "byPriority": { "critical": 2, "high": 12, "medium": 25, "low": 36 },
  "bySource": { "release-notes": 55, "header-scan": 17, "binding-audit": 3 },
  "byLabel": { "quick-win": 3, "hidden-api": 17, "perf": 6, "behavior-change": 4, "action-needed": 2 }
}
```

## `findings` — Unified Array

Every finding (release notes features, hidden APIs, performance notes, deprecations) is in
one array with the same shape. Use `source` to know where it came from and `labels` for
filtering.

### Required fields (every finding)

| Field | Type | Description |
|-------|------|-------------|
| `name` | string | Human-readable name (used as identifier) |
| `category` | string | See category enum below |
| `priority` | string | `critical`, `high`, `medium`, `low` |
| `description` | string | What it is and why it matters |
| `bindingStatus` | string | See binding status enum below |
| `source` | string | `release-notes`, `header-scan`, or `binding-audit` |

### Optional fields

| Field | Type | When to include |
|-------|------|-----------------|
| `labels` | string[] | Freeform tags for filtering: `quick-win`, `hidden-api`, `perf`, `behavior-change`, `action-needed` |
| `milestone` | integer | Primary milestone (when introduced or relevant) |
| `milestones` | integer[] | Additional milestones where enhanced/changed |
| `milestoneDeprecated` | integer | When deprecated |
| `milestoneRemoved` | integer | When removed |
| `skiaApi` | string | Upstream Skia API name(s) |
| `userValue` | string | Why an app developer would want this |
| `notes` | string | Additional context |
| `cApiFunction` | string | C API function name if present |
| `cApiFile` | string | C API header file |
| `csharpMethod` | string | C# method/property name |
| `csharpFile` | string | C# file |
| `cppClass` | string | C++ class (for hidden APIs from header scan) |
| `cppHeader` | string | C++ header path |
| `cppMethod` | string | C++ method signature |
| `replacement` | string | Replacement API (for deprecations) |
| `obsoleteMessage` | string | Suggested `[Obsolete("...")]` text |
| `impact` | string | Impact description (for perf/behavior changes) |
| `bindingActionNeeded` | boolean | Whether binding work is needed (for perf notes) |
| `skillToUse` | string | Which skill to invoke for follow-up |
| `effort` | string | `trivial`, `small`, `medium`, `large` |

### Example: Release notes feature

```json
{
  "name": "SkMesh / Custom Vertex Mesh Drawing",
  "category": "new_feature",
  "priority": "high",
  "description": "Draw custom vertex meshes with user-defined attributes and varyings using SkSL.",
  "bindingStatus": "missing",
  "source": "release-notes",
  "milestone": 106,
  "milestones": [110, 117, 119, 120],
  "skiaApi": "SkMesh::Make, SkMeshSpecification",
  "userValue": "Enables particle systems, deformable meshes, and custom geometry rendering.",
  "skillToUse": "add-api",
  "effort": "large"
}
```

### Example: Hidden API from header scan

```json
{
  "name": "SkImage::reinterpretColorSpace",
  "category": "image",
  "priority": "medium",
  "description": "Re-tag image with new color space without converting pixels.",
  "bindingStatus": "missing",
  "source": "header-scan",
  "labels": ["hidden-api"],
  "cppClass": "SkImage",
  "cppHeader": "include/core/SkImage.h",
  "cppMethod": "reinterpretColorSpace(sk_sp<SkColorSpace>) const",
  "skillToUse": "add-api",
  "effort": "small"
}
```

### Example: Performance note

```json
{
  "name": "Perlin noise raster performance",
  "category": "performance",
  "priority": "low",
  "description": "Perlin noise shaders now properly rotate when transformed. Raster performance significantly improved.",
  "bindingStatus": "not_applicable",
  "source": "release-notes",
  "labels": ["perf"],
  "milestone": 124,
  "impact": "Automatic speedup for apps using Perlin noise on raster surfaces.",
  "bindingActionNeeded": false
}
```

### Example: Deprecation needing action

```json
{
  "name": "SkPath mutable methods deprecation",
  "category": "deprecation",
  "priority": "critical",
  "description": "SkPath is migrating to immutable. moveTo/lineTo/etc. will be removed.",
  "bindingStatus": "action_needed",
  "source": "release-notes",
  "labels": ["action-needed"],
  "milestone": 143,
  "skiaApi": "SkPath::moveTo, lineTo, quadTo, cubicTo, etc.",
  "csharpMethod": "SKPath.MoveTo(), LineTo(), etc.",
  "csharpFile": "SKPath.cs",
  "replacement": "SKPathBuilder (not yet bound)",
  "obsoleteMessage": "SkPath is becoming immutable. Use SKPathBuilder to create paths."
}
```

## Category Enum

`new_feature`, `codec`, `image`, `image_filter`, `shader`, `color`, `canvas`, `path`,
`font`, `utility`, `performance`, `behavior_change`, `deprecation`

## Binding Status Enum

| Value | Description |
|-------|-------------|
| `full` | C API + C# wrapper both exist |
| `partial` | C API exists but C# wrapper missing (quick win) |
| `missing` | Neither C API nor C# wrapper |
| `action_needed` | SkiaSharp wraps deprecated/removed Skia API |
| `correctly_absent` | Skia removed; SkiaSharp never wrapped |
| `not_applicable` | Doesn't need a binding |

## Source Enum

| Value | Description |
|-------|-------------|
| `release-notes` | Found in Skia RELEASE_NOTES.md |
| `header-scan` | Found by comparing C++ headers against C API |
| `binding-audit` | Found by auditing existing SkiaSharp bindings |

## `nextSteps` — Prioritized Action Plan

Separate from findings. Ordered list of what to do next.

```json
{
  "severity": "critical",
  "action": "Create SkPathBuilder bindings",
  "milestone": 143,
  "reason": "SkPath going immutable. All edit methods will be removed.",
  "skillToUse": "add-api",
  "effort": "large"
}
```
