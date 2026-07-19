# Issue Triage Report — #3751

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-07-19T05:14:00Z |
| Type | type/enhancement (0.82 (82%)) |
| Area | area/Build (0.90 (90%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** The SkiaSharp NuGet package incorrectly lists SkiaSharp.NativeAssets.macOS as a dependency for .NETFramework 4.6.2 and .NETFramework 4.8 TFMs, even though .NET Framework only runs on Windows.

**Analysis:** In binding/SkiaSharp/SkiaSharp.csproj, the condition `!$(TargetFramework.Contains('-'))` causes both Win32 and macOS NativeAssets to be referenced for any TFM without a platform suffix, including net462 and net48. While this design is appropriate for netstandard TFMs (which are genuinely cross-platform), net462 and net48 are Windows-only by definition and do not need the macOS dependency.

**Recommendations:** **keep-open** — Valid packaging improvement request. Not a functional bug but a worthwhile cleanup to reduce unnecessary downloads and improve dependency clarity for .NET Framework users.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/Build |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Perf | — |
| Partner | — |

## Evidence

### Reproduction

1. Install SkiaSharp 3.119.4-preview.1.1 in a .NET Framework 4.6.2 or 4.8 project
2. Inspect NuGet dependency graph: both SkiaSharp.NativeAssets.Win32 and SkiaSharp.NativeAssets.macOS are listed as dependencies

**Environment:** SkiaSharp 3.119.4-preview.1.1, .NETFramework 4.6.2 / 4.8

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.119.4-preview.1.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The ProjectReference condition has not changed — the macOS dependency is still included for all non-platform-suffixed TFMs including net462 and net48. |

## Analysis

### Technical Summary

In binding/SkiaSharp/SkiaSharp.csproj, the condition `!$(TargetFramework.Contains('-'))` causes both Win32 and macOS NativeAssets to be referenced for any TFM without a platform suffix, including net462 and net48. While this design is appropriate for netstandard TFMs (which are genuinely cross-platform), net462 and net48 are Windows-only by definition and do not need the macOS dependency.

### Rationale

This is classified as type/enhancement rather than type/bug because the package is functionally correct — the macOS native library is never loaded on Windows/.NET Framework. The issue is a packaging quality concern: it produces a misleading dependency list and causes unnecessary macOS binary downloads for .NET Framework users. The fix would require distinguishing net462/net48 from netstandard in the ProjectReference conditions.

### Key Signals

- ".NETFramework 4.6.2
SkiaSharp.NativeAssets.macOS (>= 3.119.4-preview.1.1)
SkiaSharp.NativeAssets.Win32 (>= 3.119.4-preview.1.1)" — **issue body** (macOS native assets are listed as a .NETFramework dependency — this is technically incorrect since .NET Framework only runs on Windows.)
- "Also, SkiaSharp.NativeAssets.macOS, SkiaSharp.NativeAssets.Linux and SkiaSharp.NativeAssets.Linux.NoDependencies didn't need to support .NET Framework" — **issue body** (Reporter suggests removing non-Windows NativeAssets packages from .NET Framework TFM dependencies entirely, not just macOS.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaSharp.csproj` | 20-29 | direct | ProjectReference to SkiaSharp.NativeAssets.macOS uses Condition `$(TargetFramework.Contains('-macos')) or !$(TargetFramework.Contains('-'))`. The second clause `!Contains('-')` is true for net462, net48, netstandard2.0, netstandard2.1, and net10.0, causing macOS assets to be included for all of them including Windows-only .NET Framework TFMs. |
| `source/SkiaSharp.Build.props` | 1-50 | related | FullFrameworkTargetFrameworks is explicitly defined as net462;net48. These are included under BasicTargetFrameworks alongside netstandard TFMs. No special treatment is applied to exclude platform-specific NativeAssets from .NET Framework TFMs. |

### Next Questions

- Is there a design reason to intentionally include macOS/Linux assets for .NET Framework TFMs (e.g., tooling compatibility)?
- Does adding platform conditions for net462/net48 break any downstream scenarios?

### Resolution Proposals

**Hypothesis:** The ProjectReference condition for NativeAssets packages should be tightened to exclude macOS and Linux for .NETFramework TFMs.

1. **Restrict NativeAssets references by TFM type** — fix, confidence 0.75 (75%), cost/s, validated=untested
   - Update the Condition on the NativeAssets ProjectReferences in SkiaSharp.csproj to exclude macOS/Linux for .NETFramework TFMs. For example, change `!$(TargetFramework.Contains('-'))` to also exclude `net4` prefix: `!$(TargetFramework.Contains('-')) and !$(TargetFramework.StartsWith('net4'))`; then add a separate Win32-only reference for net4xx TFMs.

**Recommended proposal:** Restrict NativeAssets references by TFM type

**Why:** Straightforward MSBuild condition change. Eliminates unnecessary cross-platform dependency for Windows-only TFMs without changing runtime behavior.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Valid packaging improvement request. Not a functional bug but a worthwhile cleanup to reduce unnecessary downloads and improve dependency clarity for .NET Framework users. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply enhancement, build area, Windows platform, and compatibility tenet labels | labels=type/enhancement, area/Build, os/Windows-Classic, tenet/compatibility |
| add-comment | medium | 0.80 (80%) | Acknowledge the valid observation and explain the design | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed observation! You're correct that `.NETFramework` target frameworks (net462, net48) only run on Windows, so `SkiaSharp.NativeAssets.macOS` should not be a required dependency for those TFMs.

In `binding/SkiaSharp/SkiaSharp.csproj`, the `ProjectReference` condition for `SkiaSharp.NativeAssets.macOS` uses `!$(TargetFramework.Contains('-'))` to cover all non-platform-specific TFMs (netstandard2.0, netstandard2.1, net462, net48, net10.0). While this is correct for `netstandard` TFMs (which run cross-platform), it's overly broad for `.NETFramework` TFMs.

This is a valid packaging improvement — fixing it would reduce unnecessary downloads for .NET Framework users. We'll keep this open as an enhancement.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3751,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-07-19T05:14:00Z"
  },
  "summary": "The SkiaSharp NuGet package incorrectly lists SkiaSharp.NativeAssets.macOS as a dependency for .NETFramework 4.6.2 and .NETFramework 4.8 TFMs, even though .NET Framework only runs on Windows.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.82
    },
    "area": {
      "value": "area/Build",
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
      "stepsToReproduce": [
        "Install SkiaSharp 3.119.4-preview.1.1 in a .NET Framework 4.6.2 or 4.8 project",
        "Inspect NuGet dependency graph: both SkiaSharp.NativeAssets.Win32 and SkiaSharp.NativeAssets.macOS are listed as dependencies"
      ],
      "environmentDetails": "SkiaSharp 3.119.4-preview.1.1, .NETFramework 4.6.2 / 4.8",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.119.4-preview.1.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The ProjectReference condition has not changed — the macOS dependency is still included for all non-platform-suffixed TFMs including net462 and net48."
    }
  },
  "analysis": {
    "summary": "In binding/SkiaSharp/SkiaSharp.csproj, the condition `!$(TargetFramework.Contains('-'))` causes both Win32 and macOS NativeAssets to be referenced for any TFM without a platform suffix, including net462 and net48. While this design is appropriate for netstandard TFMs (which are genuinely cross-platform), net462 and net48 are Windows-only by definition and do not need the macOS dependency.",
    "rationale": "This is classified as type/enhancement rather than type/bug because the package is functionally correct — the macOS native library is never loaded on Windows/.NET Framework. The issue is a packaging quality concern: it produces a misleading dependency list and causes unnecessary macOS binary downloads for .NET Framework users. The fix would require distinguishing net462/net48 from netstandard in the ProjectReference conditions.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaSharp.csproj",
        "lines": "20-29",
        "finding": "ProjectReference to SkiaSharp.NativeAssets.macOS uses Condition `$(TargetFramework.Contains('-macos')) or !$(TargetFramework.Contains('-'))`. The second clause `!Contains('-')` is true for net462, net48, netstandard2.0, netstandard2.1, and net10.0, causing macOS assets to be included for all of them including Windows-only .NET Framework TFMs.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Build.props",
        "lines": "1-50",
        "finding": "FullFrameworkTargetFrameworks is explicitly defined as net462;net48. These are included under BasicTargetFrameworks alongside netstandard TFMs. No special treatment is applied to exclude platform-specific NativeAssets from .NET Framework TFMs.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": ".NETFramework 4.6.2\nSkiaSharp.NativeAssets.macOS (>= 3.119.4-preview.1.1)\nSkiaSharp.NativeAssets.Win32 (>= 3.119.4-preview.1.1)",
        "source": "issue body",
        "interpretation": "macOS native assets are listed as a .NETFramework dependency — this is technically incorrect since .NET Framework only runs on Windows."
      },
      {
        "text": "Also, SkiaSharp.NativeAssets.macOS, SkiaSharp.NativeAssets.Linux and SkiaSharp.NativeAssets.Linux.NoDependencies didn't need to support .NET Framework",
        "source": "issue body",
        "interpretation": "Reporter suggests removing non-Windows NativeAssets packages from .NET Framework TFM dependencies entirely, not just macOS."
      }
    ],
    "nextQuestions": [
      "Is there a design reason to intentionally include macOS/Linux assets for .NET Framework TFMs (e.g., tooling compatibility)?",
      "Does adding platform conditions for net462/net48 break any downstream scenarios?"
    ],
    "resolution": {
      "hypothesis": "The ProjectReference condition for NativeAssets packages should be tightened to exclude macOS and Linux for .NETFramework TFMs.",
      "proposals": [
        {
          "title": "Restrict NativeAssets references by TFM type",
          "description": "Update the Condition on the NativeAssets ProjectReferences in SkiaSharp.csproj to exclude macOS/Linux for .NETFramework TFMs. For example, change `!$(TargetFramework.Contains('-'))` to also exclude `net4` prefix: `!$(TargetFramework.Contains('-')) and !$(TargetFramework.StartsWith('net4'))`; then add a separate Win32-only reference for net4xx TFMs.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Restrict NativeAssets references by TFM type",
      "recommendedReason": "Straightforward MSBuild condition change. Eliminates unnecessary cross-platform dependency for Windows-only TFMs without changing runtime behavior."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.8,
      "reason": "Valid packaging improvement request. Not a functional bug but a worthwhile cleanup to reduce unnecessary downloads and improve dependency clarity for .NET Framework users.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement, build area, Windows platform, and compatibility tenet labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/enhancement",
          "area/Build",
          "os/Windows-Classic",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the valid observation and explain the design",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the detailed observation! You're correct that `.NETFramework` target frameworks (net462, net48) only run on Windows, so `SkiaSharp.NativeAssets.macOS` should not be a required dependency for those TFMs.\n\nIn `binding/SkiaSharp/SkiaSharp.csproj`, the `ProjectReference` condition for `SkiaSharp.NativeAssets.macOS` uses `!$(TargetFramework.Contains('-'))` to cover all non-platform-specific TFMs (netstandard2.0, netstandard2.1, net462, net48, net10.0). While this is correct for `netstandard` TFMs (which run cross-platform), it's overly broad for `.NETFramework` TFMs.\n\nThis is a valid packaging improvement — fixing it would reduce unnecessary downloads for .NET Framework users. We'll keep this open as an enhancement."
      }
    ]
  }
}
```

</details>
