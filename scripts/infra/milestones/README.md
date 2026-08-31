# Milestone and release-cadence tooling

Repository milestone maintenance is implemented by
[`update-release-milestones.ps1`](../publishing/update-release-milestones.ps1).
It reads `scripts/VERSIONS.txt`, Git release history, GitHub milestones, and the
public Chromium schedule.

The script performs two ordered operations:

1. reconcile merged pull requests and linked issues to the release where they
   shipped; and
2. maintain upcoming dates, move remaining open work, and close shipped
   milestones.

```powershell
# Read-only
./scripts/infra/publishing/update-release-milestones.ps1 `
  -Version 4.153.0

# Push milestone changes
./scripts/infra/publishing/update-release-milestones.ps1 `
  -Version 4.153.0 `
  -Push
```

The **Release - Milestones** workflow provides the same dry-run followed by an
environment-protected apply. It is manually dispatched for now and may later be
triggered by release publication.

The independent
[`update-bug-template.py`](update-bug-template.py) script maintains issue-form
version dropdowns through **Sync - Issue Template Versions**.
