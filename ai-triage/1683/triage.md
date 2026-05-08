# Issue Triage Report — #1683

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T10:02:31Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | needs-investigation (0.80 (80%)) |

**Issue Summary:** SKGLView on iOS causes a GPU resource leak (IOGPUResource accumulation) during continuous high-frequency rendering, growing to ~60,000 leaked resources before freezing the app, while SKCanvasView does not exhibit this problem.

**Analysis:** The iOS SKGLView continuously creates GPU resources via Skia's GRContext during each render frame, but never calls context.PurgeUnlockedResources() to release the GRContext's internal resource cache. Under a CADisplayLink-driven render loop this cache grows without bound until the iOS GPU driver reports IOGPUResource exhaustion and freezes the app.

**Recommendations:** **needs-investigation** — Real bug with strong GPU-driver evidence and multiple users confirming it. However, a 2023 comment suggests possible resolution in newer versions. Needs a minimal current-version repro before deciding on fix vs close-as-fixed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/iOS |
| Backends | backend/OpenGL |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Set up an iOS project using SKGLView
2. Hook up PaintSurface and draw ~30 lines and ~30 circles each frame
3. Trigger continuous redraws via CADisplayLink or repeated touch events
4. After ~1 minute observe IOGPUResource leak messages in device console and eventual app freeze

**Environment:** SkiaSharp 2.80.2, Xamarin.iOS 14.14.2.5, iOS 12.0+ (tested on iPhone 6s and iPad Mini 4), Xcode 12.4, macOS 10.15.7

**Related issues:** #1680

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1680 — Related UWP memory leak with SKCanvas reported around the same time

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | memory-leak |
| Error message | IOReturn IOGPUDevice::new_resource(...): PID 40079 likely leaking IOGPUResource (count=60000) |
| Repro quality | partial |
| Target frameworks | xamarin.ios |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2, 2.88.6 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | Original report is on 2.80.2. A 2023 commenter tested with 2.88.6 on iOS 16.7 and ran 8.5 hours without a crash, suggesting the issue may have been fixed. The current code in the repo still does not call PurgeUnlockedResources, so it is unclear if the fix was behavioral or hardware/OS dependent. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | False |
| Confidence | 0.35 (35%) |
| Reason | A 2023 commenter ran 8.5 hours without crash on 2.88.6/iOS 16.7, but the current iOS SKGLView source still does not call context.PurgeUnlockedResources() after flushing. The absence of a resource-purge call is the likely root cause and it has not been added to the codebase. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

The iOS SKGLView continuously creates GPU resources via Skia's GRContext during each render frame, but never calls context.PurgeUnlockedResources() to release the GRContext's internal resource cache. Under a CADisplayLink-driven render loop this cache grows without bound until the iOS GPU driver reports IOGPUResource exhaustion and freezes the app.

### Rationale

The bug is clearly type/bug: the IOGPUResource leak messages from the iOS kernel driver and profiler screenshots showing unbounded memory growth confirm broken behavior. The area is SkiaSharp.Views because the issue is in the GL view's render loop code, not in user drawing code. The original reporter confirmed the issue is reproducible without SKPath, pointing to the SKGLView GL management code itself. The current source has no resource-purge calls, consistent with the reported behavior.

### Key Signals

- "IOReturn IOGPUDevice::new_resource(...): PID 40079 likely leaking IOGPUResource (count=46000...60000)" — **issue body** (iOS GPU driver directly reporting resource exhaustion — this is a GPU-side resource leak, not a .NET GC issue.)
- "When I run the exact same code with SKCanvasView it can run forever" — **issue body** (SKCanvasView (CPU path) does not use GRContext or GL; the leak is isolated to the GL rendering path in SKGLView.)
- "I believe I could reproduce the memory issue without any SKPath calls too" — **comment (IainS1986, 2021-09-07)** (The leak is not user-code or SKPath specific — it is in SKGLView's own GL resource management.)
- "It ran for 8.5 hrs -- full speed, no slow downs, no crashes. 50 FPS, recreating a 50 pt Polygon Path every single frame. I'm using Skia 2.88.6, on iOS 16.7" — **comment (najak3d, 2023-10-06)** (Possible mitigation in newer SkiaSharp or iOS versions, but cannot confirm a fix was intentionally applied — the code still lacks PurgeUnlockedResources.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKGLView.cs` | 108-165 | direct | DrawInRect calls canvas.Flush() and context.Flush() at end of each frame but never calls context.PurgeUnlockedResources() or sets a resource cache limit. Under high-frequency rendering (CADisplayLink) the GRContext GPU resource cache grows without bound. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKGLView.cs` | 114-117 | related | GRGlInterface.Create() is called as a local variable (not stored as a field) when initializing the GRContext. The C# object becomes eligible for GC immediately, but Skia's C++ GrContext increments the sk_sp refcount on construction so this is safe in practice. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKGLView.razor.cs` | 1-50 | related | Blazor SKGLView also only calls canvas.Flush() and context.Flush() without PurgeUnlockedResources — consistent pattern across GL view implementations, suggesting this was a design omission rather than an iOS-specific regression. |

### Workarounds

- Use SKCanvasView instead of SKGLView on iOS — confirmed workaround by original reporter (CPU rendering path, no GPU resource leak).
- If SKGLView with OpenGL ES is required, call context.PurgeUnlockedResources(false) after context.Flush() in the PaintSurface handler to periodically release the GRContext's GPU resource cache.

### Next Questions

- Is the issue reproducible with a blank SKGLView (no drawing at all) to confirm it is in the GL setup/teardown and not user draw calls?
- Does the issue reproduce in current SkiaSharp (3.x) on modern iOS versions, or was it silently fixed?
- Would calling GRContext.PurgeUnlockedResources(false) after each frame fully resolve the leak, or is there a deeper framebuffer allocation issue in GLKView's multisample setup?

### Resolution Proposals

**Hypothesis:** The GRContext GPU resource cache is not being periodically purged in the iOS SKGLView render loop, causing GPU resources to accumulate each frame until the iOS driver hits its resource limit and freezes the app.

1. **Switch to SKCanvasView** — workaround, confidence 0.95 (95%), cost/s, validated=yes
   - Replace SKGLView with SKCanvasView on iOS. SKCanvasView uses the CPU Raster backend and does not touch GPU resources, eliminating the leak entirely.
2. **Add PurgeUnlockedResources call after each frame** — fix, confidence 0.70 (70%), cost/xs, validated=untested
   - In DrawInRect, after calling context.Flush(), add a call to context.PurgeUnlockedResources(false) to release GPU resources held in the GRContext cache that are not currently in use.
3. **Confirm current version status before fixing** — investigation, confidence 0.85 (85%), cost/s, validated=untested
   - First reproduce the issue on current SkiaSharp (3.x) with a minimal iOS project and a CADisplayLink-driven render loop. If it no longer reproduces on modern iOS/SkiaSharp, close as fixed. If it does, apply the PurgeUnlockedResources fix.

**Recommended proposal:** Confirm current version status before fixing

**Why:** A 2023 commenter could not reproduce the issue on 2.88.6/iOS 16.7. Before investing in a code fix, a current-version repro attempt is needed to determine if this is already resolved. If still present, the PurgeUnlockedResources fix is straightforward.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.80 (80%) |
| Reason | Real bug with strong GPU-driver evidence and multiple users confirming it. However, a 2023 comment suggests possible resolution in newer versions. Needs a minimal current-version repro before deciding on fix vs close-as-fixed. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.93 (93%) | Apply type/bug, area/SkiaSharp.Views, os/iOS, backend/OpenGL, tenet/reliability | labels=type/bug, area/SkiaSharp.Views, os/iOS, backend/OpenGL, tenet/reliability |
| add-comment | medium | 0.80 (80%) | Post analysis with workaround and request for current-version reproduction | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report and profiler screenshots.

**Analysis:** The `IOGPUResource` leak messages from the iOS GPU driver suggest that Skia's `GRContext` is accumulating GPU resources without releasing them. The iOS `SKGLView` render loop calls `canvas.Flush()` and `context.Flush()` each frame but does not call `context.PurgeUnlockedResources()` to release the resource cache — under a CADisplayLink-driven loop this cache can grow until the GPU driver runs out of resource slots.

**Workaround (confirmed):** Switch to `SKCanvasView` on iOS. It uses the CPU raster renderer and has no GPU resource management overhead.

**Potential workaround (untested):** If you need the GL path, try adding a call to `context.PurgeUnlockedResources(false)` after `context.Flush()` in your `PaintSurface` handler to periodically release GPU cache entries.

A [2023 comment](https://github.com/mono/SkiaSharp/issues/1683#issuecomment-1750316461) suggests this may no longer reproduce on SkiaSharp 2.88.6/iOS 16.7 — could anyone with a current-version test confirm whether this still occurs on SkiaSharp 3.x?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1683,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T10:02:31Z"
  },
  "summary": "SKGLView on iOS causes a GPU resource leak (IOGPUResource accumulation) during continuous high-frequency rendering, growing to ~60,000 leaked resources before freezing the app, while SKCanvasView does not exhibit this problem.",
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
      "os/iOS"
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
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "memory-leak",
      "errorMessage": "IOReturn IOGPUDevice::new_resource(...): PID 40079 likely leaking IOGPUResource (count=60000)",
      "reproQuality": "partial",
      "targetFrameworks": [
        "xamarin.ios"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Set up an iOS project using SKGLView",
        "Hook up PaintSurface and draw ~30 lines and ~30 circles each frame",
        "Trigger continuous redraws via CADisplayLink or repeated touch events",
        "After ~1 minute observe IOGPUResource leak messages in device console and eventual app freeze"
      ],
      "environmentDetails": "SkiaSharp 2.80.2, Xamarin.iOS 14.14.2.5, iOS 12.0+ (tested on iPhone 6s and iPad Mini 4), Xcode 12.4, macOS 10.15.7",
      "relatedIssues": [
        1680
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1680",
          "description": "Related UWP memory leak with SKCanvas reported around the same time"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2",
        "2.88.6"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "Original report is on 2.80.2. A 2023 commenter tested with 2.88.6 on iOS 16.7 and ran 8.5 hours without a crash, suggesting the issue may have been fixed. The current code in the repo still does not call PurgeUnlockedResources, so it is unclear if the fix was behavioral or hardware/OS dependent."
    },
    "fixStatus": {
      "likelyFixed": false,
      "confidence": 0.35,
      "reason": "A 2023 commenter ran 8.5 hours without crash on 2.88.6/iOS 16.7, but the current iOS SKGLView source still does not call context.PurgeUnlockedResources() after flushing. The absence of a resource-purge call is the likely root cause and it has not been added to the codebase."
    }
  },
  "analysis": {
    "summary": "The iOS SKGLView continuously creates GPU resources via Skia's GRContext during each render frame, but never calls context.PurgeUnlockedResources() to release the GRContext's internal resource cache. Under a CADisplayLink-driven render loop this cache grows without bound until the iOS GPU driver reports IOGPUResource exhaustion and freezes the app.",
    "rationale": "The bug is clearly type/bug: the IOGPUResource leak messages from the iOS kernel driver and profiler screenshots showing unbounded memory growth confirm broken behavior. The area is SkiaSharp.Views because the issue is in the GL view's render loop code, not in user drawing code. The original reporter confirmed the issue is reproducible without SKPath, pointing to the SKGLView GL management code itself. The current source has no resource-purge calls, consistent with the reported behavior.",
    "keySignals": [
      {
        "text": "IOReturn IOGPUDevice::new_resource(...): PID 40079 likely leaking IOGPUResource (count=46000...60000)",
        "source": "issue body",
        "interpretation": "iOS GPU driver directly reporting resource exhaustion — this is a GPU-side resource leak, not a .NET GC issue."
      },
      {
        "text": "When I run the exact same code with SKCanvasView it can run forever",
        "source": "issue body",
        "interpretation": "SKCanvasView (CPU path) does not use GRContext or GL; the leak is isolated to the GL rendering path in SKGLView."
      },
      {
        "text": "I believe I could reproduce the memory issue without any SKPath calls too",
        "source": "comment (IainS1986, 2021-09-07)",
        "interpretation": "The leak is not user-code or SKPath specific — it is in SKGLView's own GL resource management."
      },
      {
        "text": "It ran for 8.5 hrs -- full speed, no slow downs, no crashes. 50 FPS, recreating a 50 pt Polygon Path every single frame. I'm using Skia 2.88.6, on iOS 16.7",
        "source": "comment (najak3d, 2023-10-06)",
        "interpretation": "Possible mitigation in newer SkiaSharp or iOS versions, but cannot confirm a fix was intentionally applied — the code still lacks PurgeUnlockedResources."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKGLView.cs",
        "lines": "108-165",
        "finding": "DrawInRect calls canvas.Flush() and context.Flush() at end of each frame but never calls context.PurgeUnlockedResources() or sets a resource cache limit. Under high-frequency rendering (CADisplayLink) the GRContext GPU resource cache grows without bound.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKGLView.cs",
        "lines": "114-117",
        "finding": "GRGlInterface.Create() is called as a local variable (not stored as a field) when initializing the GRContext. The C# object becomes eligible for GC immediately, but Skia's C++ GrContext increments the sk_sp refcount on construction so this is safe in practice.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKGLView.razor.cs",
        "lines": "1-50",
        "finding": "Blazor SKGLView also only calls canvas.Flush() and context.Flush() without PurgeUnlockedResources — consistent pattern across GL view implementations, suggesting this was a design omission rather than an iOS-specific regression.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use SKCanvasView instead of SKGLView on iOS — confirmed workaround by original reporter (CPU rendering path, no GPU resource leak).",
      "If SKGLView with OpenGL ES is required, call context.PurgeUnlockedResources(false) after context.Flush() in the PaintSurface handler to periodically release the GRContext's GPU resource cache."
    ],
    "nextQuestions": [
      "Is the issue reproducible with a blank SKGLView (no drawing at all) to confirm it is in the GL setup/teardown and not user draw calls?",
      "Does the issue reproduce in current SkiaSharp (3.x) on modern iOS versions, or was it silently fixed?",
      "Would calling GRContext.PurgeUnlockedResources(false) after each frame fully resolve the leak, or is there a deeper framebuffer allocation issue in GLKView's multisample setup?"
    ],
    "resolution": {
      "hypothesis": "The GRContext GPU resource cache is not being periodically purged in the iOS SKGLView render loop, causing GPU resources to accumulate each frame until the iOS driver hits its resource limit and freezes the app.",
      "proposals": [
        {
          "title": "Switch to SKCanvasView",
          "description": "Replace SKGLView with SKCanvasView on iOS. SKCanvasView uses the CPU Raster backend and does not touch GPU resources, eliminating the leak entirely.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/s",
          "validated": "yes"
        },
        {
          "title": "Add PurgeUnlockedResources call after each frame",
          "description": "In DrawInRect, after calling context.Flush(), add a call to context.PurgeUnlockedResources(false) to release GPU resources held in the GRContext cache that are not currently in use.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Confirm current version status before fixing",
          "description": "First reproduce the issue on current SkiaSharp (3.x) with a minimal iOS project and a CADisplayLink-driven render loop. If it no longer reproduces on modern iOS/SkiaSharp, close as fixed. If it does, apply the PurgeUnlockedResources fix.",
          "category": "investigation",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Confirm current version status before fixing",
      "recommendedReason": "A 2023 commenter could not reproduce the issue on 2.88.6/iOS 16.7. Before investing in a code fix, a current-version repro attempt is needed to determine if this is already resolved. If still present, the PurgeUnlockedResources fix is straightforward."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.8,
      "reason": "Real bug with strong GPU-driver evidence and multiple users confirming it. However, a 2023 comment suggests possible resolution in newer versions. Needs a minimal current-version repro before deciding on fix vs close-as-fixed.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views, os/iOS, backend/OpenGL, tenet/reliability",
        "risk": "low",
        "confidence": 0.93,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/iOS",
          "backend/OpenGL",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis with workaround and request for current-version reproduction",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thank you for the detailed report and profiler screenshots.\n\n**Analysis:** The `IOGPUResource` leak messages from the iOS GPU driver suggest that Skia's `GRContext` is accumulating GPU resources without releasing them. The iOS `SKGLView` render loop calls `canvas.Flush()` and `context.Flush()` each frame but does not call `context.PurgeUnlockedResources()` to release the resource cache — under a CADisplayLink-driven loop this cache can grow until the GPU driver runs out of resource slots.\n\n**Workaround (confirmed):** Switch to `SKCanvasView` on iOS. It uses the CPU raster renderer and has no GPU resource management overhead.\n\n**Potential workaround (untested):** If you need the GL path, try adding a call to `context.PurgeUnlockedResources(false)` after `context.Flush()` in your `PaintSurface` handler to periodically release GPU cache entries.\n\nA [2023 comment](https://github.com/mono/SkiaSharp/issues/1683#issuecomment-1750316461) suggests this may no longer reproduce on SkiaSharp 2.88.6/iOS 16.7 — could anyone with a current-version test confirm whether this still occurs on SkiaSharp 3.x?"
      }
    ]
  }
}
```

</details>
