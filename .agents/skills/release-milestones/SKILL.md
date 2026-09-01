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

Use **Release - Milestones** or run the repository-owned script:

```powershell
# Read-only
./scripts/infra/publishing/update-release-milestones.ps1 `
  -Version 4.153.0

# Apply GitHub milestone changes
./scripts/infra/publishing/update-release-milestones.ps1 `
  -Version 4.153.0 `
  -Push
```

The script first reconciles merged pull requests and linked issues to the
release where they shipped. It then maintains upcoming Chromium-derived dates,
moves remaining open work to the next unshipped milestone, and closes shipped
milestones.

Always run and present the read-only result before requesting confirmation for
`-Push`. Warnings block remote reconciliation or closure. This workflow never
creates release branches, tags, packages, or GitHub Releases.
