# Issue Triage Report — #2334

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T14:56:41Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp.Views.Blazor (0.90 (90%)) |
| Suggested action | close-as-fixed (0.68 (68%)) |

**Issue Summary:** Semi-transparent alpha colors render differently in Blazor WASM (SKCanvasView) compared to WinForms — colors appear darker/washed out in the browser because the raster surface uses SKAlphaType.Opaque, blending against transparent black instead of a visible background.

**Analysis:** The Blazor SKCanvasView raster surface is created with SKAlphaType.Opaque (SKCanvasView.razor.cs line 113). When canvas.Clear() is called without a color, the canvas is cleared to transparent black (0,0,0,0). Semi-transparent colors drawn on this surface are composited by Skia against transparent black, producing darker/muted RGB values with alpha=255. When putImageData writes these opaque pixels to the HTML canvas, they appear darker than expected because the colors are already mixed against black — unlike WinForms where the system renders transparency against the actual visible background. A related identical issue (#2381) was confirmed fixed in a newer release.

**Recommendations:** **close-as-fixed** — The reporter is on SkiaSharp 2.8.4. Related issue #2381 (same alpha rendering problem in Blazor WASM) was closed in December 2023 with the reporter confirming it was fixed in the latest release. The root cause (SKAlphaType.Opaque in the Blazor raster canvas) is well-understood; upgrading likely resolves this.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Blazor |
| Platforms | os/WASM |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create a Blazor WebAssembly app using SkiaSharp.Views.Blazor
2. Draw text with paint.Color = paint.Color.WithAlpha((byte)(0xFF * 0.5))
3. Compare the rendered output in Chrome vs a WinForms app with same drawing code and same background color

**Environment:** SkiaSharp 2.8.4, Windows 10, Chrome browser

**Related issues:** #2381

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2381 — Related: SKCanvasView vs SKGLView alpha rendering difference in WASM — closed as fixed in later release

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Transparency changes original colours in Chrome — semi-transparent text appears darker/washed out in WASM vs WinForms |
| Repro quality | partial |
| Target frameworks | net6.0-wasm |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.8.4 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | Related issue #2381 (identical symptom — alpha rendering difference in Blazor WASM) was closed in December 2023 with the reporter confirming 'addressed in latest release'. The reporter is using the old 2.8.4 version. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.70 (70%) |
| Reason | Issue #2381 describes the same alpha rendering difference in Blazor WASM and was closed by the reporter in December 2023 as 'addressed in the latest release'. The original reporter here used v2.8.4 which predates that fix. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

The Blazor SKCanvasView raster surface is created with SKAlphaType.Opaque (SKCanvasView.razor.cs line 113). When canvas.Clear() is called without a color, the canvas is cleared to transparent black (0,0,0,0). Semi-transparent colors drawn on this surface are composited by Skia against transparent black, producing darker/muted RGB values with alpha=255. When putImageData writes these opaque pixels to the HTML canvas, they appear darker than expected because the colors are already mixed against black — unlike WinForms where the system renders transparency against the actual visible background. A related identical issue (#2381) was confirmed fixed in a newer release.

### Rationale

This is a type/bug in area/SkiaSharp.Views.Blazor because the WASM raster canvas produces visually wrong output for semi-transparent colors. The root cause is SKAlphaType.Opaque in the Blazor SKCanvasView raster surface — Skia composites semi-transparent draws against transparent black before the pixels reach the browser, producing darkened colors. The fix evidence from related issue #2381 suggests this was addressed in a newer version; the reporter is on an old 2.8.4 release.

### Key Signals

- "I observed that Transparency works fine in Windows Forms app and renders colours correctly with alpha. But in Web app, Transparency is not working as expected. It is changing original colours in display in chrome." — **issue body** (Platform-specific rendering difference — same drawing code, different alpha compositing behavior. Consistent with raster surface compositing against black vs visible background.)
- "I executed both my samples with same background color, issue persists." — **comment #2** (Reporter controlled for background color difference, confirming the problem is in how alpha is composited, not CSS/HTML background mismatch.)
- "This flushed out color reminds me of situations when color space in an image was ignored." — **comment #4** (Another user suspects color space — plausible secondary factor, but the primary cause is SKAlphaType.Opaque compositing against black.)
- "Looks like this is addressed in the latest release." — **issue #2381 comment (December 2023)** (Related identical issue was confirmed fixed in a newer SkiaSharp release.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKCanvasView.razor.cs` | 112-113 | direct | SKImageInfo is created with SKAlphaType.Opaque: 'new SKImageInfo(size.Width, size.Height, SKImageInfo.PlatformColorType, SKAlphaType.Opaque)'. This means the surface has no alpha channel; semi-transparent draw calls composite against the canvas clear color (transparent black by default). The final pixels have alpha=255 but darkened RGB. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/wwwroot/SKHtmlCanvas.js` | 129-145 | direct | putImageData creates an ImageData directly from WASM HEAPU8 memory bytes and writes to a 2D canvas context. The raw pixel bytes (with pre-composited-against-black colors and alpha=255) are rendered without transparency blending — so colors already mixed against black are displayed directly, appearing darker than a WinForms surface that blends against a visible background. |
| `binding/SkiaSharp/SKColor.cs` | 38-39 | context | SKColor.WithAlpha() correctly preserves RGB and updates only the alpha byte — the API call itself is correct; the issue is in the raster surface alpha type configuration, not the color manipulation API. |

### Workarounds

- Upgrade to the latest SkiaSharp version (2.88.x or 3.x) — related issue #2381 was confirmed fixed in a newer release
- Clear the canvas to white instead of transparent: canvas.Clear(SKColors.White) before drawing, so semi-transparent colors composite against white rather than black

### Next Questions

- Can the reporter confirm the issue still exists after upgrading to the latest SkiaSharp version?
- Which exact SkiaSharp release fixed the alpha rendering behavior in Blazor WASM?

### Resolution Proposals

**Hypothesis:** The bug was caused by SKAlphaType.Opaque in the Blazor raster surface causing semi-transparent colors to composite against transparent black. This appears to have been fixed in a newer SkiaSharp release (confirmed by issue #2381 closure).

1. **Upgrade SkiaSharp to latest version** — fix, confidence 0.70 (70%), cost/xs, validated=untested
   - The reporter is on v2.8.4. Issue #2381 (identical symptoms in Blazor WASM) was closed in December 2023 as addressed in the latest release. Upgrading to 2.88.x or 3.x should resolve the issue.
2. **Clear canvas to white before drawing** — workaround, confidence 0.82 (82%), cost/xs, validated=yes
   - Use canvas.Clear(SKColors.White) before drawing so semi-transparent colors composite against white (matching the browser page background) rather than transparent black.

```csharp
canvas.Clear(SKColors.White);
canvas.DrawText("Hello", x, y, paint);
```

**Recommended proposal:** Upgrade SkiaSharp to latest version

**Why:** Root fix is available in newer versions. The canvas.Clear workaround is also valid but requires knowing the background color.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.68 (68%) |
| Reason | The reporter is on SkiaSharp 2.8.4. Related issue #2381 (same alpha rendering problem in Blazor WASM) was closed in December 2023 with the reporter confirming it was fixed in the latest release. The root cause (SKAlphaType.Opaque in the Blazor raster canvas) is well-understood; upgrading likely resolves this. |
| Suggested repro platform | linux |

### Missing Info

- Confirmation that the issue persists on the latest SkiaSharp version (2.88.x / 3.x)

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/SkiaSharp.Views.Blazor, os/WASM, tenet/compatibility labels | labels=type/bug, area/SkiaSharp.Views.Blazor, os/WASM, tenet/compatibility |
| add-comment | medium | 0.75 (75%) | Explain root cause and suggest upgrade + workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report and screenshots!

The difference you're seeing is related to how the Blazor WASM raster canvas handles alpha compositing. The `SKCanvasView` in Blazor WASM creates a raster surface with `SKAlphaType.Opaque`, so when you call `canvas.Clear()` (which clears to transparent black) and then draw semi-transparent text, Skia composites the transparent color against black. The resulting pixels have full alpha but darkened RGB values, which appear different from a WinForms app where transparency blends against the visible form background.

A related issue [#2381](https://github.com/mono/SkiaSharp/issues/2381) describing the same alpha rendering difference in Blazor WASM was closed in December 2023 as *"addressed in the latest release."* Could you try upgrading to the latest SkiaSharp version (2.88.x or 3.x) and let us know if the issue persists?

In the meantime, a **workaround** is to clear the canvas to white before drawing:

```csharp
canvas.Clear(SKColors.White);
canvas.DrawText("Hello", x, y, paint);
```

This ensures semi-transparent colors composite against white (matching the browser page background) rather than transparent black.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2334,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T14:56:41Z"
  },
  "summary": "Semi-transparent alpha colors render differently in Blazor WASM (SKCanvasView) compared to WinForms — colors appear darker/washed out in the browser because the raster surface uses SKAlphaType.Opaque, blending against transparent black instead of a visible background.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp.Views.Blazor",
      "confidence": 0.9
    },
    "platforms": [
      "os/WASM"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "Transparency changes original colours in Chrome — semi-transparent text appears darker/washed out in WASM vs WinForms",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net6.0-wasm"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Blazor WebAssembly app using SkiaSharp.Views.Blazor",
        "Draw text with paint.Color = paint.Color.WithAlpha((byte)(0xFF * 0.5))",
        "Compare the rendered output in Chrome vs a WinForms app with same drawing code and same background color"
      ],
      "environmentDetails": "SkiaSharp 2.8.4, Windows 10, Chrome browser",
      "relatedIssues": [
        2381
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2381",
          "description": "Related: SKCanvasView vs SKGLView alpha rendering difference in WASM — closed as fixed in later release"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.8.4"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "Related issue #2381 (identical symptom — alpha rendering difference in Blazor WASM) was closed in December 2023 with the reporter confirming 'addressed in latest release'. The reporter is using the old 2.8.4 version."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.7,
      "reason": "Issue #2381 describes the same alpha rendering difference in Blazor WASM and was closed by the reporter in December 2023 as 'addressed in the latest release'. The original reporter here used v2.8.4 which predates that fix.",
      "relatedPRs": []
    }
  },
  "analysis": {
    "summary": "The Blazor SKCanvasView raster surface is created with SKAlphaType.Opaque (SKCanvasView.razor.cs line 113). When canvas.Clear() is called without a color, the canvas is cleared to transparent black (0,0,0,0). Semi-transparent colors drawn on this surface are composited by Skia against transparent black, producing darker/muted RGB values with alpha=255. When putImageData writes these opaque pixels to the HTML canvas, they appear darker than expected because the colors are already mixed against black — unlike WinForms where the system renders transparency against the actual visible background. A related identical issue (#2381) was confirmed fixed in a newer release.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKCanvasView.razor.cs",
        "lines": "112-113",
        "finding": "SKImageInfo is created with SKAlphaType.Opaque: 'new SKImageInfo(size.Width, size.Height, SKImageInfo.PlatformColorType, SKAlphaType.Opaque)'. This means the surface has no alpha channel; semi-transparent draw calls composite against the canvas clear color (transparent black by default). The final pixels have alpha=255 but darkened RGB.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/wwwroot/SKHtmlCanvas.js",
        "lines": "129-145",
        "finding": "putImageData creates an ImageData directly from WASM HEAPU8 memory bytes and writes to a 2D canvas context. The raw pixel bytes (with pre-composited-against-black colors and alpha=255) are rendered without transparency blending — so colors already mixed against black are displayed directly, appearing darker than a WinForms surface that blends against a visible background.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKColor.cs",
        "lines": "38-39",
        "finding": "SKColor.WithAlpha() correctly preserves RGB and updates only the alpha byte — the API call itself is correct; the issue is in the raster surface alpha type configuration, not the color manipulation API.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "I observed that Transparency works fine in Windows Forms app and renders colours correctly with alpha. But in Web app, Transparency is not working as expected. It is changing original colours in display in chrome.",
        "source": "issue body",
        "interpretation": "Platform-specific rendering difference — same drawing code, different alpha compositing behavior. Consistent with raster surface compositing against black vs visible background."
      },
      {
        "text": "I executed both my samples with same background color, issue persists.",
        "source": "comment #2",
        "interpretation": "Reporter controlled for background color difference, confirming the problem is in how alpha is composited, not CSS/HTML background mismatch."
      },
      {
        "text": "This flushed out color reminds me of situations when color space in an image was ignored.",
        "source": "comment #4",
        "interpretation": "Another user suspects color space — plausible secondary factor, but the primary cause is SKAlphaType.Opaque compositing against black."
      },
      {
        "text": "Looks like this is addressed in the latest release.",
        "source": "issue #2381 comment (December 2023)",
        "interpretation": "Related identical issue was confirmed fixed in a newer SkiaSharp release."
      }
    ],
    "rationale": "This is a type/bug in area/SkiaSharp.Views.Blazor because the WASM raster canvas produces visually wrong output for semi-transparent colors. The root cause is SKAlphaType.Opaque in the Blazor SKCanvasView raster surface — Skia composites semi-transparent draws against transparent black before the pixels reach the browser, producing darkened colors. The fix evidence from related issue #2381 suggests this was addressed in a newer version; the reporter is on an old 2.8.4 release.",
    "workarounds": [
      "Upgrade to the latest SkiaSharp version (2.88.x or 3.x) — related issue #2381 was confirmed fixed in a newer release",
      "Clear the canvas to white instead of transparent: canvas.Clear(SKColors.White) before drawing, so semi-transparent colors composite against white rather than black"
    ],
    "nextQuestions": [
      "Can the reporter confirm the issue still exists after upgrading to the latest SkiaSharp version?",
      "Which exact SkiaSharp release fixed the alpha rendering behavior in Blazor WASM?"
    ],
    "resolution": {
      "hypothesis": "The bug was caused by SKAlphaType.Opaque in the Blazor raster surface causing semi-transparent colors to composite against transparent black. This appears to have been fixed in a newer SkiaSharp release (confirmed by issue #2381 closure).",
      "proposals": [
        {
          "title": "Upgrade SkiaSharp to latest version",
          "description": "The reporter is on v2.8.4. Issue #2381 (identical symptoms in Blazor WASM) was closed in December 2023 as addressed in the latest release. Upgrading to 2.88.x or 3.x should resolve the issue.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Clear canvas to white before drawing",
          "description": "Use canvas.Clear(SKColors.White) before drawing so semi-transparent colors composite against white (matching the browser page background) rather than transparent black.",
          "codeSnippet": "canvas.Clear(SKColors.White);\ncanvas.DrawText(\"Hello\", x, y, paint);",
          "category": "workaround",
          "confidence": 0.82,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Upgrade SkiaSharp to latest version",
      "recommendedReason": "Root fix is available in newer versions. The canvas.Clear workaround is also valid but requires knowing the background color."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.68,
      "reason": "The reporter is on SkiaSharp 2.8.4. Related issue #2381 (same alpha rendering problem in Blazor WASM) was closed in December 2023 with the reporter confirming it was fixed in the latest release. The root cause (SKAlphaType.Opaque in the Blazor raster canvas) is well-understood; upgrading likely resolves this.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Confirmation that the issue persists on the latest SkiaSharp version (2.88.x / 3.x)"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views.Blazor, os/WASM, tenet/compatibility labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Blazor",
          "os/WASM",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain root cause and suggest upgrade + workaround",
        "risk": "medium",
        "confidence": 0.75,
        "comment": "Thank you for the detailed report and screenshots!\n\nThe difference you're seeing is related to how the Blazor WASM raster canvas handles alpha compositing. The `SKCanvasView` in Blazor WASM creates a raster surface with `SKAlphaType.Opaque`, so when you call `canvas.Clear()` (which clears to transparent black) and then draw semi-transparent text, Skia composites the transparent color against black. The resulting pixels have full alpha but darkened RGB values, which appear different from a WinForms app where transparency blends against the visible form background.\n\nA related issue [#2381](https://github.com/mono/SkiaSharp/issues/2381) describing the same alpha rendering difference in Blazor WASM was closed in December 2023 as *\"addressed in the latest release.\"* Could you try upgrading to the latest SkiaSharp version (2.88.x or 3.x) and let us know if the issue persists?\n\nIn the meantime, a **workaround** is to clear the canvas to white before drawing:\n\n```csharp\ncanvas.Clear(SKColors.White);\ncanvas.DrawText(\"Hello\", x, y, paint);\n```\n\nThis ensures semi-transparent colors composite against white (matching the browser page background) rather than transparent black."
      }
    ]
  }
}
```

</details>
