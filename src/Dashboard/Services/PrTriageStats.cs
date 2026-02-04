namespace SkiaSharp.Dashboard.Services;

public record PrTriageStats(
    DateTime GeneratedAt,
    int TotalCount,
    TriageSummary Summary,
    List<AgeCount> BySize,
    List<AgeCount> ByAge,
    List<TriagedPullRequest> PullRequests
);

public record TriageSummary(
    int ReadyToMerge,
    int QuickReview,
    int NeedsReview,
    int NeedsAuthor,
    int ConsiderClosing
);

public record TriagedPullRequest(
    int Number,
    string Title,
    string Author,
    string AuthorAvatarUrl,
    string AuthorType,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    double DaysOpen,
    string AgeCategory,
    int FilesChanged,
    int Additions,
    int Deletions,
    int TotalChanges,
    string SizeCategory,
    bool IsDraft,
    TriageCategory Category,
    string Reasoning,
    string? Type,
    List<string> Areas,
    List<string> Backends,
    List<string> Oses,
    List<string> OtherLabels,
    string Url
);

public enum TriageCategory
{
    ReadyToMerge,
    QuickReview,
    NeedsReview,
    NeedsAuthor,
    ConsiderClosing,
    Untriaged,
    Draft
}
