# Issue Triage Report — #1238

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T20:23:25Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp (0.82 (82%)) |
| Suggested action | needs-investigation (0.78 (78%)) |

**Issue Summary:** PNG files encoded from a linear color space (RgbaF16/sRGB-linear) appear too dark in most Windows image viewers (IrfanView, Paint, Photo Viewer, Paint.NET) after a Skia version upgrade, while modern browsers render them correctly.

**Analysis:** The updated Skia version started embedding a full ICC profile for the linear sRGB color space in PNG output. Windows image viewers using WIC cannot parse this embedded linear ICC profile (reporting 'The specified color profile is invalid'), so they ignore it and render the linear data without gamma correction, making colors appear too dark. Modern browsers correctly apply the color space transform using the embedded profile.

**Recommendations:** **needs-investigation** — Confirmed regression with complete repro code. Workaround exists but no proper fix has been applied. Needs verification in current SkiaSharp 3.x and investigation into whether ICC profile embedding can be configured.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/Raster |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create an SKImageInfo with RgbaF16 color type and SKColorSpace.CreateSrgbLinear()
2. Create a bitmap and draw to it with a solid color (e.g. purple)
3. Encode to PNG using bitmap.PeekPixels().Encode(new SKPngEncoderOptions())
4. Open the PNG in Windows image viewers (IrfanView, Paint, Photo Viewer, Paint.NET)

**Environment:** Windows Classic, SkiaSharp dev/update-skia branch (commit 1590d7e1e815), regression from 1.68.2-preview.60

**Repository links:**
- https://groups.google.com/forum/#!topic/skia-discuss/b8JtsaOw7_o — Upstream Skia mailing list discussion about the color profile change

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | HRESULT: [0x800707DB], Message: The specified color profile is invalid. |
| Repro quality | complete |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.2-preview.60, 1590d7e1e815 (dev/update-skia) |
| Worked in | 1.68.2-preview.60 |
| Broke in | dev/update-skia branch (commit 1590d7e1e815) |
| Current relevance | likely |
| Relevance reason | The PNG encoder still embeds ICC profiles for linear color spaces in current Skia versions; WIC's limited ICC support for linear color spaces has not changed. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.90 (90%) |
| Reason | Reporter explicitly confirms the same code produced correct output in 1.68.2-preview.60 but produces too-dark PNG in the updated Skia branch. |
| Worked in version | 1.68.2-preview.60 |
| Broke in version | dev/update-skia (commit 1590d7e1e815) |

## Analysis

### Technical Summary

The updated Skia version started embedding a full ICC profile for the linear sRGB color space in PNG output. Windows image viewers using WIC cannot parse this embedded linear ICC profile (reporting 'The specified color profile is invalid'), so they ignore it and render the linear data without gamma correction, making colors appear too dark. Modern browsers correctly apply the color space transform using the embedded profile.

### Rationale

This is a compatibility regression caused by an upstream Skia behavior change in PNG encoding. New Skia embeds a correct-but-WIC-incompatible ICC profile for linear sRGB color spaces. The reporter and maintainer investigated this together and confirmed WIC throws an error on the embedded profile. A workaround exists (convert to sRGB first), and the issue was posted to the Skia mailing list, suggesting this is upstream-driven.

### Key Signals

- "HRESULT: [0x800707DB], Message: The specified color profile is invalid." — **comment #617715987** (Windows WIC fails to parse the embedded ICC profile for linear sRGB, confirming the root cause. WIC ignores the profile and renders linear data without gamma correction.)
- "copying the PNG to a F16 sRGB bitmap and then saving also works" — **comment #616081458** (Converting to sRGB color space before encoding avoids embedding an incompatible ICC profile. Reporter confirmed the workaround works.)
- "Note that the colors are correctly displayed in modern browsers" — **issue body** (The embedded ICC profile is valid per the PNG spec. Modern browsers correctly apply the color space transform, confirming the PNG data is correct — only WIC-based viewers fail.)
- "JPEG and WebP seem to work" — **comment #616081458** (Other image formats are not affected, isolating the issue to PNG ICC profile embedding.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/Definitions.cs` | 521-543 | direct | SKPngEncoderOptions has fICCProfile and fICCProfileDescription fields. The Default constructor leaves these unset (default), meaning Skia auto-embeds the ICC profile based on the pixmap's color space. There is no option to suppress ICC profile embedding. |
| `binding/SkiaSharp/SKPixmap.cs` | 294-312 | direct | Encode(SKPngEncoderOptions) passes options directly to the C API sk_pngencoder_encode. No color conversion occurs at the SkiaSharp layer before encoding. |
| `binding/SkiaSharp/SKImageInfo.cs` | 151-157 | related | SKImageInfo.WithColorSpace(SKColorSpace) exists and can be used to create a new image info with sRGB color space, enabling the workaround of copying pixel data to sRGB before encoding. |
| `binding/SkiaSharp/SKColorSpace.cs` | 68-74 | context | SKColorSpace.CreateSrgb() and CreateSrgbLinear() are available as static methods returning cached instances. The fix/workaround is to target sRGB for PNG output. |

**Error fingerprint:** `PNG/linear-ICC/WIC-invalid-profile`

### Workarounds

- Convert the bitmap to sRGB color space before encoding: create a new SKBitmap with imageInfo.WithColorSpace(SKColorSpace.CreateSrgb()) and draw the original into it, then encode the sRGB bitmap to PNG.

### Next Questions

- Is this issue still reproducible in current SkiaSharp 3.x releases (not just dev/update-skia from 2020)?
- Did the Skia mailing list discussion result in any upstream fix or recommended configuration?
- Can SKPngEncoderOptions be extended to allow suppressing ICC profile embedding for compatibility?

### Resolution Proposals

**Hypothesis:** New Skia embeds a linear sRGB ICC profile in PNG output; Windows WIC fails to parse it, treats image as uncorrected linear data, and displays colors too dark.

1. **Convert to sRGB before encoding** — workaround, confidence 0.90 (90%), cost/s, validated=yes
   - Copy the pixmap into a new bitmap with sRGB color space before encoding to PNG. Skia performs the correct linear-to-sRGB gamma conversion. The resulting PNG will use a standard sRGB profile understood by all viewers.

```csharp
// Convert linear bitmap to sRGB before PNG encoding
var srgbInfo = imageInfo.WithColorSpace(SKColorSpace.CreateSrgb());
using var srgbBitmap = new SKBitmap(srgbInfo);
using var srgbCanvas = new SKCanvas(srgbBitmap);
srgbCanvas.DrawBitmap(bitmap, 0, 0);
using var data = srgbBitmap.PeekPixels().Encode(new SKPngEncoderOptions());
using var stream = File.Create("output.png");
data.SaveTo(stream);
```
2. **Investigate adding ICC profile suppression option to SKPngEncoderOptions** — investigation, confidence 0.60 (60%), cost/m, validated=untested
   - Investigate whether Skia's C API allows disabling ICC profile embedding, and expose this as an option in SKPngEncoderOptions. This would allow callers to opt out of embedding profiles for compatibility.

**Recommended proposal:** Convert to sRGB before encoding

**Why:** Reporter confirmed this approach works. Converting to sRGB is the correct approach for images destined for wide display compatibility. Requires minimal code change.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.78 (78%) |
| Reason | Confirmed regression with complete repro code. Workaround exists but no proper fix has been applied. Needs verification in current SkiaSharp 3.x and investigation into whether ICC profile embedding can be configured. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Apply type/bug, area/SkiaSharp, os/Windows-Classic, backend/Raster, tenet/compatibility labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, backend/Raster, tenet/compatibility |
| add-comment | medium | 0.80 (80%) | Post analysis and confirmed workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the thorough investigation!

The root cause is that the updated Skia now embeds a full ICC profile for the linear sRGB color space in PNG output. Windows image viewers using WIC cannot parse this ICC profile (it reports `The specified color profile is invalid`), so they ignore it and render the linear pixel data without gamma correction — which is why images appear too dark. Modern browsers (and GIMP) handle it correctly.

**Workaround** — convert to sRGB color space before encoding to PNG:

```csharp
var srgbInfo = imageInfo.WithColorSpace(SKColorSpace.CreateSrgb());
using var srgbBitmap = new SKBitmap(srgbInfo);
using var srgbCanvas = new SKCanvas(srgbBitmap);
srgbCanvas.DrawBitmap(bitmap, 0, 0);
using var data = srgbBitmap.PeekPixels().Encode(new SKPngEncoderOptions());
using var stream = File.Create("output.png");
data.SaveTo(stream);
```

This allows Skia to apply the correct linear→sRGB gamma conversion, and the resulting PNG will use the standard sRGB profile that all viewers understand. We'll investigate whether we can add an option to suppress ICC profile embedding in `SKPngEncoderOptions` for cases like this.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1238,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T20:23:25Z"
  },
  "summary": "PNG files encoded from a linear color space (RgbaF16/sRGB-linear) appear too dark in most Windows image viewers (IrfanView, Paint, Photo Viewer, Paint.NET) after a Skia version upgrade, while modern browsers render them correctly.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.82
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/Raster"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "HRESULT: [0x800707DB], Message: The specified color profile is invalid.",
      "reproQuality": "complete"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKImageInfo with RgbaF16 color type and SKColorSpace.CreateSrgbLinear()",
        "Create a bitmap and draw to it with a solid color (e.g. purple)",
        "Encode to PNG using bitmap.PeekPixels().Encode(new SKPngEncoderOptions())",
        "Open the PNG in Windows image viewers (IrfanView, Paint, Photo Viewer, Paint.NET)"
      ],
      "environmentDetails": "Windows Classic, SkiaSharp dev/update-skia branch (commit 1590d7e1e815), regression from 1.68.2-preview.60",
      "repoLinks": [
        {
          "url": "https://groups.google.com/forum/#!topic/skia-discuss/b8JtsaOw7_o",
          "description": "Upstream Skia mailing list discussion about the color profile change"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.2-preview.60",
        "1590d7e1e815 (dev/update-skia)"
      ],
      "workedIn": "1.68.2-preview.60",
      "brokeIn": "dev/update-skia branch (commit 1590d7e1e815)",
      "currentRelevance": "likely",
      "relevanceReason": "The PNG encoder still embeds ICC profiles for linear color spaces in current Skia versions; WIC's limited ICC support for linear color spaces has not changed."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.9,
      "reason": "Reporter explicitly confirms the same code produced correct output in 1.68.2-preview.60 but produces too-dark PNG in the updated Skia branch.",
      "workedInVersion": "1.68.2-preview.60",
      "brokeInVersion": "dev/update-skia (commit 1590d7e1e815)"
    }
  },
  "analysis": {
    "summary": "The updated Skia version started embedding a full ICC profile for the linear sRGB color space in PNG output. Windows image viewers using WIC cannot parse this embedded linear ICC profile (reporting 'The specified color profile is invalid'), so they ignore it and render the linear data without gamma correction, making colors appear too dark. Modern browsers correctly apply the color space transform using the embedded profile.",
    "rationale": "This is a compatibility regression caused by an upstream Skia behavior change in PNG encoding. New Skia embeds a correct-but-WIC-incompatible ICC profile for linear sRGB color spaces. The reporter and maintainer investigated this together and confirmed WIC throws an error on the embedded profile. A workaround exists (convert to sRGB first), and the issue was posted to the Skia mailing list, suggesting this is upstream-driven.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "521-543",
        "finding": "SKPngEncoderOptions has fICCProfile and fICCProfileDescription fields. The Default constructor leaves these unset (default), meaning Skia auto-embeds the ICC profile based on the pixmap's color space. There is no option to suppress ICC profile embedding.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPixmap.cs",
        "lines": "294-312",
        "finding": "Encode(SKPngEncoderOptions) passes options directly to the C API sk_pngencoder_encode. No color conversion occurs at the SkiaSharp layer before encoding.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImageInfo.cs",
        "lines": "151-157",
        "finding": "SKImageInfo.WithColorSpace(SKColorSpace) exists and can be used to create a new image info with sRGB color space, enabling the workaround of copying pixel data to sRGB before encoding.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKColorSpace.cs",
        "lines": "68-74",
        "finding": "SKColorSpace.CreateSrgb() and CreateSrgbLinear() are available as static methods returning cached instances. The fix/workaround is to target sRGB for PNG output.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "HRESULT: [0x800707DB], Message: The specified color profile is invalid.",
        "source": "comment #617715987",
        "interpretation": "Windows WIC fails to parse the embedded ICC profile for linear sRGB, confirming the root cause. WIC ignores the profile and renders linear data without gamma correction."
      },
      {
        "text": "copying the PNG to a F16 sRGB bitmap and then saving also works",
        "source": "comment #616081458",
        "interpretation": "Converting to sRGB color space before encoding avoids embedding an incompatible ICC profile. Reporter confirmed the workaround works."
      },
      {
        "text": "Note that the colors are correctly displayed in modern browsers",
        "source": "issue body",
        "interpretation": "The embedded ICC profile is valid per the PNG spec. Modern browsers correctly apply the color space transform, confirming the PNG data is correct — only WIC-based viewers fail."
      },
      {
        "text": "JPEG and WebP seem to work",
        "source": "comment #616081458",
        "interpretation": "Other image formats are not affected, isolating the issue to PNG ICC profile embedding."
      }
    ],
    "workarounds": [
      "Convert the bitmap to sRGB color space before encoding: create a new SKBitmap with imageInfo.WithColorSpace(SKColorSpace.CreateSrgb()) and draw the original into it, then encode the sRGB bitmap to PNG."
    ],
    "nextQuestions": [
      "Is this issue still reproducible in current SkiaSharp 3.x releases (not just dev/update-skia from 2020)?",
      "Did the Skia mailing list discussion result in any upstream fix or recommended configuration?",
      "Can SKPngEncoderOptions be extended to allow suppressing ICC profile embedding for compatibility?"
    ],
    "errorFingerprint": "PNG/linear-ICC/WIC-invalid-profile",
    "resolution": {
      "hypothesis": "New Skia embeds a linear sRGB ICC profile in PNG output; Windows WIC fails to parse it, treats image as uncorrected linear data, and displays colors too dark.",
      "proposals": [
        {
          "title": "Convert to sRGB before encoding",
          "description": "Copy the pixmap into a new bitmap with sRGB color space before encoding to PNG. Skia performs the correct linear-to-sRGB gamma conversion. The resulting PNG will use a standard sRGB profile understood by all viewers.",
          "category": "workaround",
          "codeSnippet": "// Convert linear bitmap to sRGB before PNG encoding\nvar srgbInfo = imageInfo.WithColorSpace(SKColorSpace.CreateSrgb());\nusing var srgbBitmap = new SKBitmap(srgbInfo);\nusing var srgbCanvas = new SKCanvas(srgbBitmap);\nsrgbCanvas.DrawBitmap(bitmap, 0, 0);\nusing var data = srgbBitmap.PeekPixels().Encode(new SKPngEncoderOptions());\nusing var stream = File.Create(\"output.png\");\ndata.SaveTo(stream);",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "yes"
        },
        {
          "title": "Investigate adding ICC profile suppression option to SKPngEncoderOptions",
          "description": "Investigate whether Skia's C API allows disabling ICC profile embedding, and expose this as an option in SKPngEncoderOptions. This would allow callers to opt out of embedding profiles for compatibility.",
          "category": "investigation",
          "confidence": 0.6,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Convert to sRGB before encoding",
      "recommendedReason": "Reporter confirmed this approach works. Converting to sRGB is the correct approach for images destined for wide display compatibility. Requires minimal code change."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.78,
      "reason": "Confirmed regression with complete repro code. Workaround exists but no proper fix has been applied. Needs verification in current SkiaSharp 3.x and investigation into whether ICC profile embedding can be configured.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/Windows-Classic, backend/Raster, tenet/compatibility labels",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/Raster",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis and confirmed workaround",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the thorough investigation!\n\nThe root cause is that the updated Skia now embeds a full ICC profile for the linear sRGB color space in PNG output. Windows image viewers using WIC cannot parse this ICC profile (it reports `The specified color profile is invalid`), so they ignore it and render the linear pixel data without gamma correction — which is why images appear too dark. Modern browsers (and GIMP) handle it correctly.\n\n**Workaround** — convert to sRGB color space before encoding to PNG:\n\n```csharp\nvar srgbInfo = imageInfo.WithColorSpace(SKColorSpace.CreateSrgb());\nusing var srgbBitmap = new SKBitmap(srgbInfo);\nusing var srgbCanvas = new SKCanvas(srgbBitmap);\nsrgbCanvas.DrawBitmap(bitmap, 0, 0);\nusing var data = srgbBitmap.PeekPixels().Encode(new SKPngEncoderOptions());\nusing var stream = File.Create(\"output.png\");\ndata.SaveTo(stream);\n```\n\nThis allows Skia to apply the correct linear→sRGB gamma conversion, and the resulting PNG will use the standard sRGB profile that all viewers understand. We'll investigate whether we can add an option to suppress ICC profile embedding in `SKPngEncoderOptions` for cases like this."
      }
    ]
  }
}
```

</details>
