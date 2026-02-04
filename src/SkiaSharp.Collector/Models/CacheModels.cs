namespace SkiaSharp.Collector.Models;

/// <summary>
/// Global metadata for the cache
/// </summary>
public record CacheMeta(
    int Version,
    string Description,
    string Repository,
    DateTime CreatedAt
);

/// <summary>
/// Sync metadata for a specific data source (GitHub, NuGet)
/// </summary>
public record SyncMeta(
    int Version,
    DateTime? LastRun,
    string? LastRunStatus,
    SyncLayers Layers,
    RateLimitInfo? RateLimit,
    Dictionary<string, FailureEntry> Failures
);

public record SyncLayers(
    LayerStatus Items,
    EngagementLayerStatus Engagement
);

public record LayerStatus(
    string? Status,
    DateTime? LastSync,
    int ItemsProcessed,
    int ItemsTotal
);

public record EngagementLayerStatus(
    string? Status,
    DateTime? LastSync,
    int? LastItemProcessed,
    int ItemsProcessed,
    int ItemsTarget,
    int TotalWithEngagement
);

public record RateLimitInfo(
    int? Remaining,
    DateTime? ResetAt
);

public record FailureEntry(
    string Layer,
    DateTime FailedAt,
    int FailCount,
    ErrorInfo LastError,
    DateTime RetryAfter,
    DateTime? RemoveAfter = null,
    PartialData? PartialData = null
);

public record ErrorInfo(
    string Type,
    int? StatusCode,
    string Message
);

public record PartialData(
    bool HasComments,
    bool HasReactions
);

/// <summary>
/// Index file containing lightweight list of all items
/// </summary>
public record CacheIndex(
    DateTime? UpdatedAt,
    List<IndexItem> Items
);

public record IndexItem(
    int Number,
    string Type,  // "issue" or "pr"
    string State, // "open" or "closed"
    string Title,
    string Author,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<string> Labels,
    int CommentCount,
    int ReactionCount,
    DateTime? ItemSyncedAt,
    DateTime? EngagementSyncedAt,
    // PR-specific fields
    bool? Draft,
    bool? Mergeable,
    string? BaseBranch,
    string? HeadBranch,
    // Status tracking
    string? SyncStatus,  // "normal", "missing", "inaccessible", "stale"
    string? SyncNote
);

/// <summary>
/// Full item data stored in items/{number}.json
/// </summary>
public record CachedItem(
    int Number,
    string Type,
    string State,
    string Title,
    string Body,
    AuthorInfo Author,
    List<LabelInfo> Labels,
    List<AuthorInfo> Assignees,
    string? Milestone,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? ClosedAt,
    int CommentCount,
    int ReactionCount,
    // PR-specific fields
    bool? Draft,
    bool? Mergeable,
    string? MergeableState,
    string? BaseBranch,
    string? HeadBranch,
    int? Additions,
    int? Deletions,
    int? ChangedFiles,
    int? Commits,
    string? ReviewDecision,
    List<ReviewInfo>? Reviews,
    // Engagement data (null until synced)
    EngagementData? Engagement
);

public record AuthorInfo(
    string Login,
    string? Type = null,
    string? AvatarUrl = null
);

public record LabelInfo(
    string Name,
    string? Color = null
);

public record ReviewInfo(
    string Author,
    string State,
    DateTime SubmittedAt
);

/// <summary>
/// Engagement data for an item (comments, reactions)
/// </summary>
public record EngagementData(
    DateTime SyncedAt,
    List<ReactionInfo> Reactions,
    List<CommentInfo> Comments
);

public record ReactionInfo(
    string User,
    string Content,
    DateTime CreatedAt
);

public record CommentInfo(
    long Id,
    string Author,
    DateTime CreatedAt,
    List<ReactionInfo> Reactions
);

/// <summary>
/// NuGet-specific models
/// </summary>
public record NuGetSyncMeta(
    int Version,
    DateTime? LastSync,
    int PackagesProcessed
);

public record NuGetIndex(
    DateTime? UpdatedAt,
    List<NuGetIndexPackage> Packages
);

public record NuGetIndexPackage(
    string Id,
    string? LatestVersion,
    long TotalDownloads,
    bool IsLegacy,
    DateTime? SyncedAt
);

public record CachedPackage(
    string Id,
    string? Description,
    string? LatestVersion,
    long TotalDownloads,
    bool IsLegacy,
    List<PackageVersion> Versions,
    DateTime SyncedAt
);

public record PackageVersion(
    string Version,
    long Downloads,
    DateTime? Published
);

/// <summary>
/// Repository-level stats (stars, forks, etc.)
/// </summary>
public record RepoStats(
    int Stars,
    int Forks,
    int Watchers,
    string? Description,
    string? License,
    List<string> Topics,
    DateTime? CreatedAt,
    DateTime? PushedAt,
    DateTime SyncedAt
);

/// <summary>
/// Community sync metadata
/// </summary>
public record CommunitySyncMeta(
    int Version,
    DateTime? LastSync,
    int ContributorsProcessed,
    int MembershipChecked
);

/// <summary>
/// Cached contributor info
/// </summary>
public record CachedContributor(
    string Login,
    string AvatarUrl,
    int Contributions,
    bool? IsMicrosoft,
    DateTime? MembershipCheckedAt
);
