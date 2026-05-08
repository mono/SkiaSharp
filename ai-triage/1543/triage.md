# Issue Triage Report — #1543

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T05:46:00Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp (0.88 (88%)) |
| Suggested action | needs-investigation (0.87 (87%)) |

**Issue Summary:** Setting SKImageInfo.ColorSpace = null before decoding no longer suppresses color correction in SkiaSharp 2.80.2, whereas it worked as expected in 1.68.3, producing visually incorrect color-converted output.

**Analysis:** SKBitmap.Decode with a null ColorSpace in SKImageInfo passes IntPtr.Zero to Skia's native sk_codec_get_pixels. In Skia 1.68.x, a null destination colorspace suppressed color transformation. In Skia 2.80.x, the native codec appears to apply the source image's embedded ICC profile regardless of the null destination colorspace, converting wide-gamut images (ProPhoto, AdobeRGB) into sRGB or another default space, changing the raw pixel values.

**Recommendations:** **needs-investigation** — Complete reproduction code is provided and the regression is clearly documented with visual evidence. The C# binding is correct; native behavior needs investigation to find whether a no-transformation mode still exists in current Skia.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create an SKCodec from a JPEG with embedded wide-gamut color profile (e.g., ProPhoto RGB or Adobe RGB)
2. Get codec.Info, then set info.ColorSpace = null
3. Call SKBitmap.Decode(codec, info)
4. Encode the resulting bitmap as JPEG and compare to input

**Environment:** SkiaSharp 2.80.2, Windows 10, .NET console app

**Repository links:**
- https://user-images.githubusercontent.com/2389359/97585892-ebe2ea00-19f9-11eb-9f25-aa2c62e36dc0.jpg — Expected output (1.68.3) - ProPhoto no color correction
- https://user-images.githubusercontent.com/2389359/97585909-f0a79e00-19f9-11eb-95fc-0a67a4728102.jpg — Expected output (1.68.3) - AdobeRGB no color correction
- https://user-images.githubusercontent.com/2389359/97586149-36646680-19fa-11eb-9b8e-fbabacd12a0f.jpg — Actual output (2.80.2) - ProPhoto with unwanted color correction
- https://user-images.githubusercontent.com/2389359/97586155-3a908400-19fa-11eb-9225-7bd6177e0492.jpg — Actual output (2.80.2) - AdobeRGB with unwanted color correction

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | Color space conversion applied during decode even when ColorSpace is set to null in SKImageInfo |
| Repro quality | complete |
| Target frameworks | net5.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.3, 2.80.2 |
| Worked in | 1.68.3 |
| Broke in | 2.80.2 |
| Current relevance | likely |
| Relevance reason | The SKImageInfo.ColorSpace null-passthrough path has not changed since 2.80.2 — IntPtr.Zero is still passed to native for null colorspace, so the native behavior change likely persists. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.90 (90%) |
| Reason | Reporter explicitly identifies 1.68.3 as last known good, provides side-by-side images proving the behavior difference, and the code pattern of null ColorSpace to skip color correction was a supported workflow. |
| Worked in version | 1.68.3 |
| Broke in version | 2.80.2 |

## Analysis

### Technical Summary

SKBitmap.Decode with a null ColorSpace in SKImageInfo passes IntPtr.Zero to Skia's native sk_codec_get_pixels. In Skia 1.68.x, a null destination colorspace suppressed color transformation. In Skia 2.80.x, the native codec appears to apply the source image's embedded ICC profile regardless of the null destination colorspace, converting wide-gamut images (ProPhoto, AdobeRGB) into sRGB or another default space, changing the raw pixel values.

### Rationale

The C# binding correctly converts null ColorSpace to IntPtr.Zero (confirmed in SKImageInfoNative.FromManaged). The behavioral change is therefore in the underlying Skia C++ codec. This is a real regression: a previously documented workflow (null colorspace = raw decode) no longer works in the 2.x series, and the reporter provides complete reproduction code and before/after image evidence.

### Key Signals

- "info.ColorSpace = null; ... using var bitmap = SKBitmap.Decode(codec, info);" — **issue body** (Reporter explicitly sets null ColorSpace to suppress Skia's color transformation during decode — a pattern that worked in 1.x.)
- "Version with issue: 2.80.2 / Last known good version: 1.68.3" — **issue body** (Clear regression: the major version bump from 1.x to 2.x corresponded to a significant Skia engine update that changed codec color handling.)
- "It seems in 2.80.2 color conversion is always performed, so can't be disabled anymore?" — **issue body** (Reporter's hypothesis is consistent with Skia changing the null-colorspace behavior in newer versions.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKImageInfo.cs` | 18-25 | direct | SKImageInfoNative.FromManaged maps ColorSpace to IntPtr.Zero when null — the C# layer correctly passes null colorspace to native. The bug is therefore in native Skia behavior, not the SkiaSharp wrapper. |
| `binding/SkiaSharp/SKCodec.cs` | 119-137 | direct | GetPixels passes the nInfo struct (with zero colorspace handle) directly to sk_codec_get_pixels. No additional color-space fixup is done in C# before calling native — behavior depends entirely on Skia's codec implementation. |
| `binding/SkiaSharp/SKBitmap.cs` | 446-460 | related | Decode(SKCodec codec, SKImageInfo bitmapInfo) allocates an SKBitmap and calls codec.GetPixels with the caller-supplied bitmapInfo unchanged. No modification of bitmapInfo.ColorSpace occurs — null is preserved and forwarded. |

### Workarounds

- Try creating an SKColorSpace that matches the source image's color space exactly (so the conversion becomes a no-op): var codec = SKCodec.Create(stream); var info = codec.Info; /* keep info.ColorSpace as-is or explicitly set to match source */ — this may not be possible without access to the exact ICC profile.
- Read raw pixel bytes directly without using SKBitmap.Decode by copying codec pixel memory without transformation, if the application only needs the raw encoded values.

### Next Questions

- Does the behavior persist in SkiaSharp 2.88.x or 3.x?
- Is there a new SKCodecOptions or API in 2.x that allows disabling color correction explicitly?
- What Skia milestone change caused the null-colorspace behavior to change?
- Can the issue be reproduced on Linux (to rule out Windows-specific ICC profile handling)?

### Resolution Proposals

**Hypothesis:** Skia's codec changed its handling of a null destination colorspace between 1.68 and 2.80 — possibly now treating null as 'use source colorspace' rather than 'no transformation'. The SkiaSharp wrapper itself is correct; the fix would require either exposing a new native option or documenting the changed behavior with an alternative API.

1. **Investigate Skia codec null-colorspace behavior change** — investigation, confidence 0.85 (85%), cost/m, validated=untested
   - Review Skia changelog and SkCodec source between m76 (SkiaSharp 1.68) and m80+ (SkiaSharp 2.80) to understand how null SkColorSpace in SkImageInfo is handled by getPixels. Determine if a no-transformation mode still exists.
2. **Document new API for disabling color correction** — alternative, confidence 0.70 (70%), cost/s, validated=untested
   - If Skia now requires an explicit mechanism to skip color correction (e.g., passing a linearized or identity colorspace), document the new approach and provide a migration guide from the 1.x null-colorspace pattern.

**Recommended proposal:** Investigate Skia codec null-colorspace behavior change

**Why:** The root cause is unknown — the native behavior change needs to be confirmed before any fix or documentation can be written.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.87 (87%) |
| Reason | Complete reproduction code is provided and the regression is clearly documented with visual evidence. The C# binding is correct; native behavior needs investigation to find whether a no-transformation mode still exists in current Skia. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, area, platform, and compatibility labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/compatibility |
| add-comment | medium | 0.85 (85%) | Acknowledge regression and explain current investigation status | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report and the visual comparison images — they make the regression very clear.

The SkiaSharp binding correctly forwards `null` ColorSpace as a null pointer to Skia's native `sk_codec_get_pixels`. The behavior change is therefore in the underlying Skia engine between versions 1.68 (Skia m76) and 2.80 (Skia m80+).

In newer Skia, passing a null destination `SkColorSpace` in `SkImageInfo` to the codec may no longer suppress color transformation — Skia may now apply the source image's embedded ICC profile regardless.

As a potential workaround, you could try:
1. Keep the original `codec.Info.ColorSpace` (don't set it to null) — if you just want the raw wide-gamut pixels without sRGB conversion, you may need to explicitly set a colorspace that matches the source.
2. Investigate whether `SKPixmap` with a null colorspace offers different behavior.

We'll investigate whether the current Skia version exposes a way to explicitly disable color correction during decode.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1543,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T05:46:00Z"
  },
  "summary": "Setting SKImageInfo.ColorSpace = null before decoding no longer suppresses color correction in SkiaSharp 2.80.2, whereas it worked as expected in 1.68.3, producing visually incorrect color-converted output.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.88
    },
    "platforms": [
      "os/Windows-Classic"
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
      "errorMessage": "Color space conversion applied during decode even when ColorSpace is set to null in SKImageInfo",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net5.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKCodec from a JPEG with embedded wide-gamut color profile (e.g., ProPhoto RGB or Adobe RGB)",
        "Get codec.Info, then set info.ColorSpace = null",
        "Call SKBitmap.Decode(codec, info)",
        "Encode the resulting bitmap as JPEG and compare to input"
      ],
      "environmentDetails": "SkiaSharp 2.80.2, Windows 10, .NET console app",
      "repoLinks": [
        {
          "url": "https://user-images.githubusercontent.com/2389359/97585892-ebe2ea00-19f9-11eb-9f25-aa2c62e36dc0.jpg",
          "description": "Expected output (1.68.3) - ProPhoto no color correction"
        },
        {
          "url": "https://user-images.githubusercontent.com/2389359/97585909-f0a79e00-19f9-11eb-95fc-0a67a4728102.jpg",
          "description": "Expected output (1.68.3) - AdobeRGB no color correction"
        },
        {
          "url": "https://user-images.githubusercontent.com/2389359/97586149-36646680-19fa-11eb-9b8e-fbabacd12a0f.jpg",
          "description": "Actual output (2.80.2) - ProPhoto with unwanted color correction"
        },
        {
          "url": "https://user-images.githubusercontent.com/2389359/97586155-3a908400-19fa-11eb-9225-7bd6177e0492.jpg",
          "description": "Actual output (2.80.2) - AdobeRGB with unwanted color correction"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.3",
        "2.80.2"
      ],
      "workedIn": "1.68.3",
      "brokeIn": "2.80.2",
      "currentRelevance": "likely",
      "relevanceReason": "The SKImageInfo.ColorSpace null-passthrough path has not changed since 2.80.2 — IntPtr.Zero is still passed to native for null colorspace, so the native behavior change likely persists."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.9,
      "reason": "Reporter explicitly identifies 1.68.3 as last known good, provides side-by-side images proving the behavior difference, and the code pattern of null ColorSpace to skip color correction was a supported workflow.",
      "workedInVersion": "1.68.3",
      "brokeInVersion": "2.80.2"
    }
  },
  "analysis": {
    "summary": "SKBitmap.Decode with a null ColorSpace in SKImageInfo passes IntPtr.Zero to Skia's native sk_codec_get_pixels. In Skia 1.68.x, a null destination colorspace suppressed color transformation. In Skia 2.80.x, the native codec appears to apply the source image's embedded ICC profile regardless of the null destination colorspace, converting wide-gamut images (ProPhoto, AdobeRGB) into sRGB or another default space, changing the raw pixel values.",
    "rationale": "The C# binding correctly converts null ColorSpace to IntPtr.Zero (confirmed in SKImageInfoNative.FromManaged). The behavioral change is therefore in the underlying Skia C++ codec. This is a real regression: a previously documented workflow (null colorspace = raw decode) no longer works in the 2.x series, and the reporter provides complete reproduction code and before/after image evidence.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKImageInfo.cs",
        "lines": "18-25",
        "finding": "SKImageInfoNative.FromManaged maps ColorSpace to IntPtr.Zero when null — the C# layer correctly passes null colorspace to native. The bug is therefore in native Skia behavior, not the SkiaSharp wrapper.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "119-137",
        "finding": "GetPixels passes the nInfo struct (with zero colorspace handle) directly to sk_codec_get_pixels. No additional color-space fixup is done in C# before calling native — behavior depends entirely on Skia's codec implementation.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "446-460",
        "finding": "Decode(SKCodec codec, SKImageInfo bitmapInfo) allocates an SKBitmap and calls codec.GetPixels with the caller-supplied bitmapInfo unchanged. No modification of bitmapInfo.ColorSpace occurs — null is preserved and forwarded.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "info.ColorSpace = null; ... using var bitmap = SKBitmap.Decode(codec, info);",
        "source": "issue body",
        "interpretation": "Reporter explicitly sets null ColorSpace to suppress Skia's color transformation during decode — a pattern that worked in 1.x."
      },
      {
        "text": "Version with issue: 2.80.2 / Last known good version: 1.68.3",
        "source": "issue body",
        "interpretation": "Clear regression: the major version bump from 1.x to 2.x corresponded to a significant Skia engine update that changed codec color handling."
      },
      {
        "text": "It seems in 2.80.2 color conversion is always performed, so can't be disabled anymore?",
        "source": "issue body",
        "interpretation": "Reporter's hypothesis is consistent with Skia changing the null-colorspace behavior in newer versions."
      }
    ],
    "workarounds": [
      "Try creating an SKColorSpace that matches the source image's color space exactly (so the conversion becomes a no-op): var codec = SKCodec.Create(stream); var info = codec.Info; /* keep info.ColorSpace as-is or explicitly set to match source */ — this may not be possible without access to the exact ICC profile.",
      "Read raw pixel bytes directly without using SKBitmap.Decode by copying codec pixel memory without transformation, if the application only needs the raw encoded values."
    ],
    "nextQuestions": [
      "Does the behavior persist in SkiaSharp 2.88.x or 3.x?",
      "Is there a new SKCodecOptions or API in 2.x that allows disabling color correction explicitly?",
      "What Skia milestone change caused the null-colorspace behavior to change?",
      "Can the issue be reproduced on Linux (to rule out Windows-specific ICC profile handling)?"
    ],
    "resolution": {
      "hypothesis": "Skia's codec changed its handling of a null destination colorspace between 1.68 and 2.80 — possibly now treating null as 'use source colorspace' rather than 'no transformation'. The SkiaSharp wrapper itself is correct; the fix would require either exposing a new native option or documenting the changed behavior with an alternative API.",
      "proposals": [
        {
          "title": "Investigate Skia codec null-colorspace behavior change",
          "description": "Review Skia changelog and SkCodec source between m76 (SkiaSharp 1.68) and m80+ (SkiaSharp 2.80) to understand how null SkColorSpace in SkImageInfo is handled by getPixels. Determine if a no-transformation mode still exists.",
          "category": "investigation",
          "confidence": 0.85,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Document new API for disabling color correction",
          "description": "If Skia now requires an explicit mechanism to skip color correction (e.g., passing a linearized or identity colorspace), document the new approach and provide a migration guide from the 1.x null-colorspace pattern.",
          "category": "alternative",
          "confidence": 0.7,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Investigate Skia codec null-colorspace behavior change",
      "recommendedReason": "The root cause is unknown — the native behavior change needs to be confirmed before any fix or documentation can be written."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.87,
      "reason": "Complete reproduction code is provided and the regression is clearly documented with visual evidence. The C# binding is correct; native behavior needs investigation to find whether a no-transformation mode still exists in current Skia.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, area, platform, and compatibility labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge regression and explain current investigation status",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thank you for the detailed report and the visual comparison images — they make the regression very clear.\n\nThe SkiaSharp binding correctly forwards `null` ColorSpace as a null pointer to Skia's native `sk_codec_get_pixels`. The behavior change is therefore in the underlying Skia engine between versions 1.68 (Skia m76) and 2.80 (Skia m80+).\n\nIn newer Skia, passing a null destination `SkColorSpace` in `SkImageInfo` to the codec may no longer suppress color transformation — Skia may now apply the source image's embedded ICC profile regardless.\n\nAs a potential workaround, you could try:\n1. Keep the original `codec.Info.ColorSpace` (don't set it to null) — if you just want the raw wide-gamut pixels without sRGB conversion, you may need to explicitly set a colorspace that matches the source.\n2. Investigate whether `SKPixmap` with a null colorspace offers different behavior.\n\nWe'll investigate whether the current Skia version exposes a way to explicitly disable color correction during decode."
      }
    ]
  }
}
```

</details>
