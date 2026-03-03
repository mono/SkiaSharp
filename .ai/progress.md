# Progress & Backlog

> Project status, completed work, pending tasks, and future ideas.

## Current Status

**Overall**: ✅ Schema v1.0 dashboard rewrite complete — fix step removed

| Area | Status | Notes |
|------|--------|-------|
| Dashboard App | ✅ Complete | Blazor WASM at `src/SkiaSharp.Triage.Dashboard/` |
| Triage CLI | ✅ Complete | .NET CLI at `src/SkiaSharp.Triage.Cli/` |
| Triage Models | ✅ Complete | Shared library at `src/SkiaSharp.Triage.Models/` |
| Schema v1.0 | ✅ Complete | Triage + Repro — Fix step removed |
| Detail Page Design | ✅ Complete | 2-tab layout: Details + Repro Log |
| Multi-Repo | ✅ Complete | SkiaSharp + Extended, parallel sync |
| Deployment | ✅ Working | https://mono.github.io/SkiaSharp/dashboard/ |

---

## Schema v2.1 Migration ✅

### Shared Library ([`src/SkiaSharp.Triage.Models/`](../src/SkiaSharp.Triage.Models/))

- [x] [`TriageEnums.cs`](../src/SkiaSharp.Triage.Models/TriageEnums.cs) — 14 C# enums with `[JsonStringEnumMemberName]`
- [x] [`TriageModels.cs`](../src/SkiaSharp.Triage.Models/TriageModels.cs) — record types (`TriagedIssue` → all children)
- [x] [`TriagePayloads.cs`](../src/SkiaSharp.Triage.Models/TriagePayloads.cs) — `TriageAction` + 8 payload types
- [x] [`TriageJsonOptions.cs`](../src/SkiaSharp.Triage.Models/TriageJsonOptions.cs) — centralized serializer config
- [x] [`TriageEnumExtensions.cs`](../src/SkiaSharp.Triage.Models/TriageEnumExtensions.cs) — `.ToJsonString()` extension

### Collector ([`src/SkiaSharp.Collector/`](../src/SkiaSharp.Collector/))

- [x] [`GenerateCommand.cs`](../src/SkiaSharp.Collector/Commands/GenerateCommand.cs) — typed `Deserialize<TriagedIssue>()`, replaces `JsonNode` DOM
- [x] Accepts both v2.0 and v2.1 schema versions
- [x] `with` expressions for immutable record mutation

### Dashboard ([`src/Dashboard/`](../src/Dashboard/))

- [x] [`TriageStats.cs`](../src/Dashboard/Services/TriageStats.cs) — gutted, keeps only `TriageData` wrapper
- [x] [`DashboardDataService.cs`](../src/Dashboard/Services/DashboardDataService.cs) — uses `TriageJsonOptions.Default`
- [x] [`Triage.razor`](../src/Dashboard/Pages/Triage.razor) — enum-based filters and comparisons
- [x] [`TriageDetail.razor`](../src/Dashboard/Pages/TriageDetail.razor) — Code Investigation sidebar, enhanced proposals (category, code snippet, validated), new action types

### Verification

- [x] Build: 0 errors, 0 warnings
- [x] End-to-end: all 41 triage files (30 v2.0 + 11 v2.1) generate successfully
- [x] Visual: triage list + detail pages verified in browser

---

## AI Triage Skill ✅

Core pipeline on `main` branch at `.github/skills/triage-issue/`:

- [x] JSON Schema (`triage-schema.json`) — draft 2020-12 with cross-field rules
- [x] `issue-to-markdown.ps1` — annotated markdown from cached JSON
- [x] `validate-triage.ps1` — `Test-Json` with full error collection
- [x] `get-labels.ps1` — `-Json` mode for machine-readable output
- [x] `SKILL.md` — hardened for reliability (env checks, error handling, push retry)
- [x] 5-model review applied (GPT-5, Opus 4.5, Gemini 3, GPT-5.1, Sonnet 4)
- [ ] Test with real AI (invoke skill on real issue via Copilot CLI)

---

## Multi-Repo Extension ✅ (v0.11–0.12)

- [x] [`config.json`](../src/SkiaSharp.Collector/config.json) — repo list + NuGet discovery settings
- [x] [`ConfigService.cs`](../src/SkiaSharp.Collector/Services/ConfigService.cs) / [`ConfigModels.cs`](../src/SkiaSharp.Collector/Models/ConfigModels.cs)
- [x] [`CacheService.cs`](../src/SkiaSharp.Collector/Services/CacheService.cs) — per-repo paths `repos/{key}/`
- [x] [`sync-data-cache.yml`](../github/workflows/sync-data-cache.yml) — parallel jobs with rebase-retry
- [x] [`GenerateCommand.cs`](../src/SkiaSharp.Collector/Commands/GenerateCommand.cs) — discovers repos, merges contributors, per-repo breakdowns
- [x] [`RepoBadge.razor`](../src/Dashboard/Components/RepoBadge.razor) — repo filter + badge component

---

## Earlier Phases ✅

<details>
<summary>Phase 1–3: Foundation, Restructure, CLI Migration</summary>

### Phase 1: Foundation ✅

- [x] Orphan `docs-dashboard` branch
- [x] Blazor WASM project (.NET 10, C# 14)
- [x] 5 pages: Home, Issues, Pull Requests, Community, NuGet

### Phase 2: Dashboard Restructure ✅

| Page | Route | File |
|------|-------|------|
| Home (Insights) | `/` | [`Home.razor`](../src/Dashboard/Pages/Home.razor) |
| Issues | `/issues` | [`Issues.razor`](../src/Dashboard/Pages/Issues.razor) |
| AI Triage | `/triage` | [`Triage.razor`](../src/Dashboard/Pages/Triage.razor) |
| Triage Detail | `/triage/{id}` | [`TriageDetail.razor`](../src/Dashboard/Pages/TriageDetail.razor) |
| Pull Requests | `/pull-requests` | [`PrTriage.razor`](../src/Dashboard/Pages/PrTriage.razor) |
| Community | `/community` | [`Community.razor`](../src/Dashboard/Pages/Community.razor) |
| NuGet | `/nuget` | [`NuGet.razor`](../src/Dashboard/Pages/NuGet.razor) |

### Phase 2.7: NuGet Page Redesign ✅

- [x] Dynamic package list from VERSIONS.txt (50 packages)
- [x] Collapsible group structure with subgroups
- [x] Legacy toggle, group/subgroup subtotals

### Phase 3: Collector CLI Migration ✅

| Component | Path |
|-----------|------|
| GitHub sync | [`SyncGitHubCommand.cs`](../src/SkiaSharp.Collector/Commands/SyncGitHubCommand.cs) |
| NuGet sync | [`SyncNuGetCommand.cs`](../src/SkiaSharp.Collector/Commands/SyncNuGetCommand.cs) |
| Community sync | [`SyncCommunityCommand.cs`](../src/SkiaSharp.Collector/Commands/SyncCommunityCommand.cs) |
| Generate | [`GenerateCommand.cs`](../src/SkiaSharp.Collector/Commands/GenerateCommand.cs) |
| GitHub API | [`GitHubService.cs`](../src/SkiaSharp.Collector/Services/GitHubService.cs) |
| NuGet API | [`NuGetService.cs`](../src/SkiaSharp.Collector/Services/NuGetService.cs) |
| Cache mgmt | [`CacheService.cs`](../src/SkiaSharp.Collector/Services/CacheService.cs) |

</details>

---

## Known Issues

None! 🎉

---

## SPA Routing (Solved)

1. Use relative URLs in `NavigateTo()` — no leading slash
2. Don't call `NavigateTo()` in `OnInitializedAsync()` without a guard
3. `404.html` uses spa-github-pages redirect with `segmentCount = 2`

See [`.github/copilot-instructions.md`](../.github/copilot-instructions.md) for full documentation.

---

## Future Ideas

- Stacked area charts for trends (per-repo breakdown)
- Per-repo breakdown cards on Home page
- Milestone tracking
- Release progress view
- Historical engagement data for trend analysis

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 0.13.0 | 2026-02-11 | Schema v2.1: shared library, 14 enums, typed deserialization, new UI features |
| 0.12.0 | 2026-02-10 | AI Triage detail page, compact list, triage links from Issues |
| 0.11.0 | 2026-02-05 | Multi-repository extension: per-repo cache, parallel sync, merged stats |
| 0.10.0 | 2026-02-05 | Issue/PR trend charts: stats cards, monthly activity, time range dropdown |
| 0.9.1 | 2026-02-04 | Refactor to NuGet.Protocol SDK |
| 0.9.0 | 2026-02-04 | NuGet charts, sorting filters, engagement loop fix |
| 0.8.0 | 2026-02-04 | Community sync: contributors, MS membership |
| 0.7.x | 2026-02-04 | Checkpoint engagement sync, workflow restructure |
| 0.6.x | 2026-02-04 | Data cache architecture with engagement scoring |
| 0.5.0 | 2026-02-04 | .NET collector CLI replaces PowerShell scripts |
| 0.4.0 | 2026-02-04 | NuGet page redesign — grouped layout, 50 packages |
| 0.3.x | 2026-02-03 | SPA routing fix, charts, filters |
| 0.2.0 | 2026-02-03 | MS/Community split, unified workflow |
| 0.1.0 | 2026-02-03 | Initial implementation |
