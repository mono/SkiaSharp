# Issue Triage Report — #2229

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T13:42:53Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp (0.88 (88%)) |
| Suggested action | needs-reproduction (0.80 (80%)) |

**Issue Summary:** SKImage.Encode produces visually corrupted red channel output when encoding to JPEG or WebP formats at any quality level, while PNG encoding of the same image produces correct results.

**Analysis:** SKImage.Encode routes through ToRasterImage(true) → PeekPixels() → SKPixmap.Encode(). The JPEG path uses SKJpegEncoderOptions with Downsample420 chroma subsampling by default, which can introduce color artifacts. The WebP lossy path passes through sk_webpencoder_encode. PNG works correctly, confirming the issue is in the lossy encoder path, potentially related to chroma subsampling or color space handling in the Skia C-level encoder.

**Recommendations:** **needs-reproduction** — The issue shows clear visual evidence of wrong output but provides no reproduction code. A minimal repro is needed to confirm the root cause (chroma subsampling vs color type mismatch) and verify on current versions.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Load an image with significant red channel content
2. Encode to JPEG or WebP using SKImage.Encode
3. Compare output to PNG-encoded version of the same image

**Environment:** SkiaSharp 2.80.2 and 2.88.1

**Repository links:**
- https://user-images.githubusercontent.com/2620599/187592969-61986bf9-9881-4322-ae96-fe3d21e18f6f.jpg — Screenshot showing red channel corruption in encoded output
- https://github.com/mono/SkiaSharp/issues/2643 — Related issue #2643 — color changes during JPEG encode on real devices (closed as completed)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2, 2.88.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | Issue filed in 2022 on 2.80.2 and 2.88.1; no reproduction code provided to confirm if still present in current versions. |

## Analysis

### Technical Summary

SKImage.Encode routes through ToRasterImage(true) → PeekPixels() → SKPixmap.Encode(). The JPEG path uses SKJpegEncoderOptions with Downsample420 chroma subsampling by default, which can introduce color artifacts. The WebP lossy path passes through sk_webpencoder_encode. PNG works correctly, confirming the issue is in the lossy encoder path, potentially related to chroma subsampling or color space handling in the Skia C-level encoder.

### Rationale

Reporter provides screenshots showing clear red channel distortion in JPEG/WebP output while PNG is correct. This is a wrong-output bug in the encoder path. The most likely cause is either chroma subsampling (Downsample420 default for JPEG) causing color artifacts in saturated red regions, or a color type mismatch (BGRA vs RGBA) during the ToRasterImage conversion that causes R and B channels to swap. No code reproduction is provided, making it a partial repro quality.

### Key Signals

- "PNG encoding has no issues so it cannot be a decoding issue" — **issue body** (Confirms the problem is in the JPEG/WebP encoder path, not decoding. PNG bypasses chroma subsampling so serves as a valid control.)
- "even on 100%" — **issue body** (Quality=100 for JPEG disables quantization artifacts but Downsample420 chroma subsampling is applied regardless of quality level, which can still produce color artifacts.)
- "Version with issue: 2.80.2, 2.88.1" — **issue body** (Issue present in multiple versions spanning a year, not a transient regression.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKImage.cs` | 363-373 | direct | SKImage.Encode(format, quality) calls ToRasterImage(true) then PeekPixels() and delegates to SKPixmap.Encode. The raster conversion step may silently change color type (e.g., BGRA to RGBA) which could cause channel swaps in JPEG/WebP output. |
| `binding/SkiaSharp/SKPixmap.cs` | 235-245 | direct | SKPixmap.Encode dispatches to JPEG/WebP/PNG encoders. For JPEG it constructs SKJpegEncoderOptions(quality) with default Downsample420 chroma subsampling, which reduces color resolution and can cause visible color artifacts in red channel. |
| `binding/SkiaSharp/Definitions.cs` | 555-563 | related | SKJpegEncoderOptions(int quality) defaults to Downsample420 and AlphaOption.Ignore. Downsample420 heavily subsamples chroma channels and is a known cause of color shift artifacts particularly visible in saturated red regions. |

### Workarounds

- Encode to PNG (lossless) if color fidelity is critical — PNG shows no issues.
- For JPEG: use SKPixmap.Encode(SKJpegEncoderOptions) directly with Downsample444 instead of Downsample420 to preserve chroma resolution: new SKJpegEncoderOptions(100, SKJpegEncoderDownsample.Downsample444, SKJpegEncoderAlphaOption.Ignore)

### Next Questions

- Does encoding via SKBitmap.Encode produce the same results as SKImage.Encode?
- Is the issue reproducible on current versions (2.88.x latest, or 3.x)?
- What color type is the source image (BGRA8888, RGBA8888)? A swapped R/B channel suggests a color type mismatch.
- Does using SKJpegEncoderDownsample.Downsample444 instead of Downsample420 resolve the issue?

### Resolution Proposals

**Hypothesis:** The Downsample420 default in SKJpegEncoderOptions causes visible chroma artifacts in saturated red regions. Alternatively, a BGRA/RGBA color type mismatch during rasterization causes channel swapping visible in the red channel.

1. **Use Downsample444 for higher quality JPEG encoding** — workaround, confidence 0.65 (65%), cost/xs, validated=untested
   - As a workaround, use SKPixmap.Encode directly with Downsample444 to preserve full chroma resolution and avoid color artifacts.

```csharp
using var pixmap = image.PeekPixels();
var options = new SKJpegEncoderOptions(100, SKJpegEncoderDownsample.Downsample444, SKJpegEncoderAlphaOption.Ignore);
using var data = pixmap?.Encode(options);
```
2. **Investigate color type in ToRasterImage path** — investigation, confidence 0.70 (70%), cost/s, validated=untested
   - Trace whether sk_image_make_raster_image changes the color type from BGRA to RGBA, causing R/B swap in JPEG/WebP output.

**Recommended proposal:** Investigate color type in ToRasterImage path

**Why:** A color type channel swap (BGRA vs RGBA) would produce exactly the red-blue swap visible in screenshots. This needs investigation before a fix can be designed.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-reproduction |
| Confidence | 0.80 (80%) |
| Reason | The issue shows clear visual evidence of wrong output but provides no reproduction code. A minimal repro is needed to confirm the root cause (chroma subsampling vs color type mismatch) and verify on current versions. |
| Suggested repro platform | linux |

### Missing Info

- Minimal reproduction code showing how the image is created/loaded and encoded
- What color type the source image uses (BGRA8888, RGBA8888, etc.)
- Whether the issue reproduces on current versions (2.88.x or 3.x)

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug and SkiaSharp labels | labels=type/bug, area/SkiaSharp |
| add-comment | medium | 0.80 (80%) | Ask for minimal reproduction code and suggest chroma subsampling workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the report and the screenshots clearly showing the red channel distortion.

To reproduce and investigate this, could you provide a minimal code snippet showing:
1. How the source image is loaded (e.g., `SKImage.FromEncodedData`, `SKBitmap.Decode`, etc.)
2. The exact encode call used
3. Whether the issue reproduces on the latest SkiaSharp version

In the meantime, you can try two workarounds:

**Option 1 — Use PNG** (lossless, no color loss):
```csharp
using var data = image.Encode(SKEncodedImageFormat.Png, 100);
```

**Option 2 — Use Downsample444 for JPEG** (preserves chroma at full resolution):
```csharp
using var pixmap = image.PeekPixels();
var options = new SKJpegEncoderOptions(100, SKJpegEncoderDownsample.Downsample444, SKJpegEncoderAlphaOption.Ignore);
using var data = pixmap?.Encode(options);
```

The default JPEG encoder uses `Downsample420` chroma subsampling which can introduce visible color artifacts in saturated red regions, even at quality=100. The `Downsample444` option preserves full color information.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2229,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T13:42:53Z"
  },
  "summary": "SKImage.Encode produces visually corrupted red channel output when encoding to JPEG or WebP formats at any quality level, while PNG encoding of the same image produces correct results.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.88
    }
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Load an image with significant red channel content",
        "Encode to JPEG or WebP using SKImage.Encode",
        "Compare output to PNG-encoded version of the same image"
      ],
      "environmentDetails": "SkiaSharp 2.80.2 and 2.88.1",
      "repoLinks": [
        {
          "url": "https://user-images.githubusercontent.com/2620599/187592969-61986bf9-9881-4322-ae96-fe3d21e18f6f.jpg",
          "description": "Screenshot showing red channel corruption in encoded output"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2643",
          "description": "Related issue #2643 — color changes during JPEG encode on real devices (closed as completed)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2",
        "2.88.1"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "Issue filed in 2022 on 2.80.2 and 2.88.1; no reproduction code provided to confirm if still present in current versions."
    }
  },
  "analysis": {
    "summary": "SKImage.Encode routes through ToRasterImage(true) → PeekPixels() → SKPixmap.Encode(). The JPEG path uses SKJpegEncoderOptions with Downsample420 chroma subsampling by default, which can introduce color artifacts. The WebP lossy path passes through sk_webpencoder_encode. PNG works correctly, confirming the issue is in the lossy encoder path, potentially related to chroma subsampling or color space handling in the Skia C-level encoder.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "363-373",
        "finding": "SKImage.Encode(format, quality) calls ToRasterImage(true) then PeekPixels() and delegates to SKPixmap.Encode. The raster conversion step may silently change color type (e.g., BGRA to RGBA) which could cause channel swaps in JPEG/WebP output.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPixmap.cs",
        "lines": "235-245",
        "finding": "SKPixmap.Encode dispatches to JPEG/WebP/PNG encoders. For JPEG it constructs SKJpegEncoderOptions(quality) with default Downsample420 chroma subsampling, which reduces color resolution and can cause visible color artifacts in red channel.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "555-563",
        "finding": "SKJpegEncoderOptions(int quality) defaults to Downsample420 and AlphaOption.Ignore. Downsample420 heavily subsamples chroma channels and is a known cause of color shift artifacts particularly visible in saturated red regions.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "PNG encoding has no issues so it cannot be a decoding issue",
        "source": "issue body",
        "interpretation": "Confirms the problem is in the JPEG/WebP encoder path, not decoding. PNG bypasses chroma subsampling so serves as a valid control."
      },
      {
        "text": "even on 100%",
        "source": "issue body",
        "interpretation": "Quality=100 for JPEG disables quantization artifacts but Downsample420 chroma subsampling is applied regardless of quality level, which can still produce color artifacts."
      },
      {
        "text": "Version with issue: 2.80.2, 2.88.1",
        "source": "issue body",
        "interpretation": "Issue present in multiple versions spanning a year, not a transient regression."
      }
    ],
    "workarounds": [
      "Encode to PNG (lossless) if color fidelity is critical — PNG shows no issues.",
      "For JPEG: use SKPixmap.Encode(SKJpegEncoderOptions) directly with Downsample444 instead of Downsample420 to preserve chroma resolution: new SKJpegEncoderOptions(100, SKJpegEncoderDownsample.Downsample444, SKJpegEncoderAlphaOption.Ignore)"
    ],
    "nextQuestions": [
      "Does encoding via SKBitmap.Encode produce the same results as SKImage.Encode?",
      "Is the issue reproducible on current versions (2.88.x latest, or 3.x)?",
      "What color type is the source image (BGRA8888, RGBA8888)? A swapped R/B channel suggests a color type mismatch.",
      "Does using SKJpegEncoderDownsample.Downsample444 instead of Downsample420 resolve the issue?"
    ],
    "rationale": "Reporter provides screenshots showing clear red channel distortion in JPEG/WebP output while PNG is correct. This is a wrong-output bug in the encoder path. The most likely cause is either chroma subsampling (Downsample420 default for JPEG) causing color artifacts in saturated red regions, or a color type mismatch (BGRA vs RGBA) during the ToRasterImage conversion that causes R and B channels to swap. No code reproduction is provided, making it a partial repro quality.",
    "resolution": {
      "hypothesis": "The Downsample420 default in SKJpegEncoderOptions causes visible chroma artifacts in saturated red regions. Alternatively, a BGRA/RGBA color type mismatch during rasterization causes channel swapping visible in the red channel.",
      "proposals": [
        {
          "title": "Use Downsample444 for higher quality JPEG encoding",
          "description": "As a workaround, use SKPixmap.Encode directly with Downsample444 to preserve full chroma resolution and avoid color artifacts.",
          "category": "workaround",
          "codeSnippet": "using var pixmap = image.PeekPixels();\nvar options = new SKJpegEncoderOptions(100, SKJpegEncoderDownsample.Downsample444, SKJpegEncoderAlphaOption.Ignore);\nusing var data = pixmap?.Encode(options);",
          "confidence": 0.65,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Investigate color type in ToRasterImage path",
          "description": "Trace whether sk_image_make_raster_image changes the color type from BGRA to RGBA, causing R/B swap in JPEG/WebP output.",
          "category": "investigation",
          "confidence": 0.7,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Investigate color type in ToRasterImage path",
      "recommendedReason": "A color type channel swap (BGRA vs RGBA) would produce exactly the red-blue swap visible in screenshots. This needs investigation before a fix can be designed."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-reproduction",
      "confidence": 0.8,
      "reason": "The issue shows clear visual evidence of wrong output but provides no reproduction code. A minimal repro is needed to confirm the root cause (chroma subsampling vs color type mismatch) and verify on current versions.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Minimal reproduction code showing how the image is created/loaded and encoded",
      "What color type the source image uses (BGRA8888, RGBA8888, etc.)",
      "Whether the issue reproduces on current versions (2.88.x or 3.x)"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug and SkiaSharp labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask for minimal reproduction code and suggest chroma subsampling workaround",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thank you for the report and the screenshots clearly showing the red channel distortion.\n\nTo reproduce and investigate this, could you provide a minimal code snippet showing:\n1. How the source image is loaded (e.g., `SKImage.FromEncodedData`, `SKBitmap.Decode`, etc.)\n2. The exact encode call used\n3. Whether the issue reproduces on the latest SkiaSharp version\n\nIn the meantime, you can try two workarounds:\n\n**Option 1 — Use PNG** (lossless, no color loss):\n```csharp\nusing var data = image.Encode(SKEncodedImageFormat.Png, 100);\n```\n\n**Option 2 — Use Downsample444 for JPEG** (preserves chroma at full resolution):\n```csharp\nusing var pixmap = image.PeekPixels();\nvar options = new SKJpegEncoderOptions(100, SKJpegEncoderDownsample.Downsample444, SKJpegEncoderAlphaOption.Ignore);\nusing var data = pixmap?.Encode(options);\n```\n\nThe default JPEG encoder uses `Downsample420` chroma subsampling which can introduce visible color artifacts in saturated red regions, even at quality=100. The `Downsample444` option preserves full color information."
      }
    ]
  }
}
```

</details>
