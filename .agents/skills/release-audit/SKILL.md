---
name: release-audit
description: Read-only SkiaSharp release-state audit for release branches, shipments, and completion state.
---

# Release-state audit

Use the read-only coordinator to inspect one or more SkiaSharp releases. It
uses real release branches, tags, and public NuGet package versions above the
configured history floor; it never mutates release state.

```powershell
./scripts/infra/publishing/audit-release-state.ps1 -Discover
./scripts/infra/publishing/audit-release-state.ps1 -Version '4.150.*'
./scripts/infra/publishing/audit-release-state.ps1 -Version '4.15*'
./scripts/infra/publishing/audit-release-state.ps1 -Version '4.153.*' -IncludeMilestoneAssignments
```

It delegates validation to Prepare, Finish, assignment reconciliation,
milestone maintenance, release notes, and BAR testing. Exit `0` is complete,
`1` is pending or inconsistent, and `2` is unavailable. Use `-Json` when an
automation consumer needs the combined result. Each `-Version` value is a
single literal PowerShell wildcard; it does not infer numeric release ranges.
Milestone assignment reconciliation is opt-in because it can take several
minutes for a broad historical range.
