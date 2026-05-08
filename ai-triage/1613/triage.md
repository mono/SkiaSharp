# Issue Triage Report — #1613

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-29T13:38:05Z |
| Type | type/feature-request (0.95 (95%)) |
| Area | area/SkiaSharp.Views (0.85 (85%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** Reporter requests a dedicated SVG image control for Xamarin.Forms similar to FFImageLoading's SVG support, citing concern that FFImageLoading is unmaintained.

**Analysis:** Request for a first-party SVG image view control in the SkiaSharp.Views.Forms package, analogous to FFImageLoading's SvgCachedImage. No such control currently exists in SkiaSharp; SVG rendering is delegated to community packages such as Svg.Skia. The Xamarin.Forms platform is now legacy (superseded by MAUI).

**Recommendations:** **keep-open** — Valid feature request with community interest; no current implementation in SkiaSharp. Xamarin.Forms is legacy but the concept is relevant to MAUI. Keeping open allows tracking demand for a potential MAUI SVG view.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp.Views |
| Platforms | os/Android, os/iOS |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Environment:** Xamarin.Forms (unspecified version), no SkiaSharp version given

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1613#issuecomment-774110457 — Maintainer (mattleibow) suggested Svg.Skia NuGet as an alternative
- https://github.com/mono/SkiaSharp/issues/120 — Older broader SVG support request (closed 2016)

## Analysis

### Technical Summary

Request for a first-party SVG image view control in the SkiaSharp.Views.Forms package, analogous to FFImageLoading's SvgCachedImage. No such control currently exists in SkiaSharp; SVG rendering is delegated to community packages such as Svg.Skia. The Xamarin.Forms platform is now legacy (superseded by MAUI).

### Rationale

The issue uses the [FEATURE] prefix and is phrased as a request ('Is there any chance to create...'), making it unambiguously a feature request. No bug is described. The Xamarin.Forms platform targeted by this request is now legacy; the modern equivalent is SkiaSharp.Views.Maui. The maintainer already pointed to Svg.Skia as a workaround, indicating the feature is out-of-scope for the core library.

### Key Signals

- "Is there any chance to create an SVG control that we can use in xamarin forms" — **issue body** (Explicit feature request — new SVG display control for Xamarin.Forms)
- "I would suggesting looking at Svg.Skia: https://www.nuget.org/packages/Svg.Skia" — **comment by mattleibow** (Maintainer acknowledged the gap and pointed to the Svg.Skia community package as the current solution, implying this is not planned for the SkiaSharp core library)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKSVG.cs` | — | direct | SKSvgCanvas provides SVG *output* (rendering to an SVG stream) but no SVG *input* (parsing/displaying SVG files as images). No SVG image view control exists in the SkiaSharp binding layer. |
| `source/SkiaSharp.Views` | — | direct | The views layer contains platform-specific canvas views (SKCanvasView, SKGLView) for raster/GL rendering. No SVG view or image control is present. Xamarin.Forms-specific views are not in the current source tree (legacy Xamarin.Forms support was removed after MAUI migration). |

### Workarounds

- Use the Svg.Skia NuGet package (https://www.nuget.org/packages/Svg.Skia) together with SKCanvasView to render SVG files in Xamarin.Forms.
- For MAUI (the successor to Xamarin.Forms), use SkiaSharp.Views.Maui with a custom SKCanvasView that loads and renders SVG via Svg.Skia.

### Next Questions

- Is there appetite to add an official SVG view control to SkiaSharp.Views.Maui (the MAUI successor)?
- Should SkiaSharp bundle SVG parsing capability (currently only SVG output is supported via SKSvgCanvas)?

### Resolution Proposals

**Hypothesis:** The requested SVG control does not exist in SkiaSharp and is currently out-of-scope; the recommended path is Svg.Skia for Xamarin.Forms, or a custom SKCanvasView + Svg.Skia for MAUI.

1. **Use Svg.Skia with SKCanvasView** — workaround, confidence 0.90 (90%), cost/s, validated=untested
   - Add Svg.Skia NuGet package and render SVG in the PaintSurface handler of SKCanvasView. This works in both Xamarin.Forms and MAUI.
2. **Add SVG image view to SkiaSharp.Views.Maui** — fix, confidence 0.55 (55%), cost/xl, validated=untested
   - Implement an SKSvgImageView control in SkiaSharp.Views.Maui that wraps SVG loading and rendering. Would address the gap for the modern MAUI platform.

**Recommended proposal:** Use Svg.Skia with SKCanvasView

**Why:** Immediate workaround with low effort; addresses the need today without requiring core library changes. The Xamarin.Forms platform is now legacy.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Valid feature request with community interest; no current implementation in SkiaSharp. Xamarin.Forms is legacy but the concept is relevant to MAUI. Keeping open allows tracking demand for a potential MAUI SVG view. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply feature-request, views, android and iOS labels | labels=type/feature-request, area/SkiaSharp.Views, os/Android, os/iOS, tenet/compatibility |
| add-comment | medium | 0.85 (85%) | Acknowledge request and share Svg.Skia workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the request! A dedicated SVG image control is not currently part of SkiaSharp's roadmap for the Xamarin.Forms layer.

In the meantime, the recommended workaround is to use the [Svg.Skia](https://www.nuget.org/packages/Svg.Skia) NuGet package together with `SKCanvasView`. You can load the SVG in the `PaintSurface` callback and draw it via `SKCanvas.DrawPicture`.

If you are migrating to .NET MAUI, the same approach works with `SkiaSharp.Views.Maui` and Svg.Skia.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1613,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-29T13:38:05Z"
  },
  "summary": "Reporter requests a dedicated SVG image control for Xamarin.Forms similar to FFImageLoading's SVG support, citing concern that FFImageLoading is unmaintained.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.85
    },
    "platforms": [
      "os/Android",
      "os/iOS"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Xamarin.Forms (unspecified version), no SkiaSharp version given",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1613#issuecomment-774110457",
          "description": "Maintainer (mattleibow) suggested Svg.Skia NuGet as an alternative"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/120",
          "description": "Older broader SVG support request (closed 2016)"
        }
      ]
    }
  },
  "analysis": {
    "summary": "Request for a first-party SVG image view control in the SkiaSharp.Views.Forms package, analogous to FFImageLoading's SvgCachedImage. No such control currently exists in SkiaSharp; SVG rendering is delegated to community packages such as Svg.Skia. The Xamarin.Forms platform is now legacy (superseded by MAUI).",
    "rationale": "The issue uses the [FEATURE] prefix and is phrased as a request ('Is there any chance to create...'), making it unambiguously a feature request. No bug is described. The Xamarin.Forms platform targeted by this request is now legacy; the modern equivalent is SkiaSharp.Views.Maui. The maintainer already pointed to Svg.Skia as a workaround, indicating the feature is out-of-scope for the core library.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKSVG.cs",
        "finding": "SKSvgCanvas provides SVG *output* (rendering to an SVG stream) but no SVG *input* (parsing/displaying SVG files as images). No SVG image view control exists in the SkiaSharp binding layer.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views",
        "finding": "The views layer contains platform-specific canvas views (SKCanvasView, SKGLView) for raster/GL rendering. No SVG view or image control is present. Xamarin.Forms-specific views are not in the current source tree (legacy Xamarin.Forms support was removed after MAUI migration).",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Is there any chance to create an SVG control that we can use in xamarin forms",
        "source": "issue body",
        "interpretation": "Explicit feature request — new SVG display control for Xamarin.Forms"
      },
      {
        "text": "I would suggesting looking at Svg.Skia: https://www.nuget.org/packages/Svg.Skia",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer acknowledged the gap and pointed to the Svg.Skia community package as the current solution, implying this is not planned for the SkiaSharp core library"
      }
    ],
    "workarounds": [
      "Use the Svg.Skia NuGet package (https://www.nuget.org/packages/Svg.Skia) together with SKCanvasView to render SVG files in Xamarin.Forms.",
      "For MAUI (the successor to Xamarin.Forms), use SkiaSharp.Views.Maui with a custom SKCanvasView that loads and renders SVG via Svg.Skia."
    ],
    "nextQuestions": [
      "Is there appetite to add an official SVG view control to SkiaSharp.Views.Maui (the MAUI successor)?",
      "Should SkiaSharp bundle SVG parsing capability (currently only SVG output is supported via SKSvgCanvas)?"
    ],
    "resolution": {
      "hypothesis": "The requested SVG control does not exist in SkiaSharp and is currently out-of-scope; the recommended path is Svg.Skia for Xamarin.Forms, or a custom SKCanvasView + Svg.Skia for MAUI.",
      "proposals": [
        {
          "title": "Use Svg.Skia with SKCanvasView",
          "description": "Add Svg.Skia NuGet package and render SVG in the PaintSurface handler of SKCanvasView. This works in both Xamarin.Forms and MAUI.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Add SVG image view to SkiaSharp.Views.Maui",
          "description": "Implement an SKSvgImageView control in SkiaSharp.Views.Maui that wraps SVG loading and rendering. Would address the gap for the modern MAUI platform.",
          "category": "fix",
          "confidence": 0.55,
          "effort": "cost/xl",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use Svg.Skia with SKCanvasView",
      "recommendedReason": "Immediate workaround with low effort; addresses the need today without requiring core library changes. The Xamarin.Forms platform is now legacy."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.8,
      "reason": "Valid feature request with community interest; no current implementation in SkiaSharp. Xamarin.Forms is legacy but the concept is relevant to MAUI. Keeping open allows tracking demand for a potential MAUI SVG view.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, views, android and iOS labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp.Views",
          "os/Android",
          "os/iOS",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge request and share Svg.Skia workaround",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the request! A dedicated SVG image control is not currently part of SkiaSharp's roadmap for the Xamarin.Forms layer.\n\nIn the meantime, the recommended workaround is to use the [Svg.Skia](https://www.nuget.org/packages/Svg.Skia) NuGet package together with `SKCanvasView`. You can load the SVG in the `PaintSurface` callback and draw it via `SKCanvas.DrawPicture`.\n\nIf you are migrating to .NET MAUI, the same approach works with `SkiaSharp.Views.Maui` and Svg.Skia."
      }
    ]
  }
}
```

</details>
