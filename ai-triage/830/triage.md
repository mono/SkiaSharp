# Issue Triage Report — #830

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T15:06:47Z |
| Type | type/question (0.80 (80%)) |
| Area | area/SkiaSharp.Views (0.82 (82%)) |
| Suggested action | close-as-not-a-bug (0.78 (78%)) |

**Issue Summary:** Reporter asks how to enable subpixel (ClearType) text antialiasing on Windows using SkiaSharp WinForms views; the issue evolved to reveal that subpixel rendering regressed in 2.88.x because Skia changed the default PixelGeometry to Unknown, and the views API provides no way to pass SKSurfaceProperties to opt back in.

**Analysis:** Subpixel (ClearType) text rendering requires two conditions: (1) SKFont.Edging = SubpixelAntialias, and (2) the surface must be created with SKSurfaceProperties(SKPixelGeometry.RgbHorizontal). The SkiaSharp views (SKControl, SKElement) create surfaces without SKSurfaceProperties, so PixelGeometry defaults to Unknown and subpixel rendering is silently disabled. This was not a problem until 2.88.x when Skia changed the default. A workaround exists (render to an off-screen SKSurface with the correct properties), but the views need a SurfaceProperties property to expose this cleanly.

**Recommendations:** **close-as-not-a-bug** — The original question is answered: subpixel rendering requires SKFontEdging.SubpixelAntialias and a surface created with SKPixelGeometry.RgbHorizontal. The behavior change in 2.88.x was upstream Skia design change, not a SkiaSharp bug. A workaround exists. The related enhancement (exposing SurfaceProperties on views) should be tracked as a separate feature request.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/question |

## Evidence

### Reproduction

1. Create a WinForms app with SKControl/SKCanvasView
2. Set SKPaint.SubpixelText = true and SKFont.Edging = SubpixelAntialias
3. Draw text — subpixel (ClearType) rendering does not appear, only grayscale AA

**Environment:** Windows, SkiaSharp 2.88.x, WinForms SKControl

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2308 — Follow-up bug: subpixel rendering regressed in 2.88.x vs 2.80.x

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2, 2.80.3, 2.80.4, 2.88.0-preview, 2.88.3 |
| Worked in | 2.80.4 |
| Broke in | 2.88.0-preview |
| Current relevance | likely |
| Relevance reason | The views still create surfaces without SKSurfaceProperties; no fix has been shipped for the missing API to configure PixelGeometry on SKControl/SKElement. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.85 (85%) |
| Reason | Multiple commenters confirmed subpixel rendering worked in 2.80.x and stopped working in 2.88.x. Root cause identified: Skia upstream changed default PixelGeometry from RgbHorizontal to Unknown (https://bugs.chromium.org/p/skia/issues/detail?id=3934). |
| Worked in version | 2.80.4 |
| Broke in version | 2.88.0-preview |

## Analysis

### Technical Summary

Subpixel (ClearType) text rendering requires two conditions: (1) SKFont.Edging = SubpixelAntialias, and (2) the surface must be created with SKSurfaceProperties(SKPixelGeometry.RgbHorizontal). The SkiaSharp views (SKControl, SKElement) create surfaces without SKSurfaceProperties, so PixelGeometry defaults to Unknown and subpixel rendering is silently disabled. This was not a problem until 2.88.x when Skia changed the default. A workaround exists (render to an off-screen SKSurface with the correct properties), but the views need a SurfaceProperties property to expose this cleanly.

### Rationale

The original title labels it a question, but the comments reveal it is both a usage question (answered: SKFontEdging.SubpixelAntialias) and an enhancement gap (views don't expose SKSurfaceProperties). The regression noted in 2.88.x is real — caused by an upstream Skia change in PixelGeometry default. Related issue #2308 was closed 'completed' after a workaround was documented, but the views still lack a first-class API for this. Classified type/question because the issue title and body are usage questions and a workaround exists; the views enhancement gap is tracked as a separate observation.

### Key Signals

- "SubpixelText = true ... Neither DrawText nor DrawShapedText gave us the subpixel rendering" — **issue body** (Reporter used the old SKPaint.SubpixelText API which is now obsolete; the correct approach is SKFont.Edging = SubpixelAntialias plus the correct surface PixelGeometry)
- "ClearType is only possible on surfaces that don't use premultiplied alpha" — **comment by Gillibald** (Early hint at the surface properties requirement; partially correct — the key is PixelGeometry, not just alpha)
- "The PixelGeometry on created surface is now defaulting to Unknown instead of RgbHorizontal. To bring back the default behaviour the SKSurface ctor must be provided SKSurfaceProperties with SKPixelGeometry.RgbHorizontal" — **comment by themcoo on issue #2308** (Root cause identified: Skia upstream change broke the default; workaround is to pass SKSurfaceProperties)
- "2.80.2: Subpixel displayed ok. 2.88.0-preview.x: Subpixel is NOT displayed ok." — **comment by danipen** (Regression confirmed across multiple SkiaSharp versions)
- "I don't see a possibility currently to hook up with different pixel geometry" — **comment by themcoo on issue #2308** (The views lack a first-class API to configure SKSurfaceProperties/PixelGeometry — an enhancement gap remains)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKControl.cs` | 42 | direct | SKSurface.Create(info, data.Scan0, data.Stride) is called without SKSurfaceProperties — no PixelGeometry is specified, so it defaults to Unknown, disabling subpixel AA |
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs` | 73 | direct | SKSurface.Create(info, bitmap.BackBuffer, bitmap.BackBufferStride) is called without SKSurfaceProperties — same issue as SKControl; subpixel AA is silently disabled |
| `binding/SkiaSharp/SKFont.cs` | 67-70 | related | SKFont.Edging property maps directly to sk_font_get/set_edging — SubpixelAntialias value is correctly exposed and functional |
| `binding/SkiaSharp/SKSurfaceProperties.cs` | 15-23 | related | SKSurfaceProperties(SKPixelGeometry) constructor exists and works correctly; the workaround of creating an off-screen surface with this is valid |

### Workarounds

- Use SKFont.Edging = SKFontEdging.SubpixelAntialias (instead of the obsolete SKPaint.SubpixelText)
- Create surfaces manually with new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal) when not using the view controls
- For view controls: render to an off-screen SKSurface created with SKSurfaceProperties(SKPixelGeometry.RgbHorizontal), then blit that surface onto the main canvas

### Next Questions

- Should SKControl/SKElement/SKXamlCanvas expose a SurfaceProperties property to let users configure PixelGeometry?
- Is there a platform-specific way to query the actual display pixel geometry on Windows to set it automatically?

### Resolution Proposals

**Hypothesis:** The original question is answered: use SKFontEdging.SubpixelAntialias plus SKSurfaceProperties(SKPixelGeometry.RgbHorizontal). The views don't expose a SurfaceProperties API, so users must work around via off-screen surface.

1. **Use SKFont.Edging = SubpixelAntialias with correct surface properties** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Set SKFont.Edging = SKFontEdging.SubpixelAntialias and create surfaces with new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal). When not using a view control, pass this to SKSurface.Create.

```csharp
var font = new SKFont { Edging = SKFontEdging.SubpixelAntialias, Size = 24 };
var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
// When creating surfaces manually:
using var surface = SKSurface.Create(imageInfo, new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));
canvas.DrawText("SkiaSharp", x, y, font, paint);
```
2. **Off-screen surface workaround for views** — workaround, confidence 0.85 (85%), cost/s, validated=yes
   - When using SKControl/SKElement, render to an off-screen SKSurface created with SKSurfaceProperties(SKPixelGeometry.RgbHorizontal), then draw that surface's image onto the event canvas.

```csharp
private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
{
    var mainCanvas = e.Surface.Canvas;
    var info = e.Info;
    using var offscreen = SKSurface.Create(info, new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));
    var font = new SKFont { Edging = SKFontEdging.SubpixelAntialias, Size = 24 };
    var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
    offscreen.Canvas.Clear(SKColors.White);
    offscreen.Canvas.DrawText("SkiaSharp", info.Width / 2f, info.Height / 2f, font, paint);
    offscreen.Canvas.Flush();
    using var img = offscreen.Snapshot();
    mainCanvas.DrawImage(img, 0, 0);
}
```

**Recommended proposal:** Use SKFont.Edging = SubpixelAntialias with correct surface properties

**Why:** Simplest approach; only applicable when creating surfaces directly. For view users, the off-screen surface workaround is the only available path until the views expose SurfaceProperties.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.78 (78%) |
| Reason | The original question is answered: subpixel rendering requires SKFontEdging.SubpixelAntialias and a surface created with SKPixelGeometry.RgbHorizontal. The behavior change in 2.88.x was upstream Skia design change, not a SkiaSharp bug. A workaround exists. The related enhancement (exposing SurfaceProperties on views) should be tracked as a separate feature request. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Update labels: keep type/question, add area/SkiaSharp.Views, os/Windows-Classic, tenet/compatibility | labels=type/question, area/SkiaSharp.Views, os/Windows-Classic, tenet/compatibility |
| add-comment | high | 0.78 (78%) | Post answer with working code snippet and explanation of PixelGeometry requirement | — |
| close-issue | medium | 0.72 (72%) | Close as answered — the question is resolved via the comment above | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the question!

Enabling subpixel (ClearType) text rendering in SkiaSharp requires two things:

1. **Set `SKFont.Edging` to `SKFontEdging.SubpixelAntialias`** (the old `SKPaint.SubpixelText` is obsolete and does not do the same thing).
2. **Create the surface with `SKSurfaceProperties(SKPixelGeometry.RgbHorizontal)`** — since SkiaSharp 2.88.x, Skia changed the default surface `PixelGeometry` to `Unknown`, which silently disables subpixel rendering.

**If you control surface creation** (not using a view control):
```csharp
var font = new SKFont { Edging = SKFontEdging.SubpixelAntialias, Size = 24 };
var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
using var surface = SKSurface.Create(imageInfo, new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));
surface.Canvas.DrawText("SkiaSharp", x, y, font, paint);
```

**If you are using `SKControl`, `SKElement`, or similar views** (which do not expose `SurfaceProperties`), you can work around this by rendering to an off-screen surface:
```csharp
private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
{
    var info = e.Info;
    using var offscreen = SKSurface.Create(info, new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));
    var font = new SKFont { Edging = SKFontEdging.SubpixelAntialias, Size = 24 };
    var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
    offscreen.Canvas.Clear(SKColors.White);
    offscreen.Canvas.DrawText("SkiaSharp", info.Width / 2f, info.Height / 2f, font, paint);
    offscreen.Canvas.Flush();
    using var img = offscreen.Snapshot();
    e.Surface.Canvas.DrawImage(img, 0, 0);
}
```

Note: `SKColorType.Bgra8888` is required for subpixel AA on Windows (the default on that platform).

The upstream change is tracked at https://bugs.chromium.org/p/skia/issues/detail?id=3934. A feature request to expose `SurfaceProperties` on the SkiaSharp views would be a worthwhile enhancement to file separately.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 830,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T15:06:47Z",
    "currentLabels": [
      "type/question"
    ]
  },
  "summary": "Reporter asks how to enable subpixel (ClearType) text antialiasing on Windows using SkiaSharp WinForms views; the issue evolved to reveal that subpixel rendering regressed in 2.88.x because Skia changed the default PixelGeometry to Unknown, and the views API provides no way to pass SKSurfaceProperties to opt back in.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.8
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.82
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a WinForms app with SKControl/SKCanvasView",
        "Set SKPaint.SubpixelText = true and SKFont.Edging = SubpixelAntialias",
        "Draw text — subpixel (ClearType) rendering does not appear, only grayscale AA"
      ],
      "environmentDetails": "Windows, SkiaSharp 2.88.x, WinForms SKControl",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2308",
          "description": "Follow-up bug: subpixel rendering regressed in 2.88.x vs 2.80.x"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2",
        "2.80.3",
        "2.80.4",
        "2.88.0-preview",
        "2.88.3"
      ],
      "workedIn": "2.80.4",
      "brokeIn": "2.88.0-preview",
      "currentRelevance": "likely",
      "relevanceReason": "The views still create surfaces without SKSurfaceProperties; no fix has been shipped for the missing API to configure PixelGeometry on SKControl/SKElement."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.85,
      "reason": "Multiple commenters confirmed subpixel rendering worked in 2.80.x and stopped working in 2.88.x. Root cause identified: Skia upstream changed default PixelGeometry from RgbHorizontal to Unknown (https://bugs.chromium.org/p/skia/issues/detail?id=3934).",
      "workedInVersion": "2.80.4",
      "brokeInVersion": "2.88.0-preview"
    }
  },
  "analysis": {
    "summary": "Subpixel (ClearType) text rendering requires two conditions: (1) SKFont.Edging = SubpixelAntialias, and (2) the surface must be created with SKSurfaceProperties(SKPixelGeometry.RgbHorizontal). The SkiaSharp views (SKControl, SKElement) create surfaces without SKSurfaceProperties, so PixelGeometry defaults to Unknown and subpixel rendering is silently disabled. This was not a problem until 2.88.x when Skia changed the default. A workaround exists (render to an off-screen SKSurface with the correct properties), but the views need a SurfaceProperties property to expose this cleanly.",
    "rationale": "The original title labels it a question, but the comments reveal it is both a usage question (answered: SKFontEdging.SubpixelAntialias) and an enhancement gap (views don't expose SKSurfaceProperties). The regression noted in 2.88.x is real — caused by an upstream Skia change in PixelGeometry default. Related issue #2308 was closed 'completed' after a workaround was documented, but the views still lack a first-class API for this. Classified type/question because the issue title and body are usage questions and a workaround exists; the views enhancement gap is tracked as a separate observation.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKControl.cs",
        "lines": "42",
        "finding": "SKSurface.Create(info, data.Scan0, data.Stride) is called without SKSurfaceProperties — no PixelGeometry is specified, so it defaults to Unknown, disabling subpixel AA",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs",
        "lines": "73",
        "finding": "SKSurface.Create(info, bitmap.BackBuffer, bitmap.BackBufferStride) is called without SKSurfaceProperties — same issue as SKControl; subpixel AA is silently disabled",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFont.cs",
        "lines": "67-70",
        "finding": "SKFont.Edging property maps directly to sk_font_get/set_edging — SubpixelAntialias value is correctly exposed and functional",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKSurfaceProperties.cs",
        "lines": "15-23",
        "finding": "SKSurfaceProperties(SKPixelGeometry) constructor exists and works correctly; the workaround of creating an off-screen surface with this is valid",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "SubpixelText = true ... Neither DrawText nor DrawShapedText gave us the subpixel rendering",
        "source": "issue body",
        "interpretation": "Reporter used the old SKPaint.SubpixelText API which is now obsolete; the correct approach is SKFont.Edging = SubpixelAntialias plus the correct surface PixelGeometry"
      },
      {
        "text": "ClearType is only possible on surfaces that don't use premultiplied alpha",
        "source": "comment by Gillibald",
        "interpretation": "Early hint at the surface properties requirement; partially correct — the key is PixelGeometry, not just alpha"
      },
      {
        "text": "The PixelGeometry on created surface is now defaulting to Unknown instead of RgbHorizontal. To bring back the default behaviour the SKSurface ctor must be provided SKSurfaceProperties with SKPixelGeometry.RgbHorizontal",
        "source": "comment by themcoo on issue #2308",
        "interpretation": "Root cause identified: Skia upstream change broke the default; workaround is to pass SKSurfaceProperties"
      },
      {
        "text": "2.80.2: Subpixel displayed ok. 2.88.0-preview.x: Subpixel is NOT displayed ok.",
        "source": "comment by danipen",
        "interpretation": "Regression confirmed across multiple SkiaSharp versions"
      },
      {
        "text": "I don't see a possibility currently to hook up with different pixel geometry",
        "source": "comment by themcoo on issue #2308",
        "interpretation": "The views lack a first-class API to configure SKSurfaceProperties/PixelGeometry — an enhancement gap remains"
      }
    ],
    "workarounds": [
      "Use SKFont.Edging = SKFontEdging.SubpixelAntialias (instead of the obsolete SKPaint.SubpixelText)",
      "Create surfaces manually with new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal) when not using the view controls",
      "For view controls: render to an off-screen SKSurface created with SKSurfaceProperties(SKPixelGeometry.RgbHorizontal), then blit that surface onto the main canvas"
    ],
    "nextQuestions": [
      "Should SKControl/SKElement/SKXamlCanvas expose a SurfaceProperties property to let users configure PixelGeometry?",
      "Is there a platform-specific way to query the actual display pixel geometry on Windows to set it automatically?"
    ],
    "resolution": {
      "hypothesis": "The original question is answered: use SKFontEdging.SubpixelAntialias plus SKSurfaceProperties(SKPixelGeometry.RgbHorizontal). The views don't expose a SurfaceProperties API, so users must work around via off-screen surface.",
      "proposals": [
        {
          "title": "Use SKFont.Edging = SubpixelAntialias with correct surface properties",
          "description": "Set SKFont.Edging = SKFontEdging.SubpixelAntialias and create surfaces with new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal). When not using a view control, pass this to SKSurface.Create.",
          "category": "workaround",
          "codeSnippet": "var font = new SKFont { Edging = SKFontEdging.SubpixelAntialias, Size = 24 };\nvar paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };\n// When creating surfaces manually:\nusing var surface = SKSurface.Create(imageInfo, new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));\ncanvas.DrawText(\"SkiaSharp\", x, y, font, paint);",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Off-screen surface workaround for views",
          "description": "When using SKControl/SKElement, render to an off-screen SKSurface created with SKSurfaceProperties(SKPixelGeometry.RgbHorizontal), then draw that surface's image onto the event canvas.",
          "category": "workaround",
          "codeSnippet": "private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)\n{\n    var mainCanvas = e.Surface.Canvas;\n    var info = e.Info;\n    using var offscreen = SKSurface.Create(info, new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));\n    var font = new SKFont { Edging = SKFontEdging.SubpixelAntialias, Size = 24 };\n    var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };\n    offscreen.Canvas.Clear(SKColors.White);\n    offscreen.Canvas.DrawText(\"SkiaSharp\", info.Width / 2f, info.Height / 2f, font, paint);\n    offscreen.Canvas.Flush();\n    using var img = offscreen.Snapshot();\n    mainCanvas.DrawImage(img, 0, 0);\n}",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Use SKFont.Edging = SubpixelAntialias with correct surface properties",
      "recommendedReason": "Simplest approach; only applicable when creating surfaces directly. For view users, the off-screen surface workaround is the only available path until the views expose SurfaceProperties."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.78,
      "reason": "The original question is answered: subpixel rendering requires SKFontEdging.SubpixelAntialias and a surface created with SKPixelGeometry.RgbHorizontal. The behavior change in 2.88.x was upstream Skia design change, not a SkiaSharp bug. A workaround exists. The related enhancement (exposing SurfaceProperties on views) should be tracked as a separate feature request.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Update labels: keep type/question, add area/SkiaSharp.Views, os/Windows-Classic, tenet/compatibility",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/question",
          "area/SkiaSharp.Views",
          "os/Windows-Classic",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post answer with working code snippet and explanation of PixelGeometry requirement",
        "risk": "high",
        "confidence": 0.78,
        "comment": "Thanks for the question!\n\nEnabling subpixel (ClearType) text rendering in SkiaSharp requires two things:\n\n1. **Set `SKFont.Edging` to `SKFontEdging.SubpixelAntialias`** (the old `SKPaint.SubpixelText` is obsolete and does not do the same thing).\n2. **Create the surface with `SKSurfaceProperties(SKPixelGeometry.RgbHorizontal)`** — since SkiaSharp 2.88.x, Skia changed the default surface `PixelGeometry` to `Unknown`, which silently disables subpixel rendering.\n\n**If you control surface creation** (not using a view control):\n```csharp\nvar font = new SKFont { Edging = SKFontEdging.SubpixelAntialias, Size = 24 };\nvar paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };\nusing var surface = SKSurface.Create(imageInfo, new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));\nsurface.Canvas.DrawText(\"SkiaSharp\", x, y, font, paint);\n```\n\n**If you are using `SKControl`, `SKElement`, or similar views** (which do not expose `SurfaceProperties`), you can work around this by rendering to an off-screen surface:\n```csharp\nprivate void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)\n{\n    var info = e.Info;\n    using var offscreen = SKSurface.Create(info, new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));\n    var font = new SKFont { Edging = SKFontEdging.SubpixelAntialias, Size = 24 };\n    var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };\n    offscreen.Canvas.Clear(SKColors.White);\n    offscreen.Canvas.DrawText(\"SkiaSharp\", info.Width / 2f, info.Height / 2f, font, paint);\n    offscreen.Canvas.Flush();\n    using var img = offscreen.Snapshot();\n    e.Surface.Canvas.DrawImage(img, 0, 0);\n}\n```\n\nNote: `SKColorType.Bgra8888` is required for subpixel AA on Windows (the default on that platform).\n\nThe upstream change is tracked at https://bugs.chromium.org/p/skia/issues/detail?id=3934. A feature request to expose `SurfaceProperties` on the SkiaSharp views would be a worthwhile enhancement to file separately."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — the question is resolved via the comment above",
        "risk": "medium",
        "confidence": 0.72,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
