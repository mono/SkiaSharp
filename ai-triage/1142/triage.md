# Issue Triage Report — #1142

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T04:43:09Z |
| Type | type/enhancement (0.82 (82%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-investigation (0.78 (78%)) |

**Issue Summary:** Reporter loses RGB color data in transparent (alpha=0) pixels when compositing textures via SKSurface and exporting to PNG; root cause is Skia's premultiplied alpha pipeline zeroing color channels when alpha is zero.

**Analysis:** Skia's rendering pipeline uses premultiplied alpha internally. Any pixel with alpha=0 is stored as (0,0,0,0) because premul(R,G,B,0) = (0*R, 0*G, 0*B, 0). This affects SKSurface compositing and SKBitmap.SetPixel (which internally uses SKCanvas.DrawPoint). The color data is irrecoverably lost once drawn. SKPngEncoderOptions and SKWebpEncoderOptions offer no option to preserve transparent pixel colors (analogous to ImageSharp's TransparentColorMode.Preserve). Direct pixel buffer writes (GetPixelSpan) bypass the premultiplication path and can preserve arbitrary color data in transparent pixels.

**Recommendations:** **needs-investigation** — Well-understood root cause (premultiplied alpha) with a viable immediate workaround, but the enhancement (encoder option) requires investigating upstream Skia support before scoping the implementation.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create an SKSurface with default (premultiplied) alpha type
2. Draw textures that have non-color data encoded in the alpha channel
3. Call surface.Snapshot() to get an SKImage
4. Encode the image to PNG
5. Observe that all pixels with alpha=0 have RGB set to (0,0,0)

**Environment:** SkiaSharp 1.68.1.1, .NET Core 3.1, win-x64, Windows 64-bit

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1142#issuecomment-2229616973 — Confirmation: RGB data is completely missing from bitmap in memory, not just viewer error
- https://github.com/mono/SkiaSharp/issues/1142#issuecomment-2930136422 — User requests TransparentColorMode.Preserve equivalent for WebP encoding, references ImageSharp feature
- https://github.com/mono/SkiaSharp/issues/1142#issuecomment-3455437060 — User provides test case showing SetPixel with alpha=0 zeroes RGB; documents alpha=1 workaround

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.1.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The premultiplied alpha rendering pipeline is fundamental to Skia and unchanged. SKBitmap.SetPixel still uses SKCanvas.DrawPoint, and encoder options still lack a transparent color preservation mode. |

## Analysis

### Technical Summary

Skia's rendering pipeline uses premultiplied alpha internally. Any pixel with alpha=0 is stored as (0,0,0,0) because premul(R,G,B,0) = (0*R, 0*G, 0*B, 0). This affects SKSurface compositing and SKBitmap.SetPixel (which internally uses SKCanvas.DrawPoint). The color data is irrecoverably lost once drawn. SKPngEncoderOptions and SKWebpEncoderOptions offer no option to preserve transparent pixel colors (analogous to ImageSharp's TransparentColorMode.Preserve). Direct pixel buffer writes (GetPixelSpan) bypass the premultiplication path and can preserve arbitrary color data in transparent pixels.

### Rationale

The behavior is technically by-design (Skia's rendering always uses premultiplied alpha), but the issue has accumulated three independent confirmations and two explicit feature requests for a TransparentColorMode.Preserve-style option. Classifying as type/enhancement since: the root behavior is a Skia design choice, users want a configurable option in encoder APIs, and a direct pixel buffer workaround exists. The lack of any such option in SKPngEncoderOptions or SKWebpEncoderOptions is the actionable gap.

### Key Signals

- "all areas with an alpha of 0 have their color values set to (0,0,0)" — **issue body** (Classic premultiplied alpha color loss: premul(R,G,B,0) = (0,0,0,0). The color data is lost at the rendering stage, not at encoding.)
- "I'm pretty certain the RGB data is completely missing from the bitmap in memory and then the file on disk" — **comment by floxay** (Confirms the data is lost at the pixel level before encoding, not during PNG I/O.)
- "I'd like to switch to SkiaSharp for better performance, but currently missing the feature" — **comment by SladeThe** (Clear enhancement request: a TransparentColorMode-equivalent API for controlling transparent pixel color preservation during encoding.)
- "this behaviour should be controllable in Skia / SkiaSharp with a parameter at least" — **comment by JimmyDirect** (Multiple users concur that a configurable option is needed, not just a workaround.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 131-141 | direct | SetPixel() uses SKCanvas.DrawPoint() internally. This means all pixels are drawn through the premultiplied alpha pipeline regardless of the bitmap's declared AlphaType. For alpha=0, premul(R,G,B,0) = (0,0,0,0), destroying the original RGB values. |
| `binding/SkiaSharp/Definitions.cs` | 521-544 | direct | SKPngEncoderOptions only exposes FilterFlags and ZLibLevel — no option to control how transparent pixels are stored in the PNG output. There is no TransparentColorMode-style parameter to prevent Skia from zeroing RGB for alpha=0 pixels. |
| `binding/SkiaSharp/Definitions.cs` | 587-605 | related | SKWebpEncoderOptions only exposes Compression and Quality — no transparent color preservation option. The ImageSharp equivalient WebpTransparentColorMode.Preserve has no counterpart in SkiaSharp. |
| `binding/SkiaSharp/SKBitmap.cs` | 300-304 | context | GetPixelSpan() exposes raw pixel bytes directly, bypassing the canvas drawing path. Writing pixels directly via this span preserves arbitrary RGBA values including color in alpha=0 pixels. This is the viable workaround. |

### Workarounds

- Write pixel data directly via SKBitmap.GetPixelSpan() using an Unpremul bitmap — this bypasses the canvas drawing path and preserves arbitrary RGBA values
- Use SKBitmap with SKAlphaType.Unpremul and write raw bytes: span[0]=R, span[1]=G, span[2]=B, span[3]=0; then encode via SKBitmap.Encode()
- Near-workaround (lossy): set alpha=1 instead of alpha=0 for pixels that carry color data (from JimmyDirect's comment — low alpha may be imperceptible but avoids premul zeroing)
- Use ImageSharp with WebpTransparentColorMode.Preserve if WebP with preserved transparent color is required

### Next Questions

- Does upstream Skia's SkPngEncoder or SkWebpEncoder support a transparent color preservation mode that could be exposed?
- Would it be feasible to add a TransparentColorMode property to SKPngEncoderOptions / SKWebpEncoderOptions?
- Should SKBitmap.SetPixel be fixed to bypass premultiplication when the bitmap's AlphaType is Unpremul?

### Resolution Proposals

**Hypothesis:** The fix would be to add a TransparentColorPreservation option (enum or bool) to SKPngEncoderOptions and SKWebpEncoderOptions, and map it to the appropriate Skia encoder parameter if upstream supports it. Alternatively, a dedicated API for unpremultiplied pixel writes could be added.

1. **Direct pixel buffer workaround using GetPixelSpan** — workaround, confidence 0.90 (90%), cost/s, validated=untested
   - Create an SKBitmap with SKAlphaType.Unpremul, then write pixel data directly via GetPixelSpan() instead of using SKCanvas/SetPixel. Encode via SKBitmap.Encode().

```csharp
var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
var bmp = new SKBitmap(info);
var span = bmp.GetPixelSpan();
for (int i = 0; i < span.Length; i += 4) {
    span[i + 0] = r; // Red
    span[i + 1] = g; // Green
    span[i + 2] = b; // Blue
    span[i + 3] = a; // Alpha (can be 0 while preserving RGB)
}
using var data = bmp.Encode(SKEncodedImageFormat.Png, 100);
```
2. **Add TransparentColorPreservation option to encoder options** — fix, confidence 0.60 (60%), cost/m, validated=untested
   - Add a boolean or enum property to SKPngEncoderOptions and SKWebpEncoderOptions to control whether RGB values are preserved for alpha=0 pixels (analogous to ImageSharp's WebpTransparentColorMode.Preserve). Requires investigating upstream Skia encoder API support.

**Recommended proposal:** Direct pixel buffer workaround using GetPixelSpan

**Why:** Immediately actionable with no SkiaSharp changes needed. The GetPixelSpan() API exists today and bypasses premultiplication entirely.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.78 (78%) |
| Reason | Well-understood root cause (premultiplied alpha) with a viable immediate workaround, but the enhancement (encoder option) requires investigating upstream Skia support before scoping the implementation. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Apply enhancement, SkiaSharp area, and Windows platform labels | labels=type/enhancement, area/SkiaSharp, os/Windows-Classic |
| add-comment | medium | 0.85 (85%) | Explain premultiplied alpha root cause and provide GetPixelSpan workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report. This is caused by Skia's **premultiplied alpha** rendering pipeline. When a pixel has alpha=0, Skia stores it as `premul(R,G,B,0) = (0,0,0,0)` — the RGB data is multiplied by zero and lost before encoding even begins.

This applies to both `SKSurface` compositing and `SKBitmap.SetPixel` (which internally uses `SKCanvas.DrawPoint`).

**Workaround — write pixel data directly to bypass premultiplication:**
```csharp
var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
var bmp = new SKBitmap(info);
var span = bmp.GetPixelSpan();
for (int i = 0; i < span.Length; i += 4) {
    span[i + 0] = r; // Red
    span[i + 1] = g; // Green
    span[i + 2] = b; // Blue
    span[i + 3] = a; // Alpha (0 is fine — color is preserved)
}
using var data = bmp.Encode(SKEncodedImageFormat.Png, 100);
```

For the enhancement request (a `TransparentColorMode.Preserve`-style option in `SKPngEncoderOptions`/`SKWebpEncoderOptions`): that requires upstream Skia encoder support and is tracked here. +1s / 👍 on the original post help prioritize it.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1142,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T04:43:09Z"
  },
  "summary": "Reporter loses RGB color data in transparent (alpha=0) pixels when compositing textures via SKSurface and exporting to PNG; root cause is Skia's premultiplied alpha pipeline zeroing color channels when alpha is zero.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.82
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKSurface with default (premultiplied) alpha type",
        "Draw textures that have non-color data encoded in the alpha channel",
        "Call surface.Snapshot() to get an SKImage",
        "Encode the image to PNG",
        "Observe that all pixels with alpha=0 have RGB set to (0,0,0)"
      ],
      "environmentDetails": "SkiaSharp 1.68.1.1, .NET Core 3.1, win-x64, Windows 64-bit",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1142#issuecomment-2229616973",
          "description": "Confirmation: RGB data is completely missing from bitmap in memory, not just viewer error"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1142#issuecomment-2930136422",
          "description": "User requests TransparentColorMode.Preserve equivalent for WebP encoding, references ImageSharp feature"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1142#issuecomment-3455437060",
          "description": "User provides test case showing SetPixel with alpha=0 zeroes RGB; documents alpha=1 workaround"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.1.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The premultiplied alpha rendering pipeline is fundamental to Skia and unchanged. SKBitmap.SetPixel still uses SKCanvas.DrawPoint, and encoder options still lack a transparent color preservation mode."
    }
  },
  "analysis": {
    "summary": "Skia's rendering pipeline uses premultiplied alpha internally. Any pixel with alpha=0 is stored as (0,0,0,0) because premul(R,G,B,0) = (0*R, 0*G, 0*B, 0). This affects SKSurface compositing and SKBitmap.SetPixel (which internally uses SKCanvas.DrawPoint). The color data is irrecoverably lost once drawn. SKPngEncoderOptions and SKWebpEncoderOptions offer no option to preserve transparent pixel colors (analogous to ImageSharp's TransparentColorMode.Preserve). Direct pixel buffer writes (GetPixelSpan) bypass the premultiplication path and can preserve arbitrary color data in transparent pixels.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "131-141",
        "finding": "SetPixel() uses SKCanvas.DrawPoint() internally. This means all pixels are drawn through the premultiplied alpha pipeline regardless of the bitmap's declared AlphaType. For alpha=0, premul(R,G,B,0) = (0,0,0,0), destroying the original RGB values.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "521-544",
        "finding": "SKPngEncoderOptions only exposes FilterFlags and ZLibLevel — no option to control how transparent pixels are stored in the PNG output. There is no TransparentColorMode-style parameter to prevent Skia from zeroing RGB for alpha=0 pixels.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "587-605",
        "finding": "SKWebpEncoderOptions only exposes Compression and Quality — no transparent color preservation option. The ImageSharp equivalient WebpTransparentColorMode.Preserve has no counterpart in SkiaSharp.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "300-304",
        "finding": "GetPixelSpan() exposes raw pixel bytes directly, bypassing the canvas drawing path. Writing pixels directly via this span preserves arbitrary RGBA values including color in alpha=0 pixels. This is the viable workaround.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "all areas with an alpha of 0 have their color values set to (0,0,0)",
        "source": "issue body",
        "interpretation": "Classic premultiplied alpha color loss: premul(R,G,B,0) = (0,0,0,0). The color data is lost at the rendering stage, not at encoding."
      },
      {
        "text": "I'm pretty certain the RGB data is completely missing from the bitmap in memory and then the file on disk",
        "source": "comment by floxay",
        "interpretation": "Confirms the data is lost at the pixel level before encoding, not during PNG I/O."
      },
      {
        "text": "I'd like to switch to SkiaSharp for better performance, but currently missing the feature",
        "source": "comment by SladeThe",
        "interpretation": "Clear enhancement request: a TransparentColorMode-equivalent API for controlling transparent pixel color preservation during encoding."
      },
      {
        "text": "this behaviour should be controllable in Skia / SkiaSharp with a parameter at least",
        "source": "comment by JimmyDirect",
        "interpretation": "Multiple users concur that a configurable option is needed, not just a workaround."
      }
    ],
    "rationale": "The behavior is technically by-design (Skia's rendering always uses premultiplied alpha), but the issue has accumulated three independent confirmations and two explicit feature requests for a TransparentColorMode.Preserve-style option. Classifying as type/enhancement since: the root behavior is a Skia design choice, users want a configurable option in encoder APIs, and a direct pixel buffer workaround exists. The lack of any such option in SKPngEncoderOptions or SKWebpEncoderOptions is the actionable gap.",
    "workarounds": [
      "Write pixel data directly via SKBitmap.GetPixelSpan() using an Unpremul bitmap — this bypasses the canvas drawing path and preserves arbitrary RGBA values",
      "Use SKBitmap with SKAlphaType.Unpremul and write raw bytes: span[0]=R, span[1]=G, span[2]=B, span[3]=0; then encode via SKBitmap.Encode()",
      "Near-workaround (lossy): set alpha=1 instead of alpha=0 for pixels that carry color data (from JimmyDirect's comment — low alpha may be imperceptible but avoids premul zeroing)",
      "Use ImageSharp with WebpTransparentColorMode.Preserve if WebP with preserved transparent color is required"
    ],
    "nextQuestions": [
      "Does upstream Skia's SkPngEncoder or SkWebpEncoder support a transparent color preservation mode that could be exposed?",
      "Would it be feasible to add a TransparentColorMode property to SKPngEncoderOptions / SKWebpEncoderOptions?",
      "Should SKBitmap.SetPixel be fixed to bypass premultiplication when the bitmap's AlphaType is Unpremul?"
    ],
    "resolution": {
      "hypothesis": "The fix would be to add a TransparentColorPreservation option (enum or bool) to SKPngEncoderOptions and SKWebpEncoderOptions, and map it to the appropriate Skia encoder parameter if upstream supports it. Alternatively, a dedicated API for unpremultiplied pixel writes could be added.",
      "proposals": [
        {
          "title": "Direct pixel buffer workaround using GetPixelSpan",
          "description": "Create an SKBitmap with SKAlphaType.Unpremul, then write pixel data directly via GetPixelSpan() instead of using SKCanvas/SetPixel. Encode via SKBitmap.Encode().",
          "category": "workaround",
          "codeSnippet": "var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);\nvar bmp = new SKBitmap(info);\nvar span = bmp.GetPixelSpan();\nfor (int i = 0; i < span.Length; i += 4) {\n    span[i + 0] = r; // Red\n    span[i + 1] = g; // Green\n    span[i + 2] = b; // Blue\n    span[i + 3] = a; // Alpha (can be 0 while preserving RGB)\n}\nusing var data = bmp.Encode(SKEncodedImageFormat.Png, 100);",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Add TransparentColorPreservation option to encoder options",
          "description": "Add a boolean or enum property to SKPngEncoderOptions and SKWebpEncoderOptions to control whether RGB values are preserved for alpha=0 pixels (analogous to ImageSharp's WebpTransparentColorMode.Preserve). Requires investigating upstream Skia encoder API support.",
          "category": "fix",
          "confidence": 0.6,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Direct pixel buffer workaround using GetPixelSpan",
      "recommendedReason": "Immediately actionable with no SkiaSharp changes needed. The GetPixelSpan() API exists today and bypasses premultiplication entirely."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.78,
      "reason": "Well-understood root cause (premultiplied alpha) with a viable immediate workaround, but the enhancement (encoder option) requires investigating upstream Skia support before scoping the implementation.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement, SkiaSharp area, and Windows platform labels",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp",
          "os/Windows-Classic"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain premultiplied alpha root cause and provide GetPixelSpan workaround",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed report. This is caused by Skia's **premultiplied alpha** rendering pipeline. When a pixel has alpha=0, Skia stores it as `premul(R,G,B,0) = (0,0,0,0)` — the RGB data is multiplied by zero and lost before encoding even begins.\n\nThis applies to both `SKSurface` compositing and `SKBitmap.SetPixel` (which internally uses `SKCanvas.DrawPoint`).\n\n**Workaround — write pixel data directly to bypass premultiplication:**\n```csharp\nvar info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);\nvar bmp = new SKBitmap(info);\nvar span = bmp.GetPixelSpan();\nfor (int i = 0; i < span.Length; i += 4) {\n    span[i + 0] = r; // Red\n    span[i + 1] = g; // Green\n    span[i + 2] = b; // Blue\n    span[i + 3] = a; // Alpha (0 is fine — color is preserved)\n}\nusing var data = bmp.Encode(SKEncodedImageFormat.Png, 100);\n```\n\nFor the enhancement request (a `TransparentColorMode.Preserve`-style option in `SKPngEncoderOptions`/`SKWebpEncoderOptions`): that requires upstream Skia encoder support and is tracked here. +1s / 👍 on the original post help prioritize it."
      }
    ]
  }
}
```

</details>
