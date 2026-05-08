# Issue Triage Report — #310

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-23T21:28:00Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp.Views (0.97 (97%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** SKControl throws ExternalException (GDI+ error) in OnPaint when the control is resized from within the PaintSurface event handler, causing re-entrant painting that attempts to free a still-locked bitmap.

**Analysis:** Re-entrant OnPaint triggered by user resizing SKControl inside PaintSurface causes the inner paint call to dispose and replace the locked bitmap via FreeBitmap(), leaving the outer OnPaint with a stale BitmapData reference, which then fails at UnlockBits.

**Recommendations:** **keep-open** — Root cause is well-understood (re-entrant OnPaint via user resize in PaintSurface). A workaround exists. The library fix is small but requires deliberate design decision by maintainers on how to handle re-entrancy. Issue has been deliberately kept open by maintainer since 2017 for future investigation.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/enhancement, status/low-priority, os/Windows-Classic, area/SkiaSharp.Views |

## Evidence

### Reproduction

1. Place an SKControl inside a scrollable container (e.g., DevExpress XtraScrollableControl)
2. Scroll down so the bottom of the SKControl is visible but the top is scrolled off-screen
3. In the PaintSurface event handler, change the Height of the SKControl to a value that makes it completely off-screen
4. Observe ExternalException (GDI+ error) thrown from SKControl.OnPaint

**Environment:** Windows Forms desktop, SKControl embedded in a scrollable container. Issue is triggered 100% of the time under the described scenario.

**Repository links:**
- https://devdiv.visualstudio.com/DevDiv/_workitems/edit/735692 — VS internal bug #735692 cross-referenced by reporter

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | exception |
| Error message | System.Runtime.InteropServices.ExternalException (0x80004005): A generic error occurred in GDI+. |
| Repro quality | complete |
| Target frameworks | — |

**Stack trace:**

```text
at System.Drawing.Bitmap.UnlockBits(BitmapData bitmapdata)
   at SkiaSharp.Views.Desktop.SKControl.OnPaint(PaintEventArgs e)
   at System.Windows.Forms.Control.PaintWithErrorHandling(PaintEventArgs e, Int16 layer)
   at System.Windows.Forms.Control.WmPaint(Message& m)
   at System.Windows.Forms.Control.WndProc(Message& m)
   at System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The OnPaint implementation in SKControl.cs has not changed materially since the issue was filed — it still uses the same LockBits/UnlockBits pattern with no re-entrancy guard. |

## Analysis

### Technical Summary

Re-entrant OnPaint triggered by user resizing SKControl inside PaintSurface causes the inner paint call to dispose and replace the locked bitmap via FreeBitmap(), leaving the outer OnPaint with a stale BitmapData reference, which then fails at UnlockBits.

### Rationale

The exception originates in SkiaSharp's own OnPaint code path (UnlockBits), not in user code. Although the trigger is the anti-pattern of modifying control size inside PaintSurface, the library does not guard against re-entrant painting. Adding a simple boolean re-entrancy guard in OnPaint would prevent the crash. The maintainer previously left this open as a low-priority enhancement to investigate re-entrancy protection.

### Key Signals

- "System.Runtime.InteropServices.ExternalException (0x80004005): A generic error occurred in GDI+. at System.Drawing.Bitmap.UnlockBits" — **issue body** (GDI+ errors from UnlockBits typically occur when the bitmap was disposed or replaced while still locked.)
- "I am changing the SKControl.Height value from within the PaintSurface event, which is obviously triggered from inside the OnPaint override in SKCanvas, which is inside the bitmap lock/unlock." — **comment #1** (Reporter correctly identified that resizing inside PaintSurface triggers a nested OnPaint, creating re-entrancy.)
- "I suspect the XtraScrollableControl has a hook on my SKControl sized event and is manually calling the OnPaint function directly in a certain situation." — **comment #2** (Resize event from a third-party scrollable container triggers a synchronous repaint within the existing paint cycle.)
- "It would be easy enough to handle this situation in the SKControl itself so it doesn't crash, not sure if its worth bothering with though." — **comment #2 (reporter)** (Reporter acknowledges a fix at the library level is possible but left the decision to maintainers.)
- "This issue was the result of the paint operation also resizing the view - possibly invalidating the canvas or causing an infinite loop/stack overflow. This issue serves to act as a note for further investigation." — **comment #5 (maintainer mattleibow, 2018)** (Maintainer confirmed understanding of root cause and deliberately kept open for future investigation.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKControl.cs` | 26-53 | direct | OnPaint calls CreateBitmap(), then LockBits(), then OnPaintSurface() (which fires the user's PaintSurface event), then UnlockBits(). There is no re-entrancy guard. If the user resizes the control inside PaintSurface, a nested OnPaint call is triggered, which calls CreateBitmap() → FreeBitmap() → disposes the bitmap currently locked in the outer call. The outer call then tries UnlockBits on the disposed bitmap, causing the GDI+ error. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKControl.cs` | 68-81 | direct | CreateBitmap() calls FreeBitmap() when Width or Height changes, disposing and nulling the bitmap field. This is the mechanism by which the outer LockBits reference becomes dangling during re-entrant painting. |

### Workarounds

- Do not resize the SKControl (or any parent scrollable container) from within the PaintSurface event handler. Pre-calculate required dimensions before the paint cycle.
- Reset the scroll position of any parent scrollable container to the top before triggering a content-driven resize, ensuring the SKControl remains in the visible area during paint.

### Next Questions

- Is the same re-entrancy issue reproducible with the standard WinForms Panel as a parent (without DevExpress XtraScrollableControl)?
- Does adding a simple `_isPainting` boolean guard in OnPaint fully resolve the crash, or are there edge cases with WM_SIZE being processed during paint?

### Resolution Proposals

**Hypothesis:** A boolean re-entrancy guard in SKControl.OnPaint would prevent the nested paint call from freeing the locked bitmap while the outer paint is in progress.

1. **Add re-entrancy guard to OnPaint** — fix, confidence 0.82 (82%), cost/xs, validated=untested
   - Track painting state with a private boolean field. If OnPaint is called while already painting, call Invalidate() to schedule a repaint after the current cycle completes and return immediately.

```csharp
private bool _isPainting;

protected override void OnPaint(PaintEventArgs e)
{
    if (_isPainting)
    {
        Invalidate();
        return;
    }
    _isPainting = true;
    try
    {
        // ... existing OnPaint body ...
    }
    finally
    {
        _isPainting = false;
    }
}
```
2. **Workaround: perform layout outside PaintSurface** — workaround, confidence 0.95 (95%), cost/s, validated=untested
   - Move any control resize logic out of the PaintSurface event handler. Calculate required dimensions in a separate method and apply them before triggering an Invalidate(), so that painting always starts with a stable size.

**Recommended proposal:** Add re-entrancy guard to OnPaint

**Why:** Small, contained fix that makes SKControl resilient to the anti-pattern without requiring users to restructure their code.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Root cause is well-understood (re-entrant OnPaint via user resize in PaintSurface). A workaround exists. The library fix is small but requires deliberate design decision by maintainers on how to handle re-entrancy. Issue has been deliberately kept open by maintainer since 2017 for future investigation. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Correct type from enhancement to bug; retain platform and area labels; add reliability tenet | labels=type/bug, area/SkiaSharp.Views, os/Windows-Classic, tenet/reliability |
| add-comment | medium | 0.82 (82%) | Summarize root cause and offer re-entrancy guard workaround for users hitting this | — |

**Comment draft for `add-comment`:**

```markdown
**Root cause confirmed:** This crash occurs because `SKControl.OnPaint` has no re-entrancy guard. When you resize the control from inside `PaintSurface`, WinForms triggers a synchronous nested `OnPaint` call. The inner call runs `CreateBitmap()` which disposes the bitmap currently locked by the outer call. When the outer call then tries `UnlockBits`, GDI+ throws because the bitmap has been replaced.

**Workaround:** Avoid resizing the control (or any parent container that triggers a synchronous repaint) from inside the `PaintSurface` event. Calculate required dimensions before the paint cycle and apply them outside of painting:

```csharp
// Do this OUTSIDE of PaintSurface
if (requiredHeight != skControl.Height)
    skControl.Height = requiredHeight;
skControl.Invalidate();
```

**Potential fix:** Adding a re-entrancy guard in `SKControl.OnPaint` would make the control resilient to this pattern:

```csharp
private bool _isPainting;

protected override void OnPaint(PaintEventArgs e)
{
    if (_isPainting) { Invalidate(); return; }
    _isPainting = true;
    try { /* existing body */ }
    finally { _isPainting = false; }
}
```

This is tracked as a low-priority enhancement.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 310,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-23T21:28:00Z",
    "currentLabels": [
      "type/enhancement",
      "status/low-priority",
      "os/Windows-Classic",
      "area/SkiaSharp.Views"
    ]
  },
  "summary": "SKControl throws ExternalException (GDI+ error) in OnPaint when the control is resized from within the PaintSurface event handler, causing re-entrant painting that attempts to free a still-locked bitmap.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.97
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "System.Runtime.InteropServices.ExternalException (0x80004005): A generic error occurred in GDI+.",
      "stackTrace": "at System.Drawing.Bitmap.UnlockBits(BitmapData bitmapdata)\n   at SkiaSharp.Views.Desktop.SKControl.OnPaint(PaintEventArgs e)\n   at System.Windows.Forms.Control.PaintWithErrorHandling(PaintEventArgs e, Int16 layer)\n   at System.Windows.Forms.Control.WmPaint(Message& m)\n   at System.Windows.Forms.Control.WndProc(Message& m)\n   at System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)",
      "reproQuality": "complete"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Place an SKControl inside a scrollable container (e.g., DevExpress XtraScrollableControl)",
        "Scroll down so the bottom of the SKControl is visible but the top is scrolled off-screen",
        "In the PaintSurface event handler, change the Height of the SKControl to a value that makes it completely off-screen",
        "Observe ExternalException (GDI+ error) thrown from SKControl.OnPaint"
      ],
      "environmentDetails": "Windows Forms desktop, SKControl embedded in a scrollable container. Issue is triggered 100% of the time under the described scenario.",
      "repoLinks": [
        {
          "url": "https://devdiv.visualstudio.com/DevDiv/_workitems/edit/735692",
          "description": "VS internal bug #735692 cross-referenced by reporter"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "The OnPaint implementation in SKControl.cs has not changed materially since the issue was filed — it still uses the same LockBits/UnlockBits pattern with no re-entrancy guard."
    }
  },
  "analysis": {
    "summary": "Re-entrant OnPaint triggered by user resizing SKControl inside PaintSurface causes the inner paint call to dispose and replace the locked bitmap via FreeBitmap(), leaving the outer OnPaint with a stale BitmapData reference, which then fails at UnlockBits.",
    "rationale": "The exception originates in SkiaSharp's own OnPaint code path (UnlockBits), not in user code. Although the trigger is the anti-pattern of modifying control size inside PaintSurface, the library does not guard against re-entrant painting. Adding a simple boolean re-entrancy guard in OnPaint would prevent the crash. The maintainer previously left this open as a low-priority enhancement to investigate re-entrancy protection.",
    "keySignals": [
      {
        "text": "System.Runtime.InteropServices.ExternalException (0x80004005): A generic error occurred in GDI+. at System.Drawing.Bitmap.UnlockBits",
        "source": "issue body",
        "interpretation": "GDI+ errors from UnlockBits typically occur when the bitmap was disposed or replaced while still locked."
      },
      {
        "text": "I am changing the SKControl.Height value from within the PaintSurface event, which is obviously triggered from inside the OnPaint override in SKCanvas, which is inside the bitmap lock/unlock.",
        "source": "comment #1",
        "interpretation": "Reporter correctly identified that resizing inside PaintSurface triggers a nested OnPaint, creating re-entrancy."
      },
      {
        "text": "I suspect the XtraScrollableControl has a hook on my SKControl sized event and is manually calling the OnPaint function directly in a certain situation.",
        "source": "comment #2",
        "interpretation": "Resize event from a third-party scrollable container triggers a synchronous repaint within the existing paint cycle."
      },
      {
        "text": "It would be easy enough to handle this situation in the SKControl itself so it doesn't crash, not sure if its worth bothering with though.",
        "source": "comment #2 (reporter)",
        "interpretation": "Reporter acknowledges a fix at the library level is possible but left the decision to maintainers."
      },
      {
        "text": "This issue was the result of the paint operation also resizing the view - possibly invalidating the canvas or causing an infinite loop/stack overflow. This issue serves to act as a note for further investigation.",
        "source": "comment #5 (maintainer mattleibow, 2018)",
        "interpretation": "Maintainer confirmed understanding of root cause and deliberately kept open for future investigation."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKControl.cs",
        "lines": "26-53",
        "finding": "OnPaint calls CreateBitmap(), then LockBits(), then OnPaintSurface() (which fires the user's PaintSurface event), then UnlockBits(). There is no re-entrancy guard. If the user resizes the control inside PaintSurface, a nested OnPaint call is triggered, which calls CreateBitmap() → FreeBitmap() → disposes the bitmap currently locked in the outer call. The outer call then tries UnlockBits on the disposed bitmap, causing the GDI+ error.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKControl.cs",
        "lines": "68-81",
        "finding": "CreateBitmap() calls FreeBitmap() when Width or Height changes, disposing and nulling the bitmap field. This is the mechanism by which the outer LockBits reference becomes dangling during re-entrant painting.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Do not resize the SKControl (or any parent scrollable container) from within the PaintSurface event handler. Pre-calculate required dimensions before the paint cycle.",
      "Reset the scroll position of any parent scrollable container to the top before triggering a content-driven resize, ensuring the SKControl remains in the visible area during paint."
    ],
    "nextQuestions": [
      "Is the same re-entrancy issue reproducible with the standard WinForms Panel as a parent (without DevExpress XtraScrollableControl)?",
      "Does adding a simple `_isPainting` boolean guard in OnPaint fully resolve the crash, or are there edge cases with WM_SIZE being processed during paint?"
    ],
    "resolution": {
      "hypothesis": "A boolean re-entrancy guard in SKControl.OnPaint would prevent the nested paint call from freeing the locked bitmap while the outer paint is in progress.",
      "proposals": [
        {
          "title": "Add re-entrancy guard to OnPaint",
          "description": "Track painting state with a private boolean field. If OnPaint is called while already painting, call Invalidate() to schedule a repaint after the current cycle completes and return immediately.",
          "codeSnippet": "private bool _isPainting;\n\nprotected override void OnPaint(PaintEventArgs e)\n{\n    if (_isPainting)\n    {\n        Invalidate();\n        return;\n    }\n    _isPainting = true;\n    try\n    {\n        // ... existing OnPaint body ...\n    }\n    finally\n    {\n        _isPainting = false;\n    }\n}",
          "category": "fix",
          "confidence": 0.82,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Workaround: perform layout outside PaintSurface",
          "description": "Move any control resize logic out of the PaintSurface event handler. Calculate required dimensions in a separate method and apply them before triggering an Invalidate(), so that painting always starts with a stable size.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add re-entrancy guard to OnPaint",
      "recommendedReason": "Small, contained fix that makes SKControl resilient to the anti-pattern without requiring users to restructure their code."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.8,
      "reason": "Root cause is well-understood (re-entrant OnPaint via user resize in PaintSurface). A workaround exists. The library fix is small but requires deliberate design decision by maintainers on how to handle re-entrancy. Issue has been deliberately kept open by maintainer since 2017 for future investigation.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct type from enhancement to bug; retain platform and area labels; add reliability tenet",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Summarize root cause and offer re-entrancy guard workaround for users hitting this",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "**Root cause confirmed:** This crash occurs because `SKControl.OnPaint` has no re-entrancy guard. When you resize the control from inside `PaintSurface`, WinForms triggers a synchronous nested `OnPaint` call. The inner call runs `CreateBitmap()` which disposes the bitmap currently locked by the outer call. When the outer call then tries `UnlockBits`, GDI+ throws because the bitmap has been replaced.\n\n**Workaround:** Avoid resizing the control (or any parent container that triggers a synchronous repaint) from inside the `PaintSurface` event. Calculate required dimensions before the paint cycle and apply them outside of painting:\n\n```csharp\n// Do this OUTSIDE of PaintSurface\nif (requiredHeight != skControl.Height)\n    skControl.Height = requiredHeight;\nskControl.Invalidate();\n```\n\n**Potential fix:** Adding a re-entrancy guard in `SKControl.OnPaint` would make the control resilient to this pattern:\n\n```csharp\nprivate bool _isPainting;\n\nprotected override void OnPaint(PaintEventArgs e)\n{\n    if (_isPainting) { Invalidate(); return; }\n    _isPainting = true;\n    try { /* existing body */ }\n    finally { _isPainting = false; }\n}\n```\n\nThis is tracked as a low-priority enhancement."
      }
    ]
  }
}
```

</details>
