# Issue Triage Report — #2633

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T21:23:08Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | keep-open (0.85 (85%)) |

**Issue Summary:** Feature request to add JPEG-XL (JXL) encode/decode support to SkiaSharp, citing Skia's limited JXL support added in milestone 98 and the libjxl library as implementation paths.

**Analysis:** The SKEncodedImageFormat enum already contains Jpegxl = 13 in the auto-generated bindings (from the C API), indicating Skia's C layer recognizes JXL as a format. However, there is no corresponding JXL encoder options class (like SKJpegEncoderOptions), and the SKPixmap.Encode() switch statement does not handle SKEncodedImageFormat.Jpegxl. Full JXL support would require: verifying Skia is built with JXL decode/encode enabled, adding C API encoder/decoder wrappers if not present, and exposing a SKJxlEncoderOptions class in the C# binding.

**Recommendations:** **keep-open** — Valid feature request with community interest. SKEncodedImageFormat.Jpegxl already exists in the enum, indicating partial upstream support. Needs investigation of build flags before implementation can begin.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/feature-request |

## Evidence

### Reproduction

**Environment:** No specific version mentioned. JXL format support requested as new capability.

**Repository links:**
- https://github.com/HinTak/skia-c-examples/issues/4 — External example linking JXL C API usage in Skia

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SKEncodedImageFormat.Jpegxl = 13 is already in the generated bindings enum, but no encoder wrapper or decode path is exposed in the public C# API. |

## Analysis

### Technical Summary

The SKEncodedImageFormat enum already contains Jpegxl = 13 in the auto-generated bindings (from the C API), indicating Skia's C layer recognizes JXL as a format. However, there is no corresponding JXL encoder options class (like SKJpegEncoderOptions), and the SKPixmap.Encode() switch statement does not handle SKEncodedImageFormat.Jpegxl. Full JXL support would require: verifying Skia is built with JXL decode/encode enabled, adding C API encoder/decoder wrappers if not present, and exposing a SKJxlEncoderOptions class in the C# binding.

### Rationale

Clear feature request with some upstream Skia support already in place (enum value generated from the C API). The reporter correctly identifies that Skia added limited JXL support in milestone 98. Community interest confirmed by comments and reactions. No existing JXL encoder wrapper exists in SKPixmap.cs or SKImage.cs. This is a genuine missing feature, not a bug.

### Key Signals

- "C# binding of libjxl or it seems that Skia added limited JPEGXL support in milestone 98" — **issue body** (Reporter is aware of two possible implementation paths: direct libjxl binding or surfacing existing Skia JXL codec.)
- "At the moment, JXL provides the best lossless image compression ratio. The encoder has very few parameters, so it won't be hard to add a wrapper." — **comment by SladeThe** (Community interest confirmed; suggests JXL encoder API surface is small and wrappable.)
- "See also https://github.com/HinTak/skia-c-examples/issues/4" — **comment by HinTak** (External reference to Skia JXL C API usage — confirms the underlying Skia capability exists at least experimentally.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaApi.generated.cs` | 20617-20647 | direct | SKEncodedImageFormat enum includes Jpegxl = 13, auto-generated from the Skia C API — confirms Skia's C layer already defines JXL as a recognized format identifier. |
| `binding/SkiaSharp/SKPixmap.cs` | 235-246 | direct | SKPixmap.Encode(SKWStream, SKEncodedImageFormat, int) switch handles only Jpeg, Png, and Webp. The default case returns false — JXL encoding via this path is silently unsupported. |
| `binding/SkiaSharp/SKImage.cs` | 360-363 | related | SKImage.Encode() delegates to SKEncodedImageFormat.Png by default; no JXL-specific overload exists. |

### Workarounds

- Convert JXL to another format server-side before passing to SkiaSharp (e.g., using libjxl.NET or ImageSharp with JXL plugin).
- Use the libjxl NuGet package directly for JXL encode/decode if available, then convert the decoded pixels to an SKBitmap via SKBitmap.FromImage or SKPixmap.

### Next Questions

- Is JXL decode/encode actually enabled in SkiaSharp's Skia build (check build flags / GN args for skia_use_libjxl_decode / skia_use_libjxl_encode)?
- Does the current Skia milestone used by SkiaSharp ship JXL decode support in the C API (sk_codec_t / sk_image_t paths)?
- Is there a sk_jpegxl_encoder_* C API function in externals/skia/src/c/ that SkiaSharp could wrap?

### Resolution Proposals

**Hypothesis:** Skia already defines JXL as format 13 in the C API enum, but SkiaSharp does not expose a JXL encoder options class or hook it into the Encode() path. Implementation requires confirming Skia build flags enable JXL, then adding a SKJxlEncoderOptions type and wiring it into SKPixmap.Encode().

1. **Verify Skia JXL build support and expose existing decode path** — investigation, confidence 0.75 (75%), cost/s, validated=untested
   - Check SkiaSharp's GN build args for skia_use_libjxl_decode. If enabled, SKCodec already auto-detects JXL via SKEncodedImageFormat.Jpegxl — no decode-side wrapper needed beyond verifying SKImage.FromEncodedData() works with JXL files.
2. **Add SKJxlEncoderOptions and wire into SKPixmap.Encode()** — fix, confidence 0.65 (65%), cost/l, validated=untested
   - Add a SKJxlEncoderOptions struct mirroring SKWebpEncoderOptions, expose a sk_jpegxl_encoder C API (if missing), bind it, and add an Encode(SKWStream, SKJxlEncoderOptions) overload to SKPixmap. Also handle SKEncodedImageFormat.Jpegxl in the switch statement.
3. **Use libjxl.NET or ImageSharp JXL plugin as a workaround** — workaround, confidence 0.85 (85%), cost/s, validated=untested
   - For users needing JXL today, decode JXL to raw pixels using a third-party library, then load into SkiaSharp via SKBitmap.InstallPixels or SKImage.FromPixels.

**Recommended proposal:** Verify Skia JXL build support and expose existing decode path

**Why:** Fastest path to partial value delivery — if Skia builds with JXL decode already enabled, decoding JXL images may work without any code changes.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.85 (85%) |
| Reason | Valid feature request with community interest. SKEncodedImageFormat.Jpegxl already exists in the enum, indicating partial upstream support. Needs investigation of build flags before implementation can begin. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply feature-request, SkiaSharp area, and compatibility tenet labels | labels=type/feature-request, area/SkiaSharp, tenet/compatibility |
| add-comment | medium | 0.85 (85%) | Inform reporter of current state and investigation path | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the request! JPEG-XL support is on our radar.

The `SKEncodedImageFormat.Jpegxl` enum value (= 13) is already present in the generated bindings, which means Skia's C API recognizes JXL as a format. However, SkiaSharp does not yet expose a JXL encoder options class, and `SKPixmap.Encode()` does not currently handle `SKEncodedImageFormat.Jpegxl`.

The next step is to verify whether SkiaSharp's Skia build is compiled with JXL decode/encode enabled (via `skia_use_libjxl_decode` / `skia_use_libjxl_encode` GN flags). If decode is already enabled, `SKImage.FromEncodedData()` may already work with JXL files — worth testing.

For encoding, a new `SKJxlEncoderOptions` class and C API wrapper would be required.

**Workaround for now:** Decode JXL to raw pixels using a third-party library (e.g., a libjxl .NET binding or ImageSharp with JXL support), then load the pixels into SkiaSharp via `SKBitmap.InstallPixels()` or `SKImage.FromPixels()`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2633,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T21:23:08Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "Feature request to add JPEG-XL (JXL) encode/decode support to SkiaSharp, citing Skia's limited JXL support added in milestone 98 and the libjxl library as implementation paths.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "No specific version mentioned. JXL format support requested as new capability.",
      "repoLinks": [
        {
          "url": "https://github.com/HinTak/skia-c-examples/issues/4",
          "description": "External example linking JXL C API usage in Skia"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "SKEncodedImageFormat.Jpegxl = 13 is already in the generated bindings enum, but no encoder wrapper or decode path is exposed in the public C# API."
    }
  },
  "analysis": {
    "summary": "The SKEncodedImageFormat enum already contains Jpegxl = 13 in the auto-generated bindings (from the C API), indicating Skia's C layer recognizes JXL as a format. However, there is no corresponding JXL encoder options class (like SKJpegEncoderOptions), and the SKPixmap.Encode() switch statement does not handle SKEncodedImageFormat.Jpegxl. Full JXL support would require: verifying Skia is built with JXL decode/encode enabled, adding C API encoder/decoder wrappers if not present, and exposing a SKJxlEncoderOptions class in the C# binding.",
    "rationale": "Clear feature request with some upstream Skia support already in place (enum value generated from the C API). The reporter correctly identifies that Skia added limited JXL support in milestone 98. Community interest confirmed by comments and reactions. No existing JXL encoder wrapper exists in SKPixmap.cs or SKImage.cs. This is a genuine missing feature, not a bug.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "20617-20647",
        "finding": "SKEncodedImageFormat enum includes Jpegxl = 13, auto-generated from the Skia C API — confirms Skia's C layer already defines JXL as a recognized format identifier.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPixmap.cs",
        "lines": "235-246",
        "finding": "SKPixmap.Encode(SKWStream, SKEncodedImageFormat, int) switch handles only Jpeg, Png, and Webp. The default case returns false — JXL encoding via this path is silently unsupported.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "360-363",
        "finding": "SKImage.Encode() delegates to SKEncodedImageFormat.Png by default; no JXL-specific overload exists.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "C# binding of libjxl or it seems that Skia added limited JPEGXL support in milestone 98",
        "source": "issue body",
        "interpretation": "Reporter is aware of two possible implementation paths: direct libjxl binding or surfacing existing Skia JXL codec."
      },
      {
        "text": "At the moment, JXL provides the best lossless image compression ratio. The encoder has very few parameters, so it won't be hard to add a wrapper.",
        "source": "comment by SladeThe",
        "interpretation": "Community interest confirmed; suggests JXL encoder API surface is small and wrappable."
      },
      {
        "text": "See also https://github.com/HinTak/skia-c-examples/issues/4",
        "source": "comment by HinTak",
        "interpretation": "External reference to Skia JXL C API usage — confirms the underlying Skia capability exists at least experimentally."
      }
    ],
    "nextQuestions": [
      "Is JXL decode/encode actually enabled in SkiaSharp's Skia build (check build flags / GN args for skia_use_libjxl_decode / skia_use_libjxl_encode)?",
      "Does the current Skia milestone used by SkiaSharp ship JXL decode support in the C API (sk_codec_t / sk_image_t paths)?",
      "Is there a sk_jpegxl_encoder_* C API function in externals/skia/src/c/ that SkiaSharp could wrap?"
    ],
    "workarounds": [
      "Convert JXL to another format server-side before passing to SkiaSharp (e.g., using libjxl.NET or ImageSharp with JXL plugin).",
      "Use the libjxl NuGet package directly for JXL encode/decode if available, then convert the decoded pixels to an SKBitmap via SKBitmap.FromImage or SKPixmap."
    ],
    "resolution": {
      "hypothesis": "Skia already defines JXL as format 13 in the C API enum, but SkiaSharp does not expose a JXL encoder options class or hook it into the Encode() path. Implementation requires confirming Skia build flags enable JXL, then adding a SKJxlEncoderOptions type and wiring it into SKPixmap.Encode().",
      "proposals": [
        {
          "title": "Verify Skia JXL build support and expose existing decode path",
          "description": "Check SkiaSharp's GN build args for skia_use_libjxl_decode. If enabled, SKCodec already auto-detects JXL via SKEncodedImageFormat.Jpegxl — no decode-side wrapper needed beyond verifying SKImage.FromEncodedData() works with JXL files.",
          "category": "investigation",
          "confidence": 0.75,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Add SKJxlEncoderOptions and wire into SKPixmap.Encode()",
          "description": "Add a SKJxlEncoderOptions struct mirroring SKWebpEncoderOptions, expose a sk_jpegxl_encoder C API (if missing), bind it, and add an Encode(SKWStream, SKJxlEncoderOptions) overload to SKPixmap. Also handle SKEncodedImageFormat.Jpegxl in the switch statement.",
          "category": "fix",
          "confidence": 0.65,
          "effort": "cost/l",
          "validated": "untested"
        },
        {
          "title": "Use libjxl.NET or ImageSharp JXL plugin as a workaround",
          "description": "For users needing JXL today, decode JXL to raw pixels using a third-party library, then load into SkiaSharp via SKBitmap.InstallPixels or SKImage.FromPixels.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Verify Skia JXL build support and expose existing decode path",
      "recommendedReason": "Fastest path to partial value delivery — if Skia builds with JXL decode already enabled, decoding JXL images may work without any code changes."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.85,
      "reason": "Valid feature request with community interest. SKEncodedImageFormat.Jpegxl already exists in the enum, indicating partial upstream support. Needs investigation of build flags before implementation can begin.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, SkiaSharp area, and compatibility tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Inform reporter of current state and investigation path",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the request! JPEG-XL support is on our radar.\n\nThe `SKEncodedImageFormat.Jpegxl` enum value (= 13) is already present in the generated bindings, which means Skia's C API recognizes JXL as a format. However, SkiaSharp does not yet expose a JXL encoder options class, and `SKPixmap.Encode()` does not currently handle `SKEncodedImageFormat.Jpegxl`.\n\nThe next step is to verify whether SkiaSharp's Skia build is compiled with JXL decode/encode enabled (via `skia_use_libjxl_decode` / `skia_use_libjxl_encode` GN flags). If decode is already enabled, `SKImage.FromEncodedData()` may already work with JXL files — worth testing.\n\nFor encoding, a new `SKJxlEncoderOptions` class and C API wrapper would be required.\n\n**Workaround for now:** Decode JXL to raw pixels using a third-party library (e.g., a libjxl .NET binding or ImageSharp with JXL support), then load the pixels into SkiaSharp via `SKBitmap.InstallPixels()` or `SKImage.FromPixels()`."
      }
    ]
  }
}
```

</details>
