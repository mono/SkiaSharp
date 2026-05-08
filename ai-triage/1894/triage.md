# Issue Triage Report — #1894

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T13:00:00Z |
| Type | type/bug (0.93 (93%)) |
| Area | area/libSkiaSharp.native (0.88 (88%)) |
| Suggested action | needs-investigation (0.72 (72%)) |

**Issue Summary:** MAUI iOS app crashes with DllNotFoundException (@rpath/libSkiaSharp.framework/libSkiaSharp) when deployed from Visual Studio for Mac 2022, but works when deployed from Visual Studio for Windows 2022.

**Analysis:** The native framework @rpath/libSkiaSharp.framework is not loaded at runtime on iOS Simulator when the app is built and deployed via VS for Mac. Community investigation points to two root causes: (1) expired Apple intermediate certificates causing codesigning of the native framework to silently fail, meaning the dylib is rejected at load time; (2) stale build artifacts where a full rebuild resolves the issue.

**Recommendations:** **needs-investigation** — The bug has been confirmed by multiple users across two SkiaSharp versions. Root cause (codesigning vs. missing framework embedding vs. RuntimeIdentifier guard) is not fully determined. VS for Mac is now EOL but the issue may still affect current macOS toolchains. Needs investigation to determine current reproducibility and whether a SkiaSharp-side fix is warranted.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/iOS |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | partner/maui |
| Current labels | area/SkiaSharp.Views.Maui |

## Evidence

### Reproduction

1. Create a MAUI app targeting net6.0-ios
2. Add SkiaSharp 2.88.0-preview.155 package
3. Deploy to iOS Simulator from Visual Studio 2022 for Mac
4. App crashes with DllNotFoundException for libSkiaSharp

**Environment:** macOS 12.0.1, VS 2022 for Mac (Preview), iPhone 13 Pro Max iOS 15.0 Simulator, net6.0-ios

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2410 — Similar DllNotFoundException on iOS with MAUI 2.88.3 deployed from Windows — broader related issue
- https://stackoverflow.com/questions/69049965/failed-to-codesign-libskiasharp — Community-reported codesigning root cause: expired Apple intermediate certificates on Mac

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | System.DllNotFoundException: @rpath/libSkiaSharp.framework/libSkiaSharp assembly:<unknown assembly> type:<unknown type> member:(null) |
| Repro quality | partial |
| Target frameworks | net6.0-ios |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.0-preview.155, 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | Issue originally filed with a preview build; confirmed reproduced with 2.88.3 stable. VS for Mac was retired in August 2024, reducing the scope of this specific reproduction path. Whether the same condition arises in current .NET CLI / Rider macOS toolchains is unknown. |

## Analysis

### Technical Summary

The native framework @rpath/libSkiaSharp.framework is not loaded at runtime on iOS Simulator when the app is built and deployed via VS for Mac. Community investigation points to two root causes: (1) expired Apple intermediate certificates causing codesigning of the native framework to silently fail, meaning the dylib is rejected at load time; (2) stale build artifacts where a full rebuild resolves the issue.

### Rationale

The crash is a DllNotFoundException for the P/Invoke constant '@rpath/libSkiaSharp.framework/libSkiaSharp' (SkiaApi.cs:10). The framework is conditionally included as a NativeReference only when RuntimeIdentifier is non-empty (IncludeNativeAssets.SkiaSharp.targets:28-31). The Mac-only reproduction points to a macOS-side toolchain or codesigning issue rather than a SkiaSharp packaging defect per se. Community workarounds (certificate update, rebuild) both confirm environment-side causes. VS for Mac is now EOL (Aug 2024) so this exact repro path is less relevant, but the codesigning/native-framework embedding concern may still affect CLI or Rider workflows on macOS.

### Key Signals

- "System.DllNotFoundException: @rpath/libSkiaSharp.framework/libSkiaSharp" — **issue body** (iOS dyld failed to resolve the @rpath reference — the framework was either not embedded or its code signature was rejected.)
- "Same MAUI.iOS application working properly, if we deployed it from Visual Studio for Windows 2022" — **issue body** (Build-host difference is the key variable; the problem is in Mac-specific toolchain steps (codesigning, framework embedding, or simulator deployment).)
- "It seems like this is not a Skia bug, it is a Apple certificate issue. Updating the certificates on my Mac worked." — **comment by Joelicia** (Expired Apple intermediate CA cert prevents codesign from signing libSkiaSharp.framework; iOS Simulator rejects unsigned frameworks.)
- "I rebuild the whole solution and it work without problem." — **comment by ooikengsiang** (Stale cached build artifacts can cause the framework to be missing or incorrectly signed — a clean rebuild forces re-embedding.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaApi.cs` | 9-10 | direct | P/Invoke DLL path for iOS/tvOS is the literal string '@rpath/libSkiaSharp.framework/libSkiaSharp'. This requires the framework to be correctly embedded and code-signed in the app bundle so dyld can resolve the @rpath. |
| `binding/IncludeNativeAssets.SkiaSharp.targets` | 28-31 | direct | NativeReference for libSkiaSharp.framework is added only when '$(RuntimeIdentifier)' != ''. If the MAUI build for iOS Simulator on Mac does not set RuntimeIdentifier before this targets file is evaluated, the framework reference is skipped and the native library is never embedded. |

### Workarounds

- Update Apple intermediate certificates on the Mac (Keychain Access → Certificate Assistant → update expired certs). Reported by community to resolve codesigning failures.
- Perform a full solution rebuild (Clean → Rebuild) to clear stale build artifacts that may have caused the framework to be incorrectly embedded.

### Next Questions

- Does the same DllNotFoundException occur when building the same app on macOS with dotnet CLI instead of VS for Mac?
- Is the libSkiaSharp.framework present but with an invalid/missing code signature in the failing .app bundle, or is it entirely absent?
- Does the issue reproduce on physical iOS device (not just Simulator) when building from Mac?
- Now that VS for Mac is EOL, is this still reproducible via Rider or dotnet CLI on macOS?

### Resolution Proposals

**Hypothesis:** The crash is caused by one of two Mac-specific conditions: (a) expired Apple intermediate certificates causing codesigning of libSkiaSharp.framework to fail silently, so iOS Simulator rejects it at load time; or (b) stale MSBuild cache causing the framework to be absent from the app bundle. The RuntimeIdentifier guard in targets may also be a contributing factor on some MAUI preview toolchains.

1. **User workaround: Update Apple certificates** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - Refresh expired Apple intermediate certificates via Keychain Access or by re-running the iOS device/simulator setup in Xcode. This resolves codesigning failures on Mac.
2. **User workaround: Full clean rebuild** — workaround, confidence 0.75 (75%), cost/xs, validated=untested
   - Delete bin/obj folders, run Clean, then Rebuild. Removes stale cached build artifacts that may cause the framework embedding step to be skipped.
3. **Investigate RuntimeIdentifier guard in targets** — investigation, confidence 0.65 (65%), cost/s, validated=untested
   - Audit whether the Condition='$(RuntimeIdentifier) != ""' guard in IncludeNativeAssets.SkiaSharp.targets could evaluate to false during MAUI iOS simulator builds on Mac, causing the NativeReference to be silently omitted. Consider whether an additional fallback or diagnostic message is warranted.

**Recommended proposal:** User workaround: Update Apple certificates

**Why:** Community-confirmed fix with immediate effect. The rebuild workaround is a close second for users whose certs are current.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.72 (72%) |
| Reason | The bug has been confirmed by multiple users across two SkiaSharp versions. Root cause (codesigning vs. missing framework embedding vs. RuntimeIdentifier guard) is not fully determined. VS for Mac is now EOL but the issue may still affect current macOS toolchains. Needs investigation to determine current reproducibility and whether a SkiaSharp-side fix is warranted. |
| Suggested repro platform | macos |

### Missing Info

- Does the issue reproduce with current SkiaSharp versions (2.88.7+ or 3.x) on macOS using dotnet CLI or Rider?
- Is libSkiaSharp.framework present in the app bundle when the crash occurs, or entirely missing?
- Can the reporter confirm whether Apple intermediate certificates were up to date when the issue was first observed?

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Replace area/SkiaSharp.Views.Maui with area/libSkiaSharp.native; add type/bug, os/iOS, tenet/reliability, partner/maui | labels=type/bug, area/libSkiaSharp.native, os/iOS, tenet/reliability, partner/maui |
| add-comment | medium | 0.78 (78%) | Post analysis with known workarounds and ask for current reproducibility | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and sample project.

Based on investigation and community comments, this crash has two known workarounds:

1. **Update Apple intermediate certificates** on your Mac (Keychain Access → Certificate Assistant, or re-run Xcode device setup). Expired certificates cause `codesign` to silently fail on `libSkiaSharp.framework`, and iOS Simulator rejects the unsigned framework at load time. ([Reference](https://stackoverflow.com/questions/69049965/failed-to-codesign-libskiasharp))
2. **Perform a full clean rebuild** (delete `bin`/`obj`, Clean, then Rebuild). Stale build artifacts can cause the framework embedding step to be skipped.

Note: Visual Studio for Mac reached end-of-life in August 2024. If you are still experiencing this issue with a current SkiaSharp version using `dotnet` CLI or Rider on macOS, please provide:
- Current SkiaSharp version and .NET SDK version
- Whether `libSkiaSharp.framework` is present (but unsigned) or entirely absent in the failing app bundle
- Output of `codesign -vvv <path-to-app>/Frameworks/libSkiaSharp.framework` if the framework is present
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1894,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T13:00:00Z",
    "currentLabels": [
      "area/SkiaSharp.Views.Maui"
    ]
  },
  "summary": "MAUI iOS app crashes with DllNotFoundException (@rpath/libSkiaSharp.framework/libSkiaSharp) when deployed from Visual Studio for Mac 2022, but works when deployed from Visual Studio for Windows 2022.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.93
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.88
    },
    "platforms": [
      "os/iOS"
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
      "errorType": "crash",
      "errorMessage": "System.DllNotFoundException: @rpath/libSkiaSharp.framework/libSkiaSharp assembly:<unknown assembly> type:<unknown type> member:(null)",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net6.0-ios"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a MAUI app targeting net6.0-ios",
        "Add SkiaSharp 2.88.0-preview.155 package",
        "Deploy to iOS Simulator from Visual Studio 2022 for Mac",
        "App crashes with DllNotFoundException for libSkiaSharp"
      ],
      "environmentDetails": "macOS 12.0.1, VS 2022 for Mac (Preview), iPhone 13 Pro Max iOS 15.0 Simulator, net6.0-ios",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2410",
          "description": "Similar DllNotFoundException on iOS with MAUI 2.88.3 deployed from Windows — broader related issue"
        },
        {
          "url": "https://stackoverflow.com/questions/69049965/failed-to-codesign-libskiasharp",
          "description": "Community-reported codesigning root cause: expired Apple intermediate certificates on Mac"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.0-preview.155",
        "2.88.3"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "Issue originally filed with a preview build; confirmed reproduced with 2.88.3 stable. VS for Mac was retired in August 2024, reducing the scope of this specific reproduction path. Whether the same condition arises in current .NET CLI / Rider macOS toolchains is unknown."
    }
  },
  "analysis": {
    "summary": "The native framework @rpath/libSkiaSharp.framework is not loaded at runtime on iOS Simulator when the app is built and deployed via VS for Mac. Community investigation points to two root causes: (1) expired Apple intermediate certificates causing codesigning of the native framework to silently fail, meaning the dylib is rejected at load time; (2) stale build artifacts where a full rebuild resolves the issue.",
    "rationale": "The crash is a DllNotFoundException for the P/Invoke constant '@rpath/libSkiaSharp.framework/libSkiaSharp' (SkiaApi.cs:10). The framework is conditionally included as a NativeReference only when RuntimeIdentifier is non-empty (IncludeNativeAssets.SkiaSharp.targets:28-31). The Mac-only reproduction points to a macOS-side toolchain or codesigning issue rather than a SkiaSharp packaging defect per se. Community workarounds (certificate update, rebuild) both confirm environment-side causes. VS for Mac is now EOL (Aug 2024) so this exact repro path is less relevant, but the codesigning/native-framework embedding concern may still affect CLI or Rider workflows on macOS.",
    "keySignals": [
      {
        "text": "System.DllNotFoundException: @rpath/libSkiaSharp.framework/libSkiaSharp",
        "source": "issue body",
        "interpretation": "iOS dyld failed to resolve the @rpath reference — the framework was either not embedded or its code signature was rejected."
      },
      {
        "text": "Same MAUI.iOS application working properly, if we deployed it from Visual Studio for Windows 2022",
        "source": "issue body",
        "interpretation": "Build-host difference is the key variable; the problem is in Mac-specific toolchain steps (codesigning, framework embedding, or simulator deployment)."
      },
      {
        "text": "It seems like this is not a Skia bug, it is a Apple certificate issue. Updating the certificates on my Mac worked.",
        "source": "comment by Joelicia",
        "interpretation": "Expired Apple intermediate CA cert prevents codesign from signing libSkiaSharp.framework; iOS Simulator rejects unsigned frameworks."
      },
      {
        "text": "I rebuild the whole solution and it work without problem.",
        "source": "comment by ooikengsiang",
        "interpretation": "Stale cached build artifacts can cause the framework to be missing or incorrectly signed — a clean rebuild forces re-embedding."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaApi.cs",
        "lines": "9-10",
        "finding": "P/Invoke DLL path for iOS/tvOS is the literal string '@rpath/libSkiaSharp.framework/libSkiaSharp'. This requires the framework to be correctly embedded and code-signed in the app bundle so dyld can resolve the @rpath.",
        "relevance": "direct"
      },
      {
        "file": "binding/IncludeNativeAssets.SkiaSharp.targets",
        "lines": "28-31",
        "finding": "NativeReference for libSkiaSharp.framework is added only when '$(RuntimeIdentifier)' != ''. If the MAUI build for iOS Simulator on Mac does not set RuntimeIdentifier before this targets file is evaluated, the framework reference is skipped and the native library is never embedded.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Update Apple intermediate certificates on the Mac (Keychain Access → Certificate Assistant → update expired certs). Reported by community to resolve codesigning failures.",
      "Perform a full solution rebuild (Clean → Rebuild) to clear stale build artifacts that may have caused the framework to be incorrectly embedded."
    ],
    "nextQuestions": [
      "Does the same DllNotFoundException occur when building the same app on macOS with dotnet CLI instead of VS for Mac?",
      "Is the libSkiaSharp.framework present but with an invalid/missing code signature in the failing .app bundle, or is it entirely absent?",
      "Does the issue reproduce on physical iOS device (not just Simulator) when building from Mac?",
      "Now that VS for Mac is EOL, is this still reproducible via Rider or dotnet CLI on macOS?"
    ],
    "resolution": {
      "hypothesis": "The crash is caused by one of two Mac-specific conditions: (a) expired Apple intermediate certificates causing codesigning of libSkiaSharp.framework to fail silently, so iOS Simulator rejects it at load time; or (b) stale MSBuild cache causing the framework to be absent from the app bundle. The RuntimeIdentifier guard in targets may also be a contributing factor on some MAUI preview toolchains.",
      "proposals": [
        {
          "title": "User workaround: Update Apple certificates",
          "description": "Refresh expired Apple intermediate certificates via Keychain Access or by re-running the iOS device/simulator setup in Xcode. This resolves codesigning failures on Mac.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "User workaround: Full clean rebuild",
          "description": "Delete bin/obj folders, run Clean, then Rebuild. Removes stale cached build artifacts that may cause the framework embedding step to be skipped.",
          "category": "workaround",
          "confidence": 0.75,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Investigate RuntimeIdentifier guard in targets",
          "description": "Audit whether the Condition='$(RuntimeIdentifier) != \"\"' guard in IncludeNativeAssets.SkiaSharp.targets could evaluate to false during MAUI iOS simulator builds on Mac, causing the NativeReference to be silently omitted. Consider whether an additional fallback or diagnostic message is warranted.",
          "category": "investigation",
          "confidence": 0.65,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "User workaround: Update Apple certificates",
      "recommendedReason": "Community-confirmed fix with immediate effect. The rebuild workaround is a close second for users whose certs are current."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.72,
      "reason": "The bug has been confirmed by multiple users across two SkiaSharp versions. Root cause (codesigning vs. missing framework embedding vs. RuntimeIdentifier guard) is not fully determined. VS for Mac is now EOL but the issue may still affect current macOS toolchains. Needs investigation to determine current reproducibility and whether a SkiaSharp-side fix is warranted.",
      "suggestedReproPlatform": "macos"
    },
    "missingInfo": [
      "Does the issue reproduce with current SkiaSharp versions (2.88.7+ or 3.x) on macOS using dotnet CLI or Rider?",
      "Is libSkiaSharp.framework present in the app bundle when the crash occurs, or entirely missing?",
      "Can the reporter confirm whether Apple intermediate certificates were up to date when the issue was first observed?"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Replace area/SkiaSharp.Views.Maui with area/libSkiaSharp.native; add type/bug, os/iOS, tenet/reliability, partner/maui",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/iOS",
          "tenet/reliability",
          "partner/maui"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis with known workarounds and ask for current reproducibility",
        "risk": "medium",
        "confidence": 0.78,
        "comment": "Thanks for the detailed report and sample project.\n\nBased on investigation and community comments, this crash has two known workarounds:\n\n1. **Update Apple intermediate certificates** on your Mac (Keychain Access → Certificate Assistant, or re-run Xcode device setup). Expired certificates cause `codesign` to silently fail on `libSkiaSharp.framework`, and iOS Simulator rejects the unsigned framework at load time. ([Reference](https://stackoverflow.com/questions/69049965/failed-to-codesign-libskiasharp))\n2. **Perform a full clean rebuild** (delete `bin`/`obj`, Clean, then Rebuild). Stale build artifacts can cause the framework embedding step to be skipped.\n\nNote: Visual Studio for Mac reached end-of-life in August 2024. If you are still experiencing this issue with a current SkiaSharp version using `dotnet` CLI or Rider on macOS, please provide:\n- Current SkiaSharp version and .NET SDK version\n- Whether `libSkiaSharp.framework` is present (but unsigned) or entirely absent in the failing app bundle\n- Output of `codesign -vvv <path-to-app>/Frameworks/libSkiaSharp.framework` if the framework is present"
      }
    ]
  }
}
```

</details>
