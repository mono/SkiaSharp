# Issue Triage Report — #2326

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T06:18:00Z |
| Type | type/bug (0.93 (93%)) |
| Area | area/SkiaSharp.Views (0.95 (95%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** SKXamlCanvas used in a UWP print preview Page renders blank in version 2.88.3, regressing from 2.80.1, because PaintSurface is now fired asynchronously via Dispatcher rather than synchronously during SetPreviewPage.

**Analysis:** SKXamlCanvas.Invalidate() dispatches DoInvalidate() asynchronously via the UWP CoreDispatcher. In a print preview scenario the UWP print system captures the UIElement state immediately when SetPreviewPage is called; by that point the async invalidation has not yet completed, so the WriteableBitmap backing the canvas is still blank. In 2.80.1 the invalidation path was apparently synchronous or otherwise cooperated with the print lifecycle.

**Recommendations:** **needs-investigation** — Regression is well-evidenced and the likely cause (async Invalidate) is identified in the source. However, the fix requires confirming behaviour on a UWP Windows runner and possibly rethinking the Invalidate contract for off-screen/print use cases.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Universal-UWP |
| Backends | — |
| Tenets | tenet/compatibility, tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create a UWP app with a PageToPrint Page containing an SKXamlCanvas
2. Wire up the Windows.Graphics.Printing APIs (PrintDocument.Paginate, GetPreviewPage)
3. In PrintDocument_GetPreviewPage, call printDoc.SetPreviewPage(...) with the PageToPrint instance
4. Open Print Preview — the SKXamlCanvas renders blank

**Environment:** Windows 10, UWP minVersion 18362, Visual Studio 2019, SkiaSharp 2.88.3

**Code snippets:**

```csharp
Loaded event + SizeChanged triggers async Dispatch of DoInvalidate
```

```csharp
private void Invalidate() { Dispatcher?.RunAsync(CoreDispatcherPriority.Normal, DoInvalidate); }
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | Print preview page is blank; PaintSurface fires multiple times but not from SetPreviewPage call stack |
| Repro quality | partial |
| Target frameworks | uap10.0.18362 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.1, 2.88.3 |
| Worked in | 2.80.1 |
| Broke in | 2.88.3 |
| Current relevance | likely |
| Relevance reason | The async Dispatcher-based Invalidate pattern is still present in the current SKXamlCanvas.cs source. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.88 (88%) |
| Reason | Reporter confirms 2.80.1 worked and 2.88.3 does not. The async dispatch via Dispatcher.RunAsync introduced between those versions is the plausible root cause. |
| Worked in version | 2.80.1 |
| Broke in version | 2.88.3 |

## Analysis

### Technical Summary

SKXamlCanvas.Invalidate() dispatches DoInvalidate() asynchronously via the UWP CoreDispatcher. In a print preview scenario the UWP print system captures the UIElement state immediately when SetPreviewPage is called; by that point the async invalidation has not yet completed, so the WriteableBitmap backing the canvas is still blank. In 2.80.1 the invalidation path was apparently synchronous or otherwise cooperated with the print lifecycle.

### Rationale

The bug is classified as type/bug (regression) in area/SkiaSharp.Views because the failure is in SKXamlCanvas's async invalidation timing, not in the user's code. The platform is UWP. Severity is medium because it affects a specific scenario (printing) with a known workaround. suggestedAction is needs-investigation because the root cause is hypothesised but needs confirmation on a Windows UWP runner.

### Key Signals

- "with 2.80.1, the PaintSurface event is triggered (once) by printDoc.SetPreviewPage. In 2.88.3, the PaintSurface event is triggered multiple times, but not by SetPreviewPage. There is no triggering method in the stack trace." — **issue body** (Confirms the invalidation path changed to async dispatch; the paint no longer happens synchronously in the SetPreviewPage call chain.)
- "Version with issue: 2.88.3 / Last known good version: 2.80.1" — **issue body** (Clear regression window — change in Invalidate() async dispatch is the primary suspect.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 161-168 | direct | Invalidate() uses Dispatcher.RunAsync(CoreDispatcherPriority.Normal, DoInvalidate) for UWP, making every paint asynchronous. When the Windows print system calls SetPreviewPage and immediately renders the UIElement, DoInvalidate may not have executed yet. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 170-207 | direct | DoInvalidate() early-exits if ActualWidth or ActualHeight is non-positive. A print preview Page may not be in the visual tree and may not have been laid out, causing CreateSize() to return empty and the paint to be skipped entirely. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 125-139 | related | OnLoaded subscribes DpiChanged and calls OnDpiChanged (which calls Invalidate). In a print preview Page that may be Loaded/Unloaded multiple times the loadUnloadCounter guards this, but the first load still queues an async invalidation. |

### Workarounds

- Downgrade to SkiaSharp 2.80.x for the UWP project until the async timing issue is resolved.
- Instead of relying on SKXamlCanvas for print, render directly to an SKBitmap/SKSurface and then assign it to an Image control in the print preview page — this avoids the async Invalidate() entirely.

### Next Questions

- Does calling _canvasToPrint.Invalidate() explicitly after SetPreviewPage (with an await on the dispatcher) change the behaviour?
- Was the Invalidate() path synchronous in 2.80.1? A git bisect between 2.80.1 and 2.88.3 tags on the UWP SKXamlCanvas would confirm.
- Does the print preview Page have valid ActualWidth/ActualHeight when the print system renders it?

### Resolution Proposals

**Hypothesis:** The async Dispatcher.RunAsync in Invalidate() means the WriteableBitmap is not painted before the UWP print system captures the UIElement. A fix would either make invalidation synchronous when called from a non-UI-thread context, or expose a synchronous Redraw() method for use in print scenarios.

1. **Render directly to SKBitmap instead of SKXamlCanvas for print** — workaround, cost/m, validated=untested
   - For print preview pages, avoid SKXamlCanvas. Instead create an SKBitmap, draw to its canvas, then create a WriteableBitmap from the SKBitmap pixels and assign it to an Image control.
2. **Add synchronous Redraw() method to SKXamlCanvas** — fix, cost/s, validated=untested
   - Add a public Redraw() / InvalidateSync() method that calls DoInvalidate() directly on the calling thread (must be the UI thread). This gives callers in print scenarios a way to force synchronous rendering before handing the UIElement to the print system.

**Recommended proposal:** workaround-direct-render

**Why:** The direct-render workaround avoids the timing dependency entirely and works today without waiting for a SkiaSharp fix.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Regression is well-evidenced and the likely cause (async Invalidate) is identified in the source. However, the fix requires confirming behaviour on a UWP Windows runner and possibly rethinking the Invalidate contract for off-screen/print use cases. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply classification labels | labels=type/bug, area/SkiaSharp.Views, os/Windows-Universal-UWP, tenet/compatibility, tenet/reliability |
| add-comment | medium | 0.82 (82%) | Acknowledge regression, provide direct-render workaround, share root-cause hypothesis | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed reproduction steps and the debug observation about the call stack — that's exactly the signal we needed.

Here's a workaround you can use while we investigate. Rather than using `SKXamlCanvas` inside the print preview `Page`, render directly to an `SKBitmap` and assign its pixels to a `WriteableBitmap` hosted in a plain `Image` control. This avoids the async `Dispatcher.RunAsync` path that `SKXamlCanvas.Invalidate()` uses, which is the likely root cause:

```csharp
// In PrintDocument_GetPreviewPage (or wherever you size the page):
var info = new SKImageInfo((int)pageWidth, (int)pageHeight);
using var bitmap = new SKBitmap(info);
using var surface = SKSurface.Create(info, bitmap.GetPixels(), info.RowBytes);

// draw your content
var canvas = surface.Canvas;
canvas.Clear(SKColors.White);
// ... your drawing code ...

// Assign to a WriteableBitmap in the Image control
var wb = new WriteableBitmap((int)pageWidth, (int)pageHeight);
using (var stream = wb.PixelBuffer.AsStream())
    stream.Write(bitmap.Bytes, 0, bitmap.ByteCount);
wb.Invalidate();
myImageControl.Source = wb;
```

The root cause appears to be that `SKXamlCanvas.Invalidate()` dispatches painting asynchronously via `Dispatcher.RunAsync`. When the UWP print system calls `SetPreviewPage` and immediately captures the `UIElement`, the async paint job hasn't run yet, leaving the canvas blank. In 2.80.1 the invalidation path was apparently synchronous in this code path.

We'll investigate whether adding a synchronous `Redraw()` method to `SKXamlCanvas` is the right fix.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2326,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T06:18:00Z"
  },
  "summary": "SKXamlCanvas used in a UWP print preview Page renders blank in version 2.88.3, regressing from 2.80.1, because PaintSurface is now fired asynchronously via Dispatcher rather than synchronously during SetPreviewPage.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.93
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-Universal-UWP"
    ],
    "tenets": [
      "tenet/compatibility",
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "Print preview page is blank; PaintSurface fires multiple times but not from SetPreviewPage call stack",
      "reproQuality": "partial",
      "targetFrameworks": [
        "uap10.0.18362"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a UWP app with a PageToPrint Page containing an SKXamlCanvas",
        "Wire up the Windows.Graphics.Printing APIs (PrintDocument.Paginate, GetPreviewPage)",
        "In PrintDocument_GetPreviewPage, call printDoc.SetPreviewPage(...) with the PageToPrint instance",
        "Open Print Preview — the SKXamlCanvas renders blank"
      ],
      "codeSnippets": [
        "Loaded event + SizeChanged triggers async Dispatch of DoInvalidate",
        "private void Invalidate() { Dispatcher?.RunAsync(CoreDispatcherPriority.Normal, DoInvalidate); }"
      ],
      "environmentDetails": "Windows 10, UWP minVersion 18362, Visual Studio 2019, SkiaSharp 2.88.3",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.1",
        "2.88.3"
      ],
      "workedIn": "2.80.1",
      "brokeIn": "2.88.3",
      "currentRelevance": "likely",
      "relevanceReason": "The async Dispatcher-based Invalidate pattern is still present in the current SKXamlCanvas.cs source."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.88,
      "reason": "Reporter confirms 2.80.1 worked and 2.88.3 does not. The async dispatch via Dispatcher.RunAsync introduced between those versions is the plausible root cause.",
      "workedInVersion": "2.80.1",
      "brokeInVersion": "2.88.3"
    }
  },
  "analysis": {
    "summary": "SKXamlCanvas.Invalidate() dispatches DoInvalidate() asynchronously via the UWP CoreDispatcher. In a print preview scenario the UWP print system captures the UIElement state immediately when SetPreviewPage is called; by that point the async invalidation has not yet completed, so the WriteableBitmap backing the canvas is still blank. In 2.80.1 the invalidation path was apparently synchronous or otherwise cooperated with the print lifecycle.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "161-168",
        "finding": "Invalidate() uses Dispatcher.RunAsync(CoreDispatcherPriority.Normal, DoInvalidate) for UWP, making every paint asynchronous. When the Windows print system calls SetPreviewPage and immediately renders the UIElement, DoInvalidate may not have executed yet.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "170-207",
        "finding": "DoInvalidate() early-exits if ActualWidth or ActualHeight is non-positive. A print preview Page may not be in the visual tree and may not have been laid out, causing CreateSize() to return empty and the paint to be skipped entirely.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "125-139",
        "finding": "OnLoaded subscribes DpiChanged and calls OnDpiChanged (which calls Invalidate). In a print preview Page that may be Loaded/Unloaded multiple times the loadUnloadCounter guards this, but the first load still queues an async invalidation.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "with 2.80.1, the PaintSurface event is triggered (once) by printDoc.SetPreviewPage. In 2.88.3, the PaintSurface event is triggered multiple times, but not by SetPreviewPage. There is no triggering method in the stack trace.",
        "source": "issue body",
        "interpretation": "Confirms the invalidation path changed to async dispatch; the paint no longer happens synchronously in the SetPreviewPage call chain."
      },
      {
        "text": "Version with issue: 2.88.3 / Last known good version: 2.80.1",
        "source": "issue body",
        "interpretation": "Clear regression window — change in Invalidate() async dispatch is the primary suspect."
      }
    ],
    "rationale": "The bug is classified as type/bug (regression) in area/SkiaSharp.Views because the failure is in SKXamlCanvas's async invalidation timing, not in the user's code. The platform is UWP. Severity is medium because it affects a specific scenario (printing) with a known workaround. suggestedAction is needs-investigation because the root cause is hypothesised but needs confirmation on a Windows UWP runner.",
    "workarounds": [
      "Downgrade to SkiaSharp 2.80.x for the UWP project until the async timing issue is resolved.",
      "Instead of relying on SKXamlCanvas for print, render directly to an SKBitmap/SKSurface and then assign it to an Image control in the print preview page — this avoids the async Invalidate() entirely."
    ],
    "nextQuestions": [
      "Does calling _canvasToPrint.Invalidate() explicitly after SetPreviewPage (with an await on the dispatcher) change the behaviour?",
      "Was the Invalidate() path synchronous in 2.80.1? A git bisect between 2.80.1 and 2.88.3 tags on the UWP SKXamlCanvas would confirm.",
      "Does the print preview Page have valid ActualWidth/ActualHeight when the print system renders it?"
    ],
    "resolution": {
      "hypothesis": "The async Dispatcher.RunAsync in Invalidate() means the WriteableBitmap is not painted before the UWP print system captures the UIElement. A fix would either make invalidation synchronous when called from a non-UI-thread context, or expose a synchronous Redraw() method for use in print scenarios.",
      "proposals": [
        {
          "title": "Render directly to SKBitmap instead of SKXamlCanvas for print",
          "category": "workaround",
          "description": "For print preview pages, avoid SKXamlCanvas. Instead create an SKBitmap, draw to its canvas, then create a WriteableBitmap from the SKBitmap pixels and assign it to an Image control.",
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Add synchronous Redraw() method to SKXamlCanvas",
          "category": "fix",
          "description": "Add a public Redraw() / InvalidateSync() method that calls DoInvalidate() directly on the calling thread (must be the UI thread). This gives callers in print scenarios a way to force synchronous rendering before handing the UIElement to the print system.",
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "workaround-direct-render",
      "recommendedReason": "The direct-render workaround avoids the timing dependency entirely and works today without waiting for a SkiaSharp fix."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Regression is well-evidenced and the likely cause (async Invalidate) is identified in the source. However, the fix requires confirming behaviour on a UWP Windows runner and possibly rethinking the Invalidate contract for off-screen/print use cases.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply classification labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-Universal-UWP",
          "tenet/compatibility",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge regression, provide direct-render workaround, share root-cause hypothesis",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed reproduction steps and the debug observation about the call stack — that's exactly the signal we needed.\n\nHere's a workaround you can use while we investigate. Rather than using `SKXamlCanvas` inside the print preview `Page`, render directly to an `SKBitmap` and assign its pixels to a `WriteableBitmap` hosted in a plain `Image` control. This avoids the async `Dispatcher.RunAsync` path that `SKXamlCanvas.Invalidate()` uses, which is the likely root cause:\n\n```csharp\n// In PrintDocument_GetPreviewPage (or wherever you size the page):\nvar info = new SKImageInfo((int)pageWidth, (int)pageHeight);\nusing var bitmap = new SKBitmap(info);\nusing var surface = SKSurface.Create(info, bitmap.GetPixels(), info.RowBytes);\n\n// draw your content\nvar canvas = surface.Canvas;\ncanvas.Clear(SKColors.White);\n// ... your drawing code ...\n\n// Assign to a WriteableBitmap in the Image control\nvar wb = new WriteableBitmap((int)pageWidth, (int)pageHeight);\nusing (var stream = wb.PixelBuffer.AsStream())\n    stream.Write(bitmap.Bytes, 0, bitmap.ByteCount);\nwb.Invalidate();\nmyImageControl.Source = wb;\n```\n\nThe root cause appears to be that `SKXamlCanvas.Invalidate()` dispatches painting asynchronously via `Dispatcher.RunAsync`. When the UWP print system calls `SetPreviewPage` and immediately captures the `UIElement`, the async paint job hasn't run yet, leaving the canvas blank. In 2.80.1 the invalidation path was apparently synchronous in this code path.\n\nWe'll investigate whether adding a synchronous `Redraw()` method to `SKXamlCanvas` is the right fix."
      }
    ]
  }
}
```

</details>
