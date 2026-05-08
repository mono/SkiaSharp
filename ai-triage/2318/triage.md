# Issue Triage Report — #2318

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T14:47:46Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp.HarfBuzz (0.80 (80%)) |
| Suggested action | keep-open (0.85 (85%)) |

**Issue Summary:** Feature request to expose OpenType shaping features (liga, kern) via SKShaper.Shape, HarfBuzz variable-font coordinate setters (hb_font_set_var_coords_design, hb_font_set_var_named_instance), and SKTypeface variation-axis introspection and clone APIs — all needed for high-quality typography with variable fonts.

**Analysis:** Three related but distinct API gaps in SkiaSharp's typography stack: (1) SKShaper.Shape does not forward OpenType features to the underlying HarfBuzz shaper, even though HarfBuzzSharp.Font.Shape already accepts Feature[] and the Feature struct is exposed; (2) hb_font_set_var_coords_design and hb_font_set_var_named_instance have generated P/Invoke bindings but no managed wrappers in HarfBuzzSharp.Font; (3) SKTypeface has no C API surface for getVariationDesignParameters, getVariationDesignPosition, or makeClone, which are the key building blocks for variable font rendering in Skia.

**Recommendations:** **keep-open** — Valid, well-specified feature request covering three related typography API gaps. Two of the three gaps (SKShaper features, Font.SetVarCoordsDesign) are low-effort additions. The issue is closely related to #2397 which should be cross-linked. Community interest is high (17 +1 on this issue, 34 +1 on #2397).

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp.HarfBuzz |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Try to pass OpenType feature flags (e.g. kern, liga) to SKShaper.Shape — parameter does not exist
2. Try to set variable font design coordinates via HarfBuzz Font — no managed wrapper for hb_font_set_var_coords_design
3. Try to read variation axis parameters or clone a typeface with specific axis values from SKTypeface — methods do not exist

**Environment:** Windows .NET app, SkiaSharp (version not specified, filed 2022-11-19); reporter has been PInvoking to libSkiaSharp/libHarfBuzzSharp as workaround

**Related issues:** #2397, #2512

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2397 — Very closely related: '[FEATURE] Expose additional API for variable font support' — requests SKTypeface::makeClone, SkFontParameters.VariationAxis, hb-font-set-variations; 34 +1 reactions, triaged
- https://github.com/mono/SkiaSharp/issues/2512 — Related: '[FEATURE] Add function to SkiaSharp.Harfbuzz for getting shaped text paths'

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Code investigation confirms the three requested capabilities are still absent in the managed C# layer as of the current codebase: SKShaper.Shape lacks feature parameters, Font has no SetVarCoordsDesign wrapper, and SKTypeface has no variation introspection or clone methods. |

## Analysis

### Technical Summary

Three related but distinct API gaps in SkiaSharp's typography stack: (1) SKShaper.Shape does not forward OpenType features to the underlying HarfBuzz shaper, even though HarfBuzzSharp.Font.Shape already accepts Feature[] and the Feature struct is exposed; (2) hb_font_set_var_coords_design and hb_font_set_var_named_instance have generated P/Invoke bindings but no managed wrappers in HarfBuzzSharp.Font; (3) SKTypeface has no C API surface for getVariationDesignParameters, getVariationDesignPosition, or makeClone, which are the key building blocks for variable font rendering in Skia.

### Rationale

Classified as type/feature-request because all three requests add new API surface that does not exist today in the managed C# layer. Area is SkiaSharp.HarfBuzz because the SKShaper change is the most user-visible gap and lives in that package; the HarfBuzz Font wrappers are also in that dependency chain. Tenet/compatibility added because adding new overloads and wrapping existing P/Invokes must not break existing binary-compatible callers. Closely related to #2397 which covers some of the same ground (makeClone, hb-font-set-variations) and has more reactions — these issues should be consolidated or cross-referenced.

### Key Signals

- "SkShaper.Shape should get Feature[] features parameter which would enable easy access to features like 'liga' or 'kern'" — **issue body** (SKShaper is missing feature passthrough. HarfBuzzSharp.Font.Shape already accepts Feature[] — SKShaper just needs new overloads.)
- "hb_font_set_var_coords_design, hb_font_set_var_named_instance should be exposed" — **issue body** (P/Invoke stubs exist in generated code; only the managed wrappers in Font.cs are missing.)
- "makeClone - enables setting coords of font face variable axis" — **issue body** (SkTypeface::makeClone (C++) is not in the C API shim, so it cannot be exposed without adding sk_typeface_make_clone to externals/skia/src/c/.)
- "everything worked out, including handling fonts from arbitrary streams, variable fonts, precise text measurement and positioning" — **comment by themcoo (2024-06-29)** (Original reporter resolved their immediate needs via direct P/Invoke workarounds, but the gap remains for the broader community.)
- "What about implementing SkFontArguments / SkTypeface::makeClone for SkiaSharp 3.x?" — **comment by superbonaci (2025-07-25)** (Still actively requested in 2025, confirming the gap persists into the current version.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/SKShaper.cs` | 51-67 | direct | SKShaper.Shape(Buffer buffer, SKFont font) calls hbFont.Shape(buffer) without any Feature[] parameter — there is no overload that accepts OpenType features, so callers have no way to enable liga, kern, etc. through SKShaper. |
| `binding/HarfBuzzSharp/Font.cs` | 297-324 | direct | HarfBuzzSharp.Font.Shape already has 'public void Shape(Buffer buffer, params Feature[] features)' — the feature passthrough exists at the HarfBuzz level. SKShaper just never calls it with features. |
| `binding/HarfBuzzSharp/HarfBuzzApi.generated.cs` | — | direct | P/Invoke stubs for hb_font_set_var_coords_design and hb_font_set_var_named_instance are present in the generated bindings. However, Font.cs contains no managed public wrapper methods for these — callers cannot set variable font design coordinates without direct P/Invoke. |
| `binding/SkiaSharp/SKTypeface.cs` | — | direct | SKTypeface has no methods for GetVariationDesignParameters, GetVariationDesignPosition, or MakeClone. SkiaApi.generated.cs also has no sk_typeface_make_clone or sk_typeface_get_variation_* entries, meaning the C API shim layer itself is missing these functions. |
| `binding/HarfBuzzSharp/Feature.cs` | — | context | HarfBuzzSharp.Feature struct exists with Feature.FromString() and Feature.ToString() support — the Feature type reporters need for SKShaper is already part of the public API surface. |

### Workarounds

- For OpenType features in shaping: bypass SKShaper and use HarfBuzzSharp.Font.Shape(buffer, Feature.FromString("liga=1"), Feature.FromString("kern=1")) directly after creating a Font from the typeface.
- For variable font design coordinates: P/Invoke directly to hb_font_set_var_coords_design in libHarfBuzzSharp — the native function is already exported.
- For SKTypeface variation axis information: P/Invoke to Skia C++ via a custom libSkiaSharp build that exposes sk_typeface_get_variation_design_parameters; no clean .NET workaround currently.

### Next Questions

- Should SKShaper.Shape feature overloads simply delegate to HarfBuzzSharp.Font.Shape(buffer, features) with minimal changes, or is a higher-level API (e.g. SKShaper.Features property) preferred?
- Is the SkTypeface::makeClone API the correct path for variable font rendering in Skia, or should SKFont.WithVariations be the primary surface instead?
- Should this issue be consolidated with #2397 which covers overlapping ground (makeClone, hb_font_set_variations) and has more community reactions?

### Resolution Proposals

**Hypothesis:** Three independent but related additions are needed: (1) new SKShaper.Shape overloads that pass Feature[] to Font.Shape, (2) managed wrappers SetVarCoordsDesign/SetVarNamedInstance on HarfBuzzSharp.Font, (3) C API additions in externals/skia/src/c/ for sk_typeface_make_clone and variation axis queries, plus C# wrappers in SKTypeface.

1. **Add Feature[] overloads to SKShaper.Shape** — fix, confidence 0.90 (90%), cost/s, validated=untested
   - Add SKShaper.Shape(Buffer buffer, SKFont font, IReadOnlyList<Feature> features) and the string/offset variants. Internally pass features to hbFont.Shape(buffer, features). This is the easiest of the three gaps since HarfBuzzSharp.Font.Shape already accepts Feature[].
2. **Add SetVarCoordsDesign and SetVarNamedInstance to HarfBuzzSharp.Font** — fix, confidence 0.90 (90%), cost/s, validated=untested
   - Expose hb_font_set_var_coords_design and hb_font_set_var_named_instance as public managed wrappers on HarfBuzzSharp.Font. P/Invoke stubs already exist in HarfBuzzApi.generated.cs so only the public-facing wrappers in Font.cs are needed.
3. **Add SKTypeface variation axis API and MakeClone** — fix, confidence 0.75 (75%), cost/l, validated=untested
   - Add sk_typeface_make_clone, sk_typeface_get_variation_design_parameters, and sk_typeface_get_variation_design_position to the C API shim in externals/skia/src/c/, regenerate bindings, and add SKTypeface.MakeClone(SKFontArguments) and related C# wrappers.

**Recommended proposal:** Add Feature[] overloads to SKShaper.Shape

**Why:** Lowest effort with highest user impact — the infrastructure already exists in HarfBuzzSharp.Font. The SKTypeface variation API requires C API layer changes which carry broader risk.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.85 (85%) |
| Reason | Valid, well-specified feature request covering three related typography API gaps. Two of the three gaps (SKShaper features, Font.SetVarCoordsDesign) are low-effort additions. The issue is closely related to #2397 which should be cross-linked. Community interest is high (17 +1 on this issue, 34 +1 on #2397). |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply feature-request, HarfBuzz area, Windows-Classic platform, and compatibility tenet labels | labels=type/feature-request, area/SkiaSharp.HarfBuzz, os/Windows-Classic, tenet/compatibility |
| link-related | low | 0.95 (95%) | Cross-reference #2397 which requests overlapping variable font / makeClone APIs and has more community traction | linkedIssue=#2397 |
| add-comment | medium | 0.85 (85%) | Acknowledge the three requested APIs, explain current partial state (Font.Shape features already work), and point to workarounds | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed feature request! Here's the current status of each requested API:

**1. SKShaper.Shape with OpenType features**
The underlying `HarfBuzzSharp.Font.Shape(Buffer, params Feature[])` already accepts feature arrays, so as a workaround you can bypass `SKShaper` and call it directly:
```csharp
// Create a HarfBuzz font from the SkiaSharp typeface
using var face = new HarfBuzzSharp.Face(...);
using var hbFont = new HarfBuzzSharp.Font(face);
hbFont.SetFontSize(fontSize);

// Shape with OpenType features
using var buffer = new HarfBuzzSharp.Buffer();
buffer.AddUtf16(text);
buffer.GuessSegmentProperties();
hbFont.Shape(buffer,
    HarfBuzzSharp.Feature.FromString("liga=1"),
    HarfBuzzSharp.Feature.FromString("kern=1"));
```
Adding `Feature[]` overloads to `SKShaper.Shape` is a tracked enhancement.

**2. HarfBuzz variable-font coordinate setters**
`hb_font_set_var_coords_design` and `hb_font_set_var_named_instance` have P/Invoke stubs in the generated bindings but lack public managed wrappers on `HarfBuzzSharp.Font`. As a workaround you can P/Invoke the functions directly via `DllImport("libHarfBuzzSharp")`.

**3. SKTypeface variation axis / makeClone**
This requires new C API additions (no `sk_typeface_make_clone` exists in the current shim). See also the related issue #2397 which tracks the same gap.

We're tracking this together with #2397.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2318,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T14:47:46Z"
  },
  "summary": "Feature request to expose OpenType shaping features (liga, kern) via SKShaper.Shape, HarfBuzz variable-font coordinate setters (hb_font_set_var_coords_design, hb_font_set_var_named_instance), and SKTypeface variation-axis introspection and clone APIs — all needed for high-quality typography with variable fonts.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.HarfBuzz",
      "confidence": 0.8
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Try to pass OpenType feature flags (e.g. kern, liga) to SKShaper.Shape — parameter does not exist",
        "Try to set variable font design coordinates via HarfBuzz Font — no managed wrapper for hb_font_set_var_coords_design",
        "Try to read variation axis parameters or clone a typeface with specific axis values from SKTypeface — methods do not exist"
      ],
      "environmentDetails": "Windows .NET app, SkiaSharp (version not specified, filed 2022-11-19); reporter has been PInvoking to libSkiaSharp/libHarfBuzzSharp as workaround",
      "relatedIssues": [
        2397,
        2512
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2397",
          "description": "Very closely related: '[FEATURE] Expose additional API for variable font support' — requests SKTypeface::makeClone, SkFontParameters.VariationAxis, hb-font-set-variations; 34 +1 reactions, triaged"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2512",
          "description": "Related: '[FEATURE] Add function to SkiaSharp.Harfbuzz for getting shaped text paths'"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "Code investigation confirms the three requested capabilities are still absent in the managed C# layer as of the current codebase: SKShaper.Shape lacks feature parameters, Font has no SetVarCoordsDesign wrapper, and SKTypeface has no variation introspection or clone methods."
    }
  },
  "analysis": {
    "summary": "Three related but distinct API gaps in SkiaSharp's typography stack: (1) SKShaper.Shape does not forward OpenType features to the underlying HarfBuzz shaper, even though HarfBuzzSharp.Font.Shape already accepts Feature[] and the Feature struct is exposed; (2) hb_font_set_var_coords_design and hb_font_set_var_named_instance have generated P/Invoke bindings but no managed wrappers in HarfBuzzSharp.Font; (3) SKTypeface has no C API surface for getVariationDesignParameters, getVariationDesignPosition, or makeClone, which are the key building blocks for variable font rendering in Skia.",
    "rationale": "Classified as type/feature-request because all three requests add new API surface that does not exist today in the managed C# layer. Area is SkiaSharp.HarfBuzz because the SKShaper change is the most user-visible gap and lives in that package; the HarfBuzz Font wrappers are also in that dependency chain. Tenet/compatibility added because adding new overloads and wrapping existing P/Invokes must not break existing binary-compatible callers. Closely related to #2397 which covers some of the same ground (makeClone, hb-font-set-variations) and has more reactions — these issues should be consolidated or cross-referenced.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/SKShaper.cs",
        "lines": "51-67",
        "finding": "SKShaper.Shape(Buffer buffer, SKFont font) calls hbFont.Shape(buffer) without any Feature[] parameter — there is no overload that accepts OpenType features, so callers have no way to enable liga, kern, etc. through SKShaper.",
        "relevance": "direct"
      },
      {
        "file": "binding/HarfBuzzSharp/Font.cs",
        "lines": "297-324",
        "finding": "HarfBuzzSharp.Font.Shape already has 'public void Shape(Buffer buffer, params Feature[] features)' — the feature passthrough exists at the HarfBuzz level. SKShaper just never calls it with features.",
        "relevance": "direct"
      },
      {
        "file": "binding/HarfBuzzSharp/HarfBuzzApi.generated.cs",
        "finding": "P/Invoke stubs for hb_font_set_var_coords_design and hb_font_set_var_named_instance are present in the generated bindings. However, Font.cs contains no managed public wrapper methods for these — callers cannot set variable font design coordinates without direct P/Invoke.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "finding": "SKTypeface has no methods for GetVariationDesignParameters, GetVariationDesignPosition, or MakeClone. SkiaApi.generated.cs also has no sk_typeface_make_clone or sk_typeface_get_variation_* entries, meaning the C API shim layer itself is missing these functions.",
        "relevance": "direct"
      },
      {
        "file": "binding/HarfBuzzSharp/Feature.cs",
        "finding": "HarfBuzzSharp.Feature struct exists with Feature.FromString() and Feature.ToString() support — the Feature type reporters need for SKShaper is already part of the public API surface.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "SkShaper.Shape should get Feature[] features parameter which would enable easy access to features like 'liga' or 'kern'",
        "source": "issue body",
        "interpretation": "SKShaper is missing feature passthrough. HarfBuzzSharp.Font.Shape already accepts Feature[] — SKShaper just needs new overloads."
      },
      {
        "text": "hb_font_set_var_coords_design, hb_font_set_var_named_instance should be exposed",
        "source": "issue body",
        "interpretation": "P/Invoke stubs exist in generated code; only the managed wrappers in Font.cs are missing."
      },
      {
        "text": "makeClone - enables setting coords of font face variable axis",
        "source": "issue body",
        "interpretation": "SkTypeface::makeClone (C++) is not in the C API shim, so it cannot be exposed without adding sk_typeface_make_clone to externals/skia/src/c/."
      },
      {
        "text": "everything worked out, including handling fonts from arbitrary streams, variable fonts, precise text measurement and positioning",
        "source": "comment by themcoo (2024-06-29)",
        "interpretation": "Original reporter resolved their immediate needs via direct P/Invoke workarounds, but the gap remains for the broader community."
      },
      {
        "text": "What about implementing SkFontArguments / SkTypeface::makeClone for SkiaSharp 3.x?",
        "source": "comment by superbonaci (2025-07-25)",
        "interpretation": "Still actively requested in 2025, confirming the gap persists into the current version."
      }
    ],
    "workarounds": [
      "For OpenType features in shaping: bypass SKShaper and use HarfBuzzSharp.Font.Shape(buffer, Feature.FromString(\"liga=1\"), Feature.FromString(\"kern=1\")) directly after creating a Font from the typeface.",
      "For variable font design coordinates: P/Invoke directly to hb_font_set_var_coords_design in libHarfBuzzSharp — the native function is already exported.",
      "For SKTypeface variation axis information: P/Invoke to Skia C++ via a custom libSkiaSharp build that exposes sk_typeface_get_variation_design_parameters; no clean .NET workaround currently."
    ],
    "nextQuestions": [
      "Should SKShaper.Shape feature overloads simply delegate to HarfBuzzSharp.Font.Shape(buffer, features) with minimal changes, or is a higher-level API (e.g. SKShaper.Features property) preferred?",
      "Is the SkTypeface::makeClone API the correct path for variable font rendering in Skia, or should SKFont.WithVariations be the primary surface instead?",
      "Should this issue be consolidated with #2397 which covers overlapping ground (makeClone, hb_font_set_variations) and has more community reactions?"
    ],
    "resolution": {
      "hypothesis": "Three independent but related additions are needed: (1) new SKShaper.Shape overloads that pass Feature[] to Font.Shape, (2) managed wrappers SetVarCoordsDesign/SetVarNamedInstance on HarfBuzzSharp.Font, (3) C API additions in externals/skia/src/c/ for sk_typeface_make_clone and variation axis queries, plus C# wrappers in SKTypeface.",
      "proposals": [
        {
          "title": "Add Feature[] overloads to SKShaper.Shape",
          "description": "Add SKShaper.Shape(Buffer buffer, SKFont font, IReadOnlyList<Feature> features) and the string/offset variants. Internally pass features to hbFont.Shape(buffer, features). This is the easiest of the three gaps since HarfBuzzSharp.Font.Shape already accepts Feature[].",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Add SetVarCoordsDesign and SetVarNamedInstance to HarfBuzzSharp.Font",
          "description": "Expose hb_font_set_var_coords_design and hb_font_set_var_named_instance as public managed wrappers on HarfBuzzSharp.Font. P/Invoke stubs already exist in HarfBuzzApi.generated.cs so only the public-facing wrappers in Font.cs are needed.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Add SKTypeface variation axis API and MakeClone",
          "description": "Add sk_typeface_make_clone, sk_typeface_get_variation_design_parameters, and sk_typeface_get_variation_design_position to the C API shim in externals/skia/src/c/, regenerate bindings, and add SKTypeface.MakeClone(SKFontArguments) and related C# wrappers.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add Feature[] overloads to SKShaper.Shape",
      "recommendedReason": "Lowest effort with highest user impact — the infrastructure already exists in HarfBuzzSharp.Font. The SKTypeface variation API requires C API layer changes which carry broader risk."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.85,
      "reason": "Valid, well-specified feature request covering three related typography API gaps. Two of the three gaps (SKShaper features, Font.SetVarCoordsDesign) are low-effort additions. The issue is closely related to #2397 which should be cross-linked. Community interest is high (17 +1 on this issue, 34 +1 on #2397).",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, HarfBuzz area, Windows-Classic platform, and compatibility tenet labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp.HarfBuzz",
          "os/Windows-Classic",
          "tenet/compatibility"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference #2397 which requests overlapping variable font / makeClone APIs and has more community traction",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 2397
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the three requested APIs, explain current partial state (Font.Shape features already work), and point to workarounds",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed feature request! Here's the current status of each requested API:\n\n**1. SKShaper.Shape with OpenType features**\nThe underlying `HarfBuzzSharp.Font.Shape(Buffer, params Feature[])` already accepts feature arrays, so as a workaround you can bypass `SKShaper` and call it directly:\n```csharp\n// Create a HarfBuzz font from the SkiaSharp typeface\nusing var face = new HarfBuzzSharp.Face(...);\nusing var hbFont = new HarfBuzzSharp.Font(face);\nhbFont.SetFontSize(fontSize);\n\n// Shape with OpenType features\nusing var buffer = new HarfBuzzSharp.Buffer();\nbuffer.AddUtf16(text);\nbuffer.GuessSegmentProperties();\nhbFont.Shape(buffer,\n    HarfBuzzSharp.Feature.FromString(\"liga=1\"),\n    HarfBuzzSharp.Feature.FromString(\"kern=1\"));\n```\nAdding `Feature[]` overloads to `SKShaper.Shape` is a tracked enhancement.\n\n**2. HarfBuzz variable-font coordinate setters**\n`hb_font_set_var_coords_design` and `hb_font_set_var_named_instance` have P/Invoke stubs in the generated bindings but lack public managed wrappers on `HarfBuzzSharp.Font`. As a workaround you can P/Invoke the functions directly via `DllImport(\"libHarfBuzzSharp\")`.\n\n**3. SKTypeface variation axis / makeClone**\nThis requires new C API additions (no `sk_typeface_make_clone` exists in the current shim). See also the related issue #2397 which tracks the same gap.\n\nWe're tracking this together with #2397."
      }
    ]
  }
}
```

</details>
