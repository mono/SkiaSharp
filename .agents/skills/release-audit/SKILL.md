---
name: release-audit
description: >-
  Audit and coordinate one or more SkiaSharp release lines. Use whenever a
  maintainer asks what release work is done, what should start/resume/finish,
  whether a range such as 4.150-4.155 or 4.15* is ready, whether prepared
  branches built successfully, or what upstream Skia work blocks the next cut.
  Correlates release branches, internal skiasharp-package builds and BAR IDs,
  NuGet, tags, GitHub Releases, upstream chrome/mN state, and sync PRs into
  ordered next actions.
---

# Release state coordinator

Use this skill as a read-only coordinator. Gather the current state first, then
present the exact commands or owner workflows needed to continue. Do not start
builds, publish packages, dispatch syncs, create release branches, or run
Finish unless the user explicitly asks and confirms the mutating operation.

The coordinator answers:

1. Is a public NuGet shipment waiting for Finish?
2. Is a prepared release branch built successfully and ready for testing or
   publication?
3. Does newer `chrome/mN` work or an existing sync PR need to land before the
   next branch is cut?
4. Is the line ready for Release - Prepare?
5. Is the immediately following milestone already progressing toward `main`?

## Inputs

Accept natural release-line selectors:

| Input | Meaning |
|---|---|
| `4.152` | One exact major/minor line |
| `4.150-4.155` or `4.150..4.155` | Inclusive numeric range |
| `4.15*` | Discover matching lines from release branches, NuGet, tags, current `main`, and the immediately following milestone sync |

Reject ambiguous nonnumeric bounds. Preserve every exact public prerelease
shipment; do not collapse several builds into one abbreviated identity.

## 1. Resolve the line set

For a range, expand each numeric minor and run independent reads in parallel.
For a wildcard, combine these read-only sources and sort the unique lines:

- `release/A.B.x` and specific `release/<identity>` branches on origin;
- exact public SkiaSharp versions from the NuGet flat-container catalog;
- exact `vA.B.*` tags;
- the SkiaSharp version and Skia milestone recorded by current `main`; and
- open `skia-sync/mN -> main` pull requests for the immediately following
  milestone.

Do not invent empty lines beyond the discovered current or pending milestone.

## 2. Run the repository audit

Run each line with structured output:

```powershell
pwsh ./scripts/infra/publishing/audit-release-state.ps1 `
  -Version 4.152 `
  -Json
```

Issue independent lines as parallel tool calls rather than serial shell
background jobs. Preserve exit codes:

- `0`: no high-level action;
- `1`: release work or an inconsistency remains;
- `2`: a required tool or remote check was unavailable.

Never reinterpret exit `2` as a healthy line.

The script already determines:

- maintenance source and patch-equivalent commits beyond the latest cut;
- specific release branches, exact public NuGet provenance, tags, and GitHub
  Releases;
- Finish, publication, superseded-branch, and next-cut actions;
- upstream `chrome/mN` commits using the Skia sync detector's comparison
  precedence;
- active milestone sync branches and pull requests; and
- the immediately following milestone while `main` still carries the previous
  line.

## 3. Check internal package-build health

Verify internal package builds for every branch that could drive the next
action:

- the latest specific branch when its audit state is
  `publish release branch`;
- the maintenance branch when the audit would otherwise recommend
  `start`; and
- a maintenance branch waiting on upstream sync, reported as current evidence
  only because the sync will create a new tip that must be checked again.

Do not check finished or superseded specific branches. A package build on a
pending `skia-sync/mN` branch is informational until that milestone PR merges;
the eventual `main` tip still needs its own exact-tip build.

The authoritative pipeline is:

| Field | Value |
|---|---|
| Organization | `https://dev.azure.com/dnceng` |
| Project | `internal` |
| Definition | `1642` |
| Name | `skiasharp-package` |

Prefer the internal `ado-dnceng` build tool when available. Otherwise use the
authenticated Azure DevOps CLI:

```bash
az pipelines build list \
  --organization https://dev.azure.com/dnceng \
  --project internal \
  --definition-ids 1642 \
  --branch refs/heads/release/4.152.1 \
  --top 20 \
  -o json
```

Filter the returned builds to `sourceVersion == <exact branch tip from the
audit>`, then select the newest build by queue time or build ID. Never accept a
green build from an older commit.

Classify the exact-tip build:

| Evidence | Build state | Next action |
|---|---|---|
| No exact-tip build | Not built | Queue or investigate `skiasharp-package` |
| `status != completed` | Running | Wait for this exact build |
| Latest exact-tip result failed/canceled | Blocked | Fix or rerun the package build |
| Succeeded, no `BAR ID - N` tag | Incomplete | Investigate BAR registration |
| Succeeded with `BAR ID - N` | Green | Ready for release-testing/team publication |

Report the build ID, build number, exact commit, completion time, BAR ID, and:

```text
https://dev.azure.com/dnceng/internal/_build/results?buildId=<BUILD_ID>
```

If the internal connection or CLI is unavailable, say `Build check unavailable`;
do not turn an empty or failed query into `Not built` or `Green`.

## 4. Derive ordered release actions

Apply these dependencies per line:

1. **Finish public shipment** — NuGet exists but the exact tag or GitHub
   Release is incomplete. Use the `release-publish` skill before dispatching
   Release - Finish, and obtain confirmation for `push=true`.
2. **Resume publication** — a latest specific release branch has no public
   NuGet. If its exact-tip package build is green with a BAR ID, report it as
   ready for release-testing or team publication. A failed, missing, running,
   or BAR-less build blocks publication.
3. **Synchronize upstream** — newer `chrome/mN` commits or an incomplete sync
   topology block only the following release cut. They do not block finishing
   or publishing an already-cut branch. Any build for the old maintenance tip
   becomes historical once the sync lands; rerun the build check at the new
   tip.
4. **Complete milestone transition** — an open `skia-sync/mN -> main` PR for
   the immediately following line must complete before that line becomes
   active.
5. **Start release** — recommend Release - Prepare only when public/previous
   branch prerequisites are satisfied, upstream sync state is current, and the
   exact maintenance-tip package build succeeded with a BAR ID.

When one line has an already-cut branch plus newer maintenance/upstream work,
make the sequence explicit: publish the existing branch, land the upstream
sync, then rerun the audit before cutting the following release.

## Report

Lead with a narrow cross-line table:

| Line | Current stage | Package build / BAR | Upstream / sync | Next action |
|---|---|---|---|---|

Then provide:

### Finish now

List exact public package versions requiring Finish, or `None`.

### Resume publication

List exact prepared branches with branch-tip SHA, internal build result, BAR
ID, and whether release-testing/publication can proceed.

### Merge or synchronize before branching

List newer upstream counts, sync PRs, and copyable read-only/view or workflow
commands. Distinguish work for the existing branch from work queued for the
following release.

### Start next releases

List only lines whose dependencies are currently satisfied, with the exact
Prepare dry-run command.

### Blocked or unavailable checks

List every failed credential, service, ambiguous build, mismatched commit, or
inconsistent cross-repository state. Absence from this section means the check
completed, not that every release action is finished.

## Mutating handoffs

- **Start:** invoke `release-branch`; show the Prepare dry run before any push.
- **Test a BAR:** invoke `release-testing` with the exact package version and
  BAR ID.
- **Publish packages:** the protected team BAR-to-NuGet process is outside this
  skill; never simulate it.
- **Finish:** invoke `release-publish`; show the dry run and obtain confirmation
  before `push=true`.
- **Sync Skia:** use the `update-skia` workflow contract and obtain confirmation
  before dispatching the sync workflow.

After any mutation completes, rerun the affected line rather than assuming the
next stage succeeded.
