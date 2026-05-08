# Issue Triage Report — #1048

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T12:54:02Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views.Maui (0.90 (90%)) |
| Suggested action | ready-to-fix (0.82 (82%)) |

**Issue Summary:** On iOS, SKTouchAction.Cancelled (and Released) is never fired when a SKCanvasView (with EnableTouchEvents=true) is inside a ScrollView and the user scrolls, because iOS sends touchesCancelled to the underlying UIView rather than through the UIGestureRecognizer that SKTouchHandler extends.

**Analysis:** The iOS SKTouchHandler extends UIGestureRecognizer and implements TouchesCancelled, but iOS delivers touchesCancelled to the UIView's native touch responder rather than through the gesture recognizer when a parent UIScrollView intercepts touches. The gesture recognizer's Reset() is called instead, but SKTouchHandler does not override Reset() to send a Cancelled event for tracked touches.

**Recommendations:** **ready-to-fix** — Root cause is clearly identified, community has a working and production-tested fix, maintainer acknowledged the issue. The fix is small and contained to Apple/SKTouchHandler.cs.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/iOS |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, os/iOS, area/SkiaSharp.Views.Maui, triage/triaged |

## Evidence

### Reproduction

1. Create a Xamarin.Forms/MAUI app with a ScrollView containing a SKCanvasView
2. Set EnableTouchEvents = true on the SKCanvasView
3. Subscribe to the Touch event
4. Start touching the SKCanvasView, then scroll the ScrollView while still touching
5. Observe that SKTouchAction.Cancelled (and Released) is never fired on iOS

**Environment:** SkiaSharp 1.68.1, Xamarin.Forms, iOS. Works correctly on Android.

**Repository links:**
- https://github.com/mono/SkiaSharp/files/3916183/TestSkiaSharp.zip — Attached sample project from reporter
- https://gist.github.com/daltonks/f01cf125b580ca8b7b9213413f9299a5 — Community workaround: override UIGestureRecognizer.Reset to manually fire Cancelled events
- https://github.com/daltonks/FixedSKTouchHandler — Community repo demonstrating the fix: custom SKTouchHandler with touch tracking and Reset override

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | missing-output |
| Error message | SKTouchAction.Cancelled not invoked on iOS when parent ScrollView scrolls |
| Repro quality | complete |
| Target frameworks | Xamarin.Forms |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The Apple SKTouchHandler in source/SkiaSharp.Views.Maui still extends UIGestureRecognizer without overriding Reset() or tracking active touches — the root cause is unchanged. |

## Analysis

### Technical Summary

The iOS SKTouchHandler extends UIGestureRecognizer and implements TouchesCancelled, but iOS delivers touchesCancelled to the UIView's native touch responder rather than through the gesture recognizer when a parent UIScrollView intercepts touches. The gesture recognizer's Reset() is called instead, but SKTouchHandler does not override Reset() to send a Cancelled event for tracked touches.

### Rationale

The bug is confirmed by code inspection: SKTouchHandler on Apple extends UIGestureRecognizer and has a TouchesCancelled override, but iOS scroll view gesture conflict resolution calls Reset() on the gesture recognizer, not TouchesCancelled. This is a well-understood iOS UIKit behavior difference. The community has documented a working fix (track active touches, fire Cancelled from Reset()). The maintainer acknowledged the issue and expressed intent to fix it.

### Key Signals

- "the underlying scrollview, as soon as the scroll begins, invokes the method 'touchesCancelled' of the view and not the GestureRecognizer's" — **issue comment by reporter** (Root cause identified: iOS bypasses UIGestureRecognizer.TouchesCancelled in this scroll conflict scenario)
- "I found a way that seems to work! If you keep track of the touches with HashSet<UITouch> _touches, you can manually cancel the ones in the gesture recognizer's Reset method." — **issue comment by contributor daltonks** (Community-validated workaround: override UIGestureRecognizer.Reset() and emit Cancelled for tracked touches)
- "I've been using this successfully in production for a bit over 3 months now." — **issue comment by contributor daltonks (Oct 2020)** (The Reset() workaround is production-tested and stable)
- "I'll try to get this into the next release." — **issue comment by maintainer mattleibow** (Maintainer acknowledged the issue and intended to fix it; not yet addressed in the current codebase)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs` | 10-111 | direct | SKTouchHandler extends UIGestureRecognizer and overrides TouchesBegan, TouchesMoved, TouchesEnded, TouchesCancelled — but does NOT override Reset(). When iOS cancels a gesture recognizer (e.g., when a parent ScrollView takes over), Reset() is called instead of TouchesCancelled, so no SKTouchAction.Cancelled event is fired. Active touches are not tracked. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs` | 100-108 | related | Android SKTouchHandler handles MotionEventActions.Cancel explicitly by dispatching SKTouchAction.Cancelled. This works correctly because Android routes cancel events through the standard View.Touch event handler. |

### Workarounds

- Override UIGestureRecognizer.Reset() in a custom SKTouchHandler subclass, tracking active UITouch objects in a HashSet, and manually fire SKTouchAction.Cancelled for all in-flight touches when Reset() is called. See: https://github.com/daltonks/FixedSKTouchHandler

### Next Questions

- Does the same issue affect macOS/Mac Catalyst (which also uses the Apple SKTouchHandler)?
- Does the same issue affect SKGLView on iOS?
- Are there edge cases where iOS gestures (app minimize, system gestures) also bypass TouchesCancelled?

### Resolution Proposals

**Hypothesis:** Override Reset() in the Apple SKTouchHandler to iterate all tracked UITouch objects and fire SKTouchAction.Cancelled before calling base.Reset(). Track touches in a HashSet<UITouch> — add in TouchesBegan, remove in TouchesEnded/TouchesCancelled/Reset.

1. **Override Reset() in SKTouchHandler with touch tracking** — fix, confidence 0.88 (88%), cost/s, validated=untested
   - Track active UITouch objects in a HashSet. In TouchesBegan add touches; in TouchesEnded/TouchesCancelled remove them; override Reset() to fire Cancelled for all remaining tracked touches before clearing the set and calling base.Reset().
2. **Handle touchesCancelled at UIView level** — investigation, confidence 0.65 (65%), cost/m, validated=untested
   - Subclass the native platform view and override TouchesCancelled at the UIView level to propagate the cancel event directly. More invasive but covers additional iOS scenarios.

**Recommended proposal:** Override Reset() in SKTouchHandler with touch tracking

**Why:** Community-tested in production for 3+ months. Minimal footprint — only touches Apple SKTouchHandler, matches the pattern iOS UIKit expects for gesture recognizer cancellation.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.82 (82%) |
| Reason | Root cause is clearly identified, community has a working and production-tested fix, maintainer acknowledged the issue. The fix is small and contained to Apple/SKTouchHandler.cs. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, area, platform, tenet labels | labels=type/bug, area/SkiaSharp.Views.Maui, os/iOS, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Summarize root cause and point to community fix | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and the community investigation here!

**Root cause:** The iOS `SKTouchHandler` extends `UIGestureRecognizer` and implements `TouchesCancelled`, but when a parent `UIScrollView` intercepts touches it calls `Reset()` on the gesture recognizer — not `TouchesCancelled`. Since `SKTouchHandler` doesn't override `Reset()`, the cancel event is never forwarded to C#.

**Workaround (community-tested):** Track active `UITouch` objects and fire `SKTouchAction.Cancelled` from `UIGestureRecognizer.Reset()`. A working example is at: https://github.com/daltonks/FixedSKTouchHandler

**Fix path:** Override `Reset()` in `SKTouchHandler` (Apple) to iterate tracked touches and emit `Cancelled` before clearing and calling `base.Reset()`. This has been used in production for 3+ months.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1048,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T12:54:02Z",
    "currentLabels": [
      "type/bug",
      "os/iOS",
      "area/SkiaSharp.Views.Maui",
      "triage/triaged"
    ]
  },
  "summary": "On iOS, SKTouchAction.Cancelled (and Released) is never fired when a SKCanvasView (with EnableTouchEvents=true) is inside a ScrollView and the user scrolls, because iOS sends touchesCancelled to the underlying UIView rather than through the UIGestureRecognizer that SKTouchHandler extends.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.9
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
      "errorType": "missing-output",
      "errorMessage": "SKTouchAction.Cancelled not invoked on iOS when parent ScrollView scrolls",
      "reproQuality": "complete",
      "targetFrameworks": [
        "Xamarin.Forms"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Xamarin.Forms/MAUI app with a ScrollView containing a SKCanvasView",
        "Set EnableTouchEvents = true on the SKCanvasView",
        "Subscribe to the Touch event",
        "Start touching the SKCanvasView, then scroll the ScrollView while still touching",
        "Observe that SKTouchAction.Cancelled (and Released) is never fired on iOS"
      ],
      "environmentDetails": "SkiaSharp 1.68.1, Xamarin.Forms, iOS. Works correctly on Android.",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/3916183/TestSkiaSharp.zip",
          "description": "Attached sample project from reporter"
        },
        {
          "url": "https://gist.github.com/daltonks/f01cf125b580ca8b7b9213413f9299a5",
          "description": "Community workaround: override UIGestureRecognizer.Reset to manually fire Cancelled events"
        },
        {
          "url": "https://github.com/daltonks/FixedSKTouchHandler",
          "description": "Community repo demonstrating the fix: custom SKTouchHandler with touch tracking and Reset override"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The Apple SKTouchHandler in source/SkiaSharp.Views.Maui still extends UIGestureRecognizer without overriding Reset() or tracking active touches — the root cause is unchanged."
    }
  },
  "analysis": {
    "summary": "The iOS SKTouchHandler extends UIGestureRecognizer and implements TouchesCancelled, but iOS delivers touchesCancelled to the UIView's native touch responder rather than through the gesture recognizer when a parent UIScrollView intercepts touches. The gesture recognizer's Reset() is called instead, but SKTouchHandler does not override Reset() to send a Cancelled event for tracked touches.",
    "rationale": "The bug is confirmed by code inspection: SKTouchHandler on Apple extends UIGestureRecognizer and has a TouchesCancelled override, but iOS scroll view gesture conflict resolution calls Reset() on the gesture recognizer, not TouchesCancelled. This is a well-understood iOS UIKit behavior difference. The community has documented a working fix (track active touches, fire Cancelled from Reset()). The maintainer acknowledged the issue and expressed intent to fix it.",
    "keySignals": [
      {
        "text": "the underlying scrollview, as soon as the scroll begins, invokes the method 'touchesCancelled' of the view and not the GestureRecognizer's",
        "source": "issue comment by reporter",
        "interpretation": "Root cause identified: iOS bypasses UIGestureRecognizer.TouchesCancelled in this scroll conflict scenario"
      },
      {
        "text": "I found a way that seems to work! If you keep track of the touches with HashSet<UITouch> _touches, you can manually cancel the ones in the gesture recognizer's Reset method.",
        "source": "issue comment by contributor daltonks",
        "interpretation": "Community-validated workaround: override UIGestureRecognizer.Reset() and emit Cancelled for tracked touches"
      },
      {
        "text": "I've been using this successfully in production for a bit over 3 months now.",
        "source": "issue comment by contributor daltonks (Oct 2020)",
        "interpretation": "The Reset() workaround is production-tested and stable"
      },
      {
        "text": "I'll try to get this into the next release.",
        "source": "issue comment by maintainer mattleibow",
        "interpretation": "Maintainer acknowledged the issue and intended to fix it; not yet addressed in the current codebase"
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs",
        "lines": "10-111",
        "finding": "SKTouchHandler extends UIGestureRecognizer and overrides TouchesBegan, TouchesMoved, TouchesEnded, TouchesCancelled — but does NOT override Reset(). When iOS cancels a gesture recognizer (e.g., when a parent ScrollView takes over), Reset() is called instead of TouchesCancelled, so no SKTouchAction.Cancelled event is fired. Active touches are not tracked.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs",
        "lines": "100-108",
        "finding": "Android SKTouchHandler handles MotionEventActions.Cancel explicitly by dispatching SKTouchAction.Cancelled. This works correctly because Android routes cancel events through the standard View.Touch event handler.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Override UIGestureRecognizer.Reset() in a custom SKTouchHandler subclass, tracking active UITouch objects in a HashSet, and manually fire SKTouchAction.Cancelled for all in-flight touches when Reset() is called. See: https://github.com/daltonks/FixedSKTouchHandler"
    ],
    "nextQuestions": [
      "Does the same issue affect macOS/Mac Catalyst (which also uses the Apple SKTouchHandler)?",
      "Does the same issue affect SKGLView on iOS?",
      "Are there edge cases where iOS gestures (app minimize, system gestures) also bypass TouchesCancelled?"
    ],
    "resolution": {
      "hypothesis": "Override Reset() in the Apple SKTouchHandler to iterate all tracked UITouch objects and fire SKTouchAction.Cancelled before calling base.Reset(). Track touches in a HashSet<UITouch> — add in TouchesBegan, remove in TouchesEnded/TouchesCancelled/Reset.",
      "proposals": [
        {
          "title": "Override Reset() in SKTouchHandler with touch tracking",
          "description": "Track active UITouch objects in a HashSet. In TouchesBegan add touches; in TouchesEnded/TouchesCancelled remove them; override Reset() to fire Cancelled for all remaining tracked touches before clearing the set and calling base.Reset().",
          "confidence": 0.88,
          "effort": "cost/s",
          "category": "fix",
          "validated": "untested"
        },
        {
          "title": "Handle touchesCancelled at UIView level",
          "description": "Subclass the native platform view and override TouchesCancelled at the UIView level to propagate the cancel event directly. More invasive but covers additional iOS scenarios.",
          "confidence": 0.65,
          "effort": "cost/m",
          "category": "investigation",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Override Reset() in SKTouchHandler with touch tracking",
      "recommendedReason": "Community-tested in production for 3+ months. Minimal footprint — only touches Apple SKTouchHandler, matches the pattern iOS UIKit expects for gesture recognizer cancellation."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.82,
      "reason": "Root cause is clearly identified, community has a working and production-tested fix, maintainer acknowledged the issue. The fix is small and contained to Apple/SKTouchHandler.cs.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, area, platform, tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Maui",
          "os/iOS",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Summarize root cause and point to community fix",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed report and the community investigation here!\n\n**Root cause:** The iOS `SKTouchHandler` extends `UIGestureRecognizer` and implements `TouchesCancelled`, but when a parent `UIScrollView` intercepts touches it calls `Reset()` on the gesture recognizer — not `TouchesCancelled`. Since `SKTouchHandler` doesn't override `Reset()`, the cancel event is never forwarded to C#.\n\n**Workaround (community-tested):** Track active `UITouch` objects and fire `SKTouchAction.Cancelled` from `UIGestureRecognizer.Reset()`. A working example is at: https://github.com/daltonks/FixedSKTouchHandler\n\n**Fix path:** Override `Reset()` in `SKTouchHandler` (Apple) to iterate tracked touches and emit `Cancelled` before clearing and calling `base.Reset()`. This has been used in production for 3+ months."
      }
    ]
  }
}
```

</details>
