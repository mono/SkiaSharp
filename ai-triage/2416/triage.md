# Issue Triage Report — #2416

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T12:30:00Z |
| Type | type/bug (0.82 (82%)) |
| Area | area/SkiaSharp.Views (0.92 (92%)) |
| Suggested action | needs-investigation (0.78 (78%)) |

**Issue Summary:** SKCanvasView renders distorted when captured via view.draw(canvas) on Android API < 26 because SurfaceFactory.DrawSurface uses canvas.Width/Height (parent bitmap dimensions) instead of the view's own pixel dimensions for the destination rect.

**Analysis:** The bug is in SurfaceFactory.DrawSurface (Android): it computes `dst = new RectF(0, 0, canvas.Width, canvas.Height)`. For hardware-accelerated rendering, the Android RecordingCanvas.getWidth() returns the view's allocated width, matching Info.Width. But for software bitmap canvases (used in view.draw(canvas) screenshot capture), canvas.Width returns the underlying bitmap width — which is the root/parent view's full width, not the SKCanvasView's width. This causes the Skia bitmap (sized to SKCanvasView's pixel dimensions) to be stretched to the parent's full dimensions, producing visible distortion.

**Recommendations:** **needs-investigation** — Root cause identified in SurfaceFactory.DrawSurface: dst rect uses canvas.Width/Height (parent bitmap size) instead of bitmap.Width/Height (view size). A minimal repro is needed to confirm the hypothesis and validate the one-line fix before merging.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Android |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create an Android app targeting API < 26 with a layout containing one or more SKCanvasView children
2. Capture a screenshot of the parent view using view.draw(canvas) with a bitmap canvas sized to view.width x view.height
3. Observe that standard views render correctly but SKCanvasView content appears distorted/stretched

**Environment:** SkiaSharp 2.80.3, Android API 31 (target), API < 26 devices affected, Visual Studio

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2416 — Original issue with before/after screenshots showing distortion

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Distorted/stretched SKCanvasView content in screenshot bitmap |
| Repro quality | partial |
| Target frameworks | net-android |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SurfaceFactory.DrawSurface still uses canvas.Width/canvas.Height for the destination rect in current code. |

## Analysis

### Technical Summary

The bug is in SurfaceFactory.DrawSurface (Android): it computes `dst = new RectF(0, 0, canvas.Width, canvas.Height)`. For hardware-accelerated rendering, the Android RecordingCanvas.getWidth() returns the view's allocated width, matching Info.Width. But for software bitmap canvases (used in view.draw(canvas) screenshot capture), canvas.Width returns the underlying bitmap width — which is the root/parent view's full width, not the SKCanvasView's width. This causes the Skia bitmap (sized to SKCanvasView's pixel dimensions) to be stretched to the parent's full dimensions, producing visible distortion.

### Rationale

Reporter provides clear before/after screenshots and describes an Android-standard technique (view.draw(canvas)) that works for all native views but breaks for SKCanvasView. Code investigation confirms the dst rect in DrawSurface uses canvas.Width (parent bitmap width in software rendering) rather than the view's own Info.Width. This is a real defect in the library, not user error.

### Key Signals

- "For devices with API Level < 26 I am forced to use Canvas... This works OK for most of the views I use it with, but there are a couple of them that do not render correctly." — **issue body** (Standard Android screenshot technique works for native views but fails for SKCanvasView, pointing to SKCanvasView-specific behavior.)
- "Hi, no, I didn't find a fix/workaround yet." — **comment by reporter (2024-03-25)** (Issue remains unresolved, no known workaround from the reporter.)
- "todos tus componentes que quieras capturar en imagen deben estar en el mismo lienzo... debes de realizar la captura con la misma nuget o libreria canvas" — **comment by DiabloDaniel (2024-04-18)** (Workaround: keep all captured content within a single Skia canvas instead of using native screenshot via view.draw.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SurfaceFactory.cs` | 53-68 | direct | DrawSurface computes `dst = new RectF(0, 0, canvas.Width, canvas.Height)`. For a software bitmap canvas (screenshot), canvas.Width returns the backing bitmap's width (the parent view's width), not the SKCanvasView's own width. This causes the Skia bitmap (Info.Width x Info.Height = view pixel dimensions) to be scaled to the full parent dimensions before being clipped to the view's bounds, producing a zoomed-in/distorted result. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs` | 77-117 | direct | OnDraw passes the Android canvas (which may be a translated/clipped software canvas in screenshot scenarios) directly to surfaceFactory.DrawSurface. The view itself does not pass its own pixel dimensions to DrawSurface, leaving the method reliant on canvas.Width/Height which are unreliable for child views. |

### Workarounds

- Capture each SKCanvasView individually with getBitmapFromView(skCanvasView) rather than the parent view, then compose the final bitmap manually.
- Export content directly from the Skia surface before DrawSurface: in a custom SKCanvasView subclass, save the SKBitmap during OnPaintSurface and read it back for screenshot composition.
- Keep all content to screenshot within a single SKCanvasView PaintSurface handler so the screenshot can be taken from the Skia surface directly, avoiding view.draw(canvas) entirely (approach described in comment by DiabloDaniel).

### Next Questions

- Does the same issue affect SKGLSurfaceView or SKGLTextureView when captured via view.draw(canvas)?
- Is the bug present only when the SKCanvasView is a child (not the root capture target), or also when it is the root view?

### Resolution Proposals

**Hypothesis:** In SurfaceFactory.DrawSurface, replacing canvas.Width/canvas.Height with bitmap.Width/bitmap.Height (which equals Info.Width/Info.Height) for the destination rect would use the view's actual pixel dimensions, matching hardware-canvas behavior and producing a correct 1:1 copy in the software-canvas screenshot case.

1. **Fix DrawSurface destination rect to use bitmap dimensions** — fix, confidence 0.78 (78%), cost/xs, validated=untested
   - Change the dst rect in SurfaceFactory.DrawSurface from canvas.Width/canvas.Height to bitmap.Width/bitmap.Height. This matches the view's own pixel dimensions and is consistent with hardware-canvas behavior.

```csharp
// Before:
var dst = new RectF(0, 0, canvas.Width, canvas.Height);
// After:
var dst = new RectF(0, 0, bitmap.Width, bitmap.Height);
```
2. **Workaround: capture SKCanvasView directly** — workaround, confidence 0.85 (85%), cost/s, validated=untested
   - Instead of capturing the parent view, call getBitmapFromView on the SKCanvasView itself and stitch bitmaps together. This bypasses the canvas dimension mismatch.

**Recommended proposal:** Fix DrawSurface destination rect to use bitmap dimensions

**Why:** One-line fix in SkiaSharp that restores correct behavior without requiring reporter to change their screenshot architecture.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.78 (78%) |
| Reason | Root cause identified in SurfaceFactory.DrawSurface: dst rect uses canvas.Width/Height (parent bitmap size) instead of bitmap.Width/Height (view size). A minimal repro is needed to confirm the hypothesis and validate the one-line fix before merging. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, views, Android, and compatibility labels | labels=type/bug, area/SkiaSharp.Views, os/Android, tenet/compatibility |
| add-comment | medium | 0.78 (78%) | Explain root cause and provide workarounds | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed screenshots! This appears to be a bug in `SurfaceFactory.DrawSurface` on Android.

**Root cause:** When `view.draw(canvas)` is called on a parent view, the Android system translates and clips the canvas before calling each child's `OnDraw`. However, `canvas.Width` / `canvas.Height` on a software bitmap canvas always returns the *backing bitmap's* dimensions (the parent view's size), not the child view's size. `SKCanvasView` uses `canvas.Width/Height` to set the destination rect when blitting the Skia bitmap, so the content is stretched to the full parent dimensions before being clipped — producing the distortion you see.

**Workarounds while a fix is investigated:**
1. **Capture each `SKCanvasView` individually** — call `getBitmapFromView(skCanvasView)` on each `SKCanvasView` directly, then manually compose the final screenshot bitmap.
2. **Move all rendered content into a single `SKCanvasView`** — draw all Skia content in one `PaintSurface` handler and take the screenshot from there, avoiding `view.draw()` altogether.
3. **Use `PixelCopy` (API ≥ 26)** — you're already doing this; consider dropping support for API < 26 if feasible.

For the fix, changing `DrawSurface` to use `bitmap.Width/bitmap.Height` instead of `canvas.Width/canvas.Height` should resolve the issue.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2416,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T12:30:00Z"
  },
  "summary": "SKCanvasView renders distorted when captured via view.draw(canvas) on Android API < 26 because SurfaceFactory.DrawSurface uses canvas.Width/Height (parent bitmap dimensions) instead of the view's own pixel dimensions for the destination rect.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.82
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.92
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
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "Distorted/stretched SKCanvasView content in screenshot bitmap",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net-android"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an Android app targeting API < 26 with a layout containing one or more SKCanvasView children",
        "Capture a screenshot of the parent view using view.draw(canvas) with a bitmap canvas sized to view.width x view.height",
        "Observe that standard views render correctly but SKCanvasView content appears distorted/stretched"
      ],
      "environmentDetails": "SkiaSharp 2.80.3, Android API 31 (target), API < 26 devices affected, Visual Studio",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2416",
          "description": "Original issue with before/after screenshots showing distortion"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "SurfaceFactory.DrawSurface still uses canvas.Width/canvas.Height for the destination rect in current code."
    }
  },
  "analysis": {
    "summary": "The bug is in SurfaceFactory.DrawSurface (Android): it computes `dst = new RectF(0, 0, canvas.Width, canvas.Height)`. For hardware-accelerated rendering, the Android RecordingCanvas.getWidth() returns the view's allocated width, matching Info.Width. But for software bitmap canvases (used in view.draw(canvas) screenshot capture), canvas.Width returns the underlying bitmap width — which is the root/parent view's full width, not the SKCanvasView's width. This causes the Skia bitmap (sized to SKCanvasView's pixel dimensions) to be stretched to the parent's full dimensions, producing visible distortion.",
    "rationale": "Reporter provides clear before/after screenshots and describes an Android-standard technique (view.draw(canvas)) that works for all native views but breaks for SKCanvasView. Code investigation confirms the dst rect in DrawSurface uses canvas.Width (parent bitmap width in software rendering) rather than the view's own Info.Width. This is a real defect in the library, not user error.",
    "keySignals": [
      {
        "text": "For devices with API Level < 26 I am forced to use Canvas... This works OK for most of the views I use it with, but there are a couple of them that do not render correctly.",
        "source": "issue body",
        "interpretation": "Standard Android screenshot technique works for native views but fails for SKCanvasView, pointing to SKCanvasView-specific behavior."
      },
      {
        "text": "Hi, no, I didn't find a fix/workaround yet.",
        "source": "comment by reporter (2024-03-25)",
        "interpretation": "Issue remains unresolved, no known workaround from the reporter."
      },
      {
        "text": "todos tus componentes que quieras capturar en imagen deben estar en el mismo lienzo... debes de realizar la captura con la misma nuget o libreria canvas",
        "source": "comment by DiabloDaniel (2024-04-18)",
        "interpretation": "Workaround: keep all captured content within a single Skia canvas instead of using native screenshot via view.draw."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SurfaceFactory.cs",
        "lines": "53-68",
        "finding": "DrawSurface computes `dst = new RectF(0, 0, canvas.Width, canvas.Height)`. For a software bitmap canvas (screenshot), canvas.Width returns the backing bitmap's width (the parent view's width), not the SKCanvasView's own width. This causes the Skia bitmap (Info.Width x Info.Height = view pixel dimensions) to be scaled to the full parent dimensions before being clipped to the view's bounds, producing a zoomed-in/distorted result.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs",
        "lines": "77-117",
        "finding": "OnDraw passes the Android canvas (which may be a translated/clipped software canvas in screenshot scenarios) directly to surfaceFactory.DrawSurface. The view itself does not pass its own pixel dimensions to DrawSurface, leaving the method reliant on canvas.Width/Height which are unreliable for child views.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Capture each SKCanvasView individually with getBitmapFromView(skCanvasView) rather than the parent view, then compose the final bitmap manually.",
      "Export content directly from the Skia surface before DrawSurface: in a custom SKCanvasView subclass, save the SKBitmap during OnPaintSurface and read it back for screenshot composition.",
      "Keep all content to screenshot within a single SKCanvasView PaintSurface handler so the screenshot can be taken from the Skia surface directly, avoiding view.draw(canvas) entirely (approach described in comment by DiabloDaniel)."
    ],
    "nextQuestions": [
      "Does the same issue affect SKGLSurfaceView or SKGLTextureView when captured via view.draw(canvas)?",
      "Is the bug present only when the SKCanvasView is a child (not the root capture target), or also when it is the root view?"
    ],
    "resolution": {
      "hypothesis": "In SurfaceFactory.DrawSurface, replacing canvas.Width/canvas.Height with bitmap.Width/bitmap.Height (which equals Info.Width/Info.Height) for the destination rect would use the view's actual pixel dimensions, matching hardware-canvas behavior and producing a correct 1:1 copy in the software-canvas screenshot case.",
      "proposals": [
        {
          "title": "Fix DrawSurface destination rect to use bitmap dimensions",
          "description": "Change the dst rect in SurfaceFactory.DrawSurface from canvas.Width/canvas.Height to bitmap.Width/bitmap.Height. This matches the view's own pixel dimensions and is consistent with hardware-canvas behavior.",
          "category": "fix",
          "codeSnippet": "// Before:\nvar dst = new RectF(0, 0, canvas.Width, canvas.Height);\n// After:\nvar dst = new RectF(0, 0, bitmap.Width, bitmap.Height);",
          "confidence": 0.78,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Workaround: capture SKCanvasView directly",
          "description": "Instead of capturing the parent view, call getBitmapFromView on the SKCanvasView itself and stitch bitmaps together. This bypasses the canvas dimension mismatch.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Fix DrawSurface destination rect to use bitmap dimensions",
      "recommendedReason": "One-line fix in SkiaSharp that restores correct behavior without requiring reporter to change their screenshot architecture."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.78,
      "reason": "Root cause identified in SurfaceFactory.DrawSurface: dst rect uses canvas.Width/Height (parent bitmap size) instead of bitmap.Width/Height (view size). A minimal repro is needed to confirm the hypothesis and validate the one-line fix before merging.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views, Android, and compatibility labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Android",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain root cause and provide workarounds",
        "risk": "medium",
        "confidence": 0.78,
        "comment": "Thanks for the detailed screenshots! This appears to be a bug in `SurfaceFactory.DrawSurface` on Android.\n\n**Root cause:** When `view.draw(canvas)` is called on a parent view, the Android system translates and clips the canvas before calling each child's `OnDraw`. However, `canvas.Width` / `canvas.Height` on a software bitmap canvas always returns the *backing bitmap's* dimensions (the parent view's size), not the child view's size. `SKCanvasView` uses `canvas.Width/Height` to set the destination rect when blitting the Skia bitmap, so the content is stretched to the full parent dimensions before being clipped — producing the distortion you see.\n\n**Workarounds while a fix is investigated:**\n1. **Capture each `SKCanvasView` individually** — call `getBitmapFromView(skCanvasView)` on each `SKCanvasView` directly, then manually compose the final screenshot bitmap.\n2. **Move all rendered content into a single `SKCanvasView`** — draw all Skia content in one `PaintSurface` handler and take the screenshot from there, avoiding `view.draw()` altogether.\n3. **Use `PixelCopy` (API ≥ 26)** — you're already doing this; consider dropping support for API < 26 if feasible.\n\nFor the fix, changing `DrawSurface` to use `bitmap.Width/bitmap.Height` instead of `canvas.Width/canvas.Height` should resolve the issue."
      }
    ]
  }
}
```

</details>
