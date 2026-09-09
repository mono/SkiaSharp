# Issue Triage Report — #4721

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-09-09T04:27:55Z |
| Type | type/feature-request (0.99 (99%)) |
| Area | area/SkiaSharp.Views.Maui (0.93 (93%)) |
| Suggested action | needs-investigation (0.90 (90%)) |

**Issue Summary:** The issue requests native Graphite presentation views for Apple and Android plus a .NET MAUI SKGraphiteView, because the existing Graphite bindings only cover the record, snap, insert, and submit workflow.

**Analysis:** The core managed Graphite API has landed, including recorder creation and the basic InsertRecording/Submit path, but the repository has no Graphite presentation-view classes. Existing Apple SKMetalView and MAUI SKGLView are Ganesh-specific: they expose GRContext and use GR backend contexts, so they cannot safely represent Graphite's explicit recorder/recording lifecycle. Completing the request requires separate native presentation implementations and MAUI handlers, with Android additionally depending on presentation synchronization interop.

**Recommendations:** **needs-investigation** — The feature is actively staged but incomplete: the current tree has no requested view types, and the remaining platform/lifecycle design depends on open prerequisite work.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/Android, os/iOS, os/macOS, os/tvOS |
| Backends | backend/Metal, backend/Vulkan |
| Tenets | — |
| Perf | — |
| Partner | partner/maui |
| Current labels | type/feature-request |

## Evidence

### Reproduction

**Related issues:** #3962, #3968

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3962 — Open Graphite backend tracking issue.
- https://github.com/mono/SkiaSharp/pull/3968 — Merged core Graphite binding implementation.
- https://github.com/mono/SkiaSharp/pull/4728 — Closed superseded cumulative native and MAUI Graphite views draft.
- https://github.com/mono/SkiaSharp/pull/4729 — Open Graphite presentation interop prerequisite.
- https://github.com/mono/SkiaSharp/pull/4734 — Closed superseded native Graphite views draft.
- https://github.com/mono/SkiaSharp/pull/4739 — Closed superseded MAUI Graphite view draft.
- https://github.com/mono/SkiaSharp/pull/4745 — Open native Apple Graphite view implementation.
- https://github.com/mono/SkiaSharp/pull/4885 — Closed Graphite performance-lab sample using the existing core APIs.

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 4.152.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The current checkout contains core Graphite types but no SKGraphiteMetalView, SKGraphiteVulkanView, or SKGraphiteView implementation. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | False |
| Confidence | 0.95 (95%) |
| Reason | Core Graphite bindings are present, but the requested view types are absent; the Apple-native implementation remains in open PR #4745 and Android/MAUI work is explicitly staged after its prerequisites. |
| Related PRs | #3968, #4728, #4729, #4734, #4739, #4745, #4885 |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

The core managed Graphite API has landed, including recorder creation and the basic InsertRecording/Submit path, but the repository has no Graphite presentation-view classes. Existing Apple SKMetalView and MAUI SKGLView are Ganesh-specific: they expose GRContext and use GR backend contexts, so they cannot safely represent Graphite's explicit recorder/recording lifecycle. Completing the request requires separate native presentation implementations and MAUI handlers, with Android additionally depending on presentation synchronization interop.

### Rationale

This is a well-specified new UI integration rather than a malfunction: the requested types are absent from the current tree, while the underlying Graphite binding is present. The requested scope is primarily a MAUI view layer, spans Apple and Android, and relies on Metal and Vulkan presentation. Related PRs show active staged implementation rather than a completed fix, so the issue should remain open for investigation and coordinated landing.

### Key Signals

- "applications still cannot host Graphite in an on-screen native control or a .NET MAUI view" — **issue body** (The request is for a missing integration layer, not a defect in an existing control.)
- "After both foundations land, separate Apple MAUI and Android-native PRs can proceed in parallel." — **issue comment 5259690576** (The implementation is intentionally staged and remains incomplete.)
- "This Apple-native PR intentionally has no dependency on either. After both foundations land, Apple MAUI and Android native views will proceed as separate parallel PRs." — **PR #4745 comment 5259690531** (Open PR #4745 only covers the Apple-native foundation, not the full requested MAUI/Android scope.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/Gpu/Graphite/SKGraphiteContext.cs` | 161-254 | direct | SKGraphiteContext provides CreateRecorder, InsertRecording, and Submit, demonstrating that the core record/snap/insert/submit binding exists but does not supply an on-screen view abstraction. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/SKMetalView.cs` | 44-178 | direct | The existing Apple view owns a GRMtlBackendContext and GRContext, creates a GRBackendRenderTarget, and presents through Ganesh; it is not a Graphite presentation implementation. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SKGLView.cs` | 11-96 | direct | The existing MAUI GPU control is SKGLView and stores/exposes GRContext, confirming the current MAUI API is Ganesh-oriented rather than Graphite-oriented. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/AppHostBuilderExtensions.cs` | 11-24 | direct | UseSkiaSharp registers only SKCanvasViewHandler and SKGLViewHandler, with no Graphite view handler registration. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/ISKGLView.cs` | 5-25 | related | ISKGLView is explicitly shaped around GRContext and SKPaintGLSurfaceEventArgs, so a Graphite control needs a separate interface and event model rather than a transparent backend substitution. |

### Next Questions

- What API and lifecycle contract should SKGraphiteView expose for failed Graphite submission, device loss, and context recreation?
- Which parts of the Android Vulkan synchronization interop in #4729 and mono/skia#345 have landed before the Android native view is implemented?
- Should macOS and tvOS ship with the initial MAUI-facing feature or remain native-only as currently staged?

### Resolution Proposals

**Hypothesis:** The gap is architectural rather than a localized missing wrapper: existing views are coupled to Ganesh contexts, while Graphite requires explicit recorder ownership, ordered recording insertion, backend presentation, and lifecycle recovery.

1. **Land the staged Graphite view foundations** — investigation, confidence 0.90 (90%), cost/xl, validated=untested
   - Complete and review the independent Apple-native view work in #4745 and the Graphite presentation interop in #4729 with its mono/skia prerequisite, then implement the remaining Android-native and MAUI layers on those foundations.
2. **Define a Graphite-specific MAUI lifecycle contract** — investigation, confidence 0.88 (88%), cost/l, validated=untested
   - Before the MAUI convergence PR, specify handler behavior for recorder ownership, invalidation, rendering failures, device loss, and unsupported platforms without falling back to Ganesh.

**Recommended proposal:** Land the staged Graphite view foundations

**Why:** The current open work already isolates the required native Apple and presentation-interop foundations; the MAUI and Android layers depend on that ordering.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.90 (90%) |
| Reason | The feature is actively staged but incomplete: the current tree has no requested view types, and the remaining platform/lifecycle design depends on open prerequisite work. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.99 (99%) | Apply the feature, MAUI, platform, Graphite-backend, and MAUI-partner labels. | labels=type/feature-request, area/SkiaSharp.Views.Maui, os/Android, os/iOS, os/macOS, os/tvOS, backend/Metal, backend/Vulkan, partner/maui |
| link-related | low | 0.98 (98%) | Link the Graphite backend tracking issue. | linkedIssue=#3962 |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4721,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-09-09T04:27:55Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "The issue requests native Graphite presentation views for Apple and Android plus a .NET MAUI SKGraphiteView, because the existing Graphite bindings only cover the record, snap, insert, and submit workflow.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.99
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.93
    },
    "platforms": [
      "os/Android",
      "os/iOS",
      "os/macOS",
      "os/tvOS"
    ],
    "backends": [
      "backend/Metal",
      "backend/Vulkan"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "reproEvidence": {
      "relatedIssues": [
        3962,
        3968
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3962",
          "description": "Open Graphite backend tracking issue."
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/3968",
          "description": "Merged core Graphite binding implementation."
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/4728",
          "description": "Closed superseded cumulative native and MAUI Graphite views draft."
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/4729",
          "description": "Open Graphite presentation interop prerequisite."
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/4734",
          "description": "Closed superseded native Graphite views draft."
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/4739",
          "description": "Closed superseded MAUI Graphite view draft."
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/4745",
          "description": "Open native Apple Graphite view implementation."
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/4885",
          "description": "Closed Graphite performance-lab sample using the existing core APIs."
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "4.152.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The current checkout contains core Graphite types but no SKGraphiteMetalView, SKGraphiteVulkanView, or SKGraphiteView implementation."
    },
    "fixStatus": {
      "likelyFixed": false,
      "confidence": 0.95,
      "reason": "Core Graphite bindings are present, but the requested view types are absent; the Apple-native implementation remains in open PR #4745 and Android/MAUI work is explicitly staged after its prerequisites.",
      "relatedPRs": [
        3968,
        4728,
        4729,
        4734,
        4739,
        4745,
        4885
      ]
    }
  },
  "analysis": {
    "summary": "The core managed Graphite API has landed, including recorder creation and the basic InsertRecording/Submit path, but the repository has no Graphite presentation-view classes. Existing Apple SKMetalView and MAUI SKGLView are Ganesh-specific: they expose GRContext and use GR backend contexts, so they cannot safely represent Graphite's explicit recorder/recording lifecycle. Completing the request requires separate native presentation implementations and MAUI handlers, with Android additionally depending on presentation synchronization interop.",
    "rationale": "This is a well-specified new UI integration rather than a malfunction: the requested types are absent from the current tree, while the underlying Graphite binding is present. The requested scope is primarily a MAUI view layer, spans Apple and Android, and relies on Metal and Vulkan presentation. Related PRs show active staged implementation rather than a completed fix, so the issue should remain open for investigation and coordinated landing.",
    "keySignals": [
      {
        "text": "applications still cannot host Graphite in an on-screen native control or a .NET MAUI view",
        "source": "issue body",
        "interpretation": "The request is for a missing integration layer, not a defect in an existing control."
      },
      {
        "text": "After both foundations land, separate Apple MAUI and Android-native PRs can proceed in parallel.",
        "source": "issue comment 5259690576",
        "interpretation": "The implementation is intentionally staged and remains incomplete."
      },
      {
        "text": "This Apple-native PR intentionally has no dependency on either. After both foundations land, Apple MAUI and Android native views will proceed as separate parallel PRs.",
        "source": "PR #4745 comment 5259690531",
        "interpretation": "Open PR #4745 only covers the Apple-native foundation, not the full requested MAUI/Android scope."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/Gpu/Graphite/SKGraphiteContext.cs",
        "lines": "161-254",
        "finding": "SKGraphiteContext provides CreateRecorder, InsertRecording, and Submit, demonstrating that the core record/snap/insert/submit binding exists but does not supply an on-screen view abstraction.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/SKMetalView.cs",
        "lines": "44-178",
        "finding": "The existing Apple view owns a GRMtlBackendContext and GRContext, creates a GRBackendRenderTarget, and presents through Ganesh; it is not a Graphite presentation implementation.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SKGLView.cs",
        "lines": "11-96",
        "finding": "The existing MAUI GPU control is SKGLView and stores/exposes GRContext, confirming the current MAUI API is Ganesh-oriented rather than Graphite-oriented.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/AppHostBuilderExtensions.cs",
        "lines": "11-24",
        "finding": "UseSkiaSharp registers only SKCanvasViewHandler and SKGLViewHandler, with no Graphite view handler registration.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/ISKGLView.cs",
        "lines": "5-25",
        "finding": "ISKGLView is explicitly shaped around GRContext and SKPaintGLSurfaceEventArgs, so a Graphite control needs a separate interface and event model rather than a transparent backend substitution.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "What API and lifecycle contract should SKGraphiteView expose for failed Graphite submission, device loss, and context recreation?",
      "Which parts of the Android Vulkan synchronization interop in #4729 and mono/skia#345 have landed before the Android native view is implemented?",
      "Should macOS and tvOS ship with the initial MAUI-facing feature or remain native-only as currently staged?"
    ],
    "resolution": {
      "hypothesis": "The gap is architectural rather than a localized missing wrapper: existing views are coupled to Ganesh contexts, while Graphite requires explicit recorder ownership, ordered recording insertion, backend presentation, and lifecycle recovery.",
      "proposals": [
        {
          "title": "Land the staged Graphite view foundations",
          "description": "Complete and review the independent Apple-native view work in #4745 and the Graphite presentation interop in #4729 with its mono/skia prerequisite, then implement the remaining Android-native and MAUI layers on those foundations.",
          "category": "investigation",
          "validated": "untested",
          "confidence": 0.9,
          "effort": "cost/xl"
        },
        {
          "title": "Define a Graphite-specific MAUI lifecycle contract",
          "description": "Before the MAUI convergence PR, specify handler behavior for recorder ownership, invalidation, rendering failures, device loss, and unsupported platforms without falling back to Ganesh.",
          "category": "investigation",
          "validated": "untested",
          "confidence": 0.88,
          "effort": "cost/l"
        }
      ],
      "recommendedProposal": "Land the staged Graphite view foundations",
      "recommendedReason": "The current open work already isolates the required native Apple and presentation-interop foundations; the MAUI and Android layers depend on that ordering."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.9,
      "reason": "The feature is actively staged but incomplete: the current tree has no requested view types, and the remaining platform/lifecycle design depends on open prerequisite work.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply the feature, MAUI, platform, Graphite-backend, and MAUI-partner labels.",
        "risk": "low",
        "confidence": 0.99,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp.Views.Maui",
          "os/Android",
          "os/iOS",
          "os/macOS",
          "os/tvOS",
          "backend/Metal",
          "backend/Vulkan",
          "partner/maui"
        ]
      },
      {
        "type": "link-related",
        "description": "Link the Graphite backend tracking issue.",
        "risk": "low",
        "confidence": 0.98,
        "linkedIssue": 3962
      }
    ]
  }
}
```

</details>
