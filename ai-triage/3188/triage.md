# Issue Triage Report — #3188

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:35:38Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-investigation (0.85 (85%)) |

**Issue Summary:** Canvas.DrawVertices with vertex colors produces a black image in SkiaSharp v3.116 where the same code produced a correct gradient triangle in v2.88.9, due to an upstream Skia blend mode behavior change affecting the Modulate default used by convenience overloads.

**Analysis:** In upstream Skia m116+, drawVertices changed how it combines vertex colors with the paint. With SKBlendMode.Modulate and no shader in the paint, vertex colors are multiplied by the paint's default color (opaque black), resulting in black output. SkiaSharp's convenience overloads (DrawVertices(SKVertexMode, SKPoint[], SKColor[], SKPaint)) hardcode SKBlendMode.Modulate and are now broken for plain vertex-color use cases. The Gallery sample was updated to use SKBlendMode.Dst, but the public convenience overloads were not.

**Recommendations:** **needs-investigation** — Real regression with full repro code, clear root cause identified (upstream Skia blend mode change affecting convenience overloads), and a clear fix path. Needs decision on whether to change blend mode default or just add migration docs.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/Raster |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create SKSurface with Rgba8888/Premul imageInfo
2. Create SKPaint with IsAntialias = true (no shader, no color set)
3. Call canvas.DrawVertices(SKVertexMode.Triangles, vertices, colors, paint) with Red/Green/Blue colors
4. Observe black triangle instead of gradient

**Environment:** Windows 11, .NET 8, SkiaSharp 3.116.1, Visual Studio

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3188#issuecomment-3121487589 — Community workaround: add Shader = SKShader.CreateColor(SKColors.White) to paint
- https://api.skia.org/classSkCanvas.html#ae14234b29eb75b68c0da1c08119251f4 — Skia upstream drawVertices documentation showing changed behavior in m116+

**Code snippets:**

```csharp
var vertices = new[] { new SKPoint(110, 20), new SKPoint(160, 200), new SKPoint(10, 200) };
var colors = new[] { SKColors.Red, SKColors.Green, SKColors.Blue };
surface.Canvas.DrawVertices(SKVertexMode.Triangles, vertices, colors, paint);
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | Black triangle rendered instead of RGB gradient triangle |
| Repro quality | complete |
| Target frameworks | net8.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | likely |
| Relevance reason | SKCanvas.DrawVertices convenience overloads still hardcode SKBlendMode.Modulate; the upstream Skia behavior change has not been addressed. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.92 (92%) |
| Reason | Identical code produces correct gradient in v2.88.9 but black output in v3.116.x. No code changes were needed between versions. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.0 |

## Analysis

### Technical Summary

In upstream Skia m116+, drawVertices changed how it combines vertex colors with the paint. With SKBlendMode.Modulate and no shader in the paint, vertex colors are multiplied by the paint's default color (opaque black), resulting in black output. SkiaSharp's convenience overloads (DrawVertices(SKVertexMode, SKPoint[], SKColor[], SKPaint)) hardcode SKBlendMode.Modulate and are now broken for plain vertex-color use cases. The Gallery sample was updated to use SKBlendMode.Dst, but the public convenience overloads were not.

### Rationale

A regression where identical caller code produces different visual output between SkiaSharp v2 and v3 is a bug by definition. The root cause is an upstream Skia blend-mode semantic change, but SkiaSharp's convenience overloads did not compensate by updating the default blend mode. The Gallery sample correctly uses SKBlendMode.Dst which shows vertex colors, but public API callers using the convenience overloads are silently broken.

### Key Signals

- "output is simply black" — **issue body** (Classic symptom of Modulate blending against a black source — vertex colors multiplied by black = black.)
- "works fine in v2.88.9 but after updating to 3.116.1 the output is simply black" — **issue body** (Clear regression. Code unchanged, SkiaSharp version changed.)
- "This will give you the result you were after: Shader = SKShader.CreateColor(SKColors.White)" — **comment by molesmoke** (Workaround confirmed: providing a white shader causes Modulate to correctly pass through vertex colors (vertex_color * white = vertex_color).)
- "canvas.DrawVertices(verts, SKBlendMode.Dst, paint)" — **samples/Gallery/Shared/Samples/VertexMeshSample.cs line 97** (Gallery sample uses SKBlendMode.Dst (not Modulate), indicating maintainers know about the upstream change but didn't update the convenience overloads.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 912-933 | direct | All three convenience overloads DrawVertices(SKVertexMode, SKPoint[], SKColor[], SKPaint), DrawVertices(SKVertexMode, SKPoint[], SKPoint[], SKColor[], SKPaint), and DrawVertices(SKVertexMode, SKPoint[], SKPoint[], SKColor[], UInt16[], SKPaint) hardcode SKBlendMode.Modulate. With upstream Skia m116 behavior change, this produces black output when no shader is in the paint. |
| `samples/Gallery/Shared/Samples/VertexMeshSample.cs` | 95-97 | direct | Gallery sample uses SKVertices.CreateCopy + canvas.DrawVertices(verts, SKBlendMode.Dst, paint) — explicitly uses Dst blend mode, not Modulate, to correctly render vertex colors in v3. |
| `binding/SkiaSharp/SKCanvas.cs` | 936-943 | related | Base DrawVertices(SKVertices, SKBlendMode, SKPaint) correctly passes the blend mode through to the native sk_canvas_draw_vertices call. The issue is in the convenience wrappers choosing Modulate. |

### Workarounds

- Use SKShader.CreateColor(SKColors.White) as the paint shader: paint.Shader = SKShader.CreateColor(SKColors.White). With Modulate, vertex_color * white = vertex_color.
- Use the lower-level overload with explicit blend mode: var verts = SKVertices.CreateCopy(SKVertexMode.Triangles, vertices, null, colors); canvas.DrawVertices(verts, SKBlendMode.Dst, paint);

### Next Questions

- Should the convenience overloads change their default blend mode to Dst? This would fix behavior but change ABI semantics for callers using mixed shader+vertex-color scenarios.
- Does the same regression affect DrawPatch which also defaults to SKBlendMode.Modulate (SKCanvas.cs line 1018)?
- Is there a v2-to-v3 migration guide that should document this behavioral change?

### Resolution Proposals

**Hypothesis:** Upstream Skia m116+ changed how drawVertices blends vertex colors with the paint. SKBlendMode.Modulate now multiplies vertex colors by the paint's shader/color, which defaults to black, giving black output. SkiaSharp convenience overloads need to either use a different default blend mode (Dst) or set a white paint color as the baseline.

1. **Change convenience overload default blend mode to Dst** — fix, confidence 0.75 (75%), cost/xs, validated=untested
   - Update the three SKCanvas.DrawVertices convenience overloads to use SKBlendMode.Dst instead of SKBlendMode.Modulate. This makes vertex colors render directly without requiring a paint shader, matching v2.88.9 behavior.

```csharp
public void DrawVertices (SKVertexMode vmode, SKPoint[] vertices, SKColor[] colors, SKPaint paint)
{
    var vert = SKVertices.CreateCopy (vmode, vertices, colors);
    DrawVertices (vert, SKBlendMode.Dst, paint);
}
```
2. **Add migration documentation** — workaround, confidence 0.90 (90%), cost/s, validated=untested
   - Document the DrawVertices blend mode behavior change in the v2-to-v3 migration guide, explaining the need for a white shader or SKBlendMode.Dst when using vertex colors.

**Recommended proposal:** Change convenience overload default blend mode to Dst

**Why:** This restores v2.88.9 behavior for all callers using the convenience overloads, is a minimal change, and matches how the Gallery sample already correctly renders vertices in v3.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.85 (85%) |
| Reason | Real regression with full repro code, clear root cause identified (upstream Skia blend mode change affecting convenience overloads), and a clear fix path. Needs decision on whether to change blend mode default or just add migration docs. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.93 (93%) | Apply bug, SkiaSharp core area, Windows platform, Raster backend, and compatibility tenet labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, backend/Raster, tenet/compatibility |
| add-comment | medium | 0.87 (87%) | Confirm regression, explain root cause, provide two workarounds | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed reproduction case with screenshots!

This is a regression caused by an upstream Skia behavioral change in m116. In newer Skia, `DrawVertices` with `SKBlendMode.Modulate` (the default used by the convenience overloads) multiplies vertex colors by the paint's shader/color. Since the default paint has no shader and defaults to black, the result is `vertex_color × black = black`.

There are two workarounds available today:

**Option 1** — Set a white shader on the paint (as suggested by @molesmoke):
```csharp
var paint = new SKPaint
{
    IsAntialias = true,
    Shader = SKShader.CreateColor(SKColors.White)
};
surface.Canvas.DrawVertices(SKVertexMode.Triangles, vertices, colors, paint);
```
This works because `vertex_color × white = vertex_color`.

**Option 2** — Use `SKBlendMode.Dst` with the lower-level overload:
```csharp
var paint = new SKPaint { IsAntialias = true };
var verts = SKVertices.CreateCopy(SKVertexMode.Triangles, vertices, null, colors);
surface.Canvas.DrawVertices(verts, SKBlendMode.Dst, paint);
```
This is the approach used in the [Gallery samples](https://github.com/mono/SkiaSharp/blob/main/samples/Gallery/Shared/Samples/VertexMeshSample.cs#L97).

We are investigating whether to update the convenience overloads to use `SKBlendMode.Dst` by default to restore v2.88.9 behavior.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3188,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:35:38Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Canvas.DrawVertices with vertex colors produces a black image in SkiaSharp v3.116 where the same code produced a correct gradient triangle in v2.88.9, due to an upstream Skia blend mode behavior change affecting the Modulate default used by convenience overloads.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/Raster"
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
      "errorMessage": "Black triangle rendered instead of RGB gradient triangle",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net8.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create SKSurface with Rgba8888/Premul imageInfo",
        "Create SKPaint with IsAntialias = true (no shader, no color set)",
        "Call canvas.DrawVertices(SKVertexMode.Triangles, vertices, colors, paint) with Red/Green/Blue colors",
        "Observe black triangle instead of gradient"
      ],
      "environmentDetails": "Windows 11, .NET 8, SkiaSharp 3.116.1, Visual Studio",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3188#issuecomment-3121487589",
          "description": "Community workaround: add Shader = SKShader.CreateColor(SKColors.White) to paint"
        },
        {
          "url": "https://api.skia.org/classSkCanvas.html#ae14234b29eb75b68c0da1c08119251f4",
          "description": "Skia upstream drawVertices documentation showing changed behavior in m116+"
        }
      ],
      "codeSnippets": [
        "var vertices = new[] { new SKPoint(110, 20), new SKPoint(160, 200), new SKPoint(10, 200) };\nvar colors = new[] { SKColors.Red, SKColors.Green, SKColors.Blue };\nsurface.Canvas.DrawVertices(SKVertexMode.Triangles, vertices, colors, paint);"
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "2.88.9"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.116.0",
      "currentRelevance": "likely",
      "relevanceReason": "SKCanvas.DrawVertices convenience overloads still hardcode SKBlendMode.Modulate; the upstream Skia behavior change has not been addressed."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.92,
      "reason": "Identical code produces correct gradient in v2.88.9 but black output in v3.116.x. No code changes were needed between versions.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.0"
    }
  },
  "analysis": {
    "summary": "In upstream Skia m116+, drawVertices changed how it combines vertex colors with the paint. With SKBlendMode.Modulate and no shader in the paint, vertex colors are multiplied by the paint's default color (opaque black), resulting in black output. SkiaSharp's convenience overloads (DrawVertices(SKVertexMode, SKPoint[], SKColor[], SKPaint)) hardcode SKBlendMode.Modulate and are now broken for plain vertex-color use cases. The Gallery sample was updated to use SKBlendMode.Dst, but the public convenience overloads were not.",
    "rationale": "A regression where identical caller code produces different visual output between SkiaSharp v2 and v3 is a bug by definition. The root cause is an upstream Skia blend-mode semantic change, but SkiaSharp's convenience overloads did not compensate by updating the default blend mode. The Gallery sample correctly uses SKBlendMode.Dst which shows vertex colors, but public API callers using the convenience overloads are silently broken.",
    "keySignals": [
      {
        "text": "output is simply black",
        "source": "issue body",
        "interpretation": "Classic symptom of Modulate blending against a black source — vertex colors multiplied by black = black."
      },
      {
        "text": "works fine in v2.88.9 but after updating to 3.116.1 the output is simply black",
        "source": "issue body",
        "interpretation": "Clear regression. Code unchanged, SkiaSharp version changed."
      },
      {
        "text": "This will give you the result you were after: Shader = SKShader.CreateColor(SKColors.White)",
        "source": "comment by molesmoke",
        "interpretation": "Workaround confirmed: providing a white shader causes Modulate to correctly pass through vertex colors (vertex_color * white = vertex_color)."
      },
      {
        "text": "canvas.DrawVertices(verts, SKBlendMode.Dst, paint)",
        "source": "samples/Gallery/Shared/Samples/VertexMeshSample.cs line 97",
        "interpretation": "Gallery sample uses SKBlendMode.Dst (not Modulate), indicating maintainers know about the upstream change but didn't update the convenience overloads."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "912-933",
        "finding": "All three convenience overloads DrawVertices(SKVertexMode, SKPoint[], SKColor[], SKPaint), DrawVertices(SKVertexMode, SKPoint[], SKPoint[], SKColor[], SKPaint), and DrawVertices(SKVertexMode, SKPoint[], SKPoint[], SKColor[], UInt16[], SKPaint) hardcode SKBlendMode.Modulate. With upstream Skia m116 behavior change, this produces black output when no shader is in the paint.",
        "relevance": "direct"
      },
      {
        "file": "samples/Gallery/Shared/Samples/VertexMeshSample.cs",
        "lines": "95-97",
        "finding": "Gallery sample uses SKVertices.CreateCopy + canvas.DrawVertices(verts, SKBlendMode.Dst, paint) — explicitly uses Dst blend mode, not Modulate, to correctly render vertex colors in v3.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "936-943",
        "finding": "Base DrawVertices(SKVertices, SKBlendMode, SKPaint) correctly passes the blend mode through to the native sk_canvas_draw_vertices call. The issue is in the convenience wrappers choosing Modulate.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use SKShader.CreateColor(SKColors.White) as the paint shader: paint.Shader = SKShader.CreateColor(SKColors.White). With Modulate, vertex_color * white = vertex_color.",
      "Use the lower-level overload with explicit blend mode: var verts = SKVertices.CreateCopy(SKVertexMode.Triangles, vertices, null, colors); canvas.DrawVertices(verts, SKBlendMode.Dst, paint);"
    ],
    "nextQuestions": [
      "Should the convenience overloads change their default blend mode to Dst? This would fix behavior but change ABI semantics for callers using mixed shader+vertex-color scenarios.",
      "Does the same regression affect DrawPatch which also defaults to SKBlendMode.Modulate (SKCanvas.cs line 1018)?",
      "Is there a v2-to-v3 migration guide that should document this behavioral change?"
    ],
    "resolution": {
      "hypothesis": "Upstream Skia m116+ changed how drawVertices blends vertex colors with the paint. SKBlendMode.Modulate now multiplies vertex colors by the paint's shader/color, which defaults to black, giving black output. SkiaSharp convenience overloads need to either use a different default blend mode (Dst) or set a white paint color as the baseline.",
      "proposals": [
        {
          "title": "Change convenience overload default blend mode to Dst",
          "description": "Update the three SKCanvas.DrawVertices convenience overloads to use SKBlendMode.Dst instead of SKBlendMode.Modulate. This makes vertex colors render directly without requiring a paint shader, matching v2.88.9 behavior.",
          "codeSnippet": "public void DrawVertices (SKVertexMode vmode, SKPoint[] vertices, SKColor[] colors, SKPaint paint)\n{\n    var vert = SKVertices.CreateCopy (vmode, vertices, colors);\n    DrawVertices (vert, SKBlendMode.Dst, paint);\n}",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Add migration documentation",
          "description": "Document the DrawVertices blend mode behavior change in the v2-to-v3 migration guide, explaining the need for a white shader or SKBlendMode.Dst when using vertex colors.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Change convenience overload default blend mode to Dst",
      "recommendedReason": "This restores v2.88.9 behavior for all callers using the convenience overloads, is a minimal change, and matches how the Gallery sample already correctly renders vertices in v3."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "Real regression with full repro code, clear root cause identified (upstream Skia blend mode change affecting convenience overloads), and a clear fix path. Needs decision on whether to change blend mode default or just add migration docs.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, SkiaSharp core area, Windows platform, Raster backend, and compatibility tenet labels",
        "risk": "low",
        "confidence": 0.93,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/Raster",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Confirm regression, explain root cause, provide two workarounds",
        "risk": "medium",
        "confidence": 0.87,
        "comment": "Thanks for the detailed reproduction case with screenshots!\n\nThis is a regression caused by an upstream Skia behavioral change in m116. In newer Skia, `DrawVertices` with `SKBlendMode.Modulate` (the default used by the convenience overloads) multiplies vertex colors by the paint's shader/color. Since the default paint has no shader and defaults to black, the result is `vertex_color × black = black`.\n\nThere are two workarounds available today:\n\n**Option 1** — Set a white shader on the paint (as suggested by @molesmoke):\n```csharp\nvar paint = new SKPaint\n{\n    IsAntialias = true,\n    Shader = SKShader.CreateColor(SKColors.White)\n};\nsurface.Canvas.DrawVertices(SKVertexMode.Triangles, vertices, colors, paint);\n```\nThis works because `vertex_color × white = vertex_color`.\n\n**Option 2** — Use `SKBlendMode.Dst` with the lower-level overload:\n```csharp\nvar paint = new SKPaint { IsAntialias = true };\nvar verts = SKVertices.CreateCopy(SKVertexMode.Triangles, vertices, null, colors);\nsurface.Canvas.DrawVertices(verts, SKBlendMode.Dst, paint);\n```\nThis is the approach used in the [Gallery samples](https://github.com/mono/SkiaSharp/blob/main/samples/Gallery/Shared/Samples/VertexMeshSample.cs#L97).\n\nWe are investigating whether to update the convenience overloads to use `SKBlendMode.Dst` by default to restore v2.88.9 behavior."
      }
    ]
  }
}
```

</details>
