# Issue Triage Report — #1205

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T19:59:45Z |
| Type | type/feature-request (0.90 (90%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** Feature request to make the internal SkiaApi P/Invoke class public, enabling callers on hot code paths to bypass SkObject overhead and call Skia C API functions directly.

**Analysis:** The SkiaApi class is currently internal and auto-generated from the C API header (21,866 lines). Making it public would expose all P/Invoke entry points directly to consumers, letting performance-sensitive code bypass SkObject allocation and validation overhead. The main concern is ABI stability: since SkiaApi is generated on every Skia update, exposing it publicly would commit the project to never renaming or removing any generated symbol.

**Recommendations:** **keep-open** — Valid performance-motivated request with at least two users confirming the need. Requires design discussion about API stability strategy before any implementation. No action to close — keep open for maintainer decision.

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

**Code snippets:**

```csharp
SkiaApi.sk_path_rawiter_next(this.Handle, points1) — commenter highlights overhead in SKPathIterator.Next() that allocates and validates on every call
```

## Analysis

### Technical Summary

The SkiaApi class is currently internal and auto-generated from the C API header (21,866 lines). Making it public would expose all P/Invoke entry points directly to consumers, letting performance-sensitive code bypass SkObject allocation and validation overhead. The main concern is ABI stability: since SkiaApi is generated on every Skia update, exposing it publicly would commit the project to never renaming or removing any generated symbol.

### Rationale

This is clearly a feature request — no existing API is broken, the reporter wants a new public surface for a performance optimization. The request is technically sound (SkObject has real overhead) but raises API governance questions about committing to a stable public P/Invoke surface that currently changes freely as a generated internal implementation detail.

### Key Signals

- "SkObject initialization and housekeeping is quite costly in both CPU and allocations" — **issue body** (Performance-motivated request; reporter has profiled the overhead)
- "the only change that is needed is to make SkiaApi class to be public rather than internal" — **issue body** (Minimal change requested — just visibility modifier on the class)
- "especially for path iterators — this seems really expensive when it's applied to every path verb" — **comment #806323742** (Second user confirms real-world performance cost in tight loops (SKPathIterator))

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaApi.cs` | 7 | direct | SkiaApi is declared as 'internal partial class SkiaApi'. Making it public is a single keyword change but has broad ABI implications. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 1-21866 | direct | 21,866-line auto-generated file. Every Skia update regenerates this file and may rename, remove, or reorder functions. Exposing this publicly commits to API stability for all generated symbols. |

### Next Questions

- Would maintainers accept an 'experimental' or 'unsafe' opt-in surface to access SkiaApi without full ABI commitment?
- Are there specific high-frequency APIs (path iteration, paint reuse) that could be wrapped in zero-overhead public helpers instead?
- Would an 'advanced interop' NuGet package (like Microsoft.IO.RecyclableMemoryStream) be acceptable to separate stable vs. unstable APIs?

### Resolution Proposals

**Hypothesis:** Exposing SkiaApi directly is risky for long-term API stability, but the underlying performance need is real and affects multiple users.

1. **Make SkiaApi public** — fix, confidence 0.55 (55%), cost/s, validated=untested
   - Change 'internal partial class SkiaApi' to 'public partial class SkiaApi' in SkiaApi.cs. Simple but commits the project to keeping all ~22k generated symbols stable forever.
2. **Introduce an [Experimental] public SkiaApi with no stability guarantee** — alternative, confidence 0.70 (70%), cost/m, validated=untested
   - Mark the public SkiaApi with [Experimental] attribute and document that it can change on any release, giving advanced users access while managing expectations.
3. **Add zero-overhead public wrappers for known hot paths** — alternative, confidence 0.80 (80%), cost/l, validated=untested
   - Instead of exposing SkiaApi wholesale, add Span-based or unsafe public methods for the hottest paths (e.g., SKPath.RawIterator with a stack-allocated buffer) that avoid allocation without exposing internal P/Invoke.

**Recommended proposal:** Introduce an [Experimental] public SkiaApi with no stability guarantee

**Why:** Satisfies the performance need, gives users access to raw P/Invoke, while documenting that API stability is not guaranteed. Follows the pattern used by .NET runtime for experimental APIs.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Valid performance-motivated request with at least two users confirming the need. Requires design discussion about API stability strategy before any implementation. No action to close — keep open for maintainer decision. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply feature-request, SkiaSharp area, and performance tenet labels | labels=type/feature-request, area/SkiaSharp, tenet/performance |
| add-comment | medium | 0.75 (75%) | Acknowledge the request and explain the API stability tradeoff | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the request — this is a real performance concern and we have at least two users who've encountered the overhead.

The challenge with simply making `SkiaApi` public is that the entire class is **auto-generated** on every Skia update (currently ~22k lines). Exposing it publicly would commit us to API stability for thousands of generated P/Invoke symbols that today we're free to rename, reorder, or remove when upstream Skia changes.

Some options worth discussing:
1. **[Experimental] public SkiaApi** — make it public but decorated with `[Experimental]` or an `[EditorBrowsable(Never)]` + XML doc warning that it can change at any time.
2. **Zero-overhead public helpers** — add `Span`-based or `unsafe` public overloads for the specific hot paths (e.g., path iteration, paint reuse) rather than exposing the raw P/Invoke surface.

If you have a benchmark or profiling data showing the overhead, that would help prioritize which specific methods matter most.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1205,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T19:59:45Z"
  },
  "summary": "Feature request to make the internal SkiaApi P/Invoke class public, enabling callers on hot code paths to bypass SkObject overhead and call Skia C API functions directly.",
  "classification": {
    "type": {
      "value": "type/feature-request",
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
      "codeSnippets": [
        "SkiaApi.sk_path_rawiter_next(this.Handle, points1) — commenter highlights overhead in SKPathIterator.Next() that allocates and validates on every call"
      ]
    }
  },
  "analysis": {
    "summary": "The SkiaApi class is currently internal and auto-generated from the C API header (21,866 lines). Making it public would expose all P/Invoke entry points directly to consumers, letting performance-sensitive code bypass SkObject allocation and validation overhead. The main concern is ABI stability: since SkiaApi is generated on every Skia update, exposing it publicly would commit the project to never renaming or removing any generated symbol.",
    "rationale": "This is clearly a feature request — no existing API is broken, the reporter wants a new public surface for a performance optimization. The request is technically sound (SkObject has real overhead) but raises API governance questions about committing to a stable public P/Invoke surface that currently changes freely as a generated internal implementation detail.",
    "keySignals": [
      {
        "text": "SkObject initialization and housekeeping is quite costly in both CPU and allocations",
        "source": "issue body",
        "interpretation": "Performance-motivated request; reporter has profiled the overhead"
      },
      {
        "text": "the only change that is needed is to make SkiaApi class to be public rather than internal",
        "source": "issue body",
        "interpretation": "Minimal change requested — just visibility modifier on the class"
      },
      {
        "text": "especially for path iterators — this seems really expensive when it's applied to every path verb",
        "source": "comment #806323742",
        "interpretation": "Second user confirms real-world performance cost in tight loops (SKPathIterator)"
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaApi.cs",
        "lines": "7",
        "finding": "SkiaApi is declared as 'internal partial class SkiaApi'. Making it public is a single keyword change but has broad ABI implications.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "1-21866",
        "finding": "21,866-line auto-generated file. Every Skia update regenerates this file and may rename, remove, or reorder functions. Exposing this publicly commits to API stability for all generated symbols.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "Would maintainers accept an 'experimental' or 'unsafe' opt-in surface to access SkiaApi without full ABI commitment?",
      "Are there specific high-frequency APIs (path iteration, paint reuse) that could be wrapped in zero-overhead public helpers instead?",
      "Would an 'advanced interop' NuGet package (like Microsoft.IO.RecyclableMemoryStream) be acceptable to separate stable vs. unstable APIs?"
    ],
    "resolution": {
      "hypothesis": "Exposing SkiaApi directly is risky for long-term API stability, but the underlying performance need is real and affects multiple users.",
      "proposals": [
        {
          "title": "Make SkiaApi public",
          "description": "Change 'internal partial class SkiaApi' to 'public partial class SkiaApi' in SkiaApi.cs. Simple but commits the project to keeping all ~22k generated symbols stable forever.",
          "category": "fix",
          "confidence": 0.55,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Introduce an [Experimental] public SkiaApi with no stability guarantee",
          "description": "Mark the public SkiaApi with [Experimental] attribute and document that it can change on any release, giving advanced users access while managing expectations.",
          "category": "alternative",
          "confidence": 0.7,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Add zero-overhead public wrappers for known hot paths",
          "description": "Instead of exposing SkiaApi wholesale, add Span-based or unsafe public methods for the hottest paths (e.g., SKPath.RawIterator with a stack-allocated buffer) that avoid allocation without exposing internal P/Invoke.",
          "category": "alternative",
          "confidence": 0.8,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Introduce an [Experimental] public SkiaApi with no stability guarantee",
      "recommendedReason": "Satisfies the performance need, gives users access to raw P/Invoke, while documenting that API stability is not guaranteed. Follows the pattern used by .NET runtime for experimental APIs."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.8,
      "reason": "Valid performance-motivated request with at least two users confirming the need. Requires design discussion about API stability strategy before any implementation. No action to close — keep open for maintainer decision.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, SkiaSharp area, and performance tenet labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "tenet/performance"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the request and explain the API stability tradeoff",
        "risk": "medium",
        "confidence": 0.75,
        "comment": "Thanks for the request — this is a real performance concern and we have at least two users who've encountered the overhead.\n\nThe challenge with simply making `SkiaApi` public is that the entire class is **auto-generated** on every Skia update (currently ~22k lines). Exposing it publicly would commit us to API stability for thousands of generated P/Invoke symbols that today we're free to rename, reorder, or remove when upstream Skia changes.\n\nSome options worth discussing:\n1. **[Experimental] public SkiaApi** — make it public but decorated with `[Experimental]` or an `[EditorBrowsable(Never)]` + XML doc warning that it can change at any time.\n2. **Zero-overhead public helpers** — add `Span`-based or `unsafe` public overloads for the specific hot paths (e.g., path iteration, paint reuse) rather than exposing the raw P/Invoke surface.\n\nIf you have a benchmark or profiling data showing the overhead, that would help prioritize which specific methods matter most."
      }
    ]
  }
}
```

</details>
