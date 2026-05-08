# Issue Triage Report — #1918

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T21:25:00Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp.Views (0.92 (92%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** SKGLControl in WinForms MDI application turns black and crashes with AccessViolationException in canvas.Flush() when MDI child windows are reparented (dragged out to floating windows).

**Analysis:** When an MDI child window is reparented (dragged out to float), Windows destroys and recreates the native HWND. OpenGL contexts in Windows are tied to a specific HWND (via the device context/HDC). SKGLControl does not override OnHandleDestroyed/OnHandleCreated to reset the GRContext, surface, and render target when the handle changes, leaving them referencing the invalidated GL context which causes black rendering or AccessViolationException on the next canvas.Flush().

**Recommendations:** **needs-investigation** — Real crash (AccessViolationException) with a plausible root cause identified but no minimal repro provided. Needs deeper investigation into OpenTK/WinForms MDI HWND lifecycle to confirm and implement a fix.

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

1. Create a WinForms MDI application with two MDI child windows
2. Add an SKGLControl to each MDI child window
3. Both controls work initially when docked as MDI children
4. Drag one MDI child window out to float as its own window
5. First SKGLControl turns black — rendering stops despite OnPaint firing
6. Drag the second MDI child window out as well
7. Second SKGLControl triggers System.AccessViolationException in canvas.Flush()

**Environment:** Windows 10 build 19043.1348, SkiaSharp v2.80.3, SkiaSharp.Views.Desktop.Common, SkiaSharp.Views.WindowsForms

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | System.AccessViolationException inside canvas.Flush() |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SKGLControl.cs has no reparent/handle-recreation handling in recent code; the same code path is still present. |

## Analysis

### Technical Summary

When an MDI child window is reparented (dragged out to float), Windows destroys and recreates the native HWND. OpenGL contexts in Windows are tied to a specific HWND (via the device context/HDC). SKGLControl does not override OnHandleDestroyed/OnHandleCreated to reset the GRContext, surface, and render target when the handle changes, leaving them referencing the invalidated GL context which causes black rendering or AccessViolationException on the next canvas.Flush().

### Rationale

This is a real bug: AccessViolationException in canvas.Flush() is not user error. The root cause is that OpenGL contexts are bound to the native window handle, and MDI float/reparent operations in WinForms cause HWND recreation. SKGLControl does not handle this lifecycle event. The title says [QUESTION] but the described behavior is a crash/rendering failure — classifying as type/bug.

### Key Signals

- "System.AccessViolationException inside the canvas.Flush() method" — **issue body** (Native GL call through an invalidated context — classic HWND-tied GL context issue.)
- "the first one turns black when dragged out, even if the other MDI child window isn't created" — **issue body** (The GL context is silently invalidated and draw calls are no-ops; no crash because the specific flush path happens to tolerate it.)
- "I've also tried getting the GRContext and calling AbandonContext(true) when the parent changes. This fails on the next OnPaint() with an 'Object reference not set to an instance of an object.' exception" — **issue body** (Reporter correctly identified the pattern but the subsequent re-initialization after AbandonContext is missing — grContext is nulled but not recreated on next paint.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs` | 72-134 | direct | OnPaint calls MakeCurrent() then uses the cached grContext, renderTarget, surface, and canvas. There is no OnHandleDestroyed or OnHandleCreated override to detect when the native window handle (HWND) is recreated and reset the GL-backed objects accordingly. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs` | 142-154 | direct | Dispose() cleans up grContext, renderTarget, surface, canvas — but only on full disposal. There is no partial-reset path triggered by HWND recreation during reparenting, leaving stale object references after the OpenGL context is invalidated. |

### Workarounds

- Listen to the Control.ParentChanged event. In the handler, call grContext?.AbandonContext(true) then set grContext = null, surface = null, renderTarget = null, canvas = null. On the next OnPaint, the null-checks will trigger re-initialization from scratch for the new HWND.
- Alternatively, override OnHandleDestroyed and perform the same teardown, letting OnPaint re-create everything on the next paint cycle.

### Resolution Proposals

**Hypothesis:** SKGLControl does not handle HWND recreation during MDI float/reparent, leaving the cached GRContext and Skia surface tied to a destroyed GL context, causing black rendering or AccessViolationException.

1. **Workaround: Reset GRContext on ParentChanged** — workaround, confidence 0.70 (70%), cost/s, validated=untested
   - Subscribe to ParentChanged event and reset all GL-backed objects so that the next OnPaint call re-initializes them against the new HWND.

```csharp
// In your MDI child form or a subclass of SKGLControl:
this.ParentChanged += (s, e) => {
    GRContext?.AbandonContext(true);
    GRContext?.Dispose();
    // Force re-creation on next paint by calling Invalidate()
    Invalidate();
};
```
2. **Fix: Override OnHandleDestroyed in SKGLControl** — fix, confidence 0.80 (80%), cost/s, validated=untested
   - Override OnHandleDestroyed (and optionally OnHandleCreated) in SKGLControl to abandon and release the GRContext, surface, and render target when the native handle is destroyed during reparenting. The null checks in OnPaint will then trigger re-initialization.

**Recommended proposal:** Fix: Override OnHandleDestroyed in SKGLControl

**Why:** A proper fix in the library is preferable so all MDI users benefit without workarounds.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Real crash (AccessViolationException) with a plausible root cause identified but no minimal repro provided. Needs deeper investigation into OpenTK/WinForms MDI HWND lifecycle to confirm and implement a fix. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/SkiaSharp.Views, os/Windows-Classic, backend/OpenGL, tenet/reliability | labels=type/bug, area/SkiaSharp.Views, os/Windows-Classic, backend/OpenGL, tenet/reliability |
| add-comment | medium | 0.75 (75%) | Acknowledge the issue, explain the HWND/GL context root cause, and provide a workaround using ParentChanged | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report. This looks like a known limitation in how OpenGL contexts interact with WinForms MDI reparenting.

**Root cause hypothesis:** OpenGL contexts on Windows are tied to the native window handle (HWND). When an MDI child is dragged out to float, WinForms destroys and recreates the underlying HWND. `SKGLControl` does not currently handle this lifecycle event, so the cached `GRContext`, surface, and render target remain pointing at the now-invalidated GL context — causing silent black rendering for the first control and `AccessViolationException` in `canvas.Flush()` for the second.

**Workaround:** You can reset the GL state manually when the parent changes. In your form or a `SKGLControl` subclass, listen for `ParentChanged` and tear down the context:

```csharp
skglControl.ParentChanged += (s, e) =>
{
    // After reparenting, the old GL context is invalid
    // Let OnPaint re-create everything for the new HWND
    skglControl.GRContext?.AbandonContext(true);
    skglControl.GRContext?.Dispose();
    skglControl.Invalidate();
};
```

> ⚠️ Note: `GRContext` is currently a read-only property. You may need to subclass `SKGLControl` and expose a way to reset internal fields, or override `OnHandleDestroyed` in a subclass to call the teardown there.

We are tracking a proper fix that would add `OnHandleDestroyed`/`OnHandleCreated` overrides to `SKGLControl` so reparenting is handled automatically.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1918,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T21:25:00Z"
  },
  "summary": "SKGLControl in WinForms MDI application turns black and crashes with AccessViolationException in canvas.Flush() when MDI child windows are reparented (dragged out to floating windows).",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.92
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
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "System.AccessViolationException inside canvas.Flush()",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a WinForms MDI application with two MDI child windows",
        "Add an SKGLControl to each MDI child window",
        "Both controls work initially when docked as MDI children",
        "Drag one MDI child window out to float as its own window",
        "First SKGLControl turns black — rendering stops despite OnPaint firing",
        "Drag the second MDI child window out as well",
        "Second SKGLControl triggers System.AccessViolationException in canvas.Flush()"
      ],
      "environmentDetails": "Windows 10 build 19043.1348, SkiaSharp v2.80.3, SkiaSharp.Views.Desktop.Common, SkiaSharp.Views.WindowsForms"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "SKGLControl.cs has no reparent/handle-recreation handling in recent code; the same code path is still present."
    }
  },
  "analysis": {
    "summary": "When an MDI child window is reparented (dragged out to float), Windows destroys and recreates the native HWND. OpenGL contexts in Windows are tied to a specific HWND (via the device context/HDC). SKGLControl does not override OnHandleDestroyed/OnHandleCreated to reset the GRContext, surface, and render target when the handle changes, leaving them referencing the invalidated GL context which causes black rendering or AccessViolationException on the next canvas.Flush().",
    "rationale": "This is a real bug: AccessViolationException in canvas.Flush() is not user error. The root cause is that OpenGL contexts are bound to the native window handle, and MDI float/reparent operations in WinForms cause HWND recreation. SKGLControl does not handle this lifecycle event. The title says [QUESTION] but the described behavior is a crash/rendering failure — classifying as type/bug.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs",
        "lines": "72-134",
        "finding": "OnPaint calls MakeCurrent() then uses the cached grContext, renderTarget, surface, and canvas. There is no OnHandleDestroyed or OnHandleCreated override to detect when the native window handle (HWND) is recreated and reset the GL-backed objects accordingly.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs",
        "lines": "142-154",
        "finding": "Dispose() cleans up grContext, renderTarget, surface, canvas — but only on full disposal. There is no partial-reset path triggered by HWND recreation during reparenting, leaving stale object references after the OpenGL context is invalidated.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "System.AccessViolationException inside the canvas.Flush() method",
        "source": "issue body",
        "interpretation": "Native GL call through an invalidated context — classic HWND-tied GL context issue."
      },
      {
        "text": "the first one turns black when dragged out, even if the other MDI child window isn't created",
        "source": "issue body",
        "interpretation": "The GL context is silently invalidated and draw calls are no-ops; no crash because the specific flush path happens to tolerate it."
      },
      {
        "text": "I've also tried getting the GRContext and calling AbandonContext(true) when the parent changes. This fails on the next OnPaint() with an 'Object reference not set to an instance of an object.' exception",
        "source": "issue body",
        "interpretation": "Reporter correctly identified the pattern but the subsequent re-initialization after AbandonContext is missing — grContext is nulled but not recreated on next paint."
      }
    ],
    "workarounds": [
      "Listen to the Control.ParentChanged event. In the handler, call grContext?.AbandonContext(true) then set grContext = null, surface = null, renderTarget = null, canvas = null. On the next OnPaint, the null-checks will trigger re-initialization from scratch for the new HWND.",
      "Alternatively, override OnHandleDestroyed and perform the same teardown, letting OnPaint re-create everything on the next paint cycle."
    ],
    "resolution": {
      "hypothesis": "SKGLControl does not handle HWND recreation during MDI float/reparent, leaving the cached GRContext and Skia surface tied to a destroyed GL context, causing black rendering or AccessViolationException.",
      "proposals": [
        {
          "title": "Workaround: Reset GRContext on ParentChanged",
          "description": "Subscribe to ParentChanged event and reset all GL-backed objects so that the next OnPaint call re-initializes them against the new HWND.",
          "category": "workaround",
          "codeSnippet": "// In your MDI child form or a subclass of SKGLControl:\nthis.ParentChanged += (s, e) => {\n    GRContext?.AbandonContext(true);\n    GRContext?.Dispose();\n    // Force re-creation on next paint by calling Invalidate()\n    Invalidate();\n};",
          "confidence": 0.7,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Fix: Override OnHandleDestroyed in SKGLControl",
          "description": "Override OnHandleDestroyed (and optionally OnHandleCreated) in SKGLControl to abandon and release the GRContext, surface, and render target when the native handle is destroyed during reparenting. The null checks in OnPaint will then trigger re-initialization.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Fix: Override OnHandleDestroyed in SKGLControl",
      "recommendedReason": "A proper fix in the library is preferable so all MDI users benefit without workarounds."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Real crash (AccessViolationException) with a plausible root cause identified but no minimal repro provided. Needs deeper investigation into OpenTK/WinForms MDI HWND lifecycle to confirm and implement a fix.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views, os/Windows-Classic, backend/OpenGL, tenet/reliability",
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
        "type": "add-comment",
        "description": "Acknowledge the issue, explain the HWND/GL context root cause, and provide a workaround using ParentChanged",
        "risk": "medium",
        "confidence": 0.75,
        "comment": "Thanks for the detailed report. This looks like a known limitation in how OpenGL contexts interact with WinForms MDI reparenting.\n\n**Root cause hypothesis:** OpenGL contexts on Windows are tied to the native window handle (HWND). When an MDI child is dragged out to float, WinForms destroys and recreates the underlying HWND. `SKGLControl` does not currently handle this lifecycle event, so the cached `GRContext`, surface, and render target remain pointing at the now-invalidated GL context — causing silent black rendering for the first control and `AccessViolationException` in `canvas.Flush()` for the second.\n\n**Workaround:** You can reset the GL state manually when the parent changes. In your form or a `SKGLControl` subclass, listen for `ParentChanged` and tear down the context:\n\n```csharp\nskglControl.ParentChanged += (s, e) =>\n{\n    // After reparenting, the old GL context is invalid\n    // Let OnPaint re-create everything for the new HWND\n    skglControl.GRContext?.AbandonContext(true);\n    skglControl.GRContext?.Dispose();\n    skglControl.Invalidate();\n};\n```\n\n> ⚠️ Note: `GRContext` is currently a read-only property. You may need to subclass `SKGLControl` and expose a way to reset internal fields, or override `OnHandleDestroyed` in a subclass to call the teardown there.\n\nWe are tracking a proper fix that would add `OnHandleDestroyed`/`OnHandleCreated` overrides to `SKGLControl` so reparenting is handled automatically."
      }
    ]
  }
}
```

</details>
