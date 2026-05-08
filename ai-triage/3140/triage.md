# Issue Triage Report — #3140

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T20:05:26Z |
| Type | type/bug (0.80 (80%)) |
| Area | area/SkiaSharp.Views.Maui (0.85 (85%)) |
| Suggested action | needs-info (0.88 (88%)) |

**Issue Summary:** iOS app using landscape-only orientation crashes with 'unable to convert SKCanvasViewHandler to UIKit.UIView' when MediaGallery.CapturePhotoAsync is called while device is in portrait orientation.

**Analysis:** The crash occurs when iOS attempts to present a portrait-mode camera picker over a landscape-locked MAUI app. iOS traverses the native view hierarchy to manage orientation, and encounters what appears to be an invalid UIView reference — the error message 'unable to convert SKCanvasViewHandler to UIKit.UIView' suggests MAUI's handler infrastructure is exposing the managed handler object to UIKit instead of the native platform view. The reporter's repro code uses MediaGallery plugin and contains no SkiaSharp calls, so the SkiaSharp canvas must be somewhere else in the app's UI tree.

**Recommendations:** **needs-info** — No stack trace or log output provided. The repro code shown does not include SkiaSharp usage. SkiaSharp version fields are contradictory. Cannot diagnose without more information.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/iOS |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | partner/maui |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a MAUI app with SKCanvasView using UseSkiaSharp()
2. Lock app to LandscapeLeft and LandscapeRight orientations only
3. Hold device in portrait mode
4. Call MediaGallery.CapturePhotoAsync() to open camera UI
5. Observe crash

**Environment:** iPadOS 18.2.1, iPad 10th Gen, Visual Studio for Mac, SkiaSharp 2.88.8

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | crash |
| Error message | unable to convert skiasharp.views.maui.handlers.skcanvasviewhandler to uikit.uiview |
| Repro quality | partial |
| Target frameworks | net8.0-ios |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.8, 2.88.9 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | Reporter lists 2.88.8 as current and 2.88.9 as last known good, which is contradictory since 2.88.9 > 2.88.8. Version information may be entered incorrectly. |

## Analysis

### Technical Summary

The crash occurs when iOS attempts to present a portrait-mode camera picker over a landscape-locked MAUI app. iOS traverses the native view hierarchy to manage orientation, and encounters what appears to be an invalid UIView reference — the error message 'unable to convert SKCanvasViewHandler to UIKit.UIView' suggests MAUI's handler infrastructure is exposing the managed handler object to UIKit instead of the native platform view. The reporter's repro code uses MediaGallery plugin and contains no SkiaSharp calls, so the SkiaSharp canvas must be somewhere else in the app's UI tree.

### Rationale

Classified as type/bug because a crash during normal app usage (photo capture) is not expected behavior. Area is area/SkiaSharp.Views.Maui because the error message directly references SKCanvasViewHandler from that package. Marked needs-info because no stack trace or log output was provided and the SkiaSharp usage context is missing from the repro code.

### Key Signals

- "unable to convert skiasharp.views.maui.handlers.skcanvasviewhandler to uikit.uiview" — **issue title** (iOS UIKit is encountering the managed MAUI handler type where it expects a native UIView — likely triggered during orientation management when presenting the camera picker.)
- "Our iOS app only supports the UIInterfaceOrientation.LandscapeLeft, UIInterfaceOrientation.LandscapeRight Orientation" — **issue body** (Landscape-only apps can trigger special UIKit code paths when presenting view controllers that support portrait orientation (like the camera picker), potentially causing MAUI to be queried for UIView conversions.)
- "await MediaGallery.CapturePhotoAsync(cts.Token)" — **issue body** (The repro code is MediaGallery plugin (not SkiaSharp). This means SkiaSharp's SKCanvasView must be present elsewhere in the app's view hierarchy and is encountered when iOS traverses the hierarchy for orientation change.)
- "Version of SkiaSharp: 2.88.8 (Deprecated) / Last Known Good: 2.88.9 (Previous)" — **issue body** (Version fields appear to be entered incorrectly — 2.88.9 is newer than 2.88.8 so cannot be 'last known good' for 2.88.8. No reliable regression baseline.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Apple.cs` | 8-13 | direct | SKCanvasViewHandler on Apple/iOS inherits from ViewHandler<ISKCanvasView, SKCanvasView> where SKCanvasView is SkiaSharp.Views.iOS.SKCanvasView, a UIView subclass. The CreatePlatformView returns a proper UIView instance. The handler itself is NOT a UIView, so if iOS/MAUI passes the handler object to UIKit expecting a UIView, the cast fails. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/AppHostBuilderExtensions.cs` | 14-16 | related | UseSkiaSharp() registers SKCanvasViewHandler for SKCanvasView via MAUI's handler infrastructure. If MAUI or iOS interop fails to unwrap the handler to its PlatformView during UIKit traversal, the raw handler type would be exposed — matching the error. |

### Next Questions

- What is the full stack trace of the crash?
- Is there log output from the crash that was not captured?
- How is SKCanvasView used in the app (which page/screen)?
- Does the crash occur on iPhone as well, or only on iPad?
- Which version of .NET MAUI is in use?
- Does removing UseSkiaSharp() from the MauiProgram prevent the crash?

### Resolution Proposals

**Hypothesis:** iOS UIKit calls a method to retrieve a UIView from MAUI's view tree during orientation management when presenting the camera picker. MAUI's handler dispatch returns the SKCanvasViewHandler object instead of its platform UIView, causing a type-cast exception.

1. **Request full crash log and SkiaSharp usage context** — investigation, confidence 0.95 (95%), cost/xs, validated=untested
   - Ask reporter for the complete crash log/stack trace and how SKCanvasView is used in the app. Without this, it is impossible to determine whether the bug is in SkiaSharp or in MAUI's handler infrastructure.

**Recommended proposal:** Request full crash log and SkiaSharp usage context

**Why:** Critical information is missing. Without a stack trace and SkiaSharp usage code, the root cause cannot be determined — the bug may be in MAUI rather than SkiaSharp.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.88 (88%) |
| Reason | No stack trace or log output provided. The repro code shown does not include SkiaSharp usage. SkiaSharp version fields are contradictory. Cannot diagnose without more information. |
| Suggested repro platform | macos |

### Missing Info

- Full crash stack trace / device log
- How SKCanvasView is used in the app (page/component)
- .NET MAUI version
- Whether crash reproduces on iPhone (not just iPad)
- Correct version information (2.88.8 vs 2.88.9 entries are contradictory)

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Add area, platform, tenet, and partner labels | labels=type/bug, area/SkiaSharp.Views.Maui, os/iOS, tenet/reliability, partner/maui |
| add-comment | medium | 0.88 (88%) | Ask reporter for stack trace, SkiaSharp usage context, and MAUI version | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for reporting this issue.

To help investigate the crash, we need a few more details:

1. **Full crash log / stack trace** — Please run the app with a debugger attached (or retrieve the device crash log) and share the complete stack trace. The error message in the title is helpful, but without the call stack we can't determine where the conversion failure occurs.

2. **SkiaSharp usage in your app** — The repro code you shared (`MediaGallery.CapturePhotoAsync`) doesn't include any SkiaSharp calls. Could you share how `SKCanvasView` is used in your app (which page/control), and confirm that removing it changes the behavior?

3. **.NET MAUI version** — Which version of .NET MAUI (and .NET) are you targeting?

4. **Version clarification** — The version fields list 2.88.8 as current and 2.88.9 as "last known good" — since 2.88.9 is a newer release than 2.88.8, could you clarify which version you're using and which version worked correctly?

5. **Isolation test** — Does the crash still occur if you call `MediaGallery.CapturePhotoAsync` from a page that does **not** contain an `SKCanvasView`?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3140,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T20:05:26Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "iOS app using landscape-only orientation crashes with 'unable to convert SKCanvasViewHandler to UIKit.UIView' when MediaGallery.CapturePhotoAsync is called while device is in portrait orientation.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.8
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
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
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "unable to convert skiasharp.views.maui.handlers.skcanvasviewhandler to uikit.uiview",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0-ios"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a MAUI app with SKCanvasView using UseSkiaSharp()",
        "Lock app to LandscapeLeft and LandscapeRight orientations only",
        "Hold device in portrait mode",
        "Call MediaGallery.CapturePhotoAsync() to open camera UI",
        "Observe crash"
      ],
      "environmentDetails": "iPadOS 18.2.1, iPad 10th Gen, Visual Studio for Mac, SkiaSharp 2.88.8"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.8",
        "2.88.9"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "Reporter lists 2.88.8 as current and 2.88.9 as last known good, which is contradictory since 2.88.9 > 2.88.8. Version information may be entered incorrectly."
    }
  },
  "analysis": {
    "summary": "The crash occurs when iOS attempts to present a portrait-mode camera picker over a landscape-locked MAUI app. iOS traverses the native view hierarchy to manage orientation, and encounters what appears to be an invalid UIView reference — the error message 'unable to convert SKCanvasViewHandler to UIKit.UIView' suggests MAUI's handler infrastructure is exposing the managed handler object to UIKit instead of the native platform view. The reporter's repro code uses MediaGallery plugin and contains no SkiaSharp calls, so the SkiaSharp canvas must be somewhere else in the app's UI tree.",
    "rationale": "Classified as type/bug because a crash during normal app usage (photo capture) is not expected behavior. Area is area/SkiaSharp.Views.Maui because the error message directly references SKCanvasViewHandler from that package. Marked needs-info because no stack trace or log output was provided and the SkiaSharp usage context is missing from the repro code.",
    "keySignals": [
      {
        "text": "unable to convert skiasharp.views.maui.handlers.skcanvasviewhandler to uikit.uiview",
        "source": "issue title",
        "interpretation": "iOS UIKit is encountering the managed MAUI handler type where it expects a native UIView — likely triggered during orientation management when presenting the camera picker."
      },
      {
        "text": "Our iOS app only supports the UIInterfaceOrientation.LandscapeLeft, UIInterfaceOrientation.LandscapeRight Orientation",
        "source": "issue body",
        "interpretation": "Landscape-only apps can trigger special UIKit code paths when presenting view controllers that support portrait orientation (like the camera picker), potentially causing MAUI to be queried for UIView conversions."
      },
      {
        "text": "await MediaGallery.CapturePhotoAsync(cts.Token)",
        "source": "issue body",
        "interpretation": "The repro code is MediaGallery plugin (not SkiaSharp). This means SkiaSharp's SKCanvasView must be present elsewhere in the app's view hierarchy and is encountered when iOS traverses the hierarchy for orientation change."
      },
      {
        "text": "Version of SkiaSharp: 2.88.8 (Deprecated) / Last Known Good: 2.88.9 (Previous)",
        "source": "issue body",
        "interpretation": "Version fields appear to be entered incorrectly — 2.88.9 is newer than 2.88.8 so cannot be 'last known good' for 2.88.8. No reliable regression baseline."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Apple.cs",
        "lines": "8-13",
        "finding": "SKCanvasViewHandler on Apple/iOS inherits from ViewHandler<ISKCanvasView, SKCanvasView> where SKCanvasView is SkiaSharp.Views.iOS.SKCanvasView, a UIView subclass. The CreatePlatformView returns a proper UIView instance. The handler itself is NOT a UIView, so if iOS/MAUI passes the handler object to UIKit expecting a UIView, the cast fails.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/AppHostBuilderExtensions.cs",
        "lines": "14-16",
        "finding": "UseSkiaSharp() registers SKCanvasViewHandler for SKCanvasView via MAUI's handler infrastructure. If MAUI or iOS interop fails to unwrap the handler to its PlatformView during UIKit traversal, the raw handler type would be exposed — matching the error.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "What is the full stack trace of the crash?",
      "Is there log output from the crash that was not captured?",
      "How is SKCanvasView used in the app (which page/screen)?",
      "Does the crash occur on iPhone as well, or only on iPad?",
      "Which version of .NET MAUI is in use?",
      "Does removing UseSkiaSharp() from the MauiProgram prevent the crash?"
    ],
    "resolution": {
      "hypothesis": "iOS UIKit calls a method to retrieve a UIView from MAUI's view tree during orientation management when presenting the camera picker. MAUI's handler dispatch returns the SKCanvasViewHandler object instead of its platform UIView, causing a type-cast exception.",
      "proposals": [
        {
          "title": "Request full crash log and SkiaSharp usage context",
          "description": "Ask reporter for the complete crash log/stack trace and how SKCanvasView is used in the app. Without this, it is impossible to determine whether the bug is in SkiaSharp or in MAUI's handler infrastructure.",
          "category": "investigation",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Request full crash log and SkiaSharp usage context",
      "recommendedReason": "Critical information is missing. Without a stack trace and SkiaSharp usage code, the root cause cannot be determined — the bug may be in MAUI rather than SkiaSharp."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.88,
      "reason": "No stack trace or log output provided. The repro code shown does not include SkiaSharp usage. SkiaSharp version fields are contradictory. Cannot diagnose without more information.",
      "suggestedReproPlatform": "macos"
    },
    "missingInfo": [
      "Full crash stack trace / device log",
      "How SKCanvasView is used in the app (page/component)",
      ".NET MAUI version",
      "Whether crash reproduces on iPhone (not just iPad)",
      "Correct version information (2.88.8 vs 2.88.9 entries are contradictory)"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Add area, platform, tenet, and partner labels",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Maui",
          "os/iOS",
          "tenet/reliability",
          "partner/maui"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask reporter for stack trace, SkiaSharp usage context, and MAUI version",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thank you for reporting this issue.\n\nTo help investigate the crash, we need a few more details:\n\n1. **Full crash log / stack trace** — Please run the app with a debugger attached (or retrieve the device crash log) and share the complete stack trace. The error message in the title is helpful, but without the call stack we can't determine where the conversion failure occurs.\n\n2. **SkiaSharp usage in your app** — The repro code you shared (`MediaGallery.CapturePhotoAsync`) doesn't include any SkiaSharp calls. Could you share how `SKCanvasView` is used in your app (which page/control), and confirm that removing it changes the behavior?\n\n3. **.NET MAUI version** — Which version of .NET MAUI (and .NET) are you targeting?\n\n4. **Version clarification** — The version fields list 2.88.8 as current and 2.88.9 as \"last known good\" — since 2.88.9 is a newer release than 2.88.8, could you clarify which version you're using and which version worked correctly?\n\n5. **Isolation test** — Does the crash still occur if you call `MediaGallery.CapturePhotoAsync` from a page that does **not** contain an `SKCanvasView`?"
      }
    ]
  }
}
```

</details>
