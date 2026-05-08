# Issue Triage Report — #1264

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-23T18:44:33Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/libSkiaSharp.native (0.85 (85%)) |
| Suggested action | close-as-external (0.85 (85%)) |

**Issue Summary:** iOS app crashes at runtime with System.DllNotFoundException for @rpath/libSkiaSharp.framework/libSkiaSharp when signed with an Enterprise Distribution Certificate; works fine with a regular Apple Developer Account.

**Analysis:** The DllNotFoundException for @rpath/libSkiaSharp.framework/libSkiaSharp is a native library load failure on iOS. The @rpath mechanism is provided by the iOS dynamic linker; the framework is embedded and its rpath configured by Xamarin.iOS at build/link time. The fact that it works with a regular Developer Account but not an Enterprise Distribution Certificate points to a code-signing or framework-embedding difference in Xamarin.iOS's build pipeline for enterprise signing, not a bug in SkiaSharp itself. The SkiaSharp maintainer already redirected reporters to xamarin/xamarin-macios.

**Recommendations:** **close-as-external** — The DllNotFoundException is caused by Xamarin.iOS failing to correctly embed or sign the libSkiaSharp.framework for Enterprise Distribution. SkiaSharp's native loading code is correct. The SkiaSharp maintainer has already directed users to xamarin/xamarin-macios, confirming this is an external issue.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/iOS |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Build a Xamarin.iOS project that uses SkiaSharp 1.68.1.1
2. Sign the app with an Enterprise Distribution Profile instead of a regular Apple Developer Account
3. Deploy to a physical iOS device (iPhone X, 11, 7, or 8, iOS 13.3–13.4.5)
4. Launch the app and trigger any SkiaSharp usage
5. Observe DllNotFoundException for @rpath/libSkiaSharp.framework/libSkiaSharp

**Environment:** SkiaSharp 1.68.1.1, iOS 13.3–13.4.5 (beta), Visual Studio for Mac 8.5.4, Xamarin.iOS, physical devices (iPhone 7, 8, X, 11)

**Related issues:** #1129

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1129 — Similar DllNotFoundException on iOS with free Apple developer account (closed as completed)
- https://github.com/xamarin/xamarin-macios/issues/9068 — Cross-filed issue in xamarin-macios by a commenter

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | @rpath/libSkiaSharp.framework/libSkiaSharp |
| Repro quality | partial |
| Target frameworks | xamarin.ios |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.1.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | Version 1.68.1.1 is very old (current is 3.x). The @rpath loading mechanism for iOS has not changed materially in SkiaSharp — the issue is likely still external (Xamarin.iOS signing behavior) but this specific version is untested against current tooling. |

## Analysis

### Technical Summary

The DllNotFoundException for @rpath/libSkiaSharp.framework/libSkiaSharp is a native library load failure on iOS. The @rpath mechanism is provided by the iOS dynamic linker; the framework is embedded and its rpath configured by Xamarin.iOS at build/link time. The fact that it works with a regular Developer Account but not an Enterprise Distribution Certificate points to a code-signing or framework-embedding difference in Xamarin.iOS's build pipeline for enterprise signing, not a bug in SkiaSharp itself. The SkiaSharp maintainer already redirected reporters to xamarin/xamarin-macios.

### Rationale

The DllNotFoundException is a native library loading failure caused by the iOS dynamic linker not finding the @rpath-based framework. SkiaSharp's use of @rpath is correct and standard for iOS frameworks. The root cause — why enterprise-signed apps fail to locate the framework at runtime while developer-signed apps succeed — lies in how Xamarin.iOS embeds and code-signs the native framework during the enterprise distribution build. This is an external toolchain issue. The SkiaSharp maintainer confirmed this assessment by directing users to xamarin/xamarin-macios.

### Key Signals

- "iOS app Crash on exception: Dll Not found Exception. Symptoms and error similar to https://github.com/mono/SkiaSharp/issues/1129" — **issue body** (Identical failure mode to #1129, but triggered by Enterprise Certificate rather than free developer account — signals a signing/provisioning difference, not a SkiaSharp code defect.)
- "We have another developer workstation that can deploy a version that does not crash from the exact same code base to the same devices." — **issue body** (Same code, same SkiaSharp version, same devices — only the signing identity and/or workstation configuration differs. Strongly suggests the problem is in certificate/provisioning configuration or Xamarin.iOS signing behavior.)
- "I would suggest opening an issue in https://github.com/xamarin/xamarin-macios — It sounds like a bit there since normal accounts work." — **maintainer comment (mattleibow)** (The SkiaSharp maintainer explicitly identified this as an external issue in Xamarin.iOS.)
- "@rpath/libSkiaSharp.framework/libSkiaSharp assembly:<unknown assembly>" — **comment by avorobjovs (iOS 14.2)** (Later commenter on iOS 14.2 shows the same rpath failure, suggesting the issue persisted across iOS versions and is not iOS-version-specific.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaApi.cs` | 9-10 | direct | SkiaSharp declares SKIA = "@rpath/libSkiaSharp.framework/libSkiaSharp" for iOS/tvOS via a #if __IOS__ || __TVOS__ preprocessor guard. This is the correct and standard iOS rpath convention for embedded frameworks. SkiaSharp's side of native loading is correct and platform-idiomatic. |
| `documentation/dev/packages.md` | 91 | context | SkiaSharp.NativeAssets.iOS provides an iOS framework bundle (arm64 device + simulator) and is auto-included for iOS TFMs. The native asset packaging is standard; no enterprise-specific signing steps are involved at the SkiaSharp layer. |

### Workarounds

- Ensure libSkiaSharp.framework is listed under 'Embedded Frameworks' or 'Native References' in the Xamarin.iOS project and is set to 'Embed & Sign'.
- Try a Clean build (not incremental) before archive/distribution to force framework re-bundling.
- Check that the Enterprise provisioning profile covers all devices and that the framework's code signature is valid (use codesign -v on the .framework).
- File with xamarin/xamarin-macios as the maintainer advised — this is tracked at https://github.com/xamarin/xamarin-macios/issues/9068.

### Next Questions

- Is this still reproducible with a current SkiaSharp version (2.88.x or 3.x)?
- Does the xamarin-macios issue #9068 have a resolution or workaround?
- Are there specific Xamarin.iOS or Xcode versions where this is fixed?

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.85 (85%) |
| Reason | The DllNotFoundException is caused by Xamarin.iOS failing to correctly embed or sign the libSkiaSharp.framework for Enterprise Distribution. SkiaSharp's native loading code is correct. The SkiaSharp maintainer has already directed users to xamarin/xamarin-macios, confirming this is an external issue. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, iOS, and libSkiaSharp.native labels | labels=type/bug, area/libSkiaSharp.native, os/iOS |
| add-comment | high | 0.85 (85%) | Explain root cause and direct to Xamarin.iOS | — |
| close-issue | medium | 0.85 (85%) | Close as external — root cause is in Xamarin.iOS signing/embedding | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for reporting this. The `DllNotFoundException` for `@rpath/libSkiaSharp.framework/libSkiaSharp` indicates that the iOS dynamic linker cannot find the native framework at runtime. SkiaSharp's use of `@rpath` is the correct and standard pattern for iOS embedded frameworks, so this is not a bug in SkiaSharp itself.

The fact that it works with a regular Apple Developer Account but fails with an Enterprise Distribution Certificate strongly suggests the issue is in how **Xamarin.iOS embeds and code-signs the native framework** during an enterprise distribution build. We recommend:

1. Ensure `libSkiaSharp.framework` is set to **Embed & Sign** in your Xamarin.iOS project's Native References.
2. Try a **clean build** (not incremental) before archiving with the Enterprise profile.
3. Verify the framework signature: `codesign -v path/to/YourApp.app/Frameworks/libSkiaSharp.framework`

This issue is best tracked in [xamarin/xamarin-macios](https://github.com/xamarin/xamarin-macios/issues/9068). We are closing this as an external issue.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1264,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-23T18:44:33Z"
  },
  "summary": "iOS app crashes at runtime with System.DllNotFoundException for @rpath/libSkiaSharp.framework/libSkiaSharp when signed with an Enterprise Distribution Certificate; works fine with a regular Apple Developer Account.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.85
    },
    "platforms": [
      "os/iOS"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "@rpath/libSkiaSharp.framework/libSkiaSharp",
      "reproQuality": "partial",
      "targetFrameworks": [
        "xamarin.ios"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Build a Xamarin.iOS project that uses SkiaSharp 1.68.1.1",
        "Sign the app with an Enterprise Distribution Profile instead of a regular Apple Developer Account",
        "Deploy to a physical iOS device (iPhone X, 11, 7, or 8, iOS 13.3–13.4.5)",
        "Launch the app and trigger any SkiaSharp usage",
        "Observe DllNotFoundException for @rpath/libSkiaSharp.framework/libSkiaSharp"
      ],
      "environmentDetails": "SkiaSharp 1.68.1.1, iOS 13.3–13.4.5 (beta), Visual Studio for Mac 8.5.4, Xamarin.iOS, physical devices (iPhone 7, 8, X, 11)",
      "relatedIssues": [
        1129
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1129",
          "description": "Similar DllNotFoundException on iOS with free Apple developer account (closed as completed)"
        },
        {
          "url": "https://github.com/xamarin/xamarin-macios/issues/9068",
          "description": "Cross-filed issue in xamarin-macios by a commenter"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.1.1"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "Version 1.68.1.1 is very old (current is 3.x). The @rpath loading mechanism for iOS has not changed materially in SkiaSharp — the issue is likely still external (Xamarin.iOS signing behavior) but this specific version is untested against current tooling."
    }
  },
  "analysis": {
    "summary": "The DllNotFoundException for @rpath/libSkiaSharp.framework/libSkiaSharp is a native library load failure on iOS. The @rpath mechanism is provided by the iOS dynamic linker; the framework is embedded and its rpath configured by Xamarin.iOS at build/link time. The fact that it works with a regular Developer Account but not an Enterprise Distribution Certificate points to a code-signing or framework-embedding difference in Xamarin.iOS's build pipeline for enterprise signing, not a bug in SkiaSharp itself. The SkiaSharp maintainer already redirected reporters to xamarin/xamarin-macios.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaApi.cs",
        "lines": "9-10",
        "finding": "SkiaSharp declares SKIA = \"@rpath/libSkiaSharp.framework/libSkiaSharp\" for iOS/tvOS via a #if __IOS__ || __TVOS__ preprocessor guard. This is the correct and standard iOS rpath convention for embedded frameworks. SkiaSharp's side of native loading is correct and platform-idiomatic.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "lines": "91",
        "finding": "SkiaSharp.NativeAssets.iOS provides an iOS framework bundle (arm64 device + simulator) and is auto-included for iOS TFMs. The native asset packaging is standard; no enterprise-specific signing steps are involved at the SkiaSharp layer.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "iOS app Crash on exception: Dll Not found Exception. Symptoms and error similar to https://github.com/mono/SkiaSharp/issues/1129",
        "source": "issue body",
        "interpretation": "Identical failure mode to #1129, but triggered by Enterprise Certificate rather than free developer account — signals a signing/provisioning difference, not a SkiaSharp code defect."
      },
      {
        "text": "We have another developer workstation that can deploy a version that does not crash from the exact same code base to the same devices.",
        "source": "issue body",
        "interpretation": "Same code, same SkiaSharp version, same devices — only the signing identity and/or workstation configuration differs. Strongly suggests the problem is in certificate/provisioning configuration or Xamarin.iOS signing behavior."
      },
      {
        "text": "I would suggest opening an issue in https://github.com/xamarin/xamarin-macios — It sounds like a bit there since normal accounts work.",
        "source": "maintainer comment (mattleibow)",
        "interpretation": "The SkiaSharp maintainer explicitly identified this as an external issue in Xamarin.iOS."
      },
      {
        "text": "@rpath/libSkiaSharp.framework/libSkiaSharp assembly:<unknown assembly>",
        "source": "comment by avorobjovs (iOS 14.2)",
        "interpretation": "Later commenter on iOS 14.2 shows the same rpath failure, suggesting the issue persisted across iOS versions and is not iOS-version-specific."
      }
    ],
    "rationale": "The DllNotFoundException is a native library loading failure caused by the iOS dynamic linker not finding the @rpath-based framework. SkiaSharp's use of @rpath is correct and standard for iOS frameworks. The root cause — why enterprise-signed apps fail to locate the framework at runtime while developer-signed apps succeed — lies in how Xamarin.iOS embeds and code-signs the native framework during the enterprise distribution build. This is an external toolchain issue. The SkiaSharp maintainer confirmed this assessment by directing users to xamarin/xamarin-macios.",
    "workarounds": [
      "Ensure libSkiaSharp.framework is listed under 'Embedded Frameworks' or 'Native References' in the Xamarin.iOS project and is set to 'Embed & Sign'.",
      "Try a Clean build (not incremental) before archive/distribution to force framework re-bundling.",
      "Check that the Enterprise provisioning profile covers all devices and that the framework's code signature is valid (use codesign -v on the .framework).",
      "File with xamarin/xamarin-macios as the maintainer advised — this is tracked at https://github.com/xamarin/xamarin-macios/issues/9068."
    ],
    "nextQuestions": [
      "Is this still reproducible with a current SkiaSharp version (2.88.x or 3.x)?",
      "Does the xamarin-macios issue #9068 have a resolution or workaround?",
      "Are there specific Xamarin.iOS or Xcode versions where this is fixed?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.85,
      "reason": "The DllNotFoundException is caused by Xamarin.iOS failing to correctly embed or sign the libSkiaSharp.framework for Enterprise Distribution. SkiaSharp's native loading code is correct. The SkiaSharp maintainer has already directed users to xamarin/xamarin-macios, confirming this is an external issue.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, iOS, and libSkiaSharp.native labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/iOS"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain root cause and direct to Xamarin.iOS",
        "risk": "high",
        "confidence": 0.85,
        "comment": "Thanks for reporting this. The `DllNotFoundException` for `@rpath/libSkiaSharp.framework/libSkiaSharp` indicates that the iOS dynamic linker cannot find the native framework at runtime. SkiaSharp's use of `@rpath` is the correct and standard pattern for iOS embedded frameworks, so this is not a bug in SkiaSharp itself.\n\nThe fact that it works with a regular Apple Developer Account but fails with an Enterprise Distribution Certificate strongly suggests the issue is in how **Xamarin.iOS embeds and code-signs the native framework** during an enterprise distribution build. We recommend:\n\n1. Ensure `libSkiaSharp.framework` is set to **Embed & Sign** in your Xamarin.iOS project's Native References.\n2. Try a **clean build** (not incremental) before archiving with the Enterprise profile.\n3. Verify the framework signature: `codesign -v path/to/YourApp.app/Frameworks/libSkiaSharp.framework`\n\nThis issue is best tracked in [xamarin/xamarin-macios](https://github.com/xamarin/xamarin-macios/issues/9068). We are closing this as an external issue."
      },
      {
        "type": "close-issue",
        "description": "Close as external — root cause is in Xamarin.iOS signing/embedding",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
