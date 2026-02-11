namespace SkiaSharp.Triage.Dashboard.Services;

/// <summary>
/// Trend data for issues and PRs over time
/// </summary>
public record TrendData(
    DateTime GeneratedAt,
    TrendSummary Issues,
    TrendSummary PullRequests,
    List<MonthlyTrend> MonthlyTrends,
    List<RepoSummary>? Repos = null
);

public record TrendSummary(
    int TotalCreated,
    int TotalClosed,
    int CurrentlyOpen,
    double ClosureRate,
    double? AvgDaysToClose,
    int? OldestOpenDays,
    // PR-specific
    int? TotalMerged,
    double? MergeRate,
    double? AvgDaysToMerge
);

public record MonthlyTrend(
    string Month,  // "2025-01"
    int IssuesCreated,
    int IssuesClosed,
    int PrsCreated,
    int PrsClosed,
    int PrsMerged,
    Dictionary<string, int>? IssuesCreatedByRepo = null,
    Dictionary<string, int>? IssuesClosedByRepo = null,
    Dictionary<string, int>? PrsCreatedByRepo = null,
    Dictionary<string, int>? PrsClosedByRepo = null,
    Dictionary<string, int>? PrsMergedByRepo = null
);
