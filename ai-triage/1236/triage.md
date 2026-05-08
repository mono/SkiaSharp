# Issue Triage Report — #1236

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T12:10:35Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views (0.97 (97%)) |
| Suggested action | ready-to-fix (0.88 (88%)) |

**Issue Summary:** SKElement on WPF renders blurry lines due to WPF bitmap interpolation when DPI scaling is active.

**Analysis:** WPF's default bitmap rendering applies bilinear interpolation when drawing a WriteableBitmap to screen. SKElement creates a bitmap at physical pixel resolution (scaled by DPI factor) but draws it at WPF unit size (ActualWidth x ActualHeight) without disabling WPF's interpolation. Additionally, integer truncation in CreateSize means size.Width/scaleX does not equal ActualWidth exactly, causing a sub-pixel mismatch that triggers interpolation.

**Recommendations:** **ready-to-fix** — Root cause is clearly identified in source code, fix path is known (NearestNeighbor + rect correction), and the reporter has provided the exact fix. No reproduction needed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Classic |
| Backends | backend/Raster |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

**Environment:** Windows 10, WPF, Visual Studio, SkiaSharp 1.68.2-preview.60

**Screenshots:**
- https://user-images.githubusercontent.com/18097859/79539376-7da28f80-8086-11ea-9a51-7c72dfeb3208.png — Expected sharp pixel-accurate lines at 4x zoom
- https://user-images.githubusercontent.com/18097859/79539419-914df600-8086-11ea-92ba-ed0aa122dabc.png — Actual blurry lines when DPI scaling is active

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | net472 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.2-preview.60 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Current SKElement.cs still uses drawingContext.DrawImage(bitmap, new Rect(0, 0, ActualWidth, ActualHeight)) without setting BitmapScalingMode, so the blurry interpolation issue persists. |

## Analysis

### Technical Summary

WPF's default bitmap rendering applies bilinear interpolation when drawing a WriteableBitmap to screen. SKElement creates a bitmap at physical pixel resolution (scaled by DPI factor) but draws it at WPF unit size (ActualWidth x ActualHeight) without disabling WPF's interpolation. Additionally, integer truncation in CreateSize means size.Width/scaleX does not equal ActualWidth exactly, causing a sub-pixel mismatch that triggers interpolation.

### Rationale

This is a confirmed code-level bug. SKElement draws a DPI-aware WriteableBitmap but does not disable WPF's default bilinear interpolation via RenderOptions.SetBitmapScalingMode. The reporter provides screenshots and the exact fix. Current source confirms the issue is still present. Severity is medium because the control renders correctly logically, only pixel-accuracy is lost.

### Key Signals

- "Lines are more or less blurry, depending on the offset of WPF units to physical device pixels." — **issue body** (Blurriness varies with DPI, confirming sub-pixel misalignment or interpolation as root cause.)
- "call RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor); in constructor of SKElement" — **issue body** (Reporter has already identified the fix — this is a well-understood WPF rendering pattern.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs` | 88 | direct | drawingContext.DrawImage(bitmap, new Rect(0, 0, ActualWidth, ActualHeight)). The bitmap is created at physical pixel size but is drawn to the full ActualWidth x ActualHeight WPF rect. No BitmapScalingMode is set, so WPF uses default bilinear interpolation, causing blurry output. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs` | 68 | direct | bitmap = new WriteableBitmap(info.Width, size.Height, BitmapDpi * scaleX, BitmapDpi * scaleY, ...). Bitmap DPI is set correctly so WPF knows physical resolution, but without RenderOptions.SetBitmapScalingMode(NearestNeighbor), WPF still interpolates. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs` | 116-121 | related | CreateSize returns (int)(w * scaleX) which truncates fractional pixels. Using ActualWidth instead of size.Width/scaleX in DrawImage introduces a sub-pixel offset that can trigger bilinear filtering. |

### Resolution Proposals

**Hypothesis:** Setting BitmapScalingMode.NearestNeighbor on the element and correcting the DrawImage rect to use size.Width/scaleX instead of ActualWidth will eliminate the interpolation artifacts.

1. **Set NearestNeighbor BitmapScalingMode and fix DrawImage rect** — fix, confidence 0.90 (90%), cost/xs, validated=untested
   - In the SKElement constructor, add RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor). In OnRender, replace the DrawImage call to use size.Width/scaleX and size.Height/scaleY instead of ActualWidth/ActualHeight.

```csharp
// In constructor:
RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);

// In OnRender, replace DrawImage call:
drawingContext.DrawImage(bitmap, new Rect(0, 0, size.Width / scaleX, size.Height / scaleY));
```

**Recommended proposal:** Set NearestNeighbor BitmapScalingMode and fix DrawImage rect

**Why:** Matches reporter's own analysis; NearestNeighbor is correct for pixel-accurate rendering; the rect correction prevents sub-pixel interpolation from integer truncation.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.88 (88%) |
| Reason | Root cause is clearly identified in source code, fix path is known (NearestNeighbor + rect correction), and the reporter has provided the exact fix. No reproduction needed. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply classification labels | labels=type/bug, area/SkiaSharp.Views, os/Windows-Classic, backend/Raster, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Acknowledge bug, confirm root cause, provide workaround and note fix path | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and for providing the fix!

This is a confirmed bug. The `SKElement` creates the `WriteableBitmap` at the correct physical pixel resolution and DPI, but WPF's default bilinear interpolation fires because `BitmapScalingMode` is not set to `NearestNeighbor`. There is also a minor sub-pixel rect mismatch due to integer truncation in `CreateSize`.

**Workaround (until a fix is released):**

Subclass `SKElement` and apply the fix yourself:

```csharp
public class SharpSKElement : SKElement
{
    public SharpSKElement()
    {
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
    }
}
```

Setting `NearestNeighbor` alone is sufficient for most cases. We plan to apply both fixes in `SKElement` directly.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1236,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T12:10:35Z"
  },
  "summary": "SKElement on WPF renders blurry lines due to WPF bitmap interpolation when DPI scaling is active.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.97
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/Raster"
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
      "reproQuality": "partial",
      "targetFrameworks": [
        "net472"
      ]
    },
    "reproEvidence": {
      "codeSnippets": [],
      "screenshots": [
        {
          "url": "https://user-images.githubusercontent.com/18097859/79539376-7da28f80-8086-11ea-9a51-7c72dfeb3208.png",
          "description": "Expected sharp pixel-accurate lines at 4x zoom"
        },
        {
          "url": "https://user-images.githubusercontent.com/18097859/79539419-914df600-8086-11ea-92ba-ed0aa122dabc.png",
          "description": "Actual blurry lines when DPI scaling is active"
        }
      ],
      "repoLinks": [],
      "environmentDetails": "Windows 10, WPF, Visual Studio, SkiaSharp 1.68.2-preview.60"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.2-preview.60"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Current SKElement.cs still uses drawingContext.DrawImage(bitmap, new Rect(0, 0, ActualWidth, ActualHeight)) without setting BitmapScalingMode, so the blurry interpolation issue persists."
    }
  },
  "analysis": {
    "summary": "WPF's default bitmap rendering applies bilinear interpolation when drawing a WriteableBitmap to screen. SKElement creates a bitmap at physical pixel resolution (scaled by DPI factor) but draws it at WPF unit size (ActualWidth x ActualHeight) without disabling WPF's interpolation. Additionally, integer truncation in CreateSize means size.Width/scaleX does not equal ActualWidth exactly, causing a sub-pixel mismatch that triggers interpolation.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs",
        "lines": "88",
        "finding": "drawingContext.DrawImage(bitmap, new Rect(0, 0, ActualWidth, ActualHeight)). The bitmap is created at physical pixel size but is drawn to the full ActualWidth x ActualHeight WPF rect. No BitmapScalingMode is set, so WPF uses default bilinear interpolation, causing blurry output.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs",
        "lines": "68",
        "finding": "bitmap = new WriteableBitmap(info.Width, size.Height, BitmapDpi * scaleX, BitmapDpi * scaleY, ...). Bitmap DPI is set correctly so WPF knows physical resolution, but without RenderOptions.SetBitmapScalingMode(NearestNeighbor), WPF still interpolates.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs",
        "lines": "116-121",
        "finding": "CreateSize returns (int)(w * scaleX) which truncates fractional pixels. Using ActualWidth instead of size.Width/scaleX in DrawImage introduces a sub-pixel offset that can trigger bilinear filtering.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Lines are more or less blurry, depending on the offset of WPF units to physical device pixels.",
        "source": "issue body",
        "interpretation": "Blurriness varies with DPI, confirming sub-pixel misalignment or interpolation as root cause."
      },
      {
        "text": "call RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor); in constructor of SKElement",
        "source": "issue body",
        "interpretation": "Reporter has already identified the fix — this is a well-understood WPF rendering pattern."
      }
    ],
    "rationale": "This is a confirmed code-level bug. SKElement draws a DPI-aware WriteableBitmap but does not disable WPF's default bilinear interpolation via RenderOptions.SetBitmapScalingMode. The reporter provides screenshots and the exact fix. Current source confirms the issue is still present. Severity is medium because the control renders correctly logically, only pixel-accuracy is lost.",
    "resolution": {
      "hypothesis": "Setting BitmapScalingMode.NearestNeighbor on the element and correcting the DrawImage rect to use size.Width/scaleX instead of ActualWidth will eliminate the interpolation artifacts.",
      "proposals": [
        {
          "title": "Set NearestNeighbor BitmapScalingMode and fix DrawImage rect",
          "category": "fix",
          "effort": "cost/xs",
          "confidence": 0.9,
          "validated": "untested",
          "description": "In the SKElement constructor, add RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor). In OnRender, replace the DrawImage call to use size.Width/scaleX and size.Height/scaleY instead of ActualWidth/ActualHeight.",
          "codeSnippet": "// In constructor:\nRenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);\n\n// In OnRender, replace DrawImage call:\ndrawingContext.DrawImage(bitmap, new Rect(0, 0, size.Width / scaleX, size.Height / scaleY));"
        }
      ],
      "recommendedProposal": "Set NearestNeighbor BitmapScalingMode and fix DrawImage rect",
      "recommendedReason": "Matches reporter's own analysis; NearestNeighbor is correct for pixel-accurate rendering; the rect correction prevents sub-pixel interpolation from integer truncation."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.88,
      "reason": "Root cause is clearly identified in source code, fix path is known (NearestNeighbor + rect correction), and the reporter has provided the exact fix. No reproduction needed.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply classification labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-Classic",
          "backend/Raster",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge bug, confirm root cause, provide workaround and note fix path",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report and for providing the fix!\n\nThis is a confirmed bug. The `SKElement` creates the `WriteableBitmap` at the correct physical pixel resolution and DPI, but WPF's default bilinear interpolation fires because `BitmapScalingMode` is not set to `NearestNeighbor`. There is also a minor sub-pixel rect mismatch due to integer truncation in `CreateSize`.\n\n**Workaround (until a fix is released):**\n\nSubclass `SKElement` and apply the fix yourself:\n\n```csharp\npublic class SharpSKElement : SKElement\n{\n    public SharpSKElement()\n    {\n        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);\n    }\n}\n```\n\nSetting `NearestNeighbor` alone is sufficient for most cases. We plan to apply both fixes in `SKElement` directly."
      }
    ]
  }
}
```

</details>
