# Issue Triage Report — #4273

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-07-27T05:30:00Z |
| Type | type/feature-request (0.98 (98%)) |
| Area | area/SkiaSharp.HarfBuzz (0.97 (97%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** Request to expose HarfBuzz OpenType feature parameters (e.g. vert, smcp, liga) through new overloads of SKShaper.Shape and CanvasExtensions.DrawShapedText so callers do not need to bypass SKShaper and re-implement HarfBuzz buffer/font setup directly.

**Analysis:** SKShaper.Shape internally calls hbFont.Shape(buffer) with no feature arguments. HarfBuzzSharp.Font already has Font.Shape(Buffer, IReadOnlyList<Feature>, IReadOnlyList<string>) but SKShaper does not expose it. Adding new overloads with an optional IReadOnlyList<Feature> parameter would be a non-breaking, ABI-safe change following the existing overload pattern.

**Recommendations:** **needs-investigation** — Feature request is well-specified with a proposed implementation. Needs API design review before work begins — specifically whether to also surface the shapers parameter and which overload signatures to add.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp.HarfBuzz |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Perf | — |
| Partner | — |
| Current labels | type/feature-request |

## Evidence

### Reproduction

1. Create a SKShaper with a typeface that has OpenType features (e.g. CJK vert, small caps).
2. Call SKShaper.Shape or DrawShapedText — no way to pass HarfBuzz Feature[] parameters.
3. Observe that OpenType features cannot be activated without bypassing SKShaper entirely.

**Environment:** SkiaSharp.HarfBuzz – no version specified; applies to current main branch

**Code snippets:**

```csharp
var vert = new[] { new Feature(new Tag('v','e','r','t'), 1) };
_canvas.DrawShapedText(shaper, text, SKPoint.Empty, SKTextAlign.Left, skFont, paint, vert);
```

```csharp
hbFont.Shape(buffer, features, null); // already exists in HarfBuzzSharp.Font but not surfaced
```

## Analysis

### Technical Summary

SKShaper.Shape internally calls hbFont.Shape(buffer) with no feature arguments. HarfBuzzSharp.Font already has Font.Shape(Buffer, IReadOnlyList<Feature>, IReadOnlyList<string>) but SKShaper does not expose it. Adding new overloads with an optional IReadOnlyList<Feature> parameter would be a non-breaking, ABI-safe change following the existing overload pattern.

### Rationale

This is a clear feature request for a well-scoped API gap. The lower-level HarfBuzz API already supports feature lists; SKShaper simply needs new overloads that forward them. The reporter has provided a detailed, plausible implementation sketch. No bugs are present — the existing behavior is correct, it just lacks an extension point.

### Key Signals

- "HarfBuzzSharp.Font.Shape(Buffer, params Feature[]) already supports this; we just don't surface it." — **issue body** (The required infrastructure exists; only the public SKShaper wrapper needs new overloads.)
- "consumers... currently have to bypass the wrapper and re-implement buffer/font setup against HarfBuzzSharp directly" — **issue body** (Valid API gap — users are forced into duplicating internal setup to access a feature the lower layer already exposes.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/SKShaper.cs` | 67 | direct | hbFont.Shape(buffer) is called with no features. The private hbFont field is of type HarfBuzzSharp.Font which already supports Shape(Buffer, IReadOnlyList<Feature>, IReadOnlyList<string>). |
| `source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/CanvasExtensions.cs` | 69 | direct | DrawShapedText calls shaper.Shape(text, x, y, font) with no features and has no overload accepting Feature[] or IReadOnlyList<Feature>. |
| `binding/HarfBuzzSharp/Font.cs` | — | direct | Font.Shape(Buffer buffer, IReadOnlyList<Feature> features, IReadOnlyList<string> shapers) already exists in HarfBuzzSharp, confirming the lower-level API is available and just needs surfacing through SKShaper. |

### Next Questions

- Should the shapers (second parameter to Font.Shape) also be exposed, or just features?
- Should a params Feature[] convenience overload be provided alongside IReadOnlyList<Feature>?

### Resolution Proposals

**Hypothesis:** Add IReadOnlyList<HarfBuzzSharp.Feature>? features parameter overloads to SKShaper.Shape (Buffer+SKFont and string+SKFont variants) and corresponding DrawShapedText overloads in CanvasExtensions. Existing overloads delegate to the new ones with null features to preserve backward compatibility.

1. **Add features overloads to SKShaper.Shape and DrawShapedText** — fix, confidence 0.92 (92%), cost/s, validated=untested
   - Add new overloads SKShaper.Shape(Buffer, float, float, SKFont, IReadOnlyList<Feature>?) and Shape(string, float, float, SKFont, IReadOnlyList<Feature>?), routing to hbFont.Shape(buffer, features, null) when features is non-empty. Add matching DrawShapedText overloads in CanvasExtensions. Existing overloads call the new ones with null to remain binary-compatible.

**Recommended proposal:** p1

**Why:** Well-scoped, non-breaking, low-effort API addition that directly solves the stated need without any redesign.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Feature request is well-specified with a proposed implementation. Needs API design review before work begins — specifically whether to also surface the shapers parameter and which overload signatures to add. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.98 (98%) | Apply type/feature-request, area/SkiaSharp.HarfBuzz | labels=type/feature-request, area/SkiaSharp.HarfBuzz |
| add-comment | medium | 0.88 (88%) | Acknowledge the request, confirm the HarfBuzzSharp.Font API gap, and outline the proposed path. | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed write-up and implementation sketch!

This is a valid API gap: `HarfBuzzSharp.Font` already exposes `Shape(Buffer, IReadOnlyList<Feature>, IReadOnlyList<string>)`, but `SKShaper` has no way to forward OpenType feature lists to it, forcing callers to re-implement internal buffer/font construction.

The proposed approach — new `Shape(Buffer, float, float, SKFont, IReadOnlyList<Feature>?)` overloads that delegate to `hbFont.Shape(buffer, features, null)` when features is non-null/non-empty, with existing overloads forwarding `null` — is a clean, ABI-safe addition.

A few design questions before we finalize the signatures:
1. Should the `shapers` parameter (second arg of `Font.Shape`) also be exposed, or just `features`?
2. Should we add a `params Feature[]` convenience overload in addition to `IReadOnlyList<Feature>`?

Contributions welcome — please follow [CONTRIBUTING.md](../../CONTRIBUTING.md) and open a PR targeting `main`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4273,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-07-27T05:30:00Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "Request to expose HarfBuzz OpenType feature parameters (e.g. vert, smcp, liga) through new overloads of SKShaper.Shape and CanvasExtensions.DrawShapedText so callers do not need to bypass SKShaper and re-implement HarfBuzz buffer/font setup directly.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.98
    },
    "area": {
      "value": "area/SkiaSharp.HarfBuzz",
      "confidence": 0.97
    }
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a SKShaper with a typeface that has OpenType features (e.g. CJK vert, small caps).",
        "Call SKShaper.Shape or DrawShapedText — no way to pass HarfBuzz Feature[] parameters.",
        "Observe that OpenType features cannot be activated without bypassing SKShaper entirely."
      ],
      "codeSnippets": [
        "var vert = new[] { new Feature(new Tag('v','e','r','t'), 1) };\n_canvas.DrawShapedText(shaper, text, SKPoint.Empty, SKTextAlign.Left, skFont, paint, vert);",
        "hbFont.Shape(buffer, features, null); // already exists in HarfBuzzSharp.Font but not surfaced"
      ],
      "environmentDetails": "SkiaSharp.HarfBuzz – no version specified; applies to current main branch"
    }
  },
  "analysis": {
    "summary": "SKShaper.Shape internally calls hbFont.Shape(buffer) with no feature arguments. HarfBuzzSharp.Font already has Font.Shape(Buffer, IReadOnlyList<Feature>, IReadOnlyList<string>) but SKShaper does not expose it. Adding new overloads with an optional IReadOnlyList<Feature> parameter would be a non-breaking, ABI-safe change following the existing overload pattern.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/SKShaper.cs",
        "lines": "67",
        "finding": "hbFont.Shape(buffer) is called with no features. The private hbFont field is of type HarfBuzzSharp.Font which already supports Shape(Buffer, IReadOnlyList<Feature>, IReadOnlyList<string>).",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/CanvasExtensions.cs",
        "lines": "69",
        "finding": "DrawShapedText calls shaper.Shape(text, x, y, font) with no features and has no overload accepting Feature[] or IReadOnlyList<Feature>.",
        "relevance": "direct"
      },
      {
        "file": "binding/HarfBuzzSharp/Font.cs",
        "finding": "Font.Shape(Buffer buffer, IReadOnlyList<Feature> features, IReadOnlyList<string> shapers) already exists in HarfBuzzSharp, confirming the lower-level API is available and just needs surfacing through SKShaper.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "HarfBuzzSharp.Font.Shape(Buffer, params Feature[]) already supports this; we just don't surface it.",
        "source": "issue body",
        "interpretation": "The required infrastructure exists; only the public SKShaper wrapper needs new overloads."
      },
      {
        "text": "consumers... currently have to bypass the wrapper and re-implement buffer/font setup against HarfBuzzSharp directly",
        "source": "issue body",
        "interpretation": "Valid API gap — users are forced into duplicating internal setup to access a feature the lower layer already exposes."
      }
    ],
    "rationale": "This is a clear feature request for a well-scoped API gap. The lower-level HarfBuzz API already supports feature lists; SKShaper simply needs new overloads that forward them. The reporter has provided a detailed, plausible implementation sketch. No bugs are present — the existing behavior is correct, it just lacks an extension point.",
    "resolution": {
      "hypothesis": "Add IReadOnlyList<HarfBuzzSharp.Feature>? features parameter overloads to SKShaper.Shape (Buffer+SKFont and string+SKFont variants) and corresponding DrawShapedText overloads in CanvasExtensions. Existing overloads delegate to the new ones with null features to preserve backward compatibility.",
      "proposals": [
        {
          "title": "Add features overloads to SKShaper.Shape and DrawShapedText",
          "category": "fix",
          "effort": "cost/s",
          "confidence": 0.92,
          "validated": "untested",
          "description": "Add new overloads SKShaper.Shape(Buffer, float, float, SKFont, IReadOnlyList<Feature>?) and Shape(string, float, float, SKFont, IReadOnlyList<Feature>?), routing to hbFont.Shape(buffer, features, null) when features is non-empty. Add matching DrawShapedText overloads in CanvasExtensions. Existing overloads call the new ones with null to remain binary-compatible."
        }
      ],
      "recommendedProposal": "p1",
      "recommendedReason": "Well-scoped, non-breaking, low-effort API addition that directly solves the stated need without any redesign."
    },
    "nextQuestions": [
      "Should the shapers (second parameter to Font.Shape) also be exposed, or just features?",
      "Should a params Feature[] convenience overload be provided alongside IReadOnlyList<Feature>?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Feature request is well-specified with a proposed implementation. Needs API design review before work begins — specifically whether to also surface the shapers parameter and which overload signatures to add.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/feature-request, area/SkiaSharp.HarfBuzz",
        "risk": "low",
        "confidence": 0.98,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp.HarfBuzz"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the request, confirm the HarfBuzzSharp.Font API gap, and outline the proposed path.",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed write-up and implementation sketch!\n\nThis is a valid API gap: `HarfBuzzSharp.Font` already exposes `Shape(Buffer, IReadOnlyList<Feature>, IReadOnlyList<string>)`, but `SKShaper` has no way to forward OpenType feature lists to it, forcing callers to re-implement internal buffer/font construction.\n\nThe proposed approach — new `Shape(Buffer, float, float, SKFont, IReadOnlyList<Feature>?)` overloads that delegate to `hbFont.Shape(buffer, features, null)` when features is non-null/non-empty, with existing overloads forwarding `null` — is a clean, ABI-safe addition.\n\nA few design questions before we finalize the signatures:\n1. Should the `shapers` parameter (second arg of `Font.Shape`) also be exposed, or just `features`?\n2. Should we add a `params Feature[]` convenience overload in addition to `IReadOnlyList<Feature>`?\n\nContributions welcome — please follow [CONTRIBUTING.md](../../CONTRIBUTING.md) and open a PR targeting `main`."
      }
    ]
  }
}
```

</details>
