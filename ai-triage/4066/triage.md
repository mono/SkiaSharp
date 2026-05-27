# Issue Triage Report — #4066

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-27T05:42:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/libSkiaSharp.native (0.85 (85%)) |
| Suggested action | needs-info (0.80 (80%)) |

**Issue Summary:** iOS build fails with 'ld: framework libSkiaSharp not found' after upgrading from SkiaSharp 3.119.2 to 3.119.4 in a .NET 10 MAUI project; Android builds succeed.

**Analysis:** iOS linker cannot locate the libSkiaSharp framework after upgrading to 3.119.4. The regression coincides with Metal backend API changes introduced in 3.119.4 and may be caused by framework packaging changes, a TFM mismatch (package targets net10.0-ios26.0 but reporter uses ios26.1 workload), or an incompatible native binary layout change inside the .xcframework.

**Recommendations:** **needs-info** — The regression is clear (3.119.2 → 3.119.4, iOS only) but root cause is ambiguous. A verbose build log (/v:diag) and confirmation of the exact .NET/workload versions are needed to determine whether this is a packaging bug in 3.119.4 or a workload-version incompatibility.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/iOS |
| Backends | backend/Metal |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create or open a .NET 10 MAUI project using SkiaSharp
2. Upgrade SkiaSharp from 3.119.2 to 3.119.4
3. Build for iOS (device or simulator) in Release or Debug configuration
4. Observe linker error: ld: framework 'libSkiaSharp' not found

**Environment:** Visual Studio on Windows, .NET 10, iOS SDK net10.0_26.1, MAUI project

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2675 — Related closed issue: Cannot build native iOS libSkiaSharp with cake

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | build-error |
| Error message | ld: framework 'libSkiaSharp' not found |
| Repro quality | partial |
| Target frameworks | net10.0-ios |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.119.4, 3.119.2 |
| Worked in | 3.119.2 |
| Broke in | 3.119.4 |
| Current relevance | likely |
| Relevance reason | Regression appears tied to 3.119.4 release; commenter confirms Metal backend API changes were included in that release. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.90 (90%) |
| Reason | Reporter explicitly states 3.119.2 worked; 3.119.4 fails. Rolling back the package restores the build. Commenter confirms Metal API changes were in 3.119.4. |
| Worked in version | 3.119.2 |
| Broke in version | 3.119.4 |

## Analysis

### Technical Summary

iOS linker cannot locate the libSkiaSharp framework after upgrading to 3.119.4. The regression coincides with Metal backend API changes introduced in 3.119.4 and may be caused by framework packaging changes, a TFM mismatch (package targets net10.0-ios26.0 but reporter uses ios26.1 workload), or an incompatible native binary layout change inside the .xcframework.

### Rationale

Clear regression (3.119.2 → 3.119.4) on iOS only. The native assets package (SkiaSharp.NativeAssets.iOS) delivers the libSkiaSharp.framework via runtimes/ios/native/ and runtimes/iossimulator/native/ paths. The buildTransitive targets file is intentionally empty for iOS (NuGet runtime asset resolution is expected to wire up the framework). If the framework's internal structure changed in 3.119.4 (e.g., new binaries or renamed slices due to Metal backend refactor), the linker may not find it. The reporter's ios26.1 workload vs. package's ios26.0 TFM is a secondary suspect but TFM fallback should cover that.

### Key Signals

- "ld: framework 'libSkiaSharp' not found" — **issue body** (iOS linker cannot find the framework at link time — framework is either absent from the NuGet package layout, not being referenced correctly, or the .xcframework slice is missing.)
- "I can compile successfully with Android (both: Debug and Release) but I cannot with iOS in any mode. If I rollback the library everything works." — **comment by reporter** (Definitive regression: iOS-specific failure in 3.119.4, Android unaffected, rollback to 3.119.2 resolves it.)
- "There were some API updates required for the Metal backend with 3.119.4" — **comment by jeremy-visionaid** (Metal API changes in 3.119.4 may have altered the iOS native library structure or its packaging, causing the linker to fail.)
- "Microsoft.iOS.Sdk.net10.0_26.1" — **issue body (build error path)** (Reporter is using iOS workload 26.1; SkiaSharp currently targets ios26.0 (TPViOSCurrent=26.0 in SkiaSharp.Build.props). TFM fallback should handle this but is a secondary suspect.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.iOS/SkiaSharp.NativeAssets.iOS.csproj` | 1-15 | direct | iOS native assets package targets net10.0-ios26.0 (via TPViOSCurrent=26.0 in SkiaSharp.Build.props). The framework is packaged under runtimes/ios/native/libSkiaSharp.framework and runtimes/iossimulator/native/libSkiaSharp.framework. buildTransitive targets file is empty — no explicit NativeReference items; relies on NuGet runtime asset resolution. |
| `binding/SkiaSharp.NativeAssets.iOS/buildTransitive/SkiaSharp.targets` | 1-4 | direct | Empty targets file — no NativeReference items injected for iOS builds via the package. This is by design; the .NET SDK is expected to pick up the framework from the NuGet runtimes directory. If this mechanism breaks (e.g., due to iOS SDK version mismatch or framework structure change), the linker will not find the framework. |
| `source/SkiaSharp.Build.props` | 75,83 | related | TPViOSPrevious=18.0, TPViOSCurrent=26.0. Reporter's build path shows ios26.1 workload. Package targets 26.0; user's SDK is 26.1. NuGet TFM fallback should cover minor version differences but this mismatch is a contributing factor worth ruling out. |
| `binding/SkiaSharp.NativeAssets.MacCatalyst/buildTransitive/SkiaSharp.targets` | 3-7 | context | MacCatalyst targets file includes explicit NativeReference items (for Hot Restart builds only). iOS does not have equivalent explicit NativeReference items. The difference in build behavior between Android (uses explicit AndroidNativeLibrary) and iOS (relies on NuGet asset resolution) may explain why iOS is more sensitive to packaging changes. |

### Workarounds

- Roll back to SkiaSharp 3.119.2 until the issue is investigated.
- Try updating the .NET 10 iOS workload to the latest version (dotnet workload update) to see if the issue is workload-specific.

### Next Questions

- Does the issue also affect iOS simulator builds or only device builds?
- Does the issue reproduce with the latest .NET 10 workload (net10.0-ios26.0)?
- Is the libSkiaSharp.framework present in the NuGet package cache under runtimes/ios/native/ for 3.119.4?
- Did the xcframework structure or binary name change between 3.119.2 and 3.119.4 due to Metal API changes?
- Can the reporter share a verbose build log (/v:diag) to see which NativeReference items are resolved?

### Resolution Proposals

**Hypothesis:** The Metal backend changes in 3.119.4 may have altered the iOS framework binary layout or the NuGet package failed to include the correct iOS framework slice. Combined with a potential TFM resolution issue for ios26.1, the linker cannot locate the framework.

1. **Verify iOS framework presence in 3.119.4 NuGet package** — investigation, confidence 0.85 (85%), cost/xs, validated=untested
   - Download the SkiaSharp.NativeAssets.iOS 3.119.4 NuGet package and inspect the runtimes/ios/native/libSkiaSharp.framework directory. Compare with 3.119.2 to identify any structural changes introduced by Metal backend updates.
2. **Add explicit NativeReference in iOS buildTransitive targets** — fix, confidence 0.65 (65%), cost/s, validated=untested
   - Add explicit NativeReference items to the iOS buildTransitive targets file (similar to what MacCatalyst does) to ensure the framework is always linked regardless of NuGet runtime asset resolution behavior.

**Recommended proposal:** Verify iOS framework presence in 3.119.4 NuGet package

**Why:** The investigation must first confirm whether the framework is missing from the package or if the build system isn't wiring it up. This is the cheapest path to root cause.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.80 (80%) |
| Reason | The regression is clear (3.119.2 → 3.119.4, iOS only) but root cause is ambiguous. A verbose build log (/v:diag) and confirmation of the exact .NET/workload versions are needed to determine whether this is a packaging bug in 3.119.4 or a workload-version incompatibility. |
| Suggested repro platform | macos |

### Missing Info

- Verbose MSBuild log (dotnet build -v:diag) showing NativeReference resolution for iOS
- Exact .NET SDK version (dotnet --version) and iOS workload version (dotnet workload list)
- Whether the issue affects Simulator builds as well as device builds
- Whether updating the iOS workload (dotnet workload update) resolves the issue

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, native, iOS, Metal, and compatibility labels | labels=type/bug, area/libSkiaSharp.native, os/iOS, backend/Metal, tenet/compatibility |
| add-comment | medium | 0.82 (82%) | Ask for verbose build log and workload version details to distinguish packaging bug from workload incompatibility | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report. This looks like a real regression — iOS-specific and isolated to the 3.119.4 → upgrade.

To narrow down the root cause, could you share:

1. **Verbose build log**: Run `dotnet build -v:diag` on the iOS target and share the output (or the relevant NativeReference/linker sections). This will show whether the `libSkiaSharp.framework` is being resolved from the NuGet package.
2. **SDK/workload versions**: `dotnet --version` and `dotnet workload list`
3. **Simulator vs device**: Does the failure also occur when targeting the iOS Simulator?
4. **Workload update test**: Does running `dotnet workload update` before building change the outcome?

In the meantime, rolling back to 3.119.2 is the recommended workaround.

> Note: If you are building on an older Xcode/iOS workload version, updating to the latest may also help as the commenter suggested.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4066,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-27T05:42:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "iOS build fails with 'ld: framework libSkiaSharp not found' after upgrading from SkiaSharp 3.119.2 to 3.119.4 in a .NET 10 MAUI project; Android builds succeed.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.85
    },
    "platforms": [
      "os/iOS"
    ],
    "backends": [
      "backend/Metal"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "build-error",
      "errorMessage": "ld: framework 'libSkiaSharp' not found",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net10.0-ios"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create or open a .NET 10 MAUI project using SkiaSharp",
        "Upgrade SkiaSharp from 3.119.2 to 3.119.4",
        "Build for iOS (device or simulator) in Release or Debug configuration",
        "Observe linker error: ld: framework 'libSkiaSharp' not found"
      ],
      "environmentDetails": "Visual Studio on Windows, .NET 10, iOS SDK net10.0_26.1, MAUI project",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2675",
          "description": "Related closed issue: Cannot build native iOS libSkiaSharp with cake"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.119.4",
        "3.119.2"
      ],
      "workedIn": "3.119.2",
      "brokeIn": "3.119.4",
      "currentRelevance": "likely",
      "relevanceReason": "Regression appears tied to 3.119.4 release; commenter confirms Metal backend API changes were included in that release."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.9,
      "reason": "Reporter explicitly states 3.119.2 worked; 3.119.4 fails. Rolling back the package restores the build. Commenter confirms Metal API changes were in 3.119.4.",
      "workedInVersion": "3.119.2",
      "brokeInVersion": "3.119.4"
    }
  },
  "analysis": {
    "summary": "iOS linker cannot locate the libSkiaSharp framework after upgrading to 3.119.4. The regression coincides with Metal backend API changes introduced in 3.119.4 and may be caused by framework packaging changes, a TFM mismatch (package targets net10.0-ios26.0 but reporter uses ios26.1 workload), or an incompatible native binary layout change inside the .xcframework.",
    "rationale": "Clear regression (3.119.2 → 3.119.4) on iOS only. The native assets package (SkiaSharp.NativeAssets.iOS) delivers the libSkiaSharp.framework via runtimes/ios/native/ and runtimes/iossimulator/native/ paths. The buildTransitive targets file is intentionally empty for iOS (NuGet runtime asset resolution is expected to wire up the framework). If the framework's internal structure changed in 3.119.4 (e.g., new binaries or renamed slices due to Metal backend refactor), the linker may not find it. The reporter's ios26.1 workload vs. package's ios26.0 TFM is a secondary suspect but TFM fallback should cover that.",
    "keySignals": [
      {
        "text": "ld: framework 'libSkiaSharp' not found",
        "source": "issue body",
        "interpretation": "iOS linker cannot find the framework at link time — framework is either absent from the NuGet package layout, not being referenced correctly, or the .xcframework slice is missing."
      },
      {
        "text": "I can compile successfully with Android (both: Debug and Release) but I cannot with iOS in any mode. If I rollback the library everything works.",
        "source": "comment by reporter",
        "interpretation": "Definitive regression: iOS-specific failure in 3.119.4, Android unaffected, rollback to 3.119.2 resolves it."
      },
      {
        "text": "There were some API updates required for the Metal backend with 3.119.4",
        "source": "comment by jeremy-visionaid",
        "interpretation": "Metal API changes in 3.119.4 may have altered the iOS native library structure or its packaging, causing the linker to fail."
      },
      {
        "text": "Microsoft.iOS.Sdk.net10.0_26.1",
        "source": "issue body (build error path)",
        "interpretation": "Reporter is using iOS workload 26.1; SkiaSharp currently targets ios26.0 (TPViOSCurrent=26.0 in SkiaSharp.Build.props). TFM fallback should handle this but is a secondary suspect."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.iOS/SkiaSharp.NativeAssets.iOS.csproj",
        "lines": "1-15",
        "finding": "iOS native assets package targets net10.0-ios26.0 (via TPViOSCurrent=26.0 in SkiaSharp.Build.props). The framework is packaged under runtimes/ios/native/libSkiaSharp.framework and runtimes/iossimulator/native/libSkiaSharp.framework. buildTransitive targets file is empty — no explicit NativeReference items; relies on NuGet runtime asset resolution.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.NativeAssets.iOS/buildTransitive/SkiaSharp.targets",
        "lines": "1-4",
        "finding": "Empty targets file — no NativeReference items injected for iOS builds via the package. This is by design; the .NET SDK is expected to pick up the framework from the NuGet runtimes directory. If this mechanism breaks (e.g., due to iOS SDK version mismatch or framework structure change), the linker will not find the framework.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Build.props",
        "lines": "75,83",
        "finding": "TPViOSPrevious=18.0, TPViOSCurrent=26.0. Reporter's build path shows ios26.1 workload. Package targets 26.0; user's SDK is 26.1. NuGet TFM fallback should cover minor version differences but this mismatch is a contributing factor worth ruling out.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp.NativeAssets.MacCatalyst/buildTransitive/SkiaSharp.targets",
        "lines": "3-7",
        "finding": "MacCatalyst targets file includes explicit NativeReference items (for Hot Restart builds only). iOS does not have equivalent explicit NativeReference items. The difference in build behavior between Android (uses explicit AndroidNativeLibrary) and iOS (relies on NuGet asset resolution) may explain why iOS is more sensitive to packaging changes.",
        "relevance": "context"
      }
    ],
    "nextQuestions": [
      "Does the issue also affect iOS simulator builds or only device builds?",
      "Does the issue reproduce with the latest .NET 10 workload (net10.0-ios26.0)?",
      "Is the libSkiaSharp.framework present in the NuGet package cache under runtimes/ios/native/ for 3.119.4?",
      "Did the xcframework structure or binary name change between 3.119.2 and 3.119.4 due to Metal API changes?",
      "Can the reporter share a verbose build log (/v:diag) to see which NativeReference items are resolved?"
    ],
    "workarounds": [
      "Roll back to SkiaSharp 3.119.2 until the issue is investigated.",
      "Try updating the .NET 10 iOS workload to the latest version (dotnet workload update) to see if the issue is workload-specific."
    ],
    "resolution": {
      "hypothesis": "The Metal backend changes in 3.119.4 may have altered the iOS framework binary layout or the NuGet package failed to include the correct iOS framework slice. Combined with a potential TFM resolution issue for ios26.1, the linker cannot locate the framework.",
      "proposals": [
        {
          "title": "Verify iOS framework presence in 3.119.4 NuGet package",
          "description": "Download the SkiaSharp.NativeAssets.iOS 3.119.4 NuGet package and inspect the runtimes/ios/native/libSkiaSharp.framework directory. Compare with 3.119.2 to identify any structural changes introduced by Metal backend updates.",
          "category": "investigation",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Add explicit NativeReference in iOS buildTransitive targets",
          "description": "Add explicit NativeReference items to the iOS buildTransitive targets file (similar to what MacCatalyst does) to ensure the framework is always linked regardless of NuGet runtime asset resolution behavior.",
          "category": "fix",
          "confidence": 0.65,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Verify iOS framework presence in 3.119.4 NuGet package",
      "recommendedReason": "The investigation must first confirm whether the framework is missing from the package or if the build system isn't wiring it up. This is the cheapest path to root cause."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.8,
      "reason": "The regression is clear (3.119.2 → 3.119.4, iOS only) but root cause is ambiguous. A verbose build log (/v:diag) and confirmation of the exact .NET/workload versions are needed to determine whether this is a packaging bug in 3.119.4 or a workload-version incompatibility.",
      "suggestedReproPlatform": "macos"
    },
    "missingInfo": [
      "Verbose MSBuild log (dotnet build -v:diag) showing NativeReference resolution for iOS",
      "Exact .NET SDK version (dotnet --version) and iOS workload version (dotnet workload list)",
      "Whether the issue affects Simulator builds as well as device builds",
      "Whether updating the iOS workload (dotnet workload update) resolves the issue"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, native, iOS, Metal, and compatibility labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/iOS",
          "backend/Metal",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask for verbose build log and workload version details to distinguish packaging bug from workload incompatibility",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report. This looks like a real regression — iOS-specific and isolated to the 3.119.4 → upgrade.\n\nTo narrow down the root cause, could you share:\n\n1. **Verbose build log**: Run `dotnet build -v:diag` on the iOS target and share the output (or the relevant NativeReference/linker sections). This will show whether the `libSkiaSharp.framework` is being resolved from the NuGet package.\n2. **SDK/workload versions**: `dotnet --version` and `dotnet workload list`\n3. **Simulator vs device**: Does the failure also occur when targeting the iOS Simulator?\n4. **Workload update test**: Does running `dotnet workload update` before building change the outcome?\n\nIn the meantime, rolling back to 3.119.2 is the recommended workaround.\n\n> Note: If you are building on an older Xcode/iOS workload version, updating to the latest may also help as the commenter suggested."
      }
    ]
  }
}
```

</details>
