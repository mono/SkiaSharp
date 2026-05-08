# Issue Triage Report — #2071

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T17:57:33Z |
| Type | type/enhancement (0.97 (97%)) |
| Area | area/SkiaSharp.Views.Maui (0.97 (97%)) |
| Suggested action | keep-open (0.90 (90%)) |

**Issue Summary:** Enable Nullable Reference Types (NRT) in the SkiaSharp.Views.Maui.Controls.Compatibility package to improve null-safety for consumers using NRT-enabled projects.

**Analysis:** The SkiaSharp.Views.Maui.Controls.Compatibility package (which provides Xamarin.Forms-compatible shims for MAUI) was not annotated with Nullable Reference Types. The sibling packages SkiaSharp.Views.Maui.Core (Nullable=Enable in csproj) and SkiaSharp.Views.Maui.Controls (per-file #nullable enable) have already been partially or fully annotated. The Compatibility package source is absent from the current repo tree, indicating it may have been removed or was never fully implemented; the changelogs directory confirms it shipped in 2.88.x. NRT annotation is a quality-of-life improvement that prevents null warnings from leaking into consumer codebases.

**Recommendations:** **keep-open** — This is a well-defined enhancement from the maintainer. No repro needed; it is a code quality improvement task. Should stay open until the Compatibility package source is annotated.

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

### Reproduction

## Analysis

### Technical Summary

The SkiaSharp.Views.Maui.Controls.Compatibility package (which provides Xamarin.Forms-compatible shims for MAUI) was not annotated with Nullable Reference Types. The sibling packages SkiaSharp.Views.Maui.Core (Nullable=Enable in csproj) and SkiaSharp.Views.Maui.Controls (per-file #nullable enable) have already been partially or fully annotated. The Compatibility package source is absent from the current repo tree, indicating it may have been removed or was never fully implemented; the changelogs directory confirms it shipped in 2.88.x. NRT annotation is a quality-of-life improvement that prevents null warnings from leaking into consumer codebases.

### Rationale

This is a maintainer-filed enhancement (mattleibow) to add NRT annotations to the Controls.Compatibility package. The sibling packages are already annotated; this closes the gap. The source for Compatibility is absent from the current tree so its current state cannot be confirmed, but changelogs confirm it was shipped. This is a keep-open enhancement with low priority since the package appears to be a legacy compatibility shim.

### Key Signals

- "Enable Nullable Reference Types (SkiaSharp.Views.Maui.Controls.Compatibility)" — **issue title** (Maintainer-filed enhancement to annotate the Compatibility package for NRT)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SkiaSharp.Views.Maui.Core.csproj` | — | direct | Has <Nullable>Enable</Nullable> in the project file, confirming NRT is already enabled for the Core package. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SKCanvasView.cs` | — | direct | Contains #nullable enable at file level, showing Controls package has started NRT adoption. |
| `changelogs/SkiaSharp.Views.Maui.Controls.Compatibility` | — | context | Changelog directories exist for 2.88.0 through 2.88.9 confirming the Compatibility package was shipped, but its source is not present in the current repo tree. |

### Resolution Proposals

1. **Enable NRT in Compatibility package** — fix, cost/m, validated=untested
   - Add <Nullable>Enable</Nullable> to the SkiaSharp.Views.Maui.Controls.Compatibility project file and annotate all public APIs with appropriate nullable annotations.

**Recommended proposal:** Enable NRT in Compatibility package

**Why:** Consistent NRT adoption across all MAUI packages is the standard approach already applied to sibling packages.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.90 (90%) |
| Reason | This is a well-defined enhancement from the maintainer. No repro needed; it is a code quality improvement task. Should stay open until the Compatibility package source is annotated. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply type/enhancement, area/SkiaSharp.Views.Maui, partner/maui labels | labels=type/enhancement, area/SkiaSharp.Views.Maui, partner/maui |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2071,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T17:57:33Z"
  },
  "summary": "Enable Nullable Reference Types (NRT) in the SkiaSharp.Views.Maui.Controls.Compatibility package to improve null-safety for consumers using NRT-enabled projects.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.97
    },
    "partner": "partner/maui"
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [],
      "attachments": [],
      "repoLinks": []
    }
  },
  "analysis": {
    "summary": "The SkiaSharp.Views.Maui.Controls.Compatibility package (which provides Xamarin.Forms-compatible shims for MAUI) was not annotated with Nullable Reference Types. The sibling packages SkiaSharp.Views.Maui.Core (Nullable=Enable in csproj) and SkiaSharp.Views.Maui.Controls (per-file #nullable enable) have already been partially or fully annotated. The Compatibility package source is absent from the current repo tree, indicating it may have been removed or was never fully implemented; the changelogs directory confirms it shipped in 2.88.x. NRT annotation is a quality-of-life improvement that prevents null warnings from leaking into consumer codebases.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SkiaSharp.Views.Maui.Core.csproj",
        "finding": "Has <Nullable>Enable</Nullable> in the project file, confirming NRT is already enabled for the Core package.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SKCanvasView.cs",
        "finding": "Contains #nullable enable at file level, showing Controls package has started NRT adoption.",
        "relevance": "direct"
      },
      {
        "file": "changelogs/SkiaSharp.Views.Maui.Controls.Compatibility",
        "finding": "Changelog directories exist for 2.88.0 through 2.88.9 confirming the Compatibility package was shipped, but its source is not present in the current repo tree.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "Enable Nullable Reference Types (SkiaSharp.Views.Maui.Controls.Compatibility)",
        "source": "issue title",
        "interpretation": "Maintainer-filed enhancement to annotate the Compatibility package for NRT"
      }
    ],
    "rationale": "This is a maintainer-filed enhancement (mattleibow) to add NRT annotations to the Controls.Compatibility package. The sibling packages are already annotated; this closes the gap. The source for Compatibility is absent from the current tree so its current state cannot be confirmed, but changelogs confirm it was shipped. This is a keep-open enhancement with low priority since the package appears to be a legacy compatibility shim.",
    "resolution": {
      "proposals": [
        {
          "title": "Enable NRT in Compatibility package",
          "category": "fix",
          "description": "Add <Nullable>Enable</Nullable> to the SkiaSharp.Views.Maui.Controls.Compatibility project file and annotate all public APIs with appropriate nullable annotations.",
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Enable NRT in Compatibility package",
      "recommendedReason": "Consistent NRT adoption across all MAUI packages is the standard approach already applied to sibling packages."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.9,
      "reason": "This is a well-defined enhancement from the maintainer. No repro needed; it is a code quality improvement task. Should stay open until the Compatibility package source is annotated.",
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
      }
    ]
  }
}
```

</details>
