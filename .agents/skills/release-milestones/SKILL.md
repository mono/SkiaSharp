---
name: release-milestones
description: >
  Reconcile shipped SkiaSharp pull requests and linked issues, maintain
  Chromium-derived release milestone dates, roll open work forward, and close
  shipped milestones with the repository-owned PowerShell script. Use whenever
  a maintainer asks to update, repair, reconcile, advance, close, or inspect
  release milestones, milestone dates, or release assignments.
---

# Release Milestones

Use **Release - Milestones** with both operations selected and `push` disabled
for a read-only run. After reviewing that output, dispatch again with `push`
enabled, or run the two repository-owned scripts separately:

```powershell
# Reconcile shipped assignments (read-only)
./scripts/infra/publishing/reconcile-release-assignments.ps1 `
  -Version 4.153.0

# Reconcile shipped assignments
./scripts/infra/publishing/reconcile-release-assignments.ps1 `
  -Version 4.153.0 `
  -Push

# Update dates, roll work forward, and close shipped milestones (read-only)
./scripts/infra/publishing/update-release-milestones.ps1

# Apply milestone updates
./scripts/infra/publishing/update-release-milestones.ps1 `
  -Push
```

Assignment reconciliation maps merged pull requests and linked issues to the
release where they shipped. Milestone updates maintain upcoming
Chromium-derived dates, move remaining open work to the next unshipped
milestone, and close shipped milestones.

Always run and present the selected read-only operation(s) before requesting
confirmation for `-Push`. Warnings block remote reconciliation or closure.
These scripts never create release branches, tags, packages, or GitHub
Releases.
