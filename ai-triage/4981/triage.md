# Issue Triage Report — #4981

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-09-17T04:28:06Z |
| Type | type/enhancement (0.97 (97%)) |
| Area | area/Build (0.95 (95%)) |
| Suggested action | needs-investigation (0.92 (92%)) |

**Issue Summary:** Add a repository-owned, fail-closed check that prevents new executable references to the legacy mono repository owner while preserving documented historical and transition exceptions.

**Analysis:** Current automation validation executes focused CI-status, Skia-update, review, workflow-safety, and sync-detector tests but has no repository-identity drift scanner. Active executable workflow sources still contain legacy mono repository references, including current allowlists, so a blanket scan would need the explicit exception taxonomy in the issue and must be sequenced after consumer migrations.

**Recommendations:** **needs-investigation** — The scope and desired guard behavior are precise, but activation is intentionally blocked on remaining consumer migrations and requires a complete executable-versus-historical inventory.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/Build |
| Platforms | — |
| Backends | — |
| Tenets | tenet/reliability |
| Perf | — |
| Partner | — |

## Evidence

### Reproduction

**Related issues:** #4960

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/4960 — Parent migration tracker defining the portable repository-identity end state and requiring drift protection.
- https://github.com/mono/SkiaSharp/issues/4986 — Closed portable repository identity foundation that this enforcement work can build on.
- https://github.com/mono/SkiaSharp/issues/4984 — Open related migration work that explicitly excludes the unused issue-* skill subtree from pre-move identity migration.
- https://github.com/mono/SkiaSharp/issues/4999 — Open consumer migration for Auto Triage's repository allowlist, one of the changes that should precede broad enforcement.

## Analysis

### Technical Summary

Current automation validation executes focused CI-status, Skia-update, review, workflow-safety, and sync-detector tests but has no repository-identity drift scanner. Active executable workflow sources still contain legacy mono repository references, including current allowlists, so a blanket scan would need the explicit exception taxonomy in the issue and must be sequenced after consumer migrations.

### Rationale

This is an enhancement to repository automation rather than a runtime defect: it requests new fail-closed validation and documents precise allowed historical/current values. The current code confirms no drift check is wired into automation tooling, while related migration work remains open and the issue explicitly requires enforcement to land after those consumers migrate. The appropriate next step is a scoped implementation investigation rather than closure or an immediate broad rewrite.

### Key Signals

- "Land drift enforcement last, after the smaller consumer PRs are merged." — **issue body** (The requested enforcement has an explicit dependency on existing migration work.)
- "Classify each legacy occurrence independently so one allowed historical token cannot hide another operational token on the same line." — **issue body** (The scanner needs token-level classification rather than a simple line allowlist.)
- "final drift enforcement must exclude `.agents/skills/issue-*` for now." — **issue comment** (The documented scope intentionally excludes the unused issue skill subtree.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `.github/workflows/automation-tooling-tests.yml` | 1-83 | direct | The automation validation workflow runs CI-status, update-skia, review-skia-update, workflow-safety, and sync-detector tests, but no repository-identity drift scanner or test is present. |
| `.github/workflows/auto-triage.md` | 45-47 | direct | The executable Auto Triage workflow has a current-only allowed-repos list containing mono/skiasharp and mono/skia, confirming consumer migration is still needed before a scanner can reject legacy operational coupling. |
| `.github/workflows/auto-skia-sync.md` | 88-90 | related | The executable Auto Skia Sync workflow also has a legacy mono-only allowed-repos list, demonstrating that current workflow literals require an explicit migration or narrowly documented exception before enforcement. |
| `.gitmodules` | 1-4 | context | The Skia submodule URL is currently https://github.com/mono/skia.git, which matches the issue's explicitly allowed current-live submodule metadata. |

### Workarounds

- Until consumer migrations merge, preserve current repository-owner values only in documented transition allowlists and avoid introducing new owner-specific literals in executable automation.

### Next Questions

- Which remaining consumer migrations besides Auto Triage must merge before the scanner becomes enforceable?
- Which existing workflow-safety test module should own the scanner's fixtures and token-level exception tests?
- Which package and site generation inputs are executable/current and which are historical provenance requiring documented exceptions?

### Resolution Proposals

**Hypothesis:** Repository-owner literals are currently distributed across executable automation and generated inputs, so future transfer safety requires a centralized scanner with narrowly justified exceptions rather than manually reviewing each new literal.

1. **Inventory and migrate remaining executable consumers** — investigation, confidence 0.95 (95%), cost/m, validated=untested
   - Complete the smaller owner-independent consumer migrations, including repository allowlists, before activating a fail-closed drift check so valid current automation is not blocked.
2. **Add token-level identity drift validation** — fix, confidence 0.91 (91%), cost/l, validated=untested
   - Add a repository-owned, case-insensitive scanner to the existing automation-tooling validation that examines executable workflows, actions, scripts, skills, package/site inputs, assignments, commands, comments, and fenced blocks. Encode each allowed current, transition, fixture, cache, schema, manifest, display-name, and historical-provenance occurrence with an explicit reason; test inline and block allowed-repos syntax against lookalike bypasses.

**Recommended proposal:** Inventory and migrate remaining executable consumers

**Why:** The issue explicitly makes enforcement dependent on consumer migrations, and the current Auto Triage and Auto Skia Sync sources still contain legacy-only operational allowlists.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.92 (92%) |
| Reason | The scope and desired guard behavior are precise, but activation is intentionally blocked on remaining consumer migrations and requires a complete executable-versus-historical inventory. |
| Suggested repro platform | — |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.98 (98%) | Apply enhancement, build, and reliability labels. | labels=type/enhancement, area/Build, tenet/reliability |
| link-related | low | 0.98 (98%) | Keep the parent repository portability tracker linked for dependency visibility. | linkedIssue=#4960 |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4981,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-09-17T04:28:06Z"
  },
  "summary": "Add a repository-owned, fail-closed check that prevents new executable references to the legacy mono repository owner while preserving documented historical and transition exceptions.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.97
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "relatedIssues": [
        4960
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/4960",
          "description": "Parent migration tracker defining the portable repository-identity end state and requiring drift protection."
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/4986",
          "description": "Closed portable repository identity foundation that this enforcement work can build on."
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/4984",
          "description": "Open related migration work that explicitly excludes the unused issue-* skill subtree from pre-move identity migration."
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/4999",
          "description": "Open consumer migration for Auto Triage's repository allowlist, one of the changes that should precede broad enforcement."
        }
      ]
    }
  },
  "analysis": {
    "summary": "Current automation validation executes focused CI-status, Skia-update, review, workflow-safety, and sync-detector tests but has no repository-identity drift scanner. Active executable workflow sources still contain legacy mono repository references, including current allowlists, so a blanket scan would need the explicit exception taxonomy in the issue and must be sequenced after consumer migrations.",
    "rationale": "This is an enhancement to repository automation rather than a runtime defect: it requests new fail-closed validation and documents precise allowed historical/current values. The current code confirms no drift check is wired into automation tooling, while related migration work remains open and the issue explicitly requires enforcement to land after those consumers migrate. The appropriate next step is a scoped implementation investigation rather than closure or an immediate broad rewrite.",
    "keySignals": [
      {
        "text": "Land drift enforcement last, after the smaller consumer PRs are merged.",
        "source": "issue body",
        "interpretation": "The requested enforcement has an explicit dependency on existing migration work."
      },
      {
        "text": "Classify each legacy occurrence independently so one allowed historical token cannot hide another operational token on the same line.",
        "source": "issue body",
        "interpretation": "The scanner needs token-level classification rather than a simple line allowlist."
      },
      {
        "text": "final drift enforcement must exclude `.agents/skills/issue-*` for now.",
        "source": "issue comment",
        "interpretation": "The documented scope intentionally excludes the unused issue skill subtree."
      }
    ],
    "codeInvestigation": [
      {
        "file": ".github/workflows/automation-tooling-tests.yml",
        "lines": "1-83",
        "finding": "The automation validation workflow runs CI-status, update-skia, review-skia-update, workflow-safety, and sync-detector tests, but no repository-identity drift scanner or test is present.",
        "relevance": "direct"
      },
      {
        "file": ".github/workflows/auto-triage.md",
        "lines": "45-47",
        "finding": "The executable Auto Triage workflow has a current-only allowed-repos list containing mono/skiasharp and mono/skia, confirming consumer migration is still needed before a scanner can reject legacy operational coupling.",
        "relevance": "direct"
      },
      {
        "file": ".github/workflows/auto-skia-sync.md",
        "lines": "88-90",
        "finding": "The executable Auto Skia Sync workflow also has a legacy mono-only allowed-repos list, demonstrating that current workflow literals require an explicit migration or narrowly documented exception before enforcement.",
        "relevance": "related"
      },
      {
        "file": ".gitmodules",
        "lines": "1-4",
        "finding": "The Skia submodule URL is currently https://github.com/mono/skia.git, which matches the issue's explicitly allowed current-live submodule metadata.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "Until consumer migrations merge, preserve current repository-owner values only in documented transition allowlists and avoid introducing new owner-specific literals in executable automation."
    ],
    "nextQuestions": [
      "Which remaining consumer migrations besides Auto Triage must merge before the scanner becomes enforceable?",
      "Which existing workflow-safety test module should own the scanner's fixtures and token-level exception tests?",
      "Which package and site generation inputs are executable/current and which are historical provenance requiring documented exceptions?"
    ],
    "resolution": {
      "hypothesis": "Repository-owner literals are currently distributed across executable automation and generated inputs, so future transfer safety requires a centralized scanner with narrowly justified exceptions rather than manually reviewing each new literal.",
      "proposals": [
        {
          "title": "Inventory and migrate remaining executable consumers",
          "description": "Complete the smaller owner-independent consumer migrations, including repository allowlists, before activating a fail-closed drift check so valid current automation is not blocked.",
          "category": "investigation",
          "validated": "untested",
          "confidence": 0.95,
          "effort": "cost/m"
        },
        {
          "title": "Add token-level identity drift validation",
          "description": "Add a repository-owned, case-insensitive scanner to the existing automation-tooling validation that examines executable workflows, actions, scripts, skills, package/site inputs, assignments, commands, comments, and fenced blocks. Encode each allowed current, transition, fixture, cache, schema, manifest, display-name, and historical-provenance occurrence with an explicit reason; test inline and block allowed-repos syntax against lookalike bypasses.",
          "category": "fix",
          "validated": "untested",
          "confidence": 0.91,
          "effort": "cost/l"
        }
      ],
      "recommendedProposal": "Inventory and migrate remaining executable consumers",
      "recommendedReason": "The issue explicitly makes enforcement dependent on consumer migrations, and the current Auto Triage and Auto Skia Sync sources still contain legacy-only operational allowlists."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.92,
      "reason": "The scope and desired guard behavior are precise, but activation is intentionally blocked on remaining consumer migrations and requires a complete executable-versus-historical inventory."
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement, build, and reliability labels.",
        "risk": "low",
        "confidence": 0.98,
        "labels": [
          "type/enhancement",
          "area/Build",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-related",
        "description": "Keep the parent repository portability tracker linked for dependency visibility.",
        "risk": "low",
        "confidence": 0.98,
        "linkedIssue": 4960
      }
    ]
  }
}
```

</details>
