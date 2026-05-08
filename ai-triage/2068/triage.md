# Issue Triage Report — #2068

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T16:49:31Z |
| Type | type/enhancement (0.98 (98%)) |
| Area | area/SkiaSharp.Views (0.95 (95%)) |
| Suggested action | keep-open (0.92 (92%)) |

**Issue Summary:** Tracking issue to enable Nullable Reference Types (NRT) in the SkiaSharp.Views.WinUI project, filed by a maintainer as part of a broader NRT adoption effort across all SkiaSharp.Views packages.

**Analysis:** SkiaSharp.Views.WinUI has not yet enabled Nullable Reference Types. The .csproj is missing <Nullable>enable</Nullable>, and the shared source files lack nullable annotations. This is part of a coordinated effort (issues #2058–#2072) to annotate all SkiaSharp.Views packages.

**Recommendations:** **keep-open** — Valid maintainer-filed enhancement with clear scope. The work is not done yet — NRT is still not enabled in SkiaSharp.Views.WinUI.csproj. Keep open until a PR lands.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-WinUI |
| Backends | — |
| Tenets | tenet/reliability, tenet/compatibility |
| Partner | — |
| Current labels | type/enhancement, status/long-term |

## Evidence

### Reproduction

**Environment:** SkiaSharp.Views.WinUI — no <Nullable>enable</Nullable> in project file

**Related issues:** #2058, #2059, #2060, #2061, #2062, #2063, #2064, #2065, #2066, #2067, #2069, #2070, #2071, #2072

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The SkiaSharp.Views.WinUI.csproj does not contain <Nullable>enable</Nullable> as of current main. The shared source files (SKPaintSurfaceEventArgs.cs, Extensions.cs) also lack nullable annotations. The work is still open. |

## Analysis

### Technical Summary

SkiaSharp.Views.WinUI has not yet enabled Nullable Reference Types. The .csproj is missing <Nullable>enable</Nullable>, and the shared source files lack nullable annotations. This is part of a coordinated effort (issues #2058–#2072) to annotate all SkiaSharp.Views packages.

### Rationale

This is clearly an enhancement/tech-debt tracking issue filed by a maintainer. No bug is reported — the ask is to add <Nullable>enable</Nullable> to the .csproj and annotate the shared source files that compile into SkiaSharp.Views.WinUI. Correct classification is type/enhancement, area/SkiaSharp.Views, platform os/Windows-WinUI. The issue is still open and relevant.

### Key Signals

- "Enable Nullable Reference Types (SkiaSharp.Views.WinUI)" — **issue title** (Maintainer-filed tracking issue for NRT adoption in this specific package.)
- "Related issues #2058–#2072 all follow the same pattern across Views sub-packages" — **GitHub search** (Coordinated NRT rollout across all SkiaSharp.Views projects. Most are still open.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SkiaSharp.Views.WinUI.csproj` | — | direct | No <Nullable>enable</Nullable> property present. All other binding projects (SkiaSharp.csproj, HarfBuzzSharp.csproj, SkiaSharp.Views.Maui.Core.csproj, SkiaSharp.Views.Blazor.csproj) already have Nullable enabled. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Shared/SKPaintSurfaceEventArgs.cs` | — | direct | SKSurface Surface property has no nullable annotation. Since SKSurface is a reference type and the constructor accepts it without null check, enabling NRT would require annotating Surface as SKSurface? or adding a guard. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Shared/Extensions.cs` | — | context | EnvironmentExtensions.IsValidEnvironment — no nullable concerns here; DllNotFoundException catch pattern is NRT-safe. |

### Next Questions

- Should SKPaintSurfaceEventArgs.Surface be annotated as SKSurface? (nullable) or SKSurface (non-nullable with guard)?
- Are there any platform-specific WinUI source files beyond the shared sources that also need annotation?

### Resolution Proposals

**Hypothesis:** Add <Nullable>enable</Nullable> to SkiaSharp.Views.WinUI.csproj and audit all shared and WinUI-specific source files for nullable correctness.

1. **Enable Nullable in .csproj and annotate shared sources** — fix, confidence 0.95 (95%), cost/s, validated=untested
   - Add <Nullable>enable</Nullable> to SkiaSharp.Views.WinUI.csproj, then fix any resulting nullable warnings in the shared source files (SKPaintSurfaceEventArgs.cs, Extensions.cs, SKPaintGLSurfaceEventArgs.cs) that compile into this project.

**Recommended proposal:** Enable Nullable in .csproj and annotate shared sources

**Why:** Straightforward, well-scoped change. Other packages in the same repo already follow this pattern.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.92 (92%) |
| Reason | Valid maintainer-filed enhancement with clear scope. The work is not done yet — NRT is still not enabled in SkiaSharp.Views.WinUI.csproj. Keep open until a PR lands. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.98 (98%) | Apply enhancement, views, WinUI labels | labels=type/enhancement, area/SkiaSharp.Views, os/Windows-WinUI |
| link-related | low | 0.90 (90%) | Cross-reference parent NRT tracking issue #2058 | linkedIssue=#2058 |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2068,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T16:49:31Z",
    "currentLabels": [
      "type/enhancement",
      "status/long-term"
    ]
  },
  "summary": "Tracking issue to enable Nullable Reference Types (NRT) in the SkiaSharp.Views.WinUI project, filed by a maintainer as part of a broader NRT adoption effort across all SkiaSharp.Views packages.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.98
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-WinUI"
    ],
    "tenets": [
      "tenet/reliability",
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "SkiaSharp.Views.WinUI — no <Nullable>enable</Nullable> in project file",
      "relatedIssues": [
        2058,
        2059,
        2060,
        2061,
        2062,
        2063,
        2064,
        2065,
        2066,
        2067,
        2069,
        2070,
        2071,
        2072
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "The SkiaSharp.Views.WinUI.csproj does not contain <Nullable>enable</Nullable> as of current main. The shared source files (SKPaintSurfaceEventArgs.cs, Extensions.cs) also lack nullable annotations. The work is still open."
    }
  },
  "analysis": {
    "summary": "SkiaSharp.Views.WinUI has not yet enabled Nullable Reference Types. The .csproj is missing <Nullable>enable</Nullable>, and the shared source files lack nullable annotations. This is part of a coordinated effort (issues #2058–#2072) to annotate all SkiaSharp.Views packages.",
    "rationale": "This is clearly an enhancement/tech-debt tracking issue filed by a maintainer. No bug is reported — the ask is to add <Nullable>enable</Nullable> to the .csproj and annotate the shared source files that compile into SkiaSharp.Views.WinUI. Correct classification is type/enhancement, area/SkiaSharp.Views, platform os/Windows-WinUI. The issue is still open and relevant.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SkiaSharp.Views.WinUI.csproj",
        "finding": "No <Nullable>enable</Nullable> property present. All other binding projects (SkiaSharp.csproj, HarfBuzzSharp.csproj, SkiaSharp.Views.Maui.Core.csproj, SkiaSharp.Views.Blazor.csproj) already have Nullable enabled.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Shared/SKPaintSurfaceEventArgs.cs",
        "finding": "SKSurface Surface property has no nullable annotation. Since SKSurface is a reference type and the constructor accepts it without null check, enabling NRT would require annotating Surface as SKSurface? or adding a guard.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Shared/Extensions.cs",
        "finding": "EnvironmentExtensions.IsValidEnvironment — no nullable concerns here; DllNotFoundException catch pattern is NRT-safe.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "Enable Nullable Reference Types (SkiaSharp.Views.WinUI)",
        "source": "issue title",
        "interpretation": "Maintainer-filed tracking issue for NRT adoption in this specific package."
      },
      {
        "text": "Related issues #2058–#2072 all follow the same pattern across Views sub-packages",
        "source": "GitHub search",
        "interpretation": "Coordinated NRT rollout across all SkiaSharp.Views projects. Most are still open."
      }
    ],
    "nextQuestions": [
      "Should SKPaintSurfaceEventArgs.Surface be annotated as SKSurface? (nullable) or SKSurface (non-nullable with guard)?",
      "Are there any platform-specific WinUI source files beyond the shared sources that also need annotation?"
    ],
    "resolution": {
      "hypothesis": "Add <Nullable>enable</Nullable> to SkiaSharp.Views.WinUI.csproj and audit all shared and WinUI-specific source files for nullable correctness.",
      "proposals": [
        {
          "title": "Enable Nullable in .csproj and annotate shared sources",
          "description": "Add <Nullable>enable</Nullable> to SkiaSharp.Views.WinUI.csproj, then fix any resulting nullable warnings in the shared source files (SKPaintSurfaceEventArgs.cs, Extensions.cs, SKPaintGLSurfaceEventArgs.cs) that compile into this project.",
          "category": "fix",
          "confidence": 0.95,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Enable Nullable in .csproj and annotate shared sources",
      "recommendedReason": "Straightforward, well-scoped change. Other packages in the same repo already follow this pattern."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.92,
      "reason": "Valid maintainer-filed enhancement with clear scope. The work is not done yet — NRT is still not enabled in SkiaSharp.Views.WinUI.csproj. Keep open until a PR lands.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement, views, WinUI labels",
        "risk": "low",
        "confidence": 0.98,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp.Views",
          "os/Windows-WinUI"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference parent NRT tracking issue #2058",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 2058
      }
    ]
  }
}
```

</details>
