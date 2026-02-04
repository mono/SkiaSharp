using SkiaSharp.Collector.Models;
using SkiaSharp.Collector.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SkiaSharp.Collector.Commands;

public class IssuesCommand : AsyncCommand<CommonSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, CommonSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine($"[bold blue]Collecting issues for {settings.Owner}/{settings.Repo}...[/]");

        using var github = new GitHubService(settings.Owner, settings.Repo, settings.Verbose);

        // Paginate through all open issues
        var allIssues = new List<Octokit.Issue>();
        var page = 1;

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Fetching open issues...", async ctx =>
            {
                while (true)
                {
                    ctx.Status($"Fetching page {page}...");
                    var issues = await github.GetOpenIssuesAsync(page);
                    
                    // Filter out PRs (GitHub API returns PRs in issues endpoint)
                    var issuesOnly = issues.Where(i => i.PullRequest == null).ToList();
                    allIssues.AddRange(issuesOnly);

                    if (issues.Count < 100) break;
                    page++;
                    await GitHubService.RateLimitDelayAsync(200);
                }
            });

        if (!settings.Quiet)
            AnsiConsole.MarkupLine($"[dim]Found {allIssues.Count} open issues[/]");

        // Process issues
        var processedIssues = new List<IssueInfo>();
        var byType = new Dictionary<string, int>();
        var byArea = new Dictionary<string, int>();
        var byBackend = new Dictionary<string, int>();
        var byOs = new Dictionary<string, int>();
        var byAge = new Dictionary<string, int>
        {
            ["fresh"] = 0,
            ["recent"] = 0,
            ["aging"] = 0,
            ["stale"] = 0,
            ["ancient"] = 0
        };

        foreach (var issue in allIssues)
        {
            var daysOpen = (DateTime.UtcNow - issue.CreatedAt.UtcDateTime).TotalDays;
            var daysSinceActivity = (DateTime.UtcNow - issue.UpdatedAt.GetValueOrDefault().UtcDateTime).TotalDays;
            var ageCategory = LabelParser.GetAgeCategory((int)daysOpen);
            var labels = issue.Labels.Select(l => l.Name).ToList();
            var parsed = LabelParser.Parse(labels);

            processedIssues.Add(new IssueInfo(
                issue.Number,
                issue.Title,
                issue.User.Login,
                issue.User.AvatarUrl,
                issue.CreatedAt.UtcDateTime,
                issue.UpdatedAt.GetValueOrDefault().UtcDateTime,
                issue.Comments,
                Math.Floor(daysOpen),
                Math.Floor(daysSinceActivity),
                ageCategory,
                parsed.Type,
                parsed.Areas,
                parsed.Backends,
                parsed.Oses,
                parsed.Other,
                issue.HtmlUrl
            ));

            // Aggregate by type
            var typeKey = parsed.Type ?? "unlabeled";
            byType[typeKey] = byType.GetValueOrDefault(typeKey) + 1;

            // Aggregate by area/backend/os
            foreach (var area in parsed.Areas)
                byArea[area] = byArea.GetValueOrDefault(area) + 1;
            foreach (var backend in parsed.Backends)
                byBackend[backend] = byBackend.GetValueOrDefault(backend) + 1;
            foreach (var os in parsed.Oses)
                byOs[os] = byOs.GetValueOrDefault(os) + 1;

            byAge[ageCategory]++;
        }

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
            processedIssues
        );

        // Write output
        var outputPath = OutputService.GetOutputPath(settings.OutputDir, "issues.json");
        await OutputService.WriteJsonAsync(outputPath, output);

        if (!settings.Quiet)
        {
            var table = new Table()
                .AddColumn("Age Category")
                .AddColumn(new TableColumn("Count").RightAligned());
            
            table.AddRow("[green]Fresh (< 7d)[/]", byAge["fresh"].ToString());
            table.AddRow("Recent (7-30d)", byAge["recent"].ToString());
            table.AddRow("[yellow]Aging (30-90d)[/]", byAge["aging"].ToString());
            table.AddRow("[orange3]Stale (90-365d)[/]", byAge["stale"].ToString());
            table.AddRow("[red]Ancient (> 1y)[/]", byAge["ancient"].ToString());
            table.AddRow("[bold]Total[/]", $"[bold]{allIssues.Count}[/]");
            
            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[green]✓ Issues data written to {outputPath}[/]");
        }

        return 0;
    }
}
