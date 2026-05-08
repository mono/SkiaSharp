# Issue Triage Report — #2163

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T20:58:00Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp (0.85 (85%)) |
| Suggested action | close-as-external (0.78 (78%)) |

**Issue Summary:** Drawing with a large blur radius produces visually different results when rendered on CPU vs GPU on Windows, using SkiaSharp 2.88.0.

**Analysis:** CPU and GPU backends in Skia apply blur filters differently for large sigma values. The CPU (software rasterizer) computes a full Gaussian blur, while the GPU backend approximates it via multi-pass down-sampling or caps the kernel size, producing visible differences at large radii. This is a known Skia upstream behavior, not a SkiaSharp wrapper defect.

**Recommendations:** **close-as-external** — The visual difference between CPU and GPU blur at large sigma values is a known Skia upstream limitation in how the GPU rasterizer approximates large Gaussian kernels. The SkiaSharp C# wrappers pass sigma through unmodified and cannot address this. The fix would need to come from upstream Skia's GPU blur implementation.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/Raster, backend/Direct3D |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create a canvas backed by CPU (Raster) surface
2. Create a canvas backed by GPU (OpenGL/Direct3D) surface
3. Apply SKMaskFilter.CreateBlur() or SKImageFilter.CreateBlur() with a large sigma/radius
4. Draw the same shape with both canvases
5. Compare the output images — the GPU result shows a noticeably different blur appearance

**Environment:** Windows 10, NVIDIA RTX 2070 driver 511.79, SkiaSharp 2.88.0, Visual Studio 2022

**Repository links:**
- https://github.com/xiejiang2014/SkiaSharpBlurTest — Minimal repro project demonstrating CPU vs GPU blur difference

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | — |
| Repro quality | complete |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | GPU blur limitations in Skia are fundamental to how the GPU rasterizer approximates large Gaussian sigmas; this architectural difference has persisted across versions. |

## Analysis

### Technical Summary

CPU and GPU backends in Skia apply blur filters differently for large sigma values. The CPU (software rasterizer) computes a full Gaussian blur, while the GPU backend approximates it via multi-pass down-sampling or caps the kernel size, producing visible differences at large radii. This is a known Skia upstream behavior, not a SkiaSharp wrapper defect.

### Rationale

Reporter provides a screenshot and a repro repository clearly showing visual divergence between CPU and GPU blur output at large radius. The blur APIs (SKMaskFilter.CreateBlur, SKImageFilter.CreateBlur) are thin pass-throughs to Skia native code. The divergence for large sigma is a documented Skia limitation in the GPU path — the GPU rasterizer caps or approximates very large kernels due to GPU texture size constraints, while the CPU path computes exactly. No regression claimed; this is the initial observation.

### Key Signals

- "Drawing with CPU and GPU gives different results when using larger blur radius" — **issue title and body** (Visual divergence is GPU-backend-specific at large blur sigma values.)
- "NVIDIA RTX 2070 with driver 511.79" — **issue body** (Reporter is using hardware GPU — confirms the GPU render path is active, not a software fallback.)
- "Repro link: https://github.com/xiejiang2014/SkiaSharpBlurTest" — **issue body** (Complete self-contained repro provided, making investigation straightforward.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKMaskFilter.cs` | 34-41 | direct | SKMaskFilter.CreateBlur() delegates directly to sk_maskfilter_new_blur / sk_maskfilter_new_blur_with_flags C API — no clamping or sigma transformation is applied at the C# layer. Large sigma values are passed through unmodified to Skia's native blur implementation. |
| `binding/SkiaSharp/SKImageFilter.cs` | 39-58 | direct | SKImageFilter.CreateBlur() similarly passes sigmaX/sigmaY directly to sk_imagefilter_new_blur with no clamping. Both blur APIs are transparent pass-throughs, meaning any difference in GPU vs CPU output originates in Skia's native rendering layer. |
| `binding/SkiaSharp/GRContext.cs` | 1-30 | related | GRContext wraps the Skia GPU context (GrContext/GrDirectContext). GPU surfaces use this context for rendering. The GPU rasterizer applies different code paths for image filters (including blur) compared to the CPU rasterizer — this is where the divergence originates. |

### Workarounds

- Render on CPU surface (SKSurface.Create with SKImageInfo) and blit to GPU surface to achieve consistent blur appearance.
- Limit blur sigma to smaller values (< ~50) where GPU and CPU results are visually indistinguishable.
- Use SKImageFilter.CreateBlur() with tileMode parameter to better control edge behavior, which may reduce visible divergence.

### Next Questions

- What sigma value triggers the visible divergence? The repro repo may document this.
- Does the same divergence occur with SKImageFilter.CreateBlur vs SKMaskFilter.CreateBlur?
- Has the reporter verified the behavior with SkiaSharp 3.x?

### Resolution Proposals

**Hypothesis:** The GPU backend in Skia approximates large Gaussian blur kernels using multi-pass techniques or caps the kernel size due to GPU texture constraints, while the CPU path computes full exact convolution. This is an upstream Skia limitation with no SkiaSharp-layer fix.

1. **Use CPU rendering for large blur operations** — workaround, confidence 0.82 (82%), cost/s, validated=untested
   - Render the blurred content onto a CPU-backed SKSurface, obtain the resulting SKImage, and draw that image onto the GPU canvas. This ensures consistent Gaussian blur output regardless of GPU driver.
2. **Cap sigma to GPU-safe range** — workaround, confidence 0.70 (70%), cost/xs, validated=untested
   - Determine experimentally the sigma threshold at which GPU and CPU output diverge visually on target hardware. Clamp blur sigma below that threshold in the application.
3. **File upstream Skia issue for large-sigma GPU blur consistency** — investigation, confidence 0.75 (75%), cost/s, validated=untested
   - The root cause is in Skia's GPU blur implementation. File a bug at bugs.chromium.org/p/skia to request consistent large-sigma blur behavior across CPU and GPU backends.

**Recommended proposal:** Use CPU rendering for large blur operations

**Why:** Immediate workaround that gives consistent output without requiring upstream Skia changes.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.78 (78%) |
| Reason | The visual difference between CPU and GPU blur at large sigma values is a known Skia upstream limitation in how the GPU rasterizer approximates large Gaussian kernels. The SkiaSharp C# wrappers pass sigma through unmodified and cannot address this. The fix would need to come from upstream Skia's GPU blur implementation. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug classification labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, backend/Raster, backend/Direct3D, tenet/reliability |
| add-comment | medium | 0.78 (78%) | Explain GPU vs CPU blur divergence as a Skia upstream limitation with a CPU rendering workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report and repro project!

This difference in blur output between CPU and GPU rendering is a known limitation in the underlying Skia library rather than a SkiaSharp-specific bug. The GPU rasterizer approximates large Gaussian blur kernels using multi-pass techniques or caps the kernel size due to GPU texture size constraints, while the CPU (software) rasterizer computes the exact convolution. This divergence becomes more visible as the sigma/radius increases.

SkiaSharp's `SKMaskFilter.CreateBlur()` and `SKImageFilter.CreateBlur()` pass the sigma value directly to the native Skia library without modification, so there's no SkiaSharp-layer fix available.

**Workaround:** Render the blurred content onto a CPU-backed surface first, then draw the result onto your GPU canvas:

```csharp
// Render with CPU for consistent blur output
using var cpuSurface = SKSurface.Create(imageInfo);
var cpuCanvas = cpuSurface.Canvas;
// ... draw blurred content on cpuCanvas ...
using var blurredImage = cpuSurface.Snapshot();

// Blit to your GPU canvas
gpuCanvas.DrawImage(blurredImage, 0, 0);
```

Alternatively, limiting blur sigma to smaller values (typically below ~50) tends to produce more consistent results across both backends.

If consistent GPU blur at large sigma is critical, please consider filing an upstream Skia issue at https://issues.skia.org — that's where the fix would need to land.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2163,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T20:58:00Z"
  },
  "summary": "Drawing with a large blur radius produces visually different results when rendered on CPU vs GPU on Windows, using SkiaSharp 2.88.0.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.85
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/Raster",
      "backend/Direct3D"
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
      "reproQuality": "complete",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a canvas backed by CPU (Raster) surface",
        "Create a canvas backed by GPU (OpenGL/Direct3D) surface",
        "Apply SKMaskFilter.CreateBlur() or SKImageFilter.CreateBlur() with a large sigma/radius",
        "Draw the same shape with both canvases",
        "Compare the output images — the GPU result shows a noticeably different blur appearance"
      ],
      "environmentDetails": "Windows 10, NVIDIA RTX 2070 driver 511.79, SkiaSharp 2.88.0, Visual Studio 2022",
      "repoLinks": [
        {
          "url": "https://github.com/xiejiang2014/SkiaSharpBlurTest",
          "description": "Minimal repro project demonstrating CPU vs GPU blur difference"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "GPU blur limitations in Skia are fundamental to how the GPU rasterizer approximates large Gaussian sigmas; this architectural difference has persisted across versions."
    }
  },
  "analysis": {
    "summary": "CPU and GPU backends in Skia apply blur filters differently for large sigma values. The CPU (software rasterizer) computes a full Gaussian blur, while the GPU backend approximates it via multi-pass down-sampling or caps the kernel size, producing visible differences at large radii. This is a known Skia upstream behavior, not a SkiaSharp wrapper defect.",
    "rationale": "Reporter provides a screenshot and a repro repository clearly showing visual divergence between CPU and GPU blur output at large radius. The blur APIs (SKMaskFilter.CreateBlur, SKImageFilter.CreateBlur) are thin pass-throughs to Skia native code. The divergence for large sigma is a documented Skia limitation in the GPU path — the GPU rasterizer caps or approximates very large kernels due to GPU texture size constraints, while the CPU path computes exactly. No regression claimed; this is the initial observation.",
    "keySignals": [
      {
        "text": "Drawing with CPU and GPU gives different results when using larger blur radius",
        "source": "issue title and body",
        "interpretation": "Visual divergence is GPU-backend-specific at large blur sigma values."
      },
      {
        "text": "NVIDIA RTX 2070 with driver 511.79",
        "source": "issue body",
        "interpretation": "Reporter is using hardware GPU — confirms the GPU render path is active, not a software fallback."
      },
      {
        "text": "Repro link: https://github.com/xiejiang2014/SkiaSharpBlurTest",
        "source": "issue body",
        "interpretation": "Complete self-contained repro provided, making investigation straightforward."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKMaskFilter.cs",
        "lines": "34-41",
        "finding": "SKMaskFilter.CreateBlur() delegates directly to sk_maskfilter_new_blur / sk_maskfilter_new_blur_with_flags C API — no clamping or sigma transformation is applied at the C# layer. Large sigma values are passed through unmodified to Skia's native blur implementation.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImageFilter.cs",
        "lines": "39-58",
        "finding": "SKImageFilter.CreateBlur() similarly passes sigmaX/sigmaY directly to sk_imagefilter_new_blur with no clamping. Both blur APIs are transparent pass-throughs, meaning any difference in GPU vs CPU output originates in Skia's native rendering layer.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/GRContext.cs",
        "lines": "1-30",
        "finding": "GRContext wraps the Skia GPU context (GrContext/GrDirectContext). GPU surfaces use this context for rendering. The GPU rasterizer applies different code paths for image filters (including blur) compared to the CPU rasterizer — this is where the divergence originates.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Render on CPU surface (SKSurface.Create with SKImageInfo) and blit to GPU surface to achieve consistent blur appearance.",
      "Limit blur sigma to smaller values (< ~50) where GPU and CPU results are visually indistinguishable.",
      "Use SKImageFilter.CreateBlur() with tileMode parameter to better control edge behavior, which may reduce visible divergence."
    ],
    "nextQuestions": [
      "What sigma value triggers the visible divergence? The repro repo may document this.",
      "Does the same divergence occur with SKImageFilter.CreateBlur vs SKMaskFilter.CreateBlur?",
      "Has the reporter verified the behavior with SkiaSharp 3.x?"
    ],
    "resolution": {
      "hypothesis": "The GPU backend in Skia approximates large Gaussian blur kernels using multi-pass techniques or caps the kernel size due to GPU texture constraints, while the CPU path computes full exact convolution. This is an upstream Skia limitation with no SkiaSharp-layer fix.",
      "proposals": [
        {
          "title": "Use CPU rendering for large blur operations",
          "description": "Render the blurred content onto a CPU-backed SKSurface, obtain the resulting SKImage, and draw that image onto the GPU canvas. This ensures consistent Gaussian blur output regardless of GPU driver.",
          "category": "workaround",
          "confidence": 0.82,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Cap sigma to GPU-safe range",
          "description": "Determine experimentally the sigma threshold at which GPU and CPU output diverge visually on target hardware. Clamp blur sigma below that threshold in the application.",
          "category": "workaround",
          "confidence": 0.7,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "File upstream Skia issue for large-sigma GPU blur consistency",
          "description": "The root cause is in Skia's GPU blur implementation. File a bug at bugs.chromium.org/p/skia to request consistent large-sigma blur behavior across CPU and GPU backends.",
          "category": "investigation",
          "confidence": 0.75,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use CPU rendering for large blur operations",
      "recommendedReason": "Immediate workaround that gives consistent output without requiring upstream Skia changes."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.78,
      "reason": "The visual difference between CPU and GPU blur at large sigma values is a known Skia upstream limitation in how the GPU rasterizer approximates large Gaussian kernels. The SkiaSharp C# wrappers pass sigma through unmodified and cannot address this. The fix would need to come from upstream Skia's GPU blur implementation.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug classification labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/Raster",
          "backend/Direct3D",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain GPU vs CPU blur divergence as a Skia upstream limitation with a CPU rendering workaround",
        "risk": "medium",
        "confidence": 0.78,
        "comment": "Thank you for the detailed report and repro project!\n\nThis difference in blur output between CPU and GPU rendering is a known limitation in the underlying Skia library rather than a SkiaSharp-specific bug. The GPU rasterizer approximates large Gaussian blur kernels using multi-pass techniques or caps the kernel size due to GPU texture size constraints, while the CPU (software) rasterizer computes the exact convolution. This divergence becomes more visible as the sigma/radius increases.\n\nSkiaSharp's `SKMaskFilter.CreateBlur()` and `SKImageFilter.CreateBlur()` pass the sigma value directly to the native Skia library without modification, so there's no SkiaSharp-layer fix available.\n\n**Workaround:** Render the blurred content onto a CPU-backed surface first, then draw the result onto your GPU canvas:\n\n```csharp\n// Render with CPU for consistent blur output\nusing var cpuSurface = SKSurface.Create(imageInfo);\nvar cpuCanvas = cpuSurface.Canvas;\n// ... draw blurred content on cpuCanvas ...\nusing var blurredImage = cpuSurface.Snapshot();\n\n// Blit to your GPU canvas\ngpuCanvas.DrawImage(blurredImage, 0, 0);\n```\n\nAlternatively, limiting blur sigma to smaller values (typically below ~50) tends to produce more consistent results across both backends.\n\nIf consistent GPU blur at large sigma is critical, please consider filing an upstream Skia issue at https://issues.skia.org — that's where the fix would need to land."
      }
    ]
  }
}
```

</details>
