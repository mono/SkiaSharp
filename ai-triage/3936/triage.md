# Issue Triage Report — #3936

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-18T05:35:46Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp (0.98 (98%)) |
| Suggested action | ready-to-fix (0.95 (95%)) |

**Issue Summary:** Feature request to expose JPEG XMP metadata and EXIF orientation fields in SKJpegEncoderOptions — the underlying C API fields already exist but are not surfaced publicly in C#.

**Analysis:** The C API struct sk_jpegencoder_options_t already contains xmpMetadata, fOrigin, and fHasOrigin fields, and the generated C# struct SKJpegEncoderOptions mirrors those as private readonly fields. However, the handwritten partial struct in Definitions.cs only exposes Quality, Downsample, and AlphaOption — leaving XMP metadata and EXIF orientation inaccessible to callers. The fix is a pure C# change: add public XmpMetadata (SKData?) and Origin (SKEncodedOrigin?) properties plus updated constructors. SKCodec.EncodedOrigin already exists for reading orientation on decode.

**Recommendations:** **ready-to-fix** — All underlying infrastructure exists (C API struct fields, SKEncodedOrigin enum, decode-side SKCodec.EncodedOrigin). The fix is a pure C# wrapper change with a clear, well-scoped implementation path. Already milestone-targeted for 4.x RC 1.

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

## Evidence

### Reproduction

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The C# struct SKJpegEncoderOptions still lacks public XmpMetadata and Origin properties as of current code; the fields exist privately in the generated struct. |

## Analysis

### Technical Summary

The C API struct sk_jpegencoder_options_t already contains xmpMetadata, fOrigin, and fHasOrigin fields, and the generated C# struct SKJpegEncoderOptions mirrors those as private readonly fields. However, the handwritten partial struct in Definitions.cs only exposes Quality, Downsample, and AlphaOption — leaving XMP metadata and EXIF orientation inaccessible to callers. The fix is a pure C# change: add public XmpMetadata (SKData?) and Origin (SKEncodedOrigin?) properties plus updated constructors. SKCodec.EncodedOrigin already exists for reading orientation on decode.

### Rationale

Classified as type/feature-request because the underlying C API already exposes the fields — this is purely a C# wrapper gap, not a missing C++ or C API feature. Area is area/SkiaSharp because the change is in the core binding. No platform-specific behavior is involved. Ready-to-fix because the implementation path is fully specified: add public properties to SKJpegEncoderOptions, add constructor overload, and validate with round-trip tests. No native rebuild required.

### Key Signals

- "Pure C# change — no native/C API work needed." — **issue body** (Confirms this is a safe, low-risk addition that only touches the managed wrapper layer.)
- "This is the #1 most-voted codec feature request (👍31 combined)." — **issue body** (High community demand across issues #1139 and #2517 makes this a strong candidate for inclusion in v4.)
- "Evaluate against time constraints and value… Part of #3680" — **issue body** (Already in the v4 RC 1 milestone scope and linked to the API evaluation tracking issue.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/Definitions.cs` | 546-582 | direct | SKJpegEncoderOptions partial struct exposes Quality, Downsample, AlphaOption but NOT xmpMetadata, fOrigin, or fHasOrigin. Those fields are present as private readonly in the generated struct. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 19587-19630 | direct | sk_jpegencoder_options_t maps to a generated struct with private fields: xmpMetadata (sk_data_t), fOrigin (Int32), fHasOrigin (Byte). All three are in-struct already. |
| `binding/SkiaSharp/SKCodec.cs` | 36-37 | related | SKCodec.EncodedOrigin property already exists, returning SKEncodedOrigin from sk_codec_get_origin — decode-side origin reading is already public. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 21135 | context | SKEncodedOrigin enum is already defined in the generated bindings — can be used directly as the type for the new Origin property. |

### Resolution Proposals

**Hypothesis:** Add XmpMetadata (SKData?) and Origin (SKEncodedOrigin?) public properties to the handwritten part of SKJpegEncoderOptions in Definitions.cs, add constructor overloads that accept these parameters, and write round-trip encode/decode tests.

1. **Expose XmpMetadata and Origin on SKJpegEncoderOptions** — fix, validated=untested
   - Add public XmpMetadata (SKData?) and Origin (SKEncodedOrigin?) properties to SKJpegEncoderOptions in Definitions.cs. Add constructor overloads that accept these parameters. Write round-trip encode/decode tests verifying orientation is preserved.

**Recommended proposal:** Expose XmpMetadata and Origin on SKJpegEncoderOptions

**Why:** The implementation path is fully clear — private fields already exist in the generated struct, SKEncodedOrigin enum already exists, and SKCodec.EncodedOrigin for reading already works. This is a minimal, targeted C# change with strong community demand.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.95 (95%) |
| Reason | All underlying infrastructure exists (C API struct fields, SKEncodedOrigin enum, decode-side SKCodec.EncodedOrigin). The fix is a pure C# wrapper change with a clear, well-scoped implementation path. Already milestone-targeted for 4.x RC 1. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply type/feature-request, area/SkiaSharp, tenet/compatibility | labels=type/feature-request, area/SkiaSharp, tenet/compatibility |
| add-comment | medium | 0.90 (90%) | Acknowledge the request, confirm the implementation path, and link to related issues. | — |
| link-related | low | 0.98 (98%) | Cross-reference parent tracking issue #3680 | linkedIssue=#3680 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed write-up! The implementation path looks clear:

- `SKJpegEncoderOptions` already has `xmpMetadata`, `fOrigin`, and `fHasOrigin` as private fields in the generated struct — they just need to be surfaced publicly.
- `SKEncodedOrigin` is already defined and `SKCodec.EncodedOrigin` already works for decode-side reading.
- This is a pure C# change, no native/C API work needed.

This partially resolves #1139 (JPEG metadata on encode) and #2517 (EXIF read/write). Related to #2280 (resize loses orientation).
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3936,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-18T05:35:46Z"
  },
  "summary": "Feature request to expose JPEG XMP metadata and EXIF orientation fields in SKJpegEncoderOptions — the underlying C API fields already exist but are not surfaced publicly in C#.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.98
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "The C# struct SKJpegEncoderOptions still lacks public XmpMetadata and Origin properties as of current code; the fields exist privately in the generated struct."
    }
  },
  "analysis": {
    "summary": "The C API struct sk_jpegencoder_options_t already contains xmpMetadata, fOrigin, and fHasOrigin fields, and the generated C# struct SKJpegEncoderOptions mirrors those as private readonly fields. However, the handwritten partial struct in Definitions.cs only exposes Quality, Downsample, and AlphaOption — leaving XMP metadata and EXIF orientation inaccessible to callers. The fix is a pure C# change: add public XmpMetadata (SKData?) and Origin (SKEncodedOrigin?) properties plus updated constructors. SKCodec.EncodedOrigin already exists for reading orientation on decode.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "546-582",
        "finding": "SKJpegEncoderOptions partial struct exposes Quality, Downsample, AlphaOption but NOT xmpMetadata, fOrigin, or fHasOrigin. Those fields are present as private readonly in the generated struct.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "19587-19630",
        "finding": "sk_jpegencoder_options_t maps to a generated struct with private fields: xmpMetadata (sk_data_t), fOrigin (Int32), fHasOrigin (Byte). All three are in-struct already.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "36-37",
        "finding": "SKCodec.EncodedOrigin property already exists, returning SKEncodedOrigin from sk_codec_get_origin — decode-side origin reading is already public.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "21135",
        "finding": "SKEncodedOrigin enum is already defined in the generated bindings — can be used directly as the type for the new Origin property.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "Pure C# change — no native/C API work needed.",
        "source": "issue body",
        "interpretation": "Confirms this is a safe, low-risk addition that only touches the managed wrapper layer."
      },
      {
        "text": "This is the #1 most-voted codec feature request (👍31 combined).",
        "source": "issue body",
        "interpretation": "High community demand across issues #1139 and #2517 makes this a strong candidate for inclusion in v4."
      },
      {
        "text": "Evaluate against time constraints and value… Part of #3680",
        "source": "issue body",
        "interpretation": "Already in the v4 RC 1 milestone scope and linked to the API evaluation tracking issue."
      }
    ],
    "rationale": "Classified as type/feature-request because the underlying C API already exposes the fields — this is purely a C# wrapper gap, not a missing C++ or C API feature. Area is area/SkiaSharp because the change is in the core binding. No platform-specific behavior is involved. Ready-to-fix because the implementation path is fully specified: add public properties to SKJpegEncoderOptions, add constructor overload, and validate with round-trip tests. No native rebuild required.",
    "resolution": {
      "hypothesis": "Add XmpMetadata (SKData?) and Origin (SKEncodedOrigin?) public properties to the handwritten part of SKJpegEncoderOptions in Definitions.cs, add constructor overloads that accept these parameters, and write round-trip encode/decode tests.",
      "proposals": [
        {
          "title": "Expose XmpMetadata and Origin on SKJpegEncoderOptions",
          "category": "fix",
          "validated": "untested",
          "description": "Add public XmpMetadata (SKData?) and Origin (SKEncodedOrigin?) properties to SKJpegEncoderOptions in Definitions.cs. Add constructor overloads that accept these parameters. Write round-trip encode/decode tests verifying orientation is preserved."
        }
      ],
      "recommendedProposal": "Expose XmpMetadata and Origin on SKJpegEncoderOptions",
      "recommendedReason": "The implementation path is fully clear — private fields already exist in the generated struct, SKEncodedOrigin enum already exists, and SKCodec.EncodedOrigin for reading already works. This is a minimal, targeted C# change with strong community demand."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.95,
      "reason": "All underlying infrastructure exists (C API struct fields, SKEncodedOrigin enum, decode-side SKCodec.EncodedOrigin). The fix is a pure C# wrapper change with a clear, well-scoped implementation path. Already milestone-targeted for 4.x RC 1.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/feature-request, area/SkiaSharp, tenet/compatibility",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the request, confirm the implementation path, and link to related issues.",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the detailed write-up! The implementation path looks clear:\n\n- `SKJpegEncoderOptions` already has `xmpMetadata`, `fOrigin`, and `fHasOrigin` as private fields in the generated struct — they just need to be surfaced publicly.\n- `SKEncodedOrigin` is already defined and `SKCodec.EncodedOrigin` already works for decode-side reading.\n- This is a pure C# change, no native/C API work needed.\n\nThis partially resolves #1139 (JPEG metadata on encode) and #2517 (EXIF read/write). Related to #2280 (resize loses orientation)."
      },
      {
        "type": "link-related",
        "description": "Cross-reference parent tracking issue #3680",
        "risk": "low",
        "confidence": 0.98,
        "linkedIssue": 3680
      }
    ]
  }
}
```

</details>
