# Issue Triage Report — #1920

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T15:54:56Z |
| Type | type/question (0.88 (88%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | keep-open (0.75 (75%)) |

**Issue Summary:** Community members question whether SkiaSharp has only one maintainer (@mattleibow) and request that additional maintainers or reviewers be added, citing mounting issues/PRs and broken Uno Platform 4.x builds as evidence of an unsustainable single-maintainer model for a library used across MAUI, Xamarin, Uno Platform, and Avalonia.

**Analysis:** This is a project governance question asking the maintainers to expand the contributor/reviewer team. No technical defect is described. The concern is about project sustainability — single-maintainer risk for a library critical to MAUI, Xamarin, Uno Platform, and Avalonia. The issue was converted from GitHub Discussion #1884 and has received no maintainer response.

**Recommendations:** **keep-open** — Valid community concern about project sustainability. No code fix exists; requires maintainer response on governance. Kept open as a signal until maintainers address or convert to discussion.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/question, area/SkiaSharp, tenet/reliability, triage/triaged |

## Evidence

### Reproduction

**Environment:** Originally from GitHub Discussion #1884 (December 2021), converted to issue January 2022. Comments span January–June 2022. No maintainer response on record. Issue mentions Uno Platform 4.x build breakage at time of filing.

**Related issues:** #1723

## Analysis

### Technical Summary

This is a project governance question asking the maintainers to expand the contributor/reviewer team. No technical defect is described. The concern is about project sustainability — single-maintainer risk for a library critical to MAUI, Xamarin, Uno Platform, and Avalonia. The issue was converted from GitHub Discussion #1884 and has received no maintainer response.

### Rationale

The issue content is a governance/meta question, not a bug or feature request. Type/question is appropriate. The tenet/reliability label fits because the concern is about project reliability and sustainability. The area is SkiaSharp (general project). No platform/backend labels apply since this is a governance issue, not a technical one. CONTRIBUTING.md was examined: it documents contribution guidelines but no maintainer expansion process, confirming the gap the reporter describes.

### Key Signals

- "having responsibility left to a single individual is a high risk" — **issue body** (Reporter identifies single-maintainer SPOF risk for a widely-used library.)
- "building with Uno Platform 4.x is completely broken right now" — **issue body** (Concrete production-impact example motivating the governance request.)
- "The issues continue to build up because there is only one person to get to them" — **comment by bmitc** (Demonstrates tangible backlog effect of single-maintainer bottleneck.)
- "We tried to do a fix ourselves, but we are unable to build the project based on the explanations found in README.md (see #1723)" — **comment by thisisthekap** (Contributor friction compounds the issue — build docs are insufficient, blocking community contributions.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `CONTRIBUTING.md` | — | context | CONTRIBUTING.md documents code contribution guidelines (PRs, tests, API patterns) and references documentation/dev/maintaining.md, but contains no information about how to become a maintainer, triage access, or the maintainer team composition. This is consistent with the reporter's concern that governance is opaque. |

### Next Questions

- Has @mattleibow or the Microsoft team responded in Discussion #1884 with governance plans?
- Has additional triage/maintainer access been granted to community contributors since January 2022?
- Is documentation/dev/maintaining.md populated with maintainer onboarding info?

### Resolution Proposals

**Hypothesis:** The project should either formally respond to the governance question or convert this issue to a GitHub Discussion where the community can engage on maintainer expansion plans.

1. **Convert back to GitHub Discussion** — alternative, confidence 0.80 (80%), cost/xs, validated=untested
   - Since this originated from Discussion #1884 and is a governance/community conversation (not a trackable code fix), convert the issue back to a GitHub Discussion under the 'General' category where the maintainer team can share governance plans and community members can discuss.
2. **Maintainer acknowledgment comment** — investigation, confidence 0.75 (75%), cost/xs, validated=untested
   - Post a comment from the maintainer team acknowledging the concern, describing the current maintainer/reviewer composition, and outlining any plans for expanding contributor or triage access.

**Recommended proposal:** Convert back to GitHub Discussion

**Why:** Governance conversations are better suited to GitHub Discussions (threaded Q&A, no close pressure). The issue originated as a discussion and has no code deliverable.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.75 (75%) |
| Reason | Valid community concern about project sustainability. No code fix exists; requires maintainer response on governance. Kept open as a signal until maintainers address or convert to discussion. |
| Suggested repro platform | — |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Confirm type/question, area/SkiaSharp, tenet/reliability labels (already applied) | labels=type/question, area/SkiaSharp, tenet/reliability |
| convert-to-discussion | high | 0.75 (75%) | Convert to GitHub Discussion (General) since this is a governance/community conversation originating from Discussion #1884, not a trackable code issue | category=General |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1920,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T15:54:56Z",
    "currentLabels": [
      "type/question",
      "area/SkiaSharp",
      "tenet/reliability",
      "triage/triaged"
    ]
  },
  "summary": "Community members question whether SkiaSharp has only one maintainer (@mattleibow) and request that additional maintainers or reviewers be added, citing mounting issues/PRs and broken Uno Platform 4.x builds as evidence of an unsustainable single-maintainer model for a library used across MAUI, Xamarin, Uno Platform, and Avalonia.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Originally from GitHub Discussion #1884 (December 2021), converted to issue January 2022. Comments span January–June 2022. No maintainer response on record. Issue mentions Uno Platform 4.x build breakage at time of filing.",
      "relatedIssues": [
        1723
      ]
    }
  },
  "analysis": {
    "summary": "This is a project governance question asking the maintainers to expand the contributor/reviewer team. No technical defect is described. The concern is about project sustainability — single-maintainer risk for a library critical to MAUI, Xamarin, Uno Platform, and Avalonia. The issue was converted from GitHub Discussion #1884 and has received no maintainer response.",
    "rationale": "The issue content is a governance/meta question, not a bug or feature request. Type/question is appropriate. The tenet/reliability label fits because the concern is about project reliability and sustainability. The area is SkiaSharp (general project). No platform/backend labels apply since this is a governance issue, not a technical one. CONTRIBUTING.md was examined: it documents contribution guidelines but no maintainer expansion process, confirming the gap the reporter describes.",
    "codeInvestigation": [
      {
        "file": "CONTRIBUTING.md",
        "finding": "CONTRIBUTING.md documents code contribution guidelines (PRs, tests, API patterns) and references documentation/dev/maintaining.md, but contains no information about how to become a maintainer, triage access, or the maintainer team composition. This is consistent with the reporter's concern that governance is opaque.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "having responsibility left to a single individual is a high risk",
        "source": "issue body",
        "interpretation": "Reporter identifies single-maintainer SPOF risk for a widely-used library."
      },
      {
        "text": "building with Uno Platform 4.x is completely broken right now",
        "source": "issue body",
        "interpretation": "Concrete production-impact example motivating the governance request."
      },
      {
        "text": "The issues continue to build up because there is only one person to get to them",
        "source": "comment by bmitc",
        "interpretation": "Demonstrates tangible backlog effect of single-maintainer bottleneck."
      },
      {
        "text": "We tried to do a fix ourselves, but we are unable to build the project based on the explanations found in README.md (see #1723)",
        "source": "comment by thisisthekap",
        "interpretation": "Contributor friction compounds the issue — build docs are insufficient, blocking community contributions."
      }
    ],
    "nextQuestions": [
      "Has @mattleibow or the Microsoft team responded in Discussion #1884 with governance plans?",
      "Has additional triage/maintainer access been granted to community contributors since January 2022?",
      "Is documentation/dev/maintaining.md populated with maintainer onboarding info?"
    ],
    "resolution": {
      "hypothesis": "The project should either formally respond to the governance question or convert this issue to a GitHub Discussion where the community can engage on maintainer expansion plans.",
      "proposals": [
        {
          "title": "Convert back to GitHub Discussion",
          "description": "Since this originated from Discussion #1884 and is a governance/community conversation (not a trackable code fix), convert the issue back to a GitHub Discussion under the 'General' category where the maintainer team can share governance plans and community members can discuss.",
          "category": "alternative",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Maintainer acknowledgment comment",
          "description": "Post a comment from the maintainer team acknowledging the concern, describing the current maintainer/reviewer composition, and outlining any plans for expanding contributor or triage access.",
          "category": "investigation",
          "confidence": 0.75,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Convert back to GitHub Discussion",
      "recommendedReason": "Governance conversations are better suited to GitHub Discussions (threaded Q&A, no close pressure). The issue originated as a discussion and has no code deliverable."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.75,
      "reason": "Valid community concern about project sustainability. No code fix exists; requires maintainer response on governance. Kept open as a signal until maintainers address or convert to discussion."
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Confirm type/question, area/SkiaSharp, tenet/reliability labels (already applied)",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "tenet/reliability"
        ]
      },
      {
        "type": "convert-to-discussion",
        "description": "Convert to GitHub Discussion (General) since this is a governance/community conversation originating from Discussion #1884, not a trackable code issue",
        "risk": "high",
        "confidence": 0.75,
        "category": "General"
      }
    ]
  }
}
```

</details>
