# Issue Triage Report — #2059

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T13:38:51Z |
| Type | type/enhancement (0.97 (97%)) |
| Area | area/SkiaSharp.Views (0.95 (95%)) |
| Suggested action | keep-open (0.88 (88%)) |

**Issue Summary:** Request to enable Nullable Reference Types (NRT) annotations in the SkiaSharp.Views.Desktop.Common project.

**Analysis:** The SkiaSharp.Views.Desktop.Common project currently does not have `<Nullable>enable</Nullable>` in its .csproj, while other projects like SkiaSharp.Views.Blazor and SkiaSharp.Views.Maui.Core already have NRT enabled. Enabling NRT would improve nullability safety for consumers of the Desktop views library and align it with the rest of the codebase.

**Recommendations:** **keep-open** — Valid enhancement aligned with existing NRT adoption in the codebase. Needs implementation work to annotate shared source files properly. No urgency but clear improvement.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.Views |
| Platforms | — |
| Backends | — |
| Tenets | tenet/reliability, tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

## Analysis

### Technical Summary

The SkiaSharp.Views.Desktop.Common project currently does not have `<Nullable>enable</Nullable>` in its .csproj, while other projects like SkiaSharp.Views.Blazor and SkiaSharp.Views.Maui.Core already have NRT enabled. Enabling NRT would improve nullability safety for consumers of the Desktop views library and align it with the rest of the codebase.

### Rationale

This is a straightforward enhancement request. The Desktop.Common project does not have NRT enabled while peer projects (Blazor, Maui.Core) do. No bug, no regression. The enhancement improves code quality and developer ergonomics for consumers. Suggested action is keep-open since it requires design work to annotate all shared source files without breaking API compatibility.

### Key Signals

- "Enable Nullable Reference Types (SkiaSharp.Views.Desktop.Common)" — **issue title** (Clear enhancement request to add NRT support to the Desktop.Common views package.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.Desktop.Common/SkiaSharp.Views.Desktop.Common.csproj` | — | direct | The project file does not contain a <Nullable>enable</Nullable> property, confirming NRT is not yet enabled. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SkiaSharp.Views.Blazor.csproj` | — | related | This project already has <Nullable>enable</Nullable>, showing the pattern is established in the codebase. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Shared/SKPaintSurfaceEventArgs.cs` | — | direct | Shared source file used by Desktop.Common has no nullable annotations (no ? or ! operators), consistent with NRT being disabled. |

### Resolution Proposals

**Hypothesis:** Add `<Nullable>enable</Nullable>` to SkiaSharp.Views.Desktop.Common.csproj and annotate all shared source files compiled into this target with appropriate nullable annotations.

1. **Enable NRT in Desktop.Common project** — fix, cost/m, validated=untested
   - Add `<Nullable>enable</Nullable>` to the csproj and systematically annotate shared source files (SKPaintSurfaceEventArgs.cs, SKPaintGLSurfaceEventArgs.cs, Extensions.cs) with nullable annotations. Ensure no new warnings break the build.

**Recommended proposal:** proposal-1

**Why:** Direct implementation of the requested feature, consistent with how other views projects have already adopted NRT.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.88 (88%) |
| Reason | Valid enhancement aligned with existing NRT adoption in the codebase. Needs implementation work to annotate shared source files properly. No urgency but clear improvement. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/enhancement, area/SkiaSharp.Views, tenet/reliability, tenet/compatibility | labels=type/enhancement, area/SkiaSharp.Views, tenet/reliability, tenet/compatibility |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2059,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T13:38:51Z"
  },
  "summary": "Request to enable Nullable Reference Types (NRT) annotations in the SkiaSharp.Views.Desktop.Common project.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/reliability",
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [],
      "attachments": [],
      "repoLinks": []
    }
  },
  "analysis": {
    "summary": "The SkiaSharp.Views.Desktop.Common project currently does not have `<Nullable>enable</Nullable>` in its .csproj, while other projects like SkiaSharp.Views.Blazor and SkiaSharp.Views.Maui.Core already have NRT enabled. Enabling NRT would improve nullability safety for consumers of the Desktop views library and align it with the rest of the codebase.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Desktop.Common/SkiaSharp.Views.Desktop.Common.csproj",
        "finding": "The project file does not contain a <Nullable>enable</Nullable> property, confirming NRT is not yet enabled.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SkiaSharp.Views.Blazor.csproj",
        "finding": "This project already has <Nullable>enable</Nullable>, showing the pattern is established in the codebase.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Shared/SKPaintSurfaceEventArgs.cs",
        "finding": "Shared source file used by Desktop.Common has no nullable annotations (no ? or ! operators), consistent with NRT being disabled.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Enable Nullable Reference Types (SkiaSharp.Views.Desktop.Common)",
        "source": "issue title",
        "interpretation": "Clear enhancement request to add NRT support to the Desktop.Common views package."
      }
    ],
    "rationale": "This is a straightforward enhancement request. The Desktop.Common project does not have NRT enabled while peer projects (Blazor, Maui.Core) do. No bug, no regression. The enhancement improves code quality and developer ergonomics for consumers. Suggested action is keep-open since it requires design work to annotate all shared source files without breaking API compatibility.",
    "resolution": {
      "hypothesis": "Add `<Nullable>enable</Nullable>` to SkiaSharp.Views.Desktop.Common.csproj and annotate all shared source files compiled into this target with appropriate nullable annotations.",
      "proposals": [
        {
          "title": "Enable NRT in Desktop.Common project",
          "category": "fix",
          "effort": "cost/m",
          "validated": "untested",
          "description": "Add `<Nullable>enable</Nullable>` to the csproj and systematically annotate shared source files (SKPaintSurfaceEventArgs.cs, SKPaintGLSurfaceEventArgs.cs, Extensions.cs) with nullable annotations. Ensure no new warnings break the build."
        }
      ],
      "recommendedProposal": "proposal-1",
      "recommendedReason": "Direct implementation of the requested feature, consistent with how other views projects have already adopted NRT."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.88,
      "reason": "Valid enhancement aligned with existing NRT adoption in the codebase. Needs implementation work to annotate shared source files properly. No urgency but clear improvement.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/enhancement, area/SkiaSharp.Views, tenet/reliability, tenet/compatibility",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp.Views",
          "tenet/reliability",
          "tenet/compatibility"
        ]
      }
    ]
  }
}
```

</details>
