# Issue Triage Report — #918

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T17:24:05Z |
| Type | type/enhancement (0.90 (90%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** Request to add a Deflate/Inset method to SKRect for naming parity with SKRoundRect.Deflate, even though the same result is achievable via SKRect.Inflate with negative values.

**Analysis:** SKRect exposes Inflate(float x, float y) which grows the rectangle; shrinking it requires calling Inflate with negative values. SKRoundRect has a symmetric pair — Inflate/Deflate — making the API more discoverable and self-documenting. The request is to add a Deflate convenience method to SKRect for naming consistency. No native binding change is required since SKRect operations are implemented in pure C# arithmetic.

**Recommendations:** **needs-investigation** — Well-specified enhancement with clear scope and trivial implementation. Maintainer acknowledged it is valid. Ready for implementation once someone picks it up.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Environment:** No version specified; pure API surface request (no platform-specific behavior)

## Analysis

### Technical Summary

SKRect exposes Inflate(float x, float y) which grows the rectangle; shrinking it requires calling Inflate with negative values. SKRoundRect has a symmetric pair — Inflate/Deflate — making the API more discoverable and self-documenting. The request is to add a Deflate convenience method to SKRect for naming consistency. No native binding change is required since SKRect operations are implemented in pure C# arithmetic.

### Rationale

This is classified as type/enhancement because the functionality already exists (via Inflate with negative values) but the explicit Deflate/Inset API is missing. SKRoundRect already has both Inflate and Deflate, so this is clearly an API-parity gap rather than completely new functionality. The maintainer (mattleibow) acknowledged the inconsistency ('Good point') confirming it is a valid request.

### Key Signals

- "SKRoundRect provides a method called Deflate that binds to sk_rrect_inset. The functionality is provided by SKRoundRect but not by SKRect" — **issue body** (Reporter correctly identifies an API parity gap between SKRoundRect and SKRect.)
- "There is the equivalent Inflate where you can use negative values to deflate." — **comment #1 (mattleibow)** (Workaround exists: Inflate(-dx, -dy). Maintainer pointed this out.)
- "Sure you can inflate with negative values, but I see no reason for the Deflate support, since SKRoundRect provides such functionality" — **comment #2 (reporter)** (Reporter acknowledges workaround but argues for consistency — valid point accepted by maintainer.)
- "Good point." — **comment #3 (mattleibow)** (Maintainer concedes the parity argument is valid — request is accepted as legitimate enhancement.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/MathTypes.cs` | 428-444 | direct | SKRect has Inflate(float x, float y) (and static overload) implemented in pure C# arithmetic — no native API call. There is no Deflate, Inset, or Outset method on SKRect. |
| `binding/SkiaSharp/SKRoundRect.cs` | 151-170 | direct | SKRoundRect has both Deflate(float dx, float dy) → sk_rrect_inset and Inflate(float dx, float dy) → sk_rrect_outset, confirming the asymmetry with SKRect. |

### Workarounds

- Use SKRect.Inflate(-dx, -dy) to shrink the rectangle by (dx, dy) on each side — functionally equivalent to a Deflate/Inset call.

### Next Questions

- Should the method be named Deflate (matching SKRoundRect) or Inset (matching Skia C++ naming)? Or both as aliases?
- Should SKRectI also receive a corresponding Deflate method for consistency?

### Resolution Proposals

**Hypothesis:** Adding a Deflate convenience method to SKRect (and optionally SKRectI) that delegates to Inflate(-dx, -dy) would close the naming gap with SKRoundRect with minimal effort.

1. **Use Inflate with negative values (workaround)** — workaround, confidence 1.00 (100%), cost/xs, validated=yes
   - Call rect.Inflate(-dx, -dy) wherever inset/deflate behavior is needed. Achieves identical result without any code change.
2. **Add Deflate method to SKRect** — fix, confidence 0.92 (92%), cost/xs, validated=untested
   - Add Deflate(SKSize size), Deflate(float dx, float dy), and matching static Deflate(SKRect rect, float dx, float dy) overloads to SKRect in MathTypes.cs. Implement as Inflate(-dx, -dy). Mirror the same pattern on SKRectI for completeness.

**Recommended proposal:** Add Deflate method to SKRect

**Why:** Small effort change that closes an API naming gap, confirmed valid by the maintainer. Implementation is trivial — delegate to Inflate(-x, -y).

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Well-specified enhancement with clear scope and trivial implementation. Maintainer acknowledged it is valid. Ready for implementation once someone picks it up. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply enhancement and SkiaSharp area labels | labels=type/enhancement, area/SkiaSharp |
| add-comment | medium | 0.88 (88%) | Confirm workaround and note the request is valid | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! The existing workaround is `rect.Inflate(-dx, -dy)` which achieves the same result as a Deflate/Inset call.

That said, the parity with `SKRoundRect.Deflate` is a good argument and the implementation would be trivial (just a delegate to `Inflate(-dx, -dy)`). This is tracked as an enhancement for a future release.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 918,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T17:24:05Z"
  },
  "summary": "Request to add a Deflate/Inset method to SKRect for naming parity with SKRoundRect.Deflate, even though the same result is achievable via SKRect.Inflate with negative values.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    }
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "No version specified; pure API surface request (no platform-specific behavior)",
      "repoLinks": []
    }
  },
  "analysis": {
    "summary": "SKRect exposes Inflate(float x, float y) which grows the rectangle; shrinking it requires calling Inflate with negative values. SKRoundRect has a symmetric pair — Inflate/Deflate — making the API more discoverable and self-documenting. The request is to add a Deflate convenience method to SKRect for naming consistency. No native binding change is required since SKRect operations are implemented in pure C# arithmetic.",
    "rationale": "This is classified as type/enhancement because the functionality already exists (via Inflate with negative values) but the explicit Deflate/Inset API is missing. SKRoundRect already has both Inflate and Deflate, so this is clearly an API-parity gap rather than completely new functionality. The maintainer (mattleibow) acknowledged the inconsistency ('Good point') confirming it is a valid request.",
    "keySignals": [
      {
        "text": "SKRoundRect provides a method called Deflate that binds to sk_rrect_inset. The functionality is provided by SKRoundRect but not by SKRect",
        "source": "issue body",
        "interpretation": "Reporter correctly identifies an API parity gap between SKRoundRect and SKRect."
      },
      {
        "text": "There is the equivalent Inflate where you can use negative values to deflate.",
        "source": "comment #1 (mattleibow)",
        "interpretation": "Workaround exists: Inflate(-dx, -dy). Maintainer pointed this out."
      },
      {
        "text": "Sure you can inflate with negative values, but I see no reason for the Deflate support, since SKRoundRect provides such functionality",
        "source": "comment #2 (reporter)",
        "interpretation": "Reporter acknowledges workaround but argues for consistency — valid point accepted by maintainer."
      },
      {
        "text": "Good point.",
        "source": "comment #3 (mattleibow)",
        "interpretation": "Maintainer concedes the parity argument is valid — request is accepted as legitimate enhancement."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/MathTypes.cs",
        "lines": "428-444",
        "finding": "SKRect has Inflate(float x, float y) (and static overload) implemented in pure C# arithmetic — no native API call. There is no Deflate, Inset, or Outset method on SKRect.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKRoundRect.cs",
        "lines": "151-170",
        "finding": "SKRoundRect has both Deflate(float dx, float dy) → sk_rrect_inset and Inflate(float dx, float dy) → sk_rrect_outset, confirming the asymmetry with SKRect.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Use SKRect.Inflate(-dx, -dy) to shrink the rectangle by (dx, dy) on each side — functionally equivalent to a Deflate/Inset call."
    ],
    "nextQuestions": [
      "Should the method be named Deflate (matching SKRoundRect) or Inset (matching Skia C++ naming)? Or both as aliases?",
      "Should SKRectI also receive a corresponding Deflate method for consistency?"
    ],
    "resolution": {
      "hypothesis": "Adding a Deflate convenience method to SKRect (and optionally SKRectI) that delegates to Inflate(-dx, -dy) would close the naming gap with SKRoundRect with minimal effort.",
      "proposals": [
        {
          "title": "Use Inflate with negative values (workaround)",
          "description": "Call rect.Inflate(-dx, -dy) wherever inset/deflate behavior is needed. Achieves identical result without any code change.",
          "category": "workaround",
          "confidence": 1.0,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Add Deflate method to SKRect",
          "description": "Add Deflate(SKSize size), Deflate(float dx, float dy), and matching static Deflate(SKRect rect, float dx, float dy) overloads to SKRect in MathTypes.cs. Implement as Inflate(-dx, -dy). Mirror the same pattern on SKRectI for completeness.",
          "category": "fix",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add Deflate method to SKRect",
      "recommendedReason": "Small effort change that closes an API naming gap, confirmed valid by the maintainer. Implementation is trivial — delegate to Inflate(-x, -y)."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Well-specified enhancement with clear scope and trivial implementation. Maintainer acknowledged it is valid. Ready for implementation once someone picks it up.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement and SkiaSharp area labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Confirm workaround and note the request is valid",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the report! The existing workaround is `rect.Inflate(-dx, -dy)` which achieves the same result as a Deflate/Inset call.\n\nThat said, the parity with `SKRoundRect.Deflate` is a good argument and the implementation would be trivial (just a delegate to `Inflate(-dx, -dy)`). This is tracked as an enhancement for a future release."
      }
    ]
  }
}
```

</details>
