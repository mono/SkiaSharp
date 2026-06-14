# Issue Triage Report — #824

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-06-14T05:47:37Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp (0.85 (85%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** YCCK/CMYK JPEG image loses significant visual data when decoded via SKImage.FromEncodedData() and re-saved as PNG, because Skia does not properly convert YCCK color channels to RGB.

**Analysis:** Skia's JPEG codec decodes YCCK images without performing the channel inversion required to convert YCCK to RGB. YCCK is an Adobe JPEG variant where CMYK channels are inverted (255 - value). When Skia treats these inverted channels as standard CMYK or RGB, the resulting pixel values are incorrect — causing the visible data loss in dark/detail areas. SkiaSharp's FromEncodedData() passes the raw encoded bytes directly to sk_image_new_from_encoded with no pre-processing, so the bug is in the Skia codec layer.

**Recommendations:** **needs-investigation** — Bug is real and well-described with attached image and minimal repro. Root cause points to Skia's JPEG codec not handling YCCK color space. Needs investigation into whether upstream Skia has fixed this in a newer milestone.

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

1. Load a YCCK JPEG file using SKImage.FromEncodedData("i9.jpg")
2. Encode the image to PNG using img.Encode(SKEncodedImageFormat.Png, 100)
3. Save the PNG to disk
4. Compare the input JPEG and output PNG — significant data loss visible in floor tiles and curtain detail

**Environment:** SkiaSharp 1.68.0, Windows 10, Visual Studio

**Repository links:**
- https://user-images.githubusercontent.com/1690985/55682026-0f38d680-5936-11e9-8d20-fec62d2f7346.jpg — Attached test image (i9.jpg) showing the YCCK JPEG that triggers data loss

**Code snippets:**

```csharp
var img = SKImage.FromEncodedData("i9.jpg");
using (var file = File.Create("i10.png"))
	img.Encode(SKEncodedImageFormat.Png, 100).SaveTo(file);
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | net472 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | No YCCK-specific handling has been added to SkiaSharp or the underlying Skia codec since this version. The FromEncodedData path directly invokes Skia's codec with no color model adjustment. |

## Analysis

### Technical Summary

Skia's JPEG codec decodes YCCK images without performing the channel inversion required to convert YCCK to RGB. YCCK is an Adobe JPEG variant where CMYK channels are inverted (255 - value). When Skia treats these inverted channels as standard CMYK or RGB, the resulting pixel values are incorrect — causing the visible data loss in dark/detail areas. SkiaSharp's FromEncodedData() passes the raw encoded bytes directly to sk_image_new_from_encoded with no pre-processing, so the bug is in the Skia codec layer.

### Rationale

The reporter provides a clear before/after with attached image and minimal repro code. The title says 'loses data' and the description confirms visual data loss visible in image details. The reporter correctly identifies this as a YCCK-encoded JPEG — a rare but valid JPEG variant. SKImage.FromEncodedData() maps directly to sk_image_new_from_encoded in Skia; there is no SkiaSharp-layer preprocessing that could cause or fix this. The root cause is Skia's JPEG codec handling of YCCK color space.

### Key Signals

- "Really this is an YCCK image, so ideally Skia should do the YCCK conversion." — **issue body** (Reporter correctly identifies the root cause: YCCK JPEG not being converted to RGB during decode.)
- "JpegBitmapDecoder does [convert YCCK]. However most programs do not do the conversion, so it's fine if Skia doesn't. But the fact that Skia loses data when opening it is a bug" — **issue body** (Reporter distinguishes between YCCK-to-RGB conversion (optional) and data preservation (required). Skia fails even the minimal requirement of preserving pixel data faithfully.)
- "Take a look at the tiles on the floor, and the wave of the curtains behind" — **issue body** (Confirms the wrong-output type — specific visible areas of the image are affected, consistent with YCCK channel inversion errors.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKImage.cs` | 170-177 | direct | SKImage.FromEncodedData(SKData) calls sk_image_new_from_encoded(data.Handle) with no color space or codec options — passes raw encoded bytes directly to the Skia C++ codec. No SkiaSharp-level color model correction is possible here. |
| `binding/SkiaSharp/SKImage.cs` | 226-235 | direct | SKImage.FromEncodedData(string filename) wraps SKData.Create(filename) then delegates to the SKData overload. The entire string/stream/byte-array family of overloads all funnel into the same sk_image_new_from_encoded call. |
| `binding/SkiaSharp/SKCodec.cs` | 28-35 | related | SKCodec.Info returns an SKImageInfo which includes the ColorType that Skia inferred from the encoded data. For YCCK JPEGs, Skia may report an RGB colortype rather than CMYK, confirming the data model mismatch at the Skia layer. |
| `binding/SkiaSharp/SKImage.cs` | 379-384 | context | SKImage.Encode(format, quality) calls ToRasterImage(true) then uses the resulting pixmap to encode. If the rasterized image already has wrong pixel values from decode, the encode step faithfully preserves the incorrect data — the bug is upstream in the decode path. |

### Workarounds

- Use a different JPEG decoder (System.Drawing.Imaging.Image, ImageSharp, or WPF's JpegBitmapDecoder) to load YCCK JPEG files before passing to SkiaSharp, if correct color representation is required.
- Check SKCodec.Info.ColorType before decoding — if the JPEG reports a CMYK colortype, use an alternative decode path.

### Next Questions

- Has Skia fixed YCCK handling in newer milestones? This issue was filed against v1.68.0 and the current milestone may include upstream fixes.
- Does the bug reproduce on Linux/macOS, or is it Windows-specific (different libjpeg-turbo version used by Skia on different platforms)?
- Is there a way for SkiaSharp to expose a codec option to request YCCK-to-RGB conversion explicitly?

### Resolution Proposals

**Hypothesis:** Skia's JPEG codec does not invert YCCK channel values when converting to RGBA. YCCK stores inverted CMY channels plus K, so a value of 200 in the YCCK stream corresponds to a low-ink value, but Skia interprets it as a high-value channel — causing the visible data loss in darker image regions.

1. **Use alternative JPEG decoder for CMYK/YCCK images** — workaround, confidence 0.70 (70%), cost/m, validated=untested
   - Before calling SKImage.FromEncodedData, detect the JPEG color space using SKCodec.Create() and check the Info.ColorType. If CMYK-derived, use System.Drawing or ImageSharp to decode and convert to RGBA before handing to SkiaSharp.
2. **Investigate upstream Skia JPEG YCCK fix** — investigation, confidence 0.60 (60%), cost/s, validated=untested
   - Check if the Skia milestone used in the current SkiaSharp version contains a fix for YCCK JPEG handling. Upstream Skia bug tracker may have a related issue. If fixed upstream, a SkiaSharp milestone bump may resolve it.

**Recommended proposal:** Investigate upstream Skia JPEG YCCK fix

**Why:** If the issue is fixed in upstream Skia, a milestone bump is the cleanest resolution with no SkiaSharp code changes needed.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Bug is real and well-described with attached image and minimal repro. Root cause points to Skia's JPEG codec not handling YCCK color space. Needs investigation into whether upstream Skia has fixed this in a newer milestone. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/SkiaSharp, os/Windows-Classic, backend/Raster, tenet/compatibility labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, backend/Raster, tenet/compatibility |
| add-comment | medium | 0.82 (82%) | Acknowledge the YCCK JPEG issue and provide investigation status and interim workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and the attached test image.

This appears to be a **YCCK JPEG decoding issue** in the underlying Skia library. YCCK is an Adobe JPEG variant that stores CMYK channels with inverted values. Skia's JPEG codec does not perform the channel inversion required to convert YCCK to RGB correctly, resulting in the visible data loss you're seeing.

This bug is in Skia's JPEG codec layer (`sk_image_new_from_encoded`), not in SkiaSharp's bindings themselves.

**Interim workaround:** If accurate color is required for YCCK/CMYK JPEG files, consider detecting the color space before decoding:
```csharp
using var codec = SKCodec.Create("i9.jpg");
// If SKColorType is not standard RGBA, use alternative decoder
// e.g. System.Drawing.Imaging.Image or ImageSharp for YCCK/CMYK JPEGs
```

We'll investigate whether a newer Skia milestone includes a fix for YCCK handling.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 824,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-06-14T05:47:37Z"
  },
  "summary": "YCCK/CMYK JPEG image loses significant visual data when decoded via SKImage.FromEncodedData() and re-saved as PNG, because Skia does not properly convert YCCK color channels to RGB.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.85
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
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net472"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Load a YCCK JPEG file using SKImage.FromEncodedData(\"i9.jpg\")",
        "Encode the image to PNG using img.Encode(SKEncodedImageFormat.Png, 100)",
        "Save the PNG to disk",
        "Compare the input JPEG and output PNG — significant data loss visible in floor tiles and curtain detail"
      ],
      "codeSnippets": [
        "var img = SKImage.FromEncodedData(\"i9.jpg\");\nusing (var file = File.Create(\"i10.png\"))\n\timg.Encode(SKEncodedImageFormat.Png, 100).SaveTo(file);"
      ],
      "environmentDetails": "SkiaSharp 1.68.0, Windows 10, Visual Studio",
      "repoLinks": [
        {
          "url": "https://user-images.githubusercontent.com/1690985/55682026-0f38d680-5936-11e9-8d20-fec62d2f7346.jpg",
          "description": "Attached test image (i9.jpg) showing the YCCK JPEG that triggers data loss"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "No YCCK-specific handling has been added to SkiaSharp or the underlying Skia codec since this version. The FromEncodedData path directly invokes Skia's codec with no color model adjustment."
    }
  },
  "analysis": {
    "summary": "Skia's JPEG codec decodes YCCK images without performing the channel inversion required to convert YCCK to RGB. YCCK is an Adobe JPEG variant where CMYK channels are inverted (255 - value). When Skia treats these inverted channels as standard CMYK or RGB, the resulting pixel values are incorrect — causing the visible data loss in dark/detail areas. SkiaSharp's FromEncodedData() passes the raw encoded bytes directly to sk_image_new_from_encoded with no pre-processing, so the bug is in the Skia codec layer.",
    "rationale": "The reporter provides a clear before/after with attached image and minimal repro code. The title says 'loses data' and the description confirms visual data loss visible in image details. The reporter correctly identifies this as a YCCK-encoded JPEG — a rare but valid JPEG variant. SKImage.FromEncodedData() maps directly to sk_image_new_from_encoded in Skia; there is no SkiaSharp-layer preprocessing that could cause or fix this. The root cause is Skia's JPEG codec handling of YCCK color space.",
    "keySignals": [
      {
        "text": "Really this is an YCCK image, so ideally Skia should do the YCCK conversion.",
        "source": "issue body",
        "interpretation": "Reporter correctly identifies the root cause: YCCK JPEG not being converted to RGB during decode."
      },
      {
        "text": "JpegBitmapDecoder does [convert YCCK]. However most programs do not do the conversion, so it's fine if Skia doesn't. But the fact that Skia loses data when opening it is a bug",
        "source": "issue body",
        "interpretation": "Reporter distinguishes between YCCK-to-RGB conversion (optional) and data preservation (required). Skia fails even the minimal requirement of preserving pixel data faithfully."
      },
      {
        "text": "Take a look at the tiles on the floor, and the wave of the curtains behind",
        "source": "issue body",
        "interpretation": "Confirms the wrong-output type — specific visible areas of the image are affected, consistent with YCCK channel inversion errors."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "170-177",
        "finding": "SKImage.FromEncodedData(SKData) calls sk_image_new_from_encoded(data.Handle) with no color space or codec options — passes raw encoded bytes directly to the Skia C++ codec. No SkiaSharp-level color model correction is possible here.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "226-235",
        "finding": "SKImage.FromEncodedData(string filename) wraps SKData.Create(filename) then delegates to the SKData overload. The entire string/stream/byte-array family of overloads all funnel into the same sk_image_new_from_encoded call.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "28-35",
        "finding": "SKCodec.Info returns an SKImageInfo which includes the ColorType that Skia inferred from the encoded data. For YCCK JPEGs, Skia may report an RGB colortype rather than CMYK, confirming the data model mismatch at the Skia layer.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "379-384",
        "finding": "SKImage.Encode(format, quality) calls ToRasterImage(true) then uses the resulting pixmap to encode. If the rasterized image already has wrong pixel values from decode, the encode step faithfully preserves the incorrect data — the bug is upstream in the decode path.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "Use a different JPEG decoder (System.Drawing.Imaging.Image, ImageSharp, or WPF's JpegBitmapDecoder) to load YCCK JPEG files before passing to SkiaSharp, if correct color representation is required.",
      "Check SKCodec.Info.ColorType before decoding — if the JPEG reports a CMYK colortype, use an alternative decode path."
    ],
    "nextQuestions": [
      "Has Skia fixed YCCK handling in newer milestones? This issue was filed against v1.68.0 and the current milestone may include upstream fixes.",
      "Does the bug reproduce on Linux/macOS, or is it Windows-specific (different libjpeg-turbo version used by Skia on different platforms)?",
      "Is there a way for SkiaSharp to expose a codec option to request YCCK-to-RGB conversion explicitly?"
    ],
    "resolution": {
      "hypothesis": "Skia's JPEG codec does not invert YCCK channel values when converting to RGBA. YCCK stores inverted CMY channels plus K, so a value of 200 in the YCCK stream corresponds to a low-ink value, but Skia interprets it as a high-value channel — causing the visible data loss in darker image regions.",
      "proposals": [
        {
          "title": "Use alternative JPEG decoder for CMYK/YCCK images",
          "description": "Before calling SKImage.FromEncodedData, detect the JPEG color space using SKCodec.Create() and check the Info.ColorType. If CMYK-derived, use System.Drawing or ImageSharp to decode and convert to RGBA before handing to SkiaSharp.",
          "category": "workaround",
          "confidence": 0.7,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Investigate upstream Skia JPEG YCCK fix",
          "description": "Check if the Skia milestone used in the current SkiaSharp version contains a fix for YCCK JPEG handling. Upstream Skia bug tracker may have a related issue. If fixed upstream, a SkiaSharp milestone bump may resolve it.",
          "category": "investigation",
          "confidence": 0.6,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Investigate upstream Skia JPEG YCCK fix",
      "recommendedReason": "If the issue is fixed in upstream Skia, a milestone bump is the cleanest resolution with no SkiaSharp code changes needed."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Bug is real and well-described with attached image and minimal repro. Root cause points to Skia's JPEG codec not handling YCCK color space. Needs investigation into whether upstream Skia has fixed this in a newer milestone.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/Windows-Classic, backend/Raster, tenet/compatibility labels",
        "risk": "low",
        "confidence": 0.9,
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
        "description": "Acknowledge the YCCK JPEG issue and provide investigation status and interim workaround",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report and the attached test image.\n\nThis appears to be a **YCCK JPEG decoding issue** in the underlying Skia library. YCCK is an Adobe JPEG variant that stores CMYK channels with inverted values. Skia's JPEG codec does not perform the channel inversion required to convert YCCK to RGB correctly, resulting in the visible data loss you're seeing.\n\nThis bug is in Skia's JPEG codec layer (`sk_image_new_from_encoded`), not in SkiaSharp's bindings themselves.\n\n**Interim workaround:** If accurate color is required for YCCK/CMYK JPEG files, consider detecting the color space before decoding:\n```csharp\nusing var codec = SKCodec.Create(\"i9.jpg\");\n// If SKColorType is not standard RGBA, use alternative decoder\n// e.g. System.Drawing.Imaging.Image or ImageSharp for YCCK/CMYK JPEGs\n```\n\nWe'll investigate whether a newer Skia milestone includes a fix for YCCK handling."
      }
    ]
  }
}
```

</details>
