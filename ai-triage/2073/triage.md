# Issue Triage Report — #2073

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T18:38:46Z |
| Type | type/enhancement (0.97 (97%)) |
| Area | area/HarfBuzzSharp (0.99 (99%)) |
| Suggested action | close-as-fixed (0.92 (92%)) |

**Issue Summary:** Request to enable Nullable Reference Types (NRT) annotations in the HarfBuzzSharp binding library.

**Analysis:** This enhancement requests enabling C# Nullable Reference Types in the HarfBuzzSharp binding project. Code investigation shows that HarfBuzzSharp.csproj already has <Nullable>enable</Nullable>, meaning this request has been fulfilled. The issue was created the same day as its related NRT tracking issue #1316 was closed.

**Recommendations:** **close-as-fixed** — HarfBuzzSharp.csproj already contains <Nullable>enable</Nullable>. The enhancement requested has been implemented.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/HarfBuzzSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1316 — Related NRT feature request for SkiaSharp (closed not_planned same day #2073 was filed).

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.95 (95%) |
| Reason | HarfBuzzSharp.csproj now contains <Nullable>enable</Nullable> indicating NRT has been enabled. The companion SkiaSharp.csproj also has the same setting, suggesting this was done as part of the same initiative. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

This enhancement requests enabling C# Nullable Reference Types in the HarfBuzzSharp binding project. Code investigation shows that HarfBuzzSharp.csproj already has <Nullable>enable</Nullable>, meaning this request has been fulfilled. The issue was created the same day as its related NRT tracking issue #1316 was closed.

### Rationale

The issue is classified as type/enhancement for area/HarfBuzzSharp. Code investigation confirms the feature is already implemented: HarfBuzzSharp.csproj has <Nullable>enable</Nullable>. The related issue #1316 was closed on the same date this issue was opened, suggesting this was a tracking split. The suggestedAction is close-as-fixed with high confidence.

### Key Signals

- "<Nullable>enable</Nullable>" — **binding/HarfBuzzSharp/HarfBuzzSharp.csproj line 8** (NRT has already been enabled in the project file.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/HarfBuzzSharp/HarfBuzzSharp.csproj` | — | direct | Contains <Nullable>enable</Nullable> in the main PropertyGroup, along with WarningsAsErrors for CA1420/CA1421 and a NoWarn for CS8826 — indicating NRT is fully enabled. |
| `binding/SkiaSharp/SkiaSharp.csproj` | — | related | Also contains <Nullable>enable</Nullable>, confirming this was enabled across both binding projects. |

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.92 (92%) |
| Reason | HarfBuzzSharp.csproj already contains <Nullable>enable</Nullable>. The enhancement requested has been implemented. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply type/enhancement and area/HarfBuzzSharp labels | labels=type/enhancement, area/HarfBuzzSharp |
| add-comment | medium | 0.92 (92%) | Inform reporter that NRT has been enabled in HarfBuzzSharp | — |
| close-issue | medium | 0.92 (92%) | Close as fixed since NRT is already enabled | stateReason=completed |
| link-related | low | 0.90 (90%) | Cross-reference related NRT issue #1316 | linkedIssue=#1316 |

**Comment draft for `add-comment`:**

```markdown
This enhancement has been implemented. `HarfBuzzSharp.csproj` now has `<Nullable>enable</Nullable>` enabled, which means nullable reference type annotations are active across the HarfBuzzSharp binding. This was part of the broader NRT rollout also tracked in #1316. Closing as fixed.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2073,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T18:38:46Z"
  },
  "summary": "Request to enable Nullable Reference Types (NRT) annotations in the HarfBuzzSharp binding library.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.97
    },
    "area": {
      "value": "area/HarfBuzzSharp",
      "confidence": 0.99
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.95,
      "reason": "HarfBuzzSharp.csproj now contains <Nullable>enable</Nullable> indicating NRT has been enabled. The companion SkiaSharp.csproj also has the same setting, suggesting this was done as part of the same initiative.",
      "relatedPRs": [],
      "relatedCommits": []
    },
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1316",
          "description": "Related NRT feature request for SkiaSharp (closed not_planned same day #2073 was filed)."
        }
      ]
    }
  },
  "analysis": {
    "summary": "This enhancement requests enabling C# Nullable Reference Types in the HarfBuzzSharp binding project. Code investigation shows that HarfBuzzSharp.csproj already has <Nullable>enable</Nullable>, meaning this request has been fulfilled. The issue was created the same day as its related NRT tracking issue #1316 was closed.",
    "codeInvestigation": [
      {
        "file": "binding/HarfBuzzSharp/HarfBuzzSharp.csproj",
        "finding": "Contains <Nullable>enable</Nullable> in the main PropertyGroup, along with WarningsAsErrors for CA1420/CA1421 and a NoWarn for CS8826 — indicating NRT is fully enabled.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaSharp.csproj",
        "finding": "Also contains <Nullable>enable</Nullable>, confirming this was enabled across both binding projects.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "<Nullable>enable</Nullable>",
        "source": "binding/HarfBuzzSharp/HarfBuzzSharp.csproj line 8",
        "interpretation": "NRT has already been enabled in the project file."
      }
    ],
    "rationale": "The issue is classified as type/enhancement for area/HarfBuzzSharp. Code investigation confirms the feature is already implemented: HarfBuzzSharp.csproj has <Nullable>enable</Nullable>. The related issue #1316 was closed on the same date this issue was opened, suggesting this was a tracking split. The suggestedAction is close-as-fixed with high confidence."
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.92,
      "reason": "HarfBuzzSharp.csproj already contains <Nullable>enable</Nullable>. The enhancement requested has been implemented.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/enhancement and area/HarfBuzzSharp labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/enhancement",
          "area/HarfBuzzSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Inform reporter that NRT has been enabled in HarfBuzzSharp",
        "risk": "medium",
        "confidence": 0.92,
        "comment": "This enhancement has been implemented. `HarfBuzzSharp.csproj` now has `<Nullable>enable</Nullable>` enabled, which means nullable reference type annotations are active across the HarfBuzzSharp binding. This was part of the broader NRT rollout also tracked in #1316. Closing as fixed."
      },
      {
        "type": "close-issue",
        "description": "Close as fixed since NRT is already enabled",
        "risk": "medium",
        "confidence": 0.92,
        "stateReason": "completed"
      },
      {
        "type": "link-related",
        "description": "Cross-reference related NRT issue #1316",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 1316
      }
    ]
  }
}
```

</details>
