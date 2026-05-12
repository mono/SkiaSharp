# Issue Triage Report — #1288

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-12T05:25:06Z |
| Type | type/enhancement (0.90 (90%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | ready-to-fix (0.85 (85%)) |

**Issue Summary:** Request to call GC.AddMemoryPressure / GC.RemoveMemoryPressure for large native objects (bitmaps, images, data) so the .NET GC can make better collection decisions.

**Analysis:** The SkiaSharp binding wraps large native allocations (SKBitmap pixels, SKData buffers, SKImage pixels) without informing the .NET GC of native memory pressure. Adding GC.AddMemoryPressure on allocation and GC.RemoveMemoryPressure on dispose would allow the GC to schedule collections more aggressively when large native buffers accumulate. The maintainer has already done a feasibility investigation and confirmed the approach is sound for SKBitmap and SKData (known exact sizes), with caveats around shared pixel refs (SKPixelRef) and GPU-backed surfaces where sizes are uncertain.

**Recommendations:** **ready-to-fix** — The maintainer has already performed a feasibility investigation, confirmed the approach, scoped it to cost/s, and assigned it to the 4.x milestone. The implementation path for SKBitmap and SKData is clear.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/performance, tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | No GC.AddMemoryPressure calls exist anywhere in the binding codebase as of 2026-05. |

## Analysis

### Technical Summary

The SkiaSharp binding wraps large native allocations (SKBitmap pixels, SKData buffers, SKImage pixels) without informing the .NET GC of native memory pressure. Adding GC.AddMemoryPressure on allocation and GC.RemoveMemoryPressure on dispose would allow the GC to schedule collections more aggressively when large native buffers accumulate. The maintainer has already done a feasibility investigation and confirmed the approach is sound for SKBitmap and SKData (known exact sizes), with caveats around shared pixel refs (SKPixelRef) and GPU-backed surfaces where sizes are uncertain.

### Rationale

This is a genuine performance/reliability enhancement. No GC.AddMemoryPressure calls exist in the codebase. The maintainer has already confirmed the approach and done an investigation. The milestone is 4.x RC 1 and it already has cost/s and tenet/performance labels, indicating it's tracked and scoped. The complexity around shared pixel refs (SKPixelRef) and GPU-backed surfaces warrants careful implementation, but the fix path for SKBitmap+SKData is well-understood.

### Key Signals

- "We probably should do this soon. We have image, bitmap, surface, data and memory streams." — **maintainer comment #629004594** (Maintainer confirmed the enhancement is valid and desirable.)
- "One really needs to track the memory usage of the underlying shared pixel buffer... SKPixelRef should be tracked" — **reporter comment #643260349** (Shared pixel refs between bitmaps complicate exact tracking; naive per-object tracking may double-count.)
- "Start with SKBitmap + SKData (known exact sizes). Skip GPU-backed objects where size is uncertain." — **maintainer investigation comment #4415454463** (Feasibility study already done; incremental approach recommended.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | — | direct | No GC.AddMemoryPressure call exists. ByteCount property (RowBytes × Height) gives exact native allocation size, suitable for memory pressure reporting. |
| `binding/SkiaSharp/SKData.cs` | — | direct | No GC.AddMemoryPressure call. SKData.Size gives the exact byte count of the native buffer. Dispose/DisposeNative pattern is present. |

### Resolution Proposals

**Hypothesis:** Add GC.AddMemoryPressure(ByteCount) in SKBitmap's TryAllocPixels and GC.RemoveMemoryPressure in DisposeNative. Do the same for SKData using .Size. Handle the shared-pixel-ref complexity carefully to avoid double-counting.

1. **Add GC memory pressure for SKBitmap and SKData** — fix, cost/s, validated=untested
   - Call GC.AddMemoryPressure(ByteCount) on successful allocation in SKBitmap and GC.AddMemoryPressure(Size) in SKData.Create. Call GC.RemoveMemoryPressure in corresponding DisposeNative methods.

**Recommended proposal:** prop-1

**Why:** Maintainer has already investigated and confirmed this is the right approach. SKBitmap.ByteCount and SKData.Size provide exact native allocation sizes.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.85 (85%) |
| Reason | The maintainer has already performed a feasibility investigation, confirmed the approach, scoped it to cost/s, and assigned it to the 4.x milestone. The implementation path for SKBitmap and SKData is clear. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/enhancement, area/SkiaSharp, tenet/performance, tenet/reliability | labels=type/enhancement, area/SkiaSharp, tenet/performance, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Summarize investigation findings and implementation path | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for filing this! After reviewing the codebase and the maintainer's feasibility investigation, this is confirmed as a valid enhancement.

**Current state:** Zero calls to `GC.AddMemoryPressure` anywhere in the binding. Large native allocations in `SKBitmap`, `SKData`, and `SKImage` are invisible to the .NET GC.

**Recommended implementation path:**
- `SKBitmap`: Call `GC.AddMemoryPressure(ByteCount)` after successful `TryAllocPixels`, and `GC.RemoveMemoryPressure(ByteCount)` in `DisposeNative`.
- `SKData`: Call `GC.AddMemoryPressure(Size)` on create, `GC.RemoveMemoryPressure(Size)` in dispose.
- Skip GPU-backed surfaces and objects with uncertain sizes to avoid over-reporting.
- Track the pressure amount as a field to ensure add/remove symmetry.

This is assigned to the 4.x milestone with `cost/s`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1288,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-12T05:25:06Z"
  },
  "summary": "Request to call GC.AddMemoryPressure / GC.RemoveMemoryPressure for large native objects (bitmaps, images, data) so the .NET GC can make better collection decisions.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/performance",
      "tenet/reliability"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "No GC.AddMemoryPressure calls exist anywhere in the binding codebase as of 2026-05."
    }
  },
  "analysis": {
    "summary": "The SkiaSharp binding wraps large native allocations (SKBitmap pixels, SKData buffers, SKImage pixels) without informing the .NET GC of native memory pressure. Adding GC.AddMemoryPressure on allocation and GC.RemoveMemoryPressure on dispose would allow the GC to schedule collections more aggressively when large native buffers accumulate. The maintainer has already done a feasibility investigation and confirmed the approach is sound for SKBitmap and SKData (known exact sizes), with caveats around shared pixel refs (SKPixelRef) and GPU-backed surfaces where sizes are uncertain.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "finding": "No GC.AddMemoryPressure call exists. ByteCount property (RowBytes × Height) gives exact native allocation size, suitable for memory pressure reporting.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKData.cs",
        "finding": "No GC.AddMemoryPressure call. SKData.Size gives the exact byte count of the native buffer. Dispose/DisposeNative pattern is present.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "We probably should do this soon. We have image, bitmap, surface, data and memory streams.",
        "source": "maintainer comment #629004594",
        "interpretation": "Maintainer confirmed the enhancement is valid and desirable."
      },
      {
        "text": "One really needs to track the memory usage of the underlying shared pixel buffer... SKPixelRef should be tracked",
        "source": "reporter comment #643260349",
        "interpretation": "Shared pixel refs between bitmaps complicate exact tracking; naive per-object tracking may double-count."
      },
      {
        "text": "Start with SKBitmap + SKData (known exact sizes). Skip GPU-backed objects where size is uncertain.",
        "source": "maintainer investigation comment #4415454463",
        "interpretation": "Feasibility study already done; incremental approach recommended."
      }
    ],
    "rationale": "This is a genuine performance/reliability enhancement. No GC.AddMemoryPressure calls exist in the codebase. The maintainer has already confirmed the approach and done an investigation. The milestone is 4.x RC 1 and it already has cost/s and tenet/performance labels, indicating it's tracked and scoped. The complexity around shared pixel refs (SKPixelRef) and GPU-backed surfaces warrants careful implementation, but the fix path for SKBitmap+SKData is well-understood.",
    "resolution": {
      "hypothesis": "Add GC.AddMemoryPressure(ByteCount) in SKBitmap's TryAllocPixels and GC.RemoveMemoryPressure in DisposeNative. Do the same for SKData using .Size. Handle the shared-pixel-ref complexity carefully to avoid double-counting.",
      "proposals": [
        {
          "id": "prop-1",
          "title": "Add GC memory pressure for SKBitmap and SKData",
          "category": "fix",
          "effort": "cost/s",
          "description": "Call GC.AddMemoryPressure(ByteCount) on successful allocation in SKBitmap and GC.AddMemoryPressure(Size) in SKData.Create. Call GC.RemoveMemoryPressure in corresponding DisposeNative methods.",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "prop-1",
      "recommendedReason": "Maintainer has already investigated and confirmed this is the right approach. SKBitmap.ByteCount and SKData.Size provide exact native allocation sizes."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.85,
      "reason": "The maintainer has already performed a feasibility investigation, confirmed the approach, scoped it to cost/s, and assigned it to the 4.x milestone. The implementation path for SKBitmap and SKData is clear.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "id": "labels-1",
        "type": "update-labels",
        "description": "Apply type/enhancement, area/SkiaSharp, tenet/performance, tenet/reliability",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp",
          "tenet/performance",
          "tenet/reliability"
        ]
      },
      {
        "id": "comment-1",
        "type": "add-comment",
        "description": "Summarize investigation findings and implementation path",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for filing this! After reviewing the codebase and the maintainer's feasibility investigation, this is confirmed as a valid enhancement.\n\n**Current state:** Zero calls to `GC.AddMemoryPressure` anywhere in the binding. Large native allocations in `SKBitmap`, `SKData`, and `SKImage` are invisible to the .NET GC.\n\n**Recommended implementation path:**\n- `SKBitmap`: Call `GC.AddMemoryPressure(ByteCount)` after successful `TryAllocPixels`, and `GC.RemoveMemoryPressure(ByteCount)` in `DisposeNative`.\n- `SKData`: Call `GC.AddMemoryPressure(Size)` on create, `GC.RemoveMemoryPressure(Size)` in dispose.\n- Skip GPU-backed surfaces and objects with uncertain sizes to avoid over-reporting.\n- Track the pressure amount as a field to ensure add/remove symmetry.\n\nThis is assigned to the 4.x milestone with `cost/s`."
      }
    ]
  }
}
```

</details>
