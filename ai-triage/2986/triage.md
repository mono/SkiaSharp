# Issue Triage Report — #2986

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T03:22:00Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp (0.97 (97%)) |
| Suggested action | keep-open (0.88 (88%)) |

**Issue Summary:** Feature request to add overloads of SKCanvas.DrawPoints that accept a count/length parameter or ReadOnlySpan<SKPoint> to allow drawing a subset of a points array without allocation.

**Analysis:** SKCanvas.DrawPoints only has a single overload that accepts a full SKPoint[] array and always passes the entire array length to the native sk_canvas_draw_points call. The request is to expose a count/offset or ReadOnlySpan<SKPoint> overload so callers can draw a subset without heap allocation. Two stalled PRs (#2617, #2669) already target the broader Span migration for SkiaSharp APIs, so this request is within scope of ongoing work.

**Recommendations:** **keep-open** — The request is valid and well-specified. Two in-flight span-migration PRs (#2617, #2669) partially address it. Keeping open to track against those PRs. No blocker to implementation.

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
- https://github.com/mono/SkiaSharp/pull/2617 — Start using spans instead of arrays (stalled open PR by mattleibow)
- https://github.com/mono/SkiaSharp/pull/2669 — More Span and ReadOnlySpan (stalled open PR by ScrubN, includes SKCanvas span overloads)

**Code snippets:**

```csharp
SKCanvas.DrawPoints(mode, points, paint) — currently draws all points in the array; no count or span overload exists
```

## Analysis

### Technical Summary

SKCanvas.DrawPoints only has a single overload that accepts a full SKPoint[] array and always passes the entire array length to the native sk_canvas_draw_points call. The request is to expose a count/offset or ReadOnlySpan<SKPoint> overload so callers can draw a subset without heap allocation. Two stalled PRs (#2617, #2669) already target the broader Span migration for SkiaSharp APIs, so this request is within scope of ongoing work.

### Rationale

This is a feature request for a new API overload. The current implementation is confirmed (via code investigation) to lack any count/span variant. The request is well-specified, has a clear implementation path (add Span<SKPoint> overload mirroring existing code, optionally also add count+offset overload), and directly aligns with the stalled span-migration PRs. Classified type/feature-request at high confidence, area/SkiaSharp because the change is in SKCanvas binding. tenet/performance applies because the motivation is avoiding heap allocation.

### Key Signals

- "Add an override to SKCanvas.DrawPoints to allow for passing a count parameter to the SkiaApi.sk_canvas_draw_points method instead of just passing the entire array length as it currently does." — **issue body** (Clear, specific API-addition request with implementation detail)
- "it would be even nicer to utilize Spans for this so they can be sliced, but I see multiple PRs #2617 & #2669 are stalled" — **issue body** (Reporter is aware of in-flight span work and requests alignment)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 416-425 | direct | DrawPoints has a single overload: public void DrawPoints(SKPointMode mode, SKPoint[] points, SKPaint paint). It passes (IntPtr)points.Length directly to sk_canvas_draw_points — no count parameter or span path exists. |

### Workarounds

- Create a correctly-sized array slice before calling DrawPoints: canvas.DrawPoints(mode, points.Take(count).ToArray(), paint) — note this still allocates.
- Use ArraySegment or Memory<SKPoint>.Span.Slice and pass the sliced array to DrawPoints — still allocates a copy.
- Use unsafe/fixed code directly against sk_canvas_draw_points via P/Invoke interop to pass a count without allocation (advanced/fragile).

### Resolution Proposals

1. **Array-copy workaround** — workaround, cost/xs, validated=untested
   - Slice the array to create a new array before calling DrawPoints. Zero-allocation workaround: use a fixed+unsafe block with a pointer cast to call sk_canvas_draw_points directly via P/Invoke.
2. **Add ReadOnlySpan<SKPoint> overload** — fix, cost/s, validated=untested
   - Add a ReadOnlySpan<SKPoint> overload to SKCanvas.DrawPoints using 'fixed' on the span. This is ABI-additive (new overload only) and subsumes the count scenario since spans can be sliced. Aligns with PRs #2617/#2669.

**Recommended proposal:** Add ReadOnlySpan<SKPoint> overload

**Why:** Adding a ReadOnlySpan<SKPoint> overload is ABI-safe, zero-allocation, and is already partially addressed by in-flight PRs. It fully satisfies the request.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.88 (88%) |
| Reason | The request is valid and well-specified. Two in-flight span-migration PRs (#2617, #2669) partially address it. Keeping open to track against those PRs. No blocker to implementation. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply type/feature-request, area/SkiaSharp, tenet/performance | labels=type/feature-request, area/SkiaSharp, tenet/performance |
| link-related | low | 0.90 (90%) | Cross-reference open span PR #2617 which targets broader span migration | linkedIssue=#2617 |
| link-related | low | 0.90 (90%) | Cross-reference open span PR #2669 which includes SKCanvas span overloads | linkedIssue=#2669 |
| add-comment | medium | 0.85 (85%) | Acknowledge the request and note relationship to span PRs | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed request! You're correct that the current `SKCanvas.DrawPoints` only accepts a full array and always draws all points.

This request aligns well with the ongoing span-migration work tracked in #2617 and #2669. Once a `ReadOnlySpan<SKPoint>` overload is added, callers can simply slice the span to draw any sub-range without any heap allocation:

```csharp
// After span overload is added:
canvas.DrawPoints(SKPointMode.Points, points.AsSpan(0, count), paint);
```

In the meantime, you can work around the limitation by pre-slicing:

```csharp
var subset = new SKPoint[count];
Array.Copy(allPoints, subset, count);
canvas.DrawPoints(SKPointMode.Points, subset, paint);
```

This issue will be tracked against the span-migration PRs.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2986,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T03:22:00Z"
  },
  "summary": "Feature request to add overloads of SKCanvas.DrawPoints that accept a count/length parameter or ReadOnlySpan<SKPoint> to allow drawing a subset of a points array without allocation.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.97
    },
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "SKCanvas.DrawPoints(mode, points, paint) — currently draws all points in the array; no count or span overload exists"
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/2617",
          "description": "Start using spans instead of arrays (stalled open PR by mattleibow)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/2669",
          "description": "More Span and ReadOnlySpan (stalled open PR by ScrubN, includes SKCanvas span overloads)"
        }
      ]
    }
  },
  "analysis": {
    "summary": "SKCanvas.DrawPoints only has a single overload that accepts a full SKPoint[] array and always passes the entire array length to the native sk_canvas_draw_points call. The request is to expose a count/offset or ReadOnlySpan<SKPoint> overload so callers can draw a subset without heap allocation. Two stalled PRs (#2617, #2669) already target the broader Span migration for SkiaSharp APIs, so this request is within scope of ongoing work.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "416-425",
        "finding": "DrawPoints has a single overload: public void DrawPoints(SKPointMode mode, SKPoint[] points, SKPaint paint). It passes (IntPtr)points.Length directly to sk_canvas_draw_points — no count parameter or span path exists.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Add an override to SKCanvas.DrawPoints to allow for passing a count parameter to the SkiaApi.sk_canvas_draw_points method instead of just passing the entire array length as it currently does.",
        "source": "issue body",
        "interpretation": "Clear, specific API-addition request with implementation detail"
      },
      {
        "text": "it would be even nicer to utilize Spans for this so they can be sliced, but I see multiple PRs #2617 & #2669 are stalled",
        "source": "issue body",
        "interpretation": "Reporter is aware of in-flight span work and requests alignment"
      }
    ],
    "rationale": "This is a feature request for a new API overload. The current implementation is confirmed (via code investigation) to lack any count/span variant. The request is well-specified, has a clear implementation path (add Span<SKPoint> overload mirroring existing code, optionally also add count+offset overload), and directly aligns with the stalled span-migration PRs. Classified type/feature-request at high confidence, area/SkiaSharp because the change is in SKCanvas binding. tenet/performance applies because the motivation is avoiding heap allocation.",
    "resolution": {
      "proposals": [
        {
          "title": "Array-copy workaround",
          "category": "workaround",
          "description": "Slice the array to create a new array before calling DrawPoints. Zero-allocation workaround: use a fixed+unsafe block with a pointer cast to call sk_canvas_draw_points directly via P/Invoke.",
          "validated": "untested",
          "effort": "cost/xs"
        },
        {
          "title": "Add ReadOnlySpan<SKPoint> overload",
          "category": "fix",
          "description": "Add a ReadOnlySpan<SKPoint> overload to SKCanvas.DrawPoints using 'fixed' on the span. This is ABI-additive (new overload only) and subsumes the count scenario since spans can be sliced. Aligns with PRs #2617/#2669.",
          "validated": "untested",
          "effort": "cost/s"
        }
      ],
      "recommendedProposal": "Add ReadOnlySpan<SKPoint> overload",
      "recommendedReason": "Adding a ReadOnlySpan<SKPoint> overload is ABI-safe, zero-allocation, and is already partially addressed by in-flight PRs. It fully satisfies the request."
    },
    "workarounds": [
      "Create a correctly-sized array slice before calling DrawPoints: canvas.DrawPoints(mode, points.Take(count).ToArray(), paint) — note this still allocates.",
      "Use ArraySegment or Memory<SKPoint>.Span.Slice and pass the sliced array to DrawPoints — still allocates a copy.",
      "Use unsafe/fixed code directly against sk_canvas_draw_points via P/Invoke interop to pass a count without allocation (advanced/fragile)."
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.88,
      "reason": "The request is valid and well-specified. Two in-flight span-migration PRs (#2617, #2669) partially address it. Keeping open to track against those PRs. No blocker to implementation.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/feature-request, area/SkiaSharp, tenet/performance",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "tenet/performance"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference open span PR #2617 which targets broader span migration",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 2617
      },
      {
        "type": "link-related",
        "description": "Cross-reference open span PR #2669 which includes SKCanvas span overloads",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 2669
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the request and note relationship to span PRs",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed request! You're correct that the current `SKCanvas.DrawPoints` only accepts a full array and always draws all points.\n\nThis request aligns well with the ongoing span-migration work tracked in #2617 and #2669. Once a `ReadOnlySpan<SKPoint>` overload is added, callers can simply slice the span to draw any sub-range without any heap allocation:\n\n```csharp\n// After span overload is added:\ncanvas.DrawPoints(SKPointMode.Points, points.AsSpan(0, count), paint);\n```\n\nIn the meantime, you can work around the limitation by pre-slicing:\n\n```csharp\nvar subset = new SKPoint[count];\nArray.Copy(allPoints, subset, count);\ncanvas.DrawPoints(SKPointMode.Points, subset, paint);\n```\n\nThis issue will be tracked against the span-migration PRs."
      }
    ]
  }
}
```

</details>
