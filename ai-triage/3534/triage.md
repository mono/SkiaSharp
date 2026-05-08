# Issue Triage Report — #3534

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-05T19:53:38Z |
| Type | type/enhancement (0.97 (97%)) |
| Area | area/SkiaSharp.Views.Blazor (0.98 (98%)) |
| Suggested action | ready-to-fix (0.93 (93%)) |

**Issue Summary:** Add scroll wheel event support (v120-normalized WheelChanged) to SKCanvasView and SKGLView in SkiaSharp.Views.Blazor WASM.

**Analysis:** SKHtmlCanvas.ts/js currently has no wheel event listener. The enhancement adds a wheel event handler to Blazor WASM canvas views normalizing browser deltaY/deltaMode values to the v120 standard (120 per discrete notch). An implementation exists on branch copilot/add-touch-event-blazor-canvas with 24/24 Playwright tests passing across Chromium, Firefox, and WebKit.

**Recommendations:** **ready-to-fix** — Well-scoped enhancement with a complete, test-validated implementation on branch copilot/add-touch-event-blazor-canvas. Code investigation confirms no current wheel support in SKHtmlCanvas.ts. PR can be opened and merged.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.Views.Blazor |
| Platforms | os/WASM |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3533 — Parent issue: v120 wheel normalization standard

**Code snippets:**

```csharp
Normalization logic (TypeScript): const WHEEL_DELTA = 120; const PIXELS_PER_NOTCH = 100; const LINES_PER_NOTCH = 3; if deltaMode==LINE: delta=round(-deltaY*(120/3)); elif deltaMode==PAGE: delta=round(-deltaY*120); else: delta=round(-deltaY*(120/100))
```

## Analysis

### Technical Summary

SKHtmlCanvas.ts/js currently has no wheel event listener. The enhancement adds a wheel event handler to Blazor WASM canvas views normalizing browser deltaY/deltaMode values to the v120 standard (120 per discrete notch). An implementation exists on branch copilot/add-touch-event-blazor-canvas with 24/24 Playwright tests passing across Chromium, Firefox, and WebKit.

### Rationale

Classified as type/enhancement because the Blazor view infrastructure exists but lacks wheel event support. Area is area/SkiaSharp.Views.Blazor (more specific than the currently-applied area/SkiaSharp). Platform os/WASM applies as this is a browser-only feature. The implementation is done on a branch with full Playwright test coverage; action is ready-to-fix with high confidence.

### Key Signals

- "The v120 normalization has been implemented on branch copilot/add-touch-event-blazor-canvas (commit 052d325bc). Validated with Playwright across 3 browser engines: Chromium 24/24, Firefox 24/24, WebKit 24/24." — **issue body** (Implementation already complete and test-validated on a feature branch; PR not yet opened.)
- "Must handle both NET7+ ([JSImport]/[JSExport]) and pre-NET7 (DotNetObjectReference.invokeMethod) interop paths" — **issue body** (Cross-version compatibility requirement adds implementation complexity.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/wwwroot/SKHtmlCanvas.ts` | — | direct | No wheel event listener or WheelEvent handling present. Current event handling is limited to render loop and image data. No touch/pointer event infrastructure exists in this file. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKCanvasView.razor.cs` | — | direct | No OnTouch, SKTouchAction, or wheel-related parameters or methods found. The view exposes only OnPaintSurface and EnableRenderLoop parameters currently. |

### Resolution Proposals

**Hypothesis:** Add wheel event listener to SKHtmlCanvas.ts/.js, normalize deltaY/deltaMode to v120 integer, invoke C# callback via JSImport/JSExport (NET7+) and DotNetObjectReference (pre-NET7), call preventDefault when handled.

1. **Merge branch copilot/add-touch-event-blazor-canvas** — fix, cost/s, validated=untested
   - Open a PR from the existing implementation branch. The normalization logic, interop paths, and Playwright tests are already in place (24/24 passing). Review and merge.

**Recommended proposal:** p1

**Why:** Implementation is complete with tests passing. Opening a PR to merge is the minimal remaining step.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.93 (93%) |
| Reason | Well-scoped enhancement with a complete, test-validated implementation on branch copilot/add-touch-event-blazor-canvas. Code investigation confirms no current wheel support in SKHtmlCanvas.ts. PR can be opened and merged. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Correct area label from area/SkiaSharp to area/SkiaSharp.Views.Blazor; add os/WASM and tenet/compatibility | labels=type/enhancement, area/SkiaSharp.Views.Blazor, os/WASM, tenet/compatibility |
| add-comment | medium | 0.90 (90%) | Acknowledge the well-specified enhancement; note the branch has full test coverage; recommend opening a PR | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed spec and implementation! 🎯

Code investigation confirms that `SKHtmlCanvas.ts` currently has no wheel event handling, so this is a genuine gap. The normalization approach (v120 standard, per-mode conversion) is well-researched and the Playwright coverage across Chromium / Firefox / WebKit is excellent.

Next step: open a PR from `copilot/add-touch-event-blazor-canvas` so the implementation can be reviewed and merged.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3534,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-05T19:53:38Z"
  },
  "summary": "Add scroll wheel event support (v120-normalized WheelChanged) to SKCanvasView and SKGLView in SkiaSharp.Views.Blazor WASM.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views.Blazor",
      "confidence": 0.98
    },
    "platforms": [
      "os/WASM"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3533",
          "description": "Parent issue: v120 wheel normalization standard"
        }
      ],
      "codeSnippets": [
        "Normalization logic (TypeScript): const WHEEL_DELTA = 120; const PIXELS_PER_NOTCH = 100; const LINES_PER_NOTCH = 3; if deltaMode==LINE: delta=round(-deltaY*(120/3)); elif deltaMode==PAGE: delta=round(-deltaY*120); else: delta=round(-deltaY*(120/100))"
      ]
    }
  },
  "analysis": {
    "summary": "SKHtmlCanvas.ts/js currently has no wheel event listener. The enhancement adds a wheel event handler to Blazor WASM canvas views normalizing browser deltaY/deltaMode values to the v120 standard (120 per discrete notch). An implementation exists on branch copilot/add-touch-event-blazor-canvas with 24/24 Playwright tests passing across Chromium, Firefox, and WebKit.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/wwwroot/SKHtmlCanvas.ts",
        "finding": "No wheel event listener or WheelEvent handling present. Current event handling is limited to render loop and image data. No touch/pointer event infrastructure exists in this file.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKCanvasView.razor.cs",
        "finding": "No OnTouch, SKTouchAction, or wheel-related parameters or methods found. The view exposes only OnPaintSurface and EnableRenderLoop parameters currently.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "The v120 normalization has been implemented on branch copilot/add-touch-event-blazor-canvas (commit 052d325bc). Validated with Playwright across 3 browser engines: Chromium 24/24, Firefox 24/24, WebKit 24/24.",
        "source": "issue body",
        "interpretation": "Implementation already complete and test-validated on a feature branch; PR not yet opened."
      },
      {
        "text": "Must handle both NET7+ ([JSImport]/[JSExport]) and pre-NET7 (DotNetObjectReference.invokeMethod) interop paths",
        "source": "issue body",
        "interpretation": "Cross-version compatibility requirement adds implementation complexity."
      }
    ],
    "rationale": "Classified as type/enhancement because the Blazor view infrastructure exists but lacks wheel event support. Area is area/SkiaSharp.Views.Blazor (more specific than the currently-applied area/SkiaSharp). Platform os/WASM applies as this is a browser-only feature. The implementation is done on a branch with full Playwright test coverage; action is ready-to-fix with high confidence.",
    "resolution": {
      "hypothesis": "Add wheel event listener to SKHtmlCanvas.ts/.js, normalize deltaY/deltaMode to v120 integer, invoke C# callback via JSImport/JSExport (NET7+) and DotNetObjectReference (pre-NET7), call preventDefault when handled.",
      "proposals": [
        {
          "title": "Merge branch copilot/add-touch-event-blazor-canvas",
          "description": "Open a PR from the existing implementation branch. The normalization logic, interop paths, and Playwright tests are already in place (24/24 passing). Review and merge.",
          "category": "fix",
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "p1",
      "recommendedReason": "Implementation is complete with tests passing. Opening a PR to merge is the minimal remaining step."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.93,
      "reason": "Well-scoped enhancement with a complete, test-validated implementation on branch copilot/add-touch-event-blazor-canvas. Code investigation confirms no current wheel support in SKHtmlCanvas.ts. PR can be opened and merged.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct area label from area/SkiaSharp to area/SkiaSharp.Views.Blazor; add os/WASM and tenet/compatibility",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp.Views.Blazor",
          "os/WASM",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the well-specified enhancement; note the branch has full test coverage; recommend opening a PR",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the detailed spec and implementation! 🎯\n\nCode investigation confirms that `SKHtmlCanvas.ts` currently has no wheel event handling, so this is a genuine gap. The normalization approach (v120 standard, per-mode conversion) is well-researched and the Playwright coverage across Chromium / Firefox / WebKit is excellent.\n\nNext step: open a PR from `copilot/add-touch-event-blazor-canvas` so the implementation can be reviewed and merged."
      }
    ]
  }
}
```

</details>
