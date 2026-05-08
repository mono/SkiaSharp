# Issue Triage Report — #1909

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T23:45:00Z |
| Type | type/bug (0.98 (98%)) |
| Area | area/SkiaSharp.HarfBuzz (0.97 (97%)) |
| Suggested action | close-as-fixed (0.93 (93%)) |

**Issue Summary:** DrawShapedText old overloads (accepting only SKPaint) ignored SKPaint.TextAlign, causing text drawn via SKShaper to always render left-aligned regardless of the configured TextAlign; the fix was shipped in SkiaSharp 3.116.0 with new explicit-SKTextAlign overloads and alignment logic in CanvasExtensions.

**Analysis:** The DrawShapedText extension method in SkiaSharp.HarfBuzz.CanvasExtensions did not honor SKPaint.TextAlign in its original implementation. SkiaSharp 3.116.0 introduced new overloads with an explicit SKTextAlign parameter and added xOffset alignment logic (lines 92-98 of CanvasExtensions.cs). The old overloads were deprecated and now delegate to paint.TextAlign. This issue is effectively fixed in 3.116.0+.

**Recommendations:** **close-as-fixed** — Code investigation confirms the alignment logic exists in the current codebase (CanvasExtensions.cs lines 91-102) and was added in 3.116.0. The old overloads are marked [Obsolete] and now pass paint.TextAlign. A community comment from 2023 also notes the issue should be closed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.HarfBuzz |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create SKPaint with TextAlign = SKTextAlign.Center
2. Call canvas.DrawShapedText(shaper, text, x, y, paint)
3. Observe text is rendered left-aligned, ignoring the Center alignment

**Environment:** SkiaSharp 2.x era; SkiaSharp.HarfBuzz

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1909 — Original bug report with screenshots

**Code snippets:**

```csharp
using var paint = new SKPaint { TextAlign = SKTextAlign.Center };
canvas.DrawShapedText(shaper, "test", width / 2f, (height / 2f) + (paint.FontMetrics.XHeight / 2), paint);
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | DrawShapedText ignores SKPaint.TextAlign — text always draws left-aligned |
| Repro quality | complete |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | SkiaSharp 3.116.0 added explicit SKTextAlign overloads for DrawShapedText and the old overloads now pass paint.TextAlign. The core method in CanvasExtensions.cs lines 92-98 contains the alignment adjustment logic. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.95 (95%) |
| Reason | The SkiaSharp 3.116.0 changelog shows new DrawShapedText overloads with explicit SKTextAlign were added, and the old SKPaint-only overloads were marked [Obsolete]. Current CanvasExtensions.cs lines 50-55 show the obsolete overload passes paint.TextAlign to the new implementation. Lines 92-98 implement the alignment offset calculation. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | 3.116.0 |

## Analysis

### Technical Summary

The DrawShapedText extension method in SkiaSharp.HarfBuzz.CanvasExtensions did not honor SKPaint.TextAlign in its original implementation. SkiaSharp 3.116.0 introduced new overloads with an explicit SKTextAlign parameter and added xOffset alignment logic (lines 92-98 of CanvasExtensions.cs). The old overloads were deprecated and now delegate to paint.TextAlign. This issue is effectively fixed in 3.116.0+.

### Rationale

This is a confirmed bug (wrong-output) in SkiaSharp.HarfBuzz.CanvasExtensions where DrawShapedText ignored paint.TextAlign. Code investigation confirms the fix is present in the current codebase: the old overloads are now marked [Obsolete] and pass paint.TextAlign, while new overloads with explicit SKTextAlign were added in 3.116.0 along with alignment offset logic. Suggesting close-as-fixed with version 3.116.0.

### Key Signals

- "When drawing text with DrawShapedText, the specified TextAlign in SKPaint is ignored." — **issue body** (Root cause: the original implementation had no alignment adjustment code and always rendered left-aligned.)
- "I am opening a PR to fix it" — **issue body** (Reporter submitted a PR; the fix was incorporated (possibly under a different PR or as part of the 3.116.0 redesign).)
- "This should be closed yeah?" — **comment by trisolari, 2023-05-11** (Community observer noticed the issue is resolved, confirming the fix exists in a released version.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/CanvasExtensions.cs` | 49-55 | direct | Obsolete DrawShapedText(SKShaper, string, float, float, SKPaint) now passes paint.TextAlign to the new overload — the old alignment-ignoring behavior is gone |
| `source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/CanvasExtensions.cs` | 91-102 | direct | Core DrawShapedText implementation calculates xOffset from textAlign (Center halves width, Right uses full width) and applies it when calling canvas.DrawText(textBlob, xOffset, 0, paint) — alignment is correctly handled |

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.93 (93%) |
| Reason | Code investigation confirms the alignment logic exists in the current codebase (CanvasExtensions.cs lines 91-102) and was added in 3.116.0. The old overloads are marked [Obsolete] and now pass paint.TextAlign. A community comment from 2023 also notes the issue should be closed. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply bug and HarfBuzz labels | labels=type/bug, area/SkiaSharp.HarfBuzz, tenet/compatibility |
| add-comment | medium | 0.93 (93%) | Notify reporter that issue is fixed in 3.116.0 | — |
| close-issue | medium | 0.93 (93%) | Close as fixed in 3.116.0 | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
This bug was fixed in SkiaSharp 3.116.0. New `DrawShapedText` overloads were added that accept an explicit `SKTextAlign` parameter, and the old overloads (taking only `SKPaint`) were marked `[Obsolete]` and updated to pass `paint.TextAlign` correctly.

If you are still on SkiaSharp 2.x, you can upgrade to 3.116.0+ to get the fix. With the new API, prefer the explicit overload:
```csharp
canvas.DrawShapedText(shaper, "test", x, y, SKTextAlign.Center, font, paint);
```
Closing as fixed.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1909,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T23:45:00Z"
  },
  "summary": "DrawShapedText old overloads (accepting only SKPaint) ignored SKPaint.TextAlign, causing text drawn via SKShaper to always render left-aligned regardless of the configured TextAlign; the fix was shipped in SkiaSharp 3.116.0 with new explicit-SKTextAlign overloads and alignment logic in CanvasExtensions.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.98
    },
    "area": {
      "value": "area/SkiaSharp.HarfBuzz",
      "confidence": 0.97
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "DrawShapedText ignores SKPaint.TextAlign — text always draws left-aligned",
      "reproQuality": "complete",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create SKPaint with TextAlign = SKTextAlign.Center",
        "Call canvas.DrawShapedText(shaper, text, x, y, paint)",
        "Observe text is rendered left-aligned, ignoring the Center alignment"
      ],
      "codeSnippets": [
        "using var paint = new SKPaint { TextAlign = SKTextAlign.Center };\ncanvas.DrawShapedText(shaper, \"test\", width / 2f, (height / 2f) + (paint.FontMetrics.XHeight / 2), paint);"
      ],
      "environmentDetails": "SkiaSharp 2.x era; SkiaSharp.HarfBuzz",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1909",
          "description": "Original bug report with screenshots"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "unlikely",
      "relevanceReason": "SkiaSharp 3.116.0 added explicit SKTextAlign overloads for DrawShapedText and the old overloads now pass paint.TextAlign. The core method in CanvasExtensions.cs lines 92-98 contains the alignment adjustment logic."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.95,
      "reason": "The SkiaSharp 3.116.0 changelog shows new DrawShapedText overloads with explicit SKTextAlign were added, and the old SKPaint-only overloads were marked [Obsolete]. Current CanvasExtensions.cs lines 50-55 show the obsolete overload passes paint.TextAlign to the new implementation. Lines 92-98 implement the alignment offset calculation.",
      "fixedInVersion": "3.116.0"
    }
  },
  "analysis": {
    "summary": "The DrawShapedText extension method in SkiaSharp.HarfBuzz.CanvasExtensions did not honor SKPaint.TextAlign in its original implementation. SkiaSharp 3.116.0 introduced new overloads with an explicit SKTextAlign parameter and added xOffset alignment logic (lines 92-98 of CanvasExtensions.cs). The old overloads were deprecated and now delegate to paint.TextAlign. This issue is effectively fixed in 3.116.0+.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/CanvasExtensions.cs",
        "lines": "49-55",
        "finding": "Obsolete DrawShapedText(SKShaper, string, float, float, SKPaint) now passes paint.TextAlign to the new overload — the old alignment-ignoring behavior is gone",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/CanvasExtensions.cs",
        "lines": "91-102",
        "finding": "Core DrawShapedText implementation calculates xOffset from textAlign (Center halves width, Right uses full width) and applies it when calling canvas.DrawText(textBlob, xOffset, 0, paint) — alignment is correctly handled",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "When drawing text with DrawShapedText, the specified TextAlign in SKPaint is ignored.",
        "source": "issue body",
        "interpretation": "Root cause: the original implementation had no alignment adjustment code and always rendered left-aligned."
      },
      {
        "text": "I am opening a PR to fix it",
        "source": "issue body",
        "interpretation": "Reporter submitted a PR; the fix was incorporated (possibly under a different PR or as part of the 3.116.0 redesign)."
      },
      {
        "text": "This should be closed yeah?",
        "source": "comment by trisolari, 2023-05-11",
        "interpretation": "Community observer noticed the issue is resolved, confirming the fix exists in a released version."
      }
    ],
    "rationale": "This is a confirmed bug (wrong-output) in SkiaSharp.HarfBuzz.CanvasExtensions where DrawShapedText ignored paint.TextAlign. Code investigation confirms the fix is present in the current codebase: the old overloads are now marked [Obsolete] and pass paint.TextAlign, while new overloads with explicit SKTextAlign were added in 3.116.0 along with alignment offset logic. Suggesting close-as-fixed with version 3.116.0."
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.93,
      "reason": "Code investigation confirms the alignment logic exists in the current codebase (CanvasExtensions.cs lines 91-102) and was added in 3.116.0. The old overloads are marked [Obsolete] and now pass paint.TextAlign. A community comment from 2023 also notes the issue should be closed.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug and HarfBuzz labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/SkiaSharp.HarfBuzz",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Notify reporter that issue is fixed in 3.116.0",
        "risk": "medium",
        "confidence": 0.93,
        "comment": "This bug was fixed in SkiaSharp 3.116.0. New `DrawShapedText` overloads were added that accept an explicit `SKTextAlign` parameter, and the old overloads (taking only `SKPaint`) were marked `[Obsolete]` and updated to pass `paint.TextAlign` correctly.\n\nIf you are still on SkiaSharp 2.x, you can upgrade to 3.116.0+ to get the fix. With the new API, prefer the explicit overload:\n```csharp\ncanvas.DrawShapedText(shaper, \"test\", x, y, SKTextAlign.Center, font, paint);\n```\nClosing as fixed."
      },
      {
        "type": "close-issue",
        "description": "Close as fixed in 3.116.0",
        "risk": "medium",
        "confidence": 0.93,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
