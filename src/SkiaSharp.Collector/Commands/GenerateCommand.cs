using SkiaSharp.Collector.Models;
using SkiaSharp.Collector.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SkiaSharp.Collector.Commands;

/// <summary>
/// Generate all dashboard JSON files from multi-repo cache.
/// Discovers repos from repos/*/ structure and merges data.
/// </summary>
public class GenerateCommand : AsyncCommand<GenerateSettings>
{
    private readonly ConfigService _configService = new();

    public override async Task<int> ExecuteAsync(CommandContext context, GenerateSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("[bold blue]Generating dashboard data from cache...[/]");

        var rootCache = new CacheService(settings.CachePath);
        var config = await _configService.LoadConfigAsync();

        // Discover repos from cache
        var repoKeys = rootCache.DiscoverRepoKeys();
        if (repoKeys.Count == 0)
        {
            // Fall back to legacy single-repo structure
            AnsiConsole.MarkupLine("[yellow]No repos found in cache. Falling back to legacy structure.[/]");
            repoKeys = [""];
        }

        if (!settings.Quiet)
            AnsiConsole.MarkupLine($"[dim]Found {repoKeys.Count} repo(s) in cache: {string.Join(", ", repoKeys)}[/]");

        // Ensure output directory exists
        Directory.CreateDirectory(settings.OutputDir);

        // Generate all files from multi-repo data
        await GenerateGitHubStatsAsync(rootCache, config, repoKeys, settings);
        await GenerateIssuesAsync(rootCache, config, repoKeys, settings);
        await GeneratePrTriageAsync(rootCache, config, repoKeys, settings);
        await GenerateNuGetStatsAsync(rootCache, config, repoKeys, settings);
        await GenerateNuGetChartsAsync(rootCache, config, repoKeys, settings);
        await GenerateCommunityStatsAsync(rootCache, config, repoKeys, settings);
        await GenerateTrendDataAsync(rootCache, config, repoKeys, settings);

        if (!settings.Quiet)
            AnsiConsole.MarkupLine("[green]✓ All dashboard files generated[/]");

        return 0;
    }

    private static RepoConfig? GetRepoConfig(DashboardConfig config, string repoKey)
    {
        return config.Repos.FirstOrDefault(r => r.Key == repoKey);
    }

    private async Task GenerateGitHubStatsAsync(CacheService rootCache, DashboardConfig config, List<string> repoKeys, GenerateSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("\n[bold]Generating github-stats.json...[/]");

        var repoStats = new Dictionary<string, RepoGitHubStats>();
        var totalStats = new TotalGitHubStats(0, 0, 0, 0, 0, 0, 0);

        foreach (var repoKey in repoKeys)
        {
            var cache = string.IsNullOrEmpty(repoKey) ? rootCache : rootCache.ForRepo(repoKey);
            var repoConfig = GetRepoConfig(config, repoKey);
            var index = await cache.LoadGitHubIndexAsync();
            var stats = await cache.LoadRepoStatsAsync();

            var openIssues = index.Items.Count(i => i.Type == "issue" && i.State == "open");
            var closedIssues = index.Items.Count(i => i.Type == "issue" && i.State == "closed");
            var openPRs = index.Items.Count(i => i.Type == "pr" && i.State == "open");
            var closedPRs = index.Items.Count(i => i.Type == "pr" && i.State == "closed");

            var repoData = new RepoGitHubStats(
                repoConfig?.DisplayName ?? repoKey,
                repoConfig?.Slug ?? repoKey,
                repoConfig?.Color ?? "#888888",
                stats?.Stars ?? 0,
                stats?.Forks ?? 0,
                stats?.Watchers ?? 0,
                openIssues,
                closedIssues,
                openPRs,
                closedPRs
            );

            var fullName = repoConfig?.FullName ?? repoKey;
            repoStats[fullName] = repoData;

            totalStats = totalStats with
            {
                Stars = totalStats.Stars + repoData.Stars,
                Forks = totalStats.Forks + repoData.Forks,
                Watchers = totalStats.Watchers + repoData.Watchers,
                OpenIssues = totalStats.OpenIssues + repoData.OpenIssues,
                ClosedIssues = totalStats.ClosedIssues + repoData.ClosedIssues,
                OpenPRs = totalStats.OpenPRs + repoData.OpenPRs,
                ClosedPRs = totalStats.ClosedPRs + repoData.ClosedPRs
            };
        }

        var output = new MultiRepoGitHubStats(DateTime.UtcNow, repoStats, totalStats);
        var outputPath = Path.Combine(settings.OutputDir, "github-stats.json");
        await OutputService.WriteJsonAsync(outputPath, output);

        if (!settings.Quiet)
            AnsiConsole.MarkupLine($"[green]  ✓ {outputPath}[/]");
    }

    private async Task GenerateIssuesAsync(CacheService rootCache, DashboardConfig config, List<string> repoKeys, GenerateSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("\n[bold]Generating issues.json...[/]");

        var allIssues = new List<IssueInfo>();
        var byType = new Dictionary<string, int>();
        var byArea = new Dictionary<string, int>();
        var byBackend = new Dictionary<string, int>();
        var byOs = new Dictionary<string, int>();
        var byAge = new Dictionary<string, int>
        {
            ["fresh"] = 0, ["recent"] = 0, ["aging"] = 0, ["stale"] = 0, ["ancient"] = 0
        };
        var byRepo = new Dictionary<string, int>();

        var engagementCalculator = new EngagementCalculator();

        foreach (var repoKey in repoKeys)
        {
            var cache = string.IsNullOrEmpty(repoKey) ? rootCache : rootCache.ForRepo(repoKey);
            var repoConfig = GetRepoConfig(config, repoKey);
            var index = await cache.LoadGitHubIndexAsync();
            var fullName = repoConfig?.FullName ?? repoKey;
            var slug = repoConfig?.Slug ?? repoKey;
            var color = repoConfig?.Color ?? "#888888";

            var openIssues = index.Items
                .Where(i => i.Type == "issue" && i.State == "open")
                .OrderByDescending(i => i.UpdatedAt)
                .ToList();

            byRepo[fullName] = openIssues.Count;

            foreach (var issue in openIssues)
            {
                var cachedItem = await cache.LoadItemAsync(issue.Number);
                var daysOpen = (DateTime.UtcNow - issue.CreatedAt).TotalDays;
                var daysSinceActivity = (DateTime.UtcNow - issue.UpdatedAt).TotalDays;
                var ageCategory = LabelParser.GetAgeCategory((int)daysOpen);
                var parsed = LabelParser.Parse(issue.Labels);

                IssueEngagementScore? engagement = null;
                if (cachedItem?.Engagement != null)
                {
                    var score = engagementCalculator.CalculateScore(cachedItem.Engagement, cachedItem.CreatedAt);
                    engagement = new IssueEngagementScore(score.CurrentScore, score.PreviousScore, score.IsHot);
                }

                allIssues.Add(new IssueInfo(
                    issue.Number,
                    issue.Title,
                    issue.Author,
                    $"https://avatars.githubusercontent.com/{issue.Author}",
                    issue.CreatedAt,
                    issue.UpdatedAt,
                    issue.CommentCount,
                    Math.Floor(daysOpen),
                    Math.Floor(daysSinceActivity),
                    ageCategory,
                    parsed.Type,
                    parsed.Areas,
                    parsed.Backends,
                    parsed.Oses,
                    parsed.Other,
                    $"https://github.com/{fullName}/issues/{issue.Number}",
                    engagement,
                    fullName,
                    slug,
                    color
                ));

                var typeKey = parsed.Type ?? "unlabeled";
                byType[typeKey] = byType.GetValueOrDefault(typeKey) + 1;
                foreach (var area in parsed.Areas)
                    byArea[area] = byArea.GetValueOrDefault(area) + 1;
                foreach (var backend in parsed.Backends)
                    byBackend[backend] = byBackend.GetValueOrDefault(backend) + 1;
                foreach (var os in parsed.Oses)
                    byOs[os] = byOs.GetValueOrDefault(os) + 1;
                byAge[ageCategory]++;
            }
        }

        var hotIssues = allIssues
            .Where(i => i.Engagement?.IsHot == true)
            .OrderByDescending(i => i.Engagement?.CurrentScore ?? 0)
            .Take(10)
            .ToList();

        var repos = config.Repos.Select(r => new RepoSummary(r.FullName, r.Slug, r.DisplayName, r.Color)).ToList();

        var output = new IssuesData(
            DateTime.UtcNow,
            allIssues.Count,
            byType.OrderByDescending(kv => kv.Value).Select(kv => new LabelCount(kv.Key, kv.Value)).ToList(),
            byArea.OrderByDescending(kv => kv.Value).Select(kv => new LabelCount(kv.Key, kv.Value)).ToList(),
            byBackend.OrderByDescending(kv => kv.Value).Select(kv => new LabelCount(kv.Key, kv.Value)).ToList(),
            byOs.OrderByDescending(kv => kv.Value).Select(kv => new LabelCount(kv.Key, kv.Value)).ToList(),
            [
                new AgeCount("fresh", "< 7 days", byAge["fresh"]),
                new AgeCount("recent", "7-30 days", byAge["recent"]),
                new AgeCount("aging", "30-90 days", byAge["aging"]),
                new AgeCount("stale", "90-365 days", byAge["stale"]),
                new AgeCount("ancient", "> 1 year", byAge["ancient"])
            ],
            allIssues.OrderByDescending(i => i.UpdatedAt).ToList(),
            hotIssues,
            repos,
            byRepo
        );

        var outputPath = Path.Combine(settings.OutputDir, "issues.json");
        await OutputService.WriteJsonAsync(outputPath, output);

        if (!settings.Quiet)
        {
            AnsiConsole.MarkupLine($"[green]  ✓ {outputPath}[/]");
            AnsiConsole.MarkupLine($"[dim]    {allIssues.Count} issues, {hotIssues.Count} hot[/]");
        }
    }

    private async Task GeneratePrTriageAsync(CacheService rootCache, DashboardConfig config, List<string> repoKeys, GenerateSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("\n[bold]Generating pr-triage.json...[/]");

        var triaged = new List<PullRequestInfo>();
        var bySize = new Dictionary<string, int>
        {
            ["xs"] = 0, ["s"] = 0, ["m"] = 0, ["l"] = 0, ["xl"] = 0
        };
        var byAge = new Dictionary<string, int>
        {
            ["fresh"] = 0, ["recent"] = 0, ["aging"] = 0, ["stale"] = 0, ["ancient"] = 0
        };
        var summary = new int[5];
        var byRepo = new Dictionary<string, int>();

        foreach (var repoKey in repoKeys)
        {
            var cache = string.IsNullOrEmpty(repoKey) ? rootCache : rootCache.ForRepo(repoKey);
            var repoConfig = GetRepoConfig(config, repoKey);
            var index = await cache.LoadGitHubIndexAsync();
            var fullName = repoConfig?.FullName ?? repoKey;
            var slug = repoConfig?.Slug ?? repoKey;
            var color = repoConfig?.Color ?? "#888888";

            var openPRs = index.Items
                .Where(i => i.Type == "pr" && i.State == "open")
                .ToList();

            byRepo[fullName] = openPRs.Count;

            foreach (var pr in openPRs)
            {
                var cachedItem = await cache.LoadItemAsync(pr.Number);
                var parsed = LabelParser.Parse(pr.Labels);
                var daysOpen = (DateTime.UtcNow - pr.CreatedAt).TotalDays;
                var ageCategory = LabelParser.GetAgeCategory((int)daysOpen);

                var additions = cachedItem?.Additions ?? 0;
                var deletions = cachedItem?.Deletions ?? 0;
                var filesChanged = cachedItem?.ChangedFiles ?? 0;
                var totalChanges = additions + deletions;
                var sizeCategory = LabelParser.GetSizeCategory(totalChanges);

                var category = DetermineTriageCategory(pr.Labels, cachedItem);
                var reasoning = GetTriageReasoning(category, pr.Labels, cachedItem);

                triaged.Add(new PullRequestInfo(
                    pr.Number,
                    pr.Title,
                    pr.Author,
                    $"https://avatars.githubusercontent.com/{pr.Author}",
                    "community",
                    pr.CreatedAt,
                    pr.UpdatedAt,
                    Math.Floor(daysOpen),
                    ageCategory,
                    filesChanged,
                    additions,
                    deletions,
                    totalChanges,
                    sizeCategory,
                    cachedItem?.Draft ?? false,
                    category,
                    reasoning,
                    parsed.Type,
                    parsed.Areas,
                    parsed.Backends,
                    parsed.Oses,
                    parsed.Other,
                    $"https://github.com/{fullName}/pull/{pr.Number}",
                    fullName,
                    slug,
                    color
                ));

                bySize[sizeCategory] = bySize.GetValueOrDefault(sizeCategory) + 1;
                byAge[ageCategory]++;

                var catIndex = category switch
                {
                    "ready" => 0, "quick" => 1, "review" => 2, "author" => 3, "close" => 4, _ => 2
                };
                summary[catIndex]++;
            }
        }

        var repos = config.Repos.Select(r => new RepoSummary(r.FullName, r.Slug, r.DisplayName, r.Color)).ToList();

        var output = new PrTriageData(
            DateTime.UtcNow,
            triaged.Count,
            new TriageSummary(summary[0], summary[1], summary[2], summary[3], summary[4]),
            [
                new SizeCount("xs", "XS (<10)", bySize["xs"]),
                new SizeCount("s", "S (10-50)", bySize["s"]),
                new SizeCount("m", "M (50-200)", bySize["m"]),
                new SizeCount("l", "L (200-500)", bySize["l"]),
                new SizeCount("xl", "XL (500+)", bySize["xl"])
            ],
            [
                new AgeCount("fresh", "< 7 days", byAge["fresh"]),
                new AgeCount("recent", "7-30 days", byAge["recent"]),
                new AgeCount("aging", "30-90 days", byAge["aging"]),
                new AgeCount("stale", "90-365 days", byAge["stale"]),
                new AgeCount("ancient", "> 1 year", byAge["ancient"])
            ],
            triaged.OrderByDescending(p => p.UpdatedAt).ToList(),
            repos,
            byRepo
        );

        var outputPath = Path.Combine(settings.OutputDir, "pr-triage.json");
        await OutputService.WriteJsonAsync(outputPath, output);

        if (!settings.Quiet)
            AnsiConsole.MarkupLine($"[green]  ✓ {outputPath}[/]");
    }

    private static string GetTriageReasoning(string category, List<string> labels, CachedItem? item)
    {
        return category switch
        {
            "ready" => "Has approved reviews or ready label",
            "quick" => "Small PR with few changes",
            "review" => "Needs code review",
            "author" => "Needs author response",
            "close" => "Stale or abandoned",
            "draft" => "Work in progress",
            _ => "Needs triage"
        };
    }

    private static string DetermineTriageCategory(List<string> labels, CachedItem? item)
    {
        if (item?.Draft == true) return "draft";
        if (labels.Any(l => l.Contains("needs-review", StringComparison.OrdinalIgnoreCase))) return "needs-review";
        if (labels.Any(l => l.Contains("ready", StringComparison.OrdinalIgnoreCase))) return "ready";
        if (labels.Any(l => l.Contains("wip", StringComparison.OrdinalIgnoreCase))) return "wip";
        return "untriaged";
    }

    private async Task GenerateNuGetStatsAsync(CacheService rootCache, DashboardConfig config, List<string> repoKeys, GenerateSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("\n[bold]Generating nuget-stats.json...[/]");

        var allPackages = new List<PackageInfo>();
        long totalDownloads = 0;
        var byRepo = new Dictionary<string, long>();

        foreach (var repoKey in repoKeys)
        {
            var cache = string.IsNullOrEmpty(repoKey) ? rootCache : rootCache.ForRepo(repoKey);
            var repoConfig = GetRepoConfig(config, repoKey);
            var fullName = repoConfig?.FullName ?? repoKey;
            var index = await cache.LoadNuGetIndexAsync();

            long repoDownloads = 0;

            foreach (var pkg in index.Packages)
            {
                var cachedPkg = await cache.LoadPackageAsync(pkg.Id);
                allPackages.Add(new PackageInfo(
                    pkg.Id,
                    pkg.TotalDownloads,
                    cachedPkg?.Versions
                        .Select(v => new VersionInfo(v.Version, v.Downloads, v.Published))
                        .ToList() ?? [],
                    pkg.IsLegacy,
                    fullName
                ));
                repoDownloads += pkg.TotalDownloads;
            }

            byRepo[fullName] = repoDownloads;
            totalDownloads += repoDownloads;
        }

        var repos = config.Repos.Select(r => new RepoSummary(r.FullName, r.Slug, r.DisplayName, r.Color)).ToList();
        var output = new NuGetStats(DateTime.UtcNow, totalDownloads, allPackages, repos, byRepo);

        var outputPath = Path.Combine(settings.OutputDir, "nuget-stats.json");
        await OutputService.WriteJsonAsync(outputPath, output);

        if (!settings.Quiet)
        {
            AnsiConsole.MarkupLine($"[green]  ✓ {outputPath}[/]");
            AnsiConsole.MarkupLine($"[dim]    {totalDownloads:N0} total downloads across {allPackages.Count} packages[/]");
        }
    }

    private async Task GenerateNuGetChartsAsync(CacheService rootCache, DashboardConfig config, List<string> repoKeys, GenerateSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("\n[bold]Generating nuget-charts.json...[/]");

        var chartConfigs = new List<(string Title, string[] PackageIds)>
        {
            ("SkiaSharp", ["SkiaSharp"]),
            ("HarfBuzzSharp", ["HarfBuzzSharp"]),
            (".NET MAUI Views", ["SkiaSharp.Views.Maui.Core"]),
            ("Skottie Animation", ["SkiaSharp.Skottie"]),
            ("GPU Backends", ["SkiaSharp.Direct3D.Vortice", "SkiaSharp.Vulkan.SharpVk"]),
            ("Blazor", ["SkiaSharp.Views.Blazor"]),
            ("SkiaSharp.Extended", ["SkiaSharp.Extended", "SkiaSharp.Extended.UI.Maui"])
        };

        var charts = new List<PackageChartData>();

        foreach (var (title, packageIds) in chartConfigs)
        {
            var series = new List<PackageSeriesData>();

            foreach (var packageId in packageIds)
            {
                // Search across all repos for this package
                foreach (var repoKey in repoKeys)
                {
                    var cache = string.IsNullOrEmpty(repoKey) ? rootCache : rootCache.ForRepo(repoKey);
                    var cachedPkg = await cache.LoadPackageAsync(packageId);
                    if (cachedPkg == null) continue;

                    var dataPoints = BuildCumulativeDataPoints(cachedPkg.Versions);
                    if (dataPoints.Count > 0)
                    {
                        series.Add(new PackageSeriesData(packageId, dataPoints));
                        break; // Found it, don't check other repos
                    }
                }
            }

            if (series.Count > 0)
            {
                charts.Add(new PackageChartData(title, series));
            }
        }

        var output = new NuGetChartsData(DateTime.UtcNow, charts);
        var outputPath = Path.Combine(settings.OutputDir, "nuget-charts.json");
        await OutputService.WriteJsonAsync(outputPath, output);

        if (!settings.Quiet)
        {
            AnsiConsole.MarkupLine($"[green]  ✓ {outputPath}[/]");
            AnsiConsole.MarkupLine($"[dim]    {charts.Count} charts generated[/]");
        }
    }

    private static List<ChartDataPoint> BuildCumulativeDataPoints(List<Models.PackageVersion> versions)
    {
        var dataPoints = new List<ChartDataPoint>();
        long cumulative = 0;

        var sortedVersions = versions
            .Where(v => v.Published.HasValue)
            .OrderBy(v => v.Published!.Value)
            .ToList();

        foreach (var version in sortedVersions)
        {
            cumulative += version.Downloads;
            dataPoints.Add(new ChartDataPoint(version.Published!.Value, cumulative));
        }

        return dataPoints;
    }

    private async Task GenerateCommunityStatsAsync(CacheService rootCache, DashboardConfig config, List<string> repoKeys, GenerateSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("\n[bold]Generating community-stats.json...[/]");

        // Merge contributors from all repos, deduplicating by login
        var mergedContributors = new Dictionary<string, MergedContributor>(StringComparer.OrdinalIgnoreCase);

        foreach (var repoKey in repoKeys)
        {
            var cache = string.IsNullOrEmpty(repoKey) ? rootCache : rootCache.ForRepo(repoKey);
            var repoConfig = GetRepoConfig(config, repoKey);
            var fullName = repoConfig?.FullName ?? repoKey;
            var contributors = await cache.LoadContributorsAsync();

            foreach (var c in contributors)
            {
                if (!mergedContributors.TryGetValue(c.Login, out var existing))
                {
                    existing = new MergedContributor(c.Login, c.AvatarUrl, c.IsMicrosoft ?? false, [], 0);
                    mergedContributors[c.Login] = existing;
                }

                existing.ContributionsByRepo[fullName] = c.Contributions;
                existing.TotalContributions += c.Contributions;
            }
        }

        var sortedContributors = mergedContributors.Values
            .OrderByDescending(c => c.TotalContributions)
            .ToList();

        var msCount = sortedContributors.Count(c => c.IsMicrosoft);
        var communityCount = sortedContributors.Count - msCount;

        var topContributors = sortedContributors
            .Take(20)
            .Select(c => new ContributorInfo(
                c.Login,
                c.AvatarUrl,
                c.TotalContributions,
                c.IsMicrosoft,
                c.ContributionsByRepo
            ))
            .ToList();

        var repos = config.Repos.Select(r => new RepoSummary(r.FullName, r.Slug, r.DisplayName, r.Color)).ToList();

        var output = new CommunityStats(
            DateTime.UtcNow,
            sortedContributors.Count,
            msCount,
            communityCount,
            topContributors,
            [],
            [],
            repos
        );

        var outputPath = Path.Combine(settings.OutputDir, "community-stats.json");
        await OutputService.WriteJsonAsync(outputPath, output);

        if (!settings.Quiet)
        {
            if (sortedContributors.Count > 0)
                AnsiConsole.MarkupLine($"[green]  ✓ {outputPath} ({sortedContributors.Count} contributors, {msCount} MS)[/]");
            else
                AnsiConsole.MarkupLine($"[green]  ✓ {outputPath} (empty cache)[/]");
        }
    }

    private async Task GenerateTrendDataAsync(CacheService rootCache, DashboardConfig config, List<string> repoKeys, GenerateSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("\n[bold]Generating github-trends.json...[/]");

        // Collect all items from all repos
        var allIssues = new List<(IndexItem Item, string RepoFullName)>();
        var allPrs = new List<(IndexItem Item, string RepoFullName)>();

        foreach (var repoKey in repoKeys)
        {
            var cache = string.IsNullOrEmpty(repoKey) ? rootCache : rootCache.ForRepo(repoKey);
            var repoConfig = GetRepoConfig(config, repoKey);
            var fullName = repoConfig?.FullName ?? repoKey;
            var index = await cache.LoadGitHubIndexAsync();

            foreach (var item in index.Items)
            {
                if (item.Type == "issue")
                    allIssues.Add((item, fullName));
                else if (item.Type == "pr")
                    allPrs.Add((item, fullName));
            }
        }

        var now = DateTime.UtcNow;

        // Calculate combined issue stats
        var openIssues = allIssues.Where(i => i.Item.State == "open").ToList();
        var closedIssues = allIssues.Where(i => i.Item.State == "closed").ToList();
        var issueCloseTimes = closedIssues
            .Where(i => i.Item.ClosedAt.HasValue)
            .Select(i => (i.Item.ClosedAt!.Value - i.Item.CreatedAt).TotalDays)
            .ToList();
        var oldestOpenIssue = openIssues.OrderBy(i => i.Item.CreatedAt).FirstOrDefault();

        var issueSummary = new TrendSummary(
            TotalCreated: allIssues.Count,
            TotalClosed: closedIssues.Count,
            CurrentlyOpen: openIssues.Count,
            ClosureRate: allIssues.Count > 0 ? Math.Round(closedIssues.Count * 100.0 / allIssues.Count, 1) : 0,
            AvgDaysToClose: issueCloseTimes.Count > 0 ? Math.Round(issueCloseTimes.Average(), 1) : null,
            OldestOpenDays: oldestOpenIssue.Item != null ? (int)(now - oldestOpenIssue.Item.CreatedAt).TotalDays : null,
            TotalMerged: null,
            MergeRate: null,
            AvgDaysToMerge: null
        );

        // Calculate combined PR stats
        var openPrs = allPrs.Where(i => i.Item.State == "open").ToList();
        var closedPrs = allPrs.Where(i => i.Item.State == "closed").ToList();
        var mergedPrs = closedPrs.Where(i => i.Item.Merged == true).ToList();
        var prMergeTimes = mergedPrs
            .Where(i => i.Item.MergedAt.HasValue)
            .Select(i => (i.Item.MergedAt!.Value - i.Item.CreatedAt).TotalDays)
            .ToList();
        var oldestOpenPr = openPrs.OrderBy(i => i.Item.CreatedAt).FirstOrDefault();

        var prSummary = new TrendSummary(
            TotalCreated: allPrs.Count,
            TotalClosed: closedPrs.Count,
            CurrentlyOpen: openPrs.Count,
            ClosureRate: allPrs.Count > 0 ? Math.Round(closedPrs.Count * 100.0 / allPrs.Count, 1) : 0,
            AvgDaysToClose: null,
            OldestOpenDays: oldestOpenPr.Item != null ? (int)(now - oldestOpenPr.Item.CreatedAt).TotalDays : null,
            TotalMerged: mergedPrs.Count,
            MergeRate: closedPrs.Count > 0 ? Math.Round(mergedPrs.Count * 100.0 / closedPrs.Count, 1) : null,
            AvgDaysToMerge: prMergeTimes.Count > 0 ? Math.Round(prMergeTimes.Average(), 1) : null
        );

        // Build monthly trends with per-repo breakdown
        var monthlyTrends = BuildMonthlyTrendsMultiRepo(allIssues, allPrs, config.Repos.Select(r => r.FullName).ToList());

        var repos = config.Repos.Select(r => new RepoSummary(r.FullName, r.Slug, r.DisplayName, r.Color)).ToList();

        var output = new TrendData(
            GeneratedAt: now,
            Issues: issueSummary,
            PullRequests: prSummary,
            MonthlyTrends: monthlyTrends,
            Repos: repos
        );

        var outputPath = Path.Combine(settings.OutputDir, "github-trends.json");
        await OutputService.WriteJsonAsync(outputPath, output);

        if (!settings.Quiet)
        {
            AnsiConsole.MarkupLine($"[green]  ✓ {outputPath}[/]");
            AnsiConsole.MarkupLine($"    Issues: {issueSummary.TotalCreated:N0} total, {issueSummary.ClosureRate}% closed");
            AnsiConsole.MarkupLine($"    PRs: {prSummary.TotalCreated:N0} total, {prSummary.MergeRate ?? 0}% merged");
            AnsiConsole.MarkupLine($"    Monthly data points: {monthlyTrends.Count}");
        }
    }

    private static List<MonthlyTrend> BuildMonthlyTrendsMultiRepo(
        List<(IndexItem Item, string RepoFullName)> issues,
        List<(IndexItem Item, string RepoFullName)> prs,
        List<string> repoFullNames)
    {
        var trends = new Dictionary<string, MonthlyTrendBuilder>();

        var allItems = issues.Concat(prs).ToList();
        if (allItems.Count == 0)
            return [];

        var minDate = allItems.Min(i => i.Item.CreatedAt);
        var maxDate = DateTime.UtcNow;

        // Initialize all months
        var current = new DateTime(minDate.Year, minDate.Month, 1);
        var end = new DateTime(maxDate.Year, maxDate.Month, 1);
        while (current <= end)
        {
            var key = current.ToString("yyyy-MM");
            trends[key] = new MonthlyTrendBuilder(key, repoFullNames);
            current = current.AddMonths(1);
        }

        // Count issues
        foreach (var (issue, repo) in issues)
        {
            var createdKey = issue.CreatedAt.ToString("yyyy-MM");
            if (trends.TryGetValue(createdKey, out var t))
                t.AddIssueCreated(repo);

            if (issue.ClosedAt.HasValue)
            {
                var closedKey = issue.ClosedAt.Value.ToString("yyyy-MM");
                if (trends.TryGetValue(closedKey, out var tc))
                    tc.AddIssueClosed(repo);
            }
        }

        // Count PRs
        foreach (var (pr, repo) in prs)
        {
            var createdKey = pr.CreatedAt.ToString("yyyy-MM");
            if (trends.TryGetValue(createdKey, out var t))
                t.AddPrCreated(repo);

            if (pr.ClosedAt.HasValue)
            {
                var closedKey = pr.ClosedAt.Value.ToString("yyyy-MM");
                if (trends.TryGetValue(closedKey, out var tc))
                    tc.AddPrClosed(repo);
            }

            if (pr.Merged == true && pr.MergedAt.HasValue)
            {
                var mergedKey = pr.MergedAt.Value.ToString("yyyy-MM");
                if (trends.TryGetValue(mergedKey, out var tm))
                    tm.AddPrMerged(repo);
            }
        }

        return trends.Values
            .OrderBy(t => t.Month)
            .Select(t => t.Build())
            .ToList();
    }
}

// Helper class for building monthly trends with per-repo breakdown
internal class MonthlyTrendBuilder(string month, List<string> repoFullNames)
{
    public string Month { get; } = month;

    public int IssuesCreated { get; private set; }
    public int IssuesClosed { get; private set; }
    public int PrsCreated { get; private set; }
    public int PrsClosed { get; private set; }
    public int PrsMerged { get; private set; }

    private readonly Dictionary<string, int> _issuesCreatedByRepo = repoFullNames.ToDictionary(r => r, _ => 0);
    private readonly Dictionary<string, int> _issuesClosedByRepo = repoFullNames.ToDictionary(r => r, _ => 0);
    private readonly Dictionary<string, int> _prsCreatedByRepo = repoFullNames.ToDictionary(r => r, _ => 0);
    private readonly Dictionary<string, int> _prsClosedByRepo = repoFullNames.ToDictionary(r => r, _ => 0);
    private readonly Dictionary<string, int> _prsMergedByRepo = repoFullNames.ToDictionary(r => r, _ => 0);

    public void AddIssueCreated(string repo) { IssuesCreated++; if (_issuesCreatedByRepo.ContainsKey(repo)) _issuesCreatedByRepo[repo]++; }
    public void AddIssueClosed(string repo) { IssuesClosed++; if (_issuesClosedByRepo.ContainsKey(repo)) _issuesClosedByRepo[repo]++; }
    public void AddPrCreated(string repo) { PrsCreated++; if (_prsCreatedByRepo.ContainsKey(repo)) _prsCreatedByRepo[repo]++; }
    public void AddPrClosed(string repo) { PrsClosed++; if (_prsClosedByRepo.ContainsKey(repo)) _prsClosedByRepo[repo]++; }
    public void AddPrMerged(string repo) { PrsMerged++; if (_prsMergedByRepo.ContainsKey(repo)) _prsMergedByRepo[repo]++; }

    public MonthlyTrend Build() => new(
        Month,
        IssuesCreated,
        IssuesClosed,
        PrsCreated,
        PrsClosed,
        PrsMerged,
        _issuesCreatedByRepo.ToDictionary(kv => kv.Key, kv => kv.Value),
        _issuesClosedByRepo.ToDictionary(kv => kv.Key, kv => kv.Value),
        _prsCreatedByRepo.ToDictionary(kv => kv.Key, kv => kv.Value),
        _prsClosedByRepo.ToDictionary(kv => kv.Key, kv => kv.Value),
        _prsMergedByRepo.ToDictionary(kv => kv.Key, kv => kv.Value)
    );
}

// Helper class for merging contributors
internal class MergedContributor(string login, string avatarUrl, bool isMicrosoft, Dictionary<string, int> contributionsByRepo, int totalContributions)
{
    public string Login { get; } = login;
    public string AvatarUrl { get; } = avatarUrl;
    public bool IsMicrosoft { get; } = isMicrosoft;
    public Dictionary<string, int> ContributionsByRepo { get; } = contributionsByRepo;
    public int TotalContributions { get; set; } = totalContributions;
}

// Output models for generate command
public record EngagementScore(
    double CurrentScore,
    double PreviousScore,
    bool IsHot
);
