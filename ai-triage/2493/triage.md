# Issue Triage Report — #2493

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T16:57:53Z |
| Type | type/bug (0.93 (93%)) |
| Area | area/SkiaSharp.Views.Maui (0.95 (95%)) |
| Suggested action | needs-investigation (0.85 (85%)) |

**Issue Summary:** On iOS MAUI, no SKTouchAction.Cancelled event fires when a FlyOut navigation element opens and interrupts an active touch, unlike Android which correctly fires Cancel; a commenter further reports that only Pressed events fire on iOS at all.

**Analysis:** The iOS SKTouchHandler extends UIGestureRecognizer and implements TouchesCancelled to fire SKTouchAction.Cancelled, but when a MAUI FlyOut opens it intercepts touches at the MAUI/native layer without triggering UIKit's standard gesture-recognizer cancellation path. Additionally, when the touch handler's FireEvent returns false (touch not marked as Handled), IgnoreTouch is called on the first touch, causing UIKit to stop delivering all subsequent touch phases (Moved, Released, Cancelled) to that recognizer — explaining the commenter's observation that only Pressed fires.

**Recommendations:** **needs-investigation** — Repro sample available, code investigation identifies likely root cause (IgnoreTouch on unhandled touch), but physical iOS device needed to verify and confirm whether FlyOut scenario is separately broken.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/iOS |
| Backends | — |
| Tenets | tenet/compatibility, tenet/reliability |
| Partner | partner/maui |

## Evidence

### Reproduction

1. Create a MAUI app with SKCanvasView in a FlyoutPage or Shell with FlyOut enabled
2. Press and hold on the SKCanvasView to start tracking touch
3. Open the FlyOut (e.g., swipe from edge or tap hamburger)
4. Observe: Android fires SKTouchAction.Cancelled; iOS fires nothing

**Environment:** SkiaSharp 2.88.3, MAUI iOS (maui-ios 7.0.86/7.0.100), iPhone Pro Max 14 (physical device)

**Related issues:** #2722

**Repository links:**
- https://github.com/nm4568/SkiaSharpSamples — Minimal repro MAUI sample by commenter nm4568; also reproduces broader issue of only Pressed firing on iOS

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | platform-specific |
| Error message | No SKTouchAction.Cancelled event fired on iOS when MAUI FlyOut opens |
| Repro quality | partial |
| Target frameworks | net7.0-ios, net8.0-ios |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The iOS SKTouchHandler implementation using UIGestureRecognizer has not changed in a way that would address FlyOut-triggered cancellation since 2.88.x. |

## Analysis

### Technical Summary

The iOS SKTouchHandler extends UIGestureRecognizer and implements TouchesCancelled to fire SKTouchAction.Cancelled, but when a MAUI FlyOut opens it intercepts touches at the MAUI/native layer without triggering UIKit's standard gesture-recognizer cancellation path. Additionally, when the touch handler's FireEvent returns false (touch not marked as Handled), IgnoreTouch is called on the first touch, causing UIKit to stop delivering all subsequent touch phases (Moved, Released, Cancelled) to that recognizer — explaining the commenter's observation that only Pressed fires.

### Rationale

Clear platform parity bug: Android's View.Touch event delivers MotionEventActions.Cancel which maps to SKTouchAction.Cancelled, but the iOS UIGestureRecognizer-based approach does not receive UIKit's TouchesCancelled when a MAUI FlyOut (which operates at MAUI's navigation layer) interrupts the touch. The secondary root cause — IgnoreTouch being called when events are not marked Handled — may be the dominant code path causing both the missing Cancelled and the broader 'only Pressed fires' symptom reported in #2722.

### Key Signals

- "On Android an SKTouchAction.Cancelled is called, but on iOS no event at all" — **issue body** (Cross-platform parity gap. Android touch cancel path works; iOS does not deliver cancellation when FlyOut intercepts.)
- "Only the SKTouchAction.Pressed event is firing on iOS" — **comment by nm4568** (IgnoreTouch called on first touch when Handled=false, causing UIKit to stop delivering subsequent events (Moved, Released, Cancelled) to the gesture recognizer.)
- "We need to track those to know how many fingers are pressed. If an event is missing there is no way to correct for that." — **issue body** (Practical impact: touch-count state corrupted in apps like Mapsui that track multi-finger interactions.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs` | 87-95 | direct | TouchesCancelled is implemented and calls FireEvent(SKTouchAction.Cancelled, ...) — the handler does support cancellation at the UIKit level, but only if UIKit delivers the TouchesCancelled call. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs` | 54-64 | direct | In TouchesBegan, if FireEvent returns false (SKTouchEventArgs.Handled == false), IgnoreTouch(touch, evt) is called. UIKit then stops delivering Moved, Ended, and Cancelled for that touch to this gesture recognizer — explaining why only Pressed fires when the user does not set e.Handled = true. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs` | 100-108 | related | Android OnTouch handler maps MotionEventActions.Cancel to SKTouchAction.Cancelled correctly. No IgnoreTouch equivalent — all touch events are always delivered, explaining why Cancel works on Android. |

### Workarounds

- Set e.Handled = true in the SKCanvasView Touch event handler to prevent IgnoreTouch from being called. This ensures Moved, Released, and Cancelled are all delivered by UIKit — likely resolves the 'only Pressed fires' variant. The FlyOut cancellation scenario may still not fire if MAUI intercepts before UIKit.
- Implement an app-level cancel guard: subscribe to the MAUI Shell FlyoutIsPresented property change and when it becomes true, manually reset all tracked finger state.

### Next Questions

- Does setting e.Handled = true resolve the missing Cancelled when FlyOut opens, or does MAUI still intercept before UIKit's cancellation path?
- Does the same issue affect SKGLView on iOS?
- Is this also reproducible on Mac Catalyst or tvOS?
- Does the issue exist in the Xamarin.Forms SKTouchHandler for iOS (Views.Forms)?
- Does the FlyOut on iOS use a gesture recognizer that should trigger requiresExclusiveTouchType or shouldRecognizeSimultaneously?

### Resolution Proposals

**Hypothesis:** Two overlapping root causes: (1) IgnoreTouch is called when Handled=false, stopping all subsequent touch delivery; (2) MAUI FlyOut navigation on iOS does not trigger UIKit-level gesture cancellation on the SKTouchHandler recognizer.

1. **Document: set e.Handled = true** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - Explain in docs/comment that the Pressed event's e.Handled must be true for subsequent Moved/Released/Cancelled events to fire on iOS. This is a UIGestureRecognizer constraint.
2. **Remove IgnoreTouch call or make it opt-in** — fix, confidence 0.70 (70%), cost/s, validated=untested
   - In TouchesBegan, remove or gate the IgnoreTouch call so that not-handled touches still receive subsequent phase events. This would match Android behavior where all events are delivered regardless of Handled.
3. **Synthesize Cancelled on FlyOut open** — fix, confidence 0.60 (60%), cost/m, validated=untested
   - In the MAUI layer, listen for FlyoutIsPresented or UIKit's gesture recognizer interruption callback and synthesize a Cancelled event for all active touches.

**Recommended proposal:** Remove IgnoreTouch call or make it opt-in

**Why:** Removing the IgnoreTouch call aligns iOS behavior with Android (where all phases are always delivered) and likely resolves the reported symptom without requiring MAUI-level plumbing.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.85 (85%) |
| Reason | Repro sample available, code investigation identifies likely root cause (IgnoreTouch on unhandled touch), but physical iOS device needed to verify and confirm whether FlyOut scenario is separately broken. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, MAUI views, iOS, compatibility, reliability, MAUI partner labels | labels=type/bug, area/SkiaSharp.Views.Maui, os/iOS, tenet/compatibility, tenet/reliability, partner/maui |
| link-related | low | 0.90 (90%) | Link to #2722 which is the same reporter's broader repro of iOS-only-Pressed-fires | linkedIssue=#2722 |
| add-comment | medium | 0.80 (80%) | Post analysis with workaround (set e.Handled = true) and root cause hypothesis | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report and for providing the repro sample!

After investigating the iOS `SKTouchHandler` code, I found two likely contributors to this issue:

**1. `e.Handled` must be `true` for subsequent events to fire on iOS**

The iOS handler is a `UIGestureRecognizer`. In `TouchesBegan`, if your event handler doesn't set `e.Handled = true`, `IgnoreTouch` is called on that touch. UIKit then stops delivering `Moved`, `Ended`, and `Cancelled` phases to this recognizer. This is likely why @nm4568's repro only sees `Pressed`.

**Workaround:** Set `e.Handled = true` in your Touch handler:
```csharp
private void SkCanvasView_Touch(object? sender, SKTouchEventArgs e)
{
    e.Handled = true; // Required on iOS to receive Moved/Released/Cancelled
    // ... your logic
}
```

**2. FlyOut-triggered cancellation may still be missing**

Even with `e.Handled = true`, when a MAUI FlyOut opens, it intercepts at the MAUI navigation layer without necessarily going through UIKit's standard gesture-recognizer cancellation. As a fallback, you can subscribe to Shell/FlyoutPage state and reset finger tracking manually:
```csharp
// In your page or view model
Shell.Current.PropertyChanged += (s, e) => {
    if (e.PropertyName == nameof(Shell.FlyoutIsPresented) && Shell.Current.FlyoutIsPresented)
        ResetTouchTracking(); // clear your finger count
};
```

Could you confirm whether setting `e.Handled = true` resolves the issue? This will help narrow down which of the two causes dominates.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2493,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T16:57:53Z"
  },
  "summary": "On iOS MAUI, no SKTouchAction.Cancelled event fires when a FlyOut navigation element opens and interrupts an active touch, unlike Android which correctly fires Cancel; a commenter further reports that only Pressed events fire on iOS at all.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.93
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.95
    },
    "platforms": [
      "os/iOS"
    ],
    "tenets": [
      "tenet/compatibility",
      "tenet/reliability"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "platform-specific",
      "errorMessage": "No SKTouchAction.Cancelled event fired on iOS when MAUI FlyOut opens",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net7.0-ios",
        "net8.0-ios"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a MAUI app with SKCanvasView in a FlyoutPage or Shell with FlyOut enabled",
        "Press and hold on the SKCanvasView to start tracking touch",
        "Open the FlyOut (e.g., swipe from edge or tap hamburger)",
        "Observe: Android fires SKTouchAction.Cancelled; iOS fires nothing"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, MAUI iOS (maui-ios 7.0.86/7.0.100), iPhone Pro Max 14 (physical device)",
      "relatedIssues": [
        2722
      ],
      "repoLinks": [
        {
          "url": "https://github.com/nm4568/SkiaSharpSamples",
          "description": "Minimal repro MAUI sample by commenter nm4568; also reproduces broader issue of only Pressed firing on iOS"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The iOS SKTouchHandler implementation using UIGestureRecognizer has not changed in a way that would address FlyOut-triggered cancellation since 2.88.x."
    }
  },
  "analysis": {
    "summary": "The iOS SKTouchHandler extends UIGestureRecognizer and implements TouchesCancelled to fire SKTouchAction.Cancelled, but when a MAUI FlyOut opens it intercepts touches at the MAUI/native layer without triggering UIKit's standard gesture-recognizer cancellation path. Additionally, when the touch handler's FireEvent returns false (touch not marked as Handled), IgnoreTouch is called on the first touch, causing UIKit to stop delivering all subsequent touch phases (Moved, Released, Cancelled) to that recognizer — explaining the commenter's observation that only Pressed fires.",
    "rationale": "Clear platform parity bug: Android's View.Touch event delivers MotionEventActions.Cancel which maps to SKTouchAction.Cancelled, but the iOS UIGestureRecognizer-based approach does not receive UIKit's TouchesCancelled when a MAUI FlyOut (which operates at MAUI's navigation layer) interrupts the touch. The secondary root cause — IgnoreTouch being called when events are not marked Handled — may be the dominant code path causing both the missing Cancelled and the broader 'only Pressed fires' symptom reported in #2722.",
    "keySignals": [
      {
        "text": "On Android an SKTouchAction.Cancelled is called, but on iOS no event at all",
        "source": "issue body",
        "interpretation": "Cross-platform parity gap. Android touch cancel path works; iOS does not deliver cancellation when FlyOut intercepts."
      },
      {
        "text": "Only the SKTouchAction.Pressed event is firing on iOS",
        "source": "comment by nm4568",
        "interpretation": "IgnoreTouch called on first touch when Handled=false, causing UIKit to stop delivering subsequent events (Moved, Released, Cancelled) to the gesture recognizer."
      },
      {
        "text": "We need to track those to know how many fingers are pressed. If an event is missing there is no way to correct for that.",
        "source": "issue body",
        "interpretation": "Practical impact: touch-count state corrupted in apps like Mapsui that track multi-finger interactions."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs",
        "lines": "87-95",
        "finding": "TouchesCancelled is implemented and calls FireEvent(SKTouchAction.Cancelled, ...) — the handler does support cancellation at the UIKit level, but only if UIKit delivers the TouchesCancelled call.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs",
        "lines": "54-64",
        "finding": "In TouchesBegan, if FireEvent returns false (SKTouchEventArgs.Handled == false), IgnoreTouch(touch, evt) is called. UIKit then stops delivering Moved, Ended, and Cancelled for that touch to this gesture recognizer — explaining why only Pressed fires when the user does not set e.Handled = true.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs",
        "lines": "100-108",
        "finding": "Android OnTouch handler maps MotionEventActions.Cancel to SKTouchAction.Cancelled correctly. No IgnoreTouch equivalent — all touch events are always delivered, explaining why Cancel works on Android.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Set e.Handled = true in the SKCanvasView Touch event handler to prevent IgnoreTouch from being called. This ensures Moved, Released, and Cancelled are all delivered by UIKit — likely resolves the 'only Pressed fires' variant. The FlyOut cancellation scenario may still not fire if MAUI intercepts before UIKit.",
      "Implement an app-level cancel guard: subscribe to the MAUI Shell FlyoutIsPresented property change and when it becomes true, manually reset all tracked finger state."
    ],
    "nextQuestions": [
      "Does setting e.Handled = true resolve the missing Cancelled when FlyOut opens, or does MAUI still intercept before UIKit's cancellation path?",
      "Does the same issue affect SKGLView on iOS?",
      "Is this also reproducible on Mac Catalyst or tvOS?",
      "Does the issue exist in the Xamarin.Forms SKTouchHandler for iOS (Views.Forms)?",
      "Does the FlyOut on iOS use a gesture recognizer that should trigger requiresExclusiveTouchType or shouldRecognizeSimultaneously?"
    ],
    "resolution": {
      "hypothesis": "Two overlapping root causes: (1) IgnoreTouch is called when Handled=false, stopping all subsequent touch delivery; (2) MAUI FlyOut navigation on iOS does not trigger UIKit-level gesture cancellation on the SKTouchHandler recognizer.",
      "proposals": [
        {
          "title": "Document: set e.Handled = true",
          "description": "Explain in docs/comment that the Pressed event's e.Handled must be true for subsequent Moved/Released/Cancelled events to fire on iOS. This is a UIGestureRecognizer constraint.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Remove IgnoreTouch call or make it opt-in",
          "description": "In TouchesBegan, remove or gate the IgnoreTouch call so that not-handled touches still receive subsequent phase events. This would match Android behavior where all events are delivered regardless of Handled.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Synthesize Cancelled on FlyOut open",
          "description": "In the MAUI layer, listen for FlyoutIsPresented or UIKit's gesture recognizer interruption callback and synthesize a Cancelled event for all active touches.",
          "category": "fix",
          "confidence": 0.6,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Remove IgnoreTouch call or make it opt-in",
      "recommendedReason": "Removing the IgnoreTouch call aligns iOS behavior with Android (where all phases are always delivered) and likely resolves the reported symptom without requiring MAUI-level plumbing."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "Repro sample available, code investigation identifies likely root cause (IgnoreTouch on unhandled touch), but physical iOS device needed to verify and confirm whether FlyOut scenario is separately broken.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, MAUI views, iOS, compatibility, reliability, MAUI partner labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Maui",
          "os/iOS",
          "tenet/compatibility",
          "tenet/reliability",
          "partner/maui"
        ]
      },
      {
        "type": "link-related",
        "description": "Link to #2722 which is the same reporter's broader repro of iOS-only-Pressed-fires",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 2722
      },
      {
        "type": "add-comment",
        "description": "Post analysis with workaround (set e.Handled = true) and root cause hypothesis",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the report and for providing the repro sample!\n\nAfter investigating the iOS `SKTouchHandler` code, I found two likely contributors to this issue:\n\n**1. `e.Handled` must be `true` for subsequent events to fire on iOS**\n\nThe iOS handler is a `UIGestureRecognizer`. In `TouchesBegan`, if your event handler doesn't set `e.Handled = true`, `IgnoreTouch` is called on that touch. UIKit then stops delivering `Moved`, `Ended`, and `Cancelled` phases to this recognizer. This is likely why @nm4568's repro only sees `Pressed`.\n\n**Workaround:** Set `e.Handled = true` in your Touch handler:\n```csharp\nprivate void SkCanvasView_Touch(object? sender, SKTouchEventArgs e)\n{\n    e.Handled = true; // Required on iOS to receive Moved/Released/Cancelled\n    // ... your logic\n}\n```\n\n**2. FlyOut-triggered cancellation may still be missing**\n\nEven with `e.Handled = true`, when a MAUI FlyOut opens, it intercepts at the MAUI navigation layer without necessarily going through UIKit's standard gesture-recognizer cancellation. As a fallback, you can subscribe to Shell/FlyoutPage state and reset finger tracking manually:\n```csharp\n// In your page or view model\nShell.Current.PropertyChanged += (s, e) => {\n    if (e.PropertyName == nameof(Shell.FlyoutIsPresented) && Shell.Current.FlyoutIsPresented)\n        ResetTouchTracking(); // clear your finger count\n};\n```\n\nCould you confirm whether setting `e.Handled = true` resolves the issue? This will help narrow down which of the two causes dominates."
      }
    ]
  }
}
```

</details>
