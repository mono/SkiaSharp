# Issue Triage Report — #689

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T15:45:00Z |
| Type | type/enhancement (0.92 (92%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | keep-open (0.85 (85%)) |

**Issue Summary:** Broad enhancement request to adopt modern .NET performance types (Span<T>, ReadOnlySpan<T>, Memory<T>) throughout SkiaSharp to reduce allocations; significant progress has been made since filing but some APIs still use arrays.

**Analysis:** This is a tracking issue for general performance improvements using modern .NET types. Since filing in 2018, SkiaSharp has adopted Span<T> and ReadOnlySpan<T> in many APIs (SKPath.AddPoly, SKBitmap.GetPixelSpan, SKPixmap.GetPixelSpan<T>, SKFont text methods, etc.). However, some APIs still expose only array-based overloads (e.g., SKCanvas.DrawPoints, DrawVertices, SKMatrix.MapPoints/MapVectors, SKPaint.GetTextPath). The issue should remain open as a tracking item for the remaining work.

**Recommendations:** **keep-open** — Significant Span<T> adoption has happened since 2018 (SKPath, SKBitmap, SKFont, SKPixmap all updated), but the issue is a useful tracker for remaining work in SKCanvas and SKMatrix. No missing info blocks progress.

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

**Code snippets:**

```csharp
extension method: AddPoly(this SKPath path, ReadOnlySpan<SKPoint> points, bool close) via fixed pointer + DllImport
```

```csharp
extension method: GetPixelSpan(this SKBitmap bitmap) returning Span<byte> via unsafe pointer
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | .NET 4.8, .NET Core 2.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The issue is still open; while many Span<T> overloads have been added, some array-only APIs remain (SKCanvas.DrawPoints, DrawVertices, SKMatrix.MapPoints/MapVectors). |

## Analysis

### Technical Summary

This is a tracking issue for general performance improvements using modern .NET types. Since filing in 2018, SkiaSharp has adopted Span<T> and ReadOnlySpan<T> in many APIs (SKPath.AddPoly, SKBitmap.GetPixelSpan, SKPixmap.GetPixelSpan<T>, SKFont text methods, etc.). However, some APIs still expose only array-based overloads (e.g., SKCanvas.DrawPoints, DrawVertices, SKMatrix.MapPoints/MapVectors, SKPaint.GetTextPath). The issue should remain open as a tracking item for the remaining work.

### Rationale

Classified as type/enhancement because the request improves existing APIs (adding Span overloads) rather than adding entirely new functionality. Area is area/SkiaSharp as all affected APIs are in the core binding. tenet/performance is the only applicable tenet. Action is keep-open because significant progress has been made (SKPath, SKBitmap, SKFont, SKPixmap all have Span support), but some APIs remain array-only and the broad tracking nature means more work can be done.

### Key Signals

- "new types introduced to reduce allocations and increase performance. We should really start to have a look at this and see where/how we can make use of new things." — **issue body** (Broad tracking issue for adopting Span<T>/Memory<T> across the API surface.)
- "SKPoint allocations are 20% of all allocations. This would reduce allocations by 50%." — **comment by softlion (2019-03-27)** (Real-world performance impact confirmed; user profiling shows significant allocation overhead.)
- "SKBitmap.Bytes copies the whole bitmap to a new byte[], so this is very slow and needs more memory." — **comment by MichaelRumpler (2019-05-27)** (Specific pain point around pixel access allocation — now addressed by GetPixelSpan.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPath.cs` | 393-402 | direct | SKPath.AddPoly has a ReadOnlySpan<SKPoint> overload alongside the original array overload — the specific workaround from comment #439622084 has been implemented. |
| `binding/SkiaSharp/SKBitmap.cs` | 300-304 | direct | SKBitmap.GetPixelSpan() and GetPixelSpan(x,y) return Span<byte> — the extension method from comment #496163934 has been implemented as a first-class API. |
| `binding/SkiaSharp/SKPixmap.cs` | 117-166 | direct | SKPixmap.GetPixelSpan<T>() and typed overloads are present, providing allocation-free pixel access. |
| `binding/SkiaSharp/SKFont.cs` | 125-334 | direct | SKFont has extensive ReadOnlySpan<char/byte/int/ushort> and Span<ushort> overloads for glyph and text measurement methods. |
| `binding/SkiaSharp/SKCanvas.cs` | 416,912-930,1017-1020 | direct | SKCanvas.DrawPoints, DrawVertices, and DrawPatch still accept only SKPoint[] arrays — these are opportunities for Span overloads not yet addressed. |
| `binding/SkiaSharp/SKMatrix.cs` | 334-402 | related | SKMatrix.MapPoints and MapVectors still use SKPoint[] arrays with no Span overloads. |

### Workarounds

- Users can write extension methods using unsafe fixed pointers as shown in the issue comments — both SKPath.AddPoly and SKBitmap pixel span have been upstreamed as first-class APIs.
- For remaining array-only APIs, allocating from ArrayPool<T>.Shared reduces GC pressure while awaiting first-class Span overloads.

### Next Questions

- Which remaining array-only APIs have the highest user-impact priority (DrawPoints/DrawVertices vs MapPoints/MapVectors)?
- Should a milestone be set to track completion of the remaining Span API additions?

### Resolution Proposals

**Hypothesis:** The original request has been substantially addressed. Remaining work is incremental: add ReadOnlySpan overloads to SKCanvas.DrawPoints, DrawVertices, DrawPatch and SKMatrix.MapPoints/MapVectors.

1. **Keep open as a tracking issue for remaining Span API work** — investigation, cost/m, validated=untested
   - The issue serves as an umbrella tracking item. The remaining array-only APIs (SKCanvas.DrawPoints, DrawVertices, SKMatrix.MapPoints/MapVectors) can be converted incrementally, each as a small PR.
2. **Use ArrayPool<T> as interim workaround for remaining array-only APIs** — workaround, cost/xs, validated=untested
   - For callers needing to avoid allocations with APIs that still accept only arrays, renting from ArrayPool<SKPoint>.Shared eliminates per-call heap allocations.

**Recommended proposal:** keep-open-tracking

**Why:** The issue is a long-running tracker with real ongoing value. Closing prematurely would lose visibility on the remaining gaps.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.85 (85%) |
| Reason | Significant Span<T> adoption has happened since 2018 (SKPath, SKBitmap, SKFont, SKPixmap all updated), but the issue is a useful tracker for remaining work in SKCanvas and SKMatrix. No missing info blocks progress. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/enhancement, area/SkiaSharp, tenet/performance, triage/triaged labels | labels=type/enhancement, area/SkiaSharp, tenet/performance, triage/triaged |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 689,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T15:45:00Z"
  },
  "summary": "Broad enhancement request to adopt modern .NET performance types (Span<T>, ReadOnlySpan<T>, Memory<T>) throughout SkiaSharp to reduce allocations; significant progress has been made since filing but some APIs still use arrays.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.92
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
        "extension method: AddPoly(this SKPath path, ReadOnlySpan<SKPoint> points, bool close) via fixed pointer + DllImport",
        "extension method: GetPixelSpan(this SKBitmap bitmap) returning Span<byte> via unsafe pointer"
      ],
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        ".NET 4.8",
        ".NET Core 2.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The issue is still open; while many Span<T> overloads have been added, some array-only APIs remain (SKCanvas.DrawPoints, DrawVertices, SKMatrix.MapPoints/MapVectors)."
    }
  },
  "analysis": {
    "summary": "This is a tracking issue for general performance improvements using modern .NET types. Since filing in 2018, SkiaSharp has adopted Span<T> and ReadOnlySpan<T> in many APIs (SKPath.AddPoly, SKBitmap.GetPixelSpan, SKPixmap.GetPixelSpan<T>, SKFont text methods, etc.). However, some APIs still expose only array-based overloads (e.g., SKCanvas.DrawPoints, DrawVertices, SKMatrix.MapPoints/MapVectors, SKPaint.GetTextPath). The issue should remain open as a tracking item for the remaining work.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPath.cs",
        "lines": "393-402",
        "finding": "SKPath.AddPoly has a ReadOnlySpan<SKPoint> overload alongside the original array overload — the specific workaround from comment #439622084 has been implemented.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "300-304",
        "finding": "SKBitmap.GetPixelSpan() and GetPixelSpan(x,y) return Span<byte> — the extension method from comment #496163934 has been implemented as a first-class API.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPixmap.cs",
        "lines": "117-166",
        "finding": "SKPixmap.GetPixelSpan<T>() and typed overloads are present, providing allocation-free pixel access.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFont.cs",
        "lines": "125-334",
        "finding": "SKFont has extensive ReadOnlySpan<char/byte/int/ushort> and Span<ushort> overloads for glyph and text measurement methods.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "416,912-930,1017-1020",
        "finding": "SKCanvas.DrawPoints, DrawVertices, and DrawPatch still accept only SKPoint[] arrays — these are opportunities for Span overloads not yet addressed.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKMatrix.cs",
        "lines": "334-402",
        "finding": "SKMatrix.MapPoints and MapVectors still use SKPoint[] arrays with no Span overloads.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "new types introduced to reduce allocations and increase performance. We should really start to have a look at this and see where/how we can make use of new things.",
        "source": "issue body",
        "interpretation": "Broad tracking issue for adopting Span<T>/Memory<T> across the API surface."
      },
      {
        "text": "SKPoint allocations are 20% of all allocations. This would reduce allocations by 50%.",
        "source": "comment by softlion (2019-03-27)",
        "interpretation": "Real-world performance impact confirmed; user profiling shows significant allocation overhead."
      },
      {
        "text": "SKBitmap.Bytes copies the whole bitmap to a new byte[], so this is very slow and needs more memory.",
        "source": "comment by MichaelRumpler (2019-05-27)",
        "interpretation": "Specific pain point around pixel access allocation — now addressed by GetPixelSpan."
      }
    ],
    "rationale": "Classified as type/enhancement because the request improves existing APIs (adding Span overloads) rather than adding entirely new functionality. Area is area/SkiaSharp as all affected APIs are in the core binding. tenet/performance is the only applicable tenet. Action is keep-open because significant progress has been made (SKPath, SKBitmap, SKFont, SKPixmap all have Span support), but some APIs remain array-only and the broad tracking nature means more work can be done.",
    "workarounds": [
      "Users can write extension methods using unsafe fixed pointers as shown in the issue comments — both SKPath.AddPoly and SKBitmap pixel span have been upstreamed as first-class APIs.",
      "For remaining array-only APIs, allocating from ArrayPool<T>.Shared reduces GC pressure while awaiting first-class Span overloads."
    ],
    "nextQuestions": [
      "Which remaining array-only APIs have the highest user-impact priority (DrawPoints/DrawVertices vs MapPoints/MapVectors)?",
      "Should a milestone be set to track completion of the remaining Span API additions?"
    ],
    "resolution": {
      "hypothesis": "The original request has been substantially addressed. Remaining work is incremental: add ReadOnlySpan overloads to SKCanvas.DrawPoints, DrawVertices, DrawPatch and SKMatrix.MapPoints/MapVectors.",
      "proposals": [
        {
          "title": "Keep open as a tracking issue for remaining Span API work",
          "category": "investigation",
          "effort": "cost/m",
          "validated": "untested",
          "description": "The issue serves as an umbrella tracking item. The remaining array-only APIs (SKCanvas.DrawPoints, DrawVertices, SKMatrix.MapPoints/MapVectors) can be converted incrementally, each as a small PR."
        },
        {
          "title": "Use ArrayPool<T> as interim workaround for remaining array-only APIs",
          "category": "workaround",
          "effort": "cost/xs",
          "validated": "untested",
          "description": "For callers needing to avoid allocations with APIs that still accept only arrays, renting from ArrayPool<SKPoint>.Shared eliminates per-call heap allocations."
        }
      ],
      "recommendedProposal": "keep-open-tracking",
      "recommendedReason": "The issue is a long-running tracker with real ongoing value. Closing prematurely would lose visibility on the remaining gaps."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.85,
      "reason": "Significant Span<T> adoption has happened since 2018 (SKPath, SKBitmap, SKFont, SKPixmap all updated), but the issue is a useful tracker for remaining work in SKCanvas and SKMatrix. No missing info blocks progress.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/enhancement, area/SkiaSharp, tenet/performance, triage/triaged labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp",
          "tenet/performance",
          "triage/triaged"
        ]
      }
    ]
  }
}
```

</details>
