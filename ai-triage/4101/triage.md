# Issue Triage Report — #4101

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-31T05:45:00Z |
| Type | type/enhancement (0.95 (95%)) |
| Area | area/SkiaSharp (0.98 (98%)) |
| Suggested action | keep-open (0.88 (88%)) |

**Issue Summary:** Proposal to investigate lock striping/sharding for HandleDictionary to reduce contention in highly parallel workloads that create/dispose many distinct SKObject wrappers concurrently.

**Analysis:** The issue proposes partitioning the single global HandleDictionary lock into N independently-locked shards keyed by a hash of the native handle. Microbenchmark data shows 2.5–3.7x contention reduction at 8–16 threads for distinct-handle workloads, with zero single-thread overhead, but potential harm for shared-singleton workloads and a new cross-shard deadlock hazard from the factory-under-lock pattern. The author explicitly marks it exploratory and gated on an in-tree benchmark with real SKObjects.

**Recommendations:** **keep-open** — Well-specified performance enhancement proposal with benchmark data, design sketch, kill criteria, and explicit exploratory status. Needs in-tree benchmark and deadlock resolution before implementation. No urgency — non-blocking for #4080.

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
| Current labels | enhancement |

## Evidence

### Reproduction

**Environment:** Observed at 8–16+ threads; standalone microbenchmark provided inline

**Related issues:** #4080, #3817

## Analysis

### Technical Summary

The issue proposes partitioning the single global HandleDictionary lock into N independently-locked shards keyed by a hash of the native handle. Microbenchmark data shows 2.5–3.7x contention reduction at 8–16 threads for distinct-handle workloads, with zero single-thread overhead, but potential harm for shared-singleton workloads and a new cross-shard deadlock hazard from the factory-under-lock pattern. The author explicitly marks it exploratory and gated on an in-tree benchmark with real SKObjects.

### Rationale

This is a performance enhancement to existing lock infrastructure (HandleDictionary), not a bug fix or new feature. The existing design (one lock + upgradeable-read pattern) is confirmed by code inspection. The issue is well-specified with benchmark data, design sketch, and explicit kill criteria. It cross-links #4080 (which handles correctness) and is filed as a future, non-blocking investigation.

### Key Signals

- "Status: exploratory. A standalone microbenchmark shows real but conditional value." — **issue body** (Author self-classifies as exploratory investigation, not a concrete implementation request.)
- "2.5–3.7× contention reduction for high-thread parallel churn of distinct objects, with zero single-thread cost — but no benefit (slight harm) for shared-singleton hot paths" — **issue body** (Real performance benefit documented with benchmark data, but scope is conditional.)
- "Non-blocking for #4080." — **issue body** (Decoupled from the correctness work in #4080; can be addressed independently.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/HandleDictionary.cs` | 21-28 | direct | Single process-wide lock: `internal static readonly IPlatformLock instancesLock = PlatformLock.Create()` guards one `Dictionary<IntPtr, WeakReference> instances`. All GetInstance, GetOrAddObject, RegisterHandle, and DeregisterHandle calls serialise through this lock. |
| `binding/SkiaSharp/HandleDictionary.cs` | 74-97 | direct | GetOrAddObject holds an upgradeable-read lock for its entire duration including the objectFactory invocation, which re-enters as a write lock via RegisterHandle (called from the constructor via Handle setter). This nested lock pattern is the main reentrancy constraint any striped design must preserve. |
| `binding/SkiaSharp/HandleDictionary.cs` | 135-171 | direct | RegisterHandle takes a write lock, replaces the WeakReference entry, then disposes any replaced object OUTSIDE the lock (objectToDispose?.DisposeInternal()). This outside-lock disposal pattern is an existing invariant that striped design must continue to honour. |
| `binding/SkiaSharp/SKObject.cs` | 279-284 | related | Dispose() checks IgnorePublicDispose before calling DisposeInternal(). The issue references this as a coupling point where striped design requires SKObject.Dispose() to route to the per-handle shard lock rather than a global lock. |

### Next Questions

- Does an in-tree BenchmarkDotNet harness against real SKObjects confirm measurable contention under realistic usage patterns?
- Can the cross-shard deadlock hazard be cleanly eliminated (factory-outside-lock vs lock-ordering approach)?
- Do shared-singleton hot paths (sRGB colorspace, default typeface) regress measurably with sharding?

### Resolution Proposals

**Hypothesis:** Sharding HandleDictionary's lock into N=ProcessorCount shards would reduce lock contention for parallel workloads creating/disposing distinct objects, but requires resolving a cross-shard deadlock hazard and validating with real-object benchmarks first.

1. **Add BenchmarkDotNet harness for HandleDictionary** — investigation, confidence 0.90 (90%), cost/s, validated=untested
   - Add a committed BenchmarkDotNet harness under `benchmarks/` exercising N-thread create/register/dispose against the real HandleDictionary as a baseline before any striping implementation.
2. **Prototype striped HandleDictionary behind feature flag** — fix, confidence 0.70 (70%), cost/l, validated=untested
   - Implement N-shard dictionary using PlatformLock.Factory() N times, with shard count resolved from AppContext/env var, bit-mixed pointer hash for shard index, and N=1 fallback to current behavior.

**Recommended proposal:** Add BenchmarkDotNet harness for HandleDictionary

**Why:** The issue itself lists in-tree benchmarking as the first prerequisite. Establishing a real-object baseline is lower risk and gates the striping implementation decision.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.88 (88%) |
| Reason | Well-specified performance enhancement proposal with benchmark data, design sketch, kill criteria, and explicit exploratory status. Needs in-tree benchmark and deadlock resolution before implementation. No urgency — non-blocking for #4080. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply enhancement, SkiaSharp area, and performance tenet labels | labels=type/enhancement, area/SkiaSharp, tenet/performance |
| add-comment | medium | 0.88 (88%) | Acknowledge the detailed proposal and confirm next steps | — |
| link-related | low | 0.95 (95%) | Cross-reference the correctness hardening issue | linkedIssue=#4080 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed design analysis and benchmark data! This is a well-structured exploratory proposal.

The benchmark results are promising — 2.5–3.7× contention reduction at 8–16 threads with zero single-thread regression is a strong signal. The cross-shard deadlock hazard you identified (factory-under-lock acquiring a second shard) is the key design constraint to resolve before implementation.

Suggested sequencing per your proposal:
1. Add a BenchmarkDotNet harness under `benchmarks/` to establish a real-SKObject baseline (confirms the bottleneck is real in practice)
2. Prototype with N=1 fallback to verify test suite passes unchanged
3. Resolve the deadlock hazard (factory-outside-lock or strict lock ordering)

Keeping open for tracking. This is non-blocking for #4080.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4101,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-31T05:45:00Z",
    "currentLabels": [
      "enhancement"
    ]
  },
  "summary": "Proposal to investigate lock striping/sharding for HandleDictionary to reduce contention in highly parallel workloads that create/dispose many distinct SKObject wrappers concurrently.",
  "classification": {
    "type": {
      "value": "type/enhancement",
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
      "environmentDetails": "Observed at 8–16+ threads; standalone microbenchmark provided inline",
      "relatedIssues": [
        4080,
        3817
      ],
      "repoLinks": []
    }
  },
  "analysis": {
    "summary": "The issue proposes partitioning the single global HandleDictionary lock into N independently-locked shards keyed by a hash of the native handle. Microbenchmark data shows 2.5–3.7x contention reduction at 8–16 threads for distinct-handle workloads, with zero single-thread overhead, but potential harm for shared-singleton workloads and a new cross-shard deadlock hazard from the factory-under-lock pattern. The author explicitly marks it exploratory and gated on an in-tree benchmark with real SKObjects.",
    "rationale": "This is a performance enhancement to existing lock infrastructure (HandleDictionary), not a bug fix or new feature. The existing design (one lock + upgradeable-read pattern) is confirmed by code inspection. The issue is well-specified with benchmark data, design sketch, and explicit kill criteria. It cross-links #4080 (which handles correctness) and is filed as a future, non-blocking investigation.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/HandleDictionary.cs",
        "lines": "21-28",
        "finding": "Single process-wide lock: `internal static readonly IPlatformLock instancesLock = PlatformLock.Create()` guards one `Dictionary<IntPtr, WeakReference> instances`. All GetInstance, GetOrAddObject, RegisterHandle, and DeregisterHandle calls serialise through this lock.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/HandleDictionary.cs",
        "lines": "74-97",
        "finding": "GetOrAddObject holds an upgradeable-read lock for its entire duration including the objectFactory invocation, which re-enters as a write lock via RegisterHandle (called from the constructor via Handle setter). This nested lock pattern is the main reentrancy constraint any striped design must preserve.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/HandleDictionary.cs",
        "lines": "135-171",
        "finding": "RegisterHandle takes a write lock, replaces the WeakReference entry, then disposes any replaced object OUTSIDE the lock (objectToDispose?.DisposeInternal()). This outside-lock disposal pattern is an existing invariant that striped design must continue to honour.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKObject.cs",
        "lines": "279-284",
        "finding": "Dispose() checks IgnorePublicDispose before calling DisposeInternal(). The issue references this as a coupling point where striped design requires SKObject.Dispose() to route to the per-handle shard lock rather than a global lock.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Status: exploratory. A standalone microbenchmark shows real but conditional value.",
        "source": "issue body",
        "interpretation": "Author self-classifies as exploratory investigation, not a concrete implementation request."
      },
      {
        "text": "2.5–3.7× contention reduction for high-thread parallel churn of distinct objects, with zero single-thread cost — but no benefit (slight harm) for shared-singleton hot paths",
        "source": "issue body",
        "interpretation": "Real performance benefit documented with benchmark data, but scope is conditional."
      },
      {
        "text": "Non-blocking for #4080.",
        "source": "issue body",
        "interpretation": "Decoupled from the correctness work in #4080; can be addressed independently."
      }
    ],
    "nextQuestions": [
      "Does an in-tree BenchmarkDotNet harness against real SKObjects confirm measurable contention under realistic usage patterns?",
      "Can the cross-shard deadlock hazard be cleanly eliminated (factory-outside-lock vs lock-ordering approach)?",
      "Do shared-singleton hot paths (sRGB colorspace, default typeface) regress measurably with sharding?"
    ],
    "resolution": {
      "hypothesis": "Sharding HandleDictionary's lock into N=ProcessorCount shards would reduce lock contention for parallel workloads creating/disposing distinct objects, but requires resolving a cross-shard deadlock hazard and validating with real-object benchmarks first.",
      "proposals": [
        {
          "title": "Add BenchmarkDotNet harness for HandleDictionary",
          "description": "Add a committed BenchmarkDotNet harness under `benchmarks/` exercising N-thread create/register/dispose against the real HandleDictionary as a baseline before any striping implementation.",
          "category": "investigation",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Prototype striped HandleDictionary behind feature flag",
          "description": "Implement N-shard dictionary using PlatformLock.Factory() N times, with shard count resolved from AppContext/env var, bit-mixed pointer hash for shard index, and N=1 fallback to current behavior.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add BenchmarkDotNet harness for HandleDictionary",
      "recommendedReason": "The issue itself lists in-tree benchmarking as the first prerequisite. Establishing a real-object baseline is lower risk and gates the striping implementation decision."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.88,
      "reason": "Well-specified performance enhancement proposal with benchmark data, design sketch, kill criteria, and explicit exploratory status. Needs in-tree benchmark and deadlock resolution before implementation. No urgency — non-blocking for #4080.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement, SkiaSharp area, and performance tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp",
          "tenet/performance"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the detailed proposal and confirm next steps",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed design analysis and benchmark data! This is a well-structured exploratory proposal.\n\nThe benchmark results are promising — 2.5–3.7× contention reduction at 8–16 threads with zero single-thread regression is a strong signal. The cross-shard deadlock hazard you identified (factory-under-lock acquiring a second shard) is the key design constraint to resolve before implementation.\n\nSuggested sequencing per your proposal:\n1. Add a BenchmarkDotNet harness under `benchmarks/` to establish a real-SKObject baseline (confirms the bottleneck is real in practice)\n2. Prototype with N=1 fallback to verify test suite passes unchanged\n3. Resolve the deadlock hazard (factory-outside-lock or strict lock ordering)\n\nKeeping open for tracking. This is non-blocking for #4080."
      },
      {
        "type": "link-related",
        "description": "Cross-reference the correctness hardening issue",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 4080
      }
    ]
  }
}
```

</details>
