# Issue Triage Report — #1565

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T02:45:00Z |
| Type | type/bug (0.82 (82%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-external (0.85 (85%)) |

**Issue Summary:** SKCodec.Create returns null for Sony a7m3 ARW raw images while older Sony a7r2 ARW files decode successfully, indicating the upstream Skia DNG codec does not support the newer ARW format variant.

**Analysis:** SKCodec.Create returns null for newer Sony ARW (a7m3) files because Skia's DNG/RAW codec does not recognise the newer ARW format variant. SkiaSharp's wrapper correctly returns null when Skia itself cannot open the file. This is an upstream Skia limitation, not a SkiaSharp bug.

**Recommendations:** **close-as-external** — Root cause is the upstream Skia native codec not supporting newer Sony ARW format variants. SkiaSharp's wrapper is correct; no fix is possible within SkiaSharp itself.

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

## Evidence

### Reproduction

1. Open a Sony a7m3 .ARW file via SKFileStream
2. Pass stream to SKCodec.Create(stream)
3. Observe codec is null

**Environment:** Sony a7m3 ARW file (newer format); Sony a7r2 ARW works fine

**Repository links:**
- https://github.com/gfeng85/BugsForSkiaSharp — Sample ARW files (a7m3 and a7r2) provided by reporter

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | missing-output |
| Error message | SKCodec.Create returns null for newer ARW files |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | No code change in SkiaSharp's SKCodec wrapper addresses this; codec support depends entirely on upstream Skia. |

## Analysis

### Technical Summary

SKCodec.Create returns null for newer Sony ARW (a7m3) files because Skia's DNG/RAW codec does not recognise the newer ARW format variant. SkiaSharp's wrapper correctly returns null when Skia itself cannot open the file. This is an upstream Skia limitation, not a SkiaSharp bug.

### Rationale

SKCodec.Create delegates directly to sk_codec_new_from_stream; when Skia's native codec cannot sniff/decode the format it returns a null handle. SKEncodedImageFormat.Dng (value 10) exists in the enum confirming some DNG support, but newer Sony ARW versions appear to use format features not handled by the bundled Skia codec. Maintainer (mattleibow) has already confirmed this is an upstream Skia concern.

### Key Signals

- "when the ArwFileName.ARW is token by Sony a7m3 Camera, codec will be null" — **issue body** (Skia codec sniff fails on newer ARW — returns null handle.)
- "old version of ARW can support well (e.g token by Sony a7r2 Camera)" — **issue body** (Older ARW variant works; only newer format unsupported — points to codec format-version gap.)
- "SkiaSharp just wraps the native library from Google, so this is probably a question for their forums" — **maintainer comment (mattleibow)** (Maintainer explicitly attributes the limitation to upstream Skia, not SkiaSharp.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCodec.cs` | 249-264 | direct | SKCodec.Create(SKStream) calls SkiaApi.sk_codec_new_from_stream and returns null via GetObject when the native handle is zero — no SkiaSharp-level bug, codec detection is entirely delegated to the native Skia library. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 20751-20780 | related | SKEncodedImageFormat enum includes Dng = 10 confirming Skia has partial DNG/RAW support, but whether newer Sony ARW variants are covered depends on the upstream Skia codec implementation, not on SkiaSharp. |

### Workarounds

- Use a third-party .NET RAW decoding library such as LibRawSharp or ImageMagick.NET to read ARW files and then create an SKBitmap from the decoded pixel data.
- Use platform-native camera APIs (e.g. iOS PHAsset, Android CameraX/Bitmap) to decode the RAW file and pass the resulting bitmap to SkiaSharp.

### Next Questions

- Has upstream Skia added support for newer Sony ARW variants since the issue was filed?
- Which SkiaSharp / Skia milestone version first introduced any ARW support?

### Resolution Proposals

**Hypothesis:** The Skia native library bundled with SkiaSharp does not support the ARW format variant used by the Sony a7m3 and newer cameras. Fix requires an upstream Skia change or a bump to a Skia version that includes updated ARW/DNG codec support.

1. **Use a third-party RAW decoder** — workaround, confidence 0.80 (80%), cost/m, validated=untested
   - Decode the ARW file with a dedicated .NET RAW library (e.g. LibRaw via P/Invoke or a NuGet wrapper) and convert the resulting bitmap to SKBitmap for further processing in SkiaSharp.
2. **Report to upstream Skia** — investigation, confidence 0.90 (90%), cost/xs, validated=untested
   - File a bug or check existing issues on skia.googlesource.com / the skia-discuss Google Group for ARW support in the DNG codec. The fix must land in upstream Skia before it can be consumed by SkiaSharp.

**Recommended proposal:** Use a third-party RAW decoder

**Why:** Provides an actionable path for the reporter today without waiting for an upstream Skia fix.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.85 (85%) |
| Reason | Root cause is the upstream Skia native codec not supporting newer Sony ARW format variants. SkiaSharp's wrapper is correct; no fix is possible within SkiaSharp itself. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, core area, and compatibility tenet labels | labels=type/bug, area/SkiaSharp, tenet/compatibility |
| add-comment | medium | 0.85 (85%) | Explain upstream Skia limitation and provide workaround | — |
| close-issue | medium | 0.80 (80%) | Close as external — fix must come from upstream Skia | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report and sample files.

SkiaSharp delegates image decoding entirely to the native Skia library. The `SKCodec.Create` returning `null` means Skia's built-in DNG/RAW codec cannot recognise the newer Sony a7m3 ARW format variant — this is an upstream Skia limitation rather than a SkiaSharp bug.

**Workarounds while waiting for an upstream fix:**
1. Use a dedicated .NET RAW decoding library (e.g. a LibRaw binding) to decode the ARW file to a pixel buffer, then create an `SKBitmap` from those pixels.
2. Use platform-native APIs (iOS `PHImageManager`, Android `BitmapFactory` / CameraX) to decode the RAW file first, then pass the bitmap to SkiaSharp.

For upstream tracking, please check [skia-discuss](https://groups.google.com/g/skia-discuss) or the Skia issue tracker for DNG/ARW support.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1565,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T02:45:00Z"
  },
  "summary": "SKCodec.Create returns null for Sony a7m3 ARW raw images while older Sony a7r2 ARW files decode successfully, indicating the upstream Skia DNG codec does not support the newer ARW format variant.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.82
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
      "severity": "low",
      "regressionClaimed": false,
      "errorType": "missing-output",
      "errorMessage": "SKCodec.Create returns null for newer ARW files",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Open a Sony a7m3 .ARW file via SKFileStream",
        "Pass stream to SKCodec.Create(stream)",
        "Observe codec is null"
      ],
      "environmentDetails": "Sony a7m3 ARW file (newer format); Sony a7r2 ARW works fine",
      "repoLinks": [
        {
          "url": "https://github.com/gfeng85/BugsForSkiaSharp",
          "description": "Sample ARW files (a7m3 and a7r2) provided by reporter"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "No code change in SkiaSharp's SKCodec wrapper addresses this; codec support depends entirely on upstream Skia."
    }
  },
  "analysis": {
    "summary": "SKCodec.Create returns null for newer Sony ARW (a7m3) files because Skia's DNG/RAW codec does not recognise the newer ARW format variant. SkiaSharp's wrapper correctly returns null when Skia itself cannot open the file. This is an upstream Skia limitation, not a SkiaSharp bug.",
    "rationale": "SKCodec.Create delegates directly to sk_codec_new_from_stream; when Skia's native codec cannot sniff/decode the format it returns a null handle. SKEncodedImageFormat.Dng (value 10) exists in the enum confirming some DNG support, but newer Sony ARW versions appear to use format features not handled by the bundled Skia codec. Maintainer (mattleibow) has already confirmed this is an upstream Skia concern.",
    "keySignals": [
      {
        "text": "when the ArwFileName.ARW is token by Sony a7m3 Camera, codec will be null",
        "source": "issue body",
        "interpretation": "Skia codec sniff fails on newer ARW — returns null handle."
      },
      {
        "text": "old version of ARW can support well (e.g token by Sony a7r2 Camera)",
        "source": "issue body",
        "interpretation": "Older ARW variant works; only newer format unsupported — points to codec format-version gap."
      },
      {
        "text": "SkiaSharp just wraps the native library from Google, so this is probably a question for their forums",
        "source": "maintainer comment (mattleibow)",
        "interpretation": "Maintainer explicitly attributes the limitation to upstream Skia, not SkiaSharp."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "249-264",
        "finding": "SKCodec.Create(SKStream) calls SkiaApi.sk_codec_new_from_stream and returns null via GetObject when the native handle is zero — no SkiaSharp-level bug, codec detection is entirely delegated to the native Skia library.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "20751-20780",
        "finding": "SKEncodedImageFormat enum includes Dng = 10 confirming Skia has partial DNG/RAW support, but whether newer Sony ARW variants are covered depends on the upstream Skia codec implementation, not on SkiaSharp.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use a third-party .NET RAW decoding library such as LibRawSharp or ImageMagick.NET to read ARW files and then create an SKBitmap from the decoded pixel data.",
      "Use platform-native camera APIs (e.g. iOS PHAsset, Android CameraX/Bitmap) to decode the RAW file and pass the resulting bitmap to SkiaSharp."
    ],
    "nextQuestions": [
      "Has upstream Skia added support for newer Sony ARW variants since the issue was filed?",
      "Which SkiaSharp / Skia milestone version first introduced any ARW support?"
    ],
    "resolution": {
      "hypothesis": "The Skia native library bundled with SkiaSharp does not support the ARW format variant used by the Sony a7m3 and newer cameras. Fix requires an upstream Skia change or a bump to a Skia version that includes updated ARW/DNG codec support.",
      "proposals": [
        {
          "title": "Use a third-party RAW decoder",
          "description": "Decode the ARW file with a dedicated .NET RAW library (e.g. LibRaw via P/Invoke or a NuGet wrapper) and convert the resulting bitmap to SKBitmap for further processing in SkiaSharp.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Report to upstream Skia",
          "description": "File a bug or check existing issues on skia.googlesource.com / the skia-discuss Google Group for ARW support in the DNG codec. The fix must land in upstream Skia before it can be consumed by SkiaSharp.",
          "category": "investigation",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use a third-party RAW decoder",
      "recommendedReason": "Provides an actionable path for the reporter today without waiting for an upstream Skia fix."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.85,
      "reason": "Root cause is the upstream Skia native codec not supporting newer Sony ARW format variants. SkiaSharp's wrapper is correct; no fix is possible within SkiaSharp itself.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, core area, and compatibility tenet labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain upstream Skia limitation and provide workaround",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the report and sample files.\n\nSkiaSharp delegates image decoding entirely to the native Skia library. The `SKCodec.Create` returning `null` means Skia's built-in DNG/RAW codec cannot recognise the newer Sony a7m3 ARW format variant — this is an upstream Skia limitation rather than a SkiaSharp bug.\n\n**Workarounds while waiting for an upstream fix:**\n1. Use a dedicated .NET RAW decoding library (e.g. a LibRaw binding) to decode the ARW file to a pixel buffer, then create an `SKBitmap` from those pixels.\n2. Use platform-native APIs (iOS `PHImageManager`, Android `BitmapFactory` / CameraX) to decode the RAW file first, then pass the bitmap to SkiaSharp.\n\nFor upstream tracking, please check [skia-discuss](https://groups.google.com/g/skia-discuss) or the Skia issue tracker for DNG/ARW support."
      },
      {
        "type": "close-issue",
        "description": "Close as external — fix must come from upstream Skia",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
