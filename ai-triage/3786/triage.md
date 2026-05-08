# Issue Triage Report — #3786

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T23:13:18Z |
| Type | type/bug (0.78 (78%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-reproduction (0.85 (85%)) |

**Issue Summary:** Reporter observes a ~13.7x performance slowdown (135ms vs 1846ms) when using SetMatrix with a 5x scale transform to draw a 1280x768 image onto a 5000x2500 CPU raster surface; a community contributor could not reproduce from the partial code snippet.

**Analysis:** The observed ~13.7x slowdown closely matches the expected pixel-area ratio: without a matrix, DrawImage fills ~1280x768 = 983K pixels; with a 5x scale on a 5000x2500 raster surface, it fills up to 12.5M pixels (~12.7x more). CPU raster rendering time is linear in pixel count, so the timing difference is consistent with expected behavior. A community contributor could not reproduce from the incomplete snippet (Raster.Image is undefined), and the reporter has offered to create a full sample.

**Recommendations:** **needs-reproduction** — Community contributor could not reproduce from the partial code. Raster.Image is undefined in the snippet. The timing difference is consistent with expected CPU-raster pixel-count scaling (12.7x more pixels = 13.7x more time). A self-contained repro is needed to confirm or rule out other causes.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/performance |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a 5000x2500 CPU raster SKSurface
2. Define a SKMatrix with ScaleX~5.08, ScaleY~5.08 and negative translation
3. Call surface.Canvas.SetMatrix(matrix)
4. Call surface.Canvas.DrawImage(image, new SKPoint(0, 0)) with a 1280x768 image
5. Compare elapsed time against the same draw without SetMatrix

**Environment:** Windows, .NET 10, SkiaSharp 3.119.2

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/758 — Related closed issue: rotated bitmap rendering slow compared to System.Drawing (resolved in v1.68.1)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | performance |
| Error message | — |
| Repro quality | partial |
| Target frameworks | net10.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.119.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Current stable version; no known regression or fix related to raster DrawImage performance. |

## Analysis

### Technical Summary

The observed ~13.7x slowdown closely matches the expected pixel-area ratio: without a matrix, DrawImage fills ~1280x768 = 983K pixels; with a 5x scale on a 5000x2500 raster surface, it fills up to 12.5M pixels (~12.7x more). CPU raster rendering time is linear in pixel count, so the timing difference is consistent with expected behavior. A community contributor could not reproduce from the incomplete snippet (Raster.Image is undefined), and the reporter has offered to create a full sample.

### Rationale

Performance complaint filed as [BUG] with concrete timing data and a partial code snippet. The ~13.7x slowdown is proportional to the ~12.7x increase in rendered pixel area caused by a 5x scale on a large CPU raster surface. Code investigation confirms the default DrawImage path uses nearest-neighbor sampling (fastest possible) and SetMatrix maps directly to a native call with no C# overhead. This suggests expected CPU-raster behavior, not a SkiaSharp defect, but a complete minimal repro is required before closing.

### Key Signals

- "Draw Image w/ Matrix 1846 ms vs Draw Image No Matrix 135 ms" — **issue body** (13.7x slowdown; closely matches the 12.7x increase in rendered pixel area (983K -> 12.5M pixels) caused by the 5x scale on a 5000x2500 raster surface.)
- "ScaleX: 5.0792174, ScaleY: 5.07657, TransX: -647.4011, TransY: -768.93756" — **issue body** (5x scale with negative translation places the 1280x768 source image to cover most of the 5000x2500 canvas — rendering the entire large surface in CPU.)
- "No repro for me - might need a full sample." — **comment by jeremy-visionaid (CONTRIBUTOR)** (Incomplete snippet prevents reproduction; Raster.Image is not defined so image source, format, and caching behavior are unknown.)
- "I will try to create a repository for this soon." — **comment by gktval (reporter)** (Reporter acknowledged the code is incomplete and is willing to provide a self-contained sample.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 455-479 | direct | DrawImage(SKImage, SKPoint, SKPaint?) calls DrawImage with SKSamplingOptions.Default when no paint is provided. Default is new() which sets fFilter=0 (Nearest) and fMipmap=0 (None) — the fastest possible sampling mode. No extra per-pixel filtering cost introduced by SkiaSharp. |
| `binding/SkiaSharp/SKCanvas.cs` | 895-906 | direct | SetMatrix(in SKMatrix) converts to SKMatrix44 and calls sk_canvas_set_matrix via P/Invoke. No buffering or extra processing in the C# wrapper; the transform is applied at the native Skia rasterization layer. |
| `binding/SkiaSharp/Definitions.cs` | 617-619 | context | SKSamplingOptions.Default = new() is a default-initialized struct (fFilter=Nearest, fMipmap=None, no cubic). This is the fastest sampling option available. |

### Workarounds

- Pre-scale the source image once to the desired output dimensions using SKImage.ScalePixels() or SKBitmap.Resize() before the render loop, then DrawImage at 1:1 scale without a matrix.
- Reduce the target surface size to match the actual visible content needed; a smaller raster surface means fewer pixels to fill.
- Use Save/Scale/Translate instead of SetMatrix when composing multiple objects, so transforms are relative rather than absolute.

### Next Questions

- What is Raster.Image? Is it an SKImage (possibly lazy/DeferredFromEncoded) or an SKBitmap? Lazy decode on first draw under a matrix could explain the slowdown.
- Are the timing measurements for a single draw or an average over multiple iterations? First-call JIT and image-decode costs could skew single-shot measurements.
- Does the slowdown persist across repeated calls in a warm loop, or only on the first draw?
- Does the issue reproduce with a programmatically generated SKImage (avoiding lazy decode) to isolate transform cost from image loading cost?

### Resolution Proposals

**Hypothesis:** The slowdown is proportional to rendered pixel area on a CPU raster surface: a 5x scale causes Skia to fill ~12.5M pixels vs ~983K without a matrix — a 12.7x increase consistent with the observed 13.7x timing difference. This is expected CPU raster behavior, not a SkiaSharp defect.

1. **Pre-scale the source image before the render loop** — workaround, confidence 0.80 (80%), cost/s, validated=untested
   - Scale the source image once to the target dimensions and draw at 1:1. This avoids per-frame transform overhead on CPU surfaces.
2. **Request minimal self-contained repro from reporter** — investigation, confidence 0.95 (95%), cost/xs, validated=untested
   - Ask the reporter to provide the promised self-contained sample including the image source, so the behavior can be verified and potential lazy-decode or measurement artifacts can be ruled out.

**Recommended proposal:** Request minimal self-contained repro from reporter

**Why:** A community contributor could not reproduce from the partial snippet. The reporter offered to create a full sample. Without a minimal repro the slowdown cannot be distinguished from image decode cost, JIT warmup, or genuine raster scaling overhead.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-reproduction |
| Confidence | 0.85 (85%) |
| Reason | Community contributor could not reproduce from the partial code. Raster.Image is undefined in the snippet. The timing difference is consistent with expected CPU-raster pixel-count scaling (12.7x more pixels = 13.7x more time). A self-contained repro is needed to confirm or rule out other causes. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Apply performance bug labels for Windows Classic and performance tenet | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/performance |
| add-comment | medium | 0.80 (80%) | Ask for minimal repro and explain likely root cause (pixel-area scaling) plus workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed timing report!

A contributor was unable to reproduce this from the snippet — `Raster.Image` and its source type/format are needed for a self-contained test. As you offered, a minimal repo would help a lot.

**Likely explanation:** Your target surface is 5000×2500 pixels. Without a matrix, `DrawImage` only fills 1280×768 ≈ 983K pixels. With your ~5× scale matrix, the image expands to cover roughly 6490×3894 logical pixels — filling most of the 5000×2500 canvas, or about 12.5M pixels. CPU raster rendering time scales linearly with pixels filled, so a 12.7× pixel increase matches your observed ~13.7× slowdown. This is expected behavior for a CPU (raster) surface, not a SkiaSharp defect.

**Immediate workaround:** Pre-scale the source image once to the target size (e.g. using `SKBitmap.Resize()` or `SKImage.ScalePixels()`) and draw at 1:1 scale. This moves the scale cost out of the render loop.

**On SKBitmap:** It is not deprecated in SkiaSharp, but `SKImage` is the preferred immutable type for decoded images. Either works for drawing; `SKImage` is better for caching and sharing across surfaces.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3786,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T23:13:18Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter observes a ~13.7x performance slowdown (135ms vs 1846ms) when using SetMatrix with a 5x scale transform to draw a 1280x768 image onto a 5000x2500 CPU raster surface; a community contributor could not reproduce from the partial code snippet.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.78
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "low",
      "regressionClaimed": false,
      "errorType": "performance",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net10.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a 5000x2500 CPU raster SKSurface",
        "Define a SKMatrix with ScaleX~5.08, ScaleY~5.08 and negative translation",
        "Call surface.Canvas.SetMatrix(matrix)",
        "Call surface.Canvas.DrawImage(image, new SKPoint(0, 0)) with a 1280x768 image",
        "Compare elapsed time against the same draw without SetMatrix"
      ],
      "environmentDetails": "Windows, .NET 10, SkiaSharp 3.119.2",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/758",
          "description": "Related closed issue: rotated bitmap rendering slow compared to System.Drawing (resolved in v1.68.1)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.119.2"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Current stable version; no known regression or fix related to raster DrawImage performance."
    }
  },
  "analysis": {
    "summary": "The observed ~13.7x slowdown closely matches the expected pixel-area ratio: without a matrix, DrawImage fills ~1280x768 = 983K pixels; with a 5x scale on a 5000x2500 raster surface, it fills up to 12.5M pixels (~12.7x more). CPU raster rendering time is linear in pixel count, so the timing difference is consistent with expected behavior. A community contributor could not reproduce from the incomplete snippet (Raster.Image is undefined), and the reporter has offered to create a full sample.",
    "rationale": "Performance complaint filed as [BUG] with concrete timing data and a partial code snippet. The ~13.7x slowdown is proportional to the ~12.7x increase in rendered pixel area caused by a 5x scale on a large CPU raster surface. Code investigation confirms the default DrawImage path uses nearest-neighbor sampling (fastest possible) and SetMatrix maps directly to a native call with no C# overhead. This suggests expected CPU-raster behavior, not a SkiaSharp defect, but a complete minimal repro is required before closing.",
    "keySignals": [
      {
        "text": "Draw Image w/ Matrix 1846 ms vs Draw Image No Matrix 135 ms",
        "source": "issue body",
        "interpretation": "13.7x slowdown; closely matches the 12.7x increase in rendered pixel area (983K -> 12.5M pixels) caused by the 5x scale on a 5000x2500 raster surface."
      },
      {
        "text": "ScaleX: 5.0792174, ScaleY: 5.07657, TransX: -647.4011, TransY: -768.93756",
        "source": "issue body",
        "interpretation": "5x scale with negative translation places the 1280x768 source image to cover most of the 5000x2500 canvas — rendering the entire large surface in CPU."
      },
      {
        "text": "No repro for me - might need a full sample.",
        "source": "comment by jeremy-visionaid (CONTRIBUTOR)",
        "interpretation": "Incomplete snippet prevents reproduction; Raster.Image is not defined so image source, format, and caching behavior are unknown."
      },
      {
        "text": "I will try to create a repository for this soon.",
        "source": "comment by gktval (reporter)",
        "interpretation": "Reporter acknowledged the code is incomplete and is willing to provide a self-contained sample."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "455-479",
        "finding": "DrawImage(SKImage, SKPoint, SKPaint?) calls DrawImage with SKSamplingOptions.Default when no paint is provided. Default is new() which sets fFilter=0 (Nearest) and fMipmap=0 (None) — the fastest possible sampling mode. No extra per-pixel filtering cost introduced by SkiaSharp.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "895-906",
        "finding": "SetMatrix(in SKMatrix) converts to SKMatrix44 and calls sk_canvas_set_matrix via P/Invoke. No buffering or extra processing in the C# wrapper; the transform is applied at the native Skia rasterization layer.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "617-619",
        "finding": "SKSamplingOptions.Default = new() is a default-initialized struct (fFilter=Nearest, fMipmap=None, no cubic). This is the fastest sampling option available.",
        "relevance": "context"
      }
    ],
    "nextQuestions": [
      "What is Raster.Image? Is it an SKImage (possibly lazy/DeferredFromEncoded) or an SKBitmap? Lazy decode on first draw under a matrix could explain the slowdown.",
      "Are the timing measurements for a single draw or an average over multiple iterations? First-call JIT and image-decode costs could skew single-shot measurements.",
      "Does the slowdown persist across repeated calls in a warm loop, or only on the first draw?",
      "Does the issue reproduce with a programmatically generated SKImage (avoiding lazy decode) to isolate transform cost from image loading cost?"
    ],
    "workarounds": [
      "Pre-scale the source image once to the desired output dimensions using SKImage.ScalePixels() or SKBitmap.Resize() before the render loop, then DrawImage at 1:1 scale without a matrix.",
      "Reduce the target surface size to match the actual visible content needed; a smaller raster surface means fewer pixels to fill.",
      "Use Save/Scale/Translate instead of SetMatrix when composing multiple objects, so transforms are relative rather than absolute."
    ],
    "resolution": {
      "hypothesis": "The slowdown is proportional to rendered pixel area on a CPU raster surface: a 5x scale causes Skia to fill ~12.5M pixels vs ~983K without a matrix — a 12.7x increase consistent with the observed 13.7x timing difference. This is expected CPU raster behavior, not a SkiaSharp defect.",
      "proposals": [
        {
          "title": "Pre-scale the source image before the render loop",
          "description": "Scale the source image once to the target dimensions and draw at 1:1. This avoids per-frame transform overhead on CPU surfaces.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Request minimal self-contained repro from reporter",
          "description": "Ask the reporter to provide the promised self-contained sample including the image source, so the behavior can be verified and potential lazy-decode or measurement artifacts can be ruled out.",
          "category": "investigation",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Request minimal self-contained repro from reporter",
      "recommendedReason": "A community contributor could not reproduce from the partial snippet. The reporter offered to create a full sample. Without a minimal repro the slowdown cannot be distinguished from image decode cost, JIT warmup, or genuine raster scaling overhead."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-reproduction",
      "confidence": 0.85,
      "reason": "Community contributor could not reproduce from the partial code. Raster.Image is undefined in the snippet. The timing difference is consistent with expected CPU-raster pixel-count scaling (12.7x more pixels = 13.7x more time). A self-contained repro is needed to confirm or rule out other causes.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply performance bug labels for Windows Classic and performance tenet",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/performance"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask for minimal repro and explain likely root cause (pixel-area scaling) plus workaround",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the detailed timing report!\n\nA contributor was unable to reproduce this from the snippet — `Raster.Image` and its source type/format are needed for a self-contained test. As you offered, a minimal repo would help a lot.\n\n**Likely explanation:** Your target surface is 5000×2500 pixels. Without a matrix, `DrawImage` only fills 1280×768 ≈ 983K pixels. With your ~5× scale matrix, the image expands to cover roughly 6490×3894 logical pixels — filling most of the 5000×2500 canvas, or about 12.5M pixels. CPU raster rendering time scales linearly with pixels filled, so a 12.7× pixel increase matches your observed ~13.7× slowdown. This is expected behavior for a CPU (raster) surface, not a SkiaSharp defect.\n\n**Immediate workaround:** Pre-scale the source image once to the target size (e.g. using `SKBitmap.Resize()` or `SKImage.ScalePixels()`) and draw at 1:1 scale. This moves the scale cost out of the render loop.\n\n**On SKBitmap:** It is not deprecated in SkiaSharp, but `SKImage` is the preferred immutable type for decoded images. Either works for drawing; `SKImage` is better for caching and sharing across surfaces."
      }
    ]
  }
}
```

</details>
