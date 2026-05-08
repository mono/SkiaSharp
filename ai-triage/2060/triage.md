# Issue Triage Report — #2060

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T14:07:13Z |
| Type | type/enhancement (0.98 (98%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | keep-open (0.70 (70%)) |

**Issue Summary:** Maintainer-filed tracking issue to enable Nullable Reference Types (NRT) annotations in the SkiaSharp.Views.Gtk2 project, part of a batch of similar NRT-enablement issues across the SkiaSharp ecosystem.

**Analysis:** This is a maintainer-filed tracking issue for enabling C# Nullable Reference Types in SkiaSharp.Views.Gtk2, part of a coordinated batch of NRT-enablement issues across the SkiaSharp solution. Investigation reveals that the SkiaSharp.Views.Gtk2 project no longer exists in the repository (last changelog entry is 2.88.9); the codebase now contains only Gtk3 and Gtk4 variants. The issue may be stale.

**Recommendations:** **keep-open** — Maintainer-filed tracking issue with status/long-term. The Gtk2 project appears discontinued but a human maintainer should confirm before closing.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.Views |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/compatibility, tenet/reliability |
| Partner | — |
| Current labels | type/enhancement, status/long-term |

## Evidence

### Reproduction

**Related issues:** #2055, #2057, #2058, #2059, #2061, #2062, #2063, #2064, #2065, #2067, #2068, #2072, #2074, #2075

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | The SkiaSharp.Views.Gtk2 project appears to have been discontinued at version 2.88.9 and no longer exists in the repository. Only Gtk3 and Gtk4 variants are present. |

## Analysis

### Technical Summary

This is a maintainer-filed tracking issue for enabling C# Nullable Reference Types in SkiaSharp.Views.Gtk2, part of a coordinated batch of NRT-enablement issues across the SkiaSharp solution. Investigation reveals that the SkiaSharp.Views.Gtk2 project no longer exists in the repository (last changelog entry is 2.88.9); the codebase now contains only Gtk3 and Gtk4 variants. The issue may be stale.

### Rationale

This is a maintainer-created tracking issue for enabling nullable reference type annotations in SkiaSharp.Views.Gtk2. The issue was filed as part of a batch (alongside #2055, #2057–#2075) to track NRT adoption across the entire SkiaSharp solution. The SkiaSharp.Views.Gtk2 project no longer exists in the repository (discontinued at v2.88.9), which means the work cannot be implemented as originally scoped — the issue may be stale or needs to be redirected to the Gtk3 successor.

### Key Signals

- "Enable Nullable Reference Types (SkiaSharp.Views.Gtk2)" — **issue title** (Part of a coordinated batch of issues (#2055-#2075) to enable NRT across all SkiaSharp projects)
- "status/long-term label applied" — **issue labels** (Maintainer flagged this as low-priority long-term work at filing time)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.Gtk3/SkiaSharp.Views.Gtk3.csproj` | — | related | Gtk3 project (likely successor to Gtk2) does not set <Nullable>enable</Nullable> — NRT not enabled in any GTK variant |
| `changelogs/SkiaSharp.Views.Gtk2` | — | direct | SkiaSharp.Views.Gtk2 changelog directory exists with entries up to 2.88.9 only — no 3.x entries, indicating the package was discontinued |

### Next Questions

- Was SkiaSharp.Views.Gtk2 intentionally discontinued, and should this issue be closed as not_planned?
- Should the NRT work be tracked against SkiaSharp.Views.Gtk3 instead (covered by #2061)?

### Resolution Proposals

**Hypothesis:** The SkiaSharp.Views.Gtk2 package was discontinued at v2.88.9 and this tracking issue is stale; the equivalent work for the active Gtk3 successor is tracked in #2061.

1. **Close as stale — Gtk2 discontinued** — investigation, confidence 0.75 (75%), cost/xs, validated=untested
   - SkiaSharp.Views.Gtk2 no longer exists in the codebase. NRT enablement for the active GTK bindings is tracked in #2061 (Gtk3). Close this issue as not_planned.
2. **Keep open for historical tracking** — alternative, confidence 0.50 (50%), cost/xs, validated=untested
   - Leave the issue open as a record that NRT was never enabled in the now-deprecated Gtk2 package, consistent with the status/long-term label applied by the maintainer.

**Recommended proposal:** Close as stale — Gtk2 discontinued

**Why:** The target project no longer exists in the repository. The equivalent work for active GTK bindings is tracked in #2061.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.70 (70%) |
| Reason | Maintainer-filed tracking issue with status/long-term. The Gtk2 project appears discontinued but a human maintainer should confirm before closing. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply enhancement and views area labels; add Linux platform | labels=type/enhancement, area/SkiaSharp.Views, os/Linux, tenet/compatibility, tenet/reliability |
| add-comment | high | 0.70 (70%) | Note that SkiaSharp.Views.Gtk2 appears discontinued and suggest redirecting to #2061 | — |

**Comment draft for `add-comment`:**

```markdown
During triage, it was found that the `SkiaSharp.Views.Gtk2` project no longer exists in the repository (last changelog entry is v2.88.9). NRT enablement for the active GTK bindings appears to be tracked in #2061 (`SkiaSharp.Views.Gtk3`). Should this issue be closed as not-planned given that the Gtk2 package was discontinued?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2060,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T14:07:13Z",
    "currentLabels": [
      "type/enhancement",
      "status/long-term"
    ]
  },
  "summary": "Maintainer-filed tracking issue to enable Nullable Reference Types (NRT) annotations in the SkiaSharp.Views.Gtk2 project, part of a batch of similar NRT-enablement issues across the SkiaSharp ecosystem.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.98
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.9
    },
    "platforms": [
      "os/Linux"
    ],
    "tenets": [
      "tenet/compatibility",
      "tenet/reliability"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "relatedIssues": [
        2055,
        2057,
        2058,
        2059,
        2061,
        2062,
        2063,
        2064,
        2065,
        2067,
        2068,
        2072,
        2074,
        2075
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "unlikely",
      "relevanceReason": "The SkiaSharp.Views.Gtk2 project appears to have been discontinued at version 2.88.9 and no longer exists in the repository. Only Gtk3 and Gtk4 variants are present."
    }
  },
  "analysis": {
    "summary": "This is a maintainer-filed tracking issue for enabling C# Nullable Reference Types in SkiaSharp.Views.Gtk2, part of a coordinated batch of NRT-enablement issues across the SkiaSharp solution. Investigation reveals that the SkiaSharp.Views.Gtk2 project no longer exists in the repository (last changelog entry is 2.88.9); the codebase now contains only Gtk3 and Gtk4 variants. The issue may be stale.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Gtk3/SkiaSharp.Views.Gtk3.csproj",
        "finding": "Gtk3 project (likely successor to Gtk2) does not set <Nullable>enable</Nullable> — NRT not enabled in any GTK variant",
        "relevance": "related"
      },
      {
        "file": "changelogs/SkiaSharp.Views.Gtk2",
        "finding": "SkiaSharp.Views.Gtk2 changelog directory exists with entries up to 2.88.9 only — no 3.x entries, indicating the package was discontinued",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Enable Nullable Reference Types (SkiaSharp.Views.Gtk2)",
        "source": "issue title",
        "interpretation": "Part of a coordinated batch of issues (#2055-#2075) to enable NRT across all SkiaSharp projects"
      },
      {
        "text": "status/long-term label applied",
        "source": "issue labels",
        "interpretation": "Maintainer flagged this as low-priority long-term work at filing time"
      }
    ],
    "rationale": "This is a maintainer-created tracking issue for enabling nullable reference type annotations in SkiaSharp.Views.Gtk2. The issue was filed as part of a batch (alongside #2055, #2057–#2075) to track NRT adoption across the entire SkiaSharp solution. The SkiaSharp.Views.Gtk2 project no longer exists in the repository (discontinued at v2.88.9), which means the work cannot be implemented as originally scoped — the issue may be stale or needs to be redirected to the Gtk3 successor.",
    "nextQuestions": [
      "Was SkiaSharp.Views.Gtk2 intentionally discontinued, and should this issue be closed as not_planned?",
      "Should the NRT work be tracked against SkiaSharp.Views.Gtk3 instead (covered by #2061)?"
    ],
    "resolution": {
      "hypothesis": "The SkiaSharp.Views.Gtk2 package was discontinued at v2.88.9 and this tracking issue is stale; the equivalent work for the active Gtk3 successor is tracked in #2061.",
      "proposals": [
        {
          "title": "Close as stale — Gtk2 discontinued",
          "description": "SkiaSharp.Views.Gtk2 no longer exists in the codebase. NRT enablement for the active GTK bindings is tracked in #2061 (Gtk3). Close this issue as not_planned.",
          "category": "investigation",
          "confidence": 0.75,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Keep open for historical tracking",
          "description": "Leave the issue open as a record that NRT was never enabled in the now-deprecated Gtk2 package, consistent with the status/long-term label applied by the maintainer.",
          "category": "alternative",
          "confidence": 0.5,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Close as stale — Gtk2 discontinued",
      "recommendedReason": "The target project no longer exists in the repository. The equivalent work for active GTK bindings is tracked in #2061."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.7,
      "reason": "Maintainer-filed tracking issue with status/long-term. The Gtk2 project appears discontinued but a human maintainer should confirm before closing.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement and views area labels; add Linux platform",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp.Views",
          "os/Linux",
          "tenet/compatibility",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Note that SkiaSharp.Views.Gtk2 appears discontinued and suggest redirecting to #2061",
        "risk": "high",
        "confidence": 0.7,
        "comment": "During triage, it was found that the `SkiaSharp.Views.Gtk2` project no longer exists in the repository (last changelog entry is v2.88.9). NRT enablement for the active GTK bindings appears to be tracked in #2061 (`SkiaSharp.Views.Gtk3`). Should this issue be closed as not-planned given that the Gtk2 package was discontinued?"
      }
    ]
  }
}
```

</details>
