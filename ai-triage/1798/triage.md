# Issue Triage Report — #1798

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T06:45:00Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp.HarfBuzz (0.95 (95%)) |
| Suggested action | needs-investigation (0.80 (80%)) |

**Issue Summary:** DrawShapedText throws ArgumentNullException when SKPaint has no Typeface set, because the deprecated overload passes paint.GetFont().Typeface (which can be null) directly to SKShaper constructor which rejects null.

**Analysis:** The deprecated DrawShapedText(SKCanvas, string, float, float, SKPaint) overload calls paint.GetFont().Typeface and passes the result directly to new SKShaper(typeface), which throws ArgumentNullException if no Typeface was set on the paint. The fix is to substitute SKTypeface.Default when font.Typeface is null in the deprecated overload chain.

**Recommendations:** **needs-investigation** — Bug is real and reproducible. Fix is clear (null-guard or migration to new API). Deprecated path still crashes; a minor fix or at minimum a better error message would help users who haven't migrated yet.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.HarfBuzz |
| Platforms | — |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create a new SKPaint without setting Typeface
2. Call canvas.DrawShapedText(text, x, y, paint) using the obsolete overload
3. Observe ArgumentNullException: paint.GetFont().Typeface is null, SKShaper constructor rejects null typeface

**Environment:** SkiaSharp 2.80.0; no platform specified

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | exception |
| Error message | ArgumentNullException: typeface is null (in SKShaper constructor) |
| Repro quality | complete |
| Target frameworks | net-unspecified |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The deprecated overload still exists in current source and the null-typeface path still crashes via SKShaper constructor (source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/CanvasExtensions.cs line 33). |

## Analysis

### Technical Summary

The deprecated DrawShapedText(SKCanvas, string, float, float, SKPaint) overload calls paint.GetFont().Typeface and passes the result directly to new SKShaper(typeface), which throws ArgumentNullException if no Typeface was set on the paint. The fix is to substitute SKTypeface.Default when font.Typeface is null in the deprecated overload chain.

### Rationale

This is a real exception caused by a missing null-guard in the deprecated overload. The deprecated API still exists in current source and the crash path is unchanged. Reporter acknowledged in 2024 that the deprecated overload is now the trigger, but the bug (unguarded null) has not been fixed. A simple null-coalescing fallback to SKTypeface.Default would prevent the crash.

### Key Signals

- "Exception due to paint.GetFont().Typeface being null in CanvasExtensions.cs" — **issue body** (The deprecated overload does not validate or default the typeface before passing to SKShaper.)
- "as the constructor doesn't accept null" — **issue body** (SKShaper.ctor explicitly throws ArgumentNullException for null typeface — no fallback behavior.)
- "The outlined crash is probably less-relevant with the deprecation of SKPaint.Font. The overload I was using in this issue is now deprecated anyway." — **comment by Youssef1313 (2024-04-24)** (Reporter considers it a lower-priority fix since the API is deprecated; however, the deprecated path still crashes users who haven't migrated.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/CanvasExtensions.cs` | 19-21 | direct | Obsolete overload DrawShapedText(canvas, text, x, y, paint) delegates to DrawShapedText(..., paint.GetFont(), paint) — paint.GetFont().Typeface can be null if no Typeface was explicitly set |
| `source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/CanvasExtensions.cs` | 28-35 | direct | DrawShapedText(..., SKFont font, SKPaint paint) creates SKShaper with font.Typeface at line 33 — no null-check or fallback, so if font.Typeface is null an ArgumentNullException propagates to the caller |
| `source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/SKShaper.cs` | 15-17 | direct | SKShaper(SKTypeface typeface) constructor: Typeface = typeface ?? throw new ArgumentNullException(nameof(typeface)) — explicitly rejects null, confirming the crash site |

### Workarounds

- Set paint.Typeface explicitly before calling the deprecated DrawShapedText overload: paint.Typeface = SKTypeface.Default;
- Migrate to the non-obsolete overload DrawShapedText(text, x, y, font, paint) providing an SKFont with a non-null Typeface

### Next Questions

- Should the deprecated overload silently fall back to SKTypeface.Default, or should it throw a more descriptive exception?
- Is there a test covering the null-typeface path in the deprecated overload?

### Resolution Proposals

**Hypothesis:** The deprecated overload should guard against null Typeface by falling back to SKTypeface.Default before constructing SKShaper.

1. **Add null-typeface fallback in deprecated overload** — fix, confidence 0.82 (82%), cost/xs, validated=yes
   - In the non-font overload chain (lines 28-35 of CanvasExtensions.cs), replace font.Typeface with font.Typeface ?? SKTypeface.Default when constructing SKShaper. This prevents the crash for callers who haven't migrated to the new API.

```csharp
using var shaper = new SKShaper(font.Typeface ?? SKTypeface.Default);
```
2. **Use new API overload with explicit SKFont** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Switch from the deprecated DrawShapedText(paint) overload to DrawShapedText(text, x, y, font, paint) where font is created with an explicit SKTypeface.

```csharp
var font = new SKFont(SKTypeface.Default, paint.TextSize);
canvas.DrawShapedText(text, x, y, font, paint);
```

**Recommended proposal:** Use new API overload with explicit SKFont

**Why:** The deprecated overload is already marked for removal; callers should migrate to the modern API rather than rely on a patch to a deprecated code path.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.80 (80%) |
| Reason | Bug is real and reproducible. Fix is clear (null-guard or migration to new API). Deprecated path still crashes; a minor fix or at minimum a better error message would help users who haven't migrated yet. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, HarfBuzz area, and reliability tenet labels | labels=type/bug, area/SkiaSharp.HarfBuzz, tenet/reliability |
| add-comment | medium | 0.82 (82%) | Acknowledge bug, explain root cause, provide workaround and migration guidance | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report! The root cause is that the deprecated `DrawShapedText(canvas, text, x, y, paint)` overload calls `paint.GetFont().Typeface` and passes the result directly to `new SKShaper(typeface)`, which throws `ArgumentNullException` when no `Typeface` is set on the paint.

**Workaround (immediate fix):** Set `Typeface` explicitly on the paint before calling the deprecated overload:
```csharp
paint.Typeface = SKTypeface.Default; // or your desired font
canvas.DrawShapedText(line, 0, 0, paint);
```

**Recommended migration:** Switch to the modern overload that takes an explicit `SKFont`:
```csharp
var font = new SKFont(SKTypeface.Default, paint.TextSize);
canvas.DrawShapedText(line, 0, 0, font, paint);
```

Note that as of later versions, `SKPaint.Font` / the `DrawShapedText(paint)` overload are deprecated in favour of the `SKFont`-based APIs.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1798,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T06:45:00Z"
  },
  "summary": "DrawShapedText throws ArgumentNullException when SKPaint has no Typeface set, because the deprecated overload passes paint.GetFont().Typeface (which can be null) directly to SKShaper constructor which rejects null.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp.HarfBuzz",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "ArgumentNullException: typeface is null (in SKShaper constructor)",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net-unspecified"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a new SKPaint without setting Typeface",
        "Call canvas.DrawShapedText(text, x, y, paint) using the obsolete overload",
        "Observe ArgumentNullException: paint.GetFont().Typeface is null, SKShaper constructor rejects null typeface"
      ],
      "environmentDetails": "SkiaSharp 2.80.0; no platform specified",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The deprecated overload still exists in current source and the null-typeface path still crashes via SKShaper constructor (source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/CanvasExtensions.cs line 33)."
    }
  },
  "analysis": {
    "summary": "The deprecated DrawShapedText(SKCanvas, string, float, float, SKPaint) overload calls paint.GetFont().Typeface and passes the result directly to new SKShaper(typeface), which throws ArgumentNullException if no Typeface was set on the paint. The fix is to substitute SKTypeface.Default when font.Typeface is null in the deprecated overload chain.",
    "rationale": "This is a real exception caused by a missing null-guard in the deprecated overload. The deprecated API still exists in current source and the crash path is unchanged. Reporter acknowledged in 2024 that the deprecated overload is now the trigger, but the bug (unguarded null) has not been fixed. A simple null-coalescing fallback to SKTypeface.Default would prevent the crash.",
    "keySignals": [
      {
        "text": "Exception due to paint.GetFont().Typeface being null in CanvasExtensions.cs",
        "source": "issue body",
        "interpretation": "The deprecated overload does not validate or default the typeface before passing to SKShaper."
      },
      {
        "text": "as the constructor doesn't accept null",
        "source": "issue body",
        "interpretation": "SKShaper.ctor explicitly throws ArgumentNullException for null typeface — no fallback behavior."
      },
      {
        "text": "The outlined crash is probably less-relevant with the deprecation of SKPaint.Font. The overload I was using in this issue is now deprecated anyway.",
        "source": "comment by Youssef1313 (2024-04-24)",
        "interpretation": "Reporter considers it a lower-priority fix since the API is deprecated; however, the deprecated path still crashes users who haven't migrated."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/CanvasExtensions.cs",
        "lines": "19-21",
        "finding": "Obsolete overload DrawShapedText(canvas, text, x, y, paint) delegates to DrawShapedText(..., paint.GetFont(), paint) — paint.GetFont().Typeface can be null if no Typeface was explicitly set",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/CanvasExtensions.cs",
        "lines": "28-35",
        "finding": "DrawShapedText(..., SKFont font, SKPaint paint) creates SKShaper with font.Typeface at line 33 — no null-check or fallback, so if font.Typeface is null an ArgumentNullException propagates to the caller",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/SKShaper.cs",
        "lines": "15-17",
        "finding": "SKShaper(SKTypeface typeface) constructor: Typeface = typeface ?? throw new ArgumentNullException(nameof(typeface)) — explicitly rejects null, confirming the crash site",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Set paint.Typeface explicitly before calling the deprecated DrawShapedText overload: paint.Typeface = SKTypeface.Default;",
      "Migrate to the non-obsolete overload DrawShapedText(text, x, y, font, paint) providing an SKFont with a non-null Typeface"
    ],
    "nextQuestions": [
      "Should the deprecated overload silently fall back to SKTypeface.Default, or should it throw a more descriptive exception?",
      "Is there a test covering the null-typeface path in the deprecated overload?"
    ],
    "resolution": {
      "hypothesis": "The deprecated overload should guard against null Typeface by falling back to SKTypeface.Default before constructing SKShaper.",
      "proposals": [
        {
          "title": "Add null-typeface fallback in deprecated overload",
          "description": "In the non-font overload chain (lines 28-35 of CanvasExtensions.cs), replace font.Typeface with font.Typeface ?? SKTypeface.Default when constructing SKShaper. This prevents the crash for callers who haven't migrated to the new API.",
          "category": "fix",
          "codeSnippet": "using var shaper = new SKShaper(font.Typeface ?? SKTypeface.Default);",
          "confidence": 0.82,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Use new API overload with explicit SKFont",
          "description": "Switch from the deprecated DrawShapedText(paint) overload to DrawShapedText(text, x, y, font, paint) where font is created with an explicit SKTypeface.",
          "category": "workaround",
          "codeSnippet": "var font = new SKFont(SKTypeface.Default, paint.TextSize);\ncanvas.DrawShapedText(text, x, y, font, paint);",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Use new API overload with explicit SKFont",
      "recommendedReason": "The deprecated overload is already marked for removal; callers should migrate to the modern API rather than rely on a patch to a deprecated code path."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.8,
      "reason": "Bug is real and reproducible. Fix is clear (null-guard or migration to new API). Deprecated path still crashes; a minor fix or at minimum a better error message would help users who haven't migrated yet.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, HarfBuzz area, and reliability tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.HarfBuzz",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge bug, explain root cause, provide workaround and migration guidance",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report! The root cause is that the deprecated `DrawShapedText(canvas, text, x, y, paint)` overload calls `paint.GetFont().Typeface` and passes the result directly to `new SKShaper(typeface)`, which throws `ArgumentNullException` when no `Typeface` is set on the paint.\n\n**Workaround (immediate fix):** Set `Typeface` explicitly on the paint before calling the deprecated overload:\n```csharp\npaint.Typeface = SKTypeface.Default; // or your desired font\ncanvas.DrawShapedText(line, 0, 0, paint);\n```\n\n**Recommended migration:** Switch to the modern overload that takes an explicit `SKFont`:\n```csharp\nvar font = new SKFont(SKTypeface.Default, paint.TextSize);\ncanvas.DrawShapedText(line, 0, 0, font, paint);\n```\n\nNote that as of later versions, `SKPaint.Font` / the `DrawShapedText(paint)` overload are deprecated in favour of the `SKFont`-based APIs."
      }
    ]
  }
}
```

</details>
