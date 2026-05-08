# Issue Triage Report — #826

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T05:57:00Z |
| Type | type/feature-request (0.92 (92%)) |
| Area | area/SkiaSharp.Views.Maui (0.80 (80%)) |
| Suggested action | keep-open (0.85 (85%)) |

**Issue Summary:** Feature request to support macOS trackpad multitouch (pinch, two-finger pan) gestures in SkiaSharp touch event handling on Xamarin.Forms/MAUI macOS.

**Analysis:** The reporter requests that macOS trackpad multitouch gestures (pinch/magnify, two-finger pan/scroll) be mapped to SKTouch events, similar to how iOS/Android handle UITouch events. The old Xamarin.Forms Mac SKTouchHandler used NSGestureRecognizer but did not implement magnify or scroll wheel overrides. Xamarin.Forms is now deprecated; the MAUI replacement uses UIKit's UIGestureRecognizer (Apple platform handler) which handles UITouch events but does not capture NSEvent-based trackpad gestures on macOS. The feature gap persists in MAUI.

**Recommendations:** **keep-open** — Valid, acknowledged feature request. Xamarin.Forms is deprecated; the equivalent gap exists in MAUI's Apple touch handler which lacks NSEvent/AppKit trackpad multitouch support. Maintainer previously flagged for future implementation.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/macOS |
| Backends | — |
| Tenets | — |
| Partner | partner/maui |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/pull/309 — PR that added touch event support to SkiaSharp (referenced in issue)

**Code snippets:**

```csharp
override void Magnify(NSEvent magnifyEvent) — proposed by reporter as implementation path for macOS pinch gesture support
```

## Analysis

### Technical Summary

The reporter requests that macOS trackpad multitouch gestures (pinch/magnify, two-finger pan/scroll) be mapped to SKTouch events, similar to how iOS/Android handle UITouch events. The old Xamarin.Forms Mac SKTouchHandler used NSGestureRecognizer but did not implement magnify or scroll wheel overrides. Xamarin.Forms is now deprecated; the MAUI replacement uses UIKit's UIGestureRecognizer (Apple platform handler) which handles UITouch events but does not capture NSEvent-based trackpad gestures on macOS. The feature gap persists in MAUI.

### Rationale

This is a feature request: macOS trackpad multitouch gestures (pinch, two-finger pan) are not currently supported in the SkiaSharp touch event pipeline. The original Xamarin.Forms Mac handler used NSGestureRecognizer but never implemented trackpad event overrides. In the current MAUI implementation, a shared UIKit-based handler covers iOS and Mac Catalyst, with no NSEvent/AppKit path for trackpad multitouch. The area is updated to SkiaSharp.Views.Maui since Xamarin.Forms is deprecated.

### Key Signals

- "I tried a pinch and two-finger pan gesture with the trackpad but `OnTouch(SKTouchEventArgs e)` doesn't get called." — **issue body** (Confirms that macOS trackpad multitouch does not fire SKTouchEventArgs — the touch handler does not capture NSEvent-based trackpad input.)
- "SKTouchHandler derives from NSGestureRecognizer so maybe we could add override void Magnify(NSEvent magnifyEvent)" — **issue body** (Reporter suggests a concrete implementation path using NSGestureRecognizer overrides for macOS AppKit.)
- "Tentatively moving this to the next version of SkiaSharp." — **comment by mattleibow (2020-04-26)** (Maintainer acknowledged and deferred the feature, indicating it is a legitimate request.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs` | 1-112 | direct | The Apple-platform SKTouchHandler derives from UIGestureRecognizer and handles TouchesBegan/Moved/Ended/Cancelled using UITouch objects. There is no implementation of NSEvent-based overrides (Magnify, ScrollWheel) for macOS trackpad multitouch. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandlerProxy.cs` | 1-55 | direct | The SKTouchHandlerProxy wires up the SKTouchHandler to the platform UIView for both iOS and Mac Catalyst. There is no separate macOS-native (AppKit/NSGestureRecognizer) handler for trackpad events. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Apple.cs` | 1-89 | related | The Apple SKCanvasViewHandler uses the shared Apple SKTouchHandlerProxy, confirming there is no platform split for macOS multitouch. Both iOS and macOS (Mac Catalyst) use the same UIKit-based handler. |

### Resolution Proposals

1. **Add macOS trackpad multitouch support to MAUI Apple touch handler** — fix, cost/m, validated=untested
   - Create a macOS-specific touch handler (or extend the existing Apple handler with conditional NSGestureRecognizer overrides for Magnify and ScrollWheel) so that macOS trackpad multitouch events fire SKTouchEventArgs. This would require adding AppKit-specific code paths and possibly a separate SKTouchHandler for macOS.

**Recommended proposal:** Add macOS trackpad multitouch support to MAUI Apple touch handler

**Why:** Extends parity with iOS/Android touch support and aligns with the maintainer's earlier acknowledgement to implement this in a future release.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.85 (85%) |
| Reason | Valid, acknowledged feature request. Xamarin.Forms is deprecated; the equivalent gap exists in MAUI's Apple touch handler which lacks NSEvent/AppKit trackpad multitouch support. Maintainer previously flagged for future implementation. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply type/feature-request, area/SkiaSharp.Views.Maui, os/macOS, partner/maui labels | labels=type/feature-request, area/SkiaSharp.Views.Maui, os/macOS, partner/maui |
| add-comment | medium | 0.80 (80%) | Acknowledge the feature request and note context migration from Xamarin.Forms to MAUI | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the feature request! macOS trackpad multitouch (pinch, two-finger pan) is currently not handled by SkiaSharp's touch event pipeline. The original Xamarin.Forms Mac handler used `NSGestureRecognizer` but did not implement the `Magnify` or `ScrollWheel` overrides.

With the migration to .NET MAUI, the equivalent gap now lies in `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs`, which uses UIKit's `UIGestureRecognizer` and handles standard `UITouch` events only. Adding macOS trackpad multitouch would require a platform-specific handler using AppKit's `NSGestureRecognizer` (or NSEvent overrides) to forward `Magnify` and `ScrollWheel` events as `SKTouchEventArgs`.

This remains a valid enhancement for a future release.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 826,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T05:57:00Z"
  },
  "summary": "Feature request to support macOS trackpad multitouch (pinch, two-finger pan) gestures in SkiaSharp touch event handling on Xamarin.Forms/MAUI macOS.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.8
    },
    "platforms": [
      "os/macOS"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/309",
          "description": "PR that added touch event support to SkiaSharp (referenced in issue)"
        }
      ],
      "codeSnippets": [
        "override void Magnify(NSEvent magnifyEvent) — proposed by reporter as implementation path for macOS pinch gesture support"
      ]
    }
  },
  "analysis": {
    "summary": "The reporter requests that macOS trackpad multitouch gestures (pinch/magnify, two-finger pan/scroll) be mapped to SKTouch events, similar to how iOS/Android handle UITouch events. The old Xamarin.Forms Mac SKTouchHandler used NSGestureRecognizer but did not implement magnify or scroll wheel overrides. Xamarin.Forms is now deprecated; the MAUI replacement uses UIKit's UIGestureRecognizer (Apple platform handler) which handles UITouch events but does not capture NSEvent-based trackpad gestures on macOS. The feature gap persists in MAUI.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs",
        "finding": "The Apple-platform SKTouchHandler derives from UIGestureRecognizer and handles TouchesBegan/Moved/Ended/Cancelled using UITouch objects. There is no implementation of NSEvent-based overrides (Magnify, ScrollWheel) for macOS trackpad multitouch.",
        "relevance": "direct",
        "lines": "1-112"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandlerProxy.cs",
        "finding": "The SKTouchHandlerProxy wires up the SKTouchHandler to the platform UIView for both iOS and Mac Catalyst. There is no separate macOS-native (AppKit/NSGestureRecognizer) handler for trackpad events.",
        "relevance": "direct",
        "lines": "1-55"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Apple.cs",
        "finding": "The Apple SKCanvasViewHandler uses the shared Apple SKTouchHandlerProxy, confirming there is no platform split for macOS multitouch. Both iOS and macOS (Mac Catalyst) use the same UIKit-based handler.",
        "relevance": "related",
        "lines": "1-89"
      }
    ],
    "keySignals": [
      {
        "text": "I tried a pinch and two-finger pan gesture with the trackpad but `OnTouch(SKTouchEventArgs e)` doesn't get called.",
        "source": "issue body",
        "interpretation": "Confirms that macOS trackpad multitouch does not fire SKTouchEventArgs — the touch handler does not capture NSEvent-based trackpad input."
      },
      {
        "text": "SKTouchHandler derives from NSGestureRecognizer so maybe we could add override void Magnify(NSEvent magnifyEvent)",
        "source": "issue body",
        "interpretation": "Reporter suggests a concrete implementation path using NSGestureRecognizer overrides for macOS AppKit."
      },
      {
        "text": "Tentatively moving this to the next version of SkiaSharp.",
        "source": "comment by mattleibow (2020-04-26)",
        "interpretation": "Maintainer acknowledged and deferred the feature, indicating it is a legitimate request."
      }
    ],
    "rationale": "This is a feature request: macOS trackpad multitouch gestures (pinch, two-finger pan) are not currently supported in the SkiaSharp touch event pipeline. The original Xamarin.Forms Mac handler used NSGestureRecognizer but never implemented trackpad event overrides. In the current MAUI implementation, a shared UIKit-based handler covers iOS and Mac Catalyst, with no NSEvent/AppKit path for trackpad multitouch. The area is updated to SkiaSharp.Views.Maui since Xamarin.Forms is deprecated.",
    "resolution": {
      "proposals": [
        {
          "title": "Add macOS trackpad multitouch support to MAUI Apple touch handler",
          "description": "Create a macOS-specific touch handler (or extend the existing Apple handler with conditional NSGestureRecognizer overrides for Magnify and ScrollWheel) so that macOS trackpad multitouch events fire SKTouchEventArgs. This would require adding AppKit-specific code paths and possibly a separate SKTouchHandler for macOS.",
          "category": "fix",
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add macOS trackpad multitouch support to MAUI Apple touch handler",
      "recommendedReason": "Extends parity with iOS/Android touch support and aligns with the maintainer's earlier acknowledgement to implement this in a future release."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.85,
      "reason": "Valid, acknowledged feature request. Xamarin.Forms is deprecated; the equivalent gap exists in MAUI's Apple touch handler which lacks NSEvent/AppKit trackpad multitouch support. Maintainer previously flagged for future implementation.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/feature-request, area/SkiaSharp.Views.Maui, os/macOS, partner/maui labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp.Views.Maui",
          "os/macOS",
          "partner/maui"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the feature request and note context migration from Xamarin.Forms to MAUI",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thank you for the feature request! macOS trackpad multitouch (pinch, two-finger pan) is currently not handled by SkiaSharp's touch event pipeline. The original Xamarin.Forms Mac handler used `NSGestureRecognizer` but did not implement the `Magnify` or `ScrollWheel` overrides.\n\nWith the migration to .NET MAUI, the equivalent gap now lies in `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs`, which uses UIKit's `UIGestureRecognizer` and handles standard `UITouch` events only. Adding macOS trackpad multitouch would require a platform-specific handler using AppKit's `NSGestureRecognizer` (or NSEvent overrides) to forward `Magnify` and `ScrollWheel` events as `SKTouchEventArgs`.\n\nThis remains a valid enhancement for a future release."
      }
    ]
  }
}
```

</details>
