# Issue Triage Report — #3237

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:16:18Z |
| Type | type/enhancement (0.90 (90%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-fixed (0.97 (97%)) |

**Issue Summary:** Reporter requested an SKPath.AddPoly overload with a count parameter to avoid array recreation; confirmed resolved in SkiaSharp 3.119.0 via a ReadOnlySpan<SKPoint> overload.

**Analysis:** Reporter requested an SKPath.AddPoly overload with a count parameter to avoid reallocating arrays on each frame. SkiaSharp 3.119.0 added a ReadOnlySpan<SKPoint> overload that achieves this goal — callers can pass points.AsSpan(0, count) to control point count without array recreation. The reporter confirmed the fix works and requested closure.

**Recommendations:** **close-as-fixed** — Reporter confirmed that SkiaSharp 3.119.0's ReadOnlySpan<SKPoint> AddPoly overload resolves the request. Code investigation confirms the overload exists in the current codebase.

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
| Current labels | type/bug |

## Evidence

### Reproduction

1. Use SkiaSharp 2.88.x
2. Try to call path.AddPoly(points, pointCount, false) — method does not exist
3. Only path.AddPoly(points, false) is available which always uses full array length

**Environment:** SkiaSharp 2.88.9, Visual Studio on Windows, targets Android

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.9, 3.119.0 |
| Worked in | 3.119.0 |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | Reporter confirmed the ReadOnlySpan<T> overload in 3.119.0 resolves their need. The fix is already shipped. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.98 (98%) |
| Reason | Reporter tested SkiaSharp 3.119.0 and confirmed the ReadOnlySpan<SKPoint> overload resolves the issue. Code investigation confirms AddPoly(ReadOnlySpan<SKPoint>, bool) exists in current codebase at binding/SkiaSharp/SKPath.cs line 393. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | 3.119.0 |

## Analysis

### Technical Summary

Reporter requested an SKPath.AddPoly overload with a count parameter to avoid reallocating arrays on each frame. SkiaSharp 3.119.0 added a ReadOnlySpan<SKPoint> overload that achieves this goal — callers can pass points.AsSpan(0, count) to control point count without array recreation. The reporter confirmed the fix works and requested closure.

### Rationale

This is an enhancement request, not a bug — AddPoly worked as documented but lacked an overload for partial-array usage. The ReadOnlySpan<SKPoint> overload added in 3.119.0 resolves the request: callers can pass points.AsSpan(0, count) to control the point count without array recreation. The reporter confirmed the fix works and explicitly asked for closure.

### Key Signals

- "Instead of always setting PointCount = points.Length, we'd like to set this count ourselves." — **issue body** (Performance optimization request: avoid reallocating arrays when point count changes frequently)
- "3.119.0 has overload with ReadOnlySpan<T> support - which effectively provides the fix, and I tested it to be sure -- it works!" — **comment by najak3d** (Reporter confirmed the enhancement is satisfied by the span overload)
- "This issue can be closed as 'fixed'." — **comment by najak3d** (Explicit close request from the original reporter)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPath.cs` | 393-400 | direct | AddPoly(ReadOnlySpan<SKPoint> points, bool close = true) exists — allows callers to pass a span slice like points.AsSpan(0, count) to control how many points are added without creating a new array |
| `binding/SkiaSharp/SKPath.cs` | 402-409 | direct | Original AddPoly(SKPoint[] points, bool close = true) still exists and always uses points.Length — this was the limitation the reporter encountered in 2.88.x |

### Workarounds

- In SkiaSharp 3.119.0+, use path.AddPoly(points.AsSpan(0, count), false) to limit the number of points without array recreation.
- In older versions, maintain a fixed-size array and use Array.Copy to a correctly-sized array before calling AddPoly.

### Resolution Proposals

**Hypothesis:** The ReadOnlySpan<T> overload in 3.119.0 resolves the original request by allowing callers to pass a span of any desired length.

1. **Close as fixed** — fix, confidence 0.98 (98%), cost/xs, validated=yes
   - The AddPoly(ReadOnlySpan<SKPoint>, bool) overload in SkiaSharp 3.119.0 addresses the reporter's need. Use path.AddPoly(points.AsSpan(0, count), false) to control how many points are used.

**Recommended proposal:** Close as fixed

**Why:** Reporter confirmed the ReadOnlySpan overload works. Issue should be closed as completed.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.97 (97%) |
| Reason | Reporter confirmed that SkiaSharp 3.119.0's ReadOnlySpan<SKPoint> AddPoly overload resolves the request. Code investigation confirms the overload exists in the current codebase. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Correct type from bug to enhancement; add area/SkiaSharp and performance tenet | labels=type/enhancement, area/SkiaSharp, tenet/performance |
| add-comment | high | 0.97 (97%) | Confirm fix in 3.119.0 and close the issue | — |
| close-issue | medium | 0.97 (97%) | Close as completed — fix confirmed by reporter | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the follow-up! Glad the `ReadOnlySpan<SKPoint>` overload in 3.119.0 covers your use case. You can pass a span slice to control how many points are used:

```csharp
path.AddPoly(points.AsSpan(0, pointCount), false);
```

Closing as fixed. 🎉
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3237,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:16:18Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter requested an SKPath.AddPoly overload with a count parameter to avoid array recreation; confirmed resolved in SkiaSharp 3.119.0 via a ReadOnlySpan<SKPoint> overload.",
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
      "stepsToReproduce": [
        "Use SkiaSharp 2.88.x",
        "Try to call path.AddPoly(points, pointCount, false) — method does not exist",
        "Only path.AddPoly(points, false) is available which always uses full array length"
      ],
      "environmentDetails": "SkiaSharp 2.88.9, Visual Studio on Windows, targets Android"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.9",
        "3.119.0"
      ],
      "workedIn": "3.119.0",
      "currentRelevance": "unlikely",
      "relevanceReason": "Reporter confirmed the ReadOnlySpan<T> overload in 3.119.0 resolves their need. The fix is already shipped."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.98,
      "reason": "Reporter tested SkiaSharp 3.119.0 and confirmed the ReadOnlySpan<SKPoint> overload resolves the issue. Code investigation confirms AddPoly(ReadOnlySpan<SKPoint>, bool) exists in current codebase at binding/SkiaSharp/SKPath.cs line 393.",
      "fixedInVersion": "3.119.0"
    }
  },
  "analysis": {
    "summary": "Reporter requested an SKPath.AddPoly overload with a count parameter to avoid reallocating arrays on each frame. SkiaSharp 3.119.0 added a ReadOnlySpan<SKPoint> overload that achieves this goal — callers can pass points.AsSpan(0, count) to control point count without array recreation. The reporter confirmed the fix works and requested closure.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPath.cs",
        "lines": "393-400",
        "finding": "AddPoly(ReadOnlySpan<SKPoint> points, bool close = true) exists — allows callers to pass a span slice like points.AsSpan(0, count) to control how many points are added without creating a new array",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPath.cs",
        "lines": "402-409",
        "finding": "Original AddPoly(SKPoint[] points, bool close = true) still exists and always uses points.Length — this was the limitation the reporter encountered in 2.88.x",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Instead of always setting PointCount = points.Length, we'd like to set this count ourselves.",
        "source": "issue body",
        "interpretation": "Performance optimization request: avoid reallocating arrays when point count changes frequently"
      },
      {
        "text": "3.119.0 has overload with ReadOnlySpan<T> support - which effectively provides the fix, and I tested it to be sure -- it works!",
        "source": "comment by najak3d",
        "interpretation": "Reporter confirmed the enhancement is satisfied by the span overload"
      },
      {
        "text": "This issue can be closed as 'fixed'.",
        "source": "comment by najak3d",
        "interpretation": "Explicit close request from the original reporter"
      }
    ],
    "rationale": "This is an enhancement request, not a bug — AddPoly worked as documented but lacked an overload for partial-array usage. The ReadOnlySpan<SKPoint> overload added in 3.119.0 resolves the request: callers can pass points.AsSpan(0, count) to control the point count without array recreation. The reporter confirmed the fix works and explicitly asked for closure.",
    "workarounds": [
      "In SkiaSharp 3.119.0+, use path.AddPoly(points.AsSpan(0, count), false) to limit the number of points without array recreation.",
      "In older versions, maintain a fixed-size array and use Array.Copy to a correctly-sized array before calling AddPoly."
    ],
    "resolution": {
      "hypothesis": "The ReadOnlySpan<T> overload in 3.119.0 resolves the original request by allowing callers to pass a span of any desired length.",
      "proposals": [
        {
          "title": "Close as fixed",
          "description": "The AddPoly(ReadOnlySpan<SKPoint>, bool) overload in SkiaSharp 3.119.0 addresses the reporter's need. Use path.AddPoly(points.AsSpan(0, count), false) to control how many points are used.",
          "category": "fix",
          "validated": "yes",
          "confidence": 0.98,
          "effort": "cost/xs"
        }
      ],
      "recommendedProposal": "Close as fixed",
      "recommendedReason": "Reporter confirmed the ReadOnlySpan overload works. Issue should be closed as completed."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.97,
      "reason": "Reporter confirmed that SkiaSharp 3.119.0's ReadOnlySpan<SKPoint> AddPoly overload resolves the request. Code investigation confirms the overload exists in the current codebase.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct type from bug to enhancement; add area/SkiaSharp and performance tenet",
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
        "description": "Confirm fix in 3.119.0 and close the issue",
        "risk": "high",
        "confidence": 0.97,
        "comment": "Thanks for the follow-up! Glad the `ReadOnlySpan<SKPoint>` overload in 3.119.0 covers your use case. You can pass a span slice to control how many points are used:\n\n```csharp\npath.AddPoly(points.AsSpan(0, pointCount), false);\n```\n\nClosing as fixed. 🎉"
      },
      {
        "type": "close-issue",
        "description": "Close as completed — fix confirmed by reporter",
        "risk": "medium",
        "confidence": 0.97,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
