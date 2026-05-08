# Issue Triage Report — #3524

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-05T23:45:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views.Maui (0.95 (95%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** On Mac Catalyst, SKCanvasView with EnableTouchEvents=true never fires Touch events with SKTouchAction.WheelChanged when the user scrolls with a mouse wheel or two-finger trackpad gesture, because the Apple platform SKTouchHandler (UIGestureRecognizer subclass) has no scroll wheel handling.

**Analysis:** The Apple platform SKTouchHandler (source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs) is a UIGestureRecognizer subclass that overrides only TouchesBegan, TouchesMoved, TouchesEnded, and TouchesCancelled. It has no mechanism for capturing scroll wheel or trackpad scroll input on Mac Catalyst. On Mac Catalyst, scroll wheel events require either overriding ScrollWheel on the UIView or adding a UIPanGestureRecognizer with allowedScrollTypesMask. Windows platform already implements WheelChanged via PointerWheelChanged. This is a platform parity gap.

**Recommendations:** **needs-investigation** — Root cause is clearly identified in source code (missing scroll wheel handling in Apple SKTouchHandler). A complete reproduction scenario is provided. Implementation requires testing on Mac Catalyst which requires macOS hardware.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/macOS |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | partner/maui |

## Evidence

### Reproduction

1. Create a MAUI app targeting net9.0-maccatalyst
2. Add an SKCanvasView with EnableTouchEvents=True
3. Subscribe to the Touch event and log all events including ActionType and WheelDelta
4. Run on macOS
5. Scroll with mouse wheel or two-finger trackpad gesture over the canvas
6. Observe: no WheelChanged events are logged, WheelDelta is always 0

**Environment:** SkiaSharp 3.119.1, SkiaSharp.Views.Maui.Controls 3.119.1, .NET 9.0, macOS (Mac Catalyst)

**Related issues:** #3523, #3533, #3534

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | missing-output |
| Error message | — |
| Repro quality | complete |
| Target frameworks | net9.0-maccatalyst |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.119.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The Apple SKTouchHandler still lacks scroll wheel handling as confirmed by reading the source code. |

## Analysis

### Technical Summary

The Apple platform SKTouchHandler (source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs) is a UIGestureRecognizer subclass that overrides only TouchesBegan, TouchesMoved, TouchesEnded, and TouchesCancelled. It has no mechanism for capturing scroll wheel or trackpad scroll input on Mac Catalyst. On Mac Catalyst, scroll wheel events require either overriding ScrollWheel on the UIView or adding a UIPanGestureRecognizer with allowedScrollTypesMask. Windows platform already implements WheelChanged via PointerWheelChanged. This is a platform parity gap.

### Rationale

The SKTouchAction.WheelChanged enum and WheelDelta property exist in the shared API surface, and Windows correctly delivers these events via PointerWheelChanged. On Mac Catalyst the Apple SKTouchHandler (UIGestureRecognizer) has no scroll wheel handling at all. This is a platform parity bug, not a usage question or missing feature request. The root cause is clearly identified in the source code. Severity is medium because it is isolated to Mac Catalyst scroll wheel input with no data loss or crash.

### Key Signals

- "No WheelChanged events are delivered. Scrolling with mouse wheel or trackpad two-finger scroll over the canvas produces no touch events at all." — **issue body** (The Apple SKTouchHandler's UIGestureRecognizer subclass cannot intercept scroll events — UIKit requires a separate mechanism (UIScrollView integration, ScrollWheel override, or UIPanGestureRecognizer with allowedScrollTypesMask).)
- "#3523 (Mouse hover events also not delivered on Mac Catalyst)" — **issue body** (Part of a broader pattern: multiple pointer input types (hover, scroll wheel) are missing from the Mac Catalyst touch handler.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs` | 54-111 | direct | SKTouchHandler on Apple only overrides TouchesBegan, TouchesMoved, TouchesEnded, and TouchesCancelled. There is no ScrollWheel override or UIPanGestureRecognizer for scroll input, confirming scroll wheel events are never captured. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Windows/SKTouchHandler.cs` | 35-44 | related | Windows handler registers PointerWheelChanged event and maps it to SKTouchAction.WheelChanged with wheelDelta from MouseWheelDelta property. Demonstrates the expected behavior that is missing on Apple. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SKTouchEventArgs.cs` | 60-70 | context | SKTouchAction.WheelChanged enum value and WheelDelta property on SKTouchEventArgs are fully defined and present — the contract exists but the Mac Catalyst handler does not implement it. |

### Workarounds

- There is no clean SkiaSharp-level workaround. Developers can add their own UIPanGestureRecognizer with allowedScrollTypesMask on the native view, but this requires platform-specific code outside of SkiaSharp.
- For Mac Catalyst 15.0+, use UIPanGestureRecognizer with allowedScrollTypesMask = .all to capture trackpad scroll gestures.

### Next Questions

- Should the fix target UIGestureRecognizer.ScrollWheel override or UIPanGestureRecognizer with allowedScrollTypesMask?
- Does the same issue affect SKGLView on Mac Catalyst?
- Is there a related parent issue (#3533) tracking all platforms for wheel support?

### Resolution Proposals

**Hypothesis:** The Apple SKTouchHandler needs to either override ScrollWheel on the underlying UIView or add a separate UIPanGestureRecognizer configured to receive scroll events, then translate the scroll delta into SKTouchAction.WheelChanged events.

1. **Add UIPanGestureRecognizer with allowedScrollTypesMask** — fix, confidence 0.80 (80%), cost/m, validated=untested
   - Add a UIPanGestureRecognizer with allowedScrollTypesMask = .all to the Apple SKTouchHandler when enableTouchEvents is true. Translate pan gesture state and velocity/translation into WheelChanged events. Requires Mac Catalyst 15.0+.
2. **Override ScrollWheel on native UIView** — fix, confidence 0.75 (75%), cost/m, validated=untested
   - Override the ScrollWheel method on the native UIView subclass used by SKCanvasView on Mac Catalyst. Extract scroll delta from the UIEvent and fire SKTouchAction.WheelChanged.

**Recommended proposal:** Add UIPanGestureRecognizer with allowedScrollTypesMask

**Why:** Aligns with the approach described in related issue #3523 for hover events (UIHoverGestureRecognizer), keeps all input handling in the gesture recognizer layer, and is consistent with the existing SKTouchHandler architecture. This approach is actively being used in the related implementation work for scroll wheel support.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Root cause is clearly identified in source code (missing scroll wheel handling in Apple SKTouchHandler). A complete reproduction scenario is provided. Implementation requires testing on Mac Catalyst which requires macOS hardware. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, MAUI views, macOS, compatibility tenet, and partner/maui labels | labels=type/bug, area/SkiaSharp.Views.Maui, os/macOS, tenet/compatibility, partner/maui |
| link-related | low | 0.90 (90%) | Cross-reference related Mac Catalyst hover issue #3523 | linkedIssue=#3523 |
| add-comment | medium | 0.88 (88%) | Acknowledge the bug with root cause summary | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and root cause analysis. This is confirmed: the Apple platform `SKTouchHandler` is a `UIGestureRecognizer` subclass that only overrides `TouchesBegan`/`TouchesMoved`/`TouchesEnded`/`TouchesCancelled` — it has no scroll wheel handling at all for Mac Catalyst. The `SKTouchAction.WheelChanged` API exists and works on Windows (via `PointerWheelChanged`), but the Apple side lacks the equivalent implementation.

This is related to #3523 (mouse hover not delivered on Mac Catalyst) — both stem from the Apple `SKTouchHandler` needing Mac Catalyst-specific input handling.

The fix would involve adding a `UIPanGestureRecognizer` with `allowedScrollTypesMask = .all` (Mac Catalyst 15.0+) or overriding `ScrollWheel` on the native view.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3524,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-05T23:45:00Z"
  },
  "summary": "On Mac Catalyst, SKCanvasView with EnableTouchEvents=true never fires Touch events with SKTouchAction.WheelChanged when the user scrolls with a mouse wheel or two-finger trackpad gesture, because the Apple platform SKTouchHandler (UIGestureRecognizer subclass) has no scroll wheel handling.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.95
    },
    "platforms": [
      "os/macOS"
    ],
    "tenets": [
      "tenet/compatibility"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "missing-output",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net9.0-maccatalyst"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a MAUI app targeting net9.0-maccatalyst",
        "Add an SKCanvasView with EnableTouchEvents=True",
        "Subscribe to the Touch event and log all events including ActionType and WheelDelta",
        "Run on macOS",
        "Scroll with mouse wheel or two-finger trackpad gesture over the canvas",
        "Observe: no WheelChanged events are logged, WheelDelta is always 0"
      ],
      "environmentDetails": "SkiaSharp 3.119.1, SkiaSharp.Views.Maui.Controls 3.119.1, .NET 9.0, macOS (Mac Catalyst)",
      "relatedIssues": [
        3523,
        3533,
        3534
      ],
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.119.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The Apple SKTouchHandler still lacks scroll wheel handling as confirmed by reading the source code."
    }
  },
  "analysis": {
    "summary": "The Apple platform SKTouchHandler (source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs) is a UIGestureRecognizer subclass that overrides only TouchesBegan, TouchesMoved, TouchesEnded, and TouchesCancelled. It has no mechanism for capturing scroll wheel or trackpad scroll input on Mac Catalyst. On Mac Catalyst, scroll wheel events require either overriding ScrollWheel on the UIView or adding a UIPanGestureRecognizer with allowedScrollTypesMask. Windows platform already implements WheelChanged via PointerWheelChanged. This is a platform parity gap.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs",
        "lines": "54-111",
        "finding": "SKTouchHandler on Apple only overrides TouchesBegan, TouchesMoved, TouchesEnded, and TouchesCancelled. There is no ScrollWheel override or UIPanGestureRecognizer for scroll input, confirming scroll wheel events are never captured.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Windows/SKTouchHandler.cs",
        "lines": "35-44",
        "finding": "Windows handler registers PointerWheelChanged event and maps it to SKTouchAction.WheelChanged with wheelDelta from MouseWheelDelta property. Demonstrates the expected behavior that is missing on Apple.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SKTouchEventArgs.cs",
        "lines": "60-70",
        "finding": "SKTouchAction.WheelChanged enum value and WheelDelta property on SKTouchEventArgs are fully defined and present — the contract exists but the Mac Catalyst handler does not implement it.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "No WheelChanged events are delivered. Scrolling with mouse wheel or trackpad two-finger scroll over the canvas produces no touch events at all.",
        "source": "issue body",
        "interpretation": "The Apple SKTouchHandler's UIGestureRecognizer subclass cannot intercept scroll events — UIKit requires a separate mechanism (UIScrollView integration, ScrollWheel override, or UIPanGestureRecognizer with allowedScrollTypesMask)."
      },
      {
        "text": "#3523 (Mouse hover events also not delivered on Mac Catalyst)",
        "source": "issue body",
        "interpretation": "Part of a broader pattern: multiple pointer input types (hover, scroll wheel) are missing from the Mac Catalyst touch handler."
      }
    ],
    "rationale": "The SKTouchAction.WheelChanged enum and WheelDelta property exist in the shared API surface, and Windows correctly delivers these events via PointerWheelChanged. On Mac Catalyst the Apple SKTouchHandler (UIGestureRecognizer) has no scroll wheel handling at all. This is a platform parity bug, not a usage question or missing feature request. The root cause is clearly identified in the source code. Severity is medium because it is isolated to Mac Catalyst scroll wheel input with no data loss or crash.",
    "workarounds": [
      "There is no clean SkiaSharp-level workaround. Developers can add their own UIPanGestureRecognizer with allowedScrollTypesMask on the native view, but this requires platform-specific code outside of SkiaSharp.",
      "For Mac Catalyst 15.0+, use UIPanGestureRecognizer with allowedScrollTypesMask = .all to capture trackpad scroll gestures."
    ],
    "nextQuestions": [
      "Should the fix target UIGestureRecognizer.ScrollWheel override or UIPanGestureRecognizer with allowedScrollTypesMask?",
      "Does the same issue affect SKGLView on Mac Catalyst?",
      "Is there a related parent issue (#3533) tracking all platforms for wheel support?"
    ],
    "resolution": {
      "hypothesis": "The Apple SKTouchHandler needs to either override ScrollWheel on the underlying UIView or add a separate UIPanGestureRecognizer configured to receive scroll events, then translate the scroll delta into SKTouchAction.WheelChanged events.",
      "proposals": [
        {
          "title": "Add UIPanGestureRecognizer with allowedScrollTypesMask",
          "description": "Add a UIPanGestureRecognizer with allowedScrollTypesMask = .all to the Apple SKTouchHandler when enableTouchEvents is true. Translate pan gesture state and velocity/translation into WheelChanged events. Requires Mac Catalyst 15.0+.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Override ScrollWheel on native UIView",
          "description": "Override the ScrollWheel method on the native UIView subclass used by SKCanvasView on Mac Catalyst. Extract scroll delta from the UIEvent and fire SKTouchAction.WheelChanged.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add UIPanGestureRecognizer with allowedScrollTypesMask",
      "recommendedReason": "Aligns with the approach described in related issue #3523 for hover events (UIHoverGestureRecognizer), keeps all input handling in the gesture recognizer layer, and is consistent with the existing SKTouchHandler architecture. This approach is actively being used in the related implementation work for scroll wheel support."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Root cause is clearly identified in source code (missing scroll wheel handling in Apple SKTouchHandler). A complete reproduction scenario is provided. Implementation requires testing on Mac Catalyst which requires macOS hardware.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, MAUI views, macOS, compatibility tenet, and partner/maui labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Maui",
          "os/macOS",
          "tenet/compatibility",
          "partner/maui"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference related Mac Catalyst hover issue #3523",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 3523
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the bug with root cause summary",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report and root cause analysis. This is confirmed: the Apple platform `SKTouchHandler` is a `UIGestureRecognizer` subclass that only overrides `TouchesBegan`/`TouchesMoved`/`TouchesEnded`/`TouchesCancelled` — it has no scroll wheel handling at all for Mac Catalyst. The `SKTouchAction.WheelChanged` API exists and works on Windows (via `PointerWheelChanged`), but the Apple side lacks the equivalent implementation.\n\nThis is related to #3523 (mouse hover not delivered on Mac Catalyst) — both stem from the Apple `SKTouchHandler` needing Mac Catalyst-specific input handling.\n\nThe fix would involve adding a `UIPanGestureRecognizer` with `allowedScrollTypesMask = .all` (Mac Catalyst 15.0+) or overriding `ScrollWheel` on the native view."
      }
    ]
  }
}
```

</details>
