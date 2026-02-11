using SkiaSharp.Triage.Models;

namespace SkiaSharp.Triage.Dashboard.Services;

// Repo summary for filter dropdown
public record RepoSummary(
    string FullName,
    string Slug,
    string DisplayName,
    string Color
);

public record IssuesData(
    DateTime GeneratedAt,
    int TotalCount,
    List<LabelCount> ByType,
    List<LabelCount> ByArea,
    List<LabelCount> ByBackend,
    List<LabelCount> ByOs,
    List<AgeCount> ByAge,
    List<IssueInfo> Issues,
    List<IssueInfo>? HotIssues = null,
    List<RepoSummary>? Repos = null,
    Dictionary<string, int>? ByRepo = null
);

public record AgeCount(
    string Label,
    string Display,
    int Count
);

public record IssueInfo(
    int Number,
    string Title,
    string Author,
    string AuthorAvatarUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int CommentCount,
    double DaysOpen,
    double DaysSinceActivity,
    string AgeCategory,
    string? Type,
    List<string> Areas,
    List<string> Backends,
    List<string> Oses,
    List<string> OtherLabels,
    string Url,
    EngagementScore? Engagement = null,
    string? Repo = null,
    string? RepoSlug = null,
    string? RepoColor = null
);

public record EngagementScore(
    double CurrentScore,
    double PreviousScore,
    bool IsHot
);
