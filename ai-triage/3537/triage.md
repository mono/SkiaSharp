# Issue Triage Report — #3537

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-05T21:13:57Z |
| Type | type/enhancement (0.97 (97%)) |
| Area | area/SkiaSharp.Views.Maui (0.95 (95%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** Add scroll wheel support to SkiaSharp's iOS/iPadOS and Mac Catalyst MAUI views using UIPanGestureRecognizer with allowedScrollTypesMask, normalized to the v120 standard (120 units = one discrete mouse wheel notch).

**Analysis:** Scroll wheel support is entirely absent from the Apple (iOS/iPadOS/Mac Catalyst) SKTouchHandler in SkiaSharp.Views.Maui. The handler only processes UITouch events (TouchesBegan, TouchesMoved, TouchesEnded, TouchesCancelled) and has no UIPanGestureRecognizer for scroll input. This is a sub-issue of #3533 which aims to standardize WheelDelta to the v120 convention across all platforms. Implementation requires adding a UIPanGestureRecognizer with allowedScrollTypesMask = [.discrete, .continuous] and normalizing translation deltas to v120 units.

**Recommendations:** **needs-investigation** — Well-specified enhancement from a maintainer with clear implementation path, but requires empirical calibration on real hardware to determine PointsPerNotch constant before implementation can be finalized.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/iOS, os/macOS |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | partner/maui |
| Current labels | type/enhancement, os/iOS, area/SkiaSharp |

## Evidence

### Reproduction

1. Use a MAUI app with SKCanvasView/SKGLView on iOS/iPadOS 13.4+ or Mac Catalyst
2. Connect a Bluetooth mouse or use a Magic Keyboard trackpad
3. Scroll the mouse wheel over the canvas view
4. Observe that no SKTouchAction.WheelChanged events are fired

**Environment:** iOS/iPadOS 13.4+, Mac Catalyst, MAUI with SkiaSharp.Views.Maui

**Related issues:** #3533, #3534, #3535, #3536, #3538, #3539, #3540

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3533 — Parent issue: Standardize WheelDelta to v120 convention across all platforms

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The Apple SKTouchHandler has no scroll/wheel gesture recognizer and the feature has never been implemented. |

## Analysis

### Technical Summary

Scroll wheel support is entirely absent from the Apple (iOS/iPadOS/Mac Catalyst) SKTouchHandler in SkiaSharp.Views.Maui. The handler only processes UITouch events (TouchesBegan, TouchesMoved, TouchesEnded, TouchesCancelled) and has no UIPanGestureRecognizer for scroll input. This is a sub-issue of #3533 which aims to standardize WheelDelta to the v120 convention across all platforms. Implementation requires adding a UIPanGestureRecognizer with allowedScrollTypesMask = [.discrete, .continuous] and normalizing translation deltas to v120 units.

### Rationale

Classified as type/enhancement because scroll wheel support has never existed on this platform — this is entirely new functionality, not a regression or broken behavior. Area is area/SkiaSharp.Views.Maui because the change is entirely within the MAUI views layer (Apple platform handler). The author (a maintainer) filed this as a well-scoped sub-task with clear implementation guidance. Platforms are os/iOS (covers iPadOS) and os/macOS (Mac Catalyst). Partner is partner/maui since this is exclusively a MAUI views concern.

### Key Signals

- "Does NOT have any scroll/wheel gesture recognizer" — **issue body — Current State section** (Confirms the feature is entirely missing, not regressed.)
- "allowedScrollTypesMask is a filter that configures which scroll types the recognizer accepts — it is NOT a per-event classifier" — **issue body — Scroll Type Distinction section** (Implementation caveat: cannot distinguish discrete vs continuous per-event, normalization must work for both.)
- "The PointsPerNotch value is an initial estimate. It has been reported as anywhere from 22 to 60 depending on the mouse and iOS version." — **issue body — Normalization Logic section** (Empirical calibration on real hardware is required before finalizing the implementation.)
- "Minimum iOS version: 13.4 (when allowedScrollTypesMask was introduced)" — **issue body — Implementation Notes** (Need runtime availability check or compile-time guard for the API.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs` | 1-112 | direct | The Apple SKTouchHandler extends UIGestureRecognizer and handles only UITouch events via TouchesBegan, TouchesMoved, TouchesEnded, TouchesCancelled. There is no UIPanGestureRecognizer, no allowedScrollTypesMask configuration, and no scroll/wheel delta handling. SKTouchAction.WheelChanged is never fired from this handler. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs` | 15-21 | related | Constructor accepts onTouchAction callback and scalePixels converter — both would be reused by a scroll gesture handler. No scroll-specific infrastructure exists. |

### Next Questions

- What is the correct PointsPerNotch calibration constant on real iOS/iPadOS hardware with a Bluetooth mouse?
- Should Mac Catalyst use UIPanGestureRecognizer or NSEvent bridging for more precise control?
- Should the scroll recognizer be implemented as a separate gesture recognizer or integrated into the existing SKTouchHandler class?
- How should the iOS 13.4 availability guard be implemented — conditional compilation or runtime check?

### Resolution Proposals

**Hypothesis:** Add a UIPanGestureRecognizer with allowedScrollTypesMask=[discrete,continuous] to SKTouchHandler, fire SKTouchAction.WheelChanged with normalized v120 delta, guarded behind iOS 13.4+ availability.

1. **Add scroll UIPanGestureRecognizer to SKTouchHandler** — fix, confidence 0.85 (85%), cost/m, validated=untested
   - Add a secondary UIPanGestureRecognizer with allowedScrollTypesMask and allowedTouchTypes=[] to SKTouchHandler. On gesture changed, compute incremental translation delta, negate Y, multiply by 120/PointsPerNotch, and fire SKTouchAction.WheelChanged with SKTouchDeviceType.Mouse.
2. **Use NSEvent bridging on Mac Catalyst** — alternative, confidence 0.60 (60%), cost/l, validated=untested
   - On Mac Catalyst, directly handle NSEvent scroll wheel events via AppKit bridging for more precise per-event discrete/continuous classification. Requires platform-specific code path.

**Recommended proposal:** Add scroll UIPanGestureRecognizer to SKTouchHandler

**Why:** The UIPanGestureRecognizer approach works for both iOS and Mac Catalyst with a single code path, aligns with Apple's recommended approach, and reuses existing handler infrastructure.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Well-specified enhancement from a maintainer with clear implementation path, but requires empirical calibration on real hardware to determine PointsPerNotch constant before implementation can be finalized. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Correct area label from area/SkiaSharp to area/SkiaSharp.Views.Maui, add os/macOS for Mac Catalyst, tenet/compatibility, partner/maui | labels=type/enhancement, area/SkiaSharp.Views.Maui, os/iOS, os/macOS, tenet/compatibility, partner/maui |
| link-related | low | 0.99 (99%) | Link to parent issue #3533 (WheelDelta v120 standardization) | linkedIssue=#3533 |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3537,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-05T21:13:57Z",
    "currentLabels": [
      "type/enhancement",
      "os/iOS",
      "area/SkiaSharp"
    ]
  },
  "summary": "Add scroll wheel support to SkiaSharp's iOS/iPadOS and Mac Catalyst MAUI views using UIPanGestureRecognizer with allowedScrollTypesMask, normalized to the v120 standard (120 units = one discrete mouse wheel notch).",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.95
    },
    "platforms": [
      "os/iOS",
      "os/macOS"
    ],
    "tenets": [
      "tenet/compatibility"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Use a MAUI app with SKCanvasView/SKGLView on iOS/iPadOS 13.4+ or Mac Catalyst",
        "Connect a Bluetooth mouse or use a Magic Keyboard trackpad",
        "Scroll the mouse wheel over the canvas view",
        "Observe that no SKTouchAction.WheelChanged events are fired"
      ],
      "environmentDetails": "iOS/iPadOS 13.4+, Mac Catalyst, MAUI with SkiaSharp.Views.Maui",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3533",
          "description": "Parent issue: Standardize WheelDelta to v120 convention across all platforms"
        }
      ],
      "relatedIssues": [
        3533,
        3534,
        3535,
        3536,
        3538,
        3539,
        3540
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "The Apple SKTouchHandler has no scroll/wheel gesture recognizer and the feature has never been implemented."
    }
  },
  "analysis": {
    "summary": "Scroll wheel support is entirely absent from the Apple (iOS/iPadOS/Mac Catalyst) SKTouchHandler in SkiaSharp.Views.Maui. The handler only processes UITouch events (TouchesBegan, TouchesMoved, TouchesEnded, TouchesCancelled) and has no UIPanGestureRecognizer for scroll input. This is a sub-issue of #3533 which aims to standardize WheelDelta to the v120 convention across all platforms. Implementation requires adding a UIPanGestureRecognizer with allowedScrollTypesMask = [.discrete, .continuous] and normalizing translation deltas to v120 units.",
    "rationale": "Classified as type/enhancement because scroll wheel support has never existed on this platform — this is entirely new functionality, not a regression or broken behavior. Area is area/SkiaSharp.Views.Maui because the change is entirely within the MAUI views layer (Apple platform handler). The author (a maintainer) filed this as a well-scoped sub-task with clear implementation guidance. Platforms are os/iOS (covers iPadOS) and os/macOS (Mac Catalyst). Partner is partner/maui since this is exclusively a MAUI views concern.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs",
        "lines": "1-112",
        "finding": "The Apple SKTouchHandler extends UIGestureRecognizer and handles only UITouch events via TouchesBegan, TouchesMoved, TouchesEnded, TouchesCancelled. There is no UIPanGestureRecognizer, no allowedScrollTypesMask configuration, and no scroll/wheel delta handling. SKTouchAction.WheelChanged is never fired from this handler.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs",
        "lines": "15-21",
        "finding": "Constructor accepts onTouchAction callback and scalePixels converter — both would be reused by a scroll gesture handler. No scroll-specific infrastructure exists.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Does NOT have any scroll/wheel gesture recognizer",
        "source": "issue body — Current State section",
        "interpretation": "Confirms the feature is entirely missing, not regressed."
      },
      {
        "text": "allowedScrollTypesMask is a filter that configures which scroll types the recognizer accepts — it is NOT a per-event classifier",
        "source": "issue body — Scroll Type Distinction section",
        "interpretation": "Implementation caveat: cannot distinguish discrete vs continuous per-event, normalization must work for both."
      },
      {
        "text": "The PointsPerNotch value is an initial estimate. It has been reported as anywhere from 22 to 60 depending on the mouse and iOS version.",
        "source": "issue body — Normalization Logic section",
        "interpretation": "Empirical calibration on real hardware is required before finalizing the implementation."
      },
      {
        "text": "Minimum iOS version: 13.4 (when allowedScrollTypesMask was introduced)",
        "source": "issue body — Implementation Notes",
        "interpretation": "Need runtime availability check or compile-time guard for the API."
      }
    ],
    "nextQuestions": [
      "What is the correct PointsPerNotch calibration constant on real iOS/iPadOS hardware with a Bluetooth mouse?",
      "Should Mac Catalyst use UIPanGestureRecognizer or NSEvent bridging for more precise control?",
      "Should the scroll recognizer be implemented as a separate gesture recognizer or integrated into the existing SKTouchHandler class?",
      "How should the iOS 13.4 availability guard be implemented — conditional compilation or runtime check?"
    ],
    "resolution": {
      "hypothesis": "Add a UIPanGestureRecognizer with allowedScrollTypesMask=[discrete,continuous] to SKTouchHandler, fire SKTouchAction.WheelChanged with normalized v120 delta, guarded behind iOS 13.4+ availability.",
      "proposals": [
        {
          "title": "Add scroll UIPanGestureRecognizer to SKTouchHandler",
          "description": "Add a secondary UIPanGestureRecognizer with allowedScrollTypesMask and allowedTouchTypes=[] to SKTouchHandler. On gesture changed, compute incremental translation delta, negate Y, multiply by 120/PointsPerNotch, and fire SKTouchAction.WheelChanged with SKTouchDeviceType.Mouse.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Use NSEvent bridging on Mac Catalyst",
          "description": "On Mac Catalyst, directly handle NSEvent scroll wheel events via AppKit bridging for more precise per-event discrete/continuous classification. Requires platform-specific code path.",
          "category": "alternative",
          "confidence": 0.6,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add scroll UIPanGestureRecognizer to SKTouchHandler",
      "recommendedReason": "The UIPanGestureRecognizer approach works for both iOS and Mac Catalyst with a single code path, aligns with Apple's recommended approach, and reuses existing handler infrastructure."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Well-specified enhancement from a maintainer with clear implementation path, but requires empirical calibration on real hardware to determine PointsPerNotch constant before implementation can be finalized.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct area label from area/SkiaSharp to area/SkiaSharp.Views.Maui, add os/macOS for Mac Catalyst, tenet/compatibility, partner/maui",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp.Views.Maui",
          "os/iOS",
          "os/macOS",
          "tenet/compatibility",
          "partner/maui"
        ]
      },
      {
        "type": "link-related",
        "description": "Link to parent issue #3533 (WheelDelta v120 standardization)",
        "risk": "low",
        "confidence": 0.99,
        "linkedIssue": 3533
      }
    ]
  }
}
```

</details>
