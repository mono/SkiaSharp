# Issue Triage Report — #2312

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T17:30:00Z |
| Type | type/question (0.85 (85%)) |
| Area | area/SkiaSharp (0.88 (88%)) |
| Suggested action | needs-investigation (0.75 (75%)) |

**Issue Summary:** Reporter asks whether SkiaSharp supports animated AVIF images (multi-frame AVIF), noting that AVIF format supports frame animation.

**Analysis:** Reporter asks if SkiaSharp supports animated AVIF. SkiaSharp exposes SKCodec with FrameCount, FrameInfo, and RepetitionCount APIs which serve animation for GIF/WebP. AVIF is recognized as SKEncodedImageFormat.Avif. Whether Skia's AVIF codec actually decodes multiple frames depends on Skia build and version; the API surface exists but AVIF animated support in the underlying Skia codec is uncertain.

**Recommendations:** **needs-investigation** — Animation support depends on Skia's AVIF codec implementation in the bundled version. Needs a test with SKCodec to confirm if FrameCount works for animated AVIF.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://files.catbox.moe/w3meta.avif — Example animated AVIF file provided by reporter

## Analysis

### Technical Summary

Reporter asks if SkiaSharp supports animated AVIF. SkiaSharp exposes SKCodec with FrameCount, FrameInfo, and RepetitionCount APIs which serve animation for GIF/WebP. AVIF is recognized as SKEncodedImageFormat.Avif. Whether Skia's AVIF codec actually decodes multiple frames depends on Skia build and version; the API surface exists but AVIF animated support in the underlying Skia codec is uncertain.

### Rationale

This is a question about feature availability, not a bug report. The reporter correctly identifies that AVIF supports animation and wants to know if SkiaSharp passes that through. The SKCodec animation API infrastructure exists; AVIF codec animation support depends on the underlying Skia implementation.

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCodec.cs` | 68-90 | direct | SKCodec exposes FrameCount, RepetitionCount, FrameInfo[], and GetFrameInfo(int, out SKCodecFrameInfo) for animated image decoding. This API works for animated GIF and WebP and would work for animated AVIF if the underlying Skia codec supports it. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 21054-21055 | context | SKEncodedImageFormat.Avif = 12 is present, confirming AVIF is a recognized format in SkiaSharp. |

### Workarounds

- Use SKCodec.Create() with an AVIF stream, then check FrameCount — if > 1, animated AVIF is supported by the current Skia build. Iterate frames with GetPixels using SKCodecOptions with FrameIndex set to each frame index.

### Next Questions

- Does the version of Skia bundled with SkiaSharp at the time of the issue support multi-frame AVIF decoding?
- What SkiaSharp version is the reporter using?

### Resolution Proposals

**Hypothesis:** SkiaSharp has the API infrastructure (SKCodec.FrameCount, FrameInfo) to decode animated images. Whether animated AVIF specifically works depends on Skia's AVIF codec support in the bundled version.

1. **Test animated AVIF via SKCodec** — workaround, confidence 0.70 (70%), cost/xs, validated=untested
   - Use SKCodec.Create() on an AVIF file, check FrameCount > 1, and iterate frames using GetPixels with SKCodecOptions.FrameIndex. The same pattern used for animated GIF/WebP.

**Recommended proposal:** Test animated AVIF via SKCodec

**Why:** The existing SKCodec animation API should be tried first; if FrameCount returns > 1 for the AVIF file, the feature is already supported.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.75 (75%) |
| Reason | Animation support depends on Skia's AVIF codec implementation in the bundled version. Needs a test with SKCodec to confirm if FrameCount works for animated AVIF. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply question and SkiaSharp area labels | labels=type/question, area/SkiaSharp |
| add-comment | medium | 0.75 (75%) | Answer with guidance on using SKCodec for animated images and note uncertainty on AVIF animated support | — |

**Comment draft for `add-comment`:**

```markdown
SkiaSharp's `SKCodec` class provides animation support via `FrameCount`, `RepetitionCount`, and `FrameInfo` properties, which work for animated GIF and WebP images. Whether animated AVIF works depends on the AVIF decoder in the bundled Skia version.

You can test this with:

```csharp
using var stream = File.OpenRead("animation.avif");
using var codec = SKCodec.Create(stream);

Console.WriteLine($"Frame count: {codec.FrameCount}");
Console.WriteLine($"Repetitions: {codec.RepetitionCount}");

for (int i = 0; i < codec.FrameCount; i++) {
    var options = new SKCodecOptions(i);
    var bitmap = new SKBitmap(codec.Info);
    using var pixmap = bitmap.PeekPixels();
    codec.GetPixels(codec.Info, pixmap.GetPixels(), options);
    // use bitmap for this frame
}
```

If `FrameCount` returns `1` for an animated AVIF file, it means the bundled Skia version does not yet support animated AVIF decoding. Please share which SkiaSharp version you are using.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2312,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T17:30:00Z"
  },
  "summary": "Reporter asks whether SkiaSharp supports animated AVIF images (multi-frame AVIF), noting that AVIF format supports frame animation.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.85
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.88
    }
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://files.catbox.moe/w3meta.avif",
          "description": "Example animated AVIF file provided by reporter"
        }
      ]
    }
  },
  "analysis": {
    "summary": "Reporter asks if SkiaSharp supports animated AVIF. SkiaSharp exposes SKCodec with FrameCount, FrameInfo, and RepetitionCount APIs which serve animation for GIF/WebP. AVIF is recognized as SKEncodedImageFormat.Avif. Whether Skia's AVIF codec actually decodes multiple frames depends on Skia build and version; the API surface exists but AVIF animated support in the underlying Skia codec is uncertain.",
    "rationale": "This is a question about feature availability, not a bug report. The reporter correctly identifies that AVIF supports animation and wants to know if SkiaSharp passes that through. The SKCodec animation API infrastructure exists; AVIF codec animation support depends on the underlying Skia implementation.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "68-90",
        "finding": "SKCodec exposes FrameCount, RepetitionCount, FrameInfo[], and GetFrameInfo(int, out SKCodecFrameInfo) for animated image decoding. This API works for animated GIF and WebP and would work for animated AVIF if the underlying Skia codec supports it.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "21054-21055",
        "finding": "SKEncodedImageFormat.Avif = 12 is present, confirming AVIF is a recognized format in SkiaSharp.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "Use SKCodec.Create() with an AVIF stream, then check FrameCount — if > 1, animated AVIF is supported by the current Skia build. Iterate frames with GetPixels using SKCodecOptions with FrameIndex set to each frame index."
    ],
    "nextQuestions": [
      "Does the version of Skia bundled with SkiaSharp at the time of the issue support multi-frame AVIF decoding?",
      "What SkiaSharp version is the reporter using?"
    ],
    "resolution": {
      "hypothesis": "SkiaSharp has the API infrastructure (SKCodec.FrameCount, FrameInfo) to decode animated images. Whether animated AVIF specifically works depends on Skia's AVIF codec support in the bundled version.",
      "proposals": [
        {
          "title": "Test animated AVIF via SKCodec",
          "description": "Use SKCodec.Create() on an AVIF file, check FrameCount > 1, and iterate frames using GetPixels with SKCodecOptions.FrameIndex. The same pattern used for animated GIF/WebP.",
          "category": "workaround",
          "confidence": 0.7,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Test animated AVIF via SKCodec",
      "recommendedReason": "The existing SKCodec animation API should be tried first; if FrameCount returns > 1 for the AVIF file, the feature is already supported."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.75,
      "reason": "Animation support depends on Skia's AVIF codec implementation in the bundled version. Needs a test with SKCodec to confirm if FrameCount works for animated AVIF.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and SkiaSharp area labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Answer with guidance on using SKCodec for animated images and note uncertainty on AVIF animated support",
        "risk": "medium",
        "confidence": 0.75,
        "comment": "SkiaSharp's `SKCodec` class provides animation support via `FrameCount`, `RepetitionCount`, and `FrameInfo` properties, which work for animated GIF and WebP images. Whether animated AVIF works depends on the AVIF decoder in the bundled Skia version.\n\nYou can test this with:\n\n```csharp\nusing var stream = File.OpenRead(\"animation.avif\");\nusing var codec = SKCodec.Create(stream);\n\nConsole.WriteLine($\"Frame count: {codec.FrameCount}\");\nConsole.WriteLine($\"Repetitions: {codec.RepetitionCount}\");\n\nfor (int i = 0; i < codec.FrameCount; i++) {\n    var options = new SKCodecOptions(i);\n    var bitmap = new SKBitmap(codec.Info);\n    using var pixmap = bitmap.PeekPixels();\n    codec.GetPixels(codec.Info, pixmap.GetPixels(), options);\n    // use bitmap for this frame\n}\n```\n\nIf `FrameCount` returns `1` for an animated AVIF file, it means the bundled Skia version does not yet support animated AVIF decoding. Please share which SkiaSharp version you are using."
      }
    ]
  }
}
```

</details>
