# Issue Triage Report — #3142

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:16:19Z |
| Type | type/question (0.88 (88%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** Reporter gets a NullReferenceException when calling SKImage.Encode() with formats other than JPEG, PNG, or WebP (e.g., HEIF, AVIF, BMP, GIF), not realizing that Skia only supports encoding to JPEG, PNG, and WebP.

**Analysis:** Reporter expects SKImage.Encode() to support all SKEncodedImageFormat values, but Skia only provides encoders for JPEG, PNG, and WebP. Passing other formats causes Encode() to return null, then calling SaveTo() on null throws NullReferenceException.

**Recommendations:** **close-as-not-a-bug** — Behavior is by design from upstream Skia — only JPEG, PNG, and WebP are supported for encoding. Confirmed by maintainer in issue #64 and by a community member in this issue. The same version is listed as both current and last known good, confirming no regression.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create an SKBitmap and draw on it with SKCanvas
2. Convert to SKImage using SKImage.FromBitmap(bitmap)
3. Call image.Encode(SKEncodedImageFormat.Heif, 100)
4. The returned SKData is null
5. Calling data.SaveTo(stream) throws NullReferenceException

**Environment:** ASP.NET Core, .NET 8, SkiaSharp 2.88.8, Windows 10, Visual Studio 2022

**Related issues:** #64

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.8 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SKPixmap.Encode switch statement only handles JPEG, PNG, and WebP; this has not changed between 2.88.x and current versions. The issue is not a regression — the same version is listed as both current and last known good. |

## Analysis

### Technical Summary

Reporter expects SKImage.Encode() to support all SKEncodedImageFormat values, but Skia only provides encoders for JPEG, PNG, and WebP. Passing other formats causes Encode() to return null, then calling SaveTo() on null throws NullReferenceException.

### Rationale

The confusion arises because SKEncodedImageFormat is used for both decoding (identifying the format of input data) and encoding (specifying output format), but only JPEG, PNG, and WebP are supported for encoding. The maintainer (mattleibow) confirmed in issue #64 that this is expected behavior from upstream Skia. A community member confirmed the same for this issue. The same version is listed as both current and last known good, confirming this is not a regression.

### Key Signals

- "This is not a bug. The encodings for those formats are not implemented by Skia." — **comment by nandor23** (Community member correctly identifies this as by-design behavior from upstream Skia)
- "This issue does not occur when using formats such as JPG, PNG, and WEBP." — **issue body** (Reporter has correctly identified that only JPEG, PNG, and WebP work for encoding)
- "Last Known Good Version of SkiaSharp: 2.88.8 (Deprecated)" — **issue body** (Same as current version — this was never working, not a regression)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPixmap.cs` | — | direct | SKPixmap.Encode(SKWStream, SKEncodedImageFormat, int) uses a switch that handles only Jpeg, Png, and Webp — all other formats fall through to '_ => false', causing Encode(format, quality) to return null |
| `binding/SkiaSharp/SkiaApi.generated.cs` | — | context | SKEncodedImageFormat enum has 14 values (Bmp=0, Gif=1, Ico=2, Jpeg=3, Png=4, Wbmp=5, Webp=6, Pkm=7, Ktx=8, Astc=9, Dng=10, Heif=11, Avif=12, Jpegxl=13) — these represent formats Skia can decode/read, not all formats it can encode |
| `binding/SkiaSharp/SKImage.cs` | — | direct | SKImage.Encode(SKEncodedImageFormat, int) delegates to pixmap?.Encode(format, quality) — returns null if pixmap is null or if SKPixmap.Encode returns null for unsupported formats |

### Workarounds

- Use only JPEG, PNG, or WebP for SKImage.Encode() — these are the only formats Skia supports for encoding
- Always null-check the return value of Encode() before calling SaveTo()
- For HEIF/AVIF output, encode to JPEG/PNG first, then use a separate platform library for conversion if needed

### Resolution Proposals

**Hypothesis:** Reporter is passing SKEncodedImageFormat.Heif (and other unsupported formats) to SKImage.Encode(), which returns null for any format other than JPEG, PNG, or WebP, causing NullReferenceException.

1. **Use a supported encoding format with proper disposal** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Switch to JPEG, PNG, or WebP and wrap the returned SKData in a using statement.

```csharp
// Only JPEG, PNG, and WebP are supported for encoding:
using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
if (data != null)
{
    data.SaveTo(stream);
}
```

**Recommended proposal:** Use a supported encoding format with proper disposal

**Why:** Skia only encodes to JPEG, PNG, and WebP — this is an upstream constraint. The workaround is immediate and correct; using var ensures SKData is properly disposed.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | Behavior is by design from upstream Skia — only JPEG, PNG, and WebP are supported for encoding. Confirmed by maintainer in issue #64 and by a community member in this issue. The same version is listed as both current and last known good, confirming no regression. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Correct label from type/bug to type/question, add area/SkiaSharp and os/Windows-Classic | labels=type/question, area/SkiaSharp, os/Windows-Classic |
| add-comment | high | 0.88 (88%) | Explain that only JPEG, PNG, and WebP are supported for encoding and provide workaround | — |
| link-related | low | 0.95 (95%) | Link to the original related issue #64 which covers the same by-design behavior | linkedIssue=#64 |
| close-issue | medium | 0.85 (85%) | Close as not a bug — confirmed by-design Skia limitation | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for reporting! This is expected behavior — Skia only supports **encoding** (writing) to three formats: **JPEG**, **PNG**, and **WebP**. The `SKEncodedImageFormat` enum contains all formats that Skia can *decode/read*, but only those three are supported for encoding output.

Formats like BMP, GIF, ICO, HEIF, AVIF, etc. are recognized for decoding input images but have no encoder in Skia, so `SKImage.Encode()` returns `null` for them. Calling `.SaveTo()` on that `null` result throws a `NullReferenceException`.

Here's the corrected approach:

```csharp
// Only JPEG, PNG, and WebP work for encoding:
using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
if (data != null)
{
    data.SaveTo(stream);
}
```

This same question was answered in #64. Closing as not a bug — this is an upstream Skia limitation.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3142,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:16:19Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter gets a NullReferenceException when calling SKImage.Encode() with formats other than JPEG, PNG, or WebP (e.g., HEIF, AVIF, BMP, GIF), not realizing that Skia only supports encoding to JPEG, PNG, and WebP.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-Classic"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKBitmap and draw on it with SKCanvas",
        "Convert to SKImage using SKImage.FromBitmap(bitmap)",
        "Call image.Encode(SKEncodedImageFormat.Heif, 100)",
        "The returned SKData is null",
        "Calling data.SaveTo(stream) throws NullReferenceException"
      ],
      "environmentDetails": "ASP.NET Core, .NET 8, SkiaSharp 2.88.8, Windows 10, Visual Studio 2022",
      "relatedIssues": [
        64
      ],
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.8"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "SKPixmap.Encode switch statement only handles JPEG, PNG, and WebP; this has not changed between 2.88.x and current versions. The issue is not a regression — the same version is listed as both current and last known good."
    }
  },
  "analysis": {
    "summary": "Reporter expects SKImage.Encode() to support all SKEncodedImageFormat values, but Skia only provides encoders for JPEG, PNG, and WebP. Passing other formats causes Encode() to return null, then calling SaveTo() on null throws NullReferenceException.",
    "rationale": "The confusion arises because SKEncodedImageFormat is used for both decoding (identifying the format of input data) and encoding (specifying output format), but only JPEG, PNG, and WebP are supported for encoding. The maintainer (mattleibow) confirmed in issue #64 that this is expected behavior from upstream Skia. A community member confirmed the same for this issue. The same version is listed as both current and last known good, confirming this is not a regression.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPixmap.cs",
        "finding": "SKPixmap.Encode(SKWStream, SKEncodedImageFormat, int) uses a switch that handles only Jpeg, Png, and Webp — all other formats fall through to '_ => false', causing Encode(format, quality) to return null",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "SKEncodedImageFormat enum has 14 values (Bmp=0, Gif=1, Ico=2, Jpeg=3, Png=4, Wbmp=5, Webp=6, Pkm=7, Ktx=8, Astc=9, Dng=10, Heif=11, Avif=12, Jpegxl=13) — these represent formats Skia can decode/read, not all formats it can encode",
        "relevance": "context"
      },
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "finding": "SKImage.Encode(SKEncodedImageFormat, int) delegates to pixmap?.Encode(format, quality) — returns null if pixmap is null or if SKPixmap.Encode returns null for unsupported formats",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "This is not a bug. The encodings for those formats are not implemented by Skia.",
        "source": "comment by nandor23",
        "interpretation": "Community member correctly identifies this as by-design behavior from upstream Skia"
      },
      {
        "text": "This issue does not occur when using formats such as JPG, PNG, and WEBP.",
        "source": "issue body",
        "interpretation": "Reporter has correctly identified that only JPEG, PNG, and WebP work for encoding"
      },
      {
        "text": "Last Known Good Version of SkiaSharp: 2.88.8 (Deprecated)",
        "source": "issue body",
        "interpretation": "Same as current version — this was never working, not a regression"
      }
    ],
    "workarounds": [
      "Use only JPEG, PNG, or WebP for SKImage.Encode() — these are the only formats Skia supports for encoding",
      "Always null-check the return value of Encode() before calling SaveTo()",
      "For HEIF/AVIF output, encode to JPEG/PNG first, then use a separate platform library for conversion if needed"
    ],
    "resolution": {
      "hypothesis": "Reporter is passing SKEncodedImageFormat.Heif (and other unsupported formats) to SKImage.Encode(), which returns null for any format other than JPEG, PNG, or WebP, causing NullReferenceException.",
      "proposals": [
        {
          "title": "Use a supported encoding format with proper disposal",
          "description": "Switch to JPEG, PNG, or WebP and wrap the returned SKData in a using statement.",
          "category": "workaround",
          "codeSnippet": "// Only JPEG, PNG, and WebP are supported for encoding:\nusing var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);\nif (data != null)\n{\n    data.SaveTo(stream);\n}",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Use a supported encoding format with proper disposal",
      "recommendedReason": "Skia only encodes to JPEG, PNG, and WebP — this is an upstream constraint. The workaround is immediate and correct; using var ensures SKData is properly disposed."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "Behavior is by design from upstream Skia — only JPEG, PNG, and WebP are supported for encoding. Confirmed by maintainer in issue #64 and by a community member in this issue. The same version is listed as both current and last known good, confirming no regression.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct label from type/bug to type/question, add area/SkiaSharp and os/Windows-Classic",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "os/Windows-Classic"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain that only JPEG, PNG, and WebP are supported for encoding and provide workaround",
        "risk": "high",
        "confidence": 0.88,
        "comment": "Thanks for reporting! This is expected behavior — Skia only supports **encoding** (writing) to three formats: **JPEG**, **PNG**, and **WebP**. The `SKEncodedImageFormat` enum contains all formats that Skia can *decode/read*, but only those three are supported for encoding output.\n\nFormats like BMP, GIF, ICO, HEIF, AVIF, etc. are recognized for decoding input images but have no encoder in Skia, so `SKImage.Encode()` returns `null` for them. Calling `.SaveTo()` on that `null` result throws a `NullReferenceException`.\n\nHere's the corrected approach:\n\n```csharp\n// Only JPEG, PNG, and WebP work for encoding:\nusing var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);\nif (data != null)\n{\n    data.SaveTo(stream);\n}\n```\n\nThis same question was answered in #64. Closing as not a bug — this is an upstream Skia limitation."
      },
      {
        "type": "link-related",
        "description": "Link to the original related issue #64 which covers the same by-design behavior",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 64
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — confirmed by-design Skia limitation",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
