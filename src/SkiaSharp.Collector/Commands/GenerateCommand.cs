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

        var cache = new CacheService(settings.CachePath);

        // Ensure output directory exists
        Directory.CreateDirectory(settings.OutputDir);

        // Generate GitHub stats
        await GenerateGitHubStatsAsync(cache, settings);

        // Generate issues data
        await GenerateIssuesAsync(cache, settings);

        // Generate PR triage data
        await GeneratePrTriageAsync(cache, settings);

        // Generate NuGet stats
        await GenerateNuGetStatsAsync(cache, settings);

        // Generate community stats
        await GenerateCommunityStatsAsync(cache, settings);

        if (!settings.Quiet)
            AnsiConsole.MarkupLine("[green]✓ All dashboard files generated[/]");

        return 0;
    }

    private async Task GenerateGitHubStatsAsync(CacheService cache, GenerateSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("\n[bold]Generating github-stats.json...[/]");

        var index = await cache.LoadGitHubIndexAsync();
        
        var openIssues = index.Items.Where(i => i.Type == "issue" && i.State == "open").ToList();
        var closedIssues = index.Items.Where(i => i.Type == "issue" && i.State == "closed").ToList();
        var openPRs = index.Items.Where(i => i.Type == "pr" && i.State == "open").ToList();
        var closedPRs = index.Items.Where(i => i.Type == "pr" && i.State == "closed").ToList();

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var recentOpenedIssues = openIssues.Count(i => i.CreatedAt >= thirtyDaysAgo);
        var recentClosedIssues = closedIssues.Count(i => i.UpdatedAt >= thirtyDaysAgo);
        var recentOpenedPRs = index.Items.Count(i => i.Type == "pr" && i.CreatedAt >= thirtyDaysAgo);

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
                0, // Stars not in cache yet
                0, // Forks not in cache yet
                0, // Watchers not in cache yet
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
            if (index.Items.Count == 0)
                AnsiConsole.MarkupLine($"[yellow]  ✓ {outputPath} (empty cache)[/]");
            else
                AnsiConsole.MarkupLine($"[green]  ✓ {outputPath}[/]");
        }
    }

    private async Task GenerateIssuesAsync(CacheService cache, GenerateSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("\n[bold]Generating issues.json...[/]");

        var index = await cache.LoadGitHubIndexAsync();
        var openIssues = index.Items
            .Where(i => i.Type == "issue" && i.State == "open")
            .OrderByDescending(i => i.UpdatedAt)
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

        foreach (var issue in openIssues)
        {
            var cachedItem = await cache.LoadItemAsync(issue.Number);
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
                $"https://github.com/mono/SkiaSharp/issues/{issue.Number}",
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
        }

        // Get hot issues
        var hotIssues = issueInfos
            .Where(i => i.Engagement?.IsHot == true)
            .OrderByDescending(i => i.Engagement?.CurrentScore ?? 0)
            .Take(10)
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

    private async Task GeneratePrTriageAsync(CacheService cache, GenerateSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("\n[bold]Generating pr-triage.json...[/]");

        var index = await cache.LoadGitHubIndexAsync();
        var openPRs = index.Items
            .Where(i => i.Type == "pr" && i.State == "open")
            .OrderByDescending(i => i.UpdatedAt)
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
        var summary = new int[5]; // ready, quick, review, author, close

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
                $"https://github.com/mono/SkiaSharp/pull/{pr.Number}"
            ));

            // Aggregations
            bySize[sizeCategory] = bySize.GetValueOrDefault(sizeCategory) + 1;
            byAge[ageCategory]++;

            var catIndex = category switch
            {
                "ready" => 0, "quick" => 1, "review" => 2, "author" => 3, "close" => 4, _ => 2
            };
            summary[catIndex]++;
        }

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
                    .Select(v => new VersionInfo(v.Version, v.Downloads))
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

    private async Task GenerateCommunityStatsAsync(CacheService cache, GenerateSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("\n[bold]Generating community-stats.json...[/]");

        // Community stats require contributors data which isn't in cache yet
        // For now, generate a minimal placeholder
        var output = new CommunityStats(
            DateTime.UtcNow,
            0,
            0,
            0,
            [],
            [],
            []
        );

        var outputPath = Path.Combine(settings.OutputDir, "community-stats.json");
        await OutputService.WriteJsonAsync(outputPath, output);
        
        if (!settings.Quiet)
            AnsiConsole.MarkupLine($"[green]  ✓ {outputPath}[/]");
    }
}

// Output models for generate command
public record EngagementScore(
    double CurrentScore,
    double PreviousScore,
    bool IsHot
);
