# Issue Triage Report — #2210

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-29T17:18:00Z |
| Type | type/bug (0.85 (85%)) |
| Area | area/SkiaSharp.Views (0.82 (82%)) |
| Suggested action | needs-info (0.78 (78%)) |

**Issue Summary:** Regression on Android in SkiaSharp.Views.Forms: SVG/canvas scaling is incorrect in 2.88.0 vs 2.80.0, likely caused by canvas.ResetMatrix() losing the density transform that was set up by the Forms Android renderer.

**Analysis:** The reporter uses canvas.ResetMatrix() followed by canvas.LocalClipBounds to position elements after drawing an SVG. In 2.88.0, Android canvas density handling changed: the Forms Android renderer applies a density Scale() to the Skia canvas before invoking PaintSurface (when IgnorePixelScaling=true) or passes the raw physical-pixel surface. Calling ResetMatrix() strips this transform and sets the matrix to identity, so LocalClipBounds then returns physical-pixel dimensions rather than the expected logical/DIP dimensions. This causes positioning calculations that depend on LocalClipBounds to produce visually different results on high-density Android screens. The maintainer has already suggested using Save/Restore instead of ResetMatrix as a workaround.

**Recommendations:** **needs-info** — Repro is incomplete (garbled XAML, empty Expected/Actual sections). Reporter has not responded to the maintainer's Save/Restore workaround suggestion. Need confirmation of whether the workaround helps and a minimal repro before deeper investigation.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Android |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Use SkiaSharp.Views.Forms.SKCanvasView in a Xamarin.Forms Android app
2. In PaintSurface handler: call canvas.Translate, canvas.Scale, canvas.DrawPicture to render an SVG
3. Call canvas.ResetMatrix() then use canvas.LocalClipBounds for subsequent element positioning
4. Observe that with 2.88.0 the elements are scaled/positioned incorrectly on Android; 2.80.0 renders correctly

**Environment:** Samsung Galaxy S10/Z Flip 3 (Android 12), Xamarin.Forms 5.0.0.2515, Xamarin.Essentials 1.7.3, VS 2022

**Attachments:**
- 2.80.0.jpg — https://user-images.githubusercontent.com/62118624/183975619-6cbc5c3b-6783-464c-855c-f1bc3fa3a368.jpg — Screenshot showing correct rendering in SkiaSharp 2.80.0
- 2.88.0.jpg — https://user-images.githubusercontent.com/62118624/183975716-e21f89e5-87a1-4f6a-9e97-c0714b62c1b5.jpg — Screenshot showing incorrect/scaled rendering in SkiaSharp 2.88.0

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | Image scaling incorrect on Android after upgrading from 2.80.0 to 2.88.0; canvas.ResetMatrix() and canvas.Scale() appear to behave differently |
| Repro quality | partial |
| Target frameworks | Xamarin.Android |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.0, 2.88.0 |
| Worked in | 2.80.0 |
| Broke in | 2.88.0 |
| Current relevance | unknown |
| Relevance reason | Xamarin.Forms is EOL; MAUI equivalents use a different canvas setup path. The core canvas.ResetMatrix() behavior is unchanged, but the Forms Android renderer may set up the canvas transform differently in 2.88.0 due to the IgnorePixelScaling/density changes. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.80 (80%) |
| Reason | Reporter explicitly states worked in 2.80.0, broken in 2.88.0 on Android. iOS unaffected. Rolling back to 2.80.0 fixes the issue. |
| Worked in version | 2.80.0 |
| Broke in version | 2.88.0 |

## Analysis

### Technical Summary

The reporter uses canvas.ResetMatrix() followed by canvas.LocalClipBounds to position elements after drawing an SVG. In 2.88.0, Android canvas density handling changed: the Forms Android renderer applies a density Scale() to the Skia canvas before invoking PaintSurface (when IgnorePixelScaling=true) or passes the raw physical-pixel surface. Calling ResetMatrix() strips this transform and sets the matrix to identity, so LocalClipBounds then returns physical-pixel dimensions rather than the expected logical/DIP dimensions. This causes positioning calculations that depend on LocalClipBounds to produce visually different results on high-density Android screens. The maintainer has already suggested using Save/Restore instead of ResetMatrix as a workaround.

### Rationale

Classified as type/bug because the reporter observes a regression (2.80.0 worked, 2.88.0 broken) with screenshot evidence. The area is area/SkiaSharp.Views because the root cause is in the Forms Android canvas renderer's density transform setup, not in core SkiaSharp. The repro is partial — XAML code in the issue is garbled, expected/actual behavior fields are empty, and the reporter has not responded to the maintainer's workaround suggestion. A workaround (Save/Restore instead of ResetMatrix) already exists and was posted by the maintainer.

### Key Signals

- "I am going to copy in my code below...what I do know is that the SkiaSharp.Views.Forms.SKCanvasView behaves completely different in 2.88.0." — **issue body** (Confirms regression is in the Forms view specifically.)
- "I rolled back to 2.80.0 and it behaves correctly." — **issue body** (Classic regression — rollback confirms the 2.88.0 change is the cause.)
- "Android is the only one that seems to have the issue" — **issue body** (Platform-specific — iOS does not apply density scale to the canvas in the same way, so ResetMatrix has no visible effect there.)
- "What happens if you wrap the draw logic inside a save restore instead of resetting the matrix?" — **comment by mattleibow** (Maintainer confirms the workaround: use Save/Restore to preserve canvas state rather than ResetMatrix.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs` | 99-116 | direct | When IgnorePixelScaling=true, the renderer calls skiaCanvas.Scale(density) then skiaCanvas.Save() before invoking PaintSurface. Calling canvas.ResetMatrix() inside PaintSurface resets to identity, losing the density scale. LocalClipBounds after reset then returns physical-pixel bounds rather than logical DIP bounds. |
| `binding/SkiaSharp/SKCanvas.cs` | 890-893 | direct | ResetMatrix() delegates to sk_canvas_reset_matrix which sets the transform to identity — it does NOT restore the canvas save stack. So it bypasses any density pre-transform unconditionally. |
| `binding/SkiaSharp/SKCanvas.cs` | 276-281 | related | LocalClipBounds returns clip bounds in the current local coordinate space. After ResetMatrix() (identity), this reflects physical pixel dimensions rather than any scaled coordinate space. |
| `changelogs/SkiaSharp.Views.Forms/2.88.0/SkiaSharp.Views.Forms.md` | — | related | In 2.88.0 SKPaintSurfaceEventArgs gained RawInfo property — signals that the canvas coordinate system was refactored in 2.88.0 to separate logical (info) from physical (rawInfo) pixel coordinates, which aligns with the regression. |

### Workarounds

- Use canvas.Save() before and canvas.Restore() after the SVG drawing block instead of calling canvas.ResetMatrix(). This preserves the density transform applied by the Forms Android renderer.
- Example: replace 'canvas.ResetMatrix()' with 'canvas.Restore()' after using canvas.Save() at the start of the draw block.

### Next Questions

- Did the reporter try the Save/Restore workaround suggested by the maintainer?
- Is IgnorePixelScaling explicitly set, or is the default being used?
- Does this affect SKGLView as well as SKCanvasView?
- Is this still reproducible with current SkiaSharp versions (post-2.88.x)?

### Resolution Proposals

**Hypothesis:** The 2.88.0 Forms Android renderer changed how the canvas density transform is applied (evidenced by the RawInfo addition to SKPaintSurfaceEventArgs). ResetMatrix() resets to identity regardless of any pre-applied density scale, so code that uses ResetMatrix+LocalClipBounds breaks on high-density Android screens.

1. **Use Save/Restore instead of ResetMatrix** — workaround, confidence 0.88 (88%), cost/xs, validated=untested
   - Replace canvas.ResetMatrix() with canvas.Restore() (paired with canvas.Save() at the start of the SVG draw block). This correctly restores the canvas to the pre-SVG state — including density transforms — rather than resetting to identity.
2. **Investigate root cause in Forms Android renderer** — investigation, confidence 0.70 (70%), cost/m, validated=untested
   - Check what changed between 2.80.0 and 2.88.0 in the Forms Android SKCanvasView renderer regarding IgnorePixelScaling and density scale setup. The fix might be to document that ResetMatrix() is unsafe inside PaintSurface handlers or to ensure the canvas save stack protects the density transform.

**Recommended proposal:** Use Save/Restore instead of ResetMatrix

**Why:** The Save/Restore approach is the correct Skia pattern and the maintainer has already suggested it. It avoids relying on canvas state that was set up by the renderer.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.78 (78%) |
| Reason | Repro is incomplete (garbled XAML, empty Expected/Actual sections). Reporter has not responded to the maintainer's Save/Restore workaround suggestion. Need confirmation of whether the workaround helps and a minimal repro before deeper investigation. |
| Suggested repro platform | linux |

### Missing Info

- Did the Save/Restore workaround (suggested by maintainer) resolve the issue?
- Minimal reproducible code (XAML is garbled in the issue)
- Expected vs actual behavior description
- Whether IgnorePixelScaling is set and to what value

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Apply bug, views, android, compatibility labels | labels=type/bug, area/SkiaSharp.Views, os/Android, tenet/compatibility |
| add-comment | medium | 0.78 (78%) | Ask reporter if Save/Restore workaround works and request minimal repro | — |

**Comment draft for `add-comment`:**

```markdown
Hi @sisaacks — thanks for the report and the screenshots. The regression you're seeing between 2.80.0 and 2.88.0 on Android is likely related to how `canvas.ResetMatrix()` interacts with the canvas density transform that the Android renderer sets up.

As @mattleibow mentioned, please try wrapping your SVG draw block with `canvas.Save()` / `canvas.Restore()` instead of using `canvas.ResetMatrix()`. For example:

```csharp
canvas.Save();
// ... your translate/scale/drawPicture calls ...
canvas.Restore(); // instead of canvas.ResetMatrix()

// Now LocalClipBounds returns the correct dimensions
float imageCenter = canvas.LocalClipBounds.Width / 2;
```

`ResetMatrix()` resets to the identity transform, which discards any density scaling that was set up by the renderer. `Restore()` returns to exactly the state before your draw block, preserving that context.

Could you also share:
1. Whether the `Save()`/`Restore()` workaround fixes the rendering?
2. A minimal reproducible code snippet (the XAML in the original report appears garbled)?
3. Whether `IgnorePixelScaling` is set on the `SKCanvasView`?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2210,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-29T17:18:00Z"
  },
  "summary": "Regression on Android in SkiaSharp.Views.Forms: SVG/canvas scaling is incorrect in 2.88.0 vs 2.80.0, likely caused by canvas.ResetMatrix() losing the density transform that was set up by the Forms Android renderer.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.85
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.82
    },
    "platforms": [
      "os/Android"
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
      "errorMessage": "Image scaling incorrect on Android after upgrading from 2.80.0 to 2.88.0; canvas.ResetMatrix() and canvas.Scale() appear to behave differently",
      "reproQuality": "partial",
      "targetFrameworks": [
        "Xamarin.Android"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Use SkiaSharp.Views.Forms.SKCanvasView in a Xamarin.Forms Android app",
        "In PaintSurface handler: call canvas.Translate, canvas.Scale, canvas.DrawPicture to render an SVG",
        "Call canvas.ResetMatrix() then use canvas.LocalClipBounds for subsequent element positioning",
        "Observe that with 2.88.0 the elements are scaled/positioned incorrectly on Android; 2.80.0 renders correctly"
      ],
      "attachments": [
        {
          "url": "https://user-images.githubusercontent.com/62118624/183975619-6cbc5c3b-6783-464c-855c-f1bc3fa3a368.jpg",
          "filename": "2.80.0.jpg",
          "description": "Screenshot showing correct rendering in SkiaSharp 2.80.0"
        },
        {
          "url": "https://user-images.githubusercontent.com/62118624/183975716-e21f89e5-87a1-4f6a-9e97-c0714b62c1b5.jpg",
          "filename": "2.88.0.jpg",
          "description": "Screenshot showing incorrect/scaled rendering in SkiaSharp 2.88.0"
        }
      ],
      "environmentDetails": "Samsung Galaxy S10/Z Flip 3 (Android 12), Xamarin.Forms 5.0.0.2515, Xamarin.Essentials 1.7.3, VS 2022",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.0",
        "2.88.0"
      ],
      "workedIn": "2.80.0",
      "brokeIn": "2.88.0",
      "currentRelevance": "unknown",
      "relevanceReason": "Xamarin.Forms is EOL; MAUI equivalents use a different canvas setup path. The core canvas.ResetMatrix() behavior is unchanged, but the Forms Android renderer may set up the canvas transform differently in 2.88.0 due to the IgnorePixelScaling/density changes."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.8,
      "reason": "Reporter explicitly states worked in 2.80.0, broken in 2.88.0 on Android. iOS unaffected. Rolling back to 2.80.0 fixes the issue.",
      "workedInVersion": "2.80.0",
      "brokeInVersion": "2.88.0"
    }
  },
  "analysis": {
    "summary": "The reporter uses canvas.ResetMatrix() followed by canvas.LocalClipBounds to position elements after drawing an SVG. In 2.88.0, Android canvas density handling changed: the Forms Android renderer applies a density Scale() to the Skia canvas before invoking PaintSurface (when IgnorePixelScaling=true) or passes the raw physical-pixel surface. Calling ResetMatrix() strips this transform and sets the matrix to identity, so LocalClipBounds then returns physical-pixel dimensions rather than the expected logical/DIP dimensions. This causes positioning calculations that depend on LocalClipBounds to produce visually different results on high-density Android screens. The maintainer has already suggested using Save/Restore instead of ResetMatrix as a workaround.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs",
        "lines": "99-116",
        "finding": "When IgnorePixelScaling=true, the renderer calls skiaCanvas.Scale(density) then skiaCanvas.Save() before invoking PaintSurface. Calling canvas.ResetMatrix() inside PaintSurface resets to identity, losing the density scale. LocalClipBounds after reset then returns physical-pixel bounds rather than logical DIP bounds.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "890-893",
        "finding": "ResetMatrix() delegates to sk_canvas_reset_matrix which sets the transform to identity — it does NOT restore the canvas save stack. So it bypasses any density pre-transform unconditionally.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "276-281",
        "finding": "LocalClipBounds returns clip bounds in the current local coordinate space. After ResetMatrix() (identity), this reflects physical pixel dimensions rather than any scaled coordinate space.",
        "relevance": "related"
      },
      {
        "file": "changelogs/SkiaSharp.Views.Forms/2.88.0/SkiaSharp.Views.Forms.md",
        "finding": "In 2.88.0 SKPaintSurfaceEventArgs gained RawInfo property — signals that the canvas coordinate system was refactored in 2.88.0 to separate logical (info) from physical (rawInfo) pixel coordinates, which aligns with the regression.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "I am going to copy in my code below...what I do know is that the SkiaSharp.Views.Forms.SKCanvasView behaves completely different in 2.88.0.",
        "source": "issue body",
        "interpretation": "Confirms regression is in the Forms view specifically."
      },
      {
        "text": "I rolled back to 2.80.0 and it behaves correctly.",
        "source": "issue body",
        "interpretation": "Classic regression — rollback confirms the 2.88.0 change is the cause."
      },
      {
        "text": "Android is the only one that seems to have the issue",
        "source": "issue body",
        "interpretation": "Platform-specific — iOS does not apply density scale to the canvas in the same way, so ResetMatrix has no visible effect there."
      },
      {
        "text": "What happens if you wrap the draw logic inside a save restore instead of resetting the matrix?",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer confirms the workaround: use Save/Restore to preserve canvas state rather than ResetMatrix."
      }
    ],
    "rationale": "Classified as type/bug because the reporter observes a regression (2.80.0 worked, 2.88.0 broken) with screenshot evidence. The area is area/SkiaSharp.Views because the root cause is in the Forms Android canvas renderer's density transform setup, not in core SkiaSharp. The repro is partial — XAML code in the issue is garbled, expected/actual behavior fields are empty, and the reporter has not responded to the maintainer's workaround suggestion. A workaround (Save/Restore instead of ResetMatrix) already exists and was posted by the maintainer.",
    "workarounds": [
      "Use canvas.Save() before and canvas.Restore() after the SVG drawing block instead of calling canvas.ResetMatrix(). This preserves the density transform applied by the Forms Android renderer.",
      "Example: replace 'canvas.ResetMatrix()' with 'canvas.Restore()' after using canvas.Save() at the start of the draw block."
    ],
    "nextQuestions": [
      "Did the reporter try the Save/Restore workaround suggested by the maintainer?",
      "Is IgnorePixelScaling explicitly set, or is the default being used?",
      "Does this affect SKGLView as well as SKCanvasView?",
      "Is this still reproducible with current SkiaSharp versions (post-2.88.x)?"
    ],
    "resolution": {
      "hypothesis": "The 2.88.0 Forms Android renderer changed how the canvas density transform is applied (evidenced by the RawInfo addition to SKPaintSurfaceEventArgs). ResetMatrix() resets to identity regardless of any pre-applied density scale, so code that uses ResetMatrix+LocalClipBounds breaks on high-density Android screens.",
      "proposals": [
        {
          "title": "Use Save/Restore instead of ResetMatrix",
          "description": "Replace canvas.ResetMatrix() with canvas.Restore() (paired with canvas.Save() at the start of the SVG draw block). This correctly restores the canvas to the pre-SVG state — including density transforms — rather than resetting to identity.",
          "category": "workaround",
          "confidence": 0.88,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Investigate root cause in Forms Android renderer",
          "description": "Check what changed between 2.80.0 and 2.88.0 in the Forms Android SKCanvasView renderer regarding IgnorePixelScaling and density scale setup. The fix might be to document that ResetMatrix() is unsafe inside PaintSurface handlers or to ensure the canvas save stack protects the density transform.",
          "category": "investigation",
          "confidence": 0.7,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use Save/Restore instead of ResetMatrix",
      "recommendedReason": "The Save/Restore approach is the correct Skia pattern and the maintainer has already suggested it. It avoids relying on canvas state that was set up by the renderer."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.78,
      "reason": "Repro is incomplete (garbled XAML, empty Expected/Actual sections). Reporter has not responded to the maintainer's Save/Restore workaround suggestion. Need confirmation of whether the workaround helps and a minimal repro before deeper investigation.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Did the Save/Restore workaround (suggested by maintainer) resolve the issue?",
      "Minimal reproducible code (XAML is garbled in the issue)",
      "Expected vs actual behavior description",
      "Whether IgnorePixelScaling is set and to what value"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views, android, compatibility labels",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Android",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask reporter if Save/Restore workaround works and request minimal repro",
        "risk": "medium",
        "confidence": 0.78,
        "comment": "Hi @sisaacks — thanks for the report and the screenshots. The regression you're seeing between 2.80.0 and 2.88.0 on Android is likely related to how `canvas.ResetMatrix()` interacts with the canvas density transform that the Android renderer sets up.\n\nAs @mattleibow mentioned, please try wrapping your SVG draw block with `canvas.Save()` / `canvas.Restore()` instead of using `canvas.ResetMatrix()`. For example:\n\n```csharp\ncanvas.Save();\n// ... your translate/scale/drawPicture calls ...\ncanvas.Restore(); // instead of canvas.ResetMatrix()\n\n// Now LocalClipBounds returns the correct dimensions\nfloat imageCenter = canvas.LocalClipBounds.Width / 2;\n```\n\n`ResetMatrix()` resets to the identity transform, which discards any density scaling that was set up by the renderer. `Restore()` returns to exactly the state before your draw block, preserving that context.\n\nCould you also share:\n1. Whether the `Save()`/`Restore()` workaround fixes the rendering?\n2. A minimal reproducible code snippet (the XAML in the original report appears garbled)?\n3. Whether `IgnorePixelScaling` is set on the `SKCanvasView`?"
      }
    ]
  }
}
```

</details>
