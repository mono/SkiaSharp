# Issue Triage Report — #2049

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T20:16:48Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp.Views.Blazor (0.95 (95%)) |
| Suggested action | needs-info (0.82 (82%)) |

**Issue Summary:** SKGLView in Blazor WASM does not render some elements on iOS Safari, while SKCanvasView renders correctly and desktop browsers are unaffected.

**Analysis:** SKGLView fails to render some elements on iOS Safari. The root cause is likely iOS Safari's limited or buggy WebGL 2.0 support at time of report — SKHtmlCanvas.js tries WebGL 2 first, falls back to WebGL 1, and this fallback may not handle all rendering operations identically (especially framebuffer info, stencil buffer, or surface origin). SKCanvasView (raster path) works because it uses 2D Canvas API instead of WebGL.

**Recommendations:** **needs-info** — The report has screenshots and links to complex external code, but lacks a minimal repro and iOS/Safari version details needed to investigate the GL rendering failure.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Blazor |
| Platforms | os/iOS, os/WASM |
| Backends | backend/OpenGL |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Create a Blazor WASM app using SKGLView
2. Draw complex shapes (e.g., column flowsheet with gradient fills)
3. Run in iOS Safari
4. Observe that some drawn elements are missing

**Environment:** SkiaSharp Blazor View preview.256; iOS Safari; works on desktop Safari, Chrome, Edge

**Repository links:**
- https://github.com/DanWBR/dwsim/blob/windows/DWSIM.Drawing.SkiaSharp/GraphicObjects/Shapes/RigorousColumn.vb#L116 — Reporter's drawing code for the missing column shapes

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | net6.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | preview.256 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The WebGL context creation path and iOS Safari WebGL 2 support issue has not been explicitly fixed in later versions based on code review. |

## Analysis

### Technical Summary

SKGLView fails to render some elements on iOS Safari. The root cause is likely iOS Safari's limited or buggy WebGL 2.0 support at time of report — SKHtmlCanvas.js tries WebGL 2 first, falls back to WebGL 1, and this fallback may not handle all rendering operations identically (especially framebuffer info, stencil buffer, or surface origin). SKCanvasView (raster path) works because it uses 2D Canvas API instead of WebGL.

### Rationale

Reporter provides screenshots confirming SKCanvasView works but SKGLView does not render the same elements on iOS. The bug is clearly in the GL/WebGL rendering path. Code inspection shows createWebGLContext attempts WebGL 2.0 then falls back to 1.0; iOS Safari had known limited WebGL 2.0 support in early 2022. The surfaceOrigin is BottomLeft (standard OpenGL) which may cause issues with some iOS WebGL contexts when drawing certain elements.

### Key Signals

- "SKGLView doesn't render some elements when run on iOS" — **issue title** (Partial rendering failure specific to the GL code path.)
- "It does work on all desktop browsers I've tried (Safari, Chrome, Edge, etc)" — **issue body** (Desktop browsers fully support WebGL 2 / emscripten GL layer. iOS Safari has historically limited WebGL 2 support.)
- "SKCanvasView [works correctly on iOS]" — **issue body with screenshot** (The raster/2D Canvas path is fine; only the GL path has the issue.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/wwwroot/SKHtmlCanvas.js` | 147-170 | direct | createWebGLContext() tries WebGL 2.0 first (majorVersion: 2), falling back to WebGL 1.0 if context creation fails. iOS Safari WebGL 2 support was incomplete or unavailable on older iOS versions in mid-2022, which may cause partial rendering failures rather than full context creation failure. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKGLView.razor.cs` | 20-22 | related | surfaceOrigin is GRSurfaceOrigin.BottomLeft and colorType is SKColorType.Rgba8888. On iOS WebGL 1.0 fallback context, framebuffer binding or pixel format differences could cause some draw calls to produce missing output. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKGLView.razor.cs` | 100-154 | direct | OnRenderFrame creates a GRContext from GRGlInterface.Create(), then builds a render target from jsGLInfo.FboId. If the WebGL context falls back to v1, GRGlInterface.Create() might not correctly enumerate all extensions, affecting how Skia executes certain draw operations. |

### Next Questions

- Which iOS version and Safari version was used?
- Does the issue still occur on current iOS/Safari versions with WebGL 2 support?
- Does setting a preserveDrawingBuffer or using explicit WebGL 1.0 context change the behavior?
- Can the reporter provide a minimal reproducible example without the external DWSIM codebase?

### Resolution Proposals

**Hypothesis:** iOS Safari WebGL 2.0 support was incomplete in mid-2022, causing the fallback to WebGL 1.0. The Skia GL layer may not correctly render certain operations (e.g., gradient-filled shapes) under WebGL 1.0 with limited extension support on iOS.

1. **Request minimal repro + iOS version details** — investigation, confidence 0.90 (90%), cost/xs, validated=untested
   - Ask the reporter for their iOS version and a minimal self-contained Blazor WASM repro that doesn't require the full DWSIM codebase.
2. **Workaround: use SKCanvasView instead of SKGLView** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - As shown in reporter's own screenshot, SKCanvasView renders correctly on iOS. Users who need iOS Safari compatibility should use SKCanvasView (raster path) rather than SKGLView for Blazor WASM applications.

**Recommended proposal:** Request minimal repro + iOS version details

**Why:** The issue is from 2022 and may be resolved with current iOS Safari WebGL 2 support. Need minimal repro to verify if it still exists and to investigate the exact GL rendering path failure.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.82 (82%) |
| Reason | The report has screenshots and links to complex external code, but lacks a minimal repro and iOS/Safari version details needed to investigate the GL rendering failure. |
| Suggested repro platform | macos |

### Missing Info

- iOS version and Safari version
- Minimal self-contained Blazor WASM repro (without DWSIM dependency)
- Whether the issue still occurs on current SkiaSharp Blazor version

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, Blazor views, iOS, WASM, OpenGL labels | labels=type/bug, area/SkiaSharp.Views.Blazor, os/iOS, os/WASM, backend/OpenGL |
| add-comment | medium | 0.82 (82%) | Request iOS version and minimal repro, suggest SKCanvasView workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report and screenshots!

To investigate this further, could you provide:
1. **iOS version** and **Safari version** you tested on
2. A **minimal self-contained Blazor WASM repro** that doesn't require the full DWSIM project — ideally something simple that shows the same missing elements
3. The **current SkiaSharp version** you're using (the issue mentions preview.256, which is quite old)

**Workaround in the meantime:** As shown in your own screenshots, `SKCanvasView` renders correctly on iOS. If you need iOS Safari compatibility, you can use `SKCanvasView` instead of `SKGLView` for your Blazor WASM application — the drawing API is the same.

Note: iOS Safari's WebGL 2.0 support was limited/buggy in early 2022. This issue may have improved with newer iOS/Safari versions.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2049,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T20:16:48Z"
  },
  "summary": "SKGLView in Blazor WASM does not render some elements on iOS Safari, while SKCanvasView renders correctly and desktop browsers are unaffected.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp.Views.Blazor",
      "confidence": 0.95
    },
    "platforms": [
      "os/iOS",
      "os/WASM"
    ],
    "backends": [
      "backend/OpenGL"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net6.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Blazor WASM app using SKGLView",
        "Draw complex shapes (e.g., column flowsheet with gradient fills)",
        "Run in iOS Safari",
        "Observe that some drawn elements are missing"
      ],
      "environmentDetails": "SkiaSharp Blazor View preview.256; iOS Safari; works on desktop Safari, Chrome, Edge",
      "repoLinks": [
        {
          "url": "https://github.com/DanWBR/dwsim/blob/windows/DWSIM.Drawing.SkiaSharp/GraphicObjects/Shapes/RigorousColumn.vb#L116",
          "description": "Reporter's drawing code for the missing column shapes"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "preview.256"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The WebGL context creation path and iOS Safari WebGL 2 support issue has not been explicitly fixed in later versions based on code review."
    }
  },
  "analysis": {
    "summary": "SKGLView fails to render some elements on iOS Safari. The root cause is likely iOS Safari's limited or buggy WebGL 2.0 support at time of report — SKHtmlCanvas.js tries WebGL 2 first, falls back to WebGL 1, and this fallback may not handle all rendering operations identically (especially framebuffer info, stencil buffer, or surface origin). SKCanvasView (raster path) works because it uses 2D Canvas API instead of WebGL.",
    "rationale": "Reporter provides screenshots confirming SKCanvasView works but SKGLView does not render the same elements on iOS. The bug is clearly in the GL/WebGL rendering path. Code inspection shows createWebGLContext attempts WebGL 2.0 then falls back to 1.0; iOS Safari had known limited WebGL 2.0 support in early 2022. The surfaceOrigin is BottomLeft (standard OpenGL) which may cause issues with some iOS WebGL contexts when drawing certain elements.",
    "keySignals": [
      {
        "text": "SKGLView doesn't render some elements when run on iOS",
        "source": "issue title",
        "interpretation": "Partial rendering failure specific to the GL code path."
      },
      {
        "text": "It does work on all desktop browsers I've tried (Safari, Chrome, Edge, etc)",
        "source": "issue body",
        "interpretation": "Desktop browsers fully support WebGL 2 / emscripten GL layer. iOS Safari has historically limited WebGL 2 support."
      },
      {
        "text": "SKCanvasView [works correctly on iOS]",
        "source": "issue body with screenshot",
        "interpretation": "The raster/2D Canvas path is fine; only the GL path has the issue."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/wwwroot/SKHtmlCanvas.js",
        "lines": "147-170",
        "finding": "createWebGLContext() tries WebGL 2.0 first (majorVersion: 2), falling back to WebGL 1.0 if context creation fails. iOS Safari WebGL 2 support was incomplete or unavailable on older iOS versions in mid-2022, which may cause partial rendering failures rather than full context creation failure.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKGLView.razor.cs",
        "lines": "20-22",
        "finding": "surfaceOrigin is GRSurfaceOrigin.BottomLeft and colorType is SKColorType.Rgba8888. On iOS WebGL 1.0 fallback context, framebuffer binding or pixel format differences could cause some draw calls to produce missing output.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKGLView.razor.cs",
        "lines": "100-154",
        "finding": "OnRenderFrame creates a GRContext from GRGlInterface.Create(), then builds a render target from jsGLInfo.FboId. If the WebGL context falls back to v1, GRGlInterface.Create() might not correctly enumerate all extensions, affecting how Skia executes certain draw operations.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "Which iOS version and Safari version was used?",
      "Does the issue still occur on current iOS/Safari versions with WebGL 2 support?",
      "Does setting a preserveDrawingBuffer or using explicit WebGL 1.0 context change the behavior?",
      "Can the reporter provide a minimal reproducible example without the external DWSIM codebase?"
    ],
    "resolution": {
      "hypothesis": "iOS Safari WebGL 2.0 support was incomplete in mid-2022, causing the fallback to WebGL 1.0. The Skia GL layer may not correctly render certain operations (e.g., gradient-filled shapes) under WebGL 1.0 with limited extension support on iOS.",
      "proposals": [
        {
          "title": "Request minimal repro + iOS version details",
          "description": "Ask the reporter for their iOS version and a minimal self-contained Blazor WASM repro that doesn't require the full DWSIM codebase.",
          "category": "investigation",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Workaround: use SKCanvasView instead of SKGLView",
          "description": "As shown in reporter's own screenshot, SKCanvasView renders correctly on iOS. Users who need iOS Safari compatibility should use SKCanvasView (raster path) rather than SKGLView for Blazor WASM applications.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Request minimal repro + iOS version details",
      "recommendedReason": "The issue is from 2022 and may be resolved with current iOS Safari WebGL 2 support. Need minimal repro to verify if it still exists and to investigate the exact GL rendering path failure."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.82,
      "reason": "The report has screenshots and links to complex external code, but lacks a minimal repro and iOS/Safari version details needed to investigate the GL rendering failure.",
      "suggestedReproPlatform": "macos"
    },
    "missingInfo": [
      "iOS version and Safari version",
      "Minimal self-contained Blazor WASM repro (without DWSIM dependency)",
      "Whether the issue still occurs on current SkiaSharp Blazor version"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, Blazor views, iOS, WASM, OpenGL labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Blazor",
          "os/iOS",
          "os/WASM",
          "backend/OpenGL"
        ]
      },
      {
        "type": "add-comment",
        "description": "Request iOS version and minimal repro, suggest SKCanvasView workaround",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thank you for the detailed report and screenshots!\n\nTo investigate this further, could you provide:\n1. **iOS version** and **Safari version** you tested on\n2. A **minimal self-contained Blazor WASM repro** that doesn't require the full DWSIM project — ideally something simple that shows the same missing elements\n3. The **current SkiaSharp version** you're using (the issue mentions preview.256, which is quite old)\n\n**Workaround in the meantime:** As shown in your own screenshots, `SKCanvasView` renders correctly on iOS. If you need iOS Safari compatibility, you can use `SKCanvasView` instead of `SKGLView` for your Blazor WASM application — the drawing API is the same.\n\nNote: iOS Safari's WebGL 2.0 support was limited/buggy in early 2022. This issue may have improved with newer iOS/Safari versions."
      }
    ]
  }
}
```

</details>
