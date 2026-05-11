# Issue Triage Report — #2779

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-11T05:31:00Z |
| Type | type/enhancement (0.95 (95%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | keep-open (0.92 (92%)) |

**Issue Summary:** SKMatrix operations (Invert, Concat, MapRect, etc.) use native P/Invoke calls that are ~2x slower than equivalent managed C# math; request to replace them with pure C# implementations.

**Analysis:** SKMatrix currently delegates math operations (Invert, Concat, MapRect, MapPoints, MapVector, MapRadius) to native Skia C++ via P/Invoke (sk_matrix_try_invert, sk_matrix_concat, sk_matrix_map_rect, etc.). The benchmark data shows SKMatrix.Invert() takes ~45 ns vs ~19 ns for managed Matrix4x4.Invert. The proposal is to replace all native calls in SKMatrix with pure C# implementations, similar to how SKMatrix44 already converts to Matrix4x4 for its invert.

**Recommendations:** **keep-open** — Well-specified performance enhancement with benchmark evidence, already tracked on the 4.x RC 1 milestone. Triage confirms the native calls are still present and the managed implementation path is clear.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/performance |
| Partner | — |

## Evidence

### Reproduction

**Code snippets:**

```csharp
SKMatrix.Invert() via SkiaApi.sk_matrix_try_invert — 45.30 ns vs Matrix4x4.Invert 19.20 ns
```

```csharp
SKMatrix44.Invert() converts to Matrix4x4, inverts, converts back — 22.52 ns
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 4.x RC 1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Issue is open, milestone is 4.x RC 1, native P/Invoke calls still present in SKMatrix.cs |

## Analysis

### Technical Summary

SKMatrix currently delegates math operations (Invert, Concat, MapRect, MapPoints, MapVector, MapRadius) to native Skia C++ via P/Invoke (sk_matrix_try_invert, sk_matrix_concat, sk_matrix_map_rect, etc.). The benchmark data shows SKMatrix.Invert() takes ~45 ns vs ~19 ns for managed Matrix4x4.Invert. The proposal is to replace all native calls in SKMatrix with pure C# implementations, similar to how SKMatrix44 already converts to Matrix4x4 for its invert.

### Rationale

This is a well-scoped enhancement with clear benchmark evidence. SKMatrix is a hot path in rendering pipelines; replacing P/Invoke calls with pure C# math will reduce overhead without changing the public API surface. The issue is already tracked under milestone 4.x RC 1 with labels tenet/performance and cost/s, indicating maintainer intent to implement this. suggestedAction is keep-open as the work is planned and well-specified.

### Key Signals

- "InvertUsingSKMatrix: 45.30 ns vs InvertUsingMatrix4x4: 19.20 ns — basically half the time" — **issue body benchmark** (Native P/Invoke for matrix invert is ~2.4x slower than managed Matrix4x4)
- "SKMatrix44 is converting from SKMatrix44 to Matrix4x4, then running the invert, and then converting back — still 49% of the time" — **issue body** (Even with conversion overhead, managed math beats native interop)
- "In complex rendering scenarios when layer count is way over 100k - we've seen a significant 10-20% perf improvement" — **comment by Lunacy team** (Real-world impact for high-throughput rendering workloads is significant)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKMatrix.cs` | 241-417 | direct | All non-trivial SKMatrix operations (IsInvertible, TryInvert, Concat, PreConcat, PostConcat, MapRect, MapPoint, MapPoints, MapVector, MapVectors, MapRadius) delegate to native via SkiaApi.sk_matrix_* P/Invoke calls. |
| `binding/SkiaSharp/SKMatrix44.cs` | — | related | SKMatrix44.Invert() already converts to System.Numerics.Matrix4x4 for the invert operation and converts back, demonstrating the pattern that managed math can replace native calls. |

### Resolution Proposals

**Hypothesis:** Replace all sk_matrix_* P/Invoke calls in SKMatrix.cs with equivalent managed C# math using System.Numerics or inline float arithmetic. The 3x3 affine matrix invert is well-known: determinant-based computation in ~10 multiply-add operations.

1. **Implement SKMatrix math in pure C#** — fix, cost/s, validated=untested
   - Replace SkiaApi.sk_matrix_try_invert, sk_matrix_concat, sk_matrix_map_rect, sk_matrix_map_xy, sk_matrix_map_points, sk_matrix_map_vector, sk_matrix_map_vectors, sk_matrix_map_radius with equivalent C# implementations. Use System.Numerics.Matrix4x4 as a conversion bridge or implement direct 3x3 math.

**Recommended proposal:** proposal-1

**Why:** Directly eliminates the P/Invoke overhead on every matrix operation; already planned by maintainer (milestone 4.x RC 1, cost/s).

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.92 (92%) |
| Reason | Well-specified performance enhancement with benchmark evidence, already tracked on the 4.x RC 1 milestone. Triage confirms the native calls are still present and the managed implementation path is clear. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/enhancement, area/SkiaSharp, tenet/performance labels | labels=type/enhancement, area/SkiaSharp, tenet/performance |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2779,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-11T05:31:00Z"
  },
  "summary": "SKMatrix operations (Invert, Concat, MapRect, etc.) use native P/Invoke calls that are ~2x slower than equivalent managed C# math; request to replace them with pure C# implementations.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "SKMatrix.Invert() via SkiaApi.sk_matrix_try_invert — 45.30 ns vs Matrix4x4.Invert 19.20 ns",
        "SKMatrix44.Invert() converts to Matrix4x4, inverts, converts back — 22.52 ns"
      ],
      "attachments": [],
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "4.x RC 1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Issue is open, milestone is 4.x RC 1, native P/Invoke calls still present in SKMatrix.cs"
    }
  },
  "analysis": {
    "summary": "SKMatrix currently delegates math operations (Invert, Concat, MapRect, MapPoints, MapVector, MapRadius) to native Skia C++ via P/Invoke (sk_matrix_try_invert, sk_matrix_concat, sk_matrix_map_rect, etc.). The benchmark data shows SKMatrix.Invert() takes ~45 ns vs ~19 ns for managed Matrix4x4.Invert. The proposal is to replace all native calls in SKMatrix with pure C# implementations, similar to how SKMatrix44 already converts to Matrix4x4 for its invert.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKMatrix.cs",
        "lines": "241-417",
        "finding": "All non-trivial SKMatrix operations (IsInvertible, TryInvert, Concat, PreConcat, PostConcat, MapRect, MapPoint, MapPoints, MapVector, MapVectors, MapRadius) delegate to native via SkiaApi.sk_matrix_* P/Invoke calls.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKMatrix44.cs",
        "finding": "SKMatrix44.Invert() already converts to System.Numerics.Matrix4x4 for the invert operation and converts back, demonstrating the pattern that managed math can replace native calls.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "InvertUsingSKMatrix: 45.30 ns vs InvertUsingMatrix4x4: 19.20 ns — basically half the time",
        "source": "issue body benchmark",
        "interpretation": "Native P/Invoke for matrix invert is ~2.4x slower than managed Matrix4x4"
      },
      {
        "text": "SKMatrix44 is converting from SKMatrix44 to Matrix4x4, then running the invert, and then converting back — still 49% of the time",
        "source": "issue body",
        "interpretation": "Even with conversion overhead, managed math beats native interop"
      },
      {
        "text": "In complex rendering scenarios when layer count is way over 100k - we've seen a significant 10-20% perf improvement",
        "source": "comment by Lunacy team",
        "interpretation": "Real-world impact for high-throughput rendering workloads is significant"
      }
    ],
    "rationale": "This is a well-scoped enhancement with clear benchmark evidence. SKMatrix is a hot path in rendering pipelines; replacing P/Invoke calls with pure C# math will reduce overhead without changing the public API surface. The issue is already tracked under milestone 4.x RC 1 with labels tenet/performance and cost/s, indicating maintainer intent to implement this. suggestedAction is keep-open as the work is planned and well-specified.",
    "resolution": {
      "hypothesis": "Replace all sk_matrix_* P/Invoke calls in SKMatrix.cs with equivalent managed C# math using System.Numerics or inline float arithmetic. The 3x3 affine matrix invert is well-known: determinant-based computation in ~10 multiply-add operations.",
      "proposals": [
        {
          "id": "proposal-1",
          "title": "Implement SKMatrix math in pure C#",
          "category": "fix",
          "effort": "cost/s",
          "validated": "untested",
          "description": "Replace SkiaApi.sk_matrix_try_invert, sk_matrix_concat, sk_matrix_map_rect, sk_matrix_map_xy, sk_matrix_map_points, sk_matrix_map_vector, sk_matrix_map_vectors, sk_matrix_map_radius with equivalent C# implementations. Use System.Numerics.Matrix4x4 as a conversion bridge or implement direct 3x3 math."
        }
      ],
      "recommendedProposal": "proposal-1",
      "recommendedReason": "Directly eliminates the P/Invoke overhead on every matrix operation; already planned by maintainer (milestone 4.x RC 1, cost/s)."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.92,
      "reason": "Well-specified performance enhancement with benchmark evidence, already tracked on the 4.x RC 1 milestone. Triage confirms the native calls are still present and the managed implementation path is clear.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "id": "labels-1",
        "type": "update-labels",
        "description": "Apply type/enhancement, area/SkiaSharp, tenet/performance labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp",
          "tenet/performance"
        ]
      }
    ]
  }
}
```

</details>
