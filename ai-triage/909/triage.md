# Issue Triage Report — #909

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T07:45:00Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp.Views.Forms (0.85 (85%)) |
| Suggested action | needs-investigation (0.80 (80%)) |

**Issue Summary:** On iOS, touch events on SKCanvasView are interrupted after a few pixels when a parent Xamarin.Forms container has Pan and/or Pinch gesture recognizers attached, causing drawing to stop mid-gesture.

**Analysis:** iOS UIKit's gesture recognizer system cancels the SKTouchHandler gesture recognizer when a competing pan or pinch gesture recognizer on a parent view takes precedence. Because SKTouchHandler extends UIGestureRecognizer but does not implement ShouldRecognizeSimultaneouslyWithGestureRecognizer, iOS will cancel SKTouchHandler's touches once the parent's gesture recognizer activates, causing the touch sequence to be cut short.

**Recommendations:** **needs-investigation** — Bug is well-described with repro repo, but filed against deprecated Xamarin.Forms (v1.68). Needs verification against current MAUI SkiaSharp to confirm if it is still reproducible and if the proposed ShouldRecognizeSimultaneously fix addresses it.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Forms |
| Platforms | os/iOS |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create a Xamarin.Forms app with an SKCanvasView (with EnableTouchEvents=true) inside a container (e.g., AbsoluteLayout) that has PanGestureRecognizer and/or PinchGestureRecognizer attached
2. Run on iOS device or simulator
3. Attempt to draw with a single finger — observe drawing stops after a few pixels
4. If PinchGestureRecognizer is attached, attempt two-finger drawing — observe same interruption

**Environment:** SkiaSharp 1.68, Xamarin.Forms, iOS only (Android not affected)

**Related issues:** #1048

**Repository links:**
- https://github.com/codemode18/SkiaSharpPanPinchBugRepro — Sample application demonstrating the bug (finger paint app)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | Xamarin.Forms |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The iOS SKTouchHandler still uses UIGestureRecognizer without ShouldRecognizeSimultaneouslyWithGestureRecognizer, so the same conflict exists in current MAUI code. |

## Analysis

### Technical Summary

iOS UIKit's gesture recognizer system cancels the SKTouchHandler gesture recognizer when a competing pan or pinch gesture recognizer on a parent view takes precedence. Because SKTouchHandler extends UIGestureRecognizer but does not implement ShouldRecognizeSimultaneouslyWithGestureRecognizer, iOS will cancel SKTouchHandler's touches once the parent's gesture recognizer activates, causing the touch sequence to be cut short.

### Rationale

The bug is clearly described with a repro link. iOS gesture recognizer competition is a well-known platform behavior. The SKTouchHandler (UIGestureRecognizer subclass) lacks ShouldRecognizeSimultaneouslyWithGestureRecognizer, which is why parent gesture recognizers cancel it. Related issue #1048 (ScrollView on iOS) confirms this same pattern is known. While this was filed against Xamarin.Forms, the MAUI equivalent code has the same architecture.

### Key Signals

- "On iOS only, after a few pixels, the line stops drawing" — **issue body** (TouchesMoved stops being called because iOS gesture recognizer cancels the touch sequence when parent gesture recognizer activates)
- "If a Pan Gesture Recognizer is attached to the container, single finger drawing is incorrect" — **issue body** (iOS pan gesture recognizer on parent container takes ownership of pan gesture, cancelling child SKTouchHandler)
- "If a Pinch Gesture is attached, 2-finger drawing is incorrect" — **issue body** (Pinch gesture recognizer competes with multi-touch on SKCanvasView)
- "I believe this issue is on Android as well" — **comment by exendahal, 2022-11-08** (Possibly broader platform impact, but not confirmed by original reporter)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs` | 10-111 | direct | SKTouchHandler extends UIGestureRecognizer and handles TouchesBegan/Moved/Ended/Cancelled. It does NOT override ShouldRecognizeSimultaneouslyWithGestureRecognizer or implement UIGestureRecognizerDelegate, so it will conflict with parent container gesture recognizers (Pan, Pinch) — the system will cancel one when the other activates. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandlerProxy.cs` | 1-55 | related | SKTouchHandlerProxy wires up SKTouchHandler as a UIGestureRecognizer on the platform view. There is no gesture recognizer coordination logic (no ShouldRecognizeSimultaneously, no RequireGestureRecognizerToFail). |

### Workarounds

- Remove Pan/Pinch gesture recognizers from parent container and implement equivalent behavior using SKCanvasView touch events directly
- Set DelaysTouchesBegan = false and DelaysTouchesEnded = false on the SKTouchHandler instance (requires custom renderer)
- Implement a custom iOS renderer that configures UIGestureRecognizerDelegate to allow simultaneous recognition

### Next Questions

- Is this still reproducible in SkiaSharp 2.x or 3.x with MAUI (Xamarin.Forms is now deprecated)?
- Can Android also reproduce this (a commenter suggested it might)?
- Would implementing ShouldRecognizeSimultaneouslyWithGestureRecognizer in SKTouchHandler fix the issue?

### Resolution Proposals

**Hypothesis:** Adding ShouldRecognizeSimultaneouslyWithGestureRecognizer returning true in SKTouchHandler would allow iOS to deliver touches to both the SKTouchHandler and the parent gesture recognizers simultaneously.

1. **Implement ShouldRecognizeSimultaneouslyWithGestureRecognizer** — fix, confidence 0.80 (80%), cost/s, validated=untested
   - Override ShouldRecognizeSimultaneouslyWithGestureRecognizer in SKTouchHandler to return true for all other gesture recognizers, allowing SKTouchHandler to coexist with parent container Pan/Pinch gesture recognizers.
2. **Use raw touch handling instead of UIGestureRecognizer** — alternative, confidence 0.70 (70%), cost/m, validated=untested
   - Instead of subclassing UIGestureRecognizer, override TouchesBegan/Moved/Ended/Cancelled directly on the UIView, bypassing gesture recognizer competition entirely.

**Recommended proposal:** Implement ShouldRecognizeSimultaneouslyWithGestureRecognizer

**Why:** Minimal change that directly solves the gesture recognizer conflict. The pattern is standard iOS practice for touch-heavy views that must coexist with gesture recognizers.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.80 (80%) |
| Reason | Bug is well-described with repro repo, but filed against deprecated Xamarin.Forms (v1.68). Needs verification against current MAUI SkiaSharp to confirm if it is still reproducible and if the proposed ShouldRecognizeSimultaneously fix addresses it. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, iOS, Views.Forms, reliability labels | labels=type/bug, area/SkiaSharp.Views.Forms, os/iOS, tenet/reliability |
| link-related | low | 0.85 (85%) | Link to related issue #1048 (same iOS gesture recognizer conflict with ScrollView) | linkedIssue=#1048 |
| add-comment | medium | 0.80 (80%) | Request verification against current SkiaSharp/MAUI and provide workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report and repro project!

This looks like a known iOS UIKit behavior: when `SKTouchHandler` (a `UIGestureRecognizer`) is added to a view that is inside a container with `PanGestureRecognizer` or `PinchGestureRecognizer`, iOS's gesture recognizer system will cancel one of them once the other activates. Since `SKTouchHandler` doesn't implement `ShouldRecognizeSimultaneouslyWithGestureRecognizer`, the parent container's gesture recognizers win and touch delivery to the SkiaSharp view is cut short.

**Workaround (while a fix is being investigated):**
Remove the Xamarin.Forms `PanGestureRecognizer`/`PinchGestureRecognizer` from the parent container and implement equivalent pan/pinch logic directly inside your `SKCanvasView` touch handler using the provided `SKTouchEventArgs` (track `id`, compute delta between `Pressed`→`Moved` points for pan; track two-finger distance for pinch).

**Note:** Xamarin.Forms is now end-of-life and replaced by .NET MAUI. Could you verify if this issue also reproduces with the current SkiaSharp 3.x and .NET MAUI? The same underlying architecture exists in MAUI (see also related issue #1048).
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 909,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T07:45:00Z"
  },
  "summary": "On iOS, touch events on SKCanvasView are interrupted after a few pixels when a parent Xamarin.Forms container has Pan and/or Pinch gesture recognizers attached, causing drawing to stop mid-gesture.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp.Views.Forms",
      "confidence": 0.85
    },
    "platforms": [
      "os/iOS"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "reproQuality": "partial",
      "targetFrameworks": [
        "Xamarin.Forms"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Xamarin.Forms app with an SKCanvasView (with EnableTouchEvents=true) inside a container (e.g., AbsoluteLayout) that has PanGestureRecognizer and/or PinchGestureRecognizer attached",
        "Run on iOS device or simulator",
        "Attempt to draw with a single finger — observe drawing stops after a few pixels",
        "If PinchGestureRecognizer is attached, attempt two-finger drawing — observe same interruption"
      ],
      "environmentDetails": "SkiaSharp 1.68, Xamarin.Forms, iOS only (Android not affected)",
      "repoLinks": [
        {
          "url": "https://github.com/codemode18/SkiaSharpPanPinchBugRepro",
          "description": "Sample application demonstrating the bug (finger paint app)"
        }
      ],
      "relatedIssues": [
        1048
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The iOS SKTouchHandler still uses UIGestureRecognizer without ShouldRecognizeSimultaneouslyWithGestureRecognizer, so the same conflict exists in current MAUI code."
    }
  },
  "analysis": {
    "summary": "iOS UIKit's gesture recognizer system cancels the SKTouchHandler gesture recognizer when a competing pan or pinch gesture recognizer on a parent view takes precedence. Because SKTouchHandler extends UIGestureRecognizer but does not implement ShouldRecognizeSimultaneouslyWithGestureRecognizer, iOS will cancel SKTouchHandler's touches once the parent's gesture recognizer activates, causing the touch sequence to be cut short.",
    "rationale": "The bug is clearly described with a repro link. iOS gesture recognizer competition is a well-known platform behavior. The SKTouchHandler (UIGestureRecognizer subclass) lacks ShouldRecognizeSimultaneouslyWithGestureRecognizer, which is why parent gesture recognizers cancel it. Related issue #1048 (ScrollView on iOS) confirms this same pattern is known. While this was filed against Xamarin.Forms, the MAUI equivalent code has the same architecture.",
    "keySignals": [
      {
        "text": "On iOS only, after a few pixels, the line stops drawing",
        "source": "issue body",
        "interpretation": "TouchesMoved stops being called because iOS gesture recognizer cancels the touch sequence when parent gesture recognizer activates"
      },
      {
        "text": "If a Pan Gesture Recognizer is attached to the container, single finger drawing is incorrect",
        "source": "issue body",
        "interpretation": "iOS pan gesture recognizer on parent container takes ownership of pan gesture, cancelling child SKTouchHandler"
      },
      {
        "text": "If a Pinch Gesture is attached, 2-finger drawing is incorrect",
        "source": "issue body",
        "interpretation": "Pinch gesture recognizer competes with multi-touch on SKCanvasView"
      },
      {
        "text": "I believe this issue is on Android as well",
        "source": "comment by exendahal, 2022-11-08",
        "interpretation": "Possibly broader platform impact, but not confirmed by original reporter"
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs",
        "lines": "10-111",
        "finding": "SKTouchHandler extends UIGestureRecognizer and handles TouchesBegan/Moved/Ended/Cancelled. It does NOT override ShouldRecognizeSimultaneouslyWithGestureRecognizer or implement UIGestureRecognizerDelegate, so it will conflict with parent container gesture recognizers (Pan, Pinch) — the system will cancel one when the other activates.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandlerProxy.cs",
        "lines": "1-55",
        "finding": "SKTouchHandlerProxy wires up SKTouchHandler as a UIGestureRecognizer on the platform view. There is no gesture recognizer coordination logic (no ShouldRecognizeSimultaneously, no RequireGestureRecognizerToFail).",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Remove Pan/Pinch gesture recognizers from parent container and implement equivalent behavior using SKCanvasView touch events directly",
      "Set DelaysTouchesBegan = false and DelaysTouchesEnded = false on the SKTouchHandler instance (requires custom renderer)",
      "Implement a custom iOS renderer that configures UIGestureRecognizerDelegate to allow simultaneous recognition"
    ],
    "nextQuestions": [
      "Is this still reproducible in SkiaSharp 2.x or 3.x with MAUI (Xamarin.Forms is now deprecated)?",
      "Can Android also reproduce this (a commenter suggested it might)?",
      "Would implementing ShouldRecognizeSimultaneouslyWithGestureRecognizer in SKTouchHandler fix the issue?"
    ],
    "resolution": {
      "hypothesis": "Adding ShouldRecognizeSimultaneouslyWithGestureRecognizer returning true in SKTouchHandler would allow iOS to deliver touches to both the SKTouchHandler and the parent gesture recognizers simultaneously.",
      "proposals": [
        {
          "title": "Implement ShouldRecognizeSimultaneouslyWithGestureRecognizer",
          "description": "Override ShouldRecognizeSimultaneouslyWithGestureRecognizer in SKTouchHandler to return true for all other gesture recognizers, allowing SKTouchHandler to coexist with parent container Pan/Pinch gesture recognizers.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Use raw touch handling instead of UIGestureRecognizer",
          "description": "Instead of subclassing UIGestureRecognizer, override TouchesBegan/Moved/Ended/Cancelled directly on the UIView, bypassing gesture recognizer competition entirely.",
          "category": "alternative",
          "confidence": 0.7,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Implement ShouldRecognizeSimultaneouslyWithGestureRecognizer",
      "recommendedReason": "Minimal change that directly solves the gesture recognizer conflict. The pattern is standard iOS practice for touch-heavy views that must coexist with gesture recognizers."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.8,
      "reason": "Bug is well-described with repro repo, but filed against deprecated Xamarin.Forms (v1.68). Needs verification against current MAUI SkiaSharp to confirm if it is still reproducible and if the proposed ShouldRecognizeSimultaneously fix addresses it.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, iOS, Views.Forms, reliability labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Forms",
          "os/iOS",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-related",
        "description": "Link to related issue #1048 (same iOS gesture recognizer conflict with ScrollView)",
        "risk": "low",
        "confidence": 0.85,
        "linkedIssue": 1048
      },
      {
        "type": "add-comment",
        "description": "Request verification against current SkiaSharp/MAUI and provide workaround",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thank you for the detailed report and repro project!\n\nThis looks like a known iOS UIKit behavior: when `SKTouchHandler` (a `UIGestureRecognizer`) is added to a view that is inside a container with `PanGestureRecognizer` or `PinchGestureRecognizer`, iOS's gesture recognizer system will cancel one of them once the other activates. Since `SKTouchHandler` doesn't implement `ShouldRecognizeSimultaneouslyWithGestureRecognizer`, the parent container's gesture recognizers win and touch delivery to the SkiaSharp view is cut short.\n\n**Workaround (while a fix is being investigated):**\nRemove the Xamarin.Forms `PanGestureRecognizer`/`PinchGestureRecognizer` from the parent container and implement equivalent pan/pinch logic directly inside your `SKCanvasView` touch handler using the provided `SKTouchEventArgs` (track `id`, compute delta between `Pressed`→`Moved` points for pan; track two-finger distance for pinch).\n\n**Note:** Xamarin.Forms is now end-of-life and replaced by .NET MAUI. Could you verify if this issue also reproduces with the current SkiaSharp 3.x and .NET MAUI? The same underlying architecture exists in MAUI (see also related issue #1048)."
      }
    ]
  }
}
```

</details>
