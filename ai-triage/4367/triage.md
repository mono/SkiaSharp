# Issue Triage Report — #4367

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-07-30T05:12:56Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp.Views.Blazor (0.99 (99%)) |
| Suggested action | keep-open (0.92 (92%)) |

**Issue Summary:** Feature request to add partial (damage-based) frame transfer to the Blazor Server/Hybrid bridged rendering path and track additional follow-ups from PR #4363, including a Blazor Hybrid sample, full test web-app pages, native-asset pack verification, static-SSR poster frame, shared presenter refactor, and API XML docs.

**Analysis:** The issue bundles several follow-up work items for the Blazor Server/Hybrid bridged rendering path introduced by draft PR #4363: (1) the primary enhancement — partial/damage-based frame transfer using RFB-style pixel-buffer diffing to send only the changed bounding-rect over SignalR instead of a full frame; (2) a Blazor Hybrid BlazorWebView sample plus UITesting page; (3) full test web-app pages (Static/Server/Auto) + CI lane; (4) native-asset packaging pack-verification; (5) static-SSR poster frame; (6) folding the WASM SKHtmlCanvas paint onto the shared SKCanvasPresenter; (7) API XML docs for the new surface. The damage-based transfer is well-designed: keep a previous RGBA buffer, scan row/col min–max to get a bounding rect, encode only the sub-rect, blit with putImageData/texSubImage2D, suppress if ≥65% changed, opt-in via SKBlazorOptions.DifferentialTransfer.

**Recommendations:** **keep-open** — Valid, well-scoped feature request with clear design. Depends on draft PR #4363 landing first. No information is missing; issue should remain open as a tracking issue for follow-up enhancements.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp.Views.Blazor |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Perf | — |
| Partner | — |
| Current labels | type/feature-request, area/SkiaSharp.Views.Blazor |

## Evidence

### Reproduction

**Environment:** Blazor Server / Hybrid (BlazorWebView), .NET 9+; introduced by PR #4363

**Repository links:**
- https://github.com/mono/SkiaSharp/pull/4363 — PR #4363 — Support SkiaSharp Blazor views on Server and Hybrid hosts (draft, parent work)

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The bridged rendering path does not yet exist in main; PR #4363 is still a draft. All follow-ups are new work. |

## Analysis

### Technical Summary

The issue bundles several follow-up work items for the Blazor Server/Hybrid bridged rendering path introduced by draft PR #4363: (1) the primary enhancement — partial/damage-based frame transfer using RFB-style pixel-buffer diffing to send only the changed bounding-rect over SignalR instead of a full frame; (2) a Blazor Hybrid BlazorWebView sample plus UITesting page; (3) full test web-app pages (Static/Server/Auto) + CI lane; (4) native-asset packaging pack-verification; (5) static-SSR poster frame; (6) folding the WASM SKHtmlCanvas paint onto the shared SKCanvasPresenter; (7) API XML docs for the new surface. The damage-based transfer is well-designed: keep a previous RGBA buffer, scan row/col min–max to get a bounding rect, encode only the sub-rect, blit with putImageData/texSubImage2D, suppress if ≥65% changed, opt-in via SKBlazorOptions.DifferentialTransfer.

### Rationale

This is unambiguously a feature-request: the author is the same maintainer who wrote PR #4363, and is tracking future enhancements. There is no bug, regression, or incorrect behaviour — only planned-but-unimplemented improvements. The primary feature (damage-based frame transfer) has a thorough design with alternatives considered. The other items are smaller follow-ups. Classified keep-open because #4363 is still draft and all sub-features have known implementation paths.

### Key Signals

- "every Invalidate() transfers the whole canvas frame, even when only a tiny region changed" — **issue body** (Clear performance problem in the Server/Hybrid bridged path that damage-based transfer would fix.)
- "Opt-in (SKBlazorOptions.DifferentialTransfer + per-control override); bridged-only, WebAssembly unaffected" — **issue body** (Author has already designed the API surface; implementation path is clear.)
- "Introduced by: #4363" — **issue body** (This issue is a structured tracking list for post-merge follow-up work; all items depend on #4363 landing first.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKCanvasView.razor.cs` | 12-108 | direct | Current implementation is decorated [SupportedOSPlatform("browser")] and calls interop.PutImageData with the full pixel buffer each frame. No bridged/Server/Hybrid path exists in main yet — confirms all requested features are genuinely new work not yet implemented. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKGLView.razor.cs` | — | direct | SKGLView similarly targets browser-only path with no damage-rect or bridged renderer. GPU-side texSubImage2D for partial updates would be needed as the second target for damage-based transfer. |

### Next Questions

- Has PR #4363 merged? All follow-ups depend on it.
- Should the multiple follow-up items be split into separate issues for independent tracking?
- Is SKBlazorOptions.DifferentialTransfer the desired API name or is it subject to change?

### Resolution Proposals

**Hypothesis:** All listed follow-ups are valid, well-defined enhancements to the Blazor Server/Hybrid bridged path. The damage-based transfer is the highest-value item; the rest are test coverage, samples, packaging, and docs.

1. **Track as umbrella issue for Blazor bridged-path follow-ups** — alternative, confidence 0.90 (90%), cost/xs, validated=untested
   - Keep this issue open as the parent tracking issue for all Blazor Server/Hybrid follow-ups. Link sub-issues or PRs as they are created.
2. **Implement damage-based frame transfer** — fix, confidence 0.85 (85%), cost/m, validated=untested
   - Add a previous-frame RGBA buffer to the bridged renderer, scan row/col min-max to compute a bounding rect, encode only that sub-rect, emit {x,y,w,h,bytes} to the JS presenter which calls putImageData(sub,x,y) (Canvas2D) or texSubImage2D (WebGL). Suppress when change area ≥65% or on first frame/resize. Expose opt-in via SKBlazorOptions.DifferentialTransfer.

**Recommended proposal:** Track as umbrella issue for Blazor bridged-path follow-ups

**Why:** The issue is already structured as a tracking list. Keeping it open with links to sub-PRs is the cleanest project management approach while #4363 is still draft.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.92 (92%) |
| Reason | Valid, well-scoped feature request with clear design. Depends on draft PR #4363 landing first. No information is missing; issue should remain open as a tracking issue for follow-up enhancements. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Confirm existing labels are correct (type/feature-request, area/SkiaSharp.Views.Blazor); add triage/triaged | labels=type/feature-request, area/SkiaSharp.Views.Blazor, triage/triaged |
| link-related | low | 0.99 (99%) | Cross-reference PR #4363 (parent Blazor Server/Hybrid implementation that this issue depends on) | linkedIssue=#4363 |
| add-comment | medium | 0.85 (85%) | Acknowledge the feature request, confirm tracking scope, note dependency on #4363 | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed design write-up! This is now tracked as the umbrella follow-up for the Blazor Server/Hybrid bridged rendering path introduced in #4363.

The damage-based frame transfer proposal is well-specified (RFB-style bounding-rect diff → `{x,y,w,h,bytes}` → `putImageData`/`texSubImage2D`, opt-in via `SKBlazorOptions.DifferentialTransfer`). We'll pick this up once #4363 lands.

The other follow-ups (Blazor Hybrid sample + UITesting, full test web-app pages + CI lane, native-asset pack-verification, static-SSR poster frame, shared presenter refactor, API XML docs) are all noted here for post-merge work.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4367,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-07-30T05:12:56Z",
    "currentLabels": [
      "type/feature-request",
      "area/SkiaSharp.Views.Blazor"
    ]
  },
  "summary": "Feature request to add partial (damage-based) frame transfer to the Blazor Server/Hybrid bridged rendering path and track additional follow-ups from PR #4363, including a Blazor Hybrid sample, full test web-app pages, native-asset pack verification, static-SSR poster frame, shared presenter refactor, and API XML docs.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views.Blazor",
      "confidence": 0.99
    }
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Blazor Server / Hybrid (BlazorWebView), .NET 9+; introduced by PR #4363",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/4363",
          "description": "PR #4363 — Support SkiaSharp Blazor views on Server and Hybrid hosts (draft, parent work)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "The bridged rendering path does not yet exist in main; PR #4363 is still a draft. All follow-ups are new work."
    }
  },
  "analysis": {
    "summary": "The issue bundles several follow-up work items for the Blazor Server/Hybrid bridged rendering path introduced by draft PR #4363: (1) the primary enhancement — partial/damage-based frame transfer using RFB-style pixel-buffer diffing to send only the changed bounding-rect over SignalR instead of a full frame; (2) a Blazor Hybrid BlazorWebView sample plus UITesting page; (3) full test web-app pages (Static/Server/Auto) + CI lane; (4) native-asset packaging pack-verification; (5) static-SSR poster frame; (6) folding the WASM SKHtmlCanvas paint onto the shared SKCanvasPresenter; (7) API XML docs for the new surface. The damage-based transfer is well-designed: keep a previous RGBA buffer, scan row/col min–max to get a bounding rect, encode only the sub-rect, blit with putImageData/texSubImage2D, suppress if ≥65% changed, opt-in via SKBlazorOptions.DifferentialTransfer.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKCanvasView.razor.cs",
        "lines": "12-108",
        "finding": "Current implementation is decorated [SupportedOSPlatform(\"browser\")] and calls interop.PutImageData with the full pixel buffer each frame. No bridged/Server/Hybrid path exists in main yet — confirms all requested features are genuinely new work not yet implemented.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKGLView.razor.cs",
        "finding": "SKGLView similarly targets browser-only path with no damage-rect or bridged renderer. GPU-side texSubImage2D for partial updates would be needed as the second target for damage-based transfer.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "every Invalidate() transfers the whole canvas frame, even when only a tiny region changed",
        "source": "issue body",
        "interpretation": "Clear performance problem in the Server/Hybrid bridged path that damage-based transfer would fix."
      },
      {
        "text": "Opt-in (SKBlazorOptions.DifferentialTransfer + per-control override); bridged-only, WebAssembly unaffected",
        "source": "issue body",
        "interpretation": "Author has already designed the API surface; implementation path is clear."
      },
      {
        "text": "Introduced by: #4363",
        "source": "issue body",
        "interpretation": "This issue is a structured tracking list for post-merge follow-up work; all items depend on #4363 landing first."
      }
    ],
    "rationale": "This is unambiguously a feature-request: the author is the same maintainer who wrote PR #4363, and is tracking future enhancements. There is no bug, regression, or incorrect behaviour — only planned-but-unimplemented improvements. The primary feature (damage-based frame transfer) has a thorough design with alternatives considered. The other items are smaller follow-ups. Classified keep-open because #4363 is still draft and all sub-features have known implementation paths.",
    "nextQuestions": [
      "Has PR #4363 merged? All follow-ups depend on it.",
      "Should the multiple follow-up items be split into separate issues for independent tracking?",
      "Is SKBlazorOptions.DifferentialTransfer the desired API name or is it subject to change?"
    ],
    "resolution": {
      "hypothesis": "All listed follow-ups are valid, well-defined enhancements to the Blazor Server/Hybrid bridged path. The damage-based transfer is the highest-value item; the rest are test coverage, samples, packaging, and docs.",
      "proposals": [
        {
          "title": "Track as umbrella issue for Blazor bridged-path follow-ups",
          "description": "Keep this issue open as the parent tracking issue for all Blazor Server/Hybrid follow-ups. Link sub-issues or PRs as they are created.",
          "category": "alternative",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Implement damage-based frame transfer",
          "description": "Add a previous-frame RGBA buffer to the bridged renderer, scan row/col min-max to compute a bounding rect, encode only that sub-rect, emit {x,y,w,h,bytes} to the JS presenter which calls putImageData(sub,x,y) (Canvas2D) or texSubImage2D (WebGL). Suppress when change area ≥65% or on first frame/resize. Expose opt-in via SKBlazorOptions.DifferentialTransfer.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Track as umbrella issue for Blazor bridged-path follow-ups",
      "recommendedReason": "The issue is already structured as a tracking list. Keeping it open with links to sub-PRs is the cleanest project management approach while #4363 is still draft."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.92,
      "reason": "Valid, well-scoped feature request with clear design. Depends on draft PR #4363 landing first. No information is missing; issue should remain open as a tracking issue for follow-up enhancements.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Confirm existing labels are correct (type/feature-request, area/SkiaSharp.Views.Blazor); add triage/triaged",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp.Views.Blazor",
          "triage/triaged"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference PR #4363 (parent Blazor Server/Hybrid implementation that this issue depends on)",
        "risk": "low",
        "confidence": 0.99,
        "linkedIssue": 4363
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the feature request, confirm tracking scope, note dependency on #4363",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed design write-up! This is now tracked as the umbrella follow-up for the Blazor Server/Hybrid bridged rendering path introduced in #4363.\n\nThe damage-based frame transfer proposal is well-specified (RFB-style bounding-rect diff → `{x,y,w,h,bytes}` → `putImageData`/`texSubImage2D`, opt-in via `SKBlazorOptions.DifferentialTransfer`). We'll pick this up once #4363 lands.\n\nThe other follow-ups (Blazor Hybrid sample + UITesting, full test web-app pages + CI lane, native-asset pack-verification, static-SSR poster frame, shared presenter refactor, API XML docs) are all noted here for post-merge work."
      }
    ]
  }
}
```

</details>
