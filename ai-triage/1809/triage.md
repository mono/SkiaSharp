# Issue Triage Report — #1809

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-05T10:55:41Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-fixed (0.93 (93%)) |

**Issue Summary:** Feature request to add `kSRGBA_8888_SkColorType` to `SKColorType` enum for correct alpha blending on sRGB GPU surfaces — already implemented as `SKColorType.Srgba8888`.

**Analysis:** The request was to expose `kSRGBA_8888_SkColorType` from Skia as a new `SKColorType` enum value. Investigation confirms `SKColorType.Srgba8888 = 22` already exists in `binding/SkiaSharp/Definitions.cs`, the native enum constant `SRGBA_8888_SK_COLORTYPE = 26` is generated in `SkiaApi.generated.cs`, and both `SKColorType.Srgba8888 → SKColorTypeNative.Srgba8888` and the reverse mapping are present in `EnumMappings.cs`. The feature is fully implemented.

**Recommendations:** **close-as-fixed** — SKColorType.Srgba8888 is already present in the codebase with a complete native mapping. The requested feature has been delivered.

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

## Evidence

### Reproduction

**Repository links:**
- https://skia.googlesource.com/skia/+/0f7c10ef5681d5c739bcd8862c58d856fd663a0c — Upstream Skia commit adding kSRGBA_8888_SkColorType referenced in the issue
- https://groups.google.com/g/skia-discuss/c/gi_WWpnOJno/m/Jr8L1pYDBwAJ — Skia forum discussion on the topic referenced in the issue

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | SKColorType.Srgba8888 (value 22) is already present in Definitions.cs and fully mapped in EnumMappings.cs |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.97 (97%) |
| Reason | SKColorType.Srgba8888 is present in the enum at value 22, the C native mapping SRGBA_8888_SK_COLORTYPE is generated, and the forward/reverse mappings in EnumMappings.cs are complete. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | unknown |

## Analysis

### Technical Summary

The request was to expose `kSRGBA_8888_SkColorType` from Skia as a new `SKColorType` enum value. Investigation confirms `SKColorType.Srgba8888 = 22` already exists in `binding/SkiaSharp/Definitions.cs`, the native enum constant `SRGBA_8888_SK_COLORTYPE = 26` is generated in `SkiaApi.generated.cs`, and both `SKColorType.Srgba8888 → SKColorTypeNative.Srgba8888` and the reverse mapping are present in `EnumMappings.cs`. The feature is fully implemented.

### Rationale

Classified as type/feature-request (not type/bug) because the reporter is asking for a new enum value that was absent at the time of filing. Code investigation confirms the feature was subsequently implemented: `Srgba8888` appears in the enum, in the native mapping table, and in generated API code. suggestedAction is close-as-fixed.

### Key Signals

- "Update to latest Skia and update the `SKColorType` enum to include the new option `kSRGBA_8888_SkColorType`" — **issue body** (Requested feature is now present as SKColorType.Srgba8888 = 22)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/Definitions.cs` | 36-66 | direct | SKColorType enum contains `Srgba8888 = 22`. |
| `binding/SkiaSharp/EnumMappings.cs` | 69,103 | direct | Bidirectional mapping `SKColorType.Srgba8888 <-> SKColorTypeNative.Srgba8888` is present. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 21022 | direct | Generated native constant `SRGBA_8888_SK_COLORTYPE = 26` confirms C-level binding exists. |

### Resolution Proposals

**Hypothesis:** The requested `kSRGBA_8888_SkColorType` was added to `SKColorType` as `Srgba8888 = 22` in a subsequent SkiaSharp update; the issue can be closed as fixed.

1. **Inform reporter that the feature has been implemented** — fix, cost/xs, validated=untested
   - Comment on the issue confirming SKColorType.Srgba8888 is available and suggest the reporter verify with the latest NuGet package.

**Recommended proposal:** comment-1

**Why:** Closing with a friendly comment confirming the implementation is the correct action for a feature that has already shipped.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.93 (93%) |
| Reason | SKColorType.Srgba8888 is already present in the codebase with a complete native mapping. The requested feature has been delivered. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply type/feature-request, area/SkiaSharp labels | labels=type/feature-request, area/SkiaSharp |
| add-comment | medium | 0.93 (93%) | Notify reporter that the feature is now available as SKColorType.Srgba8888 | — |
| close-issue | medium | 0.93 (93%) | Close as fixed — the requested SKColorType.Srgba8888 is implemented | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Hi @tebjan 👋

Good news — the feature you requested has already been implemented! `SKColorType.Srgba8888` (mapping to Skia's `kSRGBA_8888_SkColorType`) is available in the current version of SkiaSharp.

You can use it like any other color type:
```csharp
var info = new SKImageInfo(width, height, SKColorType.Srgba8888, SKAlphaType.Premul);
```

Please update to the latest NuGet package and let us know if this resolves your use case. Closing as fixed.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1809,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-05T10:55:41Z"
  },
  "summary": "Feature request to add `kSRGBA_8888_SkColorType` to `SKColorType` enum for correct alpha blending on sRGB GPU surfaces — already implemented as `SKColorType.Srgba8888`.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    }
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://skia.googlesource.com/skia/+/0f7c10ef5681d5c739bcd8862c58d856fd663a0c",
          "description": "Upstream Skia commit adding kSRGBA_8888_SkColorType referenced in the issue"
        },
        {
          "url": "https://groups.google.com/g/skia-discuss/c/gi_WWpnOJno/m/Jr8L1pYDBwAJ",
          "description": "Skia forum discussion on the topic referenced in the issue"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "unlikely",
      "relevanceReason": "SKColorType.Srgba8888 (value 22) is already present in Definitions.cs and fully mapped in EnumMappings.cs"
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.97,
      "reason": "SKColorType.Srgba8888 is present in the enum at value 22, the C native mapping SRGBA_8888_SK_COLORTYPE is generated, and the forward/reverse mappings in EnumMappings.cs are complete.",
      "fixedInVersion": "unknown"
    }
  },
  "analysis": {
    "summary": "The request was to expose `kSRGBA_8888_SkColorType` from Skia as a new `SKColorType` enum value. Investigation confirms `SKColorType.Srgba8888 = 22` already exists in `binding/SkiaSharp/Definitions.cs`, the native enum constant `SRGBA_8888_SK_COLORTYPE = 26` is generated in `SkiaApi.generated.cs`, and both `SKColorType.Srgba8888 → SKColorTypeNative.Srgba8888` and the reverse mapping are present in `EnumMappings.cs`. The feature is fully implemented.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "36-66",
        "finding": "SKColorType enum contains `Srgba8888 = 22`.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/EnumMappings.cs",
        "lines": "69,103",
        "finding": "Bidirectional mapping `SKColorType.Srgba8888 <-> SKColorTypeNative.Srgba8888` is present.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "21022",
        "finding": "Generated native constant `SRGBA_8888_SK_COLORTYPE = 26` confirms C-level binding exists.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Update to latest Skia and update the `SKColorType` enum to include the new option `kSRGBA_8888_SkColorType`",
        "source": "issue body",
        "interpretation": "Requested feature is now present as SKColorType.Srgba8888 = 22"
      }
    ],
    "rationale": "Classified as type/feature-request (not type/bug) because the reporter is asking for a new enum value that was absent at the time of filing. Code investigation confirms the feature was subsequently implemented: `Srgba8888` appears in the enum, in the native mapping table, and in generated API code. suggestedAction is close-as-fixed.",
    "resolution": {
      "hypothesis": "The requested `kSRGBA_8888_SkColorType` was added to `SKColorType` as `Srgba8888 = 22` in a subsequent SkiaSharp update; the issue can be closed as fixed.",
      "proposals": [
        {
          "title": "Inform reporter that the feature has been implemented",
          "category": "fix",
          "effort": "cost/xs",
          "validated": "untested",
          "description": "Comment on the issue confirming SKColorType.Srgba8888 is available and suggest the reporter verify with the latest NuGet package."
        }
      ],
      "recommendedProposal": "comment-1",
      "recommendedReason": "Closing with a friendly comment confirming the implementation is the correct action for a feature that has already shipped."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.93,
      "reason": "SKColorType.Srgba8888 is already present in the codebase with a complete native mapping. The requested feature has been delivered.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/feature-request, area/SkiaSharp labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Notify reporter that the feature is now available as SKColorType.Srgba8888",
        "risk": "medium",
        "confidence": 0.93,
        "comment": "Hi @tebjan 👋\n\nGood news — the feature you requested has already been implemented! `SKColorType.Srgba8888` (mapping to Skia's `kSRGBA_8888_SkColorType`) is available in the current version of SkiaSharp.\n\nYou can use it like any other color type:\n```csharp\nvar info = new SKImageInfo(width, height, SKColorType.Srgba8888, SKAlphaType.Premul);\n```\n\nPlease update to the latest NuGet package and let us know if this resolves your use case. Closing as fixed."
      },
      {
        "type": "close-issue",
        "description": "Close as fixed — the requested SKColorType.Srgba8888 is implemented",
        "risk": "medium",
        "confidence": 0.93,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
