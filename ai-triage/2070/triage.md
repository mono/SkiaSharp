# Issue Triage Report — #2070

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T17:32:51Z |
| Type | type/enhancement (0.97 (97%)) |
| Area | area/SkiaSharp.Views.Maui (0.97 (97%)) |
| Suggested action | keep-open (0.92 (92%)) |

**Issue Summary:** Request to enable project-wide Nullable Reference Types in SkiaSharp.Views.Maui.Controls by adding <Nullable>Enable</Nullable> to the .csproj, completing the partial per-file opt-ins already present.

**Analysis:** The Controls project has per-file #nullable enable in SKCanvasView.cs and SKGLView.cs, but the csproj lacks <Nullable>Enable</Nullable>. The sibling Core project already has this enabled. AppHostBuilderExtensions.cs and RendererTypes.cs do not yet have per-file opt-ins, so enabling project-wide NRT would require fixing nullable warnings in those files.

**Recommendations:** **keep-open** — Valid, low-risk enhancement filed by a maintainer and tagged status/long-term. No blocker — implementation path is clear, but it requires dedicated effort to fix NRT warnings across the project.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | — |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/enhancement, status/long-term, area/SkiaSharp.Views.Maui |

## Evidence

### Reproduction

**Environment:** SkiaSharp.Views.Maui.Controls project — no platform or version restrictions

## Analysis

### Technical Summary

The Controls project has per-file #nullable enable in SKCanvasView.cs and SKGLView.cs, but the csproj lacks <Nullable>Enable</Nullable>. The sibling Core project already has this enabled. AppHostBuilderExtensions.cs and RendererTypes.cs do not yet have per-file opt-ins, so enabling project-wide NRT would require fixing nullable warnings in those files.

### Rationale

Clearly an enhancement — no broken behavior, just a code-quality improvement. The issue is self-filed by a maintainer with status/long-term. The Core project already has Nullable enabled, confirming the intent. The Controls project partially uses #nullable enable at the file level; adding the project property completes the work and catches the remaining files.

### Key Signals

- "Enable Nullable Reference Types (SkiaSharp.Views.Maui.Controls)" — **issue title** (Direct enhancement request to add <Nullable>Enable</Nullable> to the csproj)
- "<Nullable>Enable</Nullable> present in Core but absent in Controls" — **source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SkiaSharp.Views.Maui.Core.csproj line 8** (Core already done; Controls is the remaining gap)
- "#nullable enable present in SKCanvasView.cs and SKGLView.cs, absent in AppHostBuilderExtensions.cs and RendererTypes.cs" — **source files** (Partial adoption — project-level setting needed to enforce uniformly)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SkiaSharp.Views.Maui.Controls.csproj` | — | direct | No <Nullable>Enable</Nullable> property in PropertyGroup — project-level NRT is disabled |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SkiaSharp.Views.Maui.Core.csproj` | 8 | related | <Nullable>Enable</Nullable> is already present in Core, establishing the pattern |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SKCanvasView.cs` | 1 | context | #nullable enable at top of file — per-file opt-in already present |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/AppHostBuilderExtensions.cs` | — | direct | No #nullable enable — will require nullable annotation fixes once project-level NRT is enabled |

### Next Questions

- Are there any nullable warnings in AppHostBuilderExtensions.cs or RendererTypes.cs that need addressing before this can be merged?
- Should GetPropertyValueEventArgs<T>.Value in RendererTypes.cs be annotated as nullable?

### Resolution Proposals

**Hypothesis:** Add <Nullable>Enable</Nullable> to SkiaSharp.Views.Maui.Controls.csproj and fix any resulting NRT warnings in AppHostBuilderExtensions.cs and RendererTypes.cs.

1. **Add Nullable=Enable to Controls csproj** — fix, confidence 0.95 (95%), cost/s, validated=untested
   - Add <Nullable>Enable</Nullable> to the PropertyGroup in SkiaSharp.Views.Maui.Controls.csproj, then resolve any NRT warnings in the remaining files.

**Recommended proposal:** Add Nullable=Enable to Controls csproj

**Why:** Straightforward one-property addition following the existing Core pattern; minimal scope.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.92 (92%) |
| Reason | Valid, low-risk enhancement filed by a maintainer and tagged status/long-term. No blocker — implementation path is clear, but it requires dedicated effort to fix NRT warnings across the project. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Ensure type/enhancement and area/SkiaSharp.Views.Maui labels are applied | labels=type/enhancement, area/SkiaSharp.Views.Maui, tenet/reliability |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2070,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T17:32:51Z",
    "currentLabels": [
      "type/enhancement",
      "status/long-term",
      "area/SkiaSharp.Views.Maui"
    ]
  },
  "summary": "Request to enable project-wide Nullable Reference Types in SkiaSharp.Views.Maui.Controls by adding <Nullable>Enable</Nullable> to the .csproj, completing the partial per-file opt-ins already present.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.97
    },
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "SkiaSharp.Views.Maui.Controls project — no platform or version restrictions"
    }
  },
  "analysis": {
    "summary": "The Controls project has per-file #nullable enable in SKCanvasView.cs and SKGLView.cs, but the csproj lacks <Nullable>Enable</Nullable>. The sibling Core project already has this enabled. AppHostBuilderExtensions.cs and RendererTypes.cs do not yet have per-file opt-ins, so enabling project-wide NRT would require fixing nullable warnings in those files.",
    "rationale": "Clearly an enhancement — no broken behavior, just a code-quality improvement. The issue is self-filed by a maintainer with status/long-term. The Core project already has Nullable enabled, confirming the intent. The Controls project partially uses #nullable enable at the file level; adding the project property completes the work and catches the remaining files.",
    "keySignals": [
      {
        "text": "Enable Nullable Reference Types (SkiaSharp.Views.Maui.Controls)",
        "source": "issue title",
        "interpretation": "Direct enhancement request to add <Nullable>Enable</Nullable> to the csproj"
      },
      {
        "text": "<Nullable>Enable</Nullable> present in Core but absent in Controls",
        "source": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SkiaSharp.Views.Maui.Core.csproj line 8",
        "interpretation": "Core already done; Controls is the remaining gap"
      },
      {
        "text": "#nullable enable present in SKCanvasView.cs and SKGLView.cs, absent in AppHostBuilderExtensions.cs and RendererTypes.cs",
        "source": "source files",
        "interpretation": "Partial adoption — project-level setting needed to enforce uniformly"
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SkiaSharp.Views.Maui.Controls.csproj",
        "finding": "No <Nullable>Enable</Nullable> property in PropertyGroup — project-level NRT is disabled",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SkiaSharp.Views.Maui.Core.csproj",
        "lines": "8",
        "finding": "<Nullable>Enable</Nullable> is already present in Core, establishing the pattern",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SKCanvasView.cs",
        "lines": "1",
        "finding": "#nullable enable at top of file — per-file opt-in already present",
        "relevance": "context"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/AppHostBuilderExtensions.cs",
        "finding": "No #nullable enable — will require nullable annotation fixes once project-level NRT is enabled",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "Are there any nullable warnings in AppHostBuilderExtensions.cs or RendererTypes.cs that need addressing before this can be merged?",
      "Should GetPropertyValueEventArgs<T>.Value in RendererTypes.cs be annotated as nullable?"
    ],
    "resolution": {
      "hypothesis": "Add <Nullable>Enable</Nullable> to SkiaSharp.Views.Maui.Controls.csproj and fix any resulting NRT warnings in AppHostBuilderExtensions.cs and RendererTypes.cs.",
      "proposals": [
        {
          "title": "Add Nullable=Enable to Controls csproj",
          "description": "Add <Nullable>Enable</Nullable> to the PropertyGroup in SkiaSharp.Views.Maui.Controls.csproj, then resolve any NRT warnings in the remaining files.",
          "category": "fix",
          "confidence": 0.95,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add Nullable=Enable to Controls csproj",
      "recommendedReason": "Straightforward one-property addition following the existing Core pattern; minimal scope."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.92,
      "reason": "Valid, low-risk enhancement filed by a maintainer and tagged status/long-term. No blocker — implementation path is clear, but it requires dedicated effort to fix NRT warnings across the project.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Ensure type/enhancement and area/SkiaSharp.Views.Maui labels are applied",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp.Views.Maui",
          "tenet/reliability"
        ]
      }
    ]
  }
}
```

</details>
