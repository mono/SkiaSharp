# Issue Triage Report — #315

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T14:17:00Z |
| Type | type/bug (0.75 (75%)) |
| Area | area/SkiaSharp.Views.Forms (0.85 (85%)) |
| Suggested action | needs-info (0.75 (75%)) |

**Issue Summary:** Reporter says Pinch and Pan touch gestures work on iOS but not on Android in a Xamarin.Forms app using the SkiaSharp touch effect from the FingerPaint demo.

**Analysis:** Android touch event consumption conflict: the SkiaSharp touch effect on Android uses View.Touch, and whether the event is marked Handled determines whether Android gesture recognizers (pan/pinch) also receive the touch stream. If the touch event is consumed by SkiaSharp, the Xamarin.Forms GestureRecognizers never fire; if not consumed, the native gesture recognizers intercept the stream and the drawing breaks. This is a fundamental Android architecture tension, not a simple bug. The Xamarin.Forms stack has since been deprecated in favor of MAUI.

**Recommendations:** **needs-info** — No version numbers, no inline repro steps, no stack trace. The affected library (SkiaSharp.Views.Forms) is deprecated. Need confirmation whether this is still relevant in MAUI and what exact SkiaSharp version was used.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Forms |
| Platforms | os/Android |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Use SkiaSharp FingerPaint touch effect in Xamarin.Forms
2. Add Pinch and Pan gesture code on top
3. Run on Android — gesture does not work
4. Run on iOS — gesture works

**Environment:** Xamarin.Forms, Android vs iOS. No version numbers provided.

**Related issues:** #309, #826, #909

**Repository links:**
- https://github.com/JavedAppdevelopment/DrawTrackingForms/tree/master/DrawTrarkingForms — Reporter's reproduction repository

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | platform-specific |
| Error message | — |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | The issue targets Xamarin.Forms which is deprecated; SkiaSharp.Views.Forms has been removed from the repo and replaced by SkiaSharp.Views.Maui. No version numbers were provided. |

## Analysis

### Technical Summary

Android touch event consumption conflict: the SkiaSharp touch effect on Android uses View.Touch, and whether the event is marked Handled determines whether Android gesture recognizers (pan/pinch) also receive the touch stream. If the touch event is consumed by SkiaSharp, the Xamarin.Forms GestureRecognizers never fire; if not consumed, the native gesture recognizers intercept the stream and the drawing breaks. This is a fundamental Android architecture tension, not a simple bug. The Xamarin.Forms stack has since been deprecated in favor of MAUI.

### Rationale

Platform-specific behavior difference (iOS works, Android does not) points to a real bug or architectural limitation. The comment from nor0x in 2019 corroborates the issue persists, referencing #309 TouchEvents interfering with Xamarin.Forms GestureRecognizers specifically on Android. Code investigation shows the Android SKTouchHandler sets e.Handled from args.Handled — creating the conflict. Issue lacks version, repro steps, and exact error, and the affected library (SkiaSharp.Views.Forms) has been removed from the codebase.

### Key Signals

- "Pinch and Pan with Touch works in iOS version but not work in Android version" — **issue body** (Platform-specific failure — likely Android touch event consumption/interception difference.)
- "#309 introduces TouchEvents but somehow they interfere with Xamarin.Forms GestureRecognizers on Android" — **comment by nor0x (2019)** (Corroborates the issue; SKTouchEffect consumes touch events, preventing XF GestureRecognizers from firing on Android.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs` | 41-110 | direct | The MAUI Android SKTouchHandler sets e.Handled = args.Handled for each touch action. If the consumer marks the touch as handled, Android gesture recognizers won't fire; if not handled, SkiaSharp's touch tracking can be interrupted by gesture recognizers. The same pattern existed in the legacy Xamarin.Forms handler. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs` | 20-29 | direct | SetEnabled hooks View.Touch event. Android's touch dispatch system means View.Touch competes with GestureDetectors attached at higher levels. There is no RequestDisallowInterceptTouchEvent call, which means parent ViewGroups can still intercept touch events during scrolling/panning. |

### Workarounds

- In MAUI (replacement for Xamarin.Forms): handle the touch events via SKTouchEventArgs and implement pinch/pan manually using multi-touch point tracking instead of relying on platform GestureRecognizers.
- Call parent.RequestDisallowInterceptTouchEvent(true) from a custom Android renderer to prevent parent ViewGroups from intercepting mid-gesture.

### Next Questions

- Which version of SkiaSharp was the reporter using?
- Does the same issue reproduce in SkiaSharp.Views.Maui on Android?
- Is the reporter willing to migrate to MAUI since Xamarin.Forms is now deprecated?

### Resolution Proposals

**Hypothesis:** Android touch dispatch prevents simultaneous use of SkiaSharp's View.Touch handler and Xamarin.Forms GestureRecognizers. The SKTouchEffect consumes events so gesture recognizers never receive them, or vice versa.

1. **Implement pinch/pan from raw touch events** — workaround, confidence 0.80 (80%), cost/m, validated=untested
   - Rather than using Xamarin.Forms PinchGestureRecognizer/PanGestureRecognizer alongside SkiaSharp touch, implement the gesture detection manually by tracking multiple touch points in the SKTouch handler. Compute scale/translation from pointer distance changes.
2. **Migrate to MAUI** — alternative, confidence 0.90 (90%), cost/l, validated=untested
   - Xamarin.Forms is deprecated. Migrate to .NET MAUI with SkiaSharp.Views.Maui, where the same touch architecture exists but the platform is actively maintained.

**Recommended proposal:** Implement pinch/pan from raw touch events

**Why:** Directly solves the gesture conflict by bypassing platform gesture recognizers entirely; implementable without migrating the whole app.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.75 (75%) |
| Reason | No version numbers, no inline repro steps, no stack trace. The affected library (SkiaSharp.Views.Forms) is deprecated. Need confirmation whether this is still relevant in MAUI and what exact SkiaSharp version was used. |
| Suggested repro platform | linux |

### Missing Info

- SkiaSharp NuGet version used
- Xamarin.Forms version
- Android target API level
- Exact device/emulator type
- Whether this is reproducible in current SkiaSharp.Views.Maui on .NET MAUI

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Apply bug, Views.Forms, and Android labels | labels=type/bug, area/SkiaSharp.Views.Forms, os/Android, tenet/compatibility |
| add-comment | medium | 0.75 (75%) | Ask for version info and note Xamarin.Forms deprecation; suggest workaround | — |
| link-related | low | 0.80 (80%) | Link to related issue #909 (same touch/gesture conflict on iOS in XF) | linkedIssue=#909 |

**Comment draft for `add-comment`:**

```markdown
Thanks for reporting this! The conflict between SkiaSharp's touch effect and Xamarin.Forms gesture recognizers on Android is a known architectural tension — Android's touch dispatch allows only one consumer per touch stream.

Could you please provide:
- SkiaSharp NuGet version
- Xamarin.Forms version
- Android target API level

As a **workaround**, instead of using `PinchGestureRecognizer`/`PanGestureRecognizer` alongside the SkiaSharp touch effect, consider tracking multiple touch points directly in your `SKTouch` handler and computing the scale/translation manually from pointer position deltas.

Also note that **Xamarin.Forms is now deprecated** in favor of .NET MAUI. If you migrate to MAUI with `SkiaSharp.Views.Maui`, the same workaround applies, but you'll be on a supported platform.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 315,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T14:17:00Z"
  },
  "summary": "Reporter says Pinch and Pan touch gestures work on iOS but not on Android in a Xamarin.Forms app using the SkiaSharp touch effect from the FingerPaint demo.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.75
    },
    "area": {
      "value": "area/SkiaSharp.Views.Forms",
      "confidence": 0.85
    },
    "platforms": [
      "os/Android"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "low",
      "regressionClaimed": false,
      "errorType": "platform-specific",
      "reproQuality": "partial"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Use SkiaSharp FingerPaint touch effect in Xamarin.Forms",
        "Add Pinch and Pan gesture code on top",
        "Run on Android — gesture does not work",
        "Run on iOS — gesture works"
      ],
      "environmentDetails": "Xamarin.Forms, Android vs iOS. No version numbers provided.",
      "repoLinks": [
        {
          "url": "https://github.com/JavedAppdevelopment/DrawTrackingForms/tree/master/DrawTrarkingForms",
          "description": "Reporter's reproduction repository"
        }
      ],
      "relatedIssues": [
        309,
        826,
        909
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "unlikely",
      "relevanceReason": "The issue targets Xamarin.Forms which is deprecated; SkiaSharp.Views.Forms has been removed from the repo and replaced by SkiaSharp.Views.Maui. No version numbers were provided."
    }
  },
  "analysis": {
    "summary": "Android touch event consumption conflict: the SkiaSharp touch effect on Android uses View.Touch, and whether the event is marked Handled determines whether Android gesture recognizers (pan/pinch) also receive the touch stream. If the touch event is consumed by SkiaSharp, the Xamarin.Forms GestureRecognizers never fire; if not consumed, the native gesture recognizers intercept the stream and the drawing breaks. This is a fundamental Android architecture tension, not a simple bug. The Xamarin.Forms stack has since been deprecated in favor of MAUI.",
    "rationale": "Platform-specific behavior difference (iOS works, Android does not) points to a real bug or architectural limitation. The comment from nor0x in 2019 corroborates the issue persists, referencing #309 TouchEvents interfering with Xamarin.Forms GestureRecognizers specifically on Android. Code investigation shows the Android SKTouchHandler sets e.Handled from args.Handled — creating the conflict. Issue lacks version, repro steps, and exact error, and the affected library (SkiaSharp.Views.Forms) has been removed from the codebase.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs",
        "lines": "41-110",
        "finding": "The MAUI Android SKTouchHandler sets e.Handled = args.Handled for each touch action. If the consumer marks the touch as handled, Android gesture recognizers won't fire; if not handled, SkiaSharp's touch tracking can be interrupted by gesture recognizers. The same pattern existed in the legacy Xamarin.Forms handler.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs",
        "lines": "20-29",
        "finding": "SetEnabled hooks View.Touch event. Android's touch dispatch system means View.Touch competes with GestureDetectors attached at higher levels. There is no RequestDisallowInterceptTouchEvent call, which means parent ViewGroups can still intercept touch events during scrolling/panning.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Pinch and Pan with Touch works in iOS version but not work in Android version",
        "source": "issue body",
        "interpretation": "Platform-specific failure — likely Android touch event consumption/interception difference."
      },
      {
        "text": "#309 introduces TouchEvents but somehow they interfere with Xamarin.Forms GestureRecognizers on Android",
        "source": "comment by nor0x (2019)",
        "interpretation": "Corroborates the issue; SKTouchEffect consumes touch events, preventing XF GestureRecognizers from firing on Android."
      }
    ],
    "workarounds": [
      "In MAUI (replacement for Xamarin.Forms): handle the touch events via SKTouchEventArgs and implement pinch/pan manually using multi-touch point tracking instead of relying on platform GestureRecognizers.",
      "Call parent.RequestDisallowInterceptTouchEvent(true) from a custom Android renderer to prevent parent ViewGroups from intercepting mid-gesture."
    ],
    "nextQuestions": [
      "Which version of SkiaSharp was the reporter using?",
      "Does the same issue reproduce in SkiaSharp.Views.Maui on Android?",
      "Is the reporter willing to migrate to MAUI since Xamarin.Forms is now deprecated?"
    ],
    "resolution": {
      "hypothesis": "Android touch dispatch prevents simultaneous use of SkiaSharp's View.Touch handler and Xamarin.Forms GestureRecognizers. The SKTouchEffect consumes events so gesture recognizers never receive them, or vice versa.",
      "proposals": [
        {
          "title": "Implement pinch/pan from raw touch events",
          "description": "Rather than using Xamarin.Forms PinchGestureRecognizer/PanGestureRecognizer alongside SkiaSharp touch, implement the gesture detection manually by tracking multiple touch points in the SKTouch handler. Compute scale/translation from pointer distance changes.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Migrate to MAUI",
          "description": "Xamarin.Forms is deprecated. Migrate to .NET MAUI with SkiaSharp.Views.Maui, where the same touch architecture exists but the platform is actively maintained.",
          "category": "alternative",
          "confidence": 0.9,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Implement pinch/pan from raw touch events",
      "recommendedReason": "Directly solves the gesture conflict by bypassing platform gesture recognizers entirely; implementable without migrating the whole app."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.75,
      "reason": "No version numbers, no inline repro steps, no stack trace. The affected library (SkiaSharp.Views.Forms) is deprecated. Need confirmation whether this is still relevant in MAUI and what exact SkiaSharp version was used.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "SkiaSharp NuGet version used",
      "Xamarin.Forms version",
      "Android target API level",
      "Exact device/emulator type",
      "Whether this is reproducible in current SkiaSharp.Views.Maui on .NET MAUI"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, Views.Forms, and Android labels",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Forms",
          "os/Android",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask for version info and note Xamarin.Forms deprecation; suggest workaround",
        "risk": "medium",
        "confidence": 0.75,
        "comment": "Thanks for reporting this! The conflict between SkiaSharp's touch effect and Xamarin.Forms gesture recognizers on Android is a known architectural tension — Android's touch dispatch allows only one consumer per touch stream.\n\nCould you please provide:\n- SkiaSharp NuGet version\n- Xamarin.Forms version\n- Android target API level\n\nAs a **workaround**, instead of using `PinchGestureRecognizer`/`PanGestureRecognizer` alongside the SkiaSharp touch effect, consider tracking multiple touch points directly in your `SKTouch` handler and computing the scale/translation manually from pointer position deltas.\n\nAlso note that **Xamarin.Forms is now deprecated** in favor of .NET MAUI. If you migrate to MAUI with `SkiaSharp.Views.Maui`, the same workaround applies, but you'll be on a supported platform."
      },
      {
        "type": "link-related",
        "description": "Link to related issue #909 (same touch/gesture conflict on iOS in XF)",
        "risk": "low",
        "confidence": 0.8,
        "linkedIssue": 909
      }
    ]
  }
}
```

</details>
