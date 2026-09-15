# Issue Triage Report — #4987

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-09-15T04:27:33Z |
| Type | type/enhancement (0.99 (99%)) |
| Area | area/Build (0.96 (96%)) |
| Suggested action | keep-open (0.96 (96%)) |

**Issue Summary:** The migration task requests replacing canonical repository owner/name workflow gates with the stable SkiaSharp repository ID and temporarily authorizing both mono and dotnet repository slugs without changing the existing event or fork protections.

**Analysis:** This is a narrowly defined automation portability enhancement rather than a runtime defect: the current workflow sources gate execution on mutable mono owner/name values, and the issue specifies repository ID 52293126 plus constrained transition allowlists as the replacement. An open focused pull request already tracks the implementation, so the item should remain open until its source, regenerated locks, and focused tests are accepted.

**Recommendations:** **keep-open** — The enhancement has a defined active implementation path in open PR #4997 but no evidence it has merged or been validated against the required deterministic lock and workflow-test checks.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/Build |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility, tenet/reliability |
| Perf | — |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/4960 — Parent repository-identity migration tracker defining the stable repository-ID and transition-allowlist requirements.
- https://github.com/mono/SkiaSharp/pull/4964 — Prior broad identity-portability pull request from which this workflow-only scope was extracted.
- https://github.com/mono/SkiaSharp/pull/4997 — Open focused pull request referenced by the issue for the six canonical workflow-gate families.
- https://github.com/mono/SkiaSharp/issues/4999 — Related split issue for the Auto Triage transition allowlist, explicitly excluded from this issue's scope.

## Analysis

### Technical Summary

This is a narrowly defined automation portability enhancement rather than a runtime defect: the current workflow sources gate execution on mutable mono owner/name values, and the issue specifies repository ID 52293126 plus constrained transition allowlists as the replacement. An open focused pull request already tracks the implementation, so the item should remain open until its source, regenerated locks, and focused tests are accepted.

### Rationale

The issue requests a planned improvement to existing GitHub Actions and gh-aw workflow configuration, with explicit scope, exclusions, and completion criteria. Source inspection confirms the affected workflows presently use mono-specific canonical gates, while the parent tracker defines the stable repository ID and transition requirements. The linked open PR #4997 is an active implementation path, not evidence that the change has shipped.

### Key Signals

- "Replace canonical repository owner/name gates with stable repository ID 52293126 ... while preserving event/fork guards." — **issue body** (The desired change is a focused configuration enhancement with explicit safety invariants.)
- "Temporarily allow both mono/skiasharp and dotnet/skiasharp, plus both mono/skia and dotnet/skia where required; retain google/skia." — **issue body** (The transition allowlist is deliberately bounded and should not broaden unrelated access.)
- "The Auto Triage transition allowlist was intentionally split into #4999 so PR #4997 remains limited to the six reviewed workflow-gate families." — **issue comment #1** (The related Auto Triage work is intentionally excluded and should remain a separate change.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `.github/workflows/memory-leak-fixer.md` | 57,91-93 | direct | The workflow gates execution on github.repository == 'mono/SkiaSharp' and authorizes only mono/skiasharp for GitHub tool access. |
| `.github/workflows/performance-fixer.md` | 59,96-98 | direct | The workflow uses the same mono/SkiaSharp canonical gate and mono/skiasharp-only tool allowlist targeted by the issue. |
| `.github/workflows/merge-message.md` | 10,28-31 | direct | The slash-command workflow uses a mono/SkiaSharp gate and an allowlist containing mono/skiasharp, mono/skia, and google/skia. |
| `.github/workflows/track-benchmarks.yml` | 59,80,199,369 | direct | Benchmark tracking uses github.repository_owner == 'mono' at resolve, benchmark, PR-source, and reporting gates. |
| `.github/workflows/track-artifact-sizes.yml` | 57 | direct | Artifact-size tracking's resolve job is restricted by github.repository_owner == 'mono'. |
| `.github/workflows/pr-artifacts-comment.yml` | 24 | direct | The PR-artifacts comment job is restricted by github.repository_owner == 'mono'. |

### Workarounds

- Keep the current mono-hosted workflows in use until the focused migration change is merged; the present gates intentionally remain functional for the source repository.
- Keep the Auto Triage allowlist work isolated in #4999, as documented by the issue comment, rather than expanding this change beyond its six reviewed workflow families.

### Next Questions

- Does PR #4997 regenerate every affected gh-aw lock file deterministically from its Markdown source?
- Do the focused workflow tests assert both mono and dotnet transition allowlists while preserving event and fork guards?

### Resolution Proposals

**Hypothesis:** Mutable owner/name equality gates would prevent the specified canonical workflows from operating after the planned mono-to-dotnet repository transfer, despite the repository retaining its stable GitHub repository ID.

1. **Complete the focused workflow-gate migration** — fix, confidence 0.94 (94%), cost/m, validated=untested
   - Review and merge the existing focused implementation in PR #4997 after confirming it replaces only the listed canonical gates with the stable repository ID, preserves event/fork protections, adds the bounded mono+dotnet allowlists where required, regenerates locks deterministically, and passes the relevant workflow tests.
2. **Keep Auto Triage as a separate change** — workaround, confidence 0.98 (98%), cost/xs, validated=untested
   - Leave the Auto Triage transition allowlist to #4999 so the review and validation scope of this issue remains limited to the six documented workflow-gate families.

**Recommended proposal:** Complete the focused workflow-gate migration

**Why:** It directly implements the scoped migration requirements and already has an open focused pull request, while retaining safety checks and avoiding the intentionally separated Auto Triage scope.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.96 (96%) |
| Reason | The enhancement has a defined active implementation path in open PR #4997 but no evidence it has merged or been validated against the required deterministic lock and workflow-test checks. |
| Suggested repro platform | — |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.99 (99%) | Apply enhancement, build, compatibility, and reliability labels. | labels=type/enhancement, area/Build, tenet/compatibility, tenet/reliability |
| link-related | low | 0.98 (98%) | Retain the related Auto Triage migration issue as explicitly split work. | linkedIssue=#4999 |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4987,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-09-15T04:27:33Z",
    "currentLabels": []
  },
  "summary": "The migration task requests replacing canonical repository owner/name workflow gates with the stable SkiaSharp repository ID and temporarily authorizing both mono and dotnet repository slugs without changing the existing event or fork protections.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.99
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.96
    },
    "tenets": [
      "tenet/compatibility",
      "tenet/reliability"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/4960",
          "description": "Parent repository-identity migration tracker defining the stable repository-ID and transition-allowlist requirements."
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/4964",
          "description": "Prior broad identity-portability pull request from which this workflow-only scope was extracted."
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/4997",
          "description": "Open focused pull request referenced by the issue for the six canonical workflow-gate families."
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/4999",
          "description": "Related split issue for the Auto Triage transition allowlist, explicitly excluded from this issue's scope."
        }
      ]
    }
  },
  "analysis": {
    "summary": "This is a narrowly defined automation portability enhancement rather than a runtime defect: the current workflow sources gate execution on mutable mono owner/name values, and the issue specifies repository ID 52293126 plus constrained transition allowlists as the replacement. An open focused pull request already tracks the implementation, so the item should remain open until its source, regenerated locks, and focused tests are accepted.",
    "rationale": "The issue requests a planned improvement to existing GitHub Actions and gh-aw workflow configuration, with explicit scope, exclusions, and completion criteria. Source inspection confirms the affected workflows presently use mono-specific canonical gates, while the parent tracker defines the stable repository ID and transition requirements. The linked open PR #4997 is an active implementation path, not evidence that the change has shipped.",
    "keySignals": [
      {
        "text": "Replace canonical repository owner/name gates with stable repository ID 52293126 ... while preserving event/fork guards.",
        "source": "issue body",
        "interpretation": "The desired change is a focused configuration enhancement with explicit safety invariants."
      },
      {
        "text": "Temporarily allow both mono/skiasharp and dotnet/skiasharp, plus both mono/skia and dotnet/skia where required; retain google/skia.",
        "source": "issue body",
        "interpretation": "The transition allowlist is deliberately bounded and should not broaden unrelated access."
      },
      {
        "text": "The Auto Triage transition allowlist was intentionally split into #4999 so PR #4997 remains limited to the six reviewed workflow-gate families.",
        "source": "issue comment #1",
        "interpretation": "The related Auto Triage work is intentionally excluded and should remain a separate change."
      }
    ],
    "codeInvestigation": [
      {
        "file": ".github/workflows/memory-leak-fixer.md",
        "lines": "57,91-93",
        "finding": "The workflow gates execution on github.repository == 'mono/SkiaSharp' and authorizes only mono/skiasharp for GitHub tool access.",
        "relevance": "direct"
      },
      {
        "file": ".github/workflows/performance-fixer.md",
        "lines": "59,96-98",
        "finding": "The workflow uses the same mono/SkiaSharp canonical gate and mono/skiasharp-only tool allowlist targeted by the issue.",
        "relevance": "direct"
      },
      {
        "file": ".github/workflows/merge-message.md",
        "lines": "10,28-31",
        "finding": "The slash-command workflow uses a mono/SkiaSharp gate and an allowlist containing mono/skiasharp, mono/skia, and google/skia.",
        "relevance": "direct"
      },
      {
        "file": ".github/workflows/track-benchmarks.yml",
        "lines": "59,80,199,369",
        "finding": "Benchmark tracking uses github.repository_owner == 'mono' at resolve, benchmark, PR-source, and reporting gates.",
        "relevance": "direct"
      },
      {
        "file": ".github/workflows/track-artifact-sizes.yml",
        "lines": "57",
        "finding": "Artifact-size tracking's resolve job is restricted by github.repository_owner == 'mono'.",
        "relevance": "direct"
      },
      {
        "file": ".github/workflows/pr-artifacts-comment.yml",
        "lines": "24",
        "finding": "The PR-artifacts comment job is restricted by github.repository_owner == 'mono'.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Keep the current mono-hosted workflows in use until the focused migration change is merged; the present gates intentionally remain functional for the source repository.",
      "Keep the Auto Triage allowlist work isolated in #4999, as documented by the issue comment, rather than expanding this change beyond its six reviewed workflow families."
    ],
    "nextQuestions": [
      "Does PR #4997 regenerate every affected gh-aw lock file deterministically from its Markdown source?",
      "Do the focused workflow tests assert both mono and dotnet transition allowlists while preserving event and fork guards?"
    ],
    "resolution": {
      "hypothesis": "Mutable owner/name equality gates would prevent the specified canonical workflows from operating after the planned mono-to-dotnet repository transfer, despite the repository retaining its stable GitHub repository ID.",
      "proposals": [
        {
          "title": "Complete the focused workflow-gate migration",
          "description": "Review and merge the existing focused implementation in PR #4997 after confirming it replaces only the listed canonical gates with the stable repository ID, preserves event/fork protections, adds the bounded mono+dotnet allowlists where required, regenerates locks deterministically, and passes the relevant workflow tests.",
          "category": "fix",
          "validated": "untested",
          "confidence": 0.94,
          "effort": "cost/m"
        },
        {
          "title": "Keep Auto Triage as a separate change",
          "description": "Leave the Auto Triage transition allowlist to #4999 so the review and validation scope of this issue remains limited to the six documented workflow-gate families.",
          "category": "workaround",
          "validated": "untested",
          "confidence": 0.98,
          "effort": "cost/xs"
        }
      ],
      "recommendedProposal": "Complete the focused workflow-gate migration",
      "recommendedReason": "It directly implements the scoped migration requirements and already has an open focused pull request, while retaining safety checks and avoiding the intentionally separated Auto Triage scope."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.96,
      "reason": "The enhancement has a defined active implementation path in open PR #4997 but no evidence it has merged or been validated against the required deterministic lock and workflow-test checks."
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement, build, compatibility, and reliability labels.",
        "risk": "low",
        "confidence": 0.99,
        "labels": [
          "type/enhancement",
          "area/Build",
          "tenet/compatibility",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-related",
        "description": "Retain the related Auto Triage migration issue as explicitly split work.",
        "risk": "low",
        "confidence": 0.98,
        "linkedIssue": 4999
      }
    ]
  }
}
```

</details>
