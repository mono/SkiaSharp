# Issue Triage Report — #2768

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-27T22:30:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views.Maui (0.90 (90%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** CaptureAsync on Android MAUI produces a stretched/incorrect image when the view hierarchy contains an SKCanvasView, because SurfaceFactory.DrawSurface uses canvas.Width/Height as the destination rect, which during screen capture is the full capture-target size rather than the view's own size.

**Analysis:** In SurfaceFactory.DrawSurface (source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SurfaceFactory.cs), the destination rect is set to (0, 0, canvas.Width, canvas.Height). During normal rendering, the Android system calls OnDraw with a canvas whose Width/Height match the view dimensions. During MAUI's CaptureAsync, a single larger canvas is created for the whole capture target and child views draw onto it with a translation offset, but canvas.Width/Height still report the full capture canvas size. This causes the SKCanvasView bitmap to be stretched to fill the entire capture region instead of its own pixel bounds.

**Recommendations:** **needs-investigation** — Root cause is identified in SurfaceFactory.DrawSurface. The fix is small but should be validated against multiple Android API levels and confirmed not to break normal rendering.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/Android |
| Backends | backend/Raster |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a MAUI app with a Border (250x250) containing an AbsoluteLayout with an SKCanvasView (100x100, offset 40,40)
2. Add a Button that calls CaptureAsync on the Border
3. Run on Android (13 or 14)
4. Tap the capture button
5. Observe: captured image shows SKCanvasView content stretched to fill the full 250x250 area

**Environment:** SkiaSharp 2.88.3, Android 13/14, Pixel 5 Emulator, Visual Studio (Windows)

**Repository links:**
- https://github.com/Pairoba/CaptureAsyncSkiaSharp — Minimal MAUI repro project provided by reporter

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | SKCanvasView content is scaled to fill the entire capture-target area instead of occupying its proper position and size |
| Repro quality | complete |
| Target frameworks | net8.0-android |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SurfaceFactory.DrawSurface code using canvas.Width/Height as dst has not changed since this report. |

## Analysis

### Technical Summary

In SurfaceFactory.DrawSurface (source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SurfaceFactory.cs), the destination rect is set to (0, 0, canvas.Width, canvas.Height). During normal rendering, the Android system calls OnDraw with a canvas whose Width/Height match the view dimensions. During MAUI's CaptureAsync, a single larger canvas is created for the whole capture target and child views draw onto it with a translation offset, but canvas.Width/Height still report the full capture canvas size. This causes the SKCanvasView bitmap to be stretched to fill the entire capture region instead of its own pixel bounds.

### Rationale

The bug is a clear wrong-output issue on Android caused by SurfaceFactory.DrawSurface incorrectly using canvas.Width/canvas.Height as the blit destination. The reporter identified a workaround (pre-scaling the view) and provided a complete repro. The fix requires using the view's own Width/Height or the canvas clip bounds as the destination rect instead.

### Key Signals

- "The canvas on the snapshot is scaled to the snapshot target's size" — **comment #1 by Pairoba** (Reporter correctly identifies the symptom: the SKCanvasView draws to the capture target's full area.)
- "var dst = new RectF(0, 0, canvas.Width, canvas.Height);" — **source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SurfaceFactory.cs:64** (During CaptureAsync, canvas.Width/Height is the full capture canvas size, not the view's own size.)
- "Works on Windows" — **issue body** (Windows capture path does not use SurfaceFactory or this bitmap blit pattern, confirming this is Android-specific.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SurfaceFactory.cs` | 53-67 | direct | DrawSurface uses canvas.Width/canvas.Height as destination rect. During CaptureAsync, canvas is the full capture-target canvas; Width/Height report the capture container size, not the SKCanvasView's own size. The bitmap (sized to the view) is then stretched over the entire container area. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs` | 77-117 | direct | OnDraw calls surfaceFactory.CreateSurface (which uses the size from OnSizeChanged, i.e., the view's own W/H in pixels) and then DrawSurface. The surface is correctly sized, but DrawSurface stretches it to canvas.Width/canvas.Height. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Android.cs` | 14-14 | related | MAUI handler creates the native SkiaSharp.Views.Android.SKCanvasView, so it inherits the SurfaceFactory blit bug. |

### Workarounds

- Before calling CaptureAsync, scale the SKCanvasView to (Width / CaptureTarget.Width, Height / CaptureTarget.Height) with AnchorX/AnchorY = 0, then restore after capture.

### Next Questions

- Does the same bug affect SKGLView on Android (uses OpenGL, different drawing path)?
- Does this also affect Xamarin.Forms/Legacy Android apps using SurfaceFactory?
- Is the correct fix to use view.Width/view.Height in DrawSurface, or use canvas.ClipBounds?

### Resolution Proposals

**Hypothesis:** During CaptureAsync, Android draws child views onto a shared canvas. The canvas.Width/Height represents the capture target, not the child view. The fix is to use the view's actual dimensions or the canvas clip bounds as the DrawBitmap destination rect.

1. **Use canvas clip bounds as destination rect** — fix, confidence 0.75 (75%), cost/xs, validated=untested
   - In SurfaceFactory.DrawSurface, replace `new RectF(0, 0, canvas.Width, canvas.Height)` with `canvas.ClipBounds` (converted to RectF). During normal rendering ClipBounds == view bounds; during CaptureAsync ClipBounds reflects the child view's clipped area.
2. **Pass view dimensions to DrawSurface** — fix, confidence 0.80 (80%), cost/xs, validated=untested
   - Have SKCanvasView.OnDraw pass its own Width/Height to DrawSurface instead of relying on canvas.Width/Height. This ensures the blit destination always matches the view's layout size regardless of canvas transforms.

**Recommended proposal:** Pass view dimensions to DrawSurface

**Why:** Passing the view's actual pixel dimensions is more explicit and avoids any uncertainty about ClipBounds behavior across API levels.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Root cause is identified in SurfaceFactory.DrawSurface. The fix is small but should be validated against multiple Android API levels and confirmed not to break normal rendering. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, MAUI views, Android, raster labels | labels=type/bug, area/SkiaSharp.Views.Maui, os/Android, backend/Raster, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Acknowledge bug, confirm root cause, thank reporter for workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed repro and the workaround! We've identified the root cause: in `SurfaceFactory.DrawSurface` (Android), the destination rect is set to `(0, 0, canvas.Width, canvas.Height)`. During normal rendering this equals the view's own size, but during MAUI's `CaptureAsync` the shared canvas reports the capture-target dimensions, causing the bitmap to be stretched over the entire capture area.

The fix is to use the view's actual pixel dimensions (or `canvas.ClipBounds`) as the destination rect instead. This is tracked for an upcoming fix.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2768,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-27T22:30:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "CaptureAsync on Android MAUI produces a stretched/incorrect image when the view hierarchy contains an SKCanvasView, because SurfaceFactory.DrawSurface uses canvas.Width/Height as the destination rect, which during screen capture is the full capture-target size rather than the view's own size.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.9
    },
    "platforms": [
      "os/Android"
    ],
    "backends": [
      "backend/Raster"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "SKCanvasView content is scaled to fill the entire capture-target area instead of occupying its proper position and size",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net8.0-android"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a MAUI app with a Border (250x250) containing an AbsoluteLayout with an SKCanvasView (100x100, offset 40,40)",
        "Add a Button that calls CaptureAsync on the Border",
        "Run on Android (13 or 14)",
        "Tap the capture button",
        "Observe: captured image shows SKCanvasView content stretched to fill the full 250x250 area"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, Android 13/14, Pixel 5 Emulator, Visual Studio (Windows)",
      "repoLinks": [
        {
          "url": "https://github.com/Pairoba/CaptureAsyncSkiaSharp",
          "description": "Minimal MAUI repro project provided by reporter"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "SurfaceFactory.DrawSurface code using canvas.Width/Height as dst has not changed since this report."
    }
  },
  "analysis": {
    "summary": "In SurfaceFactory.DrawSurface (source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SurfaceFactory.cs), the destination rect is set to (0, 0, canvas.Width, canvas.Height). During normal rendering, the Android system calls OnDraw with a canvas whose Width/Height match the view dimensions. During MAUI's CaptureAsync, a single larger canvas is created for the whole capture target and child views draw onto it with a translation offset, but canvas.Width/Height still report the full capture canvas size. This causes the SKCanvasView bitmap to be stretched to fill the entire capture region instead of its own pixel bounds.",
    "rationale": "The bug is a clear wrong-output issue on Android caused by SurfaceFactory.DrawSurface incorrectly using canvas.Width/canvas.Height as the blit destination. The reporter identified a workaround (pre-scaling the view) and provided a complete repro. The fix requires using the view's own Width/Height or the canvas clip bounds as the destination rect instead.",
    "keySignals": [
      {
        "text": "The canvas on the snapshot is scaled to the snapshot target's size",
        "source": "comment #1 by Pairoba",
        "interpretation": "Reporter correctly identifies the symptom: the SKCanvasView draws to the capture target's full area."
      },
      {
        "text": "var dst = new RectF(0, 0, canvas.Width, canvas.Height);",
        "source": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SurfaceFactory.cs:64",
        "interpretation": "During CaptureAsync, canvas.Width/Height is the full capture canvas size, not the view's own size."
      },
      {
        "text": "Works on Windows",
        "source": "issue body",
        "interpretation": "Windows capture path does not use SurfaceFactory or this bitmap blit pattern, confirming this is Android-specific."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SurfaceFactory.cs",
        "lines": "53-67",
        "finding": "DrawSurface uses canvas.Width/canvas.Height as destination rect. During CaptureAsync, canvas is the full capture-target canvas; Width/Height report the capture container size, not the SKCanvasView's own size. The bitmap (sized to the view) is then stretched over the entire container area.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs",
        "lines": "77-117",
        "finding": "OnDraw calls surfaceFactory.CreateSurface (which uses the size from OnSizeChanged, i.e., the view's own W/H in pixels) and then DrawSurface. The surface is correctly sized, but DrawSurface stretches it to canvas.Width/canvas.Height.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Android.cs",
        "lines": "14-14",
        "finding": "MAUI handler creates the native SkiaSharp.Views.Android.SKCanvasView, so it inherits the SurfaceFactory blit bug.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Before calling CaptureAsync, scale the SKCanvasView to (Width / CaptureTarget.Width, Height / CaptureTarget.Height) with AnchorX/AnchorY = 0, then restore after capture."
    ],
    "nextQuestions": [
      "Does the same bug affect SKGLView on Android (uses OpenGL, different drawing path)?",
      "Does this also affect Xamarin.Forms/Legacy Android apps using SurfaceFactory?",
      "Is the correct fix to use view.Width/view.Height in DrawSurface, or use canvas.ClipBounds?"
    ],
    "resolution": {
      "hypothesis": "During CaptureAsync, Android draws child views onto a shared canvas. The canvas.Width/Height represents the capture target, not the child view. The fix is to use the view's actual dimensions or the canvas clip bounds as the DrawBitmap destination rect.",
      "proposals": [
        {
          "title": "Use canvas clip bounds as destination rect",
          "description": "In SurfaceFactory.DrawSurface, replace `new RectF(0, 0, canvas.Width, canvas.Height)` with `canvas.ClipBounds` (converted to RectF). During normal rendering ClipBounds == view bounds; during CaptureAsync ClipBounds reflects the child view's clipped area.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Pass view dimensions to DrawSurface",
          "description": "Have SKCanvasView.OnDraw pass its own Width/Height to DrawSurface instead of relying on canvas.Width/Height. This ensures the blit destination always matches the view's layout size regardless of canvas transforms.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Pass view dimensions to DrawSurface",
      "recommendedReason": "Passing the view's actual pixel dimensions is more explicit and avoids any uncertainty about ClipBounds behavior across API levels."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Root cause is identified in SurfaceFactory.DrawSurface. The fix is small but should be validated against multiple Android API levels and confirmed not to break normal rendering.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, MAUI views, Android, raster labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Maui",
          "os/Android",
          "backend/Raster",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge bug, confirm root cause, thank reporter for workaround",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed repro and the workaround! We've identified the root cause: in `SurfaceFactory.DrawSurface` (Android), the destination rect is set to `(0, 0, canvas.Width, canvas.Height)`. During normal rendering this equals the view's own size, but during MAUI's `CaptureAsync` the shared canvas reports the capture-target dimensions, causing the bitmap to be stretched over the entire capture area.\n\nThe fix is to use the view's actual pixel dimensions (or `canvas.ClipBounds`) as the destination rect instead. This is tracked for an upcoming fix."
      }
    ]
  }
}
```

</details>
