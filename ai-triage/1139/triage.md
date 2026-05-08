# Issue Triage Report — #1139

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T23:26:00Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** Feature request to expose XMP/EXIF metadata writing in SKJpegEncoderOptions when encoding JPEG images via SKPixmap.Encode(), with a community comment extending the request to PNG as well.

**Analysis:** The reporter wants to embed XMP metadata (e.g., equirectangular panorama projection type) in JPEG images at encode time. The Skia native layer already contains an xmpMetadata field in the JPEG encoder options internal struct, but the public SKJpegEncoderOptions C# wrapper does not expose it. Implementing this feature only requires adding a public property and constructor overload to SKJpegEncoderOptions in Definitions.cs.

**Recommendations:** **needs-investigation** — Well-scoped feature request with a clear implementation path. The native Skia struct already has xmpMetadata; only the C# wrapper needs updating. Needs design confirmation (SKData vs byte[] API surface) before implementation.

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

## Evidence

### Reproduction

**Environment:** No platform specified; encoder API is cross-platform

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1139#issuecomment-1588939014 — Community bump comment (Jun 2023)
- https://github.com/mono/SkiaSharp/issues/1139#issuecomment-2171754766 — Community comment extending request to PNG metadata (Jun 2024)

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The public SKJpegEncoderOptions struct still does not expose xmpMetadata as of the current codebase. The native layer already supports it but C# does not expose it. |

## Analysis

### Technical Summary

The reporter wants to embed XMP metadata (e.g., equirectangular panorama projection type) in JPEG images at encode time. The Skia native layer already contains an xmpMetadata field in the JPEG encoder options internal struct, but the public SKJpegEncoderOptions C# wrapper does not expose it. Implementing this feature only requires adding a public property and constructor overload to SKJpegEncoderOptions in Definitions.cs.

### Rationale

This is a feature request: the reporter is asking for new public API surface to write XMP metadata to JPEG files. The underlying native capability already exists in Skia (xmpMetadata field in internal struct), so this is purely a C# binding gap. Implementation is low-risk and well-scoped: add a public XmpMetadata property to SKJpegEncoderOptions using a new constructor overload (to preserve ABI stability). The request has 19 +1 votes demonstrating significant community interest.

### Key Signals

- "Add a dictionary property to SKJpegEncoderOptions to enable setting metadata properties when encoding JPEG images" — **issue body** (Reporter wants XMP metadata injection at encode time, specifically for panorama projection metadata)
- "PNG format has metadata as well which should be writable." — **comment by BenMcLean** (Community interest extends to PNG; SKPngEncoderOptions also has an fComments field that is not publicly exposed)
- "private readonly sk_data_t xmpMetadata (in SKJpegEncoderOptionsInternal)" — **binding/SkiaSharp/SkiaApi.generated.cs** (Native Skia already supports XMP metadata in JPEG encoding. The C# binding just needs to expose it.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/Definitions.cs` | 545-585 | direct | SKJpegEncoderOptions public struct initializes xmpMetadata to default in both constructors but does not expose a public property for it. Only Quality, Downsample, and AlphaOption are exposed. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 19125-19126 | direct | The internal struct (generated from native headers) already contains: '// public const sk_data_t* xmpMetadata' and 'private readonly sk_data_t xmpMetadata'. This confirms native Skia supports XMP metadata in JPEG encoding. |
| `binding/SkiaSharp/SKPixmap.cs` | — | related | SKPixmap.Encode(SKJpegEncoderOptions) overloads exist and pass the struct directly to native code. The xmpMetadata field would be picked up automatically once exposed. |

### Workarounds

- Post-process the JPEG output with a third-party metadata library such as MetadataExtractor (read) or ExifLibrary/LibXmp to inject XMP data after SkiaSharp encodes the image.
- For the panorama use case specifically, use exiftool externally to add the XMP tags after SkiaSharp creates the JPEG.

### Resolution Proposals

**Hypothesis:** Add a public XmpMetadata property (typed as SKData?) to SKJpegEncoderOptions in Definitions.cs, wired to the existing private xmpMetadata field. A new constructor overload accepting XmpMetadata should be added to maintain ABI stability.

1. **Expose XmpMetadata property on SKJpegEncoderOptions** — fix, confidence 0.90 (90%), cost/s, validated=untested
   - Add a new constructor overload to SKJpegEncoderOptions that accepts an SKData xmpMetadata parameter, and expose a public XmpMetadata => ... property. The internal struct field already exists in the generated code.
2. **Post-process JPEG with third-party metadata library** — workaround, confidence 0.85 (85%), cost/s, validated=untested
   - Use MetadataExtractor or ExifLibrary after SkiaSharp encodes the JPEG to inject XMP metadata bytes into the output stream. Works today with no SkiaSharp changes.

**Recommended proposal:** Expose XmpMetadata property on SKJpegEncoderOptions

**Why:** The native layer already supports it; exposing the field in C# is low-risk, low-effort, and directly addresses the request. Significant community interest (19 +1s) justifies implementing it.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Well-scoped feature request with a clear implementation path. The native Skia struct already has xmpMetadata; only the C# wrapper needs updating. Needs design confirmation (SKData vs byte[] API surface) before implementation. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply feature-request and core SkiaSharp area labels | labels=type/feature-request, area/SkiaSharp |
| add-comment | medium | 0.88 (88%) | Acknowledge the request, note that native support exists, and describe the implementation path | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the request! The good news is that the underlying Skia native library already supports XMP metadata in JPEG encoding — the `xmpMetadata` field exists in the internal encoder options struct. The gap is that the public `SKJpegEncoderOptions` C# wrapper does not yet expose it.

In the meantime, a workaround is to post-process the JPEG output using a third-party library such as [MetadataExtractor](https://github.com/drewnoakes/metadata-extractor-dotnet) or ExifLibrary to inject XMP metadata after SkiaSharp encodes the image. External tools like `exiftool` also work for batch processing.

We'd welcome a PR that adds an `XmpMetadata` property to `SKJpegEncoderOptions` via a new constructor overload.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1139,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T23:26:00Z"
  },
  "summary": "Feature request to expose XMP/EXIF metadata writing in SKJpegEncoderOptions when encoding JPEG images via SKPixmap.Encode(), with a community comment extending the request to PNG as well.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    }
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "No platform specified; encoder API is cross-platform",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1139#issuecomment-1588939014",
          "description": "Community bump comment (Jun 2023)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1139#issuecomment-2171754766",
          "description": "Community comment extending request to PNG metadata (Jun 2024)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "The public SKJpegEncoderOptions struct still does not expose xmpMetadata as of the current codebase. The native layer already supports it but C# does not expose it."
    }
  },
  "analysis": {
    "summary": "The reporter wants to embed XMP metadata (e.g., equirectangular panorama projection type) in JPEG images at encode time. The Skia native layer already contains an xmpMetadata field in the JPEG encoder options internal struct, but the public SKJpegEncoderOptions C# wrapper does not expose it. Implementing this feature only requires adding a public property and constructor overload to SKJpegEncoderOptions in Definitions.cs.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "545-585",
        "finding": "SKJpegEncoderOptions public struct initializes xmpMetadata to default in both constructors but does not expose a public property for it. Only Quality, Downsample, and AlphaOption are exposed.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "19125-19126",
        "finding": "The internal struct (generated from native headers) already contains: '// public const sk_data_t* xmpMetadata' and 'private readonly sk_data_t xmpMetadata'. This confirms native Skia supports XMP metadata in JPEG encoding.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPixmap.cs",
        "finding": "SKPixmap.Encode(SKJpegEncoderOptions) overloads exist and pass the struct directly to native code. The xmpMetadata field would be picked up automatically once exposed.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Add a dictionary property to SKJpegEncoderOptions to enable setting metadata properties when encoding JPEG images",
        "source": "issue body",
        "interpretation": "Reporter wants XMP metadata injection at encode time, specifically for panorama projection metadata"
      },
      {
        "text": "PNG format has metadata as well which should be writable.",
        "source": "comment by BenMcLean",
        "interpretation": "Community interest extends to PNG; SKPngEncoderOptions also has an fComments field that is not publicly exposed"
      },
      {
        "text": "private readonly sk_data_t xmpMetadata (in SKJpegEncoderOptionsInternal)",
        "source": "binding/SkiaSharp/SkiaApi.generated.cs",
        "interpretation": "Native Skia already supports XMP metadata in JPEG encoding. The C# binding just needs to expose it."
      }
    ],
    "rationale": "This is a feature request: the reporter is asking for new public API surface to write XMP metadata to JPEG files. The underlying native capability already exists in Skia (xmpMetadata field in internal struct), so this is purely a C# binding gap. Implementation is low-risk and well-scoped: add a public XmpMetadata property to SKJpegEncoderOptions using a new constructor overload (to preserve ABI stability). The request has 19 +1 votes demonstrating significant community interest.",
    "workarounds": [
      "Post-process the JPEG output with a third-party metadata library such as MetadataExtractor (read) or ExifLibrary/LibXmp to inject XMP data after SkiaSharp encodes the image.",
      "For the panorama use case specifically, use exiftool externally to add the XMP tags after SkiaSharp creates the JPEG."
    ],
    "resolution": {
      "hypothesis": "Add a public XmpMetadata property (typed as SKData?) to SKJpegEncoderOptions in Definitions.cs, wired to the existing private xmpMetadata field. A new constructor overload accepting XmpMetadata should be added to maintain ABI stability.",
      "proposals": [
        {
          "title": "Expose XmpMetadata property on SKJpegEncoderOptions",
          "description": "Add a new constructor overload to SKJpegEncoderOptions that accepts an SKData xmpMetadata parameter, and expose a public XmpMetadata => ... property. The internal struct field already exists in the generated code.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Post-process JPEG with third-party metadata library",
          "description": "Use MetadataExtractor or ExifLibrary after SkiaSharp encodes the JPEG to inject XMP metadata bytes into the output stream. Works today with no SkiaSharp changes.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Expose XmpMetadata property on SKJpegEncoderOptions",
      "recommendedReason": "The native layer already supports it; exposing the field in C# is low-risk, low-effort, and directly addresses the request. Significant community interest (19 +1s) justifies implementing it."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Well-scoped feature request with a clear implementation path. The native Skia struct already has xmpMetadata; only the C# wrapper needs updating. Needs design confirmation (SKData vs byte[] API surface) before implementation.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request and core SkiaSharp area labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the request, note that native support exists, and describe the implementation path",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the request! The good news is that the underlying Skia native library already supports XMP metadata in JPEG encoding — the `xmpMetadata` field exists in the internal encoder options struct. The gap is that the public `SKJpegEncoderOptions` C# wrapper does not yet expose it.\n\nIn the meantime, a workaround is to post-process the JPEG output using a third-party library such as [MetadataExtractor](https://github.com/drewnoakes/metadata-extractor-dotnet) or ExifLibrary to inject XMP metadata after SkiaSharp encodes the image. External tools like `exiftool` also work for batch processing.\n\nWe'd welcome a PR that adds an `XmpMetadata` property to `SKJpegEncoderOptions` via a new constructor overload."
      }
    ]
  }
}
```

</details>
