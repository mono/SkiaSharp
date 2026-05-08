# Issue Triage Report — #2449

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T05:45:00Z |
| Type | type/enhancement (0.88 (88%)) |
| Area | area/SkiaSharp.Views (0.85 (85%)) |
| Suggested action | keep-open (0.85 (85%)) |

**Issue Summary:** Feature request to add mouse scroll-wheel support (SKTouchAction.WheelChanged) to the Mac platform SKTouchHandler, which currently does not fire wheel events unlike the Windows/UWP handler.

**Analysis:** The Apple platform SKTouchHandler (UIKit/UIGestureRecognizer) handles only UITouch events and has no scroll wheel handling. The Windows MAUI handler already implements WheelChanged via PointerWheelChanged. On macOS, scroll wheel events come via NSResponder.scrollWheel (AppKit) or UIPanGestureRecognizer (Mac Catalyst). Related tracking issues #3533 and #3536 have been created to address this gap in MAUI.

**Recommendations:** **keep-open** — Valid enhancement request with related tracking issues (#3533, #3536). The fix requires macOS platform-specific implementation that is actively being tracked.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.Views |
| Platforms | os/macOS |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Environment:** Xamarin.Forms Mac (SkiaSharp.Views.Forms.Mac), applicable to MAUI macOS as well

**Related issues:** #3536, #3533, #3524

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3536 — Sub-issue: Add wheel support to macOS MAUI views with v120 normalization
- https://github.com/mono/SkiaSharp/issues/3533 — Parent: Standardize WheelDelta to v120 convention across all platforms
- https://github.com/mono/SkiaSharp/issues/3524 — Related: Mac Catalyst mouse wheel/trackpad scroll events not delivered

## Analysis

### Technical Summary

The Apple platform SKTouchHandler (UIKit/UIGestureRecognizer) handles only UITouch events and has no scroll wheel handling. The Windows MAUI handler already implements WheelChanged via PointerWheelChanged. On macOS, scroll wheel events come via NSResponder.scrollWheel (AppKit) or UIPanGestureRecognizer (Mac Catalyst). Related tracking issues #3533 and #3536 have been created to address this gap in MAUI.

### Rationale

This is a platform parity enhancement request — the scroll wheel event infrastructure (SKTouchAction.WheelChanged, WheelDelta on SKTouchEventArgs) already exists and works on Windows, but the Apple/macOS platform handler does not implement it. The reporter clearly frames it as a feature request ('Can support be added'). The issue context has evolved from Xamarin.Forms.Mac to MAUI, with dedicated tracking issues #3533 (WheelDelta standardization) and #3536 (macOS MAUI implementation) already filed.

### Key Signals

- "the mouse wheel handling is missing on the Mac platform in SkTouchHandler.cs" — **issue body** (Platform parity gap — Windows/UWP has scroll wheel support via PointerWheelChanged but the Apple/Mac handler does not.)
- "as a work-around we have been able to add a custom renderer for Mac, and overriding the NSResponder ScrollWheel event" — **issue body** (Workaround exists (custom renderer overriding NSResponder.scrollWheel) but requires extra boilerplate per application.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs` | — | direct | The Apple SKTouchHandler (UIGestureRecognizer) handles only TouchesBegan, TouchesMoved, TouchesEnded, TouchesCancelled — no ScrollWheel override, no SKTouchAction.WheelChanged is ever fired. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Windows/SKTouchHandler.cs` | — | related | The Windows handler subscribes to PointerWheelChanged and calls CommonHandler with SKTouchAction.WheelChanged, passing pointerPoint.Properties.MouseWheelDelta. This is the reference implementation for scroll wheel support that macOS lacks. |

### Workarounds

- Create a custom renderer for Mac that overrides NSResponder.scrollWheel and fires an SKTouchEvent with SKTouchAction.WheelChanged and the scroll delta.

### Resolution Proposals

**Hypothesis:** The Apple platform SKTouchHandler needs a scrollWheel override (macOS AppKit using NSEvent.scrollingDeltaY) or UIPanGestureRecognizer scroll support (Mac Catalyst) to forward wheel events as SKTouchAction.WheelChanged.

1. **Override scrollWheel in macOS MAUI handler** — fix, confidence 0.80 (80%), cost/m, validated=untested
   - Add a scrollWheel(with:) override to the Apple SKTouchHandler for macOS, normalizing NSEvent.scrollingDeltaY to v120 units as described in issue #3536 and the broader #3533 tracking issue.
2. **Custom renderer workaround (existing)** — workaround, confidence 0.90 (90%), cost/s, validated=untested
   - Override NSResponder.scrollWheel in a custom Mac renderer and convert NSEvent.scrollingDeltaY to an SKTouchEventArgs with SKTouchAction.WheelChanged.

**Recommended proposal:** Override scrollWheel in macOS MAUI handler

**Why:** The proper fix is tracked in #3536 as part of the broader #3533 WheelDelta standardization effort across all platforms.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.85 (85%) |
| Reason | Valid enhancement request with related tracking issues (#3533, #3536). The fix requires macOS platform-specific implementation that is actively being tracked. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply enhancement, views area, macOS platform, and compatibility tenet labels | labels=type/enhancement, area/SkiaSharp.Views, os/macOS, tenet/compatibility |
| link-related | low | 0.90 (90%) | Link to parent tracking issue #3533 (WheelDelta standardization) | linkedIssue=#3533 |
| link-related | low | 0.90 (90%) | Link to MAUI macOS wheel sub-issue #3536 | linkedIssue=#3536 |
| add-comment | medium | 0.85 (85%) | Comment confirming the gap, linking to tracking issues, and noting workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for filing this! The scroll wheel gap on macOS is confirmed — the Apple platform `SKTouchHandler` only handles `UITouch` events and does not implement `ScrollWheel` / `WheelChanged`.

This is now being tracked as part of a broader effort to standardize wheel delta support across all platforms:
- **#3533** — Standardize `WheelDelta` to v120 convention across all platforms
- **#3536** — Add wheel support to macOS MAUI views with v120 normalization

In the meantime, the workaround you mentioned (custom renderer overriding `NSResponder.scrollWheel`) remains the best approach for Xamarin.Forms.Mac. For MAUI macOS, the same pattern applies — override `scrollWheel(with:)` in a custom platform handler and convert `NSEvent.scrollingDeltaY` to an `SKTouchEventArgs` with `SKTouchAction.WheelChanged`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2449,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T05:45:00Z"
  },
  "summary": "Feature request to add mouse scroll-wheel support (SKTouchAction.WheelChanged) to the Mac platform SKTouchHandler, which currently does not fire wheel events unlike the Windows/UWP handler.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.85
    },
    "platforms": [
      "os/macOS"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Xamarin.Forms Mac (SkiaSharp.Views.Forms.Mac), applicable to MAUI macOS as well",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3536",
          "description": "Sub-issue: Add wheel support to macOS MAUI views with v120 normalization"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3533",
          "description": "Parent: Standardize WheelDelta to v120 convention across all platforms"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3524",
          "description": "Related: Mac Catalyst mouse wheel/trackpad scroll events not delivered"
        }
      ],
      "relatedIssues": [
        3536,
        3533,
        3524
      ]
    }
  },
  "analysis": {
    "summary": "The Apple platform SKTouchHandler (UIKit/UIGestureRecognizer) handles only UITouch events and has no scroll wheel handling. The Windows MAUI handler already implements WheelChanged via PointerWheelChanged. On macOS, scroll wheel events come via NSResponder.scrollWheel (AppKit) or UIPanGestureRecognizer (Mac Catalyst). Related tracking issues #3533 and #3536 have been created to address this gap in MAUI.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs",
        "finding": "The Apple SKTouchHandler (UIGestureRecognizer) handles only TouchesBegan, TouchesMoved, TouchesEnded, TouchesCancelled — no ScrollWheel override, no SKTouchAction.WheelChanged is ever fired.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Windows/SKTouchHandler.cs",
        "finding": "The Windows handler subscribes to PointerWheelChanged and calls CommonHandler with SKTouchAction.WheelChanged, passing pointerPoint.Properties.MouseWheelDelta. This is the reference implementation for scroll wheel support that macOS lacks.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "the mouse wheel handling is missing on the Mac platform in SkTouchHandler.cs",
        "source": "issue body",
        "interpretation": "Platform parity gap — Windows/UWP has scroll wheel support via PointerWheelChanged but the Apple/Mac handler does not."
      },
      {
        "text": "as a work-around we have been able to add a custom renderer for Mac, and overriding the NSResponder ScrollWheel event",
        "source": "issue body",
        "interpretation": "Workaround exists (custom renderer overriding NSResponder.scrollWheel) but requires extra boilerplate per application."
      }
    ],
    "rationale": "This is a platform parity enhancement request — the scroll wheel event infrastructure (SKTouchAction.WheelChanged, WheelDelta on SKTouchEventArgs) already exists and works on Windows, but the Apple/macOS platform handler does not implement it. The reporter clearly frames it as a feature request ('Can support be added'). The issue context has evolved from Xamarin.Forms.Mac to MAUI, with dedicated tracking issues #3533 (WheelDelta standardization) and #3536 (macOS MAUI implementation) already filed.",
    "workarounds": [
      "Create a custom renderer for Mac that overrides NSResponder.scrollWheel and fires an SKTouchEvent with SKTouchAction.WheelChanged and the scroll delta."
    ],
    "resolution": {
      "hypothesis": "The Apple platform SKTouchHandler needs a scrollWheel override (macOS AppKit using NSEvent.scrollingDeltaY) or UIPanGestureRecognizer scroll support (Mac Catalyst) to forward wheel events as SKTouchAction.WheelChanged.",
      "proposals": [
        {
          "title": "Override scrollWheel in macOS MAUI handler",
          "description": "Add a scrollWheel(with:) override to the Apple SKTouchHandler for macOS, normalizing NSEvent.scrollingDeltaY to v120 units as described in issue #3536 and the broader #3533 tracking issue.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Custom renderer workaround (existing)",
          "description": "Override NSResponder.scrollWheel in a custom Mac renderer and convert NSEvent.scrollingDeltaY to an SKTouchEventArgs with SKTouchAction.WheelChanged.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Override scrollWheel in macOS MAUI handler",
      "recommendedReason": "The proper fix is tracked in #3536 as part of the broader #3533 WheelDelta standardization effort across all platforms."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.85,
      "reason": "Valid enhancement request with related tracking issues (#3533, #3536). The fix requires macOS platform-specific implementation that is actively being tracked.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement, views area, macOS platform, and compatibility tenet labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp.Views",
          "os/macOS",
          "tenet/compatibility"
        ]
      },
      {
        "type": "link-related",
        "description": "Link to parent tracking issue #3533 (WheelDelta standardization)",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 3533
      },
      {
        "type": "link-related",
        "description": "Link to MAUI macOS wheel sub-issue #3536",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 3536
      },
      {
        "type": "add-comment",
        "description": "Comment confirming the gap, linking to tracking issues, and noting workaround",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for filing this! The scroll wheel gap on macOS is confirmed — the Apple platform `SKTouchHandler` only handles `UITouch` events and does not implement `ScrollWheel` / `WheelChanged`.\n\nThis is now being tracked as part of a broader effort to standardize wheel delta support across all platforms:\n- **#3533** — Standardize `WheelDelta` to v120 convention across all platforms\n- **#3536** — Add wheel support to macOS MAUI views with v120 normalization\n\nIn the meantime, the workaround you mentioned (custom renderer overriding `NSResponder.scrollWheel`) remains the best approach for Xamarin.Forms.Mac. For MAUI macOS, the same pattern applies — override `scrollWheel(with:)` in a custom platform handler and convert `NSEvent.scrollingDeltaY` to an `SKTouchEventArgs` with `SKTouchAction.WheelChanged`."
      }
    ]
  }
}
```

</details>
