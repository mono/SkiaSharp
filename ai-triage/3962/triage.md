# Issue Triage Report — #3962

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-06-29T05:55:00Z |
| Type | type/feature-request (0.98 (98%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** Feature request to add C# bindings for Skia's Graphite GPU backend (replacing Ganesh), exposing Context, Recorder, and Recording APIs for Metal, Vulkan, and Dawn backends; tracked as part of the SkiaSharp 4.150.x release cycle milestone 4.150.0-preview.3, assigned to a contributor.

**Analysis:** No Graphite bindings exist in SkiaSharp: GRContext.cs and GRRecordingContext.cs expose only Ganesh (OpenGL, Vulkan, D3D, Metal via gr_direct_context_make_*). Graphite requires entirely new C API shims for skgpu::graphite::Context, Recorder, and Recording, plus new C# types and platform view updates. The issue is well-specified with a checklist, assigned to a contributor, and milestoned for 4.150.0-preview.3 in the active release cycle.

**Recommendations:** **needs-investigation** — Well-specified feature request, assigned and milestoned. Design decisions on the C API shape, Dawn strategy, and platform view changes must be resolved before coding can begin.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | os/Android, os/iOS, os/Linux, os/macOS, os/WASM, os/Windows-Classic |
| Backends | backend/Metal, backend/Vulkan |
| Tenets | — |
| Partner | partner/unoplatform |
| Current labels | type/feature-request, partner/unoplatform, upgrading/4.x, cost/xl, priority/3 |

## Evidence

### Reproduction

**Related issues:** #4015

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/4015 — SkiaSharp 4.148.x/4.150.x Release Tracking — Graphite GPU backend is a key work stream in the milestone

## Analysis

### Technical Summary

No Graphite bindings exist in SkiaSharp: GRContext.cs and GRRecordingContext.cs expose only Ganesh (OpenGL, Vulkan, D3D, Metal via gr_direct_context_make_*). Graphite requires entirely new C API shims for skgpu::graphite::Context, Recorder, and Recording, plus new C# types and platform view updates. The issue is well-specified with a checklist, assigned to a contributor, and milestoned for 4.150.0-preview.3 in the active release cycle.

### Rationale

The issue is explicitly labeled type/feature-request, assigned, and milestoned — it is a planned feature with a clear scope. The Graphite programming model (Context → Recorder → Recording) differs fundamentally from Ganesh (GRContext immediate submission) and requires additive new types. The issue already provides a thorough technical description and implementation checklist, making the classification and action straightforward. The suggestedAction is needs-investigation because the design decisions (C API shape, Dawn strategy, platform view changes) need to be resolved before implementation.

### Key Signals

- "Part of #4015" — **issue body** (This is a tracked, milestoned feature in the active 4.150.x release cycle — not a speculative request.)
- "Graphite is already in production — Chrome ships it on Apple Silicon Macs with ~15% rendering performance improvement" — **issue body** (Upstream Graphite API is stable enough for Chrome production; SkiaSharp adoption is well-timed and not premature.)
- "Nothing changes today — SkiaSharp v4 continues to use Ganesh for all GPU rendering" — **issue body** (Graphite support would be purely additive; no breaking changes required for existing users.)
- "Design C API shims for Graphite core types (Context, Recorder, Recording)" — **issue body checklist** (First concrete implementation task is well-defined: expose sk_graphite_context_t, sk_graphite_recorder_t, and sk_graphite_recording_t in the C shim layer.)
- "Dawn support was removed from Ganesh in Skia m118 — Dawn is Graphite-only" — **issue body** (Dawn (and thus D3D12/WebGPU on Windows/WASM) is only accessible through Graphite, making this feature critical for those platforms long-term.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/GRContext.cs` | — | direct | GRContext exposes only Ganesh GPU contexts via CreateGl, CreateVulkan, CreateMetal, CreateDirect3D factory methods. No skgpu::graphite::Context bindings exist — a new class (e.g. GRGraphiteContext) would be needed. |
| `binding/SkiaSharp/GRRecordingContext.cs` | — | related | GRRecordingContext wraps Ganesh's GrRecordingContext. Graphite's Recorder (skgpu::graphite::Recorder) is an unrelated type — it exposes an SKCanvas for draw call accumulation then produces a Recording object for submission, requiring a separate C# binding class. |

### Workarounds

- Continue using Ganesh-based GPU rendering via GRContext.CreateGl(), CreateVulkan(), CreateMetal(), or CreateDirect3D() — all existing GPU code remains fully functional until Graphite support lands.
- For CPU-only scenarios, use SKSurface.Create() with a raster backend as a temporary alternative that avoids GPU complexity entirely.

### Next Questions

- Will Dawn be included in the initial Graphite implementation or deferred to a follow-up issue, and what are the build infrastructure implications?
- Should new platform view types (e.g. SKGraphiteCanvasView) be introduced, or will existing SKCanvasView/SKGLView be updated to support Graphite context creation?
- What is the strategy for conditionally shipping Dawn as an optional NuGet package to avoid mandatory build dependencies on platforms that don't need it?

### Resolution Proposals

**Hypothesis:** Graphite support requires additive C API shims plus new C# wrapper types (GRGraphiteContext, GRGraphiteRecorder, GRGraphiteRecording), starting with Metal and Vulkan backends where Graphite is most mature in upstream Skia.

1. **Phase 1: Metal + Vulkan Graphite bindings** — fix, confidence 0.85 (85%), cost/xl, validated=untested
   - Add C API shims for skgpu::graphite::Context, Recorder, and Recording. Add C# wrappers GRGraphiteContext, GRGraphiteRecorder, GRGraphiteRecording. Expose SKSurface.CreateAsGraphiteRenderTarget for Metal (macOS/iOS) and Vulkan (Android/Linux). No new build dependencies required.
2. **Phase 2: Dawn backend support** — fix, confidence 0.75 (75%), cost/xl, validated=untested
   - Add Dawn (WebGPU) backend to unlock D3D12 on Windows and WebGPU on WASM. Requires evaluating Dawn build dependencies and whether to ship Dawn as an optional NuGet package. This is the only path for post-Ganesh GPU support on Windows and WASM.
3. **Coexistence migration period** — alternative, confidence 0.95 (95%), cost/xs, validated=untested
   - Keep all Ganesh APIs (GRContext, GRRecordingContext) intact with no deprecation during the Graphite rollout. Allow opt-in to Graphite by constructing a GRGraphiteContext. This avoids breaking changes and lets developers test Graphite on a per-app basis before committing to migration.

**Recommended proposal:** Phase 1: Metal + Vulkan Graphite bindings

**Why:** Matches the implementation priority in the issue checklist ('Metal most mature → Vulkan → Dawn'). Covers the largest user segments (macOS/iOS via Metal, Android/Linux via Vulkan) without incurring Dawn build complexity, and unblocks partner/unoplatform scenarios on those platforms.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Well-specified feature request, assigned and milestoned. Design decisions on the C API shape, Dawn strategy, and platform view changes must be resolved before coding can begin. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply feature-request, area/SkiaSharp, affected platform and backend labels, and partner/unoplatform | labels=type/feature-request, area/SkiaSharp, os/Android, os/iOS, os/Linux, os/macOS, os/WASM, os/Windows-Classic, backend/Metal, backend/Vulkan, partner/unoplatform |
| link-related | low | 0.98 (98%) | Cross-reference the 4.150.x release tracking issue | linkedIssue=#4015 |
| add-comment | medium | 0.85 (85%) | Acknowledge the plan and surface key design questions before implementation | — |

**Comment draft for `add-comment`:**

```markdown
The implementation plan is well-scoped and the phased Metal → Vulkan → Dawn prioritization matches upstream Graphite maturity.

A few design questions worth settling before coding begins:

1. **C# type hierarchy** — Graphite's `Context` and `Recorder` are fundamentally different from Ganesh's `GRContext`/`GRRecordingContext`. New types (`GRGraphiteContext`, `GRGraphiteRecorder`, `GRGraphiteRecording`) would avoid naming confusion and preserve ABI stability on the Ganesh side during coexistence.

2. **SKSurface integration** — A `SKSurface.CreateAsGraphiteRenderTarget()` factory (mirroring the existing `CreateAsRenderTarget`) would let most existing rendering code work unchanged once the context is switched.

3. **Dawn scope** — Deferring Dawn to a Phase 2 issue seems prudent given the extra build dependencies. Consider opening a separate tracking issue for Dawn so milestone scope stays clear.

Continuing to watch progress on this — great work scoping it out!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3962,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-06-29T05:55:00Z",
    "currentLabels": [
      "type/feature-request",
      "partner/unoplatform",
      "upgrading/4.x",
      "cost/xl",
      "priority/3"
    ]
  },
  "summary": "Feature request to add C# bindings for Skia's Graphite GPU backend (replacing Ganesh), exposing Context, Recorder, and Recording APIs for Metal, Vulkan, and Dawn backends; tracked as part of the SkiaSharp 4.150.x release cycle milestone 4.150.0-preview.3, assigned to a contributor.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.98
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    },
    "platforms": [
      "os/Android",
      "os/iOS",
      "os/Linux",
      "os/macOS",
      "os/WASM",
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/Metal",
      "backend/Vulkan"
    ],
    "partner": "partner/unoplatform"
  },
  "evidence": {
    "reproEvidence": {
      "relatedIssues": [
        4015
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/4015",
          "description": "SkiaSharp 4.148.x/4.150.x Release Tracking — Graphite GPU backend is a key work stream in the milestone"
        }
      ]
    }
  },
  "analysis": {
    "summary": "No Graphite bindings exist in SkiaSharp: GRContext.cs and GRRecordingContext.cs expose only Ganesh (OpenGL, Vulkan, D3D, Metal via gr_direct_context_make_*). Graphite requires entirely new C API shims for skgpu::graphite::Context, Recorder, and Recording, plus new C# types and platform view updates. The issue is well-specified with a checklist, assigned to a contributor, and milestoned for 4.150.0-preview.3 in the active release cycle.",
    "rationale": "The issue is explicitly labeled type/feature-request, assigned, and milestoned — it is a planned feature with a clear scope. The Graphite programming model (Context → Recorder → Recording) differs fundamentally from Ganesh (GRContext immediate submission) and requires additive new types. The issue already provides a thorough technical description and implementation checklist, making the classification and action straightforward. The suggestedAction is needs-investigation because the design decisions (C API shape, Dawn strategy, platform view changes) need to be resolved before implementation.",
    "keySignals": [
      {
        "text": "Part of #4015",
        "source": "issue body",
        "interpretation": "This is a tracked, milestoned feature in the active 4.150.x release cycle — not a speculative request."
      },
      {
        "text": "Graphite is already in production — Chrome ships it on Apple Silicon Macs with ~15% rendering performance improvement",
        "source": "issue body",
        "interpretation": "Upstream Graphite API is stable enough for Chrome production; SkiaSharp adoption is well-timed and not premature."
      },
      {
        "text": "Nothing changes today — SkiaSharp v4 continues to use Ganesh for all GPU rendering",
        "source": "issue body",
        "interpretation": "Graphite support would be purely additive; no breaking changes required for existing users."
      },
      {
        "text": "Design C API shims for Graphite core types (Context, Recorder, Recording)",
        "source": "issue body checklist",
        "interpretation": "First concrete implementation task is well-defined: expose sk_graphite_context_t, sk_graphite_recorder_t, and sk_graphite_recording_t in the C shim layer."
      },
      {
        "text": "Dawn support was removed from Ganesh in Skia m118 — Dawn is Graphite-only",
        "source": "issue body",
        "interpretation": "Dawn (and thus D3D12/WebGPU on Windows/WASM) is only accessible through Graphite, making this feature critical for those platforms long-term."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/GRContext.cs",
        "finding": "GRContext exposes only Ganesh GPU contexts via CreateGl, CreateVulkan, CreateMetal, CreateDirect3D factory methods. No skgpu::graphite::Context bindings exist — a new class (e.g. GRGraphiteContext) would be needed.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/GRRecordingContext.cs",
        "finding": "GRRecordingContext wraps Ganesh's GrRecordingContext. Graphite's Recorder (skgpu::graphite::Recorder) is an unrelated type — it exposes an SKCanvas for draw call accumulation then produces a Recording object for submission, requiring a separate C# binding class.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Continue using Ganesh-based GPU rendering via GRContext.CreateGl(), CreateVulkan(), CreateMetal(), or CreateDirect3D() — all existing GPU code remains fully functional until Graphite support lands.",
      "For CPU-only scenarios, use SKSurface.Create() with a raster backend as a temporary alternative that avoids GPU complexity entirely."
    ],
    "nextQuestions": [
      "Will Dawn be included in the initial Graphite implementation or deferred to a follow-up issue, and what are the build infrastructure implications?",
      "Should new platform view types (e.g. SKGraphiteCanvasView) be introduced, or will existing SKCanvasView/SKGLView be updated to support Graphite context creation?",
      "What is the strategy for conditionally shipping Dawn as an optional NuGet package to avoid mandatory build dependencies on platforms that don't need it?"
    ],
    "resolution": {
      "hypothesis": "Graphite support requires additive C API shims plus new C# wrapper types (GRGraphiteContext, GRGraphiteRecorder, GRGraphiteRecording), starting with Metal and Vulkan backends where Graphite is most mature in upstream Skia.",
      "proposals": [
        {
          "title": "Phase 1: Metal + Vulkan Graphite bindings",
          "description": "Add C API shims for skgpu::graphite::Context, Recorder, and Recording. Add C# wrappers GRGraphiteContext, GRGraphiteRecorder, GRGraphiteRecording. Expose SKSurface.CreateAsGraphiteRenderTarget for Metal (macOS/iOS) and Vulkan (Android/Linux). No new build dependencies required.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/xl",
          "validated": "untested"
        },
        {
          "title": "Phase 2: Dawn backend support",
          "description": "Add Dawn (WebGPU) backend to unlock D3D12 on Windows and WebGPU on WASM. Requires evaluating Dawn build dependencies and whether to ship Dawn as an optional NuGet package. This is the only path for post-Ganesh GPU support on Windows and WASM.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/xl",
          "validated": "untested"
        },
        {
          "title": "Coexistence migration period",
          "description": "Keep all Ganesh APIs (GRContext, GRRecordingContext) intact with no deprecation during the Graphite rollout. Allow opt-in to Graphite by constructing a GRGraphiteContext. This avoids breaking changes and lets developers test Graphite on a per-app basis before committing to migration.",
          "category": "alternative",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Phase 1: Metal + Vulkan Graphite bindings",
      "recommendedReason": "Matches the implementation priority in the issue checklist ('Metal most mature → Vulkan → Dawn'). Covers the largest user segments (macOS/iOS via Metal, Android/Linux via Vulkan) without incurring Dawn build complexity, and unblocks partner/unoplatform scenarios on those platforms."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Well-specified feature request, assigned and milestoned. Design decisions on the C API shape, Dawn strategy, and platform view changes must be resolved before coding can begin.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, area/SkiaSharp, affected platform and backend labels, and partner/unoplatform",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "os/Android",
          "os/iOS",
          "os/Linux",
          "os/macOS",
          "os/WASM",
          "os/Windows-Classic",
          "backend/Metal",
          "backend/Vulkan",
          "partner/unoplatform"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference the 4.150.x release tracking issue",
        "risk": "low",
        "confidence": 0.98,
        "linkedIssue": 4015
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the plan and surface key design questions before implementation",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "The implementation plan is well-scoped and the phased Metal → Vulkan → Dawn prioritization matches upstream Graphite maturity.\n\nA few design questions worth settling before coding begins:\n\n1. **C# type hierarchy** — Graphite's `Context` and `Recorder` are fundamentally different from Ganesh's `GRContext`/`GRRecordingContext`. New types (`GRGraphiteContext`, `GRGraphiteRecorder`, `GRGraphiteRecording`) would avoid naming confusion and preserve ABI stability on the Ganesh side during coexistence.\n\n2. **SKSurface integration** — A `SKSurface.CreateAsGraphiteRenderTarget()` factory (mirroring the existing `CreateAsRenderTarget`) would let most existing rendering code work unchanged once the context is switched.\n\n3. **Dawn scope** — Deferring Dawn to a Phase 2 issue seems prudent given the extra build dependencies. Consider opening a separate tracking issue for Dawn so milestone scope stays clear.\n\nContinuing to watch progress on this — great work scoping it out!"
      }
    ]
  }
}
```

</details>
