# Issue Triage Report — #732

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-23T21:04:18Z |
| Type | type/enhancement (0.95 (95%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | keep-open (0.85 (85%)) |

**Issue Summary:** Request to expose a ColorSpace property on SKCanvasView so that surfaces are created with a specified SKColorSpace rather than always defaulting to no colorspace, enabling Skia's color-corrected rendering pipeline.

**Analysis:** SKCanvasView creates its internal drawing surface with no SKColorSpace, bypassing Skia's color management pipeline. While SKImageInfo already supports a ColorSpace property and constructor overload, none of the platform SKCanvasView implementations expose it. The maintainer acknowledged the request and designed a proposed XAML-friendly API (a ColorSpace bindable property that triggers a surface refresh), but the feature has not been implemented.

**Recommendations:** **keep-open** — Valid, well-understood enhancement with maintainer buy-in. The implementation path is clear but requires coordinated changes across all platform views. No regression risk; currently open with no associated PR.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.Views |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/enhancement, area/SkiaSharp.Views, area/SkiaSharp.Views.Forms |

## Evidence

### Reproduction

1. Create an SKCanvasView in a Xamarin.Forms or MAUI app
2. Try to use color-corrected drawing with a specific color space
3. Observe that there is no API to set the colorspace on the canvas view surface

**Environment:** All platforms (Android, iOS, macOS) — Xamarin.Forms, no version info given. Filed Dec 2018.

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Code investigation confirms the issue still exists. SurfaceFactory.cs and SKCGSurfaceFactory.cs still create SKImageInfo without a ColorSpace parameter. MAUI views also have no ColorSpace property. |

## Analysis

### Technical Summary

SKCanvasView creates its internal drawing surface with no SKColorSpace, bypassing Skia's color management pipeline. While SKImageInfo already supports a ColorSpace property and constructor overload, none of the platform SKCanvasView implementations expose it. The maintainer acknowledged the request and designed a proposed XAML-friendly API (a ColorSpace bindable property that triggers a surface refresh), but the feature has not been implemented.

### Rationale

The issue clearly requests adding a ColorSpace property to SKCanvasView across all platforms. Code investigation confirms the gap: SurfaceFactory.CreateInfo and SKCGSurfaceFactory.CreateInfo both hardcode no colorspace. SKImageInfo already supports it (binding/SkiaSharp/SKImageInfo.cs). The maintainer validated the approach and provided a design sketch in comments. This is type/enhancement improving existing surface creation behavior.

### Key Signals

- "SKCanvasView always get created using an SKImageInfo that lacks colorspace specification" — **issue body** (Core gap: surface creation omits colorspace, disabling Skia color management.)
- "new SKImageInfo(0, 0, SKColorType.Rgba8888, SKAlphaType.Premul) — no colorspace at the end" — **comment by reporter** (Exact code path confirmed — the SurfaceFactory CreateInfo method does not pass ColorSpace.)
- "A property might work because I technically only guarantee the surface during the draw method. If you change the colorspace, all we have to do is nuke the surface and invalidate." — **maintainer comment (mattleibow)** (Maintainer validated that a bindable ColorSpace property is technically feasible.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SurfaceFactory.cs` | 89-90 | direct | CreateInfo() returns `new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul)` with no ColorSpace — the gap the reporter describes. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/SKCGSurfaceFactory.cs` | 113-114 | direct | Apple platforms share the same pattern: `new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul)` — no ColorSpace. |
| `binding/SkiaSharp/SKImageInfo.cs` | 95-101 | related | SKImageInfo has a full constructor accepting SKColorSpace and a WithColorSpace() helper — the building blocks to fix this are already in place. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SKCanvasView.cs` | — | related | MAUI SKCanvasView has no ColorSpace property or BindableProperty for it — the gap exists in MAUI views as well. |

### Workarounds

- As a workaround, users can post-process the drawing surface in the PaintSurface callback by manually converting colors using SKColorSpace.CreateSrgb() and SKPaint.ColorFilter with an SKColorFilter.CreateColorSpace() — but this is complex and not equivalent to setting colorspace at surface creation.
- For specific cases (e.g., linear sRGB), users can apply gamma correction manually via shader or paint filter in the PaintSurface handler.

### Next Questions

- Should this be a BindableProperty (for MAUI/Xamarin.Forms XAML) or just a regular property on the platform views?
- Should the API accept SKColorSpace directly (potentially not XAML-serializable) or a helper enum/class as the maintainer sketched?
- How should null/default behave — no colorspace (current), sRGB, or display P3?

### Resolution Proposals

**Hypothesis:** Add a ColorSpace property to SKCanvasView on all platforms that is used by SurfaceFactory.CreateInfo() and SKCGSurfaceFactory.CreateInfo() when constructing SKImageInfo.

1. **Add ColorSpace property to platform SKCanvasView implementations** — fix, confidence 0.80 (80%), cost/m, validated=untested
   - Expose a `SKColorSpace ColorSpace { get; set; }` property on Android SKCanvasView and Apple SKCanvasView platform views. When set, pass the colorspace to SKImageInfo in CreateInfo(). Setting it triggers Invalidate()/surface re-creation. For MAUI, add a BindableProperty.
2. **Use workaround in PaintSurface callback with color filter** — workaround, confidence 0.60 (60%), cost/s, validated=untested
   - Apply a color space conversion filter in the PaintSurface callback as a stop-gap until the property is implemented. Less clean but works today.

**Recommended proposal:** Add ColorSpace property to platform SKCanvasView implementations

**Why:** The maintainer already validated this approach. The binding layer (SKImageInfo) already supports it. Implementing a ColorSpace property on each platform view and wiring it through SurfaceFactory is a clean, maintainable solution.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.85 (85%) |
| Reason | Valid, well-understood enhancement with maintainer buy-in. The implementation path is clear but requires coordinated changes across all platform views. No regression risk; currently open with no associated PR. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Correct labels: keep type/enhancement, area/SkiaSharp.Views, add tenet/compatibility; remove stale area/SkiaSharp.Views.Forms (Xamarin.Forms is superseded by MAUI) | labels=type/enhancement, area/SkiaSharp.Views, tenet/compatibility |
| add-comment | medium | 0.80 (80%) | Summarize current state, confirm gap still exists, outline implementation approach | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for filing this. The gap is confirmed: `SurfaceFactory.CreateInfo()` (Android) and `SKCGSurfaceFactory.CreateInfo()` (Apple/iOS/macOS) both create `SKImageInfo` without a `ColorSpace`, so Skia's color management pipeline is never activated for canvas views.

The `SKImageInfo` binding already has the full constructor accepting `SKColorSpace` and a `WithColorSpace()` helper — the pieces are in place.

A `ColorSpace` property on each platform view (and a `BindableProperty` in MAUI) that is passed through to `CreateInfo()` would fix this. Setting it would nuke and re-create the surface on next paint. The MAUI migration also means the Xamarin.Forms-specific API design questions from 2018 are less of a concern today.

This is a valid enhancement; keeping it open for a future PR.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 732,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-23T21:04:18Z",
    "currentLabels": [
      "type/enhancement",
      "area/SkiaSharp.Views",
      "area/SkiaSharp.Views.Forms"
    ]
  },
  "summary": "Request to expose a ColorSpace property on SKCanvasView so that surfaces are created with a specified SKColorSpace rather than always defaulting to no colorspace, enabling Skia's color-corrected rendering pipeline.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.9
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKCanvasView in a Xamarin.Forms or MAUI app",
        "Try to use color-corrected drawing with a specific color space",
        "Observe that there is no API to set the colorspace on the canvas view surface"
      ],
      "environmentDetails": "All platforms (Android, iOS, macOS) — Xamarin.Forms, no version info given. Filed Dec 2018."
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "Code investigation confirms the issue still exists. SurfaceFactory.cs and SKCGSurfaceFactory.cs still create SKImageInfo without a ColorSpace parameter. MAUI views also have no ColorSpace property."
    }
  },
  "analysis": {
    "summary": "SKCanvasView creates its internal drawing surface with no SKColorSpace, bypassing Skia's color management pipeline. While SKImageInfo already supports a ColorSpace property and constructor overload, none of the platform SKCanvasView implementations expose it. The maintainer acknowledged the request and designed a proposed XAML-friendly API (a ColorSpace bindable property that triggers a surface refresh), but the feature has not been implemented.",
    "rationale": "The issue clearly requests adding a ColorSpace property to SKCanvasView across all platforms. Code investigation confirms the gap: SurfaceFactory.CreateInfo and SKCGSurfaceFactory.CreateInfo both hardcode no colorspace. SKImageInfo already supports it (binding/SkiaSharp/SKImageInfo.cs). The maintainer validated the approach and provided a design sketch in comments. This is type/enhancement improving existing surface creation behavior.",
    "keySignals": [
      {
        "text": "SKCanvasView always get created using an SKImageInfo that lacks colorspace specification",
        "source": "issue body",
        "interpretation": "Core gap: surface creation omits colorspace, disabling Skia color management."
      },
      {
        "text": "new SKImageInfo(0, 0, SKColorType.Rgba8888, SKAlphaType.Premul) — no colorspace at the end",
        "source": "comment by reporter",
        "interpretation": "Exact code path confirmed — the SurfaceFactory CreateInfo method does not pass ColorSpace."
      },
      {
        "text": "A property might work because I technically only guarantee the surface during the draw method. If you change the colorspace, all we have to do is nuke the surface and invalidate.",
        "source": "maintainer comment (mattleibow)",
        "interpretation": "Maintainer validated that a bindable ColorSpace property is technically feasible."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SurfaceFactory.cs",
        "lines": "89-90",
        "finding": "CreateInfo() returns `new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul)` with no ColorSpace — the gap the reporter describes.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/SKCGSurfaceFactory.cs",
        "lines": "113-114",
        "finding": "Apple platforms share the same pattern: `new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul)` — no ColorSpace.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImageInfo.cs",
        "lines": "95-101",
        "finding": "SKImageInfo has a full constructor accepting SKColorSpace and a WithColorSpace() helper — the building blocks to fix this are already in place.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SKCanvasView.cs",
        "finding": "MAUI SKCanvasView has no ColorSpace property or BindableProperty for it — the gap exists in MAUI views as well.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "As a workaround, users can post-process the drawing surface in the PaintSurface callback by manually converting colors using SKColorSpace.CreateSrgb() and SKPaint.ColorFilter with an SKColorFilter.CreateColorSpace() — but this is complex and not equivalent to setting colorspace at surface creation.",
      "For specific cases (e.g., linear sRGB), users can apply gamma correction manually via shader or paint filter in the PaintSurface handler."
    ],
    "nextQuestions": [
      "Should this be a BindableProperty (for MAUI/Xamarin.Forms XAML) or just a regular property on the platform views?",
      "Should the API accept SKColorSpace directly (potentially not XAML-serializable) or a helper enum/class as the maintainer sketched?",
      "How should null/default behave — no colorspace (current), sRGB, or display P3?"
    ],
    "resolution": {
      "hypothesis": "Add a ColorSpace property to SKCanvasView on all platforms that is used by SurfaceFactory.CreateInfo() and SKCGSurfaceFactory.CreateInfo() when constructing SKImageInfo.",
      "proposals": [
        {
          "title": "Add ColorSpace property to platform SKCanvasView implementations",
          "description": "Expose a `SKColorSpace ColorSpace { get; set; }` property on Android SKCanvasView and Apple SKCanvasView platform views. When set, pass the colorspace to SKImageInfo in CreateInfo(). Setting it triggers Invalidate()/surface re-creation. For MAUI, add a BindableProperty.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Use workaround in PaintSurface callback with color filter",
          "description": "Apply a color space conversion filter in the PaintSurface callback as a stop-gap until the property is implemented. Less clean but works today.",
          "category": "workaround",
          "confidence": 0.6,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add ColorSpace property to platform SKCanvasView implementations",
      "recommendedReason": "The maintainer already validated this approach. The binding layer (SKImageInfo) already supports it. Implementing a ColorSpace property on each platform view and wiring it through SurfaceFactory is a clean, maintainable solution."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.85,
      "reason": "Valid, well-understood enhancement with maintainer buy-in. The implementation path is clear but requires coordinated changes across all platform views. No regression risk; currently open with no associated PR.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct labels: keep type/enhancement, area/SkiaSharp.Views, add tenet/compatibility; remove stale area/SkiaSharp.Views.Forms (Xamarin.Forms is superseded by MAUI)",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp.Views",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Summarize current state, confirm gap still exists, outline implementation approach",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for filing this. The gap is confirmed: `SurfaceFactory.CreateInfo()` (Android) and `SKCGSurfaceFactory.CreateInfo()` (Apple/iOS/macOS) both create `SKImageInfo` without a `ColorSpace`, so Skia's color management pipeline is never activated for canvas views.\n\nThe `SKImageInfo` binding already has the full constructor accepting `SKColorSpace` and a `WithColorSpace()` helper — the pieces are in place.\n\nA `ColorSpace` property on each platform view (and a `BindableProperty` in MAUI) that is passed through to `CreateInfo()` would fix this. Setting it would nuke and re-create the surface on next paint. The MAUI migration also means the Xamarin.Forms-specific API design questions from 2018 are less of a concern today.\n\nThis is a valid enhancement; keeping it open for a future PR."
      }
    ]
  }
}
```

</details>
