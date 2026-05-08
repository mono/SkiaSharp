# Issue Triage Report — #2061

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T14:30:00Z |
| Type | type/enhancement (0.97 (97%)) |
| Area | area/SkiaSharp.Views (0.95 (95%)) |
| Suggested action | keep-open (0.90 (90%)) |

**Issue Summary:** Request to enable Nullable Reference Types (NRT) in the SkiaSharp.Views.Gtk3 project.

**Analysis:** The SkiaSharp.Views.Gtk3 project does not have <Nullable>enable</Nullable> in its csproj. The SKDrawingArea class contains nullable fields (pix, surface) without annotations, which would require nullable annotation when NRT is enabled. Other projects in the repo (Blazor, Maui, Direct3D) have already enabled NRT, so this is part of a systematic effort to annotate the entire codebase.

**Recommendations:** **keep-open** — Valid enhancement request consistent with ongoing NRT enablement campaign. Scope is well-defined (one project, two files).

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.Views |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2058 — Parent issue: Enable Nullable Reference Types (SkiaSharp.Views) – same NRT enablement campaign, already triaged

## Analysis

### Technical Summary

The SkiaSharp.Views.Gtk3 project does not have <Nullable>enable</Nullable> in its csproj. The SKDrawingArea class contains nullable fields (pix, surface) without annotations, which would require nullable annotation when NRT is enabled. Other projects in the repo (Blazor, Maui, Direct3D) have already enabled NRT, so this is part of a systematic effort to annotate the entire codebase.

### Rationale

This is a straightforward enhancement request to enable C# nullable reference types in SkiaSharp.Views.Gtk3. The project lacks the <Nullable>enable</Nullable> property. The code has nullable fields that would need annotation. It is part of a repository-wide NRT enablement campaign (see #2058). Action is keep-open as this is a valid enhancement that requires annotation work.

### Key Signals

- "Enable Nullable Reference Types (SkiaSharp.Views.Gtk3)" — **issue title** (Explicit request to enable NRT for this specific project.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.Gtk3/SkiaSharp.Views.Gtk3.csproj` | — | direct | No <Nullable>enable</Nullable> property present in the project file. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Gtk3/SKDrawingArea.cs` | — | direct | Fields 'pix' (ImageSurface) and 'surface' (SKSurface) are non-nullable reference type fields but are assigned null in FreeDrawingObjects. These require nullable annotations (?) once NRT is enabled. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SkiaSharp.Views.Blazor.csproj` | — | context | Has <Nullable>enable</Nullable> – confirms the pattern used in sibling projects. |

### Resolution Proposals

**Hypothesis:** Add <Nullable>enable</Nullable> to SkiaSharp.Views.Gtk3.csproj and annotate nullable fields in SKDrawingArea.cs with ? suffix.

1. **Enable NRT in Gtk3 project** — fix, cost/s, validated=untested
   - Add <Nullable>enable</Nullable> to the csproj and annotate pix and surface fields as nullable (ImageSurface? and SKSurface?).

**Recommended proposal:** proposal-1

**Why:** Small, contained change consistent with what other projects in the repo have already done.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.90 (90%) |
| Reason | Valid enhancement request consistent with ongoing NRT enablement campaign. Scope is well-defined (one project, two files). |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/enhancement, area/SkiaSharp.Views, os/Linux, tenet/compatibility | labels=type/enhancement, area/SkiaSharp.Views, os/Linux, tenet/compatibility |
| link-related | low | 0.95 (95%) | Link to parent NRT tracking issue #2058 (Enable Nullable Reference Types for SkiaSharp.Views) | linkedIssue=#2058 |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2061,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T14:30:00Z"
  },
  "summary": "Request to enable Nullable Reference Types (NRT) in the SkiaSharp.Views.Gtk3 project.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.95
    },
    "platforms": [
      "os/Linux"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2058",
          "description": "Parent issue: Enable Nullable Reference Types (SkiaSharp.Views) – same NRT enablement campaign, already triaged"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The SkiaSharp.Views.Gtk3 project does not have <Nullable>enable</Nullable> in its csproj. The SKDrawingArea class contains nullable fields (pix, surface) without annotations, which would require nullable annotation when NRT is enabled. Other projects in the repo (Blazor, Maui, Direct3D) have already enabled NRT, so this is part of a systematic effort to annotate the entire codebase.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Gtk3/SkiaSharp.Views.Gtk3.csproj",
        "finding": "No <Nullable>enable</Nullable> property present in the project file.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Gtk3/SKDrawingArea.cs",
        "finding": "Fields 'pix' (ImageSurface) and 'surface' (SKSurface) are non-nullable reference type fields but are assigned null in FreeDrawingObjects. These require nullable annotations (?) once NRT is enabled.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SkiaSharp.Views.Blazor.csproj",
        "finding": "Has <Nullable>enable</Nullable> – confirms the pattern used in sibling projects.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "Enable Nullable Reference Types (SkiaSharp.Views.Gtk3)",
        "source": "issue title",
        "interpretation": "Explicit request to enable NRT for this specific project."
      }
    ],
    "rationale": "This is a straightforward enhancement request to enable C# nullable reference types in SkiaSharp.Views.Gtk3. The project lacks the <Nullable>enable</Nullable> property. The code has nullable fields that would need annotation. It is part of a repository-wide NRT enablement campaign (see #2058). Action is keep-open as this is a valid enhancement that requires annotation work.",
    "resolution": {
      "hypothesis": "Add <Nullable>enable</Nullable> to SkiaSharp.Views.Gtk3.csproj and annotate nullable fields in SKDrawingArea.cs with ? suffix.",
      "proposals": [
        {
          "title": "Enable NRT in Gtk3 project",
          "description": "Add <Nullable>enable</Nullable> to the csproj and annotate pix and surface fields as nullable (ImageSurface? and SKSurface?).",
          "category": "fix",
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "proposal-1",
      "recommendedReason": "Small, contained change consistent with what other projects in the repo have already done."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.9,
      "reason": "Valid enhancement request consistent with ongoing NRT enablement campaign. Scope is well-defined (one project, two files).",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/enhancement, area/SkiaSharp.Views, os/Linux, tenet/compatibility",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp.Views",
          "os/Linux",
          "tenet/compatibility"
        ]
      },
      {
        "type": "link-related",
        "description": "Link to parent NRT tracking issue #2058 (Enable Nullable Reference Types for SkiaSharp.Views)",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 2058
      }
    ]
  }
}
```

</details>
