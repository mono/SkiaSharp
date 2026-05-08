# Issue Triage Report — #2431

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T17:19:00Z |
| Type | type/feature-request (0.95 (95%)) |
| Area | area/SkiaSharp (0.98 (98%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** Feature request to add SKVertices.CreateCopy overload accepting vertex/index offset and count parameters to enable zero-allocation slicing of shared vertex arrays.

**Analysis:** The current SKVertices.CreateCopy API requires full arrays and does not support offset/count slicing. The underlying native function sk_vertices_make_copy already accepts separate vertexCount and indexCount integer parameters and raw pointers, so a C# overload using pointer arithmetic (p + vertexOffset) is technically feasible. The reporter also submitted PR #2432 implementing this feature. The request is valid and low-risk: adding an overload is ABI-safe and avoids allocations when consuming third-party APIs that provide merged vertex/index buffers.

**Recommendations:** **needs-investigation** — Well-specified feature request with an associated implementation PR (#2432). Needs review of the PR for correctness before merging.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/performance |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/pull/2432 — PR submitted by reporter implementing the proposed overload

**Code snippets:**

```csharp
SKVertices CreateCopy (SKVertexMode vmode, int vertexOffset, int vertexCount, SKPoint[] positions, SKPoint[] texs, SKColor[] colors, int indexOffset, int indexCount, UInt16[] indices)
```

## Analysis

### Technical Summary

The current SKVertices.CreateCopy API requires full arrays and does not support offset/count slicing. The underlying native function sk_vertices_make_copy already accepts separate vertexCount and indexCount integer parameters and raw pointers, so a C# overload using pointer arithmetic (p + vertexOffset) is technically feasible. The reporter also submitted PR #2432 implementing this feature. The request is valid and low-risk: adding an overload is ABI-safe and avoids allocations when consuming third-party APIs that provide merged vertex/index buffers.

### Rationale

Classified as type/feature-request because the requested overload does not currently exist. The underlying native API already supports the offset pattern via pointer arithmetic. The reporter submitted PR #2432 with an implementation. The tenet/performance label is appropriate as the request is specifically to avoid extra allocations.

### Key Signals

- "the third-party API provides me with complete merged array of vertices and indexes that I want to use without additional allocations" — **issue body** (Real-world use case: consuming merged vertex buffers from external APIs without copying sub-arrays.)
- "PR: https://github.com/mono/SkiaSharp/pull/2432" — **issue body** (Reporter has already submitted an implementation PR.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKVertices.cs` | 25-54 | direct | Existing CreateCopy overloads always pass full arrays and use positions.Length as vertexCount. No overload accepts offset/count parameters. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | — | direct | sk_vertices_make_copy signature accepts SKVertexMode, Int32 vertexCount, SKPoint* positions, SKPoint* texs, UInt32* colors, Int32 indexCount, UInt16* indices — separate count params already support slicing via pointer arithmetic. |

### Resolution Proposals

**Hypothesis:** Add new CreateCopy overloads accepting vertexOffset, vertexCount, indexOffset, indexCount parameters, using pointer arithmetic on fixed arrays to pass offset pointers to the existing native call.

1. **Review and merge PR #2432** — fix, cost/s, validated=untested
   - The reporter submitted PR #2432 implementing the proposed overload. Review the PR for correctness (bounds checks, pointer arithmetic safety, ABI compliance) and merge if valid.
2. **Add Span<T>-based overloads as alternative** — alternative, cost/m, validated=untested
   - As a more idiomatic .NET alternative, expose overloads using ReadOnlySpan<T> with slice support, which avoids unsafe pointer arithmetic and aligns with modern .NET API design guidelines.

**Recommended proposal:** Review and merge PR #2432

**Why:** PR #2432 already implements the requested feature and is the fastest path to resolution.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Well-specified feature request with an associated implementation PR (#2432). Needs review of the PR for correctness before merging. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/feature-request label (issue currently only has area/SkiaSharp and tenet/performance) | labels=type/feature-request, area/SkiaSharp, tenet/performance |
| link-related | low | 0.98 (98%) | Link to PR #2432 which implements the requested feature | linkedIssue=#2432 |
| add-comment | medium | 0.85 (85%) | Acknowledge the feature request and note the associated PR | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the feature request and for submitting PR #2432 with an implementation!

The underlying `sk_vertices_make_copy` native call already supports separate vertex/index counts, so the offset-based approach via pointer arithmetic is technically feasible.

We'll review PR #2432 for correctness — specifically checking bounds validation and pointer arithmetic safety — before merging.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2431,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T17:19:00Z"
  },
  "summary": "Feature request to add SKVertices.CreateCopy overload accepting vertex/index offset and count parameters to enable zero-allocation slicing of shared vertex arrays.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.98
    },
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/2432",
          "description": "PR submitted by reporter implementing the proposed overload"
        }
      ],
      "codeSnippets": [
        "SKVertices CreateCopy (SKVertexMode vmode, int vertexOffset, int vertexCount, SKPoint[] positions, SKPoint[] texs, SKColor[] colors, int indexOffset, int indexCount, UInt16[] indices)"
      ]
    }
  },
  "analysis": {
    "summary": "The current SKVertices.CreateCopy API requires full arrays and does not support offset/count slicing. The underlying native function sk_vertices_make_copy already accepts separate vertexCount and indexCount integer parameters and raw pointers, so a C# overload using pointer arithmetic (p + vertexOffset) is technically feasible. The reporter also submitted PR #2432 implementing this feature. The request is valid and low-risk: adding an overload is ABI-safe and avoids allocations when consuming third-party APIs that provide merged vertex/index buffers.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKVertices.cs",
        "finding": "Existing CreateCopy overloads always pass full arrays and use positions.Length as vertexCount. No overload accepts offset/count parameters.",
        "relevance": "direct",
        "lines": "25-54"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "sk_vertices_make_copy signature accepts SKVertexMode, Int32 vertexCount, SKPoint* positions, SKPoint* texs, UInt32* colors, Int32 indexCount, UInt16* indices — separate count params already support slicing via pointer arithmetic.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "the third-party API provides me with complete merged array of vertices and indexes that I want to use without additional allocations",
        "source": "issue body",
        "interpretation": "Real-world use case: consuming merged vertex buffers from external APIs without copying sub-arrays."
      },
      {
        "text": "PR: https://github.com/mono/SkiaSharp/pull/2432",
        "source": "issue body",
        "interpretation": "Reporter has already submitted an implementation PR."
      }
    ],
    "rationale": "Classified as type/feature-request because the requested overload does not currently exist. The underlying native API already supports the offset pattern via pointer arithmetic. The reporter submitted PR #2432 with an implementation. The tenet/performance label is appropriate as the request is specifically to avoid extra allocations.",
    "resolution": {
      "hypothesis": "Add new CreateCopy overloads accepting vertexOffset, vertexCount, indexOffset, indexCount parameters, using pointer arithmetic on fixed arrays to pass offset pointers to the existing native call.",
      "proposals": [
        {
          "title": "Review and merge PR #2432",
          "description": "The reporter submitted PR #2432 implementing the proposed overload. Review the PR for correctness (bounds checks, pointer arithmetic safety, ABI compliance) and merge if valid.",
          "category": "fix",
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Add Span<T>-based overloads as alternative",
          "description": "As a more idiomatic .NET alternative, expose overloads using ReadOnlySpan<T> with slice support, which avoids unsafe pointer arithmetic and aligns with modern .NET API design guidelines.",
          "category": "alternative",
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Review and merge PR #2432",
      "recommendedReason": "PR #2432 already implements the requested feature and is the fastest path to resolution."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Well-specified feature request with an associated implementation PR (#2432). Needs review of the PR for correctness before merging.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/feature-request label (issue currently only has area/SkiaSharp and tenet/performance)",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "tenet/performance"
        ]
      },
      {
        "type": "link-related",
        "description": "Link to PR #2432 which implements the requested feature",
        "risk": "low",
        "confidence": 0.98,
        "linkedIssue": 2432
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the feature request and note the associated PR",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the feature request and for submitting PR #2432 with an implementation!\n\nThe underlying `sk_vertices_make_copy` native call already supports separate vertex/index counts, so the offset-based approach via pointer arithmetic is technically feasible.\n\nWe'll review PR #2432 for correctness — specifically checking bounds validation and pointer arithmetic safety — before merging."
      }
    ]
  }
}
```

</details>
