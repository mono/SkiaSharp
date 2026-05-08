# Issue Triage Report — #607

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T12:30:00Z |
| Type | type/bug (0.87 (87%)) |
| Area | area/SkiaSharp.Views.Maui (0.90 (90%)) |
| Suggested action | keep-open (0.82 (82%)) |

**Issue Summary:** SKTouchAction.Entered and SKTouchAction.Exited events are never fired on iOS and Android; these actions exist in the SKTouchAction enum and are dispatched by the Windows platform handler but the iOS and Android handlers have no mechanism to synthesize them, creating a platform parity gap.

**Analysis:** The SKTouchAction enum defines Entered and Exited values; the Windows handler wires PointerEntered/PointerExited OS events to them, but the Android handler (MotionEvent switch) and iOS handler (UIGestureRecognizer overrides) have no equivalent. Because mobile OSes do not raise native enter/exit events, these must be synthesized by tracking touch position relative to view bounds during Move events. Tizen already synthesizes Exited via PointStateType.Leave. The fix path is clear but requires per-platform state tracking.

**Recommendations:** **keep-open** — Valid platform parity bug; fix path is clear (synthesize Entered/Exited from Move events with bounds tracking) but requires per-platform implementation work. A user-space workaround exists. Maintainer acknowledged in 2018; issue should remain open until implemented.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/iOS, os/Android |
| Backends | — |
| Tenets | tenet/compatibility, tenet/reliability |
| Partner | — |
| Current labels | type/bug, type/enhancement, os/iOS, os/Android, area/SkiaSharp.Views, type/feature-request, area/SkiaSharp.Views.Maui, tenet/compatibility, tenet/reliability, triage/triaged |

## Evidence

### Reproduction

1. Create a SkiaSharp.Views.Maui app with touch events enabled on iOS or Android
2. Handle the Touch event and listen for SKTouchAction.Entered or SKTouchAction.Exited
3. Touch the view and drag the finger outside its bounds or re-enter
4. Observe that Entered and Exited are never raised on iOS or Android (they are on Windows)

**Environment:** iOS / Android, SkiaSharp.Views.Maui, all versions

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/607#issuecomment-411721040 — Reporter's workaround with bounds tracking and scale-factor hack

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | missing-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | net8.0-ios, net8.0-android |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Code inspection of the current MAUI Android and iOS SKTouchHandler confirms no Entered/Exited synthesis is present; the issue persists. |

## Analysis

### Technical Summary

The SKTouchAction enum defines Entered and Exited values; the Windows handler wires PointerEntered/PointerExited OS events to them, but the Android handler (MotionEvent switch) and iOS handler (UIGestureRecognizer overrides) have no equivalent. Because mobile OSes do not raise native enter/exit events, these must be synthesized by tracking touch position relative to view bounds during Move events. Tizen already synthesizes Exited via PointStateType.Leave. The fix path is clear but requires per-platform state tracking.

### Rationale

Platform parity gap: SKTouchAction.Entered and Exited exist in the public API and are dispatched on Windows (via native OS events) but silently dropped on iOS and Android. Per the label taxonomy this qualifies as type/bug. The reporter provided a working pseudocode algorithm and a user-space workaround with a known coordinate-scaling caveat. Maintainer acknowledged in 2018 the feature would be addressed.

### Key Signals

- "Currently iOS is implemented only on UWP (because it is connected as a passthrough), but it would be useful to have this functionality connected on iOS/Android" — **issue body** (Reporter correctly identifies that Windows uses native passthrough while iOS/Android need synthesis from move events.)
- "it was the expected case on iOS/Android (which required looking through the code here)" — **comment #2 (reporter)** (Discoverability problem: the gap is not documented and users reasonably expect parity from a cross-platform API.)
- "This is something that we will be looking at in the future." — **comment #4 (maintainer mattleibow)** (Maintainer acknowledged the request in 2018; the issue is accepted work, not contested.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Windows/SKTouchHandler.cs` | 29-43 | direct | Windows handler subscribes to PointerEntered and PointerExited native events and maps them to SKTouchAction.Entered and SKTouchAction.Exited via CommonHandler. This is the only platform where these actions are dispatched. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs` | 63-109 | direct | Android handler switches on MotionEventActions (Down, Move, Up, Cancel). There is no case for Entered or Exited. The Move branch iterates pointers and fires SKTouchAction.Moved only, with no bounds check. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs` | 54-95 | direct | iOS handler overrides UIGestureRecognizer callbacks (TouchesBegan, TouchesMoved, TouchesEnded, TouchesCancelled). No override dispatches SKTouchAction.Entered or Exited; TouchesMoved only fires SKTouchAction.Moved. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Tizen/SKTouchHandler.cs` | 65-73 | related | Tizen handler maps PointStateType.Leave to SKTouchAction.Exited but has no mapping for Entered (no PointStateType.Enter equivalent used). Partial parity only. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SKTouchEventArgs.cs` | 60-69 | context | SKTouchAction enum defines: Entered, Pressed, Moved, Released, Cancelled, Exited, WheelChanged. All seven values are in the public API; only Windows fires Entered and Exited. |

### Workarounds

- Track a per-touch-id boolean IsInside flag; on SKTouchAction.Pressed set it to true; on SKTouchAction.Moved compare e.Location to the view Bounds and fire a synthesized Entered or Exited by re-raising your own event.
- Reporter noted the raw e.Location is in view-local pixel space and Bounds may need a device-pixel scaling factor (~2x on Retina/HiDPI) when comparing.

### Next Questions

- Should the Android implementation also handle MotionEventActions.HoverEnter/HoverExit for mouse/stylus pointers on Android (which do have native enter/exit events)?
- Should Entered fire on Pressed (touch started inside view) to match Windows behavior where entering with touch already down can happen?
- Does the iOS UIGestureRecognizer approach allow detecting re-entry after the finger left the view bounds, or is a different mechanism needed?

### Resolution Proposals

**Hypothesis:** Both Android and iOS need per-pointer state (inside/outside) tracked in their SKTouchHandler, and the Move event loop must check position against view bounds to synthesize and fire Entered/Exited transitions.

1. **Synthesize Entered/Exited in Android SKTouchHandler** — fix, confidence 0.80 (80%), cost/s, validated=untested
   - Add a per-pointer Dictionary<int, bool> isInsideMap to the Android handler. On Down set isInside=true. On Move, after computing coords, compare to view.Width/Height (in same pixel space as scalePixels output). Fire Entered or Exited when the inside state changes. On Up/Cancel remove the pointer from the map.
2. **Synthesize Entered/Exited in iOS SKTouchHandler** — fix, confidence 0.78 (78%), cost/s, validated=untested
   - Add a per-touch NSMapTable or Dictionary tracking inside state. In TouchesMoved check touch.LocationInView(View) against View.Bounds and fire extra FireEvent calls for Entered/Exited before the normal Moved event when boundary transitions occur.
3. **User-space workaround using current API** — workaround, confidence 0.85 (85%), cost/xs, validated=untested
   - Handle SKTouchAction.Pressed/Moved/Released manually and synthesize Entered/Exited by comparing e.Location to the SkiaSharp canvas bounds. See reporter's workaround in comments; apply appropriate device-pixel scaling (e.g. 2× on Retina) when comparing to Bounds.

**Recommended proposal:** Synthesize Entered/Exited in Android SKTouchHandler

**Why:** Android bounds tracking is more straightforward and the View provides the hit-test geometry in the same coordinate space as MotionEvent. iOS requires similar work in parallel.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.82 (82%) |
| Reason | Valid platform parity bug; fix path is clear (synthesize Entered/Exited from Move events with bounds tracking) but requires per-platform implementation work. A user-space workaround exists. Maintainer acknowledged in 2018; issue should remain open until implemented. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Correct labels: use single type/bug (remove duplicate type labels), set area/SkiaSharp.Views.Maui, keep os/iOS + os/Android and tenets | labels=type/bug, area/SkiaSharp.Views.Maui, os/iOS, os/Android, tenet/compatibility, tenet/reliability |
| add-comment | medium | 0.82 (82%) | Acknowledge the gap, confirm code investigation findings, share the workaround with the coordinate-scaling caveat noted | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and for sharing your workaround.

Code investigation confirms the issue: the Windows handler wires the native `PointerEntered`/`PointerExited` OS events directly to `SKTouchAction.Entered`/`SKTouchAction.Exited`, but the Android and iOS handlers have no equivalent native events — these would need to be **synthesized** by tracking whether each touch pointer is inside or outside the view bounds during `Moved` events.

**User-space workaround (while this is open):**

Track an `isInside` flag per touch ID and compute it from `e.Location` relative to the view bounds. Note that on Retina/HiDPI displays the raw `e.Location` is in view-pixel space, so you may need to scale the comparison (the `~2×` factor the reporter noted):

```csharp
private readonly Dictionary<long, bool> _isInsideByTouchId = new();

void OnTouch(object sender, SKTouchEventArgs e)
{
    var view = (SKCanvasView)sender;
    var bounds = new SKRect(0, 0, view.CanvasSize.Width, view.CanvasSize.Height);
    var inBounds = bounds.Contains(e.Location);

    switch (e.ActionType)
    {
        case SKTouchAction.Pressed:
            _isInsideByTouchId[e.Id] = true;
            break;
        case SKTouchAction.Moved:
            if (_isInsideByTouchId.TryGetValue(e.Id, out var wasInside))
            {
                if (!wasInside && inBounds)
                {
                    _isInsideByTouchId[e.Id] = true;
                    HandleAction(SKTouchAction.Entered, e);
                }
                else if (wasInside && !inBounds)
                {
                    _isInsideByTouchId[e.Id] = false;
                    HandleAction(SKTouchAction.Exited, e);
                }
            }
            break;
        case SKTouchAction.Released:
        case SKTouchAction.Cancelled:
            _isInsideByTouchId.Remove(e.Id);
            break;
    }
    e.Handled = true;
}
```

A proper fix in the library would add this bounds-tracking logic directly to the Android (`SKTouchHandler.OnTouch` Move branch) and iOS (`SKTouchHandler.TouchesMoved`) handlers. PRs welcome!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 607,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T12:30:00Z",
    "currentLabels": [
      "type/bug",
      "type/enhancement",
      "os/iOS",
      "os/Android",
      "area/SkiaSharp.Views",
      "type/feature-request",
      "area/SkiaSharp.Views.Maui",
      "tenet/compatibility",
      "tenet/reliability",
      "triage/triaged"
    ]
  },
  "summary": "SKTouchAction.Entered and SKTouchAction.Exited events are never fired on iOS and Android; these actions exist in the SKTouchAction enum and are dispatched by the Windows platform handler but the iOS and Android handlers have no mechanism to synthesize them, creating a platform parity gap.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.87
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.9
    },
    "platforms": [
      "os/iOS",
      "os/Android"
    ],
    "tenets": [
      "tenet/compatibility",
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "missing-output",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0-ios",
        "net8.0-android"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a SkiaSharp.Views.Maui app with touch events enabled on iOS or Android",
        "Handle the Touch event and listen for SKTouchAction.Entered or SKTouchAction.Exited",
        "Touch the view and drag the finger outside its bounds or re-enter",
        "Observe that Entered and Exited are never raised on iOS or Android (they are on Windows)"
      ],
      "environmentDetails": "iOS / Android, SkiaSharp.Views.Maui, all versions",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/607#issuecomment-411721040",
          "description": "Reporter's workaround with bounds tracking and scale-factor hack"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "Code inspection of the current MAUI Android and iOS SKTouchHandler confirms no Entered/Exited synthesis is present; the issue persists."
    }
  },
  "analysis": {
    "summary": "The SKTouchAction enum defines Entered and Exited values; the Windows handler wires PointerEntered/PointerExited OS events to them, but the Android handler (MotionEvent switch) and iOS handler (UIGestureRecognizer overrides) have no equivalent. Because mobile OSes do not raise native enter/exit events, these must be synthesized by tracking touch position relative to view bounds during Move events. Tizen already synthesizes Exited via PointStateType.Leave. The fix path is clear but requires per-platform state tracking.",
    "rationale": "Platform parity gap: SKTouchAction.Entered and Exited exist in the public API and are dispatched on Windows (via native OS events) but silently dropped on iOS and Android. Per the label taxonomy this qualifies as type/bug. The reporter provided a working pseudocode algorithm and a user-space workaround with a known coordinate-scaling caveat. Maintainer acknowledged in 2018 the feature would be addressed.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Windows/SKTouchHandler.cs",
        "lines": "29-43",
        "finding": "Windows handler subscribes to PointerEntered and PointerExited native events and maps them to SKTouchAction.Entered and SKTouchAction.Exited via CommonHandler. This is the only platform where these actions are dispatched.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs",
        "lines": "63-109",
        "finding": "Android handler switches on MotionEventActions (Down, Move, Up, Cancel). There is no case for Entered or Exited. The Move branch iterates pointers and fires SKTouchAction.Moved only, with no bounds check.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs",
        "lines": "54-95",
        "finding": "iOS handler overrides UIGestureRecognizer callbacks (TouchesBegan, TouchesMoved, TouchesEnded, TouchesCancelled). No override dispatches SKTouchAction.Entered or Exited; TouchesMoved only fires SKTouchAction.Moved.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Tizen/SKTouchHandler.cs",
        "lines": "65-73",
        "finding": "Tizen handler maps PointStateType.Leave to SKTouchAction.Exited but has no mapping for Entered (no PointStateType.Enter equivalent used). Partial parity only.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SKTouchEventArgs.cs",
        "lines": "60-69",
        "finding": "SKTouchAction enum defines: Entered, Pressed, Moved, Released, Cancelled, Exited, WheelChanged. All seven values are in the public API; only Windows fires Entered and Exited.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "Currently iOS is implemented only on UWP (because it is connected as a passthrough), but it would be useful to have this functionality connected on iOS/Android",
        "source": "issue body",
        "interpretation": "Reporter correctly identifies that Windows uses native passthrough while iOS/Android need synthesis from move events."
      },
      {
        "text": "it was the expected case on iOS/Android (which required looking through the code here)",
        "source": "comment #2 (reporter)",
        "interpretation": "Discoverability problem: the gap is not documented and users reasonably expect parity from a cross-platform API."
      },
      {
        "text": "This is something that we will be looking at in the future.",
        "source": "comment #4 (maintainer mattleibow)",
        "interpretation": "Maintainer acknowledged the request in 2018; the issue is accepted work, not contested."
      }
    ],
    "workarounds": [
      "Track a per-touch-id boolean IsInside flag; on SKTouchAction.Pressed set it to true; on SKTouchAction.Moved compare e.Location to the view Bounds and fire a synthesized Entered or Exited by re-raising your own event.",
      "Reporter noted the raw e.Location is in view-local pixel space and Bounds may need a device-pixel scaling factor (~2x on Retina/HiDPI) when comparing."
    ],
    "nextQuestions": [
      "Should the Android implementation also handle MotionEventActions.HoverEnter/HoverExit for mouse/stylus pointers on Android (which do have native enter/exit events)?",
      "Should Entered fire on Pressed (touch started inside view) to match Windows behavior where entering with touch already down can happen?",
      "Does the iOS UIGestureRecognizer approach allow detecting re-entry after the finger left the view bounds, or is a different mechanism needed?"
    ],
    "resolution": {
      "hypothesis": "Both Android and iOS need per-pointer state (inside/outside) tracked in their SKTouchHandler, and the Move event loop must check position against view bounds to synthesize and fire Entered/Exited transitions.",
      "proposals": [
        {
          "title": "Synthesize Entered/Exited in Android SKTouchHandler",
          "description": "Add a per-pointer Dictionary<int, bool> isInsideMap to the Android handler. On Down set isInside=true. On Move, after computing coords, compare to view.Width/Height (in same pixel space as scalePixels output). Fire Entered or Exited when the inside state changes. On Up/Cancel remove the pointer from the map.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Synthesize Entered/Exited in iOS SKTouchHandler",
          "description": "Add a per-touch NSMapTable or Dictionary tracking inside state. In TouchesMoved check touch.LocationInView(View) against View.Bounds and fire extra FireEvent calls for Entered/Exited before the normal Moved event when boundary transitions occur.",
          "category": "fix",
          "confidence": 0.78,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "User-space workaround using current API",
          "description": "Handle SKTouchAction.Pressed/Moved/Released manually and synthesize Entered/Exited by comparing e.Location to the SkiaSharp canvas bounds. See reporter's workaround in comments; apply appropriate device-pixel scaling (e.g. 2× on Retina) when comparing to Bounds.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Synthesize Entered/Exited in Android SKTouchHandler",
      "recommendedReason": "Android bounds tracking is more straightforward and the View provides the hit-test geometry in the same coordinate space as MotionEvent. iOS requires similar work in parallel."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.82,
      "reason": "Valid platform parity bug; fix path is clear (synthesize Entered/Exited from Move events with bounds tracking) but requires per-platform implementation work. A user-space workaround exists. Maintainer acknowledged in 2018; issue should remain open until implemented.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct labels: use single type/bug (remove duplicate type labels), set area/SkiaSharp.Views.Maui, keep os/iOS + os/Android and tenets",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Maui",
          "os/iOS",
          "os/Android",
          "tenet/compatibility",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the gap, confirm code investigation findings, share the workaround with the coordinate-scaling caveat noted",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report and for sharing your workaround.\n\nCode investigation confirms the issue: the Windows handler wires the native `PointerEntered`/`PointerExited` OS events directly to `SKTouchAction.Entered`/`SKTouchAction.Exited`, but the Android and iOS handlers have no equivalent native events — these would need to be **synthesized** by tracking whether each touch pointer is inside or outside the view bounds during `Moved` events.\n\n**User-space workaround (while this is open):**\n\nTrack an `isInside` flag per touch ID and compute it from `e.Location` relative to the view bounds. Note that on Retina/HiDPI displays the raw `e.Location` is in view-pixel space, so you may need to scale the comparison (the `~2×` factor the reporter noted):\n\n```csharp\nprivate readonly Dictionary<long, bool> _isInsideByTouchId = new();\n\nvoid OnTouch(object sender, SKTouchEventArgs e)\n{\n    var view = (SKCanvasView)sender;\n    var bounds = new SKRect(0, 0, view.CanvasSize.Width, view.CanvasSize.Height);\n    var inBounds = bounds.Contains(e.Location);\n\n    switch (e.ActionType)\n    {\n        case SKTouchAction.Pressed:\n            _isInsideByTouchId[e.Id] = true;\n            break;\n        case SKTouchAction.Moved:\n            if (_isInsideByTouchId.TryGetValue(e.Id, out var wasInside))\n            {\n                if (!wasInside && inBounds)\n                {\n                    _isInsideByTouchId[e.Id] = true;\n                    HandleAction(SKTouchAction.Entered, e);\n                }\n                else if (wasInside && !inBounds)\n                {\n                    _isInsideByTouchId[e.Id] = false;\n                    HandleAction(SKTouchAction.Exited, e);\n                }\n            }\n            break;\n        case SKTouchAction.Released:\n        case SKTouchAction.Cancelled:\n            _isInsideByTouchId.Remove(e.Id);\n            break;\n    }\n    e.Handled = true;\n}\n```\n\nA proper fix in the library would add this bounds-tracking logic directly to the Android (`SKTouchHandler.OnTouch` Move branch) and iOS (`SKTouchHandler.TouchesMoved`) handlers. PRs welcome!"
      }
    ]
  }
}
```

</details>
