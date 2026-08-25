---
name: release-status
description: >
  Check the current SkiaSharp release pipeline status. Use when the user asks
  how a release build is progressing, whether packages are ready, which build
  will be released, or what to do next. This is the second release step: resolve
  one exact commit, follow its connected Build and Tests runs, verify the BAR
  build and signed assets, and report the next action.
---

# Release Status

This skill is **Step 2 of 5**:

[release-branch](../release-branch/SKILL.md) → **release-status** →
[release-testing](../release-testing/SKILL.md) →
[release-publish](../release-publish/SKILL.md) →
[release-milestones](../release-milestones/SKILL.md)

## Contract

- Use `pipeline-status.py` for all Git, Azure DevOps, Darc, and BAR queries.
- This skill is read-only: it may fetch refs but never checks out a branch or
  queues, cancels, retries, promotes, or publishes anything.
- Resolve an exact release branch to its current remote tip; preserve a supplied
  commit exactly. `release/X.Y.x` is an integration branch: return to
  release-branch and cut an exact release branch before status tracking.
- Verify the target commit contains the minimum behavioral Arcade backport:
  combined Build role, folder-qualified Tests resource, fail-closed exact
  artifact selection, exact internal release versioning, plus metadata required
  to validate the historical transport package templates and branch-only
  transport staging so BAR contains one version per NonShipping package ID. Do
  not require Apple symbols or another main-only feature; BAR runtime evidence
  proves signing and asset routing.
- Select the newest combined Build attempt for that exact commit, then accept
  only a Tests run whose runtime pipeline resource points to that exact Build
  run, build number, and folder-qualified source.
- Resolve the Build run's `ReleaseConfigs` artifact to one exact BAR build ID.
  Verify BAR repository metadata, commit, branch, Build run/definition, stable
  flag, package versions, observed channels, and exact asset locations.
- Never combine runs, BAR metadata, or assets from different Build attempts.
- Reject BARs with duplicate NonShipping transport package IDs (for example,
  branch and commit aliases for `_NuGets`); only the branch identity belongs in
  BAR, while commit aliases may remain pipeline artifacts.
- Build, connected Tests, and both exact BAR-recorded package assets must be
  ready before the default testing handoff.
- The user may explicitly override only the wait for incomplete CI tests.
  Preserve all selected Build/Tests/BAR/package metadata and report the override.

## Pipeline chain

| Pipeline | ID | Role |
|----------|----|------|
| `skiasharp-package` | 1642 | Combined native/managed build, signing, BAR registration/validation |
| `skiasharp-tests` | 1630 | Connected tests consuming `\dotnet\skiasharp\skiasharp-package` |

The script links Tests through its runtime pipeline resource, reads the BAR ID
from the exact Build run's `ReleaseConfigs` artifact, and gets signed package
versions and locations from that immutable BAR record. Stable assets are exact
`X.Y.Z`; no `-stable.{build}` version is synthesized.

## Actions

| `nextAction` | Response |
|--------------|----------|
| `backport-arcade-release` | Show every `migration.missing[]` prerequisite and stop before querying/selecting builds. |
| `wait-for-build` | Report that the exact combined Build has not started or is running. |
| `retry-build` | Show the authoritative failed/canceled Build run. |
| `retry-bar-check` | Report the exact BAR registration or identity failure. |
| `wait-for-tests-trigger` | Build passed; connected Tests have not started. |
| `wait-for-tests` | Report tests progress; wait by default. |
| `retry-tests` | Show failed/canceled tests and jobs. |
| `configure-default-channels` | The exact Build has no default-channel mapping; configure the target release branch before retrying. |
| `configure-feed-routing` | The BAR does not record both core packages on the signed `skiasharp` feed; configure routing before retrying. |
| `start-release-testing` | Hand the immutable Build/Tests/BAR/package identity to release-testing. |

Only `start-release-testing` is ready by default.

## Workflow

Run:

```bash
python3 .agents/skills/release-status/scripts/pipeline-status.py \
  {release-branch-or-commit}
```

Render:

```markdown
## Release status

**Release:** `{branch}`
**Commit:** `{commit}`
**State:** `{state}`

| Pipeline | Status | Run | Build |
|----------|--------|-----|-------|
| Build | `{buildRun.state}` | [run `{buildRun.runId}`]({buildRun.url}) | `{buildRun.buildNumber}` |
| Tests | `{testsRun.state}` | [run `{testsRun.runId}`]({testsRun.url}) | `{testsRun.buildNumber}` |

**BAR build:** `{barBuild.id}` (`{barBuild.state}`)
**BAR channels:** `{barBuild.channels}`
**Default channel IDs:** `{barBuild.defaultChannelIds}`
**Test packages:** SkiaSharp `{packageVersions.test.SkiaSharp}`,
HarfBuzzSharp `{packageVersions.test.HarfBuzzSharp}`
**Public versions:** SkiaSharp `{packageVersions.public.SkiaSharp}`,
HarfBuzzSharp `{packageVersions.public.HarfBuzzSharp}`
**Next:** translate `{nextAction}`
```

Include both core assets' BAR-recorded locations, active/failed jobs, and every
warning. Omit missing run rows/links. Do not independently query or replace the
script-selected identity.

Exact untagged snapshots such as `release/4.150.3`,
`release/4.151.2`, and `release/4.152.0-rc.1` become status inputs after the
minimum Arcade backport is present. Their `.x` siblings have already advanced
to future integration state; `release/4.150.x` and `release/4.151.x` remain
Step 1 inputs for later `4.150.4` and `4.151.3` cuts.

For `start-release-testing`, invoke
[release-testing](../release-testing/SKILL.md) with the complete `buildRun`,
`testsRun`, `barBuild`, and exact package pairs. For wait/retry actions, report
the relevant run URL and stop. Never select or promote a BAR by channel name.

See [releasing.md](../../../documentation/dev/releasing.md) for the complete
release process.
