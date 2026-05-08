# Issue Triage Report — #1208

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T20:44:33Z |
| Type | type/question (0.92 (92%)) |
| Area | area/SkiaSharp (0.85 (85%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** Community member showcases a visual programming language built with SkiaSharp and asks whether there is a place to gather projects that use SkiaSharp.

**Analysis:** This is a community showcase post, not a bug report. The author is grateful for SkiaSharp and shares their visual programming language built on top of it. The only question asked is whether there is a place to gather projects using SkiaSharp. The maintainer (mattleibow) responded positively suggesting a wiki. The issue would be more at home in GitHub Discussions as a community showcase.

**Recommendations:** **close-as-not-a-bug** — This is a community showcase post with an informal question already answered by the maintainer (wiki suggestion). Not a bug, feature request, or open technical question.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Environment:** No platform or version specified — cross-platform showcase

**Repository links:**
- https://visualprogramming.net/ — Visual programming language (vvvv) built on SkiaSharp

## Analysis

### Technical Summary

This is a community showcase post, not a bug report. The author is grateful for SkiaSharp and shares their visual programming language built on top of it. The only question asked is whether there is a place to gather projects using SkiaSharp. The maintainer (mattleibow) responded positively suggesting a wiki. The issue would be more at home in GitHub Discussions as a community showcase.

### Rationale

No bug or technical failure is described. The post is a thank-you and showcase with a single community question about a wiki or gallery for SkiaSharp-based projects. The maintainer's comment already answered the question informally. This pattern fits type/question best and can be closed as the question was answered.

### Key Signals

- "Is there any place where you gather projects that use SkiaSharp?" — **issue body** (Community question about a showcase gallery — not a bug or feature request.)
- "I think it is time for a wiki for cool things!" — **comment by mattleibow** (Maintainer acknowledged the question and informally answered it — wiki is the suggested venue.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `README.md` | — | context | README contains no 'Projects using SkiaSharp' or community showcase section, confirming that no official gallery existed at the time of the issue. |

### Resolution Proposals

**Hypothesis:** The reporter wants a community space to list projects using SkiaSharp. The GitHub wiki is a suitable place.

1. **Convert to Discussion** — alternative, confidence 0.90 (90%), cost/xs
   - Move this issue to GitHub Discussions (Show and Tell category) where community showcases belong.
2. **Point to GitHub Wiki** — workaround, confidence 0.85 (85%), cost/xs
   - Reply pointing to the GitHub wiki as the community-curated showcase page for SkiaSharp projects.

**Recommended proposal:** Convert to Discussion

**Why:** Community showcases are better served by GitHub Discussions than issues. Converting keeps the content accessible while clearing the issue queue.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | This is a community showcase post with an informal question already answered by the maintainer (wiki suggestion). Not a bug, feature request, or open technical question. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply question label | labels=type/question, area/SkiaSharp |
| convert-to-discussion | high | 0.88 (88%) | Convert to GitHub Discussions (Show and Tell) — community showcase posts belong there rather than the issue tracker | category=Show and Tell |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1208,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T20:44:33Z"
  },
  "summary": "Community member showcases a visual programming language built with SkiaSharp and asks whether there is a place to gather projects that use SkiaSharp.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.85
    }
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://visualprogramming.net/",
          "description": "Visual programming language (vvvv) built on SkiaSharp"
        }
      ],
      "environmentDetails": "No platform or version specified — cross-platform showcase"
    }
  },
  "analysis": {
    "summary": "This is a community showcase post, not a bug report. The author is grateful for SkiaSharp and shares their visual programming language built on top of it. The only question asked is whether there is a place to gather projects using SkiaSharp. The maintainer (mattleibow) responded positively suggesting a wiki. The issue would be more at home in GitHub Discussions as a community showcase.",
    "rationale": "No bug or technical failure is described. The post is a thank-you and showcase with a single community question about a wiki or gallery for SkiaSharp-based projects. The maintainer's comment already answered the question informally. This pattern fits type/question best and can be closed as the question was answered.",
    "codeInvestigation": [
      {
        "file": "README.md",
        "finding": "README contains no 'Projects using SkiaSharp' or community showcase section, confirming that no official gallery existed at the time of the issue.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "Is there any place where you gather projects that use SkiaSharp?",
        "source": "issue body",
        "interpretation": "Community question about a showcase gallery — not a bug or feature request."
      },
      {
        "text": "I think it is time for a wiki for cool things!",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer acknowledged the question and informally answered it — wiki is the suggested venue."
      }
    ],
    "resolution": {
      "hypothesis": "The reporter wants a community space to list projects using SkiaSharp. The GitHub wiki is a suitable place.",
      "proposals": [
        {
          "title": "Convert to Discussion",
          "description": "Move this issue to GitHub Discussions (Show and Tell category) where community showcases belong.",
          "category": "alternative",
          "confidence": 0.9,
          "effort": "cost/xs"
        },
        {
          "title": "Point to GitHub Wiki",
          "description": "Reply pointing to the GitHub wiki as the community-curated showcase page for SkiaSharp projects.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/xs"
        }
      ],
      "recommendedProposal": "Convert to Discussion",
      "recommendedReason": "Community showcases are better served by GitHub Discussions than issues. Converting keeps the content accessible while clearing the issue queue."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "This is a community showcase post with an informal question already answered by the maintainer (wiki suggestion). Not a bug, feature request, or open technical question.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question label",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "convert-to-discussion",
        "description": "Convert to GitHub Discussions (Show and Tell) — community showcase posts belong there rather than the issue tracker",
        "risk": "high",
        "confidence": 0.88,
        "category": "Show and Tell"
      }
    ]
  }
}
```

</details>
