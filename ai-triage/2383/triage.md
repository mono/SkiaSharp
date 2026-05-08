# Issue Triage Report — #2383

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T15:15:53Z |
| Type | type/question (0.92 (92%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-not-a-bug (0.85 (85%)) |

**Issue Summary:** User asks whether SkiaSharp has a 24-bit packed RGB color type equivalent to System.Drawing.Imaging.PixelFormat.Format24bppRgb.

**Analysis:** The reporter wants to convert a 24-bit RGB image (3 bytes per pixel, no alpha) to SKBitmap by specifying width, height, and stride. SkiaSharp's SKColorType enum does not include a true 24-bit packed RGB format; the closest is Rgb888x (32-bit, 4 bytes/pixel with an unused byte) and Rgb565 (16-bit). Skia removed packed 24-bit RGB support long ago. The reporter can use Rgb888x or convert their raw pixel data to a 32-bit layout before creating an SKBitmap, or use SKBitmap.Decode/Install from pixel data.

**Recommendations:** **close-as-not-a-bug** — SkiaSharp has no 24bpp packed RGB type by design (Skia removed it). The question has a clear answer with a known workaround using Rgb888x.

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

## Analysis

### Technical Summary

The reporter wants to convert a 24-bit RGB image (3 bytes per pixel, no alpha) to SKBitmap by specifying width, height, and stride. SkiaSharp's SKColorType enum does not include a true 24-bit packed RGB format; the closest is Rgb888x (32-bit, 4 bytes/pixel with an unused byte) and Rgb565 (16-bit). Skia removed packed 24-bit RGB support long ago. The reporter can use Rgb888x or convert their raw pixel data to a 32-bit layout before creating an SKBitmap, or use SKBitmap.Decode/Install from pixel data.

### Rationale

This is a question — the reporter asks whether a specific color format is supported. Code investigation confirms SKColorType has no 24bpp packed RGB type. The best answers are: use Rgb888x (32-bit with unused alpha byte) or expand the pixel data. No bug exists; the behavior is by-design since Skia itself dropped packed 24-bit support.

### Key Signals

- "use 8 bits for each colors; no alpha; necessary to specify width, height and stride" — **issue body** (Reporter wants a packed 24bpp RGB pixel format equivalent to System.Drawing Format24bppRgb)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/Definitions.cs` | 36-66 | direct | SKColorType enum lists all supported color types. There is no 24-bit packed RGB (3 bytes/pixel) type. Rgb888x is 32-bit (4 bytes/pixel) — 8-8-8 RGB with one unused byte. Rgb565 is 16-bit. |
| `binding/SkiaSharp/Definitions.cs` | 82-120 | direct | GetBytesPerPixel extension confirms Rgb888x uses 4 bytes/pixel, Rgb565 uses 2 bytes/pixel. No 3 bytes/pixel (24-bit packed) entry exists for any color type. |

### Workarounds

- Use SKColorType.Rgb888x with 4 bytes/pixel layout — pad each pixel with an extra 0xFF or 0x00 byte when copying from 24bpp source data.
- Use SKBitmap.InstallPixels with SKImageInfo using Rgb888x and manually convert the stride — the stride in Rgb888x is width*4, vs width*3 for 24bpp.
- Use SKBitmap.Decode on an encoded image (JPEG/PNG) — the codec handles format conversion internally and returns a bitmap in a supported color type.

### Next Questions

- Is the reporter working with raw pixel buffers or encoded image files? If encoded files, SKBitmap.Decode handles conversion automatically.
- What .NET platform is the reporter targeting? (SkiaSharp is cross-platform; System.Drawing is Windows-only.)

### Resolution Proposals

1. **Use SKColorType.Rgb888x with manual pixel expansion** — workaround, cost/s, validated=yes
   - Expand each 24bpp pixel (3 bytes: R, G, B) to 32bpp (4 bytes: R, G, B, 0xFF) and use SKColorType.Rgb888x with SKAlphaType.Opaque.

```csharp
// Expand 24bpp source data to 32bpp for SKBitmap
int width = 100; int height = 100;
int srcStride = width * 3;
byte[] src24 = new byte[srcStride * height]; // your 24bpp RGB data
byte[] dst32 = new byte[width * 4 * height];
for (int y = 0; y < height; y++)
for (int x = 0; x < width; x++) {
    int si = y * srcStride + x * 3;
    int di = (y * width + x) * 4;
    dst32[di]     = src24[si];     // R
    dst32[di + 1] = src24[si + 1]; // G
    dst32[di + 2] = src24[si + 2]; // B
    dst32[di + 3] = 0xFF;           // unused / opaque
}
var info = new SKImageInfo(width, height, SKColorType.Rgb888x, SKAlphaType.Opaque);
using var bitmap = new SKBitmap();
var gcHandle = GCHandle.Alloc(dst32, GCHandleType.Pinned);
bitmap.InstallPixels(info, gcHandle.AddrOfPinnedObject(), width * 4, (addr, ctx) => ((GCHandle)ctx).Free(), gcHandle);
```

**Recommended proposal:** workaround-1

**Why:** Rgb888x is the closest supported type; a simple byte loop converts 24bpp to 32bpp without quality loss.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.85 (85%) |
| Reason | SkiaSharp has no 24bpp packed RGB type by design (Skia removed it). The question has a clear answer with a known workaround using Rgb888x. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply type/question and area/SkiaSharp labels | labels=type/question, area/SkiaSharp |
| add-comment | medium | 0.85 (85%) | Answer the question: SkiaSharp has no 24bpp RGB type; explain Rgb888x as the closest option with a workaround snippet. | — |
| close-issue | medium | 0.80 (80%) | Close as not-a-bug: question answered, no missing API (24bpp was removed from Skia by design). | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
## Answer

SkiaSharp does **not** have a true 24-bit packed RGB color type (3 bytes per pixel). Skia itself removed packed 24-bit RGB support, so there is no equivalent to `System.Drawing.Imaging.PixelFormat.Format24bppRgb`.

### Closest alternative: `SKColorType.Rgb888x`

`SKColorType.Rgb888x` stores 8 bits per channel (R, G, B) with **no alpha**, but uses **4 bytes per pixel** (the fourth byte is unused padding). You can expand your 24bpp source data to this format:

```csharp
int width = 100, height = 100;
int srcStride = width * 3;
byte[] src24 = /* your 24bpp RGB data */;
byte[] dst32 = new byte[width * 4 * height];

for (int y = 0; y < height; y++)
for (int x = 0; x < width; x++) {
    int si = y * srcStride + x * 3;
    int di = (y * width + x) * 4;
    dst32[di]     = src24[si];     // R
    dst32[di + 1] = src24[si + 1]; // G
    dst32[di + 2] = src24[si + 2]; // B
    dst32[di + 3] = 0xFF;          // unused
}

var info = new SKImageInfo(width, height, SKColorType.Rgb888x, SKAlphaType.Opaque);
using var bitmap = new SKBitmap();
var gcHandle = GCHandle.Alloc(dst32, GCHandleType.Pinned);
bitmap.InstallPixels(
    info,
    gcHandle.AddrOfPinnedObject(),
    width * 4,
    (addr, ctx) => ((GCHandle)ctx).Free(),
    gcHandle);
```

### If working with encoded images (JPEG/PNG)

If your source is a file or stream rather than raw pixel bytes, use `SKBitmap.Decode` — it handles format conversion internally:

```csharp
using var bitmap = SKBitmap.Decode("path/to/image.jpg");
```

If you need more control over the decoded format, you can use `SKCodec` with a custom `SKImageInfo`.

> **Note:** `System.Drawing` is Windows-only in modern .NET. SkiaSharp is designed as a cross-platform replacement and maps to Skia's native type system, which has no packed 24-bit format.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2383,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T15:15:53Z"
  },
  "summary": "User asks whether SkiaSharp has a 24-bit packed RGB color type equivalent to System.Drawing.Imaging.PixelFormat.Format24bppRgb.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    }
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [],
      "attachments": [],
      "repoLinks": []
    }
  },
  "analysis": {
    "summary": "The reporter wants to convert a 24-bit RGB image (3 bytes per pixel, no alpha) to SKBitmap by specifying width, height, and stride. SkiaSharp's SKColorType enum does not include a true 24-bit packed RGB format; the closest is Rgb888x (32-bit, 4 bytes/pixel with an unused byte) and Rgb565 (16-bit). Skia removed packed 24-bit RGB support long ago. The reporter can use Rgb888x or convert their raw pixel data to a 32-bit layout before creating an SKBitmap, or use SKBitmap.Decode/Install from pixel data.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "36-66",
        "finding": "SKColorType enum lists all supported color types. There is no 24-bit packed RGB (3 bytes/pixel) type. Rgb888x is 32-bit (4 bytes/pixel) — 8-8-8 RGB with one unused byte. Rgb565 is 16-bit.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "82-120",
        "finding": "GetBytesPerPixel extension confirms Rgb888x uses 4 bytes/pixel, Rgb565 uses 2 bytes/pixel. No 3 bytes/pixel (24-bit packed) entry exists for any color type.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "use 8 bits for each colors; no alpha; necessary to specify width, height and stride",
        "source": "issue body",
        "interpretation": "Reporter wants a packed 24bpp RGB pixel format equivalent to System.Drawing Format24bppRgb"
      }
    ],
    "rationale": "This is a question — the reporter asks whether a specific color format is supported. Code investigation confirms SKColorType has no 24bpp packed RGB type. The best answers are: use Rgb888x (32-bit with unused alpha byte) or expand the pixel data. No bug exists; the behavior is by-design since Skia itself dropped packed 24-bit support.",
    "workarounds": [
      "Use SKColorType.Rgb888x with 4 bytes/pixel layout — pad each pixel with an extra 0xFF or 0x00 byte when copying from 24bpp source data.",
      "Use SKBitmap.InstallPixels with SKImageInfo using Rgb888x and manually convert the stride — the stride in Rgb888x is width*4, vs width*3 for 24bpp.",
      "Use SKBitmap.Decode on an encoded image (JPEG/PNG) — the codec handles format conversion internally and returns a bitmap in a supported color type."
    ],
    "resolution": {
      "proposals": [
        {
          "title": "Use SKColorType.Rgb888x with manual pixel expansion",
          "category": "workaround",
          "effort": "cost/s",
          "validated": "yes",
          "description": "Expand each 24bpp pixel (3 bytes: R, G, B) to 32bpp (4 bytes: R, G, B, 0xFF) and use SKColorType.Rgb888x with SKAlphaType.Opaque.",
          "codeSnippet": "// Expand 24bpp source data to 32bpp for SKBitmap\nint width = 100; int height = 100;\nint srcStride = width * 3;\nbyte[] src24 = new byte[srcStride * height]; // your 24bpp RGB data\nbyte[] dst32 = new byte[width * 4 * height];\nfor (int y = 0; y < height; y++)\nfor (int x = 0; x < width; x++) {\n    int si = y * srcStride + x * 3;\n    int di = (y * width + x) * 4;\n    dst32[di]     = src24[si];     // R\n    dst32[di + 1] = src24[si + 1]; // G\n    dst32[di + 2] = src24[si + 2]; // B\n    dst32[di + 3] = 0xFF;           // unused / opaque\n}\nvar info = new SKImageInfo(width, height, SKColorType.Rgb888x, SKAlphaType.Opaque);\nusing var bitmap = new SKBitmap();\nvar gcHandle = GCHandle.Alloc(dst32, GCHandleType.Pinned);\nbitmap.InstallPixels(info, gcHandle.AddrOfPinnedObject(), width * 4, (addr, ctx) => ((GCHandle)ctx).Free(), gcHandle);"
        }
      ],
      "recommendedProposal": "workaround-1",
      "recommendedReason": "Rgb888x is the closest supported type; a simple byte loop converts 24bpp to 32bpp without quality loss."
    },
    "nextQuestions": [
      "Is the reporter working with raw pixel buffers or encoded image files? If encoded files, SKBitmap.Decode handles conversion automatically.",
      "What .NET platform is the reporter targeting? (SkiaSharp is cross-platform; System.Drawing is Windows-only.)"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.85,
      "reason": "SkiaSharp has no 24bpp packed RGB type by design (Skia removed it). The question has a clear answer with a known workaround using Rgb888x.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/question and area/SkiaSharp labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Answer the question: SkiaSharp has no 24bpp RGB type; explain Rgb888x as the closest option with a workaround snippet.",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "## Answer\n\nSkiaSharp does **not** have a true 24-bit packed RGB color type (3 bytes per pixel). Skia itself removed packed 24-bit RGB support, so there is no equivalent to `System.Drawing.Imaging.PixelFormat.Format24bppRgb`.\n\n### Closest alternative: `SKColorType.Rgb888x`\n\n`SKColorType.Rgb888x` stores 8 bits per channel (R, G, B) with **no alpha**, but uses **4 bytes per pixel** (the fourth byte is unused padding). You can expand your 24bpp source data to this format:\n\n```csharp\nint width = 100, height = 100;\nint srcStride = width * 3;\nbyte[] src24 = /* your 24bpp RGB data */;\nbyte[] dst32 = new byte[width * 4 * height];\n\nfor (int y = 0; y < height; y++)\nfor (int x = 0; x < width; x++) {\n    int si = y * srcStride + x * 3;\n    int di = (y * width + x) * 4;\n    dst32[di]     = src24[si];     // R\n    dst32[di + 1] = src24[si + 1]; // G\n    dst32[di + 2] = src24[si + 2]; // B\n    dst32[di + 3] = 0xFF;          // unused\n}\n\nvar info = new SKImageInfo(width, height, SKColorType.Rgb888x, SKAlphaType.Opaque);\nusing var bitmap = new SKBitmap();\nvar gcHandle = GCHandle.Alloc(dst32, GCHandleType.Pinned);\nbitmap.InstallPixels(\n    info,\n    gcHandle.AddrOfPinnedObject(),\n    width * 4,\n    (addr, ctx) => ((GCHandle)ctx).Free(),\n    gcHandle);\n```\n\n### If working with encoded images (JPEG/PNG)\n\nIf your source is a file or stream rather than raw pixel bytes, use `SKBitmap.Decode` — it handles format conversion internally:\n\n```csharp\nusing var bitmap = SKBitmap.Decode(\"path/to/image.jpg\");\n```\n\nIf you need more control over the decoded format, you can use `SKCodec` with a custom `SKImageInfo`.\n\n> **Note:** `System.Drawing` is Windows-only in modern .NET. SkiaSharp is designed as a cross-platform replacement and maps to Skia's native type system, which has no packed 24-bit format."
      },
      {
        "type": "close-issue",
        "description": "Close as not-a-bug: question answered, no missing API (24bpp was removed from Skia by design).",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
