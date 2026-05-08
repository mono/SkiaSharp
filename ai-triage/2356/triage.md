# Issue Triage Report — #2356

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-29T19:01:43Z |
| Type | type/question (0.95 (95%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-not-a-bug (0.90 (90%)) |

**Issue Summary:** Reporter asks why drawing text at Y=0 clips the top of the glyphs — by-design Skia typography: the (x,y) coordinate in DrawText is the text baseline, not the top-left corner of the bounding box.

**Analysis:** The reporter is confused about Skia's text coordinate system — the Y parameter in DrawText is the text baseline, not the top-left of the bounding box. Drawing at Y=0 puts the baseline at the top of the image, clipping ascenders. A community member already explained this in the comments. The fix is to offset Y by -Ascent so the baseline is shifted down enough to show the full ascent.

**Recommendations:** **close-as-not-a-bug** — By-design typography behavior: DrawText Y is the baseline, not top-left. The question has already been answered in the comments. No defect in SkiaSharp.

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

1. Create a 256x256 SKSurface
2. Call canvas.DrawText("SkiaSharp", new SKPoint(0, 0), paint) with TextSize=50
3. Save to PNG and observe text is clipped at the top

**Environment:** Linux, netcoreapp3.1, Arial.ttf at TextSize=50

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1516 — Related: FontSpacing and font metrics question about Ascent/Top — same coordinate system concepts

## Analysis

### Technical Summary

The reporter is confused about Skia's text coordinate system — the Y parameter in DrawText is the text baseline, not the top-left of the bounding box. Drawing at Y=0 puts the baseline at the top of the image, clipping ascenders. A community member already explained this in the comments. The fix is to offset Y by -Ascent so the baseline is shifted down enough to show the full ascent.

### Rationale

The reporter explicitly asks 'Is it a bug or did I do something wrong?' — this is a usage question about typography coordinate conventions, not a defect. The behavior matches Skia's documented design: DrawText's Y coordinate is the baseline, not the top of glyphs. Community member themcoo correctly identified the root cause in the comments.

### Key Signals

- "I want that my text has a position 0,0. But in the picture, I see that only the X position is 0, but the Y position is wrong. Is it a bug or did I do something wrong?" — **issue body** (Explicit question about by-design typography behavior — Y is baseline, not top-left.)
- "glyphs' coordinate space does not start in top left corner but at baseline. You need to position the text shifted in y-axis by -ascent (paint.FontMetrics.Ascent)" — **comment #1380169828** (Community member correctly identified the root cause — the answer is already present in the issue.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 652-673 | direct | DrawText(string, float x, float y, SKTextAlign, SKFont, SKPaint) takes (x,y) as the baseline origin — y is the baseline coordinate, not the top of the glyph. |
| `binding/SkiaSharp/Definitions.cs` | 297-330 | direct | SKFontMetrics.Ascent is a negative float (distance above baseline). Applying -Ascent as the Y coordinate shifts the baseline down so ascenders start at Y=0. |

### Workarounds

- Use -font.Metrics.Ascent as the Y coordinate: canvas.DrawText(text, 0, -font.Metrics.Ascent, font, paint)
- Use font.MeasureText to get the tight glyph bounding box and offset by -textBounds.Top for exact top-left positioning

### Resolution Proposals

**Hypothesis:** DrawText's Y coordinate is the text baseline; the user needs to offset Y by the ascent height to position text so the top of glyphs aligns with Y=0.

1. **Offset by -Ascent for top-left positioning** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Use font.Metrics.Ascent (negative value) negated as the Y coordinate so that the text top aligns with Y=0.

```csharp
// Ascent is negative in Skia (above baseline), so negate it to get the Y for top-aligned text
var metrics = font.Metrics;
canvas.DrawText("SkiaSharp", 0, -metrics.Ascent, font, paint);
```
2. **Use MeasureText bounds for tight top-left positioning** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Call font.MeasureText to get the actual glyph bounding box, then offset so the top-left of the bounding box is at (0,0). More precise than Ascent for mixed-case text.

```csharp
// textBounds.Top is negative (above baseline), so negate it for precise top-left placement
font.MeasureText("SkiaSharp", out SKRect textBounds, paint);
canvas.DrawText("SkiaSharp", -textBounds.Left, -textBounds.Top, font, paint);
```

**Recommended proposal:** Offset by -Ascent for top-left positioning

**Why:** Simpler and sufficient for most cases. The MeasureText approach is better when tight per-string bounds are required.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.90 (90%) |
| Reason | By-design typography behavior: DrawText Y is the baseline, not top-left. The question has already been answered in the comments. No defect in SkiaSharp. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply question and area labels | labels=type/question, area/SkiaSharp |
| add-comment | high | 0.90 (90%) | Post explanation of baseline coordinate system with workaround code | — |
| close-issue | medium | 0.85 (85%) | Close as answered — by-design behavior, workaround provided | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
This is by-design behavior in Skia/SkiaSharp: the `(x, y)` coordinate passed to `DrawText` is the **text baseline**, not the top-left corner of the glyphs. When you draw at `Y=0`, the baseline sits at the top of the image and the ascenders (the tall parts of uppercase letters) are clipped.

To draw text so the top of the glyphs appears at `Y=0`, offset by the negated ascent:

```csharp
// Option 1 — use font metrics (Ascent is negative in Skia, so negate it)
var metrics = font.Metrics;
canvas.DrawText("SkiaSharp", 0, -metrics.Ascent, font, paint);

// Option 2 — use MeasureText for tight per-string glyph bounds
font.MeasureText("SkiaSharp", out SKRect textBounds, paint);
canvas.DrawText("SkiaSharp", -textBounds.Left, -textBounds.Top, font, paint);
```

For more context on typography coordinates, see the [Android Typography 101](https://proandroiddev.com/android-and-typography-101-5f06722dd611) article linked in the comment above — the same baseline conventions apply in Skia.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2356,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-29T19:01:43Z"
  },
  "summary": "Reporter asks why drawing text at Y=0 clips the top of the glyphs — by-design Skia typography: the (x,y) coordinate in DrawText is the text baseline, not the top-left corner of the bounding box.",
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
        "Create a 256x256 SKSurface",
        "Call canvas.DrawText(\"SkiaSharp\", new SKPoint(0, 0), paint) with TextSize=50",
        "Save to PNG and observe text is clipped at the top"
      ],
      "environmentDetails": "Linux, netcoreapp3.1, Arial.ttf at TextSize=50",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1516",
          "description": "Related: FontSpacing and font metrics question about Ascent/Top — same coordinate system concepts"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The reporter is confused about Skia's text coordinate system — the Y parameter in DrawText is the text baseline, not the top-left of the bounding box. Drawing at Y=0 puts the baseline at the top of the image, clipping ascenders. A community member already explained this in the comments. The fix is to offset Y by -Ascent so the baseline is shifted down enough to show the full ascent.",
    "rationale": "The reporter explicitly asks 'Is it a bug or did I do something wrong?' — this is a usage question about typography coordinate conventions, not a defect. The behavior matches Skia's documented design: DrawText's Y coordinate is the baseline, not the top of glyphs. Community member themcoo correctly identified the root cause in the comments.",
    "keySignals": [
      {
        "text": "I want that my text has a position 0,0. But in the picture, I see that only the X position is 0, but the Y position is wrong. Is it a bug or did I do something wrong?",
        "source": "issue body",
        "interpretation": "Explicit question about by-design typography behavior — Y is baseline, not top-left."
      },
      {
        "text": "glyphs' coordinate space does not start in top left corner but at baseline. You need to position the text shifted in y-axis by -ascent (paint.FontMetrics.Ascent)",
        "source": "comment #1380169828",
        "interpretation": "Community member correctly identified the root cause — the answer is already present in the issue."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "652-673",
        "finding": "DrawText(string, float x, float y, SKTextAlign, SKFont, SKPaint) takes (x,y) as the baseline origin — y is the baseline coordinate, not the top of the glyph.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "297-330",
        "finding": "SKFontMetrics.Ascent is a negative float (distance above baseline). Applying -Ascent as the Y coordinate shifts the baseline down so ascenders start at Y=0.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Use -font.Metrics.Ascent as the Y coordinate: canvas.DrawText(text, 0, -font.Metrics.Ascent, font, paint)",
      "Use font.MeasureText to get the tight glyph bounding box and offset by -textBounds.Top for exact top-left positioning"
    ],
    "resolution": {
      "hypothesis": "DrawText's Y coordinate is the text baseline; the user needs to offset Y by the ascent height to position text so the top of glyphs aligns with Y=0.",
      "proposals": [
        {
          "title": "Offset by -Ascent for top-left positioning",
          "description": "Use font.Metrics.Ascent (negative value) negated as the Y coordinate so that the text top aligns with Y=0.",
          "category": "workaround",
          "codeSnippet": "// Ascent is negative in Skia (above baseline), so negate it to get the Y for top-aligned text\nvar metrics = font.Metrics;\ncanvas.DrawText(\"SkiaSharp\", 0, -metrics.Ascent, font, paint);",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Use MeasureText bounds for tight top-left positioning",
          "description": "Call font.MeasureText to get the actual glyph bounding box, then offset so the top-left of the bounding box is at (0,0). More precise than Ascent for mixed-case text.",
          "category": "workaround",
          "codeSnippet": "// textBounds.Top is negative (above baseline), so negate it for precise top-left placement\nfont.MeasureText(\"SkiaSharp\", out SKRect textBounds, paint);\ncanvas.DrawText(\"SkiaSharp\", -textBounds.Left, -textBounds.Top, font, paint);",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Offset by -Ascent for top-left positioning",
      "recommendedReason": "Simpler and sufficient for most cases. The MeasureText approach is better when tight per-string bounds are required."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.9,
      "reason": "By-design typography behavior: DrawText Y is the baseline, not top-left. The question has already been answered in the comments. No defect in SkiaSharp.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and area labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post explanation of baseline coordinate system with workaround code",
        "risk": "high",
        "confidence": 0.9,
        "comment": "This is by-design behavior in Skia/SkiaSharp: the `(x, y)` coordinate passed to `DrawText` is the **text baseline**, not the top-left corner of the glyphs. When you draw at `Y=0`, the baseline sits at the top of the image and the ascenders (the tall parts of uppercase letters) are clipped.\n\nTo draw text so the top of the glyphs appears at `Y=0`, offset by the negated ascent:\n\n```csharp\n// Option 1 — use font metrics (Ascent is negative in Skia, so negate it)\nvar metrics = font.Metrics;\ncanvas.DrawText(\"SkiaSharp\", 0, -metrics.Ascent, font, paint);\n\n// Option 2 — use MeasureText for tight per-string glyph bounds\nfont.MeasureText(\"SkiaSharp\", out SKRect textBounds, paint);\ncanvas.DrawText(\"SkiaSharp\", -textBounds.Left, -textBounds.Top, font, paint);\n```\n\nFor more context on typography coordinates, see the [Android Typography 101](https://proandroiddev.com/android-and-typography-101-5f06722dd611) article linked in the comment above — the same baseline conventions apply in Skia."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — by-design behavior, workaround provided",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
