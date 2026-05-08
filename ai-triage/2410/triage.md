# Issue Triage Report — #2410

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T11:43:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/libSkiaSharp.native (0.85 (85%)) |
| Suggested action | needs-investigation (0.78 (78%)) |

**Issue Summary:** MAUI app on iOS (iPad) crashes with DllNotFoundException for libSkiaSharp.framework when deployed from Windows using Hot Restart; deploying from Mac works fine.

**Analysis:** The app crashes on physical iOS device because the native libSkiaSharp.framework is not available at runtime when deploying via Visual Studio Hot Restart on Windows. Hot Restart only bundles managed assemblies, not native frameworks. Deploying via a Mac (remote devices) correctly embeds the framework in the app bundle. The SkiaSharp.NativeAssets.iOS package ships the framework bundle correctly (runtimes/ios/native/libSkiaSharp.framework), and the buildTransitive/SkiaSharp.targets file for iOS is empty (no linker hints provided). The issue is triggered by the Windows-based Hot Restart deployment mechanism, which does not support native framework embedding.

**Recommendations:** **needs-investigation** — Multiple users confirm the crash with clear workaround, but root cause (Hot Restart limitation vs. linker stripping vs. missing NativeReference hints in targets) needs investigation to determine if SkiaSharp can provide a packaging fix.

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

## Evidence

### Reproduction

1. Create a .NET MAUI app targeting iOS
2. Add SkiaSharp.Views.Maui.Controls 2.88.3 NuGet package
3. Instantiate SKPaint in a page constructor
4. Deploy from Visual Studio on Windows to a physical iOS device (using Hot Restart)
5. Observe crash: DllNotFoundException for @rpath/libSkiaSharp.framework/libSkiaSharp

**Environment:** Visual Studio 2022 17.5.0 (Windows 11), MAUI net6.0-ios13.6, iPad 9th Gen iPadOS 16.3.1, SkiaSharp.Views.Maui.Controls 2.88.3

**Related issues:** #1894, #2437

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | @rpath/libSkiaSharp.framework/libSkiaSharp |
| Repro quality | partial |
| Target frameworks | net6.0-ios |

**Stack trace:**

```text
at SkiaSharp.SKPaint..ctor()
   at MauiApp1_60.MainPage..ctor()
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | Issue filed against 2.88.3; current version is newer. Related issue #2437 was closed as completed but no explicit fix commit identified. |

## Analysis

### Technical Summary

The app crashes on physical iOS device because the native libSkiaSharp.framework is not available at runtime when deploying via Visual Studio Hot Restart on Windows. Hot Restart only bundles managed assemblies, not native frameworks. Deploying via a Mac (remote devices) correctly embeds the framework in the app bundle. The SkiaSharp.NativeAssets.iOS package ships the framework bundle correctly (runtimes/ios/native/libSkiaSharp.framework), and the buildTransitive/SkiaSharp.targets file for iOS is empty (no linker hints provided). The issue is triggered by the Windows-based Hot Restart deployment mechanism, which does not support native framework embedding.

### Rationale

The DllNotFoundException with @rpath prefix is a runtime iOS framework loading failure. The framework IS included in the NuGet package (confirmed by examining SkiaSharp.NativeAssets.iOS.csproj). Multiple users confirm the same failure, always on physical device via Windows deployment, and always resolved by deploying via Mac. This pattern exactly matches the known Hot Restart limitation: it skips the native framework copy step. Related closed issue #2437 (completed) had a community-found workaround via linker flags but no official SkiaSharp fix. This is partially external (Hot Restart limitation in MAUI tooling) but SkiaSharp could potentially mitigate it with linker preservation hints.

### Key Signals

- "'@rpath/libSkiaSharp.framework/libSkiaSharp'. Callstack: at SkiaSharp.SKPaint..ctor()" — **issue body** (Classic iOS framework not found at @rpath — the framework was not embedded in the app bundle during deployment.)
- "App crashes only when i try to deploy to iPad. For Android and Windows, it works." — **issue body** (Platform-specific failure limited to physical iOS device deployment — points to deployment mechanism, not SkiaSharp code.)
- "i connect my iPad directly to the Mac, and deploy the app to iOS Remote Devices -> device rather than doing the Hot Reload thingy" — **comment #1506543879** (The workaround explicitly bypasses Hot Restart. Hot Restart is the likely root cause.)
- "If you're using a Windows machine and Hot Restart, this is the issue: https://github.com/Mapsui/Mapsui/issues/1872" — **comment on #2437** (Community confirms this is a Hot Restart limitation on Windows — same issue reported in other libraries using native frameworks.)
- "Adding <MtouchLink>None</MtouchLink> is a simple way of making it go away" — **comment on #2437** (Alternative workaround disabling iOS linker to prevent framework stripping — suggests the linker may also be involved in some configurations.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.iOS/SkiaSharp.NativeAssets.iOS.csproj` | — | direct | Framework bundle is correctly packaged at runtimes/ios/native/libSkiaSharp.framework from output/native/ios/. The NuGet package structure is correct. |
| `binding/SkiaSharp.NativeAssets.iOS/buildTransitive/SkiaSharp.targets` | — | direct | The buildTransitive targets file for iOS is completely empty (no content). No linker preservation hints, no NativeReference items, and no MtouchLink configuration are included. This means no workaround is shipped to protect against linker stripping or Hot Restart framework exclusion. |

### Workarounds

- Deploy to iOS device via a Mac build host (iOS Remote Devices in VS for Windows) instead of using Hot Restart
- Add <MtouchLink>None</MtouchLink> to the iOS section of the MAUI csproj to disable linker (development only, not for release builds)
- Set minimum iOS target framework to net6.0-ios13.6 to match the SkiaSharp NuGet TFM

### Next Questions

- Does the issue reproduce with SkiaSharp 3.x (current version) or only with 2.88.3?
- Does adding a NativeReference to libSkiaSharp.framework in the .csproj prevent the Hot Restart issue?
- Can SkiaSharp add a linker preservation hint (XmlLinkDescription or preserve attribute) to the iOS targets file to prevent framework stripping?

### Resolution Proposals

**Hypothesis:** The libSkiaSharp.framework is not being embedded by Hot Restart (Windows-based iOS deployment). SkiaSharp could potentially mitigate this by adding NativeReference or linker preservation hints to its iOS buildTransitive targets, though the root cause is in MAUI tooling.

1. **Deploy via Mac (immediate workaround)** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Connect the iOS device to a Mac and use Visual Studio's iOS Remote Devices (pair-to-Mac) deployment instead of Hot Restart. This ensures the full app bundle including native frameworks is built and deployed correctly.
2. **Disable iOS linker during development** — workaround, confidence 0.80 (80%), cost/xs, validated=yes
   - Add <MtouchLink>None</MtouchLink> inside the iOS-specific PropertyGroup in the .csproj. This prevents the linker from stripping the native framework reference. Use only during development; for release, configure linker exceptions instead.

```csharp
<PropertyGroup Condition="$(TargetFramework.Contains('-ios'))">
  <MtouchLink>None</MtouchLink>
</PropertyGroup>
```
3. **Add linker preservation hints to SkiaSharp iOS targets** — fix, confidence 0.65 (65%), cost/s, validated=untested
   - Populate binding/SkiaSharp.NativeAssets.iOS/buildTransitive/SkiaSharp.targets with a NativeReference or linker XML description to ensure libSkiaSharp.framework is preserved by the linker and included by Hot Restart.

**Recommended proposal:** Deploy via Mac (immediate workaround)

**Why:** Immediately actionable, confirmed working by the original reporter and other commenters. The linker/targets fix requires investigation.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.78 (78%) |
| Reason | Multiple users confirm the crash with clear workaround, but root cause (Hot Restart limitation vs. linker stripping vs. missing NativeReference hints in targets) needs investigation to determine if SkiaSharp can provide a packaging fix. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, native iOS, and MAUI partner labels | labels=type/bug, area/libSkiaSharp.native, os/iOS, tenet/reliability, partner/maui |
| add-comment | medium | 0.82 (82%) | Provide workarounds and explain root cause | — |
| link-related | low | 0.90 (90%) | Cross-reference related iOS DllNotFoundException issue | linkedIssue=#2437 |
| link-related | low | 0.85 (85%) | Cross-reference older related iOS deployment issue | linkedIssue=#1894 |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report, and thanks to everyone who has confirmed this issue.

The crash (`DllNotFoundException: @rpath/libSkiaSharp.framework/libSkiaSharp`) occurs because the native `libSkiaSharp.framework` bundle is **not embedded in the app bundle** when deploying to a physical iOS device via **Visual Studio Hot Restart on Windows**. Hot Restart is a quick-deploy mechanism that skips the full native build/link step, which means native frameworks (`.framework` bundles) are not included.

**Workarounds:**

1. **Deploy via Mac** — Connect your iOS device to a Mac, pair Visual Studio for Windows to the Mac build host, and deploy using *iOS Remote Devices*. This performs a full build and correctly embeds `libSkiaSharp.framework`.

2. **Disable the iOS linker (development only)** — Add this to your `.csproj`:
   ```xml
   <PropertyGroup Condition="$(TargetFramework.Contains('-ios'))">
     <MtouchLink>None</MtouchLink>
   </PropertyGroup>
   ```
   ⚠️ Do not ship release builds with `MtouchLink=None`. For release, configure linker exceptions instead.

3. **Set minimum iOS TFM** — Ensure your project targets `net6.0-ios13.6` or higher (to match the SkiaSharp NuGet TFM).

We will investigate whether SkiaSharp can add linker preservation hints to its iOS build targets to help with this scenario.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2410,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T11:43:00Z"
  },
  "summary": "MAUI app on iOS (iPad) crashes with DllNotFoundException for libSkiaSharp.framework when deployed from Windows using Hot Restart; deploying from Mac works fine.",
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
      "errorMessage": "@rpath/libSkiaSharp.framework/libSkiaSharp",
      "stackTrace": "at SkiaSharp.SKPaint..ctor()\n   at MauiApp1_60.MainPage..ctor()",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net6.0-ios"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET MAUI app targeting iOS",
        "Add SkiaSharp.Views.Maui.Controls 2.88.3 NuGet package",
        "Instantiate SKPaint in a page constructor",
        "Deploy from Visual Studio on Windows to a physical iOS device (using Hot Restart)",
        "Observe crash: DllNotFoundException for @rpath/libSkiaSharp.framework/libSkiaSharp"
      ],
      "environmentDetails": "Visual Studio 2022 17.5.0 (Windows 11), MAUI net6.0-ios13.6, iPad 9th Gen iPadOS 16.3.1, SkiaSharp.Views.Maui.Controls 2.88.3",
      "relatedIssues": [
        1894,
        2437
      ],
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "Issue filed against 2.88.3; current version is newer. Related issue #2437 was closed as completed but no explicit fix commit identified."
    }
  },
  "analysis": {
    "summary": "The app crashes on physical iOS device because the native libSkiaSharp.framework is not available at runtime when deploying via Visual Studio Hot Restart on Windows. Hot Restart only bundles managed assemblies, not native frameworks. Deploying via a Mac (remote devices) correctly embeds the framework in the app bundle. The SkiaSharp.NativeAssets.iOS package ships the framework bundle correctly (runtimes/ios/native/libSkiaSharp.framework), and the buildTransitive/SkiaSharp.targets file for iOS is empty (no linker hints provided). The issue is triggered by the Windows-based Hot Restart deployment mechanism, which does not support native framework embedding.",
    "rationale": "The DllNotFoundException with @rpath prefix is a runtime iOS framework loading failure. The framework IS included in the NuGet package (confirmed by examining SkiaSharp.NativeAssets.iOS.csproj). Multiple users confirm the same failure, always on physical device via Windows deployment, and always resolved by deploying via Mac. This pattern exactly matches the known Hot Restart limitation: it skips the native framework copy step. Related closed issue #2437 (completed) had a community-found workaround via linker flags but no official SkiaSharp fix. This is partially external (Hot Restart limitation in MAUI tooling) but SkiaSharp could potentially mitigate it with linker preservation hints.",
    "keySignals": [
      {
        "text": "'@rpath/libSkiaSharp.framework/libSkiaSharp'. Callstack: at SkiaSharp.SKPaint..ctor()",
        "source": "issue body",
        "interpretation": "Classic iOS framework not found at @rpath — the framework was not embedded in the app bundle during deployment."
      },
      {
        "text": "App crashes only when i try to deploy to iPad. For Android and Windows, it works.",
        "source": "issue body",
        "interpretation": "Platform-specific failure limited to physical iOS device deployment — points to deployment mechanism, not SkiaSharp code."
      },
      {
        "text": "i connect my iPad directly to the Mac, and deploy the app to iOS Remote Devices -> device rather than doing the Hot Reload thingy",
        "source": "comment #1506543879",
        "interpretation": "The workaround explicitly bypasses Hot Restart. Hot Restart is the likely root cause."
      },
      {
        "text": "If you're using a Windows machine and Hot Restart, this is the issue: https://github.com/Mapsui/Mapsui/issues/1872",
        "source": "comment on #2437",
        "interpretation": "Community confirms this is a Hot Restart limitation on Windows — same issue reported in other libraries using native frameworks."
      },
      {
        "text": "Adding <MtouchLink>None</MtouchLink> is a simple way of making it go away",
        "source": "comment on #2437",
        "interpretation": "Alternative workaround disabling iOS linker to prevent framework stripping — suggests the linker may also be involved in some configurations."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.iOS/SkiaSharp.NativeAssets.iOS.csproj",
        "finding": "Framework bundle is correctly packaged at runtimes/ios/native/libSkiaSharp.framework from output/native/ios/. The NuGet package structure is correct.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.NativeAssets.iOS/buildTransitive/SkiaSharp.targets",
        "finding": "The buildTransitive targets file for iOS is completely empty (no content). No linker preservation hints, no NativeReference items, and no MtouchLink configuration are included. This means no workaround is shipped to protect against linker stripping or Hot Restart framework exclusion.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Deploy to iOS device via a Mac build host (iOS Remote Devices in VS for Windows) instead of using Hot Restart",
      "Add <MtouchLink>None</MtouchLink> to the iOS section of the MAUI csproj to disable linker (development only, not for release builds)",
      "Set minimum iOS target framework to net6.0-ios13.6 to match the SkiaSharp NuGet TFM"
    ],
    "nextQuestions": [
      "Does the issue reproduce with SkiaSharp 3.x (current version) or only with 2.88.3?",
      "Does adding a NativeReference to libSkiaSharp.framework in the .csproj prevent the Hot Restart issue?",
      "Can SkiaSharp add a linker preservation hint (XmlLinkDescription or preserve attribute) to the iOS targets file to prevent framework stripping?"
    ],
    "resolution": {
      "hypothesis": "The libSkiaSharp.framework is not being embedded by Hot Restart (Windows-based iOS deployment). SkiaSharp could potentially mitigate this by adding NativeReference or linker preservation hints to its iOS buildTransitive targets, though the root cause is in MAUI tooling.",
      "proposals": [
        {
          "title": "Deploy via Mac (immediate workaround)",
          "description": "Connect the iOS device to a Mac and use Visual Studio's iOS Remote Devices (pair-to-Mac) deployment instead of Hot Restart. This ensures the full app bundle including native frameworks is built and deployed correctly.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Disable iOS linker during development",
          "description": "Add <MtouchLink>None</MtouchLink> inside the iOS-specific PropertyGroup in the .csproj. This prevents the linker from stripping the native framework reference. Use only during development; for release, configure linker exceptions instead.",
          "codeSnippet": "<PropertyGroup Condition=\"$(TargetFramework.Contains('-ios'))\">\n  <MtouchLink>None</MtouchLink>\n</PropertyGroup>",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Add linker preservation hints to SkiaSharp iOS targets",
          "description": "Populate binding/SkiaSharp.NativeAssets.iOS/buildTransitive/SkiaSharp.targets with a NativeReference or linker XML description to ensure libSkiaSharp.framework is preserved by the linker and included by Hot Restart.",
          "category": "fix",
          "confidence": 0.65,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Deploy via Mac (immediate workaround)",
      "recommendedReason": "Immediately actionable, confirmed working by the original reporter and other commenters. The linker/targets fix requires investigation."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.78,
      "reason": "Multiple users confirm the crash with clear workaround, but root cause (Hot Restart limitation vs. linker stripping vs. missing NativeReference hints in targets) needs investigation to determine if SkiaSharp can provide a packaging fix.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, native iOS, and MAUI partner labels",
        "risk": "low",
        "confidence": 0.95,
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
        "description": "Provide workarounds and explain root cause",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thank you for the detailed report, and thanks to everyone who has confirmed this issue.\n\nThe crash (`DllNotFoundException: @rpath/libSkiaSharp.framework/libSkiaSharp`) occurs because the native `libSkiaSharp.framework` bundle is **not embedded in the app bundle** when deploying to a physical iOS device via **Visual Studio Hot Restart on Windows**. Hot Restart is a quick-deploy mechanism that skips the full native build/link step, which means native frameworks (`.framework` bundles) are not included.\n\n**Workarounds:**\n\n1. **Deploy via Mac** — Connect your iOS device to a Mac, pair Visual Studio for Windows to the Mac build host, and deploy using *iOS Remote Devices*. This performs a full build and correctly embeds `libSkiaSharp.framework`.\n\n2. **Disable the iOS linker (development only)** — Add this to your `.csproj`:\n   ```xml\n   <PropertyGroup Condition=\"$(TargetFramework.Contains('-ios'))\">\n     <MtouchLink>None</MtouchLink>\n   </PropertyGroup>\n   ```\n   ⚠️ Do not ship release builds with `MtouchLink=None`. For release, configure linker exceptions instead.\n\n3. **Set minimum iOS TFM** — Ensure your project targets `net6.0-ios13.6` or higher (to match the SkiaSharp NuGet TFM).\n\nWe will investigate whether SkiaSharp can add linker preservation hints to its iOS build targets to help with this scenario."
      },
      {
        "type": "link-related",
        "description": "Cross-reference related iOS DllNotFoundException issue",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 2437
      },
      {
        "type": "link-related",
        "description": "Cross-reference older related iOS deployment issue",
        "risk": "low",
        "confidence": 0.85,
        "linkedIssue": 1894
      }
    ]
  }
}
```

</details>
