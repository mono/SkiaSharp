# Issue Triage Report — #1699

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T21:22:58Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-not-a-bug (0.85 (85%)) |

**Issue Summary:** SKColorFilter.CreateColorMatrix produces wrong output in SkiaSharp 2.80.2 vs 1.68.3 when the 5th column (translation/offset) contains non-zero values, because the upstream Skia library changed the color matrix translation column convention from 0–255 to 0–1 (normalized) in the v2 era.

**Analysis:** The underlying Skia C++ library changed the color matrix format between the Skia milestones used by SkiaSharp 1.x and 2.x. In SkiaSharp 2.x, the 5th column (translation offset) of the 4×5 color matrix must be in the normalized 0–1 range; SkiaSharp 1.x used the older 0–255 range. SKColorFilter.CreateColorMatrix passes the float array directly to sk_colorfilter_new_color_matrix in the C API without any transformation, so this is an upstream convention change, not a SkiaSharp binding bug.

**Recommendations:** **close-as-not-a-bug** — Current behavior is correct per the upstream Skia API. The translation column of the color matrix changed from 0-255 to 0-1 normalized values in SkiaSharp 2.x. The workaround (divide by 255) is confirmed by the reporter.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/iOS |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create a Xamarin.Forms / MAUI iOS app
2. Apply SKColorFilter.CreateColorMatrix with non-zero values in the 5th column
3. Observe the rendered output — colors are incorrect vs SkiaSharp 1.68.3

**Environment:** iOS 14.5, SkiaSharp 2.80.2, Visual Studio for Mac 8.9.8

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1699#issuecomment-973433370 — Community workaround: divide 5th-column values by 255 to normalize to 0-1 range

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | Color filter produces visually wrong image when 5th column (offset) has non-zero values |
| Repro quality | partial |
| Target frameworks | net6.0-ios |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.3, 2.80.2 |
| Worked in | 1.68.3 |
| Broke in | 2.80.2 |
| Current relevance | unlikely |
| Relevance reason | The normalized 0–1 convention is consistent across all current Skia versions. The reporter confirmed the workaround (divide by 255) resolves the issue. No code change needed in SkiaSharp. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.90 (90%) |
| Reason | Works in 1.68.3 with 0-255 offset values; produces wrong output in 2.80.2. Upstream Skia changed color matrix translation column convention to normalized 0-1 range. |
| Worked in version | 1.68.3 |
| Broke in version | 2.80.2 |

## Analysis

### Technical Summary

The underlying Skia C++ library changed the color matrix format between the Skia milestones used by SkiaSharp 1.x and 2.x. In SkiaSharp 2.x, the 5th column (translation offset) of the 4×5 color matrix must be in the normalized 0–1 range; SkiaSharp 1.x used the older 0–255 range. SKColorFilter.CreateColorMatrix passes the float array directly to sk_colorfilter_new_color_matrix in the C API without any transformation, so this is an upstream convention change, not a SkiaSharp binding bug.

### Rationale

This is a real regression from 1.x to 2.x, but it originates from an upstream Skia convention change in the color matrix format. The SkiaSharp C# binding simply passes the matrix through to Skia's native function without any scaling, which is by design. The current 2.x behavior (0-1 normalized offset column) is correct per the current Skia API. A workaround is confirmed: divide the translation column values by 255. This should be closed as not-a-bug with documentation of the migration workaround.

### Key Signals

- "the fifth column of color-transform matrix has value, it will occur the wrong phenomenon in the latest version (2.80.2). However, the version of 1.68.3 works." — **issue body** (Regression in offset/translation column handling between major Skia versions.)
- "Now in 2.x it seems to want values between 0-1. Old Code (Works in 1.x): Color.R * 255. New Code (Works in 2.x): Color.R" — **comment by cabal95** (Community-confirmed root cause: Skia changed the translation column scale from 0-255 to 0-1 (normalized).)
- "Oh, that's cool! ... Your New Code makes me can use the newest version." — **reply by issue reporter** (Reporter confirmed that dividing the offset values by 255 fully resolves the issue.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKColorFilter.cs` | 78-85 | direct | CreateColorMatrix(ReadOnlySpan<float> matrix) validates length=20 then passes the float array directly to SkiaApi.sk_colorfilter_new_color_matrix via P/Invoke — no scaling or normalization is applied, confirming SkiaSharp is a transparent pass-through and the convention comes from Skia itself. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | — | direct | sk_colorfilter_new_color_matrix P/Invoke signature takes a Single* (float pointer) matching the 20-element matrix — no translation from the C# side. The format is dictated entirely by the native Skia library. |

### Workarounds

- Divide 5th-column (translation/offset) values by 255 to normalize them to the 0-1 range expected by SkiaSharp 2.x. Example: replace `Color.R * 255` with `Color.R`.
- Use SKColorFilter.CreateBlendMode for simple solid-color tinting where only the offset column is non-zero.

### Resolution Proposals

**Hypothesis:** The Skia C++ library changed the color matrix translation column convention from 0-255 to 0-1 (normalized) between the Skia milestones used in SkiaSharp 1.x (m67 era) and 2.x (m80+). The SkiaSharp binding passes through unchanged.

1. **Close as not-a-bug with migration comment** — workaround, confidence 0.88 (88%), cost/xs, validated=yes
   - Close the issue as not-a-bug, posting a comment explaining the upstream Skia convention change and providing the workaround: divide 5th-column values by 255.

**Recommended proposal:** Close as not-a-bug with migration comment

**Why:** The current behavior is correct per Skia's normalized color matrix API. The reporter confirmed the community workaround resolves the issue. Closing with a clear explanation and the fix prevents duplicate reports.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.85 (85%) |
| Reason | Current behavior is correct per the upstream Skia API. The translation column of the color matrix changed from 0-255 to 0-1 normalized values in SkiaSharp 2.x. The workaround (divide by 255) is confirmed by the reporter. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug type, SkiaSharp area, iOS platform, and compatibility tenet labels | labels=type/bug, area/SkiaSharp, os/iOS, tenet/compatibility |
| add-comment | medium | 0.88 (88%) | Post explanation of the upstream Skia convention change and confirmed workaround | — |
| close-issue | medium | 0.85 (85%) | Close as not-a-bug — upstream Skia convention change, workaround confirmed | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report and for the community follow-up!

This is a breaking change in the upstream Skia library between SkiaSharp 1.x and 2.x. In SkiaSharp 2.x, Skia changed the color matrix format so that the **5th column (translation/offset)** now expects **normalized values in the 0–1 range** instead of the old 0–255 range.

SkiaSharp passes the matrix directly to Skia's native `sk_colorfilter_new_color_matrix` function without any conversion, so this behavior comes from Skia itself.

**Migration:** Divide your 5th-column values by 255:

```csharp
// SkiaSharp 1.x (old - values in 0-255 range)
var matrix = new float[]
{
    0, 0, 0, 0, (float)(Color.R * 255),
    0, 0, 0, 0, (float)(Color.G * 255),
    0, 0, 0, 0, (float)(Color.B * 255),
    0, 0, 0, 1, 0
};

// SkiaSharp 2.x (new - values normalized to 0-1 range)
var matrix = new float[]
{
    0, 0, 0, 0, (float)Color.R,
    0, 0, 0, 0, (float)Color.G,
    0, 0, 0, 0, (float)Color.B,
    0, 0, 0, 1, 0
};
```

As confirmed in the comments above, the reporter verified this fix resolves the issue. Closing as not-a-bug since this is the correct behavior per the current Skia API.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1699,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T21:22:58Z"
  },
  "summary": "SKColorFilter.CreateColorMatrix produces wrong output in SkiaSharp 2.80.2 vs 1.68.3 when the 5th column (translation/offset) contains non-zero values, because the upstream Skia library changed the color matrix translation column convention from 0–255 to 0–1 (normalized) in the v2 era.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "platforms": [
      "os/iOS"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "Color filter produces visually wrong image when 5th column (offset) has non-zero values",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net6.0-ios"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Xamarin.Forms / MAUI iOS app",
        "Apply SKColorFilter.CreateColorMatrix with non-zero values in the 5th column",
        "Observe the rendered output — colors are incorrect vs SkiaSharp 1.68.3"
      ],
      "environmentDetails": "iOS 14.5, SkiaSharp 2.80.2, Visual Studio for Mac 8.9.8",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1699#issuecomment-973433370",
          "description": "Community workaround: divide 5th-column values by 255 to normalize to 0-1 range"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.3",
        "2.80.2"
      ],
      "workedIn": "1.68.3",
      "brokeIn": "2.80.2",
      "currentRelevance": "unlikely",
      "relevanceReason": "The normalized 0–1 convention is consistent across all current Skia versions. The reporter confirmed the workaround (divide by 255) resolves the issue. No code change needed in SkiaSharp."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.9,
      "reason": "Works in 1.68.3 with 0-255 offset values; produces wrong output in 2.80.2. Upstream Skia changed color matrix translation column convention to normalized 0-1 range.",
      "workedInVersion": "1.68.3",
      "brokeInVersion": "2.80.2"
    }
  },
  "analysis": {
    "summary": "The underlying Skia C++ library changed the color matrix format between the Skia milestones used by SkiaSharp 1.x and 2.x. In SkiaSharp 2.x, the 5th column (translation offset) of the 4×5 color matrix must be in the normalized 0–1 range; SkiaSharp 1.x used the older 0–255 range. SKColorFilter.CreateColorMatrix passes the float array directly to sk_colorfilter_new_color_matrix in the C API without any transformation, so this is an upstream convention change, not a SkiaSharp binding bug.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKColorFilter.cs",
        "lines": "78-85",
        "finding": "CreateColorMatrix(ReadOnlySpan<float> matrix) validates length=20 then passes the float array directly to SkiaApi.sk_colorfilter_new_color_matrix via P/Invoke — no scaling or normalization is applied, confirming SkiaSharp is a transparent pass-through and the convention comes from Skia itself.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "sk_colorfilter_new_color_matrix P/Invoke signature takes a Single* (float pointer) matching the 20-element matrix — no translation from the C# side. The format is dictated entirely by the native Skia library.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "the fifth column of color-transform matrix has value, it will occur the wrong phenomenon in the latest version (2.80.2). However, the version of 1.68.3 works.",
        "source": "issue body",
        "interpretation": "Regression in offset/translation column handling between major Skia versions."
      },
      {
        "text": "Now in 2.x it seems to want values between 0-1. Old Code (Works in 1.x): Color.R * 255. New Code (Works in 2.x): Color.R",
        "source": "comment by cabal95",
        "interpretation": "Community-confirmed root cause: Skia changed the translation column scale from 0-255 to 0-1 (normalized)."
      },
      {
        "text": "Oh, that's cool! ... Your New Code makes me can use the newest version.",
        "source": "reply by issue reporter",
        "interpretation": "Reporter confirmed that dividing the offset values by 255 fully resolves the issue."
      }
    ],
    "rationale": "This is a real regression from 1.x to 2.x, but it originates from an upstream Skia convention change in the color matrix format. The SkiaSharp C# binding simply passes the matrix through to Skia's native function without any scaling, which is by design. The current 2.x behavior (0-1 normalized offset column) is correct per the current Skia API. A workaround is confirmed: divide the translation column values by 255. This should be closed as not-a-bug with documentation of the migration workaround.",
    "workarounds": [
      "Divide 5th-column (translation/offset) values by 255 to normalize them to the 0-1 range expected by SkiaSharp 2.x. Example: replace `Color.R * 255` with `Color.R`.",
      "Use SKColorFilter.CreateBlendMode for simple solid-color tinting where only the offset column is non-zero."
    ],
    "resolution": {
      "hypothesis": "The Skia C++ library changed the color matrix translation column convention from 0-255 to 0-1 (normalized) between the Skia milestones used in SkiaSharp 1.x (m67 era) and 2.x (m80+). The SkiaSharp binding passes through unchanged.",
      "proposals": [
        {
          "title": "Close as not-a-bug with migration comment",
          "description": "Close the issue as not-a-bug, posting a comment explaining the upstream Skia convention change and providing the workaround: divide 5th-column values by 255.",
          "category": "workaround",
          "confidence": 0.88,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Close as not-a-bug with migration comment",
      "recommendedReason": "The current behavior is correct per Skia's normalized color matrix API. The reporter confirmed the community workaround resolves the issue. Closing with a clear explanation and the fix prevents duplicate reports."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.85,
      "reason": "Current behavior is correct per the upstream Skia API. The translation column of the color matrix changed from 0-255 to 0-1 normalized values in SkiaSharp 2.x. The workaround (divide by 255) is confirmed by the reporter.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug type, SkiaSharp area, iOS platform, and compatibility tenet labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/iOS",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post explanation of the upstream Skia convention change and confirmed workaround",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the report and for the community follow-up!\n\nThis is a breaking change in the upstream Skia library between SkiaSharp 1.x and 2.x. In SkiaSharp 2.x, Skia changed the color matrix format so that the **5th column (translation/offset)** now expects **normalized values in the 0–1 range** instead of the old 0–255 range.\n\nSkiaSharp passes the matrix directly to Skia's native `sk_colorfilter_new_color_matrix` function without any conversion, so this behavior comes from Skia itself.\n\n**Migration:** Divide your 5th-column values by 255:\n\n```csharp\n// SkiaSharp 1.x (old - values in 0-255 range)\nvar matrix = new float[]\n{\n    0, 0, 0, 0, (float)(Color.R * 255),\n    0, 0, 0, 0, (float)(Color.G * 255),\n    0, 0, 0, 0, (float)(Color.B * 255),\n    0, 0, 0, 1, 0\n};\n\n// SkiaSharp 2.x (new - values normalized to 0-1 range)\nvar matrix = new float[]\n{\n    0, 0, 0, 0, (float)Color.R,\n    0, 0, 0, 0, (float)Color.G,\n    0, 0, 0, 0, (float)Color.B,\n    0, 0, 0, 1, 0\n};\n```\n\nAs confirmed in the comments above, the reporter verified this fix resolves the issue. Closing as not-a-bug since this is the correct behavior per the current Skia API."
      },
      {
        "type": "close-issue",
        "description": "Close as not-a-bug — upstream Skia convention change, workaround confirmed",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
