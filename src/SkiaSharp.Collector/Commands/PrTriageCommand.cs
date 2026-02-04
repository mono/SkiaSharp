using SkiaSharp.Collector.Models;
using SkiaSharp.Collector.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SkiaSharp.Collector.Commands;

public class PrTriageCommand : AsyncCommand<CommonSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, CommonSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine($"[bold blue]Collecting PR triage data for {settings.Owner}/{settings.Repo}...[/]");

        using var github = new GitHubService(settings.Owner, settings.Repo, settings.Verbose);

        // Get open PRs
        var openPRs = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Fetching open PRs...", async _ =>
            {
                return await github.GetOpenPullRequestsAsync();
            });

        if (!settings.Quiet)
            AnsiConsole.MarkupLine($"[dim]Found {openPRs.Count} open PRs[/]");

        var triagedPRs = new List<PullRequestInfo>();
        var summary = new Dictionary<string, int>
        {
            ["ReadyToMerge"] = 0,
            ["QuickReview"] = 0,
            ["NeedsReview"] = 0,
            ["NeedsAuthor"] = 0,
            ["ConsiderClosing"] = 0
        };
        var bySize = new Dictionary<string, int> { ["xs"] = 0, ["s"] = 0, ["m"] = 0, ["l"] = 0, ["xl"] = 0 };
        var byAge = new Dictionary<string, int> { ["fresh"] = 0, ["recent"] = 0, ["aging"] = 0, ["stale"] = 0, ["ancient"] = 0 };

        // Process each PR
        await AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("[green]Analyzing PRs[/]", maxValue: openPRs.Count);

                foreach (var pr in openPRs)
                {
                    task.Description = $"[dim]PR #{pr.Number}[/]";

                    // Get PR details
                    var prDetails = await github.GetPullRequestAsync(pr.Number);
                    await GitHubService.RateLimitDelayAsync(100);

                    var totalChanges = prDetails.Additions + prDetails.Deletions;
                    var daysOpen = (DateTime.UtcNow - pr.CreatedAt.DateTime).TotalDays;
                    var sizeCategory = LabelParser.GetSizeCategory(totalChanges);
                    var ageCategory = LabelParser.GetAgeCategory((int)daysOpen);
                    var labels = pr.Labels.Select(l => l.Name).ToList();
                    var parsed = LabelParser.Parse(labels);

                    // Check author type
                    var authorType = pr.User.Login.EndsWith("[bot]") ? "bot" :
                        await github.IsMicrosoftMemberAsync(pr.User.Login) ? "microsoft" : "community";
                    await GitHubService.RateLimitDelayAsync(100);

                    // Determine triage category
                    var (category, reasoning) = await GetTriageCategoryAsync(github, pr, prDetails, daysOpen);

                    triagedPRs.Add(new PullRequestInfo(
                        pr.Number,
                        pr.Title,
                        pr.User.Login,
                        pr.User.AvatarUrl,
                        authorType,
                        pr.CreatedAt.DateTime,
                        pr.UpdatedAt.DateTime,
                        Math.Floor(daysOpen),
                        ageCategory,
                        prDetails.ChangedFiles,
                        prDetails.Additions,
                        prDetails.Deletions,
                        totalChanges,
                        sizeCategory,
                        pr.Draft,
                        category,
                        reasoning,
                        parsed.Type,
                        parsed.Areas,
                        parsed.Backends,
                        parsed.Oses,
                        parsed.Other,
                        pr.HtmlUrl
                    ));

                    summary[category]++;
                    bySize[sizeCategory]++;
                    byAge[ageCategory]++;

                    task.Increment(1);
                    await GitHubService.RateLimitDelayAsync(200);
                }

                task.Description = "[green]Analyzing PRs[/]";
            });

        var output = new PrTriageData(
            DateTime.UtcNow,
            openPRs.Count,
            new TriageSummary(
                summary["ReadyToMerge"],
                summary["QuickReview"],
                summary["NeedsReview"],
                summary["NeedsAuthor"],
                summary["ConsiderClosing"]
            ),
            [
                new SizeCount("xs", "< 10 lines", bySize["xs"]),
                new SizeCount("s", "10-50 lines", bySize["s"]),
                new SizeCount("m", "50-200 lines", bySize["m"]),
                new SizeCount("l", "200-500 lines", bySize["l"]),
                new SizeCount("xl", "> 500 lines", bySize["xl"])
            ],
            [
                new AgeCount("fresh", "< 7 days", byAge["fresh"]),
                new AgeCount("recent", "7-30 days", byAge["recent"]),
                new AgeCount("aging", "30-90 days", byAge["aging"]),
                new AgeCount("stale", "90-365 days", byAge["stale"]),
                new AgeCount("ancient", "> 1 year", byAge["ancient"])
            ],
            triagedPRs
        );

        // Write output
        var outputPath = OutputService.GetOutputPath(settings.OutputDir, "pr-triage.json");
        await OutputService.WriteJsonAsync(outputPath, output);

        if (!settings.Quiet)
        {
            var table = new Table()
                .AddColumn("Category")
                .AddColumn(new TableColumn("Count").RightAligned());
            
            table.AddRow("[green]Ready to Merge[/]", summary["ReadyToMerge"].ToString());
            table.AddRow("[cyan]Quick Review[/]", summary["QuickReview"].ToString());
            table.AddRow("Needs Review", summary["NeedsReview"].ToString());
            table.AddRow("[yellow]Needs Author[/]", summary["NeedsAuthor"].ToString());
            table.AddRow("[red]Consider Closing[/]", summary["ConsiderClosing"].ToString());
            table.AddRow("[bold]Total[/]", $"[bold]{openPRs.Count}[/]");
            
            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[green]✓ PR triage data written to {outputPath}[/]");
        }

        return 0;
    }

    private static async Task<(string category, string reasoning)> GetTriageCategoryAsync(
        GitHubService github, Octokit.PullRequest pr, Octokit.PullRequest prDetails, double daysOpen)
    {
        var totalChanges = prDetails.Additions + prDetails.Deletions;
        var hasApproval = false;
        var hasChangesRequested = false;

        try
        {
            var reviews = await github.GetPullRequestReviewsAsync(pr.Number);
            hasApproval = reviews.Any(r => r.State.Value == Octokit.PullRequestReviewState.Approved);
            hasChangesRequested = reviews.Any(r => r.State.Value == Octokit.PullRequestReviewState.ChangesRequested);
        }
        catch
        {
            // Ignore review fetch errors
        }

        // Ready to merge
        if (hasApproval && !hasChangesRequested && !pr.Draft)
            return ("ReadyToMerge", "PR has approval and no changes requested.");

        // Needs author response
        if (hasChangesRequested)
        {
            if (daysOpen > 90)
                return ("ConsiderClosing", "Changes requested over 90 days ago with no response.");
            return ("NeedsAuthor", "Changes have been requested, waiting for author.");
        }

        // Very old
        if (daysOpen > 365)
            return ("ConsiderClosing", "PR is over a year old and may be stale.");

        // Quick review opportunity
        if (totalChanges <= 50 && daysOpen < 30 && !pr.Draft)
            return ("QuickReview", $"Small PR ({totalChanges} lines) - quick review opportunity.");

        // Default: needs review
        if (pr.Draft)
            return ("NeedsReview", "Draft PR - not ready for review yet.");
        if (totalChanges > 500)
            return ("NeedsReview", $"Large PR ({totalChanges} lines) requires careful review.");
        if (prDetails.ChangedFiles > 20)
            return ("NeedsReview", $"PR touches many files ({prDetails.ChangedFiles}) and needs thorough review.");

        return ("NeedsReview", "PR needs human review to assess quality and fit.");
    }
}
