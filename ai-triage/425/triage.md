# Issue Triage Report — #425

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-27T10:44:15Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | keep-open (0.85 (85%)) |

**Issue Summary:** SKCodec.EncodedOrigin (previously Origin) always returns TopLeft for DNG and CR2 raw image files, ignoring the actual EXIF orientation metadata; the Skia DNG codec does not read orientation, unlike JPEG and WebP codecs.

**Analysis:** The DNG (and CR2) codec in upstream Skia does not read the EXIF orientation tag, so sk_codec_get_origin always returns the default TopLeft value for raw image formats. Only JPEG and WebP codecs respect this property. The SkiaSharp binding (SKCodec.EncodedOrigin) correctly passes through the native value but cannot compensate for the upstream omission.

**Recommendations:** **keep-open** — Root cause confirmed as upstream Skia DNG codec not reading orientation metadata. Already labeled status/skia-update-required. Issue should stay open until a Skia update resolves it or a SkiaSharp-level workaround is shipped.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug, status/skia-update-required |

## Evidence

### Reproduction

1. Open a DNG or CR2 raw image file with SKCodec.Create(stream)
2. Read codec.Origin (or codec.EncodedOrigin in newer API)
3. Observe that TopLeft is always returned, regardless of actual EXIF orientation

**Environment:** Any platform; DNG and CR2 raw image files with EXIF orientation != TopLeft

**Attachments:**
- APC_0316.zip — https://github.com/mono/SkiaSharp/files/1616452/APC_0316.zip — Sample DNG file with non-default EXIF orientation
- APC_0193.zip — https://github.com/mono/SkiaSharp/files/1616453/APC_0193.zip — Second sample DNG file with non-default EXIF orientation
- IMG_1361.zip — https://github.com/mono/SkiaSharp/files/1645431/IMG_1361.zip — Sample CR2 file that should report LeftBottom but reports TopLeft

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | SKCodecOrigin.TopLeft returned for DNG/CR2 files regardless of actual EXIF orientation |
| Repro quality | complete |
| Target frameworks | — |

## Analysis

### Technical Summary

The DNG (and CR2) codec in upstream Skia does not read the EXIF orientation tag, so sk_codec_get_origin always returns the default TopLeft value for raw image formats. Only JPEG and WebP codecs respect this property. The SkiaSharp binding (SKCodec.EncodedOrigin) correctly passes through the native value but cannot compensate for the upstream omission.

### Rationale

The issue is clearly a wrong-output bug: the reporter provides DNG and CR2 sample files and sample code demonstrating that orientation is always TopLeft when it should not be. The maintainer confirmed the root cause is in upstream Skia's DNG codec. The label status/skia-update-required already reflects this. Classification as type/bug is confirmed because the SKCodec contract promises the actual encoded origin, and the DNG codec fails to fulfill it. No platform label is needed as this is format-specific and cross-platform.

### Key Signals

- "The following sample code results in SKCodecOrigin.TopLeft, which is incorrect." — **issue body** (Wrong-output bug — orientation metadata not being read from DNG files.)
- "Looks like this is not limited to just dng. Here's a sample cr2 that is also affected. It is reported as TopLeft but it should be LeftBottom." — **comment by LukePulverenti** (The problem affects multiple raw camera formats, not just DNG.)
- "The DNG codec ignores this. The only ones that appear to respect this are jpeg and webp." — **comment by mattleibow** (Maintainer-confirmed root cause in upstream Skia's DNG codec implementation.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCodec.cs` | 36-37 | direct | SKCodec.EncodedOrigin simply delegates to SkiaApi.sk_codec_get_origin(Handle) — no transformation or filtering; the C# binding faithfully passes through whatever the native codec returns. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | — | related | sk_codec_get_origin is a direct P/Invoke to the native sk_codec_get_origin function in libSkiaSharp; the generated binding is correct and the issue is entirely in native Skia behavior. |

### Workarounds

- Use a third-party .NET EXIF library (e.g., MetadataExtractor) to read orientation from the raw file's metadata, then manually rotate/flip the decoded bitmap using SKBitmap transform methods.
- Convert raw files to JPEG before processing with SkiaSharp — JPEG orientation is correctly handled by the Skia JPEG codec.

### Next Questions

- Has Skia upstream added DNG/CR2 orientation support in more recent milestones?
- Is the issue limited to DNG/CR2 or do other raw formats (ARW, NEF, RAF) share the same problem?

### Resolution Proposals

**Hypothesis:** Upstream Skia's SkRawCodec (DNG/CR2 handler) does not parse or report EXIF orientation metadata, unlike the JPEG and WebP codecs that do. A fix requires updating Skia or post-processing orientation in SkiaSharp using a separate EXIF reader.

1. **Workaround: read orientation via MetadataExtractor** — workaround, confidence 0.85 (85%), cost/s, validated=untested
   - Use MetadataExtractor NuGet package to read EXIF orientation from the raw file independently, then manually rotate or flip the decoded SKBitmap to match.
2. **Fix: update Skia to a milestone where DNG codec reads orientation** — fix, confidence 0.60 (60%), cost/l, validated=untested
   - Investigate recent Skia milestones to determine whether the upstream DNG/CR2 codec has been updated to report orientation. If so, bump the Skia dependency.

**Recommended proposal:** Workaround: read orientation via MetadataExtractor

**Why:** Provides immediate relief for users without requiring an upstream Skia change. The fix path depends on upstream availability.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.85 (85%) |
| Reason | Root cause confirmed as upstream Skia DNG codec not reading orientation metadata. Already labeled status/skia-update-required. Issue should stay open until a Skia update resolves it or a SkiaSharp-level workaround is shipped. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug and area labels; add tenet/compatibility since raw camera format orientation is a compatibility concern. | labels=type/bug, area/SkiaSharp, tenet/compatibility |
| add-comment | medium | 0.85 (85%) | Acknowledge root cause and suggest MetadataExtractor workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report and the sample files. The root cause is in upstream Skia: the DNG and CR2 raw-image codecs do not read the EXIF orientation tag, so `SKCodec.EncodedOrigin` always returns `TopLeft` for these formats. Only the JPEG and WebP codecs currently report orientation correctly.

This is tracked as a Skia upstream issue (`status/skia-update-required`).

**Workaround:** Use the [MetadataExtractor](https://github.com/drewnoakes/metadata-extractor-dotnet) NuGet package to read the EXIF orientation from your DNG/CR2 file independently, then manually rotate or flip the decoded `SKBitmap` to match the actual orientation.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 425,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-27T10:44:15Z",
    "currentLabels": [
      "type/bug",
      "status/skia-update-required"
    ]
  },
  "summary": "SKCodec.EncodedOrigin (previously Origin) always returns TopLeft for DNG and CR2 raw image files, ignoring the actual EXIF orientation metadata; the Skia DNG codec does not read orientation, unlike JPEG and WebP codecs.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
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
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "SKCodecOrigin.TopLeft returned for DNG/CR2 files regardless of actual EXIF orientation",
      "reproQuality": "complete"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Open a DNG or CR2 raw image file with SKCodec.Create(stream)",
        "Read codec.Origin (or codec.EncodedOrigin in newer API)",
        "Observe that TopLeft is always returned, regardless of actual EXIF orientation"
      ],
      "environmentDetails": "Any platform; DNG and CR2 raw image files with EXIF orientation != TopLeft",
      "attachments": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/1616452/APC_0316.zip",
          "filename": "APC_0316.zip",
          "description": "Sample DNG file with non-default EXIF orientation"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/files/1616453/APC_0193.zip",
          "filename": "APC_0193.zip",
          "description": "Second sample DNG file with non-default EXIF orientation"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/files/1645431/IMG_1361.zip",
          "filename": "IMG_1361.zip",
          "description": "Sample CR2 file that should report LeftBottom but reports TopLeft"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The DNG (and CR2) codec in upstream Skia does not read the EXIF orientation tag, so sk_codec_get_origin always returns the default TopLeft value for raw image formats. Only JPEG and WebP codecs respect this property. The SkiaSharp binding (SKCodec.EncodedOrigin) correctly passes through the native value but cannot compensate for the upstream omission.",
    "rationale": "The issue is clearly a wrong-output bug: the reporter provides DNG and CR2 sample files and sample code demonstrating that orientation is always TopLeft when it should not be. The maintainer confirmed the root cause is in upstream Skia's DNG codec. The label status/skia-update-required already reflects this. Classification as type/bug is confirmed because the SKCodec contract promises the actual encoded origin, and the DNG codec fails to fulfill it. No platform label is needed as this is format-specific and cross-platform.",
    "keySignals": [
      {
        "text": "The following sample code results in SKCodecOrigin.TopLeft, which is incorrect.",
        "source": "issue body",
        "interpretation": "Wrong-output bug — orientation metadata not being read from DNG files."
      },
      {
        "text": "Looks like this is not limited to just dng. Here's a sample cr2 that is also affected. It is reported as TopLeft but it should be LeftBottom.",
        "source": "comment by LukePulverenti",
        "interpretation": "The problem affects multiple raw camera formats, not just DNG."
      },
      {
        "text": "The DNG codec ignores this. The only ones that appear to respect this are jpeg and webp.",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer-confirmed root cause in upstream Skia's DNG codec implementation."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "36-37",
        "finding": "SKCodec.EncodedOrigin simply delegates to SkiaApi.sk_codec_get_origin(Handle) — no transformation or filtering; the C# binding faithfully passes through whatever the native codec returns.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "sk_codec_get_origin is a direct P/Invoke to the native sk_codec_get_origin function in libSkiaSharp; the generated binding is correct and the issue is entirely in native Skia behavior.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use a third-party .NET EXIF library (e.g., MetadataExtractor) to read orientation from the raw file's metadata, then manually rotate/flip the decoded bitmap using SKBitmap transform methods.",
      "Convert raw files to JPEG before processing with SkiaSharp — JPEG orientation is correctly handled by the Skia JPEG codec."
    ],
    "nextQuestions": [
      "Has Skia upstream added DNG/CR2 orientation support in more recent milestones?",
      "Is the issue limited to DNG/CR2 or do other raw formats (ARW, NEF, RAF) share the same problem?"
    ],
    "resolution": {
      "hypothesis": "Upstream Skia's SkRawCodec (DNG/CR2 handler) does not parse or report EXIF orientation metadata, unlike the JPEG and WebP codecs that do. A fix requires updating Skia or post-processing orientation in SkiaSharp using a separate EXIF reader.",
      "proposals": [
        {
          "title": "Workaround: read orientation via MetadataExtractor",
          "description": "Use MetadataExtractor NuGet package to read EXIF orientation from the raw file independently, then manually rotate or flip the decoded SKBitmap to match.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Fix: update Skia to a milestone where DNG codec reads orientation",
          "description": "Investigate recent Skia milestones to determine whether the upstream DNG/CR2 codec has been updated to report orientation. If so, bump the Skia dependency.",
          "category": "fix",
          "confidence": 0.6,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Workaround: read orientation via MetadataExtractor",
      "recommendedReason": "Provides immediate relief for users without requiring an upstream Skia change. The fix path depends on upstream availability."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.85,
      "reason": "Root cause confirmed as upstream Skia DNG codec not reading orientation metadata. Already labeled status/skia-update-required. Issue should stay open until a Skia update resolves it or a SkiaSharp-level workaround is shipped.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug and area labels; add tenet/compatibility since raw camera format orientation is a compatibility concern.",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge root cause and suggest MetadataExtractor workaround",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the report and the sample files. The root cause is in upstream Skia: the DNG and CR2 raw-image codecs do not read the EXIF orientation tag, so `SKCodec.EncodedOrigin` always returns `TopLeft` for these formats. Only the JPEG and WebP codecs currently report orientation correctly.\n\nThis is tracked as a Skia upstream issue (`status/skia-update-required`).\n\n**Workaround:** Use the [MetadataExtractor](https://github.com/drewnoakes/metadata-extractor-dotnet) NuGet package to read the EXIF orientation from your DNG/CR2 file independently, then manually rotate or flip the decoded `SKBitmap` to match the actual orientation."
      }
    ]
  }
}
```

</details>
