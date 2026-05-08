# Issue Triage Report — #2111

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T21:18:27Z |
| Type | type/enhancement (0.95 (95%)) |
| Area | area/Build (0.90 (90%)) |
| Suggested action | close-as-fixed (0.88 (88%)) |

**Issue Summary:** Reporter requests dropping .NET Standard 1.3 support in SkiaSharp to ease maintainability and enable newer framework features without preprocessor directives.

**Analysis:** The request to drop .NET Standard 1.3 has already been fulfilled. The current build configuration targets netstandard2.0 as the minimum .NET Standard version. The only remaining netstandard1.3 reference is a stale path lookup in UpdateDocs.cake for a package cache directory, not an actual compilation target.

**Recommendations:** **close-as-fixed** — Code investigation confirms netstandard1.3 is no longer a build target. The enhancement has already been implemented.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/Build |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Environment:** Filed 2022-06-14. No version specified.

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | The current source/SkiaSharp.Build.props defines AllTargetFrameworks without netstandard1.3; the minimum .NET Standard target is netstandard2.0. The request appears to have already been fulfilled. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.90 (90%) |
| Reason | source/SkiaSharp.Build.props no longer includes netstandard1.3 in AllTargetFrameworks. BasicTargetFrameworksCurrent is netstandard2.0;netstandard2.1;net462;net48 plus modern TFMs. Only a single reference to netstandard1.3 remains in scripts/cake/UpdateDocs.cake as a legacy cache-lookup path, not a compilation target. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

The request to drop .NET Standard 1.3 has already been fulfilled. The current build configuration targets netstandard2.0 as the minimum .NET Standard version. The only remaining netstandard1.3 reference is a stale path lookup in UpdateDocs.cake for a package cache directory, not an actual compilation target.

### Rationale

Type is enhancement because the reporter is asking for a build target removal to improve maintainability. Area is Build because the change is entirely in build configuration (csproj TFMs and props files). Code investigation confirms the requested change has already been made. suggestedAction is close-as-fixed because the enhancement is already implemented.

### Key Signals

- "To ease maintainability and enable newer framework features without #ifdefs, you should consider dropping support for .NET Standard 1.3 in SkiaSharp." — **issue body** (Enhancement request for build configuration simplification.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Build.props` | 101-129 | direct | BasicTargetFrameworksCurrent is defined as 'netstandard2.0;netstandard2.1;net462;net48;$(TFMCurrent)'. AllTargetFrameworks does not include netstandard1.3. The minimum .NET Standard target is netstandard2.0. |
| `scripts/cake/UpdateDocs.cake` | 62 | context | Single reference to netstandard1.3 as a package cache path pattern for legacy HarfBuzzSharp cached NuGet artifacts, not a compilation target. |

### Resolution Proposals

**Hypothesis:** The .NET Standard 1.3 TFM was removed from the SkiaSharp build configuration at some point after this issue was filed.

1. **Close as already fixed** — fix, confidence 0.90 (90%), cost/xs, validated=untested
   - Verify and close the issue, noting that netstandard1.3 is no longer a compilation target in the current codebase. The build targets netstandard2.0 as the minimum .NET Standard.

**Recommended proposal:** Close as already fixed

**Why:** Code investigation confirms AllTargetFrameworks in source/SkiaSharp.Build.props no longer includes netstandard1.3.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.88 (88%) |
| Reason | Code investigation confirms netstandard1.3 is no longer a build target. The enhancement has already been implemented. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply enhancement and build labels | labels=type/enhancement, area/Build, tenet/compatibility |
| add-comment | medium | 0.88 (88%) | Post comment noting the enhancement has been completed | — |
| close-issue | medium | 0.88 (88%) | Close as completed — enhancement already implemented | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the suggestion! After reviewing the current codebase, it looks like .NET Standard 1.3 has already been dropped. The current build configuration in `source/SkiaSharp.Build.props` targets `netstandard2.0` as the minimum .NET Standard version — `netstandard1.3` is no longer a compilation target. Closing as fixed.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2111,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T21:18:27Z"
  },
  "summary": "Reporter requests dropping .NET Standard 1.3 support in SkiaSharp to ease maintainability and enable newer framework features without preprocessor directives.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.95
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.9
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Filed 2022-06-14. No version specified."
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "unlikely",
      "relevanceReason": "The current source/SkiaSharp.Build.props defines AllTargetFrameworks without netstandard1.3; the minimum .NET Standard target is netstandard2.0. The request appears to have already been fulfilled."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.9,
      "reason": "source/SkiaSharp.Build.props no longer includes netstandard1.3 in AllTargetFrameworks. BasicTargetFrameworksCurrent is netstandard2.0;netstandard2.1;net462;net48 plus modern TFMs. Only a single reference to netstandard1.3 remains in scripts/cake/UpdateDocs.cake as a legacy cache-lookup path, not a compilation target."
    }
  },
  "analysis": {
    "summary": "The request to drop .NET Standard 1.3 has already been fulfilled. The current build configuration targets netstandard2.0 as the minimum .NET Standard version. The only remaining netstandard1.3 reference is a stale path lookup in UpdateDocs.cake for a package cache directory, not an actual compilation target.",
    "rationale": "Type is enhancement because the reporter is asking for a build target removal to improve maintainability. Area is Build because the change is entirely in build configuration (csproj TFMs and props files). Code investigation confirms the requested change has already been made. suggestedAction is close-as-fixed because the enhancement is already implemented.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Build.props",
        "lines": "101-129",
        "finding": "BasicTargetFrameworksCurrent is defined as 'netstandard2.0;netstandard2.1;net462;net48;$(TFMCurrent)'. AllTargetFrameworks does not include netstandard1.3. The minimum .NET Standard target is netstandard2.0.",
        "relevance": "direct"
      },
      {
        "file": "scripts/cake/UpdateDocs.cake",
        "lines": "62",
        "finding": "Single reference to netstandard1.3 as a package cache path pattern for legacy HarfBuzzSharp cached NuGet artifacts, not a compilation target.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "To ease maintainability and enable newer framework features without #ifdefs, you should consider dropping support for .NET Standard 1.3 in SkiaSharp.",
        "source": "issue body",
        "interpretation": "Enhancement request for build configuration simplification."
      }
    ],
    "resolution": {
      "hypothesis": "The .NET Standard 1.3 TFM was removed from the SkiaSharp build configuration at some point after this issue was filed.",
      "proposals": [
        {
          "title": "Close as already fixed",
          "description": "Verify and close the issue, noting that netstandard1.3 is no longer a compilation target in the current codebase. The build targets netstandard2.0 as the minimum .NET Standard.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Close as already fixed",
      "recommendedReason": "Code investigation confirms AllTargetFrameworks in source/SkiaSharp.Build.props no longer includes netstandard1.3."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.88,
      "reason": "Code investigation confirms netstandard1.3 is no longer a build target. The enhancement has already been implemented.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement and build labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/enhancement",
          "area/Build",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post comment noting the enhancement has been completed",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the suggestion! After reviewing the current codebase, it looks like .NET Standard 1.3 has already been dropped. The current build configuration in `source/SkiaSharp.Build.props` targets `netstandard2.0` as the minimum .NET Standard version — `netstandard1.3` is no longer a compilation target. Closing as fixed."
      },
      {
        "type": "close-issue",
        "description": "Close as completed — enhancement already implemented",
        "risk": "medium",
        "confidence": 0.88,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
