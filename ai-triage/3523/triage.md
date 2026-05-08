# Issue Triage Report — #3523

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T00:07:00Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp.Views.Maui (0.95 (95%)) |
| Suggested action | ready-to-fix (0.90 (90%)) |

**Issue Summary:** On Mac Catalyst, SKCanvasView with EnableTouchEvents=true does not deliver mouse hover (non-contact move/entered/exited) events because UIHoverGestureRecognizer is not added to the Apple touch handler.

**Analysis:** The Apple platform SKTouchHandler extends UIGestureRecognizer and only intercepts touchesBegan/touchesMoved/touchesEnded, which are contact-only events. UIKit requires a separate UIHoverGestureRecognizer (available since iOS 13/Mac Catalyst 13) to receive passive pointer movement events. The SKCanvasViewHandler.Apple.cs and SKTouchHandlerProxy.cs do not add a UIHoverGestureRecognizer, so hover events are silently dropped.

**Recommendations:** **ready-to-fix** — Root cause is identified (UIHoverGestureRecognizer not added), fix path is well-understood, reproduction is complete, and the reporter provided a working code sample. This is ready for implementation.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/macOS, os/iOS |
| Backends | — |
| Tenets | — |
| Partner | partner/maui |

## Evidence

### Reproduction

1. Create a MAUI app targeting net9.0-maccatalyst
2. Add SKCanvasView with EnableTouchEvents="True"
3. Subscribe to Touch event and log all events
4. Run on macOS
5. Move mouse cursor over canvas without clicking
6. Observe: no events are logged

**Environment:** SkiaSharp 3.119.1, SkiaSharp.Views.Maui.Controls 3.119.1, .NET 9.0, macOS (Mac Catalyst). Also affects iPadOS with trackpad/mouse support.

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | missing-output |
| Error message | No touch events delivered when mouse hovers without button pressed on Mac Catalyst |
| Repro quality | complete |
| Target frameworks | net9.0-maccatalyst |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.119.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The Apple touch handler (SKTouchHandler.cs) does not use UIHoverGestureRecognizer and this has not changed in recent commits. |

## Analysis

### Technical Summary

The Apple platform SKTouchHandler extends UIGestureRecognizer and only intercepts touchesBegan/touchesMoved/touchesEnded, which are contact-only events. UIKit requires a separate UIHoverGestureRecognizer (available since iOS 13/Mac Catalyst 13) to receive passive pointer movement events. The SKCanvasViewHandler.Apple.cs and SKTouchHandlerProxy.cs do not add a UIHoverGestureRecognizer, so hover events are silently dropped.

### Rationale

This is a clear platform parity bug. The Mac Catalyst UIKit model requires UIHoverGestureRecognizer for passive pointer tracking, which is distinct from contact (touch/click) events. The current SKTouchHandler does not implement this. The reporter (a collaborator) has identified the root cause and provided a working code sample for the fix. The issue is well-specified and actionable.

### Key Signals

- "No touch events are delivered at all when the mouse hovers over the canvas without pressing a button." — **issue body** (Missing-output bug, not wrong-output. The feature is simply absent for non-contact pointer movement.)
- "UIHoverGestureRecognizer is available since iOS 13.0 / Mac Catalyst 13.0" — **issue body** (The fix uses a stable, widely-available UIKit API. No version constraints beyond what SkiaSharp already targets.)
- "This works correctly on other platforms (e.g., WPF, WinUI)." — **issue body** (Platform parity gap — hover events work on Windows platforms but not Mac Catalyst.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs` | 10-111 | direct | SKTouchHandler extends UIGestureRecognizer and only overrides TouchesBegan, TouchesMoved, TouchesEnded, TouchesCancelled. There is no UIHoverGestureRecognizer added anywhere, confirming hover events are not handled. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandlerProxy.cs` | 1-55 | direct | SKTouchHandlerProxy creates SKTouchHandler and calls SetEnabled which only adds the UIGestureRecognizer to the view. No UIHoverGestureRecognizer is created or registered for Mac Catalyst or iPadOS. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Apple.cs` | 1-89 | related | SKCanvasViewHandler.Apple.cs delegates touch events entirely to SKTouchHandlerProxy, which does not handle UIHoverGestureRecognizer. The MapEnableTouchEvents method only toggles the existing UIGestureRecognizer. |

### Workarounds

- No known workaround — UIHoverGestureRecognizer must be added to the native view to receive hover events on Mac Catalyst. There is no way to intercept passive mouse movement via the existing UIGestureRecognizer subclass.

### Next Questions

- Should UIHoverGestureRecognizer be added only for Mac Catalyst (via conditional compilation) or also for iPadOS builds?
- Does SKGLView on Mac Catalyst have the same gap? The GLViewHandler also uses SKTouchHandlerProxy.
- What minimum deployment target is required? UIHoverGestureRecognizer is iOS/Mac Catalyst 13+.

### Resolution Proposals

**Hypothesis:** Adding UIHoverGestureRecognizer to SKTouchHandler (or SKTouchHandlerProxy) for Mac Catalyst and iPadOS builds will deliver SKTouchAction.Entered, Moved (InContact=false), and Exited events for passive pointer movement.

1. **Add UIHoverGestureRecognizer to SKTouchHandler** — fix, confidence 0.90 (90%), cost/s, validated=untested
   - In SKTouchHandler (or SKTouchHandlerProxy), conditionally add a UIHoverGestureRecognizer when EnableTouchEvents is true and running on Mac Catalyst or iPadOS. Map UIGestureRecognizerState.Began → SKTouchAction.Entered, Changed → SKTouchAction.Moved (inContact: false), Ended/Cancelled → SKTouchAction.Exited.

**Recommended proposal:** Add UIHoverGestureRecognizer to SKTouchHandler

**Why:** Root cause is clear, the fix is minimal and confined to the Apple touch handler, and UIHoverGestureRecognizer is stable since iOS/Mac Catalyst 13.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.90 (90%) |
| Reason | Root cause is identified (UIHoverGestureRecognizer not added), fix path is well-understood, reproduction is complete, and the reporter provided a working code sample. This is ready for implementation. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, MAUI views, macOS, iOS, and partner/maui labels | labels=type/bug, area/SkiaSharp.Views.Maui, os/macOS, os/iOS, partner/maui |
| add-comment | medium | 0.90 (90%) | Acknowledge the well-analyzed report and confirm ready-to-fix status | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed analysis and the suggested fix! This is a confirmed gap — `UIHoverGestureRecognizer` is required on Mac Catalyst/iPadOS to receive passive pointer movement events, and it's not currently wired up in `SKTouchHandler`.

The fix proposed in the issue looks correct: add a `UIHoverGestureRecognizer` alongside the existing `UIGestureRecognizer` in the Apple touch handler, mapping the gesture recognizer states to `SKTouchAction.Entered`, `Moved` (with `inContact: false`), and `Exited`.

A few open questions for the implementation:
- Should this be Mac Catalyst-only, or also enabled for iPadOS (which also supports trackpad/mouse)?
- Does `SKGLView` on Mac Catalyst need the same fix?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3523,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T00:07:00Z"
  },
  "summary": "On Mac Catalyst, SKCanvasView with EnableTouchEvents=true does not deliver mouse hover (non-contact move/entered/exited) events because UIHoverGestureRecognizer is not added to the Apple touch handler.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.95
    },
    "platforms": [
      "os/macOS",
      "os/iOS"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "missing-output",
      "errorMessage": "No touch events delivered when mouse hovers without button pressed on Mac Catalyst",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net9.0-maccatalyst"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a MAUI app targeting net9.0-maccatalyst",
        "Add SKCanvasView with EnableTouchEvents=\"True\"",
        "Subscribe to Touch event and log all events",
        "Run on macOS",
        "Move mouse cursor over canvas without clicking",
        "Observe: no events are logged"
      ],
      "environmentDetails": "SkiaSharp 3.119.1, SkiaSharp.Views.Maui.Controls 3.119.1, .NET 9.0, macOS (Mac Catalyst). Also affects iPadOS with trackpad/mouse support."
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.119.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The Apple touch handler (SKTouchHandler.cs) does not use UIHoverGestureRecognizer and this has not changed in recent commits."
    }
  },
  "analysis": {
    "summary": "The Apple platform SKTouchHandler extends UIGestureRecognizer and only intercepts touchesBegan/touchesMoved/touchesEnded, which are contact-only events. UIKit requires a separate UIHoverGestureRecognizer (available since iOS 13/Mac Catalyst 13) to receive passive pointer movement events. The SKCanvasViewHandler.Apple.cs and SKTouchHandlerProxy.cs do not add a UIHoverGestureRecognizer, so hover events are silently dropped.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs",
        "lines": "10-111",
        "finding": "SKTouchHandler extends UIGestureRecognizer and only overrides TouchesBegan, TouchesMoved, TouchesEnded, TouchesCancelled. There is no UIHoverGestureRecognizer added anywhere, confirming hover events are not handled.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandlerProxy.cs",
        "lines": "1-55",
        "finding": "SKTouchHandlerProxy creates SKTouchHandler and calls SetEnabled which only adds the UIGestureRecognizer to the view. No UIHoverGestureRecognizer is created or registered for Mac Catalyst or iPadOS.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Apple.cs",
        "lines": "1-89",
        "finding": "SKCanvasViewHandler.Apple.cs delegates touch events entirely to SKTouchHandlerProxy, which does not handle UIHoverGestureRecognizer. The MapEnableTouchEvents method only toggles the existing UIGestureRecognizer.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "No touch events are delivered at all when the mouse hovers over the canvas without pressing a button.",
        "source": "issue body",
        "interpretation": "Missing-output bug, not wrong-output. The feature is simply absent for non-contact pointer movement."
      },
      {
        "text": "UIHoverGestureRecognizer is available since iOS 13.0 / Mac Catalyst 13.0",
        "source": "issue body",
        "interpretation": "The fix uses a stable, widely-available UIKit API. No version constraints beyond what SkiaSharp already targets."
      },
      {
        "text": "This works correctly on other platforms (e.g., WPF, WinUI).",
        "source": "issue body",
        "interpretation": "Platform parity gap — hover events work on Windows platforms but not Mac Catalyst."
      }
    ],
    "rationale": "This is a clear platform parity bug. The Mac Catalyst UIKit model requires UIHoverGestureRecognizer for passive pointer tracking, which is distinct from contact (touch/click) events. The current SKTouchHandler does not implement this. The reporter (a collaborator) has identified the root cause and provided a working code sample for the fix. The issue is well-specified and actionable.",
    "workarounds": [
      "No known workaround — UIHoverGestureRecognizer must be added to the native view to receive hover events on Mac Catalyst. There is no way to intercept passive mouse movement via the existing UIGestureRecognizer subclass."
    ],
    "nextQuestions": [
      "Should UIHoverGestureRecognizer be added only for Mac Catalyst (via conditional compilation) or also for iPadOS builds?",
      "Does SKGLView on Mac Catalyst have the same gap? The GLViewHandler also uses SKTouchHandlerProxy.",
      "What minimum deployment target is required? UIHoverGestureRecognizer is iOS/Mac Catalyst 13+."
    ],
    "resolution": {
      "hypothesis": "Adding UIHoverGestureRecognizer to SKTouchHandler (or SKTouchHandlerProxy) for Mac Catalyst and iPadOS builds will deliver SKTouchAction.Entered, Moved (InContact=false), and Exited events for passive pointer movement.",
      "proposals": [
        {
          "title": "Add UIHoverGestureRecognizer to SKTouchHandler",
          "description": "In SKTouchHandler (or SKTouchHandlerProxy), conditionally add a UIHoverGestureRecognizer when EnableTouchEvents is true and running on Mac Catalyst or iPadOS. Map UIGestureRecognizerState.Began → SKTouchAction.Entered, Changed → SKTouchAction.Moved (inContact: false), Ended/Cancelled → SKTouchAction.Exited.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add UIHoverGestureRecognizer to SKTouchHandler",
      "recommendedReason": "Root cause is clear, the fix is minimal and confined to the Apple touch handler, and UIHoverGestureRecognizer is stable since iOS/Mac Catalyst 13."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.9,
      "reason": "Root cause is identified (UIHoverGestureRecognizer not added), fix path is well-understood, reproduction is complete, and the reporter provided a working code sample. This is ready for implementation.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, MAUI views, macOS, iOS, and partner/maui labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Maui",
          "os/macOS",
          "os/iOS",
          "partner/maui"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the well-analyzed report and confirm ready-to-fix status",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the detailed analysis and the suggested fix! This is a confirmed gap — `UIHoverGestureRecognizer` is required on Mac Catalyst/iPadOS to receive passive pointer movement events, and it's not currently wired up in `SKTouchHandler`.\n\nThe fix proposed in the issue looks correct: add a `UIHoverGestureRecognizer` alongside the existing `UIGestureRecognizer` in the Apple touch handler, mapping the gesture recognizer states to `SKTouchAction.Entered`, `Moved` (with `inContact: false`), and `Exited`.\n\nA few open questions for the implementation:\n- Should this be Mac Catalyst-only, or also enabled for iPadOS (which also supports trackpad/mouse)?\n- Does `SKGLView` on Mac Catalyst need the same fix?"
      }
    ]
  }
}
```

</details>
