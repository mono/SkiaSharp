# Issue Triage Report — #1484

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T05:25:00Z |
| Type | type/question (0.80 (80%)) |
| Area | area/SkiaSharp (0.85 (85%)) |
| Suggested action | close-as-not-a-bug (0.72 (72%)) |

**Issue Summary:** Reporter observes GPU memory growth when drawing a SKPicture containing CubicTo path segments on a UWP SKSwapChainPanel after canvas matrix translations, and asks whether this is expected behavior.

**Analysis:** GPU memory grows when drawing a SKPicture with complex path segments (CubicTo) on a GPU surface after canvas matrix changes. This is Skia's expected tessellation caching behavior: each unique CTM may create new GPU cache entries for complex path tessellations. The GRContext resource cache bounds total growth (default ~96MB), but needs periodic cleanup via PurgeUnlockedResources() to evict old entries. SKSwapChainPanel does not call PurgeUnlockedResources() automatically. Workarounds exist.

**Recommendations:** **close-as-not-a-bug** — GPU memory growth is expected Skia tessellation cache behavior; the GRContext resource cache is bounded and workarounds exist via PurgeUnlockedResources and SetResourceCacheLimit. The reporter is asking for explanation, not reporting a regression.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Universal-UWP |
| Backends | backend/OpenGL |
| Tenets | tenet/performance, tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create a UWP app with SKSwapChainPanel
2. In PaintSurface handler, set canvas matrix to a translation via surfaceCanvas.SetMatrix(_canvasMatrix)
3. Draw a SKPicture containing a CubicTo path segment
4. Change the translation matrix and redraw
5. Observe increasing GPU memory usage after each translation change

**Environment:** UWP app, SKSwapChainPanel, ANGLE/OpenGL GPU backend, Windows

**Repository links:**
- https://github.com/validvoid/Skiasharp_Cubic_Bezier_issue — Sample UWP app demonstrating GPU memory growth with SKPicture and canvas translations

## Analysis

### Technical Summary

GPU memory grows when drawing a SKPicture with complex path segments (CubicTo) on a GPU surface after canvas matrix changes. This is Skia's expected tessellation caching behavior: each unique CTM may create new GPU cache entries for complex path tessellations. The GRContext resource cache bounds total growth (default ~96MB), but needs periodic cleanup via PurgeUnlockedResources() to evict old entries. SKSwapChainPanel does not call PurgeUnlockedResources() automatically. Workarounds exist.

### Rationale

Reporter classifies this as a question and asks why the behavior occurs, suggesting they want explanation rather than a bug fix. The behavior is consistent with Skia's GPU path tessellation caching (expected Skia behavior), not a SkiaSharp defect. The GRContext exposes purge methods to manage this, and SKSwapChainPanel does not call them automatically. Maintainer comment confirms uncertainty, seeking expert input rather than dismissing or confirming the issue.

### Key Signals

- "the operation of drawing a SKPicture of a path which contains multiple segments(>=3, CubicTo or multiple LineTo) consumes more GPU memory" — **issue body** (Complex paths (CubicTo) trigger Skia's GPU path renderer which tessellates path geometry and caches it in GRContext resource cache.)
- "every time I translate the canvas, the operation ... consumes more GPU memory" — **issue body** (Changing canvas CTM may create new tessellation cache entries for the same path, leading to accumulation until cache limit is reached.)
- "@danwalmsley would you know how to debug this? Were you able to make use of the new SKGraphics type and other bits to catch things like this?" — **comment by mattleibow** (Maintainer is seeking expert input, suggesting this needs further investigation and is not a trivially known/dismissed behavior.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/GRContext.cs` | 159-169 | direct | GRContext exposes PurgeResources(), PurgeUnlockedResources(bool scratchResourcesOnly), PurgeUnlockedResources(long bytesToPurge, bool preferScratchResources), and PurgeUnusedResources(long milliseconds) for controlling GPU resource cache lifecycle. These can be called between frames to evict stale tessellation cache entries. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKSwapChainPanel.cs` | 41-98 | direct | SKSwapChainPanel calls canvas.Flush() and context.Flush() after each frame (lines 96-97) but does NOT call PurgeUnlockedResources(). The GRContext is exposed via the GRContext property (line 32) for user-side cache management. |
| `binding/SkiaSharp/GRContext.cs` | 116-128 | related | GRContext.GetResourceCacheUsage(out int maxResources, out long maxResourceBytes) and SetResourceCacheLimit(long maxResourceBytes) allow inspecting and bounding GPU cache memory; available but require explicit opt-in from user code. |
| `binding/SkiaSharp/SKGraphics.cs` | 17-58 | context | SKGraphics.PurgeResourceCache() and GetResourceCacheTotalBytesUsed() manage CPU-side resource caches (font, raster). mattleibow mentioned SKGraphics as a debugging tool in comments, but GPU tessellation cache is managed via GRContext, not SKGraphics. |

### Workarounds

- Call GRContext.PurgeUnlockedResources(scratchResourcesOnly: false) at the start of each frame (or every N frames) via SKSwapChainPanel.GRContext to evict unused GPU tessellation cache entries from previous frames
- Lower the GRContext resource cache limit using GRContext.SetResourceCacheLimit(maxBytes) to prevent accumulation beyond a desired threshold
- Monitor GPU cache usage with GRContext.GetResourceCacheUsage(out int maxResources, out long maxResourceBytes) to understand the growth pattern

### Resolution Proposals

**Hypothesis:** Skia's GPU path renderer caches tessellated path geometry per unique rendering parameters including canvas CTM. When drawing complex paths (CubicTo, multiple LineTo) on a GPU surface (GRContext), changing the canvas matrix creates new tessellation cache entries. The SKSwapChainPanel does not call PurgeUnlockedResources() automatically, causing accumulation up to the GRContext resource cache limit (~96MB default).

1. **Periodically purge unlocked GPU resources** — workaround, confidence 0.82 (82%), cost/xs, validated=yes
   - At the start of your PaintSurface handler (before drawing), periodically call PurgeUnlockedResources() on the GRContext. Calling it at frame start ensures previous-frame resources are already unlocked and can be evicted.

```csharp
private int _frameCount = 0;

private void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
{
    // Purge unlocked GPU resources every 60 frames to control cache growth.
    // Call at frame start so previous frame's resources are already unlocked.
    var panel = sender as SKSwapChainPanel;
    if (panel?.GRContext != null && _frameCount++ % 60 == 0)
        panel.GRContext.PurgeUnlockedResources(scratchResourcesOnly: false);

    // ... your existing drawing code ...
}
```
2. **Set a lower resource cache limit** — workaround, confidence 0.75 (75%), cost/xs, validated=yes
   - After the GRContext is created, set a lower resource cache limit to bound GPU memory usage. The default is ~96MB.

```csharp
var grContext = skSwapChainPanel.GRContext;
if (grContext != null)
    grContext.SetResourceCacheLimit(32 * 1024 * 1024); // 32MB limit
```

**Recommended proposal:** Periodically purge unlocked GPU resources

**Why:** Directly addresses accumulated stale tessellation cache entries by evicting them at frame boundaries without affecting render quality or requiring architecture changes.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.72 (72%) |
| Reason | GPU memory growth is expected Skia tessellation cache behavior; the GRContext resource cache is bounded and workarounds exist via PurgeUnlockedResources and SetResourceCacheLimit. The reporter is asking for explanation, not reporting a regression. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply question, SkiaSharp, UWP, and OpenGL labels | labels=type/question, area/SkiaSharp, os/Windows-Universal-UWP, backend/OpenGL |
| add-comment | medium | 0.72 (72%) | Explain Skia GPU tessellation cache behavior and provide validated workarounds | — |
| close-issue | medium | 0.65 (65%) | Close as answered — GPU memory growth is expected Skia caching behavior with documented workarounds | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and sample repository!

The GPU memory growth you're seeing is related to how Skia's GPU path renderer handles complex paths (`CubicTo`, multiple `LineTo` segments) when rendering to a GPU-backed surface like `SKSwapChainPanel`.

When you draw a `SKPicture` containing complex paths, Skia's GPU backend tessellates the path geometry and stores the result in the `GRContext`'s GPU resource cache. Changing the canvas matrix via `SetMatrix` can cause Skia to create new tessellation cache entries for the path at each unique transform, leading to gradual GPU memory growth.

The cache is bounded by the `GRContext`'s resource cache limit (default ~96 MB), so growth should eventually plateau. However, you can control this proactively:

**Option 1: Periodically purge unlocked GPU resources**

Add this at the *start* of your `PaintSurface` handler — calling it before drawing ensures previous-frame resources are already unlocked:

```csharp
private int _frameCount = 0;

private void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
{
    var panel = sender as SKSwapChainPanel;
    if (panel?.GRContext != null && _frameCount++ % 60 == 0)
        panel.GRContext.PurgeUnlockedResources(scratchResourcesOnly: false);

    // ... your existing drawing code ...
}
```

**Option 2: Lower the resource cache limit**

```csharp
var grContext = skSwapChainPanel.GRContext;
if (grContext != null)
    grContext.SetResourceCacheLimit(32 * 1024 * 1024); // 32MB
```

**To monitor current GPU cache usage:**

```csharp
grContext.GetResourceCacheUsage(out int maxResources, out long maxResourceBytes);
System.Diagnostics.Debug.WriteLine($"GPU cache: {maxResources} resources, {maxResourceBytes / 1024 / 1024}MB");
```
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1484,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T05:25:00Z"
  },
  "summary": "Reporter observes GPU memory growth when drawing a SKPicture containing CubicTo path segments on a UWP SKSwapChainPanel after canvas matrix translations, and asks whether this is expected behavior.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.8
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.85
    },
    "platforms": [
      "os/Windows-Universal-UWP"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "tenets": [
      "tenet/performance",
      "tenet/reliability"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a UWP app with SKSwapChainPanel",
        "In PaintSurface handler, set canvas matrix to a translation via surfaceCanvas.SetMatrix(_canvasMatrix)",
        "Draw a SKPicture containing a CubicTo path segment",
        "Change the translation matrix and redraw",
        "Observe increasing GPU memory usage after each translation change"
      ],
      "environmentDetails": "UWP app, SKSwapChainPanel, ANGLE/OpenGL GPU backend, Windows",
      "repoLinks": [
        {
          "url": "https://github.com/validvoid/Skiasharp_Cubic_Bezier_issue",
          "description": "Sample UWP app demonstrating GPU memory growth with SKPicture and canvas translations"
        }
      ]
    }
  },
  "analysis": {
    "summary": "GPU memory grows when drawing a SKPicture with complex path segments (CubicTo) on a GPU surface after canvas matrix changes. This is Skia's expected tessellation caching behavior: each unique CTM may create new GPU cache entries for complex path tessellations. The GRContext resource cache bounds total growth (default ~96MB), but needs periodic cleanup via PurgeUnlockedResources() to evict old entries. SKSwapChainPanel does not call PurgeUnlockedResources() automatically. Workarounds exist.",
    "rationale": "Reporter classifies this as a question and asks why the behavior occurs, suggesting they want explanation rather than a bug fix. The behavior is consistent with Skia's GPU path tessellation caching (expected Skia behavior), not a SkiaSharp defect. The GRContext exposes purge methods to manage this, and SKSwapChainPanel does not call them automatically. Maintainer comment confirms uncertainty, seeking expert input rather than dismissing or confirming the issue.",
    "keySignals": [
      {
        "text": "the operation of drawing a SKPicture of a path which contains multiple segments(>=3, CubicTo or multiple LineTo) consumes more GPU memory",
        "source": "issue body",
        "interpretation": "Complex paths (CubicTo) trigger Skia's GPU path renderer which tessellates path geometry and caches it in GRContext resource cache."
      },
      {
        "text": "every time I translate the canvas, the operation ... consumes more GPU memory",
        "source": "issue body",
        "interpretation": "Changing canvas CTM may create new tessellation cache entries for the same path, leading to accumulation until cache limit is reached."
      },
      {
        "text": "@danwalmsley would you know how to debug this? Were you able to make use of the new SKGraphics type and other bits to catch things like this?",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer is seeking expert input, suggesting this needs further investigation and is not a trivially known/dismissed behavior."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/GRContext.cs",
        "lines": "159-169",
        "finding": "GRContext exposes PurgeResources(), PurgeUnlockedResources(bool scratchResourcesOnly), PurgeUnlockedResources(long bytesToPurge, bool preferScratchResources), and PurgeUnusedResources(long milliseconds) for controlling GPU resource cache lifecycle. These can be called between frames to evict stale tessellation cache entries.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKSwapChainPanel.cs",
        "lines": "41-98",
        "finding": "SKSwapChainPanel calls canvas.Flush() and context.Flush() after each frame (lines 96-97) but does NOT call PurgeUnlockedResources(). The GRContext is exposed via the GRContext property (line 32) for user-side cache management.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/GRContext.cs",
        "lines": "116-128",
        "finding": "GRContext.GetResourceCacheUsage(out int maxResources, out long maxResourceBytes) and SetResourceCacheLimit(long maxResourceBytes) allow inspecting and bounding GPU cache memory; available but require explicit opt-in from user code.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKGraphics.cs",
        "lines": "17-58",
        "finding": "SKGraphics.PurgeResourceCache() and GetResourceCacheTotalBytesUsed() manage CPU-side resource caches (font, raster). mattleibow mentioned SKGraphics as a debugging tool in comments, but GPU tessellation cache is managed via GRContext, not SKGraphics.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "Call GRContext.PurgeUnlockedResources(scratchResourcesOnly: false) at the start of each frame (or every N frames) via SKSwapChainPanel.GRContext to evict unused GPU tessellation cache entries from previous frames",
      "Lower the GRContext resource cache limit using GRContext.SetResourceCacheLimit(maxBytes) to prevent accumulation beyond a desired threshold",
      "Monitor GPU cache usage with GRContext.GetResourceCacheUsage(out int maxResources, out long maxResourceBytes) to understand the growth pattern"
    ],
    "resolution": {
      "hypothesis": "Skia's GPU path renderer caches tessellated path geometry per unique rendering parameters including canvas CTM. When drawing complex paths (CubicTo, multiple LineTo) on a GPU surface (GRContext), changing the canvas matrix creates new tessellation cache entries. The SKSwapChainPanel does not call PurgeUnlockedResources() automatically, causing accumulation up to the GRContext resource cache limit (~96MB default).",
      "proposals": [
        {
          "title": "Periodically purge unlocked GPU resources",
          "description": "At the start of your PaintSurface handler (before drawing), periodically call PurgeUnlockedResources() on the GRContext. Calling it at frame start ensures previous-frame resources are already unlocked and can be evicted.",
          "category": "workaround",
          "codeSnippet": "private int _frameCount = 0;\n\nprivate void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)\n{\n    // Purge unlocked GPU resources every 60 frames to control cache growth.\n    // Call at frame start so previous frame's resources are already unlocked.\n    var panel = sender as SKSwapChainPanel;\n    if (panel?.GRContext != null && _frameCount++ % 60 == 0)\n        panel.GRContext.PurgeUnlockedResources(scratchResourcesOnly: false);\n\n    // ... your existing drawing code ...\n}",
          "confidence": 0.82,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Set a lower resource cache limit",
          "description": "After the GRContext is created, set a lower resource cache limit to bound GPU memory usage. The default is ~96MB.",
          "category": "workaround",
          "codeSnippet": "var grContext = skSwapChainPanel.GRContext;\nif (grContext != null)\n    grContext.SetResourceCacheLimit(32 * 1024 * 1024); // 32MB limit",
          "confidence": 0.75,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Periodically purge unlocked GPU resources",
      "recommendedReason": "Directly addresses accumulated stale tessellation cache entries by evicting them at frame boundaries without affecting render quality or requiring architecture changes."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.72,
      "reason": "GPU memory growth is expected Skia tessellation cache behavior; the GRContext resource cache is bounded and workarounds exist via PurgeUnlockedResources and SetResourceCacheLimit. The reporter is asking for explanation, not reporting a regression.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question, SkiaSharp, UWP, and OpenGL labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "os/Windows-Universal-UWP",
          "backend/OpenGL"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain Skia GPU tessellation cache behavior and provide validated workarounds",
        "risk": "medium",
        "confidence": 0.72,
        "comment": "Thanks for the detailed report and sample repository!\n\nThe GPU memory growth you're seeing is related to how Skia's GPU path renderer handles complex paths (`CubicTo`, multiple `LineTo` segments) when rendering to a GPU-backed surface like `SKSwapChainPanel`.\n\nWhen you draw a `SKPicture` containing complex paths, Skia's GPU backend tessellates the path geometry and stores the result in the `GRContext`'s GPU resource cache. Changing the canvas matrix via `SetMatrix` can cause Skia to create new tessellation cache entries for the path at each unique transform, leading to gradual GPU memory growth.\n\nThe cache is bounded by the `GRContext`'s resource cache limit (default ~96 MB), so growth should eventually plateau. However, you can control this proactively:\n\n**Option 1: Periodically purge unlocked GPU resources**\n\nAdd this at the *start* of your `PaintSurface` handler — calling it before drawing ensures previous-frame resources are already unlocked:\n\n```csharp\nprivate int _frameCount = 0;\n\nprivate void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)\n{\n    var panel = sender as SKSwapChainPanel;\n    if (panel?.GRContext != null && _frameCount++ % 60 == 0)\n        panel.GRContext.PurgeUnlockedResources(scratchResourcesOnly: false);\n\n    // ... your existing drawing code ...\n}\n```\n\n**Option 2: Lower the resource cache limit**\n\n```csharp\nvar grContext = skSwapChainPanel.GRContext;\nif (grContext != null)\n    grContext.SetResourceCacheLimit(32 * 1024 * 1024); // 32MB\n```\n\n**To monitor current GPU cache usage:**\n\n```csharp\ngrContext.GetResourceCacheUsage(out int maxResources, out long maxResourceBytes);\nSystem.Diagnostics.Debug.WriteLine($\"GPU cache: {maxResources} resources, {maxResourceBytes / 1024 / 1024}MB\");\n```"
      },
      {
        "type": "close-issue",
        "description": "Close as answered — GPU memory growth is expected Skia caching behavior with documented workarounds",
        "risk": "medium",
        "confidence": 0.65,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
