# Issue Triage Report — #1548

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T09:30:00Z |
| Type | type/question (0.95 (95%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** User asks how to replace black-colored pixels in an SKSurface with transparent/empty before compositing via DrawSurface, wanting to know if SKBlendMode or SKColorFilter is the right tool.

**Analysis:** The reporter wants to 'erase' previously drawn black circles on an off-screen GPU-backed SKSurface before compositing it onto the main canvas. The correct approach is to use SKBlendMode.Clear on the erasing paint, or alternatively draw the black circles last with SKBlendMode.DstOut to cut them out. SKCanvas.Clear() is also available to reset the whole surface.

**Recommendations:** **close-as-not-a-bug** — This is a usage question with a clear answer using existing SKBlendMode.Clear API. No bug is present.

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

**Environment:** GRContext.Create(GRBackend.OpenGL), SKSurface.Create(_locgrContext, true, sKImageInfo)

**Code snippets:**

```csharp
radarsurfspoke.Canvas.DrawCircle((int)x, (int)y, 5f, black);
// ... later ...
canvas.DrawSurface(radarsurfspoke, new SKPoint(0f, 0f));
```

## Analysis

### Technical Summary

The reporter wants to 'erase' previously drawn black circles on an off-screen GPU-backed SKSurface before compositing it onto the main canvas. The correct approach is to use SKBlendMode.Clear on the erasing paint, or alternatively draw the black circles last with SKBlendMode.DstOut to cut them out. SKCanvas.Clear() is also available to reset the whole surface.

### Rationale

Issue title is tagged [QUESTION] and the body asks 'how can I do that?' with no description of broken behavior. The SKBlendMode and SKColorFilter APIs exist and work as expected. This is a usage question.

### Key Signals

- "how can i do that? is it using SKBlendMode or ColorFilter to achieve this" — **issue body** (Usage question — reporter is asking which API to use, not reporting broken behavior.)
- "replace the black colored dots (can be several) with Empty or Transparent" — **issue body** (Reporter wants selective per-shape erasure using blend modes.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 100-109 | direct | SKCanvas.Clear() and Clear(SKColor) exist and reset the entire canvas to transparent/a given color — confirmed API available. |
| `binding/SkiaSharp/SKCanvas.cs` | 78-84 | direct | SKCanvas.DrawColor(SKColor, SKBlendMode) exists — allows drawing a color with any blend mode including Clear over a region. |
| `binding/SkiaSharp/SKPaint.cs` | 215-216 | direct | SKPaint.BlendMode property exists — setting BlendMode = SKBlendMode.Clear on a paint will erase pixels at the drawn shape's location, making them transparent. |

### Workarounds

- Use a paint with BlendMode = SKBlendMode.Clear when drawing the 'eraser' circles — this punches transparent holes at exactly those positions.
- Use canvas.Clear() to reset the whole surface to transparent, then redraw without the black circles.
- Draw the same circles with a transparent color and BlendMode = SKBlendMode.Src to overwrite.

### Resolution Proposals

**Hypothesis:** Reporter needs to use SKBlendMode.Clear on the SKPaint when drawing the circles they want to erase. The surface must use premultiplied alpha (default for GPU surfaces) for Clear blend mode to work correctly.

1. **Use SKBlendMode.Clear paint to erase circles** — fix, confidence 0.90 (90%), cost/xs, validated=yes
   - Create a separate paint with BlendMode set to SKBlendMode.Clear and draw the same circles at the same positions to punch out transparent holes.

```csharp
// Create an eraser paint
using var eraserPaint = new SKPaint { BlendMode = SKBlendMode.Clear };

// Draw the eraser circles at the same positions as the black ones
radarsurfspoke.Canvas.DrawCircle((int)x, (int)y, 5f, eraserPaint);

// Then composite the surface
canvas.DrawSurface(radarsurfspoke, new SKPoint(0f, 0f));
```
2. **Reset entire surface and redraw without black** — alternative, confidence 0.85 (85%), cost/xs, validated=yes
   - Call canvas.Clear() on the surface canvas before redrawing to reset all pixels to transparent, then skip the black circles.

```csharp
// Clear the surface
radarsurfspoke.Canvas.Clear();

// Redraw without the black circles
radarsurfspoke.Canvas.DrawCircle((int)x, (int)y, 5f, theColorStrong);
// ...
```

**Recommended proposal:** Use SKBlendMode.Clear paint to erase circles

**Why:** Most precise — erases only the desired positions without redrawing everything.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | This is a usage question with a clear answer using existing SKBlendMode.Clear API. No bug is present. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply question type and SkiaSharp area labels | labels=type/question, area/SkiaSharp |
| add-comment | high | 0.88 (88%) | Answer the question with SKBlendMode.Clear pattern | — |
| close-issue | medium | 0.85 (85%) | Close as answered question | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the question! You can use `SKBlendMode.Clear` on a paint to erase pixels at a specific location, making them transparent:

```csharp
// Create an eraser paint
using var eraserPaint = new SKPaint { BlendMode = SKBlendMode.Clear };

// Draw the eraser circles at the same positions as the black dots
radarsurfspoke.Canvas.DrawCircle((int)x, (int)y, 5f, eraserPaint);

// Now composite — those positions will be transparent
canvas.DrawSurface(radarsurfspoke, new SKPoint(0f, 0f));
```

Alternatively, if you want to reset the entire surface and redraw without the black circles, use `canvas.Clear()` before redrawing:

```csharp
radarsurfspoke.Canvas.Clear();  // resets all pixels to transparent
// then redraw only the non-black circles
```

Make sure your `SKImageInfo` uses `SKAlphaType.Premul` (the default for GPU surfaces) for transparent pixels to composite correctly.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1548,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T09:30:00Z"
  },
  "summary": "User asks how to replace black-colored pixels in an SKSurface with transparent/empty before compositing via DrawSurface, wanting to know if SKBlendMode or SKColorFilter is the right tool.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    }
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "radarsurfspoke.Canvas.DrawCircle((int)x, (int)y, 5f, black);\n// ... later ...\ncanvas.DrawSurface(radarsurfspoke, new SKPoint(0f, 0f));"
      ],
      "environmentDetails": "GRContext.Create(GRBackend.OpenGL), SKSurface.Create(_locgrContext, true, sKImageInfo)"
    }
  },
  "analysis": {
    "summary": "The reporter wants to 'erase' previously drawn black circles on an off-screen GPU-backed SKSurface before compositing it onto the main canvas. The correct approach is to use SKBlendMode.Clear on the erasing paint, or alternatively draw the black circles last with SKBlendMode.DstOut to cut them out. SKCanvas.Clear() is also available to reset the whole surface.",
    "rationale": "Issue title is tagged [QUESTION] and the body asks 'how can I do that?' with no description of broken behavior. The SKBlendMode and SKColorFilter APIs exist and work as expected. This is a usage question.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "100-109",
        "finding": "SKCanvas.Clear() and Clear(SKColor) exist and reset the entire canvas to transparent/a given color — confirmed API available.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "78-84",
        "finding": "SKCanvas.DrawColor(SKColor, SKBlendMode) exists — allows drawing a color with any blend mode including Clear over a region.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "lines": "215-216",
        "finding": "SKPaint.BlendMode property exists — setting BlendMode = SKBlendMode.Clear on a paint will erase pixels at the drawn shape's location, making them transparent.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "how can i do that? is it using SKBlendMode or ColorFilter to achieve this",
        "source": "issue body",
        "interpretation": "Usage question — reporter is asking which API to use, not reporting broken behavior."
      },
      {
        "text": "replace the black colored dots (can be several) with Empty or Transparent",
        "source": "issue body",
        "interpretation": "Reporter wants selective per-shape erasure using blend modes."
      }
    ],
    "workarounds": [
      "Use a paint with BlendMode = SKBlendMode.Clear when drawing the 'eraser' circles — this punches transparent holes at exactly those positions.",
      "Use canvas.Clear() to reset the whole surface to transparent, then redraw without the black circles.",
      "Draw the same circles with a transparent color and BlendMode = SKBlendMode.Src to overwrite."
    ],
    "resolution": {
      "hypothesis": "Reporter needs to use SKBlendMode.Clear on the SKPaint when drawing the circles they want to erase. The surface must use premultiplied alpha (default for GPU surfaces) for Clear blend mode to work correctly.",
      "proposals": [
        {
          "title": "Use SKBlendMode.Clear paint to erase circles",
          "description": "Create a separate paint with BlendMode set to SKBlendMode.Clear and draw the same circles at the same positions to punch out transparent holes.",
          "category": "fix",
          "codeSnippet": "// Create an eraser paint\nusing var eraserPaint = new SKPaint { BlendMode = SKBlendMode.Clear };\n\n// Draw the eraser circles at the same positions as the black ones\nradarsurfspoke.Canvas.DrawCircle((int)x, (int)y, 5f, eraserPaint);\n\n// Then composite the surface\ncanvas.DrawSurface(radarsurfspoke, new SKPoint(0f, 0f));",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Reset entire surface and redraw without black",
          "description": "Call canvas.Clear() on the surface canvas before redrawing to reset all pixels to transparent, then skip the black circles.",
          "category": "alternative",
          "codeSnippet": "// Clear the surface\nradarsurfspoke.Canvas.Clear();\n\n// Redraw without the black circles\nradarsurfspoke.Canvas.DrawCircle((int)x, (int)y, 5f, theColorStrong);\n// ...",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Use SKBlendMode.Clear paint to erase circles",
      "recommendedReason": "Most precise — erases only the desired positions without redrawing everything."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "This is a usage question with a clear answer using existing SKBlendMode.Clear API. No bug is present.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question type and SkiaSharp area labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Answer the question with SKBlendMode.Clear pattern",
        "risk": "high",
        "confidence": 0.88,
        "comment": "Thanks for the question! You can use `SKBlendMode.Clear` on a paint to erase pixels at a specific location, making them transparent:\n\n```csharp\n// Create an eraser paint\nusing var eraserPaint = new SKPaint { BlendMode = SKBlendMode.Clear };\n\n// Draw the eraser circles at the same positions as the black dots\nradarsurfspoke.Canvas.DrawCircle((int)x, (int)y, 5f, eraserPaint);\n\n// Now composite — those positions will be transparent\ncanvas.DrawSurface(radarsurfspoke, new SKPoint(0f, 0f));\n```\n\nAlternatively, if you want to reset the entire surface and redraw without the black circles, use `canvas.Clear()` before redrawing:\n\n```csharp\nradarsurfspoke.Canvas.Clear();  // resets all pixels to transparent\n// then redraw only the non-black circles\n```\n\nMake sure your `SKImageInfo` uses `SKAlphaType.Premul` (the default for GPU surfaces) for transparent pixels to composite correctly."
      },
      {
        "type": "close-issue",
        "description": "Close as answered question",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
