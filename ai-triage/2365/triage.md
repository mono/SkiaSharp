# Issue Triage Report — #2365

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-23T11:52:29Z |
| Type | type/feature-request (0.95 (95%)) |
| Area | area/SkiaSharp (0.80 (80%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** Feature request to add C# bindings for the full SkSG (Skia scene graph) module, which currently only exposes InvalidationController in the SkiaSharp.SceneGraph package.

**Analysis:** The SkiaSharp.SceneGraph package currently exposes only the InvalidationController from the SkSG module. The reporter wants bindings for the full SkSG scene graph (RenderNodes, GroupNode, transformations, effects, invalidation). The maintainer asked about use cases; interest was expressed for hierarchical 2D scene definition and rendering.

**Recommendations:** **keep-open** — Valid feature request with community interest. Maintainer is evaluating use cases. No blocking design decision yet. Keeping open allows the community to provide use-case feedback that could inform prioritization.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Environment:** No specific platform or version mentioned. Cross-platform request.

**Repository links:**
- https://api.skia.org/namespacesksg.html — Upstream Skia SkSG namespace reference (linked by commenter)

## Analysis

### Technical Summary

The SkiaSharp.SceneGraph package currently exposes only the InvalidationController from the SkSG module. The reporter wants bindings for the full SkSG scene graph (RenderNodes, GroupNode, transformations, effects, invalidation). The maintainer asked about use cases; interest was expressed for hierarchical 2D scene definition and rendering.

### Rationale

The title and content clearly express a request for new functionality (full SkSG bindings), not a bug. SkiaSharp.SceneGraph already exists as a package but only wraps InvalidationController. Expanding it requires adding a C API shim for the full RenderNode hierarchy in the Skia submodule, regenerating bindings, and writing C# wrappers — a medium-to-large effort. The maintainer is not opposed but wants use-case justification. The feature is valid and open; keeping it open for design discussion is appropriate.

### Key Signals

- "Is there a reason (other than lack of need) SkiaSharp doesn't have bindings for the scene graph module for anything but the InvalidationController?" — **issue body** (Reporter correctly identifies that only InvalidationController is bound and asks whether this is a deliberate scope decision or simply lack of demand.)
- "It isn't very big, and not that complex. For a nice API and probably better performance, it is probably better for C# apps to implement their own scene graph." — **comment by shanemikel (OP)** (OP acknowledges the SkSG module is limited; suggests the real value may be using it as a design reference rather than binding it verbatim.)
- "hierarchical scene definition, with RenderNodes rendering, grouping, transforming, effects and invalidations" — **comment by angelofb** (Second user articulates the core use case: composited 2D scene graph with RenderNode hierarchy for transformations and effects.)
- "What do you see it being used for?" — **comment by mattleibow (maintainer)** (Maintainer is evaluating the use case before committing. Typical needs-design-input response. No rejection.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.SceneGraph/InvalidationController.cs` | — | direct | Only InvalidationController is implemented in the SceneGraph package. Wraps sksg_invalidation_controller_* C API functions for begin/end/reset/inval/get_bounds lifecycle. |
| `binding/SkiaSharp.SceneGraph/SceneGraphApi.generated.cs` | — | direct | Generated P/Invoke bindings cover only sksg_invalidation_controller_* functions (new, delete, begin, end, reset, inval, get_bounds). No RenderNode, GroupNode, DrawNode, or other SkSG node types are bound. |
| `binding/libSkiaSharp.SceneGraph.json` | — | related | The SceneGraph build config maps sksg_* to the SceneGraph package but the C API shim in externals/skia only exposes InvalidationController — the broader SkSG node hierarchy has no C API wrapping yet. |

### Workarounds

- Use SkiaSharp's SKCanvas drawing API directly with custom C# scene graph logic — this provides full control and avoids the overhead of wrapping SkSG's C++ object hierarchy.
- Use the existing InvalidationController from SkiaSharp.SceneGraph to track dirty regions when implementing a manual C# scene graph over SKCanvas.
- Reference the upstream SkSG C++ implementation (https://api.skia.org/namespacesksg.html) as a design pattern for implementing a pure-managed C# scene graph.

### Next Questions

- What specific SkSG node types are needed (RenderNode subclasses: GroupNode, DrawNode, Transform, etc.)?
- Would a pure C# scene graph implementation be preferable to native SkSG bindings?
- Is this for Lottie/animation use cases, or general-purpose UI scene composition?

### Resolution Proposals

**Hypothesis:** SkSG bindings are missing because the SkSG module has no C API shim beyond InvalidationController — wrapping it requires substantial C API additions. A pure C# scene graph on top of SKCanvas may be a better user-facing design.

1. **Implement full SkSG C API and C# bindings** — fix, confidence 0.65 (65%), cost/xl, validated=untested
   - Add C API shim for SkSG RenderNode hierarchy (GroupNode, DrawNode, etc.) in externals/skia/src/c/, regenerate bindings, and write C# wrappers in SkiaSharp.SceneGraph. This is the direct path to fulfilling the request.
2. **Build a C# scene graph over SKCanvas (workaround)** — workaround, confidence 0.80 (80%), cost/l, validated=untested
   - Implement a pure-managed C# scene graph using SKCanvas as the renderer, using the SkSG C++ source as a design reference. Avoids C API work entirely and gives better API ergonomics for C#.
3. **Use InvalidationController with manual SKCanvas scene management** — workaround, confidence 0.85 (85%), cost/s, validated=untested
   - Use the existing InvalidationController to track dirty regions, paired with custom SKCanvas drawing logic. This covers the most common scene-graph use case (selective re-rendering) with current APIs.

**Recommended proposal:** Use InvalidationController with manual SKCanvas scene management

**Why:** Most practical near-term workaround with existing APIs. Full SkSG binding would need design work; a pure-managed C# approach is also worth considering but is more effort.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Valid feature request with community interest. Maintainer is evaluating use cases. No blocking design decision yet. Keeping open allows the community to provide use-case feedback that could inform prioritization. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply feature-request and SkiaSharp labels | labels=type/feature-request, area/SkiaSharp |
| add-comment | medium | 0.80 (80%) | Acknowledge the request, explain current state, and provide workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed question! Currently, the `SkiaSharp.SceneGraph` package only wraps `InvalidationController` from the SkSG module — the rest of the SkSG node hierarchy (RenderNode, GroupNode, DrawNode, Transform, etc.) has no C API shim, which is the layer required before C# bindings can be generated.

Adding full SkSG bindings would require:
1. Writing a C API shim for the SkSG RenderNode hierarchy in `externals/skia/src/c/`
2. Regenerating P/Invoke bindings
3. Writing C# wrapper classes for each node type

This is a non-trivial effort, and as one commenter noted, a pure-managed C# scene graph on top of `SKCanvas` may actually give better API ergonomics for .NET.

**In the meantime**, here are some practical alternatives:
- Use `InvalidationController` (already available in `SkiaSharp.SceneGraph`) to track dirty regions in your own rendering loop.
- Implement your own C# scene graph using `SKCanvas` as the renderer — refer to the [upstream SkSG source](https://api.skia.org/namespacesksg.html) as a design reference.

If you'd like to contribute SkSG bindings, the `documentation/dev/adding-apis.md` guide explains the C API + binding generation workflow. A PR adding the C API shim would be a great starting point!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2365,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-23T11:52:29Z"
  },
  "summary": "Feature request to add C# bindings for the full SkSG (Skia scene graph) module, which currently only exposes InvalidationController in the SkiaSharp.SceneGraph package.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.8
    }
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "No specific platform or version mentioned. Cross-platform request.",
      "repoLinks": [
        {
          "url": "https://api.skia.org/namespacesksg.html",
          "description": "Upstream Skia SkSG namespace reference (linked by commenter)"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The SkiaSharp.SceneGraph package currently exposes only the InvalidationController from the SkSG module. The reporter wants bindings for the full SkSG scene graph (RenderNodes, GroupNode, transformations, effects, invalidation). The maintainer asked about use cases; interest was expressed for hierarchical 2D scene definition and rendering.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.SceneGraph/InvalidationController.cs",
        "finding": "Only InvalidationController is implemented in the SceneGraph package. Wraps sksg_invalidation_controller_* C API functions for begin/end/reset/inval/get_bounds lifecycle.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.SceneGraph/SceneGraphApi.generated.cs",
        "finding": "Generated P/Invoke bindings cover only sksg_invalidation_controller_* functions (new, delete, begin, end, reset, inval, get_bounds). No RenderNode, GroupNode, DrawNode, or other SkSG node types are bound.",
        "relevance": "direct"
      },
      {
        "file": "binding/libSkiaSharp.SceneGraph.json",
        "finding": "The SceneGraph build config maps sksg_* to the SceneGraph package but the C API shim in externals/skia only exposes InvalidationController — the broader SkSG node hierarchy has no C API wrapping yet.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Is there a reason (other than lack of need) SkiaSharp doesn't have bindings for the scene graph module for anything but the InvalidationController?",
        "source": "issue body",
        "interpretation": "Reporter correctly identifies that only InvalidationController is bound and asks whether this is a deliberate scope decision or simply lack of demand."
      },
      {
        "text": "It isn't very big, and not that complex. For a nice API and probably better performance, it is probably better for C# apps to implement their own scene graph.",
        "source": "comment by shanemikel (OP)",
        "interpretation": "OP acknowledges the SkSG module is limited; suggests the real value may be using it as a design reference rather than binding it verbatim."
      },
      {
        "text": "hierarchical scene definition, with RenderNodes rendering, grouping, transforming, effects and invalidations",
        "source": "comment by angelofb",
        "interpretation": "Second user articulates the core use case: composited 2D scene graph with RenderNode hierarchy for transformations and effects."
      },
      {
        "text": "What do you see it being used for?",
        "source": "comment by mattleibow (maintainer)",
        "interpretation": "Maintainer is evaluating the use case before committing. Typical needs-design-input response. No rejection."
      }
    ],
    "rationale": "The title and content clearly express a request for new functionality (full SkSG bindings), not a bug. SkiaSharp.SceneGraph already exists as a package but only wraps InvalidationController. Expanding it requires adding a C API shim for the full RenderNode hierarchy in the Skia submodule, regenerating bindings, and writing C# wrappers — a medium-to-large effort. The maintainer is not opposed but wants use-case justification. The feature is valid and open; keeping it open for design discussion is appropriate.",
    "workarounds": [
      "Use SkiaSharp's SKCanvas drawing API directly with custom C# scene graph logic — this provides full control and avoids the overhead of wrapping SkSG's C++ object hierarchy.",
      "Use the existing InvalidationController from SkiaSharp.SceneGraph to track dirty regions when implementing a manual C# scene graph over SKCanvas.",
      "Reference the upstream SkSG C++ implementation (https://api.skia.org/namespacesksg.html) as a design pattern for implementing a pure-managed C# scene graph."
    ],
    "resolution": {
      "hypothesis": "SkSG bindings are missing because the SkSG module has no C API shim beyond InvalidationController — wrapping it requires substantial C API additions. A pure C# scene graph on top of SKCanvas may be a better user-facing design.",
      "proposals": [
        {
          "title": "Implement full SkSG C API and C# bindings",
          "description": "Add C API shim for SkSG RenderNode hierarchy (GroupNode, DrawNode, etc.) in externals/skia/src/c/, regenerate bindings, and write C# wrappers in SkiaSharp.SceneGraph. This is the direct path to fulfilling the request.",
          "category": "fix",
          "confidence": 0.65,
          "effort": "cost/xl",
          "validated": "untested"
        },
        {
          "title": "Build a C# scene graph over SKCanvas (workaround)",
          "description": "Implement a pure-managed C# scene graph using SKCanvas as the renderer, using the SkSG C++ source as a design reference. Avoids C API work entirely and gives better API ergonomics for C#.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/l",
          "validated": "untested"
        },
        {
          "title": "Use InvalidationController with manual SKCanvas scene management",
          "description": "Use the existing InvalidationController to track dirty regions, paired with custom SKCanvas drawing logic. This covers the most common scene-graph use case (selective re-rendering) with current APIs.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use InvalidationController with manual SKCanvas scene management",
      "recommendedReason": "Most practical near-term workaround with existing APIs. Full SkSG binding would need design work; a pure-managed C# approach is also worth considering but is more effort."
    },
    "nextQuestions": [
      "What specific SkSG node types are needed (RenderNode subclasses: GroupNode, DrawNode, Transform, etc.)?",
      "Would a pure C# scene graph implementation be preferable to native SkSG bindings?",
      "Is this for Lottie/animation use cases, or general-purpose UI scene composition?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.8,
      "reason": "Valid feature request with community interest. Maintainer is evaluating use cases. No blocking design decision yet. Keeping open allows the community to provide use-case feedback that could inform prioritization.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request and SkiaSharp labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the request, explain current state, and provide workaround",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the detailed question! Currently, the `SkiaSharp.SceneGraph` package only wraps `InvalidationController` from the SkSG module — the rest of the SkSG node hierarchy (RenderNode, GroupNode, DrawNode, Transform, etc.) has no C API shim, which is the layer required before C# bindings can be generated.\n\nAdding full SkSG bindings would require:\n1. Writing a C API shim for the SkSG RenderNode hierarchy in `externals/skia/src/c/`\n2. Regenerating P/Invoke bindings\n3. Writing C# wrapper classes for each node type\n\nThis is a non-trivial effort, and as one commenter noted, a pure-managed C# scene graph on top of `SKCanvas` may actually give better API ergonomics for .NET.\n\n**In the meantime**, here are some practical alternatives:\n- Use `InvalidationController` (already available in `SkiaSharp.SceneGraph`) to track dirty regions in your own rendering loop.\n- Implement your own C# scene graph using `SKCanvas` as the renderer — refer to the [upstream SkSG source](https://api.skia.org/namespacesksg.html) as a design reference.\n\nIf you'd like to contribute SkSG bindings, the `documentation/dev/adding-apis.md` guide explains the C API + binding generation workflow. A PR adding the C API shim would be a great starting point!"
      }
    ]
  }
}
```

</details>
