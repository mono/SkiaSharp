# Issue Triage Report — #2421

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T19:10:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/libSkiaSharp.native (0.92 (92%)) |
| Suggested action | close-as-duplicate (0.92 (92%)) |

**Issue Summary:** DllNotFoundException '@rpath/libSkiaSharp.framework/libSkiaSharp' when creating SKPaint on a real iOS device with .NET MAUI 2.88.3 — works on simulator and Android.

**Analysis:** The libSkiaSharp native framework is not being found at @rpath on a real iOS device. This is a native deployment issue — the iOS framework bundle is not correctly embedded in the app when deploying from Windows to a physical device. The iOS simulator works because it uses a different code path; Android works because the native library packaging differs.

**Recommendations:** **close-as-duplicate** — Exact same crash signature as #2410: same error '@rpath/libSkiaSharp.framework/libSkiaSharp', same SKPaint crash, same 2.88.3 version, same iOS real device symptom. The reporter of #2421 even commented on #2410 confirming it's the same issue.

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

1. Create a .NET MAUI app that uses SkiaSharp.Views.Maui.Controls 2.88.3 and SkiaSharp.HarfBuzz 2.88.3
2. Deploy to a real iOS device (not simulator)
3. Observe crash when SKPaint constructor is called

**Environment:** SkiaSharp.Views.Maui.Controls 2.88.3, SkiaSharp.HarfBuzz 2.88.3, .NET MAUI, real iOS device (any iOS version), works on iOS simulator and Android

**Related issues:** #2410

**Repository links:**
- https://github.com/iniceice88/OxyPlot.Maui.Skia — Third-party library using SkiaSharp MAUI that triggers the crash
- https://github.com/mono/SkiaSharp/issues/2410 — Identical crash: same error, same versions, same platform — reporter cross-referenced this issue

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | exception |
| Error message | @rpath/libSkiaSharp.framework/libSkiaSharp |
| Repro quality | partial |
| Target frameworks | net6.0-ios |

**Stack trace:**

```text
at SkiaSharp.SKPaint..ctor() → OxyPlot.Maui.Skia.SkiaRenderContext..ctor()
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The issue is about native library deployment on real iOS devices via Windows, which is a known deployment path problem not tied to a specific SkiaSharp version. |

## Analysis

### Technical Summary

The libSkiaSharp native framework is not being found at @rpath on a real iOS device. This is a native deployment issue — the iOS framework bundle is not correctly embedded in the app when deploying from Windows to a physical device. The iOS simulator works because it uses a different code path; Android works because the native library packaging differs.

### Rationale

This is the same crash as #2410 (identical error message, identical versions, identical symptom: works on simulator not device). The reporter of #2421 even commented on #2410 stating 'My issue regarding this: #2421'. This is a duplicate. The root cause appears to be a native deployment issue when deploying from Windows to a physical iOS device via remote debugging rather than from a Mac directly.

### Key Signals

- "@rpath/libSkiaSharp.framework/libSkiaSharp" — **issue body** (The iOS framework bundle is not found at the expected rpath — library not embedded or rpath not configured.)
- "it works in the emulator but not on a real device" — **issue body** (Simulator and device deployments differ in how framework bundles are embedded. Points to a deployment/linking issue, not a runtime bug.)
- "it works good on android and iOS simulator but not on a real IOS device" — **issue body** (iOS-specific deployment issue with physical device builds.)
- "My issue regarding this: #2421" — **comment on #2410** (The reporter of #2421 confirmed this is the same issue as #2410.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `documentation/dev/packages.md` | — | direct | SkiaSharp.NativeAssets.iOS is listed as auto-included when targeting iOS TFM (net8.0-ios / net6.0-ios). The framework bundle should be automatically deployed. |
| `documentation/dev/packages.md` | — | direct | NativeAssets packages must be referenced in the application project. If only in a transitive library, the binary may not be copied to output — causing DllNotFoundException at runtime. |

### Workarounds

- Deploy by connecting the iOS device directly to a Mac and deploying from there (not via Windows Hot Reload remote), as confirmed working by multiple users in #2410.
- Set Minimum Target iOS Framework to 13.6 (matching SkiaSharp's net6.0-ios13.6 target) in project properties.

### Next Questions

- Is the user deploying from Windows via Hot Reload to a remote device? This deployment path appears broken.
- Is SkiaSharp.NativeAssets.iOS referenced directly in the application project or only transitively?

### Resolution Proposals

**Hypothesis:** When deploying from Windows to a physical iOS device via remote Hot Reload, the native iOS framework bundle is not correctly embedded in the app bundle, causing the @rpath lookup to fail at runtime. Deploying from a Mac directly resolves the issue.

1. **Deploy from Mac directly** — workaround, confidence 0.85 (85%), cost/xs, validated=untested
   - Connect the iOS device to a Mac and deploy from Visual Studio for Mac or Rider, bypassing the Windows-to-iOS-device remote deployment path that fails to embed the native framework.
2. **Close as duplicate of #2410** — investigation, confidence 0.92 (92%), cost/xs, validated=untested
   - This issue is identical to #2410 (same error, same versions, same reporter cross-referencing). Track the fix in #2410.

**Recommended proposal:** Close as duplicate of #2410

**Why:** The reporter themselves cross-referenced #2410 as the same issue. Centralizing discussion there is more effective.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-duplicate |
| Confidence | 0.92 (92%) |
| Reason | Exact same crash signature as #2410: same error '@rpath/libSkiaSharp.framework/libSkiaSharp', same SKPaint crash, same 2.88.3 version, same iOS real device symptom. The reporter of #2421 even commented on #2410 confirming it's the same issue. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, ios, native, maui, reliability labels | labels=type/bug, area/libSkiaSharp.native, os/iOS, partner/maui, tenet/reliability |
| link-duplicate | medium | 0.92 (92%) | Mark as duplicate of #2410 which tracks the same iOS native deployment crash | linkedIssue=#2410 |
| close-issue | medium | 0.90 (90%) | Close as duplicate — tracked in #2410 | stateReason=not_planned |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2421,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T19:10:00Z"
  },
  "summary": "DllNotFoundException '@rpath/libSkiaSharp.framework/libSkiaSharp' when creating SKPaint on a real iOS device with .NET MAUI 2.88.3 — works on simulator and Android.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.92
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
      "errorType": "exception",
      "errorMessage": "@rpath/libSkiaSharp.framework/libSkiaSharp",
      "stackTrace": "at SkiaSharp.SKPaint..ctor() → OxyPlot.Maui.Skia.SkiaRenderContext..ctor()",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net6.0-ios"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET MAUI app that uses SkiaSharp.Views.Maui.Controls 2.88.3 and SkiaSharp.HarfBuzz 2.88.3",
        "Deploy to a real iOS device (not simulator)",
        "Observe crash when SKPaint constructor is called"
      ],
      "environmentDetails": "SkiaSharp.Views.Maui.Controls 2.88.3, SkiaSharp.HarfBuzz 2.88.3, .NET MAUI, real iOS device (any iOS version), works on iOS simulator and Android",
      "repoLinks": [
        {
          "url": "https://github.com/iniceice88/OxyPlot.Maui.Skia",
          "description": "Third-party library using SkiaSharp MAUI that triggers the crash"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2410",
          "description": "Identical crash: same error, same versions, same platform — reporter cross-referenced this issue"
        }
      ],
      "relatedIssues": [
        2410
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The issue is about native library deployment on real iOS devices via Windows, which is a known deployment path problem not tied to a specific SkiaSharp version."
    }
  },
  "analysis": {
    "summary": "The libSkiaSharp native framework is not being found at @rpath on a real iOS device. This is a native deployment issue — the iOS framework bundle is not correctly embedded in the app when deploying from Windows to a physical device. The iOS simulator works because it uses a different code path; Android works because the native library packaging differs.",
    "rationale": "This is the same crash as #2410 (identical error message, identical versions, identical symptom: works on simulator not device). The reporter of #2421 even commented on #2410 stating 'My issue regarding this: #2421'. This is a duplicate. The root cause appears to be a native deployment issue when deploying from Windows to a physical iOS device via remote debugging rather than from a Mac directly.",
    "codeInvestigation": [
      {
        "file": "documentation/dev/packages.md",
        "finding": "SkiaSharp.NativeAssets.iOS is listed as auto-included when targeting iOS TFM (net8.0-ios / net6.0-ios). The framework bundle should be automatically deployed.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "finding": "NativeAssets packages must be referenced in the application project. If only in a transitive library, the binary may not be copied to output — causing DllNotFoundException at runtime.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "@rpath/libSkiaSharp.framework/libSkiaSharp",
        "source": "issue body",
        "interpretation": "The iOS framework bundle is not found at the expected rpath — library not embedded or rpath not configured."
      },
      {
        "text": "it works in the emulator but not on a real device",
        "source": "issue body",
        "interpretation": "Simulator and device deployments differ in how framework bundles are embedded. Points to a deployment/linking issue, not a runtime bug."
      },
      {
        "text": "it works good on android and iOS simulator but not on a real IOS device",
        "source": "issue body",
        "interpretation": "iOS-specific deployment issue with physical device builds."
      },
      {
        "text": "My issue regarding this: #2421",
        "source": "comment on #2410",
        "interpretation": "The reporter of #2421 confirmed this is the same issue as #2410."
      }
    ],
    "workarounds": [
      "Deploy by connecting the iOS device directly to a Mac and deploying from there (not via Windows Hot Reload remote), as confirmed working by multiple users in #2410.",
      "Set Minimum Target iOS Framework to 13.6 (matching SkiaSharp's net6.0-ios13.6 target) in project properties."
    ],
    "nextQuestions": [
      "Is the user deploying from Windows via Hot Reload to a remote device? This deployment path appears broken.",
      "Is SkiaSharp.NativeAssets.iOS referenced directly in the application project or only transitively?"
    ],
    "resolution": {
      "hypothesis": "When deploying from Windows to a physical iOS device via remote Hot Reload, the native iOS framework bundle is not correctly embedded in the app bundle, causing the @rpath lookup to fail at runtime. Deploying from a Mac directly resolves the issue.",
      "proposals": [
        {
          "title": "Deploy from Mac directly",
          "description": "Connect the iOS device to a Mac and deploy from Visual Studio for Mac or Rider, bypassing the Windows-to-iOS-device remote deployment path that fails to embed the native framework.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Close as duplicate of #2410",
          "description": "This issue is identical to #2410 (same error, same versions, same reporter cross-referencing). Track the fix in #2410.",
          "category": "investigation",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Close as duplicate of #2410",
      "recommendedReason": "The reporter themselves cross-referenced #2410 as the same issue. Centralizing discussion there is more effective."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-duplicate",
      "confidence": 0.92,
      "reason": "Exact same crash signature as #2410: same error '@rpath/libSkiaSharp.framework/libSkiaSharp', same SKPaint crash, same 2.88.3 version, same iOS real device symptom. The reporter of #2421 even commented on #2410 confirming it's the same issue.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, ios, native, maui, reliability labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/iOS",
          "partner/maui",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-duplicate",
        "description": "Mark as duplicate of #2410 which tracks the same iOS native deployment crash",
        "risk": "medium",
        "confidence": 0.92,
        "linkedIssue": 2410
      },
      {
        "type": "close-issue",
        "description": "Close as duplicate — tracked in #2410",
        "risk": "medium",
        "confidence": 0.9,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
