namespace SkiaSharp.Dashboard.Services;

public record PrTriageStats(
    DateTime GeneratedAt,
    TriageSummary Summary,
    List<TriagedPullRequest> PullRequests
);

public record TriageSummary(
    int ReadyToMerge,
    int NeedsReview,
    int ConsiderClosing
);

public record TriagedPullRequest(
    int Number,
    string Title,
    string Author,
    string AuthorAvatarUrl,
    DateTime CreatedAt,
    int FilesChanged,
    int Additions,
    int Deletions,
    TriageCategory Category,
    string AiReasoning,
    List<string> Labels
);

public enum TriageCategory
{
    ReadyToMerge,
    NeedsReview,
    ConsiderClosing
}
