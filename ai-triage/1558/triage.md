# Issue Triage Report — #1558

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T04:38:58Z |
| Type | type/bug (0.85 (85%)) |
| Area | area/Build (0.95 (95%)) |
| Suggested action | close-as-fixed (0.82 (82%)) |

**Issue Summary:** Build error CS1501 on line 47 of native/linux/build.cake when running bootstrapper.ps1 on Windows 10 host: 'No overload for method Contains takes 2 arguments' — likely a Cake version or .NET version incompatibility that has since been resolved by rewriting the build scripts.

**Analysis:** This 2020 build error occurred because native/linux/build.cake called string.Contains(string, StringComparison) which requires .NET Core 2.1+ or .NET 5+ and is unavailable in .NET Framework-based Cake runners. The entire native/linux/build.cake file has since been rewritten and no longer contains this pattern, so the issue is almost certainly resolved in current versions.

**Recommendations:** **close-as-fixed** — The native/linux/build.cake file has been completely rewritten since this 2020 issue. The CS1501 error is no longer present in the current codebase.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/Build |
| Platforms | os/Linux, os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Clone SkiaSharp on a Windows 10 host
2. Run .\bootstrapper.ps1 -Target everything in PowerShell
3. Observe CS1501 error in native/linux/build.cake at line 47

**Environment:** Windows 10 host, building externals-linux target, circa December 2020

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | build-error |
| Error message | CS1501: No overload for method 'Contains' takes 2 arguments |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | The native/linux/build.cake file has been completely rewritten since December 2020 and no longer contains the string.Contains(string, StringComparison) call that caused the error. The error was due to using a StringComparison overload not available in the .NET Framework / older Cake runtime. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.88 (88%) |
| Reason | The native/linux/build.cake file has been completely rewritten since this 2020 issue was filed. The old line 47 containing the two-argument Contains call no longer exists. The current file uses different patterns for any similar checks. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

This 2020 build error occurred because native/linux/build.cake called string.Contains(string, StringComparison) which requires .NET Core 2.1+ or .NET 5+ and is unavailable in .NET Framework-based Cake runners. The entire native/linux/build.cake file has since been rewritten and no longer contains this pattern, so the issue is almost certainly resolved in current versions.

### Rationale

Classified as type/bug because the build script was genuinely broken due to a .NET version incompatibility (CS1501). Area is area/Build since it's a Cake build script issue, not SkiaSharp runtime code. Marked low severity because it only affects developers trying to build native binaries from source (not end users), and the issue appears to be fixed in the current codebase. suggestedAction is close-as-fixed since the build.cake file has been completely rewritten.

### Key Signals

- "C:/MyDevProjects/MyGitHub/SkiaSharp/native/linux/build.cake(47,31): error CS1501: No overload for method 'Contains' takes 2 arguments" — **issue body** (string.Contains(string, StringComparison) was called but the Cake/.NET runtime didn't support it — typical of .NET Framework 4.x based build runners which lack this overload)
- "Ran .\bootstrapper.ps1 -Target everything in power shell" — **issue body** (Reporter is building on Windows host with -Target everything, triggering the externals-linux build step)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `native/linux/build.cake` | — | direct | Current file (2026) is completely different from the 2020 version. It no longer has a 'Contains' call with two arguments at line 47 or anywhere in the file. The file now uses different logic entirely (GPU/Vulkan flags, architecture configuration, CheckLinuxDependencies helper). |
| `scripts/cake/native-shared.cake` | — | related | The build scripts were refactored and now use a shared native-shared.cake pattern. The problematic Contains(string, StringComparison) call was removed as part of a build system overhaul. |

### Resolution Proposals

**Hypothesis:** The string.Contains(string, StringComparison) overload was used in native/linux/build.cake circa 2020, which is only available in .NET Core 2.1+ / .NET 5+. The Cake bootstrapper was using a .NET Framework-based runner that didn't have this overload. The build scripts have since been completely rewritten and this specific issue no longer exists.

1. **Close as fixed** — fix, cost/xs, validated=untested
   - The native/linux/build.cake file has been completely rewritten since 2020. The CS1501 error no longer exists in the current code. Close with a note that the issue is resolved in newer versions.

**Recommended proposal:** close-fixed

**Why:** Code investigation confirms the problematic pattern is gone from the current build scripts. Keeping this open adds noise to the backlog.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.82 (82%) |
| Reason | The native/linux/build.cake file has been completely rewritten since this 2020 issue. The CS1501 error is no longer present in the current codebase. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/Build, os/Linux, os/Windows-Classic, tenet/compatibility | labels=type/bug, area/Build, os/Linux, os/Windows-Classic, tenet/compatibility |
| close-issue | medium | 0.82 (82%) | Close as completed since the build scripts have been rewritten and the error no longer exists | stateReason=completed |
| add-comment | medium | 0.82 (82%) | Inform reporter that the build scripts have been rewritten and the issue is resolved | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report.

The `native/linux/build.cake` file has been completely rewritten since this was filed in December 2020. The `string.Contains(string, StringComparison)` overload that caused `CS1501` is no longer present — the build scripts were refactored as part of a broader build system overhaul.

If you're still seeing this error, please make sure you're using the latest version of the repository. If you run into a different build error on the latest code, feel free to open a new issue with the updated details.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1558,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T04:38:58Z"
  },
  "summary": "Build error CS1501 on line 47 of native/linux/build.cake when running bootstrapper.ps1 on Windows 10 host: 'No overload for method Contains takes 2 arguments' — likely a Cake version or .NET version incompatibility that has since been resolved by rewriting the build scripts.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.85
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.95
    },
    "platforms": [
      "os/Linux",
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "low",
      "regressionClaimed": false,
      "errorType": "build-error",
      "errorMessage": "CS1501: No overload for method 'Contains' takes 2 arguments",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Clone SkiaSharp on a Windows 10 host",
        "Run .\\bootstrapper.ps1 -Target everything in PowerShell",
        "Observe CS1501 error in native/linux/build.cake at line 47"
      ],
      "environmentDetails": "Windows 10 host, building externals-linux target, circa December 2020"
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "unlikely",
      "relevanceReason": "The native/linux/build.cake file has been completely rewritten since December 2020 and no longer contains the string.Contains(string, StringComparison) call that caused the error. The error was due to using a StringComparison overload not available in the .NET Framework / older Cake runtime."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.88,
      "reason": "The native/linux/build.cake file has been completely rewritten since this 2020 issue was filed. The old line 47 containing the two-argument Contains call no longer exists. The current file uses different patterns for any similar checks."
    }
  },
  "analysis": {
    "summary": "This 2020 build error occurred because native/linux/build.cake called string.Contains(string, StringComparison) which requires .NET Core 2.1+ or .NET 5+ and is unavailable in .NET Framework-based Cake runners. The entire native/linux/build.cake file has since been rewritten and no longer contains this pattern, so the issue is almost certainly resolved in current versions.",
    "codeInvestigation": [
      {
        "file": "native/linux/build.cake",
        "finding": "Current file (2026) is completely different from the 2020 version. It no longer has a 'Contains' call with two arguments at line 47 or anywhere in the file. The file now uses different logic entirely (GPU/Vulkan flags, architecture configuration, CheckLinuxDependencies helper).",
        "relevance": "direct"
      },
      {
        "file": "scripts/cake/native-shared.cake",
        "finding": "The build scripts were refactored and now use a shared native-shared.cake pattern. The problematic Contains(string, StringComparison) call was removed as part of a build system overhaul.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "C:/MyDevProjects/MyGitHub/SkiaSharp/native/linux/build.cake(47,31): error CS1501: No overload for method 'Contains' takes 2 arguments",
        "source": "issue body",
        "interpretation": "string.Contains(string, StringComparison) was called but the Cake/.NET runtime didn't support it — typical of .NET Framework 4.x based build runners which lack this overload"
      },
      {
        "text": "Ran .\\bootstrapper.ps1 -Target everything in power shell",
        "source": "issue body",
        "interpretation": "Reporter is building on Windows host with -Target everything, triggering the externals-linux build step"
      }
    ],
    "rationale": "Classified as type/bug because the build script was genuinely broken due to a .NET version incompatibility (CS1501). Area is area/Build since it's a Cake build script issue, not SkiaSharp runtime code. Marked low severity because it only affects developers trying to build native binaries from source (not end users), and the issue appears to be fixed in the current codebase. suggestedAction is close-as-fixed since the build.cake file has been completely rewritten.",
    "resolution": {
      "hypothesis": "The string.Contains(string, StringComparison) overload was used in native/linux/build.cake circa 2020, which is only available in .NET Core 2.1+ / .NET 5+. The Cake bootstrapper was using a .NET Framework-based runner that didn't have this overload. The build scripts have since been completely rewritten and this specific issue no longer exists.",
      "proposals": [
        {
          "title": "Close as fixed",
          "description": "The native/linux/build.cake file has been completely rewritten since 2020. The CS1501 error no longer exists in the current code. Close with a note that the issue is resolved in newer versions.",
          "category": "fix",
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "close-fixed",
      "recommendedReason": "Code investigation confirms the problematic pattern is gone from the current build scripts. Keeping this open adds noise to the backlog."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.82,
      "reason": "The native/linux/build.cake file has been completely rewritten since this 2020 issue. The CS1501 error is no longer present in the current codebase.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/Build, os/Linux, os/Windows-Classic, tenet/compatibility",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/Build",
          "os/Linux",
          "os/Windows-Classic",
          "tenet/compatibility"
        ]
      },
      {
        "type": "close-issue",
        "description": "Close as completed since the build scripts have been rewritten and the error no longer exists",
        "risk": "medium",
        "confidence": 0.82,
        "stateReason": "completed"
      },
      {
        "type": "add-comment",
        "description": "Inform reporter that the build scripts have been rewritten and the issue is resolved",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the report.\n\nThe `native/linux/build.cake` file has been completely rewritten since this was filed in December 2020. The `string.Contains(string, StringComparison)` overload that caused `CS1501` is no longer present — the build scripts were refactored as part of a broader build system overhaul.\n\nIf you're still seeing this error, please make sure you're using the latest version of the repository. If you run into a different build error on the latest code, feel free to open a new issue with the updated details."
      }
    ]
  }
}
```

</details>
