# Issue Triage Report — #1722

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T10:31:10Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | keep-open (0.88 (88%)) |

**Issue Summary:** Request to add a helper method that combines DCT-scale decoding via SKCodec with EXIF auto-orientation in a single API call, removing the ~200-line boilerplate developers currently need.

**Analysis:** SkiaSharp currently requires ~200 lines of boilerplate to load a JPEG using DCT-scaled decoding (via SKCodec.GetScaledDimensions) and then manually apply EXIF auto-orientation. A convenience method wrapping both steps would be ~6x faster than SKImage.FromEncodedData for thumbnail generation and would remove the burden on every developer who needs correctly-oriented scaled thumbnails.

**Recommendations:** **keep-open** — Well-justified feature request with community support and a related open issue (#836). No breaking change required. Needs API design discussion before implementation.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/performance |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/836 — Related: Image auto orientation method request
- https://github.com/mono/SkiaSharp/issues/1517 — Related: codec orientation bug, contains workaround implementation

**Code snippets:**

```csharp
SKCodec codec = SKCodec.Create(source.FullName);
SKImageInfo info = codec.Info;
var supportedScale = codec.GetScaledDimensions((float)desiredWidth / info.Width);
var nearest = new SKImageInfo(supportedScale.Width, supportedScale.Height);
var bmp = SKBitmap.Decode(codec, nearest);
// then manually apply codec.EncodedOrigin ...
```

## Analysis

### Technical Summary

SkiaSharp currently requires ~200 lines of boilerplate to load a JPEG using DCT-scaled decoding (via SKCodec.GetScaledDimensions) and then manually apply EXIF auto-orientation. A convenience method wrapping both steps would be ~6x faster than SKImage.FromEncodedData for thumbnail generation and would remove the burden on every developer who needs correctly-oriented scaled thumbnails.

### Rationale

The request is for a completely new API method that does not currently exist in SkiaSharp. Both the performance case (~6-10x speedup measured on M1) and usability case (removing ~200 lines of boilerplate) are well-argued. Related issue #836 requested AutoOrient() and is still open. This is a legitimate feature request with good community traction (4 upvotes, follow-up comment). Classified as type/feature-request since no such combined method exists anywhere in the codebase.

### Key Signals

- "doing the second approach takes around 150ms ... so it's nearly an order of magnitude faster to use the codec+orient method, despite being about 200 lines of code instead of two." — **issue body** (Strong concrete performance justification with benchmarks for the new API.)
- "most developers/end users of SkiaSharp are likely to be loading down-scaled images, correctly oriented, for their apps" — **issue body** (High potential user impact — thumbnail generation is a very common use case.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCodec.cs` | 36-47 | direct | SKCodec exposes EncodedOrigin property and GetScaledDimensions but no helper combining them into an oriented-decode workflow. |
| `binding/SkiaSharp/SKBitmap.cs` | 446-464 | direct | SKBitmap.Decode(SKCodec, SKImageInfo) exists and can decode at a scaled resolution, but no overload handles auto-orientation after decode. |

### Workarounds

- Use SKCodec.Create, call GetScaledDimensions with desired scale, decode with SKBitmap.Decode(codec, scaledInfo), then manually apply codec.EncodedOrigin via SKBitmap transformation matrix — as described in issue #836 comment thread.

### Next Questions

- Should the new API live on SKCodec, SKBitmap, or SKImage?
- Should it return SKBitmap or SKImage?
- What should the exact signature look like (separate width/height, or SKSizeI, or float scale)?

### Resolution Proposals

**Hypothesis:** Add a static helper method (e.g., SKBitmap.DecodeOriented or SKCodec-based utility) that internally calls GetScaledDimensions, decodes to the nearest scale, and applies EXIF orientation correction.

1. **Add SKBitmap.DecodeOriented(string filename, int desiredWidth, int desiredHeight)** — investigation, validated=untested
   - Add a new static method on SKBitmap that wraps codec creation, GetScaledDimensions, Decode, and EncodedOrigin-based rotation in a single convenient call.

**Recommended proposal:** Add SKBitmap.DecodeOriented(string filename, int desiredWidth, int desiredHeight)

**Why:** Straightforward API addition that directly solves the stated need; design decision (return type, exact placement) needs maintainer input before implementation.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.88 (88%) |
| Reason | Well-justified feature request with community support and a related open issue (#836). No breaking change required. Needs API design discussion before implementation. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply type/feature-request, area/SkiaSharp, tenet/performance | labels=type/feature-request, area/SkiaSharp, tenet/performance |
| link-related | low | 0.95 (95%) | Cross-reference related AutoOrient request #836 | linkedIssue=#836 |
| link-related | low | 0.90 (90%) | Cross-reference related codec orientation issue #1517 | linkedIssue=#1517 |
| add-comment | medium | 0.85 (85%) | Acknowledge the request, point to the workaround in #836, and ask for API design preferences | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed write-up with benchmark numbers! This is a great feature request.

While we evaluate adding a built-in helper, the workaround discussed in #836 demonstrates the codec + orientation approach. The general pattern is:

```csharp
using var codec = SKCodec.Create(filePath);
var info = codec.Info;
var scale = codec.GetScaledDimensions((float)desiredWidth / info.Width);
var scaled = new SKImageInfo(scale.Width, scale.Height);
using var bitmap = SKBitmap.Decode(codec, scaled);
// then apply codec.EncodedOrigin via a canvas transformation
```

For the proposed API, a few design questions worth discussing:
1. Should the method return `SKBitmap` or `SKImage`?
2. Should it live on `SKBitmap`, `SKCodec`, or as a new utility class?
3. Should the desired size be passed as separate `width`/`height` integers, an `SKSizeI`, or a scale factor?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1722,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T10:31:10Z"
  },
  "summary": "Request to add a helper method that combines DCT-scale decoding via SKCodec with EXIF auto-orientation in a single API call, removing the ~200-line boilerplate developers currently need.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "SKCodec codec = SKCodec.Create(source.FullName);\nSKImageInfo info = codec.Info;\nvar supportedScale = codec.GetScaledDimensions((float)desiredWidth / info.Width);\nvar nearest = new SKImageInfo(supportedScale.Width, supportedScale.Height);\nvar bmp = SKBitmap.Decode(codec, nearest);\n// then manually apply codec.EncodedOrigin ..."
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/836",
          "description": "Related: Image auto orientation method request"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1517",
          "description": "Related: codec orientation bug, contains workaround implementation"
        }
      ]
    }
  },
  "analysis": {
    "summary": "SkiaSharp currently requires ~200 lines of boilerplate to load a JPEG using DCT-scaled decoding (via SKCodec.GetScaledDimensions) and then manually apply EXIF auto-orientation. A convenience method wrapping both steps would be ~6x faster than SKImage.FromEncodedData for thumbnail generation and would remove the burden on every developer who needs correctly-oriented scaled thumbnails.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "finding": "SKCodec exposes EncodedOrigin property and GetScaledDimensions but no helper combining them into an oriented-decode workflow.",
        "relevance": "direct",
        "lines": "36-47"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "finding": "SKBitmap.Decode(SKCodec, SKImageInfo) exists and can decode at a scaled resolution, but no overload handles auto-orientation after decode.",
        "relevance": "direct",
        "lines": "446-464"
      }
    ],
    "keySignals": [
      {
        "text": "doing the second approach takes around 150ms ... so it's nearly an order of magnitude faster to use the codec+orient method, despite being about 200 lines of code instead of two.",
        "source": "issue body",
        "interpretation": "Strong concrete performance justification with benchmarks for the new API."
      },
      {
        "text": "most developers/end users of SkiaSharp are likely to be loading down-scaled images, correctly oriented, for their apps",
        "source": "issue body",
        "interpretation": "High potential user impact — thumbnail generation is a very common use case."
      }
    ],
    "rationale": "The request is for a completely new API method that does not currently exist in SkiaSharp. Both the performance case (~6-10x speedup measured on M1) and usability case (removing ~200 lines of boilerplate) are well-argued. Related issue #836 requested AutoOrient() and is still open. This is a legitimate feature request with good community traction (4 upvotes, follow-up comment). Classified as type/feature-request since no such combined method exists anywhere in the codebase.",
    "workarounds": [
      "Use SKCodec.Create, call GetScaledDimensions with desired scale, decode with SKBitmap.Decode(codec, scaledInfo), then manually apply codec.EncodedOrigin via SKBitmap transformation matrix — as described in issue #836 comment thread."
    ],
    "nextQuestions": [
      "Should the new API live on SKCodec, SKBitmap, or SKImage?",
      "Should it return SKBitmap or SKImage?",
      "What should the exact signature look like (separate width/height, or SKSizeI, or float scale)?"
    ],
    "resolution": {
      "hypothesis": "Add a static helper method (e.g., SKBitmap.DecodeOriented or SKCodec-based utility) that internally calls GetScaledDimensions, decodes to the nearest scale, and applies EXIF orientation correction.",
      "proposals": [
        {
          "title": "Add SKBitmap.DecodeOriented(string filename, int desiredWidth, int desiredHeight)",
          "description": "Add a new static method on SKBitmap that wraps codec creation, GetScaledDimensions, Decode, and EncodedOrigin-based rotation in a single convenient call.",
          "category": "investigation",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add SKBitmap.DecodeOriented(string filename, int desiredWidth, int desiredHeight)",
      "recommendedReason": "Straightforward API addition that directly solves the stated need; design decision (return type, exact placement) needs maintainer input before implementation."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.88,
      "reason": "Well-justified feature request with community support and a related open issue (#836). No breaking change required. Needs API design discussion before implementation.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/feature-request, area/SkiaSharp, tenet/performance",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "tenet/performance"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference related AutoOrient request #836",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 836
      },
      {
        "type": "link-related",
        "description": "Cross-reference related codec orientation issue #1517",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 1517
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the request, point to the workaround in #836, and ask for API design preferences",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed write-up with benchmark numbers! This is a great feature request.\n\nWhile we evaluate adding a built-in helper, the workaround discussed in #836 demonstrates the codec + orientation approach. The general pattern is:\n\n```csharp\nusing var codec = SKCodec.Create(filePath);\nvar info = codec.Info;\nvar scale = codec.GetScaledDimensions((float)desiredWidth / info.Width);\nvar scaled = new SKImageInfo(scale.Width, scale.Height);\nusing var bitmap = SKBitmap.Decode(codec, scaled);\n// then apply codec.EncodedOrigin via a canvas transformation\n```\n\nFor the proposed API, a few design questions worth discussing:\n1. Should the method return `SKBitmap` or `SKImage`?\n2. Should it live on `SKBitmap`, `SKCodec`, or as a new utility class?\n3. Should the desired size be passed as separate `width`/`height` integers, an `SKSizeI`, or a scale factor?"
      }
    ]
  }
}
```

</details>
