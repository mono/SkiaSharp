# Issue Triage Report — #2872

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T15:25:42Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | close-as-external (0.88 (88%)) |

**Issue Summary:** SKGLElement crashes with an access violation in GLWpfControl (DXRegisterObjectNV) when a second SKGLElement is created on a separate STA thread on Windows WPF, because the underlying GLWpfControl uses a static shared OpenGL context that cannot be current on more than one thread simultaneously.

**Analysis:** The crash originates in GLWpfControl's DX/GL interop layer (DXRegisterObjectNV) when a second instance is created on a new thread. GLWpfControl uses a static shared OpenGL context that is not thread-safe; creating a new GLWpfControl-derived control (SKGLElement) on a different thread tries to use that context while it may be current on the original thread, causing the access violation. This is a confirmed upstream GLWpfControl limitation.

**Recommendations:** **close-as-external** — The crash is in GLWpfControl (OpenTK external dependency) which uses a static shared OpenGL context not supporting multi-thread instantiation. A GLWpfControl contributor confirmed this. SkiaSharp cannot fix it without changes in the external library.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Classic |
| Backends | backend/OpenGL |
| Tenets | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a WPF application with an SKGLElement in the main window XAML
2. Spawn a new STA thread that creates a second MainWindow (with SKGLElement) and calls form.Show() + Dispatcher.Run()
3. Observe access violation crash in DXRegisterObjectNV within GLWpfControl

**Environment:** Windows 11 23H2 64-bit, Visual Studio, SkiaSharp 3.x Alpha

**Repository links:**
- https://github.com/opentk/GLWpfControl/issues/58 — Related GLWpfControl upstream issue about multi-thread context sharing

**Code snippets:**

```csharp
var t = new Thread(() => { var form = new MainWindow(); form.Show(); System.Windows.Threading.Dispatcher.Run(); }); t.SetApartmentState(ApartmentState.STA); t.Start();
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | crash |
| Error message | Access violation in OpenTK.Graphics.Wgl.Wgl.DXRegisterObjectNV at GLWpfControl DxGLFramebuffer constructor |
| Repro quality | complete |
| Target frameworks | net3.x-windows |

**Stack trace:**

```text
OpenTK.Graphics.Wgl.Wgl.DXRegisterObjectNV -> OpenTK.Wpf.DxGLFramebuffer.ctor -> GLWpfControlRenderer.SetSize -> GLWpfControl.OnRender -> SKGLElement.OnRender
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.x |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SKGLElement still directly inherits from GLWpfControl without any per-thread context isolation. The upstream GLWpfControl static context limitation has not been resolved. |

## Analysis

### Technical Summary

The crash originates in GLWpfControl's DX/GL interop layer (DXRegisterObjectNV) when a second instance is created on a new thread. GLWpfControl uses a static shared OpenGL context that is not thread-safe; creating a new GLWpfControl-derived control (SKGLElement) on a different thread tries to use that context while it may be current on the original thread, causing the access violation. This is a confirmed upstream GLWpfControl limitation.

### Rationale

The crash is real and reproducible — confirmed by both the call stack and a GLWpfControl maintainer (NogginBops). The root cause is in the external OpenTK/GLWpfControl dependency which uses a static context not designed for multi-thread instantiation. SKGLElement directly extends GLWpfControl without adding any thread isolation, so it inherits the limitation. The reporter acknowledged the external root cause and decided not to pursue the WPF migration. Classifying as close-as-external is appropriate since the fix requires changes in GLWpfControl, not SkiaSharp.

### Key Signals

- "Currently creating GLWPFControls on separate threads is likely not going to work as GLWPFControl is using a static shared OpenGL context which can only be current on one thread." — **comment by NogginBops (GLWpfControl contributor)** (Confirms the root cause is in the external GLWpfControl dependency — not in SkiaSharp itself.)
- "Not sure if related to https://github.com/opentk/GLWpfControl/issues/58" — **reporter comment** (Reporter already suspects the upstream GLWpfControl issue.)
- "I am going to hold off on my Winforms -> WPF rewrite for the time being" — **reporter comment** (Reporter accepted the limitation — no longer actively pursuing the issue.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKGLElement.cs` | 29-65 | direct | SKGLElement extends GLWpfControl directly and calls Start(settings) in Initialize() with no thread-safety or per-thread context management. There is no guard or documentation preventing use from multiple threads. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKGLElement.cs` | 108-115 | related | OnRender delegates to base.OnRender which triggers GLWpfControl.OnRender. The crash happens inside GLWpfControl.DxGLFramebuffer constructor (DXRegisterObjectNV) — in the external OpenTK package — not in SkiaSharp code itself. SkiaSharp cannot fix this without forking or replacing GLWpfControl. |

### Workarounds

- Use SKCanvasView (software/raster renderer) instead of SKGLElement — it does not depend on GLWpfControl and supports multiple WPF windows on separate STA threads.
- Keep all SKGLElement instances on the main UI thread rather than creating windows on separate threads.

### Next Questions

- Has GLWpfControl upsteam (opentk/GLWpfControl) fixed multi-thread context support in a newer version?
- Could SKGLElement be refactored to not use GLWpfControl (e.g., use ANGLE or WGL directly) to avoid this limitation?

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.88 (88%) |
| Reason | The crash is in GLWpfControl (OpenTK external dependency) which uses a static shared OpenGL context not supporting multi-thread instantiation. A GLWpfControl contributor confirmed this. SkiaSharp cannot fix it without changes in the external library. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, views, Windows labels | labels=type/bug, area/SkiaSharp.Views, os/Windows-Classic, backend/OpenGL |
| add-comment | medium | 0.88 (88%) | Explain the external root cause and provide workarounds | — |
| close-issue | medium | 0.85 (85%) | Close as external dependency issue | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and the call stack.

As confirmed by a GLWpfControl contributor in the comments, this crash originates in the external **GLWpfControl** (OpenTK) dependency, which uses a static shared OpenGL context that can only be current on one thread at a time. When a second `SKGLElement` (which inherits from `GLWpfControl`) is created on a different thread, the DX/GL interop initialization (`DXRegisterObjectNV`) fails because the shared context is already in use.

This is a limitation of the underlying GLWpfControl library tracked at [opentk/GLWpfControl#58](https://github.com/opentk/GLWpfControl/issues/58). SkiaSharp cannot resolve it without upstream changes.

**Workarounds:**
- Use `SKElement` (the software/raster renderer) instead of `SKGLElement` — it does not depend on GLWpfControl and works fine across multiple WPF windows on separate STA threads.
- Keep all `SKGLElement` instances on the single main UI thread.

Closing as the root cause is in an external dependency. Please reopen if the upstream library is fixed and the issue persists in SkiaSharp.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2872,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T15:25:42Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKGLElement crashes with an access violation in GLWpfControl (DXRegisterObjectNV) when a second SKGLElement is created on a separate STA thread on Windows WPF, because the underlying GLWpfControl uses a static shared OpenGL context that cannot be current on more than one thread simultaneously.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/OpenGL"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "Access violation in OpenTK.Graphics.Wgl.Wgl.DXRegisterObjectNV at GLWpfControl DxGLFramebuffer constructor",
      "stackTrace": "OpenTK.Graphics.Wgl.Wgl.DXRegisterObjectNV -> OpenTK.Wpf.DxGLFramebuffer.ctor -> GLWpfControlRenderer.SetSize -> GLWpfControl.OnRender -> SKGLElement.OnRender",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net3.x-windows"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a WPF application with an SKGLElement in the main window XAML",
        "Spawn a new STA thread that creates a second MainWindow (with SKGLElement) and calls form.Show() + Dispatcher.Run()",
        "Observe access violation crash in DXRegisterObjectNV within GLWpfControl"
      ],
      "codeSnippets": [
        "var t = new Thread(() => { var form = new MainWindow(); form.Show(); System.Windows.Threading.Dispatcher.Run(); }); t.SetApartmentState(ApartmentState.STA); t.Start();"
      ],
      "environmentDetails": "Windows 11 23H2 64-bit, Visual Studio, SkiaSharp 3.x Alpha",
      "repoLinks": [
        {
          "url": "https://github.com/opentk/GLWpfControl/issues/58",
          "description": "Related GLWpfControl upstream issue about multi-thread context sharing"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.x"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "SKGLElement still directly inherits from GLWpfControl without any per-thread context isolation. The upstream GLWpfControl static context limitation has not been resolved."
    }
  },
  "analysis": {
    "summary": "The crash originates in GLWpfControl's DX/GL interop layer (DXRegisterObjectNV) when a second instance is created on a new thread. GLWpfControl uses a static shared OpenGL context that is not thread-safe; creating a new GLWpfControl-derived control (SKGLElement) on a different thread tries to use that context while it may be current on the original thread, causing the access violation. This is a confirmed upstream GLWpfControl limitation.",
    "rationale": "The crash is real and reproducible — confirmed by both the call stack and a GLWpfControl maintainer (NogginBops). The root cause is in the external OpenTK/GLWpfControl dependency which uses a static context not designed for multi-thread instantiation. SKGLElement directly extends GLWpfControl without adding any thread isolation, so it inherits the limitation. The reporter acknowledged the external root cause and decided not to pursue the WPF migration. Classifying as close-as-external is appropriate since the fix requires changes in GLWpfControl, not SkiaSharp.",
    "keySignals": [
      {
        "text": "Currently creating GLWPFControls on separate threads is likely not going to work as GLWPFControl is using a static shared OpenGL context which can only be current on one thread.",
        "source": "comment by NogginBops (GLWpfControl contributor)",
        "interpretation": "Confirms the root cause is in the external GLWpfControl dependency — not in SkiaSharp itself."
      },
      {
        "text": "Not sure if related to https://github.com/opentk/GLWpfControl/issues/58",
        "source": "reporter comment",
        "interpretation": "Reporter already suspects the upstream GLWpfControl issue."
      },
      {
        "text": "I am going to hold off on my Winforms -> WPF rewrite for the time being",
        "source": "reporter comment",
        "interpretation": "Reporter accepted the limitation — no longer actively pursuing the issue."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKGLElement.cs",
        "lines": "29-65",
        "finding": "SKGLElement extends GLWpfControl directly and calls Start(settings) in Initialize() with no thread-safety or per-thread context management. There is no guard or documentation preventing use from multiple threads.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKGLElement.cs",
        "lines": "108-115",
        "finding": "OnRender delegates to base.OnRender which triggers GLWpfControl.OnRender. The crash happens inside GLWpfControl.DxGLFramebuffer constructor (DXRegisterObjectNV) — in the external OpenTK package — not in SkiaSharp code itself. SkiaSharp cannot fix this without forking or replacing GLWpfControl.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use SKCanvasView (software/raster renderer) instead of SKGLElement — it does not depend on GLWpfControl and supports multiple WPF windows on separate STA threads.",
      "Keep all SKGLElement instances on the main UI thread rather than creating windows on separate threads."
    ],
    "nextQuestions": [
      "Has GLWpfControl upsteam (opentk/GLWpfControl) fixed multi-thread context support in a newer version?",
      "Could SKGLElement be refactored to not use GLWpfControl (e.g., use ANGLE or WGL directly) to avoid this limitation?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.88,
      "reason": "The crash is in GLWpfControl (OpenTK external dependency) which uses a static shared OpenGL context not supporting multi-thread instantiation. A GLWpfControl contributor confirmed this. SkiaSharp cannot fix it without changes in the external library.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views, Windows labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-Classic",
          "backend/OpenGL"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain the external root cause and provide workarounds",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report and the call stack.\n\nAs confirmed by a GLWpfControl contributor in the comments, this crash originates in the external **GLWpfControl** (OpenTK) dependency, which uses a static shared OpenGL context that can only be current on one thread at a time. When a second `SKGLElement` (which inherits from `GLWpfControl`) is created on a different thread, the DX/GL interop initialization (`DXRegisterObjectNV`) fails because the shared context is already in use.\n\nThis is a limitation of the underlying GLWpfControl library tracked at [opentk/GLWpfControl#58](https://github.com/opentk/GLWpfControl/issues/58). SkiaSharp cannot resolve it without upstream changes.\n\n**Workarounds:**\n- Use `SKElement` (the software/raster renderer) instead of `SKGLElement` — it does not depend on GLWpfControl and works fine across multiple WPF windows on separate STA threads.\n- Keep all `SKGLElement` instances on the single main UI thread.\n\nClosing as the root cause is in an external dependency. Please reopen if the upstream library is fixed and the issue persists in SkiaSharp."
      },
      {
        "type": "close-issue",
        "description": "Close as external dependency issue",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
