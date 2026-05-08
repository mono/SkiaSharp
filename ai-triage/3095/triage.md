# Issue Triage Report — #3095

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T15:05:40Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/libSkiaSharp.native (0.90 (90%)) |
| Suggested action | close-as-external (0.88 (88%)) |

**Issue Summary:** Radial gradient shader has no visible effect when drawing on SKSvgCanvas — the resulting SVG file renders as a flat fill instead of the expected gradient, while linear gradient works correctly.

**Analysis:** Radial gradient is not emitted by Skia's SVG device backend (SkSVGDevice.cpp) because the feature was never implemented upstream — there is a TODO comment at the relevant code path. SkiaSharp's SVG canvas (SKSvgCanvas) is a thin wrapper over sk_svgcanvas_create_with_stream, which delegates entirely to Skia's native SkSVGDevice. There is no fix possible within SkiaSharp itself.

**Recommendations:** **close-as-external** — Root cause is a known unimplemented TODO in upstream Skia's SkSVGDevice.cpp. SkiaSharp is a pass-through wrapper and cannot fix this internally. The appropriate path is closing as external with guidance to report upstream.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | — |
| Backends | backend/SVG |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a SKSvgCanvas with SKSvgCanvas.Create(bounds, stream)
2. Create a SKPaint with Shader = SKShader.CreateRadialGradient(...)
3. Call canvas.DrawPaint(paint)
4. Inspect the resulting SVG file — gradient is missing

**Environment:** Linux, SkiaSharp 2.88.9

**Repository links:**
- https://github.com/nandor23/skiasharp-gradient-sample — Minimal repro project from reporter
- https://github.com/google/skia/blob/main/src/svg/SkSVGDevice.cpp#L449 — Upstream Skia SkSVGDevice.cpp — community-identified TODO for radial gradient support

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Radial gradient shader silently ignored when drawing to SKSvgCanvas; output SVG renders as flat color |
| Repro quality | complete |
| Target frameworks | net9.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.9 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The upstream TODO in SkSVGDevice.cpp has not been resolved; this is a known upstream limitation not addressed in SkiaSharp. |

## Analysis

### Technical Summary

Radial gradient is not emitted by Skia's SVG device backend (SkSVGDevice.cpp) because the feature was never implemented upstream — there is a TODO comment at the relevant code path. SkiaSharp's SVG canvas (SKSvgCanvas) is a thin wrapper over sk_svgcanvas_create_with_stream, which delegates entirely to Skia's native SkSVGDevice. There is no fix possible within SkiaSharp itself.

### Rationale

The community member correctly identified the root cause as an unimplemented feature (TODO) in upstream Skia's SkSVGDevice.cpp. SkiaSharp's SKSVG.cs simply calls sk_svgcanvas_create_with_stream and has no code path to intercept or supplement SVG gradient rendering. This is an upstream Skia limitation, not a SkiaSharp bug. Suggested action is close-as-external.

### Key Signals

- "It was never implemented in Skia svg canvas. They left a 'TODO' in that section of the code." — **comment by yrymrr (community member)** (Confirms the root cause is an unimplemented feature in upstream Skia's SkSVGDevice.cpp.)
- "the radial gradient shader is treated as if it wasn't defined at all" — **issue body** (Silent fallback — the shader is silently dropped or replaced with a flat fill in the SVG output.)
- "https://github.com/google/skia/blob/main/src/svg/SkSVGDevice.cpp#L449" — **comment by yrymrr** (Direct pointer to the unimplemented code path in upstream Skia.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKSVG.cs` | 26-31 | direct | SKSvgCanvas.Create delegates entirely to sk_svgcanvas_create_with_stream via P/Invoke. There is no SkiaSharp-level code for SVG gradient rendering — all gradient-to-SVG translation happens in the native Skia SkSVGDevice. SkiaSharp cannot fix this without an upstream change. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 15573-15589 | related | sk_svgcanvas_create_with_stream is a generated P/Invoke binding for the native Skia SVG canvas API. No wrapper logic — confirms this is a pass-through to native Skia behavior. |

### Workarounds

- Render to a raster surface (SKBitmap or SKPixmap) using the raster backend, then encode as PNG and embed in an SVG manually via a data URI.
- Use a solid color approximation (center color of the gradient) as a workaround if exact gradient fidelity is not required.
- Report the issue to upstream Skia at https://skia.org/docs/dev/contrib/ to request radial gradient support in SkSVGDevice.

### Next Questions

- Has upstream Skia fixed this TODO in a version after 2.88.9 bundled?
- Does the issue also affect sweep gradients and two-point conical gradients in SKSvgCanvas?

### Resolution Proposals

**Hypothesis:** The radial gradient feature was simply never implemented in Skia's SkSVGDevice. The fix must come from upstream Skia, not SkiaSharp.

1. **Render to raster and embed as PNG data URI in SVG** — workaround, confidence 0.85 (85%), cost/s, validated=untested
   - As a workaround, render the drawing to an SKBitmap with the raster backend, encode as PNG, then write a minimal SVG that embeds the PNG as a base64 data URI inside an <image> element.
2. **Report upstream to Skia project** — investigation, confidence 0.90 (90%), cost/m, validated=untested
   - File an upstream Skia bug at https://skia.org/docs/dev/contrib/ requesting that radial gradient support be added to SkSVGDevice.cpp. Once fixed upstream, SkiaSharp will pick it up automatically on the next Skia milestone bump.

**Recommended proposal:** Render to raster and embed as PNG data URI in SVG

**Why:** Actionable today without waiting for upstream Skia changes.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.88 (88%) |
| Reason | Root cause is a known unimplemented TODO in upstream Skia's SkSVGDevice.cpp. SkiaSharp is a pass-through wrapper and cannot fix this internally. The appropriate path is closing as external with guidance to report upstream. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, native library, SVG backend labels | labels=type/bug, area/libSkiaSharp.native, backend/SVG, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Explain upstream root cause and provide workaround | — |
| close-issue | medium | 0.80 (80%) | Close as external upstream limitation | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and sample repository!

As a community member correctly identified, this is a known limitation in **upstream Skia** rather than a SkiaSharp bug. Skia's `SkSVGDevice` — the component responsible for translating draw calls into SVG markup — has a TODO for radial gradient support that was never implemented. Since `SKSvgCanvas` is a thin wrapper over Skia's native SVG device, SkiaSharp cannot fix this independently.

**Workaround:**
If you need SVG output with a radial gradient effect, you can render to a raster surface first and then embed the result as a PNG `<image>` element in your SVG:

```csharp
// Render to bitmap
var info = new SKImageInfo((int)size, (int)size);
using var surface = SKSurface.Create(info);
using var canvas = surface.Canvas;
using var paint = new SKPaint
{
    Shader = SKShader.CreateRadialGradient(
        new SKPoint(size / 2, size / 2),
        size / 2,
        new[] { SKColors.CornflowerBlue, SKColors.YellowGreen },
        null,
        SKShaderTileMode.Clamp)
};
canvas.DrawPaint(paint);

// Encode as PNG and embed in SVG
using var image = surface.Snapshot();
using var data = image.Encode(SKEncodedImageFormat.Png, 100);
var base64 = Convert.ToBase64String(data.ToArray());
var svg = $"""<svg xmlns='http://www.w3.org/2000/svg' width='{size}' height='{size}'><image href='data:image/png;base64,{base64}' width='{size}' height='{size}'/></svg>""";
File.WriteAllText("radial_gradient.svg", svg);
```

To request this feature be fixed in upstream Skia, you can report it via https://skia.org/docs/dev/contrib/
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3095,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T15:05:40Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Radial gradient shader has no visible effect when drawing on SKSvgCanvas — the resulting SVG file renders as a flat fill instead of the expected gradient, while linear gradient works correctly.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.9
    },
    "backends": [
      "backend/SVG"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "Radial gradient shader silently ignored when drawing to SKSvgCanvas; output SVG renders as flat color",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net9.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a SKSvgCanvas with SKSvgCanvas.Create(bounds, stream)",
        "Create a SKPaint with Shader = SKShader.CreateRadialGradient(...)",
        "Call canvas.DrawPaint(paint)",
        "Inspect the resulting SVG file — gradient is missing"
      ],
      "environmentDetails": "Linux, SkiaSharp 2.88.9",
      "repoLinks": [
        {
          "url": "https://github.com/nandor23/skiasharp-gradient-sample",
          "description": "Minimal repro project from reporter"
        },
        {
          "url": "https://github.com/google/skia/blob/main/src/svg/SkSVGDevice.cpp#L449",
          "description": "Upstream Skia SkSVGDevice.cpp — community-identified TODO for radial gradient support"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.9"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The upstream TODO in SkSVGDevice.cpp has not been resolved; this is a known upstream limitation not addressed in SkiaSharp."
    }
  },
  "analysis": {
    "summary": "Radial gradient is not emitted by Skia's SVG device backend (SkSVGDevice.cpp) because the feature was never implemented upstream — there is a TODO comment at the relevant code path. SkiaSharp's SVG canvas (SKSvgCanvas) is a thin wrapper over sk_svgcanvas_create_with_stream, which delegates entirely to Skia's native SkSVGDevice. There is no fix possible within SkiaSharp itself.",
    "rationale": "The community member correctly identified the root cause as an unimplemented feature (TODO) in upstream Skia's SkSVGDevice.cpp. SkiaSharp's SKSVG.cs simply calls sk_svgcanvas_create_with_stream and has no code path to intercept or supplement SVG gradient rendering. This is an upstream Skia limitation, not a SkiaSharp bug. Suggested action is close-as-external.",
    "keySignals": [
      {
        "text": "It was never implemented in Skia svg canvas. They left a 'TODO' in that section of the code.",
        "source": "comment by yrymrr (community member)",
        "interpretation": "Confirms the root cause is an unimplemented feature in upstream Skia's SkSVGDevice.cpp."
      },
      {
        "text": "the radial gradient shader is treated as if it wasn't defined at all",
        "source": "issue body",
        "interpretation": "Silent fallback — the shader is silently dropped or replaced with a flat fill in the SVG output."
      },
      {
        "text": "https://github.com/google/skia/blob/main/src/svg/SkSVGDevice.cpp#L449",
        "source": "comment by yrymrr",
        "interpretation": "Direct pointer to the unimplemented code path in upstream Skia."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKSVG.cs",
        "lines": "26-31",
        "finding": "SKSvgCanvas.Create delegates entirely to sk_svgcanvas_create_with_stream via P/Invoke. There is no SkiaSharp-level code for SVG gradient rendering — all gradient-to-SVG translation happens in the native Skia SkSVGDevice. SkiaSharp cannot fix this without an upstream change.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "15573-15589",
        "finding": "sk_svgcanvas_create_with_stream is a generated P/Invoke binding for the native Skia SVG canvas API. No wrapper logic — confirms this is a pass-through to native Skia behavior.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Render to a raster surface (SKBitmap or SKPixmap) using the raster backend, then encode as PNG and embed in an SVG manually via a data URI.",
      "Use a solid color approximation (center color of the gradient) as a workaround if exact gradient fidelity is not required.",
      "Report the issue to upstream Skia at https://skia.org/docs/dev/contrib/ to request radial gradient support in SkSVGDevice."
    ],
    "nextQuestions": [
      "Has upstream Skia fixed this TODO in a version after 2.88.9 bundled?",
      "Does the issue also affect sweep gradients and two-point conical gradients in SKSvgCanvas?"
    ],
    "resolution": {
      "hypothesis": "The radial gradient feature was simply never implemented in Skia's SkSVGDevice. The fix must come from upstream Skia, not SkiaSharp.",
      "proposals": [
        {
          "title": "Render to raster and embed as PNG data URI in SVG",
          "description": "As a workaround, render the drawing to an SKBitmap with the raster backend, encode as PNG, then write a minimal SVG that embeds the PNG as a base64 data URI inside an <image> element.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Report upstream to Skia project",
          "description": "File an upstream Skia bug at https://skia.org/docs/dev/contrib/ requesting that radial gradient support be added to SkSVGDevice.cpp. Once fixed upstream, SkiaSharp will pick it up automatically on the next Skia milestone bump.",
          "category": "investigation",
          "confidence": 0.9,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Render to raster and embed as PNG data URI in SVG",
      "recommendedReason": "Actionable today without waiting for upstream Skia changes."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.88,
      "reason": "Root cause is a known unimplemented TODO in upstream Skia's SkSVGDevice.cpp. SkiaSharp is a pass-through wrapper and cannot fix this internally. The appropriate path is closing as external with guidance to report upstream.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, native library, SVG backend labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "backend/SVG",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain upstream root cause and provide workaround",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report and sample repository!\n\nAs a community member correctly identified, this is a known limitation in **upstream Skia** rather than a SkiaSharp bug. Skia's `SkSVGDevice` — the component responsible for translating draw calls into SVG markup — has a TODO for radial gradient support that was never implemented. Since `SKSvgCanvas` is a thin wrapper over Skia's native SVG device, SkiaSharp cannot fix this independently.\n\n**Workaround:**\nIf you need SVG output with a radial gradient effect, you can render to a raster surface first and then embed the result as a PNG `<image>` element in your SVG:\n\n```csharp\n// Render to bitmap\nvar info = new SKImageInfo((int)size, (int)size);\nusing var surface = SKSurface.Create(info);\nusing var canvas = surface.Canvas;\nusing var paint = new SKPaint\n{\n    Shader = SKShader.CreateRadialGradient(\n        new SKPoint(size / 2, size / 2),\n        size / 2,\n        new[] { SKColors.CornflowerBlue, SKColors.YellowGreen },\n        null,\n        SKShaderTileMode.Clamp)\n};\ncanvas.DrawPaint(paint);\n\n// Encode as PNG and embed in SVG\nusing var image = surface.Snapshot();\nusing var data = image.Encode(SKEncodedImageFormat.Png, 100);\nvar base64 = Convert.ToBase64String(data.ToArray());\nvar svg = $\"\"\"<svg xmlns='http://www.w3.org/2000/svg' width='{size}' height='{size}'><image href='data:image/png;base64,{base64}' width='{size}' height='{size}'/></svg>\"\"\";\nFile.WriteAllText(\"radial_gradient.svg\", svg);\n```\n\nTo request this feature be fixed in upstream Skia, you can report it via https://skia.org/docs/dev/contrib/"
      },
      {
        "type": "close-issue",
        "description": "Close as external upstream limitation",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
