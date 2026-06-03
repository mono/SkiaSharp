# Issue Triage Report — #3943

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-06-03T05:51:42Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | needs-investigation (0.92 (92%)) |

**Issue Summary:** Feature request to expose SkCanvas::drawGlyphs() as SKCanvas.DrawGlyphs() in C#, enabling per-glyph positioning and RSXform-based transforms for advanced text effects not possible with the current TextBlob API.

**Analysis:** SKCanvas.DrawGlyphs() does not exist in SkiaSharp. The upstream Skia C++ API (SkCanvas::drawGlyphs) has three overloads supporting position-based and RSXform-based glyph rendering, but no C API shim or C# binding exists. All prerequisite types (SKFont, SKRotationScaleMatrix, SKPoint) are already wrapped. The issue provides a complete implementation proposal including C API names, C# method signatures, and effort estimate.

**Recommendations:** **needs-investigation** — Well-specified feature request with complete implementation plan assigned to an active milestone. Ready to route to api-add-review.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/feature-request, upgrading/4.x, cost/s, priority/2 |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3680 — Parent tracker: Evaluate and wrap new Skia APIs for v4

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 4.148.0-preview.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Issue is assigned to milestone 4.148.0-preview.1 and confirmed not yet implemented. |

## Analysis

### Technical Summary

SKCanvas.DrawGlyphs() does not exist in SkiaSharp. The upstream Skia C++ API (SkCanvas::drawGlyphs) has three overloads supporting position-based and RSXform-based glyph rendering, but no C API shim or C# binding exists. All prerequisite types (SKFont, SKRotationScaleMatrix, SKPoint) are already wrapped. The issue provides a complete implementation proposal including C API names, C# method signatures, and effort estimate.

### Rationale

This is unambiguously a type/feature-request: new functionality (DrawGlyphs) does not exist in any layer (C API shim or C# binding). The area is area/SkiaSharp because the change is to SKCanvas in the core binding. No platform-specific code is involved. The issue is part of tracker #3680 (API evaluation for Skia m147), is milestone-assigned, and has a clear 2-3 day implementation path described by the author.

### Key Signals

- "C++ ✅ 3 overloads in include/core/SkCanvas.h: position-based, clusters+text, RSXform-based. C API ❌ No sk_canvas_draw_glyphs. C# ❌ Not exposed." — **issue body** (Author has done the feasibility analysis; all prerequisite types exist, only C API shim and C# wrapper are missing.)
- "Part of #3680" — **issue body** (Part of the coordinated API evaluation tracker for Skia m147 milestone — maintainer-initiated, not a random community request.)
- "Estimated effort ~2-3 days including C API shim work and tests." — **issue body** (Author (mattleibow, a contributor) provides a well-scoped estimate, indicating this is implementable in short order.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 617-680 | direct | DrawText overloads (string and SKTextBlob) exist but no DrawGlyphs method is present. No per-glyph positioning or RSXform variant exists. |
| `binding/SkiaSharp/SKFont.cs` | 116-149 | related | GetGlyph() and GetGlyphs() exist and return ushort[] or Span<ushort> glyph IDs. These are the glyph values that DrawGlyphs would consume. |
| `binding/SkiaSharp/SKRotationScaleMatrix.cs` | 7-50 | related | SKRotationScaleMatrix is fully implemented with factory methods (CreateRotation, CreateTranslation, CreateScale, etc.). Required by the RSXform DrawGlyphs overload. |

### Resolution Proposals

**Hypothesis:** Add sk_canvas_draw_glyphs and sk_canvas_draw_glyphs_rsxform to the C API shim, regenerate P/Invoke bindings, and add two C# overloads on SKCanvas with ReadOnlySpan parameters.

1. **Implement via api-add-review workflow** — fix, confidence 0.92 (92%), cost/s, validated=untested
   - Follow the standard API addition workflow: add C API functions to externals/skia/src/c/sk_canvas.cpp and externals/skia/include/c/sk_canvas.h, regenerate bindings, add C# overloads to SKCanvas with ReadOnlySpan<ushort> glyphs + ReadOnlySpan<SKPoint> positions and ReadOnlySpan<SKRotationScaleMatrix> xforms variants.
2. **Workaround via SKTextBlobBuilder** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - Users needing per-glyph positioning today can use SKTextBlobBuilder.AllocatePositionedRun() to place each glyph individually, then call DrawText(blob, ...). This is more ceremony but achieves similar positioning results without RSXform rotation.

**Recommended proposal:** Implement via api-add-review workflow

**Why:** The feature has a clear implementation path, is milestone-assigned, and all prerequisite types already exist. The workaround lacks RSXform support.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.92 (92%) |
| Reason | Well-specified feature request with complete implementation plan assigned to an active milestone. Ready to route to api-add-review. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply feature-request, SkiaSharp area, and compatibility tenet labels | labels=type/feature-request, area/SkiaSharp, tenet/compatibility |
| add-comment | medium | 0.88 (88%) | Acknowledge the request and outline the implementation path via api-add-review | — |
| link-related | low | 0.95 (95%) | Cross-reference parent tracker issue #3680 | linkedIssue=#3680 |

**Comment draft for `add-comment`:**

```markdown
Thanks for this well-scoped proposal! ✅

All prerequisite types are already in place (`SKFont`, `SKRotationScaleMatrix`, `SKPoint`). The work involves:

1. Add `sk_canvas_draw_glyphs` + `sk_canvas_draw_glyphs_rsxform` to the C API shim
2. Regenerate P/Invoke bindings
3. Add `DrawGlyphs` overloads to `SKCanvas` with `ReadOnlySpan` parameters
4. Tests with known glyph positions

In the meantime, `SKTextBlobBuilder.AllocatePositionedRun()` can be used for per-glyph position placement (though RSXform rotation isn't available via that path).

Tracked as part of #3680.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3943,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-06-03T05:51:42Z",
    "currentLabels": [
      "type/feature-request",
      "upgrading/4.x",
      "cost/s",
      "priority/2"
    ]
  },
  "summary": "Feature request to expose SkCanvas::drawGlyphs() as SKCanvas.DrawGlyphs() in C#, enabling per-glyph positioning and RSXform-based transforms for advanced text effects not possible with the current TextBlob API.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3680",
          "description": "Parent tracker: Evaluate and wrap new Skia APIs for v4"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "4.148.0-preview.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Issue is assigned to milestone 4.148.0-preview.1 and confirmed not yet implemented."
    }
  },
  "analysis": {
    "summary": "SKCanvas.DrawGlyphs() does not exist in SkiaSharp. The upstream Skia C++ API (SkCanvas::drawGlyphs) has three overloads supporting position-based and RSXform-based glyph rendering, but no C API shim or C# binding exists. All prerequisite types (SKFont, SKRotationScaleMatrix, SKPoint) are already wrapped. The issue provides a complete implementation proposal including C API names, C# method signatures, and effort estimate.",
    "rationale": "This is unambiguously a type/feature-request: new functionality (DrawGlyphs) does not exist in any layer (C API shim or C# binding). The area is area/SkiaSharp because the change is to SKCanvas in the core binding. No platform-specific code is involved. The issue is part of tracker #3680 (API evaluation for Skia m147), is milestone-assigned, and has a clear 2-3 day implementation path described by the author.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "617-680",
        "finding": "DrawText overloads (string and SKTextBlob) exist but no DrawGlyphs method is present. No per-glyph positioning or RSXform variant exists.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFont.cs",
        "lines": "116-149",
        "finding": "GetGlyph() and GetGlyphs() exist and return ushort[] or Span<ushort> glyph IDs. These are the glyph values that DrawGlyphs would consume.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKRotationScaleMatrix.cs",
        "lines": "7-50",
        "finding": "SKRotationScaleMatrix is fully implemented with factory methods (CreateRotation, CreateTranslation, CreateScale, etc.). Required by the RSXform DrawGlyphs overload.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "C++ ✅ 3 overloads in include/core/SkCanvas.h: position-based, clusters+text, RSXform-based. C API ❌ No sk_canvas_draw_glyphs. C# ❌ Not exposed.",
        "source": "issue body",
        "interpretation": "Author has done the feasibility analysis; all prerequisite types exist, only C API shim and C# wrapper are missing."
      },
      {
        "text": "Part of #3680",
        "source": "issue body",
        "interpretation": "Part of the coordinated API evaluation tracker for Skia m147 milestone — maintainer-initiated, not a random community request."
      },
      {
        "text": "Estimated effort ~2-3 days including C API shim work and tests.",
        "source": "issue body",
        "interpretation": "Author (mattleibow, a contributor) provides a well-scoped estimate, indicating this is implementable in short order."
      }
    ],
    "resolution": {
      "hypothesis": "Add sk_canvas_draw_glyphs and sk_canvas_draw_glyphs_rsxform to the C API shim, regenerate P/Invoke bindings, and add two C# overloads on SKCanvas with ReadOnlySpan parameters.",
      "proposals": [
        {
          "title": "Implement via api-add-review workflow",
          "description": "Follow the standard API addition workflow: add C API functions to externals/skia/src/c/sk_canvas.cpp and externals/skia/include/c/sk_canvas.h, regenerate bindings, add C# overloads to SKCanvas with ReadOnlySpan<ushort> glyphs + ReadOnlySpan<SKPoint> positions and ReadOnlySpan<SKRotationScaleMatrix> xforms variants.",
          "category": "fix",
          "confidence": 0.92,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Workaround via SKTextBlobBuilder",
          "description": "Users needing per-glyph positioning today can use SKTextBlobBuilder.AllocatePositionedRun() to place each glyph individually, then call DrawText(blob, ...). This is more ceremony but achieves similar positioning results without RSXform rotation.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Implement via api-add-review workflow",
      "recommendedReason": "The feature has a clear implementation path, is milestone-assigned, and all prerequisite types already exist. The workaround lacks RSXform support."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.92,
      "reason": "Well-specified feature request with complete implementation plan assigned to an active milestone. Ready to route to api-add-review.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, SkiaSharp area, and compatibility tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the request and outline the implementation path via api-add-review",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for this well-scoped proposal! ✅\n\nAll prerequisite types are already in place (`SKFont`, `SKRotationScaleMatrix`, `SKPoint`). The work involves:\n\n1. Add `sk_canvas_draw_glyphs` + `sk_canvas_draw_glyphs_rsxform` to the C API shim\n2. Regenerate P/Invoke bindings\n3. Add `DrawGlyphs` overloads to `SKCanvas` with `ReadOnlySpan` parameters\n4. Tests with known glyph positions\n\nIn the meantime, `SKTextBlobBuilder.AllocatePositionedRun()` can be used for per-glyph position placement (though RSXform rotation isn't available via that path).\n\nTracked as part of #3680."
      },
      {
        "type": "link-related",
        "description": "Cross-reference parent tracker issue #3680",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 3680
      }
    ]
  }
}
```

</details>
