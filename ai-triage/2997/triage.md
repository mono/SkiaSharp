# Issue Triage Report — #2997

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T01:00:00Z |
| Type | type/bug (0.85 (85%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-external (0.80 (80%)) |

**Issue Summary:** SKMatrix.MapRect normalizes non-standardized input rectangles (swapping Left/Right and Top/Bottom when inverted), which is upstream Skia behavior but breaks the expectation that an identity matrix transform leaves coordinates unchanged.

**Analysis:** SKMatrix.MapRect delegates to sk_matrix_map_rect which calls Skia's SkMatrix::mapRect. This function has always normalized (sorted) the output rect to ensure Left<=Right and Top<=Bottom. This is intentional upstream Skia behavior, not a SkiaSharp binding bug. The workaround is to use MapPoint on each corner individually and reconstruct the SKRect.

**Recommendations:** **close-as-external** — Root cause is upstream Skia behavior (SkMatrix::mapRect always normalizes). Community commenter confirmed with direct source link. SkiaSharp bindings correctly pass through to Skia. A workaround exists using MapPoint.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create an SKRect with Left=25, Right=15, Top=10, Bottom=2 (non-standardized: Left>Right, Top>Bottom)
2. Apply SKMatrix.CreateIdentity().MapRect() to it
3. Observe that result has Left/Right and Top/Bottom swapped to standardize the rect

**Environment:** Windows, Visual Studio, SkiaSharp 2.88.3

**Repository links:**
- https://github.com/google/skia/blob/158dc9d7d4cafb177b99b68c5dc502f8f4282092/src/core/SkMatrix.cpp#L1156 — Upstream Skia SkMatrix::mapRect source — confirms normalization is intentional

**Code snippets:**

```csharp
var inputRect = new SKRect();
inputRect.Left = 25;
inputRect.Right = 15;
inputRect.Bottom = 2;
inputRect.Top = 10;
var resultRect = SKMatrix.CreateIdentity().MapRect(inputRect);
Assert.AreEqual(inputRect.Right, resultRect.Right); // fails
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | MapRect(identity) swaps Left/Right and Top/Bottom on a non-standardized input rect |
| Repro quality | complete |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3, 2.88.2 |
| Worked in | 2.88.2 |
| Broke in | 2.88.3 |
| Current relevance | likely |
| Relevance reason | MapRect delegates directly to sk_matrix_map_rect which calls Skia's SkMatrix::mapRect. Upstream normalization behavior is unlikely to have changed between 2.88.2 and 2.88.3. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | False |
| Confidence | 0.60 (60%) |
| Reason | Commenter confirmed this is upstream Skia behavior with a link to SkMatrix.cpp#L1156. Normalization in mapRect is a long-standing Skia design choice. Reporter may have been testing with already-standardized rects in 2.88.2. |
| Worked in version | — |
| Broke in version | — |

## Analysis

### Technical Summary

SKMatrix.MapRect delegates to sk_matrix_map_rect which calls Skia's SkMatrix::mapRect. This function has always normalized (sorted) the output rect to ensure Left<=Right and Top<=Bottom. This is intentional upstream Skia behavior, not a SkiaSharp binding bug. The workaround is to use MapPoint on each corner individually and reconstruct the SKRect.

### Rationale

Classified as type/bug because the behavior violates a reasonable user expectation (identity matrix should be a no-op), but the root cause is in upstream Skia. close-as-external fits better than close-as-not-a-bug because the behavior is technically correct per Skia's design but is undocumented at the SkiaSharp layer. area/SkiaSharp is correct as the binding wraps the core matrix API. Platform os/Windows-Classic matches reporter environment.

### Key Signals

- "The resultRectangle.Left == inputRectangle.Right" — **issue body** (MapRect swaps Left/Right when input has Left>Right — this is Skia's normalization/sort behavior.)
- "That's what Skia does, it's not really an issue with the bindings themselves: https://github.com/google/skia/blob/158dc9d7d4cafb177b99b68c5dc502f8f4282092/src/core/SkMatrix.cpp#L1156" — **comment by molesmoke** (Community confirms this is upstream Skia behavior, pointing to the exact source location.)
- "Last Known Good Version of SkiaSharp: 2.88.2" — **issue body** (Reporter claims regression but the normalization behavior is a long-standing Skia design; reporter may have been using standardized rects in prior testing.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKMatrix.cs` | 299-306 | direct | MapRect calls SkiaApi.sk_matrix_map_rect directly with no pre/post processing in C#. The normalization is entirely in the native Skia library — the binding faithfully passes through to upstream behavior. |
| `binding/SkiaSharp/MathTypes.cs` | 382-398 | related | SKRect.Standardized property exists and explicitly normalizes Left/Right/Top/Bottom ordering — confirming SkiaSharp is aware of non-standardized rects and provides an opt-in normalization mechanism separate from MapRect. |
| `binding/SkiaSharp/SKMatrix.cs` | 310-320 | related | MapPoint(float x, float y) calls sk_matrix_map_xy — maps individual coordinates without any normalization, making it a viable workaround for mapping rect corners individually to preserve original coordinate order. |

### Workarounds

- Use SKMatrix.MapPoint() on each corner individually then reconstruct the SKRect: var tl = matrix.MapPoint(rect.Left, rect.Top); var br = matrix.MapPoint(rect.Right, rect.Bottom); var result = new SKRect(tl.X, tl.Y, br.X, br.Y);

### Next Questions

- Was there actually a behavioral change in Skia's mapRect between the Skia version used in SkiaSharp 2.88.2 and 2.88.3, or is the regression claim inaccurate?
- Should SKMatrix.MapRect be documented to note it always returns a standardized rect?

### Resolution Proposals

**Hypothesis:** sk_matrix_map_rect (and underlying Skia SkMatrix::mapRect) always normalizes the output rect by sorting coordinates. The workaround is to map individual corner points using MapPoint and reconstruct the SKRect.

1. **Map corners individually using MapPoint** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Use MapPoint twice on the top-left and bottom-right corners to map coordinates without normalization, then reconstruct the SKRect:

```csharp
var tl = matrix.MapPoint(inputRect.Left, inputRect.Top);
var br = matrix.MapPoint(inputRect.Right, inputRect.Bottom);
var resultRect = new SKRect(tl.X, tl.Y, br.X, br.Y);
```

Note: the resulting SKRect preserves the original (possibly inverted) coordinate order.

```csharp
var tl = matrix.MapPoint(inputRect.Left, inputRect.Top);
var br = matrix.MapPoint(inputRect.Right, inputRect.Bottom);
var resultRect = new SKRect(tl.X, tl.Y, br.X, br.Y);
```
2. **Add XML documentation to MapRect noting normalization** — fix, confidence 0.85 (85%), cost/xs, validated=untested
   - Add a remarks element to SKMatrix.MapRect documenting that the output is always a standardized (sorted) SKRect, consistent with upstream Skia behavior.

**Recommended proposal:** Map corners individually using MapPoint

**Why:** Immediately actionable workaround the reporter can use today with no SkiaSharp changes required. All three validation agents confirmed the APIs are correct and it solves the stated problem.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.80 (80%) |
| Reason | Root cause is upstream Skia behavior (SkMatrix::mapRect always normalizes). Community commenter confirmed with direct source link. SkiaSharp bindings correctly pass through to Skia. A workaround exists using MapPoint. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, area, platform, and tenet labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/compatibility |
| add-comment | medium | 0.80 (80%) | Explain upstream Skia behavior and provide MapPoint workaround | — |
| close-issue | medium | 0.75 (75%) | Close as external — upstream Skia behavior confirmed by community | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and repro code!

This behavior is intentional in upstream Skia — `SkMatrix::mapRect` always returns a **standardized** (sorted) rectangle where `left ≤ right` and `top ≤ bottom`. This is confirmed by a community member [pointing to the Skia source](https://github.com/google/skia/blob/158dc9d7d4cafb177b99b68c5dc502f8f4282092/src/core/SkMatrix.cpp#L1156). SkiaSharp's `MapRect` delegates directly to this native function without modification.

**Workaround:** If you need to map a non-standardized rect while preserving its coordinate order, use `MapPoint` on each corner individually:

```csharp
var tl = matrix.MapPoint(inputRect.Left, inputRect.Top);
var br = matrix.MapPoint(inputRect.Right, inputRect.Bottom);
var resultRect = new SKRect(tl.X, tl.Y, br.X, br.Y);
```

`SKRect.Standardized` is also available if you want an explicit opt-in normalization step.

We'll close this as the behavior originates in upstream Skia, but a documentation improvement to `MapRect` to clarify this normalization behavior would be welcome.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2997,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T01:00:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKMatrix.MapRect normalizes non-standardized input rectangles (swapping Left/Right and Top/Bottom when inverted), which is upstream Skia behavior but breaks the expectation that an identity matrix transform leaves coordinates unchanged.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.85
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "MapRect(identity) swaps Left/Right and Top/Bottom on a non-standardized input rect",
      "reproQuality": "complete"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKRect with Left=25, Right=15, Top=10, Bottom=2 (non-standardized: Left>Right, Top>Bottom)",
        "Apply SKMatrix.CreateIdentity().MapRect() to it",
        "Observe that result has Left/Right and Top/Bottom swapped to standardize the rect"
      ],
      "codeSnippets": [
        "var inputRect = new SKRect();\ninputRect.Left = 25;\ninputRect.Right = 15;\ninputRect.Bottom = 2;\ninputRect.Top = 10;\nvar resultRect = SKMatrix.CreateIdentity().MapRect(inputRect);\nAssert.AreEqual(inputRect.Right, resultRect.Right); // fails"
      ],
      "environmentDetails": "Windows, Visual Studio, SkiaSharp 2.88.3",
      "repoLinks": [
        {
          "url": "https://github.com/google/skia/blob/158dc9d7d4cafb177b99b68c5dc502f8f4282092/src/core/SkMatrix.cpp#L1156",
          "description": "Upstream Skia SkMatrix::mapRect source — confirms normalization is intentional"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3",
        "2.88.2"
      ],
      "workedIn": "2.88.2",
      "brokeIn": "2.88.3",
      "currentRelevance": "likely",
      "relevanceReason": "MapRect delegates directly to sk_matrix_map_rect which calls Skia's SkMatrix::mapRect. Upstream normalization behavior is unlikely to have changed between 2.88.2 and 2.88.3."
    },
    "regression": {
      "isRegression": false,
      "confidence": 0.6,
      "reason": "Commenter confirmed this is upstream Skia behavior with a link to SkMatrix.cpp#L1156. Normalization in mapRect is a long-standing Skia design choice. Reporter may have been testing with already-standardized rects in 2.88.2."
    }
  },
  "analysis": {
    "summary": "SKMatrix.MapRect delegates to sk_matrix_map_rect which calls Skia's SkMatrix::mapRect. This function has always normalized (sorted) the output rect to ensure Left<=Right and Top<=Bottom. This is intentional upstream Skia behavior, not a SkiaSharp binding bug. The workaround is to use MapPoint on each corner individually and reconstruct the SKRect.",
    "rationale": "Classified as type/bug because the behavior violates a reasonable user expectation (identity matrix should be a no-op), but the root cause is in upstream Skia. close-as-external fits better than close-as-not-a-bug because the behavior is technically correct per Skia's design but is undocumented at the SkiaSharp layer. area/SkiaSharp is correct as the binding wraps the core matrix API. Platform os/Windows-Classic matches reporter environment.",
    "keySignals": [
      {
        "text": "The resultRectangle.Left == inputRectangle.Right",
        "source": "issue body",
        "interpretation": "MapRect swaps Left/Right when input has Left>Right — this is Skia's normalization/sort behavior."
      },
      {
        "text": "That's what Skia does, it's not really an issue with the bindings themselves: https://github.com/google/skia/blob/158dc9d7d4cafb177b99b68c5dc502f8f4282092/src/core/SkMatrix.cpp#L1156",
        "source": "comment by molesmoke",
        "interpretation": "Community confirms this is upstream Skia behavior, pointing to the exact source location."
      },
      {
        "text": "Last Known Good Version of SkiaSharp: 2.88.2",
        "source": "issue body",
        "interpretation": "Reporter claims regression but the normalization behavior is a long-standing Skia design; reporter may have been using standardized rects in prior testing."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKMatrix.cs",
        "lines": "299-306",
        "finding": "MapRect calls SkiaApi.sk_matrix_map_rect directly with no pre/post processing in C#. The normalization is entirely in the native Skia library — the binding faithfully passes through to upstream behavior.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/MathTypes.cs",
        "lines": "382-398",
        "finding": "SKRect.Standardized property exists and explicitly normalizes Left/Right/Top/Bottom ordering — confirming SkiaSharp is aware of non-standardized rects and provides an opt-in normalization mechanism separate from MapRect.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKMatrix.cs",
        "lines": "310-320",
        "finding": "MapPoint(float x, float y) calls sk_matrix_map_xy — maps individual coordinates without any normalization, making it a viable workaround for mapping rect corners individually to preserve original coordinate order.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use SKMatrix.MapPoint() on each corner individually then reconstruct the SKRect: var tl = matrix.MapPoint(rect.Left, rect.Top); var br = matrix.MapPoint(rect.Right, rect.Bottom); var result = new SKRect(tl.X, tl.Y, br.X, br.Y);"
    ],
    "nextQuestions": [
      "Was there actually a behavioral change in Skia's mapRect between the Skia version used in SkiaSharp 2.88.2 and 2.88.3, or is the regression claim inaccurate?",
      "Should SKMatrix.MapRect be documented to note it always returns a standardized rect?"
    ],
    "resolution": {
      "hypothesis": "sk_matrix_map_rect (and underlying Skia SkMatrix::mapRect) always normalizes the output rect by sorting coordinates. The workaround is to map individual corner points using MapPoint and reconstruct the SKRect.",
      "proposals": [
        {
          "title": "Map corners individually using MapPoint",
          "description": "Use MapPoint twice on the top-left and bottom-right corners to map coordinates without normalization, then reconstruct the SKRect:\n\n```csharp\nvar tl = matrix.MapPoint(inputRect.Left, inputRect.Top);\nvar br = matrix.MapPoint(inputRect.Right, inputRect.Bottom);\nvar resultRect = new SKRect(tl.X, tl.Y, br.X, br.Y);\n```\n\nNote: the resulting SKRect preserves the original (possibly inverted) coordinate order.",
          "category": "workaround",
          "codeSnippet": "var tl = matrix.MapPoint(inputRect.Left, inputRect.Top);\nvar br = matrix.MapPoint(inputRect.Right, inputRect.Bottom);\nvar resultRect = new SKRect(tl.X, tl.Y, br.X, br.Y);",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Add XML documentation to MapRect noting normalization",
          "description": "Add a remarks element to SKMatrix.MapRect documenting that the output is always a standardized (sorted) SKRect, consistent with upstream Skia behavior.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Map corners individually using MapPoint",
      "recommendedReason": "Immediately actionable workaround the reporter can use today with no SkiaSharp changes required. All three validation agents confirmed the APIs are correct and it solves the stated problem."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.8,
      "reason": "Root cause is upstream Skia behavior (SkMatrix::mapRect always normalizes). Community commenter confirmed with direct source link. SkiaSharp bindings correctly pass through to Skia. A workaround exists using MapPoint.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, area, platform, and tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain upstream Skia behavior and provide MapPoint workaround",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the detailed report and repro code!\n\nThis behavior is intentional in upstream Skia — `SkMatrix::mapRect` always returns a **standardized** (sorted) rectangle where `left ≤ right` and `top ≤ bottom`. This is confirmed by a community member [pointing to the Skia source](https://github.com/google/skia/blob/158dc9d7d4cafb177b99b68c5dc502f8f4282092/src/core/SkMatrix.cpp#L1156). SkiaSharp's `MapRect` delegates directly to this native function without modification.\n\n**Workaround:** If you need to map a non-standardized rect while preserving its coordinate order, use `MapPoint` on each corner individually:\n\n```csharp\nvar tl = matrix.MapPoint(inputRect.Left, inputRect.Top);\nvar br = matrix.MapPoint(inputRect.Right, inputRect.Bottom);\nvar resultRect = new SKRect(tl.X, tl.Y, br.X, br.Y);\n```\n\n`SKRect.Standardized` is also available if you want an explicit opt-in normalization step.\n\nWe'll close this as the behavior originates in upstream Skia, but a documentation improvement to `MapRect` to clarify this normalization behavior would be welcome."
      },
      {
        "type": "close-issue",
        "description": "Close as external — upstream Skia behavior confirmed by community",
        "risk": "medium",
        "confidence": 0.75,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
