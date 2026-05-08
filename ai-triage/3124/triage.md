# Issue Triage Report — #3124

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T20:30:00Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp (0.85 (85%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** DrawPath with a cubic bezier curve whose control points have extreme float coordinates (millions of units) renders incorrectly compared to GDI+, likely due to float precision loss in Skia's path subdivision algorithm.

**Analysis:** Cubic bezier path rendering is incorrect when control points have very large float coordinates (≈±6 million units). The SkiaSharp bindings pass through to Skia's C API unchanged; the issue is that Skia's path subdivision algorithm loses precision for coordinates this far from the canvas origin, resulting in a visually wrong curve that differs from GDI+.

**Recommendations:** **needs-investigation** — Complete repro provided with visual evidence of wrong output. Root cause is likely in upstream Skia float arithmetic for extreme coordinates; needs investigation to confirm and determine if workaround or fix is possible.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/Raster |
| Tenets | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create an SKBitmap 650x350
2. Add a cubic bezier path with control points at (-6388600, 3162518), (-6010237, 6388600), (639, 40), (639, 40)
3. DrawPath with stroke style
4. Compare output to GDI+ DrawBeziers with the same points

**Environment:** SkiaSharp 3.116.0, Visual Studio on Windows

**Repository links:**
- https://github.com/user-attachments/assets/b8adb0a5-373d-4d2f-88fa-035d756d6ac7 — Expected output (GDI+)
- https://github.com/user-attachments/assets/6fd56b61-4d99-4436-a8b4-d51f8ab7945a — Actual output (SkiaSharp)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | — |
| Repro quality | complete |
| Target frameworks | net9.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The path cubic-to code paths in SkiaSharp delegate directly to the Skia C API without any coordinate transformation or clamping, and this behavior is rooted in upstream Skia float arithmetic. |

## Analysis

### Technical Summary

Cubic bezier path rendering is incorrect when control points have very large float coordinates (≈±6 million units). The SkiaSharp bindings pass through to Skia's C API unchanged; the issue is that Skia's path subdivision algorithm loses precision for coordinates this far from the canvas origin, resulting in a visually wrong curve that differs from GDI+.

### Rationale

The reporter provides a complete repro with screenshots comparing Skia vs GDI+ output for a specific cubic bezier with extreme coordinates. SkiaSharp's CubicTo and DrawPath bindings are pass-throughs with no coordinate processing. The discrepancy is almost certainly caused by float precision limits in upstream Skia's bezier subdivision when control points are millions of units away from the viewport. This is a known class of floating-point rendering artifact in GPU/Skia path processing.

### Key Signals

- "new SKPoint(-6388600, 3162518), new SKPoint(-6010237, 6388600)" — **issue body** (Control points at ~6 million units — far beyond typical canvas dimensions, triggering float precision loss in Skia's bezier math.)
- "This is the result with SkiaSharp [incorrect curve image]" — **issue body** (The rendered path differs significantly from the mathematically expected curve, indicating incorrect subdivision or clipping.)
- "I am not sure whether this behavior is a bug" — **issue body** (Reporter is uncertain, but the visual discrepancy clearly shows wrong output.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPathBuilder.cs` | 108-112 | direct | CubicTo passes float coordinates directly to sk_pathbuilder_cubic_to with no clamping, scaling, or range validation. Extreme coordinate values are forwarded as-is to the Skia C API. |
| `binding/SkiaSharp/SKCanvas.cs` | 405-412 | related | DrawPath delegates directly to sk_canvas_draw_path with no pre-processing. The issue is downstream in Skia's rendering engine. |

### Workarounds

- Normalize/scale the coordinate space so control points are within a reasonable range before drawing (e.g., apply a canvas transform/scale to bring coordinates to a smaller range)
- Apply an SKCanvas.Translate/Scale to shift the origin near the path coordinates, then scale down

### Next Questions

- Does the issue occur with the same coordinates on Linux/macOS, or is it Windows-specific (Direct2D vs Raster backend)?
- Is this a known upstream Skia limitation with extreme float coordinates in bezier subdivision?
- Is the behavior different between SkiaSharp 2.88.x and 3.x, and if so what changed?

### Resolution Proposals

**Hypothesis:** Skia's cubic bezier subdivision algorithm loses float precision when control points are at coordinates of magnitude ~6 million, producing incorrect curve geometry. GDI+ may use double-precision arithmetic internally, avoiding this issue.

1. **Investigate upstream Skia float precision limits** — investigation, confidence 0.80 (80%), cost/m, validated=untested
   - Check whether Skia has a known coordinate range limit (e.g., SkScalar_Max or path clipping constants) that explains this behavior. This is likely an upstream Skia issue, not a SkiaSharp binding issue.
2. **Workaround: Apply canvas transform to normalize coordinates** — workaround, confidence 0.75 (75%), cost/s, validated=untested
   - Scale the canvas so extreme control points are brought within a numerically stable range. Use SKCanvas.Scale and Translate to center the drawing area near the path coordinates.

**Recommended proposal:** Investigate upstream Skia float precision limits

**Why:** Understanding whether this is an upstream Skia bug or by-design behavior determines whether a fix is possible in SkiaSharp or must be tracked in the Skia issue tracker.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Complete repro provided with visual evidence of wrong output. Root cause is likely in upstream Skia float arithmetic for extreme coordinates; needs investigation to confirm and determine if workaround or fix is possible. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, core area, and Windows platform labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, backend/Raster |
| add-comment | medium | 0.82 (82%) | Acknowledge the issue, explain probable cause, and provide workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report and comparison screenshots!

The coordinates in your path (`-6388600`, `6388600`, etc.) are very far from the canvas origin. Skia uses single-precision float arithmetic internally, and at this scale (~6 million units), float precision loss can affect cubic bezier subdivision, producing a different curve than GDI+ (which may use double-precision internally).

This is likely an upstream Skia behavior rather than a SkiaSharp binding issue. The SkiaSharp bindings pass coordinates directly to Skia without any clamping or transformation.

**Workaround:** Apply a canvas transform to bring the coordinates into a numerically stable range before drawing:
```csharp
// Example: apply a scale+translate to normalize the coordinate space
using (SKAutoCanvasRestore _ = new SKAutoCanvasRestore(canvas, true)) {
    float scale = 0.0001f; // adjust as needed
    canvas.Scale(scale, scale);
    canvas.DrawPath(path, paint);
}
```

Alternatively, pre-scale all your points to a smaller range before creating the path.

We'll investigate whether this is a known upstream Skia limitation or something we can address in SkiaSharp.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3124,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T20:30:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "DrawPath with a cubic bezier curve whose control points have extreme float coordinates (millions of units) renders incorrectly compared to GDI+, likely due to float precision loss in Skia's path subdivision algorithm.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.85
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/Raster"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net9.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKBitmap 650x350",
        "Add a cubic bezier path with control points at (-6388600, 3162518), (-6010237, 6388600), (639, 40), (639, 40)",
        "DrawPath with stroke style",
        "Compare output to GDI+ DrawBeziers with the same points"
      ],
      "environmentDetails": "SkiaSharp 3.116.0, Visual Studio on Windows",
      "repoLinks": [
        {
          "url": "https://github.com/user-attachments/assets/b8adb0a5-373d-4d2f-88fa-035d756d6ac7",
          "description": "Expected output (GDI+)"
        },
        {
          "url": "https://github.com/user-attachments/assets/6fd56b61-4d99-4436-a8b4-d51f8ab7945a",
          "description": "Actual output (SkiaSharp)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The path cubic-to code paths in SkiaSharp delegate directly to the Skia C API without any coordinate transformation or clamping, and this behavior is rooted in upstream Skia float arithmetic."
    }
  },
  "analysis": {
    "summary": "Cubic bezier path rendering is incorrect when control points have very large float coordinates (≈±6 million units). The SkiaSharp bindings pass through to Skia's C API unchanged; the issue is that Skia's path subdivision algorithm loses precision for coordinates this far from the canvas origin, resulting in a visually wrong curve that differs from GDI+.",
    "rationale": "The reporter provides a complete repro with screenshots comparing Skia vs GDI+ output for a specific cubic bezier with extreme coordinates. SkiaSharp's CubicTo and DrawPath bindings are pass-throughs with no coordinate processing. The discrepancy is almost certainly caused by float precision limits in upstream Skia's bezier subdivision when control points are millions of units away from the viewport. This is a known class of floating-point rendering artifact in GPU/Skia path processing.",
    "keySignals": [
      {
        "text": "new SKPoint(-6388600, 3162518), new SKPoint(-6010237, 6388600)",
        "source": "issue body",
        "interpretation": "Control points at ~6 million units — far beyond typical canvas dimensions, triggering float precision loss in Skia's bezier math."
      },
      {
        "text": "This is the result with SkiaSharp [incorrect curve image]",
        "source": "issue body",
        "interpretation": "The rendered path differs significantly from the mathematically expected curve, indicating incorrect subdivision or clipping."
      },
      {
        "text": "I am not sure whether this behavior is a bug",
        "source": "issue body",
        "interpretation": "Reporter is uncertain, but the visual discrepancy clearly shows wrong output."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPathBuilder.cs",
        "lines": "108-112",
        "finding": "CubicTo passes float coordinates directly to sk_pathbuilder_cubic_to with no clamping, scaling, or range validation. Extreme coordinate values are forwarded as-is to the Skia C API.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "405-412",
        "finding": "DrawPath delegates directly to sk_canvas_draw_path with no pre-processing. The issue is downstream in Skia's rendering engine.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Does the issue occur with the same coordinates on Linux/macOS, or is it Windows-specific (Direct2D vs Raster backend)?",
      "Is this a known upstream Skia limitation with extreme float coordinates in bezier subdivision?",
      "Is the behavior different between SkiaSharp 2.88.x and 3.x, and if so what changed?"
    ],
    "workarounds": [
      "Normalize/scale the coordinate space so control points are within a reasonable range before drawing (e.g., apply a canvas transform/scale to bring coordinates to a smaller range)",
      "Apply an SKCanvas.Translate/Scale to shift the origin near the path coordinates, then scale down"
    ],
    "resolution": {
      "hypothesis": "Skia's cubic bezier subdivision algorithm loses float precision when control points are at coordinates of magnitude ~6 million, producing incorrect curve geometry. GDI+ may use double-precision arithmetic internally, avoiding this issue.",
      "proposals": [
        {
          "title": "Investigate upstream Skia float precision limits",
          "description": "Check whether Skia has a known coordinate range limit (e.g., SkScalar_Max or path clipping constants) that explains this behavior. This is likely an upstream Skia issue, not a SkiaSharp binding issue.",
          "category": "investigation",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Workaround: Apply canvas transform to normalize coordinates",
          "description": "Scale the canvas so extreme control points are brought within a numerically stable range. Use SKCanvas.Scale and Translate to center the drawing area near the path coordinates.",
          "category": "workaround",
          "confidence": 0.75,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Investigate upstream Skia float precision limits",
      "recommendedReason": "Understanding whether this is an upstream Skia bug or by-design behavior determines whether a fix is possible in SkiaSharp or must be tracked in the Skia issue tracker."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Complete repro provided with visual evidence of wrong output. Root cause is likely in upstream Skia float arithmetic for extreme coordinates; needs investigation to confirm and determine if workaround or fix is possible.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, core area, and Windows platform labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/Raster"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the issue, explain probable cause, and provide workaround",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thank you for the detailed report and comparison screenshots!\n\nThe coordinates in your path (`-6388600`, `6388600`, etc.) are very far from the canvas origin. Skia uses single-precision float arithmetic internally, and at this scale (~6 million units), float precision loss can affect cubic bezier subdivision, producing a different curve than GDI+ (which may use double-precision internally).\n\nThis is likely an upstream Skia behavior rather than a SkiaSharp binding issue. The SkiaSharp bindings pass coordinates directly to Skia without any clamping or transformation.\n\n**Workaround:** Apply a canvas transform to bring the coordinates into a numerically stable range before drawing:\n```csharp\n// Example: apply a scale+translate to normalize the coordinate space\nusing (SKAutoCanvasRestore _ = new SKAutoCanvasRestore(canvas, true)) {\n    float scale = 0.0001f; // adjust as needed\n    canvas.Scale(scale, scale);\n    canvas.DrawPath(path, paint);\n}\n```\n\nAlternatively, pre-scale all your points to a smaller range before creating the path.\n\nWe'll investigate whether this is a known upstream Skia limitation or something we can address in SkiaSharp."
      }
    ]
  }
}
```

</details>
