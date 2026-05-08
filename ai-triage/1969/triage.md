# Issue Triage Report — #1969

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T22:24:00Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp.Views (0.88 (88%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** SKGLView on iOS renders incomplete drawings when ClipRoundRect is combined with near-integer float coordinates (e.g., 199.999f) — bug present from v1.68.1 through v2.80.3, does not reproduce on Android, WinForms, or with CPU rendering.

**Analysis:** GPU clip precision bug on iOS: SKGLView uses OpenGLES2 with 4x MSAA (DrawableMultisample.Sample4x) and stencil-based clip (DrawableStencilFormat.Format8). ClipRoundRect triggers GPU tessellation of the rounded-rect clip path. When a subsequent DrawRect has a top coordinate extremely close to (but below) an integer boundary (199.999f ≈ 200.0), floating-point precision differences in Skia's GPU stencil clip path can cause the draw to be incorrectly excluded from the clip region. This does not occur on CPU because the CPU rasterizer uses a different precision path.

**Recommendations:** **needs-investigation** — Complete minimal repro, confirmed on all iOS devices/simulators across many versions, clear GPU-specific trigger. Root cause is likely in Skia's GPU stencil clip path — needs investigation into whether MSAA is the contributing factor and whether this is an upstream Skia bug.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/iOS |
| Backends | backend/OpenGL |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Create an iOS app with SKGLView
2. In OnPaintSurface, call canvas.ClipRoundRect with a 10-unit radius
3. Call canvas.DrawRect with a rect whose top coordinate is 199.999f
4. Run on any iOS device or Simulator (iOS 9–15)

**Environment:** iOS 9–15, iPhone, iPad, Simulator. SkiaSharp 1.68.1–2.80.3. Visual Studio.

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1969 — Original issue with screenshots and minimal repro

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Yellow rectangle missing from rendered output when DrawRect uses y=199.999f after ClipRoundRect |
| Repro quality | complete |
| Target frameworks | net6.0-ios, xamarin.ios |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.1, 2.80.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | No known good version — bug present from very old versions to latest tested. The iOS SKGLView code uses OpenGLES2 with 4x MSAA and stencil clipping; this path hasn't fundamentally changed. |

## Analysis

### Technical Summary

GPU clip precision bug on iOS: SKGLView uses OpenGLES2 with 4x MSAA (DrawableMultisample.Sample4x) and stencil-based clip (DrawableStencilFormat.Format8). ClipRoundRect triggers GPU tessellation of the rounded-rect clip path. When a subsequent DrawRect has a top coordinate extremely close to (but below) an integer boundary (199.999f ≈ 200.0), floating-point precision differences in Skia's GPU stencil clip path can cause the draw to be incorrectly excluded from the clip region. This does not occur on CPU because the CPU rasterizer uses a different precision path.

### Rationale

Strong bug signal: minimal, self-contained repro provided, confirmed on all iOS devices and simulators, iOS 9–15, multiple SkiaSharp versions. Issue is GPU-only (disappears on CPU), specific to round-rect clip (disappears with radius=0), and specific to near-integer float coordinates (disappears with integer 200). The stencil-based clip implementation in Skia's GPU backend (SkGpuDevice / GrClipStack) is the most plausible root cause — precision edge cases in tessellation of the clip polygon boundary.

### Key Signals

- "switch to CPU rendering (SKCanvasView), the bug disappears" — **issue body** (Root cause is in GPU code path, not in the SkiaSharp C# wrapper or the drawing API.)
- "change the radius of the round-rect clipping to 0 (not rounded anymore), the bug disappears" — **issue body** (Rounded-rect tessellation is required to trigger the bug — axis-aligned rect clip works fine.)
- "change the coordinate 199.999f to 200, the bug disappears" — **issue body** (Floating-point precision at the boundary of a clip segment is the trigger — classic off-by-epsilon in stencil clip.)
- "could not reproduce it with Android (SkiaSharp.Views.Android.SKGLSurfaceView) nor with WinForms (SkiaSharp.Views.Desktop.SKGLControl)" — **issue body** (iOS-specific — likely due to iOS Metal/OpenGLES driver behavior or iOS-specific 4x MSAA multisampled stencil path.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKGLView.cs` | 86-98 | direct | Initialize() sets DrawableMultisample = GLKViewDrawableMultisample.Sample4x and DrawableStencilFormat = GLKViewDrawableStencilFormat.Format8. The 4x MSAA multisampled stencil buffer is used for GPU clip path evaluation, which is where floating-point edge cases manifest. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKGLView.cs` | 130-145 | direct | DrawInRect reads GL_STENCIL_BITS and GL_SAMPLES from the framebuffer and passes them to GRBackendRenderTarget. Sample count is capped by context.GetMaxSurfaceSampleCount(colorType). This is the correct setup for MSAA, but multisampled stencil clip on iOS OpenGLES may behave differently from other platforms. |

### Workarounds

- Switch from SKGLView to SKCanvasView (CPU rendering) — eliminates the bug but sacrifices GPU acceleration.
- Round all DrawRect coordinates to integers before drawing (e.g., use Math.Round or (float)Math.Floor) — avoids the precision edge case.
- Use a non-rounded clip rectangle (radius = 0) if the visual design permits.

### Next Questions

- Does the bug reproduce with SKMetalView on iOS (Metal backend) or only with the deprecated OpenGLES SKGLView?
- Is the bug present in Skia upstream (chromium/skia) with the same coordinate pattern on iOS Metal/OpenGLES?
- Does disabling MSAA (DrawableMultisample = None) eliminate the bug?

### Resolution Proposals

**Hypothesis:** Skia's GPU stencil clip path tessellates the rounded-rect boundary; near-integer float coordinates (199.999f) fall on or very near a stencil sample boundary, causing a precision error that excludes subsequent draws. The 4x MSAA multisampled stencil on iOS may amplify this edge case.

1. **Investigate disabling MSAA or changing MSAA mode in SKGLView** — investigation, confidence 0.65 (65%), cost/xs, validated=untested
   - Try setting DrawableMultisample = GLKViewDrawableMultisample.None in Initialize() to see if this eliminates the stencil precision issue. This is a diagnostic step.
2. **Workaround: round coordinates to integers** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - Callers can round DrawRect/DrawPath coordinates to integer pixel boundaries before drawing. This avoids the precision edge case. Not a fix, but reliably avoids the bug.
3. **Report upstream to Skia** — investigation, confidence 0.80 (80%), cost/s, validated=untested
   - File a bug in the Skia issue tracker (bugs.chromium.org/p/skia) with the minimal repro. The root cause is almost certainly in Skia's GPU clip path implementation, not in SkiaSharp's wrapper.

**Recommended proposal:** Investigate disabling MSAA or changing MSAA mode in SKGLView

**Why:** Quickest way to isolate whether the multisampled stencil buffer is the cause, which would guide whether to report to Skia upstream or fix in the SkiaSharp iOS SKGLView initialization.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Complete minimal repro, confirmed on all iOS devices/simulators across many versions, clear GPU-specific trigger. Root cause is likely in Skia's GPU stencil clip path — needs investigation into whether MSAA is the contributing factor and whether this is an upstream Skia bug. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, views, iOS, and OpenGL labels | labels=type/bug, area/SkiaSharp.Views, os/iOS, backend/OpenGL |
| add-comment | medium | 0.88 (88%) | Acknowledge bug, share workarounds, ask about Metal vs OpenGL | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the very clear minimal repro and screenshots!

This looks like a floating-point precision edge case in Skia's GPU stencil clip path. When `ClipRoundRect` tessellates the round-rect boundary and a subsequent `DrawRect` has a coordinate extremely close to an integer (like `199.999f`), the GPU stencil test may incorrectly exclude the draw.

**Confirmed triggers:** SKGLView + ClipRoundRect + near-integer float coordinate. **Does not occur:** CPU rendering, integer coordinates, or zero clip radius.

**Workaround while this is investigated:**
- Switch to `SKCanvasView` (CPU rendering), or
- Round your `DrawRect` coordinates to integers: `(float)Math.Round(199.999f)` → `200f`.

**One question:** Does the same code also fail with `SKMetalView` (the Metal-based view, available since SkiaSharp 2.x)? That would help narrow down whether this is an OpenGL ES driver issue or a core Skia GPU clip bug.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1969,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T22:24:00Z"
  },
  "summary": "SKGLView on iOS renders incomplete drawings when ClipRoundRect is combined with near-integer float coordinates (e.g., 199.999f) — bug present from v1.68.1 through v2.80.3, does not reproduce on Android, WinForms, or with CPU rendering.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.88
    },
    "platforms": [
      "os/iOS"
    ],
    "backends": [
      "backend/OpenGL"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "Yellow rectangle missing from rendered output when DrawRect uses y=199.999f after ClipRoundRect",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net6.0-ios",
        "xamarin.ios"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an iOS app with SKGLView",
        "In OnPaintSurface, call canvas.ClipRoundRect with a 10-unit radius",
        "Call canvas.DrawRect with a rect whose top coordinate is 199.999f",
        "Run on any iOS device or Simulator (iOS 9–15)"
      ],
      "environmentDetails": "iOS 9–15, iPhone, iPad, Simulator. SkiaSharp 1.68.1–2.80.3. Visual Studio.",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1969",
          "description": "Original issue with screenshots and minimal repro"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.1",
        "2.80.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "No known good version — bug present from very old versions to latest tested. The iOS SKGLView code uses OpenGLES2 with 4x MSAA and stencil clipping; this path hasn't fundamentally changed."
    }
  },
  "analysis": {
    "summary": "GPU clip precision bug on iOS: SKGLView uses OpenGLES2 with 4x MSAA (DrawableMultisample.Sample4x) and stencil-based clip (DrawableStencilFormat.Format8). ClipRoundRect triggers GPU tessellation of the rounded-rect clip path. When a subsequent DrawRect has a top coordinate extremely close to (but below) an integer boundary (199.999f ≈ 200.0), floating-point precision differences in Skia's GPU stencil clip path can cause the draw to be incorrectly excluded from the clip region. This does not occur on CPU because the CPU rasterizer uses a different precision path.",
    "rationale": "Strong bug signal: minimal, self-contained repro provided, confirmed on all iOS devices and simulators, iOS 9–15, multiple SkiaSharp versions. Issue is GPU-only (disappears on CPU), specific to round-rect clip (disappears with radius=0), and specific to near-integer float coordinates (disappears with integer 200). The stencil-based clip implementation in Skia's GPU backend (SkGpuDevice / GrClipStack) is the most plausible root cause — precision edge cases in tessellation of the clip polygon boundary.",
    "keySignals": [
      {
        "text": "switch to CPU rendering (SKCanvasView), the bug disappears",
        "source": "issue body",
        "interpretation": "Root cause is in GPU code path, not in the SkiaSharp C# wrapper or the drawing API."
      },
      {
        "text": "change the radius of the round-rect clipping to 0 (not rounded anymore), the bug disappears",
        "source": "issue body",
        "interpretation": "Rounded-rect tessellation is required to trigger the bug — axis-aligned rect clip works fine."
      },
      {
        "text": "change the coordinate 199.999f to 200, the bug disappears",
        "source": "issue body",
        "interpretation": "Floating-point precision at the boundary of a clip segment is the trigger — classic off-by-epsilon in stencil clip."
      },
      {
        "text": "could not reproduce it with Android (SkiaSharp.Views.Android.SKGLSurfaceView) nor with WinForms (SkiaSharp.Views.Desktop.SKGLControl)",
        "source": "issue body",
        "interpretation": "iOS-specific — likely due to iOS Metal/OpenGLES driver behavior or iOS-specific 4x MSAA multisampled stencil path."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKGLView.cs",
        "lines": "86-98",
        "finding": "Initialize() sets DrawableMultisample = GLKViewDrawableMultisample.Sample4x and DrawableStencilFormat = GLKViewDrawableStencilFormat.Format8. The 4x MSAA multisampled stencil buffer is used for GPU clip path evaluation, which is where floating-point edge cases manifest.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKGLView.cs",
        "lines": "130-145",
        "finding": "DrawInRect reads GL_STENCIL_BITS and GL_SAMPLES from the framebuffer and passes them to GRBackendRenderTarget. Sample count is capped by context.GetMaxSurfaceSampleCount(colorType). This is the correct setup for MSAA, but multisampled stencil clip on iOS OpenGLES may behave differently from other platforms.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Switch from SKGLView to SKCanvasView (CPU rendering) — eliminates the bug but sacrifices GPU acceleration.",
      "Round all DrawRect coordinates to integers before drawing (e.g., use Math.Round or (float)Math.Floor) — avoids the precision edge case.",
      "Use a non-rounded clip rectangle (radius = 0) if the visual design permits."
    ],
    "nextQuestions": [
      "Does the bug reproduce with SKMetalView on iOS (Metal backend) or only with the deprecated OpenGLES SKGLView?",
      "Is the bug present in Skia upstream (chromium/skia) with the same coordinate pattern on iOS Metal/OpenGLES?",
      "Does disabling MSAA (DrawableMultisample = None) eliminate the bug?"
    ],
    "resolution": {
      "hypothesis": "Skia's GPU stencil clip path tessellates the rounded-rect boundary; near-integer float coordinates (199.999f) fall on or very near a stencil sample boundary, causing a precision error that excludes subsequent draws. The 4x MSAA multisampled stencil on iOS may amplify this edge case.",
      "proposals": [
        {
          "title": "Investigate disabling MSAA or changing MSAA mode in SKGLView",
          "description": "Try setting DrawableMultisample = GLKViewDrawableMultisample.None in Initialize() to see if this eliminates the stencil precision issue. This is a diagnostic step.",
          "category": "investigation",
          "confidence": 0.65,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Workaround: round coordinates to integers",
          "description": "Callers can round DrawRect/DrawPath coordinates to integer pixel boundaries before drawing. This avoids the precision edge case. Not a fix, but reliably avoids the bug.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Report upstream to Skia",
          "description": "File a bug in the Skia issue tracker (bugs.chromium.org/p/skia) with the minimal repro. The root cause is almost certainly in Skia's GPU clip path implementation, not in SkiaSharp's wrapper.",
          "category": "investigation",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Investigate disabling MSAA or changing MSAA mode in SKGLView",
      "recommendedReason": "Quickest way to isolate whether the multisampled stencil buffer is the cause, which would guide whether to report to Skia upstream or fix in the SkiaSharp iOS SKGLView initialization."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Complete minimal repro, confirmed on all iOS devices/simulators across many versions, clear GPU-specific trigger. Root cause is likely in Skia's GPU stencil clip path — needs investigation into whether MSAA is the contributing factor and whether this is an upstream Skia bug.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views, iOS, and OpenGL labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/iOS",
          "backend/OpenGL"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge bug, share workarounds, ask about Metal vs OpenGL",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the very clear minimal repro and screenshots!\n\nThis looks like a floating-point precision edge case in Skia's GPU stencil clip path. When `ClipRoundRect` tessellates the round-rect boundary and a subsequent `DrawRect` has a coordinate extremely close to an integer (like `199.999f`), the GPU stencil test may incorrectly exclude the draw.\n\n**Confirmed triggers:** SKGLView + ClipRoundRect + near-integer float coordinate. **Does not occur:** CPU rendering, integer coordinates, or zero clip radius.\n\n**Workaround while this is investigated:**\n- Switch to `SKCanvasView` (CPU rendering), or\n- Round your `DrawRect` coordinates to integers: `(float)Math.Round(199.999f)` → `200f`.\n\n**One question:** Does the same code also fail with `SKMetalView` (the Metal-based view, available since SkiaSharp 2.x)? That would help narrow down whether this is an OpenGL ES driver issue or a core Skia GPU clip bug."
      }
    ]
  }
}
```

</details>
