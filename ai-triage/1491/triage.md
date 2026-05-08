# Issue Triage Report — #1491

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T21:54:14Z |
| Type | type/bug (0.75 (75%)) |
| Area | area/SkiaSharp.Views (0.85 (85%)) |
| Suggested action | needs-info (0.72 (72%)) |

**Issue Summary:** Reporter gets AccessViolationException in sk_canvas_draw_image when calling InvalidateVisual() from a background worker thread on a WPF SKElement that uses a manually-created GPU GRContext/SKSurface via an external WpfGles library.

**Analysis:** The crash is caused by a threading violation: the reporter creates an OpenGL GRContext on one thread using an external WpfGles library, then triggers re-renders via InvalidateVisual() from a background worker. WPF calls OnRender on the UI thread, but the OpenGL context is bound to a different thread, causing memory corruption and an AccessViolationException in native Skia drawing code.

**Recommendations:** **needs-info** — No minimal repro provided; crash is likely a threading/GL context violation in user code rather than a SkiaSharp bug. The issue is from 2020 on a preview version, no reporter follow-up after maintainer suggestions, and SKGLElement (the proper solution) has since been added. Needs confirmation whether the issue persists with current SkiaSharp or is resolved by SKGLElement.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Classic |
| Backends | backend/OpenGL |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create a WPF app with an SKElement
2. Manually create a GRContext via an external WpfGles library (WpfGles.GlesImage)
3. Create an SKSurface backed by the GRContext
4. Call InvalidateVisual() on the SKElement from a background worker thread
5. Observe AccessViolationException in sk_canvas_draw_image during OnRender

**Environment:** SkiaSharp 2.80.2-preview.36, Visual Studio 2019, WPF, Windows, background worker calls InvalidateVisual()

**Related issues:** #745

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1491#issuecomment-689702207 — Maintainer points to SKElement always returning to UI thread, offers to accept GLES+SkiaSharp example
- https://github.com/mono/SkiaSharp/issues/1491#issuecomment-708089388 — Maintainer references PR #1521 and issue #745 as related
- https://github.com/mono/SkiaSharp/issues/745 — GPU-accelerated WPF without WindowsFormsHost — related request, closed as completed in 2024

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | crash |
| Error message | System.AccessViolationException at SkiaSharp.SkiaApi.sk_canvas_draw_image |
| Repro quality | partial |
| Target frameworks | — |

**Stack trace:**

```text
at SkiaSharp.SkiaApi.sk_canvas_draw_image(IntPtr param0, IntPtr param1, Single x, Single y, IntPtr param4)
  at SkiaSharp.SKCanvas.DrawImage(SKImage image, SKPoint p, SKPaint paint)
  at sdg.VideoConceptor.Scene_PaintSurface(...)
  at SkiaSharp.Views.WPF.SKElement.OnRender(DrawingContext drawingContext)
  at System.Windows.UIElement.Arrange(Rect finalRect)
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2-preview.36 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | This was reported against a preview version from 2020. SKGLElement (proper GPU WPF element) has since been added to the repo, which addresses the underlying need. |

## Analysis

### Technical Summary

The crash is caused by a threading violation: the reporter creates an OpenGL GRContext on one thread using an external WpfGles library, then triggers re-renders via InvalidateVisual() from a background worker. WPF calls OnRender on the UI thread, but the OpenGL context is bound to a different thread, causing memory corruption and an AccessViolationException in native Skia drawing code.

### Rationale

The AccessViolationException with a stack trace in sk_canvas_draw_image is consistent with either a threading violation (GL context not current on the rendering thread) or premature disposal of GPU-backed objects. The reporter explicitly notes the crash only happens when calling InvalidateVisual() from a background worker. The standard SKElement is CPU-only; the reporter is manually layering an external GL context on top, bypassing SkiaSharp.Views' own lifecycle management. The maintainer's diagnosis (comment 688474915) confirms the threading hypothesis. SKGLElement (added later) is the correct API for GPU-accelerated WPF rendering.

### Key Signals

- "The error was triggered when the InvalidateVisual() was called on the SKElement from a background worker" — **issue body** (Caller is triggering render invalidation from wrong thread; OpenGL contexts are thread-affine — the GL context created on one thread is not current on the WPF render thread.)
- "if I don't use the background worker, everything goes fine" — **issue body** (Confirms this is a threading issue, not a general rendering bug.)
- "var vb = new WpfGles.GlesImage(); // Set The GrContext" — **issue body code snippet** (Reporter is using an external GLES library (github.com/l3m/wpf-gles) to create an OpenGL context outside of SkiaSharp's own view lifecycle — non-standard usage.)
- "SKElement is not the most exciting things, but it will always come back to the UI thread" — **maintainer comment 689702207** (Maintainer confirms SKElement is CPU-only and always renders on the UI thread — the GPU surface created externally is incompatible with this model.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs` | 45-89 | direct | SKElement.OnRender uses a CPU-based WriteableBitmap and SKSurface.Create(info, backBuffer, stride) — no GRContext involvement. It always executes on the WPF UI thread via the standard WPF rendering pipeline. There is no GL context management here. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKGLElement.cs` | 44-181 | related | SKGLElement is the proper GPU-accelerated WPF control. It internally manages GRContext creation (line 131-132), surface lifecycle, and integrates with OpenTK/GLWpfControl for correct GL context threading. This is the correct replacement for manual GRContext usage in WPF. |

### Workarounds

- Call InvalidateVisual() on the UI thread using Dispatcher.Invoke() or Dispatcher.BeginInvoke() instead of calling it directly from a background worker.
- Use SKGLElement instead of SKElement for GPU-accelerated WPF rendering — it handles GL context lifecycle correctly.
- Use the off-screen GPU rendering pattern from issue #745: render to an off-screen SKSurface on a dedicated thread, then blit the result to a CPU-backed SKElement on the UI thread.

### Next Questions

- Was this resolved after the reporter tried the alternative approaches suggested by the maintainer?
- Does the crash still occur with the current SkiaSharp version now that SKGLElement exists?
- Is the external WpfGles library (github.com/l3m/wpf-gles) still needed, or does SKGLElement fully replace the use case?

### Resolution Proposals

**Hypothesis:** The crash is a threading/GL context violation, not a SkiaSharp bug. The correct fix is to either dispatch InvalidateVisual() to the UI thread or switch to SKGLElement which manages GL context threading properly.

1. **Dispatch InvalidateVisual to UI thread** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Wrap the InvalidateVisual() call in a Dispatcher.Invoke to ensure it is called from the UI thread, preventing the GL context from being used across threads.

```csharp
// Instead of calling directly from background thread:
// skElement.InvalidateVisual(); // WRONG

// Use Dispatcher.Invoke:
skElement.Dispatcher.Invoke(() => skElement.InvalidateVisual());
```
2. **Switch to SKGLElement for GPU rendering** — alternative, confidence 0.85 (85%), cost/m, validated=untested
   - Replace the custom WpfGles.GlesImage + SKElement approach with SKGLElement, which integrates OpenGL context management correctly within the WPF rendering pipeline via OpenTK.

**Recommended proposal:** Dispatch InvalidateVisual to UI thread

**Why:** Lowest effort fix — a one-line change that corrects the threading violation. Switching to SKGLElement is the longer-term correct approach but requires more refactoring.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.72 (72%) |
| Reason | No minimal repro provided; crash is likely a threading/GL context violation in user code rather than a SkiaSharp bug. The issue is from 2020 on a preview version, no reporter follow-up after maintainer suggestions, and SKGLElement (the proper solution) has since been added. Needs confirmation whether the issue persists with current SkiaSharp or is resolved by SKGLElement. |
| Suggested repro platform | windows |

### Missing Info

- Minimal standalone repro project (reporter said they would try alternatives but never provided one)
- Confirmation whether the crash still occurs with the current SkiaSharp release
- Whether SKGLElement resolves the use case

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, WPF views, Windows-Classic, OpenGL labels | labels=type/bug, area/SkiaSharp.Views, os/Windows-Classic, backend/OpenGL, tenet/reliability |
| link-related | low | 0.90 (90%) | Link to related issue #745 (GPU-accelerated WPF, closed as completed) | linkedIssue=#745 |
| add-comment | medium | 0.80 (80%) | Post analysis with workaround (dispatch to UI thread) and suggest SKGLElement | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed stack trace! The `AccessViolationException` is most likely caused by a **threading violation**: `InvalidateVisual()` is being called from a background worker, but the OpenGL context created via `WpfGles.GlesImage` is bound to a specific thread. When WPF's render pipeline calls `OnRender` on the UI thread, the GL context is no longer current — leading to a native memory access crash.

**Immediate workaround:** Dispatch `InvalidateVisual()` to the UI thread:
```csharp
skElement.Dispatcher.Invoke(() => skElement.InvalidateVisual());
```

**Longer-term solution:** Consider switching to `SKGLElement` (now available in `SkiaSharp.Views.WPF`), which handles the OpenGL context lifecycle correctly within the WPF rendering pipeline. See also the approach discussed in #745.

Could you confirm whether this issue still occurs with the current SkiaSharp release, or if `SKGLElement` addresses your use case?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1491,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T21:54:14Z"
  },
  "summary": "Reporter gets AccessViolationException in sk_canvas_draw_image when calling InvalidateVisual() from a background worker thread on a WPF SKElement that uses a manually-created GPU GRContext/SKSurface via an external WpfGles library.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.75
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.85
    },
    "platforms": [
      "os/Windows-Classic"
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
      "errorType": "crash",
      "errorMessage": "System.AccessViolationException at SkiaSharp.SkiaApi.sk_canvas_draw_image",
      "stackTrace": "at SkiaSharp.SkiaApi.sk_canvas_draw_image(IntPtr param0, IntPtr param1, Single x, Single y, IntPtr param4)\n  at SkiaSharp.SKCanvas.DrawImage(SKImage image, SKPoint p, SKPaint paint)\n  at sdg.VideoConceptor.Scene_PaintSurface(...)\n  at SkiaSharp.Views.WPF.SKElement.OnRender(DrawingContext drawingContext)\n  at System.Windows.UIElement.Arrange(Rect finalRect)",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a WPF app with an SKElement",
        "Manually create a GRContext via an external WpfGles library (WpfGles.GlesImage)",
        "Create an SKSurface backed by the GRContext",
        "Call InvalidateVisual() on the SKElement from a background worker thread",
        "Observe AccessViolationException in sk_canvas_draw_image during OnRender"
      ],
      "environmentDetails": "SkiaSharp 2.80.2-preview.36, Visual Studio 2019, WPF, Windows, background worker calls InvalidateVisual()",
      "relatedIssues": [
        745
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1491#issuecomment-689702207",
          "description": "Maintainer points to SKElement always returning to UI thread, offers to accept GLES+SkiaSharp example"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1491#issuecomment-708089388",
          "description": "Maintainer references PR #1521 and issue #745 as related"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/745",
          "description": "GPU-accelerated WPF without WindowsFormsHost — related request, closed as completed in 2024"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2-preview.36"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "This was reported against a preview version from 2020. SKGLElement (proper GPU WPF element) has since been added to the repo, which addresses the underlying need."
    }
  },
  "analysis": {
    "summary": "The crash is caused by a threading violation: the reporter creates an OpenGL GRContext on one thread using an external WpfGles library, then triggers re-renders via InvalidateVisual() from a background worker. WPF calls OnRender on the UI thread, but the OpenGL context is bound to a different thread, causing memory corruption and an AccessViolationException in native Skia drawing code.",
    "rationale": "The AccessViolationException with a stack trace in sk_canvas_draw_image is consistent with either a threading violation (GL context not current on the rendering thread) or premature disposal of GPU-backed objects. The reporter explicitly notes the crash only happens when calling InvalidateVisual() from a background worker. The standard SKElement is CPU-only; the reporter is manually layering an external GL context on top, bypassing SkiaSharp.Views' own lifecycle management. The maintainer's diagnosis (comment 688474915) confirms the threading hypothesis. SKGLElement (added later) is the correct API for GPU-accelerated WPF rendering.",
    "keySignals": [
      {
        "text": "The error was triggered when the InvalidateVisual() was called on the SKElement from a background worker",
        "source": "issue body",
        "interpretation": "Caller is triggering render invalidation from wrong thread; OpenGL contexts are thread-affine — the GL context created on one thread is not current on the WPF render thread."
      },
      {
        "text": "if I don't use the background worker, everything goes fine",
        "source": "issue body",
        "interpretation": "Confirms this is a threading issue, not a general rendering bug."
      },
      {
        "text": "var vb = new WpfGles.GlesImage(); // Set The GrContext",
        "source": "issue body code snippet",
        "interpretation": "Reporter is using an external GLES library (github.com/l3m/wpf-gles) to create an OpenGL context outside of SkiaSharp's own view lifecycle — non-standard usage."
      },
      {
        "text": "SKElement is not the most exciting things, but it will always come back to the UI thread",
        "source": "maintainer comment 689702207",
        "interpretation": "Maintainer confirms SKElement is CPU-only and always renders on the UI thread — the GPU surface created externally is incompatible with this model."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs",
        "lines": "45-89",
        "finding": "SKElement.OnRender uses a CPU-based WriteableBitmap and SKSurface.Create(info, backBuffer, stride) — no GRContext involvement. It always executes on the WPF UI thread via the standard WPF rendering pipeline. There is no GL context management here.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKGLElement.cs",
        "lines": "44-181",
        "finding": "SKGLElement is the proper GPU-accelerated WPF control. It internally manages GRContext creation (line 131-132), surface lifecycle, and integrates with OpenTK/GLWpfControl for correct GL context threading. This is the correct replacement for manual GRContext usage in WPF.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Call InvalidateVisual() on the UI thread using Dispatcher.Invoke() or Dispatcher.BeginInvoke() instead of calling it directly from a background worker.",
      "Use SKGLElement instead of SKElement for GPU-accelerated WPF rendering — it handles GL context lifecycle correctly.",
      "Use the off-screen GPU rendering pattern from issue #745: render to an off-screen SKSurface on a dedicated thread, then blit the result to a CPU-backed SKElement on the UI thread."
    ],
    "nextQuestions": [
      "Was this resolved after the reporter tried the alternative approaches suggested by the maintainer?",
      "Does the crash still occur with the current SkiaSharp version now that SKGLElement exists?",
      "Is the external WpfGles library (github.com/l3m/wpf-gles) still needed, or does SKGLElement fully replace the use case?"
    ],
    "resolution": {
      "hypothesis": "The crash is a threading/GL context violation, not a SkiaSharp bug. The correct fix is to either dispatch InvalidateVisual() to the UI thread or switch to SKGLElement which manages GL context threading properly.",
      "proposals": [
        {
          "title": "Dispatch InvalidateVisual to UI thread",
          "description": "Wrap the InvalidateVisual() call in a Dispatcher.Invoke to ensure it is called from the UI thread, preventing the GL context from being used across threads.",
          "category": "workaround",
          "codeSnippet": "// Instead of calling directly from background thread:\n// skElement.InvalidateVisual(); // WRONG\n\n// Use Dispatcher.Invoke:\nskElement.Dispatcher.Invoke(() => skElement.InvalidateVisual());",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Switch to SKGLElement for GPU rendering",
          "description": "Replace the custom WpfGles.GlesImage + SKElement approach with SKGLElement, which integrates OpenGL context management correctly within the WPF rendering pipeline via OpenTK.",
          "category": "alternative",
          "confidence": 0.85,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Dispatch InvalidateVisual to UI thread",
      "recommendedReason": "Lowest effort fix — a one-line change that corrects the threading violation. Switching to SKGLElement is the longer-term correct approach but requires more refactoring."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.72,
      "reason": "No minimal repro provided; crash is likely a threading/GL context violation in user code rather than a SkiaSharp bug. The issue is from 2020 on a preview version, no reporter follow-up after maintainer suggestions, and SKGLElement (the proper solution) has since been added. Needs confirmation whether the issue persists with current SkiaSharp or is resolved by SKGLElement.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "Minimal standalone repro project (reporter said they would try alternatives but never provided one)",
      "Confirmation whether the crash still occurs with the current SkiaSharp release",
      "Whether SKGLElement resolves the use case"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, WPF views, Windows-Classic, OpenGL labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-Classic",
          "backend/OpenGL",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-related",
        "description": "Link to related issue #745 (GPU-accelerated WPF, closed as completed)",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 745
      },
      {
        "type": "add-comment",
        "description": "Post analysis with workaround (dispatch to UI thread) and suggest SKGLElement",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the detailed stack trace! The `AccessViolationException` is most likely caused by a **threading violation**: `InvalidateVisual()` is being called from a background worker, but the OpenGL context created via `WpfGles.GlesImage` is bound to a specific thread. When WPF's render pipeline calls `OnRender` on the UI thread, the GL context is no longer current — leading to a native memory access crash.\n\n**Immediate workaround:** Dispatch `InvalidateVisual()` to the UI thread:\n```csharp\nskElement.Dispatcher.Invoke(() => skElement.InvalidateVisual());\n```\n\n**Longer-term solution:** Consider switching to `SKGLElement` (now available in `SkiaSharp.Views.WPF`), which handles the OpenGL context lifecycle correctly within the WPF rendering pipeline. See also the approach discussed in #745.\n\nCould you confirm whether this issue still occurs with the current SkiaSharp release, or if `SKGLElement` addresses your use case?"
      }
    ]
  }
}
```

</details>
