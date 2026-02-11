using Octokit;
using SkiaSharp.Triage.Cli.Models;
using SkiaSharp.Triage.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using AuthorInfo = SkiaSharp.Triage.Cli.Models.AuthorInfo;
using LabelInfo = SkiaSharp.Triage.Cli.Models.LabelInfo;
using ReviewInfo = SkiaSharp.Triage.Cli.Models.ReviewInfo;

namespace SkiaSharp.Triage.Cli.Commands;

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

        // Handle --reset: delete all cached data and start fresh
        if (settings.Reset)
        {
            if (!settings.Quiet)
                AnsiConsole.MarkupLine("[yellow]Resetting cache - deleting all items...[/]");
            
            await cache.ClearGitHubItemsAsync();
            index = new CacheIndex(null, []);
            syncMeta = syncMeta with
            {
                InitialSyncComplete = false,
                LastPage = 0,
                Layers = new SyncLayers(
                    new LayerStatus(null, null, 0, 0),
                    new EngagementLayerStatus(null, null, null, 0, 0, 0)
                ),
                Failures = []
            };
            
            // Save immediately so state is consistent even if we crash or hit rate limit
            await cache.SaveGitHubIndexAsync(index);
            await cache.SaveGitHubSyncMetaAsync(syncMeta);
            
            if (!settings.Quiet)
                AnsiConsole.MarkupLine("[green]Cache cleared. Starting fresh sync...[/]");
        }

        // Check rate limit before starting
        var rateLimit = await apiClient.RateLimit.GetRateLimits();
        if (rateLimit.Resources.Core.Remaining < RateLimitThreshold)
        {
            AnsiConsole.MarkupLine($"[yellow]Rate limit low ({rateLimit.Resources.Core.Remaining} remaining). Exiting gracefully.[/]");
            syncMeta = syncMeta with 
            { 
                LastRun = now,
                LastRunStatus = "rate_limited",
                RateLimit = new RateLimitInfo(rateLimit.Resources.Core.Remaining, rateLimit.Resources.Core.Reset.DateTime)
            };
            await cache.SaveGitHubSyncMetaAsync(syncMeta);
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

        // Determine sync mode based on InitialSyncComplete flag
        var isInitialSync = !syncMeta.InitialSyncComplete;

        try
        {
            // Layer 1: Basic item data
            if (!settings.EngagementOnly)
            {
                (index, syncMeta, itemsProcessed, itemsComplete) = await SyncLayer1Async(
                    apiClient, cache, index, syncMeta, settings, isInitialSync);
                
                // Detect if Layer 1 hit rate limit (it returns allDone=false, status=rate_limited)
                if (syncMeta.Layers.Items.Status == "rate_limited")
                    rateLimitHit = true;
            }

            // Layer 2: Engagement data
            if (!settings.ItemsOnly && !rateLimitHit)
            {
                (index, syncMeta, engagementProcessed, rateLimitHit) = await SyncLayer2Async(
                    apiClient, cache, index, syncMeta, settings);
            }

            // Check if initial sync is complete
            if (isInitialSync && itemsComplete && !settings.ItemsOnly)
            {
                // Items are done, check if engagement is also done
                var engagementDone = !index.Items.Any(i => i.EngagementSyncedAt == null);
                if (engagementDone || settings.EngagementOnly)
                {
                    syncMeta = syncMeta with { InitialSyncComplete = true, LastPage = 0 };
                    if (!settings.Quiet)
                        AnsiConsole.MarkupLine("[green]Initial sync complete! Switching to incremental mode.[/]");
                }
            }
            else if (isInitialSync && itemsComplete && settings.ItemsOnly)
            {
                // Items-only mode and items are done
                syncMeta = syncMeta with { InitialSyncComplete = true, LastPage = 0 };
                if (!settings.Quiet)
                    AnsiConsole.MarkupLine("[green]Initial items sync complete![/]");
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
            
            table.AddRow("Mode", isInitialSync ? "[yellow]Initial[/]" : "[green]Incremental[/]");
            table.AddRow("Items Synced", $"[green]{itemsProcessed}[/]");
            table.AddRow("Engagement Synced", $"[green]{engagementProcessed}[/]");
            table.AddRow("Total in Cache", index.Items.Count.ToString());
            table.AddRow("With Engagement", index.Items.Count(i => i.EngagementSyncedAt != null).ToString());
            
            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[green]✓ Sync complete[/]");
        }

        // Exit codes:
        // 0 = work done successfully (batch complete, more to do)
        // 1 = rate limit hit
        // 2 = all done (items complete AND engagement complete or nothing pending)
        if (rateLimitHit)
            return 1;
        
        // Check if there's actually pending work (excluding cooldown items)
        var pendingEngagement = index.Items.Count(i => 
            !syncMeta.Failures.ContainsKey(i.Number.ToString()) &&
            (i.EngagementSyncedAt == null || i.UpdatedAt > i.EngagementSyncedAt));
        
        if (settings.ItemsOnly && itemsComplete)
            return 2;
        if (settings.EngagementOnly && engagementProcessed == 0 && pendingEngagement == 0)
            return 2;
        if (itemsComplete && engagementProcessed == 0 && pendingEngagement == 0)
            return 2;
        return 0;
    }

    private async Task<(CacheIndex, SyncMeta, int, bool)> SyncLayer1Async(
        GitHubClient client,
        CacheService cache,
        CacheIndex index,
        SyncMeta meta,
        SyncGitHubSettings settings,
        bool isInitialSync)
    {
        var syncMode = isInitialSync ? "initial (Created ASC)" : "incremental (Updated DESC)";
        if (!settings.Quiet)
            AnsiConsole.MarkupLine($"\n[bold]Layer 1: Basic item data[/] [dim]({syncMode})[/]");

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
        
        // Initial sync: Created ASC, no since filter (paginate through all)
        // Incremental: Updated DESC with since=LastSync (for recent changes)
        DateTimeOffset? since = isInitialSync ? null : meta.Layers.Items.LastSync;
        
        var itemsDict = index.Items.ToDictionary(i => i.Number);
        var processed = 0;
        var pagesProcessed = 0;
        var maxPages = settings.PageCount > 0 ? settings.PageCount : int.MaxValue;
        var startTime = DateTime.UtcNow;
        var pageTimes = new Queue<double>();
        var rateLimitHit = false;
        var allDone = false;

        // For initial sync: resume from last completed page
        var page = isInitialSync && meta.LastPage > 0 ? meta.LastPage + 1 : 1;
        
        if (isInitialSync && meta.LastPage > 0 && !settings.Quiet)
            AnsiConsole.MarkupLine($"[dim]  Resuming from page {page} (last completed: {meta.LastPage})[/]");

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Fetching items...", async ctx =>
            {
                while (pagesProcessed < maxPages)
                {
                    var pageStart = DateTime.UtcNow;
                    var pct = totalCount > 0 ? (processed * 100 / totalCount) : 0;
                    var eta = GetEta(pageTimes, totalCount - processed, 100);
                    
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
                        SortProperty = isInitialSync ? IssueSort.Created : IssueSort.Updated,
                        SortDirection = isInitialSync ? SortDirection.Ascending : SortDirection.Descending,
                        Since = since?.UtcDateTime
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
                        // Early exit for incremental: stop when we hit unchanged cached item
                        if (!isInitialSync && itemsDict.TryGetValue(issue.Number, out var cached))
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
                        itemsDict[issue.Number] = CreateIndexItem(item);
                        processed++;

                        if (settings.Verbose)
                            AnsiConsole.MarkupLine($"  [dim]#{issue.Number}: {Markup.Escape(issue.Title.Truncate(50))}[/]");
                    }

                    if (earlyExit)
                        break;

                    // Save index and meta after each page for crash recovery
                    index = index with 
                    { 
                        UpdatedAt = DateTime.UtcNow,
                        Items = [.. itemsDict.Values.OrderBy(i => i.Number)]
                    };
                    await cache.SaveGitHubIndexAsync(index);
                    
                    // Save last completed page for resume
                    if (isInitialSync)
                    {
                        meta = meta with { LastPage = page };
                        await cache.SaveGitHubSyncMetaAsync(meta);
                    }

                    var pageTime = (DateTime.UtcNow - pageStart).TotalSeconds;
                    pageTimes.Enqueue(pageTime);
                    if (pageTimes.Count > 5) pageTimes.Dequeue();

                    pagesProcessed++;
                    page++;
                    await Task.Delay(100);
                }
            });

        var elapsed = DateTime.UtcNow - startTime;
        
        index = index with 
        { 
            UpdatedAt = DateTime.UtcNow,
            Items = [.. itemsDict.Values.OrderBy(i => i.Number)]
        };

        // Update sync metadata (LastPage already saved per-page, just update layers)
        meta = meta with 
        {
            Layers = meta.Layers with 
            {
                Items = new LayerStatus(
                    allDone ? "success" : (rateLimitHit ? "rate_limited" : "partial"),
                    allDone ? DateTime.UtcNow : meta.Layers.Items.LastSync,
                    processed,
                    index.Items.Count
                )
            }
        };

        if (!settings.Quiet)
        {
            if (allDone)
                AnsiConsole.MarkupLine($"[green]  ✓ {processed:N0} items synced in {FormatTime(elapsed)} (complete)[/]");
            else if (rateLimitHit)
                AnsiConsole.MarkupLine($"[yellow]  → {processed:N0} items synced in {FormatTime(elapsed)} (rate limited, page {page - 1})[/]");
            else
                AnsiConsole.MarkupLine($"[blue]  → {processed:N0} items synced in {FormatTime(elapsed)} (batch complete, page {page - 1})[/]");
        }

        return (index, meta, processed, allDone);
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
        var itemTimes = new Queue<double>();
        var rateLimitHit = false;

        // Process skip list retries first
        var toRetry = cache.GetItemsToRetry(meta, now);
        var toRemove = cache.GetItemsToRemove(meta, now);

        foreach (var num in toRemove)
        {
            meta = cache.RemoveFailure(meta, num);
            if (settings.Verbose)
                AnsiConsole.MarkupLine($"  [dim]Removed #{num} from skip list (expired)[/]");
        }

        // Find items needing engagement sync:
        // - Items with no engagement data (EngagementSyncedAt = null)
        // - Items updated since last engagement sync (UpdatedAt > EngagementSyncedAt)
        var needsSync = index.Items
            .Where(i => 
                !meta.Failures.ContainsKey(i.Number.ToString()) &&
                (i.EngagementSyncedAt == null || i.UpdatedAt > i.EngagementSyncedAt))
            .OrderByDescending(i => i.UpdatedAt)
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

                        // Fix for clock skew: ensure SyncedAt is at least UpdatedAt
                        var syncedAt = engagement.SyncedAt >= item.UpdatedAt ? engagement.SyncedAt : item.UpdatedAt;

                        item = item with { Engagement = engagement };
                        await cache.SaveItemAsync(item);

                        itemsDict[indexItem.Number] = itemsDict[indexItem.Number] with 
                        { 
                            EngagementSyncedAt = syncedAt 
                        };

                        meta = cache.RemoveFailure(meta, indexItem.Number);
                        processed++;

                        // Update index and save periodically
                        index = index with 
                        { 
                            Items = [.. itemsDict.Values.OrderBy(i => i.Number)]
                        };

                        if (processed % 5 == 0)
                        {
                            await cache.SaveGitHubIndexAsync(index);
                            await cache.SaveGitHubSyncMetaAsync(meta);
                        }

                        if (settings.Verbose)
                            AnsiConsole.MarkupLine($"  [dim]#{indexItem.Number}: {engagement.Comments.Count} comments, {engagement.Reactions.Count} reactions[/]");

                        var itemTime = (DateTime.UtcNow - itemStart).TotalSeconds;
                        itemTimes.Enqueue(itemTime);
                        if (itemTimes.Count > 5) itemTimes.Dequeue();

                        await Task.Delay(100);
                    }
                    catch (RateLimitExceededException)
                    {
                        AnsiConsole.MarkupLine($"[yellow]Rate limit hit during item #{indexItem.Number}. Stopping.[/]");
                        rateLimitHit = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        var status = 500;
                        var type = "server_error";
                        var msg = ex.Message;

                        if (ex is NotFoundException) { status = 404; type = "not_found"; }
                        else if (ex is ApiException apiEx) { status = (int)apiEx.StatusCode; type = status == 404 ? "not_found" : "api_error"; }

                        meta = cache.AddFailure(meta, indexItem.Number, "engagement", type, status, msg);
                        await cache.SaveGitHubSyncMetaAsync(meta);
                        
                        if (settings.Verbose)
                            AnsiConsole.MarkupLine($"  [yellow]#{indexItem.Number}: Failed ({type}): {msg}[/]");
                    }
                }
            });

        var elapsed = DateTime.UtcNow - startTime;
        
        // Update index FIRST, then check remaining count
        index = index with 
        { 
            Items = [.. itemsDict.Values.OrderBy(i => i.Number)]
        };

        // Check if more items need processing (using updated index)
        var remainingCount = index.Items.Count(i => 
            !meta.Failures.ContainsKey(i.Number.ToString()) &&
            (i.EngagementSyncedAt == null || i.UpdatedAt > i.EngagementSyncedAt));
        var itemsInCooldown = meta.Failures.Count(f => f.Value.RetryAfter > DateTime.UtcNow);
        var allDone = remainingCount == 0 && (processed > 0 || itemsInCooldown == 0);

        meta = meta with 
        {
            Layers = meta.Layers with 
            {
                Engagement = new EngagementLayerStatus(
                    rateLimitHit ? "rate_limited" : (allDone ? "success" : "partial"),
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
            if (allDone)
                AnsiConsole.MarkupLine($"[green]  ✓ {processed} items with engagement synced in {FormatTime(elapsed)} (complete)[/]");
            else
                AnsiConsole.MarkupLine($"[blue]  → {processed} items synced in {FormatTime(elapsed)} ({remainingCount} remaining)[/]");
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
            catch (RateLimitExceededException)
            {
                throw;
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
            catch (RateLimitExceededException) { throw; }
            catch { /* Ignore other PR fetch errors, use basic data */ }
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
