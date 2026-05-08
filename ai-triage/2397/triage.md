# Issue Triage Report — #2397

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-23T02:10:00Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp (0.85 (85%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** Feature request to expose variable font API: SkFontArguments / SKTypeface.MakeClone in SkiaSharp and hb_font_set_variations in HarfBuzzSharp.

**Analysis:** The issue requests exposure of two separate APIs for variable font rendering: (1) Skia's SkFontArguments struct and SkTypeface::makeClone, which allow creating a typeface instance with specific variation axis values (wght, slnt, etc.), and (2) HarfBuzz's hb_font_set_variations (and related hb_font_set_var_coords_design, hb_font_set_var_coords_normalized, hb_font_set_var_named_instance), needed for proper shaping of variable fonts. Investigation shows the HarfBuzz functions are already generated in HarfBuzzApi.generated.cs but lack C# wrapper methods in Font.cs, while the Skia side (SKFontArguments, SKTypeface.MakeClone) is absent from both the C API and the C# binding layer.

**Recommendations:** **needs-investigation** — Well-specified feature request with clear upstream API targets, strong community demand (39 reactions), and a hint of active maintainer work. Scoping the full implementation warrants investigation.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Related issues:** #2318

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The requested APIs are not present in binding/SkiaSharp (SKFontArguments/MakeClone) nor exposed at the C# wrapper level in HarfBuzzSharp (hb_font_set_variations and related). A comment from April 2026 hints that maintainer may be working on this in a dev branch. |

## Analysis

### Technical Summary

The issue requests exposure of two separate APIs for variable font rendering: (1) Skia's SkFontArguments struct and SkTypeface::makeClone, which allow creating a typeface instance with specific variation axis values (wght, slnt, etc.), and (2) HarfBuzz's hb_font_set_variations (and related hb_font_set_var_coords_design, hb_font_set_var_coords_normalized, hb_font_set_var_named_instance), needed for proper shaping of variable fonts. Investigation shows the HarfBuzz functions are already generated in HarfBuzzApi.generated.cs but lack C# wrapper methods in Font.cs, while the Skia side (SKFontArguments, SKTypeface.MakeClone) is absent from both the C API and the C# binding layer.

### Rationale

Classified as type/feature-request because the requested APIs (SKFontArguments, SKTypeface.MakeClone, HarfBuzz SetVariations) do not exist in SkiaSharp and require new C API shims plus C# wrappers. Area is area/SkiaSharp since the primary ask is SKTypeface.MakeClone; HarfBuzzSharp exposure is secondary. The feature has significant community demand (39 reactions, multiple comments spanning 3 years) and a maintainer hint of active dev work. Action is needs-investigation to scope the full implementation work.

### Key Signals

- "I'd like to paint text with font variation options but the needed API is not exposed to HarfBuzzSharp nor SkiaSharp." — **issue body** (Core request: expose existing upstream APIs to the C# layer)
- "Adding the needed APIs to HarfBuzzSharp should be trivial. These just needs to be added via generator." — **comment by contributor @Gillibald** (HarfBuzz side is low-effort; already generated at P/Invoke level)
- "All we actually need is sk_sp<SkTypeface> SkTypeface::makeClone (const SkFontArguments&)" — **comment by @Mikolaytis** (Narrows the Skia-side request to SKTypeface.MakeClone + SKFontArguments struct)
- "@mattleibow I love to see that you are doing work in a dev branch!" — **comment by @Mikolaytis, April 2026** (Maintainer may already be working on this feature; no PR is linked yet)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/HarfBuzzSharp/HarfBuzzApi.generated.cs` | — | direct | hb_font_set_variations, hb_font_set_var_coords_design, hb_font_set_var_coords_normalized, and hb_font_set_var_named_instance are all present as generated P/Invoke declarations but are not surfaced as public C# methods in Font.cs. |
| `binding/HarfBuzzSharp/Font.cs` | — | direct | No SetVariations, SetVarCoordsDesign, SetVarCoordsNormalized, or SetVarNamedInstance methods exist in the Font class. The generated low-level API is available but not wrapped. |
| `binding/SkiaSharp/SKTypeface.cs` | — | direct | No SKFontArguments type and no MakeClone method exist. The C# wrapper has no support for font variation axis overrides via the Skia path. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | — | direct | No sk_typeface_make_clone or font_arguments entries found. The C API shim has not been extended for this feature. |

### Next Questions

- Is the maintainer's dev branch work in progress for this feature?
- Should SKFontArguments use tag strings (e.g. 'wght') or numeric SkFourByteTag values?
- Is there a target milestone for this feature?

### Resolution Proposals

**Hypothesis:** Expose SkFontArguments as SKFontArguments struct + add MakeClone to SKTypeface (requires new C API), and add SetVariations/SetVarCoordsDesign/SetVarCoordsNormalized/SetVarNamedInstance to HarfBuzzSharp.Font (HarfBuzz P/Invoke already generated).

1. **Add SKFontArguments and SKTypeface.MakeClone** — fix, cost/m, validated=untested
   - Add a new SKFontArguments C# struct wrapping SkFontArguments (tag + value pairs), expose sk_typeface_make_clone in the C API shim, then add SKTypeface.MakeClone(SKFontArguments) in the C# wrapper. This follows the add-api skill workflow.
2. **Add SetVariations / SetVarCoords* methods to HarfBuzzSharp.Font** — fix, cost/s, validated=untested
   - The HarfBuzz P/Invoke bindings for hb_font_set_variations, hb_font_set_var_coords_design, hb_font_set_var_coords_normalized, and hb_font_set_var_named_instance are already generated. Adding public C# wrapper methods in Font.cs is low effort and unblocks variable font shaping.
3. **Workaround: Direct P/Invoke to libHarfBuzzSharp** — workaround, cost/s, validated=untested
   - Users can call hb_font_set_variations directly via P/Invoke against libHarfBuzzSharp until the wrapper is added. The function signature is already documented in the generated file.

**Recommended proposal:** Add SetVariations / SetVarCoords* methods to HarfBuzzSharp.Font

**Why:** HarfBuzz wrapper methods are the fastest path to unblock shaping of variable fonts; SKFontArguments/MakeClone is the complementary painting fix and should follow.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Well-specified feature request with clear upstream API targets, strong community demand (39 reactions), and a hint of active maintainer work. Scoping the full implementation warrants investigation. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply feature-request type, SkiaSharp area, and compatibility tenet labels | labels=type/feature-request, area/SkiaSharp, tenet/compatibility |
| link-related | low | 0.90 (90%) | Cross-reference related issue #2318 (broader variable fonts + OpenType features request) | linkedIssue=#2318 |
| add-comment | medium | 0.82 (82%) | Acknowledge the request, summarize implementation path, and provide P/Invoke workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for this feature request!

After investigating the codebase:

**Current state:**
- **HarfBuzz side:** `hb_font_set_variations`, `hb_font_set_var_coords_design`, `hb_font_set_var_coords_normalized`, and `hb_font_set_var_named_instance` are already present in the generated P/Invoke layer but not yet exposed as public `Font` methods.
- **Skia side:** `SKFontArguments` struct and `SKTypeface.MakeClone(SKFontArguments)` do not exist yet and require a new C API shim.

**Workaround (HarfBuzz shaping):** Until the wrapper is added, you can call `hb_font_set_variations` directly via P/Invoke against `libHarfBuzzSharp`.

**Related:** See also #2318 which covers variable font shaping and OpenType features more broadly.

This is tracked for a future release. We'll update this issue when work begins.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2397,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-23T02:10:00Z"
  },
  "summary": "Feature request to expose variable font API: SkFontArguments / SKTypeface.MakeClone in SkiaSharp and hb_font_set_variations in HarfBuzzSharp.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.85
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "relatedIssues": [
        2318
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "The requested APIs are not present in binding/SkiaSharp (SKFontArguments/MakeClone) nor exposed at the C# wrapper level in HarfBuzzSharp (hb_font_set_variations and related). A comment from April 2026 hints that maintainer may be working on this in a dev branch."
    }
  },
  "analysis": {
    "summary": "The issue requests exposure of two separate APIs for variable font rendering: (1) Skia's SkFontArguments struct and SkTypeface::makeClone, which allow creating a typeface instance with specific variation axis values (wght, slnt, etc.), and (2) HarfBuzz's hb_font_set_variations (and related hb_font_set_var_coords_design, hb_font_set_var_coords_normalized, hb_font_set_var_named_instance), needed for proper shaping of variable fonts. Investigation shows the HarfBuzz functions are already generated in HarfBuzzApi.generated.cs but lack C# wrapper methods in Font.cs, while the Skia side (SKFontArguments, SKTypeface.MakeClone) is absent from both the C API and the C# binding layer.",
    "codeInvestigation": [
      {
        "file": "binding/HarfBuzzSharp/HarfBuzzApi.generated.cs",
        "finding": "hb_font_set_variations, hb_font_set_var_coords_design, hb_font_set_var_coords_normalized, and hb_font_set_var_named_instance are all present as generated P/Invoke declarations but are not surfaced as public C# methods in Font.cs.",
        "relevance": "direct"
      },
      {
        "file": "binding/HarfBuzzSharp/Font.cs",
        "finding": "No SetVariations, SetVarCoordsDesign, SetVarCoordsNormalized, or SetVarNamedInstance methods exist in the Font class. The generated low-level API is available but not wrapped.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "finding": "No SKFontArguments type and no MakeClone method exist. The C# wrapper has no support for font variation axis overrides via the Skia path.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "No sk_typeface_make_clone or font_arguments entries found. The C API shim has not been extended for this feature.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "I'd like to paint text with font variation options but the needed API is not exposed to HarfBuzzSharp nor SkiaSharp.",
        "source": "issue body",
        "interpretation": "Core request: expose existing upstream APIs to the C# layer"
      },
      {
        "text": "Adding the needed APIs to HarfBuzzSharp should be trivial. These just needs to be added via generator.",
        "source": "comment by contributor @Gillibald",
        "interpretation": "HarfBuzz side is low-effort; already generated at P/Invoke level"
      },
      {
        "text": "All we actually need is sk_sp<SkTypeface> SkTypeface::makeClone (const SkFontArguments&)",
        "source": "comment by @Mikolaytis",
        "interpretation": "Narrows the Skia-side request to SKTypeface.MakeClone + SKFontArguments struct"
      },
      {
        "text": "@mattleibow I love to see that you are doing work in a dev branch!",
        "source": "comment by @Mikolaytis, April 2026",
        "interpretation": "Maintainer may already be working on this feature; no PR is linked yet"
      }
    ],
    "rationale": "Classified as type/feature-request because the requested APIs (SKFontArguments, SKTypeface.MakeClone, HarfBuzz SetVariations) do not exist in SkiaSharp and require new C API shims plus C# wrappers. Area is area/SkiaSharp since the primary ask is SKTypeface.MakeClone; HarfBuzzSharp exposure is secondary. The feature has significant community demand (39 reactions, multiple comments spanning 3 years) and a maintainer hint of active dev work. Action is needs-investigation to scope the full implementation work.",
    "resolution": {
      "hypothesis": "Expose SkFontArguments as SKFontArguments struct + add MakeClone to SKTypeface (requires new C API), and add SetVariations/SetVarCoordsDesign/SetVarCoordsNormalized/SetVarNamedInstance to HarfBuzzSharp.Font (HarfBuzz P/Invoke already generated).",
      "proposals": [
        {
          "title": "Add SKFontArguments and SKTypeface.MakeClone",
          "category": "fix",
          "effort": "cost/m",
          "validated": "untested",
          "description": "Add a new SKFontArguments C# struct wrapping SkFontArguments (tag + value pairs), expose sk_typeface_make_clone in the C API shim, then add SKTypeface.MakeClone(SKFontArguments) in the C# wrapper. This follows the add-api skill workflow."
        },
        {
          "title": "Add SetVariations / SetVarCoords* methods to HarfBuzzSharp.Font",
          "category": "fix",
          "effort": "cost/s",
          "validated": "untested",
          "description": "The HarfBuzz P/Invoke bindings for hb_font_set_variations, hb_font_set_var_coords_design, hb_font_set_var_coords_normalized, and hb_font_set_var_named_instance are already generated. Adding public C# wrapper methods in Font.cs is low effort and unblocks variable font shaping."
        },
        {
          "title": "Workaround: Direct P/Invoke to libHarfBuzzSharp",
          "category": "workaround",
          "effort": "cost/s",
          "validated": "untested",
          "description": "Users can call hb_font_set_variations directly via P/Invoke against libHarfBuzzSharp until the wrapper is added. The function signature is already documented in the generated file."
        }
      ],
      "recommendedProposal": "Add SetVariations / SetVarCoords* methods to HarfBuzzSharp.Font",
      "recommendedReason": "HarfBuzz wrapper methods are the fastest path to unblock shaping of variable fonts; SKFontArguments/MakeClone is the complementary painting fix and should follow."
    },
    "nextQuestions": [
      "Is the maintainer's dev branch work in progress for this feature?",
      "Should SKFontArguments use tag strings (e.g. 'wght') or numeric SkFourByteTag values?",
      "Is there a target milestone for this feature?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Well-specified feature request with clear upstream API targets, strong community demand (39 reactions), and a hint of active maintainer work. Scoping the full implementation warrants investigation.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request type, SkiaSharp area, and compatibility tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "tenet/compatibility"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference related issue #2318 (broader variable fonts + OpenType features request)",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 2318
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the request, summarize implementation path, and provide P/Invoke workaround",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for this feature request!\n\nAfter investigating the codebase:\n\n**Current state:**\n- **HarfBuzz side:** `hb_font_set_variations`, `hb_font_set_var_coords_design`, `hb_font_set_var_coords_normalized`, and `hb_font_set_var_named_instance` are already present in the generated P/Invoke layer but not yet exposed as public `Font` methods.\n- **Skia side:** `SKFontArguments` struct and `SKTypeface.MakeClone(SKFontArguments)` do not exist yet and require a new C API shim.\n\n**Workaround (HarfBuzz shaping):** Until the wrapper is added, you can call `hb_font_set_variations` directly via P/Invoke against `libHarfBuzzSharp`.\n\n**Related:** See also #2318 which covers variable font shaping and OpenType features more broadly.\n\nThis is tracked for a future release. We'll update this issue when work begins."
      }
    ]
  }
}
```

</details>
