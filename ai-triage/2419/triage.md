# Issue Triage Report — #2419

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T00:01:59Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp.Views.Maui (0.82 (82%)) |
| Suggested action | needs-info (0.78 (78%)) |

**Issue Summary:** Building a .NET MAUI app targeting net7.0-ios with SkiaSharp 2.0.0-preview.61 fails at the iOS linker stage with 'ld: framework not found System', blocking deployment entirely.

**Analysis:** The iOS clang linker fails with 'framework not found System' when building a MAUI app that depends on SkiaSharp 2.0.0-preview.61 (and SkiaSharp.Extended.UI.Maui). This error typically means a NativeReference item in the NuGet package metadata declares 'System' as a required Apple framework, but iOS SDK does not ship a System.framework — the reporter may be hitting a known incompatibility between the preview NuGet packaging and Xcode 14.3 / Microsoft.iOS.Sdk 16.2.1024. A second user confirmed the same environment and symptoms, referencing the xamarin-macios XCode 14.3 support issue.

**Recommendations:** **needs-info** — The issue was filed against a two-year-old preview (2.0.0-preview.61); no full build log, no minimal repro project, and no confirmation that the problem persists on the current stable release. Current relevance is unknown.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/iOS |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | partner/maui |

## Evidence

### Reproduction

1. Create a .NET MAUI project targeting net7.0-ios
2. Add SkiaSharp 2.0.0-preview.61 and SkiaSharp.Extended.UI.Maui
3. Build for iOS (device or simulator, iOS 16.2)

**Environment:** SkiaSharp 2.0.0-preview.61, Microsoft.iOS.Sdk 16.2.1024, Xcode 14.3, Visual Studio / Visual Studio for Mac, iPhone SE/X and Simulator

**Repository links:**
- https://github.com/xamarin/xamarin-macios/issues/17561 — xamarin-macios#17561 — Xcode 14.3 support, referenced by second commenter as likely root-cause context

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | build-error |
| Error message | ld: framework not found System
clang: error: linker command failed with exit code 1 |
| Repro quality | partial |
| Target frameworks | net7.0-ios |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.0.0-preview.61 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | Issue filed against a preview version; no known-good version given; current release is 3.x so the issue may be fixed upstream. |

## Analysis

### Technical Summary

The iOS clang linker fails with 'framework not found System' when building a MAUI app that depends on SkiaSharp 2.0.0-preview.61 (and SkiaSharp.Extended.UI.Maui). This error typically means a NativeReference item in the NuGet package metadata declares 'System' as a required Apple framework, but iOS SDK does not ship a System.framework — the reporter may be hitting a known incompatibility between the preview NuGet packaging and Xcode 14.3 / Microsoft.iOS.Sdk 16.2.1024. A second user confirmed the same environment and symptoms, referencing the xamarin-macios XCode 14.3 support issue.

### Rationale

Classified as type/bug because the build is completely blocked by a linker error that the reporter cannot work around. Classified as area/SkiaSharp.Views.Maui because the error manifests only when SkiaSharp (and SkiaSharp.Extended.UI.Maui) is present on net7.0-ios and no other MAUI-specific iOS linker concerns were found in the codebase. Severity is high because it prevents any iOS deployment. suggestedAction is needs-info because: no full build log is provided, no minimal reproducible project is linked, the issue was filed against a preview version, a second commenter suggests an upstream toolchain fix already exists, and current relevance against the released 3.x line is unknown.

### Key Signals

- "ld: framework not found System
clang: error: linker command failed with exit code 1" — **issue body — error output** (iOS linker cannot find a framework named 'System'; this is not a standard Apple framework name and suggests it is being injected incorrectly by a NativeReference or linker flag.)
- "It seems this should be working since 14.3 is now fully supported as per xamarin/xamarin-macios#17561" — **comment by jordanwilcox** (A second affected user links the issue to an upstream toolchain fix for Xcode 14.3 support; the bug may have been resolved in xamarin-macios or a later SkiaSharp release.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SkiaSharp.Views.Maui.Core.csproj` | — | related | Project targets MauiTargetFrameworks (including net7.0-ios) with ProjectReference to binding/SkiaSharp and SkiaSharp.Views; no explicit NativeReference or framework link items found at the csproj level. |
| `source/SkiaSharp.Views.Maui/Directory.Build.targets` | — | context | Imports SkiaSharp.Build.targets; no iOS-specific linker flag overrides present. |

### Next Questions

- Does the issue reproduce with SkiaSharp 2.88.x stable or the current 3.x release?
- Is a full verbose build log (msbuild /v:diag) available showing which NativeReference or framework item introduces 'System'?
- Does removing SkiaSharp.Extended.UI.Maui but keeping SkiaSharp.Views.Maui still reproduce the error?
- Which Xcode and dotnet workload versions are installed?

### Resolution Proposals

1. **Confirm current-version reproduction** — investigation, cost/xs, validated=untested
   - Ask the reporter to reproduce with the latest stable SkiaSharp (3.x) and provide a full diagnostic build log. The preview version is years old and the issue may already be fixed.
2. **Update to a stable SkiaSharp release** — workaround, cost/xs, validated=untested
   - Upgrade from 2.0.0-preview.61 to the latest stable SkiaSharp NuGet packages (2.88.x or 3.x). Preview builds are not supported for production use and often contain packaging defects that are fixed in stable releases.

**Recommended proposal:** p1

**Why:** Before any fix can be scoped, it must be confirmed that the issue still exists against a supported version; upgrading to stable is the likely resolution for a preview-only report.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.78 (78%) |
| Reason | The issue was filed against a two-year-old preview (2.0.0-preview.61); no full build log, no minimal repro project, and no confirmation that the problem persists on the current stable release. Current relevance is unknown. |
| Suggested repro platform | macos |

### Missing Info

- Full MSBuild diagnostic log (/v:diag) showing the linker invocation and which item introduces 'System' as a framework
- Confirmation whether the issue reproduces with the latest stable SkiaSharp (2.88.x or 3.x)
- Minimal reproducible project (ideally publicly hosted)
- Exact Xcode version and dotnet workload versions installed

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Apply type/bug, area/SkiaSharp.Views.Maui, os/iOS, tenet/compatibility, partner/maui | labels=type/bug, area/SkiaSharp.Views.Maui, os/iOS, tenet/compatibility, partner/maui |
| add-comment | medium | 0.78 (78%) | Request updated repro info and confirm whether still reproducible on a stable release | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! This issue was filed against **SkiaSharp 2.0.0-preview.61**, which is a very old preview build. A lot has changed since then, including many iOS/MAUI linker fixes.

Before we investigate further, could you please confirm:

1. **Does this still happen with the latest stable SkiaSharp?** (2.88.x or 3.x — try `dotnet add package SkiaSharp` for the latest version)
2. **If yes, please share a full diagnostic build log** by running: `dotnet build -v diag > build.log` (or the VS equivalent). The log will show which NativeReference or MSBuild item is introducing `System` as a required iOS framework.
3. **A minimal reproducible project** (hosted on GitHub or attached as a zip) would also help narrow this down quickly.

If the issue is resolved by upgrading to a stable release, please close this issue. Thanks!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2419,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T00:01:59Z"
  },
  "summary": "Building a .NET MAUI app targeting net7.0-ios with SkiaSharp 2.0.0-preview.61 fails at the iOS linker stage with 'ld: framework not found System', blocking deployment entirely.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.82
    },
    "platforms": [
      "os/iOS"
    ],
    "tenets": [
      "tenet/compatibility"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "build-error",
      "errorMessage": "ld: framework not found System\nclang: error: linker command failed with exit code 1",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net7.0-ios"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET MAUI project targeting net7.0-ios",
        "Add SkiaSharp 2.0.0-preview.61 and SkiaSharp.Extended.UI.Maui",
        "Build for iOS (device or simulator, iOS 16.2)"
      ],
      "environmentDetails": "SkiaSharp 2.0.0-preview.61, Microsoft.iOS.Sdk 16.2.1024, Xcode 14.3, Visual Studio / Visual Studio for Mac, iPhone SE/X and Simulator",
      "repoLinks": [
        {
          "url": "https://github.com/xamarin/xamarin-macios/issues/17561",
          "description": "xamarin-macios#17561 — Xcode 14.3 support, referenced by second commenter as likely root-cause context"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.0.0-preview.61"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "Issue filed against a preview version; no known-good version given; current release is 3.x so the issue may be fixed upstream."
    }
  },
  "analysis": {
    "summary": "The iOS clang linker fails with 'framework not found System' when building a MAUI app that depends on SkiaSharp 2.0.0-preview.61 (and SkiaSharp.Extended.UI.Maui). This error typically means a NativeReference item in the NuGet package metadata declares 'System' as a required Apple framework, but iOS SDK does not ship a System.framework — the reporter may be hitting a known incompatibility between the preview NuGet packaging and Xcode 14.3 / Microsoft.iOS.Sdk 16.2.1024. A second user confirmed the same environment and symptoms, referencing the xamarin-macios XCode 14.3 support issue.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SkiaSharp.Views.Maui.Core.csproj",
        "finding": "Project targets MauiTargetFrameworks (including net7.0-ios) with ProjectReference to binding/SkiaSharp and SkiaSharp.Views; no explicit NativeReference or framework link items found at the csproj level.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/Directory.Build.targets",
        "finding": "Imports SkiaSharp.Build.targets; no iOS-specific linker flag overrides present.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "ld: framework not found System\nclang: error: linker command failed with exit code 1",
        "source": "issue body — error output",
        "interpretation": "iOS linker cannot find a framework named 'System'; this is not a standard Apple framework name and suggests it is being injected incorrectly by a NativeReference or linker flag."
      },
      {
        "text": "It seems this should be working since 14.3 is now fully supported as per xamarin/xamarin-macios#17561",
        "source": "comment by jordanwilcox",
        "interpretation": "A second affected user links the issue to an upstream toolchain fix for Xcode 14.3 support; the bug may have been resolved in xamarin-macios or a later SkiaSharp release."
      }
    ],
    "rationale": "Classified as type/bug because the build is completely blocked by a linker error that the reporter cannot work around. Classified as area/SkiaSharp.Views.Maui because the error manifests only when SkiaSharp (and SkiaSharp.Extended.UI.Maui) is present on net7.0-ios and no other MAUI-specific iOS linker concerns were found in the codebase. Severity is high because it prevents any iOS deployment. suggestedAction is needs-info because: no full build log is provided, no minimal reproducible project is linked, the issue was filed against a preview version, a second commenter suggests an upstream toolchain fix already exists, and current relevance against the released 3.x line is unknown.",
    "nextQuestions": [
      "Does the issue reproduce with SkiaSharp 2.88.x stable or the current 3.x release?",
      "Is a full verbose build log (msbuild /v:diag) available showing which NativeReference or framework item introduces 'System'?",
      "Does removing SkiaSharp.Extended.UI.Maui but keeping SkiaSharp.Views.Maui still reproduce the error?",
      "Which Xcode and dotnet workload versions are installed?"
    ],
    "resolution": {
      "proposals": [
        {
          "category": "investigation",
          "title": "Confirm current-version reproduction",
          "description": "Ask the reporter to reproduce with the latest stable SkiaSharp (3.x) and provide a full diagnostic build log. The preview version is years old and the issue may already be fixed.",
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "category": "workaround",
          "title": "Update to a stable SkiaSharp release",
          "description": "Upgrade from 2.0.0-preview.61 to the latest stable SkiaSharp NuGet packages (2.88.x or 3.x). Preview builds are not supported for production use and often contain packaging defects that are fixed in stable releases.",
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "p1",
      "recommendedReason": "Before any fix can be scoped, it must be confirmed that the issue still exists against a supported version; upgrading to stable is the likely resolution for a preview-only report."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.78,
      "reason": "The issue was filed against a two-year-old preview (2.0.0-preview.61); no full build log, no minimal repro project, and no confirmation that the problem persists on the current stable release. Current relevance is unknown.",
      "suggestedReproPlatform": "macos"
    },
    "missingInfo": [
      "Full MSBuild diagnostic log (/v:diag) showing the linker invocation and which item introduces 'System' as a framework",
      "Confirmation whether the issue reproduces with the latest stable SkiaSharp (2.88.x or 3.x)",
      "Minimal reproducible project (ideally publicly hosted)",
      "Exact Xcode version and dotnet workload versions installed"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views.Maui, os/iOS, tenet/compatibility, partner/maui",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Maui",
          "os/iOS",
          "tenet/compatibility",
          "partner/maui"
        ]
      },
      {
        "type": "add-comment",
        "description": "Request updated repro info and confirm whether still reproducible on a stable release",
        "risk": "medium",
        "confidence": 0.78,
        "comment": "Thanks for the report! This issue was filed against **SkiaSharp 2.0.0-preview.61**, which is a very old preview build. A lot has changed since then, including many iOS/MAUI linker fixes.\n\nBefore we investigate further, could you please confirm:\n\n1. **Does this still happen with the latest stable SkiaSharp?** (2.88.x or 3.x — try `dotnet add package SkiaSharp` for the latest version)\n2. **If yes, please share a full diagnostic build log** by running: `dotnet build -v diag > build.log` (or the VS equivalent). The log will show which NativeReference or MSBuild item is introducing `System` as a required iOS framework.\n3. **A minimal reproducible project** (hosted on GitHub or attached as a zip) would also help narrow this down quickly.\n\nIf the issue is resolved by upgrading to a stable release, please close this issue. Thanks!"
      }
    ]
  }
}
```

</details>
