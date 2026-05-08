# Issue Triage Report — #1687

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T17:22:37Z |
| Type | type/feature-request (0.95 (95%)) |
| Area | area/SkiaSharp (0.98 (98%)) |
| Suggested action | keep-open (0.88 (88%)) |

**Issue Summary:** Feature request to expose SkVertices::Builder and add Span overloads to SKVertices.CreateCopy for more efficient vertex buffer construction.

**Analysis:** The current SKVertices.CreateCopy API only accepts arrays, forcing callers to allocate new arrays when they want to copy a subset of a larger shared buffer. The upstream Skia C++ API exposes a SkVertices::Builder class for incremental vertex buffer construction, which is not bound in SkiaSharp. Adding Span<T> overloads would allow offset/length control without extra allocations.

**Recommendations:** **keep-open** — Well-specified feature request with clear implementation path. Suitable to track as enhancement backlog item. No blocker or repro needed.

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

## Analysis

### Technical Summary

The current SKVertices.CreateCopy API only accepts arrays, forcing callers to allocate new arrays when they want to copy a subset of a larger shared buffer. The upstream Skia C++ API exposes a SkVertices::Builder class for incremental vertex buffer construction, which is not bound in SkiaSharp. Adding Span<T> overloads would allow offset/length control without extra allocations.

### Rationale

This is a clear feature-request for new API surface (Span overloads + Builder class). The current code-path is confirmed to lack Span support and count parameters. No duplicate found. The request is well-specified and has an obvious implementation path.

### Key Signals

- "vertex and index counts are not present in the API" — **issue body** (Caller cannot specify a subset of a shared array — must always copy the whole array.)
- "Vertex and index counts are needed when a shared array is used or we want to copy only a subset. This forces user to allocate new arrays frequently" — **issue body** (Performance problem: unnecessary heap allocations when using subset of a large buffer.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKVertices.cs` | 25-58 | direct | SKVertices.CreateCopy has four overloads, all accepting plain arrays (SKPoint[], SKColor[], UInt16[]). No Span<T> overloads exist. The C# wrapper hard-codes vertexCount = positions.Length and indexCount = indices?.Length, preventing subset copies. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 16984-17022 | direct | Only sk_vertices_make_copy is exposed in the generated C API bindings. There is no binding for any SkVertices::Builder-equivalent C function, confirming the builder class is not yet wrapped. |

### Resolution Proposals

**Hypothesis:** Add ReadOnlySpan<T> overloads to SKVertices.CreateCopy accepting explicit vertex/index counts, and optionally bind SkVertices::Builder through a new C API shim.

1. **Add Span overloads to SKVertices.CreateCopy** — fix, cost/m, validated=untested
   - Add overloads accepting ReadOnlySpan<SKPoint>, ReadOnlySpan<SKColor>, ReadOnlySpan<UInt16> with explicit count parameters so callers can pass sub-ranges of larger buffers without extra array allocation.
2. **Expose SkVertices::Builder via C API and C# wrapper** — fix, cost/l, validated=untested
   - Add C API shim functions for SkVertices::Builder (allocate, set positions/texcoords/colors/indices, detach) and a corresponding SKVertices.Builder C# class.

**Recommended proposal:** span-overloads

**Why:** Span overloads are a pure C# change with no native rebuild required and directly address the reported performance problem. The Builder class is a larger effort requiring C API changes.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.88 (88%) |
| Reason | Well-specified feature request with clear implementation path. Suitable to track as enhancement backlog item. No blocker or repro needed. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/feature-request, area/SkiaSharp, tenet/performance | labels=type/feature-request, area/SkiaSharp, tenet/performance |
| add-comment | medium | 0.85 (85%) | Acknowledge the request and confirm both proposals (Span overloads + Builder binding) are valid improvements | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed feature request!

You're right that the current `SKVertices.CreateCopy` API forces callers to allocate fresh arrays even when they only need a subset of a shared buffer. Two improvements would address this:

1. **Span overloads** — `CreateCopy(SKVertexMode mode, ReadOnlySpan<SKPoint> positions, ...)` with explicit `vertexCount`/`indexCount` parameters. This is a pure C# change.
2. **Builder class** — Exposing `SkVertices::Builder` via a new C API shim and a `SKVertices.Builder` wrapper class for incremental construction.

Keeping this open to track both improvements.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1687,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T17:22:37Z"
  },
  "summary": "Feature request to expose SkVertices::Builder and add Span overloads to SKVertices.CreateCopy for more efficient vertex buffer construction.",
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
      "codeSnippets": [],
      "attachments": [],
      "repoLinks": []
    }
  },
  "analysis": {
    "summary": "The current SKVertices.CreateCopy API only accepts arrays, forcing callers to allocate new arrays when they want to copy a subset of a larger shared buffer. The upstream Skia C++ API exposes a SkVertices::Builder class for incremental vertex buffer construction, which is not bound in SkiaSharp. Adding Span<T> overloads would allow offset/length control without extra allocations.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKVertices.cs",
        "finding": "SKVertices.CreateCopy has four overloads, all accepting plain arrays (SKPoint[], SKColor[], UInt16[]). No Span<T> overloads exist. The C# wrapper hard-codes vertexCount = positions.Length and indexCount = indices?.Length, preventing subset copies.",
        "relevance": "direct",
        "lines": "25-58"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "Only sk_vertices_make_copy is exposed in the generated C API bindings. There is no binding for any SkVertices::Builder-equivalent C function, confirming the builder class is not yet wrapped.",
        "relevance": "direct",
        "lines": "16984-17022"
      }
    ],
    "keySignals": [
      {
        "text": "vertex and index counts are not present in the API",
        "source": "issue body",
        "interpretation": "Caller cannot specify a subset of a shared array — must always copy the whole array."
      },
      {
        "text": "Vertex and index counts are needed when a shared array is used or we want to copy only a subset. This forces user to allocate new arrays frequently",
        "source": "issue body",
        "interpretation": "Performance problem: unnecessary heap allocations when using subset of a large buffer."
      }
    ],
    "rationale": "This is a clear feature-request for new API surface (Span overloads + Builder class). The current code-path is confirmed to lack Span support and count parameters. No duplicate found. The request is well-specified and has an obvious implementation path.",
    "resolution": {
      "hypothesis": "Add ReadOnlySpan<T> overloads to SKVertices.CreateCopy accepting explicit vertex/index counts, and optionally bind SkVertices::Builder through a new C API shim.",
      "proposals": [
        {
          "title": "Add Span overloads to SKVertices.CreateCopy",
          "description": "Add overloads accepting ReadOnlySpan<SKPoint>, ReadOnlySpan<SKColor>, ReadOnlySpan<UInt16> with explicit count parameters so callers can pass sub-ranges of larger buffers without extra array allocation.",
          "category": "fix",
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Expose SkVertices::Builder via C API and C# wrapper",
          "description": "Add C API shim functions for SkVertices::Builder (allocate, set positions/texcoords/colors/indices, detach) and a corresponding SKVertices.Builder C# class.",
          "category": "fix",
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "span-overloads",
      "recommendedReason": "Span overloads are a pure C# change with no native rebuild required and directly address the reported performance problem. The Builder class is a larger effort requiring C API changes."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.88,
      "reason": "Well-specified feature request with clear implementation path. Suitable to track as enhancement backlog item. No blocker or repro needed.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/feature-request, area/SkiaSharp, tenet/performance",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "tenet/performance"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the request and confirm both proposals (Span overloads + Builder binding) are valid improvements",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed feature request!\n\nYou're right that the current `SKVertices.CreateCopy` API forces callers to allocate fresh arrays even when they only need a subset of a shared buffer. Two improvements would address this:\n\n1. **Span overloads** — `CreateCopy(SKVertexMode mode, ReadOnlySpan<SKPoint> positions, ...)` with explicit `vertexCount`/`indexCount` parameters. This is a pure C# change.\n2. **Builder class** — Exposing `SkVertices::Builder` via a new C API shim and a `SKVertices.Builder` wrapper class for incremental construction.\n\nKeeping this open to track both improvements."
      }
    ]
  }
}
```

</details>
