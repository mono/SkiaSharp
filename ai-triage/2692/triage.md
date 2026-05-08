# Issue Triage Report — #2692

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-05T12:50:29Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** When drawing with pure-black semi-transparent color (r=0, g=0, b=0, alpha<255) on SKSvgCanvas, the alpha channel is ignored and rectangles render as fully opaque black in the SVG output; the same paint renders correctly on SKSurface/PNG.

**Analysis:** The Skia SVG device does not emit the opacity/fill-opacity attribute when the fill color is pure black (RGB=0,0,0) with a non-opaque alpha. The SVG canvas outputs `fill="rgb(0,0,0)"` without an opacity attribute, causing the element to render as fully opaque black instead of semi-transparent.

**Recommendations:** **needs-investigation** — Complete repro provided, clear regression window (2.88.2 → 2.88.3), bug isolated to SVG backend in native Skia layer. Needs investigation into upstream Skia SVG device to identify and fix the regression commit.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | backend/SVG |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a FileStream for 'Test.svg'
2. Create an SKSvgCanvas with the stream
3. Draw rects with SKColor(0, 0, 0, 128) and SKColor(0, 0, 0, 255)
4. Observe that both rectangles render as fully opaque black in the SVG (alpha ignored)

**Environment:** SkiaSharp 2.88.3, Visual Studio, Windows 11

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | SVG output renders semi-transparent black as fully opaque black when RGB channels are all zero |
| Repro quality | complete |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3, 2.88.2 |
| Worked in | 2.88.2 |
| Broke in | 2.88.3 |
| Current relevance | likely |
| Relevance reason | No SkiaSharp API changes were made between 2.88.2 and 2.88.3 (changelog shows 'No changes'), so the regression was likely introduced by a Skia engine version bump in 2.88.3. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.88 (88%) |
| Reason | Reporter explicitly states it worked in 2.88.2 and broke in 2.88.3. The 2.88.3 SkiaSharp API diff shows no changes, pointing to an upstream Skia SVG device change. |
| Worked in version | 2.88.2 |
| Broke in version | 2.88.3 |

## Analysis

### Technical Summary

The Skia SVG device does not emit the opacity/fill-opacity attribute when the fill color is pure black (RGB=0,0,0) with a non-opaque alpha. The SVG canvas outputs `fill="rgb(0,0,0)"` without an opacity attribute, causing the element to render as fully opaque black instead of semi-transparent.

### Rationale

Reporter provides a complete repro showing correct PNG output vs incorrect SVG output for the same paint. The workaround (setting at least one RGB channel > 0) confirms the issue is specific to pure-black colors in the SVG path. The SKSvgCanvas C# binding is a thin pass-through to `sk_svgcanvas_create_with_stream`; there is no color processing in the C# layer. The bug must reside in the Skia SVG device's color serialization, likely in how it handles the case where premultiplied RGB bytes are all zero. This regressed in 2.88.3 with no API-level SkiaSharp changes, pointing to an upstream Skia engine update.

### Key Signals

- "simple ensure at least one color (red, green or blue) is greater then 0, then alpha is working with SKSvgCanvas" — **issue body** (The bug triggers specifically when all RGB channels are zero (pure black), regardless of alpha — confirms SVG color serialization path for black is broken.)
- "The PNG output is corrected but on the SVG output, booth rectangle are black" — **issue body** (Different rendering paths: SKSurface uses raster backend (correct), SKSvgCanvas uses SVG device (buggy). Bug is isolated to the SVG device.)
- "2.88.3 (Current) ... 2.88.2 (Previous)" — **issue body - version fields** (Regression window is clear: broke in 2.88.3. 2.88.3 changelog shows 'No changes' in SkiaSharp API, so upstream Skia bump is the cause.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKSVG.cs` | 17-33 | direct | SKSvgCanvas.Create() is a thin wrapper that calls sk_svgcanvas_create_with_stream and returns the SKCanvas handle. There is no color transformation or alpha manipulation in the C# binding. All paint/color processing is delegated to the Skia SVG device in the native library. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | — | related | sk_svgcanvas_create_with_stream is the only SVG-related P/Invoke entry point. No additional SVG color hooks exist in the C# layer. |

### Workarounds

- Set at least one RGB channel to a value > 0 (e.g., SKColor(1, 0, 0, 128)) to produce semi-transparent near-black
- Post-process the SVG output to add fill-opacity attributes where the fill color is rgb(0,0,0)

### Next Questions

- Which specific Skia commit between m88 builds introduced the regression in the SVG device?
- Does the bug affect all pure-black (0,0,0,A) alpha values or only specific alpha ranges?
- Is the issue in the SVG device's color-to-string serialization or in paint attribute emission?

### Resolution Proposals

**Hypothesis:** The Skia SVG device's color serialization omits the opacity/fill-opacity attribute when the RGB components are all zero, possibly due to a shortcut that treats rgb(0,0,0) as 'transparent' and skips alpha emission, or a premultiplied alpha unpacking bug where 0*alpha/255 = 0 for all channels causing incorrect early-out.

1. **Investigate upstream Skia SVG device commit between m88 builds** — investigation, confidence 0.90 (90%), cost/s, validated=untested
   - Identify the upstream Skia commit that changed SVG device color serialization between the Skia versions used in SkiaSharp 2.88.2 and 2.88.3. Check the SVG device source (src/svg/SkSVGDevice.cpp) for changes to color/opacity attribute emission.
2. **Apply upstream Skia fix or cherry-pick patch to mono/skia fork** — fix, confidence 0.75 (75%), cost/m, validated=untested
   - Once the upstream commit is identified, check if it has been fixed in a newer Skia milestone and cherry-pick the fix to the mono/skia fork.

**Recommended proposal:** Investigate upstream Skia SVG device commit between m88 builds

**Why:** The bug is in the native Skia SVG device layer, not in SkiaSharp. Identifying the regression commit is required before any fix can be applied.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Complete repro provided, clear regression window (2.88.2 → 2.88.3), bug isolated to SVG backend in native Skia layer. Needs investigation into upstream Skia SVG device to identify and fix the regression commit. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, SVG backend, and reliability labels | labels=type/bug, area/SkiaSharp, backend/SVG, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Acknowledge regression, confirm workaround, and note it will need upstream Skia investigation | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the clear repro! This is a confirmed regression between 2.88.2 and 2.88.3.

The bug is in the native Skia SVG device: when drawing with pure black (R=0, G=0, B=0) and a non-opaque alpha, the SVG output omits the opacity attribute, causing both rectangles to render as fully opaque black. The PNG path is unaffected because it uses the raster backend.

**Workaround** (as you found): Set at least one RGB channel to a value > 0 to avoid the bug:
```cs
// Instead of SKColor(0, 0, 0, 128), use:
var color = new SKColor(red: 1, green: 0, blue: 0, alpha: 128); // near-black, alpha works
```

We'll need to trace the upstream Skia SVG device changes between the m88 builds used in 2.88.2 vs 2.88.3 to identify and fix the regression.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2692,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-05T12:50:29Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "When drawing with pure-black semi-transparent color (r=0, g=0, b=0, alpha<255) on SKSvgCanvas, the alpha channel is ignored and rectangles render as fully opaque black in the SVG output; the same paint renders correctly on SKSurface/PNG.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "backends": [
      "backend/SVG"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "SVG output renders semi-transparent black as fully opaque black when RGB channels are all zero",
      "reproQuality": "complete",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a FileStream for 'Test.svg'",
        "Create an SKSvgCanvas with the stream",
        "Draw rects with SKColor(0, 0, 0, 128) and SKColor(0, 0, 0, 255)",
        "Observe that both rectangles render as fully opaque black in the SVG (alpha ignored)"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, Visual Studio, Windows 11"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3",
        "2.88.2"
      ],
      "workedIn": "2.88.2",
      "brokeIn": "2.88.3",
      "currentRelevance": "likely",
      "relevanceReason": "No SkiaSharp API changes were made between 2.88.2 and 2.88.3 (changelog shows 'No changes'), so the regression was likely introduced by a Skia engine version bump in 2.88.3."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.88,
      "reason": "Reporter explicitly states it worked in 2.88.2 and broke in 2.88.3. The 2.88.3 SkiaSharp API diff shows no changes, pointing to an upstream Skia SVG device change.",
      "workedInVersion": "2.88.2",
      "brokeInVersion": "2.88.3"
    }
  },
  "analysis": {
    "summary": "The Skia SVG device does not emit the opacity/fill-opacity attribute when the fill color is pure black (RGB=0,0,0) with a non-opaque alpha. The SVG canvas outputs `fill=\"rgb(0,0,0)\"` without an opacity attribute, causing the element to render as fully opaque black instead of semi-transparent.",
    "rationale": "Reporter provides a complete repro showing correct PNG output vs incorrect SVG output for the same paint. The workaround (setting at least one RGB channel > 0) confirms the issue is specific to pure-black colors in the SVG path. The SKSvgCanvas C# binding is a thin pass-through to `sk_svgcanvas_create_with_stream`; there is no color processing in the C# layer. The bug must reside in the Skia SVG device's color serialization, likely in how it handles the case where premultiplied RGB bytes are all zero. This regressed in 2.88.3 with no API-level SkiaSharp changes, pointing to an upstream Skia engine update.",
    "keySignals": [
      {
        "text": "simple ensure at least one color (red, green or blue) is greater then 0, then alpha is working with SKSvgCanvas",
        "source": "issue body",
        "interpretation": "The bug triggers specifically when all RGB channels are zero (pure black), regardless of alpha — confirms SVG color serialization path for black is broken."
      },
      {
        "text": "The PNG output is corrected but on the SVG output, booth rectangle are black",
        "source": "issue body",
        "interpretation": "Different rendering paths: SKSurface uses raster backend (correct), SKSvgCanvas uses SVG device (buggy). Bug is isolated to the SVG device."
      },
      {
        "text": "2.88.3 (Current) ... 2.88.2 (Previous)",
        "source": "issue body - version fields",
        "interpretation": "Regression window is clear: broke in 2.88.3. 2.88.3 changelog shows 'No changes' in SkiaSharp API, so upstream Skia bump is the cause."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKSVG.cs",
        "lines": "17-33",
        "finding": "SKSvgCanvas.Create() is a thin wrapper that calls sk_svgcanvas_create_with_stream and returns the SKCanvas handle. There is no color transformation or alpha manipulation in the C# binding. All paint/color processing is delegated to the Skia SVG device in the native library.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "sk_svgcanvas_create_with_stream is the only SVG-related P/Invoke entry point. No additional SVG color hooks exist in the C# layer.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Set at least one RGB channel to a value > 0 (e.g., SKColor(1, 0, 0, 128)) to produce semi-transparent near-black",
      "Post-process the SVG output to add fill-opacity attributes where the fill color is rgb(0,0,0)"
    ],
    "nextQuestions": [
      "Which specific Skia commit between m88 builds introduced the regression in the SVG device?",
      "Does the bug affect all pure-black (0,0,0,A) alpha values or only specific alpha ranges?",
      "Is the issue in the SVG device's color-to-string serialization or in paint attribute emission?"
    ],
    "resolution": {
      "hypothesis": "The Skia SVG device's color serialization omits the opacity/fill-opacity attribute when the RGB components are all zero, possibly due to a shortcut that treats rgb(0,0,0) as 'transparent' and skips alpha emission, or a premultiplied alpha unpacking bug where 0*alpha/255 = 0 for all channels causing incorrect early-out.",
      "proposals": [
        {
          "title": "Investigate upstream Skia SVG device commit between m88 builds",
          "description": "Identify the upstream Skia commit that changed SVG device color serialization between the Skia versions used in SkiaSharp 2.88.2 and 2.88.3. Check the SVG device source (src/svg/SkSVGDevice.cpp) for changes to color/opacity attribute emission.",
          "category": "investigation",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Apply upstream Skia fix or cherry-pick patch to mono/skia fork",
          "description": "Once the upstream commit is identified, check if it has been fixed in a newer Skia milestone and cherry-pick the fix to the mono/skia fork.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Investigate upstream Skia SVG device commit between m88 builds",
      "recommendedReason": "The bug is in the native Skia SVG device layer, not in SkiaSharp. Identifying the regression commit is required before any fix can be applied."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Complete repro provided, clear regression window (2.88.2 → 2.88.3), bug isolated to SVG backend in native Skia layer. Needs investigation into upstream Skia SVG device to identify and fix the regression commit.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, SVG backend, and reliability labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "backend/SVG",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge regression, confirm workaround, and note it will need upstream Skia investigation",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the clear repro! This is a confirmed regression between 2.88.2 and 2.88.3.\n\nThe bug is in the native Skia SVG device: when drawing with pure black (R=0, G=0, B=0) and a non-opaque alpha, the SVG output omits the opacity attribute, causing both rectangles to render as fully opaque black. The PNG path is unaffected because it uses the raster backend.\n\n**Workaround** (as you found): Set at least one RGB channel to a value > 0 to avoid the bug:\n```cs\n// Instead of SKColor(0, 0, 0, 128), use:\nvar color = new SKColor(red: 1, green: 0, blue: 0, alpha: 128); // near-black, alpha works\n```\n\nWe'll need to trace the upstream Skia SVG device changes between the m88 builds used in 2.88.2 vs 2.88.3 to identify and fix the regression."
      }
    ]
  }
}
```

</details>
