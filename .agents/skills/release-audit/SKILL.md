---
name: release-audit
description: >-
  Audit and coordinate one or more SkiaSharp release lines. Use whenever a
  maintainer asks what release work is done, what should start/resume/finish,
  whether a range such as 4.150-4.155 or 4.15* is ready, or what blocks the next
  release action. Runs the repository audit command, aggregates its branch,
  internal build/BAR, NuGet, tag, GitHub Release, and upstream-sync evidence,
  investigates exceptional failures, and routes confirmed mutations to the
  owning release skills.
---

# Release state coordinator

Use the repository command as the source of release facts:

```powershell
pwsh ./scripts/infra/publishing/audit-release-state.ps1 -Version 4.152
```

The command is read-only and reports:

- maintenance and specific release branches;
- exact public NuGet provenance, tags, and GitHub Releases;
- exact-tip internal `skiasharp-package` build status and BAR ID;
- newer `chrome/mN` work and active sync PRs;
- pending next-milestone transitions; and
- dependency-ordered Finish, build, publication, sync, and Prepare actions.

Use `-Json` when coordinating several lines. Exit `0` means no action, `1`
means work or inconsistency remains, and `2` means a required check was
unavailable. Never reinterpret exit `2` as healthy.

## Resolve requested lines

Accept these natural selectors:

| Input | Meaning |
|---|---|
| `4.152` | One exact major/minor line |
| `4.150-4.155` or `4.150..4.155` | Inclusive numeric range |
| `4.15*` | Matching discovered release lines plus current/pending `main` |

For a numeric range, expand each minor. For a wildcard, discover unique lines
from release branches, exact public NuGet versions, exact tags, current
`main`, and the immediately following `skia-sync/mN -> main` transition.
Do not invent empty lines beyond current or pending evidence.

## Run and aggregate

Run independent line audits in parallel:

```powershell
pwsh ./scripts/infra/publishing/audit-release-state.ps1 `
  -Version 4.152 `
  -Json
```

Do not manually repeat branch ordering, build selection, BAR parsing, upstream
comparison, or action gating in the skill. Those deterministic rules belong to
the PowerShell command and its shared modules.

Lead with:

| Line | Current stage | Exact-tip build / BAR | Upstream / sync | Next action |
|---|---|---|---|---|

Then group exact actions under:

1. **Finish now** — public packages with incomplete tags or Releases.
2. **Resume publication** — prepared branches whose exact-tip build is green
   and has one BAR ID.
3. **Build blocked or running** — missing, active, failed, canceled, or BAR-less
   exact-tip builds.
4. **Merge or synchronize before branching** — upstream work and sync PRs.
5. **Start next releases** — only Prepare actions emitted by the script.
6. **Unavailable or inconsistent checks** — every exit `2`, provenance
   mismatch, ambiguous state, or unreachable service.

When a line has an already-cut branch plus newer maintenance/upstream work,
state the sequence explicitly: publish the existing branch, land the sync,
then rerun before cutting the following release.

## Investigate only exceptions

The command should provide enough information for normal release decisions.
Use AI investigation only when it reports:

- an unavailable internal pipeline or external service;
- a failed or canceled exact-tip build;
- several/missing BAR IDs;
- mismatched branch, commit, tag, or public package provenance;
- an incomplete cross-repository sync;
- an ambiguous release identity; or
- a workflow failure requiring logs or root-cause analysis.

Keep the original command output and exact IDs in the investigation. Do not
replace unavailable evidence with an assumption.

## Mutating handoffs

The audit and this skill remain read-only until the user asks to act:

- **Start:** invoke `release-branch`; show the Prepare dry run and planned refs
  before `push=true`.
- **Test a BAR:** invoke `release-testing` with the exact package version and
  BAR ID.
- **Publish packages:** the protected team BAR-to-NuGet process remains outside
  this skill.
- **Finish:** invoke `release-publish`; show the dry run and obtain confirmation
  before `push=true`.
- **Sync Skia:** follow `update-skia` and obtain confirmation before dispatch.

After any mutation completes, rerun the affected line. Never advance to the
next stage based only on a successfully queued workflow.
