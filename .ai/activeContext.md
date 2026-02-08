# Active Context

> Current focus, recent changes, immediate next steps, and working patterns.
> **Update this file frequently** - it's the "working memory" for AI assistants.

## Current Focus

**Phase**: AI Triage Skill — Review fixes applied
**Status**: All 5-model review fixes applied and tested
**Branch**: docs-dashboard

## Recent Changes

### 2026-02-08 (5-Model Review Fixes)
- **Applied fixes from 5-model review** (GPT-5, Opus 4.5, Gemini 3, GPT-5.1, Sonnet 4):
  - **triage-schema.json**: Fixed backend enum (`Xps`→`XPS`, removed `Android`/`SkiaSharp`, added `Direct3D`), added `minItems:1` to all arrays, `minProperties:1` to `reproEvidenceObject`, required `hasCrash`/`hasStackTrace` in bugSignals, UTC-only `analyzedAt` pattern, tightened `repoLink` URL to GitHub-only
  - **issue-to-markdown.ps1**: Added `begin/process/end` blocks for proper pipeline input, `$input` fallback for external piping, `Parse-DateSafe` with `InvariantCulture`, safe `roleCounts` key access, tightened issue regex to exclude `#0`
  - **get-labels.ps1**: Added `-Json` switch for machine-readable output, fixed `$LASTEXITCODE` check ordering (capture raw output first)
  - **SKILL.md**: Added Step 0 env checks (pwsh, gh, branch verify, git pull), concrete `$CACHE` variable, OS-agnostic temp paths, exact sync command, `-Json` label mode, exit code table, push failure handling with retry, non-interactive re-triage policy, explicit `requiresHumanReview` field list
- **All scripts tested**: Direct path, bash pipe, PS pipeline — all produce identical output

### 2026-02-08 (Scripts → PowerShell 7.5)
- **Converted all 3 scripts** from C# (.NET 10 file-based apps) to PowerShell 7.5+
  - `validate-triage.ps1` (43 lines) — Uses built-in `Test-Json -Schema` which fully supports draft 2020-12, `$defs`, `if/then`, `oneOf`, `pattern`, cross-field rules. Collects all errors via `-ErrorVariable`.
  - `get-labels.ps1` (72 lines) — Native `ConvertFrom-Json`, `Group-Object`, `Where-Object`. File-based 10-min cache with `Get-Item .LastWriteTime`.
  - `issue-to-markdown.ps1` (278 lines) — Idiomatic PowerShell: `?.`/`??` operators, `-replace` chains, `[regex]::Matches()`, `ConvertFrom-Json` dot notation. Output verified **identical** to Python on issues #2794, #3239, #3429, #3484.
- **Removed**: `validate-triage.cs`, `get-labels.cs`, `issue-to-markdown.cs`
- **Total**: 393 lines PowerShell (down from 651 C#, 595 Python/Bash)
- **Only dependency**: `pwsh` and `gh` CLI
- **Key learnings**:
  - `Test-Json` in PowerShell 7.5 fully supports JSON Schema draft 2020-12 (no third-party library needed)
  - `-like '*[bot]'` treats brackets as character classes → use `.EndsWith('[bot]')` instead
  - `Write-Output` appends a trailing newline → use `[Console]::Write()` for exact output control
  - Non-static local functions were still needed in C# for closures; PowerShell eliminates this entirely with script-scoped variables

### 2026-02-07 (Triage Skill Review)
- Reviewed Copilot triage skill (schema, labels, scripts, validator) and documented specific actionable improvements.

### 2026-02-06 (Sync Design Review)

- Reviewed full/resume GitHub sync design and documented edge cases around page drift, resume markers, crash windows, and concurrency.
- Completed adversarial reliability review of GitHub sync (state, rate limits, and checkpoint loop failure modes).

### 2026-02-05 (UI Enhancements - v0.12.0)

**Repo Badges & Filters ✅**
- Created `RepoBadge.razor` component (rounded pill with repo name)
- Added Repository filter dropdown to Issues page
- Added Repository filter dropdown to PRs page
- Repo badges display in issue table rows and PR cards
- URL query param support: `?repo=mono/SkiaSharp.Extended`

**Extended NuGet Sync Fix ✅**
- Updated author filter to accept "Xamarin" (Xamarin Inc. is Microsoft)
- Added `supportedPackages` whitelist for legacy detection
- Now syncs all 13 Extended packages (was 4)
- Only Extended + UI.Maui marked as supported, rest are legacy

**NuGet Page Grouping ✅**
- Added "SkiaSharp.Extended" as new top-level group
- Subgroups: Core, UI Controls, Iconify
- All 13 Extended packages properly categorized

**Live Dashboard Stats**:
| Metric | Total | SkiaSharp | Extended |
|--------|-------|-----------|----------|
| Stars | 5,514 | 5,257 | 257 |
| Open Issues | 690 | 658 | 32 |
| Open PRs | 70 | 52 | 18 |
| NuGet Packages | 63 | 50 | 13 |
| Contributors | 105 | (merged) | (merged) |

**Phase 1: Cache Restructure ✅**
- Created `config.json` with repo list (SkiaSharp + Extended)
- Created `ConfigModels.cs` and `ConfigService.cs`
- Updated `CacheService` for per-repo paths: `repos/{key}/github/`, `repos/{key}/nuget/`
- Contributors moved to `github/` folder (it's GitHub API data)

**Phase 2: Multi-Repo Sync ✅**
- Updated all sync commands to accept `--repo owner/name` argument
- `SyncGitHubCommand`, `SyncCommunityCommand`: per-repo cache paths
- `NuGetService`: Added `DiscoverPackagesAsync()` with two strategies:
  - `versions-txt`: Parse VERSIONS.txt files for SkiaSharp
  - `nuget-search`: Search NuGet API for `SkiaSharp.Extended*` by Microsoft
- `SyncNuGetCommand`: Uses per-repo config and cache paths

**Phase 3: Parallel Workflow ✅**
- Split `sync-data-cache.yml` into parallel jobs:
  - `sync-skiasharp`: Syncs mono/SkiaSharp
  - `sync-extended`: Syncs mono/SkiaSharp.Extended
- Added rebase-retry push logic (different folders = no conflicts)
- Added `--repo` filter in workflow_dispatch for testing

**Phase 4: Generate Consolidation ✅**
- Updated `GenerateCommand` to discover repos from `repos/*/` structure
- Added `repo`, `repoSlug`, `repoColor` fields to issues/PRs
- Merged contributors (deduplicate by login, track per-repo contributions)
- Per-repo breakdown in all stats (byRepo dictionaries)
- MonthlyTrend includes per-repo breakdown for stacked charts

**Phase 5: Dashboard UI ✅**
- Updated all service models for multi-repo fields
- `MultiRepoGitHubStats` replaces `GitHubStats` with `Repos` + `Total`
- Home/Community pages use `Total` stats
- All models backward compatible (new fields optional)

## Architecture Summary

```
docs-dashboard/                      # SOURCE CODE BRANCH
├── src/SkiaSharp.Collector/
│   ├── config.json                  # Repo list + NuGet discovery settings
│   └── ...

docs-data-cache/                     # DATA CACHE BRANCH
└── repos/
    ├── mono-SkiaSharp/
    │   ├── github/
    │   │   ├── sync-meta.json
    │   │   ├── repo.json
    │   │   ├── contributors.json
    │   │   ├── index.json
    │   │   └── items/*.json
    │   └── nuget/
    │       ├── sync-meta.json
    │       ├── index.json
    │       └── packages/*.json
    └── mono-SkiaSharp.Extended/
        └── ... (same structure)
```

### Parallel Sync with Rebase-Retry
```yaml
jobs:
  sync-skiasharp:
    # Syncs to repos/mono-SkiaSharp/*
    # Uses rebase-retry for push conflicts
    
  sync-extended:  # Runs in PARALLEL
    # Syncs to repos/mono-SkiaSharp.Extended/*
    # Different folders = no git conflicts on rebase
```

### NuGet Discovery
| Repo | Method | Details |
|------|--------|---------|
| mono/SkiaSharp | versions-txt | Parse VERSIONS.txt from main + release/2.x |
| mono/SkiaSharp.Extended | nuget-search | Search `SkiaSharp.Extended*` by Microsoft |

## Context for Next AI Session

When resuming work:
1. Read ALL files in `.ai/` folder first
2. Branch is `docs-dashboard`
3. Data cache is `docs-data-cache` branch with `repos/` structure
4. Live at https://mono.github.io/SkiaSharp/dashboard/
5. Multi-repo sync runs in parallel (two jobs)

## Remaining Enhancements (Future)

- [ ] Repo filter dropdown on Issues/PRs pages
- [ ] Repo color badges on issue/PR table rows
- [ ] Stacked area charts for trends (using per-repo breakdown)
- [ ] Per-repo breakdown cards on Home page

## Previous Completed Phases

### Phase 5 - Issue/PR Trend Charts (v0.10.0) ✅
- Monthly activity charts with time range dropdown
- Stats cards for issues and PRs
- Merged/MergedAt fields for PR tracking

### Phase 4 - Data Cache Architecture ✅
- Separate cache branch with sync commands
- Engagement scoring with hot issue detection
- Checkpoint-based sync with rate limit handling

### Phase 3 - Collector CLI ✅
- .NET CLI replaces PowerShell scripts
- Commands: sync github/nuget/community, generate

### Phase 2 - Dashboard Features ✅
- NuGet page with grouped layout
- SPA routing fixed
- Charts with ApexCharts
