# Issue Triage Report — #937

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T09:22:09Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp.Views (0.88 (88%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** When SKCanvasView is placed inside a ScrollView on Android, touch events are not properly consumed by the canvas, allowing the parent scroll container to intercept them and scroll even while the user drags inside the canvas.

**Analysis:** On Android, child views must call parent.RequestDisallowInterceptTouchEvent(true) to prevent ancestor ScrollViews from intercepting touch events once a drag begins. The SKTouchHandler sets e.Handled = args.Handled on MotionEventActions.Down/Move but never calls RequestDisallowInterceptTouchEvent, so the Android view hierarchy still delivers ACTION_CANCEL to the canvas and starts scrolling the parent.

**Recommendations:** **needs-investigation** — Root cause is clear (missing RequestDisallowInterceptTouchEvent call in Android SKTouchHandler), but the fix requires design decisions: should it be unconditional on Down or conditional on args.Handled, and whether the same fix is needed for iOS. The issue also persists in MAUI per the 2024 comment.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Android, os/iOS |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Place an SKCanvasView with EnableTouchEvents=True inside a ScrollView (e.g. ScrollView > StackLayout > SKCanvasView + ListView)
2. Run on Android device
3. Touch and drag inside the SKCanvasView
4. Observe that the parent ScrollView scrolls despite the touch being 'handled' in the Touch callback

**Environment:** SkiaSharp 1.68.0 (latest stable at time of filing), Android 8.1, Visual Studio, Xamarin.Forms

**Repository links:**
- https://github.com/mattleibow/SkiaSharpDemo/tree/master/SkiaSharpDemo/SkiaSharpDemo — Demo project used as repro basis
- https://stackoverflow.com/questions/6018309/action-cancel-while-touching — SO explanation of Android ACTION_CANCEL touch interception
- https://github.com/xamarin/Xamarin.Forms/issues/8497 — Related Xamarin.Forms bug about ScrollOrientation.Neither
- https://github.com/dotnet/maui/discussions/18758 — Related MAUI discussion for the same issue in .NET MAUI

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | platform-specific |
| Error message | — |
| Repro quality | complete |
| Target frameworks | monoandroid8.1 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The Android SKTouchHandler in SkiaSharp.Views.Maui still does not call RequestDisallowInterceptTouchEvent, and a 2024 comment references the same problem in MAUI. |

## Analysis

### Technical Summary

On Android, child views must call parent.RequestDisallowInterceptTouchEvent(true) to prevent ancestor ScrollViews from intercepting touch events once a drag begins. The SKTouchHandler sets e.Handled = args.Handled on MotionEventActions.Down/Move but never calls RequestDisallowInterceptTouchEvent, so the Android view hierarchy still delivers ACTION_CANCEL to the canvas and starts scrolling the parent.

### Rationale

This is a platform-specific touch interception bug on Android, not a usage error. The reporter correctly sets e.Handled = true in the Touch callback, but Android's touch dispatch model requires the child to actively opt out of parent interception via RequestDisallowInterceptTouchEvent. This call is absent from the current SKTouchHandler. The maintainer confirmed it needs fixing and referenced a Xamarin.Forms bug as a blocker. A 2024 comment shows the problem persists in MAUI.

### Key Signals

- "e.Handled = args.Handled" — **source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs lines 71, 86, 96, 105** (Handled flag is propagated back to Android, but without RequestDisallowInterceptTouchEvent the parent ScrollView can still steal the gesture.)
- "What I found was that UWP works in that as soon as you start drawing, it captures the pointer and won't scroll. This PR will fix it for Android." — **maintainer comment #553694781** (Maintainer (mattleibow) confirmed it needs fixing for Android, but was blocked on a Xamarin.Forms issue.)
- "For .NET MAUI: https://github.com/dotnet/maui/discussions/18758" — **comment #2106976714 (2024)** (The issue is still open and reproduced in MAUI in 2024.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs` | 63-109 | direct | OnTouch handles Down/Move/Up/Cancel motion events and sets e.Handled = args.Handled, but never calls view.Parent?.RequestDisallowInterceptTouchEvent(true) on Down or when the event is handled. Without this call, Android parent ScrollViews remain free to intercept subsequent MotionEvents and send ACTION_CANCEL to the child. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs` | 1-156 | context | The native Android SKCanvasView (Views package, not MAUI) has no touch handling code at all — it is a plain Android View subclass. Touch events for the canvas in Xamarin.Forms / MAUI go through a separate handler (SKTouchHandler). |

### Workarounds

- In the SKCanvasView Touch callback, when the event is handled, call ((View)((FrameworkElement)sender).Handler?.PlatformView)?.Parent?.RequestDisallowInterceptTouchEvent(true) manually (MAUI-specific, fragile).
- Disable the parent ScrollView when a MotionEventActions.Down is detected inside the canvas and re-enable it on Up/Cancel — requires user-level coordination between SKCanvasView and ScrollView.
- Use a custom ScrollView subclass that overrides OnInterceptTouchEvent and returns false while the canvas is being touched.

### Next Questions

- Does the same touch propagation problem affect iOS (UIScrollView) with the SkiaSharp MAUI handler?
- Should RequestDisallowInterceptTouchEvent be called unconditionally on Down, or only when args.Handled is true?
- Does SKGLView (GL texture view) have the same problem on Android?

### Resolution Proposals

**Hypothesis:** Calling parent.RequestDisallowInterceptTouchEvent(true) in SKTouchHandler.OnTouch when a Down event is received (or when any event is marked Handled) will prevent Android parent ScrollViews from stealing the gesture sequence.

1. **Call RequestDisallowInterceptTouchEvent in SKTouchHandler** — fix, confidence 0.80 (80%), cost/s, validated=untested
   - In SKTouchHandler.OnTouch, after handling MotionEventActions.Down, call (sender as View)?.Parent?.RequestDisallowInterceptTouchEvent(args.Handled). On Up/Cancel, call RequestDisallowInterceptTouchEvent(false) to restore normal behavior.
2. **User-side ScrollView disabling workaround** — workaround, confidence 0.70 (70%), cost/s, validated=untested
   - Users can subscribe to the Touch event and, upon Pressed, disable the parent scroll container. On Released/Cancelled, re-enable it. This is fragile but works today without a SkiaSharp change.

**Recommended proposal:** Call RequestDisallowInterceptTouchEvent in SKTouchHandler

**Why:** A small focused change in SKTouchHandler.cs on Android that mirrors how other Android drawing views handle touch interception. The user-side workaround is fragile and not discoverable.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Root cause is clear (missing RequestDisallowInterceptTouchEvent call in Android SKTouchHandler), but the fix requires design decisions: should it be unconditional on Down or conditional on args.Handled, and whether the same fix is needed for iOS. The issue also persists in MAUI per the 2024 comment. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, views, android, and reliability labels | labels=type/bug, area/SkiaSharp.Views, os/Android, tenet/reliability |
| add-comment | medium | 0.82 (82%) | Summarize root cause and suggest investigation path | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and GIF repro.

The root cause is that Android's touch dispatch model requires a child view to explicitly call `parent.RequestDisallowInterceptTouchEvent(true)` to prevent ancestor `ScrollView`s from intercepting touch events once a drag begins. The current `SKTouchHandler` sets `e.Handled = args.Handled` but does not call `RequestDisallowInterceptTouchEvent`, so the parent can still steal the gesture and deliver `ACTION_CANCEL` to the canvas.

The fix would be to add `(sender as View)?.Parent?.RequestDisallowInterceptTouchEvent(args.Handled)` after handling the `Down` event in `SKTouchHandler.OnTouch` (and `false` on `Up`/`Cancel`).

Note: this issue appears to still reproduce in .NET MAUI (see the 2024 comment linking to https://github.com/dotnet/maui/discussions/18758).

**Workaround (user-side):** In your `Touch` callback, when you handle a `Pressed` event, programmatically disable scrolling on the parent `ScrollView` and re-enable it on `Released`/`Cancelled`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 937,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T09:22:09Z"
  },
  "summary": "When SKCanvasView is placed inside a ScrollView on Android, touch events are not properly consumed by the canvas, allowing the parent scroll container to intercept them and scroll even while the user drags inside the canvas.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.88
    },
    "platforms": [
      "os/Android",
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
      "errorType": "platform-specific",
      "reproQuality": "complete",
      "targetFrameworks": [
        "monoandroid8.1"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Place an SKCanvasView with EnableTouchEvents=True inside a ScrollView (e.g. ScrollView > StackLayout > SKCanvasView + ListView)",
        "Run on Android device",
        "Touch and drag inside the SKCanvasView",
        "Observe that the parent ScrollView scrolls despite the touch being 'handled' in the Touch callback"
      ],
      "environmentDetails": "SkiaSharp 1.68.0 (latest stable at time of filing), Android 8.1, Visual Studio, Xamarin.Forms",
      "repoLinks": [
        {
          "url": "https://github.com/mattleibow/SkiaSharpDemo/tree/master/SkiaSharpDemo/SkiaSharpDemo",
          "description": "Demo project used as repro basis"
        },
        {
          "url": "https://stackoverflow.com/questions/6018309/action-cancel-while-touching",
          "description": "SO explanation of Android ACTION_CANCEL touch interception"
        },
        {
          "url": "https://github.com/xamarin/Xamarin.Forms/issues/8497",
          "description": "Related Xamarin.Forms bug about ScrollOrientation.Neither"
        },
        {
          "url": "https://github.com/dotnet/maui/discussions/18758",
          "description": "Related MAUI discussion for the same issue in .NET MAUI"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The Android SKTouchHandler in SkiaSharp.Views.Maui still does not call RequestDisallowInterceptTouchEvent, and a 2024 comment references the same problem in MAUI."
    }
  },
  "analysis": {
    "summary": "On Android, child views must call parent.RequestDisallowInterceptTouchEvent(true) to prevent ancestor ScrollViews from intercepting touch events once a drag begins. The SKTouchHandler sets e.Handled = args.Handled on MotionEventActions.Down/Move but never calls RequestDisallowInterceptTouchEvent, so the Android view hierarchy still delivers ACTION_CANCEL to the canvas and starts scrolling the parent.",
    "rationale": "This is a platform-specific touch interception bug on Android, not a usage error. The reporter correctly sets e.Handled = true in the Touch callback, but Android's touch dispatch model requires the child to actively opt out of parent interception via RequestDisallowInterceptTouchEvent. This call is absent from the current SKTouchHandler. The maintainer confirmed it needs fixing and referenced a Xamarin.Forms bug as a blocker. A 2024 comment shows the problem persists in MAUI.",
    "keySignals": [
      {
        "text": "e.Handled = args.Handled",
        "source": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs lines 71, 86, 96, 105",
        "interpretation": "Handled flag is propagated back to Android, but without RequestDisallowInterceptTouchEvent the parent ScrollView can still steal the gesture."
      },
      {
        "text": "What I found was that UWP works in that as soon as you start drawing, it captures the pointer and won't scroll. This PR will fix it for Android.",
        "source": "maintainer comment #553694781",
        "interpretation": "Maintainer (mattleibow) confirmed it needs fixing for Android, but was blocked on a Xamarin.Forms issue."
      },
      {
        "text": "For .NET MAUI: https://github.com/dotnet/maui/discussions/18758",
        "source": "comment #2106976714 (2024)",
        "interpretation": "The issue is still open and reproduced in MAUI in 2024."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs",
        "lines": "63-109",
        "finding": "OnTouch handles Down/Move/Up/Cancel motion events and sets e.Handled = args.Handled, but never calls view.Parent?.RequestDisallowInterceptTouchEvent(true) on Down or when the event is handled. Without this call, Android parent ScrollViews remain free to intercept subsequent MotionEvents and send ACTION_CANCEL to the child.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs",
        "lines": "1-156",
        "finding": "The native Android SKCanvasView (Views package, not MAUI) has no touch handling code at all — it is a plain Android View subclass. Touch events for the canvas in Xamarin.Forms / MAUI go through a separate handler (SKTouchHandler).",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "In the SKCanvasView Touch callback, when the event is handled, call ((View)((FrameworkElement)sender).Handler?.PlatformView)?.Parent?.RequestDisallowInterceptTouchEvent(true) manually (MAUI-specific, fragile).",
      "Disable the parent ScrollView when a MotionEventActions.Down is detected inside the canvas and re-enable it on Up/Cancel — requires user-level coordination between SKCanvasView and ScrollView.",
      "Use a custom ScrollView subclass that overrides OnInterceptTouchEvent and returns false while the canvas is being touched."
    ],
    "nextQuestions": [
      "Does the same touch propagation problem affect iOS (UIScrollView) with the SkiaSharp MAUI handler?",
      "Should RequestDisallowInterceptTouchEvent be called unconditionally on Down, or only when args.Handled is true?",
      "Does SKGLView (GL texture view) have the same problem on Android?"
    ],
    "resolution": {
      "hypothesis": "Calling parent.RequestDisallowInterceptTouchEvent(true) in SKTouchHandler.OnTouch when a Down event is received (or when any event is marked Handled) will prevent Android parent ScrollViews from stealing the gesture sequence.",
      "proposals": [
        {
          "title": "Call RequestDisallowInterceptTouchEvent in SKTouchHandler",
          "description": "In SKTouchHandler.OnTouch, after handling MotionEventActions.Down, call (sender as View)?.Parent?.RequestDisallowInterceptTouchEvent(args.Handled). On Up/Cancel, call RequestDisallowInterceptTouchEvent(false) to restore normal behavior.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "User-side ScrollView disabling workaround",
          "description": "Users can subscribe to the Touch event and, upon Pressed, disable the parent scroll container. On Released/Cancelled, re-enable it. This is fragile but works today without a SkiaSharp change.",
          "category": "workaround",
          "confidence": 0.7,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Call RequestDisallowInterceptTouchEvent in SKTouchHandler",
      "recommendedReason": "A small focused change in SKTouchHandler.cs on Android that mirrors how other Android drawing views handle touch interception. The user-side workaround is fragile and not discoverable."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Root cause is clear (missing RequestDisallowInterceptTouchEvent call in Android SKTouchHandler), but the fix requires design decisions: should it be unconditional on Down or conditional on args.Handled, and whether the same fix is needed for iOS. The issue also persists in MAUI per the 2024 comment.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views, android, and reliability labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Android",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Summarize root cause and suggest investigation path",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report and GIF repro.\n\nThe root cause is that Android's touch dispatch model requires a child view to explicitly call `parent.RequestDisallowInterceptTouchEvent(true)` to prevent ancestor `ScrollView`s from intercepting touch events once a drag begins. The current `SKTouchHandler` sets `e.Handled = args.Handled` but does not call `RequestDisallowInterceptTouchEvent`, so the parent can still steal the gesture and deliver `ACTION_CANCEL` to the canvas.\n\nThe fix would be to add `(sender as View)?.Parent?.RequestDisallowInterceptTouchEvent(args.Handled)` after handling the `Down` event in `SKTouchHandler.OnTouch` (and `false` on `Up`/`Cancel`).\n\nNote: this issue appears to still reproduce in .NET MAUI (see the 2024 comment linking to https://github.com/dotnet/maui/discussions/18758).\n\n**Workaround (user-side):** In your `Touch` callback, when you handle a `Pressed` event, programmatically disable scrolling on the parent `ScrollView` and re-enable it on `Released`/`Cancelled`."
      }
    ]
  }
}
```

</details>
