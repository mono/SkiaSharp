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
    
    // These are set by ExecuteAsync and used by helper methods
    private string _owner = "";
    private string _repoName = "";
    
    public override async Task<int> ExecuteAsync(CommandContext context, SyncGitHubSettings settings)
    {
        (_owner, _repoName) = settings.ParseRepository();
        
        // Validate flags
        if (settings.FullRefresh && settings.Resume)
        {
            AnsiConsole.MarkupLine("[red]Error: Cannot use --full and --resume together[/]");
            return 1;
        }

        if (!settings.Quiet)
            AnsiConsole.MarkupLine($"[bold blue]Syncing GitHub data for {_owner}/{_repoName}...[/]");

        // Load config to get repo key for cache path
        var configService = new ConfigService();
        var repoConfig = await configService.GetRepoByFullNameAsync(_owner, _repoName);
        var repoKey = repoConfig?.Key ?? $"{_owner}-{_repoName}";

        // Use per-repo cache path
        var rootCache = new CacheService(settings.CachePath);
        var cache = rootCache.ForRepo(repoKey);

        using var github = new GitHubService(_owner, _repoName, settings.Verbose);
        var apiClient = CreateOctokitClient();

        var syncMeta = await cache.LoadGitHubSyncMetaAsync();
        var index = await cache.LoadGitHubIndexAsync();
        var now = DateTime.UtcNow;

        // Handle --full: clear any existing full sync state and start fresh
        if (settings.FullRefresh)
        {
            syncMeta = syncMeta with
            {
                FullSync = new FullSyncState(
                    StartedAt: now,
                    ItemsComplete: false,
                    EngagementComplete: false,
                    ItemsLastPage: 0,
                    EngagementLastNumber: 0
                )
            };
            if (!settings.Quiet)
                AnsiConsole.MarkupLine("[blue]Starting fresh full sync...[/]");
        }
        // Handle --resume: require existing full sync state
        else if (settings.Resume)
        {
            if (syncMeta.FullSync == null)
            {
                AnsiConsole.MarkupLine("[red]Error: No full sync in progress to resume. Use --full to start a new full sync.[/]");
                return 1;
            }
            if (!settings.Quiet)
                AnsiConsole.MarkupLine($"[blue]Resuming full sync from {syncMeta.FullSync.StartedAt:yyyy-MM-dd HH:mm}...[/]");
        }

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
        var itemsComplete = false;

        // Determine if we're in full sync mode (either starting or resuming)
        var isFullSync = settings.FullRefresh || settings.Resume;

        try
        {
            // Layer 1: Basic item data
            if (!settings.EngagementOnly)
            {
                (index, syncMeta, itemsProcessed, itemsComplete) = await SyncLayer1Async(
                    apiClient, cache, index, syncMeta, settings, isFullSync);
            }

            // Layer 2: Engagement data
            if (!settings.ItemsOnly)
            {
                (index, syncMeta, engagementProcessed, rateLimitHit) = await SyncLayer2Async(
                    apiClient, cache, index, syncMeta, settings, isFullSync);
            }

            // Check if full sync is complete (both layers done)
            if (isFullSync && syncMeta.FullSync != null)
            {
                var itemsDone = settings.EngagementOnly || syncMeta.FullSync.ItemsComplete;
                var engagementDone = settings.ItemsOnly || syncMeta.FullSync.EngagementComplete;
                
                if (itemsDone && engagementDone)
                {
                    // Full sync complete - clear the state
                    syncMeta = syncMeta with { FullSync = null };
                    if (!settings.Quiet)
                        AnsiConsole.MarkupLine("[green]Full sync complete! Clearing resume state.[/]");
                }
            }

            syncMeta = syncMeta with { LastRunStatus = rateLimitHit ? "rate_limited" : "success" };
        }
        catch (RateLimitExceededException)
        {
            AnsiConsole.MarkupLine("[yellow]Rate limit exceeded. Progress saved.[/]");
            syncMeta = syncMeta with { LastRunStatus = "rate_limited" };
            rateLimitHit = true;
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
            if (isFullSync && syncMeta.FullSync != null)
                table.AddRow("Full Sync Progress", $"Items: {(syncMeta.FullSync.ItemsComplete ? "✓" : syncMeta.FullSync.ItemsLastPage + " pages")}, Engagement: {(syncMeta.FullSync.EngagementComplete ? "✓" : syncMeta.FullSync.EngagementLastNumber.ToString())}");
            
            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[green]✓ Sync complete[/]");
        }

        // Exit codes:
        // 0 = work done successfully (batch complete, more to do)
        // 1 = rate limit hit (more work to do)
        // 2 = all done (items-only complete OR all engagement up to date OR full sync complete)
        if (rateLimitHit)
            return 1;
        if (settings.ItemsOnly && itemsComplete)
            return 2; // All items synced
        if (settings.EngagementOnly && engagementProcessed == 0)
            return 2; // All engagement synced, nothing more to do
        if (isFullSync && syncMeta.FullSync == null)
            return 2; // Full sync complete
        return 0;
    }

    private async Task<(CacheIndex, SyncMeta, int, bool)> SyncLayer1Async(
        GitHubClient client,
        CacheService cache,
        CacheIndex index,
        SyncMeta meta,
        SyncGitHubSettings settings,
        bool isFullSync)
    {
        var syncMode = isFullSync ? "full (Created ASC)" : "incremental (Updated DESC)";
        if (!settings.Quiet)
            AnsiConsole.MarkupLine($"\n[bold]Layer 1: Basic item data[/] [dim]({syncMode})[/]");

        // Check if items sync is already complete for this full sync
        if (isFullSync && meta.FullSync?.ItemsComplete == true)
        {
            if (!settings.Quiet)
                AnsiConsole.MarkupLine("[dim]  Items already complete for this full sync[/]");
            return (index, meta, 0, true);
        }

        // Get repo info (also gives us stars/forks/watchers)
        var repo = await client.Repository.Get(_owner, _repoName);
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
        
        // Full sync: fetch all items sorted by Created ASC (stable ordering)
        // Incremental: fetch recently updated items sorted by Updated DESC (recent first)
        var since = isFullSync ? null : meta.Layers.Items.LastSync;
        var itemsDict = index.Items.ToDictionary(i => i.Number);
        var processed = 0;
        
        // For resume, start from the last processed page
        var startPage = (isFullSync && meta.FullSync != null) ? meta.FullSync.ItemsLastPage + 1 : 1;
        var page = startPage;
        
        if (startPage > 1 && !settings.Quiet)
            AnsiConsole.MarkupLine($"[dim]  Resuming from page {startPage}[/]");
        
        var pagesProcessed = 0;
        var maxPages = settings.PageCount > 0 ? settings.PageCount : int.MaxValue;
        var startTime = DateTime.UtcNow;
        var pageTimes = new Queue<double>(); // Rolling average for ETA
        var rateLimitHit = false;
        var allDone = false;

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Fetching items...", async ctx =>
            {
                while (pagesProcessed < maxPages)
                {
                    var pageStart = DateTime.UtcNow;
                    var pct = totalCount > 0 ? (processed * 100 / totalCount) : 0;
                    var eta = GetEta(pageTimes, totalCount - processed, 100); // 100 items per page
                    
                    ctx.Status($"Fetching page {page}... {pct}% ({processed:N0}/{totalCount:N0}){eta}");
                    
                    // Check rate limit
                    var rateLimit = await client.RateLimit.GetRateLimits();
                    if (rateLimit.Resources.Core.Remaining < RateLimitThreshold)
                    {
                        AnsiConsole.MarkupLine($"[yellow]Rate limit low. Stopping at page {page}.[/]");
                        rateLimitHit = true;
                        break;
                    }

                    var request = new RepositoryIssueRequest
                    {
                        State = ItemStateFilter.All,
                        SortProperty = isFullSync ? IssueSort.Created : IssueSort.Updated,
                        SortDirection = isFullSync ? SortDirection.Ascending : SortDirection.Descending,
                        Since = since?.ToUniversalTime()
                    };

                    var issues = await client.Issue.GetAllForRepository(
                        _owner, _repoName, request,
                        new ApiOptions { PageSize = 100, PageCount = 1, StartPage = page });

                    if (issues.Count == 0)
                    {
                        allDone = true;
                        break;
                    }

                    var earlyExit = false;
                    foreach (var issue in issues)
                    {
                        // Early exit optimization for incremental sync:
                        // When sorted by Updated DESC, if we hit an item that's already 
                        // in our cache with the same UpdatedAt, everything after is also synced
                        if (!isFullSync && itemsDict.TryGetValue(issue.Number, out var cached))
                        {
                            if (issue.UpdatedAt.HasValue && cached.UpdatedAt == issue.UpdatedAt.Value.DateTime)
                            {
                                if (settings.Verbose)
                                    AnsiConsole.MarkupLine($"  [dim]Hit cached item #{issue.Number}, stopping early[/]");
                                earlyExit = true;
                                allDone = true;
                                break;
                            }
                        }

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

                    if (earlyExit)
                        break;

                    // Save index after each page to prevent data loss on crash
                    index = index with 
                    { 
                        UpdatedAt = DateTime.UtcNow,
                        Items = [.. itemsDict.Values.OrderBy(i => i.Number)]
                    };
                    await cache.SaveGitHubIndexAsync(index);

                    // Track page time for ETA
                    var pageTime = (DateTime.UtcNow - pageStart).TotalSeconds;
                    pageTimes.Enqueue(pageTime);
                    if (pageTimes.Count > 5) pageTimes.Dequeue(); // Rolling avg of last 5

                    pagesProcessed++;
                    page++;
                    await Task.Delay(100); // Small delay between pages
                }
            });

        var elapsed = DateTime.UtcNow - startTime;
        
        // Index was already saved after each page, but ensure final state is saved
        index = index with 
        { 
            UpdatedAt = DateTime.UtcNow,
            Items = [.. itemsDict.Values.OrderBy(i => i.Number)]
        };

        // Only update LastSync timestamp when we've completed all pages (allDone)
        // This ensures incremental sync uses correct baseline
        meta = meta with 
        {
            Layers = meta.Layers with 
            {
                Items = new LayerStatus(
                    allDone ? "success" : (rateLimitHit ? "rate_limited" : "partial"),
                    allDone ? DateTime.UtcNow : meta.Layers.Items.LastSync, // Only update on complete
                    processed,
                    index.Items.Count
                )
            }
        };

        // Update full sync state if in full sync mode
        if (isFullSync && meta.FullSync != null)
        {
            meta = meta with
            {
                FullSync = meta.FullSync with
                {
                    ItemsLastPage = page - 1,  // Last completed page
                    ItemsComplete = allDone
                }
            };
        }

        if (!settings.Quiet)
        {
            if (allDone)
                AnsiConsole.MarkupLine($"[green]  ✓ {processed:N0} items synced in {FormatTime(elapsed)} (complete)[/]");
            else if (rateLimitHit)
                AnsiConsole.MarkupLine($"[yellow]  → {processed:N0} items synced in {FormatTime(elapsed)} (rate limited)[/]");
            else
                AnsiConsole.MarkupLine($"[blue]  → {processed:N0} items synced in {FormatTime(elapsed)} (batch at page {page - 1}, more available)[/]");
        }

        return (index, meta, processed, allDone);
    }

    private async Task<(CacheIndex, SyncMeta, int, bool)> SyncLayer2Async(
        GitHubClient client,
        CacheService cache,
        CacheIndex index,
        SyncMeta meta,
        SyncGitHubSettings settings,
        bool isFullSync)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("\n[bold]Layer 2: Engagement data[/]");

        // Check if engagement sync is already complete for this full sync
        if (isFullSync && meta.FullSync?.EngagementComplete == true)
        {
            if (!settings.Quiet)
                AnsiConsole.MarkupLine("[dim]  Engagement already complete for this full sync[/]");
            return (index, meta, 0, false);
        }

        var now = DateTime.UtcNow;
        var fullSyncStartedAt = meta.FullSync?.StartedAt ?? now;
        var lastProcessedNumber = (isFullSync && meta.FullSync != null) ? meta.FullSync.EngagementLastNumber : 0;
        
        var itemsDict = index.Items.ToDictionary(i => i.Number);
        var processed = 0;
        var startTime = DateTime.UtcNow;
        var itemTimes = new Queue<double>(); // Rolling average for ETA
        var rateLimitHit = false;
        var lastNumber = lastProcessedNumber;

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
        IEnumerable<IndexItem> needsSync;
        
        if (isFullSync)
        {
            // Full sync: process items that were synced before this full sync started
            // Order by number to enable resume from lastProcessedNumber
            needsSync = index.Items
                .Where(i => 
                    !meta.Failures.ContainsKey(i.Number.ToString()) &&
                    i.Number > lastProcessedNumber && // Resume from last processed
                    (i.EngagementSyncedAt == null || i.EngagementSyncedAt < fullSyncStartedAt))
                .OrderBy(i => i.Number) // Process in order for predictable resume
                .Take(settings.EngagementCount);
        }
        else
        {
            // Incremental: process items updated since last engagement sync
            needsSync = index.Items
                .Where(i => 
                    !meta.Failures.ContainsKey(i.Number.ToString()) &&
                    (i.EngagementSyncedAt == null || i.UpdatedAt > i.EngagementSyncedAt))
                .OrderByDescending(i => i.UpdatedAt) // Most recently updated first
                .Take(settings.EngagementCount);
        }

        var needsSyncList = needsSync.ToList();

        // Add retry items to front of queue
        var retryItems = toRetry
            .Where(n => itemsDict.ContainsKey(n))
            .Select(n => itemsDict[n])
            .ToList();
        
        var toProcess = retryItems.Concat(needsSyncList).Take(settings.EngagementCount).ToList();
        var totalToProcess = toProcess.Count;

        if (isFullSync && lastProcessedNumber > 0 && !settings.Quiet)
            AnsiConsole.MarkupLine($"  [dim]Resuming from item #{lastProcessedNumber}[/]");
        
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
                        lastNumber = indexItem.Number; // Track for resume

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
                        lastNumber = indexItem.Number; // Still track for resume
                        if (settings.Verbose)
                            AnsiConsole.MarkupLine($"  [yellow]#{indexItem.Number}: Not found, added to skip list[/]");
                    }
                    catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                    {
                        meta = cache.AddFailure(meta, indexItem.Number, "engagement", "server_error", 500, ex.Message);
                        lastNumber = indexItem.Number; // Still track for resume
                        if (settings.Verbose)
                            AnsiConsole.MarkupLine($"  [yellow]#{indexItem.Number}: Server error, added to skip list[/]");
                    }
                }
            });

        var elapsed = DateTime.UtcNow - startTime;
        
        // Check if all engagement is done (no more items to process)
        var allDone = needsSyncList.Count == 0 || (processed == needsSyncList.Count && !rateLimitHit);
        
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
                    lastNumber,
                    processed,
                    settings.EngagementCount,
                    index.Items.Count(i => i.EngagementSyncedAt != null)
                )
            }
        };

        // Update full sync state if in full sync mode
        if (isFullSync && meta.FullSync != null)
        {
            meta = meta with
            {
                FullSync = meta.FullSync with
                {
                    EngagementLastNumber = lastNumber,
                    EngagementComplete = allDone
                }
            };
        }

        if (!settings.Quiet)
        {
            if (allDone)
                AnsiConsole.MarkupLine($"[green]  ✓ {processed} items with engagement synced in {FormatTime(elapsed)} (complete)[/]");
            else
                AnsiConsole.MarkupLine($"[blue]  → {processed} items synced in {FormatTime(elapsed)} (last: #{lastNumber})[/]");
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
        var reactions = await client.Reaction.Issue.GetAll(_owner, _repoName, number);
        await Task.Delay(50);

        // Fetch comments
        var comments = await client.Issue.Comment.GetAllForIssue(_owner, _repoName, number);
        await Task.Delay(50);

        var commentInfos = new List<CommentInfo>();
        
        // Fetch reactions for each comment (limited to avoid rate limits)
        foreach (var comment in comments.Take(50))
        {
            try
            {
                var commentReactions = await client.Reaction.IssueComment.GetAll(_owner, _repoName, comment.Id);
                commentInfos.Add(new CommentInfo(
                    comment.Id,
                    comment.User?.Login ?? "unknown",
                    comment.Body ?? "",
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
                    comment.Body ?? "",
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
                pr = await client.PullRequest.Get(_owner, _repoName, issue.Number);
                var prReviews = await client.PullRequest.Review.GetAll(_owner, _repoName, issue.Number);
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
            pr?.Merged,
            pr?.MergedAt?.DateTime,
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
        item.ClosedAt,
        [.. item.Labels.Select(l => l.Name)],
        item.CommentCount,
        item.ReactionCount,
        DateTime.UtcNow,
        item.Engagement?.SyncedAt,
        item.Draft,
        item.Mergeable,
        item.Merged,
        item.MergedAt,
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
