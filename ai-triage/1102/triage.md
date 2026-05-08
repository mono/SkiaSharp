# Issue Triage Report — #1102

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T13:08:00Z |
| Type | type/question (0.90 (90%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | close-as-not-a-bug (0.85 (85%)) |

**Issue Summary:** Reporter asks why SKPaint.MeasureText() returns a significantly wider measurement for Arabic text compared to GDI MeasureString, and whether this is a bug or if there is a workaround using SkiaSharp alone.

**Analysis:** SKFont.MeasureText() / SKPaint.MeasureText() calls Skia's sk_font_measure_text_no_return which performs raw glyph-by-glyph measurement without text shaping. Arabic uses ligatures: multiple code points combine into fewer shaped glyphs, so the unshaped path counts each code point independently and returns a larger width. This is by-design — proper Arabic measurement requires HarfBuzz text shaping (SkiaSharp.HarfBuzz package). The GDI comparison is also imperfect because MeasureString adds extra padding by design per MSDN docs.

**Recommendations:** **close-as-not-a-bug** — SKFont.MeasureText() does not perform text shaping by design. Arabic script requires HarfBuzz for correct ligature-aware measurement. Contributor confirmed this and demonstrated the workaround. GDI comparison is not equivalent due to MeasureString's extra padding.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/question, os/Windows-Classic, area/SkiaSharp, tenet/compatibility, triage/triaged |

## Evidence

### Reproduction

1. Load a Traditional Arabic TTF font with SKTypeface.FromFile()
2. Call SKPaint.MeasureText() on Arabic string "مرحبا بكم في التزامن "
3. Compare result (95.61 pts) to GDI Graphics.MeasureString() result (~66 pts)

**Environment:** SkiaSharp 1.59.3, .NET Core 2.1, Visual Studio 2017, Windows

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1102 — Original issue with 35 comments showing extended investigation

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.59.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SKFont.MeasureText() still calls sk_font_measure_text_no_return directly without text shaping — behavior is unchanged by design. |

## Analysis

### Technical Summary

SKFont.MeasureText() / SKPaint.MeasureText() calls Skia's sk_font_measure_text_no_return which performs raw glyph-by-glyph measurement without text shaping. Arabic uses ligatures: multiple code points combine into fewer shaped glyphs, so the unshaped path counts each code point independently and returns a larger width. This is by-design — proper Arabic measurement requires HarfBuzz text shaping (SkiaSharp.HarfBuzz package). The GDI comparison is also imperfect because MeasureString adds extra padding by design per MSDN docs.

### Rationale

Classified as type/question because the reporter explicitly asks 'is this a bug?' and 'any way to achieve this in SkiaSharp 1.59.3?' — not reporting definitively broken behavior. The behavior is by-design: Skia's MeasureText never did text shaping. The contributor (Gillibald) confirmed this and demonstrated the workaround using HarfBuzz. Existing labels are correct.

### Key Signals

- "Simple answer it is not possible to solve without HarfBuzz" — **comment by Gillibald (CONTRIBUTOR)** (Confirms by-design behavior: SKPaint.MeasureText does not perform text shaping for complex scripts.)
- "please confirm me whether it's a bug" — **issue body** (Reporter is asking a question about expected behavior, not definitively reporting a defect.)
- "The MeasureString method is designed for use with individual strings and includes a small amount of extra space before and after the string" — **comment by Gillibald quoting MSDN docs** (GDI MeasureString adds extra padding — not an apples-to-apples comparison with SKFont.MeasureText.)
- "i got correct measurement by implementing code from the above picture" — **comment by Jack-Paul** (Reporter confirmed HarfBuzz-based approach eventually produced better results.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKFont.cs` | 315-323 | direct | SKFont.MeasureText() calls SkiaApi.sk_font_measure_text_no_return() directly — no text shaping is performed. Glyph widths are summed without ligature or BiDi processing. |
| `binding/SkiaSharp/SKPaint.cs` | 284-286 | direct | SKPaint.MeasureText() is marked [Obsolete] and delegates to SKFont.MeasureText() — same unshaping behavior. |
| `binding/HarfBuzzSharp/Buffer.cs` | 82-84 | related | HarfBuzzSharp.Buffer.GlyphPositions exposes shaped glyph positions (XAdvance per glyph) after Font.Shape() is called — this is the correct path for Arabic text measurement. |

### Workarounds

- Use SkiaSharp.HarfBuzz package: create HarfBuzzSharp.Buffer, add UTF-16 text, call Font.Shape(), then sum GlyphPositions[i].XAdvance (scaled by fontSize/512) to get accurate Arabic text width.
- Use SKFont.MeasureText(ReadOnlySpan<ushort> glyphs) with HarfBuzz-shaped glyph IDs to get Skia's own measurement of the correctly-shaped glyph sequence.

### Resolution Proposals

**Hypothesis:** SKFont.MeasureText() does not perform text shaping; Arabic requires HarfBuzz to resolve ligatures and correct glyph sequences before measuring.

1. **Use HarfBuzz for shaped text measurement** — workaround, confidence 0.88 (88%), cost/s, validated=untested
   - Use SkiaSharp.HarfBuzz to shape the Arabic text buffer, then sum GlyphPositions[i].XAdvance scaled to the desired font size.
2. **Pass HarfBuzz-shaped glyph IDs to SKFont.MeasureText** — workaround, confidence 0.82 (82%), cost/s, validated=untested
   - After shaping with HarfBuzz, extract the GlyphInfo[i].Codepoint (glyph IDs) array, convert to ushort[], and call SKFont.MeasureText(ReadOnlySpan<ushort>) with SKTextEncoding.GlyphId to let Skia measure the already-shaped glyph sequence.

**Recommended proposal:** Use HarfBuzz for shaped text measurement

**Why:** Most direct and widely documented approach. Confirmed working by contributor Gillibald in issue comments.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.85 (85%) |
| Reason | SKFont.MeasureText() does not perform text shaping by design. Arabic script requires HarfBuzz for correct ligature-aware measurement. Contributor confirmed this and demonstrated the workaround. GDI comparison is not equivalent due to MeasureString's extra padding. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Confirm existing labels are correct: type/question, area/SkiaSharp, os/Windows-Classic, tenet/compatibility | labels=type/question, area/SkiaSharp, os/Windows-Classic, tenet/compatibility |
| add-comment | high | 0.85 (85%) | Explain why MeasureText differs for Arabic text and direct to HarfBuzz workaround | — |
| close-issue | medium | 0.85 (85%) | Close as not a bug — by-design behavior, HarfBuzz workaround exists | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report.

`SKFont.MeasureText()` (and the deprecated `SKPaint.MeasureText()`) measures text by summing raw glyph advance widths without performing **text shaping**. Arabic script uses ligatures — multiple Unicode code points combine into fewer shaped glyphs. Without shaping, each code point is counted independently, producing a wider measurement than the actual rendered width.

This is by-design behavior in Skia: **proper Arabic measurement requires HarfBuzz text shaping**.

Note also that GDI's `MeasureString` is not a direct comparison target — per [MSDN docs](https://docs.microsoft.com/en-us/dotnet/api/system.drawing.graphics.measurestring) it adds extra space before/after the string for overhanging glyphs, so the numbers will always differ.

**Recommended approach** — use the `SkiaSharp.HarfBuzz` NuGet package:

1. Create a `HarfBuzzSharp.Buffer`, add your text, and call `font.Shape(buffer)` to get ligature-aware glyph positions.
2. Sum `buffer.GlyphPositions[i].XAdvance` across all glyphs, scaled by `fontSize / font.UnitsPerEm`, to get the shaped text width.

Alternatively, extract the shaped glyph IDs from `buffer.GlyphInfos[i].Codepoint`, convert to `ushort[]`, and pass to `SKFont.MeasureText(ReadOnlySpan<ushort>, paint)` with `TextEncoding = SKTextEncoding.GlyphId` so Skia measures the already-shaped glyph sequence.

Closing as by-design. If you hit specific issues with the HarfBuzz integration, please open a new issue with a minimal reproduction.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1102,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T13:08:00Z",
    "currentLabels": [
      "type/question",
      "os/Windows-Classic",
      "area/SkiaSharp",
      "tenet/compatibility",
      "triage/triaged"
    ]
  },
  "summary": "Reporter asks why SKPaint.MeasureText() returns a significantly wider measurement for Arabic text compared to GDI MeasureString, and whether this is a bug or if there is a workaround using SkiaSharp alone.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Load a Traditional Arabic TTF font with SKTypeface.FromFile()",
        "Call SKPaint.MeasureText() on Arabic string \"مرحبا بكم في التزامن \"",
        "Compare result (95.61 pts) to GDI Graphics.MeasureString() result (~66 pts)"
      ],
      "environmentDetails": "SkiaSharp 1.59.3, .NET Core 2.1, Visual Studio 2017, Windows",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1102",
          "description": "Original issue with 35 comments showing extended investigation"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.59.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "SKFont.MeasureText() still calls sk_font_measure_text_no_return directly without text shaping — behavior is unchanged by design."
    }
  },
  "analysis": {
    "summary": "SKFont.MeasureText() / SKPaint.MeasureText() calls Skia's sk_font_measure_text_no_return which performs raw glyph-by-glyph measurement without text shaping. Arabic uses ligatures: multiple code points combine into fewer shaped glyphs, so the unshaped path counts each code point independently and returns a larger width. This is by-design — proper Arabic measurement requires HarfBuzz text shaping (SkiaSharp.HarfBuzz package). The GDI comparison is also imperfect because MeasureString adds extra padding by design per MSDN docs.",
    "rationale": "Classified as type/question because the reporter explicitly asks 'is this a bug?' and 'any way to achieve this in SkiaSharp 1.59.3?' — not reporting definitively broken behavior. The behavior is by-design: Skia's MeasureText never did text shaping. The contributor (Gillibald) confirmed this and demonstrated the workaround using HarfBuzz. Existing labels are correct.",
    "keySignals": [
      {
        "text": "Simple answer it is not possible to solve without HarfBuzz",
        "source": "comment by Gillibald (CONTRIBUTOR)",
        "interpretation": "Confirms by-design behavior: SKPaint.MeasureText does not perform text shaping for complex scripts."
      },
      {
        "text": "please confirm me whether it's a bug",
        "source": "issue body",
        "interpretation": "Reporter is asking a question about expected behavior, not definitively reporting a defect."
      },
      {
        "text": "The MeasureString method is designed for use with individual strings and includes a small amount of extra space before and after the string",
        "source": "comment by Gillibald quoting MSDN docs",
        "interpretation": "GDI MeasureString adds extra padding — not an apples-to-apples comparison with SKFont.MeasureText."
      },
      {
        "text": "i got correct measurement by implementing code from the above picture",
        "source": "comment by Jack-Paul",
        "interpretation": "Reporter confirmed HarfBuzz-based approach eventually produced better results."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKFont.cs",
        "lines": "315-323",
        "finding": "SKFont.MeasureText() calls SkiaApi.sk_font_measure_text_no_return() directly — no text shaping is performed. Glyph widths are summed without ligature or BiDi processing.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "lines": "284-286",
        "finding": "SKPaint.MeasureText() is marked [Obsolete] and delegates to SKFont.MeasureText() — same unshaping behavior.",
        "relevance": "direct"
      },
      {
        "file": "binding/HarfBuzzSharp/Buffer.cs",
        "lines": "82-84",
        "finding": "HarfBuzzSharp.Buffer.GlyphPositions exposes shaped glyph positions (XAdvance per glyph) after Font.Shape() is called — this is the correct path for Arabic text measurement.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use SkiaSharp.HarfBuzz package: create HarfBuzzSharp.Buffer, add UTF-16 text, call Font.Shape(), then sum GlyphPositions[i].XAdvance (scaled by fontSize/512) to get accurate Arabic text width.",
      "Use SKFont.MeasureText(ReadOnlySpan<ushort> glyphs) with HarfBuzz-shaped glyph IDs to get Skia's own measurement of the correctly-shaped glyph sequence."
    ],
    "resolution": {
      "hypothesis": "SKFont.MeasureText() does not perform text shaping; Arabic requires HarfBuzz to resolve ligatures and correct glyph sequences before measuring.",
      "proposals": [
        {
          "title": "Use HarfBuzz for shaped text measurement",
          "description": "Use SkiaSharp.HarfBuzz to shape the Arabic text buffer, then sum GlyphPositions[i].XAdvance scaled to the desired font size.",
          "category": "workaround",
          "confidence": 0.88,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Pass HarfBuzz-shaped glyph IDs to SKFont.MeasureText",
          "description": "After shaping with HarfBuzz, extract the GlyphInfo[i].Codepoint (glyph IDs) array, convert to ushort[], and call SKFont.MeasureText(ReadOnlySpan<ushort>) with SKTextEncoding.GlyphId to let Skia measure the already-shaped glyph sequence.",
          "category": "workaround",
          "confidence": 0.82,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use HarfBuzz for shaped text measurement",
      "recommendedReason": "Most direct and widely documented approach. Confirmed working by contributor Gillibald in issue comments."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.85,
      "reason": "SKFont.MeasureText() does not perform text shaping by design. Arabic script requires HarfBuzz for correct ligature-aware measurement. Contributor confirmed this and demonstrated the workaround. GDI comparison is not equivalent due to MeasureString's extra padding.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Confirm existing labels are correct: type/question, area/SkiaSharp, os/Windows-Classic, tenet/compatibility",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain why MeasureText differs for Arabic text and direct to HarfBuzz workaround",
        "risk": "high",
        "confidence": 0.85,
        "comment": "Thanks for the detailed report.\n\n`SKFont.MeasureText()` (and the deprecated `SKPaint.MeasureText()`) measures text by summing raw glyph advance widths without performing **text shaping**. Arabic script uses ligatures — multiple Unicode code points combine into fewer shaped glyphs. Without shaping, each code point is counted independently, producing a wider measurement than the actual rendered width.\n\nThis is by-design behavior in Skia: **proper Arabic measurement requires HarfBuzz text shaping**.\n\nNote also that GDI's `MeasureString` is not a direct comparison target — per [MSDN docs](https://docs.microsoft.com/en-us/dotnet/api/system.drawing.graphics.measurestring) it adds extra space before/after the string for overhanging glyphs, so the numbers will always differ.\n\n**Recommended approach** — use the `SkiaSharp.HarfBuzz` NuGet package:\n\n1. Create a `HarfBuzzSharp.Buffer`, add your text, and call `font.Shape(buffer)` to get ligature-aware glyph positions.\n2. Sum `buffer.GlyphPositions[i].XAdvance` across all glyphs, scaled by `fontSize / font.UnitsPerEm`, to get the shaped text width.\n\nAlternatively, extract the shaped glyph IDs from `buffer.GlyphInfos[i].Codepoint`, convert to `ushort[]`, and pass to `SKFont.MeasureText(ReadOnlySpan<ushort>, paint)` with `TextEncoding = SKTextEncoding.GlyphId` so Skia measures the already-shaped glyph sequence.\n\nClosing as by-design. If you hit specific issues with the HarfBuzz integration, please open a new issue with a minimal reproduction."
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — by-design behavior, HarfBuzz workaround exists",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
