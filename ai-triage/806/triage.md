# Issue Triage Report — #806

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-06-06T05:30:00Z |
| Type | type/bug (0.72 (72%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-not-a-bug (0.80 (80%)) |

**Issue Summary:** Reporter creates a Gray8 SKImage using SKImage.FromPixelCopy with SKColorType.Gray8, then calls SKBitmap.FromImage which always converts to the platform native color type (Bgra8888 on Windows), resulting in a 4-bytes-per-pixel bitmap instead of the expected 1-byte-per-pixel Gray8 bitmap.

**Analysis:** SKBitmap.FromImage unconditionally creates the output bitmap using SKImageInfo.PlatformColorType (Bgra8888 on Windows), converting any source image—including Gray8—to the platform native format. The user should use image.ReadPixels() with an explicitly constructed Gray8 SKImageInfo instead.

**Recommendations:** **close-as-not-a-bug** — SKBitmap.FromImage is designed to produce a platform-native bitmap; behavior is intentional. Trivial workaround using ReadPixels directly is available.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Create SKImageInfo with SKColorType.Gray8 for a given width/height
2. Create SKData from Gray8 raw pixel buffer using SKData.CreateCopy
3. Call SKImage.FromPixelCopy(imageInfo, data.Data, size.Width) to create the image
4. Call SKBitmap.FromImage(image)
5. Observe resulting bitmap has ColorType=Bgra8888 (4 bytes/pixel) instead of Gray8 (1 byte/pixel)

**Environment:** Visual Studio 2017 v15.7.1, .NET Core 2.0, Windows 10, SkiaSharp 1.68.0

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | .net core 2.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SKBitmap.FromImage still hardcodes PlatformColorType in the current codebase and has not been changed to preserve source image color type. |

## Analysis

### Technical Summary

SKBitmap.FromImage unconditionally creates the output bitmap using SKImageInfo.PlatformColorType (Bgra8888 on Windows), converting any source image—including Gray8—to the platform native format. The user should use image.ReadPixels() with an explicitly constructed Gray8 SKImageInfo instead.

### Rationale

The behavior is by design: SKBitmap.FromImage creates a platform-native bitmap for compatibility with platform drawing APIs. Code investigation confirms the source sets PlatformColorType directly. However, the behavior is non-obvious to users who expect output format to match input image format. A trivial workaround using ReadPixels directly is available.

### Key Signals

- "the bitmap become 4 bytes per pixel, color type become bgra8888" — **issue body** (SKBitmap.FromImage converts Gray8 source to Bgra8888 because it always uses PlatformColorType, not the source image's color type.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 715-728 | direct | SKBitmap.FromImage hardcodes SKImageInfo.PlatformColorType when constructing the output bitmap info, causing conversion of any source color type (including Gray8) to the platform native format (Bgra8888 on Windows). |
| `binding/SkiaSharp/SKImage.cs` | 483-490 | related | SKImage.ReadPixels has overloads accepting a custom SKImageInfo destination, allowing callers to specify Gray8 as the target format and avoid automatic platform-format conversion. |

### Workarounds

- Manually create an SKBitmap with Gray8 SKImageInfo and use image.ReadPixels() to copy pixels directly without color conversion.

### Resolution Proposals

**Hypothesis:** SKBitmap.FromImage always converts to PlatformColorType. The user needs to use SKImage.ReadPixels with an explicitly constructed Gray8 SKImageInfo to preserve the original format.

1. **Use ReadPixels with explicit Gray8 info** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Instead of SKBitmap.FromImage, manually construct a Gray8 SKBitmap and call image.ReadPixels() with a matching Gray8 SKImageInfo. This copies pixels without any color type conversion.

```csharp
var bitmapInfo = new SKImageInfo(image.Width, image.Height, SKColorType.Gray8, SKAlphaType.Opaque);
var bitmap = new SKBitmap(bitmapInfo);
image.ReadPixels(bitmapInfo, bitmap.GetPixels(), bitmapInfo.RowBytes, 0, 0);
// bitmap.ColorType is now Gray8, 1 byte per pixel
```

**Recommended proposal:** Use ReadPixels with explicit Gray8 info

**Why:** SKImage.ReadPixels accepts a target SKImageInfo giving full control over output color type. It is the correct API for format-preserving pixel extraction.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.80 (80%) |
| Reason | SKBitmap.FromImage is designed to produce a platform-native bitmap; behavior is intentional. Trivial workaround using ReadPixels directly is available. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, SkiaSharp area, and Windows Classic platform labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic |
| add-comment | medium | 0.90 (90%) | Explain that SKBitmap.FromImage converts to platform color type and provide ReadPixels workaround | — |
| close-issue | medium | 0.75 (75%) | Close as not a bug — behavior is by design, workaround documented in comment | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report!

The behavior you're seeing is by design: `SKBitmap.FromImage` always creates a bitmap using the platform's native color type (`SKImageInfo.PlatformColorType`, which is `Bgra8888` on Windows), regardless of the source image's format. This is intentional for compatibility with platform drawing APIs.

To get a `Gray8` bitmap that preserves the original 1-byte-per-pixel format, use `SKImage.ReadPixels` directly with an explicitly constructed `Gray8` `SKImageInfo`:

```csharp
var bitmapInfo = new SKImageInfo(image.Width, image.Height, SKColorType.Gray8, SKAlphaType.Opaque);
var bitmap = new SKBitmap(bitmapInfo);
image.ReadPixels(bitmapInfo, bitmap.GetPixels(), bitmapInfo.RowBytes, 0, 0);
// bitmap.ColorType == SKColorType.Gray8, 1 byte per pixel
```

This copies the pixels from the image directly into the `Gray8` bitmap without any color conversion.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 806,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-06-06T05:30:00Z"
  },
  "summary": "Reporter creates a Gray8 SKImage using SKImage.FromPixelCopy with SKColorType.Gray8, then calls SKBitmap.FromImage which always converts to the platform native color type (Bgra8888 on Windows), resulting in a 4-bytes-per-pixel bitmap instead of the expected 1-byte-per-pixel Gray8 bitmap.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.72
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-Classic"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "low",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "reproQuality": "partial",
      "targetFrameworks": [
        ".net core 2.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create SKImageInfo with SKColorType.Gray8 for a given width/height",
        "Create SKData from Gray8 raw pixel buffer using SKData.CreateCopy",
        "Call SKImage.FromPixelCopy(imageInfo, data.Data, size.Width) to create the image",
        "Call SKBitmap.FromImage(image)",
        "Observe resulting bitmap has ColorType=Bgra8888 (4 bytes/pixel) instead of Gray8 (1 byte/pixel)"
      ],
      "environmentDetails": "Visual Studio 2017 v15.7.1, .NET Core 2.0, Windows 10, SkiaSharp 1.68.0"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "SKBitmap.FromImage still hardcodes PlatformColorType in the current codebase and has not been changed to preserve source image color type."
    }
  },
  "analysis": {
    "summary": "SKBitmap.FromImage unconditionally creates the output bitmap using SKImageInfo.PlatformColorType (Bgra8888 on Windows), converting any source image—including Gray8—to the platform native format. The user should use image.ReadPixels() with an explicitly constructed Gray8 SKImageInfo instead.",
    "rationale": "The behavior is by design: SKBitmap.FromImage creates a platform-native bitmap for compatibility with platform drawing APIs. Code investigation confirms the source sets PlatformColorType directly. However, the behavior is non-obvious to users who expect output format to match input image format. A trivial workaround using ReadPixels directly is available.",
    "keySignals": [
      {
        "text": "the bitmap become 4 bytes per pixel, color type become bgra8888",
        "source": "issue body",
        "interpretation": "SKBitmap.FromImage converts Gray8 source to Bgra8888 because it always uses PlatformColorType, not the source image's color type."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "715-728",
        "finding": "SKBitmap.FromImage hardcodes SKImageInfo.PlatformColorType when constructing the output bitmap info, causing conversion of any source color type (including Gray8) to the platform native format (Bgra8888 on Windows).",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "483-490",
        "finding": "SKImage.ReadPixels has overloads accepting a custom SKImageInfo destination, allowing callers to specify Gray8 as the target format and avoid automatic platform-format conversion.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Manually create an SKBitmap with Gray8 SKImageInfo and use image.ReadPixels() to copy pixels directly without color conversion."
    ],
    "resolution": {
      "hypothesis": "SKBitmap.FromImage always converts to PlatformColorType. The user needs to use SKImage.ReadPixels with an explicitly constructed Gray8 SKImageInfo to preserve the original format.",
      "proposals": [
        {
          "title": "Use ReadPixels with explicit Gray8 info",
          "description": "Instead of SKBitmap.FromImage, manually construct a Gray8 SKBitmap and call image.ReadPixels() with a matching Gray8 SKImageInfo. This copies pixels without any color type conversion.",
          "category": "workaround",
          "codeSnippet": "var bitmapInfo = new SKImageInfo(image.Width, image.Height, SKColorType.Gray8, SKAlphaType.Opaque);\nvar bitmap = new SKBitmap(bitmapInfo);\nimage.ReadPixels(bitmapInfo, bitmap.GetPixels(), bitmapInfo.RowBytes, 0, 0);\n// bitmap.ColorType is now Gray8, 1 byte per pixel",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Use ReadPixels with explicit Gray8 info",
      "recommendedReason": "SKImage.ReadPixels accepts a target SKImageInfo giving full control over output color type. It is the correct API for format-preserving pixel extraction."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.8,
      "reason": "SKBitmap.FromImage is designed to produce a platform-native bitmap; behavior is intentional. Trivial workaround using ReadPixels directly is available.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, SkiaSharp area, and Windows Classic platform labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain that SKBitmap.FromImage converts to platform color type and provide ReadPixels workaround",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the detailed report!\n\nThe behavior you're seeing is by design: `SKBitmap.FromImage` always creates a bitmap using the platform's native color type (`SKImageInfo.PlatformColorType`, which is `Bgra8888` on Windows), regardless of the source image's format. This is intentional for compatibility with platform drawing APIs.\n\nTo get a `Gray8` bitmap that preserves the original 1-byte-per-pixel format, use `SKImage.ReadPixels` directly with an explicitly constructed `Gray8` `SKImageInfo`:\n\n```csharp\nvar bitmapInfo = new SKImageInfo(image.Width, image.Height, SKColorType.Gray8, SKAlphaType.Opaque);\nvar bitmap = new SKBitmap(bitmapInfo);\nimage.ReadPixels(bitmapInfo, bitmap.GetPixels(), bitmapInfo.RowBytes, 0, 0);\n// bitmap.ColorType == SKColorType.Gray8, 1 byte per pixel\n```\n\nThis copies the pixels from the image directly into the `Gray8` bitmap without any color conversion."
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — behavior is by design, workaround documented in comment",
        "risk": "medium",
        "confidence": 0.75,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
