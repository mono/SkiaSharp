# Issue Triage Report — #2362

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T23:19:14Z |
| Type | type/question (0.97 (97%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** Reporter asks whether there is a documented mapping between SkiaSharp's SKColorType enum and System.Drawing's PixelFormat enum.

**Analysis:** The issue title ('aspect ratio with System.Drawing') is misleading; the body asks for a SKColorType-to-System.Drawing.PixelFormat mapping. SkiaSharp has no built-in conversion helper between these two enums. Common mappings can be constructed manually (e.g., Bgra8888 ↔ Format32bppArgb). This is a usage question with a known answer.

**Recommendations:** **close-as-not-a-bug** — This is a usage question. SkiaSharp does not provide an automatic mapping but one can be constructed manually. Answering and closing is appropriate.

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

**Environment:** No version, framework, or platform specified by reporter

## Analysis

### Technical Summary

The issue title ('aspect ratio with System.Drawing') is misleading; the body asks for a SKColorType-to-System.Drawing.PixelFormat mapping. SkiaSharp has no built-in conversion helper between these two enums. Common mappings can be constructed manually (e.g., Bgra8888 ↔ Format32bppArgb). This is a usage question with a known answer.

### Rationale

The body explicitly asks for a format correlation table. No broken behavior is described, and no reproduction is provided. SkiaSharp does not reference System.Drawing anywhere in its source. A mapping can be provided as a community answer.

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/Definitions.cs` | 36-66 | direct | SKColorType enum lists 27 color formats (Unknown through R16Unorm). No reference to System.Drawing.PixelFormat anywhere in this file. |
| `binding/SkiaSharp/SKImageInfo.cs` | 40-144 | related | SKImageInfo struct uses SKColorType for pixel layout; no conversion utility to System.Drawing types exists in the codebase. |

### Workarounds

- Manually map the most common formats: SKColorType.Bgra8888 ↔ PixelFormat.Format32bppArgb, SKColorType.Rgb565 ↔ PixelFormat.Format16bppRgb565, SKColorType.Gray8 ↔ PixelFormat.Format8bppIndexed.
- Use SKBitmap.GetPixels() / Bitmap.LockBits() and copy the pixel buffer directly when both images use compatible layouts.

### Resolution Proposals

**Hypothesis:** Reporter wants to interop between SkiaSharp and System.Drawing by converting between their respective color format enums.

1. **Manual mapping table** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - Provide a community-supplied switch/dictionary that maps common SKColorType values to System.Drawing.PixelFormat values.

**Recommended proposal:** Manual mapping table

**Why:** SkiaSharp has no built-in mapping; a manual table is the only available approach and covers all common use cases.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | This is a usage question. SkiaSharp does not provide an automatic mapping but one can be constructed manually. Answering and closing is appropriate. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply question and SkiaSharp area labels | labels=type/question, area/SkiaSharp |
| add-comment | high | 0.88 (88%) | Answer with format mapping guidance and close | — |
| close-issue | medium | 0.88 (88%) | Close as answered usage question | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Hi @dmitrysavitski! SkiaSharp does not have a built-in mapping between `SKColorType` and `System.Drawing.PixelFormat`, but the two enums can be correlated manually.

Here are the most common equivalents:

| `SKColorType` | `System.Drawing.PixelFormat` |
|---|---|
| `Bgra8888` | `Format32bppArgb` |
| `Rgba8888` | `Format32bppArgb` (byte-order differs — RGBA vs BGRA) |
| `Rgb888x` | `Format32bppRgb` |
| `Rgb565` | `Format16bppRgb565` |
| `Argb4444` | `Format16bppArgb1555` (approximate) |
| `Gray8` | `Format8bppIndexed` (grayscale palette) |
| `Alpha8` | `Format8bppIndexed` |

Note that `System.Drawing.Bitmap` is Windows-only while SkiaSharp is cross-platform, so mixing the two is typically platform-specific interop.

Hope this helps! Feel free to ask if you need more details.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2362,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T23:19:14Z"
  },
  "summary": "Reporter asks whether there is a documented mapping between SkiaSharp's SKColorType enum and System.Drawing's PixelFormat enum.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    }
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "No version, framework, or platform specified by reporter"
    }
  },
  "analysis": {
    "summary": "The issue title ('aspect ratio with System.Drawing') is misleading; the body asks for a SKColorType-to-System.Drawing.PixelFormat mapping. SkiaSharp has no built-in conversion helper between these two enums. Common mappings can be constructed manually (e.g., Bgra8888 ↔ Format32bppArgb). This is a usage question with a known answer.",
    "rationale": "The body explicitly asks for a format correlation table. No broken behavior is described, and no reproduction is provided. SkiaSharp does not reference System.Drawing anywhere in its source. A mapping can be provided as a community answer.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "36-66",
        "finding": "SKColorType enum lists 27 color formats (Unknown through R16Unorm). No reference to System.Drawing.PixelFormat anywhere in this file.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImageInfo.cs",
        "lines": "40-144",
        "finding": "SKImageInfo struct uses SKColorType for pixel layout; no conversion utility to System.Drawing types exists in the codebase.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Manually map the most common formats: SKColorType.Bgra8888 ↔ PixelFormat.Format32bppArgb, SKColorType.Rgb565 ↔ PixelFormat.Format16bppRgb565, SKColorType.Gray8 ↔ PixelFormat.Format8bppIndexed.",
      "Use SKBitmap.GetPixels() / Bitmap.LockBits() and copy the pixel buffer directly when both images use compatible layouts."
    ],
    "resolution": {
      "hypothesis": "Reporter wants to interop between SkiaSharp and System.Drawing by converting between their respective color format enums.",
      "proposals": [
        {
          "title": "Manual mapping table",
          "description": "Provide a community-supplied switch/dictionary that maps common SKColorType values to System.Drawing.PixelFormat values.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Manual mapping table",
      "recommendedReason": "SkiaSharp has no built-in mapping; a manual table is the only available approach and covers all common use cases."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "This is a usage question. SkiaSharp does not provide an automatic mapping but one can be constructed manually. Answering and closing is appropriate.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and SkiaSharp area labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Answer with format mapping guidance and close",
        "risk": "high",
        "confidence": 0.88,
        "comment": "Hi @dmitrysavitski! SkiaSharp does not have a built-in mapping between `SKColorType` and `System.Drawing.PixelFormat`, but the two enums can be correlated manually.\n\nHere are the most common equivalents:\n\n| `SKColorType` | `System.Drawing.PixelFormat` |\n|---|---|\n| `Bgra8888` | `Format32bppArgb` |\n| `Rgba8888` | `Format32bppArgb` (byte-order differs — RGBA vs BGRA) |\n| `Rgb888x` | `Format32bppRgb` |\n| `Rgb565` | `Format16bppRgb565` |\n| `Argb4444` | `Format16bppArgb1555` (approximate) |\n| `Gray8` | `Format8bppIndexed` (grayscale palette) |\n| `Alpha8` | `Format8bppIndexed` |\n\nNote that `System.Drawing.Bitmap` is Windows-only while SkiaSharp is cross-platform, so mixing the two is typically platform-specific interop.\n\nHope this helps! Feel free to ask if you need more details."
      },
      {
        "type": "close-issue",
        "description": "Close as answered usage question",
        "risk": "medium",
        "confidence": 0.88,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
