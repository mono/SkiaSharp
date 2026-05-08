# Issue Triage Report — #3429

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T02:34:23Z |
| Type | type/bug (0.82 (82%)) |
| Area | area/SkiaSharp.Views (0.75 (75%)) |
| Suggested action | needs-info (0.88 (88%)) |

**Issue Summary:** Reporter observes a memory leak in a WPF application when drawing patterns on a high-resolution monitor, but only when using an Nvidia Quadro GPU; the issue is claimed as a regression from 2.88.9 to 3.116.0 and is resolved by forcing software-only WPF rendering.

**Analysis:** Memory leak reported in WPF when drawing patterns on high-DPI monitors with Nvidia Quadro GPUs. The leak is resolved by forcing WPF to software-only rendering mode (`RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly`), which disables WPF's DirectX compositing pipeline. SKElement renders Skia content to a CPU-side WriteableBitmap that WPF's DirectX compositor then blits to the screen; on Nvidia Quadro at high DPI the compositor may not release GPU-side texture resources correctly. Without a code sample or memory profile, the root cause cannot be pinpointed — the leak could also be in user code (undisposed SKShader/SKPaint objects holding patterns).

**Recommendations:** **needs-info** — The issue describes a memory leak but provides no code sample, no memory profiler output, and does not identify which SkiaSharp WPF control is in use. Without a minimal repro the bug cannot be confirmed or reproduced.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, os/Windows-Classic, tenet/reliability |

## Evidence

### Reproduction

1. Run a WPF application that draws patterns using SkiaSharp on a high-resolution monitor
2. Machine must have an Nvidia Quadro GPU
3. Observe memory growing over time

**Environment:** Windows, SkiaSharp 3.116.0, Visual Studio, Nvidia Quadro GPU, high-resolution monitor

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | memory-leak |
| Error message | — |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | likely |
| Relevance reason | Reporter explicitly names the version they are using (3.116.0) as affected and 2.88.9 as last known good. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.65 (65%) |
| Reason | Reporter explicitly states it worked in 2.88.9 and no longer works in 3.116.0. Version gap is large; without repro code the regression cannot be confirmed. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.0 |

## Analysis

### Technical Summary

Memory leak reported in WPF when drawing patterns on high-DPI monitors with Nvidia Quadro GPUs. The leak is resolved by forcing WPF to software-only rendering mode (`RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly`), which disables WPF's DirectX compositing pipeline. SKElement renders Skia content to a CPU-side WriteableBitmap that WPF's DirectX compositor then blits to the screen; on Nvidia Quadro at high DPI the compositor may not release GPU-side texture resources correctly. Without a code sample or memory profile, the root cause cannot be pinpointed — the leak could also be in user code (undisposed SKShader/SKPaint objects holding patterns).

### Rationale

Classified as type/bug because the reporter describes a memory leak that degrades the application over time. Area is area/SkiaSharp.Views because the WPF view layer is where the GPU interaction occurs. Severity is medium because a workaround exists (SoftwareOnly mode) even though it carries a performance penalty that makes it unsuitable for the reporter. Action is needs-info because no repro code, no memory profile, and no identification of which control (SKElement vs SKGLElement) is used — without these the bug cannot be reproduced or fixed.

### Key Signals

- "memory leak issue was encountered on the high resolution monitor" — **issue body** (DPI scaling increases the WriteableBitmap pixel size, allocating more GPU texture memory per frame on high-DPI displays.)
- "it only occurred in the Nvidia Quadro series" — **issue body** (GPU-driver-specific behavior points to WPF's DirectX compositor or OpenGL driver resource management rather than Skia's CPU rendering path.)
- "We have confirmed that the issue does not occur when we add the code below [RenderMode.SoftwareOnly]" — **issue body** (The fix disables WPF's hardware acceleration entirely, bypassing DirectX compositing. This is the strongest signal that the leak is in WPF's GPU compositing layer interacting with the Quadro driver, not in Skia itself.)
- "Last Known Good Version of SkiaSharp: 2.88.9" — **issue body** (Claimed regression; the large version gap (2.88.9 → 3.116.0) means many subsystems changed and the root cause cannot be isolated without a minimal repro.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs` | 45-88 | direct | OnRender creates a WriteableBitmap scaled by DPI (scaleX, scaleY) and creates an SKSurface over its BackBuffer (properly disposed via 'using'). The final DrawImage call hands the bitmap to WPF's DrawingContext for GPU compositing. At high DPI the WriteableBitmap is proportionally larger; WPF's DirectX compositor may keep GPU-side copies of the texture that are not released promptly on Nvidia Quadro drivers. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKGLElement.cs` | 127-181 | related | SKGLElement uses OpenGL (via OpenTK) and manages a GRContext + GRBackendRenderTarget + SKSurface lifecycle. Release() disposes all of them. If the reporter uses SKGLElement rather than SKElement, the GRContext and render target management is a more direct GPU leak candidate. |
| `binding/SkiaSharp/SKShader.cs` | 1-50 | related | SKShader.CreateBitmap and CreateImage are the standard SkiaSharp APIs for drawing tiled patterns. SKShader implements ISKReferenceCounted and must be disposed. If the reporter creates pattern shaders per frame without disposing them, native Skia objects accumulate. |

### Workarounds

- Set `RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly` in `OnStartup` to disable WPF hardware acceleration (reported to work by reporter, but has a performance cost).
- If using `SKGLElement`, switch to `SKElement` to use the CPU-side WriteableBitmap path and avoid OpenGL entirely.
- Ensure all `SKShader`, `SKPaint`, and `SKBitmap` objects used for patterns are properly disposed (e.g., with `using`) to prevent unmanaged object accumulation.

### Next Questions

- Is the application using SKElement (CPU/WriteableBitmap) or SKGLElement (OpenGL)?
- What does 'drawing patterns' mean in the reporter's code — are they using SKShader.CreateBitmap/CreateImage, or something else?
- Can the reporter provide a minimal C# repro project or code snippet?
- What is the Windows version and display DPI scaling percentage?
- Does the leak occur with a plain SKElement paint handler that draws a solid rectangle (no patterns), or only when using patterned shaders?

### Resolution Proposals

**Hypothesis:** The memory leak is most likely caused by WPF's DirectX compositor allocating GPU textures for each WriteableBitmap update and not releasing them promptly on Nvidia Quadro drivers at high DPI. A secondary possibility is that the reporter's pattern-drawing code does not dispose SKShader/SKPaint objects per frame.

1. **Ask reporter for minimal repro + memory profile** — investigation, confidence 0.90 (90%), cost/xs, validated=untested
   - Request a minimal C# WPF project that reproduces the leak, along with which control is used (SKElement vs SKGLElement), Windows version, DPI scaling, and a memory profiler snapshot showing what is growing.
2. **Dispose pattern shaders per frame** — workaround, confidence 0.70 (70%), cost/xs, validated=untested
   - If the reporter creates SKShader or SKPaint objects on every OnPaintSurface call, wrapping them in 'using' blocks ensures native objects are released immediately instead of waiting for the GC.

```csharp
private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
{
    var canvas = e.Surface.Canvas;
    canvas.Clear(SKColors.White);

    // Dispose shader and paint after each use
    using var patternBitmap = CreatePatternBitmap();
    using var shader = SKShader.CreateBitmap(patternBitmap, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
    using var paint = new SKPaint { Shader = shader };
    canvas.DrawRect(e.Info.Rect, paint);
}
```

**Recommended proposal:** Ask reporter for minimal repro + memory profile

**Why:** Root cause cannot be determined from the current issue description. A minimal repro is required before any fix can be implemented.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.88 (88%) |
| Reason | The issue describes a memory leak but provides no code sample, no memory profiler output, and does not identify which SkiaSharp WPF control is in use. Without a minimal repro the bug cannot be confirmed or reproduced. |
| Suggested repro platform | windows |

### Missing Info

- Which SkiaSharp WPF control is used: SKElement or SKGLElement?
- Code snippet or minimal project showing how patterns are drawn
- Windows version and DPI scaling percentage
- Memory profiler output or task manager screenshot showing the growth
- Does the leak occur without patterns (e.g., drawing a solid fill)?

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, WPF area, Windows-Classic, reliability labels | labels=type/bug, area/SkiaSharp.Views, os/Windows-Classic, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Request minimal repro and clarifying information from reporter | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report and for identifying the workaround!

To help investigate this memory leak, could you provide:

1. **Which control are you using?** `SKElement` (CPU rendering) or `SKGLElement` (OpenGL rendering)?
2. **A minimal code snippet** showing how you draw your patterns — specifically the `PaintSurface` handler and how the `SKShader`/`SKPaint` objects are created.
3. **Windows version** and the display scaling percentage (e.g., 150%, 200%).
4. **A memory profile or Task Manager screenshot** showing how memory grows over time.

Also, as an immediate workaround to try before we identify the root cause:
- Make sure any `SKShader`, `SKPaint`, or `SKBitmap` objects used for patterns are wrapped in `using` blocks so they are disposed immediately rather than waiting for the garbage collector.

This information will help us determine whether the leak is in SkiaSharp's WPF rendering layer or in WPF's DirectX compositor interacting with the Quadro driver.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3429,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T02:34:23Z",
    "currentLabels": [
      "type/bug",
      "os/Windows-Classic",
      "tenet/reliability"
    ]
  },
  "summary": "Reporter observes a memory leak in a WPF application when drawing patterns on a high-resolution monitor, but only when using an Nvidia Quadro GPU; the issue is claimed as a regression from 2.88.9 to 3.116.0 and is resolved by forcing software-only WPF rendering.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.82
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.75
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "memory-leak",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Run a WPF application that draws patterns using SkiaSharp on a high-resolution monitor",
        "Machine must have an Nvidia Quadro GPU",
        "Observe memory growing over time"
      ],
      "environmentDetails": "Windows, SkiaSharp 3.116.0, Visual Studio, Nvidia Quadro GPU, high-resolution monitor"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "2.88.9"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.116.0",
      "currentRelevance": "likely",
      "relevanceReason": "Reporter explicitly names the version they are using (3.116.0) as affected and 2.88.9 as last known good."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.65,
      "reason": "Reporter explicitly states it worked in 2.88.9 and no longer works in 3.116.0. Version gap is large; without repro code the regression cannot be confirmed.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.0"
    }
  },
  "analysis": {
    "summary": "Memory leak reported in WPF when drawing patterns on high-DPI monitors with Nvidia Quadro GPUs. The leak is resolved by forcing WPF to software-only rendering mode (`RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly`), which disables WPF's DirectX compositing pipeline. SKElement renders Skia content to a CPU-side WriteableBitmap that WPF's DirectX compositor then blits to the screen; on Nvidia Quadro at high DPI the compositor may not release GPU-side texture resources correctly. Without a code sample or memory profile, the root cause cannot be pinpointed — the leak could also be in user code (undisposed SKShader/SKPaint objects holding patterns).",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs",
        "lines": "45-88",
        "finding": "OnRender creates a WriteableBitmap scaled by DPI (scaleX, scaleY) and creates an SKSurface over its BackBuffer (properly disposed via 'using'). The final DrawImage call hands the bitmap to WPF's DrawingContext for GPU compositing. At high DPI the WriteableBitmap is proportionally larger; WPF's DirectX compositor may keep GPU-side copies of the texture that are not released promptly on Nvidia Quadro drivers.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKGLElement.cs",
        "lines": "127-181",
        "finding": "SKGLElement uses OpenGL (via OpenTK) and manages a GRContext + GRBackendRenderTarget + SKSurface lifecycle. Release() disposes all of them. If the reporter uses SKGLElement rather than SKElement, the GRContext and render target management is a more direct GPU leak candidate.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKShader.cs",
        "lines": "1-50",
        "finding": "SKShader.CreateBitmap and CreateImage are the standard SkiaSharp APIs for drawing tiled patterns. SKShader implements ISKReferenceCounted and must be disposed. If the reporter creates pattern shaders per frame without disposing them, native Skia objects accumulate.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "memory leak issue was encountered on the high resolution monitor",
        "source": "issue body",
        "interpretation": "DPI scaling increases the WriteableBitmap pixel size, allocating more GPU texture memory per frame on high-DPI displays."
      },
      {
        "text": "it only occurred in the Nvidia Quadro series",
        "source": "issue body",
        "interpretation": "GPU-driver-specific behavior points to WPF's DirectX compositor or OpenGL driver resource management rather than Skia's CPU rendering path."
      },
      {
        "text": "We have confirmed that the issue does not occur when we add the code below [RenderMode.SoftwareOnly]",
        "source": "issue body",
        "interpretation": "The fix disables WPF's hardware acceleration entirely, bypassing DirectX compositing. This is the strongest signal that the leak is in WPF's GPU compositing layer interacting with the Quadro driver, not in Skia itself."
      },
      {
        "text": "Last Known Good Version of SkiaSharp: 2.88.9",
        "source": "issue body",
        "interpretation": "Claimed regression; the large version gap (2.88.9 → 3.116.0) means many subsystems changed and the root cause cannot be isolated without a minimal repro."
      }
    ],
    "workarounds": [
      "Set `RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly` in `OnStartup` to disable WPF hardware acceleration (reported to work by reporter, but has a performance cost).",
      "If using `SKGLElement`, switch to `SKElement` to use the CPU-side WriteableBitmap path and avoid OpenGL entirely.",
      "Ensure all `SKShader`, `SKPaint`, and `SKBitmap` objects used for patterns are properly disposed (e.g., with `using`) to prevent unmanaged object accumulation."
    ],
    "rationale": "Classified as type/bug because the reporter describes a memory leak that degrades the application over time. Area is area/SkiaSharp.Views because the WPF view layer is where the GPU interaction occurs. Severity is medium because a workaround exists (SoftwareOnly mode) even though it carries a performance penalty that makes it unsuitable for the reporter. Action is needs-info because no repro code, no memory profile, and no identification of which control (SKElement vs SKGLElement) is used — without these the bug cannot be reproduced or fixed.",
    "nextQuestions": [
      "Is the application using SKElement (CPU/WriteableBitmap) or SKGLElement (OpenGL)?",
      "What does 'drawing patterns' mean in the reporter's code — are they using SKShader.CreateBitmap/CreateImage, or something else?",
      "Can the reporter provide a minimal C# repro project or code snippet?",
      "What is the Windows version and display DPI scaling percentage?",
      "Does the leak occur with a plain SKElement paint handler that draws a solid rectangle (no patterns), or only when using patterned shaders?"
    ],
    "resolution": {
      "hypothesis": "The memory leak is most likely caused by WPF's DirectX compositor allocating GPU textures for each WriteableBitmap update and not releasing them promptly on Nvidia Quadro drivers at high DPI. A secondary possibility is that the reporter's pattern-drawing code does not dispose SKShader/SKPaint objects per frame.",
      "proposals": [
        {
          "title": "Ask reporter for minimal repro + memory profile",
          "description": "Request a minimal C# WPF project that reproduces the leak, along with which control is used (SKElement vs SKGLElement), Windows version, DPI scaling, and a memory profiler snapshot showing what is growing.",
          "category": "investigation",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Dispose pattern shaders per frame",
          "description": "If the reporter creates SKShader or SKPaint objects on every OnPaintSurface call, wrapping them in 'using' blocks ensures native objects are released immediately instead of waiting for the GC.",
          "category": "workaround",
          "codeSnippet": "private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)\n{\n    var canvas = e.Surface.Canvas;\n    canvas.Clear(SKColors.White);\n\n    // Dispose shader and paint after each use\n    using var patternBitmap = CreatePatternBitmap();\n    using var shader = SKShader.CreateBitmap(patternBitmap, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);\n    using var paint = new SKPaint { Shader = shader };\n    canvas.DrawRect(e.Info.Rect, paint);\n}",
          "confidence": 0.7,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Ask reporter for minimal repro + memory profile",
      "recommendedReason": "Root cause cannot be determined from the current issue description. A minimal repro is required before any fix can be implemented."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.88,
      "reason": "The issue describes a memory leak but provides no code sample, no memory profiler output, and does not identify which SkiaSharp WPF control is in use. Without a minimal repro the bug cannot be confirmed or reproduced.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "Which SkiaSharp WPF control is used: SKElement or SKGLElement?",
      "Code snippet or minimal project showing how patterns are drawn",
      "Windows version and DPI scaling percentage",
      "Memory profiler output or task manager screenshot showing the growth",
      "Does the leak occur without patterns (e.g., drawing a solid fill)?"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, WPF area, Windows-Classic, reliability labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Request minimal repro and clarifying information from reporter",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the report and for identifying the workaround!\n\nTo help investigate this memory leak, could you provide:\n\n1. **Which control are you using?** `SKElement` (CPU rendering) or `SKGLElement` (OpenGL rendering)?\n2. **A minimal code snippet** showing how you draw your patterns — specifically the `PaintSurface` handler and how the `SKShader`/`SKPaint` objects are created.\n3. **Windows version** and the display scaling percentage (e.g., 150%, 200%).\n4. **A memory profile or Task Manager screenshot** showing how memory grows over time.\n\nAlso, as an immediate workaround to try before we identify the root cause:\n- Make sure any `SKShader`, `SKPaint`, or `SKBitmap` objects used for patterns are wrapped in `using` blocks so they are disposed immediately rather than waiting for the garbage collector.\n\nThis information will help us determine whether the leak is in SkiaSharp's WPF rendering layer or in WPF's DirectX compositor interacting with the Quadro driver."
      }
    ]
  }
}
```

</details>
