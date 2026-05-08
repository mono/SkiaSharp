# Issue Triage Report — #1246

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T20:23:13Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp (0.88 (88%)) |
| Suggested action | close-as-external (0.80 (80%)) |

**Issue Summary:** SKBitmap.Encode returns null when the bitmap uses SKColorType.Rgb888x because Skia's JPEG encoder does not support that pixel format and SkiaSharp does not auto-convert it.

**Analysis:** Skia's JPEG encoder (sk_jpegencoder_encode) returns false for kRGB_888x pixel format. SkiaSharp passes the pixmap directly to the native encoder without converting unsupported color types, so SKData comes back null. The maintainer confirmed this is upstream Skia behavior. A workaround exists: copy the bitmap to SKColorType.Rgba8888 before encoding.

**Recommendations:** **close-as-external** — Root cause is in Skia's JPEG encoder not supporting kRGB_888x. Confirmed by maintainer. A clear workaround (copy to Rgba8888) exists. The issue has been inactive since 2020.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/Raster |
| Tenets | tenet/reliability, tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create an SKBitmap with SKColorType.Rgb888x
2. Call bitmap.Encode(SKEncodedImageFormat.Jpeg, 92)
3. Observe that the returned SKData is null

**Environment:** Windows Classic; SkiaSharp m68 and m80; IDE: any

**Repository links:**
- https://groups.google.com/forum/#!topic/skia-discuss/CSMIjI_Rky8 — Skia mailing list discussion linked by maintainer — Skia JPEG encoder format support

**Code snippets:**

```csharp
var bitmap = new SKBitmap(new SKImageInfo(512, 512, SKColorType.Rgb888x));
var data = bitmap.Encode(SKEncodedImageFormat.Jpeg, 92);
Debug.Assert(data != null); // fails
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | missing-output |
| Error message | SKData returned by SKBitmap.Encode is null when using SKColorType.Rgb888x |
| Repro quality | complete |
| Target frameworks | netstandard2.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | m68, m80 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The encoding path in SkiaSharp has not changed — SKBitmap.Encode delegates to sk_jpegencoder_encode without any format conversion. Upstream Skia's JPEG encoder still lacks Rgb888x support. |

## Analysis

### Technical Summary

Skia's JPEG encoder (sk_jpegencoder_encode) returns false for kRGB_888x pixel format. SkiaSharp passes the pixmap directly to the native encoder without converting unsupported color types, so SKData comes back null. The maintainer confirmed this is upstream Skia behavior. A workaround exists: copy the bitmap to SKColorType.Rgba8888 before encoding.

### Rationale

The bug is real and reproducible with a minimal, self-contained code sample. The root cause is in upstream Skia's JPEG encoder which does not handle the kRGB_888x format — this is confirmed by the maintainer's 2020 comment referencing the Skia mailing list. SkiaSharp does not apply any automatic color-type conversion before calling the native encoder. Classifying as close-as-external because the fix would require Skia to support the format or SkiaSharp to silently convert it (the latter is an enhancement, not a bug in SkiaSharp's wrapper layer).

### Key Signals

- "Most likely a Skia bug (happens in both m68 and m80)" — **issue body** (Consistent across two major Skia milestones — unlikely to be a regression, more likely an unsupported format in the encoder.)
- "This is a Skia bug, or is by design. Waiting for feedback from the mailing list." — **comment by ziriax (CONTRIBUTOR)** (Reporter self-diagnosed and escalated to Skia maintainers.)
- "https://groups.google.com/forum/#!topic/skia-discuss/CSMIjI_Rky8" — **comment by mattleibow (COLLABORATOR)** (Maintainer confirmed and linked Skia mailing list discussion — root cause is upstream.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 730-751 | direct | SKBitmap.Encode(SKEncodedImageFormat, int) peeks a pixmap and delegates to SKPixmap.Encode. No color-type validation or conversion before encoding. |
| `binding/SkiaSharp/SKPixmap.cs` | 235-289 | direct | SKPixmap.Encode for JPEG calls SkiaApi.sk_jpegencoder_encode directly, passing the pixmap handle without any format conversion. If sk_jpegencoder_encode returns false for kRGB_888x, the resulting SKData is null. |
| `binding/SkiaSharp/SKBitmap.cs` | 165-212 | related | SKBitmap.Copy(SKColorType) uses an SKCanvas drawPaint path to do full color-type conversion. This is the correct workaround API: Copy(SKColorType.Rgba8888) produces a JPEG-encodable bitmap. |
| `binding/SkiaSharp/Definitions.cs` | 199-205 | context | SKColorType.Rgb888x is classified as opaque alpha type — a valid, well-supported format in SkiaSharp. The gap is specifically in the JPEG encoder path. |

### Workarounds

- Convert the bitmap to SKColorType.Rgba8888 before encoding: `using var converted = bitmap.Copy(SKColorType.Rgba8888); var data = converted.Encode(SKEncodedImageFormat.Jpeg, 92);`
- Use PNG encoding instead: SKColorType.Rgb888x encodes successfully with SKEncodedImageFormat.Png

### Next Questions

- Has the upstream Skia JPEG encoder gained kRGB_888x support in milestones after m80?
- Should SkiaSharp add a pre-encode format conversion step (enhancement) to handle unsupported color types gracefully?

### Resolution Proposals

**Hypothesis:** Skia's JPEG encoder does not handle kRGB_888x. SkiaSharp passes the raw pixmap without converting, so encoding silently fails.

1. **Convert to Rgba8888 before encoding** — workaround, confidence 0.92 (92%), cost/xs, validated=yes
   - Use SKBitmap.Copy(SKColorType.Rgba8888) to create a compatible bitmap, then call Encode on the copy.

```csharp
var bitmap = new SKBitmap(new SKImageInfo(512, 512, SKColorType.Rgb888x));
using var converted = bitmap.Copy(SKColorType.Rgba8888);
var data = converted.Encode(SKEncodedImageFormat.Jpeg, 92);
Debug.Assert(data != null);
```
2. **SkiaSharp: auto-convert unsupported color types before JPEG encode** — fix, confidence 0.70 (70%), cost/s, validated=untested
   - Add format-conversion logic inside SKPixmap.Encode or SKBitmap.Encode so that unsupported pixel formats (like Rgb888x) are silently converted to Rgba8888 before passing to the native encoder.

**Recommended proposal:** Convert to Rgba8888 before encoding

**Why:** Immediate fix requiring no SkiaSharp changes. The auto-conversion enhancement is a separate feature request.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.80 (80%) |
| Reason | Root cause is in Skia's JPEG encoder not supporting kRGB_888x. Confirmed by maintainer. A clear workaround (copy to Rgba8888) exists. The issue has been inactive since 2020. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug and area labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, backend/Raster, tenet/reliability, tenet/compatibility |
| add-comment | medium | 0.82 (82%) | Post workaround and close explanation | — |
| close-issue | medium | 0.75 (75%) | Close as external upstream issue with workaround provided | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed repro! This is a known limitation in Skia's JPEG encoder: it does not support encoding the `kRGB_888x` pixel format directly.

**Workaround** — convert the bitmap to `Rgba8888` before encoding:

```csharp
var bitmap = new SKBitmap(new SKImageInfo(512, 512, SKColorType.Rgb888x));
using var converted = bitmap.Copy(SKColorType.Rgba8888);
var data = converted.Encode(SKEncodedImageFormat.Jpeg, 92);
Debug.Assert(data != null);
```

Alternatively, `SKEncodedImageFormat.Png` supports `Rgb888x` directly.

The root cause lives in [upstream Skia](https://groups.google.com/forum/#!topic/skia-discuss/CSMIjI_Rky8) — the JPEG encoder does not perform automatic pixel-format conversion. Closing as external; a future enhancement to SkiaSharp could auto-convert unsupported formats before passing them to the native encoder.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1246,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T20:23:13Z"
  },
  "summary": "SKBitmap.Encode returns null when the bitmap uses SKColorType.Rgb888x because Skia's JPEG encoder does not support that pixel format and SkiaSharp does not auto-convert it.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.88
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/Raster"
    ],
    "tenets": [
      "tenet/reliability",
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "missing-output",
      "errorMessage": "SKData returned by SKBitmap.Encode is null when using SKColorType.Rgb888x",
      "reproQuality": "complete",
      "targetFrameworks": [
        "netstandard2.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKBitmap with SKColorType.Rgb888x",
        "Call bitmap.Encode(SKEncodedImageFormat.Jpeg, 92)",
        "Observe that the returned SKData is null"
      ],
      "environmentDetails": "Windows Classic; SkiaSharp m68 and m80; IDE: any",
      "repoLinks": [
        {
          "url": "https://groups.google.com/forum/#!topic/skia-discuss/CSMIjI_Rky8",
          "description": "Skia mailing list discussion linked by maintainer — Skia JPEG encoder format support"
        }
      ],
      "codeSnippets": [
        "var bitmap = new SKBitmap(new SKImageInfo(512, 512, SKColorType.Rgb888x));\nvar data = bitmap.Encode(SKEncodedImageFormat.Jpeg, 92);\nDebug.Assert(data != null); // fails"
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "m68",
        "m80"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The encoding path in SkiaSharp has not changed — SKBitmap.Encode delegates to sk_jpegencoder_encode without any format conversion. Upstream Skia's JPEG encoder still lacks Rgb888x support."
    }
  },
  "analysis": {
    "summary": "Skia's JPEG encoder (sk_jpegencoder_encode) returns false for kRGB_888x pixel format. SkiaSharp passes the pixmap directly to the native encoder without converting unsupported color types, so SKData comes back null. The maintainer confirmed this is upstream Skia behavior. A workaround exists: copy the bitmap to SKColorType.Rgba8888 before encoding.",
    "rationale": "The bug is real and reproducible with a minimal, self-contained code sample. The root cause is in upstream Skia's JPEG encoder which does not handle the kRGB_888x format — this is confirmed by the maintainer's 2020 comment referencing the Skia mailing list. SkiaSharp does not apply any automatic color-type conversion before calling the native encoder. Classifying as close-as-external because the fix would require Skia to support the format or SkiaSharp to silently convert it (the latter is an enhancement, not a bug in SkiaSharp's wrapper layer).",
    "keySignals": [
      {
        "text": "Most likely a Skia bug (happens in both m68 and m80)",
        "source": "issue body",
        "interpretation": "Consistent across two major Skia milestones — unlikely to be a regression, more likely an unsupported format in the encoder."
      },
      {
        "text": "This is a Skia bug, or is by design. Waiting for feedback from the mailing list.",
        "source": "comment by ziriax (CONTRIBUTOR)",
        "interpretation": "Reporter self-diagnosed and escalated to Skia maintainers."
      },
      {
        "text": "https://groups.google.com/forum/#!topic/skia-discuss/CSMIjI_Rky8",
        "source": "comment by mattleibow (COLLABORATOR)",
        "interpretation": "Maintainer confirmed and linked Skia mailing list discussion — root cause is upstream."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "730-751",
        "finding": "SKBitmap.Encode(SKEncodedImageFormat, int) peeks a pixmap and delegates to SKPixmap.Encode. No color-type validation or conversion before encoding.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPixmap.cs",
        "lines": "235-289",
        "finding": "SKPixmap.Encode for JPEG calls SkiaApi.sk_jpegencoder_encode directly, passing the pixmap handle without any format conversion. If sk_jpegencoder_encode returns false for kRGB_888x, the resulting SKData is null.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "165-212",
        "finding": "SKBitmap.Copy(SKColorType) uses an SKCanvas drawPaint path to do full color-type conversion. This is the correct workaround API: Copy(SKColorType.Rgba8888) produces a JPEG-encodable bitmap.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "199-205",
        "finding": "SKColorType.Rgb888x is classified as opaque alpha type — a valid, well-supported format in SkiaSharp. The gap is specifically in the JPEG encoder path.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "Convert the bitmap to SKColorType.Rgba8888 before encoding: `using var converted = bitmap.Copy(SKColorType.Rgba8888); var data = converted.Encode(SKEncodedImageFormat.Jpeg, 92);`",
      "Use PNG encoding instead: SKColorType.Rgb888x encodes successfully with SKEncodedImageFormat.Png"
    ],
    "nextQuestions": [
      "Has the upstream Skia JPEG encoder gained kRGB_888x support in milestones after m80?",
      "Should SkiaSharp add a pre-encode format conversion step (enhancement) to handle unsupported color types gracefully?"
    ],
    "resolution": {
      "hypothesis": "Skia's JPEG encoder does not handle kRGB_888x. SkiaSharp passes the raw pixmap without converting, so encoding silently fails.",
      "proposals": [
        {
          "title": "Convert to Rgba8888 before encoding",
          "description": "Use SKBitmap.Copy(SKColorType.Rgba8888) to create a compatible bitmap, then call Encode on the copy.",
          "category": "workaround",
          "codeSnippet": "var bitmap = new SKBitmap(new SKImageInfo(512, 512, SKColorType.Rgb888x));\nusing var converted = bitmap.Copy(SKColorType.Rgba8888);\nvar data = converted.Encode(SKEncodedImageFormat.Jpeg, 92);\nDebug.Assert(data != null);",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "SkiaSharp: auto-convert unsupported color types before JPEG encode",
          "description": "Add format-conversion logic inside SKPixmap.Encode or SKBitmap.Encode so that unsupported pixel formats (like Rgb888x) are silently converted to Rgba8888 before passing to the native encoder.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Convert to Rgba8888 before encoding",
      "recommendedReason": "Immediate fix requiring no SkiaSharp changes. The auto-conversion enhancement is a separate feature request."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.8,
      "reason": "Root cause is in Skia's JPEG encoder not supporting kRGB_888x. Confirmed by maintainer. A clear workaround (copy to Rgba8888) exists. The issue has been inactive since 2020.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug and area labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/Raster",
          "tenet/reliability",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post workaround and close explanation",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed repro! This is a known limitation in Skia's JPEG encoder: it does not support encoding the `kRGB_888x` pixel format directly.\n\n**Workaround** — convert the bitmap to `Rgba8888` before encoding:\n\n```csharp\nvar bitmap = new SKBitmap(new SKImageInfo(512, 512, SKColorType.Rgb888x));\nusing var converted = bitmap.Copy(SKColorType.Rgba8888);\nvar data = converted.Encode(SKEncodedImageFormat.Jpeg, 92);\nDebug.Assert(data != null);\n```\n\nAlternatively, `SKEncodedImageFormat.Png` supports `Rgb888x` directly.\n\nThe root cause lives in [upstream Skia](https://groups.google.com/forum/#!topic/skia-discuss/CSMIjI_Rky8) — the JPEG encoder does not perform automatic pixel-format conversion. Closing as external; a future enhancement to SkiaSharp could auto-convert unsupported formats before passing them to the native encoder."
      },
      {
        "type": "close-issue",
        "description": "Close as external upstream issue with workaround provided",
        "risk": "medium",
        "confidence": 0.75,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
