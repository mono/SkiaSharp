# Active Context

> Current focus, recent changes, immediate next steps, and working patterns.
> **Update this file frequently** — it's the "working memory" for AI assistants.

## Current Focus

| | |
|---|---|
| **Phase** | AI Triage Dashboard — Schema v1.0 rewrite complete |
| **Status** | All 3 schemas (triage, repro, fix) rewritten, pipeline stepper, fix tab added |
| **Branch** | `docs-dashboard` |

## Recent Changes

### 2026-02-12 — Schema v1.0 Rewrite (Clean Cut)

Complete rewrite for new v1.0 schemas across all 3 AI skills (triage, repro, fix):

**Models (`src/SkiaSharp.Triage.Models/`):**
| File | Changes |
|------|---------|
| `TriageModels.cs` | Flat actions (no payloads), new BugSignals fields, CodeRelevance enum, rationale replaces fieldRationales |
| `TriageEnums.cs` | Removed: VerificationStatus, AttachmentType, CommentType, CloseReason, ProposalCategory, ProposalValidation, MissingInfoKind. Added: CodeRelevance. Updated: ReproQuality (removed steps-only), ProposalEffort (trivial/small/medium/large) |
| `TriagePayloads.cs` | **DELETED** — typed payloads eliminated, flat fields on TriageAction |
| `ReproModels.cs` | Added: ReproInputs, ReproFeedback, TriageCorrection. Moved TriageFile into Inputs object |
| `ReproEnums.cs` | Added: ReproAssessment enum |
| `FixModels.cs` | **NEW** — FixResult, FixRootCause, FixChanges, FixTests, FixVerification, FixPR, FixFeedback |
| `FixEnums.cs` | **NEW** — FixStatus, RootCauseCategory, RootCauseArea, ChangeType, RiskLevel, TestResult, ReproScenarioResult, PRStatus, CorrectionSource |
| `TriageIndexModels.cs` | Removed HumanReview field |

**CLI (`src/SkiaSharp.Triage.Cli/`):**
- Deleted 6 legacy commands + Settings.cs (−827 lines)
- Updated GenerateCommand: v1.0 version gates, fix file generation, nullable area
- Added AiFixPath to CacheService

**Dashboard:**
- Actions tab: flat field rendering (labels, comment, linkedIssue)
- BugSignals: errorType/errorMessage/stackTrace display
- Analysis tab: rationale, workarounds, nextQuestions, relevance badges
- Repro tab: feedback corrections, inputs display
- **NEW Fix tab**: root cause, changes, tests, verification, PR, feedback
- **NEW Pipeline stepper**: Triage → Repro → Fix visual header
- GetFixDetailAsync added to DashboardDataService

Other changes:

- **[`GenerateCommand.cs`](../src/SkiaSharp.Collector/Commands/GenerateCommand.cs)** — replaced `JsonNode` DOM with `JsonSerializer.Deserialize<TriagedIssue>()`; accepts v2.0 + v2.1; uses `with` expressions for record mutation
- **[`TriageStats.cs`](../src/Dashboard/Services/TriageStats.cs)** — gutted: removed all triage types (moved to shared lib), keeps only `TriageData` wrapper
- **[`DashboardDataService.cs`](../src/Dashboard/Services/DashboardDataService.cs)** — uses `TriageJsonOptions.Default` for triage endpoint
- **[`TriageDetail.razor`](../src/Dashboard/Pages/TriageDetail.razor)** — Code Investigation sidebar, enhanced proposal cards (category, code snippet, validated), new action type handlers (convert-to-discussion, update-project, set-milestone), all helpers use enum types
- **[`Triage.razor`](../src/Dashboard/Pages/Triage.razor)** — enum-based comparisons and helper methods

### 2026-02-10 — Triage Detail Page + Compact List

- **[`Triage.razor`](../src/Dashboard/Pages/Triage.razor)** — compact clickable rows, URL-persisted filters, inline regression/review badges
- **[`TriageDetail.razor`](../src/Dashboard/Pages/TriageDetail.razor)** — full detail page: classification, actions, evidence, analysis sidebar, resolution proposals, draft response with Copy button
- **[`Issues.razor`](../src/Dashboard/Pages/Issues.razor)** — added 🤖 triage link for triaged issues

### 2026-02-09 — AI Triage Dashboard Page (Initial)

- **[`Triage.razor`](../src/Dashboard/Pages/Triage.razor)** — initial page with summary cards, filters, expandable issue cards
- **[`TriageStats.cs`](../src/Dashboard/Services/TriageStats.cs)** — C# records matching triage-schema.json
- **[`GenerateCommand.cs`](../src/SkiaSharp.Collector/Commands/GenerateCommand.cs)** — `GenerateTriageAsync` reads ai-triage/*.json, computes summary stats
- **[`NavMenu.razor`](../src/Dashboard/Layout/NavMenu.razor)** — added "AI Triage" nav item

### 2026-02-08 — Scripts → PowerShell 7.5

Converted 3 scripts from C# to PowerShell 7.5+ (393 lines, down from 651 C#). Scripts live on `main` branch at `.github/skills/triage-issue/references/`:
- `validate-triage.ps1` — `Test-Json -Schema` (draft 2020-12)
- `get-labels.ps1` — label fetcher with file-based cache
- `issue-to-markdown.ps1` — annotated markdown from cached JSON

### 2026-02-07 — Triage Skill Review

Reviewed Copilot triage skill and documented improvements.

### 2026-02-06 — Sync Design Review

Adversarial reliability review of GitHub sync (page drift, resume markers, crash windows, concurrency).

### 2026-02-05 — Multi-Repo Extension (v0.12.0)

- **[`RepoBadge.razor`](../src/Dashboard/Components/RepoBadge.razor)** — repo pill component
- **[`config.json`](../src/SkiaSharp.Collector/config.json)** — repo list + NuGet discovery settings
- **[`ConfigService.cs`](../src/SkiaSharp.Collector/Services/ConfigService.cs)** / [`ConfigModels.cs`](../src/SkiaSharp.Collector/Models/ConfigModels.cs) — multi-repo config
- **[`CacheService.cs`](../src/SkiaSharp.Collector/Services/CacheService.cs)** — per-repo paths: `repos/{key}/github/`, `repos/{key}/nuget/`
- **`sync-data-cache.yml`** — parallel jobs with rebase-retry push

## Architecture

```
docs-dashboard branch                   docs-data-cache branch
├── src/                                └── repos/
│   ├── SkiaSharp.Triage.Models/  ←NEW      ├── mono-SkiaSharp/
│   │   ├── TriageEnums.cs                  │   ├── github/ (sync-meta, repo, index, items/*)
│   │   ├── TriageModels.cs                 │   ├── nuget/  (sync-meta, index, packages/*)
│   │   ├── TriagePayloads.cs               │   └── ai-triage/*.json
│   │   ├── TriageJsonOptions.cs            └── mono-SkiaSharp.Extended/
│   │   └── TriageEnumExtensions.cs             └── ... (same structure)
│   ├── Dashboard/
│   │   ├── Pages/ (Home, Issues, Triage, TriageDetail, PrTriage, Community, NuGet)
│   │   ├── Services/ (DashboardDataService, TriageStats, GitHubStats, ...)
│   │   └── Layout/ (MainLayout, NavMenu)
│   └── SkiaSharp.Collector/
│       ├── Commands/ (Generate, Sync*, legacy)
│       ├── Services/ (Cache, GitHub, NuGet, Config)
│       └── Models/ (CacheModels, ConfigModels, ...)
└── .github/workflows/          (on GitHub, not in orphan branch checkout)
    ├── sync-data-cache.yml
    └── build-dashboard.yml
```

## Context for Next AI Session

1. Read ALL files in `.ai/` first
2. Branch is `docs-dashboard`
3. Data cache is `docs-data-cache` branch with `repos/` structure
4. Live at https://mono.github.io/SkiaSharp/dashboard/
5. Multi-repo sync runs in parallel (two jobs)

## Remaining Enhancements (Future)

- [ ] Stacked area charts for trends (using per-repo breakdown)
- [ ] Per-repo breakdown cards on Home page

## Previous Completed Phases

| Phase | Version | Summary |
|-------|---------|---------|
| Issue/PR Trend Charts | v0.10.0 | Monthly activity charts, stats cards, time range dropdown |
| Data Cache Architecture | v0.6–0.8 | Cache branch, engagement scoring, checkpoint sync |
| Collector CLI | v0.5.0 | .NET CLI replaces PowerShell scripts |
| Dashboard Features | v0.3.0 | Charts, filters, SPA routing, NuGet grouped layout |
