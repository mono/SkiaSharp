# Issue Triage Report — #3313

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T00:49:27Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp.Views.Blazor (0.95 (95%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** SKCanvasView in Blazor/WASM shows severe frame-rate degradation (120fps → ~10fps) when drawing a scaled SKImage due to software rasterization and large putImageData transfers, unlike SKGLView which is GPU-accelerated.

**Analysis:** SKCanvasView in Blazor uses a CPU software raster pipeline: Skia renders into a byte[] backed SKSurface in WASM memory, then the entire framebuffer is transferred to the HTML canvas via ctx.putImageData(). When the image is scaled, Skia must perform software bilinear filtering for every output pixel in WASM, and the resulting large pixel buffer (e.g. 1600x1600x4 = ~10MB) must cross the WASM/JS boundary on every frame. SKGLView bypasses this entirely by using WebGL, where the GPU handles texture scaling in hardware with no per-pixel CPU work and no large buffer transfer.

**Recommendations:** **needs-investigation** — Real performance bug with clear repro numbers and a known root cause (software raster + putImageData). Needs investigation into whether a GPU-backed fix is feasible for SKCanvasView, and the reporter needs a workaround.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Blazor |
| Platforms | os/WASM |
| Backends | — |
| Tenets | tenet/performance |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. In a Blazor WASM app using SKCanvasView, load a 786x1090 SKBitmap and create a cached SKImage from it
2. Draw the cached SKImage scaled to a larger rect (e.g. 800x800 or 1600x1600) using canvas.DrawImage()
3. Observe FPS drop from 120fps (1:1 drawing) to 50fps (800x800) to 12fps (1600x1600)
4. Apply canvas.Scale(2) before 1:1 drawing and observe same degradation (~10fps)

**Environment:** macOS M2 Pro, Chrome/Firefox/Safari, SkiaSharp 3.119.0

**Screenshots:**
- https://github.com/user-attachments/assets/085a25a2-0300-4adc-8c6d-343d8e88d898 — Screenshot: 800x800 rect at ~50fps
- https://github.com/user-attachments/assets/1cd90261-e281-4d8b-9109-605a28980f6f — Screenshot: 1600x1600 rect at ~12fps
- https://github.com/user-attachments/assets/835f2669-29da-40dc-aa22-98ed79a458f9 — Screenshot: 1:1 786x1090 rect at 120fps

**Code snippets:**

```csharp
using (new SKAutoCanvasRestore(canvas)) {
    canvas.ResetMatrix();
    canvas.Scale(2);
    canvas.DrawImage(image.Image, new SKRect(0,0, 786, 1090), SKSamplingOptions.Default, theme.BitmapPaint);
}
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | performance |
| Error message | — |
| Repro quality | partial |
| Target frameworks | net8.0-browser |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.119.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The software raster architecture of SKCanvasView in Blazor has not changed — putImageData transfer approach is the same. |

## Analysis

### Technical Summary

SKCanvasView in Blazor uses a CPU software raster pipeline: Skia renders into a byte[] backed SKSurface in WASM memory, then the entire framebuffer is transferred to the HTML canvas via ctx.putImageData(). When the image is scaled, Skia must perform software bilinear filtering for every output pixel in WASM, and the resulting large pixel buffer (e.g. 1600x1600x4 = ~10MB) must cross the WASM/JS boundary on every frame. SKGLView bypasses this entirely by using WebGL, where the GPU handles texture scaling in hardware with no per-pixel CPU work and no large buffer transfer.

### Rationale

Classified as type/bug because the reporter observes a severe platform-specific performance disparity (iOS/Android handle this smoothly). The architecture of SKCanvasView in Blazor is inherently software-raster based, creating a known WASM performance trap. The area is area/SkiaSharp.Views.Blazor since the issue is in the Blazor-specific canvas view implementation.

### Key Signals

- "When drawing exactly in a 786x1090 Rect, we get the solid 120fps. If I then just do a canvas.Scale(2) before drawing in the fixed size rect, we go back to around 10fps." — **issue body** (Scaling triggers expensive software resampling regardless of output rect size — confirms Skia is doing per-pixel software work, not GPU-accelerated scaling.)
- "SKCanvasView tanks performance, SKGLView handles it fine." — **issue body** (The perf gap is between software raster (SKCanvasView) and GPU-accelerated (SKGLView) — confirms architecture is root cause.)
- "I've tried experimenting with different SKSamplingOptions, but none improve it." — **issue body** (Filter quality changes don't help because the bottleneck is the entire software raster pipeline and putImageData transfer, not just the sampling algorithm.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKCanvasView.razor.cs` | 87-108 | direct | OnRenderFrame() creates SKSurface backed by a pinned byte[] array, invokes user paint callback, then calls interop.PutImageData() to transfer the whole pixel buffer to the HTML canvas. Every frame requires a full WASM→JS pixel buffer copy, which is O(width*height*4) bytes. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/wwwroot/SKHtmlCanvas.js` | 129-145 | direct | putImageData() calls ctx.putImageData(new ImageData(Uint8ClampedArray from WASM heap), 0, 0). This transfers the entire pixel buffer from WASM linear memory to the browser's ImageData every frame — no GPU compositing, no caching. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKCanvasView.razor.cs` | 67-77 | related | OnAfterRenderAsync calls interop.InitRaster() (not InitGL), locking this view to software raster mode. There is no option to switch to GPU-backed rendering without using SKGLView. |

### Workarounds

- Use SKGLView instead of SKCanvasView for image-heavy rendering in Blazor WASM (confirmed smooth by reporter); investigate the DrawLine performance issue separately.
- Pre-scale the SKImage to the exact display size once (using SKImage.Resize or drawing to a new SKBitmap at target size), then cache the pre-scaled image and draw 1:1 — this avoids per-frame software resampling.
- Reduce the canvas size or use IgnorePixelScaling=true to lower the pixel buffer size that must be transferred each frame.

### Next Questions

- Can the Blazor SKCanvasView be enhanced to use OffscreenCanvas + GPU compositing for scaling to reduce CPU pressure?
- Is there an intermediate option to render to a smaller surface and let the browser scale, as a workaround for image-heavy content?
- Does pre-scaling the SKImage to the target size before caching eliminate the per-frame cost?

### Resolution Proposals

**Hypothesis:** SKCanvasView in Blazor has a fundamental architecture mismatch: it uses software rasterization + full-frame putImageData, which is extremely expensive for scaled image drawing in WASM. Fixing properly would require offloading compositing to the browser GPU or using an intermediate OffscreenCanvas.

1. **Pre-scale image to display size** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - Create a pre-scaled version of the SKImage at the target display dimensions and cache it. Draw 1:1 each frame to avoid per-frame software resampling.
2. **Investigate OffscreenCanvas + GPU compositing** — investigation, confidence 0.60 (60%), cost/l, validated=untested
   - Explore whether SKCanvasView's putImageData approach could be replaced with a createImageBitmap + drawImage call that leverages browser GPU compositing for scaling operations.

**Recommended proposal:** Pre-scale image to display size

**Why:** Immediately actionable workaround with no library changes needed. Eliminates per-frame software scaling cost entirely.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Real performance bug with clear repro numbers and a known root cause (software raster + putImageData). Needs investigation into whether a GPU-backed fix is feasible for SKCanvasView, and the reporter needs a workaround. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, Blazor area, WASM, and performance tenet labels | labels=type/bug, area/SkiaSharp.Views.Blazor, os/WASM, tenet/performance |
| add-comment | medium | 0.85 (85%) | Explain root cause (software raster) and suggest pre-scaling workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed repro with FPS numbers!

**Root cause:** `SKCanvasView` in Blazor WASM uses a software raster pipeline — Skia draws into a pinned `byte[]` in WASM memory, then the entire pixel buffer is transferred to the HTML canvas every frame via `ctx.putImageData()`. When you scale an image, Skia must compute filtered pixel values for every output pixel in CPU/software, and then transfer a potentially large buffer (e.g. 1600×1600×4 ≈ 10MB) across the WASM/JS boundary on every frame. This is fundamentally different from `SKGLView`, which uses WebGL so GPU hardware handles scaling with no per-pixel CPU work.

**Workaround:** Pre-scale the image to the exact display dimensions once and cache the result:
```csharp
// Do this once when the image or target size changes
var scaledBitmap = new SKBitmap(displayWidth, displayHeight);
using var canvas = new SKCanvas(scaledBitmap);
canvas.DrawImage(sourceImage, new SKRect(0, 0, displayWidth, displayHeight), SKSamplingOptions.Default);
var scaledImage = SKImage.FromBitmap(scaledBitmap);

// Then each frame, draw 1:1 (no scaling = no per-frame resampling cost)
canvas.DrawImage(scaledImage, 0, 0);
```

This eliminates per-frame software resampling entirely.

We're tracking whether `SKCanvasView` in Blazor can be improved to use GPU-backed compositing for scaling operations.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3313,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T00:49:27Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKCanvasView in Blazor/WASM shows severe frame-rate degradation (120fps → ~10fps) when drawing a scaled SKImage due to software rasterization and large putImageData transfers, unlike SKGLView which is GPU-accelerated.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp.Views.Blazor",
      "confidence": 0.95
    },
    "platforms": [
      "os/WASM"
    ],
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "performance",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0-browser"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "In a Blazor WASM app using SKCanvasView, load a 786x1090 SKBitmap and create a cached SKImage from it",
        "Draw the cached SKImage scaled to a larger rect (e.g. 800x800 or 1600x1600) using canvas.DrawImage()",
        "Observe FPS drop from 120fps (1:1 drawing) to 50fps (800x800) to 12fps (1600x1600)",
        "Apply canvas.Scale(2) before 1:1 drawing and observe same degradation (~10fps)"
      ],
      "environmentDetails": "macOS M2 Pro, Chrome/Firefox/Safari, SkiaSharp 3.119.0",
      "codeSnippets": [
        "using (new SKAutoCanvasRestore(canvas)) {\n    canvas.ResetMatrix();\n    canvas.Scale(2);\n    canvas.DrawImage(image.Image, new SKRect(0,0, 786, 1090), SKSamplingOptions.Default, theme.BitmapPaint);\n}"
      ],
      "screenshots": [
        {
          "url": "https://github.com/user-attachments/assets/085a25a2-0300-4adc-8c6d-343d8e88d898",
          "description": "Screenshot: 800x800 rect at ~50fps"
        },
        {
          "url": "https://github.com/user-attachments/assets/1cd90261-e281-4d8b-9109-605a28980f6f",
          "description": "Screenshot: 1600x1600 rect at ~12fps"
        },
        {
          "url": "https://github.com/user-attachments/assets/835f2669-29da-40dc-aa22-98ed79a458f9",
          "description": "Screenshot: 1:1 786x1090 rect at 120fps"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.119.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The software raster architecture of SKCanvasView in Blazor has not changed — putImageData transfer approach is the same."
    }
  },
  "analysis": {
    "summary": "SKCanvasView in Blazor uses a CPU software raster pipeline: Skia renders into a byte[] backed SKSurface in WASM memory, then the entire framebuffer is transferred to the HTML canvas via ctx.putImageData(). When the image is scaled, Skia must perform software bilinear filtering for every output pixel in WASM, and the resulting large pixel buffer (e.g. 1600x1600x4 = ~10MB) must cross the WASM/JS boundary on every frame. SKGLView bypasses this entirely by using WebGL, where the GPU handles texture scaling in hardware with no per-pixel CPU work and no large buffer transfer.",
    "rationale": "Classified as type/bug because the reporter observes a severe platform-specific performance disparity (iOS/Android handle this smoothly). The architecture of SKCanvasView in Blazor is inherently software-raster based, creating a known WASM performance trap. The area is area/SkiaSharp.Views.Blazor since the issue is in the Blazor-specific canvas view implementation.",
    "keySignals": [
      {
        "text": "When drawing exactly in a 786x1090 Rect, we get the solid 120fps. If I then just do a canvas.Scale(2) before drawing in the fixed size rect, we go back to around 10fps.",
        "source": "issue body",
        "interpretation": "Scaling triggers expensive software resampling regardless of output rect size — confirms Skia is doing per-pixel software work, not GPU-accelerated scaling."
      },
      {
        "text": "SKCanvasView tanks performance, SKGLView handles it fine.",
        "source": "issue body",
        "interpretation": "The perf gap is between software raster (SKCanvasView) and GPU-accelerated (SKGLView) — confirms architecture is root cause."
      },
      {
        "text": "I've tried experimenting with different SKSamplingOptions, but none improve it.",
        "source": "issue body",
        "interpretation": "Filter quality changes don't help because the bottleneck is the entire software raster pipeline and putImageData transfer, not just the sampling algorithm."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKCanvasView.razor.cs",
        "lines": "87-108",
        "finding": "OnRenderFrame() creates SKSurface backed by a pinned byte[] array, invokes user paint callback, then calls interop.PutImageData() to transfer the whole pixel buffer to the HTML canvas. Every frame requires a full WASM→JS pixel buffer copy, which is O(width*height*4) bytes.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/wwwroot/SKHtmlCanvas.js",
        "lines": "129-145",
        "finding": "putImageData() calls ctx.putImageData(new ImageData(Uint8ClampedArray from WASM heap), 0, 0). This transfers the entire pixel buffer from WASM linear memory to the browser's ImageData every frame — no GPU compositing, no caching.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKCanvasView.razor.cs",
        "lines": "67-77",
        "finding": "OnAfterRenderAsync calls interop.InitRaster() (not InitGL), locking this view to software raster mode. There is no option to switch to GPU-backed rendering without using SKGLView.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Can the Blazor SKCanvasView be enhanced to use OffscreenCanvas + GPU compositing for scaling to reduce CPU pressure?",
      "Is there an intermediate option to render to a smaller surface and let the browser scale, as a workaround for image-heavy content?",
      "Does pre-scaling the SKImage to the target size before caching eliminate the per-frame cost?"
    ],
    "workarounds": [
      "Use SKGLView instead of SKCanvasView for image-heavy rendering in Blazor WASM (confirmed smooth by reporter); investigate the DrawLine performance issue separately.",
      "Pre-scale the SKImage to the exact display size once (using SKImage.Resize or drawing to a new SKBitmap at target size), then cache the pre-scaled image and draw 1:1 — this avoids per-frame software resampling.",
      "Reduce the canvas size or use IgnorePixelScaling=true to lower the pixel buffer size that must be transferred each frame."
    ],
    "resolution": {
      "hypothesis": "SKCanvasView in Blazor has a fundamental architecture mismatch: it uses software rasterization + full-frame putImageData, which is extremely expensive for scaled image drawing in WASM. Fixing properly would require offloading compositing to the browser GPU or using an intermediate OffscreenCanvas.",
      "proposals": [
        {
          "title": "Pre-scale image to display size",
          "description": "Create a pre-scaled version of the SKImage at the target display dimensions and cache it. Draw 1:1 each frame to avoid per-frame software resampling.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Investigate OffscreenCanvas + GPU compositing",
          "description": "Explore whether SKCanvasView's putImageData approach could be replaced with a createImageBitmap + drawImage call that leverages browser GPU compositing for scaling operations.",
          "category": "investigation",
          "confidence": 0.6,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Pre-scale image to display size",
      "recommendedReason": "Immediately actionable workaround with no library changes needed. Eliminates per-frame software scaling cost entirely."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Real performance bug with clear repro numbers and a known root cause (software raster + putImageData). Needs investigation into whether a GPU-backed fix is feasible for SKCanvasView, and the reporter needs a workaround.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, Blazor area, WASM, and performance tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Blazor",
          "os/WASM",
          "tenet/performance"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain root cause (software raster) and suggest pre-scaling workaround",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed repro with FPS numbers!\n\n**Root cause:** `SKCanvasView` in Blazor WASM uses a software raster pipeline — Skia draws into a pinned `byte[]` in WASM memory, then the entire pixel buffer is transferred to the HTML canvas every frame via `ctx.putImageData()`. When you scale an image, Skia must compute filtered pixel values for every output pixel in CPU/software, and then transfer a potentially large buffer (e.g. 1600×1600×4 ≈ 10MB) across the WASM/JS boundary on every frame. This is fundamentally different from `SKGLView`, which uses WebGL so GPU hardware handles scaling with no per-pixel CPU work.\n\n**Workaround:** Pre-scale the image to the exact display dimensions once and cache the result:\n```csharp\n// Do this once when the image or target size changes\nvar scaledBitmap = new SKBitmap(displayWidth, displayHeight);\nusing var canvas = new SKCanvas(scaledBitmap);\ncanvas.DrawImage(sourceImage, new SKRect(0, 0, displayWidth, displayHeight), SKSamplingOptions.Default);\nvar scaledImage = SKImage.FromBitmap(scaledBitmap);\n\n// Then each frame, draw 1:1 (no scaling = no per-frame resampling cost)\ncanvas.DrawImage(scaledImage, 0, 0);\n```\n\nThis eliminates per-frame software resampling entirely.\n\nWe're tracking whether `SKCanvasView` in Blazor can be improved to use GPU-backed compositing for scaling operations."
      }
    ]
  }
}
```

</details>
