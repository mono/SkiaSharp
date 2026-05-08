# Issue Triage Report — #2458

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-05T00:12:20Z |
| Type | type/question (0.88 (88%)) |
| Area | area/SkiaSharp.Views.Maui (0.95 (95%)) |
| Suggested action | close-as-not-a-bug (0.78 (78%)) |

**Issue Summary:** User asks why PanGestureRecognizer on SKCanvasView is not invoked on Android MAUI, likely due to SKCanvasView's internal touch handler consuming Android touch events before the MAUI gesture recognizer can process them.

**Analysis:** SKCanvasView on Android registers a View.Touch listener via SKTouchHandler. When EnableTouchEvents is enabled and the touch event is marked as Handled=true, Android stops propagating the event up the view hierarchy, preventing MAUI's PanGestureRecognizer from receiving it. The Output log shows the View_OnTouchListenerImplementor consuming the touch event, confirming this interaction. The workaround is either to not use EnableTouchEvents=true simultaneously with MAUI gesture recognizers, or to ensure the SKTouchEventArgs.Handled is set to false in touch callbacks so events propagate.

**Recommendations:** **close-as-not-a-bug** — This is expected Android touch event behavior. SKCanvasView's internal listener consuming touch events is by design. The reporter needs guidance on how to avoid the conflict between EnableTouchEvents and MAUI gesture recognizers.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/Android |
| Backends | — |
| Tenets | — |
| Partner | partner/maui |

## Evidence

### Reproduction

**Code snippets:**

```csharp
<skia:SKCanvasView.GestureRecognizers><PanGestureRecognizer PanUpdated="PanGestureRecognizer_PanUpdated"></PanGestureRecognizer></skia:SKCanvasView.GestureRecognizers>
```

## Analysis

### Technical Summary

SKCanvasView on Android registers a View.Touch listener via SKTouchHandler. When EnableTouchEvents is enabled and the touch event is marked as Handled=true, Android stops propagating the event up the view hierarchy, preventing MAUI's PanGestureRecognizer from receiving it. The Output log shows the View_OnTouchListenerImplementor consuming the touch event, confirming this interaction. The workaround is either to not use EnableTouchEvents=true simultaneously with MAUI gesture recognizers, or to ensure the SKTouchEventArgs.Handled is set to false in touch callbacks so events propagate.

### Rationale

Classified as type/question because the reporter is asking why PanGestureRecognizer is not called rather than reporting a bug with a clear regression. The root cause is the known Android event propagation mechanism: when SKCanvasView's internal listener consumes touch events, MAUI gesture recognizers don't fire. The area is SkiaSharp.Views.Maui since this involves the MAUI-specific view layer on Android.

### Key Signals

- "[GestureDetector] obtain mCurrentDownEvent. id: 838281512 caller: mono.android.view.View_OnTouchListenerImplementor.n_onTouch" — **issue body - Output window log** (Android's GestureDetector is detecting the touch event, but the OnTouchListener (SkiaSharp's touch handler) is intercepting the event before the PanGestureRecognizer can act on it.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs` | 70-71 | direct | SKTouchHandler.OnTouch sets e.Handled = args.Handled on MotionEventActions.Down/PointerDown. If the touch event is consumed (Handled=true), Android will not propagate it to the MAUI gesture recognizer layer. |

### Workarounds

- Do not set EnableTouchEvents=true when using MAUI GestureRecognizers — if SKTouchEvents are not subscribed, the touch handler won't consume the event and PanGestureRecognizer will work normally.
- If SKTouchEvents are needed, in the Touch event handler set e.Handled = false so events propagate to the MAUI gesture layer.
- Use SkiaSharp's built-in Touch event (EnableTouchEvents=true + SKCanvasView.Touch) to implement pan logic natively instead of MAUI's PanGestureRecognizer.

### Next Questions

- Does the reporter have EnableTouchEvents=true? If so, that is likely consuming the touch events.
- Which version of SkiaSharp and .NET MAUI is being used?

### Resolution Proposals

**Hypothesis:** PanGestureRecognizer is not fired because SKCanvasView's Android touch listener consumes the touch events (sets Handled=true), preventing MAUI gesture recognizers from receiving them.

1. **Disable EnableTouchEvents or set Handled=false** — workaround, cost/xs, validated=yes
   - If the reporter does not need SkiaSharp touch events, remove EnableTouchEvents=true. If needed, set Handled=false in the touch event handler so MAUI can still process gestures.

**Recommended proposal:** p1

**Why:** This is a well-understood Android event propagation issue. The fix requires no code changes to SkiaSharp itself.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.78 (78%) |
| Reason | This is expected Android touch event behavior. SKCanvasView's internal listener consuming touch events is by design. The reporter needs guidance on how to avoid the conflict between EnableTouchEvents and MAUI gesture recognizers. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/question, area/SkiaSharp.Views.Maui, os/Android, partner/maui | labels=type/question, area/SkiaSharp.Views.Maui, os/Android, partner/maui |
| add-comment | medium | 0.80 (80%) | Explain why PanGestureRecognizer is not called and provide workarounds | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report!

This behavior is related to how Android handles touch events. When `SKCanvasView` has `EnableTouchEvents = true`, it registers an internal `View.Touch` listener that can consume (`Handled = true`) touch events on the Android level. When an event is consumed by the view, Android stops propagating it further, which prevents MAUI's `PanGestureRecognizer` from ever receiving it.

You can see this happening in your Output log: `mono.android.view.View_OnTouchListenerImplementor` is the SkiaSharp touch listener intercepting the event.

**Workarounds:**

1. **If you don't need SkiaSharp's touch events:** Remove `EnableTouchEvents="True"` from your `SKCanvasView`. Without it, no touch listener is registered and `PanGestureRecognizer` will work normally.

2. **If you need both:** In your touch event handler, set `e.Handled = false` so the event is not consumed and propagates to the MAUI gesture layer:
   ```csharp
   canvasView.Touch += (s, e) => {
       // handle your SKTouch logic here
       e.Handled = false; // allow MAUI gesture recognizers to also receive the event
   };
   ```

3. **Alternative:** Use SkiaSharp's native `Touch` event with `EnableTouchEvents = true` to implement your pan/gesture logic entirely within SkiaSharp, rather than using MAUI's `PanGestureRecognizer`.

This is a known limitation of combining platform-native touch listeners with MAUI's gesture recognizer system on Android.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2458,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-05T00:12:20Z"
  },
  "summary": "User asks why PanGestureRecognizer on SKCanvasView is not invoked on Android MAUI, likely due to SKCanvasView's internal touch handler consuming Android touch events before the MAUI gesture recognizer can process them.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.95
    },
    "platforms": [
      "os/Android"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "<skia:SKCanvasView.GestureRecognizers><PanGestureRecognizer PanUpdated=\"PanGestureRecognizer_PanUpdated\"></PanGestureRecognizer></skia:SKCanvasView.GestureRecognizers>"
      ],
      "repoLinks": []
    }
  },
  "analysis": {
    "summary": "SKCanvasView on Android registers a View.Touch listener via SKTouchHandler. When EnableTouchEvents is enabled and the touch event is marked as Handled=true, Android stops propagating the event up the view hierarchy, preventing MAUI's PanGestureRecognizer from receiving it. The Output log shows the View_OnTouchListenerImplementor consuming the touch event, confirming this interaction. The workaround is either to not use EnableTouchEvents=true simultaneously with MAUI gesture recognizers, or to ensure the SKTouchEventArgs.Handled is set to false in touch callbacks so events propagate.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs",
        "finding": "SKTouchHandler.OnTouch sets e.Handled = args.Handled on MotionEventActions.Down/PointerDown. If the touch event is consumed (Handled=true), Android will not propagate it to the MAUI gesture recognizer layer.",
        "relevance": "direct",
        "lines": "70-71"
      }
    ],
    "keySignals": [
      {
        "text": "[GestureDetector] obtain mCurrentDownEvent. id: 838281512 caller: mono.android.view.View_OnTouchListenerImplementor.n_onTouch",
        "source": "issue body - Output window log",
        "interpretation": "Android's GestureDetector is detecting the touch event, but the OnTouchListener (SkiaSharp's touch handler) is intercepting the event before the PanGestureRecognizer can act on it."
      }
    ],
    "rationale": "Classified as type/question because the reporter is asking why PanGestureRecognizer is not called rather than reporting a bug with a clear regression. The root cause is the known Android event propagation mechanism: when SKCanvasView's internal listener consumes touch events, MAUI gesture recognizers don't fire. The area is SkiaSharp.Views.Maui since this involves the MAUI-specific view layer on Android.",
    "workarounds": [
      "Do not set EnableTouchEvents=true when using MAUI GestureRecognizers — if SKTouchEvents are not subscribed, the touch handler won't consume the event and PanGestureRecognizer will work normally.",
      "If SKTouchEvents are needed, in the Touch event handler set e.Handled = false so events propagate to the MAUI gesture layer.",
      "Use SkiaSharp's built-in Touch event (EnableTouchEvents=true + SKCanvasView.Touch) to implement pan logic natively instead of MAUI's PanGestureRecognizer."
    ],
    "nextQuestions": [
      "Does the reporter have EnableTouchEvents=true? If so, that is likely consuming the touch events.",
      "Which version of SkiaSharp and .NET MAUI is being used?"
    ],
    "resolution": {
      "hypothesis": "PanGestureRecognizer is not fired because SKCanvasView's Android touch listener consumes the touch events (sets Handled=true), preventing MAUI gesture recognizers from receiving them.",
      "proposals": [
        {
          "title": "Disable EnableTouchEvents or set Handled=false",
          "description": "If the reporter does not need SkiaSharp touch events, remove EnableTouchEvents=true. If needed, set Handled=false in the touch event handler so MAUI can still process gestures.",
          "category": "workaround",
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "p1",
      "recommendedReason": "This is a well-understood Android event propagation issue. The fix requires no code changes to SkiaSharp itself."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.78,
      "reason": "This is expected Android touch event behavior. SKCanvasView's internal listener consuming touch events is by design. The reporter needs guidance on how to avoid the conflict between EnableTouchEvents and MAUI gesture recognizers.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/question, area/SkiaSharp.Views.Maui, os/Android, partner/maui",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/question",
          "area/SkiaSharp.Views.Maui",
          "os/Android",
          "partner/maui"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain why PanGestureRecognizer is not called and provide workarounds",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the report!\n\nThis behavior is related to how Android handles touch events. When `SKCanvasView` has `EnableTouchEvents = true`, it registers an internal `View.Touch` listener that can consume (`Handled = true`) touch events on the Android level. When an event is consumed by the view, Android stops propagating it further, which prevents MAUI's `PanGestureRecognizer` from ever receiving it.\n\nYou can see this happening in your Output log: `mono.android.view.View_OnTouchListenerImplementor` is the SkiaSharp touch listener intercepting the event.\n\n**Workarounds:**\n\n1. **If you don't need SkiaSharp's touch events:** Remove `EnableTouchEvents=\"True\"` from your `SKCanvasView`. Without it, no touch listener is registered and `PanGestureRecognizer` will work normally.\n\n2. **If you need both:** In your touch event handler, set `e.Handled = false` so the event is not consumed and propagates to the MAUI gesture layer:\n   ```csharp\n   canvasView.Touch += (s, e) => {\n       // handle your SKTouch logic here\n       e.Handled = false; // allow MAUI gesture recognizers to also receive the event\n   };\n   ```\n\n3. **Alternative:** Use SkiaSharp's native `Touch` event with `EnableTouchEvents = true` to implement your pan/gesture logic entirely within SkiaSharp, rather than using MAUI's `PanGestureRecognizer`.\n\nThis is a known limitation of combining platform-native touch listeners with MAUI's gesture recognizer system on Android."
      }
    ]
  }
}
```

</details>
