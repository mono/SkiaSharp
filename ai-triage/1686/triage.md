# Issue Triage Report — #1686

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T18:11:44Z |
| Type | type/question (0.95 (95%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** User asks how to parse a 24-bit RGB (PPM/Netpbm) image with SkiaSharp, since there is no built-in PPM codec and the reporter did not know about SKColorType.Rgb888x.

**Analysis:** SkiaSharp does not include a built-in PPM/Netpbm codec, so SKBitmap.Decode() will not read PPM files. The answer is to manually parse the PPM format and load the pixel data into an SKBitmap using SKColorType.Rgb888x (32-bit storage with an ignored alpha byte) or by converting pixels to RGBA and calling InstallPixels. A community commenter already posted a complete working implementation.

**Recommendations:** **close-as-not-a-bug** — This is a usage question with a clear answer — SkiaSharp does not support PPM natively, and the workaround (manual parsing + SKColorType.Rgb888x) is well-established and already posted in the comments.

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

1. Have a raw PPM image byte array (24-bit RGB, no alpha channel)
2. Try to decode it with SKBitmap — fails because SkiaSharp has no PPM codec
3. Ask whether there is a 24-bit RGB color type or API for manual pixel loading

**Environment:** No specific platform or version mentioned

**Repository links:**
- https://en.wikipedia.org/wiki/Netpbm — Wikipedia PPM format reference linked by reporter
- https://github.com/mono/SkiaSharp/issues/1686#issuecomment-2031515228 — Community workaround: full PPM parser using SKColorType.Rgb888x and SetPixel

## Analysis

### Technical Summary

SkiaSharp does not include a built-in PPM/Netpbm codec, so SKBitmap.Decode() will not read PPM files. The answer is to manually parse the PPM format and load the pixel data into an SKBitmap using SKColorType.Rgb888x (32-bit storage with an ignored alpha byte) or by converting pixels to RGBA and calling InstallPixels. A community commenter already posted a complete working implementation.

### Rationale

This is a how-to question. No bug exists — PPM is a niche format not supported by SkiaSharp's codec layer (upstream Skia). The answer is to manually parse the PPM header/pixels and fill an SKBitmap using SKColorType.Rgb888x. The community commenter provided a correct, working implementation. The issue should be closed as answered.

### Key Signals

- "There is no alpha channel so reading this as a 32-bit ARGB or BGRA will not read properly." — **issue body** (Reporter is unaware of SKColorType.Rgb888x, which stores 24-bit RGB in a 32-bit pixel with ignored alpha.)
- "I have not been successful in reading this image with a SKBitmap." — **issue body** (SkiaSharp has no native PPM codec, so auto-decode will always fail. Manual pixel loading is required.)
- "var bitmap = new SKBitmap(width, height, SKColorType.Rgb888x, SKAlphaType.Opaque); ... bitmap.SetPixel(x++, y, color);" — **comment by simonegli8** (Community commenter already published a complete PPM parser using the correct SKColorType.Rgb888x approach.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/Definitions.cs` | 36-60 | direct | SKColorType enum includes Rgb888x (value 5), which stores R, G, B in 24 meaningful bits inside a 32-bit (4-byte) pixel with the high byte ignored. This is the closest SkiaSharp type to 24-bit RGB. |
| `binding/SkiaSharp/Definitions.cs` | 104 | direct | GetBytesPerPixel for SKColorType.Rgb888x returns 4 — it stores RGB data in 4 bytes, not 3, so raw PPM data must be expanded per pixel. |
| `binding/SkiaSharp/SKBitmap.cs` | 131-135 | direct | SKBitmap.SetPixel(x, y, SKColor) is a public API that accepts an SKColor (which holds R/G/B without requiring alpha). It can be used to populate pixels from a manually parsed PPM stream. |
| `binding/SkiaSharp/SKBitmap.cs` | 591-620 | related | SKBitmap.InstallPixels(SKImageInfo, IntPtr, int) allows bulk-loading a pre-formatted pixel buffer. For higher performance, the caller can expand 3-byte PPM pixels to 4-byte Rgb888x format and pass the buffer via InstallPixels. |

### Workarounds

- Parse PPM pixels manually and call SKBitmap.SetPixel(x, y, new SKColor(r, g, b)) using SKColorType.Rgb888x for the bitmap (as shown in the community comment by simonegli8).
- For high-performance bulk loading: expand the PPM pixel stream to a 4-byte-per-pixel RGBX buffer and pass it to SKBitmap.InstallPixels(info, pixelPtr, rowBytes).

### Resolution Proposals

**Hypothesis:** Reporter needs to manually parse the PPM format and populate an SKBitmap because SkiaSharp does not include a PPM codec.

1. **Manual PPM parser with SetPixel** — workaround, confidence 0.92 (92%), cost/s, validated=yes
   - Create an SKBitmap with SKColorType.Rgb888x and SKAlphaType.Opaque, then loop over PPM pixel data reading each R/G/B triplet and calling SetPixel. The community comment provides a complete ready-to-use implementation.

```csharp
// P6 binary PPM example
using var stream = File.OpenRead("image.ppm");
// ... parse PPM header to get width, height
var bitmap = new SKBitmap(width, height, SKColorType.Rgb888x, SKAlphaType.Opaque);
for (int y = 0; y < height; y++) {
    for (int x = 0; x < width; x++) {
        byte r = (byte)stream.ReadByte();
        byte g = (byte)stream.ReadByte();
        byte b = (byte)stream.ReadByte();
        bitmap.SetPixel(x, y, new SKColor(r, g, b));
    }
}
```
2. **Bulk pixel load with InstallPixels** — alternative, confidence 0.82 (82%), cost/m, validated=untested
   - For better performance, read all PPM pixel data, expand each 3-byte RGB triplet to 4-byte RGBX (set alpha=0xFF or just 0), allocate the buffer, and call SKBitmap.InstallPixels to avoid per-pixel overhead.

**Recommended proposal:** Manual PPM parser with SetPixel

**Why:** Directly solves the problem with simple code. The community commenter already validated this approach with a full implementation.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | This is a usage question with a clear answer — SkiaSharp does not support PPM natively, and the workaround (manual parsing + SKColorType.Rgb888x) is well-established and already posted in the comments. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply question and core library labels | labels=type/question, area/SkiaSharp |
| add-comment | medium | 0.90 (90%) | Post answer explaining SKColorType.Rgb888x and manual parsing approach | — |
| close-issue | medium | 0.85 (85%) | Close as answered | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
SkiaSharp does not include a built-in PPM/Netpbm codec, so `SKBitmap.Decode()` will not work for this format. The solution is to manually parse the PPM header and pixel data, then populate an `SKBitmap` using `SKColorType.Rgb888x` and `SKAlphaType.Opaque`.

`SKColorType.Rgb888x` stores R, G, B in 24 meaningful bits inside a 32-bit pixel (the fourth byte is ignored), which is the closest match to a 24-bit RGB format in SkiaSharp.

```csharp
// P6 binary PPM example
using var stream = File.OpenRead("image.ppm");
// ... parse PPM header for width, height, maxval
var bitmap = new SKBitmap(width, height, SKColorType.Rgb888x, SKAlphaType.Opaque);
for (int y = 0; y < height; y++) {
    for (int x = 0; x < width; x++) {
        byte r = (byte)stream.ReadByte();
        byte g = (byte)stream.ReadByte();
        byte b = (byte)stream.ReadByte();
        bitmap.SetPixel(x, y, new SKColor(r, g, b));
    }
}
```

Note that `SetPixel` is convenient but can be slow for large images. For better performance, expand each 3-byte RGB pixel to 4-byte RGBX format in a byte array, then call `SKBitmap.InstallPixels(info, pixelPtr, rowBytes)` to bulk-load the data.

A complete ready-to-use implementation (including P3 ASCII and P6 binary PPM support, plus `maxval` normalization) was posted in the comments above by @simonegli8.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1686,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T18:11:44Z"
  },
  "summary": "User asks how to parse a 24-bit RGB (PPM/Netpbm) image with SkiaSharp, since there is no built-in PPM codec and the reporter did not know about SKColorType.Rgb888x.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    }
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Have a raw PPM image byte array (24-bit RGB, no alpha channel)",
        "Try to decode it with SKBitmap — fails because SkiaSharp has no PPM codec",
        "Ask whether there is a 24-bit RGB color type or API for manual pixel loading"
      ],
      "environmentDetails": "No specific platform or version mentioned",
      "repoLinks": [
        {
          "url": "https://en.wikipedia.org/wiki/Netpbm",
          "description": "Wikipedia PPM format reference linked by reporter"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1686#issuecomment-2031515228",
          "description": "Community workaround: full PPM parser using SKColorType.Rgb888x and SetPixel"
        }
      ]
    }
  },
  "analysis": {
    "summary": "SkiaSharp does not include a built-in PPM/Netpbm codec, so SKBitmap.Decode() will not read PPM files. The answer is to manually parse the PPM format and load the pixel data into an SKBitmap using SKColorType.Rgb888x (32-bit storage with an ignored alpha byte) or by converting pixels to RGBA and calling InstallPixels. A community commenter already posted a complete working implementation.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "36-60",
        "finding": "SKColorType enum includes Rgb888x (value 5), which stores R, G, B in 24 meaningful bits inside a 32-bit (4-byte) pixel with the high byte ignored. This is the closest SkiaSharp type to 24-bit RGB.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "104",
        "finding": "GetBytesPerPixel for SKColorType.Rgb888x returns 4 — it stores RGB data in 4 bytes, not 3, so raw PPM data must be expanded per pixel.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "131-135",
        "finding": "SKBitmap.SetPixel(x, y, SKColor) is a public API that accepts an SKColor (which holds R/G/B without requiring alpha). It can be used to populate pixels from a manually parsed PPM stream.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "591-620",
        "finding": "SKBitmap.InstallPixels(SKImageInfo, IntPtr, int) allows bulk-loading a pre-formatted pixel buffer. For higher performance, the caller can expand 3-byte PPM pixels to 4-byte Rgb888x format and pass the buffer via InstallPixels.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "There is no alpha channel so reading this as a 32-bit ARGB or BGRA will not read properly.",
        "source": "issue body",
        "interpretation": "Reporter is unaware of SKColorType.Rgb888x, which stores 24-bit RGB in a 32-bit pixel with ignored alpha."
      },
      {
        "text": "I have not been successful in reading this image with a SKBitmap.",
        "source": "issue body",
        "interpretation": "SkiaSharp has no native PPM codec, so auto-decode will always fail. Manual pixel loading is required."
      },
      {
        "text": "var bitmap = new SKBitmap(width, height, SKColorType.Rgb888x, SKAlphaType.Opaque); ... bitmap.SetPixel(x++, y, color);",
        "source": "comment by simonegli8",
        "interpretation": "Community commenter already published a complete PPM parser using the correct SKColorType.Rgb888x approach."
      }
    ],
    "rationale": "This is a how-to question. No bug exists — PPM is a niche format not supported by SkiaSharp's codec layer (upstream Skia). The answer is to manually parse the PPM header/pixels and fill an SKBitmap using SKColorType.Rgb888x. The community commenter provided a correct, working implementation. The issue should be closed as answered.",
    "workarounds": [
      "Parse PPM pixels manually and call SKBitmap.SetPixel(x, y, new SKColor(r, g, b)) using SKColorType.Rgb888x for the bitmap (as shown in the community comment by simonegli8).",
      "For high-performance bulk loading: expand the PPM pixel stream to a 4-byte-per-pixel RGBX buffer and pass it to SKBitmap.InstallPixels(info, pixelPtr, rowBytes)."
    ],
    "resolution": {
      "hypothesis": "Reporter needs to manually parse the PPM format and populate an SKBitmap because SkiaSharp does not include a PPM codec.",
      "proposals": [
        {
          "title": "Manual PPM parser with SetPixel",
          "description": "Create an SKBitmap with SKColorType.Rgb888x and SKAlphaType.Opaque, then loop over PPM pixel data reading each R/G/B triplet and calling SetPixel. The community comment provides a complete ready-to-use implementation.",
          "category": "workaround",
          "codeSnippet": "// P6 binary PPM example\nusing var stream = File.OpenRead(\"image.ppm\");\n// ... parse PPM header to get width, height\nvar bitmap = new SKBitmap(width, height, SKColorType.Rgb888x, SKAlphaType.Opaque);\nfor (int y = 0; y < height; y++) {\n    for (int x = 0; x < width; x++) {\n        byte r = (byte)stream.ReadByte();\n        byte g = (byte)stream.ReadByte();\n        byte b = (byte)stream.ReadByte();\n        bitmap.SetPixel(x, y, new SKColor(r, g, b));\n    }\n}",
          "confidence": 0.92,
          "effort": "cost/s",
          "validated": "yes"
        },
        {
          "title": "Bulk pixel load with InstallPixels",
          "description": "For better performance, read all PPM pixel data, expand each 3-byte RGB triplet to 4-byte RGBX (set alpha=0xFF or just 0), allocate the buffer, and call SKBitmap.InstallPixels to avoid per-pixel overhead.",
          "category": "alternative",
          "confidence": 0.82,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Manual PPM parser with SetPixel",
      "recommendedReason": "Directly solves the problem with simple code. The community commenter already validated this approach with a full implementation."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "This is a usage question with a clear answer — SkiaSharp does not support PPM natively, and the workaround (manual parsing + SKColorType.Rgb888x) is well-established and already posted in the comments.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and core library labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post answer explaining SKColorType.Rgb888x and manual parsing approach",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "SkiaSharp does not include a built-in PPM/Netpbm codec, so `SKBitmap.Decode()` will not work for this format. The solution is to manually parse the PPM header and pixel data, then populate an `SKBitmap` using `SKColorType.Rgb888x` and `SKAlphaType.Opaque`.\n\n`SKColorType.Rgb888x` stores R, G, B in 24 meaningful bits inside a 32-bit pixel (the fourth byte is ignored), which is the closest match to a 24-bit RGB format in SkiaSharp.\n\n```csharp\n// P6 binary PPM example\nusing var stream = File.OpenRead(\"image.ppm\");\n// ... parse PPM header for width, height, maxval\nvar bitmap = new SKBitmap(width, height, SKColorType.Rgb888x, SKAlphaType.Opaque);\nfor (int y = 0; y < height; y++) {\n    for (int x = 0; x < width; x++) {\n        byte r = (byte)stream.ReadByte();\n        byte g = (byte)stream.ReadByte();\n        byte b = (byte)stream.ReadByte();\n        bitmap.SetPixel(x, y, new SKColor(r, g, b));\n    }\n}\n```\n\nNote that `SetPixel` is convenient but can be slow for large images. For better performance, expand each 3-byte RGB pixel to 4-byte RGBX format in a byte array, then call `SKBitmap.InstallPixels(info, pixelPtr, rowBytes)` to bulk-load the data.\n\nA complete ready-to-use implementation (including P3 ASCII and P6 binary PPM support, plus `maxval` normalization) was posted in the comments above by @simonegli8."
      },
      {
        "type": "close-issue",
        "description": "Close as answered",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
