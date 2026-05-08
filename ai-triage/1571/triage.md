# Issue Triage Report — #1571

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-29T14:34:55Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-fixed (0.93 (93%)) |

**Issue Summary:** Feature request to expose SkCanvas::drawImageRect in SkiaSharp so that a sub-rectangle of an image can be drawn into a destination rectangle with subpixel sampling; this functionality is already implemented as SKCanvas.DrawImage(image, source, dest, ...) overloads.

**Analysis:** The requested SkCanvas::drawImageRect functionality is fully available through SKCanvas.DrawImage(image, sourceRect, destRect, samplingOptions, paint) and the simpler overload without explicit sampling. These methods call sk_canvas_draw_image_rect, which maps directly to the C++ SkCanvas::drawImageRect API and supports subpixel/fractional source cropping.

**Recommendations:** **close-as-fixed** — SKCanvas.DrawImage(image, sourceRect, destRect, SKSamplingOptions, SKPaint) overloads are present in current codebase and map directly to sk_canvas_draw_image_rect (SkCanvas::drawImageRect). The feature request has been fully satisfied.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Reporter wants to draw a sub-rectangle (src) of an SKImage into a destination rectangle (dst) with subpixel sampling.
2. Reporter states SKCanvas::drawImage does not support subpixel/fractional source cropping.
3. Reporter notes drawImageRect should handle this case.

**Environment:** No version specified; issue filed 2020-12-31.

**Repository links:**
- https://api.skia.org/classSkCanvas.html#a769bf7da37709068444a0d05243aa22a — Skia upstream API reference for SkCanvas::drawImageRect

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | The requested feature has since been implemented: SKCanvas.DrawImage(image, source, dest, sampling, paint) overloads call sk_canvas_draw_image_rect and fully cover the drawImageRect use-case. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.97 (97%) |
| Reason | DrawImage overloads with (SKImage, SKRect source, SKRect dest, SKSamplingOptions, SKPaint) and (SKImage, SKRect source, SKRect dest, SKPaint) now exist in SKCanvas.cs and delegate to sk_canvas_draw_image_rect, which is the C API binding of SkCanvas::drawImageRect. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

The requested SkCanvas::drawImageRect functionality is fully available through SKCanvas.DrawImage(image, sourceRect, destRect, samplingOptions, paint) and the simpler overload without explicit sampling. These methods call sk_canvas_draw_image_rect, which maps directly to the C++ SkCanvas::drawImageRect API and supports subpixel/fractional source cropping.

### Rationale

Issue requests drawImageRect exposure. Code investigation confirms the feature is now implemented via DrawImage(source, dest) overloads using sk_canvas_draw_image_rect. This warrants close-as-fixed.

### Key Signals

- "This Skia API function is not supported." — **issue body** (Reporter believes drawImageRect is missing from SkiaSharp bindings.)
- "It seems drawImageRect can do this, at least that's what I understand from the comments." — **issue body** (Reporter needs the source-rect overload of draw with subpixel sampling support.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 493-510 | direct | DrawImage(SKImage, SKRect source, SKRect dest, SKPaint) and DrawImage(SKImage, SKRect source, SKRect dest, SKSamplingOptions, SKPaint) overloads exist and call sk_canvas_draw_image_rect, providing full drawImageRect functionality including subpixel sampling. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 2494-2510 | direct | sk_canvas_draw_image_rect is generated as a P/Invoke binding: void sk_canvas_draw_image_rect(sk_canvas_t*, sk_image_t*, sk_rect_t* srcR, sk_rect_t* dstR, sk_sampling_options_t*, sk_paint_t*) — direct mapping of SkCanvas::drawImageRect. |

### Workarounds

- Use SKCanvas.DrawImage(image, sourceRect, destRect, SKSamplingOptions.Default, paint) — this is the implemented equivalent of SkCanvas::drawImageRect and supports subpixel/fractional source cropping.

### Resolution Proposals

**Hypothesis:** The feature was implemented after the issue was filed. The DrawImage overloads with source+dest rects and SKSamplingOptions fulfill the drawImageRect contract.

1. **Close as fixed — use DrawImage(image, source, dest, sampling, paint)** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - The requested API is now available via SKCanvas.DrawImage overloads. The reporter can use DrawImage(image, sourceRect, destRect, SKSamplingOptions.Default, paint) for subpixel sampling.

```csharp
canvas.DrawImage(image, sourceRect, destRect, SKSamplingOptions.Default);
```

**Recommended proposal:** Close as fixed — use DrawImage(image, source, dest, sampling, paint)

**Why:** Feature fully implemented; close to reduce noise on the tracker.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.93 (93%) |
| Reason | SKCanvas.DrawImage(image, sourceRect, destRect, SKSamplingOptions, SKPaint) overloads are present in current codebase and map directly to sk_canvas_draw_image_rect (SkCanvas::drawImageRect). The feature request has been fully satisfied. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply feature-request and SkiaSharp area labels | labels=type/feature-request, area/SkiaSharp, tenet/compatibility |
| add-comment | medium | 0.93 (93%) | Inform reporter the feature is implemented as DrawImage overloads | — |
| close-issue | medium | 0.93 (93%) | Close as completed — feature is implemented | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! This functionality is now available via `SKCanvas.DrawImage` overloads that accept both a source and destination rectangle:

```csharp
// Draw a sub-rectangle of an image into a destination rect with subpixel sampling
canvas.DrawImage(image, sourceRect, destRect, SKSamplingOptions.Default);

// Or with explicit paint and custom sampling:
canvas.DrawImage(image, sourceRect, destRect, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None), paint);
```

These methods internally call `sk_canvas_draw_image_rect` which maps directly to `SkCanvas::drawImageRect` and fully supports subpixel/fractional source cropping. Feel free to reopen if you encounter any issues.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1571,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-29T14:34:55Z"
  },
  "summary": "Feature request to expose SkCanvas::drawImageRect in SkiaSharp so that a sub-rectangle of an image can be drawn into a destination rectangle with subpixel sampling; this functionality is already implemented as SKCanvas.DrawImage(image, source, dest, ...) overloads.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Reporter wants to draw a sub-rectangle (src) of an SKImage into a destination rectangle (dst) with subpixel sampling.",
        "Reporter states SKCanvas::drawImage does not support subpixel/fractional source cropping.",
        "Reporter notes drawImageRect should handle this case."
      ],
      "environmentDetails": "No version specified; issue filed 2020-12-31.",
      "repoLinks": [
        {
          "url": "https://api.skia.org/classSkCanvas.html#a769bf7da37709068444a0d05243aa22a",
          "description": "Skia upstream API reference for SkCanvas::drawImageRect"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "unlikely",
      "relevanceReason": "The requested feature has since been implemented: SKCanvas.DrawImage(image, source, dest, sampling, paint) overloads call sk_canvas_draw_image_rect and fully cover the drawImageRect use-case."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.97,
      "reason": "DrawImage overloads with (SKImage, SKRect source, SKRect dest, SKSamplingOptions, SKPaint) and (SKImage, SKRect source, SKRect dest, SKPaint) now exist in SKCanvas.cs and delegate to sk_canvas_draw_image_rect, which is the C API binding of SkCanvas::drawImageRect."
    }
  },
  "analysis": {
    "summary": "The requested SkCanvas::drawImageRect functionality is fully available through SKCanvas.DrawImage(image, sourceRect, destRect, samplingOptions, paint) and the simpler overload without explicit sampling. These methods call sk_canvas_draw_image_rect, which maps directly to the C++ SkCanvas::drawImageRect API and supports subpixel/fractional source cropping.",
    "rationale": "Issue requests drawImageRect exposure. Code investigation confirms the feature is now implemented via DrawImage(source, dest) overloads using sk_canvas_draw_image_rect. This warrants close-as-fixed.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "493-510",
        "finding": "DrawImage(SKImage, SKRect source, SKRect dest, SKPaint) and DrawImage(SKImage, SKRect source, SKRect dest, SKSamplingOptions, SKPaint) overloads exist and call sk_canvas_draw_image_rect, providing full drawImageRect functionality including subpixel sampling.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "2494-2510",
        "finding": "sk_canvas_draw_image_rect is generated as a P/Invoke binding: void sk_canvas_draw_image_rect(sk_canvas_t*, sk_image_t*, sk_rect_t* srcR, sk_rect_t* dstR, sk_sampling_options_t*, sk_paint_t*) — direct mapping of SkCanvas::drawImageRect.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "This Skia API function is not supported.",
        "source": "issue body",
        "interpretation": "Reporter believes drawImageRect is missing from SkiaSharp bindings."
      },
      {
        "text": "It seems drawImageRect can do this, at least that's what I understand from the comments.",
        "source": "issue body",
        "interpretation": "Reporter needs the source-rect overload of draw with subpixel sampling support."
      }
    ],
    "workarounds": [
      "Use SKCanvas.DrawImage(image, sourceRect, destRect, SKSamplingOptions.Default, paint) — this is the implemented equivalent of SkCanvas::drawImageRect and supports subpixel/fractional source cropping."
    ],
    "resolution": {
      "hypothesis": "The feature was implemented after the issue was filed. The DrawImage overloads with source+dest rects and SKSamplingOptions fulfill the drawImageRect contract.",
      "proposals": [
        {
          "title": "Close as fixed — use DrawImage(image, source, dest, sampling, paint)",
          "description": "The requested API is now available via SKCanvas.DrawImage overloads. The reporter can use DrawImage(image, sourceRect, destRect, SKSamplingOptions.Default, paint) for subpixel sampling.",
          "category": "workaround",
          "codeSnippet": "canvas.DrawImage(image, sourceRect, destRect, SKSamplingOptions.Default);",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Close as fixed — use DrawImage(image, source, dest, sampling, paint)",
      "recommendedReason": "Feature fully implemented; close to reduce noise on the tracker."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.93,
      "reason": "SKCanvas.DrawImage(image, sourceRect, destRect, SKSamplingOptions, SKPaint) overloads are present in current codebase and map directly to sk_canvas_draw_image_rect (SkCanvas::drawImageRect). The feature request has been fully satisfied.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request and SkiaSharp area labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Inform reporter the feature is implemented as DrawImage overloads",
        "risk": "medium",
        "confidence": 0.93,
        "comment": "Thanks for the report! This functionality is now available via `SKCanvas.DrawImage` overloads that accept both a source and destination rectangle:\n\n```csharp\n// Draw a sub-rectangle of an image into a destination rect with subpixel sampling\ncanvas.DrawImage(image, sourceRect, destRect, SKSamplingOptions.Default);\n\n// Or with explicit paint and custom sampling:\ncanvas.DrawImage(image, sourceRect, destRect, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None), paint);\n```\n\nThese methods internally call `sk_canvas_draw_image_rect` which maps directly to `SkCanvas::drawImageRect` and fully supports subpixel/fractional source cropping. Feel free to reopen if you encounter any issues."
      },
      {
        "type": "close-issue",
        "description": "Close as completed — feature is implemented",
        "risk": "medium",
        "confidence": 0.93,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
