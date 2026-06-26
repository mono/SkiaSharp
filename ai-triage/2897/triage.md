# Issue Triage Report — #2897

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-06-26T05:50:00Z |
| Type | type/feature-request (0.98 (98%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | keep-open (0.88 (88%)) |

**Issue Summary:** Reporter requests animated GIF encoding API in SkiaSharp to encode frame sequences with per-frame delays, currently using SixLabors.ImageSharp as a workaround due to its non-MIT license.

**Analysis:** Animated GIF encoding is absent from SkiaSharp because Skia itself does not expose a GIF encoder in its public C++ API — GIF support in Skia is decode-only via its codec infrastructure. SkiaSharp can only wrap what Skia provides, so adding animated GIF encoding would require either integrating a third-party library or waiting for upstream Skia to add GIF encoding.

**Recommendations:** **keep-open** — Valid, well-specified feature request but Skia lacks a GIF encoder in its C++ API. Cannot be implemented by simple wrapping. Keep open as a tracked request; animated WebP is available as a workaround.

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

1. Attempt to encode a series of RGBA8888 pixel arrays as an animated GIF using SkiaSharp
2. Observe that no animated GIF encoding API exists in SkiaSharp

**Environment:** Cross-platform .NET; no specific platform restriction mentioned

## Analysis

### Technical Summary

Animated GIF encoding is absent from SkiaSharp because Skia itself does not expose a GIF encoder in its public C++ API — GIF support in Skia is decode-only via its codec infrastructure. SkiaSharp can only wrap what Skia provides, so adding animated GIF encoding would require either integrating a third-party library or waiting for upstream Skia to add GIF encoding.

### Rationale

This is a genuine feature request for animated GIF encoding. The feature is confirmed absent: no sk_gifencoder_* function exists in the generated C API, and Skia's C++ engine does not provide a GIF encoder. The reporter provides clear scope (RGBA8888 pixel arrays, per-frame delays, width parameter). The animated WebP encoder (SKWebpEncoder.EncodeAnimated) is a structural analog but a different format. Implementation would require significant work at the Skia C API layer or integration of a third-party GIF library.

### Key Signals

- "I want to encode animated GIFs but SkiaSharp seems to be missing this feature." — **issue body** (Reporter has already checked and confirmed the feature is absent — this is a genuine missing API.)
- "ImageSharp is my only dependency which isn't MIT-licensed" — **issue body** (Motivation is license compatibility; reporter wants to replace SixLabors.ImageSharp with an MIT-licensed solution.)
- "This is the only thing I'm using ImageSharp for which SkiaSharp (AFAIK) can't do" — **issue body** (Reporter is well-informed about SkiaSharp's capabilities; the gap is real and narrow.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaApi.generated.cs` | 21165-21166 | direct | SKEncodedFormat.Gif = 1 exists for format identification during decoding. No sk_gifencoder_* function appears anywhere in the generated C API bindings, confirming that animated GIF encoding is not supported. |
| `binding/SkiaSharp/SKWebpEncoder.cs` | 39-74 | related | SKWebpEncoder.EncodeAnimated exists and encodes animated WebP frames via sk_webpencoder_encode_animated. This is the only multi-frame animated encoding API in SkiaSharp, and it is format-specific to WebP. No analogous SKGifEncoder class or file exists. |

### Workarounds

- Use SKWebpEncoder.EncodeAnimated for animated WebP output — widely supported in modern browsers and platforms, MIT-compatible, but produces .webp not .gif
- Continue using SixLabors.ImageSharp for GIF encoding (dual-library approach)
- Use a pure-.NET GIF encoder library such as AnimatedGif (MIT-licensed) as a standalone dependency

### Next Questions

- Does upstream Skia plan to add GIF encoding support in a future milestone?
- Would the reporter accept animated WebP as an alternative if browser/platform support is sufficient for their use-case?

### Resolution Proposals

**Hypothesis:** Skia lacks a GIF encoder, so SkiaSharp cannot expose one by wrapping. Implementation would require integrating a third-party GIF encoder or waiting for Skia to add one upstream.

1. **Use animated WebP instead** — workaround, confidence 0.85 (85%), cost/xs, validated=untested
   - SKWebpEncoder.EncodeAnimated already exists in SkiaSharp and produces animated .webp files from SKPixmap/SKBitmap/SKImage frames. Most modern browsers and platforms support WebP animation, and the license is MIT-compatible.
2. **Add GIF encoding via third-party library integration** — investigation, confidence 0.50 (50%), cost/xl, validated=untested
   - SkiaSharp could add a SKGifEncoder class that wraps a third-party GIF encoding library (e.g., giflib). This would require adding a native dependency and exposing it through the C API layer, making it a significant addition.

**Recommended proposal:** Use animated WebP instead

**Why:** SKWebpEncoder.EncodeAnimated is already available in SkiaSharp and produces animated images with per-frame delay control. If the output format can be WebP rather than GIF, this is an immediate MIT-licensed solution that removes the ImageSharp dependency.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.88 (88%) |
| Reason | Valid, well-specified feature request but Skia lacks a GIF encoder in its C++ API. Cannot be implemented by simple wrapping. Keep open as a tracked request; animated WebP is available as a workaround. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply feature-request and area/SkiaSharp labels (already labeled type/feature-request, confirm area) | labels=type/feature-request, area/SkiaSharp |
| add-comment | medium | 0.85 (85%) | Explain that Skia lacks a GIF encoder, suggest animated WebP as a workaround, acknowledge the feature request | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the feature request!

Unfortunately, animated GIF encoding is not currently available in SkiaSharp. Skia's C++ engine only supports GIF **decoding** (via its codec infrastructure) and has no GIF encoder, so SkiaSharp cannot expose one by wrapping.

As a workaround, SkiaSharp already supports **animated WebP encoding** via `SKWebpEncoder.EncodeAnimated`. If your use-case allows WebP output (it is widely supported in modern browsers and platforms), this may be a viable MIT-licensed alternative:

```csharp
using var pixmap1 = bitmap1.PeekPixels();
using var pixmap2 = bitmap2.PeekPixels();

var frames = new SKWebpEncoderFrame[]
{
    new SKWebpEncoderFrame(pixmap1, TimeSpan.FromMilliseconds(100)),
    new SKWebpEncoderFrame(pixmap2, TimeSpan.FromMilliseconds(100)),
};

using var data = SKWebpEncoder.EncodeAnimated(frames, SKWebpEncoderOptions.Default);
// data contains the animated .webp bytes
```

If you need GIF specifically, other MIT-licensed pure-.NET options include [AnimatedGif](https://github.com/mrousavy/AnimatedGif) as a standalone dependency.

We will keep this open as a feature request to track community demand for a `SKGifEncoder` API.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2897,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-06-26T05:50:00Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "Reporter requests animated GIF encoding API in SkiaSharp to encode frame sequences with per-frame delays, currently using SixLabors.ImageSharp as a workaround due to its non-MIT license.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.98
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    }
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Attempt to encode a series of RGBA8888 pixel arrays as an animated GIF using SkiaSharp",
        "Observe that no animated GIF encoding API exists in SkiaSharp"
      ],
      "environmentDetails": "Cross-platform .NET; no specific platform restriction mentioned"
    }
  },
  "analysis": {
    "summary": "Animated GIF encoding is absent from SkiaSharp because Skia itself does not expose a GIF encoder in its public C++ API — GIF support in Skia is decode-only via its codec infrastructure. SkiaSharp can only wrap what Skia provides, so adding animated GIF encoding would require either integrating a third-party library or waiting for upstream Skia to add GIF encoding.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "21165-21166",
        "finding": "SKEncodedFormat.Gif = 1 exists for format identification during decoding. No sk_gifencoder_* function appears anywhere in the generated C API bindings, confirming that animated GIF encoding is not supported.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKWebpEncoder.cs",
        "lines": "39-74",
        "finding": "SKWebpEncoder.EncodeAnimated exists and encodes animated WebP frames via sk_webpencoder_encode_animated. This is the only multi-frame animated encoding API in SkiaSharp, and it is format-specific to WebP. No analogous SKGifEncoder class or file exists.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "I want to encode animated GIFs but SkiaSharp seems to be missing this feature.",
        "source": "issue body",
        "interpretation": "Reporter has already checked and confirmed the feature is absent — this is a genuine missing API."
      },
      {
        "text": "ImageSharp is my only dependency which isn't MIT-licensed",
        "source": "issue body",
        "interpretation": "Motivation is license compatibility; reporter wants to replace SixLabors.ImageSharp with an MIT-licensed solution."
      },
      {
        "text": "This is the only thing I'm using ImageSharp for which SkiaSharp (AFAIK) can't do",
        "source": "issue body",
        "interpretation": "Reporter is well-informed about SkiaSharp's capabilities; the gap is real and narrow."
      }
    ],
    "rationale": "This is a genuine feature request for animated GIF encoding. The feature is confirmed absent: no sk_gifencoder_* function exists in the generated C API, and Skia's C++ engine does not provide a GIF encoder. The reporter provides clear scope (RGBA8888 pixel arrays, per-frame delays, width parameter). The animated WebP encoder (SKWebpEncoder.EncodeAnimated) is a structural analog but a different format. Implementation would require significant work at the Skia C API layer or integration of a third-party GIF library.",
    "workarounds": [
      "Use SKWebpEncoder.EncodeAnimated for animated WebP output — widely supported in modern browsers and platforms, MIT-compatible, but produces .webp not .gif",
      "Continue using SixLabors.ImageSharp for GIF encoding (dual-library approach)",
      "Use a pure-.NET GIF encoder library such as AnimatedGif (MIT-licensed) as a standalone dependency"
    ],
    "nextQuestions": [
      "Does upstream Skia plan to add GIF encoding support in a future milestone?",
      "Would the reporter accept animated WebP as an alternative if browser/platform support is sufficient for their use-case?"
    ],
    "resolution": {
      "hypothesis": "Skia lacks a GIF encoder, so SkiaSharp cannot expose one by wrapping. Implementation would require integrating a third-party GIF encoder or waiting for Skia to add one upstream.",
      "proposals": [
        {
          "title": "Use animated WebP instead",
          "description": "SKWebpEncoder.EncodeAnimated already exists in SkiaSharp and produces animated .webp files from SKPixmap/SKBitmap/SKImage frames. Most modern browsers and platforms support WebP animation, and the license is MIT-compatible.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Add GIF encoding via third-party library integration",
          "description": "SkiaSharp could add a SKGifEncoder class that wraps a third-party GIF encoding library (e.g., giflib). This would require adding a native dependency and exposing it through the C API layer, making it a significant addition.",
          "category": "investigation",
          "confidence": 0.5,
          "effort": "cost/xl",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use animated WebP instead",
      "recommendedReason": "SKWebpEncoder.EncodeAnimated is already available in SkiaSharp and produces animated images with per-frame delay control. If the output format can be WebP rather than GIF, this is an immediate MIT-licensed solution that removes the ImageSharp dependency."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.88,
      "reason": "Valid, well-specified feature request but Skia lacks a GIF encoder in its C++ API. Cannot be implemented by simple wrapping. Keep open as a tracked request; animated WebP is available as a workaround.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request and area/SkiaSharp labels (already labeled type/feature-request, confirm area)",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain that Skia lacks a GIF encoder, suggest animated WebP as a workaround, acknowledge the feature request",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the feature request!\n\nUnfortunately, animated GIF encoding is not currently available in SkiaSharp. Skia's C++ engine only supports GIF **decoding** (via its codec infrastructure) and has no GIF encoder, so SkiaSharp cannot expose one by wrapping.\n\nAs a workaround, SkiaSharp already supports **animated WebP encoding** via `SKWebpEncoder.EncodeAnimated`. If your use-case allows WebP output (it is widely supported in modern browsers and platforms), this may be a viable MIT-licensed alternative:\n\n```csharp\nusing var pixmap1 = bitmap1.PeekPixels();\nusing var pixmap2 = bitmap2.PeekPixels();\n\nvar frames = new SKWebpEncoderFrame[]\n{\n    new SKWebpEncoderFrame(pixmap1, TimeSpan.FromMilliseconds(100)),\n    new SKWebpEncoderFrame(pixmap2, TimeSpan.FromMilliseconds(100)),\n};\n\nusing var data = SKWebpEncoder.EncodeAnimated(frames, SKWebpEncoderOptions.Default);\n// data contains the animated .webp bytes\n```\n\nIf you need GIF specifically, other MIT-licensed pure-.NET options include [AnimatedGif](https://github.com/mrousavy/AnimatedGif) as a standalone dependency.\n\nWe will keep this open as a feature request to track community demand for a `SKGifEncoder` API."
      }
    ]
  }
}
```

</details>
