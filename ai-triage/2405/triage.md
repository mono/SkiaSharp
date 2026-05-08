# Issue Triage Report — #2405

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T12:01:12Z |
| Type | type/question (0.98 (98%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-not-a-bug (0.92 (92%)) |

**Issue Summary:** User asks how to perform grayscale and binary (threshold) image processing using SkiaSharp.

**Analysis:** Usage question about grayscale and binary image processing. Already answered in comments using SKColorFilter.CreateColorMatrix with luminance weights. APIs confirmed to exist in source code. Issue should be closed as answered.

**Recommendations:** **close-as-not-a-bug** — Question is a usage how-to. Already answered in comments with working code. Reporter confirmed with thanks. Third party also noted the issue should be closed. APIs confirmed in source code.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. User asked: 'How to achieve grayscale processing and binary processing of images?'

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2405#issuecomment-1457716029 — Community answer with grayscale color matrix code snippet

## Analysis

### Technical Summary

Usage question about grayscale and binary image processing. Already answered in comments using SKColorFilter.CreateColorMatrix with luminance weights. APIs confirmed to exist in source code. Issue should be closed as answered.

### Rationale

Issue is explicitly tagged [QUESTION] and asks how to accomplish a common image processing task. A community member provided a correct grayscale solution using SKColorFilter.CreateColorMatrix. The original reporter acknowledged ('Thanks'). A third party noted in March 2024 the issue should be closed. The relevant APIs (CreateColorMatrix, CreateTable) are confirmed in source code.

### Key Signals

- "How to achieve grayscale processing and binary processing of images?" — **issue body** (Classic how-to usage question — no broken behavior reported.)
- "Something like this should work for grayscale." — **comment #issuecomment-1457716029** (Community member provided a working code answer using SKColorFilter.CreateColorMatrix with ITU-R BT.709 luminance weights.)
- "This issue can be marked as solved and be closed." — **comment #issuecomment-2007062613** (Third-party observer confirms the question was answered and issue is ready to close.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKColorFilter.cs` | 71-85 | direct | SKColorFilter.CreateColorMatrix(float[]) and CreateColorMatrix(ReadOnlySpan<float>) are public APIs requiring a 20-element matrix. This enables grayscale conversion by applying luminance weights (e.g., 0.2126, 0.7152, 0.0722) across RGB channels. |
| `binding/SkiaSharp/SKColorFilter.cs` | 101-141 | direct | SKColorFilter.CreateTable(byte[]) and CreateTable(tableA, tableR, tableG, tableB) accept 256-entry lookup tables. Binary/threshold processing can be achieved by composing grayscale matrix with a threshold table mapping values below threshold to 0 and above to 255. |
| `binding/SkiaSharp/SKColorFilter.cs` | 149-156 | related | SKColorFilter.CreateHighContrast(bool grayscale, SKHighContrastConfigInvertStyle, float contrast) offers an alternative one-call grayscale API. |
| `binding/SkiaSharp/SKBitmap.cs` | 539-551 | context | SKBitmap.Decode(string filename) is a valid public API for loading images from file paths, confirming the community code example is correct. |

### Workarounds

- Use SKColorFilter.CreateColorMatrix with luminance weights (0.2126, 0.7152, 0.0722) for grayscale conversion.
- Use SKColorFilter.CreateHighContrast(grayscale: true, SKHighContrastConfigInvertStyle.NoInvert, 0) as a simpler one-call grayscale approach.
- Chain grayscale matrix filter with SKColorFilter.CreateTable threshold table for binary (black/white) output.

### Resolution Proposals

**Hypothesis:** User wants to convert color images to grayscale or binary (black-and-white threshold) representations using SkiaSharp's color filter APIs.

1. **Grayscale via CreateColorMatrix** — fix, confidence 0.95 (95%), cost/xs, validated=yes
   - Apply a 5x4 color matrix with ITU-R BT.709 luminance weights to convert any image to grayscale. Draw the bitmap to a new destination bitmap using SKPaint with the filter to avoid in-place mutation.

```csharp
using var sourceBitmap = SKBitmap.Decode("path/to/image");
using var grayBitmap = new SKBitmap(sourceBitmap.Width, sourceBitmap.Height);
using var canvas = new SKCanvas(grayBitmap);
using var paint = new SKPaint();
paint.ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
{
    0.2126f, 0.7152f, 0.0722f, 0, 0,
    0.2126f, 0.7152f, 0.0722f, 0, 0,
    0.2126f, 0.7152f, 0.0722f, 0, 0,
    0,       0,       0,       1, 0
});
canvas.DrawBitmap(sourceBitmap, 0, 0, paint);
```
2. **Binary (threshold) via composed color filters** — fix, confidence 0.90 (90%), cost/s, validated=yes
   - Compose grayscale matrix with a threshold CreateTable filter to produce a binary black-and-white image. Values above threshold become white (255), below become black (0).

```csharp
var threshold = 128;
var thresholdTable = new byte[256];
for (int i = 0; i < 256; i++) thresholdTable[i] = (byte)(i >= threshold ? 255 : 0);

var grayscaleFilter = SKColorFilter.CreateColorMatrix(new float[]
{
    0.2126f, 0.7152f, 0.0722f, 0, 0,
    0.2126f, 0.7152f, 0.0722f, 0, 0,
    0.2126f, 0.7152f, 0.0722f, 0, 0,
    0,       0,       0,       1, 0
});
var tableFilter = SKColorFilter.CreateTable(null, thresholdTable, thresholdTable, thresholdTable);
var composedFilter = SKColorFilter.CreateCompose(tableFilter, grayscaleFilter);

using var sourceBitmap = SKBitmap.Decode("path/to/image");
using var binaryBitmap = new SKBitmap(sourceBitmap.Width, sourceBitmap.Height);
using var canvas = new SKCanvas(binaryBitmap);
using var paint = new SKPaint { ColorFilter = composedFilter };
canvas.DrawBitmap(sourceBitmap, 0, 0, paint);
```

**Recommended proposal:** Grayscale via CreateColorMatrix

**Why:** Simplest approach, directly matches the answer already provided in comments. Use composed filter for binary output.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.92 (92%) |
| Reason | Question is a usage how-to. Already answered in comments with working code. Reporter confirmed with thanks. Third party also noted the issue should be closed. APIs confirmed in source code. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.98 (98%) | Apply question and SkiaSharp area labels | labels=type/question, area/SkiaSharp |
| add-comment | high | 0.92 (92%) | Post answer with grayscale and binary code examples and close note | — |
| close-issue | medium | 0.90 (90%) | Close as answered usage question | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the question! SkiaSharp provides color filter APIs that handle both cases.

**Grayscale** — use `SKColorFilter.CreateColorMatrix` with ITU-R BT.709 luminance weights:

```csharp
using var sourceBitmap = SKBitmap.Decode("path/to/image");
using var grayBitmap = new SKBitmap(sourceBitmap.Width, sourceBitmap.Height);
using var canvas = new SKCanvas(grayBitmap);
using var paint = new SKPaint();
paint.ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
{
    0.2126f, 0.7152f, 0.0722f, 0, 0,  // red output
    0.2126f, 0.7152f, 0.0722f, 0, 0,  // green output
    0.2126f, 0.7152f, 0.0722f, 0, 0,  // blue output
    0,       0,       0,       1, 0   // alpha unchanged
});
canvas.DrawBitmap(sourceBitmap, 0, 0, paint);
```

**Binary (threshold)** — compose grayscale with a lookup-table filter:

```csharp
var threshold = 128;
var thresholdTable = new byte[256];
for (int i = 0; i < 256; i++) thresholdTable[i] = (byte)(i >= threshold ? 255 : 0);

var grayscaleFilter = SKColorFilter.CreateColorMatrix(new float[]
{
    0.2126f, 0.7152f, 0.0722f, 0, 0,
    0.2126f, 0.7152f, 0.0722f, 0, 0,
    0.2126f, 0.7152f, 0.0722f, 0, 0,
    0,       0,       0,       1, 0
});
var tableFilter = SKColorFilter.CreateTable(null, thresholdTable, thresholdTable, thresholdTable);
var composedFilter = SKColorFilter.CreateCompose(tableFilter, grayscaleFilter);

using var sourceBitmap = SKBitmap.Decode("path/to/image");
using var binaryBitmap = new SKBitmap(sourceBitmap.Width, sourceBitmap.Height);
using var canvas = new SKCanvas(binaryBitmap);
using var paint = new SKPaint { ColorFilter = composedFilter };
canvas.DrawBitmap(sourceBitmap, 0, 0, paint);
```

Closing as answered.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2405,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T12:01:12Z"
  },
  "summary": "User asks how to perform grayscale and binary (threshold) image processing using SkiaSharp.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.98
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    }
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "User asked: 'How to achieve grayscale processing and binary processing of images?'"
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2405#issuecomment-1457716029",
          "description": "Community answer with grayscale color matrix code snippet"
        }
      ]
    }
  },
  "analysis": {
    "summary": "Usage question about grayscale and binary image processing. Already answered in comments using SKColorFilter.CreateColorMatrix with luminance weights. APIs confirmed to exist in source code. Issue should be closed as answered.",
    "rationale": "Issue is explicitly tagged [QUESTION] and asks how to accomplish a common image processing task. A community member provided a correct grayscale solution using SKColorFilter.CreateColorMatrix. The original reporter acknowledged ('Thanks'). A third party noted in March 2024 the issue should be closed. The relevant APIs (CreateColorMatrix, CreateTable) are confirmed in source code.",
    "keySignals": [
      {
        "text": "How to achieve grayscale processing and binary processing of images?",
        "source": "issue body",
        "interpretation": "Classic how-to usage question — no broken behavior reported."
      },
      {
        "text": "Something like this should work for grayscale.",
        "source": "comment #issuecomment-1457716029",
        "interpretation": "Community member provided a working code answer using SKColorFilter.CreateColorMatrix with ITU-R BT.709 luminance weights."
      },
      {
        "text": "This issue can be marked as solved and be closed.",
        "source": "comment #issuecomment-2007062613",
        "interpretation": "Third-party observer confirms the question was answered and issue is ready to close."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKColorFilter.cs",
        "lines": "71-85",
        "finding": "SKColorFilter.CreateColorMatrix(float[]) and CreateColorMatrix(ReadOnlySpan<float>) are public APIs requiring a 20-element matrix. This enables grayscale conversion by applying luminance weights (e.g., 0.2126, 0.7152, 0.0722) across RGB channels.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKColorFilter.cs",
        "lines": "101-141",
        "finding": "SKColorFilter.CreateTable(byte[]) and CreateTable(tableA, tableR, tableG, tableB) accept 256-entry lookup tables. Binary/threshold processing can be achieved by composing grayscale matrix with a threshold table mapping values below threshold to 0 and above to 255.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKColorFilter.cs",
        "lines": "149-156",
        "finding": "SKColorFilter.CreateHighContrast(bool grayscale, SKHighContrastConfigInvertStyle, float contrast) offers an alternative one-call grayscale API.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "539-551",
        "finding": "SKBitmap.Decode(string filename) is a valid public API for loading images from file paths, confirming the community code example is correct.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "Use SKColorFilter.CreateColorMatrix with luminance weights (0.2126, 0.7152, 0.0722) for grayscale conversion.",
      "Use SKColorFilter.CreateHighContrast(grayscale: true, SKHighContrastConfigInvertStyle.NoInvert, 0) as a simpler one-call grayscale approach.",
      "Chain grayscale matrix filter with SKColorFilter.CreateTable threshold table for binary (black/white) output."
    ],
    "resolution": {
      "hypothesis": "User wants to convert color images to grayscale or binary (black-and-white threshold) representations using SkiaSharp's color filter APIs.",
      "proposals": [
        {
          "title": "Grayscale via CreateColorMatrix",
          "description": "Apply a 5x4 color matrix with ITU-R BT.709 luminance weights to convert any image to grayscale. Draw the bitmap to a new destination bitmap using SKPaint with the filter to avoid in-place mutation.",
          "category": "fix",
          "codeSnippet": "using var sourceBitmap = SKBitmap.Decode(\"path/to/image\");\nusing var grayBitmap = new SKBitmap(sourceBitmap.Width, sourceBitmap.Height);\nusing var canvas = new SKCanvas(grayBitmap);\nusing var paint = new SKPaint();\npaint.ColorFilter = SKColorFilter.CreateColorMatrix(new float[]\n{\n    0.2126f, 0.7152f, 0.0722f, 0, 0,\n    0.2126f, 0.7152f, 0.0722f, 0, 0,\n    0.2126f, 0.7152f, 0.0722f, 0, 0,\n    0,       0,       0,       1, 0\n});\ncanvas.DrawBitmap(sourceBitmap, 0, 0, paint);",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Binary (threshold) via composed color filters",
          "description": "Compose grayscale matrix with a threshold CreateTable filter to produce a binary black-and-white image. Values above threshold become white (255), below become black (0).",
          "category": "fix",
          "codeSnippet": "var threshold = 128;\nvar thresholdTable = new byte[256];\nfor (int i = 0; i < 256; i++) thresholdTable[i] = (byte)(i >= threshold ? 255 : 0);\n\nvar grayscaleFilter = SKColorFilter.CreateColorMatrix(new float[]\n{\n    0.2126f, 0.7152f, 0.0722f, 0, 0,\n    0.2126f, 0.7152f, 0.0722f, 0, 0,\n    0.2126f, 0.7152f, 0.0722f, 0, 0,\n    0,       0,       0,       1, 0\n});\nvar tableFilter = SKColorFilter.CreateTable(null, thresholdTable, thresholdTable, thresholdTable);\nvar composedFilter = SKColorFilter.CreateCompose(tableFilter, grayscaleFilter);\n\nusing var sourceBitmap = SKBitmap.Decode(\"path/to/image\");\nusing var binaryBitmap = new SKBitmap(sourceBitmap.Width, sourceBitmap.Height);\nusing var canvas = new SKCanvas(binaryBitmap);\nusing var paint = new SKPaint { ColorFilter = composedFilter };\ncanvas.DrawBitmap(sourceBitmap, 0, 0, paint);",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Grayscale via CreateColorMatrix",
      "recommendedReason": "Simplest approach, directly matches the answer already provided in comments. Use composed filter for binary output."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.92,
      "reason": "Question is a usage how-to. Already answered in comments with working code. Reporter confirmed with thanks. Third party also noted the issue should be closed. APIs confirmed in source code.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and SkiaSharp area labels",
        "risk": "low",
        "confidence": 0.98,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post answer with grayscale and binary code examples and close note",
        "risk": "high",
        "confidence": 0.92,
        "comment": "Thanks for the question! SkiaSharp provides color filter APIs that handle both cases.\n\n**Grayscale** — use `SKColorFilter.CreateColorMatrix` with ITU-R BT.709 luminance weights:\n\n```csharp\nusing var sourceBitmap = SKBitmap.Decode(\"path/to/image\");\nusing var grayBitmap = new SKBitmap(sourceBitmap.Width, sourceBitmap.Height);\nusing var canvas = new SKCanvas(grayBitmap);\nusing var paint = new SKPaint();\npaint.ColorFilter = SKColorFilter.CreateColorMatrix(new float[]\n{\n    0.2126f, 0.7152f, 0.0722f, 0, 0,  // red output\n    0.2126f, 0.7152f, 0.0722f, 0, 0,  // green output\n    0.2126f, 0.7152f, 0.0722f, 0, 0,  // blue output\n    0,       0,       0,       1, 0   // alpha unchanged\n});\ncanvas.DrawBitmap(sourceBitmap, 0, 0, paint);\n```\n\n**Binary (threshold)** — compose grayscale with a lookup-table filter:\n\n```csharp\nvar threshold = 128;\nvar thresholdTable = new byte[256];\nfor (int i = 0; i < 256; i++) thresholdTable[i] = (byte)(i >= threshold ? 255 : 0);\n\nvar grayscaleFilter = SKColorFilter.CreateColorMatrix(new float[]\n{\n    0.2126f, 0.7152f, 0.0722f, 0, 0,\n    0.2126f, 0.7152f, 0.0722f, 0, 0,\n    0.2126f, 0.7152f, 0.0722f, 0, 0,\n    0,       0,       0,       1, 0\n});\nvar tableFilter = SKColorFilter.CreateTable(null, thresholdTable, thresholdTable, thresholdTable);\nvar composedFilter = SKColorFilter.CreateCompose(tableFilter, grayscaleFilter);\n\nusing var sourceBitmap = SKBitmap.Decode(\"path/to/image\");\nusing var binaryBitmap = new SKBitmap(sourceBitmap.Width, sourceBitmap.Height);\nusing var canvas = new SKCanvas(binaryBitmap);\nusing var paint = new SKPaint { ColorFilter = composedFilter };\ncanvas.DrawBitmap(sourceBitmap, 0, 0, paint);\n```\n\nClosing as answered."
      },
      {
        "type": "close-issue",
        "description": "Close as answered usage question",
        "risk": "medium",
        "confidence": 0.9,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
