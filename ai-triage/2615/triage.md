# Issue Triage Report — #2615

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T09:19:01Z |
| Type | type/enhancement (0.95 (95%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | keep-open (0.92 (92%)) |

**Issue Summary:** Epic tracking issue for adopting newer .NET interop primitives in SkiaSharp — native function pointers, Span<T>/ReadOnlySpan<T>, removing MonoPInvokeCallback, and using hardware Intrinsics.

**Analysis:** This is a maintainer-authored epic issue tracking the modernization of SkiaSharp's interop layer. Two tasks are complete (native function pointers via delegate* in SkiaApi.generated.cs, and removing MonoPInvokeCallback to fallback #else branches), and two remain open: #2616 (Span<T>/ReadOnlySpan<T> migration across ~25 core binding files) and Use Intrinsics. The epic should stay open until all sub-tasks are resolved.

**Recommendations:** **keep-open** — Active epic with two sub-tasks still open (#2616 Span migration and Intrinsics). Should remain open to track overall progress.

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

**Related issues:** #2616

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2616 — Sub-task: Switch to Span/ReadOnlySpan

## Analysis

### Technical Summary

This is a maintainer-authored epic issue tracking the modernization of SkiaSharp's interop layer. Two tasks are complete (native function pointers via delegate* in SkiaApi.generated.cs, and removing MonoPInvokeCallback to fallback #else branches), and two remain open: #2616 (Span<T>/ReadOnlySpan<T> migration across ~25 core binding files) and Use Intrinsics. The epic should stay open until all sub-tasks are resolved.

### Rationale

This is a type/enhancement because it improves existing functionality (modernizing interop code with no behavioral regression). It spans the core SkiaSharp binding layer, not platform views or native lib. Performance is the primary quality tenet. No bugs are described — this is proactive modernization.

### Key Signals

- "[x] Switch to native function pointers" — **issue body** (Completed — SkiaApi.generated.cs uses delegate* unmanaged[Cdecl] blocks guarded by #if !USE_DELEGATES.)
- "[x] Remove [MonoPInvokeCallback] attributes" — **issue body** (Completed — MonoPInvokeCallback preserved only in #else (USE_DELEGATES fallback) branches in SkiaApi.generated.cs.)
- "- [ ] #2616" — **issue body** (Span<T>/ReadOnlySpan<T> migration is still open across ~25 binding files.)
- "- [ ] Use Intrinsics where we can/should" — **issue body** (Hardware intrinsics task is open with no sub-issue yet created.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaApi.generated.cs` | 121-200 | direct | Native function pointer pattern (delegate* unmanaged[Cdecl]) is used inside #if !USE_DELEGATES blocks; MonoPInvokeCallback callbacks preserved in #else fallback — confirms both checklist items are implemented. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 21532-21572 | related | MonoPInvokeCallback still appears in #else blocks (33 occurrences) as legacy Mono/iOS fallback — this is intentional backward compatibility, not a missed task. |

### Next Questions

- Is there a timeline or milestone associated with completing #2616?
- Are there specific SkiaSharp methods (e.g., pixel operations, matrix math) that would most benefit from Intrinsics?

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.92 (92%) |
| Reason | Active epic with two sub-tasks still open (#2616 Span migration and Intrinsics). Should remain open to track overall progress. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply enhancement, area, and tenet labels | labels=type/enhancement, area/SkiaSharp, tenet/performance |
| link-related | low | 0.95 (95%) | Link to sub-task #2616 (Span migration) | linkedIssue=#2616 |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2615,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T09:19:01Z"
  },
  "summary": "Epic tracking issue for adopting newer .NET interop primitives in SkiaSharp — native function pointers, Span<T>/ReadOnlySpan<T>, removing MonoPInvokeCallback, and using hardware Intrinsics.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "relatedIssues": [
        2616
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2616",
          "description": "Sub-task: Switch to Span/ReadOnlySpan"
        }
      ]
    }
  },
  "analysis": {
    "summary": "This is a maintainer-authored epic issue tracking the modernization of SkiaSharp's interop layer. Two tasks are complete (native function pointers via delegate* in SkiaApi.generated.cs, and removing MonoPInvokeCallback to fallback #else branches), and two remain open: #2616 (Span<T>/ReadOnlySpan<T> migration across ~25 core binding files) and Use Intrinsics. The epic should stay open until all sub-tasks are resolved.",
    "rationale": "This is a type/enhancement because it improves existing functionality (modernizing interop code with no behavioral regression). It spans the core SkiaSharp binding layer, not platform views or native lib. Performance is the primary quality tenet. No bugs are described — this is proactive modernization.",
    "keySignals": [
      {
        "text": "[x] Switch to native function pointers",
        "source": "issue body",
        "interpretation": "Completed — SkiaApi.generated.cs uses delegate* unmanaged[Cdecl] blocks guarded by #if !USE_DELEGATES."
      },
      {
        "text": "[x] Remove [MonoPInvokeCallback] attributes",
        "source": "issue body",
        "interpretation": "Completed — MonoPInvokeCallback preserved only in #else (USE_DELEGATES fallback) branches in SkiaApi.generated.cs."
      },
      {
        "text": "- [ ] #2616",
        "source": "issue body",
        "interpretation": "Span<T>/ReadOnlySpan<T> migration is still open across ~25 binding files."
      },
      {
        "text": "- [ ] Use Intrinsics where we can/should",
        "source": "issue body",
        "interpretation": "Hardware intrinsics task is open with no sub-issue yet created."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "121-200",
        "finding": "Native function pointer pattern (delegate* unmanaged[Cdecl]) is used inside #if !USE_DELEGATES blocks; MonoPInvokeCallback callbacks preserved in #else fallback — confirms both checklist items are implemented.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "21532-21572",
        "finding": "MonoPInvokeCallback still appears in #else blocks (33 occurrences) as legacy Mono/iOS fallback — this is intentional backward compatibility, not a missed task.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Is there a timeline or milestone associated with completing #2616?",
      "Are there specific SkiaSharp methods (e.g., pixel operations, matrix math) that would most benefit from Intrinsics?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.92,
      "reason": "Active epic with two sub-tasks still open (#2616 Span migration and Intrinsics). Should remain open to track overall progress.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement, area, and tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp",
          "tenet/performance"
        ]
      },
      {
        "type": "link-related",
        "description": "Link to sub-task #2616 (Span migration)",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 2616
      }
    ]
  }
}
```

</details>
