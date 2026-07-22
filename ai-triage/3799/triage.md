# Issue Triage Report — #3799

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-07-22T05:20:00Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/libSkiaSharp.native (0.88 (88%)) |
| Suggested action | close-as-fixed (0.75 (75%)) |

**Issue Summary:** SKGLView crashes on Windows .NET 9 with a FileNotFoundException for SkiaSharp.Views.WinUI.Native.Projection v4.147.0 because the NativeAssets.WinUI package did not include the Projection DLL under the net9.0 lib folder, only under net10.0.

**Analysis:** The SkiaSharp.NativeAssets.WinUI package in 4.147.0-preview.1.1 only placed the WinUI.Native.Projection DLL in the net10.0 lib folder, omitting the net9.0 folder. .NET 9 apps could not find the assembly at runtime. Current main adds the missing net9 entries to the packaging csproj.

**Recommendations:** **close-as-fixed** — Current main includes the Projection DLL under lib/net9.0-windows10.0.19041.0 for all RIDs in SkiaSharp.NativeAssets.WinUI. The issue appears fixed but should be confirmed against a published build.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Windows-WinUI |
| Backends | — |
| Tenets | tenet/reliability |
| Perf | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a .NET 9 MAUI app for Windows
2. Reference SkiaSharp.Views.Maui.Controls 4.147.0-preview.1.1
3. Add SKGLView to a ContentPage
4. Build and run on Windows — observe FileNotFoundException at startup

**Environment:** Windows 11, .NET 9, SkiaSharp 4.147.0-preview.1.1

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | exception |
| Error message | Could not load file or assembly 'SkiaSharp.Views.WinUI.Native.Projection, Version=4.147.0.0'. The system cannot find the file specified. |
| Repro quality | complete |
| Target frameworks | net9.0-windows10.0.19041.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 4.147.0-preview.1.1, 3.118.0-preview.2 |
| Worked in | 3.118.0-preview.2 |
| Broke in | 4.147.0-preview.1.1 |
| Current relevance | unlikely |
| Relevance reason | Current main (binding/SkiaSharp.NativeAssets.WinUI/SkiaSharp.NativeAssets.WinUI.csproj) now includes the Projection DLL under both lib/$(WindowsTargetFrameworksCurrent) (net10) and lib/$(WindowsTargetFrameworksPrevious) (net9), indicating the fix was applied after 4.147.0-preview.1.1. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.92 (92%) |
| Reason | Last known good 3.118.0-preview.2 worked; 4.147.0-preview.1.1 broke. Current main contains the fix. |
| Worked in version | 3.118.0-preview.2 |
| Broke in version | 4.147.0-preview.1.1 |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.82 (82%) |
| Reason | binding/SkiaSharp.NativeAssets.WinUI/SkiaSharp.NativeAssets.WinUI.csproj in main now includes NativeAssetPackageFile entries for lib/$(WindowsTargetFrameworksPrevious) (net9.0-windows10.0.19041.0) alongside lib/$(WindowsTargetFrameworksCurrent) (net10.0-windows10.0.19041.0) for all three RIDs (win-x64, win-x86, win-arm64), ensuring the Projection DLL is deployed for .NET 9 consumers. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

The SkiaSharp.NativeAssets.WinUI package in 4.147.0-preview.1.1 only placed the WinUI.Native.Projection DLL in the net10.0 lib folder, omitting the net9.0 folder. .NET 9 apps could not find the assembly at runtime. Current main adds the missing net9 entries to the packaging csproj.

### Rationale

This is a high-confidence type/bug in area/libSkiaSharp.native because the root cause is a missing native asset entry in the NuGet packaging project (NativeAssets.WinUI.csproj) — specifically, the net9.0 lib folder was not populated with the Projection DLL in v4.147.0-preview.1.1. Current main adds the missing entries, suggesting close-as-fixed is appropriate with a note to verify in the next shipped preview.

### Key Signals

- "File SkiaSharp.Views.WinUI.Native.Projection.dll is not inside the bin folder for .net9, while it is well present there for .net10." — **issue body** (Direct confirmation that the assembly is missing for net9 only — classic missing native asset for that TFM.)
- "Last Known Good Version: 3.118.0-preview.2" — **issue body** (Regressed when migrating from 3.x to 4.x series. Likely related to the .NET 10 TFM migration where net9 entries may not have been added immediately.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.WinUI/SkiaSharp.NativeAssets.WinUI.csproj` | 10, 13, 16 | direct | Contains NativeAssetPackageFile entries for 'lib/$(WindowsTargetFrameworksPrevious)' (resolves to net9.0-windows10.0.19041.0) for all three RIDs. This ensures the Projection DLL is packaged for .NET 9 consumers. |
| `source/SkiaSharp.Build.props` | 64, 66, 145, 146 | direct | TFMPrevious=net9.0, TFMCurrent=net10.0. WindowsTargetFrameworksPrevious=net9.0-windows10.0.19041.0 (set only when IsWindows=true). This variable drives the net9 lib folder path in the NativeAssets.WinUI package. |
| `native/winui/build.cake` | — | related | The WinUI native build hard-codes net9.0-windows10.0.19041.0 as the output path for the Projection DLL, placing it in output/native/winui/any/. This is the file included by NativeAssets.WinUI.csproj. |
| `native/winui/SkiaSharp.Views.WinUI.Native/SkiaSharp.Views.WinUI.Native.Projection/SkiaSharp.Views.WinUI.Native.Projection.csproj` | — | related | TargetFramework=$(WindowsTargetFrameworksPrevious) which evaluates to net9.0-windows10.0.19041.0. Generates the Projection DLL via CsWinRT. |

### Next Questions

- Was the $(WindowsTargetFrameworksPrevious) line recently added to NativeAssets.WinUI.csproj or was it always there (shallow clone prevents full history check)?
- Does the issue reproduce with SkiaSharp >= 4.151.0-preview builds?

### Resolution Proposals

**Hypothesis:** In 4.147.0-preview.1.1, the NativeAssets.WinUI.csproj only included the Projection DLL for WindowsTargetFrameworksCurrent (net10) and not for WindowsTargetFrameworksPrevious (net9). The fix is already present in main (lines 10, 13, 16 of the csproj add the Previous TFM entries).

1. **Verify fix ships in next preview** — investigation, cost/xs, validated=untested
   - Confirm that a build from current main (VERSIONS.txt shows 4.151.0) correctly packages SkiaSharp.Views.WinUI.Native.Projection.dll under lib/net9.0-windows10.0.19041.0/ in SkiaSharp.NativeAssets.WinUI. Close the issue once confirmed.

**Recommended proposal:** Verify fix ships in next preview

**Why:** The code fix is already in main. Only verification that it ships is needed before closing.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.75 (75%) |
| Reason | Current main includes the Projection DLL under lib/net9.0-windows10.0.19041.0 for all RIDs in SkiaSharp.NativeAssets.WinUI. The issue appears fixed but should be confirmed against a published build. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply classification labels | labels=type/bug, area/libSkiaSharp.native, os/Windows-WinUI, tenet/reliability, triage/triaged |
| add-comment | medium | 0.75 (75%) | Inform reporter of likely fix in newer preview and ask to verify | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the clear report, @taublast! 🔍

We investigated the root cause: in `4.147.0-preview.1.1`, the `SkiaSharp.NativeAssets.WinUI` package did not include `SkiaSharp.Views.WinUI.Native.Projection.dll` under the `lib/net9.0-windows10.0.19041.0/` folder — only under the `net10.0` folder.

The fix is already in `main` ([`binding/SkiaSharp.NativeAssets.WinUI/SkiaSharp.NativeAssets.WinUI.csproj`](https://github.com/mono/SkiaSharp/blob/main/binding/SkiaSharp.NativeAssets.WinUI/SkiaSharp.NativeAssets.WinUI.csproj) now packages the Projection DLL for both `net9.0` and `net10.0`).

**Could you please verify** whether this issue is resolved in a newer preview build (≥ `4.148.0-preview` or `4.151.0`)? If it is, we'll close this as fixed. If not, please let us know and we'll investigate further.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3799,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-07-22T05:20:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKGLView crashes on Windows .NET 9 with a FileNotFoundException for SkiaSharp.Views.WinUI.Native.Projection v4.147.0 because the NativeAssets.WinUI package did not include the Projection DLL under the net9.0 lib folder, only under net10.0.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.88
    },
    "platforms": [
      "os/Windows-WinUI"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "exception",
      "errorMessage": "Could not load file or assembly 'SkiaSharp.Views.WinUI.Native.Projection, Version=4.147.0.0'. The system cannot find the file specified.",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net9.0-windows10.0.19041.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET 9 MAUI app for Windows",
        "Reference SkiaSharp.Views.Maui.Controls 4.147.0-preview.1.1",
        "Add SKGLView to a ContentPage",
        "Build and run on Windows — observe FileNotFoundException at startup"
      ],
      "environmentDetails": "Windows 11, .NET 9, SkiaSharp 4.147.0-preview.1.1",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "4.147.0-preview.1.1",
        "3.118.0-preview.2"
      ],
      "workedIn": "3.118.0-preview.2",
      "brokeIn": "4.147.0-preview.1.1",
      "currentRelevance": "unlikely",
      "relevanceReason": "Current main (binding/SkiaSharp.NativeAssets.WinUI/SkiaSharp.NativeAssets.WinUI.csproj) now includes the Projection DLL under both lib/$(WindowsTargetFrameworksCurrent) (net10) and lib/$(WindowsTargetFrameworksPrevious) (net9), indicating the fix was applied after 4.147.0-preview.1.1."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.92,
      "reason": "Last known good 3.118.0-preview.2 worked; 4.147.0-preview.1.1 broke. Current main contains the fix.",
      "workedInVersion": "3.118.0-preview.2",
      "brokeInVersion": "4.147.0-preview.1.1"
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.82,
      "reason": "binding/SkiaSharp.NativeAssets.WinUI/SkiaSharp.NativeAssets.WinUI.csproj in main now includes NativeAssetPackageFile entries for lib/$(WindowsTargetFrameworksPrevious) (net9.0-windows10.0.19041.0) alongside lib/$(WindowsTargetFrameworksCurrent) (net10.0-windows10.0.19041.0) for all three RIDs (win-x64, win-x86, win-arm64), ensuring the Projection DLL is deployed for .NET 9 consumers."
    }
  },
  "analysis": {
    "summary": "The SkiaSharp.NativeAssets.WinUI package in 4.147.0-preview.1.1 only placed the WinUI.Native.Projection DLL in the net10.0 lib folder, omitting the net9.0 folder. .NET 9 apps could not find the assembly at runtime. Current main adds the missing net9 entries to the packaging csproj.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.WinUI/SkiaSharp.NativeAssets.WinUI.csproj",
        "finding": "Contains NativeAssetPackageFile entries for 'lib/$(WindowsTargetFrameworksPrevious)' (resolves to net9.0-windows10.0.19041.0) for all three RIDs. This ensures the Projection DLL is packaged for .NET 9 consumers.",
        "relevance": "direct",
        "lines": "10, 13, 16"
      },
      {
        "file": "source/SkiaSharp.Build.props",
        "finding": "TFMPrevious=net9.0, TFMCurrent=net10.0. WindowsTargetFrameworksPrevious=net9.0-windows10.0.19041.0 (set only when IsWindows=true). This variable drives the net9 lib folder path in the NativeAssets.WinUI package.",
        "relevance": "direct",
        "lines": "64, 66, 145, 146"
      },
      {
        "file": "native/winui/build.cake",
        "finding": "The WinUI native build hard-codes net9.0-windows10.0.19041.0 as the output path for the Projection DLL, placing it in output/native/winui/any/. This is the file included by NativeAssets.WinUI.csproj.",
        "relevance": "related"
      },
      {
        "file": "native/winui/SkiaSharp.Views.WinUI.Native/SkiaSharp.Views.WinUI.Native.Projection/SkiaSharp.Views.WinUI.Native.Projection.csproj",
        "finding": "TargetFramework=$(WindowsTargetFrameworksPrevious) which evaluates to net9.0-windows10.0.19041.0. Generates the Projection DLL via CsWinRT.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "File SkiaSharp.Views.WinUI.Native.Projection.dll is not inside the bin folder for .net9, while it is well present there for .net10.",
        "source": "issue body",
        "interpretation": "Direct confirmation that the assembly is missing for net9 only — classic missing native asset for that TFM."
      },
      {
        "text": "Last Known Good Version: 3.118.0-preview.2",
        "source": "issue body",
        "interpretation": "Regressed when migrating from 3.x to 4.x series. Likely related to the .NET 10 TFM migration where net9 entries may not have been added immediately."
      }
    ],
    "rationale": "This is a high-confidence type/bug in area/libSkiaSharp.native because the root cause is a missing native asset entry in the NuGet packaging project (NativeAssets.WinUI.csproj) — specifically, the net9.0 lib folder was not populated with the Projection DLL in v4.147.0-preview.1.1. Current main adds the missing entries, suggesting close-as-fixed is appropriate with a note to verify in the next shipped preview.",
    "resolution": {
      "hypothesis": "In 4.147.0-preview.1.1, the NativeAssets.WinUI.csproj only included the Projection DLL for WindowsTargetFrameworksCurrent (net10) and not for WindowsTargetFrameworksPrevious (net9). The fix is already present in main (lines 10, 13, 16 of the csproj add the Previous TFM entries).",
      "proposals": [
        {
          "title": "Verify fix ships in next preview",
          "category": "investigation",
          "description": "Confirm that a build from current main (VERSIONS.txt shows 4.151.0) correctly packages SkiaSharp.Views.WinUI.Native.Projection.dll under lib/net9.0-windows10.0.19041.0/ in SkiaSharp.NativeAssets.WinUI. Close the issue once confirmed.",
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Verify fix ships in next preview",
      "recommendedReason": "The code fix is already in main. Only verification that it ships is needed before closing."
    },
    "nextQuestions": [
      "Was the $(WindowsTargetFrameworksPrevious) line recently added to NativeAssets.WinUI.csproj or was it always there (shallow clone prevents full history check)?",
      "Does the issue reproduce with SkiaSharp >= 4.151.0-preview builds?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.75,
      "reason": "Current main includes the Projection DLL under lib/net9.0-windows10.0.19041.0 for all RIDs in SkiaSharp.NativeAssets.WinUI. The issue appears fixed but should be confirmed against a published build.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply classification labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Windows-WinUI",
          "tenet/reliability",
          "triage/triaged"
        ]
      },
      {
        "type": "add-comment",
        "description": "Inform reporter of likely fix in newer preview and ask to verify",
        "risk": "medium",
        "confidence": 0.75,
        "comment": "Thanks for the clear report, @taublast! 🔍\n\nWe investigated the root cause: in `4.147.0-preview.1.1`, the `SkiaSharp.NativeAssets.WinUI` package did not include `SkiaSharp.Views.WinUI.Native.Projection.dll` under the `lib/net9.0-windows10.0.19041.0/` folder — only under the `net10.0` folder.\n\nThe fix is already in `main` ([`binding/SkiaSharp.NativeAssets.WinUI/SkiaSharp.NativeAssets.WinUI.csproj`](https://github.com/mono/SkiaSharp/blob/main/binding/SkiaSharp.NativeAssets.WinUI/SkiaSharp.NativeAssets.WinUI.csproj) now packages the Projection DLL for both `net9.0` and `net10.0`).\n\n**Could you please verify** whether this issue is resolved in a newer preview build (≥ `4.148.0-preview` or `4.151.0`)? If it is, we'll close this as fixed. If not, please let us know and we'll investigate further."
      }
    ]
  }
}
```

</details>
