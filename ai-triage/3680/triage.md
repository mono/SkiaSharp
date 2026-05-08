# Issue Triage Report — #3680

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T20:35:32Z |
| Type | type/feature-request (0.95 (95%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | keep-open (0.92 (92%)) |

**Issue Summary:** Maintainer-created tracking issue for evaluating and wrapping new Skia C++ APIs after the v4 Skia milestone bump, with sub-tasks for running a release-notes-audit, triaging new API value, and opening per-API sub-issues.

**Analysis:** This is a maintainer-authored engineering tracking issue for the SkiaSharp v4 release. It tracks the work of identifying and wrapping new Skia C++ APIs added in the latest bumped milestone. The prerequisite Skia bump (#3678) must land first; once done, a release-notes-audit determines which APIs to prioritize. Community contributors have already requested GlyphRun and related text APIs (PR #3491, issue #2397) as candidates.

**Recommendations:** **keep-open** — Valid maintainer-created tracking issue for planned v4 API work. Prerequisite #3678 (Skia bump) is still open. Issue should remain open until the milestone tasks are complete.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | upgrading/4.x, cost/m |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3684 — v4 release tracking parent issue
- https://github.com/mono/SkiaSharp/issues/3678 — Prerequisite: Skia bump to latest stable milestone
- https://github.com/mono/SkiaSharp/pull/3491 — Related PR mentioned by contributor Mikolaytis
- https://github.com/mono/SkiaSharp/issues/2397 — Related issue mentioned by contributor Mikolaytis

## Analysis

### Technical Summary

This is a maintainer-authored engineering tracking issue for the SkiaSharp v4 release. It tracks the work of identifying and wrapping new Skia C++ APIs added in the latest bumped milestone. The prerequisite Skia bump (#3678) must land first; once done, a release-notes-audit determines which APIs to prioritize. Community contributors have already requested GlyphRun and related text APIs (PR #3491, issue #2397) as candidates.

### Rationale

Issue is a well-scoped internal tracking issue created by a project maintainer for planned v4 API work. Classified as type/feature-request because the primary deliverable is new C# API wrappers for Skia functionality not yet exposed. No bug behavior or misuse — purely additive engineering. Keep open pending completion of prerequisite #3678.

### Key Signals

- "Part of #3684. After the Skia bump lands, evaluate what new APIs are available in the bumped milestone range" — **issue body** (Intentional tracking issue — not a user bug. Work is blocked on prerequisite #3678 (Skia bump).)
- "please dont forget https://github.com/mono/SkiaSharp/pull/3491 and https://github.com/mono/SkiaSharp/issues/2397" — **comment by Mikolaytis** (Community contributors have specific API requests queued for this evaluation cycle.)
- "Please expose GlyphRun and its builder. This is the modern text API and should be preferred." — **comment by Gillibald** (High-demand API request: GlyphRun/GlyphRunBuilder is the modern Skia text rendering path, preferred over legacy DrawText overloads.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKFont.cs` | — | direct | SKFont class exists with text measurement and rendering API; no GlyphRun or GlyphRunBuilder wrapper present in the current codebase, confirming the gap noted by contributor Gillibald |
| `binding/SkiaSharp/SKCanvas.cs` | 617-672 | related | DrawText and DrawTextBlob methods exist; no DrawGlyphRun or GlyphRun-based drawing API found, showing the modern Skia text path is not yet exposed in C# bindings |

### Next Questions

- Which Chrome milestone does the Skia bump target? This determines which new APIs are available.
- Is PR #3491 still relevant and applicable to the bumped milestone range?
- Will GlyphRun/GlyphRunBuilder be exposed as part of this issue or tracked in a dedicated sub-issue?

### Resolution Proposals

**Hypothesis:** Once the Skia bump (#3678) lands, run the skia-feature-scout skill to identify new APIs, then open per-API sub-issues for must-have additions targeting Preview 3.

1. **Run skia-feature-scout after bump lands** — investigation, confidence 0.95 (95%), cost/s, validated=untested
   - Use the skia-feature-scout skill to scan Skia release notes and identify new APIs worth wrapping. This produces a prioritized gap analysis for Preview 3.
2. **Prioritize GlyphRun/GlyphRunBuilder API** — investigation, confidence 0.80 (80%), cost/m, validated=untested
   - Community contributors explicitly request GlyphRun API exposure as the modern Skia text path. Evaluate whether the API is stable in the bumped milestone and open a sub-issue via the api-add-review skill if it qualifies.

**Recommended proposal:** Run skia-feature-scout after bump lands

**Why:** Systematic API discovery via the skia-feature-scout skill before manually triaging individual requests ensures no high-value APIs are missed and all requests are evaluated in context.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.92 (92%) |
| Reason | Valid maintainer-created tracking issue for planned v4 API work. Prerequisite #3678 (Skia bump) is still open. Issue should remain open until the milestone tasks are complete. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply type/feature-request, area/SkiaSharp, and tenet/compatibility labels | labels=type/feature-request, area/SkiaSharp, tenet/compatibility |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3680,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T20:35:32Z",
    "currentLabels": [
      "upgrading/4.x",
      "cost/m"
    ]
  },
  "summary": "Maintainer-created tracking issue for evaluating and wrapping new Skia C++ APIs after the v4 Skia milestone bump, with sub-tasks for running a release-notes-audit, triaging new API value, and opening per-API sub-issues.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3684",
          "description": "v4 release tracking parent issue"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3678",
          "description": "Prerequisite: Skia bump to latest stable milestone"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/3491",
          "description": "Related PR mentioned by contributor Mikolaytis"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2397",
          "description": "Related issue mentioned by contributor Mikolaytis"
        }
      ]
    }
  },
  "analysis": {
    "summary": "This is a maintainer-authored engineering tracking issue for the SkiaSharp v4 release. It tracks the work of identifying and wrapping new Skia C++ APIs added in the latest bumped milestone. The prerequisite Skia bump (#3678) must land first; once done, a release-notes-audit determines which APIs to prioritize. Community contributors have already requested GlyphRun and related text APIs (PR #3491, issue #2397) as candidates.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKFont.cs",
        "finding": "SKFont class exists with text measurement and rendering API; no GlyphRun or GlyphRunBuilder wrapper present in the current codebase, confirming the gap noted by contributor Gillibald",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "617-672",
        "finding": "DrawText and DrawTextBlob methods exist; no DrawGlyphRun or GlyphRun-based drawing API found, showing the modern Skia text path is not yet exposed in C# bindings",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Part of #3684. After the Skia bump lands, evaluate what new APIs are available in the bumped milestone range",
        "source": "issue body",
        "interpretation": "Intentional tracking issue — not a user bug. Work is blocked on prerequisite #3678 (Skia bump)."
      },
      {
        "text": "please dont forget https://github.com/mono/SkiaSharp/pull/3491 and https://github.com/mono/SkiaSharp/issues/2397",
        "source": "comment by Mikolaytis",
        "interpretation": "Community contributors have specific API requests queued for this evaluation cycle."
      },
      {
        "text": "Please expose GlyphRun and its builder. This is the modern text API and should be preferred.",
        "source": "comment by Gillibald",
        "interpretation": "High-demand API request: GlyphRun/GlyphRunBuilder is the modern Skia text rendering path, preferred over legacy DrawText overloads."
      }
    ],
    "rationale": "Issue is a well-scoped internal tracking issue created by a project maintainer for planned v4 API work. Classified as type/feature-request because the primary deliverable is new C# API wrappers for Skia functionality not yet exposed. No bug behavior or misuse — purely additive engineering. Keep open pending completion of prerequisite #3678.",
    "nextQuestions": [
      "Which Chrome milestone does the Skia bump target? This determines which new APIs are available.",
      "Is PR #3491 still relevant and applicable to the bumped milestone range?",
      "Will GlyphRun/GlyphRunBuilder be exposed as part of this issue or tracked in a dedicated sub-issue?"
    ],
    "resolution": {
      "hypothesis": "Once the Skia bump (#3678) lands, run the skia-feature-scout skill to identify new APIs, then open per-API sub-issues for must-have additions targeting Preview 3.",
      "proposals": [
        {
          "title": "Run skia-feature-scout after bump lands",
          "description": "Use the skia-feature-scout skill to scan Skia release notes and identify new APIs worth wrapping. This produces a prioritized gap analysis for Preview 3.",
          "category": "investigation",
          "confidence": 0.95,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Prioritize GlyphRun/GlyphRunBuilder API",
          "description": "Community contributors explicitly request GlyphRun API exposure as the modern Skia text path. Evaluate whether the API is stable in the bumped milestone and open a sub-issue via the api-add-review skill if it qualifies.",
          "category": "investigation",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Run skia-feature-scout after bump lands",
      "recommendedReason": "Systematic API discovery via the skia-feature-scout skill before manually triaging individual requests ensures no high-value APIs are missed and all requests are evaluated in context."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.92,
      "reason": "Valid maintainer-created tracking issue for planned v4 API work. Prerequisite #3678 (Skia bump) is still open. Issue should remain open until the milestone tasks are complete.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/feature-request, area/SkiaSharp, and tenet/compatibility labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "tenet/compatibility"
        ]
      }
    ]
  }
}
```

</details>
