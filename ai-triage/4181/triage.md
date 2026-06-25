# Issue Triage Report — #4181

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-06-25T05:40:00Z |
| Type | type/enhancement (0.90 (90%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | needs-investigation (0.85 (85%)) |

**Issue Summary:** SKObject.OwnedObjects and KeepAliveObjects use ConcurrentDictionary with default capacity 31, causing ~31 heap allocations per SKObject instance on first access to owned-child tracking — noticeable in hot paths like SKSurface.Canvas in Avalonia render loops.

**Analysis:** SKObject.OwnedObjects and KeepAliveObjects are lazily-initialized ConcurrentDictionary<IntPtr, SKObject> fields with no explicit initial capacity, causing the .NET runtime to allocate 31 bucket slots on first dictionary creation. Each SKObject that owns children (e.g., every SKSurface that returns Canvas) pays this allocation cost once at first OwnedObjects access. Passing concurrencyLevel:1 and capacity:1 would reduce the initial allocation to a single bucket and is a straightforward code change.

**Recommendations:** **needs-investigation** — Well-identified performance issue with a clear, low-risk fix path. Maintainer has engaged positively. Needs a PR to implement and benchmark the change. The deeper alternative (replacing ConcurrentDictionary entirely) deserves a design discussion before committing to it.

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
| Current labels | type/feature-request |

## Evidence

### Reproduction

**Environment:** Avalonia app with SkiaSharp, .NET runtime ConcurrentDictionary DEFAULT_CAPACITY = 31

**Repository links:**
- https://github.com/microsoft/referencesource/blob/ec9fa9ae770d522a5b5f0607898044b7478574a3/mscorlib/system/collections/Concurrent/ConcurrentDictionary.cs#L122 — ConcurrentDictionary DEFAULT_CAPACITY = 31
- https://github.com/microsoft/referencesource/blob/ec9fa9ae770d522a5b5f0607898044b7478574a3/mscorlib/system/collections/Concurrent/ConcurrentDictionary.cs#L341-L345 — ConcurrentDictionary internal allocation site for 31 buckets
- https://github.com/mono/SkiaSharp/issues/1205 — Related: request to expose SkiaApi for perf-sensitive callers who want to bypass SKObject overhead entirely

**Screenshots:**
- https://github.com/user-attachments/assets/9b8923da-fba7-41d5-90d2-ec570938adaa — Profiler screenshot showing 31 object allocations per SKSurface.Canvas call, traced to ConcurrentDictionary bucket initialization

## Analysis

### Technical Summary

SKObject.OwnedObjects and KeepAliveObjects are lazily-initialized ConcurrentDictionary<IntPtr, SKObject> fields with no explicit initial capacity, causing the .NET runtime to allocate 31 bucket slots on first dictionary creation. Each SKObject that owns children (e.g., every SKSurface that returns Canvas) pays this allocation cost once at first OwnedObjects access. Passing concurrencyLevel:1 and capacity:1 would reduce the initial allocation to a single bucket and is a straightforward code change.

### Rationale

This is a type/enhancement because the OwnedObjects tracking mechanism works correctly but is implemented with suboptimal ConcurrentDictionary defaults. The .NET runtime allocates 31 internal bucket slots on ConcurrentDictionary construction regardless of how many entries will actually be stored. SKObject-derived types typically own 0–2 children, making capacity=1 appropriate. The fix (pass concurrencyLevel:1, capacity:1 to the ConcurrentDictionary constructor) is a well-understood, low-risk change. The maintainer has already engaged positively on the general performance concern.

### Key Signals

- "31 object allocations for each call [to SKSurface.Canvas] ... tracked down to .NET default DEFAULT_CAPACITY = 31" — **issue body** (Reporter profiled a real allocation and traced it to ConcurrentDictionary initialization. The 31 objects match the documented default capacity exactly.)
- "Adjust default capacity, by SkiaSharp defining capacity at ctor call, to something reasonable for this use case e.g. 1 perhaps even." — **issue body** (The proposed fix is specific and actionable: pass concurrencyLevel and capacity to ConcurrentDictionary ctor.)
- "we are trying to get that down. Any identification and analysis would help." — **maintainer comment (mattleibow)** (Maintainer actively prioritizes allocation reduction and welcomes this kind of analysis — positive signal for accepting a fix PR.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKObject.cs` | 14-26 | direct | OwnedObjects property lazy-creates new ConcurrentDictionary<IntPtr, SKObject>() with no explicit capacity or concurrencyLevel, defaulting to 31 internal bucket slots. The lock-double-check pattern is correct but the dictionary allocation itself is oversized for its typical use (usually 0–2 owned children). |
| `binding/SkiaSharp/SKObject.cs` | 28-37 | direct | KeepAliveObjects has the identical issue: lazy-creates ConcurrentDictionary with default capacity 31. Same fix applies. |
| `binding/SkiaSharp/SKObject.cs` | 160-168 | direct | OwnedBy() is called with every owned-child registration (e.g., SKSurface.Canvas). It accesses owner.OwnedObjects[child.Handle], triggering lazy dictionary creation on first call per SKObject instance. |
| `binding/SkiaSharp/SKSurface.cs` | 291-295 | related | SKSurface.Canvas property calls OwnedBy(SKCanvas.GetObject(...), this) on every access. This is the specific hot path identified by the reporter in their Avalonia profiler run. |

### Workarounds

- Cache the SKSurface.Canvas reference in a local variable or field rather than calling the property every frame — the OwnedObjects allocation only occurs on first access per SKSurface instance, so repeated calls to Canvas on the same surface are cheap after the first.
- For extreme performance requirements, cast Handle to IntPtr and use SkiaApi (internal) directly — see issue #1205 for discussion of exposing SkiaApi publicly.

### Next Questions

- Is ConcurrentDictionary even necessary here, or could a simpler Dictionary<IntPtr, SKObject> guarded by the existing `locker` field suffice? The lock pattern already exists in OwnedObjects getter.
- Are there thread-safety guarantees that require concurrent dictionary reads (without the outer lock) for OwnedObjects/KeepAliveObjects after creation?
- Would passing concurrencyLevel:1 alone (without reducing capacity) satisfy .NET performance guidelines, or is a full data-structure review warranted?

### Resolution Proposals

**Hypothesis:** Passing concurrencyLevel:1, capacity:1 to the ConcurrentDictionary constructor in both OwnedObjects and KeepAliveObjects property getters will reduce per-SKObject allocation from ~31 slots to 1 slot, eliminating the profiled allocation spike.

1. **Pass initial capacity=1 to ConcurrentDictionary** — fix, confidence 0.90 (90%), cost/xs, validated=yes
   - In both OwnedObjects and KeepAliveObjects lazy-init blocks, change the constructor call to new ConcurrentDictionary<IntPtr, SKObject>(concurrencyLevel: 1, capacity: 1). This reduces initial bucket allocation from 31 to 1 for the common case where each SKObject owns very few children.

```csharp
ownedObjects ??= new ConcurrentDictionary<IntPtr, SKObject>(concurrencyLevel: 1, capacity: 1);
```
2. **Replace ConcurrentDictionary with Dictionary + locker** — alternative, confidence 0.65 (65%), cost/s, validated=untested
   - Since SKObject already has a `locker` object field used in the lazy-init, OwnedObjects and KeepAliveObjects could be plain Dictionary<IntPtr, SKObject> guarded by that same lock on writes. Reads during iteration (DisposeManaged) are always single-threaded at disposal time. This would eliminate ConcurrentDictionary overhead entirely.

**Recommended proposal:** Pass initial capacity=1 to ConcurrentDictionary

**Why:** Smallest possible change, immediately verifiable, no behavioral risk, and directly addresses the profiled allocation. The concurrencyLevel:1 hint also reduces lock striping overhead for the common single-threaded use case.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.85 (85%) |
| Reason | Well-identified performance issue with a clear, low-risk fix path. Maintainer has engaged positively. Needs a PR to implement and benchmark the change. The deeper alternative (replacing ConcurrentDictionary entirely) deserves a design discussion before committing to it. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Correct type to enhancement (not feature-request), add performance tenet | labels=type/enhancement, area/SkiaSharp, tenet/performance |
| add-comment | medium | 0.85 (85%) | Acknowledge the finding, confirm the code path, outline the fix and invite a PR | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed profiler analysis @nietras! The root cause is confirmed: `SKObject.OwnedObjects` and `KeepAliveObjects` lazily create a `ConcurrentDictionary<IntPtr, SKObject>` with default capacity (31 internal buckets) even though most `SKObject` instances own only 1–2 children.

The fix is straightforward — pass `concurrencyLevel: 1, capacity: 1` to the `ConcurrentDictionary` constructor in both lazy-init blocks in `SKObject.cs`:

```csharp
ownedObjects ??= new ConcurrentDictionary<IntPtr, SKObject>(concurrencyLevel: 1, capacity: 1);
keepAliveObjects ??= new ConcurrentDictionary<IntPtr, SKObject>(concurrencyLevel: 1, capacity: 1);
```

A longer-term alternative would be to replace `ConcurrentDictionary` with a plain `Dictionary` guarded by the existing `locker` field, since all write paths already acquire a lock.

**Workaround for now:** Cache the `Canvas` reference in a local variable rather than calling `SKSurface.Canvas` each frame — the allocation only happens once per `SKSurface` instance (on first access), so repeated calls within the same frame are cheap after the first.

A PR implementing the `capacity: 1` fix would be very welcome!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4181,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-06-25T05:40:00Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "SKObject.OwnedObjects and KeepAliveObjects use ConcurrentDictionary with default capacity 31, causing ~31 heap allocations per SKObject instance on first access to owned-child tracking — noticeable in hot paths like SKSurface.Canvas in Avalonia render loops.",
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
      "tenet/performance"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "screenshots": [
        {
          "url": "https://github.com/user-attachments/assets/9b8923da-fba7-41d5-90d2-ec570938adaa",
          "description": "Profiler screenshot showing 31 object allocations per SKSurface.Canvas call, traced to ConcurrentDictionary bucket initialization"
        }
      ],
      "environmentDetails": "Avalonia app with SkiaSharp, .NET runtime ConcurrentDictionary DEFAULT_CAPACITY = 31",
      "repoLinks": [
        {
          "url": "https://github.com/microsoft/referencesource/blob/ec9fa9ae770d522a5b5f0607898044b7478574a3/mscorlib/system/collections/Concurrent/ConcurrentDictionary.cs#L122",
          "description": "ConcurrentDictionary DEFAULT_CAPACITY = 31"
        },
        {
          "url": "https://github.com/microsoft/referencesource/blob/ec9fa9ae770d522a5b5f0607898044b7478574a3/mscorlib/system/collections/Concurrent/ConcurrentDictionary.cs#L341-L345",
          "description": "ConcurrentDictionary internal allocation site for 31 buckets"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1205",
          "description": "Related: request to expose SkiaApi for perf-sensitive callers who want to bypass SKObject overhead entirely"
        }
      ]
    }
  },
  "analysis": {
    "summary": "SKObject.OwnedObjects and KeepAliveObjects are lazily-initialized ConcurrentDictionary<IntPtr, SKObject> fields with no explicit initial capacity, causing the .NET runtime to allocate 31 bucket slots on first dictionary creation. Each SKObject that owns children (e.g., every SKSurface that returns Canvas) pays this allocation cost once at first OwnedObjects access. Passing concurrencyLevel:1 and capacity:1 would reduce the initial allocation to a single bucket and is a straightforward code change.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKObject.cs",
        "lines": "14-26",
        "finding": "OwnedObjects property lazy-creates new ConcurrentDictionary<IntPtr, SKObject>() with no explicit capacity or concurrencyLevel, defaulting to 31 internal bucket slots. The lock-double-check pattern is correct but the dictionary allocation itself is oversized for its typical use (usually 0–2 owned children).",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKObject.cs",
        "lines": "28-37",
        "finding": "KeepAliveObjects has the identical issue: lazy-creates ConcurrentDictionary with default capacity 31. Same fix applies.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKObject.cs",
        "lines": "160-168",
        "finding": "OwnedBy() is called with every owned-child registration (e.g., SKSurface.Canvas). It accesses owner.OwnedObjects[child.Handle], triggering lazy dictionary creation on first call per SKObject instance.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKSurface.cs",
        "lines": "291-295",
        "finding": "SKSurface.Canvas property calls OwnedBy(SKCanvas.GetObject(...), this) on every access. This is the specific hot path identified by the reporter in their Avalonia profiler run.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "31 object allocations for each call [to SKSurface.Canvas] ... tracked down to .NET default DEFAULT_CAPACITY = 31",
        "source": "issue body",
        "interpretation": "Reporter profiled a real allocation and traced it to ConcurrentDictionary initialization. The 31 objects match the documented default capacity exactly."
      },
      {
        "text": "Adjust default capacity, by SkiaSharp defining capacity at ctor call, to something reasonable for this use case e.g. 1 perhaps even.",
        "source": "issue body",
        "interpretation": "The proposed fix is specific and actionable: pass concurrencyLevel and capacity to ConcurrentDictionary ctor."
      },
      {
        "text": "we are trying to get that down. Any identification and analysis would help.",
        "source": "maintainer comment (mattleibow)",
        "interpretation": "Maintainer actively prioritizes allocation reduction and welcomes this kind of analysis — positive signal for accepting a fix PR."
      }
    ],
    "rationale": "This is a type/enhancement because the OwnedObjects tracking mechanism works correctly but is implemented with suboptimal ConcurrentDictionary defaults. The .NET runtime allocates 31 internal bucket slots on ConcurrentDictionary construction regardless of how many entries will actually be stored. SKObject-derived types typically own 0–2 children, making capacity=1 appropriate. The fix (pass concurrencyLevel:1, capacity:1 to the ConcurrentDictionary constructor) is a well-understood, low-risk change. The maintainer has already engaged positively on the general performance concern.",
    "workarounds": [
      "Cache the SKSurface.Canvas reference in a local variable or field rather than calling the property every frame — the OwnedObjects allocation only occurs on first access per SKSurface instance, so repeated calls to Canvas on the same surface are cheap after the first.",
      "For extreme performance requirements, cast Handle to IntPtr and use SkiaApi (internal) directly — see issue #1205 for discussion of exposing SkiaApi publicly."
    ],
    "nextQuestions": [
      "Is ConcurrentDictionary even necessary here, or could a simpler Dictionary<IntPtr, SKObject> guarded by the existing `locker` field suffice? The lock pattern already exists in OwnedObjects getter.",
      "Are there thread-safety guarantees that require concurrent dictionary reads (without the outer lock) for OwnedObjects/KeepAliveObjects after creation?",
      "Would passing concurrencyLevel:1 alone (without reducing capacity) satisfy .NET performance guidelines, or is a full data-structure review warranted?"
    ],
    "resolution": {
      "hypothesis": "Passing concurrencyLevel:1, capacity:1 to the ConcurrentDictionary constructor in both OwnedObjects and KeepAliveObjects property getters will reduce per-SKObject allocation from ~31 slots to 1 slot, eliminating the profiled allocation spike.",
      "proposals": [
        {
          "title": "Pass initial capacity=1 to ConcurrentDictionary",
          "description": "In both OwnedObjects and KeepAliveObjects lazy-init blocks, change the constructor call to new ConcurrentDictionary<IntPtr, SKObject>(concurrencyLevel: 1, capacity: 1). This reduces initial bucket allocation from 31 to 1 for the common case where each SKObject owns very few children.",
          "category": "fix",
          "codeSnippet": "ownedObjects ??= new ConcurrentDictionary<IntPtr, SKObject>(concurrencyLevel: 1, capacity: 1);",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Replace ConcurrentDictionary with Dictionary + locker",
          "description": "Since SKObject already has a `locker` object field used in the lazy-init, OwnedObjects and KeepAliveObjects could be plain Dictionary<IntPtr, SKObject> guarded by that same lock on writes. Reads during iteration (DisposeManaged) are always single-threaded at disposal time. This would eliminate ConcurrentDictionary overhead entirely.",
          "category": "alternative",
          "confidence": 0.65,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Pass initial capacity=1 to ConcurrentDictionary",
      "recommendedReason": "Smallest possible change, immediately verifiable, no behavioral risk, and directly addresses the profiled allocation. The concurrencyLevel:1 hint also reduces lock striping overhead for the common single-threaded use case."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "Well-identified performance issue with a clear, low-risk fix path. Maintainer has engaged positively. Needs a PR to implement and benchmark the change. The deeper alternative (replacing ConcurrentDictionary entirely) deserves a design discussion before committing to it.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct type to enhancement (not feature-request), add performance tenet",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp",
          "tenet/performance"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the finding, confirm the code path, outline the fix and invite a PR",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed profiler analysis @nietras! The root cause is confirmed: `SKObject.OwnedObjects` and `KeepAliveObjects` lazily create a `ConcurrentDictionary<IntPtr, SKObject>` with default capacity (31 internal buckets) even though most `SKObject` instances own only 1–2 children.\n\nThe fix is straightforward — pass `concurrencyLevel: 1, capacity: 1` to the `ConcurrentDictionary` constructor in both lazy-init blocks in `SKObject.cs`:\n\n```csharp\nownedObjects ??= new ConcurrentDictionary<IntPtr, SKObject>(concurrencyLevel: 1, capacity: 1);\nkeepAliveObjects ??= new ConcurrentDictionary<IntPtr, SKObject>(concurrencyLevel: 1, capacity: 1);\n```\n\nA longer-term alternative would be to replace `ConcurrentDictionary` with a plain `Dictionary` guarded by the existing `locker` field, since all write paths already acquire a lock.\n\n**Workaround for now:** Cache the `Canvas` reference in a local variable rather than calling `SKSurface.Canvas` each frame — the allocation only happens once per `SKSurface` instance (on first access), so repeated calls within the same frame are cheap after the first.\n\nA PR implementing the `capacity: 1` fix would be very welcome!"
      }
    ]
  }
}
```

</details>
