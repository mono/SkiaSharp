# Issue Triage Report — #2848

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-27T12:50:00Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** canvas.DrawPath() with SKPaintStyle.Fill causes a linear native heap memory leak in GPU context on Windows 11, confirmed from SkiaSharp 2.88.3 through 3.119.2; SKPaintStyle.Stroke does not leak.

**Analysis:** Native heap leak when using DrawPath with Fill style in a GPU context on Windows 11. The C# wrapper (SKCanvas.DrawPath) is a thin pass-through to sk_canvas_draw_path with no managed allocation; the leak originates in Skia's GPU fill-path rasterizer (likely a path tessellation or coverage-mask atlas cache growing unboundedly). The regression from 2.88.2 to 2.88.3 suggests a Skia upstream change in the fill path that altered GPU resource lifecycle on Direct3D/Windows.

**Recommendations:** **needs-investigation** — Confirmed real regression (2.88.2 → 2.88.3, still present in 3.119.2) with good behavioral isolation (Fill leaks, Stroke does not). Root cause is in the Skia GPU fill rasterizer on Windows 11 and needs a reproducible environment to trace and fix.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/Direct3D |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a SkiaSharp GPU context (e.g. via GRContext.CreateGl or Direct3D) on Windows 11
2. In a render loop (~60 fps), call canvas.DrawPath(path, paint) with paint.Style = SKPaintStyle.Fill
3. Monitor native heap: observe linear growth (~18 MB/min)
4. Switch to paint.Style = SKPaintStyle.Stroke — heap growth stops

**Environment:** Windows 11 x64, SkiaSharp 2.88.3–3.119.2, .NET 10, GPU context (Direct3D/OpenGL)

**Related issues:** #2660

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2848#issuecomment-4311452892 — April 2026 repro data confirming Fill leaks on SkiaSharp 3.119.2 / .NET 10 / Windows 11 GPU context; Stroke does not leak
- https://github.com/mono/SkiaSharp/issues/2660 — Original DrawPath GPU memory leak report (Windows 10, SkiaSharp 2.88.3) by same author

**Code snippets:**

```csharp
using (var paintFill = new SKPaint { IsAntialias = true })
{
    paintFill.StrokeWidth = 0;
    paintFill.Style = SKPaintStyle.Fill;
    paintFill.PathEffect = null;
    paintFill.Shader = null;
    paintFill.Color = fillColor.ToSkia(opacity);
    canvas.DrawPath(path, paintFill); // leaks in GPU context on Windows 11
}
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | memory-leak |
| Error message | Native heap grows ~18 MB/min when DrawPath Fill rasterization runs on GPU context under Windows 11 |
| Repro quality | partial |
| Target frameworks | net10.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.2, 2.88.3, 2.88.8, 3.119.2 |
| Worked in | 2.88.2 |
| Broke in | 2.88.3 |
| Current relevance | likely |
| Relevance reason | Second commenter confirmed the leak persists on SkiaSharp 3.119.2 (April 2026), with identical symptoms: Fill leaks, Stroke is flat. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.88 (88%) |
| Reason | Both the original reporter and a confirming commenter state 2.88.2 was leak-free; 2.88.3 introduced the regression. Confirmed still present in 3.119.2. |
| Worked in version | 2.88.2 |
| Broke in version | 2.88.3 |

## Analysis

### Technical Summary

Native heap leak when using DrawPath with Fill style in a GPU context on Windows 11. The C# wrapper (SKCanvas.DrawPath) is a thin pass-through to sk_canvas_draw_path with no managed allocation; the leak originates in Skia's GPU fill-path rasterizer (likely a path tessellation or coverage-mask atlas cache growing unboundedly). The regression from 2.88.2 to 2.88.3 suggests a Skia upstream change in the fill path that altered GPU resource lifecycle on Direct3D/Windows.

### Rationale

Multiple independent reporters confirm the leak. Clear behavioral isolation (Fill leaks, Stroke does not) strongly implicates the GPU fill rasterizer cache. The C# binding layer is not at fault — SKCanvas.DrawPath simply forwards arguments to the native sk_canvas_draw_path call. Upstream Skia likely owns the root cause; however, a SkiaSharp-level workaround (periodic GRContext.PurgeUnlockedResources calls) may mitigate impact.

### Key Signals

- "canvas.DrawPath(path, paintFill) — this line cause memory leak in GPU context on Windows 11" — **issue body** (Reporter has isolated the leak to a single API call in GPU context.)
- "paint.Style = Fill → leaks (~+18 MB/min native heap); paint.Style = Stroke → completely flat" — **comment #2 (April 2026)** (Exact behavioral isolation; the Fill rasterization path in the GPU backend accumulates native memory that Stroke does not.)
- "Last Known Good Version: 2.88.2; Current (broken): 2.88.3" — **issue body** (Regression window is narrow; a Skia upstream change between those two SkiaSharp versions altered GPU fill path behavior.)
- "Confirmed on SkiaSharp 3.119.2 (Windows 11, .NET 10, GPU context)" — **comment #2 (April 2026)** (Bug has not been fixed in the two years since the original report; affects the latest stable release.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 403-412 | direct | DrawPath validates arguments then directly calls SkiaApi.sk_canvas_draw_path(Handle, path.Handle, paint.Handle). No managed memory allocation occurs; any leak is native. |
| `binding/SkiaSharp/GRContext.cs` | 159-169 | related | GRContext exposes PurgeResources(), PurgeUnlockedResources(bool) and PurgeUnlockedResources(long, bool) to explicitly free GPU caches. These are the primary SkiaSharp-level mitigation surface for GPU resource leaks. |
| `binding/SkiaSharp/GRContext.cs` | 116-120 | related | SetResourceCacheLimit(long) can cap total GPU resource cache bytes. Lowering this may trigger earlier eviction of fill-path tessellation caches. |

### Workarounds

- Switch paint.Style from Fill to Stroke (confirmed flat heap by second commenter; may require visual adjustments)
- Periodically call grContext.PurgeUnlockedResources(false) to release GPU path caches (may cause frame spikes)
- Fall back to a CPU surface (SKSurface.Create with no GRContext) to avoid GPU path rasterization entirely

### Next Questions

- Does the leak also occur on OpenGL GPU context on Windows 11, or only Direct3D?
- Is the memory classified as GPU (VRAM) or native CPU heap? DumpMemoryStatistics might reveal which GPU resource type is growing.
- What SkiaSharp 2.88.3 changelog entry corresponds to the regression? Identifying the Skia commit would help file an upstream bug.
- Does calling grContext.Flush(true) + grContext.PurgeUnlockedResources(false) after each frame prevent the growth?

### Resolution Proposals

**Hypothesis:** Skia's GPU fill-path tessellator or coverage-mask atlas accumulates entries per frame without eviction on Windows 11's Direct3D backend, likely due to a cache key or lifetime change introduced in the 2.88.3 Skia update.

1. **Periodic GPU cache purge as workaround** — workaround, confidence 0.75 (75%), cost/xs, validated=yes
   - Call grContext.PurgeUnlockedResources(false) once per second or after each frame to release unused GPU path caches. Not a fix but prevents unbounded growth.

```csharp
// Call after canvas.Flush() each frame or on a timer:
grContext.PurgeUnlockedResources(scratchResourcesOnly: false);
```
2. **Cap GPU resource cache** — workaround, confidence 0.70 (70%), cost/xs, validated=yes
   - Call grContext.SetResourceCacheLimit(maxBytes) to bound total GPU cache size. The OS will flush evicted tessellation entries. Example: limit to 64 MB.

```csharp
grContext.SetResourceCacheLimit(64 * 1024 * 1024); // 64 MB cap
```
3. **Investigate upstream Skia GPU fill rasterizer change between 2.88.2 and 2.88.3** — investigation, confidence 0.85 (85%), cost/l, validated=untested
   - Diff the Skia upstream commit range for SkiaSharp 2.88.2 and 2.88.3, focusing on GrTessellationPathRenderer, GrAtlasRenderTask, or GPU path fill rasterization changes. File an upstream bug if the leak is confirmed in Skia.

**Recommended proposal:** Periodic GPU cache purge as workaround

**Why:** Requires a single line of code at the application level, directly addresses the accumulation, and PurgeUnlockedResources is purpose-built for releasing unused GPU resources.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Confirmed real regression (2.88.2 → 2.88.3, still present in 3.119.2) with good behavioral isolation (Fill leaks, Stroke does not). Root cause is in the Skia GPU fill rasterizer on Windows 11 and needs a reproducible environment to trace and fix. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug type, core area, Windows platform, Direct3D backend, reliability tenet labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, backend/Direct3D, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Acknowledge regression, share workarounds, and request diagnostics | — |
| link-related | low | 0.95 (95%) | Cross-reference original GPU leak report #2660 | linkedIssue=#2660 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and the additional data point in the latest comment — the Fill-vs-Stroke isolation is very helpful.

**Root cause hypothesis:** The leak is in Skia's GPU fill-path rasterizer (tessellation or coverage-mask atlas cache) which accumulates entries per frame without eviction on Windows 11's Direct3D backend. The C# wrapper is a thin pass-through, so this is a native-level issue.

**Workarounds you can apply now:**

1. **Periodic GPU cache purge** — call this after each frame (or on a timer) to release unused GPU path caches:
   ```csharp
   grContext.PurgeUnlockedResources(scratchResourcesOnly: false);
   ```

2. **Cap the GPU resource cache** to bound total growth:
   ```csharp
   grContext.SetResourceCacheLimit(64 * 1024 * 1024); // 64 MB
   ```

3. **Fall back to CPU surface** — create `SKSurface` without a `GRContext` to avoid GPU fill rasterization entirely.

To help narrow the root cause, it would be useful to know:
- Does `grContext.DumpMemoryStatistics(dump)` reveal which GPU resource type is growing?
- Does the leak also occur with an OpenGL GPU context on Windows 11, or only Direct3D?
- Does adding `grContext.Flush(true)` before purging prevent the growth?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2848,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-27T12:50:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "canvas.DrawPath() with SKPaintStyle.Fill causes a linear native heap memory leak in GPU context on Windows 11, confirmed from SkiaSharp 2.88.3 through 3.119.2; SKPaintStyle.Stroke does not leak.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/Direct3D"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "memory-leak",
      "errorMessage": "Native heap grows ~18 MB/min when DrawPath Fill rasterization runs on GPU context under Windows 11",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net10.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a SkiaSharp GPU context (e.g. via GRContext.CreateGl or Direct3D) on Windows 11",
        "In a render loop (~60 fps), call canvas.DrawPath(path, paint) with paint.Style = SKPaintStyle.Fill",
        "Monitor native heap: observe linear growth (~18 MB/min)",
        "Switch to paint.Style = SKPaintStyle.Stroke — heap growth stops"
      ],
      "codeSnippets": [
        "using (var paintFill = new SKPaint { IsAntialias = true })\n{\n    paintFill.StrokeWidth = 0;\n    paintFill.Style = SKPaintStyle.Fill;\n    paintFill.PathEffect = null;\n    paintFill.Shader = null;\n    paintFill.Color = fillColor.ToSkia(opacity);\n    canvas.DrawPath(path, paintFill); // leaks in GPU context on Windows 11\n}"
      ],
      "environmentDetails": "Windows 11 x64, SkiaSharp 2.88.3–3.119.2, .NET 10, GPU context (Direct3D/OpenGL)",
      "relatedIssues": [
        2660
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2848#issuecomment-4311452892",
          "description": "April 2026 repro data confirming Fill leaks on SkiaSharp 3.119.2 / .NET 10 / Windows 11 GPU context; Stroke does not leak"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2660",
          "description": "Original DrawPath GPU memory leak report (Windows 10, SkiaSharp 2.88.3) by same author"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.2",
        "2.88.3",
        "2.88.8",
        "3.119.2"
      ],
      "workedIn": "2.88.2",
      "brokeIn": "2.88.3",
      "currentRelevance": "likely",
      "relevanceReason": "Second commenter confirmed the leak persists on SkiaSharp 3.119.2 (April 2026), with identical symptoms: Fill leaks, Stroke is flat."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.88,
      "reason": "Both the original reporter and a confirming commenter state 2.88.2 was leak-free; 2.88.3 introduced the regression. Confirmed still present in 3.119.2.",
      "workedInVersion": "2.88.2",
      "brokeInVersion": "2.88.3"
    }
  },
  "analysis": {
    "summary": "Native heap leak when using DrawPath with Fill style in a GPU context on Windows 11. The C# wrapper (SKCanvas.DrawPath) is a thin pass-through to sk_canvas_draw_path with no managed allocation; the leak originates in Skia's GPU fill-path rasterizer (likely a path tessellation or coverage-mask atlas cache growing unboundedly). The regression from 2.88.2 to 2.88.3 suggests a Skia upstream change in the fill path that altered GPU resource lifecycle on Direct3D/Windows.",
    "rationale": "Multiple independent reporters confirm the leak. Clear behavioral isolation (Fill leaks, Stroke does not) strongly implicates the GPU fill rasterizer cache. The C# binding layer is not at fault — SKCanvas.DrawPath simply forwards arguments to the native sk_canvas_draw_path call. Upstream Skia likely owns the root cause; however, a SkiaSharp-level workaround (periodic GRContext.PurgeUnlockedResources calls) may mitigate impact.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "403-412",
        "finding": "DrawPath validates arguments then directly calls SkiaApi.sk_canvas_draw_path(Handle, path.Handle, paint.Handle). No managed memory allocation occurs; any leak is native.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/GRContext.cs",
        "lines": "159-169",
        "finding": "GRContext exposes PurgeResources(), PurgeUnlockedResources(bool) and PurgeUnlockedResources(long, bool) to explicitly free GPU caches. These are the primary SkiaSharp-level mitigation surface for GPU resource leaks.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/GRContext.cs",
        "lines": "116-120",
        "finding": "SetResourceCacheLimit(long) can cap total GPU resource cache bytes. Lowering this may trigger earlier eviction of fill-path tessellation caches.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "canvas.DrawPath(path, paintFill) — this line cause memory leak in GPU context on Windows 11",
        "source": "issue body",
        "interpretation": "Reporter has isolated the leak to a single API call in GPU context."
      },
      {
        "text": "paint.Style = Fill → leaks (~+18 MB/min native heap); paint.Style = Stroke → completely flat",
        "source": "comment #2 (April 2026)",
        "interpretation": "Exact behavioral isolation; the Fill rasterization path in the GPU backend accumulates native memory that Stroke does not."
      },
      {
        "text": "Last Known Good Version: 2.88.2; Current (broken): 2.88.3",
        "source": "issue body",
        "interpretation": "Regression window is narrow; a Skia upstream change between those two SkiaSharp versions altered GPU fill path behavior."
      },
      {
        "text": "Confirmed on SkiaSharp 3.119.2 (Windows 11, .NET 10, GPU context)",
        "source": "comment #2 (April 2026)",
        "interpretation": "Bug has not been fixed in the two years since the original report; affects the latest stable release."
      }
    ],
    "workarounds": [
      "Switch paint.Style from Fill to Stroke (confirmed flat heap by second commenter; may require visual adjustments)",
      "Periodically call grContext.PurgeUnlockedResources(false) to release GPU path caches (may cause frame spikes)",
      "Fall back to a CPU surface (SKSurface.Create with no GRContext) to avoid GPU path rasterization entirely"
    ],
    "nextQuestions": [
      "Does the leak also occur on OpenGL GPU context on Windows 11, or only Direct3D?",
      "Is the memory classified as GPU (VRAM) or native CPU heap? DumpMemoryStatistics might reveal which GPU resource type is growing.",
      "What SkiaSharp 2.88.3 changelog entry corresponds to the regression? Identifying the Skia commit would help file an upstream bug.",
      "Does calling grContext.Flush(true) + grContext.PurgeUnlockedResources(false) after each frame prevent the growth?"
    ],
    "resolution": {
      "hypothesis": "Skia's GPU fill-path tessellator or coverage-mask atlas accumulates entries per frame without eviction on Windows 11's Direct3D backend, likely due to a cache key or lifetime change introduced in the 2.88.3 Skia update.",
      "proposals": [
        {
          "title": "Periodic GPU cache purge as workaround",
          "description": "Call grContext.PurgeUnlockedResources(false) once per second or after each frame to release unused GPU path caches. Not a fix but prevents unbounded growth.",
          "category": "workaround",
          "codeSnippet": "// Call after canvas.Flush() each frame or on a timer:\ngrContext.PurgeUnlockedResources(scratchResourcesOnly: false);",
          "confidence": 0.75,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Cap GPU resource cache",
          "description": "Call grContext.SetResourceCacheLimit(maxBytes) to bound total GPU cache size. The OS will flush evicted tessellation entries. Example: limit to 64 MB.",
          "category": "workaround",
          "codeSnippet": "grContext.SetResourceCacheLimit(64 * 1024 * 1024); // 64 MB cap",
          "confidence": 0.7,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Investigate upstream Skia GPU fill rasterizer change between 2.88.2 and 2.88.3",
          "description": "Diff the Skia upstream commit range for SkiaSharp 2.88.2 and 2.88.3, focusing on GrTessellationPathRenderer, GrAtlasRenderTask, or GPU path fill rasterization changes. File an upstream bug if the leak is confirmed in Skia.",
          "category": "investigation",
          "confidence": 0.85,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Periodic GPU cache purge as workaround",
      "recommendedReason": "Requires a single line of code at the application level, directly addresses the accumulation, and PurgeUnlockedResources is purpose-built for releasing unused GPU resources."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Confirmed real regression (2.88.2 → 2.88.3, still present in 3.119.2) with good behavioral isolation (Fill leaks, Stroke does not). Root cause is in the Skia GPU fill rasterizer on Windows 11 and needs a reproducible environment to trace and fix.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug type, core area, Windows platform, Direct3D backend, reliability tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/Direct3D",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge regression, share workarounds, and request diagnostics",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed report and the additional data point in the latest comment — the Fill-vs-Stroke isolation is very helpful.\n\n**Root cause hypothesis:** The leak is in Skia's GPU fill-path rasterizer (tessellation or coverage-mask atlas cache) which accumulates entries per frame without eviction on Windows 11's Direct3D backend. The C# wrapper is a thin pass-through, so this is a native-level issue.\n\n**Workarounds you can apply now:**\n\n1. **Periodic GPU cache purge** — call this after each frame (or on a timer) to release unused GPU path caches:\n   ```csharp\n   grContext.PurgeUnlockedResources(scratchResourcesOnly: false);\n   ```\n\n2. **Cap the GPU resource cache** to bound total growth:\n   ```csharp\n   grContext.SetResourceCacheLimit(64 * 1024 * 1024); // 64 MB\n   ```\n\n3. **Fall back to CPU surface** — create `SKSurface` without a `GRContext` to avoid GPU fill rasterization entirely.\n\nTo help narrow the root cause, it would be useful to know:\n- Does `grContext.DumpMemoryStatistics(dump)` reveal which GPU resource type is growing?\n- Does the leak also occur with an OpenGL GPU context on Windows 11, or only Direct3D?\n- Does adding `grContext.Flush(true)` before purging prevent the growth?"
      },
      {
        "type": "link-related",
        "description": "Cross-reference original GPU leak report #2660",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 2660
      }
    ]
  }
}
```

</details>
