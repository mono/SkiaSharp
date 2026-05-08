# Issue Triage Report — #2299

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T14:59:16Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp.Views (0.92 (92%)) |
| Suggested action | ready-to-fix (0.87 (87%)) |

**Issue Summary:** On Android, SurfaceFactory.DrawSurface draws the SkiaSharp bitmap to the full canvas dimensions instead of the clip region, causing incorrect scaling/positioning when the canvas is a child canvas with an applied clip and translation (e.g. via DrawChild in a custom SKCanvasViewRenderer).

**Analysis:** SurfaceFactory.DrawSurface computes the destination RectF as (0, 0, canvas.Width, canvas.Height). When Android's system passes a canvas with a clip region and translation (as happens with DrawChild), canvas.Width and canvas.Height reflect the root/parent view dimensions, not the child view's visible area. The bitmap is therefore scaled to fill the entire parent canvas, producing wrong-output. The fix would be to use canvas.ClipBounds instead.

**Recommendations:** **ready-to-fix** — Root cause is clearly identified, confirmed in source code, fix is trivially small. Reporter volunteered to submit a PR.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Android |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create a custom SKCanvasViewRenderer on Android
2. Override DrawChild and pass the native canvas to DrawChild for a child SKCanvasView
3. Observe that the SkiaSharp content is scaled/positioned incorrectly because it is drawn to full parent canvas size

**Environment:** Android, SkiaSharp >= 2.88.0

**Repository links:**
- https://github.com/mono/SkiaSharp/discussions/2175 — Related discussion about odd scaling on Android blurred background effect

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Bitmap drawn to full canvas dimensions instead of clip region bounds when canvas has clip+translation applied |
| Repro quality | partial |
| Target frameworks | monoandroid |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SurfaceFactory.DrawSurface still uses canvas.Width/canvas.Height for the destination rect; no fix found in git history. |

## Analysis

### Technical Summary

SurfaceFactory.DrawSurface computes the destination RectF as (0, 0, canvas.Width, canvas.Height). When Android's system passes a canvas with a clip region and translation (as happens with DrawChild), canvas.Width and canvas.Height reflect the root/parent view dimensions, not the child view's visible area. The bitmap is therefore scaled to fill the entire parent canvas, producing wrong-output. The fix would be to use canvas.ClipBounds instead.

### Rationale

The reporter accurately identifies the root cause and fix. Code investigation confirms SurfaceFactory.DrawSurface uses canvas.Width/canvas.Height for the destination rect which is incorrect when the canvas has a clip region applied. The bug exists in current code with no fix in git history. Severity is medium because a workaround exists (custom renderer) and it only affects advanced usage patterns (custom DrawChild scenarios).

### Key Signals

- "the native canvas passed to the DrawChild method of a SKCanvasViewRenderer, is NOT the size of immediate view but the size of the view on which the original draw request was made. Instead, the canvas has a clip region and translation applied to it." — **issue body** (Root cause is correctly identified by reporter: destination rect should use clip bounds, not canvas.Width/Height.)
- "I think that this needs to be changed to draw to the height and width of the clipping region." — **issue body** (Reporter proposes correct fix direction — use canvas.ClipBounds for dst.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SurfaceFactory.cs` | 63-65 | direct | DrawSurface sets dst = new RectF(0, 0, canvas.Width, canvas.Height). When the canvas has clip+translation applied (e.g. via DrawChild), canvas.Width/Height are parent dimensions, not the child view bounds. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs` | 77-116 | related | OnDraw calls surfaceFactory.UpdateCanvasSize(Width, Height) via OnSizeChanged and then surfaceFactory.CreateSurface/DrawSurface with the view's own Width/Height. This is correct for normal rendering but the surfaceFactory bitmap is sized to the view, while DrawSurface destination uses the canvas dimensions which can differ when the canvas is re-used from a parent draw pass. |

### Workarounds

- Create a custom SKCanvasViewRenderer that overrides the drawing logic to use canvas.ClipBounds for the destination rect.

### Resolution Proposals

**Hypothesis:** Change SurfaceFactory.DrawSurface to use canvas.ClipBounds (obtained via canvas.GetClipBounds) as the destination RectF instead of (0, 0, canvas.Width, canvas.Height).

1. **Use canvas.ClipBounds for destination rect in DrawSurface** — fix, cost/xs, validated=untested
   - In SurfaceFactory.DrawSurface, replace the dst computation with the canvas clip bounds to correctly handle canvases with clip+translation applied.

```csharp
// Before:
var dst = new RectF(0, 0, canvas.Width, canvas.Height);
// After:
var clipBounds = new RectF();
canvas.GetClipBounds(clipBounds);
var dst = clipBounds;
```
2. **Create a custom SKCanvasViewRenderer** — workaround, cost/m, validated=untested
   - Override SKCanvasViewRenderer and intercept the DrawChild call to provide correct clip-aware drawing. The reporter has already done this successfully.

**Recommended proposal:** fix-1

**Why:** The fix is trivially small (one-line change) and clearly correct. The reporter identified the fix and volunteered to PR it.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.87 (87%) |
| Reason | Root cause is clearly identified, confirmed in source code, fix is trivially small. Reporter volunteered to submit a PR. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply type/bug, area/SkiaSharp.Views, os/Android, tenet/reliability | labels=type/bug, area/SkiaSharp.Views, os/Android, tenet/reliability |
| add-comment | medium | 0.87 (87%) | Acknowledge the bug, confirm root cause, invite PR from reporter | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed bug report and for tracing it to the root cause!

I can confirm that `SurfaceFactory.DrawSurface` uses `canvas.Width` / `canvas.Height` for the destination `RectF`, which is incorrect when the `Canvas` has a clip region and translation applied (as happens during `DrawChild`). The fix is to use `canvas.GetClipBounds` instead:

```csharp
// Replace:
var dst = new RectF(0, 0, canvas.Width, canvas.Height);
// With:
var clipBounds = new RectF();
canvas.GetClipBounds(clipBounds);
var dst = clipBounds;
```

If you'd like to submit a PR with this fix, that would be very welcome!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2299,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T14:59:16Z"
  },
  "summary": "On Android, SurfaceFactory.DrawSurface draws the SkiaSharp bitmap to the full canvas dimensions instead of the clip region, causing incorrect scaling/positioning when the canvas is a child canvas with an applied clip and translation (e.g. via DrawChild in a custom SKCanvasViewRenderer).",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.92
    },
    "platforms": [
      "os/Android"
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
      "errorMessage": "Bitmap drawn to full canvas dimensions instead of clip region bounds when canvas has clip+translation applied",
      "reproQuality": "partial",
      "targetFrameworks": [
        "monoandroid"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a custom SKCanvasViewRenderer on Android",
        "Override DrawChild and pass the native canvas to DrawChild for a child SKCanvasView",
        "Observe that the SkiaSharp content is scaled/positioned incorrectly because it is drawn to full parent canvas size"
      ],
      "environmentDetails": "Android, SkiaSharp >= 2.88.0",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/discussions/2175",
          "description": "Related discussion about odd scaling on Android blurred background effect"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "SurfaceFactory.DrawSurface still uses canvas.Width/canvas.Height for the destination rect; no fix found in git history."
    }
  },
  "analysis": {
    "summary": "SurfaceFactory.DrawSurface computes the destination RectF as (0, 0, canvas.Width, canvas.Height). When Android's system passes a canvas with a clip region and translation (as happens with DrawChild), canvas.Width and canvas.Height reflect the root/parent view dimensions, not the child view's visible area. The bitmap is therefore scaled to fill the entire parent canvas, producing wrong-output. The fix would be to use canvas.ClipBounds instead.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SurfaceFactory.cs",
        "lines": "63-65",
        "finding": "DrawSurface sets dst = new RectF(0, 0, canvas.Width, canvas.Height). When the canvas has clip+translation applied (e.g. via DrawChild), canvas.Width/Height are parent dimensions, not the child view bounds.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs",
        "lines": "77-116",
        "finding": "OnDraw calls surfaceFactory.UpdateCanvasSize(Width, Height) via OnSizeChanged and then surfaceFactory.CreateSurface/DrawSurface with the view's own Width/Height. This is correct for normal rendering but the surfaceFactory bitmap is sized to the view, while DrawSurface destination uses the canvas dimensions which can differ when the canvas is re-used from a parent draw pass.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "the native canvas passed to the DrawChild method of a SKCanvasViewRenderer, is NOT the size of immediate view but the size of the view on which the original draw request was made. Instead, the canvas has a clip region and translation applied to it.",
        "source": "issue body",
        "interpretation": "Root cause is correctly identified by reporter: destination rect should use clip bounds, not canvas.Width/Height."
      },
      {
        "text": "I think that this needs to be changed to draw to the height and width of the clipping region.",
        "source": "issue body",
        "interpretation": "Reporter proposes correct fix direction — use canvas.ClipBounds for dst."
      }
    ],
    "rationale": "The reporter accurately identifies the root cause and fix. Code investigation confirms SurfaceFactory.DrawSurface uses canvas.Width/canvas.Height for the destination rect which is incorrect when the canvas has a clip region applied. The bug exists in current code with no fix in git history. Severity is medium because a workaround exists (custom renderer) and it only affects advanced usage patterns (custom DrawChild scenarios).",
    "workarounds": [
      "Create a custom SKCanvasViewRenderer that overrides the drawing logic to use canvas.ClipBounds for the destination rect."
    ],
    "resolution": {
      "hypothesis": "Change SurfaceFactory.DrawSurface to use canvas.ClipBounds (obtained via canvas.GetClipBounds) as the destination RectF instead of (0, 0, canvas.Width, canvas.Height).",
      "proposals": [
        {
          "title": "Use canvas.ClipBounds for destination rect in DrawSurface",
          "description": "In SurfaceFactory.DrawSurface, replace the dst computation with the canvas clip bounds to correctly handle canvases with clip+translation applied.",
          "category": "fix",
          "effort": "cost/xs",
          "validated": "untested",
          "codeSnippet": "// Before:\nvar dst = new RectF(0, 0, canvas.Width, canvas.Height);\n// After:\nvar clipBounds = new RectF();\ncanvas.GetClipBounds(clipBounds);\nvar dst = clipBounds;"
        },
        {
          "title": "Create a custom SKCanvasViewRenderer",
          "description": "Override SKCanvasViewRenderer and intercept the DrawChild call to provide correct clip-aware drawing. The reporter has already done this successfully.",
          "category": "workaround",
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "fix-1",
      "recommendedReason": "The fix is trivially small (one-line change) and clearly correct. The reporter identified the fix and volunteered to PR it."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.87,
      "reason": "Root cause is clearly identified, confirmed in source code, fix is trivially small. Reporter volunteered to submit a PR.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views, os/Android, tenet/reliability",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Android",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the bug, confirm root cause, invite PR from reporter",
        "risk": "medium",
        "confidence": 0.87,
        "comment": "Thanks for the detailed bug report and for tracing it to the root cause!\n\nI can confirm that `SurfaceFactory.DrawSurface` uses `canvas.Width` / `canvas.Height` for the destination `RectF`, which is incorrect when the `Canvas` has a clip region and translation applied (as happens during `DrawChild`). The fix is to use `canvas.GetClipBounds` instead:\n\n```csharp\n// Replace:\nvar dst = new RectF(0, 0, canvas.Width, canvas.Height);\n// With:\nvar clipBounds = new RectF();\ncanvas.GetClipBounds(clipBounds);\nvar dst = clipBounds;\n```\n\nIf you'd like to submit a PR with this fix, that would be very welcome!"
      }
    ]
  }
}
```

</details>
