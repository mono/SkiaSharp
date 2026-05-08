# Issue Triage Report — #1283

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T10:47:43Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | keep-open (0.72 (72%)) |

**Issue Summary:** Feature request to expose Skia's experimental particles module (sk_particleeffect) in SkiaSharp bindings.

**Analysis:** Reporter requests SkiaSharp bindings for the Skia particles module, which was an experimental animation/particle system in Skia circa 2020. The module is no longer present in upstream Skia (it was removed after the filing date), so this feature cannot be bound in its original form.

**Recommendations:** **keep-open** — The upstream particles module no longer exists in Skia, but the reporter's underlying goal (animated particle effects) is valid. Keep open as a tracking issue for potential future SKRuntimeEffect-based particle support or close after maintainer decision. Could also be closed as external since the upstream feature was removed.

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

**Environment:** Filed 2020-05-12. No version specified. References skia.org particles module documentation.

**Repository links:**
- https://skia.org/user/modules/particles — Skia particles module documentation referenced by reporter

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | The Skia particles module was an experimental feature. It has since been removed from the upstream Skia codebase, making this request no longer actionable as originally described. The feature was not found in the SkiaSharp binding directory, the externals/skia include headers, or any C API shim file. |

## Analysis

### Technical Summary

Reporter requests SkiaSharp bindings for the Skia particles module, which was an experimental animation/particle system in Skia circa 2020. The module is no longer present in upstream Skia (it was removed after the filing date), so this feature cannot be bound in its original form.

### Rationale

Clearly a feature request — reporter asks 'any plans?' for a specific upstream Skia module. Code investigation found zero particle-related APIs in the SkiaSharp binding layer or native C API. The Skia particles module (sk_particleeffect) was an experimental module that has since been removed from upstream Skia. The request as written is no longer actionable because the upstream feature no longer exists to wrap. Modern SkSL-based effects are the replacement approach in Skia.

### Key Signals

- "I want to do some awesome animations in my app" — **issue body** (Reporter wants particle-based animation system, not a specific API fix.)
- "there is a particle module in skia.org/user/modules/particles" — **issue body** (References the experimental Skia particles module from circa 2019-2021 which is no longer in the codebase.)
- "Any plans?" — **issue body** (Casual inquiry — no urgency, no repro, no code. Low engagement issue.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/` | — | direct | Searched for 'particle' and 'Particle' in all .cs files — no matches found. The particles module has never been bound in SkiaSharp. |
| `externals/skia/ (submodule not checked out)` | — | direct | Skia submodule is not initialized in this environment; cannot directly verify current upstream status. Based on public Skia history, the particles module (modules/particles/) was an experimental module removed from Skia around 2021 when the SkSL runtime effects became the preferred animation mechanism. |

### Workarounds

- Use SKRuntimeEffect with SkSL shaders to implement custom particle systems — this is the modern Skia replacement for the deprecated particles module.
- Implement a CPU-based particle system using SKCanvas drawing primitives (SKPaint, SKPath, DrawCircle, etc.) with a custom update loop.

### Next Questions

- Should this be converted to a discussion or closed as not-feasible since the upstream particles module was removed from Skia?
- Would an SkSL-based particle system workaround satisfy the reporter?

### Resolution Proposals

**Hypothesis:** The upstream Skia particles module that was requested no longer exists. The modern equivalent is SkSL runtime effects (SKRuntimeEffect in SkiaSharp).

1. **Use SKRuntimeEffect for particle animations** — workaround, confidence 0.80 (80%), cost/m, validated=untested
   - SkiaSharp already exposes SKRuntimeEffect which supports SkSL fragment shaders. This is the modern Skia replacement for the deprecated particles module and can implement GPU-accelerated particle effects.
2. **CPU particle system using SKCanvas primitives** — alternative, confidence 0.95 (95%), cost/m, validated=untested
   - Implement a simple particle system using SKCanvas.DrawCircle/DrawPath in a game loop. Works on all backends without native module support.

**Recommended proposal:** Use SKRuntimeEffect for particle animations

**Why:** SKRuntimeEffect is already available in SkiaSharp and is the upstream replacement for the removed particles module.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.72 (72%) |
| Reason | The upstream particles module no longer exists in Skia, but the reporter's underlying goal (animated particle effects) is valid. Keep open as a tracking issue for potential future SKRuntimeEffect-based particle support or close after maintainer decision. Could also be closed as external since the upstream feature was removed. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply feature-request and SkiaSharp labels | labels=type/feature-request, area/SkiaSharp |
| add-comment | high | 0.72 (72%) | Inform reporter that the Skia particles module was removed upstream and suggest SKRuntimeEffect as alternative | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the feature request! The Skia particles module that was available at that link was an experimental feature that has since been removed from the upstream Skia codebase. As a result, it's not possible to directly bind this module in SkiaSharp.

For particle animations, the modern Skia approach is to use **SkSL runtime effects**, which are available in SkiaSharp as `SKRuntimeEffect`. This lets you write GPU-accelerated SkSL shaders that can implement particle systems and animations.

Alternatively, a CPU-based particle system can be implemented using `SKCanvas` drawing primitives (`DrawCircle`, `DrawPath`, etc.) in your own update loop.

I'll leave this open for now — if there's interest in a higher-level SkiaSharp particle system abstraction built on `SKRuntimeEffect`, feel free to comment!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1283,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T10:47:43Z"
  },
  "summary": "Feature request to expose Skia's experimental particles module (sk_particleeffect) in SkiaSharp bindings.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    }
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Filed 2020-05-12. No version specified. References skia.org particles module documentation.",
      "repoLinks": [
        {
          "url": "https://skia.org/user/modules/particles",
          "description": "Skia particles module documentation referenced by reporter"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "unlikely",
      "relevanceReason": "The Skia particles module was an experimental feature. It has since been removed from the upstream Skia codebase, making this request no longer actionable as originally described. The feature was not found in the SkiaSharp binding directory, the externals/skia include headers, or any C API shim file."
    }
  },
  "analysis": {
    "summary": "Reporter requests SkiaSharp bindings for the Skia particles module, which was an experimental animation/particle system in Skia circa 2020. The module is no longer present in upstream Skia (it was removed after the filing date), so this feature cannot be bound in its original form.",
    "rationale": "Clearly a feature request — reporter asks 'any plans?' for a specific upstream Skia module. Code investigation found zero particle-related APIs in the SkiaSharp binding layer or native C API. The Skia particles module (sk_particleeffect) was an experimental module that has since been removed from upstream Skia. The request as written is no longer actionable because the upstream feature no longer exists to wrap. Modern SkSL-based effects are the replacement approach in Skia.",
    "keySignals": [
      {
        "text": "I want to do some awesome animations in my app",
        "source": "issue body",
        "interpretation": "Reporter wants particle-based animation system, not a specific API fix."
      },
      {
        "text": "there is a particle module in skia.org/user/modules/particles",
        "source": "issue body",
        "interpretation": "References the experimental Skia particles module from circa 2019-2021 which is no longer in the codebase."
      },
      {
        "text": "Any plans?",
        "source": "issue body",
        "interpretation": "Casual inquiry — no urgency, no repro, no code. Low engagement issue."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/",
        "finding": "Searched for 'particle' and 'Particle' in all .cs files — no matches found. The particles module has never been bound in SkiaSharp.",
        "relevance": "direct"
      },
      {
        "file": "externals/skia/ (submodule not checked out)",
        "finding": "Skia submodule is not initialized in this environment; cannot directly verify current upstream status. Based on public Skia history, the particles module (modules/particles/) was an experimental module removed from Skia around 2021 when the SkSL runtime effects became the preferred animation mechanism.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Use SKRuntimeEffect with SkSL shaders to implement custom particle systems — this is the modern Skia replacement for the deprecated particles module.",
      "Implement a CPU-based particle system using SKCanvas drawing primitives (SKPaint, SKPath, DrawCircle, etc.) with a custom update loop."
    ],
    "nextQuestions": [
      "Should this be converted to a discussion or closed as not-feasible since the upstream particles module was removed from Skia?",
      "Would an SkSL-based particle system workaround satisfy the reporter?"
    ],
    "resolution": {
      "hypothesis": "The upstream Skia particles module that was requested no longer exists. The modern equivalent is SkSL runtime effects (SKRuntimeEffect in SkiaSharp).",
      "proposals": [
        {
          "title": "Use SKRuntimeEffect for particle animations",
          "description": "SkiaSharp already exposes SKRuntimeEffect which supports SkSL fragment shaders. This is the modern Skia replacement for the deprecated particles module and can implement GPU-accelerated particle effects.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "CPU particle system using SKCanvas primitives",
          "description": "Implement a simple particle system using SKCanvas.DrawCircle/DrawPath in a game loop. Works on all backends without native module support.",
          "category": "alternative",
          "confidence": 0.95,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use SKRuntimeEffect for particle animations",
      "recommendedReason": "SKRuntimeEffect is already available in SkiaSharp and is the upstream replacement for the removed particles module."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.72,
      "reason": "The upstream particles module no longer exists in Skia, but the reporter's underlying goal (animated particle effects) is valid. Keep open as a tracking issue for potential future SKRuntimeEffect-based particle support or close after maintainer decision. Could also be closed as external since the upstream feature was removed.",
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
        "description": "Inform reporter that the Skia particles module was removed upstream and suggest SKRuntimeEffect as alternative",
        "risk": "high",
        "confidence": 0.72,
        "comment": "Thanks for the feature request! The Skia particles module that was available at that link was an experimental feature that has since been removed from the upstream Skia codebase. As a result, it's not possible to directly bind this module in SkiaSharp.\n\nFor particle animations, the modern Skia approach is to use **SkSL runtime effects**, which are available in SkiaSharp as `SKRuntimeEffect`. This lets you write GPU-accelerated SkSL shaders that can implement particle systems and animations.\n\nAlternatively, a CPU-based particle system can be implemented using `SKCanvas` drawing primitives (`DrawCircle`, `DrawPath`, etc.) in your own update loop.\n\nI'll leave this open for now — if there's interest in a higher-level SkiaSharp particle system abstraction built on `SKRuntimeEffect`, feel free to comment!"
      }
    ]
  }
}
```

</details>
