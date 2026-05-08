# Issue Triage Report — #2295

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T16:48:00Z |
| Type | type/question (0.82 (82%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.75 (75%)) |

**Issue Summary:** Reporter asks how to prevent SKXamlCanvas from appearing to stretch and shrink when a storyboard animation rapidly changes the width of a sibling grid element in a WinUI application.

**Analysis:** The SKXamlCanvas dispatches redraws asynchronously via DispatcherQueue in OnSizeChanged. During a rapid storyboard animation, layout passes and new sizes arrive faster than the asynchronous DoInvalidate executes, so the old WriteableBitmap is temporarily shown at the wrong canvas size, creating the stretching illusion.

**Recommendations:** **close-as-not-a-bug** — The stretching effect is a consequence of the async-dispatch architecture of SKXamlCanvas — DoInvalidate runs after the layout pass completes. This is an existing known limitation discussed in #2341. A workaround exists (avoid resizing the canvas during animation). The issue does not represent broken or incorrect behavior relative to the current implementation contract.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-WinUI |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Create a WinUI app with a Grid containing SKXamlCanvas and a Border whose width is animated via a DoubleAnimation that AutoReverses
2. Run the animation loop at 100ms duration so the available size of the SKXamlCanvas changes rapidly
3. Observe that the canvas content stretches and shrinks as size changes

**Environment:** WinUI (Windows) — SkiaSharp.Views.Windows.SKXamlCanvas.PaintSurface

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2341 — Related WinUI scale delay bug caused by the same async Invalidate path

**Attachments:**
- SkiaStretchIssue.zip — https://github.com/mono/SkiaSharp/files/9867051/SkiaStretchIssue.zip — Minimal WinUI repro project demonstrating canvas stretch during storyboard animation

## Analysis

### Technical Summary

The SKXamlCanvas dispatches redraws asynchronously via DispatcherQueue in OnSizeChanged. During a rapid storyboard animation, layout passes and new sizes arrive faster than the asynchronous DoInvalidate executes, so the old WriteableBitmap is temporarily shown at the wrong canvas size, creating the stretching illusion.

### Rationale

The title is explicitly tagged [QUESTION] and the body ends with a direct question asking for a prevention approach. The underlying cause is the async dispatch in Invalidate() — a known design characteristic of SKXamlCanvas that also manifests in issue #2341. The behavior is by-design for the current architecture but can be confusing. Classified as type/question because no crash or data loss is reported and the user wants usage guidance.

### Key Signals

- "Is there anyway I can prevent this hapenning?" — **issue body** (User is asking a usage question, not reporting a crash — wants to know how to avoid the visual artifact.)
- "SkiaStretchIssue.zip" — **issue body** (Repro project provided, confirming the scenario is reproducible.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 120-168 | direct | OnSizeChanged calls Invalidate() which enqueues DoInvalidate via DispatcherQueue.TryEnqueue(Normal). This async dispatch means the bitmap is not updated synchronously with the layout pass — the old bitmap remains as Background during the gap. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 229-269 | direct | CreateBitmap allocates a WriteableBitmap at the new pixel size and sets it as Background with Stretch=None and a ScaleTransform(1/Dpi). When canvas size changes but bitmap has not yet been recreated, the old bitmap (sized for the previous layout pass) still fills the Background at its old scale, appearing to stretch or leave gaps. |

### Workarounds

- Avoid animating the available size of the SKXamlCanvas at high frequency. Instead, animate a sibling element that does not force the canvas to resize (e.g. use a fixed-width column for the canvas).
- Call canvas.Invalidate() explicitly in the storyboard Completed or Tick handler to force a fresh redraw after the animation settles.
- Track the related issue #2341 for a fix that calls DoInvalidate directly from OnSizeChanged instead of the async Invalidate(), which would eliminate the visual gap.

### Resolution Proposals

**Hypothesis:** The async DispatcherQueue dispatch in Invalidate() creates a gap between layout size change and bitmap redraw; calling DoInvalidate() directly from OnSizeChanged (as suggested in #2341) would fix the visual artifact at the framework level.

1. **Avoid resizing SKXamlCanvas at animation speed** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - Place the SKXamlCanvas in a fixed-width Grid column (e.g. '*' star-sized but isolated) so the storyboard only affects the sibling column without forcing the canvas to relayout on every animation frame.
2. **Track fix in related issue #2341** — investigation, confidence 0.85 (85%), cost/s, validated=untested
   - Issue #2341 identifies that calling DoInvalidate directly in OnSizeChanged instead of the async Invalidate() eliminates the rendering gap during resize. Following that issue for a framework-level fix is the recommended long-term solution.

**Recommended proposal:** Avoid resizing SKXamlCanvas at animation speed

**Why:** Immediately actionable workaround with no code changes needed in SkiaSharp itself.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.75 (75%) |
| Reason | The stretching effect is a consequence of the async-dispatch architecture of SKXamlCanvas — DoInvalidate runs after the layout pass completes. This is an existing known limitation discussed in #2341. A workaround exists (avoid resizing the canvas during animation). The issue does not represent broken or incorrect behavior relative to the current implementation contract. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply question, WinUI, and views labels | labels=type/question, area/SkiaSharp.Views, os/Windows-WinUI |
| add-comment | high | 0.75 (75%) | Post explanation of async dispatch cause and workarounds | — |
| link-related | low | 0.90 (90%) | Cross-reference related WinUI scale/resize issue | linkedIssue=#2341 |
| close-issue | medium | 0.75 (75%) | Close as not a bug — behavior is by design, workaround available | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed repro! The stretching you see is caused by how `SKXamlCanvas` redraws itself: when `SizeChanged` fires, it enqueues a `DoInvalidate` call on the `DispatcherQueue`. During a rapid storyboard animation, new layout sizes arrive faster than the async redraw can execute, so the old `WriteableBitmap` is temporarily shown at the new canvas dimensions — giving the stretching appearance.

This is a known limitation also described in #2341.

**Workarounds:**
1. **Isolate the canvas from the animation** — Put the `SKXamlCanvas` in a `Grid` column with a fixed or star (`*`) width that is not affected by the animated column, so rapid size changes do not force the canvas to relayout.
2. **Call `Invalidate()` after the animation ends** — Hook the storyboard's `Completed` event (or add a `Tick` from a slower `DispatcherTimer`) to force a clean redraw once the animation settles.

A framework-level fix (calling `DoInvalidate` synchronously from `OnSizeChanged`) is being discussed in #2341.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2295,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T16:48:00Z"
  },
  "summary": "Reporter asks how to prevent SKXamlCanvas from appearing to stretch and shrink when a storyboard animation rapidly changes the width of a sibling grid element in a WinUI application.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.82
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-WinUI"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a WinUI app with a Grid containing SKXamlCanvas and a Border whose width is animated via a DoubleAnimation that AutoReverses",
        "Run the animation loop at 100ms duration so the available size of the SKXamlCanvas changes rapidly",
        "Observe that the canvas content stretches and shrinks as size changes"
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2341",
          "description": "Related WinUI scale delay bug caused by the same async Invalidate path"
        }
      ],
      "environmentDetails": "WinUI (Windows) — SkiaSharp.Views.Windows.SKXamlCanvas.PaintSurface",
      "attachments": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/9867051/SkiaStretchIssue.zip",
          "filename": "SkiaStretchIssue.zip",
          "description": "Minimal WinUI repro project demonstrating canvas stretch during storyboard animation"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The SKXamlCanvas dispatches redraws asynchronously via DispatcherQueue in OnSizeChanged. During a rapid storyboard animation, layout passes and new sizes arrive faster than the asynchronous DoInvalidate executes, so the old WriteableBitmap is temporarily shown at the wrong canvas size, creating the stretching illusion.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "120-168",
        "finding": "OnSizeChanged calls Invalidate() which enqueues DoInvalidate via DispatcherQueue.TryEnqueue(Normal). This async dispatch means the bitmap is not updated synchronously with the layout pass — the old bitmap remains as Background during the gap.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "229-269",
        "finding": "CreateBitmap allocates a WriteableBitmap at the new pixel size and sets it as Background with Stretch=None and a ScaleTransform(1/Dpi). When canvas size changes but bitmap has not yet been recreated, the old bitmap (sized for the previous layout pass) still fills the Background at its old scale, appearing to stretch or leave gaps.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Is there anyway I can prevent this hapenning?",
        "source": "issue body",
        "interpretation": "User is asking a usage question, not reporting a crash — wants to know how to avoid the visual artifact."
      },
      {
        "text": "SkiaStretchIssue.zip",
        "source": "issue body",
        "interpretation": "Repro project provided, confirming the scenario is reproducible."
      }
    ],
    "rationale": "The title is explicitly tagged [QUESTION] and the body ends with a direct question asking for a prevention approach. The underlying cause is the async dispatch in Invalidate() — a known design characteristic of SKXamlCanvas that also manifests in issue #2341. The behavior is by-design for the current architecture but can be confusing. Classified as type/question because no crash or data loss is reported and the user wants usage guidance.",
    "workarounds": [
      "Avoid animating the available size of the SKXamlCanvas at high frequency. Instead, animate a sibling element that does not force the canvas to resize (e.g. use a fixed-width column for the canvas).",
      "Call canvas.Invalidate() explicitly in the storyboard Completed or Tick handler to force a fresh redraw after the animation settles.",
      "Track the related issue #2341 for a fix that calls DoInvalidate directly from OnSizeChanged instead of the async Invalidate(), which would eliminate the visual gap."
    ],
    "resolution": {
      "hypothesis": "The async DispatcherQueue dispatch in Invalidate() creates a gap between layout size change and bitmap redraw; calling DoInvalidate() directly from OnSizeChanged (as suggested in #2341) would fix the visual artifact at the framework level.",
      "proposals": [
        {
          "title": "Avoid resizing SKXamlCanvas at animation speed",
          "description": "Place the SKXamlCanvas in a fixed-width Grid column (e.g. '*' star-sized but isolated) so the storyboard only affects the sibling column without forcing the canvas to relayout on every animation frame.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Track fix in related issue #2341",
          "description": "Issue #2341 identifies that calling DoInvalidate directly in OnSizeChanged instead of the async Invalidate() eliminates the rendering gap during resize. Following that issue for a framework-level fix is the recommended long-term solution.",
          "category": "investigation",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Avoid resizing SKXamlCanvas at animation speed",
      "recommendedReason": "Immediately actionable workaround with no code changes needed in SkiaSharp itself."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.75,
      "reason": "The stretching effect is a consequence of the async-dispatch architecture of SKXamlCanvas — DoInvalidate runs after the layout pass completes. This is an existing known limitation discussed in #2341. A workaround exists (avoid resizing the canvas during animation). The issue does not represent broken or incorrect behavior relative to the current implementation contract.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question, WinUI, and views labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp.Views",
          "os/Windows-WinUI"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post explanation of async dispatch cause and workarounds",
        "risk": "high",
        "confidence": 0.75,
        "comment": "Thanks for the detailed repro! The stretching you see is caused by how `SKXamlCanvas` redraws itself: when `SizeChanged` fires, it enqueues a `DoInvalidate` call on the `DispatcherQueue`. During a rapid storyboard animation, new layout sizes arrive faster than the async redraw can execute, so the old `WriteableBitmap` is temporarily shown at the new canvas dimensions — giving the stretching appearance.\n\nThis is a known limitation also described in #2341.\n\n**Workarounds:**\n1. **Isolate the canvas from the animation** — Put the `SKXamlCanvas` in a `Grid` column with a fixed or star (`*`) width that is not affected by the animated column, so rapid size changes do not force the canvas to relayout.\n2. **Call `Invalidate()` after the animation ends** — Hook the storyboard's `Completed` event (or add a `Tick` from a slower `DispatcherTimer`) to force a clean redraw once the animation settles.\n\nA framework-level fix (calling `DoInvalidate` synchronously from `OnSizeChanged`) is being discussed in #2341."
      },
      {
        "type": "link-related",
        "description": "Cross-reference related WinUI scale/resize issue",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 2341
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — behavior is by design, workaround available",
        "risk": "medium",
        "confidence": 0.75,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
