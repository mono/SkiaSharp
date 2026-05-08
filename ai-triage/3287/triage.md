# Issue Triage Report — #3287

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T20:08:12Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views.Maui (0.92 (92%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** On Android MAUI with SkiaSharp 3.116.x, SKGLView.PaintSurface fires on the GL rendering thread instead of the main thread, causing crashes when accessing UI properties; this is a regression from Xamarin (2.88.x) and inconsistent with all other platforms.

**Analysis:** Android's SKGLTextureViewRenderer.OnDrawFrame (the method that invokes PaintSurface) is called by the GL thread — the dedicated OpenGL rendering thread that Android uses for GLTextureView/GLSurfaceView. The iOS handler explicitly marshals display requests to the main thread via BeginInvokeOnMainThread, but the Android MAUI handler (SKGLViewHandler.Android.cs) does no such marshaling. This causes PaintSurface to fire off the main thread on Android only, breaking access to UI properties like IsVisible, IsEnabled, Width, etc.

**Recommendations:** **needs-investigation** — Root cause is clear (no main-thread marshaling in Android handler vs iOS which does marshal), but the correct fix strategy requires investigation — specifically whether marshaling all frames to the main thread has unacceptable performance cost on Android, and whether the Xamarin 2.88.x behavior was actually also GL-threaded.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/Android |
| Backends | backend/OpenGL |
| Tenets | tenet/compatibility, tenet/reliability |
| Partner | partner/maui |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a .NET MAUI app targeting Android with SkiaSharp 3.116.1
2. Add an SKGLView to a page
3. In the PaintSurface event handler, check MainThread.IsMainThread
4. Observe it returns false (not the main thread)

**Environment:** SkiaSharp 3.116.1, .NET MAUI 9.0.50, Android 15, Google Pixel 6a, Samsung SM-P615

**Related issues:** #2840

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | platform-specific |
| Error message | MainThread.IsMainThread returns false inside SKGLView.PaintSurface on Android MAUI |
| Repro quality | partial |
| Target frameworks | net9.0-android |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 3.116.1, 2.88.8, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | likely |
| Relevance reason | The Android SKGLView handler has no main-thread marshaling; the GL thread calls PaintSurface directly. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.90 (90%) |
| Reason | Reporter confirms PaintSurface was always called on the main thread in Xamarin 2.88.x for both SKGLView and SKCanvasView. In MAUI 3.x on Android, SKGLView uses GLTextureView with a dedicated GL thread and no marshaling to the main thread. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.0 |

## Analysis

### Technical Summary

Android's SKGLTextureViewRenderer.OnDrawFrame (the method that invokes PaintSurface) is called by the GL thread — the dedicated OpenGL rendering thread that Android uses for GLTextureView/GLSurfaceView. The iOS handler explicitly marshals display requests to the main thread via BeginInvokeOnMainThread, but the Android MAUI handler (SKGLViewHandler.Android.cs) does no such marshaling. This causes PaintSurface to fire off the main thread on Android only, breaking access to UI properties like IsVisible, IsEnabled, Width, etc.

### Rationale

This is a clear platform-inconsistency regression. On iOS (via RenderLoopManager.RequestDisplay), the MAUI SKGLView handler wraps nativeView.Display() in BeginInvokeOnMainThread — ensuring PaintSurface fires on the main thread. On Android, GLTextureView drives OnDrawFrame on a dedicated GL thread with no main-thread dispatch. The Xamarin.Android implementation likely also ran on the GL thread, but the previous SkiaSharp 2.88.x Android backend may have used a different rendering path. Regardless, the inconsistency between Android and all other platforms (iOS, Windows, macOS) is the core bug.

### Key Signals

- "MainThread.IsMainThread function... returns false" — **issue body** (Hard evidence the PaintSurface callback fires off the main thread on Android MAUI.)
- "this problem happens only in MAUI on Android. MAUI iOS, MAUI Windows, and Xamarin Android all seem to work correctly" — **comment #1 (janne-hmp)** (Isolates the bug to Android MAUI handler only — all other platforms marshal correctly.)
- "In Xamarin with SkiaSharp 2.88.8, the PaintSurface seemed to be always called from the main thread with both SKGLView and SKCanvasView." — **issue body** (Confirms regression from 2.88.x to 3.x MAUI migration.)
- "We actually had a crash due to reading IsVisible property on Android with SKGLView." — **comment #3 (janne-hmp)** (Real-world crash consequence of accessing UI properties from the GL thread.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.Android.cs` | 85-104 | direct | OnPaintSurface is called directly from the GL thread via SKGLTextureView's InternalRenderer — no main-thread dispatch. VirtualView.OnPaintSurface is called on whatever thread GLTextureView.IRenderer.OnDrawFrame fires on. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLTextureViewRenderer.cs` | 43-98 | direct | OnDrawFrame (which calls OnPaintSurface) is part of GLTextureView.IRenderer — Android calls this on the dedicated GL thread, not the main thread. There is no main-thread marshaling here. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/GLTextureView.cs` | 36-60 | related | GLTextureView extends Android's TextureView with a GLThread field — confirms rendering happens on a dedicated background thread, not the main/UI thread. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.iOS.cs` | 136-148 | related | iOS RenderLoopManager.RequestDisplay() wraps nativeView.Display() in BeginInvokeOnMainThread — this is why PaintSurface fires on the main thread on iOS. Android has no equivalent marshaling. |

### Workarounds

- Wrap all UI property accesses inside the PaintSurface handler with MainThread.InvokeOnMainThread() or cache UI state in fields updated from the main thread.
- Use SKCanvasView instead of SKGLView on Android MAUI — SKCanvasView.PaintSurface is always called on the main thread.
- Read required UI properties (IsVisible, Width, etc.) before the PaintSurface is triggered and cache them in thread-safe fields.

### Next Questions

- Should Android SKGLView always marshal PaintSurface to the main thread (matching iOS behavior), or should the fix add a documented threading guarantee so users can write thread-safe handlers?
- Does the Xamarin.Android 2.88.x implementation also run PaintSurface on the GL thread? If so, this may be a MAUI migration issue, not a SkiaSharp regression.
- Are there performance implications to marshaling every frame render to the main thread on Android?

### Resolution Proposals

**Hypothesis:** The Android SKGLViewHandler needs to dispatch PaintSurface to the main thread (like iOS does), or document clearly that it fires on the GL thread so users know they must synchronize UI access manually.

1. **Marshal PaintSurface to main thread in Android handler** — fix, confidence 0.70 (70%), cost/s, validated=untested
   - In SKGLViewHandler.Android.cs, wrap the VirtualView.OnPaintSurface call inside MainThread.BeginInvokeOnMainThread (or equivalent) to match iOS behavior. This ensures consistent threading across all platforms.
2. **Cache UI state outside PaintSurface** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - As a user-side workaround, cache all needed UI properties (IsVisible, Width, etc.) in fields that are updated from the main thread, then read the cached values inside PaintSurface safely.
3. **Switch to SKCanvasView on Android** — alternative, confidence 0.95 (95%), cost/s, validated=untested
   - Replace SKGLView with SKCanvasView on Android if main-thread PaintSurface behavior is required. SKCanvasView always fires on the main thread on all platforms.

**Recommended proposal:** Marshal PaintSurface to main thread in Android handler

**Why:** Matches the behavior already implemented on iOS via BeginInvokeOnMainThread, restores Xamarin 2.88.x behavior, and ensures a consistent cross-platform API contract.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Root cause is clear (no main-thread marshaling in Android handler vs iOS which does marshal), but the correct fix strategy requires investigation — specifically whether marshaling all frames to the main thread has unacceptable performance cost on Android, and whether the Xamarin 2.88.x behavior was actually also GL-threaded. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, MAUI views, Android, OpenGL, compatibility/reliability, MAUI partner labels | labels=type/bug, area/SkiaSharp.Views.Maui, os/Android, backend/OpenGL, tenet/compatibility, tenet/reliability, partner/maui |
| add-comment | medium | 0.88 (88%) | Acknowledge regression, explain root cause (GL thread vs main thread), provide workarounds, ask about Xamarin 2.88 behavior | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and platform comparison — this really helped isolate the issue.

**Root cause:** On Android, `SKGLView` uses a `GLTextureView` which drives rendering on a dedicated GL thread (this is standard Android OpenGL behavior). The `PaintSurface` event is fired directly from that GL thread with no main-thread marshaling. On iOS, the MAUI handler explicitly wraps display requests in `BeginInvokeOnMainThread`, which is why iOS always fires `PaintSurface` on the main thread. This inconsistency is the bug.

**Workarounds (while a fix is investigated):**

1. **Cache UI state** — Read UI properties (like `IsVisible`, `Width`, `IsEnabled`) from the main thread and store them in thread-safe fields. Read the cached values inside `PaintSurface`:
   ```csharp
   // In your page or ViewModel, updated from main thread:
   private volatile bool _isViewVisible = true;

   // In PaintSurface — safe to read cached field:
   if (!_isViewVisible) return;
   ```

2. **Use SKCanvasView** — If you don't need GPU acceleration, `SKCanvasView` always fires `PaintSurface` on the main thread on all platforms.

3. **Wrap UI access with MainThread.InvokeOnMainThread** — For any UI property access needed during painting, marshal back:
   ```csharp
   void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
   {
       // Do not access UI properties directly here on Android
       // Use cached state instead
   }
   ```

We'll investigate whether the fix should marshal `PaintSurface` to the main thread (matching iOS) or document the GL-thread behavior as intentional with guidance for thread-safe usage.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3287,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T20:08:12Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "On Android MAUI with SkiaSharp 3.116.x, SKGLView.PaintSurface fires on the GL rendering thread instead of the main thread, causing crashes when accessing UI properties; this is a regression from Xamarin (2.88.x) and inconsistent with all other platforms.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.92
    },
    "platforms": [
      "os/Android"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "tenets": [
      "tenet/compatibility",
      "tenet/reliability"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "platform-specific",
      "errorMessage": "MainThread.IsMainThread returns false inside SKGLView.PaintSurface on Android MAUI",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net9.0-android"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET MAUI app targeting Android with SkiaSharp 3.116.1",
        "Add an SKGLView to a page",
        "In the PaintSurface event handler, check MainThread.IsMainThread",
        "Observe it returns false (not the main thread)"
      ],
      "environmentDetails": "SkiaSharp 3.116.1, .NET MAUI 9.0.50, Android 15, Google Pixel 6a, Samsung SM-P615",
      "relatedIssues": [
        2840
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "3.116.1",
        "2.88.8",
        "2.88.9"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.116.0",
      "currentRelevance": "likely",
      "relevanceReason": "The Android SKGLView handler has no main-thread marshaling; the GL thread calls PaintSurface directly."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.9,
      "reason": "Reporter confirms PaintSurface was always called on the main thread in Xamarin 2.88.x for both SKGLView and SKCanvasView. In MAUI 3.x on Android, SKGLView uses GLTextureView with a dedicated GL thread and no marshaling to the main thread.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.0"
    }
  },
  "analysis": {
    "summary": "Android's SKGLTextureViewRenderer.OnDrawFrame (the method that invokes PaintSurface) is called by the GL thread — the dedicated OpenGL rendering thread that Android uses for GLTextureView/GLSurfaceView. The iOS handler explicitly marshals display requests to the main thread via BeginInvokeOnMainThread, but the Android MAUI handler (SKGLViewHandler.Android.cs) does no such marshaling. This causes PaintSurface to fire off the main thread on Android only, breaking access to UI properties like IsVisible, IsEnabled, Width, etc.",
    "rationale": "This is a clear platform-inconsistency regression. On iOS (via RenderLoopManager.RequestDisplay), the MAUI SKGLView handler wraps nativeView.Display() in BeginInvokeOnMainThread — ensuring PaintSurface fires on the main thread. On Android, GLTextureView drives OnDrawFrame on a dedicated GL thread with no main-thread dispatch. The Xamarin.Android implementation likely also ran on the GL thread, but the previous SkiaSharp 2.88.x Android backend may have used a different rendering path. Regardless, the inconsistency between Android and all other platforms (iOS, Windows, macOS) is the core bug.",
    "keySignals": [
      {
        "text": "MainThread.IsMainThread function... returns false",
        "source": "issue body",
        "interpretation": "Hard evidence the PaintSurface callback fires off the main thread on Android MAUI."
      },
      {
        "text": "this problem happens only in MAUI on Android. MAUI iOS, MAUI Windows, and Xamarin Android all seem to work correctly",
        "source": "comment #1 (janne-hmp)",
        "interpretation": "Isolates the bug to Android MAUI handler only — all other platforms marshal correctly."
      },
      {
        "text": "In Xamarin with SkiaSharp 2.88.8, the PaintSurface seemed to be always called from the main thread with both SKGLView and SKCanvasView.",
        "source": "issue body",
        "interpretation": "Confirms regression from 2.88.x to 3.x MAUI migration."
      },
      {
        "text": "We actually had a crash due to reading IsVisible property on Android with SKGLView.",
        "source": "comment #3 (janne-hmp)",
        "interpretation": "Real-world crash consequence of accessing UI properties from the GL thread."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.Android.cs",
        "lines": "85-104",
        "finding": "OnPaintSurface is called directly from the GL thread via SKGLTextureView's InternalRenderer — no main-thread dispatch. VirtualView.OnPaintSurface is called on whatever thread GLTextureView.IRenderer.OnDrawFrame fires on.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLTextureViewRenderer.cs",
        "lines": "43-98",
        "finding": "OnDrawFrame (which calls OnPaintSurface) is part of GLTextureView.IRenderer — Android calls this on the dedicated GL thread, not the main thread. There is no main-thread marshaling here.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/GLTextureView.cs",
        "lines": "36-60",
        "finding": "GLTextureView extends Android's TextureView with a GLThread field — confirms rendering happens on a dedicated background thread, not the main/UI thread.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.iOS.cs",
        "lines": "136-148",
        "finding": "iOS RenderLoopManager.RequestDisplay() wraps nativeView.Display() in BeginInvokeOnMainThread — this is why PaintSurface fires on the main thread on iOS. Android has no equivalent marshaling.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Wrap all UI property accesses inside the PaintSurface handler with MainThread.InvokeOnMainThread() or cache UI state in fields updated from the main thread.",
      "Use SKCanvasView instead of SKGLView on Android MAUI — SKCanvasView.PaintSurface is always called on the main thread.",
      "Read required UI properties (IsVisible, Width, etc.) before the PaintSurface is triggered and cache them in thread-safe fields."
    ],
    "nextQuestions": [
      "Should Android SKGLView always marshal PaintSurface to the main thread (matching iOS behavior), or should the fix add a documented threading guarantee so users can write thread-safe handlers?",
      "Does the Xamarin.Android 2.88.x implementation also run PaintSurface on the GL thread? If so, this may be a MAUI migration issue, not a SkiaSharp regression.",
      "Are there performance implications to marshaling every frame render to the main thread on Android?"
    ],
    "resolution": {
      "hypothesis": "The Android SKGLViewHandler needs to dispatch PaintSurface to the main thread (like iOS does), or document clearly that it fires on the GL thread so users know they must synchronize UI access manually.",
      "proposals": [
        {
          "title": "Marshal PaintSurface to main thread in Android handler",
          "description": "In SKGLViewHandler.Android.cs, wrap the VirtualView.OnPaintSurface call inside MainThread.BeginInvokeOnMainThread (or equivalent) to match iOS behavior. This ensures consistent threading across all platforms.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Cache UI state outside PaintSurface",
          "description": "As a user-side workaround, cache all needed UI properties (IsVisible, Width, etc.) in fields that are updated from the main thread, then read the cached values inside PaintSurface safely.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Switch to SKCanvasView on Android",
          "description": "Replace SKGLView with SKCanvasView on Android if main-thread PaintSurface behavior is required. SKCanvasView always fires on the main thread on all platforms.",
          "category": "alternative",
          "confidence": 0.95,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Marshal PaintSurface to main thread in Android handler",
      "recommendedReason": "Matches the behavior already implemented on iOS via BeginInvokeOnMainThread, restores Xamarin 2.88.x behavior, and ensures a consistent cross-platform API contract."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Root cause is clear (no main-thread marshaling in Android handler vs iOS which does marshal), but the correct fix strategy requires investigation — specifically whether marshaling all frames to the main thread has unacceptable performance cost on Android, and whether the Xamarin 2.88.x behavior was actually also GL-threaded.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, MAUI views, Android, OpenGL, compatibility/reliability, MAUI partner labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Maui",
          "os/Android",
          "backend/OpenGL",
          "tenet/compatibility",
          "tenet/reliability",
          "partner/maui"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge regression, explain root cause (GL thread vs main thread), provide workarounds, ask about Xamarin 2.88 behavior",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report and platform comparison — this really helped isolate the issue.\n\n**Root cause:** On Android, `SKGLView` uses a `GLTextureView` which drives rendering on a dedicated GL thread (this is standard Android OpenGL behavior). The `PaintSurface` event is fired directly from that GL thread with no main-thread marshaling. On iOS, the MAUI handler explicitly wraps display requests in `BeginInvokeOnMainThread`, which is why iOS always fires `PaintSurface` on the main thread. This inconsistency is the bug.\n\n**Workarounds (while a fix is investigated):**\n\n1. **Cache UI state** — Read UI properties (like `IsVisible`, `Width`, `IsEnabled`) from the main thread and store them in thread-safe fields. Read the cached values inside `PaintSurface`:\n   ```csharp\n   // In your page or ViewModel, updated from main thread:\n   private volatile bool _isViewVisible = true;\n\n   // In PaintSurface — safe to read cached field:\n   if (!_isViewVisible) return;\n   ```\n\n2. **Use SKCanvasView** — If you don't need GPU acceleration, `SKCanvasView` always fires `PaintSurface` on the main thread on all platforms.\n\n3. **Wrap UI access with MainThread.InvokeOnMainThread** — For any UI property access needed during painting, marshal back:\n   ```csharp\n   void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)\n   {\n       // Do not access UI properties directly here on Android\n       // Use cached state instead\n   }\n   ```\n\nWe'll investigate whether the fix should marshal `PaintSurface` to the main thread (matching iOS) or document the GL-thread behavior as intentional with guidance for thread-safe usage."
      }
    ]
  }
}
```

</details>
