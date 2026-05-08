# Issue Triage Report — #2057

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T12:32:48Z |
| Type | type/enhancement (0.97 (97%)) |
| Area | area/SkiaSharp (0.98 (98%)) |
| Suggested action | keep-open (0.93 (93%)) |

**Issue Summary:** Tracking issue to enable Nullable Reference Types (NRT) annotations in the main SkiaSharp binding package, part of the broader nullable epic #2055.

**Analysis:** The SkiaSharp main binding csproj already enables nullable project-wide, but the vast majority of source files (75/88) still opt-out via #nullable disable, suppressing compiler enforcement. The issue tracks completing per-file annotation work.

**Recommendations:** **keep-open** — Valid long-term enhancement, work partially started. Tracked as part of epic #2055. No action beyond labelling needed until implementation prioritized.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/enhancement, status/long-term |

## Evidence

### Reproduction

**Related issues:** #2055, #2058, #2059, #2060, #2061, #2062, #2063, #2064, #2065, #2066, #2067, #2068, #2069, #2070, #2071, #2072, #2073, #2074, #2075

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2055 — Parent epic: Enable Nullable Reference Types (all packages)

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The csproj has <Nullable>enable</Nullable> but 75 of 88 source files still carry #nullable disable overrides, meaning annotation work remains incomplete. |

## Analysis

### Technical Summary

The SkiaSharp main binding csproj already enables nullable project-wide, but the vast majority of source files (75/88) still opt-out via #nullable disable, suppressing compiler enforcement. The issue tracks completing per-file annotation work.

### Rationale

Filed by a project maintainer as an enhancement to improve null-safety for consumers. The project-level Nullable setting exists; per-file annotations are the remaining work. Clearly an enhancement, not a bug. Tracked under long-term epic #2055.

### Key Signals

- "Enable Nullable Reference Types (SkiaSharp)" — **issue title** (Scoped to the main SkiaSharp package; sibling issues cover other packages.)
- "Part of epic #2055 tracking all SkiaSharp packages" — **issue #2055 body** (Coordinated, multi-issue effort by the maintainer team.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaSharp.csproj` | 7 | direct | <Nullable>enable</Nullable> is already set project-wide, but 75 out of 88 source files have a top-level #nullable disable directive overriding it. |
| `binding/SkiaSharp/SKCanvas.cs` | 1,54,76 | related | File starts with #nullable disable, then has a small region with #nullable enable around lines 54–76, showing the incremental per-file approach already begun. |

### Next Questions

- Which files are highest priority to annotate first (public API surface vs. internals)?
- Are there any existing callers / downstream packages that would be affected by NRT warnings?

### Resolution Proposals

**Hypothesis:** The enhancement requires annotating the remaining 75 source files in binding/SkiaSharp with nullable annotations, removing the per-file #nullable disable overrides.

1. **Incremental per-file nullable annotation** — fix, confidence 0.90 (90%), cost/xl, validated=untested
   - Enable nullable annotations file-by-file, removing #nullable disable, annotating public API parameters/return types, and fixing any resulting warnings. Follow the incremental pattern already started in SKCanvas.cs.

**Recommended proposal:** Incremental per-file nullable annotation

**Why:** Matches the approach already started in the codebase and minimizes the risk of NRT-induced breaking changes on a file-by-file basis.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.93 (93%) |
| Reason | Valid long-term enhancement, work partially started. Tracked as part of epic #2055. No action beyond labelling needed until implementation prioritized. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply enhancement, area, and tenet labels | labels=type/enhancement, area/SkiaSharp, tenet/compatibility |
| link-related | low | 0.95 (95%) | Cross-reference parent nullable epic #2055 | linkedIssue=#2055 |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2057,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T12:32:48Z",
    "currentLabels": [
      "type/enhancement",
      "status/long-term"
    ]
  },
  "summary": "Tracking issue to enable Nullable Reference Types (NRT) annotations in the main SkiaSharp binding package, part of the broader nullable epic #2055.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.98
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "relatedIssues": [
        2055,
        2058,
        2059,
        2060,
        2061,
        2062,
        2063,
        2064,
        2065,
        2066,
        2067,
        2068,
        2069,
        2070,
        2071,
        2072,
        2073,
        2074,
        2075
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2055",
          "description": "Parent epic: Enable Nullable Reference Types (all packages)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "The csproj has <Nullable>enable</Nullable> but 75 of 88 source files still carry #nullable disable overrides, meaning annotation work remains incomplete."
    }
  },
  "analysis": {
    "summary": "The SkiaSharp main binding csproj already enables nullable project-wide, but the vast majority of source files (75/88) still opt-out via #nullable disable, suppressing compiler enforcement. The issue tracks completing per-file annotation work.",
    "rationale": "Filed by a project maintainer as an enhancement to improve null-safety for consumers. The project-level Nullable setting exists; per-file annotations are the remaining work. Clearly an enhancement, not a bug. Tracked under long-term epic #2055.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaSharp.csproj",
        "lines": "7",
        "finding": "<Nullable>enable</Nullable> is already set project-wide, but 75 out of 88 source files have a top-level #nullable disable directive overriding it.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "1,54,76",
        "finding": "File starts with #nullable disable, then has a small region with #nullable enable around lines 54–76, showing the incremental per-file approach already begun.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Enable Nullable Reference Types (SkiaSharp)",
        "source": "issue title",
        "interpretation": "Scoped to the main SkiaSharp package; sibling issues cover other packages."
      },
      {
        "text": "Part of epic #2055 tracking all SkiaSharp packages",
        "source": "issue #2055 body",
        "interpretation": "Coordinated, multi-issue effort by the maintainer team."
      }
    ],
    "nextQuestions": [
      "Which files are highest priority to annotate first (public API surface vs. internals)?",
      "Are there any existing callers / downstream packages that would be affected by NRT warnings?"
    ],
    "resolution": {
      "hypothesis": "The enhancement requires annotating the remaining 75 source files in binding/SkiaSharp with nullable annotations, removing the per-file #nullable disable overrides.",
      "proposals": [
        {
          "title": "Incremental per-file nullable annotation",
          "description": "Enable nullable annotations file-by-file, removing #nullable disable, annotating public API parameters/return types, and fixing any resulting warnings. Follow the incremental pattern already started in SKCanvas.cs.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/xl",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Incremental per-file nullable annotation",
      "recommendedReason": "Matches the approach already started in the codebase and minimizes the risk of NRT-induced breaking changes on a file-by-file basis."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.93,
      "reason": "Valid long-term enhancement, work partially started. Tracked as part of epic #2055. No action beyond labelling needed until implementation prioritized.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement, area, and tenet labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp",
          "tenet/compatibility"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference parent nullable epic #2055",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 2055
      }
    ]
  }
}
```

</details>
