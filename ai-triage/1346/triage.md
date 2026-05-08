# Issue Triage Report — #1346

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T02:47:27Z |
| Type | type/question (0.95 (95%)) |
| Area | area/SkiaSharp.Views.Forms (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** Reporter asks why SKGLView always clears the entire canvas on InvalidateSurface while SKCanvasView retains previous drawing; maintainer already answered that GL always requires a full clear and SKCanvasView not clearing is actually a bug.

**Analysis:** Reporter is confused by the difference in behavior between SKGLView (which always clears the GL buffer before drawing) and SKCanvasView (which does not explicitly clear, retaining previous frame content). The SKGLView behavior is correct for OpenGL — the framebuffer must be cleared each frame. The SKCanvasView appearing to retain state is actually a bug: it relies on the underlying Android View's bitmap not being cleared between frames, which can fail on orientation change, app resume, or with transparency/antialiasing. The maintainer already explained this fully in comment #647680207.

**Recommendations:** **close-as-not-a-bug** — SKGLView clearing the framebuffer on every InvalidateSurface is correct OpenGL behavior. The maintainer already provided a full explanation and workaround. SKCanvasView retaining state is the bug, not SKGLView. Issue can be closed with the existing comment as resolution.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp.Views.Forms |
| Platforms | os/Android |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Create a Xamarin.Forms app using SKGLView
2. Draw spokes to a circle incrementally, calling InvalidateSurface each time
3. Observe that the entire canvas is cleared on each InvalidateSurface call

**Environment:** Android emulator, Visual Studio 2019 16.6.2, Xamarin.Forms 4.6.0.847, SkiaSharp.Views.Forms 1.68.3, SkiaSharp 1.68.3

**Related issues:** #1345

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | The behavior described (GL always clears, raster does not) is by design and documented in maintainer comment. SKCanvasView not clearing is the bug; SKGLView clearing is correct GL behavior. The Xamarin.Forms package is also obsolete. |

## Analysis

### Technical Summary

Reporter is confused by the difference in behavior between SKGLView (which always clears the GL buffer before drawing) and SKCanvasView (which does not explicitly clear, retaining previous frame content). The SKGLView behavior is correct for OpenGL — the framebuffer must be cleared each frame. The SKCanvasView appearing to retain state is actually a bug: it relies on the underlying Android View's bitmap not being cleared between frames, which can fail on orientation change, app resume, or with transparency/antialiasing. The maintainer already explained this fully in comment #647680207.

### Rationale

Classified as type/question because the reporter is asking 'how to solve this?' rather than reporting broken behavior. The GL surface clearing behavior is correct and documented as intentional. SKCanvasView retaining state is explicitly called out as a bug by the maintainer. No code change is required. Suggested action is close-as-not-a-bug with workaround already provided by maintainer.

### Key Signals

- "the whole canvas gets completly redrawn(cleared) with every call to ... SKGLView.InvalidateSurface()" — **issue body** (Reporter observes correct GL behavior (full clear each frame) and expects partial update behavior instead.)
- "SKCanvasView redraws the canvas at InvalidateSurface but i was under the impression that the functionality was the same" — **issue body** (Reporter assumes both views should behave identically; in fact they use different rendering backends with different clearing semantics.)
- "what you are seeing is that the GL view is always cleared, and the canvas view is not (technically a bug)" — **comment #647680207 (mattleibow, collaborator)** (Maintainer explicitly states SKGLView clearing is correct, SKCanvasView NOT clearing is the bug.)
- "the correct way is to always clear the canvas before drawing" — **comment #647680207 (mattleibow, collaborator)** (Official best-practice guidance: always clear before drawing in PaintSurface, regardless of view type.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceViewRenderer.cs` | 34 | direct | OnDrawFrame calls GLES20.GlClear(GlColorBufferBit | GlDepthBufferBit | GlStencilBufferBit) unconditionally at the start of every frame — confirms that SKGLView always fully clears the framebuffer before the user's PaintSurface handler runs. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs` | 77-80 | related | SKCanvasView.OnDraw does not issue an explicit clear of the backing bitmap before calling the PaintSurface handler — the Android View system may or may not retain previous pixel data between draws. This is the 'bug' the maintainer refers to: relying on retained pixels is undefined behavior. |

### Workarounds

- Always call canvas.Clear() (or canvas.Clear(SKColors.Transparent)) at the start of every PaintSurface handler — works correctly with both SKGLView and SKCanvasView.
- For performance with incremental drawing, maintain an off-screen SKBitmap as a back-buffer. Draw incrementally into the bitmap, then use canvas.DrawBitmap() to blit the entire bitmap to the screen surface on each frame.

### Resolution Proposals

**Hypothesis:** The user expects partial/retained rendering from SKGLView, but OpenGL always requires a full clear each frame. The correct pattern is to always clear and redraw fully, using an off-screen bitmap cache for expensive backgrounds.

1. **Always clear and use bitmap back-buffer** — workaround, confidence 0.90 (90%), cost/s, validated=yes
   - Always call canvas.Clear() at the start of PaintSurface. Maintain an SKBitmap for the accumulating circle drawing. Draw each new spoke onto the bitmap, then call canvas.DrawBitmap(bitmap, 0, 0) to render it. This works reliably with both SKGLView and SKCanvasView.

**Recommended proposal:** Always clear and use bitmap back-buffer

**Why:** Matches the official guidance from the maintainer. Portable across view types and handles orientation changes correctly.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | SKGLView clearing the framebuffer on every InvalidateSurface is correct OpenGL behavior. The maintainer already provided a full explanation and workaround. SKCanvasView retaining state is the bug, not SKGLView. Issue can be closed with the existing comment as resolution. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply question, views-forms, android labels | labels=type/question, area/SkiaSharp.Views.Forms, os/Android |
| add-comment | high | 0.88 (88%) | Post closing comment summarizing the answer and pointing to the back-buffer pattern | — |
| close-issue | medium | 0.85 (85%) | Close as not a bug — SKGLView behavior is correct, answer already provided by maintainer | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
As noted by @mattleibow above, the `SKGLView` behavior (clearing the framebuffer on every `InvalidateSurface`) is **correct** — OpenGL always requires a full clear before each frame. The `SKCanvasView` appearing to retain previous pixels is actually undefined behavior (a bug that was planned to be fixed in v2).

The recommended approach is:

1. **Always clear** at the start of your `PaintSurface` handler:
   ```csharp
   void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
   {
       var canvas = e.Surface.Canvas;
       canvas.Clear();
       // your drawing code here
   }
   ```

2. **For caching incremental drawing** (e.g., a circle being filled spoke-by-spoke), maintain an off-screen `SKBitmap`:
   ```csharp
   private SKBitmap _backBuffer;

   void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
   {
       var canvas = e.Surface.Canvas;
       var info = e.Info;

       // Initialize or resize back-buffer
       if (_backBuffer == null || _backBuffer.Width != info.Width || _backBuffer.Height != info.Height)
           _backBuffer = new SKBitmap(info.Width, info.Height);

       // Draw new spoke onto back-buffer
       using (var bbCanvas = new SKCanvas(_backBuffer))
       {
           // draw only the new spoke here
       }

       // Blit the entire back-buffer to screen
       canvas.Clear();
       canvas.DrawBitmap(_backBuffer, 0, 0);
   }
   ```

This pattern works correctly with both `SKGLView` and `SKCanvasView` and survives orientation changes.

Closing as by-design — if you have further questions about the drawing pattern, please open a new discussion.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1346,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T02:47:27Z"
  },
  "summary": "Reporter asks why SKGLView always clears the entire canvas on InvalidateSurface while SKCanvasView retains previous drawing; maintainer already answered that GL always requires a full clear and SKCanvasView not clearing is actually a bug.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views.Forms",
      "confidence": 0.9
    },
    "platforms": [
      "os/Android"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Xamarin.Forms app using SKGLView",
        "Draw spokes to a circle incrementally, calling InvalidateSurface each time",
        "Observe that the entire canvas is cleared on each InvalidateSurface call"
      ],
      "environmentDetails": "Android emulator, Visual Studio 2019 16.6.2, Xamarin.Forms 4.6.0.847, SkiaSharp.Views.Forms 1.68.3, SkiaSharp 1.68.3",
      "relatedIssues": [
        1345
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.3"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "The behavior described (GL always clears, raster does not) is by design and documented in maintainer comment. SKCanvasView not clearing is the bug; SKGLView clearing is correct GL behavior. The Xamarin.Forms package is also obsolete."
    }
  },
  "analysis": {
    "summary": "Reporter is confused by the difference in behavior between SKGLView (which always clears the GL buffer before drawing) and SKCanvasView (which does not explicitly clear, retaining previous frame content). The SKGLView behavior is correct for OpenGL — the framebuffer must be cleared each frame. The SKCanvasView appearing to retain state is actually a bug: it relies on the underlying Android View's bitmap not being cleared between frames, which can fail on orientation change, app resume, or with transparency/antialiasing. The maintainer already explained this fully in comment #647680207.",
    "rationale": "Classified as type/question because the reporter is asking 'how to solve this?' rather than reporting broken behavior. The GL surface clearing behavior is correct and documented as intentional. SKCanvasView retaining state is explicitly called out as a bug by the maintainer. No code change is required. Suggested action is close-as-not-a-bug with workaround already provided by maintainer.",
    "keySignals": [
      {
        "text": "the whole canvas gets completly redrawn(cleared) with every call to ... SKGLView.InvalidateSurface()",
        "source": "issue body",
        "interpretation": "Reporter observes correct GL behavior (full clear each frame) and expects partial update behavior instead."
      },
      {
        "text": "SKCanvasView redraws the canvas at InvalidateSurface but i was under the impression that the functionality was the same",
        "source": "issue body",
        "interpretation": "Reporter assumes both views should behave identically; in fact they use different rendering backends with different clearing semantics."
      },
      {
        "text": "what you are seeing is that the GL view is always cleared, and the canvas view is not (technically a bug)",
        "source": "comment #647680207 (mattleibow, collaborator)",
        "interpretation": "Maintainer explicitly states SKGLView clearing is correct, SKCanvasView NOT clearing is the bug."
      },
      {
        "text": "the correct way is to always clear the canvas before drawing",
        "source": "comment #647680207 (mattleibow, collaborator)",
        "interpretation": "Official best-practice guidance: always clear before drawing in PaintSurface, regardless of view type."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceViewRenderer.cs",
        "lines": "34",
        "finding": "OnDrawFrame calls GLES20.GlClear(GlColorBufferBit | GlDepthBufferBit | GlStencilBufferBit) unconditionally at the start of every frame — confirms that SKGLView always fully clears the framebuffer before the user's PaintSurface handler runs.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs",
        "lines": "77-80",
        "finding": "SKCanvasView.OnDraw does not issue an explicit clear of the backing bitmap before calling the PaintSurface handler — the Android View system may or may not retain previous pixel data between draws. This is the 'bug' the maintainer refers to: relying on retained pixels is undefined behavior.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Always call canvas.Clear() (or canvas.Clear(SKColors.Transparent)) at the start of every PaintSurface handler — works correctly with both SKGLView and SKCanvasView.",
      "For performance with incremental drawing, maintain an off-screen SKBitmap as a back-buffer. Draw incrementally into the bitmap, then use canvas.DrawBitmap() to blit the entire bitmap to the screen surface on each frame."
    ],
    "resolution": {
      "hypothesis": "The user expects partial/retained rendering from SKGLView, but OpenGL always requires a full clear each frame. The correct pattern is to always clear and redraw fully, using an off-screen bitmap cache for expensive backgrounds.",
      "proposals": [
        {
          "title": "Always clear and use bitmap back-buffer",
          "description": "Always call canvas.Clear() at the start of PaintSurface. Maintain an SKBitmap for the accumulating circle drawing. Draw each new spoke onto the bitmap, then call canvas.DrawBitmap(bitmap, 0, 0) to render it. This works reliably with both SKGLView and SKCanvasView.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Always clear and use bitmap back-buffer",
      "recommendedReason": "Matches the official guidance from the maintainer. Portable across view types and handles orientation changes correctly."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "SKGLView clearing the framebuffer on every InvalidateSurface is correct OpenGL behavior. The maintainer already provided a full explanation and workaround. SKCanvasView retaining state is the bug, not SKGLView. Issue can be closed with the existing comment as resolution.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question, views-forms, android labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/question",
          "area/SkiaSharp.Views.Forms",
          "os/Android"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post closing comment summarizing the answer and pointing to the back-buffer pattern",
        "risk": "high",
        "confidence": 0.88,
        "comment": "As noted by @mattleibow above, the `SKGLView` behavior (clearing the framebuffer on every `InvalidateSurface`) is **correct** — OpenGL always requires a full clear before each frame. The `SKCanvasView` appearing to retain previous pixels is actually undefined behavior (a bug that was planned to be fixed in v2).\n\nThe recommended approach is:\n\n1. **Always clear** at the start of your `PaintSurface` handler:\n   ```csharp\n   void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)\n   {\n       var canvas = e.Surface.Canvas;\n       canvas.Clear();\n       // your drawing code here\n   }\n   ```\n\n2. **For caching incremental drawing** (e.g., a circle being filled spoke-by-spoke), maintain an off-screen `SKBitmap`:\n   ```csharp\n   private SKBitmap _backBuffer;\n\n   void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)\n   {\n       var canvas = e.Surface.Canvas;\n       var info = e.Info;\n\n       // Initialize or resize back-buffer\n       if (_backBuffer == null || _backBuffer.Width != info.Width || _backBuffer.Height != info.Height)\n           _backBuffer = new SKBitmap(info.Width, info.Height);\n\n       // Draw new spoke onto back-buffer\n       using (var bbCanvas = new SKCanvas(_backBuffer))\n       {\n           // draw only the new spoke here\n       }\n\n       // Blit the entire back-buffer to screen\n       canvas.Clear();\n       canvas.DrawBitmap(_backBuffer, 0, 0);\n   }\n   ```\n\nThis pattern works correctly with both `SKGLView` and `SKCanvasView` and survives orientation changes.\n\nClosing as by-design — if you have further questions about the drawing pattern, please open a new discussion."
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — SKGLView behavior is correct, answer already provided by maintainer",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
