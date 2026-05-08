# Issue Triage Report — #2227

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T23:25:15Z |
| Type | type/feature-request (0.95 (95%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | keep-open (0.87 (87%)) |

**Issue Summary:** Feature request to add a TextVerticalAlign property (Top/Middle/Bottom) to SKPaint or SKFont, analogous to the horizontal SKTextAlign, to simplify vertical text positioning for use cases like polar graph labels.

**Analysis:** No TextVerticalAlign equivalent exists in SkiaSharp. SKPaint.TextAlign (horizontal) is itself deprecated in favor of SKTextAlign method parameters, and Skia's upstream C++ API does not expose a vertical alignment concept. Vertical positioning must be manually computed using SKFontMetrics.Ascent and .Descent, which is a confirmed pattern already used in SkiaSharp samples (TextLabSample.cs). A convenience API would be welcome but requires design to fit the modern parameter-based approach.

**Recommendations:** **keep-open** — Valid, well-described feature request with clear use case and community interest. The workaround is functional but verbose; a convenience API would genuinely improve the developer experience.

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

## Evidence

### Reproduction

1. Create a polar graph with text labels at various angles
2. Set SKTextAlign.Center to try to center labels horizontally
3. Observe that labels are not vertically centered — the anchor is always at the text baseline, not the visual center

**Screenshots:**
- https://user-images.githubusercontent.com/1275999/187276713-e19f35d6-c07b-4a35-b8ee-d905f534ce15.png — Polar graph labels with SKTextAlign.Center showing inconsistent vertical placement due to baseline anchor
- https://user-images.githubusercontent.com/1275999/187277314-3724fa72-21b5-4bc8-8842-148886d82ac7.png — Same graph with manually computed offsets applied to approximate vertical centering
- https://user-images.githubusercontent.com/1275999/187277054-c7f1a4e9-9cc1-47d8-a09f-c0fc965cd13a.png — Desired 9-anchor-point grid: Top/Middle/Bottom x Left/Center/Right

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | No TextVerticalAlign API has been added; SKPaint.TextAlign is deprecated in favor of SKTextAlign method parameters but no vertical equivalent exists in current codebase. |

## Analysis

### Technical Summary

No TextVerticalAlign equivalent exists in SkiaSharp. SKPaint.TextAlign (horizontal) is itself deprecated in favor of SKTextAlign method parameters, and Skia's upstream C++ API does not expose a vertical alignment concept. Vertical positioning must be manually computed using SKFontMetrics.Ascent and .Descent, which is a confirmed pattern already used in SkiaSharp samples (TextLabSample.cs). A convenience API would be welcome but requires design to fit the modern parameter-based approach.

### Rationale

This is a feature request for entirely new functionality — no vertical alignment property or parameter exists in SKPaint, SKFont, or any DrawText overload. The horizontal SKTextAlign is already deprecated; a vertical equivalent would need to follow the same modern method-parameter pattern. The manual workaround using SKFontMetrics is valid and confirmed by samples. Keeping the issue open is correct — the request is well-described with clear use cases and community interest.

### Key Signals

- "the 'anchor' for the text is at the bottom left corner" — **issue body** (Reporter correctly identifies that Skia draws text with the anchor at the baseline, not the visual center of the glyph bounds.)
- "I'd like to see a property added to SKPaint that implements TextVerticalAlign (Bottom, Middle, Top)" — **issue body** (Clear feature request for a new enum and property/parameter analogous to SKTextAlign.)
- "RichTextKit is not an option because it only supports up to .NET 5.0, and I need to target iOS" — **comment by JGKle (2023-11-25)** (Third-party library workaround (RichTextKit) not viable for cross-platform targets; confirms demand for a first-class SkiaSharp API.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPaint.cs` | 229-233 | direct | SKPaint.TextAlign is marked [Obsolete] — users directed to SKTextAlign method overloads instead. No vertical alignment property exists anywhere on SKPaint. |
| `binding/SkiaSharp/SKCanvas.cs` | 639-660 | direct | DrawText(string, float, float, SKTextAlign, SKFont, SKPaint) implements horizontal alignment by adjusting X via font.MeasureText(). No vertical Y adjustment exists. This is where a vertical alignment parameter would be added. |
| `binding/SkiaSharp/Definitions.cs` | 293-329 | direct | SKFontMetrics exposes Ascent (negative, above baseline), Descent (positive, below baseline), Top, Bottom, CapHeight, XHeight. These are the building blocks for computing vertical offsets, confirming the workaround path. |

### Workarounds

- Use SKFont.GetFontMetrics(out var metrics) to obtain Ascent (negative) and Descent (positive), then adjust Y: middle = anchorY - (metrics.Ascent + metrics.Descent) / 2f; top = anchorY - metrics.Ascent; bottom = anchorY - metrics.Descent
- Use the SKFont.Metrics shorthand property to access the same values without an out parameter: var metrics = font.Metrics;

### Resolution Proposals

**Hypothesis:** Vertical text alignment requires Y-coordinate adjustment based on font metrics. SkiaSharp can expose this as convenience overloads with a new SKTextVerticalAlign enum, mirroring the horizontal SKTextAlign approach.

1. **Manual workaround using SKFontMetrics** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Use SKFont.GetFontMetrics() to compute Y offsets. In Skia, Ascent is negative (above baseline) and Descent is positive (below baseline). Applying the offset shifts the baseline so the desired anchor point lands at the target coordinate.

```csharp
using var font = new SKFont(typeface, textSize);
font.GetFontMetrics(out var metrics);

// Center: visual center of text height at anchorY
float yCenter = anchorY - (metrics.Ascent + metrics.Descent) / 2f;

// Top: top of ascenders at anchorY
float yTop = anchorY - metrics.Ascent;

// Bottom: bottom of descenders at anchorY
float yBottom = anchorY - metrics.Descent;

// Draw centered both horizontally and vertically:
canvas.DrawText(text, anchorX, yCenter, SKTextAlign.Center, font, paint);
```
2. **Add vertical alignment overloads to DrawText** — fix, confidence 0.75 (75%), cost/s, validated=untested
   - Add a new SKTextVerticalAlign enum (Top, Middle, Bottom) and new overloads to SKCanvas.DrawText that accept it, computing the Y offset internally using font metrics — mirroring how SKTextAlign is handled for horizontal alignment in lines 648-653 of SKCanvas.cs.

**Recommended proposal:** Manual workaround using SKFontMetrics

**Why:** Immediately usable with existing APIs; no library changes needed. The identical pattern (var y = anchorY - (metrics.Ascent + metrics.Descent) / 2f) is already used in SkiaSharp samples.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.87 (87%) |
| Reason | Valid, well-described feature request with clear use case and community interest. The workaround is functional but verbose; a convenience API would genuinely improve the developer experience. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply feature-request and SkiaSharp area labels | labels=type/feature-request, area/SkiaSharp |
| add-comment | medium | 0.90 (90%) | Post workaround using SKFontMetrics with code example, confirm request is tracked | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed diagrams — they clearly illustrate the problem.

While a built-in `TextVerticalAlign` doesn't exist yet, you can achieve all three anchor points today using `SKFontMetrics`. In Skia, text is drawn with the **baseline** at the Y coordinate you pass. `Ascent` (a negative number) is how far above the baseline glyphs reach; `Descent` (positive) is how far below:

```csharp
using var font = new SKFont(typeface, textSize);
font.GetFontMetrics(out var metrics);

// Center: place the visual center of the text at anchorY
float yCenter = anchorY - (metrics.Ascent + metrics.Descent) / 2f;

// Top: place the top of ascenders at anchorY
float yTop = anchorY - metrics.Ascent;

// Bottom: place the bottom of descenders at anchorY
float yBottom = anchorY - metrics.Descent;

// Draw centered both horizontally and vertically:
canvas.DrawText(text, anchorX, yCenter, SKTextAlign.Center, font, paint);
```

Keeping this open as a feature request to add a convenience `SKTextVerticalAlign` enum and corresponding overloads.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2227,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T23:25:15Z"
  },
  "summary": "Feature request to add a TextVerticalAlign property (Top/Middle/Bottom) to SKPaint or SKFont, analogous to the horizontal SKTextAlign, to simplify vertical text positioning for use cases like polar graph labels.",
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
        "Create a polar graph with text labels at various angles",
        "Set SKTextAlign.Center to try to center labels horizontally",
        "Observe that labels are not vertically centered — the anchor is always at the text baseline, not the visual center"
      ],
      "screenshots": [
        {
          "url": "https://user-images.githubusercontent.com/1275999/187276713-e19f35d6-c07b-4a35-b8ee-d905f534ce15.png",
          "description": "Polar graph labels with SKTextAlign.Center showing inconsistent vertical placement due to baseline anchor"
        },
        {
          "url": "https://user-images.githubusercontent.com/1275999/187277314-3724fa72-21b5-4bc8-8842-148886d82ac7.png",
          "description": "Same graph with manually computed offsets applied to approximate vertical centering"
        },
        {
          "url": "https://user-images.githubusercontent.com/1275999/187277054-c7f1a4e9-9cc1-47d8-a09f-c0fc965cd13a.png",
          "description": "Desired 9-anchor-point grid: Top/Middle/Bottom x Left/Center/Right"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "No TextVerticalAlign API has been added; SKPaint.TextAlign is deprecated in favor of SKTextAlign method parameters but no vertical equivalent exists in current codebase."
    }
  },
  "analysis": {
    "summary": "No TextVerticalAlign equivalent exists in SkiaSharp. SKPaint.TextAlign (horizontal) is itself deprecated in favor of SKTextAlign method parameters, and Skia's upstream C++ API does not expose a vertical alignment concept. Vertical positioning must be manually computed using SKFontMetrics.Ascent and .Descent, which is a confirmed pattern already used in SkiaSharp samples (TextLabSample.cs). A convenience API would be welcome but requires design to fit the modern parameter-based approach.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "lines": "229-233",
        "finding": "SKPaint.TextAlign is marked [Obsolete] — users directed to SKTextAlign method overloads instead. No vertical alignment property exists anywhere on SKPaint.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "639-660",
        "finding": "DrawText(string, float, float, SKTextAlign, SKFont, SKPaint) implements horizontal alignment by adjusting X via font.MeasureText(). No vertical Y adjustment exists. This is where a vertical alignment parameter would be added.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "293-329",
        "finding": "SKFontMetrics exposes Ascent (negative, above baseline), Descent (positive, below baseline), Top, Bottom, CapHeight, XHeight. These are the building blocks for computing vertical offsets, confirming the workaround path.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "the 'anchor' for the text is at the bottom left corner",
        "source": "issue body",
        "interpretation": "Reporter correctly identifies that Skia draws text with the anchor at the baseline, not the visual center of the glyph bounds."
      },
      {
        "text": "I'd like to see a property added to SKPaint that implements TextVerticalAlign (Bottom, Middle, Top)",
        "source": "issue body",
        "interpretation": "Clear feature request for a new enum and property/parameter analogous to SKTextAlign."
      },
      {
        "text": "RichTextKit is not an option because it only supports up to .NET 5.0, and I need to target iOS",
        "source": "comment by JGKle (2023-11-25)",
        "interpretation": "Third-party library workaround (RichTextKit) not viable for cross-platform targets; confirms demand for a first-class SkiaSharp API."
      }
    ],
    "workarounds": [
      "Use SKFont.GetFontMetrics(out var metrics) to obtain Ascent (negative) and Descent (positive), then adjust Y: middle = anchorY - (metrics.Ascent + metrics.Descent) / 2f; top = anchorY - metrics.Ascent; bottom = anchorY - metrics.Descent",
      "Use the SKFont.Metrics shorthand property to access the same values without an out parameter: var metrics = font.Metrics;"
    ],
    "rationale": "This is a feature request for entirely new functionality — no vertical alignment property or parameter exists in SKPaint, SKFont, or any DrawText overload. The horizontal SKTextAlign is already deprecated; a vertical equivalent would need to follow the same modern method-parameter pattern. The manual workaround using SKFontMetrics is valid and confirmed by samples. Keeping the issue open is correct — the request is well-described with clear use cases and community interest.",
    "resolution": {
      "hypothesis": "Vertical text alignment requires Y-coordinate adjustment based on font metrics. SkiaSharp can expose this as convenience overloads with a new SKTextVerticalAlign enum, mirroring the horizontal SKTextAlign approach.",
      "proposals": [
        {
          "title": "Manual workaround using SKFontMetrics",
          "description": "Use SKFont.GetFontMetrics() to compute Y offsets. In Skia, Ascent is negative (above baseline) and Descent is positive (below baseline). Applying the offset shifts the baseline so the desired anchor point lands at the target coordinate.",
          "category": "workaround",
          "codeSnippet": "using var font = new SKFont(typeface, textSize);\nfont.GetFontMetrics(out var metrics);\n\n// Center: visual center of text height at anchorY\nfloat yCenter = anchorY - (metrics.Ascent + metrics.Descent) / 2f;\n\n// Top: top of ascenders at anchorY\nfloat yTop = anchorY - metrics.Ascent;\n\n// Bottom: bottom of descenders at anchorY\nfloat yBottom = anchorY - metrics.Descent;\n\n// Draw centered both horizontally and vertically:\ncanvas.DrawText(text, anchorX, yCenter, SKTextAlign.Center, font, paint);",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Add vertical alignment overloads to DrawText",
          "description": "Add a new SKTextVerticalAlign enum (Top, Middle, Bottom) and new overloads to SKCanvas.DrawText that accept it, computing the Y offset internally using font metrics — mirroring how SKTextAlign is handled for horizontal alignment in lines 648-653 of SKCanvas.cs.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Manual workaround using SKFontMetrics",
      "recommendedReason": "Immediately usable with existing APIs; no library changes needed. The identical pattern (var y = anchorY - (metrics.Ascent + metrics.Descent) / 2f) is already used in SkiaSharp samples."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.87,
      "reason": "Valid, well-described feature request with clear use case and community interest. The workaround is functional but verbose; a convenience API would genuinely improve the developer experience.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request and SkiaSharp area labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post workaround using SKFontMetrics with code example, confirm request is tracked",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the detailed diagrams — they clearly illustrate the problem.\n\nWhile a built-in `TextVerticalAlign` doesn't exist yet, you can achieve all three anchor points today using `SKFontMetrics`. In Skia, text is drawn with the **baseline** at the Y coordinate you pass. `Ascent` (a negative number) is how far above the baseline glyphs reach; `Descent` (positive) is how far below:\n\n```csharp\nusing var font = new SKFont(typeface, textSize);\nfont.GetFontMetrics(out var metrics);\n\n// Center: place the visual center of the text at anchorY\nfloat yCenter = anchorY - (metrics.Ascent + metrics.Descent) / 2f;\n\n// Top: place the top of ascenders at anchorY\nfloat yTop = anchorY - metrics.Ascent;\n\n// Bottom: place the bottom of descenders at anchorY\nfloat yBottom = anchorY - metrics.Descent;\n\n// Draw centered both horizontally and vertically:\ncanvas.DrawText(text, anchorX, yCenter, SKTextAlign.Center, font, paint);\n```\n\nKeeping this open as a feature request to add a convenience `SKTextVerticalAlign` enum and corresponding overloads."
      }
    ]
  }
}
```

</details>
