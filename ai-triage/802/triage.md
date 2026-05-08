# Issue Triage Report — #802

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T18:26:15Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp.Views.Maui (0.80 (80%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** SkiaSharp views on Apple platforms (macOS, iOS) do not emit mouse motion events when no mouse button is pressed, because the touch handler uses UIGestureRecognizer which only tracks contacts; Windows correctly emits hover events via PointerMoved.

**Analysis:** The Apple platform touch handler uses UIGestureRecognizer (TouchesBegan/TouchesMoved/TouchesEnded), which only fires for touch contacts (mouse button held). To support hover events, the handler would need to either override AppKit mouse event methods (mouseMoved:, mouseEntered:, mouseExited:) or add a NSTrackingArea on macOS, or add a UIHoverGestureRecognizer on iOS/Mac Catalyst. The Windows handler correctly handles this via PointerMoved which fires with IsInContact=false. The SKTouchEventArgs API already supports InContact=false Moved/Entered/Exited events — the gap is purely in the Apple platform implementation.

**Recommendations:** **needs-investigation** — Root cause is confirmed by maintainer and code investigation. The fix approach (UIHoverGestureRecognizer / NSTrackingArea) is known but has not been implemented. Related issue #3523 is a newer duplicate for MAUI Mac Catalyst specifically.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/macOS, os/iOS |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create a Xamarin.Forms (or MAUI) app with SKCanvasView and EnableTouchEvents = true
2. Subscribe to the Touch event and log all events
3. Run on macOS
4. Move the mouse cursor over the canvas without pressing any button
5. Observe: no Touch events are fired; events only appear when a mouse button is held down

**Environment:** macOS (Forms/Xamarin.Mac, iOS emulator), confirmed also on MAUI Mac Catalyst

**Repository links:**
- https://github.com/ylatuya/lienzosharp/blob/master/src/LienzoSharp.Skia.Forms/LienzoFormsView.cs#L104 — Reporter's code showing use of Touch events for mouse tracking
- https://github.com/mono/SkiaSharp/issues/3523 — Related issue: [Mac Catalyst] Mouse hover (non-contact move) events not delivered by SKCanvasView (newer, MAUI-specific)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | missing-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | net-ios, net-maccatalyst |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The Apple SKTouchHandler still uses UIGestureRecognizer which only handles contacts; no NSTrackingArea or mouse event overrides have been added. The Windows handler correctly uses PointerMoved with IsInContact=false for hover. |

## Analysis

### Technical Summary

The Apple platform touch handler uses UIGestureRecognizer (TouchesBegan/TouchesMoved/TouchesEnded), which only fires for touch contacts (mouse button held). To support hover events, the handler would need to either override AppKit mouse event methods (mouseMoved:, mouseEntered:, mouseExited:) or add a NSTrackingArea on macOS, or add a UIHoverGestureRecognizer on iOS/Mac Catalyst. The Windows handler correctly handles this via PointerMoved which fires with IsInContact=false. The SKTouchEventArgs API already supports InContact=false Moved/Entered/Exited events — the gap is purely in the Apple platform implementation.

### Rationale

This is a platform parity bug: Windows correctly emits hover events but the Apple platform handler does not. The API contract (SKTouchAction.Moved with InContact=false) already exists. The maintainer confirmed the root cause in a comment. Related issue #3523 is newer and MAUI-specific but describes the same gap on Mac Catalyst.

### Key Signals

- "I do get move events when moving the mouse, but only when a button is pressed" — **reporter comment** (Confirms hover-only motion is missing; drag motion (button held) works fine)
- "Looking at the implementation, at least for the macOS/iOS part, it seems it's only handling mouse drag events" — **reporter comment** (Reporter traced the root cause to SKTouchHandler.cs handling only TouchesMoved (contact))
- "the macOS views use a gesture recognizer - which only works when the mouse is pressed. A better alternative is to override the mouse events on the view itself and/or use a NSTrackingArea" — **maintainer comment (mattleibow, 2020-04-26)** (Maintainer confirmed root cause and suggested fix approach)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs` | 55-95 | direct | SKTouchHandler extends UIGestureRecognizer and only overrides TouchesBegan, TouchesMoved, TouchesEnded, TouchesCancelled. All events fire inContact=true (or false for release). There is no handling for hover/mouse-move-without-contact. No UIHoverGestureRecognizer or NSTrackingArea is used. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Windows/SKTouchHandler.cs` | 25-45 | related | Windows handler registers PointerEntered, PointerExited, PointerMoved, PointerPressed, PointerReleased, PointerCanceled, PointerWheelChanged. PointerMoved fires for hover (IsInContact=false), confirming the Windows implementation correctly supports hover. This is the platform parity gap. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SKTouchEventArgs.cs` | 60-69 | context | SKTouchAction enum includes Entered, Moved, Exited, and SKTouchEventArgs has InContact property. The API already fully supports hover events — the gap is only in the Apple platform handler implementation. |

### Workarounds

- On macOS (AppKit-based views), override the native view's mouseMoved: method and add a NSTrackingArea to capture hover events outside SkiaSharp's built-in touch handler
- On Windows (WinUI), hover events already work correctly — no workaround needed
- Subscribe to platform-specific mouse events directly through MAUI's handler infrastructure if cross-platform abstraction is not critical

### Next Questions

- Does the issue affect SKGLView (not just SKCanvasView) on Apple?
- Is NSTrackingArea approach feasible for UIKit/Mac Catalyst or only AppKit/macOS?
- Should UIHoverGestureRecognizer (iOS 13+) be used for iOS/Mac Catalyst instead of NSTrackingArea?

### Resolution Proposals

**Hypothesis:** The Apple SKTouchHandler uses UIGestureRecognizer which only captures contact events. Adding a UIHoverGestureRecognizer (iOS/Mac Catalyst 13+) or overriding AppKit mouse event methods with NSTrackingArea (macOS) would enable non-contact mouse move events.

1. **Add UIHoverGestureRecognizer for iOS/Mac Catalyst** — fix, confidence 0.80 (80%), cost/m, validated=untested
   - In SKTouchHandler for Apple, register a UIHoverGestureRecognizer alongside the existing UIGestureRecognizer. Map its Began/Changed/Ended states to SKTouchAction.Entered/Moved/Exited with InContact=false.
2. **Add NSTrackingArea for AppKit/macOS views** — fix, confidence 0.75 (75%), cost/m, validated=untested
   - For the AppKit-based macOS views, override the native view's updateTrackingAreas method and add a NSTrackingArea to receive mouseMoved:, mouseEntered:, mouseExited: events.

**Recommended proposal:** Add UIHoverGestureRecognizer for iOS/Mac Catalyst

**Why:** UIHoverGestureRecognizer is available since iOS 13 / Mac Catalyst 13 and integrates directly with the existing UIGestureRecognizer-based touch handler without requiring separate platform code paths. It is the idiomatic UIKit approach for hover tracking.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Root cause is confirmed by maintainer and code investigation. The fix approach (UIHoverGestureRecognizer / NSTrackingArea) is known but has not been implemented. Related issue #3523 is a newer duplicate for MAUI Mac Catalyst specifically. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Apply bug, views-maui, macOS, iOS, and compatibility labels | labels=type/bug, area/SkiaSharp.Views.Maui, os/macOS, os/iOS, tenet/compatibility |
| link-related | low | 0.90 (90%) | Cross-reference related MAUI Mac Catalyst hover issue #3523 | linkedIssue=#3523 |
| add-comment | medium | 0.82 (82%) | Post analysis referencing maintainer comment, code path, and fix approach | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report. The root cause has been confirmed: the Apple platform `SKTouchHandler` uses `UIGestureRecognizer`, which only fires for contacts (mouse button pressed or finger touching). Hover events (mouse movement without a button held) are never forwarded to the `Touch` event.

For reference, the Windows handler (`SKTouchHandler.cs` for WinUI) correctly handles this via `PointerMoved`, which fires with `IsInContact = false` for hover. The `SKTouchEventArgs` API already supports this — the gap is purely in the Apple platform implementation.

**Fix approach:** Adding a `UIHoverGestureRecognizer` (available since iOS 13 / Mac Catalyst 13) alongside the existing gesture recognizer would enable non-contact mouse move events. For AppKit-based macOS, a `NSTrackingArea` would be needed. See the maintainer note from Apr 2020.

**Related:** #3523 tracks the same gap for MAUI Mac Catalyst specifically.

**Temporary workaround:** On macOS, you can subclass the native view and override `mouseMoved:` while adding a `NSTrackingArea` to receive hover events outside SkiaSharp.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 802,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T18:26:15Z"
  },
  "summary": "SkiaSharp views on Apple platforms (macOS, iOS) do not emit mouse motion events when no mouse button is pressed, because the touch handler uses UIGestureRecognizer which only tracks contacts; Windows correctly emits hover events via PointerMoved.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.8
    },
    "platforms": [
      "os/macOS",
      "os/iOS"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "missing-output",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net-ios",
        "net-maccatalyst"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Xamarin.Forms (or MAUI) app with SKCanvasView and EnableTouchEvents = true",
        "Subscribe to the Touch event and log all events",
        "Run on macOS",
        "Move the mouse cursor over the canvas without pressing any button",
        "Observe: no Touch events are fired; events only appear when a mouse button is held down"
      ],
      "environmentDetails": "macOS (Forms/Xamarin.Mac, iOS emulator), confirmed also on MAUI Mac Catalyst",
      "repoLinks": [
        {
          "url": "https://github.com/ylatuya/lienzosharp/blob/master/src/LienzoSharp.Skia.Forms/LienzoFormsView.cs#L104",
          "description": "Reporter's code showing use of Touch events for mouse tracking"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3523",
          "description": "Related issue: [Mac Catalyst] Mouse hover (non-contact move) events not delivered by SKCanvasView (newer, MAUI-specific)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "The Apple SKTouchHandler still uses UIGestureRecognizer which only handles contacts; no NSTrackingArea or mouse event overrides have been added. The Windows handler correctly uses PointerMoved with IsInContact=false for hover."
    }
  },
  "analysis": {
    "summary": "The Apple platform touch handler uses UIGestureRecognizer (TouchesBegan/TouchesMoved/TouchesEnded), which only fires for touch contacts (mouse button held). To support hover events, the handler would need to either override AppKit mouse event methods (mouseMoved:, mouseEntered:, mouseExited:) or add a NSTrackingArea on macOS, or add a UIHoverGestureRecognizer on iOS/Mac Catalyst. The Windows handler correctly handles this via PointerMoved which fires with IsInContact=false. The SKTouchEventArgs API already supports InContact=false Moved/Entered/Exited events — the gap is purely in the Apple platform implementation.",
    "rationale": "This is a platform parity bug: Windows correctly emits hover events but the Apple platform handler does not. The API contract (SKTouchAction.Moved with InContact=false) already exists. The maintainer confirmed the root cause in a comment. Related issue #3523 is newer and MAUI-specific but describes the same gap on Mac Catalyst.",
    "keySignals": [
      {
        "text": "I do get move events when moving the mouse, but only when a button is pressed",
        "source": "reporter comment",
        "interpretation": "Confirms hover-only motion is missing; drag motion (button held) works fine"
      },
      {
        "text": "Looking at the implementation, at least for the macOS/iOS part, it seems it's only handling mouse drag events",
        "source": "reporter comment",
        "interpretation": "Reporter traced the root cause to SKTouchHandler.cs handling only TouchesMoved (contact)"
      },
      {
        "text": "the macOS views use a gesture recognizer - which only works when the mouse is pressed. A better alternative is to override the mouse events on the view itself and/or use a NSTrackingArea",
        "source": "maintainer comment (mattleibow, 2020-04-26)",
        "interpretation": "Maintainer confirmed root cause and suggested fix approach"
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs",
        "lines": "55-95",
        "finding": "SKTouchHandler extends UIGestureRecognizer and only overrides TouchesBegan, TouchesMoved, TouchesEnded, TouchesCancelled. All events fire inContact=true (or false for release). There is no handling for hover/mouse-move-without-contact. No UIHoverGestureRecognizer or NSTrackingArea is used.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Windows/SKTouchHandler.cs",
        "lines": "25-45",
        "finding": "Windows handler registers PointerEntered, PointerExited, PointerMoved, PointerPressed, PointerReleased, PointerCanceled, PointerWheelChanged. PointerMoved fires for hover (IsInContact=false), confirming the Windows implementation correctly supports hover. This is the platform parity gap.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SKTouchEventArgs.cs",
        "lines": "60-69",
        "finding": "SKTouchAction enum includes Entered, Moved, Exited, and SKTouchEventArgs has InContact property. The API already fully supports hover events — the gap is only in the Apple platform handler implementation.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "On macOS (AppKit-based views), override the native view's mouseMoved: method and add a NSTrackingArea to capture hover events outside SkiaSharp's built-in touch handler",
      "On Windows (WinUI), hover events already work correctly — no workaround needed",
      "Subscribe to platform-specific mouse events directly through MAUI's handler infrastructure if cross-platform abstraction is not critical"
    ],
    "nextQuestions": [
      "Does the issue affect SKGLView (not just SKCanvasView) on Apple?",
      "Is NSTrackingArea approach feasible for UIKit/Mac Catalyst or only AppKit/macOS?",
      "Should UIHoverGestureRecognizer (iOS 13+) be used for iOS/Mac Catalyst instead of NSTrackingArea?"
    ],
    "resolution": {
      "hypothesis": "The Apple SKTouchHandler uses UIGestureRecognizer which only captures contact events. Adding a UIHoverGestureRecognizer (iOS/Mac Catalyst 13+) or overriding AppKit mouse event methods with NSTrackingArea (macOS) would enable non-contact mouse move events.",
      "proposals": [
        {
          "title": "Add UIHoverGestureRecognizer for iOS/Mac Catalyst",
          "description": "In SKTouchHandler for Apple, register a UIHoverGestureRecognizer alongside the existing UIGestureRecognizer. Map its Began/Changed/Ended states to SKTouchAction.Entered/Moved/Exited with InContact=false.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Add NSTrackingArea for AppKit/macOS views",
          "description": "For the AppKit-based macOS views, override the native view's updateTrackingAreas method and add a NSTrackingArea to receive mouseMoved:, mouseEntered:, mouseExited: events.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add UIHoverGestureRecognizer for iOS/Mac Catalyst",
      "recommendedReason": "UIHoverGestureRecognizer is available since iOS 13 / Mac Catalyst 13 and integrates directly with the existing UIGestureRecognizer-based touch handler without requiring separate platform code paths. It is the idiomatic UIKit approach for hover tracking."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Root cause is confirmed by maintainer and code investigation. The fix approach (UIHoverGestureRecognizer / NSTrackingArea) is known but has not been implemented. Related issue #3523 is a newer duplicate for MAUI Mac Catalyst specifically.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views-maui, macOS, iOS, and compatibility labels",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Maui",
          "os/macOS",
          "os/iOS",
          "tenet/compatibility"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference related MAUI Mac Catalyst hover issue #3523",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 3523
      },
      {
        "type": "add-comment",
        "description": "Post analysis referencing maintainer comment, code path, and fix approach",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report. The root cause has been confirmed: the Apple platform `SKTouchHandler` uses `UIGestureRecognizer`, which only fires for contacts (mouse button pressed or finger touching). Hover events (mouse movement without a button held) are never forwarded to the `Touch` event.\n\nFor reference, the Windows handler (`SKTouchHandler.cs` for WinUI) correctly handles this via `PointerMoved`, which fires with `IsInContact = false` for hover. The `SKTouchEventArgs` API already supports this — the gap is purely in the Apple platform implementation.\n\n**Fix approach:** Adding a `UIHoverGestureRecognizer` (available since iOS 13 / Mac Catalyst 13) alongside the existing gesture recognizer would enable non-contact mouse move events. For AppKit-based macOS, a `NSTrackingArea` would be needed. See the maintainer note from Apr 2020.\n\n**Related:** #3523 tracks the same gap for MAUI Mac Catalyst specifically.\n\n**Temporary workaround:** On macOS, you can subclass the native view and override `mouseMoved:` while adding a `NSTrackingArea` to receive hover events outside SkiaSharp."
      }
    ]
  }
}
```

</details>
