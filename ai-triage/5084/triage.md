# Issue Triage Report — #5084

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-09-26T04:26:07Z |
| Type | type/bug (0.98 (98%)) |
| Area | area/Build (0.98 (98%)) |
| Suggested action | ready-to-fix (0.96 (96%)) |

**Issue Summary:** The scheduled Sync - Skia Upstream workflow failed because its staged create_pull_request output rejected a base-branch override that the workflow supplies for its generated pull request.

**Analysis:** This is a workflow-configuration bug that prevents a detected Skia sync from reporting successful completion after its engineering work, because the declared completion safe-output rejects the workflow's selected base branch.

**Recommendations:** **ready-to-fix** — The failure message, workflow source, and compiled safe-output policy identify the missing allowlist and the required repair path.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/Build |
| Platforms | — |
| Backends | — |
| Tenets | tenet/reliability |
| Perf | — |
| Partner | — |
| Current labels | agentic-workflows |

## Evidence

### Reproduction

1. Run the Sync - Skia Upstream workflow with a resolved base branch.
2. Allow the workflow to invoke its staged create_pull_request completion output.
3. Observe the safe-output policy reject the supplied base-branch override.

**Environment:** GitHub Actions workflow run 34970028940; issue created by github-actions[bot].

**Repository links:**
- https://github.com/mono/SkiaSharp/actions/runs/34970028940 — Failed Sync - Skia Upstream workflow run cited by the issue.

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | build-error |
| Error message | create_pull_request: Base branch override is not allowed. Configure safe-outputs.create-pull-request.allowed-base-branches to allow per-run base overrides. |
| Repro quality | complete |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | gh-aw v0.88.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The checked-in compiled workflow policy still has no allowed_base_branches entry for create_pull_request. |

## Analysis

### Technical Summary

This is a workflow-configuration bug that prevents a detected Skia sync from reporting successful completion after its engineering work, because the declared completion safe-output rejects the workflow's selected base branch.

### Rationale

The failure message names the rejected safe-output policy and the required configuration. The source workflow accepts and resolves a base_branch, while the compiled lock file's create_pull_request configuration contains no allowed_base_branches setting. This is a deterministic CI configuration defect rather than a SkiaSharp rendering or package issue.

### Key Signals

- "Code Push Failed: A code push safe output failed, and subsequent safe outputs were cancelled." — **issue body** (The workflow's delivery stage did not complete.)
- "create_pull_request: Base branch override is not allowed." — **issue body** (The failure is an explicit safe-output policy rejection.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `.github/workflows/auto-skia-sync.md` | 20-23, 119-133 | direct | The workflow exposes a base_branch dispatch input, resolves it into runtime state, and declares staged create-pull-request output, so the workflow has an intentional branch-selection path that reaches the completion output. |
| `.github/workflows/auto-skia-sync.lock.yml` | GH_AW_SAFE_OUTPUTS_CONFIG entries for create_pull_request | direct | The compiled create_pull_request policy lists staged and limits but no allowed_base_branches value, matching the policy error in the failed run. |

### Workarounds

- Until the policy is corrected, avoid manual base-branch overrides and run only the workflow path whose completion output targets the policy's default branch.

### Resolution Proposals

**Hypothesis:** The workflow source needs an allowed-base-branches policy for create-pull-request that covers the base branches selected by the sync detector, followed by regeneration of the lock workflow.

1. **Allow the resolved sync base branches** — fix, confidence 0.96 (96%), cost/s, validated=untested
   - Configure create-pull-request allowed-base-branches for the supported sync target branches, regenerate auto-skia-sync.lock.yml, and rerun the failed workflow.

**Recommended proposal:** Allow the resolved sync base branches

**Why:** The safe-output error explicitly requests this policy setting, and the source/compiled workflow mismatch confirms the missing configuration.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.96 (96%) |
| Reason | The failure message, workflow source, and compiled safe-output policy identify the missing allowlist and the required repair path. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.98 (98%) | Apply workflow build and reliability classification labels. | labels=type/bug, area/Build, tenet/reliability |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 5084,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-09-26T04:26:07Z",
    "currentLabels": [
      "agentic-workflows"
    ]
  },
  "summary": "The scheduled Sync - Skia Upstream workflow failed because its staged create_pull_request output rejected a base-branch override that the workflow supplies for its generated pull request.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.98
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.98
    },
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "build-error",
      "errorMessage": "create_pull_request: Base branch override is not allowed. Configure safe-outputs.create-pull-request.allowed-base-branches to allow per-run base overrides.",
      "reproQuality": "complete"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Run the Sync - Skia Upstream workflow with a resolved base branch.",
        "Allow the workflow to invoke its staged create_pull_request completion output.",
        "Observe the safe-output policy reject the supplied base-branch override."
      ],
      "environmentDetails": "GitHub Actions workflow run 34970028940; issue created by github-actions[bot].",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/actions/runs/34970028940",
          "description": "Failed Sync - Skia Upstream workflow run cited by the issue."
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "gh-aw v0.88.2"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The checked-in compiled workflow policy still has no allowed_base_branches entry for create_pull_request."
    }
  },
  "analysis": {
    "summary": "This is a workflow-configuration bug that prevents a detected Skia sync from reporting successful completion after its engineering work, because the declared completion safe-output rejects the workflow's selected base branch.",
    "rationale": "The failure message names the rejected safe-output policy and the required configuration. The source workflow accepts and resolves a base_branch, while the compiled lock file's create_pull_request configuration contains no allowed_base_branches setting. This is a deterministic CI configuration defect rather than a SkiaSharp rendering or package issue.",
    "keySignals": [
      {
        "text": "Code Push Failed: A code push safe output failed, and subsequent safe outputs were cancelled.",
        "source": "issue body",
        "interpretation": "The workflow's delivery stage did not complete."
      },
      {
        "text": "create_pull_request: Base branch override is not allowed.",
        "source": "issue body",
        "interpretation": "The failure is an explicit safe-output policy rejection."
      }
    ],
    "codeInvestigation": [
      {
        "file": ".github/workflows/auto-skia-sync.md",
        "lines": "20-23, 119-133",
        "finding": "The workflow exposes a base_branch dispatch input, resolves it into runtime state, and declares staged create-pull-request output, so the workflow has an intentional branch-selection path that reaches the completion output.",
        "relevance": "direct"
      },
      {
        "file": ".github/workflows/auto-skia-sync.lock.yml",
        "lines": "GH_AW_SAFE_OUTPUTS_CONFIG entries for create_pull_request",
        "finding": "The compiled create_pull_request policy lists staged and limits but no allowed_base_branches value, matching the policy error in the failed run.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Until the policy is corrected, avoid manual base-branch overrides and run only the workflow path whose completion output targets the policy's default branch."
    ],
    "resolution": {
      "hypothesis": "The workflow source needs an allowed-base-branches policy for create-pull-request that covers the base branches selected by the sync detector, followed by regeneration of the lock workflow.",
      "proposals": [
        {
          "title": "Allow the resolved sync base branches",
          "description": "Configure create-pull-request allowed-base-branches for the supported sync target branches, regenerate auto-skia-sync.lock.yml, and rerun the failed workflow.",
          "category": "fix",
          "confidence": 0.96,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Allow the resolved sync base branches",
      "recommendedReason": "The safe-output error explicitly requests this policy setting, and the source/compiled workflow mismatch confirms the missing configuration."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.96,
      "reason": "The failure message, workflow source, and compiled safe-output policy identify the missing allowlist and the required repair path.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply workflow build and reliability classification labels.",
        "risk": "low",
        "confidence": 0.98,
        "labels": [
          "type/bug",
          "area/Build",
          "tenet/reliability"
        ]
      }
    ]
  }
}
```

</details>
