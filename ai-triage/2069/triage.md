# Issue Triage Report — #2069

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T17:11:17Z |
| Type | type/enhancement (0.97 (97%)) |
| Area | area/SkiaSharp.Views.Maui (0.99 (99%)) |
| Suggested action | close-as-fixed (0.90 (90%)) |

**Issue Summary:** Request to enable Nullable Reference Types in SkiaSharp.Views.Maui.Core project, which has since been implemented.

**Analysis:** The request was to enable C# Nullable Reference Types (NRT) in the SkiaSharp.Views.Maui.Core project. Code investigation shows <Nullable>Enable</Nullable> is already present in the project file, meaning this enhancement has been implemented since the issue was filed in May 2022.

**Recommendations:** **close-as-fixed** — The project file already has Nullable enabled, confirming the enhancement was implemented.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | partner/maui |

## Evidence

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.95 (95%) |
| Reason | The SkiaSharp.Views.Maui.Core.csproj already contains <Nullable>Enable</Nullable>, indicating the enhancement has been implemented. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | unknown |

## Analysis

### Technical Summary

The request was to enable C# Nullable Reference Types (NRT) in the SkiaSharp.Views.Maui.Core project. Code investigation shows <Nullable>Enable</Nullable> is already present in the project file, meaning this enhancement has been implemented since the issue was filed in May 2022.

### Rationale

This is a type/enhancement request from the maintainer themselves. Code investigation confirms the enhancement has been implemented: <Nullable>Enable</Nullable> is present in the .csproj. The issue can likely be closed as fixed.

### Key Signals

- "Enable Nullable Reference Types (SkiaSharp.Views.Maui.Core)" — **issue title** (The reporter (project maintainer) wanted to enable NRT in the Maui Core package)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SkiaSharp.Views.Maui.Core.csproj` | 8 | direct | <Nullable>Enable</Nullable> is present in the project file, confirming NRT is enabled |

### Resolution Proposals

**Hypothesis:** The nullable reference types feature was enabled after the issue was filed.

1. **Close as fixed** — fix, cost/xs, validated=yes
   - The SkiaSharp.Views.Maui.Core.csproj already has <Nullable>Enable</Nullable>. The enhancement has been implemented.

**Recommended proposal:** Close as fixed

**Why:** Code investigation confirms <Nullable>Enable</Nullable> is already in the project file, so the enhancement request is fulfilled.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.90 (90%) |
| Reason | The project file already has Nullable enabled, confirming the enhancement was implemented. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply type/enhancement, area/SkiaSharp.Views.Maui, partner/maui labels | labels=type/enhancement, area/SkiaSharp.Views.Maui, partner/maui |
| close-issue | medium | 0.90 (90%) | Close as fixed since Nullable is already enabled in the project | stateReason=completed |
| add-comment | medium | 0.90 (90%) | Inform that Nullable Reference Types are already enabled | — |

**Comment draft for `add-comment`:**

```markdown
This enhancement has been implemented. The `SkiaSharp.Views.Maui.Core.csproj` project file already contains `<Nullable>Enable</Nullable>`, so Nullable Reference Types are enabled for this package. Closing as fixed.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2069,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T17:11:17Z"
  },
  "summary": "Request to enable Nullable Reference Types in SkiaSharp.Views.Maui.Core project, which has since been implemented.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.99
    },
    "partner": "partner/maui"
  },
  "evidence": {
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.95,
      "reason": "The SkiaSharp.Views.Maui.Core.csproj already contains <Nullable>Enable</Nullable>, indicating the enhancement has been implemented.",
      "fixedInVersion": "unknown"
    }
  },
  "analysis": {
    "summary": "The request was to enable C# Nullable Reference Types (NRT) in the SkiaSharp.Views.Maui.Core project. Code investigation shows <Nullable>Enable</Nullable> is already present in the project file, meaning this enhancement has been implemented since the issue was filed in May 2022.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SkiaSharp.Views.Maui.Core.csproj",
        "finding": "<Nullable>Enable</Nullable> is present in the project file, confirming NRT is enabled",
        "relevance": "direct",
        "lines": "8"
      }
    ],
    "keySignals": [
      {
        "text": "Enable Nullable Reference Types (SkiaSharp.Views.Maui.Core)",
        "source": "issue title",
        "interpretation": "The reporter (project maintainer) wanted to enable NRT in the Maui Core package"
      }
    ],
    "rationale": "This is a type/enhancement request from the maintainer themselves. Code investigation confirms the enhancement has been implemented: <Nullable>Enable</Nullable> is present in the .csproj. The issue can likely be closed as fixed.",
    "resolution": {
      "hypothesis": "The nullable reference types feature was enabled after the issue was filed.",
      "proposals": [
        {
          "title": "Close as fixed",
          "description": "The SkiaSharp.Views.Maui.Core.csproj already has <Nullable>Enable</Nullable>. The enhancement has been implemented.",
          "category": "fix",
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Close as fixed",
      "recommendedReason": "Code investigation confirms <Nullable>Enable</Nullable> is already in the project file, so the enhancement request is fulfilled."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.9,
      "reason": "The project file already has Nullable enabled, confirming the enhancement was implemented.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/enhancement, area/SkiaSharp.Views.Maui, partner/maui labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp.Views.Maui",
          "partner/maui"
        ]
      },
      {
        "type": "close-issue",
        "description": "Close as fixed since Nullable is already enabled in the project",
        "risk": "medium",
        "confidence": 0.9,
        "stateReason": "completed"
      },
      {
        "type": "add-comment",
        "description": "Inform that Nullable Reference Types are already enabled",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "This enhancement has been implemented. The `SkiaSharp.Views.Maui.Core.csproj` project file already contains `<Nullable>Enable</Nullable>`, so Nullable Reference Types are enabled for this package. Closing as fixed."
      }
    ]
  }
}
```

</details>
