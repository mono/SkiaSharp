# Issue Triage Report — #2663

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T16:12:06Z |
| Type | type/feature-request (0.88 (88%)) |
| Area | area/SkiaSharp (0.93 (93%)) |
| Suggested action | close-as-fixed (0.88 (88%)) |

**Issue Summary:** Reporter requests that SKSamplingOptions and SKFilterMode be exposed so that image shaders passed as SKSL child effects use linear interpolation rather than nearest-pixel sampling; the API was missing in SkiaSharp 2.88.x and has since been added in SkiaSharp 3.x.

**Analysis:** In SkiaSharp 2.88.x, SKFilterMode and SKSamplingOptions were not exposed, so image shaders used as SKSL child effects always defaulted to nearest-pixel sampling. SkiaSharp 3.x adds SKFilterMode.Linear, SKSamplingOptions(SKFilterMode), and SKShader.CreateImage / SKImage.ToShader overloads that accept SKSamplingOptions, directly addressing this request.

**Recommendations:** **close-as-fixed** — SKFilterMode, SKSamplingOptions, and CreateImage/ToShader overloads with sampling options are all present in SkiaSharp 3.x, directly fulfilling the reporter's request.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a SkiaSharp RuntimeEffect (SKSL) with a child image shader
2. Pass an SKImage as a child effect using the 2.88.x API (no sampling options available)
3. Observe that sample() in the SKSL always uses nearest-pixel mode, producing blocky output

**Environment:** SkiaSharp 2.88.3, Visual Studio (Windows); also tested on 3.0.0-preview2.1 (March 2024)

**Screenshots:**
- https://github.com/mono/SkiaSharp/assets/10914515/83dddc9d-380c-4214-9e81-a48ee146e997 — Blocky relief shading without linear interpolation
- https://github.com/mono/SkiaSharp/assets/10914515/a45cfd28-1d98-4e11-9d83-30986b4ea50b — Smooth relief shading with interpolation (OpenGL reference)
- https://github.com/mono/SkiaSharp/assets/10914515/3b8146a6-f6d8-4ac3-8866-b77c44ba4678 — Image blit using nearest-pixel sampling (blocky)
- https://github.com/mono/SkiaSharp/assets/10914515/50039075-7bf9-448c-9c70-c9b85c719cec — Desired image blit result with linear sampling

**Code snippets:**

```csharp
float3 elem = sample(elev_map, cf).xyz; // desired: single call with linear filter
```

```csharp
// workaround in 2.88.x: manual bilinear interpolation using 4 nearest-pixel samples
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3, 3.0.0-preview2.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | SKSamplingOptions and SKFilterMode.Linear are now fully exposed in SkiaSharp 3.x; SKShader.CreateImage and SKImage.ToShader both have overloads accepting SKSamplingOptions. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.92 (92%) |
| Reason | The requested types (SKFilterMode, SKSamplingOptions) and the CreateImage/ToShader overloads with sampling options are present in the current SkiaSharp 3.x codebase. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | 3.x |

## Analysis

### Technical Summary

In SkiaSharp 2.88.x, SKFilterMode and SKSamplingOptions were not exposed, so image shaders used as SKSL child effects always defaulted to nearest-pixel sampling. SkiaSharp 3.x adds SKFilterMode.Linear, SKSamplingOptions(SKFilterMode), and SKShader.CreateImage / SKImage.ToShader overloads that accept SKSamplingOptions, directly addressing this request.

### Rationale

The issue title and body explicitly ask for SKFilterMode/SKSamplingOptions to be exposed; these types are now present. The current code investigation shows SKShader.CreateImage(src, tmx, tmy, new SKSamplingOptions(SKFilterMode.Linear)) is valid in 3.x. The reporter confirmed they were still missing in preview2.1 but the final release introduced them.

### Key Signals

- "SkiaSharp 2.88.xx, doesn't expose 'SKFilterMode' nor 'SKSamplingOptions' types/members." — **issue body** (Missing API in 2.88.x — feature request, not a regression.)
- "Is SkiaSharp 3.0, going to expose this functionality?" — **issue body** (Reporter explicitly frames this as a question about future API availability — classic feature request.)
- "I am now testing out Skia 3.0.0-preview2.1 - and am still not seeing an option to set Sample Mode to LINEAR." — **comment #2** (Reporter confirmed the gap in preview2.1; current 3.x release has the API.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaApi.generated.cs` | 20835-20840 | direct | SKFilterMode enum is defined with Nearest=0 and Linear=1 — the exact values the reporter needed. |
| `binding/SkiaSharp/Definitions.cs` | 627-637 | direct | SKSamplingOptions struct has constructors accepting SKFilterMode and SKFilterMode+SKMipmapMode, matching the Skia C++ API the reporter referenced. |
| `binding/SkiaSharp/SKShader.cs` | — | direct | SKShader.CreateImage(SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling) overload is present — reporter can now create an image shader with linear sampling for use as an SKSL child effect. |
| `binding/SkiaSharp/SKImage.cs` | — | direct | SKImage.ToShader(SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling) overload is present, confirming the C++ pattern image->makeShader(SkSamplingOptions(SkFilterMode::kLinear)) is now representable in C#. |

### Workarounds

- In SkiaSharp 3.x: use SKShader.CreateImage(image, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, new SKSamplingOptions(SKFilterMode.Linear)) to create an image shader with linear interpolation for SKSL child effects.
- In SkiaSharp 2.88.x (no fix): manually implement bilinear interpolation in the SKSL shader by sampling 4 neighbouring pixels and lerping (as shown in the issue body).

### Resolution Proposals

**Hypothesis:** The requested SKFilterMode and SKSamplingOptions APIs are fully available in SkiaSharp 3.x, resolving the missing-API gap reported here.

1. **Close as fixed — API available in SkiaSharp 3.x** — fix, confidence 0.90 (90%), cost/xs, validated=yes
   - Confirm that SKFilterMode.Linear, SKSamplingOptions, and the CreateImage/ToShader overloads are present and direct the reporter to upgrade to SkiaSharp 3.x.

**Recommended proposal:** Close as fixed — API available in SkiaSharp 3.x

**Why:** Code investigation confirms all four relevant types and overloads are present in the current 3.x codebase.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.88 (88%) |
| Reason | SKFilterMode, SKSamplingOptions, and CreateImage/ToShader overloads with sampling options are all present in SkiaSharp 3.x, directly fulfilling the reporter's request. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Correct the type label from type/bug to type/feature-request and add area/SkiaSharp and platform label | labels=type/feature-request, area/SkiaSharp, os/Windows-Classic |
| add-comment | medium | 0.88 (88%) | Inform reporter that SKSamplingOptions and SKFilterMode.Linear are available in SkiaSharp 3.x | — |
| close-issue | medium | 0.85 (85%) | Close as completed — feature is available in SkiaSharp 3.x | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Good news — the API you need is available in **SkiaSharp 3.x**.

Both `SKFilterMode` and `SKSamplingOptions` are now exposed. To create an image shader with linear sampling for use as an SKSL child effect, you can do:

```csharp
var imageShader = SKShader.CreateImage(
    image,
    SKShaderTileMode.Clamp,
    SKShaderTileMode.Clamp,
    new SKSamplingOptions(SKFilterMode.Linear)
);
```

Or equivalently via `SKImage`:

```csharp
var imageShader = image.ToShader(
    SKShaderTileMode.Clamp,
    SKShaderTileMode.Clamp,
    new SKSamplingOptions(SKFilterMode.Linear)
);
```

This matches the Skia C++ pattern `image->makeShader(SkSamplingOptions(SkFilterMode::kLinear))` you referenced. When this shader is passed as a child effect to your `SKRuntimeEffect`, SKSL's `sample()` will use linear interpolation, eliminating the need for the manual 4-sample bilinear lerp workaround.

Please upgrade to SkiaSharp 3.x if you haven't already. If you still see blocky output after using `SKFilterMode.Linear`, please re-open with a minimal repro.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2663,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T16:12:06Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter requests that SKSamplingOptions and SKFilterMode be exposed so that image shaders passed as SKSL child effects use linear interpolation rather than nearest-pixel sampling; the API was missing in SkiaSharp 2.88.x and has since been added in SkiaSharp 3.x.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.93
    },
    "platforms": [
      "os/Windows-Classic"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a SkiaSharp RuntimeEffect (SKSL) with a child image shader",
        "Pass an SKImage as a child effect using the 2.88.x API (no sampling options available)",
        "Observe that sample() in the SKSL always uses nearest-pixel mode, producing blocky output"
      ],
      "codeSnippets": [
        "float3 elem = sample(elev_map, cf).xyz; // desired: single call with linear filter",
        "// workaround in 2.88.x: manual bilinear interpolation using 4 nearest-pixel samples"
      ],
      "screenshots": [
        {
          "url": "https://github.com/mono/SkiaSharp/assets/10914515/83dddc9d-380c-4214-9e81-a48ee146e997",
          "description": "Blocky relief shading without linear interpolation"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/assets/10914515/a45cfd28-1d98-4e11-9d83-30986b4ea50b",
          "description": "Smooth relief shading with interpolation (OpenGL reference)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/assets/10914515/3b8146a6-f6d8-4ac3-8866-b77c44ba4678",
          "description": "Image blit using nearest-pixel sampling (blocky)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/assets/10914515/50039075-7bf9-448c-9c70-c9b85c719cec",
          "description": "Desired image blit result with linear sampling"
        }
      ],
      "environmentDetails": "SkiaSharp 2.88.3, Visual Studio (Windows); also tested on 3.0.0-preview2.1 (March 2024)"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3",
        "3.0.0-preview2.1"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "SKSamplingOptions and SKFilterMode.Linear are now fully exposed in SkiaSharp 3.x; SKShader.CreateImage and SKImage.ToShader both have overloads accepting SKSamplingOptions."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.92,
      "reason": "The requested types (SKFilterMode, SKSamplingOptions) and the CreateImage/ToShader overloads with sampling options are present in the current SkiaSharp 3.x codebase.",
      "fixedInVersion": "3.x"
    }
  },
  "analysis": {
    "summary": "In SkiaSharp 2.88.x, SKFilterMode and SKSamplingOptions were not exposed, so image shaders used as SKSL child effects always defaulted to nearest-pixel sampling. SkiaSharp 3.x adds SKFilterMode.Linear, SKSamplingOptions(SKFilterMode), and SKShader.CreateImage / SKImage.ToShader overloads that accept SKSamplingOptions, directly addressing this request.",
    "rationale": "The issue title and body explicitly ask for SKFilterMode/SKSamplingOptions to be exposed; these types are now present. The current code investigation shows SKShader.CreateImage(src, tmx, tmy, new SKSamplingOptions(SKFilterMode.Linear)) is valid in 3.x. The reporter confirmed they were still missing in preview2.1 but the final release introduced them.",
    "keySignals": [
      {
        "text": "SkiaSharp 2.88.xx, doesn't expose 'SKFilterMode' nor 'SKSamplingOptions' types/members.",
        "source": "issue body",
        "interpretation": "Missing API in 2.88.x — feature request, not a regression."
      },
      {
        "text": "Is SkiaSharp 3.0, going to expose this functionality?",
        "source": "issue body",
        "interpretation": "Reporter explicitly frames this as a question about future API availability — classic feature request."
      },
      {
        "text": "I am now testing out Skia 3.0.0-preview2.1 - and am still not seeing an option to set Sample Mode to LINEAR.",
        "source": "comment #2",
        "interpretation": "Reporter confirmed the gap in preview2.1; current 3.x release has the API."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "20835-20840",
        "finding": "SKFilterMode enum is defined with Nearest=0 and Linear=1 — the exact values the reporter needed.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "627-637",
        "finding": "SKSamplingOptions struct has constructors accepting SKFilterMode and SKFilterMode+SKMipmapMode, matching the Skia C++ API the reporter referenced.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKShader.cs",
        "finding": "SKShader.CreateImage(SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling) overload is present — reporter can now create an image shader with linear sampling for use as an SKSL child effect.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "finding": "SKImage.ToShader(SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling) overload is present, confirming the C++ pattern image->makeShader(SkSamplingOptions(SkFilterMode::kLinear)) is now representable in C#.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "In SkiaSharp 3.x: use SKShader.CreateImage(image, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, new SKSamplingOptions(SKFilterMode.Linear)) to create an image shader with linear interpolation for SKSL child effects.",
      "In SkiaSharp 2.88.x (no fix): manually implement bilinear interpolation in the SKSL shader by sampling 4 neighbouring pixels and lerping (as shown in the issue body)."
    ],
    "resolution": {
      "hypothesis": "The requested SKFilterMode and SKSamplingOptions APIs are fully available in SkiaSharp 3.x, resolving the missing-API gap reported here.",
      "proposals": [
        {
          "title": "Close as fixed — API available in SkiaSharp 3.x",
          "description": "Confirm that SKFilterMode.Linear, SKSamplingOptions, and the CreateImage/ToShader overloads are present and direct the reporter to upgrade to SkiaSharp 3.x.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Close as fixed — API available in SkiaSharp 3.x",
      "recommendedReason": "Code investigation confirms all four relevant types and overloads are present in the current 3.x codebase."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.88,
      "reason": "SKFilterMode, SKSamplingOptions, and CreateImage/ToShader overloads with sampling options are all present in SkiaSharp 3.x, directly fulfilling the reporter's request.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct the type label from type/bug to type/feature-request and add area/SkiaSharp and platform label",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "os/Windows-Classic"
        ]
      },
      {
        "type": "add-comment",
        "description": "Inform reporter that SKSamplingOptions and SKFilterMode.Linear are available in SkiaSharp 3.x",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Good news — the API you need is available in **SkiaSharp 3.x**.\n\nBoth `SKFilterMode` and `SKSamplingOptions` are now exposed. To create an image shader with linear sampling for use as an SKSL child effect, you can do:\n\n```csharp\nvar imageShader = SKShader.CreateImage(\n    image,\n    SKShaderTileMode.Clamp,\n    SKShaderTileMode.Clamp,\n    new SKSamplingOptions(SKFilterMode.Linear)\n);\n```\n\nOr equivalently via `SKImage`:\n\n```csharp\nvar imageShader = image.ToShader(\n    SKShaderTileMode.Clamp,\n    SKShaderTileMode.Clamp,\n    new SKSamplingOptions(SKFilterMode.Linear)\n);\n```\n\nThis matches the Skia C++ pattern `image->makeShader(SkSamplingOptions(SkFilterMode::kLinear))` you referenced. When this shader is passed as a child effect to your `SKRuntimeEffect`, SKSL's `sample()` will use linear interpolation, eliminating the need for the manual 4-sample bilinear lerp workaround.\n\nPlease upgrade to SkiaSharp 3.x if you haven't already. If you still see blocky output after using `SKFilterMode.Linear`, please re-open with a minimal repro."
      },
      {
        "type": "close-issue",
        "description": "Close as completed — feature is available in SkiaSharp 3.x",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
