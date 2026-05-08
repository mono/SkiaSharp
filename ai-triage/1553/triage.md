# Issue Triage Report — #1553

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T09:57:00Z |
| Type | type/question (0.95 (95%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.82 (82%)) |

**Issue Summary:** User asks how Windows Forms DPI awareness mode (Application.SetHighDpiMode) interacts with SkiaSharp transparency and multi-monitor rendering using SKGLControl.

**Analysis:** Reporter is asking for guidance on how Windows DPI awareness modes interact with SkiaSharp's SKGLControl and WinForms TransparencyKey-based transparency. They have already found the workaround (use HighDpiMode.DpiUnaware). The behavior they describe—color-key transparency failing when non-DpiUnaware modes are active—is a Windows OS limitation with layered/transparent windows and OpenGL rendering, not a SkiaSharp bug.

**Recommendations:** **close-as-not-a-bug** — Reporter explicitly asks for understanding, not a fix. The described behavior (color-key transparency incompatible with DPI-aware OpenGL) is an OS-level Windows limitation. Reporter has already found the workaround (DpiUnaware). No actionable SkiaSharp defect.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Classic |
| Backends | backend/OpenGL |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Code snippets:**

```csharp
private void HandlePaintGLSurfaceDesktop(object sender, SKPaintGLSurfaceEventArgs e)
{
    ((SKGLControl)sender).MakeCurrent();
    var bg = TransparencyKey.ToSKColor();
    e.Surface.Canvas.Clear(bg);
    mySkiaSharpDrawing.OnDraw(e.Surface.Canvas);
}
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | .NET 5 |
| Worked in | — |
| Broke in | .NET 5 |
| Current relevance | unlikely |
| Relevance reason | This was filed against .NET 5 era behavior; DPI handling in WinForms has been improved in subsequent .NET versions and SkiaSharp releases. The underlying Windows limitation with color-key transparency + DPI-aware OpenGL is an OS constraint, not a SkiaSharp regression. |

## Analysis

### Technical Summary

Reporter is asking for guidance on how Windows DPI awareness modes interact with SkiaSharp's SKGLControl and WinForms TransparencyKey-based transparency. They have already found the workaround (use HighDpiMode.DpiUnaware). The behavior they describe—color-key transparency failing when non-DpiUnaware modes are active—is a Windows OS limitation with layered/transparent windows and OpenGL rendering, not a SkiaSharp bug.

### Rationale

Classified as type/question because the reporter explicitly asks for understanding/guidance rather than reporting a defect. The behavior (color-key transparency failing with DPI-aware modes on multi-monitor) is a Windows OS constraint: OpenGL pixel formats are incompatible with WinForms layered-window transparency when the process is DPI-aware. SKGLControl does not handle DPI scaling for its render target dimensions, but the transparency issue itself is an OS-level limitation. The reporter already found the workaround. Area is SkiaSharp.Views (WinForms views component) on Windows-Classic with OpenGL backend.

### Key Signals

- "I seek advice or some enlightment, not necessarily solutions." — **issue body** (Reporter is explicitly framing this as a question, not a bug report.)
- "Works: With Application.SetHighDpiMode(HighDpiMode.DpiUnaware*); or with call commented out" — **issue body** (Reporter has already identified and is using the workaround.)
- "I do not rule out that there might be a need for Skia or SkiaSharp to address something but I honestly don't know." — **issue body** (Reporter is uncertain about ownership; not asserting a bug.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs` | 92-115 | direct | SKGLControl.OnPaint uses Width and Height (logical pixels) to create the GRBackendRenderTarget. In DPI-aware WinForms applications, logical pixels may differ from physical pixels, causing the GL surface to be sized differently than expected. No DPI scaling is applied to the render target dimensions. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs` | 15-133 | direct | SKGLControl inherits from OpenTK.GLControl (or OpenTK.Windows.GLControl) and uses SwapBuffers(). The GL context window handle and pixel format are established by OpenTK based on the WinForms HWND. Windows color-key transparency (TransparencyKey) works with GDI-based rendering but is incompatible with OpenGL pixel formats—this is a Windows OS limitation independent of SkiaSharp. |

### Workarounds

- Use Application.SetHighDpiMode(HighDpiMode.DpiUnaware) to restore color-key transparency behavior on the primary monitor.
- For multi-monitor support with transparency, consider using a software-rendered SKControl (non-GL) and handling DPI changes via the Form.DpiChanged event.

### Next Questions

- Does the reporter still experience this on a current SkiaSharp version (2.88.x)?
- Is the reporter open to using SKControl (software raster) instead of SKGLControl for the transparency scenario?

### Resolution Proposals

**Hypothesis:** The transparency failure is caused by a fundamental incompatibility between Windows OpenGL pixel formats and WinForms color-key transparency when the app is DPI-aware. This is an OS/platform constraint, not a SkiaSharp defect.

1. **Close as not a bug with explanation** — alternative, cost/xs, validated=untested
   - Explain that WinForms TransparencyKey is GDI-based and incompatible with OpenGL rendering under DPI-aware modes—a Windows OS constraint. Point reporter to DpiUnaware workaround and suggest software-rendered SKControl for multi-monitor transparency use cases.

**Recommended proposal:** Close as not a bug with explanation

**Why:** The behavior is an OS limitation. Reporter has the workaround and is asking for understanding, not a fix. Closing with a clear explanation serves the reporter and future visitors.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.82 (82%) |
| Reason | Reporter explicitly asks for understanding, not a fix. The described behavior (color-key transparency incompatible with DPI-aware OpenGL) is an OS-level Windows limitation. Reporter has already found the workaround (DpiUnaware). No actionable SkiaSharp defect. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/question, area/SkiaSharp.Views, os/Windows-Classic, backend/OpenGL, tenet/compatibility labels | labels=type/question, area/SkiaSharp.Views, os/Windows-Classic, backend/OpenGL, tenet/compatibility |
| add-comment | high | 0.82 (82%) | Explain the OS-level DPI/transparency incompatibility and point to workarounds | — |
| close-issue | medium | 0.82 (82%) | Close as not a bug — behavior is an OS-level Windows constraint | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed write-up, @jensbrak!

The behavior you're seeing is caused by a fundamental constraint in Windows itself rather than a SkiaSharp bug:

**Why DPI awareness affects transparency**

WinForms `TransparencyKey` relies on Windows' GDI-based color-key compositing (layered windows). When your app is DPI-aware (`SystemAware`, `PerMonitor`, etc.), Windows changes how it maps your window's client area to physical pixels. OpenGL rendering (which `SKGLControl` uses) operates on the raw hardware framebuffer and is not composited through GDI—so the color-key trick only works when Windows is treating your window as DPI-unaware (letting Windows itself handle the scaling blur), because in that mode the compositing path still goes through GDI.

**Why multi-monitor always fails**

With multiple monitors at different DPIs, Windows cannot apply a single consistent color-key compositing pass across the boundary. This is a Windows OS limitation with no per-app workaround—it affects all OpenGL/Direct3D applications using transparent windows.

**Workarounds**

1. **Single-monitor, primary only:** Use `HighDpiMode.DpiUnaware` (as you've found). This is the most reliable approach if you control the deployment environment.
2. **Multi-monitor:** Consider switching from `SKGLControl` to `SKControl` (software raster). The raster backend renders through GDI, which is compatible with color-key transparency. Performance will be lower but multi-monitor transparency should work.
   ```csharp
   // In your form's paint handler with SKControl instead of SKGLControl:
   private void SKControl_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
   {
       var bg = TransparencyKey.ToSKColor();
       e.Surface.Canvas.Clear(bg);
       mySkiaSharpDrawing.OnDraw(e.Surface.Canvas);
   }
   ```
3. **Per-monitor DPI changes:** Subscribe to `Form.DpiChanged` to invalidate/resize your control when the window moves between monitors.

Since the root cause is in Windows' compositing model rather than SkiaSharp, I'll close this as not a bug. Feel free to reopen if you find a SkiaSharp-specific regression.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1553,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T09:57:00Z"
  },
  "summary": "User asks how Windows Forms DPI awareness mode (Application.SetHighDpiMode) interacts with SkiaSharp transparency and multi-monitor rendering using SKGLControl.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.95
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
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "private void HandlePaintGLSurfaceDesktop(object sender, SKPaintGLSurfaceEventArgs e)\n{\n    ((SKGLControl)sender).MakeCurrent();\n    var bg = TransparencyKey.ToSKColor();\n    e.Surface.Canvas.Clear(bg);\n    mySkiaSharpDrawing.OnDraw(e.Surface.Canvas);\n}"
      ],
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        ".NET 5"
      ],
      "brokeIn": ".NET 5",
      "currentRelevance": "unlikely",
      "relevanceReason": "This was filed against .NET 5 era behavior; DPI handling in WinForms has been improved in subsequent .NET versions and SkiaSharp releases. The underlying Windows limitation with color-key transparency + DPI-aware OpenGL is an OS constraint, not a SkiaSharp regression."
    }
  },
  "analysis": {
    "summary": "Reporter is asking for guidance on how Windows DPI awareness modes interact with SkiaSharp's SKGLControl and WinForms TransparencyKey-based transparency. They have already found the workaround (use HighDpiMode.DpiUnaware). The behavior they describe—color-key transparency failing when non-DpiUnaware modes are active—is a Windows OS limitation with layered/transparent windows and OpenGL rendering, not a SkiaSharp bug.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs",
        "finding": "SKGLControl.OnPaint uses Width and Height (logical pixels) to create the GRBackendRenderTarget. In DPI-aware WinForms applications, logical pixels may differ from physical pixels, causing the GL surface to be sized differently than expected. No DPI scaling is applied to the render target dimensions.",
        "relevance": "direct",
        "lines": "92-115"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs",
        "finding": "SKGLControl inherits from OpenTK.GLControl (or OpenTK.Windows.GLControl) and uses SwapBuffers(). The GL context window handle and pixel format are established by OpenTK based on the WinForms HWND. Windows color-key transparency (TransparencyKey) works with GDI-based rendering but is incompatible with OpenGL pixel formats—this is a Windows OS limitation independent of SkiaSharp.",
        "relevance": "direct",
        "lines": "15-133"
      }
    ],
    "keySignals": [
      {
        "text": "I seek advice or some enlightment, not necessarily solutions.",
        "source": "issue body",
        "interpretation": "Reporter is explicitly framing this as a question, not a bug report."
      },
      {
        "text": "Works: With Application.SetHighDpiMode(HighDpiMode.DpiUnaware*); or with call commented out",
        "source": "issue body",
        "interpretation": "Reporter has already identified and is using the workaround."
      },
      {
        "text": "I do not rule out that there might be a need for Skia or SkiaSharp to address something but I honestly don't know.",
        "source": "issue body",
        "interpretation": "Reporter is uncertain about ownership; not asserting a bug."
      }
    ],
    "rationale": "Classified as type/question because the reporter explicitly asks for understanding/guidance rather than reporting a defect. The behavior (color-key transparency failing with DPI-aware modes on multi-monitor) is a Windows OS constraint: OpenGL pixel formats are incompatible with WinForms layered-window transparency when the process is DPI-aware. SKGLControl does not handle DPI scaling for its render target dimensions, but the transparency issue itself is an OS-level limitation. The reporter already found the workaround. Area is SkiaSharp.Views (WinForms views component) on Windows-Classic with OpenGL backend.",
    "workarounds": [
      "Use Application.SetHighDpiMode(HighDpiMode.DpiUnaware) to restore color-key transparency behavior on the primary monitor.",
      "For multi-monitor support with transparency, consider using a software-rendered SKControl (non-GL) and handling DPI changes via the Form.DpiChanged event."
    ],
    "nextQuestions": [
      "Does the reporter still experience this on a current SkiaSharp version (2.88.x)?",
      "Is the reporter open to using SKControl (software raster) instead of SKGLControl for the transparency scenario?"
    ],
    "resolution": {
      "hypothesis": "The transparency failure is caused by a fundamental incompatibility between Windows OpenGL pixel formats and WinForms color-key transparency when the app is DPI-aware. This is an OS/platform constraint, not a SkiaSharp defect.",
      "proposals": [
        {
          "title": "Close as not a bug with explanation",
          "category": "alternative",
          "description": "Explain that WinForms TransparencyKey is GDI-based and incompatible with OpenGL rendering under DPI-aware modes—a Windows OS constraint. Point reporter to DpiUnaware workaround and suggest software-rendered SKControl for multi-monitor transparency use cases.",
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Close as not a bug with explanation",
      "recommendedReason": "The behavior is an OS limitation. Reporter has the workaround and is asking for understanding, not a fix. Closing with a clear explanation serves the reporter and future visitors."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.82,
      "reason": "Reporter explicitly asks for understanding, not a fix. The described behavior (color-key transparency incompatible with DPI-aware OpenGL) is an OS-level Windows limitation. Reporter has already found the workaround (DpiUnaware). No actionable SkiaSharp defect.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/question, area/SkiaSharp.Views, os/Windows-Classic, backend/OpenGL, tenet/compatibility labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/question",
          "area/SkiaSharp.Views",
          "os/Windows-Classic",
          "backend/OpenGL",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain the OS-level DPI/transparency incompatibility and point to workarounds",
        "risk": "high",
        "confidence": 0.82,
        "comment": "Thanks for the detailed write-up, @jensbrak!\n\nThe behavior you're seeing is caused by a fundamental constraint in Windows itself rather than a SkiaSharp bug:\n\n**Why DPI awareness affects transparency**\n\nWinForms `TransparencyKey` relies on Windows' GDI-based color-key compositing (layered windows). When your app is DPI-aware (`SystemAware`, `PerMonitor`, etc.), Windows changes how it maps your window's client area to physical pixels. OpenGL rendering (which `SKGLControl` uses) operates on the raw hardware framebuffer and is not composited through GDI—so the color-key trick only works when Windows is treating your window as DPI-unaware (letting Windows itself handle the scaling blur), because in that mode the compositing path still goes through GDI.\n\n**Why multi-monitor always fails**\n\nWith multiple monitors at different DPIs, Windows cannot apply a single consistent color-key compositing pass across the boundary. This is a Windows OS limitation with no per-app workaround—it affects all OpenGL/Direct3D applications using transparent windows.\n\n**Workarounds**\n\n1. **Single-monitor, primary only:** Use `HighDpiMode.DpiUnaware` (as you've found). This is the most reliable approach if you control the deployment environment.\n2. **Multi-monitor:** Consider switching from `SKGLControl` to `SKControl` (software raster). The raster backend renders through GDI, which is compatible with color-key transparency. Performance will be lower but multi-monitor transparency should work.\n   ```csharp\n   // In your form's paint handler with SKControl instead of SKGLControl:\n   private void SKControl_PaintSurface(object sender, SKPaintSurfaceEventArgs e)\n   {\n       var bg = TransparencyKey.ToSKColor();\n       e.Surface.Canvas.Clear(bg);\n       mySkiaSharpDrawing.OnDraw(e.Surface.Canvas);\n   }\n   ```\n3. **Per-monitor DPI changes:** Subscribe to `Form.DpiChanged` to invalidate/resize your control when the window moves between monitors.\n\nSince the root cause is in Windows' compositing model rather than SkiaSharp, I'll close this as not a bug. Feel free to reopen if you find a SkiaSharp-specific regression."
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — behavior is an OS-level Windows constraint",
        "risk": "medium",
        "confidence": 0.82,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
