# Milestone & release-cadence tooling

Helper scripts that keep SkiaSharp's GitHub **milestones**, **release schedule**,
and the **issue-template version dropdowns** in sync with what actually ships.

All three read the shared source of truth in
[`scripts/VERSIONS.txt`](../../VERSIONS.txt) (the `SkiaSharp nuget X.Y.Z` and
`libSkiaSharp milestone N` lines) and talk to GitHub through the
[`gh`](https://cli.github.com/) CLI, which must be authenticated.

| Script | Language | What it manages | Automated? |
|--------|----------|-----------------|------------|
| [`sync-chrome-milestones.ps1`](sync-chrome-milestones.ps1) | PowerShell | Creates/updates the *upcoming* GitHub milestones from the Chromium release schedule. | No — run manually. |
| [`audit-milestones.ps1`](audit-milestones.ps1) | PowerShell | Fixes the milestone assigned to already-shipped PRs and their linked issues. | No — run manually. |
| [`update-bug-template.py`](update-bug-template.py) | Python | Regenerates the version dropdowns in the bug-report issue template. | **Yes** — daily workflow. |

---

## `sync-chrome-milestones.ps1`

Fetches the Chromium release schedule from `chromiumdash.appspot.com` and
creates/updates GitHub milestones for the next few Skia milestones, with due
dates derived from the Chrome cadence:

| Chrome phase | SkiaSharp milestone | Work starts on |
|--------------|---------------------|----------------|
| Beta | `Preview.1` | Branch day |
| Early Stable | `Preview.2` | Early Stable Cut day |
| Stable Cut | `RC` | Early Stable day |
| Stable | `Stable` | Stable Cut day |

```bash
# preview the next 3 milestones without touching anything
pwsh scripts/infra/milestones/sync-chrome-milestones.ps1 -DryRun -Count 3

# create/update them
pwsh scripts/infra/milestones/sync-chrome-milestones.ps1
```

Flags: `-DryRun`, `-Count <n>` (default 5), `-Repo <owner/repo>` (default `mono/SkiaSharp`).

## `audit-milestones.ps1`

Determines what shipped in each release by comparing the merge-bases of
consecutive release branches on `main`, then ensures every PR in that range —
and any issues it closed — is assigned to the correct milestone.

```bash
# report what would change
pwsh scripts/infra/milestones/audit-milestones.ps1 -DryRun

# audit a specific version instead of the one in VERSIONS.txt
pwsh scripts/infra/milestones/audit-milestones.ps1 -Version 4.150.0
```

Flags: `-DryRun`, `-Version <x.y.z>` (defaults to VERSIONS.txt), `-Repo <owner/repo>`.

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
workflow runs `update-bug-template.py` daily (09:00 UTC) and
opens/updates a PR when the dropdowns drift. It can also be triggered manually
via **workflow_dispatch**. The milestone scripts above have no such workflow —
run them by hand when preparing releases.
