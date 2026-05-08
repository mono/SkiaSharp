# Issue Triage Report — #2947

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T20:00:00Z |
| Type | type/bug (0.98 (98%)) |
| Area | area/libSkiaSharp.native (0.95 (95%)) |
| Suggested action | close-as-fixed (0.88 (88%)) |

**Issue Summary:** Building a .NET 8 tvOS project with SkiaSharp 2.88.x fails with a linker error because the NativeAssets package places the device arm64 dylib at the tvos runtime path, which is then incorrectly linked when building for the tvOS Simulator.

**Analysis:** The tvOS NativeAssets NuGet package lacked a separate simulator binary. The build.cake script combined device and simulator slices into one fat framework tagged for tvOS-device, causing modern linkers (Xcode 16+) to reject the dylib when building for the simulator. The fix — splitting the build into separate tvos/ and tvossimulator/ output directories and updating the MSBuild targets to select based on RuntimeIdentifier — has been implemented and merged (see issue #3555, closed 2026-04-02).

**Recommendations:** **close-as-fixed** — The root cause (mixed device/simulator fat binary in tvOS NativeAssets package) has been fixed in the main branch as confirmed by issue #3555 (closed 2026-04-02 as completed) and source code inspection of native/tvos/build.cake and IncludeNativeAssets.SkiaSharp.targets.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/tvOS |
| Backends | — |
| Tenets | tenet/compatibility, tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a .NET 8 tvOS application
2. Add SkiaSharp NuGet reference (2.88.3 or 2.88.8)
3. Run: dotnet build -t:Run -f net8.0-tvos
4. Observe linker error: building for tvOS Simulator but linking device dylib

**Environment:** macOS (Apple Silicon M3 Max), .NET 8, Microsoft.tvOS.Sdk 17.2.8078, SkiaSharp 2.88.3/2.88.8

**Repository links:**
- https://github.com/munit79/tvosfail — Reporter's minimal repro project
- https://github.com/mono/SkiaSharp/issues/3555 — Related issue documenting the root cause and fix — tvOS native build missing arm64 simulator slice and device/simulator split (closed as completed 2026-04-02)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | build-error |
| Error message | ld: building for tvOS Simulator, but linking in dylib built for tvOS, file '.../skiasharp.nativeassets.tvos/2.88.8/runtimes/tvos/native/libSkiaSharp.framework/libSkiaSharp' for architecture arm64 |
| Repro quality | complete |
| Target frameworks | net8.0-tvos |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3, 2.88.8 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | Issue #3555 (closed 2026-04-02 as completed) confirms the fix has been merged: native/tvos/build.cake now builds separate simulator and device outputs, and IncludeNativeAssets.SkiaSharp.targets now correctly selects tvossimulator vs tvos based on RuntimeIdentifier. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.92 (92%) |
| Reason | Related issue #3555 was closed as 'completed' on 2026-04-02 and describes the exact same root cause. Current source code in binding/IncludeNativeAssets.SkiaSharp.targets correctly separates tvossimulator and tvos native references, and native/tvos/build.cake now builds three slices (appletvsimulator x86_64, appletvsimulator arm64, appletvos arm64) into separate output directories. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

The tvOS NativeAssets NuGet package lacked a separate simulator binary. The build.cake script combined device and simulator slices into one fat framework tagged for tvOS-device, causing modern linkers (Xcode 16+) to reject the dylib when building for the simulator. The fix — splitting the build into separate tvos/ and tvossimulator/ output directories and updating the MSBuild targets to select based on RuntimeIdentifier — has been implemented and merged (see issue #3555, closed 2026-04-02).

### Rationale

This is clearly a build error (linker rejects mismatched platform dylib) affecting all tvOS simulator and App Store builds with SkiaSharp 2.88.x on .NET 8. Area is libSkiaSharp.native because the root cause is native library packaging. The issue also occurs when submitting to the App Store (device build gets the simulator-tagged library). Code investigation confirms the fix is present in the current codebase. Issue #3555 tracks the canonical root-cause analysis and was closed as completed, indicating this bug is fixed in a future release.

### Key Signals

- "ld: building for tvOS Simulator, but linking in dylib built for tvOS" — **issue body — build log** (Linker detects platform tag mismatch. The dylib in runtimes/tvos/native/ has LC_VERSION_MIN_TVOS (device), not TVOSSIMULATOR. Modern Xcode linkers enforce this strictly.)
- "I get a similar problem when compiling for the App Store, it builds OK, but Apple rejects the build because the skiasharp library is built for the simulator, not the device." — **issue body** (Bidirectional failure: simulator build gets device dylib, device/App Store build gets simulator-tagged dylib. Classic symptom of a single mixed fat binary.)
- "I suspect it will need something like this: https://github.com/mono/SkiaSharp/pull/2468" — **maintainer comment by mattleibow** (Maintainer identified the fix pattern early on — PR #2468 was the analogous fix for another Apple platform.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `native/tvos/build.cake` | — | direct | Current code builds three slices: appletvsimulator/x86_64, appletvsimulator/arm64, and appletvos/arm64. Outputs are placed into separate tvossimulator/ and tvos/ directories under output/native/. This is the corrected pattern matching iOS. |
| `binding/IncludeNativeAssets.SkiaSharp.targets` | 71-74 | direct | ItemGroup for '-tvos' TFM now uses RuntimeIdentifier.StartsWith('tvossimulator') condition to select the correct native framework — tvossimulator/ for simulator builds, tvos/ for device builds. This directly resolves the linker error. |
| `binding/IncludeNativeAssets.HarfBuzzSharp.targets` | 71-74 | related | Same tvossimulator/tvos RuntimeIdentifier split is present for HarfBuzzSharp, confirming the fix is complete for both native libraries. |

### Next Questions

- Has the fix from #3555 been released in a NuGet package version yet?
- Should this issue be closed as fixed pointing to the next release?

### Resolution Proposals

**Hypothesis:** The tvOS NativeAssets package incorrectly combined device and simulator slices into a single fat binary under one runtime path, causing the linker to reject it for whichever target it wasn't tagged for. The fix is to build and package them separately.

1. **Close as fixed — point to next release** — fix, confidence 0.90 (90%), cost/xs, validated=untested
   - The root-cause fix is confirmed merged (issue #3555 closed 2026-04-02). Inform reporter that the fix will be available in the next SkiaSharp release and close this issue.

**Recommended proposal:** Close as fixed — point to next release

**Why:** Source investigation confirms the fix is in the codebase. Closing with a note to upgrade once the fixed version is published is the right action.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.88 (88%) |
| Reason | The root cause (mixed device/simulator fat binary in tvOS NativeAssets package) has been fixed in the main branch as confirmed by issue #3555 (closed 2026-04-02 as completed) and source code inspection of native/tvos/build.cake and IncludeNativeAssets.SkiaSharp.targets. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply native, tvOS, and build-error labels | labels=type/bug, area/libSkiaSharp.native, os/tvOS, tenet/compatibility, tenet/reliability |
| link-related | low | 0.95 (95%) | Cross-reference issue #3555 which tracks the root cause and fix | linkedIssue=#3555 |
| add-comment | medium | 0.88 (88%) | Notify reporter that the fix has been merged and will be available in the next release | — |
| close-issue | medium | 0.85 (85%) | Close as fixed once the release ships, or now with a note | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report! This is caused by the tvOS NativeAssets NuGet package combining device and simulator native libraries into a single fat binary under one runtime path — the modern Apple linker (Xcode 16+) rejects the dylib when it detects a platform tag mismatch.

The fix has been merged (tracked in #3555): the build now produces separate `tvossimulator/` and `tvos/` native frameworks, and the MSBuild targets select the correct one based on `RuntimeIdentifier`. This fix will ship in the next SkiaSharp release.

In the meantime, a workaround using `vtool` to re-tag the binary is described in #3555.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2947,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T20:00:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Building a .NET 8 tvOS project with SkiaSharp 2.88.x fails with a linker error because the NativeAssets package places the device arm64 dylib at the tvos runtime path, which is then incorrectly linked when building for the tvOS Simulator.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.98
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.95
    },
    "platforms": [
      "os/tvOS"
    ],
    "tenets": [
      "tenet/compatibility",
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "build-error",
      "errorMessage": "ld: building for tvOS Simulator, but linking in dylib built for tvOS, file '.../skiasharp.nativeassets.tvos/2.88.8/runtimes/tvos/native/libSkiaSharp.framework/libSkiaSharp' for architecture arm64",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net8.0-tvos"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET 8 tvOS application",
        "Add SkiaSharp NuGet reference (2.88.3 or 2.88.8)",
        "Run: dotnet build -t:Run -f net8.0-tvos",
        "Observe linker error: building for tvOS Simulator but linking device dylib"
      ],
      "environmentDetails": "macOS (Apple Silicon M3 Max), .NET 8, Microsoft.tvOS.Sdk 17.2.8078, SkiaSharp 2.88.3/2.88.8",
      "repoLinks": [
        {
          "url": "https://github.com/munit79/tvosfail",
          "description": "Reporter's minimal repro project"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3555",
          "description": "Related issue documenting the root cause and fix — tvOS native build missing arm64 simulator slice and device/simulator split (closed as completed 2026-04-02)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3",
        "2.88.8"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "Issue #3555 (closed 2026-04-02 as completed) confirms the fix has been merged: native/tvos/build.cake now builds separate simulator and device outputs, and IncludeNativeAssets.SkiaSharp.targets now correctly selects tvossimulator vs tvos based on RuntimeIdentifier."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.92,
      "reason": "Related issue #3555 was closed as 'completed' on 2026-04-02 and describes the exact same root cause. Current source code in binding/IncludeNativeAssets.SkiaSharp.targets correctly separates tvossimulator and tvos native references, and native/tvos/build.cake now builds three slices (appletvsimulator x86_64, appletvsimulator arm64, appletvos arm64) into separate output directories.",
      "relatedPRs": []
    }
  },
  "analysis": {
    "summary": "The tvOS NativeAssets NuGet package lacked a separate simulator binary. The build.cake script combined device and simulator slices into one fat framework tagged for tvOS-device, causing modern linkers (Xcode 16+) to reject the dylib when building for the simulator. The fix — splitting the build into separate tvos/ and tvossimulator/ output directories and updating the MSBuild targets to select based on RuntimeIdentifier — has been implemented and merged (see issue #3555, closed 2026-04-02).",
    "rationale": "This is clearly a build error (linker rejects mismatched platform dylib) affecting all tvOS simulator and App Store builds with SkiaSharp 2.88.x on .NET 8. Area is libSkiaSharp.native because the root cause is native library packaging. The issue also occurs when submitting to the App Store (device build gets the simulator-tagged library). Code investigation confirms the fix is present in the current codebase. Issue #3555 tracks the canonical root-cause analysis and was closed as completed, indicating this bug is fixed in a future release.",
    "codeInvestigation": [
      {
        "file": "native/tvos/build.cake",
        "finding": "Current code builds three slices: appletvsimulator/x86_64, appletvsimulator/arm64, and appletvos/arm64. Outputs are placed into separate tvossimulator/ and tvos/ directories under output/native/. This is the corrected pattern matching iOS.",
        "relevance": "direct"
      },
      {
        "file": "binding/IncludeNativeAssets.SkiaSharp.targets",
        "lines": "71-74",
        "finding": "ItemGroup for '-tvos' TFM now uses RuntimeIdentifier.StartsWith('tvossimulator') condition to select the correct native framework — tvossimulator/ for simulator builds, tvos/ for device builds. This directly resolves the linker error.",
        "relevance": "direct"
      },
      {
        "file": "binding/IncludeNativeAssets.HarfBuzzSharp.targets",
        "lines": "71-74",
        "finding": "Same tvossimulator/tvos RuntimeIdentifier split is present for HarfBuzzSharp, confirming the fix is complete for both native libraries.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "ld: building for tvOS Simulator, but linking in dylib built for tvOS",
        "source": "issue body — build log",
        "interpretation": "Linker detects platform tag mismatch. The dylib in runtimes/tvos/native/ has LC_VERSION_MIN_TVOS (device), not TVOSSIMULATOR. Modern Xcode linkers enforce this strictly."
      },
      {
        "text": "I get a similar problem when compiling for the App Store, it builds OK, but Apple rejects the build because the skiasharp library is built for the simulator, not the device.",
        "source": "issue body",
        "interpretation": "Bidirectional failure: simulator build gets device dylib, device/App Store build gets simulator-tagged dylib. Classic symptom of a single mixed fat binary."
      },
      {
        "text": "I suspect it will need something like this: https://github.com/mono/SkiaSharp/pull/2468",
        "source": "maintainer comment by mattleibow",
        "interpretation": "Maintainer identified the fix pattern early on — PR #2468 was the analogous fix for another Apple platform."
      }
    ],
    "nextQuestions": [
      "Has the fix from #3555 been released in a NuGet package version yet?",
      "Should this issue be closed as fixed pointing to the next release?"
    ],
    "resolution": {
      "hypothesis": "The tvOS NativeAssets package incorrectly combined device and simulator slices into a single fat binary under one runtime path, causing the linker to reject it for whichever target it wasn't tagged for. The fix is to build and package them separately.",
      "proposals": [
        {
          "title": "Close as fixed — point to next release",
          "description": "The root-cause fix is confirmed merged (issue #3555 closed 2026-04-02). Inform reporter that the fix will be available in the next SkiaSharp release and close this issue.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Close as fixed — point to next release",
      "recommendedReason": "Source investigation confirms the fix is in the codebase. Closing with a note to upgrade once the fixed version is published is the right action."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.88,
      "reason": "The root cause (mixed device/simulator fat binary in tvOS NativeAssets package) has been fixed in the main branch as confirmed by issue #3555 (closed 2026-04-02 as completed) and source code inspection of native/tvos/build.cake and IncludeNativeAssets.SkiaSharp.targets.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply native, tvOS, and build-error labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/tvOS",
          "tenet/compatibility",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference issue #3555 which tracks the root cause and fix",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 3555
      },
      {
        "type": "add-comment",
        "description": "Notify reporter that the fix has been merged and will be available in the next release",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report! This is caused by the tvOS NativeAssets NuGet package combining device and simulator native libraries into a single fat binary under one runtime path — the modern Apple linker (Xcode 16+) rejects the dylib when it detects a platform tag mismatch.\n\nThe fix has been merged (tracked in #3555): the build now produces separate `tvossimulator/` and `tvos/` native frameworks, and the MSBuild targets select the correct one based on `RuntimeIdentifier`. This fix will ship in the next SkiaSharp release.\n\nIn the meantime, a workaround using `vtool` to re-tag the binary is described in #3555."
      },
      {
        "type": "close-issue",
        "description": "Close as fixed once the release ships, or now with a note",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
