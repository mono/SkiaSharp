# Issue Triage Report — #1664

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T14:30:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** SKGLView on Xamarin.Forms macOS crashes with SIGABRT inside NSOpenGLContext.flushBuffer via the Intel GPU driver intermittently during rendering.

**Analysis:** Intermittent SIGABRT crash inside the Intel GPU driver triggered by NSOpenGLContext.FlushBuffer() on macOS during SKGLView rendering. The crash originates within Apple's GPU support layer (libGPUSupportMercury.dylib, AppleIntelICLGraphicsGLDriver), indicating that invalid or malformed GL commands are being submitted to the driver. The SKGLView.DrawRect implementation calls canvas.Flush(), context.Flush(), and OpenGLContext.FlushBuffer() in sequence without any guard for an invalid or null GL context state.

**Recommendations:** **needs-investigation** — Crash is real with native stack trace but intermittent with no minimal repro. Needs investigation of the GL context state and potential race with NSOpenGLView lifecycle. The deprecated NSOpenGLView API on macOS is a contributing factor.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/macOS |
| Backends | backend/OpenGL |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Create a Xamarin.Forms macOS app using SKGLView
2. Render various shapes and images on a canvas
3. Run the app — crash occurs intermittently with no reproducible pattern

**Environment:** SkiaSharp 2.80.0–2.80.2, macOS 10.15.7, MacBookPro16,2 (Intel Iris Plus), x86_64

**Related issues:** #1412

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | Exception Type: SIGABRT — Thread 0 Crashed: gpusGenerateCrashLog (libGPUSupportMercury.dylib) via NSOpenGLContext flushBuffer |
| Repro quality | partial |
| Target frameworks | Xamarin.Forms macOS |

**Stack trace:**

```text
CGLFlushDrawable -> NSOpenGLContext flushBuffer -> xamarin_dyn_objc_msgSend -> mono_jit_runtime_invoke
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.0, 2.80.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The macOS SKGLView code calling OpenGLContext.FlushBuffer() is still present in the current source tree without additional guard conditions. |

## Analysis

### Technical Summary

Intermittent SIGABRT crash inside the Intel GPU driver triggered by NSOpenGLContext.FlushBuffer() on macOS during SKGLView rendering. The crash originates within Apple's GPU support layer (libGPUSupportMercury.dylib, AppleIntelICLGraphicsGLDriver), indicating that invalid or malformed GL commands are being submitted to the driver. The SKGLView.DrawRect implementation calls canvas.Flush(), context.Flush(), and OpenGLContext.FlushBuffer() in sequence without any guard for an invalid or null GL context state.

### Rationale

Clear crash signature with native stack trace pointing to SKGLView rendering path. The SIGABRT is triggered from within the GPU driver after NSOpenGLContext.FlushBuffer(), which is called unconditionally at the end of DrawRect. The intermittent nature suggests a race condition or transient invalid GL state. The crash path goes through SKGLView.DrawRect -> OpenGLContext.FlushBuffer -> CGLFlushDrawable -> Intel GPU driver abort. Issue is in the Xamarin.Forms macOS platform view code.

### Key Signals

- "from time to time (haven't found a pattern so far) the application crashes" — **issue body** (Intermittent crash suggests a race condition or timing-dependent invalid GL state, not a deterministic bug.)
- "gpusGenerateCrashLog -> glSwap_Exec -> CGLFlushDrawable -> NSOpenGLContext flushBuffer" — **issue body (crash log Thread 0)** (The GPU driver itself aborts after receiving an invalid buffer swap request, pointing to a malformed GL submission from SkiaSharp.)
- "reports with this issue occurred with previous versions as well - tested with 2.80.0 and above" — **issue body** (Not a regression introduced in 2.80.2 — persistent issue across multiple versions.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs` | 170-176 | direct | DrawRect ends with canvas.Flush(), context.Flush(), and OpenGLContext.FlushBuffer() called unconditionally. There is no null check on OpenGLContext before calling FlushBuffer(), and no error handling if the GL context is in an invalid state. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs` | 88-95 | related | PrepareOpenGL creates the GRContext. If context creation fails or is nil, DrawRect will still proceed to call FlushBuffer() potentially with an invalid GL state already submitted. |

### Next Questions

- Does the crash reproduce on AMD or Apple Silicon Macs, or only on Intel Iris Plus?
- Is there a specific drawing operation (image, path, shape) that triggers it more often?
- Does the crash occur if the app is moved to a different display (retina vs non-retina)?
- Does adding a null/validity check on OpenGLContext before FlushBuffer prevent the crash?
- Does the crash occur with SkiaSharp 3.x (Metal backend) or only with the legacy OpenGL path?

### Resolution Proposals

**Hypothesis:** The GPU driver aborts because an invalid or malformed GL command buffer is submitted via CGLFlushDrawable. This may be triggered by a race condition where DrawRect is invoked while the GL context is being resized (Reshape) or when the NSOpenGLView backing layer state is inconsistent. The unconditional call to OpenGLContext.FlushBuffer() without context validity checks is the proximate cause.

1. **Add null check on OpenGLContext before FlushBuffer** — investigation, confidence 0.55 (55%), cost/xs, validated=untested
   - Guard the FlushBuffer call with a null check on OpenGLContext to avoid flushing an invalid context.
2. **Migrate to Metal backend on macOS** — alternative, confidence 0.80 (80%), cost/xl, validated=untested
   - NSOpenGLView and NSOpenGLContext are deprecated since macOS 10.14. Migrating SKGLView on macOS to use Metal via SKMetalView would avoid the deprecated OpenGL path entirely and prevent these GPU driver crashes.
3. **Use SKCanvasView instead of SKGLView on macOS** — workaround, confidence 0.85 (85%), cost/s, validated=untested
   - Switch from SKGLView to SKCanvasView on macOS to avoid the OpenGL path entirely. SKCanvasView uses the CPU raster backend which does not go through CGLFlushDrawable.

**Recommended proposal:** Use SKCanvasView instead of SKGLView on macOS

**Why:** Avoids the deprecated NSOpenGLContext entirely while the root cause in the GPU driver interaction is investigated. The Metal-based migration is the long-term fix but requires significant effort.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Crash is real with native stack trace but intermittent with no minimal repro. Needs investigation of the GL context state and potential race with NSOpenGLView lifecycle. The deprecated NSOpenGLView API on macOS is a contributing factor. |
| Suggested repro platform | macos |

### Missing Info

- Minimal reproducible project (not just a crash log)
- Whether the crash is specific to Intel GPU or also affects AMD/Apple Silicon
- Which specific drawing operation (if any) triggers it more reliably

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, views, macOS, OpenGL labels | labels=type/bug, area/SkiaSharp.Views, os/macOS, backend/OpenGL |
| add-comment | medium | 0.82 (82%) | Acknowledge crash and request minimal repro with workaround suggestion | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed crash log! The stack trace points to a crash inside Apple's Intel GPU driver during `NSOpenGLContext.FlushBuffer()`, which is called at the end of every `SKGLView.DrawRect`. The intermittent nature suggests a race condition or transient invalid GL state.

A few questions to help narrow it down:
1. Does the crash happen on non-Intel Macs (AMD GPU or Apple Silicon)?
2. Is there a specific drawing operation (images, paths, etc.) that seems to trigger it more often?
3. Does the crash happen when the window is moved between retina and non-retina displays?

**Workaround:** If you need a stable short-term solution, try switching from `SKGLView` to `SKCanvasView`. The CPU raster backend avoids the deprecated `NSOpenGLContext` path entirely:

```csharp
// Instead of SKGLView, use SKCanvasView:
var canvasView = new SKCanvasView();
canvasView.PaintSurface += OnPaintSurface;
```

Note: `NSOpenGLView` and `NSOpenGLContext` are deprecated since macOS 10.14. We are tracking migration to the Metal backend as a long-term solution.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1664,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T14:30:00Z"
  },
  "summary": "SKGLView on Xamarin.Forms macOS crashes with SIGABRT inside NSOpenGLContext.flushBuffer via the Intel GPU driver intermittently during rendering.",
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
      "os/macOS"
    ],
    "backends": [
      "backend/OpenGL"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "Exception Type: SIGABRT — Thread 0 Crashed: gpusGenerateCrashLog (libGPUSupportMercury.dylib) via NSOpenGLContext flushBuffer",
      "stackTrace": "CGLFlushDrawable -> NSOpenGLContext flushBuffer -> xamarin_dyn_objc_msgSend -> mono_jit_runtime_invoke",
      "reproQuality": "partial",
      "targetFrameworks": [
        "Xamarin.Forms macOS"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Xamarin.Forms macOS app using SKGLView",
        "Render various shapes and images on a canvas",
        "Run the app — crash occurs intermittently with no reproducible pattern"
      ],
      "environmentDetails": "SkiaSharp 2.80.0–2.80.2, macOS 10.15.7, MacBookPro16,2 (Intel Iris Plus), x86_64",
      "relatedIssues": [
        1412
      ],
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.0",
        "2.80.2"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The macOS SKGLView code calling OpenGLContext.FlushBuffer() is still present in the current source tree without additional guard conditions."
    }
  },
  "analysis": {
    "summary": "Intermittent SIGABRT crash inside the Intel GPU driver triggered by NSOpenGLContext.FlushBuffer() on macOS during SKGLView rendering. The crash originates within Apple's GPU support layer (libGPUSupportMercury.dylib, AppleIntelICLGraphicsGLDriver), indicating that invalid or malformed GL commands are being submitted to the driver. The SKGLView.DrawRect implementation calls canvas.Flush(), context.Flush(), and OpenGLContext.FlushBuffer() in sequence without any guard for an invalid or null GL context state.",
    "rationale": "Clear crash signature with native stack trace pointing to SKGLView rendering path. The SIGABRT is triggered from within the GPU driver after NSOpenGLContext.FlushBuffer(), which is called unconditionally at the end of DrawRect. The intermittent nature suggests a race condition or transient invalid GL state. The crash path goes through SKGLView.DrawRect -> OpenGLContext.FlushBuffer -> CGLFlushDrawable -> Intel GPU driver abort. Issue is in the Xamarin.Forms macOS platform view code.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs",
        "lines": "170-176",
        "finding": "DrawRect ends with canvas.Flush(), context.Flush(), and OpenGLContext.FlushBuffer() called unconditionally. There is no null check on OpenGLContext before calling FlushBuffer(), and no error handling if the GL context is in an invalid state.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs",
        "lines": "88-95",
        "finding": "PrepareOpenGL creates the GRContext. If context creation fails or is nil, DrawRect will still proceed to call FlushBuffer() potentially with an invalid GL state already submitted.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "from time to time (haven't found a pattern so far) the application crashes",
        "source": "issue body",
        "interpretation": "Intermittent crash suggests a race condition or timing-dependent invalid GL state, not a deterministic bug."
      },
      {
        "text": "gpusGenerateCrashLog -> glSwap_Exec -> CGLFlushDrawable -> NSOpenGLContext flushBuffer",
        "source": "issue body (crash log Thread 0)",
        "interpretation": "The GPU driver itself aborts after receiving an invalid buffer swap request, pointing to a malformed GL submission from SkiaSharp."
      },
      {
        "text": "reports with this issue occurred with previous versions as well - tested with 2.80.0 and above",
        "source": "issue body",
        "interpretation": "Not a regression introduced in 2.80.2 — persistent issue across multiple versions."
      }
    ],
    "nextQuestions": [
      "Does the crash reproduce on AMD or Apple Silicon Macs, or only on Intel Iris Plus?",
      "Is there a specific drawing operation (image, path, shape) that triggers it more often?",
      "Does the crash occur if the app is moved to a different display (retina vs non-retina)?",
      "Does adding a null/validity check on OpenGLContext before FlushBuffer prevent the crash?",
      "Does the crash occur with SkiaSharp 3.x (Metal backend) or only with the legacy OpenGL path?"
    ],
    "resolution": {
      "hypothesis": "The GPU driver aborts because an invalid or malformed GL command buffer is submitted via CGLFlushDrawable. This may be triggered by a race condition where DrawRect is invoked while the GL context is being resized (Reshape) or when the NSOpenGLView backing layer state is inconsistent. The unconditional call to OpenGLContext.FlushBuffer() without context validity checks is the proximate cause.",
      "proposals": [
        {
          "title": "Add null check on OpenGLContext before FlushBuffer",
          "description": "Guard the FlushBuffer call with a null check on OpenGLContext to avoid flushing an invalid context.",
          "category": "investigation",
          "confidence": 0.55,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Migrate to Metal backend on macOS",
          "description": "NSOpenGLView and NSOpenGLContext are deprecated since macOS 10.14. Migrating SKGLView on macOS to use Metal via SKMetalView would avoid the deprecated OpenGL path entirely and prevent these GPU driver crashes.",
          "category": "alternative",
          "confidence": 0.8,
          "effort": "cost/xl",
          "validated": "untested"
        },
        {
          "title": "Use SKCanvasView instead of SKGLView on macOS",
          "description": "Switch from SKGLView to SKCanvasView on macOS to avoid the OpenGL path entirely. SKCanvasView uses the CPU raster backend which does not go through CGLFlushDrawable.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use SKCanvasView instead of SKGLView on macOS",
      "recommendedReason": "Avoids the deprecated NSOpenGLContext entirely while the root cause in the GPU driver interaction is investigated. The Metal-based migration is the long-term fix but requires significant effort."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Crash is real with native stack trace but intermittent with no minimal repro. Needs investigation of the GL context state and potential race with NSOpenGLView lifecycle. The deprecated NSOpenGLView API on macOS is a contributing factor.",
      "suggestedReproPlatform": "macos"
    },
    "missingInfo": [
      "Minimal reproducible project (not just a crash log)",
      "Whether the crash is specific to Intel GPU or also affects AMD/Apple Silicon",
      "Which specific drawing operation (if any) triggers it more reliably"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views, macOS, OpenGL labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/macOS",
          "backend/OpenGL"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge crash and request minimal repro with workaround suggestion",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thank you for the detailed crash log! The stack trace points to a crash inside Apple's Intel GPU driver during `NSOpenGLContext.FlushBuffer()`, which is called at the end of every `SKGLView.DrawRect`. The intermittent nature suggests a race condition or transient invalid GL state.\n\nA few questions to help narrow it down:\n1. Does the crash happen on non-Intel Macs (AMD GPU or Apple Silicon)?\n2. Is there a specific drawing operation (images, paths, etc.) that seems to trigger it more often?\n3. Does the crash happen when the window is moved between retina and non-retina displays?\n\n**Workaround:** If you need a stable short-term solution, try switching from `SKGLView` to `SKCanvasView`. The CPU raster backend avoids the deprecated `NSOpenGLContext` path entirely:\n\n```csharp\n// Instead of SKGLView, use SKCanvasView:\nvar canvasView = new SKCanvasView();\ncanvasView.PaintSurface += OnPaintSurface;\n```\n\nNote: `NSOpenGLView` and `NSOpenGLContext` are deprecated since macOS 10.14. We are tracking migration to the Metal backend as a long-term solution."
      }
    ]
  }
}
```

</details>
