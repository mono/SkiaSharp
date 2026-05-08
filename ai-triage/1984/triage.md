# Issue Triage Report — #1984

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T23:06:03Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** Calling SKImage.ToTextureImage() with a GRContext from SKMetalView causes a native crash on Xamarin.Mac via GrMtlCommandBuffer::encodeWaitForEvent inside the Metal GPU path.

**Analysis:** SKImage.ToTextureImage() crashes on macOS when using the Metal backend (SKMetalView). The managed stack trace shows the crash occurs during SKCanvas.Flush in the SKMetalView draw loop. The native crash is deep in GrMtlCommandBuffer::encodeWaitForEvent, suggesting the Metal command encoder is in an invalid state when the texture image is used for rendering. A related issue (#1164) reports the same API causing a crash on the OpenGL backend (SKGLView), also on macOS and also still unfixed.

**Recommendations:** **needs-investigation** — Real crash with full native and managed stack trace. Related to the still-open #1164. Needs investigation into GRContext state during Metal draw cycle.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/macOS |
| Backends | backend/Metal |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create a Xamarin.Mac application with SKMetalView
2. In the Draw callback, call ToTextureImage(context) on an SKImage using the GRContext from SKMetalView
3. Run the application — it crashes with a native SIGSEGV

**Environment:** SkiaSharp 2.88.0-preview.187, Xamarin.Mac, macOS 12.3, Intel KBL GPU (AppleIntelKBLGraphicsMTLDriver), Metal backend

**Related issues:** #1164

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1164 — Same ToTextureImage crash on macOS using OpenGL/SKGLView, same reporter — filed 2020, still open

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | Got a segv while executing native code — GrMtlCommandBuffer::encodeWaitForEvent |
| Repro quality | partial |
| Target frameworks | Xamarin.Mac |

**Stack trace:**

```text
GrMtlCommandBuffer::encodeWaitForEvent -> gr_backendrendertarget_get_gl_framebufferinfo -> SKCanvas.Flush -> SKMetalView.Draw
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.0-preview.187 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | ToTextureImage code path in SKImage.cs has not changed significantly; the crash is in the native Metal rendering pipeline which remains active. |

## Analysis

### Technical Summary

SKImage.ToTextureImage() crashes on macOS when using the Metal backend (SKMetalView). The managed stack trace shows the crash occurs during SKCanvas.Flush in the SKMetalView draw loop. The native crash is deep in GrMtlCommandBuffer::encodeWaitForEvent, suggesting the Metal command encoder is in an invalid state when the texture image is used for rendering. A related issue (#1164) reports the same API causing a crash on the OpenGL backend (SKGLView), also on macOS and also still unfixed.

### Rationale

Clear crash with full stack trace and specific reproduction context. The bug is real and reproducible. The Metal backend code path through GrMtlCommandBuffer::encodeWaitForEvent is the likely failure site. Related issue #1164 (OpenGL, same API) is still open, suggesting the root cause in the GPU texture promotion path has never been fixed.

### Key Signals

- "GrMtlCommandBuffer::encodeWaitForEvent in the native stacktrace" — **issue body** (Crash inside Metal's command buffer encoder suggests the GPU context is being used while Metal is mid-frame in an incompatible state.)
- "at SkiaSharp.SkiaApi:sk_canvas_flush / at SkiaSharp.SKCanvas:Flush / at SkiaSharp.Views.Mac.SKMetalView:MetalKit.IMTKViewDelegate.Draw" — **issue body — managed stacktrace** (The crash happens on canvas flush during the MetalKit draw cycle, not directly inside ToTextureImage itself — pointing to a deferred GPU execution issue triggered when the texture is actually rendered.)
- "Same reporter (nor0x) as issue #1164, same API (ToTextureImage), same platform (macOS)" — **cross-issue comparison** (This is likely a persistent issue with ToTextureImage on macOS, now surfacing with Metal just as it did with OpenGL in #1164.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKImage.cs` | 572-578 | direct | ToTextureImage(GRContext, bool, bool) delegates directly to SkiaApi.sk_image_make_texture_image after null-checking context. No additional thread-safety or state validation is performed before calling into native code. |
| `binding/SkiaSharp/SKImage.cs` | 566-570 | related | The two simpler overloads of ToTextureImage(context) and ToTextureImage(context, mipmapped) both chain to the three-parameter overload with budgeted=true — so all callers hit the same native code path. |

### Next Questions

- Does ToTextureImage work if called outside the MetalKit draw cycle (e.g., off-frame)?
- Does the same crash occur with SKGLView (OpenGL) on the same machine and SkiaSharp version?
- Is the GRContext returned from SKMetalView valid (non-null) at the point of the call?
- Does passing budgeted=false to ToTextureImage change the behavior?

### Resolution Proposals

**Hypothesis:** The GPU context from SKMetalView may not be in a drawable state when ToTextureImage is called, causing the Metal command encoder to fail when the resulting texture is flushed. The same GPU state mismatch likely affects #1164 on OpenGL.

1. **Investigate GRContext state before calling ToTextureImage** — investigation, confidence 0.70 (70%), cost/s, validated=untested
   - Add investigation to confirm whether GRContext.IsAbandoned or GRContext.ResetContext must be called before ToTextureImage in the Metal draw loop. Provide guidance in a comment.
2. **Link and consolidate with #1164** — investigation, confidence 0.90 (90%), cost/xs, validated=untested
   - Both issues report ToTextureImage crash on macOS. Linking them ensures any fix addresses both backends (Metal and OpenGL).

**Recommended proposal:** Link and consolidate with #1164

**Why:** Both issues share the same API, platform, and reporter. Consolidating them prevents duplicate work and ensures a comprehensive fix covers both backends.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Real crash with full native and managed stack trace. Related to the still-open #1164. Needs investigation into GRContext state during Metal draw cycle. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, SkiaSharp, macOS, Metal, reliability labels | labels=type/bug, area/SkiaSharp, os/macOS, backend/Metal, tenet/reliability |
| link-related | low | 0.90 (90%) | Cross-reference with #1164 (same ToTextureImage crash on macOS with OpenGL, same reporter, still open) | linkedIssue=#1164 |
| add-comment | medium | 0.88 (88%) | Acknowledge crash, note relationship with #1164, ask for additional info | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed crash report! This looks related to #1164 which reports the same `ToTextureImage` crash on macOS (on the OpenGL backend). The Metal stack trace here points to `GrMtlCommandBuffer::encodeWaitForEvent` failing during canvas flush in the MetalKit draw loop.

A few questions to help narrow down the root cause:
1. Is the `GRContext` you're passing to `ToTextureImage` the same one retrieved from `SKMetalView`?
2. Does the crash occur if you call `ToTextureImage` outside of the MetalKit draw loop (e.g., in a background setup step)?
3. Does passing `budgeted: false` to `ToTextureImage(context, mipmapped: false, budgeted: false)` change the behavior?

In the meantime, as a workaround you could try keeping the image as a raster image and drawing it directly without promoting it to a texture — this avoids the GPU texture promotion path that seems to be causing the crash.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1984,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T23:06:03Z"
  },
  "summary": "Calling SKImage.ToTextureImage() with a GRContext from SKMetalView causes a native crash on Xamarin.Mac via GrMtlCommandBuffer::encodeWaitForEvent inside the Metal GPU path.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/macOS"
    ],
    "backends": [
      "backend/Metal"
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
      "errorMessage": "Got a segv while executing native code — GrMtlCommandBuffer::encodeWaitForEvent",
      "stackTrace": "GrMtlCommandBuffer::encodeWaitForEvent -> gr_backendrendertarget_get_gl_framebufferinfo -> SKCanvas.Flush -> SKMetalView.Draw",
      "reproQuality": "partial",
      "targetFrameworks": [
        "Xamarin.Mac"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Xamarin.Mac application with SKMetalView",
        "In the Draw callback, call ToTextureImage(context) on an SKImage using the GRContext from SKMetalView",
        "Run the application — it crashes with a native SIGSEGV"
      ],
      "environmentDetails": "SkiaSharp 2.88.0-preview.187, Xamarin.Mac, macOS 12.3, Intel KBL GPU (AppleIntelKBLGraphicsMTLDriver), Metal backend",
      "relatedIssues": [
        1164
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1164",
          "description": "Same ToTextureImage crash on macOS using OpenGL/SKGLView, same reporter — filed 2020, still open"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.0-preview.187"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "ToTextureImage code path in SKImage.cs has not changed significantly; the crash is in the native Metal rendering pipeline which remains active."
    }
  },
  "analysis": {
    "summary": "SKImage.ToTextureImage() crashes on macOS when using the Metal backend (SKMetalView). The managed stack trace shows the crash occurs during SKCanvas.Flush in the SKMetalView draw loop. The native crash is deep in GrMtlCommandBuffer::encodeWaitForEvent, suggesting the Metal command encoder is in an invalid state when the texture image is used for rendering. A related issue (#1164) reports the same API causing a crash on the OpenGL backend (SKGLView), also on macOS and also still unfixed.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "572-578",
        "finding": "ToTextureImage(GRContext, bool, bool) delegates directly to SkiaApi.sk_image_make_texture_image after null-checking context. No additional thread-safety or state validation is performed before calling into native code.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "566-570",
        "finding": "The two simpler overloads of ToTextureImage(context) and ToTextureImage(context, mipmapped) both chain to the three-parameter overload with budgeted=true — so all callers hit the same native code path.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "GrMtlCommandBuffer::encodeWaitForEvent in the native stacktrace",
        "source": "issue body",
        "interpretation": "Crash inside Metal's command buffer encoder suggests the GPU context is being used while Metal is mid-frame in an incompatible state."
      },
      {
        "text": "at SkiaSharp.SkiaApi:sk_canvas_flush / at SkiaSharp.SKCanvas:Flush / at SkiaSharp.Views.Mac.SKMetalView:MetalKit.IMTKViewDelegate.Draw",
        "source": "issue body — managed stacktrace",
        "interpretation": "The crash happens on canvas flush during the MetalKit draw cycle, not directly inside ToTextureImage itself — pointing to a deferred GPU execution issue triggered when the texture is actually rendered."
      },
      {
        "text": "Same reporter (nor0x) as issue #1164, same API (ToTextureImage), same platform (macOS)",
        "source": "cross-issue comparison",
        "interpretation": "This is likely a persistent issue with ToTextureImage on macOS, now surfacing with Metal just as it did with OpenGL in #1164."
      }
    ],
    "rationale": "Clear crash with full stack trace and specific reproduction context. The bug is real and reproducible. The Metal backend code path through GrMtlCommandBuffer::encodeWaitForEvent is the likely failure site. Related issue #1164 (OpenGL, same API) is still open, suggesting the root cause in the GPU texture promotion path has never been fixed.",
    "nextQuestions": [
      "Does ToTextureImage work if called outside the MetalKit draw cycle (e.g., off-frame)?",
      "Does the same crash occur with SKGLView (OpenGL) on the same machine and SkiaSharp version?",
      "Is the GRContext returned from SKMetalView valid (non-null) at the point of the call?",
      "Does passing budgeted=false to ToTextureImage change the behavior?"
    ],
    "resolution": {
      "hypothesis": "The GPU context from SKMetalView may not be in a drawable state when ToTextureImage is called, causing the Metal command encoder to fail when the resulting texture is flushed. The same GPU state mismatch likely affects #1164 on OpenGL.",
      "proposals": [
        {
          "title": "Investigate GRContext state before calling ToTextureImage",
          "description": "Add investigation to confirm whether GRContext.IsAbandoned or GRContext.ResetContext must be called before ToTextureImage in the Metal draw loop. Provide guidance in a comment.",
          "category": "investigation",
          "confidence": 0.7,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Link and consolidate with #1164",
          "description": "Both issues report ToTextureImage crash on macOS. Linking them ensures any fix addresses both backends (Metal and OpenGL).",
          "category": "investigation",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Link and consolidate with #1164",
      "recommendedReason": "Both issues share the same API, platform, and reporter. Consolidating them prevents duplicate work and ensures a comprehensive fix covers both backends."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Real crash with full native and managed stack trace. Related to the still-open #1164. Needs investigation into GRContext state during Metal draw cycle.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, SkiaSharp, macOS, Metal, reliability labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/macOS",
          "backend/Metal",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference with #1164 (same ToTextureImage crash on macOS with OpenGL, same reporter, still open)",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 1164
      },
      {
        "type": "add-comment",
        "description": "Acknowledge crash, note relationship with #1164, ask for additional info",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed crash report! This looks related to #1164 which reports the same `ToTextureImage` crash on macOS (on the OpenGL backend). The Metal stack trace here points to `GrMtlCommandBuffer::encodeWaitForEvent` failing during canvas flush in the MetalKit draw loop.\n\nA few questions to help narrow down the root cause:\n1. Is the `GRContext` you're passing to `ToTextureImage` the same one retrieved from `SKMetalView`?\n2. Does the crash occur if you call `ToTextureImage` outside of the MetalKit draw loop (e.g., in a background setup step)?\n3. Does passing `budgeted: false` to `ToTextureImage(context, mipmapped: false, budgeted: false)` change the behavior?\n\nIn the meantime, as a workaround you could try keeping the image as a raster image and drawing it directly without promoting it to a texture — this avoids the GPU texture promotion path that seems to be causing the crash."
      }
    ]
  }
}
```

</details>
