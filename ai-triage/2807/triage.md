# Issue Triage Report — #2807

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:36:00Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp.Views.Uno (0.85 (85%)) |
| Suggested action | needs-info (0.80 (80%)) |

**Issue Summary:** SKXamlCanvas PointerWheelChanged event only fires in the top-left portion of the canvas when running an Uno WinUI app on a high-DPI Surface device, while other pointer events (PointerDown, PointerMoved) work correctly across the full canvas area.

**Analysis:** On high-DPI devices, the SKXamlCanvas background brush in the Uno Skia implementation is created from a WriteableBitmap sized at physical pixels with Stretch.Fill, which may cause Uno's hit-test region to track physical pixel coordinates rather than logical ones, limiting PointerWheelChanged dispatch to a sub-region of the canvas.

**Recommendations:** **needs-info** — Bug symptoms are consistent with a DPI hit-test region issue, but critical info is missing: whether the Skia renderer or native WinUI renderer is in use, the exact DPI setting, and any IgnorePixelScaling behavior. The code investigation identified two plausible code paths and an asymmetry between them, but without confirming which path is active, a targeted fix cannot be made.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Uno |
| Platforms | os/Windows-WinUI |
| Backends | — |
| Tenets | — |
| Partner | partner/unoplatform |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create an Uno WinUI app with SKXamlCanvas filling most of the screen
2. Attach PointerWheelChanged and PointerPressed event handlers
3. Run on a high-DPI device (e.g., Surface with 150% or 200% scaling)
4. Observe that PointerWheelChanged only fires in the top-left corner while PointerPressed fires everywhere

**Environment:** Surface device, Windows, SkiaSharp.Views.Uno.WinUI 2.88.7, SkiaSharp.Views.WinUI 2.88.7, Visual Studio (Windows)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | platform-specific |
| Error message | — |
| Repro quality | partial |
| Target frameworks | net8.0-windows |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.7 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The DPI scaling logic in SKXamlCanvas (Skia and Shared implementations) has not materially changed since 2.88.x. |

## Analysis

### Technical Summary

On high-DPI devices, the SKXamlCanvas background brush in the Uno Skia implementation is created from a WriteableBitmap sized at physical pixels with Stretch.Fill, which may cause Uno's hit-test region to track physical pixel coordinates rather than logical ones, limiting PointerWheelChanged dispatch to a sub-region of the canvas.

### Rationale

The reporter describes a clear, device-specific discrepancy: PointerWheelChanged does not fire outside the top-left corner on a high-DPI Surface but works elsewhere. The asymmetry with PointerDown (which fires correctly) suggests a hit-test region issue specific to how Uno routes wheel events. The Skia backend implementation uses Stretch.Fill with a physical-pixel-sized WriteableBitmap whereas the native WinUI implementation uses Stretch.None plus an explicit ScaleTransform — this difference is a plausible root cause. The bug is DPI-dependent (desktop at default DPI works, Surface at higher DPI fails).

### Key Signals

- "PointerWheelChanged-Event only gets invoked when the mouse pointer is in the top left corner of the Canvas" — **issue body** (Symptom consistent with a hit-test region smaller than the logical canvas — the 'active' region scales inversely with DPI.)
- "It feels like it is only nearly respecting half of the screen size" — **issue body** (At 2x DPI the physical bitmap is 2x the logical size; if the hit-test region uses physical pixel dimensions it would be half the logical area in each axis.)
- "other events like PointerDown get invoked correctly above every part of the canvas" — **issue body** (PointerPressed/PointerMoved likely use the XAML element bounds for hit testing; PointerWheelChanged routing in Uno may use a different path that is sensitive to the background brush geometry.)
- "On my desktop, it works as expected" — **issue body** (Default 100% DPI means no scaling, so the physical and logical sizes are identical and the bug does not manifest.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI.Skia/SKXamlCanvas.Skia.cs` | 89-100 | direct | WriteableBitmap created at physical pixel dimensions (info.Width × info.Height where info is DPI-scaled). ImageBrush uses Stretch.Fill with no ScaleTransform. On high-DPI, the bitmap's intrinsic DIP size equals the physical pixel count, which is larger than the logical canvas; Stretch.Fill rescales it, but Uno's internal geometry for the brush may record the unscaled DIP extent, potentially restricting the hit-test region. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 229-269 | related | The native WinUI implementation also creates a WriteableBitmap at physical pixel size, but uses Stretch.None with UpdateBrushScale() applying ScaleTransform(1/Dpi, 1/Dpi). This is a different approach: the brush shrinks from physical to logical size via transform rather than Stretch.Fill. On Windows the Uno WinUI package defers to this implementation for -windows TFM. |
| `source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI.Shared/SKXamlCanvas.cs` | 91-95 | related | Shared Uno implementation reads DPI via DisplayInformation.GetForCurrentView().LogicalDpi / 96. On WinUI 3, this UWP-era API may return different values than XamlRoot.RasterizationScale used by the native WinUI implementation. A mismatch could cause the computed physical pixel size to be incorrect. |
| `source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI/SkiaSharp.Views.Uno.WinUI.csproj` | 113-115 | context | For -windows TFM all Uno-specific code is excluded and the package defers to SkiaSharp.Views.WinUI. A Surface running native WinUI would therefore use SKXamlCanvas from SkiaSharp.Views.WinUI. A Skia-renderer Uno app on Windows uses SKXamlCanvas.Skia.cs. |

### Next Questions

- Is the reporter using the native WinUI renderer or the Uno Skia renderer on Windows?
- What exact DPI/scale factor is the Surface device set to?
- Does the issue reproduce when IgnorePixelScaling=true on SKXamlCanvas?
- Is the issue in SkiaSharp.Views.WinUI (native) or SkiaSharp.Views.Uno.WinUI.Skia (Skia renderer)?
- Is the Canvas Width/Height fixed or does it use stretch layout?

### Resolution Proposals

**Hypothesis:** On high-DPI, the Skia-renderer Uno implementation sets a physical-pixel-sized WriteableBitmap as the Canvas Background with Stretch.Fill. Uno's internal hit-test geometry for the brush may be computed from the unscaled DIP dimensions of the bitmap (= physical pixels), making the effective hit-test region smaller than the logical canvas by a factor of 1/Dpi in each dimension — matching the 'top left corner only' symptom.

1. **Request additional environment details** — investigation, confidence 0.90 (90%), cost/xs, validated=untested
   - Ask the reporter whether they are using the Uno Skia renderer or native WinUI on Windows, the exact DPI setting, and whether the issue reproduces with IgnorePixelScaling=true. This will narrow the root cause to the correct code path.
2. **Align Skia-renderer brush setup with WinUI implementation** — fix, confidence 0.65 (65%), cost/s, validated=untested
   - In SKXamlCanvas.Skia.cs, replace Stretch.Fill with Stretch.None and add a ScaleTransform(1/Dpi, 1/Dpi) on the ImageBrush.Transform (matching the native WinUI approach). This would ensure Uno's hit-test geometry is based on the logical canvas size rather than the physical pixel bitmap size.

**Recommended proposal:** Request additional environment details

**Why:** Without knowing whether the reporter uses the Uno Skia renderer or native WinUI on Windows, the exact fix path is unclear. Gathering environment info first avoids a wasted fix attempt.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.80 (80%) |
| Reason | Bug symptoms are consistent with a DPI hit-test region issue, but critical info is missing: whether the Skia renderer or native WinUI renderer is in use, the exact DPI setting, and any IgnorePixelScaling behavior. The code investigation identified two plausible code paths and an asymmetry between them, but without confirming which path is active, a targeted fix cannot be made. |
| Suggested repro platform | windows |

### Missing Info

- Which Uno renderer is in use on Windows: native WinUI or Uno Skia renderer?
- Exact Windows display DPI/scale setting on the Surface device
- Does the issue reproduce with SKXamlCanvas.IgnorePixelScaling = true?
- Target framework: net8.0-windows (WinUI) or net8.0 with Skia renderer?
- Does the issue also affect PointerMoved or only PointerWheelChanged?

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Apply bug, Uno views, Windows WinUI, and partner labels | labels=type/bug, area/SkiaSharp.Views.Uno, os/Windows-WinUI, partner/unoplatform |
| add-comment | medium | 0.80 (80%) | Request environment details to identify which code path (Skia renderer vs native WinUI) is involved | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! The DPI-dependent hit-test area you're describing is a recognized class of issue. To narrow down the root cause, could you provide:

1. **Renderer**: Is your Uno app using the **native WinUI renderer** or the **Uno Skia renderer** on Windows? (Check your app's `AppHead` or project configuration — Skia renderer apps typically reference `Uno.WinUI.Skia.*` packages.)
2. **Display scale**: What DPI/scale percentage is set on the Surface device? (Settings → Display → Scale)
3. **IgnorePixelScaling**: Does setting `IgnorePixelScaling="True"` on the `SKXamlCanvas` change the behavior?
4. **Target framework**: Is the Windows target `net8.0-windows10.0.xxxxx` or `net8.0`?

This will tell us whether the issue is in the native WinUI code path or the Uno Skia rendering path, as the two have different DPI handling implementations.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2807,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:36:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKXamlCanvas PointerWheelChanged event only fires in the top-left portion of the canvas when running an Uno WinUI app on a high-DPI Surface device, while other pointer events (PointerDown, PointerMoved) work correctly across the full canvas area.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp.Views.Uno",
      "confidence": 0.85
    },
    "platforms": [
      "os/Windows-WinUI"
    ],
    "partner": "partner/unoplatform"
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "platform-specific",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0-windows"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an Uno WinUI app with SKXamlCanvas filling most of the screen",
        "Attach PointerWheelChanged and PointerPressed event handlers",
        "Run on a high-DPI device (e.g., Surface with 150% or 200% scaling)",
        "Observe that PointerWheelChanged only fires in the top-left corner while PointerPressed fires everywhere"
      ],
      "environmentDetails": "Surface device, Windows, SkiaSharp.Views.Uno.WinUI 2.88.7, SkiaSharp.Views.WinUI 2.88.7, Visual Studio (Windows)"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.7"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The DPI scaling logic in SKXamlCanvas (Skia and Shared implementations) has not materially changed since 2.88.x."
    }
  },
  "analysis": {
    "summary": "On high-DPI devices, the SKXamlCanvas background brush in the Uno Skia implementation is created from a WriteableBitmap sized at physical pixels with Stretch.Fill, which may cause Uno's hit-test region to track physical pixel coordinates rather than logical ones, limiting PointerWheelChanged dispatch to a sub-region of the canvas.",
    "rationale": "The reporter describes a clear, device-specific discrepancy: PointerWheelChanged does not fire outside the top-left corner on a high-DPI Surface but works elsewhere. The asymmetry with PointerDown (which fires correctly) suggests a hit-test region issue specific to how Uno routes wheel events. The Skia backend implementation uses Stretch.Fill with a physical-pixel-sized WriteableBitmap whereas the native WinUI implementation uses Stretch.None plus an explicit ScaleTransform — this difference is a plausible root cause. The bug is DPI-dependent (desktop at default DPI works, Surface at higher DPI fails).",
    "keySignals": [
      {
        "text": "PointerWheelChanged-Event only gets invoked when the mouse pointer is in the top left corner of the Canvas",
        "source": "issue body",
        "interpretation": "Symptom consistent with a hit-test region smaller than the logical canvas — the 'active' region scales inversely with DPI."
      },
      {
        "text": "It feels like it is only nearly respecting half of the screen size",
        "source": "issue body",
        "interpretation": "At 2x DPI the physical bitmap is 2x the logical size; if the hit-test region uses physical pixel dimensions it would be half the logical area in each axis."
      },
      {
        "text": "other events like PointerDown get invoked correctly above every part of the canvas",
        "source": "issue body",
        "interpretation": "PointerPressed/PointerMoved likely use the XAML element bounds for hit testing; PointerWheelChanged routing in Uno may use a different path that is sensitive to the background brush geometry."
      },
      {
        "text": "On my desktop, it works as expected",
        "source": "issue body",
        "interpretation": "Default 100% DPI means no scaling, so the physical and logical sizes are identical and the bug does not manifest."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI.Skia/SKXamlCanvas.Skia.cs",
        "lines": "89-100",
        "finding": "WriteableBitmap created at physical pixel dimensions (info.Width × info.Height where info is DPI-scaled). ImageBrush uses Stretch.Fill with no ScaleTransform. On high-DPI, the bitmap's intrinsic DIP size equals the physical pixel count, which is larger than the logical canvas; Stretch.Fill rescales it, but Uno's internal geometry for the brush may record the unscaled DIP extent, potentially restricting the hit-test region.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "229-269",
        "finding": "The native WinUI implementation also creates a WriteableBitmap at physical pixel size, but uses Stretch.None with UpdateBrushScale() applying ScaleTransform(1/Dpi, 1/Dpi). This is a different approach: the brush shrinks from physical to logical size via transform rather than Stretch.Fill. On Windows the Uno WinUI package defers to this implementation for -windows TFM.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI.Shared/SKXamlCanvas.cs",
        "lines": "91-95",
        "finding": "Shared Uno implementation reads DPI via DisplayInformation.GetForCurrentView().LogicalDpi / 96. On WinUI 3, this UWP-era API may return different values than XamlRoot.RasterizationScale used by the native WinUI implementation. A mismatch could cause the computed physical pixel size to be incorrect.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI/SkiaSharp.Views.Uno.WinUI.csproj",
        "lines": "113-115",
        "finding": "For -windows TFM all Uno-specific code is excluded and the package defers to SkiaSharp.Views.WinUI. A Surface running native WinUI would therefore use SKXamlCanvas from SkiaSharp.Views.WinUI. A Skia-renderer Uno app on Windows uses SKXamlCanvas.Skia.cs.",
        "relevance": "context"
      }
    ],
    "nextQuestions": [
      "Is the reporter using the native WinUI renderer or the Uno Skia renderer on Windows?",
      "What exact DPI/scale factor is the Surface device set to?",
      "Does the issue reproduce when IgnorePixelScaling=true on SKXamlCanvas?",
      "Is the issue in SkiaSharp.Views.WinUI (native) or SkiaSharp.Views.Uno.WinUI.Skia (Skia renderer)?",
      "Is the Canvas Width/Height fixed or does it use stretch layout?"
    ],
    "resolution": {
      "hypothesis": "On high-DPI, the Skia-renderer Uno implementation sets a physical-pixel-sized WriteableBitmap as the Canvas Background with Stretch.Fill. Uno's internal hit-test geometry for the brush may be computed from the unscaled DIP dimensions of the bitmap (= physical pixels), making the effective hit-test region smaller than the logical canvas by a factor of 1/Dpi in each dimension — matching the 'top left corner only' symptom.",
      "proposals": [
        {
          "title": "Request additional environment details",
          "description": "Ask the reporter whether they are using the Uno Skia renderer or native WinUI on Windows, the exact DPI setting, and whether the issue reproduces with IgnorePixelScaling=true. This will narrow the root cause to the correct code path.",
          "category": "investigation",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Align Skia-renderer brush setup with WinUI implementation",
          "description": "In SKXamlCanvas.Skia.cs, replace Stretch.Fill with Stretch.None and add a ScaleTransform(1/Dpi, 1/Dpi) on the ImageBrush.Transform (matching the native WinUI approach). This would ensure Uno's hit-test geometry is based on the logical canvas size rather than the physical pixel bitmap size.",
          "category": "fix",
          "confidence": 0.65,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Request additional environment details",
      "recommendedReason": "Without knowing whether the reporter uses the Uno Skia renderer or native WinUI on Windows, the exact fix path is unclear. Gathering environment info first avoids a wasted fix attempt."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.8,
      "reason": "Bug symptoms are consistent with a DPI hit-test region issue, but critical info is missing: whether the Skia renderer or native WinUI renderer is in use, the exact DPI setting, and any IgnorePixelScaling behavior. The code investigation identified two plausible code paths and an asymmetry between them, but without confirming which path is active, a targeted fix cannot be made.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "Which Uno renderer is in use on Windows: native WinUI or Uno Skia renderer?",
      "Exact Windows display DPI/scale setting on the Surface device",
      "Does the issue reproduce with SKXamlCanvas.IgnorePixelScaling = true?",
      "Target framework: net8.0-windows (WinUI) or net8.0 with Skia renderer?",
      "Does the issue also affect PointerMoved or only PointerWheelChanged?"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, Uno views, Windows WinUI, and partner labels",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Uno",
          "os/Windows-WinUI",
          "partner/unoplatform"
        ]
      },
      {
        "type": "add-comment",
        "description": "Request environment details to identify which code path (Skia renderer vs native WinUI) is involved",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the report! The DPI-dependent hit-test area you're describing is a recognized class of issue. To narrow down the root cause, could you provide:\n\n1. **Renderer**: Is your Uno app using the **native WinUI renderer** or the **Uno Skia renderer** on Windows? (Check your app's `AppHead` or project configuration — Skia renderer apps typically reference `Uno.WinUI.Skia.*` packages.)\n2. **Display scale**: What DPI/scale percentage is set on the Surface device? (Settings → Display → Scale)\n3. **IgnorePixelScaling**: Does setting `IgnorePixelScaling=\"True\"` on the `SKXamlCanvas` change the behavior?\n4. **Target framework**: Is the Windows target `net8.0-windows10.0.xxxxx` or `net8.0`?\n\nThis will tell us whether the issue is in the native WinUI code path or the Uno Skia rendering path, as the two have different DPI handling implementations."
      }
    ]
  }
}
```

</details>
