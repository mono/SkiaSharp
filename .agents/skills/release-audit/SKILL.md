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
- pre-transition main-line sync PRs left open after a servicing branch is cut;
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
the PowerShell command and its shared modules. It prefers the newest succeeded
BAR build on the exact branch commit; newer canceled or failed reruns are
reported without displacing that build. A partially succeeded build with a
BAR is reported for review, not marked ready.

## Report tables

Keep the three views below, in this order, and render each as a Markdown
table, not paragraphs or bullet lists. Each view has its own build and action:
ship the current cut, ready the maintenance branch, then land upstream work.
Do not split public packages, prepared cuts, build evidence, and release actions
into separate summaries.

### 1. Current release — state as of the audit

| Line | On NuGet | Prepared cut | Build / BAR | Release action |
|---|---|---|---|---|
| A.B | Exact public version | Cut awaiting publication, or None | Linked exact-cut build, result and BAR | Release action emitted by the audit |

The prepared cut is an existing release branch, not proof CI passed. Use only
its own exact-SHA build/BAR here, never `.x`/`main` CI. Show Finish when public
packages need tags/Releases; show Publish or Prepare only when emitted by the
audit. If none is emitted, say so, rather than substituting a sync action.
Keep partial builds, unavailable checks, provenance errors, and newer
failed/canceled reruns explicit.

### 2. Branch readiness — get the base ready for Prepare

| Line | Base branch / version | Additional commits | Open PRs | Build / BAR | Readiness action |
|---|---|---|---|---|---|
| A.B | `.x` or `main`, plus checked-in version | Merged since the latest cut, excluding the version bump | Linked PRs targeting this line | Linked exact-base build, result and BAR | Fix/review/wait for base CI, review pending PRs, or base ready |

Use `openPullRequests` to retain the open work targeting each line, including
backports and draft PRs. Name the target when a PR targets a cut branch rather
than `.x`. Open PRs are not additional merged commits, nor automatically
required for the release: review their inclusion rather than assuming all
must merge. An older JSON snapshot without this check means Not checked,
not None.

This table's action concerns the source branch, not shipping an existing cut.
Base readiness is not permission to Prepare: current-shipment and upstream
gates still apply. The source version is not a public package version, and
its BAR is not the prepared cut's BAR.

### 3. Upstream / sync — land upstream changes

| Line | Base branch | Upstream commits | Merge / sync PRs | Sync build | Sync action |
|---|---|---|---|---|---|
| A.B | Target `.x` or `main` | Gap and actual native branch compared | Native PR, then parent PR, with draft/open state | Linked exact-sync-branch package build, or Not checked / Not built / N/A | Sync action emitted by the audit |

The sync build belongs to `incomingPullRequest.HeadBranch` at `HeadSha`,
from `packageBuilds`; do not borrow maintenance or prepared-release CI, or
call it a PR merge build. Keep all sync actions here. `Current` on an unmerged
sync branch does not mean its changes landed in the release source.
Upstream work concerns the next cut, not publication/Finish of an existing cut.

Put full SHAs, exceptional details, and exact workflow inputs below their
table when needed, without repeating rows as paragraphs. Keep the three
tables together instead of adding a section per fact. Historical finished
shipments remain in `-Json`. No reporting change bypasses release gates.

Always name and link every `incomingPullRequest` in the upstream section, including
draft PRs with `BlocksRelease: false`. Draft means not ready to merge, not
absent: identify the review needed and do not describe the sync branch as
already merged merely because its upstream comparison says `current`. For
cross-repository sync, check the corresponding open `mono/skia` PR on the same
sync branch and link the native PR before the parent SkiaSharp PR. Distinguish
PR review from a release-cut blocker and from a newer upstream commit.

When a line has an already-cut branch plus newer maintenance/upstream work,
state the sequence explicitly: publish the existing branch, land the sync,
then rerun before cutting the following release.

## Investigate only exceptions

The command should provide enough information for normal release decisions.
Use AI investigation only when it reports:

- an unavailable internal pipeline or external service;
- a failed or canceled exact-tip build without an earlier succeeded BAR build;
- a partially succeeded BAR build offered for review;
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
- **Publish packages:** when explicitly requested, invoke `release-publish` to
  verify the exact producing Build/Tests runs and BAR, then queue the protected
  MAUI pipeline from chat. The pipeline's human approval remains mandatory;
  an audit never queues it automatically. The audit does not track already-queued
  MAUI publication runs, so check for one before dispatch.
- **Finish:** invoke `release-publish` after exact packages are public; show the
  Finish workflow's read-only plan and obtain confirmation before `push=true`.
- **Sync Skia:** follow `update-skia` and obtain confirmation before dispatch.

After any mutation completes, rerun the affected line. Never advance to the
next stage based only on a successfully queued workflow.
