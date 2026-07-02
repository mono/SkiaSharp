# Issue Triage Report — #4275

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-07-02T05:28:49Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views (0.93 (93%)) |
| Suggested action | needs-investigation (0.85 (85%)) |

**Issue Summary:** WinUI SKXamlCanvas shows a brief background flash when switching tabs because Invalidate() asynchronously enqueues DoInvalidate at Normal priority, leaving a window where Background is null before the Skia-rendered WriteableBitmap is set.

**Analysis:** The rendering flash occurs because SKXamlCanvas.Invalidate() always enqueues DoInvalidate asynchronously via DispatcherQueue at Normal priority, and FreeBitmap() sets Background=null on Unloaded. When switching tabs, WinUI renders the tab content before the queued DoInvalidate runs, briefly showing the parent element's background through the transparent canvas.

**Recommendations:** **needs-investigation** — Bug is real and confirmed on current version 4.148.0. Root cause is identified in source code. Fix direction is clear (HasThreadAccess check) but needs verification on Windows CI to confirm the flash is eliminated. Reporter provided a complete repro project.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-WinUI |
| Backends | backend/Raster |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a blank WinUI application
2. Add a TabView with an ItemTemplate containing a Grid (Background=Red) with an SKXamlCanvas inside
3. Handle PaintSurface to clear with SKColors.LightGray
4. Add 2+ tabs using the Add button
5. Switch between tabs repeatedly

**Environment:** Visual Studio, Windows, SkiaSharp 4.148.0 (reporter initially listed 3.116.0 but confirmed 4.148.0 in comments)

**Screenshots:**
- https://github.com/user-attachments/assets/500bb4b3-861c-4aaa-8413-f61e165b915f — Flash of red background visible before SKXamlCanvas gray fill renders on tab switch

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | Flash of red background visible before SKXamlCanvas gray fill renders on tab switch |
| Repro quality | complete |
| Target frameworks | net8.0-windows10.0.19041.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 4.148.0, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | likely |
| Relevance reason | The Invalidate() dispatch pattern using DispatcherQueue is confirmed present in current source at source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs line 164. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.72 (72%) |
| Reason | Reporter explicitly states it worked in 2.88.9. The WinUI migration (UWP CoreDispatcher → DispatcherQueue) introduced the async-enqueue pattern that causes the gap. The FreeBitmap() call on Unloaded also now clears Background=null, which was likely different in the 2.88.x UWP code path. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.0 |

## Analysis

### Technical Summary

The rendering flash occurs because SKXamlCanvas.Invalidate() always enqueues DoInvalidate asynchronously via DispatcherQueue at Normal priority, and FreeBitmap() sets Background=null on Unloaded. When switching tabs, WinUI renders the tab content before the queued DoInvalidate runs, briefly showing the parent element's background through the transparent canvas.

### Rationale

The code in SKXamlCanvas.cs line 164 shows Invalidate() unconditionally enqueues DoInvalidate — even when already on the UI thread — creating a frame gap. FreeBitmap() on line 273 explicitly sets Background=null. On tab switch, the Loaded event fires and calls Invalidate(), but the async enqueue means at least one rendered frame exists with Background=null before DoInvalidate sets it to the WriteableBitmap brush. This is a real bug, not a usage error. The reporter's suggestions (HasThreadAccess check and/or higher priority) are valid mitigations.

### Key Signals

- "I experience an initial frame where the canvas isn't rendered, but instead the background quickly flickers" — **issue body** (Classic first-frame rendering gap — DoInvalidate not called synchronously before WinUI presents the first frame.)
- "DispatcherQueue?.TryEnqueue(DispatcherQueuePriority.Normal, DoInvalidate)" — **issue body** (The reporter correctly identified the async dispatch as the root cause. This always queues, even when already on UI thread.)
- "Sorry, I am using 4.148.0. I wasn't able to select a newer version when writing the bug report." — **comment by Mangepange** (Reporter is on 4.148.0, not 3.116.0. Bug is reproducible on current version.)
- "Last Known Good Version: 2.88.9" — **issue body** (Confirms regression in the 3.x generation, likely introduced during WinUI migration from UWP CoreDispatcher to DispatcherQueue API.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 161-168 | direct | Invalidate() unconditionally calls DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, DoInvalidate) without checking HasThreadAccess. This means even when called from the UI thread (e.g., in OnLoaded or OnSizeChanged), the rendering is deferred, creating a frame gap where Background is still null. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 271-277 | direct | FreeBitmap() sets Background=null, brush=null, bitmap=null. Called from OnUnloaded. When a tab is switched back into view, OnLoaded fires and calls Invalidate(), but Background remains null until the queued DoInvalidate executes — exposing the parent container background. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 125-139 | related | OnLoaded calls Invalidate() (via OnXamlRootChanged). This is always an async enqueue — no synchronous path exists for the initial render when a tab becomes visible. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 120-123 | related | OnSizeChanged also calls Invalidate(), also always async. Multiple concurrent queued DoInvalidate calls are possible on tab switch (Loaded + SizeChanged both fire). |

### Workarounds

- Set the Background of the SKXamlCanvas to match the expected render color (e.g., Background=LightGray in XAML) so the flash uses the correct color even before DoInvalidate runs.
- Set the parent Grid Background to Transparent instead of Red to eliminate the visible color difference during the flash window.
- Call canvas.Invalidate() from a higher-priority context by subclassing SKXamlCanvas and overriding OnLoaded to call DoInvalidate synchronously before base.OnLoaded (not directly supported as DoInvalidate is private).

### Next Questions

- Does changing to DispatcherQueuePriority.High eliminate the flash, or only reduce its duration?
- Would adding a HasThreadAccess check (call DoInvalidate directly if on UI thread) fully resolve the issue?
- Does the same flash occur with SKGLSurface or only with the WriteableBitmap-backed SKXamlCanvas?
- Was the 2.88.x UWP path using CoreDispatcher.RunAsync(CoreDispatcherPriority.Normal) — same async pattern or synchronous?

### Resolution Proposals

**Hypothesis:** The fix is to check DispatcherQueue.HasThreadAccess in Invalidate() and call DoInvalidate() directly when already on the UI thread, avoiding the async dispatch frame gap for Loaded/SizeChanged events.

1. **Check HasThreadAccess before enqueuing** — fix, confidence 0.82 (82%), cost/xs, validated=untested
   - In Invalidate(), check DispatcherQueue.HasThreadAccess. If true, call DoInvalidate() directly (synchronous). If false, use TryEnqueue as before. This eliminates the gap for Loaded and SizeChanged which fire on the UI thread.
2. **Workaround: match canvas background to content** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - User can set the Background property of SKXamlCanvas in XAML to match their expected rendered color (e.g., Background="LightGray"). The flash will still occur but will show the correct color, making it invisible.
3. **Use DispatcherQueuePriority.High** — fix, confidence 0.55 (55%), cost/xs, validated=untested
   - Change TryEnqueue priority from Normal to High to reduce the window between the first frame render and DoInvalidate execution. Reduces but may not eliminate the flash.

**Recommended proposal:** Check HasThreadAccess before enqueuing

**Why:** Directly addresses the root cause. HasThreadAccess check is a well-known DispatcherQueue pattern and would make OnLoaded/OnSizeChanged calls synchronous while keeping cross-thread safety for the off-thread case.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.85 (85%) |
| Reason | Bug is real and confirmed on current version 4.148.0. Root cause is identified in source code. Fix direction is clear (HasThreadAccess check) but needs verification on Windows CI to confirm the flash is eliminated. Reporter provided a complete repro project. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.93 (93%) | Add area/SkiaSharp.Views, os/Windows-WinUI, backend/Raster, tenet/reliability labels | labels=type/bug, area/SkiaSharp.Views, os/Windows-WinUI, backend/Raster, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Acknowledge the bug, confirm root cause, provide workaround, and note the fix direction | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and the minimal repro!

You've correctly identified the root cause. `Invalidate()` always enqueues `DoInvalidate` asynchronously via `DispatcherQueue.TryEnqueue(Normal)`, even when called from the UI thread during `Loaded` or `SizeChanged`. This creates a frame window where `Background` is `null` (cleared by `FreeBitmap()` on `Unloaded`), letting the parent element show through.

**Immediate workaround:** Set `Background` on the `SKXamlCanvas` to match your expected rendered color in XAML. The flash will still happen, but it will show the same color as your Skia content:

```xml
<skia:SKXamlCanvas Background="LightGray" PaintSurface="SKXamlCanvas_PaintSurface" />
```

**Fix direction:** Your suggestion of checking `DispatcherQueue.HasThreadAccess` is the right approach — if we're already on the UI thread, call `DoInvalidate()` directly rather than queueing it. This would make `Loaded`/`SizeChanged`-triggered renders synchronous, eliminating the gap.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4275,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-07-02T05:28:49Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "WinUI SKXamlCanvas shows a brief background flash when switching tabs because Invalidate() asynchronously enqueues DoInvalidate at Normal priority, leaving a window where Background is null before the Skia-rendered WriteableBitmap is set.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.93
    },
    "platforms": [
      "os/Windows-WinUI"
    ],
    "backends": [
      "backend/Raster"
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
      "errorMessage": "Flash of red background visible before SKXamlCanvas gray fill renders on tab switch",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net8.0-windows10.0.19041.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a blank WinUI application",
        "Add a TabView with an ItemTemplate containing a Grid (Background=Red) with an SKXamlCanvas inside",
        "Handle PaintSurface to clear with SKColors.LightGray",
        "Add 2+ tabs using the Add button",
        "Switch between tabs repeatedly"
      ],
      "environmentDetails": "Visual Studio, Windows, SkiaSharp 4.148.0 (reporter initially listed 3.116.0 but confirmed 4.148.0 in comments)",
      "screenshots": [
        {
          "url": "https://github.com/user-attachments/assets/500bb4b3-861c-4aaa-8413-f61e165b915f",
          "description": "Flash of red background visible before SKXamlCanvas gray fill renders on tab switch"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "4.148.0",
        "2.88.9"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.116.0",
      "currentRelevance": "likely",
      "relevanceReason": "The Invalidate() dispatch pattern using DispatcherQueue is confirmed present in current source at source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs line 164."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.72,
      "reason": "Reporter explicitly states it worked in 2.88.9. The WinUI migration (UWP CoreDispatcher → DispatcherQueue) introduced the async-enqueue pattern that causes the gap. The FreeBitmap() call on Unloaded also now clears Background=null, which was likely different in the 2.88.x UWP code path.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.0"
    }
  },
  "analysis": {
    "summary": "The rendering flash occurs because SKXamlCanvas.Invalidate() always enqueues DoInvalidate asynchronously via DispatcherQueue at Normal priority, and FreeBitmap() sets Background=null on Unloaded. When switching tabs, WinUI renders the tab content before the queued DoInvalidate runs, briefly showing the parent element's background through the transparent canvas.",
    "rationale": "The code in SKXamlCanvas.cs line 164 shows Invalidate() unconditionally enqueues DoInvalidate — even when already on the UI thread — creating a frame gap. FreeBitmap() on line 273 explicitly sets Background=null. On tab switch, the Loaded event fires and calls Invalidate(), but the async enqueue means at least one rendered frame exists with Background=null before DoInvalidate sets it to the WriteableBitmap brush. This is a real bug, not a usage error. The reporter's suggestions (HasThreadAccess check and/or higher priority) are valid mitigations.",
    "keySignals": [
      {
        "text": "I experience an initial frame where the canvas isn't rendered, but instead the background quickly flickers",
        "source": "issue body",
        "interpretation": "Classic first-frame rendering gap — DoInvalidate not called synchronously before WinUI presents the first frame."
      },
      {
        "text": "DispatcherQueue?.TryEnqueue(DispatcherQueuePriority.Normal, DoInvalidate)",
        "source": "issue body",
        "interpretation": "The reporter correctly identified the async dispatch as the root cause. This always queues, even when already on UI thread."
      },
      {
        "text": "Sorry, I am using 4.148.0. I wasn't able to select a newer version when writing the bug report.",
        "source": "comment by Mangepange",
        "interpretation": "Reporter is on 4.148.0, not 3.116.0. Bug is reproducible on current version."
      },
      {
        "text": "Last Known Good Version: 2.88.9",
        "source": "issue body",
        "interpretation": "Confirms regression in the 3.x generation, likely introduced during WinUI migration from UWP CoreDispatcher to DispatcherQueue API."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "161-168",
        "finding": "Invalidate() unconditionally calls DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, DoInvalidate) without checking HasThreadAccess. This means even when called from the UI thread (e.g., in OnLoaded or OnSizeChanged), the rendering is deferred, creating a frame gap where Background is still null.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "271-277",
        "finding": "FreeBitmap() sets Background=null, brush=null, bitmap=null. Called from OnUnloaded. When a tab is switched back into view, OnLoaded fires and calls Invalidate(), but Background remains null until the queued DoInvalidate executes — exposing the parent container background.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "125-139",
        "finding": "OnLoaded calls Invalidate() (via OnXamlRootChanged). This is always an async enqueue — no synchronous path exists for the initial render when a tab becomes visible.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "120-123",
        "finding": "OnSizeChanged also calls Invalidate(), also always async. Multiple concurrent queued DoInvalidate calls are possible on tab switch (Loaded + SizeChanged both fire).",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Set the Background of the SKXamlCanvas to match the expected render color (e.g., Background=LightGray in XAML) so the flash uses the correct color even before DoInvalidate runs.",
      "Set the parent Grid Background to Transparent instead of Red to eliminate the visible color difference during the flash window.",
      "Call canvas.Invalidate() from a higher-priority context by subclassing SKXamlCanvas and overriding OnLoaded to call DoInvalidate synchronously before base.OnLoaded (not directly supported as DoInvalidate is private)."
    ],
    "nextQuestions": [
      "Does changing to DispatcherQueuePriority.High eliminate the flash, or only reduce its duration?",
      "Would adding a HasThreadAccess check (call DoInvalidate directly if on UI thread) fully resolve the issue?",
      "Does the same flash occur with SKGLSurface or only with the WriteableBitmap-backed SKXamlCanvas?",
      "Was the 2.88.x UWP path using CoreDispatcher.RunAsync(CoreDispatcherPriority.Normal) — same async pattern or synchronous?"
    ],
    "resolution": {
      "hypothesis": "The fix is to check DispatcherQueue.HasThreadAccess in Invalidate() and call DoInvalidate() directly when already on the UI thread, avoiding the async dispatch frame gap for Loaded/SizeChanged events.",
      "proposals": [
        {
          "title": "Check HasThreadAccess before enqueuing",
          "description": "In Invalidate(), check DispatcherQueue.HasThreadAccess. If true, call DoInvalidate() directly (synchronous). If false, use TryEnqueue as before. This eliminates the gap for Loaded and SizeChanged which fire on the UI thread.",
          "category": "fix",
          "confidence": 0.82,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Workaround: match canvas background to content",
          "description": "User can set the Background property of SKXamlCanvas in XAML to match their expected rendered color (e.g., Background=\"LightGray\"). The flash will still occur but will show the correct color, making it invisible.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Use DispatcherQueuePriority.High",
          "description": "Change TryEnqueue priority from Normal to High to reduce the window between the first frame render and DoInvalidate execution. Reduces but may not eliminate the flash.",
          "category": "fix",
          "confidence": 0.55,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Check HasThreadAccess before enqueuing",
      "recommendedReason": "Directly addresses the root cause. HasThreadAccess check is a well-known DispatcherQueue pattern and would make OnLoaded/OnSizeChanged calls synchronous while keeping cross-thread safety for the off-thread case."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "Bug is real and confirmed on current version 4.148.0. Root cause is identified in source code. Fix direction is clear (HasThreadAccess check) but needs verification on Windows CI to confirm the flash is eliminated. Reporter provided a complete repro project.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Add area/SkiaSharp.Views, os/Windows-WinUI, backend/Raster, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.93,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-WinUI",
          "backend/Raster",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the bug, confirm root cause, provide workaround, and note the fix direction",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed report and the minimal repro!\n\nYou've correctly identified the root cause. `Invalidate()` always enqueues `DoInvalidate` asynchronously via `DispatcherQueue.TryEnqueue(Normal)`, even when called from the UI thread during `Loaded` or `SizeChanged`. This creates a frame window where `Background` is `null` (cleared by `FreeBitmap()` on `Unloaded`), letting the parent element show through.\n\n**Immediate workaround:** Set `Background` on the `SKXamlCanvas` to match your expected rendered color in XAML. The flash will still happen, but it will show the same color as your Skia content:\n\n```xml\n<skia:SKXamlCanvas Background=\"LightGray\" PaintSurface=\"SKXamlCanvas_PaintSurface\" />\n```\n\n**Fix direction:** Your suggestion of checking `DispatcherQueue.HasThreadAccess` is the right approach — if we're already on the UI thread, call `DoInvalidate()` directly rather than queueing it. This would make `Loaded`/`SizeChanged`-triggered renders synchronous, eliminating the gap."
      }
    ]
  }
}
```

</details>
