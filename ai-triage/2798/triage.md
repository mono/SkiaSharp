# Issue Triage Report — #2798

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:36:07Z |
| Type | type/bug (0.85 (85%)) |
| Area | area/SkiaSharp (0.88 (88%)) |
| Suggested action | needs-investigation (0.78 (78%)) |

**Issue Summary:** Reporter creates a 100×100 bitmap with solid red color and encodes it as JPEG on Xamarin Android (SkiaSharp 2.88.6); the JPEG output shows a different background color than PNG-encoded output of the same bitmap, but the issue is not observed on Windows.

**Analysis:** JPEG encoding of a solid-color raster bitmap produces different color output on Android compared to PNG. The default JPEG encoder uses Downsample420 (4:2:0 chroma subsampling) and YCbCr color space conversion, which introduces color shifts. The platform-specific behavior (Android differs from Windows) may relate to the platform default color type (RGBA_8888 on Android vs BGRA_8888 on Windows) or may be similar to the behavior in #2643 (JPEG encoding changing colors on physical devices, closed as completed but filed against same 2.88.6 version).

**Recommendations:** **needs-investigation** — Android-specific JPEG color difference with partial code repro. Related issue #2643 was closed as completed but reporter uses same 2.88.6 version. Needs reproduction to determine if this is expected JPEG YCbCr behavior or a platform-specific channel-order bug.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Android |
| Backends | backend/Raster |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, os/Android, area/SkiaSharp, backend/Raster, tenet/reliability, triage/triaged |

## Evidence

### Reproduction

1. Create a new SKBitmap(100, 100)
2. Create an SKCanvas from the bitmap
3. Draw a solid red rectangle (SKColor 255, 0, 0, 255) filling the full canvas
4. Encode the bitmap as JPEG at quality 100 using bitmap.Encode(SKEncodedImageFormat.Jpeg, 100)
5. Save output to MemoryStream and display
6. Compare color of JPEG output to PNG output from the same bitmap — colors differ on Android but not on Windows

**Environment:** Xamarin Android, Oppo physical device, SkiaSharp 2.88.6, Visual Studio on Windows

**Related issues:** #931, #2643

**Repository links:**
- https://github.com/mono/SkiaSharp/assets/131945130/dcfff30a-013a-444d-bd05-22e098091a08 — Screenshot showing the color difference between PNG and JPEG output
- https://github.com/mono/SkiaSharp/issues/931 — Related open issue: JPEG color changed after decode/encode
- https://github.com/mono/SkiaSharp/issues/2643 — Related closed issue: Broken Bitmap.Encode on real Android/iOS devices in 2.88.6 (closed as completed Jan 2024)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | xamarin.android |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.6 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Reporter lists 2.88.6 as both current and last known good, indicating no regression baseline. Related issue #2643 also affected 2.88.6 and was closed as completed; this may be the same or a related underlying issue. |

## Analysis

### Technical Summary

JPEG encoding of a solid-color raster bitmap produces different color output on Android compared to PNG. The default JPEG encoder uses Downsample420 (4:2:0 chroma subsampling) and YCbCr color space conversion, which introduces color shifts. The platform-specific behavior (Android differs from Windows) may relate to the platform default color type (RGBA_8888 on Android vs BGRA_8888 on Windows) or may be similar to the behavior in #2643 (JPEG encoding changing colors on physical devices, closed as completed but filed against same 2.88.6 version).

### Rationale

Classified as type/bug because platform-specific behavior is unexpected — the same code produces visually different JPEG output on Android vs Windows. While some JPEG color shift is expected (YCbCr conversion + 4:2:0 chroma subsampling), the Android-specific nature implies a platform-level difference. Area is area/SkiaSharp as the issue is in the core encoding path (SKBitmap.Encode). Severity is low because a workaround exists and wrong output does not crash. Closely related to #2643 (also 2.88.6, physical Android devices, JPEG color change) though that was closed as completed.

### Key Signals

- "JPEG image displays a different background color than the PNG image. This issue only occurs in Xamarin Android" — **issue body** (Platform-specific JPEG color difference with identical code path strongly suggests either different default pixel format (RGBA_8888 vs BGRA_8888) or JPEG YCbCr conversion behaving differently due to color space metadata.)
- "Color = new SKColor(255, 0, 0, 255)" — **issue body** (Pure opaque red at maximum saturation. JPEG 4:2:0 chroma subsampling most visibly distorts highly saturated single-channel colors. Alpha is fully opaque so BlendOnBlack alpha handling is not a factor.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 732-750 | direct | SKBitmap.Encode(SKEncodedImageFormat.Jpeg, quality) delegates to SKPixmap.Encode via PeekPixels(), which uses SKJpegEncoderOptions(quality) with default Downsample420 chroma subsampling — lossy even at quality 100 |
| `binding/SkiaSharp/Definitions.cs` | 549-565 | direct | SKJpegEncoderOptions single-arg constructor sets Downsample420 (4:2:0 chroma subsampling) and AlphaOption.Ignore by default — 4:2:0 reduces chroma resolution by 4x and can cause visible color shift for pure saturated colors like red |
| `binding/SkiaSharp/SKImageInfo.cs` | 40-48 | related | PlatformColorType is determined at runtime via sk_colortype_get_default_8888(); this returns RGBA_8888 on Android and BGRA_8888 on Windows — if the JPEG encoder reads pixel data without fully accounting for channel ordering, this would produce platform-specific color output |

### Workarounds

- Use PNG encoding instead of JPEG for lossless, color-accurate output: bitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(imageStream)
- Use SKJpegEncoderOptions with Downsample444 to eliminate 4:2:0 chroma subsampling, which may reduce color shift on Android

### Next Questions

- What exact color difference is observed? (slightly different hue vs drastically different color — helps distinguish YCbCr chroma shift from channel order swap)
- Does this occur on an Android emulator as well as physical devices, or only physical devices?
- Does the issue reproduce with SkiaSharp 3.x?
- What is SKImageInfo.PlatformColorType on the Android device? (helps determine if RGBA vs BGRA channel ordering is involved)

### Resolution Proposals

**Hypothesis:** Default JPEG encoder uses 4:2:0 chroma subsampling (Downsample420) which causes color shifts for pure saturated colors. Additionally, the platform default color type on Android (RGBA_8888) may differ from Windows (BGRA_8888), causing the JPEG encoder to interpret color channels differently and produce platform-specific results.

1. **Use PNG instead of JPEG** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - Encode as PNG instead of JPEG for lossless, color-accurate output. PNG does not perform YCbCr conversion or chroma subsampling. This is the simplest fix if JPEG format is not strictly required.
2. **Use Downsample444 to eliminate chroma subsampling** — workaround, confidence 0.60 (60%), cost/s, validated=yes
   - Explicitly use SKJpegEncoderOptions with Downsample444 to disable 4:2:0 chroma subsampling. This preserves full color resolution and may reduce platform-specific color shifts.

```csharp
var options = new SKJpegEncoderOptions(100, SKJpegEncoderDownsample.Downsample444, SKJpegEncoderAlphaOption.Ignore);
using var pixmap = bitmap.PeekPixels();
using var data = pixmap?.Encode(options);
data?.SaveTo(imageStream);
```

**Recommended proposal:** Use PNG instead of JPEG

**Why:** PNG encoding is lossless, platform-independent, and requires a minimal code change. Use the Downsample444 option as a secondary approach if JPEG output format is required.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.78 (78%) |
| Reason | Android-specific JPEG color difference with partial code repro. Related issue #2643 was closed as completed but reporter uses same 2.88.6 version. Needs reproduction to determine if this is expected JPEG YCbCr behavior or a platform-specific channel-order bug. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Labels already match classification; no changes needed | labels=type/bug, area/SkiaSharp, os/Android, backend/Raster, tenet/reliability |
| add-comment | medium | 0.78 (78%) | Ask for additional platform details and provide PNG and Downsample444 workarounds | — |
| link-related | low | 0.80 (80%) | Link to related open JPEG color issue #931 | linkedIssue=#931 |
| link-related | low | 0.85 (85%) | Link to closely related closed issue #2643 (JPEG encoding color change on physical devices) | linkedIssue=#2643 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report!

JPEG encoding uses a YCbCr color space conversion internally along with 4:2:0 chroma subsampling by default — this can cause visible color shifts compared to PNG, especially for pure saturated colors like red. However, the Android-specific behavior you're seeing is worth investigating.

A few questions to help narrow this down:
1. What does the color difference look like exactly? (e.g., a slightly different hue of red, or is it a completely different color?)
2. Does the issue occur on an Android emulator, or only on physical devices?
3. Are you able to test with SkiaSharp 3.x?
4. What is `SKImageInfo.PlatformColorType` on your device?

**Workaround 1 — Use PNG (simplest):**
```csharp
bitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(imageStream);
```

**Workaround 2 — Disable chroma subsampling:**
```csharp
var options = new SKJpegEncoderOptions(100, SKJpegEncoderDownsample.Downsample444, SKJpegEncoderAlphaOption.Ignore);
using var pixmap = bitmap.PeekPixels();
using var data = pixmap?.Encode(options);
data?.SaveTo(imageStream);
```

This may be related to [#2643](https://github.com/mono/SkiaSharp/issues/2643) which reported JPEG encoding color changes on physical Android/iOS devices with SkiaSharp 2.88.6.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2798,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:36:07Z",
    "currentLabels": [
      "type/bug",
      "os/Android",
      "area/SkiaSharp",
      "backend/Raster",
      "tenet/reliability",
      "triage/triaged"
    ]
  },
  "summary": "Reporter creates a 100×100 bitmap with solid red color and encodes it as JPEG on Xamarin Android (SkiaSharp 2.88.6); the JPEG output shows a different background color than PNG-encoded output of the same bitmap, but the issue is not observed on Windows.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.85
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.88
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
        "xamarin.android"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a new SKBitmap(100, 100)",
        "Create an SKCanvas from the bitmap",
        "Draw a solid red rectangle (SKColor 255, 0, 0, 255) filling the full canvas",
        "Encode the bitmap as JPEG at quality 100 using bitmap.Encode(SKEncodedImageFormat.Jpeg, 100)",
        "Save output to MemoryStream and display",
        "Compare color of JPEG output to PNG output from the same bitmap — colors differ on Android but not on Windows"
      ],
      "environmentDetails": "Xamarin Android, Oppo physical device, SkiaSharp 2.88.6, Visual Studio on Windows",
      "relatedIssues": [
        931,
        2643
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/assets/131945130/dcfff30a-013a-444d-bd05-22e098091a08",
          "description": "Screenshot showing the color difference between PNG and JPEG output"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/931",
          "description": "Related open issue: JPEG color changed after decode/encode"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2643",
          "description": "Related closed issue: Broken Bitmap.Encode on real Android/iOS devices in 2.88.6 (closed as completed Jan 2024)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.6"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Reporter lists 2.88.6 as both current and last known good, indicating no regression baseline. Related issue #2643 also affected 2.88.6 and was closed as completed; this may be the same or a related underlying issue."
    }
  },
  "analysis": {
    "summary": "JPEG encoding of a solid-color raster bitmap produces different color output on Android compared to PNG. The default JPEG encoder uses Downsample420 (4:2:0 chroma subsampling) and YCbCr color space conversion, which introduces color shifts. The platform-specific behavior (Android differs from Windows) may relate to the platform default color type (RGBA_8888 on Android vs BGRA_8888 on Windows) or may be similar to the behavior in #2643 (JPEG encoding changing colors on physical devices, closed as completed but filed against same 2.88.6 version).",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "732-750",
        "finding": "SKBitmap.Encode(SKEncodedImageFormat.Jpeg, quality) delegates to SKPixmap.Encode via PeekPixels(), which uses SKJpegEncoderOptions(quality) with default Downsample420 chroma subsampling — lossy even at quality 100",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "549-565",
        "finding": "SKJpegEncoderOptions single-arg constructor sets Downsample420 (4:2:0 chroma subsampling) and AlphaOption.Ignore by default — 4:2:0 reduces chroma resolution by 4x and can cause visible color shift for pure saturated colors like red",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImageInfo.cs",
        "lines": "40-48",
        "finding": "PlatformColorType is determined at runtime via sk_colortype_get_default_8888(); this returns RGBA_8888 on Android and BGRA_8888 on Windows — if the JPEG encoder reads pixel data without fully accounting for channel ordering, this would produce platform-specific color output",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "JPEG image displays a different background color than the PNG image. This issue only occurs in Xamarin Android",
        "source": "issue body",
        "interpretation": "Platform-specific JPEG color difference with identical code path strongly suggests either different default pixel format (RGBA_8888 vs BGRA_8888) or JPEG YCbCr conversion behaving differently due to color space metadata."
      },
      {
        "text": "Color = new SKColor(255, 0, 0, 255)",
        "source": "issue body",
        "interpretation": "Pure opaque red at maximum saturation. JPEG 4:2:0 chroma subsampling most visibly distorts highly saturated single-channel colors. Alpha is fully opaque so BlendOnBlack alpha handling is not a factor."
      }
    ],
    "rationale": "Classified as type/bug because platform-specific behavior is unexpected — the same code produces visually different JPEG output on Android vs Windows. While some JPEG color shift is expected (YCbCr conversion + 4:2:0 chroma subsampling), the Android-specific nature implies a platform-level difference. Area is area/SkiaSharp as the issue is in the core encoding path (SKBitmap.Encode). Severity is low because a workaround exists and wrong output does not crash. Closely related to #2643 (also 2.88.6, physical Android devices, JPEG color change) though that was closed as completed.",
    "workarounds": [
      "Use PNG encoding instead of JPEG for lossless, color-accurate output: bitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(imageStream)",
      "Use SKJpegEncoderOptions with Downsample444 to eliminate 4:2:0 chroma subsampling, which may reduce color shift on Android"
    ],
    "nextQuestions": [
      "What exact color difference is observed? (slightly different hue vs drastically different color — helps distinguish YCbCr chroma shift from channel order swap)",
      "Does this occur on an Android emulator as well as physical devices, or only physical devices?",
      "Does the issue reproduce with SkiaSharp 3.x?",
      "What is SKImageInfo.PlatformColorType on the Android device? (helps determine if RGBA vs BGRA channel ordering is involved)"
    ],
    "resolution": {
      "hypothesis": "Default JPEG encoder uses 4:2:0 chroma subsampling (Downsample420) which causes color shifts for pure saturated colors. Additionally, the platform default color type on Android (RGBA_8888) may differ from Windows (BGRA_8888), causing the JPEG encoder to interpret color channels differently and produce platform-specific results.",
      "proposals": [
        {
          "title": "Use PNG instead of JPEG",
          "description": "Encode as PNG instead of JPEG for lossless, color-accurate output. PNG does not perform YCbCr conversion or chroma subsampling. This is the simplest fix if JPEG format is not strictly required.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Use Downsample444 to eliminate chroma subsampling",
          "description": "Explicitly use SKJpegEncoderOptions with Downsample444 to disable 4:2:0 chroma subsampling. This preserves full color resolution and may reduce platform-specific color shifts.",
          "codeSnippet": "var options = new SKJpegEncoderOptions(100, SKJpegEncoderDownsample.Downsample444, SKJpegEncoderAlphaOption.Ignore);\nusing var pixmap = bitmap.PeekPixels();\nusing var data = pixmap?.Encode(options);\ndata?.SaveTo(imageStream);",
          "category": "workaround",
          "confidence": 0.6,
          "effort": "cost/s",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Use PNG instead of JPEG",
      "recommendedReason": "PNG encoding is lossless, platform-independent, and requires a minimal code change. Use the Downsample444 option as a secondary approach if JPEG output format is required."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.78,
      "reason": "Android-specific JPEG color difference with partial code repro. Related issue #2643 was closed as completed but reporter uses same 2.88.6 version. Needs reproduction to determine if this is expected JPEG YCbCr behavior or a platform-specific channel-order bug.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Labels already match classification; no changes needed",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Android",
          "backend/Raster",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask for additional platform details and provide PNG and Downsample444 workarounds",
        "risk": "medium",
        "confidence": 0.78,
        "comment": "Thanks for the report!\n\nJPEG encoding uses a YCbCr color space conversion internally along with 4:2:0 chroma subsampling by default — this can cause visible color shifts compared to PNG, especially for pure saturated colors like red. However, the Android-specific behavior you're seeing is worth investigating.\n\nA few questions to help narrow this down:\n1. What does the color difference look like exactly? (e.g., a slightly different hue of red, or is it a completely different color?)\n2. Does the issue occur on an Android emulator, or only on physical devices?\n3. Are you able to test with SkiaSharp 3.x?\n4. What is `SKImageInfo.PlatformColorType` on your device?\n\n**Workaround 1 — Use PNG (simplest):**\n```csharp\nbitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(imageStream);\n```\n\n**Workaround 2 — Disable chroma subsampling:**\n```csharp\nvar options = new SKJpegEncoderOptions(100, SKJpegEncoderDownsample.Downsample444, SKJpegEncoderAlphaOption.Ignore);\nusing var pixmap = bitmap.PeekPixels();\nusing var data = pixmap?.Encode(options);\ndata?.SaveTo(imageStream);\n```\n\nThis may be related to [#2643](https://github.com/mono/SkiaSharp/issues/2643) which reported JPEG encoding color changes on physical Android/iOS devices with SkiaSharp 2.88.6."
      },
      {
        "type": "link-related",
        "description": "Link to related open JPEG color issue #931",
        "risk": "low",
        "confidence": 0.8,
        "linkedIssue": 931
      },
      {
        "type": "link-related",
        "description": "Link to closely related closed issue #2643 (JPEG encoding color change on physical devices)",
        "risk": "low",
        "confidence": 0.85,
        "linkedIssue": 2643
      }
    ]
  }
}
```

</details>
