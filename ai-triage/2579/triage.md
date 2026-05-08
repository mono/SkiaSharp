# Issue Triage Report — #2579

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T23:03:36Z |
| Type | type/question (0.78 (78%)) |
| Area | area/Build (0.90 (90%)) |
| Suggested action | close-as-fixed (0.80 (80%)) |

**Issue Summary:** Reporter building SkiaSharp 2.88.4 from source on Windows gets NETSDK1139 because the Tizen workload is not in the standard .NET SDK manifest and must be installed via Samsung's separate installer; current main branch already defaults IsNetTizenSupported=false for local builds, resolving this.

**Analysis:** The reporter cloned SkiaSharp 2.88.4 and attempted a local `dotnet build`. The build system included Tizen TFMs unconditionally, but the Tizen SDK is not part of the standard .NET workload manifest — it must be installed via Samsung's own installer. The maintainer provided a workaround (`/pIsNetTizenSupported:false`). The current codebase has since been improved: `source/SkiaSharp.Build.props` now sets `IsNetTizenSupported=false` by default for local builds, so new contributors no longer hit this error.

**Recommendations:** **close-as-fixed** — The current main branch defaults Tizen to disabled for local builds, so the reported error no longer occurs. Maintainer already provided a working workaround for 2.88.4.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/Build |
| Platforms | os/Windows-Classic, os/Tizen |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | type/bug, status/needs-attention |

## Evidence

### Reproduction

1. Clone SkiaSharp 2.88.4 source
2. Run `dotnet build` in the `source/` directory on Windows without Tizen workload
3. Observe NETSDK1139: target platform identifier 'tizen' not recognized

**Environment:** Windows 11, Visual Studio Community 17.7.2, .NET 8.0.100-preview.7.23376.3

**Code snippets:**

```csharp
error NETSDK1139: The target platform identifier tizen was not recognized. [binding/SkiaSharp.NativeAssets.Tizen/SkiaSharp.NativeAssets.Tizen.csproj::TargetFramework=net7.0-tizen]
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.4 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | Current main branch defaults IsNetTizenSupported=false for local (non-CI) builds, so the error no longer occurs without the Tizen workload. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.85 (85%) |
| Reason | source/SkiaSharp.Build.props now conditionally disables Tizen for local builds: IsNetTizenSupported defaults to false when IsCI=false and BuildEverything is not set. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

The reporter cloned SkiaSharp 2.88.4 and attempted a local `dotnet build`. The build system included Tizen TFMs unconditionally, but the Tizen SDK is not part of the standard .NET workload manifest — it must be installed via Samsung's own installer. The maintainer provided a workaround (`/pIsNetTizenSupported:false`). The current codebase has since been improved: `source/SkiaSharp.Build.props` now sets `IsNetTizenSupported=false` by default for local builds, so new contributors no longer hit this error.

### Rationale

This is primarily a setup/environment question about building SkiaSharp from source: the reporter asks 'What might be the problem? Is there something I can do?' The root build issue is fixed in main. The maintainer already answered with a workaround, and a community member later posted the Samsung Tizen installation link.

### Key Signals

- "What might be the problem? Is there something I can do to build SkiaSharp?" — **issue body** (Reporter is asking for setup guidance, not reporting a regression in shipped SkiaSharp functionality.)
- "This does mean that for some reason tizen was not installed correctly. I usually use the script installer. But, you can skip it for now by passing `/pIsNetTizenSupported:false` on the build." — **comment #1690611679 (maintainer mattleibow)** (Maintainer confirmed Tizen must be installed via script installer and provided a workaround. Issue was already answered.)
- "For those of you curious on how to get the Tizen Workload installed, go here: https://github.com/Samsung/Tizen.NET/wiki/Installing-Tizen-.NET-Workload" — **comment #2381538804 (community member cyraid)** (Community confirmed the Tizen workload install steps are documented at Samsung's wiki.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Build.props` | 28-39 | direct | Current code conditionally sets IsNetTizenSupported: true for CI/BuildEverything, false for local builds by default. This means local `dotnet build` no longer requires the Tizen workload unless explicitly enabled. |
| `source/SkiaSharp.Build.Override.in.props` | 34 | related | Template override file documents how to enable Tizen locally with `<IsNetTizenSupported>true</IsNetTizenSupported>` and notes it requires the separate Samsung Tizen workload installation. |

### Workarounds

- Pass `/pIsNetTizenSupported:false` to `dotnet build` to skip Tizen projects.
- Install Tizen .NET workload via Samsung's installer: https://github.com/Samsung/Tizen.NET/wiki/Installing-Tizen-.NET-Workload

### Resolution Proposals

**Hypothesis:** The issue is fixed in current main: local builds default to IsNetTizenSupported=false. For 2.88.4, the workaround is to pass /pIsNetTizenSupported:false or install the Tizen workload via Samsung's script.

1. **Close as fixed in current main** — fix, confidence 0.85 (85%), cost/xs, validated=untested
   - The build system now defaults IsNetTizenSupported=false for local builds. New users building from main will not encounter this error.

**Recommended proposal:** Close as fixed in current main

**Why:** The root cause (unconditional Tizen TFM in local builds) is resolved in the current codebase. The issue was already answered by the maintainer.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.80 (80%) |
| Reason | The current main branch defaults Tizen to disabled for local builds, so the reported error no longer occurs. Maintainer already provided a working workaround for 2.88.4. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Correct type label from bug to question; add area/Build and os/Tizen labels | labels=type/question, area/Build, os/Windows-Classic, os/Tizen |
| add-comment | medium | 0.80 (80%) | Confirm the issue is resolved in current main and point to Tizen install docs | — |
| close-issue | medium | 0.80 (80%) | Close as fixed — resolved in current main branch | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! This build setup issue is now resolved in the current version of SkiaSharp: local builds (`dotnet build` without CI) automatically disable Tizen support unless you explicitly opt in, so you no longer need the Tizen workload installed to build locally.

If you are still on **2.88.x** and want to build with Tizen, you can:
1. **Skip Tizen:** Pass `/p:IsNetTizenSupported=false` to your `dotnet build` command.
2. **Install Tizen workload:** Follow [Samsung's Tizen .NET workload install guide](https://github.com/Samsung/Tizen.NET/wiki/Installing-Tizen-.NET-Workload).

Closing as resolved in the current codebase. Please reopen if you see this on a recent build.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2579,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T23:03:36Z",
    "currentLabels": [
      "type/bug",
      "status/needs-attention"
    ]
  },
  "summary": "Reporter building SkiaSharp 2.88.4 from source on Windows gets NETSDK1139 because the Tizen workload is not in the standard .NET SDK manifest and must be installed via Samsung's separate installer; current main branch already defaults IsNetTizenSupported=false for local builds, resolving this.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.78
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic",
      "os/Tizen"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Clone SkiaSharp 2.88.4 source",
        "Run `dotnet build` in the `source/` directory on Windows without Tizen workload",
        "Observe NETSDK1139: target platform identifier 'tizen' not recognized"
      ],
      "environmentDetails": "Windows 11, Visual Studio Community 17.7.2, .NET 8.0.100-preview.7.23376.3",
      "codeSnippets": [
        "error NETSDK1139: The target platform identifier tizen was not recognized. [binding/SkiaSharp.NativeAssets.Tizen/SkiaSharp.NativeAssets.Tizen.csproj::TargetFramework=net7.0-tizen]"
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.4"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "Current main branch defaults IsNetTizenSupported=false for local (non-CI) builds, so the error no longer occurs without the Tizen workload."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.85,
      "reason": "source/SkiaSharp.Build.props now conditionally disables Tizen for local builds: IsNetTizenSupported defaults to false when IsCI=false and BuildEverything is not set."
    }
  },
  "analysis": {
    "summary": "The reporter cloned SkiaSharp 2.88.4 and attempted a local `dotnet build`. The build system included Tizen TFMs unconditionally, but the Tizen SDK is not part of the standard .NET workload manifest — it must be installed via Samsung's own installer. The maintainer provided a workaround (`/pIsNetTizenSupported:false`). The current codebase has since been improved: `source/SkiaSharp.Build.props` now sets `IsNetTizenSupported=false` by default for local builds, so new contributors no longer hit this error.",
    "rationale": "This is primarily a setup/environment question about building SkiaSharp from source: the reporter asks 'What might be the problem? Is there something I can do?' The root build issue is fixed in main. The maintainer already answered with a workaround, and a community member later posted the Samsung Tizen installation link.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Build.props",
        "lines": "28-39",
        "finding": "Current code conditionally sets IsNetTizenSupported: true for CI/BuildEverything, false for local builds by default. This means local `dotnet build` no longer requires the Tizen workload unless explicitly enabled.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Build.Override.in.props",
        "lines": "34",
        "finding": "Template override file documents how to enable Tizen locally with `<IsNetTizenSupported>true</IsNetTizenSupported>` and notes it requires the separate Samsung Tizen workload installation.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "What might be the problem? Is there something I can do to build SkiaSharp?",
        "source": "issue body",
        "interpretation": "Reporter is asking for setup guidance, not reporting a regression in shipped SkiaSharp functionality."
      },
      {
        "text": "This does mean that for some reason tizen was not installed correctly. I usually use the script installer. But, you can skip it for now by passing `/pIsNetTizenSupported:false` on the build.",
        "source": "comment #1690611679 (maintainer mattleibow)",
        "interpretation": "Maintainer confirmed Tizen must be installed via script installer and provided a workaround. Issue was already answered."
      },
      {
        "text": "For those of you curious on how to get the Tizen Workload installed, go here: https://github.com/Samsung/Tizen.NET/wiki/Installing-Tizen-.NET-Workload",
        "source": "comment #2381538804 (community member cyraid)",
        "interpretation": "Community confirmed the Tizen workload install steps are documented at Samsung's wiki."
      }
    ],
    "workarounds": [
      "Pass `/pIsNetTizenSupported:false` to `dotnet build` to skip Tizen projects.",
      "Install Tizen .NET workload via Samsung's installer: https://github.com/Samsung/Tizen.NET/wiki/Installing-Tizen-.NET-Workload"
    ],
    "resolution": {
      "hypothesis": "The issue is fixed in current main: local builds default to IsNetTizenSupported=false. For 2.88.4, the workaround is to pass /pIsNetTizenSupported:false or install the Tizen workload via Samsung's script.",
      "proposals": [
        {
          "title": "Close as fixed in current main",
          "description": "The build system now defaults IsNetTizenSupported=false for local builds. New users building from main will not encounter this error.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Close as fixed in current main",
      "recommendedReason": "The root cause (unconditional Tizen TFM in local builds) is resolved in the current codebase. The issue was already answered by the maintainer."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.8,
      "reason": "The current main branch defaults Tizen to disabled for local builds, so the reported error no longer occurs. Maintainer already provided a working workaround for 2.88.4.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct type label from bug to question; add area/Build and os/Tizen labels",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/question",
          "area/Build",
          "os/Windows-Classic",
          "os/Tizen"
        ]
      },
      {
        "type": "add-comment",
        "description": "Confirm the issue is resolved in current main and point to Tizen install docs",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the report! This build setup issue is now resolved in the current version of SkiaSharp: local builds (`dotnet build` without CI) automatically disable Tizen support unless you explicitly opt in, so you no longer need the Tizen workload installed to build locally.\n\nIf you are still on **2.88.x** and want to build with Tizen, you can:\n1. **Skip Tizen:** Pass `/p:IsNetTizenSupported=false` to your `dotnet build` command.\n2. **Install Tizen workload:** Follow [Samsung's Tizen .NET workload install guide](https://github.com/Samsung/Tizen.NET/wiki/Installing-Tizen-.NET-Workload).\n\nClosing as resolved in the current codebase. Please reopen if you see this on a recent build."
      },
      {
        "type": "close-issue",
        "description": "Close as fixed — resolved in current main branch",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
