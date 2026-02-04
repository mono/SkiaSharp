using SkiaSharp.Collector.Models;
using SkiaSharp.Collector.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SkiaSharp.Collector.Commands;

public class CommunityCommand : AsyncCommand<CommonSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, CommonSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine($"[bold blue]Collecting community stats for {settings.Owner}/{settings.Repo}...[/]");

        using var github = new GitHubService(settings.Owner, settings.Repo, settings.Verbose);

        var stats = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Fetching contributor data...", async ctx =>
            {
                // Get contributors
                ctx.Status("Fetching contributors...");
                var contributors = await github.GetContributorsAsync();

                // Check Microsoft membership for top contributors
                ctx.Status("Checking Microsoft org membership...");
                var microsoftCount = 0;
                var communityCount = 0;
                var topContributors = new List<ContributorInfo>();

                foreach (var contributor in contributors.Take(20))
                {
                    var isMicrosoft = await github.IsMicrosoftMemberAsync(contributor.Login);
                    if (isMicrosoft) microsoftCount++;
                    else communityCount++;

                    topContributors.Add(new ContributorInfo(
                        contributor.Login,
                        contributor.AvatarUrl,
                        contributor.Contributions,
                        isMicrosoft
                    ));

                    await GitHubService.RateLimitDelayAsync(100);
                }

                // Estimate remaining contributors as community
                var remaining = contributors.Count - 20;
                if (remaining > 0)
                    communityCount += remaining;

                // Get recent commits
                ctx.Status("Fetching recent commits...");
                var commits = await github.GetCommitsAsync(20);
                var recentCommits = commits.Select(c => new CommitInfo(
                    c.Sha,
                    c.Commit.Message.Split('\n')[0],
                    c.Author?.Login ?? c.Commit.Author.Name,
                    c.Commit.Author.Date.DateTime
                )).ToList();

                // Calculate contributor growth (last 6 months)
                ctx.Status("Calculating contributor growth...");
                var contributorGrowth = new List<MonthlyCount>();
                for (var i = 5; i >= 0; i--)
                {
                    var monthStart = DateTime.UtcNow.AddMonths(-i);
                    monthStart = new DateTime(monthStart.Year, monthStart.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    var monthEnd = monthStart.AddMonths(1);

                    try
                    {
                        var monthCommits = await github.GetCommitsSinceAsync(monthStart, 100);
                        var uniqueAuthors = monthCommits
                            .Where(c => c.Commit.Author.Date.DateTime < monthEnd)
                            .Select(c => c.Author?.Login ?? c.Commit.Author.Email)
                            .Distinct()
                            .Count();

                        contributorGrowth.Add(new MonthlyCount(monthStart.ToString("yyyy-MM-01"), uniqueAuthors));
                    }
                    catch
                    {
                        // Skip months we can't fetch
                    }

                    await GitHubService.RateLimitDelayAsync(200);
                }

                return new CommunityStats(
                    DateTime.UtcNow,
                    contributors.Count,
                    microsoftCount,
                    communityCount,
                    topContributors,
                    recentCommits,
                    contributorGrowth
                );
            });

        // Write output
        var outputPath = OutputService.GetOutputPath(settings.OutputDir, "community-stats.json");
        await OutputService.WriteJsonAsync(outputPath, stats);

        if (!settings.Quiet)
        {
            var table = new Table()
                .AddColumn("Metric")
                .AddColumn(new TableColumn("Value").RightAligned());
            
            table.AddRow("Total Contributors", stats.TotalContributors.ToString());
            table.AddRow("Microsoft", $"[blue]{stats.MicrosoftContributors}[/]");
            table.AddRow("Community", $"[green]{stats.CommunityContributors}[/]");
            
            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[green]✓ Community stats written to {outputPath}[/]");
        }

        return 0;
    }
}
