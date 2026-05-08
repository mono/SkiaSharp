# Issue Triage Report — #981

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T01:41:00Z |
| Type | type/enhancement (0.82 (82%)) |
| Area | area/SkiaSharp (0.93 (93%)) |
| Suggested action | needs-investigation (0.75 (75%)) |

**Issue Summary:** Tracking issue filed by maintainer to investigate and improve image resize quality and performance in SkiaSharp, consolidating concerns from #319 and #520 about blurry output when resizing via canvas vs. SKBitmap.

**Analysis:** Tracking issue for image resize quality/performance. The original concerns in #319 and #520 (blurry canvas-drawn resize vs. crisp SKBitmap Lanczos3 resize) led the maintainer to open this as a watchlist item. Since then, SKBitmapResizeMethod and SKFilterQuality have been deprecated and replaced with SKSamplingOptions (which supports SKCubicResampler.Mitchell for high-quality bicubic resizing). However, 2022–2023 comments still report blurry output, suggesting either users are unaware of the new API or a real quality gap persists.

**Recommendations:** **needs-investigation** — Tracking issue with persistent community engagement; the modern SKSamplingOptions API exists but needs quality-comparison verification and user documentation to close the loop.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | backend/Raster |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Related issues:** #319, #520

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/319 — AndersMad: SKImage DrawImage blurry vs SKBitmap Lanczos3 crisp (closed)
- https://github.com/mono/SkiaSharp/issues/520 — dsyno: SKImage resize blurry; SKBitmap Lanczos3 crisp (closed, milestone v1.68.0)
- https://github.com/mono/SkiaSharp/pull/1007 — PR referenced in comments: performance improvement for Windows

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.60.0, 1.68.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | User comments in 2022 and 2023 report that resizing is still blurry with the current API, suggesting the underlying concern persists even though the APIs have been modernized. |

## Analysis

### Technical Summary

Tracking issue for image resize quality/performance. The original concerns in #319 and #520 (blurry canvas-drawn resize vs. crisp SKBitmap Lanczos3 resize) led the maintainer to open this as a watchlist item. Since then, SKBitmapResizeMethod and SKFilterQuality have been deprecated and replaced with SKSamplingOptions (which supports SKCubicResampler.Mitchell for high-quality bicubic resizing). However, 2022–2023 comments still report blurry output, suggesting either users are unaware of the new API or a real quality gap persists.

### Rationale

Classified as type/enhancement because the issue is a quality/performance tracking item, not a single reproducible bug with a specific stack trace. The API evolution (SKBitmapResizeMethod → SKSamplingOptions) partially addresses the original concern, but the open issue and recent community comments suggest a full investigation is warranted to verify current quality and produce migration guidance.

### Key Signals

- "We are working on a much later version of skia, so things may be better soon. But, just to keep track that this is an issue that exists." — **issue body** (Maintainer filed this as a watchlist/tracking issue; expects a Skia upgrade to improve things.)
- "Resized images are still blurry with quality parameter set to 100 when using SKBitmap." — **comment by ViRuSTriNiTy (2022)** (User conflates JPEG quality (100) with resize filter quality; may not be using SKSamplingOptions with a cubic resampler.)
- "Any update on this issue as well? Still blurry in 2023" — **comment by ggolda (2023)** (Community still encountering blur, likely using deprecated SKFilterQuality path or default SKSamplingOptions.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 656-679 | direct | SKBitmap.Resize() overloads taking SKFilterQuality and SKBitmapResizeMethod are both marked [Obsolete]; modern overloads accept SKSamplingOptions. Resize(SKImageInfo, SKSamplingOptions) allocates a new SKBitmap and calls ScalePixels. No default high-quality overload exists — callers must explicitly choose a sampling option. |
| `binding/SkiaSharp/Definitions.cs` | 610-665 | direct | SKCubicResampler.Mitchell and SKCubicResampler.CatmullRom are the high-quality options. SKSamplingOptions.Default = new() resolves to nearest-neighbor (SKFilterMode.Nearest, SKMipmapMode.None), which would produce very blurry results if used inadvertently. SKFilterQuality.High maps to SKCubicResampler.Mitchell via ToSamplingOptions(). |
| `binding/SkiaSharp/SKPaint.cs` | 17-33 | related | SKFilterQuality is still present and [Obsolete]; its ToSamplingOptions() extension maps None→Nearest, Low→Linear, Medium→LinearMipmap, High→Mitchell cubic. SKPaint.FilterQuality property still accessible for canvas-draw path. |

### Workarounds

- Use SKBitmap.Resize(info, new SKSamplingOptions(SKCubicResampler.Mitchell)) for high-quality bicubic resizing.
- Use SKBitmap.Resize(info, new SKSamplingOptions(SKCubicResampler.CatmullRom)) as an alternative high-quality option.
- Avoid SKSamplingOptions.Default (nearest-neighbor) for downscaling — always specify the desired filter explicitly.

### Next Questions

- Is the quality of SKBitmap.Resize() with SKCubicResampler.Mitchell visually equivalent to the old SKBitmapResizeMethod.Lanczos3?
- Does canvas DrawBitmap/DrawImage with an SKPaint using SKSamplingOptions cubic produce equivalent quality to SKBitmap.Resize()?
- Is documentation updated to guide users away from the deprecated SKFilterQuality/SKBitmapResizeMethod APIs toward SKSamplingOptions?

### Resolution Proposals

**Hypothesis:** The original quality concern (blurry canvas-resize vs. crisp SKBitmap.Lanczos3) is substantially addressed by SKSamplingOptions with cubic resampler, but users lack guidance on adopting the new API.

1. **Verify modern API quality parity** — investigation, confidence 0.80 (80%), cost/s, validated=untested
   - Benchmark SKBitmap.Resize(info, new SKSamplingOptions(SKCubicResampler.Mitchell)) visually against old Lanczos3 output and confirm equivalence or document the delta.
2. **Add documentation / migration guide** — workaround, confidence 0.85 (85%), cost/s, validated=untested
   - Document the migration from SKFilterQuality/SKBitmapResizeMethod to SKSamplingOptions in the changelog or README, with a high-quality resize example using SKCubicResampler.Mitchell.

**Recommended proposal:** Add documentation / migration guide

**Why:** The code investigation shows the modern API provides the right tools; the remaining gap is user education. Documenting the migration from deprecated APIs to SKSamplingOptions would resolve most community confusion seen in the 2022-2023 comments.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.75 (75%) |
| Reason | Tracking issue with persistent community engagement; the modern SKSamplingOptions API exists but needs quality-comparison verification and user documentation to close the loop. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Apply enhancement, SkiaSharp area, Raster backend, and compatibility tenet labels | labels=type/enhancement, area/SkiaSharp, backend/Raster, tenet/compatibility |
| add-comment | medium | 0.80 (80%) | Explain that SKBitmapResizeMethod/SKFilterQuality are deprecated and guide users to SKSamplingOptions with cubic resampler | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the continued interest in this issue.

Since this issue was filed, the image resizing APIs in SkiaSharp have been significantly updated:

- `SKBitmapResizeMethod` and `SKFilterQuality` are now **deprecated**.
- The modern replacement is `SKSamplingOptions`, which gives you full control over the resampling algorithm.

For high-quality resizing (equivalent to the old `Lanczos3` or `High` quality), use the **Mitchell cubic resampler**:

```csharp
var resized = bitmap.Resize(new SKImageInfo(newWidth, newHeight),
    new SKSamplingOptions(SKCubicResampler.Mitchell));
```

Or with `CatmullRom`:

```csharp
var resized = bitmap.Resize(new SKImageInfo(newWidth, newHeight),
    new SKSamplingOptions(SKCubicResampler.CatmullRom));
```

Avoiding `SKSamplingOptions.Default` (nearest-neighbor) or not specifying a sampling option will produce blocky/blurry results.

We will update the tracking item once we've verified quality parity between the new cubic options and the old Lanczos3 path.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 981,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T01:41:00Z"
  },
  "summary": "Tracking issue filed by maintainer to investigate and improve image resize quality and performance in SkiaSharp, consolidating concerns from #319 and #520 about blurry output when resizing via canvas vs. SKBitmap.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.82
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.93
    },
    "backends": [
      "backend/Raster"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "relatedIssues": [
        319,
        520
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/319",
          "description": "AndersMad: SKImage DrawImage blurry vs SKBitmap Lanczos3 crisp (closed)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/520",
          "description": "dsyno: SKImage resize blurry; SKBitmap Lanczos3 crisp (closed, milestone v1.68.0)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/1007",
          "description": "PR referenced in comments: performance improvement for Windows"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.60.0",
        "1.68.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "User comments in 2022 and 2023 report that resizing is still blurry with the current API, suggesting the underlying concern persists even though the APIs have been modernized."
    }
  },
  "analysis": {
    "summary": "Tracking issue for image resize quality/performance. The original concerns in #319 and #520 (blurry canvas-drawn resize vs. crisp SKBitmap Lanczos3 resize) led the maintainer to open this as a watchlist item. Since then, SKBitmapResizeMethod and SKFilterQuality have been deprecated and replaced with SKSamplingOptions (which supports SKCubicResampler.Mitchell for high-quality bicubic resizing). However, 2022–2023 comments still report blurry output, suggesting either users are unaware of the new API or a real quality gap persists.",
    "rationale": "Classified as type/enhancement because the issue is a quality/performance tracking item, not a single reproducible bug with a specific stack trace. The API evolution (SKBitmapResizeMethod → SKSamplingOptions) partially addresses the original concern, but the open issue and recent community comments suggest a full investigation is warranted to verify current quality and produce migration guidance.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "656-679",
        "finding": "SKBitmap.Resize() overloads taking SKFilterQuality and SKBitmapResizeMethod are both marked [Obsolete]; modern overloads accept SKSamplingOptions. Resize(SKImageInfo, SKSamplingOptions) allocates a new SKBitmap and calls ScalePixels. No default high-quality overload exists — callers must explicitly choose a sampling option.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "610-665",
        "finding": "SKCubicResampler.Mitchell and SKCubicResampler.CatmullRom are the high-quality options. SKSamplingOptions.Default = new() resolves to nearest-neighbor (SKFilterMode.Nearest, SKMipmapMode.None), which would produce very blurry results if used inadvertently. SKFilterQuality.High maps to SKCubicResampler.Mitchell via ToSamplingOptions().",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "lines": "17-33",
        "finding": "SKFilterQuality is still present and [Obsolete]; its ToSamplingOptions() extension maps None→Nearest, Low→Linear, Medium→LinearMipmap, High→Mitchell cubic. SKPaint.FilterQuality property still accessible for canvas-draw path.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "We are working on a much later version of skia, so things may be better soon. But, just to keep track that this is an issue that exists.",
        "source": "issue body",
        "interpretation": "Maintainer filed this as a watchlist/tracking issue; expects a Skia upgrade to improve things."
      },
      {
        "text": "Resized images are still blurry with quality parameter set to 100 when using SKBitmap.",
        "source": "comment by ViRuSTriNiTy (2022)",
        "interpretation": "User conflates JPEG quality (100) with resize filter quality; may not be using SKSamplingOptions with a cubic resampler."
      },
      {
        "text": "Any update on this issue as well? Still blurry in 2023",
        "source": "comment by ggolda (2023)",
        "interpretation": "Community still encountering blur, likely using deprecated SKFilterQuality path or default SKSamplingOptions."
      }
    ],
    "workarounds": [
      "Use SKBitmap.Resize(info, new SKSamplingOptions(SKCubicResampler.Mitchell)) for high-quality bicubic resizing.",
      "Use SKBitmap.Resize(info, new SKSamplingOptions(SKCubicResampler.CatmullRom)) as an alternative high-quality option.",
      "Avoid SKSamplingOptions.Default (nearest-neighbor) for downscaling — always specify the desired filter explicitly."
    ],
    "nextQuestions": [
      "Is the quality of SKBitmap.Resize() with SKCubicResampler.Mitchell visually equivalent to the old SKBitmapResizeMethod.Lanczos3?",
      "Does canvas DrawBitmap/DrawImage with an SKPaint using SKSamplingOptions cubic produce equivalent quality to SKBitmap.Resize()?",
      "Is documentation updated to guide users away from the deprecated SKFilterQuality/SKBitmapResizeMethod APIs toward SKSamplingOptions?"
    ],
    "resolution": {
      "hypothesis": "The original quality concern (blurry canvas-resize vs. crisp SKBitmap.Lanczos3) is substantially addressed by SKSamplingOptions with cubic resampler, but users lack guidance on adopting the new API.",
      "proposals": [
        {
          "title": "Verify modern API quality parity",
          "description": "Benchmark SKBitmap.Resize(info, new SKSamplingOptions(SKCubicResampler.Mitchell)) visually against old Lanczos3 output and confirm equivalence or document the delta.",
          "category": "investigation",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Add documentation / migration guide",
          "description": "Document the migration from SKFilterQuality/SKBitmapResizeMethod to SKSamplingOptions in the changelog or README, with a high-quality resize example using SKCubicResampler.Mitchell.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add documentation / migration guide",
      "recommendedReason": "The code investigation shows the modern API provides the right tools; the remaining gap is user education. Documenting the migration from deprecated APIs to SKSamplingOptions would resolve most community confusion seen in the 2022-2023 comments."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.75,
      "reason": "Tracking issue with persistent community engagement; the modern SKSamplingOptions API exists but needs quality-comparison verification and user documentation to close the loop.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement, SkiaSharp area, Raster backend, and compatibility tenet labels",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp",
          "backend/Raster",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain that SKBitmapResizeMethod/SKFilterQuality are deprecated and guide users to SKSamplingOptions with cubic resampler",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the continued interest in this issue.\n\nSince this issue was filed, the image resizing APIs in SkiaSharp have been significantly updated:\n\n- `SKBitmapResizeMethod` and `SKFilterQuality` are now **deprecated**.\n- The modern replacement is `SKSamplingOptions`, which gives you full control over the resampling algorithm.\n\nFor high-quality resizing (equivalent to the old `Lanczos3` or `High` quality), use the **Mitchell cubic resampler**:\n\n```csharp\nvar resized = bitmap.Resize(new SKImageInfo(newWidth, newHeight),\n    new SKSamplingOptions(SKCubicResampler.Mitchell));\n```\n\nOr with `CatmullRom`:\n\n```csharp\nvar resized = bitmap.Resize(new SKImageInfo(newWidth, newHeight),\n    new SKSamplingOptions(SKCubicResampler.CatmullRom));\n```\n\nAvoiding `SKSamplingOptions.Default` (nearest-neighbor) or not specifying a sampling option will produce blocky/blurry results.\n\nWe will update the tracking item once we've verified quality parity between the new cubic options and the old Lanczos3 path."
      }
    ]
  }
}
```

</details>
