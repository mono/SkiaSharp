# Issue Triage Report — #2373

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-29T19:40:00Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | keep-open (0.92 (92%)) |

**Issue Summary:** Feature request to expose SkImageFilters::RuntimeShader() as SKImageFilter.CreateRuntimeShader(), enabling custom SkSL shaders to receive the filtered image as input and participate in the image filter composition graph.

**Analysis:** Reporter requests exposure of SkImageFilters::RuntimeShader() so custom SkSL shaders can receive layer content as input and process it per-pixel as an image filter. The existing SKImageFilter.CreateShader(SKShader) is a leaf filter that does not receive the filtered image. The requested API would enable SaveLayer + custom GPU shader effects. This feature has not yet been implemented; the formal spec lives in issue #3776 (part of tracking issue #3680).

**Recommendations:** **keep-open** — Valid feature request with 10 upvotes. Upstream API exists in Skia m98+. Formal spec tracked in #3776 (part of #3680). Not yet implemented. Should remain open and linked to the tracking issue.

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

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3137 — Duplicate feature request: Add SkImageFilters::RuntimeShader (filed 2025-01-09, triaged)
- https://github.com/mono/SkiaSharp/issues/3776 — Formal spec by maintainer: Expose SkImageFilters::RuntimeShader as SKImageFilter.CreateRuntimeShader, sub-issue of #3680
- https://groups.google.com/g/skia-discuss/c/uc-5b4Hy4YU — Skia discussion thread referenced by reporter

**Code snippets:**

```csharp
var effect = SKRuntimeEffect.Create("...", out string errors);
SKShader shader = effect.ToShader(false);
var imageFilter = SKImageFilter.CreateShader(shader, ...);
var paint = new SKPaint { ImageFilter = imageFilter };
canvas.SaveLayer(paint);
// draw stuff
canvas.Restore();
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | No implementation of CreateRuntimeShader has been added to SKImageFilter.cs. The upstream SkImageFilters::RuntimeShader() C++ API is available in Skia m98+. |

## Analysis

### Technical Summary

Reporter requests exposure of SkImageFilters::RuntimeShader() so custom SkSL shaders can receive layer content as input and process it per-pixel as an image filter. The existing SKImageFilter.CreateShader(SKShader) is a leaf filter that does not receive the filtered image. The requested API would enable SaveLayer + custom GPU shader effects. This feature has not yet been implemented; the formal spec lives in issue #3776 (part of tracking issue #3680).

### Rationale

This is a clear API addition request. SKRuntimeShaderBuilder already exists in the C# bindings, and SkImageFilters::RuntimeShader() exists upstream. The missing piece is a C API shim and C# wrapper method. Issue #3776 (by maintainer) is the canonical formal spec for this feature, and #3137 is a later duplicate. Issue #2373 is the original community report with 10 upvotes.

### Key Signals

- "I found out about SkImageFilters::RuntimeShader() but it seems like it's not yet exposed in SkiaSharp" — **issue body** (Reporter correctly identifies the missing API — SkImageFilters::RuntimeShader() is not wrapped in SkiaSharp.)
- "I would like the API to expose a SKImageFilter.CreateShader() or SKImageFilter.CreateRuntimeShader() method" — **issue body** (Clear, specific API request with correct naming.)
- "+1 reactions: 10" — **issue metadata** (High community interest — 10 upvotes confirms demand for this feature.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKImageFilter.cs` | — | direct | CreateShader(SKShader shader, bool dither, SKRect* cropRect) exists as a leaf image filter — it wraps a static shader but does NOT accept the filtered image as input. No CreateRuntimeShader method exists. |
| `binding/SkiaSharp/SKRuntimeEffect.cs` | — | related | SKRuntimeShaderBuilder class exists and is fully implemented. BuildShader() factory and ToShader() are present, confirming the runtime shader infrastructure is ready. A CreateRuntimeShader overload taking SKRuntimeShaderBuilder would integrate naturally. |

### Workarounds

- Use SKImageFilter.CreateShader(shader) with a pre-baked shader — this does NOT receive the filtered image but can apply simple effects to the canvas directly.
- Chain built-in image filters (blur, color filter, etc.) via SKImageFilter — limited to predefined operations but available today.

### Next Questions

- Does the current Skia milestone in use include the SkImageFilters::RuntimeShader C++ API?
- Is a maxSampleRadius overload needed at the same time (for shaders that read neighboring pixels)?

### Resolution Proposals

**Hypothesis:** The upstream C++ API exists in Skia m98+. The implementation path requires: (1) a C API shim in externals/skia/src/c/sk_imagefilter.cpp, (2) a C# P/Invoke declaration, and (3) SKImageFilter.CreateRuntimeShader() wrapper methods.

1. **Follow formal spec in #3776** — fix, confidence 0.90 (90%), cost/m, validated=untested
   - The maintainer has created a formal spec in issue #3776 (part of #3680). The implementation would add SKImageFilter.CreateRuntimeShader(SKRuntimeShaderBuilder, string, SKImageFilter?) and a maxSampleRadius overload. This issue should be linked to #3776.
2. **Partial workaround using CreateShader** — workaround, confidence 0.85 (85%), cost/xs, validated=untested
   - For effects that do not need the filtered image as input, SKImageFilter.CreateShader(shader) already works for applying a runtime shader as a leaf filter.

**Recommended proposal:** Follow formal spec in #3776

**Why:** The maintainer has already formalized this as a trackable work item in #3776 under #3680. Implementation path is clear.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.92 (92%) |
| Reason | Valid feature request with 10 upvotes. Upstream API exists in Skia m98+. Formal spec tracked in #3776 (part of #3680). Not yet implemented. Should remain open and linked to the tracking issue. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply feature-request and SkiaSharp area labels | labels=type/feature-request, area/SkiaSharp |
| link-related | low | 0.90 (90%) | Link to formal spec issue #3776 and duplicate #3137 | linkedIssue=#3776 |
| add-comment | medium | 0.88 (88%) | Acknowledge request, note partial workaround, and point to tracking issue #3776 | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the feature request! `SKImageFilter.CreateRuntimeShader()` (wrapping `SkImageFilters::RuntimeShader()`) is not yet exposed in SkiaSharp, but this is a known gap that's now formally tracked in #3776 as part of the broader API work in #3680.

**Partial workaround today:** The existing `SKImageFilter.CreateShader(SKShader)` can apply a runtime shader as a *leaf* image filter (i.e., the shader generates output from scratch, not from the layer content). If your effect doesn't need to read the filtered image as input, this works:

```csharp
var builder = SKRuntimeEffect.BuildShader(sksl);
// set uniforms on builder...
var imageFilter = SKImageFilter.CreateShader(builder.Build());
```

If you need the layer content as input (the full `SkImageFilters::RuntimeShader` behavior), that requires the new API and cannot be simulated today.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2373,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-29T19:40:00Z"
  },
  "summary": "Feature request to expose SkImageFilters::RuntimeShader() as SKImageFilter.CreateRuntimeShader(), enabling custom SkSL shaders to receive the filtered image as input and participate in the image filter composition graph.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    }
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "var effect = SKRuntimeEffect.Create(\"...\", out string errors);\nSKShader shader = effect.ToShader(false);\nvar imageFilter = SKImageFilter.CreateShader(shader, ...);\nvar paint = new SKPaint { ImageFilter = imageFilter };\ncanvas.SaveLayer(paint);\n// draw stuff\ncanvas.Restore();"
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3137",
          "description": "Duplicate feature request: Add SkImageFilters::RuntimeShader (filed 2025-01-09, triaged)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3776",
          "description": "Formal spec by maintainer: Expose SkImageFilters::RuntimeShader as SKImageFilter.CreateRuntimeShader, sub-issue of #3680"
        },
        {
          "url": "https://groups.google.com/g/skia-discuss/c/uc-5b4Hy4YU",
          "description": "Skia discussion thread referenced by reporter"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "No implementation of CreateRuntimeShader has been added to SKImageFilter.cs. The upstream SkImageFilters::RuntimeShader() C++ API is available in Skia m98+."
    }
  },
  "analysis": {
    "summary": "Reporter requests exposure of SkImageFilters::RuntimeShader() so custom SkSL shaders can receive layer content as input and process it per-pixel as an image filter. The existing SKImageFilter.CreateShader(SKShader) is a leaf filter that does not receive the filtered image. The requested API would enable SaveLayer + custom GPU shader effects. This feature has not yet been implemented; the formal spec lives in issue #3776 (part of tracking issue #3680).",
    "rationale": "This is a clear API addition request. SKRuntimeShaderBuilder already exists in the C# bindings, and SkImageFilters::RuntimeShader() exists upstream. The missing piece is a C API shim and C# wrapper method. Issue #3776 (by maintainer) is the canonical formal spec for this feature, and #3137 is a later duplicate. Issue #2373 is the original community report with 10 upvotes.",
    "keySignals": [
      {
        "text": "I found out about SkImageFilters::RuntimeShader() but it seems like it's not yet exposed in SkiaSharp",
        "source": "issue body",
        "interpretation": "Reporter correctly identifies the missing API — SkImageFilters::RuntimeShader() is not wrapped in SkiaSharp."
      },
      {
        "text": "I would like the API to expose a SKImageFilter.CreateShader() or SKImageFilter.CreateRuntimeShader() method",
        "source": "issue body",
        "interpretation": "Clear, specific API request with correct naming."
      },
      {
        "text": "+1 reactions: 10",
        "source": "issue metadata",
        "interpretation": "High community interest — 10 upvotes confirms demand for this feature."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKImageFilter.cs",
        "finding": "CreateShader(SKShader shader, bool dither, SKRect* cropRect) exists as a leaf image filter — it wraps a static shader but does NOT accept the filtered image as input. No CreateRuntimeShader method exists.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKRuntimeEffect.cs",
        "finding": "SKRuntimeShaderBuilder class exists and is fully implemented. BuildShader() factory and ToShader() are present, confirming the runtime shader infrastructure is ready. A CreateRuntimeShader overload taking SKRuntimeShaderBuilder would integrate naturally.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use SKImageFilter.CreateShader(shader) with a pre-baked shader — this does NOT receive the filtered image but can apply simple effects to the canvas directly.",
      "Chain built-in image filters (blur, color filter, etc.) via SKImageFilter — limited to predefined operations but available today."
    ],
    "nextQuestions": [
      "Does the current Skia milestone in use include the SkImageFilters::RuntimeShader C++ API?",
      "Is a maxSampleRadius overload needed at the same time (for shaders that read neighboring pixels)?"
    ],
    "resolution": {
      "hypothesis": "The upstream C++ API exists in Skia m98+. The implementation path requires: (1) a C API shim in externals/skia/src/c/sk_imagefilter.cpp, (2) a C# P/Invoke declaration, and (3) SKImageFilter.CreateRuntimeShader() wrapper methods.",
      "proposals": [
        {
          "title": "Follow formal spec in #3776",
          "description": "The maintainer has created a formal spec in issue #3776 (part of #3680). The implementation would add SKImageFilter.CreateRuntimeShader(SKRuntimeShaderBuilder, string, SKImageFilter?) and a maxSampleRadius overload. This issue should be linked to #3776.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Partial workaround using CreateShader",
          "description": "For effects that do not need the filtered image as input, SKImageFilter.CreateShader(shader) already works for applying a runtime shader as a leaf filter.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Follow formal spec in #3776",
      "recommendedReason": "The maintainer has already formalized this as a trackable work item in #3776 under #3680. Implementation path is clear."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.92,
      "reason": "Valid feature request with 10 upvotes. Upstream API exists in Skia m98+. Formal spec tracked in #3776 (part of #3680). Not yet implemented. Should remain open and linked to the tracking issue.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request and SkiaSharp area labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "link-related",
        "description": "Link to formal spec issue #3776 and duplicate #3137",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 3776
      },
      {
        "type": "add-comment",
        "description": "Acknowledge request, note partial workaround, and point to tracking issue #3776",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the feature request! `SKImageFilter.CreateRuntimeShader()` (wrapping `SkImageFilters::RuntimeShader()`) is not yet exposed in SkiaSharp, but this is a known gap that's now formally tracked in #3776 as part of the broader API work in #3680.\n\n**Partial workaround today:** The existing `SKImageFilter.CreateShader(SKShader)` can apply a runtime shader as a *leaf* image filter (i.e., the shader generates output from scratch, not from the layer content). If your effect doesn't need to read the filtered image as input, this works:\n\n```csharp\nvar builder = SKRuntimeEffect.BuildShader(sksl);\n// set uniforms on builder...\nvar imageFilter = SKImageFilter.CreateShader(builder.Build());\n```\n\nIf you need the layer content as input (the full `SkImageFilters::RuntimeShader` behavior), that requires the new API and cannot be simulated today."
      }
    ]
  }
}
```

</details>
