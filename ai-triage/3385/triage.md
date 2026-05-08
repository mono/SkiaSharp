# Issue Triage Report — #3385

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T21:02:24Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | close-as-external (0.92 (92%)) |

**Issue Summary:** Reporter requests SKColorType.Gray16 to support 16-bit grayscale PNGs, but the feature cannot be added because upstream Skia does not define a Gray16 color type.

**Analysis:** SKColorType.Gray16 cannot be added to SkiaSharp because upstream Skia has no kGray_16 color type. The SkiaSharp SKColorType enum is a 1:1 mapping of the native Skia sk_colortype_t enum; any new color type must first exist in upstream Skia before it can be bound in SkiaSharp.

**Recommendations:** **close-as-external** — The requested SKColorType.Gray16 does not exist in upstream Skia. SkiaSharp can only expose color types that the Skia C API defines. This feature requires an upstream Skia change first.

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

**Environment:** No platform specified; request is cross-platform

**Repository links:**
- https://api.skia.org/SkColorType_8h.html — Upstream Skia SkColorType enum — no kGray_16 entry exists
- https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolortype — SkiaSharp SKColorType docs referenced by reporter — Gray16 absent

## Analysis

### Technical Summary

SKColorType.Gray16 cannot be added to SkiaSharp because upstream Skia has no kGray_16 color type. The SkiaSharp SKColorType enum is a 1:1 mapping of the native Skia sk_colortype_t enum; any new color type must first exist in upstream Skia before it can be bound in SkiaSharp.

### Rationale

The request is for a new feature (Gray16 color type) that does not exist anywhere in the stack: not in SkiaSharp's C# enum, not in the generated C API (SKColorTypeNative), and not in upstream Skia. A community comment on the issue confirms Skia has no Gray16 upstream. This is therefore not a SkiaSharp implementation gap — it is an upstream limitation, warranting close-as-external.

### Key Signals

- "i want to use 16bit grayscale pngs but it looks like it doesnt exist in skiasharp" — **issue body** (Clear feature gap request — reporter has a real use case (16-bit grayscale PNG I/O).)
- "There's no Gray16 support upstream: https://api.skia.org/SkColorType_8h.html" — **comment by molesmoke** (Confirms the root cause: upstream Skia lacks this color type, so SkiaSharp cannot expose it.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/Definitions.cs` | 36-65 | direct | SKColorType enum lists Gray8 (=9) as the only grayscale format. No Gray16 entry exists. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 20575-20614 | direct | Generated SKColorTypeNative enum (wrapping the C API sk_colortype_t) has Gray8 (=14) but no Gray16 entry. This confirms the native C API does not expose a Gray16 type. |
| `binding/SkiaSharp/EnumMappings.cs` | 43-100 | related | ToNative/FromNative mapping functions handle Gray8 but have no case for Gray16, consistent with the absence of the native enum value. |

### Workarounds

- Decode the 16-bit grayscale PNG with a library that supports it (e.g., ImageSharp or libpng via P/Invoke), then convert each pixel by shifting to 8-bit and create an SKBitmap with SKColorType.Gray8 — lossy but functional for display.
- Store data as SKColorType.Rgba16161616 by duplicating the gray channel into R, G, and B channels and setting full alpha — preserves 16-bit precision but wastes memory.
- Use SKColorType.Alpha16 as a single-channel 16-bit proxy if the pipeline only needs to read the raw pixel values and does not require color operations.

### Resolution Proposals

**Hypothesis:** Gray16 is absent from upstream Skia and therefore cannot be added to SkiaSharp bindings without first filing a feature request with the Skia team (https://bugs.chromium.org/p/skia).

1. **Downconvert to Gray8** — workaround, confidence 0.90 (90%), cost/s, validated=untested
   - Load the PNG externally, map the 16-bit values to 8-bit (val >> 8), and create an SKBitmap with SKColorType.Gray8. Simple and broadly supported, but discards the lower 8 bits.
2. **Store grayscale data as Rgba16161616** — workaround, confidence 0.80 (80%), cost/m, validated=untested
   - Expand each 16-bit gray value into RGBA by copying gray → R=G=B and alpha=65535. Preserves full precision at the cost of 4× memory. Use SKColorType.Rgba16161616.
3. **Request Gray16 in upstream Skia** — alternative, confidence 0.85 (85%), cost/xl, validated=untested
   - File a feature request at https://bugs.chromium.org/p/skia so upstream adds kGray_16. Once upstream supports it, SkiaSharp can expose it through the normal binding process.

**Recommended proposal:** Downconvert to Gray8

**Why:** Least effort for most use cases. If 16-bit precision is truly required, the Rgba16161616 approach preserves it.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.92 (92%) |
| Reason | The requested SKColorType.Gray16 does not exist in upstream Skia. SkiaSharp can only expose color types that the Skia C API defines. This feature requires an upstream Skia change first. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Confirm the existing type/feature-request label and add area/SkiaSharp | labels=type/feature-request, area/SkiaSharp |
| add-comment | high | 0.92 (92%) | Explain that Gray16 is absent from upstream Skia and provide workarounds | — |
| close-issue | medium | 0.88 (88%) | Close as external — feature blocked by upstream Skia | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the request. Unfortunately `SKColorType.Gray16` cannot be added to SkiaSharp because it does not exist in upstream Skia — the library SkiaSharp wraps. SkiaSharp's `SKColorType` enum is a direct mapping of Skia's `SkColorType` enum, and [Skia has no `kGray_16` entry](https://api.skia.org/SkColorType_8h.html).

Until Skia adds Gray16 support, here are two workarounds:

**Option 1 — Downconvert to 8-bit (simple, lossy):** Load the PNG with another library, shift each 16-bit value right by 8, and create an `SKBitmap` using `SKColorType.Gray8`.

**Option 2 — Preserve 16-bit precision via RGBA:** Expand each 16-bit gray value into an RGBA pixel (`R=G=B=grayValue`, `A=65535`) and use `SKColorType.Rgba16161616`. This keeps full precision at 4× memory cost.

If this is important to your use case, please consider filing a feature request with the [Skia team](https://bugs.chromium.org/p/skia) to add `kGray_16` upstream. Once it exists in Skia, we can expose it in SkiaSharp.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3385,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T21:02:24Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "Reporter requests SKColorType.Gray16 to support 16-bit grayscale PNGs, but the feature cannot be added because upstream Skia does not define a Gray16 color type.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    }
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "No platform specified; request is cross-platform",
      "repoLinks": [
        {
          "url": "https://api.skia.org/SkColorType_8h.html",
          "description": "Upstream Skia SkColorType enum — no kGray_16 entry exists"
        },
        {
          "url": "https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolortype",
          "description": "SkiaSharp SKColorType docs referenced by reporter — Gray16 absent"
        }
      ]
    }
  },
  "analysis": {
    "summary": "SKColorType.Gray16 cannot be added to SkiaSharp because upstream Skia has no kGray_16 color type. The SkiaSharp SKColorType enum is a 1:1 mapping of the native Skia sk_colortype_t enum; any new color type must first exist in upstream Skia before it can be bound in SkiaSharp.",
    "rationale": "The request is for a new feature (Gray16 color type) that does not exist anywhere in the stack: not in SkiaSharp's C# enum, not in the generated C API (SKColorTypeNative), and not in upstream Skia. A community comment on the issue confirms Skia has no Gray16 upstream. This is therefore not a SkiaSharp implementation gap — it is an upstream limitation, warranting close-as-external.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "36-65",
        "finding": "SKColorType enum lists Gray8 (=9) as the only grayscale format. No Gray16 entry exists.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "20575-20614",
        "finding": "Generated SKColorTypeNative enum (wrapping the C API sk_colortype_t) has Gray8 (=14) but no Gray16 entry. This confirms the native C API does not expose a Gray16 type.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/EnumMappings.cs",
        "lines": "43-100",
        "finding": "ToNative/FromNative mapping functions handle Gray8 but have no case for Gray16, consistent with the absence of the native enum value.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "i want to use 16bit grayscale pngs but it looks like it doesnt exist in skiasharp",
        "source": "issue body",
        "interpretation": "Clear feature gap request — reporter has a real use case (16-bit grayscale PNG I/O)."
      },
      {
        "text": "There's no Gray16 support upstream: https://api.skia.org/SkColorType_8h.html",
        "source": "comment by molesmoke",
        "interpretation": "Confirms the root cause: upstream Skia lacks this color type, so SkiaSharp cannot expose it."
      }
    ],
    "workarounds": [
      "Decode the 16-bit grayscale PNG with a library that supports it (e.g., ImageSharp or libpng via P/Invoke), then convert each pixel by shifting to 8-bit and create an SKBitmap with SKColorType.Gray8 — lossy but functional for display.",
      "Store data as SKColorType.Rgba16161616 by duplicating the gray channel into R, G, and B channels and setting full alpha — preserves 16-bit precision but wastes memory.",
      "Use SKColorType.Alpha16 as a single-channel 16-bit proxy if the pipeline only needs to read the raw pixel values and does not require color operations."
    ],
    "resolution": {
      "hypothesis": "Gray16 is absent from upstream Skia and therefore cannot be added to SkiaSharp bindings without first filing a feature request with the Skia team (https://bugs.chromium.org/p/skia).",
      "proposals": [
        {
          "title": "Downconvert to Gray8",
          "description": "Load the PNG externally, map the 16-bit values to 8-bit (val >> 8), and create an SKBitmap with SKColorType.Gray8. Simple and broadly supported, but discards the lower 8 bits.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Store grayscale data as Rgba16161616",
          "description": "Expand each 16-bit gray value into RGBA by copying gray → R=G=B and alpha=65535. Preserves full precision at the cost of 4× memory. Use SKColorType.Rgba16161616.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Request Gray16 in upstream Skia",
          "description": "File a feature request at https://bugs.chromium.org/p/skia so upstream adds kGray_16. Once upstream supports it, SkiaSharp can expose it through the normal binding process.",
          "category": "alternative",
          "confidence": 0.85,
          "effort": "cost/xl",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Downconvert to Gray8",
      "recommendedReason": "Least effort for most use cases. If 16-bit precision is truly required, the Rgba16161616 approach preserves it."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.92,
      "reason": "The requested SKColorType.Gray16 does not exist in upstream Skia. SkiaSharp can only expose color types that the Skia C API defines. This feature requires an upstream Skia change first.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Confirm the existing type/feature-request label and add area/SkiaSharp",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain that Gray16 is absent from upstream Skia and provide workarounds",
        "risk": "high",
        "confidence": 0.92,
        "comment": "Thanks for the request. Unfortunately `SKColorType.Gray16` cannot be added to SkiaSharp because it does not exist in upstream Skia — the library SkiaSharp wraps. SkiaSharp's `SKColorType` enum is a direct mapping of Skia's `SkColorType` enum, and [Skia has no `kGray_16` entry](https://api.skia.org/SkColorType_8h.html).\n\nUntil Skia adds Gray16 support, here are two workarounds:\n\n**Option 1 — Downconvert to 8-bit (simple, lossy):** Load the PNG with another library, shift each 16-bit value right by 8, and create an `SKBitmap` using `SKColorType.Gray8`.\n\n**Option 2 — Preserve 16-bit precision via RGBA:** Expand each 16-bit gray value into an RGBA pixel (`R=G=B=grayValue`, `A=65535`) and use `SKColorType.Rgba16161616`. This keeps full precision at 4× memory cost.\n\nIf this is important to your use case, please consider filing a feature request with the [Skia team](https://bugs.chromium.org/p/skia) to add `kGray_16` upstream. Once it exists in Skia, we can expose it in SkiaSharp."
      },
      {
        "type": "close-issue",
        "description": "Close as external — feature blocked by upstream Skia",
        "risk": "medium",
        "confidence": 0.88,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
