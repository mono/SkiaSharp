# Issue Triage Report — #2294

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-23T08:19:29Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views.Maui (0.95 (95%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** SKCanvasView (and SKGLView) never fires PaintSurface and does not draw on physical iOS devices (iPad, iPhone) in MAUI, while it works correctly on Windows and iOS simulators.

**Analysis:** The SKCanvasView fails to render on physical iOS devices in MAUI. PaintSurface never fires, and touch events are also non-functional, pointing to a handler or view lifecycle issue specific to the physical device rendering path. The same bug existed in Xamarin.Forms (#341) and was fixed, but has re-emerged with the MAUI handler architecture. Multiple downstream libraries (LiveCharts2, Mapsui) have reported the same issue, confirming it is systemic.

**Recommendations:** **needs-investigation** — Real bug with complete repro code, confirmed by multiple users on physical iOS devices and macOS. Root cause is unclear but hypothesis is testable. A physical device and deeper MAUI lifecycle investigation are needed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/iOS |
| Backends | — |
| Tenets | — |
| Partner | partner/maui |

## Evidence

### Reproduction

1. Create a MAUI app with SKCanvasView, EnableTouchEvents=true, PaintSurface handler that draws a red square
2. Call UseSkiaSharp() in the app builder
3. Run on Windows or iOS Simulator — canvas draws red as expected
4. Deploy to a physical iPad or iPhone — canvas remains black, PaintSurface never fires

**Environment:** SkiaSharp.Views.Maui.Controls 2.88.3, Visual Studio Pro 17.4.0 Preview 4.0, physical iPad and iPhone 8 (iOS 16), also reported on macOS Sonoma

**Related issues:** #341

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/341 — Identical Xamarin.Forms issue (closed as fixed in 2017) — this is the MAUI recurrence
- https://github.com/beto-rodriguez/LiveCharts2/issues/800 — LiveCharts2 downstream report of same issue with workaround
- https://github.com/Mapsui/Mapsui/issues/1872 — Mapsui downstream report of same issue

**Screenshots:**
- https://user-images.githubusercontent.com/5587254/197871740-ad130ed2-9a9c-4d42-8d52-4a01e13036d5.png — Windows: canvas correctly renders red square
- https://user-images.githubusercontent.com/5587254/197871769-415a74fc-ccbe-4a41-8730-dcf17038e83a.PNG — iPad: canvas is completely black, PaintSurface never fired

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | missing-output |
| Error message | PaintSurface event never fires on physical iPad; canvas remains black |
| Repro quality | partial |
| Target frameworks | net6.0-ios |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | A 2024 comment reports the same issue on macOS Sonoma, indicating this was not fixed in 2.88.x and likely persists in current versions. |

## Analysis

### Technical Summary

The SKCanvasView fails to render on physical iOS devices in MAUI. PaintSurface never fires, and touch events are also non-functional, pointing to a handler or view lifecycle issue specific to the physical device rendering path. The same bug existed in Xamarin.Forms (#341) and was fixed, but has re-emerged with the MAUI handler architecture. Multiple downstream libraries (LiveCharts2, Mapsui) have reported the same issue, confirming it is systemic.

### Rationale

This is clearly a type/bug in the MAUI views layer for Apple platforms. The behavior (not drawing, not responding to touch) is completely broken on physical iOS/macOS devices. The area is area/SkiaSharp.Views.Maui. The most likely root cause is a race condition or ordering problem between MAUI's handler ConnectHandler lifecycle and the iOS/iPadOS initial draw cycle — physical hardware and simulators differ in timing of layout passes. A workaround exists via downstream library reports.

### Key Signals

- "the canvas never fires the PaintSurface event, and so ends up black" — **issue body** (Total failure of drawing on physical device — Draw(CGRect) is never called or PaintSurface proxy is not firing)
- "On Windows and in the simulators, the canvas in my sample app works" — **issue body** (Platform-specific lifecycle difference between simulator and physical hardware)
- "the Touch event isn't firing on the iPad either (it does on Windows)" — **issue body** (Both drawing and touch are broken — the handler/native view setup is failing, not just the drawing path)
- "Same issue here on an iphone 8, iOS version 16" — **comment by AlexanderSCK** (Confirmed on additional physical devices — not device-model specific)
- "Also broken on my MacBook running MacOS Sonoma" — **comment by Nebula-Developer (April 2024)** (Issue persists in newer versions and extends to macOS physical devices)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Apple.cs` | 15-28 | direct | ConnectHandler attaches PaintSurfaceProxy and SKTouchHandlerProxy via weak references. If the weak reference is collected or the VirtualView becomes null before the platform view draws, neither PaintSurface nor Touch events will fire. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKCanvasView.cs` | 117-129 | direct | WillMoveToWindow calls SetNeedsDisplay() when window is non-null to trigger an initial draw. However, if called before MAUI's handler has a chance to wire the PaintSurface proxy, the initial draw fires with no listener. The drawable is disposed (but not nulled) on WillMoveToWindow(null), so subsequent calls to Draw() with a disposed-but-non-null drawable would fail silently. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKCanvasView.cs` | 138-143 | related | LayoutSubviews calls Layer.SetNeedsDisplay() which should trigger Draw(). However, if the view layout pass occurs before ConnectHandler sets up the PaintSurface proxy, the first draw will produce nothing and no subsequent redraw may be triggered. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKEventProxy.cs` | 9-12 | related | VirtualView is stored as WeakReference. On physical devices, if the MAUI view is collected before Draw() is triggered (e.g., due to memory pressure or GC timing differences vs simulator), the VirtualView will be null and PaintSurface will silently do nothing. |

### Workarounds

- Use SKGLView instead of SKCanvasView (uses Metal/OpenGL rendering path which may not be affected by the same CoreGraphics draw-cycle timing issue)
- Explicitly call InvalidateSurface() in the page's OnAppearing() override to force a redraw after the page is fully visible

### Next Questions

- Does the issue reproduce with the latest SkiaSharp (3.x) or only 2.88.x?
- Is the bug timing-related — does a delayed call to InvalidateSurface() after OnAppearing fix it?
- Does the WillMoveToWindow drawable disposal/non-null bug contribute to subsequent failure after app backgrounding?
- Does the issue reproduce on macOS Catalyst in addition to iOS?

### Resolution Proposals

**Hypothesis:** The MAUI handler's ConnectHandler completes after the initial iOS UIView layout and draw cycle on physical devices (which runs on real hardware timing), so WillMoveToWindow/LayoutSubviews fire before the PaintSurface proxy is wired. On simulators, timing is slower/different and the proxy is set up in time.

1. **Use SKGLView as a workaround** — workaround, confidence 0.65 (65%), cost/xs, validated=untested
   - Switch to SKGLView in XAML instead of SKCanvasView. SKGLView uses a Metal/OpenGL surface that redraws on a different cycle and may not be affected by the CoreGraphics draw-cycle timing issue.
2. **Investigate timing of ConnectHandler vs WillMoveToWindow** — fix, confidence 0.70 (70%), cost/xs, validated=untested
   - Add a SetNeedsDisplay() call at the end of ConnectHandler in SKCanvasViewHandler.Apple.cs to trigger a redraw after the proxy is wired. This ensures the first draw happens after the PaintSurface event is connected.

**Recommended proposal:** Investigate timing of ConnectHandler vs WillMoveToWindow

**Why:** Fixing the root cause in the handler is the correct long-term solution. The hypothesis that ConnectHandler fires after the initial draw cycle is testable and a one-line SetNeedsDisplay() call at the end of ConnectHandler could fix it.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Real bug with complete repro code, confirmed by multiple users on physical iOS devices and macOS. Root cause is unclear but hypothesis is testable. A physical device and deeper MAUI lifecycle investigation are needed. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, MAUI views, iOS, and partner labels | labels=type/bug, area/SkiaSharp.Views.Maui, os/iOS, partner/maui |
| add-comment | medium | 0.80 (80%) | Acknowledge the bug, provide workaround, and ask for version confirmation | — |
| link-related | low | 0.90 (90%) | Cross-reference the original Xamarin.Forms issue #341 | linkedIssue=#341 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report! This looks like a real bug where the `PaintSurface` event proxy is not yet connected when the iOS `UIView` initial draw cycle fires on physical hardware. A workaround that has helped others in downstream libraries is to try `SKGLView` instead of `SKCanvasView`, or to call `SampleCanvas.InvalidateSurface()` from your page's `OnAppearing()` override to force a redraw after the view is fully visible.

Could you confirm whether this also reproduces with the latest SkiaSharp version (3.x)? That will help us determine if this was already fixed in a newer release.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2294,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-23T08:19:29Z"
  },
  "summary": "SKCanvasView (and SKGLView) never fires PaintSurface and does not draw on physical iOS devices (iPad, iPhone) in MAUI, while it works correctly on Windows and iOS simulators.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.95
    },
    "platforms": [
      "os/iOS"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "missing-output",
      "errorMessage": "PaintSurface event never fires on physical iPad; canvas remains black",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net6.0-ios"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a MAUI app with SKCanvasView, EnableTouchEvents=true, PaintSurface handler that draws a red square",
        "Call UseSkiaSharp() in the app builder",
        "Run on Windows or iOS Simulator — canvas draws red as expected",
        "Deploy to a physical iPad or iPhone — canvas remains black, PaintSurface never fires"
      ],
      "environmentDetails": "SkiaSharp.Views.Maui.Controls 2.88.3, Visual Studio Pro 17.4.0 Preview 4.0, physical iPad and iPhone 8 (iOS 16), also reported on macOS Sonoma",
      "screenshots": [
        {
          "url": "https://user-images.githubusercontent.com/5587254/197871740-ad130ed2-9a9c-4d42-8d52-4a01e13036d5.png",
          "description": "Windows: canvas correctly renders red square"
        },
        {
          "url": "https://user-images.githubusercontent.com/5587254/197871769-415a74fc-ccbe-4a41-8730-dcf17038e83a.PNG",
          "description": "iPad: canvas is completely black, PaintSurface never fired"
        }
      ],
      "relatedIssues": [
        341
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/341",
          "description": "Identical Xamarin.Forms issue (closed as fixed in 2017) — this is the MAUI recurrence"
        },
        {
          "url": "https://github.com/beto-rodriguez/LiveCharts2/issues/800",
          "description": "LiveCharts2 downstream report of same issue with workaround"
        },
        {
          "url": "https://github.com/Mapsui/Mapsui/issues/1872",
          "description": "Mapsui downstream report of same issue"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "A 2024 comment reports the same issue on macOS Sonoma, indicating this was not fixed in 2.88.x and likely persists in current versions."
    }
  },
  "analysis": {
    "summary": "The SKCanvasView fails to render on physical iOS devices in MAUI. PaintSurface never fires, and touch events are also non-functional, pointing to a handler or view lifecycle issue specific to the physical device rendering path. The same bug existed in Xamarin.Forms (#341) and was fixed, but has re-emerged with the MAUI handler architecture. Multiple downstream libraries (LiveCharts2, Mapsui) have reported the same issue, confirming it is systemic.",
    "keySignals": [
      {
        "text": "the canvas never fires the PaintSurface event, and so ends up black",
        "source": "issue body",
        "interpretation": "Total failure of drawing on physical device — Draw(CGRect) is never called or PaintSurface proxy is not firing"
      },
      {
        "text": "On Windows and in the simulators, the canvas in my sample app works",
        "source": "issue body",
        "interpretation": "Platform-specific lifecycle difference between simulator and physical hardware"
      },
      {
        "text": "the Touch event isn't firing on the iPad either (it does on Windows)",
        "source": "issue body",
        "interpretation": "Both drawing and touch are broken — the handler/native view setup is failing, not just the drawing path"
      },
      {
        "text": "Same issue here on an iphone 8, iOS version 16",
        "source": "comment by AlexanderSCK",
        "interpretation": "Confirmed on additional physical devices — not device-model specific"
      },
      {
        "text": "Also broken on my MacBook running MacOS Sonoma",
        "source": "comment by Nebula-Developer (April 2024)",
        "interpretation": "Issue persists in newer versions and extends to macOS physical devices"
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Apple.cs",
        "lines": "15-28",
        "finding": "ConnectHandler attaches PaintSurfaceProxy and SKTouchHandlerProxy via weak references. If the weak reference is collected or the VirtualView becomes null before the platform view draws, neither PaintSurface nor Touch events will fire.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKCanvasView.cs",
        "lines": "117-129",
        "finding": "WillMoveToWindow calls SetNeedsDisplay() when window is non-null to trigger an initial draw. However, if called before MAUI's handler has a chance to wire the PaintSurface proxy, the initial draw fires with no listener. The drawable is disposed (but not nulled) on WillMoveToWindow(null), so subsequent calls to Draw() with a disposed-but-non-null drawable would fail silently.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKCanvasView.cs",
        "lines": "138-143",
        "finding": "LayoutSubviews calls Layer.SetNeedsDisplay() which should trigger Draw(). However, if the view layout pass occurs before ConnectHandler sets up the PaintSurface proxy, the first draw will produce nothing and no subsequent redraw may be triggered.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKEventProxy.cs",
        "lines": "9-12",
        "finding": "VirtualView is stored as WeakReference. On physical devices, if the MAUI view is collected before Draw() is triggered (e.g., due to memory pressure or GC timing differences vs simulator), the VirtualView will be null and PaintSurface will silently do nothing.",
        "relevance": "related"
      }
    ],
    "rationale": "This is clearly a type/bug in the MAUI views layer for Apple platforms. The behavior (not drawing, not responding to touch) is completely broken on physical iOS/macOS devices. The area is area/SkiaSharp.Views.Maui. The most likely root cause is a race condition or ordering problem between MAUI's handler ConnectHandler lifecycle and the iOS/iPadOS initial draw cycle — physical hardware and simulators differ in timing of layout passes. A workaround exists via downstream library reports.",
    "workarounds": [
      "Use SKGLView instead of SKCanvasView (uses Metal/OpenGL rendering path which may not be affected by the same CoreGraphics draw-cycle timing issue)",
      "Explicitly call InvalidateSurface() in the page's OnAppearing() override to force a redraw after the page is fully visible"
    ],
    "nextQuestions": [
      "Does the issue reproduce with the latest SkiaSharp (3.x) or only 2.88.x?",
      "Is the bug timing-related — does a delayed call to InvalidateSurface() after OnAppearing fix it?",
      "Does the WillMoveToWindow drawable disposal/non-null bug contribute to subsequent failure after app backgrounding?",
      "Does the issue reproduce on macOS Catalyst in addition to iOS?"
    ],
    "resolution": {
      "hypothesis": "The MAUI handler's ConnectHandler completes after the initial iOS UIView layout and draw cycle on physical devices (which runs on real hardware timing), so WillMoveToWindow/LayoutSubviews fire before the PaintSurface proxy is wired. On simulators, timing is slower/different and the proxy is set up in time.",
      "proposals": [
        {
          "title": "Use SKGLView as a workaround",
          "description": "Switch to SKGLView in XAML instead of SKCanvasView. SKGLView uses a Metal/OpenGL surface that redraws on a different cycle and may not be affected by the CoreGraphics draw-cycle timing issue.",
          "category": "workaround",
          "confidence": 0.65,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Investigate timing of ConnectHandler vs WillMoveToWindow",
          "description": "Add a SetNeedsDisplay() call at the end of ConnectHandler in SKCanvasViewHandler.Apple.cs to trigger a redraw after the proxy is wired. This ensures the first draw happens after the PaintSurface event is connected.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Investigate timing of ConnectHandler vs WillMoveToWindow",
      "recommendedReason": "Fixing the root cause in the handler is the correct long-term solution. The hypothesis that ConnectHandler fires after the initial draw cycle is testable and a one-line SetNeedsDisplay() call at the end of ConnectHandler could fix it."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Real bug with complete repro code, confirmed by multiple users on physical iOS devices and macOS. Root cause is unclear but hypothesis is testable. A physical device and deeper MAUI lifecycle investigation are needed.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, MAUI views, iOS, and partner labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Maui",
          "os/iOS",
          "partner/maui"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the bug, provide workaround, and ask for version confirmation",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the detailed report! This looks like a real bug where the `PaintSurface` event proxy is not yet connected when the iOS `UIView` initial draw cycle fires on physical hardware. A workaround that has helped others in downstream libraries is to try `SKGLView` instead of `SKCanvasView`, or to call `SampleCanvas.InvalidateSurface()` from your page's `OnAppearing()` override to force a redraw after the view is fully visible.\n\nCould you confirm whether this also reproduces with the latest SkiaSharp version (3.x)? That will help us determine if this was already fixed in a newer release."
      },
      {
        "type": "link-related",
        "description": "Cross-reference the original Xamarin.Forms issue #341",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 341
      }
    ]
  }
}
```

</details>
