# Issue Triage Report — #798

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T18:46:12Z |
| Type | type/question (0.92 (92%)) |
| Area | area/SkiaSharp.Views (0.85 (85%)) |
| Suggested action | needs-info (0.75 (75%)) |

**Issue Summary:** Reporter asks why SKGLView has poor performance on Android compared to iOS when rendering paths with 10,000+ points during pan/zoom interactions, providing a sample project for reference.

**Analysis:** The performance gap between iOS and Android for SKGLView is largely architectural: iOS GLKView integrates with Core Animation for low-latency compositing while Android GLSurfaceView requires additional compositing overhead via SurfaceFlinger. Additionally, re-tessellating 10,000+ point paths on every frame during pan is inherently expensive regardless of whether GL or CPU rendering is used.

**Recommendations:** **needs-info** — Performance question missing SkiaSharp version and Android API level. Architectural explanation and workarounds can be provided, but version info would help confirm whether newer releases address any related improvements.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp.Views |
| Platforms | os/Android, os/iOS |
| Backends | backend/OpenGL |
| Tenets | tenet/performance |
| Partner | — |
| Current labels | type/question |

## Evidence

### Reproduction

1. Create a custom SKGLView supporting pan and zoom
2. Render paths containing 10,000+ points
3. Test on Android device (Pixel or Nexus 5X)
4. Compare to iOS where performance is significantly better

**Environment:** Android devices: Pixel and Nexus 5X (both Qualcomm Adreno GPU). No SkiaSharp version specified.

**Repository links:**
- https://github.com/mono/SkiaSharp/files/2901551/SkiaPanAndZoom.zip — Sample project SkiaPanAndZoom.zip demonstrating custom pan/zoom SKGLView

## Analysis

### Technical Summary

The performance gap between iOS and Android for SKGLView is largely architectural: iOS GLKView integrates with Core Animation for low-latency compositing while Android GLSurfaceView requires additional compositing overhead via SurfaceFlinger. Additionally, re-tessellating 10,000+ point paths on every frame during pan is inherently expensive regardless of whether GL or CPU rendering is used.

### Rationale

The reporter explicitly asks 'have I missed something' and requests profiling advice — this is a usage/performance question, not a bug report. The iOS SKGLView source shows 4x MSAA enabled via GLKView, while Android SKGLSurfaceView uses a plain GLSurfaceView with no multisampling. The performance difference is architectural. Missing SkiaSharp version and Android API level prevent confirming whether newer releases addressed any related improvements.

### Key Signals

- "I expect the performance to be much better than the SKCanvasView - and it is on iOS, but not on Android." — **issue body** (GL rendering helps on iOS but not Android — points to platform-specific GPU architecture differences rather than SkiaSharp API misuse.)
- "Currently, I have a control that derives from SKCanvasView and one that derives from SKGLView (SKGLZoomView). The example uses by default the SKGLView." — **comment by APopatanasov** (Reporter confirmed they ARE using SKGLView on Android — the GL rendering path is being exercised correctly.)
- "paths that contain a great amount of points (more than 10000)" — **issue body** (10,000+ points per path rendered per-frame during pan is high CPU/GPU load. Path tessellation cost scales with point count and is not automatically optimized by the GL backend.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceView.cs` | 25-30 | direct | Android SKGLSurfaceView initializes with SetEGLContextClientVersion(2) and SetEGLConfigChooser(8,8,8,8,0,8) — depth buffer is 0, no multisampling configured. Uses GLSurfaceView, a separate Android Surface that requires SurfaceFlinger compositing on every frame. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKGLView.cs` | 86-101 | direct | iOS SKGLView initializes with DrawableMultisample = GLKViewDrawableMultisample.Sample4x (4x MSAA) and uses GLKView which integrates directly with Core Animation layers — frames are composited with near-zero overhead compared to Android's SurfaceFlinger pipeline. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceViewRenderer.cs` | 32-87 | related | OnDrawFrame clears all buffers (color+depth+stencil) every frame and performs full path re-tessellation via Skia on each call. No dirty-region tracking or path caching is built into the renderer. Two flush calls (canvas.Flush + context.Flush) are issued per frame. |

### Workarounds

- Cache rendered paths to an off-screen SKSurface/SKBitmap during pan start, draw the cached bitmap with a transform during pan, and re-render full detail on gesture end
- Pre-process paths using a point decimation algorithm (e.g., Ramer-Douglas-Peucker) to reduce 10,000+ points to under 1,000 without significant visual difference at typical zoom levels
- Throttle Invalidate() calls with a frame-rate limiter or only redraw when the transform delta exceeds a minimum threshold
- Use SKGLTextureView instead of SKGLSurfaceView on Android — TextureView allows partial updates and compositing within the View hierarchy, which can reduce overhead for some use cases

### Next Questions

- Which SkiaSharp NuGet package version is being used?
- What is the Xamarin.Android / .NET for Android version and Android API level of test devices?
- Is the lag during active panning gestures or also when the view is stationary after panning stops?
- Is the full path always redrawn on every frame, or only when the transform changes?

### Resolution Proposals

**Hypothesis:** Android GL performance lags iOS due to architectural differences (GLSurfaceView compositing overhead vs GLKView Core Animation integration) combined with the high cost of re-tessellating 10,000+ point paths on every frame. Path caching during pan is the most impactful optimization.

1. **Cache path to off-screen surface during pan** — workaround, confidence 0.85 (85%), cost/m, validated=untested
   - Render the full high-detail path to an off-screen SKSurface once when pan starts. During active pan/zoom, draw the cached surface with the current transform instead of re-tessellating all paths every frame. Trigger a full re-render only when the gesture ends or zoom level changes significantly.
2. **Reduce path point count with Douglas-Peucker decimation** — workaround, confidence 0.80 (80%), cost/s, validated=untested
   - Pre-process paths before adding to SKPath using a point decimation algorithm. 10,000+ points is often reducible to <1,000 points with no perceptible visual difference at typical zoom levels. Apply more aggressive decimation at lower zoom levels.
3. **Throttle frame invalidation rate** — workaround, confidence 0.75 (75%), cost/xs, validated=untested
   - Avoid calling Invalidate() on every touch event. Cap rendering to ~30fps using a timer or invalidate only when the cumulative transform delta exceeds a minimum threshold.

**Recommended proposal:** Cache path to off-screen surface during pan

**Why:** Highest-impact optimization for pan/zoom: eliminates per-frame path re-tessellation by trading one expensive draw for one cheap bitmap blit per frame during the gesture.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.75 (75%) |
| Reason | Performance question missing SkiaSharp version and Android API level. Architectural explanation and workarounds can be provided, but version info would help confirm whether newer releases address any related improvements. |
| Suggested repro platform | linux |

### Missing Info

- SkiaSharp NuGet package version
- Xamarin.Android or .NET for Android version
- Android API level of test devices
- Whether lag occurs during active panning or also when the view is stationary

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply question, views, Android, iOS, OpenGL, and performance labels | labels=type/question, area/SkiaSharp.Views, os/Android, os/iOS, backend/OpenGL, tenet/performance |
| add-comment | medium | 0.75 (75%) | Post explanation of Android vs iOS GL performance architectural differences and suggest workarounds, while requesting missing version info | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and sample project!

The performance difference between Android and iOS for `SKGLView` is largely architectural:

1. **iOS `GLKView`** integrates directly with Core Animation layers, so rendered frames are composited with near-zero overhead. The iOS `SKGLView` also enables 4× MSAA which is highly optimized on Apple GPUs.
2. **Android `GLSurfaceView`** renders to a separate `Surface` that Android's SurfaceFlinger must composite on every frame — this adds measurable latency compared to iOS, especially during scrolling/panning.
3. **Path tessellation cost** scales with point count. Re-tessellating 10,000+ points per frame is expensive on any platform, and Android GPUs (Adreno on Pixel/Nexus 5X) have different tessellation performance characteristics than Apple Silicon.

**Suggested optimizations:**

- **Cache during pan**: Render the full detail to an off-screen `SKSurface` once when the gesture starts. During active pan/zoom, draw the cached surface with the current transform instead of re-tessellating all paths on every frame. Re-render at full quality when the gesture ends.
- **Reduce path complexity**: Pre-process paths using a point decimation algorithm (e.g., Ramer-Douglas-Peucker). 10,000+ points is often reducible to fewer than 1,000 with no perceptible visual difference at normal zoom levels.
- **Throttle invalidation**: Only call `Invalidate()` when the transform delta exceeds a minimum threshold rather than on every touch event.

Could you also share:
- Your SkiaSharp NuGet package version?
- Your Xamarin.Android version and the Android API level of your test devices?
- Whether the lag occurs during active panning or also when the view is stationary after panning stops?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 798,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T18:46:12Z",
    "currentLabels": [
      "type/question"
    ]
  },
  "summary": "Reporter asks why SKGLView has poor performance on Android compared to iOS when rendering paths with 10,000+ points during pan/zoom interactions, providing a sample project for reference.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.85
    },
    "platforms": [
      "os/Android",
      "os/iOS"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a custom SKGLView supporting pan and zoom",
        "Render paths containing 10,000+ points",
        "Test on Android device (Pixel or Nexus 5X)",
        "Compare to iOS where performance is significantly better"
      ],
      "environmentDetails": "Android devices: Pixel and Nexus 5X (both Qualcomm Adreno GPU). No SkiaSharp version specified.",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/2901551/SkiaPanAndZoom.zip",
          "description": "Sample project SkiaPanAndZoom.zip demonstrating custom pan/zoom SKGLView"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The performance gap between iOS and Android for SKGLView is largely architectural: iOS GLKView integrates with Core Animation for low-latency compositing while Android GLSurfaceView requires additional compositing overhead via SurfaceFlinger. Additionally, re-tessellating 10,000+ point paths on every frame during pan is inherently expensive regardless of whether GL or CPU rendering is used.",
    "rationale": "The reporter explicitly asks 'have I missed something' and requests profiling advice — this is a usage/performance question, not a bug report. The iOS SKGLView source shows 4x MSAA enabled via GLKView, while Android SKGLSurfaceView uses a plain GLSurfaceView with no multisampling. The performance difference is architectural. Missing SkiaSharp version and Android API level prevent confirming whether newer releases addressed any related improvements.",
    "keySignals": [
      {
        "text": "I expect the performance to be much better than the SKCanvasView - and it is on iOS, but not on Android.",
        "source": "issue body",
        "interpretation": "GL rendering helps on iOS but not Android — points to platform-specific GPU architecture differences rather than SkiaSharp API misuse."
      },
      {
        "text": "Currently, I have a control that derives from SKCanvasView and one that derives from SKGLView (SKGLZoomView). The example uses by default the SKGLView.",
        "source": "comment by APopatanasov",
        "interpretation": "Reporter confirmed they ARE using SKGLView on Android — the GL rendering path is being exercised correctly."
      },
      {
        "text": "paths that contain a great amount of points (more than 10000)",
        "source": "issue body",
        "interpretation": "10,000+ points per path rendered per-frame during pan is high CPU/GPU load. Path tessellation cost scales with point count and is not automatically optimized by the GL backend."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceView.cs",
        "lines": "25-30",
        "finding": "Android SKGLSurfaceView initializes with SetEGLContextClientVersion(2) and SetEGLConfigChooser(8,8,8,8,0,8) — depth buffer is 0, no multisampling configured. Uses GLSurfaceView, a separate Android Surface that requires SurfaceFlinger compositing on every frame.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKGLView.cs",
        "lines": "86-101",
        "finding": "iOS SKGLView initializes with DrawableMultisample = GLKViewDrawableMultisample.Sample4x (4x MSAA) and uses GLKView which integrates directly with Core Animation layers — frames are composited with near-zero overhead compared to Android's SurfaceFlinger pipeline.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceViewRenderer.cs",
        "lines": "32-87",
        "finding": "OnDrawFrame clears all buffers (color+depth+stencil) every frame and performs full path re-tessellation via Skia on each call. No dirty-region tracking or path caching is built into the renderer. Two flush calls (canvas.Flush + context.Flush) are issued per frame.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Cache rendered paths to an off-screen SKSurface/SKBitmap during pan start, draw the cached bitmap with a transform during pan, and re-render full detail on gesture end",
      "Pre-process paths using a point decimation algorithm (e.g., Ramer-Douglas-Peucker) to reduce 10,000+ points to under 1,000 without significant visual difference at typical zoom levels",
      "Throttle Invalidate() calls with a frame-rate limiter or only redraw when the transform delta exceeds a minimum threshold",
      "Use SKGLTextureView instead of SKGLSurfaceView on Android — TextureView allows partial updates and compositing within the View hierarchy, which can reduce overhead for some use cases"
    ],
    "nextQuestions": [
      "Which SkiaSharp NuGet package version is being used?",
      "What is the Xamarin.Android / .NET for Android version and Android API level of test devices?",
      "Is the lag during active panning gestures or also when the view is stationary after panning stops?",
      "Is the full path always redrawn on every frame, or only when the transform changes?"
    ],
    "resolution": {
      "hypothesis": "Android GL performance lags iOS due to architectural differences (GLSurfaceView compositing overhead vs GLKView Core Animation integration) combined with the high cost of re-tessellating 10,000+ point paths on every frame. Path caching during pan is the most impactful optimization.",
      "proposals": [
        {
          "title": "Cache path to off-screen surface during pan",
          "description": "Render the full high-detail path to an off-screen SKSurface once when pan starts. During active pan/zoom, draw the cached surface with the current transform instead of re-tessellating all paths every frame. Trigger a full re-render only when the gesture ends or zoom level changes significantly.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Reduce path point count with Douglas-Peucker decimation",
          "description": "Pre-process paths before adding to SKPath using a point decimation algorithm. 10,000+ points is often reducible to <1,000 points with no perceptible visual difference at typical zoom levels. Apply more aggressive decimation at lower zoom levels.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Throttle frame invalidation rate",
          "description": "Avoid calling Invalidate() on every touch event. Cap rendering to ~30fps using a timer or invalidate only when the cumulative transform delta exceeds a minimum threshold.",
          "category": "workaround",
          "confidence": 0.75,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Cache path to off-screen surface during pan",
      "recommendedReason": "Highest-impact optimization for pan/zoom: eliminates per-frame path re-tessellation by trading one expensive draw for one cheap bitmap blit per frame during the gesture."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.75,
      "reason": "Performance question missing SkiaSharp version and Android API level. Architectural explanation and workarounds can be provided, but version info would help confirm whether newer releases address any related improvements.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "SkiaSharp NuGet package version",
      "Xamarin.Android or .NET for Android version",
      "Android API level of test devices",
      "Whether lag occurs during active panning or also when the view is stationary"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question, views, Android, iOS, OpenGL, and performance labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp.Views",
          "os/Android",
          "os/iOS",
          "backend/OpenGL",
          "tenet/performance"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post explanation of Android vs iOS GL performance architectural differences and suggest workarounds, while requesting missing version info",
        "risk": "medium",
        "confidence": 0.75,
        "comment": "Thanks for the detailed report and sample project!\n\nThe performance difference between Android and iOS for `SKGLView` is largely architectural:\n\n1. **iOS `GLKView`** integrates directly with Core Animation layers, so rendered frames are composited with near-zero overhead. The iOS `SKGLView` also enables 4× MSAA which is highly optimized on Apple GPUs.\n2. **Android `GLSurfaceView`** renders to a separate `Surface` that Android's SurfaceFlinger must composite on every frame — this adds measurable latency compared to iOS, especially during scrolling/panning.\n3. **Path tessellation cost** scales with point count. Re-tessellating 10,000+ points per frame is expensive on any platform, and Android GPUs (Adreno on Pixel/Nexus 5X) have different tessellation performance characteristics than Apple Silicon.\n\n**Suggested optimizations:**\n\n- **Cache during pan**: Render the full detail to an off-screen `SKSurface` once when the gesture starts. During active pan/zoom, draw the cached surface with the current transform instead of re-tessellating all paths on every frame. Re-render at full quality when the gesture ends.\n- **Reduce path complexity**: Pre-process paths using a point decimation algorithm (e.g., Ramer-Douglas-Peucker). 10,000+ points is often reducible to fewer than 1,000 with no perceptible visual difference at normal zoom levels.\n- **Throttle invalidation**: Only call `Invalidate()` when the transform delta exceeds a minimum threshold rather than on every touch event.\n\nCould you also share:\n- Your SkiaSharp NuGet package version?\n- Your Xamarin.Android version and the Android API level of your test devices?\n- Whether the lag occurs during active panning or also when the view is stationary after panning stops?"
      }
    ]
  }
}
```

</details>
