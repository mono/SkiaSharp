# Issue Triage Report — #2064

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T15:30:00Z |
| Type | type/enhancement (0.98 (98%)) |
| Area | area/SkiaSharp.Views.Forms (0.95 (95%)) |
| Suggested action | keep-open (0.75 (75%)) |

**Issue Summary:** Enhancement request to enable Nullable Reference Types (NRT) in the SkiaSharp.Views.Forms package, filed as part of the NRT epic #2055; the package has since been deprecated with no remaining source in the repo.

**Analysis:** The SkiaSharp.Views.Forms package (Xamarin.Forms bindings) has been deprecated and replaced by SkiaSharp.Views.Maui.Controls. No source directory for it exists in the current codebase. Enabling NRT on a package with no source is not actionable; the equivalent NRT work for the successor package would need its own tracking issue.

**Recommendations:** **keep-open** — Part of long-term NRT epic #2055. Maintainer should decide whether to close as obsolete or redirect to SkiaSharp.Views.Maui.Controls.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.Views.Forms |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/enhancement, status/long-term |

## Evidence

### Reproduction

**Related issues:** #2055, #2058, #2065, #2066

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | SkiaSharp.Views.Forms (Xamarin.Forms bindings) is deprecated and its source was removed from the repo; the successor is SkiaSharp.Views.Maui.Controls. |

## Analysis

### Technical Summary

The SkiaSharp.Views.Forms package (Xamarin.Forms bindings) has been deprecated and replaced by SkiaSharp.Views.Maui.Controls. No source directory for it exists in the current codebase. Enabling NRT on a package with no source is not actionable; the equivalent NRT work for the successor package would need its own tracking issue.

### Rationale

Classified as type/enhancement (NRT enablement is additive). Area is SkiaSharp.Views.Forms per the issue title, though the package is deprecated. The issue is part of epic #2055 and was filed long-term. Since the source no longer exists in the repo, the enhancement is obsolete for this specific package; the tenet/compatibility label is retained as NRT adds nullability contracts.

### Key Signals

- "Enable Nullable Reference Types (SkiaSharp.Views.Forms)" — **issue title** (Enhancement to annotate the Xamarin.Forms view package with NRT — part of a broader NRT epic.)
- "SkiaSharp.Views.Forms → SkiaSharp.Views.Maui.Controls" — **documentation/dev/packages.md** (The package targeted by this issue is deprecated; source no longer exists in the repo.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `documentation/dev/packages.md` | 168 | direct | SkiaSharp.Views.Forms is listed as deprecated and replaced by SkiaSharp.Views.Maui.Controls — no source directory exists in the current codebase. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SkiaSharp.Views.Maui.Controls.csproj` | — | related | Successor package SkiaSharp.Views.Maui.Controls does not currently have <Nullable>enable</Nullable> set, confirming NRT has not been enabled in the successor either. |

### Next Questions

- Should this issue be closed as obsolete given the deprecation of SkiaSharp.Views.Forms, or redirected to a new issue for SkiaSharp.Views.Maui.Controls?
- Is there a separate NRT tracking issue for SkiaSharp.Views.Maui.Controls in the epic #2055?

### Resolution Proposals

**Hypothesis:** The SkiaSharp.Views.Forms package no longer has source in the repo, so NRT enablement for it is moot. Any remaining NRT work should target SkiaSharp.Views.Maui.Controls.

1. **Close as obsolete — package deprecated** — investigation, confidence 0.82 (82%), cost/xs, validated=untested
   - Close this issue since SkiaSharp.Views.Forms has been deprecated and removed. A new NRT issue should be filed (or the epic updated) to track enablement in SkiaSharp.Views.Maui.Controls.
2. **Keep open and redirect to MAUI successor** — alternative, confidence 0.70 (70%), cost/xs, validated=untested
   - Keep the issue open but update the title/description to reference SkiaSharp.Views.Maui.Controls, since that is the successor package where NRT should be enabled.

**Recommended proposal:** Close as obsolete — package deprecated

**Why:** The source package no longer exists in the repo; the work cannot be done. A MAUI-specific NRT issue should be tracked separately in the epic.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.75 (75%) |
| Reason | Part of long-term NRT epic #2055. Maintainer should decide whether to close as obsolete or redirect to SkiaSharp.Views.Maui.Controls. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply enhancement, area/SkiaSharp.Views.Forms, and tenet/compatibility labels | labels=type/enhancement, area/SkiaSharp.Views.Forms, tenet/compatibility |
| add-comment | medium | 0.78 (78%) | Note that SkiaSharp.Views.Forms is deprecated and NRT work should target the MAUI successor | — |
| link-related | low | 0.99 (99%) | Link to the NRT epic | linkedIssue=#2055 |

**Comment draft for `add-comment`:**

```markdown
Note: `SkiaSharp.Views.Forms` has been deprecated and its source removed from the repository. The successor package is `SkiaSharp.Views.Maui.Controls`. NRT enablement for the Xamarin.Forms views is no longer actionable here. Consider closing this item in the epic and opening a dedicated NRT tracking issue for `SkiaSharp.Views.Maui.Controls` instead.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2064,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T15:30:00Z",
    "currentLabels": [
      "type/enhancement",
      "status/long-term"
    ]
  },
  "summary": "Enhancement request to enable Nullable Reference Types (NRT) in the SkiaSharp.Views.Forms package, filed as part of the NRT epic #2055; the package has since been deprecated with no remaining source in the repo.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.98
    },
    "area": {
      "value": "area/SkiaSharp.Views.Forms",
      "confidence": 0.95
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
        2065,
        2066
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "unlikely",
      "relevanceReason": "SkiaSharp.Views.Forms (Xamarin.Forms bindings) is deprecated and its source was removed from the repo; the successor is SkiaSharp.Views.Maui.Controls."
    }
  },
  "analysis": {
    "summary": "The SkiaSharp.Views.Forms package (Xamarin.Forms bindings) has been deprecated and replaced by SkiaSharp.Views.Maui.Controls. No source directory for it exists in the current codebase. Enabling NRT on a package with no source is not actionable; the equivalent NRT work for the successor package would need its own tracking issue.",
    "rationale": "Classified as type/enhancement (NRT enablement is additive). Area is SkiaSharp.Views.Forms per the issue title, though the package is deprecated. The issue is part of epic #2055 and was filed long-term. Since the source no longer exists in the repo, the enhancement is obsolete for this specific package; the tenet/compatibility label is retained as NRT adds nullability contracts.",
    "codeInvestigation": [
      {
        "file": "documentation/dev/packages.md",
        "lines": "168",
        "finding": "SkiaSharp.Views.Forms is listed as deprecated and replaced by SkiaSharp.Views.Maui.Controls — no source directory exists in the current codebase.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SkiaSharp.Views.Maui.Controls.csproj",
        "finding": "Successor package SkiaSharp.Views.Maui.Controls does not currently have <Nullable>enable</Nullable> set, confirming NRT has not been enabled in the successor either.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Enable Nullable Reference Types (SkiaSharp.Views.Forms)",
        "source": "issue title",
        "interpretation": "Enhancement to annotate the Xamarin.Forms view package with NRT — part of a broader NRT epic."
      },
      {
        "text": "SkiaSharp.Views.Forms → SkiaSharp.Views.Maui.Controls",
        "source": "documentation/dev/packages.md",
        "interpretation": "The package targeted by this issue is deprecated; source no longer exists in the repo."
      }
    ],
    "nextQuestions": [
      "Should this issue be closed as obsolete given the deprecation of SkiaSharp.Views.Forms, or redirected to a new issue for SkiaSharp.Views.Maui.Controls?",
      "Is there a separate NRT tracking issue for SkiaSharp.Views.Maui.Controls in the epic #2055?"
    ],
    "resolution": {
      "hypothesis": "The SkiaSharp.Views.Forms package no longer has source in the repo, so NRT enablement for it is moot. Any remaining NRT work should target SkiaSharp.Views.Maui.Controls.",
      "proposals": [
        {
          "title": "Close as obsolete — package deprecated",
          "description": "Close this issue since SkiaSharp.Views.Forms has been deprecated and removed. A new NRT issue should be filed (or the epic updated) to track enablement in SkiaSharp.Views.Maui.Controls.",
          "category": "investigation",
          "confidence": 0.82,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Keep open and redirect to MAUI successor",
          "description": "Keep the issue open but update the title/description to reference SkiaSharp.Views.Maui.Controls, since that is the successor package where NRT should be enabled.",
          "category": "alternative",
          "confidence": 0.7,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Close as obsolete — package deprecated",
      "recommendedReason": "The source package no longer exists in the repo; the work cannot be done. A MAUI-specific NRT issue should be tracked separately in the epic."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.75,
      "reason": "Part of long-term NRT epic #2055. Maintainer should decide whether to close as obsolete or redirect to SkiaSharp.Views.Maui.Controls.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement, area/SkiaSharp.Views.Forms, and tenet/compatibility labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp.Views.Forms",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Note that SkiaSharp.Views.Forms is deprecated and NRT work should target the MAUI successor",
        "risk": "medium",
        "confidence": 0.78,
        "comment": "Note: `SkiaSharp.Views.Forms` has been deprecated and its source removed from the repository. The successor package is `SkiaSharp.Views.Maui.Controls`. NRT enablement for the Xamarin.Forms views is no longer actionable here. Consider closing this item in the epic and opening a dedicated NRT tracking issue for `SkiaSharp.Views.Maui.Controls` instead."
      },
      {
        "type": "link-related",
        "description": "Link to the NRT epic",
        "risk": "low",
        "confidence": 0.99,
        "linkedIssue": 2055
      }
    ]
  }
}
```

</details>
