# Issue Triage Report — #4083

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-07-23T05:14:25Z |
| Type | type/feature-request (0.95 (95%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | keep-open (0.82 (82%)) |

**Issue Summary:** Reporter requests independent X/Y axis scaling in SKRotationScaleMatrix to support non-uniform per-sprite scaling when using SKCanvas.DrawAtlas, which currently only supports uniform scale via the SkRSXform data structure.

**Analysis:** SKRotationScaleMatrix wraps Skia's SkRSXform struct which encodes uniform rotation+scale as (fSCos=scale*cos, fSSin=scale*sin, fTX, fTY). The data structure is fundamentally constrained to uniform scaling — non-uniform X/Y scaling cannot be represented in SkRSXform without changes to Skia's drawAtlas API. Adding overloads with scaleX/scaleY that approximate non-uniform scaling would not map to the underlying C API without significant changes at the Skia/C-API level.

**Recommendations:** **keep-open** — Valid feature request. The underlying SkRSXform in Skia constrains uniform scale only; implementation requires upstream investigation or a creative C#-only workaround approach. Keep open for design discussion.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Perf | — |
| Partner | — |
| Current labels | type/feature-request |

## Evidence

### Reproduction

**Environment:** No specific platform mentioned; DrawAtlas is cross-platform

**Code snippets:**

```csharp
public static SKRotationScaleMatrix Create (float scaleX, float scaleY, float radians, float tx, float ty, float anchorX, float anchorY)
```

```csharp
public SKRotationScaleMatrix (float scosx, float scosy, float ssin, float tx, float ty)
```

## Analysis

### Technical Summary

SKRotationScaleMatrix wraps Skia's SkRSXform struct which encodes uniform rotation+scale as (fSCos=scale*cos, fSSin=scale*sin, fTX, fTY). The data structure is fundamentally constrained to uniform scaling — non-uniform X/Y scaling cannot be represented in SkRSXform without changes to Skia's drawAtlas API. Adding overloads with scaleX/scaleY that approximate non-uniform scaling would not map to the underlying C API without significant changes at the Skia/C-API level.

### Rationale

This is a valid feature request. The underlying Skia SkRSXform data structure only encodes uniform scale+rotation, making non-uniform per-sprite scaling in DrawAtlas impossible without upstream Skia changes or a new API pathway. The request is well-specified but blocked by the upstream constraint.

### Key Signals

- "SKRotationScaleMatrix currently only supports a single scale value, which is uniformly applied to both the x and y axes." — **issue body** (The reporter correctly identifies the constraint in the existing API.)
- "I am unaware of an alternative, apart from using a less performant method than DrawAtlas (e.g., DrawImage)." — **issue body** (Reporter is aware that DrawImage is a workaround but considers it less performant.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKRotationScaleMatrix.cs` | 13-51 | direct | SKRotationScaleMatrix has a single 'scale' parameter in all factory methods (Create, CreateDegrees, CreateScale). The struct fields are fSCos, fSSin, fTX, fTY — mathematically encoding uniform scale*cos(angle) and scale*sin(angle). The constructor takes (scos, ssin, tx, ty) with no separate scale axes. |
| `binding/SkiaSharp/SKCanvas.cs` | — | direct | DrawAtlas overloads accept SKRotationScaleMatrix[] transforms, directly mapping each entry to an SkRSXform in the C API. Any new overload in SKRotationScaleMatrix would still be passed as-is to the same C API, which only reads fSCos/fSSin/fTX/fTY. |

### Workarounds

- Use SKCanvas.DrawImage with explicit canvas Save/Scale/Translate/Restore calls for each sprite — this is slower but supports non-uniform scaling.
- Pre-scale the source sprite rectangles in the atlas image by the desired non-uniform ratio as a compile-time alternative.

### Next Questions

- Does the upstream Skia C++ drawAtlas or SkRSXform have any extension point for non-uniform scaling in newer Skia milestones?
- Is the performance difference between DrawAtlas and per-DrawImage approaches measurable in the reporter's use case?

### Resolution Proposals

**Hypothesis:** The feature is blocked by the underlying SkRSXform structure in Skia, which only supports uniform scale+rotation. The request is valid and well-specified but requires upstream Skia changes to SkRSXform or a new DrawAtlas overload using a different transform representation.

1. **Use DrawImage with canvas transforms** — workaround, cost/xs, validated=untested
   - For non-uniform scaling, use SKCanvas.Save(), apply a scale+rotate transform via SKCanvas.SetMatrix or concat, call DrawImage, then SKCanvas.Restore(). This avoids DrawAtlas but provides full transform control.
2. **Add scaleX/scaleY factory overloads that compute an approximate RSXform** — investigation, cost/m, validated=untested
   - Investigate whether adding Create(scaleX, scaleY, radians, tx, ty, anchorX, anchorY) is feasible as a SkiaSharp-only transformation that maps to standard SkRSXform (e.g., by applying non-uniform scale to the sprite source rect instead of the transform). This would be a pure C# addition with no C API changes.

**Recommended proposal:** Use DrawImage with canvas transforms

**Why:** The DrawImage workaround is immediately usable and provides the same non-uniform scaling capability. The upstream constraint means the feature request needs design discussion before any implementation can proceed.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.82 (82%) |
| Reason | Valid feature request. The underlying SkRSXform in Skia constrains uniform scale only; implementation requires upstream investigation or a creative C#-only workaround approach. Keep open for design discussion. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Add area/SkiaSharp label to the existing type/feature-request | labels=area/SkiaSharp |
| add-comment | medium | 0.82 (82%) | Explain the upstream constraint and provide a DrawImage workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the well-described feature request!

You're correct that `SKRotationScaleMatrix` (wrapping Skia's `SkRSXform`) only supports **uniform** scale — both axes are scaled by the same factor because the struct encodes rotation+scale as `(fSCos = scale·cos θ, fSSin = scale·sin θ)`. There's no room for independent scaleX/scaleY in the data structure.

This means `DrawAtlas` with non-uniform per-sprite scaling would require changes at the upstream Skia level, which is outside SkiaSharp's control.

**Workaround (available now):** Use `DrawImage` with canvas transforms:

```csharp
canvas.Save();
canvas.Translate(tx, ty);
canvas.RotateRadians(radians);
canvas.Scale(scaleX, scaleY);  // independent axes!
canvas.DrawImage(atlas, sourceRect, destRect, paint);
canvas.Restore();
```

This gives you full non-uniform scale control at the cost of per-sprite save/restore overhead.

We'll keep this open to track upstream Skia changes or a possible C#-only design that works around the `SkRSXform` constraint.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4083,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-07-23T05:14:25Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "Reporter requests independent X/Y axis scaling in SKRotationScaleMatrix to support non-uniform per-sprite scaling when using SKCanvas.DrawAtlas, which currently only supports uniform scale via the SkRSXform data structure.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    }
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "No specific platform mentioned; DrawAtlas is cross-platform",
      "codeSnippets": [
        "public static SKRotationScaleMatrix Create (float scaleX, float scaleY, float radians, float tx, float ty, float anchorX, float anchorY)",
        "public SKRotationScaleMatrix (float scosx, float scosy, float ssin, float tx, float ty)"
      ]
    }
  },
  "analysis": {
    "summary": "SKRotationScaleMatrix wraps Skia's SkRSXform struct which encodes uniform rotation+scale as (fSCos=scale*cos, fSSin=scale*sin, fTX, fTY). The data structure is fundamentally constrained to uniform scaling — non-uniform X/Y scaling cannot be represented in SkRSXform without changes to Skia's drawAtlas API. Adding overloads with scaleX/scaleY that approximate non-uniform scaling would not map to the underlying C API without significant changes at the Skia/C-API level.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKRotationScaleMatrix.cs",
        "finding": "SKRotationScaleMatrix has a single 'scale' parameter in all factory methods (Create, CreateDegrees, CreateScale). The struct fields are fSCos, fSSin, fTX, fTY — mathematically encoding uniform scale*cos(angle) and scale*sin(angle). The constructor takes (scos, ssin, tx, ty) with no separate scale axes.",
        "relevance": "direct",
        "lines": "13-51"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "finding": "DrawAtlas overloads accept SKRotationScaleMatrix[] transforms, directly mapping each entry to an SkRSXform in the C API. Any new overload in SKRotationScaleMatrix would still be passed as-is to the same C API, which only reads fSCos/fSSin/fTX/fTY.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "SKRotationScaleMatrix currently only supports a single scale value, which is uniformly applied to both the x and y axes.",
        "source": "issue body",
        "interpretation": "The reporter correctly identifies the constraint in the existing API."
      },
      {
        "text": "I am unaware of an alternative, apart from using a less performant method than DrawAtlas (e.g., DrawImage).",
        "source": "issue body",
        "interpretation": "Reporter is aware that DrawImage is a workaround but considers it less performant."
      }
    ],
    "rationale": "This is a valid feature request. The underlying Skia SkRSXform data structure only encodes uniform scale+rotation, making non-uniform per-sprite scaling in DrawAtlas impossible without upstream Skia changes or a new API pathway. The request is well-specified but blocked by the upstream constraint.",
    "workarounds": [
      "Use SKCanvas.DrawImage with explicit canvas Save/Scale/Translate/Restore calls for each sprite — this is slower but supports non-uniform scaling.",
      "Pre-scale the source sprite rectangles in the atlas image by the desired non-uniform ratio as a compile-time alternative."
    ],
    "nextQuestions": [
      "Does the upstream Skia C++ drawAtlas or SkRSXform have any extension point for non-uniform scaling in newer Skia milestones?",
      "Is the performance difference between DrawAtlas and per-DrawImage approaches measurable in the reporter's use case?"
    ],
    "resolution": {
      "hypothesis": "The feature is blocked by the underlying SkRSXform structure in Skia, which only supports uniform scale+rotation. The request is valid and well-specified but requires upstream Skia changes to SkRSXform or a new DrawAtlas overload using a different transform representation.",
      "proposals": [
        {
          "title": "Use DrawImage with canvas transforms",
          "category": "workaround",
          "description": "For non-uniform scaling, use SKCanvas.Save(), apply a scale+rotate transform via SKCanvas.SetMatrix or concat, call DrawImage, then SKCanvas.Restore(). This avoids DrawAtlas but provides full transform control.",
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Add scaleX/scaleY factory overloads that compute an approximate RSXform",
          "category": "investigation",
          "description": "Investigate whether adding Create(scaleX, scaleY, radians, tx, ty, anchorX, anchorY) is feasible as a SkiaSharp-only transformation that maps to standard SkRSXform (e.g., by applying non-uniform scale to the sprite source rect instead of the transform). This would be a pure C# addition with no C API changes.",
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use DrawImage with canvas transforms",
      "recommendedReason": "The DrawImage workaround is immediately usable and provides the same non-uniform scaling capability. The upstream constraint means the feature request needs design discussion before any implementation can proceed."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.82,
      "reason": "Valid feature request. The underlying SkRSXform in Skia constrains uniform scale only; implementation requires upstream investigation or a creative C#-only workaround approach. Keep open for design discussion.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Add area/SkiaSharp label to the existing type/feature-request",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain the upstream constraint and provide a DrawImage workaround",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the well-described feature request!\n\nYou're correct that `SKRotationScaleMatrix` (wrapping Skia's `SkRSXform`) only supports **uniform** scale — both axes are scaled by the same factor because the struct encodes rotation+scale as `(fSCos = scale·cos θ, fSSin = scale·sin θ)`. There's no room for independent scaleX/scaleY in the data structure.\n\nThis means `DrawAtlas` with non-uniform per-sprite scaling would require changes at the upstream Skia level, which is outside SkiaSharp's control.\n\n**Workaround (available now):** Use `DrawImage` with canvas transforms:\n\n```csharp\ncanvas.Save();\ncanvas.Translate(tx, ty);\ncanvas.RotateRadians(radians);\ncanvas.Scale(scaleX, scaleY);  // independent axes!\ncanvas.DrawImage(atlas, sourceRect, destRect, paint);\ncanvas.Restore();\n```\n\nThis gives you full non-uniform scale control at the cost of per-sprite save/restore overhead.\n\nWe'll keep this open to track upstream Skia changes or a possible C#-only design that works around the `SkRSXform` constraint."
      }
    ]
  }
}
```

</details>
