# Issue Triage Report — #2718

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T14:30:00Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-external (0.88 (88%)) |

**Issue Summary:** SKBitmap.Encode(SKEncodedImageFormat.Avif, quality) silently returns null because Skia upstream does not implement an AVIF encoder, and SkiaSharp dispatch switch falls through to a silent false/null path with no NotSupportedException.

**Analysis:** AVIF encoding silently fails because Skia upstream provides no AVIF encoder (only a decoder) and SkiaSharp Encode() dispatch switch falls through to _ => false for unsupported formats. The user receives null SKData with no indication of the limitation.

**Recommendations:** **close-as-external** — Root cause is that Skia upstream does not implement an AVIF encoder — confirmed by community comment and absence of sk_avifencoder_encode in Skia encode headers. SkiaSharp cannot encode to AVIF without an upstream encoder. A community library (LibavifSharp) fills this gap.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a SKBitmap with pixel data
2. Call bmp.Encode(SKEncodedImageFormat.Avif, 90)
3. Observe that the returned SKData is null

**Environment:** SkiaSharp 2.88.3, Visual Studio, Windows 11

**Related issues:** #3142

**Repository links:**
- https://stackoverflow.com/questions/77669075/how-to-make-avif-codec-a-dependency-of-asp-net-web-project-with-skiasharp — Related StackOverflow question about AVIF in SkiaSharp
- https://github.com/google/skia/tree/a57372ca2e66d24db942f406404b8be92870afb2/include/encode — Skia upstream encode headers — no AVIF encoder present
- https://github.com/Spacefish/libavifsharp — Third-party LibavifSharp wrapper for AVIF encoding from SKBitmap/SKImage

**Code snippets:**

```csharp
var data = bmp.Encode(SKEncodedImageFormat.Avif, 90);
return data.AsStream(true); // NullReferenceException
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | missing-output |
| Error message | SKData returned is null when encoding to AVIF format |
| Repro quality | partial |
| Target frameworks | net8.0-windows |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3, 3.0.0-preview2.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The SKPixmap.Encode switch still only dispatches to JPEG, PNG, and WebP encoders as confirmed in current source. No AVIF encoder has been added. |

## Analysis

### Technical Summary

AVIF encoding silently fails because Skia upstream provides no AVIF encoder (only a decoder) and SkiaSharp Encode() dispatch switch falls through to _ => false for unsupported formats. The user receives null SKData with no indication of the limitation.

### Rationale

The bug is the silent null return, not the absence of AVIF encoding per se. Skia intentionally omits AVIF encoding (confirmed by community comment and absence of sk_avifencoder_encode in bindings). Root cause is upstream (Skia), making this close-as-external, but the silent failure UX is real.

### Key Signals

- "Encoding to AVIF image is not working, but no information is provided just returned SKData is null" — **issue body** (Silent null return is the actual bug — no error, no exception, no indication of unsupported format.)
- "Skia does not support AVIF Encoding anyway see: https://github.com/google/skia/tree/a57372ca2e66d24db942f406404b8be92870afb2/include/encode" — **comment by Spacefish** (Root cause confirmed: upstream Skia has no AVIF encoder. Only JPEG, PNG, WebP encoders exist in Skia encode headers.)
- "NotSupportedException is thrown or working encoding" — **issue body (Expected Behavior)** (Reporter correctly identifies that a clear error is the minimum acceptable behavior.)
- "Issue still present in 3.0.0preview2.1" — **comment by Spacefish** (Not fixed in the preview release — still current.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPixmap.cs` | 235-246 | direct | Encode(SKWStream, SKEncodedImageFormat, int) is a switch expression dispatching only Jpeg, Png, and Webp to typed encoders. All other formats including Avif fall through to _ => false, causing null return from the SKData overload. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 20776 | direct | SKEncodedImageFormat.Avif = 12 exists in the generated enum. Only sk_jpegencoder_encode, sk_pngencoder_encode, and sk_webpencoder_encode are present — no sk_avifencoder_encode exists, confirming Skia has no AVIF encoder. |
| `binding/SkiaSharp/SKBitmap.cs` | — | related | SKBitmap.Encode(SKEncodedImageFormat, int) delegates to SKPixmap.Encode() via PeekPixels(). The null propagates from SKPixmap.Encode() when AVIF is requested. |

### Workarounds

- Use LibavifSharp (https://www.nuget.org/packages/LibavifSharp) — a .NET wrapper around libavif that accepts SKImage/SKBitmap for AVIF encoding.
- Encode to a supported format (PNG, JPEG, WebP) instead of AVIF.
- Always null-check the result of Encode() before calling AsStream() to avoid NullReferenceException.

### Next Questions

- Should SkiaSharp throw NotSupportedException from the _ => false default case to improve UX for all unsupported formats?
- Is there an upstream Skia AVIF encoding effort in a future milestone that SkiaSharp should track?

### Resolution Proposals

**Hypothesis:** Skia upstream does not provide an AVIF encoder. SkiaSharp correctly has no sk_avifencoder_encode binding. The UX gap is the silent null return with no error for unsupported formats.

1. **Use LibavifSharp for AVIF encoding** — workaround, confidence 0.92 (92%), cost/xs, validated=untested
   - A community member (Spacefish) published LibavifSharp, a .NET wrapper around libavif that accepts SKImage or SKBitmap directly. Available on NuGet.
2. **Encode to WebP lossless as AVIF substitute** — alternative, confidence 0.85 (85%), cost/xs, validated=untested
   - WebP lossless via SKEncodedImageFormat.Webp with quality=100 is broadly supported across browsers and offers similar compression to AVIF.

**Recommended proposal:** Use LibavifSharp for AVIF encoding

**Why:** Directly addresses the AVIF encoding requirement with minimal code change. Community-maintained and specifically designed to bridge SkiaSharp and AVIF.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.88 (88%) |
| Reason | Root cause is that Skia upstream does not implement an AVIF encoder — confirmed by community comment and absence of sk_avifencoder_encode in Skia encode headers. SkiaSharp cannot encode to AVIF without an upstream encoder. A community library (LibavifSharp) fills this gap. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, area/SkiaSharp, Windows-Classic, reliability tenet labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Explain that AVIF encoding is not supported by Skia upstream and point to LibavifSharp workaround | — |
| close-issue | medium | 0.85 (85%) | Close as external — AVIF encoding is not implemented in Skia upstream | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for reporting this. AVIF encoding (writing) is not supported by Skia upstream — the Skia library only includes decoders for AVIF (reading), but no AVIF encoder. As a result, SKBitmap.Encode(SKEncodedImageFormat.Avif, quality) silently returns null rather than throwing a NotSupportedException.

For AVIF encoding from SkiaSharp, the community has published LibavifSharp (https://www.nuget.org/packages/LibavifSharp), a .NET wrapper around libavif that accepts SKImage or SKBitmap directly.

Alternatively, WebP lossless (SKEncodedImageFormat.Webp with quality=100) provides similar compression ratios and is widely supported.

This issue will be closed as the limitation is in the upstream Skia library rather than SkiaSharp itself.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2718,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T14:30:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKBitmap.Encode(SKEncodedImageFormat.Avif, quality) silently returns null because Skia upstream does not implement an AVIF encoder, and SkiaSharp dispatch switch falls through to a silent false/null path with no NotSupportedException.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "missing-output",
      "errorMessage": "SKData returned is null when encoding to AVIF format",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0-windows"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a SKBitmap with pixel data",
        "Call bmp.Encode(SKEncodedImageFormat.Avif, 90)",
        "Observe that the returned SKData is null"
      ],
      "codeSnippets": [
        "var data = bmp.Encode(SKEncodedImageFormat.Avif, 90);\nreturn data.AsStream(true); // NullReferenceException"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, Visual Studio, Windows 11",
      "repoLinks": [
        {
          "url": "https://stackoverflow.com/questions/77669075/how-to-make-avif-codec-a-dependency-of-asp-net-web-project-with-skiasharp",
          "description": "Related StackOverflow question about AVIF in SkiaSharp"
        },
        {
          "url": "https://github.com/google/skia/tree/a57372ca2e66d24db942f406404b8be92870afb2/include/encode",
          "description": "Skia upstream encode headers — no AVIF encoder present"
        },
        {
          "url": "https://github.com/Spacefish/libavifsharp",
          "description": "Third-party LibavifSharp wrapper for AVIF encoding from SKBitmap/SKImage"
        }
      ],
      "relatedIssues": [
        3142
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3",
        "3.0.0-preview2.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The SKPixmap.Encode switch still only dispatches to JPEG, PNG, and WebP encoders as confirmed in current source. No AVIF encoder has been added."
    }
  },
  "analysis": {
    "summary": "AVIF encoding silently fails because Skia upstream provides no AVIF encoder (only a decoder) and SkiaSharp Encode() dispatch switch falls through to _ => false for unsupported formats. The user receives null SKData with no indication of the limitation.",
    "rationale": "The bug is the silent null return, not the absence of AVIF encoding per se. Skia intentionally omits AVIF encoding (confirmed by community comment and absence of sk_avifencoder_encode in bindings). Root cause is upstream (Skia), making this close-as-external, but the silent failure UX is real.",
    "keySignals": [
      {
        "text": "Encoding to AVIF image is not working, but no information is provided just returned SKData is null",
        "source": "issue body",
        "interpretation": "Silent null return is the actual bug — no error, no exception, no indication of unsupported format."
      },
      {
        "text": "Skia does not support AVIF Encoding anyway see: https://github.com/google/skia/tree/a57372ca2e66d24db942f406404b8be92870afb2/include/encode",
        "source": "comment by Spacefish",
        "interpretation": "Root cause confirmed: upstream Skia has no AVIF encoder. Only JPEG, PNG, WebP encoders exist in Skia encode headers."
      },
      {
        "text": "NotSupportedException is thrown or working encoding",
        "source": "issue body (Expected Behavior)",
        "interpretation": "Reporter correctly identifies that a clear error is the minimum acceptable behavior."
      },
      {
        "text": "Issue still present in 3.0.0preview2.1",
        "source": "comment by Spacefish",
        "interpretation": "Not fixed in the preview release — still current."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPixmap.cs",
        "lines": "235-246",
        "finding": "Encode(SKWStream, SKEncodedImageFormat, int) is a switch expression dispatching only Jpeg, Png, and Webp to typed encoders. All other formats including Avif fall through to _ => false, causing null return from the SKData overload.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "20776",
        "finding": "SKEncodedImageFormat.Avif = 12 exists in the generated enum. Only sk_jpegencoder_encode, sk_pngencoder_encode, and sk_webpencoder_encode are present — no sk_avifencoder_encode exists, confirming Skia has no AVIF encoder.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "finding": "SKBitmap.Encode(SKEncodedImageFormat, int) delegates to SKPixmap.Encode() via PeekPixels(). The null propagates from SKPixmap.Encode() when AVIF is requested.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use LibavifSharp (https://www.nuget.org/packages/LibavifSharp) — a .NET wrapper around libavif that accepts SKImage/SKBitmap for AVIF encoding.",
      "Encode to a supported format (PNG, JPEG, WebP) instead of AVIF.",
      "Always null-check the result of Encode() before calling AsStream() to avoid NullReferenceException."
    ],
    "nextQuestions": [
      "Should SkiaSharp throw NotSupportedException from the _ => false default case to improve UX for all unsupported formats?",
      "Is there an upstream Skia AVIF encoding effort in a future milestone that SkiaSharp should track?"
    ],
    "resolution": {
      "hypothesis": "Skia upstream does not provide an AVIF encoder. SkiaSharp correctly has no sk_avifencoder_encode binding. The UX gap is the silent null return with no error for unsupported formats.",
      "proposals": [
        {
          "title": "Use LibavifSharp for AVIF encoding",
          "description": "A community member (Spacefish) published LibavifSharp, a .NET wrapper around libavif that accepts SKImage or SKBitmap directly. Available on NuGet.",
          "category": "workaround",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Encode to WebP lossless as AVIF substitute",
          "description": "WebP lossless via SKEncodedImageFormat.Webp with quality=100 is broadly supported across browsers and offers similar compression to AVIF.",
          "category": "alternative",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use LibavifSharp for AVIF encoding",
      "recommendedReason": "Directly addresses the AVIF encoding requirement with minimal code change. Community-maintained and specifically designed to bridge SkiaSharp and AVIF."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.88,
      "reason": "Root cause is that Skia upstream does not implement an AVIF encoder — confirmed by community comment and absence of sk_avifencoder_encode in Skia encode headers. SkiaSharp cannot encode to AVIF without an upstream encoder. A community library (LibavifSharp) fills this gap.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, area/SkiaSharp, Windows-Classic, reliability tenet labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain that AVIF encoding is not supported by Skia upstream and point to LibavifSharp workaround",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for reporting this. AVIF encoding (writing) is not supported by Skia upstream — the Skia library only includes decoders for AVIF (reading), but no AVIF encoder. As a result, SKBitmap.Encode(SKEncodedImageFormat.Avif, quality) silently returns null rather than throwing a NotSupportedException.\n\nFor AVIF encoding from SkiaSharp, the community has published LibavifSharp (https://www.nuget.org/packages/LibavifSharp), a .NET wrapper around libavif that accepts SKImage or SKBitmap directly.\n\nAlternatively, WebP lossless (SKEncodedImageFormat.Webp with quality=100) provides similar compression ratios and is widely supported.\n\nThis issue will be closed as the limitation is in the upstream Skia library rather than SkiaSharp itself."
      },
      {
        "type": "close-issue",
        "description": "Close as external — AVIF encoding is not implemented in Skia upstream",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
