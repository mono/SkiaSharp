# Issue Triage Report — #3130

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:20:00Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | needs-investigation (0.90 (90%)) |

**Issue Summary:** SKPaint.IsAntialias = false no longer disables text antialiasing in DrawText since SkiaSharp 3.116.0, because text rendering now uses SKFont.Edging rather than SKPaint.IsAntialias.

**Analysis:** In SkiaSharp 3.116, text rendering was migrated to use the Skia modern font API (SKFont). Text antialiasing is now controlled by SKFont.Edging (SKFontEdging.Alias disables it), not by SKPaint.IsAntialias. The SKPaint.IsAntialias setter calls sk_compatpaint_set_is_antialias which updates the paint's antialias flag for shapes and fills, but does NOT propagate to the SKFont.Edging property of the associated font. The old DrawText(string, float, float, SKPaint) overload delegates to DrawText(text, x, y, textAlign, paint.GetFont(), paint), using the font from the compat paint, which still has the default edging (antialiased), so IsAntialias = false has no effect on text.

**Recommendations:** **needs-investigation** — Clear regression with complete repro and screenshots. Root cause identified in code: sk_compatpaint_set_is_antialias doesn't propagate to SKFont.Edging. Needs a fix in the native C API or the C# compatibility layer.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create an SKBitmap/SKCanvas
2. Create SKPaint with IsAntialias = false
3. Call canvas.DrawText(text, x, y, paint)
4. Observe that text is still rendered with antialiasing (same as IsAntialias = true)

**Environment:** Windows 11, Visual Studio, SkiaSharp 3.116.0+

**Repository links:**
- https://github.com/meridaio/SkiaSharpTextAntialiasing — Minimal repro project demonstrating broken and working versions

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | — |
| Repro quality | complete |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 3.116.1, 2.88.9, 3.119.0 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | likely |
| Relevance reason | A community comment confirms the bug is still present in 3.119.0. The root cause (SKFont.Edging not updated by sk_compatpaint_set_is_antialias) has not been addressed. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.97 (97%) |
| Reason | Reporter provides screenshot comparisons showing correct behavior in 2.88.9 and broken behavior starting at 3.116.0. Community commenter confirms still broken in 3.119.0. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.0 |

## Analysis

### Technical Summary

In SkiaSharp 3.116, text rendering was migrated to use the Skia modern font API (SKFont). Text antialiasing is now controlled by SKFont.Edging (SKFontEdging.Alias disables it), not by SKPaint.IsAntialias. The SKPaint.IsAntialias setter calls sk_compatpaint_set_is_antialias which updates the paint's antialias flag for shapes and fills, but does NOT propagate to the SKFont.Edging property of the associated font. The old DrawText(string, float, float, SKPaint) overload delegates to DrawText(text, x, y, textAlign, paint.GetFont(), paint), using the font from the compat paint, which still has the default edging (antialiased), so IsAntialias = false has no effect on text.

### Rationale

High confidence type/bug regression: reporter provides visual proof and a minimal repro; community confirms it is still present in 3.119.0. The root cause is that sk_compatpaint_set_is_antialias does not propagate to SKFont.Edging when false. Area is area/SkiaSharp (core bindings). Platform set to os/Windows-Classic since that is the only reported platform, though the bug may also affect other platforms as it is a core API issue. tenet/compatibility because SkiaSharp 3.x broke a 2.x API contract.

### Key Signals

- "Prior to 3.116.0, setting this property to false produces [alias rendering] ... On versions 3.116.0 and 3.116.1, the image produced when IsAntialias is false now looks like [still antialiased]" — **issue body** (Clear before/after regression with screenshots — IsAntialias stopped working for text in 3.116.0)
- "This is still an issue in 3.119.0. Setting SKFont.Edging to SKFontEdging.Alias appears to have the desired effect." — **comment by jswolf19** (Confirms regression is still present in latest release and provides the workaround via the new API)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPaint.cs` | 82-85 | direct | SKPaint.IsAntialias getter uses sk_paint_is_antialias; setter uses sk_compatpaint_set_is_antialias. Neither path updates SKFont.Edging. |
| `binding/SkiaSharp/SKCanvas.cs` | 618-624 | direct | The obsolete DrawText(string, float, float, SKPaint) overload delegates to DrawText(text, x, y, paint.TextAlign, paint.GetFont(), paint) — text rendering uses SKFont from the compat paint. |
| `binding/SkiaSharp/SKCanvas.cs` | 639-660 | direct | DrawText(string, float, float, SKTextAlign, SKFont, SKPaint) creates SKTextBlob from font (not paint), then calls DrawText(blob, x, y, paint). The font's Edging controls antialiasing; paint's IsAntialias flag does not affect text. |
| `binding/SkiaSharp/SKFont.cs` | 67-70 | direct | SKFont.Edging property maps directly to sk_font_get_edging/sk_font_set_edging. This is the correct API to control text antialiasing in the modern Skia API. |
| `binding/SkiaSharp/SKPaint.cs` | 825-827 | related | GetFont() retrieves (or caches) the SKFont from the compat paint via sk_compatpaint_get_font. This font is what DrawText uses for glyph rendering. |

### Workarounds

- Set SKFont.Edging = SKFontEdging.Alias on the font passed to DrawText to disable antialiasing instead of SKPaint.IsAntialias = false
- Use the explicit SKFont overload: canvas.DrawText(text, x, y, new SKFont { Edging = SKFontEdging.Alias }, paint)

### Next Questions

- Does sk_compatpaint_set_is_antialias update SKFont.Edging in the native C API implementation?
- Should the SKPaint.IsAntialias setter also set font.Edging = SKFontEdging.Alias when value is false (and AntiAlias/SubpixelAntialias when true)?
- Does this regression affect other platforms (Linux, macOS, Android, iOS)?

### Resolution Proposals

**Hypothesis:** sk_compatpaint_set_is_antialias should also update SKFont.Edging on the associated font: when antialias=false, set Edging to SKFontEdging.Alias; when true, restore to SKFontEdging.AntiAlias or SubpixelAntiAlias.

1. **Use SKFont.Edging directly** — workaround, cost/xs, validated=yes
   - Set SKFont.Edging = SKFontEdging.Alias on the font used for drawing text to disable antialiasing.

```csharp
// Instead of: paint.IsAntialias = false;
// Do this:
var font = new SKFont { Edging = SKFontEdging.Alias, Size = 24 };
canvas.DrawText("Hello", x, y, font, paint);

// Or if using the compat API, modify the paint's internal font:
paint.GetFont(); // Not public — use the explicit font overload instead
```
2. **Fix sk_compatpaint_set_is_antialias to propagate to SKFont.Edging** — fix, cost/s, validated=untested
   - Update the native sk_compatpaint_set_is_antialias implementation (or the C# SKPaint.IsAntialias setter) to also update the compat paint's internal SKFont.Edging: SKFontEdging.Alias when antialias=false, SKFontEdging.AntiAlias when antialias=true. This would restore the pre-3.116 behavior.

**Recommended proposal:** workaround-1

**Why:** The workaround is confirmed by a community commenter and requires only switching to the new explicit SKFont API. The fix requires native code changes in the skia submodule.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.90 (90%) |
| Reason | Clear regression with complete repro and screenshots. Root cause identified in code: sk_compatpaint_set_is_antialias doesn't propagate to SKFont.Edging. Needs a fix in the native C API or the C# compatibility layer. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply classification labels: area/SkiaSharp, os/Windows-Classic, tenet/compatibility | labels=area/SkiaSharp, os/Windows-Classic, tenet/compatibility |
| add-comment | medium | 0.90 (90%) | Acknowledge regression, explain root cause, and provide SKFont.Edging workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and screenshots — this is a confirmed regression.

Here's a workaround you can use while we investigate:

```csharp
// Instead of:
var paint = new SKPaint { IsAntialias = false };
canvas.DrawText(text, x, y, paint);

// Use SKFont.Edging directly:
var font = new SKFont
{
    Typeface = SKTypeface.Default,
    Size = 24,
    Edging = SKFontEdging.Alias  // disables antialiasing
};
var paint = new SKPaint();
canvas.DrawText(text, x, y, font, paint);
```

This was also confirmed by @jswolf19 in the comments above.

The root cause: in SkiaSharp 3.116, text rendering was migrated to use the Skia modern font API (`SKFont`). Text antialiasing is now controlled by `SKFont.Edging`, not `SKPaint.IsAntialias`. The `IsAntialias` setter calls into the native compat paint layer but doesn't propagate to the internal font's `Edging` property, so text continues to render with the default antialiased edging regardless of the paint flag.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3130,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:20:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKPaint.IsAntialias = false no longer disables text antialiasing in DrawText since SkiaSharp 3.116.0, because text rendering now uses SKFont.Edging rather than SKPaint.IsAntialias.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
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
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "reproQuality": "complete",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKBitmap/SKCanvas",
        "Create SKPaint with IsAntialias = false",
        "Call canvas.DrawText(text, x, y, paint)",
        "Observe that text is still rendered with antialiasing (same as IsAntialias = true)"
      ],
      "environmentDetails": "Windows 11, Visual Studio, SkiaSharp 3.116.0+",
      "repoLinks": [
        {
          "url": "https://github.com/meridaio/SkiaSharpTextAntialiasing",
          "description": "Minimal repro project demonstrating broken and working versions"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "3.116.1",
        "2.88.9",
        "3.119.0"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.116.0",
      "currentRelevance": "likely",
      "relevanceReason": "A community comment confirms the bug is still present in 3.119.0. The root cause (SKFont.Edging not updated by sk_compatpaint_set_is_antialias) has not been addressed."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.97,
      "reason": "Reporter provides screenshot comparisons showing correct behavior in 2.88.9 and broken behavior starting at 3.116.0. Community commenter confirms still broken in 3.119.0.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.0"
    }
  },
  "analysis": {
    "summary": "In SkiaSharp 3.116, text rendering was migrated to use the Skia modern font API (SKFont). Text antialiasing is now controlled by SKFont.Edging (SKFontEdging.Alias disables it), not by SKPaint.IsAntialias. The SKPaint.IsAntialias setter calls sk_compatpaint_set_is_antialias which updates the paint's antialias flag for shapes and fills, but does NOT propagate to the SKFont.Edging property of the associated font. The old DrawText(string, float, float, SKPaint) overload delegates to DrawText(text, x, y, textAlign, paint.GetFont(), paint), using the font from the compat paint, which still has the default edging (antialiased), so IsAntialias = false has no effect on text.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "lines": "82-85",
        "finding": "SKPaint.IsAntialias getter uses sk_paint_is_antialias; setter uses sk_compatpaint_set_is_antialias. Neither path updates SKFont.Edging.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "618-624",
        "finding": "The obsolete DrawText(string, float, float, SKPaint) overload delegates to DrawText(text, x, y, paint.TextAlign, paint.GetFont(), paint) — text rendering uses SKFont from the compat paint.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "639-660",
        "finding": "DrawText(string, float, float, SKTextAlign, SKFont, SKPaint) creates SKTextBlob from font (not paint), then calls DrawText(blob, x, y, paint). The font's Edging controls antialiasing; paint's IsAntialias flag does not affect text.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFont.cs",
        "lines": "67-70",
        "finding": "SKFont.Edging property maps directly to sk_font_get_edging/sk_font_set_edging. This is the correct API to control text antialiasing in the modern Skia API.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "lines": "825-827",
        "finding": "GetFont() retrieves (or caches) the SKFont from the compat paint via sk_compatpaint_get_font. This font is what DrawText uses for glyph rendering.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Prior to 3.116.0, setting this property to false produces [alias rendering] ... On versions 3.116.0 and 3.116.1, the image produced when IsAntialias is false now looks like [still antialiased]",
        "source": "issue body",
        "interpretation": "Clear before/after regression with screenshots — IsAntialias stopped working for text in 3.116.0"
      },
      {
        "text": "This is still an issue in 3.119.0. Setting SKFont.Edging to SKFontEdging.Alias appears to have the desired effect.",
        "source": "comment by jswolf19",
        "interpretation": "Confirms regression is still present in latest release and provides the workaround via the new API"
      }
    ],
    "rationale": "High confidence type/bug regression: reporter provides visual proof and a minimal repro; community confirms it is still present in 3.119.0. The root cause is that sk_compatpaint_set_is_antialias does not propagate to SKFont.Edging when false. Area is area/SkiaSharp (core bindings). Platform set to os/Windows-Classic since that is the only reported platform, though the bug may also affect other platforms as it is a core API issue. tenet/compatibility because SkiaSharp 3.x broke a 2.x API contract.",
    "workarounds": [
      "Set SKFont.Edging = SKFontEdging.Alias on the font passed to DrawText to disable antialiasing instead of SKPaint.IsAntialias = false",
      "Use the explicit SKFont overload: canvas.DrawText(text, x, y, new SKFont { Edging = SKFontEdging.Alias }, paint)"
    ],
    "nextQuestions": [
      "Does sk_compatpaint_set_is_antialias update SKFont.Edging in the native C API implementation?",
      "Should the SKPaint.IsAntialias setter also set font.Edging = SKFontEdging.Alias when value is false (and AntiAlias/SubpixelAntialias when true)?",
      "Does this regression affect other platforms (Linux, macOS, Android, iOS)?"
    ],
    "resolution": {
      "hypothesis": "sk_compatpaint_set_is_antialias should also update SKFont.Edging on the associated font: when antialias=false, set Edging to SKFontEdging.Alias; when true, restore to SKFontEdging.AntiAlias or SubpixelAntiAlias.",
      "proposals": [
        {
          "title": "Use SKFont.Edging directly",
          "category": "workaround",
          "effort": "cost/xs",
          "validated": "yes",
          "description": "Set SKFont.Edging = SKFontEdging.Alias on the font used for drawing text to disable antialiasing.",
          "codeSnippet": "// Instead of: paint.IsAntialias = false;\n// Do this:\nvar font = new SKFont { Edging = SKFontEdging.Alias, Size = 24 };\ncanvas.DrawText(\"Hello\", x, y, font, paint);\n\n// Or if using the compat API, modify the paint's internal font:\npaint.GetFont(); // Not public — use the explicit font overload instead"
        },
        {
          "title": "Fix sk_compatpaint_set_is_antialias to propagate to SKFont.Edging",
          "category": "fix",
          "effort": "cost/s",
          "validated": "untested",
          "description": "Update the native sk_compatpaint_set_is_antialias implementation (or the C# SKPaint.IsAntialias setter) to also update the compat paint's internal SKFont.Edging: SKFontEdging.Alias when antialias=false, SKFontEdging.AntiAlias when antialias=true. This would restore the pre-3.116 behavior."
        }
      ],
      "recommendedProposal": "workaround-1",
      "recommendedReason": "The workaround is confirmed by a community commenter and requires only switching to the new explicit SKFont API. The fix requires native code changes in the skia submodule."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.9,
      "reason": "Clear regression with complete repro and screenshots. Root cause identified in code: sk_compatpaint_set_is_antialias doesn't propagate to SKFont.Edging. Needs a fix in the native C API or the C# compatibility layer.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply classification labels: area/SkiaSharp, os/Windows-Classic, tenet/compatibility",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge regression, explain root cause, and provide SKFont.Edging workaround",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the detailed report and screenshots — this is a confirmed regression.\n\nHere's a workaround you can use while we investigate:\n\n```csharp\n// Instead of:\nvar paint = new SKPaint { IsAntialias = false };\ncanvas.DrawText(text, x, y, paint);\n\n// Use SKFont.Edging directly:\nvar font = new SKFont\n{\n    Typeface = SKTypeface.Default,\n    Size = 24,\n    Edging = SKFontEdging.Alias  // disables antialiasing\n};\nvar paint = new SKPaint();\ncanvas.DrawText(text, x, y, font, paint);\n```\n\nThis was also confirmed by @jswolf19 in the comments above.\n\nThe root cause: in SkiaSharp 3.116, text rendering was migrated to use the Skia modern font API (`SKFont`). Text antialiasing is now controlled by `SKFont.Edging`, not `SKPaint.IsAntialias`. The `IsAntialias` setter calls into the native compat paint layer but doesn't propagate to the internal font's `Edging` property, so text continues to render with the default antialiased edging regardless of the paint flag."
      }
    ]
  }
}
```

</details>
