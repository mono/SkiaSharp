# Issue Triage Report — #1233

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T09:12:42Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views (0.95 (95%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** SKElement and SKGLElement in WPF do not read the WPF Background property to clear the canvas before PaintSurface, so GPU-backed views default to black and CPU-backed views to white.

**Analysis:** Both SKElement (CPU/raster) and SKGLElement (GPU/OpenGL) in SkiaSharp.Views.WPF ignore the inherited WPF Background dependency property. SKElement creates a WriteableBitmap and renders directly without clearing using the Background brush. SKGLElement calls GL.ClearColor(Color4.Transparent) unconditionally, clearing to transparent/black instead of respecting Background. The fix would require reading Background and converting it to an SKColor to pass to canvas.Clear() or GL.ClearColor() before invoking OnPaintSurface.

**Recommendations:** **needs-investigation** — Bug is confirmed in source code — neither SKElement nor SKGLElement reads the WPF Background property. Repro project is provided. Needs investigation to decide best API approach (read Background automatically vs document manual Clear pattern).

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Classic |
| Backends | backend/Raster, backend/OpenGL |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/charlenni/SkiaTest — Reporter's WPF test project demonstrating background color not being applied to SKGLView/SKCanvasView.

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | Forms.WPF |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.2-preview.60 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Source code for SKElement and SKGLElement still does not read the WPF Background property before painting. |

## Analysis

### Technical Summary

Both SKElement (CPU/raster) and SKGLElement (GPU/OpenGL) in SkiaSharp.Views.WPF ignore the inherited WPF Background dependency property. SKElement creates a WriteableBitmap and renders directly without clearing using the Background brush. SKGLElement calls GL.ClearColor(Color4.Transparent) unconditionally, clearing to transparent/black instead of respecting Background. The fix would require reading Background and converting it to an SKColor to pass to canvas.Clear() or GL.ClearColor() before invoking OnPaintSurface.

### Rationale

Both WPF view implementations have been confirmed to not read the WPF Background property. SKElement leaves WriteableBitmap memory uninitialized (appears white), while SKGLElement clears to transparent (appears black on an opaque host). This is a missing feature in the render path — the Background property exists on FrameworkElement but is never forwarded to the Skia canvas clear step.

### Key Signals

- "The GPU backed background is black, the CPU backed background is white. If you use Canvas.Clear(), then the color is set correct." — **issue body** (Confirms the rendering path skips Background; users must manually call canvas.Clear() as workaround.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs` | 45-95 | direct | OnRender creates a WriteableBitmap and calls OnPaintSurface without any canvas.Clear() or background fill. No code reads the inherited Background DependencyProperty from FrameworkElement. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKGLElement.cs` | 138-139 | direct | OnPaint calls GL.ClearColor(Color4.Transparent) and GL.Clear() unconditionally, always producing black for GPU-backed rendering. Background property is never read. |

### Workarounds

- Call canvas.Clear(color) manually at the start of the PaintSurface handler instead of relying on the Background property.

### Next Questions

- Does SkiaSharp.Views.Forms WPF renderer (SKCanvasView for Xamarin.Forms WPF) have the same gap?
- Has this been addressed in SkiaSharp.Views.Maui for Windows?

### Resolution Proposals

**Hypothesis:** Add code in OnRender (SKElement) and OnPaint (SKGLElement) to read the Background brush, convert it to an SKColor, and call canvas.Clear() before invoking OnPaintSurface.

1. **Manually clear canvas with desired background color in PaintSurface handler** — workaround, cost/xs, validated=yes
   - At the beginning of the PaintSurface event handler, call e.Surface.Canvas.Clear(yourColor). This is the documented pattern for controlling the background of any SkiaSharp canvas.

```csharp
private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
{
    var canvas = e.Surface.Canvas;
    canvas.Clear(SKColors.White); // or any desired background color
    // ... rest of drawing code
}
```
2. **Read WPF Background property and apply to canvas in both SKElement and SKGLElement** — fix, cost/s, validated=untested
   - In SKElement.OnRender, before calling OnPaintSurface, read the Background DependencyProperty (a Brush), convert the SolidColorBrush to an SKColor, and call surface.Canvas.Clear(). In SKGLElement.OnPaint, do the same before invoking OnPaintSurface — or pass the color to GL.ClearColor() instead of Color4.Transparent.

**Recommended proposal:** workaround-1

**Why:** The workaround is immediately actionable with no library changes. The proper fix (fix-1) requires a branch and PR.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Bug is confirmed in source code — neither SKElement nor SKGLElement reads the WPF Background property. Repro project is provided. Needs investigation to decide best API approach (read Background automatically vs document manual Clear pattern). |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type, area, platform and backend labels | labels=type/bug, area/SkiaSharp.Views, os/Windows-Classic, backend/Raster, backend/OpenGL |
| add-comment | medium | 0.88 (88%) | Acknowledge the bug and provide workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the clear report and reproduction project!

This is a confirmed bug: `SKElement` and `SKGLElement` in `SkiaSharp.Views.WPF` do not read the WPF `Background` dependency property when clearing the canvas before painting. `SKElement` leaves the bitmap uninitialized (appears white), and `SKGLElement` always clears to `Color4.Transparent` (appears black).

**Workaround (available today):** Clear the canvas manually at the start of your `PaintSurface` handler:

```csharp
private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
{
    var canvas = e.Surface.Canvas;
    canvas.Clear(SKColors.White); // your desired background color
    // ... rest of drawing
}
```

This is the standard pattern for controlling the SkiaSharp canvas background regardless of platform.

The proper fix would be to read the `Background` brush from the WPF element and apply it automatically — we'll track that as a feature improvement.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1233,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T09:12:42Z"
  },
  "summary": "SKElement and SKGLElement in WPF do not read the WPF Background property to clear the canvas before PaintSurface, so GPU-backed views default to black and CPU-backed views to white.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/Raster",
      "backend/OpenGL"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "reproQuality": "partial",
      "targetFrameworks": [
        "Forms.WPF"
      ]
    },
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/charlenni/SkiaTest",
          "description": "Reporter's WPF test project demonstrating background color not being applied to SKGLView/SKCanvasView."
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.2-preview.60"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Source code for SKElement and SKGLElement still does not read the WPF Background property before painting."
    }
  },
  "analysis": {
    "summary": "Both SKElement (CPU/raster) and SKGLElement (GPU/OpenGL) in SkiaSharp.Views.WPF ignore the inherited WPF Background dependency property. SKElement creates a WriteableBitmap and renders directly without clearing using the Background brush. SKGLElement calls GL.ClearColor(Color4.Transparent) unconditionally, clearing to transparent/black instead of respecting Background. The fix would require reading Background and converting it to an SKColor to pass to canvas.Clear() or GL.ClearColor() before invoking OnPaintSurface.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs",
        "lines": "45-95",
        "finding": "OnRender creates a WriteableBitmap and calls OnPaintSurface without any canvas.Clear() or background fill. No code reads the inherited Background DependencyProperty from FrameworkElement.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKGLElement.cs",
        "lines": "138-139",
        "finding": "OnPaint calls GL.ClearColor(Color4.Transparent) and GL.Clear() unconditionally, always producing black for GPU-backed rendering. Background property is never read.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "The GPU backed background is black, the CPU backed background is white. If you use Canvas.Clear(), then the color is set correct.",
        "source": "issue body",
        "interpretation": "Confirms the rendering path skips Background; users must manually call canvas.Clear() as workaround."
      }
    ],
    "rationale": "Both WPF view implementations have been confirmed to not read the WPF Background property. SKElement leaves WriteableBitmap memory uninitialized (appears white), while SKGLElement clears to transparent (appears black on an opaque host). This is a missing feature in the render path — the Background property exists on FrameworkElement but is never forwarded to the Skia canvas clear step.",
    "workarounds": [
      "Call canvas.Clear(color) manually at the start of the PaintSurface handler instead of relying on the Background property."
    ],
    "nextQuestions": [
      "Does SkiaSharp.Views.Forms WPF renderer (SKCanvasView for Xamarin.Forms WPF) have the same gap?",
      "Has this been addressed in SkiaSharp.Views.Maui for Windows?"
    ],
    "resolution": {
      "hypothesis": "Add code in OnRender (SKElement) and OnPaint (SKGLElement) to read the Background brush, convert it to an SKColor, and call canvas.Clear() before invoking OnPaintSurface.",
      "proposals": [
        {
          "title": "Manually clear canvas with desired background color in PaintSurface handler",
          "category": "workaround",
          "effort": "cost/xs",
          "validated": "yes",
          "description": "At the beginning of the PaintSurface event handler, call e.Surface.Canvas.Clear(yourColor). This is the documented pattern for controlling the background of any SkiaSharp canvas.",
          "codeSnippet": "private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)\n{\n    var canvas = e.Surface.Canvas;\n    canvas.Clear(SKColors.White); // or any desired background color\n    // ... rest of drawing code\n}"
        },
        {
          "title": "Read WPF Background property and apply to canvas in both SKElement and SKGLElement",
          "category": "fix",
          "effort": "cost/s",
          "validated": "untested",
          "description": "In SKElement.OnRender, before calling OnPaintSurface, read the Background DependencyProperty (a Brush), convert the SolidColorBrush to an SKColor, and call surface.Canvas.Clear(). In SKGLElement.OnPaint, do the same before invoking OnPaintSurface — or pass the color to GL.ClearColor() instead of Color4.Transparent."
        }
      ],
      "recommendedProposal": "workaround-1",
      "recommendedReason": "The workaround is immediately actionable with no library changes. The proper fix (fix-1) requires a branch and PR."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Bug is confirmed in source code — neither SKElement nor SKGLElement reads the WPF Background property. Repro project is provided. Needs investigation to decide best API approach (read Background automatically vs document manual Clear pattern).",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type, area, platform and backend labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-Classic",
          "backend/Raster",
          "backend/OpenGL"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the bug and provide workaround",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thank you for the clear report and reproduction project!\n\nThis is a confirmed bug: `SKElement` and `SKGLElement` in `SkiaSharp.Views.WPF` do not read the WPF `Background` dependency property when clearing the canvas before painting. `SKElement` leaves the bitmap uninitialized (appears white), and `SKGLElement` always clears to `Color4.Transparent` (appears black).\n\n**Workaround (available today):** Clear the canvas manually at the start of your `PaintSurface` handler:\n\n```csharp\nprivate void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)\n{\n    var canvas = e.Surface.Canvas;\n    canvas.Clear(SKColors.White); // your desired background color\n    // ... rest of drawing\n}\n```\n\nThis is the standard pattern for controlling the SkiaSharp canvas background regardless of platform.\n\nThe proper fix would be to read the `Background` brush from the WPF element and apply it automatically — we'll track that as a feature improvement."
      }
    ]
  }
}
```

</details>
