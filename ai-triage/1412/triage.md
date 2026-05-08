# Issue Triage Report — #1412

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T08:10:00Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp.Views (0.88 (88%)) |
| Suggested action | needs-investigation (0.80 (80%)) |

**Issue Summary:** macOS app crashes with EXC_BAD_ACCESS on the .NET finalizer thread when GPU-backed SkiaSharp objects (SKImage, GRContext) are garbage-collected without the OpenGL context being current, because SKGLView does not override Dispose to release GPU resources on the correct GL thread.

**Analysis:** The crash is caused by GPU-backed Skia objects (SKImage_Gpu, GrGLBuffer) being finalized on the .NET finalizer thread, which calls OpenGL functions (glDeleteBuffers, glDeleteTextures) without a current GL context. This triggers an abort(). Two contributing factors: (1) the reporter was not calling Dispose() on captured SKImage screenshots, allowing them to accumulate and be GC'd; (2) macOS SKGLView does not override Dispose(bool) to explicitly release GRContext, surface, and renderTarget on the correct GL thread, meaning these GPU resources can always be finalized off-thread.

**Recommendations:** **needs-investigation** — The crash pattern is clearly diagnosed (GL ops on finalizer thread from SKGLView not disposing GRContext), and a related iOS fix was merged (#3178). Investigation is needed to determine whether the macOS OpenGL path received the same fix in current SkiaSharp, and to implement a fix if not.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/macOS |
| Backends | backend/OpenGL |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Use SKGLView on macOS with Xamarin.Forms
2. Call TakeScreenshot() or create GPU-backed SKImage objects in OnPaintSurface handler
3. Do not dispose SKImage objects before reassigning
4. Let GC run (more likely on low-memory machines) — triggers crash on Finalizer thread

**Environment:** macOS 10.13.6 (High Sierra), SkiaSharp 2.80.1, Xamarin.Forms 4.6.0.847, MacBook Pro 5,4 with 8 GB RAM, NVIDIA GeForce 9400M (OpenGL)

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3178 — Related: iOS crash when GC collects SKMetalView resources — same category of bug on iOS/Metal, closed as completed 2025-08-22

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | EXC_BAD_ACCESS (SIGABRT) — abort() called from glDeleteBuffers / glDeleteTextures on Finalizer thread |
| Repro quality | partial |
| Target frameworks | xamarinmac |

**Stack trace:**

```text
Thread 2 Crashed: Finalizer
  glDeleteBuffers / glDeleteTextures (libGL.dylib)
  GrGLBuffer::onRelease / SkImage_Gpu::~SkImage_Gpu (libSkiaSharp.dylib)
  mono_gc_run_finalize (gc.c:331)
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | macOS SKGLView.cs still has no Dispose override to clean up context, surface, renderTarget, and canvas fields on the GL thread. |

## Analysis

### Technical Summary

The crash is caused by GPU-backed Skia objects (SKImage_Gpu, GrGLBuffer) being finalized on the .NET finalizer thread, which calls OpenGL functions (glDeleteBuffers, glDeleteTextures) without a current GL context. This triggers an abort(). Two contributing factors: (1) the reporter was not calling Dispose() on captured SKImage screenshots, allowing them to accumulate and be GC'd; (2) macOS SKGLView does not override Dispose(bool) to explicitly release GRContext, surface, and renderTarget on the correct GL thread, meaning these GPU resources can always be finalized off-thread.

### Rationale

Reporter provided two crash reports and symbolicated traces confirming the call chain leads from the .NET finalizer thread through Skia GPU resource destruction into libGL.dylib. The reporter found and fixed their immediate bug (undisposed SKImage objects) but a deeper library issue exists: SKGLView.cs (macOS) holds GRContext, SKSurface, GRBackendRenderTarget, and SKCanvas as fields with no Dispose override, so they are always subject to finalizer-thread GL call crashes. This matches the pattern in issue #3178 (iOS SKMetalView, closed as completed in 2025).

### Key Signals

- "Thread 2 Crashed: Finalizer ... glDeleteBuffers + 18 (libGL.dylib) ... GrGLBuffer::onRelease() ... mono_gc_run_finalize" — **symbolicated crash report (comment #660056062)** (OpenGL deletion called on finalizer thread without current GL context — root cause of abort())
- "My mistake was that I didn't call _test.Dispose() before reassignment my new screenshot to this field." — **reporter comment #660056062** (Reporter's immediate user error — undisposed GPU SKImage accumulates and is GC'd)
- "It seems to be fixed if this context is properly disposed with SKGLView — source/SkiaSharp.Views/SkiaSharp.Views.Mac/SKGLView.cs#L92" — **community commenter (ltetak, comment #718714580)** (Points to the library-level bug: GRContext created in PrepareOpenGL() is never disposed with the view)
- "Can it be same as #3178 by any chance?" — **community commenter (taublast, comment #2954106462, 2025-06-08)** (Links to related iOS/Metal issue (#3178) which was closed as completed — pattern is the same)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs` | 28-34 | direct | Fields context (GRContext), renderTarget (GRBackendRenderTarget), surface (SKSurface), and canvas (SKCanvas) are declared but never disposed in any Dispose override. The class extends NSOpenGLView and has no Dispose(bool disposing) override. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs` | 88-95 | direct | GRContext is created in PrepareOpenGL() (called once when GL context becomes available) but there is no corresponding cleanup when the view is removed from the hierarchy or disposed. |
| `binding/SkiaSharp/GRContext.cs` | 18-23 | direct | GRContext.DisposeNative() calls AbandonContext() which calls the native gr_direct_context_abandon_context — a GPU API call. If GRContext is finalized on the .NET finalizer thread (not the GL thread), this native call executes without a current GL context, causing downstream GL resource deletion to abort(). |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKGLView.cs` | 53-57 | related | iOS SKGLView similarly holds GRContext, surface, renderTarget, canvas fields with no Dispose override — same pattern as macOS. iOS variant is obsoleted (OpenGLES deprecated) but same structural issue exists. |

**Error fingerprint:** `glDeleteBuffers-finalizer-thread-skglview-macos-opengl`

### Workarounds

- Explicitly call Dispose() on all GPU-backed SKImage/SKSurface objects before they go out of scope (especially in OnPaintSurface handlers that capture screenshots via TakeScreenshot())
- Call context.AbandonContext(releaseResources: true) when navigating away from pages containing SKGLView, to release GPU resources while the GL context is still current
- Override OnDisappearing() or equivalent lifecycle hook to call surface?.Dispose(), renderTarget?.Dispose(), context?.Dispose() on the UI thread before the view is removed from the hierarchy
- Call GC.SuppressFinalize(context) after explicitly disposing the GRContext to prevent finalizer thread reentry

### Next Questions

- Was the same fix applied to macOS OpenGL SKGLView as part of the #3178 fix for iOS Metal SKMetalView?
- Does the crash still occur in current SkiaSharp (3.x) on macOS with SKGLView?
- Should SKGLView.cs (macOS) implement IDisposable/Dispose override to abandon/dispose GRContext on the NSOpenGLView thread?

### Resolution Proposals

**Hypothesis:** SKGLView (macOS) must override Dispose(bool) to explicitly dispose GPU resources (surface, renderTarget, context) while the OpenGL context is still current on the correct thread, mirroring the fix pattern applied to SKMetalView in issue #3178.

1. **Add Dispose override to macOS SKGLView** — fix, confidence 0.82 (82%), cost/s, validated=untested
   - Override Dispose(bool) in SKGLView.cs (macOS) to dispose surface, renderTarget, and call context.AbandonContext() / context.Dispose() while making the GL context current. This ensures GPU resources are released on the correct thread, not the finalizer.
2. **Explicit application-level GPU resource disposal** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - Users should call Dispose() on all GPU-backed SkiaSharp objects (SKImage, SKSurface) before they go out of scope, and call context.AbandonContext(releaseResources: true) in the view's disappear/dispose lifecycle hook.

**Recommended proposal:** Add Dispose override to macOS SKGLView

**Why:** The fix should be in the library to prevent crashes for all users, not just those who remember to manually abandon the context. The pattern was already applied to SKMetalView (#3178).

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.80 (80%) |
| Reason | The crash pattern is clearly diagnosed (GL ops on finalizer thread from SKGLView not disposing GRContext), and a related iOS fix was merged (#3178). Investigation is needed to determine whether the macOS OpenGL path received the same fix in current SkiaSharp, and to implement a fix if not. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/SkiaSharp.Views, os/macOS, backend/OpenGL, tenet/reliability | labels=type/bug, area/SkiaSharp.Views, os/macOS, backend/OpenGL, tenet/reliability |
| link-related | low | 0.88 (88%) | Cross-reference issue #3178 (iOS/Metal GC crash — same pattern, closed as completed) | linkedIssue=#3178 |
| add-comment | medium | 0.82 (82%) | Acknowledge the user-level fix, explain the library-level issue, and provide workaround guidance | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed investigation and symbolicated stack trace!

The root crash pattern is clear: when GPU-backed SkiaSharp objects (`SKImage_Gpu`, `GrGLBuffer`) are garbage-collected, their finalizers call OpenGL functions (`glDeleteBuffers`, `glDeleteTextures`) on the .NET Finalizer thread — but those GL calls require a current OpenGL context, which the finalizer thread does not have. This causes `abort()`.

You correctly identified the immediate cause (not calling `Dispose()` on captured `SKImage` screenshots). As a broader workaround:

1. **Always dispose GPU-backed objects explicitly** before they go out of scope — especially `SKImage` objects from `TakeScreenshot()`.
2. **Call `context.AbandonContext(releaseResources: true)`** in your page/view disappear lifecycle hook (before the view is removed from the hierarchy).
3. **Dispose surface, renderTarget, and context** on the UI thread when done.

The underlying library issue is that `SKGLView` (macOS) does not override `Dispose()` to release the `GRContext` and related GPU resources while the OpenGL context is current on the correct thread. A similar issue was fixed for iOS `SKMetalView` in #3178.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1412,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T08:10:00Z"
  },
  "summary": "macOS app crashes with EXC_BAD_ACCESS on the .NET finalizer thread when GPU-backed SkiaSharp objects (SKImage, GRContext) are garbage-collected without the OpenGL context being current, because SKGLView does not override Dispose to release GPU resources on the correct GL thread.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.88
    },
    "platforms": [
      "os/macOS"
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
      "errorMessage": "EXC_BAD_ACCESS (SIGABRT) — abort() called from glDeleteBuffers / glDeleteTextures on Finalizer thread",
      "stackTrace": "Thread 2 Crashed: Finalizer\n  glDeleteBuffers / glDeleteTextures (libGL.dylib)\n  GrGLBuffer::onRelease / SkImage_Gpu::~SkImage_Gpu (libSkiaSharp.dylib)\n  mono_gc_run_finalize (gc.c:331)",
      "reproQuality": "partial",
      "targetFrameworks": [
        "xamarinmac"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Use SKGLView on macOS with Xamarin.Forms",
        "Call TakeScreenshot() or create GPU-backed SKImage objects in OnPaintSurface handler",
        "Do not dispose SKImage objects before reassigning",
        "Let GC run (more likely on low-memory machines) — triggers crash on Finalizer thread"
      ],
      "environmentDetails": "macOS 10.13.6 (High Sierra), SkiaSharp 2.80.1, Xamarin.Forms 4.6.0.847, MacBook Pro 5,4 with 8 GB RAM, NVIDIA GeForce 9400M (OpenGL)",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3178",
          "description": "Related: iOS crash when GC collects SKMetalView resources — same category of bug on iOS/Metal, closed as completed 2025-08-22"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "macOS SKGLView.cs still has no Dispose override to clean up context, surface, renderTarget, and canvas fields on the GL thread."
    }
  },
  "analysis": {
    "summary": "The crash is caused by GPU-backed Skia objects (SKImage_Gpu, GrGLBuffer) being finalized on the .NET finalizer thread, which calls OpenGL functions (glDeleteBuffers, glDeleteTextures) without a current GL context. This triggers an abort(). Two contributing factors: (1) the reporter was not calling Dispose() on captured SKImage screenshots, allowing them to accumulate and be GC'd; (2) macOS SKGLView does not override Dispose(bool) to explicitly release GRContext, surface, and renderTarget on the correct GL thread, meaning these GPU resources can always be finalized off-thread.",
    "rationale": "Reporter provided two crash reports and symbolicated traces confirming the call chain leads from the .NET finalizer thread through Skia GPU resource destruction into libGL.dylib. The reporter found and fixed their immediate bug (undisposed SKImage objects) but a deeper library issue exists: SKGLView.cs (macOS) holds GRContext, SKSurface, GRBackendRenderTarget, and SKCanvas as fields with no Dispose override, so they are always subject to finalizer-thread GL call crashes. This matches the pattern in issue #3178 (iOS SKMetalView, closed as completed in 2025).",
    "keySignals": [
      {
        "text": "Thread 2 Crashed: Finalizer ... glDeleteBuffers + 18 (libGL.dylib) ... GrGLBuffer::onRelease() ... mono_gc_run_finalize",
        "source": "symbolicated crash report (comment #660056062)",
        "interpretation": "OpenGL deletion called on finalizer thread without current GL context — root cause of abort()"
      },
      {
        "text": "My mistake was that I didn't call _test.Dispose() before reassignment my new screenshot to this field.",
        "source": "reporter comment #660056062",
        "interpretation": "Reporter's immediate user error — undisposed GPU SKImage accumulates and is GC'd"
      },
      {
        "text": "It seems to be fixed if this context is properly disposed with SKGLView — source/SkiaSharp.Views/SkiaSharp.Views.Mac/SKGLView.cs#L92",
        "source": "community commenter (ltetak, comment #718714580)",
        "interpretation": "Points to the library-level bug: GRContext created in PrepareOpenGL() is never disposed with the view"
      },
      {
        "text": "Can it be same as #3178 by any chance?",
        "source": "community commenter (taublast, comment #2954106462, 2025-06-08)",
        "interpretation": "Links to related iOS/Metal issue (#3178) which was closed as completed — pattern is the same"
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs",
        "lines": "28-34",
        "finding": "Fields context (GRContext), renderTarget (GRBackendRenderTarget), surface (SKSurface), and canvas (SKCanvas) are declared but never disposed in any Dispose override. The class extends NSOpenGLView and has no Dispose(bool disposing) override.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs",
        "lines": "88-95",
        "finding": "GRContext is created in PrepareOpenGL() (called once when GL context becomes available) but there is no corresponding cleanup when the view is removed from the hierarchy or disposed.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/GRContext.cs",
        "lines": "18-23",
        "finding": "GRContext.DisposeNative() calls AbandonContext() which calls the native gr_direct_context_abandon_context — a GPU API call. If GRContext is finalized on the .NET finalizer thread (not the GL thread), this native call executes without a current GL context, causing downstream GL resource deletion to abort().",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKGLView.cs",
        "lines": "53-57",
        "finding": "iOS SKGLView similarly holds GRContext, surface, renderTarget, canvas fields with no Dispose override — same pattern as macOS. iOS variant is obsoleted (OpenGLES deprecated) but same structural issue exists.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Explicitly call Dispose() on all GPU-backed SKImage/SKSurface objects before they go out of scope (especially in OnPaintSurface handlers that capture screenshots via TakeScreenshot())",
      "Call context.AbandonContext(releaseResources: true) when navigating away from pages containing SKGLView, to release GPU resources while the GL context is still current",
      "Override OnDisappearing() or equivalent lifecycle hook to call surface?.Dispose(), renderTarget?.Dispose(), context?.Dispose() on the UI thread before the view is removed from the hierarchy",
      "Call GC.SuppressFinalize(context) after explicitly disposing the GRContext to prevent finalizer thread reentry"
    ],
    "nextQuestions": [
      "Was the same fix applied to macOS OpenGL SKGLView as part of the #3178 fix for iOS Metal SKMetalView?",
      "Does the crash still occur in current SkiaSharp (3.x) on macOS with SKGLView?",
      "Should SKGLView.cs (macOS) implement IDisposable/Dispose override to abandon/dispose GRContext on the NSOpenGLView thread?"
    ],
    "errorFingerprint": "glDeleteBuffers-finalizer-thread-skglview-macos-opengl",
    "resolution": {
      "hypothesis": "SKGLView (macOS) must override Dispose(bool) to explicitly dispose GPU resources (surface, renderTarget, context) while the OpenGL context is still current on the correct thread, mirroring the fix pattern applied to SKMetalView in issue #3178.",
      "proposals": [
        {
          "title": "Add Dispose override to macOS SKGLView",
          "description": "Override Dispose(bool) in SKGLView.cs (macOS) to dispose surface, renderTarget, and call context.AbandonContext() / context.Dispose() while making the GL context current. This ensures GPU resources are released on the correct thread, not the finalizer.",
          "category": "fix",
          "confidence": 0.82,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Explicit application-level GPU resource disposal",
          "description": "Users should call Dispose() on all GPU-backed SkiaSharp objects (SKImage, SKSurface) before they go out of scope, and call context.AbandonContext(releaseResources: true) in the view's disappear/dispose lifecycle hook.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add Dispose override to macOS SKGLView",
      "recommendedReason": "The fix should be in the library to prevent crashes for all users, not just those who remember to manually abandon the context. The pattern was already applied to SKMetalView (#3178)."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.8,
      "reason": "The crash pattern is clearly diagnosed (GL ops on finalizer thread from SKGLView not disposing GRContext), and a related iOS fix was merged (#3178). Investigation is needed to determine whether the macOS OpenGL path received the same fix in current SkiaSharp, and to implement a fix if not.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views, os/macOS, backend/OpenGL, tenet/reliability",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/macOS",
          "backend/OpenGL",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference issue #3178 (iOS/Metal GC crash — same pattern, closed as completed)",
        "risk": "low",
        "confidence": 0.88,
        "linkedIssue": 3178
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the user-level fix, explain the library-level issue, and provide workaround guidance",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed investigation and symbolicated stack trace!\n\nThe root crash pattern is clear: when GPU-backed SkiaSharp objects (`SKImage_Gpu`, `GrGLBuffer`) are garbage-collected, their finalizers call OpenGL functions (`glDeleteBuffers`, `glDeleteTextures`) on the .NET Finalizer thread — but those GL calls require a current OpenGL context, which the finalizer thread does not have. This causes `abort()`.\n\nYou correctly identified the immediate cause (not calling `Dispose()` on captured `SKImage` screenshots). As a broader workaround:\n\n1. **Always dispose GPU-backed objects explicitly** before they go out of scope — especially `SKImage` objects from `TakeScreenshot()`.\n2. **Call `context.AbandonContext(releaseResources: true)`** in your page/view disappear lifecycle hook (before the view is removed from the hierarchy).\n3. **Dispose surface, renderTarget, and context** on the UI thread when done.\n\nThe underlying library issue is that `SKGLView` (macOS) does not override `Dispose()` to release the `GRContext` and related GPU resources while the OpenGL context is current on the correct thread. A similar issue was fixed for iOS `SKMetalView` in #3178."
      }
    ]
  }
}
```

</details>
