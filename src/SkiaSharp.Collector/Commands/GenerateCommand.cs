using SkiaSharp.Collector.Models;
using SkiaSharp.Collector.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SkiaSharp.Collector.Commands;

/// <summary>
/// Generate all dashboard JSON files from cache.
/// </summary>
public class GenerateCommand : AsyncCommand<GenerateSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("[bold blue]Generating dashboard data from cache...[/]");

        var rootCache = new CacheService(settings.CachePath);

        // Load repos configuration
        var reposConfig = await rootCache.LoadReposConfigAsync();
        var repos = reposConfig.Repos;

        // Build combined list of all items from all repos
        var allItems = new List<(RepoDefinition Repo, IndexItem Item)>();
        var allContributors = new List<CachedContributor>();
        CacheService? primaryCache = null;
        RepoStats? primaryRepoStats = null;

        foreach (var repo in repos)
        {
            var repoCache = CacheService.ForRepo(settings.CachePath, repo);
            
            // Load GitHub index for this repo
            var index = await repoCache.LoadGitHubIndexAsync();
            foreach (var item in index.Items)
            {
                allItems.Add((repo, item));
            }

            // Collect contributors from repos with SyncCommunity = true
            if (repo.SyncCommunity)
            {
                var contributors = await repoCache.LoadContributorsAsync();
                allContributors.AddRange(contributors);
            }

            // Store primary repo cache for NuGet/chart generation
            if (repo.IsPrimary)
            {
                primaryCache = repoCache;
                primaryRepoStats = await repoCache.LoadRepoStatsAsync();
            }
        }

        // Merge contributors by login (keep highest contribution count and MS status)
        var mergedContributors = allContributors
            .GroupBy(c => c.Login)
            .Select(g => new CachedContributor(
                g.Key,
                g.First().AvatarUrl,
                g.Sum(c => c.Contributions),
                g.Any(c => c.IsMicrosoft == true),
                g.Max(c => c.MembershipCheckedAt)
            ))
            .OrderByDescending(c => c.Contributions)
            .ToList();

        // Ensure output directory exists
        Directory.CreateDirectory(settings.OutputDir);

        // Generate GitHub stats
        await GenerateGitHubStatsAsync(allItems, primaryRepoStats, settings);

        // Generate issues data
        await GenerateIssuesAsync(repos, allItems, settings);

        // Generate PR triage data
        await GeneratePrTriageAsync(repos, allItems, settings);

        // Generate NuGet stats (primary repo only)
        if (primaryCache != null)
            await GenerateNuGetStatsAsync(primaryCache, settings);

        // Generate NuGet chart data (primary repo only)
        if (primaryCache != null)
            await GenerateNuGetChartsAsync(primaryCache, settings);

        // Generate community stats
        await GenerateCommunityStatsAsync(mergedContributors, settings);

        // Generate trend data
        await GenerateTrendDataAsync(allItems, settings);

        if (!settings.Quiet)
            AnsiConsole.MarkupLine("[green]✓ All dashboard files generated[/]");

        return 0;
    }

    private async Task GenerateGitHubStatsAsync(List<(RepoDefinition Repo, IndexItem Item)> allItems, RepoStats? primaryRepoStats, GenerateSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("\n[bold]Generating github-stats.json...[/]");

        var openIssues = allItems.Where(i => i.Item.Type == "issue" && i.Item.State == "open").Select(i => i.Item).ToList();
        var closedIssues = allItems.Where(i => i.Item.Type == "issue" && i.Item.State == "closed").Select(i => i.Item).ToList();
        var openPRs = allItems.Where(i => i.Item.Type == "pr" && i.Item.State == "open").Select(i => i.Item).ToList();
        var closedPRs = allItems.Where(i => i.Item.Type == "pr" && i.Item.State == "closed").Select(i => i.Item).ToList();

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var recentOpenedIssues = openIssues.Count(i => i.CreatedAt >= thirtyDaysAgo);
        var recentClosedIssues = closedIssues.Count(i => i.UpdatedAt >= thirtyDaysAgo);
        var recentOpenedPRs = allItems.Count(i => i.Item.Type == "pr" && i.Item.CreatedAt >= thirtyDaysAgo);

        // Get top labels
        var labelCounts = openIssues
            .SelectMany(i => i.Labels)
            .GroupBy(l => l)
            .Select(g => new LabelCount(g.Key, g.Count()))
            .OrderByDescending(l => l.Count)
            .Take(10)
            .ToList();

        var stats = new GitHubStats(
            DateTime.UtcNow,
            new RepositoryInfo(
                primaryRepoStats?.Stars ?? 0,
                primaryRepoStats?.Forks ?? 0,
                primaryRepoStats?.Watchers ?? 0,
                openIssues.Count + openPRs.Count,
                closedIssues.Count + closedPRs.Count
            ),
            new IssuesInfo(
                openIssues.Count,
                closedIssues.Count,
                recentOpenedIssues,
                recentClosedIssues,
                labelCounts
            ),
            new PullRequestsInfo(
                openPRs.Count,
                closedPRs.Count(i => i.State == "closed"), // Merged PRs would need extra tracking
                0, // Closed non-merged
                recentOpenedPRs,
                0  // Recent merged
            ),
            new ActivityInfo(0, [])
        );

        var outputPath = Path.Combine(settings.OutputDir, "github-stats.json");
        await OutputService.WriteJsonAsync(outputPath, stats);
        
        if (!settings.Quiet)
        {
            if (allItems.Count == 0)
                AnsiConsole.MarkupLine($"[yellow]  ✓ {outputPath} (empty cache)[/]");
            else
                AnsiConsole.MarkupLine($"[green]  ✓ {outputPath}[/]");
        }
    }

    private async Task GenerateIssuesAsync(List<RepoDefinition> repos, List<(RepoDefinition Repo, IndexItem Item)> allItems, GenerateSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("\n[bold]Generating issues.json...[/]");

        var openIssues = allItems
            .Where(i => i.Item.Type == "issue" && i.Item.State == "open")
            .OrderByDescending(i => i.Item.UpdatedAt)
            .ToList();

        // Calculate engagement scores and build issue list
        var engagementCalculator = new EngagementCalculator();
        var issueInfos = new List<IssueInfo>();
        var byType = new Dictionary<string, int>();
        var byArea = new Dictionary<string, int>();
        var byBackend = new Dictionary<string, int>();
        var byOs = new Dictionary<string, int>();
        var byAge = new Dictionary<string, int>
        {
            ["fresh"] = 0, ["recent"] = 0, ["aging"] = 0, ["stale"] = 0, ["ancient"] = 0
        };
        var byRepo = new Dictionary<string, int>();

        foreach (var (repo, issue) in openIssues)
        {
            var repoCache = CacheService.ForRepo(settings.CachePath, repo);
            var cachedItem = await repoCache.LoadItemAsync(issue.Number);
            var daysOpen = (DateTime.UtcNow - issue.CreatedAt).TotalDays;
            var daysSinceActivity = (DateTime.UtcNow - issue.UpdatedAt).TotalDays;
            var ageCategory = LabelParser.GetAgeCategory((int)daysOpen);
            var parsed = LabelParser.Parse(issue.Labels);

            // Calculate engagement score if data available
            IssueEngagementScore? engagement = null;
            if (cachedItem?.Engagement != null)
            {
                var score = engagementCalculator.CalculateScore(cachedItem.Engagement, cachedItem.CreatedAt);
                engagement = new IssueEngagementScore(score.CurrentScore, score.PreviousScore, score.IsHot);
            }

            issueInfos.Add(new IssueInfo(
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
                $"https://github.com/{repo.FullName}/issues/{issue.Number}",
                repo.FullName,
                repo.DisplayName ?? repo.Name,
                engagement
            ));

            // Aggregations
            var typeKey = parsed.Type ?? "unlabeled";
            byType[typeKey] = byType.GetValueOrDefault(typeKey) + 1;
            foreach (var area in parsed.Areas)
                byArea[area] = byArea.GetValueOrDefault(area) + 1;
            foreach (var backend in parsed.Backends)
                byBackend[backend] = byBackend.GetValueOrDefault(backend) + 1;
            foreach (var os in parsed.Oses)
                byOs[os] = byOs.GetValueOrDefault(os) + 1;
            byAge[ageCategory]++;
            byRepo[repo.FullName] = byRepo.GetValueOrDefault(repo.FullName) + 1;
        }

        // Get hot issues
        var hotIssues = issueInfos
            .Where(i => i.Engagement?.IsHot == true)
            .OrderByDescending(i => i.Engagement?.CurrentScore ?? 0)
            .Take(10)
            .ToList();

        // Build repo counts from repos list
        var repoCounts = repos
            .Where(r => r.SyncIssues)
            .Select(r => new RepoCount(r.FullName, r.DisplayName ?? r.Name, byRepo.GetValueOrDefault(r.FullName)))
            .ToList();

        var output = new IssuesData(
            DateTime.UtcNow,
            openIssues.Count,
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
            repoCounts,
            issueInfos,
            hotIssues
        );

        var outputPath = Path.Combine(settings.OutputDir, "issues.json");
        await OutputService.WriteJsonAsync(outputPath, output);
        
        if (!settings.Quiet)
        {
            if (openIssues.Count == 0)
                AnsiConsole.MarkupLine($"[yellow]  ✓ {outputPath} (empty cache)[/]");
            else
            {
                AnsiConsole.MarkupLine($"[green]  ✓ {outputPath}[/]");
                AnsiConsole.MarkupLine($"[dim]    {openIssues.Count} issues, {hotIssues.Count} hot[/]");
            }
        }
    }

    private async Task GeneratePrTriageAsync(List<RepoDefinition> repos, List<(RepoDefinition Repo, IndexItem Item)> allItems, GenerateSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("\n[bold]Generating pr-triage.json...[/]");

        var openPRs = allItems
            .Where(i => i.Item.Type == "pr" && i.Item.State == "open")
            .OrderByDescending(i => i.Item.UpdatedAt)
            .ToList();

        var triaged = new List<PullRequestInfo>();
        var bySize = new Dictionary<string, int>
        {
            ["xs"] = 0, ["s"] = 0, ["m"] = 0, ["l"] = 0, ["xl"] = 0
        };
        var byAge = new Dictionary<string, int>
        {
            ["fresh"] = 0, ["recent"] = 0, ["aging"] = 0, ["stale"] = 0, ["ancient"] = 0
        };
        var byRepo = new Dictionary<string, int>();
        var summary = new int[5]; // ready, quick, review, author, close

        foreach (var (repo, pr) in openPRs)
        {
            var repoCache = CacheService.ForRepo(settings.CachePath, repo);
            var cachedItem = await repoCache.LoadItemAsync(pr.Number);
            var parsed = LabelParser.Parse(pr.Labels);
            var daysOpen = (DateTime.UtcNow - pr.CreatedAt).TotalDays;
            var ageCategory = LabelParser.GetAgeCategory((int)daysOpen);
            
            var additions = cachedItem?.Additions ?? 0;
            var deletions = cachedItem?.Deletions ?? 0;
            var filesChanged = cachedItem?.ChangedFiles ?? 0;
            var totalChanges = additions + deletions;
            var sizeCategory = LabelParser.GetSizeCategory(totalChanges);
            
            // Determine triage category
            var category = DetermineTriageCategory(pr.Labels, cachedItem);
            var reasoning = GetTriageReasoning(category, pr.Labels, cachedItem);

            triaged.Add(new PullRequestInfo(
                pr.Number,
                pr.Title,
                pr.Author,
                $"https://avatars.githubusercontent.com/{pr.Author}",
                "community",  // Default - would need author lookup
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
                $"https://github.com/{repo.FullName}/pull/{pr.Number}",
                repo.FullName,
                repo.DisplayName ?? repo.Name
            ));

            // Aggregations
            bySize[sizeCategory] = bySize.GetValueOrDefault(sizeCategory) + 1;
            byAge[ageCategory]++;
            byRepo[repo.FullName] = byRepo.GetValueOrDefault(repo.FullName) + 1;

            var catIndex = category switch
            {
                "ready" => 0, "quick" => 1, "review" => 2, "author" => 3, "close" => 4, _ => 2
            };
            summary[catIndex]++;
        }

        // Build repo counts from repos list
        var repoCounts = repos
            .Where(r => r.SyncIssues)
            .Select(r => new RepoCount(r.FullName, r.DisplayName ?? r.Name, byRepo.GetValueOrDefault(r.FullName)))
            .ToList();

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
            repoCounts,
            triaged
        );

        var outputPath = Path.Combine(settings.OutputDir, "pr-triage.json");
        await OutputService.WriteJsonAsync(outputPath, output);
        
        if (!settings.Quiet)
        {
            if (openPRs.Count == 0)
                AnsiConsole.MarkupLine($"[yellow]  ✓ {outputPath} (empty cache)[/]");
            else
                AnsiConsole.MarkupLine($"[green]  ✓ {outputPath}[/]");
        }
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

    private async Task GenerateNuGetStatsAsync(CacheService cache, GenerateSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("\n[bold]Generating nuget-stats.json...[/]");

        var index = await cache.LoadNuGetIndexAsync();
        
        var totalDownloads = index.Packages.Sum(p => p.TotalDownloads);
        var packages = new List<PackageInfo>();

        foreach (var pkg in index.Packages)
        {
            var cachedPkg = await cache.LoadPackageAsync(pkg.Id);
            packages.Add(new PackageInfo(
                pkg.Id,
                pkg.TotalDownloads,
                cachedPkg?.Versions
                    .Select(v => new VersionInfo(v.Version, v.Downloads, v.Published))
                    .ToList() ?? [],
                pkg.IsLegacy
            ));
        }

        var output = new NuGetStats(DateTime.UtcNow, totalDownloads, packages);

        var outputPath = Path.Combine(settings.OutputDir, "nuget-stats.json");
        await OutputService.WriteJsonAsync(outputPath, output);
        
        if (!settings.Quiet)
        {
            if (index.Packages.Count == 0)
                AnsiConsole.MarkupLine($"[yellow]  ✓ {outputPath} (empty cache)[/]");
            else
            {
                AnsiConsole.MarkupLine($"[green]  ✓ {outputPath}[/]");
                AnsiConsole.MarkupLine($"[dim]    {totalDownloads:N0} total downloads[/]");
            }
        }
    }

    private async Task GenerateNuGetChartsAsync(CacheService cache, GenerateSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("\n[bold]Generating nuget-charts.json...[/]");

        // Define chart configurations
        var chartConfigs = new List<(string Title, string[] PackageIds)>
        {
            ("SkiaSharp", ["SkiaSharp"]),
            ("HarfBuzzSharp", ["HarfBuzzSharp"]),
            (".NET MAUI Views", ["SkiaSharp.Views.Maui.Core"]),
            ("Skottie Animation", ["SkiaSharp.Skottie"]),
            ("GPU Backends", ["SkiaSharp.Direct3D.Vortice", "SkiaSharp.Vulkan.SharpVk"]),
            ("Blazor", ["SkiaSharp.Views.Blazor"])
        };

        var charts = new List<PackageChartData>();

        foreach (var (title, packageIds) in chartConfigs)
        {
            var series = new List<PackageSeriesData>();

            foreach (var packageId in packageIds)
            {
                var cachedPkg = await cache.LoadPackageAsync(packageId);
                if (cachedPkg == null) continue;

                // Build cumulative download data points from version history
                var dataPoints = BuildCumulativeDataPoints(cachedPkg.Versions);
                
                if (dataPoints.Count > 0)
                {
                    series.Add(new PackageSeriesData(packageId, dataPoints));
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

    /// <summary>
    /// Build cumulative download data points from version history.
    /// Each version adds its downloads to the running total at its publish date.
    /// </summary>
    private static List<ChartDataPoint> BuildCumulativeDataPoints(List<Models.PackageVersion> versions)
    {
        var dataPoints = new List<ChartDataPoint>();
        long cumulative = 0;

        // Sort by publish date and build cumulative totals
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

    private async Task GenerateCommunityStatsAsync(List<CachedContributor> contributors, GenerateSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("\n[bold]Generating community-stats.json...[/]");

        var msCount = contributors.Count(c => c.IsMicrosoft == true);
        var communityCount = contributors.Count - msCount; // Everyone else is community

        // Convert to output format (top 20 for display)
        var topContributors = contributors
            .Take(20)
            .Select(c => new ContributorInfo(
                c.Login,
                c.AvatarUrl,
                c.Contributions,
                c.IsMicrosoft ?? false
            ))
            .ToList();

        var output = new CommunityStats(
            DateTime.UtcNow,
            contributors.Count,
            msCount,
            communityCount,
            topContributors,
            [], // RecentCommits - not needed for now
            []  // ContributorGrowth - not needed for now
        );

        var outputPath = Path.Combine(settings.OutputDir, "community-stats.json");
        await OutputService.WriteJsonAsync(outputPath, output);
        
        if (!settings.Quiet)
        {
            if (contributors.Count > 0)
                AnsiConsole.MarkupLine($"[green]  ✓ {outputPath} ({contributors.Count} contributors, {msCount} MS)[/]");
            else
                AnsiConsole.MarkupLine($"[green]  ✓ {outputPath} (empty cache)[/]");
        }
    }

    private async Task GenerateTrendDataAsync(List<(RepoDefinition Repo, IndexItem Item)> allItems, GenerateSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("\n[bold]Generating github-trends.json...[/]");

        var now = DateTime.UtcNow;

        // Separate issues and PRs
        var issues = allItems.Where(i => i.Item.Type == "issue").Select(i => i.Item).ToList();
        var prs = allItems.Where(i => i.Item.Type == "pr").Select(i => i.Item).ToList();

        // Calculate issue stats
        var openIssues = issues.Where(i => i.State == "open").ToList();
        var closedIssues = issues.Where(i => i.State == "closed").ToList();
        var issueCloseTimes = closedIssues
            .Where(i => i.ClosedAt.HasValue)
            .Select(i => (i.ClosedAt!.Value - i.CreatedAt).TotalDays)
            .ToList();
        var oldestOpenIssue = openIssues.OrderBy(i => i.CreatedAt).FirstOrDefault();

        var issueSummary = new TrendSummary(
            TotalCreated: issues.Count,
            TotalClosed: closedIssues.Count,
            CurrentlyOpen: openIssues.Count,
            ClosureRate: issues.Count > 0 ? Math.Round(closedIssues.Count * 100.0 / issues.Count, 1) : 0,
            AvgDaysToClose: issueCloseTimes.Count > 0 ? Math.Round(issueCloseTimes.Average(), 1) : null,
            OldestOpenDays: oldestOpenIssue != null ? (int)(now - oldestOpenIssue.CreatedAt).TotalDays : null,
            TotalMerged: null,
            MergeRate: null,
            AvgDaysToMerge: null
        );

        // Calculate PR stats
        var openPrs = prs.Where(i => i.State == "open").ToList();
        var closedPrs = prs.Where(i => i.State == "closed").ToList();
        var mergedPrs = closedPrs.Where(i => i.Merged == true).ToList();
        var prMergeTimes = mergedPrs
            .Where(i => i.MergedAt.HasValue)
            .Select(i => (i.MergedAt!.Value - i.CreatedAt).TotalDays)
            .ToList();
        var oldestOpenPr = openPrs.OrderBy(i => i.CreatedAt).FirstOrDefault();

        var prSummary = new TrendSummary(
            TotalCreated: prs.Count,
            TotalClosed: closedPrs.Count,
            CurrentlyOpen: openPrs.Count,
            ClosureRate: prs.Count > 0 ? Math.Round(closedPrs.Count * 100.0 / prs.Count, 1) : 0,
            AvgDaysToClose: null, // Use merge time instead for PRs
            OldestOpenDays: oldestOpenPr != null ? (int)(now - oldestOpenPr.CreatedAt).TotalDays : null,
            TotalMerged: mergedPrs.Count,
            MergeRate: closedPrs.Count > 0 ? Math.Round(mergedPrs.Count * 100.0 / closedPrs.Count, 1) : null,
            AvgDaysToMerge: prMergeTimes.Count > 0 ? Math.Round(prMergeTimes.Average(), 1) : null
        );

        // Build monthly trends
        var monthlyTrends = BuildMonthlyTrends(issues, prs);

        var output = new TrendData(
            GeneratedAt: now,
            Issues: issueSummary,
            PullRequests: prSummary,
            MonthlyTrends: monthlyTrends
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

    private static List<MonthlyTrend> BuildMonthlyTrends(List<IndexItem> issues, List<IndexItem> prs)
    {
        var trends = new Dictionary<string, MonthlyTrend>();

        // Find date range (all time)
        var allItems = issues.Concat(prs).ToList();
        if (allItems.Count == 0)
            return [];

        var minDate = allItems.Min(i => i.CreatedAt);
        var maxDate = DateTime.UtcNow;

        // Initialize all months
        var current = new DateTime(minDate.Year, minDate.Month, 1);
        var end = new DateTime(maxDate.Year, maxDate.Month, 1);
        while (current <= end)
        {
            var key = current.ToString("yyyy-MM");
            trends[key] = new MonthlyTrend(key, 0, 0, 0, 0, 0);
            current = current.AddMonths(1);
        }

        // Count issues created/closed by month
        foreach (var issue in issues)
        {
            var createdKey = issue.CreatedAt.ToString("yyyy-MM");
            if (trends.TryGetValue(createdKey, out var t))
                trends[createdKey] = t with { IssuesCreated = t.IssuesCreated + 1 };

            if (issue.ClosedAt.HasValue)
            {
                var closedKey = issue.ClosedAt.Value.ToString("yyyy-MM");
                if (trends.TryGetValue(closedKey, out var tc))
                    trends[closedKey] = tc with { IssuesClosed = tc.IssuesClosed + 1 };
            }
        }

        // Count PRs created/closed/merged by month
        foreach (var pr in prs)
        {
            var createdKey = pr.CreatedAt.ToString("yyyy-MM");
            if (trends.TryGetValue(createdKey, out var t))
                trends[createdKey] = t with { PrsCreated = t.PrsCreated + 1 };

            if (pr.ClosedAt.HasValue)
            {
                var closedKey = pr.ClosedAt.Value.ToString("yyyy-MM");
                if (trends.TryGetValue(closedKey, out var tc))
                    trends[closedKey] = tc with { PrsClosed = tc.PrsClosed + 1 };
            }

            if (pr.Merged == true && pr.MergedAt.HasValue)
            {
                var mergedKey = pr.MergedAt.Value.ToString("yyyy-MM");
                if (trends.TryGetValue(mergedKey, out var tm))
                    trends[mergedKey] = tm with { PrsMerged = tm.PrsMerged + 1 };
            }
        }

        return [.. trends.Values.OrderBy(t => t.Month)];
    }
}

// Output models for generate command
public record EngagementScore(
    double CurrentScore,
    double PreviousScore,
    bool IsHot
);
