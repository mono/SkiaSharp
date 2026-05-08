# Issue Triage Report — #3234

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T16:01:00Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp (0.78 (78%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** On Windows, stretching/squeezing a bitmap at certain zoom levels causes a middle pixel column to be omitted, likely due to floating-point rounding in scale calculations — only reproducible on Windows with specific Display Scale settings.

**Analysis:** At certain fractional zoom/scale levels on Windows, Skia's image scaling can drop a pixel column due to floating-point rounding. The Windows canvas uses integer truncation (not rounding) when computing pixel dimensions from DPI-scaled view sizes (SKXamlCanvas.CreateSize), and floating-point scale transforms applied in user code can land on ratios where a source pixel column maps to a sub-pixel gap in the destination, effectively omitting it. This is Windows-only because Windows Display > Scale settings produce non-integer DPI multipliers (e.g. 1.25×, 1.5×) that interact with the truncation logic.

**Recommendations:** **needs-investigation** — Real visual bug with repro project and video evidence. Windows-only behavior suggests platform-specific rounding issue. Needs minimal repro to isolate Skia raster scaler vs canvas pixel dimension truncation.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | partner/maui |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Clone the reproduction repository at https://github.com/hyvanmielenpelit/GnollHackMAUIEasyBuild
2. Build it for Windows according to instructions
3. Start the game, press Play Game, and create a character to start playing
4. Choose a zoom level using the mouse wheel while focusing on your character
5. At some zoom levels, the middle pixels are omitted in the breathing animations

**Environment:** Windows 11, MSI Windows Laptop, SkiaSharp 3.116.0, Visual Studio Windows

**Repository links:**
- https://github.com/hyvanmielenpelit/GnollHackMAUIEasyBuild — Full reproduction project (MAUI game app)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Middle pixel column omitted during bitmap stretch/squeeze animation at certain zoom levels on Windows |
| Repro quality | partial |
| Target frameworks | net9.0-windows10.0.19041.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The bitmap drawing and DPI scaling code paths have not changed substantially since 3.116.0. The integer truncation in CreateSize() that could cause off-by-one pixel behavior is still present. |

## Analysis

### Technical Summary

At certain fractional zoom/scale levels on Windows, Skia's image scaling can drop a pixel column due to floating-point rounding. The Windows canvas uses integer truncation (not rounding) when computing pixel dimensions from DPI-scaled view sizes (SKXamlCanvas.CreateSize), and floating-point scale transforms applied in user code can land on ratios where a source pixel column maps to a sub-pixel gap in the destination, effectively omitting it. This is Windows-only because Windows Display > Scale settings produce non-integer DPI multipliers (e.g. 1.25×, 1.5×) that interact with the truncation logic.

### Rationale

This is a visual rendering bug specific to Windows — the reporter provides a reproduction project and video demonstrating the issue. The most likely root cause is floating-point rounding during bitmap scale transforms at fractional DPI-influenced zoom levels. The integer truncation in SKXamlCanvas.CreateSize() is a candidate contributing factor. Severity is medium because it is a visual artifact with no crash, and a workaround (choosing a different zoom level) exists.

### Key Signals

- "At some zoom levels, the middle pixels are omitted in the breathing animations" — **issue body** (Classic off-by-one or float rounding artifact during bitmap scaling; the issue is geometry-dependent (only at specific scale ratios).)
- "Happens only on Windows, possibly when scaled using Windows Display > Scale setting" — **issue body** (Non-integer Windows DPI multipliers (125%, 150%) interact with integer truncation in CreateSize(), making Windows uniquely susceptible.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 209-226 | direct | CreateSize() computes pixelSize as (int)(w * dpi) which truncates rather than rounds. With non-integer DPI values (1.25, 1.5, etc.) this can produce a pixel buffer one pixel narrower than expected, potentially leaving a pixel column unaddressed at the edge or interior of a scaled image. |
| `binding/SkiaSharp/SKCanvas.cs` | 500-510 | related | DrawImage(source, dest, sampling) calls sk_canvas_draw_image_rect. The sampling defaults to FilterQuality-derived options. At fractional scale ratios, Skia's raster scaler maps source pixels to floating-point destination coordinates; if the scale is such that the middle source column maps to a sub-pixel gap in the destination, it may not be rendered. |

### Workarounds

- Use a zoom level that avoids the problematic fractional scale ratio
- Draw the bitmap slightly wider (e.g., dest.Width + 1) to compensate for the dropped column
- Use Math.Ceiling instead of truncation when computing destination rectangle dimensions

### Next Questions

- Does the issue reproduce with a fixed display scale (100%) and only application-level zoom?
- Does using SKSamplingOptions.Linear vs Nearest-neighbor change the behavior?
- Does the pixel column disappear at predictable mathematical scale ratios (e.g., exactly N/M for small M)?
- Does the issue occur when using SKGLView instead of SKXamlCanvas?

### Resolution Proposals

**Hypothesis:** Floating-point rounding in Skia's image scaler causes a source pixel column to map to a sub-pixel gap at certain scale ratios, especially on Windows where the DPI multiplier is non-integer and SKXamlCanvas.CreateSize() truncates rather than rounds pixel dimensions.

1. **Use Math.Round in CreateSize() pixel dimension calculation** — fix, confidence 0.65 (65%), cost/xs, validated=untested
   - Change pixelSize computation from (int)(w * dpi) to (int)Math.Round(w * dpi) to avoid systematic under-counting of pixel dimensions at fractional DPI values.
2. **Investigate Skia scaler behavior at specific scale ratios** — investigation, confidence 0.85 (85%), cost/s, validated=untested
   - Reproduce with a minimal SkiaSharp drawing sample (no MAUI dependency) to determine if the pixel-dropping is in Skia's core raster path or in the Windows view layer.

**Recommended proposal:** Investigate Skia scaler behavior at specific scale ratios

**Why:** The full repro requires a large MAUI game app; a focused investigation with a minimal sample will isolate whether the bug is in Skia's raster path or the Windows canvas pixel rounding, enabling a targeted fix.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Real visual bug with repro project and video evidence. Windows-only behavior suggests platform-specific rounding issue. Needs minimal repro to isolate Skia raster scaler vs canvas pixel dimension truncation. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, core, windows, reliability labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability, partner/maui |
| add-comment | medium | 0.82 (82%) | Request minimal repro and additional environment info | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed reproduction steps and videos!

This looks like a floating-point rounding issue when stretching bitmaps at fractional scale ratios on Windows — the kind of artifact that can happen when the Windows Display Scale setting (e.g. 125% or 150%) interacts with application-level zoom calculations.

To help narrow down the root cause, it would be helpful to know:
1. **Does the issue reproduce at 100% Windows Display Scale?** (Settings → Display → Scale)
2. **Which Display Scale percentage** are you using when you observe the missing pixel column?
3. **Does the issue still occur if you use `SKSamplingOptions.Linear` explicitly** when drawing the bitmap instead of relying on the default?
4. **Could you share a minimal standalone SkiaSharp sample** (not the full game) that reproduces the pixel-dropping at a specific scale ratio?

In the meantime, you might try using `Math.Ceiling` on your destination rectangle dimensions when drawing the bitmap to ensure the full source is covered:
```csharp
var dest = new SKRect(x, y, (float)Math.Ceiling(x + scaledWidth), (float)Math.Ceiling(y + scaledHeight));
canvas.DrawBitmap(bitmap, dest);
```
This is a workaround, not a fix, but may help while the root cause is investigated.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3234,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T16:01:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "On Windows, stretching/squeezing a bitmap at certain zoom levels causes a middle pixel column to be omitted, likely due to floating-point rounding in scale calculations — only reproducible on Windows with specific Display Scale settings.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.78
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/reliability"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "Middle pixel column omitted during bitmap stretch/squeeze animation at certain zoom levels on Windows",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net9.0-windows10.0.19041.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Clone the reproduction repository at https://github.com/hyvanmielenpelit/GnollHackMAUIEasyBuild",
        "Build it for Windows according to instructions",
        "Start the game, press Play Game, and create a character to start playing",
        "Choose a zoom level using the mouse wheel while focusing on your character",
        "At some zoom levels, the middle pixels are omitted in the breathing animations"
      ],
      "environmentDetails": "Windows 11, MSI Windows Laptop, SkiaSharp 3.116.0, Visual Studio Windows",
      "repoLinks": [
        {
          "url": "https://github.com/hyvanmielenpelit/GnollHackMAUIEasyBuild",
          "description": "Full reproduction project (MAUI game app)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The bitmap drawing and DPI scaling code paths have not changed substantially since 3.116.0. The integer truncation in CreateSize() that could cause off-by-one pixel behavior is still present."
    }
  },
  "analysis": {
    "summary": "At certain fractional zoom/scale levels on Windows, Skia's image scaling can drop a pixel column due to floating-point rounding. The Windows canvas uses integer truncation (not rounding) when computing pixel dimensions from DPI-scaled view sizes (SKXamlCanvas.CreateSize), and floating-point scale transforms applied in user code can land on ratios where a source pixel column maps to a sub-pixel gap in the destination, effectively omitting it. This is Windows-only because Windows Display > Scale settings produce non-integer DPI multipliers (e.g. 1.25×, 1.5×) that interact with the truncation logic.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "209-226",
        "finding": "CreateSize() computes pixelSize as (int)(w * dpi) which truncates rather than rounds. With non-integer DPI values (1.25, 1.5, etc.) this can produce a pixel buffer one pixel narrower than expected, potentially leaving a pixel column unaddressed at the edge or interior of a scaled image.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "500-510",
        "finding": "DrawImage(source, dest, sampling) calls sk_canvas_draw_image_rect. The sampling defaults to FilterQuality-derived options. At fractional scale ratios, Skia's raster scaler maps source pixels to floating-point destination coordinates; if the scale is such that the middle source column maps to a sub-pixel gap in the destination, it may not be rendered.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "At some zoom levels, the middle pixels are omitted in the breathing animations",
        "source": "issue body",
        "interpretation": "Classic off-by-one or float rounding artifact during bitmap scaling; the issue is geometry-dependent (only at specific scale ratios)."
      },
      {
        "text": "Happens only on Windows, possibly when scaled using Windows Display > Scale setting",
        "source": "issue body",
        "interpretation": "Non-integer Windows DPI multipliers (125%, 150%) interact with integer truncation in CreateSize(), making Windows uniquely susceptible."
      }
    ],
    "rationale": "This is a visual rendering bug specific to Windows — the reporter provides a reproduction project and video demonstrating the issue. The most likely root cause is floating-point rounding during bitmap scale transforms at fractional DPI-influenced zoom levels. The integer truncation in SKXamlCanvas.CreateSize() is a candidate contributing factor. Severity is medium because it is a visual artifact with no crash, and a workaround (choosing a different zoom level) exists.",
    "nextQuestions": [
      "Does the issue reproduce with a fixed display scale (100%) and only application-level zoom?",
      "Does using SKSamplingOptions.Linear vs Nearest-neighbor change the behavior?",
      "Does the pixel column disappear at predictable mathematical scale ratios (e.g., exactly N/M for small M)?",
      "Does the issue occur when using SKGLView instead of SKXamlCanvas?"
    ],
    "workarounds": [
      "Use a zoom level that avoids the problematic fractional scale ratio",
      "Draw the bitmap slightly wider (e.g., dest.Width + 1) to compensate for the dropped column",
      "Use Math.Ceiling instead of truncation when computing destination rectangle dimensions"
    ],
    "resolution": {
      "hypothesis": "Floating-point rounding in Skia's image scaler causes a source pixel column to map to a sub-pixel gap at certain scale ratios, especially on Windows where the DPI multiplier is non-integer and SKXamlCanvas.CreateSize() truncates rather than rounds pixel dimensions.",
      "proposals": [
        {
          "title": "Use Math.Round in CreateSize() pixel dimension calculation",
          "description": "Change pixelSize computation from (int)(w * dpi) to (int)Math.Round(w * dpi) to avoid systematic under-counting of pixel dimensions at fractional DPI values.",
          "category": "fix",
          "confidence": 0.65,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Investigate Skia scaler behavior at specific scale ratios",
          "description": "Reproduce with a minimal SkiaSharp drawing sample (no MAUI dependency) to determine if the pixel-dropping is in Skia's core raster path or in the Windows view layer.",
          "category": "investigation",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Investigate Skia scaler behavior at specific scale ratios",
      "recommendedReason": "The full repro requires a large MAUI game app; a focused investigation with a minimal sample will isolate whether the bug is in Skia's raster path or the Windows canvas pixel rounding, enabling a targeted fix."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Real visual bug with repro project and video evidence. Windows-only behavior suggests platform-specific rounding issue. Needs minimal repro to isolate Skia raster scaler vs canvas pixel dimension truncation.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, core, windows, reliability labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/reliability",
          "partner/maui"
        ]
      },
      {
        "type": "add-comment",
        "description": "Request minimal repro and additional environment info",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed reproduction steps and videos!\n\nThis looks like a floating-point rounding issue when stretching bitmaps at fractional scale ratios on Windows — the kind of artifact that can happen when the Windows Display Scale setting (e.g. 125% or 150%) interacts with application-level zoom calculations.\n\nTo help narrow down the root cause, it would be helpful to know:\n1. **Does the issue reproduce at 100% Windows Display Scale?** (Settings → Display → Scale)\n2. **Which Display Scale percentage** are you using when you observe the missing pixel column?\n3. **Does the issue still occur if you use `SKSamplingOptions.Linear` explicitly** when drawing the bitmap instead of relying on the default?\n4. **Could you share a minimal standalone SkiaSharp sample** (not the full game) that reproduces the pixel-dropping at a specific scale ratio?\n\nIn the meantime, you might try using `Math.Ceiling` on your destination rectangle dimensions when drawing the bitmap to ensure the full source is covered:\n```csharp\nvar dest = new SKRect(x, y, (float)Math.Ceiling(x + scaledWidth), (float)Math.Ceiling(y + scaledHeight));\ncanvas.DrawBitmap(bitmap, dest);\n```\nThis is a workaround, not a fix, but may help while the root cause is investigated."
      }
    ]
  }
}
```

</details>
