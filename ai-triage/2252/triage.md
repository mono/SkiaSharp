# Issue Triage Report — #2252

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T12:35:00Z |
| Type | type/bug (0.80 (80%)) |
| Area | area/SkiaSharp.Views.Forms (0.80 (80%)) |
| Suggested action | needs-investigation (0.72 (72%)) |

**Issue Summary:** Edge pixels are clipped when drawing a path at/near the canvas boundary in SkiaSharp.Views.Forms on Android.

**Analysis:** Reporter observes that pixels on canvas edges are occasionally not rendered when drawing a path that spans the full canvas on Android. This is consistent with Skia's by-design behavior of clipping rendered output to the surface bounds—strokes whose center falls at the canvas boundary have half their stroke width outside the surface and get clipped. The SurfaceFactory in SKCanvasView (Android) correctly creates a bitmap-backed surface and blits it to the Android Canvas without extra clipping, so the clipping likely originates in Skia's rasterizer rather than the Views layer. However, the exact reproduction depends on the attached zip, and the grid layout means individual cell border paths may straddle canvas edges inconsistently based on row/column count.

**Recommendations:** **needs-investigation** — Sample code is in a zip attachment. The behavior is consistent with Skia's surface boundary clipping but the layout-dependent variation warrants investigation to determine if it's by-design or a coordinate precision bug.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Forms |
| Platforms | os/Android |
| Backends | backend/Raster |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

**Screenshots:**
- https://user-images.githubusercontent.com/12727359/189909081-bed3f9d4-5bc4-4fd0-b2f5-58c3628ba5d3.png — Grid with bottom edges of cells 1 and 2 showing missing border pixels

**Attachments:**
- EdgePixelsForPath.zip — https://github.com/mono/SkiaSharp/files/9557341/EdgePixelsForPath.zip

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | net6.0-android |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | No fix or change for this behavior was identified in the codebase. |

## Analysis

### Technical Summary

Reporter observes that pixels on canvas edges are occasionally not rendered when drawing a path that spans the full canvas on Android. This is consistent with Skia's by-design behavior of clipping rendered output to the surface bounds—strokes whose center falls at the canvas boundary have half their stroke width outside the surface and get clipped. The SurfaceFactory in SKCanvasView (Android) correctly creates a bitmap-backed surface and blits it to the Android Canvas without extra clipping, so the clipping likely originates in Skia's rasterizer rather than the Views layer. However, the exact reproduction depends on the attached zip, and the grid layout means individual cell border paths may straddle canvas edges inconsistently based on row/column count.

### Rationale

Classified as type/bug in area/SkiaSharp.Views.Forms on Android because the edge-clipping behavior varies with layout parameters, suggesting a real rendering inconsistency rather than purely expected behavior. The root cause is likely Skia rasterizer clipping strokes to surface bounds, which is by-design, but the inconsistency across row/column counts indicates the real issue may be floating-point coordinate calculation in user code interacting with Skia's integer surface bounds. Confidence is moderate because the actual sample code is in an attachment zip.

### Key Signals

- "Pixels on the canvas edges does not always get rendered when drawing a path around the canvas" — **issue body** (Intermittent edge clipping suggests sub-pixel positioning issue or stroke half-width being clipped at surface boundary.)
- "Unrendered edges will vary by changing the number of rows and/or columns." — **issue body** (Layout-dependent clipping suggests fractional pixel positions cause certain strokes to straddle the surface boundary differently.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SurfaceFactory.cs` | 53-68 | direct | DrawSurface blits bitmap to Android Canvas using DrawBitmap(bitmap, src, dst, null). Src is full surface bounds (0,0,W,H) and dst is full canvas bounds—no extra clipping applied at the Views layer. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs` | 77-117 | direct | OnDraw calls surfaceFactory.CreateSurface then invokes OnPaintSurface with a raster surface. When IgnorePixelScaling is true, a density scale is applied to the Skia canvas. The surface dimensions are pixel-accurate to the Android View size. |
| `binding/SkiaSharp/SKCanvas.cs` | 245-295 | context | Skia canvas exposes ClipRect/ClipPath/ClipRegion but these are user-invoked. Skia implicitly clips all rendering to the surface device bounds. Strokes drawn at the exact edge of the canvas will have half the stroke width clipped. |

### Next Questions

- Does the issue reproduce with a minimal standalone test case without Xamarin.Forms?
- Is the stroke width 1px or larger? Larger strokes are more likely to be clipped at edges.
- Does the issue reproduce on iOS or only on Android?
- Is IgnorePixelScaling set? If so, density scaling may cause fractional pixel placement.

### Resolution Proposals

1. **Inset path drawing by half stroke width** — workaround, cost/xs, validated=untested
   - When drawing strokes at the canvas boundary, inset the coordinates inward by half the stroke width so the stroke is fully within the surface bounds.
2. **Investigate floating-point coordinate rounding with varying row/column counts** — investigation, cost/s, validated=untested
   - Reproduce using the attached sample to confirm whether the issue is sub-pixel coordinate placement causing strokes to fall outside the surface boundary.

**Recommended proposal:** Investigate floating-point coordinate rounding with varying row/column counts

**Why:** The actual reproduction requires the attached zip file. The behavior is consistent with Skia's by-design surface clipping but the layout-dependency suggests a coordinate precision issue worth investigating.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.72 (72%) |
| Reason | Sample code is in a zip attachment. The behavior is consistent with Skia's surface boundary clipping but the layout-dependent variation warrants investigation to determine if it's by-design or a coordinate precision bug. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.80 (80%) | Apply type/bug, area/SkiaSharp.Views.Forms, os/Android, backend/Raster, tenet/reliability labels | labels=type/bug, area/SkiaSharp.Views.Forms, os/Android, backend/Raster, tenet/reliability |
| add-comment | medium | 0.72 (72%) | Acknowledge the issue and provide a workaround about insetting coordinates by half stroke width | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the report and the attached sample!

The behavior you're seeing is consistent with how Skia's rasterizer works: rendering is clipped to the surface device bounds. When a stroke is drawn with its center exactly at the canvas edge, half the stroke width falls outside the surface and gets clipped.

A common **workaround** is to inset your path coordinates inward by half the stroke width:

```csharp
float halfStroke = paint.StrokeWidth / 2f;
// Instead of drawing to exact canvas bounds (0 to width):
// Draw inset by half the stroke width:
// path.MoveTo(halfStroke, halfStroke);
// path.LineTo(width - halfStroke, halfStroke);
```

The variation you observe when changing row/column counts is likely due to fractional pixel positions being rounded differently, causing some strokes to fall just inside vs. just outside the surface boundary.

Could you confirm if increasing the stroke width makes the clipping more consistent? That would help confirm this diagnosis.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2252,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T12:35:00Z"
  },
  "summary": "Edge pixels are clipped when drawing a path at/near the canvas boundary in SkiaSharp.Views.Forms on Android.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.8
    },
    "area": {
      "value": "area/SkiaSharp.Views.Forms",
      "confidence": 0.8
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
      "severity": "low",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net6.0-android"
      ]
    },
    "reproEvidence": {
      "attachments": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/9557341/EdgePixelsForPath.zip",
          "filename": "EdgePixelsForPath.zip"
        }
      ],
      "screenshots": [
        {
          "url": "https://user-images.githubusercontent.com/12727359/189909081-bed3f9d4-5bc4-4fd0-b2f5-58c3628ba5d3.png",
          "description": "Grid with bottom edges of cells 1 and 2 showing missing border pixels"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.2"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "No fix or change for this behavior was identified in the codebase."
    }
  },
  "analysis": {
    "summary": "Reporter observes that pixels on canvas edges are occasionally not rendered when drawing a path that spans the full canvas on Android. This is consistent with Skia's by-design behavior of clipping rendered output to the surface bounds—strokes whose center falls at the canvas boundary have half their stroke width outside the surface and get clipped. The SurfaceFactory in SKCanvasView (Android) correctly creates a bitmap-backed surface and blits it to the Android Canvas without extra clipping, so the clipping likely originates in Skia's rasterizer rather than the Views layer. However, the exact reproduction depends on the attached zip, and the grid layout means individual cell border paths may straddle canvas edges inconsistently based on row/column count.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SurfaceFactory.cs",
        "finding": "DrawSurface blits bitmap to Android Canvas using DrawBitmap(bitmap, src, dst, null). Src is full surface bounds (0,0,W,H) and dst is full canvas bounds—no extra clipping applied at the Views layer.",
        "relevance": "direct",
        "lines": "53-68"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs",
        "finding": "OnDraw calls surfaceFactory.CreateSurface then invokes OnPaintSurface with a raster surface. When IgnorePixelScaling is true, a density scale is applied to the Skia canvas. The surface dimensions are pixel-accurate to the Android View size.",
        "relevance": "direct",
        "lines": "77-117"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "finding": "Skia canvas exposes ClipRect/ClipPath/ClipRegion but these are user-invoked. Skia implicitly clips all rendering to the surface device bounds. Strokes drawn at the exact edge of the canvas will have half the stroke width clipped.",
        "relevance": "context",
        "lines": "245-295"
      }
    ],
    "keySignals": [
      {
        "text": "Pixels on the canvas edges does not always get rendered when drawing a path around the canvas",
        "source": "issue body",
        "interpretation": "Intermittent edge clipping suggests sub-pixel positioning issue or stroke half-width being clipped at surface boundary."
      },
      {
        "text": "Unrendered edges will vary by changing the number of rows and/or columns.",
        "source": "issue body",
        "interpretation": "Layout-dependent clipping suggests fractional pixel positions cause certain strokes to straddle the surface boundary differently."
      }
    ],
    "rationale": "Classified as type/bug in area/SkiaSharp.Views.Forms on Android because the edge-clipping behavior varies with layout parameters, suggesting a real rendering inconsistency rather than purely expected behavior. The root cause is likely Skia rasterizer clipping strokes to surface bounds, which is by-design, but the inconsistency across row/column counts indicates the real issue may be floating-point coordinate calculation in user code interacting with Skia's integer surface bounds. Confidence is moderate because the actual sample code is in an attachment zip.",
    "resolution": {
      "proposals": [
        {
          "title": "Inset path drawing by half stroke width",
          "description": "When drawing strokes at the canvas boundary, inset the coordinates inward by half the stroke width so the stroke is fully within the surface bounds.",
          "category": "workaround",
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Investigate floating-point coordinate rounding with varying row/column counts",
          "description": "Reproduce using the attached sample to confirm whether the issue is sub-pixel coordinate placement causing strokes to fall outside the surface boundary.",
          "category": "investigation",
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Investigate floating-point coordinate rounding with varying row/column counts",
      "recommendedReason": "The actual reproduction requires the attached zip file. The behavior is consistent with Skia's by-design surface clipping but the layout-dependency suggests a coordinate precision issue worth investigating."
    },
    "nextQuestions": [
      "Does the issue reproduce with a minimal standalone test case without Xamarin.Forms?",
      "Is the stroke width 1px or larger? Larger strokes are more likely to be clipped at edges.",
      "Does the issue reproduce on iOS or only on Android?",
      "Is IgnorePixelScaling set? If so, density scaling may cause fractional pixel placement."
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.72,
      "reason": "Sample code is in a zip attachment. The behavior is consistent with Skia's surface boundary clipping but the layout-dependent variation warrants investigation to determine if it's by-design or a coordinate precision bug.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views.Forms, os/Android, backend/Raster, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.8,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Forms",
          "os/Android",
          "backend/Raster",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the issue and provide a workaround about insetting coordinates by half stroke width",
        "risk": "medium",
        "confidence": 0.72,
        "comment": "Thank you for the report and the attached sample!\n\nThe behavior you're seeing is consistent with how Skia's rasterizer works: rendering is clipped to the surface device bounds. When a stroke is drawn with its center exactly at the canvas edge, half the stroke width falls outside the surface and gets clipped.\n\nA common **workaround** is to inset your path coordinates inward by half the stroke width:\n\n```csharp\nfloat halfStroke = paint.StrokeWidth / 2f;\n// Instead of drawing to exact canvas bounds (0 to width):\n// Draw inset by half the stroke width:\n// path.MoveTo(halfStroke, halfStroke);\n// path.LineTo(width - halfStroke, halfStroke);\n```\n\nThe variation you observe when changing row/column counts is likely due to fractional pixel positions being rounded differently, causing some strokes to fall just inside vs. just outside the surface boundary.\n\nCould you confirm if increasing the stroke width makes the clipping more consistent? That would help confirm this diagnosis."
      }
    ]
  }
}
```

</details>
