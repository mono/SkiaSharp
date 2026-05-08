# Issue Triage Report — #3176

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T03:30:00Z |
| Type | type/feature-request (0.95 (95%)) |
| Area | area/SkiaSharp (0.85 (85%)) |
| Suggested action | close-as-external (0.82 (82%)) |

**Issue Summary:** Reporter requests the ability to access raw (pre-debayered) pixels from DNG camera RAW files, which Skia/SkiaSharp does not currently support.

**Analysis:** The reporter wants to read the raw Bayer sensor data from a DNG file before any demosaicing or post-processing is applied. SkiaSharp/Skia recognizes DNG as an encoded image format (SKEncodedImageFormat.Dng = 10) and can decode DNG files to RGB via SKCodec.GetPixels(), but Skia processes the DNG to a standard decoded RGB/RGBA bitmap during decode. There is no API surface in SkiaSharp or Skia to access the pre-demosaicing raw sensor values. This functionality requires a specialized RAW processing library such as LibRaw.

**Recommendations:** **close-as-external** — Pre-debayering DNG pixel access is outside Skia's scope as a 2D rendering library. Skia decodes DNG to standard RGB but does not expose pre-demosaicing sensor data. LibRaw is the appropriate library for this use case.

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

**Environment:** No specific platform or version mentioned

## Analysis

### Technical Summary

The reporter wants to read the raw Bayer sensor data from a DNG file before any demosaicing or post-processing is applied. SkiaSharp/Skia recognizes DNG as an encoded image format (SKEncodedImageFormat.Dng = 10) and can decode DNG files to RGB via SKCodec.GetPixels(), but Skia processes the DNG to a standard decoded RGB/RGBA bitmap during decode. There is no API surface in SkiaSharp or Skia to access the pre-demosaicing raw sensor values. This functionality requires a specialized RAW processing library such as LibRaw.

### Rationale

This is a feature request to access pre-debayered RAW sensor data from DNG files. SkiaSharp wraps Skia which is a 2D rendering/compositing library — it decodes image formats to displayable bitmaps but does not expose the underlying raw sensor data. Accessing pre-demosaicing Bayer pixels from DNG is outside the scope of a graphics rendering library and is handled by specialized libraries. The request is clearly for new, out-of-scope functionality.

### Key Signals

- "I would like to access to raw pixels of a DNG image (before debayering and post processing)" — **issue body** (This is a request for pre-demosaicing Bayer pattern data — a specialized RAW processing feature beyond Skia's scope as a rendering library.)
- "There aren't easy alternatives" — **issue body** (Reporter has already considered alternatives, but specialized RAW libraries like LibRaw exist for this exact purpose.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaApi.generated.cs` | 21029-21051 | direct | SKEncodedImageFormat.Dng = 10 is defined, confirming SkiaSharp recognizes DNG as an encoded format. However, this is only used for identifying the format after decode — no special Bayer/raw pixel access API exists. |
| `binding/SkiaSharp/SKCodec.cs` | 56-64 | direct | SKCodec.GetPixels() decodes a DNG to a standard bitmap (RGB/RGBA). The decoded result is the fully processed image, not pre-debayering raw sensor data. No method exists for accessing pre-demosaiced pixels. |

### Workarounds

- Use LibRaw (https://www.libraw.org/) for .NET via P/Invoke or via third-party NuGet wrappers to access pre-debayered DNG pixel data.
- Use the dcraw command-line tool to extract raw sensor data from DNG files.

### Resolution Proposals

**Hypothesis:** Accessing pre-debayered DNG pixels requires specialized RAW processing capabilities that Skia/SkiaSharp does not and is unlikely to expose, as it is a rendering library not a RAW processing library.

1. **Use LibRaw for pre-debayering access** — alternative, confidence 0.90 (90%), cost/m, validated=untested
   - LibRaw is a C library specifically designed for reading RAW camera files including DNG. For .NET, the reporter can P/Invoke into LibRaw directly, or use a NuGet wrapper such as LibRawNet. This gives full access to the pre-demosaicing Bayer pattern data.

**Recommended proposal:** Use LibRaw for pre-debayering access

**Why:** LibRaw is purpose-built for exactly this use case. This feature is outside Skia's design scope.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.82 (82%) |
| Reason | Pre-debayering DNG pixel access is outside Skia's scope as a 2D rendering library. Skia decodes DNG to standard RGB but does not expose pre-demosaicing sensor data. LibRaw is the appropriate library for this use case. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply feature-request and SkiaSharp area labels | labels=type/feature-request, area/SkiaSharp |
| add-comment | high | 0.82 (82%) | Explain that pre-debayering DNG access is out of Skia scope and suggest LibRaw as an alternative | — |
| close-issue | medium | 0.78 (78%) | Close as external — feature is out of scope for Skia/SkiaSharp | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the request! Unfortunately, this is outside the scope of SkiaSharp/Skia. Skia is a 2D rendering library — when it decodes a DNG file via `SKCodec`, it performs the full demosaicing/post-processing pipeline and returns a standard RGB/RGBA bitmap. There is no API to access the raw pre-debayered Bayer sensor values.

For accessing raw sensor data from DNG files in .NET, I'd recommend using **LibRaw** (https://www.libraw.org/), which is specifically designed for this use case. You can call it from .NET via P/Invoke or look for an existing NuGet wrapper. This gives you full access to the pre-demosaiced data.

If you need both raw pixel access (via LibRaw) and then rendering/compositing of the processed image (via SkiaSharp), both libraries can coexist in the same application.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3176,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T03:30:00Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "Reporter requests the ability to access raw (pre-debayered) pixels from DNG camera RAW files, which Skia/SkiaSharp does not currently support.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.85
    }
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "No specific platform or version mentioned"
    }
  },
  "analysis": {
    "summary": "The reporter wants to read the raw Bayer sensor data from a DNG file before any demosaicing or post-processing is applied. SkiaSharp/Skia recognizes DNG as an encoded image format (SKEncodedImageFormat.Dng = 10) and can decode DNG files to RGB via SKCodec.GetPixels(), but Skia processes the DNG to a standard decoded RGB/RGBA bitmap during decode. There is no API surface in SkiaSharp or Skia to access the pre-demosaicing raw sensor values. This functionality requires a specialized RAW processing library such as LibRaw.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "21029-21051",
        "finding": "SKEncodedImageFormat.Dng = 10 is defined, confirming SkiaSharp recognizes DNG as an encoded format. However, this is only used for identifying the format after decode — no special Bayer/raw pixel access API exists.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "56-64",
        "finding": "SKCodec.GetPixels() decodes a DNG to a standard bitmap (RGB/RGBA). The decoded result is the fully processed image, not pre-debayering raw sensor data. No method exists for accessing pre-demosaiced pixels.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "I would like to access to raw pixels of a DNG image (before debayering and post processing)",
        "source": "issue body",
        "interpretation": "This is a request for pre-demosaicing Bayer pattern data — a specialized RAW processing feature beyond Skia's scope as a rendering library."
      },
      {
        "text": "There aren't easy alternatives",
        "source": "issue body",
        "interpretation": "Reporter has already considered alternatives, but specialized RAW libraries like LibRaw exist for this exact purpose."
      }
    ],
    "rationale": "This is a feature request to access pre-debayered RAW sensor data from DNG files. SkiaSharp wraps Skia which is a 2D rendering/compositing library — it decodes image formats to displayable bitmaps but does not expose the underlying raw sensor data. Accessing pre-demosaicing Bayer pixels from DNG is outside the scope of a graphics rendering library and is handled by specialized libraries. The request is clearly for new, out-of-scope functionality.",
    "workarounds": [
      "Use LibRaw (https://www.libraw.org/) for .NET via P/Invoke or via third-party NuGet wrappers to access pre-debayered DNG pixel data.",
      "Use the dcraw command-line tool to extract raw sensor data from DNG files."
    ],
    "resolution": {
      "hypothesis": "Accessing pre-debayered DNG pixels requires specialized RAW processing capabilities that Skia/SkiaSharp does not and is unlikely to expose, as it is a rendering library not a RAW processing library.",
      "proposals": [
        {
          "title": "Use LibRaw for pre-debayering access",
          "description": "LibRaw is a C library specifically designed for reading RAW camera files including DNG. For .NET, the reporter can P/Invoke into LibRaw directly, or use a NuGet wrapper such as LibRawNet. This gives full access to the pre-demosaicing Bayer pattern data.",
          "category": "alternative",
          "confidence": 0.9,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use LibRaw for pre-debayering access",
      "recommendedReason": "LibRaw is purpose-built for exactly this use case. This feature is outside Skia's design scope."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.82,
      "reason": "Pre-debayering DNG pixel access is outside Skia's scope as a 2D rendering library. Skia decodes DNG to standard RGB but does not expose pre-demosaicing sensor data. LibRaw is the appropriate library for this use case.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request and SkiaSharp area labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain that pre-debayering DNG access is out of Skia scope and suggest LibRaw as an alternative",
        "risk": "high",
        "confidence": 0.82,
        "comment": "Thanks for the request! Unfortunately, this is outside the scope of SkiaSharp/Skia. Skia is a 2D rendering library — when it decodes a DNG file via `SKCodec`, it performs the full demosaicing/post-processing pipeline and returns a standard RGB/RGBA bitmap. There is no API to access the raw pre-debayered Bayer sensor values.\n\nFor accessing raw sensor data from DNG files in .NET, I'd recommend using **LibRaw** (https://www.libraw.org/), which is specifically designed for this use case. You can call it from .NET via P/Invoke or look for an existing NuGet wrapper. This gives you full access to the pre-demosaiced data.\n\nIf you need both raw pixel access (via LibRaw) and then rendering/compositing of the processed image (via SkiaSharp), both libraries can coexist in the same application."
      },
      {
        "type": "close-issue",
        "description": "Close as external — feature is out of scope for Skia/SkiaSharp",
        "risk": "medium",
        "confidence": 0.78,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
