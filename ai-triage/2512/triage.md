# Issue Triage Report — #2512

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T23:03:38Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp.HarfBuzz (0.97 (97%)) |
| Suggested action | keep-open (0.88 (88%)) |

**Issue Summary:** Feature request to add a GetShapedTextPath extension method to SkiaSharp.HarfBuzz that uses HarfBuzz shaping to return an SKPath, mirroring how DrawShapedText works.

**Analysis:** The SkiaSharp.HarfBuzz package provides DrawShapedText extension methods (CanvasExtensions.cs) that use HarfBuzz for proper Unicode text shaping. However, there is no companion GetShapedTextPath method that would return an SKPath instead of drawing. SKFont already has GetTextPath and GetTextPath(with positions) methods, but they do not apply HarfBuzz shaping. A new CanvasExtensions (or FontExtensions) method in SkiaSharp.HarfBuzz would bridge the gap by running SKShaper.Shape() to get positioned glyphs and then calling SKFont.GetTextPath with those positions.

**Recommendations:** **keep-open** — Valid, well-scoped feature request. The building blocks exist; implementation is straightforward but requires design review for API surface.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp.HarfBuzz |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Code snippets:**

```csharp
SKPath GetShapedTextPath(this SKFont font, SKShaper shaper, string text, float x, float y, SKTextAlign textAlign)
```

## Analysis

### Technical Summary

The SkiaSharp.HarfBuzz package provides DrawShapedText extension methods (CanvasExtensions.cs) that use HarfBuzz for proper Unicode text shaping. However, there is no companion GetShapedTextPath method that would return an SKPath instead of drawing. SKFont already has GetTextPath and GetTextPath(with positions) methods, but they do not apply HarfBuzz shaping. A new CanvasExtensions (or FontExtensions) method in SkiaSharp.HarfBuzz would bridge the gap by running SKShaper.Shape() to get positioned glyphs and then calling SKFont.GetTextPath with those positions.

### Rationale

This is a well-scoped feature request for SkiaSharp.HarfBuzz. The building blocks (SKShaper.Shape() + SKFont.GetTextPath with positions) already exist in the codebase; only the glue is missing. Implementation requires no C API changes. The request is reasonable and consistent with existing API patterns.

### Key Signals

- "Currently, there is no shaped equivalent to SKPaint.GetTextPath(), which can be inconvenient in scenarios that utilize CanvasExtensions.DrawShapedText() and require drawing an outline around the text." — **issue body** (Clear gap: shaping + path extraction combined is not available)
- "An extension method for SKFont that returns the shaped path of a text input." — **issue body** (Reporter wants the feature in SkiaSharp.HarfBuzz, mirroring DrawShapedText's approach)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/CanvasExtensions.cs` | 58-103 | direct | DrawShapedText overloads exist but no GetShapedTextPath equivalent. The core of DrawShapedText uses SKShaper.Shape() to get glyph codepoints and positions, then builds an SKTextBlob and calls canvas.DrawText. A GetShapedTextPath would follow the same shaping logic but call SKFont.GetTextPath with the positioned glyphs instead. |
| `binding/SkiaSharp/SKFont.cs` | 758-778 | direct | SKFont.GetTextPath(string text, ReadOnlySpan<SKPoint> positions) exists and accepts pre-positioned glyphs, which is exactly what SKShaper.Shape() produces. This makes implementation feasible without new C API changes. |
| `source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/SKShaper.cs` | 54-95 | direct | SKShaper.Shape() returns a Result with Codepoints and Points arrays, which represent glyph IDs and their 2D positions after HarfBuzz shaping. These can be passed directly to SKFont.GetTextPath. |

### Workarounds

- Manually call shaper.Shape(text, x, y, font) to get Result, then call font.GetTextPath(text, result.Points) using the shaped glyph positions.

### Resolution Proposals

**Hypothesis:** Add GetShapedTextPath extension methods to CanvasExtensions (or a new static class) in SkiaSharp.HarfBuzz, mirroring the DrawShapedText pattern: shape with SKShaper, then call SKFont.GetTextPath with positioned glyphs.

1. **Add GetShapedTextPath extension methods to SkiaSharp.HarfBuzz** — fix, cost/s, validated=untested
   - Add static extension methods alongside CanvasExtensions that shape text with HarfBuzz and return an SKPath. Mirrors the overload set of DrawShapedText.
2. **Workaround: manually compose shaper result with GetTextPath** — workaround, cost/xs, validated=untested
   - Call shaper.Shape(text, x, y, font) and pass result.Points to font.GetTextPath to achieve the same result today.

```csharp
using var shaper = new SKShaper(font.Typeface);
var result = shaper.Shape(text, x, y, font);
// Build positions array from shaped result
var positions = result.Points;
using var path = font.GetTextPath(text, positions);
```

**Recommended proposal:** Add GetShapedTextPath extension methods to SkiaSharp.HarfBuzz

**Why:** The feature is feasible with existing primitives, no C API changes needed, and follows established patterns in the codebase.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.88 (88%) |
| Reason | Valid, well-scoped feature request. The building blocks exist; implementation is straightforward but requires design review for API surface. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply type/feature-request and area/SkiaSharp.HarfBuzz labels | labels=type/feature-request, area/SkiaSharp.HarfBuzz, tenet/compatibility |
| add-comment | medium | 0.85 (85%) | Acknowledge the request, confirm feasibility, and provide workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the feature request! You're right that there's no shaped equivalent to `SKFont.GetTextPath()` in `SkiaSharp.HarfBuzz`.

Until this is implemented, you can achieve the same result manually:

```csharp
using var shaper = new SKShaper(font.Typeface);
var result = shaper.Shape(text, x, y, font);
using var path = font.GetTextPath(text, result.Points);
```

The `SKShaper.Shape()` returns glyph positions after HarfBuzz shaping, and `SKFont.GetTextPath(string, ReadOnlySpan<SKPoint>)` accepts pre-positioned glyphs — so the two can be composed today.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2512,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T23:03:38Z"
  },
  "summary": "Feature request to add a GetShapedTextPath extension method to SkiaSharp.HarfBuzz that uses HarfBuzz shaping to return an SKPath, mirroring how DrawShapedText works.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.HarfBuzz",
      "confidence": 0.97
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "SKPath GetShapedTextPath(this SKFont font, SKShaper shaper, string text, float x, float y, SKTextAlign textAlign)"
      ],
      "repoLinks": []
    }
  },
  "analysis": {
    "summary": "The SkiaSharp.HarfBuzz package provides DrawShapedText extension methods (CanvasExtensions.cs) that use HarfBuzz for proper Unicode text shaping. However, there is no companion GetShapedTextPath method that would return an SKPath instead of drawing. SKFont already has GetTextPath and GetTextPath(with positions) methods, but they do not apply HarfBuzz shaping. A new CanvasExtensions (or FontExtensions) method in SkiaSharp.HarfBuzz would bridge the gap by running SKShaper.Shape() to get positioned glyphs and then calling SKFont.GetTextPath with those positions.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/CanvasExtensions.cs",
        "finding": "DrawShapedText overloads exist but no GetShapedTextPath equivalent. The core of DrawShapedText uses SKShaper.Shape() to get glyph codepoints and positions, then builds an SKTextBlob and calls canvas.DrawText. A GetShapedTextPath would follow the same shaping logic but call SKFont.GetTextPath with the positioned glyphs instead.",
        "relevance": "direct",
        "lines": "58-103"
      },
      {
        "file": "binding/SkiaSharp/SKFont.cs",
        "finding": "SKFont.GetTextPath(string text, ReadOnlySpan<SKPoint> positions) exists and accepts pre-positioned glyphs, which is exactly what SKShaper.Shape() produces. This makes implementation feasible without new C API changes.",
        "relevance": "direct",
        "lines": "758-778"
      },
      {
        "file": "source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/SKShaper.cs",
        "finding": "SKShaper.Shape() returns a Result with Codepoints and Points arrays, which represent glyph IDs and their 2D positions after HarfBuzz shaping. These can be passed directly to SKFont.GetTextPath.",
        "relevance": "direct",
        "lines": "54-95"
      }
    ],
    "keySignals": [
      {
        "text": "Currently, there is no shaped equivalent to SKPaint.GetTextPath(), which can be inconvenient in scenarios that utilize CanvasExtensions.DrawShapedText() and require drawing an outline around the text.",
        "source": "issue body",
        "interpretation": "Clear gap: shaping + path extraction combined is not available"
      },
      {
        "text": "An extension method for SKFont that returns the shaped path of a text input.",
        "source": "issue body",
        "interpretation": "Reporter wants the feature in SkiaSharp.HarfBuzz, mirroring DrawShapedText's approach"
      }
    ],
    "rationale": "This is a well-scoped feature request for SkiaSharp.HarfBuzz. The building blocks (SKShaper.Shape() + SKFont.GetTextPath with positions) already exist in the codebase; only the glue is missing. Implementation requires no C API changes. The request is reasonable and consistent with existing API patterns.",
    "workarounds": [
      "Manually call shaper.Shape(text, x, y, font) to get Result, then call font.GetTextPath(text, result.Points) using the shaped glyph positions."
    ],
    "resolution": {
      "hypothesis": "Add GetShapedTextPath extension methods to CanvasExtensions (or a new static class) in SkiaSharp.HarfBuzz, mirroring the DrawShapedText pattern: shape with SKShaper, then call SKFont.GetTextPath with positioned glyphs.",
      "proposals": [
        {
          "title": "Add GetShapedTextPath extension methods to SkiaSharp.HarfBuzz",
          "description": "Add static extension methods alongside CanvasExtensions that shape text with HarfBuzz and return an SKPath. Mirrors the overload set of DrawShapedText.",
          "category": "fix",
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Workaround: manually compose shaper result with GetTextPath",
          "description": "Call shaper.Shape(text, x, y, font) and pass result.Points to font.GetTextPath to achieve the same result today.",
          "category": "workaround",
          "effort": "cost/xs",
          "codeSnippet": "using var shaper = new SKShaper(font.Typeface);\nvar result = shaper.Shape(text, x, y, font);\n// Build positions array from shaped result\nvar positions = result.Points;\nusing var path = font.GetTextPath(text, positions);",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add GetShapedTextPath extension methods to SkiaSharp.HarfBuzz",
      "recommendedReason": "The feature is feasible with existing primitives, no C API changes needed, and follows established patterns in the codebase."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.88,
      "reason": "Valid, well-scoped feature request. The building blocks exist; implementation is straightforward but requires design review for API surface.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/feature-request and area/SkiaSharp.HarfBuzz labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp.HarfBuzz",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the request, confirm feasibility, and provide workaround",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the feature request! You're right that there's no shaped equivalent to `SKFont.GetTextPath()` in `SkiaSharp.HarfBuzz`.\n\nUntil this is implemented, you can achieve the same result manually:\n\n```csharp\nusing var shaper = new SKShaper(font.Typeface);\nvar result = shaper.Shape(text, x, y, font);\nusing var path = font.GetTextPath(text, result.Points);\n```\n\nThe `SKShaper.Shape()` returns glyph positions after HarfBuzz shaping, and `SKFont.GetTextPath(string, ReadOnlySpan<SKPoint>)` accepts pre-positioned glyphs — so the two can be composed today."
      }
    ]
  }
}
```

</details>
