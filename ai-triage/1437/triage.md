# Issue Triage Report — #1437

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T20:00:00Z |
| Type | type/question (0.88 (88%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-external (0.85 (85%)) |

**Issue Summary:** Reporter is loading JPEG bytes extracted from BLP (Blizzard Picture Format) files and expecting SkiaSharp to decode an alpha channel, but standard JPEG does not support alpha; the BLP container stores alpha separately and the caller must apply it manually.

**Analysis:** Standard JPEG does not support alpha channels. In the BLP2 file format, color data is stored as JPEG but the alpha channel is stored separately in the BLP container as raw 8-bit values. When the reporter passes only the JPEG bytes to SkiaSharp, Skia's JPEG codec correctly reports the image as opaque (no alpha) because the JPEG data itself contains no alpha information. The alpha must be extracted from the BLP container separately and then manually applied to the decoded bitmap.

**Recommendations:** **close-as-external** — The JPEG format does not support alpha channels; behavior is correct per JPEG spec. The BLP2 container stores alpha separately from JPEG scan data. SkiaSharp cannot decode alpha that is not present in the JPEG byte stream. The caller must extract both planes from the BLP container and combine them manually.

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
| Current labels | type/question |

## Evidence

### Reproduction

**Environment:** No version specified; uses SkiaSharp to decode byte arrays extracted from BLP files

**Repository links:**
- https://github.com/Drake53/War3Net/tree/master/tests/War3Net.Common.Testing/TestData/Blp — Reporter's test files including the BLP, raw JPEG, and correct PNG reference
- https://www.hiveworkshop.com/threads/the-evil-that-is-blp.269997/ — BLP format overview on Hive Workshop
- https://www.hiveworkshop.com/threads/blp-specifications-wc3.279306/ — BLP2 specification: alpha is stored separately from JPEG color data

## Analysis

### Technical Summary

Standard JPEG does not support alpha channels. In the BLP2 file format, color data is stored as JPEG but the alpha channel is stored separately in the BLP container as raw 8-bit values. When the reporter passes only the JPEG bytes to SkiaSharp, Skia's JPEG codec correctly reports the image as opaque (no alpha) because the JPEG data itself contains no alpha information. The alpha must be extracted from the BLP container separately and then manually applied to the decoded bitmap.

### Rationale

The behavior is correct per the JPEG specification. JPEG (JFIF/Exif) has no alpha channel support. Skia's JPEG codec sets AlphaType.Opaque for JPEG streams, which is confirmed by the SkiaSharp source code (SKBitmap.Decode reads codec.Info, which is opaque for JPEG). The alpha in BLP2 files is stored in the container structure alongside but separate from the JPEG scan data. This is not a SkiaSharp or Skia deficiency — the caller must parse the BLP format to extract and combine both planes. The maintainer's comment ('This is a discussion for the native skia team') reflects uncertainty about JPEG alpha extensions, but for BLP specifically the alpha is structurally outside the JPEG stream. This is a question with a clear answer: manually apply the BLP alpha channel after decoding.

### Key Signals

- "this jpg data can have an alpha channel, but when I load the byte array using SkiaSharp, this information is lost" — **issue body** (Reporter is passing only the JPEG bytes from the BLP container, not the separate BLP alpha plane — the alpha is never in the JPEG stream.)
- "Instead of fully transparent white pixels, I'm getting opaque black pixels" — **issue body** (Confirms JPEG is decoded opaque (black at alpha=0 areas when premultiplied to nothing). Expected output is RGBA transparency.)
- "I tried using different SKColorSpaces and setting SKImageInfo.AlphaType, but nothing seems to work" — **issue body** (Changing AlphaType in the target SKImageInfo does not inject alpha data; the JPEG stream has no alpha to extract regardless of the decode target format.)
- "Not gonna lie, that transparent jpeg threw me a bit. I didn't know that was even a thing. This is a discussion for the native skia team." — **comment by maintainer mattleibow** (Maintainer is unsure about JPEG alpha support; for BLP2 specifically the alpha is stored outside the JPEG scan data in the BLP container header/alpha section.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 434-443 | direct | SKBitmap.Decode(SKCodec) reads codec.Info for the target image info. For JPEG, Skia's codec reports AlphaType.Opaque — there is no alpha channel in the JPEG stream. Setting a different AlphaType in the destination SKImageInfo only controls the pixel memory layout, not whether alpha data is decoded from the source. |
| `binding/SkiaSharp/SKBitmap.cs` | 295-314 | related | GetPixelSpan() and SetPixels() provide direct access to raw pixel memory. A caller who has the BLP alpha plane can decode the JPEG, create a new RGBA8888 bitmap, copy pixels from the decoded JPEG, and then write the separate alpha bytes into the alpha channel of each pixel using GetPixelSpan(). |

### Workarounds

- Extract the alpha channel bytes from the BLP container structure separately from the JPEG data. Decode the JPEG using SKBitmap.Decode(). Create a new SKBitmap with SKColorType.Rgba8888 and SKAlphaType.Premul at the same dimensions. Copy each pixel from the decoded JPEG bitmap and set the alpha byte from the BLP alpha plane, then premultiply if needed.

### Resolution Proposals

**Hypothesis:** The BLP2 format stores alpha as raw 8-bit values in a separate section of the container, completely outside the JPEG scan data. The fix is to parse and apply the BLP alpha after JPEG decoding using SkiaSharp's pixel access APIs.

1. **Manually apply BLP alpha after JPEG decode** — workaround, confidence 0.80 (80%), cost/s, validated=untested
   - Decode the JPEG bytes with SKBitmap.Decode(), then create a new Rgba8888 bitmap. For each pixel, copy the RGB from the decoded JPEG and write the alpha byte from the separately-extracted BLP alpha plane.

```csharp
// jpegBytes: byte[] — JPEG data from BLP container
// blpAlpha: byte[] — alpha plane extracted from BLP container (one byte per pixel, row-major)
using var jpegBitmap = SKBitmap.Decode(jpegBytes);
var info = new SKImageInfo(jpegBitmap.Width, jpegBitmap.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
using var result = new SKBitmap(info);
var srcSpan = jpegBitmap.GetPixelSpan();
var dstSpan = result.GetPixelSpan();
for (int i = 0, a = 0; i < dstSpan.Length; i += 4, a++) {
    byte alpha = blpAlpha[a];
    // Source pixels from JPEG decoded as BGRA or RGBA — adjust channel order as needed
    dstSpan[i + 0] = (byte)(srcSpan[i + 0] * alpha / 255); // R premultiplied
    dstSpan[i + 1] = (byte)(srcSpan[i + 1] * alpha / 255); // G premultiplied
    dstSpan[i + 2] = (byte)(srcSpan[i + 2] * alpha / 255); // B premultiplied
    dstSpan[i + 3] = alpha;
}
```

**Recommended proposal:** Manually apply BLP alpha after JPEG decode

**Why:** The BLP alpha plane is structurally outside the JPEG stream; no SkiaSharp API can decode what isn't in the JPEG data. Manual composition is the only correct path.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.85 (85%) |
| Reason | The JPEG format does not support alpha channels; behavior is correct per JPEG spec. The BLP2 container stores alpha separately from JPEG scan data. SkiaSharp cannot decode alpha that is not present in the JPEG byte stream. The caller must extract both planes from the BLP container and combine them manually. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/question and area/SkiaSharp labels | labels=type/question, area/SkiaSharp |
| add-comment | high | 0.85 (85%) | Explain that standard JPEG has no alpha, BLP stores alpha separately, and provide workaround | — |
| close-issue | medium | 0.80 (80%) | Close as answered — behavior is correct per JPEG spec, workaround documented | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed test files — those were helpful for understanding what's happening.

The core issue is that **standard JPEG does not support alpha channels**. The alpha in BLP2 files is stored in a completely separate section of the BLP container, not inside the JPEG scan data. When you pass just the JPEG bytes to SkiaSharp, Skia's JPEG codec reports the image as fully opaque — correctly, because the JPEG stream contains no alpha information regardless of the alpha type you request on decode.

The fix needs to happen in your BLP parser: extract the alpha plane from the BLP container separately, decode the JPEG for RGB data, then combine them:

```csharp
// jpegBytes: JPEG data from BLP container
// blpAlpha: alpha plane from BLP container (one byte per pixel, row-major)
using var jpegBitmap = SKBitmap.Decode(jpegBytes);
var info = new SKImageInfo(jpegBitmap.Width, jpegBitmap.Height,
    SKColorType.Rgba8888, SKAlphaType.Premul);
using var result = new SKBitmap(info);
var srcSpan = jpegBitmap.GetPixelSpan();
var dstSpan = result.GetPixelSpan();
for (int i = 0, a = 0; i < dstSpan.Length; i += 4, a++) {
    byte alpha = blpAlpha[a];
    dstSpan[i + 0] = (byte)(srcSpan[i + 0] * alpha / 255);
    dstSpan[i + 1] = (byte)(srcSpan[i + 1] * alpha / 255);
    dstSpan[i + 2] = (byte)(srcSpan[i + 2] * alpha / 255);
    dstSpan[i + 3] = alpha;
}
```

Note: the channel order of `srcSpan` depends on the platform color type (BGRA vs RGBA) — check `SKImageInfo.PlatformColorType` on your platform and adjust accordingly.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1437,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T20:00:00Z",
    "currentLabels": [
      "type/question"
    ]
  },
  "summary": "Reporter is loading JPEG bytes extracted from BLP (Blizzard Picture Format) files and expecting SkiaSharp to decode an alpha channel, but standard JPEG does not support alpha; the BLP container stores alpha separately and the caller must apply it manually.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    }
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "No version specified; uses SkiaSharp to decode byte arrays extracted from BLP files",
      "repoLinks": [
        {
          "url": "https://github.com/Drake53/War3Net/tree/master/tests/War3Net.Common.Testing/TestData/Blp",
          "description": "Reporter's test files including the BLP, raw JPEG, and correct PNG reference"
        },
        {
          "url": "https://www.hiveworkshop.com/threads/the-evil-that-is-blp.269997/",
          "description": "BLP format overview on Hive Workshop"
        },
        {
          "url": "https://www.hiveworkshop.com/threads/blp-specifications-wc3.279306/",
          "description": "BLP2 specification: alpha is stored separately from JPEG color data"
        }
      ]
    }
  },
  "analysis": {
    "summary": "Standard JPEG does not support alpha channels. In the BLP2 file format, color data is stored as JPEG but the alpha channel is stored separately in the BLP container as raw 8-bit values. When the reporter passes only the JPEG bytes to SkiaSharp, Skia's JPEG codec correctly reports the image as opaque (no alpha) because the JPEG data itself contains no alpha information. The alpha must be extracted from the BLP container separately and then manually applied to the decoded bitmap.",
    "rationale": "The behavior is correct per the JPEG specification. JPEG (JFIF/Exif) has no alpha channel support. Skia's JPEG codec sets AlphaType.Opaque for JPEG streams, which is confirmed by the SkiaSharp source code (SKBitmap.Decode reads codec.Info, which is opaque for JPEG). The alpha in BLP2 files is stored in the container structure alongside but separate from the JPEG scan data. This is not a SkiaSharp or Skia deficiency — the caller must parse the BLP format to extract and combine both planes. The maintainer's comment ('This is a discussion for the native skia team') reflects uncertainty about JPEG alpha extensions, but for BLP specifically the alpha is structurally outside the JPEG stream. This is a question with a clear answer: manually apply the BLP alpha channel after decoding.",
    "keySignals": [
      {
        "text": "this jpg data can have an alpha channel, but when I load the byte array using SkiaSharp, this information is lost",
        "source": "issue body",
        "interpretation": "Reporter is passing only the JPEG bytes from the BLP container, not the separate BLP alpha plane — the alpha is never in the JPEG stream."
      },
      {
        "text": "Instead of fully transparent white pixels, I'm getting opaque black pixels",
        "source": "issue body",
        "interpretation": "Confirms JPEG is decoded opaque (black at alpha=0 areas when premultiplied to nothing). Expected output is RGBA transparency."
      },
      {
        "text": "I tried using different SKColorSpaces and setting SKImageInfo.AlphaType, but nothing seems to work",
        "source": "issue body",
        "interpretation": "Changing AlphaType in the target SKImageInfo does not inject alpha data; the JPEG stream has no alpha to extract regardless of the decode target format."
      },
      {
        "text": "Not gonna lie, that transparent jpeg threw me a bit. I didn't know that was even a thing. This is a discussion for the native skia team.",
        "source": "comment by maintainer mattleibow",
        "interpretation": "Maintainer is unsure about JPEG alpha support; for BLP2 specifically the alpha is stored outside the JPEG scan data in the BLP container header/alpha section."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "434-443",
        "finding": "SKBitmap.Decode(SKCodec) reads codec.Info for the target image info. For JPEG, Skia's codec reports AlphaType.Opaque — there is no alpha channel in the JPEG stream. Setting a different AlphaType in the destination SKImageInfo only controls the pixel memory layout, not whether alpha data is decoded from the source.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "295-314",
        "finding": "GetPixelSpan() and SetPixels() provide direct access to raw pixel memory. A caller who has the BLP alpha plane can decode the JPEG, create a new RGBA8888 bitmap, copy pixels from the decoded JPEG, and then write the separate alpha bytes into the alpha channel of each pixel using GetPixelSpan().",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Extract the alpha channel bytes from the BLP container structure separately from the JPEG data. Decode the JPEG using SKBitmap.Decode(). Create a new SKBitmap with SKColorType.Rgba8888 and SKAlphaType.Premul at the same dimensions. Copy each pixel from the decoded JPEG bitmap and set the alpha byte from the BLP alpha plane, then premultiply if needed."
    ],
    "resolution": {
      "hypothesis": "The BLP2 format stores alpha as raw 8-bit values in a separate section of the container, completely outside the JPEG scan data. The fix is to parse and apply the BLP alpha after JPEG decoding using SkiaSharp's pixel access APIs.",
      "proposals": [
        {
          "title": "Manually apply BLP alpha after JPEG decode",
          "description": "Decode the JPEG bytes with SKBitmap.Decode(), then create a new Rgba8888 bitmap. For each pixel, copy the RGB from the decoded JPEG and write the alpha byte from the separately-extracted BLP alpha plane.",
          "category": "workaround",
          "codeSnippet": "// jpegBytes: byte[] — JPEG data from BLP container\n// blpAlpha: byte[] — alpha plane extracted from BLP container (one byte per pixel, row-major)\nusing var jpegBitmap = SKBitmap.Decode(jpegBytes);\nvar info = new SKImageInfo(jpegBitmap.Width, jpegBitmap.Height, SKColorType.Rgba8888, SKAlphaType.Premul);\nusing var result = new SKBitmap(info);\nvar srcSpan = jpegBitmap.GetPixelSpan();\nvar dstSpan = result.GetPixelSpan();\nfor (int i = 0, a = 0; i < dstSpan.Length; i += 4, a++) {\n    byte alpha = blpAlpha[a];\n    // Source pixels from JPEG decoded as BGRA or RGBA — adjust channel order as needed\n    dstSpan[i + 0] = (byte)(srcSpan[i + 0] * alpha / 255); // R premultiplied\n    dstSpan[i + 1] = (byte)(srcSpan[i + 1] * alpha / 255); // G premultiplied\n    dstSpan[i + 2] = (byte)(srcSpan[i + 2] * alpha / 255); // B premultiplied\n    dstSpan[i + 3] = alpha;\n}",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Manually apply BLP alpha after JPEG decode",
      "recommendedReason": "The BLP alpha plane is structurally outside the JPEG stream; no SkiaSharp API can decode what isn't in the JPEG data. Manual composition is the only correct path."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.85,
      "reason": "The JPEG format does not support alpha channels; behavior is correct per JPEG spec. The BLP2 container stores alpha separately from JPEG scan data. SkiaSharp cannot decode alpha that is not present in the JPEG byte stream. The caller must extract both planes from the BLP container and combine them manually.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/question and area/SkiaSharp labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain that standard JPEG has no alpha, BLP stores alpha separately, and provide workaround",
        "risk": "high",
        "confidence": 0.85,
        "comment": "Thanks for the detailed test files — those were helpful for understanding what's happening.\n\nThe core issue is that **standard JPEG does not support alpha channels**. The alpha in BLP2 files is stored in a completely separate section of the BLP container, not inside the JPEG scan data. When you pass just the JPEG bytes to SkiaSharp, Skia's JPEG codec reports the image as fully opaque — correctly, because the JPEG stream contains no alpha information regardless of the alpha type you request on decode.\n\nThe fix needs to happen in your BLP parser: extract the alpha plane from the BLP container separately, decode the JPEG for RGB data, then combine them:\n\n```csharp\n// jpegBytes: JPEG data from BLP container\n// blpAlpha: alpha plane from BLP container (one byte per pixel, row-major)\nusing var jpegBitmap = SKBitmap.Decode(jpegBytes);\nvar info = new SKImageInfo(jpegBitmap.Width, jpegBitmap.Height,\n    SKColorType.Rgba8888, SKAlphaType.Premul);\nusing var result = new SKBitmap(info);\nvar srcSpan = jpegBitmap.GetPixelSpan();\nvar dstSpan = result.GetPixelSpan();\nfor (int i = 0, a = 0; i < dstSpan.Length; i += 4, a++) {\n    byte alpha = blpAlpha[a];\n    dstSpan[i + 0] = (byte)(srcSpan[i + 0] * alpha / 255);\n    dstSpan[i + 1] = (byte)(srcSpan[i + 1] * alpha / 255);\n    dstSpan[i + 2] = (byte)(srcSpan[i + 2] * alpha / 255);\n    dstSpan[i + 3] = alpha;\n}\n```\n\nNote: the channel order of `srcSpan` depends on the platform color type (BGRA vs RGBA) — check `SKImageInfo.PlatformColorType` on your platform and adjust accordingly."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — behavior is correct per JPEG spec, workaround documented",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
