# Issue Triage Report — #3255

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T18:38:30Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp.Views.Maui (0.90 (90%)) |
| Suggested action | needs-investigation (0.85 (85%)) |

**Issue Summary:** On .NET MAUI Windows, SKCanvasView.InvalidateSurface() called at app startup causes OnPaintSurface to fire (with loadUnloadCounter==1) but the drawn content is not visible on screen until a subsequent invalidation.

**Analysis:** MAUI on Windows fires the WinUI Loaded/Unloaded events multiple times during app initialization. Each Unloaded call with loadUnloadCounter reaching 0 triggers FreeBitmap(), setting Background=null and discarding the rendered bitmap. Although OnPaintSurface is called while loadUnloadCounter==1, the content is wiped shortly after by FreeBitmap() when Unloaded fires. No subsequent automatic Invalidate() is triggered to re-create the Background brush, leaving the canvas visually blank.

**Recommendations:** **needs-investigation** — Clear regression with an identified code path and working workaround. Root cause is understood but the correct fix needs careful testing to avoid reintroducing issue #1118.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/Windows-WinUI |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a .NET MAUI app for Windows with a custom SKCanvasView (or multiple)
2. Call InvalidateSurface during or immediately after app startup
3. Observe that OnPaintSurface is invoked (possibly multiple times) but canvas remains blank
4. When the canvas is later invalidated (e.g., by user interaction), content appears correctly

**Environment:** Windows 11, Visual Studio on Windows, SkiaSharp 3.116.0, .NET MAUI

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1118 — Original UWP bug that introduced loadUnloadCounter workaround in SKXamlCanvas

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | Canvas renders blank during initial app load on MAUI Windows; content appears only after a subsequent InvalidateSurface call |
| Repro quality | partial |
| Target frameworks | net9.0-windows10.0.19041 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | likely |
| Relevance reason | The loadUnloadCounter logic in SKXamlCanvas has not changed since being introduced; the MAUI Windows handler still wraps SKXamlCanvas directly |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.82 (82%) |
| Reason | Reporter explicitly states worked in 2.88.9 and broke in 3.116.0. The SkiaSharp.Views.Maui package and its Windows handler (SKXamlCanvas) were significantly changed during the MAUI migration. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.0 |

## Analysis

### Technical Summary

MAUI on Windows fires the WinUI Loaded/Unloaded events multiple times during app initialization. Each Unloaded call with loadUnloadCounter reaching 0 triggers FreeBitmap(), setting Background=null and discarding the rendered bitmap. Although OnPaintSurface is called while loadUnloadCounter==1, the content is wiped shortly after by FreeBitmap() when Unloaded fires. No subsequent automatic Invalidate() is triggered to re-create the Background brush, leaving the canvas visually blank.

### Rationale

Bug classification is clear: the behavior is broken (OnPaintSurface fires but nothing displays) and regressed from 2.88.9 to 3.116.0. The area is SkiaSharp.Views.Maui because the issue manifests specifically through the MAUI Windows handler wrapping SKXamlCanvas. The loadUnloadCounter pattern (introduced as a workaround for #1118) does not handle rapid Loaded/Unloaded cycling from MAUI's visual tree setup correctly.

### Key Signals

- "the value of SKXamlCanvas loadUnloadCounter was 1 [when nothing shows]; when things show up correctly, loadUnloadCounter is 0" — **issue body** (When counter==1 the canvas has just been loaded; when Unloaded fires (counter->0) FreeBitmap() destroys the background. The subsequent Loaded re-registers events but the repaint from OnXamlRootChanged may run before layout is complete, or is wiped by another Unloaded.)
- "calling InvalidateSurface on the SKXamlCanvas.Unloaded event - and it worked" — **issue body** (Forcing an Invalidate after FreeBitmap() re-creates the Background brush at a point where the visual tree is ready, bypassing the timing issue.)
- "Last Known Good Version of SkiaSharp: 2.88.9" — **issue body** (Confirms regression introduced in 3.x MAUI migration cycle.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 45-46,125-158 | direct | loadUnloadCounter tracks Loaded/Unloaded cycles. OnLoaded only runs setup (XamlRoot.Changed subscription + OnXamlRootChanged → Invalidate) when counter==1. OnUnloaded calls FreeBitmap() when counter reaches 0, setting Background=null and clearing the bitmap. If MAUI fires multiple Loaded/Unloaded events during startup, FreeBitmap() can wipe a just-painted canvas without any subsequent auto-invalidation. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Windows.cs` | 13-39 | related | SKCanvasViewHandler creates SKXamlCanvas as the platform view and hooks PaintSurface. OnInvalidateSurface calls platformView.Invalidate(). There is no special handling for the Windows Loaded/Unloaded lifecycle beyond what SKXamlCanvas provides. |

### Workarounds

- Subclass SKCanvasView, hook HandlerChanged, find the underlying SKXamlCanvas, and subscribe to its Unloaded event to call this.InvalidateSurface() — exactly the workaround the reporter discovered.
- Delay the initial InvalidateSurface call (e.g., via Dispatcher.Dispatch) until after the view is fully loaded and stable.

### Next Questions

- Does the same blank-canvas behaviour occur with SKGLView on MAUI Windows?
- How many Loaded/Unloaded cycles does MAUI actually fire during startup on Windows?
- Would calling Invalidate() unconditionally (not just for counter==1) at the end of OnLoaded fix the issue without reintroducing #1118?

### Resolution Proposals

**Hypothesis:** The fix is to call Invalidate() at the end of OnLoaded regardless of loadUnloadCounter value, so that every Loaded event (not just the first) triggers a repaint after FreeBitmap() cleared the background.

1. **Call Invalidate() on every Loaded event** — fix, confidence 0.72 (72%), cost/s, validated=untested
   - In SKXamlCanvas.OnLoaded, move the Invalidate() call (currently inside OnXamlRootChanged) outside the counter==1 guard so it fires on every Loaded event. This ensures the background brush is re-created after any FreeBitmap() call.
2. **Workaround: invalidate on Unloaded** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - Subclass SKCanvasView and hook the Unloaded event on the SKXamlCanvas platform view to call InvalidateSurface() as the reporter did.

```csharp
public class MySKCanvasView : SKCanvasView
{
    public MySKCanvasView()
    {
        HandlerChanged += OnHandlerChanged;
    }

    private void OnHandlerChanged(object? sender, EventArgs e)
    {
#if WINDOWS
        if (Handler?.PlatformView is SkiaSharp.Views.Windows.SKXamlCanvas canvas)
            canvas.Unloaded += (_, _) => InvalidateSurface();
#endif
    }
}
```

**Recommended proposal:** Workaround: invalidate on Unloaded

**Why:** The reporter has already confirmed this workaround works. The fix requires deeper investigation to ensure it does not reintroduce the original #1118 issue.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.85 (85%) |
| Reason | Clear regression with an identified code path and working workaround. Root cause is understood but the correct fix needs careful testing to avoid reintroducing issue #1118. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, MAUI views area, Windows WinUI, reliability tenet labels | labels=type/bug, area/SkiaSharp.Views.Maui, os/Windows-WinUI, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Acknowledge the regression, confirm the workaround, and note the code investigation findings | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed write-up and for sharing the workaround!

After investigating the code, the root cause appears to be in `SKXamlCanvas` (`source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs`). MAUI fires the WinUI `Loaded`/`Unloaded` events multiple times during app startup. Each time `Unloaded` fires with `loadUnloadCounter` reaching 0, `FreeBitmap()` is called — this sets `Background = null` and discards the rendered bitmap. Although `OnPaintSurface` is invoked correctly while the counter is 1, the output is wiped by the next `Unloaded`. No automatic re-invalidation follows to rebuild the `Background` brush.

Your workaround is correct and confirmed:
```csharp
public class MySKCanvasView : SKCanvasView
{
    public MySKCanvasView()
    {
        HandlerChanged += OnHandlerChanged;
    }

    private void OnHandlerChanged(object? sender, EventArgs e)
    {
#if WINDOWS
        if (Handler?.PlatformView is SkiaSharp.Views.Windows.SKXamlCanvas canvas)
            canvas.Unloaded += (_, _) => InvalidateSurface();
#endif
    }
}
```

This is a regression from 2.88.9 introduced during the MAUI migration. We'll investigate the proper fix (likely calling `Invalidate()` on every `Loaded` event, not just the first).
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3255,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T18:38:30Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "On .NET MAUI Windows, SKCanvasView.InvalidateSurface() called at app startup causes OnPaintSurface to fire (with loadUnloadCounter==1) but the drawn content is not visible on screen until a subsequent invalidation.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-WinUI"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "Canvas renders blank during initial app load on MAUI Windows; content appears only after a subsequent InvalidateSurface call",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net9.0-windows10.0.19041"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET MAUI app for Windows with a custom SKCanvasView (or multiple)",
        "Call InvalidateSurface during or immediately after app startup",
        "Observe that OnPaintSurface is invoked (possibly multiple times) but canvas remains blank",
        "When the canvas is later invalidated (e.g., by user interaction), content appears correctly"
      ],
      "environmentDetails": "Windows 11, Visual Studio on Windows, SkiaSharp 3.116.0, .NET MAUI",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1118",
          "description": "Original UWP bug that introduced loadUnloadCounter workaround in SKXamlCanvas"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "2.88.9"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.116.0",
      "currentRelevance": "likely",
      "relevanceReason": "The loadUnloadCounter logic in SKXamlCanvas has not changed since being introduced; the MAUI Windows handler still wraps SKXamlCanvas directly"
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.82,
      "reason": "Reporter explicitly states worked in 2.88.9 and broke in 3.116.0. The SkiaSharp.Views.Maui package and its Windows handler (SKXamlCanvas) were significantly changed during the MAUI migration.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.0"
    }
  },
  "analysis": {
    "summary": "MAUI on Windows fires the WinUI Loaded/Unloaded events multiple times during app initialization. Each Unloaded call with loadUnloadCounter reaching 0 triggers FreeBitmap(), setting Background=null and discarding the rendered bitmap. Although OnPaintSurface is called while loadUnloadCounter==1, the content is wiped shortly after by FreeBitmap() when Unloaded fires. No subsequent automatic Invalidate() is triggered to re-create the Background brush, leaving the canvas visually blank.",
    "rationale": "Bug classification is clear: the behavior is broken (OnPaintSurface fires but nothing displays) and regressed from 2.88.9 to 3.116.0. The area is SkiaSharp.Views.Maui because the issue manifests specifically through the MAUI Windows handler wrapping SKXamlCanvas. The loadUnloadCounter pattern (introduced as a workaround for #1118) does not handle rapid Loaded/Unloaded cycling from MAUI's visual tree setup correctly.",
    "keySignals": [
      {
        "text": "the value of SKXamlCanvas loadUnloadCounter was 1 [when nothing shows]; when things show up correctly, loadUnloadCounter is 0",
        "source": "issue body",
        "interpretation": "When counter==1 the canvas has just been loaded; when Unloaded fires (counter->0) FreeBitmap() destroys the background. The subsequent Loaded re-registers events but the repaint from OnXamlRootChanged may run before layout is complete, or is wiped by another Unloaded."
      },
      {
        "text": "calling InvalidateSurface on the SKXamlCanvas.Unloaded event - and it worked",
        "source": "issue body",
        "interpretation": "Forcing an Invalidate after FreeBitmap() re-creates the Background brush at a point where the visual tree is ready, bypassing the timing issue."
      },
      {
        "text": "Last Known Good Version of SkiaSharp: 2.88.9",
        "source": "issue body",
        "interpretation": "Confirms regression introduced in 3.x MAUI migration cycle."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "45-46,125-158",
        "finding": "loadUnloadCounter tracks Loaded/Unloaded cycles. OnLoaded only runs setup (XamlRoot.Changed subscription + OnXamlRootChanged → Invalidate) when counter==1. OnUnloaded calls FreeBitmap() when counter reaches 0, setting Background=null and clearing the bitmap. If MAUI fires multiple Loaded/Unloaded events during startup, FreeBitmap() can wipe a just-painted canvas without any subsequent auto-invalidation.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Windows.cs",
        "lines": "13-39",
        "finding": "SKCanvasViewHandler creates SKXamlCanvas as the platform view and hooks PaintSurface. OnInvalidateSurface calls platformView.Invalidate(). There is no special handling for the Windows Loaded/Unloaded lifecycle beyond what SKXamlCanvas provides.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Subclass SKCanvasView, hook HandlerChanged, find the underlying SKXamlCanvas, and subscribe to its Unloaded event to call this.InvalidateSurface() — exactly the workaround the reporter discovered.",
      "Delay the initial InvalidateSurface call (e.g., via Dispatcher.Dispatch) until after the view is fully loaded and stable."
    ],
    "nextQuestions": [
      "Does the same blank-canvas behaviour occur with SKGLView on MAUI Windows?",
      "How many Loaded/Unloaded cycles does MAUI actually fire during startup on Windows?",
      "Would calling Invalidate() unconditionally (not just for counter==1) at the end of OnLoaded fix the issue without reintroducing #1118?"
    ],
    "resolution": {
      "hypothesis": "The fix is to call Invalidate() at the end of OnLoaded regardless of loadUnloadCounter value, so that every Loaded event (not just the first) triggers a repaint after FreeBitmap() cleared the background.",
      "proposals": [
        {
          "title": "Call Invalidate() on every Loaded event",
          "description": "In SKXamlCanvas.OnLoaded, move the Invalidate() call (currently inside OnXamlRootChanged) outside the counter==1 guard so it fires on every Loaded event. This ensures the background brush is re-created after any FreeBitmap() call.",
          "category": "fix",
          "confidence": 0.72,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Workaround: invalidate on Unloaded",
          "description": "Subclass SKCanvasView and hook the Unloaded event on the SKXamlCanvas platform view to call InvalidateSurface() as the reporter did.",
          "codeSnippet": "public class MySKCanvasView : SKCanvasView\n{\n    public MySKCanvasView()\n    {\n        HandlerChanged += OnHandlerChanged;\n    }\n\n    private void OnHandlerChanged(object? sender, EventArgs e)\n    {\n#if WINDOWS\n        if (Handler?.PlatformView is SkiaSharp.Views.Windows.SKXamlCanvas canvas)\n            canvas.Unloaded += (_, _) => InvalidateSurface();\n#endif\n    }\n}",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Workaround: invalidate on Unloaded",
      "recommendedReason": "The reporter has already confirmed this workaround works. The fix requires deeper investigation to ensure it does not reintroduce the original #1118 issue."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "Clear regression with an identified code path and working workaround. Root cause is understood but the correct fix needs careful testing to avoid reintroducing issue #1118.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, MAUI views area, Windows WinUI, reliability tenet labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Maui",
          "os/Windows-WinUI",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the regression, confirm the workaround, and note the code investigation findings",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed write-up and for sharing the workaround!\n\nAfter investigating the code, the root cause appears to be in `SKXamlCanvas` (`source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs`). MAUI fires the WinUI `Loaded`/`Unloaded` events multiple times during app startup. Each time `Unloaded` fires with `loadUnloadCounter` reaching 0, `FreeBitmap()` is called — this sets `Background = null` and discards the rendered bitmap. Although `OnPaintSurface` is invoked correctly while the counter is 1, the output is wiped by the next `Unloaded`. No automatic re-invalidation follows to rebuild the `Background` brush.\n\nYour workaround is correct and confirmed:\n```csharp\npublic class MySKCanvasView : SKCanvasView\n{\n    public MySKCanvasView()\n    {\n        HandlerChanged += OnHandlerChanged;\n    }\n\n    private void OnHandlerChanged(object? sender, EventArgs e)\n    {\n#if WINDOWS\n        if (Handler?.PlatformView is SkiaSharp.Views.Windows.SKXamlCanvas canvas)\n            canvas.Unloaded += (_, _) => InvalidateSurface();\n#endif\n    }\n}\n```\n\nThis is a regression from 2.88.9 introduced during the MAUI migration. We'll investigate the proper fix (likely calling `Invalidate()` on every `Loaded` event, not just the first)."
      }
    ]
  }
}
```

</details>
