# Issue Triage Report — #853

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T18:33:50Z |
| Type | type/question (0.88 (88%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.82 (82%)) |

**Issue Summary:** Reporter asks why SKTouchAction.Moved fires immediately after Pressed on a real iOS device even for taps, while mouse input in the simulator only triggers Moved on intentional drag; multiple users confirmed the same behavior on Android.

**Analysis:** The behavior is by design: on real touch devices iOS and Android report tiny movements the instant a finger contacts the screen due to the large physical contact area of a fingertip. SkiaSharp faithfully forwards all native touch events without any dead-zone filtering — UITouchPhaseMoved on iOS and MotionEventActions.Move on Android are passed through verbatim as SKTouchAction.Moved. The simulator with mouse input does not exhibit this because mice only generate move events on intentional motion. A user-space distance-threshold workaround is the correct solution.

**Recommendations:** **close-as-not-a-bug** — SKTouchAction.Moved firing on tap is expected platform behavior — SkiaSharp correctly forwards native touch events that include micro-movements on physical devices. A validated workaround using SKPoint.Distance exists and is documented in the comments.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp.Views |
| Platforms | os/iOS, os/Android |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | type/question |

## Evidence

### Reproduction

1. Create an app with SKCanvasView and subscribe to the Touch event
2. Handle SKTouchAction.Pressed and SKTouchAction.Moved in the event handler
3. Test in simulator with mouse: Moved only fires on click-and-drag
4. Deploy to a physical iOS or Android device: Moved fires immediately after Pressed even on a tap

**Environment:** iOS Xs Max (original reporter); also confirmed on both iOS and Android by second commenter

## Analysis

### Technical Summary

The behavior is by design: on real touch devices iOS and Android report tiny movements the instant a finger contacts the screen due to the large physical contact area of a fingertip. SkiaSharp faithfully forwards all native touch events without any dead-zone filtering — UITouchPhaseMoved on iOS and MotionEventActions.Move on Android are passed through verbatim as SKTouchAction.Moved. The simulator with mouse input does not exhibit this because mice only generate move events on intentional motion. A user-space distance-threshold workaround is the correct solution.

### Rationale

The reporter explicitly asked whether this is a bug or desired behavior. Code investigation confirms SkiaSharp correctly forwards native touch events without filtering. Adding a global dead-zone filter in SkiaSharp would break drawing apps and other use cases that rely on micro-movement detection. The fix belongs in user space. A community workaround using SKPoint.Distance is already documented in the comments.

### Key Signals

- "everytime I touch the screen and the touch event fires, the SKTouchAction.Moved is matched which is not what I would expect unless I keep my finger down" — **issue body** (Native touch input reports micro-movements on contact — expected physical behavior of capacitive touch screens)
- "This does not happen when using the mouse in the simulator" — **issue body** (Mouse hardware only generates move events on intentional movement; capacitive touch hardware does not filter micro-movements)
- "on the actual device (Android and iOS) SKTouchAction.Moved is always called right after SKTouchAction.Pressed even if I'm trying not to move my finger" — **comment by anpin** (Confirms consistent behavior across iOS and Android — platform-level behavior, not SkiaSharp-specific)
- "My workaround was to call a function to check, within the SKTouchAction.Moved switch case, if a move is actually a tap" — **comment by EmmanuelJego** (Community workaround using distance threshold confirms this is expected behavior with a viable user-space fix)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs` | 67-75 | direct | TouchesMoved directly maps to SKTouchAction.Moved with no distance threshold — raw iOS UITouchPhaseMoved events are forwarded unchanged to the consumer |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs` | 75-89 | direct | MotionEventActions.Move directly maps to SKTouchAction.Moved for all active pointers — Android events are forwarded unchanged with no dead-zone filtering |

### Workarounds

- Track initial touch position on Pressed, then filter Moved events where SKPoint.Distance(currentLocation, pressedLocation) is below a small threshold (e.g., 10 pixels) before processing as a drag gesture

### Resolution Proposals

**Hypothesis:** Native touch subsystems on iOS and Android report micro-movements immediately upon finger contact. SkiaSharp correctly forwards these events. Users must implement their own dead-zone filter to distinguish tap from drag.

1. **Dead-zone distance filter** — workaround, confidence 0.80 (80%), cost/xs, validated=yes
   - Track the initial touch position on Pressed and ignore Moved events within a small radius. This prevents micro-movements from being treated as drag gestures. Add a Cancelled case alongside Released to clear state when the gesture is interrupted.

```csharp
private SKPoint? _startPoint;

void OnTouch(object sender, SKTouchEventArgs e)
{
    switch (e.ActionType)
    {
        case SKTouchAction.Pressed:
            _startPoint = e.Location;
            break;
        case SKTouchAction.Moved:
            if (_startPoint.HasValue &&
                SKPoint.Distance(e.Location, _startPoint.Value) < 10)
                return; // micro-movement, treat as tap
            // real drag handling here
            break;
        case SKTouchAction.Released:
        case SKTouchAction.Cancelled:
            _startPoint = null;
            break;
    }
}
```

**Recommended proposal:** Dead-zone distance filter

**Why:** Simple, well-validated user-space solution already confirmed working by the community. Uses the SKPoint.Distance API correctly. Tuning the threshold value (10 pixels) may be needed for high-DPI screens.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.82 (82%) |
| Reason | SKTouchAction.Moved firing on tap is expected platform behavior — SkiaSharp correctly forwards native touch events that include micro-movements on physical devices. A validated workaround using SKPoint.Distance exists and is documented in the comments. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply question, views, iOS, and Android labels | labels=type/question, area/SkiaSharp.Views, os/iOS, os/Android |
| add-comment | high | 0.82 (82%) | Explain by-design behavior and provide validated dead-zone distance workaround | — |
| close-issue | medium | 0.75 (75%) | Close as answered — by-design behavior with workaround provided | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
This is expected platform behavior. On real touch devices, iOS and Android report tiny movements the instant a finger makes contact, due to the large physical contact area of a fingertip. SkiaSharp faithfully forwards these native events without filtering. The simulator with mouse input doesn't exhibit this because mice only generate move events on intentional movement.

Here's a workaround you can use to filter out micro-movements:

```csharp
private SKPoint? _startPoint;

void OnTouch(object sender, SKTouchEventArgs e)
{
    switch (e.ActionType)
    {
        case SKTouchAction.Pressed:
            _startPoint = e.Location;
            break;
        case SKTouchAction.Moved:
            if (_startPoint.HasValue &&
                SKPoint.Distance(e.Location, _startPoint.Value) < 10)
                return; // micro-movement, ignore
            // real drag handling here
            break;
        case SKTouchAction.Released:
        case SKTouchAction.Cancelled:
            _startPoint = null;
            break;
    }
}
```

You may need to tune the `10`-pixel threshold based on your device's screen density — on high-DPI screens a value of 15–20 may work better.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 853,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T18:33:50Z",
    "currentLabels": [
      "type/question"
    ]
  },
  "summary": "Reporter asks why SKTouchAction.Moved fires immediately after Pressed on a real iOS device even for taps, while mouse input in the simulator only triggers Moved on intentional drag; multiple users confirmed the same behavior on Android.",
  "classification": {
    "type": {
      "value": "type/question",
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
        "Create an app with SKCanvasView and subscribe to the Touch event",
        "Handle SKTouchAction.Pressed and SKTouchAction.Moved in the event handler",
        "Test in simulator with mouse: Moved only fires on click-and-drag",
        "Deploy to a physical iOS or Android device: Moved fires immediately after Pressed even on a tap"
      ],
      "environmentDetails": "iOS Xs Max (original reporter); also confirmed on both iOS and Android by second commenter"
    }
  },
  "analysis": {
    "summary": "The behavior is by design: on real touch devices iOS and Android report tiny movements the instant a finger contacts the screen due to the large physical contact area of a fingertip. SkiaSharp faithfully forwards all native touch events without any dead-zone filtering — UITouchPhaseMoved on iOS and MotionEventActions.Move on Android are passed through verbatim as SKTouchAction.Moved. The simulator with mouse input does not exhibit this because mice only generate move events on intentional motion. A user-space distance-threshold workaround is the correct solution.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs",
        "lines": "67-75",
        "finding": "TouchesMoved directly maps to SKTouchAction.Moved with no distance threshold — raw iOS UITouchPhaseMoved events are forwarded unchanged to the consumer",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs",
        "lines": "75-89",
        "finding": "MotionEventActions.Move directly maps to SKTouchAction.Moved for all active pointers — Android events are forwarded unchanged with no dead-zone filtering",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "everytime I touch the screen and the touch event fires, the SKTouchAction.Moved is matched which is not what I would expect unless I keep my finger down",
        "source": "issue body",
        "interpretation": "Native touch input reports micro-movements on contact — expected physical behavior of capacitive touch screens"
      },
      {
        "text": "This does not happen when using the mouse in the simulator",
        "source": "issue body",
        "interpretation": "Mouse hardware only generates move events on intentional movement; capacitive touch hardware does not filter micro-movements"
      },
      {
        "text": "on the actual device (Android and iOS) SKTouchAction.Moved is always called right after SKTouchAction.Pressed even if I'm trying not to move my finger",
        "source": "comment by anpin",
        "interpretation": "Confirms consistent behavior across iOS and Android — platform-level behavior, not SkiaSharp-specific"
      },
      {
        "text": "My workaround was to call a function to check, within the SKTouchAction.Moved switch case, if a move is actually a tap",
        "source": "comment by EmmanuelJego",
        "interpretation": "Community workaround using distance threshold confirms this is expected behavior with a viable user-space fix"
      }
    ],
    "rationale": "The reporter explicitly asked whether this is a bug or desired behavior. Code investigation confirms SkiaSharp correctly forwards native touch events without filtering. Adding a global dead-zone filter in SkiaSharp would break drawing apps and other use cases that rely on micro-movement detection. The fix belongs in user space. A community workaround using SKPoint.Distance is already documented in the comments.",
    "workarounds": [
      "Track initial touch position on Pressed, then filter Moved events where SKPoint.Distance(currentLocation, pressedLocation) is below a small threshold (e.g., 10 pixels) before processing as a drag gesture"
    ],
    "resolution": {
      "hypothesis": "Native touch subsystems on iOS and Android report micro-movements immediately upon finger contact. SkiaSharp correctly forwards these events. Users must implement their own dead-zone filter to distinguish tap from drag.",
      "proposals": [
        {
          "title": "Dead-zone distance filter",
          "description": "Track the initial touch position on Pressed and ignore Moved events within a small radius. This prevents micro-movements from being treated as drag gestures. Add a Cancelled case alongside Released to clear state when the gesture is interrupted.",
          "category": "workaround",
          "codeSnippet": "private SKPoint? _startPoint;\n\nvoid OnTouch(object sender, SKTouchEventArgs e)\n{\n    switch (e.ActionType)\n    {\n        case SKTouchAction.Pressed:\n            _startPoint = e.Location;\n            break;\n        case SKTouchAction.Moved:\n            if (_startPoint.HasValue &&\n                SKPoint.Distance(e.Location, _startPoint.Value) < 10)\n                return; // micro-movement, treat as tap\n            // real drag handling here\n            break;\n        case SKTouchAction.Released:\n        case SKTouchAction.Cancelled:\n            _startPoint = null;\n            break;\n    }\n}",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Dead-zone distance filter",
      "recommendedReason": "Simple, well-validated user-space solution already confirmed working by the community. Uses the SKPoint.Distance API correctly. Tuning the threshold value (10 pixels) may be needed for high-DPI screens."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.82,
      "reason": "SKTouchAction.Moved firing on tap is expected platform behavior — SkiaSharp correctly forwards native touch events that include micro-movements on physical devices. A validated workaround using SKPoint.Distance exists and is documented in the comments.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question, views, iOS, and Android labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp.Views",
          "os/iOS",
          "os/Android"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain by-design behavior and provide validated dead-zone distance workaround",
        "risk": "high",
        "confidence": 0.82,
        "comment": "This is expected platform behavior. On real touch devices, iOS and Android report tiny movements the instant a finger makes contact, due to the large physical contact area of a fingertip. SkiaSharp faithfully forwards these native events without filtering. The simulator with mouse input doesn't exhibit this because mice only generate move events on intentional movement.\n\nHere's a workaround you can use to filter out micro-movements:\n\n```csharp\nprivate SKPoint? _startPoint;\n\nvoid OnTouch(object sender, SKTouchEventArgs e)\n{\n    switch (e.ActionType)\n    {\n        case SKTouchAction.Pressed:\n            _startPoint = e.Location;\n            break;\n        case SKTouchAction.Moved:\n            if (_startPoint.HasValue &&\n                SKPoint.Distance(e.Location, _startPoint.Value) < 10)\n                return; // micro-movement, ignore\n            // real drag handling here\n            break;\n        case SKTouchAction.Released:\n        case SKTouchAction.Cancelled:\n            _startPoint = null;\n            break;\n    }\n}\n```\n\nYou may need to tune the `10`-pixel threshold based on your device's screen density — on high-DPI screens a value of 15–20 may work better."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — by-design behavior with workaround provided",
        "risk": "medium",
        "confidence": 0.75,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
