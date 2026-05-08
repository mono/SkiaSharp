# Issue Triage Report — #1885

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T07:20:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/Build (0.88 (88%)) |
| Suggested action | close-as-fixed (0.82 (82%)) |

**Issue Summary:** Build error MSB4184 in SkiaSharp.NativeAssets.MacCatalyst.targets when deploying a MAUI application to Mac Catalyst using SkiaSharp 2.88.0-preview.155: the targets file's File.GetAttributes check fails because the libSkiaSharp.framework binary is not copied to the output path.

**Analysis:** The SkiaSharp.NativeAssets.MacCatalyst.targets file in version 2.88.0-preview.155 contained MSBuild inline C# that called File.GetAttributes() to check whether the libSkiaSharp framework binary had been copied into the app bundle. This check failed because the framework was never placed at that path during the Mac Catalyst build, completely blocking deployment. The current code no longer contains this check; the packaging was redesigned to distribute the framework as a .zip archive.

**Recommendations:** **close-as-fixed** — The targets file that produced the MSB4184 build error in 2.88.0-preview.155 has been completely reworked. The current SkiaSharp.NativeAssets.MacCatalyst targets file no longer contains the File.GetAttributes check. The issue is against a 4+-year-old preview build and is almost certainly resolved in current releases.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/Build |
| Platforms | os/macOS |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | partner/maui |
| Current labels | area/SkiaSharp.Views.Maui |

## Evidence

### Reproduction

1. Create a simple MAUI application
2. Set net6.0-maccatalyst (My Mac) as the target platform
3. Add SkiaSharp 2.88.0-preview.155 NuGet package
4. Build the application — MSB4184 error is produced

**Environment:** Visual Studio 2022 for Mac (Preview), macOS 12.0.1, MacBook Air M1, net6.0-maccatalyst

**Repository links:**
- https://github.com/mono/SkiaSharp/files/7674389/MauiAppSkia.zip — Attached minimal MAUI repro project from reporter

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | build-error |
| Error message | MSB4184: The expression "[System.IO.File]::GetAttributes(bin/Debug/net6.0-maccatalyst/maccatalyst-x64/MauiApp1.app/Contents/Frameworks/libSkiaSharp.framework/libSkiaSharp)" cannot be evaluated. Could not find a part of the path. |
| Repro quality | complete |
| Target frameworks | net6.0-maccatalyst |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.0-preview.155 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | This issue was filed against a preview build from December 2021. The current SkiaSharp.NativeAssets.MacCatalyst targets file no longer contains the File.GetAttributes check; packaging was reworked to use .framework.zip with CompressedAppleFramework instead. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.85 (85%) |
| Reason | The current binding/SkiaSharp.NativeAssets.MacCatalyst/buildTransitive/SkiaSharp.targets file (HEAD) no longer contains a File.GetAttributes expression or any path-existence check for the framework binary. The packaging approach was changed to ship the framework as a .zip archive (CompressedAppleFramework). This eliminates the code path that produced MSB4184. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | 2.88.0 (stable) or a later preview |

## Analysis

### Technical Summary

The SkiaSharp.NativeAssets.MacCatalyst.targets file in version 2.88.0-preview.155 contained MSBuild inline C# that called File.GetAttributes() to check whether the libSkiaSharp framework binary had been copied into the app bundle. This check failed because the framework was never placed at that path during the Mac Catalyst build, completely blocking deployment. The current code no longer contains this check; the packaging was redesigned to distribute the framework as a .zip archive.

### Rationale

This is clearly a build-error bug specific to the NativeAssets packaging targets for Mac Catalyst in the 2.88.0-preview.155 build. The current code has been reworked — the File.GetAttributes check is gone and the framework is now deployed as a .zip. The issue is very likely fixed in subsequent stable/preview releases.

### Key Signals

- "MSB4184: The expression "[System.IO.File]::GetAttributes(bin/Debug/net6.0-maccatalyst/maccatalyst-x64/MauiApp1.app/Contents/Frameworks/libSkiaSharp.framework/libSkiaSharp)" cannot be evaluated. Could not find a part of the path." — **issue body** (The .targets file used inline C# to verify the framework binary existed in the output bundle. The binary was never copied there, so the check always failed.)
- "Version with issue: 2.88.0-preview.155 / Last known good version: None" — **issue body** (First attempt with SkiaSharp on Mac Catalyst; this was an early MAUI/SkiaSharp preview with known packaging gaps.)
- "This sample is working properly in all other MAUI environments such as Windows, Android, iOS." — **issue body** (Mac Catalyst-specific packaging issue, not a general SkiaSharp problem. Other platforms don't use the same .targets logic.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.MacCatalyst/buildTransitive/SkiaSharp.targets` | — | direct | Current file contains only a Hot Restart NativeReference ItemGroup for iossimulator/ios frameworks. There is no File.GetAttributes expression, no path-existence check, and no reference to maccatalyst-x64 output paths. The logic that produced MSB4184 has been removed. |
| `binding/SkiaSharp.NativeAssets.MacCatalyst/SkiaSharp.NativeAssets.MacCatalyst.csproj` | — | direct | Package now includes the Mac Catalyst framework as 'output/native/maccatalyst/libSkiaSharp*.framework.zip' with PackagePath 'runtimes/maccatalyst/native'. This .zip approach (CompressedAppleFramework) replaced the old copy-and-check mechanism that caused MSB4184. |
| `binding/IncludeNativeAssets.SkiaSharp.targets` | 1-50 | related | Local development targets reference the framework as a .zip with PublishFolderType='CompressedAppleFramework' for maccatalyst. This confirms the newer packaging mechanism does not rely on File.GetAttributes checks. |

### Resolution Proposals

**Hypothesis:** The .targets file in 2.88.0-preview.155 assumed the libSkiaSharp.framework binary would be present at a specific output path after a build step, but that step was either missing or ran in a different order on Mac Catalyst. Subsequent versions redesigned packaging to use .framework.zip (CompressedAppleFramework), removing the dependency on that file path check.

1. **Upgrade to current stable SkiaSharp** — workaround, confidence 0.85 (85%), cost/xs, validated=untested
   - Update from 2.88.0-preview.155 to the current stable SkiaSharp (2.88.x stable or 3.x). The targets file was reworked and the MSB4184 check was removed.

**Recommended proposal:** Upgrade to current stable SkiaSharp

**Why:** The root cause (the File.GetAttributes check in the targets file) no longer exists in current code. Upgrading is the simplest resolution.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.82 (82%) |
| Reason | The targets file that produced the MSB4184 build error in 2.88.0-preview.155 has been completely reworked. The current SkiaSharp.NativeAssets.MacCatalyst targets file no longer contains the File.GetAttributes check. The issue is against a 4+-year-old preview build and is almost certainly resolved in current releases. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply correct type, area, platform, tenet and partner labels | labels=type/bug, area/Build, os/macOS, tenet/reliability, partner/maui |
| add-comment | high | 0.82 (82%) | Inform reporter this was a known preview packaging issue and suggest upgrading | — |
| close-issue | medium | 0.82 (82%) | Close as fixed — build error no longer present in current releases | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report. This error originated in the `SkiaSharp.NativeAssets.MacCatalyst.targets` file included in **2.88.0-preview.155**, which contained an MSBuild inline C# expression (`File.GetAttributes(...)`) that checked for the presence of the `libSkiaSharp.framework` binary in the app bundle output path. That path was never populated during the Mac Catalyst build, causing MSB4184.

The native assets packaging for Mac Catalyst was subsequently reworked — the framework is now distributed as a compressed `.framework.zip` archive and no longer uses a path-existence check. This code path was removed, fixing the issue.

**Recommended action:** Please upgrade to the current stable version of SkiaSharp (2.88.x or 3.x). If you still experience build issues on Mac Catalyst after upgrading, please open a new issue with the updated version information.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1885,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T07:20:00Z",
    "currentLabels": [
      "area/SkiaSharp.Views.Maui"
    ]
  },
  "summary": "Build error MSB4184 in SkiaSharp.NativeAssets.MacCatalyst.targets when deploying a MAUI application to Mac Catalyst using SkiaSharp 2.88.0-preview.155: the targets file's File.GetAttributes check fails because the libSkiaSharp.framework binary is not copied to the output path.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.88
    },
    "platforms": [
      "os/macOS"
    ],
    "tenets": [
      "tenet/reliability"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "build-error",
      "errorMessage": "MSB4184: The expression \"[System.IO.File]::GetAttributes(bin/Debug/net6.0-maccatalyst/maccatalyst-x64/MauiApp1.app/Contents/Frameworks/libSkiaSharp.framework/libSkiaSharp)\" cannot be evaluated. Could not find a part of the path.",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net6.0-maccatalyst"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a simple MAUI application",
        "Set net6.0-maccatalyst (My Mac) as the target platform",
        "Add SkiaSharp 2.88.0-preview.155 NuGet package",
        "Build the application — MSB4184 error is produced"
      ],
      "environmentDetails": "Visual Studio 2022 for Mac (Preview), macOS 12.0.1, MacBook Air M1, net6.0-maccatalyst",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/7674389/MauiAppSkia.zip",
          "description": "Attached minimal MAUI repro project from reporter"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.0-preview.155"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "This issue was filed against a preview build from December 2021. The current SkiaSharp.NativeAssets.MacCatalyst targets file no longer contains the File.GetAttributes check; packaging was reworked to use .framework.zip with CompressedAppleFramework instead."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.85,
      "reason": "The current binding/SkiaSharp.NativeAssets.MacCatalyst/buildTransitive/SkiaSharp.targets file (HEAD) no longer contains a File.GetAttributes expression or any path-existence check for the framework binary. The packaging approach was changed to ship the framework as a .zip archive (CompressedAppleFramework). This eliminates the code path that produced MSB4184.",
      "fixedInVersion": "2.88.0 (stable) or a later preview"
    }
  },
  "analysis": {
    "summary": "The SkiaSharp.NativeAssets.MacCatalyst.targets file in version 2.88.0-preview.155 contained MSBuild inline C# that called File.GetAttributes() to check whether the libSkiaSharp framework binary had been copied into the app bundle. This check failed because the framework was never placed at that path during the Mac Catalyst build, completely blocking deployment. The current code no longer contains this check; the packaging was redesigned to distribute the framework as a .zip archive.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.MacCatalyst/buildTransitive/SkiaSharp.targets",
        "finding": "Current file contains only a Hot Restart NativeReference ItemGroup for iossimulator/ios frameworks. There is no File.GetAttributes expression, no path-existence check, and no reference to maccatalyst-x64 output paths. The logic that produced MSB4184 has been removed.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.NativeAssets.MacCatalyst/SkiaSharp.NativeAssets.MacCatalyst.csproj",
        "finding": "Package now includes the Mac Catalyst framework as 'output/native/maccatalyst/libSkiaSharp*.framework.zip' with PackagePath 'runtimes/maccatalyst/native'. This .zip approach (CompressedAppleFramework) replaced the old copy-and-check mechanism that caused MSB4184.",
        "relevance": "direct"
      },
      {
        "file": "binding/IncludeNativeAssets.SkiaSharp.targets",
        "lines": "1-50",
        "finding": "Local development targets reference the framework as a .zip with PublishFolderType='CompressedAppleFramework' for maccatalyst. This confirms the newer packaging mechanism does not rely on File.GetAttributes checks.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "MSB4184: The expression \"[System.IO.File]::GetAttributes(bin/Debug/net6.0-maccatalyst/maccatalyst-x64/MauiApp1.app/Contents/Frameworks/libSkiaSharp.framework/libSkiaSharp)\" cannot be evaluated. Could not find a part of the path.",
        "source": "issue body",
        "interpretation": "The .targets file used inline C# to verify the framework binary existed in the output bundle. The binary was never copied there, so the check always failed."
      },
      {
        "text": "Version with issue: 2.88.0-preview.155 / Last known good version: None",
        "source": "issue body",
        "interpretation": "First attempt with SkiaSharp on Mac Catalyst; this was an early MAUI/SkiaSharp preview with known packaging gaps."
      },
      {
        "text": "This sample is working properly in all other MAUI environments such as Windows, Android, iOS.",
        "source": "issue body",
        "interpretation": "Mac Catalyst-specific packaging issue, not a general SkiaSharp problem. Other platforms don't use the same .targets logic."
      }
    ],
    "rationale": "This is clearly a build-error bug specific to the NativeAssets packaging targets for Mac Catalyst in the 2.88.0-preview.155 build. The current code has been reworked — the File.GetAttributes check is gone and the framework is now deployed as a .zip. The issue is very likely fixed in subsequent stable/preview releases.",
    "resolution": {
      "hypothesis": "The .targets file in 2.88.0-preview.155 assumed the libSkiaSharp.framework binary would be present at a specific output path after a build step, but that step was either missing or ran in a different order on Mac Catalyst. Subsequent versions redesigned packaging to use .framework.zip (CompressedAppleFramework), removing the dependency on that file path check.",
      "proposals": [
        {
          "title": "Upgrade to current stable SkiaSharp",
          "description": "Update from 2.88.0-preview.155 to the current stable SkiaSharp (2.88.x stable or 3.x). The targets file was reworked and the MSB4184 check was removed.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Upgrade to current stable SkiaSharp",
      "recommendedReason": "The root cause (the File.GetAttributes check in the targets file) no longer exists in current code. Upgrading is the simplest resolution."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.82,
      "reason": "The targets file that produced the MSB4184 build error in 2.88.0-preview.155 has been completely reworked. The current SkiaSharp.NativeAssets.MacCatalyst targets file no longer contains the File.GetAttributes check. The issue is against a 4+-year-old preview build and is almost certainly resolved in current releases.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply correct type, area, platform, tenet and partner labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/Build",
          "os/macOS",
          "tenet/reliability",
          "partner/maui"
        ]
      },
      {
        "type": "add-comment",
        "description": "Inform reporter this was a known preview packaging issue and suggest upgrading",
        "risk": "high",
        "confidence": 0.82,
        "comment": "Thank you for the detailed report. This error originated in the `SkiaSharp.NativeAssets.MacCatalyst.targets` file included in **2.88.0-preview.155**, which contained an MSBuild inline C# expression (`File.GetAttributes(...)`) that checked for the presence of the `libSkiaSharp.framework` binary in the app bundle output path. That path was never populated during the Mac Catalyst build, causing MSB4184.\n\nThe native assets packaging for Mac Catalyst was subsequently reworked — the framework is now distributed as a compressed `.framework.zip` archive and no longer uses a path-existence check. This code path was removed, fixing the issue.\n\n**Recommended action:** Please upgrade to the current stable version of SkiaSharp (2.88.x or 3.x). If you still experience build issues on Mac Catalyst after upgrading, please open a new issue with the updated version information."
      },
      {
        "type": "close-issue",
        "description": "Close as fixed — build error no longer present in current releases",
        "risk": "medium",
        "confidence": 0.82,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
