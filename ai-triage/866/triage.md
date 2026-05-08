# Issue Triage Report — #866

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-27T04:06:23Z |
| Type | type/bug (0.80 (80%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-investigation (0.75 (75%)) |

**Issue Summary:** SKMatrixConvolutionTileMode.Clamp does not eliminate border artifacts in matrix convolution and blur image filters; Clamp and ClampToBlack modes appear to produce identical results on Android and iOS with SkiaSharp 1.68.0.

**Analysis:** Border artifacts appear when applying matrix convolution or blur filters; SKMatrixConvolutionTileMode.Clamp and ClampToBlack behave identically, suggesting the tileMode parameter had no effect in the Skia version packaged with SkiaSharp 1.68.0. The reporter also notes that applying filters in OnPaintSurface has no effect at all. This is likely a Skia upstream behavior issue at image boundaries when drawing onto a transparent canvas.

**Recommendations:** **needs-investigation** — The issue has valid code and screenshots demonstrating wrong output, but was filed against SkiaSharp 1.68.0 (2019). The API has since changed from SKMatrixConvolutionTileMode to SKShaderTileMode. The issue needs reproduction with the current version before determining if a fix is needed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Android, os/iOS |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create a Xamarin.Forms app targeting Android and iOS
2. Load an image as SKImage
3. Create a 5x5 sharpening kernel and call SKImageFilter.CreateMatrixConvolution with SKMatrixConvolutionTileMode.Clamp
4. Draw the image onto a new SKBitmap canvas using the filter paint
5. Observe border artifacts around the filtered image

**Environment:** SkiaSharp 1.68.0, Xamarin.Forms, Android + iOS, 2019

**Screenshots:**
- https://user-images.githubusercontent.com/1614598/59851013-366b4580-9374-11e9-9778-95c38fe67023.png — Original unfiltered image
- https://user-images.githubusercontent.com/1614598/59851028-4420cb00-9374-11e9-803c-53d6379b0dd0.png — Blurred image with visible border artifacts
- https://user-images.githubusercontent.com/1614598/59851047-4be06f80-9374-11e9-84c6-b7d169828970.png — Sharpened image with visible border artifacts

**Code snippets:**

```csharp
SKImageFilter.CreateMatrixConvolution(size, kernel, gain, bias, offset, SKMatrixConvolutionTileMode.Clamp, true)
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Border artifacts visible on filtered images; Clamp and ClampToBlack produce same result |
| Repro quality | partial |
| Target frameworks | Xamarin.Forms |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | The API changed from SKMatrixConvolutionTileMode to SKShaderTileMode in later versions. The underlying Skia behavior at image boundaries may also have changed. Testing with current SkiaSharp 3.x is needed to confirm whether the issue persists. |

## Analysis

### Technical Summary

Border artifacts appear when applying matrix convolution or blur filters; SKMatrixConvolutionTileMode.Clamp and ClampToBlack behave identically, suggesting the tileMode parameter had no effect in the Skia version packaged with SkiaSharp 1.68.0. The reporter also notes that applying filters in OnPaintSurface has no effect at all. This is likely a Skia upstream behavior issue at image boundaries when drawing onto a transparent canvas.

### Rationale

Reporter shows wrong visual output (visible border artifacts) in screenshots and provides sample code. The claim that Clamp and ClampToBlack produce identical results indicates a potential Skia upstream bug or that the tile mode was not respected by the filter in this version. The API has since been updated to use SKShaderTileMode, which may or may not have resolved the behavior. Classified as type/bug with medium severity because a workaround (crop the result) exists.

### Key Signals

- "I cannot see any difference between SKMatrixConvolutionTileMode.Clamp and SKMatrixConvolutionTileMode.ClampToBlack" — **issue body** (The tileMode parameter appears to be ignored or incorrectly applied — two semantically different modes produce identical output.)
- "if I try to draw the image directly to the SKCanvasView or SKGLView and apply the filter on the fly, then nothing happens - the image appears unfiltered" — **comment #504025994** (A secondary bug: SKImageFilter applied to SKPaint when drawing directly to a surface canvas is silently ignored, possibly a GPU backend limitation.)
- "Being able to create a blur that clamps edge pixels is also something I've needed." — **comment #550287142** (Another user confirms the same need, suggesting the issue affects more than one reporter.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKImageFilter.cs` | 215-233 | direct | CreateMatrixConvolution in the current codebase uses SKShaderTileMode (not SKMatrixConvolutionTileMode). The C API call is sk_imagefilter_new_matrix_convolution passing tileMode as SKShaderTileMode. The old SKMatrixConvolutionTileMode enum (Clamp=0, Repeat=1, ClampToBlack=2) values map differently to SKShaderTileMode (Clamp=0, Repeat=1, Mirror=2, Decal=3), making the old ClampToBlack value numerically equivalent to Mirror in the new enum. |
| `binding/SkiaSharp/SKImageFilter.cs` | 37-58 | related | CreateBlur currently accepts SKShaderTileMode with full overloads (including tileMode variants). The two-argument CreateBlur(sigmaX, sigmaY) defaults to SKShaderTileMode.Decal. This addresses the reporter's secondary concern about blur not accepting a tile mode, since the current API does expose this. |
| `binding/libSkiaSharp.json` | — | context | SKMatrixConvolutionTileMode enum is still present in the binding JSON, mapped to C type sk_matrix_convolution_tilemode_t. However, CreateMatrixConvolution no longer uses this type — it was replaced with SKShaderTileMode in the C# API. |

### Next Questions

- Does the issue reproduce with current SkiaSharp 3.x using SKShaderTileMode.Clamp?
- Is the secondary bug (filter ignored on SKGLView/SKCanvasView) reproducible on current SkiaSharp?
- Was there a Skia upstream fix for the matrix convolution tileMode behavior?

### Resolution Proposals

**Hypothesis:** In SkiaSharp 1.68.0, the SKMatrixConvolutionTileMode.Clamp parameter was either not respected by the underlying Skia version, or the filter was operating on the full canvas bounds (including transparent areas) rather than just the drawn image bounds, causing edge clamping to clamp to transparent pixels. The current API migration to SKShaderTileMode may or may not have resolved the underlying Skia behavior.

1. **Reproduce with current SkiaSharp version** — investigation, confidence 0.85 (85%), cost/s, validated=untested
   - Test whether SKImageFilter.CreateMatrixConvolution with SKShaderTileMode.Clamp still produces border artifacts in SkiaSharp 3.x. If fixed, the issue can be closed as fixed.
2. **Use cropRect to constrain the filter** — workaround, confidence 0.65 (65%), cost/xs, validated=untested
   - Pass an explicit cropRect matching the image bounds to SKImageFilter.CreateMatrixConvolution. This tells the filter to operate only within those bounds, preventing it from seeing transparent canvas pixels at the edges.

**Recommended proposal:** Reproduce with current SkiaSharp version

**Why:** The API has changed significantly since 1.68.0 and the issue may be resolved. Confirming with the latest version is the most important first step before any fix work.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.75 (75%) |
| Reason | The issue has valid code and screenshots demonstrating wrong output, but was filed against SkiaSharp 1.68.0 (2019). The API has since changed from SKMatrixConvolutionTileMode to SKShaderTileMode. The issue needs reproduction with the current version before determining if a fix is needed. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, SkiaSharp core, Android, iOS labels | labels=type/bug, area/SkiaSharp, os/Android, os/iOS, tenet/reliability |
| add-comment | medium | 0.75 (75%) | Ask reporter to test with current SkiaSharp version and explain tileMode API change | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report with screenshots! Since this was filed against SkiaSharp 1.68.0 (2019), a few things have changed:

1. The API has been updated — `SKMatrixConvolutionTileMode` has been replaced by `SKShaderTileMode` in `CreateMatrixConvolution`. Use `SKShaderTileMode.Clamp` with current SkiaSharp.

2. `CreateBlur` now also has overloads accepting `SKShaderTileMode`, so you can control edge behavior for blurs too.

Could you test with the latest SkiaSharp (3.x) and report whether the border artifacts still appear? 

If the issue persists, a workaround to try is passing an explicit `cropRect` matching your image bounds to `CreateMatrixConvolution` — this constrains the filter to operate within those bounds and may prevent it from sampling transparent pixels at the canvas edges.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 866,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-27T04:06:23Z"
  },
  "summary": "SKMatrixConvolutionTileMode.Clamp does not eliminate border artifacts in matrix convolution and blur image filters; Clamp and ClampToBlack modes appear to produce identical results on Android and iOS with SkiaSharp 1.68.0.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.8
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Android",
      "os/iOS"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "Border artifacts visible on filtered images; Clamp and ClampToBlack produce same result",
      "reproQuality": "partial",
      "targetFrameworks": [
        "Xamarin.Forms"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Xamarin.Forms app targeting Android and iOS",
        "Load an image as SKImage",
        "Create a 5x5 sharpening kernel and call SKImageFilter.CreateMatrixConvolution with SKMatrixConvolutionTileMode.Clamp",
        "Draw the image onto a new SKBitmap canvas using the filter paint",
        "Observe border artifacts around the filtered image"
      ],
      "codeSnippets": [
        "SKImageFilter.CreateMatrixConvolution(size, kernel, gain, bias, offset, SKMatrixConvolutionTileMode.Clamp, true)"
      ],
      "environmentDetails": "SkiaSharp 1.68.0, Xamarin.Forms, Android + iOS, 2019",
      "screenshots": [
        {
          "url": "https://user-images.githubusercontent.com/1614598/59851013-366b4580-9374-11e9-9778-95c38fe67023.png",
          "description": "Original unfiltered image"
        },
        {
          "url": "https://user-images.githubusercontent.com/1614598/59851028-4420cb00-9374-11e9-803c-53d6379b0dd0.png",
          "description": "Blurred image with visible border artifacts"
        },
        {
          "url": "https://user-images.githubusercontent.com/1614598/59851047-4be06f80-9374-11e9-84c6-b7d169828970.png",
          "description": "Sharpened image with visible border artifacts"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.0"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "The API changed from SKMatrixConvolutionTileMode to SKShaderTileMode in later versions. The underlying Skia behavior at image boundaries may also have changed. Testing with current SkiaSharp 3.x is needed to confirm whether the issue persists."
    }
  },
  "analysis": {
    "summary": "Border artifacts appear when applying matrix convolution or blur filters; SKMatrixConvolutionTileMode.Clamp and ClampToBlack behave identically, suggesting the tileMode parameter had no effect in the Skia version packaged with SkiaSharp 1.68.0. The reporter also notes that applying filters in OnPaintSurface has no effect at all. This is likely a Skia upstream behavior issue at image boundaries when drawing onto a transparent canvas.",
    "rationale": "Reporter shows wrong visual output (visible border artifacts) in screenshots and provides sample code. The claim that Clamp and ClampToBlack produce identical results indicates a potential Skia upstream bug or that the tile mode was not respected by the filter in this version. The API has since been updated to use SKShaderTileMode, which may or may not have resolved the behavior. Classified as type/bug with medium severity because a workaround (crop the result) exists.",
    "keySignals": [
      {
        "text": "I cannot see any difference between SKMatrixConvolutionTileMode.Clamp and SKMatrixConvolutionTileMode.ClampToBlack",
        "source": "issue body",
        "interpretation": "The tileMode parameter appears to be ignored or incorrectly applied — two semantically different modes produce identical output."
      },
      {
        "text": "if I try to draw the image directly to the SKCanvasView or SKGLView and apply the filter on the fly, then nothing happens - the image appears unfiltered",
        "source": "comment #504025994",
        "interpretation": "A secondary bug: SKImageFilter applied to SKPaint when drawing directly to a surface canvas is silently ignored, possibly a GPU backend limitation."
      },
      {
        "text": "Being able to create a blur that clamps edge pixels is also something I've needed.",
        "source": "comment #550287142",
        "interpretation": "Another user confirms the same need, suggesting the issue affects more than one reporter."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKImageFilter.cs",
        "lines": "215-233",
        "finding": "CreateMatrixConvolution in the current codebase uses SKShaderTileMode (not SKMatrixConvolutionTileMode). The C API call is sk_imagefilter_new_matrix_convolution passing tileMode as SKShaderTileMode. The old SKMatrixConvolutionTileMode enum (Clamp=0, Repeat=1, ClampToBlack=2) values map differently to SKShaderTileMode (Clamp=0, Repeat=1, Mirror=2, Decal=3), making the old ClampToBlack value numerically equivalent to Mirror in the new enum.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImageFilter.cs",
        "lines": "37-58",
        "finding": "CreateBlur currently accepts SKShaderTileMode with full overloads (including tileMode variants). The two-argument CreateBlur(sigmaX, sigmaY) defaults to SKShaderTileMode.Decal. This addresses the reporter's secondary concern about blur not accepting a tile mode, since the current API does expose this.",
        "relevance": "related"
      },
      {
        "file": "binding/libSkiaSharp.json",
        "finding": "SKMatrixConvolutionTileMode enum is still present in the binding JSON, mapped to C type sk_matrix_convolution_tilemode_t. However, CreateMatrixConvolution no longer uses this type — it was replaced with SKShaderTileMode in the C# API.",
        "relevance": "context"
      }
    ],
    "nextQuestions": [
      "Does the issue reproduce with current SkiaSharp 3.x using SKShaderTileMode.Clamp?",
      "Is the secondary bug (filter ignored on SKGLView/SKCanvasView) reproducible on current SkiaSharp?",
      "Was there a Skia upstream fix for the matrix convolution tileMode behavior?"
    ],
    "resolution": {
      "hypothesis": "In SkiaSharp 1.68.0, the SKMatrixConvolutionTileMode.Clamp parameter was either not respected by the underlying Skia version, or the filter was operating on the full canvas bounds (including transparent areas) rather than just the drawn image bounds, causing edge clamping to clamp to transparent pixels. The current API migration to SKShaderTileMode may or may not have resolved the underlying Skia behavior.",
      "proposals": [
        {
          "title": "Reproduce with current SkiaSharp version",
          "description": "Test whether SKImageFilter.CreateMatrixConvolution with SKShaderTileMode.Clamp still produces border artifacts in SkiaSharp 3.x. If fixed, the issue can be closed as fixed.",
          "category": "investigation",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Use cropRect to constrain the filter",
          "description": "Pass an explicit cropRect matching the image bounds to SKImageFilter.CreateMatrixConvolution. This tells the filter to operate only within those bounds, preventing it from seeing transparent canvas pixels at the edges.",
          "category": "workaround",
          "confidence": 0.65,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Reproduce with current SkiaSharp version",
      "recommendedReason": "The API has changed significantly since 1.68.0 and the issue may be resolved. Confirming with the latest version is the most important first step before any fix work."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.75,
      "reason": "The issue has valid code and screenshots demonstrating wrong output, but was filed against SkiaSharp 1.68.0 (2019). The API has since changed from SKMatrixConvolutionTileMode to SKShaderTileMode. The issue needs reproduction with the current version before determining if a fix is needed.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, SkiaSharp core, Android, iOS labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Android",
          "os/iOS",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask reporter to test with current SkiaSharp version and explain tileMode API change",
        "risk": "medium",
        "confidence": 0.75,
        "comment": "Thanks for the detailed report with screenshots! Since this was filed against SkiaSharp 1.68.0 (2019), a few things have changed:\n\n1. The API has been updated — `SKMatrixConvolutionTileMode` has been replaced by `SKShaderTileMode` in `CreateMatrixConvolution`. Use `SKShaderTileMode.Clamp` with current SkiaSharp.\n\n2. `CreateBlur` now also has overloads accepting `SKShaderTileMode`, so you can control edge behavior for blurs too.\n\nCould you test with the latest SkiaSharp (3.x) and report whether the border artifacts still appear? \n\nIf the issue persists, a workaround to try is passing an explicit `cropRect` matching your image bounds to `CreateMatrixConvolution` — this constrains the filter to operate within those bounds and may prevent it from sampling transparent pixels at the canvas edges."
      }
    ]
  }
}
```

</details>
