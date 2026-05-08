# Issue Triage Report — #836

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T17:55:40Z |
| Type | type/feature-request (0.95 (95%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | keep-open (0.88 (88%)) |

**Issue Summary:** Feature request for an AutoOrient() convenience method that applies EXIF orientation data to correctly rotate/flip images decoded from mobile camera uploads.

**Analysis:** SkiaSharp lacks a built-in AutoOrient() helper method. The SKEncodedOrigin enum already exists (accessible via SKCodec.EncodedOrigin), but there is no convenience method to apply the transformation to an SKBitmap or SKImage. Multiple community workarounds using canvas transforms exist. The maintainer (mattleibow) acknowledged this is desirable. The SKImage path may already handle this transparently according to one commenter (ziriax), but SKBitmap.Decode() does not.

**Recommendations:** **keep-open** — Valid feature request acknowledged by maintainer, with clear use case and community demand. No built-in API exists to satisfy this request. Multiple workarounds exist so not blocking, but the feature is genuinely useful and in scope.

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

1. Take a photo on a mobile device (typically saved in landscape regardless of device orientation)
2. Decode the image using SKCodec or SKBitmap.Decode()
3. Image pixels appear rotated/mirrored without applying EXIF orientation data

**Environment:** Mobile cameras (iOS/Android) save images with EXIF orientation metadata rather than rotating pixel data

**Related issues:** #1517

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/836#issuecomment-489923097 — Workaround implementation shared by reporter covering all 8 SKEncodedOrigin cases
- https://github.com/mono/SkiaSharp/issues/836#issuecomment-532716649 — Complete tested implementation by community member iainxt covering all origins
- https://github.com/mono/SkiaSharp/issues/836#issuecomment-853148393 — Comment by ziriax noting SKImage may apply orientation during decoding, unlike SKBitmap

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | No AutoOrient() method has been added to SKBitmap or SKImage in any released version. SKEncodedOrigin enum exists but no built-in helper to apply it. |

## Analysis

### Technical Summary

SkiaSharp lacks a built-in AutoOrient() helper method. The SKEncodedOrigin enum already exists (accessible via SKCodec.EncodedOrigin), but there is no convenience method to apply the transformation to an SKBitmap or SKImage. Multiple community workarounds using canvas transforms exist. The maintainer (mattleibow) acknowledged this is desirable. The SKImage path may already handle this transparently according to one commenter (ziriax), but SKBitmap.Decode() does not.

### Rationale

This is clearly a feature request — no existing API is broken. The reporter is asking for a new convenience method. The SKEncodedOrigin infrastructure (codec origin reading, the enum itself) is already present; what's missing is the application layer. The maintainer has endorsed the concept. Community workarounds exist but are verbose and repetitive.

### Key Signals

- "it would be nice to have something like `AutoOrient()` which would correctly rotate the image based on its EXIF data" — **issue body** (Feature request for a new convenience API, not a bug report.)
- "Definitely something to have, thanks for the work." — **comment by mattleibow (maintainer)** (Maintainer has acknowledged this is in-scope and desirable, raising priority.)
- "SKImage applies the orientation during decoding. SKBitmap doesn't. So if you need to auto-orient your images, just use SKImage?" — **comment by ziriax** (Possible partial workaround — SKImage.FromEncodedData() may already apply EXIF orientation, but this needs verification.)
- "it would be helpful to store SKEncodedOrigin in SKBitmap" — **comment by craigwi** (Related design request — the community also wants orientation metadata preserved on SKBitmap.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaApi.generated.cs` | 20783-20800 | direct | SKEncodedOrigin enum is present with all 8 EXIF orientation values (TopLeft through LeftBottom). The infrastructure for representing orientation exists. |
| `binding/SkiaSharp/SKCodec.cs` | 36-37 | direct | SKCodec.EncodedOrigin property exposes orientation from the codec — reporters can read it. But no API exists to apply the orientation transform to pixels. |
| `binding/SkiaSharp/SKBitmap.cs` | — | direct | No AutoOrient(), Rotate(), or any orientation-application method found. SKBitmap has no way to apply EXIF orientation automatically. |
| `binding/SkiaSharp/Definitions.cs` | 216-270 | related | SKCodecOptions struct has no auto-orient flag — orientation application is entirely the caller's responsibility. |

### Workarounds

- Use SKImage.FromEncodedData() instead of SKBitmap.Decode() — according to community reports, SKImage may apply EXIF orientation automatically during decoding (unverified, needs testing on specific device).
- Read SKCodec.EncodedOrigin and manually apply the appropriate canvas transform using SKCanvas.RotateDegrees() / Scale() — complete implementations shared in issue comments (see iainxt's comment #issuecomment-532716649).
- Use a library like ImageSharp which has AutoOrient() built-in, then convert result to SKBitmap if SkiaSharp processing is needed downstream.

### Next Questions

- Does SKImage.FromEncodedData() actually apply EXIF orientation on all platforms, or only on some? Needs a test case.
- Should the method be on SKBitmap, SKImage, a new utility class, or an extension method?
- Should orientation application be opt-in (explicit call) or opt-out (flag on decode)?
- Should SKBitmap store the original SKEncodedOrigin as a property after decoding?

### Resolution Proposals

**Hypothesis:** A static utility method (e.g., SKBitmap.DecodeBounds with orientation flag, or an extension/helper method) that reads SKCodec.EncodedOrigin and applies the appropriate affine transform would satisfy this request with minimal API surface.

1. **Use SKImage.FromEncodedData() as workaround** — workaround, confidence 0.60 (60%), cost/xs, validated=untested
   - Per community reports, SKImage may already apply EXIF orientation during decoding, unlike SKBitmap. Try using SKImage.FromEncodedData(stream) and see if the resulting image is correctly oriented.
2. **Manual orientation via SKCodec + SKCanvas transforms** — workaround, confidence 0.90 (90%), cost/s, validated=untested
   - Read SKCodec.EncodedOrigin, then apply appropriate rotation/flip transforms via SKCanvas. A complete, tested implementation covering all 8 origins was posted in the issue by community members.

```csharp
using var codec = SKCodec.Create(stream);
var origin = codec.EncodedOrigin;
using var original = SKBitmap.Decode(codec);
// Then apply rotation/flip based on origin using SKCanvas transforms
```
3. **Add SKBitmap.AutoOrient() or utility extension** — fix, confidence 0.85 (85%), cost/m, validated=untested
   - Add a built-in helper method (e.g., on SKBitmap or a static utility class) that takes an SKBitmap and SKEncodedOrigin and returns a correctly-oriented SKBitmap. This is the proper fix.

**Recommended proposal:** Manual orientation via SKCodec + SKCanvas transforms

**Why:** Immediate workaround that is proven to work (community tested). The proper fix (adding AutoOrient() to the API) is tracked by keeping this issue open.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.88 (88%) |
| Reason | Valid feature request acknowledged by maintainer, with clear use case and community demand. No built-in API exists to satisfy this request. Multiple workarounds exist so not blocking, but the feature is genuinely useful and in scope. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Confirm existing label is correct and add area label | labels=type/feature-request, area/SkiaSharp |
| add-comment | medium | 0.85 (85%) | Share existing workarounds and note SKImage behavior | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the request! An `AutoOrient()` convenience method is something we'd like to add.

In the meantime, here are some workarounds:

**Option 1 — Try `SKImage` instead of `SKBitmap`:**
According to some community reports, `SKImage.FromEncodedData()` may apply EXIF orientation automatically during decoding (unlike `SKBitmap.Decode()`). Worth testing on your platform:

```csharp
using var image = SKImage.FromEncodedData(stream);
```

**Option 2 — Manual orientation using `SKCodec.EncodedOrigin`:**
You can read the orientation via `SKCodec` and apply the appropriate transform yourself. See the complete implementations shared in the comments above (by [@iainxt](https://github.com/mono/SkiaSharp/issues/836#issuecomment-532716649)) which cover all 8 `SKEncodedOrigin` values.

The `SKEncodedOrigin` enum and `SKCodec.EncodedOrigin` property are already available in SkiaSharp to support this pattern.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 836,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T17:55:40Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "Feature request for an AutoOrient() convenience method that applies EXIF orientation data to correctly rotate/flip images decoded from mobile camera uploads.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    }
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Take a photo on a mobile device (typically saved in landscape regardless of device orientation)",
        "Decode the image using SKCodec or SKBitmap.Decode()",
        "Image pixels appear rotated/mirrored without applying EXIF orientation data"
      ],
      "environmentDetails": "Mobile cameras (iOS/Android) save images with EXIF orientation metadata rather than rotating pixel data",
      "relatedIssues": [
        1517
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/836#issuecomment-489923097",
          "description": "Workaround implementation shared by reporter covering all 8 SKEncodedOrigin cases"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/836#issuecomment-532716649",
          "description": "Complete tested implementation by community member iainxt covering all origins"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/836#issuecomment-853148393",
          "description": "Comment by ziriax noting SKImage may apply orientation during decoding, unlike SKBitmap"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "No AutoOrient() method has been added to SKBitmap or SKImage in any released version. SKEncodedOrigin enum exists but no built-in helper to apply it."
    }
  },
  "analysis": {
    "summary": "SkiaSharp lacks a built-in AutoOrient() helper method. The SKEncodedOrigin enum already exists (accessible via SKCodec.EncodedOrigin), but there is no convenience method to apply the transformation to an SKBitmap or SKImage. Multiple community workarounds using canvas transforms exist. The maintainer (mattleibow) acknowledged this is desirable. The SKImage path may already handle this transparently according to one commenter (ziriax), but SKBitmap.Decode() does not.",
    "rationale": "This is clearly a feature request — no existing API is broken. The reporter is asking for a new convenience method. The SKEncodedOrigin infrastructure (codec origin reading, the enum itself) is already present; what's missing is the application layer. The maintainer has endorsed the concept. Community workarounds exist but are verbose and repetitive.",
    "keySignals": [
      {
        "text": "it would be nice to have something like `AutoOrient()` which would correctly rotate the image based on its EXIF data",
        "source": "issue body",
        "interpretation": "Feature request for a new convenience API, not a bug report."
      },
      {
        "text": "Definitely something to have, thanks for the work.",
        "source": "comment by mattleibow (maintainer)",
        "interpretation": "Maintainer has acknowledged this is in-scope and desirable, raising priority."
      },
      {
        "text": "SKImage applies the orientation during decoding. SKBitmap doesn't. So if you need to auto-orient your images, just use SKImage?",
        "source": "comment by ziriax",
        "interpretation": "Possible partial workaround — SKImage.FromEncodedData() may already apply EXIF orientation, but this needs verification."
      },
      {
        "text": "it would be helpful to store SKEncodedOrigin in SKBitmap",
        "source": "comment by craigwi",
        "interpretation": "Related design request — the community also wants orientation metadata preserved on SKBitmap."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "20783-20800",
        "finding": "SKEncodedOrigin enum is present with all 8 EXIF orientation values (TopLeft through LeftBottom). The infrastructure for representing orientation exists.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "36-37",
        "finding": "SKCodec.EncodedOrigin property exposes orientation from the codec — reporters can read it. But no API exists to apply the orientation transform to pixels.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "finding": "No AutoOrient(), Rotate(), or any orientation-application method found. SKBitmap has no way to apply EXIF orientation automatically.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "216-270",
        "finding": "SKCodecOptions struct has no auto-orient flag — orientation application is entirely the caller's responsibility.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use SKImage.FromEncodedData() instead of SKBitmap.Decode() — according to community reports, SKImage may apply EXIF orientation automatically during decoding (unverified, needs testing on specific device).",
      "Read SKCodec.EncodedOrigin and manually apply the appropriate canvas transform using SKCanvas.RotateDegrees() / Scale() — complete implementations shared in issue comments (see iainxt's comment #issuecomment-532716649).",
      "Use a library like ImageSharp which has AutoOrient() built-in, then convert result to SKBitmap if SkiaSharp processing is needed downstream."
    ],
    "nextQuestions": [
      "Does SKImage.FromEncodedData() actually apply EXIF orientation on all platforms, or only on some? Needs a test case.",
      "Should the method be on SKBitmap, SKImage, a new utility class, or an extension method?",
      "Should orientation application be opt-in (explicit call) or opt-out (flag on decode)?",
      "Should SKBitmap store the original SKEncodedOrigin as a property after decoding?"
    ],
    "resolution": {
      "hypothesis": "A static utility method (e.g., SKBitmap.DecodeBounds with orientation flag, or an extension/helper method) that reads SKCodec.EncodedOrigin and applies the appropriate affine transform would satisfy this request with minimal API surface.",
      "proposals": [
        {
          "title": "Use SKImage.FromEncodedData() as workaround",
          "description": "Per community reports, SKImage may already apply EXIF orientation during decoding, unlike SKBitmap. Try using SKImage.FromEncodedData(stream) and see if the resulting image is correctly oriented.",
          "category": "workaround",
          "confidence": 0.6,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Manual orientation via SKCodec + SKCanvas transforms",
          "description": "Read SKCodec.EncodedOrigin, then apply appropriate rotation/flip transforms via SKCanvas. A complete, tested implementation covering all 8 origins was posted in the issue by community members.",
          "category": "workaround",
          "codeSnippet": "using var codec = SKCodec.Create(stream);\nvar origin = codec.EncodedOrigin;\nusing var original = SKBitmap.Decode(codec);\n// Then apply rotation/flip based on origin using SKCanvas transforms",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Add SKBitmap.AutoOrient() or utility extension",
          "description": "Add a built-in helper method (e.g., on SKBitmap or a static utility class) that takes an SKBitmap and SKEncodedOrigin and returns a correctly-oriented SKBitmap. This is the proper fix.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Manual orientation via SKCodec + SKCanvas transforms",
      "recommendedReason": "Immediate workaround that is proven to work (community tested). The proper fix (adding AutoOrient() to the API) is tracked by keeping this issue open."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.88,
      "reason": "Valid feature request acknowledged by maintainer, with clear use case and community demand. No built-in API exists to satisfy this request. Multiple workarounds exist so not blocking, but the feature is genuinely useful and in scope.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Confirm existing label is correct and add area label",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Share existing workarounds and note SKImage behavior",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the request! An `AutoOrient()` convenience method is something we'd like to add.\n\nIn the meantime, here are some workarounds:\n\n**Option 1 — Try `SKImage` instead of `SKBitmap`:**\nAccording to some community reports, `SKImage.FromEncodedData()` may apply EXIF orientation automatically during decoding (unlike `SKBitmap.Decode()`). Worth testing on your platform:\n\n```csharp\nusing var image = SKImage.FromEncodedData(stream);\n```\n\n**Option 2 — Manual orientation using `SKCodec.EncodedOrigin`:**\nYou can read the orientation via `SKCodec` and apply the appropriate transform yourself. See the complete implementations shared in the comments above (by [@iainxt](https://github.com/mono/SkiaSharp/issues/836#issuecomment-532716649)) which cover all 8 `SKEncodedOrigin` values.\n\nThe `SKEncodedOrigin` enum and `SKCodec.EncodedOrigin` property are already available in SkiaSharp to support this pattern."
      }
    ]
  }
}
```

</details>
