# Issue Triage Report — #4277

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-06-30T05:36:40Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views (0.95 (95%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** SKSwapChainPanel on WinUI crashes with error 0xc0000409 (STATUS_STACK_BUFFER_OVERRUN) when rapidly resizing the window and then closing it, specifically when DrawInBackground=true and EnableRenderLoop=true due to a race condition between the background render thread and context teardown on unload.

**Analysis:** Race condition in AngleSwapChainPanel: when DrawInBackground=true the render loop and one-shot render workers execute EGL calls on background threads, but OnUnloaded cancels the loop action and destroys the GlesContext without waiting for those threads to finish. Rapid resize events can queue multiple renderOnceWorker calls that run concurrently with context disposal, causing a use-after-free of EGL resources which Windows reports as a stack buffer overrun.

**Recommendations:** **needs-investigation** — Race condition is confirmed by code analysis and reporter's observation. Root cause is clear but a thread-safe fix requires careful rework of the background render loop teardown sequence in AngleSwapChainPanel.

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

1. Create a WinUI app using SKSwapChainPanel with DrawInBackground=true and EnableRenderLoop=true
2. Rapidly and repeatedly resize the application window
3. Close the window immediately after resizing

**Environment:** SkiaSharp.Views.WinUI 4.148.0, Visual Studio on Windows

**Related issues:** #1490

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | Application crashed with error code 0xc0000409 (STATUS_STACK_BUFFER_OVERRUN) |
| Repro quality | partial |
| Target frameworks | net8.0-windows |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 4.148.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The race-condition code path in AngleSwapChainPanel has not changed since this version was released — UpdateRenderLoop still cancels without awaiting and OnUnloaded does not join background threads. |

## Analysis

### Technical Summary

Race condition in AngleSwapChainPanel: when DrawInBackground=true the render loop and one-shot render workers execute EGL calls on background threads, but OnUnloaded cancels the loop action and destroys the GlesContext without waiting for those threads to finish. Rapid resize events can queue multiple renderOnceWorker calls that run concurrently with context disposal, causing a use-after-free of EGL resources which Windows reports as a stack buffer overrun.

### Rationale

The crash is triggered by a clear threading hazard: UpdateRenderLoop cancels the IAsyncAction but does not wait for the background thread to exit before DestroyRenderSurface and glesContext.Dispose run on the UI thread. Additionally renderOnceWorker is never cancelled or joined in OnUnloaded. Setting DrawInBackground=false routes rendering through the UI thread (DispatcherQueue), which serializes access and eliminates the race — consistent with the reporter's observation.

### Key Signals

- "error code:0xc0000409" — **issue body** (STATUS_STACK_BUFFER_OVERRUN — typically a Windows security cookie violation triggered by native memory corruption (use-after-free of EGL resources).)
- "maybe related to DrawInBackground=true, if set false, it seems be ok" — **comment #1** (Confirms the race: DrawInBackground=false routes frames through the dispatcher (serialized), DrawInBackground=true runs frames on thread pool (unguarded teardown).)
- "SKSwapChainPanel DrawInBackground="true" EnableRenderLoop="true"" — **XAML in issue body** (Both DrawInBackground and EnableRenderLoop are true — rendering runs entirely on background threads with no synchronization against context disposal.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs` | 137-151 | direct | OnUnloaded calls UpdateRenderLoop(false), DestroyRenderSurface(), then glesContext.Dispose() in sequence without waiting for background threads to complete. The Cancel() call on the IAsyncAction is non-blocking. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs` | 241-262 | direct | UpdateRenderLoop(false) calls renderLoopWorker.Cancel() and sets it to null, but never awaits termination. The background RenderLoop thread can still be executing when the caller proceeds to destroy the context. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs` | 96-113 | direct | Invalidate() with DrawInBackground=true spawns a new ThreadPool.RunAsync(RenderOnce) worker if none is running. Rapid resize events can queue multiple such workers. OnUnloaded does not cancel or await renderOnceWorker at all. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs` | 264-282 | direct | RenderOnce runs RenderFrame() directly on the background thread when DrawInBackground=true. After completion it nulls out renderOnceWorker inside a lock, but this happens after EGL calls that can race with OnUnloaded disposing glesContext. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/GlesContext.cs` | 127-134 | direct | DestroySurface() calls eglDestroySurface() immediately. If a background thread is mid-way through MakeCurrent() or SwapBuffers() using the same surface, this produces a use-after-free at the native layer. |

### Workarounds

- Set DrawInBackground="false" on SKSwapChainPanel. Rendering will execute on the UI thread via the dispatcher, eliminating the race. This may reduce frame rate under heavy resize load but is safe.
- Disable EnableRenderLoop and call Invalidate() manually from the UI thread only, keeping DrawInBackground=false.

### Next Questions

- Does the same crash occur with EnableRenderLoop=false (only Invalidate-driven renders)?
- Is there a minimal repro project that reliably triggers the crash, to aid a fix?
- Does the crash also affect SKGLView in WinUI MAUI (which shares the same AngleSwapChainPanel base)?

### Resolution Proposals

**Hypothesis:** Background render threads access EGL surfaces concurrently with context disposal in OnUnloaded. Adding a cancellation token and a Task-based join (or a ManualResetEventSlim) to synchronize OnUnloaded with all in-flight background render workers before destroying the context would fix the crash.

1. **Set DrawInBackground=false as a workaround** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Use DrawInBackground=false on SKSwapChainPanel. Rendering runs on the UI thread via DispatcherQueue and is safe against concurrent disposal. Performance may be lower but correctness is guaranteed.
2. **Synchronize OnUnloaded with background render threads** — fix, confidence 0.80 (80%), cost/m, validated=untested
   - Add a CancellationTokenSource and awaitable completion signal (e.g., SemaphoreSlim or ManualResetEventSlim) to AngleSwapChainPanel. Set the token in OnUnloaded and wait for in-flight RenderLoop/RenderOnce workers to exit before calling DestroyRenderSurface() and glesContext.Dispose(). Also cancel renderOnceWorker in OnUnloaded.

**Recommended proposal:** Set DrawInBackground=false as a workaround

**Why:** Immediate, zero-code workaround the reporter can apply now. The proper fix requires synchronization rework in AngleSwapChainPanel.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Race condition is confirmed by code analysis and reporter's observation. Root cause is clear but a thread-safe fix requires careful rework of the background render loop teardown sequence in AngleSwapChainPanel. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, views, WinUI, OpenGL, and reliability labels | labels=type/bug, area/SkiaSharp.Views, os/Windows-WinUI, backend/OpenGL, tenet/reliability |
| link-related | low | 0.88 (88%) | Cross-reference issue #1490 — same DrawInBackground race condition on UWP/WinUI | linkedIssue=#1490 |
| add-comment | medium | 0.88 (88%) | Post analysis with confirmed workaround and root cause explanation | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report and the helpful narrowing clue about `DrawInBackground`.

**Root cause:** This is a race condition in `AngleSwapChainPanel`. When `DrawInBackground="true"` and `EnableRenderLoop="true"`, frames are rendered on a background thread pool thread. When the window is closed, `OnUnloaded` cancels the render loop `IAsyncAction` but does not *wait* for the background thread to finish before destroying the GLES context (`glesContext.Dispose()` / `DestroyRenderSurface()`). Rapid resize events also queue one-shot `renderOnceWorker` tasks that are never cancelled in `OnUnloaded`. When the background thread tries to call `MakeCurrent()` or `SwapBuffers()` on the already-destroyed EGL surface, Windows reports the resulting native memory corruption as `0xc0000409`.

**Workaround (immediate):** Set `DrawInBackground="false"` on the panel. Rendering will be dispatched to the UI thread, which serializes access to the EGL context and eliminates the race.

```xaml
<skia:SKSwapChainPanel DrawInBackground="false" EnableRenderLoop="true" />
```

This is consistent with your observation. There will be no crash, though frame delivery under extreme resize load may be slightly slower.

**Related issue:** This is the same underlying race as #1490 (AccessViolationException in UWP with `DrawInBackground=true`).

A proper fix requires synchronizing `OnUnloaded` with all in-flight background render workers before context teardown — this needs investigation into the best synchronization primitive to use without introducing deadlocks.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4277,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-06-30T05:36:40Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKSwapChainPanel on WinUI crashes with error 0xc0000409 (STATUS_STACK_BUFFER_OVERRUN) when rapidly resizing the window and then closing it, specifically when DrawInBackground=true and EnableRenderLoop=true due to a race condition between the background render thread and context teardown on unload.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.95
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
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "Application crashed with error code 0xc0000409 (STATUS_STACK_BUFFER_OVERRUN)",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0-windows"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a WinUI app using SKSwapChainPanel with DrawInBackground=true and EnableRenderLoop=true",
        "Rapidly and repeatedly resize the application window",
        "Close the window immediately after resizing"
      ],
      "environmentDetails": "SkiaSharp.Views.WinUI 4.148.0, Visual Studio on Windows",
      "relatedIssues": [
        1490
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "4.148.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The race-condition code path in AngleSwapChainPanel has not changed since this version was released — UpdateRenderLoop still cancels without awaiting and OnUnloaded does not join background threads."
    }
  },
  "analysis": {
    "summary": "Race condition in AngleSwapChainPanel: when DrawInBackground=true the render loop and one-shot render workers execute EGL calls on background threads, but OnUnloaded cancels the loop action and destroys the GlesContext without waiting for those threads to finish. Rapid resize events can queue multiple renderOnceWorker calls that run concurrently with context disposal, causing a use-after-free of EGL resources which Windows reports as a stack buffer overrun.",
    "rationale": "The crash is triggered by a clear threading hazard: UpdateRenderLoop cancels the IAsyncAction but does not wait for the background thread to exit before DestroyRenderSurface and glesContext.Dispose run on the UI thread. Additionally renderOnceWorker is never cancelled or joined in OnUnloaded. Setting DrawInBackground=false routes rendering through the UI thread (DispatcherQueue), which serializes access and eliminates the race — consistent with the reporter's observation.",
    "keySignals": [
      {
        "text": "error code:0xc0000409",
        "source": "issue body",
        "interpretation": "STATUS_STACK_BUFFER_OVERRUN — typically a Windows security cookie violation triggered by native memory corruption (use-after-free of EGL resources)."
      },
      {
        "text": "maybe related to DrawInBackground=true, if set false, it seems be ok",
        "source": "comment #1",
        "interpretation": "Confirms the race: DrawInBackground=false routes frames through the dispatcher (serialized), DrawInBackground=true runs frames on thread pool (unguarded teardown)."
      },
      {
        "text": "SKSwapChainPanel DrawInBackground=\"true\" EnableRenderLoop=\"true\"",
        "source": "XAML in issue body",
        "interpretation": "Both DrawInBackground and EnableRenderLoop are true — rendering runs entirely on background threads with no synchronization against context disposal."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs",
        "lines": "137-151",
        "finding": "OnUnloaded calls UpdateRenderLoop(false), DestroyRenderSurface(), then glesContext.Dispose() in sequence without waiting for background threads to complete. The Cancel() call on the IAsyncAction is non-blocking.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs",
        "lines": "241-262",
        "finding": "UpdateRenderLoop(false) calls renderLoopWorker.Cancel() and sets it to null, but never awaits termination. The background RenderLoop thread can still be executing when the caller proceeds to destroy the context.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs",
        "lines": "96-113",
        "finding": "Invalidate() with DrawInBackground=true spawns a new ThreadPool.RunAsync(RenderOnce) worker if none is running. Rapid resize events can queue multiple such workers. OnUnloaded does not cancel or await renderOnceWorker at all.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs",
        "lines": "264-282",
        "finding": "RenderOnce runs RenderFrame() directly on the background thread when DrawInBackground=true. After completion it nulls out renderOnceWorker inside a lock, but this happens after EGL calls that can race with OnUnloaded disposing glesContext.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/GlesContext.cs",
        "lines": "127-134",
        "finding": "DestroySurface() calls eglDestroySurface() immediately. If a background thread is mid-way through MakeCurrent() or SwapBuffers() using the same surface, this produces a use-after-free at the native layer.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Set DrawInBackground=\"false\" on SKSwapChainPanel. Rendering will execute on the UI thread via the dispatcher, eliminating the race. This may reduce frame rate under heavy resize load but is safe.",
      "Disable EnableRenderLoop and call Invalidate() manually from the UI thread only, keeping DrawInBackground=false."
    ],
    "nextQuestions": [
      "Does the same crash occur with EnableRenderLoop=false (only Invalidate-driven renders)?",
      "Is there a minimal repro project that reliably triggers the crash, to aid a fix?",
      "Does the crash also affect SKGLView in WinUI MAUI (which shares the same AngleSwapChainPanel base)?"
    ],
    "resolution": {
      "hypothesis": "Background render threads access EGL surfaces concurrently with context disposal in OnUnloaded. Adding a cancellation token and a Task-based join (or a ManualResetEventSlim) to synchronize OnUnloaded with all in-flight background render workers before destroying the context would fix the crash.",
      "proposals": [
        {
          "title": "Set DrawInBackground=false as a workaround",
          "description": "Use DrawInBackground=false on SKSwapChainPanel. Rendering runs on the UI thread via DispatcherQueue and is safe against concurrent disposal. Performance may be lower but correctness is guaranteed.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Synchronize OnUnloaded with background render threads",
          "description": "Add a CancellationTokenSource and awaitable completion signal (e.g., SemaphoreSlim or ManualResetEventSlim) to AngleSwapChainPanel. Set the token in OnUnloaded and wait for in-flight RenderLoop/RenderOnce workers to exit before calling DestroyRenderSurface() and glesContext.Dispose(). Also cancel renderOnceWorker in OnUnloaded.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Set DrawInBackground=false as a workaround",
      "recommendedReason": "Immediate, zero-code workaround the reporter can apply now. The proper fix requires synchronization rework in AngleSwapChainPanel."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Race condition is confirmed by code analysis and reporter's observation. Root cause is clear but a thread-safe fix requires careful rework of the background render loop teardown sequence in AngleSwapChainPanel.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views, WinUI, OpenGL, and reliability labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-WinUI",
          "backend/OpenGL",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference issue #1490 — same DrawInBackground race condition on UWP/WinUI",
        "risk": "low",
        "confidence": 0.88,
        "linkedIssue": 1490
      },
      {
        "type": "add-comment",
        "description": "Post analysis with confirmed workaround and root cause explanation",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thank you for the detailed report and the helpful narrowing clue about `DrawInBackground`.\n\n**Root cause:** This is a race condition in `AngleSwapChainPanel`. When `DrawInBackground=\"true\"` and `EnableRenderLoop=\"true\"`, frames are rendered on a background thread pool thread. When the window is closed, `OnUnloaded` cancels the render loop `IAsyncAction` but does not *wait* for the background thread to finish before destroying the GLES context (`glesContext.Dispose()` / `DestroyRenderSurface()`). Rapid resize events also queue one-shot `renderOnceWorker` tasks that are never cancelled in `OnUnloaded`. When the background thread tries to call `MakeCurrent()` or `SwapBuffers()` on the already-destroyed EGL surface, Windows reports the resulting native memory corruption as `0xc0000409`.\n\n**Workaround (immediate):** Set `DrawInBackground=\"false\"` on the panel. Rendering will be dispatched to the UI thread, which serializes access to the EGL context and eliminates the race.\n\n```xaml\n<skia:SKSwapChainPanel DrawInBackground=\"false\" EnableRenderLoop=\"true\" />\n```\n\nThis is consistent with your observation. There will be no crash, though frame delivery under extreme resize load may be slightly slower.\n\n**Related issue:** This is the same underlying race as #1490 (AccessViolationException in UWP with `DrawInBackground=true`).\n\nA proper fix requires synchronizing `OnUnloaded` with all in-flight background render workers before context teardown — this needs investigation into the best synchronization primitive to use without introducing deadlocks."
      }
    ]
  }
}
```

</details>
