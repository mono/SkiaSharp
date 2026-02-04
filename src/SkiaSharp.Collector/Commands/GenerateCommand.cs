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
        if (index.Items.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]  No GitHub data in cache[/]");
            return;
        }

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
            AnsiConsole.MarkupLine($"[green]  ✓ {outputPath}[/]");
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

        if (openIssues.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]  No issues in cache[/]");
            return;
        }

        // Calculate engagement scores for issues with engagement data
        var engagementCalculator = new EngagementCalculator();
        var issuesWithScores = new List<IssueWithEngagement>();

        foreach (var issue in openIssues)
        {
            var cachedItem = await cache.LoadItemAsync(issue.Number);
            var score = cachedItem?.Engagement != null 
                ? engagementCalculator.CalculateScore(cachedItem.Engagement, cachedItem.CreatedAt)
                : new EngagementScore(0, 0, false);

            issuesWithScores.Add(new IssueWithEngagement(
                issue.Number,
                issue.Title,
                issue.Author,
                issue.CreatedAt,
                issue.UpdatedAt,
                issue.Labels,
                issue.CommentCount,
                issue.ReactionCount,
                score
            ));
        }

        // Get hot issues (top 10 by score where isHot)
        var hotIssues = issuesWithScores
            .Where(i => i.Engagement.IsHot)
            .OrderByDescending(i => i.Engagement.CurrentScore)
            .Take(10)
            .ToList();

        var output = new IssuesOutput(
            DateTime.UtcNow,
            openIssues.Count,
            issuesWithScores,
            hotIssues
        );

        var outputPath = Path.Combine(settings.OutputDir, "issues.json");
        await OutputService.WriteJsonAsync(outputPath, output);
        
        if (!settings.Quiet)
        {
            AnsiConsole.MarkupLine($"[green]  ✓ {outputPath}[/]");
            AnsiConsole.MarkupLine($"[dim]    {openIssues.Count} issues, {hotIssues.Count} hot[/]");
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

        if (openPRs.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]  No PRs in cache[/]");
            return;
        }

        var triaged = new List<TriagedPullRequest>();
        foreach (var pr in openPRs)
        {
            var cachedItem = await cache.LoadItemAsync(pr.Number);
            var labels = pr.Labels;
            
            // Determine triage category based on labels and state
            var category = DetermineTriageCategory(labels, cachedItem);
            
            triaged.Add(new TriagedPullRequest(
                pr.Number,
                pr.Title,
                pr.Author,
                pr.CreatedAt,
                pr.UpdatedAt,
                labels,
                category,
                false, // IsMicrosoftAuthor - would need author lookup
                cachedItem?.Draft ?? false,
                pr.CommentCount
            ));
        }

        var output = new PrTriageOutput(
            DateTime.UtcNow,
            triaged.Count,
            triaged.GroupBy(p => p.Category).ToDictionary(g => g.Key, g => g.Count()),
            triaged
        );

        var outputPath = Path.Combine(settings.OutputDir, "pr-triage.json");
        await OutputService.WriteJsonAsync(outputPath, output);
        
        if (!settings.Quiet)
            AnsiConsole.MarkupLine($"[green]  ✓ {outputPath}[/]");
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
        if (index.Packages.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]  No NuGet data in cache[/]");
            return;
        }

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
            AnsiConsole.MarkupLine($"[green]  ✓ {outputPath}[/]");
            AnsiConsole.MarkupLine($"[dim]    {totalDownloads:N0} total downloads[/]");
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
public record IssuesOutput(
    DateTime UpdatedAt,
    int TotalOpen,
    List<IssueWithEngagement> Issues,
    List<IssueWithEngagement> HotIssues
);

public record IssueWithEngagement(
    int Number,
    string Title,
    string Author,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<string> Labels,
    int CommentCount,
    int ReactionCount,
    EngagementScore Engagement
);

public record EngagementScore(
    double CurrentScore,
    double PreviousScore,
    bool IsHot
);

public record PrTriageOutput(
    DateTime UpdatedAt,
    int TotalOpen,
    Dictionary<string, int> Categories,
    List<TriagedPullRequest> PullRequests
);

public record TriagedPullRequest(
    int Number,
    string Title,
    string Author,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<string> Labels,
    string Category,
    bool IsMicrosoftAuthor,
    bool IsDraft,
    int CommentCount
);
