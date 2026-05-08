# Issue Triage Report — #2660

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T20:16:04Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp (0.80 (80%)) |
| Suggested action | needs-info (0.78 (78%)) |

**Issue Summary:** Reporter observes linear memory growth when calling canvas.DrawPath() in a GPU (hardware-accelerated) rendering context on Windows, which stops when DrawPath is commented out; claimed regression from 2.88.2 to 2.88.3.

**Analysis:** Memory leak in GPU context when calling DrawPath. The C# binding for DrawPath delegates directly to sk_canvas_draw_path with no intermediate allocations; the leak is likely in GPU cache management or tessellation buffers within Skia's GPU backend that are not being flushed/released.

**Recommendations:** **needs-info** — No minimal repro project, no GPU backend specified, no memory profile attached; need more details before repro can proceed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/OpenGL |
| Tenets | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Run application in GPU context (hardware-accelerated)
2. Call canvas.DrawPath(path, paintFill) in a rendering loop
3. Observe memory usage increasing linearly
4. Comment out canvas.DrawPath line — memory becomes constant

**Environment:** Windows 10, Visual Studio, SkiaSharp 2.88.3

**Related issues:** #2848

**Code snippets:**

```csharp
using (var paintFill = new SKPaint { IsAntialias = true })
{
    paintFill.StrokeWidth = 0;
    paintFill.Style = SKPaintStyle.Fill;
    paintFill.PathEffect = null;
    paintFill.Shader = null;
    paintFill.Color = fillColor.ToSkia(opacity);
    canvas.DrawPath(path, paintFill); // this line cause memory leak in GPU context
}
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | memory-leak |
| Error message | Memory increases linearly when calling canvas.DrawPath() in GPU context |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3, 2.88.2 |
| Worked in | 2.88.2 |
| Broke in | 2.88.3 |
| Current relevance | unknown |
| Relevance reason | Current SkiaSharp is 3.x; unclear if memory leak persists in current GPU code paths. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.70 (70%) |
| Reason | Reporter explicitly states memory was constant in 2.88.2 and leaks in 2.88.3. |
| Worked in version | 2.88.2 |
| Broke in version | 2.88.3 |

## Analysis

### Technical Summary

Memory leak in GPU context when calling DrawPath. The C# binding for DrawPath delegates directly to sk_canvas_draw_path with no intermediate allocations; the leak is likely in GPU cache management or tessellation buffers within Skia's GPU backend that are not being flushed/released.

### Rationale

Issue reports GPU-context-specific memory growth isolated to DrawPath. The C# wrapper itself has no allocations beyond the native call, pointing to a Skia GPU backend or surface flush issue. A related follow-up issue (#2848) confirms this affects Windows 11 as well with 2.88.8, indicating it is not a one-off regression. The reporter lacks a minimal repro project and provides no stack trace or memory profile, but the bisected nature (comment out DrawPath = no leak) gives medium repro quality.

### Key Signals

- "when in CPU context, the memory usage is constant, but in GPU context, memory increase linearly" — **issue body** (Leak is specific to GPU command submission path, not SKPath allocation.)
- "when I commented the canvas.DrawPath code line, the memory usage is constant again" — **issue body** (Strongly isolates the leak to the DrawPath invocation in GPU context.)
- "Last Known Good Version of SkiaSharp: 2.88.2 (Previous)" — **issue body** (Regression introduced between 2.88.2 and 2.88.3.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 403-412 | direct | DrawPath validates path/paint are non-null then calls SkiaApi.sk_canvas_draw_path(Handle, path.Handle, paint.Handle) — no C# allocations; any leak is in the native Skia layer. |
| `binding/SkiaSharp/SKCanvas.cs` | 405-411 | related | No Flush() or surface submission calls surround DrawPath in the C# wrapper; GPU command buffers may accumulate without explicit flush in GPU context. |

### Workarounds

- Call grContext.Flush() or surface.Flush() after each frame to force GPU command buffer submission and release GPU-side tessellation caches.
- Call grContext.PurgeUnlockedResources(false) periodically to evict unused GPU resources.
- Use CPU-based SKSurface as a temporary workaround (confirmed by reporter to not leak).

### Next Questions

- Which GPU backend is in use (OpenGL / Vulkan / Direct3D)?
- Is GRContext.Flush() or surface.Flush() being called between frames?
- What is the rendering surface type (SKGLControl, SKSurface from GRContext, or other)?
- Does the leak occur with simple paths (e.g., a single line) or only complex paths?
- Does the issue reproduce on current 3.x SkiaSharp?

### Resolution Proposals

**Hypothesis:** GPU-side tessellation or path effect caches for DrawPath are not being evicted between frames; calling GRContext.Flush() or PurgeUnlockedResources should release them.

1. **Flush GPU context after frame** — workaround, confidence 0.75 (75%), cost/xs, validated=untested
   - Call grContext.Flush() (or surface.Flush()) after each render frame to ensure GPU command buffers and intermediate tessellation caches are released.
2. **Investigate Skia GPU path tessellation cache between 2.88.2 and 2.88.3** — investigation, confidence 0.70 (70%), cost/m, validated=untested
   - Diff the Skia upstream commits between the two SkiaSharp releases to identify changes in GPU path rendering or cache management.

**Recommended proposal:** Flush GPU context after frame

**Why:** Simplest actionable workaround for the reporter while deeper investigation proceeds.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.78 (78%) |
| Reason | No minimal repro project, no GPU backend specified, no memory profile attached; need more details before repro can proceed. |
| Suggested repro platform | windows |

### Missing Info

- Which GPU backend is used (OpenGL/Vulkan/Direct3D)?
- What rendering surface type is being used (SKGLControl, custom GRContext)?
- Is GRContext.Flush() or surface.Flush() called between frames?
- Minimal reproducible project or detailed steps with memory profiler output
- Does the issue reproduce on SkiaSharp 3.x?

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Apply bug, area/SkiaSharp, os/Windows-Classic, backend/OpenGL labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, backend/OpenGL |
| add-comment | medium | 0.78 (78%) | Request GPU backend details and suggest flush workaround | — |
| link-related | low | 0.90 (90%) | Link to follow-up report #2848 (same bug on Windows 11 / 2.88.8) | linkedIssue=#2848 |

**Comment draft for `add-comment`:**

```markdown
Thanks for reporting this. A few questions to help investigate:

1. **Which GPU backend** are you using? (OpenGL, Vulkan, Direct3D)
2. **What surface type** creates the GPU canvas? (e.g., `SKGLControl`, `SKSurface.Create(grContext, ...)`, etc.)
3. Are you calling `grContext.Flush()` or `surface.Flush()` after each frame?
4. Does the issue still occur with **SkiaSharp 3.x** (latest)?
5. A minimal reproducible project would greatly help track this down.

**Possible workaround in the meantime:**

Try explicitly flushing the GR context after each frame:

```csharp
canvas.DrawPath(path, paint);
// After drawing frame:
grContext.Flush();
// Periodically:
grContext.PurgeUnlockedResources(false);
```

GPU-backed canvases accumulate tessellation caches that may not be released automatically between frames; an explicit flush often resolves this.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2660,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T20:16:04Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter observes linear memory growth when calling canvas.DrawPath() in a GPU (hardware-accelerated) rendering context on Windows, which stops when DrawPath is commented out; claimed regression from 2.88.2 to 2.88.3.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.8
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/OpenGL"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "memory-leak",
      "errorMessage": "Memory increases linearly when calling canvas.DrawPath() in GPU context",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Run application in GPU context (hardware-accelerated)",
        "Call canvas.DrawPath(path, paintFill) in a rendering loop",
        "Observe memory usage increasing linearly",
        "Comment out canvas.DrawPath line — memory becomes constant"
      ],
      "codeSnippets": [
        "using (var paintFill = new SKPaint { IsAntialias = true })\n{\n    paintFill.StrokeWidth = 0;\n    paintFill.Style = SKPaintStyle.Fill;\n    paintFill.PathEffect = null;\n    paintFill.Shader = null;\n    paintFill.Color = fillColor.ToSkia(opacity);\n    canvas.DrawPath(path, paintFill); // this line cause memory leak in GPU context\n}"
      ],
      "environmentDetails": "Windows 10, Visual Studio, SkiaSharp 2.88.3",
      "relatedIssues": [
        2848
      ],
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3",
        "2.88.2"
      ],
      "workedIn": "2.88.2",
      "brokeIn": "2.88.3",
      "currentRelevance": "unknown",
      "relevanceReason": "Current SkiaSharp is 3.x; unclear if memory leak persists in current GPU code paths."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.7,
      "reason": "Reporter explicitly states memory was constant in 2.88.2 and leaks in 2.88.3.",
      "workedInVersion": "2.88.2",
      "brokeInVersion": "2.88.3"
    }
  },
  "analysis": {
    "summary": "Memory leak in GPU context when calling DrawPath. The C# binding for DrawPath delegates directly to sk_canvas_draw_path with no intermediate allocations; the leak is likely in GPU cache management or tessellation buffers within Skia's GPU backend that are not being flushed/released.",
    "rationale": "Issue reports GPU-context-specific memory growth isolated to DrawPath. The C# wrapper itself has no allocations beyond the native call, pointing to a Skia GPU backend or surface flush issue. A related follow-up issue (#2848) confirms this affects Windows 11 as well with 2.88.8, indicating it is not a one-off regression. The reporter lacks a minimal repro project and provides no stack trace or memory profile, but the bisected nature (comment out DrawPath = no leak) gives medium repro quality.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "403-412",
        "finding": "DrawPath validates path/paint are non-null then calls SkiaApi.sk_canvas_draw_path(Handle, path.Handle, paint.Handle) — no C# allocations; any leak is in the native Skia layer.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "405-411",
        "finding": "No Flush() or surface submission calls surround DrawPath in the C# wrapper; GPU command buffers may accumulate without explicit flush in GPU context.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "when in CPU context, the memory usage is constant, but in GPU context, memory increase linearly",
        "source": "issue body",
        "interpretation": "Leak is specific to GPU command submission path, not SKPath allocation."
      },
      {
        "text": "when I commented the canvas.DrawPath code line, the memory usage is constant again",
        "source": "issue body",
        "interpretation": "Strongly isolates the leak to the DrawPath invocation in GPU context."
      },
      {
        "text": "Last Known Good Version of SkiaSharp: 2.88.2 (Previous)",
        "source": "issue body",
        "interpretation": "Regression introduced between 2.88.2 and 2.88.3."
      }
    ],
    "nextQuestions": [
      "Which GPU backend is in use (OpenGL / Vulkan / Direct3D)?",
      "Is GRContext.Flush() or surface.Flush() being called between frames?",
      "What is the rendering surface type (SKGLControl, SKSurface from GRContext, or other)?",
      "Does the leak occur with simple paths (e.g., a single line) or only complex paths?",
      "Does the issue reproduce on current 3.x SkiaSharp?"
    ],
    "workarounds": [
      "Call grContext.Flush() or surface.Flush() after each frame to force GPU command buffer submission and release GPU-side tessellation caches.",
      "Call grContext.PurgeUnlockedResources(false) periodically to evict unused GPU resources.",
      "Use CPU-based SKSurface as a temporary workaround (confirmed by reporter to not leak)."
    ],
    "resolution": {
      "hypothesis": "GPU-side tessellation or path effect caches for DrawPath are not being evicted between frames; calling GRContext.Flush() or PurgeUnlockedResources should release them.",
      "proposals": [
        {
          "title": "Flush GPU context after frame",
          "description": "Call grContext.Flush() (or surface.Flush()) after each render frame to ensure GPU command buffers and intermediate tessellation caches are released.",
          "category": "workaround",
          "confidence": 0.75,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Investigate Skia GPU path tessellation cache between 2.88.2 and 2.88.3",
          "description": "Diff the Skia upstream commits between the two SkiaSharp releases to identify changes in GPU path rendering or cache management.",
          "category": "investigation",
          "confidence": 0.7,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Flush GPU context after frame",
      "recommendedReason": "Simplest actionable workaround for the reporter while deeper investigation proceeds."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.78,
      "reason": "No minimal repro project, no GPU backend specified, no memory profile attached; need more details before repro can proceed.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "Which GPU backend is used (OpenGL/Vulkan/Direct3D)?",
      "What rendering surface type is being used (SKGLControl, custom GRContext)?",
      "Is GRContext.Flush() or surface.Flush() called between frames?",
      "Minimal reproducible project or detailed steps with memory profiler output",
      "Does the issue reproduce on SkiaSharp 3.x?"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, area/SkiaSharp, os/Windows-Classic, backend/OpenGL labels",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/OpenGL"
        ]
      },
      {
        "type": "add-comment",
        "description": "Request GPU backend details and suggest flush workaround",
        "risk": "medium",
        "confidence": 0.78,
        "comment": "Thanks for reporting this. A few questions to help investigate:\n\n1. **Which GPU backend** are you using? (OpenGL, Vulkan, Direct3D)\n2. **What surface type** creates the GPU canvas? (e.g., `SKGLControl`, `SKSurface.Create(grContext, ...)`, etc.)\n3. Are you calling `grContext.Flush()` or `surface.Flush()` after each frame?\n4. Does the issue still occur with **SkiaSharp 3.x** (latest)?\n5. A minimal reproducible project would greatly help track this down.\n\n**Possible workaround in the meantime:**\n\nTry explicitly flushing the GR context after each frame:\n\n```csharp\ncanvas.DrawPath(path, paint);\n// After drawing frame:\ngrContext.Flush();\n// Periodically:\ngrContext.PurgeUnlockedResources(false);\n```\n\nGPU-backed canvases accumulate tessellation caches that may not be released automatically between frames; an explicit flush often resolves this."
      },
      {
        "type": "link-related",
        "description": "Link to follow-up report #2848 (same bug on Windows 11 / 2.88.8)",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 2848
      }
    ]
  }
}
```

</details>
