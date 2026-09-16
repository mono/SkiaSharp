---
name: release-audit
description: Summarize the current state of one SkiaSharp release line without mutations.
---

# Release-line audit

Use the fast, read-only release-line summary:

```powershell
pwsh ./scripts/infra/publishing/audit-release-state.ps1 -Version 4.152
```

`-Version` accepts exactly one `A.B` line; `-Json` is optional. The report
shows maintenance, specific release branches, every public exact shipment, and
the expected incoming milestone sync pull request. Servicing lines use
`skia-sync/release-A.B.x`; the current line on `main` uses the milestone branch
recorded in `scripts/VERSIONS.txt`, such as `skia-sync/m154`. The separate
`skia-sync/main` branch follows the bleeding-edge Google Skia `main` tip and is
not a release-readiness signal. The report shows only the next merge,
preparation, publication, or finish action. It deliberately does not replace
detailed Prepare/Finish dry runs, BAR validation, release notes, or milestone
workflows.
