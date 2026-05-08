# Issue Triage Report — #1906

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T18:32:00Z |
| Type | type/question (0.92 (92%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.85 (85%)) |

**Issue Summary:** User asks how to save a PNG as 8-bit (palette/indexed color) using SkiaSharp, mistakenly believing Argb4444 is 8-bit.

**Analysis:** The reporter wants to save a 32-bit PNG as an 8-bit indexed/palette PNG for file size reduction. They tried SKColorType.Argb4444, which is 16-bit (4 bits per channel × 4 channels), not an 8-bit palette type. Skia/SkiaSharp does not support writing indexed/palette PNGs (PNG type 3); only Gray8 (8-bit grayscale) can produce a true 8-bit per-pixel output. Color quantization to produce indexed PNGs requires an external library.

**Recommendations:** **close-as-not-a-bug** — Usage question with a documented answer. Skia does not support indexed/palette PNG encoding; this is by design. Workarounds exist.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Code snippets:**

```csharp
var skImageInfo = new SKImageInfo((int)width, (int)height);
skImageInfo.ColorType = SKColorType.Argb4444;
using var resizedBitmap = image.Resize(skImageInfo, SKFilterQuality.Medium);
```

## Analysis

### Technical Summary

The reporter wants to save a 32-bit PNG as an 8-bit indexed/palette PNG for file size reduction. They tried SKColorType.Argb4444, which is 16-bit (4 bits per channel × 4 channels), not an 8-bit palette type. Skia/SkiaSharp does not support writing indexed/palette PNGs (PNG type 3); only Gray8 (8-bit grayscale) can produce a true 8-bit per-pixel output. Color quantization to produce indexed PNGs requires an external library.

### Rationale

The title and body frame this as a usage question about API capabilities. No crash or broken behavior is reported. The confusion between bit depth and color type is a common misunderstanding. Code investigation confirms SkiaSharp's PNG encoder has no palette/indexed color support.

### Key Signals

- "How to save PNG 8Bit without degradation of image quality" — **issue body** (Reporter wants indexed/palette PNG — common for icon/logo assets.)
- "I tried to set ColorType = SKColorType.Argb4444 But it has 16bit" — **issue body** (Reporter confused 8-bit indexed color with Argb4444 (16-bit).)
- "I could change the depth in Adobe Photoshop and Magic.Net but I want to use SkiaSharp" — **issue body** (Confirms feature-level ask — external tools can do it.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/EnumMappings.cs` | 43-64 | direct | SKColorType enum includes Gray8 (8 bpp grayscale) and Argb4444 (16-bit, 4 bits per channel). No indexed/palette color type exists in SkiaSharp. |
| `binding/SkiaSharp/Definitions.cs` | 525-541 | direct | SKPngEncoderOptions only exposes FilterFlags and ZLibLevel — no bit depth, no palette, no quantization parameters. |
| `binding/SkiaSharp/SKPixmap.cs` | 221-241 | related | SKPixmap.Encode for PNG always uses SKPngEncoderOptions.Default — compression-only, no color reduction path. |

### Workarounds

- To produce an 8-bit grayscale PNG: use SKColorType.Gray8 with SKAlphaType.Opaque and encode as PNG.
- To produce an 8-bit indexed/palette PNG: use an external quantization library (e.g., Magick.NET, pngquant, or nQuant) after encoding from SkiaSharp to a 32-bit PNG byte array.

### Resolution Proposals

**Hypothesis:** SkiaSharp's underlying Skia library does not support writing palette/indexed PNGs. The closest native option is Gray8 for grayscale images. For indexed color, color quantization must be performed externally.

1. **Use SKColorType.Gray8 for 8-bit grayscale PNGs** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - If the image is grayscale, resize into a Gray8 pixmap and encode to PNG — this produces a true 8-bit per-pixel PNG.

```csharp
var info = new SKImageInfo(width, height, SKColorType.Gray8, SKAlphaType.Opaque);
using var bmp = image.Resize(info, SKFilterQuality.Medium);
var pngData = bmp.Encode(SKEncodedImageFormat.Png, 100);
```
2. **Use Magick.NET for palette quantization after SkiaSharp rendering** — alternative, confidence 0.85 (85%), cost/s, validated=untested
   - Render to 32-bit PNG bytes with SkiaSharp, then pass the bytes to Magick.NET's Quantize() for 8-bit palette reduction.

**Recommended proposal:** Use SKColorType.Gray8 for 8-bit grayscale PNGs

**Why:** Only requires SkiaSharp if the image is grayscale. For color images, external quantization is unavoidable since Skia has no palette PNG encoder.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.85 (85%) |
| Reason | Usage question with a documented answer. Skia does not support indexed/palette PNG encoding; this is by design. Workarounds exist. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply question and SkiaSharp area labels | labels=type/question, area/SkiaSharp |
| add-comment | high | 0.85 (85%) | Explain that Skia has no palette PNG encoder and suggest Gray8 or external quantization | — |
| close-issue | medium | 0.80 (80%) | Close as answered — not a bug, usage question with workarounds provided | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the question!

`SKColorType.Argb4444` is a 16-bit format (4 bits × 4 channels), not an 8-bit indexed/palette type, so that won't produce the file size you're looking for.

Unfortunately, the underlying Skia library does not support writing indexed/palette PNGs (PNG type 3 — the format used for 256-color images). `SKPngEncoderOptions` only controls compression level and filter flags.

**Options depending on your use case:**

1. **Grayscale images** — Use `SKColorType.Gray8` to get a true 8-bit-per-pixel (grayscale) PNG:
```csharp
var info = new SKImageInfo(width, height, SKColorType.Gray8, SKAlphaType.Opaque);
using var bmp = image.Resize(info, SKFilterQuality.Medium);
var pngData = bmp.Encode(SKEncodedImageFormat.Png, 100);
```

2. **Color images with 256-color palette** — Render to a 32-bit PNG with SkiaSharp, then pass the bytes to an external quantization library such as [Magick.NET](https://github.com/dlemstra/Magick.NET), [pngquant](https://pngquant.org/), or [nQuant](https://github.com/mcychan/nQuant.cs) to reduce to a palette.

There is no direct 8-bit indexed PNG path within SkiaSharp today.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1906,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T18:32:00Z"
  },
  "summary": "User asks how to save a PNG as 8-bit (palette/indexed color) using SkiaSharp, mistakenly believing Argb4444 is 8-bit.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    }
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "var skImageInfo = new SKImageInfo((int)width, (int)height);\nskImageInfo.ColorType = SKColorType.Argb4444;\nusing var resizedBitmap = image.Resize(skImageInfo, SKFilterQuality.Medium);"
      ]
    }
  },
  "analysis": {
    "summary": "The reporter wants to save a 32-bit PNG as an 8-bit indexed/palette PNG for file size reduction. They tried SKColorType.Argb4444, which is 16-bit (4 bits per channel × 4 channels), not an 8-bit palette type. Skia/SkiaSharp does not support writing indexed/palette PNGs (PNG type 3); only Gray8 (8-bit grayscale) can produce a true 8-bit per-pixel output. Color quantization to produce indexed PNGs requires an external library.",
    "rationale": "The title and body frame this as a usage question about API capabilities. No crash or broken behavior is reported. The confusion between bit depth and color type is a common misunderstanding. Code investigation confirms SkiaSharp's PNG encoder has no palette/indexed color support.",
    "keySignals": [
      {
        "text": "How to save PNG 8Bit without degradation of image quality",
        "source": "issue body",
        "interpretation": "Reporter wants indexed/palette PNG — common for icon/logo assets."
      },
      {
        "text": "I tried to set ColorType = SKColorType.Argb4444 But it has 16bit",
        "source": "issue body",
        "interpretation": "Reporter confused 8-bit indexed color with Argb4444 (16-bit)."
      },
      {
        "text": "I could change the depth in Adobe Photoshop and Magic.Net but I want to use SkiaSharp",
        "source": "issue body",
        "interpretation": "Confirms feature-level ask — external tools can do it."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/EnumMappings.cs",
        "lines": "43-64",
        "finding": "SKColorType enum includes Gray8 (8 bpp grayscale) and Argb4444 (16-bit, 4 bits per channel). No indexed/palette color type exists in SkiaSharp.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "525-541",
        "finding": "SKPngEncoderOptions only exposes FilterFlags and ZLibLevel — no bit depth, no palette, no quantization parameters.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPixmap.cs",
        "lines": "221-241",
        "finding": "SKPixmap.Encode for PNG always uses SKPngEncoderOptions.Default — compression-only, no color reduction path.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "To produce an 8-bit grayscale PNG: use SKColorType.Gray8 with SKAlphaType.Opaque and encode as PNG.",
      "To produce an 8-bit indexed/palette PNG: use an external quantization library (e.g., Magick.NET, pngquant, or nQuant) after encoding from SkiaSharp to a 32-bit PNG byte array."
    ],
    "resolution": {
      "hypothesis": "SkiaSharp's underlying Skia library does not support writing palette/indexed PNGs. The closest native option is Gray8 for grayscale images. For indexed color, color quantization must be performed externally.",
      "proposals": [
        {
          "title": "Use SKColorType.Gray8 for 8-bit grayscale PNGs",
          "description": "If the image is grayscale, resize into a Gray8 pixmap and encode to PNG — this produces a true 8-bit per-pixel PNG.",
          "category": "workaround",
          "codeSnippet": "var info = new SKImageInfo(width, height, SKColorType.Gray8, SKAlphaType.Opaque);\nusing var bmp = image.Resize(info, SKFilterQuality.Medium);\nvar pngData = bmp.Encode(SKEncodedImageFormat.Png, 100);",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Use Magick.NET for palette quantization after SkiaSharp rendering",
          "description": "Render to 32-bit PNG bytes with SkiaSharp, then pass the bytes to Magick.NET's Quantize() for 8-bit palette reduction.",
          "category": "alternative",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use SKColorType.Gray8 for 8-bit grayscale PNGs",
      "recommendedReason": "Only requires SkiaSharp if the image is grayscale. For color images, external quantization is unavoidable since Skia has no palette PNG encoder."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.85,
      "reason": "Usage question with a documented answer. Skia does not support indexed/palette PNG encoding; this is by design. Workarounds exist.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and SkiaSharp area labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain that Skia has no palette PNG encoder and suggest Gray8 or external quantization",
        "risk": "high",
        "confidence": 0.85,
        "comment": "Thanks for the question!\n\n`SKColorType.Argb4444` is a 16-bit format (4 bits × 4 channels), not an 8-bit indexed/palette type, so that won't produce the file size you're looking for.\n\nUnfortunately, the underlying Skia library does not support writing indexed/palette PNGs (PNG type 3 — the format used for 256-color images). `SKPngEncoderOptions` only controls compression level and filter flags.\n\n**Options depending on your use case:**\n\n1. **Grayscale images** — Use `SKColorType.Gray8` to get a true 8-bit-per-pixel (grayscale) PNG:\n```csharp\nvar info = new SKImageInfo(width, height, SKColorType.Gray8, SKAlphaType.Opaque);\nusing var bmp = image.Resize(info, SKFilterQuality.Medium);\nvar pngData = bmp.Encode(SKEncodedImageFormat.Png, 100);\n```\n\n2. **Color images with 256-color palette** — Render to a 32-bit PNG with SkiaSharp, then pass the bytes to an external quantization library such as [Magick.NET](https://github.com/dlemstra/Magick.NET), [pngquant](https://pngquant.org/), or [nQuant](https://github.com/mcychan/nQuant.cs) to reduce to a palette.\n\nThere is no direct 8-bit indexed PNG path within SkiaSharp today."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — not a bug, usage question with workarounds provided",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
