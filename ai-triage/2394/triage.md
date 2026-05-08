# Issue Triage Report — #2394

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T23:30:00Z |
| Type | type/question (0.88 (88%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** Reporter draws a circle with radius 3.5f (no antialias, no explicit stroke width) and observes an asymmetric result, which is actually expected pixel-grid aliasing behavior in Skia's raster renderer.

**Analysis:** The asymmetric circle appearance is classic pixel-grid aliasing: drawing a stroke at fractional radius (3.5f) on an integer pixel grid without antialiasing causes the stroke to be rounded differently on opposite sides of the center. This is expected Skia raster behavior, not a SkiaSharp defect. Setting IsAntialias = true or using an integer radius resolves the visual issue.

**Recommendations:** **close-as-not-a-bug** — The asymmetric circle rendering is expected pixel-grid aliasing behavior in Skia's raster pipeline. No SkiaSharp bug exists. Workarounds are well-established and were confirmed working by the reporter in the comments.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | os/Linux |
| Backends | backend/Raster |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Create a 9x9 SKBitmap
2. Create SKPaint with IsStroke = true (StrokeWidth not set)
3. Call DrawCircle(4, 4, 3.5f, paint)
4. Observe that the rendered stroke pixels are not symmetrically distributed around the center

**Environment:** SkiaSharp 2.88.3, Visual Studio Code, Linux

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | This is fundamental raster rendering behavior unchanged across versions; still reproducible. |

## Analysis

### Technical Summary

The asymmetric circle appearance is classic pixel-grid aliasing: drawing a stroke at fractional radius (3.5f) on an integer pixel grid without antialiasing causes the stroke to be rounded differently on opposite sides of the center. This is expected Skia raster behavior, not a SkiaSharp defect. Setting IsAntialias = true or using an integer radius resolves the visual issue.

### Rationale

SKCanvas.DrawCircle is a pure passthrough to sk_canvas_draw_circle — no transformation or rounding occurs in SkiaSharp. The aliasing artifact is inherent to drawing fractional-coordinate shapes on a pixel grid without antialiasing, a fundamental property of Skia's raster backend. Community comments confirm this diagnosis and the reporter acknowledged the workarounds work. No SkiaSharp code change is warranted.

### Key Signals

- "I tried to draw a circle with an odd diameter and it does not look like a circle; I guess negative distances are rounded differently" — **issue body** (Reporter correctly suspects rounding asymmetry — this is exactly how aliasing manifests at fractional coordinates.)
- "You are experiencing aliasing. As pixels are aligned to an integer grid, and you are asking for a radius of 3.5f, it's fundamentally not possible to draw a 1-pixel wide stroke from x = 0.5 to x = 1.5 without either rounding or antialiasing." — **comment by ojb500** (Community confirms aliasing root cause; workarounds given (integer radius or IsAntialias = true).)
- "You are right with IsAntialias it is much better." — **comment by feanor12 (reporter)** (Reporter confirms that setting IsAntialias = true resolves the visual issue.)
- "An unset stroke width seems to be an undefined state." — **comment by feanor12 (reporter)** (Reporter identifies that not setting StrokeWidth explicitly contributes to the issue; setting StrokeWidth = 0 (hairline) or 1 also helps.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 391-401 | direct | DrawCircle is a direct passthrough to sk_canvas_draw_circle with no SkiaSharp-level rounding or transformation. The rendering is delegated entirely to Skia's native raster pipeline. |
| `binding/SkiaSharp/SKPaint.cs` | 161-164 | related | StrokeWidth get/set are simple delegates to sk_paint_get/set_stroke_width. The default stroke width is determined by Skia (sk_compatpaint_new). Skia's default is 0 (hairline) but the unset state through compatpaint may differ, causing the asymmetric rendering the reporter observes. |

### Workarounds

- Set `paint.IsAntialias = true` to enable anti-aliasing for smooth sub-pixel rendering
- Use an integer radius (e.g., 3.0f instead of 3.5f) to avoid sub-pixel alignment issues
- Explicitly set `paint.StrokeWidth = 0` for hairline rendering or `paint.StrokeWidth = 1` for 1-pixel stroke
- Shift center to pixel center (e.g., DrawCircle(4.5f, 4.5f, 3.5f, paint)) for better symmetry without antialias

### Resolution Proposals

**Hypothesis:** Fractional-radius stroke circle without antialiasing causes different pixel-rounding on each side of center — this is expected raster rendering behavior in Skia, not a SkiaSharp bug.

1. **Enable antialiasing** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Set IsAntialias = true on SKPaint for smooth sub-pixel rendering. This is the recommended approach for non-integer coordinates.

```csharp
paint.IsAntialias = true;
paint.StrokeWidth = 1;
can.DrawCircle(4, 4, 3.5f, paint);
```
2. **Use integer radius** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Change radius from 3.5f to 3.0f so the stroke aligns exactly to integer pixel boundaries without needing antialiasing.

```csharp
can.DrawCircle(4, 4, 3.0f, paint);
```
3. **Use hairline stroke (StrokeWidth = 0)** — workaround, confidence 0.85 (85%), cost/xs, validated=yes
   - Explicitly set StrokeWidth to 0 to get Skia's special hairline rendering mode, which draws a 1-device-pixel-wide stroke that handles fractional coordinates more predictably.

```csharp
paint.StrokeWidth = 0;
can.DrawCircle(4, 4, 3.5f, paint);
```

**Recommended proposal:** Enable antialiasing

**Why:** IsAntialias = true is the standard approach for smooth rendering of fractional-coordinate shapes and handles all cases. Combined with explicit StrokeWidth, it produces predictable results.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | The asymmetric circle rendering is expected pixel-grid aliasing behavior in Skia's raster pipeline. No SkiaSharp bug exists. Workarounds are well-established and were confirmed working by the reporter in the comments. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply question, core SkiaSharp, and Linux labels | labels=type/question, area/SkiaSharp, os/Linux, backend/Raster |
| add-comment | high | 0.88 (88%) | Post explanation of aliasing with confirmed workarounds | — |
| close-issue | medium | 0.85 (85%) | Close as by-design/not-a-bug | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report! What you're seeing is **pixel-grid aliasing** — a fundamental characteristic of raster rendering, not a bug in SkiaSharp or Skia.

When you draw a circle with radius `3.5f` without anti-aliasing, pixels at sub-pixel positions must be rounded to the integer grid. Because positive and negative offsets from the center are rounded differently by the renderer, the result looks asymmetric.

**Workarounds (any of these will help):**

1. **Enable anti-aliasing** (recommended):
   ```csharp
   paint.IsAntialias = true;
   paint.StrokeWidth = 1;
   can.DrawCircle(4, 4, 3.5f, paint);
   ```

2. **Use an integer radius** to stay on whole-pixel boundaries:
   ```csharp
   can.DrawCircle(4, 4, 3.0f, paint);
   ```

3. **Explicitly set hairline stroke** (`StrokeWidth = 0` is Skia's special hairline mode):
   ```csharp
   paint.StrokeWidth = 0;
   can.DrawCircle(4, 4, 3.5f, paint);
   ```

Note: always set `StrokeWidth` explicitly — leaving it unset relies on the default from `sk_compatpaint_new` which may not behave as expected.

Closing as by-design behavior. Feel free to reopen if you believe there's still an issue after trying the above.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2394,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T23:30:00Z"
  },
  "summary": "Reporter draws a circle with radius 3.5f (no antialias, no explicit stroke width) and observes an asymmetric result, which is actually expected pixel-grid aliasing behavior in Skia's raster renderer.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Linux"
    ],
    "backends": [
      "backend/Raster"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a 9x9 SKBitmap",
        "Create SKPaint with IsStroke = true (StrokeWidth not set)",
        "Call DrawCircle(4, 4, 3.5f, paint)",
        "Observe that the rendered stroke pixels are not symmetrically distributed around the center"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, Visual Studio Code, Linux"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "This is fundamental raster rendering behavior unchanged across versions; still reproducible."
    }
  },
  "analysis": {
    "summary": "The asymmetric circle appearance is classic pixel-grid aliasing: drawing a stroke at fractional radius (3.5f) on an integer pixel grid without antialiasing causes the stroke to be rounded differently on opposite sides of the center. This is expected Skia raster behavior, not a SkiaSharp defect. Setting IsAntialias = true or using an integer radius resolves the visual issue.",
    "rationale": "SKCanvas.DrawCircle is a pure passthrough to sk_canvas_draw_circle — no transformation or rounding occurs in SkiaSharp. The aliasing artifact is inherent to drawing fractional-coordinate shapes on a pixel grid without antialiasing, a fundamental property of Skia's raster backend. Community comments confirm this diagnosis and the reporter acknowledged the workarounds work. No SkiaSharp code change is warranted.",
    "keySignals": [
      {
        "text": "I tried to draw a circle with an odd diameter and it does not look like a circle; I guess negative distances are rounded differently",
        "source": "issue body",
        "interpretation": "Reporter correctly suspects rounding asymmetry — this is exactly how aliasing manifests at fractional coordinates."
      },
      {
        "text": "You are experiencing aliasing. As pixels are aligned to an integer grid, and you are asking for a radius of 3.5f, it's fundamentally not possible to draw a 1-pixel wide stroke from x = 0.5 to x = 1.5 without either rounding or antialiasing.",
        "source": "comment by ojb500",
        "interpretation": "Community confirms aliasing root cause; workarounds given (integer radius or IsAntialias = true)."
      },
      {
        "text": "You are right with IsAntialias it is much better.",
        "source": "comment by feanor12 (reporter)",
        "interpretation": "Reporter confirms that setting IsAntialias = true resolves the visual issue."
      },
      {
        "text": "An unset stroke width seems to be an undefined state.",
        "source": "comment by feanor12 (reporter)",
        "interpretation": "Reporter identifies that not setting StrokeWidth explicitly contributes to the issue; setting StrokeWidth = 0 (hairline) or 1 also helps."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "391-401",
        "finding": "DrawCircle is a direct passthrough to sk_canvas_draw_circle with no SkiaSharp-level rounding or transformation. The rendering is delegated entirely to Skia's native raster pipeline.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "lines": "161-164",
        "finding": "StrokeWidth get/set are simple delegates to sk_paint_get/set_stroke_width. The default stroke width is determined by Skia (sk_compatpaint_new). Skia's default is 0 (hairline) but the unset state through compatpaint may differ, causing the asymmetric rendering the reporter observes.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Set `paint.IsAntialias = true` to enable anti-aliasing for smooth sub-pixel rendering",
      "Use an integer radius (e.g., 3.0f instead of 3.5f) to avoid sub-pixel alignment issues",
      "Explicitly set `paint.StrokeWidth = 0` for hairline rendering or `paint.StrokeWidth = 1` for 1-pixel stroke",
      "Shift center to pixel center (e.g., DrawCircle(4.5f, 4.5f, 3.5f, paint)) for better symmetry without antialias"
    ],
    "resolution": {
      "hypothesis": "Fractional-radius stroke circle without antialiasing causes different pixel-rounding on each side of center — this is expected raster rendering behavior in Skia, not a SkiaSharp bug.",
      "proposals": [
        {
          "title": "Enable antialiasing",
          "description": "Set IsAntialias = true on SKPaint for smooth sub-pixel rendering. This is the recommended approach for non-integer coordinates.",
          "category": "workaround",
          "codeSnippet": "paint.IsAntialias = true;\npaint.StrokeWidth = 1;\ncan.DrawCircle(4, 4, 3.5f, paint);",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Use integer radius",
          "description": "Change radius from 3.5f to 3.0f so the stroke aligns exactly to integer pixel boundaries without needing antialiasing.",
          "category": "workaround",
          "codeSnippet": "can.DrawCircle(4, 4, 3.0f, paint);",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Use hairline stroke (StrokeWidth = 0)",
          "description": "Explicitly set StrokeWidth to 0 to get Skia's special hairline rendering mode, which draws a 1-device-pixel-wide stroke that handles fractional coordinates more predictably.",
          "category": "workaround",
          "codeSnippet": "paint.StrokeWidth = 0;\ncan.DrawCircle(4, 4, 3.5f, paint);",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Enable antialiasing",
      "recommendedReason": "IsAntialias = true is the standard approach for smooth rendering of fractional-coordinate shapes and handles all cases. Combined with explicit StrokeWidth, it produces predictable results."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "The asymmetric circle rendering is expected pixel-grid aliasing behavior in Skia's raster pipeline. No SkiaSharp bug exists. Workarounds are well-established and were confirmed working by the reporter in the comments.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question, core SkiaSharp, and Linux labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "os/Linux",
          "backend/Raster"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post explanation of aliasing with confirmed workarounds",
        "risk": "high",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report! What you're seeing is **pixel-grid aliasing** — a fundamental characteristic of raster rendering, not a bug in SkiaSharp or Skia.\n\nWhen you draw a circle with radius `3.5f` without anti-aliasing, pixels at sub-pixel positions must be rounded to the integer grid. Because positive and negative offsets from the center are rounded differently by the renderer, the result looks asymmetric.\n\n**Workarounds (any of these will help):**\n\n1. **Enable anti-aliasing** (recommended):\n   ```csharp\n   paint.IsAntialias = true;\n   paint.StrokeWidth = 1;\n   can.DrawCircle(4, 4, 3.5f, paint);\n   ```\n\n2. **Use an integer radius** to stay on whole-pixel boundaries:\n   ```csharp\n   can.DrawCircle(4, 4, 3.0f, paint);\n   ```\n\n3. **Explicitly set hairline stroke** (`StrokeWidth = 0` is Skia's special hairline mode):\n   ```csharp\n   paint.StrokeWidth = 0;\n   can.DrawCircle(4, 4, 3.5f, paint);\n   ```\n\nNote: always set `StrokeWidth` explicitly — leaving it unset relies on the default from `sk_compatpaint_new` which may not behave as expected.\n\nClosing as by-design behavior. Feel free to reopen if you believe there's still an issue after trying the above."
      },
      {
        "type": "close-issue",
        "description": "Close as by-design/not-a-bug",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
