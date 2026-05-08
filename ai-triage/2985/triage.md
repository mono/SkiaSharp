# Issue Triage Report — #2985

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:16:32Z |
| Type | type/bug (0.82 (82%)) |
| Area | area/Build (0.80 (80%)) |
| Suggested action | ready-to-fix (0.80 (80%)) |

**Issue Summary:** The samples.zip file referenced in samples/README.md has been missing from GitHub releases since approximately August 2020, leaving users unable to download pre-built samples as advertised.

**Analysis:** The samples/README.md promises a samples.zip download on each GitHub release, but this artifact has not been uploaded to any release since approximately August 2020. The build cake script (scripts/cake/samples.cake) still generates the file, and the release-publish process documents an upload step for it, but the step appears to have been dropped in practice. The README link is stale.

**Recommendations:** **ready-to-fix** — Root cause is clear: README references a release artifact that is no longer being published. The immediate fix (updating the README) is straightforward and low-risk. A follow-up to restore the release upload is also well-defined.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/Build |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Navigate to https://github.com/mono/SkiaSharp/releases
2. Look for samples.zip attachment on any release after August 2020
3. Observe that no samples.zip file is attached to any release

**Repository links:**
- https://github.com/mono/SkiaSharp/releases — GitHub releases page — screenshot shows no samples.zip asset
- https://github.com/mono/SkiaSharp/blob/main/samples/README.md — README that references samples.zip on each release

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | True |
| Error type | missing-output |
| Error message | samples.zip not found in GitHub releases since approximately August 2020 |
| Repro quality | complete |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The README still references samples.zip on releases but the file has not been uploaded since approximately 2020; no code change has addressed this. |

## Analysis

### Technical Summary

The samples/README.md promises a samples.zip download on each GitHub release, but this artifact has not been uploaded to any release since approximately August 2020. The build cake script (scripts/cake/samples.cake) still generates the file, and the release-publish process documents an upload step for it, but the step appears to have been dropped in practice. The README link is stale.

### Rationale

The reporter provides a screenshot confirming no samples.zip exists on releases. The README directly promises this file. The build cake task exists to create it. The release-publish skill documents the upload step. The discrepancy is a clear release process gap. Classified as type/bug in area/Build because the release pipeline stopped producing a documented artifact.

### Key Signals

- "samples.zip mentioned at samples/README.md is missing since around August 2020" — **issue body** (Screenshot confirms the release asset has not been uploaded for approximately four years.)
- "So either the readme is outdated, or there is an issue with release process" — **issue body** (Code investigation confirms both: the README is stale and the release process no longer uploads the artifact.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `samples/README.md` | — | direct | README states 'check out the samples.zip file on each release' and links to https://github.com/mono/SkiaSharp/releases. This is a promise to users that is no longer fulfilled by the release pipeline. |
| `scripts/cake/samples.cake` | 1-20 | direct | The 'samples-generate' task creates output/samples.zip by calling CreateSamplesDirectory and Zip. The build infrastructure to generate samples.zip is intact and functional. |
| `.agents/skills/release-publish/SKILL.md` | 222-224 | related | Documents 'gh release upload {tag} samples.zip' with comment '# Upload samples for stable releases (if available)'. The upload step is conditional and appears to not be executed in practice. |

### Next Questions

- Was samples.zip successfully uploaded to pre-2020 releases?
- Was the samples upload step removed from CI/release automation around August 2020?
- Should the README be updated to point to the git repo samples directory, or should the release process be fixed to upload the artifact?

### Resolution Proposals

**Hypothesis:** The release process stopped uploading samples.zip around August 2020, either due to CI pipeline changes or an oversight. The README was never updated to reflect this.

1. **Update README to remove stale samples.zip reference** — workaround, confidence 0.92 (92%), cost/xs
   - Replace the samples.zip download link in samples/README.md with a note directing users to the samples/ directory in the repository and instructions to clone and build locally.
2. **Restore samples.zip upload in the release process** — fix, confidence 0.72 (72%), cost/m
   - Investigate why samples.zip stopped being uploaded around 2020 and restore the upload step in the release pipeline. The samples-generate cake task already creates the file; it just needs to be executed and uploaded during releases.

**Recommended proposal:** Update README to remove stale samples.zip reference

**Why:** Quick, low-risk fix that accurately reflects what is actually available. Restoring the release pipeline upload can be a separate follow-up.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.80 (80%) |
| Reason | Root cause is clear: README references a release artifact that is no longer being published. The immediate fix (updating the README) is straightforward and low-risk. A follow-up to restore the release upload is also well-defined. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply correct labels: bug type and Build area | labels=type/bug, area/Build |
| add-comment | medium | 0.85 (85%) | Acknowledge the issue, confirm the missing artifact, and point users to the samples directory in the repo as a workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for reporting this! You're right — `samples.zip` was referenced in `samples/README.md` and should be available as a release attachment, but it has not been uploaded to releases since approximately August 2020.

In the meantime, the samples are available directly in the repository under the `samples/` directory. You can clone the repository and build any of the sample projects from there.

We'll look at either restoring the `samples.zip` upload in the release process or updating the README to remove the stale reference.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2985,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:16:32Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "The samples.zip file referenced in samples/README.md has been missing from GitHub releases since approximately August 2020, leaving users unable to download pre-built samples as advertised.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.82
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.8
    }
  },
  "evidence": {
    "bugSignals": {
      "severity": "low",
      "regressionClaimed": true,
      "errorType": "missing-output",
      "errorMessage": "samples.zip not found in GitHub releases since approximately August 2020",
      "reproQuality": "complete"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Navigate to https://github.com/mono/SkiaSharp/releases",
        "Look for samples.zip attachment on any release after August 2020",
        "Observe that no samples.zip file is attached to any release"
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/releases",
          "description": "GitHub releases page — screenshot shows no samples.zip asset"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/blob/main/samples/README.md",
          "description": "README that references samples.zip on each release"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "The README still references samples.zip on releases but the file has not been uploaded since approximately 2020; no code change has addressed this."
    }
  },
  "analysis": {
    "summary": "The samples/README.md promises a samples.zip download on each GitHub release, but this artifact has not been uploaded to any release since approximately August 2020. The build cake script (scripts/cake/samples.cake) still generates the file, and the release-publish process documents an upload step for it, but the step appears to have been dropped in practice. The README link is stale.",
    "rationale": "The reporter provides a screenshot confirming no samples.zip exists on releases. The README directly promises this file. The build cake task exists to create it. The release-publish skill documents the upload step. The discrepancy is a clear release process gap. Classified as type/bug in area/Build because the release pipeline stopped producing a documented artifact.",
    "keySignals": [
      {
        "text": "samples.zip mentioned at samples/README.md is missing since around August 2020",
        "source": "issue body",
        "interpretation": "Screenshot confirms the release asset has not been uploaded for approximately four years."
      },
      {
        "text": "So either the readme is outdated, or there is an issue with release process",
        "source": "issue body",
        "interpretation": "Code investigation confirms both: the README is stale and the release process no longer uploads the artifact."
      }
    ],
    "codeInvestigation": [
      {
        "file": "samples/README.md",
        "finding": "README states 'check out the samples.zip file on each release' and links to https://github.com/mono/SkiaSharp/releases. This is a promise to users that is no longer fulfilled by the release pipeline.",
        "relevance": "direct"
      },
      {
        "file": "scripts/cake/samples.cake",
        "lines": "1-20",
        "finding": "The 'samples-generate' task creates output/samples.zip by calling CreateSamplesDirectory and Zip. The build infrastructure to generate samples.zip is intact and functional.",
        "relevance": "direct"
      },
      {
        "file": ".agents/skills/release-publish/SKILL.md",
        "lines": "222-224",
        "finding": "Documents 'gh release upload {tag} samples.zip' with comment '# Upload samples for stable releases (if available)'. The upload step is conditional and appears to not be executed in practice.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Was samples.zip successfully uploaded to pre-2020 releases?",
      "Was the samples upload step removed from CI/release automation around August 2020?",
      "Should the README be updated to point to the git repo samples directory, or should the release process be fixed to upload the artifact?"
    ],
    "resolution": {
      "hypothesis": "The release process stopped uploading samples.zip around August 2020, either due to CI pipeline changes or an oversight. The README was never updated to reflect this.",
      "proposals": [
        {
          "title": "Update README to remove stale samples.zip reference",
          "description": "Replace the samples.zip download link in samples/README.md with a note directing users to the samples/ directory in the repository and instructions to clone and build locally.",
          "category": "workaround",
          "confidence": 0.92,
          "effort": "cost/xs"
        },
        {
          "title": "Restore samples.zip upload in the release process",
          "description": "Investigate why samples.zip stopped being uploaded around 2020 and restore the upload step in the release pipeline. The samples-generate cake task already creates the file; it just needs to be executed and uploaded during releases.",
          "category": "fix",
          "confidence": 0.72,
          "effort": "cost/m"
        }
      ],
      "recommendedProposal": "Update README to remove stale samples.zip reference",
      "recommendedReason": "Quick, low-risk fix that accurately reflects what is actually available. Restoring the release pipeline upload can be a separate follow-up."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.8,
      "reason": "Root cause is clear: README references a release artifact that is no longer being published. The immediate fix (updating the README) is straightforward and low-risk. A follow-up to restore the release upload is also well-defined.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply correct labels: bug type and Build area",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/Build"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the issue, confirm the missing artifact, and point users to the samples directory in the repo as a workaround",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for reporting this! You're right — `samples.zip` was referenced in `samples/README.md` and should be available as a release attachment, but it has not been uploaded to releases since approximately August 2020.\n\nIn the meantime, the samples are available directly in the repository under the `samples/` directory. You can clone the repository and build any of the sample projects from there.\n\nWe'll look at either restoring the `samples.zip` upload in the release process or updating the README to remove the stale reference."
      }
    ]
  }
}
```

</details>
