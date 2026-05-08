# Issue Triage Report — #2866

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T01:47:35Z |
| Type | type/feature-request (0.95 (95%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** Feature request to expose libjpeg-turbo's do_fancy_upsampling and do_block_smoothing decode options through SkiaSharp's SKCodecOptions so that pixel-level JPEG decoding parity with System.Drawing can be achieved.

**Analysis:** Reporter is migrating a pixel-based fingerprinting system from net48/System.Drawing to SkiaSharp. System.Drawing's GDI+ JPEG decoder decodes YUV420 JPEGs without fancy upsampling or block smoothing, while SkiaSharp's libjpeg-turbo backend enables these by default, producing slightly different pixel values that break the fingerprinting algorithm. The reporter requests new SKCodecOptions properties to disable these decoder settings. Currently, SKCodecOptions exposes only ZeroInitialized, Subset, FrameIndex, and PriorFrame — no JPEG-specific decode hints.

**Recommendations:** **keep-open** — Legitimate feature request for JPEG decode quality control. Valid use case for pixel-exact porting scenarios. Requires upstream Skia investigation before implementation is feasible in SkiaSharp.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | type/feature-request |

## Evidence

### Reproduction

**Environment:** Migrating from net48 System.Drawing.Imaging to .NET 5+ SkiaSharp; YUV420 JPEG decoding produces slightly different pixel values

**Repository links:**
- https://github.com/libjpeg-turbo/libjpeg-turbo/blob/9ddcae4a8f4aaa096ccdcddec36afc52fbc01481/jpeglib.h#L548-L549 — libjpeg-turbo do_fancy_upsampling and do_block_smoothing flags in jpeglib.h

## Analysis

### Technical Summary

Reporter is migrating a pixel-based fingerprinting system from net48/System.Drawing to SkiaSharp. System.Drawing's GDI+ JPEG decoder decodes YUV420 JPEGs without fancy upsampling or block smoothing, while SkiaSharp's libjpeg-turbo backend enables these by default, producing slightly different pixel values that break the fingerprinting algorithm. The reporter requests new SKCodecOptions properties to disable these decoder settings. Currently, SKCodecOptions exposes only ZeroInitialized, Subset, FrameIndex, and PriorFrame — no JPEG-specific decode hints.

### Rationale

This is a feature request — the reporter is not reporting broken behavior but requesting new API to control existing libjpeg-turbo behavior. The use case (pixel-perfect porting of a fingerprinting algorithm) is legitimate and niche. Implementing this would require changes at multiple layers: Skia C++ codec level (SkJpegCodec) and then the SkiaSharp C API shim and C# wrapper. The Skia codec does not currently expose these flags via SkCodec::Options. This may require upstream Skia changes or a SkiaSharp-level fork/shim.

### Key Signals

- "apparently yuv420 jpegs decoded with System.Drawing.Imaging don't have fancy upsampling or block smoothing applied" — **issue body** (GDI+ JPEG decoder leaves these libjpeg-turbo options disabled by default; SkiaSharp enables them, causing pixel differences.)
- "can we have corresponding SkCodecOptions to set them?" — **issue body** (Clear new API request — expose JPEG decode quality flags through SKCodecOptions.)
- "the workaround is to use an alternative jpeg codec or call into libjpeg-turbo directly" — **issue body** (Reporter has identified workarounds but finds them undesirable; confirms this is a gap in the existing API.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/Definitions.cs` | 220-291 | direct | SKCodecOptions struct exposes ZeroInitialized, Subset, FrameIndex, and PriorFrame. No JPEG-specific decode quality options (fancy upsampling, block smoothing) are present. |
| `binding/SkiaSharp/SKCodec.cs` | 119-137 | direct | GetPixels maps SKCodecOptions to SKCodecOptionsInternal (fZeroInitialized, fSubset, fFrameIndex, fPriorFrame). No JPEG-specific flags are passed through to the native codec. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 18597-18635 | direct | SKCodecOptionsInternal (native interop struct) mirrors the C struct: fZeroInitialized, fSubset, fFrameIndex, fPriorFrame, fMaxDecodeMemory. No JPEG decoder options present. |

### Workarounds

- Use an alternative JPEG codec library (e.g., ImageSharp, LibJpegTurbo NuGet) that allows disabling fancy upsampling.
- P/Invoke into libjpeg-turbo directly to decode the JPEG with the desired options, bypassing SkiaSharp's codec.
- Adapt the fingerprinting algorithm to work with SkiaSharp's default decoder output instead of replicating legacy behavior.

### Next Questions

- Does upstream Skia's SkCodec::Options (or SkJpegCodec) support any mechanism to pass decoder hints to libjpeg-turbo?
- Would this be better addressed by contributing decoder hint support to upstream Skia first, or is a SkiaSharp-level shim feasible?
- How many other users would benefit from JPEG decode quality control options?

### Resolution Proposals

**Hypothesis:** SkiaSharp's SKCodecOptions does not expose libjpeg-turbo's do_fancy_upsampling/do_block_smoothing flags because Skia's SkCodec::Options struct itself does not include JPEG-specific decoder hints.

1. **Use alternative JPEG decoder** — workaround, confidence 0.85 (85%), cost/s, validated=untested
   - Use ImageSharp or a direct libjpeg-turbo NuGet package with the desired decode options to get pixel-identical output to System.Drawing, then copy the result into an SKBitmap.
2. **Upstream Skia change + SkiaSharp API surface** — fix, confidence 0.60 (60%), cost/xl, validated=untested
   - Propose adding JPEG decoder hints to SkCodec::Options in upstream Skia (SkJpegCodec), then expose them through the C API shim and SKCodecOptions in SkiaSharp.

**Recommended proposal:** Use alternative JPEG decoder

**Why:** Implementing upstream Skia changes is a large effort and uncertain timeline. Using a dedicated decoder for legacy-compat pixels is a practical near-term workaround.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Legitimate feature request for JPEG decode quality control. Valid use case for pixel-exact porting scenarios. Requires upstream Skia investigation before implementation is feasible in SkiaSharp. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Confirm existing feature-request label; add area/SkiaSharp | labels=type/feature-request, area/SkiaSharp |
| add-comment | medium | 0.85 (85%) | Acknowledge the request, explain the current architecture limitation, and provide workarounds | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed request and context!

You're right that SkiaSharp's JPEG decoding goes through libjpeg-turbo (bundled with Skia) with fancy upsampling and block smoothing enabled by default. Unfortunately, `SKCodecOptions` currently only surfaces frame/subset/zero-init options — Skia's `SkCodec::Options` struct does not include JPEG-specific decoder hints, so there's no path to expose these flags without upstream Skia changes.

**Workarounds for now:**

1. **Alternative JPEG decoder** — Use [ImageSharp](https://github.com/SixLabors/ImageSharp) or a direct libjpeg-turbo binding (e.g., [`libjpeg-turbo` NuGet](https://www.nuget.org/packages/LibJpegTurbo)) to decode YUV420 JPEGs with the desired settings, then copy the decoded pixels into an `SKBitmap`.

2. **Direct P/Invoke** — If you already have libjpeg-turbo in your process (it ships with SkiaSharp's native library), you could potentially decode directly with the flags disabled, though this is fragile and version-dependent.

We'll keep this open as a feature request. If upstream Skia adds decoder hint support to `SkCodec::Options`, we can surface it in `SKCodecOptions` relatively quickly.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2866,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T01:47:35Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "Feature request to expose libjpeg-turbo's do_fancy_upsampling and do_block_smoothing decode options through SkiaSharp's SKCodecOptions so that pixel-level JPEG decoding parity with System.Drawing can be achieved.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    }
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Migrating from net48 System.Drawing.Imaging to .NET 5+ SkiaSharp; YUV420 JPEG decoding produces slightly different pixel values",
      "repoLinks": [
        {
          "url": "https://github.com/libjpeg-turbo/libjpeg-turbo/blob/9ddcae4a8f4aaa096ccdcddec36afc52fbc01481/jpeglib.h#L548-L549",
          "description": "libjpeg-turbo do_fancy_upsampling and do_block_smoothing flags in jpeglib.h"
        }
      ]
    }
  },
  "analysis": {
    "summary": "Reporter is migrating a pixel-based fingerprinting system from net48/System.Drawing to SkiaSharp. System.Drawing's GDI+ JPEG decoder decodes YUV420 JPEGs without fancy upsampling or block smoothing, while SkiaSharp's libjpeg-turbo backend enables these by default, producing slightly different pixel values that break the fingerprinting algorithm. The reporter requests new SKCodecOptions properties to disable these decoder settings. Currently, SKCodecOptions exposes only ZeroInitialized, Subset, FrameIndex, and PriorFrame — no JPEG-specific decode hints.",
    "rationale": "This is a feature request — the reporter is not reporting broken behavior but requesting new API to control existing libjpeg-turbo behavior. The use case (pixel-perfect porting of a fingerprinting algorithm) is legitimate and niche. Implementing this would require changes at multiple layers: Skia C++ codec level (SkJpegCodec) and then the SkiaSharp C API shim and C# wrapper. The Skia codec does not currently expose these flags via SkCodec::Options. This may require upstream Skia changes or a SkiaSharp-level fork/shim.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "220-291",
        "finding": "SKCodecOptions struct exposes ZeroInitialized, Subset, FrameIndex, and PriorFrame. No JPEG-specific decode quality options (fancy upsampling, block smoothing) are present.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "119-137",
        "finding": "GetPixels maps SKCodecOptions to SKCodecOptionsInternal (fZeroInitialized, fSubset, fFrameIndex, fPriorFrame). No JPEG-specific flags are passed through to the native codec.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "18597-18635",
        "finding": "SKCodecOptionsInternal (native interop struct) mirrors the C struct: fZeroInitialized, fSubset, fFrameIndex, fPriorFrame, fMaxDecodeMemory. No JPEG decoder options present.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "apparently yuv420 jpegs decoded with System.Drawing.Imaging don't have fancy upsampling or block smoothing applied",
        "source": "issue body",
        "interpretation": "GDI+ JPEG decoder leaves these libjpeg-turbo options disabled by default; SkiaSharp enables them, causing pixel differences."
      },
      {
        "text": "can we have corresponding SkCodecOptions to set them?",
        "source": "issue body",
        "interpretation": "Clear new API request — expose JPEG decode quality flags through SKCodecOptions."
      },
      {
        "text": "the workaround is to use an alternative jpeg codec or call into libjpeg-turbo directly",
        "source": "issue body",
        "interpretation": "Reporter has identified workarounds but finds them undesirable; confirms this is a gap in the existing API."
      }
    ],
    "workarounds": [
      "Use an alternative JPEG codec library (e.g., ImageSharp, LibJpegTurbo NuGet) that allows disabling fancy upsampling.",
      "P/Invoke into libjpeg-turbo directly to decode the JPEG with the desired options, bypassing SkiaSharp's codec.",
      "Adapt the fingerprinting algorithm to work with SkiaSharp's default decoder output instead of replicating legacy behavior."
    ],
    "nextQuestions": [
      "Does upstream Skia's SkCodec::Options (or SkJpegCodec) support any mechanism to pass decoder hints to libjpeg-turbo?",
      "Would this be better addressed by contributing decoder hint support to upstream Skia first, or is a SkiaSharp-level shim feasible?",
      "How many other users would benefit from JPEG decode quality control options?"
    ],
    "resolution": {
      "hypothesis": "SkiaSharp's SKCodecOptions does not expose libjpeg-turbo's do_fancy_upsampling/do_block_smoothing flags because Skia's SkCodec::Options struct itself does not include JPEG-specific decoder hints.",
      "proposals": [
        {
          "title": "Use alternative JPEG decoder",
          "description": "Use ImageSharp or a direct libjpeg-turbo NuGet package with the desired decode options to get pixel-identical output to System.Drawing, then copy the result into an SKBitmap.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Upstream Skia change + SkiaSharp API surface",
          "description": "Propose adding JPEG decoder hints to SkCodec::Options in upstream Skia (SkJpegCodec), then expose them through the C API shim and SKCodecOptions in SkiaSharp.",
          "category": "fix",
          "confidence": 0.6,
          "effort": "cost/xl",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use alternative JPEG decoder",
      "recommendedReason": "Implementing upstream Skia changes is a large effort and uncertain timeline. Using a dedicated decoder for legacy-compat pixels is a practical near-term workaround."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.8,
      "reason": "Legitimate feature request for JPEG decode quality control. Valid use case for pixel-exact porting scenarios. Requires upstream Skia investigation before implementation is feasible in SkiaSharp.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Confirm existing feature-request label; add area/SkiaSharp",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the request, explain the current architecture limitation, and provide workarounds",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed request and context!\n\nYou're right that SkiaSharp's JPEG decoding goes through libjpeg-turbo (bundled with Skia) with fancy upsampling and block smoothing enabled by default. Unfortunately, `SKCodecOptions` currently only surfaces frame/subset/zero-init options — Skia's `SkCodec::Options` struct does not include JPEG-specific decoder hints, so there's no path to expose these flags without upstream Skia changes.\n\n**Workarounds for now:**\n\n1. **Alternative JPEG decoder** — Use [ImageSharp](https://github.com/SixLabors/ImageSharp) or a direct libjpeg-turbo binding (e.g., [`libjpeg-turbo` NuGet](https://www.nuget.org/packages/LibJpegTurbo)) to decode YUV420 JPEGs with the desired settings, then copy the decoded pixels into an `SKBitmap`.\n\n2. **Direct P/Invoke** — If you already have libjpeg-turbo in your process (it ships with SkiaSharp's native library), you could potentially decode directly with the flags disabled, though this is fragile and version-dependent.\n\nWe'll keep this open as a feature request. If upstream Skia adds decoder hint support to `SkCodec::Options`, we can surface it in `SKCodecOptions` relatively quickly."
      }
    ]
  }
}
```

</details>
