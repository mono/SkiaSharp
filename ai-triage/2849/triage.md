# Issue Triage Report — #2849

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:16:27Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-fixed (0.82 (82%)) |

**Issue Summary:** SKBitmap.ColorSpace always returns null in SkiaSharp 2.88.x due to an explicit backwards-compatibility override, making it impossible to detect CMYK vs RGB images via the ColorSpace API; fixed in 3.0+.

**Analysis:** In 2.88.x, SKBitmap.ColorSpace delegates to Info.ColorSpace (SKImageInfo), which reads from the native sk_bitmap_get_info C API. A backwards-compatibility override was explicitly forcing ColorSpace to null during decode. This was removed in 3.0. However, detecting CMYK specifically via ColorSpace is a deeper limitation: Skia does not have a CMYK SKColorSpace representation (CreateIcc with CMYK ICC returns null as confirmed by tests), so CMYK images decoded via SKBitmap.Decode will still have a null ColorSpace in all versions — though RGB images with ICC profiles will now correctly expose their colorspace in 3.0+.

**Recommendations:** **close-as-fixed** — Maintainer confirmed the backwards-compat null override was removed in 3.0. Current stable is 3.119.3. The reporter was waiting for 3.0 stable which is now available. The specific reported bug (ColorSpace always null in 2.88.x) is fixed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug, status/needs-attention |

## Evidence

### Reproduction

1. Load a JPEG image (CMYK or RGB) using SKBitmap.Decode
2. Read SKBitmap.ColorSpace
3. Observe that it is always null regardless of the image's actual color space

**Environment:** SkiaSharp 2.88.3, Visual Studio (Windows), all platforms

**Related issues:** #523, #1768

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/523 — Related CMYK color skew issue (closed)
- https://github.com/mono/SkiaSharp/issues/1768 — Related: how to detect CMYK images (closed)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | SKBitmap.ColorSpace always returns null |
| Repro quality | partial |
| Target frameworks | net8.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | Maintainer confirmed that 3.0+ removed the backwards-compatibility line that forced ColorSpace to null. Current stable is 3.119.x. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.88 (88%) |
| Reason | Maintainer @mattleibow confirmed in issue comments that 3.0 already has the compat line removed. Current stable version is 3.119.3. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | 3.0.0 |

## Analysis

### Technical Summary

In 2.88.x, SKBitmap.ColorSpace delegates to Info.ColorSpace (SKImageInfo), which reads from the native sk_bitmap_get_info C API. A backwards-compatibility override was explicitly forcing ColorSpace to null during decode. This was removed in 3.0. However, detecting CMYK specifically via ColorSpace is a deeper limitation: Skia does not have a CMYK SKColorSpace representation (CreateIcc with CMYK ICC returns null as confirmed by tests), so CMYK images decoded via SKBitmap.Decode will still have a null ColorSpace in all versions — though RGB images with ICC profiles will now correctly expose their colorspace in 3.0+.

### Rationale

This is a confirmed bug in 2.88.x (explicit null override) that is fixed in 3.0+. The specific goal of CMYK detection via ColorSpace remains impossible because Skia does not model CMYK as an SKColorSpace type — CMYK JPEGs are decoded to an RGB color type internally. The issue is filed against 2.88.3 and the fix is in 3.0+, which is now stable (3.119.3).

### Key Signals

- "SKBitmap.ColorSpace always returns null" — **issue body** (In 2.88.x, an explicit backwards-compat override forces null. In 3.0+, ColorSpace reads from native correctly but CMYK images still have null ColorSpace because Skia doesn't model CMYK.)
- "I just checked 3.0 and that already has the comapt line removed" — **comment by @mattleibow (2024-05-31)** (Maintainer confirms the bug is fixed in 3.0. Reporter was waiting for 3.0 stable — it is now available as 3.119.3.)
- "You could use SKImage as well - or use SKCodec directly and SKBitmap.Decode" — **comment by @mattleibow (2024-04-29)** (Workaround suggested by maintainer for 2.88.x: use SKCodec or SKImage path instead of SKBitmap.Decode.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 279-281 | direct | SKBitmap.ColorSpace delegates to Info.ColorSpace, which is populated by SKImageInfoNative.ToManaged via sk_bitmap_get_info native call. There is no explicit null override in the current (3.x) code. |
| `binding/SkiaSharp/SKImageInfo.cs` | 27-34 | direct | SKImageInfoNative.ToManaged creates ColorSpace via SKColorSpace.GetObject(native.colorspace). If native colorspace handle is 0/null (which happens for CMYK images since Skia has no CMYK SKColorSpace), ColorSpace will be null regardless of version. |
| `binding/SkiaSharp/Definitions.cs` | 36-64 | related | SKColorType enum has no CMYK entry — Skia converts CMYK images to an RGB-based color type internally, so there is no SKColorType that represents CMYK in SkiaSharp. |
| `tests/Tests/SkiaSharp/SKColorSpaceTest.cs` | 228-239 | related | Test USWebCoatedSWOPIsUnsupportedCMYK confirms that SKColorSpace.CreateIcc with a CMYK ICC profile returns null — Skia does not support CMYK color spaces in the SKColorSpace model. |

### Workarounds

- In 2.88.x: use SKCodec.Create(stream) with SKBitmap.Decode(codec, info) instead of SKBitmap.Decode(stream) — this preserves the colorspace from the codec info.
- Upgrade to SkiaSharp 3.119.x (stable) where the backwards-compat null override is removed and ColorSpace correctly returns values for images that have colorspace information.
- For CMYK detection specifically: check JPEG markers in the raw image data (APP14 marker with 'Adobe' identifier and transform byte = 0 indicates CMYK/YCCK) — this is a lower-level approach outside SkiaSharp.

### Next Questions

- Is CMYK detection (not just colorspace access) a requirement that needs a new API in SkiaSharp, such as exposing the encoded JPEG color space markers?
- Does SKCodec.Info report different ColorType or ColorSpace for CMYK JPEGs in 3.x that might help with detection?

### Resolution Proposals

**Hypothesis:** The 2.88.x bug (explicit null override) is fixed in 3.0+. Pure CMYK detection via ColorSpace is not possible in any version since Skia does not support CMYK SKColorSpace.

1. **Upgrade to SkiaSharp 3.119.x** — fix, confidence 0.88 (88%), cost/xs, validated=untested
   - The backwards-compat null override has been removed in 3.0+. ColorSpace now returns correct values for images with color space data. Note: CMYK images will still have null ColorSpace since Skia has no CMYK representation.
2. **Use SKCodec for colorspace-preserving decode in 2.88.x** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - Use SKCodec.Create(stream) and SKBitmap.Decode(codec, info) instead of SKBitmap.Decode(stream). The maintainer confirmed this preserves colorspace.

**Recommended proposal:** Upgrade to SkiaSharp 3.119.x

**Why:** The bug is fixed in 3.0+, which is now stable. For CMYK detection, a separate approach is needed regardless of version since Skia has no CMYK colorspace model.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.82 (82%) |
| Reason | Maintainer confirmed the backwards-compat null override was removed in 3.0. Current stable is 3.119.3. The reporter was waiting for 3.0 stable which is now available. The specific reported bug (ColorSpace always null in 2.88.x) is fixed. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, SkiaSharp area, and compatibility tenet labels | labels=type/bug, area/SkiaSharp, tenet/compatibility |
| add-comment | medium | 0.82 (82%) | Inform reporter that the fix is in 3.0+, now stable as 3.119.3, and provide CMYK detection workaround | — |
| close-issue | medium | 0.78 (78%) | Close as fixed in 3.0+ (stable 3.119.3) | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! This was fixed in SkiaSharp 3.0 (now stable as **3.119.3**) — the backwards-compatibility override that forced `ColorSpace` to `null` was removed. Upgrading to 3.119.x should restore correct color space values for images that carry color space information.

**Important note on CMYK detection:** Skia does not model CMYK as an `SKColorSpace` type (CMYK ICC profiles return `null` from `SKColorSpace.CreateIcc`), so `SKBitmap.ColorSpace` will still be `null` for CMYK images in all SkiaSharp versions. This is a fundamental Skia limitation. To detect whether a JPEG is CMYK, you would need to inspect the raw JPEG data (e.g., check for the `APP14 Adobe` marker with transform byte `0x00` for CMYK/YCCK encoding) using a JPEG parsing library.

For the workaround in 2.88.x: use `SKCodec` directly with `SKBitmap.Decode(codec, info)` as @mattleibow suggested — this path does not apply the null override.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2849,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:16:27Z",
    "currentLabels": [
      "type/bug",
      "status/needs-attention"
    ]
  },
  "summary": "SKBitmap.ColorSpace always returns null in SkiaSharp 2.88.x due to an explicit backwards-compatibility override, making it impossible to detect CMYK vs RGB images via the ColorSpace API; fixed in 3.0+.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "SKBitmap.ColorSpace always returns null",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Load a JPEG image (CMYK or RGB) using SKBitmap.Decode",
        "Read SKBitmap.ColorSpace",
        "Observe that it is always null regardless of the image's actual color space"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, Visual Studio (Windows), all platforms",
      "relatedIssues": [
        523,
        1768
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/523",
          "description": "Related CMYK color skew issue (closed)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1768",
          "description": "Related: how to detect CMYK images (closed)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "Maintainer confirmed that 3.0+ removed the backwards-compatibility line that forced ColorSpace to null. Current stable is 3.119.x."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.88,
      "reason": "Maintainer @mattleibow confirmed in issue comments that 3.0 already has the compat line removed. Current stable version is 3.119.3.",
      "fixedInVersion": "3.0.0"
    }
  },
  "analysis": {
    "summary": "In 2.88.x, SKBitmap.ColorSpace delegates to Info.ColorSpace (SKImageInfo), which reads from the native sk_bitmap_get_info C API. A backwards-compatibility override was explicitly forcing ColorSpace to null during decode. This was removed in 3.0. However, detecting CMYK specifically via ColorSpace is a deeper limitation: Skia does not have a CMYK SKColorSpace representation (CreateIcc with CMYK ICC returns null as confirmed by tests), so CMYK images decoded via SKBitmap.Decode will still have a null ColorSpace in all versions — though RGB images with ICC profiles will now correctly expose their colorspace in 3.0+.",
    "rationale": "This is a confirmed bug in 2.88.x (explicit null override) that is fixed in 3.0+. The specific goal of CMYK detection via ColorSpace remains impossible because Skia does not model CMYK as an SKColorSpace type — CMYK JPEGs are decoded to an RGB color type internally. The issue is filed against 2.88.3 and the fix is in 3.0+, which is now stable (3.119.3).",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "279-281",
        "finding": "SKBitmap.ColorSpace delegates to Info.ColorSpace, which is populated by SKImageInfoNative.ToManaged via sk_bitmap_get_info native call. There is no explicit null override in the current (3.x) code.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImageInfo.cs",
        "lines": "27-34",
        "finding": "SKImageInfoNative.ToManaged creates ColorSpace via SKColorSpace.GetObject(native.colorspace). If native colorspace handle is 0/null (which happens for CMYK images since Skia has no CMYK SKColorSpace), ColorSpace will be null regardless of version.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "36-64",
        "finding": "SKColorType enum has no CMYK entry — Skia converts CMYK images to an RGB-based color type internally, so there is no SKColorType that represents CMYK in SkiaSharp.",
        "relevance": "related"
      },
      {
        "file": "tests/Tests/SkiaSharp/SKColorSpaceTest.cs",
        "lines": "228-239",
        "finding": "Test USWebCoatedSWOPIsUnsupportedCMYK confirms that SKColorSpace.CreateIcc with a CMYK ICC profile returns null — Skia does not support CMYK color spaces in the SKColorSpace model.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "SKBitmap.ColorSpace always returns null",
        "source": "issue body",
        "interpretation": "In 2.88.x, an explicit backwards-compat override forces null. In 3.0+, ColorSpace reads from native correctly but CMYK images still have null ColorSpace because Skia doesn't model CMYK."
      },
      {
        "text": "I just checked 3.0 and that already has the comapt line removed",
        "source": "comment by @mattleibow (2024-05-31)",
        "interpretation": "Maintainer confirms the bug is fixed in 3.0. Reporter was waiting for 3.0 stable — it is now available as 3.119.3."
      },
      {
        "text": "You could use SKImage as well - or use SKCodec directly and SKBitmap.Decode",
        "source": "comment by @mattleibow (2024-04-29)",
        "interpretation": "Workaround suggested by maintainer for 2.88.x: use SKCodec or SKImage path instead of SKBitmap.Decode."
      }
    ],
    "workarounds": [
      "In 2.88.x: use SKCodec.Create(stream) with SKBitmap.Decode(codec, info) instead of SKBitmap.Decode(stream) — this preserves the colorspace from the codec info.",
      "Upgrade to SkiaSharp 3.119.x (stable) where the backwards-compat null override is removed and ColorSpace correctly returns values for images that have colorspace information.",
      "For CMYK detection specifically: check JPEG markers in the raw image data (APP14 marker with 'Adobe' identifier and transform byte = 0 indicates CMYK/YCCK) — this is a lower-level approach outside SkiaSharp."
    ],
    "resolution": {
      "hypothesis": "The 2.88.x bug (explicit null override) is fixed in 3.0+. Pure CMYK detection via ColorSpace is not possible in any version since Skia does not support CMYK SKColorSpace.",
      "proposals": [
        {
          "title": "Upgrade to SkiaSharp 3.119.x",
          "description": "The backwards-compat null override has been removed in 3.0+. ColorSpace now returns correct values for images with color space data. Note: CMYK images will still have null ColorSpace since Skia has no CMYK representation.",
          "category": "fix",
          "confidence": 0.88,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Use SKCodec for colorspace-preserving decode in 2.88.x",
          "description": "Use SKCodec.Create(stream) and SKBitmap.Decode(codec, info) instead of SKBitmap.Decode(stream). The maintainer confirmed this preserves colorspace.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Upgrade to SkiaSharp 3.119.x",
      "recommendedReason": "The bug is fixed in 3.0+, which is now stable. For CMYK detection, a separate approach is needed regardless of version since Skia has no CMYK colorspace model."
    },
    "nextQuestions": [
      "Is CMYK detection (not just colorspace access) a requirement that needs a new API in SkiaSharp, such as exposing the encoded JPEG color space markers?",
      "Does SKCodec.Info report different ColorType or ColorSpace for CMYK JPEGs in 3.x that might help with detection?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.82,
      "reason": "Maintainer confirmed the backwards-compat null override was removed in 3.0. Current stable is 3.119.3. The reporter was waiting for 3.0 stable which is now available. The specific reported bug (ColorSpace always null in 2.88.x) is fixed.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, SkiaSharp area, and compatibility tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Inform reporter that the fix is in 3.0+, now stable as 3.119.3, and provide CMYK detection workaround",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the report! This was fixed in SkiaSharp 3.0 (now stable as **3.119.3**) — the backwards-compatibility override that forced `ColorSpace` to `null` was removed. Upgrading to 3.119.x should restore correct color space values for images that carry color space information.\n\n**Important note on CMYK detection:** Skia does not model CMYK as an `SKColorSpace` type (CMYK ICC profiles return `null` from `SKColorSpace.CreateIcc`), so `SKBitmap.ColorSpace` will still be `null` for CMYK images in all SkiaSharp versions. This is a fundamental Skia limitation. To detect whether a JPEG is CMYK, you would need to inspect the raw JPEG data (e.g., check for the `APP14 Adobe` marker with transform byte `0x00` for CMYK/YCCK encoding) using a JPEG parsing library.\n\nFor the workaround in 2.88.x: use `SKCodec` directly with `SKBitmap.Decode(codec, info)` as @mattleibow suggested — this path does not apply the null override."
      },
      {
        "type": "close-issue",
        "description": "Close as fixed in 3.0+ (stable 3.119.3)",
        "risk": "medium",
        "confidence": 0.78,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
