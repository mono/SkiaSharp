# Issue Triage Report — #902

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T06:41:07Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views (0.85 (85%)) |
| Suggested action | needs-investigation (0.75 (75%)) |

**Issue Summary:** On UWP (SkiaSharp 1.68.0), clearing the canvas with SKColors.White and then calling SKSurface.Snapshot() returns an image where all pixels are #00000000, while the same code works correctly on Android.

**Analysis:** On UWP in SkiaSharp 1.68.0, the SKCanvasView likely rendered via ANGLE (OpenGL ES via Direct3D), meaning the surface passed to PaintSurface was GPU-backed. Calling Snapshot() on a GPU surface without first flushing/submitting the GPU work returns an uninitialized (all-zero) image because the draw calls have not been committed to the GPU framebuffer. On Android the same code path works because the GPU flush timing is different. In the current codebase, SKXamlCanvas creates a raster (CPU) surface backed by WriteableBitmap pixels, which should make Snapshot() work correctly, but SKSwapChainPanel (GL view) may still exhibit the same issue if Snapshot() is called before canvas.Flush().

**Recommendations:** **needs-investigation** — The issue has a complete repro and clear root cause hypothesis (GPU surface not flushed before Snapshot), but was filed against SkiaSharp 1.68.0. The current codebase has changed significantly; whether the bug persists in current SKSwapChainPanel needs verification. The issue also has a confirmed workaround (flush before snapshot, or use raster canvas).

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Universal-UWP |
| Backends | backend/OpenGL |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create a UWP app with SKCanvasView or SKGLView using SkiaSharp 1.68.0
2. In PaintSurface handler, call canvas.Clear(SKColors.White)
3. Call surface.Snapshot() and read pixels
4. Observe all pixels are #00000000 on UWP, but correct on Android

**Environment:** SkiaSharp 1.68.0, Windows 10.0.18362, Android 9.0 (Samsung Galaxy S8)

**Repository links:**
- https://github.com/daltonks/SkiaSharp.UwpSnapshotBug — Minimal UWP + Android repro project by original reporter

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | SKSurface.Snapshot() returns all #00000000 pixels on UWP; works on Android |
| Repro quality | complete |
| Target frameworks | uap10.0.16299 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | Issue was filed against 1.68.0 in 2019. The current SKXamlCanvas uses a raster surface (WriteableBitmap-backed), which should make Snapshot() work correctly. The old 1.68 UWP implementation may have used an ANGLE GPU surface, causing the readback issue. Relevance in current code is uncertain without testing. |

## Analysis

### Technical Summary

On UWP in SkiaSharp 1.68.0, the SKCanvasView likely rendered via ANGLE (OpenGL ES via Direct3D), meaning the surface passed to PaintSurface was GPU-backed. Calling Snapshot() on a GPU surface without first flushing/submitting the GPU work returns an uninitialized (all-zero) image because the draw calls have not been committed to the GPU framebuffer. On Android the same code path works because the GPU flush timing is different. In the current codebase, SKXamlCanvas creates a raster (CPU) surface backed by WriteableBitmap pixels, which should make Snapshot() work correctly, but SKSwapChainPanel (GL view) may still exhibit the same issue if Snapshot() is called before canvas.Flush().

### Rationale

The bug is classified as type/bug with medium severity because clear wrong-output behavior is described with a complete repro, but a workaround exists (use SKCanvasView instead of SKGLView, or flush before Snapshot). The area is SkiaSharp.Views because the issue is in the view-layer surface setup, not in core SkiaSharp. The platform is UWP; backend is OpenGL (via ANGLE). Comments in the issue confirm the core problem is specific to SKGLView (GPU surface), not SKCanvasView (raster surface). The current codebase has changed significantly since 1.68 — SKXamlCanvas now uses a raster surface — so the specific form of the bug may be partially resolved. However, SKSwapChainPanel (the GL view) may still be affected if users call Snapshot() without flushing first.

### Key Signals

- "Android writes `Has non-zero color: True`, but UWP writes `Has non-zero color: False`" — **issue body** (Platform-divergent behavior: drawing and snapshot work correctly on Android but return all-zero pixels on UWP. Points to a surface-type or flush-timing difference.)
- "SKSurface.Snapshot() is returning a null image on iOS using this same method" — **comment #508581736** (Reporter observes same symptom (null/empty image) on iOS when using SKGLView, strengthening the hypothesis that GPU surfaces (ANGLE/OpenGL) are affected.)
- "I also noticed that the SKSurface.Snapshot() returns null only when using SKGLView" — **comment #511726708** (Direct confirmation that the bug is specific to SKGLView (GPU/ANGLE surface), not SKCanvasView (raster surface).)
- "I guess in case no dedicated GPU is available SKGLView can't be used" — **comment #511787306** (Contributor hints at GPU availability as a factor, which aligns with ANGLE-based surfaces failing without hardware acceleration.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 195-206 | direct | DoInvalidate() creates a raster surface via SKSurface.Create(info, pixels, info.RowBytes) backed by the WriteableBitmap pixel buffer, then invokes OnPaintSurface. Snapshot() on this surface reads from CPU memory and should work correctly in the current code. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKSwapChainPanel.cs` | 41-98 | direct | OnRenderFrame() creates an ANGLE OpenGL surface (GRSurfaceOrigin.BottomLeft) via SKSurface.Create(context, renderTarget, surfaceOrigin, colorType). After PaintSurface, canvas.Flush() and context.Flush() are called, but no glFinish() or synchronous GPU readback is issued before the frame ends. Calling Snapshot() inside the PaintSurface callback (before canvas.Flush()) would capture an uninitialized GPU surface. |
| `binding/SkiaSharp/SKSurface.cs` | 50-73 | context | SKSurface.Create(SKImageInfo, IntPtr, int) creates a raster-direct surface writing to a caller-supplied pixel buffer. Snapshot() on such surfaces reads from the CPU buffer synchronously — no GPU readback required. |

### Workarounds

- Call canvas.Flush() before surface.Snapshot() to ensure GPU draw calls are committed: `canvas.Flush(); var image = surface.Snapshot();`
- Use SKCanvasView (raster surface) instead of SKGLView for scenarios that require Snapshot(); raster surfaces have synchronous pixel access
- Instead of Snapshot(), use surface.ReadPixels() into a pre-allocated SKBitmap which forces a synchronous GPU readback

### Next Questions

- Is this still reproducible with current SkiaSharp (2.88.x or 3.x) using SKSwapChainPanel on UWP/WinUI?
- Does calling canvas.Flush() before surface.Snapshot() fix the issue on current code?
- Does the issue affect WinUI (WINDOWS) as well as UWP?

### Resolution Proposals

**Hypothesis:** The bug originates from calling Snapshot() on a GPU-backed ANGLE surface before the pending draw commands have been flushed to the GPU. The resulting SKImage references a GPU texture that has not received the Clear() command yet, so all pixels appear as zero.

1. **Flush canvas before Snapshot()** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - In the user's PaintSurface handler, call canvas.Flush() before surface.Snapshot() to ensure all GPU draw commands are submitted before the pixel readback.

```csharp
void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
{
    e.Surface.Canvas.Clear(SKColors.White);
    e.Surface.Canvas.Flush(); // flush pending GPU commands before snapshot
    using var image = e.Surface.Snapshot();
    // now image has correct pixels
}
```
2. **Use SKCanvasView (raster) instead of SKGLView** — alternative, confidence 0.90 (90%), cost/s, validated=untested
   - Switch from SKGLView/SKSwapChainPanel to SKCanvasView/SKXamlCanvas. The raster canvas writes directly to CPU memory, making Snapshot() synchronous and reliable.

**Recommended proposal:** Flush canvas before Snapshot()

**Why:** Minimal change; avoids losing GPU rendering performance. Flushing before snapshot is the standard pattern for GPU-backed Skia surfaces.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.75 (75%) |
| Reason | The issue has a complete repro and clear root cause hypothesis (GPU surface not flushed before Snapshot), but was filed against SkiaSharp 1.68.0. The current codebase has changed significantly; whether the bug persists in current SKSwapChainPanel needs verification. The issue also has a confirmed workaround (flush before snapshot, or use raster canvas). |
| Suggested repro platform | windows |

### Missing Info

- What SkiaSharp control was used: SKCanvasView or SKGLView (SKSwapChainPanel)?
- Is this still reproducible with current SkiaSharp (2.88.x or 3.x) on WinUI/UWP?

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/SkiaSharp.Views, os/Windows-Universal-UWP, backend/OpenGL, tenet/reliability labels | labels=type/bug, area/SkiaSharp.Views, os/Windows-Universal-UWP, backend/OpenGL, tenet/reliability |
| add-comment | medium | 0.75 (75%) | Post analysis with root cause hypothesis and workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed repro!

After reviewing the code, the root cause appears to be that `SKSurface.Snapshot()` is called on a **GPU-backed surface** (ANGLE/OpenGL via `SKGLView` / `SKSwapChainPanel`) before the pending draw commands have been flushed to the GPU. The resulting `SKImage` references a GPU texture that hasn't received the `Clear()` command yet, so all pixels appear as `#00000000`.

This is also confirmed by the follow-up comments: the same issue appears on iOS and macOS when using `SKGLView` but *not* with `SKCanvasView` (which uses a CPU/raster surface).

**Workarounds:**

1. **Flush before Snapshot** — call `canvas.Flush()` before `surface.Snapshot()` to commit GPU commands:
   ```csharp
   e.Surface.Canvas.Clear(SKColors.White);
   e.Surface.Canvas.Flush(); // ensure GPU commands are committed
   using var image = e.Surface.Snapshot();
   ```

2. **Use `SKCanvasView` instead of `SKGLView`** — the raster canvas writes directly to CPU memory and `Snapshot()` is always synchronous.

Could you confirm which control (`SKCanvasView` or `SKGLView`) was used in the repro, and whether you can test with current SkiaSharp (2.88.x or 3.x)?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 902,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T06:41:07Z"
  },
  "summary": "On UWP (SkiaSharp 1.68.0), clearing the canvas with SKColors.White and then calling SKSurface.Snapshot() returns an image where all pixels are #00000000, while the same code works correctly on Android.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.85
    },
    "platforms": [
      "os/Windows-Universal-UWP"
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
      "errorMessage": "SKSurface.Snapshot() returns all #00000000 pixels on UWP; works on Android",
      "reproQuality": "complete",
      "targetFrameworks": [
        "uap10.0.16299"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a UWP app with SKCanvasView or SKGLView using SkiaSharp 1.68.0",
        "In PaintSurface handler, call canvas.Clear(SKColors.White)",
        "Call surface.Snapshot() and read pixels",
        "Observe all pixels are #00000000 on UWP, but correct on Android"
      ],
      "environmentDetails": "SkiaSharp 1.68.0, Windows 10.0.18362, Android 9.0 (Samsung Galaxy S8)",
      "repoLinks": [
        {
          "url": "https://github.com/daltonks/SkiaSharp.UwpSnapshotBug",
          "description": "Minimal UWP + Android repro project by original reporter"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.0"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "Issue was filed against 1.68.0 in 2019. The current SKXamlCanvas uses a raster surface (WriteableBitmap-backed), which should make Snapshot() work correctly. The old 1.68 UWP implementation may have used an ANGLE GPU surface, causing the readback issue. Relevance in current code is uncertain without testing."
    }
  },
  "analysis": {
    "summary": "On UWP in SkiaSharp 1.68.0, the SKCanvasView likely rendered via ANGLE (OpenGL ES via Direct3D), meaning the surface passed to PaintSurface was GPU-backed. Calling Snapshot() on a GPU surface without first flushing/submitting the GPU work returns an uninitialized (all-zero) image because the draw calls have not been committed to the GPU framebuffer. On Android the same code path works because the GPU flush timing is different. In the current codebase, SKXamlCanvas creates a raster (CPU) surface backed by WriteableBitmap pixels, which should make Snapshot() work correctly, but SKSwapChainPanel (GL view) may still exhibit the same issue if Snapshot() is called before canvas.Flush().",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "195-206",
        "finding": "DoInvalidate() creates a raster surface via SKSurface.Create(info, pixels, info.RowBytes) backed by the WriteableBitmap pixel buffer, then invokes OnPaintSurface. Snapshot() on this surface reads from CPU memory and should work correctly in the current code.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKSwapChainPanel.cs",
        "lines": "41-98",
        "finding": "OnRenderFrame() creates an ANGLE OpenGL surface (GRSurfaceOrigin.BottomLeft) via SKSurface.Create(context, renderTarget, surfaceOrigin, colorType). After PaintSurface, canvas.Flush() and context.Flush() are called, but no glFinish() or synchronous GPU readback is issued before the frame ends. Calling Snapshot() inside the PaintSurface callback (before canvas.Flush()) would capture an uninitialized GPU surface.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKSurface.cs",
        "lines": "50-73",
        "finding": "SKSurface.Create(SKImageInfo, IntPtr, int) creates a raster-direct surface writing to a caller-supplied pixel buffer. Snapshot() on such surfaces reads from the CPU buffer synchronously — no GPU readback required.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "Android writes `Has non-zero color: True`, but UWP writes `Has non-zero color: False`",
        "source": "issue body",
        "interpretation": "Platform-divergent behavior: drawing and snapshot work correctly on Android but return all-zero pixels on UWP. Points to a surface-type or flush-timing difference."
      },
      {
        "text": "SKSurface.Snapshot() is returning a null image on iOS using this same method",
        "source": "comment #508581736",
        "interpretation": "Reporter observes same symptom (null/empty image) on iOS when using SKGLView, strengthening the hypothesis that GPU surfaces (ANGLE/OpenGL) are affected."
      },
      {
        "text": "I also noticed that the SKSurface.Snapshot() returns null only when using SKGLView",
        "source": "comment #511726708",
        "interpretation": "Direct confirmation that the bug is specific to SKGLView (GPU/ANGLE surface), not SKCanvasView (raster surface)."
      },
      {
        "text": "I guess in case no dedicated GPU is available SKGLView can't be used",
        "source": "comment #511787306",
        "interpretation": "Contributor hints at GPU availability as a factor, which aligns with ANGLE-based surfaces failing without hardware acceleration."
      }
    ],
    "rationale": "The bug is classified as type/bug with medium severity because clear wrong-output behavior is described with a complete repro, but a workaround exists (use SKCanvasView instead of SKGLView, or flush before Snapshot). The area is SkiaSharp.Views because the issue is in the view-layer surface setup, not in core SkiaSharp. The platform is UWP; backend is OpenGL (via ANGLE). Comments in the issue confirm the core problem is specific to SKGLView (GPU surface), not SKCanvasView (raster surface). The current codebase has changed significantly since 1.68 — SKXamlCanvas now uses a raster surface — so the specific form of the bug may be partially resolved. However, SKSwapChainPanel (the GL view) may still be affected if users call Snapshot() without flushing first.",
    "nextQuestions": [
      "Is this still reproducible with current SkiaSharp (2.88.x or 3.x) using SKSwapChainPanel on UWP/WinUI?",
      "Does calling canvas.Flush() before surface.Snapshot() fix the issue on current code?",
      "Does the issue affect WinUI (WINDOWS) as well as UWP?"
    ],
    "workarounds": [
      "Call canvas.Flush() before surface.Snapshot() to ensure GPU draw calls are committed: `canvas.Flush(); var image = surface.Snapshot();`",
      "Use SKCanvasView (raster surface) instead of SKGLView for scenarios that require Snapshot(); raster surfaces have synchronous pixel access",
      "Instead of Snapshot(), use surface.ReadPixels() into a pre-allocated SKBitmap which forces a synchronous GPU readback"
    ],
    "resolution": {
      "hypothesis": "The bug originates from calling Snapshot() on a GPU-backed ANGLE surface before the pending draw commands have been flushed to the GPU. The resulting SKImage references a GPU texture that has not received the Clear() command yet, so all pixels appear as zero.",
      "proposals": [
        {
          "title": "Flush canvas before Snapshot()",
          "description": "In the user's PaintSurface handler, call canvas.Flush() before surface.Snapshot() to ensure all GPU draw commands are submitted before the pixel readback.",
          "category": "workaround",
          "codeSnippet": "void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)\n{\n    e.Surface.Canvas.Clear(SKColors.White);\n    e.Surface.Canvas.Flush(); // flush pending GPU commands before snapshot\n    using var image = e.Surface.Snapshot();\n    // now image has correct pixels\n}",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Use SKCanvasView (raster) instead of SKGLView",
          "description": "Switch from SKGLView/SKSwapChainPanel to SKCanvasView/SKXamlCanvas. The raster canvas writes directly to CPU memory, making Snapshot() synchronous and reliable.",
          "category": "alternative",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Flush canvas before Snapshot()",
      "recommendedReason": "Minimal change; avoids losing GPU rendering performance. Flushing before snapshot is the standard pattern for GPU-backed Skia surfaces."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.75,
      "reason": "The issue has a complete repro and clear root cause hypothesis (GPU surface not flushed before Snapshot), but was filed against SkiaSharp 1.68.0. The current codebase has changed significantly; whether the bug persists in current SKSwapChainPanel needs verification. The issue also has a confirmed workaround (flush before snapshot, or use raster canvas).",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "What SkiaSharp control was used: SKCanvasView or SKGLView (SKSwapChainPanel)?",
      "Is this still reproducible with current SkiaSharp (2.88.x or 3.x) on WinUI/UWP?"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views, os/Windows-Universal-UWP, backend/OpenGL, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-Universal-UWP",
          "backend/OpenGL",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis with root cause hypothesis and workaround",
        "risk": "medium",
        "confidence": 0.75,
        "comment": "Thanks for the detailed repro!\n\nAfter reviewing the code, the root cause appears to be that `SKSurface.Snapshot()` is called on a **GPU-backed surface** (ANGLE/OpenGL via `SKGLView` / `SKSwapChainPanel`) before the pending draw commands have been flushed to the GPU. The resulting `SKImage` references a GPU texture that hasn't received the `Clear()` command yet, so all pixels appear as `#00000000`.\n\nThis is also confirmed by the follow-up comments: the same issue appears on iOS and macOS when using `SKGLView` but *not* with `SKCanvasView` (which uses a CPU/raster surface).\n\n**Workarounds:**\n\n1. **Flush before Snapshot** — call `canvas.Flush()` before `surface.Snapshot()` to commit GPU commands:\n   ```csharp\n   e.Surface.Canvas.Clear(SKColors.White);\n   e.Surface.Canvas.Flush(); // ensure GPU commands are committed\n   using var image = e.Surface.Snapshot();\n   ```\n\n2. **Use `SKCanvasView` instead of `SKGLView`** — the raster canvas writes directly to CPU memory and `Snapshot()` is always synchronous.\n\nCould you confirm which control (`SKCanvasView` or `SKGLView`) was used in the repro, and whether you can test with current SkiaSharp (2.88.x or 3.x)?"
      }
    ]
  }
}
```

</details>
