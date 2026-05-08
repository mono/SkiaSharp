# Issue Triage Report — #1251

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T16:18:09Z |
| Type | type/enhancement (0.92 (92%)) |
| Area | area/SkiaSharp.Views (0.95 (95%)) |
| Suggested action | keep-open (0.85 (85%)) |

**Issue Summary:** SKSwapChainPanel.OnRenderFrame allocates SKAutoCanvasRestore and SKPaintGLSurfaceEventArgs on each frame, causing GC pressure in the render hot path; reporter requests a zero-allocation rendering mode.

**Analysis:** SKSwapChainPanel.OnRenderFrame still makes two heap allocations per frame: SKAutoCanvasRestore on line 89 and SKPaintGLSurfaceEventArgs on line 92. The maintainer acknowledged both issues in comments. SKAutoCanvasRestore can be trivially replaced with canvas.Save()/RestoreToCount(). Reusing SKPaintGLSurfaceEventArgs was deemed a breaking change but an opt-in zero-allocation mode was proposed.

**Recommendations:** **keep-open** — Valid performance enhancement with at least one trivial fix (SKAutoCanvasRestore replacement). The args reuse requires design discussion. Both allocations confirmed in current code.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-WinUI, os/Windows-Universal-UWP |
| Backends | backend/OpenGL |
| Tenets | tenet/performance |
| Partner | — |
| Current labels | type/enhancement, area/SkiaSharp.Views, backend/OpenGL |

## Evidence

### Reproduction

1. Use SKSwapChainPanel on UWP/WinUI with a PaintSurface handler
2. Profile the render loop with a memory profiler
3. Observe two heap allocations per frame: SKAutoCanvasRestore and SKPaintGLSurfaceEventArgs

**Environment:** UWP / WinUI SwapChainPanel with OpenGL (ANGLE) backend

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Both allocations are still present in the current WinUI SKSwapChainPanel.cs (lines 89 and 92). The SKCanvas property allocation was addressed by caching canvas at line 86. |

## Analysis

### Technical Summary

SKSwapChainPanel.OnRenderFrame still makes two heap allocations per frame: SKAutoCanvasRestore on line 89 and SKPaintGLSurfaceEventArgs on line 92. The maintainer acknowledged both issues in comments. SKAutoCanvasRestore can be trivially replaced with canvas.Save()/RestoreToCount(). Reusing SKPaintGLSurfaceEventArgs was deemed a breaking change but an opt-in zero-allocation mode was proposed.

### Rationale

Confirmed enhancement: both allocations exist in current code (verified in source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKSwapChainPanel.cs). The SKAutoCanvasRestore fix is trivial and safe. The args reuse requires API design to avoid breaking users who store the EventArgs reference. This is a valid performance enhancement worth keeping open.

### Key Signals

- "SkSwapChainPanel::OnRenderFrame(Rect) makes 2 heap allocations" — **issue body** (Confirmed allocation in hot path, still present in current code.)
- "Can SKPaintGLSurfaceEventArgs be reused rather than allocated each frame?" — **issue body** (Reporter requests object reuse to reduce GC pressure.)
- "We can't reuse the args as this is a breaking change" — **comment by mattleibow (2020-04-27)** (Maintainer acknowledged risk; opt-in property was suggested as a design.)
- "We can totally replace the SKAutoCanvasRestore with a canvas.RestoreToCount(0); after the draw" — **comment by mattleibow (2020-04-27)** (Simple fix acknowledged by maintainer but not yet implemented.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKSwapChainPanel.cs` | 89-93 | direct | using (new SKAutoCanvasRestore(canvas, true)) and new SKPaintGLSurfaceEventArgs(...) are still allocated per frame. Canvas is correctly cached at line 86, but the two call-site allocations remain unaddressed. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Shared/SKPaintGLSurfaceEventArgs.cs` | 27-70 | direct | SKPaintGLSurfaceEventArgs is a class (heap-allocated reference type). It has no reset or reuse API. Converting to a struct or adding opt-in reuse would be API/behavioral changes. |

### Workarounds

- Cache SKPaintGLSurfaceEventArgs.Surface.Canvas locally in the PaintSurface handler instead of accessing it via the property each call to avoid any additional re-allocations.

### Next Questions

- Should the fix be applied to the Uno platform SKSwapChainPanel as well?
- Is an opt-in zero-allocation mode (e.g., a bool property) acceptable as a non-breaking approach for args reuse?
- Could SKAutoCanvasRestore be changed to a ref struct to allow stack allocation?

### Resolution Proposals

**Hypothesis:** Replace SKAutoCanvasRestore with manual save/restore and add an opt-in cached-args mode to achieve zero-allocation rendering per frame.

1. **Replace SKAutoCanvasRestore with manual save/restore** — fix, confidence 0.92 (92%), cost/xs, validated=untested
   - Replace `using (new SKAutoCanvasRestore(canvas, true))` with an explicit save count and `canvas.RestoreToCount(saveCount)` to eliminate one heap allocation per frame.
2. **Add opt-in zero-allocation rendering mode** — fix, confidence 0.75 (75%), cost/s, validated=untested
   - Add a boolean property (e.g., EnableZeroAllocationRendering) that reuses a cached SKPaintGLSurfaceEventArgs instance instead of allocating a new one each frame. When disabled (default), behavior is unchanged for backward compatibility.

**Recommended proposal:** Replace SKAutoCanvasRestore with manual save/restore

**Why:** Low-risk, trivial change that the maintainer already explicitly suggested. No breaking changes and provides immediate GC pressure reduction.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.85 (85%) |
| Reason | Valid performance enhancement with at least one trivial fix (SKAutoCanvasRestore replacement). The args reuse requires design discussion. Both allocations confirmed in current code. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Add performance tenet and WinUI/UWP platform labels to the existing correct type/area/backend labels | labels=type/enhancement, area/SkiaSharp.Views, backend/OpenGL, tenet/performance, os/Windows-WinUI, os/Windows-Universal-UWP |
| add-comment | medium | 0.85 (85%) | Acknowledge the still-open allocations and outline the fix path | — |

**Comment draft for `add-comment`:**

```markdown
Both allocations are still present in the current codebase:

1. **`SKAutoCanvasRestore`** — can be replaced with a manual `int saveCount = canvas.Save(); ... canvas.RestoreToCount(saveCount);` pattern, eliminating one heap allocation per frame with no API change.
2. **`SKPaintGLSurfaceEventArgs`** — reuse requires an opt-in API to avoid breaking consumers that store event args references. A cached instance behind an opt-in property is the proposed approach.

Contributions welcome — the `SKAutoCanvasRestore` change in particular is a low-risk, small PR.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1251,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T16:18:09Z",
    "currentLabels": [
      "type/enhancement",
      "area/SkiaSharp.Views",
      "backend/OpenGL"
    ]
  },
  "summary": "SKSwapChainPanel.OnRenderFrame allocates SKAutoCanvasRestore and SKPaintGLSurfaceEventArgs on each frame, causing GC pressure in the render hot path; reporter requests a zero-allocation rendering mode.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-WinUI",
      "os/Windows-Universal-UWP"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Use SKSwapChainPanel on UWP/WinUI with a PaintSurface handler",
        "Profile the render loop with a memory profiler",
        "Observe two heap allocations per frame: SKAutoCanvasRestore and SKPaintGLSurfaceEventArgs"
      ],
      "environmentDetails": "UWP / WinUI SwapChainPanel with OpenGL (ANGLE) backend"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Both allocations are still present in the current WinUI SKSwapChainPanel.cs (lines 89 and 92). The SKCanvas property allocation was addressed by caching canvas at line 86."
    }
  },
  "analysis": {
    "summary": "SKSwapChainPanel.OnRenderFrame still makes two heap allocations per frame: SKAutoCanvasRestore on line 89 and SKPaintGLSurfaceEventArgs on line 92. The maintainer acknowledged both issues in comments. SKAutoCanvasRestore can be trivially replaced with canvas.Save()/RestoreToCount(). Reusing SKPaintGLSurfaceEventArgs was deemed a breaking change but an opt-in zero-allocation mode was proposed.",
    "rationale": "Confirmed enhancement: both allocations exist in current code (verified in source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKSwapChainPanel.cs). The SKAutoCanvasRestore fix is trivial and safe. The args reuse requires API design to avoid breaking users who store the EventArgs reference. This is a valid performance enhancement worth keeping open.",
    "keySignals": [
      {
        "text": "SkSwapChainPanel::OnRenderFrame(Rect) makes 2 heap allocations",
        "source": "issue body",
        "interpretation": "Confirmed allocation in hot path, still present in current code."
      },
      {
        "text": "Can SKPaintGLSurfaceEventArgs be reused rather than allocated each frame?",
        "source": "issue body",
        "interpretation": "Reporter requests object reuse to reduce GC pressure."
      },
      {
        "text": "We can't reuse the args as this is a breaking change",
        "source": "comment by mattleibow (2020-04-27)",
        "interpretation": "Maintainer acknowledged risk; opt-in property was suggested as a design."
      },
      {
        "text": "We can totally replace the SKAutoCanvasRestore with a canvas.RestoreToCount(0); after the draw",
        "source": "comment by mattleibow (2020-04-27)",
        "interpretation": "Simple fix acknowledged by maintainer but not yet implemented."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKSwapChainPanel.cs",
        "lines": "89-93",
        "finding": "using (new SKAutoCanvasRestore(canvas, true)) and new SKPaintGLSurfaceEventArgs(...) are still allocated per frame. Canvas is correctly cached at line 86, but the two call-site allocations remain unaddressed.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Shared/SKPaintGLSurfaceEventArgs.cs",
        "lines": "27-70",
        "finding": "SKPaintGLSurfaceEventArgs is a class (heap-allocated reference type). It has no reset or reuse API. Converting to a struct or adding opt-in reuse would be API/behavioral changes.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Cache SKPaintGLSurfaceEventArgs.Surface.Canvas locally in the PaintSurface handler instead of accessing it via the property each call to avoid any additional re-allocations."
    ],
    "nextQuestions": [
      "Should the fix be applied to the Uno platform SKSwapChainPanel as well?",
      "Is an opt-in zero-allocation mode (e.g., a bool property) acceptable as a non-breaking approach for args reuse?",
      "Could SKAutoCanvasRestore be changed to a ref struct to allow stack allocation?"
    ],
    "resolution": {
      "hypothesis": "Replace SKAutoCanvasRestore with manual save/restore and add an opt-in cached-args mode to achieve zero-allocation rendering per frame.",
      "proposals": [
        {
          "title": "Replace SKAutoCanvasRestore with manual save/restore",
          "description": "Replace `using (new SKAutoCanvasRestore(canvas, true))` with an explicit save count and `canvas.RestoreToCount(saveCount)` to eliminate one heap allocation per frame.",
          "category": "fix",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Add opt-in zero-allocation rendering mode",
          "description": "Add a boolean property (e.g., EnableZeroAllocationRendering) that reuses a cached SKPaintGLSurfaceEventArgs instance instead of allocating a new one each frame. When disabled (default), behavior is unchanged for backward compatibility.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Replace SKAutoCanvasRestore with manual save/restore",
      "recommendedReason": "Low-risk, trivial change that the maintainer already explicitly suggested. No breaking changes and provides immediate GC pressure reduction."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.85,
      "reason": "Valid performance enhancement with at least one trivial fix (SKAutoCanvasRestore replacement). The args reuse requires design discussion. Both allocations confirmed in current code.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Add performance tenet and WinUI/UWP platform labels to the existing correct type/area/backend labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp.Views",
          "backend/OpenGL",
          "tenet/performance",
          "os/Windows-WinUI",
          "os/Windows-Universal-UWP"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the still-open allocations and outline the fix path",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Both allocations are still present in the current codebase:\n\n1. **`SKAutoCanvasRestore`** — can be replaced with a manual `int saveCount = canvas.Save(); ... canvas.RestoreToCount(saveCount);` pattern, eliminating one heap allocation per frame with no API change.\n2. **`SKPaintGLSurfaceEventArgs`** — reuse requires an opt-in API to avoid breaking consumers that store event args references. A cached instance behind an opt-in property is the proposed approach.\n\nContributions welcome — the `SKAutoCanvasRestore` change in particular is a low-risk, small PR."
      }
    ]
  }
}
```

</details>
