# Milestone & release-cadence tooling

Helper scripts that keep SkiaSharp's GitHub **milestones**, **release schedule**,
and the **issue-template version dropdowns** in sync with what actually ships.

The tooling reads the shared source of truth in
[`scripts/VERSIONS.txt`](../../VERSIONS.txt) (the `SkiaSharp nuget X.Y.Z` and
`libSkiaSharp milestone N` lines). C# release closeout uses Octokit; the
standalone issue-template updater uses the authenticated
[`gh`](https://cli.github.com/) CLI.

| Component | What it manages | Automated? |
|-----------|-----------------|------------|
| [`SkiaSharp.ReleaseTool`](../../../utils/SkiaSharp.ReleaseTool/) | Reconciles shipped PR/issue assignments, moves open work, and closes tagged milestones. | **Yes** — `Release - Finish` closeout |
| [`update-bug-template.py`](update-bug-template.py) | Regenerates the version dropdowns in the bug-report issue template. | **Yes** — daily and post-release workflows |

---

## Release closeout

The release CLI consumes the immutable Finish plan:

```text
dotnet run --no-build --no-restore --configuration Release \
  --project utils/SkiaSharp.ReleaseTool/SkiaSharp.ReleaseTool.csproj -- \
  finish closeout \
  --plan finish-plan.json \
  --expected-plan-id <planId-guid> \
  --dry-run
```

It determines shipped releases from immutable tags, reconciles merged PRs and
their linked issues to the release where they shipped, moves remaining open
items to the next unshipped milestone, and closes the shipped milestone. The
command is idempotent and blocks on ambiguous release boundaries.

Before rollover, it also fetches the Chromium release schedule and creates or
updates the next three SkiaSharp preview/RC/stable milestones and due dates.
Schedule lookup failures are reported as warnings and do not suppress release
notes or issue-template dispatches.

The normal `Release - Finish` workflow performs this automatically after the
GitHub Release is published. The CLI command exists for read-only diagnostics
and recovery.

## `update-bug-template.py`

Regenerates the two SkiaSharp version dropdowns in
[`.github/ISSUE_TEMPLATE/bug-report.yml`](../../../.github/ISSUE_TEMPLATE/bug-report.yml)
from the **published GitHub Releases** (the source of truth for what a user can
actually install). The supported major is read from `VERSIONS.txt`.

| Dropdown | Contents |
|----------|----------|
| **Version of SkiaSharp** (`version`) | A `Nightly / CI build` option (people testing the CI feed), then concrete builds of the currently-supported major only (`Pre-release` / `Current` / `Previous` / `Deprecated`). Only the newest in-flight build gets a single `Pre-release` entry — when asking what you're on *now*, the triage answer for an older pre-release build is always "update to the latest". Every older major collapses to a single `N.x (Obsolete)` entry, because those lines are unmaintained and the triage response is simply "please update". |
| **Last Known Good Version** (`goodversion`) | **Every** in-flight pre-release build listed individually (`preview.1`, `preview.2`, `rc.1`, …), then the same supported-major stables, then every older major collapsed to a single `N.x (Obsolete)` entry. Last-known-good matters for triage, but the exact retired build does not — "somewhere in 3.x" is a good enough answer once a line is unmaintained. |

The two dropdowns treat pre-releases differently on purpose. The in-flight
release moves `preview -> rc -> stable`, and `preview.1` / `preview.2` / `rc.1`
are all builds of the *same* upcoming version. For **Version** only the newest
build is listed (a single `Pre-release` entry), since that is the one to
reproduce against. For **Last Known Good** every build is listed: a reporter may
have been fine on `preview.1` but hit a regression in `preview.2`, and
pinpointing that boundary is exactly what last-known-good is meant to capture.

```bash
# preview what would change
python3 scripts/infra/milestones/update-bug-template.py --dry-run

# apply the change
python3 scripts/infra/milestones/update-bug-template.py
```

Flags: `--dry-run`, `--repo <owner/repo>` (default `mono/SkiaSharp`),
`--file <path>` (default the bug-report template). `PyYAML`, if installed, is
used to validate the result.

### Automation

The [`Sync - Issue Template Versions`](../../../.github/workflows/auto-update-issue-template-versions.yml)
workflow runs `update-bug-template.py` daily (09:00 UTC) and opens/updates a PR
when the dropdowns drift. It can also be triggered manually via
**workflow_dispatch**. Stable release closeout dispatches the same workflow so
the public version list can converge immediately.
