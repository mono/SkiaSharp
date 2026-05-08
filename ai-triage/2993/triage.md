# Issue Triage Report — #2993

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-27T12:03:08Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** Feature request to add TIFF image format support to SkiaSharp, citing that upstream Skia now provides a TIFF codec (SkTiffUtility) whereas the original 2018 request was closed due to lack of upstream support.

**Analysis:** SkiaSharp does not expose TIFF encode/decode support. The SKEncodedImageFormat enum (in SkiaApi.generated.cs) currently lists BMP, GIF, ICO, JPEG, PNG, WBMP, WebP, PKM, KTX, ASTC, DNG, HEIF, AVIF, and JPEGXL — no TIFF entry. The SKPixmap.Encode dispatcher (SKPixmap.cs) only handles JPEG, PNG, and WebP via specialized encoder options. Upstream Skia has added a TIFF codec (SkTiffUtility.h), but exposing it in SkiaSharp requires: verifying the C API enum has a TIFF entry, adding a TIFF value to SKEncodedImageFormat, and optionally adding a SKTiffEncoderOptions class with a corresponding C API and native rebuild.

**Recommendations:** **needs-investigation** — Feature is well-justified: upstream Skia now has a TIFF codec. Next step is to verify whether the C API enum already exposes TIFF and assess the full implementation scope across native, C API, and C# layers.

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

**Repository links:**
- https://github.com/google/skia/blob/320dccf1a32dbc4d477fdb194515f75731903a6a/src/codec/SkTiffUtility.h — Upstream Skia TIFF codec utility header referenced by reporter
- https://github.com/mono/SkiaSharp/issues/433 — Original 2018 TIFF issue closed because upstream Skia did not support TIFF at the time

## Analysis

### Technical Summary

SkiaSharp does not expose TIFF encode/decode support. The SKEncodedImageFormat enum (in SkiaApi.generated.cs) currently lists BMP, GIF, ICO, JPEG, PNG, WBMP, WebP, PKM, KTX, ASTC, DNG, HEIF, AVIF, and JPEGXL — no TIFF entry. The SKPixmap.Encode dispatcher (SKPixmap.cs) only handles JPEG, PNG, and WebP via specialized encoder options. Upstream Skia has added a TIFF codec (SkTiffUtility.h), but exposing it in SkiaSharp requires: verifying the C API enum has a TIFF entry, adding a TIFF value to SKEncodedImageFormat, and optionally adding a SKTiffEncoderOptions class with a corresponding C API and native rebuild.

### Rationale

This is a clearly-scoped feature request with upstream Skia now providing a TIFF codec. The original blocker (no Skia support) is resolved. Implementation requires changes at all three layers: native C API (expose TIFF encoder/decoder enum value and encode function), C# binding (new enum value, encoder options class, Encode dispatcher update), and potential native rebuild. No existing workaround in SkiaSharp; users must use a separate TIFF library.

### Key Signals

- "Issue #433 was closed in 2018 for Skia not supporting TIFF. This doesn't seem the case anymore" — **issue body** (Reporter correctly identifies that the blocker (no upstream Skia TIFF codec) no longer applies, making this a valid re-evaluation of a previously-closed request.)
- "The only problematic part... is that tiff supports a wide variety of pixel formats so it might take some effort to get full support for all of those." — **comment by Tampa** (Community member correctly anticipates the non-trivial scope: TIFF pixel format diversity means a full implementation would need careful mapping to SKColorType/SKAlphaType.)
- "Is there any latest development now?" — **comment by LUJIAN2020 (2026-04-22)** (Recent community interest confirms the request is still relevant.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaApi.generated.cs` | 20822-20851 | direct | SKEncodedImageFormat enum lists 14 formats (BMP through JPEGXL, values 0-13) — TIFF is absent. Adding TIFF requires a new enum value and corresponding C API enum update in the native layer. |
| `binding/SkiaSharp/SKPixmap.cs` | 235-246 | direct | The Encode(SKWStream, SKEncodedImageFormat, int) dispatcher uses a switch expression handling only SKEncodedImageFormat.Jpeg, .Png, and .Webp. A TIFF encoder path would need to be added here once a SKTiffEncoderOptions class and native C API support are in place. |

### Workarounds

- Use a dedicated .NET TIFF library (e.g. Magick.NET, LibTiff.NET, or System.Drawing on Windows) to decode TIFF to raw pixels, then create an SKBitmap from the pixel pointer using SKBitmap.InstallPixels or SKBitmap.FromImage.
- For encoding: render to SKBitmap, extract pixel bytes via SKPixmap, and write a TIFF using a third-party library.

### Resolution Proposals

**Hypothesis:** Upstream Skia has the TIFF codec available; the work is to expose it through the SkiaSharp C API shim, regenerate bindings, add a TIFF value to SKEncodedImageFormat, and implement encode/decode paths in C#.

1. **Expose TIFF via existing codec infrastructure** — fix, confidence 0.70 (70%), cost/l, validated=untested
   - Check whether the upstream sk_encoded_image_format_t C enum already has a TIFF entry; if so, regenerate bindings to surface it in SKEncodedImageFormat. Then add a TIFF decode path (SKCodec already reads any supported format) and a SKTiffEncoderOptions + C API encode function for writing TIFF.
2. **Workaround via third-party TIFF library** — workaround, confidence 0.90 (90%), cost/s, validated=untested
   - Use LibTiff.NET or Magick.NET to decode TIFF bytes to a raw RGBA array, then wrap with SKBitmap.InstallPixels for use in SkiaSharp drawing. Encoding is the reverse: copy SKPixmap pixels to a byte array and write TIFF via the third-party library.

**Recommended proposal:** Workaround via third-party TIFF library

**Why:** Provides an immediate path for users while the native implementation work is scoped and prioritised.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Feature is well-justified: upstream Skia now has a TIFF codec. Next step is to verify whether the C API enum already exposes TIFF and assess the full implementation scope across native, C API, and C# layers. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Confirm type/feature-request label already present; area/SkiaSharp is not yet set | labels=type/feature-request, area/SkiaSharp |
| add-comment | medium | 0.82 (82%) | Acknowledge the upstream change, describe scope, and provide workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for reviving this! You're right that upstream Skia now includes a TIFF codec (`SkTiffUtility`) which was the original blocker.

Here's the current state in SkiaSharp:
- `SKEncodedImageFormat` does **not** yet have a `Tiff` value — it would need to be added to the C API enum and the bindings regenerated.
- `SKPixmap.Encode()` only dispatches for JPEG, PNG, and WebP — a TIFF path would need a new `SKTiffEncoderOptions` class and native C API function.
- `SKCodec` (decode path) already reads any format the native library supports, so decoding may work once the native build includes the TIFF codec.

Note that TIFF supports a wide range of pixel formats (8/16/32-bit, float, CMYK, etc.) so full round-trip support will need careful bit-depth mapping.

**Workaround (decode):** Use [LibTiff.NET](https://bitmiracle.com/libtiff/) or [Magick.NET](https://github.com/dlemstra/Magick.NET) to decode TIFF to raw RGBA bytes, then pass to `SKBitmap.InstallPixels` or `SKBitmap.FromImage`.

**Workaround (encode):** Render to `SKBitmap`, copy pixels via `SKPixmap`, and write TIFF with a third-party library.

We're tracking this for a future release — 10 👍 confirms there's community demand.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2993,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-27T12:03:08Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "Feature request to add TIFF image format support to SkiaSharp, citing that upstream Skia now provides a TIFF codec (SkTiffUtility) whereas the original 2018 request was closed due to lack of upstream support.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    }
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/google/skia/blob/320dccf1a32dbc4d477fdb194515f75731903a6a/src/codec/SkTiffUtility.h",
          "description": "Upstream Skia TIFF codec utility header referenced by reporter"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/433",
          "description": "Original 2018 TIFF issue closed because upstream Skia did not support TIFF at the time"
        }
      ]
    }
  },
  "analysis": {
    "summary": "SkiaSharp does not expose TIFF encode/decode support. The SKEncodedImageFormat enum (in SkiaApi.generated.cs) currently lists BMP, GIF, ICO, JPEG, PNG, WBMP, WebP, PKM, KTX, ASTC, DNG, HEIF, AVIF, and JPEGXL — no TIFF entry. The SKPixmap.Encode dispatcher (SKPixmap.cs) only handles JPEG, PNG, and WebP via specialized encoder options. Upstream Skia has added a TIFF codec (SkTiffUtility.h), but exposing it in SkiaSharp requires: verifying the C API enum has a TIFF entry, adding a TIFF value to SKEncodedImageFormat, and optionally adding a SKTiffEncoderOptions class with a corresponding C API and native rebuild.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "20822-20851",
        "finding": "SKEncodedImageFormat enum lists 14 formats (BMP through JPEGXL, values 0-13) — TIFF is absent. Adding TIFF requires a new enum value and corresponding C API enum update in the native layer.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPixmap.cs",
        "lines": "235-246",
        "finding": "The Encode(SKWStream, SKEncodedImageFormat, int) dispatcher uses a switch expression handling only SKEncodedImageFormat.Jpeg, .Png, and .Webp. A TIFF encoder path would need to be added here once a SKTiffEncoderOptions class and native C API support are in place.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Issue #433 was closed in 2018 for Skia not supporting TIFF. This doesn't seem the case anymore",
        "source": "issue body",
        "interpretation": "Reporter correctly identifies that the blocker (no upstream Skia TIFF codec) no longer applies, making this a valid re-evaluation of a previously-closed request."
      },
      {
        "text": "The only problematic part... is that tiff supports a wide variety of pixel formats so it might take some effort to get full support for all of those.",
        "source": "comment by Tampa",
        "interpretation": "Community member correctly anticipates the non-trivial scope: TIFF pixel format diversity means a full implementation would need careful mapping to SKColorType/SKAlphaType."
      },
      {
        "text": "Is there any latest development now?",
        "source": "comment by LUJIAN2020 (2026-04-22)",
        "interpretation": "Recent community interest confirms the request is still relevant."
      }
    ],
    "rationale": "This is a clearly-scoped feature request with upstream Skia now providing a TIFF codec. The original blocker (no Skia support) is resolved. Implementation requires changes at all three layers: native C API (expose TIFF encoder/decoder enum value and encode function), C# binding (new enum value, encoder options class, Encode dispatcher update), and potential native rebuild. No existing workaround in SkiaSharp; users must use a separate TIFF library.",
    "workarounds": [
      "Use a dedicated .NET TIFF library (e.g. Magick.NET, LibTiff.NET, or System.Drawing on Windows) to decode TIFF to raw pixels, then create an SKBitmap from the pixel pointer using SKBitmap.InstallPixels or SKBitmap.FromImage.",
      "For encoding: render to SKBitmap, extract pixel bytes via SKPixmap, and write a TIFF using a third-party library."
    ],
    "resolution": {
      "hypothesis": "Upstream Skia has the TIFF codec available; the work is to expose it through the SkiaSharp C API shim, regenerate bindings, add a TIFF value to SKEncodedImageFormat, and implement encode/decode paths in C#.",
      "proposals": [
        {
          "title": "Expose TIFF via existing codec infrastructure",
          "description": "Check whether the upstream sk_encoded_image_format_t C enum already has a TIFF entry; if so, regenerate bindings to surface it in SKEncodedImageFormat. Then add a TIFF decode path (SKCodec already reads any supported format) and a SKTiffEncoderOptions + C API encode function for writing TIFF.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/l",
          "validated": "untested"
        },
        {
          "title": "Workaround via third-party TIFF library",
          "description": "Use LibTiff.NET or Magick.NET to decode TIFF bytes to a raw RGBA array, then wrap with SKBitmap.InstallPixels for use in SkiaSharp drawing. Encoding is the reverse: copy SKPixmap pixels to a byte array and write TIFF via the third-party library.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Workaround via third-party TIFF library",
      "recommendedReason": "Provides an immediate path for users while the native implementation work is scoped and prioritised."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Feature is well-justified: upstream Skia now has a TIFF codec. Next step is to verify whether the C API enum already exposes TIFF and assess the full implementation scope across native, C API, and C# layers.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Confirm type/feature-request label already present; area/SkiaSharp is not yet set",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the upstream change, describe scope, and provide workaround",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for reviving this! You're right that upstream Skia now includes a TIFF codec (`SkTiffUtility`) which was the original blocker.\n\nHere's the current state in SkiaSharp:\n- `SKEncodedImageFormat` does **not** yet have a `Tiff` value — it would need to be added to the C API enum and the bindings regenerated.\n- `SKPixmap.Encode()` only dispatches for JPEG, PNG, and WebP — a TIFF path would need a new `SKTiffEncoderOptions` class and native C API function.\n- `SKCodec` (decode path) already reads any format the native library supports, so decoding may work once the native build includes the TIFF codec.\n\nNote that TIFF supports a wide range of pixel formats (8/16/32-bit, float, CMYK, etc.) so full round-trip support will need careful bit-depth mapping.\n\n**Workaround (decode):** Use [LibTiff.NET](https://bitmiracle.com/libtiff/) or [Magick.NET](https://github.com/dlemstra/Magick.NET) to decode TIFF to raw RGBA bytes, then pass to `SKBitmap.InstallPixels` or `SKBitmap.FromImage`.\n\n**Workaround (encode):** Render to `SKBitmap`, copy pixels via `SKPixmap`, and write TIFF with a third-party library.\n\nWe're tracking this for a future release — 10 👍 confirms there's community demand."
      }
    ]
  }
}
```

</details>
