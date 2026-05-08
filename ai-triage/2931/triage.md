# Issue Triage Report — #2931

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T15:44:54Z |
| Type | type/feature-request (0.95 (95%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** Feature request to add color quantization and palette-mapping APIs to SkiaSharp, enabling RGBA bitmaps to be reduced to indexed-color representations with a 32-bit palette — functionality that was dropped from upstream Skia.

**Analysis:** Reporter requests three new APIs: (1) quantize an RGBA SKBitmap to a 32-bit indexed SKBitmap, (2) shift/remap quantization palettes, and (3) draw an indexed bitmap back to RGBA using a palette. Upstream Skia removed its colormap support, so SkiaSharp cannot expose it directly. The maintainer (mattleibow) has already suggested using SkSL runtime effects / shaders as an alternative approach, and contributor taublast confirmed shaders are capable of implementing this in SkiaSharp v3.

**Recommendations:** **keep-open** — Valid feature request with no upstream Skia support; keep open for design discussion. A shader-based workaround exists but does not fully address the 3-part quantization API requested.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | type/feature-request |

## Evidence

### Reproduction

**Environment:** No specific version or platform mentioned; request is platform-agnostic.

## Analysis

### Technical Summary

Reporter requests three new APIs: (1) quantize an RGBA SKBitmap to a 32-bit indexed SKBitmap, (2) shift/remap quantization palettes, and (3) draw an indexed bitmap back to RGBA using a palette. Upstream Skia removed its colormap support, so SkiaSharp cannot expose it directly. The maintainer (mattleibow) has already suggested using SkSL runtime effects / shaders as an alternative approach, and contributor taublast confirmed shaders are capable of implementing this in SkiaSharp v3.

### Rationale

This is clearly a feature request: the reporter wants new functionality (palette quantization) that no longer exists in upstream Skia and has never been part of SkiaSharp's public API. No broken behavior is described. The request is well-specified with three distinct sub-APIs. Because upstream Skia dropped colormap support, a native binding approach is not feasible; any implementation would need to be done at the C# layer or via SkSL shaders. The maintainer's comment already points to the shader-based workaround path.

### Key Signals

- "Skia dropped colormap support for some reason" — **issue body** (Upstream removal means a direct C-API binding is not feasible; any solution must live at the SkiaSharp C# layer or use shaders.)
- "I tried using avalonia ui WriteableBitmaps but they are too slow." — **issue body** (Performance is a key driver; reporter needs a hardware-accelerated or efficient path.)
- "I wonder if this is better with a custom runtime effect / shader?" — **comment by mattleibow** (Maintainer is suggesting SkSL shaders as the practical workaround, not committing to adding a native quantization API.)
- "upon SkiaSharp v3 release it would be totally up to us developers to provide an implementation for absolutely any image processing" — **comment by taublast** (Contributors confirm shaders unlock this capability in v3; confirms the workaround path is viable.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | — | direct | No quantization, palette, or colormap APIs exist in SKBitmap — confirms the feature is absent and has not been implemented. |
| `binding/SkiaSharp/SKColorFilter.cs` | — | related | SKColorFilter.CreateTable() provides per-channel LUT remapping (up to 256 entries per channel) but is not a full palette/quantization API — related but insufficient for the requested 32-bit indexed color mapping. |
| `binding/SkiaSharp/SKRuntimeEffect.cs` | — | related | SKRuntimeEffect.CreateShader() and BuildShader() expose SkSL shader compilation, allowing custom per-pixel color transformations including palette lookups. This is the path the maintainer suggested as a workaround. |

### Workarounds

- Use SKRuntimeEffect.CreateShader() with a custom SkSL shader to implement palette lookup — the shader can accept a palette as a uniform array and map each pixel's integer color value to its palette entry.
- Use SKColorFilter.CreateTable() for simple per-channel remapping (limited to 256 entries per channel, not full RGBA quantization).
- Implement quantization in C# using Span<T>/unsafe pixel access on SKBitmap.GetPixels() for batch processing, then draw the result with a lookup shader.

### Resolution Proposals

**Hypothesis:** Upstream Skia removed colormap/quantization APIs; the equivalent functionality can be achieved with SkSL runtime shaders operating on a palette uniform, or via CPU-side pixel manipulation using SKBitmap.

1. **Use SkSL runtime shader for palette mapping** — workaround, confidence 0.80 (80%), cost/s, validated=untested
   - Write a SkSL shader that accepts a palette as a uniform and maps each pixel's integer representation to a palette color. Apply via SKPaint with the shader set. This leverages hardware acceleration and avoids any new API.
2. **Implement palette quantization as a helper class in C#** — fix, confidence 0.65 (65%), cost/l, validated=untested
   - Add a helper class (e.g. SKQuantizer) that processes SKBitmap pixels via unsafe pointer access to build a palette and produce an indexed representation. Would be a pure C# addition without native bindings.

**Recommended proposal:** Use SkSL runtime shader for palette mapping

**Why:** Leverages existing SKRuntimeEffect API, is hardware-accelerated, and the maintainer already pointed to this path.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Valid feature request with no upstream Skia support; keep open for design discussion. A shader-based workaround exists but does not fully address the 3-part quantization API requested. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Confirm type/feature-request label and add area/SkiaSharp | labels=type/feature-request, area/SkiaSharp |
| add-comment | medium | 0.80 (80%) | Post response acknowledging the request, confirming upstream removal, and explaining the SkSL shader workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed request! Upstream Skia removed colormap/quantization support, so SkiaSharp can't expose it via a direct native binding.

In the meantime, **SkSL runtime shaders** (via `SKRuntimeEffect`) are the recommended path for GPU-accelerated palette mapping:

```csharp
// Example: apply a 256-entry palette via shader
string sksl = """
uniform shader image;
uniform half4 palette[256];
half4 main(float2 coord) {
    half4 src = image.eval(coord);
    // quantize to palette index (simplified)
    int idx = int(src.r * 255.0);
    return palette[idx];
}
""";
var effect = SKRuntimeEffect.CreateShader(sksl, out var err);
```

See https://skia.org/docs/user/sksl/ for full SkSL docs and https://shaders.skia.org/ for the playground.

We're keeping this open to track interest in a higher-level C# helper API. Feel free to add a 👍 if this is important to your use case.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2931,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T15:44:54Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "Feature request to add color quantization and palette-mapping APIs to SkiaSharp, enabling RGBA bitmaps to be reduced to indexed-color representations with a 32-bit palette — functionality that was dropped from upstream Skia.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    }
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "No specific version or platform mentioned; request is platform-agnostic.",
      "repoLinks": []
    }
  },
  "analysis": {
    "summary": "Reporter requests three new APIs: (1) quantize an RGBA SKBitmap to a 32-bit indexed SKBitmap, (2) shift/remap quantization palettes, and (3) draw an indexed bitmap back to RGBA using a palette. Upstream Skia removed its colormap support, so SkiaSharp cannot expose it directly. The maintainer (mattleibow) has already suggested using SkSL runtime effects / shaders as an alternative approach, and contributor taublast confirmed shaders are capable of implementing this in SkiaSharp v3.",
    "rationale": "This is clearly a feature request: the reporter wants new functionality (palette quantization) that no longer exists in upstream Skia and has never been part of SkiaSharp's public API. No broken behavior is described. The request is well-specified with three distinct sub-APIs. Because upstream Skia dropped colormap support, a native binding approach is not feasible; any implementation would need to be done at the C# layer or via SkSL shaders. The maintainer's comment already points to the shader-based workaround path.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "finding": "No quantization, palette, or colormap APIs exist in SKBitmap — confirms the feature is absent and has not been implemented.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKColorFilter.cs",
        "finding": "SKColorFilter.CreateTable() provides per-channel LUT remapping (up to 256 entries per channel) but is not a full palette/quantization API — related but insufficient for the requested 32-bit indexed color mapping.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKRuntimeEffect.cs",
        "finding": "SKRuntimeEffect.CreateShader() and BuildShader() expose SkSL shader compilation, allowing custom per-pixel color transformations including palette lookups. This is the path the maintainer suggested as a workaround.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Skia dropped colormap support for some reason",
        "source": "issue body",
        "interpretation": "Upstream removal means a direct C-API binding is not feasible; any solution must live at the SkiaSharp C# layer or use shaders."
      },
      {
        "text": "I tried using avalonia ui WriteableBitmaps but they are too slow.",
        "source": "issue body",
        "interpretation": "Performance is a key driver; reporter needs a hardware-accelerated or efficient path."
      },
      {
        "text": "I wonder if this is better with a custom runtime effect / shader?",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer is suggesting SkSL shaders as the practical workaround, not committing to adding a native quantization API."
      },
      {
        "text": "upon SkiaSharp v3 release it would be totally up to us developers to provide an implementation for absolutely any image processing",
        "source": "comment by taublast",
        "interpretation": "Contributors confirm shaders unlock this capability in v3; confirms the workaround path is viable."
      }
    ],
    "workarounds": [
      "Use SKRuntimeEffect.CreateShader() with a custom SkSL shader to implement palette lookup — the shader can accept a palette as a uniform array and map each pixel's integer color value to its palette entry.",
      "Use SKColorFilter.CreateTable() for simple per-channel remapping (limited to 256 entries per channel, not full RGBA quantization).",
      "Implement quantization in C# using Span<T>/unsafe pixel access on SKBitmap.GetPixels() for batch processing, then draw the result with a lookup shader."
    ],
    "resolution": {
      "hypothesis": "Upstream Skia removed colormap/quantization APIs; the equivalent functionality can be achieved with SkSL runtime shaders operating on a palette uniform, or via CPU-side pixel manipulation using SKBitmap.",
      "proposals": [
        {
          "title": "Use SkSL runtime shader for palette mapping",
          "description": "Write a SkSL shader that accepts a palette as a uniform and maps each pixel's integer representation to a palette color. Apply via SKPaint with the shader set. This leverages hardware acceleration and avoids any new API.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Implement palette quantization as a helper class in C#",
          "description": "Add a helper class (e.g. SKQuantizer) that processes SKBitmap pixels via unsafe pointer access to build a palette and produce an indexed representation. Would be a pure C# addition without native bindings.",
          "category": "fix",
          "confidence": 0.65,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use SkSL runtime shader for palette mapping",
      "recommendedReason": "Leverages existing SKRuntimeEffect API, is hardware-accelerated, and the maintainer already pointed to this path."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.8,
      "reason": "Valid feature request with no upstream Skia support; keep open for design discussion. A shader-based workaround exists but does not fully address the 3-part quantization API requested.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Confirm type/feature-request label and add area/SkiaSharp",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post response acknowledging the request, confirming upstream removal, and explaining the SkSL shader workaround",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the detailed request! Upstream Skia removed colormap/quantization support, so SkiaSharp can't expose it via a direct native binding.\n\nIn the meantime, **SkSL runtime shaders** (via `SKRuntimeEffect`) are the recommended path for GPU-accelerated palette mapping:\n\n```csharp\n// Example: apply a 256-entry palette via shader\nstring sksl = \"\"\"\nuniform shader image;\nuniform half4 palette[256];\nhalf4 main(float2 coord) {\n    half4 src = image.eval(coord);\n    // quantize to palette index (simplified)\n    int idx = int(src.r * 255.0);\n    return palette[idx];\n}\n\"\"\";\nvar effect = SKRuntimeEffect.CreateShader(sksl, out var err);\n```\n\nSee https://skia.org/docs/user/sksl/ for full SkSL docs and https://shaders.skia.org/ for the playground.\n\nWe're keeping this open to track interest in a higher-level C# helper API. Feel free to add a 👍 if this is important to your use case."
      }
    ]
  }
}
```

</details>
