# Issue Triage Report — #1407

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T18:55:00Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** Reporter describes that SKPaint.GetTextPath() returns a path at the given x/y coordinates without adjusting for paint.TextAlign, while SKCanvas.DrawText() adjusts the origin based on TextAlign. When TextAlign is Center or Right, the halo path rendered via DrawPath(GetTextPath(...)) is misaligned with the DrawText() output. Reported against SkiaSharp 2.80.1 on Windows; believed to affect all platforms.

**Analysis:** SKPaint.GetTextPath() does not apply TextAlign to the origin when computing the text outline path, while SKCanvas.DrawText() explicitly adjusts the x coordinate for Center/Right alignment. This mismatch means that when a developer uses GetTextPath() with a Center-aligned paint to draw a halo, the path is positioned as if TextAlign were Left. The root cause is that SKPaint.GetTextPath() simply delegates to SKFont.GetTextPath(), which calls sk_text_utils_get_path natively without alignment — alignment is not in scope for SKFont. By contrast, DrawText() explicitly computes font.MeasureText() and subtracts the offset. A workaround is to manually apply the translation after getting the path.

**Recommendations:** **needs-investigation** — Real behavior mismatch confirmed by code inspection. Workaround exists. Decision needed on whether to fix the legacy SKPaint.GetTextPath() wrapper before issue #3732 promotes it to an error, or just document the manual alignment pattern.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create SKPaint with TextAlign = SKTextAlign.Center
2. Call paint.GetTextPath("Test Text", 0, 0) to get path for halo rendering
3. Call canvas.DrawPath(path, haloPaint) to draw halo
4. Call canvas.DrawText("Test Text", 0, 0, paint) to draw text
5. Observe halo is left-aligned while text is center-aligned

**Environment:** SkiaSharp 2.80.1, Visual Studio, Windows (believed to affect all platforms)

**Screenshots:**
- https://user-images.githubusercontent.com/63121477/87454665-e8059600-c5c9-11ea-8426-81ec935e470e.png — Red text with blue halo showing alignment offset when TextAlign is Center

**Code snippets:**

```csharp
SKPaint paint = new SKPaint();
paint.TextAlign = SKTextAlign.Center;
// Draw Halo
canvas.DrawPath(paint.GetTextPath("Test Text", 0, 0), haloPaint);
// Draw text
canvas.DrawText("Test Text", 0, 0, paint);
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | Halo text path position doesn't match actual text position when paint.TextAlign != SKTextAlign.Left |
| Repro quality | complete |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.1, 1.68.3 |
| Worked in | 1.68.3 |
| Broke in | 2.80.1 |
| Current relevance | likely |
| Relevance reason | SKPaint.GetTextPath() is now marked [Obsolete] but still delegates to SKFont.GetTextPath() without any alignment adjustment. The behavior mismatch persists in current code. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.55 (55%) |
| Reason | Reporter states it worked in 1.68.3. The 1.x-to-2.x API refactoring moved text handling to SKFont, which does not apply alignment — causing the mismatch. Whether 1.68.3 applied alignment is unverified. |
| Worked in version | 1.68.3 |
| Broke in version | 2.80.1 |

## Analysis

### Technical Summary

SKPaint.GetTextPath() does not apply TextAlign to the origin when computing the text outline path, while SKCanvas.DrawText() explicitly adjusts the x coordinate for Center/Right alignment. This mismatch means that when a developer uses GetTextPath() with a Center-aligned paint to draw a halo, the path is positioned as if TextAlign were Left. The root cause is that SKPaint.GetTextPath() simply delegates to SKFont.GetTextPath(), which calls sk_text_utils_get_path natively without alignment — alignment is not in scope for SKFont. By contrast, DrawText() explicitly computes font.MeasureText() and subtracts the offset. A workaround is to manually apply the translation after getting the path.

### Rationale

The behavior mismatch is confirmed by code inspection: SKCanvas.DrawText() at lines 661-665 explicitly adjusts x for non-Left alignment, while SKPaint.GetTextPath() at line 427-428 delegates to SKFont.GetTextPath() without any adjustment. The reporter's screenshot visually confirms the offset. A CONTRIBUTOR comment already explains the architectural reason and provides a valid workaround. The [Obsolete] SKPaint.GetTextPath() path still exists in the codebase and issue #3732 tracks deprecation. The inconsistency is real and worth tracking even as the API is deprecated.

### Key Signals

- "paint.GetTextPath() doesn't respect paint.TextAlign property" — **issue title** (Direct API inconsistency between GetTextPath and DrawText)
- "the halo was off because the text Align is Center, but Halo seems still using TextAlign.Left" — **issue body** (Clear description of alignment mismatch with screenshot evidence)
- "I think SKPaint.GetTextPath should be deprecated...Neither SKFont nor that native SkTextUtils::GetPath have alignment" — **comment by ziriax (CONTRIBUTOR)** (Architectural explanation: alignment was intentionally not included in SKFont.GetTextPath — it is the caller's responsibility)
- "You can align the path yourself with e.g. path.Transform(SKMatrix.CreateTranslation(-path.Bounds.Width / 2f, 0))" — **comment by ziriax (CONTRIBUTOR)** (A working workaround already exists and was positively received (+1 reaction))

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 652-673 | direct | DrawText(string, float, float, SKTextAlign, SKFont, SKPaint) explicitly adjusts x for non-Left alignment: computes font.MeasureText(text) and subtracts full width (Right) or half width (Center). This is why DrawText positions correctly while GetTextPath does not. |
| `binding/SkiaSharp/SKPaint.cs` | 426-428 | direct | SKPaint.GetTextPath(string, float, float) is marked [Obsolete] and delegates directly to GetFont().GetTextPath(text, new SKPoint(x, y)) with no alignment adjustment applied. |
| `binding/SkiaSharp/SKFont.cs` | 746-754 | related | SKFont.GetTextPath() calls native sk_text_utils_get_path with the given origin coordinates; no alignment offset is applied at any layer. |

### Workarounds

- For Center alignment: var path = paint.GetTextPath(text, x, y); path.Transform(SKMatrix.CreateTranslation(-paint.MeasureText(text) / 2f, 0));
- For Right alignment: var path = paint.GetTextPath(text, x, y); path.Transform(SKMatrix.CreateTranslation(-paint.MeasureText(text), 0));
- Alternatively: compute the alignment offset yourself and pass adjusted coordinates to GetTextPath — e.g., for Center pass x - (width/2) as the x argument.

### Next Questions

- Should the obsolete SKPaint.GetTextPath() be patched to apply TextAlign before it is promoted to [Obsolete(error:true)] in issue #3732?
- Should SKFont.GetTextPath() offer an overload accepting a textAlign parameter for convenience?

### Resolution Proposals

**Hypothesis:** SKFont.GetTextPath() intentionally omits alignment (alignment is a canvas-level concern). SKPaint.GetTextPath() inherited this behavior when it was refactored to delegate to SKFont. The fix is either to apply alignment in the legacy SKPaint wrapper, or document and enforce the manual-translation pattern.

1. **Apply manual translation after GetTextPath** — workaround, confidence 0.92 (92%), cost/xs, validated=yes
   - Use the existing (obsolete) or new SKFont.GetTextPath(), then apply a SKMatrix.CreateTranslation() to correct for alignment. Mirrors exactly what DrawText() does internally.

```csharp
// Get the path
var path = paint.GetTextPath("Test Text", x, y);
// Apply alignment offset to match DrawText behavior
float width = paint.MeasureText("Test Text");
if (paint.TextAlign == SKTextAlign.Center)
    path.Transform(SKMatrix.CreateTranslation(-width / 2f, 0));
else if (paint.TextAlign == SKTextAlign.Right)
    path.Transform(SKMatrix.CreateTranslation(-width, 0));
```
2. **Fix SKPaint.GetTextPath to apply alignment before deprecation error** — fix, confidence 0.72 (72%), cost/s, validated=untested
   - Update the obsolete SKPaint.GetTextPath(string, float, float) overloads to apply TextAlign adjustment (matching DrawText behavior) before issue #3732 promotes them to [Obsolete(error: true)].

**Recommended proposal:** Apply manual translation after GetTextPath

**Why:** Immediate workaround using existing public APIs, no code changes required. Mirrors the exact logic used internally by DrawText. The code fix depends on #3732 timeline decisions.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Real behavior mismatch confirmed by code inspection. Workaround exists. Decision needed on whether to fix the legacy SKPaint.GetTextPath() wrapper before issue #3732 promotes it to an error, or just document the manual alignment pattern. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, core API area, and compatibility tenet labels | labels=type/bug, area/SkiaSharp, tenet/compatibility |
| add-comment | medium | 0.90 (90%) | Post analysis with workaround for immediate use | — |
| link-related | low | 0.85 (85%) | Link to #3732 which tracks SKPaint text API deprecation | linkedIssue=#3732 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and screenshot.

This is a confirmed inconsistency: `SKCanvas.DrawText()` adjusts the x origin based on `TextAlign`, but `SKPaint.GetTextPath()` (and the underlying `SKFont.GetTextPath()`) does not — alignment is the caller's responsibility when working with paths.

As a workaround, apply the alignment offset manually after getting the path:

```csharp
var path = paint.GetTextPath("Test Text", x, y);
float width = paint.MeasureText("Test Text");
if (paint.TextAlign == SKTextAlign.Center)
    path.Transform(SKMatrix.CreateTranslation(-width / 2f, 0));
else if (paint.TextAlign == SKTextAlign.Right)
    path.Transform(SKMatrix.CreateTranslation(-width, 0));
```

Note: `SKPaint.GetTextPath()` is now marked `[Obsolete]` — the modern replacement is `SKFont.GetTextPath()`. See also #3732 which tracks the migration of text APIs from `SKPaint` to `SKFont`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1407,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T18:55:00Z"
  },
  "summary": "Reporter describes that SKPaint.GetTextPath() returns a path at the given x/y coordinates without adjusting for paint.TextAlign, while SKCanvas.DrawText() adjusts the origin based on TextAlign. When TextAlign is Center or Right, the halo path rendered via DrawPath(GetTextPath(...)) is misaligned with the DrawText() output. Reported against SkiaSharp 2.80.1 on Windows; believed to affect all platforms.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "Halo text path position doesn't match actual text position when paint.TextAlign != SKTextAlign.Left",
      "reproQuality": "complete"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create SKPaint with TextAlign = SKTextAlign.Center",
        "Call paint.GetTextPath(\"Test Text\", 0, 0) to get path for halo rendering",
        "Call canvas.DrawPath(path, haloPaint) to draw halo",
        "Call canvas.DrawText(\"Test Text\", 0, 0, paint) to draw text",
        "Observe halo is left-aligned while text is center-aligned"
      ],
      "codeSnippets": [
        "SKPaint paint = new SKPaint();\npaint.TextAlign = SKTextAlign.Center;\n// Draw Halo\ncanvas.DrawPath(paint.GetTextPath(\"Test Text\", 0, 0), haloPaint);\n// Draw text\ncanvas.DrawText(\"Test Text\", 0, 0, paint);"
      ],
      "screenshots": [
        {
          "url": "https://user-images.githubusercontent.com/63121477/87454665-e8059600-c5c9-11ea-8426-81ec935e470e.png",
          "description": "Red text with blue halo showing alignment offset when TextAlign is Center"
        }
      ],
      "environmentDetails": "SkiaSharp 2.80.1, Visual Studio, Windows (believed to affect all platforms)"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.1",
        "1.68.3"
      ],
      "workedIn": "1.68.3",
      "brokeIn": "2.80.1",
      "currentRelevance": "likely",
      "relevanceReason": "SKPaint.GetTextPath() is now marked [Obsolete] but still delegates to SKFont.GetTextPath() without any alignment adjustment. The behavior mismatch persists in current code."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.55,
      "reason": "Reporter states it worked in 1.68.3. The 1.x-to-2.x API refactoring moved text handling to SKFont, which does not apply alignment — causing the mismatch. Whether 1.68.3 applied alignment is unverified.",
      "workedInVersion": "1.68.3",
      "brokeInVersion": "2.80.1"
    }
  },
  "analysis": {
    "summary": "SKPaint.GetTextPath() does not apply TextAlign to the origin when computing the text outline path, while SKCanvas.DrawText() explicitly adjusts the x coordinate for Center/Right alignment. This mismatch means that when a developer uses GetTextPath() with a Center-aligned paint to draw a halo, the path is positioned as if TextAlign were Left. The root cause is that SKPaint.GetTextPath() simply delegates to SKFont.GetTextPath(), which calls sk_text_utils_get_path natively without alignment — alignment is not in scope for SKFont. By contrast, DrawText() explicitly computes font.MeasureText() and subtracts the offset. A workaround is to manually apply the translation after getting the path.",
    "rationale": "The behavior mismatch is confirmed by code inspection: SKCanvas.DrawText() at lines 661-665 explicitly adjusts x for non-Left alignment, while SKPaint.GetTextPath() at line 427-428 delegates to SKFont.GetTextPath() without any adjustment. The reporter's screenshot visually confirms the offset. A CONTRIBUTOR comment already explains the architectural reason and provides a valid workaround. The [Obsolete] SKPaint.GetTextPath() path still exists in the codebase and issue #3732 tracks deprecation. The inconsistency is real and worth tracking even as the API is deprecated.",
    "keySignals": [
      {
        "text": "paint.GetTextPath() doesn't respect paint.TextAlign property",
        "source": "issue title",
        "interpretation": "Direct API inconsistency between GetTextPath and DrawText"
      },
      {
        "text": "the halo was off because the text Align is Center, but Halo seems still using TextAlign.Left",
        "source": "issue body",
        "interpretation": "Clear description of alignment mismatch with screenshot evidence"
      },
      {
        "text": "I think SKPaint.GetTextPath should be deprecated...Neither SKFont nor that native SkTextUtils::GetPath have alignment",
        "source": "comment by ziriax (CONTRIBUTOR)",
        "interpretation": "Architectural explanation: alignment was intentionally not included in SKFont.GetTextPath — it is the caller's responsibility"
      },
      {
        "text": "You can align the path yourself with e.g. path.Transform(SKMatrix.CreateTranslation(-path.Bounds.Width / 2f, 0))",
        "source": "comment by ziriax (CONTRIBUTOR)",
        "interpretation": "A working workaround already exists and was positively received (+1 reaction)"
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "652-673",
        "finding": "DrawText(string, float, float, SKTextAlign, SKFont, SKPaint) explicitly adjusts x for non-Left alignment: computes font.MeasureText(text) and subtracts full width (Right) or half width (Center). This is why DrawText positions correctly while GetTextPath does not.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "lines": "426-428",
        "finding": "SKPaint.GetTextPath(string, float, float) is marked [Obsolete] and delegates directly to GetFont().GetTextPath(text, new SKPoint(x, y)) with no alignment adjustment applied.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFont.cs",
        "lines": "746-754",
        "finding": "SKFont.GetTextPath() calls native sk_text_utils_get_path with the given origin coordinates; no alignment offset is applied at any layer.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "For Center alignment: var path = paint.GetTextPath(text, x, y); path.Transform(SKMatrix.CreateTranslation(-paint.MeasureText(text) / 2f, 0));",
      "For Right alignment: var path = paint.GetTextPath(text, x, y); path.Transform(SKMatrix.CreateTranslation(-paint.MeasureText(text), 0));",
      "Alternatively: compute the alignment offset yourself and pass adjusted coordinates to GetTextPath — e.g., for Center pass x - (width/2) as the x argument."
    ],
    "nextQuestions": [
      "Should the obsolete SKPaint.GetTextPath() be patched to apply TextAlign before it is promoted to [Obsolete(error:true)] in issue #3732?",
      "Should SKFont.GetTextPath() offer an overload accepting a textAlign parameter for convenience?"
    ],
    "resolution": {
      "hypothesis": "SKFont.GetTextPath() intentionally omits alignment (alignment is a canvas-level concern). SKPaint.GetTextPath() inherited this behavior when it was refactored to delegate to SKFont. The fix is either to apply alignment in the legacy SKPaint wrapper, or document and enforce the manual-translation pattern.",
      "proposals": [
        {
          "title": "Apply manual translation after GetTextPath",
          "description": "Use the existing (obsolete) or new SKFont.GetTextPath(), then apply a SKMatrix.CreateTranslation() to correct for alignment. Mirrors exactly what DrawText() does internally.",
          "category": "workaround",
          "codeSnippet": "// Get the path\nvar path = paint.GetTextPath(\"Test Text\", x, y);\n// Apply alignment offset to match DrawText behavior\nfloat width = paint.MeasureText(\"Test Text\");\nif (paint.TextAlign == SKTextAlign.Center)\n    path.Transform(SKMatrix.CreateTranslation(-width / 2f, 0));\nelse if (paint.TextAlign == SKTextAlign.Right)\n    path.Transform(SKMatrix.CreateTranslation(-width, 0));",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Fix SKPaint.GetTextPath to apply alignment before deprecation error",
          "description": "Update the obsolete SKPaint.GetTextPath(string, float, float) overloads to apply TextAlign adjustment (matching DrawText behavior) before issue #3732 promotes them to [Obsolete(error: true)].",
          "category": "fix",
          "confidence": 0.72,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Apply manual translation after GetTextPath",
      "recommendedReason": "Immediate workaround using existing public APIs, no code changes required. Mirrors the exact logic used internally by DrawText. The code fix depends on #3732 timeline decisions."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Real behavior mismatch confirmed by code inspection. Workaround exists. Decision needed on whether to fix the legacy SKPaint.GetTextPath() wrapper before issue #3732 promotes it to an error, or just document the manual alignment pattern.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, core API area, and compatibility tenet labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis with workaround for immediate use",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the detailed report and screenshot.\n\nThis is a confirmed inconsistency: `SKCanvas.DrawText()` adjusts the x origin based on `TextAlign`, but `SKPaint.GetTextPath()` (and the underlying `SKFont.GetTextPath()`) does not — alignment is the caller's responsibility when working with paths.\n\nAs a workaround, apply the alignment offset manually after getting the path:\n\n```csharp\nvar path = paint.GetTextPath(\"Test Text\", x, y);\nfloat width = paint.MeasureText(\"Test Text\");\nif (paint.TextAlign == SKTextAlign.Center)\n    path.Transform(SKMatrix.CreateTranslation(-width / 2f, 0));\nelse if (paint.TextAlign == SKTextAlign.Right)\n    path.Transform(SKMatrix.CreateTranslation(-width, 0));\n```\n\nNote: `SKPaint.GetTextPath()` is now marked `[Obsolete]` — the modern replacement is `SKFont.GetTextPath()`. See also #3732 which tracks the migration of text APIs from `SKPaint` to `SKFont`."
      },
      {
        "type": "link-related",
        "description": "Link to #3732 which tracks SKPaint text API deprecation",
        "risk": "low",
        "confidence": 0.85,
        "linkedIssue": 3732
      }
    ]
  }
}
```

</details>
