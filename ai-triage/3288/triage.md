# Issue Triage Report — #3288

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T09:58:39Z |
| Type | type/bug (0.85 (85%)) |
| Area | area/SkiaSharp (0.60 (60%)) |
| Suggested action | close-as-external (0.97 (97%)) |

**Issue Summary:** Reporter filed a bug about ctx.measureText() incorrectly wrapping Chinese text, but the issue is for skia-canvas (a Node.js npm package), not SkiaSharp (.NET). The reporter themselves acknowledged they posted to the wrong repository.

**Analysis:** This issue was filed in the wrong repository. The reporter is using skia-canvas, a Node.js npm package (https://github.com/samizdatco/skia-canvas), and the API ctx.measureText(text, maxWidth) is part of the HTML Canvas 2D Context specification as wrapped by skia-canvas, not a SkiaSharp (.NET) API. The reporter confirmed this themselves with 'Sorry, I just sent wrong place'. SkiaSharp's MeasureText() is on SKFont/SKPaint and has no equivalent maxWidth parameter.

**Recommendations:** **close-as-external** — The issue is for skia-canvas (Node.js npm package), not SkiaSharp (.NET). The reporter confirmed 'Sorry, I just sent wrong place'. The API ctx.measureText() does not exist in SkiaSharp.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/macOS |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Use skia-canvas npm package version ^2.0.2
2. Call ctx.measureText(chineseText, 952) on a string of Chinese characters
3. Observe that the first line's width (1654) greatly exceeds the specified max width (952)

**Environment:** Node.js, skia-canvas ^2.0.2, macOS, Visual Studio Code

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | measureText returns line width exceeding specified maximum width for Chinese text |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 2.88.9 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | Reporter filled SkiaSharp version fields as part of the GitHub template but is actually using skia-canvas (Node.js npm package), not SkiaSharp (.NET). The versions listed in the template do not apply to this issue. |

## Analysis

### Technical Summary

This issue was filed in the wrong repository. The reporter is using skia-canvas, a Node.js npm package (https://github.com/samizdatco/skia-canvas), and the API ctx.measureText(text, maxWidth) is part of the HTML Canvas 2D Context specification as wrapped by skia-canvas, not a SkiaSharp (.NET) API. The reporter confirmed this themselves with 'Sorry, I just sent wrong place'. SkiaSharp's MeasureText() is on SKFont/SKPaint and has no equivalent maxWidth parameter.

### Rationale

The API used (ctx.measureText with a maxWidth argument) is the HTML Canvas 2D API, not a .NET SkiaSharp API. SkiaSharp exposes SKFont.MeasureText() and SKPaint.MeasureText() which return a float and accept a string or byte span, not a maxWidth constraint. The skia-canvas package at npmjs.com/package/skia-canvas is a separate Node.js project. The reporter's own comment 'Sorry, I just sent wrong place' confirms this is not a SkiaSharp issue.

### Key Signals

- "Version: "skia-canvas": "^2.0.2"" — **issue body** (Reporter is using the skia-canvas npm package, not SkiaSharp (.NET))
- "ctx.measureText(fullText, 952)" — **issue body** (This is the HTML Canvas 2D API signature; SkiaSharp's MeasureText() has no maxWidth parameter)
- "Sorry, I just sent wrong place" — **issue comment #2950642103** (Reporter themselves confirmed this was filed in the wrong repository)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPaint.cs` | 305-353 | direct | SkiaSharp's MeasureText() overloads on SKPaint accept string, ReadOnlySpan<char>, byte[], ReadOnlySpan<byte>, or IntPtr — none accept a maxWidth parameter. They delegate to SKFont.MeasureText(). This is a fundamentally different API signature from the HTML Canvas ctx.measureText(text, maxWidth). |
| `binding/SkiaSharp/SKFont.cs` | — | direct | SKFont.MeasureText() returns a float representing the advance width of the text. No line-wrapping or maxWidth concept exists in SkiaSharp's text measurement API, confirming that ctx.measureText(text, maxWidth) from skia-canvas is an unrelated Node.js API. |

### Next Questions

- Should the issue be redirected to the skia-canvas GitHub repository (https://github.com/samizdatco/skia-canvas)?

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.97 (97%) |
| Reason | The issue is for skia-canvas (Node.js npm package), not SkiaSharp (.NET). The reporter confirmed 'Sorry, I just sent wrong place'. The API ctx.measureText() does not exist in SkiaSharp. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug and area/SkiaSharp labels | labels=type/bug, area/SkiaSharp, os/macOS |
| add-comment | medium | 0.97 (97%) | Redirect reporter to skia-canvas and close | — |
| close-issue | medium | 0.97 (97%) | Close as external — wrong repository | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Hi @chriswang- — thanks for the report! It looks like this issue is with the **skia-canvas** npm package (`ctx.measureText()`), which is a separate Node.js project unrelated to SkiaSharp (.NET). As you noted, this was sent to the wrong place.

Please open an issue in the skia-canvas repository instead: https://github.com/samizdatco/skia-canvas/issues

Closing this issue as it is not related to SkiaSharp.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3288,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T09:58:39Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter filed a bug about ctx.measureText() incorrectly wrapping Chinese text, but the issue is for skia-canvas (a Node.js npm package), not SkiaSharp (.NET). The reporter themselves acknowledged they posted to the wrong repository.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.85
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.6
    },
    "platforms": [
      "os/macOS"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "low",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "measureText returns line width exceeding specified maximum width for Chinese text",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Use skia-canvas npm package version ^2.0.2",
        "Call ctx.measureText(chineseText, 952) on a string of Chinese characters",
        "Observe that the first line's width (1654) greatly exceeds the specified max width (952)"
      ],
      "environmentDetails": "Node.js, skia-canvas ^2.0.2, macOS, Visual Studio Code",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "2.88.9"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "Reporter filled SkiaSharp version fields as part of the GitHub template but is actually using skia-canvas (Node.js npm package), not SkiaSharp (.NET). The versions listed in the template do not apply to this issue."
    }
  },
  "analysis": {
    "summary": "This issue was filed in the wrong repository. The reporter is using skia-canvas, a Node.js npm package (https://github.com/samizdatco/skia-canvas), and the API ctx.measureText(text, maxWidth) is part of the HTML Canvas 2D Context specification as wrapped by skia-canvas, not a SkiaSharp (.NET) API. The reporter confirmed this themselves with 'Sorry, I just sent wrong place'. SkiaSharp's MeasureText() is on SKFont/SKPaint and has no equivalent maxWidth parameter.",
    "rationale": "The API used (ctx.measureText with a maxWidth argument) is the HTML Canvas 2D API, not a .NET SkiaSharp API. SkiaSharp exposes SKFont.MeasureText() and SKPaint.MeasureText() which return a float and accept a string or byte span, not a maxWidth constraint. The skia-canvas package at npmjs.com/package/skia-canvas is a separate Node.js project. The reporter's own comment 'Sorry, I just sent wrong place' confirms this is not a SkiaSharp issue.",
    "keySignals": [
      {
        "text": "Version: \"skia-canvas\": \"^2.0.2\"",
        "source": "issue body",
        "interpretation": "Reporter is using the skia-canvas npm package, not SkiaSharp (.NET)"
      },
      {
        "text": "ctx.measureText(fullText, 952)",
        "source": "issue body",
        "interpretation": "This is the HTML Canvas 2D API signature; SkiaSharp's MeasureText() has no maxWidth parameter"
      },
      {
        "text": "Sorry, I just sent wrong place",
        "source": "issue comment #2950642103",
        "interpretation": "Reporter themselves confirmed this was filed in the wrong repository"
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "lines": "305-353",
        "finding": "SkiaSharp's MeasureText() overloads on SKPaint accept string, ReadOnlySpan<char>, byte[], ReadOnlySpan<byte>, or IntPtr — none accept a maxWidth parameter. They delegate to SKFont.MeasureText(). This is a fundamentally different API signature from the HTML Canvas ctx.measureText(text, maxWidth).",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFont.cs",
        "finding": "SKFont.MeasureText() returns a float representing the advance width of the text. No line-wrapping or maxWidth concept exists in SkiaSharp's text measurement API, confirming that ctx.measureText(text, maxWidth) from skia-canvas is an unrelated Node.js API.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "Should the issue be redirected to the skia-canvas GitHub repository (https://github.com/samizdatco/skia-canvas)?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.97,
      "reason": "The issue is for skia-canvas (Node.js npm package), not SkiaSharp (.NET). The reporter confirmed 'Sorry, I just sent wrong place'. The API ctx.measureText() does not exist in SkiaSharp.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug and area/SkiaSharp labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/macOS"
        ]
      },
      {
        "type": "add-comment",
        "description": "Redirect reporter to skia-canvas and close",
        "risk": "medium",
        "confidence": 0.97,
        "comment": "Hi @chriswang- — thanks for the report! It looks like this issue is with the **skia-canvas** npm package (`ctx.measureText()`), which is a separate Node.js project unrelated to SkiaSharp (.NET). As you noted, this was sent to the wrong place.\n\nPlease open an issue in the skia-canvas repository instead: https://github.com/samizdatco/skia-canvas/issues\n\nClosing this issue as it is not related to SkiaSharp."
      },
      {
        "type": "close-issue",
        "description": "Close as external — wrong repository",
        "risk": "medium",
        "confidence": 0.97,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
