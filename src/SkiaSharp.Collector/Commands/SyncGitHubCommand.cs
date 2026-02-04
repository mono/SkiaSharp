using Octokit;
using SkiaSharp.Collector.Models;
using SkiaSharp.Collector.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using AuthorInfo = SkiaSharp.Collector.Models.AuthorInfo;
using LabelInfo = SkiaSharp.Collector.Models.LabelInfo;
using ReviewInfo = SkiaSharp.Collector.Models.ReviewInfo;

namespace SkiaSharp.Collector.Commands;

/// <summary>
/// Syncs GitHub data to the cache.
/// </summary>
public class SyncGitHubCommand : AsyncCommand<SyncGitHubSettings>
{
    private const int RateLimitThreshold = 100;
    
    public override async Task<int> ExecuteAsync(CommandContext context, SyncGitHubSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine($"[bold blue]Syncing GitHub data for {settings.Owner}/{settings.Repo}...[/]");

        var cache = new CacheService(settings.CachePath);
        using var github = new GitHubService(settings.Owner, settings.Repo, settings.Verbose);
        var apiClient = CreateOctokitClient();

        var syncMeta = await cache.LoadGitHubSyncMetaAsync();
        var index = await cache.LoadGitHubIndexAsync();
        var now = DateTime.UtcNow;

        // Check rate limit before starting
        var rateLimit = await apiClient.RateLimit.GetRateLimits();
        if (rateLimit.Resources.Core.Remaining < RateLimitThreshold)
        {
            AnsiConsole.MarkupLine($"[yellow]Rate limit low ({rateLimit.Resources.Core.Remaining} remaining). Exiting gracefully.[/]");
            return 1;
        }

        syncMeta = syncMeta with 
        { 
            LastRun = now,
            RateLimit = new RateLimitInfo(rateLimit.Resources.Core.Remaining, rateLimit.Resources.Core.Reset.DateTime)
        };

        var itemsProcessed = 0;
        var engagementProcessed = 0;
        var rateLimitHit = false;

        try
        {
            // Layer 1: Basic item data
            if (!settings.EngagementOnly)
            {
                (index, syncMeta, itemsProcessed) = await SyncLayer1Async(
                    apiClient, cache, index, syncMeta, settings);
            }

            // Layer 2: Engagement data
            if (!settings.ItemsOnly)
            {
                (index, syncMeta, engagementProcessed, rateLimitHit) = await SyncLayer2Async(
                    apiClient, cache, index, syncMeta, settings);
            }

            syncMeta = syncMeta with { LastRunStatus = rateLimitHit ? "rate_limited" : "success" };
        }
        catch (RateLimitExceededException)
        {
            AnsiConsole.MarkupLine("[yellow]Rate limit exceeded. Progress saved.[/]");
            syncMeta = syncMeta with { LastRunStatus = "rate_limited" };
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            syncMeta = syncMeta with { LastRunStatus = "error" };
            throw;
        }
        finally
        {
            // Always save state
            await cache.SaveGitHubIndexAsync(index);
            await cache.SaveGitHubSyncMetaAsync(syncMeta);
        }

        if (!settings.Quiet)
        {
            var table = new Table()
                .AddColumn("Metric")
                .AddColumn(new TableColumn("Value").RightAligned());
            
            table.AddRow("Items Synced", $"[green]{itemsProcessed}[/]");
            table.AddRow("Engagement Synced", $"[green]{engagementProcessed}[/]");
            table.AddRow("Total in Cache", index.Items.Count.ToString());
            table.AddRow("With Engagement", index.Items.Count(i => i.EngagementSyncedAt != null).ToString());
            
            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[green]✓ Sync complete[/]");
        }

        // Return exit code 1 if rate limited (signals "more work to do")
        return rateLimitHit ? 1 : 0;
    }

    private async Task<(CacheIndex, SyncMeta, int)> SyncLayer1Async(
        GitHubClient client,
        CacheService cache,
        CacheIndex index,
        SyncMeta meta,
        SyncGitHubSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("\n[bold]Layer 1: Basic item data[/]");

        // Get repo info (also gives us stars/forks/watchers)
        var repo = await client.Repository.Get(settings.Owner, settings.Repo);
        var totalCount = repo.OpenIssuesCount; // Includes open issues + PRs

        // Save repo stats
        var repoStats = new RepoStats(
            repo.StargazersCount,
            repo.ForksCount,
            repo.SubscribersCount,
            repo.Description,
            repo.License?.Name,
            repo.Topics?.ToList() ?? [],
            repo.CreatedAt.DateTime,
            repo.PushedAt?.DateTime,
            DateTime.UtcNow
        );
        await cache.SaveRepoStatsAsync(repoStats);
        if (settings.Verbose)
            AnsiConsole.MarkupLine($"  [dim]Saved repo stats: {repoStats.Stars:N0} stars, {repoStats.Forks:N0} forks[/]");
        
        var since = settings.FullRefresh ? null : meta.Layers.Items.LastSync;
        var itemsDict = index.Items.ToDictionary(i => i.Number);
        var processed = 0;
        var page = 1;
        var startTime = DateTime.UtcNow;
        var pageTimes = new Queue<double>(); // Rolling average for ETA

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Fetching items...", async ctx =>
            {
                while (true)
                {
                    var pageStart = DateTime.UtcNow;
                    var pct = totalCount > 0 ? (processed * 100 / totalCount) : 0;
                    var eta = GetEta(pageTimes, totalCount - processed, 100); // 100 items per page
                    
                    ctx.Status($"Fetching... {pct}% ({processed:N0}/{totalCount:N0}){eta}");
                    
                    // Check rate limit
                    var rateLimit = await client.RateLimit.GetRateLimits();
                    if (rateLimit.Resources.Core.Remaining < RateLimitThreshold)
                    {
                        AnsiConsole.MarkupLine($"[yellow]Rate limit low. Stopping at page {page}.[/]");
                        break;
                    }

                    var request = new RepositoryIssueRequest
                    {
                        State = ItemStateFilter.All,
                        SortProperty = IssueSort.Updated,
                        SortDirection = SortDirection.Descending,
                        Since = since?.ToUniversalTime()
                    };

                    var issues = await client.Issue.GetAllForRepository(
                        settings.Owner, settings.Repo, request,
                        new ApiOptions { PageSize = 100, PageCount = 1, StartPage = page });

                    if (issues.Count == 0)
                        break;

                    foreach (var issue in issues)
                    {
                        var isPr = issue.PullRequest != null;
                        var item = await CreateCachedItemAsync(client, issue, isPr, settings);
                        
                        // Preserve existing engagement data
                        var existing = await cache.LoadItemAsync(issue.Number);
                        if (existing?.Engagement != null)
                            item = item with { Engagement = existing.Engagement };

                        await cache.SaveItemAsync(item);

                        // Update index
                        itemsDict[issue.Number] = CreateIndexItem(item);
                        processed++;

                        if (settings.Verbose)
                            AnsiConsole.MarkupLine($"  [dim]#{issue.Number}: {Markup.Escape(issue.Title.Truncate(50))}[/]");
                    }

                    // Track page time for ETA
                    var pageTime = (DateTime.UtcNow - pageStart).TotalSeconds;
                    pageTimes.Enqueue(pageTime);
                    if (pageTimes.Count > 5) pageTimes.Dequeue(); // Rolling avg of last 5

                    page++;
                    await Task.Delay(100); // Small delay between pages
                }
            });

        var elapsed = DateTime.UtcNow - startTime;
        
        index = index with 
        { 
            UpdatedAt = DateTime.UtcNow,
            Items = [.. itemsDict.Values.OrderBy(i => i.Number)]
        };

        meta = meta with 
        {
            Layers = meta.Layers with 
            {
                Items = new LayerStatus(
                    "success",
                    DateTime.UtcNow,
                    processed,
                    index.Items.Count
                )
            }
        };

        if (!settings.Quiet)
            AnsiConsole.MarkupLine($"[green]  ✓ {processed:N0} items synced in {FormatTime(elapsed)}[/]");

        return (index, meta, processed);
    }

    private async Task<(CacheIndex, SyncMeta, int, bool)> SyncLayer2Async(
        GitHubClient client,
        CacheService cache,
        CacheIndex index,
        SyncMeta meta,
        SyncGitHubSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("\n[bold]Layer 2: Engagement data[/]");

        var now = DateTime.UtcNow;
        var itemsDict = index.Items.ToDictionary(i => i.Number);
        var processed = 0;
        var startTime = DateTime.UtcNow;
        var itemTimes = new Queue<double>(); // Rolling average for ETA
        var rateLimitHit = false;

        // Process skip list retries first
        var toRetry = cache.GetItemsToRetry(meta, now);
        var toRemove = cache.GetItemsToRemove(meta, now);

        // Remove expired failures
        foreach (var num in toRemove)
        {
            meta = cache.RemoveFailure(meta, num);
            if (settings.Verbose)
                AnsiConsole.MarkupLine($"  [dim]Removed #{num} from skip list (expired)[/]");
        }

        // Find items needing engagement sync
        var needsSync = index.Items
            .Where(i => 
                !meta.Failures.ContainsKey(i.Number.ToString()) && // Not on skip list
                (i.EngagementSyncedAt == null || i.UpdatedAt > i.EngagementSyncedAt)) // Needs sync
            .OrderByDescending(i => i.UpdatedAt) // Most recently updated first
            .Take(settings.EngagementCount)
            .ToList();

        // Add retry items to front of queue
        var retryItems = toRetry
            .Where(n => itemsDict.ContainsKey(n))
            .Select(n => itemsDict[n])
            .ToList();
        
        var toProcess = retryItems.Concat(needsSync).Take(settings.EngagementCount).ToList();
        var totalToProcess = toProcess.Count;

        if (!settings.Quiet && toRetry.Count > 0)
            AnsiConsole.MarkupLine($"  [dim]Retrying {Math.Min(toRetry.Count, settings.EngagementCount)} items from skip list[/]");

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync($"Syncing engagement for {toProcess.Count} items...", async ctx =>
            {
                foreach (var indexItem in toProcess)
                {
                    var itemStart = DateTime.UtcNow;
                    var pct = totalToProcess > 0 ? ((processed + 1) * 100 / totalToProcess) : 0;
                    var eta = GetEta(itemTimes, totalToProcess - processed - 1, 1);
                    
                    ctx.Status($"{pct}% ({processed + 1}/{totalToProcess}) #{indexItem.Number}{eta}");

                    // Check rate limit
                    var rateLimit = await client.RateLimit.GetRateLimits();
                    if (rateLimit.Resources.Core.Remaining < RateLimitThreshold)
                    {
                        AnsiConsole.MarkupLine($"[yellow]Rate limit low ({rateLimit.Resources.Core.Remaining} remaining). Stopping engagement sync.[/]");
                        rateLimitHit = true;
                        break;
                    }

                    try
                    {
                        var item = await cache.LoadItemAsync(indexItem.Number);
                        if (item == null) continue;

                        var engagement = await FetchEngagementAsync(client, settings, indexItem.Number);
                        item = item with { Engagement = engagement };
                        await cache.SaveItemAsync(item);

                        // Update index
                        itemsDict[indexItem.Number] = itemsDict[indexItem.Number] with 
                        { 
                            EngagementSyncedAt = engagement.SyncedAt 
                        };

                        // Remove from skip list if it was there
                        meta = cache.RemoveFailure(meta, indexItem.Number);
                        processed++;

                        if (settings.Verbose)
                            AnsiConsole.MarkupLine($"  [dim]#{indexItem.Number}: {engagement.Comments.Count} comments, {engagement.Reactions.Count} reactions[/]");

                        // Track item time for ETA
                        var itemTime = (DateTime.UtcNow - itemStart).TotalSeconds;
                        itemTimes.Enqueue(itemTime);
                        if (itemTimes.Count > 5) itemTimes.Dequeue();

                        await Task.Delay(100);
                    }
                    catch (NotFoundException)
                    {
                        meta = cache.AddFailure(meta, indexItem.Number, "engagement", "not_found", 404, "Item not found");
                        if (settings.Verbose)
                            AnsiConsole.MarkupLine($"  [yellow]#{indexItem.Number}: Not found, added to skip list[/]");
                    }
                    catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                    {
                        meta = cache.AddFailure(meta, indexItem.Number, "engagement", "server_error", 500, ex.Message);
                        if (settings.Verbose)
                            AnsiConsole.MarkupLine($"  [yellow]#{indexItem.Number}: Server error, added to skip list[/]");
                    }
                }
            });

        var elapsed = DateTime.UtcNow - startTime;
        
        index = index with 
        { 
            Items = [.. itemsDict.Values.OrderBy(i => i.Number)]
        };

        meta = meta with 
        {
            Layers = meta.Layers with 
            {
                Engagement = new EngagementLayerStatus(
                    "success",
                    DateTime.UtcNow,
                    null,
                    processed,
                    settings.EngagementCount,
                    index.Items.Count(i => i.EngagementSyncedAt != null)
                )
            }
        };

        if (!settings.Quiet)
        {
            AnsiConsole.MarkupLine($"[green]  ✓ {processed} items with engagement synced in {FormatTime(elapsed)}[/]");
            if (meta.Failures.Count > 0)
                AnsiConsole.MarkupLine($"[yellow]  ⚠ {meta.Failures.Count} items on skip list[/]");
            if (rateLimitHit)
                AnsiConsole.MarkupLine($"[yellow]  ⚠ Rate limit hit - will resume on next run[/]");
        }

        return (index, meta, processed, rateLimitHit);
    }

    private async Task<EngagementData> FetchEngagementAsync(GitHubClient client, SyncGitHubSettings settings, int number)
    {
        // Fetch issue-level reactions
        var reactions = await client.Reaction.Issue.GetAll(settings.Owner, settings.Repo, number);
        await Task.Delay(50);

        // Fetch comments
        var comments = await client.Issue.Comment.GetAllForIssue(settings.Owner, settings.Repo, number);
        await Task.Delay(50);

        var commentInfos = new List<CommentInfo>();
        
        // Fetch reactions for each comment (limited to avoid rate limits)
        foreach (var comment in comments.Take(50))
        {
            try
            {
                var commentReactions = await client.Reaction.IssueComment.GetAll(settings.Owner, settings.Repo, comment.Id);
                commentInfos.Add(new CommentInfo(
                    comment.Id,
                    comment.User?.Login ?? "unknown",
                    comment.CreatedAt.DateTime,
                    [.. commentReactions.Select(r => new ReactionInfo(
                        r.User?.Login ?? "unknown",
                        r.Content.StringValue,
                        DateTime.UtcNow // Reaction API doesn't provide created_at
                    ))]
                ));
                await Task.Delay(50);
            }
            catch
            {
                // If we can't get reactions for a comment, still include the comment
                commentInfos.Add(new CommentInfo(
                    comment.Id,
                    comment.User?.Login ?? "unknown",
                    comment.CreatedAt.DateTime,
                    []
                ));
            }
        }

        return new EngagementData(
            DateTime.UtcNow,
            [.. reactions.Select(r => new ReactionInfo(
                r.User?.Login ?? "unknown",
                r.Content.StringValue,
                DateTime.UtcNow // Reaction API doesn't provide created_at
            ))],
            commentInfos
        );
    }

    private async Task<CachedItem> CreateCachedItemAsync(GitHubClient client, Issue issue, bool isPr, SyncGitHubSettings settings)
    {
        // For PRs, fetch additional data
        PullRequest? pr = null;
        List<ReviewInfo>? reviews = null;

        if (isPr)
        {
            try
            {
                pr = await client.PullRequest.Get(settings.Owner, settings.Repo, issue.Number);
                var prReviews = await client.PullRequest.Review.GetAll(settings.Owner, settings.Repo, issue.Number);
                reviews = [.. prReviews.Select(r => new ReviewInfo(
                    r.User?.Login ?? "unknown",
                    r.State.StringValue,
                    r.SubmittedAt.DateTime
                ))];
            }
            catch { /* Ignore PR fetch errors, use basic data */ }
        }

        return new CachedItem(
            issue.Number,
            isPr ? "pr" : "issue",
            issue.State.StringValue.ToLower(),
            issue.Title,
            issue.Body ?? "",
            new AuthorInfo(issue.User?.Login ?? "unknown", issue.User?.Type?.ToString()),
            [.. issue.Labels.Select(l => new LabelInfo(l.Name, l.Color))],
            [.. issue.Assignees.Select(a => new AuthorInfo(a.Login, a.Type?.ToString()))],
            issue.Milestone?.Title,
            issue.CreatedAt.DateTime,
            issue.UpdatedAt?.DateTime ?? issue.CreatedAt.DateTime,
            issue.ClosedAt?.DateTime,
            issue.Comments,
            issue.Reactions?.TotalCount ?? 0,
            // PR fields
            pr?.Draft,
            pr?.Mergeable,
            pr?.MergeableState?.StringValue,
            pr?.Base?.Ref,
            pr?.Head?.Ref,
            pr?.Additions,
            pr?.Deletions,
            pr?.ChangedFiles,
            pr?.Commits,
            null, // ReviewDecision not available in REST API
            reviews,
            null // Engagement synced separately
        );
    }

    private static IndexItem CreateIndexItem(CachedItem item) => new(
        item.Number,
        item.Type,
        item.State,
        item.Title,
        item.Author.Login,
        item.CreatedAt,
        item.UpdatedAt,
        [.. item.Labels.Select(l => l.Name)],
        item.CommentCount,
        item.ReactionCount,
        DateTime.UtcNow,
        item.Engagement?.SyncedAt,
        item.Draft,
        item.Mergeable,
        item.BaseBranch,
        item.HeadBranch,
        null,
        null
    );

    private static GitHubClient CreateOctokitClient()
    {
        var client = new GitHubClient(new ProductHeaderValue("SkiaSharp-Collector"));
        var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (!string.IsNullOrEmpty(token))
            client.Credentials = new Credentials(token);
        return client;
    }

    private static string GetEta(Queue<double> times, int remaining, int itemsPerUnit)
    {
        if (times.Count == 0 || remaining <= 0)
            return "";
        
        var avgTime = times.Average();
        var unitsRemaining = (double)remaining / itemsPerUnit;
        var secondsRemaining = avgTime * unitsRemaining;
        
        if (secondsRemaining < 5)
            return "";
            
        return $" ~{FormatTime(TimeSpan.FromSeconds(secondsRemaining))} remaining";
    }

    private static string FormatTime(TimeSpan ts)
    {
        if (ts.TotalHours >= 1)
            return $"{(int)ts.TotalHours}h {ts.Minutes}m";
        if (ts.TotalMinutes >= 1)
            return $"{(int)ts.TotalMinutes}m {ts.Seconds}s";
        return $"{ts.Seconds}s";
    }
}

internal static class StringExtensions
{
    public static string Truncate(this string value, int maxLength)
        => value.Length <= maxLength ? value : value[..(maxLength - 3)] + "...";
}
