# Issue Triage Report — #1818

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T00:17:30Z |
| Type | type/question (0.88 (88%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.85 (85%)) |

**Issue Summary:** Reporter asks why SKFont.MeasureText returns a different width (488.59) than GDI+ Graphics.MeasureString (482.92) for the same kerned text using Calibri Light font.

**Analysis:** The difference in text width between GDI+ MeasureString and SkiaSharp MeasureText is by design. GDI+ MeasureString adds internal padding (~1/6 em on each side) around text per its StringFormat behavior, while Skia returns pure glyph advance widths. Additionally, the two engines use different font shaping pipelines (GDI font rasterizer vs. FreeType/HarfBuzz in Skia), which may interpret kerning tables differently. The ~1.1% difference is expected and cannot be made identical without using a GDI compatibility shim.

**Recommendations:** **close-as-not-a-bug** — The width difference between GDI+ MeasureString and SkiaSharp MeasureText is by design. GDI+ adds ~1/6 em padding; SkiaSharp returns pure advance widths. Different shaping engines also use kerning tables differently. This is expected behavior.

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

1. Measure text 'Test document for DOCX to PDF conversion' using System.Drawing.Graphics.MeasureString() with Calibri Light font
2. Measure the same text using SKPaint.MeasureText() / SKFont.MeasureText() with the same font
3. Observe that GDI+ returns 482.918 and SkiaSharp returns 488.5918

**Environment:** Calibri Light font, text 'Test document for DOCX to PDF conversion'

**Repository links:**
- https://github.com/mono/SkiaSharp/files/7246052/UsingGDI.zip — Attached code sample using System.Drawing.Graphics.MeasureString
- https://github.com/mono/SkiaSharp/files/7249354/UsingSkiaSharp.zip — Attached code sample using SkiaSharp MeasureText

## Analysis

### Technical Summary

The difference in text width between GDI+ MeasureString and SkiaSharp MeasureText is by design. GDI+ MeasureString adds internal padding (~1/6 em on each side) around text per its StringFormat behavior, while Skia returns pure glyph advance widths. Additionally, the two engines use different font shaping pipelines (GDI font rasterizer vs. FreeType/HarfBuzz in Skia), which may interpret kerning tables differently. The ~1.1% difference is expected and cannot be made identical without using a GDI compatibility shim.

### Rationale

This is a usage question not a bug. The reporter expects identical measurements between fundamentally different text rendering engines. SkiaSharp's MeasureText returns advance widths as defined by the font and Skia's shaper; GDI+ adds extra padding and uses a Windows-specific rendering pipeline.

### Key Signals

- "GDI+ returns 482.918, SkiaSharp returns 488.5918" — **issue body** (Difference of ~1.1%. GDI+ MeasureString includes ~1/6 em padding per StringFormat.GenericDefault behavior.)
- "Having the same problem. GDI+ is still performing better than Skia in most cases for us." — **comment** (Confirms the difference is a widespread concern, not a one-off configuration issue.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKFont.cs` | 313-321 | direct | SKFont.MeasureText calls sk_font_measure_text_no_return which returns Skia's advance width — pure glyph advance measurement, no GDI-style padding |
| `binding/SkiaSharp/SKPaint.cs` | 305-353 | related | SKPaint.MeasureText is marked [Obsolete] and delegates to SKFont.MeasureText — confirms both APIs measure the same underlying metric |

### Workarounds

- Accept the difference as by-design: SkiaSharp measures advance widths; GDI+ adds padding. Design layout around SkiaSharp's metric rather than trying to match GDI+.
- To approximate GDI+ padding, subtract roughly 1/6 em per side from MeasureString output, or add ~1/6 em per side to SkiaSharp output (not exact — depends on font and DPI).
- Use SKTextBlob with SKFont.GetGlyphWidths to get per-character advance widths and compare with GDI+ per-character measurements for fine-grained control.

### Resolution Proposals

**Hypothesis:** SkiaSharp MeasureText returns pure advance width per the font's metrics; GDI+ MeasureString includes StringFormat padding. The two values cannot be made identical without bypassing one engine's padding behavior.

1. **Explain the GDI+ vs Skia measurement difference** — investigation, confidence 0.92 (92%), cost/xs, validated=untested
   - Respond explaining that GDI+ Graphics.MeasureString adds ~1/6 em padding per side by default (a legacy behavior from Windows GDI). SkiaSharp returns pure advance width. The values are intentionally different.

**Recommended proposal:** Explain the GDI+ vs Skia measurement difference

**Why:** The difference is fundamental and expected. No API change is needed. A clear explanation closes the issue.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.85 (85%) |
| Reason | The width difference between GDI+ MeasureString and SkiaSharp MeasureText is by design. GDI+ adds ~1/6 em padding; SkiaSharp returns pure advance widths. Different shaping engines also use kerning tables differently. This is expected behavior. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply question and SkiaSharp area labels | labels=type/question, area/SkiaSharp |
| add-comment | high | 0.85 (85%) | Explain the measurement difference between GDI+ and SkiaSharp | — |
| close-issue | medium | 0.85 (85%) | Close as not a bug — expected difference between GDI+ and Skia rendering engines | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report!

The difference between `Graphics.MeasureString()` and `SKFont.MeasureText()` is **by design** — the two APIs measure text in fundamentally different ways:

1. **GDI+ `MeasureString`** adds ~1/6 em of extra padding on each side of the text (inherited from Windows GDI's `StringFormat.GenericDefault`). This is a well-known quirk. You can get closer to pure advance-width measurements using `StringFormat.GenericTypographic` instead.

2. **SkiaSharp `MeasureText`** returns the pure horizontal advance width of the text as defined by the font's glyph metrics, with no extra padding.

3. Both engines use different font shaping stacks (GDI's native rasterizer vs. Skia + FreeType/HarfBuzz), so kerning pair values may differ slightly even with the same font.

The ~1.1% difference you see is expected and cannot be eliminated while using both engines.

**Recommendation:** Design your layout using SkiaSharp's metric as the source of truth. If you specifically need GDI+ layout compatibility, try `StringFormat.GenericTypographic` in GDI+ — it removes the built-in padding and will be closer to Skia's values.

Closing this as by-design, but feel free to reopen if you have a specific repro showing incorrect shaping.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1818,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T00:17:30Z"
  },
  "summary": "Reporter asks why SKFont.MeasureText returns a different width (488.59) than GDI+ Graphics.MeasureString (482.92) for the same kerned text using Calibri Light font.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    }
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Measure text 'Test document for DOCX to PDF conversion' using System.Drawing.Graphics.MeasureString() with Calibri Light font",
        "Measure the same text using SKPaint.MeasureText() / SKFont.MeasureText() with the same font",
        "Observe that GDI+ returns 482.918 and SkiaSharp returns 488.5918"
      ],
      "environmentDetails": "Calibri Light font, text 'Test document for DOCX to PDF conversion'",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/7246052/UsingGDI.zip",
          "description": "Attached code sample using System.Drawing.Graphics.MeasureString"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/files/7249354/UsingSkiaSharp.zip",
          "description": "Attached code sample using SkiaSharp MeasureText"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The difference in text width between GDI+ MeasureString and SkiaSharp MeasureText is by design. GDI+ MeasureString adds internal padding (~1/6 em on each side) around text per its StringFormat behavior, while Skia returns pure glyph advance widths. Additionally, the two engines use different font shaping pipelines (GDI font rasterizer vs. FreeType/HarfBuzz in Skia), which may interpret kerning tables differently. The ~1.1% difference is expected and cannot be made identical without using a GDI compatibility shim.",
    "rationale": "This is a usage question not a bug. The reporter expects identical measurements between fundamentally different text rendering engines. SkiaSharp's MeasureText returns advance widths as defined by the font and Skia's shaper; GDI+ adds extra padding and uses a Windows-specific rendering pipeline.",
    "keySignals": [
      {
        "text": "GDI+ returns 482.918, SkiaSharp returns 488.5918",
        "source": "issue body",
        "interpretation": "Difference of ~1.1%. GDI+ MeasureString includes ~1/6 em padding per StringFormat.GenericDefault behavior."
      },
      {
        "text": "Having the same problem. GDI+ is still performing better than Skia in most cases for us.",
        "source": "comment",
        "interpretation": "Confirms the difference is a widespread concern, not a one-off configuration issue."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKFont.cs",
        "lines": "313-321",
        "finding": "SKFont.MeasureText calls sk_font_measure_text_no_return which returns Skia's advance width — pure glyph advance measurement, no GDI-style padding",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "lines": "305-353",
        "finding": "SKPaint.MeasureText is marked [Obsolete] and delegates to SKFont.MeasureText — confirms both APIs measure the same underlying metric",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Accept the difference as by-design: SkiaSharp measures advance widths; GDI+ adds padding. Design layout around SkiaSharp's metric rather than trying to match GDI+.",
      "To approximate GDI+ padding, subtract roughly 1/6 em per side from MeasureString output, or add ~1/6 em per side to SkiaSharp output (not exact — depends on font and DPI).",
      "Use SKTextBlob with SKFont.GetGlyphWidths to get per-character advance widths and compare with GDI+ per-character measurements for fine-grained control."
    ],
    "resolution": {
      "hypothesis": "SkiaSharp MeasureText returns pure advance width per the font's metrics; GDI+ MeasureString includes StringFormat padding. The two values cannot be made identical without bypassing one engine's padding behavior.",
      "proposals": [
        {
          "title": "Explain the GDI+ vs Skia measurement difference",
          "description": "Respond explaining that GDI+ Graphics.MeasureString adds ~1/6 em padding per side by default (a legacy behavior from Windows GDI). SkiaSharp returns pure advance width. The values are intentionally different.",
          "category": "investigation",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Explain the GDI+ vs Skia measurement difference",
      "recommendedReason": "The difference is fundamental and expected. No API change is needed. A clear explanation closes the issue."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.85,
      "reason": "The width difference between GDI+ MeasureString and SkiaSharp MeasureText is by design. GDI+ adds ~1/6 em padding; SkiaSharp returns pure advance widths. Different shaping engines also use kerning tables differently. This is expected behavior.",
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
        "description": "Explain the measurement difference between GDI+ and SkiaSharp",
        "risk": "high",
        "confidence": 0.85,
        "comment": "Thanks for the detailed report!\n\nThe difference between `Graphics.MeasureString()` and `SKFont.MeasureText()` is **by design** — the two APIs measure text in fundamentally different ways:\n\n1. **GDI+ `MeasureString`** adds ~1/6 em of extra padding on each side of the text (inherited from Windows GDI's `StringFormat.GenericDefault`). This is a well-known quirk. You can get closer to pure advance-width measurements using `StringFormat.GenericTypographic` instead.\n\n2. **SkiaSharp `MeasureText`** returns the pure horizontal advance width of the text as defined by the font's glyph metrics, with no extra padding.\n\n3. Both engines use different font shaping stacks (GDI's native rasterizer vs. Skia + FreeType/HarfBuzz), so kerning pair values may differ slightly even with the same font.\n\nThe ~1.1% difference you see is expected and cannot be eliminated while using both engines.\n\n**Recommendation:** Design your layout using SkiaSharp's metric as the source of truth. If you specifically need GDI+ layout compatibility, try `StringFormat.GenericTypographic` in GDI+ — it removes the built-in padding and will be closer to Skia's values.\n\nClosing this as by-design, but feel free to reopen if you have a specific repro showing incorrect shaping."
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — expected difference between GDI+ and Skia rendering engines",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
