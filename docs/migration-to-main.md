# Migration Plan: Dashboard → Main Branch

> **Status:** Waiting for .NET 10 PR to land on main
> **Prerequisites:** [.NET 10 SDK upgrade PR](https://github.com/mono/SkiaSharp/pulls) must be merged first

## Why

The dashboard, collector, and triage models live on an orphan `docs-dashboard` branch, separate from the main SkiaSharp codebase. This prevents:

1. **Atomic cross-cutting changes** — when updating the triage skill schema (`.github/skills/triage-issue/` on `main`), the collector and dashboard (on `docs-dashboard`) must be updated separately. AI agents cannot make a single PR that touches schema + models + collector + dashboard.

2. **AI agent context** — agents working on the triage skill cannot see how the collector/dashboard consume the schema. Everything on one branch gives full visibility.

## Project Renames

| Current | New | Namespace |
|---------|-----|-----------|
| `src/Dashboard/` | `utils/SkiaSharp.Triage.Dashboard/` | `SkiaSharp.Triage.Dashboard` |
| `src/SkiaSharp.Collector/` | `utils/SkiaSharp.Triage.Cli/` | `SkiaSharp.Triage.Cli` |
| `src/SkiaSharp.Triage.Models/` | `utils/SkiaSharp.Triage.Models/` | *(unchanged)* |
| `SkiaSharp.slnx` | `utils/SkiaSharp.Triage.slnx` | — |

## Target Structure on Main

```
mono/SkiaSharp (main branch)
├── .github/
│   ├── copilot-instructions.md        # Merged: SkiaSharp core + Dashboard
│   ├── skills/triage-issue/           # Schema source of truth
│   └── workflows/
│       ├── build-dashboard.yml        # Moved from docs-dashboard
│       ├── sync-data-cache.yml        # Moved from docs-dashboard
│       └── ...existing workflows...
├── documentation/
│   ├── ...existing docs...
│   └── dashboard.md                   # NEW: architecture + dev setup reference
├── utils/
│   ├── Utils.sln                      # Existing (generator + wasm runner)
│   ├── SkiaSharp.Triage.slnx          # NEW (triage projects)
│   ├── README.md                      # Updated with triage section
│   ├── SkiaSharpGenerator/            # Existing
│   ├── WasmTestRunner/                # Existing
│   ├── SkiaSharp.Triage.Dashboard/    # NEW (Blazor WASM app)
│   ├── SkiaSharp.Triage.Cli/          # NEW (data sync CLI)
│   └── SkiaSharp.Triage.Models/       # NEW (shared triage models)
├── global.json                        # .NET 10 (covers everything)
└── ...existing structure...
```

## What Moves

### Copy to main

- **3 project directories** → `utils/` (renamed as above)
- **2 workflow files** → `.github/workflows/` (update triggers + paths)

### Merge into existing main files

- `.github/copilot-instructions.md` — append dashboard/triage section
- `.gitignore` — add dashboard-specific entries

### Create new

- `documentation/dashboard.md` — architecture + dev setup reference (from `.ai/architecture.md` + `.ai/techContext.md`)
- `utils/SkiaSharp.Triage.slnx` — solution file for the 3 triage projects

### Drop (not migrated)

- `.ai/` — memory bank, stale; useful parts go into `documentation/dashboard.md`
- `.editorconfig` — main already has one
- `global.json` — main's root `global.json` (.NET 10) covers everything
- `SkiaSharp.slnx` — replaced by `SkiaSharp.Triage.slnx`
- `README.md` — content folded into `utils/README.md`
- `docs/` — old and out of date
- `github/`, `nuget/` — empty placeholder dirs

## Namespace Changes

### Dashboard → `SkiaSharp.Triage.Dashboard`

- `RootNamespace` in csproj
- `_Imports.razor` — 4 `@using` lines
- `NavMenu.razor` + `MainLayout.razor` — `@namespace` directive
- 8 `Services/*.cs` files
- `Program.cs`

### Collector → `SkiaSharp.Triage.Cli`

- Add `RootNamespace` to csproj
- ~20 `.cs` files across `Commands/`, `Services/`, `Models/`

### Triage.Models — no changes

## CI/CD Changes

### Workflow triggers

Current (on `docs-dashboard`):
```yaml
on:
  push:
    branches: [docs-dashboard]
  schedule:
    - cron: '...'
```

New (on `main`):
```yaml
on:
  push:
    branches: [main]
    paths:
      - 'utils/SkiaSharp.Triage.*/**'
      - '.github/workflows/build-dashboard.yml'
  schedule:
    - cron: '...'
```

> Note: `schedule` triggers don't support `paths:` filters — this is fine, sync/build workflows are idempotent.

### Workflow path updates

All `dotnet run --project` commands change from:
```bash
dotnet run --project src/SkiaSharp.Collector -- ...
```
To:
```bash
dotnet run --project utils/SkiaSharp.Triage.Cli -- ...
```

### Unaffected

- `docs-data-cache` branch — stays as-is (data cache)
- `docs-live` branch — stays as-is (deployment target)
- Azure Pipelines (Cake) — doesn't reference `utils/SkiaSharp.Triage.*`

## Implementation Steps

1. Create feature branch `dev/dashboard-migration` on main
2. Copy and rename project directories to `utils/`
3. Rename csproj files and update all namespaces
4. Create `utils/SkiaSharp.Triage.slnx`
5. Create `documentation/dashboard.md`
6. Update main's `.gitignore`
7. Merge copilot instructions
8. Copy and update workflow files
9. Update `utils/README.md`
10. Verify build: `dotnet build utils/SkiaSharp.Triage.slnx`
11. Verify main build is unaffected

## Risks

| Risk | Mitigation |
|------|-----------|
| Workflow triggers too broad | `paths:` filters on push; schedule is idempotent |
| Main branch PR noise | CODEOWNERS routes `utils/SkiaSharp.Triage.*` separately |
| Existing pipelines break | Azure Pipelines config is in `scripts/`, doesn't reference triage projects |

## After Migration

- Keep `docs-dashboard` branch alive until migration is verified in production
- Archive `docs-dashboard` branch once confirmed working
- Update any documentation referencing the old branch structure
