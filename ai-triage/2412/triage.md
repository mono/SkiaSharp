# Issue Triage Report — #2412

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-29T22:57:04Z |
| Type | type/question (0.92 (92%)) |
| Area | area/SkiaSharp (0.88 (88%)) |
| Suggested action | close-as-not-a-bug (0.87 (87%)) |

**Issue Summary:** User asks how to convert AV_PIX_FMT_BGR24 (FFmpeg 24-bit BGR pixel data) to SKBitmap; self-proposed code uses wrong color type causing stride mismatch.

**Analysis:** The reporter wants to wrap raw FFmpeg BGR24 pixel data (3 bytes/pixel: B, G, R, no alpha) in an SKBitmap. SkiaSharp has no native 24-bit BGR color type; the closest types are Bgra8888 (4 bytes/pixel) and Rgb888x (4 bytes/pixel, R,G,B,padding). The reporter's self-proposed code uses SKColorType.Bgra8888 with BGR24 data, which is incorrect — BGR24 stride is width*3 bytes but Bgra8888 expects width*4, causing corrupted output. The correct approach is to expand each 3-byte BGR pixel to 4-byte BGRA (with alpha=255) before calling InstallPixels.

**Recommendations:** **close-as-not-a-bug** — Usage question with a clear answer. SkiaSharp has no native 24-bit BGR type; reporter needs to expand BGR24 to BGRA before using InstallPixels. No code change required in the library.

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

**Environment:** SIPSorceryMedia.Abstractions.RawImage in AV_PIX_FMT_BGR24 format (8-bit B, 8-bit G, 8-bit R, no alpha, stride = width * 3)

**Code snippets:**

```csharp
var bmpInfo = new SKImageInfo(rawImage.Width, rawImage.Height, SKColorType.Bgra8888, SKAlphaType.Opaque);
var bitmap = new SKBitmap();
bitmap.InstallPixels(bmpInfo, rawImage.Sample, rawImage.Stride);
```

## Analysis

### Technical Summary

The reporter wants to wrap raw FFmpeg BGR24 pixel data (3 bytes/pixel: B, G, R, no alpha) in an SKBitmap. SkiaSharp has no native 24-bit BGR color type; the closest types are Bgra8888 (4 bytes/pixel) and Rgb888x (4 bytes/pixel, R,G,B,padding). The reporter's self-proposed code uses SKColorType.Bgra8888 with BGR24 data, which is incorrect — BGR24 stride is width*3 bytes but Bgra8888 expects width*4, causing corrupted output. The correct approach is to expand each 3-byte BGR pixel to 4-byte BGRA (with alpha=255) before calling InstallPixels.

### Rationale

This is a usage question, not a bug. The API (InstallPixels, SKColorType) exists and works correctly. The reporter needs guidance on matching the correct SKColorType to their 24-bit BGR data and expanding the pixel buffer accordingly.

### Key Signals

- "8 bits for B, 8 bits for G, 8 bits for R (=> 24 bits for one pixel)" — **issue body** (BGR24 = 3 bytes/pixel, no alpha. SkiaSharp has no direct 24-bit BGR color type.)
- "var bmpInfo = new SKImageInfo(rawImage.Width, rawImage.Height, SKColorType.Bgra8888, SKAlphaType.Opaque)" — **comment #1461525728** (Reporter self-proposes Bgra8888 (4 bytes/pixel) but BGR24 data has 3 bytes/pixel — stride mismatch would produce corrupted image.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/Definitions.cs` | 43,104 | direct | SKColorType.Rgb888x exists (value=5) and has bytesPerPixel=4 (R,G,B,padding). There is no 24-bit BGR color type in SkiaSharp — BGR24 data must be expanded to 4 bytes/pixel before use. |
| `binding/SkiaSharp/SKBitmap.cs` | 589-617 | direct | SKBitmap.InstallPixels(SKImageInfo, IntPtr, int rowBytes) exists and accepts an explicit rowBytes parameter; the rowBytes must match the color type's bytes-per-pixel * width, not the BGR24 stride. |

### Workarounds

- Convert BGR24 to BGRA by expanding each 3-byte pixel to 4 bytes (insert alpha=255 after each R), then use SKColorType.Bgra8888 with SKAlphaType.Opaque and the new stride.
- Use unsafe pointer operations or Marshal to iterate pixels and write BGRA into a pinned byte[] before passing to InstallPixels.

### Resolution Proposals

**Hypothesis:** No SkiaSharp bug. Reporter needs to convert BGR24 byte buffer to BGRA (4 bytes/pixel) before constructing SKBitmap.

1. **Expand BGR24 to BGRA and use InstallPixels** — workaround, confidence 0.88 (88%), cost/s, validated=untested
   - Convert each 3-byte BGR pixel to 4-byte BGRA (alpha=255). The resulting buffer can be passed directly to SKBitmap.InstallPixels with SKColorType.Bgra8888 and SKAlphaType.Opaque.

```csharp
public static SKBitmap ConvertBgr24ToSkBitmap(int width, int height, byte[] bgr24Pixels)
{
    var bgra = new byte[width * height * 4];
    for (int i = 0, j = 0; i < width * height; i++, j += 3)
    {
        bgra[i * 4 + 0] = bgr24Pixels[j + 0]; // B
        bgra[i * 4 + 1] = bgr24Pixels[j + 1]; // G
        bgra[i * 4 + 2] = bgr24Pixels[j + 2]; // R
        bgra[i * 4 + 3] = 255;                  // A
    }
    var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque);
    var bitmap = new SKBitmap();
    var handle = GCHandle.Alloc(bgra, GCHandleType.Pinned);
    try
    {
        bitmap.InstallPixels(info, handle.AddrOfPinnedObject(), width * 4);
        return bitmap.Copy(); // copy so the GCHandle can be freed
    }
    finally
    {
        handle.Free();
    }
}
```

**Recommended proposal:** Expand BGR24 to BGRA and use InstallPixels

**Why:** Only approach that correctly maps BGR24 data to a supported SKColorType without color channel reordering.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.87 (87%) |
| Reason | Usage question with a clear answer. SkiaSharp has no native 24-bit BGR type; reporter needs to expand BGR24 to BGRA before using InstallPixels. No code change required in the library. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply question and SkiaSharp area labels | labels=type/question, area/SkiaSharp |
| add-comment | medium | 0.87 (87%) | Explain why BGR24 needs expansion to BGRA and provide corrected code | — |
| close-issue | medium | 0.82 (82%) | Close as answered — usage question, no code change needed | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the question! SkiaSharp does not have a native 24-bit BGR color type — all supported formats are 1, 2, 4, or 8 bytes per pixel. `AV_PIX_FMT_BGR24` uses 3 bytes per pixel (B, G, R), but the closest SkiaSharp type is `SKColorType.Bgra8888` which requires **4 bytes per pixel** (B, G, R, A). Passing your BGR24 buffer directly with `Bgra8888` will produce corrupted output due to the stride mismatch.

You need to **expand the pixel buffer** by inserting an alpha byte (255 = fully opaque) after every R byte:

```csharp
public static SKBitmap ConvertBgr24ToSkBitmap(int width, int height, byte[] bgr24Pixels)
{
    var bgra = new byte[width * height * 4];
    for (int i = 0, j = 0; i < width * height; i++, j += 3)
    {
        bgra[i * 4 + 0] = bgr24Pixels[j + 0]; // B
        bgra[i * 4 + 1] = bgr24Pixels[j + 1]; // G
        bgra[i * 4 + 2] = bgr24Pixels[j + 2]; // R
        bgra[i * 4 + 3] = 255;                  // A
    }
    var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque);
    var bitmap = new SKBitmap();
    var handle = GCHandle.Alloc(bgra, GCHandleType.Pinned);
    try
    {
        bitmap.InstallPixels(info, handle.AddrOfPinnedObject(), width * 4);
        return bitmap.Copy();
    }
    finally
    {
        handle.Free();
    }
}
```

Note: `bitmap.Copy()` is used so the GCHandle can be freed safely. Alternatively, keep the `GCHandle` alive as long as the bitmap is in use.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2412,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-29T22:57:04Z"
  },
  "summary": "User asks how to convert AV_PIX_FMT_BGR24 (FFmpeg 24-bit BGR pixel data) to SKBitmap; self-proposed code uses wrong color type causing stride mismatch.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.88
    }
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "var bmpInfo = new SKImageInfo(rawImage.Width, rawImage.Height, SKColorType.Bgra8888, SKAlphaType.Opaque);\nvar bitmap = new SKBitmap();\nbitmap.InstallPixels(bmpInfo, rawImage.Sample, rawImage.Stride);"
      ],
      "environmentDetails": "SIPSorceryMedia.Abstractions.RawImage in AV_PIX_FMT_BGR24 format (8-bit B, 8-bit G, 8-bit R, no alpha, stride = width * 3)"
    }
  },
  "analysis": {
    "summary": "The reporter wants to wrap raw FFmpeg BGR24 pixel data (3 bytes/pixel: B, G, R, no alpha) in an SKBitmap. SkiaSharp has no native 24-bit BGR color type; the closest types are Bgra8888 (4 bytes/pixel) and Rgb888x (4 bytes/pixel, R,G,B,padding). The reporter's self-proposed code uses SKColorType.Bgra8888 with BGR24 data, which is incorrect — BGR24 stride is width*3 bytes but Bgra8888 expects width*4, causing corrupted output. The correct approach is to expand each 3-byte BGR pixel to 4-byte BGRA (with alpha=255) before calling InstallPixels.",
    "rationale": "This is a usage question, not a bug. The API (InstallPixels, SKColorType) exists and works correctly. The reporter needs guidance on matching the correct SKColorType to their 24-bit BGR data and expanding the pixel buffer accordingly.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "43,104",
        "finding": "SKColorType.Rgb888x exists (value=5) and has bytesPerPixel=4 (R,G,B,padding). There is no 24-bit BGR color type in SkiaSharp — BGR24 data must be expanded to 4 bytes/pixel before use.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "589-617",
        "finding": "SKBitmap.InstallPixels(SKImageInfo, IntPtr, int rowBytes) exists and accepts an explicit rowBytes parameter; the rowBytes must match the color type's bytes-per-pixel * width, not the BGR24 stride.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "8 bits for B, 8 bits for G, 8 bits for R (=> 24 bits for one pixel)",
        "source": "issue body",
        "interpretation": "BGR24 = 3 bytes/pixel, no alpha. SkiaSharp has no direct 24-bit BGR color type."
      },
      {
        "text": "var bmpInfo = new SKImageInfo(rawImage.Width, rawImage.Height, SKColorType.Bgra8888, SKAlphaType.Opaque)",
        "source": "comment #1461525728",
        "interpretation": "Reporter self-proposes Bgra8888 (4 bytes/pixel) but BGR24 data has 3 bytes/pixel — stride mismatch would produce corrupted image."
      }
    ],
    "workarounds": [
      "Convert BGR24 to BGRA by expanding each 3-byte pixel to 4 bytes (insert alpha=255 after each R), then use SKColorType.Bgra8888 with SKAlphaType.Opaque and the new stride.",
      "Use unsafe pointer operations or Marshal to iterate pixels and write BGRA into a pinned byte[] before passing to InstallPixels."
    ],
    "resolution": {
      "hypothesis": "No SkiaSharp bug. Reporter needs to convert BGR24 byte buffer to BGRA (4 bytes/pixel) before constructing SKBitmap.",
      "proposals": [
        {
          "title": "Expand BGR24 to BGRA and use InstallPixels",
          "description": "Convert each 3-byte BGR pixel to 4-byte BGRA (alpha=255). The resulting buffer can be passed directly to SKBitmap.InstallPixels with SKColorType.Bgra8888 and SKAlphaType.Opaque.",
          "category": "workaround",
          "codeSnippet": "public static SKBitmap ConvertBgr24ToSkBitmap(int width, int height, byte[] bgr24Pixels)\n{\n    var bgra = new byte[width * height * 4];\n    for (int i = 0, j = 0; i < width * height; i++, j += 3)\n    {\n        bgra[i * 4 + 0] = bgr24Pixels[j + 0]; // B\n        bgra[i * 4 + 1] = bgr24Pixels[j + 1]; // G\n        bgra[i * 4 + 2] = bgr24Pixels[j + 2]; // R\n        bgra[i * 4 + 3] = 255;                  // A\n    }\n    var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque);\n    var bitmap = new SKBitmap();\n    var handle = GCHandle.Alloc(bgra, GCHandleType.Pinned);\n    try\n    {\n        bitmap.InstallPixels(info, handle.AddrOfPinnedObject(), width * 4);\n        return bitmap.Copy(); // copy so the GCHandle can be freed\n    }\n    finally\n    {\n        handle.Free();\n    }\n}",
          "confidence": 0.88,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Expand BGR24 to BGRA and use InstallPixels",
      "recommendedReason": "Only approach that correctly maps BGR24 data to a supported SKColorType without color channel reordering."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.87,
      "reason": "Usage question with a clear answer. SkiaSharp has no native 24-bit BGR type; reporter needs to expand BGR24 to BGRA before using InstallPixels. No code change required in the library.",
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
        "description": "Explain why BGR24 needs expansion to BGRA and provide corrected code",
        "risk": "medium",
        "confidence": 0.87,
        "comment": "Thanks for the question! SkiaSharp does not have a native 24-bit BGR color type — all supported formats are 1, 2, 4, or 8 bytes per pixel. `AV_PIX_FMT_BGR24` uses 3 bytes per pixel (B, G, R), but the closest SkiaSharp type is `SKColorType.Bgra8888` which requires **4 bytes per pixel** (B, G, R, A). Passing your BGR24 buffer directly with `Bgra8888` will produce corrupted output due to the stride mismatch.\n\nYou need to **expand the pixel buffer** by inserting an alpha byte (255 = fully opaque) after every R byte:\n\n```csharp\npublic static SKBitmap ConvertBgr24ToSkBitmap(int width, int height, byte[] bgr24Pixels)\n{\n    var bgra = new byte[width * height * 4];\n    for (int i = 0, j = 0; i < width * height; i++, j += 3)\n    {\n        bgra[i * 4 + 0] = bgr24Pixels[j + 0]; // B\n        bgra[i * 4 + 1] = bgr24Pixels[j + 1]; // G\n        bgra[i * 4 + 2] = bgr24Pixels[j + 2]; // R\n        bgra[i * 4 + 3] = 255;                  // A\n    }\n    var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque);\n    var bitmap = new SKBitmap();\n    var handle = GCHandle.Alloc(bgra, GCHandleType.Pinned);\n    try\n    {\n        bitmap.InstallPixels(info, handle.AddrOfPinnedObject(), width * 4);\n        return bitmap.Copy();\n    }\n    finally\n    {\n        handle.Free();\n    }\n}\n```\n\nNote: `bitmap.Copy()` is used so the GCHandle can be freed safely. Alternatively, keep the `GCHandle` alive as long as the bitmap is in use."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — usage question, no code change needed",
        "risk": "medium",
        "confidence": 0.82,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
