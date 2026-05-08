# Issue Triage Report — #1465

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T05:35:00Z |
| Type | type/feature-request (0.92 (92%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** Feature request to add an option on SKGLView to marshal the PaintSurface callback to the UI thread on Android, where the GL renderer calls OnDrawFrame on a background GL thread unlike iOS and UWP which use the main thread.

**Analysis:** Android GLSurfaceView and GLTextureView call the renderer's OnDrawFrame on a dedicated GL background thread by design. iOS GLKView calls DrawInRect on the main thread. This asymmetry causes concurrency issues in Xamarin.Forms (and MAUI) apps that manage shared state on the UI thread. The request is to add a threading option so Android PaintSurface can be marshaled to the UI thread.

**Recommendations:** **keep-open** — Valid feature request with multiple community supporters. The threading asymmetry is real and confirmed in source code. Xamarin.Forms is deprecated but the gap persists in MAUI. No fix has been implemented; keeping open for tracking.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp.Views |
| Platforms | os/Android |
| Backends | backend/OpenGL |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Create a Xamarin.Forms app with SKGLView on Android
2. Manage UI state on the main thread
3. Observe PaintSurface is called on a background GL thread, not the main thread
4. Attempt to update state from PaintSurface causing concurrency issues

**Environment:** Xamarin.Forms, Android, SKGLView. iOS and UWP paint on main thread.

**Repository links:**
- https://github.com/tmijieux/TestForMapsui/tree/skglview-thread-android — Minimal reproduction: NullReferenceException when tapping buttons due to GL thread / UI thread race

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Code investigation confirms Android GL renderer still calls OnDrawFrame on a background thread in both the Views and Maui handlers. No UI-thread marshaling option has been added. |

## Analysis

### Technical Summary

Android GLSurfaceView and GLTextureView call the renderer's OnDrawFrame on a dedicated GL background thread by design. iOS GLKView calls DrawInRect on the main thread. This asymmetry causes concurrency issues in Xamarin.Forms (and MAUI) apps that manage shared state on the UI thread. The request is to add a threading option so Android PaintSurface can be marshaled to the UI thread.

### Rationale

The issue is clearly a feature request: no option exists to paint on the UI thread for Android GL views. The gap was confirmed in source code: SKGLSurfaceViewRenderer.OnDrawFrame and SKGLTextureViewRenderer.OnDrawFrame are called by the GL thread, and the MAUI Android handler (SKGLViewHandler.Android.cs) also uses SKGLTextureView which follows the same threading model. The feature is well-scoped but requires design work. Xamarin.Forms is deprecated but the same gap persists in MAUI.

### Key Signals

- "SKGLView on iOS and UWP paints the surface on the main thread, and Android does not" — **issue body** (Platform threading asymmetry in PaintSurface callback confirmed by reporter and code investigation.)
- "Calling InvokeOnMainThreadAsync and waiting for it, but that deadlocks when rotating the screen" — **issue body** (The naive workaround is not safe; a proper solution requires thread-safe state management or a supported marshaling option in the library.)
- "NullReferenceException when tapping buttons due to GL thread / UI thread race" — **comment by tmijieux** (The threading race causes real bugs, not just inconvenience. Repro project provided.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceViewRenderer.cs` | 33-87 | direct | OnDrawFrame is called by Android's GL thread. PaintSurface event is raised here with no UI-thread marshaling. No option exists to change this behavior. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLTextureViewRenderer.cs` | 43-98 | direct | OnDrawFrame in GLTextureView renderer also called on GL thread. Same threading model as SKGLSurfaceViewRenderer. MAUI handler (SKGLViewHandler.Android.cs) uses this class. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKGLView.cs` | 108-165 | related | DrawInRect (iOS) is called by GLKView on the main thread, confirming the cross-platform asymmetry. No special threading needed on iOS. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.Android.cs` | 1-145 | related | MAUI Android handler uses SKGLTextureView which inherits the same GL-thread rendering model. No UI-thread option in MAUI either — the gap persists after the Xamarin.Forms → MAUI migration. |

### Workarounds

- Use thread-safe state containers (ConcurrentDictionary, locks, Interlocked) in PaintSurface to avoid depending on UI-thread data directly.
- Use SKCanvasView instead of SKGLView on Android — SKCanvasView paints on the main thread and avoids the threading asymmetry (at the cost of no GPU acceleration).
- Post state updates via Android MainLooper.MainLooper.Post() or Xamarin.Forms Device.BeginInvokeOnMainThread(), and read a copy of the state snapshot from PaintSurface.

### Next Questions

- Should this be tracked as a MAUI-specific request (area/SkiaSharp.Views.Maui) since Xamarin.Forms is deprecated?
- Is there interest in a HasRenderLoop-style property or a simpler synchronization hook?
- Would marshaling to the UI thread introduce performance concerns for render-loop scenarios?

### Resolution Proposals

**Hypothesis:** No API exists to marshal Android GL paint callbacks to the UI thread. Adding a bool property (e.g., PaintOnMainThread or UseMainThread) on SKGLView / the handler would allow apps to opt in to UI-thread painting at the cost of potential frame drops.

1. **Use SKCanvasView on Android (workaround)** — workaround, confidence 0.90 (90%), cost/s, validated=untested
   - Switch from SKGLView to SKCanvasView on Android. SKCanvasView paints on the main thread via the standard Android View.onDraw mechanism. No GPU acceleration, but removes threading complexity.
2. **Thread-safe state snapshot (workaround)** — workaround, confidence 0.85 (85%), cost/s, validated=untested
   - Capture a snapshot of state on the UI thread and consume it inside PaintSurface. Use Interlocked.Exchange or a lock-protected copy. Avoids blocking either thread.
3. **Add PaintOnMainThread option to Android GL handlers (fix)** — fix, confidence 0.60 (60%), cost/l, validated=untested
   - Add a property to SKGLSurfaceView / SKGLTextureView and the MAUI handler that, when enabled, marshals the OnDrawFrame callback to the UI thread before raising PaintSurface. Requires careful GL context current-thread management since GL calls must stay on the GL thread.

**Recommended proposal:** Use SKCanvasView on Android (workaround)

**Why:** Simplest immediate workaround. The fix proposal has complexity around GL context affinity that requires careful design. For apps that don't need GPU acceleration on Android, SKCanvasView is the correct choice.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Valid feature request with multiple community supporters. The threading asymmetry is real and confirmed in source code. Xamarin.Forms is deprecated but the gap persists in MAUI. No fix has been implemented; keeping open for tracking. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply feature-request, views, android, opengl labels | labels=type/feature-request, area/SkiaSharp.Views, os/Android, backend/OpenGL |
| add-comment | medium | 0.82 (82%) | Acknowledge the threading asymmetry, confirm it persists in MAUI, offer workarounds | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report! The threading asymmetry is by design in Android's `GLSurfaceView`/`GLTextureView`: `OnDrawFrame` runs on a dedicated GL thread, while iOS `GLKView` calls its delegate on the main thread.

This gap still exists in the MAUI `SKGLViewHandler.Android` (uses `SKGLTextureView`, same threading model).

**Workarounds while this is open:**

1. **Use `SKCanvasView` on Android** — paints on the main thread via `View.onDraw`, no GPU acceleration but no threading complexity.
2. **Thread-safe state snapshot** — capture a data copy on the UI thread and read it inside `PaintSurface`. Use `Interlocked.Exchange` or a lock-protected field:
   ```csharp
   // UI thread: update shared state
   Interlocked.Exchange(ref _myState, newState);

   // PaintSurface (GL thread): read snapshot
   var state = Volatile.Read(ref _myState);
   ```
3. **Avoid `InvokeOnMainThreadAsync` from inside `PaintSurface`** — this deadlocks under rotation as noted in the original report.

A proper `PaintOnMainThread` property would need careful GL context management (GL calls must stay on the GL thread), so we're tracking this as a future enhancement.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1465,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T05:35:00Z"
  },
  "summary": "Feature request to add an option on SKGLView to marshal the PaintSurface callback to the UI thread on Android, where the GL renderer calls OnDrawFrame on a background GL thread unlike iOS and UWP which use the main thread.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.9
    },
    "platforms": [
      "os/Android"
    ],
    "backends": [
      "backend/OpenGL"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Xamarin.Forms app with SKGLView on Android",
        "Manage UI state on the main thread",
        "Observe PaintSurface is called on a background GL thread, not the main thread",
        "Attempt to update state from PaintSurface causing concurrency issues"
      ],
      "environmentDetails": "Xamarin.Forms, Android, SKGLView. iOS and UWP paint on main thread.",
      "repoLinks": [
        {
          "url": "https://github.com/tmijieux/TestForMapsui/tree/skglview-thread-android",
          "description": "Minimal reproduction: NullReferenceException when tapping buttons due to GL thread / UI thread race"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "Code investigation confirms Android GL renderer still calls OnDrawFrame on a background thread in both the Views and Maui handlers. No UI-thread marshaling option has been added."
    }
  },
  "analysis": {
    "summary": "Android GLSurfaceView and GLTextureView call the renderer's OnDrawFrame on a dedicated GL background thread by design. iOS GLKView calls DrawInRect on the main thread. This asymmetry causes concurrency issues in Xamarin.Forms (and MAUI) apps that manage shared state on the UI thread. The request is to add a threading option so Android PaintSurface can be marshaled to the UI thread.",
    "rationale": "The issue is clearly a feature request: no option exists to paint on the UI thread for Android GL views. The gap was confirmed in source code: SKGLSurfaceViewRenderer.OnDrawFrame and SKGLTextureViewRenderer.OnDrawFrame are called by the GL thread, and the MAUI Android handler (SKGLViewHandler.Android.cs) also uses SKGLTextureView which follows the same threading model. The feature is well-scoped but requires design work. Xamarin.Forms is deprecated but the same gap persists in MAUI.",
    "keySignals": [
      {
        "text": "SKGLView on iOS and UWP paints the surface on the main thread, and Android does not",
        "source": "issue body",
        "interpretation": "Platform threading asymmetry in PaintSurface callback confirmed by reporter and code investigation."
      },
      {
        "text": "Calling InvokeOnMainThreadAsync and waiting for it, but that deadlocks when rotating the screen",
        "source": "issue body",
        "interpretation": "The naive workaround is not safe; a proper solution requires thread-safe state management or a supported marshaling option in the library."
      },
      {
        "text": "NullReferenceException when tapping buttons due to GL thread / UI thread race",
        "source": "comment by tmijieux",
        "interpretation": "The threading race causes real bugs, not just inconvenience. Repro project provided."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceViewRenderer.cs",
        "lines": "33-87",
        "finding": "OnDrawFrame is called by Android's GL thread. PaintSurface event is raised here with no UI-thread marshaling. No option exists to change this behavior.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLTextureViewRenderer.cs",
        "lines": "43-98",
        "finding": "OnDrawFrame in GLTextureView renderer also called on GL thread. Same threading model as SKGLSurfaceViewRenderer. MAUI handler (SKGLViewHandler.Android.cs) uses this class.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKGLView.cs",
        "lines": "108-165",
        "finding": "DrawInRect (iOS) is called by GLKView on the main thread, confirming the cross-platform asymmetry. No special threading needed on iOS.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.Android.cs",
        "lines": "1-145",
        "finding": "MAUI Android handler uses SKGLTextureView which inherits the same GL-thread rendering model. No UI-thread option in MAUI either — the gap persists after the Xamarin.Forms → MAUI migration.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Should this be tracked as a MAUI-specific request (area/SkiaSharp.Views.Maui) since Xamarin.Forms is deprecated?",
      "Is there interest in a HasRenderLoop-style property or a simpler synchronization hook?",
      "Would marshaling to the UI thread introduce performance concerns for render-loop scenarios?"
    ],
    "workarounds": [
      "Use thread-safe state containers (ConcurrentDictionary, locks, Interlocked) in PaintSurface to avoid depending on UI-thread data directly.",
      "Use SKCanvasView instead of SKGLView on Android — SKCanvasView paints on the main thread and avoids the threading asymmetry (at the cost of no GPU acceleration).",
      "Post state updates via Android MainLooper.MainLooper.Post() or Xamarin.Forms Device.BeginInvokeOnMainThread(), and read a copy of the state snapshot from PaintSurface."
    ],
    "resolution": {
      "hypothesis": "No API exists to marshal Android GL paint callbacks to the UI thread. Adding a bool property (e.g., PaintOnMainThread or UseMainThread) on SKGLView / the handler would allow apps to opt in to UI-thread painting at the cost of potential frame drops.",
      "proposals": [
        {
          "title": "Use SKCanvasView on Android (workaround)",
          "description": "Switch from SKGLView to SKCanvasView on Android. SKCanvasView paints on the main thread via the standard Android View.onDraw mechanism. No GPU acceleration, but removes threading complexity.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Thread-safe state snapshot (workaround)",
          "description": "Capture a snapshot of state on the UI thread and consume it inside PaintSurface. Use Interlocked.Exchange or a lock-protected copy. Avoids blocking either thread.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Add PaintOnMainThread option to Android GL handlers (fix)",
          "description": "Add a property to SKGLSurfaceView / SKGLTextureView and the MAUI handler that, when enabled, marshals the OnDrawFrame callback to the UI thread before raising PaintSurface. Requires careful GL context current-thread management since GL calls must stay on the GL thread.",
          "category": "fix",
          "confidence": 0.6,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use SKCanvasView on Android (workaround)",
      "recommendedReason": "Simplest immediate workaround. The fix proposal has complexity around GL context affinity that requires careful design. For apps that don't need GPU acceleration on Android, SKCanvasView is the correct choice."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.8,
      "reason": "Valid feature request with multiple community supporters. The threading asymmetry is real and confirmed in source code. Xamarin.Forms is deprecated but the gap persists in MAUI. No fix has been implemented; keeping open for tracking.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, views, android, opengl labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp.Views",
          "os/Android",
          "backend/OpenGL"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the threading asymmetry, confirm it persists in MAUI, offer workarounds",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report! The threading asymmetry is by design in Android's `GLSurfaceView`/`GLTextureView`: `OnDrawFrame` runs on a dedicated GL thread, while iOS `GLKView` calls its delegate on the main thread.\n\nThis gap still exists in the MAUI `SKGLViewHandler.Android` (uses `SKGLTextureView`, same threading model).\n\n**Workarounds while this is open:**\n\n1. **Use `SKCanvasView` on Android** — paints on the main thread via `View.onDraw`, no GPU acceleration but no threading complexity.\n2. **Thread-safe state snapshot** — capture a data copy on the UI thread and read it inside `PaintSurface`. Use `Interlocked.Exchange` or a lock-protected field:\n   ```csharp\n   // UI thread: update shared state\n   Interlocked.Exchange(ref _myState, newState);\n\n   // PaintSurface (GL thread): read snapshot\n   var state = Volatile.Read(ref _myState);\n   ```\n3. **Avoid `InvokeOnMainThreadAsync` from inside `PaintSurface`** — this deadlocks under rotation as noted in the original report.\n\nA proper `PaintOnMainThread` property would need careful GL context management (GL calls must stay on the GL thread), so we're tracking this as a future enhancement."
      }
    ]
  }
}
```

</details>
