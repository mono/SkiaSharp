# Issue Triage Report — #1845

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-29T00:02:24Z |
| Type | type/question (0.90 (90%)) |
| Area | area/SkiaSharp.Views.Forms (0.82 (82%)) |
| Suggested action | close-as-external (0.85 (85%)) |

**Issue Summary:** Reporter asks why SVG icons stop rendering after upgrading Android target SDK from 29 to 30; the SVG rendering uses the deprecated SkiaSharp.Extended.Svg package from a separate repository, not core SkiaSharp.

**Analysis:** The reporter's SVG icons stopped rendering after upgrading to Android API 30. The SVG parsing is handled by SkiaSharp.Extended.Svg.SKSvg, which lives in the separate mono/SkiaSharp.Extended repository and has not been maintained for years. Core SkiaSharp's Android SKCanvasView surface pipeline is unrelated to this failure. The recommended fix is to migrate to the Svg.Skia package.

**Recommendations:** **close-as-external** — SVG rendering failure is in SkiaSharp.Extended.Svg (mono/SkiaSharp.Extended), a separate deprecated package. Core SkiaSharp's Android surface pipeline is not involved. Reporter should migrate to Svg.Skia.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp.Views.Forms |
| Platforms | os/Android |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Environment:** Xamarin.Forms app on Android 11 (API 30); upgraded targetSdkVersion from 29 to 30; uses SkiaSharp.Extended.Svg.SKSvg for SVG loading and DrawPicture for rendering. No SkiaSharp version number given.

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | No SkiaSharp version specified; the SVG package is from SkiaSharp.Extended (separate repo, not this one). |

## Analysis

### Technical Summary

The reporter's SVG icons stopped rendering after upgrading to Android API 30. The SVG parsing is handled by SkiaSharp.Extended.Svg.SKSvg, which lives in the separate mono/SkiaSharp.Extended repository and has not been maintained for years. Core SkiaSharp's Android SKCanvasView surface pipeline is unrelated to this failure. The recommended fix is to migrate to the Svg.Skia package.

### Rationale

The issue title explicitly says [QUESTION] and asks for guidance, not reporting a reproducible crash. The SVG functionality used (SkiaSharp.Extended.Svg.SKSvg) is from a separate NuGet package not in this repository, so any API 30 compatibility issue there is outside SkiaSharp's scope. The community comment from charlesroddie correctly identifies that SkiaSharp.Extended.Svg is deprecated and Svg.Skia is the recommended replacement.

### Key Signals

- "[QUESTION] SVG not showing up after upgrade to Android 11 / API 30" — **issue title** (Reporter frames this as a usage question, not a crash or regression bug.)
- "SkiaSharp.Extended.Svg.SKSvg" — **issue body** (Reporter uses SkiaSharp.Extended.Svg from mono/SkiaSharp.Extended (a separate, unmaintained repo) for SVG parsing — not part of core SkiaSharp.)
- "Use Svg.Skia instead. The internal svg stuff here hasn't been updated for a very long time and the recommendation for several years has been to move to Svg.Skia for anything serious." — **comment #1557064216 by charlesroddie** (Community member correctly identifies that SkiaSharp.Extended.Svg is deprecated and Svg.Skia is the recommended active replacement.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs` | 77-117 | context | Android SKCanvasView.OnDraw() creates a surface via SurfaceFactory, calls OnPaintSurface(), and blits via DrawSurface(). No Android API 30-specific code changes. The surface pipeline is intact and unrelated to SVG parsing. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs` | 134-139 | context | OnDetachedFromWindow disposes the SurfaceFactory; no SKSvg or SkiaSharp.Extended references anywhere in this file — confirms SVG rendering is entirely in the external SkiaSharp.Extended.Svg package. |

### Workarounds

- Replace SkiaSharp.Extended.Svg NuGet package with Svg.Skia (https://github.com/wieslawsoltes/Svg.Skia) and update SVG loading code to use Svg.Skia's SvgSource or SKSvg API.

### Resolution Proposals

**Hypothesis:** The SkiaSharp.Extended.Svg package is unmaintained and may have compatibility issues with Android API 30. The root cause is outside core SkiaSharp. The fix is to migrate to the actively maintained Svg.Skia package.

1. **Migrate to Svg.Skia** — alternative, confidence 0.90 (90%), cost/s, validated=untested
   - Replace the SkiaSharp.Extended.Svg NuGet package with Svg.Skia (NuGet: Svg.Skia). Svg.Skia is actively maintained, supports Xamarin/MAUI, and provides similar DrawPicture-based rendering with SkiaSharp.

**Recommended proposal:** Migrate to Svg.Skia

**Why:** SkiaSharp.Extended.Svg is unmaintained; Svg.Skia is the community-endorsed replacement already recommended in the issue thread.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.85 (85%) |
| Reason | SVG rendering failure is in SkiaSharp.Extended.Svg (mono/SkiaSharp.Extended), a separate deprecated package. Core SkiaSharp's Android surface pipeline is not involved. Reporter should migrate to Svg.Skia. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply question, views-forms, and android labels | labels=type/question, area/SkiaSharp.Views.Forms, os/Android |
| add-comment | high | 0.85 (85%) | Post guidance explaining SkiaSharp.Extended.Svg is deprecated and recommend Svg.Skia | — |
| close-issue | medium | 0.80 (80%) | Close as not planned — SVG issue is in the external SkiaSharp.Extended package | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for reaching out! The SVG rendering in your code uses `SkiaSharp.Extended.Svg.SKSvg`, which comes from the separate [`SkiaSharp.Extended`](https://github.com/mono/SkiaSharp.Extended) package. That package has not been actively maintained for several years, so compatibility with newer Android API levels (API 30+) is not something that can be addressed in core SkiaSharp.

As already suggested in the comments, the recommended solution is to migrate to the actively maintained [**Svg.Skia**](https://github.com/wieslawsoltes/Svg.Skia) NuGet package (`Svg.Skia`). It provides a similar API for loading and rendering SVG files with SkiaSharp and is supported on Xamarin.Forms and MAUI.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1845,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-29T00:02:24Z"
  },
  "summary": "Reporter asks why SVG icons stop rendering after upgrading Android target SDK from 29 to 30; the SVG rendering uses the deprecated SkiaSharp.Extended.Svg package from a separate repository, not core SkiaSharp.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp.Views.Forms",
      "confidence": 0.82
    },
    "platforms": [
      "os/Android"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Xamarin.Forms app on Android 11 (API 30); upgraded targetSdkVersion from 29 to 30; uses SkiaSharp.Extended.Svg.SKSvg for SVG loading and DrawPicture for rendering. No SkiaSharp version number given."
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "unknown",
      "relevanceReason": "No SkiaSharp version specified; the SVG package is from SkiaSharp.Extended (separate repo, not this one)."
    }
  },
  "analysis": {
    "summary": "The reporter's SVG icons stopped rendering after upgrading to Android API 30. The SVG parsing is handled by SkiaSharp.Extended.Svg.SKSvg, which lives in the separate mono/SkiaSharp.Extended repository and has not been maintained for years. Core SkiaSharp's Android SKCanvasView surface pipeline is unrelated to this failure. The recommended fix is to migrate to the Svg.Skia package.",
    "rationale": "The issue title explicitly says [QUESTION] and asks for guidance, not reporting a reproducible crash. The SVG functionality used (SkiaSharp.Extended.Svg.SKSvg) is from a separate NuGet package not in this repository, so any API 30 compatibility issue there is outside SkiaSharp's scope. The community comment from charlesroddie correctly identifies that SkiaSharp.Extended.Svg is deprecated and Svg.Skia is the recommended replacement.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs",
        "lines": "77-117",
        "finding": "Android SKCanvasView.OnDraw() creates a surface via SurfaceFactory, calls OnPaintSurface(), and blits via DrawSurface(). No Android API 30-specific code changes. The surface pipeline is intact and unrelated to SVG parsing.",
        "relevance": "context"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs",
        "lines": "134-139",
        "finding": "OnDetachedFromWindow disposes the SurfaceFactory; no SKSvg or SkiaSharp.Extended references anywhere in this file — confirms SVG rendering is entirely in the external SkiaSharp.Extended.Svg package.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "[QUESTION] SVG not showing up after upgrade to Android 11 / API 30",
        "source": "issue title",
        "interpretation": "Reporter frames this as a usage question, not a crash or regression bug."
      },
      {
        "text": "SkiaSharp.Extended.Svg.SKSvg",
        "source": "issue body",
        "interpretation": "Reporter uses SkiaSharp.Extended.Svg from mono/SkiaSharp.Extended (a separate, unmaintained repo) for SVG parsing — not part of core SkiaSharp."
      },
      {
        "text": "Use Svg.Skia instead. The internal svg stuff here hasn't been updated for a very long time and the recommendation for several years has been to move to Svg.Skia for anything serious.",
        "source": "comment #1557064216 by charlesroddie",
        "interpretation": "Community member correctly identifies that SkiaSharp.Extended.Svg is deprecated and Svg.Skia is the recommended active replacement."
      }
    ],
    "workarounds": [
      "Replace SkiaSharp.Extended.Svg NuGet package with Svg.Skia (https://github.com/wieslawsoltes/Svg.Skia) and update SVG loading code to use Svg.Skia's SvgSource or SKSvg API."
    ],
    "resolution": {
      "hypothesis": "The SkiaSharp.Extended.Svg package is unmaintained and may have compatibility issues with Android API 30. The root cause is outside core SkiaSharp. The fix is to migrate to the actively maintained Svg.Skia package.",
      "proposals": [
        {
          "title": "Migrate to Svg.Skia",
          "description": "Replace the SkiaSharp.Extended.Svg NuGet package with Svg.Skia (NuGet: Svg.Skia). Svg.Skia is actively maintained, supports Xamarin/MAUI, and provides similar DrawPicture-based rendering with SkiaSharp.",
          "category": "alternative",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Migrate to Svg.Skia",
      "recommendedReason": "SkiaSharp.Extended.Svg is unmaintained; Svg.Skia is the community-endorsed replacement already recommended in the issue thread."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.85,
      "reason": "SVG rendering failure is in SkiaSharp.Extended.Svg (mono/SkiaSharp.Extended), a separate deprecated package. Core SkiaSharp's Android surface pipeline is not involved. Reporter should migrate to Svg.Skia.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question, views-forms, and android labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp.Views.Forms",
          "os/Android"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post guidance explaining SkiaSharp.Extended.Svg is deprecated and recommend Svg.Skia",
        "risk": "high",
        "confidence": 0.85,
        "comment": "Thanks for reaching out! The SVG rendering in your code uses `SkiaSharp.Extended.Svg.SKSvg`, which comes from the separate [`SkiaSharp.Extended`](https://github.com/mono/SkiaSharp.Extended) package. That package has not been actively maintained for several years, so compatibility with newer Android API levels (API 30+) is not something that can be addressed in core SkiaSharp.\n\nAs already suggested in the comments, the recommended solution is to migrate to the actively maintained [**Svg.Skia**](https://github.com/wieslawsoltes/Svg.Skia) NuGet package (`Svg.Skia`). It provides a similar API for loading and rendering SVG files with SkiaSharp and is supported on Xamarin.Forms and MAUI."
      },
      {
        "type": "close-issue",
        "description": "Close as not planned — SVG issue is in the external SkiaSharp.Extended package",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
