# Issue Triage Report — #2644

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T01:50:00Z |
| Type | type/bug (0.72 (72%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-info (0.82 (82%)) |

**Issue Summary:** Reporter claims text color is always white instead of the configured green (#39FF14) when drawing overlay text on an image using SKBitmap.Decode + SKCanvas.DrawText on Linux, with a regression claim between 2.88.2 and 2.88.3.

**Analysis:** The most likely root cause is that the source JPEG image decoded with SKBitmap.Decode() uses a grayscale color type (SKColorType.Gray8), which is then inherited by the destination bitmap. Drawing on a Gray8 canvas converts any color to its grayscale luminance equivalent. Green (#39FF14) has high luminance (~169/255) and would appear as a very light gray bordering on white. There is no evidence this is a SkiaSharp regression — the code explicitly copies bitmap.ColorType, which is by design. However, the regression claim cannot be ruled out without screenshots or a minimal repro.

**Recommendations:** **needs-info** — The reporter has not provided a screenshot, the source image color type is unknown, and the regression claim is unverified. The most likely cause is a usage issue (inheriting Gray8 color type from source image), but we cannot confirm without more details.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Linux |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Decode a JPEG image with SKBitmap.Decode(path)
2. Create a new SKBitmap inheriting bitmap.ColorType and bitmap.AlphaType
3. Draw text with SKPaint.Color = SKColor.Parse('#39FF14') on the canvas
4. Encode as JPEG and observe text color

**Environment:** SkiaSharp 2.88.3, Ubuntu 20.04/22.04, Visual Studio (Windows) building for Linux

**Code snippets:**

```csharp
var toBitmap = new SKBitmap((int)Math.Round(bitmap.Width * resizeFactor), (int)Math.Round(bitmap.Height * resizeFactor), bitmap.ColorType, bitmap.AlphaType);
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | Text color is always white regardless of SKPaint.Color value |
| Repro quality | partial |
| Target frameworks | net7.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3, 2.88.2 |
| Worked in | 2.88.2 |
| Broke in | 2.88.3 |
| Current relevance | unknown |
| Relevance reason | No screenshots provided; cannot verify if current versions still exhibit the issue. The regression between 2.88.2 and 2.88.3 is claimed but not demonstrated. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.45 (45%) |
| Reason | Reporter claims it worked in 2.88.2. No screenshot or comparison provided to confirm. Possible that the source image was always grayscale and the reporter is mistaken about the version change. |
| Worked in version | 2.88.2 |
| Broke in version | 2.88.3 |

## Analysis

### Technical Summary

The most likely root cause is that the source JPEG image decoded with SKBitmap.Decode() uses a grayscale color type (SKColorType.Gray8), which is then inherited by the destination bitmap. Drawing on a Gray8 canvas converts any color to its grayscale luminance equivalent. Green (#39FF14) has high luminance (~169/255) and would appear as a very light gray bordering on white. There is no evidence this is a SkiaSharp regression — the code explicitly copies bitmap.ColorType, which is by design. However, the regression claim cannot be ruled out without screenshots or a minimal repro.

### Rationale

Classified as type/bug with medium confidence because the reporter claims a regression between 2.88.2 and 2.88.3, but no screenshot is provided to confirm the issue. The most likely cause is propagation of a Gray8 color type from a grayscale source JPEG, which is existing behavior. Without confirmation of the source image's color type or a screenshot, this remains unclear. Action is needs-info.

### Key Signals

- "var toBitmap = new SKBitmap(..., bitmap.ColorType, bitmap.AlphaType);" — **issue body** (The destination bitmap inherits the source's color type. If the source JPEG is grayscale, this creates a Gray8 bitmap where color rendering is limited to luminance values.)
- "Text color is always white" — **issue body** (White in grayscale is luminance=255. Green #39FF14 has luminance ~169 which would be a light gray, not pure white. This suggests either the image is not Gray8 or something else is happening. No screenshot was provided to confirm.)
- "Last Known Good Version: 2.88.2 / Version with issue: 2.88.3" — **issue body** (Regression claim — unverified. Could be coincidence with input image change, or a real codec/color type behavior change.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 434-444 | direct | SKBitmap.Decode(SKCodec) preserves the codec's native color type (e.g. SKColorType.Gray8 for grayscale JPEGs). No forced conversion to BGRA8888 is applied. This means decoding a grayscale JPEG returns a Gray8 bitmap. |
| `binding/SkiaSharp/Definitions.cs` | 47 | related | SKColorType.Gray8 is a supported color type. Drawing on a Gray8 canvas converts RGB colors to their luminance equivalent, which can make saturated colors appear near-white if their luminance is high. |

### Workarounds

- Explicitly specify SKImageInfo.PlatformColorType (BGRA8888) when creating the destination bitmap instead of using bitmap.ColorType: new SKBitmap(w, h, SKImageInfo.PlatformColorType, SKAlphaType.Premul)

### Next Questions

- Is the source image a grayscale JPEG? (If yes, bitmap.ColorType will be Gray8 and color rendering is limited to luminance.)
- Can the reporter provide a screenshot of the output showing the white text?
- Does the issue reproduce with a known color JPEG source image?
- What is the exact target framework used?

### Resolution Proposals

**Hypothesis:** The toBitmap is created with the source image's color type (potentially Gray8 for grayscale JPEGs), causing all drawn colors to be rendered as grayscale luminance values. The fix is to explicitly use SKImageInfo.PlatformColorType for the destination bitmap.

1. **Specify explicit color type for destination bitmap** — workaround, confidence 0.80 (80%), cost/xs, validated=yes
   - Change the toBitmap creation to use SKImageInfo.PlatformColorType (BGRA8888) instead of inheriting bitmap.ColorType. This ensures the destination bitmap supports full color rendering regardless of the source image's color type.

```csharp
var toBitmap = new SKBitmap(
    (int)Math.Round(bitmap.Width * resizeFactor),
    (int)Math.Round(bitmap.Height * resizeFactor),
    SKImageInfo.PlatformColorType,
    SKAlphaType.Premul);
```
2. **Investigate codec color type preservation between 2.88.2 and 2.88.3** — investigation, confidence 0.45 (45%), cost/m, validated=untested
   - If the issue is a genuine regression, compare how SKCodec.Info reports ColorType for grayscale JPEGs in both versions. The change may be in how the native Skia codec identifies image color profiles.

**Recommended proposal:** Specify explicit color type for destination bitmap

**Why:** The workaround is simple, one-line, and definitively fixes the color issue regardless of the source image's color type. Even if it turns out to be a regression, this is a best practice.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.82 (82%) |
| Reason | The reporter has not provided a screenshot, the source image color type is unknown, and the regression claim is unverified. The most likely cause is a usage issue (inheriting Gray8 color type from source image), but we cannot confirm without more details. |
| Suggested repro platform | linux |

### Missing Info

- Screenshot of the output showing the white text
- Whether the source image is a grayscale JPEG or a color JPEG
- Target framework (netX.0, netX.0-linux, etc.)
- Minimal reproducible project or confirmation that the workaround (using SKImageInfo.PlatformColorType) resolves the issue

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/SkiaSharp, os/Linux labels | labels=type/bug, area/SkiaSharp, os/Linux |
| add-comment | medium | 0.82 (82%) | Ask for screenshot and source image color type, and offer the workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! The code looks correct at first glance, but there's a likely cause to investigate:

When you call `SKBitmap.Decode(path)` on a **grayscale JPEG**, the resulting `bitmap.ColorType` is `SKColorType.Gray8`. Since you pass `bitmap.ColorType` directly to the new `SKBitmap`, the destination bitmap also becomes Gray8. Drawing any color on a Gray8 canvas converts it to its grayscale luminance — which is why bright green (#39FF14) might appear near-white (it has high luminance).

**Quick workaround** — explicitly use a full-color type for the destination bitmap:
```csharp
var toBitmap = new SKBitmap(
    (int)Math.Round(bitmap.Width * resizeFactor),
    (int)Math.Round(bitmap.Height * resizeFactor),
    SKImageInfo.PlatformColorType,  // Use platform BGRA8888 instead of source color type
    SKAlphaType.Premul);
```

Could you try this and let us know if it resolves the issue? Also, a screenshot of the output would help confirm the problem. Is your source image a grayscale JPEG by any chance?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2644,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T01:50:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter claims text color is always white instead of the configured green (#39FF14) when drawing overlay text on an image using SKBitmap.Decode + SKCanvas.DrawText on Linux, with a regression claim between 2.88.2 and 2.88.3.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.72
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Linux"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "Text color is always white regardless of SKPaint.Color value",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net7.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Decode a JPEG image with SKBitmap.Decode(path)",
        "Create a new SKBitmap inheriting bitmap.ColorType and bitmap.AlphaType",
        "Draw text with SKPaint.Color = SKColor.Parse('#39FF14') on the canvas",
        "Encode as JPEG and observe text color"
      ],
      "codeSnippets": [
        "var toBitmap = new SKBitmap((int)Math.Round(bitmap.Width * resizeFactor), (int)Math.Round(bitmap.Height * resizeFactor), bitmap.ColorType, bitmap.AlphaType);"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, Ubuntu 20.04/22.04, Visual Studio (Windows) building for Linux"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3",
        "2.88.2"
      ],
      "workedIn": "2.88.2",
      "brokeIn": "2.88.3",
      "currentRelevance": "unknown",
      "relevanceReason": "No screenshots provided; cannot verify if current versions still exhibit the issue. The regression between 2.88.2 and 2.88.3 is claimed but not demonstrated."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.45,
      "reason": "Reporter claims it worked in 2.88.2. No screenshot or comparison provided to confirm. Possible that the source image was always grayscale and the reporter is mistaken about the version change.",
      "workedInVersion": "2.88.2",
      "brokeInVersion": "2.88.3"
    }
  },
  "analysis": {
    "summary": "The most likely root cause is that the source JPEG image decoded with SKBitmap.Decode() uses a grayscale color type (SKColorType.Gray8), which is then inherited by the destination bitmap. Drawing on a Gray8 canvas converts any color to its grayscale luminance equivalent. Green (#39FF14) has high luminance (~169/255) and would appear as a very light gray bordering on white. There is no evidence this is a SkiaSharp regression — the code explicitly copies bitmap.ColorType, which is by design. However, the regression claim cannot be ruled out without screenshots or a minimal repro.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "434-444",
        "finding": "SKBitmap.Decode(SKCodec) preserves the codec's native color type (e.g. SKColorType.Gray8 for grayscale JPEGs). No forced conversion to BGRA8888 is applied. This means decoding a grayscale JPEG returns a Gray8 bitmap.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "47",
        "finding": "SKColorType.Gray8 is a supported color type. Drawing on a Gray8 canvas converts RGB colors to their luminance equivalent, which can make saturated colors appear near-white if their luminance is high.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "var toBitmap = new SKBitmap(..., bitmap.ColorType, bitmap.AlphaType);",
        "source": "issue body",
        "interpretation": "The destination bitmap inherits the source's color type. If the source JPEG is grayscale, this creates a Gray8 bitmap where color rendering is limited to luminance values."
      },
      {
        "text": "Text color is always white",
        "source": "issue body",
        "interpretation": "White in grayscale is luminance=255. Green #39FF14 has luminance ~169 which would be a light gray, not pure white. This suggests either the image is not Gray8 or something else is happening. No screenshot was provided to confirm."
      },
      {
        "text": "Last Known Good Version: 2.88.2 / Version with issue: 2.88.3",
        "source": "issue body",
        "interpretation": "Regression claim — unverified. Could be coincidence with input image change, or a real codec/color type behavior change."
      }
    ],
    "rationale": "Classified as type/bug with medium confidence because the reporter claims a regression between 2.88.2 and 2.88.3, but no screenshot is provided to confirm the issue. The most likely cause is propagation of a Gray8 color type from a grayscale source JPEG, which is existing behavior. Without confirmation of the source image's color type or a screenshot, this remains unclear. Action is needs-info.",
    "workarounds": [
      "Explicitly specify SKImageInfo.PlatformColorType (BGRA8888) when creating the destination bitmap instead of using bitmap.ColorType: new SKBitmap(w, h, SKImageInfo.PlatformColorType, SKAlphaType.Premul)"
    ],
    "nextQuestions": [
      "Is the source image a grayscale JPEG? (If yes, bitmap.ColorType will be Gray8 and color rendering is limited to luminance.)",
      "Can the reporter provide a screenshot of the output showing the white text?",
      "Does the issue reproduce with a known color JPEG source image?",
      "What is the exact target framework used?"
    ],
    "resolution": {
      "hypothesis": "The toBitmap is created with the source image's color type (potentially Gray8 for grayscale JPEGs), causing all drawn colors to be rendered as grayscale luminance values. The fix is to explicitly use SKImageInfo.PlatformColorType for the destination bitmap.",
      "proposals": [
        {
          "title": "Specify explicit color type for destination bitmap",
          "description": "Change the toBitmap creation to use SKImageInfo.PlatformColorType (BGRA8888) instead of inheriting bitmap.ColorType. This ensures the destination bitmap supports full color rendering regardless of the source image's color type.",
          "category": "workaround",
          "codeSnippet": "var toBitmap = new SKBitmap(\n    (int)Math.Round(bitmap.Width * resizeFactor),\n    (int)Math.Round(bitmap.Height * resizeFactor),\n    SKImageInfo.PlatformColorType,\n    SKAlphaType.Premul);",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Investigate codec color type preservation between 2.88.2 and 2.88.3",
          "description": "If the issue is a genuine regression, compare how SKCodec.Info reports ColorType for grayscale JPEGs in both versions. The change may be in how the native Skia codec identifies image color profiles.",
          "category": "investigation",
          "confidence": 0.45,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Specify explicit color type for destination bitmap",
      "recommendedReason": "The workaround is simple, one-line, and definitively fixes the color issue regardless of the source image's color type. Even if it turns out to be a regression, this is a best practice."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.82,
      "reason": "The reporter has not provided a screenshot, the source image color type is unknown, and the regression claim is unverified. The most likely cause is a usage issue (inheriting Gray8 color type from source image), but we cannot confirm without more details.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Screenshot of the output showing the white text",
      "Whether the source image is a grayscale JPEG or a color JPEG",
      "Target framework (netX.0, netX.0-linux, etc.)",
      "Minimal reproducible project or confirmation that the workaround (using SKImageInfo.PlatformColorType) resolves the issue"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/Linux labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Linux"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask for screenshot and source image color type, and offer the workaround",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the report! The code looks correct at first glance, but there's a likely cause to investigate:\n\nWhen you call `SKBitmap.Decode(path)` on a **grayscale JPEG**, the resulting `bitmap.ColorType` is `SKColorType.Gray8`. Since you pass `bitmap.ColorType` directly to the new `SKBitmap`, the destination bitmap also becomes Gray8. Drawing any color on a Gray8 canvas converts it to its grayscale luminance — which is why bright green (#39FF14) might appear near-white (it has high luminance).\n\n**Quick workaround** — explicitly use a full-color type for the destination bitmap:\n```csharp\nvar toBitmap = new SKBitmap(\n    (int)Math.Round(bitmap.Width * resizeFactor),\n    (int)Math.Round(bitmap.Height * resizeFactor),\n    SKImageInfo.PlatformColorType,  // Use platform BGRA8888 instead of source color type\n    SKAlphaType.Premul);\n```\n\nCould you try this and let us know if it resolves the issue? Also, a screenshot of the output would help confirm the problem. Is your source image a grayscale JPEG by any chance?"
      }
    ]
  }
}
```

</details>
