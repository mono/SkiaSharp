# Issue Triage Report — #2862

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:16:47Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | keep-open (0.85 (85%)) |

**Issue Summary:** Feature request to add implicit conversion operators between ValueTuple<float,float> and SKPoint, and a Deconstruct method to allow tuple-based construction and decomposition syntax.

**Analysis:** Reporter requests adding ValueTuple-based implicit conversion and Deconstruct to SKPoint. The feature does not currently exist — SKPoint already has implicit operators for System.Numerics.Vector2 but no ValueTuple equivalent. Implementation is pure C# with no native layer changes required.

**Recommendations:** **keep-open** — Valid, well-specified feature request with no duplicate. Needs maintainer decision on scope (SKPoint only vs other structs like SKPointI, SKSize). The Vector2 workaround exists, so low urgency.

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
| Current labels | type/feature-request |

## Evidence

### Reproduction

**Code snippets:**

```csharp
public static implicit operator SKPoint((float x, float y) tuple) => new SKPoint(tuple.x, tuple.y);
public void Deconstruct(out float x, out float y) => (x = X, y = Y);
```

## Analysis

### Technical Summary

Reporter requests adding ValueTuple-based implicit conversion and Deconstruct to SKPoint. The feature does not currently exist — SKPoint already has implicit operators for System.Numerics.Vector2 but no ValueTuple equivalent. Implementation is pure C# with no native layer changes required.

### Rationale

This is a pure C# ergonomics enhancement. SKPoint already has implicit operators for Vector2 in MathTypes.cs; this extends the same pattern to ValueTuples. The implementation path is clear and fully specified by the reporter. Classification as feature-request is correct — nothing is broken, this is new API surface. Design consideration: whether to scope to SKPoint alone or include SKPointI, SKSize, etc.

### Key Signals

- "public static implicit operator SKPoint((float x, float y) tuple) => new SKPoint(tuple.x, tuple.y);" — **issue body** (Reporter has thought through the implementation; the code is correct and idiomatic C#.)
- "It is currently (C#12) not possible to declare an implicit operator as extension method." — **issue body** (Reporter correctly identifies that the operator must be declared in the struct itself, which requires a SkiaSharp code change.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/MathTypes.cs` | 8-101 | direct | SKPoint struct currently has implicit operators to/from System.Numerics.Vector2 but no ValueTuple conversion or Deconstruct method. The feature does not exist. |
| `binding/SkiaSharp/MathTypes.cs` | 259-308 | related | SKSize, SKSizeI, SKPointI, SKRect, SKRectI are other value-type structs in the same file with no Deconstruct methods either, suggesting this would be a broader design decision. |

### Workarounds

- Use existing System.Numerics.Vector2 implicit conversion: SKPoint point = new Vector2(x, y); (Vector2 implicitly converts to SKPoint).
- Use the SKPoint constructor directly: SKPoint point = new SKPoint(x, y);

### Resolution Proposals

**Hypothesis:** Adding implicit (float, float) ↔ SKPoint and Deconstruct methods to the SKPoint struct in MathTypes.cs. No native changes required.

1. **Add implicit ValueTuple conversion and Deconstruct to SKPoint** — fix, confidence 0.85 (85%), cost/xs, validated=untested
   - Add the two members proposed by the reporter to SKPoint in binding/SkiaSharp/MathTypes.cs. Pure C# change, no native layer impact.
2. **Workaround using Vector2 implicit conversion** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Users can use System.Numerics.Vector2 which already has implicit conversion to SKPoint. SKPoint point = new Vector2(x, y); works today.

**Recommended proposal:** Add implicit ValueTuple conversion and Deconstruct to SKPoint

**Why:** Small, safe, additive pure-C# change. Implementation is fully specified. Improves ergonomics and aligns with modern C# patterns.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.85 (85%) |
| Reason | Valid, well-specified feature request with no duplicate. Needs maintainer decision on scope (SKPoint only vs other structs like SKPointI, SKSize). The Vector2 workaround exists, so low urgency. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Confirm feature-request type and apply area label | labels=type/feature-request, area/SkiaSharp |
| add-comment | medium | 0.85 (85%) | Acknowledge the feature request and mention existing Vector2 workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the well-thought-out feature request! A `Deconstruct` method and implicit `(float, float)` conversion would indeed be a nice ergonomic addition.

As a **workaround today**, you can use the existing `System.Numerics.Vector2` implicit conversion:
```csharp
SKPoint point = new Vector2(x, y); // Vector2 implicitly converts to SKPoint
```

We'll track this for a future release. If we add it to `SKPoint` we'd likely also want to add `Deconstruct` to `SKPointI`, `SKSize`, `SKSizeI`, and similar structs for consistency.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2862,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:16:47Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "Feature request to add implicit conversion operators between ValueTuple<float,float> and SKPoint, and a Deconstruct method to allow tuple-based construction and decomposition syntax.",
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
        "public static implicit operator SKPoint((float x, float y) tuple) => new SKPoint(tuple.x, tuple.y);\npublic void Deconstruct(out float x, out float y) => (x = X, y = Y);"
      ]
    }
  },
  "analysis": {
    "summary": "Reporter requests adding ValueTuple-based implicit conversion and Deconstruct to SKPoint. The feature does not currently exist — SKPoint already has implicit operators for System.Numerics.Vector2 but no ValueTuple equivalent. Implementation is pure C# with no native layer changes required.",
    "rationale": "This is a pure C# ergonomics enhancement. SKPoint already has implicit operators for Vector2 in MathTypes.cs; this extends the same pattern to ValueTuples. The implementation path is clear and fully specified by the reporter. Classification as feature-request is correct — nothing is broken, this is new API surface. Design consideration: whether to scope to SKPoint alone or include SKPointI, SKSize, etc.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/MathTypes.cs",
        "lines": "8-101",
        "finding": "SKPoint struct currently has implicit operators to/from System.Numerics.Vector2 but no ValueTuple conversion or Deconstruct method. The feature does not exist.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/MathTypes.cs",
        "lines": "259-308",
        "finding": "SKSize, SKSizeI, SKPointI, SKRect, SKRectI are other value-type structs in the same file with no Deconstruct methods either, suggesting this would be a broader design decision.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "public static implicit operator SKPoint((float x, float y) tuple) => new SKPoint(tuple.x, tuple.y);",
        "source": "issue body",
        "interpretation": "Reporter has thought through the implementation; the code is correct and idiomatic C#."
      },
      {
        "text": "It is currently (C#12) not possible to declare an implicit operator as extension method.",
        "source": "issue body",
        "interpretation": "Reporter correctly identifies that the operator must be declared in the struct itself, which requires a SkiaSharp code change."
      }
    ],
    "workarounds": [
      "Use existing System.Numerics.Vector2 implicit conversion: SKPoint point = new Vector2(x, y); (Vector2 implicitly converts to SKPoint).",
      "Use the SKPoint constructor directly: SKPoint point = new SKPoint(x, y);"
    ],
    "resolution": {
      "hypothesis": "Adding implicit (float, float) ↔ SKPoint and Deconstruct methods to the SKPoint struct in MathTypes.cs. No native changes required.",
      "proposals": [
        {
          "title": "Add implicit ValueTuple conversion and Deconstruct to SKPoint",
          "description": "Add the two members proposed by the reporter to SKPoint in binding/SkiaSharp/MathTypes.cs. Pure C# change, no native layer impact.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Workaround using Vector2 implicit conversion",
          "description": "Users can use System.Numerics.Vector2 which already has implicit conversion to SKPoint. SKPoint point = new Vector2(x, y); works today.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Add implicit ValueTuple conversion and Deconstruct to SKPoint",
      "recommendedReason": "Small, safe, additive pure-C# change. Implementation is fully specified. Improves ergonomics and aligns with modern C# patterns."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.85,
      "reason": "Valid, well-specified feature request with no duplicate. Needs maintainer decision on scope (SKPoint only vs other structs like SKPointI, SKSize). The Vector2 workaround exists, so low urgency.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Confirm feature-request type and apply area label",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the feature request and mention existing Vector2 workaround",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the well-thought-out feature request! A `Deconstruct` method and implicit `(float, float)` conversion would indeed be a nice ergonomic addition.\n\nAs a **workaround today**, you can use the existing `System.Numerics.Vector2` implicit conversion:\n```csharp\nSKPoint point = new Vector2(x, y); // Vector2 implicitly converts to SKPoint\n```\n\nWe'll track this for a future release. If we add it to `SKPoint` we'd likely also want to add `Deconstruct` to `SKPointI`, `SKSize`, `SKSizeI`, and similar structs for consistency."
      }
    ]
  }
}
```

</details>
