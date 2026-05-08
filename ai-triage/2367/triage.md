# Issue Triage Report — #2367

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-23T12:53:00Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp.Views.Maui (0.95 (95%)) |
| Suggested action | ready-to-fix (0.87 (87%)) |

**Issue Summary:** On MacCatalyst, SKCanvasView with EnableTouchEvents=true captures mouse input and does not release it after a press/move/release sequence when the Touch event handler sets e.Handled=true, blocking touch events to sibling controls.

**Analysis:** SKTouchHandler (a UIGestureRecognizer subclass on Apple platforms) never sets its State to Recognized after TouchesEnded, so UIKit retains mouse capture even after the user releases the button, preventing sibling controls from receiving events.

**Recommendations:** **ready-to-fix** — Root cause is clearly identified in source (SKTouchHandler.TouchesEnded not setting UIGestureRecognizerState.Recognized), confirmed by community member who implemented the fix. The code change is minimal and the fix path is known.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/macOS |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | partner/maui |

## Evidence

### Reproduction

1. Create a MAUI app with a Button and SKCanvasView in a VerticalStackLayout
2. Set EnableTouchEvents=true on the SKCanvasView
3. Handle the Touch event and set e.Handled = true
4. Run on MacCatalyst
5. Mouse down, move, then release inside the SKCanvasView
6. Click the Button — it does not fire its Clicked event

**Environment:** macOS 12.6, SkiaSharp 2.88.3, Visual Studio for Mac / VS Code, MacCatalyst

**Repository links:**
- https://github.com/telerik/ms-samples/tree/main/Maui/SKCanvasViewTouchCaptured_MacCatalyst — Minimal MAUI repro project from reporter
- https://user-images.githubusercontent.com/61967449/214117627-a77342c7-ebde-433d-8d00-4ead0c74ad28.mov — Screen recording demonstrating the capture bug

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | platform-specific |
| Error message | — |
| Repro quality | complete |
| Target frameworks | net-maccatalyst |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3, 2.88.6 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The SKTouchHandler code path in Platform/Apple/SKTouchHandler.cs has not been changed to set UIGestureRecognizerState.Recognized after TouchesEnded — the root cause identified in comments remains present in the current code. |

## Analysis

### Technical Summary

SKTouchHandler (a UIGestureRecognizer subclass on Apple platforms) never sets its State to Recognized after TouchesEnded, so UIKit retains mouse capture even after the user releases the button, preventing sibling controls from receiving events.

### Rationale

The bug is clearly reproduced with a minimal app, confirmed by multiple users, and the root cause is identifiable directly in source code. SKTouchHandler extends UIGestureRecognizer but never transitions its State to Recognized after handling a complete touch sequence (Pressed → Moved → Released). UIKit's gesture recognizer state machine requires this transition to release input capture. This is MacCatalyst-specific because mouse input routing via UIGestureRecognizer is more strict about state transitions on Mac Catalyst than on touch-only iOS.

### Key Signals

- "Mouse down, then move, then up in the SKCanvasView. Then click the Button - the Button is not clicked." — **issue body** (Classic symptom of a UIGestureRecognizer retaining mouse capture because its state machine was not completed.)
- "In SKTouchHandler.TouchesEnded, FireEvent is called for each subscriber but the UIGestureRecognizer.State is left unchanged. I changed the logic to set State to Recognized if any subscriber sets TouchEventArgs.Handled to true." — **comment by DanTravison (#1845661121)** (Community member isolated the root cause and confirmed the fix with a custom UIGestureRecognizer.)
- "it appears the capture remains in place until I switch away, click on the apps window header, or wait for a while" — **comment by DanTravison (#1832414576)** (Confirms the gesture recognizer is holding capture; external events that cancel gesture tracking release it.)
- "When I click the mouse button, I receive a Pressed event and set e.Handled to true. As long as I keep the mouse pointer within the application's Window, the capture is NOT released when the mouse button is released." — **comment by DanTravison (#1833990907)** (Narrows the trigger to: Handled=true on Pressed + mouse pointer stays within window = capture not released.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs` | 77-85 | direct | TouchesEnded calls FireEvent for each touch but never sets this.State = UIGestureRecognizerState.Recognized (or Ended). The UIGestureRecognizer state machine is never explicitly completed, leaving the recognizer in an ambiguous state that retains mouse capture on MacCatalyst. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs` | 54-65 | direct | TouchesBegan calls IgnoreTouch when FireEvent returns false (i.e., not handled), but TouchesEnded has no equivalent logic to finalize the gesture recognizer state when handled=true. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandlerProxy.cs` | 1-55 | related | SKTouchHandlerProxy creates and manages SKTouchHandler via SetEnabled; does not intercept or supplement the gesture state machine — the fix must be in SKTouchHandler itself. |

### Workarounds

- Replace SKCanvasView touch handling by subclassing the platform handler and injecting a custom UIGestureRecognizer (modeled on SKTouchHandler) that sets State = UIGestureRecognizerState.Recognized in TouchesEnded when any handler sets e.Handled = true. Community member DanTravison confirmed this resolves the issue.

### Next Questions

- Does the same bug affect SKGLView on MacCatalyst?
- Is the fix safe to apply to iOS and tvOS as well, or only MacCatalyst?
- Should State be set to Recognized unconditionally in TouchesEnded, or only when Handled=true?

### Resolution Proposals

**Hypothesis:** Setting UIGestureRecognizer.State to Recognized inside TouchesEnded (when at least one subscriber sets Handled=true) will release mouse capture and allow sibling controls to receive events normally.

1. **Set State = Recognized in TouchesEnded when handled** — fix, confidence 0.88 (88%), cost/xs, validated=untested
   - In SKTouchHandler.TouchesEnded, after calling FireEvent for all touches, if any touch was handled (FireEvent returned true), set this.State = UIGestureRecognizerState.Recognized to complete the gesture recognizer state machine and release mouse capture.
2. **Inject custom UIGestureRecognizer via HandlerChanged workaround** — workaround, confidence 0.80 (80%), cost/m, validated=untested
   - As a short-term workaround, create a custom UIGestureRecognizer subclass that replicates SKTouchHandler but correctly sets State = Recognized on TouchesEnded. Inject it via the platform handler's HandlerChanged callback. This avoids changing the library but requires platform-specific MAUI code from the consumer.

**Recommended proposal:** Set State = Recognized in TouchesEnded when handled

**Why:** Minimal, targeted fix in the correct location. Confirmed by community analysis to be the root cause. Low risk as it only affects the gesture recognizer lifecycle, not the touch event data itself.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.87 (87%) |
| Reason | Root cause is clearly identified in source (SKTouchHandler.TouchesEnded not setting UIGestureRecognizerState.Recognized), confirmed by community member who implemented the fix. The code change is minimal and the fix path is known. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, MAUI views area, macOS platform, reliability tenet, and MAUI partner labels | labels=type/bug, area/SkiaSharp.Views.Maui, os/macOS, tenet/reliability, partner/maui |
| add-comment | medium | 0.87 (87%) | Acknowledge root cause, thank community member for investigation, provide workaround and fix direction | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report and to @DanTravison for narrowing down the root cause.

The issue is in `SKTouchHandler.TouchesEnded` — after firing the touch event, the `UIGestureRecognizer.State` is never set to `Recognized`. On Mac Catalyst, UIKit requires this state transition to release mouse capture after a complete press/move/release sequence. Without it, the recognizer holds onto the input stream indefinitely.

**Workaround (until this is fixed in the library):**
You can replace the default touch handler by injecting a custom `UIGestureRecognizer` through the MAUI platform handler's `HandlerChanged` callback. The custom recognizer should mirror `SKTouchHandler` but add `this.State = UIGestureRecognizerState.Recognized;` at the end of `TouchesEnded` when any subscriber sets `e.Handled = true`. @DanTravison confirmed this resolves the issue.

**Fix direction:**
Add `this.State = UIGestureRecognizerState.Recognized;` in `SKTouchHandler.TouchesEnded` when the handled flag is true, completing the gesture recognizer state machine.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2367,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-23T12:53:00Z"
  },
  "summary": "On MacCatalyst, SKCanvasView with EnableTouchEvents=true captures mouse input and does not release it after a press/move/release sequence when the Touch event handler sets e.Handled=true, blocking touch events to sibling controls.",
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
      "os/macOS"
    ],
    "tenets": [
      "tenet/reliability"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "platform-specific",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net-maccatalyst"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a MAUI app with a Button and SKCanvasView in a VerticalStackLayout",
        "Set EnableTouchEvents=true on the SKCanvasView",
        "Handle the Touch event and set e.Handled = true",
        "Run on MacCatalyst",
        "Mouse down, move, then release inside the SKCanvasView",
        "Click the Button — it does not fire its Clicked event"
      ],
      "environmentDetails": "macOS 12.6, SkiaSharp 2.88.3, Visual Studio for Mac / VS Code, MacCatalyst",
      "repoLinks": [
        {
          "url": "https://github.com/telerik/ms-samples/tree/main/Maui/SKCanvasViewTouchCaptured_MacCatalyst",
          "description": "Minimal MAUI repro project from reporter"
        },
        {
          "url": "https://user-images.githubusercontent.com/61967449/214117627-a77342c7-ebde-433d-8d00-4ead0c74ad28.mov",
          "description": "Screen recording demonstrating the capture bug"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3",
        "2.88.6"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The SKTouchHandler code path in Platform/Apple/SKTouchHandler.cs has not been changed to set UIGestureRecognizerState.Recognized after TouchesEnded — the root cause identified in comments remains present in the current code."
    }
  },
  "analysis": {
    "summary": "SKTouchHandler (a UIGestureRecognizer subclass on Apple platforms) never sets its State to Recognized after TouchesEnded, so UIKit retains mouse capture even after the user releases the button, preventing sibling controls from receiving events.",
    "rationale": "The bug is clearly reproduced with a minimal app, confirmed by multiple users, and the root cause is identifiable directly in source code. SKTouchHandler extends UIGestureRecognizer but never transitions its State to Recognized after handling a complete touch sequence (Pressed → Moved → Released). UIKit's gesture recognizer state machine requires this transition to release input capture. This is MacCatalyst-specific because mouse input routing via UIGestureRecognizer is more strict about state transitions on Mac Catalyst than on touch-only iOS.",
    "keySignals": [
      {
        "text": "Mouse down, then move, then up in the SKCanvasView. Then click the Button - the Button is not clicked.",
        "source": "issue body",
        "interpretation": "Classic symptom of a UIGestureRecognizer retaining mouse capture because its state machine was not completed."
      },
      {
        "text": "In SKTouchHandler.TouchesEnded, FireEvent is called for each subscriber but the UIGestureRecognizer.State is left unchanged. I changed the logic to set State to Recognized if any subscriber sets TouchEventArgs.Handled to true.",
        "source": "comment by DanTravison (#1845661121)",
        "interpretation": "Community member isolated the root cause and confirmed the fix with a custom UIGestureRecognizer."
      },
      {
        "text": "it appears the capture remains in place until I switch away, click on the apps window header, or wait for a while",
        "source": "comment by DanTravison (#1832414576)",
        "interpretation": "Confirms the gesture recognizer is holding capture; external events that cancel gesture tracking release it."
      },
      {
        "text": "When I click the mouse button, I receive a Pressed event and set e.Handled to true. As long as I keep the mouse pointer within the application's Window, the capture is NOT released when the mouse button is released.",
        "source": "comment by DanTravison (#1833990907)",
        "interpretation": "Narrows the trigger to: Handled=true on Pressed + mouse pointer stays within window = capture not released."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs",
        "lines": "77-85",
        "finding": "TouchesEnded calls FireEvent for each touch but never sets this.State = UIGestureRecognizerState.Recognized (or Ended). The UIGestureRecognizer state machine is never explicitly completed, leaving the recognizer in an ambiguous state that retains mouse capture on MacCatalyst.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs",
        "lines": "54-65",
        "finding": "TouchesBegan calls IgnoreTouch when FireEvent returns false (i.e., not handled), but TouchesEnded has no equivalent logic to finalize the gesture recognizer state when handled=true.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandlerProxy.cs",
        "lines": "1-55",
        "finding": "SKTouchHandlerProxy creates and manages SKTouchHandler via SetEnabled; does not intercept or supplement the gesture state machine — the fix must be in SKTouchHandler itself.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Replace SKCanvasView touch handling by subclassing the platform handler and injecting a custom UIGestureRecognizer (modeled on SKTouchHandler) that sets State = UIGestureRecognizerState.Recognized in TouchesEnded when any handler sets e.Handled = true. Community member DanTravison confirmed this resolves the issue."
    ],
    "nextQuestions": [
      "Does the same bug affect SKGLView on MacCatalyst?",
      "Is the fix safe to apply to iOS and tvOS as well, or only MacCatalyst?",
      "Should State be set to Recognized unconditionally in TouchesEnded, or only when Handled=true?"
    ],
    "resolution": {
      "hypothesis": "Setting UIGestureRecognizer.State to Recognized inside TouchesEnded (when at least one subscriber sets Handled=true) will release mouse capture and allow sibling controls to receive events normally.",
      "proposals": [
        {
          "title": "Set State = Recognized in TouchesEnded when handled",
          "description": "In SKTouchHandler.TouchesEnded, after calling FireEvent for all touches, if any touch was handled (FireEvent returned true), set this.State = UIGestureRecognizerState.Recognized to complete the gesture recognizer state machine and release mouse capture.",
          "category": "fix",
          "confidence": 0.88,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Inject custom UIGestureRecognizer via HandlerChanged workaround",
          "description": "As a short-term workaround, create a custom UIGestureRecognizer subclass that replicates SKTouchHandler but correctly sets State = Recognized on TouchesEnded. Inject it via the platform handler's HandlerChanged callback. This avoids changing the library but requires platform-specific MAUI code from the consumer.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Set State = Recognized in TouchesEnded when handled",
      "recommendedReason": "Minimal, targeted fix in the correct location. Confirmed by community analysis to be the root cause. Low risk as it only affects the gesture recognizer lifecycle, not the touch event data itself."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.87,
      "reason": "Root cause is clearly identified in source (SKTouchHandler.TouchesEnded not setting UIGestureRecognizerState.Recognized), confirmed by community member who implemented the fix. The code change is minimal and the fix path is known.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, MAUI views area, macOS platform, reliability tenet, and MAUI partner labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Maui",
          "os/macOS",
          "tenet/reliability",
          "partner/maui"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge root cause, thank community member for investigation, provide workaround and fix direction",
        "risk": "medium",
        "confidence": 0.87,
        "comment": "Thank you for the detailed report and to @DanTravison for narrowing down the root cause.\n\nThe issue is in `SKTouchHandler.TouchesEnded` — after firing the touch event, the `UIGestureRecognizer.State` is never set to `Recognized`. On Mac Catalyst, UIKit requires this state transition to release mouse capture after a complete press/move/release sequence. Without it, the recognizer holds onto the input stream indefinitely.\n\n**Workaround (until this is fixed in the library):**\nYou can replace the default touch handler by injecting a custom `UIGestureRecognizer` through the MAUI platform handler's `HandlerChanged` callback. The custom recognizer should mirror `SKTouchHandler` but add `this.State = UIGestureRecognizerState.Recognized;` at the end of `TouchesEnded` when any subscriber sets `e.Handled = true`. @DanTravison confirmed this resolves the issue.\n\n**Fix direction:**\nAdd `this.State = UIGestureRecognizerState.Recognized;` in `SKTouchHandler.TouchesEnded` when the handled flag is true, completing the gesture recognizer state machine."
      }
    ]
  }
}
```

</details>
