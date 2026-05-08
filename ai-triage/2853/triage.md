# Issue Triage Report — #2853

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T15:10:00Z |
| Type | type/bug (0.80 (80%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-external (0.82 (82%)) |

**Issue Summary:** Decoding TIFF files with SKImage.FromEncodedData or SKBitmap.Decode returns null or throws ArgumentNullException because Skia has no built-in TIFF codec.

**Analysis:** Skia does not include a TIFF decoder — TIFF is not in the SKEncodedImageFormat enum — so SKCodec.Create() returns null for .tif files, causing SKImage.FromEncodedData to silently return null and causing an ArgumentNullException if users explicitly pass the null codec to SKBitmap.Decode(SKCodec). The behavior is consistent with the underlying Skia limitation, but the poor error experience (silent null vs unhandled exception) is a usability problem.

**Recommendations:** **close-as-external** — TIFF support is absent from Skia's codec list (confirmed in SKEncodedImageFormat enum). This is an upstream Skia limitation, not a SkiaSharp wrapping bug. A workaround exists via third-party TIFF libraries.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Obtain a large .tif file
2. Call SKImage.FromEncodedData(stream/bytes/filename) or SKBitmap.Decode(stream/bytes/filename) with the file
3. Observe null return or ArgumentNullException

**Environment:** Windows 10.0.19045 / Windows 11, SkiaSharp 2.88.3; also confirmed on Debian Linux with v3.119.2

**Repository links:**
- https://github.com/mono/SkiaSharp/files/15253367/C.7.142.18-0002.zip — Sample TIFF file that fails to decode
- https://github.com/user-attachments/files/25918796/test.tiff — Second commenter's TIFF file that also fails on Linux

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | exception |
| Error message | Value cannot be null. (Parameter 'codec') at SkiaSharp.SKBitmap.Decode(SKCodec codec) |
| Repro quality | partial |
| Target frameworks | net8.0-windows |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3, 3.119.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | TIFF is absent from the SKEncodedImageFormat enum in the current generated bindings, confirming Skia still lacks a TIFF codec. |

## Analysis

### Technical Summary

Skia does not include a TIFF decoder — TIFF is not in the SKEncodedImageFormat enum — so SKCodec.Create() returns null for .tif files, causing SKImage.FromEncodedData to silently return null and causing an ArgumentNullException if users explicitly pass the null codec to SKBitmap.Decode(SKCodec). The behavior is consistent with the underlying Skia limitation, but the poor error experience (silent null vs unhandled exception) is a usability problem.

### Rationale

The issue is classified as type/bug because the ArgumentNullException crash from SKBitmap.Decode(SKCodec) with a null codec (obtained from SKCodec.Create on a TIFF file) is unexpected for callers who reasonably expect null to propagate gracefully. The root cause is that Skia itself has no TIFF codec (TIFF is absent from the SKEncodedImageFormat enum). The null-return path from SKBitmap.Decode(Stream) is correct behavior, but this limitation is undocumented and the exception path is confusing. Reproduced on multiple platforms and SkiaSharp versions.

### Key Signals

- "they all return null for this image or crash with an ArgumentNullException `Value cannot be null. (Parameter 'codec')`" — **issue body** (Two separate failure modes: null return (when decode path short-circuits on null codec) and exception (when user passes null codec to the overload that null-checks).)
- "I have the same error in v3.119.2, but I use Debian OS" — **comment by Rombersoft** (Issue persists in the latest major version (3.119.2) and is cross-platform, ruling out Windows-only or version-specific regression.)
- "TIFF is absent from the SKEncodedImageFormat enum (enum values 0-13 end with Jpegxl)" — **binding/SkiaSharp/SkiaApi.generated.cs** (Skia has no built-in TIFF codec. All decode entry points that rely on SKCodec.Create will fail silently for TIFF input.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaApi.generated.cs` | 21029-21058 | direct | SKEncodedImageFormat enum lists Bmp, Gif, Ico, Jpeg, Png, Wbmp, Webp, Pkm, Ktx, Astc, Dng, Heif, Avif, Jpegxl — TIFF is absent. This confirms Skia has no TIFF codec. |
| `binding/SkiaSharp/SKBitmap.cs` | 461-471 | direct | SKBitmap.Decode(Stream) calls SKCodec.Create(stream); if codec is null, returns null. This is the correct null-propagation path for unsupported formats. |
| `binding/SkiaSharp/SKBitmap.cs` | 434-444 | direct | SKBitmap.Decode(SKCodec codec) throws ArgumentNullException when codec is null. If a caller manually calls SKCodec.Create on a TIFF file and passes the resulting null to this overload, it throws instead of returning null. |
| `binding/SkiaSharp/SKImage.cs` | 166-173 | related | SKImage.FromEncodedData(SKData) calls sk_image_new_from_encoded via P/Invoke; returns null (GetObject(IntPtr.Zero)) when Skia cannot decode the input format. |

### Workarounds

- Use a third-party .NET TIFF library (e.g., ImageSharp, BitMiracle.LibTiff.NET) to decode the TIFF to a raw pixel buffer, then create an SKBitmap using SKBitmap.InstallPixels or SKBitmap.FromImage.
- Convert .tif files to PNG or JPEG before passing them to SkiaSharp.

### Next Questions

- Is this a pure SkiaSharp limitation or could Skia optionally link libtiff in its codec list?
- Should the decode APIs return a more informative error / throw a meaningful exception rather than returning null?

### Resolution Proposals

**Hypothesis:** Skia deliberately excludes TIFF from its built-in codecs. The null return is expected, but the silent failure and confusing exception path are a usability issue.

1. **Use a third-party TIFF decoder to get raw pixels** — workaround, confidence 0.90 (90%), cost/s, validated=untested
   - Decode the TIFF via ImageSharp or LibTiff.NET, extract the pixel buffer, and wrap it in an SKBitmap.
2. **Convert TIFF to PNG/JPEG before decoding** — alternative, confidence 0.95 (95%), cost/xs, validated=untested
   - Pre-convert the .tif file to a supported format using any available image tool.

**Recommended proposal:** Use a third-party TIFF decoder to get raw pixels

**Why:** Enables in-process TIFF loading without file conversion; best solution for users needing to load TIFF programmatically.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.82 (82%) |
| Reason | TIFF support is absent from Skia's codec list (confirmed in SKEncodedImageFormat enum). This is an upstream Skia limitation, not a SkiaSharp wrapping bug. A workaround exists via third-party TIFF libraries. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, SkiaSharp core, compatibility tenet labels | labels=type/bug, area/SkiaSharp, tenet/compatibility |
| add-comment | medium | 0.85 (85%) | Explain TIFF is not supported by Skia and provide workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the report! Unfortunately, TIFF is not a format supported by the underlying Skia graphics library — it is not included in Skia's built-in codec list. As a result, `SKCodec.Create` returns `null` for `.tif` inputs, causing `FromEncodedData` and `SKBitmap.Decode` to return `null` as well.

**Workaround:** You can decode the TIFF using a .NET TIFF library (e.g., [BitMiracle.LibTiff.NET](https://bitmiracle.com/libtiff/) or [ImageSharp](https://sixlabors.com/products/imagesharp/)) to extract raw pixel data, then construct an `SKBitmap` from that buffer. For example:

```csharp
// Using ImageSharp as an example
using var image = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(tifPath);
var bitmap = new SKBitmap(image.Width, image.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
using var pixmap = bitmap.PeekPixels();
image.CopyPixelDataTo(MemoryMarshal.Cast<byte, SixLabors.ImageSharp.PixelFormats.Rgba32>(
    new Span<byte>((void*)pixmap.GetPixels(), pixmap.RowBytes * pixmap.Height)));
```

Alternatively, convert your `.tif` files to PNG or JPEG before loading them with SkiaSharp.

This is an upstream Skia limitation, so adding TIFF support would require changes to the Skia engine itself.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2853,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T15:10:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Decoding TIFF files with SKImage.FromEncodedData or SKBitmap.Decode returns null or throws ArgumentNullException because Skia has no built-in TIFF codec.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.8
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "Value cannot be null. (Parameter 'codec') at SkiaSharp.SKBitmap.Decode(SKCodec codec)",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0-windows"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Obtain a large .tif file",
        "Call SKImage.FromEncodedData(stream/bytes/filename) or SKBitmap.Decode(stream/bytes/filename) with the file",
        "Observe null return or ArgumentNullException"
      ],
      "environmentDetails": "Windows 10.0.19045 / Windows 11, SkiaSharp 2.88.3; also confirmed on Debian Linux with v3.119.2",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/15253367/C.7.142.18-0002.zip",
          "description": "Sample TIFF file that fails to decode"
        },
        {
          "url": "https://github.com/user-attachments/files/25918796/test.tiff",
          "description": "Second commenter's TIFF file that also fails on Linux"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3",
        "3.119.2"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "TIFF is absent from the SKEncodedImageFormat enum in the current generated bindings, confirming Skia still lacks a TIFF codec."
    }
  },
  "analysis": {
    "summary": "Skia does not include a TIFF decoder — TIFF is not in the SKEncodedImageFormat enum — so SKCodec.Create() returns null for .tif files, causing SKImage.FromEncodedData to silently return null and causing an ArgumentNullException if users explicitly pass the null codec to SKBitmap.Decode(SKCodec). The behavior is consistent with the underlying Skia limitation, but the poor error experience (silent null vs unhandled exception) is a usability problem.",
    "rationale": "The issue is classified as type/bug because the ArgumentNullException crash from SKBitmap.Decode(SKCodec) with a null codec (obtained from SKCodec.Create on a TIFF file) is unexpected for callers who reasonably expect null to propagate gracefully. The root cause is that Skia itself has no TIFF codec (TIFF is absent from the SKEncodedImageFormat enum). The null-return path from SKBitmap.Decode(Stream) is correct behavior, but this limitation is undocumented and the exception path is confusing. Reproduced on multiple platforms and SkiaSharp versions.",
    "keySignals": [
      {
        "text": "they all return null for this image or crash with an ArgumentNullException `Value cannot be null. (Parameter 'codec')`",
        "source": "issue body",
        "interpretation": "Two separate failure modes: null return (when decode path short-circuits on null codec) and exception (when user passes null codec to the overload that null-checks)."
      },
      {
        "text": "I have the same error in v3.119.2, but I use Debian OS",
        "source": "comment by Rombersoft",
        "interpretation": "Issue persists in the latest major version (3.119.2) and is cross-platform, ruling out Windows-only or version-specific regression."
      },
      {
        "text": "TIFF is absent from the SKEncodedImageFormat enum (enum values 0-13 end with Jpegxl)",
        "source": "binding/SkiaSharp/SkiaApi.generated.cs",
        "interpretation": "Skia has no built-in TIFF codec. All decode entry points that rely on SKCodec.Create will fail silently for TIFF input."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "21029-21058",
        "finding": "SKEncodedImageFormat enum lists Bmp, Gif, Ico, Jpeg, Png, Wbmp, Webp, Pkm, Ktx, Astc, Dng, Heif, Avif, Jpegxl — TIFF is absent. This confirms Skia has no TIFF codec.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "461-471",
        "finding": "SKBitmap.Decode(Stream) calls SKCodec.Create(stream); if codec is null, returns null. This is the correct null-propagation path for unsupported formats.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "434-444",
        "finding": "SKBitmap.Decode(SKCodec codec) throws ArgumentNullException when codec is null. If a caller manually calls SKCodec.Create on a TIFF file and passes the resulting null to this overload, it throws instead of returning null.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "166-173",
        "finding": "SKImage.FromEncodedData(SKData) calls sk_image_new_from_encoded via P/Invoke; returns null (GetObject(IntPtr.Zero)) when Skia cannot decode the input format.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use a third-party .NET TIFF library (e.g., ImageSharp, BitMiracle.LibTiff.NET) to decode the TIFF to a raw pixel buffer, then create an SKBitmap using SKBitmap.InstallPixels or SKBitmap.FromImage.",
      "Convert .tif files to PNG or JPEG before passing them to SkiaSharp."
    ],
    "nextQuestions": [
      "Is this a pure SkiaSharp limitation or could Skia optionally link libtiff in its codec list?",
      "Should the decode APIs return a more informative error / throw a meaningful exception rather than returning null?"
    ],
    "resolution": {
      "hypothesis": "Skia deliberately excludes TIFF from its built-in codecs. The null return is expected, but the silent failure and confusing exception path are a usability issue.",
      "proposals": [
        {
          "title": "Use a third-party TIFF decoder to get raw pixels",
          "description": "Decode the TIFF via ImageSharp or LibTiff.NET, extract the pixel buffer, and wrap it in an SKBitmap.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Convert TIFF to PNG/JPEG before decoding",
          "description": "Pre-convert the .tif file to a supported format using any available image tool.",
          "category": "alternative",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use a third-party TIFF decoder to get raw pixels",
      "recommendedReason": "Enables in-process TIFF loading without file conversion; best solution for users needing to load TIFF programmatically."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.82,
      "reason": "TIFF support is absent from Skia's codec list (confirmed in SKEncodedImageFormat enum). This is an upstream Skia limitation, not a SkiaSharp wrapping bug. A workaround exists via third-party TIFF libraries.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, SkiaSharp core, compatibility tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain TIFF is not supported by Skia and provide workaround",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thank you for the report! Unfortunately, TIFF is not a format supported by the underlying Skia graphics library — it is not included in Skia's built-in codec list. As a result, `SKCodec.Create` returns `null` for `.tif` inputs, causing `FromEncodedData` and `SKBitmap.Decode` to return `null` as well.\n\n**Workaround:** You can decode the TIFF using a .NET TIFF library (e.g., [BitMiracle.LibTiff.NET](https://bitmiracle.com/libtiff/) or [ImageSharp](https://sixlabors.com/products/imagesharp/)) to extract raw pixel data, then construct an `SKBitmap` from that buffer. For example:\n\n```csharp\n// Using ImageSharp as an example\nusing var image = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(tifPath);\nvar bitmap = new SKBitmap(image.Width, image.Height, SKColorType.Rgba8888, SKAlphaType.Premul);\nusing var pixmap = bitmap.PeekPixels();\nimage.CopyPixelDataTo(MemoryMarshal.Cast<byte, SixLabors.ImageSharp.PixelFormats.Rgba32>(\n    new Span<byte>((void*)pixmap.GetPixels(), pixmap.RowBytes * pixmap.Height)));\n```\n\nAlternatively, convert your `.tif` files to PNG or JPEG before loading them with SkiaSharp.\n\nThis is an upstream Skia limitation, so adding TIFF support would require changes to the Skia engine itself."
      }
    ]
  }
}
```

</details>
