# Issue Triage Report — #765

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-27T09:58:35Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views (0.92 (92%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** SKCanvas.Clear() renders a lighter color (128,128,128 → 188,188,188) in SKGLControl on AMD Radeon HD 7600M/7640G (Windows 10, v1.68) — consistent with sRGB gamma applied by GL_FRAMEBUFFER_SRGB on AMD drivers; SKControl (CPU raster) is unaffected.

**Analysis:** SKGLControl creates an OpenGL-backed Skia surface without specifying a color space (null). On AMD Radeon GPUs, the OpenGL driver enables GL_FRAMEBUFFER_SRGB by default, applying gamma expansion when writing to the framebuffer. Linear color 128/255 ≈ 0.502 converted through sRGB gamma yields ≈ 0.735 ≈ 188/255, which exactly matches the reported (188, 188, 188). NVIDIA drivers likely do not enable GL_FRAMEBUFFER_SRGB by default, so the problem does not appear there. The regression in v1.68 suggests a change in color-space handling in that release enabled sRGB framebuffers on AMD.

**Recommendations:** **needs-investigation** — Root cause hypothesis is strong (sRGB gamma mismatch) and regression is confirmed to v1.68, but the exact change in v1.68 and the correct fix location (GL.Disable vs color space) needs verification on AMD hardware.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Classic |
| Backends | backend/OpenGL |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, os/Windows-Classic, area/SkiaSharp.Views, backend/Raster, backend/OpenGL |

## Evidence

### Reproduction

1. Create a Windows Forms app with SKGLControl on a machine with AMD Radeon HD 7600M/7640G
2. Override OnPaintSurface and call canvas.Clear(new SKColor(128, 128, 128, 255))
3. Observe the rendered color — Photoshop reports (188, 188, 188) instead of (128, 128, 128)

**Environment:** AMD Radeon HD 7600M/7640G, Windows 10, Visual Studio, SkiaSharp 1.68

**Repository links:**
- https://user-images.githubusercontent.com/1189037/51404113-bf2a4f00-1b52-11e9-8a74-b520086222bd.PNG — Screenshot showing lighter color in SKGLControl

**Code snippets:**

```csharp
canvas.Clear(new SkiaSharp.SKColor(128, 128, 128, 255));
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | SKCanvas.Clear() color (128,128,128) is displayed as (188,188,188) in SKGLControl on AMD Radeon HD 7600M/7640G |
| Repro quality | complete |
| Target frameworks | net-framework |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68 |
| Worked in | — |
| Broke in | 1.68 |
| Current relevance | likely |
| Relevance reason | SKGLControl still creates surfaces without specifying a color space, and the GL_FRAMEBUFFER_SRGB interaction with AMD drivers has not been addressed. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.90 (90%) |
| Reason | Reporter explicitly states the behavior was introduced in v1.68; prior versions worked correctly. |
| Worked in version | — |
| Broke in version | 1.68 |

## Analysis

### Technical Summary

SKGLControl creates an OpenGL-backed Skia surface without specifying a color space (null). On AMD Radeon GPUs, the OpenGL driver enables GL_FRAMEBUFFER_SRGB by default, applying gamma expansion when writing to the framebuffer. Linear color 128/255 ≈ 0.502 converted through sRGB gamma yields ≈ 0.735 ≈ 188/255, which exactly matches the reported (188, 188, 188). NVIDIA drivers likely do not enable GL_FRAMEBUFFER_SRGB by default, so the problem does not appear there. The regression in v1.68 suggests a change in color-space handling in that release enabled sRGB framebuffers on AMD.

### Rationale

The color shift is mathematically consistent with sRGB gamma applied once by the GL_FRAMEBUFFER_SRGB extension. AMD drivers enable this extension by default; NVIDIA does not. The regression to v1.68 implies a change in how Skia or SKGLControl configures the GL context/surface enabled this extension. The fix is to either explicitly disable GL_FRAMEBUFFER_SRGB before rendering or to create the surface with an appropriate color space so Skia can compensate.

### Key Signals

- "SKCanvas.Clear() with (128,128,128) → Photoshop reports (188,188,188)" — **issue body** (The shift from 128 to 188 is consistent with sRGB gamma expansion: 0.502^(1/2.2) ≈ 0.735 ≈ 188/255.)
- "On NVIDIA machines everything works fine." — **issue body** (NVIDIA drivers likely do not enable GL_FRAMEBUFFER_SRGB by default, while AMD drivers do. GPU-driver difference, not a Skia math error.)
- "This strange behavior was introduced by 1.68 version." — **comment by antongit** (A change in v1.68 enabled or exposed the sRGB framebuffer path on AMD hardware.)
- "SKControl behaves correctly." — **issue body** (CPU raster path is unaffected, confirming the issue is in the OpenGL surface creation, not in Skia's color math.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs` | 121 | direct | SKSurface.Create is called without a color space argument (null), which resolves to the overload `Create(context, renderTarget, origin, colorType, null, null)`. With no explicit linear or sRGB color space, Skia may not compensate for sRGB framebuffer encoding applied by AMD's OpenGL driver. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs` | 17 | direct | colorType is fixed to SKColorType.Rgba8888, and surfaceOrigin to GRSurfaceOrigin.BottomLeft. No sRGB or linear color space is set, leaving the surface color interpretation up to the driver's framebuffer configuration. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKControl.cs` | 42-43 | related | SKControl creates a CPU raster surface via SKSurface.Create(info, ptr, stride) with SKImageInfo.PlatformColorType — no GPU framebuffer involved, so GL_FRAMEBUFFER_SRGB has no effect. This explains why SKControl is unaffected. |

### Workarounds

- Disable sRGB framebuffer before paint: add `GL.Disable((EnableCap)0x8DB9);` (GL_FRAMEBUFFER_SRGB = 0x8DB9) at the start of OnPaintSurface before calling canvas.Clear()
- Use SKColorSpace.CreateSrgbLinear() when creating the surface to tell Skia to treat the framebuffer as linear, preventing double gamma application

### Next Questions

- What exactly changed in v1.68 to expose GL_FRAMEBUFFER_SRGB on AMD drivers?
- Does passing SKColorSpace.CreateSrgb() to SKSurface.Create fully compensate for the AMD driver's GL_FRAMEBUFFER_SRGB, or is GL.Disable required?

### Resolution Proposals

**Hypothesis:** AMD's OpenGL driver enables GL_FRAMEBUFFER_SRGB by default, causing gamma expansion on framebuffer writes. SKGLControl does not disable this or set a matching color space on the Skia surface.

1. **Disable GL_FRAMEBUFFER_SRGB in SKGLControl.OnPaint** — fix, confidence 0.75 (75%), cost/s, validated=untested
   - Add GL.Disable(EnableCap.FramebufferSrgb) after MakeCurrent() to prevent AMD drivers from applying sRGB gamma to framebuffer writes.
2. **Workaround: disable GL_FRAMEBUFFER_SRGB in OnPaintSurface override** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - User can call GL.Disable((EnableCap)0x8DB9) at the start of their OnPaintSurface override before drawing. This avoids waiting for an upstream fix.

**Recommended proposal:** Disable GL_FRAMEBUFFER_SRGB in SKGLControl.OnPaint

**Why:** Centralizes the fix in the control so all users benefit without requiring per-app workarounds.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Root cause hypothesis is strong (sRGB gamma mismatch) and regression is confirmed to v1.68, but the exact change in v1.68 and the correct fix location (GL.Disable vs color space) needs verification on AMD hardware. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.93 (93%) | Apply correct labels: type/bug, area/SkiaSharp.Views, os/Windows-Classic, backend/OpenGL, tenet/reliability. Remove incorrect backend/Raster. | labels=type/bug, area/SkiaSharp.Views, os/Windows-Classic, backend/OpenGL, tenet/reliability |
| add-comment | medium | 0.82 (82%) | Share color-shift analysis and GL_FRAMEBUFFER_SRGB workaround with reporter | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and screenshot!

The color shift from `(128,128,128)` to `(188,188,188)` is mathematically consistent with sRGB gamma expansion: `(128/255)^(1/2.2) ≈ 0.735 ≈ 188/255`. This strongly suggests that the AMD driver is enabling `GL_FRAMEBUFFER_SRGB` by default, causing the GPU to apply gamma correction when writing to the framebuffer. NVIDIA drivers appear to have this disabled by default.

**Workaround you can try now:**

Add the following line at the start of your `OnPaintSurface` override, before any drawing:

```csharp
OpenTK.Graphics.ES20.GL.Disable((OpenTK.Graphics.ES20.EnableCap)0x8DB9); // GL_FRAMEBUFFER_SRGB
```

This disables the sRGB framebuffer extension and should restore correct color output on AMD hardware.

We'll investigate what changed in v1.68 that exposed this on AMD GPUs.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 765,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-27T09:58:35Z",
    "currentLabels": [
      "type/bug",
      "os/Windows-Classic",
      "area/SkiaSharp.Views",
      "backend/Raster",
      "backend/OpenGL"
    ]
  },
  "summary": "SKCanvas.Clear() renders a lighter color (128,128,128 → 188,188,188) in SKGLControl on AMD Radeon HD 7600M/7640G (Windows 10, v1.68) — consistent with sRGB gamma applied by GL_FRAMEBUFFER_SRGB on AMD drivers; SKControl (CPU raster) is unaffected.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.92
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "SKCanvas.Clear() color (128,128,128) is displayed as (188,188,188) in SKGLControl on AMD Radeon HD 7600M/7640G",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net-framework"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Windows Forms app with SKGLControl on a machine with AMD Radeon HD 7600M/7640G",
        "Override OnPaintSurface and call canvas.Clear(new SKColor(128, 128, 128, 255))",
        "Observe the rendered color — Photoshop reports (188, 188, 188) instead of (128, 128, 128)"
      ],
      "environmentDetails": "AMD Radeon HD 7600M/7640G, Windows 10, Visual Studio, SkiaSharp 1.68",
      "repoLinks": [
        {
          "url": "https://user-images.githubusercontent.com/1189037/51404113-bf2a4f00-1b52-11e9-8a74-b520086222bd.PNG",
          "description": "Screenshot showing lighter color in SKGLControl"
        }
      ],
      "codeSnippets": [
        "canvas.Clear(new SkiaSharp.SKColor(128, 128, 128, 255));"
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68"
      ],
      "brokeIn": "1.68",
      "currentRelevance": "likely",
      "relevanceReason": "SKGLControl still creates surfaces without specifying a color space, and the GL_FRAMEBUFFER_SRGB interaction with AMD drivers has not been addressed."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.9,
      "reason": "Reporter explicitly states the behavior was introduced in v1.68; prior versions worked correctly.",
      "brokeInVersion": "1.68"
    }
  },
  "analysis": {
    "summary": "SKGLControl creates an OpenGL-backed Skia surface without specifying a color space (null). On AMD Radeon GPUs, the OpenGL driver enables GL_FRAMEBUFFER_SRGB by default, applying gamma expansion when writing to the framebuffer. Linear color 128/255 ≈ 0.502 converted through sRGB gamma yields ≈ 0.735 ≈ 188/255, which exactly matches the reported (188, 188, 188). NVIDIA drivers likely do not enable GL_FRAMEBUFFER_SRGB by default, so the problem does not appear there. The regression in v1.68 suggests a change in color-space handling in that release enabled sRGB framebuffers on AMD.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs",
        "lines": "121",
        "finding": "SKSurface.Create is called without a color space argument (null), which resolves to the overload `Create(context, renderTarget, origin, colorType, null, null)`. With no explicit linear or sRGB color space, Skia may not compensate for sRGB framebuffer encoding applied by AMD's OpenGL driver.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs",
        "lines": "17",
        "finding": "colorType is fixed to SKColorType.Rgba8888, and surfaceOrigin to GRSurfaceOrigin.BottomLeft. No sRGB or linear color space is set, leaving the surface color interpretation up to the driver's framebuffer configuration.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKControl.cs",
        "lines": "42-43",
        "finding": "SKControl creates a CPU raster surface via SKSurface.Create(info, ptr, stride) with SKImageInfo.PlatformColorType — no GPU framebuffer involved, so GL_FRAMEBUFFER_SRGB has no effect. This explains why SKControl is unaffected.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "SKCanvas.Clear() with (128,128,128) → Photoshop reports (188,188,188)",
        "source": "issue body",
        "interpretation": "The shift from 128 to 188 is consistent with sRGB gamma expansion: 0.502^(1/2.2) ≈ 0.735 ≈ 188/255."
      },
      {
        "text": "On NVIDIA machines everything works fine.",
        "source": "issue body",
        "interpretation": "NVIDIA drivers likely do not enable GL_FRAMEBUFFER_SRGB by default, while AMD drivers do. GPU-driver difference, not a Skia math error."
      },
      {
        "text": "This strange behavior was introduced by 1.68 version.",
        "source": "comment by antongit",
        "interpretation": "A change in v1.68 enabled or exposed the sRGB framebuffer path on AMD hardware."
      },
      {
        "text": "SKControl behaves correctly.",
        "source": "issue body",
        "interpretation": "CPU raster path is unaffected, confirming the issue is in the OpenGL surface creation, not in Skia's color math."
      }
    ],
    "rationale": "The color shift is mathematically consistent with sRGB gamma applied once by the GL_FRAMEBUFFER_SRGB extension. AMD drivers enable this extension by default; NVIDIA does not. The regression to v1.68 implies a change in how Skia or SKGLControl configures the GL context/surface enabled this extension. The fix is to either explicitly disable GL_FRAMEBUFFER_SRGB before rendering or to create the surface with an appropriate color space so Skia can compensate.",
    "workarounds": [
      "Disable sRGB framebuffer before paint: add `GL.Disable((EnableCap)0x8DB9);` (GL_FRAMEBUFFER_SRGB = 0x8DB9) at the start of OnPaintSurface before calling canvas.Clear()",
      "Use SKColorSpace.CreateSrgbLinear() when creating the surface to tell Skia to treat the framebuffer as linear, preventing double gamma application"
    ],
    "nextQuestions": [
      "What exactly changed in v1.68 to expose GL_FRAMEBUFFER_SRGB on AMD drivers?",
      "Does passing SKColorSpace.CreateSrgb() to SKSurface.Create fully compensate for the AMD driver's GL_FRAMEBUFFER_SRGB, or is GL.Disable required?"
    ],
    "resolution": {
      "hypothesis": "AMD's OpenGL driver enables GL_FRAMEBUFFER_SRGB by default, causing gamma expansion on framebuffer writes. SKGLControl does not disable this or set a matching color space on the Skia surface.",
      "proposals": [
        {
          "title": "Disable GL_FRAMEBUFFER_SRGB in SKGLControl.OnPaint",
          "description": "Add GL.Disable(EnableCap.FramebufferSrgb) after MakeCurrent() to prevent AMD drivers from applying sRGB gamma to framebuffer writes.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Workaround: disable GL_FRAMEBUFFER_SRGB in OnPaintSurface override",
          "description": "User can call GL.Disable((EnableCap)0x8DB9) at the start of their OnPaintSurface override before drawing. This avoids waiting for an upstream fix.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Disable GL_FRAMEBUFFER_SRGB in SKGLControl.OnPaint",
      "recommendedReason": "Centralizes the fix in the control so all users benefit without requiring per-app workarounds."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Root cause hypothesis is strong (sRGB gamma mismatch) and regression is confirmed to v1.68, but the exact change in v1.68 and the correct fix location (GL.Disable vs color space) needs verification on AMD hardware.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply correct labels: type/bug, area/SkiaSharp.Views, os/Windows-Classic, backend/OpenGL, tenet/reliability. Remove incorrect backend/Raster.",
        "risk": "low",
        "confidence": 0.93,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-Classic",
          "backend/OpenGL",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Share color-shift analysis and GL_FRAMEBUFFER_SRGB workaround with reporter",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report and screenshot!\n\nThe color shift from `(128,128,128)` to `(188,188,188)` is mathematically consistent with sRGB gamma expansion: `(128/255)^(1/2.2) ≈ 0.735 ≈ 188/255`. This strongly suggests that the AMD driver is enabling `GL_FRAMEBUFFER_SRGB` by default, causing the GPU to apply gamma correction when writing to the framebuffer. NVIDIA drivers appear to have this disabled by default.\n\n**Workaround you can try now:**\n\nAdd the following line at the start of your `OnPaintSurface` override, before any drawing:\n\n```csharp\nOpenTK.Graphics.ES20.GL.Disable((OpenTK.Graphics.ES20.EnableCap)0x8DB9); // GL_FRAMEBUFFER_SRGB\n```\n\nThis disables the sRGB framebuffer extension and should restore correct color output on AMD hardware.\n\nWe'll investigate what changed in v1.68 that exposed this on AMD GPUs."
      }
    ]
  }
}
```

</details>
