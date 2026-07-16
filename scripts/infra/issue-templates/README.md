# Issue-template automation

## `update-bug-template.py`

Regenerates the two SkiaSharp version dropdowns in
[`.github/ISSUE_TEMPLATE/bug-report.yml`](../../../.github/ISSUE_TEMPLATE/bug-report.yml):

| Dropdown | Contents |
|----------|----------|
| **Version of SkiaSharp** (`version`) | A `Nightly / CI build` option (people testing the CI feed), then concrete builds of the currently-supported major only (`Pre-release` / `Current` / `Previous` / `Deprecated`). Only the newest in-flight build gets a single `Pre-release` entry — when asking what you're on *now*, the triage answer for an older pre-release build is always "update to the latest". Every older major collapses to a single `N.x (Obsolete)` entry, because those lines are unmaintained and the triage response is simply "please update". |
| **Last Known Good Version** (`goodversion`) | **Every** in-flight pre-release build listed individually (`preview.1`, `preview.2`, `rc.1`, …), then the same supported-major stables, **plus** every stable release of the previous major listed individually (last-known-good matters for triage even on retired lines), then the remaining older majors collapsed to `N.x (Obsolete)`. |

The two dropdowns treat pre-releases differently on purpose. The in-flight release
moves `preview -> rc -> stable`, and `preview.1` / `preview.2` / `rc.1` are all
builds of the *same* upcoming version. For **Version** only the newest build is
listed (a single `Pre-release` entry), since that is the one to reproduce against.
For **Last Known Good** every build is listed: a reporter may have been fine on
`preview.1` but hit a regression in `preview.2`, and pinpointing that boundary is
exactly what last-known-good is meant to capture.

The option lists are generated from the **published GitHub Releases** (the source
of truth for what a user can actually install), and the supported major is read
from [`scripts/VERSIONS.txt`](../../VERSIONS.txt) (`SkiaSharp nuget X.Y.Z`).

### Run it manually

```bash
# preview what would change
python3 scripts/infra/issue-templates/update-bug-template.py --dry-run

# apply the change
python3 scripts/infra/issue-templates/update-bug-template.py
```

Requires the [`gh`](https://cli.github.com/) CLI (authenticated). `PyYAML`, if
installed, is used to validate the result.

### Automation

The [`auto-update-issue-template-versions`](../../../.github/workflows/auto-update-issue-template-versions.yml)
workflow runs this weekly (Mondays 09:00 UTC) and opens/updates a PR when the
dropdowns drift. It can also be triggered manually via **workflow_dispatch**.

## Related milestone tooling

These are **manual** helpers (no workflow runs them today) that manage the
GitHub *milestones* rather than the issue template:

- [`scripts/infra/milestones/sync-chrome-milestones.ps1`](../milestones/sync-chrome-milestones.ps1)
  — creates/updates milestones for upcoming Skia milestones from the Chromium
  release schedule.
- [`scripts/infra/milestones/audit-milestones.ps1`](../milestones/audit-milestones.ps1)
  — audits/fixes the milestone assigned to shipped PRs and issues.
