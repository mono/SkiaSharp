namespace SkiaSharp.Collector.Models;

// PR Triage Data
public record PrTriageData(
    DateTime GeneratedAt,
    int TotalCount,
    TriageSummary Summary,
    List<SizeCount> BySize,
    List<AgeCount> ByAge,
    List<PullRequestInfo> PullRequests
);

public record TriageSummary(
    int ReadyToMerge,
    int QuickReview,
    int NeedsReview,
    int NeedsAuthor,
    int ConsiderClosing
);

public record SizeCount(string Label, string Display, int Count);

public record PullRequestInfo(
    int Number,
    string Title,
    string Author,
    string AuthorAvatarUrl,
    string AuthorType,  // "microsoft", "community", "bot"
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
    string Category,     // Triage category
    string Reasoning,
    string? Type,
    List<string> Areas,
    List<string> Backends,
    List<string> Oses,
    List<string> OtherLabels,
    string Url
);
