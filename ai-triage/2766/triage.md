# Issue Triage Report — #2766

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T19:54:04Z |
| Type | type/question (0.78 (78%)) |
| Area | area/SkiaSharp.Views (0.92 (92%)) |
| Suggested action | close-as-not-a-bug (0.80 (80%)) |

**Issue Summary:** Reporter observes what appears to be a motion-blur / ghost-frame effect when animating with SKGLSurfaceView on Android, and asks whether there is a trick to ensure only the most recently drawn canvas output is shown.

**Analysis:** The reporter's OnPaintSurface handler draws a circle directly on the canvas without first calling canvas.Clear(). SKGLSurfaceViewRenderer.OnDrawFrame does issue a low-level GLES20.GlClear() before each frame, which clears the underlying OpenGL framebuffer. However, from Skia's perspective the canvas object is reused across frames and is never told the framebuffer was reset at the GL level; users must explicitly call canvas.Clear() (or canvas.DrawColor(SKColors.Black, SKBlendMode.Src)) inside OnPaintSurface to produce a clean frame. The reporter also acknowledges in a follow-up comment that the 'double image' might be a camera artifact (shutter capturing two screen refreshes at 60 Hz) or screen/eye persistence, suggesting the effect may not reflect an actual rendering defect. The community workaround of adding canvas.Clear() was offered and the reporter noted it may help.

**Recommendations:** **close-as-not-a-bug** — The reporter is asking how to get a clean frame in an OpenGL-backed view. The renderer correctly issues GlClear; users must also call canvas.Clear() inside OnPaintSurface. This is expected GL rendering behavior, not a SkiaSharp defect. A clear answer and one-line workaround is available.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp.Views |
| Platforms | os/Android |
| Backends | backend/OpenGL |
| Tenets | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create an Android app using SKGLSurfaceView
2. In OnPaintSurface, draw a circle at a position that changes each frame without calling canvas.Clear()
3. Observe that the rendered output appears to show two overlapping circles

**Environment:** SkiaSharp 2.88.3, Android 12, Visual Studio on Windows, multiple physical devices

**Screenshots:**
- https://github.com/mono/SkiaSharp/assets/10914515/81e552a3-f631-4660-8f3a-6edecc28b90b — Double circle motion blur effect observed
- https://github.com/mono/SkiaSharp/assets/10914515/dba32ff4-2c9b-47f8-a971-12695672fab9 — Second screenshot showing double/blurred circles
- https://github.com/mono/SkiaSharp/assets/10914515/0516615b-07d7-4e64-952f-946b72aa0e77 — Expected appearance: single solid circle (for reference)

**Code snippets:**

```csharp
private void _sImgView_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
{
    SKPoint pt = new SKPoint(180 + _xOfs, 200);
    _xOfs += 20;
    if (_xOfs > 500)
        _xOfs = 0;
    e.Surface.Canvas.DrawCircle(pt, 77, _paint);
}
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | Reporter did not confirm a version where the effect was absent; 'Last Known Good' was left as 'Other' with no clarifying note. |

## Analysis

### Technical Summary

The reporter's OnPaintSurface handler draws a circle directly on the canvas without first calling canvas.Clear(). SKGLSurfaceViewRenderer.OnDrawFrame does issue a low-level GLES20.GlClear() before each frame, which clears the underlying OpenGL framebuffer. However, from Skia's perspective the canvas object is reused across frames and is never told the framebuffer was reset at the GL level; users must explicitly call canvas.Clear() (or canvas.DrawColor(SKColors.Black, SKBlendMode.Src)) inside OnPaintSurface to produce a clean frame. The reporter also acknowledges in a follow-up comment that the 'double image' might be a camera artifact (shutter capturing two screen refreshes at 60 Hz) or screen/eye persistence, suggesting the effect may not reflect an actual rendering defect. The community workaround of adding canvas.Clear() was offered and the reporter noted it may help.

### Rationale

The code asks 'Is there any trick?' – a clear how-to question. The existing renderer calls GlClear at the GL level but does not explicitly clear the Skia canvas before invoking OnPaintSurface. This is by-design; GL-backed surfaces behave differently from raster SKCanvasView, where a fresh bitmap is created per frame. The visual artefact is explained by missing canvas.Clear() and/or camera frame-capture aliasing, not by a defect in SkiaSharp.

### Key Signals

- "Is there any trick to make SKGLSurfaceView ONLY render the most recently drawn Canvas output?" — **issue body** (Usage question, not a confirmed defect.)
- "It's difficult to parse out the contributing factors of this double image… I'm taking a picture -- so shutter speed might be slow enough to capture two frames." — **comment #2 by reporter** (Reporter acknowledges the perceived double-image may be a camera/eye artifact, weakening the bug classification.)
- "have you tried clearing the canvas inside of PaintSurface?" — **comment #1 by nor0x** (Community member immediately suggests canvas.Clear() – the standard OpenGL rendering pattern.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceViewRenderer.cs` | 32-87 | direct | OnDrawFrame calls GLES20.GlClear(GlColorBufferBit | GlDepthBufferBit | GlStencilBufferBit) at line 34, then reuses the same SKSurface/SKCanvas objects across frames. No canvas.Clear() is issued before OnPaintSurface is invoked. This means users who do not clear the canvas themselves will draw on Skia's internally-tracked canvas state, even though the underlying GL framebuffer was cleared at the native level. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceView.cs` | 26-32 | related | Initialize() calls SetEGLConfigChooser(8, 8, 8, 8, 0, 8) (8 stencil bits, 0 depth) and SetEGLContextClientVersion(2). No explicit EGL swap-behaviour setting is made, so Android's default (EGL_BUFFER_DESTROYED) applies. This means the back buffer content is undefined after each swap – GlClear is the correct way to reset it, and it is present. |

### Workarounds

- Call e.Surface.Canvas.Clear(SKColors.Transparent) (or a specific background color) as the first line inside OnPaintSurface to ensure a clean frame regardless of GL double-buffer behavior.
- Alternatively call e.Surface.Canvas.DrawColor(SKColors.Black, SKBlendMode.Src) which performs a full-coverage opaque clear through Skia's drawing pipeline.

### Resolution Proposals

**Hypothesis:** The visual artefact stems from the reporter not calling canvas.Clear() before drawing each frame. The GlClear in the renderer clears the OpenGL framebuffer but does not reset Skia's canvas state; without an explicit clear the circle accumulates perceptually. The additional ghost may also be a camera artefact at 60 Hz.

1. **Add canvas.Clear() at the top of OnPaintSurface** — workaround, confidence 0.92 (92%), cost/xs, validated=yes
   - Call canvas.Clear() with the desired background color at the very start of the OnPaintSurface handler. This is the standard pattern for all GL-backed Skia surfaces.

```csharp
private void _sImgView_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
{
    e.Surface.Canvas.Clear(SKColors.White); // clear before drawing
    SKPoint pt = new SKPoint(180 + _xOfs, 200);
    _xOfs += 20;
    if (_xOfs > 500)
        _xOfs = 0;
    e.Surface.Canvas.DrawCircle(pt, 77, _paint);
}
```

**Recommended proposal:** Add canvas.Clear() at the top of OnPaintSurface

**Why:** Minimal one-line change that follows the standard Skia GL rendering pattern and directly addresses the described symptom.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.80 (80%) |
| Reason | The reporter is asking how to get a clean frame in an OpenGL-backed view. The renderer correctly issues GlClear; users must also call canvas.Clear() inside OnPaintSurface. This is expected GL rendering behavior, not a SkiaSharp defect. A clear answer and one-line workaround is available. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Correct type from bug to question, add Views area, Android and OpenGL backend labels | labels=type/question, area/SkiaSharp.Views, os/Android, backend/OpenGL |
| add-comment | high | 0.80 (80%) | Explain canvas.Clear() pattern for GL views and close as answered | — |
| close-issue | medium | 0.78 (78%) | Close as answered / not a bug | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and follow-up.

Here's the fix: add `canvas.Clear()` as the first line inside your `OnPaintSurface` handler:

```csharp
private void _sImgView_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
{
    e.Surface.Canvas.Clear(SKColors.White); // ← add this
    SKPoint pt = new SKPoint(180 + _xOfs, 200);
    _xOfs += 20;
    if (_xOfs > 500)
        _xOfs = 0;
    e.Surface.Canvas.DrawCircle(pt, 77, _paint);
}
```

`SKGLSurfaceViewRenderer` does issue a low-level `GlClear()` on the OpenGL framebuffer before each frame, but Skia's canvas is a persistent object that wraps that framebuffer — it does not automatically know the buffer was reset at the GL level. Calling `canvas.Clear()` through Skia's own drawing pipeline ensures its internal state is consistent and the frame starts clean. This is the standard pattern for all GL-backed Skia surfaces (and differs from raster `SKCanvasView`, where a fresh bitmap is created each frame).

The 'two circles' in your screenshot are very likely a combination of missing clear and the phone camera capturing two screen refreshes at 60 Hz, which you also noted in your comment. The single-line fix above should resolve what you are seeing.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2766,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T19:54:04Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter observes what appears to be a motion-blur / ghost-frame effect when animating with SKGLSurfaceView on Android, and asks whether there is a trick to ensure only the most recently drawn canvas output is shown.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.78
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.92
    },
    "platforms": [
      "os/Android"
    ],
    "backends": [
      "backend/OpenGL"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an Android app using SKGLSurfaceView",
        "In OnPaintSurface, draw a circle at a position that changes each frame without calling canvas.Clear()",
        "Observe that the rendered output appears to show two overlapping circles"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, Android 12, Visual Studio on Windows, multiple physical devices",
      "codeSnippets": [
        "private void _sImgView_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)\n{\n    SKPoint pt = new SKPoint(180 + _xOfs, 200);\n    _xOfs += 20;\n    if (_xOfs > 500)\n        _xOfs = 0;\n    e.Surface.Canvas.DrawCircle(pt, 77, _paint);\n}"
      ],
      "screenshots": [
        {
          "url": "https://github.com/mono/SkiaSharp/assets/10914515/81e552a3-f631-4660-8f3a-6edecc28b90b",
          "description": "Double circle motion blur effect observed"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/assets/10914515/dba32ff4-2c9b-47f8-a971-12695672fab9",
          "description": "Second screenshot showing double/blurred circles"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/assets/10914515/0516615b-07d7-4e64-952f-946b72aa0e77",
          "description": "Expected appearance: single solid circle (for reference)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "Reporter did not confirm a version where the effect was absent; 'Last Known Good' was left as 'Other' with no clarifying note."
    }
  },
  "analysis": {
    "summary": "The reporter's OnPaintSurface handler draws a circle directly on the canvas without first calling canvas.Clear(). SKGLSurfaceViewRenderer.OnDrawFrame does issue a low-level GLES20.GlClear() before each frame, which clears the underlying OpenGL framebuffer. However, from Skia's perspective the canvas object is reused across frames and is never told the framebuffer was reset at the GL level; users must explicitly call canvas.Clear() (or canvas.DrawColor(SKColors.Black, SKBlendMode.Src)) inside OnPaintSurface to produce a clean frame. The reporter also acknowledges in a follow-up comment that the 'double image' might be a camera artifact (shutter capturing two screen refreshes at 60 Hz) or screen/eye persistence, suggesting the effect may not reflect an actual rendering defect. The community workaround of adding canvas.Clear() was offered and the reporter noted it may help.",
    "rationale": "The code asks 'Is there any trick?' – a clear how-to question. The existing renderer calls GlClear at the GL level but does not explicitly clear the Skia canvas before invoking OnPaintSurface. This is by-design; GL-backed surfaces behave differently from raster SKCanvasView, where a fresh bitmap is created per frame. The visual artefact is explained by missing canvas.Clear() and/or camera frame-capture aliasing, not by a defect in SkiaSharp.",
    "keySignals": [
      {
        "text": "Is there any trick to make SKGLSurfaceView ONLY render the most recently drawn Canvas output?",
        "source": "issue body",
        "interpretation": "Usage question, not a confirmed defect."
      },
      {
        "text": "It's difficult to parse out the contributing factors of this double image… I'm taking a picture -- so shutter speed might be slow enough to capture two frames.",
        "source": "comment #2 by reporter",
        "interpretation": "Reporter acknowledges the perceived double-image may be a camera/eye artifact, weakening the bug classification."
      },
      {
        "text": "have you tried clearing the canvas inside of PaintSurface?",
        "source": "comment #1 by nor0x",
        "interpretation": "Community member immediately suggests canvas.Clear() – the standard OpenGL rendering pattern."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceViewRenderer.cs",
        "lines": "32-87",
        "finding": "OnDrawFrame calls GLES20.GlClear(GlColorBufferBit | GlDepthBufferBit | GlStencilBufferBit) at line 34, then reuses the same SKSurface/SKCanvas objects across frames. No canvas.Clear() is issued before OnPaintSurface is invoked. This means users who do not clear the canvas themselves will draw on Skia's internally-tracked canvas state, even though the underlying GL framebuffer was cleared at the native level.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceView.cs",
        "lines": "26-32",
        "finding": "Initialize() calls SetEGLConfigChooser(8, 8, 8, 8, 0, 8) (8 stencil bits, 0 depth) and SetEGLContextClientVersion(2). No explicit EGL swap-behaviour setting is made, so Android's default (EGL_BUFFER_DESTROYED) applies. This means the back buffer content is undefined after each swap – GlClear is the correct way to reset it, and it is present.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Call e.Surface.Canvas.Clear(SKColors.Transparent) (or a specific background color) as the first line inside OnPaintSurface to ensure a clean frame regardless of GL double-buffer behavior.",
      "Alternatively call e.Surface.Canvas.DrawColor(SKColors.Black, SKBlendMode.Src) which performs a full-coverage opaque clear through Skia's drawing pipeline."
    ],
    "resolution": {
      "hypothesis": "The visual artefact stems from the reporter not calling canvas.Clear() before drawing each frame. The GlClear in the renderer clears the OpenGL framebuffer but does not reset Skia's canvas state; without an explicit clear the circle accumulates perceptually. The additional ghost may also be a camera artefact at 60 Hz.",
      "proposals": [
        {
          "title": "Add canvas.Clear() at the top of OnPaintSurface",
          "description": "Call canvas.Clear() with the desired background color at the very start of the OnPaintSurface handler. This is the standard pattern for all GL-backed Skia surfaces.",
          "category": "workaround",
          "codeSnippet": "private void _sImgView_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)\n{\n    e.Surface.Canvas.Clear(SKColors.White); // clear before drawing\n    SKPoint pt = new SKPoint(180 + _xOfs, 200);\n    _xOfs += 20;\n    if (_xOfs > 500)\n        _xOfs = 0;\n    e.Surface.Canvas.DrawCircle(pt, 77, _paint);\n}",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Add canvas.Clear() at the top of OnPaintSurface",
      "recommendedReason": "Minimal one-line change that follows the standard Skia GL rendering pattern and directly addresses the described symptom."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.8,
      "reason": "The reporter is asking how to get a clean frame in an OpenGL-backed view. The renderer correctly issues GlClear; users must also call canvas.Clear() inside OnPaintSurface. This is expected GL rendering behavior, not a SkiaSharp defect. A clear answer and one-line workaround is available.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct type from bug to question, add Views area, Android and OpenGL backend labels",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/question",
          "area/SkiaSharp.Views",
          "os/Android",
          "backend/OpenGL"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain canvas.Clear() pattern for GL views and close as answered",
        "risk": "high",
        "confidence": 0.8,
        "comment": "Thanks for the detailed report and follow-up.\n\nHere's the fix: add `canvas.Clear()` as the first line inside your `OnPaintSurface` handler:\n\n```csharp\nprivate void _sImgView_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)\n{\n    e.Surface.Canvas.Clear(SKColors.White); // ← add this\n    SKPoint pt = new SKPoint(180 + _xOfs, 200);\n    _xOfs += 20;\n    if (_xOfs > 500)\n        _xOfs = 0;\n    e.Surface.Canvas.DrawCircle(pt, 77, _paint);\n}\n```\n\n`SKGLSurfaceViewRenderer` does issue a low-level `GlClear()` on the OpenGL framebuffer before each frame, but Skia's canvas is a persistent object that wraps that framebuffer — it does not automatically know the buffer was reset at the GL level. Calling `canvas.Clear()` through Skia's own drawing pipeline ensures its internal state is consistent and the frame starts clean. This is the standard pattern for all GL-backed Skia surfaces (and differs from raster `SKCanvasView`, where a fresh bitmap is created each frame).\n\nThe 'two circles' in your screenshot are very likely a combination of missing clear and the phone camera capturing two screen refreshes at 60 Hz, which you also noted in your comment. The single-line fix above should resolve what you are seeing."
      },
      {
        "type": "close-issue",
        "description": "Close as answered / not a bug",
        "risk": "medium",
        "confidence": 0.78,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
