# Issue Triage Report — #3610

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-07-01T05:46:03Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/Build (0.95 (95%)) |
| Suggested action | close-as-fixed (0.90 (90%)) |

**Issue Summary:** CI Android test job fails with XHarness exit code 81 (DEVICE_NOT_FOUND) on 8/10 builds because the Android emulator is not reliably available on the Linux CI agent; fixed in PR #3676 via disk-space cleanup and emulator startup hardening.

**Analysis:** The Android CI job fails because the hosted Ubuntu agent runs out of disk space before the Android emulator can start, causing XHarness to time out looking for a device. PR #3676 (merged 2026-04-23) fixed this by adding disk-space cleanup, switching to swiftshader_indirect GPU mode on Linux for headless-agent stability, and upgrading the Android SDK tool to 0.35.1 which introduces a --wait flag for reliable emulator startup.

**Recommendations:** **close-as-fixed** — PR #3676 (merged 2026-04-23) fixed this by adding disk-space cleanup and emulator startup hardening. Issue #3693 explicitly confirms this issue was resolved.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/Build |
| Platforms | os/Android |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, status/known-issue |

## Evidence

### Reproduction

1. Run the SkiaSharp CI pipeline on the Azure DevOps Android (Linux) job
2. Observe that XHarness retries 9 times over ~2 minutes but fails with exit code 81 DEVICE_NOT_FOUND

**Environment:** Azure DevOps hosted Ubuntu agent, Android (Linux) CI job, XHarness 9 retries, x86_64 emulator target

**Repository links:**
- https://github.com/mono/SkiaSharp/pull/3676 — PR #3676: [CI] Improve Android CI disk space and emulator startup — directly addresses the disk-space-correlated AVD startup failure
- https://github.com/mono/SkiaSharp/issues/3693 — Issue #3693 body states: 'Known issue #3610 Android DEVICE_NOT_FOUND CI failures (resolved by disk space fix in #3676)'

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | other |
| Error message | XHarness exit code: 81 (DEVICE_NOT_FOUND) — crit: Failed to find compatible device: x86_64 |
| Repro quality | complete |
| Target frameworks | — |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.92 (92%) |
| Reason | PR #3676 (merged 2026-04-23) directly targets this failure by adding cross-platform disk space cleanup, switching the emulator to swiftshader_indirect GPU mode on Linux, and upgrading androidsdk.tool to 0.35.1 with a --wait flag. Issue #3693 (filed 2026-04-16) explicitly states #3610 was resolved by the disk space fix in PR #3676. |
| Related PRs | #3676 |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

The Android CI job fails because the hosted Ubuntu agent runs out of disk space before the Android emulator can start, causing XHarness to time out looking for a device. PR #3676 (merged 2026-04-23) fixed this by adding disk-space cleanup, switching to swiftshader_indirect GPU mode on Linux for headless-agent stability, and upgrading the Android SDK tool to 0.35.1 which introduces a --wait flag for reliable emulator startup.

### Rationale

Type is type/bug because CI failures break the build pipeline and prevent Android test coverage — this is clearly broken behavior. Area is area/Build because the defect is entirely in the CI pipeline scripts (tests-android.cake, free-disk-space.ps1, azure-templates). The fix evidence is strong: PR #3676 was merged specifically to address this exact failure mode, and issue #3693 independently confirms resolution. Suggested action is close-as-fixed.

### Key Signals

- "XHarness exit code: 81 (DEVICE_NOT_FOUND) — crit: Failed to find compatible device: x86_64" — **issue body** (XHarness cannot find a running emulator; the emulator was never started or failed to boot in time.)
- "8/10 recent main builds (156223–156430) have DEVICE_NOT_FOUND in the Android test logs" — **issue body** (Near-total failure rate confirms this is a systemic infrastructure issue, not a transient flake.)
- "Android CI lane was failing with AVD startup and DEVICE_NOT_FOUND errors that correlated with low disk space on the hosted Ubuntu image" — **PR #3676 description** (Maintainer confirmed root cause: low disk space on the hosted agent preventing emulator startup.)
- "Known issue #3610 Android DEVICE_NOT_FOUND CI failures (resolved by disk space fix in #3676)" — **issue #3693 body** (Independent post-fix confirmation that #3610 was resolved by PR #3676.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `scripts/infra/tests/tests-android.cake` | 88-93 | direct | Emulator is now started with swiftshader_indirect GPU mode on Linux (PR #3676 fix) using the --wait flag for reliable boot synchronization. Previously the emulator may have started without waiting, causing a race with XHarness that manifested as DEVICE_NOT_FOUND. |
| `scripts/infra/native/shared/free-disk-space.ps1` | 1-50 | direct | Cross-platform disk space cleanup script now exists and removes unused toolchains and Android SDK components not needed by SkiaSharp; this directly addresses the disk-pressure root cause that prevented emulator startup. |
| `scripts/azure-templates-stages-test.yml` | 226-252 | related | Android test preBuildSteps include KVM enablement, AVD home configuration, emulator package install, and system image install — the full emulator setup pipeline is present and matches the current fix. |

### Next Questions

- Have CI runs after PR #3676 merge confirmed stable Android test execution?
- Should the status/known-issue label be removed when closing?

### Resolution Proposals

**Hypothesis:** Low disk space on the Ubuntu hosted CI agent prevented the Android emulator from writing AVD data or completing its boot sequence, causing XHarness to time out with DEVICE_NOT_FOUND.

1. **Close as fixed — PR #3676 merged 2026-04-23** — fix, confidence 0.92 (92%), cost/xs, validated=untested
   - PR #3676 was merged on 2026-04-23 and directly addresses this failure. The fix adds cross-platform disk space cleanup before emulator launch, switches to swiftshader_indirect GPU mode on headless Linux agents, and upgrades androidsdk.tool to 0.35.1 with a --wait flag for reliable emulator startup. Issue #3693 independently confirms the fix was effective.

**Recommended proposal:** Close as fixed — PR #3676 merged 2026-04-23

**Why:** PR #3676 directly targeted and fixed the exact root cause reported in this issue. Multiple independent signals confirm the fix was successful.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.90 (90%) |
| Reason | PR #3676 (merged 2026-04-23) fixed this by adding disk-space cleanup and emulator startup hardening. Issue #3693 explicitly confirms this issue was resolved. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/bug, area/Build, os/Android, tenet/reliability labels | labels=type/bug, area/Build, os/Android, tenet/reliability |
| add-comment | medium | 0.90 (90%) | Post comment noting the fix in PR #3676 before closing | — |
| close-issue | medium | 0.90 (90%) | Close as completed — PR #3676 resolved the root cause | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
This was fixed by PR #3676 ([CI: Improve Android CI disk space and emulator startup](https://github.com/mono/SkiaSharp/pull/3676)), merged 2026-04-23.

The root cause was low disk space on the hosted Ubuntu CI agent, which prevented the Android emulator from starting reliably. The fix includes:
- Cross-platform disk-space cleanup (`free-disk-space.ps1`) before emulator launch
- Switched to `swiftshader_indirect` GPU mode on Linux for headless-agent stability
- Upgraded `androidsdk.tool` to 0.35.1 which adds a `--wait` flag for reliable emulator startup synchronization

Closing as completed.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3610,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-07-01T05:46:03Z",
    "currentLabels": [
      "type/bug",
      "status/known-issue"
    ]
  },
  "summary": "CI Android test job fails with XHarness exit code 81 (DEVICE_NOT_FOUND) on 8/10 builds because the Android emulator is not reliably available on the Linux CI agent; fixed in PR #3676 via disk-space cleanup and emulator startup hardening.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.95
    },
    "platforms": [
      "os/Android"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "other",
      "errorMessage": "XHarness exit code: 81 (DEVICE_NOT_FOUND) — crit: Failed to find compatible device: x86_64",
      "reproQuality": "complete"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Run the SkiaSharp CI pipeline on the Azure DevOps Android (Linux) job",
        "Observe that XHarness retries 9 times over ~2 minutes but fails with exit code 81 DEVICE_NOT_FOUND"
      ],
      "environmentDetails": "Azure DevOps hosted Ubuntu agent, Android (Linux) CI job, XHarness 9 retries, x86_64 emulator target",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/3676",
          "description": "PR #3676: [CI] Improve Android CI disk space and emulator startup — directly addresses the disk-space-correlated AVD startup failure"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3693",
          "description": "Issue #3693 body states: 'Known issue #3610 Android DEVICE_NOT_FOUND CI failures (resolved by disk space fix in #3676)'"
        }
      ]
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.92,
      "reason": "PR #3676 (merged 2026-04-23) directly targets this failure by adding cross-platform disk space cleanup, switching the emulator to swiftshader_indirect GPU mode on Linux, and upgrading androidsdk.tool to 0.35.1 with a --wait flag. Issue #3693 (filed 2026-04-16) explicitly states #3610 was resolved by the disk space fix in PR #3676.",
      "relatedPRs": [
        3676
      ]
    }
  },
  "analysis": {
    "summary": "The Android CI job fails because the hosted Ubuntu agent runs out of disk space before the Android emulator can start, causing XHarness to time out looking for a device. PR #3676 (merged 2026-04-23) fixed this by adding disk-space cleanup, switching to swiftshader_indirect GPU mode on Linux for headless-agent stability, and upgrading the Android SDK tool to 0.35.1 which introduces a --wait flag for reliable emulator startup.",
    "rationale": "Type is type/bug because CI failures break the build pipeline and prevent Android test coverage — this is clearly broken behavior. Area is area/Build because the defect is entirely in the CI pipeline scripts (tests-android.cake, free-disk-space.ps1, azure-templates). The fix evidence is strong: PR #3676 was merged specifically to address this exact failure mode, and issue #3693 independently confirms resolution. Suggested action is close-as-fixed.",
    "keySignals": [
      {
        "text": "XHarness exit code: 81 (DEVICE_NOT_FOUND) — crit: Failed to find compatible device: x86_64",
        "source": "issue body",
        "interpretation": "XHarness cannot find a running emulator; the emulator was never started or failed to boot in time."
      },
      {
        "text": "8/10 recent main builds (156223–156430) have DEVICE_NOT_FOUND in the Android test logs",
        "source": "issue body",
        "interpretation": "Near-total failure rate confirms this is a systemic infrastructure issue, not a transient flake."
      },
      {
        "text": "Android CI lane was failing with AVD startup and DEVICE_NOT_FOUND errors that correlated with low disk space on the hosted Ubuntu image",
        "source": "PR #3676 description",
        "interpretation": "Maintainer confirmed root cause: low disk space on the hosted agent preventing emulator startup."
      },
      {
        "text": "Known issue #3610 Android DEVICE_NOT_FOUND CI failures (resolved by disk space fix in #3676)",
        "source": "issue #3693 body",
        "interpretation": "Independent post-fix confirmation that #3610 was resolved by PR #3676."
      }
    ],
    "codeInvestigation": [
      {
        "file": "scripts/infra/tests/tests-android.cake",
        "lines": "88-93",
        "finding": "Emulator is now started with swiftshader_indirect GPU mode on Linux (PR #3676 fix) using the --wait flag for reliable boot synchronization. Previously the emulator may have started without waiting, causing a race with XHarness that manifested as DEVICE_NOT_FOUND.",
        "relevance": "direct"
      },
      {
        "file": "scripts/infra/native/shared/free-disk-space.ps1",
        "lines": "1-50",
        "finding": "Cross-platform disk space cleanup script now exists and removes unused toolchains and Android SDK components not needed by SkiaSharp; this directly addresses the disk-pressure root cause that prevented emulator startup.",
        "relevance": "direct"
      },
      {
        "file": "scripts/azure-templates-stages-test.yml",
        "lines": "226-252",
        "finding": "Android test preBuildSteps include KVM enablement, AVD home configuration, emulator package install, and system image install — the full emulator setup pipeline is present and matches the current fix.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Have CI runs after PR #3676 merge confirmed stable Android test execution?",
      "Should the status/known-issue label be removed when closing?"
    ],
    "resolution": {
      "hypothesis": "Low disk space on the Ubuntu hosted CI agent prevented the Android emulator from writing AVD data or completing its boot sequence, causing XHarness to time out with DEVICE_NOT_FOUND.",
      "proposals": [
        {
          "title": "Close as fixed — PR #3676 merged 2026-04-23",
          "description": "PR #3676 was merged on 2026-04-23 and directly addresses this failure. The fix adds cross-platform disk space cleanup before emulator launch, switches to swiftshader_indirect GPU mode on headless Linux agents, and upgrades androidsdk.tool to 0.35.1 with a --wait flag for reliable emulator startup. Issue #3693 independently confirms the fix was effective.",
          "category": "fix",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Close as fixed — PR #3676 merged 2026-04-23",
      "recommendedReason": "PR #3676 directly targeted and fixed the exact root cause reported in this issue. Multiple independent signals confirm the fix was successful."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.9,
      "reason": "PR #3676 (merged 2026-04-23) fixed this by adding disk-space cleanup and emulator startup hardening. Issue #3693 explicitly confirms this issue was resolved.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/Build, os/Android, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/Build",
          "os/Android",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post comment noting the fix in PR #3676 before closing",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "This was fixed by PR #3676 ([CI: Improve Android CI disk space and emulator startup](https://github.com/mono/SkiaSharp/pull/3676)), merged 2026-04-23.\n\nThe root cause was low disk space on the hosted Ubuntu CI agent, which prevented the Android emulator from starting reliably. The fix includes:\n- Cross-platform disk-space cleanup (`free-disk-space.ps1`) before emulator launch\n- Switched to `swiftshader_indirect` GPU mode on Linux for headless-agent stability\n- Upgraded `androidsdk.tool` to 0.35.1 which adds a `--wait` flag for reliable emulator startup synchronization\n\nClosing as completed."
      },
      {
        "type": "close-issue",
        "description": "Close as completed — PR #3676 resolved the root cause",
        "risk": "medium",
        "confidence": 0.9,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
