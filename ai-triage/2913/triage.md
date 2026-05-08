# Issue Triage Report — #2913

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-23T16:37:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** SKGLView on Windows with Intel UHD Graphics intermittently renders a stray horizontal (or vertical) colored line across the screen at certain zoom levels, believed to be a texture sampling off-by-one error in the ANGLE/Direct3D rendering path.

**Analysis:** SKGLView on Windows uses ANGLE (OpenGL ES over Direct3D 11) via SKSwapChainPanel/AngleSwapChainPanel. The rendering path uses GRSurfaceOrigin.BottomLeft and EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE. On Intel UHD Graphics, bilinear texture filtering samples adjacent pixels at atlas sprite boundaries when coordinates land on odd pixel values, causing color bleeding as stray lines. The reporter found a workaround (+1.0f to source rect left and destination rect bottom in DrawImage calls) suggesting two off-by-one coordinate errors in the Windows GL rendering path.

**Recommendations:** **needs-investigation** — Real bug with visual evidence (video + screenshot) and a partial user-identified workaround. Root cause points to ANGLE/Intel GPU interaction in the SwapChainPanel rendering path; needs a minimal repro and targeted investigation of EGL fast-path and surface origin settings.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-WinUI |
| Backends | backend/OpenGL |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Run a Windows app using SKGLView (WinUI SwapChainPanel) with Intel UHD Graphics
2. Draw game graphics using DrawImage at various zoom levels
3. Observe intermittent horizontal red (or vertical green) line appearing across the display

**Environment:** Windows 11, Intel UHD Graphics, SkiaSharp 3.x Alpha (3.0 Preview 3.1), Visual Studio (Windows). NVIDIA card on same laptop does not exhibit the issue.

**Repository links:**
- https://github.com/hyvanmielenpelit/GnollHack — Full game project demonstrating the issue with SKGLView on Windows
- https://github.com/mono/SkiaSharp/assets/16661034/9e1d2e17-25d9-4f4b-9be2-2f12ff98bb6a — Video recording showing the red horizontal line
- https://github.com/mono/SkiaSharp/assets/16661034/b9788dec-3845-45da-9174-f5ad955becbf — Screenshot showing the stray colored line

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Horizontal (or vertical) colored line drawn across the screen by SKGLView |
| Repro quality | partial |
| Target frameworks | net8.0-windows10.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.x Alpha, 3.0 Preview 3.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The SKSwapChainPanel/AngleSwapChainPanel Windows rendering path in the codebase still uses ANGLE with EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE and GRSurfaceOrigin.BottomLeft; no evidence this specific rendering artifact has been addressed. |

## Analysis

### Technical Summary

SKGLView on Windows uses ANGLE (OpenGL ES over Direct3D 11) via SKSwapChainPanel/AngleSwapChainPanel. The rendering path uses GRSurfaceOrigin.BottomLeft and EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE. On Intel UHD Graphics, bilinear texture filtering samples adjacent pixels at atlas sprite boundaries when coordinates land on odd pixel values, causing color bleeding as stray lines. The reporter found a workaround (+1.0f to source rect left and destination rect bottom in DrawImage calls) suggesting two off-by-one coordinate errors in the Windows GL rendering path.

### Rationale

The visual evidence (video + screenshot), GPU-specific behavior (Intel vs NVIDIA), and SKGLView-only reproduction (SKCanvasView unaffected) all confirm a real rendering bug in the ANGLE/Direct3D path. The reporter's workaround pointing to off-by-one in DrawImage rect coordinates is consistent with the OpenGL texture atlas bilinear filtering behavior described in comments. This is a wrong-output bug, not a crash.

### Key Signals

- "SKGLView draws randomly a red horizontal line across the whole screen. A dedicated NVIDIA graphics card on the same laptop works fine. SKCanvasView also works fine." — **issue body** (GPU-specific and view-type-specific: strongly points to the ANGLE/OpenGL ES rendering path rather than core SkiaSharp drawing logic.)
- "This seems to be fixed on Windows by adding +1.0f to source rectangle's left and +1.0f to destination rectangle's bottom in every DrawImage call." — **comment #4 (janne-hmp)** (Two off-by-one coordinate offsets in SKSwapChainPanel DrawImage calls on Windows; implies the surface origin or coordinate mapping is slightly misaligned.)
- "For texture atlases you need to align everything to 2x2 rectangles because bilinear texture filtering will sample 4 adjacent pixels. If you put the sprite boundary on an odd pixel, there will be some color bleeding from the neighboring sprite." — **comment #5 (TommiGustafsson-HMP, quoting OpenGL Reddit)** (Root cause hypothesis: atlas sprite coordinates at sub-pixel boundaries cause bilinear filtering bleed; the bottom-left surface origin flip in ANGLE may produce a half-pixel offset on Intel but not NVIDIA.)
- "I've got a similar problem but green vertical lines on certain zoom levels." — **comment #3 (janne-hmp)** (Both horizontal and vertical artifacts, and only at specific zoom levels, is consistent with texture sampling coordinate errors amplified at non-integer scale factors.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKSwapChainPanel.cs` | 13-14 | direct | SKSwapChainPanel uses GRSurfaceOrigin.BottomLeft — standard OpenGL convention, but ANGLE translates this to Direct3D coordinate space. A subtle off-by-one in this translation could explain Intel-specific artifacts. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/GlesContext.cs` | 163-175 | direct | ANGLE is initialized with EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE. This 'fast path' skips some present-stage compositing steps and could interact differently with Intel's D3D driver vs NVIDIA, potentially introducing a 1-pixel alignment difference. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs` | 228-231 | related | OnRenderFrame is called with a Rect using integer surface dimensions queried from EGL. The rect is passed to SKSwapChainPanel.OnRenderFrame which creates the GRBackendRenderTarget. Coordinates are truncated to int via cast. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.Windows.cs` | 13 | context | MAUI handler wraps MauiSKSwapChainPanel (subclass of SKSwapChainPanel) — same underlying rendering path is used in both MAUI and non-MAUI SKGLView on Windows. |

### Workarounds

- Add +1.0f to the left coordinate of the source rectangle in every DrawImage call on Windows (user-identified workaround).
- Add +1.0f to the bottom coordinate of the destination rectangle in every DrawImage call on Windows (user-identified workaround).
- Use SKCanvasView instead of SKGLView on Windows — unaffected by the OpenGL rendering path.

### Next Questions

- Does the issue reproduce with a minimal SkiaSharp project (not the full game), or only when using sprite atlas DrawImage calls at non-integer scale factors?
- Does removing EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE from the ANGLE init attributes mitigate the issue on Intel?
- Does switching from GRSurfaceOrigin.BottomLeft to GRSurfaceOrigin.TopLeft in SKSwapChainPanel change the behavior?
- Is the off-by-one only in DrawImage, or does it appear with DrawBitmap or other primitives?
- Which SkiaSharp 3.x preview version first exhibited the problem — was this present in 2.x?

### Resolution Proposals

**Hypothesis:** The ANGLE D3D11 fast-path present combined with GRSurfaceOrigin.BottomLeft produces a sub-pixel offset in texture sampling on Intel UHD Graphics drivers, causing bilinear filtering bleed at sprite atlas boundaries. This manifests as stray colored lines at certain zoom/scale levels.

1. **Disable EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE on Intel GPUs** — fix, confidence 0.55 (55%), cost/m, validated=untested
   - Detect Intel GPU and fall back to EGL_EXPERIMENTAL_PRESENT_PATH_COMPATIBLE_ANGLE (or no fast path) to avoid the sub-pixel offset. Requires P/Invoke or DXGI query to detect Intel GPU vendor ID (0x8086).
2. **Switch surface origin to TopLeft for Windows** — investigation, confidence 0.45 (45%), cost/s, validated=untested
   - Try GRSurfaceOrigin.TopLeft in SKSwapChainPanel on Windows to see if the coordinate flip eliminates the off-by-one. D3D convention is top-left; the BottomLeft convention may introduce a 1-pixel shift in the ANGLE translation on some drivers.
3. **Round DrawImage coordinates to nearest even pixel** — workaround, confidence 0.70 (70%), cost/xs, validated=untested
   - Align DrawImage source and destination rectangle coordinates to 2-pixel boundaries to avoid bilinear sampling across atlas sprite borders. This is a user-space recommendation, not a framework change.

**Recommended proposal:** Round DrawImage coordinates to nearest even pixel

**Why:** This user-space workaround directly addresses the bilinear bleeding root cause (misaligned atlas boundaries) without requiring driver-specific hacks, and can be applied immediately while the underlying ANGLE issue is investigated.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Real bug with visual evidence (video + screenshot) and a partial user-identified workaround. Root cause points to ANGLE/Intel GPU interaction in the SwapChainPanel rendering path; needs a minimal repro and targeted investigation of EGL fast-path and surface origin settings. |
| Suggested repro platform | windows |

### Missing Info

- Minimal reproduction project (not the full game) to isolate the issue to SKGLView DrawImage
- Confirmation of which SkiaSharp 3.x preview version first showed the problem
- Whether the issue appears in SkiaSharp 2.x or only in 3.x (to identify if it's a regression)

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply area, platform and backend labels | labels=type/bug, area/SkiaSharp.Views, os/Windows-WinUI, backend/OpenGL, tenet/reliability |
| add-comment | medium | 0.82 (82%) | Acknowledge the visual evidence, explain the suspected root cause, provide workaround, and request a minimal repro | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the video, screenshot, and the repro project link — very helpful!

Based on the evidence (GPU-specific behavior on Intel vs NVIDIA, SKGLView-only, zoom-level dependency), this looks like a texture sampling coordinate issue in the **ANGLE/Direct3D 11** rendering path used by `SKGLView` on Windows (`SKSwapChainPanel`). The behavior is consistent with bilinear texture filtering bleeding across sprite atlas boundaries when coordinates land on sub-pixel values — something that manifests differently depending on the GPU driver's implementation of the OpenGL-to-D3D coordinate transform.

**Workaround (while we investigate):**
As you found, you can compensate by adjusting the source and destination rectangle coordinates in `DrawImage` calls:
```csharp
// Align atlas sprite coordinates to even pixels to avoid bilinear bleed
var srcRect = new SKRect(sprite.Left + 1f, sprite.Top, sprite.Right, sprite.Bottom);
var dstRect = new SKRect(dest.Left, dest.Top, dest.Right, dest.Bottom + 1f);
canvas.DrawImage(image, srcRect, dstRect, paint);
```
Alternatively, aligning all sprite coordinates to 2-pixel boundaries avoids the issue entirely.

**To help us narrow this down further**, could you provide:
1. A minimal standalone SkiaSharp project (not the full game) that reliably triggers the line?
2. Which SkiaSharp 3.x preview version first showed the problem? Did it also occur on 2.x?

We will investigate whether the `EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE` setting or the `GRSurfaceOrigin.BottomLeft` flip in `SKSwapChainPanel` is the root cause.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2913,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-23T16:37:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKGLView on Windows with Intel UHD Graphics intermittently renders a stray horizontal (or vertical) colored line across the screen at certain zoom levels, believed to be a texture sampling off-by-one error in the ANGLE/Direct3D rendering path.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-WinUI"
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
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "Horizontal (or vertical) colored line drawn across the screen by SKGLView",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0-windows10.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Run a Windows app using SKGLView (WinUI SwapChainPanel) with Intel UHD Graphics",
        "Draw game graphics using DrawImage at various zoom levels",
        "Observe intermittent horizontal red (or vertical green) line appearing across the display"
      ],
      "environmentDetails": "Windows 11, Intel UHD Graphics, SkiaSharp 3.x Alpha (3.0 Preview 3.1), Visual Studio (Windows). NVIDIA card on same laptop does not exhibit the issue.",
      "repoLinks": [
        {
          "url": "https://github.com/hyvanmielenpelit/GnollHack",
          "description": "Full game project demonstrating the issue with SKGLView on Windows"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/assets/16661034/9e1d2e17-25d9-4f4b-9be2-2f12ff98bb6a",
          "description": "Video recording showing the red horizontal line"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/assets/16661034/b9788dec-3845-45da-9174-f5ad955becbf",
          "description": "Screenshot showing the stray colored line"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.x Alpha",
        "3.0 Preview 3.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The SKSwapChainPanel/AngleSwapChainPanel Windows rendering path in the codebase still uses ANGLE with EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE and GRSurfaceOrigin.BottomLeft; no evidence this specific rendering artifact has been addressed."
    }
  },
  "analysis": {
    "summary": "SKGLView on Windows uses ANGLE (OpenGL ES over Direct3D 11) via SKSwapChainPanel/AngleSwapChainPanel. The rendering path uses GRSurfaceOrigin.BottomLeft and EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE. On Intel UHD Graphics, bilinear texture filtering samples adjacent pixels at atlas sprite boundaries when coordinates land on odd pixel values, causing color bleeding as stray lines. The reporter found a workaround (+1.0f to source rect left and destination rect bottom in DrawImage calls) suggesting two off-by-one coordinate errors in the Windows GL rendering path.",
    "rationale": "The visual evidence (video + screenshot), GPU-specific behavior (Intel vs NVIDIA), and SKGLView-only reproduction (SKCanvasView unaffected) all confirm a real rendering bug in the ANGLE/Direct3D path. The reporter's workaround pointing to off-by-one in DrawImage rect coordinates is consistent with the OpenGL texture atlas bilinear filtering behavior described in comments. This is a wrong-output bug, not a crash.",
    "keySignals": [
      {
        "text": "SKGLView draws randomly a red horizontal line across the whole screen. A dedicated NVIDIA graphics card on the same laptop works fine. SKCanvasView also works fine.",
        "source": "issue body",
        "interpretation": "GPU-specific and view-type-specific: strongly points to the ANGLE/OpenGL ES rendering path rather than core SkiaSharp drawing logic."
      },
      {
        "text": "This seems to be fixed on Windows by adding +1.0f to source rectangle's left and +1.0f to destination rectangle's bottom in every DrawImage call.",
        "source": "comment #4 (janne-hmp)",
        "interpretation": "Two off-by-one coordinate offsets in SKSwapChainPanel DrawImage calls on Windows; implies the surface origin or coordinate mapping is slightly misaligned."
      },
      {
        "text": "For texture atlases you need to align everything to 2x2 rectangles because bilinear texture filtering will sample 4 adjacent pixels. If you put the sprite boundary on an odd pixel, there will be some color bleeding from the neighboring sprite.",
        "source": "comment #5 (TommiGustafsson-HMP, quoting OpenGL Reddit)",
        "interpretation": "Root cause hypothesis: atlas sprite coordinates at sub-pixel boundaries cause bilinear filtering bleed; the bottom-left surface origin flip in ANGLE may produce a half-pixel offset on Intel but not NVIDIA."
      },
      {
        "text": "I've got a similar problem but green vertical lines on certain zoom levels.",
        "source": "comment #3 (janne-hmp)",
        "interpretation": "Both horizontal and vertical artifacts, and only at specific zoom levels, is consistent with texture sampling coordinate errors amplified at non-integer scale factors."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKSwapChainPanel.cs",
        "lines": "13-14",
        "finding": "SKSwapChainPanel uses GRSurfaceOrigin.BottomLeft — standard OpenGL convention, but ANGLE translates this to Direct3D coordinate space. A subtle off-by-one in this translation could explain Intel-specific artifacts.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/GlesContext.cs",
        "lines": "163-175",
        "finding": "ANGLE is initialized with EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE. This 'fast path' skips some present-stage compositing steps and could interact differently with Intel's D3D driver vs NVIDIA, potentially introducing a 1-pixel alignment difference.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs",
        "lines": "228-231",
        "finding": "OnRenderFrame is called with a Rect using integer surface dimensions queried from EGL. The rect is passed to SKSwapChainPanel.OnRenderFrame which creates the GRBackendRenderTarget. Coordinates are truncated to int via cast.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.Windows.cs",
        "lines": "13",
        "finding": "MAUI handler wraps MauiSKSwapChainPanel (subclass of SKSwapChainPanel) — same underlying rendering path is used in both MAUI and non-MAUI SKGLView on Windows.",
        "relevance": "context"
      }
    ],
    "nextQuestions": [
      "Does the issue reproduce with a minimal SkiaSharp project (not the full game), or only when using sprite atlas DrawImage calls at non-integer scale factors?",
      "Does removing EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE from the ANGLE init attributes mitigate the issue on Intel?",
      "Does switching from GRSurfaceOrigin.BottomLeft to GRSurfaceOrigin.TopLeft in SKSwapChainPanel change the behavior?",
      "Is the off-by-one only in DrawImage, or does it appear with DrawBitmap or other primitives?",
      "Which SkiaSharp 3.x preview version first exhibited the problem — was this present in 2.x?"
    ],
    "workarounds": [
      "Add +1.0f to the left coordinate of the source rectangle in every DrawImage call on Windows (user-identified workaround).",
      "Add +1.0f to the bottom coordinate of the destination rectangle in every DrawImage call on Windows (user-identified workaround).",
      "Use SKCanvasView instead of SKGLView on Windows — unaffected by the OpenGL rendering path."
    ],
    "resolution": {
      "hypothesis": "The ANGLE D3D11 fast-path present combined with GRSurfaceOrigin.BottomLeft produces a sub-pixel offset in texture sampling on Intel UHD Graphics drivers, causing bilinear filtering bleed at sprite atlas boundaries. This manifests as stray colored lines at certain zoom/scale levels.",
      "proposals": [
        {
          "title": "Disable EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE on Intel GPUs",
          "description": "Detect Intel GPU and fall back to EGL_EXPERIMENTAL_PRESENT_PATH_COMPATIBLE_ANGLE (or no fast path) to avoid the sub-pixel offset. Requires P/Invoke or DXGI query to detect Intel GPU vendor ID (0x8086).",
          "category": "fix",
          "confidence": 0.55,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Switch surface origin to TopLeft for Windows",
          "description": "Try GRSurfaceOrigin.TopLeft in SKSwapChainPanel on Windows to see if the coordinate flip eliminates the off-by-one. D3D convention is top-left; the BottomLeft convention may introduce a 1-pixel shift in the ANGLE translation on some drivers.",
          "category": "investigation",
          "confidence": 0.45,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Round DrawImage coordinates to nearest even pixel",
          "description": "Align DrawImage source and destination rectangle coordinates to 2-pixel boundaries to avoid bilinear sampling across atlas sprite borders. This is a user-space recommendation, not a framework change.",
          "category": "workaround",
          "confidence": 0.7,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Round DrawImage coordinates to nearest even pixel",
      "recommendedReason": "This user-space workaround directly addresses the bilinear bleeding root cause (misaligned atlas boundaries) without requiring driver-specific hacks, and can be applied immediately while the underlying ANGLE issue is investigated."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Real bug with visual evidence (video + screenshot) and a partial user-identified workaround. Root cause points to ANGLE/Intel GPU interaction in the SwapChainPanel rendering path; needs a minimal repro and targeted investigation of EGL fast-path and surface origin settings.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "Minimal reproduction project (not the full game) to isolate the issue to SKGLView DrawImage",
      "Confirmation of which SkiaSharp 3.x preview version first showed the problem",
      "Whether the issue appears in SkiaSharp 2.x or only in 3.x (to identify if it's a regression)"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply area, platform and backend labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-WinUI",
          "backend/OpenGL",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the visual evidence, explain the suspected root cause, provide workaround, and request a minimal repro",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thank you for the video, screenshot, and the repro project link — very helpful!\n\nBased on the evidence (GPU-specific behavior on Intel vs NVIDIA, SKGLView-only, zoom-level dependency), this looks like a texture sampling coordinate issue in the **ANGLE/Direct3D 11** rendering path used by `SKGLView` on Windows (`SKSwapChainPanel`). The behavior is consistent with bilinear texture filtering bleeding across sprite atlas boundaries when coordinates land on sub-pixel values — something that manifests differently depending on the GPU driver's implementation of the OpenGL-to-D3D coordinate transform.\n\n**Workaround (while we investigate):**\nAs you found, you can compensate by adjusting the source and destination rectangle coordinates in `DrawImage` calls:\n```csharp\n// Align atlas sprite coordinates to even pixels to avoid bilinear bleed\nvar srcRect = new SKRect(sprite.Left + 1f, sprite.Top, sprite.Right, sprite.Bottom);\nvar dstRect = new SKRect(dest.Left, dest.Top, dest.Right, dest.Bottom + 1f);\ncanvas.DrawImage(image, srcRect, dstRect, paint);\n```\nAlternatively, aligning all sprite coordinates to 2-pixel boundaries avoids the issue entirely.\n\n**To help us narrow this down further**, could you provide:\n1. A minimal standalone SkiaSharp project (not the full game) that reliably triggers the line?\n2. Which SkiaSharp 3.x preview version first showed the problem? Did it also occur on 2.x?\n\nWe will investigate whether the `EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE` setting or the `GRSurfaceOrigin.BottomLeft` flip in `SKSwapChainPanel` is the root cause."
      }
    ]
  }
}
```

</details>
