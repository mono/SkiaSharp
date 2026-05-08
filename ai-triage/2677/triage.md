# Issue Triage Report — #2677

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T12:27:44Z |
| Type | type/bug (0.55 (55%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.80 (80%)) |

**Issue Summary:** Reporter's SKBitmap crop code has swapped Width/Height arguments and inconsistent scale multipliers, not a SkiaSharp defect.

**Analysis:** The reporter's crop helper has two bugs: (1) SKBitmap constructor arguments are swapped (Height passed as width, Width as height), and (2) the source rectangle uses multiplier 3 for Left/Top/Right but 4 for Bottom, producing a distorted crop. The SkiaSharp DrawBitmap(source, dest) API is implemented correctly — it delegates to SKImage.DrawImage which is backed by Skia's native draw call. This is a user-code error, not a SkiaSharp defect.

**Recommendations:** **close-as-not-a-bug** — Bugs are in the reporter's own code (swapped dimensions, inconsistent multipliers). DrawBitmap API is correct.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Android |
| Backends | — |
| Tenets | — |
| Partner | partner/maui |

## Evidence

### Reproduction

**Code snippets:**

```csharp
SKBitmap croppedBitmap = new SKBitmap((int)frameBounds.Height, (int)frameBounds.Width); // Width/Height swapped
```

```csharp
SKRect sourceRect = new SKRect(frameBounds.Left*3, frameBounds.Top*3, frameBounds.Right*3, frameBounds.Bottom*4); // inconsistent multipliers
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | net7.0-android |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3, 2.88.2 |
| Worked in | 2.88.2 |
| Broke in | 2.88.3 |
| Current relevance | unlikely |
| Relevance reason | The reported regression between 2.88.2 and 2.88.3 is not credible given the user code has obvious bugs (swapped dimensions, inconsistent scale factors). DrawBitmap API did not change between those versions. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | False |
| Confidence | 0.85 (85%) |
| Reason | No API changes to DrawBitmap between 2.88.2 and 2.88.3; the reported symptom is consistent with the bugs in the reporter's own code. |
| Worked in version | — |
| Broke in version | — |

## Analysis

### Technical Summary

The reporter's crop helper has two bugs: (1) SKBitmap constructor arguments are swapped (Height passed as width, Width as height), and (2) the source rectangle uses multiplier 3 for Left/Top/Right but 4 for Bottom, producing a distorted crop. The SkiaSharp DrawBitmap(source, dest) API is implemented correctly — it delegates to SKImage.DrawImage which is backed by Skia's native draw call. This is a user-code error, not a SkiaSharp defect.

### Rationale

Classified as type/bug with low severity because the actual SkiaSharp API is correct; the bugs are in the reporter's user code. suggestedAction is close-as-not-a-bug with a workaround comment explaining the correct usage.

### Key Signals

- "new SKBitmap((int)frameBounds.Height, (int)frameBounds.Width)" — **issue body** (SKBitmap constructor is SKBitmap(width, height) but Width and Height are passed in reversed order.)
- "SKRect sourceRect = new SKRect(frameBounds.Left*3, frameBounds.Top*3, frameBounds.Right*3, frameBounds.Bottom*4)" — **issue body** (Bottom is multiplied by 4 while Left/Top/Right are multiplied by 3, causing a non-uniform crop.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 583-587 | direct | DrawBitmap(SKBitmap, SKRect source, SKRect dest, SKPaint) converts to SKImage and calls DrawImage — standard delegation, no bug. |
| `binding/SkiaSharp/SKCanvas.cs` | 577-581 | related | DrawBitmap(SKBitmap, SKRect dest) also delegates to DrawImage correctly. |

### Workarounds

- Fix the SKBitmap constructor to pass Width then Height: new SKBitmap((int)frameBounds.Width, (int)frameBounds.Height)
- Use consistent multipliers for all four sides of sourceRect, or omit the multiplier if source coordinates are already in pixel space

### Resolution Proposals

**Hypothesis:** The crop looks wrong because the destination bitmap has swapped dimensions and the source rectangle has an off-by-one scale on the Bottom edge.

1. **Fix user code: swap Width/Height and use consistent scale** — workaround, cost/xs, validated=yes
   - Correct the SKBitmap constructor argument order and ensure all four source rect coordinates use the same pixel-density scale factor.

```csharp
public static SKBitmap CropImage1(SKBitmap sourceImage, Rectangle frameBounds)
{
    // SKBitmap(width, height) — do NOT swap
    SKBitmap croppedBitmap = new SKBitmap((int)frameBounds.Width, (int)frameBounds.Height);
    SKRect destRect = new SKRect(0, 0, croppedBitmap.Width, croppedBitmap.Height);
    // If frameBounds is in logical pixels, multiply by display density (devicePixelRatio).
    // Using *3 here only as an example — replace with your actual density value.
    float density = 3f;
    SKRect sourceRect = new SKRect(
        frameBounds.Left  * density,
        frameBounds.Top   * density,
        frameBounds.Right * density,
        frameBounds.Bottom * density);

    using (SKCanvas canvas = new SKCanvas(croppedBitmap))
    {
        canvas.DrawBitmap(sourceImage, sourceRect, destRect);
    }
    return croppedBitmap;
}
```

**Recommended proposal:** workaround-1

**Why:** Fixes both bugs in the reporter's code with minimal change.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.80 (80%) |
| Reason | Bugs are in the reporter's own code (swapped dimensions, inconsistent multipliers). DrawBitmap API is correct. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/SkiaSharp, os/Android, partner/maui | labels=type/bug, area/SkiaSharp, os/Android, partner/maui |
| add-comment | high | 0.80 (80%) | Explain the two bugs in the reporter's code and provide corrected snippet | — |
| close-issue | medium | 0.80 (80%) | Close as not a bug — issue is in user code | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thank you for reporting this! After reviewing the code snippet, it looks like the unexpected crop is caused by two bugs in the helper method rather than a SkiaSharp issue:

1. **Swapped dimensions** — `new SKBitmap((int)frameBounds.Height, (int)frameBounds.Width)` passes Height as the first argument, but `SKBitmap(width, height)` expects Width first. Swap these.

2. **Inconsistent scale multiplier** — `frameBounds.Bottom*4` uses `4` while all other edges use `3`. This stretches the crop vertically. Make sure all four sides use the same density multiplier.

Corrected version:
```csharp
public static SKBitmap CropImage1(SKBitmap sourceImage, Rectangle frameBounds)
{
    SKBitmap croppedBitmap = new SKBitmap((int)frameBounds.Width, (int)frameBounds.Height);
    SKRect destRect = new SKRect(0, 0, croppedBitmap.Width, croppedBitmap.Height);
    float density = 3f; // replace with your actual display density
    SKRect sourceRect = new SKRect(
        frameBounds.Left   * density,
        frameBounds.Top    * density,
        frameBounds.Right  * density,
        frameBounds.Bottom * density);

    using (SKCanvas canvas = new SKCanvas(croppedBitmap))
    {
        canvas.DrawBitmap(sourceImage, sourceRect, destRect);
    }
    return croppedBitmap;
}
```

The `DrawBitmap(source, dest)` API in SkiaSharp is working correctly — it maps the region of the source bitmap defined by `sourceRect` onto `destRect`. If the issue persists after applying these fixes, please reopen with an updated code snippet.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2677,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T12:27:44Z"
  },
  "summary": "Reporter's SKBitmap crop code has swapped Width/Height arguments and inconsistent scale multipliers, not a SkiaSharp defect.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.55
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Android"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "bugSignals": {
      "severity": "low",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net7.0-android"
      ]
    },
    "reproEvidence": {
      "codeSnippets": [
        "SKBitmap croppedBitmap = new SKBitmap((int)frameBounds.Height, (int)frameBounds.Width); // Width/Height swapped",
        "SKRect sourceRect = new SKRect(frameBounds.Left*3, frameBounds.Top*3, frameBounds.Right*3, frameBounds.Bottom*4); // inconsistent multipliers"
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3",
        "2.88.2"
      ],
      "workedIn": "2.88.2",
      "brokeIn": "2.88.3",
      "currentRelevance": "unlikely",
      "relevanceReason": "The reported regression between 2.88.2 and 2.88.3 is not credible given the user code has obvious bugs (swapped dimensions, inconsistent scale factors). DrawBitmap API did not change between those versions."
    },
    "regression": {
      "isRegression": false,
      "confidence": 0.85,
      "reason": "No API changes to DrawBitmap between 2.88.2 and 2.88.3; the reported symptom is consistent with the bugs in the reporter's own code."
    }
  },
  "analysis": {
    "summary": "The reporter's crop helper has two bugs: (1) SKBitmap constructor arguments are swapped (Height passed as width, Width as height), and (2) the source rectangle uses multiplier 3 for Left/Top/Right but 4 for Bottom, producing a distorted crop. The SkiaSharp DrawBitmap(source, dest) API is implemented correctly — it delegates to SKImage.DrawImage which is backed by Skia's native draw call. This is a user-code error, not a SkiaSharp defect.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "583-587",
        "finding": "DrawBitmap(SKBitmap, SKRect source, SKRect dest, SKPaint) converts to SKImage and calls DrawImage — standard delegation, no bug.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "577-581",
        "finding": "DrawBitmap(SKBitmap, SKRect dest) also delegates to DrawImage correctly.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "new SKBitmap((int)frameBounds.Height, (int)frameBounds.Width)",
        "source": "issue body",
        "interpretation": "SKBitmap constructor is SKBitmap(width, height) but Width and Height are passed in reversed order."
      },
      {
        "text": "SKRect sourceRect = new SKRect(frameBounds.Left*3, frameBounds.Top*3, frameBounds.Right*3, frameBounds.Bottom*4)",
        "source": "issue body",
        "interpretation": "Bottom is multiplied by 4 while Left/Top/Right are multiplied by 3, causing a non-uniform crop."
      }
    ],
    "rationale": "Classified as type/bug with low severity because the actual SkiaSharp API is correct; the bugs are in the reporter's user code. suggestedAction is close-as-not-a-bug with a workaround comment explaining the correct usage.",
    "workarounds": [
      "Fix the SKBitmap constructor to pass Width then Height: new SKBitmap((int)frameBounds.Width, (int)frameBounds.Height)",
      "Use consistent multipliers for all four sides of sourceRect, or omit the multiplier if source coordinates are already in pixel space"
    ],
    "resolution": {
      "hypothesis": "The crop looks wrong because the destination bitmap has swapped dimensions and the source rectangle has an off-by-one scale on the Bottom edge.",
      "proposals": [
        {
          "title": "Fix user code: swap Width/Height and use consistent scale",
          "description": "Correct the SKBitmap constructor argument order and ensure all four source rect coordinates use the same pixel-density scale factor.",
          "category": "workaround",
          "effort": "cost/xs",
          "validated": "yes",
          "codeSnippet": "public static SKBitmap CropImage1(SKBitmap sourceImage, Rectangle frameBounds)\n{\n    // SKBitmap(width, height) — do NOT swap\n    SKBitmap croppedBitmap = new SKBitmap((int)frameBounds.Width, (int)frameBounds.Height);\n    SKRect destRect = new SKRect(0, 0, croppedBitmap.Width, croppedBitmap.Height);\n    // If frameBounds is in logical pixels, multiply by display density (devicePixelRatio).\n    // Using *3 here only as an example — replace with your actual density value.\n    float density = 3f;\n    SKRect sourceRect = new SKRect(\n        frameBounds.Left  * density,\n        frameBounds.Top   * density,\n        frameBounds.Right * density,\n        frameBounds.Bottom * density);\n\n    using (SKCanvas canvas = new SKCanvas(croppedBitmap))\n    {\n        canvas.DrawBitmap(sourceImage, sourceRect, destRect);\n    }\n    return croppedBitmap;\n}"
        }
      ],
      "recommendedProposal": "workaround-1",
      "recommendedReason": "Fixes both bugs in the reporter's code with minimal change."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.8,
      "reason": "Bugs are in the reporter's own code (swapped dimensions, inconsistent multipliers). DrawBitmap API is correct.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/Android, partner/maui",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Android",
          "partner/maui"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain the two bugs in the reporter's code and provide corrected snippet",
        "risk": "high",
        "confidence": 0.8,
        "comment": "Thank you for reporting this! After reviewing the code snippet, it looks like the unexpected crop is caused by two bugs in the helper method rather than a SkiaSharp issue:\n\n1. **Swapped dimensions** — `new SKBitmap((int)frameBounds.Height, (int)frameBounds.Width)` passes Height as the first argument, but `SKBitmap(width, height)` expects Width first. Swap these.\n\n2. **Inconsistent scale multiplier** — `frameBounds.Bottom*4` uses `4` while all other edges use `3`. This stretches the crop vertically. Make sure all four sides use the same density multiplier.\n\nCorrected version:\n```csharp\npublic static SKBitmap CropImage1(SKBitmap sourceImage, Rectangle frameBounds)\n{\n    SKBitmap croppedBitmap = new SKBitmap((int)frameBounds.Width, (int)frameBounds.Height);\n    SKRect destRect = new SKRect(0, 0, croppedBitmap.Width, croppedBitmap.Height);\n    float density = 3f; // replace with your actual display density\n    SKRect sourceRect = new SKRect(\n        frameBounds.Left   * density,\n        frameBounds.Top    * density,\n        frameBounds.Right  * density,\n        frameBounds.Bottom * density);\n\n    using (SKCanvas canvas = new SKCanvas(croppedBitmap))\n    {\n        canvas.DrawBitmap(sourceImage, sourceRect, destRect);\n    }\n    return croppedBitmap;\n}\n```\n\nThe `DrawBitmap(source, dest)` API in SkiaSharp is working correctly — it maps the region of the source bitmap defined by `sourceRect` onto `destRect`. If the issue persists after applying these fixes, please reopen with an updated code snippet."
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — issue is in user code",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
