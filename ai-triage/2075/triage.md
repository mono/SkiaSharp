# Issue Triage Report — #2075

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T19:33:00Z |
| Type | type/enhancement (0.98 (98%)) |
| Area | area/SkiaSharp (0.85 (85%)) |
| Suggested action | keep-open (0.90 (90%)) |

**Issue Summary:** Enhancement request to enable C# Nullable Reference Types annotations in the SkiaSharp.Vulkan.SharpVk package, filed as part of the nullable epic #2055 tracking all SkiaSharp packages.

**Analysis:** The SkiaSharp.Vulkan.SharpVk package does not yet have Nullable Reference Types enabled. The main SkiaSharp.csproj already has <Nullable>enable</Nullable>, but the Vulkan.SharpVk.csproj does not. This is part of a broad nullable-annotation epic (#2055) covering all packages.

**Recommendations:** **keep-open** — Valid, well-scoped enhancement filed by maintainer as part of a planned epic. Already marked long-term — no immediate action required beyond tracking.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | type/enhancement, status/long-term |

## Evidence

### Reproduction

**Related issues:** #2055, #2057, #2058, #2073, #2074

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2055 — Parent nullable epic tracking all packages

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The SkiaSharp.Vulkan.SharpVk.csproj still lacks <Nullable>enable</Nullable> as of the current codebase. |

## Analysis

### Technical Summary

The SkiaSharp.Vulkan.SharpVk package does not yet have Nullable Reference Types enabled. The main SkiaSharp.csproj already has <Nullable>enable</Nullable>, but the Vulkan.SharpVk.csproj does not. This is part of a broad nullable-annotation epic (#2055) covering all packages.

### Rationale

This is a clear, well-scoped enhancement request filed by the maintainer as part of a planned nullable annotation epic. The fix is well-understood: add <Nullable>enable</Nullable> to the csproj and annotate public APIs. The status/long-term label indicates it is acknowledged but not yet scheduled.

### Key Signals

- "Enable Nullable Reference Types (SkiaSharp.Vulkan.SharpVk)" — **issue title** (Explicit request to add NRT support to the Vulkan.SharpVk package; part of the cross-package nullable epic.)
- "Part of epic #2055 tracking nullable annotations across all packages" — **related issues search** (This is a tracked, intentional long-term improvement, not an ad-hoc request.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Vulkan/SkiaSharp.Vulkan.SharpVk/SkiaSharp.Vulkan.SharpVk.csproj` | — | direct | The project file does not contain <Nullable>enable</Nullable>; nullable reference type annotations are not yet enabled for this package. |
| `binding/SkiaSharp/SkiaSharp.csproj` | — | context | The main SkiaSharp project already has <Nullable>enable</Nullable> on line 7, demonstrating the pattern to apply. |
| `source/SkiaSharp.Vulkan/SkiaSharp.Vulkan.SharpVk/GRSharpVkBackendContext.cs` | — | direct | Key public API class — fields like vkInstance, vkDevice, getProc would need nullable annotations (e.g., Instance?, Device?, GRSharpVkGetProcedureAddressDelegate?). |

### Workarounds

- Users who need nullable-aware bindings can wrap the SharpVk APIs in their own nullable-annotated adapter layer.

### Resolution Proposals

**Hypothesis:** Adding <Nullable>enable</Nullable> to SkiaSharp.Vulkan.SharpVk.csproj and annotating public API types (nullable fields, return types, parameters) will satisfy the request.

1. **Enable nullable in csproj and annotate APIs** — fix, confidence 0.92 (92%), cost/s, validated=untested
   - Add <Nullable>enable</Nullable> to SkiaSharp.Vulkan.SharpVk.csproj, then annotate public members in GRSharpVkBackendContext.cs and GRVkExtensionsSharpVkExtensions.cs with ? where nullable, and ensure non-nullable fields have proper initialization.

**Recommended proposal:** Enable nullable in csproj and annotate APIs

**Why:** Straightforward change matching the pattern already applied to the main SkiaSharp package.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.90 (90%) |
| Reason | Valid, well-scoped enhancement filed by maintainer as part of a planned epic. Already marked long-term — no immediate action required beyond tracking. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Confirm type/enhancement and area/SkiaSharp labels | labels=type/enhancement, area/SkiaSharp |
| link-related | low | 0.95 (95%) | Cross-reference parent nullable epic | linkedIssue=#2055 |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2075,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T19:33:00Z",
    "currentLabels": [
      "type/enhancement",
      "status/long-term"
    ]
  },
  "summary": "Enhancement request to enable C# Nullable Reference Types annotations in the SkiaSharp.Vulkan.SharpVk package, filed as part of the nullable epic #2055 tracking all SkiaSharp packages.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.98
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.85
    }
  },
  "evidence": {
    "reproEvidence": {
      "relatedIssues": [
        2055,
        2057,
        2058,
        2073,
        2074
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2055",
          "description": "Parent nullable epic tracking all packages"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "The SkiaSharp.Vulkan.SharpVk.csproj still lacks <Nullable>enable</Nullable> as of the current codebase."
    }
  },
  "analysis": {
    "summary": "The SkiaSharp.Vulkan.SharpVk package does not yet have Nullable Reference Types enabled. The main SkiaSharp.csproj already has <Nullable>enable</Nullable>, but the Vulkan.SharpVk.csproj does not. This is part of a broad nullable-annotation epic (#2055) covering all packages.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Vulkan/SkiaSharp.Vulkan.SharpVk/SkiaSharp.Vulkan.SharpVk.csproj",
        "finding": "The project file does not contain <Nullable>enable</Nullable>; nullable reference type annotations are not yet enabled for this package.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaSharp.csproj",
        "finding": "The main SkiaSharp project already has <Nullable>enable</Nullable> on line 7, demonstrating the pattern to apply.",
        "relevance": "context"
      },
      {
        "file": "source/SkiaSharp.Vulkan/SkiaSharp.Vulkan.SharpVk/GRSharpVkBackendContext.cs",
        "finding": "Key public API class — fields like vkInstance, vkDevice, getProc would need nullable annotations (e.g., Instance?, Device?, GRSharpVkGetProcedureAddressDelegate?).",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Enable Nullable Reference Types (SkiaSharp.Vulkan.SharpVk)",
        "source": "issue title",
        "interpretation": "Explicit request to add NRT support to the Vulkan.SharpVk package; part of the cross-package nullable epic."
      },
      {
        "text": "Part of epic #2055 tracking nullable annotations across all packages",
        "source": "related issues search",
        "interpretation": "This is a tracked, intentional long-term improvement, not an ad-hoc request."
      }
    ],
    "rationale": "This is a clear, well-scoped enhancement request filed by the maintainer as part of a planned nullable annotation epic. The fix is well-understood: add <Nullable>enable</Nullable> to the csproj and annotate public APIs. The status/long-term label indicates it is acknowledged but not yet scheduled.",
    "workarounds": [
      "Users who need nullable-aware bindings can wrap the SharpVk APIs in their own nullable-annotated adapter layer."
    ],
    "resolution": {
      "hypothesis": "Adding <Nullable>enable</Nullable> to SkiaSharp.Vulkan.SharpVk.csproj and annotating public API types (nullable fields, return types, parameters) will satisfy the request.",
      "proposals": [
        {
          "title": "Enable nullable in csproj and annotate APIs",
          "description": "Add <Nullable>enable</Nullable> to SkiaSharp.Vulkan.SharpVk.csproj, then annotate public members in GRSharpVkBackendContext.cs and GRVkExtensionsSharpVkExtensions.cs with ? where nullable, and ensure non-nullable fields have proper initialization.",
          "category": "fix",
          "confidence": 0.92,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Enable nullable in csproj and annotate APIs",
      "recommendedReason": "Straightforward change matching the pattern already applied to the main SkiaSharp package."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.9,
      "reason": "Valid, well-scoped enhancement filed by maintainer as part of a planned epic. Already marked long-term — no immediate action required beyond tracking.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Confirm type/enhancement and area/SkiaSharp labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference parent nullable epic",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 2055
      }
    ]
  }
}
```

</details>
