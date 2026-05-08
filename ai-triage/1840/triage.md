# Issue Triage Report — #1840

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T03:22:08Z |
| Type | type/question (0.95 (95%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.80 (80%)) |

**Issue Summary:** User asks whether SkiaSharp provides an API to quantize (reduce) a bitmap's color palette, with no existing API or workaround provided in the issue.

**Analysis:** SkiaSharp does not expose a color quantization API. Skia itself removed indexed bitmap (Index8 ColorType) support, and no dedicated palette quantization function exists in either the C++ Skia layer or the SkiaSharp C# bindings. Workarounds require a third-party .NET library (e.g., ColorThief, nQuant) to perform quantization, then draw results back onto an SKBitmap.

**Recommendations:** **close-as-not-a-bug** — This is a how-to question. SkiaSharp does not have a built-in quantization API; the answer is to use a third-party library. The issue can be closed with a workaround comment. Related feature request #2931 tracks native support.

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
- https://github.com/mono/SkiaSharp/issues/2931 — Related feature request #2931 for color mapping/quantization API added in 2024

## Analysis

### Technical Summary

SkiaSharp does not expose a color quantization API. Skia itself removed indexed bitmap (Index8 ColorType) support, and no dedicated palette quantization function exists in either the C++ Skia layer or the SkiaSharp C# bindings. Workarounds require a third-party .NET library (e.g., ColorThief, nQuant) to perform quantization, then draw results back onto an SKBitmap.

### Rationale

This is a how-to question about a missing feature. No quantize API is exposed in SkiaSharp. The SKColorType enum no longer contains Index8. The referenced constant 'UnsupportedColorTypeMessage' for Index8 in SKBitmap.cs is dead code — that color type was dropped upstream. Related feature request #2931 confirms the feature is missing and has not been implemented.

### Key Signals

- "Is there a way to quantize color palette using SkiaSharp?" — **issue body** (Asking about a specific API feature — palette quantization.)
- "would the maintainer or any contributor happen to know if this feature is available or planned?" — **comment** (Confirms no answer has been provided; feature availability is unknown to community.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/Definitions.cs` | 36-66 | direct | SKColorType enum has no Index8 value — Skia dropped indexed bitmap support. No quantize API exists in the enum or associated methods. |
| `binding/SkiaSharp/SKBitmap.cs` | 16 | related | Dead constant 'UnsupportedColorTypeMessage' references Index8 ColorType, which no longer exists in SKColorType enum — confirms indexed color support was removed. |

### Workarounds

- Use a third-party .NET color quantization library such as ColorThief.NET or nQuant to reduce colors from an SKBitmap's pixel data (obtained via GetPixels), then draw the quantized pixels back.
- Manually iterate pixels and apply a nearest-neighbor lookup in a custom palette using SKColor comparisons.

### Next Questions

- Does the reporter want palette generation (find N representative colors) or actual palette reduction (re-encode image with limited colors)?
- Is there a specific use case (GIF export, memory reduction, artistic effect)?
- Should this be tracked as a feature request for a future SkiaSharp release?

### Resolution Proposals

**Hypothesis:** SkiaSharp has no built-in color quantization API; the underlying Skia library removed indexed color support. Third-party .NET libraries can fill the gap.

1. **Use third-party quantization library** — workaround, confidence 0.85 (85%), cost/s, validated=untested
   - Obtain raw pixel data from SKBitmap via GetPixels or ReadPixels, pass to ColorThief.NET or nQuant, then write quantized pixels back to a new SKBitmap.
2. **Convert issue to feature request** — alternative, confidence 0.80 (80%), cost/xs, validated=untested
   - If the reporter wants this natively, ask them to reopen as a feature request or link to #2931 which requests the same capability.

**Recommended proposal:** Use third-party quantization library

**Why:** Provides an actionable workaround now without requiring SkiaSharp changes. The feature may eventually be tracked via #2931.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.80 (80%) |
| Reason | This is a how-to question. SkiaSharp does not have a built-in quantization API; the answer is to use a third-party library. The issue can be closed with a workaround comment. Related feature request #2931 tracks native support. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply question and core area labels | labels=type/question, area/SkiaSharp |
| add-comment | high | 0.80 (80%) | Answer the question with workaround and link to related feature request | — |
| close-issue | medium | 0.75 (75%) | Close as answered — no SkiaSharp API exists, workaround provided | stateReason=completed |
| link-related | low | 0.90 (90%) | Link to related feature request #2931 | linkedIssue=#2931 |

**Comment draft for `add-comment`:**

```markdown
SkiaSharp does not currently provide a built-in color quantization / palette reduction API. Skia removed support for indexed (`Index8`) bitmaps some time ago, so there is no native path for this.

**Workaround:** You can perform quantization outside of SkiaSharp using a .NET library such as [ColorThief.NET](https://github.com/KSemenenko/ColorThief) or [nQuant](https://github.com/mcychan/nQuant.cs), then draw the resulting colors back onto an `SKBitmap`:

1. Read pixel data from your `SKBitmap` via `bitmap.Pixels` or `bitmap.GetPixels()`.
2. Pass the pixel array to the quantization library to obtain a reduced palette.
3. Create a new `SKBitmap` and write the palette-mapped pixels back.

If you'd like to see native quantization support in SkiaSharp, you may want to add a 👍 to issue #2931 which requests a similar color mapping API.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1840,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T03:22:08Z"
  },
  "summary": "User asks whether SkiaSharp provides an API to quantize (reduce) a bitmap's color palette, with no existing API or workaround provided in the issue.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    }
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2931",
          "description": "Related feature request #2931 for color mapping/quantization API added in 2024"
        }
      ]
    }
  },
  "analysis": {
    "summary": "SkiaSharp does not expose a color quantization API. Skia itself removed indexed bitmap (Index8 ColorType) support, and no dedicated palette quantization function exists in either the C++ Skia layer or the SkiaSharp C# bindings. Workarounds require a third-party .NET library (e.g., ColorThief, nQuant) to perform quantization, then draw results back onto an SKBitmap.",
    "rationale": "This is a how-to question about a missing feature. No quantize API is exposed in SkiaSharp. The SKColorType enum no longer contains Index8. The referenced constant 'UnsupportedColorTypeMessage' for Index8 in SKBitmap.cs is dead code — that color type was dropped upstream. Related feature request #2931 confirms the feature is missing and has not been implemented.",
    "keySignals": [
      {
        "text": "Is there a way to quantize color palette using SkiaSharp?",
        "source": "issue body",
        "interpretation": "Asking about a specific API feature — palette quantization."
      },
      {
        "text": "would the maintainer or any contributor happen to know if this feature is available or planned?",
        "source": "comment",
        "interpretation": "Confirms no answer has been provided; feature availability is unknown to community."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "36-66",
        "finding": "SKColorType enum has no Index8 value — Skia dropped indexed bitmap support. No quantize API exists in the enum or associated methods.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "16",
        "finding": "Dead constant 'UnsupportedColorTypeMessage' references Index8 ColorType, which no longer exists in SKColorType enum — confirms indexed color support was removed.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use a third-party .NET color quantization library such as ColorThief.NET or nQuant to reduce colors from an SKBitmap's pixel data (obtained via GetPixels), then draw the quantized pixels back.",
      "Manually iterate pixels and apply a nearest-neighbor lookup in a custom palette using SKColor comparisons."
    ],
    "nextQuestions": [
      "Does the reporter want palette generation (find N representative colors) or actual palette reduction (re-encode image with limited colors)?",
      "Is there a specific use case (GIF export, memory reduction, artistic effect)?",
      "Should this be tracked as a feature request for a future SkiaSharp release?"
    ],
    "resolution": {
      "hypothesis": "SkiaSharp has no built-in color quantization API; the underlying Skia library removed indexed color support. Third-party .NET libraries can fill the gap.",
      "proposals": [
        {
          "title": "Use third-party quantization library",
          "description": "Obtain raw pixel data from SKBitmap via GetPixels or ReadPixels, pass to ColorThief.NET or nQuant, then write quantized pixels back to a new SKBitmap.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Convert issue to feature request",
          "description": "If the reporter wants this natively, ask them to reopen as a feature request or link to #2931 which requests the same capability.",
          "category": "alternative",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use third-party quantization library",
      "recommendedReason": "Provides an actionable workaround now without requiring SkiaSharp changes. The feature may eventually be tracked via #2931."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.8,
      "reason": "This is a how-to question. SkiaSharp does not have a built-in quantization API; the answer is to use a third-party library. The issue can be closed with a workaround comment. Related feature request #2931 tracks native support.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and core area labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Answer the question with workaround and link to related feature request",
        "risk": "high",
        "confidence": 0.8,
        "comment": "SkiaSharp does not currently provide a built-in color quantization / palette reduction API. Skia removed support for indexed (`Index8`) bitmaps some time ago, so there is no native path for this.\n\n**Workaround:** You can perform quantization outside of SkiaSharp using a .NET library such as [ColorThief.NET](https://github.com/KSemenenko/ColorThief) or [nQuant](https://github.com/mcychan/nQuant.cs), then draw the resulting colors back onto an `SKBitmap`:\n\n1. Read pixel data from your `SKBitmap` via `bitmap.Pixels` or `bitmap.GetPixels()`.\n2. Pass the pixel array to the quantization library to obtain a reduced palette.\n3. Create a new `SKBitmap` and write the palette-mapped pixels back.\n\nIf you'd like to see native quantization support in SkiaSharp, you may want to add a 👍 to issue #2931 which requests a similar color mapping API."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — no SkiaSharp API exists, workaround provided",
        "risk": "medium",
        "confidence": 0.75,
        "stateReason": "completed"
      },
      {
        "type": "link-related",
        "description": "Link to related feature request #2931",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 2931
      }
    ]
  }
}
```

</details>
