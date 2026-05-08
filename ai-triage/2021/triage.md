# Issue Triage Report — #2021

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T10:28:28Z |
| Type | type/question (0.92 (92%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-not-a-bug (0.90 (90%)) |

**Issue Summary:** Reporter observes that SKPaint.MeasureText returns 4.5 for the Narrow No-Break Space character (U+202F) with FrutigerNext LT LightCn at size 9, while System.Drawing returns 1.1953125, and asks how to achieve matching results.

**Analysis:** The measurement difference is by design: Skia's MeasureText measures raw glyph advances without text shaping, while System.Drawing uses GDI/Uniscribe which applies Unicode text shaping. For special Unicode characters like U+202F (Narrow No-Break Space), the shaping process can substitute a different glyph or apply spacing rules that change the measured width significantly. A contributor (Gillibald) confirmed this is not a bug and suggested RichTextKit for cases requiring text shaping.

**Recommendations:** **close-as-not-a-bug** — The behavior difference between SkiaSharp and System.Drawing for special Unicode characters is by design. Skia does not perform text shaping; System.Drawing/GDI does. A contributor already confirmed this. The workaround is to use a text shaping library like RichTextKit or SkiaSharp.HarfBuzz.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Call SKPaint.MeasureText with the Narrow No-Break Space character (char)8239 and font FrutigerNext LT LightCn at size 9
2. Compare the result (4.5) with System.Drawing measurement (1.1953125)

**Environment:** SkiaSharp 2.88.0-preview.256, .NET Core 3.1, Visual Studio 2019, ASP.NET Core

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1102 — Related: wrong Width value while measuring Arabic Text — same root cause (no text shaping in Skia vs GDI)

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.0-preview.256 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Skia has never performed text shaping in SKPaint/SKFont.MeasureText; this architectural difference is unchanged in all subsequent versions. |

## Analysis

### Technical Summary

The measurement difference is by design: Skia's MeasureText measures raw glyph advances without text shaping, while System.Drawing uses GDI/Uniscribe which applies Unicode text shaping. For special Unicode characters like U+202F (Narrow No-Break Space), the shaping process can substitute a different glyph or apply spacing rules that change the measured width significantly. A contributor (Gillibald) confirmed this is not a bug and suggested RichTextKit for cases requiring text shaping.

### Rationale

The reporter filed this as a bug, but a contributor confirmed it is not. The root cause is an architectural difference between Skia (no text shaping) and System.Drawing/GDI (Uniscribe shaping). This is the same pattern as issue #1102. The issue is a question about how to achieve System.Drawing-equivalent measurements in SkiaSharp. The answer is to use a text shaping library (SkiaSharp.HarfBuzz or RichTextKit).

### Key Signals

- "Skia isn't performing any text shaping therefore you don't get similar results. No issue here." — **comment by contributor Gillibald** (Confirmed by a contributor that this is expected behavior, not a bug. Root cause is absence of text shaping in Skia.)
- "You can't. System.Drawing uses GDI under the hood. You can only get similar results. I suggest using RichTextKit on top of SkiaSharp." — **comment by contributor Gillibald** (Canonical workaround: use RichTextKit (or SkiaSharp.HarfBuzz) for text shaping to get GDI-comparable measurements.)
- "This issue occurs specific to this font only." — **issue body** (The reporter narrowed it to a specific font/character combination, but the root cause is systemic: any special Unicode character that requires shaping will differ.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKFont.cs` | 315-323 | direct | MeasureText delegates directly to sk_font_measure_text_no_return — no text shaping is performed; glyph advances are summed as-is from the font metrics |
| `binding/SkiaSharp/SKPaint.cs` | 284-330 | related | SKPaint.MeasureText overloads are all marked [Obsolete] and delegate to SKFont.MeasureText, confirming no shaping layer exists in the measurement pipeline |

### Workarounds

- Use SkiaSharp.HarfBuzz (HarfBuzz text shaping on top of SkiaSharp) to shape text before measuring
- Use RichTextKit (https://github.com/toptensoftware/RichTextKit) — a third-party library built on SkiaSharp that handles Unicode text shaping
- Accept the difference: SkiaSharp and GDI are different rendering engines with different text models; exact pixel-parity is not achievable

### Resolution Proposals

**Hypothesis:** Reporter wants GDI-equivalent text measurement for Unicode special characters. Skia does not do text shaping; this is a known architectural limitation.

1. **Use SkiaSharp.HarfBuzz for shaped text measurement** — workaround, confidence 0.85 (85%), cost/s, validated=untested
   - SkiaSharp.HarfBuzz provides HarfBuzz text shaping integrated with SkiaSharp. Shape the text first, then measure the shaped glyphs to get accurate Unicode-aware widths.
2. **Use RichTextKit library** — alternative, confidence 0.90 (90%), cost/s, validated=untested
   - RichTextKit (https://github.com/toptensoftware/RichTextKit) is a third-party library that performs text shaping on top of SkiaSharp, providing Unicode-aware layout and measurement.

**Recommended proposal:** Use RichTextKit library

**Why:** RichTextKit is already suggested by a contributor and is specifically designed to provide GDI-like text shaping and measurement on top of SkiaSharp.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.90 (90%) |
| Reason | The behavior difference between SkiaSharp and System.Drawing for special Unicode characters is by design. Skia does not perform text shaping; System.Drawing/GDI does. A contributor already confirmed this. The workaround is to use a text shaping library like RichTextKit or SkiaSharp.HarfBuzz. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply question, area/SkiaSharp, and tenet/compatibility labels | labels=type/question, area/SkiaSharp, tenet/compatibility |
| add-comment | medium | 0.90 (90%) | Explain text shaping difference and provide workaround | — |
| close-issue | medium | 0.88 (88%) | Close as not a bug — behavior difference is by design (no text shaping in Skia vs GDI) | stateReason=not_planned |
| link-related | low | 0.95 (95%) | Link to related issue #1102 (same text shaping vs GDI measurement pattern) | linkedIssue=#1102 |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report.

The difference in measurement is expected and by design. SkiaSharp (Skia) does not perform Unicode **text shaping** — it measures raw glyph advances directly from the font. System.Drawing, on the other hand, uses **GDI/Uniscribe** under the hood, which applies Unicode text shaping rules. For special Unicode characters like U+202F (Narrow No-Break Space), the shaping process can substitute glyphs or apply different spacing, leading to significantly different measured widths.

This is the same root cause as #1102.

**Workaround options:**

1. **[SkiaSharp.HarfBuzz](https://www.nuget.org/packages/SkiaSharp.HarfBuzz)** — Adds HarfBuzz text shaping on top of SkiaSharp. Shape your text first, then measure the resulting glyphs.
2. **[RichTextKit](https://github.com/toptensoftware/RichTextKit)** — A third-party library built on SkiaSharp that provides Unicode-aware text layout and measurement, giving results closer to GDI.

Exact parity with System.Drawing/GDI is not achievable because they are fundamentally different rendering engines. Closing as not a bug.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2021,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T10:28:28Z"
  },
  "summary": "Reporter observes that SKPaint.MeasureText returns 4.5 for the Narrow No-Break Space character (U+202F) with FrutigerNext LT LightCn at size 9, while System.Drawing returns 1.1953125, and asks how to achieve matching results.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Call SKPaint.MeasureText with the Narrow No-Break Space character (char)8239 and font FrutigerNext LT LightCn at size 9",
        "Compare the result (4.5) with System.Drawing measurement (1.1953125)"
      ],
      "environmentDetails": "SkiaSharp 2.88.0-preview.256, .NET Core 3.1, Visual Studio 2019, ASP.NET Core",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1102",
          "description": "Related: wrong Width value while measuring Arabic Text — same root cause (no text shaping in Skia vs GDI)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.0-preview.256"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Skia has never performed text shaping in SKPaint/SKFont.MeasureText; this architectural difference is unchanged in all subsequent versions."
    }
  },
  "analysis": {
    "summary": "The measurement difference is by design: Skia's MeasureText measures raw glyph advances without text shaping, while System.Drawing uses GDI/Uniscribe which applies Unicode text shaping. For special Unicode characters like U+202F (Narrow No-Break Space), the shaping process can substitute a different glyph or apply spacing rules that change the measured width significantly. A contributor (Gillibald) confirmed this is not a bug and suggested RichTextKit for cases requiring text shaping.",
    "rationale": "The reporter filed this as a bug, but a contributor confirmed it is not. The root cause is an architectural difference between Skia (no text shaping) and System.Drawing/GDI (Uniscribe shaping). This is the same pattern as issue #1102. The issue is a question about how to achieve System.Drawing-equivalent measurements in SkiaSharp. The answer is to use a text shaping library (SkiaSharp.HarfBuzz or RichTextKit).",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKFont.cs",
        "lines": "315-323",
        "finding": "MeasureText delegates directly to sk_font_measure_text_no_return — no text shaping is performed; glyph advances are summed as-is from the font metrics",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "lines": "284-330",
        "finding": "SKPaint.MeasureText overloads are all marked [Obsolete] and delegate to SKFont.MeasureText, confirming no shaping layer exists in the measurement pipeline",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Skia isn't performing any text shaping therefore you don't get similar results. No issue here.",
        "source": "comment by contributor Gillibald",
        "interpretation": "Confirmed by a contributor that this is expected behavior, not a bug. Root cause is absence of text shaping in Skia."
      },
      {
        "text": "You can't. System.Drawing uses GDI under the hood. You can only get similar results. I suggest using RichTextKit on top of SkiaSharp.",
        "source": "comment by contributor Gillibald",
        "interpretation": "Canonical workaround: use RichTextKit (or SkiaSharp.HarfBuzz) for text shaping to get GDI-comparable measurements."
      },
      {
        "text": "This issue occurs specific to this font only.",
        "source": "issue body",
        "interpretation": "The reporter narrowed it to a specific font/character combination, but the root cause is systemic: any special Unicode character that requires shaping will differ."
      }
    ],
    "workarounds": [
      "Use SkiaSharp.HarfBuzz (HarfBuzz text shaping on top of SkiaSharp) to shape text before measuring",
      "Use RichTextKit (https://github.com/toptensoftware/RichTextKit) — a third-party library built on SkiaSharp that handles Unicode text shaping",
      "Accept the difference: SkiaSharp and GDI are different rendering engines with different text models; exact pixel-parity is not achievable"
    ],
    "resolution": {
      "hypothesis": "Reporter wants GDI-equivalent text measurement for Unicode special characters. Skia does not do text shaping; this is a known architectural limitation.",
      "proposals": [
        {
          "title": "Use SkiaSharp.HarfBuzz for shaped text measurement",
          "description": "SkiaSharp.HarfBuzz provides HarfBuzz text shaping integrated with SkiaSharp. Shape the text first, then measure the shaped glyphs to get accurate Unicode-aware widths.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Use RichTextKit library",
          "description": "RichTextKit (https://github.com/toptensoftware/RichTextKit) is a third-party library that performs text shaping on top of SkiaSharp, providing Unicode-aware layout and measurement.",
          "category": "alternative",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use RichTextKit library",
      "recommendedReason": "RichTextKit is already suggested by a contributor and is specifically designed to provide GDI-like text shaping and measurement on top of SkiaSharp."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.9,
      "reason": "The behavior difference between SkiaSharp and System.Drawing for special Unicode characters is by design. Skia does not perform text shaping; System.Drawing/GDI does. A contributor already confirmed this. The workaround is to use a text shaping library like RichTextKit or SkiaSharp.HarfBuzz.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question, area/SkiaSharp, and tenet/compatibility labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain text shaping difference and provide workaround",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thank you for the detailed report.\n\nThe difference in measurement is expected and by design. SkiaSharp (Skia) does not perform Unicode **text shaping** — it measures raw glyph advances directly from the font. System.Drawing, on the other hand, uses **GDI/Uniscribe** under the hood, which applies Unicode text shaping rules. For special Unicode characters like U+202F (Narrow No-Break Space), the shaping process can substitute glyphs or apply different spacing, leading to significantly different measured widths.\n\nThis is the same root cause as #1102.\n\n**Workaround options:**\n\n1. **[SkiaSharp.HarfBuzz](https://www.nuget.org/packages/SkiaSharp.HarfBuzz)** — Adds HarfBuzz text shaping on top of SkiaSharp. Shape your text first, then measure the resulting glyphs.\n2. **[RichTextKit](https://github.com/toptensoftware/RichTextKit)** — A third-party library built on SkiaSharp that provides Unicode-aware text layout and measurement, giving results closer to GDI.\n\nExact parity with System.Drawing/GDI is not achievable because they are fundamentally different rendering engines. Closing as not a bug."
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — behavior difference is by design (no text shaping in Skia vs GDI)",
        "risk": "medium",
        "confidence": 0.88,
        "stateReason": "not_planned"
      },
      {
        "type": "link-related",
        "description": "Link to related issue #1102 (same text shaping vs GDI measurement pattern)",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 1102
      }
    ]
  }
}
```

</details>
