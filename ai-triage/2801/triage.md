# Issue Triage Report — #2801

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-05T18:06:48Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-investigation (0.85 (85%)) |

**Issue Summary:** SKBitmap.Decode(string) returns null for a specific PNG file extracted from an FBX 3D model, even though the PNG opens correctly in Photoshop, IrfanView, and Windows Photo Viewer.

**Analysis:** Skia's underlying PNG decoder rejects this specific PNG file (likely due to non-standard or unusual PNG chunks from the FBX export), causing SKCodec.Create() to return null, which propagates as a null return from SKBitmap.Decode(). The same file is accepted by more lenient decoders (Photoshop, Windows Photo Viewer). This is a recurring pattern in SkiaSharp where Skia's strict decoder rejects valid-but-unusual images.

**Recommendations:** **needs-investigation** — Concrete repro file provided; the bug is real and reproducible. Multiple related open issues confirm a pattern. Needs native-level investigation into what PNG characteristic causes Skia to reject the file.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Extract an embedded PNG texture from an FBX 3D model file
2. Call SKBitmap.Decode(pathToExtractedPng)
3. Observe null return value

**Environment:** SkiaSharp 2.88.3, Visual Studio on Windows

**Repository links:**
- https://github.com/mono/SkiaSharp/assets/9727447/cfea40ee-71f9-45e2-8103-774e02fbbce4 — Attached PNG file that fails to decode
- https://github.com/mono/SkiaSharp/issues/2429 — Related: SKBitmap.Decode always returns null for a specific image (JPEG)
- https://github.com/mono/SkiaSharp/issues/2759 — Related: SKBitmap.Decode returns null when decode PNG image

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | missing-output |
| Error message | SKBitmap.Decode(string) returns null |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | No fix has been identified for this class of Skia PNG decoder strictness issue. Related issues #2429 and #2759 remain open. |

## Analysis

### Technical Summary

Skia's underlying PNG decoder rejects this specific PNG file (likely due to non-standard or unusual PNG chunks from the FBX export), causing SKCodec.Create() to return null, which propagates as a null return from SKBitmap.Decode(). The same file is accepted by more lenient decoders (Photoshop, Windows Photo Viewer). This is a recurring pattern in SkiaSharp where Skia's strict decoder rejects valid-but-unusual images.

### Rationale

Reporter provides a concrete PNG file that fails. The API returns null silently without explanation. Code inspection shows SKBitmap.Decode(string) calls SKCodec.Create(filename) and returns null if the codec is null — no error detail is surfaced. Multiple related issues (#2429, #2759) describe the same pattern. The root cause likely lives in upstream Skia's PNG decoder strictness.

### Key Signals

- "SKBitmap.Decode(string) returns null" — **issue title** (Codec creation fails at the Skia native layer — the PNG is being rejected before decoding begins.)
- "works without problems in other applications (Photoshop, Irfanview, Windows Photo Viewer)" — **issue body** (The file is a valid PNG, but Skia's decoder is stricter than OS/commercial decoders.)
- "extracted from a 3D model file (FBX)" — **issue body** (FBX-embedded textures may contain non-standard PNG metadata chunks that confuse Skia's codec.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 539-550 | direct | SKBitmap.Decode(string) creates an SKCodec via SKCodec.Create(filename) and returns null silently when the codec is null — no error detail is surfaced to the caller. |
| `binding/SkiaSharp/SKCodec.cs` | 229-241 | direct | SKCodec.Create(string filename) opens an SKFileStream then calls Create(SKStream stream, out SKCodecResult result). If the native sk_codec_new_from_stream returns null (decoder rejection), the whole chain returns null. |
| `binding/SkiaSharp/SKBitmap.cs` | 446-459 | related | SKBitmap.Decode(SKCodec codec, SKImageInfo) returns null if codec.GetPixels result is neither Success nor IncompleteInput — but this code is never reached when the codec itself is null. |

### Workarounds

- Try loading via SKData.Create(File.ReadAllBytes(path)) and SKBitmap.Decode(data) to bypass file-stream path
- Try SKImage.FromEncodedData(File.ReadAllBytes(path)) as an alternative decoder path
- Use System.Drawing or ImageSharp to decode the PNG first, then create an SKBitmap from raw pixel data

### Next Questions

- Does SKBitmap.Decode(byte[]) with the same file also return null?
- What SKCodecResult is returned by SKCodec.Create(filename, out result) for this file?
- Does the PNG have unusual metadata chunks (iTXt, tEXt, zTXt, private chunks) that might trigger rejection?
- Is this reproducible on Linux/macOS or only Windows?

### Resolution Proposals

**Hypothesis:** Skia's native PNG decoder rejects the file due to non-standard PNG chunks present in FBX-exported textures. The same issue affects any PNG with unusual-but-valid metadata.

1. **Use byte array decode as workaround** — workaround, confidence 0.65 (65%), cost/xs, validated=untested
   - Load the file bytes manually and decode via SKBitmap.Decode(byte[]) or SKImage.FromEncodedData(). Some codec paths differ.
2. **Investigate upstream Skia PNG codec strictness** — investigation, confidence 0.80 (80%), cost/m, validated=untested
   - Examine what PNG chunk causes rejection in the native sk_codec_new_from_stream call. Compare with upstream Skia bug tracker and consider patching the C shim or filing an upstream issue.

**Recommended proposal:** Investigate upstream Skia PNG codec strictness

**Why:** Multiple reports (2429, 2759) of the same pattern suggest a systemic issue in Skia's decoder that needs root-cause analysis.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.85 (85%) |
| Reason | Concrete repro file provided; the bug is real and reproducible. Multiple related open issues confirm a pattern. Needs native-level investigation into what PNG characteristic causes Skia to reject the file. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, area, platform, and tenet labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/compatibility |
| link-related | low | 0.85 (85%) | Cross-reference related issue #2429 (same decode-returns-null pattern) | linkedIssue=#2429 |
| link-related | low | 0.85 (85%) | Cross-reference related issue #2759 (same decode-returns-null pattern for PNG) | linkedIssue=#2759 |
| add-comment | medium | 0.85 (85%) | Acknowledge the bug, provide workaround suggestions, and ask for diagnostic info | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report and the attached PNG!

This is a known class of issue where Skia's native PNG decoder is stricter than OS/commercial decoders and rejects certain valid-but-unusual files. Related: #2429, #2759.

While we investigate, here are some workarounds to try:

```csharp
// Option 1: Decode from byte array
var bytes = File.ReadAllBytes(path);
var bitmap = SKBitmap.Decode(bytes);

// Option 2: Via SKImage
using var data = SKData.Create(path);
var image = SKImage.FromEncodedData(data);
var bitmap = image?.ToRasterImage() != null ? SKBitmap.FromImage(image) : null;
```

To help diagnose, could you check what error code is returned?

```csharp
using var codec = SKCodec.Create(path, out var result);
Console.WriteLine($"Codec: {codec}, Result: {result}");
```
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2801,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-05T18:06:48Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKBitmap.Decode(string) returns null for a specific PNG file extracted from an FBX 3D model, even though the PNG opens correctly in Photoshop, IrfanView, and Windows Photo Viewer.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "missing-output",
      "errorMessage": "SKBitmap.Decode(string) returns null",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Extract an embedded PNG texture from an FBX 3D model file",
        "Call SKBitmap.Decode(pathToExtractedPng)",
        "Observe null return value"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, Visual Studio on Windows",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/assets/9727447/cfea40ee-71f9-45e2-8103-774e02fbbce4",
          "description": "Attached PNG file that fails to decode"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2429",
          "description": "Related: SKBitmap.Decode always returns null for a specific image (JPEG)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2759",
          "description": "Related: SKBitmap.Decode returns null when decode PNG image"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "No fix has been identified for this class of Skia PNG decoder strictness issue. Related issues #2429 and #2759 remain open."
    }
  },
  "analysis": {
    "summary": "Skia's underlying PNG decoder rejects this specific PNG file (likely due to non-standard or unusual PNG chunks from the FBX export), causing SKCodec.Create() to return null, which propagates as a null return from SKBitmap.Decode(). The same file is accepted by more lenient decoders (Photoshop, Windows Photo Viewer). This is a recurring pattern in SkiaSharp where Skia's strict decoder rejects valid-but-unusual images.",
    "rationale": "Reporter provides a concrete PNG file that fails. The API returns null silently without explanation. Code inspection shows SKBitmap.Decode(string) calls SKCodec.Create(filename) and returns null if the codec is null — no error detail is surfaced. Multiple related issues (#2429, #2759) describe the same pattern. The root cause likely lives in upstream Skia's PNG decoder strictness.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "539-550",
        "finding": "SKBitmap.Decode(string) creates an SKCodec via SKCodec.Create(filename) and returns null silently when the codec is null — no error detail is surfaced to the caller.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "229-241",
        "finding": "SKCodec.Create(string filename) opens an SKFileStream then calls Create(SKStream stream, out SKCodecResult result). If the native sk_codec_new_from_stream returns null (decoder rejection), the whole chain returns null.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "446-459",
        "finding": "SKBitmap.Decode(SKCodec codec, SKImageInfo) returns null if codec.GetPixels result is neither Success nor IncompleteInput — but this code is never reached when the codec itself is null.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "SKBitmap.Decode(string) returns null",
        "source": "issue title",
        "interpretation": "Codec creation fails at the Skia native layer — the PNG is being rejected before decoding begins."
      },
      {
        "text": "works without problems in other applications (Photoshop, Irfanview, Windows Photo Viewer)",
        "source": "issue body",
        "interpretation": "The file is a valid PNG, but Skia's decoder is stricter than OS/commercial decoders."
      },
      {
        "text": "extracted from a 3D model file (FBX)",
        "source": "issue body",
        "interpretation": "FBX-embedded textures may contain non-standard PNG metadata chunks that confuse Skia's codec."
      }
    ],
    "workarounds": [
      "Try loading via SKData.Create(File.ReadAllBytes(path)) and SKBitmap.Decode(data) to bypass file-stream path",
      "Try SKImage.FromEncodedData(File.ReadAllBytes(path)) as an alternative decoder path",
      "Use System.Drawing or ImageSharp to decode the PNG first, then create an SKBitmap from raw pixel data"
    ],
    "nextQuestions": [
      "Does SKBitmap.Decode(byte[]) with the same file also return null?",
      "What SKCodecResult is returned by SKCodec.Create(filename, out result) for this file?",
      "Does the PNG have unusual metadata chunks (iTXt, tEXt, zTXt, private chunks) that might trigger rejection?",
      "Is this reproducible on Linux/macOS or only Windows?"
    ],
    "resolution": {
      "hypothesis": "Skia's native PNG decoder rejects the file due to non-standard PNG chunks present in FBX-exported textures. The same issue affects any PNG with unusual-but-valid metadata.",
      "proposals": [
        {
          "title": "Use byte array decode as workaround",
          "description": "Load the file bytes manually and decode via SKBitmap.Decode(byte[]) or SKImage.FromEncodedData(). Some codec paths differ.",
          "category": "workaround",
          "confidence": 0.65,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Investigate upstream Skia PNG codec strictness",
          "description": "Examine what PNG chunk causes rejection in the native sk_codec_new_from_stream call. Compare with upstream Skia bug tracker and consider patching the C shim or filing an upstream issue.",
          "category": "investigation",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Investigate upstream Skia PNG codec strictness",
      "recommendedReason": "Multiple reports (2429, 2759) of the same pattern suggest a systemic issue in Skia's decoder that needs root-cause analysis."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "Concrete repro file provided; the bug is real and reproducible. Multiple related open issues confirm a pattern. Needs native-level investigation into what PNG characteristic causes Skia to reject the file.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, area, platform, and tenet labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/compatibility"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference related issue #2429 (same decode-returns-null pattern)",
        "risk": "low",
        "confidence": 0.85,
        "linkedIssue": 2429
      },
      {
        "type": "link-related",
        "description": "Cross-reference related issue #2759 (same decode-returns-null pattern for PNG)",
        "risk": "low",
        "confidence": 0.85,
        "linkedIssue": 2759
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the bug, provide workaround suggestions, and ask for diagnostic info",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the report and the attached PNG!\n\nThis is a known class of issue where Skia's native PNG decoder is stricter than OS/commercial decoders and rejects certain valid-but-unusual files. Related: #2429, #2759.\n\nWhile we investigate, here are some workarounds to try:\n\n```csharp\n// Option 1: Decode from byte array\nvar bytes = File.ReadAllBytes(path);\nvar bitmap = SKBitmap.Decode(bytes);\n\n// Option 2: Via SKImage\nusing var data = SKData.Create(path);\nvar image = SKImage.FromEncodedData(data);\nvar bitmap = image?.ToRasterImage() != null ? SKBitmap.FromImage(image) : null;\n```\n\nTo help diagnose, could you check what error code is returned?\n\n```csharp\nusing var codec = SKCodec.Create(path, out var result);\nConsole.WriteLine($\"Codec: {codec}, Result: {result}\");\n```"
      }
    ]
  }
}
```

</details>
