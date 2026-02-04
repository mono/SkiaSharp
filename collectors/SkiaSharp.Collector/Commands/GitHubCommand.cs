using SkiaSharp.Collector.Models;
using SkiaSharp.Collector.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SkiaSharp.Collector.Commands;

public class GitHubCommand : AsyncCommand<CommonSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, CommonSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine($"[bold blue]Collecting GitHub stats for {settings.Owner}/{settings.Repo}...[/]");

        using var github = new GitHubService(settings.Owner, settings.Repo, settings.Verbose);

        var stats = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Fetching repository data...", async ctx =>
            {
                // Get repository info
                ctx.Status("Fetching repository info...");
                var repo = await github.GetRepositoryAsync();

                // Get closed issues count
                ctx.Status("Counting closed issues...");
                var closedIssues = await github.SearchIssuesAsync($"repo:{settings.Owner}/{settings.Repo} is:issue is:closed");
                await GitHubService.RateLimitDelayAsync();

                // Get issues opened/closed in last 30 days
                ctx.Status("Fetching 30-day issue activity...");
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
                var recentOpened = await github.SearchIssuesAsync($"repo:{settings.Owner}/{settings.Repo} is:issue created:>={thirtyDaysAgo}");
                await GitHubService.RateLimitDelayAsync();
                var recentClosed = await github.SearchIssuesAsync($"repo:{settings.Owner}/{settings.Repo} is:issue is:closed closed:>={thirtyDaysAgo}");
                await GitHubService.RateLimitDelayAsync();

                // Get PR counts
                ctx.Status("Fetching PR statistics...");
                var openPRs = await github.SearchIssuesAsync($"repo:{settings.Owner}/{settings.Repo} is:pr is:open");
                await GitHubService.RateLimitDelayAsync();
                var mergedPRs = await github.SearchIssuesAsync($"repo:{settings.Owner}/{settings.Repo} is:pr is:merged");
                await GitHubService.RateLimitDelayAsync();
                var closedPRs = await github.SearchIssuesAsync($"repo:{settings.Owner}/{settings.Repo} is:pr is:closed is:unmerged");
                await GitHubService.RateLimitDelayAsync();

                // Get PR activity in last 30 days
                ctx.Status("Fetching 30-day PR activity...");
                var recentMerged = await github.SearchIssuesAsync($"repo:{settings.Owner}/{settings.Repo} is:pr is:merged merged:>={thirtyDaysAgo}");
                await GitHubService.RateLimitDelayAsync();
                var recentOpenedPRs = await github.SearchIssuesAsync($"repo:{settings.Owner}/{settings.Repo} is:pr created:>={thirtyDaysAgo}");
                await GitHubService.RateLimitDelayAsync();

                // Get recent commits
                ctx.Status("Fetching recent commits...");
                var commits = await github.GetCommitsAsync(10);
                var recentCommits = commits.Select(c => new CommitInfo(
                    c.Sha,
                    c.Commit.Message.Split('\n')[0],
                    c.Commit.Author.Name,
                    c.Commit.Author.Date.DateTime
                )).ToList();

                // Get commits in last 30 days
                var commitsSince = await github.GetCommitsSinceAsync(DateTimeOffset.UtcNow.AddDays(-30));

                // Get top labels
                ctx.Status("Fetching label statistics...");
                var labels = await github.GetLabelsAsync();
                var labelCounts = new List<LabelCount>();
                foreach (var label in labels.Take(10))
                {
                    var labelIssues = await github.SearchIssuesAsync($"repo:{settings.Owner}/{settings.Repo} is:issue is:open label:\"{label.Name}\"");
                    if (labelIssues.TotalCount > 0)
                        labelCounts.Add(new LabelCount(label.Name, labelIssues.TotalCount));
                    await GitHubService.RateLimitDelayAsync(50);
                }

                return new GitHubStats(
                    DateTime.UtcNow,
                    new RepositoryInfo(
                        repo.StargazersCount,
                        repo.ForksCount,
                        repo.SubscribersCount,
                        repo.OpenIssuesCount,
                        closedIssues.TotalCount
                    ),
                    new IssuesInfo(
                        repo.OpenIssuesCount,
                        closedIssues.TotalCount,
                        recentOpened.TotalCount,
                        recentClosed.TotalCount,
                        labelCounts
                    ),
                    new PullRequestsInfo(
                        openPRs.TotalCount,
                        mergedPRs.TotalCount,
                        closedPRs.TotalCount,
                        recentOpenedPRs.TotalCount,
                        recentMerged.TotalCount
                    ),
                    new ActivityInfo(
                        commitsSince.Count,
                        recentCommits
                    )
                );
            });

        // Write output
        var outputPath = OutputService.GetOutputPath(settings.OutputDir, "github-stats.json");
        await OutputService.WriteJsonAsync(outputPath, stats);

        if (!settings.Quiet)
        {
            var table = new Table()
                .AddColumn("Metric")
                .AddColumn(new TableColumn("Value").RightAligned());
            
            table.AddRow("Stars", $"[yellow]{stats.Repository.Stars:N0}[/]");
            table.AddRow("Forks", stats.Repository.Forks.ToString("N0"));
            table.AddRow("Open Issues", stats.Issues.Open.ToString("N0"));
            table.AddRow("Open PRs", stats.PullRequests.Open.ToString("N0"));
            table.AddRow("Commits (30d)", stats.Activity.CommitsLast30Days.ToString("N0"));
            
            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[green]✓ GitHub stats written to {outputPath}[/]");
        }

        return 0;
    }
}
