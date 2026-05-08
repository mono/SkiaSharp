# Issue Triage Report — #2337

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T08:03:28Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp.Views (0.92 (92%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** SKGLView with DrawInBackground=true on UWP stops rendering after prolonged runtime (>8 hours) because RenderOnce does not use try-finally to clear renderOnceWorker, and there is no EGL recovery when SwapBuffers fails.

**Analysis:** When SwapBuffers fails after an EGL/D3D device-lost event, the comment in AngleSwapChainPanel.RenderFrame acknowledges reinitialisation is required but no recovery code exists. On the next Invalidate call, MakeCurrent may throw an exception that propagates out of RenderOnce before the lock block that sets renderOnceWorker = null, leaving the field permanently non-null. Future Invalidate calls see a non-null renderOnceWorker and never dispatch a new render worker, freezing rendering indefinitely.

**Recommendations:** **needs-investigation** — Root cause is identified in source (missing try-finally + no EGL recovery), but the issue requires a Windows/UWP environment to reproduce and validate the fix. The fix is straightforward but needs testing against the exact hardware failure scenario.

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

1. Create a Xamarin.Forms UWP app with a custom SKGLViewRenderer that sets DrawInBackground=true
2. Run the app with continuously updating signal data for more than 8 hours
3. Observe that the render thread stops — draw callback is never called again

**Environment:** SkiaSharp 2.88.0-preview.178, Visual Studio 2019, Windows 10 1809 (10.0.17763.0)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | missing-output |
| Error message | Draw callback is not called anymore after >8 hours of runtime |
| Repro quality | partial |
| Target frameworks | uap10.0.17763 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.0-preview.178 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The AngleSwapChainPanel.RenderOnce method in the current WinUI codebase still lacks a try-finally guard around renderOnceWorker cleanup, and SwapBuffers failure still has no recovery logic. |

## Analysis

### Technical Summary

When SwapBuffers fails after an EGL/D3D device-lost event, the comment in AngleSwapChainPanel.RenderFrame acknowledges reinitialisation is required but no recovery code exists. On the next Invalidate call, MakeCurrent may throw an exception that propagates out of RenderOnce before the lock block that sets renderOnceWorker = null, leaving the field permanently non-null. Future Invalidate calls see a non-null renderOnceWorker and never dispatch a new render worker, freezing rendering indefinitely.

### Rationale

The bug has two related root causes found in source: (1) missing try-finally in RenderOnce means an unhandled exception permanently blocks future renders, and (2) the SwapBuffers failure handler is an empty comment stub with no EGL recovery. Both are confirmed by reading AngleSwapChainPanel.cs and GlesContext.cs.

### Key Signals

- "No exception is thrown, the draw method is simply not called anymore" — **issue body** (The exception is silently swallowed by the ThreadPool IAsyncAction; the render pipeline is stuck because renderOnceWorker is never cleared.)
- "I am wondering if glesContext.SwapBuffers() might fail at some point after the application was running for more than 8 hours" — **issue body** (Correct hypothesis — EGL/D3D device loss after sustained runtime causes SwapBuffers to fail, which is unhandled.)
- "Might investigate what happens if GRContext becomes invalid when drawing with DrawInBackground" — **comment by contributor taublast (2025-06-09)** (Corroborates the device-loss / context-invalidation hypothesis.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs` | 264-282 | direct | RenderOnce sets renderOnceWorker = null inside a lock block, but there is no try-finally. If RenderFrame throws (e.g. MakeCurrent throws after EGL context loss), the lock is never reached and renderOnceWorker remains non-null indefinitely, preventing future Invalidate calls from queuing new renders. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs` | 213-238 | direct | RenderFrame calls glesContext.SwapBuffers() and on false return the comment says 'we must reinitialize EGL and the GL resources' but the block is empty — no recovery is attempted. The broken EGL state persists until the view is unloaded. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/GlesContext.cs` | 136-142 | direct | MakeCurrent throws an Exception if eglMakeCurrent returns EGL_FALSE. After a SwapBuffers failure, the EGL context is likely invalid and a subsequent MakeCurrent call will throw, propagating the exception through RenderFrame and RenderOnce. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/GlesContext.cs` | 144-147 | related | SwapBuffers simply calls eglSwapBuffers and returns bool; it does not inspect or surface the EGL error code (e.g. EGL_CONTEXT_LOST), making recovery harder for callers. |

### Workarounds

- Override OnPaintSurface/SKGLViewRenderer and detect when drawing has stopped; call Invalidate() manually on a timer to force a new render cycle.
- Wrap the render loop in a try-catch and handle re-initialization of EGL manually via the Reset() method on GlesContext.

### Next Questions

- Does the same freeze occur with EnableRenderLoop=true (uses RenderLoop instead of RenderOnce)?
- What EGL error code is returned when SwapBuffers fails — EGL_CONTEXT_LOST, EGL_BAD_SURFACE, or another?
- Does the issue reproduce on WinUI 3 (current Windows SDK) or only on UWP Xamarin.Forms?

### Resolution Proposals

**Hypothesis:** EGL/D3D device loss after long runtime causes SwapBuffers to fail; the missing try-finally in RenderOnce then permanently blocks future render dispatch.

1. **Add try-finally to RenderOnce to guarantee renderOnceWorker cleanup** — fix, confidence 0.85 (85%), cost/xs, validated=untested
   - Wrap the body of RenderOnce in a try-finally so that renderOnceWorker is always set to null, even when RenderFrame throws. This prevents the permanent freeze.
2. **Implement EGL recovery after SwapBuffers failure** — fix, confidence 0.75 (75%), cost/m, validated=untested
   - When SwapBuffers returns false, query the EGL error code and reinitialize the context (call DestroyRenderSurface, then glesContext.Reset(), then EnsureRenderSurface). This addresses the root cause comment already present in the code.

**Recommended proposal:** Add try-finally to RenderOnce to guarantee renderOnceWorker cleanup

**Why:** Smallest, safest change that prevents the permanent freeze. The EGL recovery can be addressed in a follow-up as a separate improvement.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Root cause is identified in source (missing try-finally + no EGL recovery), but the issue requires a Windows/UWP environment to reproduce and validate the fix. The fix is straightforward but needs testing against the exact hardware failure scenario. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, views, UWP, OpenGL, reliability labels | labels=type/bug, area/SkiaSharp.Views, os/Windows-Universal-UWP, backend/OpenGL, tenet/reliability |
| add-comment | medium | 0.82 (82%) | Post analysis confirming hypothesis and explaining two root causes | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed investigation and for pointing at `AngleSwapChainPanel.cs`.

After looking at the current source, there are two related issues:

1. **No EGL recovery when `SwapBuffers` fails.** The comment in `RenderFrame` already says *"we must reinitialize EGL and the GL resources"* but the block is empty — no recovery happens. After a long runtime, a D3D/EGL device-lost event can cause `SwapBuffers` to return `false`, leaving the context in a broken state.

2. **Missing try-finally in `RenderOnce`.** If `MakeCurrent()` subsequently throws an exception (which it will when the EGL context is invalid), the exception propagates out of `RenderOnce` before the `lock` block that clears `renderOnceWorker`. Because `Invalidate()` only queues a new background render when `renderOnceWorker == null`, the render thread is permanently blocked.

A minimal fix is to wrap `RenderOnce` with try-finally to guarantee cleanup. A fuller fix would add EGL reinitialization after `SwapBuffers` failure. We'll look at addressing both.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2337,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T08:03:28Z"
  },
  "summary": "SKGLView with DrawInBackground=true on UWP stops rendering after prolonged runtime (>8 hours) because RenderOnce does not use try-finally to clear renderOnceWorker, and there is no EGL recovery when SwapBuffers fails.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.92
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
      "errorType": "missing-output",
      "errorMessage": "Draw callback is not called anymore after >8 hours of runtime",
      "reproQuality": "partial",
      "targetFrameworks": [
        "uap10.0.17763"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Xamarin.Forms UWP app with a custom SKGLViewRenderer that sets DrawInBackground=true",
        "Run the app with continuously updating signal data for more than 8 hours",
        "Observe that the render thread stops — draw callback is never called again"
      ],
      "environmentDetails": "SkiaSharp 2.88.0-preview.178, Visual Studio 2019, Windows 10 1809 (10.0.17763.0)"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.0-preview.178"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The AngleSwapChainPanel.RenderOnce method in the current WinUI codebase still lacks a try-finally guard around renderOnceWorker cleanup, and SwapBuffers failure still has no recovery logic."
    }
  },
  "analysis": {
    "summary": "When SwapBuffers fails after an EGL/D3D device-lost event, the comment in AngleSwapChainPanel.RenderFrame acknowledges reinitialisation is required but no recovery code exists. On the next Invalidate call, MakeCurrent may throw an exception that propagates out of RenderOnce before the lock block that sets renderOnceWorker = null, leaving the field permanently non-null. Future Invalidate calls see a non-null renderOnceWorker and never dispatch a new render worker, freezing rendering indefinitely.",
    "rationale": "The bug has two related root causes found in source: (1) missing try-finally in RenderOnce means an unhandled exception permanently blocks future renders, and (2) the SwapBuffers failure handler is an empty comment stub with no EGL recovery. Both are confirmed by reading AngleSwapChainPanel.cs and GlesContext.cs.",
    "keySignals": [
      {
        "text": "No exception is thrown, the draw method is simply not called anymore",
        "source": "issue body",
        "interpretation": "The exception is silently swallowed by the ThreadPool IAsyncAction; the render pipeline is stuck because renderOnceWorker is never cleared."
      },
      {
        "text": "I am wondering if glesContext.SwapBuffers() might fail at some point after the application was running for more than 8 hours",
        "source": "issue body",
        "interpretation": "Correct hypothesis — EGL/D3D device loss after sustained runtime causes SwapBuffers to fail, which is unhandled."
      },
      {
        "text": "Might investigate what happens if GRContext becomes invalid when drawing with DrawInBackground",
        "source": "comment by contributor taublast (2025-06-09)",
        "interpretation": "Corroborates the device-loss / context-invalidation hypothesis."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs",
        "lines": "264-282",
        "finding": "RenderOnce sets renderOnceWorker = null inside a lock block, but there is no try-finally. If RenderFrame throws (e.g. MakeCurrent throws after EGL context loss), the lock is never reached and renderOnceWorker remains non-null indefinitely, preventing future Invalidate calls from queuing new renders.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs",
        "lines": "213-238",
        "finding": "RenderFrame calls glesContext.SwapBuffers() and on false return the comment says 'we must reinitialize EGL and the GL resources' but the block is empty — no recovery is attempted. The broken EGL state persists until the view is unloaded.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/GlesContext.cs",
        "lines": "136-142",
        "finding": "MakeCurrent throws an Exception if eglMakeCurrent returns EGL_FALSE. After a SwapBuffers failure, the EGL context is likely invalid and a subsequent MakeCurrent call will throw, propagating the exception through RenderFrame and RenderOnce.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/GlesContext.cs",
        "lines": "144-147",
        "finding": "SwapBuffers simply calls eglSwapBuffers and returns bool; it does not inspect or surface the EGL error code (e.g. EGL_CONTEXT_LOST), making recovery harder for callers.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Does the same freeze occur with EnableRenderLoop=true (uses RenderLoop instead of RenderOnce)?",
      "What EGL error code is returned when SwapBuffers fails — EGL_CONTEXT_LOST, EGL_BAD_SURFACE, or another?",
      "Does the issue reproduce on WinUI 3 (current Windows SDK) or only on UWP Xamarin.Forms?"
    ],
    "workarounds": [
      "Override OnPaintSurface/SKGLViewRenderer and detect when drawing has stopped; call Invalidate() manually on a timer to force a new render cycle.",
      "Wrap the render loop in a try-catch and handle re-initialization of EGL manually via the Reset() method on GlesContext."
    ],
    "resolution": {
      "hypothesis": "EGL/D3D device loss after long runtime causes SwapBuffers to fail; the missing try-finally in RenderOnce then permanently blocks future render dispatch.",
      "proposals": [
        {
          "title": "Add try-finally to RenderOnce to guarantee renderOnceWorker cleanup",
          "description": "Wrap the body of RenderOnce in a try-finally so that renderOnceWorker is always set to null, even when RenderFrame throws. This prevents the permanent freeze.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Implement EGL recovery after SwapBuffers failure",
          "description": "When SwapBuffers returns false, query the EGL error code and reinitialize the context (call DestroyRenderSurface, then glesContext.Reset(), then EnsureRenderSurface). This addresses the root cause comment already present in the code.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add try-finally to RenderOnce to guarantee renderOnceWorker cleanup",
      "recommendedReason": "Smallest, safest change that prevents the permanent freeze. The EGL recovery can be addressed in a follow-up as a separate improvement."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Root cause is identified in source (missing try-finally + no EGL recovery), but the issue requires a Windows/UWP environment to reproduce and validate the fix. The fix is straightforward but needs testing against the exact hardware failure scenario.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views, UWP, OpenGL, reliability labels",
        "risk": "low",
        "confidence": 0.92,
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
        "description": "Post analysis confirming hypothesis and explaining two root causes",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed investigation and for pointing at `AngleSwapChainPanel.cs`.\n\nAfter looking at the current source, there are two related issues:\n\n1. **No EGL recovery when `SwapBuffers` fails.** The comment in `RenderFrame` already says *\"we must reinitialize EGL and the GL resources\"* but the block is empty — no recovery happens. After a long runtime, a D3D/EGL device-lost event can cause `SwapBuffers` to return `false`, leaving the context in a broken state.\n\n2. **Missing try-finally in `RenderOnce`.** If `MakeCurrent()` subsequently throws an exception (which it will when the EGL context is invalid), the exception propagates out of `RenderOnce` before the `lock` block that clears `renderOnceWorker`. Because `Invalidate()` only queues a new background render when `renderOnceWorker == null`, the render thread is permanently blocked.\n\nA minimal fix is to wrap `RenderOnce` with try-finally to guarantee cleanup. A fuller fix would add EGL reinitialization after `SwapBuffers` failure. We'll look at addressing both."
      }
    ]
  }
}
```

</details>
