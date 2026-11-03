# Issue Triage Report — #4484

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-11-03T04:17:36Z |
| Type | type/enhancement (0.96 (96%)) |
| Area | area/HarfBuzzSharp (0.99 (99%)) |
| Suggested action | close-as-fixed (0.99 (99%)) |

**Issue Summary:** The automated report identified a redundant Feature[] copy in HarfBuzzSharp.Font.Shape that allocated once per shaped text run when the caller supplied the common params array.

**Analysis:** The original unconditional ToArray materialization has been replaced with an array fast path. For the common params Feature[] overload, Font.Shape now pins the caller array directly; non-array IReadOnlyList inputs preserve the prior materialization behavior. The generated hb_shape_full declaration documents a const feature pointer, and the repository now includes equivalence tests plus a benchmark covering the optimization.

**Recommendations:** **close-as-fixed** — Current source, dedicated tests, benchmark coverage, and the merged resolution for duplicate #4689 confirm that the reported allocation optimization is already implemented.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/HarfBuzzSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/performance |
| Perf | perf/allocations |
| Partner | — |
| Current labels | tenet/performance, partner/agentic-workflows, perf/allocations |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/4689 — Closed duplicate performance report with measured benchmark and equivalence-test evidence for the same Font.Shape allocation.
- https://github.com/mono/SkiaSharp/pull/4690 — Merged pull request that implemented the exact proposed Feature[] fast path.

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | The checked-out implementation already uses the proposed array fast path, with a fallback copy only for non-array IReadOnlyList implementations. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.99 (99%) |
| Reason | The current Font.Shape implementation contains the exact proposed Feature[] fast path; closed issue #4689 records a measured allocation reduction and its merged PR #4690 as the resolution. |
| Related PRs | #4690 |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

The original unconditional ToArray materialization has been replaced with an array fast path. For the common params Feature[] overload, Font.Shape now pins the caller array directly; non-array IReadOnlyList inputs preserve the prior materialization behavior. The generated hb_shape_full declaration documents a const feature pointer, and the repository now includes equivalence tests plus a benchmark covering the optimization.

### Rationale

This is an enhancement to the managed HarfBuzzSharp binding's allocation behavior, not a platform-specific rendering failure. The issue is already resolved: current source matches the requested body-only change, the duplicate report #4689 was closed after merged PR #4690, and the repository contains direct test and benchmark coverage.

### Key Signals

- "var featuresArray = features as Feature[] ?? features?.ToArray ();" — **binding/HarfBuzzSharp/Font.cs** (The proposed fast path is present in the current implementation.)
- "Closed duplicate report #4689 identifies merged PR #4690 as the fix and records measured 72 B to ~0 B allocation reduction." — **GitHub issue #4689** (The same optimization was implemented and empirically validated after this earlier hypothesis.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/HarfBuzzSharp/Font.cs` | 673-692 | direct | The params Feature[] overload forwards to the IReadOnlyList overload, which now uses an array fast path before pinning and calling hb_shape_full; non-array lists retain the ToArray fallback. |
| `binding/HarfBuzzSharp/Generated/HarfBuzzApi_src_hb-shape_h.generated.cs` | 1-8 | direct | The generated native declaration documents hb_shape_full with const hb_feature_t* features, supporting the direct pinning of an existing Feature[] for the call. |
| `tests/Tests/HarfBuzzSharp/HBFontShapeFeaturesTest.cs` | 9-93 | direct | Equivalence tests compare array and non-array IReadOnlyList feature inputs across empty, populated, and null cases, asserting identical glyph information and positions. |
| `benchmarks/SkiaSharp.Benchmarks/Benchmarks/HarfBuzzShapeFeaturesBenchmark.cs` | 10-107 | related | The benchmark contrasts direct Feature[] input with a non-array wrapper to measure the avoided copy while preserving the same Font.Shape overload and shaping setup. |

### Workarounds

- No workaround is needed because the redundant copy has already been removed for Feature[] callers.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.99 (99%) |
| Reason | Current source, dedicated tests, benchmark coverage, and the merged resolution for duplicate #4689 confirm that the reported allocation optimization is already implemented. |
| Suggested repro platform | — |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.99 (99%) | Apply the enhancement, HarfBuzzSharp, and performance allocation classification labels. | labels=type/enhancement, area/HarfBuzzSharp, tenet/performance, perf/allocations |
| link-related | low | 0.99 (99%) | Link the implemented duplicate report #4689 for the benchmark and merged-fix evidence. | linkedIssue=#4689 |
| close-issue | medium | 0.99 (99%) | Close as fixed because the requested optimization is in the current source and was merged through PR #4690. | stateReason=completed |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4484,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-11-03T04:17:36Z",
    "currentLabels": [
      "tenet/performance",
      "partner/agentic-workflows",
      "perf/allocations"
    ]
  },
  "summary": "The automated report identified a redundant Feature[] copy in HarfBuzzSharp.Font.Shape that allocated once per shaped text run when the caller supplied the common params array.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.96
    },
    "area": {
      "value": "area/HarfBuzzSharp",
      "confidence": 0.99
    },
    "tenets": [
      "tenet/performance"
    ],
    "perf": [
      "perf/allocations"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/4689",
          "description": "Closed duplicate performance report with measured benchmark and equivalence-test evidence for the same Font.Shape allocation."
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/4690",
          "description": "Merged pull request that implemented the exact proposed Feature[] fast path."
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "unlikely",
      "relevanceReason": "The checked-out implementation already uses the proposed array fast path, with a fallback copy only for non-array IReadOnlyList implementations."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.99,
      "reason": "The current Font.Shape implementation contains the exact proposed Feature[] fast path; closed issue #4689 records a measured allocation reduction and its merged PR #4690 as the resolution.",
      "relatedPRs": [
        4690
      ]
    }
  },
  "analysis": {
    "summary": "The original unconditional ToArray materialization has been replaced with an array fast path. For the common params Feature[] overload, Font.Shape now pins the caller array directly; non-array IReadOnlyList inputs preserve the prior materialization behavior. The generated hb_shape_full declaration documents a const feature pointer, and the repository now includes equivalence tests plus a benchmark covering the optimization.",
    "rationale": "This is an enhancement to the managed HarfBuzzSharp binding's allocation behavior, not a platform-specific rendering failure. The issue is already resolved: current source matches the requested body-only change, the duplicate report #4689 was closed after merged PR #4690, and the repository contains direct test and benchmark coverage.",
    "keySignals": [
      {
        "text": "var featuresArray = features as Feature[] ?? features?.ToArray ();",
        "source": "binding/HarfBuzzSharp/Font.cs",
        "interpretation": "The proposed fast path is present in the current implementation."
      },
      {
        "text": "Closed duplicate report #4689 identifies merged PR #4690 as the fix and records measured 72 B to ~0 B allocation reduction.",
        "source": "GitHub issue #4689",
        "interpretation": "The same optimization was implemented and empirically validated after this earlier hypothesis."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/HarfBuzzSharp/Font.cs",
        "lines": "673-692",
        "finding": "The params Feature[] overload forwards to the IReadOnlyList overload, which now uses an array fast path before pinning and calling hb_shape_full; non-array lists retain the ToArray fallback.",
        "relevance": "direct"
      },
      {
        "file": "binding/HarfBuzzSharp/Generated/HarfBuzzApi_src_hb-shape_h.generated.cs",
        "lines": "1-8",
        "finding": "The generated native declaration documents hb_shape_full with const hb_feature_t* features, supporting the direct pinning of an existing Feature[] for the call.",
        "relevance": "direct"
      },
      {
        "file": "tests/Tests/HarfBuzzSharp/HBFontShapeFeaturesTest.cs",
        "lines": "9-93",
        "finding": "Equivalence tests compare array and non-array IReadOnlyList feature inputs across empty, populated, and null cases, asserting identical glyph information and positions.",
        "relevance": "direct"
      },
      {
        "file": "benchmarks/SkiaSharp.Benchmarks/Benchmarks/HarfBuzzShapeFeaturesBenchmark.cs",
        "lines": "10-107",
        "finding": "The benchmark contrasts direct Feature[] input with a non-array wrapper to measure the avoided copy while preserving the same Font.Shape overload and shaping setup.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "No workaround is needed because the redundant copy has already been removed for Feature[] callers."
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.99,
      "reason": "Current source, dedicated tests, benchmark coverage, and the merged resolution for duplicate #4689 confirm that the reported allocation optimization is already implemented."
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply the enhancement, HarfBuzzSharp, and performance allocation classification labels.",
        "risk": "low",
        "confidence": 0.99,
        "labels": [
          "type/enhancement",
          "area/HarfBuzzSharp",
          "tenet/performance",
          "perf/allocations"
        ]
      },
      {
        "type": "link-related",
        "description": "Link the implemented duplicate report #4689 for the benchmark and merged-fix evidence.",
        "risk": "low",
        "confidence": 0.99,
        "linkedIssue": 4689
      },
      {
        "type": "close-issue",
        "description": "Close as fixed because the requested optimization is in the current source and was merged through PR #4690.",
        "risk": "medium",
        "confidence": 0.99,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
