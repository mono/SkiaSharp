# Issue Triage Report — #1723

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T19:57:19Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp.Views (0.97 (97%)) |
| Suggested action | ready-to-fix (0.93 (93%)) |

**Issue Summary:** NullReferenceException crash in SKGLTextureViewRenderer.OnDrawFrame on Android because SKSurface.Create can return null but its return value is used without a null check.

**Analysis:** In SKGLTextureViewRenderer.OnDrawFrame, SKSurface.Create is documented to return null when the GPU does not support the requested configuration. The code immediately dereferences the return value (surface.Canvas) without a null check, causing a NullReferenceException. This reproduces exclusively on low-end Huawei/Honor Android 8.1 devices, which likely have restricted OpenGL ES capabilities. The fix is a one-line null guard: if surface is null after Create, return early and skip the draw frame.

**Recommendations:** **ready-to-fix** — Root cause is a missing null check on SKSurface.Create return value at a known file and line. Fix is trivial. Multiple production reporters confirm the crash and community investigation already identified the exact fix needed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Android |
| Backends | backend/OpenGL |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

**Environment:** Android 8.1.0 on Huawei Y7 Prime 2019, HUAWEI Y7s, Honor 8C/8X Max. Version 2.80.2.

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/229 — Earlier issue confirming SKSurface.Create returns null on Android when GPU rejects the GL configuration

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | exception |
| Error message | System.NullReferenceException: Object reference not set to an instance of an object at SkiaSharp.Views.Android.SKGLTextureViewRenderer.OnDrawFrame |
| Repro quality | partial |
| Target frameworks | MonoAndroid11.0 |

**Stack trace:**

```text
System.NullReferenceException: Object reference not set to an instance of an object
  at SkiaSharp.Views.Android.SKGLTextureViewRenderer.OnDrawFrame (Javax.Microedition.Khronos.Opengles.IGL10 gl) [0x0008a]
  at SkiaSharp.Views.Android.GLTextureView+GLThread.GuardedRun () [0x003bd]
  at SkiaSharp.Views.Android.GLTextureView+GLThread.Run () [0x00028]
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The null-check omission at line 85 of SKGLTextureViewRenderer.cs is still present in the current source. |

## Analysis

### Technical Summary

In SKGLTextureViewRenderer.OnDrawFrame, SKSurface.Create is documented to return null when the GPU does not support the requested configuration. The code immediately dereferences the return value (surface.Canvas) without a null check, causing a NullReferenceException. This reproduces exclusively on low-end Huawei/Honor Android 8.1 devices, which likely have restricted OpenGL ES capabilities. The fix is a one-line null guard: if surface is null after Create, return early and skip the draw frame.

### Rationale

Classified as type/bug in area/SkiaSharp.Views because the NullReferenceException is caused by a missing null check on SKSurface.Create's return value within the SkiaSharp.Views.Android renderer. The API is documented to return null on unsupported GPU configurations, but the renderer does not handle this. The crash is reproducible on specific Huawei/Honor Android 8.1 devices. Action is ready-to-fix because the root cause is pinpointed to a single line and the fix is straightforward.

### Key Signals

- "Heavily reported by our production monitoring, not able to reproduce locally." — **issue body** (Device-specific hardware limitation; specific Huawei/Honor low-end Android 8.1 devices trigger it consistently.)
- "SKSurface.Create can return null on purpose. Returns the new surface if it could be created and the configuration is supported, otherwise null." — **comment by Huaba93 (#945391399)** (Community correctly identified the exact line causing the crash. The API contract documents null return but the caller does not handle it.)
- "This issue keeps to be the top reason for crashes for all of our android apps." — **comment by thisisthekap (#1025676349)** (High-impact production crash affecting multiple production apps across multiple reporters.)
- "Setting HasRenderLoop=false on disappearing/sleep did not change anything." — **comment by Huaba93 (#944018894)** (The suggested lifecycle workaround from the maintainer did not resolve the issue, confirming the root cause is the null surface path, not a lifecycle race.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLTextureViewRenderer.cs` | 83-86 | direct | Lines 84-85: surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType); canvas = surface.Canvas; — no null check between these two statements. SKSurface.Create returns null when the GPU rejects the configuration, causing a NullReferenceException on the canvas assignment. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLTextureViewRenderer.cs` | 127-135 | related | FreeContext() properly null-checks surface, renderTarget, and context before disposing. The same null-safe pattern is not applied to the surface creation path in OnDrawFrame. |

### Next Questions

- Does the same null-check omission exist in the equivalent SKGLSurfaceViewRenderer or other platform GL renderers?
- Should the renderer fall back to a raster surface when GL surface creation fails, rather than silently skipping the frame?

### Resolution Proposals

**Hypothesis:** SKSurface.Create returns null when the device GPU does not support the requested GL framebuffer configuration. Line 85 dereferences this null result unconditionally. Adding a null guard that returns early when surface creation fails will prevent the crash.

1. **Add null check after SKSurface.Create in OnDrawFrame** — fix, cost/xs, validated=yes
   - In SKGLTextureViewRenderer.OnDrawFrame, guard the surface.Canvas access with a null check on the surface creation result. If surface is null, return early from OnDrawFrame.

```csharp
// create the surface
if (surface == null)
{
    surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType);
    if (surface == null)
        return;
    canvas = surface.Canvas;
}
```
2. **Use SKCanvasView (CPU raster) instead of SKGLView on affected devices** — workaround, cost/s, validated=untested
   - Replace SKGLView with SKCanvasView to avoid OpenGL surface creation entirely. SKCanvasView uses the CPU raster backend which does not call SKSurface.Create with a GL context and will not crash on these devices.

**Recommended proposal:** Add null check after SKSurface.Create in OnDrawFrame

**Why:** The fix is a one-line guard that resolves the crash at the source for all affected devices without requiring app changes. It aligns with the API contract where null is a documented return value.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.93 (93%) |
| Reason | Root cause is a missing null check on SKSurface.Create return value at a known file and line. Fix is trivial. Multiple production reporters confirm the crash and community investigation already identified the exact fix needed. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply classification labels | labels=type/bug, area/SkiaSharp.Views, os/Android, backend/OpenGL, tenet/reliability |
| add-comment | medium | 0.93 (93%) | Explain root cause, confirm fix, provide workaround | — |
| link-related | low | 0.85 (85%) | Link to earlier issue about SKSurface.Create returning null on Android | linkedIssue=#229 |

**Comment draft for `add-comment`:**

```markdown
## Root Cause

The crash is caused by a missing null check in `SKGLTextureViewRenderer.OnDrawFrame`. `SKSurface.Create` is documented to return `null` when the device GPU does not support the requested configuration — and on these Huawei/Honor Android 8.1 devices, the GL framebuffer configuration is rejected. The renderer then immediately dereferences the null surface:

```csharp
// source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLTextureViewRenderer.cs, lines 84-85
surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType);
canvas = surface.Canvas; // NullReferenceException when surface is null
```

## Fix

A null guard should be added:

```csharp
if (surface == null)
{
    surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType);
    if (surface == null)
        return;
    canvas = surface.Canvas;
}
```

## Workaround (while waiting for a fix)

Replace `SKGLView` with `SKCanvasView` (CPU raster backend). It does not use OpenGL surface creation and will not crash on these devices. The drawing code using `SKCanvas` remains identical.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1723,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T19:57:19Z"
  },
  "summary": "NullReferenceException crash in SKGLTextureViewRenderer.OnDrawFrame on Android because SKSurface.Create can return null but its return value is used without a null check.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
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
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "System.NullReferenceException: Object reference not set to an instance of an object at SkiaSharp.Views.Android.SKGLTextureViewRenderer.OnDrawFrame",
      "stackTrace": "System.NullReferenceException: Object reference not set to an instance of an object\n  at SkiaSharp.Views.Android.SKGLTextureViewRenderer.OnDrawFrame (Javax.Microedition.Khronos.Opengles.IGL10 gl) [0x0008a]\n  at SkiaSharp.Views.Android.GLTextureView+GLThread.GuardedRun () [0x003bd]\n  at SkiaSharp.Views.Android.GLTextureView+GLThread.Run () [0x00028]",
      "reproQuality": "partial",
      "targetFrameworks": [
        "MonoAndroid11.0"
      ]
    },
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/229",
          "description": "Earlier issue confirming SKSurface.Create returns null on Android when GPU rejects the GL configuration"
        }
      ],
      "environmentDetails": "Android 8.1.0 on Huawei Y7 Prime 2019, HUAWEI Y7s, Honor 8C/8X Max. Version 2.80.2."
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The null-check omission at line 85 of SKGLTextureViewRenderer.cs is still present in the current source."
    }
  },
  "analysis": {
    "summary": "In SKGLTextureViewRenderer.OnDrawFrame, SKSurface.Create is documented to return null when the GPU does not support the requested configuration. The code immediately dereferences the return value (surface.Canvas) without a null check, causing a NullReferenceException. This reproduces exclusively on low-end Huawei/Honor Android 8.1 devices, which likely have restricted OpenGL ES capabilities. The fix is a one-line null guard: if surface is null after Create, return early and skip the draw frame.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLTextureViewRenderer.cs",
        "lines": "83-86",
        "finding": "Lines 84-85: surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType); canvas = surface.Canvas; — no null check between these two statements. SKSurface.Create returns null when the GPU rejects the configuration, causing a NullReferenceException on the canvas assignment.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLTextureViewRenderer.cs",
        "lines": "127-135",
        "finding": "FreeContext() properly null-checks surface, renderTarget, and context before disposing. The same null-safe pattern is not applied to the surface creation path in OnDrawFrame.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Heavily reported by our production monitoring, not able to reproduce locally.",
        "source": "issue body",
        "interpretation": "Device-specific hardware limitation; specific Huawei/Honor low-end Android 8.1 devices trigger it consistently."
      },
      {
        "text": "SKSurface.Create can return null on purpose. Returns the new surface if it could be created and the configuration is supported, otherwise null.",
        "source": "comment by Huaba93 (#945391399)",
        "interpretation": "Community correctly identified the exact line causing the crash. The API contract documents null return but the caller does not handle it."
      },
      {
        "text": "This issue keeps to be the top reason for crashes for all of our android apps.",
        "source": "comment by thisisthekap (#1025676349)",
        "interpretation": "High-impact production crash affecting multiple production apps across multiple reporters."
      },
      {
        "text": "Setting HasRenderLoop=false on disappearing/sleep did not change anything.",
        "source": "comment by Huaba93 (#944018894)",
        "interpretation": "The suggested lifecycle workaround from the maintainer did not resolve the issue, confirming the root cause is the null surface path, not a lifecycle race."
      }
    ],
    "rationale": "Classified as type/bug in area/SkiaSharp.Views because the NullReferenceException is caused by a missing null check on SKSurface.Create's return value within the SkiaSharp.Views.Android renderer. The API is documented to return null on unsupported GPU configurations, but the renderer does not handle this. The crash is reproducible on specific Huawei/Honor Android 8.1 devices. Action is ready-to-fix because the root cause is pinpointed to a single line and the fix is straightforward.",
    "resolution": {
      "hypothesis": "SKSurface.Create returns null when the device GPU does not support the requested GL framebuffer configuration. Line 85 dereferences this null result unconditionally. Adding a null guard that returns early when surface creation fails will prevent the crash.",
      "proposals": [
        {
          "title": "Add null check after SKSurface.Create in OnDrawFrame",
          "category": "fix",
          "effort": "cost/xs",
          "validated": "yes",
          "description": "In SKGLTextureViewRenderer.OnDrawFrame, guard the surface.Canvas access with a null check on the surface creation result. If surface is null, return early from OnDrawFrame.",
          "codeSnippet": "// create the surface\nif (surface == null)\n{\n    surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType);\n    if (surface == null)\n        return;\n    canvas = surface.Canvas;\n}"
        },
        {
          "title": "Use SKCanvasView (CPU raster) instead of SKGLView on affected devices",
          "category": "workaround",
          "effort": "cost/s",
          "validated": "untested",
          "description": "Replace SKGLView with SKCanvasView to avoid OpenGL surface creation entirely. SKCanvasView uses the CPU raster backend which does not call SKSurface.Create with a GL context and will not crash on these devices."
        }
      ],
      "recommendedProposal": "Add null check after SKSurface.Create in OnDrawFrame",
      "recommendedReason": "The fix is a one-line guard that resolves the crash at the source for all affected devices without requiring app changes. It aligns with the API contract where null is a documented return value."
    },
    "nextQuestions": [
      "Does the same null-check omission exist in the equivalent SKGLSurfaceViewRenderer or other platform GL renderers?",
      "Should the renderer fall back to a raster surface when GL surface creation fails, rather than silently skipping the frame?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.93,
      "reason": "Root cause is a missing null check on SKSurface.Create return value at a known file and line. Fix is trivial. Multiple production reporters confirm the crash and community investigation already identified the exact fix needed.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply classification labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Android",
          "backend/OpenGL",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain root cause, confirm fix, provide workaround",
        "risk": "medium",
        "confidence": 0.93,
        "comment": "## Root Cause\n\nThe crash is caused by a missing null check in `SKGLTextureViewRenderer.OnDrawFrame`. `SKSurface.Create` is documented to return `null` when the device GPU does not support the requested configuration — and on these Huawei/Honor Android 8.1 devices, the GL framebuffer configuration is rejected. The renderer then immediately dereferences the null surface:\n\n```csharp\n// source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLTextureViewRenderer.cs, lines 84-85\nsurface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType);\ncanvas = surface.Canvas; // NullReferenceException when surface is null\n```\n\n## Fix\n\nA null guard should be added:\n\n```csharp\nif (surface == null)\n{\n    surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType);\n    if (surface == null)\n        return;\n    canvas = surface.Canvas;\n}\n```\n\n## Workaround (while waiting for a fix)\n\nReplace `SKGLView` with `SKCanvasView` (CPU raster backend). It does not use OpenGL surface creation and will not crash on these devices. The drawing code using `SKCanvas` remains identical."
      },
      {
        "type": "link-related",
        "description": "Link to earlier issue about SKSurface.Create returning null on Android",
        "risk": "low",
        "confidence": 0.85,
        "linkedIssue": 229
      }
    ]
  }
}
```

</details>
