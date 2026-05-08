# Issue Triage Report — #2580

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T13:33:41Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp.Views (0.95 (95%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** SKElement.InvalidateVisual() triggers OnPaintSurface but the visual output on screen does not update on WPF Windows; changing the Width property instead forces a visible redraw.

**Analysis:** SKElement.InvalidateVisual() correctly triggers OnRender and OnPaintSurface (confirmed by Console output), but the updated WriteableBitmap contents are not reflected on screen on some Windows configurations. The root cause is likely a WPF composition layer issue where WriteableBitmap change notifications (AddDirtyRect/Unlock) do not reliably propagate to the hardware-accelerated compositor on certain GPU driver configurations when triggered from within an OnRender callback. A secondary commenter (issue #2176) reports the same pattern—works for most users but fails intermittently on some machines, confirming the hardware/driver dependency.

**Recommendations:** **needs-investigation** — Real bug with partial repro code, confirmed by a second user. OnPaintSurface fires correctly so the issue is in WPF composition propagation; root cause requires further investigation on a Windows machine with specific GPU drivers.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Classic |
| Backends | backend/Raster |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a WPF app with a SKElement named MainCanvas
2. Start a System.Timers.Timer at 50ms interval
3. On each tick call Dispatcher.Invoke to increment a variable x, then call MainCanvas.InvalidateVisual()
4. In OnPaintSurface draw a circle at x position
5. Observe: Console.WriteLine(x) prints increasing values but the circle on screen does not move

**Environment:** SkiaSharp 2.88.3, WPF, Windows 10, Rider IDE

**Related issues:** #2176, #1351, #1506, #328

**Code snippets:**

```csharp
var timer = new Timer(50);
timer.Elapsed += (_, _) => Dispatcher.Invoke(() => { x += 10; MainCanvas.InvalidateVisual(); });
timer.Start();
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Visual does not update after SKElement.InvalidateVisual(); OnPaintSurface is called but screen stays unchanged |
| Repro quality | partial |
| Target frameworks | net-unknown |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The SKElement.OnRender implementation using WriteableBitmap has not changed materially since 2.88.3; the same code path is present in the current codebase. |

## Analysis

### Technical Summary

SKElement.InvalidateVisual() correctly triggers OnRender and OnPaintSurface (confirmed by Console output), but the updated WriteableBitmap contents are not reflected on screen on some Windows configurations. The root cause is likely a WPF composition layer issue where WriteableBitmap change notifications (AddDirtyRect/Unlock) do not reliably propagate to the hardware-accelerated compositor on certain GPU driver configurations when triggered from within an OnRender callback. A secondary commenter (issue #2176) reports the same pattern—works for most users but fails intermittently on some machines, confirming the hardware/driver dependency.

### Rationale

The reporter explicitly confirms OnPaintSurface IS being called (via Console.WriteLine), ruling out a rendering-loop issue. The visual-only failure, the workaround of changing Width (which triggers a full layout+render cycle), and the corroborating report in #2176 all point to a WPF composition/DirectX integration issue with WriteableBitmap when only a render-invalidation (without layout) is performed.

### Key Signals

- "x changes, OnPaintSurface is called, but a circle doesn't move" — **issue body** (The drawing pipeline runs but the composition layer ignores the updated bitmap pixels.)
- "if I change Width property instead of this call, then graphics is updated" — **issue body** (A full layout+render cycle forces WPF to re-compose; a bare InvalidateVisual() (render-only) does not. This points to a WPF retained-mode rendering optimisation or a composition-layer integration gap.)
- "I also encountered this problem on some computers." — **comment #2014171311 (VollYoung)** (Intermittent occurrence across hardware confirms a GPU driver or WPF rendering mode dependency rather than a pure code defect.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs` | 45-88 | direct | OnRender locks the WriteableBitmap, draws Skia content to its BackBuffer, calls AddDirtyRect and Unlock, then records DrawImage(bitmap, rect) in the DrawingContext. The bitmap object is reused across renders when the size is unchanged (lines 66-69). This design relies on WPF propagating the WriteableBitmap.Changed notification to the composition thread — which may not be reliable on all GPU/driver configurations. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs` | 97-102 | related | OnRenderSizeChanged calls InvalidateVisual() — same code path as the reporter's explicit call — yet this works because a layout pass (triggered by Width change) precedes it and forces a full visual re-composition that the standalone InvalidateVisual() path skips. |

### Workarounds

- Change a layout property (e.g., Width += 0.001; Width -= 0.001) to trigger a full layout+render cycle on each tick — confirmed to work by the reporter.
- Disable WPF hardware rendering: System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly — may resolve composition propagation issues on affected GPUs.
- Use CompositionTarget.Rendering event for animation instead of a timer: CompositionTarget.Rendering += (s,e) => { x+=10; InvalidateVisual(); }; — hooks into WPF's own render loop.

### Next Questions

- Does disabling WPF hardware acceleration (RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly) make InvalidateVisual() work correctly?
- Does using CompositionTarget.Rendering instead of a timer-driven InvalidateVisual() resolve the issue?
- Which GPU driver / Direct3D feature level reproduces the issue most reliably?
- Does the SKGLElement (OpenGL-backed) show the same problem?

### Resolution Proposals

**Hypothesis:** On certain GPU driver configurations, WPF's composition layer does not re-composite a visual when only WriteableBitmap.Changed fires without a concurrent layout invalidation. SKElement may need to additionally invalidate the visual's render data or force a layout pass to ensure the compositor picks up the updated bitmap.

1. **Force layout invalidation alongside render invalidation** — fix, confidence 0.60 (60%), cost/s, validated=untested
   - Override the public InvalidateVisual call on SKElement or hook into the rendering path to also call InvalidateMeasure(), ensuring WPF's full layout+render cycle runs and forces a compositor update.
2. **Workaround: use CompositionTarget.Rendering for animation** — workaround, confidence 0.75 (75%), cost/xs, validated=untested
   - Instead of driving invalidation from a timer, attach to WPF's CompositionTarget.Rendering event and call InvalidateVisual() there — this runs inside WPF's own render cycle and avoids the cross-thread notification race.

**Recommended proposal:** Workaround: use CompositionTarget.Rendering for animation

**Why:** Lowest risk and effort; directly sidesteps the WPF composition race without changing SKElement internals. The fix proposal needs deeper investigation into WPF rendering internals first.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Real bug with partial repro code, confirmed by a second user. OnPaintSurface fires correctly so the issue is in WPF composition propagation; root cause requires further investigation on a Windows machine with specific GPU drivers. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, views, and platform labels | labels=type/bug, area/SkiaSharp.Views, os/Windows-Classic, backend/Raster, tenet/reliability |
| add-comment | medium | 0.82 (82%) | Acknowledge the bug, share analysis and workarounds, ask for GPU/driver details | — |
| link-related | low | 0.88 (88%) | Cross-reference related intermittent SKElement rendering issue | linkedIssue=#2176 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and code sample! Your observation that `OnPaintSurface` is called (console shows increasing `x`) but the visual doesn't update points to a WPF composition-layer issue — specifically, on some GPU/driver configurations the `WriteableBitmap` change notification does not reliably propagate to the DirectX compositor when triggered only via `InvalidateVisual()` without a preceding layout pass.

This is also reported intermittently in #2176.

**Workarounds to try:**

1. **Use `CompositionTarget.Rendering`** instead of a timer:
   ```csharp
   System.Windows.Media.CompositionTarget.Rendering += (s, e) =>
   {
       x += 10;
       MainCanvas.InvalidateVisual();
   };
   ```
   This hooks into WPF's own render loop and avoids the timing race.

2. **Disable hardware rendering** to see if it is a GPU driver issue:
   ```csharp
   System.Windows.Media.RenderOptions.ProcessRenderMode =
       System.Windows.Interop.RenderMode.SoftwareOnly;
   ```

Could you also share your GPU model and Windows version? That would help isolate whether this is driver-specific.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2580,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T13:33:41Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKElement.InvalidateVisual() triggers OnPaintSurface but the visual output on screen does not update on WPF Windows; changing the Width property instead forces a visible redraw.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/Raster"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "Visual does not update after SKElement.InvalidateVisual(); OnPaintSurface is called but screen stays unchanged",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net-unknown"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a WPF app with a SKElement named MainCanvas",
        "Start a System.Timers.Timer at 50ms interval",
        "On each tick call Dispatcher.Invoke to increment a variable x, then call MainCanvas.InvalidateVisual()",
        "In OnPaintSurface draw a circle at x position",
        "Observe: Console.WriteLine(x) prints increasing values but the circle on screen does not move"
      ],
      "codeSnippets": [
        "var timer = new Timer(50);\ntimer.Elapsed += (_, _) => Dispatcher.Invoke(() => { x += 10; MainCanvas.InvalidateVisual(); });\ntimer.Start();"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, WPF, Windows 10, Rider IDE",
      "relatedIssues": [
        2176,
        1351,
        1506,
        328
      ],
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The SKElement.OnRender implementation using WriteableBitmap has not changed materially since 2.88.3; the same code path is present in the current codebase."
    }
  },
  "analysis": {
    "summary": "SKElement.InvalidateVisual() correctly triggers OnRender and OnPaintSurface (confirmed by Console output), but the updated WriteableBitmap contents are not reflected on screen on some Windows configurations. The root cause is likely a WPF composition layer issue where WriteableBitmap change notifications (AddDirtyRect/Unlock) do not reliably propagate to the hardware-accelerated compositor on certain GPU driver configurations when triggered from within an OnRender callback. A secondary commenter (issue #2176) reports the same pattern—works for most users but fails intermittently on some machines, confirming the hardware/driver dependency.",
    "rationale": "The reporter explicitly confirms OnPaintSurface IS being called (via Console.WriteLine), ruling out a rendering-loop issue. The visual-only failure, the workaround of changing Width (which triggers a full layout+render cycle), and the corroborating report in #2176 all point to a WPF composition/DirectX integration issue with WriteableBitmap when only a render-invalidation (without layout) is performed.",
    "keySignals": [
      {
        "text": "x changes, OnPaintSurface is called, but a circle doesn't move",
        "source": "issue body",
        "interpretation": "The drawing pipeline runs but the composition layer ignores the updated bitmap pixels."
      },
      {
        "text": "if I change Width property instead of this call, then graphics is updated",
        "source": "issue body",
        "interpretation": "A full layout+render cycle forces WPF to re-compose; a bare InvalidateVisual() (render-only) does not. This points to a WPF retained-mode rendering optimisation or a composition-layer integration gap."
      },
      {
        "text": "I also encountered this problem on some computers.",
        "source": "comment #2014171311 (VollYoung)",
        "interpretation": "Intermittent occurrence across hardware confirms a GPU driver or WPF rendering mode dependency rather than a pure code defect."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs",
        "lines": "45-88",
        "finding": "OnRender locks the WriteableBitmap, draws Skia content to its BackBuffer, calls AddDirtyRect and Unlock, then records DrawImage(bitmap, rect) in the DrawingContext. The bitmap object is reused across renders when the size is unchanged (lines 66-69). This design relies on WPF propagating the WriteableBitmap.Changed notification to the composition thread — which may not be reliable on all GPU/driver configurations.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs",
        "lines": "97-102",
        "finding": "OnRenderSizeChanged calls InvalidateVisual() — same code path as the reporter's explicit call — yet this works because a layout pass (triggered by Width change) precedes it and forces a full visual re-composition that the standalone InvalidateVisual() path skips.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Does disabling WPF hardware acceleration (RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly) make InvalidateVisual() work correctly?",
      "Does using CompositionTarget.Rendering instead of a timer-driven InvalidateVisual() resolve the issue?",
      "Which GPU driver / Direct3D feature level reproduces the issue most reliably?",
      "Does the SKGLElement (OpenGL-backed) show the same problem?"
    ],
    "workarounds": [
      "Change a layout property (e.g., Width += 0.001; Width -= 0.001) to trigger a full layout+render cycle on each tick — confirmed to work by the reporter.",
      "Disable WPF hardware rendering: System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly — may resolve composition propagation issues on affected GPUs.",
      "Use CompositionTarget.Rendering event for animation instead of a timer: CompositionTarget.Rendering += (s,e) => { x+=10; InvalidateVisual(); }; — hooks into WPF's own render loop."
    ],
    "resolution": {
      "hypothesis": "On certain GPU driver configurations, WPF's composition layer does not re-composite a visual when only WriteableBitmap.Changed fires without a concurrent layout invalidation. SKElement may need to additionally invalidate the visual's render data or force a layout pass to ensure the compositor picks up the updated bitmap.",
      "proposals": [
        {
          "title": "Force layout invalidation alongside render invalidation",
          "description": "Override the public InvalidateVisual call on SKElement or hook into the rendering path to also call InvalidateMeasure(), ensuring WPF's full layout+render cycle runs and forces a compositor update.",
          "category": "fix",
          "confidence": 0.6,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Workaround: use CompositionTarget.Rendering for animation",
          "description": "Instead of driving invalidation from a timer, attach to WPF's CompositionTarget.Rendering event and call InvalidateVisual() there — this runs inside WPF's own render cycle and avoids the cross-thread notification race.",
          "category": "workaround",
          "confidence": 0.75,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Workaround: use CompositionTarget.Rendering for animation",
      "recommendedReason": "Lowest risk and effort; directly sidesteps the WPF composition race without changing SKElement internals. The fix proposal needs deeper investigation into WPF rendering internals first."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Real bug with partial repro code, confirmed by a second user. OnPaintSurface fires correctly so the issue is in WPF composition propagation; root cause requires further investigation on a Windows machine with specific GPU drivers.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views, and platform labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-Classic",
          "backend/Raster",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the bug, share analysis and workarounds, ask for GPU/driver details",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report and code sample! Your observation that `OnPaintSurface` is called (console shows increasing `x`) but the visual doesn't update points to a WPF composition-layer issue — specifically, on some GPU/driver configurations the `WriteableBitmap` change notification does not reliably propagate to the DirectX compositor when triggered only via `InvalidateVisual()` without a preceding layout pass.\n\nThis is also reported intermittently in #2176.\n\n**Workarounds to try:**\n\n1. **Use `CompositionTarget.Rendering`** instead of a timer:\n   ```csharp\n   System.Windows.Media.CompositionTarget.Rendering += (s, e) =>\n   {\n       x += 10;\n       MainCanvas.InvalidateVisual();\n   };\n   ```\n   This hooks into WPF's own render loop and avoids the timing race.\n\n2. **Disable hardware rendering** to see if it is a GPU driver issue:\n   ```csharp\n   System.Windows.Media.RenderOptions.ProcessRenderMode =\n       System.Windows.Interop.RenderMode.SoftwareOnly;\n   ```\n\nCould you also share your GPU model and Windows version? That would help isolate whether this is driver-specific."
      },
      {
        "type": "link-related",
        "description": "Cross-reference related intermittent SKElement rendering issue",
        "risk": "low",
        "confidence": 0.88,
        "linkedIssue": 2176
      }
    ]
  }
}
```

</details>
