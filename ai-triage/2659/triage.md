# Issue Triage Report — #2659

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T02:52:28Z |
| Type | type/question (0.88 (88%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.87 (87%)) |

**Issue Summary:** Reporter expects that passing an SKPaint with StrokeWidth=0 to DrawPicture will prevent SVG stroke scaling, but the paint parameter on DrawPicture only applies global effects (opacity, filters) not per-stroke overrides.

**Analysis:** The reporter misunderstands the role of the paint argument in DrawPicture. SKPicture stores pre-recorded drawing commands including their original paints; the paint passed to DrawPicture acts as a global filter (for opacity, color filter, image filter) over the whole picture, not as a stroke-override for individual commands. To achieve hairline cosmetic strokes in a scaled SVG, the SVG drawing commands themselves must use StrokeWidth=0 — this cannot be applied retroactively via DrawPicture's paint parameter.

**Recommendations:** **close-as-not-a-bug** — The DrawPicture paint parameter intentionally acts as a global filter, not a per-stroke override. This is Skia's documented design. A community contributor already clarified this. The behavior is correct.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | os/Linux, os/Windows-Classic |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

**Environment:** SkiaSharp 2.88.3, Visual Studio (Windows), Linux and Windows

**Code snippets:**

```csharp
var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 0 };
canvas.Scale(4);
canvas.DrawPicture(svg.Picture, paint);
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3, 2.88.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | The behavior of DrawPicture paint as a filter is by design in Skia and unchanged across versions. |

## Analysis

### Technical Summary

The reporter misunderstands the role of the paint argument in DrawPicture. SKPicture stores pre-recorded drawing commands including their original paints; the paint passed to DrawPicture acts as a global filter (for opacity, color filter, image filter) over the whole picture, not as a stroke-override for individual commands. To achieve hairline cosmetic strokes in a scaled SVG, the SVG drawing commands themselves must use StrokeWidth=0 — this cannot be applied retroactively via DrawPicture's paint parameter.

### Rationale

A community contributor (taublast) correctly explained this behavior. The regression claim (worked in 2.88.2) is unlikely — the underlying Skia behavior has been consistent. The code in SKCanvas.DrawPicture simply passes the paint handle through to sk_canvas_draw_picture, which uses the paint only as a layer filter, per Skia's design. No code defect exists.

### Key Signals

- "SkPicture holds drawing operations that were already performed. What the paint could affect here is just some global stuff like opacity, filter, shader." — **comment by contributor taublast** (Confirms this is by-design Skia behavior, not a SkiaSharp bug.)
- "Is it possible to scale svg without changing it's edges thickness?" — **issue body** (The real question: how to achieve cosmetic/hairline strokes in a scaled SVG — a usage question.)
- "Last Known Good Version of SkiaSharp: 2.88.2 (Previous)" — **issue body** (Regression claim, but behavior is by-design and has not changed. Likely the reporter only recently discovered the behavior.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 533-538 | direct | DrawPicture(SKPicture, SKPaint) passes the paint handle directly to sk_canvas_draw_picture; this mirrors Skia's API which uses the paint as a layer filter (alpha, color filter, image filter) over the recorded picture — not as a stroke override for individual commands inside the picture. |
| `binding/SkiaSharp/SKCanvas.cs` | 525-531 | related | The matrix overload also passes paint as a filter. No code path overrides individual stroke paints inside the picture — consistent with Skia's documented behavior. |

### Workarounds

- To render an SVG with hairline (cosmetic) strokes that do not scale: re-parse the SVG and apply canvas transformations without using a scale that should affect stroke width. Use SKSvg to get the picture, then draw it at native size and scale the bitmap instead.
- Use canvas.Scale() only for position/bounds, then draw the SVG at the appropriate size by setting the SVG viewport to the target dimensions.
- If you control the SVG source, set stroke-width='0' or stroke-width='1px' with vector-effect='non-scaling-stroke' in the SVG XML directly — this ensures the SVG records hairline strokes and they scale cosmetically.

### Resolution Proposals

**Hypothesis:** Reporter wants SVG strokes to remain thin when the canvas is scaled. This requires either drawing the SVG at its native size into an offscreen bitmap and then scaling the bitmap, or having the SVG source use non-scaling stroke attributes.

1. **Draw SVG at native size, then scale bitmap** — workaround, confidence 0.85 (85%), cost/s, validated=untested
   - Render the SVG into a bitmap at its original size, then draw that bitmap scaled. Stroke width remains cosmetic.
2. **Use non-scaling-stroke in SVG source** — alternative, confidence 0.75 (75%), cost/m, validated=untested
   - Edit the SVG XML to add vector-effect='non-scaling-stroke' on stroke elements so Skia records them as cosmetic/hairline.

**Recommended proposal:** Draw SVG at native size, then scale bitmap

**Why:** Does not require modifying the SVG source and works reliably across all versions.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.87 (87%) |
| Reason | The DrawPicture paint parameter intentionally acts as a global filter, not a per-stroke override. This is Skia's documented design. A community contributor already clarified this. The behavior is correct. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Correct label from type/bug to type/question; add area and platform labels | labels=type/question, area/SkiaSharp, os/Linux, os/Windows-Classic |
| add-comment | high | 0.87 (87%) | Explain DrawPicture paint semantics and provide workaround | — |
| close-issue | medium | 0.85 (85%) | Close as answered / not a bug | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for reporting! This is actually by-design behavior in Skia (the underlying C++ library). The `SKPaint` passed to `DrawPicture` is a *layer filter* — it can control opacity, color filters, and image filters applied to the whole picture as a unit. It does **not** override the stroke widths recorded inside the picture.

As @taublast noted, the picture stores its drawing operations with their original paints already recorded.

**To prevent strokes from scaling**, the workaround is to render the SVG into a bitmap at its native size, then scale that bitmap:

```csharp
// render at native size
using var bitmap = new SKBitmap((int)svg.Picture.CullRect.Width, (int)svg.Picture.CullRect.Height);
using (var offscreen = new SKCanvas(bitmap))
    offscreen.DrawPicture(svg.Picture);

// draw scaled bitmap (strokes remain cosmetic)
using (var canvas = new SKCanvas(targetBitmap))
    canvas.DrawBitmap(bitmap, SKRect.Create(0, 0, targetBitmap.Width, targetBitmap.Height));
```

Alternatively, if you control the SVG source, add `vector-effect="non-scaling-stroke"` to your SVG stroke elements.

Closing as not a bug — if you have follow-up questions feel free to open a new discussion.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2659,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T02:52:28Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter expects that passing an SKPaint with StrokeWidth=0 to DrawPicture will prevent SVG stroke scaling, but the paint parameter on DrawPicture only applies global effects (opacity, filters) not per-stroke overrides.",
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
      "os/Linux",
      "os/Windows-Classic"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "SkiaSharp 2.88.3, Visual Studio (Windows), Linux and Windows",
      "codeSnippets": [
        "var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 0 };\ncanvas.Scale(4);\ncanvas.DrawPicture(svg.Picture, paint);"
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3",
        "2.88.2"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "The behavior of DrawPicture paint as a filter is by design in Skia and unchanged across versions."
    }
  },
  "analysis": {
    "summary": "The reporter misunderstands the role of the paint argument in DrawPicture. SKPicture stores pre-recorded drawing commands including their original paints; the paint passed to DrawPicture acts as a global filter (for opacity, color filter, image filter) over the whole picture, not as a stroke-override for individual commands. To achieve hairline cosmetic strokes in a scaled SVG, the SVG drawing commands themselves must use StrokeWidth=0 — this cannot be applied retroactively via DrawPicture's paint parameter.",
    "rationale": "A community contributor (taublast) correctly explained this behavior. The regression claim (worked in 2.88.2) is unlikely — the underlying Skia behavior has been consistent. The code in SKCanvas.DrawPicture simply passes the paint handle through to sk_canvas_draw_picture, which uses the paint only as a layer filter, per Skia's design. No code defect exists.",
    "keySignals": [
      {
        "text": "SkPicture holds drawing operations that were already performed. What the paint could affect here is just some global stuff like opacity, filter, shader.",
        "source": "comment by contributor taublast",
        "interpretation": "Confirms this is by-design Skia behavior, not a SkiaSharp bug."
      },
      {
        "text": "Is it possible to scale svg without changing it's edges thickness?",
        "source": "issue body",
        "interpretation": "The real question: how to achieve cosmetic/hairline strokes in a scaled SVG — a usage question."
      },
      {
        "text": "Last Known Good Version of SkiaSharp: 2.88.2 (Previous)",
        "source": "issue body",
        "interpretation": "Regression claim, but behavior is by-design and has not changed. Likely the reporter only recently discovered the behavior."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "533-538",
        "finding": "DrawPicture(SKPicture, SKPaint) passes the paint handle directly to sk_canvas_draw_picture; this mirrors Skia's API which uses the paint as a layer filter (alpha, color filter, image filter) over the recorded picture — not as a stroke override for individual commands inside the picture.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "525-531",
        "finding": "The matrix overload also passes paint as a filter. No code path overrides individual stroke paints inside the picture — consistent with Skia's documented behavior.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "To render an SVG with hairline (cosmetic) strokes that do not scale: re-parse the SVG and apply canvas transformations without using a scale that should affect stroke width. Use SKSvg to get the picture, then draw it at native size and scale the bitmap instead.",
      "Use canvas.Scale() only for position/bounds, then draw the SVG at the appropriate size by setting the SVG viewport to the target dimensions.",
      "If you control the SVG source, set stroke-width='0' or stroke-width='1px' with vector-effect='non-scaling-stroke' in the SVG XML directly — this ensures the SVG records hairline strokes and they scale cosmetically."
    ],
    "resolution": {
      "hypothesis": "Reporter wants SVG strokes to remain thin when the canvas is scaled. This requires either drawing the SVG at its native size into an offscreen bitmap and then scaling the bitmap, or having the SVG source use non-scaling stroke attributes.",
      "proposals": [
        {
          "title": "Draw SVG at native size, then scale bitmap",
          "description": "Render the SVG into a bitmap at its original size, then draw that bitmap scaled. Stroke width remains cosmetic.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Use non-scaling-stroke in SVG source",
          "description": "Edit the SVG XML to add vector-effect='non-scaling-stroke' on stroke elements so Skia records them as cosmetic/hairline.",
          "category": "alternative",
          "confidence": 0.75,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Draw SVG at native size, then scale bitmap",
      "recommendedReason": "Does not require modifying the SVG source and works reliably across all versions."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.87,
      "reason": "The DrawPicture paint parameter intentionally acts as a global filter, not a per-stroke override. This is Skia's documented design. A community contributor already clarified this. The behavior is correct.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct label from type/bug to type/question; add area and platform labels",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "os/Linux",
          "os/Windows-Classic"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain DrawPicture paint semantics and provide workaround",
        "risk": "high",
        "confidence": 0.87,
        "comment": "Thanks for reporting! This is actually by-design behavior in Skia (the underlying C++ library). The `SKPaint` passed to `DrawPicture` is a *layer filter* — it can control opacity, color filters, and image filters applied to the whole picture as a unit. It does **not** override the stroke widths recorded inside the picture.\n\nAs @taublast noted, the picture stores its drawing operations with their original paints already recorded.\n\n**To prevent strokes from scaling**, the workaround is to render the SVG into a bitmap at its native size, then scale that bitmap:\n\n```csharp\n// render at native size\nusing var bitmap = new SKBitmap((int)svg.Picture.CullRect.Width, (int)svg.Picture.CullRect.Height);\nusing (var offscreen = new SKCanvas(bitmap))\n    offscreen.DrawPicture(svg.Picture);\n\n// draw scaled bitmap (strokes remain cosmetic)\nusing (var canvas = new SKCanvas(targetBitmap))\n    canvas.DrawBitmap(bitmap, SKRect.Create(0, 0, targetBitmap.Width, targetBitmap.Height));\n```\n\nAlternatively, if you control the SVG source, add `vector-effect=\"non-scaling-stroke\"` to your SVG stroke elements.\n\nClosing as not a bug — if you have follow-up questions feel free to open a new discussion."
      },
      {
        "type": "close-issue",
        "description": "Close as answered / not a bug",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
