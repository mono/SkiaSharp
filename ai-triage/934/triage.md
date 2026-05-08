# Issue Triage Report — #934

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T08:40:00Z |
| Type | type/question (0.92 (92%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** Reporter asks why SKCanvas.ClipRect with SKClipOperation.Difference does not reduce LocalClipBounds, while Intersect does; reporter found working alternatives using SKPath.Op and surrounding-rect decomposition.

**Analysis:** LocalClipBounds returns a conservative axis-aligned bounding box of the drawable area, not the exact clip shape. Skia does not tighten the bounding box after a Difference clip because the result can be non-convex; it conservatively keeps the original extent. The Difference clip operation itself functions correctly for rendering. Reporter found two working alternatives: decomposing the subtraction into surrounding Intersect rects, and using SKPath.Op with SKPathOp.Difference to compute the resulting path first, then clipping with that.

**Recommendations:** **close-as-not-a-bug** — LocalClipBounds returning conservative bounds after Difference is by-design Skia behavior. The reporter has already found working alternatives (SKPath.Op) and the question is answered. No actual rendering bug exists.

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

1. Create an SKSurface from rect1 (10,10,110,110)
2. Translate canvas by (-rect1.Left, -rect1.Top)
3. Call canvas.ClipRect(rect2, SKClipOperation.Difference) where rect2 = (10,60,110,110)
4. Read canvas.LocalClipBounds — bounds remain (10,10,110,110) instead of shrinking

**Code snippets:**

```csharp
surface.Canvas.ClipRect(rect2, SKClipOperation.Difference);
```

```csharp
// Expected: LocalClipBounds shrinks to top half. Actual: unchanged.
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | This is by-design Skia behavior unchanged across versions; LocalClipBounds has always been a conservative bounding box. |

## Analysis

### Technical Summary

LocalClipBounds returns a conservative axis-aligned bounding box of the drawable area, not the exact clip shape. Skia does not tighten the bounding box after a Difference clip because the result can be non-convex; it conservatively keeps the original extent. The Difference clip operation itself functions correctly for rendering. Reporter found two working alternatives: decomposing the subtraction into surrounding Intersect rects, and using SKPath.Op with SKPathOp.Difference to compute the resulting path first, then clipping with that.

### Rationale

Issue title and body say [QUESTION] and 'is there something I'm missing?'. No crash or wrong-rendering is described — only unexpected LocalClipBounds behavior. This is by-design in Skia: getLocalClipBounds() is a conservative estimate. Reporter self-resolved with two working approaches in subsequent comments.

### Key Signals

- "the clip bounds always remain unchanged" — **issue body** (Reporter observes LocalClipBounds is not tightened after Difference; this is expected conservative behavior from Skia.)
- "This works however. Intersecting with rect2 reduces the clip bounds to a height of 50" — **issue body** (Intersect tightens bounds because intersection of two rectangles is a rectangle; Skia can compute it exactly. Difference yields a non-rectangular shape so bounds stay at original extent.)
- "if I put my main rectangle into a new path and use the SKPath.Op method with the SKPathOp.Difference parameter, it works just fine" — **comment by PlanetBloopy (2019-11-13)** (SKPath.Op correctly computes the boolean difference of two paths as a new path; clipping with that path gives exact results.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 247-266 | direct | ClipRect, ClipPath, ClipRoundRect all pass through to the Skia C API with the given SKClipOperation. No SkiaSharp-level filtering or transformation of the operation. The Difference operation reaches the native layer unchanged. |
| `binding/SkiaSharp/SKCanvas.cs` | 276-306 | direct | LocalClipBounds delegates directly to sk_canvas_get_local_clip_bounds via P/Invoke. No SkiaSharp post-processing. The returned value is whatever Skia reports, which is a conservative bounding box — not the tight clip-shape boundary after Difference operations. |
| `binding/SkiaSharp/SKPath.cs` | 417-427 | related | SKPath.Op(SKPath other, SKPathOp op) / SKPath.Op(SKPath other, SKPathOp op, SKPath result) perform boolean operations on paths (Difference, Intersect, Union, XOR, ReverseDifference). Using SKPathOp.Difference correctly computes the subtracted path shape, which can then be used with canvas.ClipPath for an exact clip. |

### Workarounds

- Use SKPath.Op with SKPathOp.Difference: create a path from the main rectangle, call path.Op(subtractPath, SKPathOp.Difference) to get the result path, then canvas.ClipPath(resultPath).
- Decompose the subtraction into up to 4 surrounding rectangles and add each to an SKPath, then call canvas.ClipPath(path) with Intersect (reporter's first workaround).

### Resolution Proposals

**Hypothesis:** Reporter expected LocalClipBounds to reflect the tight boundary of the clip after Difference, but Skia returns a conservative bounding box. The actual clip region IS correct for rendering. The fix is to use SKPath.Op for boolean path operations rather than using ClipRect/ClipPath to compute visual bounds.

1. **Use SKPath.Op for boolean subtraction** — workaround, confidence 0.92 (92%), cost/xs, validated=yes
   - Create a main rectangle path and a subtraction path, use SKPath.Op with SKPathOp.Difference to compute the result path, then use canvas.ClipPath on the result. This gives correct rendering and correct path bounds via resultPath.Bounds.

```csharp
using var mainPath = new SKPath();
mainPath.AddRect(rect1);
using var subtractPath = new SKPath();
subtractPath.AddRect(rect2);
using var resultPath = mainPath.Op(subtractPath, SKPathOp.Difference);
canvas.ClipPath(resultPath);
// resultPath.Bounds gives the tight bounding box
```
2. **Decompose into surrounding rectangles** — alternative, confidence 0.88 (88%), cost/s, validated=yes
   - Compute the space around the rectangle to subtract (up to 4 rects), add them to a path, and intersect-clip with that path.

**Recommended proposal:** Use SKPath.Op for boolean subtraction

**Why:** Cleaner, one-step approach. SKPath.Op is the right API for boolean path operations. Reporter confirmed this works.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | LocalClipBounds returning conservative bounds after Difference is by-design Skia behavior. The reporter has already found working alternatives (SKPath.Op) and the question is answered. No actual rendering bug exists. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply question and core SkiaSharp area labels | labels=type/question, area/SkiaSharp |
| add-comment | high | 0.88 (88%) | Explain LocalClipBounds conservative behavior and confirm SKPath.Op workaround | — |
| close-issue | medium | 0.85 (85%) | Close as answered/by-design | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed question and for following up with your workarounds!

This is by-design behavior from Skia's underlying engine. `LocalClipBounds` returns a **conservative axis-aligned bounding box** of the drawable area — it is not guaranteed to be the exact tight boundary after a `Difference` clip. Intersect works 'tightly' because the intersection of two rectangles is always a rectangle that Skia can compute exactly. Difference produces a shape that may be non-convex (e.g., a C-shape or L-shape), so Skia keeps the conservative original bounds.

The actual clip region **is** applied correctly for rendering — pixels outside the clip are not drawn. Only `LocalClipBounds` doesn't reflect the tighter boundary.

Your `SKPath.Op` approach is exactly the right solution:

```csharp
using var mainPath = new SKPath();
mainPath.AddRect(rect1);
using var subtractPath = new SKPath();
subtractPath.AddRect(rect2);
using var resultPath = mainPath.Op(subtractPath, SKPathOp.Difference);
canvas.ClipPath(resultPath);
// Use resultPath.Bounds to get the tight bounding box
```

This uses `SKPath.Op` to compute the boolean difference as a real path (with correct `Bounds`), then clips with that path.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 934,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T08:40:00Z"
  },
  "summary": "Reporter asks why SKCanvas.ClipRect with SKClipOperation.Difference does not reduce LocalClipBounds, while Intersect does; reporter found working alternatives using SKPath.Op and surrounding-rect decomposition.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    }
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKSurface from rect1 (10,10,110,110)",
        "Translate canvas by (-rect1.Left, -rect1.Top)",
        "Call canvas.ClipRect(rect2, SKClipOperation.Difference) where rect2 = (10,60,110,110)",
        "Read canvas.LocalClipBounds — bounds remain (10,10,110,110) instead of shrinking"
      ],
      "codeSnippets": [
        "surface.Canvas.ClipRect(rect2, SKClipOperation.Difference);",
        "// Expected: LocalClipBounds shrinks to top half. Actual: unchanged."
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "This is by-design Skia behavior unchanged across versions; LocalClipBounds has always been a conservative bounding box."
    }
  },
  "analysis": {
    "summary": "LocalClipBounds returns a conservative axis-aligned bounding box of the drawable area, not the exact clip shape. Skia does not tighten the bounding box after a Difference clip because the result can be non-convex; it conservatively keeps the original extent. The Difference clip operation itself functions correctly for rendering. Reporter found two working alternatives: decomposing the subtraction into surrounding Intersect rects, and using SKPath.Op with SKPathOp.Difference to compute the resulting path first, then clipping with that.",
    "rationale": "Issue title and body say [QUESTION] and 'is there something I'm missing?'. No crash or wrong-rendering is described — only unexpected LocalClipBounds behavior. This is by-design in Skia: getLocalClipBounds() is a conservative estimate. Reporter self-resolved with two working approaches in subsequent comments.",
    "keySignals": [
      {
        "text": "the clip bounds always remain unchanged",
        "source": "issue body",
        "interpretation": "Reporter observes LocalClipBounds is not tightened after Difference; this is expected conservative behavior from Skia."
      },
      {
        "text": "This works however. Intersecting with rect2 reduces the clip bounds to a height of 50",
        "source": "issue body",
        "interpretation": "Intersect tightens bounds because intersection of two rectangles is a rectangle; Skia can compute it exactly. Difference yields a non-rectangular shape so bounds stay at original extent."
      },
      {
        "text": "if I put my main rectangle into a new path and use the SKPath.Op method with the SKPathOp.Difference parameter, it works just fine",
        "source": "comment by PlanetBloopy (2019-11-13)",
        "interpretation": "SKPath.Op correctly computes the boolean difference of two paths as a new path; clipping with that path gives exact results."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "247-266",
        "finding": "ClipRect, ClipPath, ClipRoundRect all pass through to the Skia C API with the given SKClipOperation. No SkiaSharp-level filtering or transformation of the operation. The Difference operation reaches the native layer unchanged.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "276-306",
        "finding": "LocalClipBounds delegates directly to sk_canvas_get_local_clip_bounds via P/Invoke. No SkiaSharp post-processing. The returned value is whatever Skia reports, which is a conservative bounding box — not the tight clip-shape boundary after Difference operations.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPath.cs",
        "lines": "417-427",
        "finding": "SKPath.Op(SKPath other, SKPathOp op) / SKPath.Op(SKPath other, SKPathOp op, SKPath result) perform boolean operations on paths (Difference, Intersect, Union, XOR, ReverseDifference). Using SKPathOp.Difference correctly computes the subtracted path shape, which can then be used with canvas.ClipPath for an exact clip.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use SKPath.Op with SKPathOp.Difference: create a path from the main rectangle, call path.Op(subtractPath, SKPathOp.Difference) to get the result path, then canvas.ClipPath(resultPath).",
      "Decompose the subtraction into up to 4 surrounding rectangles and add each to an SKPath, then call canvas.ClipPath(path) with Intersect (reporter's first workaround)."
    ],
    "resolution": {
      "hypothesis": "Reporter expected LocalClipBounds to reflect the tight boundary of the clip after Difference, but Skia returns a conservative bounding box. The actual clip region IS correct for rendering. The fix is to use SKPath.Op for boolean path operations rather than using ClipRect/ClipPath to compute visual bounds.",
      "proposals": [
        {
          "title": "Use SKPath.Op for boolean subtraction",
          "description": "Create a main rectangle path and a subtraction path, use SKPath.Op with SKPathOp.Difference to compute the result path, then use canvas.ClipPath on the result. This gives correct rendering and correct path bounds via resultPath.Bounds.",
          "category": "workaround",
          "codeSnippet": "using var mainPath = new SKPath();\nmainPath.AddRect(rect1);\nusing var subtractPath = new SKPath();\nsubtractPath.AddRect(rect2);\nusing var resultPath = mainPath.Op(subtractPath, SKPathOp.Difference);\ncanvas.ClipPath(resultPath);\n// resultPath.Bounds gives the tight bounding box",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Decompose into surrounding rectangles",
          "description": "Compute the space around the rectangle to subtract (up to 4 rects), add them to a path, and intersect-clip with that path.",
          "category": "alternative",
          "confidence": 0.88,
          "effort": "cost/s",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Use SKPath.Op for boolean subtraction",
      "recommendedReason": "Cleaner, one-step approach. SKPath.Op is the right API for boolean path operations. Reporter confirmed this works."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "LocalClipBounds returning conservative bounds after Difference is by-design Skia behavior. The reporter has already found working alternatives (SKPath.Op) and the question is answered. No actual rendering bug exists.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and core SkiaSharp area labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain LocalClipBounds conservative behavior and confirm SKPath.Op workaround",
        "risk": "high",
        "confidence": 0.88,
        "comment": "Thanks for the detailed question and for following up with your workarounds!\n\nThis is by-design behavior from Skia's underlying engine. `LocalClipBounds` returns a **conservative axis-aligned bounding box** of the drawable area — it is not guaranteed to be the exact tight boundary after a `Difference` clip. Intersect works 'tightly' because the intersection of two rectangles is always a rectangle that Skia can compute exactly. Difference produces a shape that may be non-convex (e.g., a C-shape or L-shape), so Skia keeps the conservative original bounds.\n\nThe actual clip region **is** applied correctly for rendering — pixels outside the clip are not drawn. Only `LocalClipBounds` doesn't reflect the tighter boundary.\n\nYour `SKPath.Op` approach is exactly the right solution:\n\n```csharp\nusing var mainPath = new SKPath();\nmainPath.AddRect(rect1);\nusing var subtractPath = new SKPath();\nsubtractPath.AddRect(rect2);\nusing var resultPath = mainPath.Op(subtractPath, SKPathOp.Difference);\ncanvas.ClipPath(resultPath);\n// Use resultPath.Bounds to get the tight bounding box\n```\n\nThis uses `SKPath.Op` to compute the boolean difference as a real path (with correct `Bounds`), then clips with that path."
      },
      {
        "type": "close-issue",
        "description": "Close as answered/by-design",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
