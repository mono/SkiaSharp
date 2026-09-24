# Issue Triage Report — #5051

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-09-24T04:27:54Z |
| Type | type/enhancement (0.93 (93%)) |
| Area | area/SkiaSharp (0.98 (98%)) |
| Suggested action | ready-to-fix (0.91 (91%)) |

**Issue Summary:** The issue proposes removing the temporary SKData allocation made by the byte-array and ReadOnlySpan SKImage.FromPixelCopy overloads while preserving their validation behavior, with benchmarks reporting lower managed allocation and improved small-image throughput on Linux/.NET 10.

**Analysis:** Both managed-buffer overloads currently copy their input into a temporary SKData and then use the data-backed raster path. A separate IntPtr overload already calls sk_image_new_raster_copy, which provides the direct native-copy route proposed by the issue. The current tests cover the byte-array happy path but do not cover the span overload or the invalid metadata, stride, and undersized-buffer equivalence cases described in the issue, so the implementation and expanded regression coverage in the linked pull request require review rather than further issue discovery.

**Recommendations:** **ready-to-fix** — The performance cause and implementation path are explicit, the current code confirms both the redundant data-backed path and the existing direct-copy primitive, and an open pull request provides the proposed change and regression coverage.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/performance |
| Perf | perf/allocations |
| Partner | — |
| Current labels | tenet/performance, partner/agentic-workflows, perf/allocations |

## Evidence

### Reproduction

**Environment:** BenchmarkDotNet on Linux x64 with .NET 10.0.11 and RyuJIT AVX2; the issue records two independent runs for 32x32 and 256x256 inputs.

**Repository links:**
- https://github.com/mono/SkiaSharp/pull/5052 — Open linked pull request, [performance] Optimize managed SKImage pixel copies.

## Analysis

### Technical Summary

Both managed-buffer overloads currently copy their input into a temporary SKData and then use the data-backed raster path. A separate IntPtr overload already calls sk_image_new_raster_copy, which provides the direct native-copy route proposed by the issue. The current tests cover the byte-array happy path but do not cover the span overload or the invalid metadata, stride, and undersized-buffer equivalence cases described in the issue, so the implementation and expanded regression coverage in the linked pull request require review rather than further issue discovery.

### Rationale

This is an improvement to an existing API's allocation profile rather than a behavior failure: the issue supplies measured benchmarks, a narrow implementation path, and an open linked pull request. The affected code is in the core managed wrapper and is platform-independent. Removing an avoidable allocation makes performance the applicable quality tenet and allocations the applicable performance subtype.

### Key Signals

- "Repeated managed-buffer image uploads pay for an avoidable native object plus managed wrapper on every call." — **issue body** (The requested change improves an existing hot path rather than adding a new API.)
- "32x32 measurements report 208 B before and 104 B after, with mean ratios of 0.62 in two runs." — **issue body benchmark table** (The primary impact is managed allocation reduction with a measurable small-input speed benefit.)
- "No public signature or ABI changes." — **issue body** (The proposed implementation is an internal optimization with no intended API-surface change.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKImage.cs` | 114-122 | direct | The byte[] overload validates null, creates SKData.CreateCopy(pixels), then calls the data-backed FromPixels overload inside a using block. |
| `binding/SkiaSharp/SKImage.cs` | 137-144 | direct | The IntPtr overload already converts SKImageInfo and calls SkiaApi.sk_image_new_raster_copy, the direct native raster-copy API identified by the issue. |
| `binding/SkiaSharp/SKImage.cs` | 173-178 | direct | The ReadOnlySpan<byte> overload has the same SKData.CreateCopy followed by FromPixels pattern as the byte[] overload. |
| `tests/Tests/SkiaSharp/SKImageTest.cs` | 242-255 | related | The existing byte-array test verifies a normal image copy, but it does not exercise the span overload or validation-equivalence edge cases described in the issue. |

### Workarounds

- No fully managed public overload currently avoids the temporary SKData allocation; callers that already own an unmanaged pixel buffer can use the existing IntPtr overload, which takes the direct raster-copy path.

### Next Questions

- Does the linked implementation preserve the existing null-array exception and native failure behavior for invalid rowBytes, empty image information, and undersized inputs?
- Do the expanded equivalence tests cover both byte-array and ReadOnlySpan<byte> overloads across all supported target frameworks?

### Resolution Proposals

**Hypothesis:** Routing valid managed buffers to the existing native raster-copy function can remove the temporary SKData native object and managed wrapper while retaining the required pixel copy.

1. **Review and land the direct raster-copy optimization** — fix, confidence 0.90 (90%), cost/s, validated=untested
   - Review the linked pull request's valid-input fast path and its fallback to the existing data-backed path for invalid or undersized inputs, together with the proposed equivalence and allocation regression coverage.

**Recommended proposal:** Review and land the direct raster-copy optimization

**Why:** The issue identifies an existing direct-copy interop path, includes repeatable benchmark evidence, and links a concrete implementation that must preserve current edge-case semantics.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.91 (91%) |
| Reason | The performance cause and implementation path are explicit, the current code confirms both the redundant data-backed path and the existing direct-copy primitive, and an open pull request provides the proposed change and regression coverage. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.98 (98%) | Apply enhancement, core SkiaSharp, performance, and allocation labels. | labels=type/enhancement, area/SkiaSharp, tenet/performance, perf/allocations |
| link-related | low | 0.99 (99%) | Keep the existing linked implementation visible during review. | linkedIssue=#5052 |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 5051,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-09-24T04:27:54Z",
    "currentLabels": [
      "tenet/performance",
      "partner/agentic-workflows",
      "perf/allocations"
    ]
  },
  "summary": "The issue proposes removing the temporary SKData allocation made by the byte-array and ReadOnlySpan SKImage.FromPixelCopy overloads while preserving their validation behavior, with benchmarks reporting lower managed allocation and improved small-image throughput on Linux/.NET 10.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.93
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.98
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
      "environmentDetails": "BenchmarkDotNet on Linux x64 with .NET 10.0.11 and RyuJIT AVX2; the issue records two independent runs for 32x32 and 256x256 inputs.",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/5052",
          "description": "Open linked pull request, [performance] Optimize managed SKImage pixel copies."
        }
      ]
    }
  },
  "analysis": {
    "summary": "Both managed-buffer overloads currently copy their input into a temporary SKData and then use the data-backed raster path. A separate IntPtr overload already calls sk_image_new_raster_copy, which provides the direct native-copy route proposed by the issue. The current tests cover the byte-array happy path but do not cover the span overload or the invalid metadata, stride, and undersized-buffer equivalence cases described in the issue, so the implementation and expanded regression coverage in the linked pull request require review rather than further issue discovery.",
    "rationale": "This is an improvement to an existing API's allocation profile rather than a behavior failure: the issue supplies measured benchmarks, a narrow implementation path, and an open linked pull request. The affected code is in the core managed wrapper and is platform-independent. Removing an avoidable allocation makes performance the applicable quality tenet and allocations the applicable performance subtype.",
    "keySignals": [
      {
        "text": "Repeated managed-buffer image uploads pay for an avoidable native object plus managed wrapper on every call.",
        "source": "issue body",
        "interpretation": "The requested change improves an existing hot path rather than adding a new API."
      },
      {
        "text": "32x32 measurements report 208 B before and 104 B after, with mean ratios of 0.62 in two runs.",
        "source": "issue body benchmark table",
        "interpretation": "The primary impact is managed allocation reduction with a measurable small-input speed benefit."
      },
      {
        "text": "No public signature or ABI changes.",
        "source": "issue body",
        "interpretation": "The proposed implementation is an internal optimization with no intended API-surface change."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "114-122",
        "finding": "The byte[] overload validates null, creates SKData.CreateCopy(pixels), then calls the data-backed FromPixels overload inside a using block.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "137-144",
        "finding": "The IntPtr overload already converts SKImageInfo and calls SkiaApi.sk_image_new_raster_copy, the direct native raster-copy API identified by the issue.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "173-178",
        "finding": "The ReadOnlySpan<byte> overload has the same SKData.CreateCopy followed by FromPixels pattern as the byte[] overload.",
        "relevance": "direct"
      },
      {
        "file": "tests/Tests/SkiaSharp/SKImageTest.cs",
        "lines": "242-255",
        "finding": "The existing byte-array test verifies a normal image copy, but it does not exercise the span overload or validation-equivalence edge cases described in the issue.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "No fully managed public overload currently avoids the temporary SKData allocation; callers that already own an unmanaged pixel buffer can use the existing IntPtr overload, which takes the direct raster-copy path."
    ],
    "nextQuestions": [
      "Does the linked implementation preserve the existing null-array exception and native failure behavior for invalid rowBytes, empty image information, and undersized inputs?",
      "Do the expanded equivalence tests cover both byte-array and ReadOnlySpan<byte> overloads across all supported target frameworks?"
    ],
    "resolution": {
      "hypothesis": "Routing valid managed buffers to the existing native raster-copy function can remove the temporary SKData native object and managed wrapper while retaining the required pixel copy.",
      "proposals": [
        {
          "title": "Review and land the direct raster-copy optimization",
          "description": "Review the linked pull request's valid-input fast path and its fallback to the existing data-backed path for invalid or undersized inputs, together with the proposed equivalence and allocation regression coverage.",
          "category": "fix",
          "validated": "untested",
          "confidence": 0.9,
          "effort": "cost/s"
        }
      ],
      "recommendedProposal": "Review and land the direct raster-copy optimization",
      "recommendedReason": "The issue identifies an existing direct-copy interop path, includes repeatable benchmark evidence, and links a concrete implementation that must preserve current edge-case semantics."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.91,
      "reason": "The performance cause and implementation path are explicit, the current code confirms both the redundant data-backed path and the existing direct-copy primitive, and an open pull request provides the proposed change and regression coverage.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement, core SkiaSharp, performance, and allocation labels.",
        "risk": "low",
        "confidence": 0.98,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp",
          "tenet/performance",
          "perf/allocations"
        ]
      },
      {
        "type": "link-related",
        "description": "Keep the existing linked implementation visible during review.",
        "risk": "low",
        "confidence": 0.99,
        "linkedIssue": 5052
      }
    ]
  }
}
```

</details>
