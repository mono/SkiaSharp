# Issue Triage Report — #533

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T21:09:45Z |
| Type | type/enhancement (0.82 (82%)) |
| Area | area/SkiaSharp.Views (0.97 (97%)) |
| Suggested action | keep-open (0.85 (85%)) |

**Issue Summary:** After upgrading SkiaSharp.Views.Forms from 1.59.3 to 1.60.1 on Android, SKGLView with HasRenderLoop=true fires PaintSurface at uncapped framerate (300–1200fps) instead of the previous ~60fps, because the switch from GLSurfaceView (V-Sync enforced) to GLTextureView (no V-Sync) removed the implicit frame rate cap.

**Analysis:** The implicit 60fps cap from GLSurfaceView's V-Sync was lost when SkiaSharp switched to the custom GLTextureView (TextureView-based) renderer. The fix would be integrating Android's Choreographer API to synchronize render loop callbacks to display VSYNC, or exposing a frame-rate limit API.

**Recommendations:** **keep-open** — Valid enhancement with sustained community interest over 8 years, multiple affected users, and a clear implementation path (Choreographer). The maintainer has already investigated and acknowledged the issue. No fix has been merged.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.Views |
| Platforms | os/Android |
| Backends | backend/OpenGL |
| Tenets | tenet/performance, tenet/reliability |
| Partner | — |
| Current labels | type/bug, type/enhancement, status/help-wanted, os/Android, area/SkiaSharp.Views, backend/OpenGL |

## Evidence

### Reproduction

1. Create a Xamarin.Forms app using SkiaSharp.Views.Forms
2. Add an SKGLView with HasRenderLoop=true
3. Measure PaintSurface call frequency via Stopwatch
4. Observe ~300fps on Android after upgrading from 1.59.3 to 1.60.1

**Environment:** Android 7 API 24, Samsung Galaxy S7 Edge, Xamarin.Forms 3.0.0.446417, SkiaSharp.Views.Forms 1.60.1

**Code snippets:**

```csharp
Stopwatch sw = Stopwatch.StartNew(); TimeSpan last; private void OnPaint(object sender, SKPaintGLSurfaceEventArgs e) { var c = sw.Elapsed; var ts = c - last; last = c; var fps = 1.0 / ts.TotalSeconds; }
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.59.3, 1.60.1 |
| Worked in | 1.59.3 |
| Broke in | 1.60.1 |
| Current relevance | likely |
| Relevance reason | The GLTextureView (TextureView-based) renderer is still the Android implementation in current source. No Choreographer-based frame pacing has been added. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.90 (90%) |
| Reason | The maintainer confirmed the change: 1.59.3 used GLSurfaceView (inherits V-Sync behavior), 1.60.1 switched to GLTextureView (TextureView-based, no V-Sync). The frame rate cap was an implicit behavior of GLSurfaceView that was lost. |
| Worked in version | 1.59.3 |
| Broke in version | 1.60.1 |

## Analysis

### Technical Summary

The implicit 60fps cap from GLSurfaceView's V-Sync was lost when SkiaSharp switched to the custom GLTextureView (TextureView-based) renderer. The fix would be integrating Android's Choreographer API to synchronize render loop callbacks to display VSYNC, or exposing a frame-rate limit API.

### Rationale

The switch from GLSurfaceView (with implicit VSYNC via EGL swap interval) to GLTextureView (no VSYNC) was intentional for better rendering capabilities, but it inadvertently removed the frame rate cap for HasRenderLoop users. This is correctly classified as type/enhancement rather than type/bug because: (1) the maintainer confirmed the architectural change was intentional, (2) SkiaSharp does not contract to cap frame rate — that was incidental GLSurfaceView behavior, and (3) the resolution requires adding new functionality (Choreographer integration or frame rate API). The issue remains relevant as current code still has no frame pacing. Multiple users are affected and community workarounds exist.

### Key Signals

- "framerate went from a capped 60fps to a ~300fps" — **issue body** (Confirms a clear behavioral regression introduced by the GLSurfaceView → GLTextureView migration.)
- "I _see_ the change, and I think it has to do with the fact that we are no longer using the old GLSurfaceView to render the content, but the new TextureView" — **maintainer comment #392336322** (Maintainer confirmed root cause: TextureView does not enforce VSYNC unlike GLSurfaceView.)
- "I really need to look at this. Noticing 1.2K FPS in some emulators." — **maintainer comment #664105794** (Issue acknowledged as real and still open after 2 years, confirming it is not resolved.)
- "GLSurfaceView probably uses V-Sync (glSwapInterval call), when TextureView doesn't use that" — **community comment #664335967** (Community correctly identified the V-Sync mechanism difference between the two view types.)
- "We might be able to look at Choreographer" — **maintainer comment #664527988** (Maintainer proposed Choreographer as the solution to sync rendering to display refresh rate.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/GLTextureView.cs` | 147-217 | direct | GLTextureView exposes RenderMode (Continuously/WhenDirty) via glThread.SetRenderMode, but has no VSYNC or Choreographer integration. In Continuously mode, the GL thread renders as fast as the GPU allows with no frame rate cap. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceView.cs` | 9-60 | direct | SKGLSurfaceView wraps Android's GLSurfaceView which natively enforces VSYNC via EGL swap interval, providing the implicit 60fps cap that existed in SkiaSharp 1.59.3. This is the pre-1.60.1 implementation. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLTextureView.cs` | 36-71 | direct | SKGLTextureView wraps GLTextureView and uses SKGLTextureViewRenderer. The Initialize() method does not configure any VSYNC or frame rate limiting — rendering is driven by a background GL thread that runs continuously without any display sync. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLTextureViewRenderer.cs` | 43-98 | direct | OnDrawFrame() renders immediately each time it is called by the GL thread with no rate limiting logic. The entire render path has no Choreographer or VSYNC synchronization. |

### Workarounds

- Use a time-delta based animation model instead of frame-count based — track elapsed time between PaintSurface calls with a Stopwatch and scale animations by time.
- Implement a manual frame rate limiter using Thread.Sleep: measure frame execution time and sleep for the remaining interval to hit a target FPS (e.g., 16.67ms for 60fps).
- Use Android's Choreographer in a custom subclass to only call RequestRender() on each display VSYNC tick instead of relying on Continuously render mode.

### Next Questions

- Does the same uncapped FPS issue affect SkiaSharp.Views.Maui on Android (SKGLViewHandler.Android)?
- Has the Android Frame Pacing Library been considered as an alternative to raw Choreographer?
- Is there appetite to add a MaxFPS or UseVSync property to SKGLView for Android?

### Resolution Proposals

**Hypothesis:** Add Choreographer-based VSYNC synchronization to GLTextureView's GL thread so that rendering callbacks align with display refresh rate, preventing >60fps (or >screen-refresh-rate) rendering.

1. **Use time-based animation (user workaround)** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - The immediate workaround: track elapsed time between PaintSurface calls with Stopwatch and scale all animations by time delta rather than frame count.
2. **Manual frame rate limiter (user workaround)** — workaround, confidence 0.85 (85%), cost/s, validated=untested
   - Measure frame duration and use Thread.Sleep to limit the render loop to a target FPS. Simple but imprecise due to sleep granularity.
3. **Choreographer integration in GLTextureView** — fix, confidence 0.75 (75%), cost/m, validated=untested
   - Integrate Android's Choreographer.IFrameCallback into GLTextureView's GL thread to trigger frames only on display VSYNC signals, naturally capping at the device's screen refresh rate.

**Recommended proposal:** Choreographer integration in GLTextureView

**Why:** Aligns rendering with the actual display refresh cycle, avoids wasted GPU/CPU cycles, is the approach already identified by the maintainer, and provides the best battery efficiency for render-loop use cases.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.85 (85%) |
| Reason | Valid enhancement with sustained community interest over 8 years, multiple affected users, and a clear implementation path (Choreographer). The maintainer has already investigated and acknowledged the issue. No fix has been merged. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.82 (82%) | Replace type/bug with type/enhancement — the original behavior change was intentional; the ask is adding frame-pacing capability | labels=type/enhancement, area/SkiaSharp.Views, os/Android, backend/OpenGL, tenet/performance, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Provide context on root cause, current status, and workarounds for affected users | — |

**Comment draft for `add-comment`:**

```markdown
**Root cause confirmed:** The switch from `GLSurfaceView` (which enforces V-Sync via EGL swap interval) to the custom `GLTextureView` in 1.60.1 removed the implicit 60fps cap. The GL thread in `GLTextureView` runs continuously with no display-sync.

**Status:** No Choreographer integration has been added yet. The maintainer identified this in 2020 and it is still open.

**Workarounds available now:**

1. **Time-based animations (recommended):** Instead of frame-count-based logic, track elapsed time:
   ```csharp
   private Stopwatch _sw = Stopwatch.StartNew();
   private TimeSpan _last;
   void OnPaintSurface(object s, SKPaintGLSurfaceEventArgs e) {
       var elapsed = _sw.Elapsed - _last;
       _last = _sw.Elapsed;
       // Use elapsed.TotalSeconds to scale animations
   }
   ```

2. **Manual FPS cap:** Add a sleep in your render callback to limit FPS:
   ```csharp
   private Stopwatch _sw = Stopwatch.StartNew();
   private TimeSpan _last;
   private const double TargetMs = 1000.0 / 60.0;
   void OnPaintSurface(object s, SKPaintGLSurfaceEventArgs e) {
       // ... draw ...
       var elapsed = (_sw.Elapsed - _last).TotalMilliseconds;
       _last = _sw.Elapsed;
       var wait = TargetMs - elapsed;
       if (wait > 0) Thread.Sleep((int)wait);
   }
   ```

A proper fix would integrate Android's `Choreographer` to trigger renders only on display VSYNC, naturally capping at the screen's refresh rate and reducing battery usage.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 533,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T21:09:45Z",
    "currentLabels": [
      "type/bug",
      "type/enhancement",
      "status/help-wanted",
      "os/Android",
      "area/SkiaSharp.Views",
      "backend/OpenGL"
    ]
  },
  "summary": "After upgrading SkiaSharp.Views.Forms from 1.59.3 to 1.60.1 on Android, SKGLView with HasRenderLoop=true fires PaintSurface at uncapped framerate (300–1200fps) instead of the previous ~60fps, because the switch from GLSurfaceView (V-Sync enforced) to GLTextureView (no V-Sync) removed the implicit frame rate cap.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.82
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.97
    },
    "platforms": [
      "os/Android"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "tenets": [
      "tenet/performance",
      "tenet/reliability"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Xamarin.Forms app using SkiaSharp.Views.Forms",
        "Add an SKGLView with HasRenderLoop=true",
        "Measure PaintSurface call frequency via Stopwatch",
        "Observe ~300fps on Android after upgrading from 1.59.3 to 1.60.1"
      ],
      "environmentDetails": "Android 7 API 24, Samsung Galaxy S7 Edge, Xamarin.Forms 3.0.0.446417, SkiaSharp.Views.Forms 1.60.1",
      "codeSnippets": [
        "Stopwatch sw = Stopwatch.StartNew(); TimeSpan last; private void OnPaint(object sender, SKPaintGLSurfaceEventArgs e) { var c = sw.Elapsed; var ts = c - last; last = c; var fps = 1.0 / ts.TotalSeconds; }"
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.59.3",
        "1.60.1"
      ],
      "workedIn": "1.59.3",
      "brokeIn": "1.60.1",
      "currentRelevance": "likely",
      "relevanceReason": "The GLTextureView (TextureView-based) renderer is still the Android implementation in current source. No Choreographer-based frame pacing has been added."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.9,
      "reason": "The maintainer confirmed the change: 1.59.3 used GLSurfaceView (inherits V-Sync behavior), 1.60.1 switched to GLTextureView (TextureView-based, no V-Sync). The frame rate cap was an implicit behavior of GLSurfaceView that was lost.",
      "workedInVersion": "1.59.3",
      "brokeInVersion": "1.60.1"
    }
  },
  "analysis": {
    "summary": "The implicit 60fps cap from GLSurfaceView's V-Sync was lost when SkiaSharp switched to the custom GLTextureView (TextureView-based) renderer. The fix would be integrating Android's Choreographer API to synchronize render loop callbacks to display VSYNC, or exposing a frame-rate limit API.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/GLTextureView.cs",
        "lines": "147-217",
        "finding": "GLTextureView exposes RenderMode (Continuously/WhenDirty) via glThread.SetRenderMode, but has no VSYNC or Choreographer integration. In Continuously mode, the GL thread renders as fast as the GPU allows with no frame rate cap.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceView.cs",
        "lines": "9-60",
        "finding": "SKGLSurfaceView wraps Android's GLSurfaceView which natively enforces VSYNC via EGL swap interval, providing the implicit 60fps cap that existed in SkiaSharp 1.59.3. This is the pre-1.60.1 implementation.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLTextureView.cs",
        "lines": "36-71",
        "finding": "SKGLTextureView wraps GLTextureView and uses SKGLTextureViewRenderer. The Initialize() method does not configure any VSYNC or frame rate limiting — rendering is driven by a background GL thread that runs continuously without any display sync.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLTextureViewRenderer.cs",
        "lines": "43-98",
        "finding": "OnDrawFrame() renders immediately each time it is called by the GL thread with no rate limiting logic. The entire render path has no Choreographer or VSYNC synchronization.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "framerate went from a capped 60fps to a ~300fps",
        "source": "issue body",
        "interpretation": "Confirms a clear behavioral regression introduced by the GLSurfaceView → GLTextureView migration."
      },
      {
        "text": "I _see_ the change, and I think it has to do with the fact that we are no longer using the old GLSurfaceView to render the content, but the new TextureView",
        "source": "maintainer comment #392336322",
        "interpretation": "Maintainer confirmed root cause: TextureView does not enforce VSYNC unlike GLSurfaceView."
      },
      {
        "text": "I really need to look at this. Noticing 1.2K FPS in some emulators.",
        "source": "maintainer comment #664105794",
        "interpretation": "Issue acknowledged as real and still open after 2 years, confirming it is not resolved."
      },
      {
        "text": "GLSurfaceView probably uses V-Sync (glSwapInterval call), when TextureView doesn't use that",
        "source": "community comment #664335967",
        "interpretation": "Community correctly identified the V-Sync mechanism difference between the two view types."
      },
      {
        "text": "We might be able to look at Choreographer",
        "source": "maintainer comment #664527988",
        "interpretation": "Maintainer proposed Choreographer as the solution to sync rendering to display refresh rate."
      }
    ],
    "rationale": "The switch from GLSurfaceView (with implicit VSYNC via EGL swap interval) to GLTextureView (no VSYNC) was intentional for better rendering capabilities, but it inadvertently removed the frame rate cap for HasRenderLoop users. This is correctly classified as type/enhancement rather than type/bug because: (1) the maintainer confirmed the architectural change was intentional, (2) SkiaSharp does not contract to cap frame rate — that was incidental GLSurfaceView behavior, and (3) the resolution requires adding new functionality (Choreographer integration or frame rate API). The issue remains relevant as current code still has no frame pacing. Multiple users are affected and community workarounds exist.",
    "workarounds": [
      "Use a time-delta based animation model instead of frame-count based — track elapsed time between PaintSurface calls with a Stopwatch and scale animations by time.",
      "Implement a manual frame rate limiter using Thread.Sleep: measure frame execution time and sleep for the remaining interval to hit a target FPS (e.g., 16.67ms for 60fps).",
      "Use Android's Choreographer in a custom subclass to only call RequestRender() on each display VSYNC tick instead of relying on Continuously render mode."
    ],
    "nextQuestions": [
      "Does the same uncapped FPS issue affect SkiaSharp.Views.Maui on Android (SKGLViewHandler.Android)?",
      "Has the Android Frame Pacing Library been considered as an alternative to raw Choreographer?",
      "Is there appetite to add a MaxFPS or UseVSync property to SKGLView for Android?"
    ],
    "resolution": {
      "hypothesis": "Add Choreographer-based VSYNC synchronization to GLTextureView's GL thread so that rendering callbacks align with display refresh rate, preventing >60fps (or >screen-refresh-rate) rendering.",
      "proposals": [
        {
          "title": "Use time-based animation (user workaround)",
          "description": "The immediate workaround: track elapsed time between PaintSurface calls with Stopwatch and scale all animations by time delta rather than frame count.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Manual frame rate limiter (user workaround)",
          "description": "Measure frame duration and use Thread.Sleep to limit the render loop to a target FPS. Simple but imprecise due to sleep granularity.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Choreographer integration in GLTextureView",
          "description": "Integrate Android's Choreographer.IFrameCallback into GLTextureView's GL thread to trigger frames only on display VSYNC signals, naturally capping at the device's screen refresh rate.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Choreographer integration in GLTextureView",
      "recommendedReason": "Aligns rendering with the actual display refresh cycle, avoids wasted GPU/CPU cycles, is the approach already identified by the maintainer, and provides the best battery efficiency for render-loop use cases."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.85,
      "reason": "Valid enhancement with sustained community interest over 8 years, multiple affected users, and a clear implementation path (Choreographer). The maintainer has already investigated and acknowledged the issue. No fix has been merged.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Replace type/bug with type/enhancement — the original behavior change was intentional; the ask is adding frame-pacing capability",
        "risk": "low",
        "confidence": 0.82,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp.Views",
          "os/Android",
          "backend/OpenGL",
          "tenet/performance",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Provide context on root cause, current status, and workarounds for affected users",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "**Root cause confirmed:** The switch from `GLSurfaceView` (which enforces V-Sync via EGL swap interval) to the custom `GLTextureView` in 1.60.1 removed the implicit 60fps cap. The GL thread in `GLTextureView` runs continuously with no display-sync.\n\n**Status:** No Choreographer integration has been added yet. The maintainer identified this in 2020 and it is still open.\n\n**Workarounds available now:**\n\n1. **Time-based animations (recommended):** Instead of frame-count-based logic, track elapsed time:\n   ```csharp\n   private Stopwatch _sw = Stopwatch.StartNew();\n   private TimeSpan _last;\n   void OnPaintSurface(object s, SKPaintGLSurfaceEventArgs e) {\n       var elapsed = _sw.Elapsed - _last;\n       _last = _sw.Elapsed;\n       // Use elapsed.TotalSeconds to scale animations\n   }\n   ```\n\n2. **Manual FPS cap:** Add a sleep in your render callback to limit FPS:\n   ```csharp\n   private Stopwatch _sw = Stopwatch.StartNew();\n   private TimeSpan _last;\n   private const double TargetMs = 1000.0 / 60.0;\n   void OnPaintSurface(object s, SKPaintGLSurfaceEventArgs e) {\n       // ... draw ...\n       var elapsed = (_sw.Elapsed - _last).TotalMilliseconds;\n       _last = _sw.Elapsed;\n       var wait = TargetMs - elapsed;\n       if (wait > 0) Thread.Sleep((int)wait);\n   }\n   ```\n\nA proper fix would integrate Android's `Choreographer` to trigger renders only on display VSYNC, naturally capping at the screen's refresh rate and reducing battery usage."
      }
    ]
  }
}
```

</details>
