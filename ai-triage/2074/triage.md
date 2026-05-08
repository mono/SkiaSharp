# Issue Triage Report — #2074

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T19:03:59Z |
| Type | type/enhancement (0.98 (98%)) |
| Area | area/SkiaSharp.HarfBuzz (0.99 (99%)) |
| Suggested action | needs-investigation (0.90 (90%)) |

**Issue Summary:** Enable Nullable Reference Types (NRTs) in the SkiaSharp.HarfBuzz package, adding nullable annotations to all public APIs.

**Analysis:** SkiaSharp.HarfBuzz (source/SkiaSharp.HarfBuzz/) lacks Nullable Reference Types annotations. The csproj has no <Nullable>enable</Nullable> property and source files do not include nullable annotations. This is a tracked sub-task of the NRT epic (#2055) filed by the maintainer.

**Recommendations:** **needs-investigation** — Well-scoped maintainer-filed enhancement with clear implementation path; needs a developer to do the annotation work.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.HarfBuzz |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2055 — Parent epic: Enable Nullable Reference Types across all SkiaSharp packages
- https://github.com/mono/SkiaSharp/issues/2073 — Sibling issue: Enable NRTs in HarfBuzzSharp
- https://github.com/mono/SkiaSharp/issues/2057 — Sibling issue: Enable NRTs in SkiaSharp core

## Analysis

### Technical Summary

SkiaSharp.HarfBuzz (source/SkiaSharp.HarfBuzz/) lacks Nullable Reference Types annotations. The csproj has no <Nullable>enable</Nullable> property and source files do not include nullable annotations. This is a tracked sub-task of the NRT epic (#2055) filed by the maintainer.

### Rationale

This is a maintainer-filed enhancement clearly scoped to enabling NRTs in the SkiaSharp.HarfBuzz package. The issue is part of the NRT epic (#2055). Code investigation confirms NRTs are not yet enabled in SkiaSharp.HarfBuzz. The enhancement is low-risk, follows an established pattern from the SkiaSharp core package, and requires annotation work in a small set of files.

### Key Signals

- "Enable Nullable Reference Types (SkiaSharp.HarfBuzz)" — **issue title** (Maintainer-filed enhancement to add NRT support to this package as part of the broader NRT initiative)
- "Part of epic #2055 (Enable Nullable Reference Types), listed as checklist item" — **issue #2055 body** (Well-defined scope, tracked work item with clear implementation path)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz.csproj` | — | direct | No <Nullable> property present in the project file; nullable reference types are not enabled for this package. |
| `source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/SKShaper.cs` | — | direct | No #nullable enable or nullable annotations present in source files; method parameters and return types lack nullability annotations. |
| `binding/SkiaSharp/SkiaSharp.csproj` | — | context | SkiaSharp core binding has <Nullable>enable</Nullable> set, demonstrating the pattern to follow for SkiaSharp.HarfBuzz. |

### Resolution Proposals

**Hypothesis:** Enable <Nullable>enable</Nullable> in SkiaSharp.HarfBuzz.csproj and add nullable annotations to all public APIs in SKShaper.cs, BlobExtensions.cs, CanvasExtensions.cs, and FontExtensions.cs.

1. **Add Nullable enable to SkiaSharp.HarfBuzz.csproj and annotate source files** — fix, cost/m, validated=untested
   - Add <Nullable>enable</Nullable> to the SkiaSharp.HarfBuzz.csproj PropertyGroup. Then annotate all public APIs in SKShaper.cs, BlobExtensions.cs, CanvasExtensions.cs, and FontExtensions.cs with nullable reference type annotations.

**Recommended proposal:** proposal-1

**Why:** This is the standard NRT enablement pattern used across other SkiaSharp packages.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.90 (90%) |
| Reason | Well-scoped maintainer-filed enhancement with clear implementation path; needs a developer to do the annotation work. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.99 (99%) | Apply enhancement and area labels | labels=type/enhancement, area/SkiaSharp.HarfBuzz, tenet/compatibility, triage/triaged |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2074,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T19:03:59Z"
  },
  "summary": "Enable Nullable Reference Types (NRTs) in the SkiaSharp.HarfBuzz package, adding nullable annotations to all public APIs.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.98
    },
    "area": {
      "value": "area/SkiaSharp.HarfBuzz",
      "confidence": 0.99
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2055",
          "description": "Parent epic: Enable Nullable Reference Types across all SkiaSharp packages"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2073",
          "description": "Sibling issue: Enable NRTs in HarfBuzzSharp"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2057",
          "description": "Sibling issue: Enable NRTs in SkiaSharp core"
        }
      ]
    }
  },
  "analysis": {
    "summary": "SkiaSharp.HarfBuzz (source/SkiaSharp.HarfBuzz/) lacks Nullable Reference Types annotations. The csproj has no <Nullable>enable</Nullable> property and source files do not include nullable annotations. This is a tracked sub-task of the NRT epic (#2055) filed by the maintainer.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz.csproj",
        "finding": "No <Nullable> property present in the project file; nullable reference types are not enabled for this package.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/SKShaper.cs",
        "finding": "No #nullable enable or nullable annotations present in source files; method parameters and return types lack nullability annotations.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaSharp.csproj",
        "finding": "SkiaSharp core binding has <Nullable>enable</Nullable> set, demonstrating the pattern to follow for SkiaSharp.HarfBuzz.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "Enable Nullable Reference Types (SkiaSharp.HarfBuzz)",
        "source": "issue title",
        "interpretation": "Maintainer-filed enhancement to add NRT support to this package as part of the broader NRT initiative"
      },
      {
        "text": "Part of epic #2055 (Enable Nullable Reference Types), listed as checklist item",
        "source": "issue #2055 body",
        "interpretation": "Well-defined scope, tracked work item with clear implementation path"
      }
    ],
    "rationale": "This is a maintainer-filed enhancement clearly scoped to enabling NRTs in the SkiaSharp.HarfBuzz package. The issue is part of the NRT epic (#2055). Code investigation confirms NRTs are not yet enabled in SkiaSharp.HarfBuzz. The enhancement is low-risk, follows an established pattern from the SkiaSharp core package, and requires annotation work in a small set of files.",
    "resolution": {
      "hypothesis": "Enable <Nullable>enable</Nullable> in SkiaSharp.HarfBuzz.csproj and add nullable annotations to all public APIs in SKShaper.cs, BlobExtensions.cs, CanvasExtensions.cs, and FontExtensions.cs.",
      "proposals": [
        {
          "category": "fix",
          "title": "Add Nullable enable to SkiaSharp.HarfBuzz.csproj and annotate source files",
          "description": "Add <Nullable>enable</Nullable> to the SkiaSharp.HarfBuzz.csproj PropertyGroup. Then annotate all public APIs in SKShaper.cs, BlobExtensions.cs, CanvasExtensions.cs, and FontExtensions.cs with nullable reference type annotations.",
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "proposal-1",
      "recommendedReason": "This is the standard NRT enablement pattern used across other SkiaSharp packages."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.9,
      "reason": "Well-scoped maintainer-filed enhancement with clear implementation path; needs a developer to do the annotation work.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement and area labels",
        "risk": "low",
        "confidence": 0.99,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp.HarfBuzz",
          "tenet/compatibility",
          "triage/triaged"
        ]
      }
    ]
  }
}
```

</details>
