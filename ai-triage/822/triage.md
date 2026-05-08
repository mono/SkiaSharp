# Issue Triage Report — #822

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T18:03:04Z |
| Type | type/feature-request (0.88 (88%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** Reporter asks how to implement fling/velocity gesture detection using touch events in SKGLView on iOS and Android, and requests that higher-level gesture events (single tap, double tap, long tap, fling with velocity) be added to SKGLView and SKCanvasView.

**Analysis:** The current SKTouchEventArgs API provides only primitive touch data (ActionType, Location, InContact, Pressure) — no velocity, no gesture recognition. Implementing fling requires tracking touch history to compute velocity. On iOS the SKTouchHandler is itself a UIGestureRecognizer, which conflicts with adding additional gesture recognizers to the native view. The maintainer pointed to SkiaSharp.Extended PR #79 as a potential implementation path, suggesting gesture support might live in an extension package rather than core.

**Recommendations:** **keep-open** — Valid feature request with community interest. Maintainer has explored gesture support in SkiaSharp.Extended. The feature has not been implemented in core SkiaSharp.Views. Keep open to track interest and design decision.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp.Views |
| Platforms | os/iOS, os/Android |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | type/enhancement, type/question, type/feature-request |

## Evidence

### Reproduction

1. Use SKGLView in Xamarin.Forms on iOS or Android
2. Try to detect a fling (velocity-based swipe) gesture
3. On Android: workaround is to re-convert touch events into native MotionEvents and use platform GestureDetector (complex)
4. On iOS: adding a UIGestureRecognizer to the native view overrides the touches within SKGLView — no workaround found

**Environment:** SkiaSharp.Views.Forms, Xamarin.Forms, iOS and Android

**Repository links:**
- https://github.com/mono/SkiaSharp.Extended/pull/79/files#diff-9fc0fb5f255e13c0c4d03a22334523f9 — SkiaSharp.Extended PR #79 — gesture recognizer implementations for fling, scale, rotate (referenced by maintainer mattleibow)

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SKTouchEventArgs still does not expose velocity or gesture-derived data in the current codebase. The feature request remains valid. |

## Analysis

### Technical Summary

The current SKTouchEventArgs API provides only primitive touch data (ActionType, Location, InContact, Pressure) — no velocity, no gesture recognition. Implementing fling requires tracking touch history to compute velocity. On iOS the SKTouchHandler is itself a UIGestureRecognizer, which conflicts with adding additional gesture recognizers to the native view. The maintainer pointed to SkiaSharp.Extended PR #79 as a potential implementation path, suggesting gesture support might live in an extension package rather than core.

### Rationale

This is a feature request to add higher-level gesture events (fling, tap, long-press with velocity) to SKGLView/SKCanvasView. The existing touch API only provides raw touch events with no velocity data. The iOS conflict with UIGestureRecognizer is a real platform constraint. Classified as feature-request because this adds entirely new functionality (gesture recognition layer) not currently in any form in the views package.

### Key Signals

- "When I add the gesture recognizer to the native view it overrides the touches within the SKGLView" — **issue body** (iOS SKTouchHandler is itself a UIGestureRecognizer — adding a second recognizer without proper coordination causes conflict.)
- "Maybe adding some gestures to the SKGLView and SKCanvasView would be enable more people to utilize the skia views in forms easily" — **issue body** (Core feature request: add gesture events to the views API.)
- "I previously started some ideas that did gestures such as fling, scale and rotate: https://github.com/mono/SkiaSharp.Extended/pull/79" — **comment by mattleibow (maintainer)** (Maintainer has explored this in SkiaSharp.Extended — suggests the feature may be intentionally kept in an extension package.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SKTouchEventArgs.cs` | 1-86 | direct | SKTouchEventArgs exposes Id, ActionType (Pressed/Moved/Released/Cancelled/WheelChanged), Location, InContact, WheelDelta, Pressure — no velocity or gesture-derived data exists anywhere in the current API. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs` | 10-111 | direct | iOS SKTouchHandler extends UIGestureRecognizer and fires only primitive Pressed/Moved/Released/Cancelled events. No velocity tracking. Being a UIGestureRecognizer itself is why adding additional gesture recognizers to the native view conflicts. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs` | 1-139 | direct | Android SKTouchHandler hooks View.Touch and processes MotionEventActions (Down/Move/Up/Cancel). The underlying MotionEvent has velocity data via VelocityTracker but it is never extracted or surfaced in SKTouchEventArgs. |

### Workarounds

- Track touch history manually: store (timestamp, location) on each Moved event, compute velocity on Released using the last N samples over a time window.
- On Android: use VelocityTracker directly in a custom renderer — the raw MotionEvent velocity is available but not exposed by SKTouchHandler.
- Refer to SkiaSharp.Extended PR #79 for a community gesture recognizer implementation that wraps the primitive touch events.

### Next Questions

- Is gesture support planned for SkiaSharp.Views.Maui or will it remain in SkiaSharp.Extended only?
- Should velocity be added to SKTouchEventArgs itself, or should a separate SKGestureEventArgs be introduced?
- Does the iOS UIGestureRecognizer conflict need to be resolved before gesture support can be added on that platform?

### Resolution Proposals

**Hypothesis:** Velocity tracking can be computed from existing touch events. For a first-class solution, velocity could be added to SKTouchEventArgs (Android via VelocityTracker, iOS via UITouch.estimatedProperties or manual tracking) or a new gesture event layer could be added above the raw touch API.

1. **Compute velocity from touch history (userland workaround)** — workaround, confidence 0.85 (85%), cost/s, validated=untested
   - Track (timestamp, location) pairs in the SKTouch handler. On Released, compute dx/dt and dy/dt over the last 100ms to derive fling velocity. No library changes needed.
2. **Add velocity fields to SKTouchEventArgs** — fix, confidence 0.70 (70%), cost/m, validated=untested
   - Extend SKTouchEventArgs with VelocityX and VelocityY properties. Populate from VelocityTracker on Android and manual tracking on iOS. This is an additive ABI-safe change.
3. **Use SkiaSharp.Extended gesture recognizers** — alternative, confidence 0.80 (80%), cost/s, validated=untested
   - Reference the gesture recognizer implementation in SkiaSharp.Extended PR #79 (fling, scale, rotate) as a standalone helper that wraps primitive SKTouchEventArgs events.

**Recommended proposal:** Compute velocity from touch history (userland workaround)

**Why:** No library changes needed, works today, and the maintainer's own reference (SkiaSharp.Extended) demonstrates this pattern is viable.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Valid feature request with community interest. Maintainer has explored gesture support in SkiaSharp.Extended. The feature has not been implemented in core SkiaSharp.Views. Keep open to track interest and design decision. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Correct labels: single type/feature-request, area/SkiaSharp.Views, platforms | labels=type/feature-request, area/SkiaSharp.Views, os/iOS, os/Android |
| add-comment | medium | 0.82 (82%) | Explain workaround and point to SkiaSharp.Extended implementation | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report! You can implement fling velocity today by tracking touch history manually:

```csharp
// Track points and times in your touch handler
var history = new List<(SKPoint pt, DateTime time)>();

void OnTouch(object sender, SKTouchEventArgs e)
{
    switch (e.ActionType)
    {
        case SKTouchAction.Pressed:
            history.Clear();
            history.Add((e.Location, DateTime.UtcNow));
            break;
        case SKTouchAction.Moved:
            history.Add((e.Location, DateTime.UtcNow));
            // Keep only last 100ms
            var cutoff = DateTime.UtcNow.AddMilliseconds(-100);
            history.RemoveAll(h => h.time < cutoff);
            break;
        case SKTouchAction.Released:
            if (history.Count >= 2)
            {
                var oldest = history[0];
                var newest = history[^1];
                var dt = (float)(newest.time - oldest.time).TotalSeconds;
                if (dt > 0)
                {
                    float vx = (newest.pt.X - oldest.pt.X) / dt;
                    float vy = (newest.pt.Y - oldest.pt.Y) / dt;
                    // vx, vy are pixels/second fling velocity
                }
            }
            break;
    }
    e.Handled = true;
}
```

For a more complete solution, @mattleibow previously started gesture recognizer implementations (fling, scale, rotate) in [SkiaSharp.Extended PR #79](https://github.com/mono/SkiaSharp.Extended/pull/79) — that may be worth tracking for a higher-level API.

Adding velocity directly to `SKTouchEventArgs` or a new gesture event layer is a valid enhancement for the library itself.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 822,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T18:03:04Z",
    "currentLabels": [
      "type/enhancement",
      "type/question",
      "type/feature-request"
    ]
  },
  "summary": "Reporter asks how to implement fling/velocity gesture detection using touch events in SKGLView on iOS and Android, and requests that higher-level gesture events (single tap, double tap, long tap, fling with velocity) be added to SKGLView and SKCanvasView.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.9
    },
    "platforms": [
      "os/iOS",
      "os/Android"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Use SKGLView in Xamarin.Forms on iOS or Android",
        "Try to detect a fling (velocity-based swipe) gesture",
        "On Android: workaround is to re-convert touch events into native MotionEvents and use platform GestureDetector (complex)",
        "On iOS: adding a UIGestureRecognizer to the native view overrides the touches within SKGLView — no workaround found"
      ],
      "environmentDetails": "SkiaSharp.Views.Forms, Xamarin.Forms, iOS and Android",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp.Extended/pull/79/files#diff-9fc0fb5f255e13c0c4d03a22334523f9",
          "description": "SkiaSharp.Extended PR #79 — gesture recognizer implementations for fling, scale, rotate (referenced by maintainer mattleibow)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "SKTouchEventArgs still does not expose velocity or gesture-derived data in the current codebase. The feature request remains valid."
    }
  },
  "analysis": {
    "summary": "The current SKTouchEventArgs API provides only primitive touch data (ActionType, Location, InContact, Pressure) — no velocity, no gesture recognition. Implementing fling requires tracking touch history to compute velocity. On iOS the SKTouchHandler is itself a UIGestureRecognizer, which conflicts with adding additional gesture recognizers to the native view. The maintainer pointed to SkiaSharp.Extended PR #79 as a potential implementation path, suggesting gesture support might live in an extension package rather than core.",
    "rationale": "This is a feature request to add higher-level gesture events (fling, tap, long-press with velocity) to SKGLView/SKCanvasView. The existing touch API only provides raw touch events with no velocity data. The iOS conflict with UIGestureRecognizer is a real platform constraint. Classified as feature-request because this adds entirely new functionality (gesture recognition layer) not currently in any form in the views package.",
    "keySignals": [
      {
        "text": "When I add the gesture recognizer to the native view it overrides the touches within the SKGLView",
        "source": "issue body",
        "interpretation": "iOS SKTouchHandler is itself a UIGestureRecognizer — adding a second recognizer without proper coordination causes conflict."
      },
      {
        "text": "Maybe adding some gestures to the SKGLView and SKCanvasView would be enable more people to utilize the skia views in forms easily",
        "source": "issue body",
        "interpretation": "Core feature request: add gesture events to the views API."
      },
      {
        "text": "I previously started some ideas that did gestures such as fling, scale and rotate: https://github.com/mono/SkiaSharp.Extended/pull/79",
        "source": "comment by mattleibow (maintainer)",
        "interpretation": "Maintainer has explored this in SkiaSharp.Extended — suggests the feature may be intentionally kept in an extension package."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SKTouchEventArgs.cs",
        "lines": "1-86",
        "finding": "SKTouchEventArgs exposes Id, ActionType (Pressed/Moved/Released/Cancelled/WheelChanged), Location, InContact, WheelDelta, Pressure — no velocity or gesture-derived data exists anywhere in the current API.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs",
        "lines": "10-111",
        "finding": "iOS SKTouchHandler extends UIGestureRecognizer and fires only primitive Pressed/Moved/Released/Cancelled events. No velocity tracking. Being a UIGestureRecognizer itself is why adding additional gesture recognizers to the native view conflicts.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs",
        "lines": "1-139",
        "finding": "Android SKTouchHandler hooks View.Touch and processes MotionEventActions (Down/Move/Up/Cancel). The underlying MotionEvent has velocity data via VelocityTracker but it is never extracted or surfaced in SKTouchEventArgs.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Track touch history manually: store (timestamp, location) on each Moved event, compute velocity on Released using the last N samples over a time window.",
      "On Android: use VelocityTracker directly in a custom renderer — the raw MotionEvent velocity is available but not exposed by SKTouchHandler.",
      "Refer to SkiaSharp.Extended PR #79 for a community gesture recognizer implementation that wraps the primitive touch events."
    ],
    "nextQuestions": [
      "Is gesture support planned for SkiaSharp.Views.Maui or will it remain in SkiaSharp.Extended only?",
      "Should velocity be added to SKTouchEventArgs itself, or should a separate SKGestureEventArgs be introduced?",
      "Does the iOS UIGestureRecognizer conflict need to be resolved before gesture support can be added on that platform?"
    ],
    "resolution": {
      "hypothesis": "Velocity tracking can be computed from existing touch events. For a first-class solution, velocity could be added to SKTouchEventArgs (Android via VelocityTracker, iOS via UITouch.estimatedProperties or manual tracking) or a new gesture event layer could be added above the raw touch API.",
      "proposals": [
        {
          "title": "Compute velocity from touch history (userland workaround)",
          "description": "Track (timestamp, location) pairs in the SKTouch handler. On Released, compute dx/dt and dy/dt over the last 100ms to derive fling velocity. No library changes needed.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Add velocity fields to SKTouchEventArgs",
          "description": "Extend SKTouchEventArgs with VelocityX and VelocityY properties. Populate from VelocityTracker on Android and manual tracking on iOS. This is an additive ABI-safe change.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Use SkiaSharp.Extended gesture recognizers",
          "description": "Reference the gesture recognizer implementation in SkiaSharp.Extended PR #79 (fling, scale, rotate) as a standalone helper that wraps primitive SKTouchEventArgs events.",
          "category": "alternative",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Compute velocity from touch history (userland workaround)",
      "recommendedReason": "No library changes needed, works today, and the maintainer's own reference (SkiaSharp.Extended) demonstrates this pattern is viable."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.8,
      "reason": "Valid feature request with community interest. Maintainer has explored gesture support in SkiaSharp.Extended. The feature has not been implemented in core SkiaSharp.Views. Keep open to track interest and design decision.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct labels: single type/feature-request, area/SkiaSharp.Views, platforms",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp.Views",
          "os/iOS",
          "os/Android"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain workaround and point to SkiaSharp.Extended implementation",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report! You can implement fling velocity today by tracking touch history manually:\n\n```csharp\n// Track points and times in your touch handler\nvar history = new List<(SKPoint pt, DateTime time)>();\n\nvoid OnTouch(object sender, SKTouchEventArgs e)\n{\n    switch (e.ActionType)\n    {\n        case SKTouchAction.Pressed:\n            history.Clear();\n            history.Add((e.Location, DateTime.UtcNow));\n            break;\n        case SKTouchAction.Moved:\n            history.Add((e.Location, DateTime.UtcNow));\n            // Keep only last 100ms\n            var cutoff = DateTime.UtcNow.AddMilliseconds(-100);\n            history.RemoveAll(h => h.time < cutoff);\n            break;\n        case SKTouchAction.Released:\n            if (history.Count >= 2)\n            {\n                var oldest = history[0];\n                var newest = history[^1];\n                var dt = (float)(newest.time - oldest.time).TotalSeconds;\n                if (dt > 0)\n                {\n                    float vx = (newest.pt.X - oldest.pt.X) / dt;\n                    float vy = (newest.pt.Y - oldest.pt.Y) / dt;\n                    // vx, vy are pixels/second fling velocity\n                }\n            }\n            break;\n    }\n    e.Handled = true;\n}\n```\n\nFor a more complete solution, @mattleibow previously started gesture recognizer implementations (fling, scale, rotate) in [SkiaSharp.Extended PR #79](https://github.com/mono/SkiaSharp.Extended/pull/79) — that may be worth tracking for a higher-level API.\n\nAdding velocity directly to `SKTouchEventArgs` or a new gesture event layer is a valid enhancement for the library itself."
      }
    ]
  }
}
```

</details>
