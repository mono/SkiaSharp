namespace SkiaSharp.Dashboard.Services;

public record GitHubStats(
    DateTime GeneratedAt,
    RepositoryInfo Repository,
    IssueStats Issues,
    PullRequestStats PullRequests,
    ActivityStats Activity
);

public record RepositoryInfo(
    int Stars,
    int Forks,
    int Watchers,
    int OpenIssues,
    int ClosedIssues
);

public record IssueStats(
    int Open,
    int Closed,
    int OpenedLast30Days,
    int ClosedLast30Days,
    List<LabelCount> ByLabel
);

public record LabelCount(string Label, int Count);

public record PullRequestStats(
    int Open,
    int Merged,
    int Closed,
    int OpenedLast30Days,
    int MergedLast30Days
);

public record ActivityStats(
    int CommitsLast30Days,
    List<CommitInfo> RecentCommits
);

public record CommitInfo(
    string Sha,
    string Message,
    string Author,
    DateTime Date
);
