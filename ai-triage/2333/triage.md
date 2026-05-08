# Issue Triage Report — #2333

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T03:31:41Z |
| Type | type/question (0.95 (95%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-not-a-bug (0.90 (90%)) |

**Issue Summary:** Reporter asks why SKPaint.MeasureText() and DrawText() do not interpret escape sequences (\n) for multiline text rendering — this is by-design behavior inherited from the Skia graphics engine.

**Analysis:** Skia (and SkiaSharp) does not natively support multiline text. The DrawText and MeasureText APIs operate on a single line of glyphs. The \n character has no glyph in most fonts, so it renders as a missing-glyph placeholder rectangle. Users must split text manually and draw each line at a different y-coordinate using SKFont.Spacing for line height.

**Recommendations:** **close-as-not-a-bug** — By-design behavior from Skia; usage question answered by existing APIs

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

1. Create a string containing \n
2. Call SKPaint.MeasureText() on that string
3. Observe that measurement is for a single line
4. Call canvas.DrawText() with that string
5. Observe empty rectangle glyph instead of line break

**Environment:** No platform or version specified

## Analysis

### Technical Summary

Skia (and SkiaSharp) does not natively support multiline text. The DrawText and MeasureText APIs operate on a single line of glyphs. The \n character has no glyph in most fonts, so it renders as a missing-glyph placeholder rectangle. Users must split text manually and draw each line at a different y-coordinate using SKFont.Spacing for line height.

### Rationale

The title says QUESTION and the body asks 'Am I doing something wrong? Or is there a different way?' — a usage question, not a bug. The behavior described (no newline interpretation) is by-design in Skia. Code investigation confirms DrawText creates a single SKTextBlob without newline handling, and MeasureText measures a flat glyph stream.

### Key Signals

- "SKPain.MeasureText() seems to ignore escape sequences entirely. If I include a newline command (\n), it still measures the text as a single line." — **issue body** (By-design: MeasureText measures a single glyph run; newline has no meaning in Skia's text model)
- "the sequence gets replaced by an 'empty rectangle' character (No glyph found for that character)" — **issue body** (Expected: \n (U+000A) has no glyph in standard fonts, so Skia renders the missing-glyph placeholder)
- "Am I doing something wrong? Or is there a different way to process text in skia?" — **issue body** (Usage question — reporter needs guidance on how to achieve multiline text)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 652-673 | direct | DrawText converts the string to a single SKTextBlob via SKTextBlob.Create(text, font) with no newline splitting — the \n character is treated as any other character for which no glyph is usually found |
| `binding/SkiaSharp/SKFont.cs` | 95-98 | related | SKFont.Spacing returns the recommended line spacing from font metrics via sk_font_get_metrics — this is the value users should use to increment y when manually drawing multiline text |
| `binding/SkiaSharp/SKPaint.cs` | 307-353 | direct | SKPaint.MeasureText delegates to SKFont.MeasureText, which measures a flat glyph stream without special-casing \n — confirmed by absence of any split/newline logic |

### Workarounds

- Split text by \n, then loop drawing each line using canvas.DrawText(line, x, y, font, paint) incrementing y by font.Spacing each iteration
- Use SKFont.MeasureText per line to get per-line widths for alignment

### Resolution Proposals

**Hypothesis:** Reporter does not know that Skia's text APIs are single-line only and needs an example of manual multiline rendering.

1. **Manual multiline drawing** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Split the string on \n and draw each line individually, incrementing y by SKFont.Spacing.

```csharp
var lines = text.Split('\n');
var y = startY;
foreach (var line in lines)
{
    canvas.DrawText(line, x, y, font, paint);
    y += font.Spacing;
}
```

**Recommended proposal:** Manual multiline drawing

**Why:** Simple, idiomatic, uses existing public APIs (SKFont.Spacing) — no changes to SkiaSharp needed

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.90 (90%) |
| Reason | By-design behavior from Skia; usage question answered by existing APIs |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply question and SkiaSharp area labels | labels=type/question, area/SkiaSharp |
| add-comment | high | 0.90 (90%) | Explain multiline text limitation and provide workaround | — |
| close-issue | medium | 0.85 (85%) | Close as answered — by-design behavior | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Hi @AquilaAbriel! This is expected behavior — Skia's text APIs operate on a single line of glyphs and do not interpret `\n` as a line break. The `\n` character (U+000A) has no glyph in most fonts, so it renders as a missing-glyph placeholder rectangle.

To draw multiline text, split your string manually and draw each line at an increasing y-coordinate:

```csharp
var lines = text.Split('\n');
var y = startY;
foreach (var line in lines)
{
    canvas.DrawText(line, x, y, font, paint);
    y += font.Spacing;
}
```

`SKFont.Spacing` gives you the recommended line height for the font. You can use `SKFont.MeasureText(line)` per-line if you need the width for alignment.

Hope that helps!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2333,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T03:31:41Z"
  },
  "summary": "Reporter asks why SKPaint.MeasureText() and DrawText() do not interpret escape sequences (\\n) for multiline text rendering — this is by-design behavior inherited from the Skia graphics engine.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    }
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a string containing \\n",
        "Call SKPaint.MeasureText() on that string",
        "Observe that measurement is for a single line",
        "Call canvas.DrawText() with that string",
        "Observe empty rectangle glyph instead of line break"
      ],
      "environmentDetails": "No platform or version specified"
    }
  },
  "analysis": {
    "summary": "Skia (and SkiaSharp) does not natively support multiline text. The DrawText and MeasureText APIs operate on a single line of glyphs. The \\n character has no glyph in most fonts, so it renders as a missing-glyph placeholder rectangle. Users must split text manually and draw each line at a different y-coordinate using SKFont.Spacing for line height.",
    "rationale": "The title says QUESTION and the body asks 'Am I doing something wrong? Or is there a different way?' — a usage question, not a bug. The behavior described (no newline interpretation) is by-design in Skia. Code investigation confirms DrawText creates a single SKTextBlob without newline handling, and MeasureText measures a flat glyph stream.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "652-673",
        "finding": "DrawText converts the string to a single SKTextBlob via SKTextBlob.Create(text, font) with no newline splitting — the \\n character is treated as any other character for which no glyph is usually found",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFont.cs",
        "lines": "95-98",
        "finding": "SKFont.Spacing returns the recommended line spacing from font metrics via sk_font_get_metrics — this is the value users should use to increment y when manually drawing multiline text",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "lines": "307-353",
        "finding": "SKPaint.MeasureText delegates to SKFont.MeasureText, which measures a flat glyph stream without special-casing \\n — confirmed by absence of any split/newline logic",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "SKPain.MeasureText() seems to ignore escape sequences entirely. If I include a newline command (\\n), it still measures the text as a single line.",
        "source": "issue body",
        "interpretation": "By-design: MeasureText measures a single glyph run; newline has no meaning in Skia's text model"
      },
      {
        "text": "the sequence gets replaced by an 'empty rectangle' character (No glyph found for that character)",
        "source": "issue body",
        "interpretation": "Expected: \\n (U+000A) has no glyph in standard fonts, so Skia renders the missing-glyph placeholder"
      },
      {
        "text": "Am I doing something wrong? Or is there a different way to process text in skia?",
        "source": "issue body",
        "interpretation": "Usage question — reporter needs guidance on how to achieve multiline text"
      }
    ],
    "workarounds": [
      "Split text by \\n, then loop drawing each line using canvas.DrawText(line, x, y, font, paint) incrementing y by font.Spacing each iteration",
      "Use SKFont.MeasureText per line to get per-line widths for alignment"
    ],
    "resolution": {
      "hypothesis": "Reporter does not know that Skia's text APIs are single-line only and needs an example of manual multiline rendering.",
      "proposals": [
        {
          "title": "Manual multiline drawing",
          "description": "Split the string on \\n and draw each line individually, incrementing y by SKFont.Spacing.",
          "category": "workaround",
          "codeSnippet": "var lines = text.Split('\\n');\nvar y = startY;\nforeach (var line in lines)\n{\n    canvas.DrawText(line, x, y, font, paint);\n    y += font.Spacing;\n}",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Manual multiline drawing",
      "recommendedReason": "Simple, idiomatic, uses existing public APIs (SKFont.Spacing) — no changes to SkiaSharp needed"
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.9,
      "reason": "By-design behavior from Skia; usage question answered by existing APIs",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and SkiaSharp area labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain multiline text limitation and provide workaround",
        "risk": "high",
        "confidence": 0.9,
        "comment": "Hi @AquilaAbriel! This is expected behavior — Skia's text APIs operate on a single line of glyphs and do not interpret `\\n` as a line break. The `\\n` character (U+000A) has no glyph in most fonts, so it renders as a missing-glyph placeholder rectangle.\n\nTo draw multiline text, split your string manually and draw each line at an increasing y-coordinate:\n\n```csharp\nvar lines = text.Split('\\n');\nvar y = startY;\nforeach (var line in lines)\n{\n    canvas.DrawText(line, x, y, font, paint);\n    y += font.Spacing;\n}\n```\n\n`SKFont.Spacing` gives you the recommended line height for the font. You can use `SKFont.MeasureText(line)` per-line if you need the width for alignment.\n\nHope that helps!"
      },
      {
        "type": "close-issue",
        "description": "Close as answered — by-design behavior",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
