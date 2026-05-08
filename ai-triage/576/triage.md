# Issue Triage Report — #576

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-27T11:26:00Z |
| Type | type/bug (0.78 (78%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | close-as-not-a-bug (0.82 (82%)) |

**Issue Summary:** Reporter observed 30-40 MB GPU memory growth per iteration when using GPU-accelerated SKSurface with Snapshot() on Windows with Intel HD Graphics 520; root cause identified in thread as overwriting the SKImage array slot without disposing the old image first.

**Analysis:** The memory leak is caused by overwriting SKImage array slots without disposing the previous entry first. GPU-backed SKImage objects hold native GPU texture memory; the .NET GC cannot observe or reclaim GPU memory pressure, so objects must be explicitly disposed. The reporter self-identified this in the comment thread and confirmed the fix.

**Recommendations:** **close-as-not-a-bug** — Memory leak caused by user not disposing old SKImage entries before overwriting. GPU memory is not visible to the .NET GC and requires explicit Dispose(). Reporter confirmed the fix in the thread. Behavior is by design (ISKReferenceCounted).

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/OpenGL |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, area/SkiaSharp, backend/OpenGL |

## Evidence

### Reproduction

1. Create a GPU-accelerated SKSurface using SKSurface.Create(GRContext, false, SKImageInfo)
2. Render to the canvas, flush, and call surface.Snapshot()
3. Store the Snapshot result in an array indexed by plane without disposing the old entry
4. Dispose the surface and repeat in a loop
5. Observe 30-40 MB memory growth per iteration until app crashes

**Environment:** Windows desktop app, Intel HD Graphics 520, Visual Studio memory diagnostics showing 30-40 MB step-ups

**Related issues:** #1244

**Code snippets:**

```csharp
var surface = SKSurface.Create(_skiaGlControl.GRContext, false, new SKImageInfo(Width, Height));
_skiaMain.Compose(surface.Canvas, plane);
surface.Canvas.Flush();
_skiaMain.DisplayImages[plane] = surface.Snapshot(); // old image not disposed before overwrite
surface.Dispose();
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | memory-leak |
| Error message | — |
| Repro quality | partial |
| Target frameworks | — |

## Analysis

### Technical Summary

The memory leak is caused by overwriting SKImage array slots without disposing the previous entry first. GPU-backed SKImage objects hold native GPU texture memory; the .NET GC cannot observe or reclaim GPU memory pressure, so objects must be explicitly disposed. The reporter self-identified this in the comment thread and confirmed the fix.

### Rationale

The root cause is a usage pattern issue: GPU-backed SKImage objects must be explicitly disposed before overwriting references because the .NET GC cannot observe native GPU memory. The reporter confirmed this and found the fix themselves. The behavior is by-design in SkiaSharp (ISKReferenceCounted pattern), and no code change in SkiaSharp is warranted. Closing as not-a-bug is appropriate, paired with an explanatory comment for future visitors.

### Key Signals

- "every time I run through this section of code visual studios shows 30-40MB step ups in memory usage until eventually it crashes" — **issue body** (Classic GPU texture accumulation — each Snapshot() allocates a new GPU texture that is never freed because the old SKImage is silently overwritten.)
- "when I would overwrite the SkImage without disposing first then the ref was getting lost and when it was getting lost in the GPU memory GC.Collect was not cleaning up the lost reference" — **reporter comment (mclose90, 2018-07-24)** (Reporter self-diagnosed: GPU memory is not visible to the .NET GC, so overwriting a reference without Dispose() leaks GPU memory indefinitely.)
- "I have desperately used garbage collection as a means to make the program run longer" — **issue body** (GC.Collect() does not help with GPU-backed native resources — only explicit Dispose() releases the underlying GPU texture.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKImage.cs` | 16-24 | direct | SKImage extends SKObject and implements ISKReferenceCounted. The native ref is only released on explicit Dispose() or when the finalizer runs. The .NET GC cannot see GPU memory, so unreferenced SKImage objects holding GPU textures may not be collected promptly enough to prevent GPU OOM. |
| `binding/SkiaSharp/SKSurface.cs` | 274-275 | direct | SKSurface.Snapshot() returns a new SKImage that owns a GPU texture (via sk_surface_new_image_snapshot). Calling this repeatedly and storing results in an array without disposing old entries accumulates unreleased GPU textures. |

### Workarounds

- Dispose the old SKImage before overwriting: call _skiaMain.DisplayImages[plane]?.Dispose() immediately before assigning surface.Snapshot()
- Call GRContext.PurgeUnlockedResources(false) periodically to release GPU resource cache entries that have already been disposed
- Avoid Snapshot() entirely for intermediate frames if the rendered image is only needed temporarily — draw directly and call Flush() instead

### Resolution Proposals

**Hypothesis:** Not a SkiaSharp defect. GPU-backed objects require explicit Dispose() before abandoning references; GC cannot reclaim GPU memory. Reporter confirmed the fix.

1. **Dispose old image before overwriting** — fix, confidence 0.95 (95%), cost/xs, validated=yes
   - Call Dispose() on the existing SKImage entry before assigning a new Snapshot() result to release the GPU texture immediately.

```csharp
_skiaMain.DisplayImages[plane]?.Dispose();
_skiaMain.DisplayImages[plane] = surface.Snapshot();
```
2. **Periodic GPU cache purge** — workaround, confidence 0.78 (78%), cost/xs, validated=untested
   - After disposing images, call GRContext.PurgeUnlockedResources(false) to release GPU memory from Skia's internal cache. Helpful when accumulation is still observed.

```csharp
_skiaGlControl.GRContext.PurgeUnlockedResources(false);
```

**Recommended proposal:** Dispose old image before overwriting

**Why:** Reporter confirmed this is exactly what fixed the leak. Trivial change, zero risk.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.82 (82%) |
| Reason | Memory leak caused by user not disposing old SKImage entries before overwriting. GPU memory is not visible to the .NET GC and requires explicit Dispose(). Reporter confirmed the fix in the thread. Behavior is by design (ISKReferenceCounted). |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply or confirm: type/bug, area/SkiaSharp, os/Windows-Classic, backend/OpenGL, tenet/reliability | labels=type/bug, area/SkiaSharp, os/Windows-Classic, backend/OpenGL, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Explain GPU disposal pattern, confirm reporter's fix, and add the GRContext.PurgeUnlockedResources tip | — |
| close-issue | medium | 0.82 (82%) | Close as not a bug — by-design GPU disposal behaviour; reporter confirmed fix | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the follow-up and for sharing what fixed it!

This is a very common gotcha with GPU-backed SkiaSharp objects: the .NET garbage collector cannot see GPU/native memory, so `GC.Collect()` will not free GPU textures. You must explicitly call `Dispose()` on any `SKImage` (or other GPU-backed object) **before** overwriting or abandoning the reference.

The correct pattern:

```csharp
// Dispose the old image BEFORE overwriting
_skiaMain.DisplayImages[plane]?.Dispose();
_skiaMain.DisplayImages[plane] = surface.Snapshot();
```

As an additional tip: if you still see GPU memory growing over time (due to Skia's internal resource cache), you can call `GRContext.PurgeUnlockedResources(false)` periodically to flush GPU resources that are cached but no longer in active use.

Closing as by-design — let us know if you hit anything else!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 576,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-27T11:26:00Z",
    "currentLabels": [
      "type/bug",
      "area/SkiaSharp",
      "backend/OpenGL"
    ]
  },
  "summary": "Reporter observed 30-40 MB GPU memory growth per iteration when using GPU-accelerated SKSurface with Snapshot() on Windows with Intel HD Graphics 520; root cause identified in thread as overwriting the SKImage array slot without disposing the old image first.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.78
    },
    "area": {
      "value": "area/SkiaSharp",
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
      "regressionClaimed": false,
      "errorType": "memory-leak",
      "reproQuality": "partial"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a GPU-accelerated SKSurface using SKSurface.Create(GRContext, false, SKImageInfo)",
        "Render to the canvas, flush, and call surface.Snapshot()",
        "Store the Snapshot result in an array indexed by plane without disposing the old entry",
        "Dispose the surface and repeat in a loop",
        "Observe 30-40 MB memory growth per iteration until app crashes"
      ],
      "environmentDetails": "Windows desktop app, Intel HD Graphics 520, Visual Studio memory diagnostics showing 30-40 MB step-ups",
      "relatedIssues": [
        1244
      ],
      "codeSnippets": [
        "var surface = SKSurface.Create(_skiaGlControl.GRContext, false, new SKImageInfo(Width, Height));\n_skiaMain.Compose(surface.Canvas, plane);\nsurface.Canvas.Flush();\n_skiaMain.DisplayImages[plane] = surface.Snapshot(); // old image not disposed before overwrite\nsurface.Dispose();"
      ]
    }
  },
  "analysis": {
    "summary": "The memory leak is caused by overwriting SKImage array slots without disposing the previous entry first. GPU-backed SKImage objects hold native GPU texture memory; the .NET GC cannot observe or reclaim GPU memory pressure, so objects must be explicitly disposed. The reporter self-identified this in the comment thread and confirmed the fix.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "16-24",
        "finding": "SKImage extends SKObject and implements ISKReferenceCounted. The native ref is only released on explicit Dispose() or when the finalizer runs. The .NET GC cannot see GPU memory, so unreferenced SKImage objects holding GPU textures may not be collected promptly enough to prevent GPU OOM.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKSurface.cs",
        "lines": "274-275",
        "finding": "SKSurface.Snapshot() returns a new SKImage that owns a GPU texture (via sk_surface_new_image_snapshot). Calling this repeatedly and storing results in an array without disposing old entries accumulates unreleased GPU textures.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "every time I run through this section of code visual studios shows 30-40MB step ups in memory usage until eventually it crashes",
        "source": "issue body",
        "interpretation": "Classic GPU texture accumulation — each Snapshot() allocates a new GPU texture that is never freed because the old SKImage is silently overwritten."
      },
      {
        "text": "when I would overwrite the SkImage without disposing first then the ref was getting lost and when it was getting lost in the GPU memory GC.Collect was not cleaning up the lost reference",
        "source": "reporter comment (mclose90, 2018-07-24)",
        "interpretation": "Reporter self-diagnosed: GPU memory is not visible to the .NET GC, so overwriting a reference without Dispose() leaks GPU memory indefinitely."
      },
      {
        "text": "I have desperately used garbage collection as a means to make the program run longer",
        "source": "issue body",
        "interpretation": "GC.Collect() does not help with GPU-backed native resources — only explicit Dispose() releases the underlying GPU texture."
      }
    ],
    "rationale": "The root cause is a usage pattern issue: GPU-backed SKImage objects must be explicitly disposed before overwriting references because the .NET GC cannot observe native GPU memory. The reporter confirmed this and found the fix themselves. The behavior is by-design in SkiaSharp (ISKReferenceCounted pattern), and no code change in SkiaSharp is warranted. Closing as not-a-bug is appropriate, paired with an explanatory comment for future visitors.",
    "workarounds": [
      "Dispose the old SKImage before overwriting: call _skiaMain.DisplayImages[plane]?.Dispose() immediately before assigning surface.Snapshot()",
      "Call GRContext.PurgeUnlockedResources(false) periodically to release GPU resource cache entries that have already been disposed",
      "Avoid Snapshot() entirely for intermediate frames if the rendered image is only needed temporarily — draw directly and call Flush() instead"
    ],
    "resolution": {
      "hypothesis": "Not a SkiaSharp defect. GPU-backed objects require explicit Dispose() before abandoning references; GC cannot reclaim GPU memory. Reporter confirmed the fix.",
      "proposals": [
        {
          "title": "Dispose old image before overwriting",
          "description": "Call Dispose() on the existing SKImage entry before assigning a new Snapshot() result to release the GPU texture immediately.",
          "category": "fix",
          "codeSnippet": "_skiaMain.DisplayImages[plane]?.Dispose();\n_skiaMain.DisplayImages[plane] = surface.Snapshot();",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Periodic GPU cache purge",
          "description": "After disposing images, call GRContext.PurgeUnlockedResources(false) to release GPU memory from Skia's internal cache. Helpful when accumulation is still observed.",
          "category": "workaround",
          "codeSnippet": "_skiaGlControl.GRContext.PurgeUnlockedResources(false);",
          "confidence": 0.78,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Dispose old image before overwriting",
      "recommendedReason": "Reporter confirmed this is exactly what fixed the leak. Trivial change, zero risk."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.82,
      "reason": "Memory leak caused by user not disposing old SKImage entries before overwriting. GPU memory is not visible to the .NET GC and requires explicit Dispose(). Reporter confirmed the fix in the thread. Behavior is by design (ISKReferenceCounted).",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply or confirm: type/bug, area/SkiaSharp, os/Windows-Classic, backend/OpenGL, tenet/reliability",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/OpenGL",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain GPU disposal pattern, confirm reporter's fix, and add the GRContext.PurgeUnlockedResources tip",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the follow-up and for sharing what fixed it!\n\nThis is a very common gotcha with GPU-backed SkiaSharp objects: the .NET garbage collector cannot see GPU/native memory, so `GC.Collect()` will not free GPU textures. You must explicitly call `Dispose()` on any `SKImage` (or other GPU-backed object) **before** overwriting or abandoning the reference.\n\nThe correct pattern:\n\n```csharp\n// Dispose the old image BEFORE overwriting\n_skiaMain.DisplayImages[plane]?.Dispose();\n_skiaMain.DisplayImages[plane] = surface.Snapshot();\n```\n\nAs an additional tip: if you still see GPU memory growing over time (due to Skia's internal resource cache), you can call `GRContext.PurgeUnlockedResources(false)` periodically to flush GPU resources that are cached but no longer in active use.\n\nClosing as by-design — let us know if you hit anything else!"
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — by-design GPU disposal behaviour; reporter confirmed fix",
        "risk": "medium",
        "confidence": 0.82,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
