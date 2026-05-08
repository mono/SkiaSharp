# Issue Triage Report — #2395

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T14:27:49Z |
| Type | type/question (0.88 (88%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-not-a-bug (0.82 (82%)) |

**Issue Summary:** Reporter asks whether SKImageFilter.CreateErode supports a circular or custom-shaped structuring element instead of the rectangular erosion that the two-radius API currently provides.

**Analysis:** The reporter wants morphological erosion with a disc (circular) structuring element, comparable to MATLAB's strel('disk',...). Skia's built-in erode filter (SkMorphologyImageFilter) only applies axis-aligned rectangular min-filter over (radiusX, radiusY) extents — there is no circular or custom structuring element option exposed in either the C++ API or the SkiaSharp binding. This is a known limitation of Skia's morphology filter. A workaround exists by approximating a disc via repeated passes or using a shader-based approach.

**Recommendations:** **close-as-not-a-bug** — This is a usage question. The current behavior is by design. Circular structuring elements are not supported by Skia's morphology filter. A workaround via SKRuntimeEffect exists for advanced users.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Environment:** No platform, framework version, or SkiaSharp version specified.

**Repository links:**
- https://user-images.githubusercontent.com/5293502/220184021-9a28c850-32db-40be-aa9a-b9a76c3298c3.png — Screenshot showing sharp (non-circular) erosion result

**Code snippets:**

```csharp
using var paint = new SKPaint { ColorFilter = SKColorFilter.CreateTable(table), ImageFilter = SKImageFilter.CreateErode(dilationRadius, dilationRadius) };
var result = new SKBitmap(img.Info.Width, img.Info.Height, SKColorType.Bgra8888, SKAlphaType.Opaque);
using var canvas = new SKCanvas(result);
canvas.DrawBitmap(img, 0, 0, paint);
return result;
```

## Analysis

### Technical Summary

The reporter wants morphological erosion with a disc (circular) structuring element, comparable to MATLAB's strel('disk',...). Skia's built-in erode filter (SkMorphologyImageFilter) only applies axis-aligned rectangular min-filter over (radiusX, radiusY) extents — there is no circular or custom structuring element option exposed in either the C++ API or the SkiaSharp binding. This is a known limitation of Skia's morphology filter. A workaround exists by approximating a disc via repeated passes or using a shader-based approach.

### Rationale

The title is [QUESTION] and the body ends with 'Is there a way to do this?'. No broken behavior is reported — the erode filter works as designed (rectangular). The question is whether a circular structuring element is available. Code investigation confirms only radiusX/radiusY are accepted and map directly to Skia's rectangular morphology filter. Classification as type/question is appropriate; the implicit feature gap (no disc structuring element) could be a follow-on feature-request.

### Key Signals

- "But the resulting image looks like it was eroded with a square." — **issue body** (Reporter correctly identifies that CreateErode uses a rectangular structuring element.)
- "Is there a way to do this?" — **issue body** (This is a usage question, not a bug report.)
- "CreateErode only accepts 2 params for what I guess are the 2 dimensions of a rect." — **issue body** (Reporter has correctly deduced the API limitation.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKImageFilter.cs` | — | direct | CreateErode(float radiusX, float radiusY, ...) delegates to sk_imagefilter_new_erode with only axis-aligned radius parameters. No overload or parameter exists for a custom or circular structuring element. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | — | direct | The C API sk_imagefilter_new_erode signature is (float radiusX, float radiusY, sk_imagefilter_t* input, sk_rect_t* cropRect). Skia's underlying SkMorphologyImageFilter only supports rectangular structuring elements — no circular shape option exists in the upstream C++ API. |

### Workarounds

- Approximate a circular disc by chaining multiple small erode passes at different angles (not pixel-perfect but visually close for large radii).
- Use a custom SKShader with GLSL-style per-pixel sampling to implement disc erosion at the shader level via SKRuntimeEffect.
- For binary images specifically, pre-process with a soft circular mask convolution, then threshold.

### Resolution Proposals

**Hypothesis:** No circular structuring element is available in Skia's morphology filter. The user needs a workaround or would need to request it as a feature in upstream Skia.

1. **Explain Skia limitation and suggest SKRuntimeEffect workaround** — workaround, confidence 0.80 (80%), cost/m, validated=untested
   - Explain that Skia's erode/dilate filters use rectangular structuring elements by design, and the limitation is in the upstream library. For disc erosion on binary images, a custom SKRuntimeEffect (runtime shader) can sample neighbors in a circle.
2. **Suggest chained approach** — alternative, confidence 0.65 (65%), cost/s, validated=untested
   - Apply multiple small erosions with equal radiusX and radiusY (i.e., square passes) and combine with a blur+threshold step to approximate circular morphology.

**Recommended proposal:** Explain Skia limitation and suggest SKRuntimeEffect workaround

**Why:** Honest explanation of the upstream constraint, with an actionable workaround that can achieve the desired result.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.82 (82%) |
| Reason | This is a usage question. The current behavior is by design. Circular structuring elements are not supported by Skia's morphology filter. A workaround via SKRuntimeEffect exists for advanced users. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply question and core SkiaSharp area labels | labels=type/question, area/SkiaSharp |
| add-comment | high | 0.82 (82%) | Explain the rectangular limitation of CreateErode and provide workaround direction | — |
| close-issue | medium | 0.78 (78%) | Close as answered — behavior is by design in upstream Skia | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed question!

`SKImageFilter.CreateErode` wraps Skia's `SkMorphologyImageFilter`, which only supports **rectangular** structuring elements defined by `(radiusX, radiusY)`. There is no built-in support for circular, disc, or custom-shaped structuring elements — this is a limitation of the underlying Skia engine.

**Possible workarounds:**

1. **SKRuntimeEffect (custom shader):** For a true disc erosion on binary images, you can write a custom runtime shader that samples pixels in a circular neighborhood. This is the most accurate approach but requires writing GLSL-style SkSL code.

2. **Approximate with equal radii:** Using `CreateErode(r, r)` with equal radii gives a square, not a circle. The visual difference is most noticeable at corners, as you observed. For many use cases the square approximation is acceptable.

3. **Multi-pass approximation:** Applying a series of 1-pixel erosions with `CreateErode(1, 1)` and combining results is still rectangular, but applying the filter in both passes with different crop rects does not produce a true disc either.

If disc morphology is important for your use case, you may want to open a feature request against the upstream [Skia project](https://bugs.chromium.org/p/skia) to add circular structuring element support to `SkMorphologyImageFilter`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2395,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T14:27:49Z"
  },
  "summary": "Reporter asks whether SKImageFilter.CreateErode supports a circular or custom-shaped structuring element instead of the rectangular erosion that the two-radius API currently provides.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    }
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "using var paint = new SKPaint { ColorFilter = SKColorFilter.CreateTable(table), ImageFilter = SKImageFilter.CreateErode(dilationRadius, dilationRadius) };\nvar result = new SKBitmap(img.Info.Width, img.Info.Height, SKColorType.Bgra8888, SKAlphaType.Opaque);\nusing var canvas = new SKCanvas(result);\ncanvas.DrawBitmap(img, 0, 0, paint);\nreturn result;"
      ],
      "environmentDetails": "No platform, framework version, or SkiaSharp version specified.",
      "repoLinks": [
        {
          "url": "https://user-images.githubusercontent.com/5293502/220184021-9a28c850-32db-40be-aa9a-b9a76c3298c3.png",
          "description": "Screenshot showing sharp (non-circular) erosion result"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The reporter wants morphological erosion with a disc (circular) structuring element, comparable to MATLAB's strel('disk',...). Skia's built-in erode filter (SkMorphologyImageFilter) only applies axis-aligned rectangular min-filter over (radiusX, radiusY) extents — there is no circular or custom structuring element option exposed in either the C++ API or the SkiaSharp binding. This is a known limitation of Skia's morphology filter. A workaround exists by approximating a disc via repeated passes or using a shader-based approach.",
    "rationale": "The title is [QUESTION] and the body ends with 'Is there a way to do this?'. No broken behavior is reported — the erode filter works as designed (rectangular). The question is whether a circular structuring element is available. Code investigation confirms only radiusX/radiusY are accepted and map directly to Skia's rectangular morphology filter. Classification as type/question is appropriate; the implicit feature gap (no disc structuring element) could be a follow-on feature-request.",
    "keySignals": [
      {
        "text": "But the resulting image looks like it was eroded with a square.",
        "source": "issue body",
        "interpretation": "Reporter correctly identifies that CreateErode uses a rectangular structuring element."
      },
      {
        "text": "Is there a way to do this?",
        "source": "issue body",
        "interpretation": "This is a usage question, not a bug report."
      },
      {
        "text": "CreateErode only accepts 2 params for what I guess are the 2 dimensions of a rect.",
        "source": "issue body",
        "interpretation": "Reporter has correctly deduced the API limitation."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKImageFilter.cs",
        "finding": "CreateErode(float radiusX, float radiusY, ...) delegates to sk_imagefilter_new_erode with only axis-aligned radius parameters. No overload or parameter exists for a custom or circular structuring element.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "The C API sk_imagefilter_new_erode signature is (float radiusX, float radiusY, sk_imagefilter_t* input, sk_rect_t* cropRect). Skia's underlying SkMorphologyImageFilter only supports rectangular structuring elements — no circular shape option exists in the upstream C++ API.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Approximate a circular disc by chaining multiple small erode passes at different angles (not pixel-perfect but visually close for large radii).",
      "Use a custom SKShader with GLSL-style per-pixel sampling to implement disc erosion at the shader level via SKRuntimeEffect.",
      "For binary images specifically, pre-process with a soft circular mask convolution, then threshold."
    ],
    "resolution": {
      "hypothesis": "No circular structuring element is available in Skia's morphology filter. The user needs a workaround or would need to request it as a feature in upstream Skia.",
      "proposals": [
        {
          "title": "Explain Skia limitation and suggest SKRuntimeEffect workaround",
          "description": "Explain that Skia's erode/dilate filters use rectangular structuring elements by design, and the limitation is in the upstream library. For disc erosion on binary images, a custom SKRuntimeEffect (runtime shader) can sample neighbors in a circle.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Suggest chained approach",
          "description": "Apply multiple small erosions with equal radiusX and radiusY (i.e., square passes) and combine with a blur+threshold step to approximate circular morphology.",
          "category": "alternative",
          "confidence": 0.65,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Explain Skia limitation and suggest SKRuntimeEffect workaround",
      "recommendedReason": "Honest explanation of the upstream constraint, with an actionable workaround that can achieve the desired result."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.82,
      "reason": "This is a usage question. The current behavior is by design. Circular structuring elements are not supported by Skia's morphology filter. A workaround via SKRuntimeEffect exists for advanced users.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and core SkiaSharp area labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain the rectangular limitation of CreateErode and provide workaround direction",
        "risk": "high",
        "confidence": 0.82,
        "comment": "Thanks for the detailed question!\n\n`SKImageFilter.CreateErode` wraps Skia's `SkMorphologyImageFilter`, which only supports **rectangular** structuring elements defined by `(radiusX, radiusY)`. There is no built-in support for circular, disc, or custom-shaped structuring elements — this is a limitation of the underlying Skia engine.\n\n**Possible workarounds:**\n\n1. **SKRuntimeEffect (custom shader):** For a true disc erosion on binary images, you can write a custom runtime shader that samples pixels in a circular neighborhood. This is the most accurate approach but requires writing GLSL-style SkSL code.\n\n2. **Approximate with equal radii:** Using `CreateErode(r, r)` with equal radii gives a square, not a circle. The visual difference is most noticeable at corners, as you observed. For many use cases the square approximation is acceptable.\n\n3. **Multi-pass approximation:** Applying a series of 1-pixel erosions with `CreateErode(1, 1)` and combining results is still rectangular, but applying the filter in both passes with different crop rects does not produce a true disc either.\n\nIf disc morphology is important for your use case, you may want to open a feature request against the upstream [Skia project](https://bugs.chromium.org/p/skia) to add circular structuring element support to `SkMorphologyImageFilter`."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — behavior is by design in upstream Skia",
        "risk": "medium",
        "confidence": 0.78,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
