# Issue Triage Report — #1938

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T21:45:00Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp.Views.Uno (0.90 (90%)) |
| Suggested action | needs-info (0.80 (80%)) |

**Issue Summary:** SKXamlCanvas on Android renders with wrong proportions/smaller image when used alongside AcrylicBrush in an Uno Platform app; works correctly on UWP.

**Analysis:** The Android SKXamlCanvas draws its Skia bitmap using canvas.Width/canvas.Height as the destination rect in DrawSurface. When AcrylicBrush is active, the Android Canvas passed to OnDraw may have different clip bounds or coordinate transforms applied, causing a mismatch between the bitmap's physical-pixel size (ActualWidth * Dpi) and the destination rect, producing the wrong-proportion output reported.

**Recommendations:** **needs-info** — Bug is plausible with a repro repo, but filed against SkiaSharp 2.80.x (very old). Need confirmation it still reproduces on current versions before investigating or fixing.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Uno |
| Platforms | os/Android, os/Windows-Universal-UWP |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | partner/unoplatform |
| Current labels | partner/unoplatform |

## Evidence

### Reproduction

1. Create an Uno Platform app using SKXamlCanvas in a layout that also contains an AcrylicBrush
2. Run on Android (tested: Huawei P30 Pro, Android 11)
3. Observe that the SKXamlCanvas content appears smaller or with wrong proportions compared to UWP

**Environment:** SkiaSharp 2.80.2, SkiaSharp.Views.Uno 2.80.3, VS 2022, Android 11.0, Huawei P30 Pro

**Repository links:**
- https://github.com/TopperDEL/UnoAcrylicBrushSkiaError — Minimal Uno repro project from reporter
- https://github.com/unoplatform/uno/issues/7255 — Original cross-posted issue on the Uno Platform repo

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Image appears smaller/with wrong proportions below AcrylicBrush on Android |
| Repro quality | partial |
| Target frameworks | monoandroid11.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2, 2.80.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | Issue was filed with 2.80.x; the Android SKXamlCanvas DrawSurface logic has been unchanged since then but DPI/canvas-size interaction with AcrylicBrush layers has not been re-examined. |

## Analysis

### Technical Summary

The Android SKXamlCanvas draws its Skia bitmap using canvas.Width/canvas.Height as the destination rect in DrawSurface. When AcrylicBrush is active, the Android Canvas passed to OnDraw may have different clip bounds or coordinate transforms applied, causing a mismatch between the bitmap's physical-pixel size (ActualWidth * Dpi) and the destination rect, producing the wrong-proportion output reported.

### Rationale

The issue reports visual wrong-output (smaller/misaligned image) on Android only; UWP works. The Uno Android SKXamlCanvas.OnDraw calls surfaceFactory.DrawSurface which uses canvas.Width and canvas.Height as the destination bounds — these may differ from the bitmap dimensions when the platform canvas is clipped or scaled by a parent AcrylicBrush layer. The reporter self-describes it as a duplicate of an Uno issue and provides a repro repo. Classification is type/bug because the rendering result is incorrect and differs across platform implementations of the same API.

### Key Signals

- "image seems to be smaller/with wrong proportions below the AcrylicBrush" — **issue body** (Wrong-output bug — destination rect in DrawBitmap does not match expected content size.)
- "it works on UWP but brings weird results on Android" — **issue body** (Platform-specific: the Android Uno Canvas layer passed to OnDraw may have different clip/transform state when AcrylicBrush is compositing.)
- "var dst = new RectF(0, 0, canvas.Width, canvas.Height);" — **SurfaceFactory.cs line 64** (Destination rect is taken from the Android Canvas bounds, not from ActualWidth/ActualHeight; AcrylicBrush may alter those bounds.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SurfaceFactory.cs` | 53-68 | direct | DrawSurface uses canvas.Width and canvas.Height as the destination RectF for DrawBitmap. If the Android Canvas has a clipping region or transform set by an outer AcrylicBrush layer, these values may not match the bitmap's physical pixel dimensions, producing the observed size/proportion mismatch. |
| `source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI/SKXamlCanvas.Android.cs` | 29-32 | direct | DoInvalidate sizes the bitmap with ActualWidth * Dpi and ActualHeight * Dpi, but DrawSurface uses the live Android Canvas width/height — these can diverge when AcrylicBrush applies a layer transform. |

### Next Questions

- Does the same issue reproduce on other Android devices or only Huawei P30 Pro?
- Does removing AcrylicBrush from the layout fix the rendering?
- Is the issue still present with SkiaSharp 2.88.x or 3.x?

### Resolution Proposals

**Hypothesis:** The Android Canvas passed to OnDraw has its width/height clipped by the AcrylicBrush compositing layer; using Info.Width/Info.Height (bitmap's actual size) as the destination rect, or using ActualWidth*Dpi and ActualHeight*Dpi directly, would match the bitmap to the view's true extent.

1. **Use bitmap dimensions as destination rect in DrawSurface** — fix, confidence 0.65 (65%), cost/s, validated=untested
   - Change DrawSurface to use Info.Width and Info.Height (or ActualWidth*Dpi/ActualHeight*Dpi from the parent view) instead of canvas.Width and canvas.Height to ensure the bitmap fills the expected surface regardless of what the Android Canvas bounds say.
2. **Reproduce and compare with current SkiaSharp version** — investigation, confidence 0.85 (85%), cost/xs, validated=untested
   - Ask reporter to test with latest SkiaSharp (3.x) and latest Uno, to determine if the issue is already fixed upstream before investing in a fix.

**Recommended proposal:** Reproduce and compare with current SkiaSharp version

**Why:** The issue was filed against 2.80.x in January 2022. Significant Uno/SkiaSharp changes have landed since then. Confirming reproducibility on current versions before fixing avoids unnecessary work.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.80 (80%) |
| Reason | Bug is plausible with a repro repo, but filed against SkiaSharp 2.80.x (very old). Need confirmation it still reproduces on current versions before investigating or fixing. |
| Suggested repro platform | linux |

### Missing Info

- Does the issue still reproduce with the latest SkiaSharp (3.x) and Uno Platform?
- Does removing AcrylicBrush from the layout fix the rendering on Android?

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, Uno views, Android labels | labels=type/bug, area/SkiaSharp.Views.Uno, os/Android, tenet/compatibility, partner/unoplatform |
| add-comment | medium | 0.82 (82%) | Ask reporter to verify with current SkiaSharp and Uno versions | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and the repro project!

This issue was filed against SkiaSharp 2.80.x and Uno 2.80.x, which are quite old. Before we investigate further, could you please check:

1. Does the issue still reproduce with the latest SkiaSharp (3.x) and the latest Uno Platform?
2. If you remove the `AcrylicBrush` from the layout, does `SKXamlCanvas` render correctly on Android?

This will help us determine whether the issue has already been fixed upstream or whether it still needs investigation.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1938,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T21:45:00Z",
    "currentLabels": [
      "partner/unoplatform"
    ]
  },
  "summary": "SKXamlCanvas on Android renders with wrong proportions/smaller image when used alongside AcrylicBrush in an Uno Platform app; works correctly on UWP.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp.Views.Uno",
      "confidence": 0.9
    },
    "platforms": [
      "os/Android",
      "os/Windows-Universal-UWP"
    ],
    "tenets": [
      "tenet/compatibility"
    ],
    "partner": "partner/unoplatform"
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "Image appears smaller/with wrong proportions below AcrylicBrush on Android",
      "reproQuality": "partial",
      "targetFrameworks": [
        "monoandroid11.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an Uno Platform app using SKXamlCanvas in a layout that also contains an AcrylicBrush",
        "Run on Android (tested: Huawei P30 Pro, Android 11)",
        "Observe that the SKXamlCanvas content appears smaller or with wrong proportions compared to UWP"
      ],
      "environmentDetails": "SkiaSharp 2.80.2, SkiaSharp.Views.Uno 2.80.3, VS 2022, Android 11.0, Huawei P30 Pro",
      "repoLinks": [
        {
          "url": "https://github.com/TopperDEL/UnoAcrylicBrushSkiaError",
          "description": "Minimal Uno repro project from reporter"
        },
        {
          "url": "https://github.com/unoplatform/uno/issues/7255",
          "description": "Original cross-posted issue on the Uno Platform repo"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2",
        "2.80.3"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "Issue was filed with 2.80.x; the Android SKXamlCanvas DrawSurface logic has been unchanged since then but DPI/canvas-size interaction with AcrylicBrush layers has not been re-examined."
    }
  },
  "analysis": {
    "summary": "The Android SKXamlCanvas draws its Skia bitmap using canvas.Width/canvas.Height as the destination rect in DrawSurface. When AcrylicBrush is active, the Android Canvas passed to OnDraw may have different clip bounds or coordinate transforms applied, causing a mismatch between the bitmap's physical-pixel size (ActualWidth * Dpi) and the destination rect, producing the wrong-proportion output reported.",
    "rationale": "The issue reports visual wrong-output (smaller/misaligned image) on Android only; UWP works. The Uno Android SKXamlCanvas.OnDraw calls surfaceFactory.DrawSurface which uses canvas.Width and canvas.Height as the destination bounds — these may differ from the bitmap dimensions when the platform canvas is clipped or scaled by a parent AcrylicBrush layer. The reporter self-describes it as a duplicate of an Uno issue and provides a repro repo. Classification is type/bug because the rendering result is incorrect and differs across platform implementations of the same API.",
    "keySignals": [
      {
        "text": "image seems to be smaller/with wrong proportions below the AcrylicBrush",
        "source": "issue body",
        "interpretation": "Wrong-output bug — destination rect in DrawBitmap does not match expected content size."
      },
      {
        "text": "it works on UWP but brings weird results on Android",
        "source": "issue body",
        "interpretation": "Platform-specific: the Android Uno Canvas layer passed to OnDraw may have different clip/transform state when AcrylicBrush is compositing."
      },
      {
        "text": "var dst = new RectF(0, 0, canvas.Width, canvas.Height);",
        "source": "SurfaceFactory.cs line 64",
        "interpretation": "Destination rect is taken from the Android Canvas bounds, not from ActualWidth/ActualHeight; AcrylicBrush may alter those bounds."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SurfaceFactory.cs",
        "lines": "53-68",
        "finding": "DrawSurface uses canvas.Width and canvas.Height as the destination RectF for DrawBitmap. If the Android Canvas has a clipping region or transform set by an outer AcrylicBrush layer, these values may not match the bitmap's physical pixel dimensions, producing the observed size/proportion mismatch.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI/SKXamlCanvas.Android.cs",
        "lines": "29-32",
        "finding": "DoInvalidate sizes the bitmap with ActualWidth * Dpi and ActualHeight * Dpi, but DrawSurface uses the live Android Canvas width/height — these can diverge when AcrylicBrush applies a layer transform.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "Does the same issue reproduce on other Android devices or only Huawei P30 Pro?",
      "Does removing AcrylicBrush from the layout fix the rendering?",
      "Is the issue still present with SkiaSharp 2.88.x or 3.x?"
    ],
    "resolution": {
      "hypothesis": "The Android Canvas passed to OnDraw has its width/height clipped by the AcrylicBrush compositing layer; using Info.Width/Info.Height (bitmap's actual size) as the destination rect, or using ActualWidth*Dpi and ActualHeight*Dpi directly, would match the bitmap to the view's true extent.",
      "proposals": [
        {
          "title": "Use bitmap dimensions as destination rect in DrawSurface",
          "description": "Change DrawSurface to use Info.Width and Info.Height (or ActualWidth*Dpi/ActualHeight*Dpi from the parent view) instead of canvas.Width and canvas.Height to ensure the bitmap fills the expected surface regardless of what the Android Canvas bounds say.",
          "category": "fix",
          "confidence": 0.65,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Reproduce and compare with current SkiaSharp version",
          "description": "Ask reporter to test with latest SkiaSharp (3.x) and latest Uno, to determine if the issue is already fixed upstream before investing in a fix.",
          "category": "investigation",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Reproduce and compare with current SkiaSharp version",
      "recommendedReason": "The issue was filed against 2.80.x in January 2022. Significant Uno/SkiaSharp changes have landed since then. Confirming reproducibility on current versions before fixing avoids unnecessary work."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.8,
      "reason": "Bug is plausible with a repro repo, but filed against SkiaSharp 2.80.x (very old). Need confirmation it still reproduces on current versions before investigating or fixing.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Does the issue still reproduce with the latest SkiaSharp (3.x) and Uno Platform?",
      "Does removing AcrylicBrush from the layout fix the rendering on Android?"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, Uno views, Android labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Uno",
          "os/Android",
          "tenet/compatibility",
          "partner/unoplatform"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask reporter to verify with current SkiaSharp and Uno versions",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report and the repro project!\n\nThis issue was filed against SkiaSharp 2.80.x and Uno 2.80.x, which are quite old. Before we investigate further, could you please check:\n\n1. Does the issue still reproduce with the latest SkiaSharp (3.x) and the latest Uno Platform?\n2. If you remove the `AcrylicBrush` from the layout, does `SKXamlCanvas` render correctly on Android?\n\nThis will help us determine whether the issue has already been fixed upstream or whether it still needs investigation."
      }
    ]
  }
}
```

</details>
