# Issue Triage Report — #1538

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-29T11:50:00Z |
| Type | type/question (0.97 (97%)) |
| Area | area/SkiaSharp.Views (0.92 (92%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** User asks whether SKCanvasView supports a smaller internal canvas resolution stretched to fill the screen, to improve performance on high-DPI iPads where the default 2048x1276 physical canvas is too slow.

**Analysis:** The reporter wants to reduce the SKCanvasView canvas resolution to improve performance on high-DPI iPads. The IgnorePixelScaling property already exists and answers this question: setting it to true causes the canvas to render at logical point size (half the physical pixels on 2x Retina), then scale up — exactly the quarter-resolution the reporter is seeking.

**Recommendations:** **close-as-not-a-bug** — Usage question answered by the existing IgnorePixelScaling API. No bug present.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp.Views |
| Platforms | os/iOS |
| Backends | — |
| Tenets | tenet/performance |
| Partner | — |

## Evidence

### Reproduction

1. Use SKCanvasView filling most of the screen on an iPad Mini 4
2. Draw lines, text, and circles with gesture-based scaling/rotation
3. Observe significant lag (~15-20fps) vs iPhone 6 (~60fps) due to larger canvas size

**Environment:** iPad Mini 4 (~15-20fps at 2048x1276), iPhone 6 (~60fps at 750x1086). Platform: iOS/Xamarin.Forms. SkiaSharp version not specified.

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | The IgnorePixelScaling property already exists and answers this question. The feature has been present for a long time. |

## Analysis

### Technical Summary

The reporter wants to reduce the SKCanvasView canvas resolution to improve performance on high-DPI iPads. The IgnorePixelScaling property already exists and answers this question: setting it to true causes the canvas to render at logical point size (half the physical pixels on 2x Retina), then scale up — exactly the quarter-resolution the reporter is seeking.

### Rationale

Issue title and body both frame this as a question ('Can we set...?', 'Is that possible?'). The IgnorePixelScaling property on SKCanvasView exists in both iOS and Android implementations and does exactly what the reporter needs. The reporter tried ContentScaleFactor on the UIView (wrong approach) but missed the IgnorePixelScaling property on the SKCanvasView itself.

### Key Signals

- "Can I set the SKCanvasView to fill screen, but say have an internal canvas of 1024x638 (same aspect ratio, but quarter the resolution)?" — **issue body** (Direct question — the reporter wants IgnorePixelScaling=true behavior, just doesn't know the API.)
- "I've tried out ContentScaleFactor on the view to see if that would have an effect, but the SKCanvas size is always of the raw device pixel size regardless of the ContentScaleFactor value." — **comment #1** (Reporter tried the wrong property (UIView's ContentScaleFactor) instead of SKCanvasView.IgnorePixelScaling.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKCanvasView.cs` | 69-77 | direct | IgnorePixelScaling property exists. When true, CanvasSize is set to logical point size (Bounds.Width/Height) instead of physical pixels, and the canvas is scaled by ContentScaleFactor. This reduces a 2x Retina iPad's canvas from 2048x1276 to 1024x638. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs` | 66-75 | related | Same IgnorePixelScaling property on Android — when true, canvas size is reported in density-independent pixels (dp) and content is scaled by display density. Confirms the feature is cross-platform. |

### Workarounds

- Set IgnorePixelScaling = true on SKCanvasView. The canvas will render at logical point size (e.g. 1024x638 on a 2x Retina iPad instead of 2048x1276). Adjust your paint stroke widths and text sizes by ContentScaleFactor (2.0) if needed.

### Resolution Proposals

**Hypothesis:** The IgnorePixelScaling property on SKCanvasView already exists and does exactly what the reporter needs.

1. **Use IgnorePixelScaling = true** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Set IgnorePixelScaling to true on the SKCanvasView. On a 2x Retina iPad Mini 4, this reduces the canvas from 2048x1276 physical pixels to 1024x638 logical points — the exact quarter-resolution the reporter is asking for. The drawn content is then scaled up to fill the screen by the system.

**Recommended proposal:** Use IgnorePixelScaling = true

**Why:** Direct answer to the question. One property change, no custom rendering pipeline needed.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | Usage question answered by the existing IgnorePixelScaling API. No bug present. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply question and views labels | labels=type/question, area/SkiaSharp.Views, os/iOS, tenet/performance |
| add-comment | medium | 0.92 (92%) | Answer with IgnorePixelScaling solution | — |
| close-issue | medium | 0.85 (85%) | Close as answered | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Yes, this is possible using the `IgnorePixelScaling` property on `SKCanvasView`.

Set `IgnorePixelScaling = true` on your view. When enabled, the canvas is created at the logical point size (UIKit points, not physical pixels), so on a 2× Retina iPad Mini 4 your 2048×1276 physical canvas becomes a 1024×638 logical canvas — exactly the quarter-resolution you're after. The drawn content is then scaled up to fill the screen by the rendering pipeline.

```csharp
// In your page/view setup:
mySkCanvasView.IgnorePixelScaling = true;
```

Note that `CanvasSize` reported in `PaintSurface` will reflect the logical size (1024×638), so any sizing/scaling logic you already have based on `CanvasSize` should continue to work without changes.

The reason setting `ContentScaleFactor` directly on the view didn't help is that `SKCanvasView` reads that value internally to create the surface at full resolution — `IgnorePixelScaling` is the knob to opt out of that behavior.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1538,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-29T11:50:00Z"
  },
  "summary": "User asks whether SKCanvasView supports a smaller internal canvas resolution stretched to fill the screen, to improve performance on high-DPI iPads where the default 2048x1276 physical canvas is too slow.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.92
    },
    "platforms": [
      "os/iOS"
    ],
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "iPad Mini 4 (~15-20fps at 2048x1276), iPhone 6 (~60fps at 750x1086). Platform: iOS/Xamarin.Forms. SkiaSharp version not specified.",
      "stepsToReproduce": [
        "Use SKCanvasView filling most of the screen on an iPad Mini 4",
        "Draw lines, text, and circles with gesture-based scaling/rotation",
        "Observe significant lag (~15-20fps) vs iPhone 6 (~60fps) due to larger canvas size"
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "unlikely",
      "relevanceReason": "The IgnorePixelScaling property already exists and answers this question. The feature has been present for a long time."
    }
  },
  "analysis": {
    "summary": "The reporter wants to reduce the SKCanvasView canvas resolution to improve performance on high-DPI iPads. The IgnorePixelScaling property already exists and answers this question: setting it to true causes the canvas to render at logical point size (half the physical pixels on 2x Retina), then scale up — exactly the quarter-resolution the reporter is seeking.",
    "rationale": "Issue title and body both frame this as a question ('Can we set...?', 'Is that possible?'). The IgnorePixelScaling property on SKCanvasView exists in both iOS and Android implementations and does exactly what the reporter needs. The reporter tried ContentScaleFactor on the UIView (wrong approach) but missed the IgnorePixelScaling property on the SKCanvasView itself.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKCanvasView.cs",
        "lines": "69-77",
        "finding": "IgnorePixelScaling property exists. When true, CanvasSize is set to logical point size (Bounds.Width/Height) instead of physical pixels, and the canvas is scaled by ContentScaleFactor. This reduces a 2x Retina iPad's canvas from 2048x1276 to 1024x638.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs",
        "lines": "66-75",
        "finding": "Same IgnorePixelScaling property on Android — when true, canvas size is reported in density-independent pixels (dp) and content is scaled by display density. Confirms the feature is cross-platform.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Can I set the SKCanvasView to fill screen, but say have an internal canvas of 1024x638 (same aspect ratio, but quarter the resolution)?",
        "source": "issue body",
        "interpretation": "Direct question — the reporter wants IgnorePixelScaling=true behavior, just doesn't know the API."
      },
      {
        "text": "I've tried out ContentScaleFactor on the view to see if that would have an effect, but the SKCanvas size is always of the raw device pixel size regardless of the ContentScaleFactor value.",
        "source": "comment #1",
        "interpretation": "Reporter tried the wrong property (UIView's ContentScaleFactor) instead of SKCanvasView.IgnorePixelScaling."
      }
    ],
    "workarounds": [
      "Set IgnorePixelScaling = true on SKCanvasView. The canvas will render at logical point size (e.g. 1024x638 on a 2x Retina iPad instead of 2048x1276). Adjust your paint stroke widths and text sizes by ContentScaleFactor (2.0) if needed."
    ],
    "resolution": {
      "hypothesis": "The IgnorePixelScaling property on SKCanvasView already exists and does exactly what the reporter needs.",
      "proposals": [
        {
          "title": "Use IgnorePixelScaling = true",
          "description": "Set IgnorePixelScaling to true on the SKCanvasView. On a 2x Retina iPad Mini 4, this reduces the canvas from 2048x1276 physical pixels to 1024x638 logical points — the exact quarter-resolution the reporter is asking for. The drawn content is then scaled up to fill the screen by the system.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Use IgnorePixelScaling = true",
      "recommendedReason": "Direct answer to the question. One property change, no custom rendering pipeline needed."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "Usage question answered by the existing IgnorePixelScaling API. No bug present.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and views labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/question",
          "area/SkiaSharp.Views",
          "os/iOS",
          "tenet/performance"
        ]
      },
      {
        "type": "add-comment",
        "description": "Answer with IgnorePixelScaling solution",
        "risk": "medium",
        "confidence": 0.92,
        "comment": "Yes, this is possible using the `IgnorePixelScaling` property on `SKCanvasView`.\n\nSet `IgnorePixelScaling = true` on your view. When enabled, the canvas is created at the logical point size (UIKit points, not physical pixels), so on a 2× Retina iPad Mini 4 your 2048×1276 physical canvas becomes a 1024×638 logical canvas — exactly the quarter-resolution you're after. The drawn content is then scaled up to fill the screen by the rendering pipeline.\n\n```csharp\n// In your page/view setup:\nmySkCanvasView.IgnorePixelScaling = true;\n```\n\nNote that `CanvasSize` reported in `PaintSurface` will reflect the logical size (1024×638), so any sizing/scaling logic you already have based on `CanvasSize` should continue to work without changes.\n\nThe reason setting `ContentScaleFactor` directly on the view didn't help is that `SKCanvasView` reads that value internally to create the surface at full resolution — `IgnorePixelScaling` is the knob to opt out of that behavior."
      },
      {
        "type": "close-issue",
        "description": "Close as answered",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
