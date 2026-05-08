# Issue Triage Report — #2065

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T15:51:00Z |
| Type | type/enhancement (0.97 (97%)) |
| Area | area/SkiaSharp.Views.Forms (0.90 (90%)) |
| Suggested action | keep-open (0.88 (88%)) |

**Issue Summary:** Enable Nullable Reference Types (NRT) in the SkiaSharp.Views.Forms.WPF package to improve null-safety for consumers using C# nullable annotations.

**Analysis:** The issue requests enabling C# Nullable Reference Types (NRT) annotations in the SkiaSharp.Views.Forms.WPF package. Investigation shows the current WPF views source (`SkiaSharp.Views.WPF`) does not have `<Nullable>enable</Nullable>` in its csproj, and the C# source files lack `#nullable enable` directives. Other packages like SkiaSharp.Views.Blazor and SkiaSharp.Views.Maui have already adopted NRT. This is a code quality/developer-experience enhancement that is part of a broader NRT adoption effort across SkiaSharp view packages.

**Recommendations:** **keep-open** — Valid enhancement request that is part of a broader NRT adoption effort. Already labeled as status/long-term. No immediate repro needed — this is a code quality improvement task.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.Views.Forms |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2064 — Sister issue: Enable Nullable Reference Types (SkiaSharp.Views.Forms)

## Analysis

### Technical Summary

The issue requests enabling C# Nullable Reference Types (NRT) annotations in the SkiaSharp.Views.Forms.WPF package. Investigation shows the current WPF views source (`SkiaSharp.Views.WPF`) does not have `<Nullable>enable</Nullable>` in its csproj, and the C# source files lack `#nullable enable` directives. Other packages like SkiaSharp.Views.Blazor and SkiaSharp.Views.Maui have already adopted NRT. This is a code quality/developer-experience enhancement that is part of a broader NRT adoption effort across SkiaSharp view packages.

### Rationale

This is a clear enhancement request to enable NRT annotations in one specific package. The pattern is already established in SkiaSharp.Views.Blazor. Paired with issue #2064 for SkiaSharp.Views.Forms base. Classified as `type/enhancement` (improving existing functionality), `area/SkiaSharp.Views.Forms` (the Xamarin.Forms WPF backend), and `os/Windows-Classic` (WPF targets Windows desktop). `tenet/compatibility` applies because adding NRT annotations is a source-breaking change for consumers with warnings-as-errors.

### Key Signals

- "Enable Nullable Reference Types (SkiaSharp.Views.Forms.WPF)" — **issue title** (Explicit request to enable NRT in the WPF backend of the Views.Forms package)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SkiaSharp.Views.WPF.csproj` | — | direct | No `<Nullable>enable</Nullable>` property set in the project file. NRT is not enabled for this package. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs` | — | direct | No `#nullable enable` directive present. Fields like `WriteableBitmap bitmap` are non-nullable without NRT annotations, which would need to be updated when enabling NRT. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SkiaSharp.Views.Blazor.csproj` | — | related | `<Nullable>enable</Nullable>` is already set — demonstrates the pattern to follow for WPF views. |

### Resolution Proposals

**Hypothesis:** Enable `<Nullable>enable</Nullable>` in SkiaSharp.Views.WPF.csproj and add nullable annotations to all source files in that project, following the pattern used in SkiaSharp.Views.Blazor.

1. **Enable NRT in SkiaSharp.Views.WPF project** — fix, cost/m, validated=untested
   - Add `<Nullable>enable</Nullable>` to SkiaSharp.Views.WPF.csproj and annotate all public APIs and fields with appropriate nullable annotations (`?` for nullable, non-null for required). Follow the pattern established in SkiaSharp.Views.Blazor.

**Recommended proposal:** Enable NRT in SkiaSharp.Views.WPF project

**Why:** Directly addresses the request and follows established patterns in the repository.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.88 (88%) |
| Reason | Valid enhancement request that is part of a broader NRT adoption effort. Already labeled as status/long-term. No immediate repro needed — this is a code quality improvement task. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply classification labels | labels=type/enhancement, area/SkiaSharp.Views.Forms, os/Windows-Classic, tenet/compatibility, triage/triaged |
| link-related | low | 0.95 (95%) | Link to sister issue #2064 Enable Nullable Reference Types (SkiaSharp.Views.Forms) | linkedIssue=#2064 |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2065,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T15:51:00Z"
  },
  "summary": "Enable Nullable Reference Types (NRT) in the SkiaSharp.Views.Forms.WPF package to improve null-safety for consumers using C# nullable annotations.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views.Forms",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [],
      "attachments": [],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2064",
          "description": "Sister issue: Enable Nullable Reference Types (SkiaSharp.Views.Forms)"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The issue requests enabling C# Nullable Reference Types (NRT) annotations in the SkiaSharp.Views.Forms.WPF package. Investigation shows the current WPF views source (`SkiaSharp.Views.WPF`) does not have `<Nullable>enable</Nullable>` in its csproj, and the C# source files lack `#nullable enable` directives. Other packages like SkiaSharp.Views.Blazor and SkiaSharp.Views.Maui have already adopted NRT. This is a code quality/developer-experience enhancement that is part of a broader NRT adoption effort across SkiaSharp view packages.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SkiaSharp.Views.WPF.csproj",
        "finding": "No `<Nullable>enable</Nullable>` property set in the project file. NRT is not enabled for this package.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs",
        "finding": "No `#nullable enable` directive present. Fields like `WriteableBitmap bitmap` are non-nullable without NRT annotations, which would need to be updated when enabling NRT.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SkiaSharp.Views.Blazor.csproj",
        "finding": "`<Nullable>enable</Nullable>` is already set — demonstrates the pattern to follow for WPF views.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Enable Nullable Reference Types (SkiaSharp.Views.Forms.WPF)",
        "source": "issue title",
        "interpretation": "Explicit request to enable NRT in the WPF backend of the Views.Forms package"
      }
    ],
    "rationale": "This is a clear enhancement request to enable NRT annotations in one specific package. The pattern is already established in SkiaSharp.Views.Blazor. Paired with issue #2064 for SkiaSharp.Views.Forms base. Classified as `type/enhancement` (improving existing functionality), `area/SkiaSharp.Views.Forms` (the Xamarin.Forms WPF backend), and `os/Windows-Classic` (WPF targets Windows desktop). `tenet/compatibility` applies because adding NRT annotations is a source-breaking change for consumers with warnings-as-errors.",
    "resolution": {
      "hypothesis": "Enable `<Nullable>enable</Nullable>` in SkiaSharp.Views.WPF.csproj and add nullable annotations to all source files in that project, following the pattern used in SkiaSharp.Views.Blazor.",
      "proposals": [
        {
          "title": "Enable NRT in SkiaSharp.Views.WPF project",
          "category": "fix",
          "description": "Add `<Nullable>enable</Nullable>` to SkiaSharp.Views.WPF.csproj and annotate all public APIs and fields with appropriate nullable annotations (`?` for nullable, non-null for required). Follow the pattern established in SkiaSharp.Views.Blazor.",
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Enable NRT in SkiaSharp.Views.WPF project",
      "recommendedReason": "Directly addresses the request and follows established patterns in the repository."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.88,
      "reason": "Valid enhancement request that is part of a broader NRT adoption effort. Already labeled as status/long-term. No immediate repro needed — this is a code quality improvement task.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply classification labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp.Views.Forms",
          "os/Windows-Classic",
          "tenet/compatibility",
          "triage/triaged"
        ]
      },
      {
        "type": "link-related",
        "description": "Link to sister issue #2064 Enable Nullable Reference Types (SkiaSharp.Views.Forms)",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 2064
      }
    ]
  }
}
```

</details>
