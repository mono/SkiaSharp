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
the expected incoming Skia sync pull request. It shows only the next merge,
preparation, publication, or finish action. It deliberately does not replace
detailed Prepare/Finish dry runs, BAR validation, release notes, or milestone
workflows.
