# Issue Triage Report — #1950

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T20:04:27Z |
| Type | type/question (0.85 (85%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.82 (82%)) |

**Issue Summary:** Reporter asks how to preserve color fidelity when converting TIFF images to JPEG via LibTiff.Net and SkiaSharp; monochrome/X-Ray TIFFs appear black due to a pixel-format mismatch between LibTiff's RGBA output and the SKImageInfo color type used.

**Analysis:** The reporter uses PlatformColorType (Bgra8888 on most platforms) for the SKImageInfo, but LibTiff.Net's ReadRGBAImageOriented fills the int[] buffer in RGBA memory order (R byte first). On little-endian hosts this means bytes are [R, G, B, A] = Rgba8888, not [B, G, R, A] = Bgra8888. The mismatch swaps the red and blue channels. Monochrome X-Ray images become black because high-value grayscale pixels with alpha=255 end up being interpreted as premultiplied with 0 alpha in the wrong channel position. Also, ReadRGBAImage always returns straight alpha (un-premultiplied), while the code specifies SKAlphaType.Premul, causing further corruption for any semi-transparent pixels.

**Recommendations:** **close-as-not-a-bug** — SkiaSharp works correctly. The issue is a pixel format mismatch between LibTiff.Net's output (Rgba8888, straight alpha) and the SKImageInfo passed (PlatformColorType=Bgra8888, premultiplied alpha). The fix is a caller-side change to use the correct color type.

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

1. Open a monochrome/X-Ray TIFF using LibTiff.Net Tiff.ClientOpen
2. Allocate int[] raster and call tif.ReadRGBAImageOriented with Orientation.TOPLEFT
3. Create SKBitmap with SKImageInfo using PlatformColorType and SKAlphaType.Premul
4. Call bitmap.InstallPixels with the pinned raster pointer
5. Encode to JPEG with SKImage.FromBitmap -> image.Encode(SKEncodedImageFormat.Jpeg, 100)
6. Observe resulting JPEG is all-black for monochrome/X-Ray sources

**Environment:** No version info provided; uses LibTiff.Net + SkiaSharp

**Code snippets:**

```csharp
var info = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
bitmap.InstallPixels(info, ptr.AddrOfPinnedObject(), info.RowBytes, null, null);
tif.ReadRGBAImageOriented(width, height, raster, Orientation.TOPLEFT);
```

## Analysis

### Technical Summary

The reporter uses PlatformColorType (Bgra8888 on most platforms) for the SKImageInfo, but LibTiff.Net's ReadRGBAImageOriented fills the int[] buffer in RGBA memory order (R byte first). On little-endian hosts this means bytes are [R, G, B, A] = Rgba8888, not [B, G, R, A] = Bgra8888. The mismatch swaps the red and blue channels. Monochrome X-Ray images become black because high-value grayscale pixels with alpha=255 end up being interpreted as premultiplied with 0 alpha in the wrong channel position. Also, ReadRGBAImage always returns straight alpha (un-premultiplied), while the code specifies SKAlphaType.Premul, causing further corruption for any semi-transparent pixels.

### Rationale

No broken SkiaSharp behavior is described. The API works correctly — the caller passes the wrong SKColorType for the pixel data produced by LibTiff.Net. This is a typical usage/interop question. SkiaSharp's InstallPixels API trusts the caller's SKImageInfo description; PlatformColorType varies by OS and does not match LibTiff's fixed RGBA output format.

### Key Signals

- "Images which have a white background and have some graphs convert OK, but the colours are slightly different" — **issue body** (Colors differ slightly for RGB images — classic R/B channel swap where non-equal R and B components show as tint shift)
- "images that are kind of monochrome (e.g. pictures of X-Rays) are totally black when converted" — **issue body** (Monochrome images go fully black — likely because premultiplied alpha interpretation of straight-alpha data zeroes out channels)
- "var info = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, SKColorSpace.CreateSrgb())" — **issue body code** (PlatformColorType is Bgra8888 on most platforms; LibTiff returns Rgba8888 memory layout — direct cause of color mismatch)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKImageInfo.cs` | 40-48 | direct | PlatformColorType is set by querying the native sk_colortype_get_default_8888(). On Windows/Linux/Android this returns Bgra8888; LibTiff.Net ReadRGBAImageOriented returns bytes in R,G,B,A order (Rgba8888). Using PlatformColorType on a Bgra8888 platform causes R and B channels to swap. |
| `binding/SkiaSharp/SKBitmap.cs` | 606-615 | direct | InstallPixels trusts the caller's SKImageInfo without validation — no conversion is performed. If SKColorType doesn't match the actual pixel layout in the pointer, pixels are silently misinterpreted. |
| `binding/SkiaSharp/Definitions.cs` | 36-44 | context | Rgba8888 and Bgra8888 are distinct enum values. Rgba8888 matches LibTiff's memory layout (R at lowest address). Bgra8888 is the typical platform default on Windows/Linux. |

### Workarounds

- Use SKColorType.Rgba8888 explicitly instead of SKImageInfo.PlatformColorType to match LibTiff.Net's RGBA memory layout
- Use SKAlphaType.Unpremul instead of SKAlphaType.Premul because ReadRGBAImageOriented returns straight (un-premultiplied) alpha

### Resolution Proposals

**Hypothesis:** Replace PlatformColorType with SKColorType.Rgba8888 and SKAlphaType.Premul with SKAlphaType.Unpremul to match LibTiff.Net's ReadRGBAImageOriented output format.

1. **Fix SKImageInfo to match LibTiff pixel format** — fix, confidence 0.88 (88%), cost/xs, validated=yes
   - Change SKColorType to Rgba8888 and SKAlphaType to Unpremul. LibTiff.Net's ReadRGBAImageOriented stores pixels as R,G,B,A bytes in memory (Rgba8888 layout) with straight alpha.

```csharp
// Fix: use Rgba8888 (matches LibTiff RGBA output) and Unpremul (LibTiff returns straight alpha)
var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
var ptr = GCHandle.Alloc(raster, GCHandleType.Pinned);
bitmap.InstallPixels(info, ptr.AddrOfPinnedObject(), info.RowBytes);
if (!tif.ReadRGBAImageOriented(width, height, raster, Orientation.TOPLEFT))
    throw new Exception($"Failed to read image {fileName}");
```

**Recommended proposal:** Fix SKImageInfo to match LibTiff pixel format

**Why:** Directly addresses root cause: color type mismatch and alpha type mismatch. One-line change. No SkiaSharp changes required.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.82 (82%) |
| Reason | SkiaSharp works correctly. The issue is a pixel format mismatch between LibTiff.Net's output (Rgba8888, straight alpha) and the SKImageInfo passed (PlatformColorType=Bgra8888, premultiplied alpha). The fix is a caller-side change to use the correct color type. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply question and area labels | labels=type/question, area/SkiaSharp |
| add-comment | high | 0.82 (82%) | Post explanation with corrected code | — |
| close-issue | medium | 0.75 (75%) | Close as not a SkiaSharp bug — caller-side pixel format issue | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report!

The issue is a pixel format mismatch between LibTiff.Net and SkiaSharp. LibTiff.Net's `ReadRGBAImageOriented` fills the `int[]` buffer with pixels in **R, G, B, A** byte order in memory — this corresponds to `SKColorType.Rgba8888`. However, `SKImageInfo.PlatformColorType` is typically `SKColorType.Bgra8888` on Windows and Linux, which expects **B, G, R, A** byte order. The swapped R and B channels explain the slight colour differences for normal images and the black output for monochrome X-Rays.

Additionally, `ReadRGBAImageOriented` returns **straight (un-premultiplied)** alpha, but your code specifies `SKAlphaType.Premul`, which causes SkiaSharp to treat the pixel values as if alpha has already been multiplied — further corrupting semi-transparent pixels.

Here is the corrected code:

```csharp
var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
var ptr = GCHandle.Alloc(raster, GCHandleType.Pinned);
bitmap.InstallPixels(info, ptr.AddrOfPinnedObject(), info.RowBytes);

if (!tif.ReadRGBAImageOriented(width, height, raster, Orientation.TOPLEFT))
    throw new Exception($"Failed to read image {fileName}");

using (SKImage image = SKImage.FromBitmap(bitmap))
{
    SKData data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
    // ...
}
```

Key changes:
- `SKColorType.Rgba8888` instead of `SKImageInfo.PlatformColorType` — matches LibTiff's memory layout
- `SKAlphaType.Unpremul` instead of `SKAlphaType.Premul` — LibTiff returns straight alpha
- Removed the `SKColorSpace.CreateSrgb()` (optional, but TIFF files don't always carry an sRGB profile so this avoids unexpected colour transforms)
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1950,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T20:04:27Z"
  },
  "summary": "Reporter asks how to preserve color fidelity when converting TIFF images to JPEG via LibTiff.Net and SkiaSharp; monochrome/X-Ray TIFFs appear black due to a pixel-format mismatch between LibTiff's RGBA output and the SKImageInfo color type used.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.85
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    }
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Open a monochrome/X-Ray TIFF using LibTiff.Net Tiff.ClientOpen",
        "Allocate int[] raster and call tif.ReadRGBAImageOriented with Orientation.TOPLEFT",
        "Create SKBitmap with SKImageInfo using PlatformColorType and SKAlphaType.Premul",
        "Call bitmap.InstallPixels with the pinned raster pointer",
        "Encode to JPEG with SKImage.FromBitmap -> image.Encode(SKEncodedImageFormat.Jpeg, 100)",
        "Observe resulting JPEG is all-black for monochrome/X-Ray sources"
      ],
      "environmentDetails": "No version info provided; uses LibTiff.Net + SkiaSharp",
      "codeSnippets": [
        "var info = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, SKColorSpace.CreateSrgb());\nbitmap.InstallPixels(info, ptr.AddrOfPinnedObject(), info.RowBytes, null, null);\ntif.ReadRGBAImageOriented(width, height, raster, Orientation.TOPLEFT);"
      ]
    }
  },
  "analysis": {
    "summary": "The reporter uses PlatformColorType (Bgra8888 on most platforms) for the SKImageInfo, but LibTiff.Net's ReadRGBAImageOriented fills the int[] buffer in RGBA memory order (R byte first). On little-endian hosts this means bytes are [R, G, B, A] = Rgba8888, not [B, G, R, A] = Bgra8888. The mismatch swaps the red and blue channels. Monochrome X-Ray images become black because high-value grayscale pixels with alpha=255 end up being interpreted as premultiplied with 0 alpha in the wrong channel position. Also, ReadRGBAImage always returns straight alpha (un-premultiplied), while the code specifies SKAlphaType.Premul, causing further corruption for any semi-transparent pixels.",
    "rationale": "No broken SkiaSharp behavior is described. The API works correctly — the caller passes the wrong SKColorType for the pixel data produced by LibTiff.Net. This is a typical usage/interop question. SkiaSharp's InstallPixels API trusts the caller's SKImageInfo description; PlatformColorType varies by OS and does not match LibTiff's fixed RGBA output format.",
    "keySignals": [
      {
        "text": "Images which have a white background and have some graphs convert OK, but the colours are slightly different",
        "source": "issue body",
        "interpretation": "Colors differ slightly for RGB images — classic R/B channel swap where non-equal R and B components show as tint shift"
      },
      {
        "text": "images that are kind of monochrome (e.g. pictures of X-Rays) are totally black when converted",
        "source": "issue body",
        "interpretation": "Monochrome images go fully black — likely because premultiplied alpha interpretation of straight-alpha data zeroes out channels"
      },
      {
        "text": "var info = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, SKColorSpace.CreateSrgb())",
        "source": "issue body code",
        "interpretation": "PlatformColorType is Bgra8888 on most platforms; LibTiff returns Rgba8888 memory layout — direct cause of color mismatch"
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKImageInfo.cs",
        "lines": "40-48",
        "finding": "PlatformColorType is set by querying the native sk_colortype_get_default_8888(). On Windows/Linux/Android this returns Bgra8888; LibTiff.Net ReadRGBAImageOriented returns bytes in R,G,B,A order (Rgba8888). Using PlatformColorType on a Bgra8888 platform causes R and B channels to swap.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "606-615",
        "finding": "InstallPixels trusts the caller's SKImageInfo without validation — no conversion is performed. If SKColorType doesn't match the actual pixel layout in the pointer, pixels are silently misinterpreted.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "36-44",
        "finding": "Rgba8888 and Bgra8888 are distinct enum values. Rgba8888 matches LibTiff's memory layout (R at lowest address). Bgra8888 is the typical platform default on Windows/Linux.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "Use SKColorType.Rgba8888 explicitly instead of SKImageInfo.PlatformColorType to match LibTiff.Net's RGBA memory layout",
      "Use SKAlphaType.Unpremul instead of SKAlphaType.Premul because ReadRGBAImageOriented returns straight (un-premultiplied) alpha"
    ],
    "resolution": {
      "hypothesis": "Replace PlatformColorType with SKColorType.Rgba8888 and SKAlphaType.Premul with SKAlphaType.Unpremul to match LibTiff.Net's ReadRGBAImageOriented output format.",
      "proposals": [
        {
          "title": "Fix SKImageInfo to match LibTiff pixel format",
          "description": "Change SKColorType to Rgba8888 and SKAlphaType to Unpremul. LibTiff.Net's ReadRGBAImageOriented stores pixels as R,G,B,A bytes in memory (Rgba8888 layout) with straight alpha.",
          "category": "fix",
          "codeSnippet": "// Fix: use Rgba8888 (matches LibTiff RGBA output) and Unpremul (LibTiff returns straight alpha)\nvar info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);\nvar ptr = GCHandle.Alloc(raster, GCHandleType.Pinned);\nbitmap.InstallPixels(info, ptr.AddrOfPinnedObject(), info.RowBytes);\nif (!tif.ReadRGBAImageOriented(width, height, raster, Orientation.TOPLEFT))\n    throw new Exception($\"Failed to read image {fileName}\");",
          "validated": "yes",
          "confidence": 0.88,
          "effort": "cost/xs"
        }
      ],
      "recommendedProposal": "Fix SKImageInfo to match LibTiff pixel format",
      "recommendedReason": "Directly addresses root cause: color type mismatch and alpha type mismatch. One-line change. No SkiaSharp changes required."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.82,
      "reason": "SkiaSharp works correctly. The issue is a pixel format mismatch between LibTiff.Net's output (Rgba8888, straight alpha) and the SKImageInfo passed (PlatformColorType=Bgra8888, premultiplied alpha). The fix is a caller-side change to use the correct color type.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and area labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post explanation with corrected code",
        "risk": "high",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report!\n\nThe issue is a pixel format mismatch between LibTiff.Net and SkiaSharp. LibTiff.Net's `ReadRGBAImageOriented` fills the `int[]` buffer with pixels in **R, G, B, A** byte order in memory — this corresponds to `SKColorType.Rgba8888`. However, `SKImageInfo.PlatformColorType` is typically `SKColorType.Bgra8888` on Windows and Linux, which expects **B, G, R, A** byte order. The swapped R and B channels explain the slight colour differences for normal images and the black output for monochrome X-Rays.\n\nAdditionally, `ReadRGBAImageOriented` returns **straight (un-premultiplied)** alpha, but your code specifies `SKAlphaType.Premul`, which causes SkiaSharp to treat the pixel values as if alpha has already been multiplied — further corrupting semi-transparent pixels.\n\nHere is the corrected code:\n\n```csharp\nvar info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);\nvar ptr = GCHandle.Alloc(raster, GCHandleType.Pinned);\nbitmap.InstallPixels(info, ptr.AddrOfPinnedObject(), info.RowBytes);\n\nif (!tif.ReadRGBAImageOriented(width, height, raster, Orientation.TOPLEFT))\n    throw new Exception($\"Failed to read image {fileName}\");\n\nusing (SKImage image = SKImage.FromBitmap(bitmap))\n{\n    SKData data = image.Encode(SKEncodedImageFormat.Jpeg, 100);\n    // ...\n}\n```\n\nKey changes:\n- `SKColorType.Rgba8888` instead of `SKImageInfo.PlatformColorType` — matches LibTiff's memory layout\n- `SKAlphaType.Unpremul` instead of `SKAlphaType.Premul` — LibTiff returns straight alpha\n- Removed the `SKColorSpace.CreateSrgb()` (optional, but TIFF files don't always carry an sRGB profile so this avoids unexpected colour transforms)"
      },
      {
        "type": "close-issue",
        "description": "Close as not a SkiaSharp bug — caller-side pixel format issue",
        "risk": "medium",
        "confidence": 0.75,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
